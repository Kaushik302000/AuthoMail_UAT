using AM.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;

namespace AM.Services
{
    public class AppUserLogins
    {
        private IDbConnection db;
        public AppUserLogins(IDbConnection db)
        {
            this.db = db;
        }
        //Remove particular logins by user and logininfo
        public int RemoveLoginBuUserAndLogin(IdentityUser user, UserLoginInfo login)
        {
            if (user == null || login == null) throw new ArgumentNullException();
            string commandText = "Delete from [Profile].[UserLogins] where UserId = @userId and LoginProvider = @loginProvider and ProviderKey = @providerKey";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("UserId", user.Id);
            parameters.Add("loginProvider", login.LoginProvider);
            parameters.Add("providerKey", login.ProviderKey);
            return db.Execute(commandText, parameters);
        }
        //Remove all login by user Id
        public int RemoveLoginByUserId(string userId)
        {
            if (userId == null) throw new ArgumentNullException();
            string commandText = "Delete from [Profile].[UserLogins] where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("UserId", userId);
            return db.Execute(commandText, parameters);
        }
        //Assign new login to user
        public int AddNewLogin(IdentityUser user, UserLoginInfo login)
        {
            if (user == null || login == null) throw new ArgumentNullException();
            string commandText = "Insert into [Profile].[UserLogins] (LoginProvider, ProviderKey, UserId) values (@loginProvider, @providerKey, @userId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("loginProvider", login.LoginProvider);
            parameters.Add("providerKey", login.ProviderKey);
            parameters.Add("userId", user.Id);
            return db.Execute(commandText, parameters);
        }
        //Get single UserId by logininfo
        public string FindUserIdByLogin(UserLoginInfo userLogin)
        {
            if (userLogin == null) throw new ArgumentNullException();
            string commandText = "Select UserId from [Profile].[UserLogins] where LoginProvider = @loginProvider and ProviderKey = @providerKey";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("loginProvider", userLogin.LoginProvider);
            parameters.Add("providerKey", userLogin.ProviderKey);
            return db.QueryFirstOrDefault<String>(commandText, parameters);
        }
        //Get List of userlogininfo by userId
        public List<UserLoginInfo> FindUserLoginInfoByUserId(string userId)
        {
            if (userId == null) throw new ArgumentNullException();
            List<UserLoginInfo> logins = new List<UserLoginInfo>();
            string commandText = "Select * from [Profile].[UserLogins] where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@userId", userId } };
            var result = db.Query(commandText, parameters);
            foreach (var item in result)
            {
                logins.Add(new UserLoginInfo(item.ProviderKey, item.LoginProvider));
            }
            return logins;
        }
    }
}
