using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.USPSLib;
using AM.USPSLib.TrackV2;
using AM.Services.PB;

namespace AM.Services
{
    public interface IUSPSService : IDisposable
    {
        Task<VerifierAddressResponse> VerifyAddressAsync(string userId, VerifierAddressRequest request);

        Task<eVSLabelResponse> RequesteVSLabelAsync(string userId, eVSLabelRequest request, USPSCredentialInfo uspsUserInfo);

        Task<eVSLabelResponse> RequestPBLabelAsync(string userId, PierbridgeShipRequest request, USPSCredentialInfo uspsUserInfo);

        Task<eVSCancelLabelResponse> RequesteVSCancelLabelAsync(string userId, eVSCancelLabelRequest request);

        Task<TrackAPIResponse> GetTrackingDetailsAsync(string userId, TrackFieldAPIRequest verifyRequest);
    }
}
