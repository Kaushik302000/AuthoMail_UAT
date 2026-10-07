using AM.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using AM.Services;

namespace AM.EDelivery
{
    public class MailUtility
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MailUtility));

        public static bool SendNoBatchReceivedNotificationEmail(Client client, DateTime cutOffTime, EmailChannel emailChannel)
        {
            bool isEmailSent = false;

            try
            {
                if (emailChannel == null)
                {
                    return true;
                }

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(emailChannel.To);
                        msg.CC.Add(emailChannel.Cc);
                        if (!string.IsNullOrEmpty(emailChannel.Bcc))
                            msg.Bcc.Add(emailChannel.Bcc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, client.AppCode, emailChannel.Subject);
                        msg.Priority = MailPriority.Normal;
                        msg.Body = string.Format("No batch available today till cutOffTime {0}.", cutOffTime.UtcToClientTimeZone(client.TimeZone).ToString("HH:mm"));
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

        public static void SendUnexpectedExceptionEmail(string exception)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(Config.Monitor_Exception_Email_To);
                        msg.CC.Add(Config.Monitor_Exception_Email_Cc);
                        msg.Subject = "{0} : {1}".Fill(Config.Common_Environment, Config.Monitor_ExceptionEmail_Subject);
                        msg.Priority = MailPriority.High;
                        msg.Body = "Unexpected exception occured during execution." + Environment.NewLine + exception;
                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static bool SendDownloadNotificationEmail(MailBatchInfo mbi, List<MailPieces> mailPieces, string errorReportPath, EmailChannel emailChannel)
        {
            bool isEmailSent = false;

            try
            {
                if (emailChannel == null)
                {
                    return true;
                }

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(emailChannel.To);
                        msg.CC.Add(emailChannel.Cc);
                        if (!string.IsNullOrEmpty(emailChannel.Bcc))
                            msg.Bcc.Add(emailChannel.Bcc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, mbi.Client.AppCode, emailChannel.Subject);
                        msg.Priority = MailPriority.Normal;

                        if (mailPieces.Count > 0)
                        {
                            #region Prepare email body

                            string downloadEmail_TD_SrNoStyle = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string downloadEmail_TD_DescriptionStyle = "style='width:80%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt'' valign='top' width='213'><p>";
                            string downloadEmail_TD_CountStyle = "style='width:10%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";

                            StringBuilder sb = new StringBuilder(File.ReadAllText(Config.Monitor_DSEmailTemplate));
                            sb = sb.Replace("<%FileName%>", mbi.MailBatch.BatchName);
                            sb = sb.Replace("<%yyyymmdd%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("MM/dd/yyyy"));
                            sb = sb.Replace("<%hhmm%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("hh:mm tt"));

                            StringBuilder data = new StringBuilder();
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "1"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_DescriptionStyle, "TOTAL LOANS RECEIVED"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Count));
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "2"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", downloadEmail_TD_DescriptionStyle, "VALID LOANS"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Where(l => l.ErrorCode == OutComeCodes.No_Error).Count()));
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "3"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", downloadEmail_TD_DescriptionStyle, "INVALID LOANS"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error).Count()));

                            sb = sb.Replace("<%trdata%>", data.ToString());
                            msg.Body = sb.ToString();

                            #endregion
                        }
                        else if (mailPieces.Count == 0)
                        {
                            msg.Body = string.Format("No records available in the batch file {0}.", mbi.MailBatch.BatchName);
                        }

                        if (msg.Body.Length > 0)
                        {
                            if (File.Exists(errorReportPath))
                                msg.Attachments.Add(new Attachment(errorReportPath));

                            smtpClient.Send(msg);
                            isEmailSent = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return isEmailSent;
        }

        public static bool SendProductionNotificationEmail(MailBatchInfo mbi, List<MailPieces> mailPieces, List<string> reports, EmailChannel emailChannel)
        {
            bool isEmailSent = false;

            try
            {
                if (emailChannel == null)
                {
                    return true;
                }

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(emailChannel.To);
                        msg.CC.Add(emailChannel.Cc);
                        if (!string.IsNullOrEmpty(emailChannel.Bcc))
                            msg.Bcc.Add(emailChannel.Bcc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, mbi.Client.AppCode, emailChannel.Subject);
                        msg.Priority = MailPriority.Normal;

                        if (mailPieces.Count > 0)
                        {
                            #region Prepare email body

                            string downloadEmail_TD_SrNoStyle = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string downloadEmail_TD_DescriptionStyle = "style='width:80%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt'' valign='top' width='213'><p>";
                            string downloadEmail_TD_CountStyle = "style='width:10%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";

                            StringBuilder sb = new StringBuilder(File.ReadAllText(Config.Monitor_PSEmailTemplate));
                            sb = sb.Replace("<%FileName%>", mbi.MailBatch.BatchName);
                            sb = sb.Replace("<%yyyymmdd%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("MM/dd/yyyy"));
                            sb = sb.Replace("<%hhmm%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("hh:mm tt"));

                            StringBuilder data = new StringBuilder();
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "1"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_DescriptionStyle, "TOTAL LOANS RECEIVED"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Count));
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "2"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", downloadEmail_TD_DescriptionStyle, "VALID LOANS"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Where(l => l.ErrorCode == OutComeCodes.No_Error).Count()));
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "3"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", downloadEmail_TD_DescriptionStyle, "INVALID LOANS"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error).Count()));

                            sb = sb.Replace("<%trdata%>", data.ToString());
                            msg.Body = sb.ToString();

                            #endregion
                        }
                        else if (mailPieces.Count == 0)
                        {
                            msg.Body = string.Format("No records available in the batch file {0}.", mbi.MailBatch.BatchName);
                        }

                        if (msg.Body.Length > 0)
                        {
                            if (reports.Count > 0)
                            {
                                foreach (var report in reports)
                                {
                                    msg.Attachments.Add(new Attachment(report));
                                }
                            }
                            smtpClient.Send(msg);
                            isEmailSent = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return isEmailSent;
        }

        public static bool SendSubscribedErrorNotificationEmail(MailBatchInfo mbi, List<MailPieces> mailPieces, string emailList)
        {
            bool isEmailSent = false;

            try
            {
                if (string.IsNullOrWhiteSpace(emailList))
                {
                    return true;
                }

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(emailList);
                        msg.CC.Add(Config.Common_ErrorSubscribedEmailCc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, mbi.Client.AppCode, Config.Common_ErrorSubscribedEmailSubject);
                        msg.Priority = MailPriority.High;

                        if (mailPieces.Count > 0)
                        {
                            #region Prepare email body

                            string downloadEmail_TD_SrNoStyle = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string downloadEmail_TD_DescriptionStyle = "style='width:45%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt'' valign='top' width='213'><p>";

                            StringBuilder sb = new StringBuilder(File.ReadAllText(Config.Common_ErrorSubscribedEmailTemplate));
                            sb = sb.Replace("<%FileName%>", mbi.MailBatch.BatchName);
                            sb = sb.Replace("<%yyyymmdd%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("MM/dd/yyyy"));
                            sb = sb.Replace("<%hhmm%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("hh:mm tt"));

                            var errorMPs = mailPieces.Where(mp => mp.ShippingStatus == ShippingStatus.Error.ToString());
                            StringBuilder errData = new StringBuilder();
                            foreach (var errMp in errorMPs)
                            {
                                errData.AppendLine("<tr><td {0}>{1}</td><td {2}>{3}</td><td{4}>{5}</td></tr>".Fill(downloadEmail_TD_SrNoStyle, errMp.BusinessFriendlyIdValue, downloadEmail_TD_DescriptionStyle, errMp.ErrorCode, downloadEmail_TD_DescriptionStyle, errMp.ErrorDesc));
                            }
                            sb = sb.Replace("<%trdata%>", errData.ToString());
                            msg.Body = sb.ToString();

                            #endregion
                        }

                        if (msg.Body.Length > 0)
                        {
                            smtpClient.Send(msg);
                            isEmailSent = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return isEmailSent;
        }

        public static bool SendNoDataNotificationEmail(MailBatchInfo mbi, EmailChannel emailChannel, string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                if (emailChannel == null)
                {
                    return true;
                }

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(emailChannel.To);
                        msg.CC.Add(emailChannel.Cc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, mbi.Client.AppCode, Config.Monitor_PSNoDataEmailSubject);
                        msg.Priority = MailPriority.Normal;

                        StringBuilder sb = new StringBuilder(File.ReadAllText(Config.Monitor_PSNoDataEmailTemplate));
                        sb = sb.Replace("<%FileName%>", mbi.MailBatch.BatchName);
                        sb = sb.Replace("<%yyyymmdd%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("MM/dd/yyyy"));
                        sb = sb.Replace("<%hhmm%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("hh:mm tt"));
                        sb = sb.Replace("<%Description%>", errorMsg);
                        msg.Body = sb.ToString();

                        if (msg.Body.Length > 0)
                        {
                            smtpClient.Send(msg);
                            isEmailSent = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return isEmailSent;
        }

        public static bool SendNoDataNotificationEmail(MailBatchInfo mbi, EmailChannel emailChannel, string errorMsg, string reportFilePath)
        {
            bool isEmailSent = false;

            try
            {
                if (emailChannel == null)
                {
                    return true;
                }

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(emailChannel.To);
                        msg.CC.Add(emailChannel.Cc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, mbi.Client.AppCode, Config.Monitor_PSNoDataEmailSubject);
                        msg.Priority = MailPriority.Normal;

                        StringBuilder sb = new StringBuilder(File.ReadAllText(Config.Monitor_PSNoDataEmailTemplate));
                        sb = sb.Replace("<%FileName%>", mbi.MailBatch.BatchName);
                        sb = sb.Replace("<%yyyymmdd%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("MM/dd/yyyy"));
                        sb = sb.Replace("<%hhmm%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("hh:mm tt"));
                        sb = sb.Replace("<%Description%>", errorMsg);
                        msg.Body = sb.ToString();

                        if (File.Exists(reportFilePath))
                        {
                            msg.Attachments.Add(new Attachment(reportFilePath));
                        }

                        if (msg.Body.Length > 0)
                        {
                            smtpClient.Send(msg);
                            isEmailSent = true;
                        }
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
