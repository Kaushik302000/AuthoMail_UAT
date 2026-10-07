using AM.Entities;
using AM.Lib;
using AM.Services;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.ProductionToUATBridge
{
    public class RegenerateLabels
    {
        public void RegerateIMBLabels()
        {
            string pnsRootPath = ConfigurationManager.AppSettings["Regen_PnSRootPath"].Trim();
            string destRootPath = ConfigurationManager.AppSettings["Regen_DestRootPath"].Trim();
            string pciFolderName = ConfigurationManager.AppSettings["Regen_PCIRequestFolderName"].Trim();
            string dbString = ConfigurationManager.AppSettings["Regen_USPSIMBDbConnectionString"].Trim();
            using (IReCalculateDueDateService recalSvc = new ReCalculateDueDateService())
            {
                var mpList = recalSvc.GetMailPiecesForReprocessCoverLetter(new DateTime(2020, 05, 02));

            }
        }
    }
}
