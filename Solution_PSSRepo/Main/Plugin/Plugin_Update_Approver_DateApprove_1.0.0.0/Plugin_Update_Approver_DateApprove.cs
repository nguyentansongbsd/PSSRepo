// Decompiled with JetBrains decompiler
// Type: Plugin_Update_Approver_DateApprove.Plugin_Update_Approver_DateApprove
// Assembly: Plugin_Update_Approver_DateApprove, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c9796ce45e6dbc16
// MVID: A682BCEF-9FA1-4A0A-802D-4CE6BF1896DA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Plugin_Update_Approver_DateApprove_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

#nullable disable
namespace Plugin_Update_Approver_DateApprove
{
  public class Plugin_Update_Approver_DateApprove : IPlugin
  {
    private IOrganizationService service = (IOrganizationService) null;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;
    private ITracingService traceService = (ITracingService) null;
    private IPluginExecutionContext context = (IPluginExecutionContext) null;

    void IPlugin.Execute(IServiceProvider serviceProvider)
    {
      this.context = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
      this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
      this.traceService = (ITracingService) serviceProvider.GetService(typeof (ITracingService));
      Entity inputParameter = (Entity) this.context.InputParameters["Target"];
      Entity entity = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(true));
      int num = entity.Contains("statuscode") ? ((OptionSetValue) entity["statuscode"]).Value : 0;
      string messageName = this.context.MessageName;
      if (!(messageName == "Create") && !(messageName == "Update"))
        return;
      if (num == 100000000 && entity.LogicalName == "bsd_approvechangeduedateinstallment" && this.context.Depth == 1)
        this.updateDateApprovaldateAndApprover(inputParameter);
      if (num == 100000001 && entity.LogicalName == "bsd_updateduedateoflastinstallmentapprove" && this.context.Depth == 1)
        this.updateDateApprovaldateAndApprover(inputParameter);
      if (num == 100000000 && entity.LogicalName == "bsd_updateduedateoflastinstallment" && this.context.Depth == 1)
        this.updateDateApprovaldateAndApprover(inputParameter);
      if (num != 100000001 || !(entity.LogicalName == "bsd_updatelandvalue") || this.context.Depth != 1)
        return;
      this.updateDateApprovaldateAndApprover(inputParameter);
    }

    private void updateDateApprovaldateAndApprover(Entity en)
    {
      Entity entity = this.service.Retrieve(en.LogicalName, en.Id, new ColumnSet(true));
      if (en.LogicalName == "bsd_approvechangeduedateinstallment")
      {
        entity["bsd_approver"] = (object) new EntityReference("systemuser", this.context.UserId);
        entity["bsd_approverejectdate"] = (object) this.RetrieveLocalTimeFromUTCTime(DateTime.Now, this.service);
      }
      if (en.LogicalName == "bsd_updateduedateoflastinstallmentapprove")
      {
        entity["bsd_approvedrejectedperson"] = (object) new EntityReference("systemuser", this.context.UserId);
        entity["bsd_approvedrejecteddate"] = (object) this.RetrieveLocalTimeFromUTCTime(DateTime.Now, this.service);
      }
      if (en.LogicalName == "bsd_updatelandvalue")
      {
        entity["bsd_approvedrejectedperson"] = (object) new EntityReference("systemuser", this.context.UserId);
        entity["bsd_approvedrejecteddate"] = (object) this.RetrieveLocalTimeFromUTCTime(DateTime.Now, this.service);
      }
      if (en.LogicalName == "bsd_updateduedateoflastinstallment")
      {
        entity["bsd_usersconfirmed"] = (object) new EntityReference("systemuser", this.context.UserId);
        entity["bsd_dateconfirmedreject"] = (object) this.RetrieveLocalTimeFromUTCTime(DateTime.Now, this.service);
      }
      this.service.Update(entity);
    }

    private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
    {
      LocalTimeFromUtcTimeRequest request = new LocalTimeFromUtcTimeRequest()
      {
        TimeZoneCode = (this.RetrieveCurrentUsersSettings(service) ?? throw new InvalidPluginExecutionException("Can't find time zone code")).Value,
        UtcTime = utcTime.ToUniversalTime()
      };
      return ((LocalTimeFromUtcTimeResponse) service.Execute((OrganizationRequest) request)).LocalTime;
    }

    private int? RetrieveCurrentUsersSettings(IOrganizationService service)
    {
      IOrganizationService organizationService = service;
      QueryExpression queryExpression1 = new QueryExpression("usersettings");
      queryExpression1.ColumnSet = new ColumnSet(new string[2]
      {
        "localeid",
        "timezonecode"
      });
      QueryExpression queryExpression2 = queryExpression1;
      FilterExpression filterExpression = new FilterExpression();
      filterExpression.Conditions.Add(new ConditionExpression("systemuserid", ConditionOperator.EqualUserId));
      queryExpression2.Criteria = filterExpression;
      QueryExpression query = queryExpression1;
      return (int?) organizationService.RetrieveMultiple((QueryBase) query).Entities[0].ToEntity<Entity>().Attributes["timezonecode"];
    }
  }
}
