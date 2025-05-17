using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_AutoShareRecord
{
    class Plugin_AutoShareRecord_V2_2
    {
        static IOrganizationService service = null;
        static ITracingService traceService = null;
        static Entity target = null;
        static Entity en = null;
        static EntityReference refProject = null;
        static string projectCode = "";
        static EntityCollection rs = null;
        public static void Run_ProcessShareTeam(IOrganizationService _service, ITracingService _traceService, Entity _target, IPluginExecutionContext _context)
        {
            service = _service;
            traceService = _traceService;
            target = _target;
            traceService.Trace("Plugin_AutoShareRecord_V2_2");


            switch (target.LogicalName)
            {
                //case "bsd_updateduedate":
                //    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM", "SALE-TEAM" });
                //    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                //    break;//
                //case "bsd_updateduedatedetail":
                //    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM", "SALE-TEAM" });
                //    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                //    break;//
                //case "bsd_updateduedateoflastinstallmentapprove":
                //    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                //    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                //    break;//
                //case "bsd_updateduedateoflastinstallment":
                //    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                //    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                //    break;//
                case "bsd_bankingloan":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_documents":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM", "CCR-TEAM", "SALE-TEAM", "SALE-MGT" });
                    break;
                case "bsd_landvalue":
                    Run_ShareTemProject(true, new List<string> { "SALE-TEAM" });
                    break;
                case "bsd_updatelandvalue":
                    Run_ShareTemProject(true, new List<string> { "SALE-TEAM" });
                    break;
                case "bsd_applybankaccountunits":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_waiverapproval":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_updateestimatehandoverdate":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_updateestimatehandoverdatedetail":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_bulkwaiver":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_bulkchangemanagementfee":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_terminateletter":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_interestsimulation":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_transactionpayment":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_waiverapprovaldetail":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_interestsimulationdetail":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
            }
        }

        public static void Run_ShareTemProject(bool hasWrite = false, List<string> teamShares = null)
        {
            traceService.Trace($"Run_ShareTemProject {target.LogicalName}");

            en = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
            refProject = GetProject();
            projectCode = GetProjectCode(refProject);
            rs = GetTeams(projectCode);
            traceService.Trace(target.LogicalName);
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {

                bool hasWriteShare = false;
                foreach (Entity team in rs.Entities)
                {
                    if (teamShares != null)
                    {
                        if (teamShares.Contains((string)team["name"]))
                            ShareTeams(target.ToEntityReference(), team.ToEntityReference(), hasWriteShare);
                    }
                }
            }
        }

        public static void ShareTeams(EntityReference sharedRecord, EntityReference shareTeams, bool hasWriteShare)
        {
            traceService.Trace("ShareTeams");

            AccessRights Access_Rights = AccessRights.ReadAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess;
            if (hasWriteShare)
                Access_Rights |= AccessRights.WriteAccess ;

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

        public static string GetProjectCode(EntityReference refProject)
        {
            traceService.Trace("GetProjectCode");

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_project"">
                <attribute name=""bsd_projectcode"" />
                <filter>
                  <condition attribute=""bsd_projectid"" operator=""eq"" value=""{refProject.Id}"" />
                  <condition attribute=""bsd_projectcode"" operator=""not-null"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                return (string)rs.Entities[0]["bsd_projectcode"];
            }
            return string.Empty;
        }

        public static EntityCollection GetTeams(string projectCode)
        {
            traceService.Trace("GetTeam");
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""team"">
                <attribute name=""name"" />
                <filter>
                  <condition attribute=""name"" operator=""in"">
                    <value>{projectCode}-CCR-TEAM</value>
                    <value>{projectCode}-FINANCE-TEAM</value>
                    <value>{projectCode}-SALE-TEAM</value>
                    <value>{projectCode}-SALE-MGT</value>
                  </condition>
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            traceService.Trace("rs.Entities.Count " + rs.Entities.Count);
            return rs;
        }
        public static EntityReference GetProject()
        {
            EntityReference enProjectRef2 = null;
            EntityReference enMasterRef = null;

            Entity enMaster =null;
            switch (target.LogicalName)
            {

                case "bsd_documents":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_landvalue":
                    enMasterRef = (EntityReference)en["bsd_updatelandvalue"];
                    enMaster = service.Retrieve(enMasterRef.LogicalName, enMasterRef.Id, new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;
                case "bsd_updatelandvalue":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_updateduedate":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_updateduedatedetail":
                    enMasterRef = (EntityReference)en["bsd_updateduedate"];
                    enMaster = service.Retrieve(enMasterRef.LogicalName, enMasterRef.Id, new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;
                case "bsd_bankingloan":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_updateduedateoflastinstallmentapprove":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_updateduedateoflastinstallment":
                    enMasterRef = (EntityReference)en["bsd_updateduedateoflastinstapprove"];
                    enMaster = service.Retrieve(enMasterRef.LogicalName, enMasterRef.Id, new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;
                case "bsd_applybankaccountunits":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_waiverapproval":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_updateestimatehandoverdate":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_updateestimatehandoverdatedetail":
                    enMasterRef = (EntityReference)en["bsd_updateestimatehandoverdate"];
                    enMaster = service.Retrieve(enMasterRef.LogicalName, enMasterRef.Id, new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;
                case "bsd_bulkwaiver":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_bulkchangemanagementfee":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_terminateletter":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_interestsimulation":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_transactionpayment":
                    enMasterRef = (EntityReference)en["bsd_payment"];
                    enMaster = service.Retrieve(enMasterRef.LogicalName, enMasterRef.Id, new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;
                case "bsd_waiverapprovaldetail":
                    enMasterRef = (EntityReference)en["bsd_waiverapproval"];
                    enMaster = service.Retrieve(enMasterRef.LogicalName, enMasterRef.Id, new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;
                case "bsd_interestsimulationdetail":
                    enMasterRef = (EntityReference) en["bsd_interestsimulation"];
                    enMaster=service.Retrieve(enMasterRef.LogicalName,enMasterRef.Id,new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;


            }
            return enProjectRef2;
        }
    }
}
