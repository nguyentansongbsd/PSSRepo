﻿// Decompiled with JetBrains decompiler
// Type: Action_TerminateLetter_GenerateTerminateLetter.Action_TerminateLetter_GenerateTerminateLetter
// Assembly: Action_TerminateLetter_GenerateTerminateLetter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=2004ca381ff4b6b9
// MVID: 8D192887-32A8-4175-AB51-C93F8985A359
// Assembly location: C:\Users\ngoct\Downloads\Action_TerminateLetter_GenerateTerminateLetter_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Action_TerminateLetter_GenerateTerminateLetter
{
  public class Action_TerminateLetter_GenerateTerminateLetter : IPlugin
  {
    private IOrganizationService service = (IOrganizationService) null;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;

    void IPlugin.Execute(IServiceProvider serviceProvider)
    {
      IPluginExecutionContext service = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));
      ((ITracingService) serviceProvider.GetService(typeof (ITracingService))).Trace(string.Format("Context Depth {0}", (object) service.Depth));
      EntityCollection paymentScheme = this.findPaymentScheme(this.service);
      if (paymentScheme.Entities.Count == 0)
        throw new InvalidPluginExecutionException("No payments found!");
      int num1 = 0;
      for (int index1 = 0; index1 < paymentScheme.Entities.Count; ++index1)
      {
        Entity entity1 = paymentScheme.Entities[index1];
        if (!this.CheckExist_optionEntry_on_Terminateletter(entity1.Id))
        {
          EntityCollection dtlFromOpentryId = this.get_pmSchDtl_fromOpentryID(entity1.Id);
          int num2 = 0;
          for (int index2 = 0; index2 < dtlFromOpentryId.Entities.Count; ++index2)
          {
            Entity entity2 = dtlFromOpentryId.Entities[index2];
            if (entity2.Contains("statuscode"))
            {
              if (((OptionSetValue) entity2["statuscode"]).Value == 100000001)
              {
                if (entity2.Contains("bsd_actualgracedays"))
                  num2 += (int) entity2["bsd_actualgracedays"];
                else
                  num2 = num2;
              }
              else if (((OptionSetValue) entity2["statuscode"]).Value == 100000000)
              {
                DateTime dateTime = DateTime.Now;
                dateTime = dateTime.Date;
                int num3 = (int) dateTime.Subtract(((DateTime) entity2["bsd_duedate"]).Date).TotalDays;
                if (num3 <= 0)
                  num3 = 0;
                num2 += num3;
              }
            }
          }
          if (this.check_overDuedate_pmShcDtl(dtlFromOpentryId) || num2 > 60)
          {
            Entity entity3 = new Entity("bsd_terminateletter");
            if (entity1.Contains("customerid"))
              entity3["bsd_customer"] = entity1["customerid"];
            entity3["bsd_name"] = (object) ("Terminate letter of " + (entity1.Contains("name") ? entity1["name"] : (object) ""));
            entity3["bsd_subject"] = (object) "Terminate letter";
            entity3["bsd_date"] = (object) DateTime.Now;
            entity3["bsd_terminatefee"] = (object) new Money(0M);
            entity3["bsd_optionentry"] = (object) entity1.ToEntityReference();
            EntityCollection units = this.findUnits(this.service, entity1);
            if (units.Entities.Count > 0)
            {
              Entity entity4 = units[0];
              if (entity4.Contains("productid"))
                entity3["bsd_units"] = entity4["productid"];
            }
            this.service.Create(entity3);
            ++num1;
          }
        }
      }
      service.OutputParameters["iCount_gen_TermLetter"] = (object) num1.ToString();
    }

    private bool check_overDuedate_pmShcDtl(EntityCollection entCll)
    {
      for (int index = 0; index < entCll.Entities.Count; ++index)
      {
        Entity entity = entCll.Entities[index];
        DateTime dateTime = DateTime.Now;
        dateTime = dateTime.Date;
        if ((int) dateTime.Subtract(((DateTime) entity["bsd_duedate"]).Date).TotalDays > 60)
          return true;
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
      return this.service.RetrieveMultiple((QueryBase) query);
    }

    private EntityCollection getPmShDtl(IOrganizationService crmservices, Entity op)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                   <entity name='bsd_paymentschemedetail'>\r\n                    <attribute name='bsd_paymentschemedetailid' />\r\n                    <attribute name='bsd_name' />\r\n                    <attribute name='createdon' />\r\n                    <order attribute='bsd_name' descending='false' />\r\n                    <filter type='and'>\r\n                      <condition attribute='bsd_optionentry' operator='eq'  value='{0}' />\r\n                    </filter>\r\n                  </entity>\r\n                </fetch>", (object) op.Id);
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
    }

    private EntityCollection findPaymentScheme(IOrganizationService crmservices)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='salesorder'>\r\n                    <attribute name='name' />\r\n                    <attribute name='customerid' />\r\n                    <attribute name='statuscode' />\r\n                    <attribute name='totalamount' />\r\n                    <attribute name='salesorderid' />\r\n                    <order attribute='name' descending='false' />\r\n                    <filter type='and'>\r\n                      <condition attribute='name' operator='not-null' />\r\n                    </filter>                    \r\n                  </entity>\r\n                </fetch>");
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
    }

    private EntityCollection findUnits(IOrganizationService crmservices, Entity OptionEntry)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                <entity name='salesorderdetail'>\r\n                <attribute name='productid' />\r\n                <attribute name='salesorderdetailid' />\r\n                <order attribute='productid' descending='false' />\r\n                <filter type='and'>\r\n              <condition attribute='salesorderid' operator='eq'  uitype='salesorder' value='{0}' />\r\n            </filter>\r\n          </entity>\r\n        </fetch>", (object) OptionEntry.Id);
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
    }

    private EntityCollection get_pmschDtl_outofDuedate(IOrganizationService crmservices, Entity op)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='salesorder'>\r\n                   <attribute name='name' />\r\n                   <attribute name='statuscode' />\r\n                    <attribute name='totalamount' />\r\n                    <attribute name='salesorderid' />\r\n                    <order attribute='name' descending='false' />\r\n                    <filter type='and'>\r\n                      <condition attribute='name' operator='not-null' />\r\n                    </filter>\r\n                    <link-entity name='bsd_paymentschemedetail' from='bsd_optionentry' to='salesorderid' alias='ab'>\r\n                      <filter type='and'>\r\n                        <condition attribute='bsd_duedate' operator='olderthan-x-days' value='59' />\r\n                        <condition attribute='statuscode' operator='eq' value='100000000' />\r\n                        <condition attribute='bsd_optionentry' operator='eq' value={0} />\r\n                      </filter>\r\n                    </link-entity>\r\n                  </entity>\r\n                </fetch>", (object) op.Id);
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
    }

    private bool CheckExist_optionEntry_on_Terminateletter(Guid opRef)
    {
      QueryExpression query = new QueryExpression("bsd_terminateletter");
      query.ColumnSet = new ColumnSet(new string[1]
      {
        "bsd_terminateletterid"
      });
      query.Criteria = new FilterExpression(LogicalOperator.And);
      query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object) opRef));
      query.TopCount = new int?(1);
      return this.service.RetrieveMultiple((QueryBase) query).Entities.Count > 0;
    }

    private EntityCollection get_pmSchDtl_fromOpentryID(Guid opID)
    {
      QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
      query.ColumnSet = new ColumnSet(new string[4]
      {
        "bsd_duedate",
        "statuscode",
        "bsd_actualgracedays",
        "bsd_amountofthisphase"
      });
      query.Criteria = new FilterExpression(LogicalOperator.And);
      query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object) opID));
      return this.service.RetrieveMultiple((QueryBase) query);
    }
  }
}
