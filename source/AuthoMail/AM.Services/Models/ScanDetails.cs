using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ScanDetails : RegisterScanDetails
    {
        public int ScanCount { get; set; }
        public int TotalCount { get; set; }
        public bool IsBulk { get; set; }
    }
    public class BulkScanDetails
    {
        public int ScanCount { get; set; }
        public int TotalCount { get; set; }
        public bool IsBulk { get; set; }
        public List<RegisterScanDetails> MailPieces { get; set; }
    }
    public class GetAllBatchCountDetails
    {
        public DateTime ReceivedOn { get; set; }
        public string AppCode { get; set; }
        public string ScannedLoans { get; set; }
        public string TotalLoans { get; set; }
        public string BatchName { get; set; }
    }

    public class AppCodeBatchDetails
    {
        public string AppCode { get; set; }
        public List<GetAllBatchCountDetails> Batches { get; set; }
    }
    public class AppCodeBatchSummary
    {
        public string AppCode { get; set; }
        public Dictionary<string, object> Batches { get; set; }
    }

    public class RegisterScanDetails
    {
        public string TrackingNo { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string JobName { get; set; }
        public string AppCode { get; set; }
    }

    public class USPSScanDetails
    {
        public string TrackingNo { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string MailPieceId { get; set; }
        public string AppCode { get; set; }
    }
}
