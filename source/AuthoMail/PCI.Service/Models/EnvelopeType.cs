using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    public class DocType
    {
        public int DocTypeId { get; set; }
        public int PnSConfigDetailId { get; set; }
        public int PnSConfigId { get; set; }
        public string DocTypeName { get; set; }
        public string PrintSize { get; set; }
        public string PrintFormat { get; set; }
        public string ContentScalePercent { get; set; }
        public string ClientEmails { get; set; }
        public string SupportEmails { get; set; }
        public int ClientId { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public bool IsBRE { get; set; }
    }
}
