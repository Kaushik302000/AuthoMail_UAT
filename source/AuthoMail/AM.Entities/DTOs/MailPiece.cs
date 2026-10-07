using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class MailPieceDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? PnSConfigDetailId { get; set; }
        public int MailBatchId { get; set; }
        public int? PrintBatchId { get; set; }
        public Guid UniqueId { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string FilePattern { get; set; }
        public string Inserts { get; set; }
        public string DocumentOrPackageType { get; set; }
        public DateTime? DueDate { get; set; }
        public string ShippingStatus { get; set; }
        public bool IsRcptInfoSpecified { get; set; }
        public string RcptPrefix { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptSuffix { get; set; }
        public string RcptEmailId { get; set; }
        public string RcptPhone { get; set; }
        public string RcptOrganization { get; set; }
        public string CoRcptPrefix { get; set; }
        public string CoRcptFirstName { get; set; }
        public string CoRcptMiddleName { get; set; }
        public string CoRcptLastName { get; set; }
        public string CoRcptSuffix { get; set; }
        public string CoRcptPhone { get; set; }
        public string CoRcptOrganization { get; set; }
        public string RcptAddrOptionalLine1AboveStreet { get; set; }
        public string RcptAddrOptionalLine2AboveStreet { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressAptOrSuiteOrOther { get; set; }
        public string RcptAddrOptionalLine1BelowStreet { get; set; }
        public string RcptAddrOptionalLine2BelowStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string RcptAddressCountry { get; set; }
        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRcptPhone { get; set; }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public string RetRcptAddressCountry { get; set; }
        public int TotalPages { get; set; }
        public int TotalSheets { get; set; }
        public int TotalBlankPages { get; set; }
        public string BlankPageNumbers { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string TrackingNo { get; set; }
        public bool IsPullBackRequested { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public string PullBackRequestedBy { get; set; }
        public bool IsPulledBack { get; set; }
        public DateTime? PulledBackOn { get; set; }
        public bool IsPrinted { get; set; }
        public DateTime? PrintedOn { get; set; }
        public bool IsPickedUp { get; set; }
        public DateTime? PickedUpOn { get; set; }
        public bool IsShipped { get; set; }
        public DateTime? ShippedOn { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Weight { get; set; }
        public bool IsQCRequested { get; set; }
        public bool IsQCPerformed { get; set; }
        public int? DocTypeId { get; set; }
        public string PrintLocation { get; set; }
        public DateTime? EffectiveReceivedOnEst { get; set; }
        public string RcptAddressTypes { get; set; }
        public DateTime? StdSubmissionTime { get; set; }
        public DateTime? MaxSubmissionTime { get; set; }
        public string MissedSLAReason { get; set; }
        public int? PrintTrailId { get; set; }
        public Int64? PDFFileSize { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public bool IsFallBackRequested { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsPrintReady { get; set; }

        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public int? ShipperId { get; set; }
        public DateTime ReceivedOn { get; set; }
        public List<MailPieceCustomInfo> MailPieceCustomInfo { get; set; }
        public string EnvelopeName { get; set; }
        public string ReturnTrackingNo { get; set; }
        public string ReturnDeliveryStatus { get; set; }
        public DateTime? ReturnDeliveryDate { get; set; }
        public Client Client { get; set; }
        public MailBatchDTO MailBatch { get; set; }
        public DocType DocType { get; set; }
        public int UsedEnvelopeId { get; set; }
    }

    public class MailPieceCustomInfo
    {
        public int MailPieceId { get; set; }
        public string Key { get; set; }
        public DataType DataType { get; set; }
        public string Value { get; set; }
    }

    public class BatchSummary
    {
        public int PrintBatch { get; set; }
        public int ClientId { get; set; }
        public string JobName { get; set; }
        public string PipelineStatus { get; set; }
        public string ValidationStatus { get; set; }
        public string OnValidationErrors { get; set; }
        public string ValidationErrors { get; set; }
        public bool IsReceived { get; set; }
        public DateTime ReceivedOn { get; set; }
        public bool IsDownloaded { get; set; }
        public DateTime DownloadedOn { get; set; }
        public bool IsPrepped { get; set; }
        public DateTime PreppedOn { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime SubmittedOn { get; set; }
        public bool IsMRDFReceived { get; set; }
        public DateTime MRDFReceivedOn { get; set; }
        public bool IsReportPrepped { get; set; }
        public DateTime ReportPreppedOn { get; set; }
        public bool IsReportedBack { get; set; }
        public DateTime ReportedBackOn { get; set; }
        public int JulianDate { get; set; }
        public int Images { get; set; }
        public DateTime DueDate { get; set; }
        public int SLADays { get; set; }
        public string BatchStatus { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime SuspendedOn { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CancelledOn { get; set; }
        public Guid BatchIdentifier { get; set; }
        public int? DocTypeId { get; set; }
        public string PrintFileName { get; set; }
        public string PrintLocation { get; set; }
        public int PrintTrailId { get; set; }
    }

    public class InternalClientWisePnSTimings
    {
        public string AppCode { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class SPELogInfo
    {
        public Guid Id { get; set; }
        public int ClientId { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public string ErrorDesc { get; set; }
    }

    public class MBInfo
    {
        public int MailBatchId { get; set; }
        public string AppCode { get; set; }
    }
}
