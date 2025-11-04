// Decompiled with JetBrains decompiler
// Type: Plugin_UpdateLandValueUnit_checkImport.Plugin_UpdateLandValueUnit_checkImport
// Assembly: Plugin_UpdateLandValueUnit_checkImport, Version=1.0.0.0, Culture=neutral, PublicKeyToken=98548fb0662f9df7
// MVID: 906C025B-0C0E-4957-A63D-9A12EC071C03
// Assembly location: C:\Users\ngoct\Downloads\New folder\Plugin_UpdateLandValueUnit_checkImport_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Plugin_UpdateLandValueUnit_checkImportv2
{
  public class Plugin_UpdateLandValueUnit_checkImportv2 : IPlugin
  {
    private IOrganizationService service = (IOrganizationService) null;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;
    private IPluginExecutionContext context = (IPluginExecutionContext) null;
    private ITracingService traceService = (ITracingService) null;

    public void Execute(IServiceProvider serviceProvider)
    {
      this.context = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
      this.traceService = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      Entity inputParameter = (Entity) this.context.InputParameters["Target"];
            traceService.Trace("start");
      if (this.context.MessageName == "Create")
      {
        if (inputParameter.Contains("bsd_units"))
        {
          Entity entity = this.service.Retrieve(((EntityReference) inputParameter["bsd_units"]).LogicalName, ((EntityReference) inputParameter["bsd_units"]).Id, new ColumnSet(true));
          Money money = entity.Contains("bsd_landvalueofunit") ? (Money) entity["bsd_landvalueofunit"] : new Money(0M);
          inputParameter["bsd_landvalueold"] = (object) money;
        }
        if (inputParameter.Contains("bsd_updatelandvalue"))
        {
          Entity entity = this.service.Retrieve(((EntityReference) inputParameter["bsd_updatelandvalue"]).LogicalName, ((EntityReference) inputParameter["bsd_updatelandvalue"]).Id, new ColumnSet(true));
          if ((entity.Contains("statuscode") ? ((OptionSetValue) entity["statuscode"]).Value : 0) == 100000001)
            throw new InvalidPluginExecutionException("Status of Update Land Value is invalid. Please check again.");
        }
        if (inputParameter.Contains("bsd_landvaluenew"))
        {
          Money money = inputParameter.Contains("bsd_landvaluenew") ? (Money) inputParameter["bsd_landvaluenew"] : new Money(0M);
          inputParameter["bsd_landvaluenew"] = (object) money;
        }
        if (inputParameter.Contains("bsd_type") && ((OptionSetValue) inputParameter["bsd_type"]).Value == 100000000 && inputParameter.Contains("bsd_optionentry"))
        {
          Entity entity1 = this.service.Retrieve(((EntityReference) inputParameter["bsd_optionentry"]).LogicalName, ((EntityReference) inputParameter["bsd_optionentry"]).Id, new ColumnSet(true));
          if ((entity1.Contains("statuscode") ? ((OptionSetValue) entity1["statuscode"]).Value : 0) == 100000006)
            throw new InvalidPluginExecutionException("Option Entry is Terminated. Please check again.");
          Money money1 = entity1.Contains("bsd_detailamount") ? (Money) entity1["bsd_detailamount"] : new Money(0M);
          Money money2 = entity1.Contains("bsd_discount") ? (Money) entity1["bsd_discount"] : new Money(0M);
          Money money3 = entity1.Contains("bsd_packagesellingamount") ? (Money) entity1["bsd_packagesellingamount"] : new Money(0M);
          Money money4 = entity1.Contains("bsd_totalamountlessfreight") ? (Money) entity1["bsd_totalamountlessfreight"] : new Money(0M);
          Money money5 = entity1.Contains("bsd_landvaluededuction") ? (Money) entity1["bsd_landvaluededuction"] : new Money(0M);
          Money money6 = entity1.Contains("totaltax") ? (Money) entity1["totaltax"] : new Money(0M);
          Money money7 = entity1.Contains("bsd_freightamount") ? (Money) entity1["bsd_freightamount"] : new Money(0M);
          Money money8 = entity1.Contains("totalamount") ? (Money) entity1["totalamount"] : new Money(0M);
          inputParameter["bsd_listedpricecurrent"] = (object) money1;
          inputParameter["bsd_discountcurrent"] = (object) money2;
          inputParameter["bsd_handoverconditionamountcurrent"] = (object) money3;
          inputParameter["bsd_netsellingpricecurrent"] = (object) money4;
          inputParameter["bsd_landvaluedeductioncurrent"] = (object) money5;
          inputParameter["bsd_totalvattaxcurrent"] = (object) money6;
          inputParameter["bsd_maintenancefeecurrent"] = (object) money7;
          inputParameter["bsd_totalamount"] = (object) money8;
          inputParameter["bsd_listedpricenew"] = (object) money1;
          inputParameter["bsd_discountnew"] = (object) money2;
          inputParameter["bsd_handoverconditionamountnew"] = (object) money3;
          inputParameter["bsd_netsellingpricenew"] = (object) money4;
          EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase) new FetchExpression(string.Format("\r\n                                            <fetch>\r\n                                              <entity name='bsd_paymentschemedetail'>\r\n                                                <filter>\r\n                                                  <condition attribute='bsd_optionentry' operator='eq' value='{0}'/>\r\n                                                  <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002'/>\r\n                                                </filter>\r\n                                              </entity>\r\n                                            </fetch>", (object) entity1.Id)));
          if (entityCollection.Entities.Count > 0)
          {
            Entity entity2 = entityCollection.Entities[0];
            inputParameter["bsd_installment"] = (object) entity2.ToEntityReference();
            Money money9 = entity2.Contains("bsd_amountofthisphase") ? (Money) entity2["bsd_amountofthisphase"] : new Money(0M);
            inputParameter["bsd_amountofthisphasecurrent"] = (object) money9;
          }
        }
      }
      if (!(this.context.MessageName == "Update"))
        return;
      Entity preEntityImage = this.context.PreEntityImages["preimage"];
      int num1 = preEntityImage.Contains("statuscode") ? ((OptionSetValue) preEntityImage["statuscode"]).Value : 0;
      int num2 = preEntityImage.Contains("statecode") ? ((OptionSetValue) preEntityImage["statecode"]).Value : 0;
      int num3 = inputParameter.Contains("statuscode") ? ((OptionSetValue) inputParameter["statuscode"]).Value : 0;
      if ((inputParameter.Contains("statecode") ? ((OptionSetValue) inputParameter["statecode"]).Value : 0) == 1 && num1 == 100000002)
        throw new InvalidPluginExecutionException("Status of Update Land Value is invalid. Please check again.");
    }
  }
}
