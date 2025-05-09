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
    class Plugin_AutoShareRecord_V2_1
    {
        static IOrganizationService service = null;
        static ITracingService traceService = null;
        static Entity target = null;
        public static void Run_Update(IOrganizationService _service, ITracingService _traceService, Entity _target, IPluginExecutionContext _context)
        {
            service = _service;
            traceService = _traceService;
            target = _target;
            traceService.Trace("Plugin_AutoShareRecord_V2_1 Run_Update");

            switch (target.LogicalName)
            {
                case "bsd_phaseslaunch":
                    Run_PhasesLaunch();
                    break;
                case "bsd_paymentscheme":
                    Run_PaymentScheme();
                    break;
                case "bsd_event":
                case "bsd_updatepricelist":
                    ShareTeams_OneEntity("bsd_project", "Sales", 100000000);
                    break;
            }


        }

        public static void Run_Create(IOrganizationService _service, ITracingService _traceService, Entity _target, IPluginExecutionContext _context)
        {
            service = _service;
            traceService = _traceService;
            target = _target;
            traceService.Trace("Plugin_AutoShareRecord_V2_1 Run_Create");

            switch (target.LogicalName)
            {
                case "bsd_followuplist":
                    ShareTeams_OneEntity("bsd_project", "Management");
                    break;
            }


        }

        public static void ShareTeams(EntityReference sharedRecord, EntityReference shareTeams, bool hasWriteShare)
        {
            traceService.Trace("ShareTeams");

            AccessRights Access_Rights = AccessRights.ReadAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess;
            if (hasWriteShare)
                Access_Rights |= AccessRights.WriteAccess | AccessRights.ShareAccess;

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
                    <value>{projectCode}_CR_Team</value>
                    <value>{projectCode}_Finance_Team</value>
                    <value>{projectCode}_Sales_Team</value>
                    <value>{projectCode}_Management_Team</value>
                  </condition>
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            traceService.Trace("rs.Entities.Count " + rs.Entities.Count);
            return rs;
        }

        private static void Run_PhasesLaunch()
        {
            traceService.Trace("Run_PhasesLaunch");

            if (target.Contains("statuscode") && ((OptionSetValue)target["statuscode"]).Value == 100000000) //Launched
            {
                Entity enPhasesLaunch = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_projectid", "bsd_discountlist" }));
                if (!enPhasesLaunch.Contains("bsd_projectid")) return;

                EntityReference refProject = (EntityReference)enPhasesLaunch["bsd_projectid"];
                string projectCode = GetProjectCode(refProject);
                EntityCollection rs = GetTeams(projectCode);
                if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
                {
                    EntityReference refPhasesLaunch = enPhasesLaunch.ToEntityReference();
                    EntityCollection rsPromotions = GetPromotions(refPhasesLaunch);
                    EntityReference refDiscountList = enPhasesLaunch.Contains("bsd_discountlist") ? (EntityReference)enPhasesLaunch["bsd_discountlist"] : null;
                    EntityCollection rsDiscounts = null;
                    if (refDiscountList != null)
                    {
                        rsDiscounts = GetDiscounts(refDiscountList);
                    }

                    EntityReference refTeam = null;
                    bool hasWriteShare = false;
                    foreach (Entity team in rs.Entities)
                    {
                        refTeam = team.ToEntityReference();
                        hasWriteShare = $"{projectCode}_Sales_Team".Equals((string)team["name"]);

                        ShareTeams(refPhasesLaunch, refTeam, hasWriteShare);

                        if (refDiscountList != null)
                        {
                            traceService.Trace("Share DiscountList");

                            ShareTeams(refDiscountList, refTeam, hasWriteShare);
                            if (rsDiscounts != null && rsDiscounts.Entities != null && rsDiscounts.Entities.Count > 0)
                            {
                                foreach (var discount in rsDiscounts.Entities)
                                {
                                    ShareTeams(discount.ToEntityReference(), refTeam, hasWriteShare);
                                }
                            }
                        }

                        if (rsPromotions != null && rsPromotions.Entities != null && rsPromotions.Entities.Count > 0)
                        {
                            foreach (var promotion in rsPromotions.Entities)
                            {
                                ShareTeams(promotion.ToEntityReference(), refTeam, hasWriteShare);
                            }
                        }
                    }
                }
            }
        }

        private static EntityCollection GetDiscounts(EntityReference refDiscountList)
        {
            traceService.Trace("GetDiscounts");
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_discount"">
                <attribute name=""bsd_name"" />
                <link-entity name=""bsd_bsd_discounttype_bsd_discount"" from=""bsd_discountid"" to=""bsd_discountid"" intersect=""true"">
                  <filter>
                    <condition attribute=""bsd_discounttypeid"" operator=""eq"" value=""{refDiscountList.Id}"" />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return rs;
        }

        private static EntityCollection GetPromotions(EntityReference refPhasesLaunch)
        {
            traceService.Trace("GetPromotions");
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_promotion"">
                <attribute name=""bsd_name"" />
                <filter>
                  <condition attribute=""bsd_phaselaunch"" operator=""eq"" value=""{refPhasesLaunch.Id}"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return rs;
        }

        private static void Run_PaymentScheme()
        {
            traceService.Trace("Run_PaymentScheme");

            if (target.Contains("statuscode") && ((OptionSetValue)target["statuscode"]).Value == 100000000) //Confirm
            {
                Entity enPS = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_project" }));
                if (!enPS.Contains("bsd_project")) return;

                EntityReference refProject = (EntityReference)enPS["bsd_project"];
                string projectCode = GetProjectCode(refProject);
                EntityCollection rs = GetTeams(projectCode);
                if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
                {
                    EntityReference refPS = enPS.ToEntityReference();
                    EntityCollection rsPSDetails = GetPaymentSchemeDetails(refPS);

                    EntityReference refTeam = null;
                    bool hasWriteShare = false;
                    foreach (Entity team in rs.Entities)
                    {
                        refTeam = team.ToEntityReference();
                        hasWriteShare = $"{projectCode}_CR_Team".Equals((string)team["name"]);

                        ShareTeams(refPS, refTeam, hasWriteShare);

                        if (rsPSDetails != null && rsPSDetails.Entities != null && rsPSDetails.Entities.Count > 0)
                        {
                            foreach (var ins in rsPSDetails.Entities)
                            {
                                ShareTeams(ins.ToEntityReference(), refTeam, hasWriteShare);
                            }
                        }
                    }
                }
            }
        }

        private static EntityCollection GetPaymentSchemeDetails(EntityReference refPS)
        {
            traceService.Trace("GetPaymentSchemeDetails");
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_name"" />
                <filter>
                  <condition attribute=""bsd_paymentscheme"" operator=""eq"" value=""{refPS.Id}"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return rs;
        }

        public static void ShareTeams_OneEntity(string projecField, string teamWriteShare, int status = -999)
        {
            traceService.Trace("ShareTeams_OneEntity");

            if (status != -999 && (!target.Contains("statuscode") || ((OptionSetValue)target["statuscode"]).Value != status))
                return;

            Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { projecField }));
            if (!enTarget.Contains(projecField)) return;

            EntityReference refProject = (EntityReference)enTarget[projecField];
            string projectCode = GetProjectCode(refProject);
            EntityCollection rs = GetTeams(projectCode);
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                EntityReference refTarget = enTarget.ToEntityReference();

                EntityReference refTeam = null;
                bool hasWriteShare = false;
                foreach (Entity team in rs.Entities)
                {
                    refTeam = team.ToEntityReference();
                    hasWriteShare = $"{projectCode}_{teamWriteShare}_Team".Equals((string)team["name"]);

                    ShareTeams(refTarget, refTeam, hasWriteShare);
                }
            }
        }
    }
}
