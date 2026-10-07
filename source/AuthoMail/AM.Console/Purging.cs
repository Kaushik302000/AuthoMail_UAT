using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    public class Purging
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static DirectoryInfo sourceAMRootDir = new DirectoryInfo(Config.Common_AuthoMailRootFolder);
        private static DirectoryInfo destAMDir = new DirectoryInfo(Config.Common_AMTrash);
        private static DirectoryInfo sourcePnSRootDir = new DirectoryInfo(Config.PnS_PCIRootFolder);
        private static DirectoryInfo destPnSDir = new DirectoryInfo(Config.Common_PnSTrash);

        //Testing Paths
        //private static DirectoryInfo sourceAMRootDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\AuthoMail_KitTest\AMRoot");
        //private static DirectoryInfo destAMDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\AuthoMail_KitTest\Delete\AMTrash");
        //private static DirectoryInfo sourcePnSRootDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\AuthoMail_KitTest\PCIRoot");
        //private static DirectoryInfo destPnSDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\AuthoMail_KitTest\Delete\PnSTrash");

        public static void StartPurging(string retentionPeriod, string trashDays)
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            using (IHomeService homeSvc = new HomeService())
            using (IClientService clientSvc = new ClientService())
            {
                var clients = homeSvc.GetAllActiveClient();
                foreach (var client in clients)
                {
                    var clientDetails = clientSvc.GetClientByAppCode(client.AppCode).Result;
                    EnumerateClientDirs(retentionPeriod, clientDetails, sourceAMRootDir, destAMDir);
                    EnumerateClientDirs(retentionPeriod, clientDetails, sourcePnSRootDir, destPnSDir);
                }
                EnumerateOtherDirs();
            }

            #region delete moved files
            DateTime startDate = Common.Today;
            startDate = startDate.AddDays(-int.Parse(trashDays));

            var destAMDirs = destAMDir.EnumerateDirectories().ToList();

            foreach (DirectoryInfo destDir in destAMDirs)
            {
                var amSubDirs = destDir.EnumerateDirectories().Where(d => d.LastWriteTime < startDate).ToList();
                if (destDir.Name == "Archive")
                {
                    var files = destDir.EnumerateFiles("*.zip").Where(d => d.LastWriteTime < startDate).ToList();
                    foreach (var file in files)
                    {
                        File.Delete(file.FullName);
                    }
                }
                foreach (DirectoryInfo amSubDir in amSubDirs)
                {
                    amSubDir.Delete(true);
                }
            }

            var destPnSDirs = destPnSDir.EnumerateDirectories().ToList();

            foreach (DirectoryInfo destDir in destPnSDirs)
            {
                var pnsSubDirs = destDir.EnumerateDirectories().Where(d => d.LastWriteTime < startDate).ToList();
                foreach (DirectoryInfo pnsSubDir in pnsSubDirs)
                {
                    pnsSubDir.Delete(true);
                }
            }
            #endregion
        }

        private static void EnumerateClientDirs(string retentionPeriod, Client client, DirectoryInfo rootDir, DirectoryInfo destDir)
        {
            int days = retentionPeriod == "DB" ? client.RetentionPeriod : int.Parse(retentionPeriod);
            var dir = new DirectoryInfo(Path.Combine(rootDir.FullName, client.AppCode));

            if (dir.Exists)
            {
                if (!Directory.Exists(Path.Combine(destDir.FullName, dir.Name)))
                    Directory.CreateDirectory(Path.Combine(destDir.FullName, dir.Name));

                MoveDirs(client, destDir, days, dir);
            }
        }

        private static void EnumerateOtherDirs()
        {
            var archiveDir = new DirectoryInfo(Path.Combine(sourceAMRootDir.FullName, "Archive"));
            var bundleRootDir = new DirectoryInfo(Path.Combine(sourcePnSRootDir.FullName, "BundlingRoot"));

            if (!Directory.Exists(Path.Combine(destAMDir.FullName, archiveDir.Name)))
                Directory.CreateDirectory(Path.Combine(destAMDir.FullName, archiveDir.Name));
            if (!Directory.Exists(Path.Combine(destPnSDir.FullName, bundleRootDir.Name)))
                Directory.CreateDirectory(Path.Combine(destPnSDir.FullName, bundleRootDir.Name));

            MoveDirs(null, destAMDir, 7, archiveDir);
            MoveDirs(null, destPnSDir, 30, bundleRootDir);
        }

        private static int MoveDirs(Client client, DirectoryInfo destDir, int days, DirectoryInfo dir)
        {
            DateTime startDate = Common.Today;
            try
            {
                do
                {
                    startDate = startDate.AddDays(-1);
                    days -= IsBusinessDay(startDate, client == null ? new List<Holiday> { } : Config.GetHolidaysByClientId(client.Id), false, false) ? 1 : 0;
                } while (0 != days);

                var subdirs = dir.EnumerateDirectories().Where(d => d.LastWriteTime < startDate).ToList();

                if (subdirs.Count == 0)
                {
                    var files = dir.EnumerateFiles("*.zip").Where(d => d.LastWriteTime < startDate).ToList();
                    foreach (var file in files)
                    {
                        File.Move(file.FullName, Path.Combine(Path.Combine(destDir.FullName, dir.Name), file.Name));
                    }
                }
                else
                {
                    foreach (var subdir in subdirs)
                    {
                        if (Directory.Exists(Path.Combine(Path.Combine(destDir.FullName, dir.Name), subdir.Name)))
                            Directory.Delete(Path.Combine(Path.Combine(destDir.FullName, dir.Name), subdir.Name));
                        Directory.Move(subdir.FullName, Path.Combine(Path.Combine(destDir.FullName, dir.Name), subdir.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "Error occured while PURGING directories and files for client {0}: {1} ".Fill(client.AppCode, ex.ToString());
                log.Error(msg, ex);
                MailUtility.SendUnexpectedExceptionEmail(msg);
            }

            return days;
        }

        private static bool IsBusinessDay(DateTime date, List<Holiday> holidays, bool isSatWorkDay, bool isSunWorkDay)
        {
            if (holidays.Select(c => c.HolidayDate).Contains(date.Date))
                return false;

            if (date.DayOfWeek == DayOfWeek.Saturday)
                return isSatWorkDay;

            if (date.DayOfWeek == DayOfWeek.Sunday)
                return isSunWorkDay;

            return true;
        }

        public static void DeleteFiles()
        {
            foreach (DirectoryInfo amDir in destAMDir.GetDirectories())
            {
                amDir.Delete(true);
            }
            foreach (DirectoryInfo pciDir in destPnSDir.GetDirectories())
            {
                pciDir.Delete(true);
            }
        }

        public static void PurgeDBRecords()
        {
            using (IReCalculateDueDateService reCalculateDueDateService = new ReCalculateDueDateService())
            {
                var recordsToUpdateOnUSPS = reCalculateDueDateService.GetUSPSRecordsToPurge();
                try
                {
                    foreach (var record in recordsToUpdateOnUSPS)
                    {
                        reCalculateDueDateService.UpdateUSPSRecords(record.Id);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while PURGING records from [API].[USPS] Table: {0} ".Fill(ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                }

                var recordsToUpdateOnUSPSeVS = reCalculateDueDateService.GetUSPSeVSRecordsToPurge();
                try
                {
                    foreach (var record in recordsToUpdateOnUSPSeVS)
                    {
                        reCalculateDueDateService.UpdateUSPSeVSRecords(record.Id);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while PURGING records from [API].[USPSeVS] Table: {0} ".Fill(ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                }
            }
        }

        public static void CheckDiskSpaceAndAlert(string driveLetter, double thresholdPercent)
        {
            string uncPath = Config.Common_SharedPath;
            
            Process.Start("net", $"use {driveLetter}: {uncPath} /persistent:no").WaitForExit();
            log.Debug("Mapped drive: " + driveLetter + " to UNC path: " + uncPath);
            driveLetter += ":\\";
            try
            {
                DriveInfo drive = new DriveInfo(driveLetter);
                log.Debug($"Checking disk space for: {uncPath}");
                log.Debug($"Total Size: {drive.TotalSize / (1024 * 1024 * 1024)} GB");
                log.Debug($"Available Space: {drive.AvailableFreeSpace / (1024 * 1024 * 1024)} GB");
                double usedPercent = ((double)(drive.TotalSize - drive.TotalFreeSpace) / drive.TotalSize) * 100;

                if (usedPercent >= thresholdPercent)
                {
                    log.Debug("Send the mail as disk usage is above threshold: " + usedPercent + "%");
                    string msg = $"Disk usage on {uncPath} (mapped as {driveLetter}) is {usedPercent:F1}%";

                    MailUtility.SendUnexpectedExceptionEmail(msg);
                }
            }
            finally
            {
                log.Debug("unmapping the drive letter: " + driveLetter);
                Process.Start("net", $"use {driveLetter}: /delete").WaitForExit();
            }
        }

        public static void PurgeArchiveFiles(string days)
        {
            using (IClientService clientService = new ClientService())
            {
                var sftChannels = clientService.GetUncSFTChannelDetails();
                
                foreach (var sftChannel in sftChannels)
                {
                    DateTime startDate = Common.Today;
                    log.Debug($"UTC StartDate: {startDate}");
                    DirectoryInfo archiveDirPath = new DirectoryInfo(Path.Combine(sftChannel.Host + sftChannel.ArchiveDir));
                    log.Debug($"Purging start for SftChannel {sftChannel.Code} on Path: {archiveDirPath}");
                    
                    int purgingDays = int.Parse(days);
                    try
                    {
                        do
                        {                            
                            startDate = startDate.AddDays(-1);
                            purgingDays -= IsBusinessDay(startDate, Config.Common_FederalHolidays, false, false) ? 1 : 0;
                            log.Debug($"PurgingDays: {purgingDays}");
                        } while (0 != purgingDays);

                        var files = archiveDirPath.EnumerateFiles().Where(d => d.LastWriteTime < startDate).ToList();
                        foreach (var file in files)
                        {
                            if (File.Exists(file.FullName))
                                File.Delete(file.FullName);
                        }
                        log.Debug($"Purging end for SftChannel {sftChannel.Code} on Path: {archiveDirPath}");

                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured while PURGING Archive files for sftchannel {0}: {1} ".Fill(sftChannel.Code, ex.ToString());
                        log.Error(msg, ex);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                    }
                }
            }
        }
    }
}
