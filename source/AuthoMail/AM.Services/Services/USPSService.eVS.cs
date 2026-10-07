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
using System.ComponentModel;
using AM.Services.PB;

namespace AM.Services
{
    public partial class USPSService
    {
        public async Task<eVSLabelResponse> RequesteVSLabelAsync(string userId, eVSLabelRequest request, USPSCredentialInfo uspsUserInfo)
        {
            try
            {
                if (bool.Parse(ConfigurationManager.AppSettings["UseUSPSV3API"].ToString()))
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException("Given request object is null");
                    }

                    //return await this.RequesteVSLabelAPIAsync(userId, request, uspsUserInfo);
                    PierbridgeShipRequest pierbridgeShipRequest = new PierbridgeShipRequest();

                    pierbridgeShipRequest.Carrier = 106;
                    pierbridgeShipRequest.ServiceType = request?.ServiceType == ServiceType.PRIORITY_EXPRESS ? 982 : 981;//Priority
                    pierbridgeShipRequest.Receiver = new Receiver { CompanyName = request.ToFirm.IsNullOrWhiteSpace() ? request.ToName : request.ToFirm, Street = request.ToStreetAddress, City = request.ToCity, Region = request.ToState, PostalCode = request.ToZip5, Country = "US" };
                    pierbridgeShipRequest.Packages = new Packages { PackageList = new List<Package> { new Package { ShipperReference = request?.CustomerRefNo, ReceiverName = request.ToName, ReceiverPhone = request.ToPhone, PackageType = 656, ContentDescription = "Documents", Height = double.Parse(request.Height.HasValue ? request.Height.Value.ToString() : "0.1"), Length = double.Parse(request.Length.HasValue ? request.Length.Value.ToString() : "12.5"), Weight = double.Parse(request.WeightInOunces), Width = double.Parse(request.Width.HasValue ? request.Width.Value.ToString() : "0.1") } } };

                    pierbridgeShipRequest.UserName = Config.Web_API_USPS_UserId;

                    //return await

                    return await RequestPBLabelAsync(userId, pierbridgeShipRequest, uspsUserInfo);
                }
                else
                {
                    bool isAPIError = false;
                    string apiErrorMSG = null;
                    Task<int> dbLog = null;
                    int logId = 0;

                    if (request == null || userId.IsNullOrWhiteSpace())
                    {
                        isAPIError = true;
                        apiErrorMSG = "For create eVS Label, UserId must be specified and Request At least one.";
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

                    FormUrlEncodedContent payload = null;
                    Task<int> uspsReqLog = null;
                    int uspsReqLogAwait = 0;
                    eVSResponse eVSresponse = null;

                    var eVSrequest = new eVSRequest(request);
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

                        var creds = Config.USPSCredentialDetails.SingleOrDefault(x => x.PrintLocation == uspsCutOffDetail.PrintVendorLocation);

                        eVSrequest.USERID = uspsCutOffDetail.USPSUserId;

                        if (!creds.PassKey.IsNullOrWhiteSpace()) { eVSrequest.PASSWORD = creds.PassKey; }
                    }
                    else
                    {
                        eVSrequest.USERID = uspsUserInfo.UserId;

                        if (!uspsUserInfo.PassKey.IsNullOrWhiteSpace()) { eVSrequest.PASSWORD = uspsUserInfo.PassKey; }

                    }

                    var eVSrequestXML = eVSrequest.Serialize();

                    using (HttpClient httpClient = new HttpClient())
                    {
                        string url = Config.Web_API_USPS_URL;

                        if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        }

                        payload = new FormUrlEncodedContent(new[]
                            {
                            new KeyValuePair<string, string>("API",Config.Web_API_USPS_eVSAPIName),
                            new KeyValuePair<string, string>("XML",eVSrequestXML)
                        });

                        var uspsReq = httpClient.PostAsync(url, payload);

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
                            isUSPSError = !responseXML.Contains("eVSResponse");

                            if (isUSPSError)
                            {
                                var err = responseXML.Deserialize<USPSError>();
                                eVSresponse = new eVSResponse { Error = err };
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("eVS hard fail while download", ex);

                            responseXML = "{0}\r\n{1}\r\n{2}".Fill(responseXML, new string('-', 105), ex.ToString());
                            string err = "{0}\r\n{1}".Fill(ex.Message, responseXML);

                            isUSPSError = true;
                            eVSresponse = new eVSResponse
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
                            eVSresponse = responseXML.Deserialize<eVSResponse>();
                        }

                        string commitmentName = null;
                        DateTime? scheduledDeliveryDate = null;

                        if (eVSresponse.Commitment != null)
                        {
                            commitmentName = eVSresponse.Commitment.CommitmentName;

                            if (!eVSresponse.Commitment.ScheduledDeliveryDate.IsNullOrWhiteSpace())
                            {
                                scheduledDeliveryDate =
                                    DateTime.ParseExact(eVSresponse.Commitment.ScheduledDeliveryDate, "yyyy-MM-dd", null);
                            }
                        }

                        var eVSLabelresponse = eVSLabelResponse(eVSresponse);

                        if (Config.Web_API_USPS_IsDbLoggingEnabled)
                        {
                            var apiResponseXML = eVSLabelresponse.Serialize();
                            uspsResXMLLogAwait = await uspsResXMLLog.ConfigureAwait(false);
                            apiResponseLog = db.ExecuteAsync("Update [API].[USPSeVS] set APIResponse = @APIResponse, APIResponseOn = @APIResponseOn, BarcodeNumber = @BarcodeNumber, Postage = @Postage,CommitmentName = @CommitmentName,ScheduledDeliveryDate = @ScheduledDeliveryDate Where Id = @Id", new { APIResponse = apiResponseXML, APIResponseOn = Common.Now, BarcodeNumber = eVSresponse.BarcodeNumber, Postage = eVSresponse.Postage, CommitmentName = commitmentName, ScheduledDeliveryDate = scheduledDeliveryDate, Id = logId });
                        }

                        apiResponseLogAwait = await apiResponseLog.ConfigureAwait(false);

                        return eVSLabelresponse;
                    }
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
                        Description = $"Unexpected error : {ex.ToString()}"
                    }
                };
            }
        }

        public async Task<eVSLabelResponse> RequesteVSLabelAPIAsync(string userId, eVSLabelRequest request, USPSCredentialInfo uspsUserInfo)
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
                    apiErrorMSG = "For create eVS Label, UserId must be specified and Request At least one.";
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

                FormUrlEncodedContent payload = null;
                Task<int> uspsReqLog = null;
                int uspsReqLogAwait = 0;
                eVSResponse eVSresponse = null;

                var eVSrequest = new eVSRequest(request);
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

                    var creds = Config.USPSCredentialDetails.SingleOrDefault(x => x.PrintLocation == uspsCutOffDetail.PrintVendorLocation);

                    eVSrequest.USERID = uspsCutOffDetail.USPSUserId;

                    if (!creds.PassKey.IsNullOrWhiteSpace()) { eVSrequest.PASSWORD = creds.PassKey; }
                }
                else
                {
                    eVSrequest.USERID = uspsUserInfo.UserId;

                    if (!uspsUserInfo.PassKey.IsNullOrWhiteSpace()) { eVSrequest.PASSWORD = uspsUserInfo.PassKey; }

                }

                var eVSrequestXML = eVSrequest.Serialize();

                var userName = ConfigurationManager.AppSettings["USPSFedexSecret"].ToString();
                var userKey = ConfigurationManager.AppSettings["USPSFedexKey"].ToString();
                var apiUrl = ConfigurationManager.AppSettings["USPSAPIUrl"].ToString();
                var crid = ConfigurationManager.AppSettings["USPSCRID"].ToString();
                var mid = ConfigurationManager.AppSettings["USPSMID"].ToString();
                var manifestMID = ConfigurationManager.AppSettings["USPSManifestMID"].ToString();
                var accountNumber = ConfigurationManager.AppSettings["USPSAccountNumber"].ToString();

                using (USPSAPIClient httpClient = new USPSAPIClient(userName, userKey))
                {
                    string url = apiUrl;

                    if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    }

                    var req = new eVSRequestV3(request);

                    var paymentToken = await httpClient.GetLabelPaymentToken(new LabelPaymentRequestV3
                    {
                        roles = new List<Role> {
                            new Role {  roleName="PAYER",
                                        CRID=crid,
                                        MID=mid,
                                        manifestMID=manifestMID,
                                        accountType="EPS",
                                        accountNumber=accountNumber },
                            new Role{
                                        roleName="LABEL_OWNER",
                                        CRID=crid,
                                        MID=mid,
                                        manifestMID=manifestMID,
                                        accountType="EPS",
                                        accountNumber=accountNumber }
                            }
                    });

                    var uspsReq = httpClient.GeteVSLabel(req, paymentToken);

                    //payload = new FormUrlEncodedContent(new[]
                    //    {
                    //        new KeyValuePair<string, string>("API",Config.Web_API_USPS_eVSAPIName),
                    //        new KeyValuePair<string, string>("XML",eVSrequestXML)
                    //    });

                    //var uspsReq = httpClient.PostAsync(url, payload);

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                        uspsReqLog = db.ExecuteAsync("Update [API].[USPSeVS] set [USPSRequest] = @USPSRequest, [USPSRequestedOn]= @USPSRequestedOn Where Id =@Id;", new { USPSRequest = eVSrequestXML, USPSRequestedOn = Common.Now, Id = logId });
                    }

                    string responseXML = null;
                    bool isUSPSError = false;
                    eVSResponseV3 responseV3 = new eVSResponseV3();
                    try
                    {
                        responseV3 = await uspsReq.ConfigureAwait(false);
                        responseXML = JsonConvert.SerializeObject(responseV3);
                        //await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        isUSPSError = !responseXML.Contains("labelAddress");

                        if (isUSPSError)
                        {
                            V3.Error err = responseV3.Error;

                            eVSresponse = new eVSResponse { Error = new USPSError() { Number = err.code, Description = err.message, Source = JsonConvert.SerializeObject(err.errors) } };
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("eVS hard fail while download", ex);

                        responseXML = "{0}\r\n{1}\r\n{2}".Fill(responseXML, new string('-', 105), ex.ToString());
                        string err = "{0}\r\n{1}".Fill(ex.Message, responseXML);

                        isUSPSError = true;
                        eVSresponse = new eVSResponse
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
                        eVSresponse = new eVSResponse() { BarcodeNumber = responseV3?.labelMetadata.trackingNumber, Commitment = new USPSLib.Commitment { CommitmentName = responseV3?.labelMetadata.commitment.CommitmentName, ScheduledDeliveryDate = responseV3?.labelMetadata.commitment.ScheduledDeliveryDate }, DimensionalWeight = responseV3?.labelMetadata.dimensionalWeight?.ToString(), ExtraServices = responseV3?.labelMetadata.extraServices.Select(s => new USPSLib.ExtraService { Price = s.Price, ServiceID = s.ServiceID, ServiceName = s.ServiceName }).ToList(), LabelImageString = responseV3?.labelImage, Postage = responseV3?.labelMetadata.postage ?? 0, ReceiptImageString = responseV3?.receiptImage, ToAptOrSuiteorOther = responseV3?.labelMetadata.labelAddress.secondaryAddress, ToStreetAddress = responseV3?.labelMetadata.labelAddress.streetAddress, ToCity = responseV3?.labelMetadata.labelAddress.city, ToState = responseV3?.labelMetadata.labelAddress.state, ToZip5 = responseV3?.labelMetadata.labelAddress.ZIPCode, ToZip4 = responseV3?.labelMetadata.labelAddress.ZIPPlus4 }; //responseXML.Deserialize<eVSResponse>();
                    }

                    string commitmentName = null;
                    DateTime? scheduledDeliveryDate = null;

                    if (eVSresponse.Commitment != null)
                    {
                        commitmentName = eVSresponse.Commitment.CommitmentName;

                        if (!eVSresponse.Commitment.ScheduledDeliveryDate.IsNullOrWhiteSpace())
                        {
                            scheduledDeliveryDate =
                                DateTime.ParseExact(eVSresponse.Commitment.ScheduledDeliveryDate, "yyyy-MM-dd", null);
                        }
                    }

                    var eVSLabelresponse = eVSLabelResponse(eVSresponse);

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        var apiResponseXML = eVSLabelresponse.Serialize();
                        uspsResXMLLogAwait = await uspsResXMLLog.ConfigureAwait(false);
                        apiResponseLog = db.ExecuteAsync("Update [API].[USPSeVS] set APIResponse = @APIResponse, APIResponseOn = @APIResponseOn, BarcodeNumber = @BarcodeNumber, Postage = @Postage,CommitmentName = @CommitmentName,ScheduledDeliveryDate = @ScheduledDeliveryDate Where Id = @Id", new { APIResponse = apiResponseXML, APIResponseOn = Common.Now, BarcodeNumber = eVSresponse.BarcodeNumber, Postage = eVSresponse.Postage, CommitmentName = commitmentName, ScheduledDeliveryDate = scheduledDeliveryDate, Id = logId });
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
        private eVSLabelResponse eVSLabelResponse(eVSResponse response)
        {
            var labelResponse = new eVSLabelResponse();

            labelResponse.BarcodeNumber = response.BarcodeNumber;
            labelResponse.LabelImageString = response.LabelImageString;
            labelResponse.ReceiptImageString = response.ReceiptImageString;
            labelResponse.ToName = response.ToName;
            labelResponse.ToFirm = response.ToFirm;
            labelResponse.ToAptOrSuiteorOther = response.ToAptOrSuiteorOther;
            labelResponse.ToStreetAddress = response.ToStreetAddress;
            labelResponse.ToCity = response.ToCity;
            labelResponse.ToState = response.ToState;
            labelResponse.ToZip5 = response.ToZip5;
            labelResponse.ToZip4 = response.ToZip4;
            labelResponse.Postnet = response.Postnet;
            labelResponse.RDC = response.RDC;
            labelResponse.Postage = response.Postage;
            labelResponse.ExtraServices = response.ExtraServices;
            labelResponse.Zone = response.Zone;
            labelResponse.DimensionalWeight = response.DimensionalWeight;
            labelResponse.CarrierRoute = response.CarrierRoute;
            labelResponse.PermitHolderName = response.PermitHolderName;
            labelResponse.InductionType = response.InductionType;
            labelResponse.LogMessage = response.LogMessage;

            labelResponse.Commitment = response.Commitment != null ? new CommitmentDetail()
            {
                CommitmentName = response.Commitment.CommitmentName,

                ScheduledDeliveryDate = !response.Commitment.ScheduledDeliveryDate.IsNullOrWhiteSpace() ?
                DateTime.ParseExact(response.Commitment.ScheduledDeliveryDate, "yyyy-MM-dd", null) : (DateTime?)null
            } : null;

            labelResponse.Error = response.Error;

            return labelResponse;
        }
    }
}
