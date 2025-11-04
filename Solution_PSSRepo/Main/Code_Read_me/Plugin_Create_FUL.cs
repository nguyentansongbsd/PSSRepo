using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Create_FUL
{
    public class Plugin_Create_FUL : IPlugin
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

            //get entity
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;
            en = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            if (((OptionSetValue)en["bsd_type"]).Value != 100000006&& ((OptionSetValue)en["bsd_type"]).Value!= 100000005) return;
            if (((OptionSetValue)en["statuscode"]).Value== 100000002)
                return;
            bool bol1 = true;
            bool bol2 = true;
            if ((en.Contains("bsd_terminateletter")) == false || ((bool)en["bsd_terminateletter"]) == false)
            {
                bol1 = false;
            }

            if (en.Contains("bsd_termination") == false || ((bool)en["bsd_termination"]) == false)
            {
                bol2 = false;
            }
            if (bol1 == false && bol2 == false)
            {


                throw new InvalidPluginExecutionException("Please check the Termination information before saving.");

            }


        }
    }
}
