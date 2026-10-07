using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    public interface IeVSRequestCommon
    {
        LabelParameters LabelParameters { get; set; }

        string FromName { get; set; }

        string FromFirm { get; set; }

        string FromAptOrSuiteorOther { get; set; }

        string FromStreetAddress { get; set; }

        string FromCity { get; set; }

        string FromState { get; set; }

        string FromZip5 { get; set; }

        string FromZip4 { get; set; }

        string FromPhone { get; set; }

        string POZipCode { get; set; }

        bool AllowNonCleansedOriginAddr { get; set; }

        string ToName { get; set; }

        string ToFirm { get; set; }

        string ToAptOrSuiteorOther { get; set; }

        string ToStreetAddress { get; set; }

        string ToCity { get; set; }

        string ToState { get; set; }

        string ToZip5 { get; set; }

        string ToZip4 { get; set; }

        string ToPhone { get; set; }

        bool POBox { get; set; }

        ToContactPreference? ToContactPreference { get; set; }

        string ToContactMessaging { get; set; }

        string ToContactEmail { get; set; }

        bool AllowNonCleansedDestAddr { get; set; }

        string WeightInOunces { get; set; }

        ServiceType? ServiceType { get; set; }

        Container? Container { get; set; }

        decimal? Width { get; set; }

        decimal? Length { get; set; }

        decimal? Height { get; set; }

        Machinable? Machinable { get; set; }

        PriceOption? PriceOptions { get; set; }

        decimal? InsuredAmount { get; set; }

        bool AddressServiceRequested { get; set; }

        ExpressMailOptions ExpressMailOptions { get; set; }

        string CustomerRefNo { get; set; }

        string CustomerRefNo2 { get; set; }

        List<ExtraServiceType> ExtraServices { get; set; }

        string CRID { get; set; }

        string MID { get; set; }

        string LogisticsManagerMID { get; set; }

        string VendorCode { get; set; }

        string VendorProductVersionNumber { get; set; }

        string SenderName { get; set; }

        string SenderEMail { get; set; }

        string RecipientName { get; set; }

        string RecipientEMail { get; set; }

        ReceiptOption? ReceiptOption { get; set; }

        ImageType? ImageType { get; set; }

        HoldForManifest? HoldForManifest { get; set; }

        bool NineDigitRoutingZip { get; set; }

        bool ShipInfo { get; set; }

        bool CarrierRelease { get; set; }

        bool ReturnCommitments { get; set; }

        bool PrintCustomerRefNo { get; set; }

        bool PrintCustomerRefNo2 { get; set; }

        Content Content { get; set; }

        ActionCode? ActionCode { get; set; }

        bool OptOutOfSPE { get; set; }

        string SortationLevel { get; set; }

        DestinationEntryFacilityType? DestinationEntryFacilityType { get; set; }
    }
}
