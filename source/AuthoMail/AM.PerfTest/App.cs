using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AM.PerfTest
{
    public class App
    {
        private Config _config;
        public App(IOptionsMonitor<Config> config)
        {
            _config = config.CurrentValue;
        }

        public async Task RunAsync(string[] args)
        {
            int total_requests = int.Parse(args[1]);
            int batch_size = int.Parse(args[2]);

            var batches = Enumerable.Range(0, total_requests)
                .Batch(batch_size);

            //var batchJob = new List<Task>(batch_size);
            var count = 0;
            using (HttpClient client = new HttpClient())
            {
                //setup client
                client.BaseAddress = new Uri(_config.BaseURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = new TimeSpan(0, 5, 0);

                foreach (var batch in batches)
                {
                    var job = batch.Select(t => SendMailRequest(client));
                    var batchTasks = job.ToArray();
                    await Task.WhenAll(batchTasks);
                    count += batchTasks.Length;
                    Console.WriteLine($"{count} complete.");
                }
            }
        }

        private MailPiece CreateMailPieceAndSendRequest()
        {
            MailPiece mailPiece = new MailPiece
            {
                Id = Guid.NewGuid(),
                ClientCode = _config.ClientCode,
                BusinessFriendlyIdName = "LoanId",
                BusinessFriendlyIdValue = "9ad105a5-8906-4ac0-899f-663ceaa32fbd",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = _config.PdfName,
                    Files = new List<string> { "F1" }
                },
                DocumentOrPackageType = "Disclosures",
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Fulfillment",
                        LastName = "Test User"
                    },
                    Address = new Address()
                    {
                        Street = "4430 Rosewood drive",
                        City = "pleasanton",
                        State = "CA",
                        Zip = "94588"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "John",
                        LastName = "Vito Paciulli 3rd"
                    },
                    Address = new Address()
                    {
                        Street = "4430 Rosewood drive",
                        City = "pleasanton",
                        State = "CA",
                        Zip = "94588"
                    }
                },
                CustomInfo = new List<Metadata>()
                {
                    new Metadata()
                    {
                        Key = "Appraisal Notice8550",
                        DataType = DataType.String,
                        Value = "FileName:Appraisal Notice8550 | PageCount: 5"
                    }
                }
            };

            return mailPiece;

        }


        private async Task<MailAPIResponse> SendMailRequest(HttpClient client)
        {
            var mailPiece = CreateMailPieceAndSendRequest();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //setup login data
            var formContent = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", _config.UName),
                    new KeyValuePair<string, string>("password", _config.Password)
                });

            //send request
            HttpResponseMessage responseMessage = await client.PostAsync("/token", formContent);

            //get access token from response body
            var responseJson = await responseMessage.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(responseJson);
            var token = jObject.GetValue("access_token").ToString();
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(new StringContent(JsonConvert.SerializeObject(mailPiece), UnicodeEncoding.UTF8, "application/json"), "Manifest");

            form.Add(new StreamContent(new FileStream(_config.PdfName, FileMode.Open, FileAccess.Read)), "F1", _config.PdfName);
            HttpResponseMessage res = await client.PostAsync("/api/mailrequest", form);
            string sd = res.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<MailAPIResponse>(sd);

        }
    }
}
