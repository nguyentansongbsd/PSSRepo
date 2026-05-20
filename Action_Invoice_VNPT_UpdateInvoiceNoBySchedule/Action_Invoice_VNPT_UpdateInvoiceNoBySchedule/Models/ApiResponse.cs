using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Models
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class SoapEnvelopeResponse
    {
        [XmlElement(ElementName = "Body")]
        public SoapBodyResponse Body { get; set; }
    }

    public class SoapBodyResponse
    {
        [XmlElement(ElementName = "GetMCCQThueByFkeysNoXMLSignResponse", Namespace = "http://tempuri.org/")]
        public GetMCCQThueByFkeysNoXMLSignResponse GetMCCQThueByFkeysNoXMLSignResponse { get; set; }
    }

    [XmlRoot(ElementName = "GetMCCQThueByFkeysNoXMLSignResponse", Namespace = "http://tempuri.org/")]
    public class GetMCCQThueByFkeysNoXMLSignResponse
    {
        [XmlElement(ElementName = "GetMCCQThueByFkeysNoXMLSignResult")]
        public string Result { get; set; }
    }
}
