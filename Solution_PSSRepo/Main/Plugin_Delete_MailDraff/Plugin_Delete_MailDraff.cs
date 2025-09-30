using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Delete_MailDraff
{
    public class Plugin_Delete_MailDraff:IPlugin
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
            var entity = context.PreEntityImages["EntityAlias"];
            var enRegarding= entity["regardingobjectid"] as EntityReference;
            if(((OptionSetValue)entity["statuscode"]).Value != 1)
            {
               throw new InvalidPluginExecutionException("The status code must be set to 'Draft' (1) before deleting the mail draft.");
            }
            Entity enUpdate= new Entity(enRegarding.LogicalName, enRegarding.Id);
            switch (enRegarding.LogicalName)
            {
                case "bsd_payment":
                    tracingService.Trace("1");
                    enUpdate["bsd_iscreatemail"] = false;
                    enUpdate["bsd_createmaildate "] = null;
                    enUpdate["bsd_emailcreator "] = null;
                    service.Update(enUpdate);

                    break;
                case "bsd_customernotices":
                    enUpdate["bsd_iscreatemail"] = false;
                    enUpdate["bsd_createmaildate "] = null;
                    enUpdate["bsd_emailcreator "] = null;
                    service.Update(enUpdate);
                    break;
                default:
                    tracingService.Trace("Unsupported entity type: " + enRegarding.LogicalName);
                    break;
            }
            

        }
    }
}
