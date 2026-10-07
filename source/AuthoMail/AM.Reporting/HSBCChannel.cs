using AM.Entities;
using AM.Services;
using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;

namespace AM.Reporting
{
    static class HSBCChannel
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateConsolidatedReport()
        {
            try
            {
                using (IClientService clientSvc = new ClientService())
                using (IHSBCService hsbcSvc = new HSBCService())
                using (ICommonService commonSvc = new CommonService())
                {
                    Client client = clientSvc.GetClientByAppCode("HSBC").Result;

                    string clientReportFolderPath = Path.Combine(Config.Common_AuthoMailRootFolder, client.AppCode, "ConsolidatedReport");

                    Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Consolidated);

                    if (deliverable != null)
                    {
                        var effectiveReceivedOnDate = Common.EstToday.AddDays(-1);

                        var consolidatedReportData = hsbcSvc.ConsolidatedReport(client.Id, effectiveReceivedOnDate);

                        var columnName = String.Join(deliverable.Delimeter, "BatchID", "StatusID", "Loan Number", "File Name", "Package Name", "Date Received", "Date Printed", "Date Mailed", "Recipient FirstName", "Recipient LastName", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Pages Shipped", "Total Postage Cost");

                        StringBuilder summaryReport = new StringBuilder(columnName);

                        summaryReport.AppendLine();

                        foreach (var mailPiece in consolidatedReportData)
                        {
                            var line = String.Join(deliverable.Delimeter,
                                mailPiece.BatchName + ".xml",
                                mailPiece.StatusID,
                                mailPiece.BusinessFriendlyIdValue,
                                mailPiece.FileName,
                                mailPiece.DocumentOrPackageType,
                                mailPiece.ReceivedOn.HasValue ? mailPiece.ReceivedOn.Value.UtcToClientTimeZone(client.TimeZone) : mailPiece.ReceivedOn,
                                mailPiece.PrintedOn.HasValue ? mailPiece.PrintedOn.Value.UtcToClientTimeZone(client.TimeZone) : mailPiece.PrintedOn,
                                mailPiece.ShippedOn.HasValue ? mailPiece.ShippedOn.Value.UtcToClientTimeZone(client.TimeZone) : mailPiece.ShippedOn,
                                mailPiece.RcptFirstName.ToCsvString(),
                                mailPiece.RcptLastName.ToCsvString(),
                                mailPiece.RcptAddressStreet.ToCsvString(),
                                mailPiece.RcptAddressCity.ToCsvString(),
                                mailPiece.RcptAddressState,
                                mailPiece.RcptAddressZIP,
                                mailPiece.TotalPages,
                                mailPiece.TotalPages);
                            summaryReport.AppendLine(line);
                        }

                        string consolidateReportName = "Delivery-{0}.csv".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                        string consolidateReportPath = Path.Combine(clientReportFolderPath, consolidateReportName);
                        try
                        {
                            File.WriteAllText(consolidateReportPath, summaryReport.ToString());

                            if (deliverable.EmailChannel != null)
                            {
                                SendConsolidatedNotificationEmail(consolidateReportPath, client.TimeZone, deliverable, effectiveReceivedOnDate);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error occured while uploading consolidated control report :" + ex);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static bool SendConsolidatedNotificationEmail(string consolidateReportPath, string clientTimeZone, Deliverable deliverable, DateTime startDate)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = deliverable.EmailChannel.IsBodyHTML;
                        msg.To.Add(deliverable.EmailChannel.To);
                        msg.CC.Add(deliverable.EmailChannel.Cc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, "HSBC", deliverable.EmailChannel.Subject); ;
                        msg.Priority = MailPriority.Normal;
                        //msg.Body = deliverable.EmailChannel.BodyTemplate;
                        msg.Body = "Consolidated Report of {0} batches".Fill(startDate.Date.ToString("MM/dd/yyyy"));
                        if (File.Exists(consolidateReportPath))
                            msg.Attachments.Add(new Attachment(consolidateReportPath));

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
