using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Approved_Updateestimatehandoverdate_Detail
{
    public class Action_Approved_Updateestimatehandoverdate_Detail : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        string masterName = "bsd_updateestimatehandoverdate";
        string detailName = "bsd_updateestimatehandoverdatedetail";

        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            //get entity
            string enDetailid = context.InputParameters["id"].ToString();
            tracingService.Trace("enDetailid :" + enDetailid);

            en = service.Retrieve(detailName, new Guid(enDetailid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            EntityReference enMasterRef = (EntityReference)en["bsd_updateestimatehandoverdate"];
            Entity enMaster = service.Retrieve(masterName, enMasterRef.Id, new ColumnSet(true));
            
            Entity item = en;
            bool result = true;
            if (!CheckConditionRun(en))
            {
                return;
            }
            try
            {
                
                tracingService.Trace("CheckExistParentInDetail");
                CheckExistParentInDetail(ref result, item);
                if (!result) return;
                int bsd_types = ((OptionSetValue)enMaster["bsd_types"]).Value;
                EntityReference enUnitRef = (EntityReference)item["bsd_units"];
                Entity enUnit = service.Retrieve(enUnitRef.LogicalName, enUnitRef.Id, new ColumnSet(true));
                tracingService.Trace($"start {en.Id}");
                tracingService.Trace($"{bsd_types}");


                switch (bsd_types)
                {
                    case 100000000://Update Only for Units

                        tracingService.Trace($"UpdateEstimateHandoverDateFromDetailToUnit");
                        UpdateEstimateHandoverDateFromDetailToUnit(ref result, item, enUnit);
                        tracingService.Trace($"UpdateOPDateFromMasterToUnit");
                        UpdateOPDateFromMasterToUnit(ref result, enMaster, enUnit);
                        AprroveDetail(item);

                        break;
                    default: //khác Update Only for Units
                        if(!en.Contains("bsd_optionentry"))
                        {
                            HandleError(item, "The product does not have a contract. Please check again.");
                            return;
                        }    
                        EntityReference enHDRef = (EntityReference)en["bsd_optionentry"];
                        Entity enHD = service.Retrieve(enHDRef.LogicalName, enHDRef.Id, new ColumnSet(true));
                        EntityReference enInstallmentRef = (EntityReference)en["bsd_installment"];
                        Entity enInstallment = service.Retrieve(enInstallmentRef.LogicalName, enInstallmentRef.Id, new ColumnSet(true));
                        tracingService.Trace($"CheckExistParentInDetail");
                        CheckExistParentInDetail(ref result, item);
                        if (!result) return;
                        tracingService.Trace($"CheckStatusHD");
                        CheckStatusHD(ref result, item, enHD);
                        if (!result) return;
                        tracingService.Trace($"CheckPaid");
                        CheckPaid(ref result, item, enInstallment);
                        if (!result) return;
                        tracingService.Trace($"CheckDueDate");
                        CheckDueDate(ref result, item, enInstallment, enHD);
                        if (!result) return;
                        if (bsd_types == 100000001)// Update all
                        {
                            tracingService.Trace($"UpdateFromDetailToUnitToInstallmentToHD");
                            UpdateFromDetailToUnitToInstallmentToHD(ref result, item, enInstallment, enUnit, enHD);
                            UpdateOPDateFromMasterToUnit(ref result, enMaster, enUnit);
                        }
                        else
                        {
                            if (bsd_types == 100000002)
                            {
                                tracingService.Trace($"UpdateFromDetailToInstallment");
                                UpdateFromDetailToInstallment(ref result, item, enInstallment);
                                UpdateOPDateFromMasterToUnit(ref result, enMaster, enUnit);
                            }
                        }
                        AprroveDetail(item);

                        break;
                }


            }
            catch (Exception ex)
            {
                HandleError(item, ex.Message);
            }
        }
        public void AprroveDetail(Entity item)
        {
            Entity enDetailUpdate = new Entity(item.LogicalName, item.Id);
            enDetailUpdate["statuscode"] = new OptionSetValue(100000000);
            service.Update(enDetailUpdate);
        }
        /// <summary>
        ///  Dự án ở entity detail có trùng với entity Cha không?
        /// </summary>
        public void CheckExistParentInDetail(ref bool result, Entity item)
        {
            var enMasterRef = (EntityReference)item["bsd_updateestimatehandoverdate"];

            var enMaster = service.Retrieve(masterName, enMasterRef.Id, new ColumnSet(true));
            if (((EntityReference)enMaster["bsd_project"]).Id != ((EntityReference)item["bsd_project"]).Id)
            {
                var mess = "The project in the Detail entity is invalid. Please check again.";
                HandleError(item, mess);

                result = false;
            }
        }
        public bool CheckConditionRun(Entity item)
        {
            var enMasterRef = (EntityReference)item["bsd_updateestimatehandoverdate"];
            var enMaster = service.Retrieve(masterName, enMasterRef.Id, new ColumnSet(true));
            if ((bool)enMaster["bsd_error"] == true && (bool)enMaster["bsd_processing_pa"] == false)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        public void HandleError(Entity item, string error)
        {
            tracingService.Trace("error  :" + error);
            var enMasterRef = (EntityReference)item["bsd_updateestimatehandoverdate"];
            var enMaster = new Entity(masterName, enMasterRef.Id);
            enMaster["bsd_error"] = true;
            enMaster["bsd_errordetail"] = error;
            enMaster["bsd_processing_pa"] = false;
            enMaster["statuscode"] = new OptionSetValue(1);
            service.Update(enMaster);
        }
        /// <summary>
        /// Cập nhật field OP Date [bsd_opdate] trên entity Cha qua field OP Date [bsd_opdate] trêm entity Unit 
        /// </summary>
        public void UpdateOPDateFromMasterToUnit(ref bool result, Entity master, Entity enUnit)
        {
            Entity enUnitUpdate = new Entity(enUnit.LogicalName, enUnit.Id);
            if (!master.Contains("bsd_opdate")) return;
            enUnitUpdate["bsd_opdate"] = master["bsd_opdate"];
            service.Update(enUnitUpdate);
        }
        /// <summary>
        /// Cập nhật field Estimate Handover Date (New) [bsd_estimatehandoverdatenew] trên entity Con 
        /// qua field Estimate Handover Date [bsd_estimatehandoverdate] trêm entity Unit 
        /// </summary>
        public void UpdateEstimateHandoverDateFromDetailToUnit(ref bool result, Entity item, Entity enUnit)
        {
            Entity enUnitUpdate = new Entity(enUnit.LogicalName, enUnit.Id);
            enUnitUpdate["bsd_estimatehandoverdate"] = item["bsd_estimatehandoverdatenew"];
            service.Update(enUnitUpdate);
        }
        /// <summary>
        /// CNOP 06 Kiểm tra trạng thái hợp đồng đã bàn giao chưa
        /// </summary>
        public void CheckStatusHD(ref bool result, Entity item, Entity enHD)
        {
            if (((OptionSetValue)enHD["statuscode"]).Value == 100000005)
            {
                var mess = "The contract has already been handed over. Please check again.";
                HandleError(item, mess);

                result = false;
            }
            else
            {
                if (((OptionSetValue)enHD["statuscode"]).Value == 100000006)
                {
                    var mess = "The record contains a contract that has already been liquidated. Please check again.";
                    HandleError(item, mess);

                    result = false;
                }
            }
        }
        /// <summary>
        ///  (Kiểm tra cả 2 field: Amount Was Paid [bsd_amountwaspaid] hoặc Deposit Amount Paid [bsd_depositamount] khác 0)
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="enInstallment"></param>
        public void CheckPaid(ref bool result, Entity item, Entity enInstallment)
        {
            if (((Money)enInstallment["bsd_depositamount"]).Value != 0 || ((Money)enInstallment["bsd_amountwaspaid"]).Value != 0)
            {
                var mess = "There is a batch that has already been paid. Please check again.";
                HandleError(item, mess);

                result = false;
            }
        }
        /// <summary>
        /// Kiểm tra ngày đến hạn mới trên entity con (Payment due date [bsd_paymentduedate] có lớn hơn đợt phía trước?
        /// Kiểm tra ngày đến hạn mới trên entity con (Payment due date [bsd_paymentduedate] có nhỏ hơn đợt phía sau?
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="enInstallment"></param>
        /// <param name="enHD"></param>
        public void CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)
        {

            var newDate = (DateTime)item["bsd_paymentduedate"];

            var query = new QueryExpression(enInstallment.LogicalName);
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enHD.Id.ToString());
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
                    if (((int)JItem["bsd_ordernumber"]) < ((int)enInstallment["bsd_ordernumber"]))
                    {
                        if ((newDate - (((DateTime)JItem["bsd_duedate"])).AddHours(7)).TotalDays <= 0)
                        {
                            var mess = "The new due date is invalid. Please check again.";
                            HandleError(item, mess);

                            result = false;
                            break;
                        }
                    }
                    if (((int)JItem["bsd_ordernumber"]) > ((int)enInstallment["bsd_ordernumber"]))
                    {
                        if ((newDate - (((DateTime)JItem["bsd_duedate"])).AddHours(7)).TotalDays >= 0)
                        {
                            var mess = "The new due date is invalid. Please check again.";
                            HandleError(item, mess);

                            result = false;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cập nhật field Estimate Handover Date (New) [bsd_estimatehandoverdatenew] trên entity Con qua 2 field 
        ///-Estimate Handover Date[bsd_estimatehandoverdate] trêm entity Unit 
        ///-Estimate Handover Date(Contract) [bsd_estimatehandoverdatecontract]
        ///Cập nhật field Payment due date[bsd_paymentduedate] trên entity Con qua field Due Date của installment
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="enInstallment"></param>
        /// <param name="enHD"></param>
        public void UpdateFromDetailToUnitToInstallmentToHD(ref bool result, Entity item, Entity enInstallment, Entity unit, Entity enHD)
        {
            Entity enUnitUpdate = new Entity(unit.LogicalName, unit.Id);
            enUnitUpdate["bsd_estimatehandoverdate"] = item["bsd_estimatehandoverdatenew"];
            service.Update(enUnitUpdate);

            Entity enInstallmentUpdate = new Entity(enInstallment.LogicalName, enInstallment.Id);
            enInstallmentUpdate["bsd_duedate"] = item["bsd_paymentduedate"];
            service.Update(enInstallmentUpdate);

            Entity enHDUpdate = new Entity(enHD.LogicalName, enHD.Id);
            enHDUpdate["bsd_estimatehandoverdatecontract"] = item["bsd_estimatehandoverdatenew"];
            service.Update(enHDUpdate);
        }
        /// <summary>
        /// Cập nhật field Payment due date [bsd_paymentduedate] trên entity Con qua field  Due Date của installment  
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="enInstallment"></param>
        public void UpdateFromDetailToInstallment(ref bool result, Entity item, Entity enInstallment)
        {
            Entity enInstallmentUpdate = new Entity(enInstallment.LogicalName, enInstallment.Id);
            enInstallmentUpdate["bsd_duedate"] = item["bsd_paymentduedate"];
            service.Update(enInstallmentUpdate);
        }
    }
}
