using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Delete_PaymentNotices
{
    public class Plugin_Delete_PaymentNotices : IPlugin
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
            tracingService.Trace("12");

            //Entity en = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            //tracingService.Trace("13");

            EntityReference enInsRef = (EntityReference)entity["bsd_paymentschemedetail"];
            tracingService.Trace("14");

            Entity enIns = service.Retrieve(enInsRef.LogicalName, enInsRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            tracingService.Trace("15");

            Entity enInsUpdate = new Entity(enIns.LogicalName, enIns.Id);
            tracingService.Trace("1");
            enInsUpdate["bsd_paymentnotices"] = false;
            enInsUpdate["bsd_paymentnoticesdate"] = null;
            enInsUpdate["bsd_paymentnoticesnumber"] = null;
            service.Update(enInsUpdate);

        }
    }
}
