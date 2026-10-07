using HttpMultipartParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.V3
{
    public interface IUSPSAPIClient
    {
        Task<AddressResponseV3> GetAddress(Address request);

        Task<eVSResponseV3> GeteVSLabel(eVSRequestV3 request, string paymentToken);

        Task<string> GetLabelPaymentToken(LabelPaymentRequestV3 request);
    }

    public class USPSAPIClient : IUSPSAPIClient, IDisposable
    {
        readonly RestClient _client;
        private string baseUSPSURL;

        public USPSAPIClient(string apiKey, string apiKeySecret)
        {
            var apiUrl = ConfigurationManager.AppSettings["USPSAPIUrl"].ToString();
            //baseUSPSURL = "https://api-cat.usps.com";
            baseUSPSURL = apiUrl;

            var options = new RestClientOptions(baseUSPSURL)
            {
                Authenticator = new USPSAPIAuthenticator(baseUSPSURL, apiKey, apiKeySecret)
            };

            _client = new RestClient(options);
        }

        public async Task<AddressResponseV3> GetAddress(Address address)
        {
            try
            {
                RestRequest request = new RestRequest("addresses/v3/address", Method.Get);
                AddressResponseV3 response = new AddressResponseV3();

                // Add query parameters to the request URL
                request.AddParameter("streetAddress", address.Address1);

                if (!string.IsNullOrWhiteSpace(address.Address2))
                    request.AddParameter("secondaryAddress", address.Address2);

                if (!string.IsNullOrWhiteSpace(address.City))
                    request.AddParameter("city", address.City);

                request.AddParameter("state", address.State);

                request.AddParameter("ZIPCode", address.Zip5);

                if (!string.IsNullOrWhiteSpace(address.Zip4))
                    request.AddParameter("ZIPPlus4", address.Zip4);

                // Execute request and handle response
                var resp = await _client.ExecuteAsync(request).ConfigureAwait(false);

                if (resp != null && resp.IsSuccessful)
                {
                    response = JsonConvert.DeserializeObject<AddressResponseV3>(resp.Content);
                }
                else
                {
                    var err = JsonConvert.DeserializeObject<AddressErrorResponseV3>(resp.Content.Replace(@"\", @"\\"));
                    response.Error = err.error;
                }

                return response;//!.Data;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return new AddressResponseV3();
            }
        }

        public async Task<eVSResponseV3> GeteVSLabel(eVSRequestV3 evsRequest, string paymentToken)
        {
            try
            {
                RestRequest request = new RestRequest("labels/v3/label", Method.Post);
                eVSResponseV3 response = new eVSResponseV3();

                request.AddHeader("X-Payment-Authorization-Token", paymentToken);


                //var jsonString = JsonConvert.SerializeObject(evsRequest).Dump();

                var jsonString = JsonConvert.SerializeObject(evsRequest, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new IgnoreNullResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });

                request.AddJsonBody(jsonString);

                try
                {
                    // Execute request and handle response
                    var resp = await _client.ExecuteAsync(request).ConfigureAwait(false);

                    if (!resp.IsSuccessful)
                    {
                        response.Error = new Error { message = resp.Content };
                        //resp.Content = File.ReadAllText(@"C:\Users\dchhatriwala\Downloads\multipartdata.txt");
                    }

                    //var multi = new MultipartFormDataStreamProvider("C:\\");
                    if (resp != null && resp.IsSuccessful)
                    {
                        //string boundry = resp.Content.Split('\n').First().Trim().TrimStart('-').Dump();

                        //var part = ParseMultipartFormData(resp.Content.ToString(), boundry).Dump();

                        //response.labelMetadata = JsonConvert.DeserializeObject<Metadata>(part.FirstOrDefault(p => p.FileName == "labelMetadata")?.Data);
                        //response.returnLabelMetadata = JsonConvert.DeserializeObject<Metadata>(part.FirstOrDefault(p => p.FileName == "returnLabelMetadata")?.Data);
                        //response.labelImage = part.FirstOrDefault(p => p.FileName == "labelImage.pdf")?.Data.Dump();
                        //response.returnLabelImage = part.FirstOrDefault(p => p.FileName == "returnLabelImage.pdf")?.Data.Dump();
                        //response.receiptImage = part.FirstOrDefault(p => p.FileName == "receiptImage.pdf")?.Data;
                        //response.returnReceiptImage = part.FirstOrDefault(p => p.FileName == "returnReceiptImage.pdf")?.Data;

                        var contentBytes = System.Text.Encoding.UTF8.GetBytes(resp.Content); //contentBytes.Dump();
                        using (var stream = new MemoryStream(contentBytes))
                        {
                            //using var stream = new MemoryStream(resp.RawBytes);

                            //PDF pdf = new PDF(resp.Content);
                            //pdf.PDFTest();

                            var parser = await MultipartFormDataParser.ParseAsync(stream).ConfigureAwait(false);

                            //					var file = parser.Files.SingleOrDefault(f => f.Name == "labelImage");
                            //
                            //					using (FileStream fileStream = new FileStream($@"C:\Users\dchhatriwala\Downloads\{file.FileName}", FileMode.Create))
                            //					{
                            //						using (var reader = new StreamReader(file.Data))
                            //						{
                            //							// Read and decode the base64 encoded content
                            //							var decodedContent = Convert.FromBase64String(reader.ReadToEnd()).Dump();
                            //
                            //							// Write the decoded content to disk
                            //							var writer = new BinaryWriter(fileStream);
                            //							writer.Write(decodedContent, 0, decodedContent.Length);
                            //							writer.Close();
                            //						}
                            //					}

                            response.labelMetadata = JsonConvert.DeserializeObject<Metadata>(parser.GetParameterValue("labelMetadata"));
                            ////response.returnLabelMetadata = JsonConvert.DeserializeObject<Metadata>(parser.GetParameterValue("returnLabelMetadata"));
                            response.labelImage = ConvertStreamToBase64(parser.Files.SingleOrDefault(f => f.Name == "labelImage"));
                            //response.returnLabelImage = ConvertStreamToBase64(parser.Files.SingleOrDefault(f => f.Name == "returnLabelImage"));
                            response.receiptImage = ConvertStreamToBase64(parser.Files.SingleOrDefault(f => f.Name == "receiptImage"));
                            //response.returnReceiptImage = ConvertStreamToBase64(parser.Files.SingleOrDefault(f => f.Name == "returnReceiptImage"));
                        }
                    }
                    else
                    {
                        var err = JsonConvert.DeserializeObject<AddressErrorResponseV3>(resp.Content?.Replace(@"\", @"\\"));
                        response.Error = err.error;
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    response.Error = new Error { message = ex.ToString() };
                }

                return response;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return new eVSResponseV3();
            }
        }

        public static string ConvertStreamToBase64(FilePart file)
        {
            if (file == null)
            {
                return string.Empty;
            }
            //
            //	// Convert the stream to a byte array System.Text.Encoding.UTF8.GetBytes(resp.Content)
            //	byte[] bytes;
            //	using (MemoryStream memoryStream = new MemoryStream())
            //	{
            //		stream.CopyTo(memoryStream);
            //		bytes = memoryStream.ToArray();
            //	}

            // Convert the byte array to a Base64 string
            string base64String = string.Empty;

            //using (FileStream fileStream = new FileStream($@"C:\Users\dchhatriwala\Downloads\{file.FileName}", FileMode.Create))
            //{
            using (var reader = new StreamReader(file.Data))
            {
                // Read and decode the base64 encoded content

                base64String = reader.ReadToEnd();

                //var decodedContent = Convert.FromBase64String(base64String);

                // Write the decoded content to disk
                //var writer = new BinaryWriter(fileStream);
                //writer.Write(decodedContent, 0, decodedContent.Length);
                //writer.Close();
            }
            //}

            return base64String;
        }

        public async Task<string> GetLabelPaymentToken(LabelPaymentRequestV3 paymentRequest)
        {
            string token = string.Empty;

            var request = new RestRequest("/payments/v3/payment-authorization", Method.Post);
            request.AddHeader("Content-Type", "application/json");

            var jsonString = JsonConvert.SerializeObject(paymentRequest);
            request.AddJsonBody(jsonString);

            //request.AddJsonBody<LabelPaymentRequestV3>(paymentRequest);

            //var response1 = await _client.ExecuteAsync(request).Dump();

            var response = await _client.PostAsync<LabelPaymentResponseV3>(request);

            if (response != null)
            {
                token = response.paymentAuthorizationToken;
            }
            return token;
        }

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class USPSAPIAuthenticator : AuthenticatorBase
    {
        readonly string _baseUrl;
        readonly string _clientId;
        readonly string _clientSecret;
        private DateTime expiredTime;

        public USPSAPIAuthenticator(string baseUrl, string clientId, string clientSecret) : base("")
        {
            _baseUrl = baseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;

            //Token = string.IsNullOrEmpty(Token) || expiredTime < DateTime.Now ? GetToken() : Token;

            ValidateToken();
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            Token = string.IsNullOrEmpty(Token) || expiredTime < DateTime.Now ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, Token);
        }

        public bool ValidateToken()
        {
            Token = string.IsNullOrEmpty(Token) || expiredTime < DateTime.Now ? GetToken().Result : Token;

            return string.IsNullOrWhiteSpace(Token);
        }
        async Task<string> GetToken()
        {
            {
                USPSTokenResponse response = new USPSTokenResponse();

                //var requestJson = new USPSTokenRequest { ClientId = "6oZjA4lgwGPfJCHSeHpnxitxupQ1qGi0", ClientPassword = "Kp1Alu4oHV9G6WB6" };

                try
                {
                    var options = new RestClientOptions(_baseUrl)
                    {
                        Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret),
                    };

                    using (var client = new RestClient(options))
                    {
                        if (this._baseUrl.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        }

                        var requestJson = new USPSTokenRequest { client_id = _clientId, client_secret = _clientSecret, grant_type = "client_credentials" };

                        var request = new RestRequest("oauth2/v3/token", Method.Post);
                        request.AddHeader("Content-Type", "application/json");

                        request.AddJsonBody(requestJson);

                        //var json = JsonConvert.SerializeObject(requestJson);

                        //request.AddJsonBody(json);

                        response = await client.PostAsync<USPSTokenResponse>(request).ConfigureAwait(false);

                        //if (resp != null && resp.IsSuccessful)
                        //{
                        //    response = JsonConvert.DeserializeObject<USPSTokenResponse>(resp.Content);
                        //}

                        if (response != null)
                        {
                            int expires_in = Convert.ToInt32(response.expires_in);
                            DateTime tokenLife = DateTime.Now.AddSeconds(expires_in);
                            expiredTime = tokenLife;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var m = ex.ToString();
                }

                return $"{response?.token_type} {response?.access_token}";
            }
        }
    }
    public class IgnoreNullResolver : DefaultContractResolver
    {
        //protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        //{
        //    JsonProperty property = base.CreateProperty(member, memberSerialization);
        //    property.ShouldSerialize = instance =>
        //    {
        //        return property.PropertyType.IsValueType ? true : instance != null;
        //    };
        //    return property;
        //}

        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(string))
            {
                property.ShouldSerialize = instance =>
                {
                    var value = property.ValueProvider.GetValue(instance) as string;
                    return !string.IsNullOrWhiteSpace(value);
                };
            }
            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) &&
                     property.PropertyType != typeof(string))
            {
                property.ShouldSerialize = instance =>
                {
                    var value = property.ValueProvider.GetValue(instance) as System.Collections.IEnumerable;
                    return value != null && value.GetEnumerator().MoveNext();
                };
            }

            return property;
        }
    }
}