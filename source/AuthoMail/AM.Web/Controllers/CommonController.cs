using AM.Services;
using AM.Web.USPSIMService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using AM.Entities;
using System.Security.Claims;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using AM.Lib;
using AM.Services.Services;
using System.Globalization;
using System.Dynamic;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using Microsoft.AspNet.SignalR.Hosting;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.Delfus + "," + AMRoles.BAOffShore)]
    public class CommonController : BaseController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IReportService repoSvc;
        IHomeService homeSvc;
        ISupportService supSvc;
        IPnSAPIService pnSAPISvc;
        IDocTypeService docTypeSvc;
        IMailPieceService mailpieceSvc;
        ICommonService commonSvc;
        IScannerService scannerSvc;

        private AMSignInManager _signInManager;
        private AMUserManager _userManager;
        //private readonly IClientPermissionService clientPermissionSvc;
        public CommonController(IReportService repoSvc, IHomeService homeSvc, ISupportService supSvc, IPnSAPIService pnsapiSvc, IDocTypeService docTypeSvc, IMailPieceService mailpieceSvc, ICommonService commonSvc, AMSignInManager signInManager, AMUserManager userManager)
        {
            this.repoSvc = repoSvc;
            this.homeSvc = homeSvc;
            this.supSvc = supSvc;
            this.pnSAPISvc = pnsapiSvc;
            this.docTypeSvc = docTypeSvc;
            this.mailpieceSvc = mailpieceSvc;
            this.commonSvc = commonSvc;
            UserManager = userManager;
            SignInManager = signInManager;
            this.scannerSvc = new ScannerService();
            //this.clientPermissionSvc = clientPermissionSvc;
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
        // GET: Common
        [HttpGet]
        public ActionResult _Channel()
        {
            ViewBag.Channel = new SelectList(repoSvc.GetChannelTitle(User.Identity.GetClientId().Value), "Code", "Title");
            return PartialView();
        }
        [HttpPost]
        public ActionResult USPSTracking(string TrackingNo, string ShippingMethod)
        {
            USPSIMClient uspsClient = new USPSIMClient();
            USPSTrackingInfo USPSInfo = uspsClient.GetShipmentStatus(TrackingNo);
            return PartialView("_TrackingInfo", USPSInfo);

        }
        [HttpGet]
        public ActionResult GetClientNameById()
        {
            var model = homeSvc.GetClientNameById(User.Identity.GetClientId().Value);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHeadingForBusinessId()
        {
            var model = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            return PartialView("_GetHeadingForBusinessId", model);
        }

        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.BAOffShore)]
        [HttpGet]
        public async Task<ActionResult> GetClientMenu()
        {
            if (User.IsInRole(AMRoles.Client))
            {
                var result = await homeSvc.GetAllActiveAppCodeForClientRole(User.Identity.GetUserId());
                return Json(JsonResponseFactory.SuccessResponse(result.ToList()), JsonRequestBehavior.AllowGet);
            }
            var model = User.IsInRole(AMRoles.BA) || User.IsInRole(AMRoles.BAOffShore) ? homeSvc.GetAllActiveClient().Where(u => u.Id > 0).ToList() : homeSvc.GetAllActiveClient();

            return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.BAOffShore)]
        [HttpPost]
        public ActionResult RefreshDataByClientId(int clientId)
        {
            int result = homeSvc.SetClientIdToUser(User.Identity.GetUserId(), clientId);

            if (result > 0)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                SignInManager.SignInAsync(user, false, false);
                return Json(JsonResponseFactory.SuccessResponse(result), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonResponseFactory.ErrorResponse("You don't have access for this client."), JsonRequestBehavior.AllowGet);
            }

        }

        //[HttpGet]
        //public async Task<ActionResult> DownloadPdf(string fileName, string appCode, int id)
        //{
        //    try
        //    {
        //        var filePath = Path.Combine(Config.PnS_PCIRootFolder, HttpUtility.UrlEncode(appCode), id.ToString(), "PCIRequest", HttpUtility.UrlEncode(fileName));
        //        HttpClient client = new HttpClient();
        //        client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
        //        HttpResponseMessage response = new HttpResponseMessage();
        //        response = await client.GetAsync($@"{Config.Web_Pdf_API_Url}\DownloadPdf?fileName={filePath}");
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var stream = await response.Content.ReadAsByteArrayAsync();
        //            //return Json(JsonResponseFactory.SuccessResponse(
        //            //new
        //            //{
        //            //    fileName = Path.GetFileNameWithoutExtension(fileName),
        //            //    stream = Convert.ToBase64String(stream)
        //            //}
        //            //));
        //            var pdfFileName = Path.GetFileName(fileName);
        //            return File(stream, "application/pdf", pdfFileName);
        //        }
        //        else
        //        {
        //            return Json(JsonResponseFactory.ErrorResponse("File not found."));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.ToString());
        //        return Json(JsonResponseFactory.ErrorResponse(ex.Message));
        //    }
        //}

        [HttpGet]
        public async Task<ActionResult> DownloadPdf(string uniqueId)
        {
            try
            {
                //HttpClient client = new HttpClient();
                //client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                //HttpResponseMessage response = new HttpResponseMessage();
                //var route = $@"http://localhost:17005/api/PdfAPI/DownloadPdf?fileId={mpId}";
                //response = await client.GetAsync(route);
                //if (response.IsSuccessStatusCode)
                //{
                //    var stream = await response.Content.ReadAsByteArrayAsync();
                //    var pdfFileName = Path.GetFileName(fileName);
                //    return File(stream, "application/pdf", pdfFileName);
                //}
                //else
                //{
                //    return Json(JsonResponseFactory.ErrorResponse("File not found."));
                //}

                var fileInfo = scannerSvc.GetDocumentInfo(uniqueId);

                if (fileInfo == null)
                {
                    //Throw 404 (Not Found) exception if File not found.

                    return Json(JsonResponseFactory.ErrorResponse("File not found."), JsonRequestBehavior.AllowGet);
                }

                var fileName = $"{fileInfo.Id}_{fileInfo.FileName}";

                var filePath = Path.Combine(Config.PnS_PCIRootFolder, fileInfo.AppCode, fileInfo.JobId.ToString(), "PCIRequest", fileName);

                log.Debug($"File Path : {filePath} ");
                //Check whether File exists.
                if (!System.IO.File.Exists(filePath))
                {
                    //Throw 404 (Not Found) exception if File not found.
                    return Json(JsonResponseFactory.ErrorResponse("File not found."), JsonRequestBehavior.AllowGet);
                }

                //Read the File into a Byte Array.
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ReshipMailPiece(int mbId, int mpId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                HttpResponseMessage response = new HttpResponseMessage();

                response = await client.GetAsync($@"{Config.Web_Reship_API_Url}/ReshipPackage?mailBatchId={mbId}&mailPieceId={mpId}");

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(JsonResponseFactory.SuccessResponse(responseString), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse(responseString), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult _PrintVendorList()
        {
            ViewBag.PrintVendors = new SelectList(repoSvc.GetActivePrintVendorsWithLocation(), "PrintVendorCode", "PrintVendorNameCode");
            return PartialView();
        }

        [HttpGet]
        public ActionResult GetReturnAddressInfoByDocTypeName(string doctypeName)
        {
            var doctype = docTypeSvc.GetDocTypeByDocTypeName(User.Identity.GetClientId().Value, doctypeName);
            return Json(JsonResponseFactory.SuccessResponse(doctype), JsonRequestBehavior.AllowGet);
        }
        #region SingleLoanEntry
        [HttpGet]
        public ActionResult SingleLoanEntry(string msg)
        {
            ViewBag.msg = msg;
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByClientIdAndLatestOccurance(User.Identity.GetClientId().Value);
            ViewBag.docTypes = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "DocTypeName", "DocTypeName");
            ViewBag.ClientCode = supSvc.FindByClientId(User.Identity.GetClientId().Value).AppCode;
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> SingleLoanEntry(MailPiece mailPiece, List<HttpPostedFileBase> pdffile, DateTime? ExpiryDate)
        {
            SPELogInfo logInfo = new SPELogInfo();
            logInfo.ClientId = User.Identity.GetClientId().Value;
            logInfo.CreatedBy = new Guid(User.Identity.GetUserId());
            logInfo.CreatedOn = Common.Now;

            if (mailPiece.eDelivery != null)
            {
                //Email Validation
                if (mailPiece.eDelivery.PrintAndShipOnExpiry && ExpiryDate == null)
                {
                    var errorMsg = "Expiry Date must be future date.";
                    logInfo.ErrorDesc = errorMsg;
                    logInfo.Status = ShippingStatus.Error.ToString();
                    mailpieceSvc.InsertSPELogInfo(logInfo);

                    ModelState.AddModelError("", errorMsg);

                    goto Error;
                }
                if (mailPiece.eDelivery.PrintAndShipOnExpiry)
                {
                    mailPiece.eDelivery.ExpiryDate = ExpiryDate.Value;
                }
                else
                {
                    mailPiece.eDelivery = null;
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (pdffile[0] == null)
                    {
                        var errorMsg = "Please upload PDF file.";
                        logInfo.ErrorDesc = errorMsg;
                        logInfo.Status = ShippingStatus.Error.ToString();
                        mailpieceSvc.InsertSPELogInfo(logInfo);

                        ModelState.AddModelError("", errorMsg);

                        goto Error;
                    }

                    // store files in temp folder
                    string root = Config.MailRequestAPI_FilesDownloadPath;
                    string downloadPath = System.IO.Path.Combine(root, "TempWebFiles");
                    List<string> downloadedFiles = new List<string>();
                    if (!Directory.Exists(downloadPath))
                        Directory.CreateDirectory(downloadPath);


                    mailPiece.ClientCode = supSvc.FindByClientId(User.Identity.GetClientId().Value).AppCode;

                    if (mailPiece.ClientCode == "LOANCARE")
                    {
                        CustomTemplate customTemplate = commonSvc.GetCustomTemplateByCommunityCode(mailPiece.ProductCode);

                        var templateFilePath = Server.MapPath($"/Template/{customTemplate.CustomTemplatePath}");
                        var base64Template = Convert.ToBase64String(System.IO.File.ReadAllBytes(templateFilePath));
                        dynamic objLoanCareReq = new ExpandoObject();
                        objLoanCareReq.Data = new
                        {
                            Comment = mailPiece.Comment
                        };

                        objLoanCareReq.Settings = new
                        {
                            MailMergeOptions = "RemoveEmptyParagraphs",
                            ReportEngineOptions = "RemoveEmptyParagraphs",
                            OutputDocType = "pdf",
                            Template = base64Template
                        };

                        objLoanCareReq.TransformData = null;

                        string coverletterPdfPath = Path.Combine(downloadPath, $"{mailPiece.ProductCode}_{Common.Today.ToString("MMddyyhhmmss")}.pdf");
                        await GenerateDoc(objLoanCareReq, coverletterPdfPath);
                        downloadedFiles.Add(coverletterPdfPath);
                    }

                    // sequence validation
                    if (pdffile.Count > 1)
                    {
                        foreach (var file in pdffile)
                        {
                            int value;
                            if (!int.TryParse(file.FileName.Split('_')[0], out value))
                            {
                                var errorMsg = $"The pdf {file.FileName} does not contain the order sequence number.";
                                logInfo.ErrorDesc = errorMsg;
                                logInfo.Status = ShippingStatus.Error.ToString();
                                mailpieceSvc.InsertSPELogInfo(logInfo);

                                ModelState.AddModelError("", errorMsg);

                                goto Error;
                            }
                        }
                        var orderedPdfsList = pdffile.Select(x => new { FileObject = x, FileOrder = int.Parse(x.FileName.Split('_')[0]) }).OrderBy(x => x.FileOrder);
                        foreach (var file in orderedPdfsList)
                        {
                            string downloadfilePath = System.IO.Path.Combine(downloadPath, file.FileObject.FileName);
                            file.FileObject.SaveAs(downloadfilePath);
                            downloadedFiles.Add(downloadfilePath);
                        }
                    }
                    else
                    {
                        string downloadfilePath = System.IO.Path.Combine(downloadPath, pdffile[0].FileName);
                        pdffile[0].SaveAs(downloadfilePath);
                        downloadedFiles.Add(downloadfilePath);
                    }


                    var mailRequestController = new MailRequestController(pnSAPISvc, docTypeSvc, mailpieceSvc);
                    var result = await mailRequestController.CreateAPIMailRequestAsync(mailPiece, downloadedFiles);

                    var response = result.GetValueOfPropertyFromPropertyName("Value");

                    var mailAPIResponse = (MailAPIResponse)response;

                    if (mailAPIResponse.MailingStatus == "Error")
                    {
                        var errorMsg = mailAPIResponse.ErrorDesc;
                        logInfo.ErrorDesc = errorMsg;
                        logInfo.Status = ShippingStatus.Error.ToString();
                        mailpieceSvc.InsertSPELogInfo(logInfo);

                        ModelState.AddModelError("", mailAPIResponse.ErrorCode);
                        ModelState.AddModelError("", errorMsg);

                        goto Error;
                    }
                    return RedirectToAction("SingleLoanEntry", new { msg = mailAPIResponse.Message });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
        Error:
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.docTypes = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "DocTypeName", "DocTypeName");
            return View(mailPiece);
        }

        private async Task GenerateDoc(dynamic objLoanCareReq, string pdfFilePath)
        {
            using (var httpClient = new HttpClient())
            using (var stringContent = new StringContent(JsonConvert.SerializeObject(objLoanCareReq), Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage httpResponse = await httpClient.PostAsync(Config.Common_DocGen_API, stringContent);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                if (!httpResponse.IsSuccessStatusCode || httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    return;
                }
                var docGenResponse = JsonConvert.DeserializeObject<DocGenResponse>(responseString);

                System.IO.File.WriteAllBytes(pdfFilePath, docGenResponse.DocumentData);
            }
        }

        #endregion SingleLoanEntry

        [HttpGet]
        public ActionResult ReshipPackage()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeName");
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            ViewBag.startDate = (estToday + new TimeSpan(0, 0, 0)).ToString("MM/dd/yy");
            ViewBag.endDate = (estToday + new TimeSpan(23, 59, 59)).ToString("MM/dd/yy");

            return View();
        }

        [HttpPost]
        public ActionResult _GetReshipMailPieceList(string businessFriendlyIdName, string businessFriendlyIdValue, int? docType, string startDate, string endDate)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

            //Ref only. remove it. also check doctype is required or not.
            var model = repoSvc.GetReshipRequestList(businessFriendlyIdName, businessFriendlyIdValue, docType, exactStartDate, exactEndDate, User.Identity.GetClientId().Value);

            model.ForEach(x =>
            {
                x.IsWithinRetentionPeriod = CheckWithinRetentionPeriod(x.RetentionPeriod, Common.Now, x.ReceivedOn.Value, x.ClientId);
            });
            //var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.Status == "InProgress").ThenBy(s => s.IsPullBackRequested).ThenByDescending(s => s.Status == "PulledBack").ThenByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        private bool CheckWithinRetentionPeriod(int retentionDays, DateTime now, DateTime receivedOn, int clientId)
        {
            int days = retentionDays;
            DateTime startDate = now;
            do
            {
                startDate = startDate.AddDays(-1);
                days -= IsBusinessDay(startDate, Config.GetHolidaysByClientId(clientId), false, false) ? 1 : 0;
            } while (0 != days);

            return startDate < receivedOn;
        }
        private static bool IsBusinessDay(DateTime date, List<Holiday> holidays, bool isSatWorkDay, bool isSunWorkDay)
        {
            if (holidays.Select(c => c.HolidayDate).Contains(date.Date))
                return false;

            if (date.DayOfWeek == DayOfWeek.Saturday)
                return isSatWorkDay;

            if (date.DayOfWeek == DayOfWeek.Sunday)
                return isSunWorkDay;

            return true;
        }
    }
}