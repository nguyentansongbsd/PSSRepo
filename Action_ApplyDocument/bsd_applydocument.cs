using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Action_ApplyDocument
{
    public class bsd_applydocument
    {
        public Guid id { get; set; }

        public string localname { get; set; }

        public Entity ent { get; set; }

        public bsd_applydocument(string _localname)
        {
            if (!(_localname != string.Empty))
                return;
            this.localname = _localname;
        }

        public bsd_applydocument() => this.defaultValue();

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
                QueryExpression query = new QueryExpression(nameof(bsd_applydocument));
                query.ColumnSet = new ColumnSet(true);
                query.TopCount = new int?(1);
                FilterExpression filterExpression = new FilterExpression(LogicalOperator.And);
                filterExpression.AddCondition(new ConditionExpression("bsd_applydocumentid", ConditionOperator.Equal, (object)ent_id));
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
                    infoError.count = 1;
                    infoError.createMessageAdd("Done, New clear list data old");
                    infoError.clearMessage();
                    infoError.createMessageNew(string.Format("{0}", (object)input[colunmname].ToString()));
                }
                else
                {
                    infoError.result = true;
                    infoError.count = 0;
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

        public info_Error get_InstallmentList(IOrganizationService service, string str_arrayid)
        {
            info_Error installmentList = new info_Error();
            if (str_arrayid == string.Empty || str_arrayid.Trim().Length < 1)
            {
                installmentList.createMessageAdd("Input is not data. Break");
                installmentList.result = true;
                return installmentList;
            }
            try
            {
                string[] strArray = str_arrayid.Split(',');
                List<Guid> guidList = new List<Guid>();
                EntityCollection entityCollection = new EntityCollection();
                installmentList.index = 1;
                installmentList.createMessageAdd(string.Format("Step 1. Length array = {0}", (object)strArray.Length));
                if (strArray.Length < 1)
                {
                    installmentList.createMessageAdd("Not value array. Break");
                    installmentList.result = true;
                    return installmentList;
                }
                for (int index = 0; index < strArray.Length; ++index)
                {
                    Guid guid = new Guid(strArray[index]);
                    guidList.Add(guid);
                }
                installmentList.index = 2;
                installmentList.createMessageAdd(string.Format("Step 2. Count list guid = {0}", (object)strArray.Length));
                bsd_paymentschemedetail paymentschemedetail = new bsd_paymentschemedetail();
                int num = 0;
                foreach (Guid ent_id in guidList)
                {
                    ++num;
                    installmentList.createMessageAdd(string.Format("Step = {0}", (object)num));
                    info_Error byId = paymentschemedetail.getByID(service, ent_id);
                    if (!byId.result)
                    {
                        installmentList.result = false;
                        installmentList.createMessageAdd(string.Format("Get record [bsd_paymentschemedetail] by id = {0}", (object)ent_id));
                        installmentList.createMessageAdd(string.Format("Fails, {0}", (object)byId.message));
                        return installmentList;
                    }
                    entityCollection.Entities.Add(byId.ent_First);
                }
                if (guidList.Count == entityCollection.Entities.Count)
                {
                    installmentList.result = true;
                    installmentList.count = entityCollection.Entities.Count;
                    installmentList.entc = entityCollection;
                    installmentList.ent_First = entityCollection.Entities[0];
                    installmentList.createMessageNew("Done");
                }
                else
                {
                    installmentList.index = 3;
                    installmentList.createMessageAdd("Fails, The list has not been processed yet");
                    installmentList.result = false;
                }
            }
            catch (Exception ex)
            {
                installmentList.result = false;
                installmentList.createMessageAdd(string.Format("Lỗi. Index = {0}, message = {1}, exception sys = {2}", (object)installmentList.index, (object)installmentList.message, (object)ex.Message));
            }
            return installmentList;
        }

        public void check_DueDate_Installment(IOrganizationService service, Entity target)
        {
            try
            {
                info_Error byId = this.getByID(service, target.Id);
                info_Error infoError1 = byId.result ? this.checkValue_Field(byId.ent_First, "bsd_arraypsdid") : throw new Exception(byId.message);
                if (!infoError1.result)
                    throw new Exception(infoError1.message);
                if (infoError1.count != 1)
                    return;
                string message = infoError1.message;
                info_Error installmentList = this.get_InstallmentList(service, message);
                if (!installmentList.result)
                    throw new Exception(infoError1.message);
                if (installmentList.count > 0)
                {
                    foreach (Entity entity in (Collection<Entity>)installmentList.entc.Entities)
                    {
                        if (entity != null)
                        {
                            info_Error infoError2 = new bsd_paymentschemedetail().checkValue_Field(entity, "bsd_duedate");
                            if (infoError2.count != 0)
                                throw new Exception("Please, check target bsd_paymentschemedetail");
                            if (!infoError2.result)
                                throw new Exception("The Installment you are paying has no due date. Please update due date before confirming payment! " + entity["bsd_name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(string.Format("{0}", (object)ex.Message));
            }
        }
    }
}
