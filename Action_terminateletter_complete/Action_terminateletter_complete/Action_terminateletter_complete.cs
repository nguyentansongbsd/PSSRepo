using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Text;

namespace Action_terminateletter_complete
{
    public class Action_terminateletter_complete : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        ITracingService TracingSe = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference target = (EntityReference)context.InputParameters["Target"];
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity terminateletter = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));

            Entity enUp = new Entity(target.LogicalName, target.Id);
            enUp["statuscode"] = new OptionSetValue(100000005);
            service.Update(enUp);

            if (terminateletter.Contains("bsd_optionentry"))
            {
                Entity enOE = new Entity(((EntityReference)terminateletter["bsd_optionentry"]).LogicalName, ((EntityReference)terminateletter["bsd_optionentry"]).Id);
                enOE["bsd_terminationletter"] = false;
                service.Update(enOE);
            }
        }
    }
}