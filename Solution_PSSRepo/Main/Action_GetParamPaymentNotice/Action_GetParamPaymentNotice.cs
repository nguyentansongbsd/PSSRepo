using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_GetParamPaymentNotice
{
    public class Action_GetParamPaymentNotice : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            // Lấy context
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity paymentNotice = service.Retrieve("bsd_customernotices", new Guid(context.InputParameters["id"].ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var enProjectRef = (EntityReference)paymentNotice["bsd_project"];
            var enProject = service.Retrieve(enProjectRef.LogicalName, enProjectRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var enOPRef = (EntityReference)paymentNotice["bsd_optionentry"];
            var enOP = service.Retrieve(enOPRef.LogicalName, enOPRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var enInsRef = (EntityReference)paymentNotice["bsd_paymentschemedetail"];
            var enIns = service.Retrieve(enInsRef.LogicalName, enInsRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var bsd_ordernumber = (int)enIns["bsd_ordernumber"];
            var bsd_ordernumbernd = "";
            if ((bsd_ordernumber) == 2)
            {
                bsd_ordernumbernd = "2nd";
            }
            else
                if ((bsd_ordernumber) == 3)
            {
                bsd_ordernumbernd = "3nd";
            }
            else
            {
                bsd_ordernumbernd = $"{bsd_ordernumber}th";
            }
            var bsd_dadate = enOP.Contains("bsd_dadate")?((DateTime)enOP["bsd_dadate"]).AddHours(7).ToString("dd/MM/yyyy"):"_____";
            var bsd_amountofthisphase = ((Money)enIns["bsd_amountofthisphase"]).Value.ToString("N0");
            var bsd_duedate = ((DateTime)enIns["bsd_duedate"]).AddHours(7).ToString("dd/MM/yyyy");
            context.OutputParameters["bsd_duedate"] = bsd_duedate;
            context.OutputParameters["bsd_ordernumber"] = bsd_ordernumber;
            context.OutputParameters["bsd_ordernumbernd"] = bsd_ordernumbernd;
            context.OutputParameters["bsd_amountofthisphase"] = bsd_amountofthisphase;
            context.OutputParameters["bsd_dadate"] = bsd_dadate;

        }
    }

}
