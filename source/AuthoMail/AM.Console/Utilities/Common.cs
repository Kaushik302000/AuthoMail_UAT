using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;
using AM.Entities;
using AM.Lib;
using AM.Console.Properties;
using AM.Services;

namespace AM.Console
{    
    public class MailPieceValidationResult
    {
        public string Code { get; set; }
        public string ErrorMessage { get; set; }
    }
}
