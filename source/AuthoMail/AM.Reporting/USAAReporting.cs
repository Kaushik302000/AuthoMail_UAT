using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class USAAReporting
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void GenerateMonthlyReconReport()
        {
            try
            {
                log.Debug("USAA Monthly Recon Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<MonthlyReconReportViewModel> monthlyReconReportData;
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var clientInfo = clientSvc.GetClientByAppCode("USAA").Result;
                    var getPreviousMonthDate = Common.EstToday.AddMonths(-1);
                    var getCurrentMonthDate = Common.EstToday;

                    DateTime startDate = new DateTime(getPreviousMonthDate.Year, getPreviousMonthDate.Month, 26, 0, 0, 0);
                    DateTime endDate = new DateTime(getCurrentMonthDate.Year, getCurrentMonthDate.Month, 25, 23, 59, 59);

                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    monthlyReconReportData = reportSvc.GetMonthlyReconReport(clientInfo.Id, startDate, endDate, "");

                    if (monthlyReconReportData.Count > 0)
                    {
                        GenerateMonthlyReport(monthlyReconReportData, startDate, endDate);
                    }
                }
                log.Debug("USAA Monthly Recon Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Monthly Recon Report For USAA", ex);
            }
        }

        internal static void GenerateErrorReport()
        {
            try
            {
                log.Debug("USAA Error Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<MailPieces> errorReportData;
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var clientInfo = clientSvc.GetClientByAppCode("USAA").Result;
                    DateTime previousDate = Common.EstToday.AddDays(-1);
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    errorReportData = reportSvc.GetErrorReportByDateOrBatch(clientInfo.Id, previousDate, previousDate, null, "");

                    if (errorReportData.Count > 0)
                    {
                        GenerateErrorReport(errorReportData, previousDate);
                    }
                }
                log.Debug("USAA Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating USAA Error Report", ex);
            }
        }

        internal static void GenerateWeeklyReport()
        {
            try
            {
                log.Debug("USAA Weekly Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<MonthlyReconReportViewModel> weeklyReportData;
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var clientInfo = clientSvc.GetClientByAppCode("USAA").Result;
                    var getPreviousSaturdayDate = Common.EstToday.AddDays(-2);//For Previous Saturday
                    var getMondayDate = getPreviousSaturdayDate.AddDays(-5);//For Previous Monday

                    DateTime startDate = new DateTime(getMondayDate.Year, getMondayDate.Month, getMondayDate.Day, 0, 0, 0);
                    DateTime endDate = new DateTime(getPreviousSaturdayDate.Year, getPreviousSaturdayDate.Month, getPreviousSaturdayDate.Day, 23, 59, 59);

                    weeklyReportData = reportSvc.GetMonthlyReconReport(clientInfo.Id, startDate, endDate, "");

                    if (weeklyReportData.Count > 0)
                    {
                        GenerateWeeklyReport(weeklyReportData, startDate, endDate);
                    }
                }
                log.Debug("USAA Weekly Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating USAA Weekly Report", ex);
            }
        }

        private static void GenerateWeeklyReport(List<MonthlyReconReportViewModel> weeklyReportData, DateTime startDate, DateTime endDate)
        {
            string subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.USAAWeeklyReportEMailSubject.Fill(startDate.ToString("MM/dd/yyyy hh:mm:ss"), endDate.ToString("MM/dd/yyyy hh:mm:ss")));

            string body = "USAA Weekly Report From {0} To {1}".Fill(startDate.ToString("MM/dd/yyyy hh:mm:ss"), endDate.ToString("MM/dd/yyyy hh:mm:ss"));


            var columnName = String.Join(",", "Loan Number", "Document Type", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Ship Method", "Tracking number", "Receive Date", "Shipped On", "Status", "Total Pages", "Delivery Status");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in weeklyReportData)
            {
                var line = String.Join(",", mp.LoanNumber.ToCsvString(), mp.DocType, mp.FirstName.ToCsvString(), mp.LastName.ToCsvString(), mp.Address.ToCsvString(), mp.City.ToCsvString(), mp.State, mp.ZIP, mp.ShippingMethod, mp.TrackingNo.ToCsvString(), mp.ReceivedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss"), mp.ShippedOn.HasValue ? mp.ShippedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss") : "", mp.Status, mp.TotalPages, mp.DeliveryStatus);
                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.USAAWeeklyReportPath, "USAAWeeklyReport_" + startDate.ToString("MMddyyyyhhmmss") + "_To_" + endDate.ToString("MMddyyyyhhmmss") + ".csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToUSAATeam(reportFilePath, subject, body);
        }

        private static void GenerateErrorReport(List<MailPieces> errorReportData, DateTime previousDate)
        {
            string subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.USAAErrorReportEMailSubject.Fill(previousDate.ToString("MM/dd/yyyy")));

            string body = "USAA Error Report For : {0}".Fill(previousDate.ToString("MM/dd/yyyy"));

            var columnName = String.Join(",", "Loan Number", "Document Type", "Error Reason", "Receive Date");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in errorReportData)
            {
                var line = String.Join(",", mp.BusinessFriendlyIdValue.ToCsvString(), mp.DocumentOrPackageType, mp.ErrorDesc, mp.MailBatches.ReceivedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss"));
                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.USAAErrorReportPath, "USAAErrorReport_" + previousDate.ToString("MM-dd-yyyy") + ".csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToUSAATeam(reportFilePath, subject, body);
        }

        private static void GenerateMonthlyReport(List<MonthlyReconReportViewModel> monthlyReconReportData, DateTime startDate, DateTime endDate)
        {
            string subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.USAAMonthlyReconEMailSubject.Fill(startDate.ToString("MM/dd/yyyy hh:mm:ss"), endDate.ToString("MM/dd/yyyy hh:mm:ss")));

            string body = "USAA Monthly Recon Report For : {0} To {1}".Fill(startDate.ToString("MM/dd/yyyy hh:mm:ss"), endDate.ToString("MM/dd/yyyy hh:mm:ss"));


            var columnName = String.Join(",", "Loan Number", "Document Type", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Ship Method", "Tracking number", "Receive Date", "Shipped On", "Status", "Total Pages");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in monthlyReconReportData)
            {
                var line = String.Join(",", mp.LoanNumber.ToCsvString(), mp.DocType, mp.FirstName.ToCsvString(), mp.LastName.ToCsvString(), mp.Address.ToCsvString(), mp.City.ToCsvString(), mp.State, mp.ZIP, mp.ShippingMethod, mp.TrackingNo.ToCsvString(), mp.ReceivedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss"), mp.ShippedOn.HasValue ? mp.ShippedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss") : "", mp.Status, mp.TotalPages);
                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.USAAMonthlyReconReportPath, "USAAMonthlyReconReport_" + startDate.ToString("MMddyyyyhhmmss") + "_To_" + endDate.ToString("MMddyyyyhhmmss") + ".csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToUSAATeam(reportFilePath, subject, body);
        }

        private static void SendReportToUSAATeam(string reportFilePath, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.USAAEMailTo);
                        msg.CC.Add(AppConfig.USAAEMailCc);
                        msg.Subject = subject;
                        msg.Priority = MailPriority.Normal;
                        msg.Body = body;

                        if (File.Exists(reportFilePath))
                            msg.Attachments.Add(new Attachment(reportFilePath));

                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending USAA Report :", ex);
            }
        }
    }
}
