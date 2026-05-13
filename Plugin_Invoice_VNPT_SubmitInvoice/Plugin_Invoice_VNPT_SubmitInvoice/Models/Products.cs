using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Models
{
    public class Products
    {
        [XmlElement("Product")]
        public List<Product> Product { get; set; }
    }
}
