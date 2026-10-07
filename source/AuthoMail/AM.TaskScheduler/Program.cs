using Dapper;
using log4net;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.TaskScheduler
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            try
            {
                
                log.Info("Job Started");

                var cs = ConfigurationManager.ConnectionStrings["AuthoMail"];
                var day = int.Parse(ConfigurationManager.AppSettings["Day"]);

                if (cs == null)
                {
                    throw new ArgumentException("Connection string with name '{0}' does not exist.", "");
                }

                var factory = DbProviderFactories.GetFactory(cs.ProviderName);
                var db = factory.CreateConnection();
                db.ConnectionString = cs.ConnectionString;

                log.Info("DB connected");

                var count = db.ExecuteScalar<int>(@"SELECT count(P.Id) FROM AM.MailPieces P
                            INNER JOIN AM.MailBatches B ON P.MailBatchId=B.Id
                            INNER JOIN PnS.MRDFRequest REQ ON P.UniqueId=REQ.UniqueId
                            INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
                            WHERE
                            BS.PipelineStatus='Prepped'
                            AND dbo.ufn_UTCToEST(B.ReceivedOn) BETWEEN @start AND @end",
                            new
                            {
                                start = DateTime.Today.AddDays(day).Add(new TimeSpan(19, 0, 1)),
                                end = DateTime.Today.AddDays(day).Add(new TimeSpan(20, 0, 0))
                            });

                log.InfoFormat("{0} Record(s) found", count);

                //var count = 1;
                if (count > 0)
                {
                    string jobname = ConfigurationManager.AppSettings["JobName"];

                    using (TaskService ts = new TaskService(@"\"))
                    {
                        // if it is not connected than exception occurred
                        //if (ts.Connected)
                        //{
                        using (Microsoft.Win32.TaskScheduler.Task t = ts.AllTasks.Single(task => task.Name == jobname))
                        {
                            // if task not found than exception occurred
                            if (t.State != TaskState.Running)// && t.IsActive)
                            {
                                t.Run();
                            }
                        }
                    }
                }

                log.Info("Job completed");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error : {0}", ex);
            }
        }
    }
}
