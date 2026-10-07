using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    public class eVSCancelLabelResponse : IeVSCancelLabelResponse
    {
        public string BarcodeNumber { get; set; }

        public string TrackingNumber
        {
            get
            {
                if (!String.IsNullOrEmpty(this.BarcodeNumber) && this.BarcodeNumber.Length == 30)
                    return this.BarcodeNumber.Substring(8);
                else
                    return this.BarcodeNumber;
            }
        }

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
