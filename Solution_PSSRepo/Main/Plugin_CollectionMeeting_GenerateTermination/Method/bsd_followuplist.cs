using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugin_CollectionMeeting_GenerateTermination
{
    public class bsd_followuplist
    {
        public Entity ent { get; set; }
        public decimal TotalForfeitureAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Refund { get; set; }
        public enum bsd_takeoutmoney
        {
            Refund = 100000000,
            Forfeiture = 100000001
        }

        public bsd_followuplist()
        {
            this.defaultVal();
        }
        public bsd_followuplist(Entity _ent)
        {
            if (_ent != null) this.ent = _ent;
        }
        private void defaultVal()
        {
            this.ent = new Entity();
            this.TotalForfeitureAmount = 0m;
            this.TotalPaid = 0m;
            this.Refund = 0m;
        }

        public info_Error getByID(IOrganizationService service, Guid ent_id)
        {
            info_Error info = new info_Error();
            try
            {
                QueryExpression query = new QueryExpression("bsd_followuplist");
                query.ColumnSet = new ColumnSet(true);
                query.TopCount = 1;
                FilterExpression filter = new FilterExpression(LogicalOperator.And);
                filter.AddCondition(new ConditionExpression("bsd_followuplistid", ConditionOperator.Equal, ent_id));
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
    }
}
