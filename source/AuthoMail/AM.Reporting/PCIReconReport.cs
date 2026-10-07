using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System.IO;
using System.Net.Mail;

namespace AM.Reporting
{
    class PCIReconReport
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GeneratePCIReconReport()
        {
            try
            {
                log.Debug("PCI recon report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<PCISubmittedBatchDetail> pciSubmittedBatchDetails;
                using (IReportService reportSvc = new ReportService())
                {
                    DateTime startDate = Common.EstToday.EstToUtc();
                    DateTime endDate = startDate.AddDays(1);

                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    pciSubmittedBatchDetails = reportSvc.GetPCIReconReport(startDate, endDate);

                    if (pciSubmittedBatchDetails.Count > 0)
                    {
                        SendEmailNotificationToPCI(pciSubmittedBatchDetails, startDate);
                    }
                }
                log.Debug("PCI recon report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Reconcilation Report for PCI", ex);
            }
        }

        private static void SendEmailNotificationToPCI(List<PCISubmittedBatchDetail> pciSubmittedBatchDetails, DateTime currentTime)
        {
            var columnName = String.Join(",", "BatchID", "Batch Name", "Submitted On (Est)", "Due Date (Est)", "Carrier", "Service Type", "Package Count", "Total Pages");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var batch in pciSubmittedBatchDetails)
            {
                var line = String.Join(",", batch.PrintTrailBatchId, batch.PrintBatchName, batch.SubmittedOn.UtcToEst(), batch.DueDate.UtcToEst(), batch.ShippingCarrier, batch.ShippingServiceType, batch.PackageCount, batch.TotalPages);
                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.PCIReconReportPath, "PCIReconReport_" + currentTime.UtcToEst().ToString("MMddyyyy") + ".csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCLTeam(reportFilePath, currentTime);
        }

        private static bool SendReportToCLTeam(string reconReportPath, DateTime todayCutOffTime)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.PCIReconEMailTo);
                        msg.CC.Add(AppConfig.PCIReconEMailCc);
                        msg.Subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.PCIReconEMailSubject.Fill(todayCutOffTime.ToString("MM/dd/yyyy")));
                        msg.Priority = MailPriority.Normal;
                        msg.Body = "PCI Recon Report For : {0}".Fill(todayCutOffTime.ToString("MM/dd/yyyy"));

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
