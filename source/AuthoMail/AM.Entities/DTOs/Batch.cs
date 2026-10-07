using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class MailBatchDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Guid UniqueId { get; set; }
        public int SFTChannelId { get; set; }
        public string ClientBatchId { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string BatchName { get; set; }
        public string PipelineStatus { get; set; }
        public string ValidationStatus { get; set; }
        public string ValidationErrors { get; set; }
        public string BatchStatus { get; set; }
        public string Priority { get; set; }
        public bool IsReceived { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public bool IsDownloaded { get; set; }
        public DateTime? DownloadedOn { get; set; }
        public bool IsValidated { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedOn { get; set; }
        public bool IsPrepped { get; set; }
        public DateTime? PreppedOn { get; set; }
        public bool IsSubmittedToShipping { get; set; }
        public DateTime? SubmittedToShippingOn { get; set; }
        public bool IsShippingResponseReceived { get; set; }
        public DateTime? ShippingResponseReceivedOn { get; set; }
        public bool IsReportPrepped { get; set; }
        public DateTime? ReportPreppedOn { get; set; }
        public bool IsReportedBack { get; set; }
        public DateTime? ReportedBackOn { get; set; }
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
        //This property will be used only to identify as E-Delivery Batch while separating. System will not store anywhere in database.
        public bool IsEDeliveryBatch { get; set; }
        public string ServerIP { get; set; }
    }

    public class MailBatchCustomInfo
    {
        public int MailBatchId { get; set; }
        public string Key { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
    }

    public class BatchData
    {
        public MailBatchDTO MailBatch { get; set; }
        public List<MailBatchCustomInfo> MailBatchCustomInfo { get; set; }
        public List<MailPieceDTO> MailPieces { get; set; }
        public List<MailPieceCustomInfo> MailPieceCustomInfo { get; set; }
    }

    public class PrintBatch
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int MailBatchId { get; set; }
        public int PrintTrailId { get; set; }
        public string AppCode { get; set; }
        public string PrintLocation { get; set; }
        public int DocTypeId { get; set; }
        public int PnSConfigDetailId { get; set; }
        public List<MailPieceDTO> MailPieces { get; set; }
    }

    public class SubmittedBatch
    {
        public int PrintBatchId { get; set; }
        public int MailBatchId { get; set; }
        public Guid UniqueId { get; set; }
        public int DocTypeId { get; set; }
        public int SFTChannelId { get; set; }
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public string SubmittedBatchName { get; set; }
        public string ResponseFilePath { get; set; }
        public string ResponseTriggerFilePath { get; set; }
    }

    public class PCISubmittedBatchDetail
    {
        public int PrintTrailBatchId { get; set; }
        public string PrintBatchName { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public int PackageCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class DALLASSubmittedBatchDetail
    {
        public string AppCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public DateTime DueDate { get; set; }
        public int PackageCount { get; set; }
        public int TotalPages { get; set; }
    }
}
