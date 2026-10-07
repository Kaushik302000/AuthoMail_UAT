using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    public class eVSLabelRequest : IeVSLabelRequest
    {
        public LabelParameters LabelParameters { get; set; }

        public string FromName { get; set; }

        public string FromFirm { get; set; }

        public string FromAptOrSuiteorOther { get; set; }

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

        public string ToAptOrSuiteorOther { get; set; }

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

        public DateTime? ShipDate { get; set; }

        public string CustomerRefNo { get; set; }

        public string CustomerRefNo2 { get; set; }

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

        public TimeSpan? DropOffTime { get; set; }

        public bool ReturnCommitments { get; set; }

        public bool PrintCustomerRefNo { get; set; }

        public bool PrintCustomerRefNo2 { get; set; }

        public Content Content { get; set; }

        public ActionCode? ActionCode { get; set; }

        public bool OptOutOfSPE { get; set; }

        public string SortationLevel { get; set; }

        public DestinationEntryFacilityType? DestinationEntryFacilityType { get; set; }
        public eVSCustomsFormsData eVSCustomsFormsData { get; set; }

        public eVSLabelRequest()
        {
            //default value as per API documentation
            this.OptOutOfSPE = true;
            this.FromName = string.Empty;
            this.FromFirm = string.Empty;
            this.FromAptOrSuiteorOther = string.Empty;
            this.FromStreetAddress = string.Empty;
            this.FromCity = string.Empty;
            this.FromState = string.Empty;
            this.FromZip5 = string.Empty;
            this.FromZip4 = string.Empty;
            this.FromPhone = string.Empty;

            this.ToName = string.Empty;
            this.ToFirm = string.Empty;
            this.ToAptOrSuiteorOther = string.Empty;
            this.ToStreetAddress = string.Empty;
            this.ToCity = string.Empty;
            this.ToState = string.Empty;
            this.ToZip5 = string.Empty;
            this.ToZip4 = string.Empty;
            this.ToPhone = string.Empty;
        }
    }
}
