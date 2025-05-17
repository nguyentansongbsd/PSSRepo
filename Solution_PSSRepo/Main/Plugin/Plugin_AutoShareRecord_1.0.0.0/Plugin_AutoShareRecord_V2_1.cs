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
        static Entity enTarget = null;

        public static void Run_Update(IOrganizationService _service, ITracingService _traceService, Entity _target, IPluginExecutionContext _context)
        {
            service = _service;
            traceService = _traceService;
            target = _target;
            traceService.Trace("Plugin_AutoShareRecord_V2_1 Run_Update " + target.LogicalName + " " + target.Id);

            switch (target.LogicalName)
            {
                case "bsd_phaseslaunch":
                    Run_PhasesLaunch();
                    break;
                case "bsd_paymentscheme":
                    Run_PaymentScheme();
                    break;
                case "bsd_event":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 0 }, { "FINANCE-TEAM", 0 }, { "SALE-TEAM", 2 }, { "SALE-MGT", 0 } }, 100000000);
                    break;
                case "bsd_updatepricelist":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 0 }, { "FINANCE-TEAM", 0 }, { "SALE-TEAM", 1 }, { "SALE-MGT", 0 } }, 100000000);
                    break;
                    //case "pricelevel":
                    //    Run_PriceList();
                    //    break;
            }


        }

        public static void Run_Create(IOrganizationService _service, ITracingService _traceService, Entity _target, IPluginExecutionContext _context)
        {
            service = _service;
            traceService = _traceService;
            target = _target;
            traceService.Trace("Plugin_AutoShareRecord_V2_1 Run_Create " + target.LogicalName + " " + target.Id);

            switch (target.LogicalName)
            {
                case "bsd_followuplist":
                case "salesorder":
                case "quote":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 }, { "SALE-TEAM", 1 }, { "SALE-MGT", 0 } });
                    break;
                case "opportunity":
                case "bsd_quotation":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 0 }, { "FINANCE-TEAM", 0 }, { "SALE-TEAM", 1 }, { "SALE-MGT", 0 } });
                    break;
                case "bsd_updateactualarea":
                case "bsd_capnhatphiquanly":
                case "bsd_updateactualareaapprove":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "SALE-TEAM", 1 } });
                    break;
                case "bsd_termination":
                case "bsd_appendixcontract":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 } });
                    break;
                case "bsd_assign":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 0 } });
                    break;
                case "bsd_paymentschemedetail":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "FINANCE-TEAM", 1 } });
                    break;
                case "bsd_advancepayment":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 }, { "SALE-TEAM", 0 }, { "SALE-MGT", 0 } });
                    break;
                case "bsd_transfermoney":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 }, { "SALE-MGT", 0 } });
                    break;
                case "bsd_terminateletter":
                case "bsd_customernotices":
                case "bsd_handovernotice":
                case "bsd_warningnotices":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "FINANCE-TEAM", 2 } });
                    break;
                case "bsd_applydocument":
                case "bsd_refund":
                case "bsd_voidpayment":
                case "bsd_payment":
                case "bsd_miscellaneous":
                case "bsd_invoice":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "FINANCE-TEAM", 2 }, { "SALE-MGT", 0 } });
                    break;
                case "bsd_updateduedate":
                case "bsd_updateduedatedetail":
                case "bsd_updateduedateoflastinstallmentapprove":
                case "bsd_updateduedateoflastinstallment":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "FINANCE-TEAM", 1 }, { "SALE-TEAM", 1 }, { "SALE-MGT", 0 } });
                    break;
            }

        }

        private static AccessRights GetAccessRights(int accessType)
        {
            AccessRights Access_Rights = AccessRights.ReadAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess;
            switch (accessType)
            {
                case 1:
                    Access_Rights |= AccessRights.WriteAccess;
                    break;
                case 2:
                    Access_Rights |= AccessRights.WriteAccess | AccessRights.ShareAccess;
                    break;
            }

            return Access_Rights;
        }

        public static void ShareTeams(EntityReference sharedRecord, EntityReference shareTeams, int accessType)
        {
            traceService.Trace($"ShareTeams {shareTeams.Name} {shareTeams.Id}");

            var grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = GetAccessRights(accessType),
                    Principal = shareTeams
                },
                Target = sharedRecord
            };
            service.Execute(grantAccessRequest);
        }

        public static string GetProjectCode()
        {
            traceService.Trace("GetProjectCode");
            string projectCode = string.Empty;

            EntityReference refProject = null;
            switch (target.LogicalName)
            {
                case "bsd_phaseslaunch":
                    enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                    if (!enTarget.Contains("bsd_projectid")) return projectCode;
                    refProject = (EntityReference)enTarget["bsd_projectid"];
                    break;
                case "quote":
                    enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_projectid" }));
                    if (!enTarget.Contains("bsd_projectid")) return projectCode;
                    refProject = (EntityReference)enTarget["bsd_projectid"];
                    break;
                case "bsd_updateactualarea":
                    enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_units" }));
                    if (!enTarget.Contains("bsd_units")) return projectCode;

                    EntityReference refUnit = (EntityReference)enTarget["bsd_units"];
                    Entity enUnit = service.Retrieve(refUnit.LogicalName, refUnit.Id, new ColumnSet(new string[] { "bsd_projectcode" }));
                    if (!enUnit.Contains("bsd_projectcode")) return projectCode;

                    refProject = (EntityReference)enUnit["bsd_projectcode"];
                    break;
                case "bsd_capnhatphiquanly":
                    enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_optionentry" }));
                    if (!enTarget.Contains("bsd_optionentry")) return projectCode;

                    EntityReference refOE = (EntityReference)enTarget["bsd_optionentry"];
                    Entity enOE = service.Retrieve(refOE.LogicalName, refOE.Id, new ColumnSet(new string[] { "bsd_project" }));
                    if (!enOE.Contains("bsd_project")) return projectCode;

                    refProject = (EntityReference)enOE["bsd_project"];
                    break;
                case "bsd_termination":
                    enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_followuplist" }));
                    if (!enTarget.Contains("bsd_followuplist")) return projectCode;

                    EntityReference refFUL = (EntityReference)enTarget["bsd_followuplist"];
                    Entity enFUL = service.Retrieve(refFUL.LogicalName, refFUL.Id, new ColumnSet(new string[] { "bsd_project" }));
                    if (!enFUL.Contains("bsd_project")) return projectCode;

                    refProject = (EntityReference)enFUL["bsd_project"];
                    break;
                case "bsd_paymentschemedetail":
                    enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_paymentscheme" }));
                    if (!enTarget.Contains("bsd_paymentscheme")) return projectCode;

                    EntityReference refPS = (EntityReference)enTarget["bsd_paymentscheme"];
                    Entity enPS = service.Retrieve(refPS.LogicalName, refPS.Id, new ColumnSet(new string[] { "bsd_project" }));
                    if (!enPS.Contains("bsd_project")) return projectCode;

                    refProject = (EntityReference)enPS["bsd_project"];
                    break;
                default:
                    enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_project" }));
                    if (!enTarget.Contains("bsd_project")) return projectCode;
                    refProject = (EntityReference)enTarget["bsd_project"];
                    break;
            }

            if (refProject == null)
                return projectCode;

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
                projectCode = (string)rs.Entities[0]["bsd_projectcode"];
            }
            return projectCode;
        }

        public static EntityCollection GetTeams(string projectCode)
        {
            traceService.Trace("GetTeam " + projectCode);
            if (string.IsNullOrEmpty(projectCode))
                return null;

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

        private static void Run_PhasesLaunch()
        {
            traceService.Trace("Run_PhasesLaunch");

            if (target.Contains("statuscode") && ((OptionSetValue)target["statuscode"]).Value == 100000000) //Launched
            {
                string projectCode = GetProjectCode();
                EntityCollection rs = GetTeams(projectCode);
                if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
                {
                    EntityReference refPhasesLaunch = enTarget.ToEntityReference();
                    EntityCollection rsPromotions = GetPromotions(refPhasesLaunch);
                    EntityCollection rsHandovers = GetHandoverCondition(refPhasesLaunch);
                    EntityReference refDiscountList = enTarget.Contains("bsd_discountlist") ? (EntityReference)enTarget["bsd_discountlist"] : null;
                    EntityCollection rsDiscounts = null;
                    if (refDiscountList != null)
                    {
                        rsDiscounts = GetDiscounts(refDiscountList);
                    }

                    EntityReference refTeam = null;
                    bool hasWriteShare = false;
                    int accessType = 0;
                    foreach (Entity team in rs.Entities)
                    {
                        refTeam = team.ToEntityReference();
                        hasWriteShare = $"{projectCode}-SALE-TEAM".Equals((string)team["name"]);
                        accessType = hasWriteShare ? 2 : 0;

                        ShareTeams(refPhasesLaunch, refTeam, accessType);

                        if (refDiscountList != null)
                        {
                            traceService.Trace("Share DiscountList");

                            ShareTeams(refDiscountList, refTeam, accessType);
                            if (rsDiscounts != null && rsDiscounts.Entities != null && rsDiscounts.Entities.Count > 0)
                            {
                                foreach (var discount in rsDiscounts.Entities)
                                {
                                    ShareTeams(discount.ToEntityReference(), refTeam, accessType);
                                }
                            }
                        }

                        if (rsPromotions != null && rsPromotions.Entities != null && rsPromotions.Entities.Count > 0)
                        {
                            foreach (var promotion in rsPromotions.Entities)
                            {
                                ShareTeams(promotion.ToEntityReference(), refTeam, accessType);
                            }
                        }

                        if (rsHandovers != null && rsHandovers.Entities != null && rsHandovers.Entities.Count > 0)
                        {
                            foreach (var handover in rsHandovers.Entities)
                            {
                                ShareTeams(handover.ToEntityReference(), refTeam, accessType);
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

        private static EntityCollection GetHandoverCondition(EntityReference refPhasesLaunch)
        {
            traceService.Trace("GetHandoverCondition");
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_packageselling"">
                <attribute name=""bsd_name"" />
                <link-entity name=""bsd_bsd_phaseslaunch_bsd_packageselling"" from=""bsd_packagesellingid"" to=""bsd_packagesellingid"" intersect=""true"">
                  <filter>
                    <condition attribute=""bsd_phaseslaunchid"" operator=""eq"" value=""{refPhasesLaunch.Id}"" />
                  </filter>
                </link-entity>
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
                string projectCode = GetProjectCode();
                EntityCollection rs = GetTeams(projectCode);
                if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
                {
                    EntityReference refPS = enTarget.ToEntityReference();
                    EntityCollection rsPSDetails = GetPaymentSchemeDetails(refPS);

                    EntityReference refTeam = null;
                    bool hasWrite = false;
                    int accessType = 0;
                    foreach (Entity team in rs.Entities)
                    {
                        refTeam = team.ToEntityReference();
                        hasWrite = $"{projectCode}-CCR-TEAM".Equals((string)team["name"]);
                        accessType = hasWrite ? 1 : 0;

                        ShareTeams(refPS, refTeam, accessType);

                        if (rsPSDetails != null && rsPSDetails.Entities != null && rsPSDetails.Entities.Count > 0)
                        {
                            foreach (var ins in rsPSDetails.Entities)
                            {
                                ShareTeams(ins.ToEntityReference(), refTeam, accessType);
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

        public static void ShareTeams_OneEntity(Dictionary<string, int> listTeamRights, int status = -999)
        {
            traceService.Trace("ShareTeams_OneEntity");

            if (status != -999 && (!target.Contains("statuscode") || ((OptionSetValue)target["statuscode"]).Value != status))
                return;

            string projectCode = GetProjectCode();
            EntityCollection rs = GetTeams(projectCode);
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                EntityReference refTarget = enTarget.ToEntityReference();
                EntityReference refTeam = null;

                Dictionary<string, Entity> allTeams = rs.Entities.ToDictionary(e => (string)e["name"], e => e);
                foreach (var teamRights in listTeamRights)
                {
                    if (allTeams.TryGetValue($"{projectCode}-{teamRights.Key}", out var tmpTeam))
                    {
                        refTeam = tmpTeam.ToEntityReference();
                        ShareTeams(refTarget, refTeam, teamRights.Value);
                    }
                }
            }
        }

        //private static void Run_PriceList()
        //{
        //    traceService.Trace("Run_PriceList");

        //    if (target.Contains("bsd_approved") && (bool)target["bsd_approved"]) //Approved
        //    {
        //        Entity enPriceList = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_project" }));
        //        if (!enPriceList.Contains("bsd_project")) return;

        //        EntityReference refProject = (EntityReference)enPriceList["bsd_project"];
        //        string projectCode = GetProjectCode(refProject);
        //        EntityCollection rs = GetTeams(projectCode);
        //        if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
        //        {
        //            EntityReference refPS = enPriceList.ToEntityReference();
        //            EntityCollection refPLItems = GetPriceListItems(refPS);

        //            if (refPLItems != null && refPLItems.Entities != null && refPLItems.Entities.Count > 0)
        //            {
        //                EntityReference refTeam = null;
        //                bool hasWrite = false;
        //                int accessType = 0;
        //                foreach (Entity team in rs.Entities)
        //                {
        //                    refTeam = team.ToEntityReference();
        //                    hasWrite = $"{projectCode}_Sales_Team".Equals((string)team["name"]);
        //                    accessType = hasWrite ? 1 : 0;

        //                    foreach (var ins in refPLItems.Entities)
        //                    {
        //                        ShareTeams(ins.ToEntityReference(), refTeam, accessType);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private static EntityCollection GetPriceListItems(EntityReference refPriceList)
        //{
        //    traceService.Trace("GetPriceListItems");
        //    var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
        //    <fetch>
        //      <entity name=""productpricelevel"">
        //        <attribute name=""productid"" />
        //        <filter>
        //          <condition attribute=""pricelevelid"" operator=""eq"" value=""{refPriceList.Id}"" />
        //        </filter>
        //      </entity>
        //    </fetch>";
        //    EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
        //    return rs;
        //}
    }
}