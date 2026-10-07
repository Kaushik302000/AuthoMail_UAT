using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    [DataContract]
    [Serializable]
    [XmlRoot(ElementName = "Error")]
    public class USPSError
    {
        [DataMember]
        [XmlElement(ElementName = "Number")]
        public string Number { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Source")]
        public string Source { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }

        [DataMember]
        [XmlElement(ElementName = "HelpFile")]
        public string HelpFile { get; set; }

        [DataMember]
        [XmlElement(ElementName = "HelpContext")]
        public string HelpContext { get; set; }
    }
}
