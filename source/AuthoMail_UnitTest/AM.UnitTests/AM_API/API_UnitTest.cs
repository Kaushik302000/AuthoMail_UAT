using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Net;
using Refit;
using System.Net.Http;
using AM.UnitTests.AM_API;
using System.IO;
using AM.Entities;
using Newtonsoft.Json;

namespace AM.UnitTests
{
    /// <summary>
    /// Summary description for API_UnitTest
    /// </summary>
    [TestClass]
    public class API_UnitTest
    {
        public API_UnitTest()
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
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
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

        [TestMethod]
        public void MRAPI_Success()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "ELLIEMAE",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
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
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual(mailPiece.Id, response.Id);
            Assert.AreEqual("Your request is queued for processing.", response.Message);            
        }
        [TestMethod]
        public void MRAPI_InvalidClientCode()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "US",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Appraisal Report Waiver",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
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
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("InvalidClientCode", response.ErrorCode);
        }
        [TestMethod]
        public void MRAPI_MissingDocument()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "USAA",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Appraisal Report Waiver",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    //Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    //IncludePatternBaseString = true
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
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_PDF_File", response.ErrorCode);
            Assert.AreEqual("Pdf file  is mandatory.", response.ErrorDesc);
        }
        [TestMethod]
        public void MRAPI_MissingPdfName()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "ELLIEMAE",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
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
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_PDF_File", response.ErrorCode);
            Assert.AreEqual("Pdf name  is mandatory.", response.ErrorDesc);
        }
        [TestMethod]
        public void MRAPI_MissingDocumentPacakgeType()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "ELLIEMAE",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
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
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Invalid_PackageType", response.ErrorCode);
            Assert.AreEqual("Document or package type is not specified.", response.ErrorDesc);
        }
        [TestMethod]
        public void MRAPI_MissingRecAddressInfo()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "USAA",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Appraisal Report Waiver",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
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
                        Zip = ""
                    }
                },
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient address is mandatory.", response.ErrorDesc);
        }
        [TestMethod]
        public void MRAPI_MissingRetRecAddressInfo()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "USAA",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Appraisal Report Waiver",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
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
                        Street = "",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient address is mandatory.", response.ErrorDesc);
        }
        
        [TestMethod]
        public void MRAPI_InvalidRequest()
        {
            MailPiece mailPiece = new MailPiece()
            {                
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void MailResponseAPI_ValidRequest()
        {
            string[] ids = { "ECC7429B-C69D-4ADE-B418-B43E1F53C6BF", "DE03C595-5C6F-4DB0-B484-F22C9AD7C478", "7EAC64B7-2831-4FC2-AEC7-623B3B1BB7FD", "4CE84B05-EBCA-449E-ABD0-3731AD08861C", "deb7ad5f-71d9-46bd-a92f-f3f3366223c1","23421697-A519-4A8B-8537-797EBC598716" };
            string[] ids1 = { "3FBBD5E7-62D7-4539-918B-A5DB4A5D9D95","63B92395-8E66-4F7E-BA40-8BAE8962911D","3CE32D02-6234-4AD7-91BF-C634C611A6C4", "5102573B-E5BD-4A57-950B-114064A62BCB", "ECC7429B-C69D-4ADE-B418-B43E1F53C6BF","DF03C595-5C6F-4DB0-B484-F22C9AD7C478","9E03C595-5C6F-4DB0-B484-F22C9AD7C478","87A1994F-01D4-4FFE-8994-3987B65EE3E1","19004F90-AC66-437B-A312-033B4F634163","D49EB72E-FCC6-45EF-B8D4-681B2ABAC4C4","DC28FAE6-54F9-4459-98EB-E19206A495AC","AA91AA85-BE64-4E0D-A7A6-CA1E5CC8B0B4","BD91AA85-BE64-4E0D-A7A6-CA1E5CC8B0B4","EE53EEC3-E575-4993-8348-5F2080F5F1B3","5FCBE6A3-94F3-40D8-9644-E6BA33992DA3","EF43EEC3-E575-4993-8348-5F2080F5F1B3","BABE2444-DE29-483C-9AB5-8D5EC1E3A252","EE43EEC3-E575-4993-8348-5F2080F5F1B3","47533386-8D70-474F-BB80-1D1112A99292","0BD2076F-70D7-43B3-994A-D976CA5D2C56","4310165F-C82C-4372-B44E-0953E0D64EAE","06765BEC-72E1-4A7F-AFF5-354E3DFF3014","33E2F722-5186-4803-920A-C7418C43BDA0","99D6D537-B51E-48C8-BA30-694B6EEEF71C","A13B6F39-D372-4C31-BBBC-AC27E0ED031E","D867AB0A-38F9-4B12-A642-85E72B5CC7DF","DD43EEC3-E575-4993-8348-5F2080F5F1B3","D0FA0B8A-7ED4-4387-915D-1F1E60A086D6","A8F04234-43A6-4256-89AF-DEC8F2DACD70","37828A43-CB06-43A7-8066-161052F68F21","701954AE-843F-4A15-AF02-F9BC58AEBD77","E6DB2EFB-DA32-4363-997A-4758B36EA5BF","FF43EEC3-E575-4993-8158-5F2080F5F1B3","74A4FF48-C5EC-4F15-A789-298B73529F09","DD43EEC3-E575-4993-8158-5F2080F5F1B3","8CEFC076-1297-4616-A1CB-406EF7030F1E","ED43EEC3-E575-4993-8158-5F2080F5F1B3","6617A11C-E7FF-4966-B0D7-099D8F34EAD6","74712012-1719-4D22-8376-D4A045AC6A42","FFEE545E-E1C8-434E-A8D2-482E6FEC4A11","3747705D-31C4-4AFD-B7B2-46E43E7C64D3","613E5975-DCD8-43E4-96D2-BDAB0300AF9A","34F0643E-EC87-4AF6-800E-15F48D1D58A6","2B09CD2E-262D-4CC6-89ED-EBD1A630BF24","B8BF5D78-7FD6-4157-BE80-E3A274188F57","F7137D79-E47E-4F7A-A6F3-323E5452F4A5","E5B10980-A6E1-44E5-A7A4-AB4CBDFD0796","2BE905B4-8833-4EEA-88F5-44C100EAEFD8","B93165CF-A0E3-44AA-9520-D3D68760BDD8","4CE84B05-EBCA-449E-ABD0-3731AD08861C" };


            List<string> mailPieceIds = new List<string>(ids1);
            List<MailAPIResponse> response = GenerateMailResponse(mailPieceIds).Result;
            var serializedString = JsonConvert.SerializeObject(response);
            Assert.AreEqual("Error", response[0].MailingStatus);            
            Assert.AreEqual("Shipped", response[1].MailingStatus);
            Assert.AreEqual("PulledBack", response[2].MailingStatus);
            Assert.AreEqual("eDelivered", response[3].MailingStatus);
            Assert.AreEqual("NotFound", response[4].MailingStatus);
            Assert.AreEqual("The specified package does not exist.", response[4].Message);
            Assert.AreEqual("InProgress", response[5].MailingStatus);
        }

        [TestMethod]
        public void MailResponseAPI_InValidRequest()
        {            

            List<string> mailPieceIds = new List<string>();
            for (int i = 0; i <= 52; i++)
            {
                mailPieceIds.Add(Guid.NewGuid().ToString());
            }

            List<MailAPIResponse> response = GenerateMailResponse(mailPieceIds).Result;
            var serializedString = JsonConvert.SerializeObject(response);     
            
            
        }

        [TestMethod]
        public void MailResponseAPI_InValidInput()
        {
            string[] ids = { "ABCD", "ebfjkbjdsbcv", "jbfibgrenbfbnf", "bsjvbjfdb vfjkv j", "fnbvjfkvnvbklfnb kcvnvb", "656565565555884" };
            List<string> mailPieceIds = new List<string>(ids);            

            List<MailAPIResponse> response = GenerateMailResponse(mailPieceIds).Result;
            var serializedString = JsonConvert.SerializeObject(response);


        }

        [TestMethod]
        public Task<MailAPIResponse> CreateMailRequest(MailPiece request)
        {
            string endpoint = "http://localhost:17005";//"https://authomailuat.corelogic.com";
            //
            // TODO: Add test logic here
            //
            if (endpoint.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }

            string uid = "admin@milestonetechnoconsulting.com";
            string pass = "Admin@123";

            var svc = RestService.For<IMailRequestService>(new HttpClient(new AuthenticatedHttpClientHandler(uid, pass)) { BaseAddress = new Uri(endpoint) });
            return svc.CreateMailRequest(request);
        }

        [TestMethod]
        public Task<List<MailAPIResponse>> GenerateMailResponse(List<string> mailPieceIds)
        {
            string endpoint = "http://localhost:17005"; //"https://authomailuat.corelogic.com";//;
            //
            // TODO: Add test logic here
            //
            if (endpoint.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }

            string uid = "admin@milestonetechnoconsulting.com";//"emuat"
            string pass = "Admin@123";//"F2c35e1e2D56@52BBd8078n";

            var svc = RestService.For<IMailRequestService>(new HttpClient(new AuthenticatedHttpClientHandler(uid, pass)) { BaseAddress = new Uri(endpoint) });
            var response = svc.MailAPIResponse(mailPieceIds);
            return response;
        }
    }
}
