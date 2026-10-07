using AM.Entities;
using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AM.Services
{
    public class AccountUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool LockoutEnabled { get; set; }
        public int? ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedOn { get; set; }
        public string ClosedBy { get; set; }
        public List<string> Roles { get; set; }
        public virtual bool IsAPIUser { get; set; }
    }
    public class AMAllUsers
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public int? ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedOn { get; set; }
        public string ClosedBy { get; set; }
        public string Roles { get; set; }
        public string Clients { get; set; }
        public DateTime? LastLoginTimeUTC { get; set; }
        public bool IsBAOrClient
        {
            get
            {
                if (Roles.IsNullOrWhiteSpace()) return false;
                return (Roles.Contains(AMRoles.Client) || Roles.Contains(AMRoles.BA));
            }
        }
    }
    public class AMAllUsersReportVM
    {
        public string ClientName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string DisplayName { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; }
        public DateTime? LastLoginTimeUTC { get; set; }
    }
}
