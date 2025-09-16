using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Action_TerminateLetter_GenerateTerminateLetter_Detail
{
    public class Action_TerminateLetter_GenerateTerminateLetter_Detail : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        IPluginExecutionContext context = null;
        ITracingService tracingService = null;
        /// <summary>
        /// caseSign=0 : Không tính lãi <br></br>
        /// caseSign=1 : Chỉ tính lãi cho các đợt trước đợt tích Sign Contract Installment <br></br>
        /// caseSign=2 : Chỉ tính lãi cho các đợt sau đợt tích Sign Contract Installment <br></br>
        /// caseSign=3 : Tính lãi cho các đợt theo mức lãi suất bình thường <br></br>
        /// </summary>
        int caseSign = 0;
        DateTime _date = DateTime.Now;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("userid"))
            {

                string userid = context.InputParameters["userid"].ToString();
                tracingService.Trace("user Action:" + userid);
                EntityReference user = new EntityReference("systemuser", new Guid(userid));
                this.service = (IOrganizationService)factory.CreateOrganizationService(user.Id);
            }
            ((ITracingService)serviceProvider.GetService(typeof(ITracingService))).Trace(string.Format("Context Depth {0}", (object)service.Depth));
            var entity1 = this.service.Retrieve("salesorder", new Guid(this.context.InputParameters["id"].ToString()), new ColumnSet(true));
            if (context.InputParameters.Contains("_date") && context.InputParameters["_date"].ToString() != "")
                _date = Convert.ToDateTime(context.InputParameters["_date"].ToString());
            var query_bsd_name = "GENTerminationLetter";
            var query = new QueryExpression("bsd_process");
            //query.TopCount = 50; query.ColumnSet.AllColumns = true;
            //query.Criteria.AddCondition("bsd_name", ConditionOperator.Equal, query_bsd_name);
            //var process = this.service.RetrieveMultiple(query);
            //if (((OptionSetValue)process.Entities[0]["statuscode"]).Value == 1)
            //    return;
            tracingService.Trace($"start: id {this.context.InputParameters["id"].ToString()}");
            if (!this.CheckExist_optionEntry_on_Terminateletter(entity1.Id))
            {
                tracingService.Trace("step1");
                EntityCollection dtlFromOpentryId = this.get_pmSchDtl_fromOpentryID(entity1.Id);
                tracingService.Trace(dtlFromOpentryId.Entities.Count.ToString());
                int num2 = 0;
                for (int index2 = 0; index2 < dtlFromOpentryId.Entities.Count; ++index2)
                {
                    Entity entity2 = dtlFromOpentryId.Entities[index2];
                    var isbsd_duedate = entity2.Contains("bsd_duedate");

                    tracingService.Trace($"step 2 :{entity2.Contains("bsd_duedate")}");
                    tracingService.Trace($"step 2 1:{entity2.LogicalName}:{entity2.Id}");

                    if (isbsd_duedate)
                    {

                        tracingService.Trace($"step 2 1:{entity2.LogicalName}:{entity2.Id}");
                        if (entity2.Contains("statuscode"))
                        {
                            if (((OptionSetValue)entity2["statuscode"]).Value == 100000001)
                            {
                                ////tracingService.Trace($"step 2.1 ");
                                //if (entity2.Contains("bsd_actualgracedays"))
                                //{
                                //    tracingService.Trace($"step 2.1.1 ");
                                //    num2 += (int)entity2["bsd_actualgracedays"];
                                //}    
                                //else
                                //    num2 = num2;
                                continue;
                            }
                            else if (((OptionSetValue)entity2["statuscode"]).Value == 100000000)
                            {
                                //tracingService.Trace($"step 2.2 ");
                                DateTime dateTime = _date;
                                dateTime = dateTime.Date;
                                int num3 = (int)dateTime.Subtract(((DateTime)entity2["bsd_duedate"]).Date).TotalDays;
                                if (num3 <= 0)
                                    num3 = 0;
                                num2 += num3;
                                //tracingService.Trace($"step 2.3 {num2}");

                            }
                        }
                    }

                }
                Entity installment = null;
                if (this.check_overDuedate_pmShcDtl(dtlFromOpentryId, ref installment) || num2 > 60)
                {
                    #region khởi tạo entity generate letter detail
                    Entity enGenDetail = new Entity("bsd_genterminationletterdetail");
                    enGenDetail["bsd_genterminationletter"] = new EntityReference("bsd_genterminationletter", new Guid(this.context.InputParameters["bsd_generate_termination_letter"].ToString()));
                    #endregion
                    try
                    {
                        tracingService.Trace($"step 3 ");
                        Entity entity3 = new Entity("bsd_terminateletter");
                        tracingService.Trace($"step 3 2 {entity3.Id}");

                        entity3["bsd_insallment"] = installment.ToEntityReference();
                        if (entity1.Contains("customerid"))
                            entity3["bsd_customer"] = entity1["customerid"];
                        tracingService.Trace($"step 3 2 {((EntityReference)entity3["bsd_customer"]).Id}");

                        entity3["bsd_name"] = (object)("Terminate letter of " + (entity1.Contains("name") ? entity1["name"] : (object)""));
                        tracingService.Trace($"step 3 3");

                        entity3["bsd_subject"] = (object)"Terminate letter";
                        tracingService.Trace($"step 3 4");

                        entity3["bsd_date"] = _date;
                        tracingService.Trace($"step 3 5");

                        entity3["bsd_terminatefee"] = (object)new Money(0M);
                        tracingService.Trace($"step 3 6");

                        entity3["bsd_optionentry"] = new EntityReference(entity1.LogicalName, entity1.Id);
                        entity3["bsd_units"] = entity1["bsd_unitnumber"];

                        tracingService.Trace($"step 3 7");
                        entity3["bsd_source"] = new OptionSetValue(100000001);
                        EntityCollection units = this.findUnits(this.service, entity1);
                        tracingService.Trace($"step 3 8");
                        entity3["bsd_system"] = true;
                        if (units.Entities.Count > 0)
                        {
                            Entity entity4 = units[0];
                            if (entity4.Contains("productid"))
                                entity3["bsd_units"] = entity4["productid"];
                        }
                        tracingService.Trace($"step 3 9");
                        entity3["bsd_project"] = entity1["bsd_project"];
                        entity3["bsd_totalforfeitureamount"] = new Money(0);
                        #region map warring notice
                        query = new QueryExpression("bsd_warningnotices");
                        query.TopCount = 2; query.ColumnSet.AllColumns = true;
                        query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, entity1.Id.ToString());
                        query.Criteria.AddCondition("bsd_paymentschemedeitail", ConditionOperator.Equal, installment.Id.ToString());
                        query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
                        var rsWN = this.service.RetrieveMultiple(query);
                        if (rsWN.Entities.Count > 0)
                        {
                            if (rsWN.Entities.Count > 1)
                            {
                                if (((int)rsWN.Entities[0]["bsd_numberofwarning"]) < ((int)rsWN.Entities[1]["bsd_numberofwarning"]))
                                {
                                    entity3[$"bsd_warning_notices_1"] = rsWN.Entities[0].ToEntityReference();
                                    entity3[$"bsd_warning_notices_2"] = rsWN.Entities[1].ToEntityReference();

                                }
                                else
                                {
                                    entity3[$"bsd_warning_notices_2"] = rsWN.Entities[0].ToEntityReference();
                                    entity3[$"bsd_warning_notices_1"] = rsWN.Entities[1].ToEntityReference();
                                }
                            }
                            else
                            {
                                entity3[$"bsd_warning_notices_1"] = rsWN.Entities[0].ToEntityReference();
                            }

                        }
                        //for (var i = 0; i < rsWN.Entities.Count; i++)
                        //{
                        //    if (i == 2)
                        //        break;
                        //    entity3[$"bsd_warning_notices_{i + 1}"] = rsWN.Entities[i].ToEntityReference();
                        //}
                        #endregion
                        #region tính Penaty  
                        var en_bsd_paymentscheme = this.service.Retrieve("bsd_paymentscheme",
                            ((EntityReference)entity1["bsd_paymentscheme"]).Id, new ColumnSet(true));
                        var bsd_totalpercent = ((decimal)entity1["bsd_totalpercent"]);
                        var bsd_spforfeiture = (decimal)en_bsd_paymentscheme["bsd_spforfeiture"];

                        #region cthuc mới 9/9/2025

                        #endregion
                        //if (bsd_totalpercent <= bsd_spforfeiture)
                        //{
                        //    var bsd_penaty = ((Money)entity1["bsd_depositamount"]).Value;
                        //    var installments = get_all_pmSchDtl_fromOpentryID(entity1.Id);
                        //    for (var i = 0; i < installments.Entities.Count; i++)
                        //    {
                        //        tracingService.Trace($"{installments[i].Id}");
                        //        var bsd_amountwaspaid = installments[i].Contains("bsd_amountwaspaid") ? ((Money)installments[i]["bsd_amountwaspaid"]).Value : 0;
                        //        tracingService.Trace($"{bsd_amountwaspaid}");
                        //        bsd_penaty += bsd_amountwaspaid;
                        //    }
                        //    tracingService.Trace("bsd_penaty " + bsd_penaty.ToString());
                        //    entity3["bsd_penaty"] = new Money(bsd_penaty);
                        //}
                        //else
                        //{
                        //    entity3["bsd_penaty"] = new Money((bsd_spforfeiture / 100) * ((Money)entity1["bsd_totalamountlessfreight"]).Value);

                        //}
                        double percent =0.2;
                        var money = ((Money)entity1["bsd_totalamountlessfreight"]).Value *  (decimal)percent;
                        tracingService.Trace("(20 / 100 " + money);
                        tracingService.Trace("bsd_totalamountlessfreight * 20% " + money.ToString());
                        entity3["bsd_penaty"] = new Money(money);
                        tracingService.Trace("bsd_totalamountlessfreight " + ((Money)entity1["bsd_totalamountlessfreight"]).Value.ToString());
                        tracingService.Trace("bsd_penaty " + ((Money)entity3["bsd_penaty"]).Value.ToString());
                        #endregion
                        #region  Overdue Interest
                        tracingService.Trace("installment name = " + installment["bsd_name"].ToString());
                        var en_bsd_interestratemaster = this.service.Retrieve("bsd_interestratemaster",
                            ((EntityReference)en_bsd_paymentscheme["bsd_interestratemaster"]).Id, new ColumnSet(true));
                        var bsd_termsinterestpercentage = (decimal)installment["bsd_interestchargeper"];
                        var bsd_gracedays = (int)en_bsd_interestratemaster["bsd_gracedays"];
                        var bsd_duedate = ((DateTime)installment["bsd_duedate"]).AddHours(7);
                        tracingService.Trace("step 4");
                        #region check caseSign
                        var bsd_signedcontractdate = new DateTime();

                        var bsd_signeddadate = new DateTime();
                        if (entity1.Contains("bsd_signedcontractdate"))
                        {
                            bsd_signedcontractdate = (DateTime)entity1["bsd_signedcontractdate"];
                            caseSign = 2;
                            if (entity1.Contains("bsd_signeddadate"))
                            {
                                bsd_signeddadate = (DateTime)entity1["bsd_signeddadate"];
                                caseSign = 3;
                            }
                        }
                        else
                        {
                            if (entity1.Contains("bsd_signeddadate"))
                            {
                                bsd_signeddadate = (DateTime)entity1["bsd_signeddadate"];
                                caseSign = 1;
                            }
                            else
                            {
                                caseSign = 4;
                            }
                        }

                        var lateDays = ((int)((_date - bsd_duedate).TotalDays) - bsd_gracedays);
                        var latedays2 = lateDays;
                        var resCheckCaseSign = checkCaseSignAndCalLateDays(installment, entity1, bsd_signedcontractdate, bsd_signeddadate, DateTime.UtcNow.AddHours(7), ref lateDays);
                        lateDays = latedays2 <= lateDays ? latedays2 : lateDays;
                        #endregion
                        if (resCheckCaseSign)
                        {
                            tracingService.Trace($"resCheckCaseSign:true");
                            entity3["bsd_overdue_interest"] = bsd_termsinterestpercentage / 100 * lateDays * (installment.Contains("bsd_balance") ? ((Money)installment["bsd_balance"]).Value : 1);
                            entity3["bsd_overdue_interest_money"] = new Money((decimal)entity3["bsd_overdue_interest"]);

                        }
                        tracingService.Trace($"bsd_termsinterestpercentage: {bsd_termsinterestpercentage}");
                        tracingService.Trace($"bsd_gracedays: {bsd_gracedays}");
                        tracingService.Trace($"bsd_duedate:{bsd_duedate}");
                        tracingService.Trace($"today:{_date}");
                        tracingService.Trace($"totalday: {((int)((_date - bsd_duedate).TotalDays) - bsd_gracedays)}");
                        tracingService.Trace($"bsd_overdue_interest: {bsd_termsinterestpercentage * lateDays * (installment.Contains("bsd_balance") ? ((Money)installment["bsd_balance"]).Value : 1)}");
                        tracingService.Trace($"bsd_balance:{((Money)installment["bsd_balance"]).Value}");


                        var bsd_overdue_interest2 = ((Money)entity3["bsd_overdue_interest_money"]).Value;
                        tracingService.Trace(" bsd_overdue_interest_money: " + bsd_overdue_interest2.ToString());
                        #region Cộng thêm lãi chưa thanh toán các đợt trước và lãi đợt đang xét đó nếu có
                        var query_bsd_ordernumber = (int)installment["bsd_ordernumber"];
                        tracingService.Trace("query_bsd_ordernumber: " + query_bsd_ordernumber.ToString());
                        var query_bsd_optionentry = installment.GetAttributeValue<EntityReference>("bsd_optionentry").Id.ToString();
                        tracingService.Trace("query_bsd_ordernumber: " + query_bsd_ordernumber);
                        tracingService.Trace("query_bsd_optionentry: " + query_bsd_optionentry.ToString());
                        tracingService.Trace("lấy các đợt trước đó để + thêm lãi chưa thanh toán cho bsd_overdue_interest");
                        var query2 = new QueryExpression("bsd_paymentschemedetail")
                        {
                            ColumnSet = new ColumnSet(true),
                            Criteria =
                                    {
                                        Conditions =
                                        {
                                            new ConditionExpression("bsd_ordernumber", ConditionOperator.LessThan, query_bsd_ordernumber),
                                            new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, query_bsd_optionentry)
                                        }
                                    }
                        };
                        var rsIns = this.service.RetrieveMultiple((QueryBase)query2);
                        foreach (var rs in rsIns.Entities)
                        {
                            tracingService.Trace("ordernumber: " + ((int)rs["bsd_ordernumber"]));
                            var bsd_interestchargeremaining = rs.Contains("bsd_interestchargeremaining") ? ((Money)rs["bsd_interestchargeremaining"]).Value : 0;
                            tracingService.Trace("bsd_interestchargeremaining: " + bsd_interestchargeremaining.ToString());
                            bsd_overdue_interest2 += bsd_interestchargeremaining;
                        }
                        entity3["bsd_overdue_interest_money"] = new Money(bsd_overdue_interest2);
                        tracingService.Trace("bsd_overdue_interest_money " + ((Money)entity3["bsd_overdue_interest_money"]).Value.ToString());

                        tracingService.Trace("check và cộng thêm lãi đợt đang xét nếu có");
                        var bsd_interestchargeremaining_current = installment.Contains("bsd_interestchargeremaining") ? ((Money)installment["bsd_interestchargeremaining"]).Value : 0;
                        tracingService.Trace("bsd_interestchargeremaining_current: " + bsd_interestchargeremaining_current.ToString());
                        bsd_overdue_interest2 += bsd_interestchargeremaining_current;
                        entity3["bsd_overdue_interest_money"] = new Money(bsd_overdue_interest2);
                        tracingService.Trace("bsd_overdue_interest_money " + ((Money)entity3["bsd_overdue_interest_money"]).Value.ToString());

                        #endregion
                        #endregion
                        entity3["bsd_generateterminationletter"] = enGenDetail["bsd_genterminationletter"];
                        var id = this.service.Create(entity3);
                        #region map và create entity generate letter detail
                        //tracingService.Trace("bsd_terminateletter:"+id.ToString());
                        //enGenDetail["bsd_optionentry"] = new EntityReference(entity1.LogicalName, entity1.Id);
                        //enGenDetail["statuscode"] = new OptionSetValue(100000002); // Error
                        //enGenDetail["bsd_teminationletter"] = new EntityReference("bsd_terminateletter", id);
                        //this.service.Create(enGenDetail);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        tracingService.Trace($"Error: {ex.Message}");
                        enGenDetail["bsd_errordetail"] = ex.Message;
                        enGenDetail["bsd_optionentry"] = new EntityReference(entity1.LogicalName, entity1.Id);
                        enGenDetail["bsd_errordetail"] = ex.Message;
                        enGenDetail["statuscode"] = new OptionSetValue(100000001); // Error
                        this.service.Create(enGenDetail);

                    }

                }
            }
        }
        public bool checkCaseSignAndCalLateDays(Entity enInstallment, Entity enOptionEntry, DateTime bsd_signedcontractdate, DateTime bsd_signeddadate, DateTime receiptdate, ref int lateDays)
        {
            bool result = false;
            bool isContainDueDate = false;
            if (enInstallment.Contains("bsd_duedate"))
                isContainDueDate = true;

            var bsd_duedateFlag = new DateTime();
            var bsd_duedate = new DateTime();
            if (isContainDueDate == true)
            {
                bsd_duedate = (DateTime)enInstallment["bsd_duedate"];
            }
            #region lấy ra đợt tích Sign Contract Installment
            var query_bsd_signcontractinstallment = true;

            var query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_signcontractinstallment", ConditionOperator.Equal, query_bsd_signcontractinstallment);
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id.ToString());
            var rs = service.RetrieveMultiple(query);
            tracingService.Trace($"enOptionEntry {enOptionEntry.Id}");
            tracingService.Trace($"enInstallment {enInstallment.Id}");
            #endregion
            tracingService.Trace($"start caseSign {caseSign}");
            tracingService.Trace($"rs.Entities.Count {rs.Entities.Count}");
            switch (caseSign)
            {
                case 0:
                    result = false;
                    break;
                case 1:
                    if (rs.Entities.Count > 0)
                    {
                        if (rs.Entities[0].Id == enInstallment.Id)
                        {
                            result = false;
                            break;
                        }
                        if (isContainDueDate == false)
                            result = false;
                        else
                        {
                            bsd_duedateFlag = (DateTime)rs.Entities[0]["bsd_duedate"];
                            if (bsd_duedate < bsd_duedateFlag)
                            {
                                result = true;
                                //tính số ngày trễ hạn 
                                lateDays = (int)(receiptdate - bsd_signeddadate).TotalDays;
                            }
                            else
                            if (bsd_duedate >= bsd_duedateFlag)
                            {
                                result = true;
                                lateDays = (int)(receiptdate - bsd_duedate).TotalDays;
                            }
                            else result = false;
                        }
                    }
                    break;
                case 2:

                    if (rs.Entities.Count > 0)
                    {
                        if (isContainDueDate == false)
                            result = false;
                        else
                        {
                            bsd_duedateFlag = (DateTime)rs.Entities[0]["bsd_duedate"];
                            if (bsd_duedate >= bsd_duedateFlag)
                            {
                                result = true;
                                lateDays = (int)(receiptdate - bsd_signedcontractdate).TotalDays;
                            }
                            else result = false;
                        }
                    }
                    break;
                case 3:
                    result = true;
                    bsd_duedateFlag = (DateTime)rs.Entities[0]["bsd_duedate"];
                    if (bsd_duedate < bsd_duedateFlag)
                    {
                        //tính số ngày trễ hạn 
                        lateDays = (int)(receiptdate - bsd_signeddadate).TotalDays;
                    }
                    else
                    {
                        //tính số ngày trễ hạn 
                        lateDays = (int)(receiptdate - bsd_duedate).TotalDays;
                    }
                    break;
                case 4:

                    if (rs.Entities.Count > 0)
                    {
                        if (isContainDueDate == false)
                            result = false;
                        else
                        {
                            bsd_duedateFlag = (DateTime)rs.Entities[0]["bsd_duedate"];
                            tracingService.Trace("bsd_duedate >= bsd_duedateFlag: " + (bsd_duedate >= bsd_duedateFlag).ToString());
                            if (bsd_duedate >= bsd_duedateFlag)
                            {
                                result = true;
                                lateDays = (int)(receiptdate - bsd_duedate).TotalDays;
                            }
                            else result = false;
                        }
                    }
                    break;
                default:
                    result = false;
                    break;
            }
            tracingService.Trace("resultCaseSign:" + result);
            return result;
        }
        private bool check_overDuedate_pmShcDtl(EntityCollection entCll, ref Entity installment)
        {
            for (int index = 0; index < entCll.Entities.Count; ++index)
            {

                Entity entity = entCll.Entities[index];
                if (entity.Contains("bsd_duedate") == false)
                    continue;
                DateTime dateTime = _date;
                dateTime = dateTime.Date;
                if ((int)dateTime.Subtract(((DateTime)entity["bsd_duedate"]).Date).TotalDays > 60)
                {
                    installment = entity;
                    return true;
                }
            }
            return false;
        }

        private EntityCollection RetrieveMultiRecord(
          IOrganizationService crmservices,
          string entity,
          ColumnSet column,
          string condition,
          object value)
        {
            QueryExpression query = new QueryExpression(entity);
            query.ColumnSet = column;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            return this.service.RetrieveMultiple((QueryBase)query);
        }

        private EntityCollection getPmShDtl(IOrganizationService crmservices, Entity op)
        {
            //string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                   <entity name='bsd_paymentschemedetail'>\r\n                    <attribute name='bsd_paymentschemedetailid' />\r\n                    <attribute name='bsd_name' />\r\n                    <attribute name='createdon' />\r\n                    <order attribute='bsd_name' descending='false' />\r\n                    <filter type='and'>\r\n                      <condition attribute='bsd_optionentry' operator='eq'  value='{0}' />\r\n                    </filter>\r\n                  </entity>\r\n                </fetch>", (object)op.Id);
            string query = string.Format(
                    "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n" +
                    "  <entity name='bsd_paymentschemedetail'>\r\n" +
                    "    <attribute name='bsd_paymentschemedetailid' />\r\n" +
                    "    <attribute name='bsd_name' />\r\n" +
                    "    <attribute name='createdon' />\r\n" +
                    "    <order attribute='bsd_name' descending='false' />\r\n" +
                    "    <filter type='and'>\r\n" +
                    "      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />\r\n" +
                    "    </filter>\r\n" +
                    "  </entity>\r\n" +
                    "</fetch>",
                    (object)op.Id
);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private EntityCollection findPaymentScheme(IOrganizationService crmservices)
        {
            //string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='salesorder'>\r\n                    <attribute name='name' />\r\n                    <attribute name='customerid' />\r\n                    <attribute name='statuscode' />\r\n                    <attribute name='totalamount' />\r\n                    <attribute name='salesorderid' />\r\n                    <order attribute='name' descending='false' />\r\n                    <filter type='and'>\r\n                      <condition attribute='name' operator='not-null' />\r\n                    </filter>                    \r\n                  </entity>\r\n                </fetch>");
            string query = string.Format(
                            "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n" +
                            "  <entity name='salesorder'>\r\n" +
                            "    <attribute name='name' />\r\n" +
                            "    <attribute name='customerid' />\r\n" +
                            "    <attribute name='statuscode' />\r\n" +
                            "    <attribute name='totalamount' />\r\n" +
                            "    <attribute name='salesorderid' />\r\n" +
                            "    <order attribute='name' descending='false' />\r\n" +
                            "    <filter type='and'>\r\n" +
                            "      <condition attribute='name' operator='not-null' />\r\n" +
                            "    </filter>\r\n" +
                            "  </entity>\r\n" +
                            "</fetch>"
                        );
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private EntityCollection findUnits(IOrganizationService crmservices, Entity OptionEntry)
        {
            //string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                <entity name='salesorderdetail'>\r\n                <attribute name='productid' />\r\n                <attribute name='salesorderdetailid' />\r\n                <order attribute='productid' descending='false' />\r\n                <filter type='and'>\r\n              <condition attribute='salesorderid' operator='eq'  uitype='salesorder' value='{0}' />\r\n            </filter>\r\n          </entity>\r\n        </fetch>", (object)OptionEntry.Id);
            string query = string.Format(
                        "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n" +
                        "  <entity name='salesorderdetail'>\r\n" +
                        "    <attribute name='productid' />\r\n" +
                        "    <attribute name='salesorderdetailid' />\r\n" +
                        "    <order attribute='productid' descending='false' />\r\n" +
                        "    <filter type='and'>\r\n" +
                        "      <condition attribute='salesorderid' operator='eq' uitype='salesorder' value='{0}' />\r\n" +
                        "    </filter>\r\n" +
                        "  </entity>\r\n" +
                        "</fetch>",
                        (object)OptionEntry.Id
                    );
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private EntityCollection get_pmschDtl_outofDuedate(IOrganizationService crmservices, Entity op)
        {
            //string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='salesorder'>\r\n                   <attribute name='name' />\r\n                   <attribute name='statuscode' />\r\n                    <attribute name='totalamount' />\r\n                    <attribute name='salesorderid' />\r\n                    <order attribute='name' descending='false' />\r\n                    <filter type='and'>\r\n                      <condition attribute='name' operator='not-null' />\r\n                    </filter>\r\n                    <link-entity name='bsd_paymentschemedetail' from='bsd_optionentry' to='salesorderid' alias='ab'>\r\n                      <filter type='and'>\r\n                        <condition attribute='bsd_duedate' operator='olderthan-x-days' value='59' />\r\n                        <condition attribute='statuscode' operator='eq' value='100000000' />\r\n                        <condition attribute='bsd_optionentry' operator='eq' value={0} />\r\n                      </filter>\r\n                    </link-entity>\r\n                  </entity>\r\n                </fetch>", (object)op.Id);
            string query = string.Format(
                    "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n" +
                    "  <entity name='salesorder'>\r\n" +
                    "    <attribute name='name' />\r\n" +
                    "    <attribute name='statuscode' />\r\n" +
                    "    <attribute name='totalamount' />\r\n" +
                    "    <attribute name='salesorderid' />\r\n" +
                    "    <order attribute='name' descending='false' />\r\n" +
                    "    <filter type='and'>\r\n" +
                    "      <condition attribute='name' operator='not-null' />\r\n" +
                    "    </filter>\r\n" +
                    "    <link-entity name='bsd_paymentschemedetail' from='bsd_optionentry' to='salesorderid' alias='ab'>\r\n" +
                    "      <filter type='and'>\r\n" +
                    "        <condition attribute='bsd_duedate' operator='olderthan-x-days' value='59' />\r\n" +
                    "        <condition attribute='statuscode' operator='eq' value='100000000' />\r\n" +
                    "        <condition attribute='bsd_optionentry' operator='eq' value={0} />\r\n" +
                    "      </filter>\r\n" +
                    "    </link-entity>\r\n" +
                    "  </entity>\r\n" +
                    "</fetch>",
    (object)op.Id
);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private bool CheckExist_optionEntry_on_Terminateletter(Guid opRef)
        {
            QueryExpression query = new QueryExpression("bsd_terminateletter");
            query.ColumnSet = new ColumnSet(new string[1]
            {
        "bsd_terminateletterid"
            });
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object)opRef));
            query.TopCount = new int?(1);
            return this.service.RetrieveMultiple((QueryBase)query).Entities.Count > 0;
        }

        private EntityCollection get_pmSchDtl_fromOpentryID(Guid opID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[10]
            {
        "bsd_duedate",
        "statuscode",
        "bsd_balance",
        "bsd_actualgracedays",
        "bsd_amountofthisphase",
                "bsd_name",
                "bsd_interestchargeper",
                "bsd_ordernumber",
                "bsd_optionentry",
                "bsd_interestchargeremaining"
            });
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object)opID));
            query.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000000));
            return this.service.RetrieveMultiple((QueryBase)query);
        }
        private EntityCollection get_all_pmSchDtl_fromOpentryID(Guid opID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[5]
            {
        "bsd_duedate",
        "statuscode",
        "bsd_actualgracedays",
        "bsd_amountofthisphase",
        "bsd_amountwaspaid",
            });
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object)opID));
            return this.service.RetrieveMultiple((QueryBase)query);
        }
    }
}
