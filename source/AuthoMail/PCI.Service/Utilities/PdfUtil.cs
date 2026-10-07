using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    public static class PdfUtil
    {
        public static void MergePDFs(params string[] pdflist)
        {
            // Make sure we have enough arguments to work with
            if (pdflist.Length < 3)
            {
                throw new ArgumentException("There should be at least three arguments to use this method.");
            }

            // Make sure we find all input files
            for (int i = 0; i < pdflist.Length - 1; i++)
            {
                var pdf = pdflist[i];

                if (!File.Exists(pdf))
                {
                    throw new ArgumentException(String.Format("File '{0}' not found.", pdf));
                }
            }

            var dest = pdflist[pdflist.Length - 1];

            if (Path.GetExtension(dest).ToLower() != ".pdf")
            {
                throw new ArgumentException("A valid destination PDF name must be specified.");
            }

            var temp = Path.Combine(Path.GetDirectoryName(dest), Path.GetRandomFileName());

            using (var ds = new FileStream(temp, FileMode.Create))
            using (var doc = new Document())
            using (var copy = new PdfSmartCopy(doc, ds))
            {
                doc.Open();
                // This is done to open and merge (Secured) PDFs which allow printing but not copying or merging
                PdfReader.unethicalreading = true;
                //Loop through each input file
                for (int i = 0; i < pdflist.Length - 1; i++)
                {
                    var pdf = pdflist[i];

                    //Create a PdfReader
                    using (var reader = new PdfReader(pdf))
                    {
                        //Add the entire document
                        copy.AddDocument(reader);
                        reader.Close();
                    }
                }
                PdfReader.unethicalreading = false;
                doc.Close();
            }

            // This is done to ensure when file is seen by other programs, it is completely ready
            if (File.Exists(dest))
            {
                File.Delete(dest);
            }

            File.Move(temp, dest);
        }
        public static void BookmarkedPdfFilesMerge(string outputPdf, string[] sourcePdfs)
        {
            PdfReader reader = null;
            Document document = new Document();
            PdfImportedPage page = null;
            PdfCopy pdfCpy = null;
            int n = 0;
            int totalPages = 0;
            int page_offset = 0;
            List<Dictionary<string, object>> bookmarks = new List<Dictionary<string, object>>();
            IList<Dictionary<string, object>> tempBookmarks;
            for (int i = 0; i <= sourcePdfs.GetUpperBound(0); i++)
            {
                reader = new PdfReader(sourcePdfs[i]);
                reader.ConsolidateNamedDestinations();
                n = reader.NumberOfPages;
                tempBookmarks = SimpleBookmark.GetBookmark(reader);

                if (i == 0)
                {
                    document = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
                    pdfCpy = new PdfCopy(document, new FileStream(outputPdf, FileMode.Create));
                    document.Open();
                    PdfReader.unethicalreading = true;
                    SimpleBookmark.ShiftPageNumbers(tempBookmarks, page_offset, null);
                    page_offset += n;
                    if (tempBookmarks != null)
                        bookmarks.AddRange(tempBookmarks);
                    totalPages = n;
                }
                else
                {
                    SimpleBookmark.ShiftPageNumbers(tempBookmarks, page_offset, null);
                    if (tempBookmarks != null)
                        bookmarks.AddRange(tempBookmarks);

                    page_offset += n;
                    totalPages += n;
                }

                for (int j = 1; j <= n; j++)
                {
                    page = pdfCpy.GetImportedPage(reader, j);
                    pdfCpy.AddPage(page);

                }
                PdfReader.unethicalreading = false;
                reader.Close();

            }
            pdfCpy.Outlines = bookmarks;
            document.Close();
        }

        public static float GetWidth(this PdfArray size)
        {
            return size.GetAsNumber(2).FloatValue - size.GetAsNumber(0).FloatValue;
        }

        public static float GetHeight(this PdfArray size)
        {
            return size.GetAsNumber(3).FloatValue - size.GetAsNumber(1).FloatValue;
        }

        public static void GenerateBarcodeManifest(string apiUrl, string templatePath, string coversheetPath, dynamic manifestData)
        {
            dynamic mergeRequest = new ExpandoObject();
            mergeRequest.Settings = new { MailMergeOptions = "RemoveEmptyParagraphs", OutputDocType = "pdf", Template = Convert.ToBase64String(File.ReadAllBytes(templatePath)) };
            mergeRequest.Data = manifestData;
            mergeRequest.TransformData = null;
            using (var client = new HttpClient())
            {
                var stringContent = new StringContent(JsonConvert.SerializeObject(mergeRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync(apiUrl, stringContent).Result;
                var responseString = httpResponse.Content.ReadAsStringAsync().Result;
                var resObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                File.WriteAllBytes(coversheetPath, Convert.FromBase64String(resObj.documentData.Value));
            }
        }
    }
}
