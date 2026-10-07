using AM.Lib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.EDelivery
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

            if (args[0] == "GENERAL")
            {
                using (var channel = ChannelFactory.Create())
                {
                    if (channel == null)
                    {
                        log.Error("Channel prefix '{0}' is invalid.".Fill(args[0]));
                        return;
                    }
                    channel.ProcessBatches();
                }
            }
            else if (args[0] == "CHECKLOG")
            {
                EDChannel.CheckActivityLogs();
            }
            else if (args[0] == "DOCDOWNLOAD")
            {
                EDChannel.DownloadEsignDocs(); 
            }
        }
    }
}
