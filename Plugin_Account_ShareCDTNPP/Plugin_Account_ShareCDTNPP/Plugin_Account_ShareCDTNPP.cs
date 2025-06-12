using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Account_ShareCDTNPP
{
    public class Plugin_Account_ShareCDTNPP : IPlugin
    {
        IOrganizationService service = null;
        ITracingService traceService = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");
            if (context.Depth > 4) return;

            Entity target = (Entity)context.InputParameters["Target"];
            Entity enAccount = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_businesstypesys", "primarycontactid" }));
            if (!enAccount.Contains("bsd_businesstypesys") || !enAccount.Contains("primarycontactid")) return;

            List<int> validValues = new List<int> { 100000002, 100000003 }; //CĐT/NPP
            OptionSetValueCollection bsd_businesstypesys = (OptionSetValueCollection)enAccount["bsd_businesstypesys"];
            if (validValues.Any(v => bsd_businesstypesys.Any(o => o.Value == v)))
            {
                EntityReference defaultTeam = GetDefaultTeam(context.UserId).ToEntityReference();
                traceService.Trace("defaultTeam " + defaultTeam.Id);

                ShareTeams(enAccount.ToEntityReference(), defaultTeam);

                EntityReference primaryContact = (EntityReference)enAccount["primarycontactid"];
                ShareTeams(primaryContact, defaultTeam);

                traceService.Trace("Done");
            }
        }

        private Entity GetDefaultTeam(Guid userId)
        {
            traceService.Trace("GetDefaultTeam");

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch distinct=""true"">
              <entity name=""team"">
                <attribute name=""teamid"" />
                <attribute name=""name"" />
                <attribute name=""isdefault"" />
                <filter>
                  <condition attribute=""isdefault"" operator=""eq"" value=""1"" />
                </filter>
                <link-entity name=""teammembership"" from=""teamid"" to=""teamid"" intersect=""true"">
                  <filter>
                    <condition attribute=""systemuserid"" operator=""eq"" value=""{userId}"" />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs != null && rs.Entities.Count > 0)
                return rs.Entities[0];

            return null;
        }

        public void ShareTeams(EntityReference sharedRecord, EntityReference shareTeams)
        {
            traceService.Trace($"ShareTeams");

            var grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.WriteAccess | AccessRights.ShareAccess,
                    Principal = shareTeams
                },
                Target = sharedRecord
            };
            service.Execute(grantAccessRequest);
        }
    }
}