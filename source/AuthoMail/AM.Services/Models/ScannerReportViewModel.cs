using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ScanedDocument
    {
        public string PrintBatchName { get; set; }
        public string BatchName { get; set; }
        public string LoanNumber { get; set; }
        public string BarcodeValue { get; set; }
        public string TotalPages { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime ShippedOn { get; set; }
        public string MailPieceStatus { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class MailPieceDetails
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ServerIP { get; set; }
        public string AppCode { get; set; }
        public int JobId { get; set; }
    }

    public class PendingScanning
    {
        public string PrintBatchName { get; set; }
        public string BatchName { get; set; }
        public string LoanNumber { get; set; }
        public string TrackingNo { get; set; }
        public string FileName { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime DueDate { get; set; }
        public string MailPieceStatus { get; set; }
        public string FilePath { get; set; }
    }
}
