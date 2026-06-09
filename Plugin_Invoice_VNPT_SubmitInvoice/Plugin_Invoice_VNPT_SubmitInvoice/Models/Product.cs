using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Models
{
    public class Product
    {
        public string Code { get; set; }

        public string ProdName { get; set; }

        public string ProdUnit { get; set; }

        public string ProdQuantity { get; set; }

        public string ProdPrice { get; set; }

        public string Discount { get; set; }

        public string DiscountAmount { get; set; }

        public string Total { get; set; }

        public string Amount { get; set; }

        public string VATRate { get; set; }

        public string VATAmount { get; set; }

        public int IsSum { get; set; }
    }
}
