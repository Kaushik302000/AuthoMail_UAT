using AM.Lib;
using AM.Lib.Utilities;
using AM.Services;
using AM.Services.Models;
using log4net;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace AM.Reporting
{
    class WizardReport
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateWizardReport()
        {
            try
            {
                log.Debug($"Wizard report Execution Triggered at {Common.Now.UtcToEst()} Est");
                List<AutoReportWizard> reportDetails;
                DateTime endDate = Common.EstToday.AddDays(-1);
                DateTime startDate = endDate.AddDays(-9);
                using (IReportService reportSvc = new ReportService())
                {
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    reportDetails = reportSvc.GetAutoReportWizard(startDate, endDate);

                    reportDetails.ForEach(m =>
                    {
                        m.ReceivedOn = m.ReceivedOn.HasValue ? m.ReceivedOn.Value.UtcToEst() : m.ReceivedOn;
                        m.ShippedOn = m.ShippedOn.HasValue ? m.ShippedOn.Value.UtcToEst() : m.ShippedOn;
                    });
                    var dataTable = ExcelHelper.ToDataTable<AutoReportWizard>(reportDetails);

                    Dictionary<string, string> columnMaaper = new Dictionary<string, string>();

                    foreach (var prop in typeof(AutoReportWizard).GetProperties())
                    {
                        columnMaaper.Add(prop.Name, prop.GetCustomAttribute<ColumnAttribute>().Name);
                    }
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        column.ColumnName = columnMaaper[column.ColumnName];
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        var memoryStream = ExcelHelper.ExportDataSet(dataTable);

                        string reportFileName = $"AutoReportWizard_{startDate.ToString("MMddyyyy")}_{endDate.ToString("MMddyyyy")}.xlsx";
                        using (FileStream steam = new FileStream(Path.Combine(AppConfig.ReportWizardReportPath, reportFileName), FileMode.Create))
                        {
                            memoryStream.WriteTo(steam);
                        }
                        memoryStream.Dispose();
                    }

                }
                log.Debug($"Wizard report Execution completed at {Common.Now.UtcToEst()} Est");
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Wizard report batch", ex);
            }
        }

        private static bool SendEmailNotification(List<ReportWizard> reportDetails, MemoryStream fileData, string reportFileName, string subject)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(AppConfig.ReportWizardEMailTo);
                        msg.CC.Add(AppConfig.ReportWizardEMailCc);
                        msg.Subject = $"{Config.Common_Environment} : {subject}";
                        msg.Priority = MailPriority.Normal;
                        msg.Body = subject;

                        if (fileData != null)
                            msg.Attachments.Add(new Attachment(fileData, reportFileName));

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
