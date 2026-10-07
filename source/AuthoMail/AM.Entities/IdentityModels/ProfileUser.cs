using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Entities
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ProfileUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ProfileUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            //userIdentity.AddClaim(new Claim("clientId", ClientId == null ? string.Empty : ClientId.ToString()));
            userIdentity.AddClaim(new Claim("ClientId", this.ClientId.ToString()));
            userIdentity.AddClaim(new Claim("IsCoreLogicUser", this.Email));
            userIdentity.AddClaim(new Claim("DisplayName", this.DisplayName == null ? "Hello" : this.DisplayName));
            userIdentity.AddClaim(new Claim("LastLoginTime", this.LastLoginTimeUTC.HasValue ? this.LastLoginTimeUTC.ToString() : DateTime.UtcNow.ToString()));
            userIdentity.AddClaim(new Claim("SFTChannelId", this.SFTChannelId.HasValue ? this.SFTChannelId.ToString() : string.Empty));
            return userIdentity;
        }

    }
    public static class IdentityExtensions
    {
        public static int? GetClientId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("clientId");
            // Test for null to avoid issues during local testing
            if (claim == null || claim.Value.IsNullOrWhiteSpace()) return null;
            return Convert.ToInt32(claim.Value);
        }
        public static bool IsCoreLogicUser(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("IsCoreLogicUser");
            if (claim == null || claim.Value.IsNullOrWhiteSpace()) return false;
            var mailAddress = new MailAddress(claim.Value);
            return mailAddress.Host.Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase) || mailAddress.Host.Equals("cotality.com", StringComparison.CurrentCultureIgnoreCase) ? true : false;
        }
        public static string DisplayName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("DisplayName");
            if (claim == null || claim.Value.IsNullOrWhiteSpace()) return string.Empty;
            return claim.Value;
        }
        public static DateTime LastLoginTime(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("LastLoginTime");
            if (claim == null || claim.Value.IsNullOrWhiteSpace()) return DateTime.UtcNow;
            return Convert.ToDateTime(claim.Value);
        }

        public static int? SFTChannelId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("SFTChannelId");
            if (claim == null || claim.Value.IsNullOrWhiteSpace()) return null;
            return Convert.ToInt32(claim.Value);
        }
    }

}
