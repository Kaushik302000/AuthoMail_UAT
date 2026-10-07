using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LabelSize
    {
        [XmlEnum( "BARCODE ONLY")]
        [JsonProperty("BARCODE ONLY")]
        BARCODE_ONLY,

        [XmlEnum( "CROP")]
        [JsonProperty("CROP")]
        CROP,

        [XmlEnum( "4X6LABEL")]
        [JsonProperty("4X6LABEL")]
        LABEL4X6,

        [XmlEnum("4X6LABELL")]
        [JsonProperty("4X6LABELL")]
        LABEL4X6L,

        [JsonProperty("6X4LABEL")]
        [XmlEnum("6X4LABEL")]
        LABEL6X4,

        [JsonProperty("4X6LABELP")]
        [XmlEnum("4X6LABELP")]
        LABEL4X6P,

        [JsonProperty("4X6LABELP PAGE")]
        [XmlEnum("4X6LABELP PAGE")]
        LABEL4X6P_PAGE,

        [JsonProperty("SEPARATECONTINUEPAGE")]
        [XmlEnum("SEPARATECONTINUEPAGE")]
        SEPARATECONTINUEPAGE,

        [JsonProperty("4X6LABELZPL")]
        [XmlEnum("4X6LABELZPL")]
        LABEL4X6ZPL,

        [JsonProperty("4X5LABEL")]
        [XmlEnum("4X5LABEL")]
        LABEL4X5,

        [JsonProperty("3X5FMB")]
        [XmlEnum("3X5FMB")]
        FMB3X5
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReceiptOption
    {
        [JsonProperty("NONE")]
        [XmlEnum("NONE")]
        None,

        [JsonProperty("SAME_PAGE")]
        [XmlEnum("SAME PAGE")]
        SAME_PAGE,

        [JsonProperty("SEPARATE_PAGE")]
        [XmlEnum("SEPARATE PAGE")]
        SEPARATE_PAGE
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceType
    {
        [XmlEnum( "PRIORITY EXPRESS")]
        [JsonProperty("PRIORITY EXPRESS")]
        PRIORITY_EXPRESS,

        [XmlEnum( "PRIORITY")]
        [JsonProperty("PRIORITY")]
        PRIORITY,

        [XmlEnum( "FIRST CLASS")]
        [JsonProperty("FIRST CLASS")]
        FIRST_CLASS,

        [XmlEnum( "PARCEL SELECT GROUND")]
        [JsonProperty("PARCEL SELECT GROUND")]
        PARCEL_SELECT_GROUND,

        [XmlEnum( "LIBRARY")]
        [JsonProperty("LIBRARY")]
        LIBRARY,

        [XmlEnum( "MEDIA")]
        [JsonProperty("MEDIA")]
        MEDIA,

        [XmlEnum( "BPM")]
        [JsonProperty("BPM")]
        BPM,

        [XmlEnum( "PRIORITY MAIL CUBIC")]
        [JsonProperty("PRIORITY MAIL CUBIC")]
        PRIORITY_MAIL_CUBIC,


        [XmlEnum( "PARCEL_SELECT")]
        [JsonProperty("PARCEL_SELECT")]
        PARCEL_SELECT,

        [XmlEnum( "PARCEL_SELECT_LIGHTWEIGHT")]
        [JsonProperty("PARCEL_SELECT_LIGHTWEIGHT")]
        PARCEL_SELECT_LIGHTWEIGHT,

        [XmlEnum( "USPS_CONNECT_LOCAL")]
        [JsonProperty("USPS_CONNECT_LOCAL")]
        USPS_CONNECT_LOCAL,

        [XmlEnum( "USPS_CONNECT_REGIONAL")]
        [JsonProperty("USPS_CONNECT_REGIONAL")]
        USPS_CONNECT_REGIONAL,

        [XmlEnum( "USPS_CONNECT_MAIL")]
        [JsonProperty("USPS_CONNECT_MAIL")]
        USPS_CONNECT_MAIL,

        [XmlEnum( "USPS_GROUND_ADVANTAGE")]
        [JsonProperty("USPS_GROUND_ADVANTAGE")]
        USPS_GROUND_ADVANTAGE,

        [XmlEnum( "PRIORITY_MAIL_EXPRESS")]
        [JsonProperty("PRIORITY_MAIL_EXPRESS")]
        PRIORITY_MAIL_EXPRESS,

        [XmlEnum( "PRIORITY_MAIL")]
        [JsonProperty("PRIORITY_MAIL")]
        PRIORITY_MAIL,

        [XmlEnum( "FIRST-CLASS_PACKAGE_SERVICE")]
        [JsonProperty("FIRST-CLASS_PACKAGE_SERVICE")]
        FIRST_CLASS_PACKAGE_SERVICE,

        [XmlEnum( "LIBRARY_MAIL")]
        [JsonProperty("LIBRARY_MAIL")]
        LIBRARY_MAIL,

        [XmlEnum( "MEDIA_MAIL")]
        [JsonProperty("MEDIA_MAIL")]
        MEDIA_MAIL,

        [XmlEnum( "BOUND_PRINTED_MATTER")]
        [JsonProperty("BOUND_PRINTED_MATTER")]
        BOUND_PRINTED_MATTER,

        [XmlEnum( "DOMESTIC_MATTER_FOR_THE_BLIND")]
        [JsonProperty("DOMESTIC_MATTER_FOR_THE_BLIND")]
        DOMESTIC_MATTER_FOR_THE_BLIND,

        [XmlEnum( "FIRST-CLASS_PACKAGE_RETURN_SERVICE")]
        [JsonProperty("FIRST-CLASS_PACKAGE_RETURN_SERVICE")]
        FIRST_CLASS_PACKAGE_RETURN_SERVICE,

        [XmlEnum( "GROUND_RETURN_SERVICE")]
        [JsonProperty("GROUND_RETURN_SERVICE")]
        GROUND_RETURN_SERVICE,

        [XmlEnum( "PRIORITY_MAIL_EXPRESS_RETURN_SERVICE")]
        [JsonProperty("PRIORITY_MAIL_EXPRESS_RETURN_SERVICE")]
        PRIORITY_MAIL_EXPRESS_RETURN_SERVICE,

        [XmlEnum( "PRIORITY_MAIL_RETURN_SERVICE")]
        [JsonProperty("PRIORITY_MAIL_RETURN_SERVICE")]
        PRIORITY_MAIL_RETURN_SERVICE,

        [XmlEnum( "USPS_GROUND_ADVANTAGE_RETURN_SERVICE")]
        [JsonProperty("USPS_GROUND_ADVANTAGE_RETURN_SERVICE")]
        USPS_GROUND_ADVANTAGE_RETURN_SERVICE
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RateIndicator
    {
        [XmlEnum( "3D")]
        [JsonProperty("3D")]
        Three_Digit,

        [XmlEnum( "3N")]
        [JsonProperty("3N")]
        Three_Digit_Dimensional_Rectangular,

        [XmlEnum( "3R")]
        [JsonProperty("3R")]
        Three_Digit_Dimensional_Nonrectangular,

        [XmlEnum( "5D")]
        [JsonProperty("5D")]
        Five_Digit,

        [XmlEnum( "BA")]
        [JsonProperty("BA")]
        Basic,

        [XmlEnum( "BB")]
        [JsonProperty("BB")]
        Mixed_NDC,

        [XmlEnum( "C1")]
        [JsonProperty("C1")]
        Cubic_Pricing_Tier_1,

        [XmlEnum( "C2")]
        [JsonProperty("C2")]
        Cubic_Pricing_Tier_2,

        [XmlEnum( "C3")]
        [JsonProperty("C3")]
        Cubic_Pricing_Tier_3,

        [XmlEnum( "C4")]
        [JsonProperty("C4")]
        Cubic_Pricing_Tier_4,

        [XmlEnum( "C5")]
        [JsonProperty("C5")]
        Cubic_Pricing_Tier_5,

        [XmlEnum( "CP")]
        [JsonProperty("CP")]
        Cubic_Parcel,

        [XmlEnum( "CM")]
        [JsonProperty("CM")]
        Connect_Local_Mail,

        [XmlEnum( "DC")]
        [JsonProperty("DC")]
        NDC,

        [XmlEnum( "DE")]
        [JsonProperty("DE")]
        SCF,

        [XmlEnum( "DF")]
        [JsonProperty("DF")]
        Five_Digit_DF,

        [XmlEnum( "DN")]
        [JsonProperty("DN")]
        Dimensional_Nonrectangular,

        [XmlEnum( "DR")]
        [JsonProperty("DR")]
        Dimensional_Rectangular,

        [XmlEnum( "E4")]
        [JsonProperty("E4")]
        Priority_Mail_Express_Flat_Rate_Envelope_Post_Office_To_Addressee,

        [XmlEnum( "E6")]
        [JsonProperty("E6")]
        Priority_Mail_Express_Legal_Flat_Rate_Envelope,

        [XmlEnum( "E7")]
        [JsonProperty("E7")]
        Priority_Mail_Express_Legal_Flat_Rate_Envelope_Sunday_Holiday,

        [XmlEnum( "FA")]
        [JsonProperty("FA")]
        Legal_Flat_Rate_Envelope,

        [XmlEnum( "FB")]
        [JsonProperty("FB")]
        Medium_Flat_Rate_Box_Large_Flat_Rate_Bag,

        [XmlEnum( "FE")]
        [JsonProperty("FE")]
        Flat_Rate_Envelope,

        [XmlEnum( "FP")]
        [JsonProperty("FP")]
        Padded_Flat_Rate_Envelope,

        [XmlEnum( "FS")]
        [JsonProperty("FS")]
        Small_Flat_Rate_Box,

        [XmlEnum( "LC")]
        [JsonProperty("LC")]
        Connect_Local_Single_Piece,

        [XmlEnum( "LF")]
        [JsonProperty("LF")]
        Flat_Rate_Box,

        [XmlEnum( "LL")]
        [JsonProperty("LL")]
        Large_Flat_Rate_Bag,

        [XmlEnum( "LO")]
        [JsonProperty("LO")]
        Connect_Local_Oversized,

        [XmlEnum( "LS")]
        [JsonProperty("LS")]
        Small_Flat_Rate_Bag,

        [XmlEnum( "NP")]
        [JsonProperty("NP")]
        Non_Presorted,

        [XmlEnum( "OS")]
        [JsonProperty("OS")]
        Oversized,

        [XmlEnum( "P5")]
        [JsonProperty("P5")]
        Cubic_Soft_Pack_Tier_1,

        [XmlEnum( "P6")]
        [JsonProperty("P6")]
        Cubic_Soft_Pack_Tier_2,

        [XmlEnum( "P7")]
        [JsonProperty("P7")]
        Cubic_Soft_Pack_Tier_3,

        [XmlEnum( "P8")]
        [JsonProperty("P8")]
        Cubic_Soft_Pack_Tier_4,

        [XmlEnum( "P9")]
        [JsonProperty("P9")]
        Cubic_Soft_Pack_Tier_5,

        [XmlEnum( "Q6")]
        [JsonProperty("Q6")]
        Cubic_Soft_Pack_Tier_6,

        [XmlEnum( "Q7")]
        [JsonProperty("Q7")]
        Cubic_Soft_Pack_Tier_7,

        [XmlEnum( "Q8")]
        [JsonProperty("Q8")]
        Cubic_Soft_Pack_Tier_8,

        [XmlEnum( "Q9")]
        [JsonProperty("Q9")]
        Cubic_Soft_Pack_Tier_9,

        [XmlEnum( "Q0")]
        [JsonProperty("Q0")]
        Cubic_Soft_Pack_Tier_10,

        [XmlEnum( "PA")]
        [JsonProperty("PA")]
        Priority_Mail_Express_Single_Piece,

        [XmlEnum( "PL")]
        [JsonProperty("PL")]
        Large_Flat_Rate_Box,

        [XmlEnum( "PM")]
        [JsonProperty("PM")]
        Large_Flat_Rate_Box_APO_FPO_DPO,

        [XmlEnum( "PR")]
        [JsonProperty("PR")]
        Presorted,

        [XmlEnum( "SN")]
        [JsonProperty("SN")]
        SCF_Dimensional_Nonrectangular,

        [XmlEnum( "SP")]
        [JsonProperty("SP")]
        Single_Piece,

        [XmlEnum( "SR")]
        [JsonProperty("SR")]
        SCF_Dimensional_Rectangular
    }

    public enum DeliveryOption
    {
        [XmlEnum( "1")]
        [JsonProperty("1")]
        NormalDelivery = 1,

        [XmlEnum( "2")]
        [JsonProperty("2")]
        NoWeekendDelivery = 2,

        [XmlEnum( "3")]
        [JsonProperty("3")]
        NoHolidayDelivery = 3,

        [XmlEnum( "4")]
        [JsonProperty("4")]
        NoWeekendorHolidayDelivery = 4,

        [XmlEnum( "5")]
        [JsonProperty("5")]
        NoSaturdayDelivery = 5,

        [XmlEnum( "6")]
        [JsonProperty("6")]
        NoSundayDelivery = 6,

        [XmlEnum( "7")]
        [JsonProperty("7")]
        SundayDelivery = 7,

        [XmlEnum( "8")]
        [JsonProperty("8")]
        HolidayDelivery = 8,

        [XmlEnum( "9")]
        [JsonProperty("9")]
        NoSundayorHolidayDelivery = 9,

        [XmlEnum( "E")]
        [JsonProperty("E")]
        ScheduledEndofDay,

        [XmlEnum( "F")]
        [JsonProperty("F")]
        ScheduledEndofDayNoSun_HolDelivery,

        [XmlEnum( "G")]
        [JsonProperty("G")]
        ScheduledEndofDayNoWeekend_HolDelivery
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageType
    {
        [XmlEnum( "PDF")]
        [JsonProperty("PDF")]
        PDF,

        [XmlEnum( "TIF")]
        [JsonProperty("TIF")]
        TIF,

        [XmlEnum( "NONE")]
        [JsonProperty("NONE")]
        None,

        [XmlEnum( "JPG")]
        [JsonProperty("JPG")]
        JPG,

        [XmlEnum( "SVG")]
        [JsonProperty("SVG")]
        SVG,

        [XmlEnum( "ZPL203DPI")]
        [JsonProperty("ZPL203DPI")]
        ZPL203DPI,

        [XmlEnum( "ZPL300DPI")]
        [JsonProperty("ZPL300DPI")]
        ZPL300DPI,

        [XmlEnum( "LABEL_BROKER")]
        [JsonProperty("LABEL_BROKER")]
        LabelBroker
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Machinable
    {
        [XmlEnum( "True")]
        [JsonProperty("True")]
        True,

        [XmlEnum( "False")]
        [JsonProperty("False")]
        False,

        [XmlEnum( "LETTERS")]
        [JsonProperty("LETTERS")]
        LETTERS,

        [XmlEnum( "FLATS")]
        [JsonProperty("FLATS")]
        FLATS,

        [XmlEnum( "MACHINABLE")]
        [JsonProperty("MACHINABLE")]
        MACHINABLE,

        [XmlEnum( "IRREGULAR")]
        [JsonProperty("IRREGULAR")]
        IRREGULAR,

        [XmlEnum( "NON_MACHINABLE")]
        [JsonProperty("NON_MACHINABLE")]
        NON_MACHINABLE
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum HoldForManifest
    {
        [XmlEnum( "Y")]
        [JsonProperty("Y")]
        Y,

        [XmlEnum( "N")]
        [JsonProperty("N")]
        N
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WeightUOM
    {
        [XmlEnum( "lb")]
        [JsonProperty("lb")]
        Lb
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DimensionUOM
    {
        [XmlEnum( "in")]
        [JsonProperty("in")]
        In
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContainerType
    {
        [XmlEnum( "VARIABLE")]
        [JsonProperty("VARIABLE")]
        VARIABLE,

        [XmlEnum( "FLAT RATE ENVELOPE")]
        [JsonProperty("FLAT RATE ENVELOPE")]
        FLAT_RATE_ENVELOPE,

        [XmlEnum( "LEGAL FLAT RATE ENVELOPE")]
        [JsonProperty("LEGAL FLAT RATE ENVELOPE")]
        LEGAL_FLAT_RATE_ENVELOPE,

        [XmlEnum( "PADDED FLAT RATE ENVELOPE")]
        [JsonProperty("PADDED FLAT RATE ENVELOPE")]
        PADDED_FLAT_RATE_ENVELOPE,

        [XmlEnum( "GIFT CARD FLAT RATE ENVELOPE")]
        [JsonProperty("GIFT CARD FLAT RATE ENVELOPE")]
        GIFT_CARD_FLAT_RATE_ENVELOPE,

        [XmlEnum( "SM FLAT RATE ENVELOPE")]
        [JsonProperty("SM FLAT RATE ENVELOPE")]
        SM_FLAT_RATE_ENVELOPE,

        [XmlEnum( "WINDOW FLAT RATE ENVELOPE")]
        [JsonProperty("WINDOW FLAT RATE ENVELOPE")]
        WINDOW_FLAT_RATE_ENVELOPE,

        [XmlEnum( "SM FLAT RATE BOX")]
        [JsonProperty("SM FLAT RATE BOX")]
        SM_FLAT_RATE_BOX,

        [XmlEnum( "MD FLAT RATE BOX")]
        [JsonProperty("MD FLAT RATE BOX")]
        MD_FLAT_RATE_BOX,

        [XmlEnum( "LG FLAT RATE BOX")]
        [JsonProperty("LG FLAT RATE BOX")]
        LG_FLAT_RATE_BOX,

        [XmlEnum( "REGIONALRATEBOXA")]
        [JsonProperty("REGIONALRATEBOXA")]
        REGIONALRATEBOXA,

        [XmlEnum( "REGIONALRATEBOXB")]
        [JsonProperty("REGIONALRATEBOXB")]
        REGIONALRATEBOXB,

        [XmlEnum( "RECTANGULAR")]
        [JsonProperty("RECTANGULAR")]
        RECTANGULAR,

        [XmlEnum( "NONRECTANGULAR")]
        [JsonProperty("NONRECTANGULAR")]
        NONRECTANGULAR,

        [XmlEnum( "PACKAGE SERVICE")]
        [JsonProperty("PACKAGE SERVICE")]
        PACKAGE_SERVICE,

        [XmlEnum( "CUBIC PARCELS")]
        [JsonProperty("CUBIC PARCELS")]
        CUBIC_PARCELS,

        [XmlEnum( "CUBIC SOFT PACK")]
        [JsonProperty("CUBIC SOFT PACK")]
        CUBIC_SOFT_PACK
    }


    //Duplicate of above because of outer label class
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Container
    {
        [XmlEnum("VARIABLE")]
        [JsonProperty("VARIABLE")]
        VARIABLE,

        [XmlEnum("FLAT RATE ENVELOPE")]
        [JsonProperty("FLAT RATE ENVELOPE")]
        FLAT_RATE_ENVELOPE,

        [XmlEnum("LEGAL FLAT RATE ENVELOPE")]
        [JsonProperty("LEGAL FLAT RATE ENVELOPE")]
        LEGAL_FLAT_RATE_ENVELOPE,

        [XmlEnum("PADDED FLAT RATE ENVELOPE")]
        [JsonProperty("PADDED FLAT RATE ENVELOPE")]
        PADDED_FLAT_RATE_ENVELOPE,

        [XmlEnum("GIFT CARD FLAT RATE ENVELOPE")]
        [JsonProperty("GIFT CARD FLAT RATE ENVELOPE")]
        GIFT_CARD_FLAT_RATE_ENVELOPE,

        [XmlEnum("SM FLAT RATE ENVELOPE")]
        [JsonProperty("SM FLAT RATE ENVELOPE")]
        SM_FLAT_RATE_ENVELOPE,

        [XmlEnum("WINDOW FLAT RATE ENVELOPE")]
        [JsonProperty("WINDOW FLAT RATE ENVELOPE")]
        WINDOW_FLAT_RATE_ENVELOPE,

        [XmlEnum("SM FLAT RATE BOX")]
        [JsonProperty("SM FLAT RATE BOX")]
        SM_FLAT_RATE_BOX,

        [XmlEnum("MD FLAT RATE BOX")]
        [JsonProperty("MD FLAT RATE BOX")]
        MD_FLAT_RATE_BOX,

        [XmlEnum("LG FLAT RATE BOX")]
        [JsonProperty("LG FLAT RATE BOX")]
        LG_FLAT_RATE_BOX,

        [XmlEnum("REGIONALRATEBOXA")]
        [JsonProperty("REGIONALRATEBOXA")]
        REGIONALRATEBOXA,

        [XmlEnum("REGIONALRATEBOXB")]
        [JsonProperty("REGIONALRATEBOXB")]
        REGIONALRATEBOXB,

        [XmlEnum("RECTANGULAR")]
        [JsonProperty("RECTANGULAR")]
        RECTANGULAR,

        [XmlEnum("NONRECTANGULAR")]
        [JsonProperty("NONRECTANGULAR")]
        NONRECTANGULAR,

        [XmlEnum("PACKAGE SERVICE")]
        [JsonProperty("PACKAGE SERVICE")]
        PACKAGE_SERVICE,

        [XmlEnum("CUBIC PARCELS")]
        [JsonProperty("CUBIC PARCELS")]
        CUBIC_PARCELS,

        [XmlEnum("CUBIC SOFT PACK")]
        [JsonProperty("CUBIC SOFT PACK")]
        CUBIC_SOFT_PACK
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum PriceOption
    {
        [XmlEnum( "Retail")]
        [JsonProperty("Retail")]
        Retail,

        [XmlEnum( "Commercial Plus")]
        [JsonProperty("Commercial Plus")]
        CommercialPlus,

        [XmlEnum( "Commercial Base")]
        [JsonProperty("Commercial Base")]
        CommercialBase
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ToContactPreference
    {
        [XmlEnum( "EMAIL")]
        [JsonProperty("EMAIL")]
        EMAIL,

        [XmlEnum( "EMAILSMS")]
        [JsonProperty("EMAILSMS")]
        EMAILSMS,

        [XmlEnum( "SMS")]
        [JsonProperty("SMS")]
        SMS,

        [XmlEnum( "WAIVED")]
        [JsonProperty("WAIVED")]
        WAIVED
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContentType
    {
        [XmlEnum( "HAZMAT")]
        [JsonProperty("HAZMAT")]
        Hazmat,

        [XmlEnum( "CREMATED_REMAINS")]
        [JsonProperty("CREMATED_REMAINS")]
        Cremated_Remains,

        [XmlEnum( "BEES")]
        [JsonProperty("BEES")]
        Bees,

        [XmlEnum( "DAY_OLD_POULTRY")]
        [JsonProperty("DAY_OLD_POULTRY")]
        Day_Old_Poultry,

        [XmlEnum( "ADULT_BIRDS")]
        [JsonProperty("ADULT_BIRDS")]
        Adult_Birds,

        [XmlEnum( "Lives")]
        [JsonProperty("Lives")]
        Lives,

        [XmlEnum( "OTHER_LIVES")]
        [JsonProperty("OTHER_LIVES")]
        Other_Lives,

        [XmlEnum( "PERISHABLE")]
        [JsonProperty("PERISHABLE")]
        Perishable,

        [XmlEnum( "PHARMACEUTICALS")]
        [JsonProperty("PHARMACEUTICALS")]
        Pharmaceuticals,

        [XmlEnum( "MEDICAL_SUPPLIES")]
        [JsonProperty("MEDICAL_SUPPLIES")]
        Medical_Supplies
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DestinationEntryFacilityType
    {
        [XmlEnum( "NONE")]
        [JsonProperty("NONE")]
        None,

        [XmlEnum( "DESTINATION_NETWORK_DISTRIBUTION_CENTER")]
        [JsonProperty("DESTINATION_NETWORK_DISTRIBUTION_CENTER")]
        DESTINATION_NETWORK_DISTRIBUTION_CENTER,

        [XmlEnum( "DESTINATION_SECTIONAL_CENTER_FACILITY")]
        [JsonProperty("DESTINATION_SECTIONAL_CENTER_FACILITY")]
        DESTINATION_SECTIONAL_CENTER_FACILITY,

        [XmlEnum( "DESTINATION_DELIVERY_UNIT")]
        [JsonProperty("DESTINATION_DELIVERY_UNIT")]
        DESTINATION_DELIVERY_UNIT,

        [XmlEnum( "DESTINATION_SERVICE_HUB")]
        [JsonProperty("DESTINATION_SERVICE_HUB")]
        DESTINATION_SERVICE_HUB
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionCode
    {
        [XmlEnum( "M0")]
        [JsonProperty("M0")]

        MailerOwner,

        [XmlEnum( "S0")]
        [JsonProperty("S0")]

        ServiceProvider
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContentDescription
    {
        [XmlEnum( "Bees")]
        [JsonProperty("Bees")]
        Bees,

        [XmlEnum( "DayOldPoultry")]
        [JsonProperty("DayOldPoultry")]
        DayOldPoultry,

        [XmlEnum( "AdultBirds")]
        [JsonProperty("AdultBirds")]
        AdultBirds,

        [XmlEnum( "Other")]
        [JsonProperty("Other")]
        Other
    }

    public enum AncillaryServiceEndorsements
    {
        [XmlEnum( "CHANGE_SERVICE_REQUESTED")]
        [JsonProperty("CHANGE_SERVICE_REQUESTED")]
        Change_Service_Requested,

        [XmlEnum( "ADDRESS_SERVICE_REQUESTED")]
        [JsonProperty("ADDRESS_SERVICE_REQUESTED")]
        Address_Service_Requested,

        [XmlEnum( "ELECTRONIC_SERVICE_REQUESTED")]
        [JsonProperty("ELECTRONIC_SERVICE_REQUESTED")]
        Electronic_Service_Requested,

        [XmlEnum( "RETURN_SERVICE_REQUESTED")]
        [JsonProperty("RETURN_SERVICE_REQUESTED")]
        Return_Service_Requested,

        [XmlEnum( "TEMP_RETURN_SERVICE_REQUESTED")]
        [JsonProperty("TEMP_RETURN_SERVICE_REQUESTED")]
        Temporary_Return_Service_Requested,

        [XmlEnum( "FORWARDING_SERVICE_REQUESTED")]
        [JsonProperty("FORWARDING_SERVICE_REQUESTED")]
        Forwarding_Service_Requested
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortType
    {
        [XmlEnum( "TRUCK_BEDLOAD")]
        [JsonProperty("TRUCK_BEDLOAD")]
        Truck_Bedload,

        [XmlEnum( "OPEN_DISTRIBUTION_FULL_POSTAL_PAKS")]
        [JsonProperty("OPEN_DISTRIBUTION_FULL_POSTAL_PAKS")]
        Open_Distribution_Full_Postal_Paks,

        [XmlEnum( "OPEN_DISTRIBUTE_EMM_TRAY_BOX")]
        [JsonProperty("OPEN_DISTRIBUTE_EMM_TRAY_BOX")]
        Open_Distribute_EMM_Tray_Box,

        [XmlEnum( "OPEN_DISTRIBUTE_FULL_TRAY_BOX")]
        [JsonProperty("OPEN_DISTRIBUTE_FULL_TRAY_BOX")]
        Open_Distribute_Full_Tray_Box,

        [XmlEnum( "OPEN_DISTRIBUTE_HALF_TRAY_BOX")]
        [JsonProperty("OPEN_DISTRIBUTE_HALF_TRAY_BOX")]
        Open_Distribute_Half_TrayBox,

        [XmlEnum( "OPEN_DISTRIBUTE_HALF_POSTAL_PAKS")]
        [JsonProperty("OPEN_DISTRIBUTE_HALF_POSTAL_PAKS")]
        Open_Distribute_Half_Postal_Paks,

        [XmlEnum( "OPEN_DISTRIBUTE_PALLET")]
        [JsonProperty("OPEN_DISTRIBUTE_PALLET")]
        Open_Distribute_Pallet,

        [XmlEnum( "OPEN_DISTRIBUTE_FLAT_TUB_TRAY_BOX")]
        [JsonProperty("OPEN_DISTRIBUTE_FLAT_TUB_TRAY_BOX")]
        Open_Distribute_Flat_Tub_TrayBox,

        [XmlEnum( "PALLET")]
        [JsonProperty("PALLET")]
        Pallet,

        [XmlEnum( "RECEPTACLE")]
        [JsonProperty("RECEPTACLE")]
        Receptacle,

        [XmlEnum( "SACK")]
        [JsonProperty("SACK")]
        Sack
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RestrictionType
    {
        [XmlEnum( "QUARANTINE")]
        [JsonProperty("QUARANTINE")]
        Quarantine,

        [XmlEnum( "SANITARY_INSPECTION")]
        [JsonProperty("SANITARY_INSPECTION")]
        Sanitary_Inspection,

        [XmlEnum( "PHYTOSANITARY_INSPECTION")]
        [JsonProperty("PHYTOSANITARY_INSPECTION")]
        Phytosanitary_Inspection,

        [XmlEnum( "OTHER")]
        [JsonProperty("OTHER")]
        Other
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomsContentType
    {
        [XmlEnum( "MERCHANDISE")]
        [JsonProperty("MERCHANDISE")]
        MERCHANDISE,

        [XmlEnum( "GIFT")]
        [JsonProperty("GIFT")]
        GIFT,

        [XmlEnum( "DOCUMENT")]
        [JsonProperty("DOCUMENT")]
        DOCUMENT,

        //Duplicate of above because of outer code changes
        [XmlEnum( "DOCUMENT")]
        [JsonProperty("DOCUMENT")]
        DOCUMENTS,

        [XmlEnum( "COMMERCIAL_SAMPLE")]
        [JsonProperty("COMMERCIAL_SAMPLE")]
        COMMERCIAL_SAMPLE,

        [XmlEnum( "RETURN")]
        [JsonProperty("RETURN")]
        RETURN,

        [XmlEnum( "RETURNED_GOODS")]
        [JsonProperty("RETURNED_GOODS")]
        RETURNED_GOODS,

        [XmlEnum( "OTHER")]
        [JsonProperty("OTHER")]
        OTHER,

        [XmlEnum( "HUMANITARIAN_DONATIONS")]
        [JsonProperty("HUMANITARIAN_DONATIONS")]
        HUMANITARIAN_DONATIONS,

        [XmlEnum( "DANGEROUS_GOODS")]
        [JsonProperty("DANGEROUS_GOODS")]
        DANGEROUS_GOODS
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NonDeliveryOption
    {
        [XmlEnum( "RETURN")]
        [JsonProperty("RETURN")]
        RETURN,

        [XmlEnum( "REDIRECT")]
        [JsonProperty("REDIRECT")]
        REDIRECT,

        [XmlEnum( "ABANDON")]
        [JsonProperty("ABANDON")]
        ABANDON
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ePostageMailerReporting
    {
        [XmlEnum( "1")]
        [JsonProperty("1")]

        ePostage_sender_information_in_SSF = 1,

        [XmlEnum( "2")]
        [JsonProperty("2")]

        ePostage_sender_information_through_DES = 2,


        [XmlEnum( "3")]
        [JsonProperty("3")]

        ePostage_sender_information_using_child_MID = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum eVSCancelStatus
    {
        [XmlEnum( "Cancelled")]
        [JsonProperty("Cancelled")]

        Cancelled,

        [XmlEnum( "Not Cancelled")]
        [JsonProperty("Not Cancelled")]

        Not_Cancelled
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum eVSCancelReason
    {
        [XmlEnum( "Order Cancelled Successfully")]
        [JsonProperty("Order Cancelled Successfully")]
        Order_Cancelled_Successfully,

        [XmlEnum( "Order Already Cancelled")]
        [JsonProperty("Order Already Cancelled")]
        Order_Already_Cancelled,

        [XmlEnum( "Order Not Found")]
        [JsonProperty("Order Not Found")]
        Order_Not_Found
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AddressType
    {
        [XmlEnum( "INVALID")]
        [JsonProperty("INVALID")]
        INVALID,

        [XmlEnum( "FOREIGN")]
        [JsonProperty("FOREIGN")]
        FOREIGN,

        [XmlEnum( "POBOX")]
        [JsonProperty("POBOX")]
        POBOX,

        [XmlEnum( "Military")]
        [JsonProperty("Military")]
        Military,

        [XmlEnum( "Territory")]
        [JsonProperty("Territory")]
        Territory,

        //DC,Hawaii,Alaska and All Mainlands
        [XmlEnum( "DHAM")]
        [JsonProperty("DHAM")]
        DHAM
    }

    //[JsonConverter(typeof(StringEnumConverter))]
    public enum ExtraServiceType
    {
        [XmlEnum( "365")]
        [JsonProperty("365")]
        Global_Direct_Entry = 365,

        [XmlEnum( "415")]
        [JsonProperty("415")]
        USPS_Label_Delivery_Service = 415,

        [XmlEnum( "480")]
        [JsonProperty("480")]
        Tracking_Plus_6Months = 480,

        [XmlEnum( "481")]
        [JsonProperty("481")]
        Tracking_Plus1_Year = 481,

        [XmlEnum( "482")]
        [JsonProperty("482")]
        Tracking_Plus3_Years = 482,

        [XmlEnum( "483")]
        [JsonProperty("483")]
        Tracking_Plus_5Years = 483,

        [XmlEnum( "484")]
        [JsonProperty("484")]
        Tracking_Plus_7Years = 484,

        [XmlEnum( "485")]
        [JsonProperty("485")]
        Tracking_Plus_10Years = 485,

        [XmlEnum( "486")]
        [JsonProperty("486")]
        Tracking_Plus_Signature_3Years = 486,

        [XmlEnum( "487")]
        [JsonProperty("487")]
        Tracking_Plus_Signature_5Years = 487,

        [XmlEnum( "488")]
        [JsonProperty("488")]
        Tracking_Plus_Signature_7Years = 488,

        [XmlEnum( "489")]
        [JsonProperty("489")]
        Tracking_Plus_Signature_10Years = 489,

        [XmlEnum( "810")]
        [JsonProperty("810")]
        Hazardous_Materials_Air_Eligible_Ethanol = 810,

        [XmlEnum( "811")]
        [JsonProperty("811")]
        Hazardous_Materials_Class1_Toy_Propellant_Safety_Fuse_Package = 811,

        [XmlEnum( "812")]
        [JsonProperty("812")]
        Hazardous_Materials_Class3_Flammable_And_Combustible_Liquids = 812,

        [XmlEnum( "813")]
        [JsonProperty("813")]
        Hazardous_Materials_Class7_Radioactive_Materials = 813,

        [XmlEnum( "814")]
        [JsonProperty("814")]
        Hazardous_Materials_Class8_Air_Eligible_Corrosive_Materials = 814,

        [XmlEnum( "815")]
        [JsonProperty("815")]
        Hazardous_Materials_Class8_Nonspillable_Wet_Batteries = 815,

        [XmlEnum( "816")]
        [JsonProperty("816")]
        Hazardous_Materials_Class9_Lithium_Battery_Marked_Ground_Only = 816,

        [XmlEnum( "817")]
        [JsonProperty("817")]
        Hazardous_Materials_Class9_Lithium_Battery_Returns = 817,

        [XmlEnum( "818")]
        [JsonProperty("818")]
        Hazardous_Materials_Class9_Marked_Lithium_Batteries = 818,

        [XmlEnum( "819")]
        [JsonProperty("819")]
        Hazardous_Materials_Class9_DryIce = 819,

        [XmlEnum( "820")]
        [JsonProperty("820")]
        Hazardous_Materials_Class9_Unmarked_Lithium_Batteries = 820,

        [XmlEnum( "821")]
        [JsonProperty("821")]
        Hazardous_Materials_Class9_Magnetized_Materials = 821,

        [XmlEnum( "822")]
        [JsonProperty("822")]
        Hazardous_Materials_Division41_Mailable_Flammable_Solids_And_Safety_Matches = 822,

        [XmlEnum( "823")]
        [JsonProperty("823")]
        Hazardous_Materials_Division51_Oxidizers = 823,

        [XmlEnum( "824")]
        [JsonProperty("824")]
        Hazardous_Materials_Division52_Organic_Peroxides = 824,

        [XmlEnum( "825")]
        [JsonProperty("825")]
        Hazardous_Materials_Division61_Toxic_Materials = 825,

        [XmlEnum( "826")]
        [JsonProperty("826")]
        Hazardous_Materials_Division62_Biological_Materials = 826,

        [XmlEnum( "827")]
        [JsonProperty("827")]
        Hazardous_Materials_Excepted_Quantity_Provision = 827,

        [XmlEnum( "828")]
        [JsonProperty("828")]
        Hazardous_Materials_Ground_Only_Hazardous_Materials = 828,

        [XmlEnum( "829")]
        [JsonProperty("829")]
        Hazardous_Materials_Air_Eligible_ID8000_Consumer_Commodity = 829,

        [XmlEnum( "830")]
        [JsonProperty("830")]
        Hazardous_Materials_Lighters = 830,

        [XmlEnum( "831")]
        [JsonProperty("831")]
        HazardousMaterials_Limited_Quantity_Ground = 831,

        [XmlEnum( "832")]
        [JsonProperty("832")]
        HazardousMaterials_Small_Quantity_Provision_Markings_Required = 832,

        [XmlEnum( "857")]
        [JsonProperty("857")]
        Hazardous_Materials = 857,

        [XmlEnum( "910")]
        [JsonProperty("910")]
        Certified_Mail = 910,

        [XmlEnum( "911")]
        [JsonProperty("911")]
        Certified_Mail_Restricted_Delivery = 911,

        [XmlEnum( "912")]
        [JsonProperty("912")]
        Certified_Mail_Adult_Signature_Required = 912,

        [XmlEnum( "913")]
        [JsonProperty("913")]
        Certified_Mail_Adult_Signature_Restricted_Delivery = 913,

        [XmlEnum( "920")]
        [JsonProperty("920")]
        USPS_Tracking_Electronic = 920,

        [XmlEnum("155")]
        [JsonProperty("155")]
        USPS_Tracking_Electronic_Old = 155,

        [XmlEnum( "921")]
        [JsonProperty("921")]
        Signature_Confirmation = 921,

        [XmlEnum( "922")]
        [JsonProperty("922")]
        Adult_Signature_Required = 922,

        [XmlEnum( "923")]
        [JsonProperty("923")]
        Adult_Signature_Restricted_Delivery = 923,

        [XmlEnum( "924")]
        [JsonProperty("924")]
        Signature_Confirmation_Restricted_Delivery = 924,

        [XmlEnum( "925")]
        [JsonProperty("925")]
        Priority_Mail_Express_Insurance = 925,

        [XmlEnum( "930")]
        [JsonProperty("930")]
        Insurance = 930,

        [XmlEnum( "934")]
        [JsonProperty("934")]
        Insurance_Restricted_Delivery = 934,

        [XmlEnum( "955")]
        [JsonProperty("955")]
        Return_Receipt = 955,

        [XmlEnum( "957")]
        [JsonProperty("957")]
        Return_Receipt_Electronic = 957,

        [XmlEnum( "981")]
        [JsonProperty("981")]
        Signature_Requested_Priority_Mail_Express_Only = 981,

        [XmlEnum( "986")]
        [JsonProperty("986")]
        PO_To_Addressee_Priority_Mail_Express_Only = 986,

        [XmlEnum( "991")]
        [JsonProperty("991")]
        Sunday_Delivery = 991
    }
}