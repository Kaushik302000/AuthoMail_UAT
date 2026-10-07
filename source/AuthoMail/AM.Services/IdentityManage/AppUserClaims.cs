using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;
using System.Data;
using System.Security.Claims;

namespace AM.Services
{
    public class AppUserClaims
    {
        private IDbConnection db;
        public AppUserClaims(IDbConnection db)
        {
            this.db = db;
        }
        //Get all Claims by userid
        public ClaimsIdentity FindByUserId(string userId)
        {
            if (userId == null) throw new ArgumentNullException();
            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            string query = "Select * from [Profile].[UserClaims] where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@UserId", userId } };
            var claims = db.Query(query, parameters);
            foreach (var item in claims)
            {
                Claim clm = new Claim(item["ClaimType"], item["ClaimValue"]);
                claimsIdentity.AddClaim(clm);
            }
            return claimsIdentity;
        }
        //Delete all claims for user by userid.
        public int RemoveClaimByUserId(string userId)
        {
            if (userId == null) throw new ArgumentNullException();
            string query = "Delete from [Profile].[UserClaims] where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", userId);
            return db.Execute(query, parameters);
        }
        //Assign claim to user by userid and claims
        public int Insert(Claim userClaim, string userId)
        {
            string query = "Insert into [Profile].[UserClaims] (ClaimValue, ClaimType, UserId) values (@value, @type, @userId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("value", userClaim.Value);
            parameters.Add("type", userClaim.Type);
            parameters.Add("userId", userId);
            return db.Execute(query, parameters);
        }
        //Remove particular claim by user and claim
        public int RemoveClaimByUserAndClaim(IdentityUser user, Claim claim)
        {
            string query = "Delete from [Profile].[UserClaims] where UserId = @userId and @ClaimValue = @value and ClaimType = @type";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", user.Id);
            parameters.Add("value", claim.Value);
            parameters.Add("type", claim.Type);
            return db.Execute(query, parameters);
        }
    }
}
