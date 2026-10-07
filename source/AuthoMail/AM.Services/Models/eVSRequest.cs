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
    public class eVSRequest : IeVSRequest
    {
        [XmlAttribute(AttributeName = "USERID")]
        public string USERID { get; set; }
        [XmlAttribute(AttributeName = "PASSWORD")]
        public string PASSWORD { get; set; }

        [XmlElement(ElementName = "ImageParameters")]
        public LabelParameters LabelParameters { get; set; }

        public string FromName { get; set; }

        public string FromFirm { get; set; }
        
        [XmlElement(ElementName = "FromAddress1")]
        public string FromAptOrSuiteorOther { get; set; }
        
        [XmlElement(ElementName = "FromAddress2")]
        public string FromStreetAddress { get; set; }

        public string FromCity { get; set; }

        public string FromState { get; set; }

        public string FromZip5 { get; set; }

        public string FromZip4 { get; set; }

        public string FromPhone { get; set; }

        public string POZipCode { get; set; }

        public bool AllowNonCleansedOriginAddr { get; set; }

        public string ToName { get; set; }

        public string ToFirm { get; set; }
        
        [XmlElement(ElementName = "ToAddress1")]
        public string ToAptOrSuiteorOther { get; set; }

        [XmlElement(ElementName = "ToAddress2")]
        public string ToStreetAddress { get; set; }

        public string ToCity { get; set; }

        public string ToState { get; set; }

        public string ToZip5 { get; set; }

        public string ToZip4 { get; set; }

        public string ToPhone { get; set; }

        public bool POBox { get; set; }

        public ToContactPreference? ToContactPreference { get; set; }

        public string ToContactMessaging { get; set; }

        public string ToContactEmail { get; set; }

        public bool AllowNonCleansedDestAddr { get; set; }

        public string WeightInOunces { get; set; }

        public ServiceType? ServiceType { get; set; }

        public Container? Container { get; set; }

        public decimal? Width { get; set; }

        public decimal? Length { get; set; }

        public decimal? Height { get; set; }

        public Machinable? Machinable { get; set; }

        public PriceOption? PriceOptions { get; set; }

        public decimal? InsuredAmount { get; set; }

        public bool AddressServiceRequested { get; set; }

        public ExpressMailOptions ExpressMailOptions { get; set; }

        public string ShipDate { get; set; }

        public string CustomerRefNo { get; set; }

        public string CustomerRefNo2 { get; set; }
        
        [XmlArrayItem(ElementName = "ExtraService")]
        public List<ExtraServiceType> ExtraServices { get; set; }

        public string CRID { get; set; }

        public string MID { get; set; }

        public string LogisticsManagerMID { get; set; }

        public string VendorCode { get; set; }

        public string VendorProductVersionNumber { get; set; }

        public string SenderName { get; set; }

        public string SenderEMail { get; set; }

        public string RecipientName { get; set; }

        public string RecipientEMail { get; set; }

        public ReceiptOption? ReceiptOption { get; set; }

        public ImageType? ImageType { get; set; }

        public HoldForManifest? HoldForManifest { get; set; }

        public bool NineDigitRoutingZip { get; set; }

        public bool ShipInfo { get; set; }

        public bool CarrierRelease { get; set; }

        public string DropOffTime { get; set; }

        public bool ReturnCommitments { get; set; }

        public bool PrintCustomerRefNo { get; set; }

        public bool PrintCustomerRefNo2 { get; set; }

        public Content Content { get; set; }

        public ActionCode? ActionCode { get; set; }

        public bool OptOutOfSPE { get; set; }

        public string SortationLevel { get; set; }

        public DestinationEntryFacilityType? DestinationEntryFacilityType { get; set; }

        public List<ItemDetail> ShippingContents { get; set; }
        public CustomsContentType? CustomsContentType { get; set; }
        public string ContentComments { get; set; }
        public RestrictionType? RestrictionType { get; set; }
        public string RestrictionComments { get; set; }
        public string AESITN { get; set; }
        public string ImportersReference { get; set; }
        public string ImportersContact { get; set; }
        public string ExportersReference { get; set; }
        public string ExportersContact { get; set; }
        public string InvoiceNumber { get; set; }
        public string LicenseNumber { get; set; }
        public string CertificateNumber { get; set; }
        public NonDeliveryOption? NonDeliveryOption { get; set; }
        public string AltReturnAddress1 { get; set; }
        public string AltReturnAddress2 { get; set; }
        public string AltReturnAddress3 { get; set; }
        public string AltReturnAddress4 { get; set; }
        public string AltReturnAddress5 { get; set; }
        public string AltReturnAddress6 { get; set; }
        public string AltReturnCountry { get; set; }
        public string LabelImportType { get; set; }
        public ePostageMailerReporting? ePostageMailerReporting { get; set; }
        public string SenderFirstName { get; set; }
        public string SenderLastName { get; set; }
        public string SenderBusinessName { get; set; }
        public string SenderAddress1 { get; set; }
        public string SenderCity { get; set; }
        public string SenderState { get; set; }
        public string SenderZip5 { get; set; }
        public string SenderPhone { get; set; }
        public string SenderEmail { get; set; }

        public bool ShouldSerializeCustomsContentType() { return this.CustomsContentType.HasValue; }
        public bool ShouldSerializeRestrictionType() { return this.RestrictionType.HasValue; }
        public bool ShouldSerializeNonDeliveryOption() { return this.NonDeliveryOption.HasValue; }
        public bool ShouldSerializeePostageMailerReporting() { return this.ePostageMailerReporting.HasValue; }

        public eVSRequest(eVSLabelRequest request)
        {
            //set everything
            this.LabelParameters = request.LabelParameters;
            this.FromName = request.FromName ?? string.Empty;
            this.FromFirm = request.FromFirm ?? string.Empty;
            this.FromAptOrSuiteorOther = request.FromAptOrSuiteorOther ?? string.Empty;
            this.FromStreetAddress = request.FromStreetAddress ?? string.Empty;
            this.FromCity = request.FromCity ?? string.Empty;
            this.FromState = request.FromState ?? string.Empty;
            this.FromZip5 = request.FromZip5 ?? string.Empty;
            this.FromZip4 = request.FromZip4 ?? string.Empty;
            this.FromPhone = request.FromPhone ?? string.Empty;
            this.POZipCode = request.POZipCode;
            this.AllowNonCleansedOriginAddr = request.AllowNonCleansedOriginAddr;
            this.ToName = request.ToName ?? string.Empty;
            this.ToFirm = request.ToFirm ?? string.Empty;
            this.ToAptOrSuiteorOther = request.ToAptOrSuiteorOther ?? string.Empty;
            this.ToStreetAddress = request.ToStreetAddress ?? string.Empty;
            this.ToCity = request.ToCity ?? string.Empty;
            this.ToState = request.ToState ?? string.Empty;
            this.ToZip5 = request.ToZip5 ?? string.Empty;
            this.ToZip4 = request.ToZip4 ?? string.Empty;
            this.ToPhone = request.ToPhone ?? string.Empty;
            this.POBox = request.POBox;
            this.ToContactPreference = request.ToContactPreference;
            this.ToContactMessaging = request.ToContactMessaging;
            this.ToContactEmail = request.ToContactEmail;
            this.AllowNonCleansedDestAddr = request.AllowNonCleansedDestAddr;
            this.WeightInOunces = request.WeightInOunces;
            this.ServiceType = request.ServiceType;
            this.Container = request.Container;
            this.Width = request.Width;
            this.Length = request.Length;
            this.Height = request.Height;
            this.Machinable = request.Machinable;
            this.PriceOptions = request.PriceOptions;
            this.InsuredAmount = request.InsuredAmount;
            this.AddressServiceRequested = request.AddressServiceRequested;
            this.ExpressMailOptions = request.ExpressMailOptions;
            this.ShipDate = request.ShipDate.HasValue ? request.ShipDate.Value.ToString("dd-MMM-yyyy") : null;
            this.CustomerRefNo = request.CustomerRefNo;
            this.CustomerRefNo2 = request.CustomerRefNo2;
            this.ExtraServices = request.ExtraServices;
            this.CRID = request.CRID;
            this.MID = request.MID;
            this.LogisticsManagerMID = request.LogisticsManagerMID;
            this.VendorCode = request.VendorCode;
            this.VendorProductVersionNumber = request.VendorProductVersionNumber;
            this.SenderName = request.SenderName;
            this.SenderEMail = request.SenderEMail;
            this.RecipientName = request.RecipientName;
            this.RecipientEMail = request.RecipientEMail;
            this.ReceiptOption = request.ReceiptOption;
            this.ImageType = request.ImageType;
            this.HoldForManifest = request.HoldForManifest;
            this.NineDigitRoutingZip = request.NineDigitRoutingZip;
            this.ShipInfo = request.ShipInfo;
            this.CarrierRelease = request.CarrierRelease;
            this.DropOffTime = request.DropOffTime.HasValue ? request.DropOffTime.Value.ToString("HH:mm") : null;
            this.ReturnCommitments = request.ReturnCommitments;
            this.PrintCustomerRefNo = request.PrintCustomerRefNo;
            this.Content = request.Content;
            this.ActionCode = request.ActionCode;
            this.OptOutOfSPE = request.OptOutOfSPE;
            this.SortationLevel = request.SortationLevel;
            this.DestinationEntryFacilityType = request.DestinationEntryFacilityType;
            
            if (request.eVSCustomsFormsData != null)
            {
                this.ShippingContents = request.eVSCustomsFormsData.ShippingContents;
                this.CustomsContentType = request.eVSCustomsFormsData.CustomsContentType;
                this.ContentComments = request.eVSCustomsFormsData.ContentComments;
                this.RestrictionType = request.eVSCustomsFormsData.RestrictionType;
                this.RestrictionComments = request.eVSCustomsFormsData.RestrictionComments;
                this.AESITN = request.eVSCustomsFormsData.AESITN;
                this.ImportersReference = request.eVSCustomsFormsData.ImportersReference;
                this.ImportersContact = request.eVSCustomsFormsData.ImportersContact;
                this.ExportersReference = request.eVSCustomsFormsData.ExportersReference;
                this.ExportersContact = request.eVSCustomsFormsData.ExportersContact;
                this.InvoiceNumber = request.eVSCustomsFormsData.InvoiceNumber;
                this.LicenseNumber = request.eVSCustomsFormsData.LicenseNumber;
                this.CertificateNumber = request.eVSCustomsFormsData.CertificateNumber;
                this.NonDeliveryOption = request.eVSCustomsFormsData.NonDeliveryOption;
                this.AltReturnAddress1 = request.eVSCustomsFormsData.AltReturnAddress1;
                this.AltReturnAddress2 = request.eVSCustomsFormsData.AltReturnAddress2;
                this.AltReturnAddress3 = request.eVSCustomsFormsData.AltReturnAddress3;
                this.AltReturnAddress4 = request.eVSCustomsFormsData.AltReturnAddress4;
                this.AltReturnAddress5 = request.eVSCustomsFormsData.AltReturnAddress5;
                this.AltReturnAddress6 = request.eVSCustomsFormsData.AltReturnAddress6;
                this.AltReturnCountry = request.eVSCustomsFormsData.AltReturnCountry;
                this.LabelImportType = request.eVSCustomsFormsData.LabelImportType;
                this.ePostageMailerReporting = request.eVSCustomsFormsData.ePostageMailerReporting;
                this.SenderFirstName = request.eVSCustomsFormsData.SenderFirstName;
                this.SenderLastName = request.eVSCustomsFormsData.SenderLastName;
                this.SenderBusinessName = request.eVSCustomsFormsData.SenderBusinessName;
                this.SenderAddress1 = request.eVSCustomsFormsData.SenderAddress1;
                this.SenderCity = request.eVSCustomsFormsData.SenderCity;
                this.SenderState = request.eVSCustomsFormsData.SenderState;
                this.SenderZip5 = request.eVSCustomsFormsData.SenderZip5;
                this.SenderPhone = request.eVSCustomsFormsData.SenderPhone;
                this.SenderEmail = request.eVSCustomsFormsData.SenderEmail;
            }
        }

        public eVSRequest()
        {

        }

        public bool ShouldSerializeWidth() { return this.Width.HasValue; }
        public bool ShouldSerializeLength() { return this.Length.HasValue; }
        public bool ShouldSerializeHeight() { return this.Height.HasValue; }
        public bool ShouldSerializeInsuredAmount() { return this.InsuredAmount.HasValue; }
        public bool ShouldSerializeToContactPreference() { return this.ToContactPreference.HasValue; }
        public bool ShouldSerializeServiceType() { return this.ServiceType.HasValue; }
        public bool ShouldSerializeContainer() { return this.Container.HasValue; }
        public bool ShouldSerializeMachinable() { return this.Machinable.HasValue; }
        public bool ShouldSerializePriceOptions() { return this.PriceOptions.HasValue; }
        public bool ShouldSerializeReceiptOption() { return this.ReceiptOption.HasValue; }
        public bool ShouldSerializeImageType() { return this.ImageType.HasValue; }
        public bool ShouldSerializeHoldForManifest() { return this.HoldForManifest.HasValue; }
        public bool ShouldSerializeActionCode() { return this.ActionCode.HasValue; }
        public bool ShouldSerializeDestinationEntryFacilityType() { return this.DestinationEntryFacilityType.HasValue; }
        public bool ShouldSerializeShipDate() { return !this.ShipDate.IsNullOrWhiteSpace(); }
        public bool ShouldSerializeDropOffTime() { return !this.DropOffTime.IsNullOrWhiteSpace(); }
    }
}
