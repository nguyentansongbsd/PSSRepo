using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_PhasesLaunch_PreventAddNewItem
{
    public class Plugin_PhasesLaunch_PreventAddNewItem : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(((IExecutionContext)service1).UserId));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            int num = 0;
            if (((IExecutionContext)service1).MessageName == "Create" || ((IExecutionContext)service1).MessageName == "Update")
            {
                Entity inputParameter = (Entity)((DataCollection<string, object>)((IExecutionContext)service1).InputParameters)["Target"];
                if (inputParameter.LogicalName == "bsd_promotion" || inputParameter.LogicalName == "bsd_unitlaunched")
                {
                    string str = !(inputParameter.LogicalName == "bsd_promotion") ? "bsd_phaseslaunchid" : "bsd_phaselaunch";
                    Entity entity1 = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[1]
                    {
                        str
                    }));
                    if (entity1.Contains(str))
                    {
                        Entity entity2 = this.service.Retrieve("bsd_phaseslaunch", ((EntityReference)entity1[str]).Id, new ColumnSet(new string[1]
                        {
                            "statuscode"
                        }));
                        if (((OptionSetValue)entity2["statuscode"]).Value == 100000000 || ((OptionSetValue)entity2["statuscode"]).Value == 100000001 || ((OptionSetValue)entity2["statuscode"]).Value == 100000002)
                            num = 1;
                    }
                }
            }
            else if (((IExecutionContext)service1).MessageName == "Delete")
            {
                EntityReference inputParameter = (EntityReference)((DataCollection<string, object>)((IExecutionContext)service1).InputParameters)["Target"];
                if (inputParameter.LogicalName == "bsd_promotion" || inputParameter.LogicalName == "bsd_unitlaunched" || inputParameter.LogicalName == "bsd_unit_preparing")
                {
                    string str = "";
                    if (inputParameter.LogicalName == "bsd_promotion")
                        str = "bsd_phaselaunch";
                    else if (inputParameter.LogicalName == "bsd_unitlaunched")
                        str = "bsd_phaseslaunchid";
                    else if (inputParameter.LogicalName == "bsd_unit_preparing")
                        str = "bsd_phareslaunch";
                    Entity entity3 = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[1]
                    {
                        str
                    }));
                    if (entity3.Contains(str))
                    {
                        Entity entity4 = this.service.Retrieve("bsd_phaseslaunch", ((EntityReference)entity3[str]).Id, new ColumnSet(new string[1]
                        {
                            "statuscode"
                        }));
                        if (((OptionSetValue)entity4["statuscode"]).Value == 100000000 || ((OptionSetValue)entity4["statuscode"]).Value == 100000001 || ((OptionSetValue)entity4["statuscode"]).Value == 100000002)
                            num = 2;
                    }
                }
            }
            else if (((IExecutionContext)service1).MessageName == "Associate")
            {
                if (((Relationship)service1.InputParameters["Relationship"]).SchemaName == "bsd_bsd_phaseslaunch_bsd_packageselling")
                {
                    var relatedEntities = (EntityReferenceCollection)service1.InputParameters["RelatedEntities"];
                    foreach (var relatedEntity in relatedEntities)
                    {
                        if (relatedEntity.LogicalName == "bsd_phaseslaunch")
                        {
                            tracingService.Trace(relatedEntity.LogicalName);
                            var phaseLaunchId = relatedEntity.Id;
                            var phaseLaunch = service.Retrieve("bsd_phaseslaunch", phaseLaunchId, new ColumnSet("statuscode"));
                            if (((OptionSetValue)phaseLaunch["statuscode"]).Value == 100000000 || ((OptionSetValue)phaseLaunch["statuscode"]).Value == 100000001 || ((OptionSetValue)phaseLaunch["statuscode"]).Value == 100000002)
                                num = 3;
                        }
                    }
                    //EntityReference inputParameter = (EntityReference)service1.InputParameters["Target"];
                    //Entity entity = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[1]
                    //{
                    //    "statuscode"
                    //}));
                }
            }
            else if (((IExecutionContext)service1).MessageName == "Disassociate" && ((Relationship)((DataCollection<string, object>)((IExecutionContext)service1).InputParameters)["Relationship"]).SchemaName == "bsd_bsd_phaseslaunch_bsd_packageselling")
            {
                EntityReference inputParameter = (EntityReference)((DataCollection<string, object>)((IExecutionContext)service1).InputParameters)["Target"];
                Entity entity = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[1]
                {
                    "statuscode"
                }));
                if (((OptionSetValue)entity["statuscode"]).Value == 100000000 || ((OptionSetValue)entity["statuscode"]).Value == 100000001 || ((OptionSetValue)entity["statuscode"]).Value == 100000002)
                    num = 4;
            }
            if (num > 0)
                throw new InvalidPluginExecutionException("This Phase had been launched or recovery. Cannot proceed this transaction.");
        }
    }
}
