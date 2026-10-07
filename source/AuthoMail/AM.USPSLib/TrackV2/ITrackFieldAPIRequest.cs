using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib.TrackV2
{
    public interface ITrackFieldAPIRequest
    {
        string Revision { get; set; }

        string ClientIp { get; set; }

        string SourceIdZIP { get; set; }

        List<TrackID> TrackID { get; set; }
    }
}
