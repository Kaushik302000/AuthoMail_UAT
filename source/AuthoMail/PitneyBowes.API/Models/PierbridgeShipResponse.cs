using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

[XmlRoot("PierbridgeShipResponse")]
public class PierbridgeShipResponse
{
    public string Error { get; set; }
    public string CommitmentLevel { get; set; }

    [XmlArray("Rates")]
    [XmlArrayItem("Rate")]
    public List<Rate> Rates { get; set; }

    [XmlArray("Packages")]
    [XmlArrayItem("Package")]
    public List<ShipmentPackage> Packages { get; set; }

    public Address CorrectedReceiverAddress { get; set; }

    public int Zone { get; set; }

    public Localization Localization { get; set; }

    public int ShipmentID { get; set; }

    public Shipping Shipping { get; set; }

    public Customer Customer { get; set; }

    public string CustomCarrierCode { get; set; }

    public int Carrier { get; set; }

    public string CarrierName { get; set; }

    public string CarrierScac { get; set; }

    public int ServiceType { get; set; }

    public string ServiceTypeName { get; set; }

    public string ServiceTypeExternalSystemCode { get; set; }

    public double Weight { get; set; }

    public ShipmentReceiver Receiver { get; set; }

    public string ServiceIsGuaranteed { get; set; }

    public Status Status { get; set; }

    public Processing Processing { get; set; }
}
public class Rate
{
    public int RateType { get; set; }
    public decimal RateWeight { get; set; }
    public string QuoteID { get; set; }
    public string OptionID { get; set; }
    public string RateDescription { get; set; }
    public DateTime ShipDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public int DeliveryIn { get; set; }
    public string DeliveryTime { get; set; }
    public string DisplayRate { get; set; }
    public string UpdateRate { get; set; }

    [XmlArray("ChargeGroups")]
    [XmlArrayItem("ChargeGroup")]
    public List<ChargeGroup> ChargeGroups { get; set; }

    public int ServiceType { get; set; }
}
public class ChargeGroup
{
    public int ChargeGroupType { get; set; }
    public string ChargeGroupDescription { get; set; }
    public decimal ChargeGroupValue { get; set; }
    public string ChargeGroupCurrency { get; set; }
    public int ChargeGroupISOCurrency { get; set; }
    public string ChargeGroupISOCurrencySymbol { get; set; }

    [XmlArray("Charges")]
    [XmlArrayItem("Charge")]
    public List<Charge> Charges { get; set; }
}

public class Charge
{
    public int ChargeType { get; set; }
    public string ChargeDescription { get; set; }
    public decimal ChargeValue { get; set; }
    public string ChargeCurrency { get; set; }
    public int ChargeISOCurrency { get; set; }
}
public class ShipmentPackage
{
    [XmlArray("Rates")]
    [XmlArrayItem("Rate")]
    public List<Rate> Rates { get; set; }

    public Status Status { get; set; }
    public string WayBillNumber { get; set; }
    public string WayBillNumber2 { get; set; }
    public string UniqueIdentifier { get; set; }

    [XmlArray("Labels")]
    [XmlArrayItem("Output")]
    public List<LabelOutput> Labels { get; set; }

    public string PackageReference { get; set; }
    public string ShipperReference { get; set; }
    public string CustomerPackageReference { get; set; }
    public double Weight { get; set; }
    public int PackageID { get; set; }

    public Shipping Shipping { get; set; }
    public Customer Customer { get; set; }
    public string AdditionalHandling { get; set; }
    public int Oversize { get; set; }
    public int PackageType { get; set; }

    public International International { get; set; }
    public string FreightClass { get; set; }
}
public class LabelOutput
{
    public string Guid { get; set; }
    public int Type { get; set; }
    public string Description { get; set; }
    public int Format { get; set; }
    public string MimeType { get; set; }

    [XmlElement("Content")]
    public string ContentBase64 { get; set; }

    public int Stock { get; set; }
}
public class Status
{
    public int Code { get; set; }
    public string Description { get; set; }

    [XmlArray("Errors")]
    [XmlArrayItem("Error")]
    public List<Error> Errors { get; set; }
}

public class Error
{
    public string Description { get; set; }
}

public class Shipping
{
    public decimal ShippingCharge { get; set; }
    public decimal AccessorialCharge { get; set; }
    public decimal OtherCharge { get; set; }
    public decimal TotalCharge { get; set; }
    public decimal NetRate { get; set; }
    public string Currency { get; set; }
    public int ISOCurrency { get; set; }
}

public class Customer : Shipping { }

public class Address
{
    public string Street { get; set; }
    public string Locale { get; set; }
    public string Other { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string Residential { get; set; }
}

public class ShipmentReceiver
{
    public string CompanyName { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string RegionName { get; set; }
    public string CountryName { get; set; }
}

public class Localization
{
    public string TimeZoneName { get; set; }
    public string TimeZoneOffset { get; set; }
    public string TimeZoneCode { get; set; }
}

public class International
{
    public string DocumentsOnly { get; set; }
}

public class Processing
{
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public int Duration { get; set; }
    public External External { get; set; }
    public string ServerName { get; set; }
    public string UserName { get; set; }
}

public class External
{
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public int Duration { get; set; }
}
