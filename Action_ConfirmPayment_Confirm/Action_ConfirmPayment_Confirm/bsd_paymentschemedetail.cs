using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Action_ConfirmPayment_Confirm
{
    public class bsd_paymentschemedetail
    {
        public Guid id { get; set; }
        public string localname { get; set; }
        public Entity ent { get; set; }
        public bsd_paymentschemedetail()
        {
            this.defaultValue();
        }

        private void defaultValue()
        {
            this.id = new Guid();
            this.ent = new Entity();
            this.localname = string.Empty;
        }

        public info_Error checkValue_Field(Entity input, string colunmname)
        {
            info_Error info = new info_Error();
            #region --- Check data input ---
            if (colunmname == string.Empty || input == null)
            {
                info.createMessageAdd(string.Format("Please, check data input!"));
                info.result = false;
                return info;
            }
            #endregion
            try
            {
                if (input.Contains(colunmname))
                {
                    info.result = true;
                    info.createMessageAdd(string.Format("Colunm {0} has value", colunmname));
                }
                else
                {
                    info.result = false;
                    info.createMessageAdd(string.Format("Colunm {0} doesn't have value", colunmname));
                }
            }
            catch (Exception ex)
            {
                info.count = -1;
                info.result = false;
                info.createMessageAdd(string.Format("Lỗi index = {0}, message = {1}, exception sys = {2}", info.index, info.message, ex.Message));
            }
            return info;
        }
    }
}
