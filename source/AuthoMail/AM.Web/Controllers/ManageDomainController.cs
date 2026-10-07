using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin)]
    public class ManageDomainController : Controller
    {
        // GET: ManageDomain
        ISupportService supportSvc;
        IHomeService homeSvc;
        IConfigService confSvc;
        public ManageDomainController(ISupportService supportSvc, IHomeService homeSvc, IConfigService confSvc)
        {
            this.supportSvc = supportSvc;
            this.homeSvc = homeSvc;
            this.confSvc = confSvc;
        }
        public ActionResult Index()
        {
            var model = homeSvc.GetAllActiveClient();
            ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
            return View();
        }
        [HttpGet]
        public ActionResult GetDomains()
        {
            try
            {
                var model = supportSvc.GetClientDomains();

                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddDomain(int clientId, string domainName)
        {

            try
            {
                var model = supportSvc.CreateClientDomain(clientId, domainName);
                if (model > 0)
                {

                    return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse("Error while adding domain name."), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult SystemSettings()
        {
            return View();
        }
        public JsonResult GetAllSystemSettings()
        {
            var model = confSvc.GetSystemSettings();
            return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        }
    }
}