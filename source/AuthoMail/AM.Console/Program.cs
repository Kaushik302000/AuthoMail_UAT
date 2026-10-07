using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    class Program
    {
        private static log4net.ILog log;
        static void Main(string[] args)
        {
                        //Code Added to create argument wise log file
            var argString = String.Join(",", args);
            log4net.LogicalThreadContext.Properties["args"] = argString;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(ConfigurationManager.AppSettings["log4net.Config"]));
            var type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            log = log4net.LogManager.GetLogger(type);

            log.InfoFormat("Command line arguments : {0}", String.Join(",", args));

            try
            {
                if (args.Length > 0)
                {
                    if (args[0] == "config")
                    {
                        System.Console.WriteLine("Seeding Start");

                        using (var cfgSvc = new ConfigService())
                        {
                            cfgSvc.SeedAMSettings();
                        }
                        Config.SaveAllSetting();
                        System.Console.WriteLine("Seed Completed.");
                        return;
                    }
                    else if (args[0] == "EffectiveReceivedOnCalculate")
                    {
                        System.Console.WriteLine("Calculation Start");

                        ReCalculateDueDate.UpdateErrorEffectiveReceivedOn();
                        System.Console.WriteLine("Calculation Completed");
                        return;
                    }
                    else if (args[0] == "VerifyAddress")
                    {
                        System.Console.WriteLine("Calculation Start");

                        ReCalculateDueDate.VerifyAddressesWithCity();
                        System.Console.WriteLine("Calculation Completed");
                        return;
                    }
                    else if (args[0] == "UpdatePnSConfigDetailId")
                    {
                        System.Console.WriteLine("Update PnSConfigDetailId Start");

                        ReCalculateDueDate.UpdatePnSConfigDetailId();

                        System.Console.WriteLine("Update PnSConfigDetailId Completed");
                        return;
                    }
                    else if (args[0] == "UpdateSFTChannels")
                    {
                        System.Console.WriteLine("Update UpdateSFTChannels Start");

                        ReCalculateDueDate.UpdateShipperSFTChannels(args);

                        System.Console.WriteLine("Update UpdateSFTChannels Completed");
                        return;
                    }
                    else if (args[0] == "ProcessHeldPackages")
                    {
                        System.Console.WriteLine("Update Batch Status To Acknowledged From Hold Status Start");

                        using (ICommonService commonSvc = new CommonService())
                        {
                            var holdPackageInfos = commonSvc.GetPackagesHoldInfo();

                            if (holdPackageInfos.Count > 0)
                            {
                                var currentHoldPackagesInfo = holdPackageInfos.Where(x => x.ProcessOn == Common.EstToday);

                                foreach (var holdInfo in currentHoldPackagesInfo)
                                {
                                    using (IMailBatchService mailBatchSvc = new MailBatchService())
                                    {
                                        mailBatchSvc.UpdateHoldBatchesInfo(holdInfo.ClientId, holdInfo.HoldOn);
                                    }
                                }
                            }
                        }

                        System.Console.WriteLine("Update Batch Status To Acknowledged From Hold Status Completed");
                        return;
                    }
                    else if (args[0] == "CustomReports")
                    {
                        using (IReportService reportSvc = new ReportService())
                        using (IGeneralService generalSvc = new GeneralService())
                        using (ICommonService commonSvc = new CommonService())
                        using (IAMCService amcSvc = new AMCService())
                        {
                            //var appCode = "PENFED";
                            //var client = clientSvc.GetClientByAppCode(appCode).Result;
                            //var mps = reportSvc.GetMailPiecesDataBetweenDates(new DateTime(2020, 09, 04).ToStartDateTime().EstToUtc(), new DateTime(2020, 09, 09).ToEndDateTime().EstToUtc(), client.Id);

                            //string responseReport = generalSvc.GeneralXMLReport(mps, appCode);
                            //string deliveryReportFileName = "{0}-{1}-{2}.xml".Fill(appCode, new Random().Next(100000, 1000000), new Random().Next(100000, 1000000));
                            //string deliveryReportPath = Path.Combine(@"D:\", deliveryReportFileName);

                            //File.WriteAllText(deliveryReportPath, responseReport);
                            var readIds = File.ReadAllLines(@"D:\MailPieceIds.txt").Select(r => int.Parse(r)).ToList();
                            List<MailPieces> mailPieces = reportSvc.GetMailPiecesDataFromMailPieceId(readIds);

                            var erroredRecords = mailPieces.Where(p => p.ShippingStatus == ShippingStatus.Error.ToString()).ToList();
                            var shippedorpulledbackRecords = mailPieces.Where(p => p.ShippingStatus == ShippingStatus.Shipped.ToString() || p.ShippingStatus == ShippingStatus.PulledBack.ToString()).ToList();


                            StringBuilder errorData = amcSvc.AMCErrorReport(erroredRecords);
                            if (errorData != null)
                            {
                                string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
                                string errorReportPath = Path.Combine(@"D:\", errorReportFileName);
                                File.WriteAllText(errorReportPath, errorData.ToString());
                            }

                            StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(shippedorpulledbackRecords);
                            if (deliveryReport != null)
                            {
                                string deliveryReportFileName = "Delivery-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
                                string deliveryReportPath = Path.Combine(@"D:\", deliveryReportFileName);
                                File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                            }
                        }
                    }
                    else if (args[0] == "BatchReporting")
                    {
                        //This will be used as How system will report back to the client. means based on response of printbatches table or full response so Here user needs to pass "Incremental" Or "Full" then comma separated SFTChannel codes
                        string reportType = args.Length > 2 ? args[1] : string.Empty;

                        if (string.IsNullOrEmpty(reportType))
                            return;


                        string[] channelList = args[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (reportType == "API")
                        {
                            PChannel.ReportBack(channelList, reportType);
                        }
                        else
                        {
                            Channel.ReportBack(channelList, reportType);
                        }
                    }
                    else if (args[0] == "Purging")
                    {
                        Purging.StartPurging(args[1], args[2]);
                    }
                    //else if (args[0] == "Delete")
                    //{
                    //    Purging.DeleteFiles();
                    //}
                    else if (args[0] == "PurgeDBRecords")
                    {
                        Purging.PurgeDBRecords();
                    }
                    else if (args[0] == "PurgeArchiveFiles")
                    {
                        Purging.PurgeArchiveFiles(args[1]);
                    }
                    else if (args[0] == "CheckDisk")
                    {
                        Purging.CheckDiskSpaceAndAlert(args[1], double.Parse(args[2])); // args[1] = "C", args[2] = "85"
                    }

                    else
                    {
                        using (var channel = ChannelFactory.Create(args))
                        {
                            if (channel == null)
                            {
                                log.Error("Channel prefix '{0}' is invalid.".Fill(args[0]));
                                return;
                            }

                            channel.ProcessBatches();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error occured in Main Program For {string.Join(",", args)}", ex);
            }
        }
    }
}
