using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Scanner.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        // GET: Reports
        public ActionResult ScanedDocuments(string clientCode)
        {
            ViewBag.clientCode = clientCode;
            return View();
        }

        public ActionResult PendingScanning(string clientCode)
        {
            ViewBag.clientCode = clientCode;
            return View();
        }
    }
}