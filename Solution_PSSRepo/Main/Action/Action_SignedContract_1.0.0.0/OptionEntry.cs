// Decompiled with JetBrains decompiler
// Type: BSDLibrary.OptionEntry
// Assembly: Action_SignedContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=91af1975bd46f505
// MVID: 64A057F8-04D7-4937-A84E-D4EF3DDC89DB
// Assembly location: C:\Users\ngoct\Downloads\Action_SignedContract_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
namespace BSDLibrary
{
  public class OptionEntry
  {
    private IOrganizationService service;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;
    private IPluginExecutionContext context = (IPluginExecutionContext) null;
    private ITracingService traceService = (ITracingService) null;
    private Common common;
    private Entity enOptionEntry;

    public OptionEntry(IServiceProvider serviceProvider, Entity enOptionEntry)
    {
      this.traceService = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      this.context = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
      this.common = new Common(this.service);
      this.traceService.Trace("1");
      this.enOptionEntry = this.service.Retrieve(enOptionEntry.LogicalName, enOptionEntry.Id, new ColumnSet(true));
    }

    public bool checkShortFallAmount(Entity enOptionEntry)
    {
      bool flag1 = false;
      if (((OptionSetValue) enOptionEntry["statuscode"]).Value == 100000000)
      {
        bool flag2 = enOptionEntry.Contains("bsd_specialcontractprintingapproval") && (bool) enOptionEntry["bsd_specialcontractprintingapproval"];
        Entity intallmentByOrdernumber = this.getIntallmentByOrdernumber(enOptionEntry, 1);
        Decimal num1 = intallmentByOrdernumber.Contains("bsd_balance") ? ((Money) intallmentByOrdernumber["bsd_balance"]).Value : 0M;
        EntityReference entityReference = enOptionEntry.Contains("bsd_project") ? (EntityReference) enOptionEntry["bsd_project"] : (EntityReference) null;
        if (entityReference != null)
        {
          Entity entity = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(true));
          Decimal num2 = entity.Contains("bsd_shortfallamount") ? ((Money) entity["bsd_shortfallamount"]).Value : 0M;
          if (num1 <= num2)
            flag1 = true;
        }
      }
      return flag1;
    }

    public Entity getIntallmentByOrdernumber(Entity enOptionEntry, int ordernumber)
    {
      int num1 = 0;
      Guid id = enOptionEntry.Id;
      int num2 = ordernumber;
      QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
      query.ColumnSet.AllColumns = true;
      query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (object) num1);
      query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, (object) id);
      query.Criteria.AddCondition("bsd_ordernumber", ConditionOperator.Equal, (object) num2);
      EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase) query);
      return entityCollection.Entities.Count > 0 ? entityCollection.Entities[0] : (Entity) null;
    }
  }
}
