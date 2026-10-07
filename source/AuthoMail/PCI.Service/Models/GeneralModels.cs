using PCI.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    static class JobTypes
    {
        public const string PnSJobMonitor = "PnSJobMonitor";
        public const string PnSCoreService = "PnSCoreService";
        public const string PCIJobMonitor = "PCIJobMonitor";
    }

    static class OperationTypes
    {
        public const string Prep = "Prep";
        public const string ProcessMRDF = "ProcessMRDF";
        public const string Submit = "Submit";
        public const string ReportBack = "ReportBack";
    }

    static class BatchValidationStatus
    {
        public const string Valid = "Valid";
        public const string PartiallyValid = "PartiallyValid";
        public const string Invalid = "Invalid";
    }

    static class BatchPipelineStatus
    {
        public const string Received = "Received";
        public const string Downloaded = "Downloaded";
        public const string Prepped = "Prepped";
        public const string Submitted = "Submitted";
        public const string MRDFReceived = "MRDFReceived";
        public const string ReportPrepped = "ReportPrepped";
        public const string ReportedBack = "ReportedBack";
        public const string Suspended = "Suspended";
        public const string Cancelled = "Cancelled";
    }

    static class LogBatchTimeLine
    {
        public const string DownloadStart = "DownloadStart";
        public const string DownloadEnd = "DownloadEnd";
        public const string PCIBatchPrepStart = "PCIBatchPrepStart";
        public const string PCIBatchPrepEnd = "PCIBatchPrepEnd";
        public const string IEReportPrepStart = "IEReportPrepStart";
        public const string IEReportPrepEnd = "IEReportPrepEnd";
        public const string SubmitToPCIStart = "SubmitToPCIStart";
        public const string SubmitToPCIEnd = "SubmitToPCIEnd";
        public const string MRDFDownloadStart = "MRDFDownloadStart";
        public const string MRDFDownloadEnd = "MRDFDownloadEnd";
        public const string ReportPrepStart = "ReportPrepStart";
        public const string ReportPrepEnd = "ReportPrepEnd";
        public const string ReportBackStart = "ReportBackStart";
        public const string ReportBackEnd = "ReportBackEnd";
    }

    enum ErrorTypes
    {
        IO = -1,
        CannotResize = -2,
        PageCountMismatch = -3,
        Reprint = 7
    }

    public static class Env
    {
        public static string Prod = "Prod";
        public static string Staging = "Staging";
        public static string QA = "QA";
        public static string Internal = "Internal";
    }
    public static class ShippingCarrier
    {
        public static string USPS = "USPS";
        public static string FedEx = "FedEx";
        public static string UPS = "UPS";
    }

    public static class ShippingServiceType
    {
        public static string USPS = "USPS";
        public static string FEDEX_2_DAY = "FEDEX_2_DAY";
        public static string FEDEX_EXPRESS_SAVER = "FEDEX_EXPRESS_SAVER";
        public static string FEDEX_GROUND = "FEDEX_GROUND";
        public static string FIRST_OVERNIGHT = "FIRST_OVERNIGHT";
        public static string GROUND_HOME_DELIVERY = "GROUND_HOME_DELIVERY";
        public static string PRIORITY_OVERNIGHT = "PRIORITY_OVERNIGHT";
        public static string STANDARD_OVERNIGHT = "STANDARD_OVERNIGHT";
        public static string USPS_EXPRESSMAIL = "USPS_EXPRESSMAIL";
    }
}