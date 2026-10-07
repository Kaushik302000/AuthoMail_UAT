using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Core
{
    public class FedExEnum
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum MergeLabelDocOption
        {
            NONE,
            LABELS_AND_DOCS,
            LABELS_ONLY
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum TinType
        {
            PERSONAL_NATIONAL,
            PERSONAL_STATE,
            FEDERAL,
            BUSINESS_NATIONAL,
            BUSINESS_STATE,
            BUSINESS_UNION
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PickupType
        {
            CONTACT_FEDEX_TO_SCHEDULE,
            DROPOFF_AT_FEDEX_LOCATION,
            USE_SCHEDULED_PICKUP,
            REGULAR_PICKUP
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ServiceType
        {
            EUROPE_FIRST_INTERNATIONAL_PRIORITY,
            FEDEX_1_DAY_FREIGHT,
            FEDEX_2_DAY,
            FEDEX_2_DAY_FREIGHT,
            FEDEX_3_DAY_FREIGHT,
            FEDEX_EXPRESS_SAVER,
            FEDEX_GROUND,
            FIRST_OVERNIGHT,
            GROUND_HOME_DELIVERY,
            INTERNATIONAL_ECONOMY,
            INTERNATIONAL_ECONOMY_FREIGHT,
            INTERNATIONAL_FIRST,
            INTERNATIONAL_PRIORITY,
            INTERNATIONAL_PRIORITY_FREIGHT,
            PRIORITY_OVERNIGHT,
            SMART_POST,
            STANDARD_OVERNIGHT,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PackagingType
        {
            FEDEX_10KG_BOX,
            FEDEX_25KG_BOX,
            FEDEX_BOX,
            FEDEX_ENVELOPE,
            FEDEX_PAK,
            FEDEX_TUBE,
            YOUR_PACKAGING,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PaymentType
        {
            ACCOUNT,
            COLLECT,
            RECIPIENT,
            SENDER,
            THIRD_PARTY,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ShipmentSpecialServiceType
        {
            BROKER_SELECT_OPTION,
            COD,
            DRY_ICE,
            ELECTRONIC_TRADE_DOCUMENTS,
            EMAIL_NOTIFICATION,
            FUTURE_DAY_SHIPMENT,
            HOLD_AT_LOCATION,
            HOME_DELIVERY_PREMIUM,
            INSIDE_DELIVERY,
            INSIDE_PICKUP,
            PENDING_SHIPMENT,
            RETURN_SHIPMENT,
            SATURDAY_DELIVERY,
            SATURDAY_PICKUP,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocumentType
        {
            CERTIFICATE_OF_ORIGIN,
            COMMERCIAL_INVOICE,
            ETD_LABEL,
            USMCA_CERTIFICATION_OF_ORIGIN,
            OTHER,
            PRO_FORMA_INVOICE,
            USMCA_COMMERCIAL_INVOICE_CERTIFICATION_OF_ORIGIN
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RequestedDocumentTypes
        {
            CERTIFICATE_OF_ORIGIN,
            COMMERCIAL_INVOICE,
            CUSTOM_PACKAGE_DOCUMENT,
            CUSTOM_SHIPMENT_DOCUMENT,
            CUSTOMER_SPECIFIED_LABELS,
            EXPORT_DECLARATION,
            DANGEROUS_GOODS_SHIPPERS_DECLARATION,
            GENERAL_AGENCY_AGREEMENT,
            LABEL,
            USMCA_CERTIFICATION_OF_ORIGIN,
            OP_900,
            PENDING_SHIPMENT_EMAIL_NOTIFICATION,
            PRO_FORMA_INVOICE,
            RETURN_INSTRUCTIONS,
            USMCA_COMMERCIAL_INVOICE_CERTIFICATION_OF_ORIGIN
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AllowedSpecialService
        {
            SATURDAY_DELIVERY,
            SATURDAY_PICKUP
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ReturnType
        {
            FEDEX_TAG,
            PENDING,
            PRINT_RETURN_LABEL
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum PendingShipmentType
        {
            EMAIL
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RecommendedDocumentSpecificationType
        {
            ANTIQUE_STATEMENT_EUROPEAN_UNION,
            ANTIQUE_STATEMENT_UNITED_STATES,
            ASSEMBLER_DECLARATION,
            BEARING_WORKSHEET,
            CERTIFICATE_OF_SHIPMENTS_TO_SYRIA,
            COMMERCIAL_INVOICE_FOR_THE_CARIBBEAN_COMMON_MARKET,
            CONIFEROUS_SOLID_WOOD_PACKAGING_MATERIAL_TO_THE_PEOPLES_REPUBLIC_OF_CHINA,
            DECLARATION_FOR_FREE_ENTRY_OF_RETURNED_AMERICAN_PRODUCTS,
            DECLARATION_OF_BIOLOGICAL_STANDARDS,
            DECLARATION_OF_IMPORTED_ELECTRONIC_PRODUCTS_SUBJECT_TO_RADIATION_CONTROL_STANDARD,
            ELECTRONIC_INTEGRATED_CIRCUIT_WORKSHEET,
            FILM_AND_VIDEO_CERTIFICATE,
            INTERIM_FOOTWEAR_INVOICE,
            USMCA_CERTIFICATION_OF_ORIGIN_CANADA_ENGLISH,
            USMCA_CERTIFICATION_OF_ORIGIN_CANADA_FRENCH,
            USMCA_CERTIFICATION_OF_ORIGIN_SPANISH,
            USMCA_CERTIFICATION_OF_ORIGIN_UNITED_STATES,
            PACKING_LIST,
            PRINTED_CIRCUIT_BOARD_WORKSHEET,
            REPAIRED_WATCH_BREAKOUT_WORKSHEET,
            STATEMENT_REGARDING_THE_IMPORT_OF_RADIO_FREQUENCY_DEVICES,
            TOXIC_SUBSTANCES_CONTROL_ACT,
            UNITED_STATES_CARIBBEAN_BASIN_TRADE_PARTNERSHIP_ACT_CERTIFICATE_OF_ORIGIN_TEXTILES,
            UNITED_STATES_CARIBBEAN_BASIN_TRADE_PARTNERSHIP_ACT_CERTIFICATE_OF_ORIGIN_NON_TEXTILES,
            UNITED_STATES_NEW_WATCH_WORKSHEET,
            UNITED_STATES_WATCH_REPAIR_DECLARATION
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LocationType
        {
            FEDEX_AUTHORIZED_SHIP_CENTER,
            FEDEX_EXPRESS_STATION,
            FEDEX_FACILITY,
            FEDEX_FREIGHT_SERVICE_CENTER,
            FEDEX_GROUND_TERMINAL,
            FEDEX_HOME_DELIVERY_STATION,
            FEDEX_OFFICE,
            FEDEX_ONSITE,
            FEDEX_SELF_SERVICE_LOCATION,
            FEDEX_SHIPSITE,
            FEDEX_SHIP_AND_GET,
            FEDEX_SMART_POST_HUB,
            FEDEX_STAFFED,
            RETAIL_ALLICANCE_LOCATION
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RateType
        {
            ACCOUNT,
            CURRENT,
            CUSTOM,
            ACTUAL,
            LIST,
            INCENTIVE,
            PREFERRED,
            PREFERRED_INCENTIVE,
            PREFERRED_CURRENCY
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RateLevelType
        {
            BUNDLED_RATE,
            INDIVIDUAL_PACKAGE_RATE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ChargeLevelType
        {
            CURRENT_PACKAGE,
            SUM_OF_PACKAGES
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ChargeType
        {
            COD_SURCHARGE,
            NET_CHARGE,
            NET_FREIGHT,
            TOTAL_CUSTOMER_CHARGE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CodCollectionType
        {
            ANY,
            CASH,
            GUARANTEED_FUNDS,
            COMPANY_CHECK,
            PERSONAL_CHECK
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ReturnReferenceIndicatorType
        {
            INVOICE,
            PO,
            REFERENCE,
            TRACKING
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Units
        {
            KG,
            LB,
            CM,
            IN
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum InternationalControlledExportType
        {
            DEA_036,
            DEA_236,
            DSP_05,
            DSP_61,
            DSP_73,
            DSP_85,
            DSP_LICENSE_AGREEMENT,
            WAREHOUSE_WITHDRAWAL
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum HomedeliveryPremiumType
        {
            APPOINTMENT,
            DATE_CERTAIN,
            EVENING
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AggregationType
        {
            PER_PACKAGE,
            PER_SHIPMENT
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EmailNotificationRecipientType
        {
            BROKER,
            OTHER,
            RECIPIENT,
            SHIPPER,
            THIRD_PARTY,
            OTHER1,
            OTHER2
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum NotificationFormatType
        {
            HTML,
            TEXT
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum NotificationEventType
        {
            ON_DELIVERY,
            ON_EXCEPTION,
            ON_SHIPMENT,
            ON_TENDER,
            ON_ESTIMATED_DELIVERY,
            ON_PICKUP,
            ON_LABEL,
            ON_PICKUP_DRIVER_ARRIVED,
            ON_PICKUP_DRIVER_ASSIGNED,
            ON_PICKUP_DRIVER_DEPARTED,
            ON_PICKUP_DRIVER_EN_ROUTE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RateElementBasis
        {
            NET_CHARGE,
            NET_FREIGHT,
            BASE_CHARGE,
            NET_CHARGE_EXCLUDING_TAXES
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RegulatoryControls
        {
            FOOD_OR_PERISHABLE,
            USMCA,
            NOT_APPLICABLE_FOR_LOW_VALUE_CUSTOMS_EXCEPTIONS,
            NOT_IN_FREE_CIRCULATION
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomerReferenceType
        {
            BILL_OF_LADING,
            CUSTOMER_REFERENCE,
            DEPARTMENT_NUMBER,
            INVOICE_NUMBER,
            P_O_NUMBER,
            SHIPMENT_INTEGRITY,
            STORE_NUMBER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum TaxesOrMiscellaneousChargeType
        {
            COMMISSIONS,
            DISCOUNTS,
            HANDLING_FEES,
            OTHER,
            ROYALTIES_AND_LICENSE_FEES,
            TAXES
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ShipmentPurpose
        {
            GIFT,
            NOT_SOLD,
            PERSONAL_EFFECTS,
            REPAIR_AND_RETURN,
            SAMPLE,
            SOLD,
            BUSINESS,
            CONSUMER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EMailNotificationRecipientType
        {
            SHIPPER,
            RECIPIENT,
            BROKER,
            OTHER,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum FreightOnValue
        {
            CARRIER_RISK,
            OWN_RISK
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum OriginCriterion
        {
            A,
            B,
            C,
            D,
            E
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RecipientCustomsIdType
        {
            COMPANY,
            INDIVIDUAL,
            PASSPORT
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomsOptionType
        {
            COURTESY_RETURN_LABEL,
            EXHIBITION_TRADE_SHOW,
            FAULTY_ITEM,
            FOLLOWING_REPAIR,
            FOR_REPAIR,
            ITEM_FOR_LOAN,
            OTHER,
            REJECTED,
            REPLACEMENT,
            TRIAL
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DestinationControlStatementType
        {
            DEPARTMENT_OF_COMMERCE,
            DEPARTMENT_OF_STATE,
            DEPARTMENT_OF_COMMERCE_AND_DEPARTMENT_OF_STATE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum B13AFilingOptionType
        {
            FILED_ELECTRONICALLY,
            MANUALLY_ATTACHED,
            NOT_REQUIRED,
            SUMMARY_REPORTING,
            FEDEX_TO_STAMP
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomsRole
        {
            EXPORTER,
            IMPORTER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SmartPostAncillaryEndorsementType
        {
            ADDRESS_CORRECTION,
            CARRIER_LEAVE_IF_NO_RESPONSE,
            CHANGE_SERVICE,
            FORWARDING_SERVICE,
            RETURN_SERVICE,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SmartPostIndiciaType
        {
            MEDIA_MAIL,
            PARCEL_RETURN,
            PARCEL_SELECT,
            PRESORTED_BOUND_PRINTED_MATTER,
            PRESORTED_STANDARD,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SmartPostShipmentSpecialServiceType
        {
            USPS_DELIVERY_CONFIRMATION
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LabelFormatType
        {
            COMMON2D,
            LABEL_DATA_ONLY
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LabelOrder
        {
            SHIPPING_LABEL_FIRST,
            SHIPPING_LABEL_LAST
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomerMaskedData
        {
            CUSTOMS_VALUE,
            DUTIES_AND_TAXES_PAYOR_ACCOUNT_NUMBER,
            INSURED_VALUE,
            SECONDARY_BARCODE,
            SHIPPER_INFORMATION,
            TERMS_AND_CONDITIONS,
            TOTAL_WEIGHT,
            TRANSPORTATION_CHARGES_PAYOR_ACCOUNT_NUMBER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum GenerationOptions
        {
            CONTENT_ON_SHIPPING_LABEL_PREFERRED,
            CONTENT_ON_SUPPLEMENTAL_LABEL_ONLY,
            CONTENT_ON_SHIPPING_LABEL_ONLY
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AdditionalLabelsType
        {
            BROKER,
            CONSIGNEE,
            CUSTOMS,
            DESTINATION,
            DESTINATION_CONTROL_STATEMENT,
            FREIGHT_REFERENCE,
            MANIFEST,
            ORIGIN,
            RECIPIENT,
            SECOND_ADDRESS,
            SHIPPER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocTabContentType
        {
            BARCODED,
            CUSTOM,
            MINIMUM,
            STANDARD,
            ZONE001,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocTabZoneJustificationType
        {
            LEFT,
            RIGHT,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BarcodeSymbologyType
        {
            CODABAR,
            CODE128,
            CODE128_WIDEBAR,
            CODE128B,
            CODE128C,
            CODE39,
            CODE93,
            I2OF5,
            MANUAL,
            PDF417,
            POSTNET,
            QR_CODE,
            UCC128
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LabelStockType
        {
            PAPER_4X6,
            PAPER_4X675,
            PAPER_4X8,
            PAPER_4X9,
            PAPER_7X475,
            PAPER_85X11_BOTTOM_HALF_LABEL,
            PAPER_LETTER,
            PAPER_85X11_TOP_HALF_LABEL,
            STOCK_4X6,
            STOCK_4X675_LEADING_DOC_TAB,
            STOCK_4X675_TRAILING_DOC_TAB,
            STOCK_4X8,
            STOCK_4X9_LEADING_DOC_TAB,
            STOCK_4X9_TRAILING_DOC_TAB,
            STOCK_4X9,
            STOCK_4X85_TRAILING_DOC_TAB,
            STOCK_4X105_TRAILING_DOC_TAB
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LabelRotation
        {
            LEFT,
            RIGHT,
            UPSIDE_DOWN,
            NONE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ImageType
        {
            EPL2,
            PDF,
            PNG,
            ZPLII
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LabelPrintingOrientation
        {
            BOTTOM_EDGE_OF_TEXT_FIRST,
            TOP_EDGE_OF_TEXT_FIRST
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Options
        {
            SHIPPING_LABEL_FIRST,
            SHIPPING_LABEL_LAST,
            SUPPRESS_ADDITIONAL_LANGUAGES
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ShippingDocumentDispositionType
        {
            CONFIRMED,
            DEFERRED_QUEUED,
            DEFERRED_STORED,
            DEFERRED_RETURNED,
            EMAILED,
            QUEUED,
            RETURNED,
            STORED
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocumentGrouping
        {
            BY_RECIPIENT,
            NONE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomerImageUsageId
        {
            IMAGE_1,
            IMAGE_2,
            IMAGE_3,
            IMAGE_4,
            IMAGE_5
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomerImageUsageType
        {
            LETTER_HEAD,
            SIGNATURE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CertifierSpecification
        {
            EXPORTER,
            IMPORTER,
            PRODUCER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ImporterSpecification
        {
            UNKNOWN,
            VARIOUS
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProducerSpecification
        {
            AVAILABLE_UPON_REQUEST,
            VARIOUS,
            SAME_AS_EXPORTER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RateRequestType
        {
            NONE,
            LIST,
            PREFERRED,
            ACCOUNT,
            INCENTIVE,
            RETAIL
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SignatureOptionType
        {
            ADULT,
            DIRECT,
            INDIRECT,
            NO_SIGNATURE_REQUIRED,
            SERVICE_DEFAULT
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AlcoholRecipientType
        {
            LICENSEE,
            CONSUMER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AccessibilityType
        {
            ACCESSIBLE,
            INACCESSIBLE
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DangerousGoodsOptions
        {
            HAZARDOUS_MATERIALS,
            BATTERY,
            ORM_D,
            REPORTABLE_QUANTITIES,
            SMALL_QUANTITY_EXCEPTION,
            LIMITED_QUANTITIES_COMMODITIES
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BatteryPackingType
        {
            CONTAINED_IN_EQUIPMENT,
            PACKED_WITH_EQUIPMENT
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BatteryMaterialType
        {
            LITHIUM_METAL,
            LITHIUM_ION
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LabelResponseOptions
        {
            URL_ONLY,
            LABEL
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ShipAction
        {
            CONFIRM,
            TRANSFER
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProcessingOptionType
        {
            SYNCHRONOUS_ONLY,
            ALLOW_ASYNCHRONOUS
        }
    }
}
