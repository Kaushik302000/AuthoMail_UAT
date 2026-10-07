using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class MailPieceCommonViewModel
    {
        public int MailPieceId { get; set; }
        public string BusinessIdValue { get; set; }
        public string FileName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string ShipmentStatus { get; set; }
        public DateTime? ShipmentStatusUpdatedOn { get; set; }

    }
}