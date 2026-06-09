using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Models
{
    [XmlRoot(ElementName = "Envelope",
    Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class SoapEnvelope
    {
        [XmlElement(ElementName = "Body")]
        public SoapBody Body { get; set; }
    }
}
