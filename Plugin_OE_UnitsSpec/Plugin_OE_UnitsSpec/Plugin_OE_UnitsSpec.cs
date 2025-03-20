using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_OE_UnitsSpec
{
    public class Plugin_OE_UnitsSpec : IPlugin
    {
        IOrganizationService service = null;
        ITracingService traceService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");
            if (context.Depth > 4) return;

            Entity target = (Entity)context.InputParameters["Target"];
            Entity enOE = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_unitsspecification" }));

            if (target.Contains("bsd_unitsspecification"))
            {
                RemoveExistUnitsSpecDetail(enOE);

                if (enOE.Contains("bsd_unitsspecification"))
                {
                    AddUnitsSpecDetail(enOE);
                }
            }
        }

        private void RemoveExistUnitsSpecDetail(Entity enOE)
        {
            traceService.Trace("RemoveExistUnitsSpecDetail");
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
              <entity name=""bsd_unitsspecificationdetails"">
                <attribute name=""bsd_unitsspecificationdetailsid"" />
                <attribute name=""bsd_name"" />
                <attribute name=""bsd_no"" />
                <attribute name=""bsd_typeno"" />
                <attribute name=""bsd_typeofroomvn"" />
                <order attribute=""createdon"" />
                <link-entity name=""bsd_salesorder_bsd_unitsspecificationdetail"" from=""bsd_unitsspecificationdetailsid"" to=""bsd_unitsspecificationdetailsid"" intersect=""true"">
                  <filter>
                    <condition attribute=""salesorderid"" operator=""eq"" value=""{enOE.Id}"" />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                traceService.Trace("rs.Entities.Count " + rs.Entities.Count);

                EntityReferenceCollection relativeEntity = new EntityReferenceCollection();
                foreach (var item in rs.Entities)
                {
                    relativeEntity.Add(new EntityReference(item.LogicalName, item.Id));
                }
                Relationship relationship = new Relationship("bsd_salesorder_bsd_unitsspecificationdetails");
                service.Disassociate(enOE.LogicalName, enOE.Id, relationship, relativeEntity);
            }
        }

        private void AddUnitsSpecDetail(Entity enOE)
        {
            traceService.Trace("AddUnitsSpecDetail");
            EntityReference refUnitsSpec = (EntityReference)enOE["bsd_unitsspecification"];
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                      <entity name=""bsd_unitsspecificationdetails"">
                        <attribute name=""bsd_unitsspecificationdetailsid"" />
                        <attribute name=""bsd_name"" />
                        <attribute name=""bsd_no"" />
                        <attribute name=""bsd_typeno"" />
                        <attribute name=""bsd_typeofroomvn"" />
                        <filter>
                          <condition attribute=""bsd_unitsspecification"" operator=""eq"" value=""{refUnitsSpec.Id}"" />
                        </filter>
                        <order attribute=""createdon"" />
                      </entity>
                    </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                traceService.Trace("rs.Entities.Count " + rs.Entities.Count);

                EntityReferenceCollection relativeEntity = new EntityReferenceCollection();
                foreach (var item in rs.Entities)
                {
                    relativeEntity.Add(new EntityReference(item.LogicalName, item.Id));
                }
                Relationship relationship = new Relationship("bsd_salesorder_bsd_unitsspecificationdetails");
                service.Associate(enOE.LogicalName, enOE.Id, relationship, relativeEntity);
            }
        }
    }
}