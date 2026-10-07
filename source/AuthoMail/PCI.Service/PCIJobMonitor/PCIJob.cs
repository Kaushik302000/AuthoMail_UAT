using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data;
using PCI.Service.Utilities;
using System.Text.RegularExpressions;
using PCI.Lib;
using System.Data;
using System.Threading;
using System.Transactions;
using Nordis.Lib;

namespace PCI.Service
{
    public static class PCIJobMonitor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PCIJobMonitor));
        private static Database db = DatabaseFactory.CreateDatabase();
        private static List<PCIField> MRDFRequestSchema { get; set; }

        static PCIJobMonitor()
        {
            5.TimesAttempt(() => MRDFRequestSchema = DAL.GetMRDFRequestSchema());
        }

        #region Submit Batches To Print Vendors

        public static void SubmitBatchToPrintVendor(string pvCode, bool isBundle, List<string> ptNames)
        {
            List<string> unprocessedPrintTrails = new List<string>();

            var pieceIdFormat = MRDFRequestSchema.Single(f => f.FieldName == "PieceId").DataFormat;
            log.Info("PCIJob SubmitBatchToPrintVendor Checking isBundle");
            if (isBundle)
            {
                try
                {
                    DateTime startDateTime = Common.EstToUtc(Common.EstToday);
                    DateTime toDateTime = Common.GetCurrentTime();
                    DateTime endDateTime = new DateTime(toDateTime.Year, toDateTime.Month, toDateTime.Day, toDateTime.Hour, toDateTime.Minute, 0);
                    int bundlingTime = 0;

                    //Waiting Code for AuthoMail as well as BatchSummary
                    while (!DAL.CheckBatchesInQueue(startDateTime, endDateTime, Common.ServerIP))
                    {
                        log.Debug("Queue Check Waiting...");

                        Thread.Sleep(6000);

                        //NOTE: If bundling is continueously running for 6 minutes then system will continue further for submitting batches to print vendor.
                        bundlingTime += 6000;
                        if (bundlingTime >= 360000)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    var msg = "SubmitBatchToPrintVendor : Error occured in PCIJob Monitor while checking for batches in queue.";
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));

                    //NOTE: Reprocess if any exception occured while getting bundling queue data check
                    Thread.Sleep(9000);
                    SubmitBatchToPrintVendor(pvCode, isBundle, ptNames);
                    return;
                }
            }
            log.Info("PCIJob SubmitBatchToPrintVendor Completed Checking isBundle");
            log.Info("PCIJob SubmitBatchToPrintVendor Inserting Loop Started");
            foreach (string ptName in ptNames)
            {
                try
                {
                    log.Info("Inserting Process Started For Print Trail {0}".Fill(ptName));
                    InsertStage(ptName, pvCode);
                    log.Info("Inserting Process Completed For Print Trail {0}".Fill(ptName));
                }
                catch (Exception ex)
                {
                    var msg = "SubmitBatchToPrintVendor : Error occured in PCIJob Monitor while getting print trail batch details for PrintTrail - {0}".Fill(ptName);
                    log.Error(msg, ex);

                    MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));

                    //NOTE: Here System will add unprocessed print trails to process it once all other print trails are getting processed successfully.
                    unprocessedPrintTrails.Add(ptName);
                    continue;
                }
            }
            log.Info("PCIJob SubmitBatchToPrintVendor Inserting Loop Ended");
            //NOTE: Here System will process unprocessed print trails.
            if (unprocessedPrintTrails.Count > 0)
            {
                log.Info("Processing Again Unprocessed PrintTrails {0}".Fill(unprocessedPrintTrails));
                SubmitBatchToPrintVendor(pvCode, isBundle, unprocessedPrintTrails);
                log.Info("Processing Completed Unprocessed PrintTrails {0}".Fill(unprocessedPrintTrails));
                return;
            }
            log.Info("PCIJob SubmitBatchToPrintVendor Submit Process Call Started");
            //NOTE : Here system will try to submit bundle batches 5 times if any error is coming during execution
            30.TimesAttempt(() => SubmitBundledBatches(pvCode));
            log.Info("PCIJob SubmitBatchToPrintVendor Submit Process Call Ended");
        }

        public static void SubmitBundledBatches(string pvCode)
        {
            //NOTE : Here system will process not submitted batches if any error occured while getting any type of data. 
            //NOTE : It will be used specifically for PCI and NORDIS client because they all have fix sweeps.
            var notSubmittedList = DAL.GetNotSubmittedPrintBatchDetailByPrintVendorCode(pvCode, Common.ServerIP);

            while (notSubmittedList.Count > 0)
            {
                log.Info("Submit Bundle Process Started");
                SubmitBatches(notSubmittedList, pvCode);
                log.Info("Submit Bundle Process Completed");
                Thread.Sleep(12000);
                notSubmittedList = DAL.GetNotSubmittedPrintBatchDetailByPrintVendorCode(pvCode, Common.ServerIP);
            }
        }

        private static void SubmitBatches(List<PrintTrailBatch> ptBatchList, string pvCode)
        {
            Parallel.ForEach(ptBatchList, (ptBatch) =>
            {
                ProcessPrintTrailBatches(ptBatch, pvCode);
            });
        }

        private static void ProcessPrintTrailBatches(PrintTrailBatch ptBatch, string pvCode)
        {
            try
            {
                log.Info("Prepped Stage Execution Started For PrintTrailBatchId - {0}".Fill(ptBatch.Id));
                PrepAndUploadStage(ptBatch);
                log.Info("Prepped Stage Execution Completed For PrintTrailBatchId - {0}".Fill(ptBatch.Id));
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("connection attempt failed") || ex.Message.Contains("connected party did not properly respond") || ex.Message.Contains("established connection failed") || ex.Message.Contains("target machine actively refused it"))
                {
                    var connectionMsg = "Error occured while connecting to Print Vendor {0}'s SFT Site with <br/> below exception <br/>{1}".Fill(pvCode, ex.Message);
                    log.Error(ex);
                    MailUtility.SendSFTConnectionIssueNotificationEmail(connectionMsg, pvCode);
                    Thread.Sleep(60000);
                    return;
                }

                var msg = "SubmitBatchToPrintVendor : Error occured while uploading batch to print vendor for PrintTrailBatchId - {0}".Fill(ptBatch.Id);
                log.Error(msg, ex);

                MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));

                return;
            }

            try
            {
                log.Info("Submitted Stage Execution Started For PrintTrailBatchId - {0}".Fill(ptBatch.Id));
                SubmitStage(ptBatch);
                log.Info("Submitted Stage Execution Completed For PrintTrailBatchId - {0}".Fill(ptBatch.Id));
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("connection attempt failed") || ex.Message.Contains("connected party did not properly respond") || ex.Message.Contains("established connection failed") || ex.Message.Contains("target machine actively refused it"))
                {
                    var connectionMsg = "Error occured while connecting to Print Vendor {0}'s SFT Site with <br/> below exception <br/>{1}".Fill(pvCode, ex.Message);
                    log.Error(ex);
                    MailUtility.SendSFTConnectionIssueNotificationEmail(connectionMsg, pvCode);
                    Thread.Sleep(60000);
                    return;
                }

                var msg = "SubmitBatchToPrintVendor : Error occured while updating Batch Status for PrintTrailBatchId - {0}".Fill(ptBatch.Id);
                log.Error(msg, ex);

                MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));

                return;
            }
        }

        private static void InsertStage(string ptName, string pvCode)
        {
            PrintTrailDetail ptDetail = DAL.GetPrintTrailDetailByPrintTrailName(ptName, pvCode);

            if (ptDetail == null)
            {
                log.Debug("SubmitBatchToPrintVendor : Print Trail {0} is not exist in the system. Please verify it.".Fill(ptName));
                return;
            }

            List<MailPieceDetail> mpDetails = DAL.GetMRDFRequestDataByPrintTrailId(ptDetail.PrintTrailId, Common.ServerIP);

            if (mpDetails == null || mpDetails.Count == 0)
            {
                log.Debug("SubmitBatchToPrintVendor : Submit Batches job queue is empty for Print Trail {0}".Fill(ptName));
                return;
            }

            if (ptDetail.VendorCode == "NORDIS")
            {
                var mpDetailsGroupByShipperId = mpDetails.GroupBy(mp => mp.ShipperId);
                foreach (var mailPieces in mpDetailsGroupByShipperId)
                {
                    var mpByDueDate = mailPieces.GroupBy(mp => Common.UtcToEst(mp.MailPieceDueDate).Date).ToList();

                    foreach (var dueDate in mpByDueDate)
                    {
                        InsertPrintTrailBatchDetails(dueDate.ToList(), ptDetail, dueDate.Key);
                    }
                }
            }
            else
            {
                InsertPrintTrailBatchDetails(mpDetails, ptDetail, null);
            }
        }

        private static void InsertPrintTrailBatchDetails(List<MailPieceDetail> mpDetails, PrintTrailDetail ptDetail, DateTime? dueDate)
        {
            //For divert primary key issue we are using starting number as 90001
            int sequenceNo = 90001;
            PrintTrailBatch ptBatchDetail;

            ptBatchDetail = new PrintTrailBatch()
            {
                PrintTrailId = ptDetail.PrintTrailId,
                ShipperId = ptDetail.ShipperId,
                ServerIP = Common.ServerIP
            };


            using (TransactionScope scope = new TransactionScope())
            {
                log.Info("Inserting Data Started.");
                ptBatchDetail.Id = DAL.InsertPrintTrailBatchDetail(ptBatchDetail);
                log.Info("Inserting Data Successfull.");

                mpDetails.ForEach(mp =>
                {
                    if (mp.MailPieceStatus == MailPieceStatus.PulledBack.ToString() || mp.MailPieceStatus == MailPieceStatus.Error.ToString())
                        mp.SequenceNo = -1;
                    else
                    {
                        mp.PrintTrailBatchId = ptBatchDetail.Id;
                        mp.SequenceNo = sequenceNo++;
                    }
                });

                var maxSequenceNo = mpDetails.Max(p => p.SequenceNo);

                mpDetails.ForEach(mrdf =>
                {
                    if (mrdf.SequenceNo == -1)
                    {
                        mrdf.SequenceNo = ++maxSequenceNo;
                    }
                });

                log.Info("Updating Data Started For MRDFRequest.");
                DAL.UpdatedPrintTrailBatchIdInMRDFRequest(ptBatchDetail.Id, mpDetails);
                log.Info("Updating Data Completed For MRDFRequest.");

                if (ptDetail.VendorCode == "NORDIS")
                {
                    if (ptDetail.TrailName.Contains("FIRSTCLASS"))
                    {
                        if (dueDate.Value.Date == Common.EstToday.Date)
                        {
                            ptBatchDetail.PrintBatchName = "{0}_{1}_{2}_{3}".Fill(ptDetail.BusinessCode, ptBatchDetail.Id, ptDetail.TrailName, "SAMEDAY");
                        }
                        else
                        {
                            ptBatchDetail.PrintBatchName = "{0}_{1}_{2}_{3}".Fill(ptDetail.BusinessCode, ptBatchDetail.Id, ptDetail.TrailName, "NEXTDAY");
                        }
                    }
                    else
                    {
                        ptBatchDetail.PrintBatchName = "{0}_{1}_{2}".Fill(ptDetail.BusinessCode, ptBatchDetail.Id, ptDetail.TrailName);
                    }
                }
                else
                {
                    ptBatchDetail.PrintBatchName = "{0}-{1}-{2}".Fill(ptDetail.BusinessCode, ptBatchDetail.Id, ptDetail.TrailName);
                }

                log.Info("Updating Data Started For PrintTrailBatch.");
                DAL.UpdatePrintTrailBatchDetail(ptBatchDetail);
                log.Info("Updating Data Started For PrintTrailBatch.");

                scope.Complete();
            }
        }

        private static void PrepAndUploadStage(PrintTrailBatch ptBatchDetails)
        {
            log.Info($"PrepAndUploadStage Processing Started For PrintTrailBatchId : {ptBatchDetails.Id}");
            var ptDetails = DAL.GetPrintTrailDetailByPrintTrailId(ptBatchDetails.PrintTrailId);

            log.Info($"Fetching MailPiece List : {ptBatchDetails.Id}");
            var mpList = DAL.GetMRDFRequestListByPrintTrailBatchId(ptBatchDetails.Id);

            if (mpList.Count == 0)
            {
                log.Info("System did not find any records with PrintTrailBatchId : {0}.".Fill(ptBatchDetails.Id));
                return;
            }

            if (ptDetails.SimulateTrail == false)
            {
                if (ptDetails.VendorCode == "CL_DALLAS")
                {
                    log.InfoFormat("Uploading Dallas Started For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                    CreateDallasBatch(mpList, ptDetails, ptBatchDetails);
                    log.InfoFormat("Uploading Dallas Completd For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                }
                else
                {
                    log.InfoFormat("Preparing Nordis Started For PrintTrailBatchId : {0}", ptBatchDetails.Id);

                    //NOTE: Bundling folder structure will be like : BundlingRoot/PrintTrailBatchId/Request & Response
                    var batchRequestPath = Path.Combine(Common.BundlingRootFolder, ptBatchDetails.Id.ToString(), Common.BundlingRequestFolderName);

                    var batchResponsePath = Path.Combine(Common.BundlingRootFolder, ptBatchDetails.Id.ToString(), Common.BundlingResponseFolderName);

                    log.InfoFormat("Creating Batch Folders For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                    batchRequestPath.CreateDirectoryIfNotExist();
                    Thread.Sleep(1000);
                    batchResponsePath.CreateDirectoryIfNotExist();

                    //For NORDIS OR Any Other Print Vendor
                    var zipFilePath = Path.Combine(batchRequestPath, ptBatchDetails.PrintBatchName + ".zip");
                    var xmlFilePath = Path.Combine(batchRequestPath, ptBatchDetails.PrintBatchName + ".xml");

                    log.InfoFormat("Creating Batch For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                    string batchPath = CreateNordisBatch(mpList, ptBatchDetails, zipFilePath, xmlFilePath);
                    log.InfoFormat("Creating Batch For PrintTrailBatchId : {0}", ptBatchDetails.Id);

                    log.InfoFormat("Uploading Started For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                    UploadBatchToPrintVendorSite(batchPath, ptDetails);
                    log.InfoFormat("Uploading Completd For PrintTrailBatchId : {0}", ptBatchDetails.Id);

                    log.InfoFormat("Preparing Nordis Completd For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                }
            }

            log.Info($"PrepAndUploadStage Processing Completed For PrintTrailBatchId : {ptBatchDetails.Id}");
        }

        private static void SubmitStage(PrintTrailBatch ptBatchDetails)
        {

            //Get total counts of envelope / BRE envelopes used for valid mail pieces.
            Dictionary<int?, int> envelopeData = new Dictionary<int?, int>();

            var ptDetails = DAL.GetPrintTrailDetailByPrintTrailId(ptBatchDetails.PrintTrailId);

            var mpList = DAL.GetMRDFRequestListByPrintTrailBatchId(ptBatchDetails.Id);

            #region Envelope Stock Update
            //After Updating status of Batch System is going to update envelope inventory stock.
            var envelopes = mpList.GroupBy(c => c.EnvelopeId);

            #region Calculate Inserts used in data
            var insertIds = mpList.Where(e => !e.Inserts.IsNullOrWhiteSpace()).Select(c => c.InsertsIds);

            Dictionary<int, int> insertCounts = new Dictionary<int, int>();
            foreach (var inserts in insertIds)
            {
                var insertIdValues = inserts.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id));

                foreach (var id in insertIdValues)
                {
                    if (insertCounts.ContainsKey(id))
                    {
                        insertCounts[id] += 1;
                    }
                    else
                    {
                        insertCounts.Add(id, 1);
                    }
                }
            }
            #endregion

            foreach (var env in envelopes.Where(e => e.Key != null))
            {
                envelopeData.Add(env.Key, mpList.Count(mp => mp.EnvelopeId == env.Key));
            }

            foreach (var insertEnv in insertCounts)
            {
                if (envelopeData.ContainsKey(insertEnv.Key))
                {
                    envelopeData[insertEnv.Key] += insertEnv.Value;
                }
                else
                {
                    envelopeData.Add(insertEnv.Key, insertEnv.Value);
                }
            }
            #endregion

            SFTUtil sftSite = new SFTUtil(ptDetails.Protocol.ParseEnum<SFT_ProtocolType>(), ptDetails.Host, ptDetails.UserName, ptDetails.Password,
               60000);

            if (ptDetails.SimulateTrail == false)
                sftSite.Connect();


            log.DebugFormat("Updating status started For PrintTrailBatchId : {0}", ptBatchDetails.Id);

            using (TransactionScope scope = new TransactionScope())
            {
                //Reason for maintaining this time because it should reflect same for 
                var currentTime = Common.GetCurrentTime();

                DAL.UpdateSubmittedForAllResReceivedBatch(ptBatchDetails.Id, currentTime);

                DAL.UpdatePrintTrailBatchStatus(ptBatchDetails.Id, BatchPipelineStatus.Submitted, currentTime);

                //NOTE : Here we are inserting used envelope stock by PrintTrailBatch in EnvelopeUsage table
                DAL.UpdateEnvelopeInventory(ptBatchDetails, envelopeData);

                if (ptDetails.VendorCode != "CL_DALLAS" && ptDetails.SimulateTrail == false)
                {
                    string serverFilePath = "{0}/{1}.uploading".Fill(ptDetails.FromADEDir, ptBatchDetails.PrintBatchName);
                    if (sftSite.IsFileAvailable(serverFilePath))
                        sftSite.RenameFile(serverFilePath, "{0}/{1}.zip".Fill(ptDetails.FromADEDir, ptBatchDetails.PrintBatchName));

                    MailUtility.SendBatchSubmittedNotificationEmail(ptDetails.VendorCode, ptDetails.PrintVendorCode, "{0}.zip".Fill(ptBatchDetails.PrintBatchName), mpList.Count, mpList.Sum(c => c.TotalPages));
                }

                scope.Complete();
            }

            log.DebugFormat("Updating status completed For PrintTrailBatchId : {0}", ptBatchDetails.Id);

            if (ptDetails.SimulateTrail == false)
                sftSite.Disconnect();
        }

        private static void CreateDallasBatch(List<MailPieceDetail> mpList, PrintTrailDetail ptDetails, PrintTrailBatch ptBatchDetail)
        {
            SFTUtil sftSite = new SFTUtil(ptDetails.Protocol.ParseEnum<SFT_ProtocolType>(), ptDetails.Host, ptDetails.UserName, ptDetails.Password,
                60000);

            sftSite.Connect();
            var sw = new Stopwatch();
            sw.Start();

            UploadBatchToInternalPrintVendorSite(Common.PCIRootFolder, sftSite, mpList, ptDetails, ptBatchDetail);

            sw.Stop();
            var time = sw.Elapsed;

            log.DebugFormat("Total time spent : {0} For Dallas Batch", time);
            sftSite.Disconnect();
        }

        private static string CreateNordisBatch(List<MailPieceDetail> mpList, PrintTrailBatch ptBatchDetails, string zipFilePath, string xmlFilePath)
        {
            var shippingMethod = mpList.Select(x => new { x.ShippingCarrier, ShippingServiceType = x.ShippingServiceType == "IMB" ? "FIRSTCLASS" : (x.ShippingServiceType == "FEDEX_2_DAY" || x.ShippingServiceType == "STANDARD_OVERNIGHT") ? "PRIORITY_OVERNIGHT" : x.ShippingServiceType }).Distinct().First();

            List<Package> packageData = new List<Package>();
            PrintRequest printRequestXml = new PrintRequest();
            printRequestXml.JobId = ptBatchDetails.Id;
            printRequestXml.ShippingMethod = shippingMethod.ShippingCarrier.ParseEnum<ShippingMethod>();
            printRequestXml.ShippingServiceType = (shippingMethod.ShippingServiceType == "FEDEX_2_DAY" || shippingMethod.ShippingServiceType == "STANDARD_OVERNIGHT") ? "PRIORITY_OVERNIGHT".ParseEnum<Nordis.Lib.ShippingServiceType>() : shippingMethod.ShippingServiceType.ParseEnum<Nordis.Lib.ShippingServiceType>();

            if (File.Exists(zipFilePath))
            {
                log.Debug("File was already exist on path {0}.".Fill(zipFilePath));
                File.Delete(zipFilePath);
            }

            using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var package in mpList)
                {
                    var packageInfo = new Package()
                    {
                        PieceId = package.MailPieceId.Value,
                        FileName = package.PDFFileName,
                        DueDate = Common.UtcToEst(package.MailPieceDueDate),
                        TotalSheets = package.TotalSheets,
                        ClientCode = package.Alias == "DELFUS" ? $"{package.Alias}_{package.CustomerId}" : package.Alias,
                        IsForeignAddress = package.IsForeignAddress,
                        TotalImages = package.TotalPages,
                        PaperSize = package.PrintSize,
                        PrintFormat = package.PrintFormat.ParseEnum<PrintFormat>(),
                        BREPartNo = package.InsertsPartNos,
                        BREDesc = package.Inserts,
                        DocumentCode = package.DocumentCode,
                        QCReqd = package.DivertVS3 == 1 ? true : false,
                    };

                    if (package.IsChecks)
                    {
                        var totalChecks = package.ChecksPageNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;

                        packageInfo.NT_DOCID_1_TotalImages = totalChecks;
                        packageInfo.NT_DOCID_1_TotalSheets = totalChecks;

                        packageInfo.NT_DOCID_2_TotalImages = (package.TotalPages - totalChecks);
                        packageInfo.NT_DOCID_2_TotalSheets = (package.TotalPages - totalChecks);
                    }

                    printRequestXml.Packages.Add(packageInfo);

                    //NOTE:Here we are directly creating Zip with Bundling into another path
                    //We have folder structure like PCIRoot/AppCode/JobId/PCIRequest
                    var localFilePath = Path.Combine(Common.PCIRootFolder, package.AppCode, package.JobId.ToString(), Common.PCIRequestFolderName, package.PDFFileName);
                    newFile.CreateEntryFromFile(localFilePath, package.PDFFileName);
                }

                File.WriteAllText(xmlFilePath, printRequestXml.ToXml());
                //make an entry for mrdf file after making entries of files into zip
                newFile.CreateEntryFromFile(xmlFilePath, Path.GetFileName(xmlFilePath));
            }

            //NOTE : Here system is storing file size of received pdf in KB
            FileInfo fileInfo = new FileInfo(zipFilePath);
            Int64 fileSize = (fileInfo.Length / 1024);
            DAL.UpdatePrintTrailBatchSize(ptBatchDetails.Id, fileSize);

            return zipFilePath;
        }

        private static void UploadBatchToPrintVendorSite(string zipFilePath, PrintTrailDetail ptDetails)
        {
            log.Debug("Started SFT Connection To {0}".Fill(ptDetails.VendorCode));
            SFTUtil sftSite = new SFTUtil(ptDetails.Protocol.ParseEnum<SFT_ProtocolType>(), ptDetails.Host, ptDetails.UserName, ptDetails.Password,
               60000);
            sftSite.Connect();
            log.Debug("SFT Connection successfull To {0}".Fill(ptDetails.VendorCode));
            var sw = new Stopwatch();
            sw.Start();

            sftSite.UploadFile(zipFilePath, string.Format("{0}/{1}.uploading", ptDetails.FromADEDir, Path.GetFileNameWithoutExtension(zipFilePath)), false, false);

            sw.Stop();
            var time = sw.Elapsed;
            log.DebugFormat("Total time spent : {0} For Batch : {1}", time, Path.GetFileNameWithoutExtension(zipFilePath));

            sftSite.Disconnect();
            log.Debug("SFT Connection disconnected with {0}".Fill(ptDetails.VendorCode));

        }

        private static void UploadBatchToInternalPrintVendorSite(string pciRootPath, SFTUtil sftSite, List<MailPieceDetail> mailPiecesDetails, PrintTrailDetail printTrailDetail, PrintTrailBatch ptBatchDetail)
        {
            var dallasBatchTimings = DAL.GetDallasBatchTimingsFromTable();

            mailPiecesDetails.ForEach(mp =>
            {
                mp.DallasBatchName = dallasBatchTimings.SingleOrDefault(x => x.IsSaturday == mp.EffectiveReceivedOnEst.Value.IsSaturday() && x.AppCode == mp.AppCode && x.ShippingCarrier == mp.ShippingCarrier && x.ShippingServiceType == mp.ShippingServiceType && (mp.EffectiveReceivedOnEst.Value >= (mp.EffectiveReceivedOnEst.Value.Date + x.StartTime) && mp.EffectiveReceivedOnEst.Value <= (mp.EffectiveReceivedOnEst.Value.Date + x.EndTime))).BatchName;
            });

            var mpGroupesBySweep = mailPiecesDetails.GroupBy(x => new { x.AppCode, x.DallasBatchName, EffectiveDate = x.EffectiveReceivedOnEst.Value.Date });

            foreach (var mpGroup in mpGroupesBySweep)
            {
                if (ptBatchDetail.IsSinglePdf)
                {
                    var batchUploadPath = Path.Combine(printTrailDetail.FromADEDir, mpGroup.Key.AppCode, mpGroup.Key.EffectiveDate.ToString("MMddyyyy"), mpGroup.Key.DallasBatchName);

                    var batchRequestPath = Path.Combine(Common.BundlingRootFolder, ptBatchDetail.Id.ToString(), Common.BundlingRequestFolderName);

                    log.InfoFormat("Creating Dallas Batch Folders For Single Pdf PrintTrailBatchId : {0}", ptBatchDetail.Id);
                    batchRequestPath.CreateDirectoryIfNotExist();
                    Thread.Sleep(1000);

                    if (!sftSite.IsDownloadDirectoryAvailable(batchUploadPath))
                        sftSite.CreateDirectory(batchUploadPath);

                    var mpgroupedByConfiguration = mpGroup.GroupBy(x => new { x.ShippingCarrier, x.ShippingServiceType, x.EnvelopeName, x.Inserts, x.PrintSize });

                    foreach (var mpByConfig in mpgroupedByConfiguration)
                    {
                        var insertsNames = !mpByConfig.Key.Inserts.IsNullOrWhiteSpace() ? $"-{mpByConfig.Key.Inserts.Replace(",", "_")}" : "";

                        var mpFilePathsData = mpByConfig.Select(x => Path.Combine(Path.Combine(pciRootPath, mpGroup.Key.AppCode, x.JobId.ToString(), Common.PCIRequestFolderName, x.PDFFileName))).ToList();

                        string mergedPdfFileName = $"{ptBatchDetail.Id}-{mpGroup.Key.DallasBatchName}-{mpGroup.Key.AppCode}-{mpByConfig.Key.ShippingCarrier}-{mpByConfig.Key.ShippingServiceType}-{mpByConfig.Key.EnvelopeName}{insertsNames}.pdf";

                        string mergedPdfFilePath = Path.Combine(batchRequestPath, mergedPdfFileName);
                        if (mpFilePathsData.Count == 1)
                        {
                            File.Copy(mpFilePathsData.SingleOrDefault(), mergedPdfFilePath, true);
                        }
                        else
                        {
                            mpFilePathsData.Add(mergedPdfFilePath);

                            if (File.Exists(mergedPdfFilePath))
                            {
                                File.Delete(mergedPdfFilePath);
                            }

                            PdfUtil.MergePDFs(mpFilePathsData.ToArray());
                        }

                        string pdfDestPath = Path.Combine(batchUploadPath, mergedPdfFileName);

                        sftSite.UploadFile(Path.Combine(batchRequestPath, mergedPdfFileName), pdfDestPath);

                        DAL.UpdateBulkFilePathForDallas(mpGroup.ToList(), pdfDestPath);
                    }
                }
                else
                {
                    var mpgroupedByConfiguration = mpGroup.GroupBy(x => new { x.ShippingCarrier, x.ShippingServiceType, x.EnvelopeName, x.Inserts, x.PrintSize });

                    foreach (var mpByConfig in mpgroupedByConfiguration)
                    {
                        var insertsNames = !mpByConfig.Key.Inserts.IsNullOrWhiteSpace() ? $"-INSERTS" : "";

                        var batchUploadPath = Path.Combine(printTrailDetail.FromADEDir, mpGroup.Key.AppCode, mpGroup.Key.EffectiveDate.ToString("MMddyyyy"), mpGroup.Key.DallasBatchName, mpByConfig.Key.ShippingCarrier + "_" + mpByConfig.Key.ShippingServiceType + "_" + mpByConfig.Key.PrintSize, $"{mpByConfig.Key.EnvelopeName}{insertsNames}");

                        if (!sftSite.IsDownloadDirectoryAvailable(batchUploadPath))
                            sftSite.CreateDirectory(batchUploadPath);

                        foreach (var mailPiece in mpByConfig)
                        {
                            string localFilePath = Path.Combine(pciRootPath, mailPiece.AppCode, mailPiece.JobId.ToString(), Common.PCIRequestFolderName, mailPiece.PDFFileName);

                            string fullFilePath = Path.Combine(batchUploadPath, mailPiece.PDFFileName);
                            sftSite.UploadFile(localFilePath, fullFilePath);

                            DAL.UpdateFilePathForDallas(mailPiece, fullFilePath);
                        }
                    }
                }

                if (ptBatchDetail.IsManifestRequired)
                {
                    var manifestUploadPath = Path.Combine(printTrailDetail.FromADEDir, mpGroup.Key.AppCode, mpGroup.Key.EffectiveDate.ToString("MMddyyyy"), mpGroup.Key.DallasBatchName);

                    var batchRequestPath = Path.Combine(Common.BundlingRootFolder, ptBatchDetail.Id.ToString(), Common.BundlingRequestFolderName);

                    var barcode = $"BS{ptBatchDetail.Id}{mpGroup.Key.AppCode}{mpGroup.Key.DallasBatchName}{mpGroup.Key.EffectiveDate.ToString("MMddyyyy")}";

                    log.InfoFormat("Creating Dallas Batch Folders For Barcode Manifest Pdf PrintTrailBatchId : {0}", ptBatchDetail.Id);
                    batchRequestPath.CreateDirectoryIfNotExist();
                    Thread.Sleep(1000);

                    GenerateBarcodeManifest(barcode, batchRequestPath, manifestUploadPath, ptBatchDetail, mpGroup.ToList(), sftSite);

                    DAL.UpdateBarcodeNumberForDallas(mpGroup.ToList(), barcode);
                }
            }
        }

        private static void GenerateBarcodeManifest(string barcode, string batchRequestPath, string submitDir, PrintTrailBatch ptBatchDetail, List<MailPieceDetail> mailPiecesDetails, SFTUtil sftSite)
        {
            var manifestPdfFileName = $"{barcode}-Manifest.pdf";

            var manifestOutputPath = Path.Combine(batchRequestPath, manifestPdfFileName);

            var groupingData = mailPiecesDetails.GroupBy(x => new { x.AppCode, x.EnvelopeName, x.ShippingCarrier, x.ShippingServiceType, x.Inserts });

            var smartDocData = groupingData.Select(x => new { x.Key.AppCode, x.Key.EnvelopeName, x.Key.ShippingCarrier, x.Key.ShippingServiceType, x.Key.Inserts, TotalPackages = x.Count(), TotalPages = x.Sum(p => p.TotalPages) }).ToList();

            dynamic objData = new { Barcode = barcode, smartDocData };

            PdfUtil.GenerateBarcodeManifest(Common.DocGenAPIUrl, Common.ManifestTemplatePath, manifestOutputPath, objData);

            string manifestDestPath = Path.Combine(submitDir, manifestPdfFileName);

            sftSite.UploadFile(manifestOutputPath, manifestDestPath);
        }
        #endregion

        #region Download Response MRDF and update batch status in DB.
        public static void GetPrintVendorResponse()
        {
            log.Debug("Print vendor response download job started.");

            List<SubmittedBatches> submittedPrintBatches;
            submittedPrintBatches = DAL.GetSubmittedBatches(Common.ServerIP);
            if (submittedPrintBatches.Count == 0)
            {
                log.Debug($"PrintVendorResponse's job queue is empty for server - {Common.ServerIP}.");
                return;
            }

            var groupBYPrintVendorCode = submittedPrintBatches.Where(x => x.PrintVendorCode != "CL_DALLAS").GroupBy(x => x.PrintVendorCode);

            foreach (var submittedBatches in groupBYPrintVendorCode)
            {
                try
                {
                    var printVendorDetail = DAL.GetPrintVendorDetailByVendorCode(submittedBatches.Key);

                    SFTUtil sftSite = new SFTUtil(printVendorDetail.Protocol.ParseEnum<SFT_ProtocolType>(), printVendorDetail.Host, printVendorDetail.UserName, printVendorDetail.Password, 60000);

                    sftSite.Connect();

                    CheckAndDownloadResponseFile(sftSite, submittedBatches.ToList(), printVendorDetail);

                    sftSite.Disconnect();
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while downloading report from Print Vendor {0}'s site".Fill(submittedBatches.Key);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));
                }
            }

            log.Debug("MRDF download job ended.");
        }

        private static void CheckAndDownloadResponseFile(SFTUtil sftSite, List<SubmittedBatches> submittedPrintBatches, PrintVendor printVendorDetail)
        {
            List<string> responseFiles;
            responseFiles = sftSite.ListDirectory(printVendorDetail.ToADEDir);

            switch (printVendorDetail.VendorCode)
            {
                case "PCI":
                    string printVendorResponseType = ".zip";
                    string printVendorResponseFileExtention = "OUT";

                    if (string.IsNullOrEmpty(printVendorResponseType) || string.IsNullOrEmpty(printVendorResponseFileExtention))
                        return;

                    var printVendorResponseFiles = responseFiles
                        .Where(file => Path.GetExtension(file).ToLower() == printVendorResponseType)
                        .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
                        .ToList();

                    foreach (var batch in submittedPrintBatches)
                    {
                        batch.ReceivedFileList.AddRange(printVendorResponseFiles.Where(resfile => resfile.Key.StartsWith(batch.PrintBatchName, StringComparison.CurrentCultureIgnoreCase)).Select(resfile => resfile.Value));
                    }

                    var downloadableFiles = submittedPrintBatches.Where(c => c.ReceivedFileList != null && c.ReceivedFileList.Count > 0).ToList();

                    if (downloadableFiles.Count == 0)
                    {
                        log.Debug("Print vendor {0}'s responses are not available for download.".Fill(printVendorDetail.VendorCode));
                        return;
                    }

                    #region Download MRDF

                    foreach (var downloadableBatch in downloadableFiles)
                    {
                        log.Debug("Downloading print response For Print Trail Batch-ID : " + downloadableBatch.PrintTrailBatchId.ToString());
                        foreach (string filePath in downloadableBatch.ReceivedFileList)
                        {
                            log.Debug("Downloading Print Vendor Response Job Started For Batch {0}".Fill(Path.GetFileNameWithoutExtension(filePath)));

                            string localPrintVendorResponseZipFilePath = Path.Combine(Common.PCIRootFolder, filePath);
                            string localPrintVendorResponseFilePath = Path.Combine(Common.PCIRootFolder, "{0}.{1}".Fill(Path.GetFileNameWithoutExtension(localPrintVendorResponseZipFilePath), printVendorResponseFileExtention));
                            string remoteVendorResponsePath = Path.Combine(printVendorDetail.ToADEDir, filePath);

                            DateTime responseReceivedOn = sftSite.LastModifiedTimeOfFile(remoteVendorResponsePath);
                            DateTime responseDownloadStartTime = Common.GetCurrentTime();

                            bool responseDelete = printVendorDetail.PostDownloadAction == "Archive" ? false : true;

                            sftSite.DownloadFile(remoteVendorResponsePath, localPrintVendorResponseZipFilePath, responseDelete, false);

                            if (!responseDelete)
                            {
                                sftSite.MoveFile(remoteVendorResponsePath, Path.Combine(printVendorDetail.ToADEDir, "Archive", filePath));
                            }

                            using (ZipArchive archive = ZipFile.OpenRead(localPrintVendorResponseZipFilePath))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (entry.FullName.ToUpper().EndsWith(".{0}".Fill(printVendorResponseFileExtention), StringComparison.OrdinalIgnoreCase))
                                    {
                                        entry.ExtractToFile(localPrintVendorResponseFilePath, true);
                                    }
                                }
                            }

                            IdentifyAndUpdateBatchStatus(localPrintVendorResponseZipFilePath, localPrintVendorResponseFilePath, responseReceivedOn, downloadableBatch.PrintTrailBatchId);
                        }
                    }

                    #endregion
                    break;
                case "NORDIS":
                    var nordisResponseExtention = ".xml";

                    if (string.IsNullOrEmpty(nordisResponseExtention))
                        return;

                    printVendorResponseFiles = responseFiles
                        .Where(file => Path.GetExtension(file).ToLower() == nordisResponseExtention)
                        .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
                        .ToList();

                    foreach (var batch in submittedPrintBatches)
                    {
                        batch.ReceivedFileList.AddRange(printVendorResponseFiles.Where(resfile => resfile.Key.StartsWith(batch.PrintBatchName, StringComparison.CurrentCultureIgnoreCase)).Select(resfile => resfile.Value));
                    }

                    downloadableFiles = submittedPrintBatches.Where(c => c.ReceivedFileList != null && c.ReceivedFileList.Count > 0).ToList();

                    if (downloadableFiles.Count == 0)
                    {
                        log.Debug("Print vendor {0}'s responses are not available for download.".Fill(printVendorDetail.VendorCode));
                        return;
                    }

                    #region Download MRDF

                    foreach (var downloadableBatch in downloadableFiles)
                    {
                        log.Debug("Downloading print response For Print Trail Batch-ID : " + downloadableBatch.PrintTrailBatchId.ToString());
                        foreach (string filePath in downloadableBatch.ReceivedFileList)
                        {
                            log.Debug("Downloading Print Vendor Response Job Started For Batch {0}".Fill(Path.GetFileNameWithoutExtension(filePath)));


                            string localPrintVendorResponseFilePath = Path.Combine(Common.PCIRootFolder, "{0}.{1}".Fill(Path.GetFileNameWithoutExtension(filePath), nordisResponseExtention));
                            string remoteVendorResponsePath = Path.Combine(printVendorDetail.ToADEDir, filePath);

                            DateTime responseReceivedOn = sftSite.LastModifiedTimeOfFile(remoteVendorResponsePath);
                            DateTime responseDownloadStartTime = Common.GetCurrentTime();

                            bool responseDelete = printVendorDetail.PostDownloadAction == "Archive" ? false : true;

                            sftSite.DownloadFile(remoteVendorResponsePath, localPrintVendorResponseFilePath, responseDelete, false);

                            if (!responseDelete)
                            {
                                sftSite.MoveFile(remoteVendorResponsePath, Path.Combine(printVendorDetail.ToADEDir, "Archive", filePath));
                            }

                            IdentifyAndUpdateNORDISBatchStatus(localPrintVendorResponseFilePath, responseReceivedOn, downloadableBatch.PrintTrailBatchId);
                        }
                    }

                    #endregion
                    break;
                default:
                    break;
            }


        }

        private static void IdentifyAndUpdateBatchStatus(string localResponseZipFilePath, string localResponseFilePath, DateTime responseReceivedOn, int printTrailBatchId)
        {
            string fileName = Path.GetFileNameWithoutExtension(localResponseFilePath);
            string outFileName = Path.GetFileNameWithoutExtension(localResponseFilePath);

            PrintTrailBatch printBatchDetail = DAL.GetPrintBatchDetailByPrintBatchTrailId(printTrailBatchId);

            if (printBatchDetail != null)
            {
                var bundlingRootPath = Path.Combine(Common.BundlingRootFolder, printTrailBatchId.ToString(), Common.BundlingResponseFolderName);

                if (!Directory.Exists(bundlingRootPath))
                {
                    bundlingRootPath.CreateDirectoryIfNotExist();
                }

                if (File.Exists(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseZipFilePath))))
                    File.Delete(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseZipFilePath)));

                //This will move zip file from root folder of pci response folder also.
                File.Move(localResponseZipFilePath, Path.Combine(bundlingRootPath, Path.GetFileName(localResponseZipFilePath)));


                if (File.Exists(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath))))
                    File.Delete(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath)));

                File.Move(localResponseFilePath, Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath)));

                log.DebugFormat("Response moved in proper batch folder : {0} after batch identified.", printTrailBatchId);
            }
        }

        private static void IdentifyAndUpdateNORDISBatchStatus(string localResponseFilePath, DateTime responseReceivedOn, int printTrailBatchId)
        {
            string fileName = Path.GetFileNameWithoutExtension(localResponseFilePath);
            string outFileName = Path.GetFileNameWithoutExtension(localResponseFilePath);

            PrintTrailBatch printBatchDetail = DAL.GetPrintBatchDetailByPrintBatchTrailId(printTrailBatchId);

            if (printBatchDetail != null)
            {
                var bundlingRootPath = Path.Combine(Common.BundlingRootFolder, printTrailBatchId.ToString(), Common.BundlingResponseFolderName);

                if (!Directory.Exists(bundlingRootPath))
                {
                    bundlingRootPath.CreateDirectoryIfNotExist();
                }

                if (File.Exists(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath))))
                    File.Delete(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath)));

                File.Move(localResponseFilePath, Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath)));

                log.Debug($"MRDF Response File {fileName} Received On {responseReceivedOn} For PrintTrailBatchId : {printTrailBatchId}");

                log.DebugFormat("Response moved in proper batch folder : {0} after batch identified.", printTrailBatchId);
            }
        }
        #endregion

        #region Return PDF Zip File Logic
        //public static void GetReturnPDFsZip()
        //{
        //    log.Debug("Return PDF job started.");
        //    List<SubmittedBatches> mrdfReceivedPrintBatches;
        //    mrdfReceivedPrintBatches = DAL.GetMRDFReceivedBatches();
        //    if (mrdfReceivedPrintBatches.Count == 0)
        //    {
        //        log.Debug("Return PDF job queue is empty.");
        //    }
        //    else
        //    {
        //        SFTUtil sftSite = new SFTUtil(Common.PCI_SFT_Protocol, Common.PCI_SFT_Host, Common.PCI_SFT_Username, Common.PCI_SFT_Password, 300000);

        //        sftSite.Connect();

        //        CheckAndDownloadReturnPDFZipFile(sftSite, mrdfReceivedPrintBatches);

        //        sftSite.Disconnect();
        //    }

        //    log.Debug("MRDF download job ended.");
        //}

        //private static void CheckAndDownloadReturnPDFZipFile(SFTUtil sftSite, List<SubmittedBatches> mrdfReceivedPrintBatches)
        //{
        //    List<string> zipFiles;
        //    zipFiles = sftSite.ListDirectory(Common.PCI_SFT_ReturnPDFDir);

        //    var retZipFiles = zipFiles
        //        .Where(file => Path.GetExtension(file).ToLower() == ".zip")
        //        .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
        //        .ToList();

        //    foreach (var batch in mrdfReceivedPrintBatches)
        //    {
        //        batch.ReceivedFileList.AddRange(retZipFiles.Where(zipfile => zipfile.Key.StartsWith(batch.PrintBatchName, StringComparison.CurrentCultureIgnoreCase)).Select(zipfile => zipfile.Value));
        //    }

        //    var downloadableFiles = mrdfReceivedPrintBatches.Where(c => c.ReceivedFileList != null && c.ReceivedFileList.Count > 0).ToList();

        //    if (downloadableFiles.Count == 0)
        //    {
        //        log.Debug("Return PDFs zip files are not available for download.");
        //    }
        //    else
        //    {
        //        foreach (var downloadableBatch in downloadableFiles)
        //        {
        //            log.Debug("Downloading return PDFs zip For Batch-ID : " + downloadableBatch.PrintTrailBatchId.ToString());
        //            foreach (string fileName in downloadableBatch.ReceivedFileList)
        //            {
        //                log.Debug("Downloading PCI Return PDF Job Started For Batch {0}".Fill(Path.GetFileNameWithoutExtension(fileName)));

        //                //Reason for creating ReturnPDFs in PCI root folder because we are currenly processing return MRDF file in same root folder and naming convention of its zip file is same as return PDFs' zip file.
        //                string localPDFsZipFilePath = Path.Combine(Common.PCIRootReturnPDFsZipFolder, fileName);

        //                string remoteZipPath = Path.Combine(Common.PCI_SFT_ReturnPDFDir, fileName);
        //                DateTime zipReceivedOn = sftSite.LastModifiedTimeOfFile(remoteZipPath);

        //                sftSite.DownloadFile(localPDFsZipFilePath, remoteZipPath, Common.Remove_PDF_Of_PCI_SFT, false);

        //                IdentifyAndMoveZip(localPDFsZipFilePath, zipReceivedOn, downloadableBatch.PrintTrailBatchId);
        //            }
        //        }
        //    }
        //}

        //private static void IdentifyAndMoveZip(string localPDFsZipFilePath, DateTime zipReceivedOn, int printBatchId)
        //{
        //    BatchSummary bs = DAL.GetBatchSummaryById(printBatchId);

        //    if (bs != null)
        //    {
        //        Client app = DAL.GetClientAppDetailsById(bs.ClientId);
        //        BatchInfo bi = new BatchInfo(bs, app);

        //        var docTypesMaps = DAL.GetDocTypeDetailsByAppId(app.Id).Where(x => x.DocTypeId == bi.BatchSummary.DocTypeId).First();
        //        if (docTypesMaps != null)
        //            bi.InitializeDocTypeMaps(docTypesMaps, bi.BatchSummary.PrintBatch, bi.Client.AppCode, bi.Client.CustCode);

        //        if (!Directory.Exists(bi.BatchRepo))
        //        {
        //            string errorMsg = string.Format("Local directory does not exists for the batch : {0} for {1}", printBatchId, localPDFsZipFilePath);
        //            log.ErrorFormat(errorMsg);
        //        }
        //        else
        //        {
        //            if (!Directory.Exists(bi.PCIResponseReturnPDFsPath))
        //                Directory.CreateDirectory(bi.PCIResponseReturnPDFsPath);

        //            string batchFolderZipFilePath = Path.Combine(bi.PCIResponseReturnPDFsPath, Path.GetFileName(localPDFsZipFilePath));

        //            if (File.Exists(batchFolderZipFilePath))
        //                File.Delete(batchFolderZipFilePath);

        //            //This will move zip file from root folder of pci response folder also.
        //            File.Move(localPDFsZipFilePath, batchFolderZipFilePath);

        //            //After moving to return pdf
        //            using (ZipArchive archive = ZipFile.OpenRead(batchFolderZipFilePath))
        //            {
        //                foreach (ZipArchiveEntry entry in archive.Entries)
        //                {
        //                    if (entry.FullName.ToUpper().EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        entry.ExtractToFile(Path.Combine(bi.PCIResponseReturnPDFsPath, Path.GetFileName(entry.FullName)), true);
        //                    }
        //                }
        //            }

        //            log.DebugFormat("Return PDFs zip is moved in proper batch folder : {0} after batch identified.", printBatchId);
        //        }
        //    }
        //}
        #endregion
    }
}
