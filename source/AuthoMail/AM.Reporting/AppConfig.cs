using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    static class AppConfig
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string DallasEMailTo { get; private set; }
        public static string DallasEMailCc { get; private set; }
        public static string DallasEMailSubject { get; private set; }
        public static string DallasEMailTemplate { get; private set; }

        public static string FedExReport_Email_To { get; private set; }
        public static string FedExReport_Email_Cc { get; private set; }
        public static string FedExReportSubject { get; private set; }
        public static string FedExAccountCode { get; private set; }
        public static TimeSpan FedExFullReportTiming { get; private set; }

        public static string PCIReconEMailTo { get; private set; }
        public static string PCIReconEMailCc { get; private set; }
        public static string PCIReconEMailSubject { get; private set; }
        public static string PCIReconReportPath { get; private set; }
        public static string DALLASReconEMailTo { get; private set; }
        public static string DALLASReconEMailCc { get; private set; }
        public static string DALLASReconEMailSubject { get; private set; }
        public static string DALLASReconReportPath { get; private set; }

        public static string USAAEMailTo { get; private set; }
        public static string USAAEMailCc { get; private set; }
        public static string USAAMonthlyReconEMailSubject { get; private set; }
        public static string USAAMonthlyReconReportPath { get; private set; }
        public static string USAAErrorReportEMailSubject { get; private set; }
        public static string USAAErrorReportPath { get; private set; }
        public static string USAAWeeklyReportEMailSubject { get; private set; }
        public static string USAAWeeklyReportPath { get; private set; }

        public static string GOSReconEMailTo { get; private set; }
        public static string GOSReconEMailCc { get; private set; }
        public static string GOSReconEMailSubject { get; private set; }
        public static string GOSReceivedBatchEMailSubject { get; private set; }
        public static string GOSReconReportPath { get; private set; }

        public static string GOSDataPointEMailTo { get; private set; }
        public static string GOSDataPointEMailCc { get; private set; }
        public static string GOSDataPointEMailSubject { get; private set; }
        public static string GOSDataPointEMailTemplate { get; private set; }
        public static string GOSDataPointReportTemplate { get; private set; }

        public static string GOSDailyDataPointReportTemplate { get; private set; }
        public static string GOSDailyDataPointEMailTemplate { get; private set; }
        public static string GOSDailyDataPointEMailTo { get; private set; }
        public static string GOSStatusMappingFile { get; private set; }

        public static string FloodMonthlyEMailTo { get; private set; }
        public static string FloodMonthlyEMailCc { get; private set; }
        public static string FloodMonthlyReportPath { get; private set; }

        public static string CapOneErrorEMailTo { get; private set; }
        public static string CapOneErrorEMailCc { get; private set; }
        public static string CapOneErrorReportPath { get; private set; }

        public static string FloodErrorEMailTo { get; private set; }
        public static string FloodErrorEMailCc { get; private set; }
        public static string FloodErrorReportPath { get; private set; }

        public static string AlaskaDailyEMailTo { get; private set; }
        public static string AlaskaDailyEMailCc { get; private set; }
        public static string AlaskaDailyReportPath { get; private set; }

        public static string FARErrorEMailTo { get; private set; }
        public static string FARErrorEMailCc { get; private set; }
        public static string FARErrorReportPath { get; private set; }

        public static string DocSolErrorEMailTo { get; private set; }
        public static string DocSolErrorEMailCc { get; private set; }
        public static string DocSolErrorReportPath { get; private set; }

        public static string BECUErrorEMailTo { get; private set; }
        public static string BECUErrorEMailCc { get; private set; }
        public static string BECUErrorReportPath { get; private set; }

        public static string RaymondJamesErrorEMailTo { get; private set; }
        public static string RaymondJamesErrorEMailCc { get; private set; }
        public static string RaymondJamesErrorReportPath { get; private set; }

        public static string CSSErrorEMailTo { get; private set; }
        public static string CSSErrorEMailCc { get; private set; }
        public static string CSSErrorReportPath { get; private set; }
        public static string CSSErrorReportEMailSubject { get; private set; }

        public static string ReportWizardEMailTo { get; private set; }
        public static string ReportWizardEMailCc { get; private set; }
        public static string ReportWizardReportPath { get; private set; }
        public static string ReportWizardEMailSubject { get; private set; }

        public static Dictionary<string, string> GOSDailyEmailListByAppCode
        {
            get
            {

                if (!string.IsNullOrWhiteSpace(GOSDailyDataPointEMailTo))
                {
                    return GOSDailyDataPointEMailTo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(c => c[0], c => c[1]);
                }

                return null;
            }
        }
        public static string GOSDailyDataPointEMailCc { get; private set; }
        public static string GOSDaily3MonthClientList { get; private set; }
        public static List<string> GOSDaily3MonthAppCodes
        {
            get
            {

                if (!string.IsNullOrWhiteSpace(GOSDaily3MonthClientList))
                {
                    return GOSDaily3MonthClientList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                return null;
            }
        }

        public static string USAAMTDReportPath { get; private set; }
        public static string USAAMTDEmailTo { get; private set; }
        public static string USAAMTDEmailCc { get; private set; }


        public static string TaxComEmailTo { get; private set; }
        public static string TaxComEmailCc { get; private set; }
        public static string TaxComErrorReportEmailSubject { get; private set; }
        public static string TaxComErrorReportPath { get; private set; }

        public static string TaxComEDeliveryReportEmailTo { get; private set; }
        public static string TaxComEDeliveryReportEmailCc { get; private set; }
        public static string TaxComEDeliveryReportEmailSubject { get; private set; }
        public static string TaxComEDeliveryReportPath { get; private set; }


        public static string AMPostageReconEMailTo { get; private set; }
        public static string AMPostageReconEMailCc { get; private set; }
        public static string AMPostageReconEMailSubject { get; private set; }
        public static string AMPostageReconReportPath { get; private set; }

        static AppConfig()
        {
            try
            {
                DallasEMailTo = ConfigurationManager.AppSettings["DallasEMailTo"].Trim();
                DallasEMailCc = ConfigurationManager.AppSettings["DallasEMailCc"].Trim();
                DallasEMailSubject = ConfigurationManager.AppSettings["DallasEMailSubject"].Trim();
                DallasEMailTemplate = ConfigurationManager.AppSettings["DallasEMailTemplate"].Trim();

                FedExReport_Email_To = ConfigurationManager.AppSettings["FedExReport_Email_To"].Trim();
                FedExReport_Email_Cc = ConfigurationManager.AppSettings["FedExReport_Email_Cc"].Trim();
                FedExReportSubject = ConfigurationManager.AppSettings["FedExReportSubject"].Trim();
                FedExAccountCode = ConfigurationManager.AppSettings["FedExAccountCode"].Trim();
                FedExFullReportTiming = TimeSpan.Parse(ConfigurationManager.AppSettings["FedExFullReportTiming"].Trim());

                PCIReconEMailTo = ConfigurationManager.AppSettings["PCIReconEMailTo"].Trim();
                PCIReconEMailCc = ConfigurationManager.AppSettings["PCIReconEMailCc"].Trim();
                PCIReconEMailSubject = ConfigurationManager.AppSettings["PCIReconEMailSubject"].Trim();
                PCIReconReportPath = ConfigurationManager.AppSettings["PCIReconReportPath"].Trim();

                DALLASReconEMailTo = ConfigurationManager.AppSettings["DALLASReconEMailTo"].Trim();
                DALLASReconEMailCc = ConfigurationManager.AppSettings["DALLASReconEMailCc"].Trim();
                DALLASReconEMailSubject = ConfigurationManager.AppSettings["DALLASReconEMailSubject"].Trim();
                DALLASReconReportPath = ConfigurationManager.AppSettings["DALLASReconReportPath"].Trim();

                USAAEMailTo = ConfigurationManager.AppSettings["USAAEMailTo"].Trim();
                USAAEMailCc = ConfigurationManager.AppSettings["USAAEMailCc"].Trim();
                USAAMonthlyReconEMailSubject = ConfigurationManager.AppSettings["USAAMonthlyReconEMailSubject"].Trim();
                USAAMonthlyReconReportPath = ConfigurationManager.AppSettings["USAAMonthlyReconReportPath"].Trim();
                USAAErrorReportEMailSubject = ConfigurationManager.AppSettings["USAAErrorReportEMailSubject"].Trim();
                USAAErrorReportPath = ConfigurationManager.AppSettings["USAAErrorReportPath"].Trim();
                USAAWeeklyReportEMailSubject = ConfigurationManager.AppSettings["USAAWeeklyReportEMailSubject"].Trim();
                USAAWeeklyReportPath = ConfigurationManager.AppSettings["USAAWeeklyReportPath"].Trim();

                GOSReconEMailTo = ConfigurationManager.AppSettings["GOSReconEMailTo"].Trim();
                GOSReconEMailCc = ConfigurationManager.AppSettings["GOSReconEMailCc"].Trim();
                GOSReconEMailSubject = ConfigurationManager.AppSettings["GOSReconEMailSubject"].Trim();
                GOSReceivedBatchEMailSubject = ConfigurationManager.AppSettings["GOSReceivedBatchEMailSubject"].Trim();
                GOSReconReportPath = ConfigurationManager.AppSettings["GOSReconReportPath"].Trim();

                GOSDataPointEMailTo = ConfigurationManager.AppSettings["GOSDataPointEMailTo"].Trim();
                GOSDataPointEMailCc = ConfigurationManager.AppSettings["GOSDataPointEMailCc"].Trim();
                GOSDataPointEMailSubject = ConfigurationManager.AppSettings["GOSDataPointEMailSubject"].Trim();
                GOSDataPointEMailTemplate = ConfigurationManager.AppSettings["GOSDataPointEMailTemplate"].Trim();
                GOSDataPointReportTemplate = ConfigurationManager.AppSettings["GOSDataPointReportTemplate"].Trim();

                GOSDailyDataPointEMailTo = ConfigurationManager.AppSettings["GOSDailyDataPointEMailTo"].Trim();
                GOSDailyDataPointEMailCc = ConfigurationManager.AppSettings["GOSDailyDataPointEMailCc"].Trim();
                GOSDailyDataPointEMailTemplate = ConfigurationManager.AppSettings["GOSDailyDataPointEMailTemplate"].Trim();
                GOSDailyDataPointReportTemplate = ConfigurationManager.AppSettings["GOSDailyDataPointReportTemplate"].Trim();
                GOSDaily3MonthClientList = ConfigurationManager.AppSettings["GOSDaily3MonthClientList"].Trim();
                GOSStatusMappingFile = ConfigurationManager.AppSettings["GOSStatusMappingFile"].Trim();


                USAAMTDReportPath = ConfigurationManager.AppSettings["USAAMTDReportPath"].Trim();
                USAAMTDEmailTo = ConfigurationManager.AppSettings["USAAMTDEmailTo"].Trim();
                USAAMTDEmailCc = ConfigurationManager.AppSettings["USAAMTDEmailCc"].Trim();

                FloodMonthlyEMailTo = ConfigurationManager.AppSettings["FloodMonthlyEMailTo"].Trim();
                FloodMonthlyEMailCc = ConfigurationManager.AppSettings["FloodMonthlyEMailCc"].Trim();
                FloodMonthlyReportPath = ConfigurationManager.AppSettings["FloodMonthlyReportPath"].Trim();

                TaxComEmailTo = ConfigurationManager.AppSettings["TaxComEmailTo"].Trim();
                TaxComEmailCc = ConfigurationManager.AppSettings["TaxComEmailCc"].Trim();

                TaxComErrorReportEmailSubject = ConfigurationManager.AppSettings["TaxComErrorReportEmailSubject"].Trim();
                TaxComErrorReportPath = ConfigurationManager.AppSettings["TaxComErrorReportPath"].Trim();

                TaxComEDeliveryReportEmailTo = ConfigurationManager.AppSettings["TaxComEDeliveryReportEmailTo"].Trim();
                TaxComEDeliveryReportEmailCc = ConfigurationManager.AppSettings["TaxComEDeliveryReportEmailCc"].Trim();
                TaxComEDeliveryReportEmailSubject = ConfigurationManager.AppSettings["TaxComEDeliveryReportEmailSubject"].Trim();
                TaxComEDeliveryReportPath = ConfigurationManager.AppSettings["TaxComEDeliveryReportPath"].Trim();


                AMPostageReconEMailTo = ConfigurationManager.AppSettings["AMPostageReconEMailTo"].Trim();
                AMPostageReconEMailCc = ConfigurationManager.AppSettings["AMPostageReconEMailCc"].Trim();
                AMPostageReconEMailSubject = ConfigurationManager.AppSettings["AMPostageReconEMailSubject"].Trim();
                AMPostageReconReportPath = ConfigurationManager.AppSettings["AMPostageReconReportPath"].Trim();

                CapOneErrorEMailTo = ConfigurationManager.AppSettings["CapOneErrorEMailTo"].Trim();
                CapOneErrorEMailCc = ConfigurationManager.AppSettings["CapOneErrorEMailCc"].Trim();
                CapOneErrorReportPath = ConfigurationManager.AppSettings["CapOneErrorReportPath"].Trim();

                FloodErrorEMailTo = ConfigurationManager.AppSettings["FloodErrorEMailTo"].Trim();
                FloodErrorEMailCc = ConfigurationManager.AppSettings["FloodErrorEMailCc"].Trim();
                FloodErrorReportPath = ConfigurationManager.AppSettings["FloodErrorReportPath"].Trim();

                AlaskaDailyEMailTo = ConfigurationManager.AppSettings["AlaskaDailyEMailTo"].Trim();
                AlaskaDailyEMailCc = ConfigurationManager.AppSettings["AlaskaDailyEMailCc"].Trim();
                AlaskaDailyReportPath = ConfigurationManager.AppSettings["AlaskaDailyReportPath"].Trim();

                FARErrorEMailTo = ConfigurationManager.AppSettings["FARErrorEMailTo"].Trim();
                FARErrorEMailCc = ConfigurationManager.AppSettings["FARErrorEMailCc"].Trim();
                FARErrorReportPath = ConfigurationManager.AppSettings["FARErrorReportPath"].Trim();

                DocSolErrorEMailTo = ConfigurationManager.AppSettings["DocSolErrorEMailTo"].Trim();
                DocSolErrorEMailCc = ConfigurationManager.AppSettings["DocSolErrorEMailCc"].Trim();
                DocSolErrorReportPath = ConfigurationManager.AppSettings["DocSolErrorReportPath"].Trim();

                BECUErrorEMailTo = ConfigurationManager.AppSettings["BECUErrorEMailTo"].Trim();
                BECUErrorEMailCc = ConfigurationManager.AppSettings["BECUErrorEMailCc"].Trim();
                BECUErrorReportPath = ConfigurationManager.AppSettings["BECUErrorReportPath"].Trim();

                RaymondJamesErrorEMailTo = ConfigurationManager.AppSettings["RaymondJamesErrorEMailTo"].Trim();
                RaymondJamesErrorEMailCc = ConfigurationManager.AppSettings["RaymondJamesErrorEMailCc"].Trim();
                RaymondJamesErrorReportPath = ConfigurationManager.AppSettings["RaymondJamesErrorReportPath"].Trim();

                CSSErrorEMailTo = ConfigurationManager.AppSettings["CSSErrorEMailTo"].Trim();
                CSSErrorEMailCc = ConfigurationManager.AppSettings["CSSErrorEMailCc"].Trim();
                CSSErrorReportPath = ConfigurationManager.AppSettings["CSSErrorReportPath"].Trim();
                CSSErrorReportEMailSubject = ConfigurationManager.AppSettings["CSSErrorReportEMailSubject"].Trim();

                ReportWizardEMailTo = ConfigurationManager.AppSettings["ReportWizardEMailTo"].Trim();
                ReportWizardEMailCc = ConfigurationManager.AppSettings["ReportWizardEMailCc"].Trim();
                ReportWizardReportPath = ConfigurationManager.AppSettings["ReportWizardReportPath"].Trim();
                ReportWizardEMailSubject = ConfigurationManager.AppSettings["ReportWizardEMailSubject"].Trim();
            }
            catch (Exception ex)
            {
                log.Error($"Error occured in App.Config Settings", ex);
            }
        }
    }
}
