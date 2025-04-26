using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_OptionEntry_Gen_DANumber
{
    public class Plugin_OptionEntry_Gen_DANumber : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceS = null;

        Entity target = null;
        Entity enOE = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            traceS = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);

            this.target = (Entity)this.context.InputParameters["Target"];
            this.enOE = this.service.Retrieve(this.target.LogicalName, this.target.Id, new ColumnSet("bsd_agreementdate", "bsd_project", "bsd_optionno"));

            updateOE();
        }
        private void updateOE()
        {
            try
            {
                string daNumber = genDANumber();
                Entity enOE_update = new Entity(this.enOE.LogicalName, this.enOE.Id);
                enOE_update["bsd_danumber"] = daNumber;
                this.service.Update(enOE_update);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private string genDANumber()
        {
            try
            {
                string optionNo = (string)this.enOE["bsd_optionno"];
                string lastFourCharacters = optionNo.Substring(optionNo.Length - 4);
                string prefix = getPrefixDA();

                return prefix + lastFourCharacters;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private string getPrefixDA()
        {
            try
            {
                Entity enProject = this.service.Retrieve(((EntityReference)this.enOE["bsd_project"]).LogicalName, ((EntityReference)this.enOE["bsd_project"]).Id, new ColumnSet("bsd_prefixda"));
                return enProject.Contains("bsd_prefixda") ? enProject["bsd_prefixda"].ToString() : null;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
