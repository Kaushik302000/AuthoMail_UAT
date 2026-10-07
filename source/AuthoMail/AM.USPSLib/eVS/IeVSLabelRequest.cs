using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    public interface IeVSLabelRequest : IeVSRequestCommon
    {
        DateTime? ShipDate { get; set; }
        TimeSpan? DropOffTime { get; set; }
    }
}
