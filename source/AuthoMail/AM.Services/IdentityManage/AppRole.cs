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
    public class AppRole
    {
        private IDbConnection db;
        public AppRole(IDbConnection db)
        {
            this.db = db;
        }
        public int Delete(string roleId)
        {
            if (roleId == null) throw new ArgumentNullException();
            string query = "Delete from [Profile].[Roles] where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", roleId);
            return db.Execute(query, parameters);
        }
        public int Insert(IdentityRole role)
        {
            if (role == null) throw new ArgumentNullException();
            var existingRole = GetRoleByName(role.Name);
            if (existingRole != null) throw new Exception("Role name already exist.");
            string query = "Insert into [Profile].[Roles] (Id, Name) values (@id, @name)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@name", role.Name);
            parameters.Add("@id", role.Id);

            return db.Execute(query, parameters);
        }
        public string GetRoleName(string roleId)
        {
            if (roleId == null) throw new ArgumentNullException();
            string query = "Select Name from [Profile].[Roles] where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", roleId);
            return db.QueryFirstOrDefault<string>(query, parameters);
        }
        public string GetRoleId(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException();
            string query = "Select Id from [Profile].[Roles] where Name = @name";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", roleName } };
            return db.QueryFirstOrDefault<string>(query, parameters);

        }
        public IdentityRole GetRoleById(string roleId)
        {
            if (roleId == null) throw new ArgumentNullException();
            var roleName = GetRoleName(roleId);
            IdentityRole role = null;

            if (roleName != null)
            {
                role = new IdentityRole(roleName, roleId);
            }
            return role;

        }
        public IdentityRole GetRoleByName(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException();
            var roleId = GetRoleId(roleName);
            IdentityRole role = null;

            if (roleId != null)
            {
                role = new IdentityRole(roleName, roleId);
            }

            return role;
        }
        public int Update(IdentityRole role)
        {
            if (role == null) throw new ArgumentNullException();
            string query = "Update [Profile].[Roles] set Name = @name where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", role.Id);

            return db.Execute(query, parameters);
        }

        public IQueryable<IdentityRole> GetAllRole()
        {
            string query = "Select Id,Name from [Profile].[Roles]";
            var roles = db.Query<IdentityRole>(query).AsQueryable();
            return roles;
        }
    }
}
