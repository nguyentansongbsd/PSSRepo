using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Models
{
    public class Invoice
    {
        public string ArisingDate { get; set; }

        public string CusCode { get; set; }

        public string CusName { get; set; }

        public string Buyer { get; set; }

        public string CusAddress { get; set; }

        public string CusPhone { get; set; }

        public string CusTaxCode { get; set; }

        public string CusBankNo { get; set; }

        public string CusBankName { get; set; }

        public string CurrencyUnit { get; set; }

        public string ExchangeRate { get; set; }

        public string PaymentStatus { get; set; }

        public string PaymentMethod { get; set; }

        public string Total { get; set; }

        public string DiscountAmount { get; set; }

        public string VATRate { get; set; }

        public string VATAmount { get; set; }

        public string Amount { get; set; }

        public string AmountInWords { get; set; }

        public string EmailDeliver { get; set; }

        public string ComAddress { get; set; }

        public string ComPhone { get; set; }

        public string ComBankNo { get; set; }

        public string ComBankName { get; set; }

        public string GrossValue { get; set; }

        public string GrossValue_NonTax { get; set; }

        public string GrossValue0 { get; set; }
               
        public string VatAmount0 { get; set; }
               
        public string GrossValue5 { get; set; }
               
        public string VatAmount5 { get; set; }
               
        public string GrossValue10 { get; set; }
               
        public string VatAmount10 { get; set; }
               
        public string GrossValue8 { get; set; }
               
        public string VatAmount8 { get; set; }

        public string SHChieu { get; set; }

        public string CCCDan { get; set; }

        public string Extra2 { get; set; }

        public Products Products { get; set; }
    }
}
