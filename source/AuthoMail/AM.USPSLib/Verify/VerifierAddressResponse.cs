using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    public class VerifierAddressResponse
    {
        public List<AddressResponse> AddressResponses { get; private set; }
        public string Error { get; set; }
        public bool HasError { get { return !string.IsNullOrWhiteSpace(this.Error); } }

        public VerifierAddressResponse(List<AddressResponse> responseList)
        {
            this.AddressResponses = responseList;
        }
        public VerifierAddressResponse()
        {
            this.AddressResponses = new List<AddressResponse>();
        }
    }

    public class AddressResponse
    {
        public string Id { get; set; }
        public string FirmName { get; set; }

        public string AptOrSuiteorOther { get; set; }

        public string StreetAddress { get; set; }

        public string StreetAddressAbbr { get; set; }

        public string City { get; set; }

        public string CityAbbr { get; set; }

        public string State { get; set; }

        public string Urbanization { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        public string Zip { get { return string.Join("-", new string[] { Zip5, Zip4 }.Where(s => !string.IsNullOrWhiteSpace(s))); } }

        public string DeliveryPoint { get; set; }

        public string CarrierRoute { get; set; }

        public string Warning { get; set; }

        public USPSError Error { get; set; }

        //public bool IsAddressVerified { get; set; }

        public List<AddressType> AddressTypes { get; set; }

        //public string StateType { get; set; }

        public string Verifier { get; set; }

        public string VerifierDesc { get; set; }

        public bool HasError
        {
            get
            {
                if (this.Error == null) return false;
                return !(String.IsNullOrWhiteSpace(this.Error.Number) &&
                         String.IsNullOrWhiteSpace(this.Error.Description));
            }
        }


        public AddressRequest AddressRequest { get; set; }

        public AddressResponse(AddressRequest request, int order)
        {
            this.AddressRequest = request;
            this.Id = string.Format("{0}{1}{2}", order, ResponseSource.CLSVC, request == null ? "" : request.Id);
        }
        public AddressResponse()
        {

        }
    }
}
