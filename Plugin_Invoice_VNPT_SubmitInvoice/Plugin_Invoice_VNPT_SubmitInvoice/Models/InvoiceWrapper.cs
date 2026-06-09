using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Models
{
    public class InvoiceWrapper
    {
        public string key { get; set; }

        public Invoice Invoice { get; set; }
    }
}
