using AM.Entities;
using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IAlaskaService : IDisposable
    {
        StringBuilder AlaskaErrorReport(List<MailPieces> mailPieces, string clientTimeZone, string delimeter);
        StringBuilder AlaskaDeliveryReport(List<MailPieces> mailPieces, string clientTimeZone, string delimeter);
    }

    public class AlaskaService : ServiceBase, IAlaskaService
    {
        public StringBuilder AlaskaErrorReport(List<MailPieces> mailPieces, string clientTimeZone, string delimeter)
        {
            var columnName = String.Join(delimeter, "BatchID", "Loan Number", "File Name", "Date Received", "Recipient FirstName", "Recipient LastName", "Recipient Street Address", "Recipient Street Address2", "Recipient City", "Recipient State", "Recipient Zip", "Error Code", "Error Text");

            StringBuilder errorReport = new StringBuilder(columnName);

            errorReport.AppendLine();

            var erroredData = mailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error).ToList();

            if (erroredData == null || erroredData.Count == 0)
            {
                return null;
            }


            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (MailPieces mailPiece in erroredData)
            {
                var line = String.Join(delimeter,
                    mailPiece.MailBatches.BatchName == null ? string.Empty : mailPiece.MailBatches.BatchName.ToCsvString(),
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName.ToCsvString(),
                        mailPiece.MailBatches.ReceivedOn == null ? string.Empty : mailPiece.MailBatches.ReceivedOn.Value.UtcToClientTimeZone(clientTimeZone).ToString(),
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName.ToCsvString(),
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName.ToCsvString(),
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet.ToCsvString(),
                        mailPiece.RcptAddressAptOrSuiteOrOther == null ? string.Empty : mailPiece.RcptAddressAptOrSuiteOrOther.ToCsvString(),
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        OutComeCodes.GetAlaskaMappingCode(mailPiece.ErrorCode),
                        mailPiece.ErrorDesc == null ? string.Empty : mailPiece.ErrorDesc.Replace("\r\n", " ").ToCsvString()
                    );
                errorReport.AppendLine(line);
            }

            return errorReport;
        }

        public StringBuilder AlaskaDeliveryReport(List<MailPieces> mailPieces, string clientTimeZone, string delimeter)
        {
            var columnName = String.Join(delimeter, "BatchID", "Loan Number", "File Name", "Date Received", "Date Printed", "Date Mailed", "Recipient FirstName", "Recipient LastName", "Recipient Street Address", "Recipient Street Address2", "Recipient City", "Recipient State", "Recipient Zip", "Pages Shipped");

            StringBuilder deliveryReport = new StringBuilder(columnName);

            deliveryReport.AppendLine();

            var shippedData = mailPieces.Where(l => l.ShippingStatus == ShippingStatus.Shipped.ToString() || l.ShippingStatus == ShippingStatus.PulledBack.ToString()).ToList();

            if (shippedData == null || shippedData.Count == 0)
            {
                return null;
            }

            foreach (MailPieces mailPiece in shippedData)
            {
                var line = String.Join(delimeter,
                    mailPiece.MailBatches.BatchName == null ? string.Empty : mailPiece.MailBatches.BatchName.ToCsvString(),
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName.ToCsvString(),
                        mailPiece.MailBatches.ReceivedOn == null ? string.Empty : mailPiece.MailBatches.ReceivedOn.Value.UtcToClientTimeZone(clientTimeZone).ToString(),
                        mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? string.Empty : mailPiece.PrintedOn.Value.UtcToClientTimeZone(clientTimeZone).ToString(),
                        mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? string.Empty : mailPiece.ShippedOn.Value.UtcToClientTimeZone(clientTimeZone).ToString(),
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName.ToCsvString(),
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName.ToCsvString(),
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet.ToCsvString(),
                        mailPiece.RcptAddressAptOrSuiteOrOther == null ? string.Empty : mailPiece.RcptAddressAptOrSuiteOrOther.ToCsvString(),
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? 0 : mailPiece.TotalPages);

                deliveryReport.AppendLine(line);
            }
            return deliveryReport;
        }
    }
}
