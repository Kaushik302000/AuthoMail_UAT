using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib.TrackV2
{
    public class TrackFieldAPIRequest : ITrackFieldAPIRequest
    {
        public string Revision { get; set; }
        public string ClientIp { get; set; }
        public string SourceIdZIP { get; set; }
        [XmlElement]
        public List<TrackID> TrackID { get; set; }

        public TrackFieldAPIRequest()
        {
            this.Revision = "0";
            this.ClientIp = "127.0.0.1";
            this.TrackID = new List<TrackID>();
        }
    }

    public class TrackID
    {
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
        public string DestinationZipCode { get; set; }
        public DateTime? MailingDate { get; set; }
        public USPSError Error { get; set; }

        [DataMember]
        [XmlIgnore]
        public bool HasError
        {
            get
            {
                if (this.Error == null) return false;
                return !(String.IsNullOrWhiteSpace(this.Error.Number) &&
                         String.IsNullOrWhiteSpace(this.Error.Description));
            }
            set { }
        }

        public bool ShouldSerializeMailingDate() { return this.MailingDate.HasValue; }
    }
}
