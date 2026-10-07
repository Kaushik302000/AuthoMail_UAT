using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.IO;
namespace AM.Entities
{
    public static class BatchValidationStatus
    {
        public const string Valid = "Valid";
        public const string PartiallyValid = "PartiallyValid";
        public const string Invalid = "Invalid";
    }

    public static class ChannelType
    {
        public const string Sweep = "Sweep";
        public const string Trigger = "Trigger";
    }

    public enum BatchStatus
    {
        InProgress,
        Failed,
        ShippedPartial,
        ShippedAll,
        PulledBack,
        eDelivered,
        eSigned
    }

    public static class BatchPipelineStatus
    {
        public const string Received = "Received";
        public const string Downloaded = "Downloaded";
        public const string SubmittedToEDelivery = "SubmittedToEDelivery";
        public const string Validated = "Validated";
        public const string Acknowledged = "Acknowledged";
        public const string Prepped = "Prepped";
        public const string SubmittedToShipping = "SubmittedToShipping";
        public const string ShippingResponseReceived = "ShippingResponseReceived";
        public const string ReportPrepped = "ReportPrepped";
        public const string ReportedBack = "ReportedBack";
        public const string Suspended = "Suspended";
        public const string Cancelled = "Cancelled";
        public const string Held = "Held";
    }

    public static class EDPipelineStatus
    {
        public const string Received = "Received";
        public const string Downloaded = "Downloaded";
        public const string Validated = "Validated";
        public const string SigningRoomCreated = "SigningRoomCreated";
        public const string InvitationSent = "InvitationSent";
        public const string eDelivered = "eDelivered";
        public const string eSigned = "eSigned";
        public const string Expired = "Expired";
        public const string Declined = "Declined";
        public const string ReportedBack = "ReportedBack";
    }

    public static class ActivityStatus
    {
        // Activity Status should match exactly with the DocMagic statuses
        public const string ESignConsentAccepted = "ESIGN Consent Accepted";
        public const string ESignConsentDeclined = "ESIGN Consent Declined";
        public const string DocumentReviewed = "Document Reviewed";
        public const string DocumentSigned = "Document Signed";
        public const string DocumentDownloaded = "Document Downloaded";
    }


    public static class OutComeCodes
    {
        public const string No_Error = "No_Error";
        public const string Missing_Zip_File = "Missing_Zip_File";
        public const string Invalid_Zip_File = "Invalid_Zip_File";
        public const string Invalid_XML_File = "Invalid_XML_File";
        public const string Missing_XML_File = "Missing_XML_File";
        public const string Invalid_PDF_File = "Invalid_PDF_File";
        public const string Missing_PDF_File = "Missing_PDF_File";
        public const string Mailing_Address_Missing_Or_Invalid = "Mailing_Address_Missing_Or_Invalid";
        public const string Property_Address_Missing_Or_Invalid = "Property_Address_Missing_Or_Invalid";
        public const string Recipient_Details_Missing_Or_Invalid = "Recipient_Details_Missing_Or_Invalid";
        public const string Unknown_Error = "Unknown_Error";
        public const string Invalid_PackageType = "Invalid_PackageType";
        public const string DuplicatePackage = "DuplicatePackage";
        public const string Invalid_ShippingDetail = "Invalid_ShippingDetail";
        public const string Invalid_DuplicateKey = "Invalid_DuplicateKey";
        public const string Issue_In_EDelivery = "Issue_In_EDelivery";
        public const string SigningRoomExpired = "SigningRoomExpired";
        public const string Missing_DocumentDate = "Missing_DocumentDate";
        public const string Missing_NoticeDate = "Missing_NoticeDate";
        public const string Missing_CommunityName = "Missing_CommunityName";
        public const string ConsentDeclined = "ConsentDeclined";
        public const string Manifest_Missing_Or_Invalid = "Manifest_Missing_Or_Invalid";
        public const string Invalid_BusinessIdentifier = "Invalid_BusinessIdentifier";
        public const string Invalid_ClientCode = "Invalid_ClientCode";
        public const string Missing_DocumentOrPackage = "Missing_DocumentOrPackage";
        public const string Missing_Files = "Missing_Files";
        public const string RequestTooLarge = "RequestTooLarge";
        public const string Invalid_MailPieceId = "Invalid_MailPieceId";
        public const string NotAcceptable = "NotAcceptable";
        public const string MaxRetriesExceeded = "MaxRetriesExceeded";
        public static string GetAMCMappingCode(string amOutcomecode)
        {
            switch (amOutcomecode)
            {
                case OutComeCodes.No_Error:
                    return AMCOutComeCodes.No_Error.ToString();

                case OutComeCodes.Missing_Zip_File:
                    return AMCOutComeCodes.Invalid_Zip_File.ToString();

                case OutComeCodes.Invalid_Zip_File:
                    return AMCOutComeCodes.Invalid_Zip_File.ToString();

                case OutComeCodes.Invalid_XML_File:
                    return AMCOutComeCodes.Invalid_XML_File.ToString();

                case OutComeCodes.Missing_XML_File:
                    return AMCOutComeCodes.Invalid_XML_File.ToString();

                case OutComeCodes.Invalid_PDF_File:
                    return AMCOutComeCodes.Invalid_PDF_File.ToString();

                case OutComeCodes.Missing_PDF_File:
                    return AMCOutComeCodes.Invalid_PDF_File.ToString();

                case OutComeCodes.Mailing_Address_Missing_Or_Invalid:
                    return AMCOutComeCodes.Mailing_address_missing_or_invalid.ToString();

                case OutComeCodes.Recipient_Details_Missing_Or_Invalid:
                    return AMCOutComeCodes.Recipient_name_missing_or_invalid.ToString();

                case OutComeCodes.Invalid_PackageType:
                    return AMCOutComeCodes.Invalid_shipping_detail.ToString();

                case OutComeCodes.Invalid_ShippingDetail:
                    return AMCOutComeCodes.Invalid_shipping_detail.ToString();

                case OutComeCodes.Invalid_DuplicateKey:
                    return AMCOutComeCodes.Unknown_Error.ToString();

                case OutComeCodes.Unknown_Error:
                default:
                    return AMCOutComeCodes.Unknown_Error.ToString();
            }
        }

        public static string GetAlaskaMappingCode(string alaskaOutcomeCode)
        {
            switch (alaskaOutcomeCode)
            {
                case OutComeCodes.No_Error:
                    return AlaskaOutComeCodes.No_error.ToString();

                case OutComeCodes.Invalid_XML_File:
                    return AlaskaOutComeCodes.Invalid_XML_file.ToString();

                case OutComeCodes.Missing_XML_File:
                    return AlaskaOutComeCodes.Invalid_XML_file.ToString();

                case OutComeCodes.Missing_PDF_File:
                    return AlaskaOutComeCodes.Invalid_PDF_file.ToString();

                case OutComeCodes.Mailing_Address_Missing_Or_Invalid:
                    return AlaskaOutComeCodes.Mailing_address_missing_or_invalid.ToString();

                case OutComeCodes.Recipient_Details_Missing_Or_Invalid:
                    return AlaskaOutComeCodes.Recipient_name_missing_or_invalid.ToString();

                case OutComeCodes.Unknown_Error:
                default:
                    return AlaskaOutComeCodes.UNKNOWN_error.ToString();
            }
        }

        public static string GetHSBCMappingCode(string hsbcOutcomeCode)
        {
            switch (hsbcOutcomeCode)
            {
                case OutComeCodes.No_Error:
                    return HSBCOutComeCodes.No_error.ToString();

                case OutComeCodes.Invalid_XML_File:
                    return HSBCOutComeCodes.Invalid_XML_file.ToString();

                case OutComeCodes.Missing_XML_File:
                    return HSBCOutComeCodes.Invalid_XML_file.ToString();

                case OutComeCodes.Missing_PDF_File:
                    return HSBCOutComeCodes.Invalid_PDF_file.ToString();

                case OutComeCodes.Mailing_Address_Missing_Or_Invalid:
                    return HSBCOutComeCodes.Mailing_address_missing_or_invalid.ToString();

                case OutComeCodes.Recipient_Details_Missing_Or_Invalid:
                    return HSBCOutComeCodes.Recipient_name_missing_or_invalid.ToString();

                case OutComeCodes.Invalid_PackageType:
                    return HSBCOutComeCodes.PackageName_missing_or_invalid.ToString();

                case OutComeCodes.Unknown_Error:
                default:
                    return HSBCOutComeCodes.UNKNOWN_error.ToString();
            }
        }
    }

    public enum AMCOutComeCodes
    {
        No_Error,
        Invalid_XML_File,
        Invalid_PDF_File,   // If PDF file is missing or unreadable then invalid PDF.
        Invalid_Zip_File,
        Mailing_address_missing_or_invalid,
        Recipient_name_missing_or_invalid,
        Unknown_Error,
        Invalid_shipping_detail
    }

    public enum AlaskaOutComeCodes
    {
        No_error,
        Invalid_XML_file,
        Invalid_PDF_file,
        Mailing_address_missing_or_invalid,
        Recipient_name_missing_or_invalid,
        UNKNOWN_error,
    }

    public enum HSBCOutComeCodes
    {
        No_error,
        Invalid_XML_file,
        Invalid_PDF_file, // If PDF file is missing or unreadable then invalid PDF.
        Mailing_address_missing_or_invalid,
        Recipient_name_missing_or_invalid,
        UNKNOWN_error,
        PackageName_missing_or_invalid,
    }

    public enum ShippingStatus
    {
        InProgress,
        PulledBack,
        Error,
        Shipped,
        eDelivered,
        eSigned,
        Unknown,
        Acknowledgement,
        NotFound
    }

    public static class PostDownloadAction
    {
        public const string Delete = "Delete";
        public const string Archive = "Archive";
    }

    public static class MissedSLAReason
    {
        public const string NA = "NA";
        public const string AM = "AM";
        public const string PCI = "PCI";
        public const string DALLAS_CL = "DALLAS_CL";
        public const string NORDIS = "NORDIS";
    }

    public static class ReportTypes
    {
        public const string Error = "Error";
        public const string Delivery = "Delivery";
        public const string Immediate = "Immediate";
        public const string Consolidated = "Consolidated";
        public const string NoBatch = "NoBatch";
    }

    public static class AMLogBatchTimeLine
    {
        public const string DownloadStart = "DownloadStart";
        public const string DownloadEnd = "DownloadEnd";
        public const string ValidationStart = "ValidationStart";
        public const string ValidationEnd = "ValidationEnd";
        public const string AcknowledgeStart = "AcknowledgeStart";
        public const string AcknowledgeEnd = "AcknowledgeEnd";
        public const string PreppedStart = "PreppedStart";
        public const string PreppedEnd = "PreppedEnd";
        public const string SubmittedToShippingStart = "SubmittedToShippingStart";
        public const string SubmittedToShippingEnd = "SubmittedToShippingEnd";
        public const string ShippingResponseReceivedStart = "ShippingResponseReceivedStart";
        public const string ShippingResponseReceivedEnd = "ShippingResponseReceivedEnd";
        public const string ReportPrepStart = "ReportPrepStart";
        public const string ReportPrepEnd = "ReportPrepEnd";
        public const string ReportBackStart = "ReportBackStart";
        public const string ReportBackEnd = "ReportBackEnd";
    }

    public enum ErrorTypes
    {
        IO = -1,
        CannotResize = -2,
        PageCountMismatch = -3,
        Reprint = 7
    }
    public static class AMRoles
    {
        public const string Admin = "Admin";
        public const string BA = "BA";
        public const string Client = "Client";
        public const string PnSOperator = "PnSOperator";
        public const string ClientUserManager = "ClientUserManager";
        public const string CoreLogicUserManager = "CoreLogicUserManager";
        public const string API = "API";
        public const string APIUSPSeVS = "API-USPS-eVS";
        public const string APIUSPSVerify = "API-USPS-Verify";
        public const string Delfus = "Delfus";
        public const string BAOffShore = "BA-OffShore";
        public const string EVault = "E-Vault";
    }
    public static class AMUserStatus
    {
        public const string Open = "Open";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string InActive = "InActive";
    }
    public enum ShippingServiceType
    {
        IMB = 0,
        FEDEX_2_DAY = 1,
        FEDEX_EXPRESS_SAVER = 2,
        FEDEX_GROUND = 3,
        FIRST_OVERNIGHT = 4,
        GROUND_HOME_DELIVERY = 5,
        PRIORITY_OVERNIGHT = 6,
        STANDARD_OVERNIGHT = 7,
        USPS_EXPRESSMAIL = 8,
        PRIORITY_EXPRESS = 9,
        PRIORITY = 10,
        FIRSTCLASS = 11
    }

    public static class SLAType
    {
        public const string FixedTime = "FixedTime";
        public const string Duration = "Duration";
        public const string NA = "NA";
    }

    public class MailBatchInfo
    {
        public string BatchRepo { get; private set; }
        public string AMRequestPath { get; private set; }
        public string PnSRequestPath { get; private set; }
        public string PnSResponsePath { get; private set; }
        public string AMResponsePath { get; private set; }
        public string BatchName { get; private set; }
        public MailBatchDTO MailBatch { get; private set; }
        public Client Client { get; private set; }

        public MailBatchInfo(MailBatchDTO mailBatch, Client client, string amRootFolder)
        {
            if (client == null || mailBatch == null) throw new ArgumentNullException("Client or MailBatch cannot be null.");

            MailBatch = mailBatch;
            Client = client;

            Initialize(client, mailBatch, amRootFolder);
        }

        private void Initialize(Client client, MailBatchDTO mailBatch, string amRootFolder)
        {
            BatchName = mailBatch.BatchName.Trim();
            //Combined path
            BatchRepo = Path.Combine(amRootFolder, client.AppCode, "{0}".Fill(mailBatch.Id));

            AMRequestPath = Path.Combine(BatchRepo, "AMRequest");
            PnSRequestPath = Path.Combine(BatchRepo, "PnSRequest");
            PnSResponsePath = Path.Combine(BatchRepo, "PnSResponse");
            AMResponsePath = Path.Combine(BatchRepo, Util.AMResponseFolderName);
        }
    }
}
