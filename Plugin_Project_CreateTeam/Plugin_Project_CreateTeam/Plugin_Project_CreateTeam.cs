using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Project_CreateTeam
{
    public class Plugin_Project_CreateTeam : IPlugin
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
            Entity enUser = service.Retrieve("systemuser", context.UserId, new ColumnSet(new string[] { "businessunitid" }));

            EntityReference refProject = enProject.ToEntityReference();

            Guid teamId = CreateTeam(projectCode, "CCR-TEAM", enUser);
            ShareTeams(refProject, new EntityReference("team", teamId));

            teamId = CreateTeam(projectCode, "FINANCE-TEAM", enUser);
            ShareTeams(refProject, new EntityReference("team", teamId));

            teamId = CreateTeam(projectCode, "SALE-TEAM", enUser);
            ShareTeams(refProject, new EntityReference("team", teamId));

            teamId = CreateTeam(projectCode, "SALE-MGT", enUser);
            ShareTeams(refProject, new EntityReference("team", teamId));

            teamId = CreateTeam(projectCode, "SALE-ADMIN", enUser);
            ShareTeams(refProject, new EntityReference("team", teamId));
        }

        private Guid CreateTeam(string projectCode, string department, Entity enUser)
        {
            traceService.Trace("CreateTeam " + department);
            Entity team = new Entity("team");
            team["name"] = $"{projectCode}-{department}";
            team["businessunitid"] = enUser.Contains("businessunitid") ? enUser["businessunitid"] : null;
            team["administratorid"] = enUser.ToEntityReference();
            team["teamtype"] = new OptionSetValue(1);   //Access
            Guid id = service.Create(team);
            return id;
        }

        private void ShareTeams(EntityReference sharedRecord, EntityReference shareTeams)
        {
            traceService.Trace("ShareTeams");

            AccessRights Access_Rights = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess;
            var grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = Access_Rights,
                    Principal = shareTeams
                },
                Target = sharedRecord
            };
            service.Execute(grantAccessRequest);
        }
    }
}