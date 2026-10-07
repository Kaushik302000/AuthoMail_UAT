using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using Dapper;

namespace AM.Services
{
    public interface IReCalculateDueDateService : IDisposable
    {
        List<Recal> GetDocTypeTest();
        List<Recal> GetDocTypeTestWithErrorMP();
        List<Recal> GetCalcListOfMailPieces();
        List<Sweep> GetSweepTimings();
        int UpdateDueDateAndEftRecOnEST(int Id, DateTime dueDate, DateTime effectiveReceivedOnEst);
        int UpdateEftRecOnEST(int Id, DateTime effectiveReceivedOnEst);
        int UpdateSubmissionTime(int id, DateTime stdSubmissionTime, DateTime maxSubmissionTime, string missedSLAReason);
        List<Recal> GetMPsToUpdatePnSConfigDetailId();
        DocTypeInfo GetDocTypeById(int id);
        DocTypeInfo GetPnSConfigDetailId(int clientId, int doctypeId, string shippingCarrier, string shippingServiceType, int totalPages, DateTime estReceivedOn);
        int UpdatePnSConfigDetailId(int mpId, int pnsConfigDetailId);
        int UpdatePnSConfigDetailIdInPrintBatch(int printBatchId, int docTypeId, int pnsConfigDetailId);
        int UpdatePnSConfigDetailIdInBatchSummary(int batchSummaryId, int docTypeId, int pnsConfigDetailId);
        int GetBatchSummaryInfo(int printBatchId);
        List<PrintBatchReporting> GetPrintBatchIdsForRegenarateReports();
        List<UspsAPI> GetUSPSRecordsToPurge();
        List<UspsAPI> GetUSPSeVSRecordsToPurge();
        int UpdateUSPSRecords(int Id);
        int UpdateUSPSeVSRecords(int Id);

        List<MPAddress> GetMailPiecesWithLast6Month();
        List<MPAddress> GetMailPiecesForReprocessCoverLetter(DateTime receivedDateEst);
        void UpdateShipperUpdateShipperSFTChannel(Dictionary<int, int> sftMapps);
    }

    public class PrintBatchReporting
    {
        public int Id { get; set; }
        public string AppCode { get; set; }
    }

    public class ReCalculateDueDateService : ServiceBase, IReCalculateDueDateService
    {

        public List<Recal> GetDocTypeTest()
        {
            string q = "select m.Id,m.DocTypeId,m.DueDate,b.ReceivedOn from AM.MailPieces m inner join AM.MailBatches b on m.MailBatchId = b.Id where m.DocTypeId is not null and m.ShippingStatus <> 'Error' order by m.id";
            return db.Query<Recal>(q).ToList();
        }
        public List<Recal> GetDocTypeTestWithErrorMP()
        {
            string q = "select m.Id,m.DocTypeId,m.ClientId,b.ReceivedOn,m.EffectiveReceivedOnEst from AM.MailPieces m inner join AM.MailBatches b on m.MailBatchId = b.Id where m.ShippingStatus = 'Error' and m.EffectiveReceivedOnEst IS NULL order by m.id";
            return db.Query<Recal>(q).ToList();
        }

        public int UpdateDueDateAndEftRecOnEST(int Id, DateTime dueDate, DateTime effectiveReceivedOnEst)
        {
            string q = "update AM.MailPieces set DueDate=@dueDate,EffectiveReceivedOnEst=@effectiveReceivedOnEst where Id=@id";
            return db.Execute(q, new { id = Id, dueDate = dueDate, effectiveReceivedOnEst = effectiveReceivedOnEst });
        }
        public int UpdateEftRecOnEST(int Id, DateTime effectiveReceivedOnEst)
        {
            string q = "update AM.MailPieces set EffectiveReceivedOnEst=@effectiveReceivedOnEst where Id=@id";
            return db.Execute(q, new { id = Id, effectiveReceivedOnEst = effectiveReceivedOnEst });
        }

        public List<Recal> GetCalcListOfMailPieces()
        {
            string q = "select M.Id,M.UniqueId,B.ReceivedOn,M.ClientId,C.AppCode,PV.VendorCode,M.SLADays,M.DueDate,M.ShippingStatus,M.DocTypeId,M.PrintLocation,M.ShippedOn,M.EffectiveReceivedOnEst,D.BizdayCutOff,D.SatCutOff,M.MissedSLAReason,M.StdSubmissionTime,M.MaxSubmissionTime from AM.MailPieces M inner join AM.MailBatches B on M.MailbatchId = B.Id inner join [dbo].[v_DocTypes] d ON m.DocTypeId=d.Id and m.PnSConfigDetailId = d.PnSConfigDetailId inner join Common.PrintTrails PT on M.PrintTrailId = PT.Id inner join Common.PrintVendors PV on PT.PrintVendorId = PV.Id inner join Common.Clients C on M.ClientId = C.Id Where DueDate IS NOT NULL and M.EffectiveReceivedOnEst IS NOT NULL and M.PrintLocation is not null";
            return db.Query<Recal>(q).ToList();
        }

        public List<Sweep> GetSweepTimings()
        {
            return db.Query<Sweep>("select * from ufn_GetSweepTimings()").ToList();
        }

        public int UpdateSubmissionTime(int id, DateTime stdSubmissionTime, DateTime maxSubmissionTime, string missedSLAReason)
        {
            string q = "UPDATE AM.MailPieces SET StdSubmissionTime = @stdSubmissionTime,MaxSubmissionTime = @maxSubmissionTime,MissedSLAReason = @missedSLAReason FROM AM.MailPieces WHERE Id = @Id";
            return db.Execute(q, new { Id = id, stdSubmissionTime = stdSubmissionTime, maxSubmissionTime = maxSubmissionTime, missedSLAReason = missedSLAReason });
        }

        public List<Recal> GetMPsToUpdatePnSConfigDetailId()
        {
            //return db.Query<Recal>("select Id,ClientId,TotalPages,DocTypeId,ClientDocTypeId from AM.MailPieces where DocTypeId IS NOT NULL").ToList();
            return db.Query<Recal>("select Id,ClientId,TotalPages,DocTypeId,EffectiveReceivedOnEst,PrintBatchId from AM.MailPieces where DocTypeId IS NOT NULL").ToList();
        }

        public DocTypeInfo GetDocTypeById(int id)
        {
            return db.QuerySingleOrDefault<DocTypeInfo>("select Id,ShippingCarrier,ShippingServiceType from AM.DocTypes WHERE Id= @Id", new { Id = id });
        }

        public DocTypeInfo GetPnSConfigDetailId(int clientId, int doctypeId, string shippingCarrier, string shippingServiceType, int totalPages, DateTime estReceivedOn)
        {
            //return db.QuerySingleOrDefault<DocTypeInfo>("SELECT D.PnSConfigDetailId FROM [dbo].[v_DocTypes] D WHERE D.ClientId=@ClientId AND D.Id=@DocTypeId AND ISNULL(D.ShippingCarrier,'') = ISNULL(@ShippingCarrier,'') AND ISNULL(D.ShippingServiceType,'') = ISNULL(@ShippingServiceType,'') AND @TotalPages BETWEEN MinPage AND MaxPage", new { ClientId = clientId, DocTypeId = doctypeId, ShippingCarrier = shippingCarrier, ShippingServiceType = shippingServiceType, TotalPages = totalPages });
            var docTypes = db.Query<DocTypeInfo>("SELECT D.Id, D.PnSConfigDetailId, BizDayStartTime, BizDayEndTime, SatStartTime, SatEndTime FROM [dbo].[v_DocTypes] D WHERE D.ClientId=@ClientId AND D.Id=@DocTypeId AND ISNULL(D.ShippingCarrier,'') = ISNULL(@ShippingCarrier,'') AND ISNULL(D.ShippingServiceType,'') = ISNULL(@ShippingServiceType,'') AND @TotalPages BETWEEN MinPage AND MaxPage", new { ClientId = clientId, DocTypeId = doctypeId, ShippingCarrier = shippingCarrier, ShippingServiceType = shippingServiceType, TotalPages = totalPages }).ToList();

            if (docTypes.Count == 0)
            {
                return null;
            }
            else if (docTypes.Count == 1)
            {
                return docTypes.Single();
            }
            else
            {
                var estReceivedOnDate = estReceivedOn.Date;
                if (estReceivedOn.DayOfWeek == DayOfWeek.Sunday)
                    return docTypes.Where(dt => estReceivedOn >= estReceivedOnDate + dt.SunStartTime && estReceivedOn <= estReceivedOnDate + dt.SunEndTime).First();
                else if (estReceivedOn.DayOfWeek == DayOfWeek.Saturday)
                    return docTypes.Where(dt => estReceivedOn >= estReceivedOnDate + dt.SatStartTime && estReceivedOn <= estReceivedOnDate + dt.SatEndTime).First();
                else
                    return docTypes.Where(dt => estReceivedOn >= estReceivedOnDate + dt.BizdayStartTime && estReceivedOn <= estReceivedOnDate + dt.BizdayEndTime).First();
            }
        }

        public int UpdatePnSConfigDetailId(int mpId, int pnsConfigDetailId)
        {
            return db.Execute("Update AM.MailPieces set PnSConfigDetailId = @PnSConfigDetailId where Id = @mpId", new { PnSConfigDetailId = pnsConfigDetailId, mpId = mpId });
        }

        public int UpdatePnSConfigDetailIdInPrintBatch(int printBatchId, int docTypeId, int pnsConfigDetailId)
        {
            return db.Execute("Update AM.PrintBatches set PnSConfigDetailId = @PnSConfigDetailId where Id = @PrintBatchId AND DocTypeId=@DocTypeId", new { PnSConfigDetailId = pnsConfigDetailId, PrintBatchId = printBatchId, DocTypeId = docTypeId });
        }

        public int UpdatePnSConfigDetailIdInBatchSummary(int batchSummaryId, int docTypeId, int pnsConfigDetailId)
        {
            return db.Execute("Update PnS.BatchSummary set PnSConfigDetailId = @PnSConfigDetailId where PrintBatch = @BatchSummaryId AND DocTypeId=@DocTypeId", new { PnSConfigDetailId = pnsConfigDetailId, BatchSummaryId = batchSummaryId, DocTypeId = docTypeId });
        }

        public int GetBatchSummaryInfo(int printBatchId)
        {
            return db.QuerySingleOrDefault<int>("SELECT PrintBatch FROM PnS.BatchSummary WHERE BatchIdentifier = (SELECT UniqueId FROM AM.PrintBatches WHERE Id=@PrintBatchId)", new { PrintBatchId = printBatchId });
        }

        public List<PrintBatchReporting> GetPrintBatchIdsForRegenarateReports()
        {
            return db.Query<PrintBatchReporting>("select * from AM.PrintBatches WHERE Id>=388646 AND ReportPreppedOn >'2020-01-28 03:05:52.137' AND AppCode IN ('USAA','BECU','HSBC','BOTW') ORDER BY ReportPreppedOn,Id", new { }).ToList();
        }

        public List<UspsAPI> GetUSPSRecordsToPurge()
        {
            string q = @"DECLARE @Offset AS nvarchar(256)
                        SET @Offset = (SELECT current_utc_offset FROM sys.time_zone_info WHERE NAME = N'US Eastern Standard Time') select Id,APIRequestedOn from [API].[USPS] where APIRequestedOn <= DATEADD(MONTH, -2, DATEADD(hour,case when @Offset = '-04:00' then -4 when @Offset = '-05:00' then -5 end,GETUTCDATE())) and USPSResponse IS NOT NULL";
            return db.Query<UspsAPI>(q).ToList();
        }

        public List<UspsAPI> GetUSPSeVSRecordsToPurge()
        {
            string q = @"DECLARE @Offset AS nvarchar(256) SET @Offset = (SELECT current_utc_offset FROM sys.time_zone_info WHERE            NAME = N'US Eastern Standard Time') select Id,APIRequestedOn from [API].[USPSeVS] where APIRequestedOn             <= DATEADD(MONTH, -2, DATEADD(hour,case when @Offset = '-04:00' then -4 when @Offset = '-05:00' then -5             end,GETUTCDATE())) and USPSResponse IS NOT NULL";
            return db.Query<UspsAPI>(q).ToList();
        }

        public int UpdateUSPSRecords(int Id)
        {
            string q = "UPDATE [API].[USPS] SET USPSRequest = null, USPSResponse = null, APIResponse = null WHERE Id = @Id";
            return db.Execute(q, new { Id = Id });
        }

        public int UpdateUSPSeVSRecords(int Id)
        {
            string q = "UPDATE [API].[USPSeVS] SET USPSResponse = null, APIResponse = null WHERE Id = @Id";
            return db.Execute(q, new { Id = Id });
        }

        public List<MPAddress> GetMailPiecesWithLast6Month()
        {
            string q = @"select RcptAddressStreet, RcptAddrOptionalLine1BelowStreet,RcptAddressCity,RcptAddressState,RcptAddressZIP,ClientId,UniqueId FROM AM.MailPieces WHERE ShippingStatus='Shipped' AND EffectiveReceivedOnEst >='2019-10-01' AND EffectiveReceivedOnEst <'2020-04-01'";
            return db.Query<MPAddress>(q).ToList();
        }

        public List<MPAddress> GetMailPiecesForReprocessCoverLetter(DateTime receivedDateEst)
        {
            string q = @"SELECT P.clientId,P.Id,P.UniqueId,P.FileName,P.TrackingNo,P.RcptFirstName,P.RcptLastName, P.RcptAddressStreet, P.RcptAddrOptionalLine1BelowStreet,P.RcptAddressCity,P.RcptAddressState,P.RcptAddressZIP,REQ.JobId AS PnSJobId,REQ.PDFFileName FROM AM.MailPieces P INNER JOIN PnS.MRDFRequest REQ ON P.UniqueId=REQ.UniqueId WHERE ClientId=101 AND P.TotalSheets>=1 AND P.TotalSheets<=7 AND PrintLocation='Fort Mill, SC' AND EffectiveReceivedOnEst >=@EffectiveReceivedOnEst";
            return db.Query<MPAddress>(q, new { EffectiveReceivedOnEst = receivedDateEst }).ToList();
        }
        public void UpdateShipperUpdateShipperSFTChannel(Dictionary<int, int> sftMapps)
        {
            foreach (var sftInfo in sftMapps)
            {
                string q = $"UPDATE [Common].[ShipperInformation] SET SFTChannelId = {sftInfo.Value} WHERE SFTChannelId = {sftInfo.Key}";
                db.Execute(q);
            }
            return;
        }
    }
    public class Recal
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int? DocTypeId { get; set; }
        public int? ClientDocTypeId { get; set; }
        public int ClientId { get; set; }
        public string AppCode { get; set; }
        public string VendorCode { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string ShippingStatus { get; set; }
        public string PrintLocation { get; set; }
        public DateTime? ShippedOn { get; set; }
        public DateTime? EffectiveReceivedOnEst { get; set; }
        public string BizdayCutOff { get; set; }
        public string SatCutOff { get; set; }
        public string MissedSLAReason { get; set; }
        public DateTime? StdSubmissionTime { get; set; }
        public DateTime? MaxSubmissionTime { get; set; }
        public int TotalPages { get; set; }
        public int? PrintBatchId { get; set; }
    }

    public class MPAddress
    {
        public int clientId { get; set; }
        public int Id { get; set; }
        public int PnSJobId { get; set; }
        public Guid UniqueId { get; set; }
        public string PDFFileName { get; set; }
        public string TrackingNo { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddrOptionalLine1BelowStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string ActualRcptAddressCity { get; set; }
        public string ActualRcptAddressState { get; set; }
        public string ActualRcptAddressZIP { get; set; }
    }
    public class DocTypeInfo
    {
        public int Id { get; set; }
        public int PnSConfigDetailId { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public TimeSpan? BizdayStartTime { get; set; }
        public TimeSpan? BizdayEndTime { get; set; }
        public TimeSpan? SatStartTime { get; set; }
        public TimeSpan? SatEndTime { get; set; }
        public TimeSpan? SunStartTime { get; set; }
        public TimeSpan? SunEndTime { get; set; }
    }

    public class UspsAPI
    {
        public int Id { get; set; }
        public DateTime APIRequestedOn { get; set; }
    }
}
