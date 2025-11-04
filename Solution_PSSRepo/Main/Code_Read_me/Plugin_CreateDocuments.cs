using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_CreateDocuments
{
    public class Plugin_CreateDocuments : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        string cusType = "";
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity entity = (Entity)context.InputParameters["Target"];
            Entity enTarget = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            tracingService.Trace("start id:" + entity.Id);
            string name = enTarget["bsd_name"].ToString();
            bool isUpdate = false;
            if (enTarget.Contains("bsd_email") && (bool)enTarget["bsd_email"]==true)
            {
                name= name + "-Email";
                isUpdate = true;
            }
            if (enTarget.Contains("bsd_project"))
            {
                var enProject = service.Retrieve("bsd_project", ((EntityReference)enTarget["bsd_project"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                name =enProject["bsd_projectcode"].ToString()+"-"+name;
                isUpdate = true;
            }
            if(isUpdate)
            {
                var enUpdate = new Entity(enTarget.LogicalName, enTarget.Id);
                enUpdate["bsd_name"] = name;
                service.Update(enUpdate);
            }

        }
    }
}
