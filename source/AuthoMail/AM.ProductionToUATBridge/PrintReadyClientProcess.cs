using AM.Entities;
using AM.Lib;
using AM.Services;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.ProductionToUATBridge
{
    public static class PrintReadyClientProcess
    {
        public static log4net.ILog log = log4net.LogManager.GetLogger(typeof(PrintReadyClientProcess));
        private static Random random = new Random();

        public static void GenerateCitiBatch(DateTime inputDate)
        {
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IClientService clientSvc = new ClientService())
            {
                int clientId = 103;
                List<MailPieceDTO> mailPieces = mailPieceSvc.GetMailPiecesForUATBridge(inputDate, clientId);
                if (mailPieces.Count == 0)
                {
                    log.Debug("No MailPieces Found");
                    return;
                }
                var client = clientSvc.GetClientById(clientId);

                SFTChannel sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(Configuration.PrintReadyUATChannelCode);

                string clientBatchId = "UAT_{0}{1}".Fill(client.AppCode, Common.Now.ToString("yyyyMMddhhmmss"));
                log.Debug("ClientBatchId : {0}".Fill(clientBatchId));
                string destinationPath = Path.Combine(Configuration.DestinationPath, client.AppCode, clientBatchId);
                destinationPath.CreateDirectoryIfNotExist();
                log.Debug("DestinationPath {0}".Fill(destinationPath));
                string xmlFilePath = Path.Combine(destinationPath, "{0}.xml".Fill(clientBatchId));
                log.Debug("XmlFilePath  : {0}".Fill(xmlFilePath));
                string zipFilePath = Path.Combine(destinationPath, "{0}.zip".Fill(clientBatchId));
                log.Debug("ZipFilePath : {0}".Fill(zipFilePath));
                string triggerFilePath = Path.Combine(destinationPath, "{0}.GO".Fill(clientBatchId));
                log.Debug("TriggerFilePath : {0}".Fill(triggerFilePath));

                MailBatch mailBatch = new MailBatch();
                mailBatch.Id = Guid.NewGuid();
                mailBatch.ClientBatchId = clientBatchId;
                mailBatch.Priority = Priority.Medium;
                mailBatch.ClientCode = client.AppCode;
                List<MailPiece> packages = new List<MailPiece>();
                foreach (var mp in mailPieces)
                {
                    try
                    {
                        RecipientInfo recipientInfo = new RecipientInfo();
                        List<Metadata> customInfo = new List<Metadata>();
                        ReturnInfo returnInfo = new ReturnInfo();
                        MailPiece package = new MailPiece();
                        package.BusinessFriendlyIdName = mp.BusinessFriendlyIdName;
                        package.BusinessFriendlyIdValue = "TEST" + mp.BusinessFriendlyIdValue;
                        package.DocumentOrPackageType = "TEST_" + mp.DocumentOrPackageType;
                        package.Id = Guid.NewGuid();

                        DocumentOrPackage documentOrPackage = new DocumentOrPackage();
                        documentOrPackage.Name = mp.FileName;

                        Recipient recipient = new Recipient();
                        recipient.FirstName = "Test1";
                        recipient.LastName = "Test2";

                        Address recipientAddress = new Address();
                        recipientAddress.Street = mp.RcptAddressStreet;
                        recipientAddress.State = mp.RcptAddressState;
                        recipientAddress.City = mp.RcptAddressCity;
                        recipientAddress.Zip = mp.RcptAddressZIP;

                        recipientInfo.Recipient = recipient;
                        recipientInfo.Address = recipientAddress;

                        Recipient returnRecipient = new Recipient();
                        returnRecipient.FirstName = "Return Test1";
                        returnRecipient.LastName = "Return Test2";

                        Address returnRecipientAddress = new Address();
                        returnRecipientAddress.Street = mp.RetRcptAddressStreet;
                        returnRecipientAddress.State = mp.RetRcptAddressState;
                        returnRecipientAddress.City = mp.RetRcptAddressCity;
                        returnRecipientAddress.Zip = mp.RetRcptAddressZIP;

                        returnInfo.Recipient = returnRecipient;
                        returnInfo.Address = returnRecipientAddress;

                        var mpCustomInfos = mailPieceSvc.GetMailPieceCustomInfos(mp.Id);
                        mpCustomInfos.ForEach(mpc =>
                        {
                            if (mpc != null)
                                customInfo.Add(new Metadata() { Key = mpc.Key, DataType = (DataType)mpc.DataType, Value = mpc.Value });
                        });

                        package.CustomInfo = customInfo;
                        package.DocumentOrPackage = documentOrPackage;
                        package.RecipientInfo = recipientInfo;
                        package.ReturnInfo = returnInfo;

                        packages.Add(package);

                        string amFolderPath = Path.Combine(Configuration.AMRootPath, client.AppCode, mp.MailBatchId.ToString());
                        string amRequestPDFPath = Path.Combine(amFolderPath, Configuration.AMRequestFolder, mp.FileName);
                        string destinationPDFPath = Path.Combine(destinationPath, mp.FileName);
                        if (File.Exists(amRequestPDFPath))
                        {
                            log.Error("Path exist : MailBatchId-{0} || MailPieceId-{1}".Fill(mailBatch.Id, mp.Id));
                            //logic to implement remove its cover page and update new cover page accordingly and merge pdf.
                            Dictionary<string, string> mpPDFFiller = new Dictionary<string, string>();
                            mpPDFFiller.Add("FromAddress", (mp.RetRcptFirstName + " " + mp.RetRcptLastName).Trim() + "\n" + mp.RetRcptAddressStreet + "\n" + mp.RetRcptAddressCity + " " + mp.RetRcptAddressState + " " + mp.RetRcptAddressZIP);
                            mpPDFFiller.Add("ToAddress", (mp.RcptFirstName + " " + mp.RcptLastName).Trim() + "\n" + mp.RcptAddressStreet + "\n" + mp.RcptAddressCity + " " + mp.RcptAddressState + " " + mp.RcptAddressZIP);
                            string templatePath = Path.Combine(Configuration.CoverPageFolder, "CitiVM.pdf");
                            string outputCoverLetterPath = Path.Combine(Configuration.CoverPageFolder, "CitiVM_Output.pdf");
                            GenerateCoverPage(templatePath, outputCoverLetterPath, mpPDFFiller);
                            string outputPDFWithoutCoverPagePath = Path.Combine(amFolderPath, Configuration.AMRequestFolder, Path.GetFileNameWithoutExtension(mp.FileName) + "_ActualPDF.pdf");
                            GeneratePDFWithoutCoverPage(amRequestPDFPath, outputPDFWithoutCoverPagePath);

                            MergePrintReadyPDF(outputCoverLetterPath, outputPDFWithoutCoverPagePath, destinationPDFPath);

                            if (File.Exists(outputCoverLetterPath))
                                File.Delete(outputCoverLetterPath);

                            if (File.Exists(outputPDFWithoutCoverPagePath))
                                File.Delete(outputPDFWithoutCoverPagePath);
                        }
                        else
                        {
                            log.Error("Path does not exist : MailBatchId-{0} || MailPieceId-{1}".Fill(mailBatch.Id, mp.Id));
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = "Error occured while processing mail piece record for client : {0} MailPieceId : {1}".Fill(client.AppCode, mp.Id);
                        MailUtility.SendUnexpectedExceptionEmail(ex, message);
                        continue;
                    }
                }

                mailBatch.MailPieces = packages;

                if (packages.Count == 0)
                {
                    var msg = "Package does not contain any mail pieces for client {0}".Fill(client.AppCode);
                    log.Debug(msg);
                    MailUtility.SendUnexpectedExceptionEmail(null, msg);
                    return;
                }
                var xml = XmlUtil.Serialize(mailBatch);
                File.WriteAllText(xmlFilePath, xml);
                var pdfList = Directory.EnumerateFiles(destinationPath, "*.pdf");
                using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var pdfPath in pdfList)
                    {
                        newFile.CreateEntryFromFile(pdfPath, Path.GetFileName(pdfPath));
                    }
                    newFile.CreateEntryFromFile(xmlFilePath, Path.GetFileName(xmlFilePath));
                }
                File.WriteAllText(triggerFilePath, "");

                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000, sftChannel.Port);
                sftSite.Connect();
                sftSite.UploadFile(zipFilePath, Path.Combine(sftChannel.ToADEDir, Path.GetFileName(zipFilePath)));
                sftSite.UploadFile(triggerFilePath, Path.Combine(sftChannel.ToADEDir, Path.GetFileName(triggerFilePath)));
                if (sftSite.IsConnected)
                    sftSite.Disconnect();
                Directory.Delete(destinationPath, true);
                log.Debug("----Batching Process completed for Client {0}----".Fill(client.AppCode));
            }
        }
        public static void GenerateCitiBatch(List<MailPieceDTO> mailPieces)
        {
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IClientService clientSvc = new ClientService())
            {
                int clientId = 103;
                //List<MailPieceDTO> mailPieces = mailPieceSvc.GetMailPiecesForUATBridge(inputDate, clientId);
                //if (mailPieces.Count == 0)
                //{
                //    log.Debug("No MailPieces Found");
                //    return;
                //}
                var client = clientSvc.GetClientById(clientId);

                SFTChannel sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(Configuration.PrintReadyUATChannelCode);

                string clientBatchId = "UAT_{0}{1}".Fill(client.AppCode, Common.Now.ToString("yyyyMMddhhmmss"));
                log.Debug("ClientBatchId : {0}".Fill(clientBatchId));
                string destinationPath = Path.Combine(Configuration.DestinationPath, client.AppCode, clientBatchId);
                destinationPath.CreateDirectoryIfNotExist();
                log.Debug("DestinationPath {0}".Fill(destinationPath));
                string xmlFilePath = Path.Combine(destinationPath, "{0}.xml".Fill(clientBatchId));
                log.Debug("XmlFilePath  : {0}".Fill(xmlFilePath));
                string zipFilePath = Path.Combine(destinationPath, "{0}.zip".Fill(clientBatchId));
                log.Debug("ZipFilePath : {0}".Fill(zipFilePath));
                string triggerFilePath = Path.Combine(destinationPath, "{0}.GO".Fill(clientBatchId));
                log.Debug("TriggerFilePath : {0}".Fill(triggerFilePath));

                MailBatch mailBatch = new MailBatch();
                mailBatch.Id = Guid.NewGuid();
                mailBatch.ClientBatchId = clientBatchId;
                mailBatch.Priority = Priority.Medium;
                mailBatch.ClientCode = client.AppCode;
                List<MailPiece> packages = new List<MailPiece>();
                foreach (var mp in mailPieces)
                {
                    try
                    {
                        RecipientInfo recipientInfo = new RecipientInfo();
                        List<Metadata> customInfo = new List<Metadata>();
                        ReturnInfo returnInfo = new ReturnInfo();
                        MailPiece package = new MailPiece();
                        package.BusinessFriendlyIdName = mp.BusinessFriendlyIdName;
                        package.BusinessFriendlyIdValue = "TEST" + mp.BusinessFriendlyIdValue;
                        package.DocumentOrPackageType = "TEST_" + mp.DocumentOrPackageType;
                        package.Id = Guid.NewGuid();

                        DocumentOrPackage documentOrPackage = new DocumentOrPackage();
                        documentOrPackage.Name = mp.FileName;

                        Recipient recipient = new Recipient();
                        recipient.FirstName = "Test1";
                        recipient.LastName = "Test2";

                        Address recipientAddress = new Address();
                        recipientAddress.Street = mp.RcptAddressStreet;
                        recipientAddress.State = mp.RcptAddressState;
                        recipientAddress.City = mp.RcptAddressCity;
                        recipientAddress.Zip = mp.RcptAddressZIP;

                        recipientInfo.Recipient = recipient;
                        recipientInfo.Address = recipientAddress;

                        Recipient returnRecipient = new Recipient();
                        returnRecipient.FirstName = "Return Test1";
                        returnRecipient.LastName = "Return Test2";

                        Address returnRecipientAddress = new Address();
                        returnRecipientAddress.Street = mp.RetRcptAddressStreet;
                        returnRecipientAddress.State = mp.RetRcptAddressState;
                        returnRecipientAddress.City = mp.RetRcptAddressCity;
                        returnRecipientAddress.Zip = mp.RetRcptAddressZIP;

                        returnInfo.Recipient = returnRecipient;
                        returnInfo.Address = returnRecipientAddress;

                        var mpCustomInfos = mailPieceSvc.GetMailPieceCustomInfos(mp.Id);
                        mpCustomInfos.ForEach(mpc =>
                        {
                            if (mpc != null)
                                customInfo.Add(new Metadata() { Key = mpc.Key, DataType = (DataType)mpc.DataType, Value = mpc.Value });
                        });

                        package.CustomInfo = customInfo;
                        package.DocumentOrPackage = documentOrPackage;
                        package.RecipientInfo = recipientInfo;
                        package.ReturnInfo = returnInfo;

                        packages.Add(package);

                        string amFolderPath = Path.Combine(Configuration.AMRootPath, client.AppCode, mp.MailBatchId.ToString());
                        string amRequestPDFPath = Path.Combine(amFolderPath, Configuration.AMRequestFolder, mp.FileName);
                        string destinationPDFPath = Path.Combine(destinationPath, mp.FileName);
                        if (File.Exists(amRequestPDFPath))
                        {
                            log.Error("Path exist : MailBatchId-{0} || MailPieceId-{1}".Fill(mailBatch.Id, mp.Id));
                            //logic to implement remove its cover page and update new cover page accordingly and merge pdf.
                            Dictionary<string, string> mpPDFFiller = new Dictionary<string, string>();
                            mpPDFFiller.Add("FromAddress", (mp.RetRcptFirstName + " " + mp.RetRcptLastName).Trim() + "\n" + mp.RetRcptAddressStreet + "\n" + mp.RetRcptAddressCity + " " + mp.RetRcptAddressState + " " + mp.RetRcptAddressZIP);
                            mpPDFFiller.Add("ToAddress", (mp.RcptFirstName + " " + mp.RcptLastName).Trim() + "\n" + mp.RcptAddressStreet + "\n" + mp.RcptAddressCity + " " + mp.RcptAddressState + " " + mp.RcptAddressZIP);
                            string templatePath = Path.Combine(Configuration.CoverPageFolder, "CitiVM.pdf");
                            string outputCoverLetterPath = Path.Combine(Configuration.CoverPageFolder, "CitiVM_Output.pdf");
                            GenerateCoverPage(templatePath, outputCoverLetterPath, mpPDFFiller);
                            string outputPDFWithoutCoverPagePath = Path.Combine(amFolderPath, Configuration.AMRequestFolder, Path.GetFileNameWithoutExtension(mp.FileName) + "_ActualPDF.pdf");
                            GeneratePDFWithoutCoverPage(amRequestPDFPath, outputPDFWithoutCoverPagePath);

                            MergePrintReadyPDF(outputCoverLetterPath, outputPDFWithoutCoverPagePath, destinationPDFPath);

                            if (File.Exists(outputCoverLetterPath))
                                File.Delete(outputCoverLetterPath);

                            if (File.Exists(outputPDFWithoutCoverPagePath))
                                File.Delete(outputPDFWithoutCoverPagePath);
                        }
                        else
                        {
                            log.Error("Path does not exist : MailBatchId-{0} || MailPieceId-{1}".Fill(mailBatch.Id, mp.Id));
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = "Error occured while processing mail piece record for client : {0} MailPieceId : {1}".Fill(client.AppCode, mp.Id);
                        MailUtility.SendUnexpectedExceptionEmail(ex, message);
                        continue;
                    }
                }

                mailBatch.MailPieces = packages;

                if (packages.Count == 0)
                {
                    var msg = "Package does not contain any mail pieces for client {0}".Fill(client.AppCode);
                    log.Debug(msg);
                    MailUtility.SendUnexpectedExceptionEmail(null, msg);
                    return;
                }
                var xml = XmlUtil.Serialize(mailBatch);
                File.WriteAllText(xmlFilePath, xml);
                var pdfList = Directory.EnumerateFiles(destinationPath, "*.pdf");
                using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var pdfPath in pdfList)
                    {
                        newFile.CreateEntryFromFile(pdfPath, Path.GetFileName(pdfPath));
                    }
                    newFile.CreateEntryFromFile(xmlFilePath, Path.GetFileName(xmlFilePath));
                }
                File.WriteAllText(triggerFilePath, "");

                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000, sftChannel.Port);
                sftSite.Connect();
                sftSite.UploadFile(zipFilePath, Path.Combine(sftChannel.ToADEDir, Path.GetFileName(zipFilePath)));
                sftSite.UploadFile(triggerFilePath, Path.Combine(sftChannel.ToADEDir, Path.GetFileName(triggerFilePath)));
                if (sftSite.IsConnected)
                    sftSite.Disconnect();
                Directory.Delete(destinationPath, true);
                log.Debug("----Batching Process completed for Client {0}----".Fill(client.AppCode));
            }
        }

        private static void MergePrintReadyPDF(string outputCoverLetterPath, string outputPDFWithoutCoverPagePath, string destinationPDFPath)
        {
            List<string> mergePdfCollection = new List<string>() { outputCoverLetterPath, outputPDFWithoutCoverPagePath, destinationPDFPath };
            PdfUtil.MergePDFs(mergePdfCollection.ToArray());
        }

        private static void GeneratePDFWithoutCoverPage(string amRequestPDFPath, string outputPDFWithoutCoverPagePath)
        {
            using (PdfReader reader = new PdfReader(amRequestPDFPath))
            {
                int pageCount = reader.NumberOfPages;
                reader.SelectPages("{0}-{1}".Fill(2, pageCount));
                using (PdfStamper pdfStamper = new PdfStamper(reader, new FileStream(outputPDFWithoutCoverPagePath, FileMode.Create)))
                {
                    pdfStamper.Close();
                }
                reader.Close();
            }
        }

        private static void GenerateCoverPage(string templatePath, string outputPath, Dictionary<string, string> mpPDFFiller)
        {
            using (var existingFileStream = new FileStream(templatePath, FileMode.Open))
            using (var newFileStream = new FileStream(outputPath, FileMode.Create))
            {
                // Open existing PDF
                var pdfReader = new PdfReader(existingFileStream);

                // PdfStamper, which will create
                var stamper = new PdfStamper(pdfReader, newFileStream);

                var form = stamper.AcroFields;
                foreach (var model in mpPDFFiller)
                {
                    form.SetField(model.Key, model.Value);
                }

                // "Flatten" the form so it wont be editable/usable anymore
                stamper.FormFlattening = true;

                // You can also specify fields to be flattened, which
                // leaves the rest of the form still be editable/usable
                // stamper.PartialFormFlattening("Address");

                stamper.Close();
                pdfReader.Close();
            }
        }

        public static void GenerateRPSBatch(List<MailPieceDTO> mailPieces)
        {
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IClientService clientSvc = new ClientService())
            {
                int clientId = 110;
                //List<MailPieceDTO> mailPieces = mailPieceSvc.GetMailPiecesForUATBridge(inputDate, clientId);
                //if (mailPieces.Count == 0)
                //{
                //    log.Debug("No MailPieces Found");
                //    return;
                //}
                var client = clientSvc.GetClientById(clientId);

                SFTChannel sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(Configuration.PrintReadyUATChannelCode);

                string clientBatchId = "UAT_{0}{1}".Fill(client.AppCode, Common.Now.ToString("yyyyMMddhhmmss"));
                log.Debug("ClientBatchId : {0}".Fill(clientBatchId));
                string destinationPath = Path.Combine(Configuration.DestinationPath, client.AppCode, clientBatchId);
                destinationPath.CreateDirectoryIfNotExist();
                log.Debug("DestinationPath {0}".Fill(destinationPath));
                string xmlFilePath = Path.Combine(destinationPath, "{0}.xml".Fill(clientBatchId));
                log.Debug("XmlFilePath  : {0}".Fill(xmlFilePath));
                string zipFilePath = Path.Combine(destinationPath, "{0}.zip".Fill(clientBatchId));
                log.Debug("ZipFilePath : {0}".Fill(zipFilePath));
                string triggerFilePath = Path.Combine(destinationPath, "{0}.GO".Fill(clientBatchId));
                log.Debug("TriggerFilePath : {0}".Fill(triggerFilePath));

                MailBatch mailBatch = new MailBatch();
                mailBatch.Id = Guid.NewGuid();
                mailBatch.ClientBatchId = clientBatchId;
                mailBatch.Priority = Priority.Medium;
                mailBatch.ClientCode = client.AppCode;
                List<MailPiece> packages = new List<MailPiece>();
                foreach (var mp in mailPieces)
                {
                    try
                    {
                        RecipientInfo recipientInfo = new RecipientInfo();
                        List<Metadata> customInfo = new List<Metadata>();
                        ReturnInfo returnInfo = new ReturnInfo();
                        MailPiece package = new MailPiece();
                        package.BusinessFriendlyIdName = mp.BusinessFriendlyIdName;
                        package.BusinessFriendlyIdValue = "TEST" + random.Next(100000, 10000000);
                        package.DocumentOrPackageType = "TEST_" + mp.DocumentOrPackageType;
                        package.Id = Guid.NewGuid();

                        DocumentOrPackage documentOrPackage = new DocumentOrPackage();
                        documentOrPackage.Name = mp.FileName;

                        Recipient recipient = new Recipient();
                        recipient.FirstName = "Test1";
                        recipient.LastName = "Test2";

                        Address recipientAddress = new Address();
                        recipientAddress.Street = "20 Petaluma Boulevard South";
                        recipientAddress.City = "Petaluma";
                        recipientAddress.State = "CA";
                        recipientAddress.Zip = "94952";

                        recipientInfo.Recipient = recipient;
                        recipientInfo.Address = recipientAddress;

                        Recipient returnRecipient = new Recipient();
                        returnRecipient.FirstName = "Return Test1";
                        returnRecipient.LastName = "Return Test2";

                        Address returnRecipientAddress = new Address();
                        returnRecipientAddress.Street = mp.RetRcptAddressStreet;
                        returnRecipientAddress.State = mp.RetRcptAddressState;
                        returnRecipientAddress.City = mp.RetRcptAddressCity;
                        returnRecipientAddress.Zip = mp.RetRcptAddressZIP;

                        returnInfo.Recipient = returnRecipient;
                        returnInfo.Address = returnRecipientAddress;

                        var mpCustomInfos = mailPieceSvc.GetMailPieceCustomInfos(mp.Id);
                        mpCustomInfos.ForEach(mpc =>
                        {
                            if (mpc != null)
                                customInfo.Add(new Metadata() { Key = mpc.Key, DataType = (DataType)mpc.DataType, Value = mpc.Value });
                        });

                        package.CustomInfo = customInfo;
                        package.DocumentOrPackage = documentOrPackage;
                        package.RecipientInfo = recipientInfo;
                        package.ReturnInfo = returnInfo;

                        packages.Add(package);

                        string amFolderPath = Path.Combine(Configuration.AMRootPath, client.AppCode, mp.MailBatchId.ToString());
                        string amRequestPDFPath = Path.Combine(amFolderPath, Configuration.AMRequestFolder, mp.FileName);
                        string destinationPDFPath = Path.Combine(destinationPath, mp.FileName);
                        if (File.Exists(amRequestPDFPath))
                        {
                            //logic to implement remove its cover page and update new cover page accordingly and merge pdf.
                            Dictionary<string, string> mpPDFFiller = new Dictionary<string, string>();
                            mpPDFFiller.Add("FromAddress", (mp.RetRcptFirstName + " " + mp.RetRcptLastName).Trim() + "\n" + mp.RetRcptAddressStreet + "\n" + mp.RetRcptAddressCity + " " + mp.RetRcptAddressState + " " + mp.RetRcptAddressZIP);
                            mpPDFFiller.Add("ToAddress", (recipient.FirstName + " " + recipient.LastName).Trim() + "\n" + recipientAddress.Street + "\n" + recipientAddress.City + " " + recipientAddress.State + " " + recipientAddress.Zip);
                            string templatePath = Path.Combine(Configuration.CoverPageFolder, "RPS.pdf");
                            string outputCoverLetterPath = Path.Combine(Configuration.CoverPageFolder, "RPS_Output.pdf");
                            GenerateCoverPage(templatePath, outputCoverLetterPath, mpPDFFiller);
                            string outputPDFWithoutCoverPagePath = Path.Combine(amFolderPath, Configuration.AMRequestFolder, Path.GetFileNameWithoutExtension(mp.FileName) + "_ActualPDF.pdf");
                            GeneratePDFWithoutCoverPage(amRequestPDFPath, outputPDFWithoutCoverPagePath);

                            MergePrintReadyPDF(outputCoverLetterPath, outputPDFWithoutCoverPagePath, destinationPDFPath);

                            if (File.Exists(outputCoverLetterPath))
                                File.Delete(outputCoverLetterPath);

                            if (File.Exists(outputPDFWithoutCoverPagePath))
                                File.Delete(outputPDFWithoutCoverPagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = "Error occured while processing mail piece record for client : {0} MailPieceId : {1}".Fill(client.AppCode, mp.Id);
                        MailUtility.SendUnexpectedExceptionEmail(ex, message);
                        continue;
                    }
                }

                mailBatch.MailPieces = packages;

                if (packages.Count == 0)
                {
                    var msg = "Package does not contain any mail pieces for client {0}".Fill(client.AppCode);
                    log.Debug(msg);
                    MailUtility.SendUnexpectedExceptionEmail(null, msg);
                    return;
                }
                var xml = XmlUtil.Serialize(mailBatch);
                File.WriteAllText(xmlFilePath, xml);
                var pdfList = Directory.EnumerateFiles(destinationPath, "*.pdf");
                using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var pdfPath in pdfList)
                    {
                        newFile.CreateEntryFromFile(pdfPath, Path.GetFileName(pdfPath));
                    }
                    newFile.CreateEntryFromFile(xmlFilePath, Path.GetFileName(xmlFilePath));
                }
                File.WriteAllText(triggerFilePath, "");

                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000, sftChannel.Port);
                sftSite.Connect();
                sftSite.UploadFile(zipFilePath, Path.Combine(sftChannel.ToADEDir, Path.GetFileName(zipFilePath)));
                sftSite.UploadFile(triggerFilePath, Path.Combine(sftChannel.ToADEDir, Path.GetFileName(triggerFilePath)));
                if (sftSite.IsConnected)
                    sftSite.Disconnect();
                Directory.Delete(destinationPath, true);
                log.Debug("----Batching Process completed for Client {0}----".Fill(client.AppCode));
            }
        }
    }
}
