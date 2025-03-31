using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Action_ApplyDocument
{
    public class bsd_applydocument
    {
        public Guid id { get; set; }
        public string localname { get; set; }
        public Entity ent { get; set; }
        public bsd_applydocument(string _localname)
        {
            if (_localname != string.Empty) this.localname = _localname;
        }
        public bsd_applydocument()
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
                QueryExpression query = new QueryExpression("bsd_applydocument");
                query.ColumnSet = new ColumnSet(true);
                query.TopCount = 1;
                FilterExpression filter = new FilterExpression(LogicalOperator.And);
                filter.AddCondition(new ConditionExpression("bsd_applydocumentid", ConditionOperator.Equal, ent_id));
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
                    info.count = 1;
                    info.createMessageAdd("Done, New clear list data old");
                    info.clearMessage();
                    info.createMessageNew(string.Format("{0}", input[colunmname].ToString()));
                }
                else
                {
                    info.result = true;
                    info.count = 0;
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

        /// <summary>
        /// Input value of field bsd_arraypsdid in form Apply Documents
        /// </summary>
        /// <param name="bsd_arraypsdid"></param>
        /// <returns></returns>
        public info_Error get_InstallmentList(IOrganizationService service, string str_arrayid)
        {
            info_Error info = new info_Error();
            #region --- Check input ---
            if (str_arrayid == string.Empty || str_arrayid.Trim().Length < 1)
            {
                info.createMessageAdd("Input is not data. Break");
                info.result = true;
                return info;
            }
            #endregion
            try
            {
                #region --- body code ---
                string[] arr = str_arrayid.Split(',');
                List<Guid> listguid = new List<Guid>();
                EntityCollection entc = new EntityCollection();

                info.index = 1;
                info.createMessageAdd(string.Format("Step 1. Length array = {0}", arr.Length));
                #region --- check Array ---
                if (arr.Length < 1)
                {
                    info.createMessageAdd("Not value array. Break");
                    info.result = true;
                    return info;
                }
                #endregion

                for (int i = 0; i < arr.Length; i++)
                {
                    var item = arr[i];
                    var guid = new Guid(item);
                    listguid.Add(guid);
                }

                info.index = 2;
                info.createMessageAdd(string.Format("Step 2. Count list guid = {0}", arr.Length));
                bsd_paymentschemedetail detail = new bsd_paymentschemedetail();
                int count = 0;
                foreach (var item in listguid)
                {
                    count += 1;
                    info.createMessageAdd(string.Format("Step = {0}", count));
                    var ent = detail.getByID(service, item);
                    if (!ent.result)
                    {
                        info.result = false;
                        info.createMessageAdd(string.Format("Get record [bsd_paymentschemedetail] by id = {0}", item));
                        info.createMessageAdd(string.Format("Fails, {0}", ent.message));
                        return info;
                    }
                    else
                    {
                        entc.Entities.Add(ent.ent_First);
                    }
                }

                #region --- #. Check result final ---
                if (listguid.Count == entc.Entities.Count)
                {
                    info.result = true;
                    info.count = entc.Entities.Count;
                    info.entc = entc;
                    info.ent_First = entc.Entities[0];
                    info.createMessageNew("Done");
                }
                else
                {
                    info.index = 3;
                    info.createMessageAdd("Fails, The list has not been processed yet");
                    info.result = false;
                }
                #endregion 
                #endregion
            }
            catch (Exception ex)
            {
                info.result = false;
                info.createMessageAdd(string.Format("Lỗi. Index = {0}, message = {1}, exception sys = {2}", info.index, info.message, ex.Message));
            }
            return info;
        }

        public void check_DueDate_Installment(IOrganizationService service, Entity target)
        {
            try
            {
                
                var applydocument = getByID(service, target.Id);
                if (!applydocument.result) throw new Exception(applydocument.message);
                var value = checkValue_Field(applydocument.ent_First, "bsd_arraypsdid");


                if (!value.result) throw new Exception(value.message);

                // Case . Click chọn Installment, chưa save trên js sẽ không thấy arrayID dưới db
                //else if (value.result && value.count != 1) throw new Exception(value.message); 

                if (value.count == 1)
                {
                    var str_array = value.message;
                    var list = get_InstallmentList(service, str_array);
                    if (!list.result) throw new Exception(value.message);
                    else if (list.count > 0)
                    {
                        var entc = list.entc;
                        foreach (var item in entc.Entities)
                        {
                            #region --- Kiểm tra duedate in Installment : Task jira CLV-1446 ---
                            if (item != null)
                            {
                                var isCheck = new bsd_paymentschemedetail().checkValue_Field(item, "bsd_duedate");
                                if (isCheck.count != 0)
                                {
                                    throw new Exception("Please, check target bsd_paymentschemedetail");
                                }
                                else if (!isCheck.result)
                                {
                                    throw new Exception("The Installment you are paying has no due date. Please update due date before confirming payment! " + item["bsd_name"].ToString());
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(string.Format("{0}", ex.Message));
            }
        }

    }
}
