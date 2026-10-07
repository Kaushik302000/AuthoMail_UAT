using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Dapper;

namespace AuthoMailAPI_UnitTest
{
    /// <summary>
    /// Summary description for AllScenarios
    /// </summary>
    [TestClass]
    public class AllScenarios
    {
        Guid mpId = Guid.Parse("29C518A8-4862-408F-B779-E0ED4BA0D46B");
        public AllScenarios()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext) 
        //{

        //}
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region ValidScenarios
        [TestMethod]
        public async Task AM_SinglePdfValid()
        {
            var files = new List<string>();
            if (ConfigurationManager.AppSettings["TokenURL"] == "https://authomail.corelogic.com/token")
            {
                files.Add(@"PdfSample\encrypetedFile2pub.pdf");
            }
            else
            {
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
            }
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),//mpId,
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };

            //dynamic expectedValues = new JObject();

            //expectedValues.ShippingCarrier = "USPS";
            //expectedValues.PrintFormat = "Simplex";
            //expectedValues.IsBRE = false;
            //expectedValues.PrintLocation = "Las Vegas";

            //expectedValues.QCPercent = 0;
            //expectedValues.TotalPages = 8;
            //expectedValues.PrintTrailName = "GENERIC_PRIEXP";
            ////this is not in view
            //expectedValues.VendorName = "Nordis Technologies";
            ////this is not in view
            //expectedValues.EnvelopeName = "TEST_USPS_PRIEXP";
            ////expectedValues.SweepTime = "";
            ////this is not in view
            //expectedValues.PdfFileSize = 70;

            //expectedValues.IsPulledBack = 0;

            //SetDefaultExpectedValuesJson(expectedValues, mailPiece, files);

            //// CHANGE MANUALLY IF NEEDED

            ////expectedValues.ErrCode = "No_Error";
            ////expectedValues.IsQCPerformed = false;
            ////expectedValues.ShippingStatus = "Shipped";


            //bool isRegistered = RegisterTestCase(mailPiece.Id.ToString(), expectedValues.ToString(), this.GetMethodName());

            //Assert.AreEqual(true, isRegistered);

            //if (isRegistered)
            //{
            //    var response = await GenerateLocalResponse(files, mailPiece);
            //    Assert.AreEqual(mailPiece.Id, response.Id);
            //    Assert.AreEqual("Your request is queued for processing.", response.Message);
            //    Assert.AreEqual("Acknowledgement", response.MailingStatus);
            //}


            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual(mailPiece.Id, response.Id);
            Assert.AreEqual("Your request is queued for processing.", response.Message);
            Assert.AreEqual("Acknowledgement", response.MailingStatus);

        }

        [TestMethod]
        public async Task AM_TwoPdfValid()
        {
            var files = new List<string>();
            if (ConfigurationManager.AppSettings["TokenURL"] == "https://authomail.corelogic.com/token")
            {
                files.Add(@"PdfSample\encrypetedFile2pub.pdf");
                files.Add(@"PdfSample\encrypetedFile2pub.pdf");
            }
            else
            {
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
            }
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1", "F2" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual(mailPiece.Id, response.Id);
            Assert.AreEqual("Your request is queued for processing.", response.Message);
            Assert.AreEqual("Acknowledgement", response.MailingStatus);
        }

        [TestMethod]
        public async Task AM_ThreePdfValid()
        {
            var files = new List<string>();
            if (ConfigurationManager.AppSettings["TokenURL"] == "https://authomail.corelogic.com/token")
            {
                files.Add(@"PdfSample\encrypetedFile2pub.pdf");
                files.Add(@"PdfSample\encrypetedFile2pub.pdf");
                files.Add(@"PdfSample\encrypetedFile2pub.pdf");
            }
            else
            {
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
            }
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1", "F2", "F3" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual(mailPiece.Id, response.Id);
            Assert.AreEqual("Your request is queued for processing.", response.Message);
            Assert.AreEqual("Acknowledgement", response.MailingStatus);
        }

        [TestMethod]
        public async Task AM_PdfInsertsValid()
        {
            var files = new List<string>();
            DocumentOrPackage docPackage;
            if (ConfigurationManager.AppSettings["TokenURL"] == "https://authomail.corelogic.com/token")
            {
                files.Add(@"PdfSample\encrypetedFile2pub.pdf");
                docPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" },
                    Inserts = "TEST_BRE,ABC,XYZ"
                };
            }
            else
            {
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
                docPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" },
                    Inserts = "TEST_BRE,ABC,DEF"
                };
            }
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Disclosures",
                DocumentOrPackage = docPackage,
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual(mailPiece.Id, response.Id);
            Assert.AreEqual("Your request is queued for processing.", response.Message);
            Assert.AreEqual("Acknowledgement", response.MailingStatus);
        }

        [TestMethod]
        public async Task AM_InsertsOnlyValid()
        {
            var files = new List<string>();
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Inserts = "TEST_BRE"
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual(mailPiece.Id, response.Id);
            Assert.AreEqual("Your request is queued for processing.", response.Message);
            Assert.AreEqual("Acknowledgement", response.MailingStatus);
        }

        #endregion

        #region InvalidScenarios

        [TestMethod]
        public async Task AM_RequestSizeGT100MBInValid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\100MB.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("RequestTooLarge", response.ErrorCode);
            Assert.AreEqual("Maximum request length exceeded.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ManifestTagMissingInValid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };

            using (HttpClient client = new HttpClient())
            {
                //File.ReadAllBytes("1155299_7698R_OR_3.pdf");                
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //setup client
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = new TimeSpan(0, 5, 0);

                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", ConfigurationManager.AppSettings["Uname"]),
                    new KeyValuePair<string, string>("password", ConfigurationManager.AppSettings["Password"])
                });

                //send request
                HttpResponseMessage responseMessage = await client.PostAsync(ConfigurationManager.AppSettings["TokenURL"], formContent);

                //get access token from response body
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(responseJson);
                var token = jObject.GetValue("access_token").ToString();
                MultipartFormDataContent form = new MultipartFormDataContent();
                for (int i = 0; i < files.Count; i++)
                {
                    form.Add(new StreamContent(new FileStream(files[i], FileMode.Open, FileAccess.Read)), string.Format("F{0}", i + 1), Path.GetFileName(files[i]));
                }
                HttpResponseMessage res = await client.PostAsync(ConfigurationManager.AppSettings["RequestURL"], form);
                string sd = res.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<MailAPIResponse>(sd);
                Assert.AreEqual("Error", response.MailingStatus);
                Assert.AreEqual("Manifest_Missing_Or_Invalid", response.ErrorCode);
                Assert.AreEqual("Manifest is mandatory.", response.ErrorDesc);
            }


        }

        [TestMethod]
        public async Task AM_ManifestMultipleEntriesInValid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };

            using (HttpClient client = new HttpClient())
            {
                //File.ReadAllBytes("1155299_7698R_OR_3.pdf");                
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //setup client
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = new TimeSpan(0, 5, 0);

                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", ConfigurationManager.AppSettings["Uname"]),
                    new KeyValuePair<string, string>("password", ConfigurationManager.AppSettings["Password"])
                });

                //send request
                HttpResponseMessage responseMessage = await client.PostAsync(ConfigurationManager.AppSettings["TokenURL"], formContent);

                //get access token from response body
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(responseJson);
                var token = jObject.GetValue("access_token").ToString();
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new StringContent(JsonConvert.SerializeObject(mailPiece), UnicodeEncoding.UTF8, "application/json"), "Manifest");
                form.Add(new StringContent(JsonConvert.SerializeObject(mailPiece), UnicodeEncoding.UTF8, "application/json"), "Manifest");

                for (int i = 0; i < files.Count; i++)
                {
                    form.Add(new StreamContent(new FileStream(files[i], FileMode.Open, FileAccess.Read)), string.Format("F{0}", i + 1), Path.GetFileName(files[i]));
                }
                HttpResponseMessage res = await client.PostAsync(ConfigurationManager.AppSettings["RequestURL"], form);
                string sd = res.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<MailAPIResponse>(sd);
                Assert.AreEqual("Error", response.MailingStatus);
                Assert.AreEqual("Manifest_Missing_Or_Invalid", response.ErrorCode);
                Assert.AreEqual("Multiple manifest entries.", response.ErrorDesc);
            }


        }

        [TestMethod]
        public async Task AM_FileKeysDuplicatesInValid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };

            using (HttpClient client = new HttpClient())
            {
                //File.ReadAllBytes("1155299_7698R_OR_3.pdf");                
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //setup client
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = new TimeSpan(0, 5, 0);

                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", ConfigurationManager.AppSettings["Uname"]),
                    new KeyValuePair<string, string>("password", ConfigurationManager.AppSettings["Password"])
                });

                //send request
                HttpResponseMessage responseMessage = await client.PostAsync(ConfigurationManager.AppSettings["TokenURL"], formContent);

                //get access token from response body
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(responseJson);
                var token = jObject.GetValue("access_token").ToString();
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new StringContent(JsonConvert.SerializeObject(mailPiece), UnicodeEncoding.UTF8, "application/json"), "Manifest");
                for (int i = 0; i < files.Count; i++)
                {
                    form.Add(new StreamContent(new FileStream(files[i], FileMode.Open, FileAccess.Read)), string.Format("F{0}", i + 1), Path.GetFileName(files[i]));
                }
                form.Add(new StreamContent(new FileStream(files[0], FileMode.Open, FileAccess.Read)), string.Format("F1"), Path.GetFileName(files[0]));
                HttpResponseMessage res = await client.PostAsync(ConfigurationManager.AppSettings["RequestURL"], form);
                string sd = res.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<MailAPIResponse>(sd);
                Assert.AreEqual("Error", response.MailingStatus);
                Assert.AreEqual("Manifest_Missing_Or_Invalid", response.ErrorCode);
                Assert.AreEqual("File key names must be unique.", response.ErrorDesc);
            }


        }

        [TestMethod]
        public async Task AM_CheckForDuplicateMPUniqueIdInValid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = mpId,
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Invalid_DuplicateKey", response.ErrorCode);
            Assert.AreEqual(string.Format("The specified package id '{0}' conflicts with an existing package. Retry with a new GUID.", mpId), response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ClientCodeInValid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "API",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Invalid_ClientCode", response.ErrorCode);
            Assert.AreEqual("Invalid client code specified.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientInfoAddressNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient Address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientInfoStreetNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientInfoCityNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "ANCIBDIOC",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientInfoStateNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientInfoZipNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientInfoUSZipInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203555",
                        Country = "USA"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient zipcode is in invalid format.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientInfoFNameNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient first name is mandatory.", response.ErrorDesc);
        }

        //[TestMethod]
        //public async Task AM_RecipientInfoLNameNullInvalid()
        //{
        //    var files = new List<string>();
        //    files.Add(@"PdfSample\encryptedsample1newkey.pdf");
        //    MailPiece mailPiece = new MailPiece()
        //    {
        //        Id = Guid.NewGuid(),
        //        ClientCode = "AMAPITEST",
        //        BusinessFriendlyIdName = "Loan Number",
        //        BusinessFriendlyIdValue = "123456789",
        //        DocumentOrPackageType = "Initial Disclosures",
        //        DocumentOrPackage = new DocumentOrPackage()
        //        {
        //            Files = new List<string> { "F1" }
        //        },
        //        RecipientInfo = new RecipientInfo()
        //        {
        //            Recipient = new Recipient()
        //            {
        //                FirstName = "Mark",
        //                LastName = ""
        //            },
        //            Address = new Address()
        //            {
        //                Street = "1363 Henery Street",
        //                City = "Wichita",
        //                State = "KS",
        //                Zip = "67203"
        //            }
        //        },
        //        ReturnInfo = new ReturnInfo()
        //        {
        //            Recipient = new Recipient()
        //            {
        //                FirstName = "Mark",
        //                LastName = "Varney"
        //            },
        //            Address = new Address()
        //            {
        //                Street = "1363 Henery Street",
        //                City = "Wichita",
        //                State = "KS",
        //                Zip = "67203"
        //            }
        //        }

        //    };
        //    var response = await GenerateLocalResponse(files, mailPiece);
        //    Assert.AreEqual("Error", response.MailingStatus);
        //    Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
        //    Assert.AreEqual("Recipient first and last name are mandatory.", response.ErrorDesc);
        //}

        [TestMethod]
        public async Task AM_ReturnInfoAddressNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return Address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ReturnInfoStreetNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ReturnInfoCityNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "ANCIBDIOC",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ReturnInfoStateNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ReturnInfoZipNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ReturnInfoUSZipInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203555",
                        Country = "USA"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient zipcode is in invalid format.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ReturnInfoRecepientNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_ReturnInfoFNameNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient first name or organization is mandatory.", response.ErrorDesc);
        }

        //[TestMethod]
        //public async Task AM_ReturnInfoLNameNullInvalid()
        //{
        //    var files = new List<string>();
        //    files.Add(@"PdfSample\encryptedsample1newkey.pdf");
        //    MailPiece mailPiece = new MailPiece()
        //    {
        //        Id = Guid.NewGuid(),
        //        ClientCode = "AMAPITEST",
        //        BusinessFriendlyIdName = "Loan Number",
        //        BusinessFriendlyIdValue = "123456789",
        //        DocumentOrPackageType = "Initial Disclosures",
        //        DocumentOrPackage = new DocumentOrPackage()
        //        {
        //            Files = new List<string> { "F1" }
        //        },
        //        RecipientInfo = new RecipientInfo()
        //        {
        //            Recipient = new Recipient()
        //            {
        //                FirstName = "Mark",
        //                LastName = "Varney"
        //            },
        //            Address = new Address()
        //            {
        //                Street = "1363 Henery Street",
        //                City = "Wichita",
        //                State = "KS",
        //                Zip = "67203"
        //            }
        //        },
        //        ReturnInfo = new ReturnInfo()
        //        {
        //            Recipient = new Recipient()
        //            {
        //                FirstName = "Mark",
        //                LastName = ""
        //            },
        //            Address = new Address()
        //            {
        //                Street = "1363 Henery Street",
        //                City = "Wichita",
        //                State = "KS",
        //                Zip = "67203"
        //            }
        //        }

        //    };
        //    var response = await GenerateLocalResponse(files, mailPiece);
        //    Assert.AreEqual("Error", response.MailingStatus);
        //    Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
        //    Assert.AreEqual("Return recipient first and last name are mandatory.", response.ErrorDesc);
        //}

        [TestMethod]
        public async Task AM_DocumentOrPackageTypeNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Invalid_PackageType", response.ErrorCode);
            Assert.AreEqual("Document or package type is not specified.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientEmailAddressNullIfEDeliveryInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                eDelivery = new eDelivery()
                {
                    ExpiryDate = DateTime.UtcNow
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Email address is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_RecipientEmailAddressFormatInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney",
                        EmailAddress = "esign@testcom"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                eDelivery = new eDelivery()
                {
                    ExpiryDate = DateTime.Now.AddDays(1)
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Email address is not in a valid format.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_eDeliveryExpiryDateInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney",
                        EmailAddress = "esign@test.com"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                eDelivery = new eDelivery()
                {
                    ExpiryDate = DateTime.Now.AddDays(-2)
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Issue_In_EDelivery", response.ErrorCode);
            Assert.AreEqual("The expiration date must be greater than or equal to current date.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_BusinessFriendlyIdNameNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Invalid_BusinessIdentifier", response.ErrorCode);
            Assert.AreEqual("Business friendly Id or value is not specified in package.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_BusinessFriendlyIdValueNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Invalid_BusinessIdentifier", response.ErrorCode);
            Assert.AreEqual("Business friendly Id or value is not specified in package.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_DocumentOrPackageNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_DocumentOrPackage", response.ErrorCode);
            Assert.AreEqual("DocumentOrPackage is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_FilesNullInsertFalseInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosu",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Inserts = "HELOC"
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_Files", response.ErrorCode);
            Assert.AreEqual("Files property must be populated for non-inserts only packages.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_FilesInsertNullInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {

                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_Files", response.ErrorCode);
            Assert.AreEqual("Request cannot proceed further without Files or Inserts value.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_InsertContainsPipelineInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" },
                    Inserts = "ABC|BCC"
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_Files", response.ErrorCode);
            Assert.AreEqual("Inserts  is not in a valid format.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_DulicateInsertValueInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" },
                    Inserts = "ABC,ABC"
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_Files", response.ErrorCode);
            Assert.AreEqual("Multiple occurrences of an insert value are not allowed.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_UnknownInsertOrNoBREInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" },
                    Inserts = "SWAMINARAYAN"
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_Files", response.ErrorCode);
            Assert.AreEqual(string.Format("Following specified inserts are invalid or not BRE: {0}", mailPiece.DocumentOrPackage.Inserts), response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_CustomInfoCountInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                CustomInfo = new List<Metadata>() { }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Manifest_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Metadata is mandatory in custominfo.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_CustomInfoKeyEmptyInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                CustomInfo = new List<Metadata>() { new Metadata { Key = "", DataType = DataType.Float, Value = "10000.50" } }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Manifest_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("CustomInfo's Key is mandatory.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_PhoneNoFormatInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney",
                        Phone = "919-645-42"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Provided phone number is not in a valid format. Please provide it in one of the following formats : (XXX) XXX XXXX, XXXXXXXXXX, XXX-XXX-XXXX, XXX XXX XXXX", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_EmailFormatInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney",
                        EmailAddress = "mwizarddummy.com"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Recipient_Details_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Email address is not in a valid format or blank.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_PdfNotSpecifiedForKeyInvalid()
        {
            var files = new List<string>();
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_PDF_File", response.ErrorCode);
            Assert.AreEqual("Pdf file not found for the following file key(s): F1.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_FileCountGT99Invalid()
        {
            var files = new List<string>();
            for (int i = 1; i <= 100; i++)
            {
                files.Add(@"PdfSample\File4.pdf");
            }
            var fileNames = new List<string>();
            for (int i = 1; i <= 100; i++)
            {
                fileNames.Add(string.Format("F{0}", i));
            }
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = fileNames
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Manifest_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Up to 99 pdf files are supported per mailpiece.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_EncryptFileInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Invalid_PDF_File", response.ErrorCode);
            Assert.AreEqual("Unable to decrypt file : F1.", response.ErrorDesc);
        }

        //[TestMethod]
        //public async Task AM_CorruptPdfInvalid()
        //{
        //    var files = new List<string>();
        //    files.Add(@"PdfSample\Corrupt.pdf");
        //    MailPiece mailPiece = new MailPiece()
        //    {
        //        Id = Guid.NewGuid(),
        //        ClientCode = "AMAPITEST",
        //        BusinessFriendlyIdName = "Loan Number",
        //        BusinessFriendlyIdValue = "123456789",
        //        DocumentOrPackageType = "Initial Disclosures",
        //        DocumentOrPackage = new DocumentOrPackage()
        //        {
        //            Files = new List<string> { "F1" }
        //        },
        //        RecipientInfo = new RecipientInfo()
        //        {
        //            Recipient = new Recipient()
        //            {
        //                FirstName = "Mark",
        //                LastName = "Varney"
        //            },
        //            Address = new Address()
        //            {
        //                Street = "1363 Henery Street",
        //                City = "Wichita",
        //                State = "KS",
        //                Zip = "67203"
        //            }
        //        },
        //        ReturnInfo = new ReturnInfo()
        //        {
        //            Recipient = new Recipient()
        //            {
        //                FirstName = "Mark",
        //                LastName = "Varney"
        //            },
        //            Address = new Address()
        //            {
        //                Street = "1363 Henery Street",
        //                City = "Wichita",
        //                State = "KS",
        //                Zip = "67203"
        //            }
        //        }

        //    };
        //    var response = await GenerateLocalResponse(files, mailPiece);
        //    Assert.AreEqual("Error", response.MailingStatus);
        //    Assert.AreEqual("Invalid_PDF_File", response.ErrorCode);
        //    Assert.AreEqual("'F1' pdf file is either corrupt or has no pages.", response.ErrorDesc);
        //}

        [TestMethod]
        public async Task AM_MisMatchMailPieceIdInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = await GenerateLocalResponse(files, mailPiece);
            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Manifest_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("PackageId provided in header must match with manifest.", response.ErrorDesc);
        }

        [TestMethod]
        public async Task AM_MaxRetriesExceededInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\encryptedfile3.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            for (int i = 1; i <= 6; i++)
            {
                var response = await GenerateLocalResponse(files, mailPiece);
                if (i > 5)
                {
                    Assert.AreEqual("Error", response.MailingStatus);
                    Assert.AreEqual("MaxRetriesExceeded", response.ErrorCode);
                    Assert.AreEqual(string.Format("Maximum retries exceeded for package id '{0}'.", mailPiece.Id), response.ErrorDesc);
                }
            }

        }

        [TestMethod]
        public async Task AM_ClientDisconnectedInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\TaskCancellation.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            try
            {
                await GenerateLocalResponse(files, mailPiece, 5);
            }
            catch (TaskCanceledException)
            {
                var response = GetMailResponse(mailPiece.Id.ToString()).Result;

                Assert.AreEqual("Error", response[0].MailingStatus);
                Assert.AreEqual("NotAcceptable", response[0].ErrorCode);
                Assert.AreEqual("The client disconnected.", response[0].ErrorDesc);

            }
        }

        [TestMethod]
        public async Task AM_NotAcceptableInProcessInvalid()
        {
            var files = new List<string>();
            files.Add(@"PdfSample\TaskCancellation.pdf");
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            try
            {
                var t1 = GenerateLocalResponse(files, mailPiece);
                await Task.Delay(1000);
                var t2 = await GenerateLocalResponse(files, mailPiece);
                Assert.AreEqual("Error", t2.MailingStatus);
                Assert.AreEqual("NotAcceptable", t2.ErrorCode);
                Assert.AreEqual(string.Format("A previous request with the specified package id '{0}' is being processed.", mailPiece.Id), t2.ErrorDesc);
            }
            catch (Exception)
            {

            }
        }

        #endregion

        // CREATED BY NIRAV
        #region New Cases

        [TestMethod]
        public async Task AM_CheckByPageNumber()
        {
            var files = new List<string>();
            if (ConfigurationManager.AppSettings["TokenURL"] == "https://authomail.corelogic.com/token")
            {
                files.Add(@"PdfSample\1.pdf");
            }
            else
            {
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
            }

            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),//mpId,
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Credit Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Test_FName",
                        LastName = "Test_LName"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Test_FName",
                        LastName = "Test_LName"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }
            };

            dynamic expectedValues = new JObject();

            expectedValues.ShippingCarrier = "USPS";
            expectedValues.PrintFormat = "Simplex";
            expectedValues.IsBRE = false;
            expectedValues.PrintLocation = "Las Vegas";

            expectedValues.QCPercent = 0;
            expectedValues.TotalPages = 1;
            expectedValues.PrintTrailName = "GENERIC_PRIEXP";
            //this is not in view
            expectedValues.VendorName = "ADE_NORDIS";
            //this is not in view
            expectedValues.EnvelopeName = "TEST_USPS_PRIEXP";
            //expectedValues.SweepTime = "";
            //this is not in view
            expectedValues.PdfFileSize = 88;

            expectedValues.IsPulledBack = 0;

            SetDefaultExpectedValuesJson(expectedValues, mailPiece, files);

            // CHANGE MANUALLY IF NEEDED

            //expectedValues.ErrCode = "No_Error";
            //expectedValues.IsQCPerformed = false;
            //expectedValues.ShippingStatus = "Shipped";
            //expectedValues.IsPulledBack = 0;

            bool isRegistered = RegisterTestCase(mailPiece.Id.ToString(), expectedValues.ToString(), this.GetMethodName());

            Assert.AreEqual(true, isRegistered);

            if (isRegistered)
            {
                var response = await GenerateLocalResponse(files, mailPiece);
                Assert.AreEqual(mailPiece.Id, response.Id);
                Assert.AreEqual("Your request is queued for processing.", response.Message);
                Assert.AreEqual("Acknowledgement", response.MailingStatus);
            }
        }

        // IsBRE

        [TestMethod]
        public async Task AM_IsBREValid()
        {
            var files = new List<string>();
            if (ConfigurationManager.AppSettings["TokenURL"] == "https://authomail.corelogic.com/token")
            {
                files.Add(@"PdfSample\1.pdf");
            }
            else
            {
                files.Add(@"PdfSample\encryptedsample1newkey.pdf");
            }

            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),//mpId,
                ClientCode = "AMAPITEST",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Stand Alone - Closing Disclosure",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Files = new List<string> { "F1" }
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Test_FName",
                        LastName = "Test_LName"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Test_FName",
                        LastName = "Test_LName"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }
            };

            dynamic expectedValues = new JObject();

            expectedValues.ShippingCarrier = "USPS";
            expectedValues.PrintFormat = "Simplex";
            expectedValues.IsBRE = true;
            expectedValues.PrintLocation = "CL_DALLAS";

            //expectedValues.QCPercent = 0;
            expectedValues.TotalPages = 1;
            expectedValues.PrintTrailName = "GENERIC_DALLAS";
            //this is not in view
            expectedValues.VendorName = "CL_DALLAS";
            //this is not in view
            expectedValues.EnvelopeName = "TEST_TYVEK";
            //expectedValues.SweepTime = "";
            //this is not in view
            expectedValues.PdfFileSize = 88;

            //expectedValues.IsPulledBack = 0;

            SetDefaultExpectedValuesJson(expectedValues, mailPiece, files);

            // CHANGE MANUALLY IF NEEDED

            //expectedValues.ErrCode = "No_Error";
            //expectedValues.IsQCPerformed = false;
            //expectedValues.ShippingStatus = "Shipped";
            //expectedValues.IsPulledBack = 0;

            bool isRegistered = RegisterTestCase(mailPiece.Id.ToString(), expectedValues.ToString(), this.GetMethodName());

            Assert.AreEqual(true, isRegistered);

            if (isRegistered)
            {
                var response = await GenerateLocalResponse(files, mailPiece);
                Assert.AreEqual(mailPiece.Id, response.Id);
                Assert.AreEqual("Your request is queued for processing.", response.Message);
                Assert.AreEqual("Acknowledgement", response.MailingStatus);
            }
        }

        // IsBRE-Inserts


        #endregion



        private static async Task<MailAPIResponse> GenerateLocalResponse(List<string> files, MailPiece mailPiece, int timeoutSec = 300)
        {
            using (HttpClient client = new HttpClient())
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //setup client
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("MailPieceId", mailPiece.Id == Guid.Empty ? Guid.NewGuid().ToString() : mailPiece.Id.ToString());
                //client.Timeout = new TimeSpan(0, 5, 0);
                client.Timeout = TimeSpan.FromSeconds(timeoutSec);
                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", ConfigurationManager.AppSettings["Uname"]),
                    new KeyValuePair<string, string>("password", ConfigurationManager.AppSettings["Password"])
                });

                //send request
                HttpResponseMessage responseMessage = await client.PostAsync(ConfigurationManager.AppSettings["TokenURL"], formContent);

                //get access token from response body
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(responseJson);
                var token = jObject.GetValue("access_token").ToString();
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new StringContent(JsonConvert.SerializeObject(mailPiece), UnicodeEncoding.UTF8, "application/json"), "Manifest");
                for (int i = 0; i < files.Count; i++)
                {
                    form.Add(new StreamContent(new FileStream(files[i], FileMode.Open, FileAccess.Read)), string.Format("F{0}", i + 1), Path.GetFileName(files[i]));
                }
                HttpResponseMessage res = await client.PostAsync(ConfigurationManager.AppSettings["RequestURL"], form);
                string sd = res.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<MailAPIResponse>(sd);

            }
        }

        private static async Task<List<MailAPIResponse>> GetMailResponse(string mailPieceId)
        {
            using (HttpClient client = new HttpClient())
            {

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //setup client
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //setup login data
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", ConfigurationManager.AppSettings["Uname"]),
                    new KeyValuePair<string, string>("password", ConfigurationManager.AppSettings["Password"])
                });

                //send request
                HttpResponseMessage responseMessage = await client.PostAsync(ConfigurationManager.AppSettings["TokenURL"], formContent);

                //get access token from response body
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(responseJson);
                var token = jObject.GetValue("access_token").ToString();

                HttpResponseMessage res = await client.GetAsync(string.Format(ConfigurationManager.AppSettings["ResponseURL"], mailPieceId));
                string sd = res.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<MailAPIResponse>>(sd);

            }
        }

        private void SetDefaultExpectedValuesJson(dynamic expectedValues, MailPiece mailPiece, List<string> files)
        {
            expectedValues.DocTypeName = mailPiece.DocumentOrPackageType;
            expectedValues.RcptAddressCity = mailPiece.RecipientInfo.Address.City;
            expectedValues.RcptAddressState = mailPiece.RecipientInfo.Address.State;
            expectedValues.RcptAddressZIP = mailPiece.RecipientInfo.Address.Zip;
            //todo db view does not configured
            expectedValues.Inserts = mailPiece.DocumentOrPackage.Inserts;

            expectedValues.IsQCPerformed = false;
            expectedValues.IsPulledBack = 0;

            expectedValues.ErrCode = "No_Error";
            expectedValues.ShippingStatus = "Shipped";

            #region PDFCount Check
            //todo this will going to static value for pdf page count
            //int pagecount = 0;
            //if (files.Count > 0)
            //{
            //    foreach (var f in mailPiece.DocumentOrPackage.Files)
            //    {
            //        int index = int.Parse(f.Replace("F", "")) - 1;
            //        if (index < files.Count)
            //        {
            //            var file = files[index];

            //            pagecount += GetPageCount(file);
            //        }
            //    }
            //}

            //expectedValues.TotalPages = pagecount;
            #endregion

            expectedValues.TotalSheets = expectedValues.TotalPages;

            if (expectedValues.PrintFormat == "Duplex")
            {
                int pagecount = int.Parse(expectedValues.TotalPages.ToString());

                expectedValues.TotalSheets = (int)(pagecount / 2) + (int)(pagecount % 2);
            }
        }

        public bool RegisterTestCase(string mailPieceId, string expectedValuesJson, string methodName = "")
        {
            try
            {
                using (var ldb = CreateDbConnection())
                {
                    ldb.Execute("Insert Into APITestResult (MailPieceId, TestCaseName, ExpectedResult, CreatedTime) Values (:MailPieceId, :TestCaseName, :ExpectedResult, :CreatedTime)", new { MailPieceId = mailPieceId, TestCaseName = methodName, ExpectedResult = expectedValuesJson, CreatedTime = DateTime.Now.ToString() });
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private string GetMethodName([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        //public static int GetPageCount(string filePath)
        //{
        //    using (PdfReader reader = new PdfReader(filePath))
        //    {
        //        return reader.NumberOfPages;
        //    }
        //}

        private static DbConnection CreateDbConnection(string connectionStringName = "AMTesting")
        {
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (cs == null)
            {
                throw new ArgumentException("Connection string with name '{0}' does not exist.", connectionStringName);
            }

            var factory = DbProviderFactories.GetFactory(cs.ProviderName);
            var connection = factory.CreateConnection();
            connection.ConnectionString = cs.ConnectionString;
            return connection;
        }
    }
}
