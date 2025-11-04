using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Updateduedate
{
    public class Plugin_Updateduedate : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
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
            var status = ((OptionSetValue)en["statuscode"]).Value;
            tracingService.Trace("start :" + status);
            //check status
            if (status == 100000000)
            {
                var result = true;
                var rs = ExistDetail(ref result);
                tracingService.Trace("count: " + rs.Entities.Count);
                Entity enDetailUpdate = new Entity(entity.LogicalName, entity.Id);
                enDetailUpdate["bsd_processing_pa"] = true; //Status Reason(entity Detail) = Approved
                enDetailUpdate["bsd_error"] = false;
                enDetailUpdate["bsd_errordetail"] = "";
                enDetailUpdate["statuscode"] = new OptionSetValue(1);
                enDetailUpdate["bsd_approvedrejecteddate"] = DateTime.UtcNow; ;
                enDetailUpdate["bsd_approvedrejectedperson"] = new EntityReference("systemuser", context.UserId); ;
                service.Update(enDetailUpdate);
                var request = new OrganizationRequest("bsd_Action_Active_Approved_Updateduedate_Detail");
                string listid = string.Join(",", rs.Entities.Select(x => x.Id.ToString()));
                request["listid"] = listid;
                request["idmaster"] = entity.Id.ToString();
                service.Execute(request);

            }

        }
        /// <summary>
        /// kiểm tra xem có danh sách chi tiết của master không 1
        /// </summary>
        public EntityCollection ExistDetail(ref bool result)
        {
            var query = new QueryExpression("bsd_updateduedatedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_updateduedate", ConditionOperator.Equal, en.Id.ToString());
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            //query.AddOrder("bsd_duedatenew", OrderType.Descending);
            tracingService.Trace($"{en.Id}");
            var rs = service.RetrieveMultiple(query);
            tracingService.Trace($"count:{rs.Entities.Count}");
            if (rs.Entities.Count == 0)
            {
                var mess = "The record does not have any details. Please check again.";
                throw new InvalidPluginExecutionException(mess);
            }
            return rs;
        }
        /// <summary>
        ///  Dự án ở entity detail có trùng với entity Cha không?
        /// </summary>
        public void CheckExistParentInDetail(ref bool result, Entity item)
        {
            if (((EntityReference)en["bsd_project"]).Id != ((EntityReference)item["bsd_project"]).Id)
            {
                var mess = "The project in the Detail entity is invalid. Please check again.";
                throw new InvalidPluginExecutionException(mess);
            }
        }
        /// <summary>
        /// Last Installment [bsd_lastinstallment] = yes 
        ///hoặc Duedate Calculating Method[bsd_duedatecalculatingmethod] =
        ///Estimate handover date?
        /// </summary>
        public void CheckIsLast(ref bool result, Entity item, Entity enInstallment)
        {
            if (((bool)enInstallment["bsd_lastinstallment"])||(enInstallment.Contains("bsd_duedatecalculatingmethod")&&((OptionSetValue)enInstallment["bsd_duedatecalculatingmethod"]).Value== 100000002))
            {
                var mess = "The record contains an invalid batch. Please check again.";
                throw new InvalidPluginExecutionException(mess);
            }
        }
        /// <summary>
        ///  Từ đợt trong entity chi tiết kiểm tra các Hợp đồng có trạng thái =Terminated
        /// </summary>
        public void CheckHD(ref bool result, Entity item, Entity enHD)
        {


            if (((OptionSetValue)enHD["statuscode"]).Value == 100000006)
            {
                var mess = "The record contains a contract that has already been liquidated. Please check again.";
                throw new InvalidPluginExecutionException(mess);
            }



        }
        /// <summary>
        /// kiểm tra đợt trong entity detail được thanh toán không? 
        /// (Kiểm tra 2 field: Amount Was Paid [bsd_amountwaspaid] ; 
        /// Deposit Amount Paid [bsd_depositamount] khác 0)
        /// </summary>
        public void CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)
        {
            if (((Money)enInstallment["bsd_depositamount"]).Value != 0 && ((Money)enInstallment["bsd_amountwaspaid"]).Value != 0)
            {
                var mess = "There is a batch that has already been paid. Please check again.";
                throw new InvalidPluginExecutionException(mess);
            }
        }
        /// <summary>
        /// Kiểm tra ngày đến hạn mới trên entity detail 
        /// (Due Date (New) [bsd_duedatenew]) có lớn hơn đợt phía trước?
        /// </summary>
        public void CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)
        {

            var newDate = (DateTime)item["bsd_duedatenew"];

            var query = new QueryExpression(enInstallment.LogicalName);
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enHD.Id.ToString());
            query.Criteria.AddCondition("bsd_paymentschemedetailid", ConditionOperator.NotEqual, enInstallment.Id.ToString());
            var rs_ = service.RetrieveMultiple(query);
            foreach (var JItem in rs_.Entities)
            {
                if (JItem.Id != enInstallment.Id)
                {
                    if(((int)JItem["bsd_ordernumber"])<((int)enInstallment["bsd_ordernumber"]))
                    {
                        if (newDate <= ((DateTime)JItem["bsd_duedate"]))
                        {
                            var mess = "The new due date is invalid. Please check again.";
                            throw new InvalidPluginExecutionException(mess);
                        }
                    }
                    if (((int)JItem["bsd_ordernumber"]) > ((int)enInstallment["bsd_ordernumber"]))
                    {
                        if (newDate >= ((DateTime)JItem["bsd_duedate"]))
                        {
                            var mess = "The new due date is invalid. Please check again.";
                            throw new InvalidPluginExecutionException(mess);
                        }
                    }
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

            var newDate = (DateTime)item["bsd_duedatenew"];
            tracingService.Trace($"{enInstallment["bsd_duedate"]}");

            tracingService.Trace($"{enInstallment.Id}");
            tracingService.Trace($"newDate {newDate}");
            if ((newDate - (DateTime)enInstallment["bsd_duedate"]).TotalDays == 0)
            {
                var mess = "The new due date is the same as the old due date. Please check again.";
                throw new InvalidPluginExecutionException(mess);
            }


        }
        public void Approve(ref bool result, Entity item, Entity enInstallment)
        {
            // Status Reason(entity cha) = Approved
            //Approved / Rejected Date[bsd_approvedrejecteddate] = Ngày cập nhật thành công
            //Approved / Rejected Person[bsd_approvedrejectedperson] = Người nhấn nút duyệt
            //Status Reason(entity Detail) = Approved
            //Due Date(New) (ở entity Detail) về field Due Date của installment ở entity Detail
            var request = new OrganizationRequest("bsd_Action_Approved_Updateduedate_Master");

            var newDate = (DateTime)item["bsd_duedatenew"];
            request["detail_id"] = item.Id.ToString();
            request["duedatenew"] = newDate.AddHours(7).ToString();
            request["statuscode"] = 100000000;
            service.Execute(request);

        }
    }
}
