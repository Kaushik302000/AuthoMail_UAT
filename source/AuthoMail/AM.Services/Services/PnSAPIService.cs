using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AM.Services.Services
{
    public interface IPnSAPIService : IDisposable
    {
        Task<AddPackageResult> AddPackage(Guid? id, int clientId, DateTime receivedOn, float requestSize);
        Task UpdatePackageStatus(RequestLog requestLog);
        RequestLog GetAMRequestInfo(string guid);
    }

    public class PnSAPIService : ServiceBase, IPnSAPIService
    {
        //public PnSAPIService() : base("AMAPI") { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="receivedOn"></param>
        /// <returns>Returns HTTP reponse status codes 200, 400, 406, 409</returns>
        public async Task<AddPackageResult> AddPackage(Guid? id, int clientId, DateTime receivedOn, float requestSize)
        {
            await db.OpenAsync();

            using (var t = db.BeginTransaction())
            {
                var responseHttpStatusCode = 0;
                bool isNewReqPrimary = false;

                if (!id.HasValue || id == Guid.Empty)
                {
                    isNewReqPrimary = true;
                    responseHttpStatusCode = 0;
                }
                else
                {
                    // check if request is successfully accepted before
                    var primaryReq = await db.QueryAsync<ReqStatus>(@"
                        select Id, HTTPStatusCode from API.AMRequestLogs
                        where MailPieceId = @id and IsPrimary = 1
                    ", new { id }, transaction: t);


                    //if found more than 1 primary requests then consider latest one the new primary request
                    if (primaryReq.Count() > 1)
                    {
                        isNewReqPrimary = true;
                        responseHttpStatusCode = 0;
                    }
                    else
                    {
                        // 406 and 409 get higher priority than 400
                        // this ensures that even if a request is tried 100 times (more than max limit)
                        // they still get 406 or 409 if applicable

                        if (primaryReq.Count() == 0)
                        {
                            isNewReqPrimary = true;
                            responseHttpStatusCode = 0;
                        }
                        else if (primaryReq.First().HTTPStatusCode == 0)
                        {
                            // Earlier request is still in progress
                            isNewReqPrimary = false;
                            responseHttpStatusCode = 406;
                        }
                        else if (primaryReq.First().HTTPStatusCode == 200)
                        {
                            // request has already been accepted
                            isNewReqPrimary = false;
                            responseHttpStatusCode = 409;
                        }
                        else
                        {
                            /// request is received for the first time
                            /// or multiple times less than max retries
                            /// or more than max retries

                            // it is not 0 or 200 so it becomes primary now
                            isNewReqPrimary = true;

                            // check if the request is received multiple times
                            var reqCount = await db.ExecuteScalarAsync<int>(@"
                            select count(Id) from API.AMRequestLogs where MailPieceId = @id
                        ", new { id }, transaction: t);

                            responseHttpStatusCode = reqCount >= Config.MailRequestAPI_MaxRetries ? 400 : 0;
                        }
                    }

                    if (primaryReq.Count() != 0 && isNewReqPrimary)
                    {
                        await db.ExecuteAsync(@"
                            update API.AMRequestLogs set IsPrimary = 0 where Id = @Id
                        ", primaryReq, transaction: t);
                    }
                }

                // record current request
                var reqId = await db.ExecuteScalarAsync<int>(@"
                    insert into API.AMRequestLogs (ClientId, MailPieceId, IsPrimary, Status, HTTPStatusCode, ReceivedOn, RequestSize)
                    OUTPUT INSERTED.ID
                    values (@clientId, @id, @isNewReqPrimary, @status, @httpStatusCode, @receivedOn, @requestSize)
                ", new { clientId, id, isNewReqPrimary, status = ReqStatus.Verifying, httpStatusCode = 0, receivedOn, requestSize }, transaction: t);

                t.Commit();
                db.Close();

                return new AddPackageResult { Id = reqId, HTTPStatusCode = responseHttpStatusCode };
            }
        }

        public RequestLog GetAMRequestInfo(string guid)
        {
            string query = @"SELECT * FROM API.AMRequestLogs WHERE MailPieceId=@UniqueId AND IsPrimary=1";

            return db.QuerySingleOrDefault<RequestLog>(query, new { UniqueId = guid });
        }

        public async Task UpdatePackageStatus(RequestLog requestLog)
        {
            await db.OpenAsync();
            // update package status
            using (var t = db.BeginTransaction())
            {
                requestLog.Status = requestLog.Status == "OK" ? ReqStatus.Queued : requestLog.Status;

                await db.ExecuteAsync(@"
                    update API.AMRequestLogs
                    set Status = @Status,
                        HTTPStatusCode = @HTTPStatusCode,
                        APIResponse = @APIResponse,
                        TotalPages = @TotalPages,
                        ReceivedOn = @ReceivedOn,
                        ProcessedOn = @ProcessedOn,
                        ErrorLog = @ErrorLog,
                        InitialChecks = @InitialChecks,
                        Download = @Download,
                        Validate = @Validate,
                        [Merge] = @Merge,
                        PostBatch = @PostBatch,
                        Cleanup = @Cleanup,
                        TotalTime = @TotalTime
                    where Id = @Id
                ", requestLog, transaction: t);

                t.Commit();
                db.Close();
            }
        }
    }
}
