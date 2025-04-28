using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Net;
using System.Text;

namespace Plugin_BulkWaiver_Create_Update
{
    public class Plugin_BulkWaiver_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceServiceClass = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (context.Depth > 3) return;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceServiceClass = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName == "Create")
            {
                Entity target = (Entity)context.InputParameters["Target"];
                if (target.Contains("bsd_name"))
                {
                    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""1"">
                      <entity name=""bsd_bulkwaiver"">
                        <attribute name=""bsd_bulkwaiverid"" />
                        <filter>
                          <condition attribute=""bsd_bulkwaiverid"" operator=""ne"" value=""{target.Id}"" />
                          <condition attribute=""bsd_name"" operator=""eq"" value=""{(string)target["bsd_name"]}"" />
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (rs.Entities.Count > 0) throw new InvalidPluginExecutionException("Name of bulk waiver already exists.");
                }
            }
            else if (context.MessageName == "Update")
            {
                Entity target = (Entity)context.InputParameters["Target"];
                if (target.Contains("bsd_name"))
                {
                    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""1"">
                      <entity name=""bsd_bulkwaiver"">
                        <attribute name=""bsd_bulkwaiverid"" />
                        <filter>
                          <condition attribute=""bsd_bulkwaiverid"" operator=""ne"" value=""{target.Id}"" />
                          <condition attribute=""bsd_name"" operator=""eq"" value=""{(string)target["bsd_name"]}"" />
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (rs.Entities.Count > 0) throw new InvalidPluginExecutionException("Name of bulk waiver already exists.");
                }
            }
        }
    }
}