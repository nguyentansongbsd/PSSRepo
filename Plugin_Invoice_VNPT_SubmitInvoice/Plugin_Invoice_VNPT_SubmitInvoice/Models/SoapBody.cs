using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Models
{
    public class SoapBody
    {
        [XmlElement("ImportInvByPattern",
            Namespace = "http://tempuri.org/")]
        public ImportInvByPatternRequest ImportInvByPattern { get; set; }
    }
}
