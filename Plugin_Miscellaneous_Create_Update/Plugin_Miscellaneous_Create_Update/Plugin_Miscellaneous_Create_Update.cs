using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;

namespace Plugin_Miscellaneous_Create_Update
{
    public class Plugin_Miscellaneous_Create_Update : IPlugin
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
            if (context.MessageName == "Create" && context.Depth == 2)
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                Entity enUp = new Entity(target.LogicalName, target.Id);
                bool checkUp = false;
                if (enTarget.Contains("bsd_optionentry"))
                {
                    var fetchXml2 = $@"
                        <fetch>
                          <entity name='salesorder'>
                            <attribute name=""bsd_unitnumber"" />
                            <filter>
                              <condition attribute='salesorderid' operator='eq' value='{((EntityReference)enTarget["bsd_optionentry"]).Id}'/>
                              <condition attribute='statuscode' operator='ne' value='100000006'/>
                            </filter>
                          </entity>
                        </fetch>";
                    EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    if (list2.Entities.Count == 0) throw new InvalidPluginExecutionException("Option Entry has been terminated. Please check again.");
                    foreach (Entity entity in list2.Entities)
                    {
                        enUp["bsd_units"] = (EntityReference)entity["bsd_unitnumber"];
                        checkUp = true;
                    }
                }
                else throw new InvalidPluginExecutionException("Option Entry is empty. Please check again.");
                if (checkUp) service.Update(enUp);
            }
        }
    }
}