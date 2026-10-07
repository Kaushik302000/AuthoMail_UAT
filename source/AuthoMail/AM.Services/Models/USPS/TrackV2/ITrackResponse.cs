using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.USPSLib.TrackV2;

namespace AM.Services.USPS.TrackV2
{
    public interface ITrackResponse
    {
        List<TrackInfo> TrackInfo { get; set; }
    }
}
