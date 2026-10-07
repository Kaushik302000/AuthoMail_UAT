using AM.USPSLib.TrackV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.USPS.TrackV2
{
    public interface ITrackFieldRequest
    {
        string USERID { get; set; }
        string Revision { get; set; }

        string ClientIp { get; set; }

        string SourceIdZIP { get; set; }

        List<TrackID> TrackID { get; set; }
    }
}
