using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Net.Http;
using Dapper;
using AM.USPSLib;
using AM.Services.V3;
using Newtonsoft.Json;
using System.Configuration;
using AM.Services.PB;

namespace AM.Services
{
    public partial class USPSService
    {
        public async Task<eVSLabelResponse> RequestPBLabelAsync(string userId, PierbridgeShipRequest request, USPSCredentialInfo uspsUserInfo)
        {
            try
            {
                bool isAPIError = false;
                string apiErrorMSG = null;
                Task<int> dbLog = null;
                int logId = 0;

                if (request == null || userId.IsNullOrWhiteSpace())
                {
                    isAPIError = true;
                    apiErrorMSG = "For create PB Label, UserId must be specified and Request At least one.";
                }

                var labelInputXML = request.Serialize();

                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    dbLog = db.ExecuteScalarAsync<int>("Insert into [API].[USPSeVS] ([APIName], [UserId], [APIRequest], [APIRequestedOn],[IsAPIError],[IsUSPSError],[APIResponse], [APIResponseOn],[Postage]) OUTPUT INSERTED.Id VALUES(@APIName, @UserId, @APIRequest, @APIRequestedOn, @IsAPIError, @IsUSPSError, @APIResponse, @APIResponseOn,@Postage);", new { APIName = Config.Web_API_USPS_eVSAPIName, UserId = userId, APIRequest = labelInputXML, APIRequestedOn = Common.Now, IsAPIError = isAPIError, IsUSPSError = false, APIResponse = apiErrorMSG, APIResponseOn = isAPIError ? Common.Now : (DateTime?)null, Postage = 0 });
                }

                if (isAPIError)
                {
                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                    }

                    return new eVSLabelResponse()
                    {
                        Error = new USPSError()
                        {
                            Source = ResponseSource.CLSVC,
                            Description = apiErrorMSG
                        }
                    };
                }

                Task<int> uspsReqLog = null;
                int uspsReqLogAwait = 0;
                PierbridgeShipResponse pbResponse = null;

                if (uspsUserInfo.PrintLocation == "api")
                {
                    USPSLabelCutOffDetail uspsCutOffDetail;
                    var estTime = Common.Now.UtcToEst();
                    var dayOfWeek = estTime.DayOfWeek;

                    if (dayOfWeek == DayOfWeek.Saturday)
                    {
                        uspsCutOffDetail = Config.USPSLabelCutOffDetails.SingleOrDefault(u => estTime.TimeOfDay >= u.SatStartTime && estTime.TimeOfDay <= u.SatEndTime);
                    }
                    else
                    {
                        uspsCutOffDetail = Config.USPSLabelCutOffDetails.SingleOrDefault(u => estTime.TimeOfDay >= u.BizStartTime && estTime.TimeOfDay <= u.BizEndTime);
                    }
                }

                var eVSrequestXML = request.Serialize();

                using (HttpClient httpClient = new HttpClient())
                {
                    string url = Config.Web_API_USPS_URL; //todo get pb api url

                    if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    }

                    var content = new StringContent(eVSrequestXML, Encoding.UTF8, "application/xml");

                    var uspsReq = httpClient.PostAsync(url, content);

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                        uspsReqLog = db.ExecuteAsync("Update [API].[USPSeVS] set [USPSRequest] = @USPSRequest, [USPSRequestedOn]= @USPSRequestedOn Where Id =@Id;", new { USPSRequest = eVSrequestXML, USPSRequestedOn = Common.Now, Id = logId });
                    }

                    string responseXML = null;
                    bool isUSPSError = false;

                    try
                    {
                        var response = await uspsReq.ConfigureAwait(false);
                        responseXML = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        isUSPSError = !responseXML.Contains("PierbridgeShipResponse");

                        if (isUSPSError)
                        {
                            //PB given an error html response so not to deserialise
                            //var err = responseXML.Deserialize<USPSError>();
                            pbResponse = new PierbridgeShipResponse { Error = new AM.USPSLib.USPSError { Source = ResponseSource.USPS, Description = responseXML } };
                        }
                        else
                        {
                            isUSPSError = pbResponse?.Status?.Code == 0;

                            if (isUSPSError)
                            {
                                pbResponse = responseXML.Deserialize<PierbridgeShipResponse>();
                                pbResponse.Error = new AM.USPSLib.USPSError
                                {
                                    Source = ResponseSource.USPS,
                                    Description = pbResponse?.Status?.Description
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("eVS hard fail while download", ex);

                        responseXML = "{0}\r\n{1}\r\n{2}".Fill(responseXML, new string('-', 105), ex.ToString());
                        string err = "{0}\r\n{1}".Fill(ex.Message, responseXML);

                        isUSPSError = true;
                        pbResponse = new PierbridgeShipResponse
                        {
                            Error = new USPSError
                            {
                                Source = ResponseSource.USPS,
                                Description = err
                            }
                        };
                    }

                    Task<int> uspsResXMLLog = null;
                    int uspsResXMLLogAwait = 0;
                    Task<int> apiResponseLog = null;
                    int apiResponseLogAwait = 0;

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        uspsReqLogAwait = await uspsReqLog.ConfigureAwait(false);
                        uspsResXMLLog = db.ExecuteAsync("Update [API].[USPSeVS] set IsUSPSError = @IsUSPSError, [USPSResponse] = @USPSResponse, [USPSResponseOn] = @USPSResponseOn Where Id = @Id;", new { IsUSPSError = isUSPSError, USPSResponse = responseXML, USPSResponseOn = Common.Now, Id = logId });
                    }

                    if (!isUSPSError)
                    {
                        pbResponse = responseXML.Deserialize<PierbridgeShipResponse>();
                    }

                    //string commitmentName = null;
                    DateTime? scheduledDeliveryDate = null;

                    //todo get commintment tag

                    if (pbResponse.CommitmentLevel != null)
                    {
                        //commitmentName = eVSresponse.Commitment.CommitmentName;

                        if (!pbResponse.CommitmentLevel.IsNullOrWhiteSpace())
                        {
                            scheduledDeliveryDate =
                                DateTime.ParseExact(pbResponse.CommitmentLevel, "yyyy-MM-dd HH:mm", null);
                        }
                    }

                    var eVSLabelresponse = eVSLabelResponse(pbResponse);

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        var apiResponseXML = eVSLabelresponse.Serialize();
                        uspsResXMLLogAwait = await uspsResXMLLog.ConfigureAwait(false);
                        apiResponseLog = db.ExecuteAsync("Update [API].[USPSeVS] set APIResponse = @APIResponse, APIResponseOn = @APIResponseOn, BarcodeNumber = @BarcodeNumber, Postage = @Postage,CommitmentName = @CommitmentName,ScheduledDeliveryDate = @ScheduledDeliveryDate Where Id = @Id", new { APIResponse = apiResponseXML, APIResponseOn = Common.Now, BarcodeNumber = eVSLabelresponse.BarcodeNumber, Postage = eVSLabelresponse.Postage, CommitmentName = ""/*commitmentName*/, ScheduledDeliveryDate = scheduledDeliveryDate, Id = logId });
                    }

                    apiResponseLogAwait = await apiResponseLog.ConfigureAwait(false);

                    return eVSLabelresponse;
                }
            }
            catch (Exception ex)
            {
                log.Error("eVS Label has this Unexpected Error : ", ex);

                return new eVSLabelResponse()
                {
                    Error = new USPSError()
                    {
                        Source = ResponseSource.CLSVC,
                        Description = "Unexpected error"
                    }
                };
            }
        }
        private eVSLabelResponse eVSLabelResponse(PierbridgeShipResponse response)
        {
            var labelResponse = new eVSLabelResponse();

            ShipmentPackage package = response.Packages[0] ?? new ShipmentPackage() { Labels = new List<LabelOutput>() { } };

            labelResponse.BarcodeNumber = package.WayBillNumber;
            labelResponse.LabelImageString = package.Labels[0]?.ContentBase64;
            //labelResponse.ReceiptImageString = response.ReceiptImageString;
            labelResponse.ToName = response?.Receiver?.CompanyName;
            //labelResponse.ToFirm = response.ToFirm;
            //labelResponse.ToAptOrSuiteorOther = response.Receiver.Street;
            labelResponse.ToStreetAddress = response?.Receiver?.Street;
            labelResponse.ToCity = response?.Receiver?.City;
            labelResponse.ToState = response?.Receiver?.Region;
            labelResponse.ToZip5 = response?.Receiver?.PostalCode;
            //labelResponse.ToZip4 = response.ToZip4;
            //labelResponse.Postnet = response.Postnet;
            //labelResponse.RDC = response.RDC;
            labelResponse.Postage = decimal.Parse(response?.Customer?.ShippingCharge.ToString());
            //labelResponse.ExtraServices = response.ExtraServices;
            //labelResponse.Zone = response.Zone;
            labelResponse.DimensionalWeight = response?.Weight.ToString();
            //labelResponse.CarrierRoute = response.CarrierRoute;
            //labelResponse.PermitHolderName = response.PermitHolderName;
            //labelResponse.InductionType = response.InductionType;
            //labelResponse.LogMessage = response.LogMessage;

            labelResponse.Commitment = response.CommitmentLevel != null ? new CommitmentDetail()
            {
                //CommitmentName = response.Commitment.CommitmentName,

                ScheduledDeliveryDate = !response.CommitmentLevel.IsNullOrWhiteSpace() ?
                DateTime.ParseExact(response.CommitmentLevel, "yyyy-MM-dd HH:mm", null) : (DateTime?)null
            } : null;

            labelResponse.Error = response.Error;

            return labelResponse;
        }
    }
}
