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
    class AlaskaReports
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void GenerateDailyOperationReport()
        {
            log.Debug($"Alaska Daily Operation report Execution Triggered at {Common.Now.UtcToEst()} Est");

            using (IReportService reportSvc = new ReportService())
            {
                var currentDate = Common.EstToday;
                try
                {
                    var reportData = reportSvc.GetDailyDetailOperationsByDateOrBatch(102, currentDate, null, null, null);

                    if (reportData.Count > 0)
                    {
                        var columnName = String.Join(",", "Batch Name", "Unique Id", "Channel", "Loan Number", "Document Type", "Shipping Status", "First Name", "Last Name", "Street", "City", "State", "Zip", "Received On (Est)", "Due Date (Est)", "Shipped On (Est)", "File Name", "Carrier", "Service Type", "Tracking Number", "Total Pages", "Total Blank Pages", "Print File Name", "Print Location");

                        StringBuilder reconReport = new StringBuilder(columnName);

                        reconReport.AppendLine();
                        foreach (var mp in reportData)
                        {
                            var line = String.Join(",", mp.MailBatches.BatchName.ToCsvString(), mp.UniqueId, mp.MailBatches.SFTChannels.Title, mp.BusinessFriendlyIdValue, mp.DocumentOrPackageType, mp.ShippingStatus, mp.RcptFirstName, mp.RcptLastName, mp.RcptAddressStreet.ToCsvString(), mp.RcptAddressCity.ToCsvString(), mp.RcptAddressState.ToCsvString(), mp.RcptAddressZIP.ToCsvString(), mp.MailBatches.ReceivedOn.Value.UtcToEst().ToString("MM-dd-yyyy hh:mm:ss"), mp.DueDate.HasValue ? mp.DueDate.Value.UtcToEst().ToString("MM-dd-yyyy hh:mm:ss") : "", mp.ShippedOn.HasValue ? mp.ShippedOn.Value.UtcToEst().ToString("MM-dd-yyyy hh:mm:ss") : "", mp.FileName, mp.ShippingCarrier, mp.ShippingServiceType, mp.TrackingNo.ToCsvString(), mp.TotalPages, mp.TotalBlankPages, mp.PrintFileName, mp.PrintLocation);
                            reconReport.AppendLine(line);
                        }
                        string reportFilePath = Path.Combine(AppConfig.AlaskaDailyReportPath, "AlaskaDailyOperationReport_" + Common.Now.UtcToEst().ToString("MMddyyyy") + ".csv");
                        File.WriteAllText(reportFilePath, reconReport.ToString());

                        SendReportToCLTeam(reportFilePath, currentDate);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error occured while generating Alaska Report For Dt : {currentDate}", ex);
                }
            }
            log.Debug($"Alaska Daily Operation report Execution Completed at {Common.Now.UtcToEst()} Est");
        }

        private static bool SendReportToCLTeam(string reconReportPath, DateTime todayCutOffTime)
        {
            bool isEmailSent = false;

            try
            {
                var body = $"Alaska Daily Operation Report For : {todayCutOffTime.ToString("MM/dd/yyyy")}";
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.AlaskaDailyEMailTo);
                        msg.CC.Add(AppConfig.AlaskaDailyEMailCc);
                        msg.Subject = $"{Config.Common_Environment} : {body}";
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
