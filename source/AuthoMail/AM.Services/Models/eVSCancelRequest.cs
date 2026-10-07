using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.Lib;
using AM.USPSLib;

namespace AM.Services
{
    public class eVSCancelRequest : IeVSCancelRequest
    {
        [XmlAttribute(AttributeName = "USERID")]
        public string USERID { get; set; }
        [XmlAttribute(AttributeName = "PASSWORD")]
        public string PASSWORD { get; set; }

        public string BarcodeNumber { get; set; }

        public eVSCancelRequest(eVSCancelLabelRequest request)
        {
            //set everything
            this.BarcodeNumber = request.BarcodeNumber ?? string.Empty;
        }

        public eVSCancelRequest()
        {

        }
    }
}
