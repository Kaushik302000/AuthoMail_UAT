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

namespace AM.Services
{
    public partial class USPSService : ServiceBase, IUSPSService
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IEnumerable<string> ValidateAddress(AddressRequest address)
        {
            // Validate if address is null
            if (address == null)
            {
                yield return "Address cannot be NULL.";
            }

            // Validate address line
            if ("{0} {1}".Fill(address.AptOrSuiteorOther, address.StreetAddress).Trim().IsNullOrWhiteSpace())
            {
                yield return "Either address line 1 or line 2 must be specified.";
            }

            if (address.Zip5.IsNullOrWhiteSpace() && (address.City.IsNullOrWhiteSpace() || address.State.IsNullOrWhiteSpace()))
            {
                yield return "The address MUST contain either one of the following: 1. ZIP or 2. City and State.";
            }
        }

        public async Task<VerifierAddressResponse> VerifyAddressAsync(string userId, VerifierAddressRequest verifyRequest)
        {
            var addressRequests = verifyRequest.AddressRequests;

            List<AddressResponse> addressResponses = new List<AddressResponse>();

            try
            {
                int logId = 0;

                Task<int> dbLog = null;
                var maxAddressCount = Config.Web_API_USPS_AddressCount;
                addressResponses.Capacity = maxAddressCount;
                var isAPIError = false;
                string apiErrorMSG = null;

                //response field and api flag , Validate list length
                if (addressRequests == null || addressRequests.Count < 1 || addressRequests.Count > maxAddressCount)
                {
                    isAPIError = true;
                    apiErrorMSG = "In addressRequests At least one and at most 5 addresses must be specified.";
                }

                if (userId.IsNullOrWhiteSpace())
                {
                    isAPIError = true;
                    apiErrorMSG = string.Join("\r\n", new string[] { apiErrorMSG, "For addressRequests UserId must be specified." }.Where(msg => !msg.IsNullOrWhiteSpace()));
                }

                //check flag and call to db
                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    var addressRequestsXML = addressRequests.Serialize();

                    dbLog = db.ExecuteScalarAsync<int>("Insert into [API].[USPS] ([APIName], [UserId], [APIRequest], [APIRequestedOn],[IsAPIError],[IsUSPSError],[APIResponse], [APIResponseOn]) OUTPUT INSERTED.Id VALUES(@APIName, @UserId, @APIRequest, @APIRequestedOn, @IsAPIError, @IsUSPSError, @APIResponse, @APIResponseOn);", new { APIName = Config.Web_API_USPS_VerifyAPIName, UserId = userId, APIRequest = addressRequestsXML, APIRequestedOn = Common.Now, IsAPIError = isAPIError, IsUSPSError = false, APIResponse = apiErrorMSG, APIResponseOn = isAPIError ? Common.Now : (DateTime?)null });
                }

                if (isAPIError)
                {
                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        var dbLogawait = await dbLog.ConfigureAwait(false);
                    }

                    //throw new ArgumentException(apiErrorMSG);

                    return new VerifierAddressResponse(addressResponses) { Error = apiErrorMSG };
                }

                List<Address> addresses = new List<Address>(maxAddressCount);

                for (int i = 0; i < addressRequests.Count; i++)
                {
                    var request = addressRequests[i];
                    var validationErrors = ValidateAddress(request);

                    //Id index 1 base
                    var response = new AddressResponse(request, i + 1);

                    response.AddressTypes = new List<AddressType>();

                    if (validationErrors.Any())
                    {
                        response.Error = new USPSError
                        {
                            Source = ResponseSource.CLSVC,
                            Description = String.Join(Environment.NewLine, validationErrors)
                        };

                        response.Id = request.Id;
                        response.AddressTypes.Add(AddressType.INVALID);
                    }
                    else
                    {
                        addresses.Add(new Address
                        {
                            //index 1 base
                            ID = response.Id,
                            FirmName = request.FirmName,
                            Address1 = request.AptOrSuiteorOther,
                            Address2 = request.StreetAddress,
                            City = request.City.IsNullOrWhiteSpace() ? string.Empty : request.City.Trim(),
                            State = request.State.IsNullOrWhiteSpace() ? string.Empty : request.State.Trim(),
                            Zip5 = request.Zip5.IsNullOrWhiteSpace() ? string.Empty : request.Zip5.Trim(),
                            Zip4 = request.Zip4.IsNullOrWhiteSpace() ? string.Empty : request.Zip4.Trim(),
                            Urbanization = request.Urbanization.IsNullOrWhiteSpace() ? string.Empty : request.Urbanization.Trim()
                        });
                    }

                    addressResponses.Add(response);
                }

                //If ALL AddressRequest elements are invalid then error occur and address list is empty.
                if (addresses.Count == 0)
                {
                    return new VerifierAddressResponse(addressResponses);
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
                    var request = new AddressValidateRequest(Config.Web_API_USPS_UserId, "1");
                    //var request = new AddressValidateRequest("719MSTC04390", "1");//"719MSTC04390", "1");
                    request.AddressList.AddRange(addresses);

                    string url = Config.Web_API_USPS_URL;
                    //string url = @"https://secure.shippingapis.com/ShippingAPI.dll";//["USPSAPI"];

                    if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    }

                    uspsInputXML = request.Serialize();

                    payload = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("API",Config.Web_API_USPS_VerifyAPIName),
                            //new KeyValuePair<string, string>("API","Verify"),
                            new KeyValuePair<string, string>("XML",uspsInputXML)
                        });

                    //var payloadResult = await payload.ReadAsStringAsync().ConfigureAwait(false);
                    //Todo disable code for not check address
                    //var uspsReq = httpClient.PostAsync(url, payload);


                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                        uspsRequestLog = db.ExecuteAsync("Update [API].[USPS] set USPSRequest = @USPSRequest, USPSRequestedOn = @USPSRequestedOn where Id = @Id", new { USPSRequest = uspsInputXML, USPSRequestedOn = Common.Now, Id = logId });
                    }

                    try
                    {
                        //Todo disable code for not check address
                        //var response = await uspsReq.ConfigureAwait(false);
                        //uspsResponseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        //isGenericUSPSError = !uspsResponseString.Contains("AddressValidateResponse");

                        //Todo disable code for not check address
                        isGenericUSPSError = true;

                        if (isGenericUSPSError)
                        {
                            //var uspsError = uspsResponseString.Deserialize<USPSError>();

                            addresses.ForEach(add =>
                            {
                                add.Error = new USPSError { Description = "Disabled service for address verify", Source = "CLSVC" };// uspsError;
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

                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    var uspsRequestLogAwait = await uspsRequestLog.ConfigureAwait(false);

                    uspsResponseLog = db.ExecuteAsync("Update [API].[USPS] set USPSResponse = @USPSResponse , USPSResponseOn = @USPSResponseOn, IsUSPSError = @IsUSPSError, IsAPIError = @IsAPIError where Id = @Id", new { USPSResponse = uspsResponseString ?? "Disabled service for address verify", USPSResponseOn = Common.Now, IsUSPSError = isGenericUSPSError, IsAPIError = false, Id = logId });
                }

                if (isGenericUSPSError)
                {
                    SetResponseValues(addressResponses, addresses);
                }
                else
                {
                    var response = uspsResponseString.Deserialize<AddressValidateResponse>();

                    SetResponseValues(addressResponses, response.AddressList);
                }

                var verifierAddressResponse = new VerifierAddressResponse(addressResponses);

                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    var addressResponseXML = verifierAddressResponse.Serialize();

                    var uspsResponseLogAwait = await uspsResponseLog.ConfigureAwait(false);

                    var responseLog = db.ExecuteAsync("Update [API].[USPS] set APIResponse = @APIResponse ,  APIResponseOn = @APIResponseOn, IsAPIError = @IsAPIError Where Id = @Id", new { APIResponse = addressResponseXML, APIResponseOn = Common.Now, IsAPIError = addressResponses.Any(a => a.AddressTypes.Count == 0), Id = logId });

                    await responseLog.ConfigureAwait(false);
                }

                return verifierAddressResponse;
            }
            catch (Exception ex)
            {
                log.Error("Verify Address has this Unexpected Error : ", ex);
                return new VerifierAddressResponse(addressResponses)
                {
                    Error = "Unexpected Error"//ex.ToString()
                };
            }
        }

        private static void SetResponseValues(List<AddressResponse> addressResponses, List<Address> addresses)
        {
            var idRegex = new Regex(@"(\d+{0})(.*)".Fill(ResponseSource.CLSVC));

            foreach (Address address in addresses)
            {
                AddressResponse responseAdd = addressResponses.Single(res => res.Id == address.ID);

                responseAdd.Id = idRegex.Match(responseAdd.Id).Groups[2].Value;

                //if error by USPS then assign requested add to respond with source CLSVC
                if (address.HasError)
                {
                    responseAdd.FirmName = responseAdd.AddressRequest.FirmName;
                    responseAdd.AptOrSuiteorOther = responseAdd.AddressRequest.AptOrSuiteorOther;
                    responseAdd.StreetAddress = responseAdd.AddressRequest.StreetAddress;
                    responseAdd.City = responseAdd.AddressRequest.City;
                    responseAdd.State = responseAdd.AddressRequest.State;
                    responseAdd.Zip5 = responseAdd.AddressRequest.Zip5;
                    responseAdd.Zip4 = responseAdd.AddressRequest.Zip4;
                    responseAdd.Urbanization = responseAdd.AddressRequest.Urbanization;
                    responseAdd.City = responseAdd.AddressRequest.City;
                    responseAdd.AddressTypes = CheckAddressType(new Address { Address1 = responseAdd.AptOrSuiteorOther, Address2 = responseAdd.StreetAddress, State = responseAdd.State, City = responseAdd.City });

                    responseAdd.Error = address.Error;
                    responseAdd.Verifier = ResponseSource.CLSVC;
                    responseAdd.VerifierDesc = "USPS Failure : {0}".Fill(address.Error.Description);

                    if (responseAdd.AddressTypes.Count == 0)
                    {
                        responseAdd.VerifierDesc = "{0}\r\n* Invalid state code '{1}', AddressType could not be determined.*".Fill(responseAdd.VerifierDesc, responseAdd.State);
                        responseAdd.AddressTypes.Add(AddressType.INVALID);
                    }
                }
                else
                {
                    responseAdd.FirmName = address.FirmName;
                    responseAdd.AptOrSuiteorOther = address.Address1;
                    responseAdd.StreetAddress = address.Address2;
                    responseAdd.StreetAddressAbbr = address.AddressLine2Abbreviation;
                    responseAdd.City = address.City;
                    responseAdd.CityAbbr = address.CityAbbreviation;
                    responseAdd.State = address.State;
                    responseAdd.Zip5 = address.Zip5;
                    responseAdd.Zip4 = address.Zip4;
                    responseAdd.Urbanization = address.Urbanization;
                    responseAdd.CarrierRoute = address.CarrierRoute;
                    responseAdd.DeliveryPoint = address.DeliveryPoint;
                    responseAdd.Warning = address.ReturnText;
                    responseAdd.Error = address.Error;
                    responseAdd.AddressTypes = CheckAddressType(address);

                    responseAdd.Verifier = ResponseSource.USPS;
                    responseAdd.VerifierDesc = address.ReturnText.IsNullOrWhiteSpace() ? string.Empty : address.ReturnText;
                }
            }
        }

        static Regex poboxRgx = new Regex("[^a-zA-Z]");

        private static List<AddressType> CheckAddressType(Address address)
        {
            var addressTypes = new List<AddressType>();
            //var dhamStates = new string[] { "AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "HI", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE", "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VA", "VT", "WA", "WI", "WV", "WY" };
            var dhamStates = Config.Web_API_USPS_MainlandStates.Split(',').Select(ms => ms.Trim());

            var militarystates = Config.Web_API_USPS_MilitaryStates.Split(',').Select(ms => ms.Trim()); ;
            //var militarystates = new string[] { "AA", "AE", "AP" };
            var militaryCities = Config.Web_API_USPS_MilitaryCities.Split(',').Select(mc => mc.Trim()); ;
            //var militaryCities = new string[] { "APO", "DPO", "FPO" };
            var territory = Config.Web_API_USPS_Territory.Split(',').Select(t => t.Trim());
            //var territory = new string[] { "AS", "FM", "GU", "MH", "MP", "PR", "PW", "VI" };

            var fullAddress = "{0} {1}".Fill(address.Address1, address.Address2).ToLower().Trim();

            if (militarystates.Contains(address.State.ToUpper()) && militaryCities.Contains(address.City.ToUpper()))
                addressTypes.Add(AddressType.Military);

            if (territory.Contains(address.State.ToUpper()))
                addressTypes.Add(AddressType.Territory);

            if (dhamStates.Contains(address.State.ToUpper()))
                addressTypes.Add(AddressType.DHAM);

            if (addressTypes.Count > 0)
            {
                if (poboxRgx.Replace(fullAddress, "").ToLower().Contains("pobox"))
                    addressTypes.Add(AddressType.POBOX);
            }

            return addressTypes;
        }
    }
}
