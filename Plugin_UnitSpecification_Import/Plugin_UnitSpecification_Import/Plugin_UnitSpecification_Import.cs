using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_UnitSpecification_Import
{
    public class Plugin_UnitSpecification_Import : IPlugin
    {
        IPluginExecutionContext context = null;
        ITracingService tracingService = null;
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        Entity target = null;
        string relationShipName_NN = "bsd_bsd_unitsspecification_bsd_unittype";

        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = factory.CreateOrganizationService(this.context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Init();
        }
        private void Init()
        {
            try
            {
                if (this.context.MessageName != "Create") return;
                this.target = (Entity)this.context.InputParameters["Target"];
                if (!this.target.Contains("bsd_unittypeimport")) return;

                EntityCollection unitTypes = getUnitTypes();
                Relationship relationShip = new Relationship(this.relationShipName_NN);
                EntityReferenceCollection enrfCollection = new EntityReferenceCollection();
                foreach (var item in unitTypes.Entities)
                {
                    enrfCollection.Add(new EntityReference(item.LogicalName, item.Id));
                }

                this.service.Associate(this.target.LogicalName, this.target.Id, relationShip, enrfCollection);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityCollection getUnitTypes()
        {
            try
            {
                string[] unitTypeCodes = this.target["bsd_unittypeimport"].ToString().Split(',');
                EntityCollection unitTypes = new EntityCollection();
                foreach (var item in unitTypeCodes)
                {
                    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""1"">
                      <entity name=""bsd_unittype"">
                        <attribute name=""bsd_unittypeid"" />
                        <filter>
                          <condition attribute=""bsd_name"" operator=""eq"" value=""{item}"" />
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (result.Entities.Count > 0)
                    {
                        unitTypes.Entities.Add(result.Entities[0]);
                    }
                }
                return unitTypes;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
