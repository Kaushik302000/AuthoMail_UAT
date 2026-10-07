using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitneyBowes.API
{
    static class AppConfig
    {

        public static string PitneyBowesAPIURL { get; private set; }
        public static string PitneyBowesAPIKey { get; private set; }
        

        static AppConfig()
        {
            try
            {
                PitneyBowesAPIURL = ConfigurationManager.AppSettings["pitneybowesapiurl"].Trim();
                PitneyBowesAPIKey = ConfigurationManager.AppSettings["pitneybowesapikey"].Trim();
                
            }
            catch (Exception ex)
            {
            }
        }
    }
}
