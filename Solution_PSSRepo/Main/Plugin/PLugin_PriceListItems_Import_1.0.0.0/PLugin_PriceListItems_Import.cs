// Decompiled with JetBrains decompiler
// Type: PLugin_PriceListItems_Import.PLugin_PriceListItems_Import
// Assembly: PLugin_PriceListItems_Import, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ab2aa3e1fab715a5
// MVID: 587925AB-EC6F-441E-95CD-22C40BDE6BB3
// Assembly location: C:\Users\ngoct\Downloads\New folder\PLugin_PriceListItems_Import_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

#nullable disable
namespace PLugin_PriceListItems_Import
{
  public class PLugin_PriceListItems_Import : IPlugin
  {
    private IPluginExecutionContext context = (IPluginExecutionContext) null;
    private IOrganizationService service = (IOrganizationService) null;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;
    private ITracingService traceService = (ITracingService) null;
    private Entity target = (Entity) null;

    public void Execute(IServiceProvider serviceProvider)
    {
      this.context = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
      this.traceService = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      this.Init();
    }

    private void Init()
    {
      try
      {
        if (this.context.Depth > 3 || this.context.MessageName != "Create")
          return;
        this.target = (Entity) this.context.InputParameters["Target"];
        if (this.target.Contains("productid") && this.getUnit((EntityReference) this.target["productid"]) != 1)
          throw new InvalidPluginExecutionException("The product status is invalid. Please check the information again.");
      }
      catch (InvalidPluginExecutionException ex)
      {
        throw ex;
      }
    }

    private int getUnit(EntityReference enfUnit)
    {
      try
      {
        return ((OptionSetValue) this.service.Retrieve(enfUnit.LogicalName, enfUnit.Id, new ColumnSet(new string[1]
        {
          "statuscode"
        }))["statuscode"]).Value;
      }
      catch (InvalidPluginExecutionException ex)
      {
        throw ex;
      }
    }
  }
}
