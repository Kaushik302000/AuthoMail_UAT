using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace AM.Scanner.Controllers
{
    [RoutePrefix("api/PdfAPI")]
    public class PdfOperationAPIController : ApiController
    {
        IScannerService scannerSvc;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PdfOperationAPIController()
        {
            this.scannerSvc = new ScannerService();
        }


        [HttpGet]
        [Route("DownloadPdf")]
        public HttpResponseMessage DownloadPdf(int fileId)
        {
            try
            {
                if (Unauthorized(HttpContext.Current.Request))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                //Create HTTP Response.
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

                var fileInfo = scannerSvc.GetDocumentInfo(fileId);

                if (fileInfo == null)
                {
                    //Throw 404 (Not Found) exception if File not found.
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ReasonPhrase = $"File not found: {fileId}.";
                    return response;
                }

                var fileName = $"{fileInfo.Id}_{fileInfo.FileName}";

                var filePath = Path.Combine(Common.Common.PCIRootPath, fileInfo.AppCode, fileInfo.JobId.ToString(), "PCIRequest", fileName);

                log.Debug($"File Path : {filePath} ");
                //Check whether File exists.
                if (!File.Exists(filePath))
                {
                    //Throw 404 (Not Found) exception if File not found.
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ReasonPhrase = $"File not found: {fileName}.";
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
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(filePath));
                return response;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpGet]
        [Route("DownloadEVaultPdf")]
        public HttpResponseMessage DownloadEVaultPdf(string fileName)
        {
            try
            {
                if (UnauthorizedEVault(HttpContext.Current.Request))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //Create HTTP Response.
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

                //Check whether File exists.
                if (!File.Exists(fileName))
                {
                    //Throw 404 (Not Found) exception if File not found.
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ReasonPhrase = string.Format("File not found: {0} .", Path.GetFileName(fileName));
                    return response;
                }

                //Read the File into a Byte Array.
                byte[] bytes = File.ReadAllBytes(fileName);

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

        public bool UnauthorizedEVault(HttpRequest request)
        {
            if (request.Headers["Id"].Trim() != "dc4c269c-0eea-424a-8be9-0cc75eb527e9")
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