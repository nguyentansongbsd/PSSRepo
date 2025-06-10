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
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Guid AdminID = new Guid("{d90ce220-655a-e811-812e-3863bb36dc00}");//CRM ADMIN
            Guid CurrentUser = context.UserId;
            service = factory.CreateOrganizationService(AdminID);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");

            int type = (int)context.InputParameters["type"];
            string id = context.InputParameters["id"].ToString();
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
            id = id.TrimEnd(',');
            //throw new InvalidPluginExecutionException(id);
            string[] arrID = id.Split(',');
            string fieldName = ""; //type == 0 ? "contact" : "account";
            traceService.Trace("step1");
            if (type == 0)//KHCN
                fieldName = "contact";
            else if (type == 1)
                fieldName = "account";
            else if(type==10)
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
                            EntityCollection rs_ = service.RetrieveMultiple(new FetchExpression(fetchXml));
                            foreach (Entity i in rs_.Entities)
                            {
                                if (i.Contains("bsd_representative"))
                                    ShareTeams((EntityReference)i["bsd_representative"], refTeam);
                                if (i.Contains("bsd_contact"))
                                    ShareTeams((EntityReference)i["bsd_contact"], refTeam);
                            }
                        }
                        var team = it;
                        var fetchXml2 = $@"
                <fetch>
                  <entity name='team'>
                    <filter>
                      <condition attribute='name' operator='like' value='%{team["name"].ToString().Split('_')[0]}%'/>
                    </filter>
                  </entity>
                </fetch>";
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
                        if (list.Any(x => x.TeamName == i.TeamName) == false)
                            list.Add(i);
                    }
                    var serializer = new JavaScriptSerializer();
                    context.OutputParameters["entityColl"] = serializer.Serialize(list);
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
                            if (i.Contains("bsd_contact"))
                                ShareTeams((EntityReference)i["bsd_contact"], refTeam);
                        }
                    }
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
            catch(Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message+" Vui Lòng thử lại", ex);
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
    }
    public class TeamReturn
    {
        public string TeamID { get; set; }
        public string TeamName { get; set; }

    }
}
