using AM.Entities;
using AM.Services;
using log4net;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using AM.Lib;
using System.IO.Compression;

namespace AM.Console
{
    public class DownloadJob
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IClientService clientSvc;
        IMailBatchService mailBatchSvc;
        ICommonService commonSvc;
        IMailPieceService mailPieceSvc;
        public DownloadJob()
        {
            clientSvc = new ClientService();
            mailBatchSvc = new MailBatchService();
            commonSvc = new CommonService();
            mailPieceSvc = new MailPieceService();
        }

        #region Download New Batch

        public void DownloadNewBatch()
        {
            log.Debug("Download Batch Process Started");

            // 

            Client client = clientSvc.GetClientByAppCode("USAA");

            if (client != null)
            {
                List<ClientSFTChannel> sftChannels = clientSvc.GetClientSFTChannelMapDetails(client.Id);

                if (sftChannels != null && sftChannels.Count > 0)
                {
                    foreach (var sftChannel in sftChannels)
                    {
                        List<string> triggerFilePaths = new List<string>();

                        SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(sftChannel.SFTChannelId);

                        if (sftChannelDetails != null)
                        {
                            SFTUtil sftSite = null;
                            try
                            {
                                sftSite = new SFTUtil(cfg.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                                sftSite.Connect();
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }

                            if (sftSite.IsDownloadDirectoryAvailable(sftChannelDetails.ToADEDir))
                            {
                                triggerFilePaths = GetTriggerFilePaths(sftSite, sftChannelDetails);
                                if (triggerFilePaths != null)
                                {
                                    foreach (string triggerFile in triggerFilePaths)
                                    {
                                        MailBatchDTO mailBatch = null;

                                        string zipFileFTPPath = Path.Combine(sftChannelDetails.ToADEDir, Regex.Replace(Path.ChangeExtension(triggerFile, ".zip"), @"(w*)_Complete", string.Empty));

                                        string triggerFilePath = Path.Combine(sftChannelDetails.ToADEDir, triggerFile);

                                        //Download and Insert the batch in database first always.
                                        string batchName = Path.GetFileNameWithoutExtension(zipFileFTPPath);

                                        string appDirRemotePath = "{0}/{1}".Fill(Config.AuthoMailRootFolder, client.AppCode);
                                        string batchPDFsDirRemotePath = "{0}/{1}".Fill(appDirRemotePath, batchName);

                                        mailBatch = new MailBatchDTO();
                                        mailBatch.ClientId = client.Id;
                                        mailBatch.BatchName = batchName;
                                        mailBatch.SFTChannelId = sftChannelDetails.Id;
                                        mailBatch.ClientBatchId = batchName;
                                        mailBatch.UniqueId = Guid.NewGuid();
                                        mailBatch.Priority = Priority.Medium.ToString();
                                        mailBatch.PrintLocation = "Fort Mill SC";

                                        //Insert New Batch Entry
                                        mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);

                                        //Update Pipeline Status of Batch Received
                                        commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, sftSite.LastModifiedTimeOfFile(triggerFilePath));
                                        commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.InProgress.ToString());

                                        //Now Download Zip File Path
                                        MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.AuthoMailRootFolder);
                                        DownloadBatchFromFTP(sftSite, zipFileFTPPath, triggerFilePath, mbi, sftChannelDetails);
                                    }
                                }
                                else
                                {
                                    log.Debug("Batches are not available to download.");
                                }

                                if (sftSite.IsConnected)
                                    sftSite.Disconnect();
                            }
                        }
                        else
                        {
                            log.Debug("SFT channel is not available or not enabled for this client.");
                        }
                    }
                }
                else
                {
                    log.Debug("SFT channel mapping with client does not exist.");
                }
            }
            else
            {
                log.Debug("USAA client is not enabled for process.");
            }
        }

        private void DownloadBatchFromFTP(SFTUtil sftSite, string zipFileFTPPath, string triggerFilePath, MailBatchInfo mbi, SFTChannel sftChannel)
        {
            #region Create directory structure.

            cfg.CleanDirectory(mbi.BatchRepo);
            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.PnSRequestPath.CreateDirectoryIfNotExist();
            mbi.PnSResponsePath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();

            #endregion

            string containsZipFilePath = string.Empty;

            if (sftSite.IsConnected == false)
                sftSite.Connect();

            // If batch zip file exists on FTP then download and process it.
            if (sftSite.IsFileAvailable(zipFileFTPPath))
            {
                containsZipFilePath = DownloadBatchZipFile(sftSite, zipFileFTPPath, mbi, sftChannel);

                if (!containsZipFilePath.Contains("Error Occured"))
                {
                    #region Try to extract zip file and delete trigger file from FTP.

                    ZipFile.ExtractToDirectory(containsZipFilePath, mbi.AMRequestPath);

                    #endregion
                    //After successfully downloaded and extracted update downloaded pipeline status
                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now);

                    //Process trigger file as per PostDownloadAction Setup for particular SFTChannel
                    //MoveTriggerFileToBackupDir(sftSite, triggerFilePath, mbi.SFTChannel);
                }
                else
                {
                    ReportInvalidBatch(mbi.MailBatch, containsZipFilePath);
                }
            }
            else
            {
                ReportInvalidBatch(mbi.MailBatch, "Zip file is missing on Sft site.");
            }
        }

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string error)
        {
            // If the zip file is not available in the FTP folder then log that error.
            mailBatch.ValidationStatus = BatchValidationStatus.Invalid;
            mailBatch.ValidationErrors = error;
            mailBatch.PrintLocation = string.Empty;
            //Update Batch Details
            mailBatchSvc.UpdateBatchDetails(mailBatch);

            //TODO: Ask to Ravi Sir for Batch level notification And how to manage status for this

            //Update Batch Status
            commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.Failed.ToString());
        }

        private static List<string> GetTriggerFilePaths(SFTUtil sftp, SFTChannel sftChannelDetails)
        {
            if (sftp.IsConnected == false)
                sftp.Connect();

            DateTime pingTime = Common.Now;

            List<string> availableTriggerFilePaths = sftp.ListDirectory(sftChannelDetails.ToADEDir).Where(f => Path.GetExtension(f).Equals(".go", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(f).EndsWith("_Complete")).ToList();

            if (availableTriggerFilePaths != null && availableTriggerFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} Trigger file found.", availableTriggerFilePaths.Count));

                return availableTriggerFilePaths;
            }
            else
            {
                log.Error("Trigger file not found.");
            }

            return null;
        }

        private void MoveTriggerFileToBackupDir(SFTUtil sftSite, string triggerFTPFilePath, SFTChannel channelDetails)
        {
            if (!sftSite.IsConnected)
                sftSite.Connect();

            string ftpBackupDir = channelDetails.ArchiveDir;

            string triggerFTPBackUpFilePath = Path.Combine(ftpBackupDir, Path.GetFileName(triggerFTPFilePath));

            if (channelDetails.PostDownloadAction == PostDownloadAction.Archive)
            {
                if (sftSite.IsFileAvailable(triggerFTPBackUpFilePath))
                    sftSite.DeleteFile(triggerFTPBackUpFilePath);

                sftSite.MoveFile(triggerFTPFilePath, triggerFTPBackUpFilePath);
            }
            else
                sftSite.DeleteFile(triggerFTPFilePath);
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string zipFileFTPPath, MailBatchInfo mbi, SFTChannel sftChannel)
        {
            commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.DownloadStart, Common.Now);

            if (sftSite.IsConnected == false)
                sftSite.Connect();

            try
            {
                string downloadedZipFilePath = string.Empty;
                string zipFileName = Path.GetFileName(zipFileFTPPath);

                downloadedZipFilePath = Path.Combine(mbi.AMRequestPath, zipFileName);

                if (File.Exists(downloadedZipFilePath))
                    File.Delete(downloadedZipFilePath);

                //Download Zip File Path
                sftSite.DownloadFile(zipFileFTPPath, downloadedZipFilePath);

                //Update LogTime for Download complete
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.DownloadEnd, Common.Now);

                if (sftChannel.PostDownloadAction == PostDownloadAction.Archive)
                {
                    string archiveFTPFilePath = Path.Combine(sftChannel.ArchiveDir, zipFileName);

                    if (sftSite.IsFileAvailable(archiveFTPFilePath))
                        sftSite.DeleteFile(archiveFTPFilePath);

                    sftSite.MoveFile(zipFileFTPPath, archiveFTPFilePath);
                }
                else
                {
                    sftSite.DeleteFile(zipFileFTPPath);
                }

                return downloadedZipFilePath;
            }
            catch (Exception ex)
            {
                //Update LogTime for Download complete
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.DownloadEnd, Common.Now);
                return "Error occured while downloading Zip file from server :" + Environment.NewLine + ex.Message + Environment.NewLine + ex.TargetSite.ToString();
            }
        }

        #endregion

        #region Separate Batches
        public void SeparateUSAABatches()
        {
            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Downloaded);
            if (downloadBatches == null)
            {
                log.Debug("Separating job queue is empty.");
                return;
            }

            foreach (var batch in downloadBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.AuthoMailRootFolder);

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

                if (!File.Exists(xmlFilePath))
                {
                    ReportInvalidBatch(batch, "Xml file does not exist in Batch-{0}".Fill(batch.BatchName));

                    continue;
                }

                XmlDocument batchXML = new XmlDocument();
                XmlNamespaceManager nsManager = null;
                XmlNodeList mailPieceNodes = null;
                // Try to parse the XML we got from the zip file. If XML is invalid then log error.
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationStart, Common.Now);
                try
                {
                    batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                    nsManager = new XmlNamespaceManager(batchXML.NameTable);
                    nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
                    mailPieceNodes = batchXML.DocumentElement.ChildNodes;
                }
                catch (XmlException err)
                {
                    log.Error(err);

                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);

                    ReportInvalidBatch(batch, err.ToString());

                    continue;
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);

                    ReportInvalidBatch(batch, ex.ToString());

                    continue;
                }

                if (mailPieceNodes == null)
                {
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);

                    ReportInvalidBatch(batch, "Xml does not contain any package info nodes.");

                    continue;
                }

                try
                {
                    PackageRequests requests = null;
                    PackageRequests amcRequest = new PackageRequests();

                    requests = XmlUtil.Deserialize<PackageRequests>(batchXML.OuterXml);

                    List<PackageInfo> packages = requests.Packages.FindAll(p => p.CustomerIdentifier != cfg.USAACustIdentifier);

                    if (packages != null && packages.Count > 0)
                    {
                        amcRequest.Packages = packages;
                        SplitXMLAndPDFS(amcRequest, Path.GetDirectoryName(xmlFilePath), mbi.BatchName);
                        requests.Packages.RemoveAll(p => p.CustomerIdentifier != cfg.USAACustIdentifier);
                        //After removing other document types from package request we are checking for any remaining USAA packages are there or not if not then we don't need to process that batch further so we will remove that batch's records and files from database and from folder
                        if (requests.Packages.Count == 0)
                        {
                            //TODO: Ask to Ravi Sir
                            DeleteBatchInfo(mbi.MailBatch.Id, mbi.BatchRepo);
                            continue;
                        }

                        if (File.Exists(xmlFilePath))
                            File.Delete(xmlFilePath);

                        File.WriteAllText(xmlFilePath, XmlUtil.Serialize<PackageRequests>(requests));
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }

        private void DeleteBatchInfo(int mailBatchId, string mailBatchRepo)
        {
            int retVal = 0;
            try
            {
                retVal = mailBatchSvc.DeleteBatchDetails(mailBatchId);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            if (retVal == 1)
            {
                Directory.Delete(mailBatchRepo, true);
            }
        }

        private void SplitXMLAndPDFS(PackageRequests request, string PDFsDir, string batchName)
        {
            //Create folder for Separate batch for AMC in UNC path
            string batchDirectoryPath = Path.Combine(cfg.AMCDirPath, batchName);

            batchDirectoryPath.CleanDirectory();

            //Xml and pdf files will be copied to batchname folder.
            string amcFilePath = Path.Combine(batchDirectoryPath, string.Format("{0}.xml", batchName));

            string zipFilePath = Path.Combine(cfg.AMCDirPath, string.Format("{0}.zip", batchName));
            string triggerFilePath = Path.Combine(cfg.AMCDirPath, string.Format("{0}_Complete.go", batchName));

            List<PackageInfo> packages = request.Packages;
            if (packages != null)
            {
                foreach (PackageInfo package in packages)
                {
                    if (File.Exists(Path.Combine(PDFsDir, package.File)))
                    {
                        string sourceFile = Path.Combine(PDFsDir, package.File);
                        string destFile = Path.Combine(batchDirectoryPath, package.File);

                        if (File.Exists(destFile))
                            File.Delete(destFile);

                        File.Copy(sourceFile, destFile);
                    }
                }

                if (File.Exists(amcFilePath))
                    File.Delete(amcFilePath);

                File.WriteAllText(amcFilePath, XmlUtil.Serialize<PackageRequests>(request));

                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);

                using (ZipArchive newFile = System.IO.Compression.ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (string file in Directory.GetFiles(batchDirectoryPath).Where(fileType => !fileType.ToLower().EndsWith(".zip")))
                    {
                        newFile.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }

                //After creating zip remove batch folder
                Directory.Delete(batchDirectoryPath, true);


                if (File.Exists(triggerFilePath))
                    File.Delete(triggerFilePath);

                File.Create(triggerFilePath);
            }
        }
        #endregion

        #region Validate And Acknowledge

        public void ValidateBatches()
        {
            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Downloaded).Where(x => x.BatchStatus != BatchStatus.Failed.ToString()).ToList();
            if (downloadBatches == null)
            {
                log.Debug("Job queue is empty for validating batches.");
                return;
            }

            foreach (var batch in downloadBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.AuthoMailRootFolder);

                ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.GetBusinessIdNameFromClientId(client.Id);

                if (clientBusinessIdDetails == null)
                {
                    log.Error("Business identifier is not assigned to client : {0}".Fill(client.Name));

                    //TODO: Ask to Ravi Sir
                    continue;
                }

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

                XmlDocument batchXML = new XmlDocument();
                XmlNamespaceManager nsManager = null;
                XmlNodeList mailPieceNodes = null;
                // Try to parse the XML we got from the zip file. If XML is invalid then log error.
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationStart, Common.Now);
                try
                {
                    batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                    nsManager = new XmlNamespaceManager(batchXML.NameTable);
                    nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
                    mailPieceNodes = batchXML.DocumentElement.ChildNodes;
                }
                catch (XmlException err)
                {
                    log.Error(err);

                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);

                    ReportInvalidBatch(batch, err.ToString());

                    continue;
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);

                    ReportInvalidBatch(batch, ex.ToString());

                    continue;
                }

                if (mailPieceNodes == null || mailPieceNodes.Count == 0)
                {
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);

                    ReportInvalidBatch(batch, "Xml does not contain any package info nodes.");

                    continue;
                }

                foreach (XmlNode mailPieceNode in mailPieceNodes)
                {
                    MailPieceDTO mailPieceDTO = new MailPieceDTO();
                    MailPieceCustomInfo customInfo = null;
                    List<MailPieceCustomInfo> mailPieceCustomInfo = new List<MailPieceCustomInfo>();
                    mailPieceDTO.ClientId = client.Id;

                    mailPieceDTO.MailBatchId = mbi.MailBatch.Id;

                    try
                    {

                        mailPieceDTO.BusinessFriendlyIdName = clientBusinessIdDetails.BusinessIdName;
                        mailPieceDTO.BusinessFriendlyIdValue = mailPieceNode.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                        mailPieceDTO.DocumentOrPackageType = mailPieceNode.SelectSingleNode("./x:PackageName/text()", nsManager) == null ? null : mailPieceNode.SelectSingleNode("./x:PackageName/text()", nsManager).Value;
                        mailPieceDTO.UniqueId = mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager) == null ? Guid.Empty : Guid.Parse(mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager).Value);
                        mailPieceDTO.FileName = mailPieceNode.SelectSingleNode("./x:File/text()", nsManager) == null ? null : mailPieceNode.SelectSingleNode("./x:File/text()", nsManager).Value;



                        var recipientElement = mailPieceNode.SelectSingleNode("./x:RecipientInfo", nsManager);
                        if (recipientElement != null)
                        {
                            if (recipientElement.SelectNodes("./x:Recipient", nsManager) != null)
                            {
                                mailPieceDTO.IsRcptInfoSpecified = true;
                                var recipientsData = recipientElement.SelectNodes("./x:Recipient", nsManager);

                                if (recipientsData.Count > 1)
                                {
                                    mailPieceDTO.RcptFirstName = recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager) == null ? null : recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager).Value;
                                    mailPieceDTO.RcptLastName = recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager) == null ? null : recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager).Value;

                                    mailPieceDTO.CoRcptFirstName = recipientsData.Item(1).SelectSingleNode("./x:FirstName/text()", nsManager) == null ? null : recipientsData.Item(1).SelectSingleNode("./x:FirstName/text()", nsManager).Value;
                                    mailPieceDTO.CoRcptLastName = recipientsData.Item(1).SelectSingleNode("./x:LastName/text()", nsManager) == null ? null : recipientsData.Item(1).SelectSingleNode("./x:LastName/text()", nsManager).Value;
                                }
                                else
                                {
                                    mailPieceDTO.RcptFirstName = recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager) == null ? null : recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager).Value;
                                    mailPieceDTO.RcptLastName = recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager) == null ? null : recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager).Value;
                                }
                            }
                            else
                                mailPieceDTO.IsRcptInfoSpecified = false;

                            mailPieceDTO.RcptPhone = recipientElement.SelectSingleNode("./x:RecipientPhoneNo/text()", nsManager) == null ? null : recipientElement.SelectSingleNode("./x:RecipientPhoneNo/text()", nsManager).Value;

                            var recipeintAddressElement = recipientElement.SelectSingleNode("./x:Address", nsManager);

                            if (recipeintAddressElement != null)
                            {
                                mailPieceDTO.RcptAddressStreet = recipeintAddressElement.SelectSingleNode("./x:Line1/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Line1/text()", nsManager).Value;

                                mailPieceDTO.RcptAddressAptOrSuiteOrOther = recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager).Value;

                                mailPieceDTO.RcptAddressCity = recipeintAddressElement.SelectSingleNode("./x:City/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:City/text()", nsManager).Value;

                                mailPieceDTO.RcptAddressState = recipeintAddressElement.SelectSingleNode("./x:State/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:State/text()", nsManager).Value;

                                mailPieceDTO.RcptAddressZIP = recipeintAddressElement.SelectSingleNode("./x:Zip/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Zip/text()", nsManager).Value;
                            }
                        }
                        else
                        {
                            mailPieceDTO.IsRcptInfoSpecified = false;
                        }

                        var returnInfoElement = mailPieceNode.SelectSingleNode("./x:ReturnInfo", nsManager);
                        if (returnInfoElement != null)
                        {
                            mailPieceDTO.IsReturnInfoSpecified = true;
                            mailPieceDTO.RetRcptFirstName = returnInfoElement.SelectSingleNode("./x:FirstName/text()", nsManager) == null ? null : returnInfoElement.SelectSingleNode("./x:FirstName/text()", nsManager).Value;

                            mailPieceDTO.RetRcptLastName = returnInfoElement.SelectSingleNode("./x:LastName/text()", nsManager) == null ? null : returnInfoElement.SelectSingleNode("./x:LastName/text()", nsManager).Value;


                            mailPieceDTO.RetRcptPhone = returnInfoElement.SelectSingleNode("./x:ReturnPhoneNo/text()", nsManager) == null ? null : returnInfoElement.SelectSingleNode("./x:ReturnPhoneNo/text()", nsManager).Value;

                            var returnAddressElement = returnInfoElement.SelectSingleNode("./x:Address", nsManager);
                            if (returnAddressElement != null)
                            {
                                mailPieceDTO.RetRcptAddressStreet = returnAddressElement.SelectSingleNode("./x:Line1/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:Line1/text()", nsManager).Value;

                                mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = returnAddressElement.SelectSingleNode("./x:Line2/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:Line2/text()", nsManager).Value;

                                mailPieceDTO.RetRcptAddressCity = returnAddressElement.SelectSingleNode("./x:City/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:City/text()", nsManager).Value;

                                mailPieceDTO.RetRcptAddressState = returnAddressElement.SelectSingleNode("./x:State/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:State/text()", nsManager).Value;

                                mailPieceDTO.RetRcptAddressZIP = returnAddressElement.SelectSingleNode("./x:Zip/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:Zip/text()", nsManager).Value;
                            }

                            if (mailPieceNode.SelectSingleNode("./x:ShippingMethod", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingMethod", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:ShippingMethod/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:CustomerIdentifier", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "CustomerIdentifier", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:CustomerIdentifier/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:RequestId", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "RequestId", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:RequestId/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:ReceivedDate", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ReceivedDate", DataType = DataType.DateTime, Value = mailPieceNode.SelectSingleNode("./x:ReceivedDate/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            var shippingCredElement = mailPieceNode.SelectSingleNode("./x:ShippingCredentials", nsManager);
                            if (shippingCredElement != null)
                            {
                                if (shippingCredElement.SelectSingleNode("./x:ClientCode", nsManager) != null)
                                {
                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingCredCode", DataType = DataType.String, Value = shippingCredElement.SelectSingleNode("./x:ClientCode/text()", nsManager).Value };

                                    mailPieceCustomInfo.Add(customInfo);
                                }
                            }
                        }
                        else
                            mailPieceDTO.IsReturnInfoSpecified = false;


                        ValidationResultAMC validationResult = cfg.ValidateXMLNode(mailPieceNode, nsManager);
                        if (validationResult.Code == OutComeCodes.No_Error)
                        {
                            string filePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);
                            if (!File.Exists(filePath))
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.Missing_PDF;
                                mailPieceDTO.ErrorDesc = "Pdf file is not available for this package.";
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }
                            else
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                            }
                        }
                        else
                        {
                            mailPieceDTO.ErrorCode = validationResult.Code;
                            mailPieceDTO.ErrorDesc = validationResult.ErrorMessage;
                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                        }

                    }
                    catch (Exception ex)
                    {
                        mailPieceDTO.ErrorCode = OutComeCodes.Invalid_XML;
                        mailPieceDTO.ErrorDesc = ex.ToString();
                        mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                    }

                    mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo);
                }
                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now);
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);
            }
        }

        public void AcknowledgeBatches()
        {
            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Validated);
            if (validatedBatches == null)
            {
                log.Debug("Acknowledge job queue is empty.");
                return;
            }

            foreach (var batch in validatedBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.AuthoMailRootFolder);

                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    var validMailPieces = data.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                    if (validMailPieces.Count == 0)
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                        mbi.MailBatch.ValidationErrors = "XML doesn't have any valid nodes.";
                        mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);
                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    }
                    else if (validMailPieces.Count < data.MailPieces.Count)
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.PartiallyValid.ToString();
                        mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);
                    }
                    else
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Valid.ToString();
                        mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);
                    }


                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.AcknowledgeStart, Common.Now);

                    MailUtility.SendDownloadNotificationEmail(mbi, data, sftChannelDetails);

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.AcknowledgeEnd, Common.Now);
                }
            }
        }
        #endregion

        #region Reporting Process
        public void ReportPreppedProcess()
        {
            List<MailBatchDTO> resReceivedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.ShippingResponseReceived);
            if (resReceivedBatches == null)
            {
                log.Debug("Uploading report job queue is empty.");
                return;
            }

            foreach (var batch in resReceivedBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);
                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.AuthoMailRootFolder);
                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    try
                    {
                        StringBuilder deliveryReport = cfg.USAADailyDetailOperationReport(data, sftChannelDetails.Title, client.TimeZone);

                        if (deliveryReport != null)
                        {
                            string deliveryReportFileName = "{0}_{1}.csv".Fill("DR", data.MailBatch.BatchName);
                            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(deliveryReport.ToString())))
                            {
                                using (FileStream file = new FileStream(Path.Combine(mbi.AMResponsePath, deliveryReportFileName), FileMode.Create, FileAccess.Write))
                                {
                                    ms.WriteTo(file);
                                }
                            }
                        }

                        StringBuilder errorReport = cfg.USAAErrorReport(data, sftChannelDetails.Title, client.TimeZone);

                        if (errorReport != null)
                        {
                            string errorReportFileName = "{0}_{1}.csv".Fill("ER", data.MailBatch.BatchName);
                            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(errorReport.ToString())))
                            {
                                using (FileStream file = new FileStream(Path.Combine(mbi.AMResponsePath, errorReportFileName), FileMode.Create, FileAccess.Write))
                                {
                                    ms.WriteTo(file);
                                }
                            }
                        }

                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured while generating reports : {0}".Fill(ex.ToString()));
                    }
                }
            }
        }

        public void ReportBackProcess()
        {
            List<MailBatchDTO> reportPreppedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.ReportPrepped);
            if (reportPreppedBatches == null)
            {
                log.Debug("Uploading report job queue is empty.");
                return;
            }

            foreach (var batch in reportPreppedBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);

                string uploadPath = cfg.ReportSFTUploadingPathAMC;

                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.AuthoMailRootFolder);

                var reportList = Directory.EnumerateFiles(mbi.AMResponsePath, "*.csv").Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("DR_") || Path.GetFileNameWithoutExtension(f).StartsWith("ER_")).ToList();

                try
                {
                    if (reportList != null && reportList.Count > 0)
                    {
                        //Here for now we are taking SFT Code to Upload Reports on different channel

                        //SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelCode(Common.ReportSFTUploadingChannelCode);
                        SFTUtil sftSite = null;

                        SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);
                        if (sftChannelDetails != null)
                        {
                            try
                            {
                                sftSite = new SFTUtil(cfg.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                                sftSite.Connect();

                                if (sftSite.IsDownloadDirectoryAvailable(uploadPath))
                                {
                                    foreach (string filePath in reportList)
                                    {
                                        string fileName = Path.GetFileName(filePath);
                                        string ftpUploadPath = Path.Combine(uploadPath, fileName);
                                        sftSite.UploadFile(ftpUploadPath, filePath);
                                    }
                                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);
                                }
                                else
                                {
                                    log.Error("Upload path does not exist for Client-{0} and SFT Channel Id-{1}".Fill(client.Name, sftChannelDetails.Code));
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }
                            sftSite.Disconnect();
                        }
                        else
                        {
                            log.Error("SFT site is not available or not enabled to upload reports.");
                        }
                    }
                    else
                    {
                        log.Error("Reports does not exist to upload for BatchId-{0}".Fill(batch.Id));
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error occured while uploading report to Client's SFT site for Batch : {0}. Please see error :" + Environment.NewLine + ex);
                }
            }
        }
        #endregion
    }
}
