using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Xml;
using PCI.Service.Utilities;

namespace PCI.Service
{
    static class PnSJobMonitor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PnSJobMonitor));
        private static Database db = DatabaseFactory.CreateDatabase();

        public static void GetNewPnSBatch()
        {
            SFTChannel sftChannel = DAL.GetSFTChannelDetailsByCode(Common.PnS_SFTChannelCode);
            SFTUtil sftSite = new SFTUtil(sftChannel.Protocol.ParseEnum<SFT_ProtocolType>(), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000);
            sftSite.Connect();

            // If at least one batch found from any of the client then re-check for all clients; if for all clients no batch found then end.
            bool foundNewBatch = false;

            while (true)
            {
                //Get all client
                List<Client> clients = DAL.GetAllClientAppDetail().Where(c => c.IsPnSActive).ToList();

                foreach (var client in clients)
                {
                    bool isOneNewBatchFound = CheckForNewPnSBatch(client, sftSite, sftChannel);
                    if (isOneNewBatchFound)
                    {
                        // at least one batch found.
                        foundNewBatch = true;
                    }
                }

                // As checked for all clients break now; remove this during deployment.
                //break;
                if (foundNewBatch)
                {
                    foundNewBatch = false;
                    continue;
                }
                else
                {
                    break;
                }
            }

            sftSite.Disconnect();
        }

        #region Sub-Methods for downloading new PnS batch

        private static bool CheckForNewPnSBatch(Client clientInfo, SFTUtil sftSite, SFTChannel sftChannel)
        {
            List<string> availableSFTFiles;
            List<string> availableBatches;

            //log.Debug(Common.ServerIP);
            log.Info($"Server Ip : {Common.ServerIP}");

            var remoteDirPath = Path.Combine(sftChannel.ToADEDir, Common.ServerIP, clientInfo.AppCode);

            availableSFTFiles = sftSite.ListDirectory(remoteDirPath);

            availableBatches = availableSFTFiles.Where(x => x.ToLower().EndsWith(".go")).ToList();

            if (availableBatches.Count > 0)
            {
                log.InfoFormat("Found {0} new batches from client : {1}", availableBatches.Count, clientInfo.AppCode);

                #region Process all found batches.

                foreach (string goFile in availableBatches)
                {
                    log.Debug("Downloading Started For Batch {0}".Fill(Path.GetFileNameWithoutExtension(goFile)));
                    string clientBatchName = Path.GetFileNameWithoutExtension(goFile);
                    string appDirRemotePath = remoteDirPath;
                    string batchPDFsDirRemotePath = Path.Combine(appDirRemotePath, clientBatchName);
                    string requestXMLRemotePath = Path.Combine(appDirRemotePath, $"{clientBatchName}.xml");

                    if (!availableSFTFiles.Contains($"{clientBatchName}.xml") || !availableSFTFiles.Contains(clientBatchName))
                    {
                        // If trigger file + XML + PDFs folder is available then only it will be considered a complete batch to be processed.
                        continue;
                    }
                    else
                    {
                        // TODO: Create batch summary record
                        BatchSummary batchSummary = new BatchSummary { ClientId = clientInfo.Id, JobName = $"{clientInfo.AppCode}-{clientBatchName}" };
                        string sql = "INSERT INTO [PnS].[BatchSummary] (ClientId,JobName,ServerIP) OUTPUT INSERTED.PrintBatch VALUES (@p1, @p2, @p3)";
                        batchSummary.PrintBatch = db.ExecuteSqlScalar<int>(sql, batchSummary.ClientId, batchSummary.JobName, Common.ServerIP);

                        // Mark the batch as received.
                        DAL.UpdateBatchPipeLineStatus(batchSummary.PrintBatch, BatchPipelineStatus.Received, sftSite.LastModifiedTimeOfFile(Path.Combine(appDirRemotePath, goFile)));

                        BatchInfo bi = new BatchInfo(batchSummary, clientInfo);

                        DownloadPDFsAndCreateNewBatch(bi, appDirRemotePath, requestXMLRemotePath, sftSite, sftChannel);
                    }
                }

                #endregion

                return true;
            }

            return false;
        }

        private static void DownloadPDFsAndCreateNewBatch(BatchInfo bi, string appDirRemotePath, string requestXMLRemotePath, SFTUtil sftSite, SFTChannel sftChannel)
        {
            string batchPDFsDirRemotePath = Path.Combine(appDirRemotePath, bi.BatchName);

            try
            {
                DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.DownloadStart, Common.GetCurrentTime());

                #region Create directory structure.

                Common.CleanDirectory(bi.BatchRepo);
                bi.PnSRequestPath.CreateDirectoryIfNotExist();
                bi.PCIRequestPath.CreateDirectoryIfNotExist();
                bi.PnSResponsePath.CreateDirectoryIfNotExist();
                bi.PCIResponsePath.CreateDirectoryIfNotExist();

                #endregion

                sftSite.DownloadFile(requestXMLRemotePath, bi.RequestXmlPath);//, Common.RemoveBatchFiles_PnS_SFT, false);

                sftSite.DownloadDirectory(bi.PnSRequestPDFsPath, batchPDFsDirRemotePath);//, Common.RemoveBatchFiles_PnS_SFT, false);

                #region Remove batch from PnS SFT site and update status.

                if (sftChannel.PostDownloadAction == "Delete")
                {
                    sftSite.DeleteFile(Path.Combine(appDirRemotePath, $"{bi.BatchName}.xml"));
                    sftSite.DeleteFile(Path.Combine(appDirRemotePath, $"{bi.BatchName}.go"));
                    sftSite.DeleteDirectory(batchPDFsDirRemotePath);
                }
                else
                {
                    string goFilePath = Path.Combine(appDirRemotePath, $"{bi.BatchName}.go");
                    string archiveGOFilePath = Path.Combine(appDirRemotePath, "triggers", $"{bi.BatchName}.go");

                    if (sftSite.IsFileAvailable(archiveGOFilePath))
                        sftSite.DeleteFile(archiveGOFilePath);

                    sftSite.MoveFile(goFilePath, archiveGOFilePath);
                }

                DAL.UpdateBatchPipeLineStatus(bi.BatchSummary.PrintBatch, BatchPipelineStatus.Downloaded, Common.GetCurrentTime());

                #endregion

            }
            catch (Exception ex)
            {
                var msg = "Error occurred during downloading new PnS batch: {0} files. With below Error <br/> {1}".Fill(bi.BatchName, ex);
                // During downloading PDFs for this batch if any error occurs; remove the batch entry from DB and remove folder structure also.
                log.Error(msg);
                DAL.DeleteInCompleteBatchById(bi.BatchSummary.PrintBatch);
                Common.CleanDirectory(bi.BatchRepo);

                MailUtility.SendUnexpectedNotificationEmail(msg);
            }
            finally
            {
                DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.DownloadEnd, Common.GetCurrentTime());
            }

        }

        #endregion

        public static void SubmitImmediateErrorReport()
        {
            Dictionary<string, string> errFilesWithAppCode = Directory.GetFiles(Common.PCIRootFolder, "*.err.*", SearchOption.TopDirectoryOnly).Where(f => Path.GetExtension(Path.GetFileNameWithoutExtension(f)) == ".err").ToDictionary(f => f, f => Path.GetExtension(f));

            if (errFilesWithAppCode.Count > 0)
            {
                log.DebugFormat("{0} immediate error report(s) found to upload.", errFilesWithAppCode.Count);

                SFTChannel sftChannel = DAL.GetSFTChannelDetailsByCode(Common.PnS_SFTChannelCode);
                SFTUtil sftSite = new SFTUtil(sftChannel.Protocol.ParseEnum<SFT_ProtocolType>(), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000);

                // Connect                    
                sftSite.Connect();

                foreach (var errFile in errFilesWithAppCode)
                {
                    UploadImmediateErrReport(errFile.Value.TrimStart('.'), errFile.Key, sftSite, sftChannel);
                }

                sftSite.Disconnect();
            }
        }

        #region Sub-Methods for uploading immediate error report.

        private static void UploadImmediateErrReport(string appCode, string ieReportTriggerFile, SFTUtil sftSite, SFTChannel sftChannel)
        {
            string batchName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(ieReportTriggerFile));
            string immediateErrorReportPath = Path.Combine(Common.PCIRootFolder, "{0}-res.xml".Fill(batchName));

            string sftTriggerFilePath = Path.Combine(Common.PCIRootFolder, Path.GetFileNameWithoutExtension(ieReportTriggerFile));
            File.AppendAllText(sftTriggerFilePath, string.Empty);

            if (File.Exists(immediateErrorReportPath))
            {
                string ieReportRemoteDirPath = Path.Combine(sftChannel.ToADEDir, Common.ServerIP, appCode, Path.GetFileName(immediateErrorReportPath));
                UploadFileOnSFTSite(immediateErrorReportPath, ieReportRemoteDirPath, Common.Remove_ImmediateErrFiles, sftSite);
                UploadFileOnSFTSite(sftTriggerFilePath, ieReportRemoteDirPath, Common.Remove_ImmediateErrFiles, sftSite);

                // Immediate error report uploaded successfully on PnS SFT site; so remove local files of immediate error report.
                File.Delete(ieReportTriggerFile);
                File.Delete(immediateErrorReportPath);
                File.Delete(sftTriggerFilePath);
            }
            else
            {
                log.ErrorFormat("Immediate Error Report file not found for : {0}", ieReportTriggerFile);
            }
        }

        private static void UploadFileOnSFTSite(string localFilePath, string remoteDirPath, bool removeLocalFileAfterUpload, SFTUtil sftSite)
        {
            var sw = new Stopwatch();
            sw.Start();

            sftSite.UploadFile(localFilePath, Path.Combine(remoteDirPath, Path.GetFileName(localFilePath)), removeLocalFileAfterUpload, false);

            sw.Stop();
            var time = sw.Elapsed;
            log.DebugFormat("Total time spent : {0}", time);

        }

        #endregion

        public static void SubmitFinalXMLReport()
        {
            List<int> batchIds = DAL.GetNextBatchIdByJobName(JobTypes.PnSJobMonitor, OperationTypes.ReportBack, Common.ServerIP);

            if (batchIds == null || batchIds.Count == 0)
            {
                log.Debug("No batch available for PnS job monitor to report back.");
                return;
            }

            SFTChannel sftChannel = DAL.GetSFTChannelDetailsByCode(Common.PnS_SFTChannelCode);
            SFTUtil sftSite = new SFTUtil(sftChannel.Protocol.ParseEnum<SFT_ProtocolType>(), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000);

            sftSite.Connect();

            foreach (var batchId in batchIds)
            {
                try
                {
                    BatchSummary bs = DAL.GetBatchSummaryById(batchId);
                    Client app = DAL.GetClientAppDetailsById(bs.ClientId);
                    BatchInfo bi = new BatchInfo(bs, app);

                    log.Debug("Final Report Uploading Started For Batch {0}".Fill(bi.BatchName));

                    #region Upload final XML report to PnS SFT site back.

                    if (sftSite != null && !sftSite.IsConnected)
                    {
                        sftSite.Connect();
                    }

                    string batchRemoteDirPath = Path.Combine(sftChannel.ToADEDir, Common.ServerIP, bi.Client.AppCode);
                    string xmlReportPath = Path.Combine(bi.PnSResponsePath, $"{bi.BatchName}-res.xml");
                    string finalTriggerPath = Path.Combine(bi.PnSResponsePath, $"{bi.BatchName}.rpt");

                    // In case of immediate error report; there will be .err as trigger file will be available.
                    // Like XML is invalid then immediate error report itself if final report.
                    if (!File.Exists(finalTriggerPath))
                    {
                        string errTriggerPath = Path.Combine(bi.PnSResponsePath, $"{bi.BatchName}.err");
                        if (File.Exists(errTriggerPath))
                        {
                            finalTriggerPath = errTriggerPath;
                        }
                    }

                    DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.ReportBackStart, Common.GetCurrentTime());

                    UploadFileOnSFTSite(xmlReportPath, batchRemoteDirPath, false, sftSite);
                    UploadFileOnSFTSite(finalTriggerPath, batchRemoteDirPath, false, sftSite);

                    // Final report uploaded successfully on PnS SFT site; so update batch status.
                    DAL.UpdateBatchPipeLineStatus(batchId, BatchPipelineStatus.ReportedBack, Common.GetCurrentTime());
                    log.InfoFormat("Batch {0} reported back successfully.", batchId);

                    DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.ReportBackEnd, Common.GetCurrentTime());
                    #endregion
                }
                catch (Exception ex)
                {
                    var errorMsg = "Error occured while reporting back pns response for Batch-Id {0} with Error - {1}".Fill(batchId, ex);
                    log.Error(errorMsg);
                    MailUtility.SendUnexpectedNotificationEmail(errorMsg);
                    continue;
                }
            }
        }
    }
}
