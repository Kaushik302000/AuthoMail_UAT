using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using AM.Scanner.Models;
using System.Web.Security;
using System.Configuration;

namespace AM.Scanner.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {

        public AccountController()
        {
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            using (new PerfTimerLogger("Login[AccountController]: "))
            {
                if (ModelState.IsValid)
                {
                    string environment = ConfigurationManager.AppSettings["Environment"];
                    if (!String.IsNullOrWhiteSpace(environment) && environment.Equals("UAT", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (model.Username.Equals("Admin", StringComparison.CurrentCultureIgnoreCase) && model.Password == "Admin@123")
                        {
                            Session["DisplayName"] = "Admin";
                            FormsAuthentication.SetAuthCookie("Admin", false);
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    model.Username = model.Username.IndexOf('@') != -1 ? model.Username.Split('@').First() : model.Username;
                    var result = CLAuthentication.LdapLogin(model.Username, model.Password);
                    if (!result.Key)
                    {
                        ModelState.AddModelError("", result.Value);
                        return View(model);
                    }
                    var nm = result.Value.Split('|');
                    Session["DisplayName"] = nm[0];
                    FormsAuthentication.SetAuthCookie(model.Username, false);
                    return RedirectToAction("Index", "Home");
                }
                return View(model);
            }
        }
        //private ActionResult RedirectToLocal(string returnUrl)
        //{
        //    //if (Url.IsLocalUrl(returnUrl))
        //    //{
        //    //    return Redirect(returnUrl);
        //    //}
        //    return RedirectToAction("Index", "Home");
        //}
        [HttpPost]
        public ActionResult LogOff()
        {
            using (new PerfTimerLogger("LogOff[AccountController]: "))
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }
        }

    }
}