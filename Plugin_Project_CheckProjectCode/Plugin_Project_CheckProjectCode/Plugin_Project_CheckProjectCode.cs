using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Project_CheckProjectCode
{
    public class Plugin_Project_CheckProjectCode : IPlugin
    {
        IOrganizationService service = null;
        ITracingService traceService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");
            if (context.Depth > 4) return;

            Entity target = (Entity)context.InputParameters["Target"];
            Entity enProject = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_projectcode" }));
            string projectCode = enProject.Contains("bsd_projectcode") ? (string)enProject["bsd_projectcode"] : string.Empty;
            if (string.IsNullOrWhiteSpace(projectCode))
                throw new InvalidPluginExecutionException("Project code is invalid. Please check again.");

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_project"">
                <attribute name=""bsd_name"" />
                <filter>
                  <condition attribute=""bsd_projectcode"" operator=""eq"" value=""{projectCode}"" />
                  <condition attribute=""bsd_projectid"" operator=""ne"" value=""{enProject.Id}"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
                throw new InvalidPluginExecutionException("Project code already exists. Please check again.");
        }
    }
}