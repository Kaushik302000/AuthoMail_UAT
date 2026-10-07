using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace AM.Console
{
    class PAMSchemaChannel : PChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string channelType;

        private static XmlSchemaSet schemas;

        static PAMSchemaChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", XmlReader.Create(new StringReader(Resources.AMMailPieceSchema)));
        }

        public PAMSchemaChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            this.channelType = channelType;
        }

        #region EnumarateBatches
        protected override bool EnumerateBatches()
        {
            log.Debug("Enumerate Batches job queue for " + SFTChannel.Code + " is started.");

            if (SFTChannel.ServerIP.IsNullOrWhiteSpace() || Config.Common_ServerIP != SFTChannel.ServerIP)
            {
                log.Debug("System cannot process API Generic channel on this server.");
                return false;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000, SFTChannel.Port);
            sftSite.Connect();

            List<string> triggerFilePaths = GetTriggerFilePaths(sftSite);

            if (triggerFilePaths.Count == 0)
                return false;


            Parallel.ForEach(triggerFilePaths.Take(Config.Common_MaxProcessingLimit), (triggerFile) =>
            {
                string channelDownloadPath = Path.Combine(Config.Common_AuthoMailRootFolder, "GENERAL", SFTChannel.Code.Split('_')[0], triggerFile);
                using (var mailBatchSvc = new MailBatchService())
                using (var mailPieceSvc = new MailPieceService())
                using (var commonSvc = new CommonService())
                using (var clientSvc = new ClientService())
                using (var docTypeSvc = new DocTypeService())
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


                        if (Directory.Exists(channelDownloadPath))
                        {
                            Directory.Delete(channelDownloadPath, true);
                        }

                        ////To ensure that folder gets created.
                        //Thread.Sleep(1000);
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
                            ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, batchName, lastModifiedTime, msg), "");
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
                            ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, batchName, lastModifiedTime, msg), localZipFilePath);
                            return;
                        }
                        #endregion

                        SeparatePackages(clientSvc, mailBatchSvc, commonSvc, xmlFilePath, extractedDir, localZipFilePath, batchName, channelDownloadPath, lastModifiedTime, sftSite);
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured in EnumerateBatches Method For BatchName: {0} with Error : {1}".Fill(triggerFile, ex.Message);
                        log.Error(msg, ex);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                    }

                    if (Directory.Exists(channelDownloadPath))
                    {
                        Directory.Delete(channelDownloadPath, true);
                    }
                }
            });

            log.Debug("Enumerate Batches job queue for " + SFTChannel.Code + " is completed.");
            return true;
        }

        private void SeparatePackages(ClientService clientSvc, MailBatchService mailBatchSvc, CommonService commonSvc, string xmlFilePath, string extractedZipDirPath, string localZipFilePath, string batchName, string channelDownloadPath, DateTime receivedOnDateTime, SFTUtil sftSite)
        {
            XmlDocument batchXML = new XmlDocument();
            XmlNamespaceManager nsManager;
            XmlNodeList mailPieceNodes;
            try
            {
                batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                nsManager = new XmlNamespaceManager(batchXML.NameTable);
                nsManager.AddNamespace("x", "http://schemas.corelogic.com/ade/authomail/v1.18.0");
                mailPieceNodes = batchXML.DocumentElement.SelectSingleNode("x:MailPieces", nsManager).ChildNodes;
            }
            catch (XmlException ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, batchName, receivedOnDateTime, msg), localZipFilePath);
                return;
            }
            catch (Exception ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, batchName, receivedOnDateTime, msg), localZipFilePath);
                return;
            }

            var rootElement = batchXML.DocumentElement;
            var mbCustomInfoNode = rootElement.SelectSingleNode("x:CustomInfo", nsManager);
            var mbReturnInfoNode = rootElement.SelectSingleNode("x:ReturnInfo", nsManager);

            List<MailPiece> printAndShipNodes = new List<MailPiece>();
            List<MailPiece> eDeliveryNodes = new List<MailPiece>();

            var mailBatchData = XmlUtil.Deserialize<MailBatch>(File.ReadAllText(xmlFilePath));

            var batchMetaData = mailBatchData.CustomInfo;
            var batchReturnInfo = mailBatchData.ReturnInfo;

            string batchPath;
            string xmlPath;
            string errorMsg = string.Empty;

            eDeliveryNodes = mailBatchData.MailPieces.Where(x => x.eDelivery != null).ToList();
            printAndShipNodes = mailBatchData.MailPieces.Where(x => x.eDelivery == null).ToList();

            if (eDeliveryNodes.Count > 0)
            {
                //System generates individual batch for each mailpiece for e-delivery
                foreach (MailPiece eDNode in eDeliveryNodes)
                {
                    MailBatch newMailBatch = new MailBatch();
                    newMailBatch.ClientBatchId = mailBatchData.ClientBatchId;
                    newMailBatch.ClientCode = mailBatchData.ClientCode;
                    newMailBatch.CustomInfo = batchMetaData;
                    newMailBatch.DocumentOrPackageType = mailBatchData.DocumentOrPackageType;
                    newMailBatch.ReturnInfo = batchReturnInfo;
                    newMailBatch.Id = Guid.NewGuid(); // Here we are generating new guid for every batch

                    //Maintain original MailBatchId for E-Delivery request for the reference
                    newMailBatch.CustomInfo.Add(new Metadata() { Key = "OriginalMailBatchId", Value = mailBatchData.Id != Guid.Empty && mailBatchData.Id != null ? mailBatchData.Id.ToString() : string.Empty });

                    newMailBatch.MailPieces.Add(eDNode);

                    batchPath = SeparateEDeliveryXMLsAndPDFs(newMailBatch, eDNode.DocumentOrPackage, channelDownloadPath, extractedZipDirPath, batchName);
                    xmlPath = Path.Combine(batchPath, batchName + ".xml");
                    ProcessXML(clientSvc, mailBatchSvc, commonSvc, xmlPath, batchName, receivedOnDateTime, batchPath);
                }
            }

            #region Hold On Packages
            var appCode = mailBatchData.ClientCode;

            var packagesHoldInfo = Config.Monitor_PackagesHoldInfo.SingleOrDefault(x => x.ClientCode == appCode && x.HoldOn == receivedOnDateTime.UtcToEst().Date);

            if (printAndShipNodes.Count > 0)
            {
                if (packagesHoldInfo != null)
                {
                    if (packagesHoldInfo.DocumentOrPackageType.IsNullOrWhiteSpace())
                    {
                        if (printAndShipNodes.Count > 0)
                        {
                            MailBatch newMailBatch = new MailBatch();
                            newMailBatch.Id = mailBatchData.Id == null || mailBatchData.Id == Guid.Empty ? Guid.NewGuid() : mailBatchData.Id; // Here system will use same uniqueid which is received in current batch at batch level

                            errorMsg = SplitBatches(clientSvc, mailBatchSvc, commonSvc, mailBatchData, newMailBatch, batchMetaData, batchReturnInfo, printAndShipNodes, channelDownloadPath, extractedZipDirPath, batchName, receivedOnDateTime);
                        }
                    }
                    else
                    {
                        var mailPiecesByHoldDocumentType = printAndShipNodes.Where(x => x.DocumentOrPackageType == packagesHoldInfo.DocumentOrPackageType).ToList();
                        if (mailPiecesByHoldDocumentType.Count() > 0)
                        {
                            MailBatch newMailBatch = new MailBatch();
                            newMailBatch.Id = Guid.NewGuid(); // Here system will use new uniqueid which is received in current batch at batch level

                            errorMsg = SplitBatches(clientSvc, mailBatchSvc, commonSvc, mailBatchData, newMailBatch, batchMetaData, batchReturnInfo, mailPiecesByHoldDocumentType, channelDownloadPath, extractedZipDirPath, batchName, receivedOnDateTime);
                        }

                        var mailPiecesByRestOfDocumentTypes = printAndShipNodes.Where(x => x.DocumentOrPackageType != packagesHoldInfo.DocumentOrPackageType).ToList();
                        if (mailPiecesByRestOfDocumentTypes.Count() > 0)
                        {
                            MailBatch newMailBatch = new MailBatch();
                            newMailBatch.Id = mailBatchData.Id == null || mailBatchData.Id == Guid.Empty ? Guid.NewGuid() : mailBatchData.Id; // Here system will use same uniqueid which is received in current batch at batch level

                            errorMsg = SplitBatches(clientSvc, mailBatchSvc, commonSvc, mailBatchData, newMailBatch, batchMetaData, batchReturnInfo, mailPiecesByRestOfDocumentTypes, channelDownloadPath, extractedZipDirPath, batchName, receivedOnDateTime);
                        }
                    }
                }
                else
                {
                    MailBatch newMailBatch = new MailBatch();
                    newMailBatch.Id = mailBatchData.Id == null || mailBatchData.Id == Guid.Empty ? Guid.NewGuid() : mailBatchData.Id; // Here system will use same uniqueid which is received in current batch at batch level

                    errorMsg = SplitBatches(clientSvc, mailBatchSvc, commonSvc, mailBatchData, newMailBatch, batchMetaData, batchReturnInfo, printAndShipNodes, channelDownloadPath, extractedZipDirPath, batchName, receivedOnDateTime);
                }
            }
            #endregion

            if (errorMsg.IsNullOrWhiteSpace())
            {
                //Here after moving all batches to particular client location , archive amc batch
                string archiveZipPath = Path.Combine(Config.Common_AuthoMailRootFolder, "Archive", batchName + ".zip");

                if (File.Exists(archiveZipPath))
                    File.Delete(archiveZipPath);

                File.Move(localZipFilePath, archiveZipPath);

                //Now delete/archive trigger and zip from SFTP
                MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + ".zip"));
                MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + ".go"));
            }
            //After complete process clean AMC folder
            channelDownloadPath.CleanDirectory();
        }

        private string ProcessXML(ClientService clientSvc, MailBatchService mailBatchSvc, CommonService commonSvc, string xmlFilePath, string batchName, DateTime receivedOnDateTime, string batchPath)
        {
            XmlDocument batchXML = new XmlDocument();
            XmlNamespaceManager nsManager;
            XmlNodeList mailPieceNodes;
            try
            {
                batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                nsManager = new XmlNamespaceManager(batchXML.NameTable);
                nsManager.AddNamespace("x", "http://schemas.corelogic.com/ade/authomail/v1.18.0");
                mailPieceNodes = batchXML.DocumentElement.SelectSingleNode("x:MailPieces", nsManager).ChildNodes;
            }
            catch (XmlException ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, batchName, receivedOnDateTime, msg), xmlFilePath);
                return "";
            }
            catch (Exception ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, batchName, receivedOnDateTime, msg), xmlFilePath);
                return "";
            }

            var mailBatchData = XmlUtil.Deserialize<MailBatch>(File.ReadAllText(xmlFilePath));

            Client client;
            MailBatchDTO mailBatch;
            List<MailBatchCustomInfo> mailBatchCustomInfoList = new List<MailBatchCustomInfo>();
            try
            {
                if (mailBatchData.ClientCode.IsNullOrWhiteSpace())
                {
                    var msg = $"Client code is required in provided XML.";
                    ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, batchName, receivedOnDateTime, msg), xmlFilePath);
                    return "";
                }

                client = clientSvc.GetClientByAppCode(mailBatchData.ClientCode).Result;

                if (client == null)
                {
                    var msg = $"Client code {mailBatchData.ClientCode} is not valid from provided XML.";
                    ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, batchName, receivedOnDateTime, msg), xmlFilePath);
                    return "";
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
                    UniqueId = mailBatchData.Id,
                    Priority = mailBatchData.Priority.ToString(),
                    DocumentOrPackageType = mailBatchData.DocumentOrPackageType,
                    ClientBatchId = mailBatchData.ClientBatchId,
                    ServerIP = Config.Common_ServerIP
                };

                //AM Mail Batch Custom Info
                var customeInfoElement = mailBatchData.CustomInfo;
                if (customeInfoElement != null && customeInfoElement.Count > 0)
                {
                    mailBatchCustomInfoList.AddRange(customeInfoElement.Select(x => new MailBatchCustomInfo() { Key = x.Key, DataType = x.DataType.ToString(), Value = x.Value }));
                }

                //AM Mail Batch Return Info
                var returnInfoElement = mailBatchData.ReturnInfo;
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
                        mailBatch.RetRcptAddressCountry = returnAddressElement.Country;
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
                return msg;
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
                    Directory.Move(batchPath, mbi.AMRequestPath);

                    commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now).Wait();

                    scope.Complete();

                }
                catch (SqlException ex)
                {
                    //2601 error code used for duplicate unique key insert 
                    if (ex.Number == 2601)
                    {
                        var msg = "{0} key id is duplicate in received batch.".Fill(mailBatch.UniqueId);
                        log.Error(msg, ex);
                        MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));

                        log.Error(msg, ex);

                        ReportInvalidBatch(clientSvc, commonSvc, InsertInvalidBatchInfo(commonSvc, mailBatchSvc, clientSvc, mailBatch.BatchName, mailBatch.ReceivedOn.Value, msg), xmlFilePath);
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while inserting General Channel batches details for Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));

                    return msg;
                }
            }

            return "";
        }

        private string SeparateEDeliveryXMLsAndPDFs(MailBatch newMailBatch, DocumentOrPackage documentOrPackageElement, string channelDownloadPath, string receivedBatchDirPath, string batchName)
        {
            //Folder Structure will be like AMRoot/GENERAL/{ChannelCode}/{ClientCode}/{UniqueId}
            string clientBatchDirectory = Path.Combine(channelDownloadPath, newMailBatch.Id.ToString());

            clientBatchDirectory.CleanDirectory();

            string amFilePath = Path.Combine(clientBatchDirectory, string.Format("{0}.xml", batchName));

            string fileName;

            if (documentOrPackageElement == null)
                fileName = string.Empty;
            else if (documentOrPackageElement.Name.IsNullOrWhiteSpace())
            {
                if (documentOrPackageElement.Pattern.IsNullOrWhiteSpace())
                {
                    fileName = string.Empty;
                }
                else
                {
                    var dirFiles = Directory.EnumerateFiles(channelDownloadPath, "*" + documentOrPackageElement.Pattern + "*.pdf");

                    if (dirFiles.Count() > 0)
                    {
                        fileName = Path.GetFileName(dirFiles.Single());
                    }
                    else
                    {
                        fileName = string.Empty;
                    }
                }
            }
            else
            {
                fileName = documentOrPackageElement.Name;
            }

            if (!string.IsNullOrEmpty(fileName) && File.Exists(Path.Combine(receivedBatchDirPath, fileName)))
            {
                string sourceFile = Path.Combine(receivedBatchDirPath, fileName);
                string destFile = Path.Combine(clientBatchDirectory, fileName);
                File.Copy(sourceFile, destFile, true);
            }

            File.WriteAllText(amFilePath, XmlUtil.Serialize(newMailBatch));

            return clientBatchDirectory;
        }

        private string SeparatePrintNShipXMLsAndPDFs(MailBatch newMailBatch, string channelDownloadPath, string receivedBatchDirPath, string batchName)
        {
            //Folder Structure will be like AMRoot/GENERAL/{ChannelCode}/{UniqueId}
            string clientBatchDirectory = Path.Combine(channelDownloadPath, $"PNS_{newMailBatch.Id}");

            clientBatchDirectory.CleanDirectory();

            string amFilePath = Path.Combine(clientBatchDirectory, string.Format("{0}.xml", batchName));

            foreach (var mailPiece in newMailBatch.MailPieces)
            {
                var documentOrPackageElement = mailPiece.DocumentOrPackage;
                string fileName;
                if (documentOrPackageElement == null)
                    fileName = string.Empty;
                else if (documentOrPackageElement.Name == null)
                {
                    if (documentOrPackageElement.Pattern == null)
                    {
                        fileName = string.Empty;
                    }
                    else
                    {
                        var dirFiles = Directory.EnumerateFiles(channelDownloadPath, "*" + documentOrPackageElement.Pattern + "*.pdf");

                        if (dirFiles.Count() > 0)
                        {
                            fileName = Path.GetFileName(dirFiles.Single());
                        }
                        else
                        {
                            fileName = string.Empty;
                        }
                    }
                }
                else
                {
                    fileName = documentOrPackageElement.Name;
                }

                if (!string.IsNullOrEmpty(fileName) && File.Exists(Path.Combine(receivedBatchDirPath, fileName)))
                {
                    string sourceFile = Path.Combine(receivedBatchDirPath, fileName);
                    string destFile = Path.Combine(clientBatchDirectory, fileName);
                    File.Copy(sourceFile, destFile, true);
                }
            }

            File.WriteAllText(amFilePath, XmlUtil.Serialize(newMailBatch));

            return clientBatchDirectory;
        }

        private string SplitBatches(ClientService clientSvc, MailBatchService mailBatchSvc, CommonService commonSvc, MailBatch mailBatchData, MailBatch newMailBatch, List<Metadata> batchMetaData, AM.Entities.ReturnInfo batchReturnInfo, List<MailPiece> printAndShipNodes, string channelDownloadPath, string extractedZipDirPath, string batchName, DateTime receivedOnDateTime)
        {
            newMailBatch.ClientBatchId = mailBatchData.ClientBatchId;
            newMailBatch.ClientCode = mailBatchData.ClientCode;
            newMailBatch.CustomInfo = batchMetaData;
            newMailBatch.DocumentOrPackageType = mailBatchData.DocumentOrPackageType;
            newMailBatch.ReturnInfo = batchReturnInfo;

            newMailBatch.MailPieces = printAndShipNodes;

            string batchPath = SeparatePrintNShipXMLsAndPDFs(newMailBatch, channelDownloadPath, extractedZipDirPath, batchName);
            string xmlPath = Path.Combine(batchPath, batchName + ".xml");
            return ProcessXML(clientSvc, mailBatchSvc, commonSvc, xmlPath, batchName, receivedOnDateTime, batchPath);
        }
        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string localZipFilePath)
        {
            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            return localZipFilePath;
        }

        private void ReportInvalidBatch(ClientService clientSvc, CommonService commonSvc, MailBatchDTO mailBatch, string localZipFilePath)
        {
            Client client = clientSvc.GetClientById(mailBatch.ClientId);

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();

            if (!string.IsNullOrEmpty(localZipFilePath))
                File.Move(localZipFilePath, Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".zip"));

            string errorReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

            using (IGeneralService generalSvc = new GeneralService())
            {
                string responseReport = generalSvc.GeneralXMLBatchErrorReport(mbi.MailBatch, mbi.Client.AppCode);

                if (!string.IsNullOrWhiteSpace(responseReport))
                {
                    File.WriteAllText(errorReportPath, responseReport);
                }
            }

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

            string remoteFilePath = Path.Combine(deliverable.UploadPath, errorReportFileName);

            var mailBatchSFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mailBatch.SFTChannelId).Result;

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(mailBatchSFTChannel.Protocol), mailBatchSFTChannel.Host, mailBatchSFTChannel.UserName, mailBatchSFTChannel.Password, 300000, SFTChannel.Port);
            sftSite.Connect();

            if (client.IsResponseRequired)
            {
                sftSite.UploadFile(errorReportPath, remoteFilePath);
            }

            if (sftSite.IsFileAvailable(Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".zip")))
                MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".zip"));

            MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".go"));

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();
        }

        private void MoveBatchToBackupDir(SFTUtil sftSite, string ftpFilePath)
        {
            if (sftSite.IsFileAvailable(ftpFilePath))
            {
                if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                {
                    sftSite.MoveFile(ftpFilePath, Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(ftpFilePath)));
                }
                else
                    sftSite.DeleteFile(ftpFilePath);
            }
        }

        private MailBatchDTO InsertInvalidBatchInfo(CommonService commonSvc, MailBatchService mailBatchSvc, ClientService clientSvc, string batchName, DateTime receivedOn, string errorMessage)
        {
            MailBatchDTO mailBatch = new MailBatchDTO();
            mailBatch.UniqueId = Guid.NewGuid();
            mailBatch.ClientId = GetInvalidClient(clientSvc, SFTChannel.GetChannelCode()).Id;
            mailBatch.SFTChannelId = SFTChannel.Id;
            mailBatch.BatchName = batchName;
            mailBatch.ClientBatchId = batchName;
            mailBatch.Priority = Priority.Low.ToString();
            mailBatch.ServerIP = Config.Common_ServerIP;
            mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);
            commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, receivedOn).Wait();
            mailBatchSvc.MarkMailBatchAsInvalid(mailBatch.Id, errorMessage).Wait();
            commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.Failed.ToString()).Wait();
            mailBatch.ValidationErrors = errorMessage;
            return mailBatch;
        }

        private Client GetInvalidClient(ClientService clientSvc, string sftChannelCode)
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
        #endregion

        protected override bool DownloadBatches()
        {
            return false;
        }

        private ConcurrentDictionary<int, ConcurrentDictionary<string, bool>> clientBusinessIdNames = new ConcurrentDictionary<int, ConcurrentDictionary<string, bool>>(Environment.ProcessorCount * 2, 60);

        #region Validate Batches
        protected override bool ValidateBatches()
        {
            log.Debug("Validate job queue is started.");

            using (var mailBatchService = new MailBatchService())
            {

                List<MailBatchDTO> downloadBatches = mailBatchService.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Downloaded, SFTChannel.Id);

                if (downloadBatches.Count == 0)
                {
                    log.Debug("Validate job queue is empty.");
                    return false;
                }

                Parallel.ForEach(downloadBatches, (mailBatch) =>
                {
                    List<double> timeTracker = new List<double>();
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    log.Debug($"Parallel Process Started For Batch Id {mailBatch.Id}");
                    //using (var mailBatchSvc = new MailBatchService())
                    //using (var mailPieceSvc = new MailPieceService())
                    using (var commonSvc = new CommonService())
                    using (var clientSvc = new ClientService())
                    using (var docTypeSvc = new DocTypeService())
                    {
                        try
                        {
                            using (var mailBatchSvc = new MailBatchService())
                            {
                                //NOTE: Here system will delete if any existing records are there with MailBatchId in mail pieces table
                                mailBatchSvc.DeleteMailPiecesDetailsByMailBatchId(mailBatch.Id);
                            }

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

                                ReportInvalidBatch(clientSvc, commonSvc, mailBatch, "");

                                return;
                            }
                            catch (Exception ex)
                            {
                                var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                                log.Error(msg, ex);
                                ReportInvalidBatch(clientSvc, commonSvc, mailBatch, "");
                                return;
                            }

                            var businessIdNames = clientBusinessIdNames.GetOrAdd(client.Id, new ConcurrentDictionary<string, bool>(Environment.ProcessorCount * 2, 60));

                            sw.Stop();
                            timeTracker.Add(sw.Elapsed.TotalSeconds);
                            sw.Restart();
                            foreach (XmlNode mailPieceNode in mailPieceNodes)
                            {
                                //mailPieceNode.Normalize();
                                MailPieceDTO mailPieceDTO = new MailPieceDTO();
                                MailPieceCustomInfo customInfo = null;
                                List<MailPieceCustomInfo> mailPieceCustomInfo = new List<MailPieceCustomInfo>();

                                mailPieceDTO.ClientId = client.Id;
                                mailPieceDTO.MailBatchId = mbi.MailBatch.Id;
                                mailPieceDTO.ShippingStatus = ShippingStatus.InProgress.ToString();

                                var requests = XmlUtil.Deserialize<MailPiece>(mailPieceNode.OuterXml);
                                try
                                {
                                    mailPieceDTO.UniqueId = requests.Id == null || requests.Id == Guid.Empty ? Guid.NewGuid() : requests.Id;
                                    mailPieceDTO.BusinessFriendlyIdName = requests.BusinessFriendlyIdName.TrimIfNotNull();
                                    mailPieceDTO.BusinessFriendlyIdValue = requests.BusinessFriendlyIdValue.TrimIfNotNull();
                                    mailPieceDTO.DocumentOrPackageType = requests.DocumentOrPackageType == null ? mailBatch.DocumentOrPackageType : requests.DocumentOrPackageType;

                                    mailPieceDTO.FileName = requests.DocumentOrPackage.Name.TrimIfNotNull();
                                    mailPieceDTO.FilePattern = requests.DocumentOrPackage.Pattern.TrimIfNotNull();
                                    mailPieceDTO.Inserts = requests.DocumentOrPackage.Inserts.TrimIfNotNull();

                                    //Here we are inserting BusinessIdentifier for Client if not exist.
                                    if (!businessIdNames.ContainsKey(mailPieceDTO.BusinessFriendlyIdName))
                                    {
                                        try
                                        {
                                            using (ICommonService commonService = new CommonService())
                                            {
                                                ClientBusinessIdentifier clientBusinessIdDetails = commonService.CheckBusinessIdNameAlreadyExist(client.Id, mailPieceDTO.BusinessFriendlyIdName);
                                                if (clientBusinessIdDetails == null)
                                                {
                                                    commonService.InsertBusinessIdentifierName(client.Id, mailPieceDTO.BusinessFriendlyIdName.TrimIfNotNull());
                                                }
                                            }
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 2601) // Cannot insert duplicate key row in object error
                                            {
                                                var msg = "Error occured in ValidateBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                                                log.Error(msg, ex);
                                                MailUtility.SendUnexpectedExceptionEmail(msg);
                                                break;
                                            }

                                            if (ex.Number == -2)
                                            {
                                                //If Timeout exception occured while processing batch then we are re-processing whole batch completly and breaking out from foreach loop.
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            var msg = "Error occured in ValidateBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                                            log.Error(msg, ex);
                                            MailUtility.SendUnexpectedExceptionEmail(msg);
                                            break;
                                        }

                                        businessIdNames.TryAdd(mailPieceDTO.BusinessFriendlyIdName, true);
                                    }


                                    if (!string.IsNullOrEmpty(mailPieceDTO.FilePattern))
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

                                            mailPieceDTO.RcptFirstName = requests.RecipientInfo.Recipient.FirstName.TrimIfNotNull();
                                            mailPieceDTO.RcptLastName = requests.RecipientInfo.Recipient.LastName.TrimIfNotNull();
                                            mailPieceDTO.RcptMiddleName = requests.RecipientInfo.Recipient.MiddleName.TrimIfNotNull();
                                            mailPieceDTO.RcptOrganization = requests.RecipientInfo.Recipient.Organization.TrimIfNotNull();
                                            mailPieceDTO.RcptPrefix = requests.RecipientInfo.Recipient.Prefix.TrimIfNotNull();
                                            mailPieceDTO.RcptSuffix = requests.RecipientInfo.Recipient.Suffix.TrimIfNotNull();
                                            mailPieceDTO.RcptPhone = requests.RecipientInfo.Recipient.Phone.TrimIfNotNull();
                                            mailPieceDTO.RcptEmailId = requests.RecipientInfo.Recipient.EmailAddress.TrimIfNotNull();
                                        }
                                        else
                                            mailPieceDTO.IsRcptInfoSpecified = false;

                                        if (requests.RecipientInfo.Address != null)
                                        {
                                            mailPieceDTO.IsRcptInfoSpecified = true;

                                            mailPieceDTO.RcptAddrOptionalLine1AboveStreet = requests.RecipientInfo.Address.OptionalLine1AboveStreet.TrimIfNotNull();
                                            mailPieceDTO.RcptAddrOptionalLine2AboveStreet = requests.RecipientInfo.Address.OptionalLine2AboveStreet.TrimIfNotNull();

                                            mailPieceDTO.RcptAddressStreet = requests.RecipientInfo.Address.Street.TrimIfNotNull();
                                            mailPieceDTO.RcptAddressAptOrSuiteOrOther = requests.RecipientInfo.Address.AptOrSuiteOrOther.TrimIfNotNull();

                                            mailPieceDTO.RcptAddrOptionalLine1BelowStreet = requests.RecipientInfo.Address.OptionalLine1BelowStreet.TrimIfNotNull();
                                            mailPieceDTO.RcptAddrOptionalLine2BelowStreet = requests.RecipientInfo.Address.OptionalLine2BelowStreet.TrimIfNotNull();

                                            mailPieceDTO.RcptAddressCity = requests.RecipientInfo.Address.City.TrimIfNotNull();
                                            mailPieceDTO.RcptAddressState = requests.RecipientInfo.Address.State.TrimIfNotNull();
                                            mailPieceDTO.RcptAddressZIP = requests.RecipientInfo.Address.Zip.TrimIfNotNull();
                                            mailPieceDTO.RcptAddressCountry = requests.RecipientInfo.Address.Country.TrimIfNotNull();
                                        }
                                    }
                                    else
                                    {
                                        mailPieceDTO.IsRcptInfoSpecified = false;
                                    }

                                    if (requests.eDelivery == null)
                                    {
                                        mailPieceDTO.IsEDeliveryRequested = false;
                                        mailPieceDTO.IsFallBackRequested = false;
                                    }
                                    else
                                    {
                                        mailPieceDTO.IsEDeliveryRequested = true;
                                        mailPieceDTO.IsFallBackRequested = requests.eDelivery.PrintAndShipOnExpiry;
                                        if (requests.eDelivery.ExpiryDate == null || requests.eDelivery.ExpiryDate == DateTime.MinValue)
                                        {
                                            if (client.SigningRoomExpiryDays >= 0)
                                            {
                                                mailPieceDTO.ExpiryDate = GetDefaultExpiryDate(commonSvc, mailBatch.ReceivedOn.Value.UtcToEst(), client.SigningRoomExpiryDays).ToEndDateTime().EstToUtc();
                                            }
                                            else
                                            {
                                                mailPieceDTO.ExpiryDate = null;
                                            }
                                        }
                                        else
                                        {
                                            mailPieceDTO.ExpiryDate = requests.eDelivery.ExpiryDate.ToEndDateTime().EstToUtc();
                                        }
                                    }

                                    if (requests.ReturnInfo == null)
                                    {
                                        mailPieceDTO.IsReturnInfoSpecified = false;
                                        //if at mail piece level system didn't find return info then it will check for MailBatch level if exist system will use that in mail piece level 
                                        if (mailBatch.IsReturnInfoSpecified)
                                        {
                                            mailPieceDTO.RetRcptFirstName = mailBatch.RetRcptFirstName.TrimIfNotNull();
                                            mailPieceDTO.RetRcptLastName = mailBatch.RetRcptLastName.TrimIfNotNull();

                                            mailPieceDTO.RetRcptMiddleName = mailBatch.RetRcptMiddleName.TrimIfNotNull();
                                            mailPieceDTO.RetRcptOrganization = mailBatch.RetRcptOrganization.TrimIfNotNull();
                                            mailPieceDTO.RetRcptPrefix = mailBatch.RetRcptPrefix.TrimIfNotNull();
                                            mailPieceDTO.RetRcptSuffix = mailBatch.RetRcptSuffix.TrimIfNotNull();
                                            mailPieceDTO.RetRcptPhone = mailBatch.RetRcptPhone.TrimIfNotNull();

                                            mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = mailBatch.RetRcptAddrOptionalLine1AboveStreet.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = mailBatch.RetRcptAddrOptionalLine2AboveStreet.TrimIfNotNull();

                                            mailPieceDTO.RetRcptAddressStreet = mailBatch.RetRcptAddressStreet.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = mailBatch.RetRcptAddressAptOrSuiteOrOther.TrimIfNotNull();

                                            mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = mailBatch.RetRcptAddrOptionalLine1BelowStreet.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = mailBatch.RetRcptAddrOptionalLine2BelowStreet.TrimIfNotNull();

                                            mailPieceDTO.RetRcptAddressCity = mailBatch.RetRcptAddressCity.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddressState = mailBatch.RetRcptAddressState.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddressZIP = mailBatch.RetRcptAddressZIP.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddressCountry = mailBatch.RetRcptAddressCountry.TrimIfNotNull();
                                        }
                                    }
                                    else
                                    {
                                        if (requests.ReturnInfo.Recipient != null)
                                        {
                                            mailPieceDTO.IsReturnInfoSpecified = true;
                                            mailPieceDTO.RetRcptFirstName = requests.ReturnInfo.Recipient.FirstName.TrimIfNotNull();
                                            mailPieceDTO.RetRcptLastName = requests.ReturnInfo.Recipient.LastName.TrimIfNotNull();

                                            mailPieceDTO.RetRcptMiddleName = requests.ReturnInfo.Recipient.MiddleName.TrimIfNotNull();
                                            mailPieceDTO.RetRcptOrganization = requests.ReturnInfo.Recipient.Organization.TrimIfNotNull();
                                            mailPieceDTO.RetRcptPrefix = requests.ReturnInfo.Recipient.Prefix.TrimIfNotNull();
                                            mailPieceDTO.RetRcptSuffix = requests.ReturnInfo.Recipient.Suffix.TrimIfNotNull();
                                            mailPieceDTO.RetRcptPhone = requests.ReturnInfo.Recipient.Phone.TrimIfNotNull();
                                        }
                                        else
                                            mailPieceDTO.IsReturnInfoSpecified = false;

                                        if (requests.ReturnInfo.Address != null)
                                        {
                                            mailPieceDTO.IsReturnInfoSpecified = true;
                                            mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = requests.ReturnInfo.Address.OptionalLine1AboveStreet.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = requests.ReturnInfo.Address.OptionalLine2AboveStreet.TrimIfNotNull();

                                            mailPieceDTO.RetRcptAddressStreet = requests.ReturnInfo.Address.Street.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = requests.ReturnInfo.Address.AptOrSuiteOrOther.TrimIfNotNull();

                                            mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = requests.ReturnInfo.Address.OptionalLine1BelowStreet.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = requests.ReturnInfo.Address.OptionalLine2BelowStreet.TrimIfNotNull();

                                            mailPieceDTO.RetRcptAddressCity = requests.ReturnInfo.Address.City.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddressState = requests.ReturnInfo.Address.State.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddressZIP = requests.ReturnInfo.Address.Zip.TrimIfNotNull();
                                            mailPieceDTO.RetRcptAddressCountry = requests.ReturnInfo.Address.Country.TrimIfNotNull();
                                        }
                                    }

                                    if (requests.CustomInfo != null)
                                    {
                                        foreach (var cstmInfo in requests.CustomInfo)
                                        {
                                            customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = cstmInfo.Key.TrimIfNotNull(), DataType = cstmInfo.DataType, Value = cstmInfo.Value.TrimIfNotNull() };

                                            mailPieceCustomInfo.Add(customInfo);
                                        }
                                        mailPieceDTO.MailPieceCustomInfo = mailPieceCustomInfo;
                                    }
                                    //First System will check for schema validation
                                    MailPieceValidationResult schemaValidationResult = ValidateMailPieceNode(mailPieceNode);
                                    if (schemaValidationResult.Code == OutComeCodes.No_Error)
                                    {
                                        //System will check for common validations
                                        MailPieceValidationResult commonValidationResult = ValidateCommonValidation(docTypeSvc, mailPieceDTO, mbi);
                                        if (commonValidationResult.Code == OutComeCodes.No_Error)
                                        { // System will check for client specific validations
                                            MailPieceValidationResult clientSpecificValidationResult = ValidateClientSpecificRules(client, mailPieceDTO);
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

                                    if (requests.CustomInfo != null)
                                    {
                                        mailPieceCustomInfo = new List<MailPieceCustomInfo>();

                                        foreach (var cstmInfo in requests.CustomInfo)
                                        {
                                            customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = cstmInfo.Key.TrimIfNotNull(), DataType = cstmInfo.DataType, Value = cstmInfo.Value.TrimIfNotNull() };

                                            mailPieceCustomInfo.Add(customInfo);
                                        }
                                        mailPieceDTO.MailPieceCustomInfo = mailPieceCustomInfo;
                                    }

                                    mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPieceDTO.ErrorDesc = "Error occured while processing package with error {0}".Fill(ex.Message);
                                    mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                }

                                using (var mailPieceSvc = new MailPieceService())
                                {
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
                            }
                            sw.Stop();
                            timeTracker.Add(sw.Elapsed.TotalSeconds);
                            sw.Restart();
                            //TODO: Do we need to check all mail pieces are valid, partially valid or failed and update batch status according to that?

                            using (var commonService = new CommonService())
                            using (var mailPieceSvc = new MailPieceService())
                            {
                                var mailPieceList = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                                if (mailPieceList != null)
                                {
                                    var validMailPieces = mailPieceList.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();
                                    sw.Stop();
                                    timeTracker.Add(sw.Elapsed.TotalSeconds);
                                    sw.Restart();
                                    //Here System will validate Addresses for valid mail pieces
                                    if (validMailPieces.Count > 0)
                                        PChannel.ValidateAddressType(commonSvc, docTypeSvc, mailPieceSvc, mailBatch, validMailPieces);

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

                                PChannel.CalculateDueDateAndPrintLocation(client, mailBatch, mailPieceList);

                                if (mailPieceList.All(mp => mp.IsEDeliveryRequested && mp.ShippingStatus != ShippingStatus.Error.ToString()))
                                {
                                    commonService.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.SubmittedToEDelivery, Common.Now).Wait();
                                    commonService.UpdateEDeliveryPipelineStatus(mbi.MailBatch.Id, EDPipelineStatus.Validated, Common.Now);
                                }
                                else
                                    commonService.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now).Wait();
                            }
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured in ValidateBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedExceptionEmail(msg);
                            return;
                        }
                    }
                    sw.Stop();
                    timeTracker.Add(sw.Elapsed.TotalSeconds);
                    log.Debug($"Time Tracker For Parallel Foreach {string.Join(",", timeTracker)}");
                    log.Debug($"Parallel Process Completed For Batch Id {mailBatch.Id}");
                });

            }
            log.Debug("Validate job queue is completed.");
            return true;
        }

        private DateTime GetDefaultExpiryDate(CommonService commonSvc, DateTime receivedOnEst, int expiryDays)
        {
            DateTime expiryDate = receivedOnEst.AddDays(expiryDays);

            if (!commonSvc.IsBusinessDay(expiryDate, Config.Common_FederalHolidays, false, false))
            {
                expiryDate = commonSvc.GetNextWorkingDay(expiryDate, Config.Common_FederalHolidays, false, false);
            }

            return expiryDate;
        }

        private MailPieceValidationResult ValidateCommonValidation(DocTypeService docTypeSvc, MailPieceDTO mailPieceDTO, MailBatchInfo mbi)
        {
            bool allowInsertsOnly = docTypeSvc.AllowInsertsOnlyDocumentType(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType);

            //in last system will check for general validations
            string localFilePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);

            if (!File.Exists(localFilePath) && !allowInsertsOnly)
            {
                return new MailPieceValidationResult() { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf file is not available for this package." };
            }

            int pageCount = 0;
            List<int> blankPageNos = new List<int>();

            if (File.Exists(localFilePath))
            {
                using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(localFilePath))
                {
                    pageCount = reader.NumberOfPages;
                    mailPieceDTO.TotalPages = pageCount;//System is going to store TotalPages only for AM.EDMailPieces table from here

                    if (pageCount == 0)
                    {
                        return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = "Pdf file is either corrupt or does not contain any page." };
                    }

                    if (mbi.Client.IsReqBlankPageValidation)
                    {
                        blankPageNos = PChannel.CheckIfPdfBlank(pageCount, blankPageNos, localFilePath, reader);
                    }
                }

                //NOTE : Here system is storing file size of received pdf in KB
                FileInfo fileInfo = new FileInfo(localFilePath);
                mailPieceDTO.PDFFileSize = (fileInfo.Length / 1024);
            }
            else if (allowInsertsOnly)
            {
                mailPieceDTO.TotalPages = 0;
            }


            //NOTE : Here we are considering that by default receivedon will be original one but if any package that is for Hold then we are considering its effective receivedon date based on ProcessOn
            DateTime receivedOn = mbi.MailBatch.ReceivedOn.Value; ;

            if (Config.Monitor_PackagesHoldInfo.Any(x => x.ClientId == mailPieceDTO.ClientId && x.HoldOn == receivedOn.UtcToEst().Date))
            {
                var packageHoldInfo = Config.Monitor_PackagesHoldInfo.SingleOrDefault(x => x.ClientId == mailPieceDTO.ClientId && x.HoldOn == receivedOn.UtcToEst().Date);
                if (packageHoldInfo.DocumentOrPackageType.IsNullOrWhiteSpace())
                {
                    receivedOn = packageHoldInfo.ProcessOn.EstToUtc();
                }
                else if (packageHoldInfo.DocumentOrPackageType == mailPieceDTO.DocumentOrPackageType)
                {
                    receivedOn = packageHoldInfo.ProcessOn.EstToUtc();
                }
                else
                {
                    receivedOn = mbi.MailBatch.ReceivedOn.Value;
                }
            }
            //Document Or Package Type validation

            bool isCountryUS = mailPieceDTO.RcptAddressCountry.IsCountryUS();
            var docType = !isCountryUS ? docTypeSvc.GetIntlDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType, pageCount, receivedOn.UtcToEst()) : docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType, pageCount, receivedOn.UtcToEst());
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
            else if (pageCount != 0 && pageCount == blankPageNos.Count)
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

            if (mailPieceDTO.IsEDeliveryRequested && mailPieceDTO.ShippingStatus != ShippingStatus.Error.ToString())
            {
                if (!mbi.Client.IsEDeliveryEnabled)
                {
                    return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = "Client has not opted for E-Delivery service." };
                }
                else if (string.IsNullOrWhiteSpace(mailPieceDTO.RcptEmailId) && mailPieceDTO.IsEDeliveryRequested)//Here system will validate if EDelivery is requested then EmailId is required, if not exist then error out
                {
                    return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient Email address is not specified for E-Delivery request.".Fill(mailPieceDTO.DocumentOrPackageType) };
                }
                else if (mailPieceDTO.ExpiryDate == null && mailPieceDTO.IsEDeliveryRequested)//Here system will validate if EDelivery is requested then Expiry date is required, if not exist then error out
                {
                    return new MailPieceValidationResult() { Code = OutComeCodes.Issue_In_EDelivery, ErrorMessage = "Client has not provided expiry date or expiry days while creating signing room.".Fill(mailPieceDTO.DocumentOrPackageType) };
                }
                else if (mailPieceDTO.ExpiryDate != null && mailPieceDTO.IsEDeliveryRequested && mailPieceDTO.ExpiryDate.Value < Common.EstToday)//Here system will validate if EDelivery is requested then Expiry date must be greater than or equal to current date otherwise failed.
                {
                    return new MailPieceValidationResult() { Code = OutComeCodes.Issue_In_EDelivery, ErrorMessage = "The expiration date must be greater than or equal to the current date." };
                }
                else
                {
                    return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
                }
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };

        }

        private MailPieceValidationResult ValidateClientSpecificRules(Client client, MailPieceDTO mailPiece)
        {
            var errors = GetAllMailPieceBasicErrors(client, mailPiece).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }

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

            if (errorMsg.Contains("Street") || errorMsg.Contains("City") || errorMsg.Contains("State") || errorMsg.Contains("Zip"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address details are missing or invalid in package." };

            if (errorMsg.Contains("Recipient") || errorMsg.Contains("FirstName") || errorMsg.Contains("LastName"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient details are missing or invalid in package." };

            if (!string.IsNullOrEmpty(errorMsg))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = errorMsg };

            yield break;
        }

        private IEnumerable<MailPieceValidationResult> GetAllMailPieceBasicErrors(Client client, MailPieceDTO mailPiece)
        {
            if (mailPiece.RcptAddressStreet.IsNullOrWhiteSpace() || mailPiece.RcptAddressCity.IsNullOrWhiteSpace() || mailPiece.RcptAddressState.IsNullOrWhiteSpace() || mailPiece.RcptAddressZIP.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address details are missing or invalid in package." };

            if (mailPiece.RetRcptAddressStreet.IsNullOrWhiteSpace() || mailPiece.RetRcptAddressCity.IsNullOrWhiteSpace() || mailPiece.RetRcptAddressState.IsNullOrWhiteSpace() || mailPiece.RetRcptAddressZIP.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return address details are missing or invalid in package." };

            if (mailPiece.RcptOrganization.IsNullOrWhiteSpace() && mailPiece.RcptFirstName.IsNullOrWhiteSpace() && mailPiece.RcptLastName.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient's first and last name are missing or invalid in package." };

            if (mailPiece.DocumentOrPackageType.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = "Document type is not specified in package." };

            if (mailPiece.BusinessFriendlyIdName.IsNullOrWhiteSpace() || mailPiece.BusinessFriendlyIdValue.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_BusinessIdentifier, ErrorMessage = "Business friendly Id or value is not specified in package." };

            if (mailPiece.FileName.IsNullOrWhiteSpace() && mailPiece.FilePattern.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf file is not provided in package." };


            if (!mailPiece.RcptAddressZIP.IsNullOrWhiteSpace() && (mailPiece.RcptAddressCountry.IsNullOrWhiteSpace() || mailPiece.RcptAddressCountry.IsCountryUS()))
            {
                if (!Regex.Match(mailPiece.RcptAddressZIP, @"^(?(^00000(|-0000))|\d{5}(?:|[-\s]+\d{4}))$").Success)
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Zipcode is in invalid format." };
                }
            }

            if (!mailPiece.RetRcptAddressZIP.IsNullOrWhiteSpace() && (mailPiece.RetRcptAddressCountry.IsNullOrWhiteSpace() || mailPiece.RcptAddressCountry.IsCountryUS()))
            {
                if (!Regex.Match(mailPiece.RetRcptAddressZIP, @"^(?(^00000(|-0000))|\d{5}(?:|[-\s]+\d{4}))$").Success)
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return Zipcode is in invalid format." };
                }
            }

            if (client.GroupAlias == "Flood Integration")
            {
                if (mailPiece.MailPieceCustomInfo.SingleOrDefault(m => m.Key == "PropertyStreetAddress").Value.IsNullOrWhiteSpace() || mailPiece.MailPieceCustomInfo.SingleOrDefault(m => m.Key == "PropertyCity").Value.IsNullOrWhiteSpace() || mailPiece.MailPieceCustomInfo.SingleOrDefault(m => m.Key == "PropertyState").Value.IsNullOrWhiteSpace() || mailPiece.MailPieceCustomInfo.SingleOrDefault(m => m.Key == "PropertyZip").Value.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Property_Address_Missing_Or_Invalid, ErrorMessage = "Property address details are missing or invalid in package." };

                if (mailPiece.MailPieceCustomInfo.SingleOrDefault(m => m.Key == "DocumentDate").Value.AnyFormatToDateTime() == DateTime.MinValue)
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_DocumentDate, ErrorMessage = "Document date is not specified in package." };

                if (mailPiece.MailPieceCustomInfo.SingleOrDefault(m => m.Key == "NoticeDate").Value.AnyFormatToDateTime() == DateTime.MinValue)
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_NoticeDate, ErrorMessage = "Notice date is not specified in package." };

                if (mailPiece.MailPieceCustomInfo.SingleOrDefault(m => m.Key == "CommunityName").Value.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_CommunityName, ErrorMessage = "Community name is not specified in package." };

                //if (mailPiece.MailPieceCustomInfo.SingleOrDefault(m => m.Key == "CommunityCode").Value.IsNullOrWhiteSpace())
                //    yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_ShippingDetail, ErrorMessage = "Community code is not specified in package." };
            }

            yield break;
        }
        #endregion

        protected override bool AcknowledgeBatches()
        {
            log.Debug("Acknowledge job queue is started.");

            using (var mailBatchService = new MailBatchService())
            {
                List<MailBatchDTO> validatedBatches = mailBatchService.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Validated, SFTChannel.Id);

                if (validatedBatches.Count == 0)
                {
                    log.Debug("Acknowledge job queue is empty.");
                    return false;
                }

                Parallel.ForEach(validatedBatches, (mailBatch) =>
                {
                    using (var mailBatchSvc = new MailBatchService())
                    using (var mailPieceSvc = new MailPieceService())
                    using (var commonSvc = new CommonService())
                    using (var clientSvc = new ClientService())
                    using (var docTypeSvc = new DocTypeService())
                    using (var generalSvc = new GeneralService())
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
                                    var emailErrorReportPath = UploadBaseReportToClient(commonSvc, generalSvc, mbi, mailPieces, deliverable);

                                    MailUtility.SendDownloadNotificationEmail(mbi, mailPieces, emailErrorReportPath, deliverable.EmailChannel);
                                    //After upload change extension of local file Path
                                    if (File.Exists(emailErrorReportPath) && mbi.MailBatch.ValidationStatus != BatchValidationStatus.Invalid)
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
                        }
                    }
                });
            }

            log.Debug("Acknowledge job queue is completed.");

            return true;
        }

        private string UploadBaseReportToClient(CommonService commonSvc, GeneralService generalSvc, MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            string errorReportFileName = "";
            string errorData;
            string errorReportPath;
            string zipFileName, zipFilePath;
            var erroredMPs = mailPieces.Where(mp => mp.ShippingStatus == ShippingStatus.Error.ToString()).ToList();

            if (erroredMPs.Count() > 0)
            {
                switch (SFTChannel.GetChannelCode().ToUpper())
                {
                    case "API_ELLIEMAE":
                        //NOTE : Reason to use FirstOrDefault : AM will get only single request using API
                        errorReportFileName = "{0}-{1}-{2}.json".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                        errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                        errorData = generalSvc.GeneralJsonReport(erroredMPs.FirstOrDefault().UniqueId.ToString());
                        if (!string.IsNullOrWhiteSpace(errorData))
                        {
                            File.WriteAllText(errorReportPath, errorData.ToString());
                        }
                        break;
                    default:
                        errorReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                        errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                        errorData = generalSvc.GeneralXMLReport(erroredMPs, mbi.MailBatch, mbi.Client.AppCode);
                        if (!string.IsNullOrWhiteSpace(errorData))
                        {
                            File.WriteAllText(errorReportPath, errorData.ToString());
                        }
                        break;
                }
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
                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000, SFTChannel.Port);
                sftSite.Connect();

                string sftChannelCode = SFTChannel.GetChannelCode().ToUpper();
                string ftpUploadPath = "";
                switch (sftChannelCode)
                {
                    case "PENFED":
                        //NOTE : Below zip file naming convention is EDG Naming convention.
                        zipFileName = "{0}FAGOS.ZYONT.N0010362.PenFedPNSOUTPUT.D{1}.T{2}.zip".Fill(Config.Common_EnvironmentInitial, Common.Now.ToString("yyyyMMdd"), Common.Now.ToString("hhmmss"));
                        zipFilePath = Path.Combine(mbi.AMResponsePath, zipFileName);

                        ftpUploadPath = Path.Combine(deliverable.UploadPath, zipFileName);

                        using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                        {
                            newFile.CreateEntryFromFile(errorReportPath, errorReportFileName);
                        }
                        sftSite.UploadFile(zipFilePath, ftpUploadPath);
                        Thread.Sleep(1000);//This line is added to avoid any duplication of file name.

                        File.Move(zipFilePath, Path.ChangeExtension(zipFilePath, ".ZipUploaded"));
                        break;
                    case "BECU":
                        //NOTE : Below zip file naming convention is EDG Naming convention.
                        zipFileName = "{0}FAGOS.ZYONT.N0010376.BECUPNSOUTPUT.D{1}.T{2}.zip".Fill(Config.Common_EnvironmentInitial, Common.Now.ToString("yyyyMMdd"), Common.Now.ToString("hhmmss"));
                        zipFilePath = Path.Combine(mbi.AMResponsePath, zipFileName);

                        ftpUploadPath = Path.Combine(deliverable.UploadPath, zipFileName);

                        using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                        {
                            newFile.CreateEntryFromFile(errorReportPath, errorReportFileName);
                        }
                        sftSite.UploadFile(zipFilePath, ftpUploadPath);
                        Thread.Sleep(1000);//This line is added to avoid any duplication of file name.

                        File.Move(zipFilePath, Path.ChangeExtension(zipFilePath, ".ZipUploaded"));
                        break;
                    default:
                        ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportFileName);
                        sftSite.UploadFile(errorReportPath, ftpUploadPath);
                        break;
                }

                if (sftSite.IsConnected)
                    sftSite.Disconnect();
                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now).Wait();

                if (Config.Monitor_PackagesHoldInfo.Any(x => x.ClientId == mbi.MailBatch.ClientId && x.HoldOn == mbi.MailBatch.ReceivedOn.Value.UtcToEst().Date))
                {
                    var packageHoldInfo = Config.Monitor_PackagesHoldInfo.SingleOrDefault(x => x.ClientId == mbi.MailBatch.ClientId && x.HoldOn == mbi.MailBatch.ReceivedOn.Value.UtcToEst().Date);
                    if (packageHoldInfo.DocumentOrPackageType.IsNullOrWhiteSpace() || mailPieces.All(x => x.DocumentOrPackageType == packageHoldInfo.DocumentOrPackageType))
                    {
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Held, packageHoldInfo.HoldOn).Wait();
                    }
                }
            }
            else
            {
                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now).Wait();

                if (Config.Monitor_PackagesHoldInfo.Any(x => x.ClientId == mbi.MailBatch.ClientId && x.HoldOn == mbi.MailBatch.ReceivedOn.Value.UtcToEst().Date))
                {
                    var packageHoldInfo = Config.Monitor_PackagesHoldInfo.SingleOrDefault(x => x.ClientId == mbi.MailBatch.ClientId && x.HoldOn == mbi.MailBatch.ReceivedOn.Value.UtcToEst().Date);
                    if (packageHoldInfo.DocumentOrPackageType.IsNullOrWhiteSpace() || mailPieces.All(x => x.DocumentOrPackageType == packageHoldInfo.DocumentOrPackageType))
                    {
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Held, packageHoldInfo.HoldOn).Wait();
                    }
                }
            }

            return errorReportPath;
        }
    }
}
