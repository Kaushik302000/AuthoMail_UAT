using AM.Services;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using AM.Lib;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AM.Web.Models
{
    public class CLAuthentication
    {
        public static bool HasCorelogicAccount(string Email)
        {
            if (Email.IsNullOrWhiteSpace()) return false;
            var mailAddress = new MailAddress(Email);
            if (mailAddress.Host.Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase) || mailAddress.Host.Equals("cotality.com", StringComparison.CurrentCultureIgnoreCase)) return true;
            return false;
        }
        public static KeyValuePair<bool, string> LdapLogin(string userName, string password)
        {
            var mailAddress = new MailAddress(userName);
            if (mailAddress.Host.Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase))
            {
                if (userName.IndexOf('@') != -1) userName = userName.Split('@').First();
                string adPath = string.Format("LDAP://infosolco.net");
                LdapAuthentication auth = new LdapAuthentication(adPath);
                return auth.IsAuthenticated("isc", userName, password);
            }
            else
            {
                if (userName.IndexOf('@') != -1) userName = userName.Split('@').First();
                string adPath = string.Format("LDAP://infosolco.net");
                LdapAuthentication auth = new LdapAuthentication(adPath);
                return auth.IsAuthenticated("isc", userName, password);
            }            
        }

        public static async Task<KeyValuePair<bool, string>> IsValidCLUser(string username)
        {
            var completeEmailId = $"{username.ToLower()}@corelogic.com";
            string token = await GenerateToken();
            using (HttpClient httpClient = new HttpClient())
            {
                System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                string url = $"{Config.CLAuth_UserDetailsUrl}".Replace("{OrganizationId}", Config.CLAuth_OrganizationId).Replace("{UserName}", completeEmailId);
                HttpResponseMessage httpResponse = await httpClient.GetAsync(url);
                if (httpResponse.IsSuccessStatusCode)
                {
                    if (Config.Common_Environment != "Prod")
                    {
                        return new KeyValuePair<bool, string>(true, "Valid user");
                    }
                    else
                    {
                        var response = await httpResponse.Content.ReadAsStringAsync();

                        var userInfo = JsonConvert.DeserializeObject<CLUser>(response);

                        if (userInfo != null && userInfo.applicationCode.Contains("AUTHOMAIL"))
                        {
                            return new KeyValuePair<bool, string>(true, "Valid user");
                        }
                    }
                    return new KeyValuePair<bool, string>(false, "Not valid user");
                }
                else
                {
                    string error = await httpResponse.Content.ReadAsStringAsync();
                    return new KeyValuePair<bool, string>(false, error.ToString());
                }
            }
        }
        private static async Task<string> GenerateToken()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{Config.CLAuth_Username}:{Config.CLAuth_Password}")));
                    HttpResponseMessage tokenResponse = await httpClient.PostAsync(Config.CLAuth_TokenUrl, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));
                    var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
                    var jObj = JObject.Parse(jsonContent);

                    if (jObj == null || jObj.GetValue("access_token") == null)
                    {
                        var msg = $"Not able to generate token.";
                        throw new Exception(msg);
                    }
                    return jObj.GetValue("access_token").ToString();
                }

            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
    public class LdapAuthentication
    {
        private String _path;
        private String _filterAttribute;

        public LdapAuthentication(String path)
        {
            _path = path;
        }

        public KeyValuePair<bool, string> IsAuthenticated(String domain, String username, String pwd)
        {
            String domainAndUsername = domain + @"\" + username;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);

            try
            {
                Object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("givenName");
                search.PropertiesToLoad.Add("sn");
                search.PropertiesToLoad.Add("displayName");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return new KeyValuePair<bool, string>(false, string.Empty);
                }

                _filterAttribute = (String)result.Properties["cn"][0];
                string userData = string.Format("{0}|{1}|{2}", (string)result.Properties["givenName"][0], result.Properties.Contains("sn") ? (string)result.Properties["sn"]?[0] : "", (string)result.Properties["displayName"][0]);
                return new KeyValuePair<bool, string>(true, userData);
            }
            catch (COMException ec)
            {
                return new KeyValuePair<bool, string>(false, ec.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error authenticating user. " + ex.Message);
            }
        }

    }
    public class EmailValidate
    {
        public string Email { get; set; }
        public string Host { get; set; }
        public string User { get; set; }
        public bool IsCoreLogic { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public EmailValidate(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);
                Email = mailAddress.Address;
                Host = mailAddress.Host;
                User = mailAddress.User;
                IsCoreLogic =CLAuthentication.HasCorelogicAccount(email);
                IsValid = true;
            }
            catch (Exception)
            {
                IsValid = false;
                Message = "Invalid email address.";
            }
        }
    }
}