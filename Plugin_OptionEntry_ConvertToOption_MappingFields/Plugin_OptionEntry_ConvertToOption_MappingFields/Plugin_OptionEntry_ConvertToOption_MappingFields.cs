using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_OptionEntry_ConvertToOption_MappingFields
{
    public class Plugin_OptionEntry_ConvertToOption_MappingFields : IPlugin
    {
        private IPluginExecutionContext context = null;
        private IOrganizationServiceFactory factory = null;
        private IOrganizationService service = null;
        private ITracingService tracingService = null;

        private Entity target = null;
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
                if (this.context.Depth > 3) return;
                if (this.context.MessageName != "Update") return;
                Entity _target = (Entity)this.context.InputParameters["Target"];
                this.target = this.service.Retrieve(_target.LogicalName,_target.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                if (this.target.Contains("bsd_unittype")) return;

                if (((OptionSetValue)this.target["statuscode"]).Value != 100000000) return; // 100000000 = Option

                MappingFiels();
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void MappingFiels()
        {
            try
            {
                Entity enOption = new Entity(this.target.LogicalName, this.target.Id);
                enOption["bsd_unittype"] = getUnitType();
                enOption["bsd_unitsspecification"] = null;
                this.service.Update(enOption);
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
                tracingService.Trace("get unit type");
                if (!this.target.Contains("bsd_unitnumber")) return null;
                Entity enUnit = this.service.Retrieve(((EntityReference)this.target["bsd_unitnumber"]).LogicalName, ((EntityReference)this.target["bsd_unitnumber"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { "bsd_unittype" }));
                if (!enUnit.Contains("bsd_unittype")) return null;
                tracingService.Trace("Có unit type");
                return (EntityReference)enUnit["bsd_unittype"];
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
