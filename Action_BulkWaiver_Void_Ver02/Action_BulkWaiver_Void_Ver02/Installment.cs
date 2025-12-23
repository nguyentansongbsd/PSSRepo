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

        public decimal MaxPercent { get; set; }

        public decimal MaxAmount { get; set; }

        public decimal InterestPercent { get; set; }

        public DateTime Duedate { get; set; }

        public Installment(IOrganizationService service, Entity enInstallment)
        {
            service = service;
            common = new Common(service);
            enInstallment = service.Retrieve(enInstallment.LogicalName, enInstallment.Id, new ColumnSet(true));
            getInterestStartDate();
        }

        private void getInterestStartDate()
        {
            EntityReference entityReference1 = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : (EntityReference)null;
            Entity entity1 = enInstallment.Contains("bsd_optionentry") ? service.Retrieve(entityReference1.LogicalName, entityReference1.Id, new ColumnSet(true)) : (Entity)null;
            EntityReference entityReference2 = entity1.Contains("bsd_paymentscheme") ? (EntityReference)entity1["bsd_paymentscheme"] : (EntityReference)null;
            Entity entity2 = service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true));
            EntityReference entityReference3 = entity2.Contains("bsd_interestratemaster") ? (EntityReference)entity2["bsd_interestratemaster"] : (EntityReference)null;
            if (entityReference3 == null)
                return;
            Entity entity3 = service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(true));
            Gracedays = entity3.Contains("bsd_gracedays") ? (int)entity3["bsd_gracedays"] : 0;
            Intereststartdatetype = ((OptionSetValue)entity3["bsd_intereststartdatetype"]).Value;
            Duedate = enInstallment.Contains("bsd_duedate") ? common.RetrieveLocalTimeFromUTCTime((DateTime)enInstallment["bsd_duedate"]) : new DateTime(0L);
            MaxPercent = entity3.Contains("bsd_toleranceinterestpercentage") ? (decimal)entity3["bsd_toleranceinterestpercentage"] : 100M;
            MaxAmount = entity3.Contains("bsd_toleranceinterestamount") ? (decimal)entity3["bsd_toleranceinterestamount"] : 0M;
            InterestPercent = entity3.Contains("bsd_termsinterestpercentage") ? (decimal)entity3["bsd_termsinterestpercentage"] : 0M;
            InterestStarDate = Duedate.AddDays((double)(Gracedays + 1));
        }

        public int getLateDays(DateTime dateCalculate)
        {
            int totalDays = (int)dateCalculate.Date.Subtract(Duedate.Date).TotalDays;
            if (Intereststartdatetype == 100000001)
                totalDays -= Gracedays;
            return totalDays;
        }

        public void voidInstallment(decimal bsd_waiveramount)
        {
            decimal num1 = enInstallment.Contains("bsd_balance") ? ((Money)enInstallment["bsd_balance"]).Value : 0M;
            decimal num2 = enInstallment.Contains("bsd_waiverinstallment") ? ((Money)enInstallment["bsd_waiverinstallment"]).Value : 0M;
            decimal num3 = enInstallment.Contains(nameof(bsd_waiveramount)) ? ((Money)enInstallment[nameof(bsd_waiveramount)]).Value : 0M;
            decimal num4 = num1 + bsd_waiveramount;
            decimal num5 = num2 - bsd_waiveramount;
            decimal num6 = num3 - bsd_waiveramount;
            Entity enUp = new Entity(enInstallment.LogicalName, enInstallment.Id);
            enUp["bsd_balance"] = new Money(num4);
            enUp["bsd_waiverinstallment"] = new Money(num5);
            enUp[nameof(bsd_waiveramount)] = new Money(num6);
            if (num4 == 0)
            {
                enUp["statuscode"] = new OptionSetValue(100000001);//--> PAID
                decimal waiverinterest = enInstallment.Contains("bsd_waiverinterest") ? ((Money)enInstallment["bsd_waiverinterest"]).Value : 0;
                decimal interestamount = enInstallment.Contains("bsd_interestchargeamount") ? ((Money)enInstallment["bsd_interestchargeamount"]).Value : 0;
                decimal interestpaid = enInstallment.Contains("bsd_interestwaspaid") ? ((Money)enInstallment["bsd_interestwaspaid"]).Value : 0;
                decimal balace_Interest = interestamount - interestpaid - waiverinterest;
                if (balace_Interest == 0)
                    enUp["bsd_interestchargestatus"] = new OptionSetValue(100000001);//--> PAID
            }
            else
            {
                enUp["statuscode"] = new OptionSetValue(100000000);
                enUp["bsd_interestchargestatus"] = new OptionSetValue(100000000);//--> NOT PAID
            }
            service.Update(enUp);
        }

        public void voidInterest(decimal bsd_waiveramount)
        {
            int statuscode = enInstallment.Contains("statuscode") ? ((OptionSetValue)enInstallment["statuscode"]).Value : 0;
            decimal num1 = enInstallment.Contains("bsd_waiverinterest") ? ((Money)enInstallment["bsd_waiverinterest"]).Value : 0M;
            decimal num2 = enInstallment.Contains(nameof(bsd_waiveramount)) ? ((Money)enInstallment[nameof(bsd_waiveramount)]).Value : 0M;
            decimal num3 = num1 - bsd_waiveramount;
            decimal num4 = num2 - bsd_waiveramount;
            Entity enUp = new Entity(enInstallment.LogicalName, enInstallment.Id);
            enUp["bsd_waiverinterest"] = new Money(num3);
            enUp[nameof(bsd_waiveramount)] = new Money(num4);
            if (statuscode == 100000001)
            {
                decimal interestamount = enInstallment.Contains("bsd_interestchargeamount") ? ((Money)enInstallment["bsd_interestchargeamount"]).Value : 0;
                decimal interestpaid = enInstallment.Contains("bsd_interestwaspaid") ? ((Money)enInstallment["bsd_interestwaspaid"]).Value : 0;
                decimal balace_Interest = interestamount - interestpaid - num3;
                if (balace_Interest == 0)
                    enUp["bsd_interestchargestatus"] = new OptionSetValue(100000001);//--> PAID
            }
            else
            {
                enUp["bsd_interestchargestatus"] = new OptionSetValue(100000000);//--> NOT PAID
            }
            service.Update(enUp);
        }

        public void voidManagementFee(decimal bsd_waiveramount)
        {
            decimal num1 = enInstallment.Contains("bsd_managementfeewaiver") ? ((Money)enInstallment["bsd_managementfeewaiver"]).Value : 0M;
            decimal num2 = enInstallment.Contains(nameof(bsd_waiveramount)) ? ((Money)enInstallment[nameof(bsd_waiveramount)]).Value : 0M;
            decimal num3 = num1 - bsd_waiveramount;
            decimal num4 = num2 - bsd_waiveramount;
            Entity enUp = new Entity(enInstallment.LogicalName, enInstallment.Id);
            enUp["bsd_managementfeewaiver"] = new Money(num3);
            enUp[nameof(bsd_waiveramount)] = new Money(num4);

            decimal managementamount = enInstallment.Contains("bsd_managementamount") ? ((Money)enInstallment["bsd_managementamount"]).Value : 0;
            decimal managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? ((Money)enInstallment["bsd_managementfeepaid"]).Value : 0;
            decimal balancemanagement = managementamount - managementfeepaid - num3;
            if (balancemanagement == 0)
            {
                enUp["bsd_managementfeesstatus"] = true;//--> PAID
            }
            else
            {
                if (balancemanagement < 0)
                {
                    throw new InvalidPluginExecutionException("Cannot set Waiver Amount bigger than the balance in Management fees: " + enInstallment["bsd_name"]);
                }
                enUp["bsd_managementfeesstatus"] = false;//--> NOT PAID
            }
            service.Update(enUp);
        }

        public void voidMaintenanceFee(decimal bsd_waiveramount)
        {
            decimal num1 = enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money)enInstallment["bsd_maintenancefeewaiver"]).Value : 0M;
            decimal num2 = enInstallment.Contains(nameof(bsd_waiveramount)) ? ((Money)enInstallment[nameof(bsd_waiveramount)]).Value : 0M;
            decimal num3 = num1 - bsd_waiveramount;
            decimal num4 = num2 - bsd_waiveramount;

            Entity enUp = new Entity(enInstallment.LogicalName, enInstallment.Id);
            enUp["bsd_maintenancefeewaiver"] = new Money(num3);
            enUp[nameof(bsd_waiveramount)] = new Money(num4);

            decimal maintenanceamount = enInstallment.Contains("bsd_maintenanceamount") ? ((Money)enInstallment["bsd_maintenanceamount"]).Value : 0;
            decimal maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? ((Money)enInstallment["bsd_maintenancefeepaid"]).Value : 0;
            decimal balancemaintence = maintenanceamount - maintenancefeepaid - num3;
            if (balancemaintence == 0)
            {
                enUp["bsd_maintenancefeesstatus"] = true;//--> PAID
            }
            else
            {
                if (balancemaintence < 0)
                {
                    throw new InvalidPluginExecutionException("Cannot set Waiver Amount bigger than the balance in Maintenance fees: " + enInstallment["bsd_name"]);
                }
                enUp["bsd_maintenancefeesstatus"] = false;//--> NOT PAID
            }
            service.Update(enUp);
        }

        public Entity getLastConfirmPayment(int bsd_paymenttype)
        {
            Guid id = enInstallment.Id;
            int num1 = bsd_paymenttype;
            int num2 = 100000000;
            QueryExpression queryExpression = new QueryExpression("bsd_payment");
            queryExpression.ColumnSet.AllColumns = true;
            queryExpression.AddOrder("bsd_paymentactualtime", (OrderType)1);
            queryExpression.Criteria.AddCondition("bsd_paymentschemedetail", (ConditionOperator)0, new object[1]
            {
         id
            });
            queryExpression.Criteria.AddCondition(nameof(bsd_paymenttype), (ConditionOperator)0, new object[1]
            {
         num1
            });
            queryExpression.Criteria.AddCondition("statuscode", (ConditionOperator)0, new object[1]
            {
         num2
            });
            EntityCollection entityCollection = service.RetrieveMultiple((QueryBase)queryExpression);
            return ((Collection<Entity>)entityCollection.Entities).Count > 0 ? ((Collection<Entity>)entityCollection.Entities)[0] : (Entity)null;
        }

        public Entity getLastConfirmTransactionPayment(int bsd_transactiontype, int bsd_feetype)
        {
            Guid id = enInstallment.Id;
            int num1 = 100000000;
            int num2 = bsd_transactiontype;
            QueryExpression queryExpression = new QueryExpression("bsd_transactionpayment");
            queryExpression.ColumnSet.AllColumns = true;
            queryExpression.AddOrder("createdon", (OrderType)1);
            queryExpression.Criteria.AddCondition("bsd_installment", (ConditionOperator)0, new object[1]
            {
         id
            });
            queryExpression.Criteria.AddCondition("statuscode", (ConditionOperator)0, new object[1]
            {
         num1
            });
            queryExpression.Criteria.AddCondition(nameof(bsd_transactiontype), (ConditionOperator)0, new object[1]
            {
         num2
            });
            if (bsd_feetype != 0)
                queryExpression.Criteria.AddCondition(nameof(bsd_feetype), (ConditionOperator)0, new object[1]
                {
           bsd_feetype
                });
            EntityCollection entityCollection = service.RetrieveMultiple((QueryBase)queryExpression);
            return ((Collection<Entity>)entityCollection.Entities).Count > 0 ? ((Collection<Entity>)entityCollection.Entities)[0] : (Entity)null;
        }
    }
}
