using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Models
{
    [XmlRoot("Invoices")]
    public class Invoices
    {
        [XmlElement("Inv")]
        public List<InvoiceWrapper> Inv { get; set; }
    }
}
