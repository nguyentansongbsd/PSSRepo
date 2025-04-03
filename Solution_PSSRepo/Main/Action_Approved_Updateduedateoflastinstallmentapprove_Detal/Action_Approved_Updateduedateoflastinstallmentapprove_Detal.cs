using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Approved_Updateduedateoflastinstallmentapprove_Detal
{
    public class Action_Approved_Updateduedateoflastinstallmentapprove_Detal : IPlugin
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

            tracingService.Trace("start ");
            //get entity
            string enDetailid = context.InputParameters["id"].ToString();
            tracingService.Trace("enDetailid :" + enDetailid);

            en = service.Retrieve("bsd_updateduedateoflastinstallment", new Guid(enDetailid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var item = en;
            if (!CheckConditionRun(en))
            {
                tracingService.Trace("stop");
                return;
            }
            var status = ((OptionSetValue)en["statuscode"]).Value;
            tracingService.Trace("start :" + status);
            //check status

            var result = true;
            tracingService.Trace($"{item.Id}");
            try
            {


                EntityReference enInstallmentRef = (EntityReference)item["bsd_lastinstallment"];
                EntityReference enInstallmentRef2 = new EntityReference(enInstallmentRef.LogicalName, enInstallmentRef.Id);
                var query_bsd_paymentschemedetailid = enInstallmentRef2.Id.ToString();
                Entity enInstallment = GetInstallment(enInstallmentRef2.Id.ToString());
                tracingService.Trace($"item {item["bsd_duedate"]}");
                //tracingService.Trace($"{enInstallment["bsd_duedate"]}");
                tracingService.Trace($"{enInstallment.Id}");
                var enHDRef = (EntityReference)enInstallment["bsd_optionentry"];
                var enHD = service.Retrieve(enHDRef.LogicalName, enHDRef.Id, new ColumnSet(true));
                tracingService.Trace("CheckExistParentInDetail");
                CheckExistParentInDetail(ref result, item);
                tracingService.Trace("CheckIsLast");
                if (!result) return;
                CheckIsLast(ref result, item, enInstallment);
                tracingService.Trace("CheckHD");
                if (!result) return;
                CheckHD(ref result, item, enHD);
                tracingService.Trace("CheckPaidDetail");
                if (!result) return;
                CheckPaidDetail(ref result, item, enInstallment);
                tracingService.Trace("CheckDueDate");
                if (!result) return;
                CheckDueDate(ref result, item, enInstallment, enHD);
                tracingService.Trace("CheckNewDate");
                if (!result) return;
                CheckNewDate(ref result, item, enInstallment);
                if (!result) return;
                tracingService.Trace("Approve");
                Approve(ref result, item, enInstallment);
            }
            catch (Exception ex)
            {
                HandleError(item, ex.Message);
            }
            //Entity enDetailUpdate = new Entity(en.LogicalName, en.Id);
            // Status Reason(entity cha) = Approved
            //Approved / Rejected Date[bsd_approvedrejecteddate] = Ngày cập nhật thành công
            //Approved / Rejected Person[bsd_approvedrejectedperson] = Người nhấn nút duyệt
            //Status Reason(entity Detail) = Approved
            //Due Date(New) (ở entity Detail) về field Due Date của installment ở entity Detail
            //enDetailUpdate["statuscode"] = new OptionSetValue(100000000); //Status Reason(entity Detail) = Approved
            //service.Update(enDetailUpdate);

        }
        public Entity GetInstallment(string id)
        {
            var fetchData = new
            {
                bsd_paymentschemedetailid = id
            };
            var fetchXml = $@"
<fetch top=""50"">
  <entity name=""bsd_paymentschemedetail"">
    <filter>
      <condition attribute=""bsd_paymentschemedetailid"" operator=""eq"" value=""{fetchData.bsd_paymentschemedetailid/*86a5f5bb-343b-ee11-bdf4-000d3aa14fb9*/}"" />
    </filter>
  </entity>
</fetch>";
            var res = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Entity enInstallment = res.Entities[0];
            return enInstallment;
        }
        /// <summary>
        ///  Dự án ở entity detail có trùng với entity Cha không?
        /// </summary>
        public void CheckExistParentInDetail(ref bool result, Entity item)
        {
            var enMasterRef = (EntityReference)item["bsd_updateduedateoflastinstapprove"];

            var enMaster = service.Retrieve("bsd_updateduedateoflastinstallmentapprove", enMasterRef.Id, new ColumnSet(true));
            if (((EntityReference)enMaster["bsd_project"]).Id != ((EntityReference)item["bsd_project"]).Id)
            {
                var mess = "The project in the Detail entity is invalid. Please check again.";
                HandleError(item, mess);

                result = false;
            }



        }
        /// <summary>
        /// Kiểm tra đây có phải là Đợt cuối? (Last Installment [bsd_lastinstallment] = yes 
        /// </summary>
        public void CheckIsLast(ref bool result, Entity item, Entity enInstallment)
        {

            var enInstallmentRef = (EntityReference)item["bsd_lastinstallment"];
            if (!((bool)enInstallment["bsd_lastinstallment"]))
            {
                var mess = "The record contains an invalid batch. Please check again.";
                HandleError(item, mess);

                result = false;
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
                HandleError(item, mess);

                result = false;
            }



        }
        /// <summary>
        /// kiểm tra đợt trong entity detail được thanh toán không? 
        /// (Kiểm tra 2 field: Amount Was Paid [bsd_amountwaspaid] ; 
        /// Deposit Amount Paid [bsd_depositamount] khác 0)
        /// </summary>
        public void CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)
        {
            //tracingService.Trace($"bsd_depositamount {((Money)enInstallment["bsd_depositamount"]).Value}");
            //tracingService.Trace($"bsd_amountwaspaid {((Money)enInstallment["bsd_amountwaspaid"]).Value}");
            //if ((((Money)enInstallment["bsd_depositamount"]).Value != 0 || ((Money)enInstallment["bsd_amountwaspaid"]).Value != 0))
            //{
            //    var mess = "There is a batch that has already been paid. Please check again.";
            //    HandleError(item, mess);

            //    result = false;
            //}
        }
        /// <summary>
        /// Kiểm tra ngày đến hạn mới trên entity detail 
        /// (Due Date (New) [bsd_duedatenew]) có lớn hơn đợt phía trước?
        /// </summary>
        public void CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)
        {
            if (!item.Contains("bsd_duedate")) return;
            var newDate = (DateTime)item["bsd_duedate"];
            tracingService.Trace("step 1");
            var query = new QueryExpression(enInstallment.LogicalName);
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enHD.Id.ToString());
            var rs_ = service.RetrieveMultiple(query);
            foreach (var JItem in rs_.Entities)
            {
                if (JItem.Id != enInstallment.Id)
                {

                    //if (!JItem.Contains("bsd_duedate")) continue;
                    tracingService.Trace($"{(DateTime)JItem["bsd_duedate"]}");

                    if (newDate <= ((DateTime)JItem["bsd_duedate"]))
                    {
                        var mess = "The new due date is invalid. Please check again.";
                        HandleError(item, mess);

                        result = false;
                        break;
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
            if (!enInstallment.Contains("bsd_duedate"))
                return;
            var newDate = (DateTime)item["bsd_duedate"];
            tracingService.Trace($"{enInstallment.Id}");
            if ((newDate - (DateTime)enInstallment["bsd_duedate"]).TotalDays == 0)
            {
                var mess = "The new due date is the same as the old due date. Please check again.";
                HandleError(item, mess);

                result = false;
            }


        }
        public void Approve(ref bool result, Entity item, Entity enInstallment)
        {
            // Status Reason(entity cha) = Approved
            //Approved / Rejected Date[bsd_approvedrejecteddate] = Ngày cập nhật thành công
            //Approved / Rejected Person[bsd_approvedrejectedperson] = Người nhấn nút duyệt
            //Status Reason(entity Detail) = Approved
            //Due Date(New) (ở entity Detail) về field Due Date của installment ở entity Detail
            var request = new OrganizationRequest("bsd_Action_Approved_UpdateDuedateOfLastInstallment_Master");

            var newDate = (DateTime)item["bsd_duedate"];
            request["detail_id"] = item.Id.ToString();
            request["duedatenew"] = newDate.AddHours(7).ToString();
            request["statuscode"] = 100000000;
            service.Execute(request);

        }
        public void HandleError(Entity item, string error)
        {
            var enMasterRef = (EntityReference)item["bsd_updateduedateoflastinstapprove"];
            var enMaster = new Entity("bsd_updateduedateoflastinstallmentapprove", enMasterRef.Id);
            enMaster["bsd_error"] = true;
            enMaster["bsd_errordetail"] = error;
            enMaster["bsd_processing_pa"] = false;

            enMaster["statuscode"] = new OptionSetValue(1);
            service.Update(enMaster);
            tracingService.Trace("error nè");
        }
        public bool CheckConditionRun(Entity item)
        {
            var enMasterRef = (EntityReference)item["bsd_updateduedateoflastinstapprove"];
            var enMaster = service.Retrieve("bsd_updateduedateoflastinstallmentapprove", enMasterRef.Id, new ColumnSet(true));
            tracingService.Trace("CheckConditionRun");
            if ((bool)enMaster["bsd_error"] == true && (bool)enMaster["bsd_processing_pa"] == false)
            {
                tracingService.Trace("error: " + (bool)enMaster["bsd_error"]);
                return false;
            }
            else
            {
                
                return true;
            }
        }
    }
}
