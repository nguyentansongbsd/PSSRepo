﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Update_Installment
{
    public class Plugin_Update_Installment_UpdatePaymentNotices : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        private Entity GetPaymentNoticesByInstalment(string  instalmentId,string OPId)
        {
            var query = new QueryExpression("bsd_customernotices");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, OPId);
            query.Criteria.AddCondition("bsd_paymentschemedetail", ConditionOperator.Equal, instalmentId);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            var rs = service.RetrieveMultiple(query);
            if (rs.Entities.Count > 0) return rs[0];
            return null;

        }
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity entity = (Entity)context.InputParameters["Target"];
            Entity enTarget = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            tracingService.Trace("start id:" + entity.Id);
            #region tìm đến PaymentNotices được map
            // lấy Option Entry
            if (!enTarget.Contains("bsd_optionentry")) return;
            EntityReference enOPRef = (EntityReference)enTarget["bsd_optionentry"];
            Entity EnOP = service.Retrieve(enOPRef.LogicalName, enOPRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
           
            //lấy ra PaymentNotices hiện hữu <tại một thời điểm chỉ có một paymentnotice-OP-Ins trạng thái Generate(1)>
            Entity enCreated = GetPaymentNoticesByInstalment(enTarget.Id.ToString(),EnOP.Id.ToString());
            if (enCreated == null) return;
            #endregion
            tracingService.Trace("paymentnotices: "+enCreated.Id.ToString());
            Entity enUpdate = new Entity(enCreated.LogicalName, enCreated.Id);
            //lấy Instalment detail 
            EntityReference enInsDetailRef = (EntityReference)enCreated["bsd_paymentschemedetail"];
            Entity enInsDetail = service.Retrieve(enInsDetailRef.LogicalName, enInsDetailRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            
            #region Amount in EDA (bsd_amountofthisphase) mapping từ Instalment của đợt làm payment notices
            enUpdate["bsd_amountofthisphase"] = enInsDetail.Contains("bsd_amountofthisphase") ? enInsDetail["bsd_amountofthisphase"]: new Money(0);
            #endregion
            tracingService.Trace("1");
            #region bsd_totaladvancepayment (Số tiền thanh toán trước)-->Field tính toán (Mapping từ option entry)-->Field tính toán
            enUpdate["bsd_totaladvancepayment"] = EnOP.Contains("bsd_totaladvancepayment")? EnOP["bsd_totaladvancepayment"]:new Money(0);
            #endregion
            tracingService.Trace("2");
            #region (Tổng số tiền thanh toán trước)-->bsd_totalprepaymentamount=bsd_totaladvancepayment+ bsd_amountwaspaid (của đợt paymnent notices)-->Hiển thị số âm (Field tính toán)
            if (enInsDetail.Contains("bsd_amountwaspaid"))
            {
                enUpdate["bsd_totalprepaymentamount"] = new Money(((Money)enUpdate["bsd_totaladvancepayment"]).Value + ((Money)enInsDetail["bsd_amountwaspaid"]).Value);

            }
            else
            {
                enUpdate["bsd_totalprepaymentamount"] = enUpdate["bsd_totaladvancepayment"];

            }
            #endregion                        tracingService.Trace("1");

            tracingService.Trace("3");
                #region Shortfall in previous Installment (Số tiền chưa thanh toán các đợt trước) (bsd_Shoftfall Installment= Sum(bsd_balance) Tổng số tiền chưa thanh toán của các đợt trước đợt làm payment notice -->Field tính toán
            decimal sum_bsd_balance = 0;
            int orderNumberInsDetail = (int)enInsDetail["bsd_ordernumber"];
            //query lấy ra các đợt trước đợt PaymentNotices
            var query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, EnOP.Id.ToString());
            query.Criteria.AddCondition("bsd_ordernumber", ConditionOperator.LessThan, orderNumberInsDetail);
            var insDetailList = service.RetrieveMultiple(query);
            if (insDetailList.Entities.Count > 0)
            {
                foreach (var item in insDetailList.Entities)
                {
                    if (!item.Contains("bsd_balance")) continue;
                    sum_bsd_balance += ((Money)item["bsd_balance"]).Value;
                }
            }
            enUpdate["bsd_shortfallinpreviousinstallment"] = new Money(sum_bsd_balance);
            #endregion                        tracingService.Trace("1");
            tracingService.Trace("4");

            #region Amount to transfer (Số tiền phải chuyển)=bsd_totaladvancepayment+Totalprepaymentamount+bsd_Shoftfall Installment -->Field tính toán
            enUpdate["bsd_amounttotransfer"] = new Money(
                ((Money)enUpdate["bsd_amountofthisphase"]).Value -
                ((Money)enUpdate["bsd_totalprepaymentamount"]).Value +
                ((Money)enUpdate["bsd_shortfallinpreviousinstallment"]).Value
                );
            #endregion
            tracingService.Trace("5");

            service.Update(enUpdate);
            #region lấy các paymentNotice khác của hợp đồng và cập nhật lại luôn
            UpdatePaymentNoticeOther(EnOP, orderNumberInsDetail);
            #endregion

        }
        public void UpdatePaymentNoticeOther(Entity EnOP,int orderNumberInsDetail)
        {
            var query = new QueryExpression("bsd_customernotices");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, EnOP.Id.ToString());
            var nDetailList = service.RetrieveMultiple(query);
            foreach(var item in nDetailList.Entities)
            {
                query = new QueryExpression("bsd_paymentschemedetail");
                query.ColumnSet.AllColumns = true;
                query.Criteria.AddCondition("bsd_paymentschemedetailid", ConditionOperator.Equal, ((EntityReference)item["bsd_paymentschemedetail"]).Id.ToString());
                query.Criteria.AddCondition("bsd_ordernumber", ConditionOperator.GreaterThan, orderNumberInsDetail);
                var rs= service.RetrieveMultiple(query);
                if(rs.Entities.Count > 0)
                {
                    Entity enTarget= (Entity)rs.Entities[0];
                    //lấy ra PaymentNotices hiện hữu <tại một thời điểm chỉ có một paymentnotice-OP-Ins trạng thái Generate(1)>
                    Entity enCreated = GetPaymentNoticesByInstalment(enTarget.Id.ToString(), EnOP.Id.ToString());
                    if (enCreated == null) return;
                    tracingService.Trace("paymentnotices_: " + enCreated.Id.ToString());
                    Entity enUpdate = new Entity(enCreated.LogicalName, enCreated.Id);
                    //lấy Instalment detail 
                    EntityReference enInsDetailRef = (EntityReference)enCreated["bsd_paymentschemedetail"];
                    Entity enInsDetail = service.Retrieve(enInsDetailRef.LogicalName, enInsDetailRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                    #region Amount in EDA (bsd_amountofthisphase) mapping từ Instalment của đợt làm payment notices
                    enUpdate["bsd_amountofthisphase"] = enInsDetail.Contains("bsd_amountofthisphase") ? enInsDetail["bsd_amountofthisphase"] : new Money(0);
                    #endregion
                    tracingService.Trace("1_");
                    #region bsd_totaladvancepayment (Số tiền thanh toán trước)-->Field tính toán (Mapping từ option entry)-->Field tính toán
                    enUpdate["bsd_totaladvancepayment"] = EnOP.Contains("bsd_totaladvancepayment") ? EnOP["bsd_totaladvancepayment"] : new Money(0);
                    #endregion
                    tracingService.Trace("2_");
                    #region (Tổng số tiền thanh toán trước)-->bsd_totalprepaymentamount=bsd_totaladvancepayment+ bsd_amountwaspaid (của đợt paymnent notices)-->Hiển thị số âm (Field tính toán)
                    if (enInsDetail.Contains("bsd_amountwaspaid"))
                    {
                        enUpdate["bsd_totalprepaymentamount"] = new Money(((Money)enUpdate["bsd_totaladvancepayment"]).Value + ((Money)enInsDetail["bsd_amountwaspaid"]).Value);

                    }
                    else
                    {
                        enUpdate["bsd_totalprepaymentamount"] = enUpdate["bsd_totaladvancepayment"];

                    }
                    #endregion                        tracingService.Trace("1");

                    tracingService.Trace("3_");
                    #region Shortfall in previous Installment (Số tiền chưa thanh toán các đợt trước) (bsd_Shoftfall Installment= Sum(bsd_balance) Tổng số tiền chưa thanh toán của các đợt trước đợt làm payment notice -->Field tính toán
                    decimal sum_bsd_balance = 0;
                    int orderNumberInsDetailCurent = (int)enInsDetail["bsd_ordernumber"];
                    //query lấy ra các đợt trước đợt PaymentNotices
                    var query2 = new QueryExpression("bsd_paymentschemedetail");
                    query2.ColumnSet.AllColumns = true;
                    query2.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, EnOP.Id.ToString());
                    query2.Criteria.AddCondition("bsd_ordernumber", ConditionOperator.LessThan, orderNumberInsDetailCurent);
                    var insDetailList = service.RetrieveMultiple(query2);
                    if (insDetailList.Entities.Count > 0)
                    {
                        foreach (var item2 in insDetailList.Entities)
                        {
                            if (!item2.Contains("bsd_balance")) continue;
                            sum_bsd_balance += ((Money)item2["bsd_balance"]).Value;
                        }
                    }
                    enUpdate["bsd_shortfallinpreviousinstallment"] = new Money(sum_bsd_balance);
                    #endregion                        
                    tracingService.Trace("4_");

                    #region Amount to transfer (Số tiền phải chuyển)=bsd_totaladvancepayment+Totalprepaymentamount+bsd_Shoftfall Installment -->Field tính toán
                    enUpdate["bsd_amounttotransfer"] = new Money(
                        ((Money)enUpdate["bsd_amountofthisphase"]).Value -
                        ((Money)enUpdate["bsd_totalprepaymentamount"]).Value +
                        ((Money)enUpdate["bsd_shortfallinpreviousinstallment"]).Value
                        );
                    #endregion
                    tracingService.Trace("5_");

                    service.Update(enUpdate);
                }
            }    
        }
    }
}
