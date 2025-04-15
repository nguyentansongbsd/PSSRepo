using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Contact_Deactive
{
    public class Action_Contact_Deactive : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        EntityReference target = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = factory.CreateOrganizationService(Guid.Parse("d90ce220-655a-e811-812e-3863bb36dc00")); //this.context.UserId
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Init();
        }
        private void Init()
        {
            try
            {
                this.target = (EntityReference)this.context.InputParameters["Target"];
                bool hadAdvance = checkAdvance();
                bool hadOptionEntry = checkOptionEntry();
                if (hadAdvance == true || hadOptionEntry == true) throw new InvalidPluginExecutionException("The customer has transactions and cannot perform this action.");
                updateContact();
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void updateContact()
        {
            try
            {
                Entity enContact = new Entity(this.target.LogicalName, this.target.Id);
                enContact["statecode"] = new OptionSetValue(1);
                enContact["statuscode"] = new OptionSetValue(2);
                this.service.Update(enContact);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private bool checkAdvance()
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_advancepayment"">
                    <attribute name=""bsd_name"" />
                    <filter>
                      <condition attribute=""bsd_customer"" operator=""eq"" value=""{this.target.Id}"" />
                      <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result == null || result.Entities.Count <= 0) return false;
                return true;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private bool checkOptionEntry()
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""salesorder"">
                    <attribute name=""name"" />
                    <filter>
                      <condition attribute=""customerid"" operator=""eq"" value=""{this.target.Id}"" />
                      <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection result = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result == null || result.Entities.Count <= 0) return false;
                return true;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
