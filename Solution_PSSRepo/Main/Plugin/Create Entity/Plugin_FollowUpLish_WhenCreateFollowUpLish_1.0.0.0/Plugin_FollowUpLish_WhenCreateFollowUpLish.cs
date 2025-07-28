// Decompiled with JetBrains decompiler
// Type: Plugin_FollowUpLish_WhenCreateFollowUpLish.Plugin_FollowUpLish_WhenCreateFollowUpLish
// Assembly: Plugin_FollowUpLish_WhenCreateFollowUpLish, Version=1.0.0.0, Culture=neutral, PublicKeyToken=cd2383821fd946cc
// MVID: 4DB237FC-C8EE-4A96-B9D1-2850D87E26CE
// Assembly location: C:\Users\ngoct\Downloads\New folder (3)\Plugin_FollowUpLish_WhenCreateFollowUpLish_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Plugin_FollowUpLish_WhenCreateFollowUpLish
{
  public class Plugin_FollowUpLish_WhenCreateFollowUpLish : IPlugin
  {
    private IOrganizationService service = (IOrganizationService) null;
    private IOrganizationServiceFactory factory = (IOrganizationServiceFactory) null;

    void IPlugin.Execute(IServiceProvider serviceProvider)
    {
      IPluginExecutionContext service = (IPluginExecutionContext) serviceProvider.GetService(typeof (IPluginExecutionContext));
      if (!service.InputParameters.Contains("Target") || !(service.InputParameters["Target"] is Entity))
        return;
      Entity inputParameter = (Entity) service.InputParameters["Target"];
      Guid id = inputParameter.Id;
      if (inputParameter.LogicalName == "bsd_followuplist" && service.MessageName == "Create" && (inputParameter.Contains("bsd_reservation") || inputParameter.Contains("bsd_optionentry")))
      {
        this.factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof (IOrganizationServiceFactory));
        this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));
        ((ITracingService) serviceProvider.GetService(typeof (ITracingService))).Trace(string.Format("Context Depth {0}", (object) service.Depth));
        if (service.Depth > 1)
          return;
        if (inputParameter.Contains("bsd_reservation"))
        {
          Entity entity = this.service.Retrieve(((EntityReference) inputParameter["bsd_reservation"]).LogicalName, ((EntityReference) inputParameter["bsd_reservation"]).Id, new ColumnSet(new string[2]
          {
            "statecode",
            "statuscode"
          }));
          if (((OptionSetValue) entity["statecode"]).Value == 2)
            throw new InvalidPluginExecutionException("This Reservation had been won. Please check again.");
          if (entity.Contains("statuscode"))
          {
            int num = ((OptionSetValue) entity["statuscode"]).Value;
            if (num == 3)
              this.service.Execute((OrganizationRequest) new SetStateRequest()
              {
                EntityMoniker = new EntityReference()
                {
                  Id = entity.Id,
                  LogicalName = entity.LogicalName
                },
                State = new OptionSetValue(0),
                Status = new OptionSetValue(100000000)
              });
            this.service.Update(new Entity(entity.LogicalName)
            {
              Id = entity.Id,
              ["bsd_followuplist"] = (object) true
            });
            if (num == 3)
              this.service.Execute((OrganizationRequest) new SetStateRequest()
              {
                EntityMoniker = new EntityReference()
                {
                  Id = entity.Id,
                  LogicalName = entity.LogicalName
                },
                State = new OptionSetValue(1),
                Status = new OptionSetValue(num)
              });
          }
        }
        else if (inputParameter.Contains("bsd_optionentry"))
          this.service.Update(new Entity(((EntityReference) inputParameter["bsd_optionentry"]).LogicalName)
          {
            Id = ((EntityReference) inputParameter["bsd_optionentry"]).Id,
            ["bsd_followuplist"] = (object) true
          });
      }
    }
  }
}
