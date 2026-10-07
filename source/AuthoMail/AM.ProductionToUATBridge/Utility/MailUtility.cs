using AM.Lib;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AM.ProductionToUATBridge
{
    public class MailUtility
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MailUtility));

        public static void SendUnexpectedExceptionEmail(Exception exception, string messageString)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(Configuration.Monitor_Exception_Email_To);
                        msg.CC.Add(Configuration.Monitor_Exception_Email_Cc);
                        msg.Subject = "{0}".Fill(Configuration.Monitor_ExceptionEmail_Subject);
                        msg.Priority = MailPriority.High;
                        msg.Body = "Unexpected exception occured during execution of Production and UAT bridge application : " + Environment.NewLine + messageString + Environment.NewLine + exception;
                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
