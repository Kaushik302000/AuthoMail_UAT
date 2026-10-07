using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;
using System.Data;

namespace AM.Services
{
    public class AppUser<TUser> where TUser : IdentityUser
    {
        private IDbConnection db;
        public AppUser(IDbConnection db)
        {
            this.db = db;
        }
        public int AddNewUser(TUser user)
        {
            if (user == null) throw new ArgumentNullException();

            var existingUser = GetUserByName(user.UserName);
            if (existingUser.Count > 0) throw new Exception("User name already exist.");

            string query = @"Insert into [Profile].[Users] (UserName, Id, PasswordHash, SecurityStamp,Email,EmailConfirmed,PhoneNumber,PhoneNumberConfirmed, AccessFailedCount,LockoutEnabled,LockoutEndDateUtc,TwoFactorEnabled,ClientId,FirstName,LastName,DisplayName,RequestedOn,Status,ClosedOn,ClosedBy,StatusModifiedBy,StatusModifiedOn,SFTChannelId)
                values (@name, @id, @pwdHash, @SecStamp,@email,@emailconfirmed,@phonenumber,@phonenumberconfirmed,@accesscount,@lockoutenabled,@lockoutenddate,@twofactorenabled,@clientId,@firstName,@lastName,@displayName,@requestedOn,@status,@closedOn,@closedBy,@StatusModifiedBy,@StatusModifiedOn,@SFTChannelId)";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@name", user.UserName);
            parameters.Add("@id", user.Id);
            parameters.Add("@pwdHash", user.PasswordHash);
            parameters.Add("@SecStamp", user.SecurityStamp);
            parameters.Add("@email", user.Email);
            parameters.Add("@emailconfirmed", user.EmailConfirmed);
            parameters.Add("@phonenumber", user.PhoneNumber);
            parameters.Add("@phonenumberconfirmed", user.PhoneNumberConfirmed);
            parameters.Add("@accesscount", user.AccessFailedCount);
            parameters.Add("@lockoutenabled", user.LockoutEnabled);
            parameters.Add("@lockoutenddate", user.LockoutEndDateUtc);
            parameters.Add("@twofactorenabled", user.TwoFactorEnabled);
            parameters.Add("@clientId", user.ClientId);
            parameters.Add("@firstName", user.FirstName);
            parameters.Add("@lastName", user.LastName);
            parameters.Add("@displayName", user.DisplayName);
            parameters.Add("@requestedOn", user.RequestedOn == DateTime.MinValue ? DateTime.UtcNow : user.RequestedOn);
            parameters.Add("@status", user.Status);
            parameters.Add("@closedOn", user.ClosedOn);
            parameters.Add("@closedBy", user.ClosedBy);
            parameters.Add("@StatusModifiedOn", user.StatusModifiedOn);
            parameters.Add("@StatusModifiedBy", user.StatusModifiedBy);
            parameters.Add("@SFTChannelId", user.SFTChannelId);
            //parameters.Add("@ChangePasswordDateUTC",DateTime.UtcNow);
            return db.Execute(query, parameters);
        }

        public List<TUser> GetUserByName(string userName)
        {
            if (userName == null) throw new ArgumentNullException();
            string query = "Select * from [Profile].[Users] where UserName = @name";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", userName } };

            List<TUser> result = db.Query<TUser>(query, parameters).ToList();
            return result;
        }
        public int Update(TUser user)
        {
            if (user == null) throw new ArgumentNullException();

            string query = @"Update [Profile].[Users] set UserName = @userName, PasswordHash = @pswHash, SecurityStamp = @secStamp, 
                Email=@email, EmailConfirmed=@emailconfirmed, PhoneNumber=@phonenumber, PhoneNumberConfirmed=@phonenumberconfirmed,
                AccessFailedCount=@accesscount, LockoutEnabled=@lockoutenabled, LockoutEndDateUtc=@lockoutenddate, TwoFactorEnabled=@twofactorenabled,ClientId=@clientId,FirstName=@firstName,LastName=@lastName,DisplayName=@displayName,RequestedOn=@requestedOn,Status=@status,ClosedOn=@closedOn,ClosedBy=@closedBy,LastLoginTimeUTC=@LastLoginTimeUTC,StatusModifiedOn=@StatusModifiedOn,StatusModifiedBy=@StatusModifiedBy,SFTChannelId = @SFTChannelId
                WHERE Id = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userName", user.UserName);
            parameters.Add("@pswHash", user.PasswordHash);
            parameters.Add("@secStamp", user.SecurityStamp);
            parameters.Add("@userId", user.Id);
            parameters.Add("@email", user.Email);
            parameters.Add("@emailconfirmed", user.EmailConfirmed);
            parameters.Add("@phonenumber", user.PhoneNumber);
            parameters.Add("@phonenumberconfirmed", user.PhoneNumberConfirmed);
            parameters.Add("@accesscount", user.AccessFailedCount);
            parameters.Add("@lockoutenabled", user.LockoutEnabled);
            parameters.Add("@lockoutenddate", user.LockoutEndDateUtc);
            parameters.Add("@twofactorenabled", user.TwoFactorEnabled);
            parameters.Add("@clientId", user.ClientId);
            parameters.Add("@firstName", user.FirstName);
            parameters.Add("@lastName", user.LastName);
            parameters.Add("@displayName", user.DisplayName);
            parameters.Add("@requestedOn", user.RequestedOn == DateTime.MinValue ? DateTime.UtcNow : user.RequestedOn);
            parameters.Add("@status", user.Status);
            parameters.Add("@closedOn", user.ClosedOn);
            parameters.Add("@closedBy", user.ClosedBy);
            parameters.Add("@LastLoginTimeUTC", user.LastLoginTimeUTC);
            parameters.Add("@StatusModifiedOn", user.StatusModifiedOn);
            parameters.Add("@StatusModifiedBy", user.StatusModifiedBy);
            parameters.Add("@SFTChannelId", user.SFTChannelId);
            return db.Execute(query, parameters);
        }
        public TUser GetUserById(string userId)
        {
            if (userId == null) throw new ArgumentNullException();
            string query = "Select * from [Profile].[Users] where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", userId } };

            return db.QuerySingleOrDefault<TUser>(query, parameters);
        }
        public string GetPasswordHash(string userId)
        {
            if (userId == null) throw new ArgumentNullException();
            string query = "Select PasswordHash from [Profile].[Users] where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", userId);

            return db.QueryFirst<string>(query, parameters);
        }

        public TUser GetUserByEmail(string email)
        {
            if (email == null) throw new ArgumentNullException();
            string query = "Select * from [Profile].[Users] where Email = @email";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@email", email } };
            return db.QuerySingleOrDefault<TUser>(query, parameters);
        }
        public IQueryable<TUser> GetAllUsers()
        {
            string query = "Select * from [Profile].[Users]";
            return db.Query<TUser>(query).AsQueryable();
        }
    }

}
