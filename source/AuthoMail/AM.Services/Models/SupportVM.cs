using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class NotReportedBackMailPieces
    {
        public int BatchId { get; set; }
        public string PrintLocation { get; set; }
        public string PrintVendorName { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public string PrintBatchName { get; set; }
        public string AppCode { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public int TotalPackages { get; set; }
        public int TotalPages { get; set; }
    }
    public class MailPieceByPrintBatch
    {
        public int PieceId { get; set; }
        public string PDFFileName { get; set; }
        public Guid UniqueId { get; set; }
        public string MailPieceStatus { get; set; }
        public string ProcessingTimeStamp { get; set; }
        public string Disposition { get; set; }
        public int MailPieceId { get; set; }
        public string LoanNumber { get; set; }
        public string DocumentOrPackageType { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public DateTime? EffectiveReceivedOnEst { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string ShippingStatus { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public int TotalPages { get; set; }
        public string PrintLocation { get; set; }
        public int? PrintBatchId { get; set; }
        public string PipelineStatus { get; set; }
    }
    public class MailBatchesSupport
    {
        public int MailBatchId { get; set; }
        public string MailBatchName { get; set; }
        public string Client { get; set; }
        public string PipelineStatus { get; set; }
        public string BatchStatus { get; set; }
        public string ValidationStatus { get; set; }
        public string ValidationErrors { get; set; }
        public string AppCode { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? DownloadedOn { get; set; }
    }
}
