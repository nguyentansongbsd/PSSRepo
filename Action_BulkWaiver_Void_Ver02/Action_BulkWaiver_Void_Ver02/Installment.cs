// Decompiled with JetBrains decompiler
// Type: BSDLibrary.Installment
// Assembly: Action_BulkWaiver_Void, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75ea86a04a814594
// MVID: 3A65A6E8-584D-4322-BEC4-3FD1D76E2BF8
// Assembly location: C:\Users\BSD\Desktop\Action_BulkWaiver_Void.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;

namespace Action_BulkWaiver_Void_Ver02
{
    public class Installment
    {
        private Common common;
        private IOrganizationService service;
        private Entity enInstallment;

        public DateTime InterestStarDate { get; set; }

        public int Intereststartdatetype { get; set; }

        public int Gracedays { get; set; }

        public int LateDays { get; set; }

        public Decimal MaxPercent { get; set; }

        public Decimal MaxAmount { get; set; }

        public Decimal InterestPercent { get; set; }

        public DateTime Duedate { get; set; }

        public Installment(IOrganizationService service, Entity enInstallment)
        {
            this.service = service;
            this.common = new Common(service);
            this.enInstallment = service.Retrieve(enInstallment.LogicalName, enInstallment.Id, new ColumnSet(true));
            this.getInterestStartDate();
        }

        private void getInterestStartDate()
        {
            EntityReference entityReference1 = this.enInstallment.Contains("bsd_optionentry") ? (EntityReference)this.enInstallment["bsd_optionentry"] : (EntityReference)null;
            Entity entity1 = this.enInstallment.Contains("bsd_optionentry") ? this.service.Retrieve(entityReference1.LogicalName, entityReference1.Id, new ColumnSet(true)) : (Entity)null;
            EntityReference entityReference2 = entity1.Contains("bsd_paymentscheme") ? (EntityReference)entity1["bsd_paymentscheme"] : (EntityReference)null;
            Entity entity2 = this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true));
            EntityReference entityReference3 = entity2.Contains("bsd_interestratemaster") ? (EntityReference)entity2["bsd_interestratemaster"] : (EntityReference)null;
            if (entityReference3 == null)
                return;
            Entity entity3 = this.service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(true));
            this.Gracedays = entity3.Contains("bsd_gracedays") ? (int)entity3["bsd_gracedays"] : 0;
            this.Intereststartdatetype = ((OptionSetValue)entity3["bsd_intereststartdatetype"]).Value;
            this.Duedate = this.enInstallment.Contains("bsd_duedate") ? this.common.RetrieveLocalTimeFromUTCTime((DateTime)this.enInstallment["bsd_duedate"]) : new DateTime(0L);
            this.MaxPercent = entity3.Contains("bsd_toleranceinterestpercentage") ? (Decimal)entity3["bsd_toleranceinterestpercentage"] : 100M;
            this.MaxAmount = entity3.Contains("bsd_toleranceinterestamount") ? (Decimal)entity3["bsd_toleranceinterestamount"] : 0M;
            this.InterestPercent = entity3.Contains("bsd_termsinterestpercentage") ? (Decimal)entity3["bsd_termsinterestpercentage"] : 0M;
            this.InterestStarDate = this.Duedate.AddDays((double)(this.Gracedays + 1));
        }

        public int getLateDays(DateTime dateCalculate)
        {
            int totalDays = (int)dateCalculate.Date.Subtract(this.Duedate.Date).TotalDays;
            if (this.Intereststartdatetype == 100000001)
                totalDays -= this.Gracedays;
            return totalDays;
        }

        public void voidInstallment(Decimal bsd_waiveramount)
        {
            Decimal num1 = this.enInstallment.Contains("bsd_balance") ? ((Money)this.enInstallment["bsd_balance"]).Value : 0M;
            Decimal num2 = this.enInstallment.Contains("bsd_waiverinstallment") ? ((Money)this.enInstallment["bsd_waiverinstallment"]).Value : 0M;
            Decimal num3 = this.enInstallment.Contains(nameof(bsd_waiveramount)) ? ((Money)this.enInstallment[nameof(bsd_waiveramount)]).Value : 0M;
            Decimal num4 = num1 + bsd_waiveramount;
            Decimal num5 = num2 - bsd_waiveramount;
            Decimal num6 = num3 - bsd_waiveramount;
            this.service.Update(new Entity(this.enInstallment.LogicalName, this.enInstallment.Id)
            {
                ["bsd_balance"] = (object)new Money(num4),
                ["bsd_waiverinstallment"] = (object)new Money(num5),
                [nameof(bsd_waiveramount)] = (object)new Money(num6)
            });
        }

        public void voidInterest(Decimal bsd_waiveramount)
        {
            Decimal num1 = this.enInstallment.Contains("bsd_waiverinterest") ? ((Money)this.enInstallment["bsd_waiverinterest"]).Value : 0M;
            Decimal num2 = this.enInstallment.Contains(nameof(bsd_waiveramount)) ? ((Money)this.enInstallment[nameof(bsd_waiveramount)]).Value : 0M;
            Decimal num3 = num1 - bsd_waiveramount;
            Decimal num4 = num2 - bsd_waiveramount;
            this.service.Update(new Entity(this.enInstallment.LogicalName, this.enInstallment.Id)
            {
                ["bsd_waiverinterest"] = (object)new Money(num3),
                [nameof(bsd_waiveramount)] = (object)new Money(num4)
            });
        }

        public void voidManagementFee(Decimal bsd_waiveramount)
        {
            Decimal num1 = this.enInstallment.Contains("bsd_managementfeewaiver") ? ((Money)this.enInstallment["bsd_managementfeewaiver"]).Value : 0M;
            Decimal num2 = this.enInstallment.Contains(nameof(bsd_waiveramount)) ? ((Money)this.enInstallment[nameof(bsd_waiveramount)]).Value : 0M;
            Decimal num3 = num1 - bsd_waiveramount;
            Decimal num4 = num2 - bsd_waiveramount;
            this.service.Update(new Entity(this.enInstallment.LogicalName, this.enInstallment.Id)
            {
                ["bsd_managementfeewaiver"] = (object)new Money(num3),
                [nameof(bsd_waiveramount)] = (object)new Money(num4)
            });
        }

        public void voidMaintenanceFee(Decimal bsd_waiveramount)
        {
            Decimal num1 = this.enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money)this.enInstallment["bsd_maintenancefeewaiver"]).Value : 0M;
            Decimal num2 = this.enInstallment.Contains(nameof(bsd_waiveramount)) ? ((Money)this.enInstallment[nameof(bsd_waiveramount)]).Value : 0M;
            Decimal num3 = num1 - bsd_waiveramount;
            Decimal num4 = num2 - bsd_waiveramount;
            this.service.Update(new Entity(this.enInstallment.LogicalName, this.enInstallment.Id)
            {
                ["bsd_maintenancefeewaiver"] = (object)new Money(num3),
                [nameof(bsd_waiveramount)] = (object)new Money(num4)
            });
        }

        public Entity getLastConfirmPayment(int bsd_paymenttype)
        {
            Guid id = this.enInstallment.Id;
            int num1 = bsd_paymenttype;
            int num2 = 100000000;
            QueryExpression queryExpression = new QueryExpression("bsd_payment");
            queryExpression.ColumnSet.AllColumns = true;
            queryExpression.AddOrder("bsd_paymentactualtime", (OrderType)1);
            queryExpression.Criteria.AddCondition("bsd_paymentschemedetail", (ConditionOperator)0, new object[1]
            {
        (object) id
            });
            queryExpression.Criteria.AddCondition(nameof(bsd_paymenttype), (ConditionOperator)0, new object[1]
            {
        (object) num1
            });
            queryExpression.Criteria.AddCondition("statuscode", (ConditionOperator)0, new object[1]
            {
        (object) num2
            });
            EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)queryExpression);
            return ((Collection<Entity>)entityCollection.Entities).Count > 0 ? ((Collection<Entity>)entityCollection.Entities)[0] : (Entity)null;
        }

        public Entity getLastConfirmTransactionPayment(int bsd_transactiontype, int bsd_feetype)
        {
            Guid id = this.enInstallment.Id;
            int num1 = 100000000;
            int num2 = bsd_transactiontype;
            QueryExpression queryExpression = new QueryExpression("bsd_transactionpayment");
            queryExpression.ColumnSet.AllColumns = true;
            queryExpression.AddOrder("createdon", (OrderType)1);
            queryExpression.Criteria.AddCondition("bsd_installment", (ConditionOperator)0, new object[1]
            {
        (object) id
            });
            queryExpression.Criteria.AddCondition("statuscode", (ConditionOperator)0, new object[1]
            {
        (object) num1
            });
            queryExpression.Criteria.AddCondition(nameof(bsd_transactiontype), (ConditionOperator)0, new object[1]
            {
        (object) num2
            });
            if (bsd_feetype != 0)
                queryExpression.Criteria.AddCondition(nameof(bsd_feetype), (ConditionOperator)0, new object[1]
                {
          (object) bsd_feetype
                });
            EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)queryExpression);
            return ((Collection<Entity>)entityCollection.Entities).Count > 0 ? ((Collection<Entity>)entityCollection.Entities)[0] : (Entity)null;
        }
    }
}
