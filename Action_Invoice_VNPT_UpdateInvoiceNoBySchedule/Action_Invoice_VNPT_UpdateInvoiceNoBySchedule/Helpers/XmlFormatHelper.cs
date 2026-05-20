using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Helpers
{
    public class XmlFormatHelper
    {
        public static string FormatXml<T>(
            T data,
            XmlSerializerNamespaces namespaces = null) where T : class
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));

                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = false
                };

                using (var sw = new StringWriter())
                using (var writer = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(writer, data, namespaces);

                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error formatting XML: " + ex.Message);
                return string.Empty;
            }
        }
    }
}
