// Decompiled with JetBrains decompiler
// Type: Plugin_UpdateLandValueUnits_Create_Update.Plugin_UpdateLandValueUnits_Create_Update
// Assembly: Plugin_UpdateLandValueUnits_Create_Update, Version=1.0.0.0, Culture=neutral, PublicKeyToken=2ca7faa3a5293468
// MVID: 5B6D5BB4-A9E0-42BA-AAA0-445AF9DE9A32
// Assembly location: C:\Users\ngoct\Downloads\New folder\Plugin_UpdateLandValueUnits_Create_Update_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

#nullable disable
namespace Plugin_UpdateLandValueUnits_Create_Update
{
  public class Plugin_UpdateLandValueUnits_Create_Update : IPlugin
  {
    private IOrganizationService service = (IOrganizationService) null;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;
    private ITracingService traceServiceClass = (ITracingService) null;

    void IPlugin.Execute(IServiceProvider serviceProvider)
    {
      IPluginExecutionContext service = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));
      this.traceServiceClass = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      if (!(service.MessageName == "Create"))
        return;
      Entity inputParameter = (Entity) service.InputParameters["Target"];
      Entity entity1 = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(true));
      int num1 = entity1.Contains("bsd_type") ? ((OptionSetValue) entity1["bsd_type"]).Value : 0;
      if (entity1.Contains("bsd_optionentry") && entity1.Contains("bsd_units") && num1 == 100000000)
      {
        Entity entity2 = this.service.Retrieve(((EntityReference) entity1["bsd_units"]).LogicalName, ((EntityReference) entity1["bsd_units"]).Id, new ColumnSet(true));
        Entity entity3 = this.service.Retrieve(((EntityReference) entity1["bsd_optionentry"]).LogicalName, ((EntityReference) entity1["bsd_optionentry"]).Id, new ColumnSet(true));
        Entity entity4 = new Entity(inputParameter.LogicalName, inputParameter.Id);
        Decimal num2 = entity3.Contains("bsd_freightamount") ? ((Money) entity3["bsd_freightamount"]).Value : 0M;
        Decimal num3 = entity1.Contains("bsd_landvaluenew") ? ((Money) entity1["bsd_landvaluenew"]).Value : 0M;
        Decimal num4 = entity1.Contains("bsd_netsellingpricenew") ? ((Money) entity1["bsd_netsellingpricenew"]).Value : 0M;
        Decimal num5 = entity1.Contains("bsd_totalamount") ? ((Money) entity1["bsd_totalamount"]).Value : 0M;
        Decimal num6 = entity1.Contains("bsd_maintenancefeenew") ? ((Money) entity1["bsd_maintenancefeenew"]).Value : 0M;
        Decimal num7 = entity1.Contains("bsd_amountofthisphasecurrent") ? ((Money) entity1["bsd_amountofthisphasecurrent"]).Value : 0M;
        Decimal num8 = entity2.Contains("bsd_netsaleablearea") ? (Decimal) entity2["bsd_netsaleablearea"] : 0M;
        Decimal num9 = num3 * num8;
        Decimal num10 = (num4 - num9) * 0.1M;
        Decimal num11 = num4 + num10 + num2;
        Decimal num12 = Math.Round(num5 - num11, 0);
        entity4["bsd_maintenancefeenew"] = (object) new Money(num2);
        entity4["bsd_landvaluedeductionnew"] = (object) new Money(num9);
        entity4["bsd_totalvattaxnew"] = (object) new Money(num10);
        entity4["bsd_valuedifference"] = (object) new Money(num12);
        entity4["bsd_amountofthisphase"] = (object) new Money(Math.Round(num7 - num12, 0));
        this.service.Update(entity4);
      }
    }
  }
}
