using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public static class Common
    {
        public static DateTime Now
        {
            get { return DateTime.UtcNow; }
        }

        public static DateTime Today
        {
            get { return DateTime.UtcNow.Date; }
        }

        public static DateTime EstToday
        {
            get { return DateTime.UtcNow.UtcToEst().Date; }
        }

        public static SFT_ProtocolType GetProtocolType(string protocolType)
        {
            SFT_ProtocolType SFT_Protocol;
            switch (protocolType.ToLower())
            {
                case "ftp":
                    SFT_Protocol = SFT_ProtocolType.ftp;
                    break;
                case "sftp":
                    SFT_Protocol = SFT_ProtocolType.sftp;
                    break;
                case "unc":
                    SFT_Protocol = SFT_ProtocolType.unc;
                    break;
                default: throw new Exception("Please provide valid protocol for 'SFT_Protocol_Value'.");
            }
            return SFT_Protocol;
        }
    }

    public static class Env
    {
        public static string Prod = "Prod";
        public static string Stage = "Stage";
        public static string QA = "QA";
        public static string Internal = "Internal";
        public static string Performance = "Perf";
    }
    public static class ClientUserMappingStatusTypes
    {
        public static string Pending = "Pending";
        public static string Approved = "Approved";
        public static string Declined = "Declined";
    }
}
