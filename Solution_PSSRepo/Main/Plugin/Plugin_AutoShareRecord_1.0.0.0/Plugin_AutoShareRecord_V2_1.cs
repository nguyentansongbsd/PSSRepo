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
        IOrganizationService service = null;
        ITracingService traceService = null;
        Entity target = null;
        Entity enTarget = null;

        public Plugin_AutoShareRecord_V2_1(IOrganizationService _service, ITracingService _traceService, Entity _target)
        {
            service = _service;
            traceService = _traceService;
            target = _target;
        }

        public void Run_Update(IPluginExecutionContext _context)
        {
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
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 0 }, { "FINANCE-TEAM", 0 }, { "SALE-TEAM", 0 }, { "SALE-MGT", 2 }, { "SALE-ADMIN", 0 } }, 100000000);
                    break;
                case "bsd_updatepricelist":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "SALE-ADMIN", 0 }, { "SALE-MGT", 1 } }, 100000000);
                    break;
                //case "pricelevel":
                //    Run_PriceList();
                //    break;
                case "bsd_discount":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 0 }, { "FINANCE-TEAM", 0 }, { "SALE-TEAM", 0 }, { "SALE-MGT", 2 }, { "SALE-ADMIN", 0 } }, 100000000);
                    break;
            }


        }

        public void Run_Create(IPluginExecutionContext _context)
        {
            traceService.Trace("Plugin_AutoShareRecord_V2_1 Run_Create " + target.LogicalName + " " + target.Id);

            switch (target.LogicalName)
            {
                case "salesorder":
                    ShareTeams_OE(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 }, { "SALE-MGT", 1 }, { "SALE-ADMIN", 1 } });
                    break;
                case "quote":
                case "bsd_followuplist":
                case "opportunity":
                case "bsd_quotation":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 }, { "SALE-MGT", 1 }, { "SALE-ADMIN", 1 } });
                    break;
                case "bsd_advancepayment":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "FINANCE-TEAM", 1 } });
                    break;
                //case "bsd_capnhatphiquanly":
                //    ShareTeams_OneEntity(new Dictionary<string, int> { { "SALE-TEAM", 1 } });
                //    break;
                case "bsd_updateactualarea":
                case "bsd_updateactualareaapprove":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "SALE-MGT", 1 } });
                    break;
                case "bsd_appendixcontract":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 } });
                    break;
                case "bsd_assign":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 0 } });
                    break;
                case "bsd_paymentschemedetail":
                    ShareTeams_Ins(new Dictionary<string, int> { { "CCR-TEAM", 2 }, { "FINANCE-TEAM", 2 }, { "SALE-MGT", 2 }, { "SALE-ADMIN", 2 } });
                    break;
                case "bsd_transfermoney":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 } });
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
                case "bsd_termination":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "CCR-TEAM", 1 }, { "FINANCE-TEAM", 1 }, { "SALE-MGT", 1 }, { "SALE-ADMIN", 1 } });
                    break;
                case "bsd_updateduedate":
                case "bsd_updateduedatedetail":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "FINANCE-TEAM", 1 }, { "SALE-MGT", 1 }, { "SALE-ADMIN", 1 } });
                    break;
                case "bsd_updateduedateoflastinstallmentapprove":
                case "bsd_updateduedateoflastinstallment":
                    ShareTeams_OneEntity(new Dictionary<string, int> { { "FINANCE-TEAM", 1 } });
                    break;
                case "bsd_confirmpayment":
                case "bsd_confirmapplydocument":
                    enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_project" }));
                    if (enTarget.Contains("bsd_project"))
                        ShareTeams_OneEntity(new Dictionary<string, int> { { "FINANCE-TEAM", 2 }, { "SALE-MGT", 0 } });
                    else
                        ShareTeams_ConfirmPayment(_context);
                    break;
            }

        }

        private AccessRights GetAccessRights(int accessType)
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

        public void ShareTeams(EntityReference sharedRecord, EntityReference shareTeams, int accessType)
        {
            try
            {
                traceService.Trace($"ShareTeams {shareTeams.Id}");
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
            catch (Exception ex)
            {
                traceService.Trace("ShareTeams Exception: " + ex.Message);
                return;
            }

        }

        public string GetProjectCode()
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

        public EntityCollection GetTeams(string projectCode)
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
                    <value>{projectCode}-SALE-ADMIN</value>
                  </condition>
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            traceService.Trace("rs.Entities.Count " + rs.Entities.Count);
            return rs;
        }

        private void Run_PhasesLaunch()
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
                        hasWriteShare = $"{projectCode}-SALE-MGT".Equals((string)team["name"]);
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

        private EntityCollection GetDiscounts(EntityReference refDiscountList)
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

        private EntityCollection GetPromotions(EntityReference refPhasesLaunch)
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

        private EntityCollection GetHandoverCondition(EntityReference refPhasesLaunch)
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

        private void Run_PaymentScheme()
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
                        accessType = hasWrite ? 2 : 0;

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

        private EntityCollection GetPaymentSchemeDetails(EntityReference refPS)
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

        public void ShareTeams_OneEntity(Dictionary<string, int> listTeamRights, int status = -999)
        {
            traceService.Trace("ShareTeams_OneEntity");

            if (status != -999 && (!target.Contains("statuscode") || ((OptionSetValue)target["statuscode"]).Value != status))
                return;
            string projectCode = GetProjectCode();
            EntityCollection rs = GetTeams(projectCode);
            traceService.Trace($"GetTeams {rs.Entities.Count}");

            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                EntityReference refTarget = enTarget.ToEntityReference();
                EntityReference refTeam = null;

                Dictionary<string, Entity> allTeams = rs.Entities.ToDictionary(e => (string)e["name"], e => e);
                foreach (var teamRights in listTeamRights)
                {

                    traceService.Trace($"ShareTeams {teamRights.Key} {teamRights.Value}");
                    if (allTeams.TryGetValue($"{projectCode}-{teamRights.Key}", out var tmpTeam))
                    {

                        refTeam = tmpTeam.ToEntityReference();
                        ShareTeams(refTarget, refTeam, teamRights.Value);
                    }
                }
            }
        }

        public void ShareTeams_OE(Dictionary<string, int> listTeamRights)
        {
            traceService.Trace("ShareTeams_OE");

            string projectCode = GetProjectCode();
            EntityCollection rs = GetTeams(projectCode);
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                EntityReference refOE = enTarget.ToEntityReference();
                EntityReference refTeam = null;

                Dictionary<string, Entity> allTeams = rs.Entities.ToDictionary(e => (string)e["name"], e => e);
                foreach (var teamRights in listTeamRights)
                {
                    if (allTeams.TryGetValue($"{projectCode}-{teamRights.Key}", out var tmpTeam))
                    {
                        refTeam = tmpTeam.ToEntityReference();
                        ShareTeams(refOE, refTeam, teamRights.Value);
                    }
                }

                Entity enOE = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "quoteid" }));
                if (enOE.Contains("quoteid"))
                {
                    EntityReference refQuote = (EntityReference)enOE["quoteid"];
                    Entity enQuote = service.Retrieve(refQuote.LogicalName, refQuote.Id, new ColumnSet(new string[] { "ownerid" }));
                    if (!enQuote.Contains("ownerid")) return;

                    EntityReference refOwnerQuote = (EntityReference)enQuote["ownerid"];
                    ShareTeams(refOE, refOwnerQuote, 0);
                }
            }
        }

        private List<string> GetProjectCodes_ConfirmPayment(Guid userId)
        {
            traceService.Trace("GetProjectCodes_ConfirmPayment");

            List<string> listCode = new List<string>();

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch distinct=""true"">
              <entity name=""team"">
                <attribute name=""name"" />
                <attribute name=""isdefault"" />
                <filter>
                  <condition attribute=""isdefault"" operator=""eq"" value=""0"" />
                </filter>
                <order attribute=""name"" />
                <link-entity name=""teammembership"" from=""teamid"" to=""teamid"" intersect=""true"">
                  <filter>
                    <condition attribute=""systemuserid"" operator=""eq"" value=""{userId}"" />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                listCode.AddRange(rs.Entities
                        .Select(x => (string)x["name"])
                        .Select(name => string.Join("-", name.Split('-').Reverse().Skip(2).Reverse()))
                        .Distinct()
                        .ToList());
            }
            return listCode;
        }

        private EntityCollection GetTeam_ConfirmPayment(Guid userId)
        {
            traceService.Trace("GetTeam_ConfirmPayment");

            List<string> listCode = GetProjectCodes_ConfirmPayment(userId);
            EntityCollection listTeam = new EntityCollection();

            if (listCode.Count > 0)
            {
                foreach (var projectCode in listCode)
                {
                    var fetchXmlTeam = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""team"">
                        <attribute name=""name"" />
                        <filter>
                          <condition attribute=""name"" operator=""in"">
                            <value>{projectCode}-FINANCE-TEAM</value>
                            <value>{projectCode}-SALE-MGT</value>
                          </condition>
                        </filter>
                        <order attribute=""name"" />
                      </entity>
                    </fetch>";
                    EntityCollection rsTeam = service.RetrieveMultiple(new FetchExpression(fetchXmlTeam));
                    if (rsTeam != null && rsTeam.Entities != null && rsTeam.Entities.Count > 0)
                    {
                        listTeam.Entities.AddRange(rsTeam.Entities.ToList());
                    }
                }
            }

            return listTeam;
        }

        private bool IsSystemAdmin(Guid userId)
        {
            traceService.Trace("IsSystemAdmin");

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch distinct=""true"">
              <entity name=""systemuser"">
                <attribute name=""systemuserid"" />
                <attribute name=""fullname"" />
                <filter>
                  <condition attribute=""systemuserid"" operator=""eq"" value=""{userId}"" />
                </filter>
                <link-entity name=""systemuserroles"" from=""systemuserid"" to=""systemuserid"" intersect=""true"">
                  <link-entity name=""role"" from=""roleid"" to=""roleid"" alias=""role"" intersect=""true"">
                    <attribute name=""name"" />
                    <filter>
                      <condition attribute=""name"" operator=""eq"" value=""System Administrator"" />
                    </filter>
                  </link-entity>
                </link-entity>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return (rs != null && rs.Entities != null && rs.Entities.Count > 0);
        }

        private EntityCollection GetTeam_ConfirmPayment_All(Guid userId)
        {
            traceService.Trace("GetTeam_ConfirmPayment");

            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch distinct=""true"">
              <entity name=""team"">
                <attribute name=""teamid"" />
                <attribute name=""name"" />
                <attribute name=""isdefault"" />
                <filter type=""or"">
                  <condition attribute=""name"" operator=""ends-with"" value=""-FINANCE-TEAM"" />
                  <condition attribute=""name"" operator=""ends-with"" value=""-SALE-MGT"" />
                </filter>
                <order attribute=""name"" />
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return rs;
        }

        public void ShareTeams_ConfirmPayment(IPluginExecutionContext _context)
        {
            traceService.Trace("ShareTeams_ConfirmPayment");

            EntityCollection rs = new EntityCollection();
            if (IsSystemAdmin(_context.UserId))
                rs = GetTeam_ConfirmPayment_All(_context.UserId);
            else
                rs = GetTeam_ConfirmPayment(_context.UserId);

            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                EntityReference refTarget = enTarget.ToEntityReference();
                int accessType = 0;
                foreach (var team in rs.Entities)
                {
                    accessType = ((string)team["name"]).EndsWith("-FINANCE-TEAM") ? 2 : 0;
                    ShareTeams(refTarget, team.ToEntityReference(), accessType);
                }
            }
        }

        public void ShareTeams_Ins(Dictionary<string, int> listTeamRights)
        {
            traceService.Trace("ShareTeams_Ins");

            string projectCode = GetProjectCode();
            EntityCollection rs = GetTeams(projectCode);
            if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
            {
                EntityReference refIns = enTarget.ToEntityReference();
                EntityReference refTeam = null;

                Dictionary<string, Entity> allTeams = rs.Entities.ToDictionary(e => (string)e["name"], e => e);
                foreach (var teamRights in listTeamRights)
                {
                    if (allTeams.TryGetValue($"{projectCode}-{teamRights.Key}", out var tmpTeam))
                    {
                        refTeam = tmpTeam.ToEntityReference();
                        ShareTeams(refIns, refTeam, teamRights.Value);
                    }
                }

                Entity enIns = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_reservation" }));
                if (enIns.Contains("bsd_reservation"))
                {
                    EntityReference refQuote = (EntityReference)enIns["bsd_reservation"];
                    Entity enQuote = service.Retrieve(refQuote.LogicalName, refQuote.Id, new ColumnSet(new string[] { "ownerid" }));
                    if (!enQuote.Contains("ownerid")) return;

                    EntityReference refOwnerQuote = (EntityReference)enQuote["ownerid"];
                    ShareTeams(refIns, refOwnerQuote, 2);
                }
            }
        }

        //private void Run_PriceList()
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

        //private EntityCollection GetPriceListItems(EntityReference refPriceList)
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