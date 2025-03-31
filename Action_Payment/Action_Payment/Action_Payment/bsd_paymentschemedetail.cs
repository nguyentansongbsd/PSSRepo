using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Action_Payment
{
    public class bsd_paymentschemedetail
    {
        public Guid id { get; set; }
        public string localname { get; set; }
        public Entity ent { get; set; }
        public bsd_paymentschemedetail(string _localname)
        {
            if (_localname != string.Empty) this.localname = _localname;
        }
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

        public info_Error getByID(IOrganizationService service, Guid ent_id)
        {
            info_Error info = new info_Error();
            try
            {
                QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
                query.ColumnSet = new ColumnSet(true);
                query.TopCount = 1;
                FilterExpression filter = new FilterExpression(LogicalOperator.And);
                filter.AddCondition(new ConditionExpression("bsd_paymentschemedetailid", ConditionOperator.Equal, ent_id));
                query.Criteria = filter;
                var rs = service.RetrieveMultiple(query);
                if (rs.Entities.Any())
                {
                    info.result = true;
                    info.count = rs.Entities.Count;
                    info.entc = rs;
                    info.ent_First = rs.Entities[0];
                    info.createMessageAdd(string.Format("Done. Total record = {0}", info.count));
                }
                else
                {
                    info.result = false;
                    info.createMessageAdd(string.Format("Done, not data. Total record = 0."));
                }
            }
            catch (Exception ex)
            {
                info.result = false;
                throw new Exception(string.Format("#.Error sys {0}", ex.Message));
            }
            return info;
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
