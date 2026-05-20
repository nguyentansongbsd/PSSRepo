using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Helpers
{
    public class ConvertBase64Helper
    {
        public static string ConvertBase64ToString(string base64String)
        {
            try
            {
                byte[] data = Convert.FromBase64String(base64String);
                return Encoding.UTF8.GetString(data);
            }
            catch (FormatException ex)
            {
                return null;
            }
        }
    }
}
