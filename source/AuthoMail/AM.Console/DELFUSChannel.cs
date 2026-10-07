using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using AM.USPSLib;
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
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AM.Console
{
    class DELFUSChannel : AMSchemaChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string channelType;
        public DELFUSChannel(string channelCode, string channelType)
            : base(channelCode, channelType)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
        }

        protected override void EnumerateBatches()
        {
            log.Debug("Enumerate Batches job queue for " + SFTChannel.Code + " is started.");

            if (SFTChannel.ServerIP.IsNullOrWhiteSpace() || Config.Common_ServerIP != SFTChannel.ServerIP)
            {
                log.Debug("System cannot process DELFUS channel on this server.");
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

                    ProcessXML(xmlFilePath, extractedDir, localZipFilePath, batchName, channelDownloadPath, lastModifiedTime, sftSite);
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

        private void ProcessXML(string xmlFilePath, string extractedDirPath, string localZipFilePath, string batchName, string channelDownloadPath, DateTime receivedOnDateTime, SFTUtil sftSite)
        {
            MailBatch mbRequest;
            try
            {
                mbRequest = XmlUtil.Deserialize<MailBatch>(File.ReadAllText(xmlFilePath));
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
                var clientCode = mbRequest.ClientCode == null ? "InvalidClientCode" : mbRequest.ClientCode;

                if (clientCode == "InvalidClientCode")
                {
                    var msg = "Client code is required in provided XML.".Fill(clientCode);
                    ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), xmlFilePath);
                    return;
                }

                client = clientSvc.GetClientByAppCode(clientCode).Result;

                if (client == null)
                {
                    var msg = "Client code {0} is not valid from provided XML.".Fill(clientCode);
                    ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), xmlFilePath);
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
                    UniqueId = mbRequest.Id == null ? Guid.NewGuid() : mbRequest.Id,
                    Priority = mbRequest.Priority.ToString(),
                    DocumentOrPackageType = mbRequest.DocumentOrPackageType.IsNullOrWhiteSpace() ? null : mbRequest.DocumentOrPackageType,
                    ClientBatchId = mbRequest.ClientBatchId == null ? batchName : mbRequest.ClientBatchId,
                    ServerIP = Config.Common_ServerIP
                };

                //AM Mail Batch Custom Info
                MailBatchCustomInfo mailBatchCustomInfo;
                var customeInfoElement = mbRequest.CustomInfo;
                if (customeInfoElement != null)
                {
                    foreach (var element in customeInfoElement)
                    {
                        mailBatchCustomInfo = new MailBatchCustomInfo() { Key = element.Key, Value = element.Value, DataType = element.DataType.ToString() };
                        mailBatchCustomInfoList.Add(mailBatchCustomInfo);
                    }
                }

                //AM Mail Batch Return Info
                var returnInfoElement = mbRequest.ReturnInfo;
                if (returnInfoElement != null)
                {
                    var returnRcptInfoElement = returnInfoElement.Recipient;
                    if (returnRcptInfoElement != null)
                    {
                        mailBatch.IsReturnInfoSpecified = true;

                        mailBatch.RetRcptFirstName = returnRcptInfoElement.FirstName;
                        mailBatch.RetRcptLastName = returnRcptInfoElement.LastName;
                        mailBatch.RetRcptMiddleName = returnRcptInfoElement.MiddleName;
                        mailBatch.RetRcptOrganization = returnRcptInfoElement.Organization;
                        mailBatch.RetRcptPhone = returnRcptInfoElement.Phone;
                        mailBatch.RetRcptPrefix = returnRcptInfoElement.Prefix;
                        mailBatch.RetRcptSuffix = returnRcptInfoElement.Suffix;
                    }

                    var returnAddressElement = returnInfoElement.Address;
                    if (returnAddressElement != null)
                    {
                        mailBatch.IsReturnInfoSpecified = true;

                        mailBatch.RetRcptAddrOptionalLine1AboveStreet = returnAddressElement.OptionalLine1AboveStreet;
                        mailBatch.RetRcptAddrOptionalLine2AboveStreet = returnAddressElement.OptionalLine2AboveStreet;
                        mailBatch.RetRcptAddressStreet = returnAddressElement.Street;
                        mailBatch.RetRcptAddressAptOrSuiteOrOther = returnAddressElement.AptOrSuiteOrOther;
                        mailBatch.RetRcptAddrOptionalLine1BelowStreet = returnAddressElement.OptionalLine1BelowStreet;
                        mailBatch.RetRcptAddrOptionalLine2BelowStreet = returnAddressElement.OptionalLine2BelowStreet;
                        mailBatch.RetRcptAddressCity = returnAddressElement.City;
                        mailBatch.RetRcptAddressState = returnAddressElement.State;
                        mailBatch.RetRcptAddressZIP = returnAddressElement.Zip;
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
                    Directory.Move(extractedDirPath, mbi.AMRequestPath);

                    commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now).Wait();

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while inserting General Channel batches details for Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                }
            }

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
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string localZipFilePath)
        {
            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            return localZipFilePath;
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

            if ((SFTChannel.Code.Contains("PENFED") || SFTChannel.Code.Contains("BECU")) && availableTriggerFilePaths.Count > 0)
            {
                if (SFTChannel.Host.Contains("FAGOSFTP"))
                {
                    availableTriggerFilePaths = availableTriggerFilePaths.Where(path => Path.GetFileNameWithoutExtension(path).StartsWith(Config.Common_EnvironmentInitial)).ToList();
                }
            }

            return availableTriggerFilePaths;
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

                    var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");
                    int totalNodes = 0;

                    MailBatch mbRequest;
                    try
                    {
                        mbRequest = XmlUtil.Deserialize<MailBatch>(File.ReadAllText(xmlFilePath));
                        totalNodes = mbRequest.MailPieces.Count;
                    }
                    catch (Exception ex)
                    {
                        var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                        log.Error(msg, ex);
                        ReportInvalidBatch(mailBatch, "");
                        continue;
                    }

                    foreach (var mp in mbRequest.MailPieces)
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
                            mailPieceDTO.BusinessFriendlyIdName = mp.BusinessFriendlyIdName.TrimIfNotNull();

                            //Here we are inserting BusinessIdentifier for Client if not exist.
                            ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.CheckBusinessIdNameAlreadyExist(client.Id, mailPieceDTO.BusinessFriendlyIdName);
                            if (clientBusinessIdDetails == null)
                            {
                                commonSvc.InsertBusinessIdentifierName(client.Id, mailPieceDTO.BusinessFriendlyIdName.TrimIfNotNull());
                            }

                            mailPieceDTO.BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue.TrimIfNotNull();
                            mailPieceDTO.DocumentOrPackageType = mp.DocumentOrPackageType == null ? mailBatch.DocumentOrPackageType : mp.DocumentOrPackageType;
                            mailPieceDTO.UniqueId = mp.Id == null || mp.Id == Guid.Empty ? Guid.NewGuid() : mp.Id;

                            mailPieceDTO.FileName = mp.DocumentOrPackage.Name.TrimIfNotNull();

                            if (mp.RecipientInfo != null)
                            {
                                if (mp.RecipientInfo.Recipient != null)
                                {
                                    mailPieceDTO.IsRcptInfoSpecified = true;

                                    mailPieceDTO.RcptFirstName = mp.RecipientInfo.Recipient.FirstName.TrimIfNotNull();
                                    mailPieceDTO.RcptLastName = mp.RecipientInfo.Recipient.LastName.TrimIfNotNull();
                                    mailPieceDTO.RcptMiddleName = mp.RecipientInfo.Recipient.MiddleName.TrimIfNotNull();
                                    mailPieceDTO.RcptOrganization = mp.RecipientInfo.Recipient.Organization.TrimIfNotNull();
                                    mailPieceDTO.RcptPrefix = mp.RecipientInfo.Recipient.Prefix.TrimIfNotNull();
                                    mailPieceDTO.RcptSuffix = mp.RecipientInfo.Recipient.Suffix.TrimIfNotNull();
                                    mailPieceDTO.RcptPhone = mp.RecipientInfo.Recipient.Phone.TrimIfNotNull();
                                    mailPieceDTO.RcptEmailId = mp.RecipientInfo.Recipient.EmailAddress.TrimIfNotNull();
                                }
                                else
                                    mailPieceDTO.IsRcptInfoSpecified = false;

                                if (mp.RecipientInfo.Address != null)
                                {
                                    mailPieceDTO.IsRcptInfoSpecified = true;

                                    mailPieceDTO.RcptAddrOptionalLine1AboveStreet = mp.RecipientInfo.Address.OptionalLine1AboveStreet.TrimIfNotNull();
                                    mailPieceDTO.RcptAddrOptionalLine2AboveStreet = mp.RecipientInfo.Address.OptionalLine2AboveStreet.TrimIfNotNull();

                                    mailPieceDTO.RcptAddressStreet = mp.RecipientInfo.Address.Street.TrimIfNotNull();
                                    mailPieceDTO.RcptAddressAptOrSuiteOrOther = mp.RecipientInfo.Address.AptOrSuiteOrOther.TrimIfNotNull();

                                    mailPieceDTO.RcptAddrOptionalLine1BelowStreet = mp.RecipientInfo.Address.OptionalLine1BelowStreet.TrimIfNotNull();
                                    mailPieceDTO.RcptAddrOptionalLine2BelowStreet = mp.RecipientInfo.Address.OptionalLine2BelowStreet.TrimIfNotNull();

                                    mailPieceDTO.RcptAddressCity = mp.RecipientInfo.Address.City.TrimIfNotNull();
                                    mailPieceDTO.RcptAddressState = mp.RecipientInfo.Address.State.TrimIfNotNull();
                                    mailPieceDTO.RcptAddressZIP = mp.RecipientInfo.Address.Zip.TrimIfNotNull();
                                    mailPieceDTO.RcptAddressCountry = mp.RecipientInfo.Address.Country.TrimIfNotNull();
                                }
                            }
                            else
                            {
                                mailPieceDTO.IsRcptInfoSpecified = false;
                            }

                            mailPieceDTO.IsEDeliveryRequested = false;
                            mailPieceDTO.IsFallBackRequested = false;

                            if (mp.ReturnInfo == null)
                            {
                                mailPieceDTO.IsReturnInfoSpecified = false;
                            }
                            else
                            {
                                if (mp.ReturnInfo.Recipient != null)
                                {
                                    mailPieceDTO.IsReturnInfoSpecified = true;
                                    mailPieceDTO.RetRcptFirstName = mp.ReturnInfo.Recipient.FirstName.TrimIfNotNull();
                                    mailPieceDTO.RetRcptLastName = mp.ReturnInfo.Recipient.LastName.TrimIfNotNull();

                                    mailPieceDTO.RetRcptMiddleName = mp.ReturnInfo.Recipient.MiddleName.TrimIfNotNull();
                                    mailPieceDTO.RetRcptOrganization = mp.ReturnInfo.Recipient.Organization.TrimIfNotNull();
                                    mailPieceDTO.RetRcptPrefix = mp.ReturnInfo.Recipient.Prefix.TrimIfNotNull();
                                    mailPieceDTO.RetRcptSuffix = mp.ReturnInfo.Recipient.Suffix.TrimIfNotNull();
                                    mailPieceDTO.RetRcptPhone = mp.ReturnInfo.Recipient.Phone.TrimIfNotNull();
                                }
                                else
                                    mailPieceDTO.IsReturnInfoSpecified = false;

                                if (mp.ReturnInfo.Address != null)
                                {
                                    mailPieceDTO.IsReturnInfoSpecified = true;
                                    mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = mp.ReturnInfo.Address.OptionalLine1AboveStreet.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = mp.ReturnInfo.Address.OptionalLine2AboveStreet.TrimIfNotNull();

                                    mailPieceDTO.RetRcptAddressStreet = mp.ReturnInfo.Address.Street.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = mp.ReturnInfo.Address.AptOrSuiteOrOther.TrimIfNotNull();

                                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = mp.ReturnInfo.Address.OptionalLine1BelowStreet.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = mp.ReturnInfo.Address.OptionalLine2BelowStreet.TrimIfNotNull();

                                    mailPieceDTO.RetRcptAddressCity = mp.ReturnInfo.Address.City.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddressState = mp.ReturnInfo.Address.State.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddressZIP = mp.ReturnInfo.Address.Zip.TrimIfNotNull();
                                }
                            }

                            if (mp.CustomInfo != null)
                            {
                                foreach (var cstmInfo in mp.CustomInfo)
                                {
                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = cstmInfo.Key.TrimIfNotNull(), DataType = cstmInfo.DataType, Value = cstmInfo.Value.TrimIfNotNull() };

                                    mailPieceCustomInfo.Add(customInfo);
                                }
                                mailPieceDTO.MailPieceCustomInfo = mailPieceCustomInfo;
                            }
                            //System will check for common validations
                            MailPieceValidationResult commonValidationResult = ValidateCommonValidation(mailPieceDTO, mbi);
                            if (commonValidationResult.Code == OutComeCodes.No_Error)
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                mailPieceDTO.ErrorDesc = "";
                                if (!mailPieceDTO.RcptAddressCountry.IsNullOrWhiteSpace() && (!mailPieceDTO.RcptAddressCountry.Contains("USA") || !mailPieceDTO.RcptAddressCountry.Contains("US") || !mailPieceDTO.RcptAddressCountry.Contains("The United States of America") || !mailPieceDTO.RcptAddressCountry.Contains("United States of America") || !mailPieceDTO.RcptAddressCountry.Contains("United States")))
                                {
                                    mailPieceDTO.RcptAddressTypes = AddressType.FOREIGN.ToString();
                                }
                                else
                                {
                                    mailPieceDTO.RcptAddressTypes = AddressType.DHAM.ToString();
                                }
                            }
                            else
                            {
                                mailPieceDTO.ErrorCode = commonValidationResult.Code;
                                mailPieceDTO.ErrorDesc = commonValidationResult.ErrorMessage;
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            var msg = "Unable to process package info node for mail batch {0}".Fill(mbi.BatchName);

                            log.Error(msg, ex);

                            mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                            mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.ToString();
                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                        }

                        try
                        {
                            mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo, mbi.MailBatch.ReceivedOn, mbi.MailBatch.DownloadedOn);
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

                        //Here System will validate Addresses for valid mail pieces
                        //if (validMailPieces.Count > 0)
                        //        Channel.ValidateAddressType(mailBatch, validMailPieces);

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

                    if (mailPieceList.All(mp => mp.IsEDeliveryRequested && mp.ShippingStatus != ShippingStatus.Error.ToString()))
                    {
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.SubmittedToEDelivery, Common.Now).Wait();
                        commonSvc.UpdateEDeliveryPipelineStatus(mbi.MailBatch.Id, EDPipelineStatus.Validated, Common.Now);
                    }
                    else
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
            if (sftSite.IsFileAvailable(ftpFilePath))
            {
                if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                {
                    sftSite.MoveFile(ftpFilePath, ftpBackUpFilePath);
                }
                else
                    sftSite.DeleteFile(ftpFilePath);
            }
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
                mailPieceDTO.TotalPages = pageCount;//System is going to store TotalPages only for AM.EDMailPieces table from here
                blankPageNos = Channel.CheckIfPdfBlank(pageCount, blankPageNos, localFilePath, reader);
            }

            //NOTE : Here system is storing file size of received pdf in KB
            FileInfo fileInfo = new FileInfo(localFilePath);
            mailPieceDTO.PDFFileSize = (fileInfo.Length / 1024);

            bool isCountryUS = mailPieceDTO.RcptAddressCountry.IsCountryUS();
            //Document Or Package Type validation
            var docType = !isCountryUS ? docTypeSvc.GetIntlDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType, pageCount, mbi.MailBatch.ReceivedOn.Value.UtcToEst()) : docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType, pageCount, mbi.MailBatch.ReceivedOn.Value.UtcToEst());

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
            else if (!mailPieceDTO.IsReturnInfoSpecified && !mbi.MailBatch.IsReturnInfoSpecified && !docType.IsReturnInfoSpecified)
            {
                return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return mailing address is not specified for this package." };
            }
            else
            {
                //if at mail piece level or at mail batch level, system didn't find return info then it will check for MailBatch level if exist system will use that in mail piece level 
                mailPieceDTO.TotalBlankPages = blankPageNos.Count;
                mailPieceDTO.BlankPageNumbers = string.Join(",", blankPageNos);
                if (!mailPieceDTO.IsReturnInfoSpecified && !mbi.MailBatch.IsReturnInfoSpecified && docType.IsReturnInfoSpecified)
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
            }
            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }
    }
}
