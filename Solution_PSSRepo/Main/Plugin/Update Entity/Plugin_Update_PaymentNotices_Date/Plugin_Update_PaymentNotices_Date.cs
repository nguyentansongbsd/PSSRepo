using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Update_PaymentNotices_Date
{
    public class Plugin_Update_PaymentNotices_Date : IPlugin
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
            Entity entity = (Entity)context.InputParameters["Target"];
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            EntityReference enInsRef = (EntityReference)enCreated["bsd_paymentschemedetail"];
            Entity enIns = service.Retrieve(enInsRef.LogicalName, enInsRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Entity enInsUpdate = new Entity(enIns.LogicalName, enIns.Id);
            enInsUpdate["bsd_paymentnoticesdate"] = enCreated["bsd_date"];
            service.Update(enInsUpdate);

        }
    }
}
