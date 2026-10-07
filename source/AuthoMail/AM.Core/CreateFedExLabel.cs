using AM.Core.ShipWebServiceRef;
using AM.Entities;
using AM.Services;
using AM.Lib;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace AM.Core
{
    class CreateFedExLabel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GeneratedFedExLabelPath { get; private set; }

        public SmartDocResponse Create(MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, Client client, DocType docType, MailBatchInfo mbi, ShipperInformation shipperInfo, bool isReturnLabel = false)
        {
            SmartDocResponse response = new SmartDocResponse();
            //Here we are assigning No_Error because if we have any general failure or end point exception then we are trying again.
            mailPiece.ErrorCode = OutComeCodes.No_Error;
            mailPiece.ErrorDesc = null;
            mailPiece.ShippingStatus = ShippingStatus.InProgress.ToString();

            ProcessShipmentRequest request = CreateShipmentRequest(mailPiece, fedExAccInfo, client, docType, shipperInfo, isReturnLabel);
            // Initialize the service
            ShipPortTypeClient service = new ShipPortTypeClient();

            // For production account we always need to use production URL. Devang sir clarified that until the shipping label barcode is scanned by FedEx we will not get billed. So, if needed for UAT also we can use production URL.

            service.Endpoint.Address = new System.ServiceModel.EndpointAddress(Config.Core_FedExService);

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                // Call the ship web service passing in a shipment request and returning a shipment reply
                ProcessShipmentReply reply = service.processShipment(request);
                // check if the call was successful
                if ((reply.HighestSeverity != NotificationSeverityType.ERROR) && (reply.HighestSeverity != NotificationSeverityType.FAILURE))
                {
                    GenerateShipmentLabel(reply, mbi, ref response);
                }
                else
                {
                    //If we get saturday delivery failed then we just remove that special handling parameters.
                    if (reply.Notifications[0].Code == "8272" || reply.Notifications[0].Message.Contains("Saturday Delivery is not allowed") || reply.Notifications[0].Message.Contains("SATURDAY_DELIVERY is not allowed"))
                    {
                        log.Error("MailPieceId - {0} does not support saturday delivery.".Fill(mailPiece.Id));
                        request.RequestedShipment.SpecialServicesRequested = null;
                        reply = service.processShipment(request);

                        if ((reply.HighestSeverity != NotificationSeverityType.ERROR) && (reply.HighestSeverity != NotificationSeverityType.FAILURE))
                        {
                            GenerateShipmentLabel(reply, mbi, ref response);
                            return response;
                        }
                    }

                    List<Notification> notificaitons = new List<Notification>(reply.Notifications);
                    StringBuilder sbMessages = new StringBuilder(string.Format("MailPiece Id : {0} Error occurred while generating FedEx label for client {1} and {2} : {3}<br/>", mailPiece.Id, mbi.Client.AppCode, mailPiece.BusinessFriendlyIdName, mailPiece.BusinessFriendlyIdValue));
                    notificaitons.ForEach(n => sbMessages = sbMessages.AppendLine(n.Message));

                    log.Error(sbMessages.ToString());

                    response.HasError = true;
                    response.CoverLetterPath = "";
                    response.ExceptionCode = reply.Notifications[0].Code;
                    response.ExceptionDetail = reply.Notifications[0].Message;
                }
            }
            catch (Exception ex)
            {
                var error = "Error occured while generating FedEx label for MailPieceId : {0} And Exception : {1}".Fill(mailPiece.Id, ex);
                log.Error(error);

                SendUnexpectedExceptionEmail(error);

                response.HasError = true;
                response.CoverLetterPath = "";
                response.ExceptionCode = OutComeCodes.Unknown_Error;
                response.ExceptionDetail = string.Format("{0}|{1}", OutComeCodes.Unknown_Error, ex.Message);
            }

            return response;
        }

        private void SendUnexpectedExceptionEmail(string exception)
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
                        msg.Body = "Unexpected exception occured during generating FedEx label :" + Environment.NewLine + exception;
                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private ProcessShipmentRequest CreateShipmentRequest(MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, Client client, DocType docType, ShipperInformation shipperInfo, bool isReturnLabel)
        {
            // Build the ShipmentRequest
            ProcessShipmentRequest shipRequest = new ProcessShipmentRequest();
            SetShipmentDetails(shipRequest, mailPiece, fedExAccInfo, docType, shipperInfo, isReturnLabel);
            SetPackageLineItems(shipRequest, mailPiece, client);

            if (!string.IsNullOrEmpty(docType.ShippingSpecialService))
            {
                SetSpecialServiceRequestLineItems(shipRequest, docType.ShippingSpecialService, mailPiece.EffectiveReceivedOnEst.Value);
            }
            return shipRequest;
        }

        private void SetShipmentDetails(ProcessShipmentRequest request, MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, DocType docType, ShipperInformation shipperInfo, bool isReturnLabel)
        {
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = fedExAccInfo.WebAuthenticationKey;
            request.WebAuthenticationDetail.UserCredential.Password = fedExAccInfo.WebAuthenticationPassword; // Replace "XXX" with the Password
            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = fedExAccInfo.AccountNumber; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = fedExAccInfo.MeterNumber; // Replace "XXX" with the client's meter number

            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = mailPiece.Id.ToString(); // The client will get the same value back in the response
            //
            request.Version = new VersionId(); // WSDL version information, value is automatically set from wsdl
            //          
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = mailPiece.EffectiveReceivedOnEst.Value; // Ship date and time
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP;

            if (isReturnLabel)
                request.RequestedShipment.ServiceType = docType.ReturnShippingServiceType.ToString();
            else
                request.RequestedShipment.ServiceType = docType.ShippingServiceType.ToString();

            // For FedEx GROUND packages the packaging type must be "YOUR_PACKAGING".
            if (request.RequestedShipment.ServiceType == ShippingServiceType.FEDEX_GROUND.ToString() || request.RequestedShipment.ServiceType == ShippingServiceType.GROUND_HOME_DELIVERY.ToString())
            {
                request.RequestedShipment.PackagingType = "YOUR_PACKAGING";
            }
            else
            {
                request.RequestedShipment.PackagingType = "FEDEX_ENVELOPE";
            }

            // Packaging type FEDEX_BOK, FEDEX_PAK, FEDEX_TUBE, YOUR_PACKAGING, ...
            request.RequestedShipment.TotalWeight = new Weight(); // Total weight information
            request.RequestedShipment.TotalWeight.Value = 1.0M;
            request.RequestedShipment.TotalWeight.Units = WeightUnits.LB;
            request.RequestedShipment.RateRequestTypes = new RateRequestType[1] { RateRequestType.LIST }; // Rate types requested LIST, MULTIWEIGHT, ...
            request.RequestedShipment.PackageCount = "1";

            SetSender(request, shipperInfo, mailPiece, isReturnLabel);
            //
            SetRecipient(request, mailPiece, docType, isReturnLabel);
            //
            SetPayment(request, fedExAccInfo);
            //
            SetLabelDetails(request, mailPiece, isReturnLabel);
        }

        private void SetPayment(ProcessShipmentRequest request, FedExAccountInfo fedExAccInfo)
        {
            request.RequestedShipment.ShippingChargesPayment = new Payment(); // Payment information
            request.RequestedShipment.ShippingChargesPayment.PaymentType = PaymentType.SENDER;
            request.RequestedShipment.ShippingChargesPayment.Payor = new Payor();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty = new Party();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.AccountNumber = fedExAccInfo.AccountNumber; // Replace "XXX" with the payor account number
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Address = new ShipWebServiceRef.Address();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Address.CountryCode = Config.Core_CountryCode;
        }

        private void SetLabelDetails(ProcessShipmentRequest request, MailPieceDTO mailPiece, bool isReturnLabel)
        {
            request.RequestedShipment.LabelSpecification = new LabelSpecification(); // Label specification
            request.RequestedShipment.LabelSpecification.ImageType = ShippingDocumentImageType.PNG;
            request.RequestedShipment.LabelSpecification.ImageTypeSpecified = true;
            request.RequestedShipment.LabelSpecification.LabelFormatType = LabelFormatType.COMMON2D;
            request.RequestedShipment.LabelSpecification.LabelStockType = LabelStockType.PAPER_4X6;
            request.RequestedShipment.LabelSpecification.LabelStockTypeSpecified = true;
            request.RequestedShipment.LabelSpecification.LabelPrintingOrientation = LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST;
            request.RequestedShipment.LabelSpecification.LabelPrintingOrientationSpecified = true;

            if (isReturnLabel)
            {
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin = new ContactAndAddress();
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Contact = new Contact();
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Contact.PersonName = string.Format("{0} {1}", mailPiece.RcptFirstName, mailPiece.RcptLastName);
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Contact.CompanyName = mailPiece.RcptOrganization;
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Contact.PhoneNumber = string.IsNullOrEmpty(mailPiece.RcptPhone) ? "0000000000" : mailPiece.RcptPhone;

                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address = new ShipWebServiceRef.Address();
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.StreetLines = new string[] { mailPiece.RcptAddressStreet, mailPiece.RcptAddrOptionalLine1BelowStreet, mailPiece.RcptAddrOptionalLine2BelowStreet };
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.City = mailPiece.RcptAddressCity;
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.StateOrProvinceCode = mailPiece.RcptAddressState;// This always has to be a 2 letter code.
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.PostalCode = mailPiece.RcptAddressZIP;
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.CountryCode = Config.Core_CountryCode;
            }
            else
            {
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin = new ContactAndAddress();
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Contact = new Contact();
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Contact.PersonName = string.Format("{0} {1}", mailPiece.RetRcptFirstName, mailPiece.RetRcptLastName);
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Contact.CompanyName = mailPiece.RetRcptOrganization;
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Contact.PhoneNumber = string.IsNullOrEmpty(mailPiece.RetRcptPhone) ? "0000000000" : mailPiece.RetRcptPhone;

                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address = new ShipWebServiceRef.Address();
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.StreetLines = new string[] { mailPiece.RetRcptAddressStreet, mailPiece.RetRcptAddrOptionalLine1BelowStreet, mailPiece.RetRcptAddrOptionalLine2BelowStreet };
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.City = mailPiece.RetRcptAddressCity;
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.StateOrProvinceCode = mailPiece.RetRcptAddressState;// This always has to be a 2 letter code.
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.PostalCode = mailPiece.RetRcptAddressZIP;
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin.Address.CountryCode = Config.Core_CountryCode;
            }
        }

        private void SetRecipient(ProcessShipmentRequest request, MailPieceDTO mailPiece, DocType docType, bool isReturnLabel)
        {
            if (isReturnLabel)
            {
                request.RequestedShipment.Recipient = new Party(); // Recipient information
                request.RequestedShipment.Recipient.Contact = new Contact();
                request.RequestedShipment.Recipient.Contact.PersonName = $"{mailPiece.RetRcptFirstName} {mailPiece.RetRcptLastName}";
                request.RequestedShipment.Recipient.Contact.CompanyName = mailPiece.RetRcptOrganization;
                request.RequestedShipment.Recipient.Contact.PhoneNumber = string.IsNullOrEmpty(mailPiece.RetRcptPhone) ? "0000000000" : "+1" + mailPiece.RcptPhone;
                request.RequestedShipment.Recipient.Address = new ShipWebServiceRef.Address();
                request.RequestedShipment.Recipient.Address.StreetLines = new string[] { mailPiece.RetRcptAddressStreet, mailPiece.RetRcptAddrOptionalLine1BelowStreet, mailPiece.RetRcptAddrOptionalLine2BelowStreet };
                request.RequestedShipment.Recipient.Address.City = mailPiece.RetRcptAddressCity;
                request.RequestedShipment.Recipient.Address.StateOrProvinceCode = mailPiece.RetRcptAddressState;// This always has to be a 2 letter code.
                request.RequestedShipment.Recipient.Address.PostalCode = mailPiece.RetRcptAddressZIP;
                request.RequestedShipment.Recipient.Address.CountryCode = Config.Core_CountryCode;
                request.RequestedShipment.Recipient.Address.Residential = true;
            }
            else
            {
                request.RequestedShipment.Recipient = new Party(); // Recipient information
                request.RequestedShipment.Recipient.Contact = new Contact();
                request.RequestedShipment.Recipient.Contact.PersonName = $"{mailPiece.RcptFirstName} {mailPiece.RcptLastName}";
                request.RequestedShipment.Recipient.Contact.CompanyName = mailPiece.RcptOrganization;
                request.RequestedShipment.Recipient.Contact.PhoneNumber = string.IsNullOrEmpty(mailPiece.RcptPhone) ? "0000000000" : "+1" + mailPiece.RcptPhone;
                request.RequestedShipment.Recipient.Address = new ShipWebServiceRef.Address();
                request.RequestedShipment.Recipient.Address.StreetLines = new string[] { mailPiece.RcptAddressStreet, mailPiece.RcptAddrOptionalLine1BelowStreet, mailPiece.RcptAddrOptionalLine2BelowStreet };
                request.RequestedShipment.Recipient.Address.City = mailPiece.RcptAddressCity;
                request.RequestedShipment.Recipient.Address.StateOrProvinceCode = mailPiece.RcptAddressState;// This always has to be a 2 letter code.
                request.RequestedShipment.Recipient.Address.PostalCode = mailPiece.RcptAddressZIP;
                request.RequestedShipment.Recipient.Address.CountryCode = Config.Core_CountryCode;
                request.RequestedShipment.Recipient.Address.Residential = true;
            }

            // For FedEx GROUND_HOME_DELIVERY packages we must specify below flag.
            if (request.RequestedShipment.ServiceType == ShippingServiceType.GROUND_HOME_DELIVERY.ToString())
            {
                request.RequestedShipment.Recipient.Address.ResidentialSpecified = true;
            }
        }

        private void SetSender(ProcessShipmentRequest request, ShipperInformation shipperInfo, MailPieceDTO mailPiece, bool isReturnLabel)
        {
            if (isReturnLabel)
            {
                request.RequestedShipment.Shipper = new Party(); // Sender information
                request.RequestedShipment.Shipper.Contact = new Contact();
                request.RequestedShipment.Shipper.Contact.PersonName = $"{mailPiece.RcptFirstName} {mailPiece.RcptLastName}";
                request.RequestedShipment.Shipper.Contact.CompanyName = mailPiece.RcptOrganization;
                request.RequestedShipment.Shipper.Contact.PhoneNumber = "0000000000";
                request.RequestedShipment.Shipper.Address = new ShipWebServiceRef.Address();
                request.RequestedShipment.Shipper.Address.StreetLines = new string[] { mailPiece.RcptAddressStreet, mailPiece.RcptAddrOptionalLine1BelowStreet, mailPiece.RcptAddrOptionalLine2BelowStreet };

                request.RequestedShipment.Shipper.Address.City = mailPiece.RcptAddressCity;
                request.RequestedShipment.Shipper.Address.StateOrProvinceCode = mailPiece.RcptAddressState;
                request.RequestedShipment.Shipper.Address.PostalCode = mailPiece.RcptAddressZIP;
                request.RequestedShipment.Shipper.Address.CountryCode = Config.Core_CountryCode;
            }
            else
            {
                request.RequestedShipment.Shipper = new Party(); // Sender information
                request.RequestedShipment.Shipper.Contact = new Contact();
                request.RequestedShipment.Shipper.Contact.PersonName = shipperInfo.PrintVendorName;
                request.RequestedShipment.Shipper.Contact.CompanyName = shipperInfo.PrintVendorName;
                request.RequestedShipment.Shipper.Contact.PhoneNumber = "0000000000";
                request.RequestedShipment.Shipper.Address = new ShipWebServiceRef.Address();
                request.RequestedShipment.Shipper.Address.StreetLines = new string[] { shipperInfo.PrintVendorStreet };

                request.RequestedShipment.Shipper.Address.City = shipperInfo.PrintVendorCity;
                request.RequestedShipment.Shipper.Address.StateOrProvinceCode = shipperInfo.PrintVendorState;
                request.RequestedShipment.Shipper.Address.PostalCode = shipperInfo.PrintVendorZip;
                request.RequestedShipment.Shipper.Address.CountryCode = Config.Core_CountryCode;
            }
        }

        private void SetPackageLineItems(ProcessShipmentRequest request, MailPieceDTO mailPiece, Client client)
        {
            string refNotes = "{0}_{1}_{2}".Fill(client.AppCode, mailPiece.Id, Path.GetFileNameWithoutExtension(mailPiece.FileName));
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[1];
            request.RequestedShipment.RequestedPackageLineItems[0] = new RequestedPackageLineItem();
            request.RequestedShipment.RequestedPackageLineItems[0].SequenceNumber = "1";
            request.RequestedShipment.RequestedPackageLineItems[0].Weight = new Weight(); // Package weight information
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Value = 1.0M;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Units = WeightUnits.LB;
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences = new CustomerReference[1];
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[0] = new CustomerReference() { CustomerReferenceType = CustomerReferenceType.CUSTOMER_REFERENCE, Value = refNotes.Length > 39 ? refNotes.Substring(0, 39) : refNotes };

            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested = new PackageSpecialServicesRequested();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes = new string[1];
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes[0] = "SIGNATURE_OPTION";
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SignatureOptionDetail = new SignatureOptionDetail();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SignatureOptionDetail.OptionType = SignatureOptionType.NO_SIGNATURE_REQUIRED;

        }

        private void SetSpecialServiceRequestLineItems(ProcessShipmentRequest request, string shipmentSpecialService, DateTime fedexShipTimeStamp)
        {
            string[] specialServices = shipmentSpecialService.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            request.RequestedShipment.SpecialServicesRequested = new ShipmentSpecialServicesRequested();
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes = new string[specialServices.Length];

            List<string> specialServiceList = new List<string>();

            for (int i = 0; i < specialServices.Length; i++)
            {
                //Here we have to check that If SATURDAY_DELIVERY option is enabled then ShipTimeStamp day should be Friday, otherwise it will considered as an error for that mail piece.
                if (specialServices[i] == "SATURDAY_DELIVERY")
                {
                    if (fedexShipTimeStamp.DayOfWeek == DayOfWeek.Friday)
                    {
                        specialServiceList.Add(specialServices[i]);
                    }
                }
                else if (specialServices[i] == "SATURDAY_PICKUP")
                {
                    if (fedexShipTimeStamp.DayOfWeek == DayOfWeek.Saturday)
                    {
                        specialServiceList.Add(specialServices[i]);
                    }
                }
            }
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes = specialServiceList.ToArray();
        }

        // FedEx shipping service will return the label in landscape orientation and with different margins etc.; so we will create PDF having content in center area from the image returned by FedEx shipping service.
        private void GenerateShipmentLabel(ProcessShipmentReply reply, MailBatchInfo mbi, ref SmartDocResponse response)
        {
            string LabelFileName = string.Empty;
            FileStream LabelFile = null;

            // Rate details for each package
            foreach (CompletedPackageDetail packageDetail in reply.CompletedShipmentDetail.CompletedPackageDetails)
            {
                log.Info("Label File Creation : Started");
                LabelFileName = Path.Combine(mbi.PnSRequestPath, string.Format("{0}.pdf", packageDetail.TrackingIds[0].TrackingNumber));
                LabelFile = new FileStream(LabelFileName, FileMode.Create);
                log.Info("Label File Creation : Completed");

                using (Document doc = new Document(PageSize.LETTER))
                {
                    PdfWriter writer1 = PdfWriter.GetInstance(doc, LabelFile);
                    doc.Open();
                    var logo1 = Image.GetInstance(packageDetail.Label.Parts[0].Image);
                    logo1.Alignment = Element.ALIGN_CENTER;
                    logo1.ScaleToFit(267, 430);
                    // logo1.SetDpi(600, 600);
                    logo1.Border = 0;
                    PdfPCell image1Cell = new PdfPCell();
                    image1Cell.AddElement(logo1);
                    image1Cell.Border = PdfPCell.NO_BORDER;
                    image1Cell.PaddingLeft = Utilities.InchesToPoints(-0.5f);
                    image1Cell.PaddingTop = Utilities.InchesToPoints(1.5f);
                    image1Cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    PdfPTable table = new PdfPTable(1);
                    table.AddCell(image1Cell);
                    doc.Add(table);
                    doc.Close();
                }

                GeneratedFedExLabelPath = LabelFileName;
                response.CoverLetterPath = LabelFileName;
                response.ExceptionCode = OutComeCodes.No_Error;
                response.ExceptionDetail = string.Empty;
                response.HasError = false;
                response.PostageCost = packageDetail.PackageRating == null ? 0.00M : packageDetail.PackageRating.PackageRateDetails[0].NetCharge == null ? 0.00M : packageDetail.PackageRating.PackageRateDetails[0].NetCharge.Amount;
                response.TrackingCode = packageDetail.TrackingIds[0].TrackingNumber;
            }
        }
    }
}
