using AM.Entities;
using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IAMCService : IDisposable
    {
        StringBuilder AMCErrorReport(List<MailPieces> mailPieces);
        StringBuilder AMCDeliveryReport(List<MailPieces> mailPieces);
    }

    public class AMCService : ServiceBase, IAMCService
    {
        const string Separator = "\t";

        public StringBuilder AMCErrorReport(List<MailPieces> mailPieces)
        {
            var columnName = String.Join(Separator, "BatchID", "ID", "Package Name", "Loan Number", "File Name", "Date Received", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Error Code", "Error Text", "Shipping Method", "Customer Identifier", "RequestID");

            StringBuilder errorReport = new StringBuilder(columnName);

            errorReport.AppendLine();

            var erroredData = mailPieces.Where(l => l.ShippingStatus == ShippingStatus.Error.ToString()).ToList();

            if (erroredData == null || erroredData.Count == 0)
            {
                return null;
            }

            using (DocTypeService docTypeSvc = new DocTypeService())
            {
                // All the loans where outcome code is anything except no_error are processed invalid loans.
                foreach (MailPieces mailPiece in erroredData)
                {
                    DocType docType = mailPiece.DocTypeId.HasValue ? docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value, mailPiece.PnSConfigDetailId.Value) : null;

                    var custId = mailPiece.MailPieceCustomInfoTest.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").SingleOrDefault();

                    var requestId = mailPiece.MailPieceCustomInfoTest.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").SingleOrDefault();

                    var line = String.Join(Separator,
                       mailPiece.MailBatches.BatchName == null ? string.Empty : mailPiece.MailBatches.BatchName,
                        mailPiece.ErrorCode == OutComeCodes.Invalid_DuplicateKey ? Guid.Parse(mailPiece.ErrorDesc.Substring(0, mailPiece.ErrorDesc.IndexOf(" ")).Trim()) : mailPiece.UniqueId,
                            mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                            mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                            mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                            mailPiece.MailBatches.ReceivedOn == null ? string.Empty : mailPiece.MailBatches.ReceivedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                            mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                            mailPiece.RcptAddressStreet == null ? string.Empty : string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther) ? mailPiece.RcptAddressStreet : mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddressAptOrSuiteOrOther,
                            mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                            mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                            mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                            OutComeCodes.GetAMCMappingCode(mailPiece.ErrorCode),
                            mailPiece.ErrorDesc == null ? string.Empty : mailPiece.ErrorDesc.Replace("\r\n", " "),
                            docType == null ? string.Empty : docType.ShippingCarrier,
                            custId == null ? string.Empty : custId.Value,
                            requestId == null ? string.Empty : requestId.Value
                        );

                    errorReport.AppendLine(line);
                }
            }

            return errorReport;
        }

        public StringBuilder AMCDeliveryReport(List<MailPieces> mailPieces)
        {
            var columnName = String.Join(Separator, "BatchID", "ID", "Package Name", "Loan Number", "File Name", "Date Received", "Date Printed", "Date Mailed", "Client Code", "Tracking Number", "Shipping Method", "Customer Identifier", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "RequestID", "Pages Shipped");

            StringBuilder deliveryReport = new StringBuilder(columnName);

            deliveryReport.AppendLine();

            var shippedData = mailPieces.Where(l => l.ShippingStatus == ShippingStatus.Shipped.ToString() || l.ShippingStatus == ShippingStatus.eDelivered.ToString() || l.ShippingStatus == ShippingStatus.PulledBack.ToString()).ToList();

            if (shippedData == null || shippedData.Count == 0)
            {
                return null;
            }

            using (DocTypeService docTypeSvc = new DocTypeService())
            {
                // All the loans where outcome code is anything except no_error are processed invalid loans.
                foreach (MailPieces mailPiece in shippedData)
                {
                    var shippingCredCode = mailPiece.MailPieceCustomInfoTest.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingCredCode").SingleOrDefault();

                    var custId = mailPiece.MailPieceCustomInfoTest.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").SingleOrDefault();

                    var requestId = mailPiece.MailPieceCustomInfoTest.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").SingleOrDefault();

                    var line = String.Join(Separator,
                        mailPiece.MailBatches.BatchName == null ? string.Empty : mailPiece.MailBatches.BatchName,
                            mailPiece.UniqueId,
                            mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                            mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                            mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                            mailPiece.MailBatches.ReceivedOn == null ? string.Empty : mailPiece.MailBatches.ReceivedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? Config.Monitor_AMCPulledBackDate : mailPiece.PrintedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? Config.Monitor_AMCPulledBackDate : mailPiece.ShippedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            shippingCredCode == null ? string.Empty : shippingCredCode.Value,
                            mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? string.Empty : mailPiece.TrackingNo,
                            mailPiece.DocTypes == null ? string.Empty : mailPiece.PnSConfigDetails.ShippingCarrier,
                            custId == null ? string.Empty : custId.Value,
                            mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                            mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                            mailPiece.RcptAddressStreet == null ? string.Empty : string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther) ? mailPiece.RcptAddressStreet : mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddressAptOrSuiteOrOther,
                            mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                            mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                            mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                            requestId == null ? string.Empty : requestId.Value,
                            mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? 0 : mailPiece.TotalPages);

                    deliveryReport.AppendLine(line);
                }
            }
            return deliveryReport;
        }
    }
}
