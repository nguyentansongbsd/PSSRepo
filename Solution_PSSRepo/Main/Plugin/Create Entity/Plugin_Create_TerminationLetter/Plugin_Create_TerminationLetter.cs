using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Create_TerminationLetter
{
    public class Plugin_Create_TerminationLetter : IPlugin
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
            Guid recordId = entity.Id;
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            tracingService.Trace("Plugin_Create_TerminationLetter" + "id: " + entity.Id.ToString());
            if (!enCreated.Contains("bsd_optionentry")) return;
            var opRef = (EntityReference)enCreated["bsd_optionentry"];
            var query_bsd_optionentry = opRef.Id.ToString();
            tracingService.Trace("Plugin_Create_TerminationLetter" + "opRef.Id: " + opRef.Id.ToString());

            var query = new QueryExpression("bsd_warningnotices");
            query.TopCount = 2; query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, query_bsd_optionentry);
            query.AddOrder("createdon", OrderType.Descending);
            var result = service.RetrieveMultiple(query);
            var enupdate = new Entity("bsd_terminateletter", enCreated.Id);
            tracingService.Trace($"count {result.Entities.Count}");
            if (result.Entities.Count > 0)
            {
                if (result.Entities.Count > 1)
                {
                    enupdate["bsd_warning_notices_1"] = new EntityReference("bsd_warningnotices", result.Entities[1].Id);
                    enupdate["bsd_warning_notices_2"] = new EntityReference("bsd_warningnotices", result.Entities[0].Id);
                }
                else
                {
                    enupdate["bsd_warning_notices_1"] = new EntityReference("bsd_warningnotices", result.Entities[0].Id);

                }
                service.Update(enupdate);
            }
        }
    }
}
