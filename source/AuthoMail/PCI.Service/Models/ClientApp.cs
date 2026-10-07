using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    public class Client
    {
        public int Id { get; set; }
        public string AppCode { get; set; }
        public string Name { get; set; }
        public bool IsPnSActive { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int BUId { get; set; }
        public string BusinessUnit { get; set; }
        public string CustCode { get; set; }
    }

    public class PrintVendor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VendorCode { get; set; }
        public string PrintVendorCode { get; set; }
        public int SFTChannelId { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromADEDir { get; set; }
        public string ToADEDir { get; set; }
        public string BusinessCode { get; set; }
        public string ArchiveDir { get; set; }
        public string PostDownloadAction { get; set; }
    }

    public class SFTChannel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ToADEDir { get; set; }
        public string FromADEDir { get; set; }
        public string ArchiveDir { get; set; }
        public string PostDownloadAction { get; set; }
    }
}
