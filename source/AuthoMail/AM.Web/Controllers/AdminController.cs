using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Text;
using System.Threading.Tasks;
using AM.Web.Models;
using System.Reflection;
using System.Data;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using System.Globalization;
using System.Net;
using System.Collections.Specialized;
using System.Net.Http;
using System.Configuration;
using System.Net.Http.Headers;
using AM.Services.Services;
using Newtonsoft.Json;
using System.Threading;
using DELFUS.TemplateImporter;

using AM.Lib;
using System.Net.Mail;
using AM.Services.Models;
using AutoMapper;
//using Microsoft.Ajax.Utilities;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager)]
    public class AdminController : BaseController
    {
        private AMUserManager _userManager;
        ISupportService supportSvc;
        IHomeService homeSvc;
        IConfigService confSvc;
        IInventoryService inventorySvc;
        IProjectRequestService projectRequestSvc;
        IDocTypeService docTypeSvc;

        private readonly IClientPermissionService clientPermissionSvc;
        public AdminController(AMUserManager userManager, ISupportService supportSvc, IHomeService homeSvc, IConfigService confSvc, IInventoryService inventorySvc, IProjectRequestService projectRequestSvc, IClientPermissionService clientPermissionSvc, IDocTypeService docTypeSvc)
        {
            UserManager = userManager;
            this.supportSvc = supportSvc;
            this.homeSvc = homeSvc;
            this.confSvc = confSvc;
            this.inventorySvc = inventorySvc;
            this.projectRequestSvc = projectRequestSvc;
            this.clientPermissionSvc = clientPermissionSvc;
            this.docTypeSvc = docTypeSvc;
        }
        // GET: Admin
        #region SignUp Request
        public ActionResult SignUpRequest()
        {

            return View();
        }
        public async Task<ActionResult> GetRoleAndAppCode(string userId)
        {
            if (userId == null)
            {
                return Json(JsonResponseFactory.ErrorResponse("User not found"), JsonRequestBehavior.AllowGet);
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(JsonResponseFactory.ErrorResponse("User not found"), JsonRequestBehavior.AllowGet);
            }
            bool IsCL = CLAuthentication.HasCorelogicAccount(user.Email);
            var roles = new List<string> { AMRoles.Client };
            if (IsCL)
            {
                roles.Add(AMRoles.BA);
                roles.Add(AMRoles.BAOffShore);
            }

            var domain = user.Email.Split('@')[1];

            var appCodes = await clientPermissionSvc.GetAppCodeByClientDomain(IsCL ? null : domain);

            var data = new
            {
                roles = roles,
                appCodes = appCodes,
                isCL = IsCL
            };
            return Json(JsonResponseFactory.SuccessResponse(data), JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> EditGetRoleAndAppCode(string userId)
        {
            if (userId == null)
            {
                return Json(JsonResponseFactory.ErrorResponse("User not found"), JsonRequestBehavior.AllowGet);
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(JsonResponseFactory.ErrorResponse("User not found"), JsonRequestBehavior.AllowGet);
            }
            bool IsCL = CLAuthentication.HasCorelogicAccount(user.Email);
            var roles = new List<string> { AMRoles.Client };
            if (IsCL)
            {
                roles.Add(AMRoles.BA);
                roles.Add(AMRoles.BAOffShore);
            }

            //var appCodes = await clientPermissionSvc.GetAppCodeByUserIdORall(IsCL ? null : userId);
            var domainAppCode = IsCL ? await clientPermissionSvc.GetClientDomainForCL() : await clientPermissionSvc.GetClientDomainByDomainName(new MailAddress(user.Email).Host);
            var userRole = await UserManager.GetRolesAsync(user.Id);
            var approveAppCode = await clientPermissionSvc.GetApproveAppCodesByUserId(user.Id);
            var data = new
            {
                roles = roles,
                isCL = IsCL,
                domainAppCode = domainAppCode,
                userRole = userRole.Where(u => u == AMRoles.BA || u == AMRoles.Client || u == AMRoles.BAOffShore).FirstOrDefault(),
                approveAppCode = approveAppCode
            };
            return Json(JsonResponseFactory.SuccessResponse(data), JsonRequestBehavior.AllowGet);
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
        public ActionResult GetSingUpRequestedList()
        {
            string userId = User.IsInRole(AMRoles.Admin) || User.IsInRole(AMRoles.CoreLogicUserManager) ? null : User.Identity.GetUserId();
            var model = supportSvc.GetUsersRequestSignUp(userId);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateUsersRequestSignUp(string userId, string status, List<int> clientIds, string role)
        {
            try
            {
                if (status.IsNullOrWhiteSpace())
                {
                    return Json(JsonResponseFactory.ErrorResponse("Status can't be empty."), JsonRequestBehavior.AllowGet);
                }
                if (role.IsNullOrWhiteSpace() && status.Equals(AMUserStatus.Approved, StringComparison.CurrentCultureIgnoreCase))
                {
                    return Json(JsonResponseFactory.ErrorResponse("Please select valid Role."), JsonRequestBehavior.AllowGet);
                }

                var user = await UserManager.FindByIdAsync(userId);
                var model = new KeyValuePair<bool, string>();
                string partiallyApproveRequest = null;
                if (!role.IsNullOrWhiteSpace() && !(role.Equals(AMRoles.BA, StringComparison.CurrentCultureIgnoreCase) || role.Equals(AMRoles.Client, StringComparison.CurrentCultureIgnoreCase) || (role.Equals(AMRoles.BAOffShore, StringComparison.CurrentCultureIgnoreCase))))
                {
                    return Json(JsonResponseFactory.ErrorResponse("Please select valid Role."), JsonRequestBehavior.AllowGet);
                }
                if (role == AMRoles.Client && status.Equals(AMUserStatus.Approved, StringComparison.CurrentCultureIgnoreCase))
                {

                    if (clientIds == null || clientIds.Count == 0)
                    {
                        return Json(JsonResponseFactory.ErrorResponse("Please select AppCode(s)."), JsonRequestBehavior.AllowGet);
                    }
                    //CL user Remove BA role and add Client role
                    if (CLAuthentication.HasCorelogicAccount(user.Email))
                    {
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.BA);
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.BAOffShore);
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.Client);
                        await UserManager.AddToRoleAsync(userId, AMRoles.Client);
                    }
                    var output = await clientPermissionSvc.ApproveOrDeclinedClientsForUser(userId, clientIds, User.Identity.GetUserId(), CLAuthentication.HasCorelogicAccount(user.Email));

                    if (output.Any(o => o.Status == ClientUserMappingStatusTypes.Declined))
                    {
                        foreach (var item in output)
                        {
                            partiallyApproveRequest = $@"{partiallyApproveRequest} <tr>
                            <th style='border: 1px solid black; padding: 5px 10px;'>{item.ClientName}</th>
                                <td style='border: 1px solid black; padding: 5px 10px;'>{item.Status}</td>
                            </tr>";
                        }
                        model = new KeyValuePair<bool, string>(true, "Sign up request approved successfully for partial AppCode(s).");
                    }
                    else
                    {
                        model = new KeyValuePair<bool, string>(true, "Sign up request approved successfully for all AppCode(s).");
                    }
                }
                else if (role == AMRoles.BA && status.Equals(AMUserStatus.Approved, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (status.Equals(AMUserStatus.Approved, StringComparison.CurrentCultureIgnoreCase) && CLAuthentication.HasCorelogicAccount(user.Email))
                    {
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.BA);
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.BAOffShore);
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.Client);
                        await UserManager.AddToRoleAsync(userId, AMRoles.BA);
                    }
                    model = supportSvc.UpdateUsersRequestSignUp(userId, status, User.Identity.GetUserId());
                }
                else
                {
                    if (status.Equals(AMUserStatus.Approved, StringComparison.CurrentCultureIgnoreCase) && CLAuthentication.HasCorelogicAccount(user.Email))
                    {
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.BA);
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.BAOffShore);
                        await UserManager.RemoveFromRoleAsync(userId, AMRoles.Client);
                        await UserManager.AddToRoleAsync(userId, AMRoles.BAOffShore);

                    }
                    model = supportSvc.UpdateUsersRequestSignUp(userId, status, User.Identity.GetUserId());
                }


                string template = status.Equals(AMUserStatus.Approved) ? "ApproveRegistration.html" : "RejectRegistration.html";
                if (!(partiallyApproveRequest.IsNullOrWhiteSpace()))
                {
                    template = "PartiallyApproveRegistration.html";
                }
                StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\" + template)));
                sb.Replace("{{name}}", user.FirstName);
                sb.Replace("{{email}}", user.Email);
                if (!partiallyApproveRequest.IsNullOrWhiteSpace())
                {
                    sb.Replace("{{PartiallyApproveRequestTableData}}", partiallyApproveRequest);
                }

                sb.Replace("{{AuthoMailURL}}", Config.Common_WebUrl);
                string emails = supportSvc.GetEmailsFromClientDomain(user.Email);
                string sub = status.Equals(AMUserStatus.Approved) ? "AuthoMail registration request approved"
                    : "AuthoMail registration request rejected";
                await InternalEmail.SendMail(user.Email, null, emails
                    , Config.Common_Environment == "QA" ? Config.Common_Environment + " : " + sub : sub
                    , sb.ToString());
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveEditRoleClient(string userId, List<int> clientIds, string role)
        {
            if (userId.IsNullOrWhiteSpace())
            {
                return Json(JsonResponseFactory.ErrorResponse("User not found."), JsonRequestBehavior.AllowGet);
            }
            if (role.IsNullOrWhiteSpace())
            {
                return Json(JsonResponseFactory.ErrorResponse("Please select valid Role."), JsonRequestBehavior.AllowGet);
            }
            if (clientIds != null)
                clientIds.RemoveAll(c => c == 0);
            if (role == AMRoles.Client && (clientIds == null || clientIds.Count == 0))
            {
                return Json(JsonResponseFactory.ErrorResponse("Please select AppCode(s)."), JsonRequestBehavior.AllowGet);
            }
            var user = await UserManager.FindByIdAsync(userId);
            var output = await clientPermissionSvc.EditSaveRoleClient(user, clientIds, role, User.Identity.GetUserId(), CLAuthentication.HasCorelogicAccount(user.Email));
            if (output.Key)
                return Json(JsonResponseFactory.SuccessResponse(output.Value), JsonRequestBehavior.AllowGet);

            return Json(JsonResponseFactory.ErrorResponse(output.Value), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Manage Domain
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetDomains()
        {
            try
            {
                var model = supportSvc.GetClientDomains();

                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetDomainByNameAndclientId(string domainName, int clientId)
        {

            if (!string.IsNullOrWhiteSpace(domainName))
            {
                ClientDomains domain = supportSvc.FindByDomainNameAndClientId(domainName, clientId);
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name", domain.ClientId);
                return PartialView("_AddOrEditDomain", domain);
            }
            else
            {
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
                return PartialView("_AddOrEditDomain", new ClientDomains());
            }
        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageDomain()
        {
            var model = homeSvc.GetAllActiveClient();

            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditDomain(int clientId, string domainName, string oldDomain)
        {

            try
            {
                int model = 0;
                if (string.IsNullOrWhiteSpace(oldDomain))
                {
                    model = supportSvc.CreateClientDomain(clientId, domainName);
                    return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model = supportSvc.UpdateClientDomain(domainName, clientId, oldDomain);
                    return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region System Settings
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult SystemSettings()
        {
            return View();
        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult GetAllSystemSettings()
        {
            var model = confSvc.GetSystemSettings();
            return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ResetSetting(string settingId)
        {
            try
            {
                var model = confSvc.ResetValueSystemSetting(User.Identity.GetUserId(), settingId);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult UpdateValueOrDescription(string settingId, string value, string description)
        {
            try
            {
                var model = confSvc.UpdateValueOrDescription(User.Identity.GetUserId(), settingId, value, description);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Manage Business Unit
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageBusinessUnits()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetBusinessUnits()
        {
            try
            {
                var model = supportSvc.GetBusinessUnit().OrderByDescending(s => s.IsActive).ToList();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditBusinessUnit(BusinessUnitModel businessUnit)
        {
            try
            {
                var model = supportSvc.CreateOrEditBusinessUnit(businessUnit);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetBusinessUnitById(int id)
        {
            return id == 0 ? PartialView("_AddOrEditBusinessUnit", new BusinessUnitModel()) : PartialView("_AddOrEditBusinessUnit", supportSvc.FindByIdBusinessUnit(id));
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult MakeActiveOrInActiveBusinessUnit(bool isActive, int Id)
        {
            try
            {
                var model = supportSvc.MakeActiveOrInActiveBusinessUnit(isActive, Id);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Manage Client

        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult _AddCoverLetter(int id)
        {
            var model = supportSvc.GetDocTypesByClientId(id);
            ViewBag.IsModelEmpty = true;
            if (model != null)
            {
                ViewBag.IsModelEmpty = false;
                ViewBag.ClientId = id;
                ViewBag.ClientName = model.FirstOrDefault().ClientName;
                ViewBag.AppCode = model.FirstOrDefault().AppCode;
                ViewBag.DocType = new SelectList(model, "Id", "DocTypeCode");
            }
            return PartialView();
        }


        [Authorize(Roles = AMRoles.Admin)]
        [HttpPost]
        public ActionResult _AddCoverLetter(int docTypeId, int seqId, string fileName, string appCode, HttpPostedFileBase file)
        {
            try
            {
                PnSConfigDetailCoverLetter coverLetter = new PnSConfigDetailCoverLetter();
                coverLetter.PnSConfigDetailId = docTypeId;
                coverLetter.SeqId = seqId;
                coverLetter.CoverLetterTemplatePath = fileName;

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("AppCode", appCode);
                client.DefaultRequestHeaders.Add("Id", "c711c4aa-8559-44fa-9edc-88600f38b3c9");
                client.DefaultRequestHeaders.Add("FileName", fileName);
                MultipartFormDataContent form = new MultipartFormDataContent();
                HttpContent content = new StringContent("file");
                content = new StreamContent(file.InputStream);
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = file.FileName
                };
                form.Add(content, "file", fileName);
                var validateCoverLetter = supportSvc.ValidateDocTypeCoverLetter(coverLetter);
                if (validateCoverLetter.Key)
                {
                    HttpResponseMessage response = client.PostAsync(ConfigurationManager.AppSettings["CoverLetterUploadUrl"].ToString(), form).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var model = supportSvc.SaveDocTypeCoverLetter(coverLetter);
                        return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var errorModel = new KeyValuePair<bool, string>(false, "File not uploaded, Please try again.");
                        return Json(JsonResponseFactory.SuccessResponse(errorModel), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(JsonResponseFactory.SuccessResponse(validateCoverLetter), JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult DownloadCoverLetter(string fileName, string appCode)
        {

            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = new HttpResponseMessage();
                string url = string.Format("{0}?fileName={1}&appCode={2}", ConfigurationManager.AppSettings["CoverLetterDownloadUrl"].ToString(), fileName, appCode);
                response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    string fileGuid = Guid.NewGuid().ToString();
                    TempData[fileGuid] = response.Content.ReadAsByteArrayAsync().Result;

                    return Json(JsonResponseFactory.SuccessResponse(new { model = new { FileGuid = fileGuid, FileName = fileName } }), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse(response.ReasonPhrase), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {

                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult _DownloadCoverLetter(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                return new EmptyResult();
            }

        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult GetCoverLetterByDocTypeId(int id)
        {
            var model = supportSvc.GetDocTypeCoverLetterByDocTypeId(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageClients()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetClients()
        {
            try
            {
                var model = supportSvc.GetClients().OrderByDescending(s => s.IsAMActive).ThenByDescending(s => s.IsPnSActive).ToList();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult MakeActiveOrDeactiveClient(bool isActive, int Id, bool isAM)
        {
            try
            {
                if (isAM)
                {
                    var model = supportSvc.MakeClientAMActiveOrInActive(Id, isActive, User.Identity.GetUserId());
                    return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var model = supportSvc.MakeClientPnSActiveOrInActive(Id, isActive, User.Identity.GetUserId());
                    return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditClient(Clients client)
        {
            try
            {
                var model = supportSvc.CreateEditClient(client, User.Identity.GetUserId());
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetClientById(int id)
        {

            if (id != 0)
            {
                Clients client = supportSvc.FindByClientId(id);
                ViewBag.BusinessUnitList = new SelectList(supportSvc.GetBusinessUnitActive(), "Id", "BusinessUnit", client.BusinessUnit);
                ViewBag.TimezoneList = new SelectList(TimeZoneInfo.GetSystemTimeZones().OrderBy(s => s.StandardName).ToList(), "StandardName", "StandardName", client.TimeZone);
                return PartialView("_AddOrEditClient", client);
            }
            else
            {
                ViewBag.BusinessUnitList = new SelectList(supportSvc.GetBusinessUnitActive(), "Id", "BusinessUnit");
                ViewBag.TimezoneList = new SelectList(TimeZoneInfo.GetSystemTimeZones().OrderBy(s => s.StandardName).ToList(), "StandardName", "StandardName");
                return PartialView("_AddOrEditClient", new Clients());
            }
        }

        #endregion

        #region Manage SFTChannel
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageSFTChannels()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetSFTChannels()
        {
            try
            {
                var model = supportSvc.GetSFTChannels();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult MakeActiveOrDeactiveSFTChannel(bool isActive, int Id)
        {
            try
            {
                var model = supportSvc.MakeActivateOrDeactivateSFTChannel(Id, isActive);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetSFTChannelById(int id)
        {
            var ProtocolList = new string[] { "ftp", "sftp", "unc" };
            var PostDownloadActionList = new string[] { "Archive", "Delete" };

            if (id != 0)
            {
                SFTChannels model = supportSvc.FindByIdSFTChannels(id);
                ViewBag.ProtocoldropDown = ProtocolList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.Protocol == s }).ToList();
                ViewBag.PostDownloadActiondropDown = PostDownloadActionList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.PostDownloadAction == s }).ToList();
                return PartialView("_AddOrEditSFTChannel", model);
            }
            else
            {
                ViewBag.ProtocoldropDown = ProtocolList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.PostDownloadActiondropDown = PostDownloadActionList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                return PartialView("_AddOrEditSFTChannel", new SFTChannels());
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditSFTChannel(SFTChannels sftChannel)
        {
            try
            {
                var model = supportSvc.CreateOrEditSFTChannel(sftChannel);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Manage DocType

        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult _GetCoverLetterFiles(int id)
        {
            var model = supportSvc.GetDocTypeCoverLetterByDocTypeId(id);
            return PartialView(model);
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult RemoveCoverLetter(int docTypeId, int seqId, string fileName)
        {
            try
            {
                var model = supportSvc.RemoveCoverletter(docTypeId, seqId, fileName);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageDocumentTypes()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetDocTypes()
        {
            try
            {
                var model = supportSvc.GetDocTypes();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetDocTypeById(int id)
        {
            var PrintSizeList = Config.Web_DD_PrintSizeList.Split(',').ToArray();
            var PrintFormatList = Config.Web_DD_PrintFormatList.Split(',').ToArray();
            var ShippingCarrierList = Config.Web_DD_ShippingCarrierList.Split(',').ToArray();
            var ShippingServiceTypeList = Config.Web_DD_ShippingServiceTypeList.Split(',').ToArray();
            var ShippingSpecialServiceList = Config.Web_DD_ShippingSpecialServiceList.Split(',').ToArray();
            ViewBag.ShippingSpecialService = ShippingSpecialServiceList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
            if (id != 0)
            {
                DocTypes model = supportSvc.FindByIdDocType(id);
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name", model.ClientId);
                ViewBag.PrintSizedropDown = PrintSizeList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.PrintSize == s }).ToList();

                ViewBag.PrintFormatdropDown = PrintFormatList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.PrintFormat == s }).ToList();

                ViewBag.ShippingCarrierdropDown = ShippingCarrierList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.ShippingCarrier == s }).ToList();
                ViewBag.ShippingServiceTypedropDown = ShippingServiceTypeList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.ShippingServiceType == s }).ToList();
                ViewBag.BREEnvelopedropDown = inventorySvc.GetEnvelopes().Where(s => s.IsBREEnvelope).Select(s => new SelectListItem() { Text = s.EnvelopeName, Value = s.Id.ToString(), Selected = model.BREEnvelopeId == s.Id }).ToList();
                ViewBag.EnvelopedropDown = inventorySvc.GetEnvelopes().Where(s => !s.IsBREEnvelope).Select(s => new SelectListItem() { Text = s.EnvelopeName, Value = s.Id.ToString(), Selected = model.BREEnvelopeId == s.Id }).ToList();
                return PartialView("_AddOrEditDocType", model);
            }
            else
            {
                ViewBag.PrintFormatdropDown = PrintFormatList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.PrintSizedropDown = PrintSizeList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.ShippingCarrierdropDown = ShippingCarrierList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.ShippingServiceTypedropDown = ShippingServiceTypeList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
                ViewBag.BREEnvelopedropDown = inventorySvc.GetEnvelopes().Where(s => s.IsBREEnvelope).Select(s => new SelectListItem() { Text = s.EnvelopeName, Value = s.Id.ToString() }).ToList();
                ViewBag.EnvelopedropDown = inventorySvc.GetEnvelopes().Where(s => !s.IsBREEnvelope).Select(s => new SelectListItem() { Text = s.EnvelopeName, Value = s.Id.ToString() }).ToList();
                return PartialView("_AddOrEditDocType", new DocTypes());
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult _DocTypePageRangeAjax(int index)
        {
            var model = PageRangeMethods(new DocTypePageRange(), index, false, "");
            return PartialView("_DocTypePageRange", model);
        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult _DocTypePageRange(DocTypePageRange docTypePageRange, int index, bool isFallBack, string shippingServiceType)
        {
            var model = PageRangeMethods(docTypePageRange, index, isFallBack, shippingServiceType);
            return PartialView("_DocTypePageRange", model);
        }
        public DocTypePageRange PageRangeMethods(DocTypePageRange docTypePageRange, int index, bool isFallBack, string shippingServiceType)
        {
            var ShippingCarrierList = Config.Web_DD_ShippingCarrierList.Split(',').ToArray();
            var ShippingServiceTypeList = Config.Web_DD_ShippingServiceTypeList.Split(',').ToArray();
            var ShippingSpecialServiceList = Config.Web_DD_ShippingSpecialServiceList.Split(',').ToArray();
            ViewBag.ShippingSpecialService = ShippingSpecialServiceList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = s == docTypePageRange.ShippingSpecialService }).ToList();
            ViewBag.ShippingCarrierdropDown = ShippingCarrierList.Select(s => new SelectListItem() { Text = s.Trim(), Value = s.Trim(), Selected = docTypePageRange.ShippingCarrier == s.Trim() }).ToList();
            ViewBag.ShippingServiceTypedropDown = ShippingServiceTypeList.Select(s => new SelectListItem() { Text = s.Trim(), Value = s.Trim(), Selected = docTypePageRange.ShippingServiceType == s.Trim() }).ToList();
            var breEnvelopes = new List<SelectListItem>() { new SelectListItem { Text = "NA", Value = "0", Selected = docTypePageRange.BREEnvelopeId == 0 } };
            breEnvelopes.AddRange(inventorySvc.GetEnvelopes().Where(s => s.IsBREEnvelope).Select(s => new SelectListItem() { Text = s.EnvelopeName, Value = s.Id.ToString(), Selected = docTypePageRange.BREEnvelopeId == s.Id }).ToList());
            ViewBag.BREEnvelopedropDown = breEnvelopes;
            ViewBag.EnvelopedropDown = inventorySvc.GetEnvelopes().Where(s => !s.IsBREEnvelope).Select(s => new SelectListItem() { Text = s.EnvelopeName, Value = s.Id.ToString(), Selected = docTypePageRange.EnvelopeId == s.Id }).ToList();
            ViewBag.index = index;
            ViewBag.isFallBack = isFallBack;
            ViewBag.shippingServiceType = shippingServiceType;
            return docTypePageRange;
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult MakeActiveOrDeactiveDocType(bool isActive, int Id)
        {
            try
            {
                var model = supportSvc.MakeActivateOrDeactivateDocType(Id, isActive, User.Identity.GetUserId());
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult CreateOrEditDocType(int id = 0, string Success = null)
        {
            var clientDocTypes = new ClientDocTypes();
            if (id != 0)
                clientDocTypes = supportSvc.GetClientDocTypesById(id);

            CreateOrEditDocTypeSettings(clientDocTypes);
            ViewBag.Success = Success;
            return View(clientDocTypes);
        }
        //[Authorize(Roles = AMRoles.Admin), HttpPost]
        //public ActionResult CreateOrEditDocType(DocTypes docTypes)
        //{
        //    try
        //    {
        //        var model = supportSvc.CreateOrEditDocType(docTypes, User.Identity.GetUserId());
        //        return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
        //    }
        //}
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditDocType(ClientDocTypes clientDocTypes)
        {
            CreateOrEditDocTypeSettings(clientDocTypes);
            try
            {
                if (ModelState.IsValid)
                {
                    var fallBackShippingServiceType = new List<string>() { "PRIORITY_EXPRESS", "PRIORITY" };
                    clientDocTypes.DocTypes.DocTypePageRangeList.RemoveAll(d => d.DocTypesId < 0 || (!d.IsFallBack && fallBackShippingServiceType.Any(f => f == d.ShippingServiceType)));
                    if (clientDocTypes.DocTypes.DocTypePageRangeList.Count() == 0)
                    {
                        ModelState.AddModelError("", "Please specify DocType page range configuration.");
                        return View(clientDocTypes);
                    }
                    var model = supportSvc.CreateOrEditDocType(clientDocTypes, User.Identity.GetUserId());
                    if (model.Key)
                    {
                        return RedirectToAction("CreateOrEditDocType", new { id = Convert.ToInt32(model.Value) });
                    }
                    else
                    {
                        ModelState.AddModelError("", model.Value);
                        return View(clientDocTypes);
                    }
                }
                else
                {
                    return View(clientDocTypes);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(clientDocTypes);
            }
        }

        private void CreateOrEditDocTypeSettings(ClientDocTypes clientDocTypes)
        {
            var PrintSizeList = Config.Web_DD_PrintSizeList.Split(',').ToArray();
            var PrintFormatList = Config.Web_DD_PrintFormatList.Split(',').ToArray();
            //var ShippingCarrierList = Config.Web_DD_ShippingCarrierList.Split(',').ToArray();
            //var ShippingServiceTypeList = Config.Web_DD_ShippingServiceTypeList.Split(',').ToArray();
            //var ShippingSpecialServiceList = Config.Web_DD_ShippingSpecialServiceList.Split(',').ToArray();
            //ViewBag.ShippingSpecialService = ShippingSpecialServiceList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
            ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name", clientDocTypes.ClientId);
            ViewBag.PrintSizedropDown = PrintSizeList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = clientDocTypes.DocTypes.PrintSize == s }).ToList();

            ViewBag.PrintFormatdropDown = PrintFormatList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = clientDocTypes.DocTypes.PrintFormat == s }).ToList();
            //ViewBag.ShippingCarrierdropDown = ShippingCarrierList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = clientDocTypes.DocTypes.ShippingCarrier == s }).ToList();
            //ViewBag.ShippingServiceTypedropDown = ShippingServiceTypeList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = clientDocTypes.DocTypes.ShippingServiceType == s }).ToList();
            //ViewBag.BREEnvelopedropDown = inventorySvc.GetEnvelopes().Where(s => s.IsBREEnvelope).Select(s => new SelectListItem() { Text = s.EnvelopeName, Value = s.Id.ToString(), Selected = clientDocTypes.DocTypes.BREEnvelopeId == s.Id }).ToList();
            //ViewBag.EnvelopedropDown = inventorySvc.GetEnvelopes().Where(s => !s.IsBREEnvelope).Select(s => new SelectListItem() { Text = s.EnvelopeName, Value = s.Id.ToString(), Selected = clientDocTypes.DocTypes.BREEnvelopeId == s.Id }).ToList();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult _DownloadDocTypeLists()
        {
            try
            {
                var result = docTypeSvc.GetDocTypeList();

                var dataTable = ExcelHelper.ToDataTable<DocTypeReport>(result);
                var memoryStream = ExcelHelper.ExportDataSet(dataTable);
                memoryStream.Seek(0, SeekOrigin.Begin);

                string fileGuid = Guid.NewGuid().ToString();

                string fileName = $"DocTypeList_{DateTime.UtcNow.ToString("MMddyyyy")}.xlsx";

                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult _PrintConfigurationByPnSConfigId(int pnsConfigId, string documentType)
        {
            ViewBag.PnSConfigId = pnsConfigId;
            ViewBag.DocumentType = documentType;
            return PartialView(supportSvc.GetPnSConfigDetailsByPnSConfigId(pnsConfigId));
        }
        #region PrintTrailMaps
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult PrintTrailMapByDocId(int id)
        {
            ViewBag.PrintTrailId = supportSvc.GetPrintTrails();
            return View(supportSvc.GetDocTypePrintTrailByDocTypeId(id));
        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult _AddNewRowPrintTrail()
        {
            ViewBag.PrintTrailId = supportSvc.GetPrintTrails();
            return PartialView();
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult SaveDocTypePrintTrail(List<PnSConfigDetailPrintTrailMap> docTypePrintTrailMapList)
        {

            try
            {
                var model = supportSvc.SaveDocTypePrintTrail(docTypePrintTrailMapList);
                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Assige Role
        [HttpGet]
        public ActionResult AssignUserManagerDetails()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetUserManagerList()
        {
            try
            {
                var users = supportSvc.GetUserManagerList();
                return Json(JsonResponseFactory.SuccessResponse(users), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult AssignUserManagerRole(string email)
        {
            try
            {
                var result = supportSvc.AssignUserManagerRole(email);
                if (result.Key)
                {
                    return Json(JsonResponseFactory.SuccessResponse(result.Value), JsonRequestBehavior.AllowGet);
                }
                return Json(JsonResponseFactory.ErrorResponse(result.Value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult RemoveUserManagerRole(string userId, string roleId)
        {
            try
            {
                var result = supportSvc.RemoveUserManagerRole(userId, roleId);
                if (result.Key)
                {
                    return Json(JsonResponseFactory.SuccessResponse(result.Value), JsonRequestBehavior.AllowGet);
                }
                return Json(JsonResponseFactory.ErrorResponse(result.Value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Project Setup

        public ActionResult AllProjectSetupDetails()
        {
            return View(projectRequestSvc.GetAllProjectRequest());
        }
        public ActionResult ProjectSetup(int id)
        {
            if (id == 0)
            {
                return View(new ProjectRequest());
            }
            else
            {
                var model = projectRequestSvc.ProjectRequestFindById(id);
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult ProjectSetup(ProjectRequest projectRequest, string SubmittedOn, string ProdStartOn, List<HttpPostedFileBase> files, List<string> fileNames)
        {

            try
            {
                projectRequest.SubmittedOn = DateTime.ParseExact(SubmittedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                projectRequest.ProdStartOn = DateTime.ParseExact(ProdStartOn, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                projectRequest.FileTransmittedVia = string.Join(",", projectRequest.FileTransmittedViaList);
                projectRequest.TypeOfFiles = string.Join(",", projectRequest.TypeOfFilesList);
                projectRequest.ServiceLevel = string.Join(",", projectRequest.ServiceLevelList);
                projectRequest.PackagesSentToCustomer = string.Join(",", projectRequest.PackagesSentToCustomerList);
                projectRequest.Envelopes = string.Join(",", projectRequest.EnvelopesList);
                projectRequest.BusinessReplyMailEnvelope = string.Join(",", projectRequest.BusinessReplyMailEnvelopeList);
                projectRequest.TransmittedTime = string.Join(",", projectRequest.TransmittedTimeList);

                projectRequest.DailyPrintSchedule = string.Join(",", projectRequest.DailyPrintScheduleList);
                projectRequest.PaperPrintRequirements = string.Join(",", projectRequest.PaperPrintRequirementsList);
                var model = projectRequestSvc.SaveProjectRequest(projectRequest, User.Identity.GetUserId());
                if (model.Key)
                {
                    int id = Convert.ToInt32(model.Value);
                    string subPath = string.Format("/SamplePdfs/{0}/", id.ToString());

                    bool exists = Directory.Exists(Server.MapPath(subPath));

                    if (!exists)
                        Directory.CreateDirectory(Server.MapPath(subPath));
                    List<string> filesName = new List<string>();
                    if (files != null && files[0] != null)
                    {
                        foreach (var file in files)
                        {
                            if (Path.GetExtension(file.FileName) == ".pdf")
                            {
                                string filePath = string.Format("{0}/{1}", subPath, file.FileName);
                                file.SaveAs(Server.MapPath(filePath));
                                filesName.Add(file.FileName);
                            }
                        }
                        if (filesName.Count > 0)
                        {
                            string names = string.Join(",", filesName).ToString();
                            projectRequestSvc.SaveProjectRequestFiles(id, names, User.Identity.GetUserId());
                        }
                    }

                    return RedirectToAction("AllProjectSetupDetails");
                }
                else
                {
                    ModelState.AddModelError("", model.Value);
                    return View(projectRequest);
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View(projectRequest);
            }
        }
        [HttpPost]
        public ActionResult RemoveFile(int id, string fileName)
        {
            try
            {
                var model = projectRequestSvc.RemoveFileName(id, fileName, User.Identity.GetUserId());
                if (model.Key)
                {
                    string file = string.Format("/SamplePdfs/{0}/{1}", id.ToString(), fileName);
                    if (System.IO.File.Exists(Server.MapPath(file)))
                    {
                        System.IO.File.Delete(Server.MapPath(file));
                    }
                }
                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadProjectSetupPdfFile(int id)
        {
            var model = projectRequestSvc.ProjectRequestFindById(id);
            var memoryStream = new MemoryStream(PDFTemplate.CreatePdf(model));
            var fileNameArry = new string[] { model.ClientName, DateTime.Now.ToString("MMddyyymmss") };
            string fileName = string.Empty;
            fileName = string.Join("_", fileNameArry.Where(s => s != "").ToArray());
            // fileName = string.Format("{0}.pdf", fileName);
            fileName = string.Format("{0}.pdf", fileName);
            //memoryStream.Seek(0, SeekOrigin.Begin);
            //return File(memoryStream, "application/pdf", fileName);
            return File(memoryStream, "application/pdf", fileName);
        }

        public ActionResult DownloadProjectSetupExcelFile(int id)
        {
            try
            {

                XLS exportFile = new XLS(Server.MapPath("~/Template/Excel/ProjectSetupTemplate.xlsx"));
                DataTable exportdt = new DataTable();

                var model = projectRequestSvc.ProjectRequestFindById(id);

                exportdt.Columns.Add("Key");
                exportdt.Columns.Add("Value");
                Type myType = model.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
                foreach (PropertyInfo prop in props)
                {
                    object propValue = prop.GetValue(model, null);
                    if (prop.PropertyType == typeof(DateTime?))
                    {
                        if (propValue != null)
                        {
                            var dt = (DateTime)propValue;
                            exportdt.Rows.Add(new Object[] { prop.Name, dt.ToString("dd/MM/yyyy") });
                        }
                        else
                        {
                            exportdt.Rows.Add(new Object[] { prop.Name, propValue });
                        }

                    }
                    else
                    {
                        exportdt.Rows.Add(new Object[] { prop.Name, propValue });
                    }

                }

                exportFile.LoadData("Sheet1", exportdt, false, true, (j) =>
                {

                    //j.SetSheetHidden(j.GetSheetIndex("Sheet2"), SheetState.Hidden);
                    j.GetSheet("Sheet2").ProtectSheet("Password");//password 
                    j.SetSheetHidden(j.GetSheetIndex("Sheet1"), SheetState.VeryHidden);
                    XSSFFormulaEvaluator.EvaluateAllFormulaCells(j);
                });
                var memoryStream = new MemoryStream();
                //Workbook workbook = new Workbook();
                //workbook.LoadFromFile(filename);
                //workbook.SaveToFile("result.pdf", FileFormat.PDF);  
                exportFile.SaveAs(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                string fileName = string.Empty;
                var fileNameArry = new string[] { model.ClientName, DateTime.Now.ToString("MMddyyymmss") };
                fileName = string.Join("_", fileNameArry.Where(s => s != "").ToArray());
                // fileName = string.Format("{0}.pdf", fileName);
                fileName = string.Format("{0}.xlsx", fileName);
                //return File(memoryStream, "application/pdf", fileName);
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        #endregion

        #region Manage Envelope
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetEnvelopes()
        {
            try
            {
                var model = Mapper.Map<List<EnvelopesListVM>>(inventorySvc.GetEnvelopes().OrderBy(e => e.Order));
                var envelopeInvStockCalc = inventorySvc.GetEnvelopeInventoryStockCalculation();
                model.ForEach(s =>
                {
                    s.AlertLevel = envelopeInvStockCalc.Where(e => e.EnvelopeId == s.Id).Max(e => e.AlertLevel);
                });
                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetEnvelopeById(int id)
        {

            if (id != 0)
            {
                Envelopes envelop = inventorySvc.FindEnvelopeById(id);
                return PartialView("_AddOrEditEnvelope", envelop);
            }
            else
            {
                return PartialView("_AddOrEditEnvelope", new Envelopes());
            }
        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageEnvelopes()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult CreateOrEditEnvelope(int? id)
        {
            var shippers = inventorySvc.ShipperSelectList();
            ViewBag.Shippers = new SelectList(shippers, "Id", "Name");
            if (id.HasValue)
            {
                return View(inventorySvc.GetEnvelopeWithShippers(id.Value));
            }

            return View(new EnvelopesVM());
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult ShipperWiseStock(int ShipperId, string ShipperName)
        {
            return PartialView("_ShipperWiseStockDetails", new EnvelopeStockVM { ShipperName = ShipperName, ShipperId = ShipperId });
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditEnvelope(EnvelopesVM envelopes)
        {
            try
            {
                return Json(JsonResponseFactory.SuccessResponse(inventorySvc.SaveEnvelopes(envelopes, User.Identity.GetUserId())), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult IsEnvelopeNameExists(int id, string envelopeName)
        {
            return Json(inventorySvc.IsEnvelopeNameExists(id, envelopeName), JsonRequestBehavior.AllowGet);
            // return Json(true, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult IsPartNumberExists(int id, string partNumber)
        {
            return Json(inventorySvc.IsPartNumberExists(id, partNumber), JsonRequestBehavior.AllowGet);
            // return Json(true, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult TransferEnvelopeQuantity(int envelopeId, string envelopeName)
        {
            ViewBag.EnvelopeId = envelopeId;
            ViewBag.EnvelopeName = envelopeName;
            ViewBag.Shippers = new SelectList(inventorySvc.ShipperByEnvelopeId(envelopeId), "Id", "Name");
            return PartialView("_TransferEnvelopeQuantity");
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetQuantityByShipperAndEnvelope(int envelopeId, int shipperId)
        {
            var model = inventorySvc.GetQuantityByShipperAndEnvelope(envelopeId, shipperId);
            model = model.HasValue ? model.Value : 0;
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult TransferEnvelopeInventory(EnvelopeTransferInventory envelopeTransferInventory)
        {
            try
            {
                envelopeTransferInventory.TransferredBy = User.Identity.GetUserId();
                envelopeTransferInventory.TransferredOn = DateTime.UtcNow;
                return Json(JsonResponseFactory.SuccessResponse(inventorySvc.TransferEnvelopeInventory(envelopeTransferInventory)), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult EnvelopeStockAndAlertByEnvelopeId(int envelopeId, string envelopeName)
        {
            ViewBag.EnvelopeName = envelopeName;
            var model = inventorySvc.GetEnvelopeInventoryStockCalculation(envelopeId);
            return PartialView("_EnvelopeStockAndAlertDetails", model);
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult ToggleEnvelopeTrackingById(int envelopeId, bool isTracking)
        {
            try
            {
                return Json(JsonResponseFactory.SuccessResponse(inventorySvc.ToggleEnvelopeTrackingById(envelopeId, isTracking, User.Identity.GetUserId())), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Manage Envelopes Inventory
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageEnvelopesInventory()
        {
            return View();
        }
        public ActionResult GetEnvelopsInventory()
        {
            try
            {
                var model = inventorySvc.GetLast3MonthsInventoryEnvelopes();

                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult GetInventoryByEnvelopeId(int id, string envelopeName)
        {
            try
            {
                ViewBag.Name = envelopeName;
                var model = inventorySvc.GetLast3MonthsInventoryByEnvelopeId(id);

                return PartialView("_EnvelopeStockHistory", model);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult AddEnvelopInventory()
        {
            var envelopes = inventorySvc.GetEnvelopes().OrderBy(e => e.Order);
            var shippers = inventorySvc.ShipperSelectList();
            ViewBag.Envelops = new SelectList(envelopes, "Id", "EnvelopeName");
            ViewBag.Shippers = new SelectList(shippers, "Id", "Name");
            return PartialView("_AddEnvelopeInventory");
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult SaveEnvelopeInventory(EnvelopeInventory envelopeInventory)
        {
            try
            {
                return Json(JsonResponseFactory.SuccessResponse(inventorySvc.SaveEnvelopeInventory(envelopeInventory, User.Identity.GetUserId())), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult RemoveEnvelopeInventoryById(int id)
        {
            try
            {
                inventorySvc.RemoveEnvelopeInventoryById(id);
                return Json(JsonResponseFactory.SuccessResponse(new { model = true }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ManageEnvelopesInventoryStock()
        {
            return View();
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetEnvelopeInventoryStock()
        {
            try
            {
                var model = inventorySvc.GetEnvelopeInventoryStockCalculation();

                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult UpdateReorderDuration(int envelopeId, int shipperId, int reorderDuration)
        {
            try
            {
                return Json(JsonResponseFactory.SuccessResponse(inventorySvc.UpdateReorderDuration(envelopeId, shipperId, reorderDuration)), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Manage Envelope Transactions
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetEnvelopeTransactions()
        {
            try
            {
                var model = inventorySvc.GetAllEnvelopeInventory();

                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetEnvelopeTransactionsByName(int id, int envelopeId)
        {
            return PartialView("_AddOrEditEnvelopeTransactions", new EnvelopeInventory() { EnvelopeId = envelopeId });
        }
        //[Authorize(Roles = AMRoles.Admin)]
        //public ActionResult ManageEnvelopeTransactions()
        //{
        //    return View();
        //}

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditEnvelopeTransactions(EnvelopeInventory envelopeInventory)
        {
            try
            {
                return Json(JsonResponseFactory.SuccessResponse(inventorySvc.SaveEnvelopeInventory(envelopeInventory, User.Identity.GetUserId())), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult GetEnvelopeStockDetails(string id)
        {
            try
            {
                return Json(JsonResponseFactory.SuccessResponse(inventorySvc.GetEnvelopeStockDetails(id)), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Manage Print Trails

        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManagePrintTrails()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetPrintTrails()
        {
            try
            {
                var model = supportSvc.GetPrintTrails().OrderByDescending(s => s.SimulateTrail).ToList();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult Simulate(bool simulateTrail, int Id)
        {
            try
            {
                var model = supportSvc.SimulatePrintTrail(Id, simulateTrail);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditPrintTrail(PrintTrail printTrail)
        {
            try
            {
                var model = supportSvc.SavePrintTrail(printTrail);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetPrintTrailById(int id)
        {

            if (id != 0)
            {
                PrintTrail printTrail = supportSvc.GetPrintTrailById(id);
                ViewBag.PrintVendors = new SelectList(supportSvc.GetPrintVendors(), "Id", "Name", printTrail.PrintVendorId);
                return PartialView("_AddOrEditPrintTrail", printTrail);
            }
            else
            {
                ViewBag.PrintVendors = new SelectList(supportSvc.GetPrintVendors(), "Id", "Name");
                return PartialView("_AddOrEditPrintTrail", new PrintTrail());
            }
        }

        #endregion


    }
}