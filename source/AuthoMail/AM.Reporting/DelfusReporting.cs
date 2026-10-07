using AM.Lib;
using AM.Services;
using AM.Services.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class DelfusReporting
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateErrorReport()
        {
            try
            {
                log.Debug("TaxCommercial Error Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<MailPieces> errorReportData;
                List<DelfusErrorReport> taxcomerrorReportData;
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                using (IDelfusTemplateService delfusTemplateService = new DelfusTemplateService())
                {
                    var clientInfo = clientSvc.GetClientByAppCode("TAXCOM").Result;
                    DateTime utcNow = DateTime.UtcNow;
                    DateTime startDate = utcNow.UtcToEst().ToStartDateTime().EstToUtc();
                    DateTime endDate = utcNow.UtcToEst().ToEndDateTime().EstToUtc();
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    errorReportData = reportSvc.GetErrorReportByDateOrBatch(clientInfo.Id, startDate, endDate, null, "");
                    taxcomerrorReportData = delfusTemplateService.GetTaxCommercialErrorData(startDate, endDate).Result.ToList();

                    if (taxcomerrorReportData.Count > 0 || errorReportData.Count > 0)
                    {
                        GenerateErrorReport(errorReportData, taxcomerrorReportData, startDate, endDate);
                    }
                }
                log.Debug("TaxCommercial Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating TaxCommercial Error Report", ex);
            }
        }

        private static void GenerateErrorReport(List<MailPieces> errorReportData, List<DelfusErrorReport> taxcomerrorReportData, DateTime startDate, DateTime endDate)
        {
            string subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.TaxComErrorReportEmailSubject.Fill($"{ startDate.ToString("MMddyyyy")}_To_{ endDate.ToString("MMddyyyy")}"));

            string body = $"TaxCommercial Error Report For : { startDate.ToString("MMddyyyy")}_To_{endDate.ToString("MMddyyyy")}";

            var columnName = String.Join(",", "ClientID", "Loan Number", "Document Type", "Delivery Type", "FileName", "Error Reason", "Receive Date", "Recipient Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();

            foreach (var mp in taxcomerrorReportData)
            {
                var line = String.Join(",", mp.RD_CUST_ID, mp.RD_LOAN_ID.ToCsvString(), mp.DocumentType, "", mp.PdfFileName, mp.DisqualifyReason.ToCsvString(), mp.DownloadedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss"), mp.RecipientName, mp.RD_SITUS_STR_NM, mp.RD_SITUS_CITY_NM, mp.RD_SITUS_STATE_ABBR, mp.RD_SITUS_ZIP_CD);
                reconReport.AppendLine(line);
            }

            foreach (var mp in errorReportData)
            {
                var line = String.Join(",", "", mp.BusinessFriendlyIdValue.ToCsvString(), mp.DocumentOrPackageType, mp.DocTypes.ShippingServiceType, mp.FileName, mp.ErrorDesc, mp.MailBatches.ReceivedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss"), mp.RcptName, mp.RcptAddressStreet, mp.RcptAddressCity, mp.RcptAddressState, mp.RcptAddressZIP);
                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.TaxComErrorReportPath, $"TaxCommercialErrorReport_{startDate.ToString("MMddyyyy")}_To_{ endDate.ToString("MMddyyyy")}.csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToTaxCommercialTeam(reportFilePath, subject, body, AppConfig.TaxComEmailTo, AppConfig.TaxComEmailCc);
        }

        internal static void GenerateTaxComEdeliveryReport(string reportType)
        {
            try
            {
                log.Debug($"TaxCommercial Edelivery {reportType} Report Execution Triggered at {Common.Now.UtcToEst()} Est");
                List<MonthlyReconReportViewModel> monthlyReconReportViewModels;
                List<DelfusEdeliveryReport> taxcomEdeliveryReportData;

                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                using (IDelfusTemplateService delfusTemplateService = new DelfusTemplateService())
                {
                    var clientInfo = clientSvc.GetClientByAppCode("TAXCOM").Result;

                    DateTime utcNow = DateTime.UtcNow;
                    //By Default consider report type Daily
                    DateTime startDate = utcNow.UtcToEst().ToStartDateTime().EstToUtc();
                    DateTime endDate = utcNow.UtcToEst().ToEndDateTime().EstToUtc();

                    if (reportType == "Monthly")
                    {
                        var previousMonth = Common.EstToday.AddMonths(-1);
                        startDate = new DateTime(previousMonth.Year, previousMonth.Month, 1).ToStartDateTime().EstToUtc();
                        endDate = new DateTime(previousMonth.Year, previousMonth.Month, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month)).ToEndDateTime().EstToUtc();
                    }

                    monthlyReconReportViewModels = reportSvc.GetMonthlyReconReport(clientInfo.Id, startDate, endDate, null);
                    taxcomEdeliveryReportData = delfusTemplateService.GetTaxCommercialEDeliveryData(startDate, endDate).Result.ToList();

                    if (monthlyReconReportViewModels.Count > 0 || taxcomEdeliveryReportData.Count > 0)
                    {
                        GenerateEdeliveryReport(monthlyReconReportViewModels, taxcomEdeliveryReportData, startDate.UtcToEst(), endDate.UtcToEst(), reportType);
                    }
                }
                log.Debug($"TaxCommercial Edelivery {reportType} Report Execution Completed at {Common.Now.UtcToEst()} Est");
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating TaxCommercial Edelivery Report", ex);
            }
        }

        private static void GenerateEdeliveryReport(List<MonthlyReconReportViewModel> monthlyReconReportViewModels, List<DelfusEdeliveryReport> taxcomEdeliveryReportData, DateTime startDate, DateTime endDate, string reportType)
        {
            string subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.TaxComEDeliveryReportEmailSubject.Fill($"{ startDate.ToString("MMddyyyy")}_To_{ endDate.ToString("MMddyyyy")}"));

            string body = $"TaxCommercial {reportType} Report For : { startDate.ToString("MMddyyyy")}_To_{endDate.ToString("MMddyyyy")}";

            var columnName = String.Join(",", "AppCode", "BatchName", "UniqueId", "LoanId", "FirstName", "LastName", "Address", "City", "State", "ZIP", "Email Address", "Channel", "DocType", "ReceivedOn", "ShippedOn", "EDeliveredOn", "PulledBackOn", "PrintLocation", "Status", "TrackingNo", "FileName", "TemplateId", "TemplateCode", "TotalPages", "TotalSheets", "LetterPageCount", "LegalPageCount", "TotalBlankPages", "IsEDeliveryRequested", "ShippingMethod", "ShippingCarrier", "ShippingServiceType", "EnvelopeName", "ErrorDesc", "Inserts", "PrintBatchName", "SubmittedOn", "DeliveryDate", "DeliveryStatus", "ShippingCost", "BlankPageNumbers", "LetterPagesImpression", "LegalPagesImpression");

            //var columnName = String.Join(",", "BatchName", "SubmittedDate", "Cust ID", "Template Code", "EDelivery Status", "EDeliveryStatus Date", "Email Address", "Recipient First Name", "Recipient Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "PdfFileName", "DocumentType", "RecordStatus", "Disqualifyreason");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();

            foreach (var mp in monthlyReconReportViewModels)
            {
                var line = String.Join(",", "TAXCOM", mp.BatchName, mp.UniqueId, mp.LoanNumber, mp.FirstName.ToCsvString(), mp.LastName.ToCsvString(), mp.Address.ToCsvString(), mp.City, mp.State, mp.ZIP, "", mp.Channel, mp.DocType,
                mp.ReceivedOn.HasValue ? mp.ReceivedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss") : "N/A",
                mp.ShippedOn.HasValue ? mp.ShippedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss") : "N/A",
                "", mp.PulledBackOn.HasValue ? mp.PulledBackOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss") : "N/A", mp.PrintLocation, mp.Status, mp.TrackingNo, mp.FileName, "", "",
                mp.TotalPages, mp.TotalSheets, mp.LetterPageCount, mp.LegalPageCount, mp.TotalBlankPages, "False", mp.ShippingMethod, mp.ShippingCarrier, mp.ShippingServiceType, mp.EnvelopeName, mp.ErrorDesc, mp.Inserts,
                mp.PrintBatchName, mp.SubmittedOn, mp.DeliveryDate, mp.DeliveryStatus, mp.ShippingCost, mp.BlankPageNumbers, mp.LetterPagesImpression, mp.LegalPagesImpression);

                reconReport.AppendLine(line);
            }

            foreach (var mp in taxcomEdeliveryReportData)
            {
                var line = String.Join(",", "TAXCOM", mp.BatchName, "", mp.RD_LOAN_ID, mp.RD_BRRW_FRST_NM.ToCsvString(), mp.RD_BRRW_LAST_NM.ToCsvString(), mp.RD_BRRW_ADDR1_TXT.ToCsvString(), mp.RD_BRRW_CITY_NM,
                mp.RD_BRRW_STATE_ABBR, mp.RD_BRRW_ZIP_CD, mp.RD_DLVR_EMAIL_ADDR_TXT, "", mp.DocumentType,
                mp.DownloadedOn.HasValue ? mp.DownloadedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss") : "N/A",
                "", mp.EDeliveryStatus == "DELIVERED" && mp.EDeliveryStatusDate.HasValue ? mp.EDeliveryStatusDate.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss") : "N/A", "", "",
                !mp.Disqualifyreason.IsNullOrEmpty() || mp.EDeliveryStatus == "BOUNCED" ? "Error" : mp.EDeliveryStatus, "", mp.PdfFileName, mp.RD_CUST_ID, mp.RD_TMPLT_CD, "0", "0", "0", "0", "0", mp.IsEDeliveryRecord,
                "EDelivery", "", "", "", mp.EDeliveryStatus == "BOUNCED" && mp.RecordStatus != "Disqualified" ? "BOUNCED" : mp.Disqualifyreason.ToCsvString(), "", "", mp.SubmittedDate, "", "", "", "", "", "");

                reconReport.AppendLine(line);
            }

            string reportFilePath = Path.Combine(AppConfig.TaxComEDeliveryReportPath, $"TaxCommercial{reportType}Report_{startDate.ToString("MMddyyyy")}_To_{ endDate.ToString("MMddyyyy")}.csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToTaxCommercialTeam(reportFilePath, subject, body, AppConfig.TaxComEDeliveryReportEmailTo, AppConfig.TaxComEDeliveryReportEmailCc);
        }

        private static void SendReportToTaxCommercialTeam(string reportFilePath, string subject, string body, string emailTo, string emailCc)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(emailTo);
                        msg.CC.Add(emailCc);
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
                log.Error("Error occured while sending TaxCommercial Report :", ex);
            }
        }
    }
}
