using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Action_ApplyDocument
{
    public class bsd_paymentschemedetail
    {
        public Guid id { get; set; }

        public string localname { get; set; }

        public Entity ent { get; set; }

        public bsd_paymentschemedetail(string _localname)
        {
            if (!(_localname != string.Empty))
                return;
            this.localname = _localname;
        }

        public bsd_paymentschemedetail() => this.defaultValue();

        private void defaultValue()
        {
            this.id = new Guid();
            this.ent = new Entity();
            this.localname = string.Empty;
        }

        public info_Error getByID(IOrganizationService service, Guid ent_id)
        {
            info_Error byId = new info_Error();
            try
            {
                QueryExpression query = new QueryExpression(nameof(bsd_paymentschemedetail));
                query.ColumnSet = new ColumnSet(true);
                query.TopCount = new int?(1);
                FilterExpression filterExpression = new FilterExpression(LogicalOperator.And);
                filterExpression.AddCondition(new ConditionExpression("bsd_paymentschemedetailid", ConditionOperator.Equal, (object)ent_id));
                query.Criteria = filterExpression;
                EntityCollection entityCollection = service.RetrieveMultiple((QueryBase)query);
                if (entityCollection.Entities.Any<Entity>())
                {
                    byId.result = true;
                    byId.count = entityCollection.Entities.Count;
                    byId.entc = entityCollection;
                    byId.ent_First = entityCollection.Entities[0];
                    byId.createMessageAdd(string.Format("Done. Total record = {0}", (object)byId.count));
                }
                else
                {
                    byId.result = false;
                    byId.createMessageAdd(string.Format("Done, not data. Total record = 0."));
                }
            }
            catch (Exception ex)
            {
                byId.result = false;
                throw new Exception(string.Format("#.Error sys {0}", (object)ex.Message));
            }
            return byId;
        }

        public info_Error checkValue_Field(Entity input, string colunmname)
        {
            info_Error infoError = new info_Error();
            if (colunmname == string.Empty || input == null)
            {
                infoError.createMessageAdd(string.Format("Please, check data input!"));
                infoError.result = false;
                return infoError;
            }
            try
            {
                if (input.Contains(colunmname))
                {
                    infoError.result = true;
                    infoError.createMessageAdd(string.Format("Colunm {0} has value", (object)colunmname));
                }
                else
                {
                    infoError.result = false;
                    infoError.createMessageAdd(string.Format("Colunm {0} doesn't have value", (object)colunmname));
                }
            }
            catch (Exception ex)
            {
                infoError.count = -1;
                infoError.result = false;
                infoError.createMessageAdd(string.Format("Lỗi index = {0}, message = {1}, exception sys = {2}", (object)infoError.index, (object)infoError.message, (object)ex.Message));
            }
            return infoError;
        }
    }
}
