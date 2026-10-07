using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;
using AM.Lib;

namespace AM.Services
{
    public interface IHomeService : IDisposable
    {
        List<DocumentType> GetDocTypeList(int clientId);
        List<Guid> GetMailPieceIdFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId);
        List<string> GetBusinessFriendlyIdNameByclientId(int clientId);
        string GetClientNameById(int clientId);
        List<ClientIdName> GetAllActiveClient();
        int SetClientIdToUser(string userId, int clientId);
        List<ErrorNotifications> GetErrorNotificationList(int? LastId);
        List<MailPiece> GetErrorMailPieces(DateTime startDate, DateTime endDate, int? clientId);
        Task<IEnumerable<ClientIdName>> GetAllActiveAppCodeForClientRole(string userId);
        List<string> GetBusinessFriendlyIdNameByClientIdAndLatestOccurance(int clientId);
    }
    public class HomeService : ServiceBase, IHomeService //It implements Interfaces 
    {
        public string GetClientNameById(int clientId)
        {
            var data = db.Query<string>(HomeServiceQuery.GetClientName, new { clientId = clientId }).Single();
            return data;
        }
        public List<DocumentType> GetDocTypeList(int clientId)
        {
            var list = db.Query<DocumentType>(HomeServiceQuery.GetDocTypeList, new { clientId = clientId }).ToList();
            return list;
        }


        public List<Guid> GetMailPieceIdFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId)
        {
            string query = HomeServiceQuery.GetMailPieceIdFromBusinessFriendlyValue;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@businessFriendlyIdName", businessFriendlyIdName);
            parameters.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            parameters.Add("@clientId", clientId);
            parameters.Add("@todayDate", Common.Today.Date);
            parameters.Add("@shippingStatus", ShippingStatus.InProgress.ToString());
            parameters.Add("@pipelineStatus", BatchPipelineStatus.SubmittedToShipping.ToString());

            var mailPieces = db.Query<Guid>(query, parameters).ToList();
            return mailPieces;
        }
        public List<string> GetBusinessFriendlyIdNameByclientId(int clientId)
        {
            string query = HomeServiceQuery.GetBusinessFriendlyIdNameByclientId;
            var data = db.Query<string>(query, new { clientId = clientId }).ToList();
            return data;
        }

        public List<string> GetBusinessFriendlyIdNameByClientIdAndLatestOccurance(int clientId)
        {
            string query = @";WITH CTE1 AS(
                            SELECT ROW_NUMBER() OVER(ORDER BY YEAR(CAST(EffectiveReceivedOnEst AS DATE)) DESC) AS RowNumber, BusinessFriendlyIdName, YEAR(CAST(EffectiveReceivedOnEst AS DATE)) AS YearOfName
                            FROM AM.MailPieces WITH(NOLOCK)
                            WHERE ClientId = @clientId AND BusinessFriendlyIdName IS NOT NULL AND EffectiveReceivedOnEst IS NOT NULL AND YEAR(CAST(EffectiveReceivedOnEst AS DATE)) = YEAR(GETDATE())
                            GROUP BY BusinessFriendlyIdName, YEAR(CAST(EffectiveReceivedOnEst AS DATE))
                            ), CTE2 AS
                            (
                            SELECT * FROM AM.ClientBusinessIdentifiers WITH(NOLOCK) WHERE ClientId =  @clientId AND (BusinessIdName IS NOT NULL OR LTRIM(RTRIM(BusinessIdName)) <> '')
                            )
                            SELECT C2.BusinessIdName,ISNULL(C1.RowNumber, 0) AS LatestOccurance FROM CTE1 C1
                             RIGHT OUTER JOIN CTE2 C2 ON C1.BusinessFriendlyIdName = C2.BusinessIdName
                            ORDER BY C1.RowNumber DESC";
            var data = db.Query<string>(query, new { clientId = clientId }).ToList();
            return data;
        }


        public List<ClientIdName> GetAllActiveClient()
        {
            return db.Query<ClientIdName>(HomeServiceQuery.GetAllActiveClient).OrderBy(o => o.Name).ToList();
        }


        public int SetClientIdToUser(string userId, int clientId)
        {
            return db.Execute(HomeServiceQuery.SetClientIdToUser, new { clientId = clientId, userId = userId });
        }


        public List<ErrorNotifications> GetErrorNotificationList(int? LastId)
        {
            if (LastId.HasValue)
            {
                string q = "select * from AM.ErrorNotifications where Id > @Id";
                return db.Query<ErrorNotifications>(q, new { Id = LastId.Value }).ToList();
            }
            else
            {
                DateTime utcNow = DateTime.UtcNow;
                DateTime startDate = utcNow.UtcToEst().ToStartDateTime().EstToUtc();
                DateTime endDate = utcNow.UtcToEst().ToEndDateTime().EstToUtc();
                string q = @"select * from AM.ErrorNotifications where ReceivedOn between @startDate and @endDate";
                return db.Query<ErrorNotifications>(q, new { startDate = startDate, endDate = endDate }).ToList();
            }
        }



        public List<MailPiece> GetErrorMailPieces(DateTime startDate, DateTime endDate, int? clientId)
        {
            string q = "select m.* from AM.Mailpieces m inner join AM.MailBatches b on m.MailBatchid=b.Id where b.ReceivedOn between @startDate and @endDate";
            if (clientId.HasValue)
            {
                q = string.Format("{0} and m.ClientId=@clientId", q);
                return db.Query<MailPiece>(q, new { startDate = startDate, endDate = endDate, clientId = clientId }).ToList();
            }
            return db.Query<MailPiece>(q, new { startDate = startDate, endDate = endDate }).ToList();
        }

        public async Task<IEnumerable<ClientIdName>> GetAllActiveAppCodeForClientRole(string userId)
        {
            string sql = @"SELECT c.Id, c.Name, c.GroupAlias FROM Common.Clients c 
inner join Common.ClientUserMapping cu on c.Id = cu.ClientId
WHERE cu.status=@status and c.IsAMActive = 1 and cu.UserId = @userId";
            return await db.QueryAsync<ClientIdName>(sql, new { userId = userId, status = ClientUserMappingStatusTypes.Approved });
        }
    }
    public static class HomeServiceQuery
    {
        public static string GetClientName = "select Name from [Common].[Clients] where Id=@clientId";

        public static string GetMailPieceByBatchId = "select * from [dbo].[view_MailPieceByParam] where MailBatchId=@batchId and clientId=@clientId";
        //Description AS
        public static string GetDocTypeList = "select Id, DocTypeName FROM [AM].[DocTypes] WHERE ClientId=@clientId and DocTypeName <>'E-Delivery' AND IsActive=1";

        public static string GetMailPieceIdFromBusinessFriendlyValue = "select m.[UniqueId] as [MailPieceId] from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id = m.MailBatchId and ((cast(ReceivedOn as date)=cast(@todayDate as date) or (m.ShippingStatus=@shippingStatus and b.PipelineStatus=@pipelineStatus))) and m.BusinessFriendlyIdName=@businessFriendlyIdName AND m.BusinessFriendlyIdValue=@businessFriendlyIdValue AND b.ClientId=@clientId";

        public static string GetBusinessFriendlyIdNameByclientId = "SELECT DISTINCT [BusinessIdName] FROM [AM].[ClientBusinessIdentifiers] WHERE [BusinessIdName] Is NOT NULL AND ClientId=@clientId";
        public static string GetAllActiveClient = "SELECT Id,Name,AppCode,GroupAlias FROM [Common].[Clients] WHERE IsAMActive=1";
        public static string SetClientIdToUser = "UPDATE [Profile].[Users] SET ClientId=@clientId WHERE Id=@userId";
    }
}
