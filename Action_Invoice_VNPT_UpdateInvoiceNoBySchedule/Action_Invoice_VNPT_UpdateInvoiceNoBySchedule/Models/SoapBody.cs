using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Models
{
    public class SoapBody
    {
        [XmlElement(ElementName = "GetMCCQThueByFkeysNoXMLSign", Namespace = "http://tempuri.org/")]
        public GetMCCQThueByFkeysNoXMLSignRequest GetMCCQThueByFkeysNoXMLSign { get; set; }
    }
}
