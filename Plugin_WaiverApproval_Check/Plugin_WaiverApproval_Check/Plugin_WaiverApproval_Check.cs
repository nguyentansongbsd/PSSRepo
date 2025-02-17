using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_WaiverApproval_Check
{
    public class Plugin_WaiverApproval_Check : IPlugin
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
                if (this.context.MessageName != "Update") return;
                this.target = (Entity)this.context.InputParameters["Target"];
                if (((OptionSetValue)this.target["statuscode"]).Value != 100000001) return; // 100000001 = Approved

                bool isEmtyInstallmentWaiverList = checkInstallmentWaiverList();
                if (isEmtyInstallmentWaiverList == true) throw new InvalidPluginExecutionException("The list of waiver to be processed is currently empty. Please check again.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private bool checkInstallmentWaiverList()
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_waiverapprovaldetail"">
                    <attribute name=""bsd_name"" />
                    <filter>
                      <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                      <condition attribute=""bsd_waiverapproval"" operator=""eq"" value=""{this.target.Id}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result == null || result.Entities.Count <= 0) return true;
                return false;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
