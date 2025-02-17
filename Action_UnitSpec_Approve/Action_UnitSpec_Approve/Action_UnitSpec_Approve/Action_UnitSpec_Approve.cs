using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_UnitSpec_Approve
{
    public class Action_UnitSpec_Approve : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        EntityReference target = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            target = (EntityReference)context.InputParameters["Target"];
            Init();
        }
        private void Init()
        {
            try
            {
                updatedStatus();
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void updatedStatus()
        {
            try
            {
                Entity enUnitSpec = new Entity(this.target.LogicalName, this.target.Id);
                enUnitSpec["statuscode"] = new OptionSetValue(100000000); // 100000000 = approve
                this.service.Update(enUnitSpec);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
