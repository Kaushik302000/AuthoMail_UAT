using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class HSBCConsolidateReport
    {
        public int MailPieceId { get; set; }
        public string BatchName { get; set; }
        public string StatusID { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string DocumentOrPackageType { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public int TotalPages { get; set; }
        public decimal ShippingCost { get; set; }
    }
}
