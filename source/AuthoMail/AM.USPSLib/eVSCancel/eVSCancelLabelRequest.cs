using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    public class eVSCancelLabelRequest : IeVSCancelLabelRequest
    {
        public string BarcodeNumber { get; set; }

        public eVSCancelLabelRequest()
        {
            this.BarcodeNumber = string.Empty;
        }
    }
}
