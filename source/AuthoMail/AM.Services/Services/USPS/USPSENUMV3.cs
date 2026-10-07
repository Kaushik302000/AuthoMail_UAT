using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.V31
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LabelSize
    {
        [EnumMember(Value = "BARCODE ONLY")]
        [JsonProperty("BARCODE ONLY")]
        BARCODE_ONLY,

        [EnumMember(Value = "CROP")]
        [JsonProperty("CROP")]
        CROP,

        [EnumMember(Value = "4X6LABEL")]
        [JsonProperty("4X6LABEL")]
        LABEL4X6,

        [EnumMember(Value = "4X6LABELL")]
        [JsonProperty("4X6LABELL")]
        LABEL4X6L,

        [EnumMember(Value = "6X4LABEL")]
        [JsonProperty("6X4LABEL")]
        LABEL6X4,

        [EnumMember(Value = "4X6LABELP")]
        [JsonProperty("4X6LABELP")]
        LABEL4X6P,

        [EnumMember(Value = "4X6LABELP PAGE")]
        [JsonProperty("4X6LABELP PAGE")]
        LABEL4X6P_PAGE,

        [EnumMember(Value = "SEPARATECONTINUEPAGE")]
        [JsonProperty("SEPARATECONTINUEPAGE")]
        SEPARATECONTINUEPAGE,

        [EnumMember(Value = "4X6LABELZPL")]
        [JsonProperty("4X6LABELZPL")]
        LABEL4X6ZPL,

        [EnumMember(Value = "4X5LABEL")]
        [JsonProperty("4X5LABEL")]
        LABEL4X5,

        [EnumMember(Value = "3X5FMB")]
        [JsonProperty("3X5FMB")]
        FMB3X5
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReceiptOption
    {
        [EnumMember(Value = "NONE")]
        [JsonProperty("NONE")]
        None,

        [EnumMember(Value = "SAME_PAGE")]
        [JsonProperty("SAME_PAGE")]
        SAME_PAGE,

        [EnumMember(Value = "SEPARATE_PAGE")]
        [JsonProperty("SEPARATE_PAGE")]
        SEPARATE_PAGE
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceType
    {
        [EnumMember(Value = "PRIORITY EXPRESS")]
        [JsonProperty("PRIORITY EXPRESS")]
        PRIORITY_EXPRESS,

        [EnumMember(Value = "PRIORITY")]
        [JsonProperty("PRIORITY")]
        PRIORITY,

        [EnumMember(Value = "FIRST CLASS")]
        [JsonProperty("FIRST CLASS")]
        FIRST_CLASS,

        [EnumMember(Value = "PARCEL SELECT GROUND")]
        [JsonProperty("PARCEL SELECT GROUND")]
        PARCEL_SELECT_GROUND,

        [EnumMember(Value = "LIBRARY")]
        [JsonProperty("LIBRARY")]
        LIBRARY,

        [EnumMember(Value = "MEDIA")]
        [JsonProperty("MEDIA")]
        MEDIA,

        [EnumMember(Value = "BPM")]
        [JsonProperty("BPM")]
        BPM,

        [EnumMember(Value = "PRIORITY MAIL CUBIC")]
        [JsonProperty("PRIORITY MAIL CUBIC")]
        PRIORITY_MAIL_CUBIC,


        [EnumMember(Value = "PARCEL_SELECT")]
        [JsonProperty("PARCEL_SELECT")]
        PARCEL_SELECT,

        [EnumMember(Value = "PARCEL_SELECT_LIGHTWEIGHT")]
        [JsonProperty("PARCEL_SELECT_LIGHTWEIGHT")]
        PARCEL_SELECT_LIGHTWEIGHT,

        [EnumMember(Value = "USPS_CONNECT_LOCAL")]
        [JsonProperty("USPS_CONNECT_LOCAL")]
        USPS_CONNECT_LOCAL,

        [EnumMember(Value = "USPS_CONNECT_REGIONAL")]
        [JsonProperty("USPS_CONNECT_REGIONAL")]
        USPS_CONNECT_REGIONAL,

        [EnumMember(Value = "USPS_CONNECT_MAIL")]
        [JsonProperty("USPS_CONNECT_MAIL")]
        USPS_CONNECT_MAIL,

        [EnumMember(Value = "USPS_GROUND_ADVANTAGE")]
        [JsonProperty("USPS_GROUND_ADVANTAGE")]
        USPS_GROUND_ADVANTAGE,

        [EnumMember(Value = "PRIORITY_MAIL_EXPRESS")]
        [JsonProperty("PRIORITY_MAIL_EXPRESS")]
        PRIORITY_MAIL_EXPRESS,

        [EnumMember(Value = "PRIORITY_MAIL")]
        [JsonProperty("PRIORITY_MAIL")]
        PRIORITY_MAIL,

        [EnumMember(Value = "FIRST-CLASS_PACKAGE_SERVICE")]
        [JsonProperty("FIRST-CLASS_PACKAGE_SERVICE")]
        FIRST_CLASS_PACKAGE_SERVICE,

        [EnumMember(Value = "LIBRARY_MAIL")]
        [JsonProperty("LIBRARY_MAIL")]
        LIBRARY_MAIL,

        [EnumMember(Value = "MEDIA_MAIL")]
        [JsonProperty("MEDIA_MAIL")]
        MEDIA_MAIL,

        [EnumMember(Value = "BOUND_PRINTED_MATTER")]
        [JsonProperty("BOUND_PRINTED_MATTER")]
        BOUND_PRINTED_MATTER,

        [EnumMember(Value = "DOMESTIC_MATTER_FOR_THE_BLIND")]
        [JsonProperty("DOMESTIC_MATTER_FOR_THE_BLIND")]
        DOMESTIC_MATTER_FOR_THE_BLIND,

        [EnumMember(Value = "FIRST-CLASS_PACKAGE_RETURN_SERVICE")]
        [JsonProperty("FIRST-CLASS_PACKAGE_RETURN_SERVICE")]
        FIRST_CLASS_PACKAGE_RETURN_SERVICE,

        [EnumMember(Value = "GROUND_RETURN_SERVICE")]
        [JsonProperty("GROUND_RETURN_SERVICE")]
        GROUND_RETURN_SERVICE,

        [EnumMember(Value = "PRIORITY_MAIL_EXPRESS_RETURN_SERVICE")]
        [JsonProperty("PRIORITY_MAIL_EXPRESS_RETURN_SERVICE")]
        PRIORITY_MAIL_EXPRESS_RETURN_SERVICE,

        [EnumMember(Value = "PRIORITY_MAIL_RETURN_SERVICE")]
        [JsonProperty("PRIORITY_MAIL_RETURN_SERVICE")]
        PRIORITY_MAIL_RETURN_SERVICE,

        [EnumMember(Value = "USPS_GROUND_ADVANTAGE_RETURN_SERVICE")]
        [JsonProperty("USPS_GROUND_ADVANTAGE_RETURN_SERVICE")]
        USPS_GROUND_ADVANTAGE_RETURN_SERVICE
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RateIndicator
    {
        [EnumMember(Value = "3D")]
        [JsonProperty("3D")]
        Three_Digit,

        [EnumMember(Value = "3N")]
        [JsonProperty("3N")]
        Three_Digit_Dimensional_Rectangular,

        [EnumMember(Value = "3R")]
        [JsonProperty("3R")]
        Three_Digit_Dimensional_Nonrectangular,

        [EnumMember(Value = "5D")]
        [JsonProperty("5D")]
        Five_Digit,

        [EnumMember(Value = "BA")]
        [JsonProperty("BA")]
        Basic,

        [EnumMember(Value = "BB")]
        [JsonProperty("BB")]
        Mixed_NDC,

        [EnumMember(Value = "C1")]
        [JsonProperty("C1")]
        Cubic_Pricing_Tier_1,

        [EnumMember(Value = "C2")]
        [JsonProperty("C2")]
        Cubic_Pricing_Tier_2,

        [EnumMember(Value = "C3")]
        [JsonProperty("C3")]
        Cubic_Pricing_Tier_3,

        [EnumMember(Value = "C4")]
        [JsonProperty("C4")]
        Cubic_Pricing_Tier_4,

        [EnumMember(Value = "C5")]
        [JsonProperty("C5")]
        Cubic_Pricing_Tier_5,

        [EnumMember(Value = "CP")]
        [JsonProperty("CP")]
        Cubic_Parcel,

        [EnumMember(Value = "CM")]
        [JsonProperty("CM")]
        Connect_Local_Mail,

        [EnumMember(Value = "DC")]
        [JsonProperty("DC")]
        NDC,

        [EnumMember(Value = "DE")]
        [JsonProperty("DE")]
        SCF,

        [EnumMember(Value = "DF")]
        [JsonProperty("DF")]
        Five_Digit_DF,

        [EnumMember(Value = "DN")]
        [JsonProperty("DN")]
        Dimensional_Nonrectangular,

        [EnumMember(Value = "DR")]
        [JsonProperty("DR")]
        Dimensional_Rectangular,

        [EnumMember(Value = "E4")]
        [JsonProperty("E4")]
        Priority_Mail_Express_Flat_Rate_Envelope_Post_Office_To_Addressee,

        [EnumMember(Value = "E6")]
        [JsonProperty("E6")]
        Priority_Mail_Express_Legal_Flat_Rate_Envelope,

        [EnumMember(Value = "E7")]
        [JsonProperty("E7")]
        Priority_Mail_Express_Legal_Flat_Rate_Envelope_Sunday_Holiday,

        [EnumMember(Value = "FA")]
        [JsonProperty("FA")]
        Legal_Flat_Rate_Envelope,

        [EnumMember(Value = "FB")]
        [JsonProperty("FB")]
        Medium_Flat_Rate_Box_Large_Flat_Rate_Bag,

        [EnumMember(Value = "FE")]
        [JsonProperty("FE")]
        Flat_Rate_Envelope,

        [EnumMember(Value = "FP")]
        [JsonProperty("FP")]
        Padded_Flat_Rate_Envelope,

        [EnumMember(Value = "FS")]
        [JsonProperty("FS")]
        Small_Flat_Rate_Box,

        [EnumMember(Value = "LC")]
        [JsonProperty("LC")]
        Connect_Local_Single_Piece,

        [EnumMember(Value = "LF")]
        [JsonProperty("LF")]
        Flat_Rate_Box,

        [EnumMember(Value = "LL")]
        [JsonProperty("LL")]
        Large_Flat_Rate_Bag,

        [EnumMember(Value = "LO")]
        [JsonProperty("LO")]
        Connect_Local_Oversized,

        [EnumMember(Value = "LS")]
        [JsonProperty("LS")]
        Small_Flat_Rate_Bag,

        [EnumMember(Value = "NP")]
        [JsonProperty("NP")]
        Non_Presorted,

        [EnumMember(Value = "OS")]
        [JsonProperty("OS")]
        Oversized,

        [EnumMember(Value = "P5")]
        [JsonProperty("P5")]
        Cubic_Soft_Pack_Tier_1,

        [EnumMember(Value = "P6")]
        [JsonProperty("P6")]
        Cubic_Soft_Pack_Tier_2,

        [EnumMember(Value = "P7")]
        [JsonProperty("P7")]
        Cubic_Soft_Pack_Tier_3,

        [EnumMember(Value = "P8")]
        [JsonProperty("P8")]
        Cubic_Soft_Pack_Tier_4,

        [EnumMember(Value = "P9")]
        [JsonProperty("P9")]
        Cubic_Soft_Pack_Tier_5,

        [EnumMember(Value = "Q6")]
        [JsonProperty("Q6")]
        Cubic_Soft_Pack_Tier_6,

        [EnumMember(Value = "Q7")]
        [JsonProperty("Q7")]
        Cubic_Soft_Pack_Tier_7,

        [EnumMember(Value = "Q8")]
        [JsonProperty("Q8")]
        Cubic_Soft_Pack_Tier_8,

        [EnumMember(Value = "Q9")]
        [JsonProperty("Q9")]
        Cubic_Soft_Pack_Tier_9,

        [EnumMember(Value = "Q0")]
        [JsonProperty("Q0")]
        Cubic_Soft_Pack_Tier_10,

        [EnumMember(Value = "PA")]
        [JsonProperty("PA")]
        Priority_Mail_Express_Single_Piece,

        [EnumMember(Value = "PL")]
        [JsonProperty("PL")]
        Large_Flat_Rate_Box,

        [EnumMember(Value = "PM")]
        [JsonProperty("PM")]
        Large_Flat_Rate_Box_APO_FPO_DPO,

        [EnumMember(Value = "PR")]
        [JsonProperty("PR")]
        Presorted,

        [EnumMember(Value = "SN")]
        [JsonProperty("SN")]
        SCF_Dimensional_Nonrectangular,

        [EnumMember(Value = "SP")]
        [JsonProperty("SP")]
        Single_Piece,

        [EnumMember(Value = "SR")]
        [JsonProperty("SR")]
        SCF_Dimensional_Rectangular
    }

    public enum DeliveryOption
    {
        [EnumMember(Value = "1")]
        [JsonProperty("1")]
        NormalDelivery = 1,

        [EnumMember(Value = "2")]
        [JsonProperty("2")]
        NoWeekendDelivery = 2,

        [EnumMember(Value = "3")]
        [JsonProperty("3")]
        NoHolidayDelivery = 3,

        [EnumMember(Value = "4")]
        [JsonProperty("4")]
        NoWeekendorHolidayDelivery = 4,

        [EnumMember(Value = "5")]
        [JsonProperty("5")]
        NoSaturdayDelivery = 5,

        [EnumMember(Value = "6")]
        [JsonProperty("6")]
        NoSundayDelivery = 6,

        [EnumMember(Value = "7")]
        [JsonProperty("7")]
        SundayDelivery = 7,

        [EnumMember(Value = "8")]
        [JsonProperty("8")]
        HolidayDelivery = 8,

        [EnumMember(Value = "9")]
        [JsonProperty("9")]
        NoSundayorHolidayDelivery = 9,

        [EnumMember(Value = "E")]
        [JsonProperty("E")]
        ScheduledEndofDay,

        [EnumMember(Value = "F")]
        [JsonProperty("F")]
        ScheduledEndofDayNoSun_HolDelivery,

        [EnumMember(Value = "G")]
        [JsonProperty("G")]
        ScheduledEndofDayNoWeekend_HolDelivery
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageType
    {
        [EnumMember(Value = "PDF")]
        [JsonProperty("PDF")]
        PDF,

        [EnumMember(Value = "TIF")]
        [JsonProperty("TIF")]
        TIF,

        [EnumMember(Value = "NONE")]
        [JsonProperty("NONE")]
        None,

        [EnumMember(Value = "JPG")]
        [JsonProperty("JPG")]
        JPG,

        [EnumMember(Value = "SVG")]
        [JsonProperty("SVG")]
        SVG,

        [EnumMember(Value = "ZPL203DPI")]
        [JsonProperty("ZPL203DPI")]
        ZPL203DPI,

        [EnumMember(Value = "ZPL300DPI")]
        [JsonProperty("ZPL300DPI")]
        ZPL300DPI,

        [EnumMember(Value = "LABEL_BROKER")]
        [JsonProperty("LABEL_BROKER")]
        LabelBroker
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Machinable
    {
        [EnumMember(Value = "True")]
        [JsonProperty("True")]
        True,

        [EnumMember(Value = "False")]
        [JsonProperty("False")]
        False,

        [EnumMember(Value = "LETTERS")]
        [JsonProperty("LETTERS")]
        LETTERS,

        [EnumMember(Value = "FLATS")]
        [JsonProperty("FLATS")]
        FLATS,

        [EnumMember(Value = "MACHINABLE")]
        [JsonProperty("MACHINABLE")]
        MACHINABLE,

        [EnumMember(Value = "IRREGULAR")]
        [JsonProperty("IRREGULAR")]
        IRREGULAR,

        [EnumMember(Value = "NON_MACHINABLE")]
        [JsonProperty("NON_MACHINABLE")]
        NON_MACHINABLE
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum HoldForManifest
    {
        [EnumMember(Value = "Y")]
        [JsonProperty("Y")]
        Y,

        [EnumMember(Value = "N")]
        [JsonProperty("N")]
        N
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WeightUOM
    {
        [EnumMember(Value = "lb")]
        [JsonProperty("lb")]
        Lb
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DimensionUOM
    {
        [EnumMember(Value = "in")]
        [JsonProperty("in")]
        In
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContainerType
    {
        [EnumMember(Value = "VARIABLE")]
        [JsonProperty("VARIABLE")]
        VARIABLE,

        [EnumMember(Value = "FLAT RATE ENVELOPE")]
        [JsonProperty("FLAT RATE ENVELOPE")]
        FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "LEGAL FLAT RATE ENVELOPE")]
        [JsonProperty("LEGAL FLAT RATE ENVELOPE")]
        LEGAL_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "PADDED FLAT RATE ENVELOPE")]
        [JsonProperty("PADDED FLAT RATE ENVELOPE")]
        PADDED_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "GIFT CARD FLAT RATE ENVELOPE")]
        [JsonProperty("GIFT CARD FLAT RATE ENVELOPE")]
        GIFT_CARD_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "SM FLAT RATE ENVELOPE")]
        [JsonProperty("SM FLAT RATE ENVELOPE")]
        SM_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "WINDOW FLAT RATE ENVELOPE")]
        [JsonProperty("WINDOW FLAT RATE ENVELOPE")]
        WINDOW_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "SM FLAT RATE BOX")]
        [JsonProperty("SM FLAT RATE BOX")]
        SM_FLAT_RATE_BOX,

        [EnumMember(Value = "MD FLAT RATE BOX")]
        [JsonProperty("MD FLAT RATE BOX")]
        MD_FLAT_RATE_BOX,

        [EnumMember(Value = "LG FLAT RATE BOX")]
        [JsonProperty("LG FLAT RATE BOX")]
        LG_FLAT_RATE_BOX,

        [EnumMember(Value = "REGIONALRATEBOXA")]
        [JsonProperty("REGIONALRATEBOXA")]
        REGIONALRATEBOXA,

        [EnumMember(Value = "REGIONALRATEBOXB")]
        [JsonProperty("REGIONALRATEBOXB")]
        REGIONALRATEBOXB,

        [EnumMember(Value = "RECTANGULAR")]
        [JsonProperty("RECTANGULAR")]
        RECTANGULAR,

        [EnumMember(Value = "NONRECTANGULAR")]
        [JsonProperty("NONRECTANGULAR")]
        NONRECTANGULAR,

        [EnumMember(Value = "PACKAGE SERVICE")]
        [JsonProperty("PACKAGE SERVICE")]
        PACKAGE_SERVICE,

        [EnumMember(Value = "CUBIC PARCELS")]
        [JsonProperty("CUBIC PARCELS")]
        CUBIC_PARCELS,

        [EnumMember(Value = "CUBIC SOFT PACK")]
        [JsonProperty("CUBIC SOFT PACK")]
        CUBIC_SOFT_PACK
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum PriceOption
    {
        [EnumMember(Value = "Retail")]
        [JsonProperty("Retail")]
        Retail,

        [EnumMember(Value = "Commercial Plus")]
        [JsonProperty("Commercial Plus")]
        CommercialPlus,

        [EnumMember(Value = "Commercial Base")]
        [JsonProperty("Commercial Base")]
        CommercialBase
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ToContactPreference
    {
        [EnumMember(Value = "EMAIL")]
        [JsonProperty("EMAIL")]
        EMAIL,

        [EnumMember(Value = "EMAILSMS")]
        [JsonProperty("EMAILSMS")]
        EMAILSMS,

        [EnumMember(Value = "SMS")]
        [JsonProperty("SMS")]
        SMS,

        [EnumMember(Value = "WAIVED")]
        [JsonProperty("WAIVED")]
        WAIVED
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContentType
    {
        [EnumMember(Value = "HAZMAT")]
        [JsonProperty("HAZMAT")]
        Hazmat,

        [EnumMember(Value = "CREMATED_REMAINS")]
        [JsonProperty("CREMATED_REMAINS")]
        Cremated_Remains,

        [EnumMember(Value = "BEES")]
        [JsonProperty("BEES")]
        Bees,

        [EnumMember(Value = "DAY_OLD_POULTRY")]
        [JsonProperty("DAY_OLD_POULTRY")]
        Day_Old_Poultry,

        [EnumMember(Value = "ADULT_BIRDS")]
        [JsonProperty("ADULT_BIRDS")]
        Adult_Birds,

        [EnumMember(Value = "Lives")]
        [JsonProperty("Lives")]
        Lives,

        [EnumMember(Value = "OTHER_LIVES")]
        [JsonProperty("OTHER_LIVES")]
        Other_Lives,

        [EnumMember(Value = "PERISHABLE")]
        [JsonProperty("PERISHABLE")]
        Perishable,

        [EnumMember(Value = "PHARMACEUTICALS")]
        [JsonProperty("PHARMACEUTICALS")]
        Pharmaceuticals,

        [EnumMember(Value = "MEDICAL_SUPPLIES")]
        [JsonProperty("MEDICAL_SUPPLIES")]
        Medical_Supplies
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DestinationEntryFacilityType
    {
        [EnumMember(Value = "NONE")]
        [JsonProperty("NONE")]
        None,

        [EnumMember(Value = "DESTINATION_NETWORK_DISTRIBUTION_CENTER")]
        [JsonProperty("DESTINATION_NETWORK_DISTRIBUTION_CENTER")]
        DESTINATION_NETWORK_DISTRIBUTION_CENTER,

        [EnumMember(Value = "DESTINATION_SECTIONAL_CENTER_FACILITY")]
        [JsonProperty("DESTINATION_SECTIONAL_CENTER_FACILITY")]
        DESTINATION_SECTIONAL_CENTER_FACILITY,

        [EnumMember(Value = "DESTINATION_DELIVERY_UNIT")]
        [JsonProperty("DESTINATION_DELIVERY_UNIT")]
        DESTINATION_DELIVERY_UNIT,

        [EnumMember(Value = "DESTINATION_SERVICE_HUB")]
        [JsonProperty("DESTINATION_SERVICE_HUB")]
        DESTINATION_SERVICE_HUB
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionCode
    {
        [EnumMember(Value = "M0")]
        [JsonProperty("M0")]

        MailerOwner,

        [EnumMember(Value = "S0")]
        [JsonProperty("S0")]

        ServiceProvider
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContentDescription
    {
        [EnumMember(Value = "Bees")]
        [JsonProperty("Bees")]
        Bees,

        [EnumMember(Value = "DayOldPoultry")]
        [JsonProperty("DayOldPoultry")]
        DayOldPoultry,

        [EnumMember(Value = "AdultBirds")]
        [JsonProperty("AdultBirds")]
        AdultBirds,

        [EnumMember(Value = "Other")]
        [JsonProperty("Other")]
        Other
    }

    public enum AncillaryServiceEndorsements
    {
        [EnumMember(Value = "CHANGE_SERVICE_REQUESTED")]
        [JsonProperty("CHANGE_SERVICE_REQUESTED")]
        Change_Service_Requested,

        [EnumMember(Value = "ADDRESS_SERVICE_REQUESTED")]
        [JsonProperty("ADDRESS_SERVICE_REQUESTED")]
        Address_Service_Requested,

        [EnumMember(Value = "ELECTRONIC_SERVICE_REQUESTED")]
        [JsonProperty("ELECTRONIC_SERVICE_REQUESTED")]
        Electronic_Service_Requested,

        [EnumMember(Value = "RETURN_SERVICE_REQUESTED")]
        [JsonProperty("RETURN_SERVICE_REQUESTED")]
        Return_Service_Requested,

        [EnumMember(Value = "TEMP_RETURN_SERVICE_REQUESTED")]
        [JsonProperty("TEMP_RETURN_SERVICE_REQUESTED")]
        Temporary_Return_Service_Requested,

        [EnumMember(Value = "FORWARDING_SERVICE_REQUESTED")]
        [JsonProperty("FORWARDING_SERVICE_REQUESTED")]
        Forwarding_Service_Requested
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortType
    {
        [EnumMember(Value = "TRUCK_BEDLOAD")]
        [JsonProperty("TRUCK_BEDLOAD")]
        Truck_Bedload,

        [EnumMember(Value = "OPEN_DISTRIBUTION_FULL_POSTAL_PAKS")]
        [JsonProperty("OPEN_DISTRIBUTION_FULL_POSTAL_PAKS")]
        Open_Distribution_Full_Postal_Paks,

        [EnumMember(Value = "OPEN_DISTRIBUTE_EMM_TRAY_BOX")]
        [JsonProperty("OPEN_DISTRIBUTE_EMM_TRAY_BOX")]
        Open_Distribute_EMM_Tray_Box,

        [EnumMember(Value = "OPEN_DISTRIBUTE_FULL_TRAY_BOX")]
        [JsonProperty("OPEN_DISTRIBUTE_FULL_TRAY_BOX")]
        Open_Distribute_Full_Tray_Box,

        [EnumMember(Value = "OPEN_DISTRIBUTE_HALF_TRAY_BOX")]
        [JsonProperty("OPEN_DISTRIBUTE_HALF_TRAY_BOX")]
        Open_Distribute_Half_TrayBox,

        [EnumMember(Value = "OPEN_DISTRIBUTE_HALF_POSTAL_PAKS")]
        [JsonProperty("OPEN_DISTRIBUTE_HALF_POSTAL_PAKS")]
        Open_Distribute_Half_Postal_Paks,

        [EnumMember(Value = "OPEN_DISTRIBUTE_PALLET")]
        [JsonProperty("OPEN_DISTRIBUTE_PALLET")]
        Open_Distribute_Pallet,

        [EnumMember(Value = "OPEN_DISTRIBUTE_FLAT_TUB_TRAY_BOX")]
        [JsonProperty("OPEN_DISTRIBUTE_FLAT_TUB_TRAY_BOX")]
        Open_Distribute_Flat_Tub_TrayBox,

        [EnumMember(Value = "PALLET")]
        [JsonProperty("PALLET")]
        Pallet,

        [EnumMember(Value = "RECEPTACLE")]
        [JsonProperty("RECEPTACLE")]
        Receptacle,

        [EnumMember(Value = "SACK")]
        [JsonProperty("SACK")]
        Sack
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RestrictionType
    {
        [EnumMember(Value = "QUARANTINE")]
        [JsonProperty("QUARANTINE")]
        Quarantine,

        [EnumMember(Value = "SANITARY_INSPECTION")]
        [JsonProperty("SANITARY_INSPECTION")]
        Sanitary_Inspection,

        [EnumMember(Value = "PHYTOSANITARY_INSPECTION")]
        [JsonProperty("PHYTOSANITARY_INSPECTION")]
        Phytosanitary_Inspection,

        [EnumMember(Value = "OTHER")]
        [JsonProperty("OTHER")]
        Other
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomsContentType
    {
        [EnumMember(Value = "MERCHANDISE")]
        [JsonProperty("MERCHANDISE")]
        MERCHANDISE,

        [EnumMember(Value = "GIFT")]
        [JsonProperty("GIFT")]
        GIFT,

        [EnumMember(Value = "DOCUMENT")]
        [JsonProperty("DOCUMENT")]
        DOCUMENT,

        [EnumMember(Value = "COMMERCIAL_SAMPLE")]
        [JsonProperty("COMMERCIAL_SAMPLE")]
        COMMERCIAL_SAMPLE,

        [EnumMember(Value = "RETURN")]
        [JsonProperty("RETURN")]
        RETURN,

        [EnumMember(Value = "RETURNED_GOODS")]
        [JsonProperty("RETURNED_GOODS")]
        RETURNED_GOODS,

        [EnumMember(Value = "OTHER")]
        [JsonProperty("OTHER")]
        OTHER,

        [EnumMember(Value = "HUMANITARIAN_DONATIONS")]
        [JsonProperty("HUMANITARIAN_DONATIONS")]
        HUMANITARIAN_DONATIONS,

        [EnumMember(Value = "DANGEROUS_GOODS")]
        [JsonProperty("DANGEROUS_GOODS")]
        DANGEROUS_GOODS
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NonDeliveryOption
    {
        [EnumMember(Value = "RETURN")]
        [JsonProperty("RETURN")]
        RETURN,

        [EnumMember(Value = "REDIRECT")]
        [JsonProperty("REDIRECT")]
        REDIRECT,

        [EnumMember(Value = "ABANDON")]
        [JsonProperty("ABANDON")]
        ABANDON
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ePostageMailerReporting
    {
        [EnumMember(Value = "1")]
        [JsonProperty("1")]

        ePostage_sender_information_in_SSF = 1,

        [EnumMember(Value = "2")]
        [JsonProperty("2")]

        ePostage_sender_information_through_DES = 2,


        [EnumMember(Value = "3")]
        [JsonProperty("3")]

        ePostage_sender_information_using_child_MID = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum eVSCancelStatus
    {
        [EnumMember(Value = "Cancelled")]
        [JsonProperty("Cancelled")]

        Cancelled,

        [EnumMember(Value = "Not Cancelled")]
        [JsonProperty("Not Cancelled")]

        Not_Cancelled
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum eVSCancelReason
    {
        [EnumMember(Value = "Order Cancelled Successfully")]
        [JsonProperty("Order Cancelled Successfully")]
        Order_Cancelled_Successfully,

        [EnumMember(Value = "Order Already Cancelled")]
        [JsonProperty("Order Already Cancelled")]
        Order_Already_Cancelled,

        [EnumMember(Value = "Order Not Found")]
        [JsonProperty("Order Not Found")]
        Order_Not_Found
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AddressType
    {
        [EnumMember(Value = "INVALID")]
        [JsonProperty("INVALID")]
        INVALID,

        [EnumMember(Value = "FOREIGN")]
        [JsonProperty("FOREIGN")]
        FOREIGN,

        [EnumMember(Value = "POBOX")]
        [JsonProperty("POBOX")]
        POBOX,

        [EnumMember(Value = "Military")]
        [JsonProperty("Military")]
        Military,

        [EnumMember(Value = "Territory")]
        [JsonProperty("Territory")]
        Territory,

        //DC,Hawaii,Alaska and All Mainlands
        [EnumMember(Value = "DHAM")]
        [JsonProperty("DHAM")]
        DHAM
    }

    //[JsonConverter(typeof(StringEnumConverter))]
    public enum ExtraServiceType
    {
        [EnumMember(Value = "365")]
        [JsonProperty("365")]
        Global_Direct_Entry = 365,

        [EnumMember(Value = "415")]
        [JsonProperty("415")]
        USPS_Label_Delivery_Service = 415,

        [EnumMember(Value = "480")]
        [JsonProperty("480")]
        Tracking_Plus_6Months = 480,

        [EnumMember(Value = "481")]
        [JsonProperty("481")]
        Tracking_Plus1_Year = 481,

        [EnumMember(Value = "482")]
        [JsonProperty("482")]
        Tracking_Plus3_Years = 482,

        [EnumMember(Value = "483")]
        [JsonProperty("483")]
        Tracking_Plus_5Years = 483,

        [EnumMember(Value = "484")]
        [JsonProperty("484")]
        Tracking_Plus_7Years = 484,

        [EnumMember(Value = "485")]
        [JsonProperty("485")]
        Tracking_Plus_10Years = 485,

        [EnumMember(Value = "486")]
        [JsonProperty("486")]
        Tracking_Plus_Signature_3Years = 486,

        [EnumMember(Value = "487")]
        [JsonProperty("487")]
        Tracking_Plus_Signature_5Years = 487,

        [EnumMember(Value = "488")]
        [JsonProperty("488")]
        Tracking_Plus_Signature_7Years = 488,

        [EnumMember(Value = "489")]
        [JsonProperty("489")]
        Tracking_Plus_Signature_10Years = 489,

        [EnumMember(Value = "810")]
        [JsonProperty("810")]
        Hazardous_Materials_Air_Eligible_Ethanol = 810,

        [EnumMember(Value = "811")]
        [JsonProperty("811")]
        Hazardous_Materials_Class1_Toy_Propellant_Safety_Fuse_Package = 811,

        [EnumMember(Value = "812")]
        [JsonProperty("812")]
        Hazardous_Materials_Class3_Flammable_And_Combustible_Liquids = 812,

        [EnumMember(Value = "813")]
        [JsonProperty("813")]
        Hazardous_Materials_Class7_Radioactive_Materials = 813,

        [EnumMember(Value = "814")]
        [JsonProperty("814")]
        Hazardous_Materials_Class8_Air_Eligible_Corrosive_Materials = 814,

        [EnumMember(Value = "815")]
        [JsonProperty("815")]
        Hazardous_Materials_Class8_Nonspillable_Wet_Batteries = 815,

        [EnumMember(Value = "816")]
        [JsonProperty("816")]
        Hazardous_Materials_Class9_Lithium_Battery_Marked_Ground_Only = 816,

        [EnumMember(Value = "817")]
        [JsonProperty("817")]
        Hazardous_Materials_Class9_Lithium_Battery_Returns = 817,

        [EnumMember(Value = "818")]
        [JsonProperty("818")]
        Hazardous_Materials_Class9_Marked_Lithium_Batteries = 818,

        [EnumMember(Value = "819")]
        [JsonProperty("819")]
        Hazardous_Materials_Class9_DryIce = 819,

        [EnumMember(Value = "820")]
        [JsonProperty("820")]
        Hazardous_Materials_Class9_Unmarked_Lithium_Batteries = 820,

        [EnumMember(Value = "821")]
        [JsonProperty("821")]
        Hazardous_Materials_Class9_Magnetized_Materials = 821,

        [EnumMember(Value = "822")]
        [JsonProperty("822")]
        Hazardous_Materials_Division41_Mailable_Flammable_Solids_And_Safety_Matches = 822,

        [EnumMember(Value = "823")]
        [JsonProperty("823")]
        Hazardous_Materials_Division51_Oxidizers = 823,

        [EnumMember(Value = "824")]
        [JsonProperty("824")]
        Hazardous_Materials_Division52_Organic_Peroxides = 824,

        [EnumMember(Value = "825")]
        [JsonProperty("825")]
        Hazardous_Materials_Division61_Toxic_Materials = 825,

        [EnumMember(Value = "826")]
        [JsonProperty("826")]
        Hazardous_Materials_Division62_Biological_Materials = 826,

        [EnumMember(Value = "827")]
        [JsonProperty("827")]
        Hazardous_Materials_Excepted_Quantity_Provision = 827,

        [EnumMember(Value = "828")]
        [JsonProperty("828")]
        Hazardous_Materials_Ground_Only_Hazardous_Materials = 828,

        [EnumMember(Value = "829")]
        [JsonProperty("829")]
        Hazardous_Materials_Air_Eligible_ID8000_Consumer_Commodity = 829,

        [EnumMember(Value = "830")]
        [JsonProperty("830")]
        Hazardous_Materials_Lighters = 830,

        [EnumMember(Value = "831")]
        [JsonProperty("831")]
        HazardousMaterials_Limited_Quantity_Ground = 831,

        [EnumMember(Value = "832")]
        [JsonProperty("832")]
        HazardousMaterials_Small_Quantity_Provision_Markings_Required = 832,

        [EnumMember(Value = "857")]
        [JsonProperty("857")]
        Hazardous_Materials = 857,

        [EnumMember(Value = "910")]
        [JsonProperty("910")]
        Certified_Mail = 910,

        [EnumMember(Value = "911")]
        [JsonProperty("911")]
        Certified_Mail_Restricted_Delivery = 911,

        [EnumMember(Value = "912")]
        [JsonProperty("912")]
        Certified_Mail_Adult_Signature_Required = 912,

        [EnumMember(Value = "913")]
        [JsonProperty("913")]
        Certified_Mail_Adult_Signature_Restricted_Delivery = 913,

        [EnumMember(Value = "920")]
        [JsonProperty("920")]
        USPS_Tracking_Electronic = 920,

        [EnumMember(Value = "921")]
        [JsonProperty("921")]
        Signature_Confirmation = 921,

        [EnumMember(Value = "922")]
        [JsonProperty("922")]
        Adult_Signature_Required = 922,

        [EnumMember(Value = "923")]
        [JsonProperty("923")]
        Adult_Signature_Restricted_Delivery = 923,

        [EnumMember(Value = "924")]
        [JsonProperty("924")]
        Signature_Confirmation_Restricted_Delivery = 924,

        [EnumMember(Value = "925")]
        [JsonProperty("925")]
        Priority_Mail_Express_Insurance = 925,

        [EnumMember(Value = "930")]
        [JsonProperty("930")]
        Insurance = 930,

        [EnumMember(Value = "934")]
        [JsonProperty("934")]
        Insurance_Restricted_Delivery = 934,

        [EnumMember(Value = "955")]
        [JsonProperty("955")]
        Return_Receipt = 955,

        [EnumMember(Value = "957")]
        [JsonProperty("957")]
        Return_Receipt_Electronic = 957,

        [EnumMember(Value = "981")]
        [JsonProperty("981")]
        Signature_Requested_Priority_Mail_Express_Only = 981,

        [EnumMember(Value = "986")]
        [JsonProperty("986")]
        PO_To_Addressee_Priority_Mail_Express_Only = 986,

        [EnumMember(Value = "991")]
        [JsonProperty("991")]
        Sunday_Delivery = 991
    }
}
