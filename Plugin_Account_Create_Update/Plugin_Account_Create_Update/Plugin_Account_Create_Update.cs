using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugin_Account_Create_Update
{
    public class Plugin_Account_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName == "Update")
            {
                traceService.Trace("Update");
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                decimal bsd_totalamountofownership = enTarget.Contains("bsd_totalamountofownership") ? ((Money)enTarget["bsd_totalamountofownership"]).Value : 0;
                int bsd_loyaltypointspa = enTarget.Contains("bsd_loyaltypointspa") ? (int)enTarget["bsd_loyaltypointspa"] : 0;
                Entity enUp = new Entity(target.LogicalName, target.Id);
                decimal bsd_loyaltypoints = 0;
                if (bsd_totalamountofownership > 0) bsd_loyaltypoints = bsd_loyaltypointspa + Math.Floor(bsd_totalamountofownership / 1_000_000m);
                enUp["bsd_loyaltypoints"] = bsd_loyaltypoints;
                service.Update(enUp);
                traceService.Trace("done");
            }
        }
    }
}
