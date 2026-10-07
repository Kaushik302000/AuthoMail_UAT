using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ClientDomains
    {
        public int ClientId { get; set; }
        public string AllowedDomain { get; set; }
        public string ClientName { get; set; }
    }
    public class UserRequestSignUp
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; }
    }
    public class EmailAllowedDomain
    {
        public string Email { get; set; }
        public string AllowedDomain { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
    public class TrackingDetailViewModel
    {
        public string TrackingNo { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
    }

    public class DocTypeCoverLetterVM
    {
        public int DocTypeId { get; set; }
        public int SeqId { get; set; }
        public string CoverLetterTemplatePath { get; set; }
    }
    public class ClientIdNameVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AppCode { get; set; }
        public string GroupAlias { get; set; }
        public string NameAppCode
        {
            get
            {
                return $"{this.Name} ({AppCode})";
            }
        }
    }
    public class ClientUserMapping
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedById { get; set; }
    }
}
