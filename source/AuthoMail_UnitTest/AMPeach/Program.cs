using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMPeach
{
    class Program
    {
        private static DbConnection db;
        private static DbConnection ldb;
        private static Dictionary<string, int> reportMailPeices;
        static void Main(string[] args)
        {
            db = CreateDbConnection("AuthoMail");
            ldb = CreateDbConnection();

            //we expect that all files are same extension and content in folder
            //reportMailPeices = Directory.GetFiles(ConfigurationManager.AppSettings["DeliveryReportPath"], "*.json")
            //                         .Select(f => JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(f)))
            //                         .Select(a => a.Id.ToString())
            //                         .GroupBy(u => u)
            //                         .ToDictionary(x => (string)x.Key, x => x.Count());

            IEnumerable<string> mailPieceIds = new List<string>();

            if (args.Length > 0)
            {

                //var a = ldb.Query<TestOutcome>("Select MailPieceId, TestCaseName, case when (MailPieceId == null) than 'Not found' else null Message From APITestResult Where CreatedTime <= @CreatedTime and MailPieceId in :MailPieceIds", new { CreatedTime = DateTime.Now.AddHours(-int.Parse(ConfigurationManager.AppSettings["ProcessDelay"])), MailPieceIds = args.ToList() });
                mailPieceIds = args.ToList();
            }
            else
            {
                mailPieceIds = ldb.Query<string>("Select MailPieceId From APITestResult Where TestResult is null");
            }

            reportMailPeices = mailPieceIds.ToDictionary(s => s, s => 0);

            var outComes = ValidateMailPiece(mailPieceIds);

            //todo search here for report check
            var outComesReportGrp = outComes.GroupBy(g => g.ClientCode);

            foreach (var ccode in outComesReportGrp)
            {
                string clientPath = Path.Combine(ConfigurationManager.AppSettings["DeliveryReportPath"], ccode.Key, "Reports");
                var files = Directory.GetFiles(clientPath, "*.*");

                var mailPiecees = ccode.Select(t => t);

                foreach (var f in files)
                {
                    var content = File.ReadAllText(f);

                    //mailPieceIds.Where(s => content.Contains(s)).Select(s => new { Id = s, count = s.Count() });

                    foreach (var m in mailPiecees)
                    {
                        if (content.Contains(m.MailPieceId))
                        {
                            //reportMailPeices[m] = reportMailPeices[m]++;
                            m.DeliveryReport = m.DeliveryReport + 1;
                        }
                    }
                }
            }

            int passcount = 0;
            foreach (var outCome in outComes)
            {
                try
                {
                    bool isPassed = ProcessMailPiece(outCome);

                    if (isPassed)
                    {
                        passcount = passcount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            string countMsg = $"Total MailPiece : {mailPieceIds.Count()}\t Total Pass: {passcount}\t Total Fail: {mailPieceIds.Count() - passcount}";

            Console.WriteLine($"{new string('-', 70)}{Environment.NewLine}{countMsg}{Environment.NewLine}{new string('-', 70)}{Environment.NewLine}Program execution completed, Press any key to close this window. ......");

            //Console.WriteLine($"{Environment.NewLine}Program execution completed, Press any key to close this window. ......");
            Console.ReadKey();
        }

        private static bool ProcessMailPiece(TestOutcome t)
        {
            if (t == null)
            {
                Console.WriteLine($"{Environment.NewLine}The given package is not valid.");
                return false;
            }

            string mailPieceId = t.MailPieceId;

            if (string.IsNullOrWhiteSpace(mailPieceId))
            {
                Console.WriteLine($"{Environment.NewLine}The given package hasn't mail piece id.");
                return false;
            }

            // check whether the mailpiece id exists in sqlite db
            // if not, print message on console: The package with {mailPieceId} is not yet registered. return
            var mailPiece = ldb.QuerySingleOrDefault<APITestResult>("Select * From APITestResult Where MailPieceId = :MailPieceId", new { MailPieceId = mailPieceId });

            if (mailPiece == null)
            {
                Console.WriteLine($"{Environment.NewLine}The package with {mailPieceId} is not yet registered.");
                return false;
            }

            //t.TestCaseName = mailPiece.TestCaseName;

            double minDiff = (DateTime.Now - DateTime.Parse(mailPiece.CreatedTime)).TotalMinutes;
            if (minDiff < double.Parse(ConfigurationManager.AppSettings["ProcessDelayMinutes"]))
            {
                string msg = $"The package with {mailPieceId} is registered in system before {(int)minDiff} minutes, so wait for {(int)(double.Parse(ConfigurationManager.AppSettings["ProcessDelayMinutes"]) - (int)minDiff)} minutes to process.";
                t.Message += msg;
                Console.WriteLine(JsonConvert.SerializeObject(t));
                return false;
            }

            #region View query
            //            string sqlActualValues = $@"SELECT m.Id AS PkgId
            //,MailBatchId
            //,m.UniqueId AS MailPieceId
            //,d.DocTypeName
            //,PCD.ShippingCarrier
            //,PCD.PrintFormat
            //,d.IsBRE IsBRE
            //,m.PrintLocation PrintLocation
            //,d.QCPercent QCPercent
            //,m.TotalPages TotalPages
            //,pt.TrailName PrintTrailName
            //,si.PrintVendorName VendorName
            //,e.EnvelopeName EnvelopeName
            //,m.PDFFileSize PdfFileSize
            //,m.ShippingStatus
            //,m.IsPulledBack
            //,m.IsQCPerformed
            //,m.ErrorCode AS ErrCode
            //,m.RcptAddressCity
            //,m.RcptAddressState
            //,m.RcptAddressZIP
            //,m.TotalSheets TotalSheets
            //FROM AM.MailPieces m
            //LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
            //LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = PCD.Id
            //LEFT OUTER JOIN Common.PrintTrails pt ON m.PrintTrailId = pt.Id
            //LEFT OUTER JOIN Common.ShipperInformation si on pt.ShipperId = si.Id 
            //LEFT OUTER JOIN Common.Envelopes as e on PCD.EnvelopeId = e.Id Where m.UniqueId = @MailPieceId FOR JSON AUTO;";

            //"Select * from [dbo].[vDVExp] Where PkgUniqueId = @MailPieceId FOR JSON AUTO"

            #endregion
            string sqlActualValues = "Select * from [dbo].[AutomateTesting] Where MailPieceId = @MailPieceId FOR JSON AUTO";

            string actualJson = db.QuerySingleOrDefault<string>(sqlActualValues, new { MailPieceId = mailPieceId });

            if (string.IsNullOrWhiteSpace(actualJson))
            {
                //log into lite db as fail
                t.Message += "Record not found in SQL database.";

                //ldb.Execute("Update APITestResult Set TestOutcome = :TestOutcome, TestResult = :TestResult, ProcessedTime = :ProcessedTime Where MailPieceId = :MailPieceId", new { MailPieceId = mailPieceId, TestOutcome = JsonConvert.SerializeObject(t), TestResult = "F", ProcessedTime = DateTime.Now });
                Console.WriteLine($"{Environment.NewLine}{JsonConvert.SerializeObject(t)}");
                return false;
            }
            //to do update db and console from here

            string testResult = EqualValues(t, mailPiece.ExpectedResult, actualJson);

            //t.DelivReport = reportMailPeices.Count(m => m.Key.ToLower() == t.MailPieceId.ToLower() && m.Value == 1);

            if (t.TestResult.Count > 0)
            {
                t.Message += $"Failed due to '{t.TestResult.Count}' Expected and Actual values not matched.";
            }

            if (t.DeliveryReport == 0)
            {
                t.Message += $"Failed due to delivery report not found in given folder.";
            }
            //todo enable after code logic for below
            //else if (!t.VendorZip)
            //{
            //    t.Message += $"File not found in given location.";
            //}

            string result = JsonConvert.SerializeObject(t, Formatting.Indented);

            Console.WriteLine($"{Environment.NewLine}{result}");

            ldb.Execute("Update APITestResult Set TestOutcome = :TestOutcome, TestResult = :TestResult, ProcessedTime = :ProcessedTime Where MailPieceId = :MailPieceId", new
            {
                MailPieceId = mailPieceId,
                TestOutcome = result,
                TestResult = testResult,
                ProcessedTime = DateTime.Now
            });

            return result == "P";
        }

        private static List<TestOutcome> ValidateMailPiece(IEnumerable<string> mailPieceIds)
        {
            List<TestOutcome> tocs;

            tocs = mailPieceIds.Select(mid => new TestOutcome() { MailPieceId = mid }).ToList();

            var registered = ldb.Query<APITestResult>("Select MailPieceId, TestCaseName, ClientCode From APITestResult Where MailPieceId in :MailPieceIds", new { MailPieceIds = mailPieceIds }).ToList();

            foreach (var t in tocs)
            {
                var mailPiece = registered.SingleOrDefault(s => s.MailPieceId == t.MailPieceId);

                //TestOutcome toc = new TestOutcome() { MailPieceId = mailPieceId };

                if (mailPiece == null)
                {
                    t.Message = $"{Environment.NewLine}The package with {t.MailPieceId} is not yet registered.";

                    continue;
                }

                t.TestCaseName = mailPiece.TestCaseName;
                t.ClientCode = mailPiece.ClientCode;
            }

            return tocs;
        }

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

        private static string EqualValues(TestOutcome t, string expectedJson, string actualJson)
        {
            JObject expectedJsonObj = JObject.Parse(expectedJson);
            JObject actualJsonObj = JObject.Parse(actualJson.TrimStart('[').TrimEnd(']'));

            foreach (var prop in expectedJsonObj)
            {
                TestResult diff = new TestResult();

                var propValue = actualJsonObj[prop.Key];

                if (propValue == null)
                {
                    diff.Key = prop.Key;
                    diff.ExValue = prop.Value.ToString();
                    diff.Actvalue = (string)null;

                    t.TestResult.Add(diff);
                }
                else
                {
                    if (!propValue.ToString().Trim().ToUpper().Equals(prop.Value.ToString().Trim().ToUpper()))
                    {
                        diff.Key = prop.Key;
                        diff.ExValue = prop.Value.ToString();
                        diff.Actvalue = propValue.ToString();

                        t.TestResult.Add(diff);
                    }
                }
            }

            return (t.TestResult.Count == 0) ? "P" : "F";
        }
    }

    public class APITestResult
    {
        public string MailPieceId { get; set; }
        public string ClientCode { get; set; }
        public string TestCaseName { get; set; }
        public string ExpectedResult { get; set; }
        public string CreatedTime { get; set; }
        public string TestOutcome { get; set; }
        public string TestResult { get; set; }
        public string ProcessedTime { get; set; }
    }

    public class TestOutcome
    {
        public string MailPieceId { get; set; }
        public string ClientCode { get; set; }
        public string TestCaseName { get; set; }
        public string Message { get; set; }
        public int DeliveryReport { get; set; }
        public bool VendorZip { get; set; }

        public List<TestResult> TestResult { get; set; }

        public TestOutcome()
        {
            this.TestResult = new List<TestResult>();
        }
    }

    public class TestResult
    {
        public string Key { get; set; }
        public string ExValue { get; set; }
        public string Actvalue { get; set; }
    }

    public static class Extension
    {
        public static bool PublicInstancePropertiesEqual<T>(T self, T to, params string[] ignore) where T : class
        {
            if (self != null && to != null)
            {
                Type type = typeof(T);
                List<string> ignoreList = new List<string>(ignore);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (!ignoreList.Contains(pi.Name))
                    {
                        object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                        object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                        if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return self == to;
        }
    }
}
