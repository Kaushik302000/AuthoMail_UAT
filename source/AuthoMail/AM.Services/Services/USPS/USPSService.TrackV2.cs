using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Net;
using Dapper;
using System.Net.Http;
using System.Text.RegularExpressions;
using AM.USPSLib;
using System.IO;
using log4net;
using AM.Services.USPS.TrackV2;
using AM.USPSLib.TrackV2;

namespace AM.Services
{
    public partial class USPSService : ServiceBase, IUSPSService
    {
        private IEnumerable<string> ValidateTrackField(TrackID address)
        {
            // Validate if address is null
            if (address == null)
            {
                yield return "Track Request cannot be NULL.";
            }

            // Validate address line
            if (address.ID.IsNullOrWhiteSpace())
            {
                yield return "Track Id must be specified.";
            }

            //if (address.Zip5.IsNullOrWhiteSpace() && (address.City.IsNullOrWhiteSpace() || address.State.IsNullOrWhiteSpace()))
            //{
            //    yield return "The address MUST contain either one of the following: 1. ZIP or 2. City and State.";
            //}
        }

        public async Task<TrackAPIResponse> GetTrackingDetailsAsync(string userId, TrackFieldAPIRequest verifyRequest)
        {
            List<TrackInfo> addressResponses = new List<TrackInfo>();
            try
            {

                int logId = 0;

                Task<int> dbLog = null;
                var maxAddressCount = Config.Web_API_USPS_TrackingCount;
                addressResponses.Capacity = maxAddressCount;
                var isAPIError = false;
                string apiErrorMSG = null;

                //response field and api flag , Validate list length
                if (verifyRequest == null || verifyRequest.TrackID.Count < 1 || verifyRequest.TrackID.Count > maxAddressCount)
                {
                    isAPIError = true;
                    apiErrorMSG = $"In Track Requests At least one and at most {maxAddressCount} addresses must be specified.";
                }

                if (userId.IsNullOrWhiteSpace())
                {
                    isAPIError = true;
                    apiErrorMSG = string.Join("\r\n", new string[] { apiErrorMSG, "For addressRequests UserId must be specified." }.Where(msg => !msg.IsNullOrWhiteSpace()));
                }

                //check flag and call to db
                if (Config.Web_API_USPS_Tracking_IsDbLoggingEnabled)
                {
                    var addressRequestsXML = verifyRequest.Serialize();

                    dbLog = db.ExecuteScalarAsync<int>("Insert into [API].[USPS] ([APIName], [UserId], [APIRequest], [APIRequestedOn],[IsAPIError],[IsUSPSError],[APIResponse], [APIResponseOn]) OUTPUT INSERTED.Id VALUES(@APIName, @UserId, @APIRequest, @APIRequestedOn, @IsAPIError, @IsUSPSError, @APIResponse, @APIResponseOn);", new { APIName = Config.Web_API_USPS_TrackingAPIName, UserId = userId, APIRequest = addressRequestsXML, APIRequestedOn = Common.Now, IsAPIError = isAPIError, IsUSPSError = false, APIResponse = apiErrorMSG, APIResponseOn = isAPIError ? Common.Now : (DateTime?)null });
                }

                if (isAPIError)
                {
                    if (Config.Web_API_USPS_Tracking_IsDbLoggingEnabled)
                    {
                        var dbLogawait = await dbLog.ConfigureAwait(false);
                    }

                    //throw new ArgumentException(apiErrorMSG);

                    return new TrackAPIResponse() { Error = apiErrorMSG };
                }

                List<TrackID> addresses = new List<TrackID>(maxAddressCount);

                for (int i = 0; i < verifyRequest.TrackID.Count; i++)
                {
                    var request = verifyRequest.TrackID[i];
                    var validationErrors = ValidateTrackField(request);

                    //Id index 1 base
                    var response = new TrackInfo() { ID = request.ID };// request, i + 1);

                    //response.AddressTypes = new List<AddressType>();

                    if (validationErrors.Any())
                    {
                        response.Error = new USPSError
                        {
                            Source = ResponseSource.CLSVC,
                            Description = String.Join(Environment.NewLine, validationErrors)
                        };

                        //response.ID = request.ID;
                        //response.AddressTypes.Add(AddressType.INVALID);
                    }
                    else
                    {
                        addresses.Add(new TrackID() { ID = request.ID });
                    }

                    addressResponses.Add(response);
                }

                //If ALL AddressRequest elements are invalid then error occur and address list is empty.
                if (addresses.Count == 0)
                {
                    return new TrackAPIResponse(addressResponses);
                    //{
                    //    Error = "Common-XmlParse : The element 'AddressValidateRequest' has incomplete content. List of possible elements expected: 'Address'."
                    //};
                }

                FormUrlEncodedContent payload = null;
                string uspsInputXML = null;
                Task<int> uspsRequestLog = null;
                string uspsResponseString = null;
                bool isGenericUSPSError = false;

                using (HttpClient httpClient = new HttpClient())
                {
                    var request = new TrackFieldRequest(Config.Web_API_USPS_UserId, "0", Config.Web_API_USPS_TrackingSourseZipCode);//"75063");
                    //var request = new AddressValidateRequest("719MSTC04390", "1");//"719MSTC04390", "1");
                    request.TrackID.AddRange(addresses);

                    string url = Config.Web_API_USPS_URL;
                    //string url = @"https://secure.shippingapis.com/ShippingAPI.dll";//["USPSAPI"];

                    if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    }

                    uspsInputXML = request.Serialize<TrackFieldRequest>();

                    payload = new FormUrlEncodedContent(new[]
                        {
                            //todo change APIName
                            new KeyValuePair<string, string>("API",Config.Web_API_USPS_TrackingAPIName),
                            //new KeyValuePair<string, string>("API","TrackV2"),
                            new KeyValuePair<string, string>("XML",uspsInputXML)
                        });

                    //var payloadResult = await payload.ReadAsStringAsync().ConfigureAwait(false);
                    var uspsReq = httpClient.PostAsync(url, payload);


                    if (Config.Web_API_USPS_Tracking_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                        uspsRequestLog = db.ExecuteAsync("Update [API].[USPS] set USPSRequest = @USPSRequest, USPSRequestedOn = @USPSRequestedOn where Id = @Id", new { USPSRequest = uspsInputXML, USPSRequestedOn = Common.Now, Id = logId });
                    }

                    try
                    {
                        var response = await uspsReq.ConfigureAwait(false);
                        uspsResponseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        isGenericUSPSError = !uspsResponseString.Contains("TrackResponse");

                        if (isGenericUSPSError)
                        {
                            var uspsError = uspsResponseString.Deserialize<USPSError>();

                            addresses.ForEach(add =>
                            {
                                //todo check
                                add.Error = uspsError;
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Verify hard fail while download", ex);

                        isGenericUSPSError = true;

                        uspsResponseString = "{0}\r\n{1}\r\n{2}".Fill(uspsResponseString, new string('-', 105), ex.ToString());
                        string err = "{0}\r\n{1}".Fill(ex.Message, uspsResponseString);

                        // Store exception details received while accessing USPS API in addresses collection
                        addresses.ForEach(add =>
                        {
                            //todo check
                            add.Error = new USPSError
                            {
                                Source = ResponseSource.USPS,
                                Description = err
                            };
                        });

                        //SetResponseValues(addressResponses, idRegex, addresses, ex);
                    }
                }

                Task<int> uspsResponseLog = null;

                if (Config.Web_API_USPS_Tracking_IsDbLoggingEnabled)
                {
                    var uspsRequestLogAwait = await uspsRequestLog.ConfigureAwait(false);

                    uspsResponseLog = db.ExecuteAsync("Update [API].[USPS] set USPSResponse = @USPSResponse , USPSResponseOn = @USPSResponseOn, IsUSPSError = @IsUSPSError, IsAPIError = @IsAPIError where Id = @Id", new { USPSResponse = uspsResponseString, USPSResponseOn = Common.Now, IsUSPSError = isGenericUSPSError, IsAPIError = false, Id = logId });
                }

                if (isGenericUSPSError)
                {
                    addressResponses.ForEach(i => { i.Error = addresses.SingleOrDefault(s => s.ID == i.ID)?.Error; });

                    //SetResponseValues(addressResponses, addresses);
                }
                else
                {
                    var response = uspsResponseString.Deserialize<TrackResponse>();

                    //addressResponses.ForEach(i =>
                    //{
                    //    var info = response.TrackInfo.SingleOrDefault(s => s.ID == i.ID);
                    //    if (info != null)
                    //    {
                    //        i = info;
                    //    }
                    //});

                    for (int i = 0; i < addressResponses.Count; i++)
                    {
                        var trackResponse = response.TrackInfo.SingleOrDefault(s => s.ID == addressResponses[i].ID);
                        if (trackResponse != null)
                        {
                            addressResponses[i] = trackResponse;
                        }
                    }

                    //SetResponseValues(addressResponses, response.TrackInfo);
                }

                var verifierAddressResponse = new TrackAPIResponse(addressResponses);

                if (Config.Web_API_USPS_Tracking_IsDbLoggingEnabled)
                {
                    var addressResponseXML = verifierAddressResponse.Serialize();

                    var uspsResponseLogAwait = await uspsResponseLog.ConfigureAwait(false);

                    var responseLog = db.ExecuteAsync("Update [API].[USPS] set APIResponse = @APIResponse ,  APIResponseOn = @APIResponseOn, IsAPIError = @IsAPIError Where Id = @Id", new { APIResponse = addressResponseXML, APIResponseOn = Common.Now, IsAPIError = addressResponses.Any(a => a.Error == null), Id = logId });

                    await responseLog.ConfigureAwait(false);
                }

                return verifierAddressResponse;
            }
            catch (Exception ex)
            {
                log.Error("Verify Address has this Unexpected Error : ", ex);
                return new TrackAPIResponse(addressResponses)
                {
                    Error = "Unexpected Error"//ex.ToString()
                };
            }
        }
    }
}
