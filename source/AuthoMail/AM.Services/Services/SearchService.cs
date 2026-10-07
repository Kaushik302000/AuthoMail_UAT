using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using System.Data;
namespace AM.Services
{
    public interface ISearchService : IDisposable
    {
        //Remove Method(make sure below method in use)
        //List<MailPieces> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel, string clientBatchId, int? mailBatchId, string trackingNo)
        KeyValuePair<int, List<SearchViewModel>> GetMailPieceByParam
            (int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, string mailPieceUniqueId, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, string clientBatchId, int? mailBatchId, string trackingNo, List<string> errorList, bool isDownload, int? mailPieceId);
        MailPieces GetMailPieceByUniqueId(Guid uniqueId);
        List<MailPieces> GetAllData(int clientId);

        List<TestModel> TestMp();

        List<DateTime> GetHolidays();
    }
    public class SearchService : ServiceBase, ISearchService
    {
        public KeyValuePair<int, List<SearchViewModel>> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, string mailPieceUniqueId, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, string clientBatchId, int? mailBatchId, string trackingNo, List<string> errorList, bool isDownload, int? mailPieceId)
        {
            string query = @"SELECT {{fields}}
	 FROM AM.MailPieces m
     inner join AM.MailBatches b on m.MailBatchId=b.Id
     inner join Common.SFTChannels ch on b.SFTChannelId=ch.Id
     inner join Common.Clients c on m.ClientId=c.Id     
	 left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id 
     left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id
	 left outer JOIN Profile.Users u ON m.PulledBackRequestedBy=u.Id
     LEFT OUTER JOIN Common.Envelopes e ON pcd.EnvelopeId=e.Id
	 LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId=req.UniqueId
	 LEFT OUTER JOIN PnS.PrintTrailBatches ptb on req.PrintTrailBatchId=ptb.Id
	 WHERE m.ClientId=@clientId";
            string fields = @"m.Id AS MailPieceId,m.MailBatchId,m.BusinessFriendlyIdValue as LoanNumber,m.ShippingStatus as [Status],m.UniqueId,
	 m.RcptFirstName as FirstName,m.RcptLastName as LastName,m.RcptAddressStreet as [RcptAddress],m.RcptAddressCity as [City],m.RcptAddressZip as ZIP,
	 m.RcptAddressState as [State],m.RcptAddressCountry AS Country,m.PickedUpOn,m.ShippedOn,m.TrackingNo,m.DueDate,m.ErrorCode,m.ErrorDesc,m.PrintLocation,m.IsEDeliveryRequested, m.IsESignRequested,m.ExpiryDate,m.Inserts,m.ShipmentStatus,m.ShipmentStatusUpdatedOn,
	 b.ReceivedOn,m.DocTypeId,
	 d.DocTypeName as DocType,pcd.ShippingServiceType as ShippingMethod,pcd.ShippingCarrier,
	 ch.Title as Channel, m.TotalSheets,m.DeliveryStatus,m.DeliveryDate,m.Inserts,
	e.EnvelopeName,ptb.PrintBatchName as PrintFileName, req.JobId, req.PDFFileName, c.AppCode, c.Id AS ClientId,C.RetentionPeriod, C.IsEDeliveryEnabled, b.ServerIP";
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add("@clientId", clientId);
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdName))
            {
                query = string.Format("{0} and m.BusinessFriendlyIdName=@BusinessFriendlyIdName", query);
                parameter.Add("@BusinessFriendlyIdName", businessFriendlyIdName);
            }
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdValue))
            {
                query = string.Format("{0} and m.businessFriendlyIdValue like @businessFriendlyIdValue", query);
                parameter.Add("@businessFriendlyIdValue", $"%{businessFriendlyIdValue}%");
            }

            if (!string.IsNullOrWhiteSpace(mailPieceUniqueId))
            {
                query = string.Format("{0} and m.UniqueId = @mailPieceUniqueId", query);
                parameter.Add("@mailPieceUniqueId", mailPieceUniqueId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = string.Format("{0} and m.ShippingStatus=@ShippingStatus", query);
                parameter.Add("@ShippingStatus", status);
            }
            if (!string.IsNullOrWhiteSpace(channel))
            {
                query = string.Format("{0} and ch.Code=@Code", query);
                parameter.Add("@Code", channel);
            }
            if (!string.IsNullOrWhiteSpace(clientBatchId))
            {
                query = string.Format("{0} and b.ClientBatchId=@ClientBatchId", query);
                parameter.Add("@ClientBatchId", clientBatchId);
            }
            if (!string.IsNullOrWhiteSpace(trackingNo))
            {
                query = string.Format("{0} and m.TrackingNo=@TrackingNo", query);
                parameter.Add("@TrackingNo", trackingNo);
            }
            if (mailBatchId.HasValue)
            {
                query = string.Format("{0} and m.MailBatchId=@MailBatchId", query);
                parameter.Add("@MailBatchId", mailBatchId.Value);
            }
            if (docType != null && docType.Count > 0)
            {
                query = string.Format("{0} and d.Id in @docType", query);
                parameter.Add("@docType", docType);
            }
            if (errorList != null && errorList.Count > 0)
            {
                query = string.Format("{0} and m.ErrorCode in @ErrorCode", query);
                parameter.Add("@ErrorCode", errorList);
            }
            if (mailPieceId.HasValue)
            {
                query = string.Format("{0} and m.Id=@mailPieceId", query);
                parameter.Add("@mailPieceId", mailPieceId.Value);
            }
            if (!startDate.HasValue && (trackingNo.IsNullOrWhiteSpace() && clientBatchId.IsNullOrWhiteSpace() && businessFriendlyIdValue.IsNullOrWhiteSpace() && !mailBatchId.HasValue && !mailPieceId.HasValue))
                startDate = endDate = Common.EstToday;

            //if (parameter.ParameterNames.Count() == 2 && !startDate.HasValue && !endDate.HasValue)
            //    startDate = endDate = Common.EstToday;

            if (startDate.HasValue && endDate.HasValue)
            {
                query = string.Format("{0} and b.ReceivedOn BETWEEN @startDate and @endDate", query);
                parameter.Add("@startDate", startDate.Value.ToStartDateTime().EstToUtc());
                parameter.Add("@endDate", endDate.Value.ToEndDateTime().EstToUtc());
            }
            int count = db.ExecuteScalar<int>(query.Replace("{{fields}}", "count(m.Id)"), parameter);
            if (count <= Config.Web_Search_DisplayLimit || (isDownload && count <= Config.Web_Search_DownloadLimit))
            {
                query = string.Format("{0} order by b.ReceivedOn desc", query);
                var data = db.Query<SearchViewModel>(query.Replace("{{fields}}", fields), parameter).ToList();
                foreach(var d in data)
                {
                    d.ImpressionCount = d.Channel.ToLower().Contains("credco") ? (d.TotalSheets * 2).ToString() : "NA";
                }
                return new KeyValuePair<int, List<SearchViewModel>>(count, data);
            }
            return new KeyValuePair<int, List<SearchViewModel>>(count, null);
        }

        public MailPieces GetMailPieceByUniqueId(Guid uniqueId)
        {
            string query = "SELECT * FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND b.ClientId=c.Id AND m.[UniqueId] = @uniqueId";
            var mp = db.Query<MailBatches, SFTChannels, Clients, MailPieces, Users, DocTypes, PnSConfigDetail, MailPieces>(query,
                 (b, s, c, m, u, d, p) =>
                 {
                     m.Clients = c;
                     m.MailBatches = b;
                     b.SFTChannels = s;
                     m.DocTypes = d;
                     m.Users = u;
                     m.PnSConfigDetails = p;
                     return m;
                 },
                 new
                 {
                     uniqueId = uniqueId
                 }).ToList().First();

            var mreq = db.QuerySingleOrDefault<MRDFRequests>("select r.*,p.PrintBatchName as PrintFileName from PnS.MRDFRequest r inner join PnS.PrintTrailBatches p on r.PrintTrailBatchId=p.Id where r.UniqueId=@uniqueId", new { uniqueId = mp.UniqueId });
            if (mreq != null) mp.MRDFRequests = mreq;
            return mp;
        }

        public List<MailPieces> GetAllData(int clientId)
        {
            //string query = "SELECT * FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id LEFT OUTER JOIN [AM].[MailPieceCustomInfo] cust ON m.Id=cust.MailPieceId WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND m.ClientId=c.Id AND m.ClientId=101";
            string query = "SELECT * FROM  [AM].[MailBatches] b,[AM].[MailPieces] m  LEFT OUTER JOIN [AM].[MailPieceCustomInfo] cust ON m.Id=cust.MailPieceId WHERE m.MailBatchId=b.Id AND m.ClientId=101";

            var dict = new Dictionary<int, MailPieces>();
            var data = db.Query<MailBatches, MailPieces, MailPieceCustomInfoTest, MailPieces>(query,
                (b, m, cust) =>
                {
                    MailPieces mp;
                    if (!dict.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mp.MailBatches = b;
                        dict.Add(mp.Id, mp);
                    }
                    mp.MailPieceCustomInfoTest.Add(cust);
                    //m.MailBatches = b;
                    return mp;
                }
                //splitOn: "MailPieceId"
                ).Distinct().ToList();

            return data;

            //    var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, Users, DocTypes, MailPieceCustomInfoTest, MailPieces>(query,
            //       (b, s, c, m, u, d, cust) =>
            //       {
            //           MailPieces mp;
            //           if (!dict.TryGetValue(m.Id, out mp))
            //           {
            //               mp = m;
            //               mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
            //               dict.Add(mp.Id, mp);
            //           }
            //           mp.MailPieceCustomInfoTest.Add(cust);
            //           mp.Clients = c;
            //           mp.MailBatches = b;
            //           b.SFTChannels = s;
            //           mp.DocTypes = d;
            //           mp.Users = u;
            //           return mp;
            //       }
            //       ).Distinct().ToList();
        }



        public List<TestModel> TestMp()
        {
            string query = "select m.id,m.shippingstatus,m.SLADays,mb.receivedon,m.duedate,m.DocTypeId,m.Documentorpackagetype,mb.validationstatus,m.duedate,m.ErrorCode,m.errordesc,d.bizdaycutoff,d.bizdayslatype,d.bizdaytimespan,d.satcutoff,d.satslatype,d.sattimespan,d.suncutoff,d.sunslatype,d.suntimespan from [AM].[MailBatches] mb, [AM].[MailPieces] m left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id where m.[MailBatchId] = mb.Id";//and mb.receivedon > '2018-08-27 04:01:00.000'"; //'2018-07-02 04:01:00.000'";

            return db.Query<TestModel>(query).ToList();
        }


        public List<DateTime> GetHolidays()
        {
            string q = "select HolidayDate from Common.Holidays";
            return db.Query<DateTime>(q).ToList();
        }
    }

    public class TestModel
    {
        public int Id { get; set; }
        public DateTime ReceivedOn { get; set; }
        public int? DocTypeId { get; set; }
        public DateTime? DueDate { get; set; }
        public string BizdayCutOff { get; set; }

        public TimeSpan? BizCutOffTimeSpan
        {
            get
            {
                if (this.BizdayCutOff.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.BizdayCutOff);
            }
        }
        public string BizdaySLAType { get; set; }
        public string bizdaytimespan { get; set; }
        public TimeSpan? ActualBizdayTimeSpan
        {
            get
            {
                if (this.bizdaytimespan.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.bizdaytimespan);
            }
        }
        public string satcutoff { get; set; }
        public TimeSpan? satcutoffTimeSpan
        {
            get
            {
                if (this.satcutoff.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.satcutoff);
            }
        }
        public string SatSLAType { get; set; }
        public string sattimespan { get; set; }
        public TimeSpan? ActualSatTimeSpan
        {
            get
            {

                if (this.sattimespan.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.sattimespan);
            }
        }
        public string suncutoff { get; set; }
        public TimeSpan? suncutoffTimeSpan
        {
            get
            {
                if (this.suncutoff.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.suncutoff);
            }
        }
        public string SunSLAType { get; set; }
        public string suntimespan { get; set; }
        public TimeSpan? ActualSunTimeSpan
        {
            get
            {
                if (this.suntimespan.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.suntimespan);
            }
        }
        public string ErrorCode { get; set; }

    }

}
