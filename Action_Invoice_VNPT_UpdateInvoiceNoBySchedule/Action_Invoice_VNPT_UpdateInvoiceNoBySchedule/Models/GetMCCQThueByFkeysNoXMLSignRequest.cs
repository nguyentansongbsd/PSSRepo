using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Models
{
    [XmlRoot(ElementName = "GetMCCQThueByFkeysNoXMLSign", Namespace = "http://tempuri.org/")]
    public class GetMCCQThueByFkeysNoXMLSignRequest
    {
        [XmlElement(ElementName = "Account")]
        public string Account { get; set; }

        [XmlElement(ElementName = "ACpass")]
        public string ACpass { get; set; }

        [XmlElement(ElementName = "username")]
        public string Username { get; set; }

        [XmlElement(ElementName = "password")]
        public string Password { get; set; }

        [XmlElement(ElementName = "pattern")]
        public string Pattern { get; set; }

        [XmlElement(ElementName = "fkeys")]
        public string Fkeys { get; set; }
    }
}
