using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_UpdateUnitSpec
{
    public class Action_UpdateUnitSpec : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        EntityReference target = null;
        Entity enUnitSpec = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = factory.CreateOrganizationService(this.context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            this.target = (EntityReference)this.context.InputParameters["Target"];
            this.enUnitSpec = this.service.Retrieve(this.target.LogicalName, this.target.Id, new ColumnSet(new string[] { "bsd_unittype" }));

            addExisting();
        }
        private void addExisting()
        {
            try
            {
                AssociateRequest request = new AssociateRequest
                {
                    Target = this.target,
                    RelatedEntities = new EntityReferenceCollection { (EntityReference)this.enUnitSpec["bsd_unittype"] },
                    Relationship = new Relationship("bsd_bsd_unitsspecification_bsd_unittype")
                };
                this.service.Execute(request);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
