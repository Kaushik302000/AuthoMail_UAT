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
    public class ManageClientsController : Controller
    {
        ISupportService supportSvc;

        public ManageClientsController(ISupportService supportSvc)
        {
            this.supportSvc = supportSvc;
        }
        // GET: ManageClients



        public ActionResult SignUpRequest()
        {
            return View();
        }

        public ActionResult GetSingUpRequestedList()
        {
            var model = supportSvc.GetUsersRequestSignUp("");
            return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateUsersRequestSignUp(string userId, string status)
        {
            try
            {
                var model = supportSvc.UpdateUsersRequestSignUp("1", userId, status);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
    }
}