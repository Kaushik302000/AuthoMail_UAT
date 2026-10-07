using AM.Entities;
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
    class ErrorReports
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void GenerateFARErrorReport()
        {
            try
            {
                log.Debug($"FAR Client Error Report Execution Triggered at {Common.Now.UtcToEst()} Est");
                List<ReportWizard> mpDetails;
                DateTime currentTime = Common.EstToday;
                DateTime endDate = currentTime.Date.ToEndDateTime();
                DateTime startDate = currentTime.Date.ToStartDateTime();
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var client = clientSvc.GetClientByAppCode("FAR").Result;
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    mpDetails = reportSvc.GetReportWizard(new List<int>() { client.Id }, null, null, startDate, endDate, null, null, null, null, null);
                    var errorMps = mpDetails.Where(x => x.Status == ShippingStatus.Error.ToString()).ToList();
                    if (errorMps.Count == 0)
                    {
                        log.Debug($"FAR client Error Report does not contain any records so exiting from the process at {Common.Now.UtcToEst()} Est");
                        return;
                    }

                    string reportFilePath = Path.Combine(AppConfig.FARErrorReportPath, $"FARErrorReport_{startDate.ToString("MMddyyyy")}.csv");
                    GenerateReportForFAR(errorMps, reportFilePath, $"{Config.Common_Environment} : FAR Error Report For {startDate.ToString("MM-dd-yyyy")}");
                }
                log.Debug("FAR Client Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating FAR Error Report", ex);
            }
        }

        private static void GenerateReportForFAR(List<ReportWizard> mpDetails, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Batch Id", "MailPiece Id", "Channel", "Document Type", "Loan Number", "File Name", "Error Code", "Error Desc", "Received On (Est)", "First Name", "Last Name", "Street", "City", "State", "ZIP", "Shipping Method");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in mpDetails)
            {
                var line = String.Join(",", mp.BatchId.ToCsvString(), mp.MailPieceId, mp.Channel, mp.DocumentOrPackageType, mp.LoanNumber.ToCsvString(), mp.FileName, mp.ErrorCode, mp.ErrorDesc, mp.ReceivedOn.Value.UtcToEst(), mp.FirstName, mp.LastName, mp.RcptAddressStreet, mp.City, mp.State, mp.ZIP, mp.ShippingMethod);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToFARTeam(reportFilePath, subject, "FAR Error Report");
        }

        private static void SendReportToFARTeam(string reportFilePath, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.FARErrorEMailTo);
                        msg.CC.Add(AppConfig.FARErrorEMailCc);
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
                log.Error("Error occured while sending FAR Report :", ex);
            }
        }


        internal static void GenerateDocSolErrorReport()
        {
            try
            {
                log.Debug($"DocSol Client Error Report Execution Triggered at {Common.Now.UtcToEst()} Est");
                List<ReportWizard> mpDetails;
                DateTime currentTime = Common.EstToday;
                DateTime endDate = currentTime.Date.ToEndDateTime();
                DateTime startDate = currentTime.Date.ToStartDateTime();
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var client = clientSvc.GetClientByAppCode("DOCSOL").Result;
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    mpDetails = reportSvc.GetReportWizard(new List<int>() { client.Id }, null, null, startDate, endDate, null, null, null, null, null);
                    var errorMps = mpDetails.Where(x => x.Status == ShippingStatus.Error.ToString()).ToList();
                    if (errorMps.Count == 0)
                    {
                        log.Debug($"DocSol client Error Report does not contain any records so exiting from the process at {Common.Now.UtcToEst()} Est");
                        return;
                    }

                    string reportFilePath = Path.Combine(AppConfig.DocSolErrorReportPath, $"DocSolErrorReport_{startDate.ToString("MMddyyyy")}.csv");
                    GenerateReportForDocSol(errorMps, reportFilePath, $"{Config.Common_Environment} : DocSol Error Report For {startDate.ToString("MM-dd-yyyy")}");
                }
                log.Debug("DocSol Client Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating DocSol Error Report", ex);
            }
        }

        private static void GenerateReportForDocSol(List<ReportWizard> mpDetails, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Batch Id", "MailPiece Id", "Channel", "Document Type", "Loan Number", "File Name", "Error Code", "Error Desc", "Received On (Est)", "First Name", "Last Name", "Street", "City", "State", "ZIP", "Shipping Method");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in mpDetails)
            {
                var line = String.Join(",", mp.BatchId.ToCsvString(), mp.MailPieceId, mp.Channel, mp.DocumentOrPackageType, mp.LoanNumber.ToCsvString(), mp.FileName, mp.ErrorCode, mp.ErrorDesc, mp.ReceivedOn.Value.UtcToEst(), mp.FirstName, mp.LastName, mp.RcptAddressStreet, mp.City, mp.State, mp.ZIP, mp.ShippingMethod);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToDocSolTeam(reportFilePath, subject, "DocSol Error Report");
        }

        private static void SendReportToDocSolTeam(string reportFilePath, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.DocSolErrorEMailTo);
                        msg.CC.Add(AppConfig.DocSolErrorEMailCc);
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
                log.Error("Error occured while sending DocSol Report :", ex);
            }
        }
        internal static void GenerateBECUErrorReport()
        {
            try
            {
                log.Debug($"BECU Client Error Report Execution Triggered at {Common.Now.UtcToEst()} Est");
                List<ReportWizard> mpDetails;
                DateTime currentTime = Common.EstToday;
                DateTime endDate = currentTime.Date.ToEndDateTime();
                DateTime startDate = currentTime.Date.ToStartDateTime();
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var client = clientSvc.GetClientByAppCode("BECU").Result;
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    mpDetails = reportSvc.GetReportWizard(new List<int>() { client.Id }, null, null, startDate, endDate, null, null, null, null, null);
                    var errorMps = mpDetails.Where(x => x.Status == ShippingStatus.Error.ToString()).ToList();
                    if (errorMps.Count == 0)
                    {
                        log.Debug($"BECU client Error Report does not contain any records so exiting from the process at {Common.Now.UtcToEst()} Est");
                        return;
                    }

                    string reportFilePath = Path.Combine(AppConfig.BECUErrorReportPath, $"BECUErrorReport_{startDate.ToString("MMddyyyy")}.csv");
                    GenerateReportForBECU(errorMps, reportFilePath, $"{Config.Common_Environment} : BECU Error Report For {startDate.ToString("MM-dd-yyyy")}");
                }
                log.Debug("BECU Client Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating BECU Error Report", ex);
            }
        }
        private static void GenerateReportForBECU(List<ReportWizard> mpDetails, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Batch Id", "MailPiece Id", "Channel", "Document Type", "Loan Number", "File Name", "Error Code", "Error Desc", "Received On (Est)", "First Name", "Last Name", "Street", "City", "State", "ZIP", "Shipping Method");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in mpDetails)
            {
                var line = String.Join(",", mp.BatchId.ToCsvString(), mp.MailPieceId, mp.Channel, mp.DocumentOrPackageType, mp.LoanNumber.ToCsvString(), mp.FileName, mp.ErrorCode, mp.ErrorDesc, mp.ReceivedOn.Value.UtcToEst(), mp.FirstName, mp.LastName, mp.RcptAddressStreet, mp.City, mp.State, mp.ZIP, mp.ShippingMethod);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToBECUTeam(reportFilePath, subject, "BECU Error Report");
        }
        private static void SendReportToBECUTeam(string reportFilePath, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.BECUErrorEMailTo);
                        msg.CC.Add(AppConfig.BECUErrorEMailCc);
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
                log.Error("Error occured while sending BECU Report :", ex);
            }
        }
        internal static void GenerateRaymondJamesErrorReport()
        {
            try
            {
                log.Debug($"RaymondJames Client Error Report Execution Triggered at {Common.Now.UtcToEst()} Est");
                List<ReportWizard> mpDetails;
                DateTime currentTime = Common.EstToday;
                DateTime endDate = currentTime.Date.ToEndDateTime();
                DateTime startDate = currentTime.Date.ToStartDateTime();
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var client = clientSvc.GetClientByAppCode("RAYJAMES").Result;
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    mpDetails = reportSvc.GetReportWizard(new List<int>() { client.Id }, null, null, startDate, endDate, null, null, null, null, null);
                    var errorMps = mpDetails.Where(x => x.Status == ShippingStatus.Error.ToString()).ToList();
                    if (errorMps.Count == 0)
                    {
                        log.Debug($"RaymondJames client Error Report does not contain any records so exiting from the process at {Common.Now.UtcToEst()} Est");
                        return;
                    }

                    string reportFilePath = Path.Combine(AppConfig.RaymondJamesErrorReportPath, $"RaymondJamesErrorReport_{startDate.ToString("MMddyyyy")}.csv");
                    GenerateReportForRaymondJames(errorMps, reportFilePath, $"{Config.Common_Environment} : RaymondJames Error Report For {startDate.ToString("MM-dd-yyyy")}");
                }
                log.Debug("RaymondJames Client Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating RaymondJames Error Report", ex);
            }
        }

        private static void GenerateReportForRaymondJames(List<ReportWizard> mpDetails, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Batch Id", "MailPiece Id", "Channel", "Document Type", "Loan Number", "File Name", "Error Code", "Error Desc", "Received On (Est)", "First Name", "Last Name", "Street", "City", "State", "ZIP", "Shipping Method");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in mpDetails)
            {
                var line = String.Join(",", mp.BatchId.ToCsvString(), mp.MailPieceId, mp.Channel, mp.DocumentOrPackageType, mp.LoanNumber.ToCsvString(), mp.FileName, mp.ErrorCode, mp.ErrorDesc, mp.ReceivedOn.Value.UtcToEst(), mp.FirstName, mp.LastName, mp.RcptAddressStreet, mp.City, mp.State, mp.ZIP, mp.ShippingMethod);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToRaymondJamesTeam(reportFilePath, subject, "RaymondJames Error Report");
        }

        private static void SendReportToRaymondJamesTeam(string reportFilePath, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.RaymondJamesErrorEMailTo);
                        msg.CC.Add(AppConfig.RaymondJamesErrorEMailCc);
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
                log.Error("Error occured while sending RaymondJames Report :", ex);
            }
        }
    }
}
