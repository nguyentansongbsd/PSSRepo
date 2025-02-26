using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Create_UpdateLandValueUnits
{
    public class Plugin_Create_UpdateLandValueUnitsIPluginExecutionContext : IPlugin
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
            Guid recordId = entity.Id;
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            if (((OptionSetValue)enCreated["bsd_type"]).Value == 100000000)
            {
                if (enCreated.Contains("bsd_units"))
                {
                    EntityReference enUnitRef = (EntityReference)enCreated["bsd_units"];

                    var query = new QueryExpression("salesorder");
                    query.Criteria.AddCondition("bsd_unitnumber", ConditionOperator.Equal, enUnitRef.Id.ToString());
                    var rs = service.RetrieveMultiple(query);
                    if (rs.Entities.Count > 0)
                    {
                        Entity enUpdate=new Entity(enCreated.LogicalName, enCreated.Id);
                        enUpdate["bsd_optionentry"]=new EntityReference(rs.Entities[0].LogicalName, rs.Entities[0].Id);
                        service.Update(enUpdate);

                    }
                }
            }
        }
    }
}
