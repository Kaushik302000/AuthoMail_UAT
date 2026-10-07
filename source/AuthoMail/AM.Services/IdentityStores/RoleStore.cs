using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;
using Microsoft.AspNet.Identity;
using AM.Services;

namespace AM.Services
{
    public class RoleStore<TRole> : ServiceBase, IQueryableRoleStore<TRole> where TRole : IdentityRole
    {
        private AppRole appRole;
        public RoleStore()
        {
            appRole = new AppRole(db);
        }

        public IQueryable<TRole> Roles
        {
            get
            {
                var result = appRole.GetAllRole();
                return (IQueryable<TRole>)result;
            }
        }

        public Task CreateAsync(TRole role)
        {
            if (role == null) throw new ArgumentNullException("role");

            var result = appRole.Insert(role);
            if (result != 0)
            {
                return Task.FromResult<Object>(result);
            }

            return Task.FromResult<Object>(null);
        }

        public Task DeleteAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = appRole.Delete(role.Id);
            if (result != 0)
            {
                return Task.FromResult<Object>(result);
            }
            return Task.FromResult<Object>(null);
        }

        public Task<TRole> FindByIdAsync(string roleId)
        {
            TRole result = appRole.GetRoleById(roleId) as TRole;

            return Task.FromResult<TRole>(result);
        }

        public Task<TRole> FindByNameAsync(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException("role");

            TRole result = appRole.GetRoleByName(roleName) as TRole;

            return Task.FromResult<TRole>(result);

        }

        public Task UpdateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = appRole.Update(role);
            if (result != 0)
            {
                return Task.FromResult<Object>(result);
            }

            return Task.FromResult<Object>(null);
        }
        public Task GetAllRole()
        {
            return Task.FromResult<Object>(null);
        }

    }
}
