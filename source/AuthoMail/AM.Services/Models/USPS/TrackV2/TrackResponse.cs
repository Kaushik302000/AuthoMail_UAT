using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.USPSLib.TrackV2;

namespace AM.Services.USPS.TrackV2
{
    public class TrackResponse : ITrackResponse
    {
        [XmlElement]
        public List<TrackInfo> TrackInfo { get; set; }
    }
}
