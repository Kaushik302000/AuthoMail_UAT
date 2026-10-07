using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using Dapper;

namespace AM.Services
{
    public interface IClientPermissionService : IDisposable
    {
        Task<IEnumerable<ClientIdNameVM>> GetClientDomainByDomainName(string domainName);
        Task<IEnumerable<ClientIdNameVM>> GetClientDomainForCL();
        Task<IEnumerable<ClientIdNameVM>> GetAppCodeByUserIdORall(string userId);
        Task<IEnumerable<ClientIdNameVM>> GetAppCodeByClientDomain(string clientDomain);
        Task InsertClientIdAndUserId(ClientUserMapping clientUserMapping);
        Task<IEnumerable<ClientUserMapping>> ApproveOrDeclinedClientsForUser(string userId, List<int> approveclientIds, string modifiedById, bool isCLUser);
        Task<IEnumerable<ClientUserMapping>> GetClientDomainByUserId(string userId);
        Task<bool> IsUserClientRequestPending(string userId);
        Task<KeyValuePair<bool, string>> EditSaveRoleClient(ProfileUser user, List<int> clientIds, string role, string modifiedById, bool isCLUser);
        Task<int[]> GetApproveAppCodesByUserId(string userId);
    }
    public class ClientPermissionService : ServiceBase, IClientPermissionService
    {
        public async Task<IEnumerable<ClientUserMapping>> ApproveOrDeclinedClientsForUser(string userId, List<int> approveclientIds, string modifiedById, bool isCLUser)
        {
            if (isCLUser)
            {
                //Remove existing ClientUserMapping request and add new request
                await db.ExecuteAsync(@"delete from Common.ClientUserMapping where UserId=@userId", new { userId = userId });
                var now = DateTime.UtcNow;
                var clientUserMapping = new ClientUserMapping
                {
                    CreatedOn = now,
                    ModifiedOn = now,
                    ModifiedById = modifiedById,
                    Status = ClientUserMappingStatusTypes.Pending,
                    UserId = userId
                };
                for (int i = 0; i < approveclientIds.Count; i++)
                {
                    clientUserMapping.ClientId = approveclientIds[i];
                    await db.ExecuteAsync(@"INSERT INTO [Common].[ClientUserMapping]
           ([ClientId],[UserId],[Status],[CreatedOn],[ModifiedOn],[ModifiedById]) VALUES (@ClientId,@UserId,@Status,@CreatedOn,@ModifiedOn,@ModifiedById)", clientUserMapping);
                }
            }

            //Approve client request
            string sql = @"update Common.ClientUserMapping
set Status=@status,
ModifiedOn=@now,
ModifiedById=@ModifiedById
where UserId=@userId and ClientId in @clientIds";
            await db.ExecuteAsync(sql, new
            {
                userId = userId,
                status = ClientUserMappingStatusTypes.Approved,
                ModifiedById = modifiedById,
                now = DateTime.UtcNow,
                clientIds = approveclientIds
            });

            //Decline client other request
            sql = @"update Common.ClientUserMapping
                set Status=@status,
                ModifiedOn=@now,
                ModifiedById=@ModifiedById
                where UserId=@userId and Status=@pendingStatus";
            db.Execute(sql, new
            {
                userId = userId,
                status = ClientUserMappingStatusTypes.Declined,
                ModifiedById = modifiedById,
                now = DateTime.UtcNow,
                pendingStatus = ClientUserMappingStatusTypes.Pending
            });

            string setClientToUser = "UPDATE [Profile].[Users] SET ClientId=@clientId,Status=@status,ClosedOn=@closedOn,ClosedBy=@closedBy WHERE Id=@userId";
            int result = db.Execute(setClientToUser, new { status = AMUserStatus.Approved, closedOn = DateTime.UtcNow, clientId = approveclientIds.Min(), closedBy = modifiedById, userId = userId });
            return await GetClientDomainByUserId(userId);
        }

        public async Task<IEnumerable<ClientUserMapping>> GetClientDomainByUserId(string userId)
        {
            string sql = @"select cum.*,c.Name +' (' +c.AppCode+')' as ClientName from Common.ClientUserMapping cum
inner join Common.Clients c on cum.ClientId=c.Id
where UserId=@userId";
            return await db.QueryAsync<ClientUserMapping>(sql, new { userId = userId });
        }
        public async Task<IEnumerable<ClientIdNameVM>> GetClientDomainByDomainName(string domainName)
        {
            string sql = @"SELECT c.Id,c.Name,c.AppCode,c.GroupAlias FROM Common.ClientDomains cd
inner join Common.Clients c on cd.ClientId=c.Id
where AllowedDomain=@domainName order by c.Name";
            var output = await db.QueryAsync<ClientIdNameVM>(sql, new { domainName = domainName });
            return output;
        }

        public async Task<IEnumerable<ClientIdNameVM>> GetClientDomainForCL()
        {
            string sql = @"SELECT distinct c.Id,c.Name,c.AppCode,c.GroupAlias FROM Common.Clients c
order by c.Name";
            var output = await db.QueryAsync<ClientIdNameVM>(sql);
            return output;
        }
        public async Task InsertClientIdAndUserId(ClientUserMapping clientUserMapping)
        {
            string isExistSql = "select top 1 1 from Common.ClientUserMapping where UserId=@userId and ClientId=@clientId";
            int? isExist = await db.QuerySingleOrDefaultAsync<int?>(isExistSql, new
            {
                userId = clientUserMapping.UserId,
                clientId = clientUserMapping.ClientId
            });
            if (!isExist.HasValue)
            {
                string sql = @"insert into Common.ClientUserMapping (ClientId,UserId,Status,CreatedOn,ModifiedOn,ModifiedById) 
values(@ClientId,@UserId,@Status,@CreatedOn,@ModifiedOn,@ModifiedById)";
                clientUserMapping.CreatedOn = clientUserMapping.ModifiedOn = DateTime.UtcNow;
                await db.ExecuteAsync(sql, clientUserMapping);
            }
        }

        public async Task<bool> IsUserClientRequestPending(string userId)
        {
            string sql = "select top 1 1 from Common.ClientUserMapping where UserId=@userId";
            int? isExist = await db.QuerySingleOrDefaultAsync<int?>(sql, new { userId = userId });
            return isExist.HasValue;
        }

        public async Task<IEnumerable<ClientIdNameVM>> GetAppCodeByUserIdORall(string userId)
        {
            if (userId == null)
            {
                string sql = @"SELECT Id, Name, AppCode, GroupAlias from Common.Clients where IsAMActive=1 order by Name";
                return await db.QueryAsync<ClientIdNameVM>(sql);
            }
            else
            {
                string sql = @"SELECT Id, Name, AppCode, GroupAlias from Common.Clients where IsAMActive=1 and Id in(
select ClientId from Common.ClientUserMapping where UserId=@userId) order by Name";
                return await db.QueryAsync<ClientIdNameVM>(sql, new { userId = userId });
            }
        }

        public async Task<IEnumerable<ClientIdNameVM>> GetAppCodeByClientDomain(string clientDomain)
        {
            if (clientDomain == null)
            {
                string sql = @"SELECT Id, Name, AppCode, GroupAlias from Common.Clients where IsAMActive=1 order by Name";
                return await db.QueryAsync<ClientIdNameVM>(sql);
            }
            else
            {
                string sql = @"SELECT Id, Name, AppCode, GroupAlias from Common.Clients where IsAMActive=1 and Id in(select ClientId from Common.ClientDomains WHERE AllowedDomain =@Domain) order by Name";
                return await db.QueryAsync<ClientIdNameVM>(sql, new { Domain = clientDomain });
            }
        }

        public async Task<KeyValuePair<bool, string>> EditSaveRoleClient(ProfileUser user, List<int> clientIds, string role, string modifiedById, bool isCLUser)
        {
            await db.OpenAsync();
            using (var t = db.BeginTransaction())
            {
                try
                {
                    var BAClientRoleId = await db.QueryAsync<string>("select Id from Profile.Roles where Name=@ba or Name=@baOffShore or Name=@client", new { ba = AMRoles.BA, baOffShore = AMRoles.BAOffShore, client = AMRoles.Client }, transaction: t);
                    if (isCLUser && (role == AMRoles.BA || role == AMRoles.BAOffShore))
                    {
                        string sql = "delete from Common.ClientUserMapping where UserId=@userId";
                        await db.ExecuteAsync(sql, new { userId = user.Id }, transaction: t);

                        sql = @"delete from Profile.UserRoles where UserId=@userId and RoleId in @roles";
                        await db.ExecuteAsync(sql, new { userId = user.Id, roles = BAClientRoleId }, transaction: t);

                        sql = @"insert into Profile.UserRoles (UserId,RoleId) values(@userId,(select Id from Profile.Roles where Name=@roleName))";
                        await db.ExecuteAsync(sql, new { userId = user.Id, roleName = role }, transaction: t);
                    }
                    else
                    {
                        if (role == AMRoles.BA)
                            return new KeyValuePair<bool, string>(false, "Invalid role.");
                        string sql = "delete from Common.ClientUserMapping where UserId=@userId";
                        await db.ExecuteAsync(sql, new { userId = user.Id }, transaction: t);

                        sql = @"delete from Profile.UserRoles where UserId=@userId and RoleId in @roles";
                        await db.ExecuteAsync(sql, new { userId = user.Id, roles = BAClientRoleId }, transaction: t);

                        sql = @"insert into Profile.UserRoles (UserId,RoleId) values(@userId,(select Id from Profile.Roles where Name=@roleName))";
                        await db.ExecuteAsync(sql, new { userId = user.Id, roleName = AMRoles.Client }, transaction: t);

                        sql = "INSERT INTO Common.ClientUserMapping (ClientId ,UserId ,Status ,CreatedOn ,ModifiedOn ,ModifiedById)  VALUES (@ClientId ,@UserId ,@Status ,@CreatedOn ,@ModifiedOn ,@ModifiedById)";
                        var now = DateTime.UtcNow;
                        for (int i = 0; i < clientIds.Count; i++)
                        {
                            await db.ExecuteAsync(sql, new
                            {
                                ClientId = clientIds[i],
                                UserId = user.Id,
                                Status = ClientUserMappingStatusTypes.Approved,
                                CreatedOn = now,
                                ModifiedOn = now,
                                ModifiedById = modifiedById
                            }, transaction: t);
                        }
                        sql = "update Profile.Users set ClientId=@clientId where Id=@userId";
                        await db.ExecuteAsync(sql, new { userId = user.Id, clientId = clientIds.Min() }, transaction: t);
                    }
                    t.Commit();
                    return new KeyValuePair<bool, string>(true, "Save successfully");
                }
                catch (Exception ex)
                {
                    t.Rollback();
                    return new KeyValuePair<bool, string>(false, ex.Message);
                }

            }
        }

        public async Task<int[]> GetApproveAppCodesByUserId(string userId)
        {
            var output = await db.QueryAsync<int>("select ClientId from Common.ClientUserMapping where UserId=@userId and Status=@status", new { userId = userId, status = ClientUserMappingStatusTypes.Approved });
            return output.ToArray();
        }
    }
}
