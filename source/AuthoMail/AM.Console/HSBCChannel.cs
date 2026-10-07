using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace AM.Console
{
    class HSBCChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static XmlSchemaSet schemas;

        private string channelType;

        protected IHSBCService hsbcSvc;
        protected Client client;

        static HSBCChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add(string.Empty, XmlReader.Create(new StringReader(Resources.HSBCRespaControlSchema)));
        }

        public HSBCChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            hsbcSvc = GetService<HSBCService>();
            string appCode = channelCode.Split('_')[0];
            client = clientSvc.GetClientByAppCode(appCode).Result;
            this.channelType = channelType;
        }

        protected override void EnumerateBatches()
        {
            log.Debug("Enumerate Batches job queue for HSBC is started.");

            if (SFTChannel.ServerIP.IsNullOrWhiteSpace() || Config.Common_ServerIP != SFTChannel.ServerIP)
            {
                log.Debug("System cannot process HSBC channel on this server.");
                return;
            }

            List<string> xmlFileNames = new List<string>();

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.NoBatch);

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            xmlFileNames = GetXmlFileNames(sftSite);
            if (xmlFileNames.Count == 0)
            {
                MailUtility.SendNoBatchReceivedNotificationEmail(client, Common.Now, deliverable.EmailChannel);
                return;
            }

            foreach (string xmlFile in xmlFileNames)
            {
                string xmlFilePath = Path.Combine(SFTChannel.ToADEDir, xmlFile + ".xml");

                //Download and Insert the batch in database first always.
                string batchName = xmlFile;

                MailBatchDTO mailBatch = new MailBatchDTO();
                mailBatch.ClientId = client.Id;
                mailBatch.BatchName = batchName;
                mailBatch.SFTChannelId = SFTChannel.Id;
                mailBatch.ClientBatchId = batchName;
                mailBatch.UniqueId = Guid.NewGuid();
                mailBatch.Priority = Priority.Medium.ToString();
                mailBatch.ServerIP = Config.Common_ServerIP;
                //Insert New Batch Entry
                mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);

                DateTime lastModifiedTime = Common.Now;
                if (channelType == ChannelType.Trigger)
                {
                    lastModifiedTime = sftSite.LastModifiedTimeOfFile(xmlFilePath);
                }

                //Update Pipeline Status of Batch Received
                commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, lastModifiedTime);

                commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.InProgress.ToString()).Wait();
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();
        }

        protected override void DownloadBatches()
        {
            log.Debug("HSBC download job is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.Received, client.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug("HSBC download job queue is empty.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            foreach (var batch in downloadBatches)
            {
                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                try
                {
                    #region Create directory structure.

                    mbi.BatchRepo.CleanDirectory();
                    mbi.AMRequestPath.CreateDirectoryIfNotExist();
                    mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                    mbi.PnSResponsePath.CreateDirectoryIfNotExist();
                    mbi.AMResponsePath.CreateDirectoryIfNotExist();

                    #endregion

                    string xmlFileName = "{0}.xml".Fill(mbi.BatchName);

                    string localXmlFilePath = Path.Combine(mbi.AMRequestPath, xmlFileName);
                    string ftpXmlFilePath = Path.Combine(SFTChannel.ToADEDir, xmlFileName);

                    sftSite.DownloadFile(ftpXmlFilePath, localXmlFilePath);

                    //Based on post download action xml file will get moved or delete from that location
                    if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                    {
                        string archiveFTPXmlFilePath = Path.Combine(SFTChannel.ArchiveDir, xmlFileName);

                        sftSite.MoveFile(ftpXmlFilePath, archiveFTPXmlFilePath);
                    }
                    else
                    {
                        sftSite.DeleteFile(ftpXmlFilePath);
                    }

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now);
                }
                catch (Exception ex)
                {
                    var error = "Error occured while downloading {0} batch from SFT site for Batch-Id : {1}".Fill(mbi.BatchName, mbi.MailBatch.Id) + Environment.NewLine + ex.ToString();

                    log.Error(error);
                    MailUtility.SendUnexpectedExceptionEmail(error);
                    continue;
                }
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            log.Debug("HSBC download job queue is completed.");
        }

        protected override void ValidateBatches()
        {
            log.Debug("HSBC validate job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.Downloaded, client.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug("HSBC validate job queue is empty.");
                return;
            }

            foreach (var mailBatch in downloadBatches)
            {
                //NOTE: Here system will delete if any existing records are there with MailBatchId in mail pieces table
                mailBatchSvc.DeleteMailPiecesDetailsByMailBatchId(mailBatch.Id);

                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                Deliverable deliverables = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.GetBusinessIdNameFromClientId(client.Id);

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

                XmlDocument mailBatchXML = new XmlDocument();
                XmlNodeList mailPieceNodes = null;

                try
                {
                    mailBatchXML.LoadXml(File.ReadAllText(xmlFilePath));
                    //If we get comma(,) separated packagename then we will create separate respa node for each packagename.
                    mailPieceNodes = SeparateRespaNodes(mailBatchXML);
                }
                catch (XmlException ex)
                {
                    var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                    log.Error(msg, ex);

                    ReportInvalidBatch(mbi, msg, deliverables);

                    continue;
                }
                catch (Exception ex)
                {
                    var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                    log.Error(msg, ex);
                    ReportInvalidBatch(mbi, ex.Message, deliverables);
                    continue;
                }

                if (mailPieceNodes == null || mailPieceNodes.Count == 0 || !mailBatchXML.DocumentElement.ChildNodes[0].HasChildNodes)
                {
                    ReportInvalidBatch(mbi, "'{0}.xml' does not contain any valid node(s).".Fill(mbi.BatchName), deliverables);

                    continue;
                }

                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                sftSite.Connect();

                List<string> fileListToMoveOrDelete = new List<string>();

                foreach (XmlNode mailPieceNode in mailPieceNodes)
                {
                    MailPieceDTO mailPieceDTO = new MailPieceDTO();
                    MailPieceCustomInfo mailPieceCustomInfo = null;
                    List<MailPieceCustomInfo> mailPieceCustomInfoList = new List<MailPieceCustomInfo>();

                    try
                    {
                        mailPieceDTO.ClientId = client.Id;
                        mailPieceDTO.UniqueId = Guid.NewGuid();
                        mailPieceDTO.MailBatchId = mbi.MailBatch.Id;
                        mailPieceDTO.ShippingStatus = ShippingStatus.InProgress.ToString();

                        HSBCRespaControl node = XmlUtil.Deserialize<HSBCRespaControl>(mailPieceNode.OuterXml);

                        mailPieceDTO.BusinessFriendlyIdName = clientBusinessIdDetails.BusinessIdName;
                        mailPieceDTO.BusinessFriendlyIdValue = node.LoanNbr;
                        mailPieceDTO.RcptFirstName = node.BorrowerFirstName;
                        mailPieceDTO.RcptLastName = node.BorrowerLastName;
                        mailPieceDTO.RcptMiddleName = node.BorrowerMiddleName;
                        mailPieceDTO.RcptSuffix = node.BorrowerSuffix;
                        mailPieceDTO.FilePattern = node.PackageName;
                        Regex regEx = new Regex(@"\[(.+?)\]");
                        var param = regEx.Match(node.PackageName);
                        mailPieceDTO.DocumentOrPackageType = param.Groups[1].Value.Trim();

                        mailPieceDTO.RcptAddressStreet = node.Address;
                        mailPieceDTO.RcptAddrOptionalLine1BelowStreet = node.Address2;
                        mailPieceDTO.RcptAddressCity = node.City;
                        mailPieceDTO.RcptAddressState = node.State;
                        mailPieceDTO.RcptAddressZIP = node.Zip;
                        mailPieceDTO.IsRcptInfoSpecified = true;

                        mailPieceDTO.IsReturnInfoSpecified = false;


                        if (node.StatusID != null)
                        {
                            mailPieceCustomInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "StatusID", DataType = DataType.String, Value = node.StatusID };
                            mailPieceCustomInfoList.Add(mailPieceCustomInfo);
                        }


                        MailPieceValidationResult validationResult = ValidateRespaControlNode(mailPieceNode);
                        if (validationResult.Code == OutComeCodes.No_Error)
                        {

                            var regex = new Regex(@"^(.*).pdf$", RegexOptions.IgnoreCase);

                            List<string> availablePdfs = sftSite.ListDirectory(SFTChannel.ToADEDir)
                                .Select(f => regex.Match(Path.GetFileName(f)))
                                .Where(m => m.Success)
                                .Select(m => m.Groups[1].Value)
                                .ToList();

                            if (!availablePdfs.Any(fName => fName.ToLower().Contains(mailPieceDTO.FilePattern.ToLower())))
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.Missing_PDF_File;
                                mailPieceDTO.ErrorDesc = "Pdf attachment is not available.";
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                mailPieceDTO.PrintLocation = null;
                            }
                            else
                            {
                                var selectedPdfs = availablePdfs.Where(p => p.ToLower().Contains(mailPieceDTO.FilePattern.ToLower()));
                                List<PdfLastWriteTime> pdfDetails = new List<PdfLastWriteTime>();
                                foreach (var pdf in selectedPdfs)
                                {
                                    PdfLastWriteTime plw = new PdfLastWriteTime();
                                    plw.PdfFilePath = Path.Combine(SFTChannel.ToADEDir, pdf + ".pdf");
                                    plw.LastWriteTime = sftSite.LastModifiedTimeOfFile(plw.PdfFilePath);
                                    pdfDetails.Add(plw);
                                }
                                string sftFilePath = pdfDetails.OrderByDescending(o => o.LastWriteTime).First().PdfFilePath;
                                string sftPdfFileName = Path.GetFileName(sftFilePath);
                                string localFilePath = Path.Combine(mbi.AMRequestPath, sftPdfFileName);

                                int pageCount = 0;
                                using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(localFilePath))
                                {
                                    pageCount = reader.NumberOfPages;
                                }

                                //Document Or Package Type validation
                                var docType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType, pageCount, mailBatch.ReceivedOn.Value);

                                if (docType != null)
                                    mailPieceDTO.DocTypeId = docType.Id;

                                if (docType == null)
                                {
                                    mailPieceDTO.ErrorCode = OutComeCodes.Invalid_PackageType;
                                    mailPieceDTO.ErrorDesc = "The specified document or package type '{0}' is not configured.".Fill(mailPieceDTO.DocumentOrPackageType);
                                    mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPieceDTO.PrintLocation = null;
                                }
                                else
                                {
                                    //If we don't have any return address at mail piece or batch level  then it should be in Doc Type level and we are storing that value in mail piece from it.
                                    if (docType.IsReturnInfoSpecified)
                                    {
                                        mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = docType.RetRcptOrganization;
                                        mailPieceDTO.RetRcptAddressStreet = docType.RetRcptAddressStreet;
                                        mailPieceDTO.RetRcptAddressCity = docType.RetRcptAddressCity;
                                        mailPieceDTO.RetRcptAddressState = docType.RetRcptAddressState;
                                        mailPieceDTO.RetRcptAddressZIP = docType.RetRcptAddressZIP;
                                    }

                                    mailPieceDTO.FileName = sftPdfFileName;
                                    if (!File.Exists(localFilePath))
                                    {
                                        //Downloading matching file name from client SFT
                                        sftSite.DownloadFile(sftFilePath, localFilePath);
                                    }

                                    //Creating collection for moving or deleting files because it may have same file for multiple nodes so can't move or delete at these level.
                                    fileListToMoveOrDelete.Add(sftFilePath);

                                    mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                }
                            }
                        }
                        else
                        {
                            mailPieceDTO.ErrorCode = validationResult.Code;
                            mailPieceDTO.ErrorDesc = validationResult.ErrorMessage;
                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            mailPieceDTO.PrintLocation = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Unable to process node for batch {0}.".Fill(mbi.BatchName);

                        log.Error(msg, ex);

                        mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                        mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                        mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                        mailPieceDTO.PrintLocation = null;
                    }


                    try
                    {
                        mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfoList);
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
                            mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfoList);
                            MailUtility.SendUnexpectedExceptionEmail(msg);
                        }
                    }
                }

                //Based on post download action pdf file will get move or delete from that location
                if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                {
                    foreach (var sftFilePath in fileListToMoveOrDelete.Distinct())
                    {
                        string archiveFTPPDFFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(sftFilePath));

                        sftSite.MoveFile(sftFilePath, archiveFTPPDFFilePath);
                    }
                }
                else
                {
                    foreach (var sftFilePath in fileListToMoveOrDelete.Distinct())
                    {
                        sftSite.DeleteFile(sftFilePath);
                    }
                }

                if (sftSite.IsConnected)
                    sftSite.Disconnect();

                //After processing all nodes of a batch for validation status
                var mailPieceData = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                if (mailPieceData != null)
                {
                    var validMailPieces = mailPieceData.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                    //Here System will validate Addresses for valid mail pieces
                    if (validMailPieces.Count > 0)
                        Channel.ValidateAddressType(mailBatch, validMailPieces);

                    if (validMailPieces.Count == 0)
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                        mbi.MailBatch.ValidationErrors = "Xml does not contain any valid respacontrol nodes.";
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
                Channel.CalculateDueDateAndPrintLocation(client, mailBatch, mailPieceData);
                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now);
            }
        }

        //protected override void CalculateDueDateAndPrintLocation()
        //{
        //    Channel.CalculateDueDateAndPrintLocation(SFTChannel.Id);
        //}

        private XmlNodeList SeparateRespaNodes(XmlDocument batchXML)
        {
            XmlNodeList respaNodes = null;

            //If XML is schema wise valid then get all XML child nodes from it.
            respaNodes = batchXML.DocumentElement.ChildNodes;

            StringBuilder newXML = new StringBuilder("<BcgRespa>");
            foreach (XmlNode respaNode in respaNodes)
            {
                XmlNode packageNode = respaNode.SelectSingleNode("PackageName");
                if (packageNode != null && !string.IsNullOrWhiteSpace(packageNode.InnerText))
                {
                    var packageNames = respaNode.SelectSingleNode("PackageName").InnerText.Split(',');
                    foreach (var packageName in packageNames)
                    {
                        var temp = respaNode.Clone();
                        temp.SelectSingleNode("PackageName").InnerText = packageName.Trim();
                        newXML.Append(temp.OuterXml);
                    }
                }
                else
                {
                    var temp = respaNode.Clone();
                    newXML.Append(temp.OuterXml);
                }
            }
            newXML.Append("</BcgRespa>");
            batchXML.LoadXml(newXML.ToString());
            respaNodes = batchXML.DocumentElement.ChildNodes;
            return respaNodes;
        }

        private void ReportInvalidBatch(MailBatchInfo mbi, string msg, Deliverable deliverable)
        {
            string mailMsg = "No records available in the batch file {0}".Fill(mbi.MailBatch.BatchName);
            string errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.MailBatch.BatchName.Split('[')[0], Common.Now.ToString("htt"));

            StringBuilder errorReport = new StringBuilder(string.Format("No matching PDFs available in the batch file {0}.", mbi.MailBatch.BatchName));

            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
            File.WriteAllText(errorReportPath, errorReport.ToString());

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            var ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportFileName);

            sftSite.UploadFile(errorReportPath, ftpUploadPath);

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            MailUtility.SendNoDataNotificationEmail(mbi, deliverable.EmailChannel, mailMsg, errorReportPath);

            //For Invalid Batch, we will generate error File
            mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, msg).Wait();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);

            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString()).Wait();
        }

        protected override void AcknowledgeBatches()
        {
            log.Debug("HSBC acknowledge job queue is started.");

            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.Validated, client.Id);

            if (validatedBatches.Count == 0)
            {
                log.Debug("HSBC acknowledge job queue is empty.");
                return;
            }

            foreach (var batch in validatedBatches)
            {
                Deliverable reportDeliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                List<MailPieces> mailPieces;
                using (IReportService reportSvc = new ReportService())
                {
                    mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                }
                if (mailPieces != null)
                {
                    try
                    {
                        string errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(client.TimeZone).ToString("htt"));

                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        //Here we are generating error report even though it does not have any errored records
                        StringBuilder errorData = hsbcSvc.HSBCErrorReport(mailPieces, mbi.Client.TimeZone, reportDeliverable.Delimeter);
                        if (errorData != null)
                        {
                            File.WriteAllText(errorReportPath, errorData.ToString());
                        }

                        //This logic added because if we get any error while sending notification to client it will update status and process batch without any error notification. So if we get true in return then only we can process further
                        if (reportDeliverable.EmailChannel != null)
                        {
                            MailUtility.SendDownloadNotificationEmail(mbi, mailPieces, errorReportPath, reportDeliverable.EmailChannel);
                        }

                        if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.Invalid)
                        {
                            //Uploading error report to client's SFT site even blank report also
                            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                            sftSite.Connect();

                            var ftpUploadPath = Path.Combine(reportDeliverable.UploadPath, errorReportFileName);

                            sftSite.UploadFile(errorReportPath, ftpUploadPath);

                            if (sftSite.IsConnected)
                                sftSite.Disconnect();

                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);
                        }
                        else if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.PartiallyValid)
                        {
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
                        }
                        else
                        {
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured in AcknowledgeBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(batch.Id, ex.Message);
                        log.Error(msg, ex);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                }
            }

            log.Debug("HSBC acknowledge job queue is completed.");
        }

        private List<string> GetXmlFileNames(SFTUtil sftSite)
        {
            var regex = new Regex(@"^(.*).xml$", RegexOptions.IgnoreCase);

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableTriggerFilePaths != null && availableTriggerFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} xml file(s) found.", availableTriggerFilePaths.Count));
            }
            else
            {
                log.Error("No trigger files found hence no batches are available to download.");
            }

            return availableTriggerFilePaths;
        }

        private MailPieceValidationResult ValidateRespaControlNode(XmlNode mailPieceNode)
        {
            var errors = GetAllRespaControlErrors(mailPieceNode).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private IEnumerable<MailPieceValidationResult> GetAllRespaControlErrors(XmlNode mailPieceNode)
        {
            XDocument xDoc = XDocument.Parse(mailPieceNode.OuterXml);
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

            if (errorMsg.Contains("LoanNbr"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = "Loan number is missing from respa control." };

            if (errorMsg.Contains("PackageName"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = "PackageName is missing or invalid in respa control." };

            if (!string.IsNullOrEmpty(errorMsg))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = errorMsg };

            yield break;
        }
    }

    public class PdfLastWriteTime
    {
        public string PdfFilePath { get; set; }
        public DateTime LastWriteTime { get; set; }
    }
}
