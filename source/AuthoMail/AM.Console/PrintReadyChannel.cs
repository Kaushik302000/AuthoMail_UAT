using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace AM.Console
{
    class PrintReadyChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IGeneralService generalSvc;

        private string channelType;

        private static XmlSchemaSet schemas;

        static PrintReadyChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", XmlReader.Create(new StringReader(Resources.AMMailPieceSchema)));
        }

        public PrintReadyChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
        }

        #region EnumarateBatches
        protected override void EnumerateBatches()
        {
            log.Debug("Enumerate Batches job queue for " + SFTChannel.Code + " is started.");

            if (SFTChannel.ServerIP.IsNullOrWhiteSpace() || Config.Common_ServerIP != SFTChannel.ServerIP)
            {
                log.Debug("System cannot process Print Ready channel on this server.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000, SFTChannel.Port);
            sftSite.Connect();

            List<string> triggerFilePaths = GetTriggerFilePaths(sftSite);

            foreach (string triggerFile in triggerFilePaths)
            {
                try
                {
                    string triggerFilePath = Path.Combine(SFTChannel.ToADEDir, triggerFile + ".go");

                    //Download and Insert the batch in database first always.
                    string batchName = triggerFile;

                    DateTime lastModifiedTime = Common.Now;
                    if (channelType == ChannelType.Trigger)
                    {
                        //TODO : Implement check for zip exist or not
                        lastModifiedTime = sftSite.LastModifiedTimeOfFile(triggerFilePath);
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
                    string xmlFilePath = Path.Combine(channelDownloadPath, batchName, batchName + ".xml");
                    string ftpZipFilePath = Path.Combine(SFTChannel.ToADEDir, zipFileName);
                    string ftpTriggerFilePath = Path.Combine(SFTChannel.ToADEDir, "{0}.go".Fill(batchName));

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

                    DownloadBatchZipFile(sftSite, ftpZipFilePath, ftpTriggerFilePath, localZipFilePath);

                    try
                    {
                        string extractedDir = Path.Combine(Path.GetDirectoryName(localZipFilePath), Path.GetFileNameWithoutExtension(localZipFilePath));

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

                    ProcessBatch(xmlFilePath, localZipFilePath, batchName, channelDownloadPath, lastModifiedTime, sftSite);
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in EnumerateBatches Method For BatchName: {0} with Error : {1}".Fill(triggerFile, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }
        }

        private void ProcessBatch(string xmlFilePath, string localZipFilePath, string batchName, string channelDownloadPath, DateTime receivedOnDateTime, SFTUtil sftSite)
        {
            XmlDocument batchXML = new XmlDocument();
            XmlNamespaceManager nsManager;
            XmlNodeList mailPieceNodes;
            try
            {
                batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                nsManager = new XmlNamespaceManager(batchXML.NameTable);
                nsManager.AddNamespace("x", "http://schemas.corelogic.com/ade/authomail/v1.18.0");
                mailPieceNodes = batchXML.DocumentElement.ChildNodes;
            }
            catch (XmlException ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), localZipFilePath);
                return;
            }
            catch (Exception ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), localZipFilePath);
                return;
            }

            Client client;
            MailBatchDTO mailBatch;
            List<MailBatchCustomInfo> mailBatchCustomInfoList = new List<MailBatchCustomInfo>();
            try
            {
                var clientCode = batchXML.DocumentElement.Attributes["ClientCode"] == null ? "InvalidClientCode" : batchXML.DocumentElement.Attributes["ClientCode"].Value;

                if (clientCode == "InvalidClientCode")
                {
                    var msg = "Client code is required in provided XML.".Fill(clientCode);
                    ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), localZipFilePath);
                    return;
                }

                client = clientSvc.GetClientByAppCode(clientCode).Result;

                if (client == null)
                {
                    var msg = "Client code {0} is not valid from provided XML.".Fill(clientCode);
                    ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), localZipFilePath);
                    return;
                }

                mailBatch = new MailBatchDTO
                {
                    ClientId = client.Id,
                    BatchName = batchName,
                    SFTChannelId = SFTChannel.Id,
                    ReceivedOn = receivedOnDateTime,
                    PipelineStatus = BatchPipelineStatus.Received,
                    BatchStatus = BatchStatus.InProgress.ToString(),
                    //Parse AM mail batch request element
                    UniqueId = batchXML.DocumentElement.Attributes["Id"] == null ? Guid.NewGuid() : Guid.Parse(batchXML.DocumentElement.Attributes["Id"].Value),
                    Priority = batchXML.DocumentElement.Attributes["Priority"] == null ? Priority.Medium.ToString() : batchXML.DocumentElement.Attributes["Priority"].Value,
                    DocumentOrPackageType = batchXML.DocumentElement.Attributes["DocumentOrPackageType"]?.Value,
                    ClientBatchId = batchXML.DocumentElement.Attributes["ClientBatchId"] == null ? batchName : batchXML.DocumentElement.Attributes["ClientBatchId"].Value,
                    ServerIP = Config.Common_ServerIP
                };
                //AM Mail Batch Custom Info

                MailBatchCustomInfo mailBatchCustomInfo;
                var customeInfoElement = batchXML.DocumentElement.SelectSingleNode("./CustomInfo", nsManager);
                if (customeInfoElement != null)
                {
                    foreach (XmlElement element in customeInfoElement.ChildNodes)
                    {
                        mailBatchCustomInfo = new MailBatchCustomInfo() { Key = element.SelectSingleNode("./x:Key/text()", nsManager) == null ? string.Empty : element.SelectSingleNode("./x:Key/text()", nsManager).Value, DataType = element.SelectSingleNode("./x:DataType/text()", nsManager) == null ? string.Empty : element.SelectSingleNode("./x:DataType/text()", nsManager).Value, Value = element.SelectSingleNode("./x:Value/text()", nsManager) == null ? string.Empty : element.SelectSingleNode("./x:Value/text()", nsManager).Value };
                        mailBatchCustomInfoList.Add(mailBatchCustomInfo);
                    }
                }

                //AM Mail Batch Return Info
                var returnInfoElement = batchXML.DocumentElement.SelectSingleNode("./ReturnInfo", nsManager);
                if (returnInfoElement != null)
                {
                    var returnRcptInfoElement = returnInfoElement.SelectSingleNode("./x:Recipient", nsManager);
                    if (returnRcptInfoElement != null)
                    {
                        mailBatch.IsReturnInfoSpecified = true;

                        mailBatch.RetRcptFirstName = returnRcptInfoElement.SelectSingleNode("./x:FirstName/text()", nsManager)?.Value;
                        mailBatch.RetRcptLastName = returnRcptInfoElement.SelectSingleNode("./x:LastName/text()", nsManager)?.Value;

                        mailBatch.RetRcptMiddleName = returnRcptInfoElement.SelectSingleNode("./x:MiddleName/text()", nsManager)?.Value;
                        mailBatch.RetRcptOrganization = returnRcptInfoElement.SelectSingleNode("./x:Organization/text()", nsManager)?.Value;
                        mailBatch.RetRcptPhone = returnRcptInfoElement.SelectSingleNode("./x:Phone/text()", nsManager)?.Value;
                        mailBatch.RetRcptPrefix = returnRcptInfoElement.SelectSingleNode("./x:Prefix/text()", nsManager)?.Value;
                        mailBatch.RetRcptSuffix = returnRcptInfoElement.SelectSingleNode("./x:Suffix/text()", nsManager)?.Value;
                    }

                    var returnAddressElement = returnInfoElement.SelectSingleNode("./x:Address", nsManager);
                    if (returnAddressElement != null)
                    {
                        mailBatch.IsReturnInfoSpecified = true;

                        mailBatch.RetRcptAddrOptionalLine1AboveStreet = returnAddressElement.SelectSingleNode("./x:OptionalLine1AboveStreet/text()", nsManager)?.Value;
                        mailBatch.RetRcptAddrOptionalLine2AboveStreet = returnAddressElement.SelectSingleNode("./x:OptionalLine2AboveStreet/text()", nsManager)?.Value;

                        mailBatch.RetRcptAddressStreet = returnAddressElement.SelectSingleNode("./x:Street/text()", nsManager)?.Value;

                        mailBatch.RetRcptAddressAptOrSuiteOrOther = returnAddressElement.SelectSingleNode("./x:AptOrSuiteOrOther/text()", nsManager)?.Value;
                        mailBatch.RetRcptAddrOptionalLine1BelowStreet = returnAddressElement.SelectSingleNode("./x:OptionalLine1BelowStreet/text()", nsManager)?.Value;
                        mailBatch.RetRcptAddrOptionalLine2BelowStreet = returnAddressElement.SelectSingleNode("./x:OptionalLine2BelowStreet/text()", nsManager)?.Value;

                        mailBatch.RetRcptAddressCity = returnAddressElement.SelectSingleNode("./x:City/text()", nsManager)?.Value;
                        mailBatch.RetRcptAddressState = returnAddressElement.SelectSingleNode("./x:State/text()", nsManager)?.Value;
                        mailBatch.RetRcptAddressZIP = returnAddressElement.SelectSingleNode("./x:Zip/text()", nsManager)?.Value;
                    }
                }
                else
                    mailBatch.IsReturnInfoSpecified = false;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while processing batch from Batch '{0}.xml'".Fill(batchName);
                log.Error(msg, ex);
                MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                return;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch, mailBatchCustomInfoList);

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    //Here we create folder structure
                    mbi.AMResponsePath.CreateDirectoryIfNotExist();
                    mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                    mbi.PnSResponsePath.CreateDirectoryIfNotExist();
                    //Here we are moving whole directory so no need to create AMRequestPath directly.
                    Directory.Move(Path.Combine(channelDownloadPath, batchName), mbi.AMRequestPath);

                    commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now).Wait();

                    //Here after moving all batches to particular client location , archive amc batch
                    string archiveZipPath = Path.Combine(Config.Common_AuthoMailRootFolder, "Archive", batchName + ".zip");

                    if (File.Exists(archiveZipPath))
                        File.Delete(archiveZipPath);

                    File.Move(localZipFilePath, archiveZipPath);

                    //Now delete/archive trigger and zip from SFTP
                    MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + ".zip"));
                    MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + ".go"));

                    //After complete process clean AMC folder
                    channelDownloadPath.CleanDirectory();

                    scope.Complete();

                }
                catch (Exception ex)
                {
                    var msg = "Error occured while inserting General Channel batches details for Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                }
            }
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string ftpTriggerFilePath, string localZipFilePath)
        {
            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            return localZipFilePath;
        }

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string localZipFilePath)
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

            File.WriteAllText(invalidBatchPath, string.Empty);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            var mailBatchSFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mailBatch.SFTChannelId).Result;

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(mailBatchSFTChannel.Protocol), mailBatchSFTChannel.Host, mailBatchSFTChannel.UserName, mailBatchSFTChannel.Password, 300000, mailBatchSFTChannel.Port);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);
            if (sftSite.IsFileAvailable(Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".zip")))
                MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".zip"));

            MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".go"));

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();
        }

        private void MoveBatchToBackupDir(SFTUtil sftSite, string ftpFilePath)
        {
            string ftpBackUpFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(ftpFilePath));

            if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
            {
                sftSite.MoveFile(ftpFilePath, ftpBackUpFilePath);
            }
            else
                sftSite.DeleteFile(ftpFilePath);
        }

        private MailBatchDTO InsertInvalidBatchInfo(string batchName, DateTime receivedOn, string errorMessage)
        {
            MailBatchDTO mailBatch = new MailBatchDTO
            {
                UniqueId = Guid.NewGuid(),
                ClientId = GetInvalidClient(SFTChannel.GetChannelCode()).Id,
                SFTChannelId = SFTChannel.Id,
                BatchName = batchName,
                ClientBatchId = batchName,
                Priority = Priority.Low.ToString(),
                ServerIP = Config.Common_ServerIP
            };
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

        private List<string> GetTriggerFilePaths(SFTUtil sftSite)
        {
            var regex = new Regex(@"^([\s\S]*).go$", RegexOptions.IgnoreCase);

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableTriggerFilePaths != null && availableTriggerFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} trigger file(s) found.", availableTriggerFilePaths.Count));
            }
            else
            {
                log.Error("No trigger files found hence no batches are available to download.");
            }

            return availableTriggerFilePaths;
        }
        #endregion

        protected override void DownloadBatches()
        {

        }

        #region Validate Batches
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

                    var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

                    XmlDocument batchXML = new XmlDocument();
                    XmlNamespaceManager nsManager = null;
                    XmlNodeList mailPieceNodes = null;
                    int totalNodes = 0;
                    try
                    {
                        batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                        nsManager = new XmlNamespaceManager(batchXML.NameTable);
                        nsManager.AddNamespace("x", "http://schemas.corelogic.com/ade/authomail/v1.18.0");
                        mailPieceNodes = batchXML.DocumentElement.SelectSingleNode("./x:MailPieces", nsManager).ChildNodes;
                        totalNodes = mailPieceNodes.Count;
                    }
                    catch (XmlException ex)
                    {
                        var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                        log.Error(msg, ex);

                        ReportInvalidBatch(mailBatch, "");

                        continue;
                    }
                    catch (Exception ex)
                    {
                        var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                        log.Error(msg, ex);
                        ReportInvalidBatch(mailBatch, "");
                        continue;
                    }

                    foreach (XmlNode mailPieceNode in mailPieceNodes)
                    {
                        //mailPieceNode.Normalize();
                        MailPieceDTO mailPieceDTO = new MailPieceDTO();
                        MailPieceCustomInfo customInfo = null;
                        List<MailPieceCustomInfo> mailPieceCustomInfo = new List<MailPieceCustomInfo>();

                        mailPieceDTO.ClientId = client.Id;
                        mailPieceDTO.MailBatchId = mbi.MailBatch.Id;
                        mailPieceDTO.ShippingStatus = ShippingStatus.InProgress.ToString();

                        try
                        {
                            var requests = XmlUtil.Deserialize<MailPiece>(mailPieceNode.OuterXml);

                            mailPieceDTO.BusinessFriendlyIdName = requests.BusinessFriendlyIdName;

                            //Here we are inserting BusinessIdentifier for Client if not exist.
                            ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.GetBusinessIdNameFromClientId(client.Id);
                            if (clientBusinessIdDetails == null)
                            {
                                commonSvc.InsertBusinessIdentifierName(client.Id, mailPieceDTO.BusinessFriendlyIdName);
                            }

                            mailPieceDTO.BusinessFriendlyIdValue = requests.BusinessFriendlyIdValue;
                            mailPieceDTO.DocumentOrPackageType = requests.DocumentOrPackageType ?? mailBatch.DocumentOrPackageType;
                            mailPieceDTO.UniqueId = requests.Id == null || requests.Id == Guid.Empty ? Guid.NewGuid() : requests.Id;
                            mailPieceDTO.FileName = requests.DocumentOrPackage.Name;
                            mailPieceDTO.FilePattern = requests.DocumentOrPackage.Pattern;
                            if (!string.IsNullOrEmpty(requests.DocumentOrPackage.Pattern))
                            {
                                var file = Directory.EnumerateFiles(mbi.AMRequestPath, "*" + mailPieceDTO.FilePattern + "*.pdf").SingleOrDefault();

                                mailPieceDTO.FileName = Path.GetFileName(file);
                            }

                            if (!string.IsNullOrEmpty(requests.DocumentOrPackage.Base64String))
                            {
                                string filePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.UniqueId.ToString() + ".pdf");
                                File.WriteAllBytes(filePath, Convert.FromBase64String(requests.DocumentOrPackage.Base64String));
                                mailPieceDTO.FileName = Path.GetFileName(filePath);
                            }

                            if (requests.RecipientInfo != null)
                            {
                                if (requests.RecipientInfo.Recipient != null)
                                {
                                    mailPieceDTO.IsRcptInfoSpecified = true;

                                    mailPieceDTO.RcptFirstName = requests.RecipientInfo.Recipient.FirstName;
                                    mailPieceDTO.RcptLastName = requests.RecipientInfo.Recipient.LastName;
                                    mailPieceDTO.RcptMiddleName = requests.RecipientInfo.Recipient.MiddleName;
                                    mailPieceDTO.RcptOrganization = requests.RecipientInfo.Recipient.Organization;
                                    mailPieceDTO.RcptPrefix = requests.RecipientInfo.Recipient.Prefix;
                                    mailPieceDTO.RcptSuffix = requests.RecipientInfo.Recipient.Suffix;
                                    mailPieceDTO.RcptPhone = requests.RecipientInfo.Recipient.Phone;
                                }
                                else
                                    mailPieceDTO.IsRcptInfoSpecified = false;

                                if (requests.RecipientInfo.Address != null)
                                {
                                    mailPieceDTO.IsRcptInfoSpecified = true;

                                    mailPieceDTO.RcptAddrOptionalLine1AboveStreet = requests.RecipientInfo.Address.OptionalLine1AboveStreet;
                                    mailPieceDTO.RcptAddrOptionalLine2AboveStreet = requests.RecipientInfo.Address.OptionalLine2AboveStreet;

                                    mailPieceDTO.RcptAddressStreet = requests.RecipientInfo.Address.Street;
                                    mailPieceDTO.RcptAddressAptOrSuiteOrOther = requests.RecipientInfo.Address.AptOrSuiteOrOther;

                                    mailPieceDTO.RcptAddrOptionalLine1BelowStreet = requests.RecipientInfo.Address.OptionalLine1BelowStreet;
                                    mailPieceDTO.RcptAddrOptionalLine2BelowStreet = requests.RecipientInfo.Address.OptionalLine2BelowStreet;

                                    mailPieceDTO.RcptAddressCity = requests.RecipientInfo.Address.City;
                                    mailPieceDTO.RcptAddressState = requests.RecipientInfo.Address.State;
                                    mailPieceDTO.RcptAddressZIP = requests.RecipientInfo.Address.Zip;
                                }
                            }
                            else
                            {
                                mailPieceDTO.IsRcptInfoSpecified = false;
                            }

                            if (requests.ReturnInfo == null)
                            {
                                mailPieceDTO.IsReturnInfoSpecified = false;
                                //if at mail piece level system didn't find return info then it will check for MailBatch level if exist system will use that in mail piece level 
                                if (mailBatch.IsReturnInfoSpecified)
                                {
                                    mailPieceDTO.RetRcptFirstName = mailBatch.RetRcptFirstName;
                                    mailPieceDTO.RetRcptLastName = mailBatch.RetRcptLastName;

                                    mailPieceDTO.RetRcptMiddleName = mailBatch.RetRcptMiddleName;
                                    mailPieceDTO.RetRcptOrganization = mailBatch.RetRcptOrganization;
                                    mailPieceDTO.RetRcptPrefix = mailBatch.RetRcptPrefix;
                                    mailPieceDTO.RetRcptSuffix = mailBatch.RetRcptSuffix;
                                    mailPieceDTO.RetRcptPhone = mailBatch.RetRcptPhone;

                                    mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = mailBatch.RetRcptAddrOptionalLine1AboveStreet;
                                    mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = mailBatch.RetRcptAddrOptionalLine2AboveStreet;

                                    mailPieceDTO.RetRcptAddressStreet = mailBatch.RetRcptAddressStreet;
                                    mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = mailBatch.RetRcptAddressAptOrSuiteOrOther;

                                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = mailBatch.RetRcptAddrOptionalLine1BelowStreet;
                                    mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = mailBatch.RetRcptAddrOptionalLine2BelowStreet;

                                    mailPieceDTO.RetRcptAddressCity = mailBatch.RetRcptAddressCity;
                                    mailPieceDTO.RetRcptAddressState = mailBatch.RetRcptAddressState;
                                    mailPieceDTO.RetRcptAddressZIP = mailBatch.RetRcptAddressZIP;
                                }
                            }
                            else
                            {
                                if (requests.ReturnInfo.Recipient != null)
                                {
                                    mailPieceDTO.IsReturnInfoSpecified = true;
                                    mailPieceDTO.RetRcptFirstName = requests.ReturnInfo.Recipient.FirstName;
                                    mailPieceDTO.RetRcptLastName = requests.ReturnInfo.Recipient.LastName;

                                    mailPieceDTO.RetRcptMiddleName = requests.ReturnInfo.Recipient.MiddleName;
                                    mailPieceDTO.RetRcptOrganization = requests.ReturnInfo.Recipient.Organization;
                                    mailPieceDTO.RetRcptPrefix = requests.ReturnInfo.Recipient.Prefix;
                                    mailPieceDTO.RetRcptSuffix = requests.ReturnInfo.Recipient.Suffix;
                                    mailPieceDTO.RetRcptPhone = requests.ReturnInfo.Recipient.Phone;
                                }
                                else
                                    mailPieceDTO.IsReturnInfoSpecified = false;

                                if (requests.ReturnInfo.Address != null)
                                {
                                    mailPieceDTO.IsReturnInfoSpecified = true;
                                    mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = requests.ReturnInfo.Address.OptionalLine1AboveStreet;
                                    mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = requests.ReturnInfo.Address.OptionalLine2AboveStreet;

                                    mailPieceDTO.RetRcptAddressStreet = requests.ReturnInfo.Address.Street;
                                    mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = requests.ReturnInfo.Address.AptOrSuiteOrOther;

                                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = requests.ReturnInfo.Address.OptionalLine1BelowStreet;
                                    mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = requests.ReturnInfo.Address.OptionalLine2BelowStreet;

                                    mailPieceDTO.RetRcptAddressCity = requests.ReturnInfo.Address.City;
                                    mailPieceDTO.RetRcptAddressState = requests.ReturnInfo.Address.State;
                                    mailPieceDTO.RetRcptAddressZIP = requests.ReturnInfo.Address.Zip;
                                }
                            }

                            if (requests.CustomInfo != null)
                            {
                                foreach (var cstmInfo in requests.CustomInfo)
                                {
                                    if (cstmInfo.Key == "TrackingNumber")
                                    {
                                        mailPieceDTO.TrackingNo = cstmInfo.Value.Trim();
                                    }
                                    else
                                    {
                                        if (cstmInfo.Key == "ShippingCarrier")
                                        {
                                            mailPieceDTO.ShippingCarrier = cstmInfo.Value;
                                        }
                                        else if (cstmInfo.Key == "ShippingServiceType")
                                        {
                                            mailPieceDTO.ShippingServiceType = cstmInfo.Value;
                                        }
                                        else if (cstmInfo.Key == "ReturnTrackingNumber")
                                        {
                                            mailPieceDTO.ReturnTrackingNo = cstmInfo.Value;
                                        }
                                    }

                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = cstmInfo.Key, DataType = cstmInfo.DataType, Value = cstmInfo.Value };

                                    mailPieceCustomInfo.Add(customInfo);

                                }
                            }
                            //First System will check for schema validation
                            MailPieceValidationResult schemaValidationResult = ValidateMailPieceNode(mailPieceNode);
                            if (schemaValidationResult.Code == OutComeCodes.No_Error)
                            {
                                //System will check for common validations
                                MailPieceValidationResult commonValidationResult = ValidateCommonValidation(mailPieceDTO, mbi);
                                if (commonValidationResult.Code == OutComeCodes.No_Error)
                                {
                                    // System will check for client specific validations
                                    MailPieceValidationResult clientSpecificValidationResult = ValidateClientSpecificRules(mailPieceDTO, mbi);
                                    if (clientSpecificValidationResult.Code == OutComeCodes.No_Error)
                                    {
                                        mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                        mailPieceDTO.ErrorDesc = "";
                                    }
                                    else
                                    {
                                        mailPieceDTO.ErrorCode = clientSpecificValidationResult.Code;
                                        mailPieceDTO.ErrorDesc = clientSpecificValidationResult.ErrorMessage;
                                        mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                    }
                                }
                                else
                                {
                                    mailPieceDTO.ErrorCode = commonValidationResult.Code;
                                    mailPieceDTO.ErrorDesc = commonValidationResult.ErrorMessage;
                                    mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                }
                            }
                            else
                            {
                                mailPieceDTO.ErrorCode = schemaValidationResult.Code;
                                mailPieceDTO.ErrorDesc = schemaValidationResult.ErrorMessage;
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            var msg = "Unable to process package info node for mail batch {0}".Fill(mbi.BatchName);

                            log.Error(msg, ex);

                            mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                            mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                        }

                        try
                        {
                            mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo);
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

                    var mailPieceList = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                    if (mailPieceList != null)
                    {
                        var validMailPieces = mailPieceList.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                        if (validMailPieces.Count == 0)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                            mbi.MailBatch.ValidationErrors = "Xml does not contain any valid mail piece nodes.";
                            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString()).Wait();
                        }
                        else if (validMailPieces.Count < mailPieceList.Count)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.PartiallyValid.ToString();
                        }
                        else
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Valid.ToString();
                        }

                        commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, mbi.MailBatch.ValidationStatus, mbi.MailBatch.ValidationErrors);
                    }

                    if (totalNodes != mailPieceList.Count)
                    {
                        throw new Exception("Mail pieces count and received xml count is mismatched for Batch-Id : {0}<br/>".Fill(mailBatch.Id) + "Mail Piece count is {0} and received xml nodes count is {1}".Fill(mailPieceList.Count, totalNodes));
                    }

                    Channel.CalculateDueDateAndPrintLocation(client, mailBatch, mailPieceList);

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now).Wait();
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in ValidateBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }
            log.Debug("Validate job queue is completed.");
        }

        private MailPieceValidationResult ValidateCommonValidation(MailPieceDTO mailPieceDTO, MailBatchInfo mbi)
        {
            //in last system will check for general validations
            string localFilePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);
            if (!File.Exists(localFilePath))
            {
                return new MailPieceValidationResult() { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf file is not available for this package." };
            }

            int pageCount = 0;
            List<int> blankPageNos = new List<int>();
            using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(localFilePath))
            {
                pageCount = reader.NumberOfPages;

                if (mbi.Client.IsReqBlankPageValidation)
                {
                    blankPageNos = Channel.CheckIfPdfBlank(pageCount, blankPageNos, localFilePath, reader);
                }
            }


            //NOTE : Here system is storing file size of received pdf in KB
            FileInfo fileInfo = new FileInfo(localFilePath);
            mailPieceDTO.PDFFileSize = (fileInfo.Length / 1024);

            string documentOrPackageType = mailPieceDTO.DocumentOrPackageType;
            DocType docType;
            bool isCountryUS = mailPieceDTO.RcptAddressCountry.IsCountryUS();
            if (!string.IsNullOrWhiteSpace(mailPieceDTO.ShippingCarrier) && !string.IsNullOrWhiteSpace(mailPieceDTO.ShippingServiceType))
            {
                docType = !isCountryUS ? docTypeSvc.GetIntlDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType, pageCount, mailPieceDTO.ShippingCarrier, mailPieceDTO.ShippingServiceType) : docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType, pageCount, mailPieceDTO.ShippingCarrier, mailPieceDTO.ShippingServiceType);
            }
            else
            {
                docType = !isCountryUS ? docTypeSvc.GetIntlDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType, pageCount, mbi.MailBatch.ReceivedOn.Value.UtcToEst()) : docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType, pageCount, mbi.MailBatch.ReceivedOn.Value.UtcToEst());
            }


            if (docType != null)
            {
                mailPieceDTO.DocTypeId = docType.Id;
                mailPieceDTO.PnSConfigDetailId = docType.PnSConfigDetailId;
                mailPieceDTO.PrintTrailId = docType.PrintTrailId;
            }

            if (docType == null)
            {
                return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = $"Either the specified document or package type '{mailPieceDTO.DocumentOrPackageType}' is not configured  {(isCountryUS == false ? "for international mailing" : "")} or no matching print configuration found for page count ({pageCount})." };
            }
            else if (pageCount == blankPageNos.Count())
            {
                mailPieceDTO.TotalBlankPages = blankPageNos.Count;
                mailPieceDTO.BlankPageNumbers = string.Join(",", blankPageNos);
                return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = "All pages are blank in received pdf file." };
            }
            else if (blankPageNos.Contains(1))//Here we are considering 1st page as coversheet or label for print ready client.
            {
                mailPieceDTO.TotalBlankPages = blankPageNos.Count;
                mailPieceDTO.BlankPageNumbers = string.Join(",", blankPageNos);
                return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = "Coversheet or label is blank." };
            }
            else
            {
                //if at mail piece level or at mail batch level, system didn't find return info then it will check for MailBatch level if exist system will use that in mail piece level 
                mailPieceDTO.TotalBlankPages = blankPageNos.Count;
                mailPieceDTO.BlankPageNumbers = string.Join(",", blankPageNos);
                if (!mailPieceDTO.IsReturnInfoSpecified || !mbi.MailBatch.IsReturnInfoSpecified)
                {
                    mailPieceDTO.RetRcptFirstName = docType.RetRcptFirstName;
                    mailPieceDTO.RetRcptLastName = docType.RetRcptLastName;

                    mailPieceDTO.RetRcptMiddleName = docType.RetRcptMiddleName;
                    mailPieceDTO.RetRcptOrganization = docType.RetRcptOrganization;
                    mailPieceDTO.RetRcptPrefix = docType.RetRcptPrefix;
                    mailPieceDTO.RetRcptSuffix = docType.RetRcptSuffix;
                    mailPieceDTO.RetRcptPhone = docType.RetRcptPhone;

                    mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = docType.RetRcptAddrOptionalLine1AboveStreet;
                    mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = docType.RetRcptAddrOptionalLine2AboveStreet;

                    mailPieceDTO.RetRcptAddressStreet = docType.RetRcptAddressStreet;
                    mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = docType.RetRcptAddressAptOrSuiteOrOther;

                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = docType.RetRcptAddrOptionalLine1BelowStreet;
                    mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = docType.RetRcptAddrOptionalLine2BelowStreet;

                    mailPieceDTO.RetRcptAddressCity = docType.RetRcptAddressCity;
                    mailPieceDTO.RetRcptAddressState = docType.RetRcptAddressState;
                    mailPieceDTO.RetRcptAddressZIP = docType.RetRcptAddressZIP;
                }
                return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
            }
        }

        private MailPieceValidationResult ValidateClientSpecificRules(MailPieceDTO mailPieceDTO, MailBatchInfo mbi)
        {
            //temparary Code
            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private MailPieceValidationResult ValidateMailPieceNode(XmlNode xmlPiece)
        {
            var errors = GetAllMailPieceErrors(xmlPiece).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private IEnumerable<MailPieceValidationResult> GetAllMailPieceErrors(XmlNode xmlPiece)
        {
            XDocument xDoc = XDocument.Parse(xmlPiece.OuterXml);
            StringBuilder sbErrorMessage = new StringBuilder();

            xDoc.Validate(schemas, (o, e) =>
            {
                sbErrorMessage.Append(e.Message);
            });

            string errorMsg = sbErrorMessage.ToString();

            //if (errorMsg.Contains("Street") || errorMsg.Contains("City") || errorMsg.Contains("State") || errorMsg.Contains("Zip"))
            //    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address details are missing or invalid in package." };

            if (errorMsg.Contains("Recipient") || errorMsg.Contains("FirstName") || errorMsg.Contains("LastName"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient details are missing or invalid in package." };

            if (!string.IsNullOrEmpty(errorMsg))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = errorMsg };

            yield break;
        }
        #endregion

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
                            UploadBaseReportToClient(mbi, mailPieces, deliverable);

                            //Here we have to provide clientwise notification so only reporting functionality will be customized over here
                            var emailErrorReportPath = GenerateClientWiseAcknowledgementReport(mbi, mailPieces, deliverable);

                            MailUtility.SendDownloadNotificationEmail(mbi, mailPieces, emailErrorReportPath, deliverable.EmailChannel);
                            //After upload change extension of local file Path
                            if (File.Exists(emailErrorReportPath))
                                File.Move(emailErrorReportPath, Path.ChangeExtension(emailErrorReportPath, ".Complete"));
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured in AcknowledgeBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedExceptionEmail(msg);
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

        private string GenerateClientWiseAcknowledgementReport(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable reportDeliverable)
        {
            string errorReportPath = string.Empty;
            string errorReportFileName = string.Empty;
            StringBuilder errorReportData;

            switch (mbi.Client.AppCode)
            {
                case "HSBC":
                    using (IHSBCService hsbcSvc = new HSBCService())
                    {
                        errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));

                        errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        //Here we are generating error report even though it does not have any errored records
                        errorReportData = hsbcSvc.HSBCErrorReport(mailPieces, mbi.Client.TimeZone, reportDeliverable.Delimeter);

                        File.WriteAllText(errorReportPath, errorReportData.ToString());
                    }
                    break;
                default:
                    errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                    errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                    errorReportData = generalSvc.GeneralErrorReport(mailPieces, reportDeliverable.Delimeter);
                    if (errorReportData != null)
                    {
                        File.WriteAllText(errorReportPath, errorReportData.ToString());
                    }
                    break;
            }

            return errorReportPath;
        }

        private void UploadBaseReportToClient(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            string errorReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
            var erroredMPs = mailPieces.Where(mp => mp.ShippingStatus == ShippingStatus.Error.ToString()).ToList();

            if (erroredMPs.Count() > 0)
            {
                string errorData = generalSvc.GeneralXMLReport(erroredMPs, mbi.MailBatch, mbi.Client.AppCode);
                if (!string.IsNullOrWhiteSpace(errorData))
                {
                    File.WriteAllText(errorReportPath, errorData.ToString());
                }
                else
                    errorReportPath = "";
            }
            else
                errorReportPath = "";

            if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.Invalid)
            {
                // Batch status Reported Back
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

                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000, SFTChannel.Port);
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
        }

        private void ReportInvalidBatch(MailBatchInfo mbi, string msg, Deliverable deliverables)
        {
            MailUtility.SendNoDataNotificationEmail(mbi, deliverables.EmailChannel, msg);
            //For Invalid Batch, we will generate error File
            mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, msg).Wait();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();

            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString()).Wait();
        }
    }
}
