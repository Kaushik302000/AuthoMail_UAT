using AM.Entities;
using AM.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Serialization;
using AM.Lib;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using System.Diagnostics;
using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Xml.Schema;
using System.Xml;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Globalization;
using iTextSharp.text.exceptions;
using System.Collections.ObjectModel;
using AM.Services.Services;
using System.Web;
using Org.BouncyCastle.Ocsp;
using Microsoft.AspNet.Identity;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.API)]
    [RoutePrefix("api")]
    public class MailRequestController : ApiController
    {
        private static XmlSchemaSet schemas = new XmlSchemaSet();

        IPnSAPIService pnsApiSvc;
        IDocTypeService doctypeSvc;
        IMailPieceService mailPieceSvc;
        public MailRequestController(IPnSAPIService pnsApiSvc, IDocTypeService doctypeSvc, IMailPieceService mailPieceSvc)
        {
            this.pnsApiSvc = pnsApiSvc;
            this.doctypeSvc = doctypeSvc;
            this.mailPieceSvc = mailPieceSvc;
        }

        static MailRequestController()
        {
            schemas.Add(null, XmlReader.Create(new StringReader(AM.Entities.Properties.Resources.MailingRequest)));
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //[Route("test")]
        //[HttpPost]
        //public void test()
        //{
        //    File.WriteAllText(@"\\Slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_PnS\Test\test.txt", "Jai Swaminarayan");
        //}

        public async Task<HttpResponseMessage> CreateAMBatch(MailBatch mailBatch, MailPiece mailPiece, SFTChannel sFTChannel, DateTime receivedOn)
        {
            try
            {
                string tempZipFilePath = Path.Combine(Config.MailRequestAPI_FilesDownloadPath, "TempZipFiles");

                mailPiece.DocumentOrPackage.IncludePatternBaseString = true;
                //create zip and response in local path first
                string zipFilePath = Path.Combine(tempZipFilePath, string.Format("{0}.zip", mailBatch.Id));
                string triggerFilePath = Path.Combine(tempZipFilePath, string.Format("{0}.GO", mailBatch.Id));

                string xml = XmlUtil.Serialize(mailBatch);

                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        // ZipFile.CreateFromDirectory("", "", CompressionLevel.Fastest, false);
                        ZipArchiveEntry xmlEntry = archive.CreateEntry(string.Format("{0}.xml", mailBatch.Id));
                        using (StreamWriter writer = new StreamWriter(xmlEntry.Open()))
                        {
                            await writer.WriteAsync(xml);
                        }
                        if (!mailPiece.DocumentOrPackage.Path.IsNullOrWhiteSpace())
                        {
                            ZipArchiveEntry pdfEntry = archive.CreateEntry(mailPiece.DocumentOrPackage.Name);
                            using (var pdfStream = pdfEntry.Open())
                            using (var payloadStream = new FileStream(mailPiece.DocumentOrPackage.Path, FileMode.Open, FileAccess.Read))
                            {
                                await payloadStream.CopyToAsync(pdfStream, 81920);
                            }
                        }
                    }
                }

                if (!File.Exists(triggerFilePath))
                {
                    using (FileStream sourceStream = new FileStream(triggerFilePath,
FileMode.Create, FileAccess.Write, FileShare.None,
bufferSize: 4096, useAsync: true)) { }

                    File.SetLastWriteTime(triggerFilePath, receivedOn);
                }

                //Once files are created to it local path. then after we will move zip and then trigger to its shared path
                string destZipFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.zip", mailBatch.Id));
                string destTriggerFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.GO", mailBatch.Id));

                File.Move(zipFilePath, destZipFilePath);
                File.Move(triggerFilePath, destTriggerFilePath);

                if (File.Exists(mailPiece.DocumentOrPackage.Path)) File.Delete(mailPiece.DocumentOrPackage.Path);

                var res = new MailAPIResponse
                {
                    Id = mailPiece.Id,
                    MailingStatus = ShippingStatus.Acknowledgement.ToString(),
                    Message = "Your request is queued for processing."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                var traceId = Guid.NewGuid();

                //await StoreInvalidOrBadRequest(JsonConvert.SerializeObject(mailPiece), new List<string>() { mailPiece.DocumentOrPackage.Path }, Path.Combine(Config.MailRequestAPI_FilesDownloadPath, "BadRequests"), traceId, ex.ToString());

                log.Error("TraceId: {0}, Message: {1}".Fill(traceId, ex.Message), ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unexpected error. TraceId : {0}".Fill(traceId));
            }
        }

        [Route("mailrequest")]
        [HttpPost]
        public async Task<HttpResponseMessage> CreateMailRequest()
        {
            var receivedOn = Common.Now;
            RequestLog requestLog = new RequestLog();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var requestSize = Request.Content.Headers.ContentLength.Value;
            // Logging Request Size just to confirm actual value
            //log.DebugFormat("Files Downloaded: {0}", sw.Elapsed.TotalSeconds);
            log.DebugFormat("RequestSizeByte: {0}, RequestSizeKB: {1}", requestSize, requestSize / 1024f);
            var mailPiece = new MailPiece();
            bool isCLPackageId = false;
            string[] manifest = new string[] { };
            string root = Config.MailRequestAPI_FilesDownloadPath;
            List<string> downloadedFiles = new List<string>();
            string errorPath = Path.Combine(root, "BadRequests");
            var addPackageResult = new AddPackageResult();
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                Request.Headers.TryGetValues("MailPieceId", out var mailPieceHeader);
                Guid mpId;

                if (mailPieceHeader == null || mailPieceHeader.FirstOrDefault().IsNullOrWhiteSpace())
                {
                    mpId = Guid.NewGuid();
                    isCLPackageId = true;
                }
                else if (!Guid.TryParse(mailPieceHeader.First(), out mpId))
                {
                    addPackageResult = await pnsApiSvc.AddPackage(null, User.Identity.GetClientId().Value, receivedOn, requestSize / 1024f);

                    string msg = $"MailPieceId must be a valid GUID: {mailPieceHeader.First()}";
                    sw.Stop();
                    requestLog.Id = addPackageResult.Id;
                    requestLog.MailPieceId = null;
                    requestLog.Status = OutComeCodes.Invalid_MailPieceId;
                    requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                    requestLog.APIResponse = msg;
                    requestLog.ReceivedOn = receivedOn;
                    requestLog.InitialChecks = sw.Elapsed.TotalSeconds;
                    await pnsApiSvc.UpdatePackageStatus(requestLog);

                    var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_MailPieceId, ErrorMessage = msg }, receivedOn);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }

                // Logging Request Size with MailPieceId just to confirm actual value
                log.DebugFormat("MailPieceId: {0}, RequestSizeByte: {1}, RequestSizeKB: {2}", mpId, requestSize, requestSize / 1024f);
                addPackageResult = await pnsApiSvc.AddPackage(mpId, User.Identity.GetClientId().Value, receivedOn, requestSize / 1024f);
                HttpStatusCode httpStatusCode = (HttpStatusCode)addPackageResult.HTTPStatusCode;

                //Check mailpiece id is unique.
                if (httpStatusCode == HttpStatusCode.Conflict || httpStatusCode == HttpStatusCode.NotAcceptable || httpStatusCode == HttpStatusCode.BadRequest)
                {
                    string msg = string.Empty;
                    string outcomeCode = string.Empty;
                    if (httpStatusCode == HttpStatusCode.Conflict)
                    {
                        msg = $"The specified package id '{mpId}' conflicts with an existing package. Retry with a new GUID.";
                        outcomeCode = OutComeCodes.Invalid_DuplicateKey;
                    }
                    else if (httpStatusCode == HttpStatusCode.NotAcceptable)
                    {
                        msg = $"A previous request with the specified package id '{mpId}' is being processed.";
                        outcomeCode = OutComeCodes.NotAcceptable;
                    }
                    else if (httpStatusCode == HttpStatusCode.BadRequest)
                    {
                        msg = $"Maximum retries exceeded for package id '{mpId}'.";
                        outcomeCode = OutComeCodes.MaxRetriesExceeded;
                    }

                    //await StoreInvalidOrBadRequest(string.Empty, provider.FileData, errorPath, mpId, msg, addPackageResult.Id);
                    //DeleteFiles(downloadedFiles);

                    sw.Stop();
                    requestLog.Id = addPackageResult.Id;
                    requestLog.MailPieceId = mpId;
                    requestLog.Status = outcomeCode;
                    requestLog.HTTPStatusCode = (int)httpStatusCode;
                    requestLog.APIResponse = msg;
                    requestLog.ReceivedOn = receivedOn;
                    requestLog.InitialChecks = sw.Elapsed.TotalSeconds;
                    await pnsApiSvc.UpdatePackageStatus(requestLog);
                    mailPiece.Id = mpId;
                    var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = outcomeCode, ErrorMessage = msg }, receivedOn);
                    return Request.CreateResponse(httpStatusCode, res);
                }

                if (requestSize / 1024f / 1024f > Config.MailRequestAPI_MaxRequestSize)
                {
                    sw.Stop();
                    requestLog.Id = addPackageResult.Id;
                    requestLog.MailPieceId = mpId;
                    requestLog.Status = OutComeCodes.RequestTooLarge;
                    requestLog.HTTPStatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
                    requestLog.APIResponse = "Maximum request length exceeded.";
                    requestLog.ReceivedOn = receivedOn;
                    requestLog.InitialChecks = sw.Elapsed.TotalSeconds;
                    await pnsApiSvc.UpdatePackageStatus(requestLog);
                    mailPiece.Id = mpId;
                    var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.RequestTooLarge, ErrorMessage = "Maximum request length exceeded." }, receivedOn);
                    return Request.CreateResponse(HttpStatusCode.RequestEntityTooLarge, res);
                }

                sw.Stop();
                requestLog.InitialChecks = sw.Elapsed.TotalSeconds;
                sw.Restart();

                try
                {
                    await Request.Content.ReadAsMultipartAsync(provider);
                }
                catch (IOException e)
                {
                    if (e.InnerException.ToString().Contains("client disconnected"))
                    {
                        string msg = "The client disconnected.";

                        await StoreInvalidOrBadRequest(manifest.Length == 0 ? string.Empty : manifest[0], provider.FileData, errorPath, mpId, e.ToString(), addPackageResult.Id);

                        sw.Stop();
                        requestLog.Id = addPackageResult.Id;
                        requestLog.MailPieceId = mpId;
                        requestLog.Status = OutComeCodes.NotAcceptable;
                        requestLog.HTTPStatusCode = (int)HttpStatusCode.NotAcceptable;
                        requestLog.ReceivedOn = receivedOn;
                        requestLog.APIResponse = msg;
                        requestLog.ErrorLog = e.ToString();
                        await pnsApiSvc.UpdatePackageStatus(requestLog);
                        mailPiece.Id = mpId;
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.NotAcceptable, ErrorMessage = msg }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, res);
                    }
                    else
                    {
                        throw;
                    }
                }
                sw.Stop();
                requestLog.Download = sw.Elapsed.TotalSeconds;
                sw.Restart();

                SFTChannel sftChannel;
                Client client;
                MailBatch mailBatch = new MailBatch()
                {
                    Id = Guid.NewGuid()
                };
                log.DebugFormat("MailPieceId: {0} of MailBatchId: {1}", mpId, mailBatch.Id);
                manifest = provider.FormData.GetValues("Manifest");
                var filekeys = provider.FileData.Select(f => f.Headers.ContentDisposition.Name).ToList();
                //validation for "Manifest"
                string manifestErrDesc = ValidateManifestRequest(ref mailPiece, mpId, isCLPackageId, manifest, filekeys);
                downloadedFiles = provider.FileData.Select(f => f.LocalFileName).ToList();
                if (!manifestErrDesc.IsNullOrWhiteSpace())
                {
                    // delete downloaded files in case of exception, send path, line no, position in bad request
                    await StoreInvalidOrBadRequest(manifest == null ? string.Empty : manifest[0], provider.FileData, errorPath, mailPiece.Id, manifestErrDesc, addPackageResult.Id);
                    DeleteFiles(downloadedFiles);

                    sw.Stop();
                    requestLog.Id = addPackageResult.Id;
                    requestLog.MailPieceId = mailPiece.Id;
                    requestLog.Status = OutComeCodes.Manifest_Missing_Or_Invalid;
                    requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                    requestLog.APIResponse = manifestErrDesc;
                    requestLog.ReceivedOn = receivedOn;
                    requestLog.Validate = sw.Elapsed.TotalSeconds;
                    await pnsApiSvc.UpdatePackageStatus(requestLog);

                    var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = manifestErrDesc }, receivedOn);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }

                using (IClientService clientService = new ClientService())
                {
                    sftChannel = await clientService.GetSFTChannelDetailsBySFTChannelId(User.Identity.SFTChannelId().Value);
                }

                // Validate manifest contents
                mailBatch.MailPieces.Add(mailPiece);

                using (IClientService clientService = new ClientService())
                {
                    if (sftChannel.Code.Contains("API_AMC"))
                    {
                        var clientId = await clientService.GetAMCClientIdByCustId(mailPiece.ClientCode);

                        if (clientId == null)
                        {
                            string msg = "Invalid client code specified.";

                            await StoreInvalidOrBadRequest(manifest[0], provider.FileData, errorPath, mailPiece.Id, msg, addPackageResult.Id);

                            DeleteFiles(downloadedFiles);

                            sw.Stop();
                            requestLog.Id = addPackageResult.Id;
                            requestLog.MailPieceId = mailPiece.Id;
                            requestLog.Status = OutComeCodes.Invalid_ClientCode;
                            requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                            requestLog.APIResponse = msg;
                            requestLog.ReceivedOn = receivedOn;
                            requestLog.Validate = sw.Elapsed.TotalSeconds;
                            await pnsApiSvc.UpdatePackageStatus(requestLog);

                            if (mailPiece.DocumentOrPackage != null) mailPiece.DocumentOrPackage.IncludePatternBaseString = false;
                            var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_ClientCode, ErrorMessage = msg }, receivedOn);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                        }

                        client = await clientService.GetClientByIdAsync(clientId.Value);
                    }
                    else
                        client = await clientService.GetClientByAppCode(mailPiece.ClientCode);

                    if (client == null)
                    {
                        string msg = "Invalid client code specified.";

                        await StoreInvalidOrBadRequest(manifest[0], provider.FileData, errorPath, mailPiece.Id, msg, addPackageResult.Id);

                        DeleteFiles(downloadedFiles);

                        sw.Stop();
                        requestLog.Id = addPackageResult.Id;
                        requestLog.MailPieceId = mailPiece.Id;
                        requestLog.Status = OutComeCodes.Invalid_ClientCode;
                        requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                        requestLog.APIResponse = msg;
                        requestLog.ReceivedOn = receivedOn;
                        requestLog.Validate = sw.Elapsed.TotalSeconds;
                        await pnsApiSvc.UpdatePackageStatus(requestLog);

                        if (mailPiece.DocumentOrPackage != null) mailPiece.DocumentOrPackage.IncludePatternBaseString = false;
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_ClientCode, ErrorMessage = msg }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                    mailBatch.ClientCode = client.AppCode;
                    mailBatch.ClientBatchId = mailBatch.Id.ToString();

                }
                // #ChannelCode
                switch (sftChannel.GetChannelCode().ToUpper())
                {
                    default:
                        var result = ValidateClientSpecificRules(mailBatch, client);
                        if (result.Code != OutComeCodes.No_Error)
                        {
                            await StoreInvalidOrBadRequest(manifest[0], provider.FileData, errorPath, mailPiece.Id, result.ErrorMessage, addPackageResult.Id);
                            DeleteFiles(downloadedFiles);

                            sw.Stop();
                            requestLog.Id = addPackageResult.Id;
                            requestLog.MailPieceId = mailPiece.Id;
                            requestLog.Status = result.Code;
                            requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                            requestLog.APIResponse = result.ErrorMessage;
                            requestLog.ReceivedOn = receivedOn;
                            requestLog.Validate = sw.Elapsed.TotalSeconds;
                            await pnsApiSvc.UpdatePackageStatus(requestLog);

                            if (mailPiece.DocumentOrPackage != null) mailPiece.DocumentOrPackage.IncludePatternBaseString = false;
                            var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = result.Code, ErrorMessage = result.ErrorMessage }, receivedOn);

                            return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                        }
                        break;
                }

                if (mailPiece.DocumentOrPackage.Files != null && mailPiece.DocumentOrPackage.Files.Count > 0)
                {
                    var files = mailPiece.DocumentOrPackage.Files
                   .Select(f =>
                   {
                       var fileData = provider.FileData.SingleOrDefault(fd => fd.Headers.ContentDisposition.Name.Replace("\"", "") == f);
                       return new KeyValuePair<string, MultipartFileData>(f, fileData);
                   })
                   .ToList();

                    var nullFilePathList = files.Where(f => f.Value == null).ToList();

                    // filepath null validation check
                    if (nullFilePathList.Count > 0)
                    {
                        var msg = "Pdf file not found for the following file key(s): {0}.".Fill(string.Join(",", nullFilePathList.Select(n => n.Key)));

                        await StoreInvalidOrBadRequest(manifest[0], provider.FileData, errorPath, mailPiece.Id, msg, addPackageResult.Id);

                        sw.Stop();
                        requestLog.Id = addPackageResult.Id;
                        requestLog.MailPieceId = mailPiece.Id;
                        requestLog.Status = OutComeCodes.Missing_PDF_File;
                        requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                        requestLog.APIResponse = msg;
                        requestLog.ReceivedOn = receivedOn;
                        requestLog.Validate = sw.Elapsed.TotalSeconds;
                        await pnsApiSvc.UpdatePackageStatus(requestLog);

                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = msg }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }

                    if (files.Count > 99)
                    {
                        var msg = "Up to 99 pdf files are supported per mailpiece.";
                        await StoreInvalidOrBadRequest(manifest[0], provider.FileData, errorPath, mailPiece.Id, msg, addPackageResult.Id);

                        sw.Stop();
                        requestLog.Id = addPackageResult.Id;
                        requestLog.MailPieceId = mailPiece.Id;
                        requestLog.Status = OutComeCodes.Manifest_Missing_Or_Invalid;
                        requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                        requestLog.APIResponse = msg;
                        requestLog.ReceivedOn = receivedOn;
                        requestLog.Validate = sw.Elapsed.TotalSeconds;
                        await pnsApiSvc.UpdatePackageStatus(requestLog);

                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = msg }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                    sw.Stop();
                    requestLog.Validate = sw.Elapsed.TotalSeconds;
                    sw.Restart();

                    string finalPdfPath = null;

                    if (mailPiece.CustomInfo == null) mailPiece.CustomInfo = new List<Metadata>();

                    string destPdf = Path.Combine(root, Path.GetRandomFileName());
                    destPdf = Path.ChangeExtension(destPdf, ".pdf");
                    int totalPdfPages = 0;
                    using (var ds = new FileStream(destPdf, FileMode.Create))
                    using (var doc = new Document())
                    using (var copy = new PdfSmartCopy(doc, ds))
                    {

                        // This is done to open and merge (Secured) PDFs which allow printing but not copying or merging
                        PdfReader.unethicalreading = true;
                        //Loop through each input file                        
                        foreach (var file in files)
                        {
                            try
                            {
                                #region decrypt pdf files
                                var decryptedFilePath = file.Value.LocalFileName;
                                if (client.IsEncrypted)
                                {
                                    try
                                    {
                                        decryptedFilePath = Path.Combine(root, string.Format("Literal_{0}.pdf", Path.GetFileNameWithoutExtension(file.Value.LocalFileName)));
                                        CryptoHelper cryptoHelper = new CryptoHelper(Config.Common_Environment);
                                        await cryptoHelper.DecryptFileAsync(file.Value.LocalFileName, Config.PGP_Passphrase, decryptedFilePath);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error(e.Message, e);

                                        var msg = $"Unable to decrypt file : {file.Key}.";
                                        await StoreInvalidOrBadRequest(manifest[0], provider.FileData, errorPath, mailPiece.Id, msg + Environment.NewLine + e.ToString(), addPackageResult.Id);

                                        sw.Stop();
                                        requestLog.Id = addPackageResult.Id;
                                        requestLog.MailPieceId = mailPiece.Id;
                                        requestLog.Status = OutComeCodes.Invalid_PDF_File;
                                        requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                                        requestLog.APIResponse = msg;
                                        requestLog.ErrorLog = e.ToString();
                                        requestLog.ReceivedOn = receivedOn;
                                        requestLog.Merge = sw.Elapsed.TotalSeconds;
                                        await pnsApiSvc.UpdatePackageStatus(requestLog);

                                        // if there are any exceptions while decrypting pdf, don't delete files and send bad reqeust with mailresponse object, it should contain file names
                                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = msg }, receivedOn);
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                                    }
                                }
                                #endregion

                                using (var reader = new PdfReader(decryptedFilePath))
                                {
                                    var pageCount = reader.NumberOfPages;
                                    mailPiece.CustomInfo.Add(new Metadata { Key = "AM:{0}".Fill(file.Key), Value = "FileName:{0} | PageCount: {1}".Fill(file.Value.Headers.ContentDisposition.FileName.Replace("\"", ""), pageCount) });
                                    totalPdfPages += pageCount;
                                    //open document
                                    if (!doc.IsOpen()) doc.Open();
                                    //Add the entire document
                                    copy.AddDocument(reader);
                                    reader.Close();
                                }
                                downloadedFiles.Add(decryptedFilePath);
                            }
                            catch (Exception e)
                            {
                                var msg = $"'{file.Key}' pdf file is either corrupt or has no pages.";
                                log.Error(e.Message, e);
                                await StoreInvalidOrBadRequest(manifest[0], provider.FileData, errorPath, mailPiece.Id, msg + Environment.NewLine + e.ToString(), addPackageResult.Id);

                                sw.Stop();
                                requestLog.Id = addPackageResult.Id;
                                requestLog.MailPieceId = mailPiece.Id;
                                requestLog.Status = OutComeCodes.Invalid_PDF_File;
                                requestLog.HTTPStatusCode = (int)HttpStatusCode.BadRequest;
                                requestLog.APIResponse = msg;
                                requestLog.ErrorLog = e.ToString();
                                requestLog.ReceivedOn = receivedOn;
                                requestLog.Merge = sw.Elapsed.TotalSeconds;
                                await pnsApiSvc.UpdatePackageStatus(requestLog);

                                // if there are any exceptions while counting pages or merging, don't delete files and send badreqeust with mailresponse object, it should contain file names
                                var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = msg }, receivedOn);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                            }
                        }
                        PdfReader.unethicalreading = false;
                        doc.Close();
                    }
                    finalPdfPath = destPdf;
                    mailPiece.DocumentOrPackage.Path = finalPdfPath;
                    sw.Stop();
                    requestLog.TotalPages = totalPdfPages;
                    requestLog.Merge = sw.Elapsed.TotalSeconds;
                    sw.Restart();
                }
                else
                {
                    sw.Stop();
                    requestLog.Validate = sw.Elapsed.TotalSeconds;
                    requestLog.Merge = 0;
                    sw.Restart();
                }
                // call method passing mailpiece

                mailPiece.DocumentOrPackage.Name = mailPiece.DocumentOrPackage.Name.IsNullOrWhiteSpace() ? "{0}.pdf".Fill(mailPiece.BusinessFriendlyIdValue.GetValidFileName()) : mailPiece.DocumentOrPackage.Name;

                var response = await CreateAMBatch(mailBatch, mailPiece, sftChannel, receivedOn);
                sw.Stop();
                requestLog.PostBatch = sw.Elapsed.TotalSeconds;
                sw.Restart();

                var responseContent = await response.Content.ReadAsStringAsync();
                var mailResponse = JsonConvert.DeserializeObject<MailAPIResponse>(responseContent);
                // if get success response then delete files. 
                if (response.IsSuccessStatusCode)
                {
                    DeleteFiles(downloadedFiles);
                    sw.Stop();
                    requestLog.Id = addPackageResult.Id;
                    requestLog.MailPieceId = mailPiece.Id;
                    requestLog.Status = HttpStatusCode.OK.ToString();
                    requestLog.HTTPStatusCode = (int)HttpStatusCode.OK;
                    requestLog.APIResponse = mailResponse.Message;
                    requestLog.ReceivedOn = receivedOn;
                    requestLog.Cleanup = sw.Elapsed.TotalSeconds;
                    await pnsApiSvc.UpdatePackageStatus(requestLog);
                    return Request.CreateResponse(HttpStatusCode.OK, mailResponse);
                }
                else
                {
                    await StoreInvalidOrBadRequest(manifest[0], provider.FileData, errorPath, mailPiece.Id, mailResponse.ErrorDesc, addPackageResult.Id);
                    DeleteFiles(downloadedFiles);
                    sw.Stop();
                    requestLog.Id = addPackageResult.Id;
                    requestLog.MailPieceId = mailPiece.Id;
                    requestLog.Status = response.StatusCode.ToString();
                    requestLog.HTTPStatusCode = (int)response.StatusCode;
                    requestLog.ReceivedOn = receivedOn;
                    requestLog.APIResponse = mailResponse.ErrorDesc;
                    requestLog.Cleanup = sw.Elapsed.TotalSeconds;
                    await pnsApiSvc.UpdatePackageStatus(requestLog);
                    return Request.CreateResponse(response.StatusCode, mailResponse);
                }

            }
            catch (Exception e)
            {
                var traceId = Guid.NewGuid();
                log.Error("TraceId: {0}, Message: {1}".Fill(traceId, e.Message), e);

                await StoreInvalidOrBadRequest(manifest.Length == 0 ? string.Empty : manifest[0], provider.FileData, errorPath, traceId, e.ToString());
                DeleteFiles(downloadedFiles);

                sw.Stop();
                requestLog.Id = addPackageResult.Id;
                requestLog.MailPieceId = traceId;
                requestLog.Status = HttpStatusCode.InternalServerError.ToString();
                requestLog.HTTPStatusCode = (int)HttpStatusCode.InternalServerError;
                requestLog.ReceivedOn = receivedOn;
                requestLog.ErrorLog = e.ToString();
                try
                {
                    await pnsApiSvc.UpdatePackageStatus(requestLog);
                }
                catch (InvalidOperationException io)
                {
                    log.Error("TraceId: {0}, Message: {1}".Fill(traceId, io.Message), io);
                }

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unexpected error. TraceId : {0}".Fill(traceId));
            }
        }

        private async Task StoreInvalidOrBadRequest(string manifest, Collection<MultipartFileData> fileData, string rejectedPath, Guid uniqueId, string error, int id = 0)
        {
            try
            {
                if (uniqueId == Guid.Empty)
                    uniqueId = Guid.NewGuid();

                //Store Error
                File.WriteAllText(Path.Combine(rejectedPath, $"{uniqueId}.txt"), error);

                string zipFilePath = Path.Combine(rejectedPath, $"{id}_{uniqueId}.zip");

                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        ZipArchiveEntry xmlEntry = archive.CreateEntry($"{uniqueId}.json");

                        using (StreamWriter writer = new StreamWriter(xmlEntry.Open()))
                        {
                            await writer.WriteAsync(manifest);
                        }

                        if (fileData.Count > 0)
                        {
                            var cnt = 1;
                            foreach (var file in fileData)
                            {
                                if (!File.Exists(file.LocalFileName))
                                {
                                    continue;
                                }
                                string fileName = $"{cnt++}_{file.Headers.ContentDisposition.Name}_{file.Headers.ContentDisposition.FileName}".Replace("\"", "");
                                ZipArchiveEntry pdfEntry = archive.CreateEntry(fileName);
                                using (var pdfStream = pdfEntry.Open())
                                using (var payloadStream = new FileStream(file.LocalFileName, FileMode.Open, FileAccess.Read))
                                {
                                    await payloadStream.CopyToAsync(pdfStream, 81920);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("TraceId: {0}, Message: {1}".Fill(uniqueId, e.Message), e);
            }
        }

        private static string ValidateManifestRequest(ref MailPiece mailPiece, Guid packageId, bool isCLPackageId, string[] manifest, List<string> filekeys)
        {
            string manifestErrDesc = null;
            if (manifest == null || manifest.Length == 0 || string.IsNullOrWhiteSpace(manifest[0]))
            {
                manifestErrDesc = "Manifest is mandatory.";
            }
            else if (manifest.Length > 1)
            {
                manifestErrDesc = "Multiple manifest entries.";
            }
            //  File keys must be unique
            else if (filekeys.Count != filekeys.Distinct().Count())
            {
                manifestErrDesc = "File key names must be unique.";
            }
            else
            {
                // try catch special
                try
                {
                    mailPiece = JsonConvert.DeserializeObject<MailPiece>(manifest[0]);
                    //mailPiece.Id = mailPiece.Id == Guid.Empty ? Guid.NewGuid() : mailPiece.Id;
                    if (isCLPackageId)
                    {
                        if (mailPiece.Id != Guid.Empty)
                            manifestErrDesc = "PackageId provided in header must match with manifest.";
                        else
                            mailPiece.Id = packageId;
                    }
                    else
                    {
                        if (mailPiece.Id != packageId)
                            manifestErrDesc = "PackageId provided in header must match with manifest.";
                    }
                }
                catch (JsonReaderException jrEx)
                {
                    manifestErrDesc = string.Format("Could not process manifest: Path '{0}', line {1}, position {2}.", jrEx.Path, jrEx.LineNumber, jrEx.LinePosition);
                }
                catch (JsonSerializationException jsEx)
                {
                    manifestErrDesc = string.Format("Could not process manifest: Path '{0}', line {1}, position {2}.", jsEx.Path, jsEx.LineNumber, jsEx.LinePosition);
                }
                catch (Exception)
                {
                    manifestErrDesc = string.Format("Unexpected error. Could not process manifest");
                }
            }
            return manifestErrDesc;
        }

        private static void DeleteFiles(List<string> files)
        {
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        [Route("mailresponse")]
        [HttpGet]
        public List<MailAPIResponse> MailAPIResponse(string mailPieceIds)
        {
            if (mailPieceIds == null) throw new HttpResponseException(HttpStatusCode.BadRequest);

            List<string> uniqueIds = mailPieceIds.Split(',').ToList();
            List<MailAPIResponse> mailAPIResponses = new List<MailAPIResponse>();

            foreach (var mailPieceId in uniqueIds)
            {
                if (Guid.TryParse(mailPieceId, out var mpId))
                {
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        //string deliveryReport = generalSvc.GeneralJsonReport(mailPieceId);
                        var response = generalSvc.GenerateMailAPIResponse(mailPieceId);
                        //mailAPIResponses.Add(JsonConvert.DeserializeObject<MailAPIResponse>(deliveryReport));
                        mailAPIResponses.Add(response);
                    }
                }
                else
                {
                    string msg = $"MailPieceId must be a valid GUID: {mailPieceId}";
                    mailAPIResponses.Add(new MailAPIResponse { Id = Guid.Empty, ClientCode = null, BusinessFriendlyIdName = null, BusinessFriendlyIdValue = null, DocumentOrPackageType = null, DocumentOrPackage = null, RecipientInfo = null, MailingStatus = MailingStatus.Error.ToString(), ErrorCode = OutComeCodes.Invalid_MailPieceId, ErrorDesc = msg });
                }
            }
            return mailAPIResponses;
        }

        private MailAPIResponse GetErrorResponse(MailPiece mailPiece, MailPieceValidationResult validationResult, DateTime receivedOn)
        {
            return new MailAPIResponse { Id = mailPiece.Id, ClientCode = mailPiece.ClientCode, BusinessFriendlyIdName = mailPiece.BusinessFriendlyIdName, BusinessFriendlyIdValue = mailPiece.BusinessFriendlyIdValue, DocumentOrPackageType = mailPiece.DocumentOrPackageType, DocumentOrPackage = mailPiece.DocumentOrPackage, RecipientInfo = mailPiece.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = validationResult.Code, ErrorDesc = validationResult.ErrorMessage, ReceivedOn = receivedOn };
        }

        private async static Task InsertInvalidBatch(DateTime receivedTime, SFTChannel sFTChannel, MailBatch mailBatch, int clientId, string errorMessage)
        {
            using (IMailBatchService mailBatchService = new MailBatchService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceService = new MailPieceService())
            {
                int mailBatchId = await mailBatchService.InsertBatchDetails(mailBatch, clientId, sFTChannel);
                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Received, receivedTime);
                await mailBatchService.MarkMailBatchAsInvalid(mailBatchId, errorMessage);
                await commonSvc.UpdateBatchStatus(mailBatchId, BatchStatus.Failed.ToString());
                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportPrepped, Common.Now);
                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
            }
        }

        private IEnumerable<MailPieceValidationResult> GetAllMailPieceBasicErrors(MailBatch mailBatch, Client client)
        {
            var mailPiece = mailBatch.MailPieces.First();

            if (mailPiece.RecipientInfo != null)
            {
                if (mailPiece.RecipientInfo.Address != null)
                {
                    if (mailPiece.RecipientInfo.Address.Street.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.City.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.State.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.Zip.IsNullOrWhiteSpace())
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address is mandatory." };

                    if (!mailPiece.RecipientInfo.Address.Zip.IsNullOrWhiteSpace() && (mailPiece.RecipientInfo.Address.Country.IsNullOrWhiteSpace() || (!mailPiece.RecipientInfo.Address.Country.IsNullOrWhiteSpace() && (mailPiece.RecipientInfo.Address.Country == "USA" || mailPiece.RecipientInfo.Address.Country == "US" || mailPiece.RecipientInfo.Address.Country == "The United States of America" || mailPiece.RecipientInfo.Address.Country == "United States of America" || mailPiece.RecipientInfo.Address.Country == "United States"))))
                    {
                        if (!Regex.Match(mailPiece.RecipientInfo.Address.Zip, @"^(?(^00000(|-0000))|\d{5}(?:|[-\s]+\d{4}))$").Success)
                        {
                            yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient zipcode is in invalid format." };
                        }
                    }
                }
                else
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient Address is mandatory." };
                }

                if (mailPiece.RecipientInfo.Recipient.FirstName.IsNullOrWhiteSpace() && mailPiece.RecipientInfo.Recipient.LastName.IsNullOrWhiteSpace() && mailPiece.RecipientInfo.Recipient.Organization.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Either Organization name or Recipient first name or Recipient last name is mandatory." };
            }

            if (mailPiece.ReturnInfo != null)
            {
                if (mailPiece.ReturnInfo.Address != null)
                {
                    if (mailPiece.ReturnInfo.Address.Street.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.City.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.State.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.Zip.IsNullOrWhiteSpace())
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return recipient address is mandatory." };

                    if (!mailPiece.ReturnInfo.Address.Zip.IsNullOrWhiteSpace() && (mailPiece.ReturnInfo.Address.Country.IsNullOrWhiteSpace() || (!mailPiece.ReturnInfo.Address.Country.IsNullOrWhiteSpace() && (mailPiece.ReturnInfo.Address.Country == "USA" || mailPiece.ReturnInfo.Address.Country == "US" || mailPiece.ReturnInfo.Address.Country == "The United States of America" || mailPiece.ReturnInfo.Address.Country == "United States of America" || mailPiece.ReturnInfo.Address.Country == "United States"))))
                    {
                        if (!Regex.Match(mailPiece.ReturnInfo.Address.Zip, @"^(?(^00000(|-0000))|\d{5}(?:|[-\s]+\d{4}))$").Success)
                        {
                            yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return recipient zipcode is in invalid format." };
                        }
                    }
                }
                else
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return Address is mandatory." };
                }

                if (mailPiece.ReturnInfo.Recipient != null)
                {
                    if (mailPiece.ReturnInfo.Recipient.FirstName.IsNullOrWhiteSpace() && mailPiece.ReturnInfo.Recipient.Organization.IsNullOrWhiteSpace())
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Return recipient first name or organization is mandatory." };
                }
                else
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Return recipient is mandatory." };
                }

            }

            if (mailPiece.DocumentOrPackageType.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = "Document or package type is not specified." };

            if (mailPiece.eDelivery != null)
            {
                //TODO : Email address validation if not empty.
                if (mailPiece.RecipientInfo.Recipient.EmailAddress.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Email address is mandatory." };
                else
                {
                    if (!IsValidEmail(mailPiece.RecipientInfo.Recipient.EmailAddress))
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Email address is not in a valid format." };
                    }
                }
                if (mailPiece.eDelivery.ExpiryDate < Common.EstToday)
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Issue_In_EDelivery, ErrorMessage = "The expiration date must be greater than or equal to current date." };
            }

            if (mailPiece.BusinessFriendlyIdName.IsNullOrWhiteSpace() || mailPiece.BusinessFriendlyIdValue.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_BusinessIdentifier, ErrorMessage = "Business friendly Id or value is not specified in package." };

            bool allowInsertsOnly;
            using (IDocTypeService docSvc = new DocTypeService())
            {
                allowInsertsOnly = docSvc.AllowInsertsOnlyDocumentType(client.Id, mailPiece.DocumentOrPackageType);
            }

            // TODO : check DocumentOrPackage is not empty and DocumentOrPackage.Files property is not empty
            if (mailPiece.DocumentOrPackage == null)
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_DocumentOrPackage, ErrorMessage = "DocumentOrPackage is mandatory." };
            else if ((mailPiece.DocumentOrPackage.Files == null || mailPiece.DocumentOrPackage.Files.Count == 0) && mailPiece.DocumentOrPackage.Base64String.IsNullOrWhiteSpace() && !allowInsertsOnly && !mailPiece.DocumentOrPackageType.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Files property must be populated for non-inserts only packages." };
            else if ((mailPiece.DocumentOrPackage.Files == null || mailPiece.DocumentOrPackage.Files.Count == 0) && mailPiece.DocumentOrPackage.Base64String.IsNullOrWhiteSpace() && mailPiece.DocumentOrPackage.Inserts.IsNullOrWhiteSpace())
            {
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Request cannot proceed further without Files or Inserts value." };
            }
            else if (!mailPiece.DocumentOrPackage.Inserts.IsNullOrWhiteSpace())
            {
                if (mailPiece.DocumentOrPackage.Inserts.Contains("|"))
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Inserts  is not in a valid format." };
                else
                {
                    var insertLst = mailPiece.DocumentOrPackage.Inserts.ToUpper().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    if ((mailPiece.DocumentOrPackage.Files == null || mailPiece.DocumentOrPackage.Files.Count == 0) && insertLst.Count == 0)
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Request cannot proceed further without Files or Inserts value." };
                    }
                    else if (insertLst.Count != insertLst.Distinct().Count())
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Multiple occurrences of an insert value are not allowed." };
                    }
                    else
                    {
                        // check if insert is BRE                            
                        using (IMailPieceService mailPieceService = new MailPieceService())
                        {
                            var invalidInserts = mailPieceService.GetInvalidInserts(insertLst);
                            if (invalidInserts.Count > 0)
                            {
                                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = $"Following specified inserts are invalid or not BRE: {string.Join(",", invalidInserts)}" };
                            }
                        }
                    }
                }
            }
            if (!mailPiece.RecipientInfo.Address.Country.IsCountryUS())
            {
                using (IDocTypeService docSvc = new DocTypeService())
                {
                    var docType = docSvc.GetDocTypeByDocTypeName(client.Id, mailPiece.DocumentOrPackageType);
                    if (docType?.IntlPnSConfigId == null || docType?.IntlPnSConfigId <= 0)
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = $"Either the specified document or package type '{mailPiece.DocumentOrPackageType}' is not configured for international mailing or no matching print configuration found." };
                    }
                }
            }
            if (mailPiece.CustomInfo != null)
            {
                if (mailPiece.CustomInfo.Count == 0)
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = "Metadata is mandatory in custominfo." };
                }

                foreach (var customInfo in mailPiece.CustomInfo)
                {
                    if (customInfo.Key.IsNullOrWhiteSpace())
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = "CustomInfo's Key is mandatory." };
                    }
                }
            }

            yield break;
        }

        private MailPieceValidationResult ValidateClientSpecificRules(MailBatch mailBatch, Client client)
        {
            var errors = GetAllMailPieceBasicErrors(mailBatch, client).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }
            else
            {
                // Schema validation
                var scehmaErrors = GetAllSchemaErrors(mailBatch, client).ToList();

                if (scehmaErrors.Count > 0)
                {
                    return new MailPieceValidationResult() { Code = scehmaErrors.First().Code, ErrorMessage = string.Join(" | ", scehmaErrors.Select(row => row.ErrorMessage)) };
                }
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private IEnumerable<MailPieceValidationResult> GetAllSchemaErrors(MailBatch mailBatch, Client client)
        {
            //TODO : Take Phone and USZIP validation with others. After validating all checks, call ValidateXml
            var xmlValidationResult = XmlUtil.ValidateXml(schemas, XmlUtil.Serialize<MailBatch>(mailBatch));

            foreach (var error in xmlValidationResult.ErrorList)
            {
                if (error.Contains("http://schemas.corelogic.com/ade/authomail/") && error.Contains("NAPhone"))
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Provided phone number is not in a valid format. Please provide it in one of the following formats : (XXX) XXX XXXX, XXXXXXXXXX, XXX-XXX-XXXX, XXX XXX XXXX" };
                }
                else if (error.Contains("http://schemas.corelogic.com/ade/authomail/") && error.Contains("USZIP"))
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Provided zip code is not in a valid format." };
                }
                else if (error.Contains("http://schemas.corelogic.com/ade/authomail/") && error.Contains("EmailAddress"))
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Email address is not in a valid format or blank." };
                }
                else
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = error };
                }
            }
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        [HttpPost, Route("apimailrequest")]
        public async Task<object> CreateAPIMailRequestAsync(MailPiece mailPiece, List<string> downloadedFiles)
        {
            SPELogInfo logInfo = new SPELogInfo();
            logInfo.ClientId = User.Identity.GetClientId().Value;
            logInfo.CreatedBy = new Guid(User.Identity.GetUserId());
            logInfo.CreatedOn = Common.Now;

            if (mailPiece == null) throw new HttpResponseException(HttpStatusCode.BadRequest);
            DateTime receivedTime = Common.Now;
            SFTChannel sFTChannel;
            Client client;
            MailBatch mailBatch = new MailBatch()
            {
                Id = Guid.NewGuid()
            };

            MailPiece mp = mailPiece;
            mp.DocumentOrPackage = new DocumentOrPackage();

            using (ICommonService commonSvc = new CommonService())
            {
                if (mp.Id != Guid.Empty && commonSvc.CheckForDuplicateMailPieceUniqueId(mailPiece.Id))
                {
                    var result = new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = OutComeCodes.Invalid_DuplicateKey, ErrorDesc = string.Format("The specified package id '{0}' conflicts with an existing package. Retry with a new GUID.", mailPiece.Id), ReceivedOn = receivedTime };

                    return new { Key = HttpStatusCode.Conflict, Value = result };
                }
            }

            mp.Id = Guid.NewGuid();

            logInfo.Id = mp.Id;

            mailBatch.MailPieces.Add(mp);

            using (IClientService clientService = new ClientService())
            {
                var clientIdOfUser = User.Identity.GetClientId();

                if (clientIdOfUser == null)
                {
                    var errorMsg = "Client is not assigned to the user. Please reach out to your manager to get it configured.";

                    logInfo.ErrorDesc = errorMsg;
                    logInfo.Status = ShippingStatus.Error.ToString();
                    mailPieceSvc.InsertSPELogInfo(logInfo);

                    var resultError = new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = OutComeCodes.Invalid_ClientCode, ErrorDesc = errorMsg, ReceivedOn = receivedTime };
                    return new { Key = HttpStatusCode.BadRequest, Value = resultError };
                }

                var clientSFTMaps = clientService.GetClientSFTChannelMapDetails(clientIdOfUser.Value);

                if (!clientSFTMaps.Any(s => s.SFTChannelId == Config.Web_SubmissionSFTChannelId))
                {
                    var errorMsg = "You do have rights to use this channel for creating print and ship package. Please reach out to your manager to get an access.";

                    logInfo.ErrorDesc = errorMsg;
                    logInfo.Status = ShippingStatus.Error.ToString();
                    mailPieceSvc.InsertSPELogInfo(logInfo);

                    var result = new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = "InvalidChannelCode", ErrorDesc = errorMsg, ReceivedOn = receivedTime };
                    return new { Key = HttpStatusCode.BadRequest, Value = result };
                }

                sFTChannel = await clientService.GetSFTChannelDetailsBySFTChannelId(clientSFTMaps.Single(s => s.SFTChannelId == Config.Web_SubmissionSFTChannelId).SFTChannelId);
            }

            if (sFTChannel == null)
            {
                var errorMsg = "You do have rights to use this channel for creating print and ship package. Please reach out to your manager to get an access.";

                logInfo.ErrorDesc = errorMsg;
                logInfo.Status = ShippingStatus.Error.ToString();
                mailPieceSvc.InsertSPELogInfo(logInfo);

                var result = new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = "InvalidChannelCode", ErrorDesc = "You do have rights to use this channel for creating print and ship package. Please reach out to your manager to get an access.", ReceivedOn = receivedTime };
                return new { Key = HttpStatusCode.BadRequest, Value = result };
            }

            try
            {
                using (IClientService clientService = new ClientService())
                {
                    client = await clientService.GetClientByAppCode(mp.ClientCode);
                    if (client == null)
                    {
                        //Return error response and insert entry of invalid_client

                        var errorMsg = "Invalid client code specified.";

                        logInfo.ErrorDesc = errorMsg;
                        logInfo.Status = ShippingStatus.Error.ToString();
                        mailPieceSvc.InsertSPELogInfo(logInfo);

                        // sagar's common method call                        
                        var invalidClient = await clientService.GetClientByAppCode(sFTChannel.Code.Split('_')[0] + "_Invalid");
                        await InsertInvalidBatch(receivedTime, sFTChannel, mailBatch, invalidClient.Id, errorMsg);
                        mp.DocumentOrPackage.IncludePatternBaseString = false;
                        var resultError = new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = "InvalidClientCode", ErrorDesc = "Invalid client code specified.", ReceivedOn = receivedTime };
                        return new { Key = HttpStatusCode.BadRequest, Value = resultError };
                    }
                    else
                    {
                        mailBatch.ClientCode = client.AppCode;
                        mailBatch.ClientBatchId = mailBatch.Id.ToString();
                    }
                }

                string destPdf = System.IO.Path.Combine(Config.MailRequestAPI_FilesDownloadPath, "TempWebFiles", System.IO.Path.GetRandomFileName());
                destPdf = System.IO.Path.ChangeExtension(destPdf, ".pdf");
                using (var ds = new FileStream(destPdf, FileMode.Create))
                using (var doc = new Document())
                using (var copy = new PdfSmartCopy(doc, ds))
                {
                    // This is done to open and merge (Secured) PDFs which allow printing but not copying or merging
                    PdfReader.unethicalreading = true;
                    //Loop through each input file                        
                    foreach (var file in downloadedFiles)
                    {
                        try
                        {
                            using (var reader = new PdfReader(file))
                            {
                                //open document
                                if (!doc.IsOpen()) doc.Open();
                                //Add the entire document
                                copy.AddDocument(reader);
                                reader.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMsg = $"'{System.IO.Path.GetFileName(file)}' pdf file is either corrupt or invalid.";

                            logInfo.ErrorDesc = errorMsg;
                            logInfo.Status = ShippingStatus.Error.ToString();
                            mailPieceSvc.InsertSPELogInfo(logInfo);

                            var errresult = new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = OutComeCodes.Invalid_PDF_File, ErrorDesc = errorMsg, ReceivedOn = receivedTime };
                            log.Error($"Error occured while reading pdf file {ex}");
                            return new { Key = HttpStatusCode.BadRequest, Value = errresult };
                        }
                    }
                    PdfReader.unethicalreading = false;
                    doc.Close();
                }
                string finalPdfPath = destPdf;
                mailPiece.DocumentOrPackage.Path = finalPdfPath;
                mailPiece.DocumentOrPackage.Name = downloadedFiles.Count == 1 ? Path.GetFileName(downloadedFiles[0]) : $"{mp.Id}.pdf";
                mp.DocumentOrPackage.Base64String = Convert.ToBase64String(File.ReadAllBytes(mailPiece.DocumentOrPackage.Path));

                var result = ValidateClientSpecificRules(mailBatch, client);

                if (result.Code == OutComeCodes.No_Error)
                {
                    mp.DocumentOrPackage.IncludePatternBaseString = true;
                    //create zip and response
                    string zipFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.zip", mailBatch.Id));
                    string triggerFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.GO", mailBatch.Id));
                    string xmlFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.xml", mailBatch.Id));
                    string pdfFilePath = Path.Combine(sFTChannel.Host + Path.Combine(sFTChannel.ToADEDir, string.Format("{0}.pdf", mp.DocumentOrPackage.Name == null ? mp.Id.ToString() : Path.GetFileNameWithoutExtension(mp.DocumentOrPackage.Name))));

                    var isDuplicate = mailPieceSvc.IsDuplicateRecord(mailPiece);

                    if (isDuplicate && client.IsRejectDupRecord)
                    {
                        var errorMsg = "System already contains same package information so you cannot submit a duplicate request.";

                        logInfo.ErrorDesc = errorMsg;
                        logInfo.Status = ShippingStatus.Error.ToString();
                        mailPieceSvc.InsertSPELogInfo(logInfo);

                        var errresult = new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = OutComeCodes.NotAcceptable, ErrorDesc = errorMsg, ReceivedOn = receivedTime };

                        return new { Key = HttpStatusCode.BadRequest, Value = errresult };
                    }

                    List<string> filesToZip = new List<string>();

                    string xml = mailBatch.Serialize<MailBatch>();

                    using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.OpenOrCreate))
                    {
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                        {
                            ZipArchiveEntry xmlEntry = archive.CreateEntry(string.Format("{0}.xml", mailBatch.Id));
                            using (StreamWriter writer = new StreamWriter(xmlEntry.Open()))
                            {
                                await writer.WriteAsync(xml);
                            }
                            ZipArchiveEntry pdfEntry = archive.CreateEntry(string.Format("{0}.pdf", mp.DocumentOrPackage.Name == null ? mp.Id.ToString() : Path.GetFileNameWithoutExtension(mp.DocumentOrPackage.Name)));
                            using (var pdfStream = pdfEntry.Open())
                            {
                                var payloadStream = new MemoryStream(Convert.FromBase64String(mp.DocumentOrPackage.Base64String));
                                await payloadStream.CopyToAsync(pdfStream);
                            }
                        }
                    }

                    if (!File.Exists(triggerFilePath))
                    {
                        using (FileStream sourceStream = new FileStream(triggerFilePath,
FileMode.Create, FileAccess.Write, FileShare.None,
bufferSize: 4096, useAsync: true))
                        {

                        }

                        File.SetLastWriteTime(triggerFilePath, receivedTime);
                    }

                    //Insert SPE Log Info with Success Record
                    logInfo.ErrorDesc = null;
                    logInfo.Status = ShippingStatus.InProgress.ToString();
                    mailPieceSvc.InsertSPELogInfo(logInfo);

                    var res = new MailAPIResponse
                    {
                        Id = mp.Id,
                        MailingStatus = ShippingStatus.Acknowledgement.ToString(),
                        Message = $"Your request is queued for processing with unique Mail Piece Id : {mp.Id}"
                    };
                    return new { Key = HttpStatusCode.OK, Value = res };
                }
                else
                {

                    var errorMsg = result.ErrorMessage;

                    logInfo.ErrorDesc = errorMsg;
                    logInfo.Status = ShippingStatus.Error.ToString();
                    mailPieceSvc.InsertSPELogInfo(logInfo);

                    //insert error and response
                    using (IMailBatchService mailBatchService = new MailBatchService())
                    using (ICommonService commonSvc = new CommonService())
                    using (IDocTypeService docTypeSvc = new DocTypeService())
                    using (IMailPieceService mailPieceService = new MailPieceService())
                    {
                        mp.EffectiveReceivedOnEst = commonSvc.GetEffectiveEstReceivedOn(client.Id, docTypeSvc.GetDocTypeWithMaximumCutOffTimeByClientId(client.Id), receivedTime);
                        int mailBatchId = await mailBatchService.InsertBatchDetails(mailBatch, client.Id, sFTChannel);
                        // insert mailpiece
                        int mailPieceId = await mailPieceService.InsertBatchMailPieceDetails(mp, client.Id, mailBatchId, result.Code, result.ErrorMessage);
                        await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Received, receivedTime);
                        await mailBatchService.MarkMailBatchAsInvalid(mailBatchId, "Request does not contain valid package info.");
                        await commonSvc.UpdateBatchStatus(mailBatchId, BatchStatus.Failed.ToString());
                        await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportPrepped, Common.Now);
                        await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
                    }
                    mp.DocumentOrPackage.IncludePatternBaseString = false;

                    var res = new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = result.Code, ErrorDesc = result.ErrorMessage, ReceivedOn = receivedTime };

                    return new { Key = HttpStatusCode.BadRequest, Value = res };
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Unexpected error occured while processing your request.");
                log.Error(msg, ex);
                var res = new MailAPIResponse { MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = OutComeCodes.Unknown_Error, ErrorDesc = msg, ReceivedOn = receivedTime };

                return new { Key = HttpStatusCode.BadRequest, Value = res };
            }
        }
        public string ToXML(MailBatch mailBatch)
        {
            using (var stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(mailBatch.GetType());
                serializer.Serialize(stringwriter, mailBatch);
                return stringwriter.ToString();
            }
        }

        public class MailPieceValidationResult
        {
            public string Code { get; set; }
            public string ErrorMessage { get; set; }
        }
    }

}
