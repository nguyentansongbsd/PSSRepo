using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugin_Account_Create_Update
{
    public class Plugin_Account_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName == "Update")
            {
                traceService.Trace("Update");
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                decimal bsd_totalamountofownership = enTarget.Contains("bsd_totalamountofownership") ? ((Money)enTarget["bsd_totalamountofownership"]).Value : 0;
                int bsd_loyaltypointspa = enTarget.Contains("bsd_loyaltypointspa") ? (int)enTarget["bsd_loyaltypointspa"] : 0;
                Entity enUp = new Entity(target.LogicalName, target.Id);
                decimal bsd_loyaltypoints = 0;
                if (bsd_totalamountofownership > 0) bsd_loyaltypoints = bsd_loyaltypointspa + Math.Floor(bsd_totalamountofownership / 1_000_000m);
                enUp["bsd_loyaltypoints"] = bsd_loyaltypoints;
                service.Update(enUp);
                traceService.Trace("done");
                int statuscode = enTarget.Contains("statuscode") ? ((OptionSetValue)enTarget["statuscode"]).Value : 0;
                if (statuscode == 2)
                {
                    service = factory.CreateOrganizationService(Guid.Parse("d90ce220-655a-e811-812e-3863bb36dc00")); //this.context.UserId
                    Init(target);
                }
            }
        }
        private void Init(Entity target)
        {
            try
            {
                bool hadAdvance = checkAdvance(target);
                bool hadOptionEntry = checkOptionEntry(target);
                bool hadQuote = checQuote(target);
                if (hadAdvance == true || hadOptionEntry == true || hadQuote == true) throw new InvalidPluginExecutionException("The customer has transactions and cannot perform this action.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private bool checkAdvance(Entity target)
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_advancepayment"">
                    <attribute name=""bsd_name"" />
                    <filter>
                      <condition attribute=""bsd_customer"" operator=""eq"" value=""{target.Id}"" />
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
        private bool checkOptionEntry(Entity target)
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""salesorder"">
                    <attribute name=""name"" />
                    <filter>
                      <condition attribute=""customerid"" operator=""eq"" value=""{target.Id}"" />
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
        private bool checQuote(Entity target)
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""quote"">
                    <attribute name=""name"" />
                    <filter>
                      <condition attribute=""customerid"" operator=""eq"" value=""{target.Id}"" />
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
