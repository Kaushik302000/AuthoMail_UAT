using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using AM.Services;

namespace AM.Reporting
{
    class USAADailyMTDVolumeReporting
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void GenerateMTDReport()
        {
            var currentDateTime = Common.Now.UtcToEst();
            log.Debug($"USAA MTD Report Execution Triggered at {currentDateTime} Est");
            DateTime startDate = new DateTime(Common.EstToday.Year, Common.EstToday.Month, 1);
            DateTime EndDate = Common.EstToday;
            startDate = startDate.EstToUtc();
            EndDate = EndDate.EstToUtc();
            try
            {
                using (IReportService reportSvc = new ReportService())
                {
                    var output = reportSvc.USAAMTDVolumeReport(startDate, EndDate);

                    string csvFilePath = Path.Combine(AppConfig.USAAMTDReportPath, $"{Config.Common_EnvironmentInitial}FAGOS.ZYONT.N0010351.USAAPNSOUTPUT.D{currentDateTime.ToString("yyyyMMdd")}.T{currentDateTime.ToString("HHmmss")}.CSV");

                    log.Debug("USAA MTD Report File Write Process Started.");
                    File.WriteAllBytes(csvFilePath, output.ToArray());
                    log.Debug("USAA MTD Report File Write Process Completed.");

                    //using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                    //{
                    //    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    //    {
                    //        ZipArchiveEntry xmlEntry = archive.CreateEntry($"{Config.Common_EnvironmentInitial}FAGOS.ZYONT.N0010351.USAAPNSOUTPUT.D{currentDateTime.ToString("yyyyMMdd")}.T{currentDateTime.ToString("HHmmss")}.csv");
                    //        using (BinaryWriter writer = new BinaryWriter(xmlEntry.Open()))
                    //        {
                    //            writer.Write(output.ToArray());
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating USAA MTD Report", ex);
            }
            log.Debug($"USAA MTD Report Execution Completed at {currentDateTime} Est");
        }
    }
}
