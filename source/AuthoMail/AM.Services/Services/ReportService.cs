using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Entities;
using AM.Lib;
using System.IO;
using AM.Services.Models;
using System.Data.SqlClient;
using CsvHelper;
using System.Globalization;
using CsvHelper.TypeConversion;

namespace AM.Services
{
    public interface IReportService : IDisposable
    {
        List<MailPieces> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel, string shippingStatus);
        List<MailPieces> GetErrorReportByDateOrBatch(int clientId, DateTime startDate, DateTime endDate, int? batchId, string channel);
        List<MailPieces> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<MailPieces> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime startDate, DateTime endDate, int? batchId, int? docType, string filterBy);
        List<MonthlyReconReportViewModel> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ChannelCodeTitle> GetChannelTitle(int clientId);
        List<PullBackRequests> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate);
        List<MailPieces> GetMailPiecesDataFromPrintBatchId(int printBatchId);
        List<MailPieces> GetMailPiecesDataFromMailBatchId(int mailBatchId);
        MailPieces GetMailPiecesDataFromMailBatchIdAndUniqueId(int mailBatchId, Guid uniqueId);
        List<PCISubmittedBatchDetail> GetPCIReconReport(DateTime startDate, DateTime endDate);
        List<DALLASSubmittedBatchDetail> GetDALLASReconReport(DateTime startDate, DateTime endDate);
        List<SupplyInventoryVM> SupplyInventoryReport(List<string> envelopes);
        MailBatches GetMailBatchDataFromMailBatchId(int mailBatchId);
        List<ReportWizard> GetReportWizard(List<int> clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, List<string> shippingMethod, string trackingNo);
        List<AutoReportWizard> GetAutoReportWizard(DateTime startDate, DateTime endDate);
        List<TrackingReport> GetFedExData(int[] isPickedUp = null);
        string GetAppcodeByClientid(int clientId);
        List<string> GetMonthlyReconDate(int clientId);
        List<GOSMailPieceData> GetGOSDailyReconReport(DateTime currentDate);
        List<GOSMailPieceData> GetGOSDataPointReport(DateTime startDate, DateTime endDate, List<string> clientCodes = null);
        List<TrackingReport> GetFedExDataForTracking(DateTime startDueDate, DateTime endDueDate, string alias = null, List<string> clientCodes = null);
        List<MailPieces> GetMailPiecesDataBetweenDates(DateTime startDate, DateTime endDate, int clientId);
        List<TrackingReport> GetUSPSeVSDataForTracking(DateTime startDueDate, DateTime endDueDate, string alias = null, List<string> clientCodes = null);
        List<MailPieces> GetMailPiecesDataFromPrintBatchId(List<int> printBatchIds);
        List<MailPieces> GetMailPiecesDataFromMailPieceId(List<int> mpIds);
        MemoryStream USAAMTDVolumeReport(DateTime startDate, DateTime endDate);
        List<PrintVendor> GetActivePrintVendors();
        List<PrintVendorLocations> GetActivePrintVendorsWithLocation();
        List<MonthlyReconReportViewModel> GetPVReconReport(string pvCode, DateTime startDate, DateTime endDate, List<int> clientId, string status, List<string> shippingMethod);
        List<EnvelopeInventoryUsage> GetEnvelopesUsageReport(int clientId, int? envelopeId, string printVendorCode, DateTime startDate, DateTime endDate);
        List<MonthlyReconReportViewModel> GetDuplicateReportByDate(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<AMPostageData> GetAMPostageData(DateTime startDate, DateTime endDate);
        void UpdateTrackingInformation();
        List<ReshipMailPieceViewModel> GetReshipRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, int? docType, DateTime startDate, DateTime endDate, int clientId);

    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<MailPieces> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel, string shippingStatus)
        {
            string query = "[dbo].[uspweb_DailyDetailOperationsReport] @clientId, @startDate, @endDate, @batchId, @channel, @shippingStatus";
            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, PnSConfigDetail, MailPieces>(query,
                (b, s, c, m, d, p) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    m.PnSConfigDetails = p;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    shippingstatus = shippingStatus,
                    batchId = batchId,
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    startDate = date.HasValue ? (date.Value.ToStartDateTime()).EstToUtc() : date,
                    endDate = date.HasValue ? (date.Value.ToEndDateTime()).EstToUtc() : date
                }
                ).ToList();
            return data;
        }

        public List<MailPieces> GetErrorReportByDateOrBatch(int clientId, DateTime startDate, DateTime endDate, int? batchId, string channel)
        {
            string query = "[dbo].[uspweb_ErrorReport] @clientId, @startDate, @endDate, @batchId, @channel, @shippingStatus";
            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, PnSConfigDetail, MailPieces>(query,
                (b, s, c, m, d, p) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    m.PnSConfigDetails = p;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    shippingstatus = ShippingStatus.Error.ToString(),
                    batchId = batchId,
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    startDate = (startDate.ToStartDateTime()).EstToUtc(),
                    endDate = (endDate.ToEndDateTime()).EstToUtc()
                }
                ).ToList();
            return data;
        }

        public List<MailPieces> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = "[dbo].[uspweb_SummaryControlReport] @clientId, @startDate, @endDate, @channel";

            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, PnSConfigDetail, MailPieces>(query,
                (b, s, c, m, d, p) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    m.PnSConfigDetails = p;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    startDate = startDate.ToStartDateTime().EstToUtc(),
                    endDate = endDate.ToEndDateTime().EstToUtc(),
                    channel = channel.IsNullOrWhiteSpace() ? null : channel
                }
                ).ToList();

            return data;
        }

        public List<MailPieces> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime startDate, DateTime endDate, int? batchId, int? docType, string filterBy)
        {
            string query = "[dbo].[uspweb_ErrorReportSLA] @clientId, @startDateRec, @endDateRec, @startDateDue, @endDateDue, @batchId, @docType";
            bool isReceived = filterBy == "ByReceivedOn" ? true : false;
            DateTime? dt = null;

            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, PnSConfigDetail, MailPieces>(query,
                (b, s, c, m, d, p) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    m.PnSConfigDetails = p;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    batchId = batchId,
                    docType = docType,
                    startDateRec = isReceived ? (startDate.ToStartDateTime()).EstToUtc() : dt,
                    endDateRec = isReceived ? (endDate.ToEndDateTime()).EstToUtc() : dt,
                    startDateDue = isReceived ? dt : (startDate.ToStartDateTime()).EstToUtc(),
                    endDateDue = isReceived ? dt : (endDate.ToEndDateTime()).EstToUtc()
                }
                ).ToList();
            return data;
        }

        public List<MonthlyReconReportViewModel> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = "[dbo].[uspweb_MonthlyReconReport] @clientId, @startDate, @endDate, @channel";
            //string appCode = GetAppcodeByClientid(clientId);
            //if (appCode == "USAA")
            //{
            //    startDate = (startDate.Date + new TimeSpan(20, 00, 01)).EstToUtc();
            //    endDate = (endDate.Date + new TimeSpan(20, 00, 00)).EstToUtc();
            //}
            //else if (appCode == "HSBC" || appCode == "BOTW")
            //{
            //    startDate = (startDate + new TimeSpan(19, 30, 01)).EstToUtc();
            //    endDate = (endDate + new TimeSpan(19, 30, 00)).EstToUtc();
            //}
            //else
            //{
            //    startDate = startDate.ToStartDateTime().EstToUtc();
            //    endDate = endDate.ToEndDateTime().EstToUtc();
            //}
            startDate = startDate.ToStartDateTime();
            endDate = endDate.ToEndDateTime();
            var data = db.Query<MonthlyReconReportViewModel>(query,
                new
                {
                    clientId = clientId,
                    startDate = startDate,
                    endDate = endDate,
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    //shippingStatus = ShippingStatus.Shipped.ToString()
                }).ToList();
            return data != null ? data.OrderBy(d => d.ReceivedOn).ToList() : data;
        }

        public List<ChannelCodeTitle> GetChannelTitle(int clientId)
        {
            string query = "SELECT Code,Title FROM [Common].[SFTChannels] WHERE IsActive=1 AND Id in (select SFTChannelId from [Common].[ClientSFTChannels] WHERE ClientId=@clientId)";
            var channelList = db.Query<ChannelCodeTitle>(query, new { clientId = clientId }).ToList();
            return channelList;
        }

        public List<PrintVendor> GetActivePrintVendors()
        {
            return db.Query<PrintVendor>("SELECT * FROM Common.PrintVendors WHERE IsActive=1").ToList();
        }

        public List<PrintVendorLocations> GetActivePrintVendorsWithLocation()
        {
            return db.Query<PrintVendorLocations>("SELECT DISTINCT PrintVendorCode,PrintVendorName from Common.ShipperInformation SI INNER JOIN Common.PrintVendors PV ON SI.PrintVendorId = PV.Id WHERE PV.IsActive = 1 ORDER BY SI.PrintVendorName").ToList();
        }

        public List<PullBackRequests> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate)
        {
            string query = "SELECT * FROM [AM].[MailBatches] b,[AM].[MailPieces] m, [AM].[PullBackRequests] p LEFT OUTER JOIN [Profile].[Users] u ON p.[RequestedBy]=u.Id  WHERE p.[ClientId] = @clientId AND p.[MailBatchId]=b.Id AND p.MailPieceId = m.Id AND p.RequestedOn BETWEEN @startDate AND @endDate";
            var data = db.Query<MailBatches, MailPieces, PullBackRequests, Users, PullBackRequests>(query,
                (b, m, p, u) =>
                {
                    p.MailPieces = m;
                    m.MailBatches = b;
                    p.Users = u;
                    return p;
                },
                new
                {
                    clientId = clientId,
                    startDate = startDate.ToStartDateTime().EstToUtc(),
                    endDate = endDate.ToEndDateTime().EstToUtc()
                }).ToList();

            //return db.Query<MailPieces>(query, new { clientId = clientId, startDate = startDate.ToStartDateTime().EstToUtc(), endDate = endDate.ToEndDateTime().EstToUtc() }).ToList();
            return data;
        }

        public List<MailPieces> GetMailPiecesDataFromPrintBatchId(int printBatchId)
        {
            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId WHERE M.PrintBatchId=@PrintBatchId";

            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            var data = db.Query<MailPieces, MailBatches, DocTypes, PnSConfigDetail, MailPieceCustomInfoTest, MailPieces>(query,
                (m, b, d, p, c) =>
                {
                    MailPieces mp;

                    if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailBatches = b;
                        mp.DocTypes = d;
                        mp.PnSConfigDetails = p;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mailPieceDictionary.Add(mp.Id, mp);
                    }

                    mp.MailPieceCustomInfoTest.Add(c);
                    return mp;
                },
                new
                {
                    PrintBatchId = printBatchId
                }, splitOn: "Id,Id,MailPieceId").Distinct().ToList();
            return data;
        }

        public List<MailPieces> GetMailPiecesDataFromMailBatchId(int mailBatchId)
        {
            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE M.MailBatchId=@MailBatchId";

            var data = db.Query<MailPieces, MailBatches, DocTypes, PnSConfigDetail, MailPieceCustomInfoTest, Clients, MailPieces>(query,
                (m, b, d, p, c, cl) =>
                {
                    MailPieces mp;

                    if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailBatches = b;
                        mp.DocTypes = d;
                        mp.PnSConfigDetails = p;
                        mp.Clients = cl;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mailPieceDictionary.Add(mp.Id, mp);
                    }

                    mp.MailPieceCustomInfoTest.Add(c);
                    return mp;
                },
                new
                {
                    MailBatchId = mailBatchId
                }, splitOn: "Id,Id,MailPieceId,Id").Distinct().ToList();
            return data;
        }

        public MailPieces GetMailPiecesDataFromMailBatchIdAndUniqueId(int mailBatchId, Guid uniqueId)
        {
            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId INNER JOIN Common.Clients C ON M.ClientId=C.Id WHERE M.MailBatchId=@MailBatchId AND M.UniqueId=@UniqueId";

            var data = db.Query<MailPieces, MailBatches, DocTypes, PnSConfigDetail, MailPieceCustomInfoTest, Clients, MailPieces>(query,
                (m, b, d, p, c, cl) =>
                {
                    MailPieces mp;

                    if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailBatches = b;
                        mp.DocTypes = d;
                        mp.PnSConfigDetails = p;
                        mp.Clients = cl;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mailPieceDictionary.Add(mp.Id, mp);
                    }

                    mp.MailPieceCustomInfoTest.Add(c);
                    return mp;
                },
                new
                {
                    MailBatchId = mailBatchId,
                    UniqueId = uniqueId
                }, splitOn: "Id,Id,MailPieceId,Id").Distinct().ToList();
            return data.SingleOrDefault();
        }

        public List<PCISubmittedBatchDetail> GetPCIReconReport(DateTime startDate, DateTime endDate)
        {
            string query = @"SELECT PT.Id as PrintTrailBatchId,PT.[PrintBatchName],PT.SubmittedOn ,MIN(MP.DueDate) AS DueDate, pcd.ShippingCarrier
,pcd.ShippingServiceType,COUNT(*) AS PackageCount,SUM(REQ.TotalSheets) AS TotalPages 
FROM PnS.PrintTrailBatches PT 
INNER JOIN PnS.MRDFRequest REQ ON PT.Id=REQ.PrintTrailBatchId
INNER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
INNER JOIN Common.PrintTrails PTS ON MP.PrintTrailId=PTS.Id
left outer join [AM].[DocTypes] d ON mp.DocTypeId=d.Id 
left outer join Common.PnSConfigDetail pcd ON mp.PnSConfigDetailId = pcd.Id
WHERE PT.SubmittedOn >= @StartDate AND  PT.SubmittedOn < @EndDate AND MP.PrintLocation='Fort Mill, SC' 
AND REQ.MailPieceStatus NOT IN ('Error','PulledBack')
GROUP BY PT.Id,PT.PrintBatchName,PT.SubmittedOn,pcd.ShippingCarrier,pcd.ShippingServiceType
ORDER BY PrintTrailBatchId";
            return db.Query<PCISubmittedBatchDetail>(query, new { StartDate = startDate, EndDate = endDate }).ToList();
        }

        public List<DALLASSubmittedBatchDetail> GetDALLASReconReport(DateTime startDate, DateTime endDate)
        {
            string query = @"SELECT C.AppCode,pcd.ShippingCarrier,pcd.ShippingServiceType,MIN(P.DueDate) as DueDate,COUNT(*) AS PackageCount,SUM(P.TotalPages) AS TotalPages FROM AM.MailPieces P
inner join [AM].[DocTypes] d ON P.DocTypeId=d.Id 
inner join Common.PnSConfigDetail pcd ON P.PnSConfigDetailId = pcd.Id
INNER JOIN Common.Clients C ON P.ClientId=C.Id
WHERE P.ShippingStatus NOT IN ('Error','PulledBack') AND P.EffectiveReceivedOnEst >= @StartDate AND P.EffectiveReceivedOnEst < @EndDate
AND P.PrintLocation='Dallas CL'
GROUP BY C.AppCode,pcd.ShippingCarrier,pcd.ShippingServiceType";
            return db.Query<DALLASSubmittedBatchDetail>(query, new { StartDate = startDate, EndDate = endDate }).ToList();
        }

        public List<SupplyInventoryVM> SupplyInventoryReport(List<string> envelopes)
        {
            envelopes.RemoveAll(e => e == "");
            string query = @"SELECT E.Id, E.EnvelopeName,E.PartNumber,S.PrintLocation,S.PrintVendorCode, EI.ReceivedOn,EI.ReceivedQuantity,EI.UnitPrice,ES.CurrentQuantity
FROM  Common.Envelopes E
LEFT OUTER JOIN Common.EnvelopeStock ES ON E.Id=ES.EnvelopeId
LEFT OUTER JOIN Common.EnvelopeInventory EI ON EI.EnvelopeId=E.Id AND EI.ShipperId=ES.ShipperId
LEFT OUTER JOIN Common.ShipperInformation S ON ES.ShipperId=S.Id";
            DynamicParameters parameter = new DynamicParameters();
            if (envelopes != null && envelopes.Count > 0)
            {
                query = string.Format("{0} where e.Id in @envelopes", query);
                parameter.Add("@envelopes", envelopes);
            }
            return db.Query<SupplyInventoryVM>(query, parameter).ToList();
        }

        public MailBatches GetMailBatchDataFromMailBatchId(int mailBatchId)
        {
            string query = @"SELECT * FROM AM.MailBatches B INNER JOIN Common.SFTChannels S ON S.Id=B.SFTChannelId INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE B.Id=@MailBatchId";

            var data = db.Query<MailBatches, SFTChannels, Clients, MailBatches>(query,
                (mb, s, c) =>
                {

                    mb.SFTChannels = s;
                    mb.Clients = c;
                    return mb;
                },
                new
                {
                    MailBatchId = mailBatchId
                });
            return data.SingleOrDefault();
        }

        public List<ReportWizard> GetReportWizard(List<int> clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, List<string> shippingMethod, string trackingNo)
        {
            if (!startDate.HasValue && businessFriendlyIdValue.IsNullOrWhiteSpace() && trackingNo.IsNullOrWhiteSpace())
                throw new Exception("Date filter required.");

            List<Client> floodClients;
            List<Client> gosClients;
            using (IClientService clientSvc = new ClientService())
            {
                floodClients = clientSvc.GetClientsByAlias("Flood Integration");
                gosClients = clientSvc.GetClientsByAlias("GOS-Print Ready");
            }

            string extraColumns = string.Empty;
            string extraQuery = string.Empty;
            string columnList = string.Empty;

            if (floodClients.Select(x => x.Id).Intersect(clientId).Any())
            {
                extraColumns = " ,CST.[RequestorName], CST.[PropertyStreetAddress], CST.[PropertyCity], CST.[PropertyState], CST.[PropertyZip], CST.[LoanNumber] AS FloodLoan_Number, CST.[NoticeDate] AS [Flood_Determination_Date]";

                columnList = "[RequestorName],[PropertyStreetAddress],[PropertyCity],[PropertyState],[PropertyZip],[LoanNumber],[NoticeDate]";
            }


            if (gosClients.Select(x => x.Id).Intersect(clientId).Any())
            {
                extraColumns = extraColumns.IsNullOrWhiteSpace() ? " ,CST.[ProductCode]" : extraColumns + " ,CST.[ProductCode]";

                columnList = columnList.IsNullOrWhiteSpace() ? "[ProductCode]" : columnList + ",[ProductCode]";
            }

            if (!columnList.IsNullOrWhiteSpace())
            {
                extraQuery = $@"
                    LEFT OUTER JOIN 
                    (
	                    SELECT *
	                    FROM 
	                    (
		                    SELECT 
		                    [MailPieceId],[Key], [Value]
		                    FROM AM.MailPieceCustomInfo
	                    ) AS S
	                    PIVOT
	                    (
		                    MAX([Value])
		                    FOR [Key] IN ({columnList})
	                    )AS PVT
                    )CST on CST.MailPieceId=M.Id";
            }


            string query = $@"
                    SELECT M.Id as MailPieceId, M.BusinessFriendlyIdValue as LoanNumber, M.ShippingStatus as [Status], M.UniqueId,
                    M.RcptFirstName as FirstName, M.RcptLastName as LastName, M.RcptAddressCity as [City], M.RcptAddressZip as ZIP,
                    M.RcptAddressStreet, M.RcptAddressState as [State], M.ShippedOn as MailedOn,M.TrackingNo, M.DueDate, M.ErrorCode, M.ErrorDesc,
                    M.PrintLocation, M.PrintedOn ,B.BatchName as BatchId, B.ReceivedOn, M.DueDate, M.MissedSLAReason, M.DocTypeId, M.FileName,
                    D.DocTypeName as DocType, (PCD.ShippingCarrier + '-' + PCD.ShippingServiceType) as ShippingMethod, M.DocumentOrPackageType,
                    M.TotalPages as PagesShipped,M.TotalBlankPages as BlankPages, M.DeliveryStatus, M.DeliveryDate, M.Inserts, CH.Title as Channel,
                    C.AppCode as ClientCode, E.EnvelopeName, PTB.PrintBatchName as PrintFileName, M.ReturnDeliveryStatus, M.ReturnDeliveryDate,
                    M.ReturnTrackingNo {extraColumns}
                    FROM [AM].[MailPieces] M WITH (NOLOCK)
                    INNER JOIN [AM].[MailBatches] B WITH (NOLOCK) ON M.MailBatchId=B.Id
                    INNER JOIN [Common].[Clients] C WITH (NOLOCK) ON M.ClientId=C.Id
                    INNER JOIN [Common].[SFTChannels] CH WITH (NOLOCK) ON B.SFTChannelId=CH.Id
                    LEFT OUTER JOIN [Profile].[Users] U WITH (NOLOCK) ON M.PulledBackRequestedBy=U.Id
                    LEFT OUTER JOIN [AM].[DocTypes] D WITH (NOLOCK) ON M.DocTypeId=D.Id 
                    LEFT OUTER JOIN Common.PnSConfigDetail PCD WITH (NOLOCK) ON M.PnSConfigDetailId = PCD.Id
                    LEFT OUTER JOIN Common.Envelopes E WITH (NOLOCK) ON PCD.EnvelopeId=E.Id
                    LEFT OUTER JOIN PnS.MRDFRequest REQ WITH (NOLOCK) ON M.UniqueId=REQ.UniqueId
                    LEFT OUTER JOIN PnS.PrintTrailBatches PTB WITH (NOLOCK) on REQ.PrintTrailBatchId=PTB.Id
                    {extraQuery} WHERE 1=1";

            DynamicParameters parameter = new DynamicParameters();
            if (clientId != null && clientId.Count > 0)
            {
                query = string.Format("{0} and m.ClientId in @clientId", query);
                parameter.Add("@clientId", clientId);
            }
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdName) && !string.IsNullOrWhiteSpace(businessFriendlyIdValue))
            {
                query = string.Format("{0} and m.BusinessFriendlyIdName=@BusinessFriendlyIdName and m.businessFriendlyIdValue=@businessFriendlyIdValue", query);
                parameter.Add("@BusinessFriendlyIdName", businessFriendlyIdName);
                parameter.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            }
            if (!status.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} and m.ShippingStatus = @ShippingStatus", query);
                parameter.Add("@ShippingStatus", status);
            }
            if (!string.IsNullOrWhiteSpace(channel))
            {
                query = string.Format("{0} and ch.Code=@Code", query);
                parameter.Add("@Code", channel);
            }

            if (!string.IsNullOrWhiteSpace(trackingNo))
            {
                query = string.Format("{0} and m.TrackingNo=@TrackingNo", query);
                parameter.Add("@TrackingNo", trackingNo);
            }


            if (docType != null && docType.Count > 0)
            {
                query = string.Format("{0} and d.Id in @docType", query);
                parameter.Add("@docType", docType);
            }
            if (shippingMethod != null && shippingMethod.Count > 0)
            {
                List<string> shipMethod = new List<string>();
                List<string> shipCarrier = new List<string>();
                shippingMethod.ForEach(s => shipCarrier.Add(s.Split('-').First()));
                shippingMethod.ForEach(s => shipMethod.Add(s.Split('-')[1]));
                query = string.Format("{0} and pcd.ShippingServiceType in @shippingMethod and pcd.ShippingCarrier in @shippingCarrier", query);
                parameter.Add("@shippingMethod", shipMethod);
                parameter.Add("@shippingCarrier", shipCarrier);
            }
            if (parameter.ParameterNames.Count() == 0 && !startDate.HasValue && !endDate.HasValue)
                startDate = endDate = Common.EstToday;
            if (startDate.HasValue && endDate.HasValue)
            {
                query = string.Format("{0} and b.ReceivedOn BETWEEN @startDate and @endDate", query);
                parameter.Add("@startDate", startDate.Value.ToStartDateTime().EstToUtc());
                parameter.Add("@endDate", endDate.Value.ToEndDateTime().EstToUtc());
            }
            query = string.Format("{0} order by b.ReceivedOn desc", query);
            var data = db.Query<ReportWizard>(query, parameter).ToList();
            return data;
        }

        public List<AutoReportWizard> GetAutoReportWizard(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.ToStartDateTime().EstToUtc();
            endDate = endDate.ToStartDateTime().EstToUtc();
            
            string query = "[dbo].[usp_AutoReportWizard] @startDate, @endDate";

            var data = db.Query<AutoReportWizard>(query,new { startDate, endDate }).ToList();
            return data;
        }

        public List<TrackingReport> GetFedExData(int[] isPickedUp)
        {
            isPickedUp = isPickedUp ?? new int[0];
            //Possible values are for isPickedUp parameter is : 1-for today's pickedup records, 0-for today's not pickedup records and default -1 for previous not pickedup records

            string queryPreviousNotPickedUp = "SELECT *,'PreviousNotPickedUp' as PickUpStatus FROM [dbo].[view_FedExData] WHERE EffectiveReceivedOnEst < @todayEffectiveDate AND IsPickedUp=0";

            string queryTodayNotPickedUp = "SELECT *,'TodayNotPickedUp' as PickUpStatus FROM [dbo].[view_FedExData] WHERE EffectiveReceivedOnEst >= @todayEffectiveDate AND EffectiveReceivedOnEst < DATEADD(DAY,1,@todayEffectiveDate) AND IsPickedUp=0";

            string queryTodayPickedUp = "SELECT *,'TodayPickedUp' as PickUpStatus FROM [dbo].[view_FedExData] WHERE EffectiveReceivedOnEst >= @todayEffectiveDate AND EffectiveReceivedOnEst < DATEADD(DAY,1,@todayEffectiveDate) AND IsPickedUp=1";


            if (isPickedUp.Any(i => i == 0) && isPickedUp.Any(i => i == -1))
            {
                //This will provide previous and today's not picked up records
                var data = db.Query<TrackingReport>(queryPreviousNotPickedUp + " UNION " + queryTodayNotPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
            else if (isPickedUp.Any(i => i == 0) && isPickedUp.Any(i => i == 1))
            {
                //This will provide today's not picked up and picked up records
                var data = db.Query<TrackingReport>(queryTodayNotPickedUp + " UNION " + queryTodayPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
            else if (isPickedUp.Any(i => i == 0))
            {
                //This will provide today's not picked up records
                var data = db.Query<TrackingReport>(queryTodayNotPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
            else if (isPickedUp.Any(i => i == -1))
            {
                //This will provide today's not picked up records
                var data = db.Query<TrackingReport>(queryPreviousNotPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
            else
            {
                //This will provide today's picked up records
                var data = db.Query<TrackingReport>(queryTodayPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
        }

        public string GetAppcodeByClientid(int clientId)
        {
            return db.QuerySingle<string>("select AppCode from Common.Clients where Id=@clientId", new { clientId = clientId });
        }

        public List<string> GetMonthlyReconDate(int clientId)
        {
            string appCode = GetAppcodeByClientid(clientId);
            //string startDate = appCode == "USAA" || appCode == "HSBC" || appCode == "BOTW" ? "25" : "26";
            string startDate = "26";
            var dateList = new List<string>();
            var currDate = DateTime.UtcNow.UtcToEst();
            if (currDate.Day <= 25) currDate = currDate.AddMonths(-1);
            for (var dt = currDate.Date; dt.Date >= currDate.AddMonths(-6).Date; dt = dt.AddMonths(-1).Date)
            {
                dateList.Add(string.Format("{0}/{1}/{2} - {3}/25/{4}", dt.Month, startDate, dt.Year, dt.AddMonths(1).Month, dt.AddMonths(1).Year));
            }
            return dateList;
        }

        public List<GOSMailPieceData> GetGOSDailyReconReport(DateTime currentDate)
        {
            string query = @"SELECT C.AppCode,B.BatchName,dbo.ufn_UTCToEST(B.ReceivedOn)AS ReceivedOnEst,P.BusinessfriendlyIdValue AS LoanNumber,P.DocumentOrPackageType,P.FileName,P.TrackingNo,P.ShippingStatus,P.TotalPages ,dbo.ufn_UTCToEST(ShippedOn) AS ShippedOnEst, P.PrintLocation, P.ErrorDesc FROM AM.MailPieces P INNER JOIN AM.MailBatches B ON P.MailBatchId=B.Id INNER JOIN Common.Clients C ON P.ClientId=C.Id WHERE B.ClientId IN (SELECT Id FROM Common.Clients WHERE Alias='GOS') AND CAST(P.EffectiveReceivedOnEst AS DATE)=@EffectiveReceivedOn";
            return db.Query<GOSMailPieceData>(query, new { EffectiveReceivedOn = currentDate }).ToList();
        }

        public List<GOSMailPieceData> GetGOSDataPointReport(DateTime startDate, DateTime endDate, List<string> clientCodes = null)
        {
            var filterByAppCode = string.Empty;

            if (clientCodes != null)
            {
                filterByAppCode = " AND AppCode IN @AppCodes";
            }

            string query = $@"With cte AS
                            (
	                            SELECT P.Id, C.AppCode, B.BatchName, dbo.ufn_UTCToEST(B.ReceivedOn) AS ReceivedOnEst, P.BusinessfriendlyIdValue AS LoanNumber,
	                            P.DocumentOrPackageType,P.FileName, P.RcptFirstName +' '+ P.RcptLastName AS BorrowerName,P.TrackingNo,P.ShippingStatus,P.TotalPages,  dbo.ufn_UTCToEST(ShippedOn) AS ShippedOnEst,
	                            P.PrintLocation, P.ErrorDesc, P.DeliveryDate, P.DeliveryStatus, P.ReturnTrackingNo, P.ReturnDeliveryStatus, P.ReturnDeliveryDate, PCD.ShippingCarrier, PCD.ShippingServiceType 
	                            FROM AM.MailPieces P 
	                            INNER JOIN AM.MailBatches B ON P.MailBatchId=B.Id 
	                            INNER JOIN Common.Clients C ON P.ClientId=C.Id
                                INNER JOIN Common.PnSConfigDetail PCD ON P.PnSConfigDetailId=PCD.Id
	                            WHERE B.ClientId IN (SELECT Id FROM Common.Clients WHERE Alias='GOS' {filterByAppCode}) 
	                            AND CAST(P.EffectiveReceivedOnEst AS DATE) BETWEEN @StartDate AND @EndDate
                            ), cte2 AS
                            (
	                            SELECT *
	                            FROM 
	                            (
		                            SELECT 
			                            [MailPieceId],[Key], [Value]
		                            FROM AM.MailPieceCustomInfo MPC
		                            WHERE MailPieceId IN 
		                            (
			                            SELECT Id FROM AM.MailPieces 
			                            WHERE ClientId IN (SELECT Id FROM Common.Clients WHERE Alias='GOS' {filterByAppCode})
			                            AND CAST(EffectiveReceivedOnEst AS DATE) BETWEEN @StartDate AND @EndDate
		                            )
		                            AND [Key]='ProductCode'
	                            ) AS s
	                            PIVOT
	                            (
		                            MAX([Value])
		                            FOR [Key] IN ([ProductCode])
	                            )AS pvt
                            )
                            SELECT * 
                            FROM cte
                            LEFT OUTER JOIN cte2 ON cte.Id=cte2.MailPieceId";
            return db.Query<GOSMailPieceData>(query, new { StartDate = startDate, EndDate = endDate, AppCodes = clientCodes }).ToList();
        }

        public List<TrackingReport> GetFedExDataForTracking(DateTime startDueDate, DateTime endDueDate, string alias = null, List<string> clientCodes = null)
        {
            string query = "SELECT * FROM [dbo].[view_FedExData] WHERE DueDate BETWEEN @startDueDate AND @endDueDate AND ((DeliveryStatus NOT LIKE '%delivered%' OR DeliveryStatus NOT LIKE '%Delivered%' OR DeliveryStatus IS NULL) OR (ReturnDeliveryStatus NOT LIKE '%delivered%' OR ReturnDeliveryStatus NOT LIKE '%Delivered%' OR ReturnDeliveryStatus IS NULL))";

            if (!alias.IsNullOrWhiteSpace())
            {
                query += $" AND ClientId IN (SELECT Id FROM Common.Clients WHERE Alias='{alias}')";
            }

            if (clientCodes != null)
            {
                query += $" AND ClientId IN (SELECT Id FROM Common.Clients WHERE AppCode IN @AppCodes)";
            }

            //This will provide previous and today's not picked up records
            var data = db.Query<TrackingReport>(query, new
            {
                startDueDate = startDueDate.EstToUtc(),
                endDueDate = endDueDate.EstToUtc(),
                AppCodes = clientCodes
            }).ToList();
            return data;
        }

        public List<MailPieces> GetMailPiecesDataBetweenDates(DateTime startDate, DateTime endDate, int clientId)
        {
            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
LEFT OUTER JOIN Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE B.ReceivedOn BETWEEN @startDate AND @endDate AND B.ClientId=@clientId AND M.ShippingStatus=@ShippingStatus";

            var data = db.Query<MailPieces, MailBatches, DocTypes, PnSConfigDetail, MailPieceCustomInfoTest, Clients, MailPieces>(query,
                (m, b, d, p, c, cl) =>
                {
                    MailPieces mp;

                    if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailBatches = b;
                        mp.DocTypes = d;
                        mp.PnSConfigDetails = p;
                        mp.Clients = cl;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mailPieceDictionary.Add(mp.Id, mp);
                    }

                    mp.MailPieceCustomInfoTest.Add(c);
                    return mp;
                },
                new
                {
                    startDate,
                    endDate,
                    clientId,
                    ShippingStatus = ShippingStatus.Error.ToString()
                }, splitOn: "Id,Id,MailPieceId,Id").Distinct().ToList();
            return data;
        }

        public MemoryStream USAAMTDVolumeReport(DateTime startDate, DateTime endDate)
        {

            var dataReader = db.ExecuteReader("dbo.SP_USAAMTDVolumeReport @startDate, @endDate", new { startDate = startDate, endDate = endDate });
            MemoryStream memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                var hasHeaderBeenWritten = false;
                while (dataReader.Read())
                {
                    if (!hasHeaderBeenWritten)
                    {
                        for (var i = 0; i < dataReader.FieldCount; i++)
                        {
                            csv.WriteField(dataReader.GetName(i));
                        }
                        csv.NextRecord();
                        hasHeaderBeenWritten = true;
                    }

                    for (var i = 0; i < dataReader.FieldCount; i++)
                    {
                        csv.WriteField(dataReader[i]);
                    }
                    csv.NextRecord();
                }
            }
            return memoryStream;



        }

        public List<TrackingReport> GetUSPSeVSDataForTracking(DateTime startDueDate, DateTime endDueDate, string alias = null, List<string> clientCodes = null)
        {
            string query = "SELECT * FROM [dbo].[view_USPSData] WHERE DueDate BETWEEN @startDueDate AND @endDueDate AND ((DeliveryStatus NOT LIKE '%delivered%' OR DeliveryStatus NOT LIKE '%Delivered%' OR DeliveryStatus IS NULL) OR (ReturnDeliveryStatus NOT LIKE '%delivered%' OR ReturnDeliveryStatus NOT LIKE '%Delivered%' OR ReturnDeliveryStatus IS NULL))";

            if (!alias.IsNullOrWhiteSpace())
            {
                query += $" AND ClientId IN (SELECT Id FROM Common.Clients WHERE Alias='{alias}')";
            }

            if (clientCodes != null)
            {
                query += $" AND ClientId IN (SELECT Id FROM Common.Clients WHERE AppCode IN @AppCodes)";
            }

            //This will provide previous and today's not picked up records
            var data = db.Query<TrackingReport>(query, new
            {
                startDueDate = startDueDate.EstToUtc(),
                endDueDate = endDueDate.EstToUtc(),
                AppCodes = clientCodes
            }).ToList();
            return data;
        }

        public List<MailPieces> GetMailPiecesDataFromPrintBatchId(List<int> printBatchIds)
        {
            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId WHERE M.PrintBatchId IN @PrintBatchId";

            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            var data = db.Query<MailPieces, MailBatches, DocTypes, PnSConfigDetail, MailPieceCustomInfoTest, MailPieces>(query,
                (m, b, d, p, c) =>
                {
                    MailPieces mp;

                    if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailBatches = b;
                        mp.DocTypes = d;
                        mp.PnSConfigDetails = p;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mailPieceDictionary.Add(mp.Id, mp);
                    }

                    mp.MailPieceCustomInfoTest.Add(c);
                    return mp;
                },
                new
                {
                    PrintBatchId = printBatchIds
                }, splitOn: "Id,Id,MailPieceId").Distinct().ToList();
            return data;
        }

        public List<MailPieces> GetMailPiecesDataFromMailPieceId(List<int> mpIds)
        {
            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId WHERE M.Id IN @MPIds";

            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            var data = db.Query<MailPieces, MailBatches, DocTypes, PnSConfigDetail, MailPieceCustomInfoTest, MailPieces>(query,
                (m, b, d, p, c) =>
                {
                    MailPieces mp;

                    if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailBatches = b;
                        mp.DocTypes = d;
                        mp.PnSConfigDetails = p;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mailPieceDictionary.Add(mp.Id, mp);
                    }

                    mp.MailPieceCustomInfoTest.Add(c);
                    return mp;
                },
                new
                {
                    MPIds = mpIds
                }, splitOn: "Id,Id,MailPieceId").Distinct().ToList();
            return data;
        }

        public List<MonthlyReconReportViewModel> GetPVReconReport(string pvCode, DateTime startDate, DateTime endDate, List<int> clientIdlst, string status, List<string> shippingMethod)
        {
            string query = $@"
                    SELECT C.AppCode, B.BatchName AS BatchName, P.UniqueId,P.BusinessFriendlyIdValue AS LoanNumber
                    ,P.RcptFirstName AS [FirstName],P.RcptLastName AS [LastName]
                    ,P.RcptAddressStreet AS [Address],P.RcptAddressCity AS [City],P.RcptAddressState AS [State],P.RcptAddressZIP AS [ZIP]
                    ,S.Code AS Channel ,P.DocumentOrPackageType AS [DocType]
                    ,B.ReceivedOn, P.ShippedOn, P.PulledBackOn, P.PrintLocation
                    ,P.ShippingStatus AS [Status], P.TrackingNo, P.[FileName]
                    ,P.TotalPages, P.TotalSheets,ISNULL(MPP.LETTER,0) AS [LetterPageCount],ISNULL(MPP.LEGAL,0) AS [LegalPageCount], P.TotalBlankPages
                    ,P.IsEDeliveryRequested, IIF(PCD.ShippingCarrier = 'E-Delivery', PCD.ShippingCarrier, PCD.ShippingCarrier + ' / ' +PCD.ShippingServiceType ) AS [ShippingMethod]
                    ,ISNULL(REPLACE(REPLACE(P.ErrorDesc, CHAR(13), ''), CHAR(10), ''),'') AS ErrorDesc
                    ,P.Inserts,PT.PrintBatchName, PT.SubmittedOn, P.DeliveryStatus, P.DeliveryDate, P.ShippingCost, E.EnvelopeName
                    FROM AM.MailPieces P
                    INNER JOIN AM.DocTypes D ON P.DocTypeId=D.Id 
                    INNER JOIN Common.PnSConfigDetail PCD ON P.PnSConfigDetailId=PCD.Id
                    INNER JOIN Common.Clients C ON P.ClientId=C.Id
                    INNER JOIN AM.MailBatches B ON P.MailBatchId=B.Id
                    INNER JOIN Common.SFTChannels S ON B.SFTChannelId=S.Id
                    INNER JOIN PnS.MRDFRequest REQ ON P.UniqueId=REQ.UniqueId
                    INNER JOIN PnS.PrintTrailBatches PT ON REQ.PrintTrailBatchId=PT.Id
                    INNER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
                    INNER JOIN Common.ShipperInformation SI ON P.ShipperId=SI.Id
                    LEFT OUTER JOIN 
                    (
	                    SELECT *
	                    FROM 
	                    (
		                    SELECT 
		                    [MailPieceId],[PageSize],[PageCount]
		                    FROM [Common].[MailPiecePageInfo]
	                    ) AS s
	                    PIVOT
	                    (
		                    MAX([PageCount])
		                    FOR [PageSize] IN ([LETTER],[LEGAL])
	                    )AS pvt2
                    )MPP on MPP.MailPieceId=P.Id
                    WHERE PT.SubmittedOn BETWEEN @startDate AND @endDate ";

            DynamicParameters parameter = new DynamicParameters();
            parameter.Add("@startDate", startDate);
            parameter.Add("@endDate", endDate);

            if (clientIdlst != null && clientIdlst.Count > 0)
            {
                query = string.Format("{0} and P.ClientId in @clientId", query);
                parameter.Add("@clientId", clientIdlst);
            }
            if (!status.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} and P.ShippingStatus = @ShippingStatus", query);
                parameter.Add("@ShippingStatus", status);
            }

            if (!pvCode.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} and SI.PrintVendorCode = @PVCode", query);
                parameter.Add("@PVCode", pvCode);
            }

            if (shippingMethod != null && shippingMethod.Count > 0)
            {
                List<string> shipMethod = new List<string>();
                List<string> shipCarrier = new List<string>();
                shippingMethod.ForEach(s => shipCarrier.Add(s.Split('-').First()));
                shippingMethod.ForEach(s => shipMethod.Add(s.Split('-')[1]));
                query = string.Format("{0} and PCD.ShippingServiceType IN @shippingMethod AND PCD.ShippingCarrier IN @shippingCarrier", query);
                parameter.Add("@shippingMethod", shipMethod);
                parameter.Add("@shippingCarrier", shipCarrier);
            }

            query = string.Format("{0} ORDER BY PT.SubmittedOn", query);
            var data = db.Query<MonthlyReconReportViewModel>(query, parameter).ToList();
            return data;
        }

        public List<EnvelopeInventoryUsage> GetEnvelopesUsageReport(int clientId, int? envelopeId, string printVendorCode, DateTime startDate, DateTime endDate)
        {
            string query = @"SELECT E.EnvelopeName,SI.PrintLocation,SI.PrintVendorName,CAST(PTB.SubmittedOn AS DATE) AS SubmittedOn,SUM(EU.Quantity) AS TotalQuantityUsed,SI.PrintVendorCode,
                ES.CurrentQuantity,ES.ReOrderDuration,E.Id as EnvelopeId
                FROM PnS.EnvelopeUsage EU
                INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
                INNER JOIN Common.ShipperInformation SI ON PTB.ShipperId=SI.Id
                INNER JOIN Common.Envelopes E ON EU.EnvelopeId=E.Id
                INNER JOIN Common.EnvelopeStock ES ON ES.EnvelopeId=E.Id AND ES.ShipperId=SI.Id
                WHERE CAST(PTB.SubmittedOn AS DATE) between @StartDate AND @EndDate AND SI.PrintVendorCode = ISNULL(@PrintVendorCode,SI.PrintVendorCode) 
                AND E.Id=ISNULL(@EnvelopeId,E.Id)
                GROUP BY E.EnvelopeName,SI.PrintLocation,SI.PrintVendorName,CAST(PTB.SubmittedOn AS DATE),ES.CurrentQuantity,ES.ReOrderDuration,SI.PrintVendorCode,E.Id
                ORDER BY CAST(PTB.SubmittedOn AS DATE),E.EnvelopeName";
            return db.Query<EnvelopeInventoryUsage>(query, new { EnvelopeId = envelopeId, PrintVendorCode = printVendorCode, StartDate = startDate, EndDate = endDate }).ToList();
        }

        public List<MonthlyReconReportViewModel> GetDuplicateReportByDate(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = @"SELECT P.Id,P.UniqueId,P.MailBatchId,B.BatchName,B.ReceivedOn,X.BusinessFriendlyIdValue AS LoanNumber
                            , X.DocumentOrPackageType AS DocType, X.[FileName],X.RcptAddressCity AS City,X.RcptAddressStreet AS Address
                            , X.RcptAddressState AS State, X.RcptAddressZIP AS Zip,X.RcptFirstName AS FirstName,X.RcptLastName AS LastName
                            , S.Code AS Channel, PCD.ShippingCarrier, PCD.ShippingServiceType, P.ShippingStatus AS [Status], P.ShippedOn
                            FROM AM.MailPieces P
                            INNER JOIN AM.MailBatches B ON P.MailBatchId = B.Id
                            INNER JOIN Common.SFTChannels S ON B.SFTChannelId=S.Id
                            INNER JOIN AM.DocTypes D ON D.Id = P.DocTypeId
                            INNER JOIN Common.PnSConfigDetail PCD ON P.PnSConfigDetailId = PCD.Id
                            INNER JOIN
                            (
                                SELECT RcptFirstName, RcptLastName, RcptAddressStreet, RcptAddressState, RcptAddressCity, RcptAddressZIP, BusinessFriendlyIdValue, [FileName], P.DocumentOrPackageType, COUNT(*) AS  TotalRecords
                                FROM AM.MailPieces P
                                INNER JOIN AM.MailBatches B ON P.MailBatchId= B.Id
                                WHERE P.EffectiveReceivedOnEst BETWEEN @startDate AND @endDate AND P.ClientId = @clientId AND P.ShippingStatus NOT IN ('Error')
                                GROUP BY P.RcptFirstName,P.RcptLastName,P.RcptAddressStreet,P.RcptAddressState,P.RcptAddressCity,P.RcptAddressZIP,P.BusinessFriendlyIdValue,P.[FileName],P.DocumentOrPackageType
                                HAVING COUNT(*) > 1
                            )X ON X.RcptFirstName = P.RcptFirstName AND X.RcptLastName = P.RcptLastName AND X.BusinessFriendlyIdValue = P.BusinessFriendlyIdValue
                            AND X.DocumentOrPackageType = P.DocumentOrPackageType AND X.FileName = P.FileName AND X.RcptAddressCity = P.RcptAddressCity
                            AND X.RcptAddressStreet = P.RcptAddressStreet AND X.RcptAddressState=P.RcptAddressState AND X.RcptAddressZIP = P.RcptAddressZIP
                            WHERE P.EffectiveReceivedOnEst BETWEEN @startDate AND @endDate AND P.ClientId = @clientId AND P.ShippingStatus NOT IN ('Error')
                            ORDER BY P.RcptFirstName,P.RcptLastName,P.RcptAddressStreet,P.RcptAddressState,P.RcptAddressCity,P.RcptAddressZIP,P.BusinessFriendlyIdValue,P.[FileName],P.DocumentOrPackageType";
            var data = db.Query<MonthlyReconReportViewModel>(query,
                new
                {
                    clientId = clientId,
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    startDate = (startDate.AddDays(-5).ToStartDateTime()).EstToUtc(),
                    endDate = (endDate.ToEndDateTime()).EstToUtc()
                }
                ).ToList();
            return data;
        }

        public List<AMPostageData> GetAMPostageData(DateTime startDate, DateTime endDate)
        {
            string query = @"SELECT C.AppCode, U.UserId,U.APIRequestedOn,U.BarcodeNumber,U.Postage,US.UserName 
                             FROM API.USPSeVS U
                             INNER JOIN Profile.Users US ON U.UserId=US.Id
                             LEFT OUTER JOIN AM.MailPieces P ON SUBSTRING(U.BarcodeNumber,9,LEN(U.BarcodeNumber)-8) = P.TrackingNo
							 LEFT OUTER JOIN Common.Clients C ON P.ClientId=C.Id
                             WHERE U.APIRequestedOn BETWEEN @StartDate AND @EndDate
                             AND (U.IsAPIError = 0 AND U.IsUSPSError = 0)
                             ORDER BY U.APIRequestedOn";
            return db.Query<AMPostageData>(query, new { StartDate = startDate.ToStartDateTime().EstToUtc(), EndDate = endDate.ToEndDateTime().EstToUtc() }).ToList();
        }
        public void UpdateTrackingInformation()
        {
            db.Execute("usp_UpdateTrackingInfo");
            return;
        }

        public List<ReshipMailPieceViewModel> GetReshipRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, int? docType, DateTime startDate, DateTime endDate, int clientId)
        {
            string query = @"select m.BusinessFriendlyIdValue as BusinessIdValue,m.UniqueId,m.FileName,(m.RcptFirstName+ ' ' +m.RcptLastName) as Name,
                            m.RcptAddressStreet as Address,m.TotalPages,m.TotalBlankPages,m.IsPullBackRequested,m.ShippingStatus as Status,m.DueDate,m.TrackingNo,m.ErrorCode,m.ErrorDesc,IsEDeliveryRequested,IsESignRequested,
                            b.ReceivedOn,
                            d.DocTypeName as DocType,
                            c.AppCode,c.RetentionPeriod,c.Id AS ClientId, m.MailBatchId,m.Id AS MailPieceId
                            from AM.MailPieces m 
                            inner join AM.MailBatches b on m.MailBatchId=b.Id
                            inner join Common.Clients c on m.ClientId=c.Id
                            LEFT OUTER JOIN [Profile].[Users] u on m.[PulledBackRequestedBy]=u.Id 
                            left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id 
                            left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id
                            where ((m.MailBatchId=b.Id and m.ClientId=c.Id) and b.ClientId=@clientId) ";

            DynamicParameters parameter = new DynamicParameters();
            parameter.Add("@clientId", clientId);
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdName))
            {
                query = string.Format("{0} and m.BusinessFriendlyIdName=@BusinessFriendlyIdName", query);
                parameter.Add("@BusinessFriendlyIdName", businessFriendlyIdName);
            }
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdValue))
            {
                query = string.Format("{0} and m.businessFriendlyIdValue=@businessFriendlyIdValue", query);
                parameter.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            }
            if (docType.HasValue)
            {
                query = string.Format("{0} and d.Id = @docTypeId", query);
                parameter.Add("@docTypeId", docType);
            }
            query = string.Format("{0} and b.ReceivedOn BETWEEN @startDate and @endDate order by b.ReceivedOn desc", query);
            parameter.Add("@startDate", startDate.ToStartDateTime().EstToUtc());
            parameter.Add("@endDate", endDate.ToEndDateTime().EstToUtc());
            var data = db.Query<ReshipMailPieceViewModel>(query, parameter).ToList();
            return data;
        }
    }
}
