using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.Console
{
    [Serializable]
    [XmlRootAttribute(Namespace = "http://schemas.corelogic.com/2015/amc/v1/packages", IsNullable = false)]
    public class PackageRequests
    {
        [XmlElementAttribute(ElementName = "PackageInfo", IsNullable = false)]
        public virtual List<PackageInfo> Packages { get; set; }
    }

    [Serializable]
    [XmlType(TypeName = "PackageInfo")]
    [XmlRootAttribute(Namespace = "http://schemas.corelogic.com/2015/amc/v1/packages", IsNullable = true)]
    public class PackageInfo
    {
        public Guid Id { get; set; }
        public string LoanNbr { get; set; }
        [XmlElement(ElementName = "RecipientInfo", IsNullable = false)]
        public virtual RecipientInfo RecipientInfo { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string File { get; set; }
        [XmlElement(ElementName = "ReturnInfo", IsNullable = false)]
        public virtual ReturnInfo ReturnInfo { get; set; }
        [XmlElement(ElementName = "ShippingMethod", IsNullable = false)]
        public virtual ShippingMethod ShippingMethod { get; set; }
        [XmlElement(ElementName = "ShippingCredentials", IsNullable = false)]
        public ShippingCredential ShippingCredentials { get { return new ShippingCredential() { ClientCode = ShippingCredCode }; } set { this.ShippingCredCode = value.ClientCode; } }

        [XmlIgnore]
        public string ShippingCredCode { get; set; }
        public string CustomerIdentifier { get; set; }
        public string RequestId { get; set; }
        public string PackageName { get; set; }
    }

    [Serializable]
    [XmlType(TypeName = "RecipientInfo")]
    public class RecipientInfo
    {
        [XmlElementAttribute(ElementName = "Recipient", IsNullable = false)]
        public virtual List<Recipient> Recipients { get; set; }
        public string RecipientPhoneNo { get; set; }
        [XmlElementAttribute(ElementName = "Address", IsNullable = false)]
        public virtual Address RecipientAddress { get; set; }
    }

    [Serializable]
    [XmlType(TypeName = "Recipient")]
    public class Recipient
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Serializable]
    [XmlType(TypeName = "ReturnInfo")]
    public class ReturnInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ReturnPhoneNo { get; set; }
        [XmlElementAttribute(ElementName = "Address", IsNullable = false)]
        public virtual Address ReturnAddress { get; set; }
    }

    [Serializable]
    [XmlType(TypeName = "Address")]
    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    [Serializable]
    [XmlType(TypeName = "ShippingCredential")]
    public class ShippingCredential
    {
        public string ClientCode { get; set; }
    }

    [Serializable]
    [XmlType(TypeName = "ShippingMethod")]
    public enum ShippingMethod : byte
    {
        USPS = 0,
        FEDEX_2_DAY = 1,
        FEDEX_EXPRESS_SAVER = 2,
        FEDEX_GROUND = 3,
        FIRST_OVERNIGHT = 4,
        GROUND_HOME_DELIVERY = 5,
        PRIORITY_OVERNIGHT = 6,
        STANDARD_OVERNIGHT = 7
    }

    public class ClientwiseFedExAccInfo
    {
        public int ID { get; set; }
        public string ClientName { get; set; }
        public string ClientCode { get; set; }
        public string WebAuthenticationKey { get; set; }
        public string WebAuthenticationPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }
    }
}