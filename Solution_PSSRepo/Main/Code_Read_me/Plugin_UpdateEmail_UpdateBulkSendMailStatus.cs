using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_UpdateEmail_UpdateBulkSendMailStatus
{
    public class Plugin_UpdateEmail_UpdateBulkSendMailStatus : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity entity = (Entity)context.InputParameters["Target"];
            Entity enTarget = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            //lấy bulkManager
            if (enTarget.Contains("bsd_bulksendmailmanager"))
            {
                var query = new QueryExpression("email");
                query.ColumnSet.AddColumn("statuscode");
                query.Criteria.AddCondition("bsd_bulksendmailmanager", ConditionOperator.Equal, ((EntityReference)enTarget["bsd_bulksendmailmanager"]).Id.ToString());
                query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
                var rs = service.RetrieveMultiple(query);

                var masterRef = (EntityReference)enTarget["bsd_bulksendmailmanager"];
                var master = service.Retrieve(masterRef.LogicalName, masterRef.Id, new ColumnSet("statuscode"));
                var enUpdate = new Entity(masterRef.LogicalName, masterRef.Id);
                if (rs.Entities.Count == 0)
                {
                    enUpdate["statuscode"] = new OptionSetValue(100000001);
                    service.Update(enUpdate);
                }
                else
                {
                    if (((OptionSetValue)master["statuscode"]).Value == 1)
                    {
                        enUpdate["statuscode"] = new OptionSetValue(100000000);
                        service.Update(enUpdate);

                    }
                }

            }
        }
    }
}
