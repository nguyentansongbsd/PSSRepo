using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLugin_PriceListItems_Import
{
    public class PLugin_PriceListItems_Import : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;

        Entity target = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Init();
        }
        private void Init()
        {
            try
            {
                if (this.context.Depth > 3) return;
                if (this.context.MessageName != "Create") return;
                this.target = (Entity)this.context.InputParameters["Target"];
                if (!this.target.Contains("productid")) return;
                int sts = getUnit((EntityReference)this.target["productid"]);
                if (sts != 1) throw new InvalidPluginExecutionException("The product status is invalid. Please check the information again.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private int getUnit(EntityReference enfUnit)
        {
            try
            {
                Entity enUnit = this.service.Retrieve(enfUnit.LogicalName, enfUnit.Id, new ColumnSet(new string[]{ "statuscode" }));
                return ((OptionSetValue)enUnit["statuscode"]).Value;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
