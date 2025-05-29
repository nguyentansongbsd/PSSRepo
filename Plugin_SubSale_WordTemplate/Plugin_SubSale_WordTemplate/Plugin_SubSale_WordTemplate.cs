using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_SubSale_WordTemplate
{
    public class Plugin_SubSale_WordTemplate : IPlugin
    {
        IOrganizationService service = null;
        ITracingService traceService = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");
            if (context.Depth > 4) return;

            Entity target = (Entity)context.InputParameters["Target"];
            Entity enSubSale = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
            traceService.Trace("enSubSale " + enSubSale.Id);

            if ("Create".Equals(context.MessageName))
            {
                UpSubSale(enSubSale, "bsd_currentcustomer", "bsd_primarycontactcr", context);
            }
            else
            {   //Update
                UpSubSale(enSubSale, "bsd_newcustomer", "bsd_primarycontactnew", context);
            }
        }

        private void UpSubSale(Entity enSubSale, string fieldCustomer, string fieldUp, IPluginExecutionContext context)
        {
            traceService.Trace("UpSubSale");

            if (enSubSale.Contains(fieldCustomer))
            {
                EntityReference refCustomer = (EntityReference)enSubSale[fieldCustomer];
                if ("account".Equals(refCustomer.LogicalName))
                {
                    Entity enAccount = service.Retrieve(refCustomer.LogicalName, refCustomer.Id, new ColumnSet(new string[] { "primarycontactid" }));
                    if (enAccount.Contains("primarycontactid"))
                    {
                        traceService.Trace("" + ((EntityReference)enAccount["primarycontactid"]).Id);

                        Entity upSubSale = new Entity(enSubSale.LogicalName, enSubSale.Id);
                        upSubSale[fieldUp] = enAccount["primarycontactid"];
                        service.Update(upSubSale);
                        return;
                    }
                }
            }

            if ("Update".Equals(context.MessageName))
            {
                Entity upSubSale = new Entity(enSubSale.LogicalName, enSubSale.Id);
                upSubSale[fieldUp] = null;
                service.Update(upSubSale);
            }
        }
    }
}
