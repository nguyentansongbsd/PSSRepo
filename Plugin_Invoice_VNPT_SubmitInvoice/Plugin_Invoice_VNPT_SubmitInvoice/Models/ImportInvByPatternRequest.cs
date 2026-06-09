using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Models
{
    [XmlRoot("ImportInvByPattern", Namespace = "http://tempuri.org/")]
    public class ImportInvByPatternRequest
    {
        public string Account { get; set; }

        public string ACpass { get; set; }

        [XmlIgnore]
        public string xmlInvData { get; set; }

        [XmlElement("xmlInvData")]
        public XmlCDataSection XmlInvDataCData
        {
            get
            {
                var doc = new XmlDocument();
                return doc.CreateCDataSection(xmlInvData);
            }
            set
            {
                xmlInvData = value?.Value;
            }
        }

        public string username { get; set; }

        public string password { get; set; }

        public string pattern { get; set; }

        public string serial { get; set; }

        public int convert { get; set; }
    }
}
