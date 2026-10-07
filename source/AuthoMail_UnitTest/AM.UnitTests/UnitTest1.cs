using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AM.Services;
using System.Collections;
using System.Collections.Generic;
using AM.Entities;
using System.IO;
using AM.Lib;
using System.Data;
using System.Data.OleDb;
using System.Xml;


namespace AM.UnitTests
{
    //branching
    enum SLATypes
    {
        NA,
        FixedTime,
        Duration
    }

    [TestClass]
    public class UnitTest1
    {
        ISearchService serSvc = new SearchService();
        TimeSpan? cutOff = null;
        TimeSpan? slaTimeSpan = null;

        [TestMethod]
        public void TestMethod1()
        {
            List<TestModel> mp = serSvc.TestMp();
            //List<TestModel> mp = conect();

            var holidayList = serSvc.GetHolidays();

            //string slaType = null;
            DateTime dueDate = new DateTime();

            // convert received on from utc to est

            var results = new Dictionary<int, bool>();

            foreach (var item in mp)
            {
                #region TEST - ONLY FOR PIECE MAIL PIELCE ID TESTING
                if (item.Id == 76442)
                {
                             //string s = "let me Check";
                }
                #endregion

                item.ReceivedOn = item.ReceivedOn.UtcToEst();

                //item.receivedon = item.receivedon.UtcToEst();

                if (!item.DocTypeId.HasValue)
                {
                    results.Add(item.Id, !item.DueDate.HasValue);
                    continue;
                }
                if (item.DocTypeId.HasValue && item.ErrorCode != "No_Error")
                {
                    results.Add(item.Id, !item.DueDate.HasValue);
                    continue;
                }

                #region LATEST CHANGES FOR DUE DATE

                var Receivedon = item.ReceivedOn.Date;
                var Receivedtime = item.ReceivedOn.TimeOfDay;
                DateTime ERDateTime = new DateTime();

                var dow = GetTimeSpanByDOW(item.ReceivedOn.DayOfWeek, item);

                if (!IsHoliday(item.ReceivedOn, holidayList, item.SatSLAType == SLATypes.NA.ToString(), item.SunSLAType == SLATypes.NA.ToString()) && Receivedtime < dow.Item1)
                {
                    ERDateTime = item.ReceivedOn;
                }
                else
                {
                    ERDateTime = GetNextWorkingDay(item.ReceivedOn.Date, holidayList, item.SatSLAType == SLATypes.NA.ToString(), item.SunSLAType == SLATypes.NA.ToString());
                }

                var Edow = GetTimeSpanByDOW(ERDateTime.DayOfWeek, item);

                TimeSpan? EffectiveTimeSpan = new TimeSpan();

                EffectiveTimeSpan = Edow.Item2.HasValue ? (TimeSpan?)Edow.Item2.Value : null;

                var slaTime = EffectiveTimeSpan.HasValue ? string.Format("{0:hh\\:mm\\:ss}", EffectiveTimeSpan.Value) : "";

                dueDate = ERDateTime.Date + TimeSpan.Parse((string.IsNullOrEmpty(slaTime) ? "00:00:00" : slaTime));

                int TimeSpanDays = item.bizdaytimespan.IsNullOrWhiteSpace() ? 0 : Convert.ToInt32(item.bizdaytimespan.Split(':').First());

                for (int i = 0; i < TimeSpanDays; i++)
                {
                    dueDate = GetNextWorkingDay(dueDate, holidayList, item.SatSLAType == SLATypes.NA.ToString(), item.SunSLAType == SLATypes.NA.ToString());
                }
                #endregion

                #region OLD CODE
                //var dow = item.ReceivedOn.DayOfWeek;

                //if (dow == DayOfWeek.Saturday)
                //{
                //    slaType = item.SatSLAType;
                //    slaTimeSpan = item.ActualSatTimeSpan;
                //    cutOff = item.satcutoffTimeSpan;

                //    if (slaType == SLATypes.NA.ToString())
                //    {
                //        dow = DayOfWeek.Sunday;
                //    }
                //}
                //if (dow == DayOfWeek.Sunday)
                //{
                //    slaType = item.SunSLAType;
                //    slaTimeSpan = item.ActualSunTimeSpan;
                //    cutOff = item.suncutoffTimeSpan;

                //    if (slaType == SLAType.NA.ToString())
                //    {
                //        dow = DayOfWeek.Monday;
                //    }
                //}
                //if (dow == DayOfWeek.Monday || dow == DayOfWeek.Tuesday || dow == DayOfWeek.Wednesday || dow == DayOfWeek.Thursday || dow == DayOfWeek.Friday)
                //{
                //    slaType = item.BizdaySLAType;
                //    slaTimeSpan = item.ActualBizdayTimeSpan;
                //    cutOff = item.BizCutOffTimeSpan;
                //}


                //if (slaType == SLAType.FixedTime.ToString())
                //{
                //    //var effectiveCutOff = item.receivedon.Date.Add(TimeSpan.Parse(cutOff));
                //    //var regularDueDate = item.receivedon.Date.Add(TimeSpan.Parse(cutOff));

                //    var effectiveCutOff = item.BizCutOffTimeSpan.HasValue ? item.ReceivedOn.Date.Add(item.BizCutOffTimeSpan.Value) : (DateTime?)null;
                //    //var regularDueDate = item.receivedon.Date.Add(cutOff.Value);
                //    var regularDueDate = item.ReceivedOn.Date.Add(slaTimeSpan.Value);

                //    //dueDate = item.ReceivedOn > effectiveCutOff ? regularDueDate.AddDays(1) : regularDueDate;

                //    // LATEST CHANGE

                //    dueDate = item.ReceivedOn.Date + slaTimeSpan.Value;
                //}
                //else
                //{
                //    dueDate = slaTimeSpan.HasValue ? item.ReceivedOn + slaTimeSpan.Value : item.ReceivedOn;
                //}

                //while (IsHoliday(dueDate, holidayList, item.SatSLAType == SLATypes.NA.ToString(), item.SunSLAType == SLATypes.NA.ToString()))
                //    dueDate = dueDate.AddDays(1);

                #endregion
                dueDate = GeneralExtensions.EstToUtc(dueDate);
                results.Add(item.Id, item.DueDate == dueDate);
            }

            #region CODE - If all duedate are correct then pass otherwise fail
            var failures = results.Where(kp => kp.Value == false).ToList();
            var failureList = String.Join(", ", failures.Select(kp => kp.Key));

            if (failures.Count > 0)
            {
                Assert.Fail("Total {0} mailpieces tested. {1} failed. List - {2}", results.Count, failures.Count, failureList);
            }
            #endregion
        }

        private Tuple<TimeSpan?, TimeSpan?> GetTimeSpanByDOW(DayOfWeek dow, TestModel item)
        {
            if (dow == DayOfWeek.Saturday)
            {
                cutOff = item.satcutoffTimeSpan;
                slaTimeSpan = item.ActualSatTimeSpan;
            }
            if (dow == DayOfWeek.Sunday)
            {
                cutOff = item.satcutoffTimeSpan;
                slaTimeSpan = item.ActualSunTimeSpan;
            }
            if (dow == DayOfWeek.Monday || dow == DayOfWeek.Tuesday || dow == DayOfWeek.Wednesday || dow == DayOfWeek.Thursday || dow == DayOfWeek.Friday)
            {
                cutOff = item.BizCutOffTimeSpan;
                slaTimeSpan = item.ActualBizdayTimeSpan;
            }
            return Tuple.Create(cutOff, slaTimeSpan);
        }

        private DateTime GetNextWorkingDay(DateTime dateTime, List<DateTime> holidayList, bool p1, bool p2)
        {
            do
            {
                dateTime = dateTime.AddDays(1);
            } while (IsHoliday(dateTime, holidayList, p1, p2));
            return dateTime;
        }


        private bool IsHoliday(DateTime dueDate, List<DateTime> holidayList, bool isSatHoliday, bool isSunHoliday)
        {
            if (holidayList.Contains(dueDate.Date))
            {
                return true;
            }

            if (dueDate.DayOfWeek == DayOfWeek.Saturday)
            {
                return isSatHoliday;
            }

            if (dueDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return isSunHoliday;
            }
            return false;
        }


        #region database connection
        public List<TestModel> conect()
        {

            var fileName = @"E:\AuthoMail\AM-Prod.DueDate.xlsx";
            DataTable dt = new DataTable();

            //var fileName = @"E:\rWiki\tfs\Main\Docs\Jagan\test.xlsx";	
            //var excelDb = CreateDatabase(fileName);

            string connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=NO;';", fileName);
            string query = string.Format("select * from [{0}$] ", "Sheet1");

            OleDbConnection con = new OleDbConnection(connectionString);
            OleDbDataAdapter dbadapter = new OleDbDataAdapter(query, con);
            dbadapter.Fill(dt);
            List<TestModel> list = new List<TestModel>();

            if (dt.Rows.Count > 0)
            {
                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    TestModel t = new TestModel();
                    t.Id = Convert.ToInt32(dt.Rows[i][0].ToString());
                    t.ReceivedOn = DateTime.Parse(dt.Rows[i][3].ToString());
                    t.DocTypeId = Convert.ToInt32(dt.Rows[i][5].ToString());
                    t.DueDate = string.IsNullOrEmpty(dt.Rows[i][4].ToString()) ? (DateTime?)null : DateTime.Parse(dt.Rows[i][4].ToString());  //DateTime.Parse(dt.Rows[i][4].ToString());
                    t.ErrorCode = dt.Rows[i][9].ToString();
                    t.BizdayCutOff = DateTime.Parse(dt.Rows[i][11].ToString()).ToString("HH:mm:ss");
                    t.BizdaySLAType = dt.Rows[i][12].ToString();
                    t.bizdaytimespan = string.IsNullOrEmpty(dt.Rows[i][13].ToString()) ? null : dt.Rows[i][13].ToString();
                    t.satcutoff = DateTime.Parse(dt.Rows[i][14].ToString()).ToString("HH:mm:ss");// string.Format("{0:hh\\:mm\\:ss}", dt.Rows[i][14]); 
                    t.SatSLAType = dt.Rows[i][15].ToString();
                    t.sattimespan = string.IsNullOrEmpty(dt.Rows[i][16].ToString()) ? null : dt.Rows[i][16].ToString();
                    t.suncutoff = string.IsNullOrEmpty(dt.Rows[i][17].ToString()) ? null : dt.Rows[i][17].ToString();
                    t.SunSLAType = dt.Rows[i][18].ToString();
                    t.suntimespan = string.IsNullOrEmpty(dt.Rows[i][19].ToString()) ? null : dt.Rows[i][19].ToString();

                    list.Add(t);
                }
            }

            return list;
            //DataSet excelData = excelDb.ExecuteDataSet(CommandType.Text, string.Format("select * from [{0}$] ", "Sheet2"));
        }
        //public static Database CreateDatabase(string filePath)
        //{

        //    //ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings[name];
        //    DbProviderFactory dbProviderFactory = DbProviderFactories.GetFactory("System.Data.OleDb");
        //    string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=YES;';";
        //    GenericDatabase db = new GenericDatabase(String.Format(connectionString, filePath), dbProviderFactory);
        //    return db;
        //}

        #endregion
    }

}
