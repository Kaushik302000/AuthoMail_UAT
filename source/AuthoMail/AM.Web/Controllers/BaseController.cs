using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    public class BaseController : Controller
    {
        protected new virtual  JsonResult Json(object data,JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Config.Web_MaxJsonLength
            };
        }
    }
}