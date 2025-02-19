// Decompiled with JetBrains decompiler
// Type: Plugin_Update_Estimate_Handover_Date_Approve.Plugin_Update_Estimate_Handover_Date_Approve
// Assembly: Plugin_Update_Estimate_Handover_Date_Approve, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75cfd1a6ec551ced
// MVID: 28675E90-35EB-49DF-8CB6-16EE8292EF5A
// Assembly location: C:\Users\ngoct\Downloads\Plugin_Update_Estimate_Handover_Date_Approve_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;

namespace Plugin_Update_Estimate_Handover_Date_Approve
{
  public class Plugin_Update_Estimate_Handover_Date_Approve : IPlugin
  {
    private IOrganizationService service = (IOrganizationService) null;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;

    void IPlugin.Execute(IServiceProvider serviceProvider)
    {
      IPluginExecutionContext service1 = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.service = this.factory.CreateOrganizationService(new Guid?(service1.UserId));
      ITracingService service2 = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      Entity inputParameter = (Entity) service1.InputParameters["Target"];
      if (!(inputParameter.LogicalName == "bsd_updateestimatehandoverdate"))
        return;
      Entity handoverdate = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(true));
      if (((OptionSetValue) handoverdate["statuscode"]).Value == 100000001 && !handoverdate.Contains("bsd_approvedrejectedperson"))
      {
        int num = ((OptionSetValue) handoverdate["bsd_types"]).Value;
        EntityCollection entityCollection1 = this.fetch_es(this.service, handoverdate);
        if (num == 100000000)
        {
          DateTime dateTime1 = handoverdate.Contains("bsd_opdate") ? (DateTime) handoverdate["bsd_opdate"] : throw new InvalidPluginExecutionException("Please input OP Date in this Update estimate handover date!");
          if (entityCollection1.Entities.Count > 0)
          {
            foreach (Entity entity1 in (Collection<Entity>) entityCollection1.Entities)
            {
              EntityReference unit = entity1.Contains("bsd_units") ? (EntityReference) entity1["bsd_units"] : (EntityReference) null;
              DateTime dateTime2 = (DateTime) entity1["bsd_estimatehandoverdatenew"];
              if (unit != null)
              {
                EntityCollection entityCollection2 = this.fetch_units(this.service, unit);
                if (entityCollection2.Entities.Count > 0)
                {
                  foreach (Entity entity2 in (Collection<Entity>) entityCollection2.Entities)
                    this.service.Update(new Entity(entity2.LogicalName)
                    {
                      Id = entity2.Id,
                      ["bsd_estimatehandoverdate"] = (object) dateTime2,
                      ["bsd_opdate"] = (object) dateTime1
                    });
                }
              }
              this.service.Update(new Entity(entity1.LogicalName)
              {
                Id = entity1.Id,
                ["statuscode"] = (object) new OptionSetValue(100000000)
              });
            }
          }
        }
        if (num == 100000002)
        {
          if (!handoverdate.Contains("bsd_opdate"))
            throw new InvalidPluginExecutionException("Please input OP Date in this Update estimate handover date!");
          if (entityCollection1.Entities.Count > 0)
          {
            foreach (Entity entity3 in (Collection<Entity>) entityCollection1.Entities)
            {
              this.service.Update(new Entity(entity3.LogicalName)
              {
                Id = entity3.Id,
                ["statuscode"] = (object) new OptionSetValue(100000000)
              });
              DateTime dateTime3 = (DateTime) handoverdate["bsd_opdate"];
              EntityReference unit = entity3.Contains("bsd_units") ? (EntityReference) entity3["bsd_units"] : (EntityReference) null;
              EntityReference ins = entity3.Contains("bsd_installment") ? (EntityReference) entity3["bsd_installment"] : (EntityReference) null;
              EntityReference op = entity3.Contains("bsd_optionentry") ? (EntityReference) entity3["bsd_optionentry"] : (EntityReference) null;
              EntityCollection entityCollection3 = this.fetch_units(this.service, unit);
              if (entityCollection3.Entities.Count > 0)
              {
                foreach (Entity entity4 in (Collection<Entity>) entityCollection3.Entities)
                  this.service.Update(new Entity(entity4.LogicalName)
                  {
                    Id = entity4.Id,
                    ["bsd_opdate"] = (object) dateTime3
                  });
              }
              if (entity3.Contains("bsd_paymentduedate"))
              {
                if (ins != null)
                {
                  EntityCollection entityCollection4 = this.fetch_ins(this.service, ins);
                  if (entityCollection4.Entities.Count > 0)
                  {
                    foreach (Entity entity5 in (Collection<Entity>) entityCollection4.Entities)
                    {
                      DateTime dateTime4 = (DateTime) entity3["bsd_paymentduedate"];
                      this.service.Update(new Entity(entity5.LogicalName)
                      {
                        Id = entity5.Id,
                        ["bsd_duedate"] = (object) dateTime4
                      });
                    }
                  }
                }
              }
              else
              {
                EntityCollection entityCollection5 = this.fetch_op_due(this.service, op);
                if (entityCollection5.Entities.Count > 0)
                {
                  foreach (Entity entity6 in (Collection<Entity>) entityCollection5.Entities)
                  {
                    DateTime dateTime5 = (DateTime) entity6["bsd_duedate"];
                    if (ins != null)
                    {
                      EntityCollection entityCollection6 = this.fetch_ins(this.service, ins);
                      if (entityCollection6.Entities.Count > 0)
                      {
                        foreach (Entity entity7 in (Collection<Entity>) entityCollection6.Entities)
                          this.service.Update(new Entity(entity7.LogicalName)
                          {
                            Id = entity7.Id,
                            ["bsd_duedate"] = (object) dateTime5
                          });
                      }
                    }
                  }
                }
              }
            }
          }
        }
        if (num == 100000001)
        {
          DateTime dateTime6 = handoverdate.Contains("bsd_opdate") ? (DateTime) handoverdate["bsd_opdate"] : throw new InvalidPluginExecutionException("Please input OP Date in this Update estimate handover date!");
          if (entityCollection1.Entities.Count > 0)
          {
            foreach (Entity entity8 in (Collection<Entity>) entityCollection1.Entities)
            {
              EntityReference unit = entity8.Contains("bsd_units") ? (EntityReference) entity8["bsd_units"] : (EntityReference) null;
              EntityReference ins = entity8.Contains("bsd_installment") ? (EntityReference) entity8["bsd_installment"] : (EntityReference) null;
              EntityReference op = entity8.Contains("bsd_optionentry") ? (EntityReference) entity8["bsd_optionentry"] : (EntityReference) null;
              DateTime dateTime7 = (DateTime) entity8["bsd_estimatehandoverdatenew"];
              if (unit != null)
              {
                EntityCollection entityCollection7 = this.fetch_units(this.service, unit);
                if (entityCollection7.Entities.Count > 0)
                {
                  foreach (Entity entity9 in (Collection<Entity>) entityCollection7.Entities)
                    this.service.Update(new Entity(entity9.LogicalName)
                    {
                      Id = entity9.Id,
                      ["bsd_estimatehandoverdate"] = (object) dateTime7,
                      ["bsd_opdate"] = (object) dateTime6
                    });
                }
              }
              DateTime dateTime8 = new DateTime();
              DateTime dateTime9 = !entity8.Contains("bsd_paymentduedate") ? (!handoverdate.Contains("bsd_paymentduedate") ? dateTime7 : (DateTime) handoverdate["bsd_paymentduedate"]) : (DateTime) entity8["bsd_paymentduedate"];
              if (ins != null)
              {
                EntityCollection entityCollection8 = this.fetch_ins(this.service, ins);
                if (entityCollection8.Entities.Count > 0)
                {
                  foreach (Entity entity10 in (Collection<Entity>) entityCollection8.Entities)
                  {
                    DateTime dateTime10 = (DateTime) entity8["bsd_paymentduedate"];
                    this.service.Update(new Entity(entity10.LogicalName)
                    {
                      Id = entity10.Id,
                      ["bsd_duedate"] = (object) dateTime9
                    });
                  }
                }
              }
              if (op != null)
              {
                EntityCollection entityCollection9 = this.fetch_op(this.service, op);
                if (entityCollection9.Entities.Count > 0)
                {
                  foreach (Entity entity11 in (Collection<Entity>) entityCollection9.Entities)
                    this.service.Update(new Entity(entity11.LogicalName)
                    {
                      Id = entity11.Id,
                      ["bsd_estimatehandoverdatecontract"] = (object) dateTime7
                    });
                }
              }
              this.service.Update(new Entity(entity8.LogicalName)
              {
                Id = entity8.Id,
                ["statuscode"] = (object) new OptionSetValue(100000000)
              });
            }
          }
        }
        this.service.Update(new Entity(inputParameter.LogicalName)
        {
          Id = inputParameter.Id,
          ["bsd_approvedrejecteddate"] = (object) DateTime.Now,
          ["bsd_approvedrejectedperson"] = (object) new EntityReference("systemuser", service1.UserId)
        });
      }
    }

    private EntityCollection fetch_es(IOrganizationService crmservices, Entity handoverdate)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\n  <entity name='bsd_updateestimatehandoverdatedetail'>\n    <attribute name='bsd_updateestimatehandoverdatedetailid' />\n    <attribute name='bsd_optionentry' />\n    <attribute name='bsd_units' />\n<attribute name='bsd_paymentduedate' />\n    <attribute name='bsd_installment' />\n  <attribute name='bsd_estimatehandoverdatenew' />\n    <order attribute='bsd_name' descending='false' />\n    <filter type='and'>\n      <condition attribute='bsd_updateestimatehandoverdate' operator='eq' value='{0}' />\n    </filter>\n  </entity>\n</fetch>", (object) handoverdate.Id);
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
    }

    private EntityCollection fetch_units(IOrganizationService crmservices, EntityReference unit)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\n  <entity name='product'>\n    <attribute name='name' />\n    <attribute name='productnumber' />\n    <attribute name='description' />\n    <attribute name='statecode' />\n    <attribute name='createdon' />\n    <attribute name='bsd_unitscodesams' />\n    <attribute name='bsd_estimatehandoverdate' />\n    <attribute name='bsd_opdate' />\n    <attribute name='productid' />\n    <order attribute='createdon' descending='true' />\n    <filter type='and'>\n      <condition attribute='productid' operator='eq' value='{0}' />\n    </filter>\n  </entity>\n</fetch>", (object) unit.Id);
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
    }

    private EntityCollection fetch_ins(IOrganizationService crmservices, EntityReference ins)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\n  <entity name='bsd_paymentschemedetail'>\n    <attribute name='bsd_paymentschemedetailid' />\n    <attribute name='bsd_name' />\n    <attribute name='createdon' />\n    <order attribute='bsd_name' descending='false' />\n    <filter type='and'>\n      <condition attribute='bsd_paymentschemedetailid' operator='eq' value='{0}' />\n    </filter>\n  </entity>\n</fetch>", (object) ins.Id);
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
    }

    private EntityCollection fetch_op(IOrganizationService crmservices, EntityReference op)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\n  <entity name='salesorder'>\n    <attribute name='name' />\n    <attribute name='customerid' />\n    <attribute name='bsd_optioncodesams' />\n    <attribute name='bsd_contractnumber' />\n    <attribute name='quoteid' />\n    <attribute name='bsd_phaseslaunch' />\n    <attribute name='salesorderid' />\n    <order attribute='createdon' descending='true' />\n    <filter type='and'>\n      <condition attribute='salesorderid' operator='eq' value='{0}' />\n    </filter>\n  </entity>\n</fetch>", (object) op.Id);
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
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

    private EntityCollection fetch_op_due(IOrganizationService crmservices, EntityReference op)
    {
      string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\n  <entity name='bsd_paymentschemedetail'>\n    <attribute name='bsd_paymentschemedetailid' />\n    <attribute name='bsd_name' />\n    <attribute name='bsd_duedate' />\n    <order attribute='bsd_name' descending='false' />\n    <filter type='and'>\n      <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002' />\n    </filter>\n    <link-entity name='salesorder' from='salesorderid' to='bsd_optionentry' alias='ab'>\n      <filter type='and'>\n        <condition attribute='salesorderid' operator='eq' value='{0}' />\n      </filter>\n    </link-entity>\n  </entity>\n</fetch>", (object) op.Id);
      return crmservices.RetrieveMultiple((QueryBase) new FetchExpression(query));
    }
  }
}
