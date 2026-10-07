using AM.Entities;
using AM.Lib;
using AM.Services;
using AM.Services.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace AM.Reporting
{
    class FloodReports
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateMonthlyReport()
        {
            try
            {
                log.Debug("Flood Monthly received batch report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<ReportWizard> mpDetails;
                DateTime currentTime = Common.EstToday;
                DateTime endDate = currentTime.Date.AddDays(-1);
                DateTime startDate = currentTime.Date.AddMonths(-1);
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var floodClients = clientSvc.GetClientsByAlias("Flood Integration");
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    mpDetails = reportSvc.GetReportWizard(floodClients.Select(x => x.Id).ToList(), null, null, startDate, endDate, null, null, null, null, null);

                    if (mpDetails.Count > 0)
                    {
                        string reportFilePath = Path.Combine(AppConfig.FloodMonthlyReportPath, $"FloodBatchReceivedReport_{startDate.ToString("MMddyyyy")}_To_{endDate.ToString("MMddyyyy")}.csv");
                        SendEmailNotificationToFlood(mpDetails, reportFilePath, $"Flood Monthly Report For {startDate.ToString("MMddyyyy")} To {endDate.ToString("MMddyyyy")}");
                    }
                }
                log.Debug("Flood Monthly received batch report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Flood Monthly received batch report", ex);
            }
        }

        internal static void GenerateCapOneErrorReport()
        {
            try
            {
                log.Debug("Flood CapOne Client Error Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<ReportWizard> mpDetails;
                DateTime currentTime = Common.EstToday;
                DateTime endDate = currentTime.Date.ToEndDateTime();
                DateTime startDate = currentTime.Date.ToStartDateTime();
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var floodClients = clientSvc.GetClientsByAlias("Flood Integration").Where(x => x.AppCode == "CAPONE");
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    mpDetails = reportSvc.GetReportWizard(floodClients.Select(x => x.Id).ToList(), null, null, startDate, endDate, null, null, null, null, null);
                    var errorMps = mpDetails.Where(x => x.Status == ShippingStatus.Error.ToString()).ToList();
                    if (errorMps.Count == 0)
                    {
                        log.Debug($"Flood CapOne Client Error Report does not contain any records so exiting from the process at {Common.Now.UtcToEst()} Est");
                        return;
                    }

                    string reportFilePath = Path.Combine(AppConfig.CapOneErrorReportPath, $"CapOneErrorReport_{startDate.ToString("MMddyyyy")}.csv");
                    GenerateReportForCapOne(errorMps, reportFilePath, $"CapOne Error Report For {startDate.ToString("MMddyyyy")}");
                }
                log.Debug("Flood CapOne Client Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Flood Monthly received batch report", ex);
            }
        }

        internal static void GenerateFloodErrorReport()
        {
            try
            {
                log.Debug("Flood Error Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<ReportWizard> mpDetails;
                DateTime currentTime = Common.EstToday;
                DateTime endDate = currentTime.Date.ToEndDateTime();
                DateTime startDate = currentTime.Date.ToStartDateTime();
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var floodClients = clientSvc.GetClientsByAlias("Flood Integration");
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    mpDetails = reportSvc.GetReportWizard(floodClients.Select(x => x.Id).ToList(), null, null, startDate, endDate, null, null, null, null, null);
                    var errorMps = mpDetails.Where(x => x.Status == ShippingStatus.Error.ToString()).ToList();
                    if (errorMps.Count == 0)
                    {
                        log.Debug($"Flood Error Report does not contain any records so exiting from the process at {Common.Now.UtcToEst()} Est");
                        return;
                    }

                    string reportFilePath = Path.Combine(AppConfig.FloodErrorReportPath, $"FloodErrorReport_{startDate.ToString("MMddyyyy")}.csv");
                    GenerateReportForFlood(errorMps, reportFilePath, $"Flood Error Report For {startDate.ToString("MMddyyyy")}");
                }
                log.Debug("Flood Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Flood Error Report", ex);
            }
        }

        private static void GenerateReportForFlood(List<ReportWizard> mpDetails, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Batch Id", "MailPiece Id", "Channel", "Document Type", "Flood Cert Number", "Lender Loan Number", "File Name", "Error Code", "Error Text", "Received On (Est)", "First Name", "Last Name", "Street", "City", "State", "ZIP", "Shipping Method");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in mpDetails)
            {
                var line = String.Join(",", mp.BatchId.ToCsvString(), mp.MailPieceId, mp.Channel, mp.DocumentOrPackageType, mp.LoanNumber.ToCsvString(), mp.FloodLoan_Number.ToCsvString(), mp.FileName, mp.ErrorCode, mp.ErrorDesc, mp.ReceivedOn.Value.UtcToEst(), mp.FirstName, mp.LastName, mp.RcptAddressStreet, mp.City, mp.State, mp.ZIP, mp.ShippingMethod);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCLTeam(reportFilePath, subject, "Flood Error Report", AppConfig.FloodErrorEMailTo, AppConfig.FloodErrorEMailCc);
        }

        private static void GenerateReportForCapOne(List<ReportWizard> mpDetails, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Batch Id", "MailPiece Id", "Channel", "Document Type", "Flood Cert Number", "Lender Loan Number", "File Name", "Error Code", "Error Text", "Received On (Est)", "First Name", "Last Name", "Street", "City", "State", "ZIP", "Shipping Method");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in mpDetails)
            {
                var line = String.Join(",", mp.BatchId.ToCsvString(), mp.MailPieceId, mp.Channel, mp.DocumentOrPackageType, mp.LoanNumber.ToCsvString(), mp.FloodLoan_Number.ToCsvString(), mp.FileName, mp.ErrorCode, mp.ErrorDesc, mp.ReceivedOn.Value.UtcToEst(), mp.FirstName, mp.LastName, mp.RcptAddressStreet, mp.City, mp.State, mp.ZIP, mp.ShippingMethod);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCLTeam(reportFilePath, subject, "Capital One Error Report", AppConfig.CapOneErrorEMailTo, AppConfig.CapOneErrorEMailCc);
        }

        private static void SendEmailNotificationToFlood(List<ReportWizard> mpDetails, string reportFilePath, string subject)
        {
            var columnName = String.Join("|", "MailPieceId", "BatchId", "Channel", "Document Or PackageType", "Loan Number", "Tracking Number", "Received On (Est)", "Printed On (Est)", "Mailed On (Est)", "File Name", "Client Code", "Shipping Method", "First Name", "Last Name", "Street", "City", "State", "ZIP", "Print Location", "Pages Shipped", "Blank Pages", "Status", "Inserts", "Print File Name", "Envelope Name", "Delivery Date", "Delivery Status", "Requestor Name", "Property Street", "Property City", "Property State", "Property Zip");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in mpDetails)
            {
                var line = String.Join("|", mp.MailPieceId, mp.BatchId.ToCsvString(), mp.Channel, mp.DocumentOrPackageType, mp.LoanNumber.ToCsvString(), mp.TrackingNo.ToCsvString(), mp.ReceivedOn.HasValue ? (DateTime?)mp.ReceivedOn.Value.UtcToEst() : null, mp.PrintedOn.HasValue ? (DateTime?)mp.PrintedOn.Value.UtcToEst() : null, mp.MailedOn.HasValue ? (DateTime?)mp.MailedOn.Value.UtcToEst() : null, mp.FileName, mp.ClientCode, mp.ShippingMethod, mp.FirstName, mp.LastName, mp.RcptAddressStreet, mp.City, mp.State, mp.ZIP, mp.PrintLocation, mp.PagesShipped, mp.BlankPages, mp.State, mp.Inserts, mp.PrintFileName, mp.EnvelopeName, mp.DeliveryDate, mp.DeliveryStatus, mp.RequestorName, mp.PropertyStreetAddress, mp.PropertyCity, mp.PropertyState, mp.PropertyZip);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCLTeam(reportFilePath, subject, "Flood Monthly Report", AppConfig.FloodMonthlyEMailTo, AppConfig.FloodMonthlyEMailCc);
        }

        private static bool SendReportToCLTeam(string reconReportPath, string subject, string body, string to, string cc)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(to);
                        msg.CC.Add(cc);
                        msg.Subject = $"{Config.Common_Environment} : {subject}";
                        msg.Priority = MailPriority.Normal;
                        msg.Body = body;

                        if (File.Exists(reconReportPath))
                            msg.Attachments.Add(new Attachment(reconReportPath));

                        smtpClient.Send(msg);
                        isEmailSent = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return isEmailSent;
        }
    }
}
