using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin_Create_UpdateDueDateDetail
{
    public class Plugin_Create_UpdateDueDateDetail : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        string enHD_name = "";
        string enIntalments_fieldNameHD = "";
        Entity enHD=new Entity();
        Entity enMaster = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            //get entity
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;
            en = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            string enDetailid = entity.Id.ToString();
            en = service.Retrieve("bsd_updateduedatedetail", new Guid(enDetailid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var item = en;
            

            var status = ((OptionSetValue)en["statuscode"]).Value;
            tracingService.Trace(status.ToString());
            if (status != 1&& status != 100000003) return;
            tracingService.Trace("start :" + status);
            tracingService.Trace("enDetailid :" + enDetailid);
            //check status
            var result = true;
            try
            {
                tracingService.Trace("start foreach");
                #region check giao dịch
                if (item.Contains("bsd_optionentry"))
                {
                    enIntalments_fieldNameHD = "bsd_optionentry";
                    enHD_name = "salesorder";
                }
                else
                {
                    if (item.Contains("bsd_quote"))
                    {
                        enIntalments_fieldNameHD = "bsd_reservation";
                        enHD_name = "quote";

                    }
                    else
                    {
                        if (item.Contains("bsd_quotation"))
                        {

                            enIntalments_fieldNameHD = "bsd_quotation";
                            enHD_name = "bsd_quotation";

                        }
                        else
                        {
                            HandleError(item, "not found entity!");
                        }

                    }

                }
                #endregion
                if (en.Contains("bsd_installment") == false)
                {
                    var query = new QueryExpression("bsd_paymentschemedetail");
                    query.ColumnSet.AllColumns = true;
                    query.Criteria.AddCondition("bsd_ordernumber", ConditionOperator.Equal, en["bsd_installmentnumber"].ToString());
                    query.Criteria.AddCondition(enIntalments_fieldNameHD, ConditionOperator.Equal, (en.Contains("bsd_quote")? ((EntityReference)en["bsd_quote"]).Id.ToString(): ((EntityReference)en["bsd_optionentry"]).Id.ToString()));
                    tracingService.Trace("@@!");

                    var rs_ = service.RetrieveMultiple(query);
                    if (rs_.Entities.Count > 0)
                    {
                        en["bsd_installment"] = new EntityReference("bsd_paymentschemedetail", rs_.Entities[0].Id);
                        item["bsd_installment"] = en["bsd_installment"];
                        var enTempUpdate = new Entity("bsd_updateduedatedetail", en.Id);

                        var enHDref =(EntityReference) rs_.Entities[0][enIntalments_fieldNameHD];
                        var endHD = service.Retrieve(enHDref.LogicalName, enHDref.Id, new ColumnSet(true));
                        var enUnitRef = enHD_name== "salesorder"? (EntityReference)endHD["bsd_unitnumber"] : (EntityReference)endHD["bsd_unitno"];
                        
                        enTempUpdate["bsd_units"] = enUnitRef;
                        enTempUpdate["bsd_duedateold"] =((DateTime) rs_.Entities[0]["bsd_duedate"]).AddHours(7);
                        enTempUpdate["bsd_installment"] = new EntityReference("bsd_paymentschemedetail", rs_.Entities[0].Id);
                        service.Update(enTempUpdate);
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("order number - InvalidPluginExecutionException ");
                    }
                }
                var enInstallmentRef = (EntityReference)item["bsd_installment"];
                var enInstallment = service.Retrieve(enInstallmentRef.LogicalName, enInstallmentRef.Id, new ColumnSet(true));
                tracingService.Trace("@@");
                var enHDRef = (EntityReference)enInstallment[enIntalments_fieldNameHD];
                 enHD = service.Retrieve(enHDRef.LogicalName, enHDRef.Id, new ColumnSet(true));
                tracingService.Trace("@@@");

                tracingService.Trace("CheckExistParentInDetail");
                CheckExistParentInDetail(ref result, item);
                tracingService.Trace("CheckIsLast");
                if (!result) return;

                CheckIsLast(ref result, item, enInstallment);
                if (!result) return;
                tracingService.Trace("CheckHD");

                CheckHD(ref result, item, enHD);
                tracingService.Trace("CheckPaidDetail");
                if (!result) return;

                CheckPaidDetail(ref result, item, enInstallment);
                //tracingService.Trace("CheckDueDate");
                //if (!result) return;
                //CheckDueDate(ref result, item, enInstallment, enHD);
                tracingService.Trace("CheckNewDate");
                if (!result) return;

                CheckNewDate(ref result, item, enInstallment);
                tracingService.Trace("Approve");
                if (!result) return;

            }
            catch (Exception ex)
            {
                HandleError(item, ex.Message);
            }
        }
        /// <summary>
        ///  Dự án ở entity detail có trùng với entity Cha không?
        /// </summary>
        public void CheckExistParentInDetail(ref bool result, Entity item)
        {
            var enMasterRef = (EntityReference)item["bsd_updateduedate"];

            enMaster = service.Retrieve("bsd_updateduedate", enMasterRef.Id, new ColumnSet(true));
            if (((EntityReference)enMaster["bsd_project"]).Id != ((EntityReference)item["bsd_project"]).Id)
            {
                var mess = "The project in the Master and Detail entities does not match. Please check again.";
                HandleError(item, mess);

                result = false;
            }
        }
        /// <summary>
        /// Last Installment [bsd_lastinstallment] = yes 
        ///hoặc Duedate Calculating Method[bsd_duedatecalculatingmethod] =
        ///Estimate handover date?
        /// </summary>
        public void CheckIsLast(ref bool result, Entity item, Entity enInstallment)
        {
            if (((bool)enInstallment["bsd_lastinstallment"]))
            {
                var mess = "This is last installment. Cannot perform this action.";
                HandleError(item, mess);
                result = false;
            }
            else if ((enInstallment.Contains("bsd_duedatecalculatingmethod") && ((OptionSetValue)enInstallment["bsd_duedatecalculatingmethod"]).Value == 100000002))
            {

                var mess = "This is estimate handover date. Cannot perform this action.";
                HandleError(item, mess);
                result = false;

            }
        }
        /// <summary>
        ///  Từ đợt trong entity chi tiết kiểm tra các Hợp đồng có trạng thái =Terminated
        /// </summary>
        public void CheckHD(ref bool result, Entity item, Entity enHD)
        {
            switch (enHD_name)
            {
                case "bsd_quotation":
                    if (((OptionSetValue)enHD["statuscode"]).Value == 100000001)
                    {
                        var mess = "The record contains a contract that has already been liquidated. Please check again.";
                        HandleError(item, mess);
                        result = false;
                    }
                    break;
                case "salesorder":
                    if (((OptionSetValue)enHD["statuscode"]).Value == 100000006)
                    {
                        var mess = "The record contains a contract that has already been liquidated. Please check again.";
                        HandleError(item, mess);

                        result = false;
                    }
                    break;
                case "quote":
                    if (((OptionSetValue)enHD["statuscode"]).Value == 100000001)
                    {
                        var mess = "The record contains a contract that has already been liquidated. Please check again.";
                        HandleError(item, mess);

                        result = false;
                    }
                    break;
            }
        }
        /// <summary>
        /// kiểm tra đợt trong entity detail được thanh toán không? 
        /// (Kiểm tra 2 field: Amount Was Paid [bsd_amountwaspaid] ; 
        /// Deposit Amount Paid [bsd_depositamount] khác 0)
        /// </summary>
        public void CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)
        {
            //bỏ điều kiện check này DevTu - 17.2.2025
            //if (((Money)enInstallment["bsd_depositamount"]).Value != 0 || ((Money)enInstallment["bsd_amountwaspaid"]).Value != 0)
            //{
            //    var mess = "There is a batch that has already been paid. Please check again.";
            //    HandleError(item, mess);

            //    result = false;
            //}
        }
        /// <summary>
        /// Kiểm tra ngày đến hạn mới trên entity detail (Due Date (New) [bsd_duedatenew]) có nhỏ hơn đợt phía sau?
        /// Kiểm tra ngày đến hạn mới trên entity detail (Due Date (New) [bsd_duedatenew]) có lớn hơn đợt phía trước?
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="enInstallment"></param>
        /// <param name="enHD"></param>
        public void CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)
        {

            var newDate = (DateTime)item["bsd_duedatenew"];

            var query = new QueryExpression(enInstallment.LogicalName);
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition(enIntalments_fieldNameHD, ConditionOperator.Equal, enHD.Id.ToString());
            query.Criteria.AddCondition("bsd_paymentschemedetailid", ConditionOperator.NotEqual, enInstallment.Id.ToString());
            var rs_ = service.RetrieveMultiple(query);
            tracingService.Trace($"enInstallment:{enInstallment.Id.ToString()}");
            foreach (var JItem in rs_.Entities)
            {
                tracingService.Trace($"JItem:{JItem.Id.ToString()}");

                if (JItem.Id != enInstallment.Id)
                {

                    if (JItem.Contains("bsd_duedate") == false) continue;
                    tracingService.Trace($"bsd_ordernumber:{(int)JItem["bsd_ordernumber"]}");
                    if (((int)JItem["bsd_ordernumber"]) < ((int)enInstallment["bsd_ordernumber"]))//lớn hơn đợt phí trước?
                    {
                        if ((newDate - (((DateTime)JItem["bsd_duedate"])).AddHours(7)).TotalDays <= 0)
                        {
                            var mess = "The new due date is earlier than the next batch.Please check again."; 
                            HandleError(item, mess);
                            result = false;
                            break;
                        }
                    }
                    if (((int)JItem["bsd_ordernumber"]) > ((int)enInstallment["bsd_ordernumber"]))//nhở hơn đợt phí sau?
                    {
                        if ((newDate - (((DateTime)JItem["bsd_duedate"])).AddHours(7)).TotalDays >= 0)
                        {
                            var mess = "The new due date is later than the previous batch. Please check again.";
                            HandleError(item, mess);
                            result = false;
                            break;

                        }
                    }

                    #region check thêm xem trong update duedate detail có đợt này cùng HD.

                    //var query2 = new QueryExpression("bsd_updateduedatedetail");
                    //query2.ColumnSet.AllColumns = true;
                    //query2.Criteria.AddCondition("bsd_paymentschemedetailid", ConditionOperator.NotEqual, enInstallment.Id.ToString());
                    //query2.Criteria.AddCondition("bsd_updateduedate", ConditionOperator.NotEqual, enMaster.Id.ToString());
                    //var rs2_ = service.RetrieveMultiple(query2);

                    #endregion
                }
            }
        }
        /// <summary>
        /// CNNDH 04.3
        ///Kiểm tra ngày mới có bằng ngày hiện tại
        ///Kiểm tra ngày đến hạn mới trên entity detail (Due Date (New) [bsd_duedatenew]) 
        ///trùng ngày đến hạn cũ Due Date (Old) [bsd_duedateold]
        /// </summary>
        public void CheckNewDate(ref bool result, Entity item, Entity enInstallment)
        {
            tracingService.Trace("item :" + item.Id.ToString());
            tracingService.Trace("enInstallment :" + enInstallment.Id.ToString());
            var newDate = (DateTime)item["bsd_duedatenew"];
            if (enInstallment.Contains("bsd_duedate") == false) return;
            tracingService.Trace($"oldDate {enInstallment["bsd_duedate"]}");

            tracingService.Trace($"{enInstallment.Id}");
            tracingService.Trace($"newDate {newDate}");
            if ((newDate - ((DateTime)enInstallment["bsd_duedate"]).AddHours(7)).TotalDays == 0)
            {


                var mess = "The new due date is the same as the old due date. Please check again.";
                HandleError(item, mess);
                result = false;
            }

        }
        public void HandleError(Entity item, string error)
        {
            tracingService.Trace(error);
            throw new InvalidPluginExecutionException(error);
        }
    }
}
