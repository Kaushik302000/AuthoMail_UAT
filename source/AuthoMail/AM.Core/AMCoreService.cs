using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AM.Core
{
    partial class AMCoreService : ServiceBase
    {
        public AMCoreService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //AMCore.PrepPnSJobs();
            //AMCore.SubmitBatchesDocTypeWiseToPrintVendor();
            //AMCore.GetSubmittedPnSResponses();
            //AMCore.ProcessPnSResponses();
            //AMCore.ReportPreppedProcess();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
