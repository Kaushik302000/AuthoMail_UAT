<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Data</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Xml</Namespace>
</Query>

public static Dictionary<string, int> dict = new Dictionary<string, int>();
static DateTime startDate = DateTime.Now.AddDays(-30);
            static DateTime endDate = DateTime.Now;
        static void Main(string[] args)
        {
            string dirPath = @"D:\Production\AuthoMail\AMRoot";
			//string dirPath = @"\\mstc-neelkanth\D\AuthoMail\AM";
            

            ProcessDirectory(dirPath);
            CheckMailBatches();
        }
        public static void ProcessDirectory(string targetDirectory)
{

// Process the list of files found in the directory.
DirectoryInfo directoryInfo = new DirectoryInfo(targetDirectory);
var fileEntries = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories).Where(f => f.LastWriteTime >= startDate && f.LastWriteTime <= endDate && !f.Name.Contains("-res")).Select(f => f.FullName).ToList();
foreach (string fileName in fileEntries)
{
var r = ProcessFile(Path.GetFileNameWithoutExtension(fileName), fileName);
if (r.Key != "0")
{
if (dict.ContainsKey(r.Key))
{
dict[r.Key] = dict[r.Key] + r.Value;
}
else
{
dict.Add(r.Key, r.Value);
}
}
}
}

        // Insert logic for processing found files here.
        public static KeyValuePair<string, int> ProcessFile(string code, string path)
{
var doc = new XmlDocument();
doc.LoadXml(File.ReadAllText(path));
//var nsManager = new XmlNamespaceManager(doc.NameTable);
//nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
//Console.WriteLine(doc.DocumentElement.Name);
//var mailPieceNodes = doc.DocumentElement.ChildNodes;
//return new KeyValuePair<string, int>(code, mailPieceNodes.Count);
if (doc.DocumentElement.Name == "PackageRequests")
{
var mailPieceNodes = doc.DocumentElement.ChildNodes;
return new KeyValuePair<string, int>(code, mailPieceNodes.Count);
}
else if (doc.DocumentElement.Name == "BcgRespa")
{
var mailPieceNodes = doc.DocumentElement.ChildNodes;
return new KeyValuePair<string, int>(code, mailPieceNodes.Count);
}
else if (doc.DocumentElement.Name == "MailBatch")
{
var mailPieceNodes = doc.DocumentElement.FirstChild.ChildNodes;
return new KeyValuePair<string, int>(code, mailPieceNodes.Count);
}
else
{
return new KeyValuePair<string, int>("0", 0);
}
}


        public static void CheckMailBatches()
        {
            //SqlConnection con = new SqlConnection(@"Data Source=MSTC-OB\apos;Initial Catalog=AuthoMail_UAT;Persist Security Info=True;User ID=sa;Password=Anant@ob");
			SqlConnection con = new SqlConnection(@"Data Source=10.212.40.106;Initial Catalog=AuthoMail;Connection Timeout=120;user id=AuthomailUser;password=Authomail@User;pooling=true;");
			string sql = string.Format("Select b.ClientBatchId, count(m.MailBatchId) as MailPieceCount from [AM].[MailPieces] m,AM.MailBatches b where m.MailBatchId=b.Id and m.MailBatchId In(Select Id from [AM].[MailBatches] where Id>{0}) group by b.ClientBatchId", 30000);
            SqlCommand cmd = new SqlCommand(sql, con);
            DataTable dataTable = new DataTable();
            con.Open();

            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dataTable);
            con.Close();
            da.Dispose();
            Dictionary<string, int> db = new Dictionary<string, int>();
            foreach (DataRow row in dataTable.Rows)
            {
                string clientBatch = row["ClientBatchId"].ToString();
                int count = Convert.ToInt32(row["MailPieceCount"].ToString());
                db.Add(clientBatch, count);
                //Console.WriteLine(string.Format("{0}: {1}", clientBatch, count));
            }
            var xml = dict;
            var resultXmlToDB = xml.Where(entry => !db.ContainsKey(entry.Key) || db[entry.Key] != entry.Value)
            .ToDictionary(entry => entry.Key, entry => entry.Value);
            Console.WriteLine("XML To DB");
            Display(resultXmlToDB);

            var resultDBToXml = db.Where(entry => !xml.ContainsKey(entry.Key) || xml[entry.Key] != entry.Value)
            .ToDictionary(entry => entry.Key, entry => entry.Value);
            Console.WriteLine("DB To XML");
            Display(resultDBToXml);

        }
        private static void Display(Dictionary<string, int> data)
        {
			 Console.WriteLine("");
            foreach (var item in data)
            {
                Console.WriteLine(string.Format("Batch Name:{0}\tCount:{1}", item.Key, item.Value));
            }
			Console.WriteLine("");
        }

// Define other methods and classes here