using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLugin_UnitType_CheckUnitTypeCode
{
    public class PLugin_UnitType_CheckUnitTypeCode : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        Entity target = null;
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
                if (this.context.MessageName != "Create" && this.context.MessageName != "Update") return;
                this.target = (Entity)this.context.InputParameters["Target"];
                if (!this.target.Contains("bsd_name")) return;

                bool isHad = checkUnitTypeCode();
                if (isHad == true) throw new InvalidPluginExecutionException("Unit types code already exists.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }

        private bool checkUnitTypeCode()
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch top=""1"">
                  <entity name=""bsd_unittype"">
                    <attribute name=""bsd_unittypeid"" />
                    <filter>
                      <condition attribute=""bsd_name"" operator=""eq"" value=""{this.target["bsd_name"]}"" />
                      <condition attribute=""bsd_unittypeid"" operator=""ne"" value=""{this.target.Id}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result.Entities.Count > 0) return true;
                else return false;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
