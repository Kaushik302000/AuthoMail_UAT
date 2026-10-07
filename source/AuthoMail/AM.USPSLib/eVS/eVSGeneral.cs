using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    public class LabelParameters
    {
        [XmlElement(ElementName = "ImageParameter")]
        public LabelSize? Size { get; set; }

        public int? XCoordinate { get; set; }

        public int? YCoordinate { get; set; }

        [XmlElement(ElementName = "LabelSequence")]
        public LabelSequence Sequence { get; set; }

        public bool ShouldSerializeSize() { return this.Size.HasValue; }
        public bool ShouldSerializeXCoordinate() { return this.XCoordinate.HasValue; }
        public bool ShouldSerializeYCoordinate() { return this.YCoordinate.HasValue; }
    }

    public class LabelSequence
    {
        public int? PackageNumber { get; set; }

        public int? TotalPackages { get; set; }

        public bool ShouldSerializePackageNumber() { return this.PackageNumber.HasValue; }
        public bool ShouldSerializeTotalPackages() { return this.TotalPackages.HasValue; }
    }

    public class ExpressMailOptions
    {
        public DeliveryOption? DeliveryOption { get; set; }

        public bool WaiverOfSignature { get; set; }

        public bool ShouldSerializeDeliveryOption() { return this.DeliveryOption.HasValue; }

        public ExpressMailOptions()
        {
            //default value as per API documentation
            this.WaiverOfSignature = true;
        }
    }

    public class Content
    {
        public ContentType? ContentType { get; set; }

        public ContentDescription? ContentDescription { get; set; }

        public bool ShouldSerializeContentType() { return this.ContentType.HasValue; }
        public bool ShouldSerializeContentDescription() { return this.ContentDescription.HasValue; }
    }

    public class ItemDetail
    {
        public string Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? Value { get; set; }
        public int? NetPounds { get; set; }
        public decimal? NetOunces { get; set; }
        public string HSTariffNumber { get; set; }
        public string CountryOfOrigin { get; set; }

        public bool ShouldSerializeQuantity() { return this.Quantity.HasValue; }
        public bool ShouldSerializeValue() { return this.Value.HasValue; }
        public bool ShouldSerializeNetPounds() { return this.NetPounds.HasValue; }
        public bool ShouldSerializeNetOunces() { return this.NetOunces.HasValue; }

        public ItemDetail()
        {
            //default value as per API documentation for get only tag
            this.HSTariffNumber = string.Empty;
        }
    }

    public class Commitment
    {
        public string CommitmentName { get; set; }

        public string ScheduledDeliveryDate { get; set; }
    }

    public class CommitmentDetail
    {
        public string CommitmentName { get; set; }

        public DateTime? ScheduledDeliveryDate { get; set; }
    }

    public class ExtraService
    {
        [XmlElement(ElementName = "ServiceID")]
        public ExtraServiceType? ServiceID { get; set; }
        public string ServiceName { get; set; }
        public decimal? Price { get; set; }
    }

    //public static class AddressTypes
    //{
    //    public const string POBOX = "POBOX";
    //    public const string Military = "Military";
    //    public const string Territory = "Territory";

    //    //DC,Hawaii,Alaska and All Mainlands
    //    public const string DHAM = "DHAM";
    //}

    public static class ResponseSource
    {
        //Response by USPS
        public const string USPS = "USPS";
        //Response by This Service
        public const string CLSVC = "CLSVC";
    }
}
