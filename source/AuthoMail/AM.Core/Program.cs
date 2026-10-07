using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AM.Core
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool prepFlag, submitFlag, submittedResFlag, parseResFlag, reportPrepResFlag;           

            do
            {
                prepFlag = AMCore.PrepPnSJobs();
                submitFlag = AMCore.SubmitBatchesDocTypeWiseToPrintVendor();
                submittedResFlag = AMCore.GetSubmittedPnSResponses();
                parseResFlag = AMCore.ProcessPnSResponses();
                reportPrepResFlag = AMCore.ReportPreppedProcess();

            } while (prepFlag || submitFlag || submittedResFlag || parseResFlag || reportPrepResFlag);
        }
    }
}
