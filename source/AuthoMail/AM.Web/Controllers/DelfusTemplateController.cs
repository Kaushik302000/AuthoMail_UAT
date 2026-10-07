using AM.Entities;
using AM.Services;
using AM.Services.Services;
using DELFUS.TemplateImporter;
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AM.Web.Models;
using System.Diagnostics;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.Delfus)]
    public class DelfusTemplateController : Controller
    {
        IDelfusTemplateService delfusTemplateSvc;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public DelfusTemplateController(IDelfusTemplateService delfusTemplateSvc)
        {
            Aspose.Words.License license = new Aspose.Words.License();
            license.SetLicense("Aspose.Words.lic");
            this.delfusTemplateSvc = delfusTemplateSvc;
        }
        // GET: Template
        [HttpGet]
        public ActionResult Templates()
        {
            return View();
        }
        [HttpGet]
        public ActionResult TemplateSummary()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SaveDelfusTemplates(HttpPostedFileBase template)
        {

            try
            {
                log.Info("Delfus save template");
                IEnumerable<string> mastermergeList = await delfusTemplateSvc.GetDelfusMergeList();
                var largeFontCoupon = System.IO.File.ReadAllBytes(Server.MapPath("/Template/LargeFontCoupon.docx"));
                TemplateValidation validation = new TemplateValidation();
                byte[] fileData = await GetByteArrayFromFile(template);
                log.Info("Delfus GetByteArrayFromFile");
                TemplateValidationInfo validationInfo;
                if (template.FileName.Contains("_16PT"))
                {
                    validationInfo = validation.VerifyTemplate(fileData, mastermergeList.ToList());
                }
                else
                {
                    validationInfo = validation.VerifyTemplate(fileData, mastermergeList.ToList(), largeFontCoupon);
                }

                string error = string.Empty;
                string[] fileType = { ".doc", ".docx" };
                log.Info("Delfus VerifyTemplate");
                if (!fileType.Contains(Path.GetExtension(template.FileName)))
                {
                    error = "Selected file type is not supported.";
                }
                if (Path.GetFileNameWithoutExtension(template.FileName).Length != 10 && !template.FileName.Contains("_16PT"))
                {
                    string newline = error.Length > 0 ? " " : "";
                    error += $"{newline}Invalid template name. (template name should have 10 characters)";
                }
                if (validationInfo.HasAnchorTagIssue)
                {
                    string newline = error.Length > 0 ? " " : "";
                    error += $"{newline}This template has anchor tag issue.";
                }
                if (validationInfo.NewMergeFields != null && validationInfo.NewMergeFields.Count > 0)
                {
                    string newline = error.Length > 0 ? " " : "";
                    error += $"{newline}This template has below merge field(s) which does not exist in Vendor file. ({string.Join(", ", validationInfo.NewMergeFields)}).";
                }
                log.Info("Delfus error check" + error);
                if (!string.IsNullOrEmpty(error))
                {
                    await delfusTemplateSvc.SaveTemplate(
                         new DelfusTemplate
                         {
                             FileName = Path.GetFileNameWithoutExtension(template.FileName),
                             FilePath = template.FileName,
                             ModifiedOn = DateTime.UtcNow,
                             ModifiedBy = User.Identity.DisplayName(),
                             Status = "Invalid",
                             Reason = error
                         }
                         );
                    return Json(JsonResponseFactory.ErrorResponse(error));
                }
                //}

                // ToDo: UsesAgencyAddress implementation
                List<string> errors = new List<string>();
                string uploadURL = $@"{Config.Web_Delfus_API_Url}\SaveTemplate";// ConfigurationManager.AppSettings["DelfusTemplateUploadUrl"].ToString();
                bool isTemplateExists = await IsTemplateExists(template.FileName);
                log.Info("Delfus IsTemplateExists");
                var uploadResult = await UploadFileToServer(template.FileName, fileData, uploadURL);
                log.Info("Delfus UploadFileToServer");

                if (validationInfo.Doc16PT != null)
                {
                    var fileExt = Path.GetExtension(template.FileName);
                    var newFileName = $"{Path.GetFileNameWithoutExtension(template.FileName)}_16PT{fileExt}";
                    log.Info("Delfus Large Upload File Upload Started");
                    var largeUploadResult = await UploadFileToServer(newFileName, validationInfo.Doc16PT, uploadURL);
                    log.Info("Delfus Large Upload File Upload Completed");
                }

                if (uploadResult.Key)
                {
                    var result = await delfusTemplateSvc.SaveTemplate(
                         new DelfusTemplate
                         {
                             FileName = Path.GetFileNameWithoutExtension(template.FileName),
                             FilePath = template.FileName,
                             ModifiedOn = DateTime.UtcNow,
                             ModifiedBy = User.Identity.DisplayName(),
                             Status = isTemplateExists ? "Modified" : "Added"
                         }
                         );
                    log.Info("Delfus success");
                    return Json(JsonResponseFactory.SuccessResponse("Template uploaded successfully."));
                }
                else
                {
                    errors.Add(uploadResult.Value);
                    await delfusTemplateSvc.SaveTemplate(
                         new DelfusTemplate
                         {
                             FileName = Path.GetFileNameWithoutExtension(template.FileName),
                             FilePath = template.FileName,
                             ModifiedOn = DateTime.UtcNow,
                             ModifiedBy = User.Identity.DisplayName(),
                             Status = "Invalid",
                             Reason = uploadResult.Value
                         }
                         );
                    log.Info("Delfus Error" + string.Join(",", error));
                }
                return Json(JsonResponseFactory.ErrorResponse(errors));

            }
            catch (Exception e)
            {
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                log.Error(+line + ":" + e.ToString());
                return Json(JsonResponseFactory.ErrorResponse(e.Message));
            }
            //HttpPostedFileBase template = templates.FirstOrDefault();


        }
        [HttpPost]
        public async Task<ActionResult> DownloadDelfusTemplate(string fileName)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                HttpResponseMessage response = new HttpResponseMessage();
                response = await client.GetAsync($@"{Config.Web_Delfus_API_Url}\DownloadTemplate?fileName={fileName}");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsByteArrayAsync();
                    return Json(JsonResponseFactory.SuccessResponse(
                    new
                    {
                        fileName = Path.GetFileNameWithoutExtension(fileName),
                        stream = Convert.ToBase64String(stream)
                    }
                    ));
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse("Template not found."));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return Json(JsonResponseFactory.ErrorResponse(ex.Message));
            }
        }
        [HttpGet]
        public async Task<object> DelfusTemplatesList()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                HttpResponseMessage response = new HttpResponseMessage();
                response = await client.GetAsync($@"{Config.Web_Delfus_API_Url}\GetTemplates");
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { data = await response.Content.ReadAsAsync<List<FileDetails>>() }, JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Json(JsonResponseFactory.ErrorResponse(e.Message));
            }
        }
        [HttpGet]
        public async Task<object> DelfusTemplatesSummary()
        {
            try
            {
                var data = await delfusTemplateSvc.DelfusTemplateSummary();
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<object> DelfusTemplatesHistory(string fileName)
        {
            try
            {
                var data = await delfusTemplateSvc.DelfusTemplatesByFileName(fileName);
                return Json(new { data = data.ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> RemoveDelfusTemplate(string fileName)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                HttpResponseMessage response = new HttpResponseMessage();
                var values = new Dictionary<string, string> { { "fileName", fileName } };
                var content = new FormUrlEncodedContent(values);
                response = await client.PostAsync($@"{Config.Web_Delfus_API_Url}\DeleteTemplate", content);
                if (response.IsSuccessStatusCode)
                {
                    await delfusTemplateSvc.SaveTemplate(
                     new DelfusTemplate
                     {
                         FileName = Path.GetFileNameWithoutExtension(fileName),
                         FilePath = fileName,
                         ModifiedOn = DateTime.UtcNow,
                         ModifiedBy = User.Identity.DisplayName(),
                         Status = "Deleted"
                     }
                     );
                    return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Json(JsonResponseFactory.ErrorResponse(e.Message));
            }
        }

        public async Task<bool> IsTemplateExists(string fileName)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                HttpResponseMessage response = new HttpResponseMessage();
                response = await client.GetAsync($@"{Config.Web_Delfus_API_Url}\IsTemplateExists?fileName={fileName}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {

                log.Error(e.ToString());
                return false;
            }
        }

        public async Task<KeyValuePair<bool, string>> UploadFileToServer(string fileName, byte[] fileData, string uploadURL)
        {

            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                client.DefaultRequestHeaders.Add("FileName", fileName);
                MultipartFormDataContent form = new MultipartFormDataContent();
                HttpContent content = new StringContent("file");
                using (MemoryStream memoryStream = new MemoryStream(fileData))
                {
                    content = new StreamContent(memoryStream);
                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "file",
                        FileName = fileName
                    };
                    form.Add(content, "file", fileName);
                    HttpResponseMessage response = await client.PostAsync(uploadURL, form);
                    log.Info(response.StatusCode + ": Delfus Uploaded");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<string>();
                        return new KeyValuePair<bool, string>(true, result);
                    }
                    else
                    {
                        return new KeyValuePair<bool, string>(false, "File not uploaded, Please try again.");
                    }

                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return new KeyValuePair<bool, string>(false, e.Message);
            }

        }

        public async Task<byte[]> GetByteArrayFromFile(HttpPostedFileBase fileBase)
        {
            try
            {
                using (MemoryStream target = new MemoryStream())
                {
                    await fileBase.InputStream.CopyToAsync(target);
                    return target.ToArray();
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return null;
            }

        }

    }
}