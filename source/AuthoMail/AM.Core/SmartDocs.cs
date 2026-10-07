using AM.Core.USPSIMService;
using AM.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using AM.USPSLib;
using AM.Entities;
using ServiceType = AM.USPSLib.ServiceType;
using System.Security;

namespace AM.Core
{
    public class SmartDocs
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private BaseFont timesBaseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
        private BaseFont timesBold = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, false);
        private BaseFont timesItalic = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, false);
        private Font arial = FontFactory.GetFont("Arial", 12.5f);
        private Font arialBold = FontFactory.GetFont("Arial", 12f, Font.BOLD);

        public SmartDocResponse GenerateUSPSIMBCoverLetter(string appCode, string coverTemplatePDFPath, string outputDir, Dictionary<string, string> mailPieceData, bool isReturnLabel = false)
        {
            appCode = Config.Common_Environment == Env.Prod ? appCode : "TEST";

            SmartDocResponse response = new SmartDocResponse();

            string labelFilePath = string.Empty;

            if (isReturnLabel)
            {
                labelFilePath = Path.Combine(outputDir, string.Format("ReturnLabel_{0}.pdf", mailPieceData["MailPieceId"]));
            }
            else
            {
                labelFilePath = Path.Combine(outputDir, string.Format("CoverLetter_{0}.pdf", mailPieceData["MailPieceId"]));
            }

            #region Generate Cover Letter

            if (!mailPieceData["AddressType"].Contains("FOREIGN"))
            {
                USPSIMClient client = new USPSIMClient();
                IMBarcodeInfo IMBarcode = client.GenerateIMBarcode(appCode, mailPieceData["MailPieceId"], mailPieceData["MailPieceZip"]);

                if (IMBarcode.Status == true)
                {
                    mailPieceData.Add("Barcode", IMBarcode.BarcodeString);
                    response.TrackingCode = IMBarcode.TrackingCode;
                    mailPieceData.Add("FrontBarcode", "*" + IMBarcode.TrackingCode + "*");
                }
                else
                {
                    response.HasError = true;
                    response.ExceptionDetail = "Error occured while generating IMBarcode :" + IMBarcode.ErrorMessage;
                    return response;
                }
            }
            else
            {
                mailPieceData.Add("FrontBarcode", "*" + mailPieceData["MailPieceId"] + "*");
            }

            try
            {
                PdfUtil.PdfFormFiller(coverTemplatePDFPath, labelFilePath, mailPieceData);

                if (!File.Exists(labelFilePath))
                {
                    response.HasError = true;
                    response.ExceptionDetail = "Unable to generate cover letter.";
                    return response;
                }

                response.CoverLetterPath = labelFilePath;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating cover letter for MailPiece-Id : {0}".Fill(mailPieceData["MailPieceId"]);
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                return response;
            }
            return response;

            #endregion
        }

        public SmartDocResponse GenerateUSPSIMBCoverLetterDocx(string appCode, string coverTemplatePDFPath, string outputDir, dynamic coversheetData)
        {
            appCode = Config.Common_Environment == Env.Prod ? appCode : "TEST";

            SmartDocResponse response = new SmartDocResponse();

            string coverLetterPDFPath = Path.Combine(outputDir, string.Format("CoverLetter_{0}.pdf", coversheetData.MailPieceId));

            #region Generate Cover Letter
            if (coversheetData.IsForeignAddress == 0)
            {
                USPSIMClient client = new USPSIMClient();
                IMBarcodeInfo IMBarcode = client.GenerateIMBarcode(appCode, coversheetData.MailPieceId.ToString(), coversheetData.ToZip);

                if (IMBarcode.Status == true)
                {
                    coversheetData.USPSIMB = IMBarcode.BarcodeString;
                    response.TrackingCode = IMBarcode.TrackingCode;
                }
                else
                {
                    response.HasError = true;
                    response.ExceptionDetail = "Error occured while generating IMBarcode :" + IMBarcode.ErrorMessage;
                    return response;
                }
            }

            try
            {
                string base64Path = Path.Combine(Config.Core_Base64Folder);
                PdfUtil.GenerateCoverSheet(Config.Core_CoversheetAPI, coverTemplatePDFPath, base64Path, coverLetterPDFPath, coversheetData);

                response.CoverLetterPath = coverLetterPDFPath;
            }
            catch (Exception ex)
            {
                var msg = $"Error occured while generating cover letter for MailPiece-Id : {coversheetData.MailPieceId}";
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                return response;
            }
            return response;

            #endregion
        }

        public SmartDocResponse GenerateCoverLetterDocx(string appCode, string coverTemplatePDFPath, string outputDir, dynamic coversheetData)
        {
            appCode = Config.Common_Environment == Env.Prod ? appCode : "TEST";

            SmartDocResponse response = new SmartDocResponse();

            string coverLetterPDFPath = Path.Combine(outputDir, string.Format("CoverLetter_{0}.pdf", coversheetData.MailPieceId));

            #region Generate Cover Letter
            try
            {
                string base64Path = Path.Combine(Config.Core_Base64Folder);
                PdfUtil.GenerateCoverSheet(Config.Core_CoversheetAPI, coverTemplatePDFPath, base64Path, coverLetterPDFPath, coversheetData);

                response.CoverLetterPath = coverLetterPDFPath;
            }
            catch (Exception ex)
            {
                var msg = $"Error occured while generating cover letter for MailPiece-Id : {coversheetData.MailPieceId}";
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                return response;
            }
            return response;

            #endregion
        }

        public SmartDocResponse GenerateeVSPriorityExpressCoverLetter(string sysUserId, string outputDir, string mailPieceId, eVSLabelRequest eVSReq, string envelopeName, USPSCredentialInfo uspsUserId, bool isReturnLabel = false)
        {
            SmartDocResponse response = new SmartDocResponse();

            response.ShippingServiceType = eVSReq.ServiceType.Value;

            string labelFilePath = string.Empty;

            if (isReturnLabel)
            {
                labelFilePath = Path.Combine(outputDir, string.Format("eVSReturnLabel_{0}.pdf", mailPieceId));
            }
            else
            {
                labelFilePath = Path.Combine(outputDir, string.Format("eVSCoverLetter_{0}.pdf", mailPieceId));
            }

            #region Generate Cover Letter

            try
            {
                eVSLabelResponse eVSRes;

                using (IUSPSService uspsSvc = new USPSService())
                {
                    eVSRes = uspsSvc.RequesteVSLabelAsync(sysUserId, eVSReq, uspsUserId).Result;

                    if (eVSRes.HasError)
                    {
                        log.Error("Error occured in MailPieceId - {0} while requesting eVS priority express label with following Error {1}".Fill(mailPieceId, eVSRes.Error.Description));

                        response.HasError = true;
                        response.ExceptionDetail = "Error occured while generating eVS label for MailPiece-Id:{0} with Exception : {1}".Fill(mailPieceId, eVSRes.Error.Description);
                        response.ExceptionCode = OutComeCodes.Unknown_Error;
                        return response;
                    }

                    File.WriteAllBytes(labelFilePath, Convert.FromBase64String(eVSRes.LabelImageString));

                    //Here system is creating new PDF for military addresses only label's size LABEL4X6L to Letter size. 
                    if (!Config.Common_LandscapeUSPSLabels.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(e => e.ToUpper() == envelopeName.ToUpper()))
                    {
                        ConvertLetterSizeLabel(labelFilePath, outputDir, mailPieceId);
                    }
                    response.CoverLetterPath = labelFilePath;
                }

                response.TrackingCode = eVSRes.TrackingNumber;
                response.PostageCost = eVSRes.Postage;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating cover letter for MailPiece-Id : {0} with Error : {1}".Fill(mailPieceId, ex.Message);
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                response.ExceptionCode = OutComeCodes.Unknown_Error;
                return response;
            }
            return response;

            #endregion
        }

        private void ConvertLetterSizeLabel(string coverLetterPDFPath, string outputDir, string mailPieceId)
        {
            string newCoverLetterPath = Path.Combine(outputDir, string.Format("eVSCoverLetter_{0}_Military.pdf", mailPieceId));
            using (PdfReader reader = new PdfReader(coverLetterPDFPath))
            using (Document document = new Document(PageSize.LETTER))
            using (PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(newCoverLetterPath, FileMode.Create)))
            {
                document.Open();
                PdfReader.unethicalreading = true;
                PdfContentByte cb = writer.DirectContent;
                for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                {
                    document.NewPage();
                    PdfImportedPage page = writer.GetImportedPage(reader, pageNumber);
                    int rotation = reader.GetPageRotation(pageNumber);
                    float offsetW = 0;
                    float offsetH = 0;
                    if (page.Width < document.PageSize.Width)
                    {
                        offsetW = (document.PageSize.Width - page.Width) / 2;
                        offsetH = (document.PageSize.Height - page.Height) / 2;
                    }
                    cb.AddTemplate(page, offsetW, offsetH, false);
                }

                document.Close();
            }

            //Here delete original cover letter and then create new cover page with old file name
            if (File.Exists(coverLetterPDFPath))
                File.Delete(coverLetterPDFPath);

            File.Move(newCoverLetterPath, coverLetterPDFPath);
        }

        public SmartDocResponse GenerateeVSPriorityCoverLetter(string sysUserId, string outputDir, string mailPieceId, eVSLabelRequest eVSReq, string envelopeName, USPSCredentialInfo uspsUserId, bool isReturnLabel = false)
        {
            SmartDocResponse response = new SmartDocResponse();

            response.ShippingServiceType = eVSReq.ServiceType.Value;

            string labelFilePath = string.Empty;

            if (isReturnLabel)
            {
                labelFilePath = Path.Combine(outputDir, string.Format("eVSReturnLabel_{0}.pdf", mailPieceId));
            }
            else
            {
                labelFilePath = Path.Combine(outputDir, string.Format("eVSCoverLetter_{0}.pdf", mailPieceId));
            }

            #region Generate Cover Letter

            try
            {
                eVSLabelResponse eVSRes;

                using (IUSPSService uspsSvc = new USPSService())
                {
                    eVSRes = uspsSvc.RequesteVSLabelAsync(sysUserId, eVSReq, uspsUserId).Result;

                    if (eVSRes.HasError)
                    {
                        response.HasError = true;
                        response.ExceptionDetail = "Error occured while generating eVS Priority label for MailPiece-Id:{0} with Exception : {1}".Fill(mailPieceId, eVSRes.Error.Description);
                        response.ExceptionCode = OutComeCodes.Unknown_Error;
                        return response;
                    }

                    File.WriteAllBytes(labelFilePath, Convert.FromBase64String(eVSRes.LabelImageString));

                    //Here system is creating new PDF for military addresses only label's size LABEL4X6L to Letter size. 
                    if (!Config.Common_LandscapeUSPSLabels.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(e => e.ToUpper() == envelopeName.ToUpper()))
                    {
                        ConvertLetterSizeLabel(labelFilePath, outputDir, mailPieceId);
                    }

                    response.CoverLetterPath = labelFilePath;
                }

                response.TrackingCode = eVSRes.TrackingNumber;
                response.PostageCost = eVSRes.Postage;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating cover letter for MailPiece-Id : {0} with Error : {1}".Fill(mailPieceId, ex.Message);
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                response.ExceptionCode = OutComeCodes.Unknown_Error;
                return response;
            }
            return response;

            #endregion
        }

        public SmartDocResponse GenerateDoc(List<string> coverLetterPDFPaths, string pdfAttachmentPath, string outputDocumentPath, float contentScalePercent, int noOfCopies, string printFormat, string returnLabelFilePath, List<string> endPagePDFPaths = null)
        {
            List<string> mergePdfCollection = new List<string>();
            SmartDocResponse response = new SmartDocResponse();

            string blankPdfPath = Path.Combine(Config.Core_CoverTemplateFolder, Config.Common_BlankPage);

            string outputDirPath = Path.GetDirectoryName(outputDocumentPath);
            string flattenPDFPath = Path.Combine(outputDirPath, "Flatten_" + Path.GetFileName(pdfAttachmentPath));
            string shrinkPDFPath = Path.Combine(outputDirPath, "Shrinked_" + Path.GetFileName(pdfAttachmentPath));

            string mergeReadyFilePath = flattenPDFPath;

            try
            {
                #region FlattenCode
                PdfUtil.FlattenPDF(pdfAttachmentPath, flattenPDFPath);
                #endregion

                #region Shrink Document
                if (contentScalePercent != 100f)
                {
                    PdfUtil.ShrinkAndCenterPDF(flattenPDFPath, shrinkPDFPath, contentScalePercent);
                    mergeReadyFilePath = shrinkPDFPath;
                }
                #endregion

                #region Merge Final Document

                if (coverLetterPDFPaths.Count > 0)
                {
                    mergePdfCollection.AddRange(coverLetterPDFPaths);

                    //Here if print format is Duplex than system is adding one blank page after coverletter / label.
                    if (printFormat == "Duplex")
                    {
                        mergePdfCollection.Add(blankPdfPath);
                    }

                    for (int i = 0; i < noOfCopies; i++)
                    {
                        mergePdfCollection.Add(mergeReadyFilePath);
                    }

                    //Here system will add return label before end page
                    if (!returnLabelFilePath.IsNullOrWhiteSpace())
                    {
                        mergePdfCollection.Add(returnLabelFilePath);
                    }

                    //Here system will add end page if any
                    if (endPagePDFPaths.Count > 0)
                    {
                        mergePdfCollection.AddRange(endPagePDFPaths);
                    }

                    mergePdfCollection.Add(outputDocumentPath);

                    PdfUtil.MergePDFs(mergePdfCollection.ToArray());
                }
                else
                {
                    for (int i = 0; i < noOfCopies; i++)
                    {
                        mergePdfCollection.Add(mergeReadyFilePath);
                    }

                    mergePdfCollection.Add(outputDocumentPath);

                    if (mergePdfCollection.Count == 2)
                    {
                        //Here we are just renaming file as a output document because we are not merging any cover letter for it
                        File.Move(mergePdfCollection[0], mergePdfCollection[1]);
                    }
                    else
                    {
                        PdfUtil.MergePDFs(mergePdfCollection.ToArray());
                    }
                }

                #endregion

                #region Remove Extra Document After Merging
                foreach (var coverLetterPDFPath in coverLetterPDFPaths)
                {
                    if (File.Exists(coverLetterPDFPath))
                    {
                        File.Delete(coverLetterPDFPath);
                    }
                }

                if (File.Exists(flattenPDFPath))
                {
                    File.Delete(flattenPDFPath);
                }

                if (File.Exists(shrinkPDFPath))
                {
                    File.Delete(shrinkPDFPath);
                }

                if (File.Exists(returnLabelFilePath))
                {
                    File.Delete(returnLabelFilePath);
                }

                foreach (var endPagePDFPath in endPagePDFPaths)
                {
                    if (File.Exists(endPagePDFPath))
                    {
                        File.Delete(endPagePDFPath);
                    }
                }
                #endregion

                response.TotalPages = PdfUtil.GetPageCount(outputDocumentPath);
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating final document '{0}.pdf'".Fill(Path.GetFileNameWithoutExtension(outputDocumentPath));

                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                response.ExceptionCode = OutComeCodes.Unknown_Error;
                return response;
            }

            return response;
        }

        public void WriteTextOnLabel(string inputPath, string text, bool isBottomRight = false)
        {
            var temp = Path.Combine(Path.GetDirectoryName(inputPath), Path.GetRandomFileName());
            using (var reader = new PdfReader(inputPath))
            {
                using (var fileStream = new FileStream(temp, FileMode.Create, FileAccess.Write))
                {
                    var document = new Document(reader.GetPageSizeWithRotation(1));
                    var writer = PdfWriter.GetInstance(document, fileStream);

                    document.Open();

                    document.NewPage();

                    var baseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    var importedPage = writer.GetImportedPage(reader, 1);

                    var contentByte = writer.DirectContent;

                    contentByte.BeginText();
                    contentByte.SetFontAndSize(baseFont, 8);

                    if (isBottomRight)
                    {
                        contentByte.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, text, importedPage.Width - (importedPage.Width - 400), importedPage.Height - (importedPage.Height - 80), 0);
                    }
                    else
                    {
                        contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, importedPage.Width - (importedPage.Width - 100), importedPage.Height - (importedPage.Height - 80), 0);
                    }

                    contentByte.EndText();
                    contentByte.AddTemplate(importedPage, 0, 0);

                    document.Close();
                    writer.Close();
                }
            }
            if (File.Exists(inputPath))
                File.Delete(inputPath);
            File.Move(temp, inputPath);
        }

        public SmartDocResponse GenerateLastPageWithBarcode(string endPageTemplatePath, string outputDocumentPath, Dictionary<string, string> mailPieceData)
        {
            SmartDocResponse response = new SmartDocResponse();

            string endPagePDFPath = Path.Combine(outputDocumentPath, string.Format("EndPage_{0}.pdf", mailPieceData["MailPieceId"]));
            try
            {
                PdfUtil.PdfFormFiller(endPageTemplatePath, endPagePDFPath, mailPieceData);
                response.CoverLetterPath = endPagePDFPath;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating end page document '{0}.pdf'".Fill(Path.GetFileNameWithoutExtension(outputDocumentPath));

                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                response.ExceptionCode = OutComeCodes.Unknown_Error;
                return response;
            }

            return response;
        }

        public SmartDocResponse GenerateFrontBarcodeCoverLetter(string coverTemplatePDFPath, string outputDir, Dictionary<string, string> mailPieceData)
        {
            SmartDocResponse response = new SmartDocResponse();

            string coverLetterPDFPath = Path.Combine(outputDir, string.Format("CoverLetter_{0}.pdf", mailPieceData["MailPieceId"]));

            #region Generate Cover Letter
            try
            {
                PdfUtil.PdfFormFiller(coverTemplatePDFPath, coverLetterPDFPath, mailPieceData);

                if (!File.Exists(coverLetterPDFPath))
                {
                    response.HasError = true;
                    response.ExceptionDetail = "Unable to generate cover letter.";
                    return response;
                }
                response.CoverLetterPath = coverLetterPDFPath;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating cover letter for MailPiece-Id : {0}".Fill(mailPieceData["MailPieceId"]);
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                response.ExceptionCode = OutComeCodes.Unknown_Error;
                return response;
            }
            return response;

            #endregion
        }

        public string StampingChecks(string pdfFilePath, string checkPageNumbers)
        {
            var newFilePath = Path.Combine(Path.GetDirectoryName(pdfFilePath), $"Stamped_{Path.GetFileName(pdfFilePath)}");

            using (FileStream Stream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write))
            using (PdfReader reader = new PdfReader(pdfFilePath))
            using (PdfStamper stamper = new PdfStamper(reader, Stream))
            {

                var pageNumberList = checkPageNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int iCount = 1; iCount <= stamper.Reader.NumberOfPages; iCount++)
                {
                    var pageSize = stamper.Reader.GetPageSizeWithRotation(iCount);

                    PdfContentByte PDFData = stamper.GetOverContent(iCount);
                    BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.WINANSI, BaseFont.EMBEDDED);
                    PDFData.BeginText();
                    PDFData.SetColorFill(BaseColor.WHITE);
                    PDFData.SetFontAndSize(baseFont, 6);
                    if (pageNumberList.Length > 0)
                    {
                        if (pageNumberList.Contains(iCount.ToString()))
                        {
                            PDFData.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, Config.Core_ChecksKey, pageSize.Width - 5, pageSize.Height - 7, 0);
                        }
                        else
                        {
                            PDFData.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, Config.Core_NonChecksKey, pageSize.Width - 5, pageSize.Height - 7, 0);
                        }
                    }
                    else
                    {
                        PDFData.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, Config.Core_NonChecksKey, pageSize.Width - 5, pageSize.Height - 7, 0);
                    }
                    PDFData.EndText();
                }
            }

            return newFilePath;
        }
    }

    public class SmartDocResponse
    {
        public string CoverLetterPath { get; set; }
        public string TrackingCode { get; set; }
        public ServiceType ShippingServiceType { get; set; }
        public decimal PostageCost { get; set; }
        public int TotalPages { get; set; }
        public bool HasError { get; set; }
        public string ExceptionDetail { get; set; }
        public string ExceptionCode { get; set; }
    }
}
