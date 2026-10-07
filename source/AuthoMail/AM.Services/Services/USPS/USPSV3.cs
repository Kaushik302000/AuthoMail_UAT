using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.USPSLib;

namespace AM.Services.V3
{
    public interface IAddressV3
    {
        string streetAddress { get; set; }
        string secondaryAddress { get; set; }
        string city { get; set; }
        string state { get; set; }
        string ZIPCode { get; set; }
        string ZIPPlus4 { get; set; }
        string urbanization { get; set; }
    }
    public class eVSRequestV3
    {
        public ImageInfo imageInfo { get; set; }
        public ToAddress toAddress { get; set; }
        public FromAddress fromAddress { get; set; }
        public SenderAddress senderAddress { get; set; }
        public ReturnAddress returnAddress { get; set; }
        public PackageDescription packageDescription { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CustomsForm customsForm { get; set; }

        public eVSRequestV3(AM.USPSLib.eVSLabelRequest eVSLabelRequest)
        {
            this.imageInfo = new ImageInfo { imageType = eVSLabelRequest.ImageType.Value, labelType = eVSLabelRequest.LabelParameters.Size.Value, shipInfo = eVSLabelRequest.ShipInfo, receiptOption = eVSLabelRequest.ReceiptOption ?? ReceiptOption.None, suppressPostage = true, suppressMailDate = true, returnLabel = eVSLabelRequest.ReturnCommitments };

            this.toAddress = new ToAddress { streetAddress = eVSLabelRequest.ToStreetAddress, secondaryAddress = eVSLabelRequest.ToAptOrSuiteorOther, city = eVSLabelRequest.ToCity, state = eVSLabelRequest.ToState, ZIPCode = eVSLabelRequest.ToZip5, ZIPPlus4 = eVSLabelRequest.ToZip4, firm = eVSLabelRequest.ToFirm, email = eVSLabelRequest.ToContactEmail, phone = eVSLabelRequest.ToPhone, firstName = eVSLabelRequest.ToName, ignoreBadAddress = false, parcelLockerDelivery = false, holdForPickup = false };

            this.fromAddress = new FromAddress { streetAddress = eVSLabelRequest.FromStreetAddress, secondaryAddress = eVSLabelRequest.FromAptOrSuiteorOther, city = eVSLabelRequest.FromCity, state = eVSLabelRequest.FromState, ZIPCode = eVSLabelRequest.FromZip5, ZIPPlus4 = eVSLabelRequest.FromZip4, firm = eVSLabelRequest.FromFirm, email = eVSLabelRequest.SenderEMail, phone = eVSLabelRequest.FromPhone, firstName = eVSLabelRequest.FromName, ignoreBadAddress = false };

            this.senderAddress = new SenderAddress { streetAddress = eVSLabelRequest.FromStreetAddress, secondaryAddress = eVSLabelRequest.FromAptOrSuiteorOther, city = eVSLabelRequest.FromCity, state = eVSLabelRequest.FromState, ZIPCode = eVSLabelRequest.FromZip5, ZIPPlus4 = eVSLabelRequest.FromZip4, firm = eVSLabelRequest.FromFirm, email = eVSLabelRequest.SenderEMail, phone = eVSLabelRequest.FromPhone, firstName = eVSLabelRequest.SenderName, ignoreBadAddress = false };

            this.returnAddress = new ReturnAddress { streetAddress = eVSLabelRequest.FromStreetAddress, secondaryAddress = eVSLabelRequest.FromAptOrSuiteorOther, city = eVSLabelRequest.FromCity, state = eVSLabelRequest.FromState, ZIPCode = eVSLabelRequest.FromZip5, ZIPPlus4 = eVSLabelRequest.FromZip4, firm = eVSLabelRequest.FromFirm, email = eVSLabelRequest.SenderEMail, phone = eVSLabelRequest.FromPhone, firstName = eVSLabelRequest.SenderName, ignoreBadAddress = false };

            int weightValue = 0;
            int.TryParse(eVSLabelRequest.WeightInOunces, out weightValue);

            this.packageDescription = new PackageDescription { weight = weightValue, length = eVSLabelRequest.Length ?? 12.5m, width = eVSLabelRequest.Width ?? 9.5m, height = eVSLabelRequest.Height ?? 0.1m, /*girth = 0.0m,*/ mailClass = eVSLabelRequest.ServiceType, processingCategory = eVSLabelRequest.Machinable, destinationEntryFacilityType = eVSLabelRequest.DestinationEntryFacilityType ?? DestinationEntryFacilityType.None, mailingDate = eVSLabelRequest.ShipDate?.ToString("yyyy-MM-dd"), carrierRelease = eVSLabelRequest.CarrierRelease, packageOptions = new PackageOptions { nonDeliveryOption = eVSLabelRequest?.eVSCustomsFormsData?.NonDeliveryOption, contentType = eVSLabelRequest.Content?.ContentType }, extraServices = eVSLabelRequest.ExtraServices ?? new List<ExtraServiceType> { ExtraServiceType.USPS_Tracking_Electronic }, rateIndicator = RateIndicator.Flat_Rate_Envelope };

            if (eVSLabelRequest?.eVSCustomsFormsData != null)
            {
                this.customsForm = new CustomsForm
                {
                    contentComments = eVSLabelRequest?.eVSCustomsFormsData?.ContentComments,
                    restrictionType = eVSLabelRequest?.eVSCustomsFormsData?.RestrictionType,
                    restrictionComments = eVSLabelRequest?.eVSCustomsFormsData?.RestrictionComments,
                    AESITN = eVSLabelRequest?.eVSCustomsFormsData?.AESITN,
                    invoiceNumber = eVSLabelRequest?.eVSCustomsFormsData?.InvoiceNumber,
                    licenseNumber = eVSLabelRequest?.eVSCustomsFormsData?.LicenseNumber,
                    certificateNumber = eVSLabelRequest?.eVSCustomsFormsData?.CertificateNumber,
                    customsContentType = eVSLabelRequest?.eVSCustomsFormsData?.CustomsContentType,
                    importersReference = eVSLabelRequest?.eVSCustomsFormsData?.ImportersReference,
                    importersContact = eVSLabelRequest?.eVSCustomsFormsData?.ImportersContact,
                    exportersReference = eVSLabelRequest?.eVSCustomsFormsData?.ExportersReference,
                    exportersContact = eVSLabelRequest?.eVSCustomsFormsData?.ExportersContact,
                    contents = eVSLabelRequest?.eVSCustomsFormsData?.ShippingContents?.Select(s => new Content
                    {
                        itemDescription = s.Description,
                        itemQuantity = s.Quantity,
                        itemValue = s.Value,
                        itemTotalValue = s.Value,
                        itemWeight = (s.NetPounds + s.NetOunces),
                        itemTotalWeight = (s.NetPounds + s.NetOunces),
                        HSTariffNumber = s.HSTariffNumber,
                        countryofOrigin = s.CountryOfOrigin
                    }).ToList()
                };
            }
        }
    }

    public class Content
    {
        public string itemDescription { get; set; }
        public int? itemQuantity { get; set; }
        public decimal? itemValue { get; set; }
        public decimal? itemTotalValue { get; set; }
        public string weightUOM { get; set; }
        public decimal? itemWeight { get; set; }
        public decimal? itemTotalWeight { get; set; }
        public string HSTariffNumber { get; set; }
        public string countryofOrigin { get; set; }
        public string itemCategory { get; set; }
        public string itemSubcategory { get; set; }

        public Content()
        {
            this.weightUOM = "lb";
        }
    }

    public class CustomsForm
    {
        public string contentComments { get; set; }
        public RestrictionType? restrictionType { get; set; }
        public string restrictionComments { get; set; }
        public string AESITN { get; set; }
        public string invoiceNumber { get; set; }
        public string licenseNumber { get; set; }
        public string certificateNumber { get; set; }
        public CustomsContentType? customsContentType { get; set; }
        public string importersReference { get; set; }
        public string importersContact { get; set; }
        public string exportersReference { get; set; }
        public string exportersContact { get; set; }
        public List<Content> contents { get; set; }
    }

    public class ImageInfo
    {
        public ImageType? imageType { get; set; }
        public LabelSize? labelType { get; set; }
        public bool shipInfo { get; set; }
        public ReceiptOption? receiptOption { get; set; }
        public bool suppressPostage { get; set; }
        public bool suppressMailDate { get; set; }
        public bool returnLabel { get; set; }
    }

    public class ToAddress : FromAddress
    {
        public bool parcelLockerDelivery { get; set; }
        public bool holdForPickup { get; set; }
        public string facilityId { get; set; }
    }

    public class FromAddress : IAddressV3
    {
        public string streetAddress { get; set; }
        public string secondaryAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string ZIPCode { get; set; }
        public string ZIPPlus4 { get; set; }
        public string urbanization { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string firm { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public bool ignoreBadAddress { get; set; }
    }

    public class SenderAddress : FromAddress
    {
        public string platformUserId { get; set; }
    }

    public class ReturnAddress : FromAddress
    {

    }

    public class DestinationEntryFacilityAddress : IAddressV3
    {
        public string streetAddress { get; set; }
        public string secondaryAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string ZIPCode { get; set; }
        public string ZIPPlus4 { get; set; }
        public string urbanization { get; set; }
    }

    public class RedirectAddress : FromAddress
    {
    }

    public class PackageOptions
    {
        public int packageValue { get; set; }
        public NonDeliveryOption? nonDeliveryOption { get; set; }
        public RedirectAddress redirectAddress { get; set; }
        public ContentType? contentType { get; set; }
        public List<Container> containers { get; set; }
        public AncillaryServiceEndorsements? ancillaryServiceEndorsements { get; set; }
    }

    public class CustomerReference
    {
        public string referenceNumber { get; set; }
        public bool printReferenceNumber { get; set; }
    }

    public class PackageDescription
    {
        public string weightUOM { get; private set; }
        public int? weight { get; set; }
        public string dimensionsUOM { get; private set; }
        public decimal? length { get; set; }
        public decimal? height { get; set; }
        public decimal? width { get; set; }
        public decimal? girth { get; set; }
        public DestinationEntryFacilityAddress destinationEntryFacilityAddress { get; set; }
        public ServiceType? mailClass { get; set; }
        public RateIndicator? rateIndicator { get; set; }
        public Machinable? processingCategory { get; set; }
        public DestinationEntryFacilityType? destinationEntryFacilityType { get; set; }
        public PackageOptions packageOptions { get; set; }
        public List<CustomerReference> customerReference { get; set; }
        public List<ExtraServiceType> extraServices { get; set; }
        public string mailingDate { get; set; }
        public bool carrierRelease { get; set; }

        public PackageDescription()
        {
            this.weightUOM = "lb";
            this.dimensionsUOM = "in";
        }
    }

    public class Container
    {
        public string containerID { get; set; }
        public SortType sortType { get; set; }
    }

    public class AddressErrorResponseV3
    {
        public string apiVersion { get; set; }
        public Error error { get; set; }
    }

    public class Error
    {
        [JsonProperty("code")]
        public string code { get; set; }

        //public string message { get; set; }

        private string _message;

        [JsonProperty("message")]
        public string message
        {
            get { return _message; }
            set { _message = @value; }
        }

        [JsonProperty("errors")]
        public List<InternalError> errors { get; set; }
    }

    public class InternalError
    {
        public string status { get; set; }
        public string code { get; set; }
        public string title { get; set; }
        public string detail { get; set; }
        public Source source { get; set; }
    }

    public class TextCode
    {

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Text")]
        public string Text { get; set; }
    }

    public class Source
    {
        public string parameter { get; set; }
        public string example { get; set; }
    }

    public class eVSResponseV3
    {
        public Metadata labelMetadata { get; set; }

        public Metadata returnLabelMetadata { get; set; }

        public string labelImage { get; set; }

        public string receiptImage { get; set; }

        public string returnLabelImage { get; set; }

        public string returnReceiptImage { get; set; }

        public Error Error { get; set; }
    }

    public class ExtraService
    {
        [XmlElement(ElementName = "ServiceID")]
        [JsonProperty("ServiceID")]
        public ExtraServiceType? ServiceID { get; set; }

        [XmlElement(ElementName = "SKU")]
        [JsonProperty("SKU")]
        //[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string SKU { get; set; }

        [JsonProperty("name")]
        public string ServiceName { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }
    }

    public class Commitment
    {
        [JsonProperty("name")]
        public string CommitmentName { get; set; }

        [JsonProperty("scheduleDeliveryDate")]
        public string ScheduledDeliveryDate { get; set; }
    }

    public class LabelAddress : FromAddress
    {
        public string streetAddressAbbreviation { get; set; }
        public string cityAbbreviation { get; set; }
    }

    public class Link
    {
        public string rel { get; set; }
        public string title { get; set; }
        public string href { get; set; }
        public string method { get; set; }
        public string submissionMediaType { get; set; }
        public string targetMediaType { get; set; }
    }

    public class Metadata
    {
        public LabelAddress labelAddress { get; set; }
        public string routingInformation { get; set; }
        public string trackingNumber { get; set; }
        public string SKU { get; set; }
        public decimal? postage { get; set; }
        public List<ExtraService> extraServices { get; set; }
        public string zone { get; set; }
        public Commitment commitment { get; set; }
        public string weightUOM { get; set; }
        public decimal? weight { get; set; }
        public decimal? dimensionalWeight { get; set; }
        public List<ExtraService> fees { get; set; }
        public string permitHolderName { get; set; }
        public object inductionType { get; set; }
        public string labelBrokerID { get; set; }
        public List<Link> links { get; set; }
    }

    public class Role
    {
        public string roleName { get; set; }
        public string CRID { get; set; }
        public string MID { get; set; }
        public string manifestMID { get; set; }
        public string accountType { get; set; }
        public string accountNumber { get; set; }

        public Role()
        {

        }
    }

    public class LabelPaymentRequestV3
    {
        public List<Role> roles { get; set; }
    }

    public class LabelPaymentResponseV3
    {
        public string paymentAuthorizationToken { get; set; }
        public List<Role> roles { get; set; }
    }

    public class USPSOptions
    {
        public string WebAuthenticationKey { get; set; }

        public string WebAuthenticationPassword { get; set; }

        public string ShipmentServiceType { get; set; }

        public string ShipmentPackagingType { get; set; }

        public string CountryCode { get; set; }

        public List<Role> PaymentRole { get; set; }
        public string BaseAPIUrl { get; set; }

    }

    public class AddressResponseV3
    {
        [JsonProperty("firm")]
        public string FirmName { get; set; }

        [JsonProperty("address")]
        public Address Address { get; set; }

        [JsonProperty("additionalInfo")]
        public AdditionalInfo AdditionalInfo { get; set; }

        [JsonProperty("corrections")]
        public TextCode Corrections { get; set; }

        [JsonProperty("matches")]
        public TextCode Matches { get; set; }

        //[JsonIgnore]
        public Error Error { get; set; }
    }

    public class AdditionalInfo
    {

        [JsonProperty("deliveryPoint")]
        public string DeliveryPoint { get; set; }

        [JsonProperty("carrierRoute")]
        public string CarrierRoute { get; set; }

        [JsonProperty("DPVConfirmation")]
        public string DPVConfirmation { get; set; }

        [JsonProperty("DPVCMRA")]
        public string DPVCMRA { get; set; }

        [JsonProperty("business")]
        public string Business { get; set; }

        [JsonProperty("centralDeliveryPoint")]
        public string CentralDeliveryPoint { get; set; }

        [JsonProperty("vacant")]
        public string Vacant { get; set; }
    }

    public class USPSTokenRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
    }

    public class USPSTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public long issued_at { get; set; }
        public int expires_in { get; set; }
        public string status { get; set; }
        public string scope { get; set; }
        public string issuer { get; set; }
        public string client_id { get; set; }
        public string application_name { get; set; }
        public string api_products { get; set; }
        public string public_key { get; set; }
    }
}
