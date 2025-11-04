using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Approve
{
    public class Plugin_Approve : IPlugin
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
            en = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("statuscode"));
            if (((OptionSetValue)en["statuscode"]).Value != 100000000)
                return;
            // Duyệt bằng PA vs user Thiện hoặc Hân thì gán người duyệt bằng user Mai Duc Phu
            Guid userId = context.UserId == Guid.Parse("093187a1-27d5-ed11-a7c7-000d3aa14877") || context.UserId == Guid.Parse("d90ce220-655a-e811-812e-3863bb36dc00") ? Guid.Parse("e7c67a0e-6c9e-e711-8111-3863bb36dc00") : context.UserId;
            Entity enUpdate = new Entity(en.LogicalName, en.Id);
            enUpdate["bsd_approvedate"] = DateTime.UtcNow;
            enUpdate["bsd_approver"] = new EntityReference("systemuser", userId);
            service.Update(enUpdate);
        }
    }
}
