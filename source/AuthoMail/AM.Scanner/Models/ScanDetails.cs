using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Scanner.Models
{
    public class ScanDetails : RegisterScanDetails
    {
        public int ScanCount { get; set; }
        public int TotalCount { get; set; }
        
       
    }
    public class GetAllBatchCountDetails
    {
        public DateTime ReceivedOn { get; set; }
        public string ScannedLoans { get; set; }
        public string TotalLoans { get; set; }
        public string BatchName { get; set; }
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