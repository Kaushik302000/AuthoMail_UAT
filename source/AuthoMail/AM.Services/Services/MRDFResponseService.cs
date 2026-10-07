using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;

namespace AM.Services
{
    public interface IMRDFResponseService : IDisposable
    {
        void GenerateMRDFResponse(string outFilePath, string zipFilePath);
    }
    public class MRDFResponseService : ServiceBase, IMRDFResponseService
    {
        public void GenerateMRDFResponse(string outFilePath, string zipFilePath)
        {
            string query = "SELECT PB.PrintFileName,PM.JobId,PM.PieceId,PM.[Date],PM.TotalSheets,PM.PDFFileName FROM PnS.BatchSummary PB INNER JOIN PnS.MRDFRequest PM ON PB.PrintBatch = PM.JobId WHERE PB.PipelineStatus = 'Submitted' AND PB.IsMRDFReceived = 0 AND PB.PrintLocation = 'Fort Mill, SC'";
            var mrdfData = db.Query<MRDFResponse>(query).ToList();

            foreach (var items in mrdfData.GroupBy(m => m.JobId))
            {
                string responseLine = new string(' ', 915);
                var responseLines = new List<string>();
                string outFileFullPath = string.Empty;
                string zipFileFullPath = string.Empty;
                Random r = new Random();
                
                foreach (var item in items)
                {
                    outFileFullPath = string.Format("{0}.out", Path.Combine(outFilePath, item.PrintFileName.Replace(".zip", "-000043HYFC")));
                    zipFileFullPath = string.Format("{0}.zip", Path.Combine(zipFilePath, Path.GetFileNameWithoutExtension(outFileFullPath)));
                    item.Date = DateTime.Parse(item.Date).ToString("yyyyMMdd");
                    item.JobId = String.Format("{0:D10}", Int32.Parse(item.JobId));
                    item.PieceId = String.Format("{0:D4}", Int32.Parse(item.PieceId));
                    item.TotalSheets = String.Format("{0:D3}", Int32.Parse(item.TotalSheets));
                    item.PDFFileName = String.Format("{0,-160}", item.PDFFileName);
                    item.OperatorId = "RichardP";
                    item.MachineId = String.Format("{0,-8}", "PCI09");
                    item.ProcessingTimeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    item.DetailedErrorId = String.Format("{0,-5}", "4691");
                    item.Disposition = String.Format("{0,-2}", "5");
                    item.Common = "000000.00 00000000Mailrun_Files            0.0000000 Mailrun_Files            ";
                    //item.PackageTrackingNumber = String.Format("{0,-40}", "91149014964518079610" + r.Next(10, 99).ToString());
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 77), item.Date, responseLine.Substring(85));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 85), item.JobId, responseLine.Substring(95));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 95), item.PieceId, responseLine.Substring(99));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 99), item.TotalSheets, responseLine.Substring(102));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 117), item.PDFFileName, responseLine.Substring(277));
                    //responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 501), item.PackageTrackingNumber, responseLine.Substring(541));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 800), item.OperatorId, responseLine.Substring(808));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 808), item.MachineId, responseLine.Substring(816));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 816), item.ProcessingTimeStamp, responseLine.Substring(830));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 830), item.DetailedErrorId, responseLine.Substring(835));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 835), item.Disposition, responseLine.Substring(837));
                    responseLine = string.Format("{0}{1}{2}", responseLine.Substring(0, 837), item.Common, responseLine.Substring(915));
                    responseLines.Add(responseLine);
                }
                File.WriteAllLines(outFileFullPath, responseLines.ToArray());
                using (ZipFile mrdfoutZip = new ZipFile())
                {
                    mrdfoutZip.AddFile(outFileFullPath, "");
                    mrdfoutZip.Save(zipFileFullPath);
                }
                if (File.Exists(outFileFullPath))
                {
                    File.Delete(outFileFullPath);
                }
            }
        }
    }
}
