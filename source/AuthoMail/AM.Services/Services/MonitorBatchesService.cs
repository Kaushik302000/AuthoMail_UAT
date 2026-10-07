using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using AM.Entities;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mail;

namespace AM.Services
{
    public interface IMonitorBatchesService : IDisposable
    {
        MonitorReport GetPendingBatchesToSubmit(int cutOffMinutes);
        List<dynamic> GetPendingBatchesToReportBack();
        void SaveEmailSentTimeStamp(int mailBatchId, bool IsSubmitted);
        int CheckPrintBatchIdExist(int mailBatchId, bool IsSubmitted);
        int CheckLastEmailSentTime(int mailBatchId, bool IsSubmitted);
        void UpdateEmailTimeStamp(int mailBatchId, bool IsSubmitted);

        bool GetDelayedPackagesStatus(int effectiveCutOff);
        void SendUnexpectedExceptionEmail(string exception);
    }
    public class MonitorBatchesService : ServiceBase, IMonitorBatchesService
    {
        public MonitorReport GetPendingBatchesToSubmit(int cutOffMinutes)
        {

            string qry = "[dbo].[uspalert_GetDelayedBatchesToSubmit] @cutOffMinutes";
            List<Sweep> timings = new List<Sweep>();
            bool isReceivedDelay = false;
            bool isProcessDelay = false;
            bool isReportBackDelay = false;
            bool isPrinterDelay = false;
            var lastBatchReceivedOn = new BatchInfo();
            var delayedBatches = new List<BatchInfo>();
            var delayedMailPieces = new List<PendingMailPieceInfo>();
            var delayedAMReportedBackBatches = new List<BatchInfo>();
            var delayedPrinterBatches = new List<BatchInfo>();
            var delayedReportedBackMailPiecesByAM = new List<PendingMailPieceInfo>();
            var delayedMailPiecesByPrinter = new List<PendingMailPieceInfo>();
            var delayedPickUpMailPieces = new List<PendingMailPieceInfo>();
            using (ICommonService commonSvc = new CommonService())
            {
                try
                {
                    timings = commonSvc.GetSweepTimings();
                }
                catch (Exception)
                {
                    isProcessDelay = true;
                }
            }
            lastBatchReceivedOn = db.QuerySingle<BatchInfo>("select top 1 ReceivedOn from AM.MailBatches where IsReceived = 1 order by ReceivedOn desc");
            //try
            //{
            //    lastBatchReceivedOn = db.QuerySingle<BatchInfo>("select top 1 ReceivedOn from AM.MailBatches where IsReceived = 1 order by ReceivedOn desc");
            //    if ((Common.Now - lastBatchReceivedOn.ReceivedOn).TotalMinutes > 60)
            //    {
            //        var triggerFiles = EnumerateBatches();
            //        if (triggerFiles.Count > 0)
            //        {
            //            isReceivedDelay = true;
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    isReceivedDelay = true;
            //}
            try
            {
                var pendingMPsToSubmit = db.Query<PendingMailPieceInfo>(qry, new { cutOffMinutes = cutOffMinutes }).ToList();

                var shippingStatusNullMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus.IsNullOrEmpty() && p.MailPieceStatus != ShippingStatus.Shipped.ToString() && p.MailPieceStatus != ShippingStatus.eDelivered.ToString()).ToList();
                var clientIdNullMailPieces = pendingMPsToSubmit.Where(p => !p.ClientId.HasValue && p.MailPieceStatus != ShippingStatus.Shipped.ToString() && p.MailPieceStatus != ShippingStatus.eDelivered.ToString() && p.MailPieceStatus != ShippingStatus.eSigned.ToString()).ToList();
                var delayedErrorMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus == ShippingStatus.Error.ToString() && p.MailPieceStatus != ShippingStatus.Shipped.ToString() && p.MailPieceStatus != ShippingStatus.eDelivered.ToString()).ToList();
                var delayedPulledBackMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus == ShippingStatus.PulledBack.ToString() && p.MailPieceStatus != ShippingStatus.Shipped.ToString() && p.MailPieceStatus != ShippingStatus.eDelivered.ToString()).ToList();
                var delayedShippedMailPieces = pendingMPsToSubmit.Where(m => (m.ShippingStatus == ShippingStatus.Shipped.ToString() && m.MailPieceStatus != ShippingStatus.Shipped.ToString() && m.MailPieceStatus != ShippingStatus.eDelivered.ToString())).ToList();
                var pendingInProgressMailPieces = pendingMPsToSubmit.Where(m => m.ShippingStatus == ShippingStatus.InProgress.ToString() && (m.IsEDeliveryRequested == false && m.IsESignRequested == false)).ToList();
                //var delayedEmptyPrintLocation = pendingInProgressMailPieces.Where(p => p.PrintLocation.IsNullOrEmpty() && p.MailPieceStatus != ShippingStatus.Shipped.ToString() && p.IsEDeliveryRequested == false).ToList();
                var delayedEmptyPrintLocation = pendingInProgressMailPieces.Where(p => p.PrintLocation.IsNullOrEmpty() && p.MailPieceStatus != ShippingStatus.Shipped.ToString() && ((p.IsEDeliveryRequested == true || p.IsESignRequested == true) && p.IsFallBackRequested == true && p.PipelineStatus == BatchPipelineStatus.Validated.ToString())).ToList();

                var delayedDallasMailPieces = pendingInProgressMailPieces.Where(p => p.VendorCode == "CL_DALLAS" && p.MailPieceStatus != ShippingStatus.Shipped.ToString() && p.MailPieceStatus != ShippingStatus.eDelivered.ToString() && p.MailPieceStatus != ShippingStatus.eSigned.ToString()).ToList();

                var delayedNordisMailPieces = pendingInProgressMailPieces.Where(mp => mp.VendorCode == "NORDIS" && Common.Now > mp.GetEstimatedSubmittedOn(timings, cutOffMinutes + 15) && mp.MailPieceStatus != ShippingStatus.Shipped.ToString() && mp.MailPieceStatus != ShippingStatus.eDelivered.ToString() && mp.MailPieceStatus != ShippingStatus.eSigned.ToString()).ToList();

                var delayedEDeliveryMailPieces = pendingInProgressMailPieces.Where(e => (e.IsEDeliveryRequested == true || e.IsESignRequested == true) && e.IsInvitationSent == false).ToList();

                //Delayed PickUp MailPieces
                delayedPickUpMailPieces = pendingMPsToSubmit.Where(mp => mp.MailPieceStatus == ShippingStatus.Shipped.ToString()).ToList();

                delayedMailPieces = delayedDallasMailPieces.Concat(delayedEmptyPrintLocation).Concat(delayedShippedMailPieces).Concat(delayedPulledBackMailPieces).Concat(delayedErrorMailPieces).Concat(clientIdNullMailPieces).Concat(shippingStatusNullMailPieces).Concat(delayedNordisMailPieces).Concat(delayedEDeliveryMailPieces).ToList();

                delayedBatches = delayedMailPieces
                    .GroupBy(mp => new { mp.MailBatchId, mp.AppCode, mp.BatchName, mp.ClientId, mp.PipelineStatus, mp.ReceivedOn })
                    .Select(g => new BatchInfo { Id = g.Key.MailBatchId, AppCode = g.Key.AppCode, BatchName = g.Key.BatchName, ClientId = g.Key.ClientId, PipelineStatus = g.Key.PipelineStatus, ReceivedOn = g.Key.ReceivedOn, DelayedMailPieces = g.ToList() }).ToList();

                if (delayedMailPieces.Count > 0 || delayedBatches.Count > 0)
                {
                    isProcessDelay = true;
                }

            }
            catch (Exception)
            {
                isProcessDelay = true;
            }

            try
            {
                delayedReportedBackMailPiecesByAM = db.Query<PendingMailPieceInfo>(@";with cte as (select MB.Id, Count(PB.Id) as TotalPrintBatches, Count(case when PB.IsReportedBack = 1 then 1 else NULL end) as TotalCompletedBatches from AM.PrintBatches PB inner join AM.MailBatches MB on PB.MailBatchId = MB.Id where MB.IsReportedBack = 0 group by MB.Id) select MP.Id,MP.MailBatchId,C.AppCode,MP.ClientId,MB.BatchName,MB.ReceivedOn,MB.PipelineStatus,MP.DueDate,MP.PrintLocation,MP.ShippingStatus,MP.Uniqueid,d.DocTypeName as DocTypeCode,pcd.ShippingCarrier,pcd.ShippingServiceType,MP.MissedSLAReason,pt.TrailName as PrintTrail
from cte inner join AM.MailBatches MB ON cte.Id = Mb.Id 
INNER JOIN AM.MailPieces MP ON MB.Id = MP.MailBatchId 
left outer join [AM].[DocTypes] d ON MP.DocTypeId=d.Id 
left outer join Common.PnSConfigDetail pcd ON MP.PnSConfigDetailId = pcd.Id
left outer join Common.PrintTrails pt on MP.PrintTrailId = pt.Id 
INNER JOIN Common.Clients C ON MP.ClientId = C.Id 
where cte.TotalPrintBatches = cte.TotalCompletedBatches 
UNION Select MP.Id,MP.MailBatchId,C.AppCode,MP.ClientId,MB.BatchName,MB.ReceivedOn,MB.PipelineStatus,MP.DueDate,MP.PrintLocation,MP.ShippingStatus,MP.Uniqueid,d.DocTypeName as DocTypeCode,pcd.ShippingCarrier,pcd.ShippingServiceType,MP.MissedSLAReason,pt.TrailName as PrintTrail 
from AM.MailPieces MP 
inner join [AM].[DocTypes] d ON MP.DocTypeId=d.Id 
inner join Common.PnSConfigDetail pcd ON MP.PnSConfigDetailId = pcd.Id
inner join Common.Clients C on MP.ClientId = C.Id  
inner join AM.MailBatches MB on MP.MailBatchId = MB.Id  
inner join PnS.MrdfRequest MR on MP.UniqueId = MR.UniqueId  
inner join PnS.BatchSummary BS on MR.JobId = BS.PrintBatch  
inner join AM.PrintBatches PB on BS.BatchIdentifier = PB.UniqueId 
left outer join Common.PrintTrails pt on MP.PrintTrailId = pt.Id 
where MP.SLADays < 0 and BS.IsReportedBack = 1 and BS.PipelineStatus <> 'Cancelled' and PB.IsReportedBack = 0 and MB.IsReportedBack = 0
").ToList();

                delayedAMReportedBackBatches = delayedReportedBackMailPiecesByAM.GroupBy(mp => new { mp.MailBatchId, mp.AppCode, mp.BatchName, mp.ClientId, mp.PipelineStatus, mp.ReceivedOn }).Select(g => new BatchInfo { Id = g.Key.MailBatchId, AppCode = g.Key.AppCode, BatchName = g.Key.BatchName, ClientId = g.Key.ClientId, PipelineStatus = g.Key.PipelineStatus, ReceivedOn = g.Key.ReceivedOn, DelayedMailPieces = g.ToList() }).ToList();

                if (delayedReportedBackMailPiecesByAM.Count > 0 || delayedAMReportedBackBatches.Count > 0)
                {
                    isReportBackDelay = true;
                }
            }
            catch (Exception)
            {
                isReportBackDelay = true;
            }

            try
            {
                delayedMailPiecesByPrinter = db.Query<PendingMailPieceInfo>(@"Select MP.Id,MP.MailBatchId,C.AppCode,MP.ClientId,MB.BatchName,MB.ReceivedOn,MB.PipelineStatus,MP.PrintLocation,MP.DueDate,MP.ShippingStatus,MP.Uniqueid,d.DocTypeName as DocTypeCode,pcd.ShippingCarrier,pcd.ShippingServiceType,pt.TrailName as PrintTrail 
from AM.MailPieces MP 
inner join [AM].[DocTypes] d ON MP.DocTypeId=d.Id 
inner join Common.PnSConfigDetail pcd ON MP.PnSConfigDetailId = pcd.Id
inner join Common.Clients C on MP.ClientId = C.Id 
inner join AM.MailBatches MB on MP.MailBatchId = MB.Id 
inner join PnS.MrdfRequest MR on MP.UniqueId = MR.UniqueId 
inner join PnS.BatchSummary BS on MR.JobId = BS.PrintBatch 
left outer join Common.PrintTrails pt on MP.PrintTrailId = pt.Id 
where BS.IsSubmitted = 1 and MP.SLADays < 0 and BS.IsReportedBack = 0 and BS.PipelineStatus <> 'Cancelled'").ToList();

                delayedPrinterBatches = delayedMailPiecesByPrinter.GroupBy(mp => new { mp.MailBatchId, mp.AppCode, mp.BatchName, mp.ClientId, mp.PipelineStatus, mp.ReceivedOn }).Select(g => new BatchInfo { Id = g.Key.MailBatchId, AppCode = g.Key.AppCode, BatchName = g.Key.BatchName, ClientId = g.Key.ClientId, PipelineStatus = g.Key.PipelineStatus, ReceivedOn = g.Key.ReceivedOn, DelayedMailPieces = g.ToList() }).ToList();

                if (delayedMailPiecesByPrinter.Count > 0 || delayedPrinterBatches.Count > 0)
                {
                    isPrinterDelay = true;
                }
            }
            catch (Exception)
            {
                isPrinterDelay = true;
            }

            return new MonitorReport { DelayedBatches = delayedBatches, DelayedMailPieces = delayedMailPieces, IsReceivedDelay = isReceivedDelay, DelayedAMReportBackBatches = delayedAMReportedBackBatches, DelayedPrinterBatches = delayedPrinterBatches, DelayedAMReportBackMailPieces = delayedReportedBackMailPiecesByAM, DelayedPrinterMailPieces = delayedMailPiecesByPrinter, LastBatchReceivedOn = lastBatchReceivedOn.ReceivedOn, IsProcessDelay = isProcessDelay, IsPrinterDelay = isPrinterDelay, IsReportBackDelay = isReportBackDelay, DelayedPickUpMailPieces = delayedPickUpMailPieces };
        }

        public List<dynamic> GetPendingBatchesToReportBack()
        {
            //return db.Query("SELECT PB.PrintBatch,CC.AppCode,PB.PrintFileName,PB.Pipelinestatus,PB.ReceivedOn,PB.DownloadedOn,PB.PreppedOn,PB.SubmittedOn FROM PnS.BatchSummary PB LEFT OUTER JOIN AM.PrintBatches AP ON AP.UniqueId = PB.BatchIdentifier INNER JOIN Common.Clients CC ON CC.Id = PB.ClientId WHERE PB.IsSubmitted = 1 AND (PB.IsReportedBack = 0 OR AP.IsShippingResponseReceived = 0) AND DATEDIFF(HOUR, ReceivedOn, GETUTCDATE()) > 10 AND PB.PipelineStatus <> 'Cancelled'").ToList();
            return db.Query("SELECT X.PrintBatchId,X.AppCode,BS.PrintFileName,BS.PipelineStatus,X.ReceivedOn,X.DownloadedOn,BS.PreppedOn,BS.SubmittedOn,X.PrintLocation,X.IsShippingResponseReceived,BS.IsReportedBack FROM(	SELECT DISTINCT P.PrintBatchId,PB.UniqueId,B.ReceivedOn,B.DownloadedOn,B.PreppedOn,PB.AppCode,P.PrintLocation,PB.IsShippingResponseReceived FROM AM.MailPieces P  INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchID INNER JOIN AM.PrintBatches PB ON PB.Id=P.PrintBatchId)X LEFT OUTER JOIN PnS.BatchSummary BS ON X.UniqueId = BS.BatchIdentifier WHERE BS.IsSubmitted = 1 AND (BS.IsReportedBack = 0 OR X.IsShippingResponseReceived = 0) AND DATEDIFF(HOUR, X.ReceivedOn, GETUTCDATE()) > 10 AND BS.PipelineStatus <> 'Cancelled'").ToList();
        }

        public void SaveEmailSentTimeStamp(int mailBatchId, bool IsSubmitted)
        {
            db.Execute("INSERT INTO [Common].[MonitorBatches] ([MailBatchId],[Email_TimeStamp],[IsSubmitted]) VALUES (@MailBatchId,@Email_TimeStamp,@IsSubmitted)", new { @MailBatchId = mailBatchId, @Email_TimeStamp = DateTime.UtcNow, @IsSubmitted = IsSubmitted });
        }
        public int CheckPrintBatchIdExist(int mailBatchId, bool IsSubmitted)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT Count(MailBatchId) FROM Common.MonitorBatches WHERE MailBatchId = @MailBatchId AND IsSubmitted = @IsSubmitted", new { @MailBatchId = mailBatchId, @IsSubmitted = IsSubmitted }).ToString());
        }
        public int CheckLastEmailSentTime(int mailBatchId, bool IsSubmitted)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT DATEDIFF(hour, Email_TimeStamp, GETUTCDATE()) AS EmailTimeStamp FROM Common.MonitorBatches WHERE MailBatchId = @MailBatchId AND IsSubmitted = @IsSubmitted", new { @MailBatchId = mailBatchId, @IsSubmitted = IsSubmitted }).ToString());
        }
        public void UpdateEmailTimeStamp(int mailBatchId, bool IsSubmitted)
        {
            db.Execute("UPDATE Common.MonitorBatches SET Email_TimeStamp = GETUTCDATE(),IsSubmitted = @IsSubmitted WHERE MailBatchId = @MailBatchId", new { @MailBatchId = mailBatchId, @IsSubmitted = IsSubmitted });
        }

        public bool GetDelayedPackagesStatus(int effectiveCutOff)
        {
            int result = db.Execute("dbo.uspalert_GetDelayedPackagesStatus @p1", new { p1 = effectiveCutOff });
            return result > 0 ? false : true;
        }

        public List<string> EnumerateBatches()
        {
            List<string> triggerFilePaths = new List<string>();
            SFTChannel sftChannel;
            using (IClientService clientSvc = new ClientService())
            {
                sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode("AMC_" + Config.Common_Environment);
            }
            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000, sftChannel.Port);
            sftSite.Connect();

            triggerFilePaths = GetTriggerFilePaths(sftSite, sftChannel.ToADEDir);

            if (sftSite.IsConnected)
                sftSite.Disconnect();
            return triggerFilePaths;
        }

        private List<string> GetTriggerFilePaths(SFTUtil sftSite, string toADEDir)
        {
            var regex = new Regex(@"^(\w*)_Complete.go$", RegexOptions.IgnoreCase);

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(toADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();
            return availableTriggerFilePaths;
        }

        public void SendUnexpectedExceptionEmail(string exception)
        {
            using (SmtpClient smtpClient = new SmtpClient())
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add("akshar.vala@milestonetechnoconsulting.com");
                    msg.CC.Add("nirav.barot@milestonetechnoconsulting.com");
                    msg.Subject = "{0} : {1}".Fill(Config.Common_Environment, "Exception in Alert API");
                    msg.Priority = MailPriority.High;
                    msg.Body = "Unexpected exception occured during execution of Alert API." + Environment.NewLine + exception;
                    smtpClient.Send(msg);
                }
            }
        }
    }
}

namespace AM.Services
{
    public class Sweep
    {
        public int SrNo { get; set; }
        public string AppCode { get; set; }
        public string VendorCode { get; set; }
        public string WeekDayType { get; set; }
        public string SweepType { get; set; }
        public TimeSpan SweepTime { get; set; }
    }
}
