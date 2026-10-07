using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    public class VerifierAddressRequest
    {
        public List<AddressRequest> AddressRequests { get; set; }
    }

    public class AddressRequest
    {
        public string Id { get; set; }

        public string FirmName { get; set; }

        /// <summary>
        /// Address Line 1 is used to provide an apartment or suite number, if applicable
        /// </summary>
        public string AptOrSuiteorOther { get; set; }

        /// <summary>
        /// Street address
        /// </summary>
        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        /// <summary>
        /// For Puerto Rico addresses only.
        /// </summary>
        public string Urbanization { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        public AddressRequest()
        {
            // Initialize all string properties to empty string. Required to have them serialized.
            foreach (var propInfo in typeof(AddressRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (propInfo.PropertyType == typeof(string) && propInfo.CanWrite)
                {
                    propInfo.SetValue(this, string.Empty);
                }
            }
        }
    }
}
