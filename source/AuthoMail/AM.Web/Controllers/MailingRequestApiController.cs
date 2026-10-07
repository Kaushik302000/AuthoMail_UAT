using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AM.Web.Controllers
{
    [RoutePrefix("api/mailingrequest")]
    public class MailingRequestApiController : ApiController
    {
        public MailingRequestApiController()
        {

        }

        [Route("GetDateTime")]
        [HttpPost]
        public object GetDateTime()
        {
            return DateTime.Now;
        }


        [Route("test")]
        [HttpGet]
        public MailPiece Test()
        {
            return new MailPiece()
            {
                DocumentOrPackage = new DocumentOrPackage(),
                // eDelivery = new eDelivery(),
                RecipientInfo = new RecipientInfo() { Recipient = new Recipient(), Address = new Address() },
                ReturnInfo = new ReturnInfo() { Recipient = new Recipient(), Address = new Address() },
                ShippingInfo = new ShippingInfo() { Carrier = new Carrier(), ServiceType = new ServiceType(), AccountCode = null },
                CustomInfo = new List<Metadata>() { new Metadata() }
            };
        }
    }
}
