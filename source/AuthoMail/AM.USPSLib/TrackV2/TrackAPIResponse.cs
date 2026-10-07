using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib.TrackV2
{
    public class TrackAPIResponse : ITrackAPIResponse
    {
        [XmlElement]
        public List<TrackInfo> TrackInfo { get; set; }

        public string Error { get; set; }
        public bool HasError { get { return !string.IsNullOrWhiteSpace(this.Error); } }

        public TrackAPIResponse()
        {
            this.TrackInfo = new List<TrackInfo>();
        }

        public TrackAPIResponse(List<TrackInfo> trackInfo)
        {
            this.TrackInfo = trackInfo;
        }
    }

    public class TrackInfo
    {
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
        public string GuaranteedDeliveryDate { get; set; }
        public bool? eSOFEligible { get; set; }
        public TrackSummary TrackSummary { get; set; }

        //[XmlArrayItem(Type = typeof(TrackDetail))]
        [XmlElement]
        public List<TrackDetail> TrackDetail { get; set; }

        public USPSError Error { get; set; }

        public bool ShouldSerializeeSOFEligible() { return this.eSOFEligible.HasValue; }
    }

    public class TrackSummary
    {
        public string EventTime { get; set; }
        public string EventDate { get; set; }
        public string Event { get; set; }
        public string EventCity { get; set; }
        public string EventState { get; set; }
        public string EventZIPCode { get; set; }
        public string EventCountry { get; set; }
        public string FirmName { get; set; }
        public string Name { get; set; }
        public string EventCode { get; set; }
        public string ActionCode { get; set; }
        public string ReasonCode { get; set; }
    }
    public class TrackDetail : TrackSummary
    {
        public string AuthorizedAgent { get; set; }
    }

    //[Serializable]
    //[XmlRoot(ElementName = "Error")]
    //public class USPSError
    //{
    //    [XmlElement(ElementName = "Number")]
    //    public string Number { get; set; }

    //    [XmlElement(ElementName = "Source")]
    //    public string Source { get; set; }

    //    [XmlElement(ElementName = "Description")]
    //    public string Description { get; set; }

    //    public string HelpFile { get; set; }
    //    public string HelpContext { get; set; }
    //}
}
