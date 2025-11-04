using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_CreatePriceListItem
{
    public class Plugin_CreatePriceListItem : IPlugin
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
            try
            {
                if (context.Depth > 3 || this.context.MessageName != "Create")
                    return;
                entity = (Entity)this.context.InputParameters["Target"];
                if (!entity.Contains("amount") || ((Money)entity["amount"]).Value<=0)
                    throw new InvalidPluginExecutionException("The product has an invalid price. Please check the information again.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }

        }
    }
}
