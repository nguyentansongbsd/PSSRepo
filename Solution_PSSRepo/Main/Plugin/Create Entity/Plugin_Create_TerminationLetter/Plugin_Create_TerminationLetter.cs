using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Create_TerminationLetter
{
    public class Plugin_Create_TerminationLetter : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        int caseSign = 0;

        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            tracingService.Trace("Plugin_Create_TerminationLetter" + "id: " + entity.Id.ToString());
            if (!enCreated.Contains("bsd_optionentry")) return;
            if(enCreated.Contains("bsd_insallment")) return ;
            var opRef = (EntityReference)enCreated["bsd_optionentry"];
            var op=service.Retrieve(opRef.LogicalName,opRef.Id,new ColumnSet(true));
            var query_bsd_optionentry = opRef.Id.ToString();
            tracingService.Trace("Plugin_Create_TerminationLetter" + "opRef.Id: " + opRef.Id.ToString());

            var query = new QueryExpression("bsd_warningnotices");
            query.TopCount = 2; query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, query_bsd_optionentry);
            query.AddOrder("createdon", OrderType.Descending);
            var result = service.RetrieveMultiple(query);
            var enupdate = new Entity("bsd_terminateletter", enCreated.Id);
            tracingService.Trace($"count {result.Entities.Count}");
            if (result.Entities.Count > 0)
            {
                if (result.Entities.Count > 1)
                {
                    enupdate["bsd_warning_notices_1"] = new EntityReference("bsd_warningnotices", result.Entities[1].Id);
                    enupdate["bsd_warning_notices_2"] = new EntityReference("bsd_warningnotices", result.Entities[0].Id);
                }
                else
                {
                    enupdate["bsd_warning_notices_1"] = new EntityReference("bsd_warningnotices", result.Entities[0].Id);

                }
                
            }
            EntityCollection dtlFromOpentryId = this.get_pmSchDtl_fromOpentryID(opRef.Id);
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
                            DateTime dateTime = DateTime.Now;
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
                enupdate["bsd_insallment"]= installment.ToEntityReference();
                #region tính Penaty  
                var en_bsd_paymentscheme = this.service.Retrieve("bsd_paymentscheme",
                ((EntityReference)op["bsd_paymentscheme"]).Id, new ColumnSet(true));
                var bsd_totalpercent = ((decimal)op["bsd_totalpercent"]);
                var bsd_spforfeiture = (decimal)en_bsd_paymentscheme["bsd_spforfeiture"];
                if (bsd_totalpercent <= bsd_spforfeiture)
                {
                    var bsd_penaty = ((Money)op["bsd_depositamount"]).Value;
                    var installments = get_all_pmSchDtl_fromOpentryID(op.Id);
                    for (var i = 0; i < installments.Entities.Count; i++)
                    {
                        tracingService.Trace($"{installments[i].Id}");
                        var bsd_amountwaspaid = installments[i].Contains("bsd_amountwaspaid") ? ((Money)installments[i]["bsd_amountwaspaid"]).Value : 0;
                        tracingService.Trace($"{bsd_amountwaspaid}");
                        bsd_penaty += bsd_amountwaspaid;
                    }
                    tracingService.Trace("bsd_penaty " + bsd_penaty.ToString());
                    enupdate["bsd_penaty"] = new Money(bsd_penaty);
                }
                else
                {
                    enupdate["bsd_penaty"] = new Money((bsd_spforfeiture / 100) * ((Money)op["bsd_totalamountlessfreight"]).Value);
                }
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
                if (op.Contains("bsd_signedcontractdate"))
                {
                    bsd_signedcontractdate = (DateTime)op["bsd_signedcontractdate"];
                    caseSign = 2;
                    if (op.Contains("bsd_signeddadate"))
                    {
                        bsd_signeddadate = (DateTime)op["bsd_signeddadate"];
                        caseSign = 3;
                    }
                }
                else
                {
                    if (op.Contains("bsd_signeddadate"))
                    {
                        bsd_signeddadate = (DateTime)op["bsd_signeddadate"];
                        caseSign = 1;
                    }
                    else
                    {
                        caseSign = 4;
                    }
                }

                var lateDays = ((int)((DateTime.UtcNow.AddHours(7) - bsd_duedate).TotalDays) - bsd_gracedays);
                var latedays2 = lateDays;
                var resCheckCaseSign = checkCaseSignAndCalLateDays(installment, op, bsd_signedcontractdate, bsd_signeddadate, DateTime.UtcNow.AddHours(7), ref lateDays);
                lateDays = latedays2 <= lateDays ? latedays2 : lateDays;
                #endregion
                if (resCheckCaseSign)
                {
                    tracingService.Trace($"resCheckCaseSign:true");
                    enupdate["bsd_overdue_interest"] = bsd_termsinterestpercentage / 100 * lateDays * (installment.Contains("bsd_balance") ? ((Money)installment["bsd_balance"]).Value : 1);
                    enupdate["bsd_overdue_interest_money"] = new Money((decimal)enupdate["bsd_overdue_interest"]);

                }
                tracingService.Trace($"bsd_termsinterestpercentage: {bsd_termsinterestpercentage}");
                tracingService.Trace($"bsd_gracedays: {bsd_gracedays}");
                tracingService.Trace($"bsd_duedate:{bsd_duedate}");
                tracingService.Trace($"today:{DateTime.UtcNow.AddHours(7)}");
                tracingService.Trace($"totalday: {((int)((DateTime.UtcNow.AddHours(7) - bsd_duedate).TotalDays) - bsd_gracedays)}");
                tracingService.Trace($"bsd_overdue_interest: {bsd_termsinterestpercentage * lateDays * (installment.Contains("bsd_balance") ? ((Money)installment["bsd_balance"]).Value : 1)}");
                tracingService.Trace($"bsd_overdue_interest1: {((Money)enupdate["bsd_overdue_interest"]).Value}");
                tracingService.Trace($"bsd_overdue_interest_money:{((Money)enupdate["bsd_overdue_interest_money"]).Value}");
                tracingService.Trace($"bsd_balance:{((Money)installment["bsd_balance"]).Value}");
                #endregion
            }
            service.Update(enupdate);
        }
        private EntityCollection get_pmSchDtl_fromOpentryID(Guid opID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[7]
            {
        "bsd_duedate",
        "statuscode",
        "bsd_balance",
        "bsd_actualgracedays",
        "bsd_amountofthisphase","bsd_name","bsd_interestchargeper"
            });
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object)opID));
            query.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000000));
            return this.service.RetrieveMultiple((QueryBase)query);
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
                DateTime dateTime = DateTime.Now;
                dateTime = dateTime.Date;
                if ((int)dateTime.Subtract(((DateTime)entity["bsd_duedate"]).Date).TotalDays > 60)
                {
                    installment = entity;
                    return true;
                }
            }
            return false;
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
