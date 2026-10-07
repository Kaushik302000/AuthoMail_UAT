using AM.USPSLib.TrackV2;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    public interface IUSPS
    {
        [Post("/api/usps/verify")]
        [Headers("Authorization: Bearer")]
        Task<VerifierAddressResponse> VerifyAddressAsync([Body(BodySerializationMethod.Json)] VerifierAddressRequest request);

        [Post("/api/usps/eVS")]
        [Headers("Authorization: Bearer")]
        Task<eVSLabelResponse> RequesteVSLabelAsync([Body(BodySerializationMethod.Json)] eVSLabelRequest request);

        [Post("/api/usps/eVSCancel")]
        [Headers("Authorization: Bearer")]
        Task<eVSCancelLabelResponse> RequesteVSCancelLabelAsync([Body(BodySerializationMethod.Json)] eVSCancelLabelRequest request);

        [Post("/api/usps/gettrackingdetails")]
        [Headers("Authorization: Bearer")]
        Task<TrackAPIResponse> GetTrackingDetailsAsync([Body(BodySerializationMethod.Json)] TrackFieldAPIRequest request);
    }
}
