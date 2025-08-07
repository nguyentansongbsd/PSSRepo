using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Action_ShareCustomerToTeam
{
    public class Action_ShareCustomerToTeam : IPlugin
    {
        IOrganizationService service = null;
        IPluginExecutionContext context = null;
        ITracingService traceService = null;
        int type = -1;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Guid AdminID = new Guid("{d90ce220-655a-e811-812e-3863bb36dc00}");//CRM ADMIN
            Guid CurrentUser = context.UserId;
            service = factory.CreateOrganizationService(AdminID);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");
            #region sharecustomer to team when approve ShareCustomer
            if (context.InputParameters.Contains("sharecusid") && context.InputParameters["sharecusid"] != null)
            {
                traceService.Trace("ShareCustomerRecordToProjectTeams");
                ShareCustomerRecordToProjectTeams(new Guid(context.InputParameters["sharecusid"].ToString()));
                return;
            }
            #endregion
            type = (int)context.InputParameters["type"];
            string id = context.InputParameters["id"].ToString();
            id = id.TrimEnd(',');
            string[] arrID = id.Split(',');
            #region tạo record sharecustomer và sharecustomerproject 
            if (context.InputParameters.Contains("CreateShareCustomer")&& context.InputParameters["CreateShareCustomer"]!=null)
            {
                traceService.Trace("CreateShareCustomer");
                traceService.Trace(context.InputParameters["CreateShareCustomer"].ToString());
                var arrTeamID = context.InputParameters["idTeam"].ToString().TrimEnd(',').Split(',');
                CreateShareCustomer(arrID, arrTeamID);
            }
            #endregion
           
            // lưu về contact ở tabzalo chat entity lead và entity project
            if (context.InputParameters["idform"] != null)
            {
                string idform = context.InputParameters["idform"].ToString();
                Entity lead = service.Retrieve("lead", Guid.Parse(idform), new ColumnSet(true));
                string zalooa_id = lead.Contains("bsd_zalooaid") ? (string)lead["bsd_zalooaid"] : "";
                Entity uplead = new Entity(lead.LogicalName, lead.Id);
                uplead["statuscode"] = new OptionSetValue(2);
                service.Update(uplead);
                //throw new InvalidPluginExecutionException("thinh"+ lead.LogicalName);

                if (lead.LogicalName == "lead")
                {
                    if (!lead.Contains("bsd_projectcode")) throw new InvalidPluginExecutionException("Khách hàng chưa quan tâm dự án.");
                    string checkzaloid = lead.Contains("bsd_zaloid") ? (string)lead["bsd_zaloid"] : "";
                    var fetchXmlcheck = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                        <fetch>
                          <entity name=""bsd_zaloinfor"">
                            <attribute name=""bsd_customer"" />
                            <filter>
                              <condition attribute=""bsd_zalocustomerid"" operator=""eq"" value=""{checkzaloid}"" />
                            </filter>
                          </entity>
                        </fetch>";
                    EntityCollection rscheck = service.RetrieveMultiple(new FetchExpression(fetchXmlcheck));
                    if (rscheck.Entities.Count > 0)
                    {
                        return;
                    }
                    else
                    {
                        Entity zaloinfor = new Entity("bsd_zaloinfor");
                        zaloinfor["bsd_customer"] = new EntityReference("contact", Guid.Parse(id));

                        var fetchXmlproject = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                        <fetch>
                          <entity name=""bsd_project"">
                            <attribute name=""bsd_projectid"" />
                            <attribute name=""bsd_zalooaname"" />
                            <filter>
                              <condition attribute=""bsd_projectcode"" operator=""eq"" value=""{lead["bsd_projectcode"]}"" />
                                <condition attribute=""statecode"" operator=""ne"" value=""{1}"" />
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection rsproject = service.RetrieveMultiple(new FetchExpression(fetchXmlproject));
                        if (rsproject.Entities.Count > 0)
                        {
                            Guid projectId = rsproject.Entities[0].GetAttributeValue<Guid>("bsd_projectid");
                            zaloinfor["bsd_project"] = new EntityReference("bsd_project", projectId);
                        }
                        zaloinfor["bsd_name"] = zaloinfor["bsd_zalooaname"] = lead.Contains("bsd_zalooaname") ? lead["bsd_zalooaname"] : "";
                        zaloinfor["bsd_zalocustomername"] = lead.Contains("bsd_tenzalo") ? lead["bsd_tenzalo"] : "";
                        zaloinfor["bsd_zalooaid"] = lead.Contains("bsd_zalooaid") ? lead["bsd_zalooaid"] : "";
                        zaloinfor["bsd_zalocustomerid"] = lead.Contains("bsd_zaloid") ? lead["bsd_zaloid"] : "";
                        service.Create(zaloinfor);
                    }

                }

            }
            //lưu về team nghiệp vụ ở tìm kiếm khách hàng

            //throw new InvalidPluginExecutionException(id);

            string fieldName = ""; //type == 0 ? "contact" : "account";
            traceService.Trace("step1");
            if (type == 0)//KHCN
                fieldName = "contact";
            else if (type == 1)
                fieldName = "account";
            else if (type == 10)
            {
                fieldName = "lead";
            }
            else
            {
                traceService.Trace("step@@");
                fieldName = type == 2 ? "contact" : (type == 3 ? "account" : "lead");
                ShareCustomerToTeam(arrID, fieldName);
            }
            traceService.Trace(fieldName);
            foreach (string item in arrID)
            {
                EntityReference enRef = new EntityReference(fieldName, Guid.Parse(item));
                EntityCollection rs = GetListTeamOfCurrentUser(CurrentUser);
                traceService.Trace("step2");
                traceService.Trace("rs.Entities.Count " + rs.Entities.Count);
                if (rs.Entities.Count == 0) throw new InvalidPluginExecutionException("Người dùng hiện tại chưa tham gia vào bất kỳ team nào !");
                if (rs.Entities.Count == 1)
                {
                    foreach (Entity it in rs.Entities)
                    {
                        Guid TeamID = Guid.Parse(it["teamid"].ToString());
                        EntityReference refTeam = new EntityReference("team", TeamID);
                        traceService.Trace("step2.0");
                        ShareTeams(enRef, refTeam);
                        traceService.Trace("step2.1_");
                        if (fieldName == "contact")
                            ShareDoiTuong_CoOwner(enRef, refTeam);
                        else
                        {
                            traceService.Trace("step2.1");
                            Entity enAcc = service.Retrieve(fieldName, enRef.Id, new ColumnSet(true));
                            if (enAcc.Contains("primarycontactid"))
                                ShareTeams((EntityReference)enAcc["primarycontactid"], refTeam);
                            //if(enAcc.Contains("bsd_chudautu"))
                            //    ShareTeams((EntityReference)enAcc["bsd_chudautu"], refTeam);
                            if (enAcc.Contains("bsd_maincompany"))
                                ShareTeams((EntityReference)enAcc["bsd_maincompany"], refTeam);
                            traceService.Trace("step2.2");

                            var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_mandatorysecondary'>
                                <attribute name='bsd_contact' />
                                <filter>
                                  <condition attribute='bsd_developeraccount' operator='eq' value='{enAcc.Id}'/>
                                  <condition attribute='statecode' operator='eq' value='0'/>
                                </filter>
                              </entity>
                            </fetch>";
                            traceService.Trace("step2.3");

                            EntityCollection rs_ = service.RetrieveMultiple(new FetchExpression(fetchXml));
                            foreach (Entity i in rs_.Entities)
                            {
                                if (i.Contains("bsd_representative"))
                                    ShareTeams((EntityReference)i["bsd_representative"], refTeam);
                                traceService.Trace("step2.4");

                                if (i.Contains("bsd_contact"))
                                    ShareTeams((EntityReference)i["bsd_contact"], refTeam);
                                traceService.Trace("step2.5");

                            }
                        }
                        var team = it;
                        Entity enTeam = service.Retrieve("team", Guid.Parse(it["teamid"].ToString()), new ColumnSet(new string[] { "name", "teamid" }));
                        var fetchXml2 = $@"
                                        <fetch>
                                          <entity name='team'>
                                            <filter>
                                              <condition attribute='name' operator='like' value='%{enTeam["name"].ToString().Split('-')[0]}%'/>
                                            </filter>
                                          </entity>
                                        </fetch>";
                        traceService.Trace("step2.6");

                        EntityCollection rs1 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in rs1.Entities)
                        {
                            traceService.Trace("team" + entity["name"].ToString());
                            TeamID = entity.Id;
                            refTeam = new EntityReference("team", TeamID);
                            ShareTeams(enRef, refTeam);
                            if (fieldName == "contact")
                                ShareDoiTuong_CoOwner(enRef, refTeam);
                            else
                            {
                                Entity enAcc = service.Retrieve(fieldName, enRef.Id, new ColumnSet(true));
                                if (enAcc.Contains("primarycontactid"))
                                    ShareTeams((EntityReference)enAcc["primarycontactid"], refTeam);
                                traceService.Trace("step2.7");

                                //if(enAcc.Contains("bsd_chudautu"))
                                //    ShareTeams((EntityReference)enAcc["bsd_chudautu"], refTeam);
                                if (enAcc.Contains("bsd_maincompany"))
                                    ShareTeams((EntityReference)enAcc["bsd_maincompany"], refTeam);

                                var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_mandatorysecondary'>
                                <attribute name='bsd_contact' />
                                <filter>
                                  <condition attribute='bsd_developeraccount' operator='eq' value='{enAcc.Id}'/>
                                  <condition attribute='statecode' operator='eq' value='0'/>
                                </filter>
                              </entity>
                            </fetch>";
                                traceService.Trace("step2.8");

                                EntityCollection rs_ = service.RetrieveMultiple(new FetchExpression(fetchXml));
                                foreach (Entity i in rs_.Entities)
                                {
                                    if (i.Contains("bsd_representative"))
                                        ShareTeams((EntityReference)i["bsd_representative"], refTeam);
                                    if (i.Contains("bsd_contact"))
                                        ShareTeams((EntityReference)i["bsd_contact"], refTeam);
                                }
                            }
                        }
                    }
                }
                else
                {
                    traceService.Trace("step3");
                    List<TeamReturn> list = new List<TeamReturn>();
                    foreach (Entity it in rs.Entities)
                    {
                        Entity enTeam = service.Retrieve("team", Guid.Parse(it["teamid"].ToString()), new ColumnSet(new string[] { "name", "teamid" }));
                        //list.Entities.Add(enTeam);
                        TeamReturn i = new TeamReturn();
                        i.TeamID = enTeam.Id.ToString();
                        i.TeamName = enTeam["name"].ToString().Split('-').ToList()[0];
                        if (i.TeamName == "pssvn") continue;
                        if (list.Any(x => x.TeamName == i.TeamName) == false)
                            list.Add(i);
                    }
                    traceService.Trace("step4 list.Count:"+ list.Count);
                    if (list.Count == 1)
                    {
                        var it = list.FirstOrDefault();
                        #region share cho các team project
                        Guid TeamID = Guid.Parse(it.TeamID.ToString());
                        EntityReference refTeam = new EntityReference("team", TeamID);
                        traceService.Trace("step2.0");
                        ShareTeams(enRef, refTeam);
                        traceService.Trace("step2.1_");
                        if (fieldName == "contact")
                            ShareDoiTuong_CoOwner(enRef, refTeam);
                        else
                        {
                            traceService.Trace("step2.1");
                            Entity enAcc = service.Retrieve(fieldName, enRef.Id, new ColumnSet(true));
                            if (enAcc.Contains("primarycontactid"))
                                ShareTeams((EntityReference)enAcc["primarycontactid"], refTeam);
                            //if(enAcc.Contains("bsd_chudautu"))
                            //    ShareTeams((EntityReference)enAcc["bsd_chudautu"], refTeam);
                            if (enAcc.Contains("bsd_maincompany"))
                                ShareTeams((EntityReference)enAcc["bsd_maincompany"], refTeam);
                            traceService.Trace("step2.2");

                            var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_mandatorysecondary'>
                                <attribute name='bsd_contact' />
                                <filter>
                                  <condition attribute='bsd_developeraccount' operator='eq' value='{enAcc.Id}'/>
                                  <condition attribute='statecode' operator='eq' value='0'/>
                                </filter>
                              </entity>
                            </fetch>";
                            traceService.Trace("step2.3");

                            EntityCollection rs_ = service.RetrieveMultiple(new FetchExpression(fetchXml));
                            foreach (Entity i in rs_.Entities)
                            {
                                if (i.Contains("bsd_representative"))
                                    ShareTeams((EntityReference)i["bsd_representative"], refTeam);
                                traceService.Trace("step2.4");

                                if (i.Contains("bsd_contact"))
                                    ShareTeams((EntityReference)i["bsd_contact"], refTeam);
                                traceService.Trace("step2.5");

                            }
                        }
                        var team = it;
                        Entity enTeam = service.Retrieve("team", Guid.Parse(it.TeamID.ToString()), new ColumnSet(new string[] { "name", "teamid" }));
                        string projectCode = enTeam["name"].ToString().Split('-')[0];
                        var fetchXml2 = $@"
                                        <fetch>
                                          <entity name='team'>
                                            <filter>
                                               <condition attribute='name' operator='in'>
                                                    <value>{projectCode}-CCR-TEAM</value>
                                                    <value>{projectCode}-FINANCE-TEAM</value>
                                                    <value>{projectCode}-SALE-MGT</value>
                                                    <value>{projectCode}-SALE-ADMIN</value>
                                               </condition>
                                            </filter>
                                          </entity>
                                        </fetch>";
                        traceService.Trace("step2.6");

                        EntityCollection rs1 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in rs1.Entities)
                        {
                            traceService.Trace("team" + entity["name"].ToString());
                            TeamID = entity.Id;
                            refTeam = new EntityReference("team", TeamID);
                            ShareTeams(enRef, refTeam);
                            if (fieldName == "contact")
                                ShareDoiTuong_CoOwner(enRef, refTeam);
                            else
                            {
                                Entity enAcc = service.Retrieve(fieldName, enRef.Id, new ColumnSet(true));
                                if (enAcc.Contains("primarycontactid"))
                                    ShareTeams((EntityReference)enAcc["primarycontactid"], refTeam);
                                traceService.Trace("step2.7");

                                //if(enAcc.Contains("bsd_chudautu"))
                                //    ShareTeams((EntityReference)enAcc["bsd_chudautu"], refTeam);
                                if (enAcc.Contains("bsd_maincompany"))
                                    ShareTeams((EntityReference)enAcc["bsd_maincompany"], refTeam);

                                var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_mandatorysecondary'>
                                <attribute name='bsd_contact' />
                                <filter>
                                  <condition attribute='bsd_developeraccount' operator='eq' value='{enAcc.Id}'/>
                                  <condition attribute='statecode' operator='eq' value='0'/>
                                </filter>
                              </entity>
                            </fetch>";
                                traceService.Trace("step2.8");

                                EntityCollection rs_ = service.RetrieveMultiple(new FetchExpression(fetchXml));
                                foreach (Entity i in rs_.Entities)
                                {
                                    if (i.Contains("bsd_representative"))
                                        ShareTeams((EntityReference)i["bsd_representative"], refTeam);
                                    if (i.Contains("bsd_contact"))
                                        ShareTeams((EntityReference)i["bsd_contact"], refTeam);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {

                        var serializer = new JavaScriptSerializer();
                        context.OutputParameters["entityColl"] = serializer.Serialize(list);
                    }
                }
            }
        }
        private void ShareCustomerToTeam(string[] arrCus, string fieldName)
        {
            string id = context.InputParameters["idTeam"].ToString();
            id = id.TrimEnd(',');
            string[] arrTeam = id.Split(',');
            //throw new InvalidPluginExecutionException("CusID: " + arrCus.Length + Environment.NewLine + "TeamID: " + id);

            foreach (string item in arrCus)
            {
                EntityReference enRef = new EntityReference(fieldName, Guid.Parse(item));
                traceService.Trace("@@@@@@@");

                foreach (string t in arrTeam)
                {
                    traceService.Trace("team12313");

                    Guid TeamID = Guid.Parse(t);
                    EntityReference refTeam = new EntityReference("team", TeamID);
                    //ShareTeams(enRef, refTeam);
                    //if (fieldName == "contact")
                    //    ShareDoiTuong_CoOwner(enRef, refTeam);
                    //else
                    //{
                    //    Entity enAcc = service.Retrieve(fieldName, enRef.Id, new ColumnSet(true));
                    //    if (enAcc.Contains("primarycontactid"))
                    //        ShareTeams((EntityReference)enAcc["primarycontactid"], refTeam);
                    //    //if(enAcc.Contains("bsd_chudautu"))
                    //    //    ShareTeams((EntityReference)enAcc["bsd_chudautu"], refTeam);
                    //    if (enAcc.Contains("bsd_maincompany"))
                    //        ShareTeams((EntityReference)enAcc["bsd_maincompany"], refTeam);

                    //    var fetchXml = $@"
                    //        <fetch>
                    //          <entity name='bsd_mandatorysecondary'>
                    //            <attribute name='bsd_contact' />
                    //            <filter>
                    //              <condition attribute='bsd_developeraccount' operator='eq' value='{enAcc.Id}'/>
                    //              <condition attribute='statecode' operator='eq' value='0'/>
                    //            </filter>
                    //          </entity>
                    //        </fetch>";
                    //    EntityCollection rs1 = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    //    foreach (Entity i in rs1.Entities)
                    //    {
                    //        if (i.Contains("bsd_contact"))
                    //            ShareTeams((EntityReference)i["bsd_contact"], refTeam);
                    //    }
                    //}
                    traceService.Trace("@@@@@1");
                    var team = service.Retrieve("team", new Guid(t), new ColumnSet(true));
                    string projectCode = team["name"].ToString().Split('-')[0];
                    var fetchXml2 = $@"
                <fetch>
                  <entity name='team'>
                    <filter>
                      <condition attribute='name' operator='in'>
                        <value>{projectCode}-CCR-TEAM</value>
                        <value>{projectCode}-FINANCE-TEAM</value>
                        <value>{projectCode}-SALE-MGT</value>
                        <value>{projectCode}-SALE-ADMIN</value>
                      </condition>
                    </filter>
                  </entity>
                </fetch>";
                    EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    foreach (Entity entity in rs.Entities)
                    {
                        TeamID = entity.Id;
                        refTeam = new EntityReference("team", TeamID);
                        traceService.Trace("team" + entity["name"].ToString());

                        ShareTeams(enRef, refTeam);
                        if (fieldName == "contact")
                            ShareDoiTuong_CoOwner(enRef, refTeam);
                        else
                        {
                            Entity enAcc = service.Retrieve(fieldName, enRef.Id, new ColumnSet(true));
                            if (enAcc.Contains("primarycontactid"))
                                ShareTeams((EntityReference)enAcc["primarycontactid"], refTeam);
                            //if(enAcc.Contains("bsd_chudautu"))
                            //    ShareTeams((EntityReference)enAcc["bsd_chudautu"], refTeam);
                            if (enAcc.Contains("bsd_maincompany"))
                                ShareTeams((EntityReference)enAcc["bsd_maincompany"], refTeam);

                            var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_mandatorysecondary'>
                                <attribute name='bsd_contact' />
                                <filter>
                                  <condition attribute='bsd_developeraccount' operator='eq' value='{enAcc.Id}'/>
                                  <condition attribute='statecode' operator='eq' value='0'/>
                                </filter>
                              </entity>
                            </fetch>";
                            EntityCollection rs1 = service.RetrieveMultiple(new FetchExpression(fetchXml));
                            foreach (Entity i in rs1.Entities)
                            {
                                if (i.Contains("bsd_representative"))
                                    ShareTeams((EntityReference)i["bsd_representative"], refTeam);
                                if (i.Contains("bsd_contact"))
                                    ShareTeams((EntityReference)i["bsd_contact"], refTeam);
                            }
                        }
                    }
                }
            }
            traceService.Trace("done");

        }
        private void ShareDoiTuong_CoOwner(EntityReference enContact, EntityReference enTeam)
        {
            //var fetchXml = $@"
            //    <fetch>
            //      <entity name='bsd_coowner'>
            //        <attribute name='bsd_name' />
            //        <attribute name='bsd_relatives' />
            //        <filter>
            //          <condition attribute='bsd_checkcoowner' operator='eq' value='0'/>
            //          <condition attribute='bsd_customer' operator='eq' value='{enContact.Id}'/>
            //        </filter>
            //      </entity>
            //    </fetch>";
            //EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            //foreach (Entity item in rs.Entities)
            //{
            //    if (item.Contains("bsd_relatives"))
            //        ShareTeams((EntityReference)item["bsd_relatives"], enTeam);
            //}
        }
        private void ShareTeams(EntityReference sharedRecord, EntityReference shareTeams)
        {
            try
            {

                AccessRights Access_Rights = new AccessRights();
                Access_Rights = AccessRights.ReadAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.WriteAccess | AccessRights.ShareAccess;
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
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message + " Vui Lòng thử lại", ex);
            }
        }
        private EntityCollection GetListTeamOfCurrentUser(Guid CurrentUser)
        {
            var fetchXml = $@"
                <fetch>
                  <entity name='teammembership'>
                    <attribute name='teamid' />
                    <filter>
                      <condition attribute='systemuserid' operator='eq' value='{CurrentUser}'/>
                      <condition attribute='teamid' operator='neq' value='e653d77d-6f7e-e911-a83b-000d3a07fbb4'/>
                    </filter>
                  </entity>
                </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return rs;
        }
        private void CreateShareCustomer(string[] arrID, string[] arrTeamID)
        {
            traceService.Trace("Bắt đầu CreateShareCustomer - Bước 1: Lấy danh sách team");
            // 1. Fetch all teams in one go
            var teamIds = arrTeamID.Select(id => $"'{id}'").ToArray();
            var fetchXmlTeams = $@"
                <fetch>
                  <entity name='team'>
                    <attribute name='teamid' />
                    <attribute name='name' />
                    <filter>
                      <condition attribute='teamid' operator='in'>
                        {string.Join("", teamIds.Select(id => $"<value>{id.Trim('\'')}</value>"))}
                      </condition>
                    </filter>
                  </entity>
                </fetch>";
            var teams = service.RetrieveMultiple(new FetchExpression(fetchXmlTeams)).Entities;
            traceService.Trace($"Đã lấy {teams.Count} team");

            // 2. Extract unique project codes from team names
            traceService.Trace("Bước 2: Lấy danh sách mã dự án từ tên team");
            var projectCodes = teams
                .Select(t => t.Contains("name") ? t["name"].ToString().Split('-')[0] : null)
                .Where(code => !string.IsNullOrEmpty(code))
                .Distinct()
                .ToList();
            traceService.Trace($"Đã lấy {projectCodes.Count} mã dự án");

            // 3. Fetch all projects by project codes in one go
            traceService.Trace("Bước 3: Lấy danh sách dự án theo mã dự án");
            var fetchXmlProjects = $@"
                <fetch>
                  <entity name='bsd_project'>
                    <attribute name='bsd_projectid' />
                    <attribute name='bsd_projectcode' />
                    <filter>
                      <condition attribute='bsd_projectcode' operator='in'>
                        {string.Join("", projectCodes.Select(code => $"<value>{code}</value>"))}
                      </condition>
                    </filter>
                  </entity>
                </fetch>";
            var projects = service.RetrieveMultiple(new FetchExpression(fetchXmlProjects)).Entities
                .ToDictionary(
                    p => p.Contains("bsd_projectcode") ? p["bsd_projectcode"].ToString() : "",
                    p => p.Id
                );
            traceService.Trace($"Đã lấy {projects.Count} dự án");

            // 4. Map teamId to projectId
            traceService.Trace("Bước 4: Ánh xạ teamId sang projectId");
            var teamIdToProjectId = teams.ToDictionary(
                t => t.Id.ToString(),
                t =>
                {
                    var name = t.Contains("name") ? t["name"].ToString() : "";
                    var projectCode = name.Split('-')[0];
                    return projects.ContainsKey(projectCode) ? projects[projectCode] : Guid.Empty;
                }
            );

            // 5. Create sharecustomer and sharecustomerproject records
            traceService.Trace("Bước 5: Tạo sharecustomer và sharecustomerproject");
            foreach (string customerId in arrID)
            {
                traceService.Trace($"Tạo sharecustomer cho khách hàng {customerId}");
                Entity shareCustomer = new Entity("bsd_sharecustomers");
                shareCustomer["bsd_name"] = $"sharecustomer {DateTime.UtcNow:yyyyMMddHHmmssfff}";
                if (type == 2 || type == 0)
                    shareCustomer["bsd_customer"] = new EntityReference("contact", Guid.Parse(customerId));
                if(type == 3 || type == 1)
                    shareCustomer["bsd_customer"] = new EntityReference("account", Guid.Parse(customerId));
                shareCustomer["ownerid"] = new EntityReference("systemuser", context.UserId);
                Guid shareCustomerId = service.Create(shareCustomer);
                traceService.Trace($"Đã tạo sharecustomer với Id: {shareCustomerId}");

                foreach (string teamId in arrTeamID)
                {
                    Guid projectId = teamIdToProjectId.ContainsKey(teamId) ? teamIdToProjectId[teamId] : Guid.Empty;
                    traceService.Trace($"Tạo sharecustomerproject cho team {teamId}, projectId: {projectId}");
                    Entity shareCustomerProject = new Entity("bsd_sharecustomerproject");
                    shareCustomerProject["bsd_sharecustomer"] = new EntityReference("bsd_sharecustomers", shareCustomerId);
                    if (projectId != Guid.Empty)
                        shareCustomerProject["bsd_project"] = new EntityReference("bsd_project", projectId);
                    service.Create(shareCustomerProject);
                }
            }
            traceService.Trace("Kết thúc CreateShareCustomer");
        }
        private void ShareCustomerRecordToProjectTeams(Guid shareCustomerId)
        {
            traceService.Trace("Bắt đầu ShareCustomerRecordToProjectTeams");
            // Retrieve sharecustomer record
            Entity shareCustomer = service.Retrieve("bsd_sharecustomers", shareCustomerId, new ColumnSet("bsd_customer", "statuscode"));
            if (!shareCustomer.Contains("bsd_customer"))
            {
                traceService.Trace("Không tìm thấy trường customer trong sharecustomer");
                throw new InvalidPluginExecutionException("ShareCustomer does not have a customer reference.");
            }

            EntityReference customerRef = (EntityReference)shareCustomer["bsd_customer"];

            // Retrieve all sharecustomerproject records linked to this sharecustomer
            traceService.Trace("Lấy danh sách sharecustomerproject liên kết với sharecustomer");
            var fetchXml = $@"
                <fetch>
                  <entity name='bsd_sharecustomerproject'>
                    <attribute name='bsd_project' />
                    <filter>
                      <condition attribute='bsd_sharecustomer' operator='eq' value='{shareCustomerId}' />
                    </filter>
                  </entity>
                </fetch>";
            var shareCustomerProjects = service.RetrieveMultiple(new FetchExpression(fetchXml));

            HashSet<string> processedProjectCodes = new HashSet<string>();

            foreach (var scp in shareCustomerProjects.Entities)
            {
                if (!scp.Contains("bsd_project")) continue;
                EntityReference projectRef = (EntityReference)scp["bsd_project"];

                // Retrieve project to get project code
                Entity project = service.Retrieve("bsd_project", projectRef.Id, new ColumnSet("bsd_projectcode"));
                if (!project.Contains("bsd_projectcode")) continue;
                string projectCode = project["bsd_projectcode"].ToString();

                // Avoid duplicate sharing for the same project
                if (processedProjectCodes.Contains(projectCode))
                {
                    traceService.Trace($"Dự án {projectCode} đã được xử lý, bỏ qua.");
                    continue;
                }
                processedProjectCodes.Add(projectCode);

                // Team names to share
                string[] teamNames = new string[]
                {
                    $"{projectCode}-CCR-TEAM",
                    $"{projectCode}-FINANCE-TEAM",
                    $"{projectCode}-SALE-MGT",
                    $"{projectCode}-SALE-ADMIN"
                };

                // Fetch teams by name
                traceService.Trace($"Lấy danh sách team theo tên cho dự án {projectCode}");
                var fetchXmlTeams = $@"
                    <fetch>
                      <entity name='team'>
                        <attribute name='teamid' />
                        <attribute name='name' />
                        <filter>
                          <condition attribute='name' operator='in'>
                            {string.Join("", teamNames.Select(n => $"<value>{n}</value>"))}
                          </condition>
                        </filter>
                      </entity>
                    </fetch>";
                var teams = service.RetrieveMultiple(new FetchExpression(fetchXmlTeams));
                traceService.Trace($"Đã lấy {teams.Entities.Count} team cho dự án {projectCode}");

                // Share customer record to each team
                foreach (var team in teams.Entities)
                {
                    EntityReference teamRef = new EntityReference("team", team.Id);
                    traceService.Trace($"Chia sẻ khách hàng cho team {team["name"]} (Id: {team.Id})");
                    GrantAccessRequest grantRequest = new GrantAccessRequest
                    {
                        Target = customerRef,
                        PrincipalAccess = new PrincipalAccess
                        {
                            Principal = teamRef,
                            AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess
                        }
                    };
                    service.Execute(grantRequest);
                }
            }
            // Update statuscode to 2
            traceService.Trace("Cập nhật trạng thái sharecustomer sang 2 (đã chia sẻ)");
            Entity updateShareCustomer = new Entity("bsd_sharecustomers", shareCustomerId);
            updateShareCustomer["statuscode"] = new OptionSetValue(100000000);
            updateShareCustomer["bsd_approver"] = new EntityReference("systemuser", context.UserId);
            updateShareCustomer["bsd_approvdeate"] = DateTime.UtcNow.AddHours(7);

            service.Update(updateShareCustomer);
            traceService.Trace("Kết thúc ShareCustomerRecordToProjectTeams");
        }
    }
    public class TeamReturn
    {
        public string TeamID { get; set; }
        public string TeamName { get; set; }

    }
}
