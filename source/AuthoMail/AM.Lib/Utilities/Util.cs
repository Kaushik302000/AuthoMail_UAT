using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Lib
{
    public static class Util
    {
        public static string AMResponseFolderName { get; private set; }
        public static string PnSResponsesFolderName { get; private set; }

        static Util()
        {
            AMResponseFolderName = "AMResponse";
            PnSResponsesFolderName = "PnSResponses";
        }
    }
    [AttributeUsage(AttributeTargets.All)]
    public class ExcludeInExcel : Attribute
    { }
}
