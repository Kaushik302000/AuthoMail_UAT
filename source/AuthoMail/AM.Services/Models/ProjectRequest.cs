using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ProjectRequest
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime? UATStartOn { get; set; }
        public DateTime? ProdStartOn { get; set; }
        public string Description { get; set; }
        public string FileTransmittedVia { get; set; }
        public List<string> FileTransmittedViaList { get; set; }//
        public string NoOfEstSweepTimes { get; set; }
        public string ESTSweepTime { get; set; }
        public string TypeOfFiles { get; set; }
        public List<string> TypeOfFilesList { get; set; }//
        public string TransmittedTime { get; set; }
        public List<string> TransmittedTimeList { get; set; }//
        public string ServiceLevel { get; set; }
        public List<string> ServiceLevelList { get; set; }//
        public int? EstimatedPagesPerPackage { get; set; }
        public int? EstimatedPackagesDaily { get; set; }
        public int? EstimatedPackagesMonthly { get; set; }
        public int? EstimatedPackagesAnnually { get; set; }
        public string DailyPrintSchedule { get; set; }
        public List<string> DailyPrintScheduleList { get; set; }
        public string PaperPrintRequirements { get; set; }
        public List<string> PaperPrintRequirementsList { get; set; }//
        public string PackagesSentToCustomer { get; set; }
        public List<string> PackagesSentToCustomerList { get; set; }//
        public bool IsSampleEnvelopeAttached { get; set; }
        public string Envelopes { get; set; }
        public List<string> EnvelopesList { get; set; }//
        public bool IsBRERequired { get; set; }
        public string BusinessReplyMailEnvelope { get; set; }
        public List<string> BusinessReplyMailEnvelopeList { get; set; }//
        public bool IsCustomerSupplyEnvelope { get; set; }
        public bool IsCustomerProvideArtworkEnvelope { get; set; }
        public bool IsClientPostageAccount { get; set; }
        public bool IsScanInAuthoMailDallas { get; set; }

        public string FilesTransmittedOther { get; set; }
        public string TypeOfFilesOther { get; set; }
        public string PaperAndPrintDetails { get; set; }
        public string ShippingMailingDetail { get; set; }
        public string MailingEnvelopeDetail { get; set; }
        public string BusinessReplyMailEnvelopeDetail { get; set; }
        public string ListOfDocTypesAndPages { get; set; }
        public string PDFSample { get; set; }
        public List<string> PDFSampleList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.PDFSample)) return null;
                return this.PDFSample.Split(',').ToList();
            }
        }
        public bool InsertRequired { get; set; }
        public bool BookletRequired { get; set; }
        public bool BusinessReplyMailEnvelopeRequired { get; set; }
        public bool OtherMaterialRequired { get; set; }
        public string InsertMaterialOrderedBy { get; set; }

        public string AdditionalInfo { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }

        public string ServiceLevelOther { get; set; }
        public string PaperPrintRequirementsOther { get; set; }
        public string PackagesSentToCustomerOther { get; set; }
        public string EnvelopesOther { get; set; }
        public string BusinessReplyMailEnvelopeOther { get; set; }
        public string FilesTransmittedDetail { get; set; }
        public string TypeOfFilesDetail { get; set; }
    }
    //public class ProjectRequestFormHeaders
    //{
    //    public string HeaderCode { get; set; }
    //    public string HeaderTitle { get; set; }
    //    public string AlphaSeqCode { get; set; }
    //}
    //public class ProjectRequestLookupForm
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public string FieldName { get; set; }
    //    public string DisplayHeader
    //    {
    //        get
    //        {
    //            if (ProjectRequestFormHeaders == null) return null;
    //            return string.Format("{0}. {1}", this.ProjectRequestFormHeaders.AlphaSeqCode, this.ProjectRequestFormHeaders.HeaderTitle);
    //        }
    //    }
    //    public ProjectRequestFormHeaders ProjectRequestFormHeaders { get; set; }
    //    public List<ProjectRequestLookupFormValue> ProjectRequestLookupFormValueList { get; set; }
    //}
    //public class ProjectRequestLookupFormValue
    //{
    //    public int Id { get; set; }
    //    public string Title { get; set; }
    //    public int ProjectRequestLookupFormId { get; set; }
    //}
    //public class ProjectRequestValueMapping
    //{
    //    public int ProjectRequestId { get; set; }
    //    public int ProjectRequestLookupFormValueId { get; set; }
    //    public string Value { get; set; }
    //}

    public class Envelopes
    {
        public int Id { get; set; }
        public string EnvelopeName { get; set; }
        public int CurrentQuantity { get; set; }
        public bool IsBREEnvelope { get; set; }
        public bool IsTrackingRequired { get; set; }
        public int? ReorderDuration { get; set; }
        public string PartNumber { get; set; }
        public string EnvelopeDesc { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int TotalDays { get; set; }
        public int TotalQuantityUsed { get; set; }
        public float AvgRate { get; set; }
        public float EnvDays { get; set; }
        public int AlertLevel
        {
            get
            {
                if (this.AvgRate == 0) return 0;//If AvgRate is zero then no need to calculate AlertLevel
                int minBufferDays = ((this.ReorderDuration.HasValue ? this.ReorderDuration.Value : 0) + Config.Common_InventoryBufferStockDays);
                int warningBufferDays = minBufferDays + 3;
                if (this.EnvDays > warningBufferDays) return 0;
                if (this.EnvDays > minBufferDays) return 1;
                return 2;
            }
        }
        public int Order
        {
            get
            {
                if (this.AlertLevel == 2) return 0;
                if (this.AlertLevel == 1) return 1;
                if (this.AlertLevel == 0 && this.EnvDays > 0) return 3;
                return 4;
            }
        }
    }
    public class EnvelopeInventory
    {
        public int Id { get; set; }

        public int EnvelopeId { get; set; }
        public string EnvelopeName { get; set; }

        public int GetEnvelopeId()
        {
            return EnvelopeId;
        }

        public string Description { get; set; }
        public DateTime ReceivedOn { get; set; }
        public int ReceivedQuantity { get; set; }
        public int ReorderDuration { get; set; }
        public decimal UnitPrice { get; set; }
        public Envelopes Envelopes { get; set; }
        public int StockUsed { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int ShipperId { get; set; }
        public string Shipper { get; set; }
    }
    public class EnvelopeInventoryStockCalculation
    {
        public int EnvelopeId { get; set; }
        public string EnvelopeName { get; set; }
        public int ShipperId { get; set; }
        public string ShipperName { get; set; }
        public int TotalDays { get; set; }
        public int TotalQuantityUsed { get; set; }
        public float AvgRate { get; set; }
        public float EnvDays { get; set; }
        public int? CurrentQuantity { get; set; }
        public int? ReorderDuration { get; set; }
        public int AlertLevel
        {
            get
            {
                if (this.AvgRate == 0) return 0;//If AvgRate is zero then no need to calculate AlertLevel
                int minBufferDays = ((this.ReorderDuration.HasValue ? this.ReorderDuration.Value : 0) + Config.Common_InventoryBufferStockDays);
                int warningBufferDays = minBufferDays + 3;
                if (this.EnvDays > warningBufferDays) return 0;
                if (this.EnvDays > minBufferDays) return 1;
                return 2;
            }
        }
    }
    public class ShipperInfoSelectListVM
    {
        public int Id { get; set; }
        public int PrintVendorId { get; set; }
        public string PrintVendor { get; set; }
        public string PrintLocation { get; set; }
        public string PrintVendorCode { get; set; }
        public string Name
        {
            get
            {
                return $"{PrintLocation} ( {PrintVendorCode} )";
            }
        }
    }
    //public class EnvelopeIdCQuantity
    //{
    //    public string Id { get; set; }
    //    public int? CurrentQuantity { get; set; }
    //    public int? ReorderDuration { get; set; }
    //    public float AvgRate { get; set; }
    //    public float EnvDays
    //    {
    //        get
    //        {
    //            return float.Parse(this.CurrentQuantity.ToString()) / this.AvgRate == 0f ? 1 : this.AvgRate;
    //        }
    //    }
    //    public int AlertLevel
    //    {
    //        get
    //        {
    //            int minBufferDays = this.ReorderDuration.HasValue ? this.ReorderDuration.Value : 0 + Config.Common_InventoryBufferStockDays;
    //            int warningBufferDays = minBufferDays + 3;
    //            if (this.EnvDays > warningBufferDays) return 0;
    //            if (this.EnvDays > minBufferDays) return 1;
    //            return 2;
    //        }
    //    }
    //}

    public class EnvelopesVM : Envelopes
    {
        public List<EnvelopeStockVM> EnvelopeStockVMs { get; set; }
        public List<EnvelopeStockVM> EnvelopeStockVMUs { get; set; }
        public EnvelopesVM()
        {
            this.EnvelopeStockVMs = new List<EnvelopeStockVM>();
        }
    }
    public class EnvelopeStockVM
    {
        public int? EnvelopeId { get; set; }
        public int ShipperId { get; set; }
        public string ShipperName { get; set; }
        public string Description { get; set; }
        public int? ReceivedQuantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? ReorderDuration { get; set; }
        public int CurrentQuantity
        {
            get
            {
                return ReceivedQuantity.HasValue ? ReceivedQuantity.Value : 0;
            }
        }


    }

    public class EnvelopesListVM
    {
        public int Id { get; set; }
        public string EnvelopeName { get; set; }
        public int CurrentQuantity { get; set; }
        public bool IsBREEnvelope { get; set; }
        public bool IsTrackingRequired { get; set; }
        public int? ReorderDuration { get; set; }
        public string PartNumber { get; set; }
        public string EnvelopeDesc { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int AlertLevel { get; set; }
        public int Order { get; set; }
    }
    public class EnvelopeTransferInventory
    {
        public int EnvelopeId { get; set; }
        public int FromShipperId { get; set; }
        public int ToShipperId { get; set; }
        public int TransferQuantity { get; set; }
        public int TransferQty
        {
            get
            {
                return -TransferQuantity;
            }
        }
        public string Description { get; set; }
        public DateTime? TransferredOn { get; set; }
        public string TransferredBy { get; set; }

    }
    public class EnvelopeInventoryUsage
    {
        public int EnvelopeId { get; set; }
        public int PrintVendorId { get; set; }
        public string EnvelopeName { get; set; }
        public string PrintLocation { get; set; }
        public string PrintVendorName { get; set; }
        public string PrintVendorCode { get; set; }
        public DateTime SubmittedOn { get; set; }
        public int TotalQuantityUsed { get; set; }
        public int CurrentQuantity { get; set; }
        public int ReOrderDuration { get; set; }
    }
}
