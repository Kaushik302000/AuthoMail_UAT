using AM.USPSLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Net.Http;
using Dapper;
using log4net;
using System.Net;

namespace AM.Services
{
    public partial class USPSService
    {
        public async Task<eVSCancelLabelResponse> RequesteVSCancelLabelAsync(string userId, eVSCancelLabelRequest request)
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
                    apiErrorMSG = "For create Cancel eVS Request, UserId must be specified and Request At least one.";
                }

                var labelInputXML = request.Serialize();

                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    dbLog = db.ExecuteScalarAsync<int>("Insert into [API].[USPSeVSCancel] ([APIName], [UserId], [APIRequest], [APIRequestedOn],[IsAPIError],[IsUSPSError],[APIResponse], [APIResponseOn]) OUTPUT INSERTED.Id VALUES(@APIName, @UserId, @APIRequest, @APIRequestedOn, @IsAPIError, @IsUSPSError, @APIResponse, @APIResponseOn);", new { APIName = Config.Web_API_USPS_eVSCancelAPIName, UserId = userId, APIRequest = labelInputXML, APIRequestedOn = Common.Now, IsAPIError = isAPIError, IsUSPSError = false, APIResponse = apiErrorMSG, APIResponseOn = isAPIError ? Common.Now : (DateTime?)null });
                }

                if (isAPIError)
                {
                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                    }

                    return new eVSCancelLabelResponse()
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
                eVSCancelResponse eVSCancelResponse = null;

                var eVSCancelrequest = new eVSCancelRequest(request);
                eVSCancelrequest.USERID = Config.Web_API_USPS_UserId;

                var eVSCancelrequestXML = eVSCancelrequest.Serialize();

                using (HttpClient httpClient = new HttpClient())
                {
                    string url = Config.Web_API_USPS_URL;
                    //string url = @"https://stg-secure.shippingapis.com/ShippingAPI.dll";

                    if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    }

                    payload = new FormUrlEncodedContent(new[] 
                        {
                            new KeyValuePair<string, string>("API",Config.Web_API_USPS_eVSCancelAPIName),
                            //new KeyValuePair<string, string>("API","Verify"),
                            new KeyValuePair<string, string>("XML",eVSCancelrequestXML)
                        });

                    //var payloadResult = await payload.ReadAsStringAsync().ConfigureAwait(false);
                    var uspsReq = httpClient.PostAsync(url, payload);

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                        uspsReqLog = db.ExecuteAsync("Update [API].[USPSeVSCancel] set [USPSRequest] = @USPSRequest, [USPSRequestedOn]= @USPSRequestedOn Where Id =@Id;", new { USPSRequest = eVSCancelrequestXML, USPSRequestedOn = Common.Now, Id = logId });
                    }

                    string responseXML = null;
                    bool isUSPSError = false;

                    try
                    {
                        var response = await uspsReq.ConfigureAwait(false);
                        responseXML = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        isUSPSError = !responseXML.Contains("eVSCancelResponse");

                        if (isUSPSError)
                        {
                            var err = responseXML.Deserialize<USPSError>();
                            eVSCancelResponse = new eVSCancelResponse { Error = err };
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("eVS cancel hard fail while download", ex);

                        responseXML = "{0}\r\n{1}\r\n{2}".Fill(responseXML, new string('-', 105), ex.ToString());
                        string err = "{0}\r\n{1}".Fill(ex.Message, responseXML);

                        isUSPSError = true;
                        eVSCancelResponse = new eVSCancelResponse
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
                        uspsResXMLLog = db.ExecuteAsync("Update [API].[USPSeVSCancel] set IsUSPSError = @IsUSPSError, [USPSResponse] = @USPSResponse, [USPSResponseOn] = @USPSResponseOn Where Id = @Id;", new { IsUSPSError = isUSPSError, USPSResponse = responseXML, USPSResponseOn = Common.Now, Id = logId });
                    }

                    if (!isUSPSError)
                    {
                        eVSCancelResponse = responseXML.Deserialize<eVSCancelResponse>();
                    }

                    var eVSCancelLabelresponse = eVSCancelLabelResponseObject(eVSCancelResponse);

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        var apiResponseXML = eVSCancelLabelresponse.Serialize();

                        uspsResXMLLogAwait = await uspsResXMLLog.ConfigureAwait(false);
                        apiResponseLog = db.ExecuteAsync("Update [API].[USPSeVSCancel] set APIResponse = @APIResponse, APIResponseOn = @APIResponseOn, BarcodeNumber = @BarcodeNumber, Status = @Status,Reason = @Reason Where Id = @Id", new { APIResponse = apiResponseXML, APIResponseOn = Common.Now, BarcodeNumber = eVSCancelResponse.BarcodeNumber, Status = eVSCancelResponse.Status.ToString(), Reason = eVSCancelResponse.Reason.ToString(), Id = logId });
                    }

                    apiResponseLogAwait = await apiResponseLog.ConfigureAwait(false);

                    return eVSCancelLabelresponse;
                }
            }
            catch (Exception ex)
            {
                log.Error("eVS Cancel Label has this Unexpected Error : ", ex);

                return new eVSCancelLabelResponse()
                {
                    Error = new USPSError()
                    {
                        Source = ResponseSource.CLSVC,
                        Description = "Unexpected error"
                    }
                };
            }
        }
        private eVSCancelLabelResponse eVSCancelLabelResponseObject(eVSCancelResponse eVSCancelResponse)
        {
            return new eVSCancelLabelResponse()
              {
                  BarcodeNumber = eVSCancelResponse.BarcodeNumber,
                  Status = eVSCancelResponse.Status,
                  Reason = eVSCancelResponse.Reason,
                  Error = eVSCancelResponse.Error
              };
        }
    }
}
