using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.USPSLib;

namespace AM.Services
{
    public class eVSCancelResponse : IeVSCancelResponse
    {
        public string BarcodeNumber { get; set; }

        public eVSCancelStatus? Status { get; set; }

        public eVSCancelReason? Reason { get; set; }

        public USPSError Error { get; set; }

        [XmlIgnore]
        public bool HasError
        {
            get
            {
                if (this.Error == null) return false;
                return !(String.IsNullOrWhiteSpace(this.Error.Number) &&
                         String.IsNullOrWhiteSpace(this.Error.Description));
            }
            set { }
        }
    }
}
