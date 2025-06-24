using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_CreateEmail_UpdateInforToEntity
{
    public class Plugin_CreateEmail_UpdateInforToEntity : IPlugin
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
            if (enTarget.Contains("bsd_entityid"))
            {
                var enMap = service.Retrieve(enTarget["bsd_entityname"].ToString(), new Guid(enTarget["bsd_entityid"].ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                var enUpdate = new Entity(enMap.LogicalName, enMap.Id);
                enUpdate["bsd_emailstatus"] = enTarget["statuscode"];
                enUpdate["bsd_emailcreator"] = enTarget["bsd_emailcreator"];
                enUpdate["bsd_createmaildate"] = DateTime.Now;

                
                service.Update(enUpdate);
            }
        }
    }
}
