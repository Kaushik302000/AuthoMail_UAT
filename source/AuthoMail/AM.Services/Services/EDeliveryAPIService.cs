using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IEDeliveryAPIService : IDisposable
    {
        Task<SigningRoomResponse> CreateSigningRoom(SigningRoomRequest signingRoomRequest, int clientId);
        Task<bool> SendInvitations(int signingRoomId, int clientId);
        Task<List<SigningRoomActivityLogResponse>> GetSigningRoomActivityLog(string signingRoomName, int clientId);
        Task<string> GetSigningRoomStatus(int signingRoomId, int clientId);
        Task<bool> DeleteSigningRoom(int signingRoomId, int clientId);
        Task<EsignDocDownload> DownloadEsignedDocument(int docId, int clientId);
    }
    public class EDeliveryAPIService : ServiceBase, IEDeliveryAPIService
    {
        public async Task<SigningRoomResponse> CreateSigningRoom(SigningRoomRequest signingRoomRequest, int clientId)
        {
            try
            {
                var token = await GetToken(clientId);
                using (var client = SetupClient(token))
                {
                    string json = JsonConvert.SerializeObject(signingRoomRequest);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(Config.E_API_CreateSigningRoomURL, content); //"api/SigningRoomManager/SigningRooms/CreateFromPackage"
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.OK)
                    {
                        return new SigningRoomResponse()
                        {
                            Error = responseString
                        };
                    }
                    SigningRoomResponse signingRoomResponse = JsonConvert.DeserializeObject<SigningRoomResponse>(responseString);
                    return signingRoomResponse;
                }
            }
            catch (Exception ex)
            {
                return new SigningRoomResponse()
                {
                    Error = string.Format("Unexpected error occured while creating signing room : {0}", ex.ToString())
                };
            }
        }

        public async Task<bool> SendInvitations(int signingRoomId, int clientId)
        {
            bool isInvitationSent = false;
            try
            {
                var token = await GetToken(clientId);
                using (var client = SetupClient(token))
                {
                    StringContent content = new StringContent("", Encoding.UTF8, "application/json");

                    //make request
                    HttpResponseMessage response = await client.PostAsync(string.Format(Config.E_API_SendInvitationURL, signingRoomId), content); //"api/SigningRoomManager/SigningRooms/{0}/Participants/SendInvitations"
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.OK)
                        isInvitationSent = true;
                    return isInvitationSent;
                }
            }
            catch (Exception)
            {
                return isInvitationSent;
            }
        }

        public async Task<List<SigningRoomActivityLogResponse>> GetSigningRoomActivityLog(string signingRoomName, int clientId)
        {
            try
            {
                var token = await GetToken(clientId);
                using (var client = SetupClient(token))
                {
                    string json = @"{ 
                                        'SigningRoomName': '" + signingRoomName + "' " +
                                   "}";
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(Config.E_API_SigningRoomActivityLogSearchURL, content);//"api/Reports/SigningRoomActivityLogSearch"
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.OK)
                    {
                        return new List<SigningRoomActivityLogResponse>()
                        {
                            new SigningRoomActivityLogResponse
                            {
                                Error = responseString
                            }
                        };
                    }
                    List<SigningRoomActivityLogResponse> signingRoomActivityLogResponse = JsonConvert.DeserializeObject<List<SigningRoomActivityLogResponse>>(responseString);
                    return signingRoomActivityLogResponse;
                }
            }
            catch (Exception ex)
            {
                return new List<SigningRoomActivityLogResponse>()
                {
                    new SigningRoomActivityLogResponse
                    {
                        Error = string.Format("Unexpected error occured while getting signing room activity logs: {0}", ex.ToString())
                    }
                };
            }
        }

        public async Task<string> GetSigningRoomStatus(int signingRoomId, int clientId)
        {
            try
            {
                var token = await GetToken(clientId);
                using (var client = SetupClient(token))
                {
                    //make request
                    HttpResponseMessage response = await client.GetAsync(string.Format(Config.E_API_SigningRoomSummaryURL, signingRoomId)); //"api/SigningRoomManager/SigningRooms/{0}/signingroomsummary"
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.OK)
                    {
                        return responseString;
                    }
                    var jObject = JObject.Parse(responseString);
                    return jObject.GetValue("Status").ToString();
                }
            }
            catch (Exception ex)
            {
                return string.Format("Unexpected error occured while getting signing room status : {0}", ex.ToString());
            }
        }

        private HttpClient SetupClient(string token)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Config.E_API_BaseURL); //"http://10.215.48.138:8080/"
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        public async Task<string> GetToken(int clientId)
        {
            try
            {
                var cred = await GetAPICredential(clientId);
                using (var client = new HttpClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    //setup client
                    client.BaseAddress = new Uri(Config.E_API_BaseURL); // http://10.215.48.138:8080/
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //setup login data
                    var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", cred.SecretKey), // "3A57E75F-E25A-4077-9B59-C3766489562C"
                    new KeyValuePair<string, string>("username", cred.UserName), //"AksharClientAdmin"
                    new KeyValuePair<string, string>("password", cred.Password) //"Intellimods123#"
                });

                    //send request
                    HttpResponseMessage responseMessage = await client.PostAsync("/token", formContent);

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

        public async Task<bool> DeleteSigningRoom(int signingRoomId, int clientId)
        {
            bool isSigningRoomDelete = false;
            try
            {
                var token = await GetToken(clientId);
                using (var client = SetupClient(token))
                {
                    //make request
                    HttpResponseMessage response = await client.DeleteAsync(string.Format(Config.E_API_DeleteSigningRoomURL, signingRoomId)); //api/SigningRoomManager/SigningRooms/{signingRoomId}
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.OK)
                        isSigningRoomDelete = true;
                    return isSigningRoomDelete;
                }
            }
            catch (Exception)
            {
                return isSigningRoomDelete;
            }
        }

        public async Task<EsignDocDownload> DownloadEsignedDocument(int docId, int clientId)
        {
            try
            {
                var token = await GetToken(clientId);
                using (var client = SetupClient(token))
                {
                    //make request
                    HttpResponseMessage response = await client.GetAsync(string.Format(Config.E_API_DownloadEsignedDocURL, docId));
                    var responseString = await response.Content.ReadAsStringAsync();
                    EsignDocDownload esignDocDownload = JsonConvert.DeserializeObject<EsignDocDownload>(responseString);
                    return esignDocDownload;
                }
            }
            catch (Exception ex)
            {
                return new EsignDocDownload() { HasError = true, ErrorDesc = ex.ToString() };
            }
        }

        internal async Task<APICredential> GetAPICredential(int clientId)
        {
            return await db.QuerySingleOrDefaultAsync<APICredential>("SELECT * FROM API.Credentials WHERE ClientId = @ClientId", new { ClientId = clientId });
        }
    }
}
