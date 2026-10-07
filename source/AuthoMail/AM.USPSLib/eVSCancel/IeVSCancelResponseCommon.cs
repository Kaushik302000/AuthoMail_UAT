using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    public interface IeVSCancelResponseCommon
    {
        string BarcodeNumber { get; set; }

        eVSCancelStatus? Status { get; set; }

        eVSCancelReason? Reason { get; set; }

        USPSError Error { get; set; }
    }
}
