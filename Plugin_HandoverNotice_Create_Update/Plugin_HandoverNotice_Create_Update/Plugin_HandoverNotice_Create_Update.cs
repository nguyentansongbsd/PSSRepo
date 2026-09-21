using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugin_HandoverNotice_Create_Update
{
    public class Plugin_HandoverNotice_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth > 2) return;
            if (context.MessageName == "Delete")
            {
                Entity enPre = context.PreEntityImages["pre"];
                if (enPre.Contains("bsd_installment"))
                {
                    EntityReference bsd_installment = (EntityReference)enPre["bsd_installment"];
                    Entity uins = new Entity(bsd_installment.LogicalName, bsd_installment.Id);
                    uins["bsd_paymentnotices"] = false;
                    uins["bsd_paymentnoticesnumber"] = null;
                    uins["bsd_paymentnoticesdate"] = null;
                    service.Update(uins);
                }
            }
            else if (context.MessageName == "Update")
            {
                Entity target = context.InputParameters["Target"] as Entity;
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                if (enTarget.Contains("bsd_installment") && enTarget.Contains("bsd_billdate"))
                {
                    EntityReference bsd_installment = (EntityReference)enTarget["bsd_installment"];
                    Entity uins = new Entity(bsd_installment.LogicalName, bsd_installment.Id);
                    uins["bsd_paymentnoticesdate"] = (DateTime)enTarget["bsd_billdate"];
                    service.Update(uins);
                }
            }
        }
    }
}
