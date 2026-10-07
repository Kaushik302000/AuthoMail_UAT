using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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
    class CapOneChannel : AMSchemaChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string channelType;

        private static XmlSchemaSet schemas;

        static CapOneChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", XmlReader.Create(new StringReader(Resources.AMMailPieceSchema)));
        }
        public CapOneChannel(string channelCode, string channelType)
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
                log.Debug("System cannot process Flood channel on this server.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000, SFTChannel.Port);
            sftSite.Connect();

            List<string> filePaths = GetFilePaths(sftSite);

            foreach (string file in filePaths)
            {
                string localCsvFilePath = null;
                string localArchivePath = null;
                try
                {
                    string filePath = Path.Combine(SFTChannel.ToADEDir, file + ".csv");

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
                    string csvFileName = "{0}.csv".Fill(batchName);
                    string localCsvDirPath = Path.Combine(channelDownloadPath, Path.GetFileNameWithoutExtension(csvFileName));
                    localCsvFilePath = Path.Combine(localCsvDirPath, csvFileName);
                    string ftpCsvFilePath = Path.Combine(SFTChannel.ToADEDir, csvFileName);

                    if (!sftSite.IsFileAvailable(ftpCsvFilePath))
                    {
                        var msg = "'{0}.csv' file is missing on SFT site.".Fill(batchName);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localCsvFilePath);
                        MoveFileToErrorFolder(localCsvFilePath, msg);
                        continue;
                    }

                    //Some time it is not creating AMC folder in flow so here we are again checking this if not created then below code will create
                    if (!Directory.Exists(localCsvDirPath))
                    {
                        log.Error("General Channel Folder was not created as expected.");
                        Directory.CreateDirectory(localCsvDirPath);
                    }

                    DownloadBatchCsvFile(sftSite, ftpCsvFilePath, localCsvFilePath);
                    localArchivePath = Path.Combine(Config.Common_AuthoMailRootFolder, "Archive", "{0}.csv".Fill(batchName));
                    //Generate List from CSV 
                    Dictionary<string, List<FloodData>> caponeClientData = new Dictionary<string, List<FloodData>>();
                    var lines = File.ReadAllLines(localCsvFilePath, Encoding.Default).ToList();
                    if (lines.Count <= 1)
                    {
                        var msg = "No records found for received file {0}.csv".Fill(batchName);
                        MailUtility.SendUnexpectedExceptionEmail(msg);

                        if (File.Exists(localCsvFilePath))
                        {
                            MoveFileToErrorFolder(localCsvFilePath, msg);
                        }
                        if (File.Exists(localCsvFilePath))
                        {

                            if (Directory.Exists(Path.Combine(Config.Common_AuthoMailRootFolder, "Archive")))
                            {

                                if (File.Exists(localArchivePath))
                                    File.Delete(localArchivePath);

                                File.Move(localCsvFilePath, localArchivePath);
                            }
                        }
                        MoveBatchToBackupDir(sftSite, ftpCsvFilePath);
                        continue;
                    }

                    List<FloodData> data = lines.Skip(1).Select(v => FromCsv(v)).ToList();

                    var clData = data.GroupBy(cl => cl.ClientCode);
                    foreach (var clInfo in clData)
                    {
                        caponeClientData.Add(clInfo.Key, clInfo.Where(cl => cl.ClientCode == clInfo.Key).ToList());
                    }

                    bool isAllSuccess = SeparatePackagesByClient(caponeClientData, localCsvFilePath, batchName, localCsvDirPath, lastModifiedTime, sftSite);
                    #endregion
                    //Here after moving all batches to particular client location , archive amc batch

                    if (File.Exists(localArchivePath))
                        File.Delete(localArchivePath);

                    if (File.Exists(localCsvFilePath))
                        File.Move(localCsvFilePath, localArchivePath);

                    //Deleting Local Directory 
                    if (Directory.Exists(localCsvDirPath))
                        Directory.Delete(localCsvDirPath, true);


                    if (isAllSuccess)
                        MoveBatchToBackupDir(sftSite, ftpCsvFilePath);

                }
                catch (Exception ex)
                {
                    string msg = "Error occured in EnumerateBatches Method For BatchName: {0} with Error : {1}".Fill(file, ex.Message);
                    log.Error(msg, ex);

                    if (File.Exists(localArchivePath))
                    {
                        MoveFileToErrorFolder(localArchivePath, msg);
                    }
                    if (File.Exists(localCsvFilePath))
                    {
                        MoveFileToErrorFolder(localCsvFilePath, msg);
                    }
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }
        }

        private bool SeparatePackagesByClient(Dictionary<string, List<FloodData>> floodClientData, string localCsvFilePath, string batchName, string localCsvDirPath, DateTime lastModifiedTime, SFTUtil sftSite)
        {

            bool isAllSuccess = false;
            List<MailBatchDTO> mbList = new List<MailBatchDTO>();
            int clientCounter = 0;

            foreach (var floodData in floodClientData)
            {
                Client client;
                try
                {
                    var clientCode = floodData.Key.IsNullOrWhiteSpace() ? "InvalidClientCode" : floodData.Key;

                    if (clientCode == "InvalidClientCode")
                    {
                        var msg = "Client code is required in provided csv.".Fill(clientCode);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localCsvFilePath);
                        MoveFileToErrorFolder(localCsvFilePath, msg);
                        continue;
                    }

                    client = clientSvc.GetClientByName(clientCode).Result;

                    if (client == null)
                    {
                        var msg = "Client code {0} is not valid from provided csv.".Fill(clientCode);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localCsvFilePath);
                        MoveFileToErrorFolder(localCsvFilePath, msg);
                        continue;
                    }

                    string batchPathByClient = Path.Combine(localCsvDirPath, client.AppCode);

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

                    var eDeliveryData = floodData.Value.Where(x => x.SendEDelivery == "Y" && !x.BorrowerEmailId.IsNullOrWhiteSpace()).ToList();
                    if (eDeliveryData.Count > 0)
                    {
                        foreach (var eDeliveryMP in eDeliveryData)
                        {
                            clientCounter++;
                            string xmlName = $"{client.AppCode}_{lastModifiedTime.ToString("MMddyyyyhhmmss")}_{clientCounter}";
                            var batchUniqueId = Guid.NewGuid();
                            MailBatchDTO mailBatch = new MailBatchDTO
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

                            MailPiece mp = new MailPiece()
                            {
                                BusinessFriendlyIdName = "Flood Cert Number",
                                BusinessFriendlyIdValue = eDeliveryMP.FloodCertNumber.IsNullOrWhiteSpace() ? null : eDeliveryMP.FloodCertNumber,
                                DocumentOrPackageType = docTypeSvc.GetDocTypeByDocTypeName(mailBatch.ClientId, eDeliveryMP.ClientId.Trim(), 0, lastModifiedTime).DocTypeName,
                                Id = Guid.NewGuid()
                            };

                            Entities.Address borrowerAddress = new Entities.Address();
                            borrowerAddress.Street = eDeliveryMP.BorrowerStreetAddress;
                            borrowerAddress.AptOrSuiteOrOther = eDeliveryMP.BorrowerStreetAddress2;
                            borrowerAddress.City = eDeliveryMP.BorrowerCity;
                            borrowerAddress.State = eDeliveryMP.BorrowerState;
                            borrowerAddress.Zip = eDeliveryMP.BorrowerZip;
                            borrowerAddress.Country = eDeliveryMP.BorrowerCountry;

                            if (eDeliveryMP.SendEDelivery == "Y")
                            {
                                eDelivery eDeliveryInfo = new eDelivery();
                                eDeliveryInfo.PrintAndShipOnExpiry = true;
                                eDeliveryInfo.IsEsign = true; //By default client wants to opt for ESign facility
                                mp.eDelivery = eDeliveryInfo;
                            }

                            Entities.Recipient borrowerName = new Entities.Recipient();
                            borrowerName.FirstName = eDeliveryMP.BorrowerFirstName;
                            borrowerName.LastName = eDeliveryMP.BorrowerLastName;
                            borrowerName.EmailAddress = eDeliveryMP.BorrowerEmailId;

                            AM.Entities.RecipientInfo recipientInfo = new Entities.RecipientInfo();
                            recipientInfo.Recipient = borrowerName;
                            recipientInfo.Address = borrowerAddress;

                            List<AM.Entities.Metadata> mpCustInfo = new List<Metadata>();
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "LoanNumber", Value = eDeliveryMP.LoanNumber });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "AccountNumber", Value = eDeliveryMP.AccountNumber });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyStreetAddress", Value = eDeliveryMP.PropertyStreetAddress });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyCity", Value = eDeliveryMP.PropertyCity });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyState", Value = eDeliveryMP.PropertyState });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyZip", Value = eDeliveryMP.PropertyZip });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.DateTime, Key = "DocumentDate", Value = Convert.ToDateTime(eDeliveryMP.DocumentDate).ToString("MMMM dd, yyyy") });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "NoticeDate", Value = Convert.ToDateTime(eDeliveryMP.NoticeDate).ToString("MM/dd/yyyy") });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CommunityName", Value = eDeliveryMP.CommunityName });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CommunityCode", Value = eDeliveryMP.CommunityCode });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "ClientId", Value = eDeliveryMP.ClientId });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "RequestorName", Value = eDeliveryMP.RequestorName });

                            mp.RecipientInfo = recipientInfo;
                            mp.CustomInfo = mpCustInfo;

                            DocumentOrPackage borrowerDocument = new DocumentOrPackage();
                            borrowerDocument.Name = $"{eDeliveryMP.FloodCertNumber.Trim()}_1.pdf";

                            mp.DocumentOrPackage = borrowerDocument;
                            //NOTE : CODE FOR Generating PDFs for selected templates Naming Conventions = {LoanNumber+ +_+{cnt}.pdf                       
                            mb.MailPieces.Add(mp);

                            //NOTE : Generate XML 

                            var xmlByClient = mb.Serialize<MailBatch>();

                            string xmlFilePath = Path.Combine(localCsvDirPath, client.AppCode, "{0}.xml".Fill(xmlName));
                            File.WriteAllText(xmlFilePath, xmlByClient);
                            log.Info("Local XML File is created");
                            mbList.Add(mailBatch);
                        }
                    }

                    var pnsData = floodData.Value.Where(x => x.SendEDelivery == "N" || x.SendEDelivery.IsNullOrWhiteSpace() || (x.SendEDelivery == "Y" && x.BorrowerEmailId.IsNullOrWhiteSpace())).ToList();
                    if (pnsData.Count > 0)
                    {
                        clientCounter++;
                        string xmlName = $"{client.AppCode}_{lastModifiedTime.ToString("MMddyyyyhhmmss")}_{clientCounter}";
                        var batchUniqueId = Guid.NewGuid();
                        MailBatchDTO mailBatch = new MailBatchDTO
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
                        foreach (var loanInfo in pnsData)
                        {
                            cnt++;
                            MailPiece mp = new MailPiece()
                            {
                                BusinessFriendlyIdName = "Flood Cert Number",
                                BusinessFriendlyIdValue = loanInfo.FloodCertNumber.IsNullOrWhiteSpace() ? null : loanInfo.FloodCertNumber,
                                DocumentOrPackageType = docTypeSvc.GetDocTypeByDocTypeName(mailBatch.ClientId, loanInfo.ClientId.Trim(), 0, lastModifiedTime).DocTypeName,
                                Id = Guid.NewGuid()
                            };

                            Entities.Address borrowerAddress = new Entities.Address();
                            borrowerAddress.Street = loanInfo.BorrowerStreetAddress;
                            borrowerAddress.AptOrSuiteOrOther = loanInfo.BorrowerStreetAddress2;
                            borrowerAddress.City = loanInfo.BorrowerCity;
                            borrowerAddress.State = loanInfo.BorrowerState;
                            borrowerAddress.Zip = loanInfo.BorrowerZip;
                            borrowerAddress.Country = loanInfo.BorrowerCountry;

                            Entities.Recipient borrowerName = new Entities.Recipient();
                            borrowerName.FirstName = loanInfo.BorrowerFirstName;
                            borrowerName.LastName = loanInfo.BorrowerLastName;
                            borrowerName.EmailAddress = loanInfo.BorrowerEmailId;

                            AM.Entities.RecipientInfo recipientInfo = new Entities.RecipientInfo();
                            recipientInfo.Recipient = borrowerName;
                            recipientInfo.Address = borrowerAddress;

                            List<AM.Entities.Metadata> mpCustInfo = new List<Metadata>();
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "LoanNumber", Value = loanInfo.LoanNumber });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "AccountNumber", Value = loanInfo.AccountNumber });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyStreetAddress", Value = loanInfo.PropertyStreetAddress });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyCity", Value = loanInfo.PropertyCity });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyState", Value = loanInfo.PropertyState });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyZip", Value = loanInfo.PropertyZip });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.DateTime, Key = "DocumentDate", Value = Convert.ToDateTime(loanInfo.DocumentDate).ToString("MMMM dd, yyyy") });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "NoticeDate", Value = Convert.ToDateTime(loanInfo.NoticeDate).ToString("MM/dd/yyyy") });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CommunityName", Value = loanInfo.CommunityName });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CommunityCode", Value = loanInfo.CommunityCode });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "ClientId", Value = loanInfo.ClientId });
                            mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "RequestorName", Value = loanInfo.RequestorName });

                            mp.RecipientInfo = recipientInfo;
                            mp.CustomInfo = mpCustInfo;

                            DocumentOrPackage borrowerDocument = new DocumentOrPackage();
                            borrowerDocument.Name = "{0}_{1}.pdf".Fill(loanInfo.FloodCertNumber.Trim(), cnt);

                            mp.DocumentOrPackage = borrowerDocument;
                            //NOTE : CODE FOR Generating PDFs for selected templates Naming Conventions = {LoanNumber+ +_+{cnt}.pdf                       
                            mb.MailPieces.Add(mp);
                        }

                        //NOTE : Generate XML 

                        var xmlByClient = mb.Serialize<MailBatch>();

                        string xmlFilePath = Path.Combine(localCsvDirPath, client.AppCode, "{0}.xml".Fill(xmlName));
                        File.WriteAllText(xmlFilePath, xmlByClient);
                        log.Info("Local XML File is created");
                        mbList.Add(mailBatch);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while processing batch from Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);

                    if (File.Exists(localCsvFilePath))
                    {
                        MoveFileToErrorFolder(localCsvFilePath, msg);
                    }


                    if (Directory.Exists(localCsvDirPath))
                        Directory.Delete(localCsvDirPath, true);

                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                    return isAllSuccess;
                }
            }
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    foreach (var mb in mbList)
                    {

                        mb.Id = mailBatchSvc.InsertBatchDetails(mb);

                        Client client = clientSvc.GetClientById(mb.ClientId);

                        //NOTE : Generate Batch path
                        string channelBatchPath = Path.Combine(localCsvDirPath, client.AppCode);

                        MailBatchInfo mbi = new MailBatchInfo(mb, client, Config.Common_AuthoMailRootFolder);

                        //Here we create folder structure
                        mbi.AMRequestPath.CreateDirectoryIfNotExist();
                        mbi.AMResponsePath.CreateDirectoryIfNotExist();
                        mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                        mbi.PnSResponsePath.CreateDirectoryIfNotExist();

                        var localFilePath = Path.Combine(localCsvDirPath, client.AppCode, $"{mbi.BatchName}.xml");
                        File.Move(localFilePath, Path.Combine(mbi.AMRequestPath, $"{mbi.BatchName}.xml"));

                        commonSvc.UpdateBatchPipeLineStatus(mb.Id, BatchPipelineStatus.Received, Common.Now).Wait();
                    }
                    scope.Complete();
                    isAllSuccess = true;

                }
                catch (Exception ex)
                {
                    var msg = "Error occured while inserting General Channel batches details for Batch '{0}.xml'".Fill(batchName);

                    log.Error(msg, ex);
                    if (File.Exists(localCsvFilePath))
                    {
                        MoveFileToErrorFolder(localCsvFilePath, msg);
                    }


                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));

                    return isAllSuccess;
                }
            }
            return isAllSuccess;
        }

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string localCsvFilePath)
        {
            Client client = clientSvc.GetClientById(mailBatch.ClientId);

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();

            //if (!string.IsNullOrEmpty(localCsvFilePath))
            //    File.Move(localCsvFilePath, Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".csv"));

            //For Invalid Batch, we will generate err file 
            string invalidBatchFileName = "{0}_InvalidCsvfile.err".Fill(mailBatch.BatchName);

            string invalidBatchPath = Path.Combine(mbi.AMResponsePath, invalidBatchFileName);

            File.WriteAllText(invalidBatchPath, string.Empty);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            var mailBatchSFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mailBatch.SFTChannelId).Result;

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(mailBatchSFTChannel.Protocol), mailBatchSFTChannel.Host, mailBatchSFTChannel.UserName, mailBatchSFTChannel.Password, 300000, mailBatchSFTChannel.Port);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);

            if (sftSite.IsFileAvailable(Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".csv")))
                MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".csv"));

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();

            if (!string.IsNullOrEmpty(localCsvFilePath) && File.Exists(localCsvFilePath))
            {
                MoveFileToErrorFolder(localCsvFilePath, "Invalid batch processed");
            }
        }
        private void MoveFileToErrorFolder(string filePath, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return;

                string errorFolderPath = Path.Combine(Config.Common_SharedPath, "Client",
                    SFTChannel.GetChannelCode(), "Inbound", "Error");

                if (!Directory.Exists(errorFolderPath))
                {
                    Directory.CreateDirectory(errorFolderPath);
                }

                string errorFilePath = Path.Combine(errorFolderPath, Path.GetFileName(filePath));
                if (File.Exists(errorFilePath))
                    File.Delete(errorFilePath);

                File.Move(filePath, errorFilePath);
                log.Info($"Moved failed file to error folder: {errorFilePath}");
                MailUtility.SendUnexpectedExceptionEmail($"{message}. File moved to error folder: {errorFilePath}");
            }
            catch (Exception moveEx)
            {
                log.Error("Failed to move file to error folder: " + filePath, moveEx);
            }
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

        private FloodData FromCsv(string csvLine)
        {
            List<string> values = csvLine.SplitCsv();
            FloodData caponeValues = new FloodData();
            caponeValues.FloodCertNumber = values[0].RemoveCSVStringFormat();
            caponeValues.AccountNumber = values[1].RemoveCSVStringFormat();
            caponeValues.BorrowerLastName = values[2].RemoveCSVStringFormat();
            caponeValues.BorrowerFirstName = values[3].RemoveCSVStringFormat();
            caponeValues.BorrowerStreetAddress = values[4].RemoveCSVStringFormat();
            caponeValues.BorrowerCity = values[5].RemoveCSVStringFormat();
            caponeValues.BorrowerState = values[6].RemoveCSVStringFormat();
            caponeValues.BorrowerZip = values[7].RemoveCSVStringFormat();
            caponeValues.DocumentDate = values[8].RemoveCSVStringFormat().AnyFormatToDateTimeString();
            caponeValues.PropertyStreetAddress = values[9].RemoveCSVStringFormat();
            caponeValues.PropertyCity = values[10].RemoveCSVStringFormat();
            caponeValues.PropertyState = values[11].RemoveCSVStringFormat();
            caponeValues.PropertyZip = values[12].RemoveCSVStringFormat();
            caponeValues.CommunityName = values[13].RemoveCSVStringFormat();
            caponeValues.CommunityCode = values[14].RemoveCSVStringFormat();
            caponeValues.LoanNumber = values[15].RemoveCSVStringFormat();
            caponeValues.NoticeDate = values[18].RemoveCSVStringFormat().AnyFormatToDateTimeString();
            caponeValues.RequestorName = values[21].RemoveCSVStringFormat().Trim();
            caponeValues.ClientId = values[22].RemoveCSVStringFormat();
            caponeValues.ClientCode = values[23].RemoveCSVStringFormat().Trim();

            caponeValues.BorrowerStreetAddress2 = values[24].RemoveCSVStringFormat().Trim();
            caponeValues.BorrowerCountry = values[25].RemoveCSVStringFormat().Trim();
            caponeValues.SendEDelivery = values[26].RemoveCSVStringFormat().Trim();
            caponeValues.BorrowerEmailId = values[27].RemoveCSVStringFormat().Trim();

            if (caponeValues.SendEDelivery == "Y" && (caponeValues.BorrowerLastName.IsNullOrWhiteSpace() || caponeValues.BorrowerFirstName.IsNullOrWhiteSpace()))
            {
                if ((caponeValues.BorrowerLastName.IsNullOrWhiteSpace() && !caponeValues.BorrowerFirstName.IsNullOrWhiteSpace()) && caponeValues.BorrowerFirstName.Any(x => Char.IsWhiteSpace(x)))
                {
                    var splitFirstName = caponeValues.BorrowerFirstName.Split(' ');

                    caponeValues.BorrowerFirstName = splitFirstName[0];
                    caponeValues.BorrowerLastName = string.Join(" ", splitFirstName.Where(x => x != splitFirstName[0]));
                }

                if ((!caponeValues.BorrowerLastName.IsNullOrWhiteSpace() && caponeValues.BorrowerFirstName.IsNullOrWhiteSpace()) && caponeValues.BorrowerLastName.Any(x => Char.IsWhiteSpace(x)))
                {
                    var splitLastName = caponeValues.BorrowerLastName.Split(' ');

                    caponeValues.BorrowerFirstName = splitLastName[0];
                    caponeValues.BorrowerLastName = string.Join(" ", splitLastName.Where(x => x != splitLastName[0]));
                }
            }

            return caponeValues;
        }

        private string DownloadBatchCsvFile(SFTUtil sftSite, string ftpCsvFilePath, string localCsvFilePath)
        {
            sftSite.DownloadFile(ftpCsvFilePath, localCsvFilePath);

            return localCsvFilePath;
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
            var regex = new Regex(@"^([\s\S]*).csv", RegexOptions.IgnoreCase);

            List<string> availableFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableFilePaths != null && availableFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} file(s) found.", availableFilePaths.Count));
            }
            else
            {
                log.Error("No files found hence no batches are available to download.");
            }

            return availableFilePaths;
        }

        protected override void DownloadBatches()
        {
            log.Debug("CAPONE download job is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Received, SFTChannel.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug("CAPONE download job queue is empty.");
                return;
            }

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
                        MailUtility.SendUnexpectedExceptionEmail($"No xml found for generating pdf for Batch Id {mbi.MailBatch.Id} and client name : {client.AppCode}");
                        continue;
                    }
                    var mbData = File.ReadAllText(xmlFilePath).Deserialize<MailBatch>();

                    foreach (var mp in mbData.MailPieces)
                    {
                        log.Debug(mp.RecipientInfo?.Recipient.FirstName);
                        log.Debug(mp.RecipientInfo?.Address.OptionalLine1AboveStreet);
                        bool isCountryUS = mp.RecipientInfo.Address.Country.IsCountryUS();
                        DocType docType = !isCountryUS
                            ? docTypeSvc.GetIntlDocTypeByDocTypeName(mailBatch.ClientId, mp.DocumentOrPackageType, 0, mailBatch.ReceivedOn.Value.UtcToEst())
                            : docTypeSvc.GetDocTypeByDocTypeName(mailBatch.ClientId, mp.DocumentOrPackageType, 0, mailBatch.ReceivedOn.Value.UtcToEst());

                        CustomTemplate customTemplate = commonSvc.GetCustomTemplateByDocTypeId(docType.Id, mp.CustomInfo.SingleOrDefault(m => m.Key == "CommunityCode")?.Value);
                        var templateFilePath = Path.Combine(Config.Monitor_CustomTemplateFolder, client.AppCode, customTemplate?.CustomTemplatePath ?? "");

                        if (customTemplate == null || !File.Exists(templateFilePath))
                        {
                            log.Error($"No valid template found for MailPiece ID: {mp.Id}");
                            continue;
                        }

                        var base64Template = Convert.ToBase64String(File.ReadAllBytes(templateFilePath));
                        dynamic objFloodRequest = new ExpandoObject();

                        objFloodRequest.Data = new
                        {
                            BorrowerFullName = mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0} {1}".Fill(mp.RecipientInfo.Recipient.FirstName, mp.RecipientInfo.Recipient.LastName).TrimIfNotNull() : "" : "",
                            BorrowerFullNameReverse = mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0}, {1}".Fill(mp.RecipientInfo.Recipient.LastName, mp.RecipientInfo.Recipient.FirstName).FormattedName().TrimIfNotNull() : "" : "",
                            BorrFName = mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0}".Fill(mp.RecipientInfo.Recipient.FirstName).TrimIfNotNull() : "" : "",
                            BorrLName = mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0}".Fill(mp.RecipientInfo.Recipient.LastName).TrimIfNotNull() : "" : "",
                            BorrStreetAddress = mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}".Fill(mp.RecipientInfo.Address.Street).TrimIfNotNull() : "" : "",
                            BorrCity = mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}".Fill(mp.RecipientInfo.Address.City).TrimIfNotNull() : "" : "",
                            BorrState = mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}".Fill(mp.RecipientInfo.Address.State).TrimIfNotNull() : "" : "",
                            BorrZip = mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}".Fill(mp.RecipientInfo.Address.Zip).TrimIfNotNull() : "" : "",
                            LoanNumber = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "LoanNumber").Value.IsNullOrWhiteSpace() ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "LoanNumber").Value.TrimIfNotNull(),
                            RetRcptFName = docType.RetRcptFirstName?.TrimIfNotNull() ?? "",

                            RetRecpAddOptLine1 = docType.RetRcptAddrOptionalLine1BelowStreet?.TrimIfNotNull() ?? "",
                            RetRecpAddStreet = docType.RetRcptAddressStreet?.TrimIfNotNull() ?? "",
                            RetRecpAddCity = docType.RetRcptAddressCity?.TrimIfNotNull() ?? "",
                            RetRecpAddState = docType.RetRcptAddressState?.TrimIfNotNull() ?? "",
                            RetRecpAddZip = docType.RetRcptAddressZIP?.TrimIfNotNull() ?? "",

                            PropStreetAddress = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyStreetAddress").Value?.TrimIfNotNull(),
                            PropCity = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyCity")?.Value?.TrimIfNotNull(),
                            PropState = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyState")?.Value?.TrimIfNotNull(),
                            PropZip = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyZip")?.Value?.TrimIfNotNull(),
                            DocumentDate = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "DocumentDate")?.Value?.TrimIfNotNull(),
                            NoticeDate = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "NoticeDate")?.Value?.TrimIfNotNull(),
                            CommunityName = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "CommunityName")?.Value?.TrimIfNotNull()
                        };


                        objFloodRequest.Settings = new ExpandoObject();
                        objFloodRequest.Settings.MailMergeOptions = "RemoveEmptyParagraphs";
                        objFloodRequest.Settings.ReportEngineOptions = "RemoveEmptyParagraphs";
                        objFloodRequest.Settings.OutputDocType = "pdf";
                        objFloodRequest.Settings.Template = base64Template;
                        objFloodRequest.TransformData = null;

                        // Generate the main PDF
                        var originalPdfPath = GenerateDoc(objFloodRequest, mp.DocumentOrPackage.Name, mbi).GetAwaiter().GetResult();

                        if (string.IsNullOrWhiteSpace(originalPdfPath))
                        {
                            log.Error($"Initial PDF generation failed for MailPiece ID: {mp.Id}");
                            continue;
                        }

                        var state = objFloodRequest?.Data.PropState.Trim()?.ToUpperInvariant();
                        if (state == "MA" || state == "NY"
                           || (
                               (mailBatch.ClientBatchId.ToLower().Contains("capone_") || (mailBatch.ClientBatchId.ToLower().Contains("ct_clear_"))
                               && state == "CT")
                              )
                           )
                        {
                            try
                            {
                                var key = $"{docType.Id}_{state}";
                                var stateTemplate = Config.GetCustomTemplate(key);

                                if (string.IsNullOrWhiteSpace(stateTemplate))
                                {
                                    log.Warn($"No custom state template found for DocType {docType.Id} and state {state}");
                                    continue;
                                }

                                var disclosureTemplatePath = Path.Combine(Config.Monitor_CustomTemplateFolder, client.AppCode, stateTemplate);

                                if (!File.Exists(disclosureTemplatePath))
                                {
                                    log.Error($"Disclosure template not found at path: {disclosureTemplatePath}");
                                    continue;
                                }

                                var base64DisclosureTemplate = Convert.ToBase64String(File.ReadAllBytes(disclosureTemplatePath));
                                objFloodRequest.Settings.Template = base64DisclosureTemplate;

                                // Generate the disclosure PDF
                                var disclosureFileName = $"Disclosure_{mp.Id}_{state}.pdf";
                                var disclosurePdfPath = GenerateDoc(objFloodRequest, disclosureFileName, mbi).Result;

                                if (string.IsNullOrWhiteSpace(disclosurePdfPath))
                                {
                                    log.Error($"Disclosure PDF generation failed for MailPiece ID: {mp.Id}, state: {state}");
                                    continue;
                                }

                                // Merge disclosure + original into same file name
                                var tempMergedPath = Path.Combine(mbi.AMRequestPath, Path.GetRandomFileName() + ".pdf");
                                PdfUtil.MergePDFs(originalPdfPath, disclosurePdfPath, tempMergedPath);

                                File.Delete(originalPdfPath); // Delete original
                                if (mailBatch.ClientBatchId.ToLower().Contains("ct_clear_") && state == "CT")//Send only disclosure if input file is CT_CLEAR
                                {
                                    File.Move(disclosurePdfPath, originalPdfPath); // Overwrite with disclosure
                                }
                                else
                                {
                                    File.Move(tempMergedPath, originalPdfPath); // Overwrite with merged
                                }
                                File.Delete(disclosurePdfPath); // Delete disclosure PDF)

                                log.Info($"Merged and replaced original PDF at {originalPdfPath} for state {state}");
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Exception during disclosure PDF generation/merge for state {state}: {ex.Message}", ex);
                                continue;
                            }
                        }

                        // optional: update metadata or log result here
                    }

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now).Wait();
                }
                catch (Exception ex)
                {
                    var msg = $"Error occurred in DownloadBatches Method for Mail Batch Id: {mailBatch.Id} - Error: {ex.Message}";
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                }
            }

            log.Debug("Download job queue is completed.");
        }

        private async Task<string> GenerateDoc(dynamic objFloodRequest, string pdfFileName, MailBatchInfo batchInfo)
        {
            using (var httpClient = new HttpClient())
            using (var stringContent = new StringContent(JsonConvert.SerializeObject(objFloodRequest), Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage httpResponse = await httpClient.PostAsync(Config.Common_DocGen_API, stringContent);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                if (!httpResponse.IsSuccessStatusCode || httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    //await _commonService.UpdateLienDisqualifyReason(lienRecord.Id, responseString);
                    return null;
                }
                var docGenResponse = JsonConvert.DeserializeObject<DocGenResponse>(responseString);
                string finalPdfPath = Path.Combine(batchInfo.AMRequestPath, pdfFileName);

                File.WriteAllBytes(finalPdfPath, docGenResponse.DocumentData);
                return finalPdfPath;
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