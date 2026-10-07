using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using AM.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AM.Console
{
    class TDBankChannel : AMSchemaChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string channelType;

        private static XmlSchemaSet schemas;

        static TDBankChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", XmlReader.Create(new StringReader(Resources.AMMailPieceSchema)));
        }
        public TDBankChannel(string channelCode, string channelType)
            : base(channelCode, channelType)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
        }

        protected override void EnumerateBatches()
        {
            //Download File logic and separate according to clientcode in csv
            log.Debug("Enumerate Batches job queue for " + SFTChannel.Code + " is started.");

            if (SFTChannel.ServerIP.IsNullOrWhiteSpace() || Config.Common_ServerIP != SFTChannel.ServerIP)
            {
                log.Debug("System cannot process TD channel on this server.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000, SFTChannel.Port);
            sftSite.Connect();

            List<string> filePaths = GetFilePaths(sftSite);

            foreach (string file in filePaths)
            {
                try
                {
                    string filePath = Path.Combine(SFTChannel.ToADEDir, file + ".zip");

                    //Download and Insert the batch in database first always.
                    string batchName = file;

                    DateTime lastModifiedTime = Common.Now;
                    if (channelType == ChannelType.Trigger)
                    {
                        //TODO : Implement check for zip exist or not
                        lastModifiedTime = sftSite.LastModifiedTimeOfFile(filePath);
                    }

                    #region Create General Channel Directory.
                    //Here first we are cleaning AMC folder
                    string channelDownloadPath = Path.Combine(Config.Common_AuthoMailRootFolder, "GENERAL", SFTChannel.GetChannelCode());

                    if (Directory.Exists(channelDownloadPath))
                    {
                        Directory.Delete(channelDownloadPath, true);
                    }

                    //To ensure that folder gets created.
                    Thread.Sleep(1000);
                    Directory.CreateDirectory(channelDownloadPath);

                    #endregion

                    #region Download Zip
                    string zipFileName = "{0}.zip".Fill(batchName);
                    string localZipFilePath = Path.Combine(channelDownloadPath, zipFileName);
                    string ftpZipFilePath = Path.Combine(SFTChannel.ToADEDir, zipFileName);
                    string ftpTriggerFilePath = Path.Combine(SFTChannel.ToADEDir, "{0}.go".Fill(batchName));
                    string localArchivePath = Path.Combine(Config.Common_AuthoMailRootFolder, "Archive", batchName + ".xlsx");

                    if (!sftSite.IsFileAvailable(ftpZipFilePath))
                    {
                        var msg = "'{0}.zip' file is missing on SFT site.".Fill(batchName);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "");
                        continue;
                    }

                    //Some time it is not creating AMC folder in flow so here we are again checking this if not created then below code will create
                    if (!Directory.Exists(channelDownloadPath))
                    {
                        log.Error("General Channel Folder was not created as expected.");
                        Directory.CreateDirectory(channelDownloadPath);
                    }

                    DownloadBatchZipFile(sftSite, ftpZipFilePath, localZipFilePath);

                    string extractedDir = Path.Combine(Path.GetDirectoryName(localZipFilePath), Path.GetFileNameWithoutExtension(localZipFilePath));
                    try
                    {
                        if (!Directory.Exists(extractedDir))
                        {
                            Directory.CreateDirectory(extractedDir);
                        }

                        ZipFile.ExtractToDirectory(localZipFilePath, extractedDir);
                    }
                    catch (Exception ex)
                    {
                        var msg = "Unable to extract '{0}.zip' file.".Fill(batchName);
                        log.Error(msg, ex);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localZipFilePath);
                        continue;
                    }
                    #endregion

                    var xlsFile = Directory.EnumerateFiles(extractedDir, "*.xlsx");

                    if (xlsFile.Count() == 0)
                    {
                        var msg = "Excel file is missing in zip file of {0}.".Fill(batchName);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "");
                        continue;
                    }

                    var localexcelFilePath = xlsFile.SingleOrDefault();

                    // Code for reading Excel file
                    DataTable excelData;
                    try
                    {
                        var excel = new XLS(localexcelFilePath);
                        excelData = excel.GetData("Sheet1");
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured in Reading Excel For BatchName: {0} with Error : {1}".Fill(file, ex.Message);
                        log.Error(msg, ex);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localZipFilePath);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        continue;
                    }

                    //var lines = File.ReadAllLines(localexcelFilePath, Encoding.Default).ToList();
                    if (excelData == null || excelData.Rows.Count == 0)
                    {
                        MailUtility.SendUnexpectedExceptionEmail("No records found for received file {0}.xlsx".Fill(batchName));
                        if (File.Exists(localexcelFilePath))
                        {
                            if (Directory.Exists(Path.Combine(Config.Common_AuthoMailRootFolder, "Archive")))
                            {
                                if (File.Exists(localArchivePath))
                                    File.Delete(localArchivePath);

                                File.Move(localexcelFilePath, localArchivePath);
                            }
                        }
                        MoveBatchToBackupDir(sftSite, ftpZipFilePath);
                        continue;
                    }

                    List<TDBankData> tdBankData = excelData.FromDataTable<TDBankData>();

                    bool isAllSuccess = SeparatePackages(tdBankData, localexcelFilePath, batchName, channelDownloadPath, lastModifiedTime, sftSite);

                    //Here after moving all batches to particular client location , archive amc batch

                    if (File.Exists(localArchivePath))
                        File.Delete(localArchivePath);

                    if (File.Exists(localexcelFilePath))
                        File.Move(localexcelFilePath, localArchivePath);

                    //Deleting Local Directory 
                    if (Directory.Exists(channelDownloadPath))
                        Directory.Delete(channelDownloadPath, true);


                    if (isAllSuccess)
                    {
                        MoveBatchToBackupDir(sftSite, ftpZipFilePath);
                        MoveBatchToBackupDir(sftSite, ftpTriggerFilePath);
                    }

                }
                catch (Exception ex)
                {
                    var msg = "Error occured in EnumerateBatches Method For BatchName: {0} with Error : {1}".Fill(file, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }
        }

        private bool SeparatePackages(List<TDBankData> tdbankClientData, string localExcelFilePath, string batchName, string localExcelDirPath, DateTime lastModifiedTime, SFTUtil sftSite)
        {

            Client client;
            MailBatchDTO mailBatch;
            try
            {
                var clientCode = "TDBANK";
                client = clientSvc.GetClientByAppCode(clientCode).Result;

                if (client == null)
                {
                    var msg = "Client code {0} is not valid.".Fill(clientCode);
                    ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localExcelFilePath);
                    return false;
                }

                string batchPathByClient = Path.Combine(localExcelDirPath, client.AppCode);

                if (!Directory.Exists(batchPathByClient))
                {
                    Thread.Sleep(1000);
                    log.Info("Directory path was not exist.So creating new directory based on client {0}".Fill(batchPathByClient));
                    Directory.CreateDirectory(batchPathByClient);
                }

                if (!Directory.Exists(batchPathByClient))
                {
                    Thread.Sleep(1000);
                    log.Info("Directory path was not exist.So creating new directory based on client {0}".Fill(batchPathByClient));
                    Directory.CreateDirectory(batchPathByClient);
                }
                log.Info("Local Folder is created");
                //NOTE : Path : {channelDownloadPath}/{localCsvFilePath}.FileNameWithOutExtention/{ClientCode}/{XML}
                string xmlName = "{0}_{1}".Fill(client.AppCode, lastModifiedTime.ToString("MMddyyyyhhmmss"));
                var batchUniqueId = Guid.NewGuid();
                mailBatch = new MailBatchDTO
                {
                    ClientId = client.Id,
                    BatchName = xmlName,
                    SFTChannelId = SFTChannel.Id,
                    ReceivedOn = lastModifiedTime,
                    PipelineStatus = BatchPipelineStatus.Received,
                    BatchStatus = BatchStatus.InProgress.ToString(),
                    //Parse AM mail batch request element
                    UniqueId = batchUniqueId,
                    Priority = Priority.Medium.ToString(),
                    ClientBatchId = batchName,
                    IsReturnInfoSpecified = false,
                    ServerIP = Config.Common_ServerIP
                };

                MailBatch mb = new MailBatch()
                {
                    ClientCode = client.AppCode,
                    Id = batchUniqueId
                };

                int cnt = 0;
                foreach (var loanInfo in tdbankClientData)
                {
                    cnt++;
                    MailPiece mp = new MailPiece()
                    {
                        BusinessFriendlyIdName = "AccountEndingIn",
                        BusinessFriendlyIdValue = loanInfo.AccountEndingIn.IsNullOrWhiteSpace() ? null : loanInfo.AccountEndingIn,
                        DocumentOrPackageType = loanInfo.LetterID,
                        Id = Guid.NewGuid()
                    };

                    Entities.Address borrowerAddress = new Entities.Address();
                    borrowerAddress.Street = loanInfo.MailingAddress1;
                    borrowerAddress.City = loanInfo.City1;
                    borrowerAddress.State = loanInfo.State1;
                    borrowerAddress.Zip = loanInfo.ZipCode1;

                    Entities.Recipient borrowerName = new Entities.Recipient();
                    borrowerName.FirstName = loanInfo.CustomerName1;
                    //borrowerName.LastName = loanInfo.CustomerName2;

                    AM.Entities.RecipientInfo recipientInfo = new Entities.RecipientInfo();
                    recipientInfo.Recipient = borrowerName;
                    recipientInfo.Address = borrowerAddress;

                    List<AM.Entities.Metadata> mpCustInfo = new List<Metadata>();
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "LetterID", Value = loanInfo.LetterID });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.DateTime, Key = "CurrentDate", Value = Convert.ToDateTime(loanInfo.CurrentDate).ToString("MMMM dd, yyyy") });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CustomerName1", Value = loanInfo.CustomerName1 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CustomerName2", Value = loanInfo.CustomerName2 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "MailingAddress1", Value = loanInfo.MailingAddress1 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "City1", Value = loanInfo.City1 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "State1", Value = loanInfo.State1 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "ZipCode1", Value = loanInfo.ZipCode1 });

                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "MailingAddress2", Value = loanInfo.MailingAddress2 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "City2", Value = loanInfo.City2 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "State2", Value = loanInfo.State2 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "ZipCode2", Value = loanInfo.ZipCode2 });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "AccountEndingIn", Value = loanInfo.AccountEndingIn });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyAddress", Value = loanInfo.PropertyAddress });
                    mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "Greeting", Value = loanInfo.Greeting });

                    mp.RecipientInfo = recipientInfo;
                    mp.CustomInfo = mpCustInfo;

                    DocumentOrPackage borrowerDocument = new DocumentOrPackage();
                    borrowerDocument.Name = $"{mp.Id}.pdf";

                    mp.DocumentOrPackage = borrowerDocument;
                    //NOTE : CODE FOR Generating PDFs for selected templates Naming Conventions = {LoanNumber+ +_+{cnt}.pdf                       
                    mb.MailPieces.Add(mp);
                }

                //NOTE : Generate XML 

                var xmlByClient = mb.Serialize<MailBatch>();

                string xmlFilePath = Path.Combine(localExcelDirPath, client.AppCode, "{0}.xml".Fill(xmlName));
                File.WriteAllText(xmlFilePath, xmlByClient);
                log.Info("Local XML File is created");
            }
            catch (Exception ex)
            {
                var msg = "Error occured while processing batch from Batch '{0}.xml'".Fill(batchName);
                log.Error(msg, ex);

                if (Directory.Exists(localExcelDirPath))
                    Directory.Delete(localExcelDirPath, true);

                MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                return false;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);
                    //Client client = clientSvc.GetClientById(mb.ClientId);
                    //NOTE : Generate Batch path
                    string channelBatchPath = Path.Combine(localExcelDirPath, client.AppCode);
                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    //Here we create folder structure
                    mbi.AMResponsePath.CreateDirectoryIfNotExist();
                    mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                    mbi.PnSResponsePath.CreateDirectoryIfNotExist();
                    //Here we are moving whole directory so no need to create AMRequestPath directly.
                    Directory.Move(channelBatchPath, mbi.AMRequestPath);

                    commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, Common.Now).Wait();

                    scope.Complete();

                }
                catch (Exception ex)
                {
                    var msg = "Error occured while inserting General Channel batches details for Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));

                    return false;
                }
            }
            return true;
        }

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string localCsvFilePath)
        {
            Client client = clientSvc.GetClientById(mailBatch.ClientId);

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();


            //For Invalid Batch, we will generate err file 
            string invalidBatchFileName = "{0}_InvalidExcelfile.err".Fill(mailBatch.BatchName);

            string invalidBatchPath = Path.Combine(mbi.AMResponsePath, invalidBatchFileName);

            File.WriteAllText(invalidBatchPath, string.Empty);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            var mailBatchSFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mailBatch.SFTChannelId).Result;

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(mailBatchSFTChannel.Protocol), mailBatchSFTChannel.Host, mailBatchSFTChannel.UserName, mailBatchSFTChannel.Password, 300000, mailBatchSFTChannel.Port);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);

            if (sftSite.IsFileAvailable(Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".xlsx")))
                MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".xlsx"));

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();
        }

        private MailBatchDTO InsertInvalidBatchInfo(string batchName, DateTime receivedOn, string errorMessage)
        {
            MailBatchDTO mailBatch = new MailBatchDTO();
            mailBatch.UniqueId = Guid.NewGuid();
            mailBatch.ClientId = GetInvalidClient(SFTChannel.GetChannelCode()).Id;
            mailBatch.SFTChannelId = SFTChannel.Id;
            mailBatch.BatchName = batchName;
            mailBatch.ClientBatchId = batchName;
            mailBatch.Priority = Priority.Low.ToString();
            mailBatch.ServerIP = Config.Common_ServerIP;
            mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);
            commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, receivedOn).Wait();
            mailBatchSvc.MarkMailBatchAsInvalid(mailBatch.Id, errorMessage).Wait();
            commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.Failed.ToString()).Wait();
            return mailBatch;
        }

        private Client GetInvalidClient(string sftChannelCode)
        {
            return clientSvc.GetClientByAppCode(sftChannelCode + "_Invalid").Result;
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string localZipFilePath)
        {
            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            return localZipFilePath;
        }

        private void MoveBatchToBackupDir(SFTUtil sftSite, string ftpFilePath)
        {
            if (sftSite.IsFileAvailable(ftpFilePath))
            {
                if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                {
                    string ftpBackUpFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(ftpFilePath));
                    sftSite.MoveFile(ftpFilePath, ftpBackUpFilePath);
                }
                else
                    sftSite.DeleteFile(ftpFilePath);
            }
        }

        private List<string> GetFilePaths(SFTUtil sftSite)
        {
            var regex = new Regex(@"^([\s\S]*).go$", RegexOptions.IgnoreCase);

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableTriggerFilePaths != null && availableTriggerFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} file(s) found.", availableTriggerFilePaths.Count));
            }
            else
            {
                log.Error("No files found hence no batches are available to download.");
            }

            if (SFTChannel.Code.Contains("TDBANK") && SFTChannel.Host.Contains("FAGOSFTP"))
            {
                availableTriggerFilePaths = availableTriggerFilePaths.Where(path => Path.GetFileNameWithoutExtension(path).StartsWith(Config.Common_EnvironmentInitial)).ToList();
            }

            return availableTriggerFilePaths;
        }

        protected override void DownloadBatches()
        {
            log.Debug("TDBANK download job is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Received, SFTChannel.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug("TDBANK download job queue is empty.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000, SFTChannel.Port);
            sftSite.Connect();

            foreach (var mailBatch in downloadBatches)
            {
                try
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    var xmlFilePath = Directory.EnumerateFiles(mbi.AMRequestPath, "*.xml").SingleOrDefault();

                    if (string.IsNullOrWhiteSpace(xmlFilePath))
                    {
                        log.Error("File does not exist");
                        MailUtility.SendUnexpectedExceptionEmail("No xml found for generating pdf for Batch Id {0} and client name : {1}".Fill(mbi.MailBatch.Id, client.AppCode));
                        continue;
                    }

                    var mbData = File.ReadAllText(xmlFilePath).Deserialize<MailBatch>();

                    foreach (var mp in mbData.MailPieces)
                    {
                        DocType docType = docTypeSvc.GetDocTypeByDocTypeName(mailBatch.ClientId, mp.DocumentOrPackageType, 0, mailBatch.ReceivedOn.Value.UtcToEst());

                        CustomTemplate customTemplate = commonSvc.GetCustomTemplateByDocTypeId(docType.Id, mp.CustomInfo.SingleOrDefault(m => m.Key == "LetterID").Value);
                        var printReadyTemplatePath = Path.Combine(mbi.AMRequestPath, mp.DocumentOrPackage.Name);

                        if (customTemplate == null)
                        {
                            log.Error("No Template found");
                            //NOTE Report Invalid                           

                            continue;
                        }

                        var templateFilePath = Path.Combine(Config.Monitor_CustomTemplateFolder, client.AppCode, customTemplate.CustomTemplatePath);
                        var base64Template = Convert.ToBase64String(File.ReadAllBytes(templateFilePath));
                        dynamic objFloodRequest = new ExpandoObject();
                        objFloodRequest.Data = new
                        {
                            CurrentDate = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "CurrentDate")?.Value,
                            CustomerName1 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "CustomerName1")?.Value,
                            CustomerName2 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "CustomerName2")?.Value,
                            MailingAddress1 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "MailingAddress1").Value,
                            City1 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "City1").Value,
                            State1 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "State1").Value,
                            ZipCode1 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "ZipCode1").Value,
                            //MailingAddress2 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "MailingAddress2").Value,
                            //City2 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "City2").Value,
                            //State2 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "State2").Value,
                            //ZipCode2 = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "ZipCode2").Value,
                            AccountEndingIn = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "AccountEndingIn").Value,
                            PropertyAddress = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyAddress").Value,
                            Greeting = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "Greeting").Value
                        };

                        objFloodRequest.Settings = new
                        {
                            MailMergeOptions = "RemoveEmptyParagraphs",
                            ReportEngineOptions = "RemoveEmptyParagraphs",
                            OutputDocType = "pdf",
                            Template = base64Template
                        };

                        objFloodRequest.TransformData = null;

                        GenerateDoc(objFloodRequest, mp.DocumentOrPackage.Name, mbi).Wait();
                    }

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now).Wait();
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in DownloadBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            log.Debug("Download job queue is completed.");
        }

        private async Task GenerateDoc(dynamic objFloodRequest, string pdfFileName, MailBatchInfo batchInfo)
        {
            using (var httpClient = new HttpClient())
            using (var stringContent = new StringContent(JsonConvert.SerializeObject(objFloodRequest), Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage httpResponse = await httpClient.PostAsync(Config.Common_DocGen_API, stringContent);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                if (!httpResponse.IsSuccessStatusCode || httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    //await _commonService.UpdateLienDisqualifyReason(lienRecord.Id, responseString);
                    return;
                }
                var docGenResponse = JsonConvert.DeserializeObject<DocGenResponse>(responseString);
                string finalPdfPath = Path.Combine(batchInfo.AMRequestPath, pdfFileName);

                File.WriteAllBytes(finalPdfPath, docGenResponse.DocumentData);
            }
        }

        protected override void ValidateBatches()
        {
            base.ValidateBatches();
        }

        protected override void AcknowledgeBatches()
        {
            base.AcknowledgeBatches();
        }
    }
}
