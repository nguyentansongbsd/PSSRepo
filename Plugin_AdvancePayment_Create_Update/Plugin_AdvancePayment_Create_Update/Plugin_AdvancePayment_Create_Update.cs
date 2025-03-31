using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Net;
using System.Text;

namespace Plugin_AdvancePayment_Create_Update
{
    public class Plugin_AdvancePayment_Create_Update : IPlugin
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
            if (context.MessageName == "Create" && context.Depth <= 3)
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enAdvancePayment = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                Entity enUp = new Entity(target.LogicalName, target.Id);
                bool checkUp = false;
                if (enAdvancePayment.Contains("bsd_optionentry"))
                {
                    var fetchXml2 = $@"
                        <fetch>
                          <entity name='salesorder'>
                            <attribute name=""customerid"" />
                            <filter>
                              <condition attribute='salesorderid' operator='eq' value='{((EntityReference)enAdvancePayment["bsd_optionentry"]).Id}'/>
                              <condition attribute='customerid' operator='not-null'/>
                            </filter>
                          </entity>
                        </fetch>";
                    EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    foreach (Entity entity in list2.Entities)
                    {
                        enUp["bsd_customer"] = (EntityReference)entity["customerid"];
                        checkUp = true;
                    }
                }
                if (!enAdvancePayment.Contains("bsd_refundamount"))
                {
                    enUp["bsd_refundamount"] = new Money(0);
                    checkUp = true;
                }
                if (!enAdvancePayment.Contains("bsd_paidamount"))
                {
                    enUp["bsd_paidamount"] = new Money(0);
                    checkUp = true;
                }
                if (checkUp) service.Update(enUp);
            }
        }
    }
}