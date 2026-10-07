using AM.Entities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class UserStore<TUser> : ServiceBase,
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
      IUserRoleStore<TUser>,
      IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserEmailStore<TUser>,
        IUserPhoneNumberStore<TUser>,
      IUserTwoFactorStore<TUser, string>,
      IUserLockoutStore<TUser, string>,
      IUserStore<TUser>
      where TUser : IdentityUser
    {
        private AppUser<TUser> appUser;
        private AppRole appRole;
        private AppUserRole appUserRole;
        private AppUserClaims appUserClaim;
        private AppUserLogins appUserLogin;

        public IQueryable<TUser> Users
        {
            get
            {
                return appUser.GetAllUsers() as IQueryable<TUser>;
            }
        }
        public UserStore()
        {
            appUser = new AppUser<TUser>(db);
            appRole = new AppRole(db);
            appUserRole = new AppUserRole(db);
            appUserClaim = new AppUserClaims(db);
            appUserLogin = new AppUserLogins(db);
        }

        public Task CreateAsync(TUser user)
        {
            if (user == null) throw new ArgumentNullException("user");
            int result = appUser.AddNewUser(user);
            return Task.FromResult(result);
        }

        public Task DeleteAsync(TUser user)
        {
            throw new NotImplementedException();
        }

        public Task<TUser> FindByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("Null or empty argument: userId");
            }

            TUser result = appUser.GetUserById(userId) as TUser;
            if (result != null)
            {
                return Task.FromResult<TUser>(result);
            }

            return Task.FromResult<TUser>(null);
        }

        public Task<TUser> FindByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Null or empty argument: userName");
            }

            List<TUser> result = appUser.GetUserByName(userName) as List<TUser>;

            // Should I throw if > 1 user?
            if (result != null && result.Count == 1)
            {
                return Task.FromResult<TUser>(result[0]);
            }

            return Task.FromResult<TUser>(null);
        }
        public TUser FindByName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Null or empty argument: userName");
            }

            List<TUser> result = appUser.GetUserByName(userName) as List<TUser>;

            // Should I throw if > 1 user?
            if (result != null && result.Count == 1)
            {
                return result[0];
            }

            return null;
        }

        public Task UpdateAsync(TUser user)
        {
            if (user == null) throw new ArgumentNullException("user");
            var result = appUser.Update(user);
            return Task.FromResult<object>(null);
        }


        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            user.AccessFailedCount++;
            appUser.Update(user);

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            user.AccessFailedCount = 0;
            appUser.Update(user);

            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            appUser.Update(user);

            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            if (lockoutEnd.UtcDateTime <= DateTime.UtcNow)
            {
                user.LockoutEndDateUtc = null;
            }
            else
            {
                user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            }
            appUser.Update(user);
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            string passwordHash = appUser.GetPasswordHash(user.Id);

            return Task.FromResult<string>(passwordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            var hasPassword = !string.IsNullOrEmpty(appUser.GetPasswordHash(user.Id));

            return Task.FromResult<bool>(Boolean.Parse(hasPassword.ToString()));
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;

            return Task.FromResult<Object>(null);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            appUser.Update(user);

            return Task.FromResult(0);
        }


        public Task AddToRoleAsync(TUser user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException("Argument cannot be null or empty: roleName.");
            }

            string roleId = appRole.GetRoleId(roleName);
            if (!string.IsNullOrEmpty(roleId))
            {
                appUserRole.AssignNewRoleToUser(user, roleId);
            }

            return Task.FromResult<object>(null);
        }

        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            List<string> roles = appUserRole.GetAllRoleNameByUserId(user.Id);
            {
                if (roles != null)
                {
                    return Task.FromResult<IList<string>>(roles);
                }
            }

            return Task.FromResult<IList<string>>(null);
        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("role");
            }

            List<string> roles = appUserRole.GetAllRoleNameByUserId(user.Id);
            {
                if (roles != null && roles.Contains(roleName))
                {
                    return Task.FromResult<bool>(true);
                }
            }

            return Task.FromResult<bool>(false);
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            if (user == null && user.Id == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("role");
            }
            string roleId = appRole.GetRoleId(roleName);
            appUserRole.RemoveRoleByUserIdAndRoleId(user.Id, roleId);
            return Task.FromResult<Object>(null);
        }




        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            appUserLogin.AddNewLogin(user, login);

            return Task.FromResult<object>(null);
        }

        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            var userId = appUserLogin.FindUserIdByLogin(login);
            if (userId != null)
            {
                TUser user = appUser.GetUserById(userId) as TUser;
                if (user != null)
                {
                    return Task.FromResult<TUser>(user);
                }
            }

            return Task.FromResult<TUser>(null);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            List<UserLoginInfo> userLogins = new List<UserLoginInfo>();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            List<UserLoginInfo> logins = appUserLogin.FindUserLoginInfoByUserId(user.Id);
            if (logins != null)
            {
                return Task.FromResult<IList<UserLoginInfo>>(logins);
            }

            return Task.FromResult<IList<UserLoginInfo>>(null);
        }

        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            appUserLogin.RemoveLoginBuUserAndLogin(user, login);

            return Task.FromResult<Object>(null);
        }

        public Task AddClaimAsync(TUser user, System.Security.Claims.Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("user");
            }

            appUserClaim.Insert(claim, user.Id);

            return Task.FromResult<object>(null);
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            ClaimsIdentity identity = appUserClaim.FindByUserId(user.Id);

            return Task.FromResult<IList<Claim>>(identity.Claims.ToList());
        }

        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            appUserClaim.RemoveClaimByUserAndClaim(user, claim);

            return Task.FromResult<object>(null);
        }

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<TUser> FindByEmailAsync(string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }

            TUser result = appUser.GetUserByEmail(email) as TUser;
            if (result != null)
            {
                return Task.FromResult<TUser>(result);
            }

            return Task.FromResult<TUser>(null);
        }

        public Task<string> GetEmailAsync(TUser user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailAsync(TUser user, string email)
        {
            user.Email = email;
            appUser.Update(user);
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            appUser.Update(user);
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            user.PhoneNumber = phoneNumber;
            appUser.Update(user);

            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            appUser.Update(user);

            return Task.FromResult(0);
        }
    }
}
