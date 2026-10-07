using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Lib;
using AM.Entities;

namespace AM.Services
{
    public interface ICommonService : IDisposable
    {
        Task<int> UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp);
        int UpdateEDeliveryPipelineStatus(int batchId, string pipeLineStatus, DateTime timestamp);
        Task<int> UpdateBatchStatus(int batchId, string batchStatus);
        int UpdateBatchValidationStatus(int batchId, string validationStatus, string validationErrors);
        int AMLogBatchTimeLine(int batchId, string activityName, DateTime timeStamp);
        int UpdatePrintBatchStatus(int printBatchId, string batchStatus, string statusDescription = null);
        int UpdatePrintBatchPipeLineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp);
        ClientBusinessIdentifier GetBusinessIdNameFromClientId(int clientId);
        ClientBusinessIdentifier CheckBusinessIdNameAlreadyExist(int clientId, string businessIdName);
        int InsertBusinessIdentifierName(int clientId, string businessIdName);
        List<Deliverable> GetDeliverablesByClientId(int clientId);
        Deliverable GetDeliverableByReportType(int clientId, string reportType);
        Dictionary<int, List<Holiday>> GetHolidayList();
        Dictionary<int, int> GetClientHolidayGroup();
        List<Holiday> GetFederalHolidayList();
        string GetSystemUserId();
        List<DallasBatch> GetDallasBatches(DateTime estToday);
        DateTime GetStandardSubmissionTime(string appCode, string vendorCode, DateTime receivedOn, List<Sweep> sweepTimings);
        List<Sweep> GetSweepTimings();
        List<PnSConfigDetailPrintTrailMap> GetTrailMapDetailsByPnSConfigDetailId(int pnsConfigDetailId);
        PrintVendor GetPrintVendorDetailsByPrintTrailId(int printTrailId, DateTime? effectiveReceivedOnEst = null);
        DateTime GetMaximumSubmissionTime(DocType docType, DateTime effectiveReceivedOnEst);
        int GetPrintTrail(int pnsConfigDetailId, DateTime estEffectiveReceivedOn);
        DateTime GetDueDate(int clientId, DocType docType, DateTime estEffectiveReceivedOn);
        DateTime GetEffectiveEstReceivedOn(int clientId, DocType docType, DateTime utcReceivedOn);
        bool IsBusinessDay(DateTime dueDate, List<Holiday> holidays, bool isSatWorkDay, bool isSunWorkDay);
        DateTime GetNextWorkingDay(DateTime estDateTime, List<Holiday> HolidayList, bool isSatWorkDay, bool isSunWorkDay);
        int InsertErrorNotifications(int mailBatchId);
        string GetErrorNotificationEmailSubscriberList(int clientId);
        void GetUpdatedDueDateAndPrintLocation(DateTime receivedOn, MailPieceDTO mailPiece, DocType docType, Client client);
        int GetReportingSequence();
        bool CheckForDuplicateMailPieceUniqueId(Guid mpUniqueId);
        List<DallasNotificationMap> GetDallasNotificationMaps();
        CustomTemplate GetCustomTemplateByDocTypeId(int docTypeId, string communityCode);
        CustomTemplate GetCustomTemplateByCommunityCode(string communityCode);
        ShipperInformation GetShipperInformation(int shipperId);
        ShipperInformation GetShipperInformationByPrintLocation(string printLocation, int printTrailId);
        Task<int> UpdateBatchPipeLineStatusByMailPieceId(Guid mailPieceId, string pipeLineStatus, DateTime timestamp);
        string GetEnvelopeCodeByEnvelopeId(int envelopeId);
        List<string> GetInsertDataByDocTypeId(int docTypeId);

        int GetDallasBatchCounts(int? mpId = 0);
        List<PackagesHoldInfo> GetPackagesHoldInfo();
        List<USPSLabelCutOffDetail> GetUSPSLabelCutOffDetails();
        List<USPSCredentialInfo> GetUSPSCredentialDetails();

        Dictionary<string, string> GetCustomTemplates();
    }
    public class CommonService : ServiceBase, ICommonService //It implements Interfaces 
    {

        public async Task<int> UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp)
        {
            string query;
            if (pipeLineStatus == BatchPipelineStatus.SubmittedToEDelivery)//System will move pipelinestatus to SubmittedToEDelivery to exclude it from regular print and ship process.
            {
                query = "UPDATE [AM].[MailBatches] SET PipelineStatus = @pipelineStatus, ValidatedOn = @timestamp, AcknowledgedOn = @timestamp, SubmittedToEDeliveryOn = @timestamp  WHERE Id = @batchId";
            }
            else
                query = $"UPDATE [AM].[MailBatches] SET PipelineStatus = @pipelineStatus, {pipeLineStatus}On = @timestamp WHERE Id = @batchId";

            return await db.ExecuteAsync(query, new { pipeLineStatus = pipeLineStatus, timestamp = timestamp, batchId = batchId });
        }

        public int UpdateEDeliveryPipelineStatus(int batchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = @pipelineStatus, {0}On = @timestamp WHERE MailBatchId = @batchId".Fill(pipeLineStatus);

            return db.Execute(query, new { pipeLineStatus = pipeLineStatus, timestamp = timestamp, batchId = batchId });
        }

        public async Task<int> UpdateBatchStatus(int batchId, string batchStatus)
        {
            string query = "UPDATE [AM].[MailBatches] SET BatchStatus = @batchStatus WHERE Id = @batchId";

            return await db.ExecuteAsync(query, new { batchStatus = batchStatus, batchId = batchId });
        }

        public int UpdateBatchValidationStatus(int batchId, string validationStatus, string validationErrors)
        {
            string query = "UPDATE [AM].[MailBatches] SET ValidationStatus = @validationStatus, ValidationErrors=@validationErrors WHERE Id = @batchId";

            return db.Execute(query, new { batchId = batchId, validationStatus = validationStatus, validationErrors = validationErrors });
        }

        public int AMLogBatchTimeLine(int batchId, string activityName, DateTime timeStamp)
        {
            return db.Execute("usp_AMLogBatchTimeLine", new { activityName = activityName, batchId = batchId, timeStamp = timeStamp }, commandType: CommandType.StoredProcedure);
        }

        public int UpdatePrintBatchStatus(int printBatchId, string batchStatus, string statusDescription = null)
        {
            string query = "UPDATE [AM].[PrintBatches] SET BatchStatus = @batchStatus,StatusDescription=@statusDescription WHERE Id = @printBatchId";

            return db.Execute(query, new { batchStatus = batchStatus, printBatchId = printBatchId, statusDescription = statusDescription });
        }

        public int UpdatePrintBatchPipeLineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[PrintBatches] SET {0}On = @timestamp WHERE Id = @printBatchId".Fill(pipeLineStatus);

            return db.Execute(query, new { timestamp = timestamp, printBatchId = printBatchId });
        }

        public ClientBusinessIdentifier GetBusinessIdNameFromClientId(int clientId)
        {
            string query = "SELECT * FROM [AM].[ClientBusinessIdentifiers] WHERE ClientId= @clientId ";

            return db.QuerySingleOrDefault<ClientBusinessIdentifier>(query, new { clientId = clientId });
        }

        public ClientBusinessIdentifier CheckBusinessIdNameAlreadyExist(int clientId, string businessIdName)
        {
            string query = "SELECT * FROM [AM].[ClientBusinessIdentifiers] WHERE ClientId= @clientId AND [BusinessIdName]=@BusinessIdName";

            return db.QuerySingleOrDefault<ClientBusinessIdentifier>(query, new { clientId = clientId, BusinessIdName = businessIdName });
        }

        public int InsertBusinessIdentifierName(int clientId, string businessIdName)
        {
            string query = "INSERT INTO [AM].[ClientBusinessIdentifiers] ( [ClientId], [BusinessIdName] ) VALUES (@ClientId, @BusinessIdName)";

            return db.Execute(query, new { ClientId = clientId, BusinessIdName = businessIdName });
        }

        public List<Deliverable> GetDeliverablesByClientId(int clientId)
        {

            string query = "[dbo].[usp_GetDeliverableByReportType] @ClientId, @ReportType";

            var data = db.Query<Deliverable, EmailChannel, Deliverable>(query,
               (d, e) =>
               {
                   d.EmailChannel = e;
                   return d;
               },
               new
               {
                   ClientId = clientId,
                   ReportType = string.Empty
               }
               );
            return data.ToList();
        }

        public Deliverable GetDeliverableByReportType(int clientId, string reportType)
        {
            string query = "[dbo].[usp_GetDeliverableByReportType] @ClientId, @ReportType";

            var data = db.Query<Deliverable, EmailChannel, Deliverable>(query,
               (d, e) =>
               {
                   d.EmailChannel = e;
                   return d;
               },
               new
               {
                   ClientId = clientId,
                   ReportType = reportType
               }
               );
            return data.SingleOrDefault();
        }

        public Dictionary<int, List<Holiday>> GetHolidayList()
        {
            string query = "SELECT * FROM [Common].[Holidays]";

            return db.Query<Holiday>(query).GroupBy(h => h.GroupId).ToDictionary(g => g.Key, g => g.ToList());
        }

        public Dictionary<int, int> GetClientHolidayGroup()
        {
            string query = "SELECT * FROM [Common].[ClientHolidayGroups]";

            return db.Query<ClientHolidayGroup>(query).ToDictionary(c => c.ClientId, g => g.HolidayGroupId);
        }

        public List<Holiday> GetFederalHolidayList()
        {
            string query = "SELECT * FROM [Common].[FederalHolidays]";

            return db.Query<Holiday>(query, null).ToList();
        }

        public string GetSystemUserId()
        {
            string q = "select Id from [Profile].[Users] where email = @email";
            return db.QuerySingle<string>(q, new { email = "system@milestonetechnoconsulting.com" });
        }

        public List<DallasBatch> GetDallasBatches(DateTime estToday)
        {
            string query = "Select * from [dbo].[ufn_GetDallasBatches](@EstToday)";

            return db.Query<DallasBatch>(query, new { EstToday = estToday }).ToList();
        }

        public DateTime GetStandardSubmissionTime(string appCode, string vendorCode, DateTime receivedOn, List<Sweep> sweepTimings)
        {
            var revisedReceivedOnEst = receivedOn.UtcToEst();
            var weekDayType = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Saturday ? "Saturday" : "Weekday";
            var continuousSweeps = sweepTimings.Where(s => s.AppCode == appCode && s.VendorCode == vendorCode && s.WeekDayType == weekDayType && s.SweepType == "Continuous").ToList();


            if (continuousSweeps.Count > 0)
                return receivedOn;

            var bundlingSweeps = sweepTimings.Where(s => s.AppCode == appCode && s.VendorCode == vendorCode && s.WeekDayType == weekDayType && s.SweepType == "OnSchedule").OrderBy(t => t.SrNo).ToList();
            if (bundlingSweeps.Count > 0)
            {
                // adjust for sunday
                revisedReceivedOnEst = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Sunday ? revisedReceivedOnEst.AddDays(1).Date : revisedReceivedOnEst;

                var applicableSweep = bundlingSweeps.FirstOrDefault(s => s.SweepTime >= revisedReceivedOnEst.TimeOfDay);

                if (applicableSweep == null)
                {
                    revisedReceivedOnEst = revisedReceivedOnEst.AddDays(1);
                    // adjust for sunday
                    revisedReceivedOnEst = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Sunday ? revisedReceivedOnEst.AddDays(1).Date : revisedReceivedOnEst;
                    applicableSweep = bundlingSweeps.First();
                }

                var standardSubmissionTime = (revisedReceivedOnEst.Date + applicableSweep.SweepTime).EstToUtc();

                return standardSubmissionTime;
            }
            else
            {
                throw new Exception(string.Format("Sweep timings are not defined for AppCode: {0}, VendorCode: {1}", appCode, vendorCode));
            }
        }

        public List<Sweep> GetSweepTimings()
        {
            return db.Query<Sweep>("SELECT * from ufn_GetSweepTimings()").ToList();
        }

        public List<PnSConfigDetailPrintTrailMap> GetTrailMapDetailsByPnSConfigDetailId(int pnsConfigDetailId)
        {
            string query = "SELECT * FROM [Common].[PnSConfigDetailPrintTrailMaps] WHERE PnSConfigDetailId = @PnSConfigDetailId";
            return db.Query<PnSConfigDetailPrintTrailMap>(query, new { PnSConfigDetailId = pnsConfigDetailId }).ToList();
        }

        public PrintVendor GetPrintVendorDetailsByPrintTrailId(int printTrailId, DateTime? effectiveReceivedOnEst = null)
        {
            string query = "SELECT pv.*,CASE WHEN DATEPART(DW,@EffectiveReceivedOnEst)=7 AND pv.VendorCode='NORDIS' THEN (SELECT Id FROM Common.ShipperInformation SI WHERE SI.PrintLocation='Las Vegas' AND SI.PrintVendorCode=pt.PrintVendorCode AND PrintVendorId = (SELECT Id FROM Common.PrintVendors WHERE VendorCode='NORDIS')) ELSE pt.ShipperId END AS ShipperId FROM [Common].[PrintTrails] pt INNER JOIN  [Common].[PrintVendors] pv ON pt.PrintVendorId = pv.Id WHERE pt.Id = @PrintTrailId";

            return db.QuerySingleOrDefault<PrintVendor>(query, new { PrintTrailId = printTrailId, EffectiveReceivedOnEst = effectiveReceivedOnEst });
        }

        public DateTime GetMaximumSubmissionTime(DocType docType, DateTime effectiveReceivedOnEst)
        {
            DayOfWeek dayOfWeek = effectiveReceivedOnEst.DayOfWeek;

            string cutOffTime = string.Empty;

            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    cutOffTime = docType.SatCutOff;
                    break;
                case DayOfWeek.Sunday:
                    cutOffTime = docType.SunCutOff;
                    break;
                default:
                    cutOffTime = docType.BizdayCutOff;
                    break;
            }

            var effectiveReceivedOnEstDate = effectiveReceivedOnEst.Date;
            var maxSubmissionDateTime = effectiveReceivedOnEstDate + TimeSpan.Parse(cutOffTime);

            return maxSubmissionDateTime.EstToUtc();
        }

        public int GetPrintTrail(int pnsConfigDetailId, DateTime estEffectiveReceivedOn)
        {
            PnSConfigDetailPrintTrailMap docTypePrintTrail;

            using (ICommonService commonSvc = new CommonService())
            {
                List<PnSConfigDetailPrintTrailMap> docTypePrintTrailMaps = commonSvc.GetTrailMapDetailsByPnSConfigDetailId(pnsConfigDetailId);
                var dayofWeek = estEffectiveReceivedOn.DayOfWeek;

                if (dayofWeek == DayOfWeek.Saturday)
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.SatStartTime && estEffectiveReceivedOn.TimeOfDay <= c.SatEndTime);
                }
                else if (dayofWeek == DayOfWeek.Sunday)
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.SunStartTime && estEffectiveReceivedOn.TimeOfDay <= c.SunEndTime);
                }
                else
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.BizDayStartTime && estEffectiveReceivedOn.TimeOfDay <= c.BizDayEndTime);
                }
            }

            return docTypePrintTrail.PrintTrailId;
        }

        public DateTime GetDueDate(int clientId, DocType docType, DateTime estEffectiveReceivedOn)
        {
            bool isSatWorkDay = docType.SatSLAType == SLAType.NA ? false : true;
            bool isSunWorkDay = docType.SunSLAType == SLAType.NA ? false : true;

            var estEffectiveReceivedOnDate = estEffectiveReceivedOn.Date;

            DayOfWeek dayOfWeek = estEffectiveReceivedOnDate.DayOfWeek;

            string slaTimeSpan = string.Empty;
            string slaType = string.Empty;

            DateTime dueDate = estEffectiveReceivedOn;

            if (dayOfWeek == DayOfWeek.Saturday)
            {
                slaTimeSpan = docType.SatTimeSpan;
                slaType = docType.SatSLAType;
            }

            if (dayOfWeek == DayOfWeek.Sunday)
            {
                slaTimeSpan = docType.SunTimeSpan;
                slaType = docType.SunSLAType;
            }

            if (dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday)
            {
                slaTimeSpan = docType.BizdayTimeSpan;
                slaType = docType.BizdaySLAType;
            }

            TimeSpan timeSpan = TimeSpan.Parse(slaTimeSpan);
            if (slaType == SLAType.FixedTime)
            {
                //Here we are taking only time from full time span which includes dates
                dueDate = estEffectiveReceivedOnDate + TimeSpan.Parse(string.Format("{0:hh\\:mm\\:ss}", timeSpan));

                for (int i = 0; i < timeSpan.Days; i++)
                {
                    dueDate = GetNextWorkingDay(dueDate, Config.GetHolidaysByClientId(clientId), isSatWorkDay, isSunWorkDay);
                }
            }
            else
            {
                dueDate = estEffectiveReceivedOn + TimeSpan.Parse(string.Format("{0:hh\\:mm\\:ss}", timeSpan));
                for (int i = 0; i < timeSpan.Days; i++)
                {
                    dueDate = GetNextWorkingDay(dueDate, Config.GetHolidaysByClientId(clientId), isSatWorkDay, isSunWorkDay);
                }
            }


            return dueDate.EstToUtc();
        }

        public DateTime GetEffectiveEstReceivedOn(int clientId, DocType docType, DateTime utcReceivedOn)
        {
            var estReceivedOn = utcReceivedOn.UtcToEst();
            var estReceivedOnDate = estReceivedOn.Date;
            var effectiveEstReceivedOn = estReceivedOn;

            bool isSatWorkDay = docType.SatSLAType == SLAType.NA ? false : true;
            bool isSunWorkDay = docType.SunSLAType == SLAType.NA ? false : true;

            if (IsBusinessDay(estReceivedOn, Config.GetHolidaysByClientId(clientId), isSatWorkDay, isSunWorkDay))
            {
                DayOfWeek dayOfWeek = estReceivedOn.DayOfWeek;

                string slaCutOff = string.Empty;
                string slaType = string.Empty;

                if (dayOfWeek == DayOfWeek.Saturday)
                {
                    slaCutOff = docType.SatCutOff;
                    slaType = docType.SatSLAType;
                }

                if (dayOfWeek == DayOfWeek.Sunday)
                {
                    slaCutOff = docType.SunCutOff;
                    slaType = docType.SunSLAType;
                }

                if (dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday)
                {
                    slaCutOff = docType.BizdayCutOff;
                    slaType = docType.BizdaySLAType;
                }

                if (slaType == SLAType.FixedTime)
                {
                    var estSLACutOffDateTime = (estReceivedOnDate + TimeSpan.Parse(slaCutOff));

                    if (estReceivedOn > estSLACutOffDateTime)
                    {
                        effectiveEstReceivedOn = GetNextWorkingDay(estReceivedOnDate, Config.GetHolidaysByClientId(clientId), isSatWorkDay, isSunWorkDay);
                    }
                }
                else
                {
                    effectiveEstReceivedOn = estReceivedOn;
                }
            }
            else
            {
                effectiveEstReceivedOn = GetNextWorkingDay(estReceivedOnDate, Config.GetHolidaysByClientId(clientId), isSatWorkDay, isSunWorkDay);
            }

            return effectiveEstReceivedOn;
        }

        public bool IsBusinessDay(DateTime dueDate, List<Holiday> holidays, bool isSatWorkDay, bool isSunWorkDay)
        {
            if (holidays.Select(c => c.HolidayDate).Contains(dueDate.Date))
                return false;

            if (dueDate.DayOfWeek == DayOfWeek.Saturday)
                return isSatWorkDay;

            if (dueDate.DayOfWeek == DayOfWeek.Sunday)
                return isSunWorkDay;

            return true;
        }

        public DateTime GetNextWorkingDay(DateTime estDateTime, List<Holiday> HolidayList, bool isSatWorkDay, bool isSunWorkDay)
        {
            do
            {
                estDateTime = estDateTime.AddDays(1);
            }
            while (!IsBusinessDay(estDateTime, HolidayList, isSatWorkDay, isSunWorkDay));

            return estDateTime;
        }

        public int InsertErrorNotifications(int mailBatchId)
        {
            return db.Execute("usp_InsertErrorNotificationData @mailBatchId", new { mailBatchId = mailBatchId });
        }

        public string GetErrorNotificationEmailSubscriberList(int clientId)
        {
            return String.Join(",", db.Query<string>("SELECT U.Email FROM [AM].[AlertSubscribers] A INNER JOIN [Profile].[Users] U ON A.UserId=U.Id WHERE U.ClientId=@ClientId", new { ClientId = clientId }));
        }

        public void GetUpdatedDueDateAndPrintLocation(DateTime receivedOn, MailPieceDTO mailPiece, DocType docType, Client client)
        {
            mailPiece.DocTypeId = docType.Id;
            mailPiece.PrintTrailId = docType.PrintTrailId;
            mailPiece.PnSConfigDetailId = docType.PnSConfigDetailId;
            mailPiece.EffectiveReceivedOnEst = GetEffectiveEstReceivedOn(client.Id, docType, receivedOn);
            //Calculate Due Date for Mail Pieces doc type wise
            mailPiece.DueDate = GetDueDate(client.Id, docType, mailPiece.EffectiveReceivedOnEst.Value);
            //Set print location where mail piece will go for print and ship
            var printVendorDetails = GetPrintVendorDetailsByPrintTrailId(mailPiece.PrintTrailId.Value, mailPiece.EffectiveReceivedOnEst);
            mailPiece.ShipperId = printVendorDetails.ShipperId;
            mailPiece.PrintLocation = GetShipperInformation(printVendorDetails.ShipperId).PrintLocation;
            mailPiece.StdSubmissionTime = GetStandardSubmissionTime(client.AppCode, printVendorDetails.VendorCode, receivedOn, GetSweepTimings());
            mailPiece.MaxSubmissionTime = GetMaximumSubmissionTime(docType, mailPiece.EffectiveReceivedOnEst.Value);
            mailPiece.EnvelopeName = GetEnvelopeName(mailPiece.PnSConfigDetailId.Value);
            mailPiece.UsedEnvelopeId = docType.EnvelopeId;
        }

        private string GetEnvelopeName(int pnsConfigDetailId)
        {
            string query = "SELECT EnvelopeName FROM Common.Envelopes E INNER JOIN Common.PnSConfigDetail P ON P.EnvelopeId=E.Id WHERE P.Id=@pnsConfigDetailId";
            return db.ExecuteScalar<string>(query, new { pnsConfigDetailId });
        }

        public int GetReportingSequence()
        {
            string query = "SELECT NEXT VALUE FOR dbo.ReportingSequence";
            return db.ExecuteScalar<int>(query);
        }

        public bool CheckForDuplicateMailPieceUniqueId(Guid mpUniqueId)
        {
            string query = "select case when exists(select 1 from AM.MailPieces where UniqueId = @UniqueId) then 1 else 0 end";
            return db.ExecuteScalar<bool>(query, new { UniqueId = mpUniqueId });
        }

        public List<DallasNotificationMap> GetDallasNotificationMaps()
        {
            string query = "SELECT AppCode,SeqNo FROM [Common].[DallasNotificationMapping]";

            return db.Query<DallasNotificationMap>(query, null).ToList();
        }

        public CustomTemplate GetCustomTemplateByDocTypeId(int docTypeId, string communityCode)
        {
            string query = "SELECT * FROM [AM].[CustomTemplates] WHERE DocTypeId= @DocTypeId and CommunityCode = @CommunityCode";

            return db.QuerySingleOrDefault<CustomTemplate>(query, new { DocTypeId = docTypeId, CommunityCode = communityCode });
        }

        public CustomTemplate GetCustomTemplateByCommunityCode(string communityCode)
        {
            string query = "SELECT * FROM [AM].[CustomTemplates] WHERE CommunityCode = @CommunityCode";

            return db.QuerySingleOrDefault<CustomTemplate>(query, new { CommunityCode = communityCode });
        }

        public ShipperInformation GetShipperInformation(int shipperId)
        {
            string query = "SELECT * FROM Common.ShipperInformation WHERE Id=@ShipperId";

            return db.QuerySingleOrDefault<ShipperInformation>(query, new { ShipperId = shipperId });
        }

        public ShipperInformation GetShipperInformationByPrintLocation(string printLocation, int printTrailId)
        {
            string query = "SELECT * FROM Common.ShipperInformation WHERE PrintVendorCode = (SELECT PrintVendorCode FROM Common.PrintTrails WHERE Id=@PrintTrailId) AND PrintLocation=@PrintLocation";

            return db.QuerySingleOrDefault<ShipperInformation>(query, new { PrintLocation = printLocation, PrintTrailId = printTrailId });
        }

        public Task<int> UpdateBatchPipeLineStatusByMailPieceId(Guid mailPieceId, string pipeLineStatus, DateTime timestamp)
        {
            string query = $"UPDATE [AM].[MailBatches] SET PipelineStatus = @pipelineStatus, {pipeLineStatus}On = @timestamp WHERE Id = (SELECT TOP 1 MailBatchId FROM AM.MailPieces WHERE UniqueId = @mailPieceId)";

            return db.ExecuteAsync(query, new { pipeLineStatus, timestamp, mailPieceId });
        }

        public string GetEnvelopeCodeByEnvelopeId(int envelopeId)
        {
            return db.QuerySingle<string>("SELECT EnvelopeName FROM Common.Envelopes WHERE Id=@EnvelopeId", new { EnvelopeId = envelopeId });
        }

        public int GetDallasBatchCounts(int? mpId = 0)
        {
            string query = @"SELECT COUNT(*) FROM AM.MailPieces P WITH (NOLOCK)
                                INNER JOIN AM.MailBatches B WITH (NOLOCK) ON P.MailBatchId = B.Id
								INNER JOIN Common.PrintTrails PT WITH (NOLOCK) ON P.PrintTrailId = PT.Id
                                WHERE PT.PrintVendorCode = 'CL_DALLAS'
                                AND B.ReceivedOn BETWEEN @StartDate AND @EndDate AND P.ShippingStatus IN('InProgress','Shipped')
								AND ISNULL(P.Id,0)<>ISNULL(@MailPieceId,0)";

            return db.ExecuteScalar<int>(query, new { StartDate = Common.EstToday.ToStartDateTime().EstToUtc(), EndDate = Common.EstToday.ToEndDateTime().EstToUtc(), MailPieceId = mpId });
        }

        public List<PackagesHoldInfo> GetPackagesHoldInfo()
        {
            string query = "SELECT C.AppCode AS ClientCode,H.* FROM [AM].[PackagesHoldInfo] H INNER JOIN Common.Clients C ON H.ClientId = C.Id";

            return db.Query<PackagesHoldInfo>(query).ToList();
        }

        public List<string> GetInsertDataByDocTypeId(int docTypeId)
        {
            return db.Query<string>("SELECT E.EnvelopeName FROM AM.DocTypeInserts DI INNER JOIN Common.Envelopes E ON E.Id=DI.BREEnvelopeId WHERE DI.DocTypeId = @DocTypeId", new { DocTypeId = docTypeId }).ToList();
        }

        public List<USPSLabelCutOffDetail> GetUSPSLabelCutOffDetails()
        {
            string query = "SELECT * FROM [API].[USPSLabelCutOffDetails]";

            return db.Query<USPSLabelCutOffDetail>(query).ToList();
        }

        public List<USPSCredentialInfo> GetUSPSCredentialDetails()
        {
            string query = "SELECT AccountNumber AS UserId, WebAuthenticationPassword AS PassKey , WebAuthenticationKey AS PrintLocation FROM [AM].[ShippingAccounts] WHERE ShippingCarrier = 'USPS'";

            return db.Query<USPSCredentialInfo>(query).ToList();
        }

        public Dictionary<string, string> GetCustomTemplates()
        {
            string query = @"
                    SELECT [DocTypeId], [CustomTemplatePath], [CommunityCode]
                    FROM [AM].[CustomTemplates]
                    WHERE [CommunityCode] IN ('MA', 'NY', 'CT')";

            var result = db.Query<(int DocTypeId, string CustomTemplatePath, string CommunityCode)>(query);

            return result.ToDictionary(
                x => $"{x.DocTypeId}_{x.CommunityCode}",
                x => x.CustomTemplatePath
            );
        }

    }
}