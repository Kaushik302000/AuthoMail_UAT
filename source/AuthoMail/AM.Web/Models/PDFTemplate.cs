using AM.Lib;
using AM.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class PDFTemplate : Helper
    {
        public static byte[] CreatePdf(ProjectRequest model)
        {

            Document document = new Document(PageSize.LETTER);
            document.SetMargins(Utilities.InchesToPoints(-0.3f), Utilities.InchesToPoints(-0.3f), Utilities.InchesToPoints(0.9f), Utilities.InchesToPoints(0.1f));
            MemoryStream pdfStream = new MemoryStream();

            PdfWriter writer = PdfWriter.GetInstance(document, pdfStream);
            document.Open();
            var logo = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("~/clui/images/corelogic.png")), BaseColor.WHITE);
            logo.Alignment = Element.ALIGN_LEFT;
            logo.ScaleToFit(Utilities.InchesToPoints(1.4f), Utilities.InchesToPoints(1.4f));
            logo.SetDpi(600, 600);


            PdfPTable table = new PdfPTable(1);
            PdfPCell imageCell = new PdfPCell(logo);
            //imageCell.PaddingTop = Utilities.InchesToPoints(-0.4f);
            imageCell.PaddingLeft = Utilities.InchesToPoints(0.6f);
            imageCell.Border = Rectangle.NO_BORDER;
            HeaderLogo(document, writer, imageCell);

            Phrase headerPh = new Phrase("New Project Request/Pricing Form", new Font(verdana, 10));
            headerPh.Leading = 5;
            PdfPCell header = new PdfPCell(headerPh);
            header.BackgroundColor = new BaseColor(189, 215, 238);
            header.HorizontalAlignment = Element.ALIGN_CENTER;
            header.VerticalAlignment = Element.ALIGN_MIDDLE;
            header.Border = Rectangle.NO_BORDER;
            header.UseAscender = true;
            header.ExtraParagraphSpace = 2;
            table.AddCell(header);

            PdfPTable projectDetails = new PdfPTable(new float[] { 20, 28, 4, 20, 28 });
            projectDetails.AddCell(LeftCell("Name of Project:"));
            projectDetails.AddCell(RightCell(model.ClientName, Rectangle.BOX));
            projectDetails.AddCell(getEmptyCell());
            projectDetails.AddCell(LeftCell("Submit Date:"));

            projectDetails.AddCell(RightCell(model.SubmittedOn.HasValue ? model.SubmittedOn.Value.ToString("dd/MM/yyyy") : "", Rectangle.BOX));

            PdfPCell tableCell = new PdfPCell(projectDetails);
            tableCell.Border = Rectangle.NO_BORDER;
            tableCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(tableCell);

            projectDetails = new PdfPTable(new float[] { 20, 28, 4, 20, 28 });
            projectDetails.AddCell(LeftCell("Submitter:"));
            projectDetails.AddCell(RightCell(model.SubmittedBy, Rectangle.BOX));
            projectDetails.AddCell(getEmptyCell());
            projectDetails.AddCell(LeftCell("Project Start Date:"));
            projectDetails.AddCell(RightCell(model.ProdStartOn.HasValue ? model.ProdStartOn.Value.ToString("dd/MM/yyyy") : "", Rectangle.BOX));

            PdfPCell tableCellsub = new PdfPCell(projectDetails);
            tableCellsub.Border = Rectangle.NO_BORDER;
            tableCellsub.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(tableCellsub);


            PdfPTable projectDesTable = new PdfPTable(new float[] { 20, 80 });
            projectDesTable.AddCell(LeftCell("Project Description:"));
            projectDesTable.AddCell(RightCell(model.Description, Rectangle.BOX));
            PdfPCell projectDestableCell = new PdfPCell(projectDesTable);
            projectDestableCell.Border = Rectangle.NO_BORDER;
            projectDestableCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(projectDestableCell);

            table.AddCell(getEmptyCell());
            table.AddCell(SectionCell("A. File Transmission & Types"));
            PdfPTable sectionA = new PdfPTable(1);


            PdfPTable fileTransmissionTable = new PdfPTable(new float[] { 18, 4, 10, 4, 14, 4, 18, 4, 24 });

            var fileTransmittedViaList = new List<string> { "Client FTP", "CoreLogic FTP", "CL Pull from Client System Imaging", "Mortgage Cadence Integration" };
            //CheckBoxList("Files transmitted via:", model.FileTransmittedVia, fileTransmissionTable, fileTransmittedViaList);
            CheckBoxListMultipleValue("Files transmitted via:", model.FileTransmittedVia, fileTransmissionTable, fileTransmittedViaList, model.FilesTransmittedOther, true);

            fileTransmissionTable.AddCell(ColspanCell(9));
            Phrase sweepTimesPh = new Phrase("How many times will Corelogic be sweeping the SFTP?", new Font(verdana, 8));
            PdfPCell sweepTimes = new PdfPCell(sweepTimesPh);
            sweepTimes.Colspan = 5;
            sweepTimes.HorizontalAlignment = Element.ALIGN_LEFT;
            sweepTimes.VerticalAlignment = Element.ALIGN_MIDDLE;
            sweepTimes.Border = Rectangle.NO_BORDER;
            sweepTimes.PaddingRight = Utilities.InchesToPoints(0);
            sweepTimes.UseAscender = true;
            sweepTimes.ExtraParagraphSpace = 2;
            fileTransmissionTable.AddCell(sweepTimes);
            fileTransmissionTable.AddCell(BorderCell(model.NoOfEstSweepTimes == "2X"));
            fileTransmissionTable.AddCell(LeftCell("2X\n", 8));
            fileTransmissionTable.AddCell(BorderCell(model.NoOfEstSweepTimes == "4X"));
            fileTransmissionTable.AddCell(LeftCell("4X\n", 8));
            fileTransmissionTable.AddCell(ColspanCell(9));
            Phrase ESTSweepTimePh = new Phrase("What times of day (EST) will the sweeps take place?", new Font(verdana, 8));
            PdfPCell ESTSweepTime = new PdfPCell(ESTSweepTimePh);
            ESTSweepTime.Colspan = 5;
            ESTSweepTime.HorizontalAlignment = Element.ALIGN_LEFT;
            ESTSweepTime.VerticalAlignment = Element.ALIGN_MIDDLE;
            ESTSweepTime.Border = Rectangle.NO_BORDER;
            ESTSweepTime.PaddingRight = Utilities.InchesToPoints(0);
            ESTSweepTime.UseAscender = true;
            ESTSweepTime.ExtraParagraphSpace = 2;
            fileTransmissionTable.AddCell(ESTSweepTime);
            fileTransmissionTable.AddCell(getEmptyCell());
            fileTransmissionTable.AddCell(RightCell(model.ESTSweepTime, Rectangle.BOX));
            fileTransmissionTable.AddCell(getEmptyCell());
            fileTransmissionTable.AddCell(getEmptyCell());
            fileTransmissionTable.AddCell(ColspanCell(9));

            var typeOfFilesList = new List<string> { "PDF", "HP", "TIFF" };
            //CheckBoxList("Type of Files:", model.TypeOfFiles, fileTransmissionTable, typeOfFilesList);
            CheckBoxListMultipleValue("Type of Files:", model.TypeOfFiles, fileTransmissionTable, typeOfFilesList, model.TypeOfFilesOther);

            fileTransmissionTable.AddCell(ColspanCell(9));

            PdfPCell sectionB = new PdfPCell(new Phrase("B. File Receipt & Service Levels", new Font(verdana, 10)));
            sectionB.Colspan = 9;
            sectionB.BackgroundColor = new BaseColor(189, 215, 238);
            sectionB.HorizontalAlignment = Element.ALIGN_LEFT;
            sectionB.VerticalAlignment = Element.ALIGN_MIDDLE;
            sectionB.Border = Rectangle.NO_BORDER;
            sectionB.PaddingRight = Utilities.InchesToPoints(0);
            sectionB.UseAscender = true;
            sectionB.ExtraParagraphSpace = 2;
            fileTransmissionTable.AddCell(sectionB);

            Phrase TransmittedTimePh = new Phrase("What time will files be transmitted to CoreLogic?", new Font(verdana, 8));
            PdfPCell TransmittedTime = new PdfPCell(TransmittedTimePh);
            TransmittedTime.Colspan = 5;
            TransmittedTime.HorizontalAlignment = Element.ALIGN_LEFT;
            TransmittedTime.VerticalAlignment = Element.ALIGN_MIDDLE;
            TransmittedTime.Border = Rectangle.NO_BORDER;
            TransmittedTime.PaddingRight = Utilities.InchesToPoints(0);
            TransmittedTime.UseAscender = true;
            TransmittedTime.ExtraParagraphSpace = 2;
            fileTransmissionTable.AddCell(TransmittedTime);
            fileTransmissionTable.AddCell(BorderCell(model.TransmittedTime.Contains("AM")));
            fileTransmissionTable.AddCell(LeftCell("AM\n", 8));
            fileTransmissionTable.AddCell(BorderCell(model.TransmittedTime.Contains("PM")));
            fileTransmissionTable.AddCell(LeftCell("PM\n", 8));
            fileTransmissionTable.AddCell(ColspanCell(9));
            //Phrase TransmittedTimePh = new Phrase("What time will files be transmitted to CoreLogic?", new Font(verdana, 8));
            //PdfPCell TransmittedTime = new PdfPCell(TransmittedTimePh);
            //TransmittedTime.Colspan = 5;
            //TransmittedTime.HorizontalAlignment = Element.ALIGN_LEFT;
            //TransmittedTime.VerticalAlignment = Element.ALIGN_MIDDLE;
            //TransmittedTime.Border = Rectangle.NO_BORDER;
            //TransmittedTime.PaddingRight = Utilities.InchesToPoints(0);
            //TransmittedTime.PaddingTop = Utilities.InchesToPoints(0.2f);
            //TransmittedTime.UseAscender = true;
            //TransmittedTime.ExtraParagraphSpace = 2;
            //fileTransmissionTable.AddCell(TransmittedTime);
            //fileTransmissionTable.AddCell(getEmptyCell());
            //fileTransmissionTable.AddCell(RightCell(model.TransmittedTime));
            //fileTransmissionTable.AddCell(getEmptyCell());
            //fileTransmissionTable.AddCell(getEmptyCell());
            //fileTransmissionTable.AddCell(ColspanCell(9));

            var serviceLevelList = new List<string> { "Same Day", "24 Hours", "48 Hours" };
            //CheckBoxList("Service Level:", model.ServiceLevel, fileTransmissionTable, serviceLevelList);
            CheckBoxListMultipleValue("Service Level:", model.ServiceLevel, fileTransmissionTable, serviceLevelList, model.ServiceLevelOther);
            fileTransmissionTable.AddCell(ColspanCell(9));



            PdfPCell fileTransmissionTableCell = new PdfPCell(fileTransmissionTable);
            fileTransmissionTableCell.Border = Rectangle.NO_BORDER;
            fileTransmissionTableCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(fileTransmissionTableCell);

            PdfPTable EstPages = new PdfPTable(new float[] { 35, 13, 4, 35, 13 });
            EstPages.AddCell(LeftCell("Est. pages per Package:", 8));
            EstPages.AddCell(RightCell(model.EstimatedPagesPerPackage.ToString()));
            EstPages.AddCell(getEmptyCell());
            EstPages.AddCell(LeftCell("Estimated # of Packages Per Month:", 8));
            EstPages.AddCell(RightCell(model.EstimatedPackagesMonthly.ToString()));
            EstPages.AddCell(LeftCell("Est. # of Packages Per Day:", 8));
            EstPages.AddCell(RightCell(model.EstimatedPackagesDaily.ToString()));
            EstPages.AddCell(getEmptyCell());
            EstPages.AddCell(LeftCell("Estimated # of Packages Annually:", 8));
            EstPages.AddCell(RightCell(model.EstimatedPackagesAnnually.ToString()));
            tableCell = new PdfPCell(EstPages);
            tableCell.Border = Rectangle.NO_BORDER;
            table.AddCell(tableCell);

            PdfPTable tableC = new PdfPTable(new float[] { 4, 7, 4, 7, 4, 7, 4, 8, 4, 7, 4, 7, 4, 7 });
            PdfPCell sectionC = new PdfPCell(new Phrase("C. Daily Print Schedule", new Font(verdana, 10)));
            sectionC.Colspan = 14;
            sectionC.BackgroundColor = new BaseColor(189, 215, 238);
            sectionC.HorizontalAlignment = Element.ALIGN_LEFT;
            sectionC.VerticalAlignment = Element.ALIGN_MIDDLE;
            sectionC.Border = Rectangle.NO_BORDER;
            sectionC.PaddingRight = Utilities.InchesToPoints(0);
            sectionC.UseAscender = true;
            sectionC.ExtraParagraphSpace = 2;
            tableC.AddCell(sectionC);
            tableC.AddCell(ColspanCell(14));
            var daysList = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DayNames.ToList();

            CheckBoxListSectionC(model.DailyPrintSchedule, tableC, daysList, 0);

            PdfPCell tableCCell = new PdfPCell(tableC);
            tableCCell.Border = Rectangle.NO_BORDER;
            tableCCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(tableCCell);
            table.AddCell(getEmptyCell());

            PdfPTable tableD = new PdfPTable(new float[] { 4, 7, 4, 7, 4, 7, 4, 8, 4, 7, 4, 7, 4, 7 });
            PdfPCell sectionD = new PdfPCell(new Phrase("D. Project Requirements: Paper & Print (Check all that apply)", new Font(verdana, 10)));
            sectionD.Colspan = 14;
            sectionD.BackgroundColor = new BaseColor(189, 215, 238);
            sectionD.HorizontalAlignment = Element.ALIGN_LEFT;
            sectionD.VerticalAlignment = Element.ALIGN_MIDDLE;
            sectionD.Border = Rectangle.NO_BORDER;
            sectionD.PaddingRight = Utilities.InchesToPoints(0);
            sectionD.UseAscender = true;
            sectionD.ExtraParagraphSpace = 2;
            tableD.AddCell(sectionD);
            tableD.AddCell(ColspanCell(14));
            var projectRequirements = new List<string> { "Mixed Media", "Letter Only", "Legal Only", "Stapled", "Not Stapled", "24# Paper", "Insert Required", "White", "Color (specify)", "Print 1-sided", "Print 2-sided", "B&W", "Color", "B&W & Color" };
            CheckBoxListSectionC(model.PaperPrintRequirements, tableD, projectRequirements, 14, false);

            tableD.AddCell(
                new PdfPCell(new Phrase(model.PaperPrintRequirementsOther == null ? "\n" : model.PaperPrintRequirementsOther, new Font(verdana, 10, Font.NORMAL, BaseColor.RED)))
                {
                    Colspan = 14,
                    Border = Rectangle.BOTTOM_BORDER,
                    PaddingTop = Utilities.InchesToPoints(0.1f)
                });

            PdfPCell tableDCell = new PdfPCell(tableD);
            tableDCell.Border = Rectangle.NO_BORDER;
            tableDCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(tableDCell);
            table.AddCell(getEmptyCell());


            PdfPTable tableE = new PdfPTable(new float[] { 4, 15, 4, 15, 4, 15, 4, 15 });
            PdfPCell sectionE = new PdfPCell(new Phrase("E. How are packages to be sent for the customer:", new Font(verdana, 10)));
            sectionE.Colspan = 8;
            sectionE.BackgroundColor = new BaseColor(189, 215, 238);
            sectionE.HorizontalAlignment = Element.ALIGN_LEFT;
            sectionE.VerticalAlignment = Element.ALIGN_MIDDLE;
            sectionE.Border = Rectangle.NO_BORDER;
            sectionE.PaddingRight = Utilities.InchesToPoints(0);
            sectionE.UseAscender = true;
            sectionE.ExtraParagraphSpace = 2;
            tableE.AddCell(sectionE);
            tableE.AddCell(ColspanCell(8));
            var packagesSentToCustomerList = new List<string> { "Mailed-USPS", "Federal Express", "Certified USPS" };
            CheckBoxListMultipleValueOtherNewline(model.PackagesSentToCustomer, tableE, packagesSentToCustomerList, model.PackagesSentToCustomerOther);
            //CheckBoxListSectionC(model.PackagesSentToCustomer, tableE, packagesSentToCustomerList, 0);
            tableE.AddCell(ColspanCell(8));
            //Phrase infoText = new Phrase("First Mortgage will sent through Federal Express and Equity line of credit will go out in USPS", new Font(verdana, 8));
            //PdfPCell info = new PdfPCell(infoText);
            //info.Colspan = 8;
            //info.HorizontalAlignment = Element.ALIGN_LEFT;
            //info.VerticalAlignment = Element.ALIGN_MIDDLE;
            //info.Border = Rectangle.NO_BORDER;
            //info.PaddingRight = Utilities.InchesToPoints(0);
            //info.UseAscender = true;
            //info.ExtraParagraphSpace = 2;
            //tableE.AddCell(info);

            PdfPCell tableECell = new PdfPCell(tableE);
            tableECell.Border = Rectangle.NO_BORDER;
            tableECell.PaddingTop = Utilities.InchesToPoints(0);
            table.AddCell(tableECell);
            //table.AddCell(getEmptyCell());


            PdfPTable tableF = new PdfPTable(new float[] { 4, 15, 4, 15, 4, 15, 4, 15, 4, 15 });
            PdfPCell sectionF = new PdfPCell(new Phrase("F. Envelopes", new Font(verdana, 10)));
            sectionF.Colspan = 10;
            sectionF.BackgroundColor = new BaseColor(189, 215, 238);
            sectionF.HorizontalAlignment = Element.ALIGN_LEFT;
            sectionF.VerticalAlignment = Element.ALIGN_MIDDLE;
            sectionF.Border = Rectangle.NO_BORDER;
            sectionF.PaddingRight = Utilities.InchesToPoints(0);
            sectionF.UseAscender = true;
            sectionF.ExtraParagraphSpace = 2;
            tableF.AddCell(sectionF);
            //tableF.AddCell(ColspanCell(8));
            Phrase envelopeText = new Phrase("If mailed, what type of envelope is required?", new Font(verdana, 8));
            PdfPCell envelope = new PdfPCell(envelopeText);
            envelope.Colspan = 4;
            envelope.HorizontalAlignment = Element.ALIGN_LEFT;
            envelope.VerticalAlignment = Element.ALIGN_MIDDLE;
            envelope.Border = Rectangle.NO_BORDER;
            envelope.PaddingRight = Utilities.InchesToPoints(0);
            envelope.UseAscender = true;
            envelope.ExtraParagraphSpace = 2;
            tableF.AddCell(envelope);
            tableF.AddCell(BorderCell(model.IsSampleEnvelopeAttached));
            //------
            Phrase sampleEnvelopePh = new Phrase("Sample envelope(s) attached?", new Font(verdana, 10, Font.NORMAL));
            PdfPCell sampleEnvelope = new PdfPCell(sampleEnvelopePh);
            sampleEnvelope.Colspan = 5;
            sampleEnvelope.HorizontalAlignment = Element.ALIGN_LEFT;
            sampleEnvelope.VerticalAlignment = Element.ALIGN_MIDDLE;
            sampleEnvelope.Border = Rectangle.NO_BORDER;
            //sampleEnvelope.BorderWidthBottom = 1f;
            sampleEnvelope.PaddingLeft = Utilities.InchesToPoints(0.1f);
            sampleEnvelope.UseAscender = true;
            sampleEnvelope.ExtraParagraphSpace = 2;
            tableF.AddCell(sampleEnvelope);
            //tableF.AddCell(RightCell("Sample envelope(s) attached?", Rectangle.NO_BORDER, 5));
            //------

            //tableF.AddCell(ColspanCell(4));
            var envelopeList = new List<string> { "# 10", "6 x 9.5", "9 x 12", "10 x 12", "10 x 13" };
            //CheckBoxListSectionC(model.Envelopes, tableF, envelopeList, 8);
            CheckBoxListMultipleValueOtherNewline(model.Envelopes, tableF, envelopeList, model.EnvelopesOther);

            //tableF.AddCell(BorderCell(!model.EnvelopesOther.IsNullOrEmpty()));
            //Phrase sampleEnvelopeOtherPh = new Phrase(model.EnvelopesOther, new Font(verdana, 10, Font.NORMAL));
            //PdfPCell sampleEnvelopeOther = new PdfPCell(sampleEnvelopeOtherPh);
            //sampleEnvelopeOther.Colspan = 9;
            //sampleEnvelopeOther.HorizontalAlignment = Element.ALIGN_LEFT;
            //sampleEnvelopeOther.VerticalAlignment = Element.ALIGN_MIDDLE;
            //sampleEnvelopeOther.Border = Rectangle.NO_BORDER;
            //sampleEnvelopeOther.BorderWidthBottom = 1f;
            //sampleEnvelopeOther.PaddingLeft = Utilities.InchesToPoints(0.1f);
            //sampleEnvelopeOther.UseAscender = true;
            //sampleEnvelopeOther.ExtraParagraphSpace = 2;
            //tableF.AddCell(sampleEnvelopeOther);


            PdfPCell tableFCell = new PdfPCell(tableF);
            tableFCell.Border = Rectangle.NO_BORDER;
            tableFCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(tableFCell);
            Footer(document, writer);
            document.Add(table);
            document.NewPage();
            HeaderLogo(document, writer, imageCell);
            table = new PdfPTable(1);
            table.AddCell(getEmptyCell());
            PdfPTable tableFNew = new PdfPTable(new float[] { 4, 15, 4, 15, 4, 15, 4, 15 });

            Phrase businessReplyMailText = new Phrase("Does it need a Business Reply Mail envelope included?", new Font(verdana, 8));
            PdfPCell businessReplyMail = new PdfPCell(businessReplyMailText);
            businessReplyMail.Colspan = 4;
            businessReplyMail.HorizontalAlignment = Element.ALIGN_LEFT;
            businessReplyMail.VerticalAlignment = Element.ALIGN_MIDDLE;
            businessReplyMail.Border = Rectangle.NO_BORDER;
            businessReplyMail.PaddingRight = Utilities.InchesToPoints(0);
            businessReplyMail.UseAscender = true;
            businessReplyMail.ExtraParagraphSpace = 2;
            tableFNew.AddCell(businessReplyMail);
            tableFNew.AddCell(BorderCell(model.BusinessReplyMailEnvelope == "Yes"));
            tableFNew.AddCell(LeftCell("Yes", 8));
            tableFNew.AddCell(BorderCell(model.BusinessReplyMailEnvelope == "No"));
            tableFNew.AddCell(LeftCell("No", 8));
            tableFNew.AddCell(BorderCell(model.BusinessReplyMailEnvelope == "#9 Business Reply Mail"));
            tableFNew.AddCell(LeftCell("#9 Business Reply Mail", 8));
            tableFNew.AddCell(BorderCell(model.BusinessReplyMailEnvelope != "Yes" && model.BusinessReplyMailEnvelope != "No" && model.BusinessReplyMailEnvelope != "#9 Business Reply Mail"));
            //tableFNew.AddCell(LeftCell(string.Format("Other({0})", model.BusinessReplyMailEnvelope != "No" && model.BusinessReplyMailEnvelope != "#9 Business Reply Mail" ? model.BusinessReplyMailEnvelope : "Specify"), 8));
            tableFNew.AddCell(LeftCell(string.Format("Other({0})", model.BusinessReplyMailEnvelopeOther), 8, 4));
            tableFNew.AddCell(ColspanCell(4));
            tableFNew.AddCell(ColspanCell(8));
            YesNoCheckBox("Will customer supply their envelopes?", tableFNew, model.IsCustomerSupplyEnvelope);
            tableFNew.AddCell(ColspanCell(8));
            YesNoCheckBox("Will customer provide artwork for the envelope(s)?", tableFNew, model.IsCustomerProvideArtworkEnvelope);
            Phrase notesText = new Phrase("Note: If custom envelope is required, please allow 3-4 weeks for delivery", new Font(verdana, 8, Font.ITALIC, BaseColor.RED));
            PdfPCell notes = new PdfPCell(notesText);
            notes.Colspan = 8;
            notes.HorizontalAlignment = Element.ALIGN_LEFT;
            notes.VerticalAlignment = Element.ALIGN_MIDDLE;
            notes.Border = Rectangle.NO_BORDER;
            notes.PaddingTop = Utilities.InchesToPoints(0.1f);
            notes.PaddingRight = Utilities.InchesToPoints(0);
            notes.UseAscender = true;
            notes.ExtraParagraphSpace = 2;
            tableFNew.AddCell(notes);
            tableFNew.AddCell(ColspanCell(8));

            PdfPCell tableFNewCell = new PdfPCell(tableFNew);
            tableFNewCell.Border = Rectangle.NO_BORDER;
            tableFNewCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(tableFNewCell);
            table.AddCell(getEmptyCell());


            PdfPTable tableG = new PdfPTable(new float[] { 4, 15, 4, 15, 4, 15, 4, 15 });
            PdfPCell sectionG = new PdfPCell(new Phrase("G. Postage *", new Font(verdana, 10)));
            sectionG.Colspan = 8;
            sectionG.BackgroundColor = new BaseColor(189, 215, 238);
            sectionG.HorizontalAlignment = Element.ALIGN_LEFT;
            sectionG.VerticalAlignment = Element.ALIGN_MIDDLE;
            sectionG.Border = Rectangle.NO_BORDER;
            sectionG.PaddingRight = Utilities.InchesToPoints(0);
            sectionG.UseAscender = true;
            sectionG.ExtraParagraphSpace = 2;
            tableG.AddCell(sectionG);
            tableG.AddCell(ColspanCell(8));
            YesNoCheckBox("Do you have an active general permit number if USPS is the preferred shipping method", tableG, model.IsClientPostageAccount);
            PdfPCell tableGCell = new PdfPCell(tableG);
            tableGCell.Border = Rectangle.NO_BORDER;
            tableGCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(tableGCell);
            table.AddCell(getEmptyCell());


            PdfPTable tableH = new PdfPTable(new float[] { 4, 15, 4, 15, 4, 15, 4, 15 });
            PdfPCell sectionH = new PdfPCell(new Phrase("H. Scanning / AuthoMail", new Font(verdana, 10)));
            sectionH.Colspan = 8;
            sectionH.BackgroundColor = new BaseColor(189, 215, 238);
            sectionH.HorizontalAlignment = Element.ALIGN_LEFT;
            sectionH.VerticalAlignment = Element.ALIGN_MIDDLE;
            sectionH.Border = Rectangle.NO_BORDER;
            sectionH.PaddingRight = Utilities.InchesToPoints(0);
            sectionH.UseAscender = true;
            sectionH.ExtraParagraphSpace = 2;
            tableH.AddCell(sectionH);
            tableH.AddCell(ColspanCell(8));
            YesNoCheckBox("Do these packages need to be scanned to AuthoMail?", tableH, model.IsScanInAuthoMailDallas);
            PdfPCell tableHCell = new PdfPCell(tableH);
            tableHCell.Border = Rectangle.NO_BORDER;
            tableHCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(tableHCell);
            table.AddCell(getEmptyCell());

            Phrase OtherPh = new Phrase("Please use this area to expand on \"Other\" requirements or to provide additional detailed information", new Font(verdana, 12));
            OtherPh.Leading = 5;
            PdfPCell otherCell = new PdfPCell(OtherPh);
            otherCell.BackgroundColor = new BaseColor(189, 215, 238);
            otherCell.HorizontalAlignment = Element.ALIGN_CENTER;
            otherCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            otherCell.Border = Rectangle.NO_BORDER;
            otherCell.UseAscender = true;
            otherCell.ExtraParagraphSpace = 2;
            table.AddCell(otherCell);
            table.AddCell(getEmptyCell());

            table.AddCell(LeftCell("Files transmitted (Other):\n\n"));
            table.AddCell(RightCell(model.FilesTransmittedDetail));
            table.AddCell(ColspanCell(1));
            table.AddCell(ColspanCell(1));
            table.AddCell(LeftCell("Types of Files (Other):\n\n"));
            table.AddCell(RightCell(model.TypeOfFilesDetail));
            table.AddCell(ColspanCell(1));
            table.AddCell(ColspanCell(1));
            table.AddCell(LeftCell("Paper & Print (Additional Details):\n\n"));
            table.AddCell(RightCell(model.PaperAndPrintDetails));
            table.AddCell(ColspanCell(1));
            table.AddCell(ColspanCell(1));
            table.AddCell(LeftCell("Shipping / Mailing (Additional Details):\n\n"));
            table.AddCell(RightCell(model.ShippingMailingDetail));
            document.Add(table);
            Footer(document, writer);
            table = new PdfPTable(1);
            document.NewPage();
            HeaderLogo(document, writer, imageCell);
            table.AddCell(getEmptyCell());
            table.AddCell(LeftCell("Mailing Envelope (Additional Details):\n\n"));
            table.AddCell(RightCell(model.MailingEnvelopeDetail));
            table.AddCell(ColspanCell(1));
            table.AddCell(ColspanCell(1));
            table.AddCell(LeftCell("Business Reply Mail Envelope (Additional Details):\n\n"));
            table.AddCell(RightCell(model.BusinessReplyMailEnvelopeDetail));
            table.AddCell(ColspanCell(1));
            table.AddCell(ColspanCell(1));
            table.AddCell(LeftCell("List of Doc Types and how many pages will each file contain:\n\n"));
            table.AddCell(RightCell(model.ListOfDocTypesAndPages));
            table.AddCell(ColspanCell(1));
            table.AddCell(ColspanCell(1));
            table.AddCell(LeftCell("Provide pdf sample (print ready format) of each document that CoreLogic will be printing and shipping and samples of envelopes if any.\n\n"));
            table.AddCell(RightCell(model.PDFSample));

            PdfPTable beTable = new PdfPTable(new float[] { 20, 10, 10 });
            beTable.AddCell(LeftCell("WILL INSERTS BE REQURIED ?"));
            beTable.AddCell(RightCell(model.InsertRequired ? "Yes" : "No"));
            beTable.AddCell(ColspanCell(1));
            beTable.AddCell(LeftCell("Are booklets required?"));
            beTable.AddCell(RightCell(model.BookletRequired ? "Yes" : "No"));
            beTable.AddCell(ColspanCell(1));
            beTable.AddCell(LeftCell("Are Business Reply Mail envelopes required?"));
            beTable.AddCell(RightCell(model.BusinessReplyMailEnvelopeRequired ? "Yes" : "No"));
            beTable.AddCell(ColspanCell(1));
            beTable.AddCell(LeftCell("Are any other materials required?"));
            beTable.AddCell(RightCell(model.OtherMaterialRequired ? "Yes" : "No"));
            beTable.AddCell(ColspanCell(1));
            beTable.AddCell(LeftCell("Will any inserted materials need to be ordered by Corelogic or provided by client? Additional information:"));
            beTable.AddCell(RightCell(model.InsertMaterialOrderedBy));
            beTable.AddCell(ColspanCell(1));



            PdfPCell beCell = new PdfPCell(beTable);
            beCell.Border = Rectangle.NO_BORDER;
            beCell.PaddingTop = Utilities.InchesToPoints(0.1f);
            table.AddCell(beCell);
            table.AddCell(getEmptyCell());

            table.AddCell(ColspanCell(1));
            table.AddCell(ColspanCell(1));
            table.AddCell(LeftCell("Additional information:\n\n"));
            table.AddCell(RightCell(model.AdditionalInfo));
            document.Add(table);
            Footer(document, writer);
            document.Close();
            return pdfStream.ToArray();
        }


    }

    public class Helper
    {
        public static string fontPathDir = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["FontsDirPath"].Trim());
        public static BaseFont courier = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false);
        public static BaseFont courierBold = BaseFont.CreateFont(BaseFont.COURIER_BOLD, BaseFont.CP1252, false);
        public static BaseFont USPSIMBStandard = BaseFont.CreateFont(Path.Combine(fontPathDir, "USPSIMBSTANDARD.TTF"), BaseFont.CP1252, true);
        public static BaseFont calibriRegular = BaseFont.CreateFont(Path.Combine(fontPathDir, "CALIBRI.TTF"), BaseFont.CP1252, false);
        public static BaseFont calibriBold = BaseFont.CreateFont(Path.Combine(fontPathDir, "CALIBRIB.TTF"), BaseFont.CP1252, false);
        public static BaseFont arial = BaseFont.CreateFont(Path.Combine(fontPathDir, "ARIAL.TTF"), BaseFont.CP1252, false);
        public static BaseFont arialBold = BaseFont.CreateFont(Path.Combine(fontPathDir, "ARIALBD.TTF"), BaseFont.CP1252, false);
        public static BaseFont free3of9 = BaseFont.CreateFont(Path.Combine(fontPathDir, "FREE3OF9.TTF"), BaseFont.CP1252, true);
        public static BaseFont timeNewRomanBd = BaseFont.CreateFont(Path.Combine(fontPathDir, "TIMESBD.TTF"), BaseFont.CP1252, false);
        public static BaseFont timeNewRomanIl = BaseFont.CreateFont(Path.Combine(fontPathDir, "TIMESI.TTF"), BaseFont.CP1252, false);
        public static BaseFont timeNewRomanRg = BaseFont.CreateFont(Path.Combine(fontPathDir, "TIMES.TTF"), BaseFont.CP1252, false);
        public static BaseFont verdana = BaseFont.CreateFont(Path.Combine(fontPathDir, "VERDANA.TTF"), BaseFont.CP1252, true);
        public static BaseFont zapfDingbats = BaseFont.CreateFont(Path.Combine(fontPathDir, "ZapfDingbats.TTF"), BaseFont.CP1252, true);

        public static PdfPCell getEmptyCell()
        {
            //create empty cell 
            return new PdfPCell(new Phrase(" "))
            {
                Border = PdfPCell.NO_BORDER,
            };
        }
        public static FieldPositioningEvents CreateEvent(PdfWriter writer1, TextField tf)
        {
            FieldPositioningEvents events = new FieldPositioningEvents(writer1, tf.GetTextField());

            return events;
        }
        public static void Footer(Document document, PdfWriter writer)
        {
            PdfPTable tblFooterCode = new PdfPTable(1);
            tblFooterCode.TotalWidth = document.PageSize.Width;
            tblFooterCode.PaddingTop = 0f;
            tblFooterCode.WidthPercentage = 100;
            tblFooterCode.DefaultCell.Border = Rectangle.NO_BORDER;
            tblFooterCode.DefaultCell.SpaceCharRatio = PdfWriter.NO_SPACE_CHAR_RATIO;

            PdfPCell code = new PdfPCell(new Phrase("©2019 CoreLogic. Private & Confidential", new Font(arialBold, 9, Font.NORMAL, BaseColor.GRAY)));
            code.Border = PdfPCell.NO_BORDER;

            //leftText.PaddingBottom = Utilities.InchesToPoints(-0.33f);
            code.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            tblFooterCode.AddCell(code);
            tblFooterCode.WriteSelectedRows(0, 130, Utilities.InchesToPoints(0f), document.Bottom + 30, writer.DirectContent);

        }
        public static void HeaderLogo(Document document, PdfWriter writer, PdfPCell imageCell)
        {
            PdfPTable tblHeaderCode = new PdfPTable(1);
            tblHeaderCode.TotalWidth = document.PageSize.Width;
            tblHeaderCode.PaddingTop = Utilities.InchesToPoints(1f);
            tblHeaderCode.WidthPercentage = 100;
            tblHeaderCode.DefaultCell.Border = Rectangle.NO_BORDER;
            tblHeaderCode.DefaultCell.SpaceCharRatio = PdfWriter.NO_SPACE_CHAR_RATIO;

            imageCell.Border = Rectangle.NO_BORDER;
            //leftText.PaddingBottom = Utilities.InchesToPoints(-0.33f);
            tblHeaderCode.AddCell(imageCell);
            tblHeaderCode.WriteSelectedRows(0, 10, Utilities.InchesToPoints(0f), document.Top + 45, writer.DirectContent);

        }
        public static void CheckBoxList(string title, string value, PdfPTable table, List<string> list)
        {
            bool isFound = false;
            table.AddCell(LeftCell(title, 8));

            foreach (var item in list)
            {
                if (item == value)
                {
                    table.AddCell(BorderCell(true));
                    table.AddCell(LeftCell(item, 8));
                    isFound = true;
                }
                else
                {
                    table.AddCell(BorderCell(false));
                    table.AddCell(LeftCell(item, 8));
                }
            }
            if (isFound)
            {
                table.AddCell(BorderCell(false));
                table.AddCell(LeftCell("Other", 8));
            }
            else
            {
                table.AddCell(BorderCell(true));
                table.AddCell(LeftCell(string.Format("Other ({0})", value), 8));
            }
        }
        public static void CheckBoxListMultipleValue(string title, string value, PdfPTable table, List<string> list, string customValue, bool skipFirst = false)
        {
            if (!title.IsNullOrEmpty())
                table.AddCell(LeftCell(title, 8));
            var valueList = value.Split(',').ToList();
            foreach (var item in list)
            {
                if (valueList.Any(v => v == item))
                {
                    table.AddCell(BorderCell(true));
                    table.AddCell(LeftCell(item, 8));
                }
                else
                {
                    table.AddCell(BorderCell(false));
                    table.AddCell(LeftCell(item, 8));
                }
            }
            if (skipFirst)
            {
                //table.AddCell(BorderCell(false));
                table.AddCell(LeftCell(string.Empty, 8));
            }
            if (customValue.IsNullOrEmpty())
            {
                table.AddCell(BorderCell(false));
                table.AddCell(LeftCell("Other", 8));
            }
            else
            {
                table.AddCell(BorderCell(true));
                table.AddCell(LeftCell(string.Format("Other ({0})", customValue), 8, 4));
            }
        }
        public static void CheckBoxListMultipleValueOtherNewline(string value, PdfPTable table, List<string> list, string customValue)
        {
            var valueList = value.Split(',').ToList();
            foreach (var item in list)
            {
                if (valueList.Any(v => v == item))
                {
                    table.AddCell(BorderCell(true));
                    table.AddCell(LeftCell(item, 8));
                }
                else
                {
                    table.AddCell(BorderCell(false));
                    table.AddCell(LeftCell(item, 8));
                }
            }


            table.AddCell(BorderCell(!customValue.IsNullOrEmpty()));
            table.AddCell(LeftCell("Other", 8));
            Phrase sampleEnvelopeOtherPh = new Phrase(customValue == null ? "\n" : customValue, new Font(verdana, 10, Font.NORMAL, customValue.IsNullOrEmpty() ? BaseColor.BLACK : BaseColor.RED));
            PdfPCell sampleEnvelopeOther = new PdfPCell(sampleEnvelopeOtherPh);
            sampleEnvelopeOther.Colspan = 8;
            sampleEnvelopeOther.HorizontalAlignment = Element.ALIGN_LEFT;
            sampleEnvelopeOther.VerticalAlignment = Element.ALIGN_MIDDLE;
            sampleEnvelopeOther.Border = Rectangle.NO_BORDER;
            sampleEnvelopeOther.BorderWidthBottom = 1f;
            sampleEnvelopeOther.PaddingLeft = Utilities.InchesToPoints(0.1f);
            sampleEnvelopeOther.UseAscender = true;
            sampleEnvelopeOther.ExtraParagraphSpace = 2;
            table.AddCell(sampleEnvelopeOther);
        }
        public static void CheckBoxListSectionC(string value, PdfPTable table, List<string> list, int colspan, bool isother = true)
        {
            bool isFound = false;
            int cnt = 0;
            var listCount = list.Count;
            foreach (var item in list)
            {
                if (item != "")
                {
                    if (value.Contains(item))
                    {
                        table.AddCell(BorderCell(true));
                        table.AddCell(LeftCell(item, 8));
                        isFound = true;
                    }
                    else
                    {
                        table.AddCell(BorderCell(false));
                        table.AddCell(LeftCell(item, 8));
                    }
                }
                cnt++;
                if (colspan != 0)
                {
                    if (listCount % 2 == 0 && listCount / 2 == cnt)
                    {
                        table.AddCell(ColspanCell(colspan));
                    }

                }
            }
            if (isother)
            {
                if (isFound)
                {
                    table.AddCell(BorderCell(false));
                    table.AddCell(LeftCell("Other", 8));
                }
                else
                {
                    table.AddCell(BorderCell(true));
                    table.AddCell(LeftCell(string.Format("Other ({0})", value), 8));
                }
            }
        }
        public static PdfPCell ColspanCell(int colspan)
        {
            return new PdfPCell(new Phrase(" "))
            {
                Border = PdfPCell.NO_BORDER,
                Colspan = colspan,
                FixedHeight = 8
            };
        }
        public static PdfPCell SectionCell(string text)
        {
            Phrase sectionPh = new Phrase(text, new Font(verdana, 10));
            sectionPh.Leading = 5;
            PdfPCell section = new PdfPCell(sectionPh);
            section.BackgroundColor = new BaseColor(189, 215, 238);
            section.HorizontalAlignment = Element.ALIGN_LEFT;
            section.VerticalAlignment = Element.ALIGN_MIDDLE;
            section.Border = Rectangle.NO_BORDER;
            section.UseAscender = true;
            section.ExtraParagraphSpace = 2;
            return section;
        }
        public static void YesNoCheckBox(string title, PdfPTable table, bool isCheck)
        {
            Phrase cellText = new Phrase(title, new Font(verdana, 8));
            PdfPCell cell = new PdfPCell(cellText);
            cell.Colspan = 4;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Border = Rectangle.NO_BORDER;
            cell.PaddingRight = Utilities.InchesToPoints(0);
            cell.UseAscender = true;
            cell.ExtraParagraphSpace = 2;
            table.AddCell(cell);
            table.AddCell(BorderCell(isCheck));
            table.AddCell(LeftCell("Yes", 8));
            table.AddCell(BorderCell(!isCheck));
            table.AddCell(LeftCell("No", 8));
        }
        public static PdfPCell BorderCell(bool isSelected)
        {
            string x = isSelected ? "x" : "";
            Phrase borderPh = new Phrase(x, new Font(verdana, 10, Font.NORMAL, BaseColor.RED));
            PdfPCell borderCell = new PdfPCell(borderPh);
            borderCell.FixedHeight = 20;
            borderCell.SpaceCharRatio = 2;
            borderCell.HorizontalAlignment = Element.ALIGN_CENTER;
            borderCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            borderCell.BorderWidth = 1;
            borderCell.UseAscender = true;
            return borderCell;
        }
        public static PdfPCell LeftCell(string key, int font = 10, int otherColspan = 0)
        {
            Phrase leftPh = new Phrase(key, new Font(verdana, font));
            PdfPCell left = new PdfPCell(leftPh);
            left.Colspan = otherColspan;
            left.HorizontalAlignment = Element.ALIGN_LEFT;
            left.VerticalAlignment = Element.ALIGN_MIDDLE;
            left.Border = Rectangle.NO_BORDER;
            left.PaddingRight = Utilities.InchesToPoints(0);
            left.UseAscender = true;
            left.ExtraParagraphSpace = 2;
            return left;
        }
        public static PdfPCell RightCell(string value, int border = Rectangle.BOTTOM_BORDER, int colspan = 0)
        {
            Phrase rightPh = new Phrase(value, new Font(verdana, 10, Font.NORMAL, BaseColor.RED));
            PdfPCell right = new PdfPCell(rightPh);
            right.Colspan = colspan;
            right.HorizontalAlignment = Element.ALIGN_LEFT;
            right.VerticalAlignment = Element.ALIGN_MIDDLE;
            right.Border = border;
            if (border == Rectangle.BOTTOM_BORDER)
                right.BorderWidthBottom = 1f;
            right.PaddingLeft = Utilities.InchesToPoints(border == Rectangle.BOTTOM_BORDER ? 0 : 0.1f);
            right.UseAscender = true;
            right.ExtraParagraphSpace = 2;
            return right;
        }
        public static void MailMerge(string templatePath, Dictionary<string, string> data, string outputFilePath)
        {
            using (var existingFileStream = new FileStream(templatePath, FileMode.Open))
            using (var newFileStream = new FileStream(outputFilePath, FileMode.Create))
            {
                // Open existing PDF
                var pdfReader = new PdfReader(existingFileStream);

                // PdfStamper, which will create
                var stamper = new PdfStamper(pdfReader, newFileStream);

                var form = stamper.AcroFields;
                foreach (var model in data)
                {
                    //IList<AcroFields.FieldPosition> fieldPositions = form.GetFieldPositions(model.Key);
                    //if (fieldPositions == null || fieldPositions.Count <= 0) throw new ApplicationException("Error locating field");
                    //AcroFields.FieldPosition fieldPosition = fieldPositions[0];
                    //fieldPosition.position.Left = Utilities.InchesToPoints(5.88f);
                    form.SetField(model.Key, model.Value);

                }

                // "Flatten" the form so it wont be editable/usable anymore
                stamper.FormFlattening = true;

                // You can also specify fields to be flattened, which
                // leaves the rest of the form still be editable/usable
                // stamper.PartialFormFlattening("Address");

                stamper.Close();
                pdfReader.Close();
            }
        }
        public static TextField CreateFieldInSingleLine(PdfWriter writer1, string fieldName, BaseFont fontType, float fontSize, int aligment = Element.ALIGN_LEFT)
        {
            var tf = new TextField(writer1, new Rectangle(500, 300), fieldName)
            {
                Alignment = aligment,
                Visibility = TextField.VISIBLE,
                FieldName = fieldName,
                BackgroundColor = BaseColor.WHITE,
                Font = fontType,
                FontSize = fontSize,
                TextColor = BaseColor.BLACK,
                Options = TextField.READ_ONLY,
                // The rotation only affects the text in the field, not the field location
                Text = " "

            };
            //tf.SetExtraMargin(Utilities.InchesToPoints(3.12f), Utilities.InchesToPoints(1.88f));


            return tf;
        }
        public static TextField CreateField(PdfWriter writer1, string fieldName, BaseFont fontType, float fontSize, int aligment = Element.ALIGN_LEFT)
        {
            var tf = new TextField(writer1, new Rectangle(500, 300), fieldName)
            {
                Alignment = aligment,
                Visibility = TextField.VISIBLE,
                FieldName = fieldName,
                BackgroundColor = BaseColor.WHITE,
                Font = fontType,
                FontSize = fontSize,
                TextColor = BaseColor.BLACK,
                Options = TextField.MULTILINE,
                // The rotation only affects the text in the field, not the field location
                Text = " "

            };
            //tf.SetExtraMargin(Utilities.InchesToPoints(3.12f), Utilities.InchesToPoints(1.88f));


            return tf;
        }
        public static TextField CreateFieldLandscape(PdfWriter writer1, string fieldName, BaseFont fontType, float fontSize, int aligment = Element.ALIGN_LEFT)
        {
            var tf = new TextField(writer1, new Rectangle(500, 300), fieldName)
            {
                Alignment = aligment,
                Rotation = 90,
                Visibility = TextField.VISIBLE,
                FieldName = fieldName,
                BackgroundColor = BaseColor.WHITE,
                Font = fontType,
                FontSize = fontSize,
                TextColor = BaseColor.BLACK,
                Options = TextField.MULTILINE,
                // The rotation only affects the text in the field, not the field location
                Text = " "
            };
            //tf.SetExtraMargin(Utilities.InchesToPoints(3.12f), Utilities.InchesToPoints(1.88f));


            return tf;
        }
        public static TextField CreateField(PdfWriter writer1, string fieldName, BaseFont fontType, float fontSize, string data, int aligment = Element.ALIGN_LEFT)
        {
            var tf = new TextField(writer1, new Rectangle(500, 300), fieldName)
            {
                Alignment = aligment,
                Visibility = TextField.VISIBLE,
                FieldName = fieldName,
                BackgroundColor = BaseColor.WHITE,
                Font = fontType,
                FontSize = fontSize,
                TextColor = BaseColor.BLACK,
                Options = TextField.MULTILINE,
                // The rotation only affects the text in the field, not the field location
                Text = data
            };

            return tf;
        }


    }
}