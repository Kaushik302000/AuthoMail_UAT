using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Services
{
    public class PullBackViewModel
    {
        public Guid MailPieceUniqueId { get; set; }
        public int ClientId { get; set; }
        public int? PrintTrailBatchId { get; set; }
        public int PrintBatch { get; set; }
        public string TrackingNo { get; set; }
        public string ShippingServiceType { get; set; }
        public string AMBatchPipelineStatus { get; set; }
        public string PnSBatchPipelineStatus { get; set; }
        public string AMMailPieceStatus { get; set; }
        public string PnSMailPieceStatus { get; set; }
        public string PrintLocation { get; set; }
        public string AMErrorDescription { get; set; }
        public string PnSErrorDescription { get; set; }
        public DateTime? PCIBatchPrepEnd { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public int SigningRoomId { get; set; }
        public string SigningRoomName { get; set; }
        public bool IsInvitationSent { get; set; }
        public bool IsDocReviewed { get; set; }
        public bool IsDocDownloaded { get; set; }
        public bool IsPullBackRequested { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public bool IsPulledBack { get; set; }
        public DateTime? PulledBackOn { get; set; }
        public string UserName { get; set; }
    }

    public class PullBackRequestForm
    {
        public string PrintFileName { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public int PrintBatch { get; set; }
        public string PieceId { get; set; }
        public string Name { get; set; }
        public string VendorCode { get; set; }
    }

    public class MailPieceViewModel
    {
        public string BusinessIdValue { get; set; }
        public Guid UniqueId { get; set; }
        public string AppCode { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int TotalPages { get; set; }
        public int TotalBlankPages { get; set; }
        public bool IsPullBackRequested { get; set; }
        public string Status { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public string TrackingNo { get; set; }
        public string DocType { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public string DisplayDocType
        {
            get
            {
                if (this.DocType.IsNullOrWhiteSpace()) return "-";
                return this.DocType.Split('|').First();
            }
        }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
    }

    public class ReshipMailPieceViewModel
    {
        public string BusinessIdValue { get; set; }
        public Guid UniqueId { get; set; }
        public string AppCode { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int TotalPages { get; set; }
        public int TotalBlankPages { get; set; }
        public bool IsPullBackRequested { get; set; }
        public string Status { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public string TrackingNo { get; set; }
        public string DocType { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public string DisplayDocType
        {
            get
            {
                if (this.DocType.IsNullOrWhiteSpace()) return "-";
                return this.DocType.Split('|').First();
            }
        }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public bool IsWithinRetentionPeriod { get; set; }
        public int RetentionPeriod { get; set; }
        public int ClientId { get; set; }
        public int MailBatchId { get; set; }
        public int MailPieceId { get; set; }
    }
}
