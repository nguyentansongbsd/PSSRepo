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
        IOrganizationService service = null;
        ITracingService traceService = null;
        Entity target = null;
        Entity en = null;
        EntityReference refProject = null;
        string projectCode = "";
        EntityCollection rs = null;
        public Plugin_AutoShareRecord_V2_2(IOrganizationService _service, ITracingService _traceService, Entity _target)
        {
            service = _service;
            traceService = _traceService;
            target = _target;
        }

        public void Run_ProcessShareTeam(IPluginExecutionContext _context)
        {
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
                case "bsd_documents":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM", "SALE-MGT", "CCR-TEAM", "SALE-TEAM", "SALE-ADMIN" });
                    break;

                case "bsd_genpaymentnotices":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_genaratewarningnotices":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_bulksendmailmanager":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    var rs = GetDetailBulkMailManager();
                    foreach (var item in rs.Entities)
                    {
                        Run_ShareTemProject(false, new List<string> { "FINANCE-TEAM" }, item);
                    }
                    //share email detail luôn.
                    break;
                case "email":
                    if (!target.Contains("bsd_bulksendmailmanager")) return;
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_bankingloan":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM", "SALE-MGT" });
                    break;
                case "bsd_landvalue":
                    Run_ShareTemProject(true, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_updatelandvalue":
                    Run_ShareTemProject(true, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_applybankaccountunits":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    Run_ShareTemProject(false, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_waiverapproval":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_updateestimatehandoverdate":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_updateestimatehandoverdatedetail":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_bulkwaiver":
                case "bsd_bulkwaiverdetail":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_bulkchangemanagementfee":
                    Run_ShareTemProject(true, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_bulkchangemanagementfeedetail":
                    Run_ShareTemProject(true, new List<string> { "SALE-MGT" });
                    break;
                case "bsd_interestsimulation":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_transactionpayment":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_waiverapprovaldetail":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_interestsimulationdetail":
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;

                case "bsd_generatehandovernotices":
                    if (!target.Contains("bsd_project")) return;
                    Run_ShareTemProject(true, new List<string> { "FINANCE-TEAM" });
                    break;
                case "bsd_handovernotice":
                    break;
            }
        }

        public void Run_ShareTemProject(bool hasWrite = false, List<string> teamShares = null, Entity enShare = null)
        {
            traceService.Trace($"Run_ShareTemProject {target.LogicalName}");

            en = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
            refProject = GetProject();
            if (refProject == null)
            {
                traceService.Trace("No project found for the target entity.");
                return;
            }
            projectCode = GetProjectCode(refProject);
            rs = GetTeams(projectCode);
            traceService.Trace(target.LogicalName);

            if (enShare == null)
            {
                enShare = target;
            }
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {

                foreach (Entity team in rs.Entities)
                {
                    if (teamShares != null)
                    {
                        if (teamShares.Contains(((string)team["name"]).Replace($"{projectCode}-", "")))
                            ShareTeams(enShare.ToEntityReference(), team.ToEntityReference(), hasWrite);
                    }
                }
            }
        }

        public void ShareTeams(EntityReference sharedRecord, EntityReference shareTeams, bool hasWriteShare)
        {
            traceService.Trace("ShareTeams");

            AccessRights Access_Rights = AccessRights.ReadAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess;
            if (hasWriteShare)
                Access_Rights |= AccessRights.WriteAccess;

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

        public string GetProjectCode(EntityReference refProject)
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

        public EntityCollection GetTeams(string projectCode)
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
                    <value>{projectCode}-SALE-ADMIN</value>
                  </condition>
                </filter>
              </entity>
            </fetch>";
            try
            {

                EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                traceService.Trace("rs.Entities.Count " + rs.Entities.Count);
                return rs;
            }
            catch (Exception ex)
            {
                traceService.Trace("Error in GetTeams: " + fetchXml);
                return null;
            }
        }
        private EntityCollection GetDetailBulkMailManager()
        {
            traceService.Trace("GetDetailBulkMailManager");
            var query = new QueryExpression("email");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_bulksendmailmanager", ConditionOperator.Equal, en.Id);
            EntityCollection rs = service.RetrieveMultiple(query);
            traceService.Trace("rs.Entities.Count " + rs.Entities.Count);
            return rs;
        }
        public EntityReference GetProject()
        {

            EntityReference enProjectRef2 = null;
            EntityReference enMasterRef = null;

            Entity enMaster = null;
            switch (target.LogicalName)
            {
                case "email":
                    enMasterRef = (EntityReference)en["bsd_bulksendmailmanager"];
                    enMaster = service.Retrieve(enMasterRef.LogicalName, enMasterRef.Id, new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;
                case "bsd_generatehandovernotices":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_bulksendmailmanager":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;

                case "bsd_genaratewarningnotices":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;
                case "bsd_genpaymentnotices":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;

                case "bsd_documents":
                    if (!en.Contains("bsd_project")) return null;
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
                case "bsd_bulkwaiverdetail":
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
                    enMasterRef = (EntityReference)en["bsd_optionentry"];
                    enMaster = service.Retrieve(enMasterRef.LogicalName, enMasterRef.Id, new ColumnSet(true));
                    enProjectRef2 = (EntityReference)enMaster["bsd_project"];
                    break;
                case "bsd_bulkchangemanagementfeedetail":
                    enProjectRef2 = (EntityReference)en["bsd_project"];
                    break;


            }
            return enProjectRef2;
        }
    }
}
