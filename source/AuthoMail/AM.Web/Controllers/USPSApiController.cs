using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using AM.Entities;
using AM.Services;
using AM.USPSLib;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    //[Authorize(Roles = AMRoles.API)]
    [RoutePrefix("api/usps")]
    public class USPSApiController : ApiController
    {
        IUSPSService uspsSvc;
        public USPSApiController(IUSPSService uspsSvc)
        {
            this.uspsSvc = uspsSvc;
        }

        [Route("verify")]
        [HttpPost]
        public async Task<VerifierAddressResponse> VerifyAddress([FromBody] VerifierAddressRequest addressRequest)
        {
            return await uspsSvc.VerifyAddressAsync(User.Identity.GetUserId(), addressRequest).ConfigureAwait(false);
        }

        [Route("eVS")]
        [HttpPost]
        public async Task<eVSLabelResponse> RequesteVSLabel([FromBody] eVSLabelRequest request)
        {
            //We are passing UspsUserID as "api" to fetch the account details of API user based on Cut-off
            return await uspsSvc.RequesteVSLabelAsync(User.Identity.GetUserId(), request, new USPSCredentialInfo() { PrintLocation = "api" }).ConfigureAwait(false);
        }

        [Route("eVSCancel")]
        [HttpPost]
        public async Task<eVSCancelLabelResponse> RequesteVSLabel([FromBody] eVSCancelLabelRequest request)
        {
            return await uspsSvc.RequesteVSCancelLabelAsync(User.Identity.GetUserId(), request).ConfigureAwait(false);
        }
    }
}
