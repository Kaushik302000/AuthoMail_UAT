using AM.Lib;
using AM.Scanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace AM.Scanner.Controllers
{
    [RoutePrefix("api/Delfus")]
    public class DelfusTemplateAPIController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [HttpGet]
        [Route("DownloadTemplate")]
        public HttpResponseMessage DownloadDelfusTemplate(string fileName)
        {
            try
            {
                if (Unauthorized(HttpContext.Current.Request))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //Create HTTP Response.
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);



                string filePath = string.Empty;
                //Set the File Path.
                if (fileName.Contains("_16PT"))
                {
                    filePath = Path.Combine(ConfigurationManager.AppSettings["DelfusTemplatePath"].ToString(), "16PT", fileName);
                }
                else
                {
                    filePath = Path.Combine(ConfigurationManager.AppSettings["DelfusTemplatePath"].ToString(), fileName);
                }

                //Check whether File exists.
                if (!File.Exists(filePath))
                {
                    //Throw 404 (Not Found) exception if File not found.
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ReasonPhrase = string.Format("File not found: {0} .", fileName);
                    return response;
                }

                //Read the File into a Byte Array.
                byte[] bytes = File.ReadAllBytes(filePath);

                //Set the Response Content.
                response.Content = new ByteArrayContent(bytes);

                //Set the Response Content Length.
                response.Content.Headers.ContentLength = bytes.LongLength;

                //Set the Content Disposition Header Value and FileName.
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = fileName;

                //Set the File Content Type.
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));
                return response;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpGet]
        [Route("GetTemplates")]
        public IHttpActionResult GetDelfusTemplates()
        {
            try
            {
                if (Unauthorized(HttpContext.Current.Request))
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }
                DirectoryInfo d = new DirectoryInfo(ConfigurationManager.AppSettings["DelfusTemplatePath"].ToString());//Assuming Test is your Folder
                DirectoryInfo largeFontDir = new DirectoryInfo(Path.Combine(ConfigurationManager.AppSettings["DelfusTemplatePath"].ToString(), "16PT"));//Assuming Test is your Folder
                List<FileDetails> fileDetails = new List<FileDetails>();
                string[] fileType = { ".doc", ".docx" };
                FileInfo[] fileInfos = d.GetFiles("*.*").Where(f => fileType.Contains(Path.GetExtension(f.FullName))).ToArray();
                foreach (var fileInfo in fileInfos)
                {
                    fileDetails.Add(new FileDetails
                    {
                        FileName = fileInfo.Name,
                        FileSize = fileInfo.Length > 0 ? fileInfo.Length / 1024 : 0,
                        Status = true,
                        ModifiedOn = fileInfo.LastWriteTimeUtc,
                        IsLargeTemplateExist = largeFontDir.GetFiles($"{Path.GetFileNameWithoutExtension(fileInfo.Name)}_16PT.*").Any()
                    });
                }
                return Ok(fileDetails); //Getting Text files
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return BadRequest(e.Message);
            }

        }

        [HttpPost]
        [Route("DeleteTemplate")]
        public IHttpActionResult DeleteTemplate()
        {
            try
            {
                var request = HttpContext.Current.Request;
                if (Unauthorized(request))
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }
                string filePath = Path.Combine(ConfigurationManager.AppSettings["DelfusTemplatePath"].ToString(), request.Params["fileName"].Trim());

                //Check whether File exists.
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Ok();

                }
                else
                {
                    //Throw 404 (Not Found) exception if File not found.
                    return NotFound();
                }
            }
            catch (Exception e)
            {

                log.Error(e.ToString());
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("SaveTemplate")]
        public IHttpActionResult SaveTemplate()
        {
            try
            {
                var request = HttpContext.Current.Request;
                if (Unauthorized(request))
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }
                string path = Path.Combine(ConfigurationManager.AppSettings["DelfusTemplatePath"].ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (request.Headers["FileName"].Trim().Contains("_16PT"))
                {
                    path = Path.Combine(path, "16PT");
                }
                request.Files[0].SaveAs(Path.Combine(path, request.Headers["FileName"].Trim()));
                return Ok("File save successfully.");
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("IsTemplateExists")]
        public IHttpActionResult IsTemplateExists(string fileName)
        {
            try
            {
                if (Unauthorized(HttpContext.Current.Request))
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }
                string filePath = Path.Combine(ConfigurationManager.AppSettings["DelfusTemplatePath"].ToString(), fileName);

                //Check whether File exists.
                if (File.Exists(filePath))
                {
                    return Ok();

                }
                else
                {
                    //Throw 404 (Not Found) exception if File not found.
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return BadRequest(e.Message);
            }

        }

        public bool Unauthorized(HttpRequest request)
        {
            if (request.Headers["Id"].Trim() != "5b189031-eecc-42d7-8b6d-b531caf1036e")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
