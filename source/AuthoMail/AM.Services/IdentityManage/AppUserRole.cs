using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;

namespace AM.Services
{
    public class AppUserRole
    {
        private IDbConnection db;
        public AppUserRole(IDbConnection db)
        {
            this.db = db;
        }
        //Get List of role names by userid
        public List<string> GetAllRoleNameByUserId(string userId)
        {
            if (userId == null) throw new ArgumentNullException();
            string query = "Select Roles.Name from [Profile].[UserRoles], [Profile].[Roles] where UserRoles.UserId = @userId and UserRoles.RoleId = Roles.Id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);

            return db.Query<string>(query, parameters).ToList();
        }
        //Delete all role which assigned to user.
        public int RemoveAllRoleByUserId(string userId)
        {
            if (userId == null) throw new ArgumentNullException();
            string query = "Delete from [Profile].[UserRoles] where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("UserId", userId);
            return db.Execute(query, parameters);
        }
        //Assign new role to user
        public int AssignNewRoleToUser(IdentityUser user, string roleId)
        {
            if (user == null || roleId == null) throw new ArgumentNullException();
            string query = "Insert into [Profile].[UserRoles] (UserId, RoleId) values (@userId, @roleId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", user.Id);
            parameters.Add("roleId", roleId);
            return db.Execute(query, parameters);
        }
        //Remove particular role by UserId and RoleId
        public int RemoveRoleByUserIdAndRoleId(string userId, string roleId)
        {
            string commandText = "Delete from [Profile].[UserRoles] where UserId = @userId and RoleId= @roleId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("UserId", userId);
            parameters.Add("roleId", roleId);
            return db.Execute(commandText, parameters);
        }
    }
}
