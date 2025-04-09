using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;

namespace Plugin_OptionEntry_UpdateStatusCode
{
    public class Plugin_OptionEntry_UpdateStatusCode:IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName.ToLower().Trim() == "update" || context.MessageName.ToLower().Trim() == "create")
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity opp = service.Retrieve("salesorder", target.Id, new ColumnSet(true));
                if (target.LogicalName == "salesorder")
                {
                    if (target.Contains("statuscode")&& ((OptionSetValue)target["statuscode"]).Value == 1)
                    {
                        Entity quote = service.Retrieve("quote", ((EntityReference)opp["quoteid"]).Id, new ColumnSet(new String[] { "statuscode" }));
                        if (((OptionSetValue)quote["statuscode"]).Value == 4|| ((OptionSetValue)quote["statuscode"]).Value == 3)
                        {
                            SetStateRequest setStateRequest = new SetStateRequest()
                            {
                                EntityMoniker = new EntityReference
                                {
                                    Id = opp.Id,
                                    LogicalName = "salesorder"
                                },
                                State = new OptionSetValue(0),
                                Status = new OptionSetValue(100000000)
                            };
                            service.Execute(setStateRequest);
                        }
                    }
                    else if (opp.Contains("bsd_pinkbookstatus") && (bool)opp["bsd_pinkbookstatus"] && target.Contains("bsd_actualhandoverdate") && context.MessageName.ToLower().Trim() == "update")
                    {
                        SetStateRequest setStateRequest = new SetStateRequest()
                        {
                            EntityMoniker = new EntityReference
                            {
                                Id = opp.Id,
                                LogicalName = opp.LogicalName
                            },
                            State = new OptionSetValue(3),
                            Status = new OptionSetValue(100001)
                        };
                        service.Execute(setStateRequest);
                    }
                }
            }
        }
    }
}
