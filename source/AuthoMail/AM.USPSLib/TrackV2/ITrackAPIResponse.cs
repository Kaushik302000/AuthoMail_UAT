using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib.TrackV2
{
    public interface ITrackAPIResponse
    {
        List<TrackInfo> TrackInfo { get; set; }
    }
}
