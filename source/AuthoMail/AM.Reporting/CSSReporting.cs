using AM.Entities;
using AM.Lib;
using AM.Services;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class CSSReporting
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void GenerateErrorReport()
        {
            try
            {
                log.Debug("CSS Error Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<ReportWizard> mpDetails;
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var clientInfo = clientSvc.GetClientsByAlias("CSS Invoices");
                    DateTime currentTime = Common.EstToday;
                    DateTime endDate = currentTime.Date.ToEndDateTime();
                    DateTime startDate = currentTime.Date.ToStartDateTime();
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call

                    mpDetails = reportSvc.GetReportWizard(clientInfo.Select(x => x.Id).ToList(), null, null, startDate, endDate, null, null, null, null, null);  
                    var errorMps = mpDetails.Where(x => x.Status == ShippingStatus.Error.ToString()).ToList();
                    if (errorMps.Count == 0)
                    {
                        log.Debug($"CSS Error Report does not contain any records so exiting from the process at {Common.Now.UtcToEst()} Est");
                        return;
                    }
                    GenerateErrorReport(errorMps, endDate);                    
                }
                log.Debug("CSS Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating CSS Error Report", ex);
            }
        }
        private static void GenerateErrorReport(List<ReportWizard> errorReportData, DateTime endDate)
        {
            string subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.CSSErrorReportEMailSubject.Fill(endDate.ToString("MM/dd/yyyy")));

            string body = "CSS Error Report For : {0}".Fill(endDate.ToString("MM/dd/yyyy"));

            var columnName = String.Join(",", "Loan Number", "Document Type", "Error Reason", "Receive Date");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in errorReportData)
            {
                var line = String.Join(",", mp.LoanNumber.ToCsvString(), mp.DocumentOrPackageType, mp.ErrorDesc, mp.ReceivedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss"));

                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.CSSErrorReportPath, "CSSErrorReport_" + endDate.ToString("MM-dd-yyyy") + ".csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCSSTeam(reportFilePath, subject, body);
        }
        private static void SendReportToCSSTeam(string reportFilePath, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.CSSErrorEMailTo);
                        msg.CC.Add(AppConfig.CSSErrorEMailCc);
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
                log.Error("Error occured while sending CSS Report :", ex);
            }
        }


    }
}
