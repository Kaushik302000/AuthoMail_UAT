using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using AM.Lib;
using AM.Services;

namespace AM.ProductionToUATBridge
{
    class Program
    {
        private static Random random = new Random();
        public static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            RegenerateLabels regenerateLabels = new RegenerateLabels();
            regenerateLabels.RegerateIMBLabels();
            return;
            //string clientCode = "";
            //if (args.Length >= 1)
            //{
            //    clientCode = args[0];
            //}
            //if (clientCode == "PDFGEN")
            //{
            //    log.Info("Document generation is started.");
            //    int minPageCount = Convert.ToInt32(args[1]);
            //    string originFilePath = @"C:\Users\sagar.g\Desktop\Nordis Scenariso\GOS\10397_Caliber_TEST871130.pdf";
            //    string destFilePath = @"C:\Users\sagar.g\Desktop\Nordis Scenariso\GOS\Converted\10397_Caliber_TEST871130_{0}.pdf".Fill(minPageCount);
            //    GeneratePDFWithNumberOfPages(minPageCount, originFilePath, destFilePath);
            //    log.Info("Document generation is completed.");
            //    return;
            //}
            //else if (clientCode == "DELETE_COMPLETED_AM")
            //{
            //    using (IMailBatchService mailBatchSvc = new MailBatchService())
            //    using (IMailPieceService mailPieceSvc = new MailPieceService())
            //    using (IClientService clientSvc = new ClientService())
            //    {
            //        //List<MBInfo> mailBatches = mailPieceSvc.GetReportedBackRecordsFromUATBridge();
            //        List<MBInfo> mailBatches = mailPieceSvc.GetReportedBackAMRecordsFromUATBridge();

            //        if (mailBatches.Count == 0)
            //        {
            //            log.Info("No records found for deleting old records.");
            //            return;
            //        }

            //        foreach (var mb in mailBatches)
            //        {
            //            string rootPath = Config.Common_AuthoMailRootFolder;

            //            string batchPath = Path.Combine(rootPath, mb.AppCode, mb.MailBatchId.ToString());

            //            if (Directory.Exists(batchPath))
            //            {
            //                Directory.Delete(batchPath, true);
            //                Console.WriteLine("Removed Batch :" + mb.MailBatchId.ToString() + " FROM AppCode :" + mb.AppCode);
            //                log.Info("Removed Batch :" + mb.MailBatchId.ToString() + " FROM AppCode :" + mb.AppCode);
            //            }
            //            else
            //            {
            //                Console.WriteLine("Path Not Found For Batch :" + mb.MailBatchId.ToString() + " FROM AppCode :" + mb.AppCode);
            //                log.Info("Path Not Found For Batch :" + mb.MailBatchId.ToString() + " FROM AppCode :" + mb.AppCode);
            //            }
            //        }
            //    }

            //}
            //else if (clientCode == "DELETE_COMPLETED_PnS")
            //{
            //    using (IMailBatchService mailBatchSvc = new MailBatchService())
            //    using (IMailPieceService mailPieceSvc = new MailPieceService())
            //    using (IClientService clientSvc = new ClientService())
            //    {
            //        //List<MBInfo> mailBatches = mailPieceSvc.GetReportedBackRecordsFromUATBridge();
            //        List<MBInfo> mailBatches = mailPieceSvc.GetReportedBackPnSRecordsFromUATBridge();

            //        if (mailBatches.Count == 0)
            //        {
            //            log.Info("No records found for deleting old records.");
            //            return;
            //        }

            //        foreach (var mb in mailBatches)
            //        {
            //            string rootPath = @"D:\MTC\AuthoMail\PnSService\PCIRoot";

            //            string batchPath = Path.Combine(rootPath, mb.AppCode, mb.MailBatchId.ToString());

            //            if (Directory.Exists(batchPath))
            //            {
            //                Directory.Delete(batchPath, true);
            //                Console.WriteLine("Removed Batch :" + mb.MailBatchId.ToString() + " FROM AppCode :" + mb.AppCode);
            //                log.Info("Removed Batch :" + mb.MailBatchId.ToString() + " FROM AppCode :" + mb.AppCode);
            //            }
            //            else
            //            {
            //                Console.WriteLine("Path Not Found For Batch :" + mb.MailBatchId.ToString() + " FROM AppCode :" + mb.AppCode);
            //                log.Info("Path Not Found For Batch :" + mb.MailBatchId.ToString() + " FROM AppCode :" + mb.AppCode);
            //            }
            //        }
            //    }

            //}
            //else if (clientCode == "CITIVM")
            //{
            //    log.Debug("----Batching Process started for Client CITIVM----");
            //    PrintReadyClientProcess.GenerateCitiBatch(Common.EstToday.Date.AddDays(Convert.ToInt32(args[1])));
            //    log.Debug("----Batching Process completed for Client CITIVM----");
            //    return;
            //}
            //else if (clientCode == "RPS")
            //{
            //    log.Debug("----Batching Process started for Client RPS----");
            //    //PrintReadyClientProcess.GenerateRPSBatch(Common.EstToday.Date);
            //    log.Debug("----Batching Process completed for Client RPS----");
            //    return;
            //}
            //else
            //{
            //    log.Debug("Working Log");
            //    using (IMailBatchService mailBatchSvc = new MailBatchService())
            //    using (IMailPieceService mailPieceSvc = new MailPieceService())
            //    using (IClientService clientSvc = new ClientService())
            //    {
            //        List<MailPieceDTO> mailPieces = mailPieceSvc.GetMailPiecesForUATBridge();

            //        if (mailPieces.Count == 0)
            //        {
            //            log.Debug("No MailPieces Found");
            //            return;
            //        }
            //        var mpByClients = mailPieces.GroupBy(mp => mp.ClientId).ToList();
            //        Dictionary<string, float> ratioList = new Dictionary<string, float>();
            //        Configuration.ProductionVolumnRatio.ToList().ForEach(rt =>
            //        {
            //            var clRatio = rt.Split('|');
            //            ratioList.Add(clRatio[0], float.Parse(clRatio[1]));
            //        });

            //        foreach (var mpData in mpByClients)
            //        {
            //            try
            //            {
            //                var client = clientSvc.GetClientById(mpData.Key);
            //                if (!string.IsNullOrWhiteSpace(clientCode) && client.AppCode != clientCode)
            //                    continue;

            //                var mpList = mpData.ToList();
            //                var mpByBatchId = mpList.GroupBy(mp => mp.MailBatchId).ToList();

            //                if (client.AppCode == "RPS")
            //                {
            //                    PrintReadyClientProcess.GenerateRPSBatch(mpList);
            //                    continue;
            //                }

            //                if (client.AppCode == "CITIVM")
            //                {
            //                    PrintReadyClientProcess.GenerateCitiBatch(mpList);
            //                    continue;
            //                }

            //                SFTChannel sftChannel;
            //                if (Configuration.PrintReadyClientCodes.Contains(client.AppCode))
            //                {
            //                    sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(Configuration.PrintReadyUATChannelCode);
            //                }
            //                else
            //                {
            //                    sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(Configuration.UATChannelCode);
            //                }

            //                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000);
            //                sftSite.Connect();


            //                foreach (var mbList in mpByBatchId)
            //                {
            //                    int mbId = mbList.Key;
            //                    var mpListByMb = mbList.ToList();

            //                    log.Debug("----Batching Process started for Client {0}----".Fill(client.AppCode));
            //                    #region Production Ratio
            //                    //double qcCount = 0;
            //                    //float qcPercentage = ratioList[client.AppCode] / 100;
            //                    //qcCount = Math.Ceiling(mpListByMb.Count * qcPercentage);
            //                    //log.Debug("Record Count : {0}".Fill(qcCount));
            //                    //var randomIndexes = new List<int>();
            //                    //while (randomIndexes.Count < qcCount)
            //                    //{
            //                    //    var index = random.Next(0, mpListByMb.Count - 1);
            //                    //    if (!randomIndexes.Contains(index))
            //                    //    {
            //                    //        randomIndexes.Add(index);
            //                    //    }
            //                    //}
            //                    //log.Debug("While Going to randomIndexes");
            //                    //randomIndexes.ForEach(index =>
            //                    //{
            //                    //    mpListByMb[index].IsQCRequested = true;
            //                    //});
            //                    //log.Debug("Going to randomIndexes");
            //                    #endregion

            //                    var selectedMPs = mpListByMb.ToList();
            //                    log.Debug("Selected MailPieces Count : {0}".Fill(selectedMPs.Count));
            //                    string clientBatchId = "UAT_{0}{1}".Fill(client.AppCode, Common.Now.ToString("yyyyMMddhhmmss"));
            //                    log.Debug("ClientBatchId : {0}".Fill(clientBatchId));
            //                    string destinationPath = Path.Combine(Configuration.DestinationPath, client.AppCode, clientBatchId);
            //                    destinationPath.CreateDirectoryIfNotExist();
            //                    log.Debug("DestinationPath {0}".Fill(destinationPath));
            //                    string xmlFilePath = Path.Combine(destinationPath, "{0}.xml".Fill(clientBatchId));
            //                    log.Debug("XmlFilePath  : {0}".Fill(xmlFilePath));
            //                    string zipFilePath = Path.Combine(destinationPath, "{0}.zip".Fill(clientBatchId));
            //                    log.Debug("ZipFilePath : {0}".Fill(zipFilePath));
            //                    string triggerFilePath = Path.Combine(destinationPath, "{0}.GO".Fill(clientBatchId));
            //                    log.Debug("TriggerFilePath : {0}".Fill(triggerFilePath));

            //                    MailBatch mailBatch = new MailBatch();
            //                    mailBatch.Id = Guid.NewGuid();
            //                    mailBatch.ClientBatchId = clientBatchId;
            //                    mailBatch.Priority = Priority.Medium;
            //                    mailBatch.ClientCode = client.AppCode;
            //                    List<MailPiece> packages = new List<MailPiece>();
            //                    foreach (var mp in selectedMPs)
            //                    {
            //                        try
            //                        {
            //                            RecipientInfo recipientInfo = new RecipientInfo();
            //                            List<Metadata> customInfo = new List<Metadata>();
            //                            ReturnInfo returnInfo = new ReturnInfo();
            //                            MailPiece package = new MailPiece();
            //                            package.BusinessFriendlyIdName = mp.BusinessFriendlyIdName;
            //                            package.BusinessFriendlyIdValue = "TEST" + mp.BusinessFriendlyIdValue;
            //                            package.DocumentOrPackageType = "TEST_" + mp.DocumentOrPackageType;
            //                            package.Id = Guid.NewGuid();

            //                            DocumentOrPackage documentOrPackage = new DocumentOrPackage();
            //                            documentOrPackage.Name = mp.FileName;

            //                            Recipient recipient = new Recipient();
            //                            recipient.FirstName = "Test1";
            //                            recipient.LastName = "Test2";

            //                            Address recipientAddress = new Address();
            //                            recipientAddress.Street = mp.RcptAddressStreet;
            //                            recipientAddress.State = mp.RcptAddressState;
            //                            recipientAddress.City = mp.RcptAddressCity;
            //                            recipientAddress.Zip = mp.RcptAddressZIP;

            //                            recipientInfo.Recipient = recipient;
            //                            recipientInfo.Address = recipientAddress;

            //                            Recipient returnRecipient = new Recipient();
            //                            returnRecipient.FirstName = "Return Test1";
            //                            returnRecipient.LastName = "Return Test2";

            //                            Address returnRecipientAddress = new Address();
            //                            returnRecipientAddress.Street = mp.RetRcptAddressStreet;
            //                            returnRecipientAddress.State = mp.RetRcptAddressState;
            //                            returnRecipientAddress.City = mp.RetRcptAddressCity;
            //                            returnRecipientAddress.Zip = mp.RetRcptAddressZIP;

            //                            returnInfo.Recipient = returnRecipient;
            //                            returnInfo.Address = returnRecipientAddress;

            //                            var mpCustomInfos = mailPieceSvc.GetMailPieceCustomInfos(mp.Id);
            //                            mpCustomInfos.ForEach(mpc =>
            //                            {
            //                                if (mpc != null)
            //                                    customInfo.Add(new Metadata() { Key = mpc.Key, DataType = (DataType)mpc.DataType, Value = mpc.Value });
            //                            });

            //                            package.CustomInfo = customInfo;
            //                            package.DocumentOrPackage = documentOrPackage;
            //                            package.RecipientInfo = recipientInfo;
            //                            package.ReturnInfo = returnInfo;

            //                            packages.Add(package);

            //                            string amFolderPath = Path.Combine(Configuration.AMRootPath, client.AppCode, mp.MailBatchId.ToString());
            //                            string amRequestPDFPath = Path.Combine(amFolderPath, Configuration.AMRequestFolder, mp.FileName);
            //                            string destinationPDFPath = Path.Combine(destinationPath, mp.FileName);
            //                            if (File.Exists(amRequestPDFPath))
            //                                File.Copy(amRequestPDFPath, destinationPDFPath, true);
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            var message = "Error occured while processing mail piece record for client : {0} MailPieceId : {1}".Fill(client.AppCode, mp.Id);
            //                            MailUtility.SendUnexpectedExceptionEmail(ex, message);
            //                            continue;
            //                        }
            //                    }

            //                    mailBatch.MailPieces = packages;

            //                    if (packages.Count == 0)
            //                    {
            //                        var msg = "Package does not contain any mail pieces for client {0}".Fill(client.AppCode);
            //                        log.Debug(msg);
            //                        MailUtility.SendUnexpectedExceptionEmail(null, msg);
            //                        continue;
            //                    }
            //                    var xml = XmlUtil.Serialize(mailBatch);
            //                    File.WriteAllText(xmlFilePath, xml);
            //                    var pdfList = Directory.EnumerateFiles(destinationPath, "*.pdf");
            //                    using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            //                    {
            //                        foreach (var pdfPath in pdfList)
            //                        {
            //                            newFile.CreateEntryFromFile(pdfPath, Path.GetFileName(pdfPath));
            //                        }
            //                        newFile.CreateEntryFromFile(xmlFilePath, Path.GetFileName(xmlFilePath));
            //                    }
            //                    File.WriteAllText(triggerFilePath, "");
            //                    //File.SetLastWriteTimeUtc(triggerFilePath, mpListByMb.Select(mp => mp.ReceivedOn).Distinct().First());
            //                    sftSite.UploadFile(zipFilePath, Path.Combine(sftChannel.ToADEDir, Path.GetFileName(zipFilePath)));
            //                    sftSite.UploadFile(triggerFilePath, Path.Combine(sftChannel.ToADEDir, Path.GetFileName(triggerFilePath)));

            //                    Directory.Delete(destinationPath, true);
            //                    log.Debug("----Batching Process completed for Client {0}----".Fill(client.AppCode));
            //                }

            //                if (sftSite.IsConnected)
            //                    sftSite.Disconnect();
            //            }
            //            catch (Exception ex)
            //            {
            //                var message = "Error occured while processing mailpieces for client :{0}".Fill(mpData.Key);
            //                MailUtility.SendUnexpectedExceptionEmail(ex, message);
            //            }
            //        }

            //        mailBatchSvc.InsertLastProcessTimeForUATBatches(mailPieces.OrderByDescending(mp => mp.ReceivedOn).Select(mp => mp.ReceivedOn).First());

            //    }
            //}
        }

        private static void GeneratePDFWithNumberOfPages(int minPageCount, string originFilePath, string destFilePath)
        {

            int pageCount = PdfUtil.GetPageCount(originFilePath);
            int loopCount = 0;
            int finalPageCount = 0;
            while (finalPageCount < minPageCount)
            {
                finalPageCount += pageCount;
                loopCount++;
            }
            List<string> filePaths = new List<string>();
            for (int i = 0; i < loopCount; i++)
            {
                filePaths.Add(originFilePath);
            }
            filePaths.Add(destFilePath);
            PdfUtil.MergePDFs(filePaths.ToArray());
        }
    }
}
