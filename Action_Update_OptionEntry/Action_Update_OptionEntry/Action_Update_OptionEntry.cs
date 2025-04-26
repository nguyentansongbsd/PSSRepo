using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Update_OptionEntry
{
    public class Action_Update_OptionEntry : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        EntityReference target = null;
        Entity enOE = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = factory.CreateOrganizationService(this.context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            this.target = (EntityReference)this.context.InputParameters["Target"];
            this.enOE = this.service.Retrieve(this.target.LogicalName, this.target.Id, new ColumnSet(new string[] { "bsd_unitnumber", "bsd_unittype", "bsd_unitsspecification" }));

            updateOE();
        }
        private void updateOE()
        {
            try
            {
                EntityReference enfUnitType = getUnitType();
                if (enfUnitType == null) return;
                if (this.enOE.Contains("bsd_unittype")) enfUnitType = (EntityReference)this.enOE["bsd_unittype"];
                EntityReference enfUnitSpec = getUnitSpec(enfUnitType);

                Entity enOE_Update = new Entity(this.enOE.LogicalName, this.enOE.Id);
                if (!this.enOE.Contains("bsd_unittype"))
                    enOE_Update["bsd_unittype"] = enfUnitType;
                if (!this.enOE.Contains("bsd_unitsspecification") && enfUnitSpec != null)
                    enOE_Update["bsd_unitsspecification"] = enfUnitSpec;
                this.service.Update(enOE_Update);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityReference getUnitType()
        {
            try
            {
                Entity enUnit = this.service.Retrieve(((EntityReference)this.enOE["bsd_unitnumber"]).LogicalName, ((EntityReference)this.enOE["bsd_unitnumber"]).Id, new ColumnSet(new string[] { "bsd_unittype" }));
                return enUnit.Contains("bsd_unittype") ? (EntityReference)enUnit["bsd_unittype"] : null;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityReference getUnitSpec(EntityReference unitType)
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_unitsspecification"">
                    <filter>
                      <condition attribute=""statuscode"" operator=""eq"" value=""100000000"" />
                    </filter>
                    <link-entity name=""bsd_bsd_unitsspecification_bsd_unittype"" from=""bsd_unitsspecificationid"" to=""bsd_unitsspecificationid"" intersect=""true"">
                      <filter>
                        <condition attribute=""bsd_unittypeid"" operator=""eq"" value=""{unitType.Id}"" />
                      </filter>
                    </link-entity>
                  </entity>
                </fetch>";
                EntityCollection result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result.Entities.Count == 1)
                {
                    return result.Entities[0].ToEntityReference();
                }
                else return null;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
