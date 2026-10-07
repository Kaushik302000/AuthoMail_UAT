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
using AM.Web.Models;
using AM.Entities;
using AM.Services;
using System.Net.Mail;
using System.Text;
using AM.Lib;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using AutoMapper;
using WebGrease.Css.Extensions;

namespace AM.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private AMSignInManager _signInManager;
        private AMUserManager _userManager;
        private readonly ISupportService supportSvc;
        private readonly IAccountManageService accSvc;
        private readonly IClientPermissionService clientPermissionSvc;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AccountController()
        {
        }

        public AccountController(AMUserManager userManager, AMSignInManager signInManager, ISupportService supportSvc, IAccountManageService accSvc, IClientPermissionService clientPermissionSvc)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            this.supportSvc = supportSvc;
            this.accSvc = accSvc;
            this.clientPermissionSvc = clientPermissionSvc;
        }

        public AMSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<AMSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public AMUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AMUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index(string message)
        {
            ViewBag.StatusMessage = message;
            return View();
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //ViewBag.clientIP = GetIp();
            //ViewBag.clientIP = Request.UserHostAddress;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        public string GetIp()
        {
            string ipAddress = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = ipAddress.Split(':');
                if (addresses.Length != 0)
                    return addresses[0];
            }

            return System.Web.HttpContext.Current.Request.UserHostAddress;
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                var mailAddress = new MailAddress(model.UserName);
                    if (mailAddress.Host.Equals("us.hsbc.com", StringComparison.CurrentCultureIgnoreCase) || mailAddress.Host.Equals("hsbc.com.mx", StringComparison.CurrentCultureIgnoreCase))
                {
                    string clientIP = GetIp();
                    IPNetwork ipnetwork;
                    IPAddress ipaddress;
                    bool isIPInRange = false;
                    foreach (var ipRange in Config.HSBC_WhiteListedIPs.Split(',').ToList())
                    {
                        ipnetwork = IPNetwork.Parse(ipRange);
                        ipaddress = IPAddress.Parse(clientIP);

                        if (ipnetwork.Contains(ipaddress))
                        {
                            isIPInRange = true;
                            break;
                        }
                    }
                    if (!isIPInRange)
                    {
                        ModelState.AddModelError("", string.Format(Config.Web_Msg_Login_IPRestricted, clientIP));
                        return View(model);
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }                

                var user = UserManager.FindByEmail(model.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("", Config.Web_Msg_Login_InvalidUserName);
                    return View(model);
                }

                bool isCoreLogicUser = false;
                if (user.Email != null && user.Email != "")
                {
                    isCoreLogicUser = CLAuthentication.HasCorelogicAccount(user.Email);
                }

                if (isCoreLogicUser)
                {
                    if (Config.CLAuth_EnabledAxisAuthentication)
                    {
                        var isValidUser = await CLAuthentication.IsValidCLUser(model.UserName);
                        if (!isValidUser.Key)
                        {
                            ModelState.AddModelError("", isValidUser.Value);
                            return View(model);
                        }
                    }

                    var hasLogin = CLAuthentication.LdapLogin(model.UserName, model.Password);
                    if (!hasLogin.Key)
                    {
                        ModelState.AddModelError("", hasLogin.Value);
                        return View(model);
                    }

                    await SignInManager.SignInAsync(user, false, model.RememberMe);
                    if (!user.EmailConfirmed)
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        ModelState.AddModelError("", Config.Web_Msg_Login_EmailConfirm);
                        return View(model);
                    }
                    if ((user.Status.Equals(AMUserStatus.Open, StringComparison.CurrentCultureIgnoreCase) || user.Status.Equals(AMUserStatus.Rejected, StringComparison.CurrentCultureIgnoreCase) || user.Status.Equals(AMUserStatus.InActive, StringComparison.CurrentCultureIgnoreCase)) || !user.ClientId.HasValue)
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        return View("InActiveUser");
                    }
                    UpdateLoginTime(user.Id);
                    return RedirectToLocal(returnUrl);
                }

                var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: true);
                switch (result)
                {
                    case SignInStatus.Success:
                        if (!user.EmailConfirmed)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            ModelState.AddModelError("", Config.Web_Msg_Login_EmailConfirm);
                            ViewBag.confirmationlink = user.Email;
                            return View(model);
                        }
                        if ((user.Status.Equals(AMUserStatus.Open, StringComparison.CurrentCultureIgnoreCase) || user.Status.Equals(AMUserStatus.Rejected, StringComparison.CurrentCultureIgnoreCase) || user.Status.Equals(AMUserStatus.InActive, StringComparison.CurrentCultureIgnoreCase)) || !user.ClientId.HasValue)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View("InActiveUser");
                        }
                        if (!isCoreLogicUser && Config.Web_Login_PasswordExpirationIsEnable && user.PasswordExpirationDays > Config.Web_Login_PasswordExpirationDays)
                        {
                            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return RedirectToAction("ChangePassword", new { unm = user.UserName });
                        }
                        UpdateLoginTime(user.Id);
                        return RedirectToLocal(returnUrl);
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", Config.Web_Msg_Login_Default);
                        return View(model);
                }
            }
            catch (FormatException)
            {
                ModelState.AddModelError("", Config.Web_Msg_Login_InvalidUserName);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", Config.Web_Msg_Login_Default);
                return View(model);
            }
        }

        public string GetClientIpAddress(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                dynamic ctx = request.Properties["MS_HttpContext"];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            if (request.Properties.ContainsKey("System.ServiceModel.Channels.RemoteEndpointMessageProperty"))
            {
                dynamic remoteEndpoint = request.Properties["System.ServiceModel.Channels.RemoteEndpointMessageProperty"];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            return null;
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult ValidateEmail(string email)
        {
            EmailValidate emailValidate = new EmailValidate(email);
            if (emailValidate.IsValid)
            {
                var IsDomainExists = supportSvc.IsDomainExists(emailValidate.Host);
                if (!IsDomainExists)
                {
                    emailValidate.Message = string.Format("Users of the specified domain name '{0}' are not allowed to register.", emailValidate.Host);
                    emailValidate.IsValid = false;
                }
                if (UserManager.FindByEmail(email) != null)
                {
                    emailValidate.Message = string.Format("Specified email {0} already exists.", email);
                    emailValidate.IsValid = false;
                }
            }
            return Json(emailValidate, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous, HttpPost]
        public JsonResult CLAuth(string email, string password)
        {
            var ldap = CLAuthentication.LdapLogin(email, password);
            if (!ldap.Key)
            {
                return Json(JsonResponseFactory.ErrorResponse(ldap.Value), JsonRequestBehavior.AllowGet);
            }
            var nm = ldap.Value.Split('|');
            var data = new
            {
                FName = nm[0],
                LName = nm[1],
                DisplayName = nm[2]
            };
            return Json(JsonResponseFactory.SuccessResponse(data), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult CreateUser()
        {
            var r = new RoleStore<IdentityRole>();
            ViewBag.Roles = new SelectList(r.Roles.ToList(), "Name", "Name");
            ViewBag.Clients = new SelectList(supportSvc.GetClients().Where(c => c.IsAMActive).Select(c => new { c.Id, c.Name }).ToList(), "Id", "Name");
            var statusList = new List<SelectListItem>() {
                new SelectListItem { Text = AMUserStatus.Open, Value = AMUserStatus.Open },
                new SelectListItem { Text = AMUserStatus.Approved, Value = AMUserStatus.Approved },
                new SelectListItem { Text = AMUserStatus.Rejected, Value = AMUserStatus.Rejected } };
            ViewBag.Status = new SelectList(statusList, "Value", "Text");
            return View(new AccountUser());
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateUser(AccountUser user)
        {
            var result = accSvc.CreateUser(user, User.Identity.GetUserId());
            if (result.Key)
            {
                return View("Success");
            }
            else
            {
                var r = new RoleStore<IdentityRole>();
                ViewBag.Roles = new SelectList(r.Roles.ToList(), "Name", "Name");
                ViewBag.Clients = new SelectList(supportSvc.GetClients().Where(c => c.IsAMActive).Select(c => new { c.Id, c.Name }).ToList(), "Id", "Name");
                var statusList = new List<SelectListItem>() {
                new SelectListItem { Text = AMUserStatus.Open, Value = AMUserStatus.Open },
                new SelectListItem { Text = AMUserStatus.Approved, Value = AMUserStatus.Approved },
                new SelectListItem { Text = AMUserStatus.Rejected, Value = AMUserStatus.Rejected } };
                ViewBag.Status = new SelectList(statusList, "Value", "Text");
                ModelState.AddModelError("", result.Value);
                return View(user);
            }
        }
        public ActionResult Success()
        {
            return View();
        }
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool isCoreLogicUser = CLAuthentication.HasCorelogicAccount(model.Email);
                    if (isCoreLogicUser)
                    {
                        var ldap = CLAuthentication.LdapLogin(model.Email, model.Password);
                        if (!ldap.Key)
                        {
                            ModelState.AddModelError("", ldap.Value);
                            return View(model);
                        }
                        if (model.DisplayName.IsNullOrWhiteSpace())
                        {
                            ModelState.AddModelError("", "Display Name is required.");
                            return View(model);
                        }
                        if (model.DisplayName.Length > 254)
                        {
                            ModelState.AddModelError("", "Display name out of range.");
                            return View(model);
                        }
                        var nm = ldap.Value.Split('|');
                        var systemUser = UserManager.FindByName("system@milestonetechnoconsulting.com");
                        var user = new ProfileUser
                        {
                            Email = model.Email,
                            EmailConfirmed = true,
                            LockoutEnabled = true,
                            UserName = model.Email.Split('@').First(),
                            FirstName = nm[0],
                            LastName = nm[1],
                            DisplayName = model.DisplayName,
                            Status = AMUserStatus.Open,
                            RequestedOn = DateTime.UtcNow,
                            ChangePasswordDateUTC = DateTime.UtcNow,
                            StatusModifiedOn = DateTime.UtcNow,
                            StatusModifiedBy = systemUser.Id
                        };

                        var result = await UserManager.CreateAsync(user);
                        if (result.Succeeded)
                        {
                            //StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\CreateAccountCL.html")));
                            //sb.Replace("{{name}}", user.FirstName);
                            UserManager.AddToRole(user.Id, AMRoles.BA);
                            //await UserManager.SendEmailAsync(user.Id, "Welcome to AuthoMail.", sb.ToString());

                            StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\RegistrationInAuthoMail.html")));
                            string emails = string.Empty;

                            sb.Replace("{{url}}", Config.Common_WebUrl);
                            emails = supportSvc.GetEmailsFromClientDomain(user.Email);

                            sb.Replace("{{email}}", user.Email);

                            //Send email to specified admin for new registration request
                            await InternalEmail.SendMail(emails, null, Config.Web_RegistrationBCCEmails, 
                                Config.Common_Environment == "QA" ? Config.Common_Environment + " : Registration in AuthoMail"
                                : "Registration in AuthoMail", 
                                sb.ToString());
                            //Send email to client to waiting for approval
                            sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\WaitForApproval.html")));
                            sb.Replace("{{name}}", user.FirstName);
                            sb.Replace("{{email}}", user.Email);
                            sb.Replace("{{defaultemails}}", string.Join(", ", emails.Split(',')));
                            await InternalEmail.SendMail(user.Email, null, Config.Web_RegistrationBCCEmails, 
                                Config.Common_Environment == "QA" ? Config.Common_Environment + " : Welcome to AuthoMail" 
                                : "Welcome to AuthoMail"
                                , sb.ToString());
                            ViewBag.IsCoreLogic = true;
                            return View("SuccessRegistration");
                        }
                        AddErrors(result);
                    }
                    else
                    {
                        if (model.FirstName.IsNullOrWhiteSpace())
                        {
                            ModelState.AddModelError("", "First name is required.");
                            return View(model);
                        }
                        if (model.LastName.IsNullOrWhiteSpace())
                        {
                            ModelState.AddModelError("", "Last name is required.");
                            return View(model);
                        }
                        if (!Regex.IsMatch(model.FirstName, @"^[a-zA-Z]+$"))
                        {
                            ModelState.AddModelError("", "Invalid first name.");
                            return View(model);
                        }
                        if (!Regex.IsMatch(model.LastName, @"^[a-zA-Z]+$"))
                        {
                            ModelState.AddModelError("", "Invalid last name.");
                            return View(model);
                        }
                        if (model.FirstName.Length > 254)
                        {
                            ModelState.AddModelError("", "First name out of range.");
                            return View(model);
                        }
                        if (model.FirstName.Length > 254)
                        {
                            ModelState.AddModelError("", "Last name out of range.");
                            return View(model);
                        }
                        model.DisplayName = model.DisplayName.IsNullOrWhiteSpace() ?
                            string.Format("{0}, {1}", model.LastName, model.FirstName) :
                            model.DisplayName;
                        if (model.DisplayName.Length > 254)
                        {
                            ModelState.AddModelError("", "Display name out of range.");
                            return View(model);
                        }
                        MailAddress mAdd = new MailAddress(model.Email);
                        var IsDomainExists = supportSvc.IsDomainExists(mAdd.Host);
                        if (!IsDomainExists)
                        {
                            ModelState.AddModelError("", string.Format("Users of the specified domain name '{0}' are not allowed to register.", mAdd.Host));
                            return View(model);
                        }
                        var systemUser = UserManager.FindByName("system@milestonetechnoconsulting.com");
                        var user = new ProfileUser
                        {
                            UserName = model.Email,
                            Email = model.Email,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            DisplayName = model.DisplayName,
                            LockoutEnabled = true,
                            Status = AMUserStatus.Open,
                            RequestedOn = DateTime.UtcNow,
                            ChangePasswordDateUTC = DateTime.UtcNow,
                            StatusModifiedOn = DateTime.UtcNow,
                            StatusModifiedBy = systemUser.Id
                        };
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\CreateAccount.html")));
                            sb.Replace("{{name}}", user.FirstName);
                            //sb.Replace("{{email}}", user.Email);

                            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);

                            var u = new Uri(callbackUrl);
                            string str = string.Format("{0}://{1}", u.Scheme, u.Host);
                            callbackUrl = callbackUrl.Replace(str, Config.Common_WebUrl);

                            sb.Replace("{{action_url}}", callbackUrl);
                            UserManager.AddToRole(user.Id, AMRoles.Client);
                            await UserManager.SendEmailAsync(user.Id, "Confirm your account", sb.ToString());
                            return View("SuccessRegistration");
                        }
                        AddErrors(result);
                    }
                }
                catch (FormatException)
                {
                    ModelState.AddModelError("", "Invalid email address.");
                    return View(model);
                }


            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> ResendConfirmation(string email)
        {
            try
            {
                var user = UserManager.FindByEmail(email);
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\CreateAccount.html")));
                sb.Replace("{{name}}", user.FirstName);

                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);

                var u = new Uri(callbackUrl);
                string str = string.Format("{0}://{1}", u.Scheme, u.Host);
                callbackUrl = callbackUrl.Replace(str, Config.Common_WebUrl);

                sb.Replace("{{action_url}}", callbackUrl);
                await UserManager.SendEmailAsync(user.Id, "Confirm your account", sb.ToString());
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                var sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\ErrorInAM.html")));
                sb.Replace("{{action}}", "Account/ConfirmEmail");
                sb.Replace("{{error}}", string.Join(",", "UserId or Code not found."));
                await InternalEmail.SendMail(Config.Web_RegistrationDefaultEmails, null, Config.Web_RegistrationBCCEmails, "Error in AuthoMail.", sb.ToString());
                ViewBag.Success = false;
                ViewBag.Message = "User or Code invalid";
                return View();
            }
            var user = UserManager.FindById(userId);
            if (user == null)
            {
                ViewBag.Success = false;
                ViewBag.Message = "User not found.";
                return View();
            }
            if (user.EmailConfirmed)
            {
                ViewBag.Success = false;
                ViewBag.Message = "Your email address is already confirmed.";
                return View();
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);

            if (result.Succeeded)
            {
                bool isCoreLogicUser = CLAuthentication.HasCorelogicAccount(user.Email);
                if (!isCoreLogicUser)
                {
                    user.EmailConfirmed = false;
                    await UserManager.UpdateAsync(user);
                    return RedirectToAction("ClientSelection", new { userId });
                }
                ViewBag.Message = await ConfirmEmailAndSendMail(user);
                ViewBag.Success = true;
                return View();
            }
            else
            {
                ViewBag.Message = string.Join(",", result.Errors);
                ViewBag.confirmationlink = user.Email;
                var sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\ErrorInAM.html")));
                sb.Replace("{{user}}", user.Email);
                sb.Replace("{{action}}", "Account/ConfirmEmail");
                sb.Replace("{{error}}", string.Join(",", result.Errors));
                await InternalEmail.SendMail(Config.Web_RegistrationDefaultEmails, null, Config.Web_RegistrationBCCEmails, Config.Common_Environment + ": Error in authomail.", sb.ToString());
                ViewBag.Message = "Invalid token.".Equals(ViewBag.Message) ? "This confirmation link has expired. <span class='link resendemaillink' role='button' style='color: white;'><u>Click here</u></span> to receive new email confirmation link." : ViewBag.Message;
                ViewBag.Success = false;
                return View();
            }
        }
        private async Task<string> ConfirmEmailAndSendMail(ProfileUser user)
        {
            var sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\RegistrationInAuthoMail.html")));
            string emails;

            sb.Replace("{{url}}", Config.Common_WebUrl);
            emails = supportSvc.GetEmailsFromClientDomain(user.Email);

            sb.Replace("{{email}}", user.Email);

            //Send email to specified client admin for new registration request.
            await InternalEmail.SendMail(emails, null, Config.Web_RegistrationBCCEmails,
                 Config.Common_Environment == "QA" ? Config.Common_Environment + " : Registration in AuthoMail"
                                : "Registration in AuthoMail",
                sb.ToString());
            //Send email to client for waiting for approval of registration
            sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\WaitForApproval.html")));
            sb.Replace("{{name}}", user.FirstName);
            sb.Replace("{{email}}", user.Email);
            sb.Replace("{{defaultemails}}", string.Join(", ", emails.Split(',')));
            await InternalEmail.SendMail(user.Email, null, Config.Web_RegistrationBCCEmails,
                Config.Common_Environment == "QA" ? Config.Common_Environment + " : Welcome to AuthoMail"
                                : "Welcome to AuthoMail",
                sb.ToString());
            return "Thank you for confirming your email. Your registration request is now awaiting approval. Please note that you may login only after your account is approved.";
        }
        [AllowAnonymous]
        public async Task<ActionResult> ClientSelection(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.Success = false;
                ViewBag.Message = "User not found.";
                return View();
            }
            var isAlreadyRequested = await clientPermissionSvc.IsUserClientRequestPending(userId);
            if (isAlreadyRequested)
            {
                ViewBag.Success = false;
                ViewBag.Message = "User's request already recorded.";
                return View();
            }
            ViewBag.Success = true;
            string domainName = new MailAddress(user.Email).Host;
            ViewBag.clientDomains = await clientPermissionSvc.GetClientDomainByDomainName(domainName);
            ViewBag.userId = userId;
            return View();
        }
        [AllowAnonymous, HttpPost]
        public async Task<ActionResult> ClientSelection(string userId, List<int> clientIds)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var systemUser = await UserManager.FindByNameAsync("system@milestonetechnoconsulting.com");
            var clientUserMapping = new ClientUserMapping
            {
                UserId = userId,
                ModifiedById = systemUser.Id
            };
            for (int i = 0; i < clientIds.Count; i++)
            {
                clientUserMapping.ClientId = clientIds[i];
                clientUserMapping.Status = ClientUserMappingStatusTypes.Pending;
                await clientPermissionSvc.InsertClientIdAndUserId(clientUserMapping);
            }
            user.EmailConfirmed = true;
            await UserManager.UpdateAsync(user);
            var message = await ConfirmEmailAndSendMail(user);
            return Json(JsonResponseFactory.SuccessResponse(message), JsonRequestBehavior.AllowGet);
        }
        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                MailAddress mAdd = new MailAddress(model.Email);
                if (CLAuthentication.HasCorelogicAccount(model.Email))
                {
                    ModelState.AddModelError("", "AuthoMail uses LDAP authentication for CoreLogic users. In other words, approved CoreLogic users may use their windows username and password to log on. Since AuthoMail does not own your windows account, it doesn't provide a way to change password. You may use traditional ways to change your password such as 'Ctrl + Alt + Del' on your computer OR <a href='https://pwm.corelogic.com' target='_blank'>https://pwm.corelogic.com.</a>");
                    return View(model);
                }
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Couldn't find your AuthoMail Account.");
                    return View(model);
                }
                if (!(await UserManager.IsEmailConfirmedAsync(user.Id)) || user.Status.Equals(AMUserStatus.Open, StringComparison.CurrentCultureIgnoreCase) || user.Status.Equals(AMUserStatus.InActive, StringComparison.CurrentCultureIgnoreCase))
                {
                    ModelState.AddModelError("", "Email is not confirmed or account is not yet approved.");
                    return View(model);
                }

                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, protocol: Request.Url.Scheme);

                var u = new Uri(callbackUrl);
                string str = string.Format("{0}://{1}", u.Scheme, u.Host);
                callbackUrl = callbackUrl.Replace(str, Config.Common_WebUrl);

                StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\ForgotPassword.html")));
                sb.Replace("{{name}}", user.FirstName);
                sb.Replace("{{action_url}}", callbackUrl);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", sb.ToString());
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                //return RedirectToAction("ResetPasswordConfirmation", "Account");
                ModelState.AddModelError("", "Couldn't find your AuthoMail Account.");
                return View(model);
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                await accSvc.UpdateChangePasswordDateAsync(user.Id);
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult AMUsers()
        {
            return View();
        }
        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.BAOffShore)]
        public async Task<JsonResult> GetAllUsers()
        {
            var users = await accSvc.GetAllUsers();
            if (users == null || users.Count() == 0) return Json(JsonResponseFactory.ErrorResponse("Users not found."), JsonRequestBehavior.AllowGet);
            return Json(JsonResponseFactory.SuccessResponse(new { model = users.ToList() }), JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.BAOffShore)]
        public async Task<JsonResult> GetAllUserToDownExcel(int timezoneOffset)
        {
            var users = await accSvc.GetAllUsers();
            var model = new List<AMAllUsersReportVM>();
            foreach (var user in users)
            {
                model.Add(
                    new AMAllUsersReportVM
                    {
                        ClientName = user.Clients,
                        Email = user.Email,
                        EmailConfirmed = user.EmailConfirmed,
                        DisplayName = user.DisplayName,
                        Status = user.Status,
                        LastLoginTimeUTC = user.LastLoginTimeUTC.HasValue ? user.LastLoginTimeUTC.Value.AddMinutes(timezoneOffset) : user.LastLoginTimeUTC,
                        RequestedOn = user.RequestedOn.AddMinutes(timezoneOffset)
                    }
                    );
            }
            var dataTable = ExcelHelper.ToDataTable<AMAllUsersReportVM>(model);
            var memoryStream = ExcelHelper.ExportDataSet(dataTable);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return Json(JsonResponseFactory.SuccessResponse(Convert.ToBase64String(memoryStream.ToArray())), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize(Roles = AMRoles.Admin)]
        public JsonResult UnlockUser(string userId)
        {
            var user = UserManager.FindById(userId);
            if (user == null) return Json(JsonResponseFactory.ErrorResponse("User not found."), JsonRequestBehavior.AllowGet);
            var result = UserManager.SetLockoutEndDate(userId, DateTime.UtcNow);
            if (result.Succeeded)
            {
                return Json(JsonResponseFactory.SuccessResponse("User unlocked successfully."), JsonRequestBehavior.AllowGet);
            }
            return Json(JsonResponseFactory.ErrorResponse(null, null, result.Errors), JsonRequestBehavior.AllowGet);
        }
        #region Expiration Change Password
        public ActionResult ChangePassword(string unm)
        {
            return View(new ChangePasswordViewModel() { UserName = unm });
        }
        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var existingUser = await UserManager.FindByNameAsync(model.UserName);
            if (existingUser == null)
            {
                ModelState.AddModelError("", "User can't change password.");
                return View(model);
            }
            if (CLAuthentication.HasCorelogicAccount(existingUser.Email))
            {
                ModelState.AddModelError("", "CoreLogic users cannot change password here.");
                return View(model);
            }
            if (!existingUser.Status.Equals(AMUserStatus.Approved))
            {
                ModelState.AddModelError("", Config.Web_Msg_Inactive_Account);
                return View(model);
            }
            if (model.OldPassword.Equals(model.NewPassword))
            {
                ModelState.AddModelError("", "Your new password must be different then old password.");
                return View(model);
            }
            string userId = existingUser.Id;
            var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await accSvc.UpdateChangePasswordDateAsync(userId);

                StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\ChangePassword.html")));
                sb.Replace("{{name}}", existingUser.FirstName);
                sb.Replace("{{username}}", User.Identity.Name);

                await UserManager.SendEmailAsync(userId, "Your password changed", sb.ToString());
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index", "Account", new { Message = "Your password has been changed." });
            }
            AddErrors(result);
            return View(model);
        }
        #endregion Expiration Change Password

        [HttpPost]
        //Change user's status
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ChangeUserStatus(string userName, string status)
        {
            if (!(AMUserStatus.Approved == status || AMUserStatus.InActive == status || AMUserStatus.Open == status || AMUserStatus.Rejected == status))
                return Json(JsonResponseFactory.ErrorResponse("Invalid user status."));
            var user = UserManager.FindByName(userName);
            if (user == null)
                return Json(JsonResponseFactory.ErrorResponse("User not found. Please check user name."));
            if (user.Status.Equals(AMUserStatus.Open, StringComparison.CurrentCultureIgnoreCase))
                return Json(JsonResponseFactory.ErrorResponse("Currently user's request is pending. Please go to the Sign Up Request page"));
            user.Status = status;
            user.StatusModifiedOn = DateTime.UtcNow;
            user.StatusModifiedBy = UserManager.FindByName("system@milestonetechnoconsulting.com").Id;
            var result = UserManager.Update(user);
            if (result.Succeeded)
                return Json(JsonResponseFactory.SuccessResponse("User's status updated successfully."));
            return Json(JsonResponseFactory.ErrorResponse(string.Join(",", result.Errors)));
        }
        public ActionResult GetLastLoginDate(int timezoneoffset)
        {
            return Json(User.Identity.LastLoginTime().AddMinutes(-timezoneoffset).ToString("MM/dd/yy hh:mm:ss tt"), JsonRequestBehavior.AllowGet);
        }
        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ProfileUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }


        #region Helpers

        //Update Last login time
        private void UpdateLoginTime(string userId)
        {
            if (userId != null)
            {
                var user = UserManager.FindById(userId);
                if (user != null)
                {
                    user.LastLoginTimeUTC = DateTime.UtcNow;
                    _ = UserManager.Update(user);
                }
            }
        }
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}