using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using AM.Entities;

namespace AM.Console
{
    public class CalcStdPrepStartTime
    {
        public static void Start()
        {

            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            using (ICommonService cSvc = new CommonService())
            {
                var calcStdPrepStartTimeList = rSvc.GetCalcListOfMailPieces();
                var timings = rSvc.GetSweepTimings();
                DateTime bundleStartDate = new DateTime(2018, 9, 13);

                foreach (var item in calcStdPrepStartTimeList)
                {
                    item.StdSubmissionTime = cSvc.GetStandardSubmissionTime(item.AppCode, item.VendorCode, item.ReceivedOn, timings);
                    if (item.EffectiveReceivedOnEst.Value.DayOfWeek == DayOfWeek.Saturday)
                    {
                        item.MaxSubmissionTime = (item.EffectiveReceivedOnEst.Value.Date + TimeSpan.Parse(item.SatCutOff)).EstToUtc();
                    }
                    else
                    {
                        item.MaxSubmissionTime = (item.EffectiveReceivedOnEst.Value.Date + TimeSpan.Parse(item.BizdayCutOff)).EstToUtc();
                    }
                    rSvc.UpdateSubmissionTime(item.Id, item.StdSubmissionTime.Value, item.MaxSubmissionTime.Value, null);
                    if (item.ReceivedOn < bundleStartDate || item.PrintLocation == "Dallas CL")
                    {
                        item.StdSubmissionTime = item.ReceivedOn;
                    }
                    else
                    {
                        item.StdSubmissionTime = cSvc.GetStandardSubmissionTime(item.AppCode, item.PrintLocation, item.ReceivedOn, timings);
                    }

                    if (item.EffectiveReceivedOnEst.Value.DayOfWeek == DayOfWeek.Saturday)
                    {
                        item.MaxSubmissionTime = (item.EffectiveReceivedOnEst.Value.Date + TimeSpan.Parse(item.SatCutOff)).EstToUtc();
                    }
                    else
                    {
                        item.MaxSubmissionTime = (item.EffectiveReceivedOnEst.Value.Date + TimeSpan.Parse(item.BizdayCutOff)).EstToUtc();
                    }

                    rSvc.UpdateSubmissionTime(item.Id, item.StdSubmissionTime.Value, item.MaxSubmissionTime.Value, null);
                }
            }
        }
    }
}
