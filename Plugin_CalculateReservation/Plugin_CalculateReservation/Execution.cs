using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_CalculateReservation
{
    public class Execution : IPlugin
    {
        IOrganizationService service = null;
        ITracingService trace = null;
        Entity target = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            if (context.MessageName == "Create")
            {
                target = context.InputParameters["Target"] as Entity;
                if (target.LogicalName == "quotedetail")
                {
                    if (target.Contains("productid") && target.Contains("quoteid"))
                    {
                        IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
                        Calculation cal = new Calculation(service, target);
                        cal.Calculate();
                    }
                }
            }
            else if (context.MessageName == "Update")
            {
                target = context.InputParameters["Target"] as Entity;
                if (target.LogicalName == "quote" && (target.Contains("bsd_taxcode") || target.Contains("bsd_discountlist") || target.Contains("bsd_discounts")||target.Contains("bsd_packagesellingamount")|| target.Contains("bsd_paymentscheme")||target.Contains("bsd_paymentscheme")))
                {
                    IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
                    QueryExpression q = new QueryExpression("quotedetail");
                    q.ColumnSet = new ColumnSet(new string[] {
                        "priceperunit",
                        "baseamount",
                        "productid",
                        "quoteid"
                    });
                    q.Criteria = new FilterExpression(LogicalOperator.And);
                    q.Criteria.AddCondition(new ConditionExpression("quoteid", ConditionOperator.Equal, target.Id));
                    q.TopCount = 1;
                    EntityCollection etnc = service.RetrieveMultiple(q);
                    if(etnc.Entities.Count >0)
                    {
                        Calculation cal = new Calculation(service,etnc.Entities[0]);
                        cal.Calculate();
                    }
                }
            }
        }
    }
}
