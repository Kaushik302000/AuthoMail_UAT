using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Services
{
    public class PendingMailPieceInfo
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; }
        public string TrackingNo { get; set; }
        public int MailBatchId { get; set; }
        public Guid? UniqueId { get; set; }
        public bool? IsSubmitted { get; set; }
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public string PipelineStatus { get; set; }
        public string EDPipelineStatus { get; set; }
        public string ShippingStatus { get; set; }
        public string MailPieceStatus { get; set; }
        public string DocTypeCode { get; set; }
        public string DisplayDocTypeCode
        {
            get
            {
                if (this.DocTypeCode.IsNullOrWhiteSpace()) return "-";
                return this.DocTypeCode.Split('|').First();
            }
        }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public int? ClientId { get; set; }
        public string PrintLocation { get; set; }
        public string VendorCode { get; set; }
        public DateTime? MaxSubmissionTime { get; set; }
        public string MissedSLAReason { get; set; }
        public bool? IsEDeliveryRequested { get; set; }
        public bool? IsESignRequested { get; set; }
        public bool? IsInvitationSent { get; set; }
        public bool? IsFallBackRequested { get; set; }
        public string PrintTrail { get; set; }
        public double MinRemainingHours
        {
            get
            {
                if (!MaxSubmissionTime.HasValue) return 0;
                return MaxSubmissionTime.Value.Subtract(Common.Now).TotalHours;
            }
        }

        public DateTime GetEstimatedSubmittedOn(List<Sweep> sweepTimings, int bufferTime)
        {
            using (ICommonService commonSvc = new CommonService())
            {
                return commonSvc.GetStandardSubmissionTime(this.AppCode, this.VendorCode, this.ReceivedOn, sweepTimings).Add(new TimeSpan(0, bufferTime, 0));
            }
        }
    }
    public class BatchInfo
    {
        public int Id { get; set; }
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string PipelineStatus { get; set; }
        public int? ClientId { get; set; }
        public List<PendingMailPieceInfo> DelayedMailPieces { get; set; }
    }
    public class MonitorReport
    {
        public List<BatchInfo> DelayedBatches { get; set; }
        public List<PendingMailPieceInfo> DelayedMailPieces { get; set; }
        public List<BatchInfo> DelayedAMReportBackBatches { get; set; }
        public List<BatchInfo> DelayedPrinterBatches { get; set; }
        public List<PendingMailPieceInfo> DelayedAMReportBackMailPieces { get; set; }
        public List<PendingMailPieceInfo> DelayedPrinterMailPieces { get; set; }
        public List<PendingMailPieceInfo> DelayedPickUpMailPieces { get; set; }
        public bool IsReceivedDelay { get; set; }
        public bool IsProcessDelay
        {
            //get
            //{
            //    return this.DelayedMailPieces.Count > 0 || this.DelayedBatches.Count > 0;

            //}
            get;
            set;
        }
        public bool IsReportBackDelay
        {
            //get
            //{
            //    return this.DelayedAMReportBackMailPieces.Count > 0 || this.DelayedAMReportBackBatches.Count > 0;
            //}
            get;
            set;
        }
        public bool IsPrinterDelay
        {
            //get
            //{
            //    return this.DelayedPrinterMailPieces.Count > 0 || this.DelayedPrinterBatches.Count > 0;
            //}
            get;
            set;
        }

        public bool IsAMDelay
        {
            get
            {
                return this.IsReceivedDelay || this.IsProcessDelay || this.IsReportBackDelay;
            }
        }
        public bool IsPickUpDelay
        {
            get
            {
                return (DelayedPickUpMailPieces == null || DelayedPickUpMailPieces.Count() == 0) ? false : true;
            }
        }
        private bool IsCritical { get { return this.IsReceivedDelay || this.IsProcessDelay; } }
        private bool IsWarning { get { return this.IsReportBackDelay || this.IsPrinterDelay; } }
        public string AlertLevel
        {
            get
            {
                if (this.IsCritical)
                {
                    return "Critical";
                }
                else if (this.IsWarning)
                {
                    return "Warning";
                }
                else
                {
                    return "Normal";
                }
            }
        }
        public List<MonitorAppCodeCount> AppCodeCountByBatch
        {
            get
            {
                if (DelayedBatches == null) return null;
                return DelayedBatches.GroupBy(g => g.AppCode).Select(g => new MonitorAppCodeCount { AppCode = g.Key, Total = g.Count() }).ToList();
            }
        }
        public List<MonitorAppCodeCount> AppCodeCountByMailPiece
        {
            get
            {
                if (DelayedMailPieces == null) return null;
                return DelayedMailPieces.GroupBy(g => g.AppCode).Select(g => new MonitorAppCodeCount { AppCode = g.Key, Total = g.Count() }).ToList();
            }
        }
        public int MinHour
        {
            get
            {
                if (DelayedMailPieces == null) return 0;
                var lst = DelayedMailPieces.Where(d => d.MinRemainingHours > 0).Select(d => d.MinRemainingHours).ToList();
                return lst.Count() > 0 ? Convert.ToInt32(lst.Min(d => d)) : 0;
            }
        }
        public int MaxHour
        {
            get
            {
                if (DelayedMailPieces == null) return 0;
                var lst = DelayedMailPieces.Where(d => d.MinRemainingHours > 0).Select(d => d.MinRemainingHours).ToList();
                return lst.Count() > 0 ? Convert.ToInt32(lst.Max(d => d)) : 0;
            }
        }
        public DateTime? LastBatchReceivedOn { get; set; }

        public DateTime LastUpdatedOn { get { return DateTime.UtcNow; } }//Status Date
    }
    public class MonitorAppCodeCount
    {
        public string AppCode { get; set; }
        public int Total { get; set; }
    }
    public class AMGlobalStatus
    {
        public int DelayedProcessBatches { get; set; }
        public int DelayedProcessMailPieces { get; set; }
        public int DelayedAMReportBackBatches { get; set; }
        public int DelayedAMReportBackMailPieces { get; set; }
        public int DelayedPrinterBatches { get; set; }
        public int DelayedPrinterMailPieces { get; set; }
        public int DelayedPickUpMailPieces { get; set; }
        public int MinHour { get; set; }
        public int MaxHour { get; set; }
        public DateTime? LastBatchReceivedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public bool IsReportBackDelay { get; set; }
        public bool IsPrinterDelay { get; set; }
        public bool IsReceivedDelay { get; set; }
        public bool IsProcessDelay { get; set; }
        public bool IsAMDelay { get; set; }
        public string AlertLevel { get; set; }
    }
    public class ErrorNotifications
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int MailBatchId { get; set; }
        public string BatchName { get; set; }
        public int TotalMPsCount { get; set; }
        public int ErrorMPsCount { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
