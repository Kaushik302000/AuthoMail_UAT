using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class DailyDetailReportViewModel:MailPieceCommonViewModel
    {
        public Guid UniqueId { get; set; }
        public string TrackingNo { get; set; }
        public int TotalPages { get; set; }
        public int TotalBlankPages { get; set; }
        public string ClientBatchId { get; set; }
        public string Channel { get; set; }
        public string DocType { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public int BatchId { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string ClientCode { get; set; }//Client Code
        public string ShippingMethod { get; set; }
        public string ShippingCarrier { get; set; }
        public string PrintLocation { get; set; }
        public string Status { get; set; }
        public string Inserts { get; set; }
        public string PrintFileName { get; set; }
        public string EnvelopeName { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; }
    }
}