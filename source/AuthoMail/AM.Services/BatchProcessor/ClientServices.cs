using AM.Services;
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
    public interface IClientService : IDisposable//Home Service interface that has number of methods
    {
        Client GetClientById(int clientId);
        Task<Client> GetClientByIdAsync(int clientId);
        Task<Client> GetClientByAppCode(string appCode);
        Task<Client> GetClientByName(string clientName);
        List<ClientSFTChannel> GetClientSFTChannelMapDetails(int clientId);
        Task<SFTChannel> GetSFTChannelDetailsBySFTChannelId(int sftChannelId);
        Task<List<SFTChannel>> GetSFTChannelDetailsByServerIP(string serverIP);
        SFTChannel GetSFTChannelDetailsBySFTChannelCode(string sftChannelCode);
        List<ClientAMCCustIdMap> GetAMCClientIdMaps();
        Task<int?> GetAMCClientIdByCustId(string custId);
        List<Client> GetClientsByAlias(string alias);
        List<SFTChannel> GetUncSFTChannelDetails();
    }
    public class ClientService : ServiceBase, IClientService //It implements Interfaces 
    {
        public Client GetClientById(int clientId)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE IsAMActive=1 and Id = @Id ";
            var data = db.QuerySingleOrDefault<Client>(query, new { Id = clientId });
            return data;
        }

        public async Task<Client> GetClientByIdAsync(int clientId)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE IsAMActive=1 and Id = @Id ";
            return await db.QuerySingleOrDefaultAsync<Client>(query, new { Id = clientId });
        }

        public async Task<Client> GetClientByAppCode(string appCode)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE IsAMActive=1 and AppCode = @AppCode";
            var data = await db.QuerySingleOrDefaultAsync<Client>(query, new { AppCode = appCode });
            return data;
        }

        public async Task<Client> GetClientByName(string clientName)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE IsAMActive=1 and Id = (select ClientId from AM.FloodClientMapping where Name = @Name)";
            var data = await db.QuerySingleOrDefaultAsync<Client>(query, new { Name = clientName });
            return data;
        }

        public List<ClientSFTChannel> GetClientSFTChannelMapDetails(int clientId)
        {
            string query = "SELECT ClientId, SFTChannelId FROM [Common].[ClientSFTChannels] WHERE ClientId=@clientId";

            var data = db.Query<ClientSFTChannel>(query, new { clientId = clientId }).ToList();
            return data;
        }

        public async Task<SFTChannel> GetSFTChannelDetailsBySFTChannelId(int sftChannelId)
        {
            string query = "SELECT Id, Code, Description, Title, Protocol, Port, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction, APITokenURL, APIResponseURL, APIMaxResponseCount, APITokenHeaders, ServerIP FROM [Common].[SFTChannels] WHERE Id=@sftChannelId AND IsActive=1";

            var data = await db.QuerySingleOrDefaultAsync<SFTChannel>(query, new { sftChannelId = sftChannelId });
            return data;
        }

        public async Task<List<SFTChannel>> GetSFTChannelDetailsByServerIP(string serverIP)
        {
            string query = "SELECT Id, Code, Description, Title, Protocol, Port, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction, APITokenURL, APIResponseURL, APIMaxResponseCount, APITokenHeaders, ServerIP FROM [Common].[SFTChannels] WHERE ServerIP=@ServerIP AND IsActive=1";

            var data = await db.QueryAsync<SFTChannel>(query, new { ServerIP = serverIP });
            return data.ToList();
        }

        public SFTChannel GetSFTChannelDetailsBySFTChannelCode(string sftChannelCode)
        {
            string query = "SELECT Id, Code, Description, Title, Protocol, Port, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction, APITokenURL, APIResponseURL, APIMaxResponseCount, APITokenHeaders, ServerIP FROM [Common].[SFTChannels] WHERE Code=@sftChannelCode AND IsActive=1";

            var data = db.QuerySingleOrDefault<SFTChannel>(query, new { sftChannelCode = sftChannelCode });
            return data;
        }

        public List<ClientAMCCustIdMap> GetAMCClientIdMaps()
        {
            string query = "SELECT [ClientId],[AMCCustId] FROM [AM].[ClientAMCCustIdMap]";

            return db.Query<ClientAMCCustIdMap>(query, null).ToList();
        }

        public async Task<int?> GetAMCClientIdByCustId(string custId)
        {
            string query = "SELECT [ClientId] FROM [AM].[ClientAMCCustIdMap] WHERE AMCCustId=@CustId";

            return await db.QuerySingleOrDefaultAsync<int?>(query, new { CustId = custId });
        }

        public List<Client> GetClientsByAlias(string alias)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE IsAMActive=1 and GroupAlias = @alias";
            return db.Query<Client>(query, new { Alias = alias }).ToList();
        }

        public List<SFTChannel> GetUncSFTChannelDetails()
        {
            string query = "SELECT Id, Code, Description, Title, Protocol, Port, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction, APITokenURL, APIResponseURL, APIMaxResponseCount, APITokenHeaders, ServerIP FROM [Common].[SFTChannels] WHERE Protocol = 'unc' AND PostDownloadAction ='Archive' AND CODE IN ('PENFED_Prod', 'GOS_Prod', 'FLOODINTEGRATION_Prod', 'BECU_Prod', 'DELFUS_Prod', 'API_ELLIEMAE_Prod', 'WEBREQUEST_Prod', 'API_AMC_Prod', 'DOCSOL_Prod', 'FAM_Prod', 'ORACLEINVOICES_Prod')";

            var data = db.Query<SFTChannel>(query).ToList();
            return data;
        }
    }
}
