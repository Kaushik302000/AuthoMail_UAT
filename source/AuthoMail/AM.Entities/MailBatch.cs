using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.Entities
{
    [Serializable]
    [XmlRootAttribute(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0", IsNullable = false)]
    public class MailBatch
    {
        public MailBatch()
        {
            this.MailPieces = new List<MailPiece>();
        }
        [XmlAttribute]
        public Guid Id { get; set; }

        [XmlAttribute]
        public string ClientCode { get; set; }

        [XmlAttribute]
        public string ClientBatchId { get; set; }

        [XmlAttribute]
        public string ChannelCode { get; set; }

        [XmlAttribute]
        public Priority Priority { get; set; }

        [XmlAttribute]
        public string DocumentOrPackageType { get; set; }

        [XmlElement(ElementName = "ReturnInfo")]
        public ReturnInfo ReturnInfo { get; set; }

        [XmlElement(ElementName = "ShippingInfo")]
        public ShippingInfo ShippingInfo { get; set; }

        [XmlArrayItem(IsNullable = false)]
        public List<MailPiece> MailPieces { get; set; }

        [XmlArrayItem(IsNullable = true)]
        public List<Metadata> CustomInfo { get; set; }
        public bool ShouldSerializeCustomInfo()
        {
            return CustomInfo != null && CustomInfo.Count > 0;
        }
    }

    [Serializable]
    [XmlType(TypeName = "MailPiece")]
    [XmlRootAttribute(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0", IsNullable = true)]
    public class MailPiece
    {
        [XmlAttribute]
        public Guid Id { get; set; }
        public bool ShouldSerializeId()
        {
            return Id != null && Id != Guid.Empty;
        }
        [XmlIgnore]
        public string ClientCode { get; set; }
        [XmlIgnore]
        public string ProductCode { get; set; }
        [XmlIgnore]
        public string Comment { get; set; }

        [XmlAttribute, DisplayName("Business Friendly Id Name")]
        public string BusinessFriendlyIdName { get; set; }

        [XmlAttribute, DisplayName("Business Friendly Id Value")]
        public string BusinessFriendlyIdValue { get; set; }

        [XmlAttribute, DisplayName("Document Or Package Type")]
        public string DocumentOrPackageType { get; set; }

        [XmlElement(ElementName = "DocumentOrPackage")]
        public DocumentOrPackage DocumentOrPackage { get; set; }

        [XmlElement(ElementName = "eDelivery")]
        public eDelivery eDelivery { get; set; }

        public bool ShouldSerializeeDelivery()
        {
            return eDelivery != null;
        }
        
        [XmlIgnore]
        public DateTime? EffectiveReceivedOnEst { get; set; }

        [XmlElement(ElementName = "RecipientInfo")]
        public RecipientInfo RecipientInfo { get; set; }

        public bool ShouldSerializeRecipientInfo()
        {
            return RecipientInfo != null;
        }

        [XmlElement(ElementName = "ReturnInfo")]
        public ReturnInfo ReturnInfo { get; set; }

        public bool ShouldSerializeReturnInfo()
        {
            return ReturnInfo != null;
        }

        [XmlElement(ElementName = "ShippingInfo")]
        public ShippingInfo ShippingInfo { get; set; }

        public bool ShouldSerializeShippingInfo()
        {
            return ShippingInfo != null;
        }

        [XmlArrayItem(IsNullable = true)]
        public List<Metadata> CustomInfo { get; set; }
        public bool ShouldSerializeCustomInfo()
        {
            return CustomInfo != null && CustomInfo.Count > 0;
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class eDelivery
    {
        [XmlAttribute(DataType = "date"), DisplayName("Expiry Date")]
        public DateTime ExpiryDate { get; set; }

        [XmlAttribute, DisplayName("Is e-Sign")]
        public bool IsEsign { get; set; }

        [XmlAttribute, DisplayName("Is PrintAndShip On Expiry")]
        public bool PrintAndShipOnExpiry { get; set; }

        public bool ShouldSerializeExpiryDate()
        {
            return ExpiryDate != null && ExpiryDate != DateTime.MinValue;
        }        
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class DocumentOrPackage
    {
        [XmlAttribute, DisplayName("Doc Type Name")]
        public string Name { get; set; }
        [XmlAttribute]
        [XmlIgnore]
        public string Pattern { get; set; }
        [XmlAttribute]
        [XmlIgnore]
        public string Base64String { get; set; }
        [XmlAttribute]
        [XmlIgnore]
        public List<string> Files { get; set; }
        [XmlAttribute]
        public string Inserts { get; set; }
        [XmlAttribute]
        [XmlIgnore]
        public string Path { get; set; }

        public bool ShouldSerializePattern()
        {
            return this.IncludePatternBaseString;
        }

        public bool ShouldSerializeBase64String()
        {
            return this.IncludePatternBaseString;
        }


        public bool ShouldSerializePath()
        {
            return this.IncludePatternBaseString;
        }

        public bool ShouldSerializeFiles()
        {
            return this.IncludePatternBaseString;
        }

        public bool ShouldSerializeInserts()
        {
            return Inserts != null;
        }

        [JsonIgnore]
        [XmlIgnore]
        public bool IncludePatternBaseString { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class RecipientInfo
    {
        [XmlElement(ElementName = "Recipient")]
        public Recipient Recipient { get; set; }
        public bool ShouldSerializeRecipient()
        {
            return Recipient != null;
        }

        [XmlElement(ElementName = "Address")]
        public Address Address { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class Recipient
    {

        [XmlAttribute, DisplayName("Organization")]
        public string Organization { get; set; }
        public bool ShouldSerializeOrganization()
        {
            return !string.IsNullOrWhiteSpace(Organization);
        }

        [XmlAttribute]
        public string Prefix { get; set; }
        public bool ShouldSerializePrefix()
        {
            return !string.IsNullOrWhiteSpace(Prefix);
        }

        [XmlAttribute, DisplayName("First Name")]
        public string FirstName { get; set; }

        [XmlAttribute, DisplayName("Middle Name")]
        public string MiddleName { get; set; }
        public bool ShouldSerializeMiddleName()
        {
            return !string.IsNullOrWhiteSpace(MiddleName);
        }

        [XmlAttribute, DisplayName("Last Name")]
        public string LastName { get; set; }

        [XmlAttribute]
        public string Suffix { get; set; }
        public bool ShouldSerializeSuffix()
        {
            return !string.IsNullOrWhiteSpace(Suffix);
        }

        [XmlAttribute]
        public string Phone { get; set; }
        public bool ShouldSerializePhone()
        {
            return !string.IsNullOrWhiteSpace(Phone);
        }
        [XmlAttribute, DisplayName("Email Address")]
        public string EmailAddress { get; set; }
        public bool ShouldSerializeEmailAddress()
        {
            return !string.IsNullOrWhiteSpace(EmailAddress);
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class Address
    {
        [XmlAttribute, DisplayName("Line1 Above Street")]
        public string OptionalLine1AboveStreet { get; set; }
        public bool ShouldSerializeOptionalLine1AboveStreet()
        {
            return !string.IsNullOrWhiteSpace(OptionalLine1AboveStreet);
        }

        [XmlAttribute, DisplayName("Line2 Above Street")]
        public string OptionalLine2AboveStreet { get; set; }
        public bool ShouldSerializeOptionalLine2AboveStreet()
        {
            return !string.IsNullOrWhiteSpace(OptionalLine2AboveStreet);
        }

        [XmlAttribute]
        public string Street { get; set; }

        [XmlAttribute, DisplayName("Apt/Suite/Other")]
        public string AptOrSuiteOrOther { get; set; }
        public bool ShouldSerializeAptOrSuiteOrOther()
        {
            return !string.IsNullOrWhiteSpace(AptOrSuiteOrOther);
        }

        [XmlAttribute, DisplayName("Line1 Below Street")]
        public string OptionalLine1BelowStreet { get; set; }
        public bool ShouldSerializeOptionalLine1BelowStreet()
        {
            return !string.IsNullOrWhiteSpace(OptionalLine1BelowStreet);
        }

        [XmlAttribute, DisplayName("Line2 Below Street")]
        public string OptionalLine2BelowStreet { get; set; }
        public bool ShouldSerializeOptionalLine2BelowStreet()
        {
            return !string.IsNullOrWhiteSpace(OptionalLine2BelowStreet);
        }

        [XmlAttribute]
        public string City { get; set; }

        [XmlAttribute]
        public string State { get; set; }

        [XmlAttribute]
        public string Zip { get; set; }
        [XmlAttribute]
        public string Country { get; set; }
        public bool ShouldSerializeCountry()
        {
            return !string.IsNullOrWhiteSpace(Country);
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class ReturnInfo
    {
        [XmlElement(ElementName = "Recipient")]
        public Recipient Recipient { get; set; }

        [XmlElement(ElementName = "Address")]
        public Address Address { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class Metadata
    {
        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public DataType DataType { get; set; }

        [XmlAttribute]
        public string Value { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class ShippingInfo
    {
        [XmlAttribute]
        public Carrier Carrier { get; set; }

        [XmlAttribute]
        public ServiceType ServiceType { get; set; }

        [XmlAttribute]
        public string AccountCode { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public enum Priority
    {
        High,
        Medium,
        Low
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public enum DataType
    {
        String,
        DateTime,
        Boolean,
        Integer,
        Float
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public enum Carrier
    {
        USPS,
        FedEx,
        UPS
    }
    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public enum ServiceType
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
}
