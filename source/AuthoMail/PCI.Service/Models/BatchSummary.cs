using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCI.Lib;
using System.IO;

namespace PCI.Service
{
    public class BatchSummary
    {
        public int PrintBatch { get; set; }
        public int ClientId { get; set; }
        public string JobName { get; set; }
        public string PipelineStatus { get; set; }
        public string ValidationStatus { get; set; }
        public string OnValidationErrors { get; set; }
        public string ValidationErrors { get; set; }
        public bool IsReceived { get; set; }
        public DateTime ReceivedOn { get; set; }
        public bool IsDownloaded { get; set; }
        public DateTime DownloadedOn { get; set; }
        public bool IsPrepped { get; set; }
        public DateTime PreppedOn { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime SubmittedOn { get; set; }
        public bool IsMRDFReceived { get; set; }
        public DateTime MRDFReceivedOn { get; set; }
        public bool IsReportPrepped { get; set; }
        public DateTime ReportPreppedOn { get; set; }
        public bool IsReportedBack { get; set; }
        public DateTime ReportedBackOn { get; set; }
        public int JulianDate { get; set; }
        public int Images { get; set; }
        public DateTime DueDate { get; set; }
        public int SLADays { get; set; }
        public string BatchStatus { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime SuspendedOn { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CancelledOn { get; set; }
        public Guid BatchIdentifier { get; set; }
        public int? DocTypeId { get; set; }
        public int? PnSConfigDetailId { get; set; }
        public string PrintFileName { get; set; }
        public string PrintLocation { get; set; }
        public int? PrintTrailId { get; set; }
    }

    public class BatchInfo
    {
        public string BatchRepo { get; private set; }
        public string BatchZipFilePath { get; private set; }
        public string PnSRequestPath { get; private set; }
        public string PCIRequestPath { get; private set; }
        public string PCIResponsePath { get; private set; }
        //public string PCIResponseReturnPDFsPath { get; private set; }
        public string PnSResponsePath { get; private set; }
        public string PnSRequestPDFsPath { get; private set; }
        public string BatchName { get; private set; }
        public string RequestXmlPath { get; private set; }

        public string SummaryPath { get; private set; }
        public string RequestMRDFPath { get; private set; }
        public string ResponseMRDFPath { get; private set; }
        public string MergedPDFPath { get; private set; }

        public BatchSummary BatchSummary { get; private set; }
        public Client Client { get; private set; }

        public BatchInfo(BatchSummary batchSummary, Client client)
        {
            if (batchSummary == null || client == null) throw new ArgumentNullException("Batch summary or client cannot be null.");

            BatchSummary = batchSummary;
            Client = client;

            Initialize(batchSummary, client);
        }

        private void Initialize(BatchSummary batchSummary, Client client)
        {
            // TODO: Use the same logic to retrieve app code folder
            //"XYZ-XYZ-123".Substring("".Length, )

            BatchName = batchSummary.JobName.Substring((client.AppCode.Length + 1),
                (batchSummary.JobName.Length - (client.AppCode.Length + 1)));

            //Combined path
            BatchRepo = Path.Combine(Common.PCIRootFolder, client.AppCode, "{0}".Fill(batchSummary.PrintBatch));

            PnSRequestPath = Path.Combine(BatchRepo, Common.PnSRequestFolderName);
            PCIRequestPath = Path.Combine(BatchRepo, Common.PCIRequestFolderName);
            PCIResponsePath = Path.Combine(BatchRepo, Common.PCIResponseFolderName);
            PnSResponsePath = Path.Combine(BatchRepo, Common.PnSResponseFolderName);


            RequestXmlPath = Path.Combine(PnSRequestPath, "{0}.xml".Fill(BatchName));

            PnSRequestPDFsPath = Path.Combine(PnSRequestPath, BatchName);

            //Used for storing PCI returned pdf to pns shared path for showing that pdfs to AuthoMail website
            //PCIResponseReturnPDFsPath = Path.Combine(Common.PnS_SFT_RootDir, "PCIReturnedPDFs", client.AppCode, batchSummary.PrintBatch.ToString());
        }
    }

    public class SubmittedBatches
    {
        public int PrintTrailBatchId { get; set; }
        public string PrintBatchName { get; set; }
        public string PrintVendorCode { get; set; }
        public List<string> ReceivedFileList { get; set; }
    }

    public class MailPieceDetail : MRDFRequest
    {
        public int ClientId { get; set; }
        public string AppCode { get; set; }
        public string Alias { get; set; }
        public int DocTypeId { get; set; }
        public int? MailPieceId { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public DateTime? EffectiveReceivedOnEst { get; set; }
        public int SequenceNo { get; set; }
        public DateTime MailPieceDueDate { get; set; }
        public int? EnvelopeId { get; set; }
        public string EnvelopeName { get; set; }
        public string EnvelopePartNo { get; set; }
        public string EnvelopeDesc { get; set; }
        public string PrintSize { get; set; }
        public string PrintFormat { get; set; }
        public int ShipperId { get; set; }
        public string Inserts { get; set; }
        public string InsertsPartNos { get; set; }
        public string InsertsIds { get; set; }
        public string DallasBatchName { get; set; }
        //Extra fields which will be used for generating MRDF Request
        public string InputWeight { get; set; }
        public string Feeder3_Sel { get; set; }
        public string Feeder5_Sel { get; set; }
        public string LowCap_Sel { get; set; }
        public string NoSeal_Sel { get; set; }
        public string Feeder2_Sel { get; set; }
        public string ZipMark_Sel { get; set; }
        public string Address2 { get; set; }
        public string Zip5 { get; set; }
        public string Zip4 { get; set; }
        public string Zip2 { get; set; }
        public string DivertVS2 { get; set; }
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
        public string EndofRecord { get; set; }
        public string CustomerId { get; set; }
        public bool IsForeignAddress { get; set; }
        public string DocumentCode { get; set; }
        public bool IsChecks { get; set; }
        public string ChecksPageNumbers { get; set; }
    }

    public class DallasBatch
    {
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
    }

    public class DallasBatchTiming
    {
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public bool IsSaturday { get; set; }
    }

    public class PrintTrailDetail
    {
        public int PrintTrailId { get; set; }
        public int PrintVendorId { get; set; }
        public int SFTChannelId { get; set; }
        public string PrintVendorName { get; set; }
        public string BusinessCode { get; set; }
        public string PrintVendorCode { get; set; }
        public string VendorCode { get; set; }
        public string TrailName { get; set; }
        public bool SimulateTrail { get; set; }
        public bool IsSinglePdf { get; set; }
        public bool IsManifestRequired { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ToADEDir { get; set; }
        public string FromADEDir { get; set; }
        public int ShipperId { get; set; }
    }

    public class PrintTrailBatch
    {
        public int Id { get; set; }
        public int ShipperId { get; set; }
        public int PrintTrailId { get; set; }
        public string PrintBatchName { get; set; }
        public bool IsSinglePdf { get; set; }
        public bool IsManifestRequired { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public bool IsMRDFReceived { get; set; }
        public DateTime? MRDFReceivedOn { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime? SuspendedOn { get; set; }
        public string ValidationErrors { get; set; }
        public Int64? ZipFileSize { get; set; }
        public string ServerIP { get; set; }
    }

    public class EmailChannel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string BodyTemplate { get; set; }
        public bool IsBodyHTML { get; set; }
    }

    public class AMSystemSetting
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }
}
