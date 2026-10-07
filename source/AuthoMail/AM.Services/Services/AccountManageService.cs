using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;
using AM.Lib;
using AM.Entities;
using System.Net.Mail;

namespace AM.Services
{
    public interface IAccountManageService : IDisposable
    {
        KeyValuePair<bool, string> CreateUser(AccountUser user, string userId);
        Task UpdateChangePasswordDateAsync(string userId);
        Task<IEnumerable<AMAllUsers>> GetAllUsers();
    }
    public class AccountManageService : ServiceBase, IAccountManageService
    {
        public KeyValuePair<bool, string> ReturnSuccess(string msg)
        {
            return new KeyValuePair<bool, string>(true, msg);
        }
        public KeyValuePair<bool, string> ReturnError(string msg)
        {
            return new KeyValuePair<bool, string>(false, msg);
        }
        public KeyValuePair<bool, string> CreateUser(AccountUser user, string userId)
        {
            try
            {
                //Common validation
                if (user.UserName.IsNullOrWhiteSpace()) return ReturnError("Username is required.");
                if (user.Roles.Count() == 0) return ReturnError("Please assign role.");
                if (user.Status.IsNullOrWhiteSpace()) return ReturnError("Status field is required.");
                if (user.UserName.Length < 2 || user.UserName.Length > 35) return ReturnError("Username is too short or long.");

                user.SecurityStamp = Guid.NewGuid().ToString();
                //Password Rules
                PasswordValidator passVal = new PasswordValidator()
                {
                    RequiredLength = 8,
                    RequireNonLetterOrDigit = true,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireUppercase = true
                };
                //Validate Password
                var r = passVal.ValidateAsync(user.PasswordHash).Result;
                if (!r.Succeeded)
                {
                    return ReturnError(string.Join(",", r.Errors));
                }

                var passHash = new PasswordHasher();
                user.PasswordHash = passHash.HashPassword(user.PasswordHash);
                user.LockoutEnabled = true;

                if (user.IsAPIUser)
                {
                    //Code for API User
                    if (!user.Email.IsNullOrWhiteSpace())
                    {
                        var mailAddress = new MailAddress(user.Email);
                        var getDomains = db.Query<string>("select AllowedDomain from Common.ClientDomains").ToList();
                        if (!getDomains.Any(g => g == mailAddress.Host)) ReturnError("Domain not registered.");
                    }
                    //Role Validation
                    var roles = new List<string>() { AMRoles.API, AMRoles.APIUSPSeVS, AMRoles.APIUSPSVerify };
                    foreach (var item in user.Roles)
                    {
                        if (!roles.Any(rl => rl == item)) return ReturnError("Please assign API roles only.");
                    }
                }
                else
                {
                    //Code for Web User
                    if (user.FirstName.IsNullOrWhiteSpace()) return ReturnError("First name is required.");
                    if (user.LastName.IsNullOrWhiteSpace()) return ReturnError("Last name is required.");
                    if (user.FirstName.Length < 2 || user.FirstName.Length > 50) return ReturnError("First name is too short or long.");
                    if (user.LastName.Length < 2 || user.LastName.Length > 50) return ReturnError("Last name is too short or long.");
                    if (user.DisplayName.IsNullOrWhiteSpace())
                        user.DisplayName = string.Format("{0}, {1}", user.LastName, user.FirstName);
                    if (user.Status == AMUserStatus.Approved && !user.ClientId.HasValue) return ReturnError("Client field is required.");
                    if (user.Email.IsNullOrWhiteSpace()) return ReturnError("Email field is required.");
                    var mailAddress = new MailAddress(user.Email);
                    //Role Validation
                    var roles = new List<string>() { AMRoles.Admin, AMRoles.BA, AMRoles.Client, AMRoles.ClientUserManager, AMRoles.CoreLogicUserManager, AMRoles.PnSOperator };
                    foreach (var item in user.Roles)
                    {
                        if (!roles.Any(rl => rl == item)) return ReturnError("Please assign Non API roles.");
                    }
                    var getDomains = db.Query<string>("select AllowedDomain from Common.ClientDomains").ToList();
                    if (!getDomains.Any(g => g == mailAddress.Host)) ReturnError("Domain not registered.");
                }

                var ust = new UserStore<IdentityUser>();
                if (!user.Email.IsNullOrWhiteSpace())
                {
                    var edata = ust.FindByEmailAsync(user.Email).Result;
                    if (edata != null) return ReturnError("Email is already exists.");

                }
                var udata = ust.FindByName(user.UserName);
                if (udata != null) return ReturnError("Username already exists.");

                user.RequestedOn = DateTime.UtcNow;
                user.ClosedOn = (user.Status == AMUserStatus.Approved || user.Status == AMUserStatus.Rejected) ? user.RequestedOn : user.ClosedOn;
                user.ClosedBy = userId;

                var rst = new RoleStore<IdentityRole>();
                var roleList = rst.Roles;
                user.Id = Guid.NewGuid().ToString();
                db.Open();
                using (var t = db.BeginTransaction())
                {
                    string query = "INSERT INTO Profile.Users (Id ,Email ,EmailConfirmed ,PasswordHash ,SecurityStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount ,UserName ,ClientId ,FirstName ,LastName ,DisplayName ,RequestedOn ,Status ,ClosedOn ,ClosedBy) VALUES (@Id ,@Email ,@EmailConfirmed ,@PasswordHash ,@SecurityStamp,0,0,0 ,@LockoutEnabled ,@UserName ,@ClientId ,@FirstName ,@LastName ,@DisplayName ,@RequestedOn ,@Status ,@ClosedOn ,@ClosedBy)";
                    db.Execute(query, user, t);
                    foreach (var item in user.Roles)
                    {
                        string rq = "INSERT INTO Profile.UserRoles(UserId,RoleId) VALUES (@userId,@roleId)";
                        db.Execute(rq, new { userId = user.Id, roleId = rst.Roles.Single(rl => rl.Name == item).Id }, t);
                    }
                    t.Commit();
                }
                return ReturnSuccess("User created successfully.");
            }
            catch (FormatException)
            {
                return ReturnError("Invalid email address.");
            }
            catch (Exception)
            {
                return ReturnError("Error Occured. Please try again.");
            }
        }



        public async Task UpdateChangePasswordDateAsync(string userId)
        {
            await db.ExecuteAsync("update Profile.Users set ChangePasswordDateUTC=@utcDate where Id=@userId", new { userId = userId, utcDate = DateTime.UtcNow });
        }
        public async Task<IEnumerable<AMAllUsers>> GetAllUsers()
        {
            string sql = @"with users as
(
	select * from Profile.Users
),
roles as
(
select u.Id,STRING_AGG(r.Name,', ') as Roles from users u
left outer join Profile.UserRoles ur on u.Id=ur.UserId
left outer join Profile.Roles r on ur.RoleId=r.Id
group by u.Id
),
clients as
(
	select cu.UserId, STRING_AGG(c.AppCode, ', ') as Clients from Common.ClientUserMapping cu
	inner join Common.Clients c on cu.ClientId=c.Id
	where cu.Status=@requestStatus
	group by cu.UserId
)
select u.*,r.Roles,c.Clients from users u
left outer join roles r on u.Id=r.Id
left outer join clients c on u.Id=c.UserId
";
            var output = await db.QueryAsync<AMAllUsers>(sql, new { requestStatus = ClientUserMappingStatusTypes.Approved });
            return output;
        }
    }
}
