using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Models
{
    public class SupplyInventoryVM
    {
        public int Id { get; set; }
        public string EnvelopeName { get; set; }
        public string PrintLocation { get; set; }
        public string PrintVendorCode { get; set; }
        public int ShipperId { get; set; }
        public string Shipper { get; set; }
        public string PartNumber { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int CurrentQuantity { get; set; }
    }
}
