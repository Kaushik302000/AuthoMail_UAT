using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Lib
{
    public static class IOHelper
    {
        public static async Task<Stream> GetASCIIStreamAsync(string stringData)
        {
            MemoryStream ms = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(ms, Encoding.ASCII, 512, true))
            {
                await writer.WriteAsync(stringData);
                await writer.FlushAsync();
                ms.Position = 0;
            }
            return ms;
        }
        public static Stream GetUTF8Stream(string stringData)
        {
            MemoryStream ms = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(ms, Encoding.UTF8, 512, true))
            {
                writer.Write(stringData);
                writer.Flush();
                ms.Position = 0;
            }
            return ms;
        }

        public static string GetString(Stream inputStream)
        {
            string output;
            using (StreamReader reader = new StreamReader(inputStream, System.Text.Encoding.UTF8))
            {
                output = reader.ReadToEnd();
            }
            return output;
        }

        public static void WriteStream(Stream inputStream, ref byte[] dataBytes)
        {
            using (Stream outputStream = inputStream)
            {
                outputStream.Write(dataBytes, 0, dataBytes.Length);
            }
        }
    }
}
