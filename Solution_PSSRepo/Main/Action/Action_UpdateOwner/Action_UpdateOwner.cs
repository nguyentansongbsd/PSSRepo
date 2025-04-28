using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_UpdateOwner
{
    public class Action_UpdateOwner : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string entityid = context.InputParameters["entityid"].ToString();
            string entityname = context.InputParameters["entityname"].ToString();
            string userid= context.InputParameters["userid"].ToString();
            var en = new Entity( entityname, new Guid(entityid.ToString()));
            en["ownerid"]=new EntityReference("systemuser",new Guid(userid.ToString()));
            service.Update(en);

        }
    }
}
