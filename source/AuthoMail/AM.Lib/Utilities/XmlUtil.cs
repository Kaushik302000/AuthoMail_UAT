using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AM.Lib
{
    public class ValidationResult
    {
        public bool HasErrors { get; set; }
        public string Errors { get; set; }
        public List<string> ErrorList { get; set; }
    }

    public static class XmlUtil
    {
        public static ValidationResult ValidateXml(XmlSchemaSet schemaSet, string xml)
        {
            ValidationResult vResult = new ValidationResult() { HasErrors = false, ErrorList = new List<string>() };

            XDocument xDoc = XDocument.Parse(xml);
            StringBuilder sbErrorMessage = new StringBuilder();

            xDoc.Validate(schemaSet, (o, e) =>
            {
                vResult.HasErrors = true;
                sbErrorMessage.AppendLine(e.Message);
                vResult.ErrorList.Add(e.Message);
            });

            vResult.Errors = sbErrorMessage.ToString();

            return vResult;
        }

        public static bool IsAttNullOrWhiteSpace(this XmlElement node, string attributeName)
        {
            if (node == null)
            {
                return true;
            }
            else if (!node.HasAttribute(attributeName))
            {
                return true;
            }
            else
            {
                return node.GetAttribute(attributeName).IsNullOrWhiteSpace();
            }
        }

        public static string GetAttributeValue(this XmlElement node, string attributeName)
        {
            return node.GetAttribute(attributeName).Trim();
        }

        public static T Deserialize<T>(string xml) where T : new()
        {
            T entity;
            var xmlSerializer = new XmlSerializer(typeof(T));

            //var attributes = typeof(T).GetCustomAttributes(typeof(SerializableAttribute), true);

            //if (attributes.Length == 0)
            //    throw new ArgumentException("T does not have attribute InsertableAttribute");

            using (var sr = new StringReader(xml))
            {
                entity = (T)xmlSerializer.Deserialize(sr);
                sr.Close();
            }

            return entity;
        }

        public static string Serialize<T>(T entity) where T : new()
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            {
                xmlSerializer.Serialize(sw, entity);
                sw.Close();
            }

            return sb.ToString();
        }

        public static string SerializeUTF8<T>(T entity) where T : new()
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, System.Text.Encoding.UTF8))
            {
                xmlSerializer.Serialize(sw, entity);
                sw.Close();
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }   

        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStringValue(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get field info for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the string value attributes
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }

        public static string ComputHash(string secretKey, string message)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretKey); //Ex: secretKey: 16424607L33Y43410780Z0H9HA956W64
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

    }

    /// <summary>
    /// This attribute is used to represent a string value
    /// for a value in an enum.
    /// </summary>
    public class StringValueAttribute : Attribute
    {

        #region Properties

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }

        #endregion

    }
}
