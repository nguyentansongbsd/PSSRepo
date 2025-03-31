using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugin_Country_Create_Update
{
    public class Plugin_Country_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (context.Depth > 2) return;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName == "Create")
            {
                traceService.Trace("Create");
                Entity target = (Entity)context.InputParameters["Target"];
                traceService.Trace("step 1");
                if (target.Contains("bsd_countryname"))
                {
                    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""1"">
                      <entity name=""bsd_country"">
                        <attribute name=""bsd_countryid"" />
                        <filter>
                          <condition attribute=""bsd_countryid"" operator=""ne"" value=""{target.Id}"" />
                          <condition attribute=""bsd_countryname"" operator=""like"" value=""{(string)target["bsd_countryname"]}"" />
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (list.Entities.Count > 0)
                        throw new InvalidPluginExecutionException("Country Name is duplicated. Please check again.");
                }
            }
            else if (context.MessageName == "Update")
            {
                traceService.Trace("Update");
                Entity target = (Entity)context.InputParameters["Target"];
                traceService.Trace("step 1");
                if (target.Contains("bsd_countryname"))
                {
                    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""1"">
                      <entity name=""bsd_country"">
                        <attribute name=""bsd_countryid"" />
                        <filter>
                          <condition attribute=""bsd_countryid"" operator=""ne"" value=""{target.Id}"" />
                          <condition attribute=""bsd_countryname"" operator=""like"" value=""{(string)target["bsd_countryname"]}"" />
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (list.Entities.Count > 0)
                        throw new InvalidPluginExecutionException("Country Name is duplicated. Please check again.");
                }
            }
        }
    }
}
