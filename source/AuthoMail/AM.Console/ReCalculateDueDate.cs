using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.IO;
using System.Threading;
using AM.USPSLib;

namespace AM.Console
{
    public class ReCalculateDueDate
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Start()
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (ICommonService commonSvc = new CommonService())
            {
                var docTypeList = rSvc.GetDocTypeTest();
                foreach (var item in docTypeList)
                {
                    var docType = docTypeSvc.GetDocTypeById(item.DocTypeId.Value, 0);
                    DateTime estEffectiveReceivedOn = commonSvc.GetEffectiveEstReceivedOn(item.ClientId, docType, item.ReceivedOn);
                    var dueDate = commonSvc.GetDueDate(item.ClientId, docType, estEffectiveReceivedOn);
                    rSvc.UpdateDueDateAndEftRecOnEST(item.Id, dueDate, estEffectiveReceivedOn);
                }
            }
        }

        public static void UpdateErrorEffectiveReceivedOn()
        {
            //StringBuilder stringBuilder = new StringBuilder(string.Join(",", "Id", "ReceivedOn", "DueDate", "EffectiveReceivedOn", "DocTypeId", "ClientId"));
            //stringBuilder.AppendLine();
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (ICommonService commonSvc = new CommonService())
            {
                var docTypeList = rSvc.GetDocTypeTestWithErrorMP();
                foreach (var item in docTypeList)
                {
                    if (!item.DocTypeId.HasValue)
                    {
                        //NOTE : If System didn't find any doctypeid for particular mail piece then it will take default max cut-off time of that particular client for calculating effective received on date.                        
                        DateTime estMaxEffectiveReceivedOn = commonSvc.GetEffectiveEstReceivedOn(item.ClientId, docTypeSvc.GetDocTypeWithMaximumCutOffTimeByClientId(item.ClientId), item.ReceivedOn);
                        rSvc.UpdateEftRecOnEST(item.Id, estMaxEffectiveReceivedOn);
                        //item.EffectiveReceivedOnEst = estMaxEffectiveReceivedOn;
                    }
                    else
                    {
                        var docType = docTypeSvc.GetDocTypeById(item.DocTypeId.Value, 0);
                        DateTime estEffectiveReceivedOn = commonSvc.GetEffectiveEstReceivedOn(item.ClientId, docType, item.ReceivedOn);
                        rSvc.UpdateEftRecOnEST(item.Id, estEffectiveReceivedOn);
                        //item.EffectiveReceivedOnEst = estEffectiveReceivedOn;
                    }
                    //stringBuilder.AppendLine(String.Join(",", item.Id, item.ReceivedOn, item.DueDate, item.EffectiveReceivedOnEst, item.DocTypeId, item.ClientId));
                }

                //System.IO.File.WriteAllText(@"C:\Users\sagar.g\Desktop\Reports MC\Calculation.csv", stringBuilder.ToString());
            }
        }

        public static void UpdatePnSConfigDetailId()
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            {

                var listofMpToUpdate = rSvc.GetMPsToUpdatePnSConfigDetailId();
                var columnName = String.Join(",", "MPID", "DocTypeID", "PnSConfigDetailId");
                var errcolumnName = String.Join(",", "MPID", "ClientId", "ClientDocTypeId", "DocTypeID", "ShippingCarrier", "ShippingServiceType", "TotalPages");
                List<PrintBatchDetail> pbDetails = new List<PrintBatchDetail>();
                List<BatchSummaryDetail> bsDetails = new List<BatchSummaryDetail>();
                StringBuilder deliveryReport = new StringBuilder(columnName);
                StringBuilder errorReport = new StringBuilder(errcolumnName);

                deliveryReport.AppendLine();
                errorReport.AppendLine();
                foreach (var mp in listofMpToUpdate)
                {
                    DocTypeInfo doctype = new DocTypeInfo(), docTypeInfo = new DocTypeInfo();
                    try
                    {
                        doctype = rSvc.GetDocTypeById(mp.DocTypeId.Value);

                        docTypeInfo = rSvc.GetPnSConfigDetailId(mp.ClientId, mp.DocTypeId.Value, doctype.ShippingCarrier, doctype.ShippingServiceType, mp.TotalPages == 0 ? 1 : mp.TotalPages, mp.EffectiveReceivedOnEst.Value);

                        deliveryReport.AppendLine(String.Join(",", mp.Id, mp.DocTypeId, docTypeInfo.PnSConfigDetailId));

                        rSvc.UpdatePnSConfigDetailId(mp.Id, docTypeInfo.PnSConfigDetailId);

                        if (mp.ShippingStatus != "Error" && mp.PrintBatchId != null && !pbDetails.Select(pb => pb.PrintBatchId).Contains(mp.PrintBatchId.Value))
                        {
                            pbDetails.Add(new PrintBatchDetail() { PrintBatchId = mp.PrintBatchId.Value, DocTypeId = docTypeInfo.Id, PnSConfigDetailId = docTypeInfo.PnSConfigDetailId });

                            var uniqueId = rSvc.GetBatchSummaryInfo(mp.PrintBatchId.Value);
                            if (uniqueId != 0 && !bsDetails.Select(pb => pb.BatchSummaryId).Contains(uniqueId))
                                bsDetails.Add(new BatchSummaryDetail() { BatchSummaryId = uniqueId, DocTypeId = docTypeInfo.Id, PnSConfigDetailId = docTypeInfo.PnSConfigDetailId });
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured in ClientId MailPieceId :{0} For DocumentType - {1} - Shipping Carrier {2} - ShippingService {3} - Total Pages {4} With Error {5}".Fill(mp.Id, doctype.Id, doctype.ShippingCarrier, doctype.ShippingServiceType, mp.TotalPages, ex));
                        errorReport.AppendLine(String.Join(",", mp.Id, mp.ClientId, mp.ClientDocTypeId, mp.DocTypeId, doctype.ShippingCarrier, doctype.ShippingServiceType, mp.TotalPages));
                    }
                }

                foreach (var pb in pbDetails)
                {
                    rSvc.UpdatePnSConfigDetailIdInPrintBatch(pb.PrintBatchId, pb.DocTypeId, pb.PnSConfigDetailId);
                }
                foreach (var bs in bsDetails)
                {
                    rSvc.UpdatePnSConfigDetailIdInBatchSummary(bs.BatchSummaryId, bs.DocTypeId, bs.PnSConfigDetailId);
                }
                File.WriteAllText(@"D:\PnSIds.csv", deliveryReport.ToString());
                File.WriteAllText(@"D:\ErrorPnSIds.csv", errorReport.ToString());

            }
        }

        public static void RegenarateMCReports()
        {
            using (IReportService report = new ReportService())
            using (IAMCService amcSvc = new AMCService())
            using (IReCalculateDueDateService rcsvc = new ReCalculateDueDateService())
            {
                var lstPrintBatches = rcsvc.GetPrintBatchIdsForRegenarateReports();
                foreach (var pb in lstPrintBatches)
                {
                    var mps = report.GetMailPiecesDataFromPrintBatchId(pb.Id);
                    StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(mps);
                    if (deliveryReport != null)
                    {
                        string deliveryReportFileName = "Delivery-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                        //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                        //string deliveryReportFileName = "Delivery-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                        string deliveryReportPath = Path.Combine(@"D:\Reports-MC\{0}".Fill(pb.AppCode), deliveryReportFileName);

                        File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                        Thread.Sleep(1000);
                    }
                }

            }
        }

        public static void VerifyAddressesWithCity()
        {
            using (ICommonService commonSvc = new CommonService())
            using (IReCalculateDueDateService rcsvc = new ReCalculateDueDateService())
            {
                var mpList = rcsvc.GetMailPiecesWithLast6Month();
                List<MPAddress> regeneratedAddress = new List<MPAddress>();
                #region Address Validation Logic
                //Get validate Addresses for each records
                List<AddressResponse> addressResponses = new List<AddressResponse>();
                List<AddressRequest> addressRequestes = new List<AddressRequest>();
                foreach (var mailPiece in mpList) // Here system will verify only those address which have fallbackrequested=true or Edeliveryrequested=false
                {
                    AddressRequest addressRequest = new AddressRequest() { Id = mailPiece.UniqueId.ToString(), StreetAddress = mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddrOptionalLine1BelowStreet, City = mailPiece.RcptAddressCity, State = mailPiece.RcptAddressState, Zip4 = mailPiece.RcptAddressZIP.SeparateZip4And5(4), Zip5 = mailPiece.RcptAddressZIP.SeparateZip4And5(5) };

                    addressRequestes.Add(addressRequest);
                }

                int requestAddressCount = addressRequestes.Count;
                int maxAddressVerification = 5;
                int verifiedAddressCount = 0;
                double loopCount = Math.Ceiling(requestAddressCount / (float)maxAddressVerification);

                for (double i = 0; i < loopCount; i++)
                {
                    var takenAddressesList = addressRequestes.Skip(verifiedAddressCount).Take(maxAddressVerification).ToList();
                    VerifierAddressRequest verifierAddRequest = new VerifierAddressRequest() { AddressRequests = takenAddressesList };
                    verifiedAddressCount += maxAddressVerification;
                    VerifierAddressResponse verifierAddResponse;

                    using (IUSPSService uspsSvc = new USPSService())
                    {
                        verifierAddResponse = uspsSvc.VerifyAddressAsync(commonSvc.GetSystemUserId(), verifierAddRequest).Result;
                    }

                    foreach (var res in verifierAddResponse.AddressResponses)
                    {
                        var mp = mpList.Single(p => p.UniqueId.ToString() == res.Id);
                        mp.ActualRcptAddressCity = res.City;
                        mp.ActualRcptAddressState = res.State;
                        mp.ActualRcptAddressZIP = res.Zip;

                        regeneratedAddress.Add(mp);
                    }
                    //if (verifierAddResponse != null)
                    //{
                    //    if (verifierAddResponse.HasError)
                    //    {
                    //        throw new Exception("Error occured while validating Addresses : {0}".Fill(verifierAddResponse.Error));
                    //    }

                    //    addressResponses.AddRange(verifierAddResponse.AddressResponses);
                    //}
                }
                var addressInfo = GenerateAddressReport(regeneratedAddress);
                File.WriteAllText(@"D:\AddressVerify.csv", addressInfo.ToString());
                #endregion
            }
        }

        public static StringBuilder GenerateAddressReport(List<MPAddress> mailPieces)
        {
            var columnName = String.Join(",", "ClientId", "UniqueId", "Street", "City", "State", "Zip", "Actual City", "Actual State", "Actual Zip");

            StringBuilder deliveryReport = new StringBuilder(columnName);

            deliveryReport.AppendLine();

            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (var mp in mailPieces)
            {

                var line = String.Join(",", mp.clientId, mp.UniqueId, mp.RcptAddressStreet.ToCsvString(), mp.RcptAddressCity.ToCsvString(), mp.RcptAddressState, mp.RcptAddressZIP, mp.ActualRcptAddressCity.ToCsvString(), mp.ActualRcptAddressState, mp.ActualRcptAddressZIP);

                deliveryReport.AppendLine(line);
            }
            return deliveryReport;
        }
        public static void UpdateShipperSFTChannels(string[] sftMaps)
        {
            using (ICommonService commonSvc = new CommonService())
            using (IReCalculateDueDateService rcsvc = new ReCalculateDueDateService())
            {
                var sftDict = new Dictionary<int, int>();
                foreach (var sftChannel in sftMaps.Skip(1))
                {
                    var maps = sftChannel.Split(',');
                    sftDict.Add(int.Parse(maps[0]), int.Parse(maps[1]));
                }
                rcsvc.UpdateShipperUpdateShipperSFTChannel(sftDict);
            }
        }
    }

    internal class PrintBatchDetail
    {
        public int PrintBatchId { get; set; }
        public int DocTypeId { get; set; }
        public int PnSConfigDetailId { get; set; }
    }

    internal class BatchSummaryDetail
    {
        public int BatchSummaryId { get; set; }
        public int DocTypeId { get; set; }
        public int PnSConfigDetailId { get; set; }
    }
}
