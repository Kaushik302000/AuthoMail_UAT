using iTextSharp.text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace AM.Services.PB
{
    [XmlRoot("PierbridgeShipRequest")]
    public class PierbridgeShipRequest
    {
        public int Carrier { get; set; }
        public int ServiceType { get; set; }
        public Receiver Receiver { get; set; }
        public Packages Packages { get; set; }
        public string UserName { get; set; }
    }

    public class Receiver
    {
        public string CompanyName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class Packages
    {
        [XmlElement("Package")]
        public List<Package> PackageList { get; set; }
    }

    public class Package
    {
        public string ShipperReference { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public int PackageType { get; set; }
        public double Weight { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string ContentDescription { get; set; }
    }
}