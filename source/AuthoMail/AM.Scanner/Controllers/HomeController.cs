using AM.Scanner.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Scanner.Models;
using System.IO;
using System.Configuration;
using AM.Services;

namespace AM.Scanner.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        IScannerService scannerSvc;

        public HomeController()
        {
            this.scannerSvc = new ScannerService();
        }
        public ActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult GetAllBatchForScan(string clientCode)
        //{
        //    var model = DBHelper.GetRecentBatchesStats(clientCode.Trim());
        //    return Json(model, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet]
        public ActionResult GetScanCount()
        {
            using (new PerfTimerLogger("GetScanCount[HomeController]: "))
            {
                var model = scannerSvc.GetScanStats();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [AllowAnonymous, HttpPost]
        public ActionResult SaveConverLetter()
        {
            try
            {

                if (Request.Headers["Id"].Trim() == "c711c4aa-8559-44fa-9edc-88600f38b3c9")
                {
                    string path = Path.Combine(ConfigurationManager.AppSettings["CoverLetterPath"].ToString(), Request.Headers["AppCode"].Trim());
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    Request.Files[0].SaveAs(Path.Combine(path, Request.Headers["FileName"].Trim()));
                    return Json("File save successfully.", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Url is not valid.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous, HttpPost]
        public ActionResult SaveDelfusTemplate()
        {
            try
            {

                if (Request.Headers["Id"].Trim() == "5b189031-eecc-42d7-8b6d-b531caf1036e")
                {
                    string path = Path.Combine(ConfigurationManager.AppSettings["DelfusTemplatePath"].ToString(), "DelfusTemplates");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    Request.Files[0].SaveAs(Path.Combine(path, Request.Headers["FileName"].Trim()));
                    return Json(JsonResponseFactory.SuccessResponse("File save successfully."), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse("Url is not valid."), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
    }
}