// Decompiled with JetBrains decompiler
// Type: BSDLibrary.Installment
// Assembly: Action_SignedContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=91af1975bd46f505
// MVID: 64A057F8-04D7-4937-A84E-D4EF3DDC89DB
// Assembly location: C:\Users\ngoct\Downloads\Action_SignedContract_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BSDLibrary
{
  public class Installment
  {
    private Common common;
    private IOrganizationService service;
    private Entity enInstallment;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;
    private IPluginExecutionContext context = (IPluginExecutionContext) null;
    private ITracingService traceService = (ITracingService) null;

    public DateTime InterestStarDate { get; set; }

    public int Intereststartdatetype { get; set; }

    public int Gracedays { get; set; }

    public int LateDays { get; set; }

    public Decimal MaxPercent { get; set; }

    public Decimal MaxAmount { get; set; }

    public Decimal InterestPercent { get; set; }

    public Decimal InterestCharge { get; set; }

    public DateTime Duedate { get; set; }

    public Installment(IServiceProvider serviceProvider, Entity enInstallment)
    {
      this.traceService = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      this.context = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
      this.common = new Common(this.service);
      this.traceService.Trace("1");
      this.enInstallment = this.service.Retrieve(enInstallment.LogicalName, enInstallment.Id, new ColumnSet(true));
      this.traceService.Trace("2");
      this.getInterestStartDate();
      this.traceService.Trace("3");
    }

    private void getInterestStartDate()
    {
      EntityReference entityReference1 = this.enInstallment.Contains("bsd_optionentry") ? (EntityReference) this.enInstallment["bsd_optionentry"] : (EntityReference) null;
      Entity entity1 = this.enInstallment.Contains("bsd_optionentry") ? this.service.Retrieve(entityReference1.LogicalName, entityReference1.Id, new ColumnSet(true)) : (Entity) null;
      EntityReference entityReference2 = entity1.Contains("bsd_paymentscheme") ? (EntityReference) entity1["bsd_paymentscheme"] : (EntityReference) null;
      Entity entity2 = this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true));
      EntityReference entityReference3 = entity2.Contains("bsd_interestratemaster") ? (EntityReference) entity2["bsd_interestratemaster"] : (EntityReference) null;
      if (entityReference3 == null)
        return;
      Entity entity3 = this.service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(true));
      this.Gracedays = entity3.Contains("bsd_gracedays") ? (int) entity3["bsd_gracedays"] : 0;
      this.Intereststartdatetype = ((OptionSetValue) entity3["bsd_intereststartdatetype"]).Value;
      this.Duedate = this.enInstallment.Contains("bsd_duedate") ? this.common.RetrieveLocalTimeFromUTCTime((DateTime) this.enInstallment["bsd_duedate"]) : new DateTime(0L);
      this.MaxPercent = entity3.Contains("bsd_toleranceinterestpercentage") ? (Decimal) entity3["bsd_toleranceinterestpercentage"] : 100M;
      this.MaxAmount = entity3.Contains("bsd_toleranceinterestamount") ? (Decimal) entity3["bsd_toleranceinterestamount"] : 0M;
      this.InterestPercent = entity3.Contains("bsd_termsinterestpercentage") ? (Decimal) entity3["bsd_termsinterestpercentage"] : 0M;
      this.InterestStarDate = this.Duedate.AddDays((double) (this.Gracedays + 1));
    }

    public int getLateDays(DateTime dateCalculate)
    {
      int lateDays = (int) dateCalculate.Date.Subtract(this.Duedate.Date).TotalDays;
      if (this.Intereststartdatetype == 100000001)
        lateDays -= this.Gracedays;
      else if (this.InterestStarDate > dateCalculate)
        lateDays = 0;
      this.LateDays = lateDays;
      return lateDays;
    }

    public Decimal calc_InterestCharge(DateTime dateCalculate, Decimal amountPay)
    {
      this.traceService.Trace(nameof (calc_InterestCharge));
      EntityReference entityReference = this.enInstallment.Contains("bsd_optionentry") ? (EntityReference) this.enInstallment["bsd_optionentry"] : (EntityReference) null;
      if (entityReference == null)
        return 0M;
      this.traceService.Trace("enrefOptionEntry: " + entityReference.Id.ToString());
      Decimal num1 = 0M;
      Entity enOptionEntry = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(true));
      if (enOptionEntry.Contains("bsd_signeddadate") || enOptionEntry.Contains("bsd_signedcontractdate"))
      {
        Entity entity1 = this.service.Retrieve("bsd_project", ((EntityReference) enOptionEntry["bsd_project"]).Id, new ColumnSet(new string[2]
        {
          "bsd_name",
          "bsd_dailyinterestchargebank"
        }));
        bool flag = entity1.Contains("bsd_dailyinterestchargebank") && (bool) entity1["bsd_dailyinterestchargebank"];
        Decimal num2 = 0M;
        this.traceService.Trace("11");
        if (flag)
        {
          EntityCollection dailyinterestrate = this.get_ec_bsd_dailyinterestrate(entity1.Id);
          Entity entity2 = dailyinterestrate.Entities[0];
          entity2.Id = dailyinterestrate.Entities[0].Id;
          num2 = entity2.Contains("bsd_interestrate") ? (Decimal) entity2["bsd_interestrate"] : throw new InvalidPluginExecutionException("Can not find Daily Interestrate in Project " + (string) entity1["bsd_name"] + " in master data. Please check again!");
        }
        this.InterestPercent += num2;
        this.traceService.Trace("22");
        Decimal num3 = this.InterestPercent / 30M / 100M * (Decimal) this.LateDays;
        this.traceService.Trace("33");
        num1 = Convert.ToDecimal(amountPay) * num3;
        this.traceService.Trace("44");
        Decimal num4 = this.sumWaiverInterest(enOptionEntry);
        Decimal num5 = this.SumInterestAM_OE(this.service, enOptionEntry.Id) - num4;
        this.traceService.Trace("55");
        this.traceService.Trace("sum_bsd_waiverinterest: " + num4.ToString());
        Decimal num6 = num5 + num1 - num4;
        this.traceService.Trace("sum_temp: " + num6.ToString());
        if ((enOptionEntry.Contains("bsd_totalamountlessfreight") ? ((Money) enOptionEntry["bsd_totalamountlessfreight"]).Value : 0M) == 0M)
          throw new InvalidPluginExecutionException("'Net Selling Price' of " + (string) enOptionEntry["name"] + " must be larger than 0");
        Decimal num7 = enOptionEntry.Contains("totalamount") ? ((Money) enOptionEntry["totalamount"]).Value : 0M;
        if (num7 == 0M)
          throw new InvalidPluginExecutionException("'Total Amount' of " + (string) enOptionEntry["name"] + " must be larger than 0");
        Decimal num8;
        if (this.MaxPercent > 0M)
        {
          Decimal num9 = num7 * this.MaxPercent / 100M;
          this.traceService.Trace("range_enOptionEntryAM: " + num9.ToString());
          num8 = !(this.MaxAmount > 0M) ? num9 : (!(num9 > this.MaxAmount) ? num9 : this.MaxAmount);
        }
        else
          num8 = this.MaxAmount > 0M ? this.MaxAmount : 0M;
        if (num6 >= num8)
        {
          this.traceService.Trace("tmp_rangeAM: " + num8.ToString());
          this.traceService.Trace("sum_Inr_AM: " + num5.ToString());
          num1 = num8 > num5 ? num8 - num5 : 0M;
        }
      }
      return num1;
    }

    public Decimal sumWaiverInterest(Entity enOptionEntry)
    {
      Guid id = enOptionEntry.Id;
      int num = 0;
      QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
      query.ColumnSet.AddColumns("bsd_name");
      query.ColumnSet.AddColumns("bsd_waiverinterest");
      query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, (object) id);
      query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (object) num);
      EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase) query);
      this.traceService.Trace("Count encolInstallment: " + entityCollection.Entities.Count.ToString());
      return entityCollection.Entities.Sum<Entity>((Func<Entity, Decimal>) (x => ((Money) x.Attributes["bsd_waiverinterest"]).Value));
    }

    public Decimal SumInterestAM_OE(IOrganizationService crmservices, Guid OEID)
    {
      Decimal num = 0M;
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='bsd_paymentschemedetail' >\r\n                    <attribute name='bsd_interestchargestatus' />\r\n                    <attribute name='bsd_interestchargeamount' />\r\n                    <filter type='and' >\r\n                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />\r\n                        <condition attribute='bsd_interestchargeamount' operator='not-null' />\r\n                    </filter>\r\n                    <order attribute='bsd_ordernumber' descending='true' />\r\n                  </entity>\r\n                </fetch>", (object) OEID);
      foreach (Entity entity in (Collection<Entity>) crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query)).Entities)
        num += entity.Contains("bsd_interestchargeamount") ? ((Money) entity["bsd_interestchargeamount"]).Value : 0M;
      return num;
    }

    public EntityCollection get_ec_bsd_dailyinterestrate(Guid projID)
    {
      return this.service.RetrieveMultiple((QueryBase) new FetchExpression(string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='bsd_dailyinterestrate' >\r\n                    <attribute name='bsd_date' />\r\n                    <attribute name='bsd_project' />\r\n                    <attribute name='bsd_interestrate' />\r\n                    <attribute name='createdon' />\r\n                    <filter type='and' >\r\n                      <condition attribute='bsd_project' operator='eq' value='{0}' />\r\n                        <condition attribute='statuscode' operator='eq' value='1' />\r\n                    </filter>\r\n                    <order attribute='createdon' descending='true' />\r\n                  </entity>\r\n                </fetch>", (object) projID)));
    }

    public void voidInstallment(Decimal bsd_waiveramount)
    {
      Decimal num1 = this.enInstallment.Contains("bsd_balance") ? ((Money) this.enInstallment["bsd_balance"]).Value : 0M;
      Decimal num2 = this.enInstallment.Contains("bsd_waiverinstallment") ? ((Money) this.enInstallment["bsd_waiverinstallment"]).Value : 0M;
      Decimal num3 = this.enInstallment.Contains(nameof (bsd_waiveramount)) ? ((Money) this.enInstallment[nameof (bsd_waiveramount)]).Value : 0M;
      Decimal num4 = num1 + bsd_waiveramount;
      Decimal num5 = num2 - bsd_waiveramount;
      Decimal num6 = num3 - bsd_waiveramount;
      this.service.Update(new Entity(this.enInstallment.LogicalName, this.enInstallment.Id)
      {
        ["bsd_balance"] = (object) new Money(num4),
        ["bsd_waiverinstallment"] = (object) new Money(num5),
        [nameof (bsd_waiveramount)] = (object) new Money(num6)
      });
    }

    public void voidInterest(Decimal bsd_waiveramount)
    {
      Decimal num1 = this.enInstallment.Contains("bsd_waiverinterest") ? ((Money) this.enInstallment["bsd_waiverinterest"]).Value : 0M;
      Decimal num2 = this.enInstallment.Contains(nameof (bsd_waiveramount)) ? ((Money) this.enInstallment[nameof (bsd_waiveramount)]).Value : 0M;
      Decimal num3 = num1 - bsd_waiveramount;
      Decimal num4 = num2 - bsd_waiveramount;
      this.service.Update(new Entity(this.enInstallment.LogicalName, this.enInstallment.Id)
      {
        ["bsd_waiverinterest"] = (object) new Money(num3),
        [nameof (bsd_waiveramount)] = (object) new Money(num4)
      });
    }

    public void voidManagementFee(Decimal bsd_waiveramount)
    {
      Decimal num1 = this.enInstallment.Contains("bsd_managementfeewaiver") ? ((Money) this.enInstallment["bsd_managementfeewaiver"]).Value : 0M;
      Decimal num2 = this.enInstallment.Contains(nameof (bsd_waiveramount)) ? ((Money) this.enInstallment[nameof (bsd_waiveramount)]).Value : 0M;
      Decimal num3 = num1 - bsd_waiveramount;
      Decimal num4 = num2 - bsd_waiveramount;
      this.service.Update(new Entity(this.enInstallment.LogicalName, this.enInstallment.Id)
      {
        ["bsd_managementfeewaiver"] = (object) new Money(num3),
        [nameof (bsd_waiveramount)] = (object) new Money(num4)
      });
    }

    public void voidMaintenanceFee(Decimal bsd_waiveramount)
    {
      Decimal num1 = this.enInstallment.Contains("bsd_maintenancefeewaiver") ? ((Money) this.enInstallment["bsd_maintenancefeewaiver"]).Value : 0M;
      Decimal num2 = this.enInstallment.Contains(nameof (bsd_waiveramount)) ? ((Money) this.enInstallment[nameof (bsd_waiveramount)]).Value : 0M;
      Decimal num3 = num1 - bsd_waiveramount;
      Decimal num4 = num2 - bsd_waiveramount;
      this.service.Update(new Entity(this.enInstallment.LogicalName, this.enInstallment.Id)
      {
        ["bsd_maintenancefeewaiver"] = (object) new Money(num3),
        [nameof (bsd_waiveramount)] = (object) new Money(num4)
      });
    }

    public Entity getLastConfirmPayment(int bsd_paymenttype)
    {
      Guid id = this.enInstallment.Id;
      int num1 = bsd_paymenttype;
      int num2 = 100000000;
      QueryExpression query = new QueryExpression("bsd_payment");
      query.ColumnSet.AllColumns = true;
      query.AddOrder("bsd_paymentactualtime", OrderType.Descending);
      query.Criteria.AddCondition("bsd_paymentschemedetail", ConditionOperator.Equal, (object) id);
      query.Criteria.AddCondition(nameof (bsd_paymenttype), ConditionOperator.Equal, (object) num1);
      query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (object) num2);
      EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase) query);
      return entityCollection.Entities.Count > 0 ? entityCollection.Entities[0] : (Entity) null;
    }

    public Entity getLastConfirmTransactionPayment(int bsd_transactiontype, int bsd_feetype)
    {
      Guid id = this.enInstallment.Id;
      int num1 = 100000000;
      int num2 = bsd_transactiontype;
      QueryExpression query = new QueryExpression("bsd_transactionpayment");
      query.ColumnSet.AllColumns = true;
      query.AddOrder("createdon", OrderType.Descending);
      query.Criteria.AddCondition("bsd_installment", ConditionOperator.Equal, (object) id);
      query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (object) num1);
      query.Criteria.AddCondition(nameof (bsd_transactiontype), ConditionOperator.Equal, (object) num2);
      if (bsd_feetype != 0)
        query.Criteria.AddCondition(nameof (bsd_feetype), ConditionOperator.Equal, (object) bsd_feetype);
      EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase) query);
      return entityCollection.Entities.Count > 0 ? entityCollection.Entities[0] : (Entity) null;
    }
  }
}
