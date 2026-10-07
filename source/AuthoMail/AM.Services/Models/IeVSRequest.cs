using AM.USPSLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IeVSRequest : IeVSRequestCommon, IeVSCustomsFormsData
    {
        string USERID { get; set; }
        string PASSWORD { get; set; }
        string ShipDate { get; set; }
        string DropOffTime { get; set; }
    }
}
