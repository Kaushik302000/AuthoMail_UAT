using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class Helper
    {
        public static void MailMerge(string templatePath, Dictionary<string, string> data, ref byte[] output)
        {
            using (var existingFileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            using (MemoryStream ms = new MemoryStream())
            {
                // Open existing PDF
                var pdfReader = new PdfReader(existingFileStream);

                // PdfStamper, which will create
                var stamper = new PdfStamper(pdfReader, ms);

                var form = stamper.AcroFields;
                foreach (var model in data)
                {
                    //IList<AcroFields.FieldPosition> fieldPositions = form.GetFieldPositions(model.Key);
                    //if (fieldPositions == null || fieldPositions.Count <= 0) throw new ApplicationException("Error locating field");
                    //AcroFields.FieldPosition fieldPosition = fieldPositions[0];
                    //fieldPosition.position.Left = Utilities.InchesToPoints(5.88f);
                    form.SetFieldProperty(model.Key, "setmultiline", true, null);
                    form.SetField(model.Key, model.Value);

                }

                // "Flatten" the form so it wont be editable/usable anymore
                stamper.FormFlattening = true;

                // You can also specify fields to be flattened, which
                // leaves the rest of the form still be editable/usable
                // stamper.PartialFormFlattening("Address");

                stamper.Close();
                pdfReader.Close();
                output = ms.ToArray();
            }
        }
    }
}
