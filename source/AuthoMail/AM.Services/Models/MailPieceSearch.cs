using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.ComponentModel.DataAnnotations;
using AM.Entities;

namespace AM.Services
{

    public class MailPieces
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int? ClientId { get; set; }
        public int MailBatchId { get; set; }
        public int? PrintBatchId { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string FilePattern { get; set; }
        public string DocumentOrPackageType { get; set; }
        public int? SLADays { get; set; }
        public DateTime? DueDate { get; set; }
        public string ShippingStatus { get; set; }
        public bool IsRcptInfoSpecified { get; set; }
        public string RcptPrefix { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptSuffix { get; set; }
        public string RcptPhone { get; set; }
        public string RcptOrganization { get; set; }
        public string RcptEmailId { get; set; }

        public string RcptName
        {
            get
            {
                string nm = $"{this.RcptPrefix} {this.RcptFirstName} {this.RcptMiddleName} {this.RcptLastName} {this.RcptSuffix}";
                return nm.IsNullOrWhiteSpace() ? null : nm.Replace("  ", " ");
            }
        }

        public string CoRcptPrefix { get; set; }
        public string CoRcptFirstName { get; set; }
        public string CoRcptMiddleName { get; set; }
        public string CoRcptLastName { get; set; }
        public string CoRcptSuffix { get; set; }
        public string CoRcptName
        {
            get
            {
                string nm = string.Format("{0} {1} {2}", this.CoRcptFirstName, this.CoRcptMiddleName, this.CoRcptLastName);
                return nm.IsNullOrWhiteSpace() ? null : nm.Replace("  ", " ");
            }
        }
        public string CoRcptPhone { get; set; }
        public string CoRcptOrganization { get; set; }
        public string RcptAddrOptionalLine1AboveStreet { get; set; }
        public string RcptAddrOptionalLine2AboveStreet { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressAptOrSuiteOrOther { get; set; }
        public string RcptAddrOptionalLine1BelowStreet { get; set; }
        public string RcptAddrOptionalLine2BelowStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string RcptAddressCountry { get; set; }

        public string RcptAddress
        {
            get
            {
                string add = string.Format("{0} {1} {2} {3} {4} {5}", this.RcptAddrOptionalLine1AboveStreet, this.RcptAddrOptionalLine2AboveStreet, this.RcptAddressStreet, this.RcptAddressAptOrSuiteOrOther, this.RcptAddrOptionalLine1BelowStreet, this.RcptAddrOptionalLine2BelowStreet);
                return add;
            }
        }

        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }

        public string RetRcptName
        {
            get
            {
                string nm = string.Format("{0} {1} {2}", this.RetRcptFirstName, this.RetRcptMiddleName, this.RetRcptLastName);
                return nm.IsNullOrWhiteSpace() ? null : nm.Replace("  ", " ");
            }
        }

        public string RetRcptPhone { get; set; }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }

        public string RetRcptAddress
        {
            get
            {
                string add = string.Format("{0} {1} {2} {3} {4} {5}", this.RetRcptAddrOptionalLine1AboveStreet, this.RetRcptAddrOptionalLine2AboveStreet, this.RetRcptAddressStreet, this.RetRcptAddressAptOrSuiteOrOther, this.RetRcptAddrOptionalLine1BelowStreet, this.RetRcptAddrOptionalLine2BelowStreet);
                return add;
            }
        }

        public int TotalPages { get; set; }
        public int TotalSheets { get; set; }
        public int TotalBlankPages { get; set; }
        public string BlankPageNumbers { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string ErrorDescription
        {
            get
            {
                if (!ErrorDesc.IsNullOrWhiteSpace()) return ErrorDesc.Replace("\r", string.Empty).Replace("\n", string.Empty);
                return ErrorDesc;
            }
        }
        public string ErrorCodeDesc
        {
            get
            {
                return string.Format("{0}{1}", this.ErrorCode, this.ErrorDesc.IsNullOrWhiteSpace() ? null : (" <br/>  " + this.ErrorDesc));
            }
        }
        public string TrackingNo { get; set; }
        public int IsPullBackRequested { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public int IsPulledBack { get; set; }
        public DateTime? PulledBackOn { get; set; }
        public string PulledBackRequestedBy { get; set; }
        public int IsPrinted { get; set; }
        public DateTime? PrintedOn { get; set; }
        public int IsShipped { get; set; }
        public DateTime? ShippedOn { get; set; }
        public int IsPickedUp { get; set; }
        public DateTime? PickedUpOn { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal? Weight { get; set; }
        public bool IsQCRequested { get; set; }
        public bool IsQCPerformed { get; set; }
        public int? DocTypeId { get; set; }
        public int? PnSConfigDetailId { get; set; }
        public string PrintLocation { get; set; }
        public string Inserts { get; set; }
        public DateTime? StdSubmissionTime { get; set; }
        public DateTime? MaxSubmissionTime { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string MissedSLAReason { get; set; }
        public MailBatches MailBatches { get; set; }
        public Clients Clients { get; set; }
        public DocTypes DocTypes { get; set; }
        public Users Users { get; set; }
        public PnSConfigDetail PnSConfigDetails { get; set; }
        public string PdfReceivedPath
        {
            get
            {
                if (this.MRDFRequests == null || this.Clients == null) return null;

                return string.Format(@"ReturnPdfs\{0}\{1}\{2}-{3}-{4}_{5}", this.Clients.AppCode, this.MRDFRequests.JobId, this.MRDFRequests.JobId, this.MRDFRequests.PieceId, this.Id, this.FileName);
            }
        }
        public List<MailPieceCustomInfoTest> MailPieceCustomInfoTest { get; set; }
        public MRDFRequests MRDFRequests { get; set; }
        public int? PrintTrailId { get; set; }
        public string SLAStatus
        {
            get
            {
                return (this.ShippingStatus == AM.Entities.ShippingStatus.InProgress.ToString() || this.ShippingStatus == AM.Entities.ShippingStatus.Shipped.ToString()) ?
                    this.SLADays >= 0 ? this.ShippingStatus == AM.Entities.ShippingStatus.InProgress.ToString() ? this.ShippingStatus : "SLA Met" : "Missed SLA" : this.ShippingStatus;
            }
        }

        public string ShipmentStatus { get; set; }
        public DateTime? ShipmentStatusUpdatedOn { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; }
        public string EnvelopeName { get; set; }
        public string PrintFileName { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
    }
    public class MailBatches
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int? ClientId { get; set; }
        public int? SFTChannelId { get; set; }
        public string ClientBatchId { get; set; }
        public string BatchName { get; set; }
        public string DocumentOrPackageType { get; set; }
        public int IsReceived { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public int IsDownloaded { get; set; }
        public DateTime? DownloadedOn { get; set; }
        public int IsAcknowledged { get; set; }
        public DateTime? AcknowledgedOn { get; set; }
        public int IsValidated { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public int IsPrepped { get; set; }
        public DateTime? PreppedOn { get; set; }
        public int IsSubmittedToShipping { get; set; }
        public DateTime? SubmittedToShippingOn { get; set; }
        public int IsShippingResponseReceived { get; set; }
        public DateTime? ShippingResponseReceivedOn { get; set; }
        public int IsReportPrepped { get; set; }
        public DateTime? ReportPreppedOn { get; set; }
        public int IsReportedBack { get; set; }
        public DateTime? ReportedBackOn { get; set; }
        public string Priority { get; set; }
        public string BatchStatus { get; set; }
        public string ValidationStatus { get; set; }
        public string ValidationErrors { get; set; }
        public string PipelineStatus { get; set; }
        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRcptPhone { get; set; }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public SFTChannels SFTChannels { get; set; }
        public Clients Clients { get; set; }

    }
    public class Clients
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AppCode { get; set; }
        public int? BusinessUnit { get; set; }
        public bool IsAMActive { get; set; }
        public bool IsPnSActive { get; set; }
        public string TimeZone { get; set; }
        public string Address { get; set; }
        public string Contact1Name { get; set; }
        public string Contact1Phone { get; set; }
        public string Contact1Email { get; set; }
        public string Contact2Name { get; set; }
        public string Contact2Phone { get; set; }
        public string Contact2Email { get; set; }
        public string Contact3Name { get; set; }
        public string Contact3Phone { get; set; }
        public string Contact3Email { get; set; }
        public string ITContactName { get; set; }
        public string ITContactPhone { get; set; }
        public string ITContactEmail { get; set; }
        public string BillingContactName { get; set; }
        public string BillingContactPhone { get; set; }
        public string BillingContactEmail { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public BusinessUnitModel BusinessUnitModel { get; set; }
    }
    public class ClientDocTypes
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Client field is required.")]
        public int ClientId { get; set; }
        [Required(ErrorMessage = "Document type name is required.")]
        public string DocTypeName { get; set; }
        public List<DocTypes> DocTypesList { get; set; }
        public DocTypes DocTypes { get; set; }
        public ClientDocTypes()
        {
            this.DocTypes = new DocTypes();
        }
    }
    public class DocTypes
    {
        public int Id { get; set; }
        public string DocTypeCode { get; set; }//Removed
        public string DocTypeName { get; set; }//Removed
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        [Required]
        public string PrintSize { get; set; }
        public decimal ContentScalePercent { get; set; }
        public string PrintFormat { get; set; }
        public string PaperType { get; set; }
        public decimal QCPercent { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        [Required]
        public string ShippingAccountCode { get; set; }
        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRcptPhone { get; set; }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public string ClientEmails { get; set; }
        public string ClientContactNo { get; set; }
        public string SupportEmails { get; set; }
        public string SupportContactNo { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public Clients Client { get; set; }
        public string BizdayCutOff { get; set; }
        public string BizdaySLAType { get; set; }
        public string BizdayTimeSpan { get; set; }
        public string SatCutOff { get; set; }
        public string SatSLAType { get; set; }
        public string SatTimeSpan { get; set; }
        public string SunCutOff { get; set; }
        public string SunSLAType { get; set; }
        public string SunTimeSpan { get; set; }
        public string ShippingSpecialService { get; set; }
        public bool IsBRE { get; set; }
        public int? EnvelopeId { get; set; }
        public int? BREEnvelopeId { get; set; }
        public int NoOfCopies { get; set; }
        public int? MinPage { get; set; }
        public int? MaxPage { get; set; }
        public int ClientDocTypeId { get; set; }
        public int? PnSConfigId { get; set; }
        public int? PnSConfigDetailId { get; set; }
        public string ClientDocTypesName { get; set; }
        public bool IsFallBack { get; set; }
        public List<DocTypePageRange> DocTypePageRangeList { get; set; }
        public DocTypes()
        {
            this.DocTypePageRangeList = new List<DocTypePageRange>();
        }
    }

    public class PnSConfigDetail
    {
        public int Id { get; set; }
        public string PrintSize { get; set; }
        public decimal ContentScalePercent { get; set; }
        public string PrintFormat { get; set; }
        public decimal QCPercent { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string ShippingAccountCode { get; set; }
        public string BizdayCutOff { get; set; }
        public string BizdaySLAType { get; set; }
        public string BizdayTimeSpan { get; set; }
        public string SatCutOff { get; set; }
        public string SatSLAType { get; set; }
        public string SatTimeSpan { get; set; }
        public string SunCutOff { get; set; }
        public string SunSLAType { get; set; }
        public string SunTimeSpan { get; set; }
        public string ShippingSpecialService { get; set; }
        public int NoOfCopies { get; set; }
        public int? MinPage { get; set; }
        public int? MaxPage { get; set; }
        public string EnvelopeName { get; set; }
        public string PartNumber { get; set; }
        public string TrailName { get; set; }
        public string VendorCode { get; set; }
        public bool IsFallBack { get; set; }
        public bool IsPrintReady { get; set; }
        public TimeSpan? BizdayStartTime { get; set; }
        public TimeSpan? BizdayEndTime { get; set; }
        public TimeSpan? SatStartTime { get; set; }
        public TimeSpan? SatEndTime { get; set; }
        public TimeSpan? SunStartTime { get; set; }
        public TimeSpan? SunEndTime { get; set; }
    }
    public class DocTypePageRange
    {
        public int DocTypesId { get; set; }
        [Required, Range(1, 998, ErrorMessage = "From field value must be between 1 to 999.")]
        public int From { get; set; }
        [Required, Range(1, 999, ErrorMessage = "To field value must be between 1 to 999.")]
        public int To { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string ShippingSpecialService { get; set; }
        public List<string> ShippingSpecialServiceList { get; set; }
        public int? EnvelopeId { get; set; }
        public int? BREEnvelopeId { get; set; }
        public bool IsFallBack { get; set; }
        public bool IsBRE { get; set; }
        //public bool HasFB { get; set; }//isfallback ref for UI
        //public bool isAdd { get; set; }
    }
    public class SFTChannels
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ToADEDir { get; set; }
        public string FromADEDir { get; set; }
        public string ArchiveDir { get; set; }
        public string PostDownloadAction { get; set; }
        public bool IsActive { get; set; }
    }
    public class Users
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public int? ClientId { get; set; }
    }

    public class MailPieceCustomInfoTest
    {
        public int MailPieceId { get; set; }
        public string Key { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
    }

    public class PullBackRequests
    {
        public int Id { get; set; }
        public int? ClientId { get; set; }
        public int MailBatchId { get; set; }
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public string Status { get; set; }
        public DateTime RequestedOn { get; set; }
        public string RequestedBy { get; set; }
        public string PBReasons { get; set; }
        public MailPieces MailPieces { get; set; }
        public Users Users { get; set; }
    }
    public class BusinessUnitModel
    {
        public int Id { get; set; }
        public string BusinessUnit { get; set; }
        public string CustCode { get; set; }
        public bool IsActive { get; set; }
    }
    public class MRDFRequests
    {
        public Guid UniqueId { get; set; }
        public int JobId { get; set; }
        public int PieceId { get; set; }
        public DateTime Date { get; set; }
        public int? TotalSheets { get; set; }
        public string InputWeight { get; set; }
        public string Feeder3_Sel { get; set; }
        public string Feeder4_Sel { get; set; }
        public string Feeder5_Sel { get; set; }
        public string LowCap_Sel { get; set; }
        public string NoSeal_Sel { get; set; }
        public string Feeder2_Sel { get; set; }
        public string ZipMark_Sel { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Zip5 { get; set; }
        public string Zip4 { get; set; }
        public string Zip2 { get; set; }
        public string DivertVS2 { get; set; }
        public string DivertVS3 { get; set; }
        public string DivertVS4 { get; set; }
        public string ReturnLogoBitMap { get; set; }
        public string ReturnLine1 { get; set; }
        public string ReturnLine2 { get; set; }
        public string ReturnLine3 { get; set; }
        public string ReturnLine4 { get; set; }
        public string ReturnLine5 { get; set; }
        public string ReturnLine6 { get; set; }
        public string GroupNumber { get; set; }
        public string SocialSecNumber { get; set; }
        public string CheckAmount { get; set; }
        public string PassThru { get; set; }
        public string Expansion { get; set; }
        public string EndofRecord { get; set; }
        public string PDFFileName { get; set; }
        public int? ErrId { get; set; }
        public string ErrDesc { get; set; }
        public string MailPieceStatus { get; set; }
        public bool IsPDFReceived { get; set; }
        public string PrintFileName { get; set; }
    }

    public class SearchViewModel
    {
        private readonly DateTime CurrentDateTime = Common.Now;
        //public DateTime StartDate { get; set; }
        public int MailPieceId { get; set; }
        public int MailBatchId { get; set; }
        public string LoanNumber { get; set; }
        public string Status { get; set; }
        public Guid UniqueId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RcptAddress { get; set; }
        public string City { get; set; }
        public string ZIP { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public DateTime? EsignedOn { get; set; }
        public DateTime? PickedUpOn { get; set; }
        public string TrackingNo { get; set; }
        public string DocType { get; set; }
        public bool IsEDeliveryEnabled { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Channel { get; set; }
        public string ImpressionCount { get; set; }
        public int TotalSheets { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingCarrier { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string PrintLocation { get; set; }
        public string ShipmentStatus { get; set; }
        public DateTime? ShipmentStatusUpdatedOn { get; set; }
        public string Inserts { get; set; }
        public string PrintFileName { get; set; }
        public string EnvelopeName { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; }
        public int JobId { get; set; }
        public string AppCode { get; set; }
        public string PDFFileName { get; set; }
        public int RetentionPeriod { get; set; }
        public int ClientId { get; set; }
        public string RequestedPdf
        {
            get
            {
                if (this.PDFFileName.IsNullOrWhiteSpace()) return null;

                return $@"{Config.PnS_PCIRootFolder}/{this.AppCode}/{this.JobId}/PCIRequest/{PDFFileName}";
            }
        }
        public bool IsWithinRetentionPeriod { get; set; }
        public string ServerIP { get; set; }
    }


    public class MonthlyReconReportViewModel
    {
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public Guid UniqueId { get; set; }
        public string LoanNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }
        public string Channel { get; set; }
        public string DocType { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public DateTime? PulledBackOn { get; set; }
        public string PrintLocation { get; set; }
        public string Status { get; set; }
        public string TrackingNo { get; set; }
        public string FileName { get; set; }
        public int TotalPages { get; set; }
        public int TotalSheets { get; set; }
        public int LetterPageCount { get; set; }
        public int LegalPageCount { get; set; }
        public int TotalBlankPages { get; set; }
        public string ImpressionCount { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public DateTime? ESignedOn { get { if (this.Status == ShippingStatus.eSigned.ToString() && ShippedOn.HasValue) return ShippedOn; return null; } }
        public string ShippingMethod { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string EnvelopeName { get; set; }
        public string ErrorDesc { get; set; }
        public string Inserts { get; set; }
        public string PrintBatchName { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; }
        public decimal ShippingCost { get; set; }
        public string LetterPageNumbers { get; set; }
        public string LegalPageNumbers { get; set; }
        public string BlankPageNumbers { get; set; }
        public List<string> BlankPageNumbersList
        {
            get
            {
                if (!BlankPageNumbers.IsNullOrWhiteSpace())
                {
                    return BlankPageNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                return null;
            }
        }
        public List<string> BlankLetterPages
        {
            get
            {
                if (BlankPageNumbersList != null)
                {
                    if (!LetterPageNumbers.IsNullOrWhiteSpace())
                    {
                        var letterPages = LetterPageNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        return BlankPageNumbersList.Intersect(letterPages).ToList();
                    }
                }
                return null;
            }
        }
        public List<string> BlankLegalPages
        {
            get
            {
                if (BlankPageNumbersList != null)
                {
                    if (!LegalPageNumbers.IsNullOrWhiteSpace())
                    {
                        var legalPages = LegalPageNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        return BlankPageNumbersList.Intersect(legalPages).ToList();
                    }
                }
                return null;
            }
        }

        public int LetterPagesImpression
        {
            get
            {
                return LetterPageCount - (BlankLetterPages != null ? BlankLetterPages.Count : 0);
            }
        }
        public int LegalPagesImpression
        {
            get
            {
                return LegalPageCount - (BlankLegalPages != null ? BlankLegalPages.Count : 0);
            }
        }
    }

}
