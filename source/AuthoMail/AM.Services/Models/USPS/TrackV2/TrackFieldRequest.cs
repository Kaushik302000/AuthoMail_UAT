using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.USPSLib;
using AM.USPSLib.TrackV2;

namespace AM.Services.USPS.TrackV2
{
    public class TrackFieldRequest : ITrackFieldRequest
    {
        [XmlAttribute(AttributeName = "USERID")]
        public string USERID { get; set; }
        public string Revision { get; set; }
        public string ClientIp { get; set; }
        public string SourceIdZIP { get; set; }
        [XmlElement]
        public List<TrackID> TrackID { get; set; }

        public TrackFieldRequest(string userId, string revision, string sourceZIP)
        {
            this.Revision = revision;
            this.ClientIp = "127.0.0.1";
            this.USERID = userId;
            this.SourceIdZIP = sourceZIP;
            this.TrackID = new List<TrackID>();
        }

        public TrackFieldRequest()
        {

        }
    }
}
