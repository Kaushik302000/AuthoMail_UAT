using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AM.Scanner.Common
{
    public static class Common
    {


        static Common()
        {
            SP_GetScanStats = ConfigurationManager.AppSettings["GetScanStats"].Trim();
            SP_RegisterScan = ConfigurationManager.AppSettings["RegisterScan"].Trim();
            SP_GetScanReportData = ConfigurationManager.AppSettings["GetScanReportData"].Trim();
            SP_GetRecentBatchesStats = ConfigurationManager.AppSettings["GetRecentBatchesStats"].Trim();
            SP_GetPendingScanning = ConfigurationManager.AppSettings["GetPendingScanning"].Trim();
            Environment = ConfigurationManager.AppSettings["Environment"].Trim();
            SP_RegisterUSPSScan = ConfigurationManager.AppSettings["RegisterUSPSScan"].Trim();
            SP_GetScannedMailPieces = ConfigurationManager.AppSettings["GetScannedMailPieces"].Trim();
            AMRootPath = ConfigurationManager.AppSettings["AMRootPath"].Trim();
            PCIRootPath = ConfigurationManager.AppSettings["PCIRootPath"].Trim();
        }
        public static string Environment { get; set; }

        public static string SP_GetScanStats { get; set; }

        public static string SP_RegisterScan { get; set; }
        public static string SP_RegisterUSPSScan { get; set; }

        public static string SP_GetScanReportData { get; set; }
        public static string SP_GetRecentBatchesStats { get; set; }
        public static string SP_GetPendingScanning { get; set; }
        public static string SP_GetScannedMailPieces { get; set; }
        public static string AMRootPath { get; set; }
        public static string PCIRootPath { get; set; }
    }
}