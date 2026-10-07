using Newtonsoft.Json.Linq;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthoMailAPI_UnitTest
{
    interface IToken
    {
        [Post("/token")]
        Task<JObject> GetToken([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);
    }
    public class AuthenticatedHttpClientHandler : HttpClientHandler
    {
        private string username;
        private string password;
        private string token = null;
        private DateTime tokenExpiresOn = DateTime.MinValue;

        public AuthenticatedHttpClientHandler(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        private async Task<string> GetToken(string endpoint)
        {
            if (token != null && tokenExpiresOn > DateTime.Now)
            {
                return token;
            }

            // If token is null or expired, get a new one

            var webapi = RestService.For<IToken>(endpoint);

            var data = new Dictionary<string, object> {
                {"grant_type", "password"},
                {"username", username},
                {"password", password}
            };

            var response = await webapi.GetToken(data);

            token = response["access_token"].ToString();
            tokenExpiresOn = DateTime.Parse(response[".expires"].ToString());

            return token;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // See if the request has an authorize header
            var auth = request.Headers.Authorization;
            if (auth != null)
            {
                var endpoint = request.RequestUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
                var token = await GetToken(endpoint).ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
