using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class Program
    {
        static void Main(string[] args)
        {
            var argString = String.Join(",", args);
            log4net.LogicalThreadContext.Properties["args"] = argString;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(ConfigurationManager.AppSettings["log4net.Config"]));


            switch (args[0])
            {
                case "HSBC":
                    HSBCChannel.GenerateConsolidatedReport();
                    break;
                case "AMC":
                    AMCChannel.GenerateSummaryReport();
                    break;
                case "FedExPickUp":
                    FedExPickUp.CheckPickedUpPackages();
                    break;
                case "FedExTracking":
                    CarrierTrackingInfo.CheckForShipmentTracking();
                    break;
                case "USPSTracking":
                    CarrierTrackingInfo.CheckForUSPSeVSShipmentTracking();
                    break;
                case "DallasNotification":
                    DallasNotification.GenerateEmailNotification();
                    break;
                case "PCIRecon":
                    PCIReconReport.GeneratePCIReconReport();
                    break;
                case "DALLASRecon":
                    DALLASReconReport.GenerateDALLASReconReport();
                    break;
                case "AlaskaDailyOperation":
                    AlaskaReports.GenerateDailyOperationReport();
                    break;
                case "AMPostageRecon":
                    DALLASReconReport.GenerateAMPostageReconReport();
                    break;
                case "GOS":
                    switch (args[1])
                    {
                        case "ReceivedBatch":
                            GOSReports.GenerateGOSReceivedBatchReport();
                            break;
                        case "Recon":
                            GOSReports.GenerateGOSReconReport();
                            break;
                        case "DataPoints":
                            GOSReports.GenerateGOSDataPointReport();
                            break;
                        case "ConsolidatedDataPoints":
                            GOSReports.GenerateGOSConsolidatedDataPointReport(args[2]);
                            break;
                        case "DailyDataPoints":
                            GOSReports.GenerateGOSDailyDataPointReport(args[2]);
                            break;
                        default:
                            break;
                    }
                    break;
                case "USAAReport":
                    switch (args[1])
                    {
                        case "MonthlyRecon":
                            USAAReporting.GenerateMonthlyReconReport();
                            break;
                        case "ErrorReport":
                            USAAReporting.GenerateErrorReport();
                            break;
                        case "WeeklyReport":
                            USAAReporting.GenerateWeeklyReport();
                            break;
                        default:
                            break;
                    }
                    break;
                case "FAR":
                    ErrorReports.GenerateFARErrorReport();
                    break;
                case "USAAMTD":
                    USAADailyMTDVolumeReporting.GenerateMTDReport();
                    break;
                case "DocSolError":
                    ErrorReports.GenerateDocSolErrorReport();
                    break;
                case "BECUError":
                    ErrorReports.GenerateBECUErrorReport();
                    break;
                case "FLOODAUTO":
                    switch (args[1])
                    {
                        case "MonthlyReport":
                            FloodReports.GenerateMonthlyReport();
                            break;
                        case "CapOneErrorReport":
                            FloodReports.GenerateCapOneErrorReport();
                            break;
                        case "FloodErrorReport":
                            FloodReports.GenerateFloodErrorReport();
                            break;
                        default:
                            break;
                    }
                    break;
                case "DelfusReport":
                    switch (args[1])
                    {
                        case "ErrorReport":
                            DelfusReporting.GenerateErrorReport();
                            break;
                        case "EdeliveryReport"://3rd parameter will be "Daily" or "Monthly"
                            DelfusReporting.GenerateTaxComEdeliveryReport(args[2]);
                            break;
                        default:
                            break;
                    }
                    break;
                case "RaymondJamesError":
                    ErrorReports.GenerateRaymondJamesErrorReport();
                    break;
                case "CSS":
                    switch (args[1])
                    {
                        case "ErrorReport":
                            CSSReporting.GenerateErrorReport();
                            break;
                        default:
                            break;
                    }
                    break;
                case "ReportWizard":
                    WizardReport.GenerateWizardReport();
                    break;
                default:
                    break;
            }
        }
    }
}
