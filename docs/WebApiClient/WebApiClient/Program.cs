using AM.USPSLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Refit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApiClient
{
    class Program
    {
        static string endpoint = ConfigurationManager.AppSettings["url"]; //"https://authomailuat.corelogic.com/";
        static void Main(string[] args)
        {
            if (endpoint.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }

            string uid = ConfigurationManager.AppSettings["uid"];
            string pass = ConfigurationManager.AppSettings["pass"];

            var svc = RestService.For<IUSPS>(new HttpClient(new AuthenticatedHttpClientHandler(uid, pass)) { BaseAddress = new Uri(endpoint) });

            //var result = svc.VerifyAddressAsync(new VerifierAddressRequest() { AddressRequests = new List<AddressRequest> { new AddressRequest { Id = "2", StreetAddress = "URB PALACIOS REALES 318 CALLE BALBI", State = "PR", City = "TOA ALTA", Zip5 = "00953" } } }).Result;

            //eVSLabelRequest eVSReq = new eVSLabelRequest();
            ////General settings
            //eVSReq.AllowNonCleansedDestAddr = true;
            //eVSReq.AllowNonCleansedOriginAddr = true;
            //eVSReq.ServiceType = AM.USPSLib.ServiceType.PRIORITY;
            //eVSReq.Container = Container.FLAT_RATE_ENVELOPE;
            //eVSReq.ShipDate = null;
            //eVSReq.ReceiptOption = ReceiptOption.SEPARATE_PAGE;
            //eVSReq.WeightInOunces = "48";
            //eVSReq.SenderName = "Nation Star";//_ShipDtl.ShipperCompanyName;
            //eVSReq.ImageType = ImageType.PDF;
            //eVSReq.LabelParameters = new LabelParameters()
            //{
            //    Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 },
            //    Size = LabelSize.LABEL4X6L
            //};
            //eVSReq.ReturnCommitments = true;

            //// To Address
            //eVSReq.ToName = "";//(isReturnLable) ? _ShipDtl.ShipperContactPerson : _ShipDtl.RecipientContactPerson;
            //eVSReq.ToFirm = "";//(isReturnLable) ? _ShipDtl.ShipperCompanyName : _ShipDtl.RecipientCompanyName;
            //eVSReq.ToStreetAddress = "";//result.AddressResponses[0].StreetAddress;
            //eVSReq.ToCity = "";//result.AddressResponses[0].City;
            //eVSReq.ToState = "";//result.AddressResponses[0].State;
            //eVSReq.ToZip5 = "";//result.AddressResponses[0].Zip5.ToString().PadLeft(5, '0');
            //eVSReq.ToPhone = "";//(isReturnLable) ? _ShipDtl.ShipperPhoneNumber : _ShipDtl.RecipientPhoneNumber;

            //// From Address
            //eVSReq.FromFirm = "";// (isReturnLable) ? _ShipDtl.RecipientCompanyName : _ShipDtl.ShipperCompanyName;
            //eVSReq.FromName = "";//(isReturnLable) ? _ShipDtl.RecipientContactPerson : _ShipDtl.ShipperContactPerson;
            //eVSReq.FromStreetAddress = "";//(isReturnLable) ? _ShipDtl.RecipientAddress : _ShipDtl.ShipperAddress;
            //eVSReq.FromCity = "";//(isReturnLable) ? _ShipDtl.RecipientCity : _ShipDtl.ShipperCity;
            //eVSReq.FromState = "";//(isReturnLable) ? _ShipDtl.RecipientState : _ShipDtl.ShipperState;
            //eVSReq.FromZip5 = "";//(isReturnLable) ? _ShipDtl.RecipientZip.ToString().PadLeft(5, '0') : _ShipDtl.ShipperZip.ToString().PadLeft(5, '0');
            //eVSReq.FromPhone = "";//(isReturnLable) ? _ShipDtl.RecipientPhoneNumber : _ShipDtl.ShipperPhoneNumber;

            //var result = svc.VerifyAddressAsync(new VerifierAddressRequest()
            //{
            //    AddressRequests = new List<AddressRequest> { 
            //        new AddressRequest { Id = "2", StreetAddress = "20 FLORENCE STREET P.O. BOX 421", City = "MARLBOROUGH", State = "MA", Zip5 = "01757" },
            //        //new AddressRequest { Id = "1", StreetAddress = "PIPERS VALLY FARM SLOUGH LANE SAUNDERTON", City = "BUCKINGHAMSHIRE HP144HN", State = "AP", Zip5 = "28310" },
            //        //new AddressRequest { Id = "3", StreetAddress = "RR 1 BOX 12  ", City = "HERNDON VA 2", State = "07", Zip5 = "-2716" },
            //        //new AddressRequest { Id = "3", StreetAddress = "PO BOX 3200", City = "LANGLEY", State = "ZZ", Zip5 = "98101" },
            //        //  new AddressRequest {  Id = "1", StreetAddress = "23 RES LLORENS TORRES", City = "SAN JUAN", State = "PR", Zip5 =  "00924" },
            //        //new AddressRequest { Id = "4", StreetAddress = "CMR 456 BOX 154", City = "APO", State = "AA"},
            //        //new AddressRequest { Id = "5", StreetAddress = "2000 Tonnelle Ave", City = "North Bergen", State = "NJ"}
            //}
            //}).Result;
            //string resul = result.AddressResponses[0].HasError ? "Not Verified" : "Verified";
            //Console.WriteLine("Address verify result : " + resul);

            //eVSLabelRequest eVSReq = new eVSLabelRequest();
            ////General settings
            //eVSReq.AllowNonCleansedDestAddr = true;
            //eVSReq.AllowNonCleansedOriginAddr = true;
            //eVSReq.ServiceType = AM.USPSLib.ServiceType.PRIORITY;
            //eVSReq.Container = Container.FLAT_RATE_ENVELOPE;
            //eVSReq.ShipDate = null;// DateTime.Today;
            //eVSReq.ReceiptOption = ReceiptOption.SEPARATE_PAGE;
            //eVSReq.WeightInOunces = "48";
            ////eVSReq.SenderName = "USAA";
            //eVSReq.ImageType = ImageType.PDF;
            //eVSReq.LabelParameters = new LabelParameters()
            //{
            //    Size = LabelSize.SEPARATECONTINUEPAGE,
            //    XCoordinate = 450,
            //    YCoordinate = 500,
            //    Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 }
            //};
            //eVSReq.ReturnCommitments = true;
            //eVSReq.POZipCode = "75099";

            //// From Address
            //eVSReq.FromName = "ABC Inc.";
            //eVSReq.FromFirm = "";
            //eVSReq.FromAptOrSuiteorOther = "Ste 113";
            //eVSReq.FromStreetAddress = "3131 Princeton Pike Bldg 4 Ste 113";
            //eVSReq.FromCity = "Lawrenceville";
            //eVSReq.FromState = "NJ";
            //eVSReq.FromZip4 = "";
            //eVSReq.FromZip5 = "08648";
            //eVSReq.FromPhone = "0000000000";// new string('0', 10);

            //// To Address
            //eVSReq.ToFirm = "";
            //eVSReq.ToName = "SAMUEL MATEO-SOTO";
            //eVSReq.ToAptOrSuiteorOther = "";
            //eVSReq.ToStreetAddress = "PO BOX 744";
            //eVSReq.ToCity = "DORADO";
            //eVSReq.ToState = "PR";
            //eVSReq.ToZip4 = "";
            //eVSReq.ToZip5 = "00646";
            //eVSReq.ToPhone = new string('0', 10);
            ////eVSReq.Width = 10;
            ////eVSReq.Length = 13m;
            ////eVSReq.Height = 0.5m;
            //eVSReq.eVSCustomsFormsData = new eVSCustomsFormsData() { ShippingContents = new List<ItemDetail>() { new ItemDetail() { HSTariffNumber = "490110", CountryOfOrigin = "United States", Description = "Loan Document", Quantity = 1, NetPounds = 2, NetOunces = 0, Value = 50 } }, CustomsContentType = CustomsContentType.DOCUMENTS };
            var eVSReq = D5();
            var res = svc.RequesteVSLabelAsync(eVSReq).Result;
            if (!string.IsNullOrWhiteSpace(res.BarcodeNumber))
            {
                Byte[] bytes = Convert.FromBase64String(res.LabelImageString);
                File.WriteAllBytes(@"Label_Priority_Express .pdf", bytes);
                //bytes = Convert.FromBase64String(res.ReceiptImageString);
                //File.WriteAllBytes(@"Receipt.pdf", bytes);
            }
            Console.WriteLine(string.IsNullOrWhiteSpace(res.BarcodeNumber) ? "Label Not Generated" : "Label generated. You can find Label.pdf in same folder as exe");
            //Console.WriteLine(string.IsNullOrWhiteSpace(res.BarcodeNumber) ? "Receipt Not Generated" : "Receipt generated. You can find Label.pdf in same folder as exe");
            Console.WriteLine("press any key to close.");
            Console.Read();
        }

        public static eVSLabelRequest GetDefaultValue(bool military)
        {
            eVSLabelRequest eVSReq = new eVSLabelRequest();
            //General settings
            eVSReq.AllowNonCleansedDestAddr = true;
            eVSReq.AllowNonCleansedOriginAddr = true;
            eVSReq.ServiceType = AM.USPSLib.ServiceType.PRIORITY_EXPRESS;
            eVSReq.Container = Container.FLAT_RATE_ENVELOPE;
            eVSReq.ShipDate = DateTime.Now.Date;
            eVSReq.ReceiptOption = ReceiptOption.SEPARATE_PAGE;
            eVSReq.WeightInOunces = "48";
            eVSReq.SenderName = "USAA";
            eVSReq.ImageType = ImageType.PDF;
            eVSReq.LabelParameters = new LabelParameters()
            {
                Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 },
                Size = LabelSize.SEPARATECONTINUEPAGE
            };
            eVSReq.ReturnCommitments = true;

            // From Address
            eVSReq.FromName = "USAA Federal Savings Bank";
            eVSReq.FromFirm = "USAA Federal Savings Bank";
            eVSReq.FromStreetAddress = "10750 McDermott Freeway";
            eVSReq.FromCity = "San Antonio";
            eVSReq.FromState = "TX";
            eVSReq.FromZip4 = "";
            eVSReq.FromZip5 = "78288";
            eVSReq.FromPhone = new string('0', 10);
            eVSReq.POZipCode = "75099";

            if (military)
            {
                eVSReq.eVSCustomsFormsData = new eVSCustomsFormsData()
                {
                    ShippingContents = new List<ItemDetail>() 
                    { 
                        new ItemDetail() { 
                            Description = "Loan Document", 
                            Quantity = 1, 
                            Value = 10,
                            NetPounds = 3, 
                            NetOunces = 0, 
                            CountryOfOrigin = "United States"
                        }
                    },
                    CustomsContentType = CustomsContentType.DOCUMENTS
                };
            }

            return eVSReq;
        }

        public static eVSLabelRequest D1()
        {
            eVSLabelRequest eVSReq = GetDefaultValue(true);
            eVSReq.ServiceType = ServiceType.PRIORITY;
            eVSReq.ToFirm = "";
            eVSReq.ToName = "LORI BAY";
            eVSReq.ToStreetAddress = "C/O RENEE REED  BOX 341";
            eVSReq.ToCity = "DPO";
            eVSReq.ToState = "AE";
            eVSReq.ToZip5 = "09831";
            eVSReq.CustomerRefNo = "D1";
            eVSReq.ToZip4 = "";
            eVSReq.ToPhone = new string('0', 10);

            return eVSReq;
        }
        public static eVSLabelRequest D2()
        {
            eVSLabelRequest eVSReq = GetDefaultValue(true);

            eVSReq.ToFirm = "";
            eVSReq.ToName = "N1";
            eVSReq.ToStreetAddress = "CMR 479 BOX 644";
            eVSReq.ToCity = "APO";
            eVSReq.ToState = "AE";
            eVSReq.ToZip5 = "09263";
            eVSReq.ToZip4 = "0007";
            eVSReq.CustomerRefNo = "N1";
            eVSReq.ToZip4 = "";
            eVSReq.ToPhone = new string('0', 10);

            return eVSReq;
        }
        public static eVSLabelRequest D3()
        {
            eVSLabelRequest eVSReq = GetDefaultValue(true);

            eVSReq.ToFirm = "";
            eVSReq.ToName = "N2";
            eVSReq.ToStreetAddress = "PSC 37 BOX 4298";
            eVSReq.ToCity = "APO";
            eVSReq.ToState = "AE";
            eVSReq.ToZip5 = "09459";
            eVSReq.ToZip4 = "0043";
            eVSReq.CustomerRefNo = "N2";
            eVSReq.ToZip4 = "";
            eVSReq.ToPhone = new string('0', 10);

            return eVSReq;
        }
        public static eVSLabelRequest D4()
        {
            eVSLabelRequest eVSReq = GetDefaultValue(true);
            eVSReq.ServiceType = ServiceType.PRIORITY;
            eVSReq.ToFirm = "";
            eVSReq.ToName = "William Cotten";
            eVSReq.ToStreetAddress = "PO BOX 3738";
            eVSReq.ToCity = "HAGATNA";
            eVSReq.ToState = "GU";
            eVSReq.ToZip5 = "96932";
            eVSReq.CustomerRefNo = "D4";
            eVSReq.ToZip4 = "";
            eVSReq.ToPhone = new string('0', 10);

            return eVSReq;
        }
        public static eVSLabelRequest D5()
        {
            eVSLabelRequest eVSReq = GetDefaultValue(false);
            //eVSReq.ServiceType = ServiceType.PRIORITY;
            //eVSReq.ToFirm = "";
            //eVSReq.ToName = "Milan Patel";
            //eVSReq.ToStreetAddress = "3 Akshardham Society Kisan Samosa lane";
            //eVSReq.ToCity = "Nadiad";
            //eVSReq.ToState = "GJ";
            //eVSReq.ToZip5 = "387001";
            //eVSReq.CustomerRefNo = "D5";
            //eVSReq.ToZip4 = "";
            //eVSReq.ToPhone = new string('0', 10);
            string json = File.ReadAllText("evsRequest.Json").Replace("{{CurrentDate}}", DateTime.Now.ToShortDateString());
            eVSReq = JsonConvert.DeserializeObject<eVSLabelRequest>(json);

            return eVSReq;
        }
        public static eVSLabelRequest D6()
        {
            eVSLabelRequest eVSReq = GetDefaultValue(true);
            //eVSReq.ServiceType = ServiceType.PRIORITY;
            eVSReq.POBox = true;
            eVSReq.ToFirm = "";
            eVSReq.ToName = "PASTORA RODRIGUEZ";
            eVSReq.ToStreetAddress = "PO BOX 10227";
            eVSReq.ToCity = "NAPLES";
            eVSReq.ToState = "FL";
            eVSReq.ToZip5 = "34101";
            eVSReq.CustomerRefNo = "D6";
            eVSReq.ToZip4 = "";
            eVSReq.ToPhone = new string('0', 10);

            return eVSReq;
        }
    }
}
