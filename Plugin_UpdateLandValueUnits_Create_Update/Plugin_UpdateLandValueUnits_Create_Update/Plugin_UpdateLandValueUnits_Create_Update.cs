using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Net;
using System.Text;

namespace Plugin_UpdateLandValueUnits_Create_Update
{
    public class Plugin_UpdateLandValueUnits_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceServiceClass = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceServiceClass = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName == "Create")
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enUpdateLandValueUnits = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                int bsd_type = enUpdateLandValueUnits.Contains("bsd_type") ? ((OptionSetValue)enUpdateLandValueUnits["bsd_type"]).Value : 0;
                if (enUpdateLandValueUnits.Contains("bsd_optionentry") && enUpdateLandValueUnits.Contains("bsd_units") && bsd_type == 100000000)
                {
                    Entity enUnit = service.Retrieve(((EntityReference)enUpdateLandValueUnits["bsd_units"]).LogicalName, ((EntityReference)enUpdateLandValueUnits["bsd_units"]).Id, new ColumnSet(true));
                    Entity enOE = service.Retrieve(((EntityReference)enUpdateLandValueUnits["bsd_optionentry"]).LogicalName, ((EntityReference)enUpdateLandValueUnits["bsd_optionentry"]).Id, new ColumnSet(true));
                    Entity enUp = new Entity(target.LogicalName, target.Id);
                    decimal bsd_freightamount = enOE.Contains("bsd_freightamount") ? ((Money)enOE["bsd_freightamount"]).Value : 0;
                    decimal bsd_landvaluenew = enUpdateLandValueUnits.Contains("bsd_landvaluenew") ? ((Money)enUpdateLandValueUnits["bsd_landvaluenew"]).Value : 0;
                    decimal bsd_netsellingpricenew = enUpdateLandValueUnits.Contains("bsd_netsellingpricenew") ? ((Money)enUpdateLandValueUnits["bsd_netsellingpricenew"]).Value : 0;
                    decimal bsd_totalamount = enUpdateLandValueUnits.Contains("bsd_totalamount") ? ((Money)enUpdateLandValueUnits["bsd_totalamount"]).Value : 0;
                    decimal bsd_maintenancefeenew = enUpdateLandValueUnits.Contains("bsd_maintenancefeenew") ? ((Money)enUpdateLandValueUnits["bsd_maintenancefeenew"]).Value : 0;
                    decimal bsd_amountofthisphasecurrent = enUpdateLandValueUnits.Contains("bsd_amountofthisphasecurrent") ? ((Money)enUpdateLandValueUnits["bsd_amountofthisphasecurrent"]).Value : 0;
                    decimal bsd_netsaleablearea = enUnit.Contains("bsd_netsaleablearea") ? (decimal)enUnit["bsd_netsaleablearea"] : 0;
                    decimal bsd_landvaluedeductionnew = bsd_landvaluenew * bsd_netsaleablearea;
                    decimal bsd_totalvattaxnew = (bsd_netsellingpricenew - bsd_landvaluedeductionnew) * (decimal)0.1;
                    decimal bsd_totalamountnew = bsd_netsellingpricenew + bsd_totalvattaxnew + bsd_freightamount;
                    decimal bsd_valuedifference = Math.Round(bsd_totalamount - bsd_totalamountnew,0);
                    enUp["bsd_maintenancefeenew"] = new Money(bsd_freightamount);
                    enUp["bsd_landvaluedeductionnew"] = new Money(bsd_landvaluedeductionnew);
                    enUp["bsd_totalvattaxnew"] = new Money(bsd_totalvattaxnew);
                    enUp["bsd_valuedifference"] = new Money(bsd_valuedifference);
                    enUp["bsd_amountofthisphase"] = new Money(Math.Round(bsd_amountofthisphasecurrent - bsd_valuedifference, 0));
                    service.Update(enUp);
                }
            }
        }
    }
}