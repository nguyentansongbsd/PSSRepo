using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Routing;

namespace Plugin_UpdateFUL_Create_Update
{
    public class Plugin_UpdateFUL_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceServiceClass = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceServiceClass = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceServiceClass.Trace("Plugin_UpdateFUL_Create_Update");
            traceServiceClass.Trace("MessageName " + context.MessageName);
            traceServiceClass.Trace("Depth " + context.Depth);
            if ((context.MessageName == "Create" && context.Depth < 3) || (context.MessageName == "Update" && context.Depth == 1))
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                var fetchXml2 = $@"
                        <fetch>
                          <entity name='bsd_updateful'>
                            <attribute name=""bsd_updatefulid"" />
                            <filter>
                              <condition attribute='bsd_updatefulid' operator='ne' value='{enTarget.Id}'/>
                              <condition attribute='bsd_newcolumn' operator='eq' value='{(string)enTarget["bsd_newcolumn"]}'/>
                              <condition attribute='statecode' operator='eq' value='0'/>
                            </filter>
                          </entity>
                        </fetch>";
                EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                if (list2.Entities.Count > 0) throw new InvalidPluginExecutionException("Duplicate name. Please check again.");
            }
        }
    }
}