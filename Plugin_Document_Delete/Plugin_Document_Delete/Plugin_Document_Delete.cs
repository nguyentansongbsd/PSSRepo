using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Document_Delete
{
    public class Plugin_Document_Delete : IPlugin
    {
        private IPluginExecutionContext context = null;
        private IOrganizationServiceFactory factory = null;
        private IOrganizationService service = null;
        private ITracingService tracingService = null;

        private Entity en = null;
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
                if (this.context.MessageName != "Delete") return;
                this.en = this.context.PreEntityImages["document"];
                Guid templateId =  getTemplate();
                if (templateId != Guid.Empty)
                    this.service.Delete("documenttemplate", templateId);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private Guid getTemplate()
        {
            try
            {
                if (!this.en.Contains("bsd_name")) return Guid.Empty;
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch top=""1"">
                  <entity name=""documenttemplate"">
                    <attribute name=""documenttemplateid"" />
                    <filter>
                      <condition attribute=""name"" operator=""eq"" value=""{this.en["bsd_name"]}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection enTemplate = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (enTemplate == null || enTemplate.Entities.Count <= 0) return Guid.Empty;
                return (Guid)enTemplate.Entities[0]["documenttemplateid"];
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
