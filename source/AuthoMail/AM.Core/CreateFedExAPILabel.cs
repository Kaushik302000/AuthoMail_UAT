using AM.Entities;
using AM.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static AM.Core.FedExEnum;
using static AM.Core.FedExRequestResponse;

namespace AM.Core
{
    class CreateFedExAPILabel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GeneratedFedExLabelPath { get; private set; }

        public async Task<SmartDocResponse> Create(MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, Client client, DocType docType, MailBatchInfo mbi, ShipperInformation shipperInfo, bool isReturnLabel = false)
        {
            SmartDocResponse response = new SmartDocResponse();
            //Here we are assigning No_Error because if we have any general failure or end point exception then we are trying again.
            mailPiece.ErrorCode = OutComeCodes.No_Error;
            mailPiece.ErrorDesc = null;
            mailPiece.ShippingStatus = ShippingStatus.InProgress.ToString();

            FedExRequest fedExRequest = CreateShipmentRequest(mailPiece, fedExAccInfo, client, docType, shipperInfo, isReturnLabel);

            try
            {
                var token = await GetToken(fedExAccInfo);

                using (var httpClient = SetupClient(token, mailPiece.Id.ToString()))
                {
                    string json = JsonConvert.SerializeObject(fedExRequest);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(Config.Common_FedExShippingLabelUrl, content);
                    var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
                    var fedExResponse = JsonConvert.DeserializeObject<FedExResponse>(responseString);
                    // check if the call was successful
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        GenerateShipmentLabel(fedExResponse, mbi, ref response);
                    }
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
                    {
                        //If we get saturday delivery failed then we just remove that special handling parameters.
                        if (fedExResponse.Errors[0].Code == "REQUESTEDSHIPMENT.SERVICETYPE.NOTSUPPORTED" || fedExResponse.Errors[0].Code == "SHIPMENT.SPECIALSERVICETYPE.NOT.ALLOWED" || fedExResponse.Errors[0].Message.Contains("SATURDAY_DELIVERY is not supported") || fedExResponse.Errors[0].Message.Contains("SATURDAY_DELIVERY is not allowed with the service selected") || fedExResponse.Errors[0].Message.Contains("special service type is not allowed"))
                        {
                            log.Error($"MailPieceId - {mailPiece.Id} does not support saturday delivery.");
                            fedExRequest.RequestedShipment.ShipmentSpecialServices = null;

                            json = JsonConvert.SerializeObject(fedExRequest);
                            content = new StringContent(json, Encoding.UTF8, "application/json");
                            httpResponseMessage = await httpClient.PostAsync(Config.Common_FedExShippingLabelUrl, content);
                            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                            {
                                responseString = await httpResponseMessage.Content.ReadAsStringAsync();
                                fedExResponse = JsonConvert.DeserializeObject<FedExResponse>(responseString);
                                GenerateShipmentLabel(fedExResponse, mbi, ref response);
                                return response;
                            }
                        }

                        StringBuilder sbMessages = new StringBuilder($"MailPiece Id : {mailPiece.Id} Error occurred while generating FedEx label for client {mbi.Client.AppCode} and {mailPiece.BusinessFriendlyIdName} : {mailPiece.BusinessFriendlyIdValue}<br/>");
                        fedExResponse.Errors.ForEach(n => sbMessages = sbMessages.AppendLine(n.Message));

                        log.Error(sbMessages.ToString());

                        response.HasError = true;
                        response.CoverLetterPath = "";
                        response.ExceptionCode = fedExResponse.Errors[0].Code;
                        response.ExceptionDetail = fedExResponse.Errors[0].Message;
                    }
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        var error = $"MailPiece Id : {mailPiece.Id} Error occurred while generating FedEx label for client {mbi.Client.AppCode} and {mailPiece.BusinessFriendlyIdName} : {mailPiece.BusinessFriendlyIdValue}<br/>";
                        log.Error(error);

                        response.HasError = true;
                        response.CoverLetterPath = "";
                        response.ExceptionCode = fedExResponse.Errors[0].Code;
                        response.ExceptionDetail = fedExResponse.Errors[0].Message;
                    }
                    else if (httpResponseMessage.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        var error = $"MailPiece Id : {mailPiece.Id} Error occurred while generating FedEx label for client {mbi.Client.AppCode} and {mailPiece.BusinessFriendlyIdName} : {mailPiece.BusinessFriendlyIdValue}<br/>";
                        log.Error(error);

                        response.HasError = true;
                        response.CoverLetterPath = "";
                        response.ExceptionCode = fedExResponse.Errors[0].Code;
                        response.ExceptionDetail = fedExResponse.Errors[0].Message;
                    }
                }
            }
            catch (Exception ex)
            {
                var error = $"Error occured while generating FedEx label for MailPieceId : {mailPiece.Id} And Exception : {ex}";
                log.Error(error);

                SendUnexpectedExceptionEmail(error);

                response.HasError = true;
                response.CoverLetterPath = "";
                response.ExceptionCode = OutComeCodes.Unknown_Error;
                response.ExceptionDetail = $"{OutComeCodes.Unknown_Error}|{ex.Message}";
            }

            return response;
        }

        private FedExRequest CreateShipmentRequest(MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, Client client, DocType docType, ShipperInformation shipperInfo, bool isReturnLabel)
        {
            // Build the ShipmentRequest
            FedExRequest shipRequest = new FedExRequest();
            shipRequest.MergeLabelDocOption = MergeLabelDocOption.LABELS_ONLY;
            shipRequest.LabelResponseOptions = LabelResponseOptions.LABEL;
            shipRequest.AccountNumber = new AccountNumber { Value = fedExAccInfo.AccountNumber };
            SetShipmentDetails(shipRequest, mailPiece, fedExAccInfo, docType, shipperInfo, isReturnLabel);
            SetPackageLineItems(shipRequest, mailPiece, client);

            if (!string.IsNullOrEmpty(docType.ShippingSpecialService))
            {
                SetSpecialServiceRequestLineItems(shipRequest, docType.ShippingSpecialService, mailPiece.EffectiveReceivedOnEst.Value);
            }
            return shipRequest;
        }

        private void SetShipmentDetails(FedExRequest request, MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, DocType docType, ShipperInformation shipperInfo, bool isReturnLabel)
        {

            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipDatestamp = DateTime.Now.ToString("yyyy-MM-dd");
            request.RequestedShipment.PickupType = PickupType.USE_SCHEDULED_PICKUP;

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
            request.RequestedShipment.TotalWeight = 1;
            request.RequestedShipment.RateRequestType = new List<RateRequestType>() { RateRequestType.LIST };
            request.RequestedShipment.TotalPackageCount = 1;

            SetSender(request, shipperInfo, mailPiece, isReturnLabel);
            //
            SetRecipient(request, mailPiece, docType, isReturnLabel);
            //
            SetPayment(request, fedExAccInfo);
            //
            SetLabelDetails(request, mailPiece, isReturnLabel);
        }

        private void SetSender(FedExRequest request, ShipperInformation shipperInfo, MailPieceDTO mailPiece, bool isReturnLabel)
        {
            if (isReturnLabel)
            {
                request.RequestedShipment.Shipper = new Shipper();
                request.RequestedShipment.Shipper.Contact = new Contact();
                request.RequestedShipment.Shipper.Contact.PersonName = $"{mailPiece.RcptFirstName} {mailPiece.RcptLastName}";
                request.RequestedShipment.Shipper.Contact.CompanyName = mailPiece.RcptOrganization;
                request.RequestedShipment.Shipper.Contact.PhoneNumber = "0000000000";

                request.RequestedShipment.Shipper.Address = new FedExRequestResponse.Address();
                request.RequestedShipment.Shipper.Address.StreetLines = new List<string> { mailPiece.RcptAddressStreet, mailPiece.RcptAddrOptionalLine1BelowStreet, mailPiece.RcptAddrOptionalLine2BelowStreet };
                request.RequestedShipment.Shipper.Address.City = mailPiece.RcptAddressCity;
                request.RequestedShipment.Shipper.Address.StateOrProvinceCode = mailPiece.RcptAddressState;
                request.RequestedShipment.Shipper.Address.PostalCode = mailPiece.RcptAddressZIP;
                request.RequestedShipment.Shipper.Address.CountryCode = Config.Core_CountryCode;
            }
            else
            {
                request.RequestedShipment.Shipper = new Shipper();
                request.RequestedShipment.Shipper.Contact = new Contact();
                request.RequestedShipment.Shipper.Contact.PersonName = shipperInfo.PrintVendorName;
                request.RequestedShipment.Shipper.Contact.CompanyName = shipperInfo.PrintVendorName;
                request.RequestedShipment.Shipper.Contact.PhoneNumber = "0000000000";

                request.RequestedShipment.Shipper.Address = new FedExRequestResponse.Address();
                request.RequestedShipment.Shipper.Address.StreetLines = new List<string> { shipperInfo.PrintVendorStreet };
                request.RequestedShipment.Shipper.Address.City = shipperInfo.PrintVendorCity;
                request.RequestedShipment.Shipper.Address.StateOrProvinceCode = shipperInfo.PrintVendorState;
                request.RequestedShipment.Shipper.Address.PostalCode = shipperInfo.PrintVendorZip;
                request.RequestedShipment.Shipper.Address.CountryCode = Config.Core_CountryCode;
            }
        }

        private void SetRecipient(FedExRequest request, MailPieceDTO mailPiece, DocType docType, bool isReturnLabel)
        {
            if (isReturnLabel)
            {
                request.RequestedShipment.Recipients = new List<FedExRequestResponse.Recipient>()
                {
                    new FedExRequestResponse.Recipient
                    {
                        Contact = new Contact
                        {
                            PersonName = $"{mailPiece.RetRcptFirstName} {mailPiece.RetRcptLastName}",
                            CompanyName  = mailPiece.RetRcptOrganization,
                            PhoneNumber = string.IsNullOrEmpty(mailPiece.RetRcptPhone) ? "0000000000" : "+1" + mailPiece.RcptPhone
                        },
                        Address = new FedExRequestResponse.Address
                        {
                            StreetLines = new List<string> { mailPiece.RetRcptAddressStreet, mailPiece.RetRcptAddrOptionalLine1BelowStreet, mailPiece.RetRcptAddrOptionalLine2BelowStreet },
                            City = mailPiece.RetRcptAddressCity,
                            StateOrProvinceCode = mailPiece.RetRcptAddressState,
                            PostalCode = mailPiece.RetRcptAddressZIP,
                            CountryCode = Config.Core_CountryCode,
                            Residential = true
                        }
                    }
                };
            }
            else
            {
                request.RequestedShipment.Recipients = new List<FedExRequestResponse.Recipient>()
                {
                    new FedExRequestResponse.Recipient
                    {
                        Contact = new Contact
                        {
                            PersonName = $"{mailPiece.RcptFirstName} {mailPiece.RcptLastName}",
                            CompanyName  = mailPiece.RcptOrganization,
                            PhoneNumber = string.IsNullOrEmpty(mailPiece.RcptPhone) ? "0000000000" : "+1" + mailPiece.RcptPhone
                        },
                        Address = new FedExRequestResponse.Address
                        {
                            StreetLines = new List<string> { mailPiece.RcptAddressStreet, mailPiece.RcptAddrOptionalLine1BelowStreet, mailPiece.RcptAddrOptionalLine2BelowStreet  },
                            City = mailPiece.RcptAddressCity,
                            StateOrProvinceCode = mailPiece.RcptAddressState,
                            PostalCode = mailPiece.RcptAddressZIP,
                            CountryCode = Config.Core_CountryCode,
                            Residential = true
                        }
                    }
                };
            }

            //.............................. TBD .............................

            // For FedEx GROUND_HOME_DELIVERY packages we must specify below flag.
            //if (request.RequestedShipment.ServiceType == ShippingServiceType.GROUND_HOME_DELIVERY.ToString())
            //{
            //    request.RequestedShipment.Recipient.Address.ResidentialSpecified = true;
            //}
        }

        private void SetPayment(FedExRequest request, FedExAccountInfo fedExAccInfo)
        {
            request.RequestedShipment.ShippingChargesPayment = new ShippingChargesPayment();
            request.RequestedShipment.ShippingChargesPayment.PaymentType = PaymentType.SENDER;
            request.RequestedShipment.ShippingChargesPayment.Payor = new Payor();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty = new ResponsibleParty();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.AccountNumber = new AccountNumber { Value = fedExAccInfo.AccountNumber };
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Address = new FedExRequestResponse.Address();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Address.CountryCode = Config.Core_CountryCode;
        }

        private void SetLabelDetails(FedExRequest request, MailPieceDTO mailPiece, bool isReturnLabel)
        {
            request.RequestedShipment.LabelSpecification = new LabelSpecification();
            request.RequestedShipment.LabelSpecification.ImageType = ImageType.PNG;
            request.RequestedShipment.LabelSpecification.LabelFormatType = LabelFormatType.COMMON2D;
            request.RequestedShipment.LabelSpecification.LabelStockType = LabelStockType.PAPER_4X6;
            request.RequestedShipment.LabelSpecification.LabelPrintingOrientation = LabelPrintingOrientation.BOTTOM_EDGE_OF_TEXT_FIRST;

            if (isReturnLabel)
            {
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin = new PrintedLabelOrigin()
                {
                    Contact = new Contact
                    {
                        PersonName = $"{mailPiece.RcptFirstName} {mailPiece.RcptLastName}",
                        CompanyName = mailPiece.RcptOrganization,
                        PhoneNumber = string.IsNullOrEmpty(mailPiece.RcptPhone) ? "0000000000" : mailPiece.RcptPhone
                    },
                    Address = new FedExRequestResponse.Address
                    {
                        StreetLines = new List<string> { mailPiece.RcptAddressStreet, mailPiece.RcptAddrOptionalLine1BelowStreet, mailPiece.RcptAddrOptionalLine2BelowStreet },
                        City = mailPiece.RcptAddressCity,
                        StateOrProvinceCode = mailPiece.RcptAddressState,
                        PostalCode = mailPiece.RcptAddressZIP,
                        CountryCode = Config.Core_CountryCode,
                    }
                };
            }
            else
            {
                request.RequestedShipment.LabelSpecification.PrintedLabelOrigin = new PrintedLabelOrigin()
                {
                    Contact = new Contact
                    {
                        PersonName = $"{mailPiece.RetRcptFirstName} {mailPiece.RetRcptLastName}",
                        CompanyName = mailPiece.RetRcptOrganization,
                        PhoneNumber = string.IsNullOrEmpty(mailPiece.RetRcptPhone) ? "0000000000" : mailPiece.RetRcptPhone
                    },
                    Address = new FedExRequestResponse.Address
                    {
                        StreetLines = new List<string> { mailPiece.RetRcptAddressStreet, mailPiece.RetRcptAddrOptionalLine1BelowStreet, mailPiece.RetRcptAddrOptionalLine2BelowStreet },
                        City = mailPiece.RetRcptAddressCity,
                        StateOrProvinceCode = mailPiece.RetRcptAddressState,
                        PostalCode = mailPiece.RetRcptAddressZIP,
                        CountryCode = Config.Core_CountryCode
                    }
                };
            }
        }

        private void SetPackageLineItems(FedExRequest request, MailPieceDTO mailPiece, Client client)
        {
            string refNotes = $"{client.AppCode}_{mailPiece.Id}_{Path.GetFileNameWithoutExtension(mailPiece.FileName)}";
            request.RequestedShipment.RequestedPackageLineItems = new List<RequestedPackageLineItem>()
            {
                new RequestedPackageLineItem
                {
                    SequenceNumber = "1",
                    Weight = new Weight { Value = 1, Units = Units.LB.ToString() },
                    CustomerReferences = new List<CustomerReference>()
                    {
                        new CustomerReference()
                        {
                            CustomerReferenceType = CustomerReferenceType.CUSTOMER_REFERENCE,
                            Value = refNotes.Length > 39 ? refNotes.Substring(0, 39) : refNotes
                        }
                    },
                    PackageSpecialServices = new PackageSpecialServices()
                    {
                        SpecialServiceTypes = new List<string>() { "SIGNATURE_OPTION" },
                        SignatureOptionType = SignatureOptionType.NO_SIGNATURE_REQUIRED
                    }
                }
            };

        }

        private void SetSpecialServiceRequestLineItems(FedExRequest request, string shipmentSpecialService, DateTime fedexShipTimeStamp)
        {
            string[] specialServices = shipmentSpecialService.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

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

            request.RequestedShipment.ShipmentSpecialServices = new ShipmentSpecialServices();
            request.RequestedShipment.ShipmentSpecialServices.SpecialServiceTypes = specialServiceList;
        }

        public async static Task<string> GetToken(FedExAccountInfo fedExAccInfo)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    //setup client
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //setup login data
                    var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", fedExAccInfo.WebAuthenticationKey),
                    new KeyValuePair<string, string>("client_secret", fedExAccInfo.WebAuthenticationPassword)
                });

                    //send request
                    HttpResponseMessage responseMessage = await httpClient.PostAsync(Config.Common_FedExTokenUrl, formContent);

                    //get access token from response body
                    var responseJson = await responseMessage.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseJson);
                    return jObject.GetValue("access_token").ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static HttpClient SetupClient(string token, string custID)
        {
            var clientHandler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            var httpClient = new HttpClient(clientHandler);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            httpClient.DefaultRequestHeaders.Add("x-customer-transaction-id", custID);
            return httpClient;
        }

        // FedEx shipping service will return the label in landscape orientation and with different margins etc.; so we will create PDF having content in center area from the image returned by FedEx shipping service.
        private void GenerateShipmentLabel(FedExResponse reply, MailBatchInfo mbi, ref SmartDocResponse response)
        {

            // Rate details for each package
            foreach (PieceResponse pieceResponse in reply.Output.TransactionShipments[0].PieceResponses)
            {
                log.Info("Label File Creation : Started");

                string LabelFileName = string.Empty;
                FileStream LabelFile = null;
                LabelFileName = Path.Combine(mbi.PnSRequestPath, string.Format("{0}.pdf", pieceResponse.TrackingNumber));
                LabelFile = new FileStream(LabelFileName, FileMode.Create);

                log.Info("Label File Creation : Completed");

                using (iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.LETTER))
                {
                    PdfWriter writer1 = PdfWriter.GetInstance(doc, LabelFile);
                    doc.Open();
                    var logo1 = Image.GetInstance(Convert.FromBase64String(pieceResponse.PackageDocuments[0].EncodedLabel));
                    logo1.Alignment = Element.ALIGN_CENTER;
                    logo1.RotationDegrees = 270;
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
                response.PostageCost = pieceResponse.NetChargeAmount == 0 ? 0.00M : (decimal)pieceResponse.NetChargeAmount;
                response.TrackingCode = pieceResponse.TrackingNumber;
            }

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
                        msg.Subject = $"{Config.Common_Environment} : {Config.Monitor_ExceptionEmail_Subject}";
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
    }
}
