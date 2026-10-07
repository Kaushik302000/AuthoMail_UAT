using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class ErrorReportViewModel:MailPieceCommonViewModel
    {
        public Guid UniqueId { get; set; }
        public string ClientBatchId { get; set; }
        public string Channel { get; set; }
        public string DocType { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public int BatchId { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingCarrier { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string Inserts { get; set; }
        public string PrintFileName { get; set; }
        public string EnvelopeName { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; }

    }
}