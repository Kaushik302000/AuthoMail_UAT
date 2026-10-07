using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Data.SqlClient;
using AM.USPSLib;
using System.Transactions;
using System.Threading;

namespace AM.Console
{
    class AMCChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IAMCService amcSvc;

        private string channelType;

        private static XmlSchemaSet schemas;

        static AMCChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/2015/amc/v1/packages", XmlReader.Create(new StringReader(Resources.PackageInfoSchema)));
        }

        public AMCChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            amcSvc = GetService<AMCService>();
            this.channelType = channelType;
        }

        #region EnumerateBatches
        protected override void EnumerateBatches()
        {
            log.Debug("EnumerateBatches job queue is started.");

            if (SFTChannel.ServerIP.IsNullOrWhiteSpace() || Config.Common_ServerIP != SFTChannel.ServerIP)
            {
                log.Debug("System cannot process AMC channel on this server.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            List<string> triggerFilePaths = GetTriggerFilePaths(sftSite);

            foreach (string triggerFile in triggerFilePaths)
            {
                try
                {
                    string triggerFilePath = Path.Combine(SFTChannel.ToADEDir, triggerFile + "_Complete.go");
                    log.Debug("Trigger File {0} Processing Started.".Fill(triggerFile));
                    //Download and Insert the batch in database first always.
                    string batchName = triggerFile;

                    DateTime lastModifiedTime = Common.Now;
                    if (channelType == ChannelType.Trigger)
                    {
                        //TODO : Implement check for zip exist or not
                        lastModifiedTime = sftSite.LastModifiedTimeOfFile(triggerFilePath);
                    }

                    #region Create AMC Directory.
                    //Here first we are cleaning AMC folder
                    string amcDownloadPath = Path.Combine(Config.Common_AuthoMailRootFolder, SFTChannel.GetChannelCode());

                    if (Directory.Exists(amcDownloadPath))
                    {
                        Directory.Delete(amcDownloadPath, true);
                    }

                    //To ensure that folder gets created.
                    Thread.Sleep(1000);
                    Directory.CreateDirectory(amcDownloadPath);

                    #endregion

                    #region Download Zip
                    string zipFileName = "{0}.zip".Fill(batchName);
                    string localZipFilePath = Path.Combine(amcDownloadPath, zipFileName);
                    string xmlFilePath = Path.Combine(amcDownloadPath, batchName + ".xml");
                    string ftpZipFilePath = Path.Combine(SFTChannel.ToADEDir, zipFileName);
                    string ftpTriggerFilePath = Path.Combine(SFTChannel.ToADEDir, "{0}_Complete.go".Fill(batchName));

                    if (!sftSite.IsFileAvailable(ftpZipFilePath))
                    {
                        var msg = "{0}.zip file is missing on SFT site.".Fill(batchName);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "", msg);
                        continue;
                    }

                    //Some time it is not creating AMC folder in flow so here we are again checking this if not created then below code will create
                    if (!Directory.Exists(amcDownloadPath))
                    {
                        log.Error("AMC Folder was not created as expected.");
                        Directory.CreateDirectory(amcDownloadPath);
                    }

                    DownloadBatchZipFile(sftSite, ftpZipFilePath, localZipFilePath);

                    try
                    {
                        ZipFile.ExtractToDirectory(localZipFilePath, Path.GetDirectoryName(localZipFilePath));
                    }
                    catch (Exception ex)
                    {
                        var msg = $"Unable to extract '{batchName}.zip' file with error message {ex.Message}";
                        log.Error(msg, ex);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localZipFilePath, msg);
                        continue;
                    }
                    #endregion

                    SeparateAMCBatchClientWise(xmlFilePath, localZipFilePath, batchName, amcDownloadPath, lastModifiedTime, sftSite);

                    log.Debug("Trigger File {0} Processing Completed.".Fill(triggerFile));
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in EnumerateBatches Method For BatchName: {0} with Error : {1}".Fill(triggerFile, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();
        }

        private MailBatchDTO InsertInvalidBatchInfo(string batchName, DateTime receivedOn, string errorMessage)
        {
            MailBatchDTO mailBatch = new MailBatchDTO
            {
                UniqueId = Guid.NewGuid(),
                ClientId = GetInvalidClient().Id,
                SFTChannelId = SFTChannel.Id,
                BatchName = batchName,
                ClientBatchId = batchName,
                Priority = Priority.Medium.ToString(),
                ServerIP = Config.Common_ServerIP
            };
            mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);
            commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, receivedOn).Wait();
            mailBatchSvc.MarkMailBatchAsInvalid(mailBatch.Id, errorMessage).Wait();
            commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.Failed.ToString()).Wait();
            return mailBatch;
        }

        private void SeparateAMCBatchClientWise(string xmlFilePath, string localZipFilePath, string batchName, string amcDownloadPath, DateTime receivedOn, SFTUtil sftSite)
        {
            List<ClientAMCCustIdMap> amcClientMaps = clientSvc.GetAMCClientIdMaps();
            List<MailBatchDTO> mailBatches = new List<MailBatchDTO>();

            XmlDocument batchXML = new XmlDocument();
            XmlNamespaceManager nsManager = null;
            XmlNodeList mailPieceNodes = null;
            try
            {
                batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                nsManager = new XmlNamespaceManager(batchXML.NameTable);
                nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
                mailPieceNodes = batchXML.DocumentElement.ChildNodes;
            }
            catch (XmlException ex)
            {
                var msg = $"{batchName}.xml is not a valid XML document.";
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOn, msg), localZipFilePath, msg);
                return;
            }
            catch (Exception ex)
            {
                var msg = $"{batchName}.xml is not a valid XML document.";
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOn, msg), localZipFilePath, msg);
                return;
            }

            //Here we are caching all clients details 
            var clients = new List<Client>();

            try
            {
                Dictionary<string, List<XmlNode>> inputXmlByCustId = new Dictionary<string, List<XmlNode>>();
                Dictionary<string, List<XmlNode>> inputXmlByEDelivery = new Dictionary<string, List<XmlNode>>();

                foreach (XmlNode node in batchXML.DocumentElement.SelectNodes("//x:PackageInfo", nsManager))
                {
                    var custIdNode = node.SelectSingleNode("x:CustomerIdentifier", nsManager);

                    string custId = custIdNode == null || !amcClientMaps.Any(c => c.AMCCustId == custIdNode.InnerXml) ? "InvalidCustId" : custIdNode.InnerXml;


                    var eDeliveryNode = node.SelectSingleNode("x:eDelivery", nsManager);

                    if (eDeliveryNode != null)
                    {
                        #region EDelivery Batch Separated
                        if (!inputXmlByEDelivery.ContainsKey(custId))
                        {
                            inputXmlByEDelivery.Add(custId, new List<XmlNode>());
                        }

                        inputXmlByEDelivery[custId].Add(node);
                        #endregion
                    }
                    else
                    {
                        #region PrintAndShip Batch
                        if (!inputXmlByCustId.ContainsKey(custId))
                        {
                            inputXmlByCustId.Add(custId, new List<XmlNode>());
                        }

                        inputXmlByCustId[custId].Add(node);
                        #endregion
                    }
                }

                foreach (var map in amcClientMaps)
                {
                    if (!inputXmlByCustId.ContainsKey(map.AMCCustId))
                        continue;

                    Client client = clientSvc.GetClientById(map.ClientId);

                    if (!clients.Select(cl => cl.Id).Contains(client.Id))
                        clients.Add(client);

                    var packages = inputXmlByCustId[map.AMCCustId];

                    if (packages.Count > 0)
                    {
                        Guid batchUniqueId = Guid.NewGuid();
                        SeparateXMLsAndPDFsClientWise(packages, nsManager, amcDownloadPath, client.AppCode, batchName, batchUniqueId);

                        MailBatchDTO mailBatch = new MailBatchDTO
                        {
                            ClientId = client.Id,
                            BatchName = batchName,
                            SFTChannelId = SFTChannel.Id,
                            ClientBatchId = batchName,
                            UniqueId = batchUniqueId,
                            Priority = Priority.Medium.ToString(),
                            ReceivedOn = receivedOn,
                            PipelineStatus = BatchPipelineStatus.Received,
                            BatchStatus = BatchStatus.InProgress.ToString(),
                            IsReturnInfoSpecified = false,
                            ServerIP = Config.Common_ServerIP
                        };
                        mailBatches.Add(mailBatch);
                    }
                }

                foreach (var map in amcClientMaps)
                {
                    if (!inputXmlByEDelivery.ContainsKey(map.AMCCustId))
                        continue;

                    Client client = clientSvc.GetClientById(map.ClientId);

                    if (!clients.Select(cl => cl.Id).Contains(client.Id))
                        clients.Add(client);

                    var packages = inputXmlByEDelivery[map.AMCCustId];

                    if (packages != null)
                    {
                        foreach (XmlNode package in packages)
                        {
                            Guid batchUniqueId = Guid.NewGuid();
                            //Folder Structure will be like : {AMCDownloadPath}/{ClientCode}/{BatchUniqueId}
                            SeparateEDXMLsAndPDFsPackageWise(package, nsManager, amcDownloadPath, client.AppCode, batchName, batchUniqueId);

                            MailBatchDTO mailBatch = new MailBatchDTO
                            {
                                ClientId = client.Id,
                                BatchName = batchName,
                                SFTChannelId = SFTChannel.Id,
                                ClientBatchId = batchName,
                                UniqueId = batchUniqueId,
                                Priority = Priority.Medium.ToString(),
                                ReceivedOn = receivedOn,
                                PipelineStatus = BatchPipelineStatus.Received,
                                BatchStatus = BatchStatus.InProgress.ToString(),
                                IsReturnInfoSpecified = false,
                                IsEDeliveryBatch = true,
                                ServerIP = Config.Common_ServerIP
                            };
                            mailBatches.Add(mailBatch);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "Error occured while separating AMC batches from Batch '{0}.xml'".Fill(batchName);
                log.Error(msg, ex);
                MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                return;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    mailBatches = mailBatchSvc.InsertBatchDetails(mailBatches);

                    foreach (var clientBatch in mailBatches)
                    {
                        Client client = clients.Single(c => c.Id == clientBatch.ClientId);
                        MailBatchInfo mbi = new MailBatchInfo(clientBatch, client, Config.Common_AuthoMailRootFolder);

                        //Here we create folder structure
                        //mbi.AMRequestPath.CreateDirectoryIfNotExist();
                        mbi.AMResponsePath.CreateDirectoryIfNotExist();
                        mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                        mbi.PnSResponsePath.CreateDirectoryIfNotExist();

                        Directory.Move(Path.Combine(amcDownloadPath, client.AppCode, clientBatch.UniqueId.ToString()), mbi.AMRequestPath);

                        commonSvc.UpdateBatchPipeLineStatus(clientBatch.Id, BatchPipelineStatus.Downloaded, Common.Now).Wait();
                    }

                    //Here after moving all batches to particular client location , archive amc batch
                    string archiveZipPath = Path.Combine(Config.Common_AuthoMailRootFolder, "Archive", batchName + ".zip");

                    if (File.Exists(archiveZipPath))
                        File.Delete(archiveZipPath);

                    File.Move(localZipFilePath, archiveZipPath);

                    //Now delete/archive trigger and zip from SFTP
                    MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + ".zip"));
                    MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + "_Complete.go"));

                    //After complete process clean AMC folder
                    amcDownloadPath.CleanDirectory();

                    scope.Complete();

                }
                catch (Exception ex)
                {
                    var msg = $"Error occured while inserting AMC batches details for Batch {batchName}";
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                }
            }
        }

        private void SeparateEDXMLsAndPDFsPackageWise(XmlNode package, XmlNamespaceManager nsManager, string amcDownloadPath, string clientCode, string batchName, Guid batchUniqueId)
        {
            string clientBatchDirectory = Path.Combine(amcDownloadPath, clientCode, batchUniqueId.ToString());

            clientBatchDirectory.CleanDirectory();

            string amcFilePath = Path.Combine(clientBatchDirectory, string.Format("{0}.xml", batchName));

            string reqRootXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><PackageRequests xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.corelogic.com/2015/amc/v1/packages\">" + Environment.NewLine + "{0}" + Environment.NewLine + "</PackageRequests>";

            var packageInfoNodes = string.Join(Environment.NewLine, package.OuterXml);


            var fileNameAttr = package.SelectSingleNode("x:File", nsManager);
            string fileName = fileNameAttr == null ? string.Empty : fileNameAttr.InnerXml;

            if (!string.IsNullOrEmpty(fileName) && File.Exists(Path.Combine(amcDownloadPath, fileName)))
            {
                string sourceFile = Path.Combine(amcDownloadPath, fileName);
                string destFile = Path.Combine(clientBatchDirectory, fileName);
                File.Copy(sourceFile, destFile, true);
            }

            File.WriteAllText(amcFilePath, reqRootXml.Fill(packageInfoNodes));
        }

        private Client GetInvalidClient()
        {
            return clientSvc.GetClientByAppCode("AMCInvalid").Result;
        }

        private void SeparateXMLsAndPDFsClientWise(List<XmlNode> packages, XmlNamespaceManager nsManager, string amcDownloadPath, string clientCode, string batchName, Guid batchUniqueId)
        {
            string clientBatchDirectory = Path.Combine(amcDownloadPath, clientCode, batchUniqueId.ToString());

            clientBatchDirectory.CleanDirectory();

            string amcFilePath = Path.Combine(clientBatchDirectory, string.Format("{0}.xml", batchName));

            string reqRootXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><PackageRequests xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.corelogic.com/2015/amc/v1/packages\">" + Environment.NewLine + "{0}" + Environment.NewLine + "</PackageRequests>";

            var packageInfoNodes = string.Join(Environment.NewLine, packages.Select(nodes => nodes.OuterXml));

            foreach (var package in packages)
            {
                var fileNameAttr = package.SelectSingleNode("x:File", nsManager);
                string fileName = fileNameAttr == null ? string.Empty : fileNameAttr.InnerXml;

                if (!string.IsNullOrEmpty(fileName) && File.Exists(Path.Combine(amcDownloadPath, fileName)))
                {
                    string sourceFile = Path.Combine(amcDownloadPath, fileName);
                    string destFile = Path.Combine(clientBatchDirectory, fileName);
                    File.Move(sourceFile, destFile);
                }
            }

            File.WriteAllText(amcFilePath, reqRootXml.Fill(packageInfoNodes));
        }

        private void MoveBatchToBackupDir(SFTUtil sftSite, string ftpFilePath)
        {
            int cnt = 0;
            Exception exception = null;
            while (cnt < 5)
            {
                try
                {
                    log.Debug($"AM is trying to remove File {Path.GetFileName(ftpFilePath)} from SFTP.");
                    string ftpBackUpFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(ftpFilePath));

                    if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                    {
                        sftSite.MoveFile(ftpFilePath, ftpBackUpFilePath);
                    }
                    else
                        sftSite.DeleteFile(ftpFilePath);

                    log.Debug($"AM has completed to remove File {Path.GetFileName(ftpFilePath)} from SFTP.");
                    exception = null;
                    break;
                }
                catch (Exception ex)
                {
                    var msg = $"Error occured while moving or removing file {Path.GetFileName(ftpFilePath)} from SFTP. System is trying for {cnt} time.";
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                    cnt++;
                    exception = ex;
                }
            }

            if (exception != null)
            {
                throw exception;
            }
        }

        private List<string> GetTriggerFilePaths(SFTUtil sftSite)
        {
            var regex = new Regex(@"^(\w*)_Complete.go$", RegexOptions.IgnoreCase);

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableTriggerFilePaths != null && availableTriggerFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} trigger file(s) found.", availableTriggerFilePaths.Count));
                log.Debug(string.Format("{0} trigger file(s) found.", string.Join(",", availableTriggerFilePaths)));
            }
            else
            {
                log.Error("No trigger files found hence no batches are available to download.");
            }

            return availableTriggerFilePaths;
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string localZipFilePath)
        {
            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            return localZipFilePath;
        }
        #endregion

        #region DownloadBatches
        protected override void DownloadBatches()
        {
        }

        #endregion

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string localZipFilePath, string errorMessage)
        {
            Client client = clientSvc.GetClientById(mailBatch.ClientId);

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();

            if (!string.IsNullOrEmpty(localZipFilePath))
                File.Move(localZipFilePath, Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".zip"));

            //For Invalid Batch, we will generate err file 
            string invalidBatchFileName = "{0}_InvalidZipfile.err".Fill(mailBatch.BatchName);

            string invalidBatchPath = Path.Combine(mbi.AMResponsePath, invalidBatchFileName);

            File.WriteAllText(invalidBatchPath, errorMessage);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);

            log.Debug($"Checking for SFTP File {mbi.BatchName}.zip");
            if (sftSite.IsFileAvailable(Path.Combine(SFTChannel.ToADEDir, mbi.BatchName + ".zip")))
            {
                log.Info($"{mbi.BatchName} is available with last modified time {sftSite.LastModifiedTimeOfFile(Path.Combine(SFTChannel.ToADEDir, mbi.BatchName + ".zip"))}");
                log.Debug($"Removing Process Started for SFTP File {mbi.BatchName}.zip");
                MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, mbi.BatchName + ".zip"));
                log.Debug($"Removing Process Completed for SFTP File {mbi.BatchName}.zip");
            }
            log.Debug($"Removing Process Started for SFTP File {mbi.BatchName}_Complete.go");
            MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, mbi.BatchName + "_Complete.go"));
            log.Debug($"Removing Process Started for SFTP File {mbi.BatchName}_Complete.go");
            if (sftSite.IsConnected)
                sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();
        }

        protected override void ValidateBatches()
        {
            log.Debug("Validate job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Downloaded, SFTChannel.Id);
            if (downloadBatches == null)
            {
                log.Debug("Validate job queue is empty.");
                return;
            }

            foreach (var mailBatch in downloadBatches)
            {
                try
                {
                    //NOTE: Here system will delete if any existing records are there with MailBatchId in mail pieces table
                    mailBatchSvc.DeleteMailPiecesDetailsByMailBatchId(mailBatch.Id);

                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    //TODO: Can we have multiple business identifier in client business?
                    ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.CheckBusinessIdNameAlreadyExist(client.Id, "Loan Number");

                    if (clientBusinessIdDetails == null)
                    {
                        log.Error("Business identifier is not assigned to client : {0}".Fill(client.Name));

                        //TODO: Ask to Ravi Sir if no data found then how to insert data and which value
                        continue;
                    }

                    var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

                    XmlDocument batchXML = new XmlDocument();
                    XmlNamespaceManager nsManager = null;
                    XmlNodeList mailPieceNodes = null;
                    int totalNodes = 0;
                    try
                    {
                        batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                        nsManager = new XmlNamespaceManager(batchXML.NameTable);
                        nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
                        mailPieceNodes = batchXML.DocumentElement.ChildNodes;
                        totalNodes = mailPieceNodes.Count;
                    }
                    catch (XmlException ex)
                    {
                        var msg = $"{mbi.BatchName}.xml is not a valid XML document.";
                        log.Error(msg, ex);

                        ReportInvalidBatch(mailBatch, "", msg);

                        continue;
                    }
                    catch (Exception ex)
                    {
                        var msg = $"{mbi.BatchName}.xml is not a valid XML document.";
                        log.Error(msg, ex);
                        ReportInvalidBatch(mailBatch, "", msg);
                        continue;
                    }



                    foreach (XmlNode mailPieceNode in mailPieceNodes)
                    {
                        MailPieceDTO mailPieceDTO = new MailPieceDTO();
                        MailPieceCustomInfo customInfo = null;
                        List<MailPieceCustomInfo> mailPieceCustomInfo = new List<MailPieceCustomInfo>();

                        mailPieceDTO.ClientId = client.Id;
                        mailPieceDTO.MailBatchId = mbi.MailBatch.Id;
                        mailPieceDTO.ShippingStatus = ShippingStatus.InProgress.ToString();

                        try
                        {
                            #region Reading nodes using XPath
                            mailPieceDTO.BusinessFriendlyIdName = clientBusinessIdDetails.BusinessIdName;
                            mailPieceDTO.BusinessFriendlyIdValue = mailPieceNode.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                            mailPieceDTO.DocumentOrPackageType = mailPieceNode.SelectSingleNode("./x:PackageName/text()", nsManager)?.Value;
                            mailPieceDTO.UniqueId = mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager) == null ? Guid.Empty : Guid.Parse(mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager).Value);
                            mailPieceDTO.FileName = mailPieceNode.SelectSingleNode("./x:File/text()", nsManager)?.Value;

                            var recipientElement = mailPieceNode.SelectSingleNode("./x:RecipientInfo", nsManager);
                            if (recipientElement != null)
                            {
                                if (recipientElement.SelectNodes("./x:Recipient", nsManager) != null)
                                {
                                    mailPieceDTO.IsRcptInfoSpecified = true;
                                    var recipientsData = recipientElement.SelectNodes("./x:Recipient", nsManager);

                                    if (recipientsData.Count > 1)
                                    {
                                        mailPieceDTO.RcptFirstName = recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager)?.Value;
                                        mailPieceDTO.RcptLastName = recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager)?.Value;
                                        mailPieceDTO.CoRcptFirstName = recipientsData.Item(1).SelectSingleNode("./x:FirstName/text()", nsManager)?.Value;
                                        mailPieceDTO.CoRcptLastName = recipientsData.Item(1).SelectSingleNode("./x:LastName/text()", nsManager)?.Value;
                                    }
                                    else
                                    {
                                        mailPieceDTO.RcptFirstName = recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager)?.Value;
                                        mailPieceDTO.RcptLastName = recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager)?.Value;
                                    }
                                }
                                else
                                    mailPieceDTO.IsRcptInfoSpecified = false;

                                mailPieceDTO.RcptPhone = recipientElement.SelectSingleNode("./x:RecipientPhoneNo/text()", nsManager)?.Value;
                                mailPieceDTO.RcptEmailId = recipientElement.SelectSingleNode("./x:RecipientEmailId/text()", nsManager)?.Value;

                                var recipeintAddressElement = recipientElement.SelectSingleNode("./x:Address", nsManager);

                                if (recipeintAddressElement != null)
                                {
                                    mailPieceDTO.RcptAddressStreet = recipeintAddressElement.SelectSingleNode("./x:Line1/text()", nsManager)?.Value;

                                    mailPieceDTO.RcptAddrOptionalLine1BelowStreet = recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager)?.Value;

                                    mailPieceDTO.RcptAddressCity = recipeintAddressElement.SelectSingleNode("./x:City/text()", nsManager)?.Value;

                                    mailPieceDTO.RcptAddressState = recipeintAddressElement.SelectSingleNode("./x:State/text()", nsManager)?.Value;

                                    mailPieceDTO.RcptAddressZIP = recipeintAddressElement.SelectSingleNode("./x:Zip/text()", nsManager)?.Value;
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
                                mailPieceDTO.RetRcptFirstName = returnInfoElement.SelectSingleNode("./x:FirstName/text()", nsManager)?.Value;
                                mailPieceDTO.RetRcptLastName = returnInfoElement.SelectSingleNode("./x:LastName/text()", nsManager)?.Value;
                                mailPieceDTO.RetRcptPhone = returnInfoElement.SelectSingleNode("./x:ReturnPhoneNo/text()", nsManager)?.Value;

                                var returnAddressElement = returnInfoElement.SelectSingleNode("./x:Address", nsManager);
                                if (returnAddressElement != null)
                                {
                                    mailPieceDTO.RetRcptAddressStreet = returnAddressElement.SelectSingleNode("./x:Line1/text()", nsManager)?.Value;
                                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = returnAddressElement.SelectSingleNode("./x:Line2/text()", nsManager)?.Value;
                                    mailPieceDTO.RetRcptAddressCity = returnAddressElement.SelectSingleNode("./x:City/text()", nsManager)?.Value;
                                    mailPieceDTO.RetRcptAddressState = returnAddressElement.SelectSingleNode("./x:State/text()", nsManager)?.Value;
                                    mailPieceDTO.RetRcptAddressZIP = returnAddressElement.SelectSingleNode("./x:Zip/text()", nsManager)?.Value;
                                }
                            }
                            else
                                mailPieceDTO.IsReturnInfoSpecified = false;

                            var eDeliveryElement = mailPieceNode.SelectSingleNode("./x:eDelivery", nsManager);
                            if (eDeliveryElement != null)
                            {
                                mailPieceDTO.IsEDeliveryRequested = true;
                                mailPieceDTO.IsFallBackRequested = Convert.ToBoolean(eDeliveryElement.Attributes["PrintAndShipOnExpiry"] == null ? "false" : eDeliveryElement.Attributes["PrintAndShipOnExpiry"].Value);
                                if (eDeliveryElement.Attributes["ExpiryDate"] == null || eDeliveryElement.Attributes["ExpiryDate"].Value == "")
                                {
                                    if (client.SigningRoomExpiryDays >= 0)
                                    {
                                        mailPieceDTO.ExpiryDate = GetDefaultExpiryDate(mailBatch.ReceivedOn.Value.UtcToEst(), client.SigningRoomExpiryDays).ToEndDateTime().EstToUtc();
                                    }
                                    else
                                    {
                                        mailPieceDTO.ExpiryDate = null;
                                    }
                                }
                                else
                                {
                                    mailPieceDTO.ExpiryDate = Convert.ToDateTime(eDeliveryElement.Attributes["ExpiryDate"].Value).ToEndDateTime().EstToUtc();
                                }
                            }
                            else
                            {
                                mailPieceDTO.IsEDeliveryRequested = false;
                                mailPieceDTO.IsFallBackRequested = false;
                                mailPieceDTO.ExpiryDate = null;
                            }

                            if (mailPieceNode.SelectSingleNode("./x:ShippingMethod", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingMethod", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:ShippingMethod/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:ShippingMethod/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:CustomerIdentifier", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "CustomerIdentifier", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:CustomerIdentifier/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:CustomerIdentifier/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:RequestId", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "RequestId", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:RequestId/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:RequestId/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:ReceivedDate", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ReceivedDate", DataType = DataType.DateTime, Value = mailPieceNode.SelectSingleNode("./x:ReceivedDate/text()", nsManager)?.Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            var shippingCredElement = mailPieceNode.SelectSingleNode("./x:ShippingCredentials", nsManager);
                            if (shippingCredElement != null)
                            {
                                if (shippingCredElement.SelectSingleNode("./x:ClientCode", nsManager) != null)
                                {
                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingCredCode", DataType = DataType.String, Value = shippingCredElement.SelectSingleNode("./x:ClientCode/text()", nsManager) == null ? string.Empty : shippingCredElement.SelectSingleNode("./x:ClientCode/text()", nsManager).Value };

                                    mailPieceCustomInfo.Add(customInfo);
                                }
                            }

                            #endregion

                            MailPieceValidationResult validationResult = ValidatePackgeInfoNode(mailPieceNode);
                            if (validationResult.Code == OutComeCodes.No_Error)
                            {
                                string localFilePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);

                                if (!File.Exists(localFilePath))
                                {
                                    mailPieceDTO.ErrorCode = OutComeCodes.Missing_PDF_File;
                                    mailPieceDTO.ErrorDesc = "Pdf file is not available for this package.";
                                    mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                }
                                else
                                {
                                    int pageCount = 0;
                                    List<int> blankPageNos = new List<int>();

                                    using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(localFilePath))
                                    {
                                        pageCount = reader.NumberOfPages;
                                        mailPieceDTO.TotalPages = pageCount; // Note : system will not store this in AM.MailPieces table but store in EDMailPieces table

                                        //check if pdf is blank or not
                                        blankPageNos = Channel.CheckIfPdfBlank(pageCount, blankPageNos, localFilePath, reader);
                                    }

                                    //NOTE : Here system is storing file size of received pdf in KB
                                    FileInfo fileInfo = new FileInfo(localFilePath);
                                    mailPieceDTO.PDFFileSize = (fileInfo.Length / 1024);

                                    string documentOrPackageType = mailPieceDTO.DocumentOrPackageType;


                                    if (!mailPieceDTO.IsEDeliveryRequested || mailPieceDTO.IsFallBackRequested)
                                    {
                                        //Document Or Package Type validation
                                        var tempDocType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType, pageCount, mbi.MailBatch.ReceivedOn.Value.UtcToEst());

                                        if (tempDocType != null)
                                        {
                                            mailPieceDTO.DocTypeId = tempDocType.Id;
                                            mailPieceDTO.PnSConfigDetailId = tempDocType.PnSConfigDetailId;
                                            mailPieceDTO.PrintTrailId = tempDocType.PrintTrailId;
                                        }

                                        if (tempDocType == null)
                                        {
                                            mailPieceDTO.ErrorCode = OutComeCodes.Invalid_PackageType;
                                            mailPieceDTO.ErrorDesc = client.AppCode == GetInvalidClient().AppCode ? "The specified customer identifier {0} is not configured.".Fill(mailPieceCustomInfo.Single(c => c.Key == "CustomerIdentifier").Value) : "Either the specified document or package type '{0}' is not configured or no matching print configuration found for page count ({1}).".Fill(mailPieceDTO.DocumentOrPackageType, mailPieceDTO.TotalPages);
                                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                        }
                                        else if (pageCount == blankPageNos.Count())
                                        {
                                            mailPieceDTO.ErrorCode = OutComeCodes.Invalid_PDF_File;
                                            mailPieceDTO.ErrorDesc = "All pages are blank in received pdf file.";
                                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                            mailPieceDTO.TotalBlankPages = blankPageNos.Count;
                                            mailPieceDTO.BlankPageNumbers = string.Join(",", blankPageNos);
                                        }
                                        else if (!mailPieceDTO.IsReturnInfoSpecified && !mailBatch.IsReturnInfoSpecified && !tempDocType.IsReturnInfoSpecified)
                                        {
                                            mailPieceDTO.ErrorCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
                                            mailPieceDTO.ErrorDesc = "Return mailing address is not specified for this package.";
                                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                        }
                                        else
                                        {
                                            mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                            mailPieceDTO.TotalBlankPages = blankPageNos.Count;
                                            mailPieceDTO.BlankPageNumbers = string.Join(",", blankPageNos);
                                            if (!mailPieceDTO.IsReturnInfoSpecified && !mailBatch.IsReturnInfoSpecified && tempDocType.IsReturnInfoSpecified)
                                            {
                                                mailPieceDTO.RetRcptFirstName = tempDocType.RetRcptFirstName;

                                                mailPieceDTO.RetRcptLastName = tempDocType.RetRcptLastName;

                                                mailPieceDTO.RetRcptPhone = tempDocType.RetRcptPhone;

                                                mailPieceDTO.RetRcptAddressStreet = tempDocType.RetRcptAddressStreet;

                                                mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = tempDocType.RetRcptAddrOptionalLine1BelowStreet;

                                                mailPieceDTO.RetRcptAddressCity = tempDocType.RetRcptAddressCity;

                                                mailPieceDTO.RetRcptAddressState = tempDocType.RetRcptAddressState;

                                                mailPieceDTO.RetRcptAddressZIP = tempDocType.RetRcptAddressZIP;
                                            }
                                        }
                                    }

                                    if (mailPieceDTO.IsEDeliveryRequested && mailPieceDTO.ShippingStatus != ShippingStatus.Error.ToString())
                                    {
                                        //NOTE : System will check for E-Delivery and set E-Delivery as a Document type even though requested for FallBack
                                        var docType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, "E-Delivery", pageCount, mbi.MailBatch.ReceivedOn.Value.UtcToEst());

                                        if (docType != null)
                                        {
                                            mailPieceDTO.DocTypeId = docType.Id;
                                            mailPieceDTO.PnSConfigDetailId = docType.PnSConfigDetailId;
                                        }

                                        if (docType == null)
                                        {
                                            mailPieceDTO.ErrorCode = OutComeCodes.Invalid_PackageType;
                                            mailPieceDTO.ErrorDesc = "Client has not opted for E-Delivery service.".Fill(mailPieceDTO.DocumentOrPackageType);
                                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                        }
                                        else if (string.IsNullOrWhiteSpace(mailPieceDTO.RcptEmailId) && mailPieceDTO.IsEDeliveryRequested)//Here system will validate if EDelivery is requested then EmailId is required, if not exist then error out
                                        {
                                            mailPieceDTO.ErrorCode = OutComeCodes.Recipient_Details_Missing_Or_Invalid;
                                            mailPieceDTO.ErrorDesc = "Recipient Email address is not specified for E-Delivery request.";
                                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                        }
                                        else if (mailPieceDTO.ExpiryDate == null && mailPieceDTO.IsEDeliveryRequested)//Here system will validate if EDelivery is requested then Expiry date is required, if not exist then error out
                                        {
                                            mailPieceDTO.ErrorCode = OutComeCodes.Issue_In_EDelivery;
                                            mailPieceDTO.ErrorDesc = "Client has not provided expiry date or expiry days while creating signing room.";
                                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                        }
                                        else
                                        {
                                            mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                            mailPieceDTO.ErrorDesc = null;
                                            mailPieceDTO.ShippingStatus = ShippingStatus.InProgress.ToString();
                                        }
                                    }
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
                            var msg = $"Unable to process package info node for mail batch {mbi.BatchName}";

                            log.Error(msg, ex);

                            mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                            mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                        }

                        try
                        {
                            mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo, mailBatch.ReceivedOn.Value, mailBatch.DownloadedOn.Value);
                        }
                        catch (SqlException ex)
                        {
                            log.Error("Error occcured while inserting MailPiece details :", ex);
                            MailUtility.SendUnexpectedExceptionEmail("Error occcured while inserting MailPiece details :" + ex.Message);
                            //2601 error code used for duplicate unique key insert 
                            if (ex.Number == 2601)
                            {
                                var msg = "{0} key id is duplicate in package info node.".Fill(mailPieceDTO.UniqueId);

                                log.Error(msg, ex);
                                mailPieceDTO.UniqueId = Guid.NewGuid();
                                mailPieceDTO.ErrorCode = OutComeCodes.Invalid_DuplicateKey;
                                mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo);
                            }

                            if (ex.Number == -2)
                            {
                                //If Timeout exception occured while processing batch then we are re-processing whole batch completly and breaking out from foreach loop.
                                break;
                            }
                        }
                    }

                    //TODO: Do we need to check all mail pieces are valid, partially valid or failed and update batch status according to that?

                    var mailPieceData = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                    if (mailPieceData != null)
                    {
                        var validMailPieces = mailPieceData.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                        //Here System will validate Addresses for valid mail pieces
                        if (validMailPieces.Count > 0)
                            Channel.ValidateAddressType(mailBatch, validMailPieces);

                        validMailPieces = validMailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                        if (validMailPieces.Count == 0)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                            mbi.MailBatch.ValidationErrors = "Xml does not contain any valid package info nodes.";
                            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString()).Wait();
                        }
                        else if (validMailPieces.Count < mailPieceData.Count)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.PartiallyValid.ToString();
                        }
                        else
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Valid.ToString();
                        }

                        commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, mbi.MailBatch.ValidationStatus, mbi.MailBatch.ValidationErrors);
                    }
                    else
                    {
                        MailUtility.SendUnexpectedExceptionEmail("Mail Pieces list is null for Batch-Id {0}".Fill(mailBatch.Id));
                    }

                    if (totalNodes != mailPieceData.Count)
                    {
                        throw new Exception("Mail pieces count and received xml count is mismatched for Batch-Id : {0}<br/>".Fill(mailBatch.Id) + "Mail Piece count is {0} and received xml nodes count is {1}".Fill(mailPieceData.Count, totalNodes));
                    }

                    Channel.CalculateDueDateAndPrintLocation(client, mailBatch, mailPieceData);

                    if (mailPieceData.All(mp => mp.IsEDeliveryRequested && mp.ShippingStatus != ShippingStatus.Error.ToString()))
                    {
                        var validationTiming = Common.Now;
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.SubmittedToEDelivery, validationTiming).Wait();
                        commonSvc.UpdateEDeliveryPipelineStatus(mbi.MailBatch.Id, EDPipelineStatus.Validated, validationTiming);
                    }
                    else
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now).Wait();
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in ValidateBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                }
            }
            log.Debug("Validate job queue is completed.");
        }

        private DateTime GetDefaultExpiryDate(DateTime receivedOnEst, int expiryDays)
        {
            DateTime expiryDate = receivedOnEst.AddDays(expiryDays);

            if (!commonSvc.IsBusinessDay(expiryDate, Config.Common_FederalHolidays, false, false))
            {
                expiryDate = commonSvc.GetNextWorkingDay(receivedOnEst.UtcToEst(), Config.Common_FederalHolidays, false, false);
            }

            return expiryDate;
        }

        protected override void AcknowledgeBatches()
        {
            log.Debug("Acknowledge job queue is started.");

            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Validated, SFTChannel.Id);

            if (validatedBatches == null)
            {
                log.Debug("Acknowledge job queue is empty.");
                return;
            }

            foreach (var mailBatch in validatedBatches)
            {
                try
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    List<MailPieces> mailPieces;
                    using (IReportService reportSvc = new ReportService())
                    {
                        mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                    }
                    if (mailPieces != null)
                    {
                        try
                        {
                            //Here we are doing generic flow for AMC.
                            var emailErrorReportPath = UploadBaseReportToAMC(mbi, mailPieces, deliverable);

                            MailUtility.SendDownloadNotificationEmail(mbi, mailPieces, emailErrorReportPath, deliverable.EmailChannel);

                            //NOTE : After Sending an Email change extention only if partially valid because when system generates final reports then it will not upload duplicate records
                            if (File.Exists(emailErrorReportPath) && mbi.MailBatch.ValidationStatus != BatchValidationStatus.Invalid)
                                File.Move(emailErrorReportPath, Path.ChangeExtension(emailErrorReportPath, ".Uploaded"));

                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured while acknowleding for Batch-Id : {0}".Fill(mbi.BatchName);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedExceptionEmail(msg + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in AcknowledgeBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("Acknowledge job queue is completed.");
        }

        private string UploadBaseReportToAMC(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            using (ICommonService commonSvc = new CommonService())
            {
                string errorReportFileName = $"Error-{mbi.BatchName}-{mbi.Client.AppCode}-{commonSvc.GetReportingSequence()}-{Common.Now.ToString("yyyyMMdd-HHmmss")}.txt";

                string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                StringBuilder errorData = amcSvc.AMCErrorReport(mailPieces);
                if (errorData != null)
                {
                    File.WriteAllText(errorReportPath, errorData.ToString());
                }
                else
                    errorReportPath = "";

                if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.Invalid)
                {
                    // Batch status Report Prepped
                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

                    //NOTE: Inserting Error Notification in this because system is not going to update any further status for batch due to full batch is invalid
                    commonSvc.InsertErrorNotifications(mbi.MailBatch.Id);
                    MailUtility.SendSubscribedErrorNotificationEmail(mbi, mailPieces, commonSvc.GetErrorNotificationEmailSubscriberList(mbi.Client.Id));
                }
                else if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.PartiallyValid)
                {
                    string errorGoFileName = "ErrReport.go";
                    string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);
                    string ftpErrorGoUploadPath = Path.Combine(deliverable.UploadPath, errorGoFileName);

                    SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                    sftSite.Connect();

                    var ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportFileName);

                    sftSite.UploadFile(errorReportPath, ftpUploadPath);

                    // Upload Error.GO file after uploading error report
                    File.WriteAllText(errorGoFilePath, string.Empty);

                    sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);

                    if (sftSite.IsConnected)
                        sftSite.Disconnect();

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now).Wait();
                }
                else
                {
                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now).Wait();
                }
                return errorReportPath;
            }
        }

        private MailPieceValidationResult ValidatePackgeInfoNode(XmlNode xmlPiece)
        {
            var errors = GetAllPackageInfoErrors(xmlPiece).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private IEnumerable<MailPieceValidationResult> GetAllPackageInfoErrors(XmlNode xmlPiece)
        {
            XDocument xDoc = XDocument.Parse(xmlPiece.OuterXml);
            StringBuilder sbErrorMessage = new StringBuilder();

            xDoc.Validate(schemas, (o, e) =>
            {
                sbErrorMessage.Append(e.Message);
            });

            string errorMsg = sbErrorMessage.ToString();

            if (errorMsg.Contains("Address") || errorMsg.Contains("City") || errorMsg.Contains("State") || errorMsg.Contains("Zip"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address details are missing or invalid in package." };

            if (errorMsg.Contains("Recipient") || errorMsg.Contains("FirstName") || errorMsg.Contains("LastName"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient details are missing or invalid in package." };

            if (!string.IsNullOrEmpty(errorMsg))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = errorMsg };

            yield break;
        }

    }
}
