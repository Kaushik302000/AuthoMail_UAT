using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    public class MailUtility
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MailUtility));
        public static bool SendBatchSubmittedNotificationEmail(string vendorCode, string printVendorCode, string fileName, int mailPiecesCount, int totalPages)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    StringBuilder sb;
                    msg.IsBodyHtml = true;
                    if (vendorCode == "NORDIS")
                    {
                        if (printVendorCode == "DEL_NORDIS")
                        {
                            msg.To.Add(Common.Email_ToDELNordis);
                            msg.CC.Add(Common.Email_CcDELNordis);
                            //Here subject string contains "Batch Submitted To {Location} : {FileName} at {Time} Total Files : {FileCount} Total Page Count : {PageCount}"
                            msg.Subject = Common.ApplicationEnvironment + " : " + Common.SubmitDELNordisBatchEmailSubject.Fill(vendorCode, Path.GetFileName(fileName), Common.UtcToEst(Common.GetCurrentTime()).ToString("MM/dd/yyyy hh:mm tt") + " EST", mailPiecesCount, totalPages);
                            sb = new StringBuilder(File.ReadAllText(Common.SubmitDELNordisBatchEmailTemplate));
                        }
                        else if (printVendorCode == "CSS_NORDIS")
                        {
                            msg.To.Add(Common.Email_ToCSSNordis);
                            msg.CC.Add(Common.Email_CcCSSNordis);
                            //Here subject string contains "Batch Submitted To {Location} : {FileName} at {Time} Total Files : {FileCount} Total Page Count : {PageCount}"
                            msg.Subject = Common.ApplicationEnvironment + " : " + Common.SubmitCSSNordisBatchEmailSubject.Fill(vendorCode, Path.GetFileName(fileName), Common.UtcToEst(Common.GetCurrentTime()).ToString("MM/dd/yyyy hh:mm tt") + " EST", mailPiecesCount, totalPages);
                            sb = new StringBuilder(File.ReadAllText(Common.SubmitCSSNordisBatchEmailTemplate));
                        }
                        else
                        {
                            msg.To.Add(Common.Email_ToNordis);
                            msg.CC.Add(Common.Email_CcNordis);
                            //Here subject string contains "Batch Submitted To {Location} : {FileName} at {Time} Total Files : {FileCount} Total Page Count : {PageCount}"
                            msg.Subject = Common.ApplicationEnvironment + " : " + Common.SubmitNordisBatchEmailSubject.Fill(vendorCode, Path.GetFileName(fileName), Common.UtcToEst(Common.GetCurrentTime()).ToString("MM/dd/yyyy hh:mm tt") + " EST", mailPiecesCount, totalPages);
                            sb = new StringBuilder(File.ReadAllText(Common.SubmitNordisBatchEmailTemplate));
                        }
                    }
                    else
                    {
                        msg.To.Add(Common.Email_ToPCI);
                        msg.CC.Add(Common.Email_CcPCI);
                        //Here subject string contains "Batch Submitted To {Location} : {FileName} at {Time} Total Files : {FileCount} Total Page Count : {PageCount}"
                        msg.Subject = Common.ApplicationEnvironment + " : " + Common.SubmitPCIBatchEmailSubject.Fill(vendorCode, Path.GetFileName(fileName), Common.UtcToEst(Common.GetCurrentTime()).ToString("MM/dd/yyyy hh:mm tt") + " EST", mailPiecesCount, totalPages);
                        sb = new StringBuilder(File.ReadAllText(Common.SubmitPCIBatchEmailTemplate));
                    }
                    msg.Priority = MailPriority.Normal;
                    sb = sb.Replace("<%FileName%>", Path.GetFileName(fileName));
                    sb = sb.Replace("<%PrintLocation%>", vendorCode);
                    sb = sb.Replace("<%yyyymmdd%>", Common.UtcToEst(Common.GetCurrentTime()).ToString("MM/dd/yyyy"));
                    sb = sb.Replace("<%hhmm%>", Common.UtcToEst(Common.GetCurrentTime()).ToString("hh:mm tt") + " EST");
                    sb = sb.Replace("<%TotalMailPieces%>", mailPiecesCount.ToString());
                    sb = sb.Replace("<%TotalPageCount%>", totalPages.ToString());

                    msg.Body = sb.ToString();
                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending batch submitted notification to PCI", ex);
            }

            return isEmailSent;
        }
        public static bool SendBatchSuspendedNotificationInternalEmail(int printBatchId, string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Common.SuspendEmail_To);
                    msg.CC.Add(Common.SuspendEmail_Cc);
                    msg.Subject = Common.ApplicationEnvironment + " : " + Common.SuspendBatchEmailSubject + printBatchId;
                    msg.Priority = MailPriority.Normal;

                    StringBuilder sb = new StringBuilder(File.ReadAllText(Common.SuspendBatchEmailTemplate));
                    sb = sb.Replace("<%Reason%>", errorMsg);

                    msg.Body = sb.ToString();
                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending batch submitted notification to PCI", ex);
            }

            return isEmailSent;
        }

        public static bool SendInvalidResponseFoundNotification(string printFileName, string fileName, string errorMsg, string printVendorCode)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(printVendorCode == "PCI" ? Common.PCI_Issue_Email_To : Common.NORDIS_Issue_Email_To);
                    msg.CC.Add(printVendorCode == "PCI" ? Common.PCI_Issue_Email_Cc : Common.NORDIS_Issue_Email_Cc);
                    msg.Subject = Common.ApplicationEnvironment + " : " + printVendorCode == "PCI" ? Common.PCI_Issue_Subject : Common.NORDIS_Issue_Subject;
                    msg.Priority = MailPriority.High;

                    StringBuilder sb = new StringBuilder(File.ReadAllText(Common.SuspendBatchEmailTemplate));
                    sb = sb.Replace("<%Reason%>", errorMsg + "<br/><br/>Batch Name: {0} <br/><br/>".Fill(Path.GetFileNameWithoutExtension(printFileName)) + " <br/>File Name : {0}".Fill(fileName));

                    msg.Body = sb.ToString();
                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending batch submitted notification to PCI", ex);
            }

            return isEmailSent;
        }

        public static bool SendUnexpectedNotificationEmail(string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Common.UnexpectedEmail_ToEmail);
                    msg.CC.Add(Common.UnexpectedEmail_CcEmail);
                    msg.Subject = Common.UnexpectedEmail_Subject;
                    msg.Priority = MailPriority.High;

                    msg.Body = errorMsg;
                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending batch submitted notification to internal team", ex);
            }

            return isEmailSent;
        }

        public static bool SendUnexpectedNotificationEmailInternal(string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Common.UnexpectedEmailInternal_ToEmail);
                    msg.Subject = Common.UnexpectedEmail_Subject;
                    msg.Priority = MailPriority.High;

                    msg.Body = errorMsg;
                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending batch submitted notification to internal team", ex);
            }

            return isEmailSent;
        }



        public static bool SendSFTConnectionIssueNotificationEmail(string errorMsg, string pvCode)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(pvCode == "PCI" ? Common.Email_ToPCI : pvCode.Contains("NORDIS") ? Common.Email_ToNordis : Common.UnexpectedEmail_ToEmail);
                    msg.CC.Add(Common.UnexpectedEmail_CcEmail + "," + Common.UnexpectedEmail_ToEmail);
                    msg.Subject = Common.ApplicationEnvironment + " : {0} SFT Site Connection issue".Fill(pvCode);
                    msg.Priority = MailPriority.High;

                    msg.Body = errorMsg;
                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending batch submitted notification to internal team", ex);
            }

            return isEmailSent;
        }
    }
}
