using AM.Entities;
using AM.Services;
using AM.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;

namespace AM.Reporting
{
    static class AMCChannel
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateSummaryReport()
        {
            try
            {
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                using (ICommonService commonSvc = new CommonService())
                {
                    Client client = clientSvc.GetClientByAppCode("USAA").Result;

                    if (client != null)
                    {
                        List<ClientSFTChannel> sftChannelList = clientSvc.GetClientSFTChannelMapDetails(client.Id);

                        if (sftChannelList != null && sftChannelList.Count > 0)
                        {
                            foreach (var sftChannel in sftChannelList)
                            {
                                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(sftChannel.SFTChannelId).Result;

                                if (sftChannelDetails == null)
                                {
                                    log.Debug("No records found for Client : {0) And SFTChannel-Id : {1}".Fill(client.AppCode, sftChannel.SFTChannelId));
                                    continue;
                                }

                                var estToday = DateTime.UtcNow.UtcToEst().Date;

                                var startDate = estToday.AddDays(-1) + TimeSpan.Parse(Config.Web_StartCutOffTime);
                                var endDate = estToday.Date + TimeSpan.Parse(Config.Web_EndCutOffTime);

                                var reportData = reportSvc.GetSummaryControlReport(client.Id, startDate, endDate, sftChannelDetails.Code).OrderBy(s => s.MailBatchId).ToList();

                                if (reportData != null && reportData.Count > 0)
                                {
                                    var columnName = String.Join(",", "BatchID", "Id", "Channel", "Package Name", "Loan Number", "File Name", "Date Received", "Due Date", "Date Printed", "Date Mailed", "Client Code", "Tracking Number", "Shipping Method", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Print Location", "Pages Shipped", "Status", "Error Code", "Error Text");

                                    StringBuilder summaryReport = new StringBuilder(columnName);

                                    summaryReport.AppendLine();

                                    foreach (var mailPiece in reportData)
                                    {
                                        var line = String.Join(",",
                                            mailPiece.MailBatches.ClientBatchId.ToCsvString(),
                                            mailPiece.UniqueId,
                                            sftChannelDetails.Title.ToCsvString(),
                                            mailPiece.DocumentOrPackageType.ToCsvString(),
                                            mailPiece.BusinessFriendlyIdValue.ToCsvString(),
                                            mailPiece.FileName.ToCsvString(),
                                            mailPiece.MailBatches.ReceivedOn == null ? string.Empty : mailPiece.MailBatches.ReceivedOn.Value.UtcToClientTimeZone(client.TimeZone).ToString(),
                                            mailPiece.DueDate == null ? string.Empty : mailPiece.DueDate.Value.UtcToClientTimeZone(client.TimeZone).ToString(),
                                            mailPiece.PrintedOn == null ? string.Empty : mailPiece.PrintedOn.Value.UtcToClientTimeZone(client.TimeZone).ToString(),
                                            mailPiece.ShippedOn == null ? string.Empty : mailPiece.ShippedOn.Value.UtcToClientTimeZone(client.TimeZone).ToString(),
                                            mailPiece.Clients.AppCode,
                                            mailPiece.TrackingNo.ToCsvString(),
                                            mailPiece.DocTypes.ShippingCarrier,
                                            mailPiece.RcptFirstName.ToCsvString(),
                                            mailPiece.RcptLastName.ToCsvString(),
                                            mailPiece.RcptAddressStreet.ToCsvString(),
                                            mailPiece.RcptAddressCity,
                                            mailPiece.RcptAddressState,
                                            mailPiece.RcptAddressZIP,
                                            mailPiece.PrintLocation.ToCsvString(),
                                            mailPiece.TotalPages,
                                            mailPiece.ShippingStatus,
                                            mailPiece.ErrorCode,
                                            mailPiece.ErrorDesc.ToCsvString());

                                        summaryReport.AppendLine(line);
                                    }

                                    string summaryReportFileName = "{0}_{1}_{2}_{3}.csv".Fill("SummaryControlReport", client.AppCode, sftChannelDetails.Code, DateTime.UtcNow.UtcToClientTimeZone(client.TimeZone).ToString("MM-dd-yyyy"));


                                    string summaryReportPath = Path.Combine(Config.Common_AuthoMailRootFolder, summaryReportFileName);
                                    try
                                    {
                                        File.WriteAllText(summaryReportPath, summaryReport.ToString());

                                        SendDownloadNotificationEmail(summaryReportPath, client.TimeZone, startDate, endDate);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error("Error occured while uploading Summary control report :" + ex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while uploading Summary report on SFT site.", ex);
            }
        }

        private static bool SendDownloadNotificationEmail(string summaryReportPath, string clientTimeZone, DateTime startDate, DateTime endDate)
        {
            bool isEmailSent = false;

            try
            {
                string subject = ConfigurationManager.AppSettings["SummaryNotificationSubject"];
                string email_To = ConfigurationManager.AppSettings["SummaryNotification_Email_To"];
                string email_Cc = ConfigurationManager.AppSettings["SummaryNotification_Email_Cc"];
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(email_To);
                        msg.CC.Add(email_Cc);
                        msg.Subject = subject + Common.Now.UtcToClientTimeZone(clientTimeZone).ToString("MM-dd-yyyy");
                        msg.Priority = MailPriority.Normal;
                        msg.Body = "Summary Control Report From Date {0} to {1}".Fill(startDate.UtcToClientTimeZone(clientTimeZone), endDate.UtcToClientTimeZone(clientTimeZone));

                        msg.Attachments.Add(new Attachment(summaryReportPath));

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
