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
        Entity enCreated = null;
        EntityCollection dtlFromOpentryId = null;
        List<string> lstInstallments = new List<string>();
        int num2 = 0;
        Entity op = null;
        bool isMapWN = false;
        Entity FUL = null;
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
            #region mapping 
            tracingService.Trace("step mapping");
            Entity enPro = service.Retrieve("bsd_project", ((EntityReference)enCreated["bsd_project"]).Id, new ColumnSet(true));
            Entity enDev = service.Retrieve("account", ((EntityReference)enPro["bsd_investor"]).Id, new ColumnSet(true));
            Entity enUpdate = new Entity("bsd_terminateletter", enCreated.Id);
            enUpdate["bsd_accountnameother_develop"] = enDev.Contains("bsd_accountnameother") ? enDev["bsd_accountnameother"] : enDev["bsd_name"];
            service.Update(enUpdate);
            #endregion
            if (!enCreated.Contains("bsd_optionentry")) return;
            if (!enCreated.Contains("bsd_followuplist")) return;

            var opRef = (EntityReference)enCreated["bsd_optionentry"];
            op = service.Retrieve(opRef.LogicalName, opRef.Id, new ColumnSet(true));
            var query_bsd_optionentry = opRef.Id.ToString();
            var FULRef = enCreated.Contains("bsd_followuplist") ? (EntityReference)enCreated["bsd_followuplist"] : null;
            FUL = FULRef != null ? service.Retrieve(FULRef.LogicalName, FULRef.Id, new ColumnSet(true)) : null;
            tracingService.Trace("Plugin_Create_TerminationLetter" + "opRef.Id: " + opRef.Id.ToString());

            var enupdate = new Entity("bsd_terminateletter", enCreated.Id);

            dtlFromOpentryId = this.get_pmSchDtl_fromOpentryID(opRef.Id);
            tracingService.Trace(dtlFromOpentryId.Entities.Count.ToString());
            foreach (var item in dtlFromOpentryId.Entities)
            {

                tracingService.Trace($"step 2.4 {item.Id}-------------------------------");


                tracingService.Trace($"entity bsd_name: {item["bsd_name"]}");
                if (item.Contains("bsd_gracedays"))
                    tracingService.Trace($"entity bsd_gracedays: {item["bsd_gracedays"]}");
                tracingService.Trace($"lstInstallments:{string.Join(",", lstInstallments)}");
                TinhLai(enupdate);
            }
            enupdate["bsd_penaty"] = enCreated["bsd_totalforfeitureamount"];
            tracingService.Trace("bsd_penaty " + enupdate["bsd_penaty"].ToString());

            enupdate["bsd_terminatefee"] = new Money(((Money)enCreated["bsd_totalforfeitureamount"]).Value - ((Money)enCreated["bsd_terminatefeewaiver"]).Value);
            service.Update(enupdate);
        }
        private void TinhLai(Entity enupdate)
        {
            var installment = new Entity("bsd_paymentschemedetail");
            if (this.check_overDuedate_pmShcDtl(dtlFromOpentryId, ref installment))
            {
                if (!isMapWN)
                {
                    var query = new QueryExpression("bsd_warningnotices");
                    query.TopCount = 2; query.ColumnSet.AllColumns = true;
                    query.Criteria.AddCondition("bsd_paymentschemedeitail", ConditionOperator.Equal, installment.Id.ToString());
                    query.AddOrder("createdon", OrderType.Descending);
                    var result = service.RetrieveMultiple(query);
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
                    enupdate["bsd_insallment"] = installment.ToEntityReference();
                    isMapWN = true;
                }

                #region tính Penaty  
                var en_bsd_paymentscheme = this.service.Retrieve("bsd_paymentscheme",
                ((EntityReference)op["bsd_paymentscheme"]).Id, new ColumnSet(true));
                var bsd_totalpercent = ((decimal)op["bsd_totalpercent"]);
                var bsd_spforfeiture = (decimal)en_bsd_paymentscheme["bsd_spforfeiture"];
                //if (bsd_totalpercent <= bsd_spforfeiture)
                //{
                //    var bsd_penaty = ((Money)op["bsd_depositamount"]).Value;
                //    var installments = get_all_pmSchDtl_fromOpentryID(op.Id);
                //    for (var i = 0; i < installments.Entities.Count; i++)
                //    {
                //        tracingService.Trace($"{installments[i].Id}");
                //        var bsd_amountwaspaid = installments[i].Contains("bsd_amountwaspaid") ? ((Money)installments[i]["bsd_amountwaspaid"]).Value : 0;
                //        tracingService.Trace($"{bsd_amountwaspaid}");
                //        bsd_penaty += bsd_amountwaspaid;
                //    }
                //    tracingService.Trace("bsd_penaty " + bsd_penaty.ToString());

                //    enupdate["bsd_penaty"] = new Money(bsd_penaty);
                //    tracingService.Trace("bsd_penaty " + bsd_penaty.ToString());
                //}
                //else
                //{
                //    enupdate["bsd_penaty"] = new Money((bsd_spforfeiture / 100) * ((Money)op["bsd_totalamountlessfreight"]).Value);
                //}
                enupdate["bsd_penaty"] = new Money(((Money)op["bsd_totalamountlessfreight"]).Value*(20/100));
                #endregion
                #region  Overdue Interest
                tracingService.Trace("installment name = " + installment["bsd_name"].ToString());
                var en_bsd_interestratemaster = this.service.Retrieve("bsd_interestratemaster",
                    ((EntityReference)en_bsd_paymentscheme["bsd_interestratemaster"]).Id, new ColumnSet(true));
                var bsd_termsinterestpercentage = (decimal)installment["bsd_interestchargeper"];
                var bsd_gracedays = (int)en_bsd_interestratemaster["bsd_gracedays"];
                var bsd_duedate = ((DateTime)installment["bsd_duedate"]).AddHours(7);
                tracingService.Trace(bsd_duedate.ToString());
                tracingService.Trace(bsd_gracedays.ToString());

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
                tracingService.Trace("4.1");
                var lateDays = ((int)((((DateTime)FUL["bsd_date"]).AddHours(7) - bsd_duedate).TotalDays) - bsd_gracedays);
                var latedays2 = lateDays;
                var resCheckCaseSign = checkCaseSignAndCalLateDays(installment, op, bsd_signedcontractdate, bsd_signeddadate, (DateTime)FUL["bsd_date"], ref lateDays);
                tracingService.Trace("step 4.2");
                lateDays = latedays2 <= lateDays ? latedays2 : lateDays;
                tracingService.Trace($"lateDays: {lateDays}");
                #endregion
                if (resCheckCaseSign)
                {
                    tracingService.Trace($"resCheckCaseSign:true");
                    if (enupdate.Contains("bsd_overdue_interest") == false)
                        enupdate["bsd_overdue_interest"] = new decimal(0);
                    tracingService.Trace("step 4.2.1");
                    var bsd_overdue_interest = (decimal)enupdate["bsd_overdue_interest"] + (decimal)
                        (bsd_termsinterestpercentage / 100 * lateDays * (installment.Contains("bsd_balance") ? ((Money)installment["bsd_balance"]).Value : new decimal(1)));
                    tracingService.Trace(bsd_overdue_interest.ToString());
                    if (bsd_overdue_interest >= 0)
                    {
                        tracingService.Trace("step 4.3");
                        enupdate["bsd_overdue_interest"] = bsd_overdue_interest;
                        enupdate["bsd_overdue_interest_money"] = new Money(((decimal)enupdate["bsd_overdue_interest"]));
                        var bsd_overdue_interest2 = ((Money)enupdate["bsd_overdue_interest_money"]).Value;
                        #region Cộng thêm lãi chưa thanh toán các đợt trước đó nếu có
                        var query_bsd_ordernumber = (int)installment["bsd_ordernumber"];
                        var query_bsd_optionentry = installment.GetAttributeValue<EntityReference>("bsd_optionentry").Id.ToString();
                        tracingService.Trace("query_bsd_ordernumber: " + query_bsd_ordernumber);
                        tracingService.Trace("query_bsd_optionentry: "+query_bsd_optionentry.ToString());
                        tracingService.Trace("lấy các đợt trước đó để + thêm lãi chưa thanh toán cho bsd_overdue_interest");
                        var query = new QueryExpression("bsd_paymentschemedetail")
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
                        var rsIns =service.RetrieveMultiple((QueryBase)query);
                        foreach (var rs in rsIns.Entities) 
                        {
                            tracingService.Trace("ordernumber: " + ((int)rs["bsd_ordernumber"]));
                            var bsd_interestchargeremaining = rs.Contains("bsd_interestchargeremaining") ? ((Money)rs["bsd_interestchargeremaining"]).Value : 0;
                            tracingService.Trace("bsd_interestchargeremaining: " + bsd_interestchargeremaining.ToString());
                            bsd_overdue_interest2 += bsd_interestchargeremaining;
                        }
                        enupdate["bsd_overdue_interest_money"] = new Money(bsd_overdue_interest2);
                        tracingService.Trace("bsd_overdue_interest_money " + ((Money)enupdate["bsd_overdue_interest_money"]).Value.ToString());


                        tracingService.Trace("check và cộng thêm lãi đợt đang xét nếu có");
                        var bsd_interestchargeremaining_current = installment.Contains("bsd_interestchargeremaining") ? ((Money)installment["bsd_interestchargeremaining"]).Value : 0;
                        tracingService.Trace("bsd_interestchargeremaining_current: " + bsd_interestchargeremaining_current.ToString());
                        bsd_overdue_interest2 += bsd_interestchargeremaining_current;
                        enupdate["bsd_overdue_interest_money"] = new Money(bsd_overdue_interest2);
                        tracingService.Trace("bsd_overdue_interest_money " + ((Money)enupdate["bsd_overdue_interest_money"]).Value.ToString());
                        #endregion
                        tracingService.Trace("bsd_overdue_interest " + ((decimal)enupdate["bsd_overdue_interest"]).ToString());
                    }

                }
                #endregion
            }

        }
        private EntityCollection get_pmSchDtl_fromOpentryID(Guid opID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[9]
            {
        "bsd_duedate",
        "statuscode",
        "bsd_balance",
        "bsd_actualgracedays",
        "bsd_amountofthisphase","bsd_name","bsd_interestchargeper","bsd_ordernumber","bsd_optionentry"
            });
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object)opID));
            query.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 100000000));
            query.AddOrder("bsd_duedate", OrderType.Descending);
            return this.service.RetrieveMultiple((QueryBase)query);
        }

        public bool checkCaseSignAndCalLateDays(Entity enInstallment, Entity enOptionEntry, DateTime bsd_signedcontractdate, DateTime bsd_signeddadate, DateTime receiptdate, ref int lateDays)
        {
            tracingService.Trace("checkCaseSignAndCalLateDays");
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
            tracingService.Trace("checkCaseSignAndCalLateDays 1");

            #region lấy ra đợt tích Sign Contract Installment
            var query_bsd_signcontractinstallment = true;

            var query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_signcontractinstallment", ConditionOperator.Equal, query_bsd_signcontractinstallment);
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enOptionEntry.Id.ToString());
            tracingService.Trace("checkCaseSignAndCalLateDays 2");

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
                        //if (rs.Entities[0].Id == enInstallment.Id)
                        //{
                        //    result = false;
                        //    break;
                        //}
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
            tracingService.Trace("check_overDuedate_pmShcDtl");
            for (int index = 0; index < entCll.Entities.Count; ++index)
            {

                Entity entity = entCll.Entities[index];
                tracingService.Trace($"{entity["bsd_name"]}");
                if (entity.Contains("bsd_duedate") == false)
                    continue;
                DateTime dateTime = (DateTime)FUL["bsd_date"];
                dateTime = dateTime.Date;
                tracingService.Trace($"dateTime: {dateTime.ToString()}");
                if (entity.Contains("bsd_gracedays"))
                    dateTime = dateTime.AddDays(((int)entity["bsd_gracedays"]));
                if ((int)dateTime.Subtract(((DateTime)entity["bsd_duedate"]).Date).TotalDays > 0 && (lstInstallments.Contains(entity.Id.ToString()) == false))
                {
                    tracingService.Trace(entity.Id.ToString());
                    installment = entity;
                    lstInstallments.Add(entity.Id.ToString());
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
