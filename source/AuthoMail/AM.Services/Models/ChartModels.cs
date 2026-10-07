using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Services
{
    public class WeeklyChart
    {
        public DateTime Date { get; set; }
        public string Weekday { get; set; }
        public int InProgress { get; set; }
        public int ShippedPartial { get; set; }
        public int ShippedAll { get; set; }
        public int Failed { get; set; }
        public int PulledBack { get; set; }
        public int eDelivered { get; set; }
        public int eSigned { get; set; }
    }
    public class TodayMailPieceChart
    {
        public int MailBatchId { get; set; }
        public string BatchName { get; set; }
        public string ClientBatchId { get; set; }
        public int InProgress { get; set; }
        public int PulledBack { get; set; }
        public int Error { get; set; }
        public int Shipped { get; set; }
        public DateTime ReceivedOn { get; set; }
    }
    public class BatchListByChartData
    {
        public string ClientBatchId { get; set; }
        public int BatchId { get; set; }
        public Guid BatchUniqueId { get; set; }
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public string BatchStatus { get; set; }
        public string DocumentOrPackageType { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? ReceivedOnClient
        {
            get
            {
                if (ReceivedOn.HasValue) return ReceivedOn.Value.UtcToClientTimeZone(ClientTimeZone);
                return null;
            }
        }
        public string ClientTimeZone { get; set; }
        public string ValidationErrors { get; set; }
        public int MailPieceCount { get; set; }
    }
    public class DailySummaryChart
    {
        public int Id { get; set; }
        public string DocTypeCode { get; set; }
        public int DocTypeId { get; set; }
        public string ShippingStatus { get; set; }        
    }
    public class DailySummaryChartVM
    {
        public string DocTypeCode { get; set; }
        public int DocTypeId { get; set; }
        public int Shipped { get; set; }
        public int eDelivered { get; set; }
        public int eSigned { get; set; }
        public int InProgress { get; set; }
        public int PulledBack { get; set; }
        public int Error { get; set; }
    }
}
