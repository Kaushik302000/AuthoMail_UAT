extern alias bc;
using bc::Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Lib
{
    public class LiteralData
    {
        public string FileName { get; set; }
        public int Format { get; set; }
        //public Stream Stream { get; set; }
        private PgpLiteralData _pgpLiteralData;

        internal LiteralData(PgpLiteralData pgpLiterlData)
        {
            FileName = pgpLiterlData.FileName;
            Format = pgpLiterlData.Format;
            _pgpLiteralData = pgpLiterlData;
        }

        public async Task SaveAsFile(string filePath = null)
        {
            var destFile = filePath ?? FileName;

            using (var os = File.Open(destFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                await _pgpLiteralData.GetInputStream().CopyToAsync(os);
            }

            //using (FileStream outputFileStream = new FileStream(destFile, FileMode.Create))
            //{
            //    outputFileStream.Position = 0;
            //    await _pgpLiteralData.GetInputStream().CopyToAsync(outputFileStream);
            //}
        }

        //public string GetAsString()
        //{
        //    using (var reader = new StreamReader(Stream, Encoding.UTF8))
        //    {
        //        return reader.ReadToEnd();
        //    }
        //}
    }
}
