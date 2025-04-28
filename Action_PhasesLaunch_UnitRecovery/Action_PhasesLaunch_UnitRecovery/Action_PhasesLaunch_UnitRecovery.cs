using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Action_PhasesLaunch_UnitRecovery
{
    public class Action_PhasesLaunch_UnitRecovery : IPlugin
    {
        private IOrganizationService service = null;
        private IOrganizationServiceFactory factory = null;
        private IOrganizationService serviceAdmin = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            EntityReference inputParameter = (EntityReference)context.InputParameters["Target"];
            if (!(inputParameter.LogicalName == "bsd_phaseslaunch"))
                return;
            string str1 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["type"]))
            {
                str1 = context.InputParameters["type"].ToString();
            }
            string inoverqueue = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["inoverqueue"]))
            {
                inoverqueue = context.InputParameters["inoverqueue"].ToString();
            }
            string instunits = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["instunits"]))
            {
                instunits = context.InputParameters["instunits"].ToString();
            }
            string inquotes = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["inquotes"]))
            {
                inquotes = context.InputParameters["inquotes"].ToString();
            }
            switch (str1)
            {
                case "unit recovery b0":
                    {
                        service = factory.CreateOrganizationService(context.UserId);
                        ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                        Entity Phl = service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[1] { "statuscode" }));
                        if (CheckEvent(service, Phl.Id))
                        {
                            throw new InvalidPluginExecutionException("This Phases Launch is currently in an Event, cannot proceed to recovery. Please check again.");
                        }
                        var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                        <fetch top=""1"">
                          <entity name=""product"">
                            <attribute name=""productid"" />
                            <filter>
                              <condition attribute=""bsd_phaseslaunchid"" operator=""eq"" value=""{inputParameter.Id}"" />
                              <condition attribute=""statuscode"" operator=""ne"" value=""{1}"" />
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                        if (rs.Entities.Count == 0) throw new InvalidPluginExecutionException("If you want to perform recovery action, please use 'Recovery' feature.");
                        Entity enUp = new Entity(inputParameter.LogicalName, inputParameter.Id);
                        enUp["bsd_powerautomate"] = true;
                        service.Update(enUp);
                        string url = "";
                        EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                            new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Phases Launch Unit Recovery");
                        foreach (Entity item in configGolive.Entities)
                        {
                            if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                        }
                        if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                        context.OutputParameters["url"] = url;
                        context.OutputParameters["idUser0"] = context.UserId.ToString();
                        break;
                    }
                case "unit recovery":
                    {
                        string idUser = "";
                        if (!string.IsNullOrEmpty((string)context.InputParameters["idUser"]))
                        {
                            idUser = context.InputParameters["idUser"].ToString();
                        }
                        service = factory.CreateOrganizationService(Guid.Parse(idUser));
                        serviceAdmin = factory.CreateOrganizationService(context.UserId);
                        ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                        string intotal = "";
                        if (!string.IsNullOrEmpty((string)context.InputParameters["intotal"]))
                        {
                            intotal = context.InputParameters["intotal"].ToString();
                            int num3 = 0;
                            if (inoverqueue != "") num3 = int.Parse(inoverqueue);
                            EntityCollection entityCollection2 = RetrieveMultiRecord(service, "product", new ColumnSet(new string[3] { "statuscode", "bsd_phaseslaunchid", "bsd_locked" }), "productid", Guid.Parse(intotal));
                            string guidList1 = "";
                            string guidList2 = "";
                            foreach (Entity entity in (Collection<Entity>)entityCollection2.Entities)
                            {
                                OptionSetValue optionSetValue = (OptionSetValue)entity["statuscode"];
                                if (optionSetValue.Value != 100000002)
                                {
                                    List<Guid> quoteID = new List<Guid>();
                                    if (!CheckReservation(service, entity.Id, inputParameter.Id, ref quoteID))
                                    {
                                        if (optionSetValue.Value == 100000000 || optionSetValue.Value == 100000004)
                                        {
                                            if (optionSetValue.Value == 100000004)
                                            {
                                                CancelQueue(service, entity.Id);
                                            }
                                            Recovery(service, entity);
                                        }
                                    }
                                    else
                                    {
                                        ++num3;
                                        guidList2 = entity.Id.ToString();
                                        if (quoteID.Count > 0)
                                        {
                                            foreach (Guid guid2 in quoteID)
                                                guidList1 = guid2.ToString();
                                        }
                                    }
                                }
                            }
                            context.OutputParameters["overqueue"] = num3.ToString();
                            if (instunits != "")
                            {
                                if (guidList2 != "")
                                {
                                    context.OutputParameters["stunits"] = instunits + ";" + guidList2;
                                }
                                else
                                {
                                    context.OutputParameters["stunits"] = instunits;
                                }
                            }
                            else
                            {
                                if (guidList2 != "")
                                {
                                    context.OutputParameters["stunits"] = guidList2;
                                }
                                else
                                {
                                    context.OutputParameters["stunits"] = instunits;
                                }
                            }
                            if (inquotes != "")
                            {
                                if (guidList1 != "")
                                {
                                    context.OutputParameters["quotes"] = inquotes + ";" + guidList1;
                                }
                                else
                                {
                                    context.OutputParameters["quotes"] = inquotes;
                                }
                            }
                            else
                            {
                                if (guidList1 != "")
                                {
                                    context.OutputParameters["quotes"] = guidList1;
                                }
                                else
                                {
                                    context.OutputParameters["quotes"] = inquotes;
                                }
                            }
                        }
                        break;
                    }
                case "unit recovery b1":
                    {
                        string idUser = "";
                        if (!string.IsNullOrEmpty((string)context.InputParameters["idUser"]))
                        {
                            idUser = context.InputParameters["idUser"].ToString();
                        }
                        service = factory.CreateOrganizationService(Guid.Parse(idUser));
                        serviceAdmin = factory.CreateOrganizationService(context.UserId);
                        ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                        string intotal = "";
                        if (!string.IsNullOrEmpty((string)context.InputParameters["intotal"]))
                        {
                            intotal = context.InputParameters["intotal"].ToString();
                            string source = "";
                            Entity entity2 = service.Retrieve("product", Guid.Parse(intotal), new ColumnSet(new string[1] { "statuscode" }));
                            int statuscode = ((OptionSetValue)entity2["statuscode"]).Value;
                            if (statuscode != 1)
                            {
                                if (statuscode == 100000004 || statuscode == 100000000)
                                {
                                    if (Checkreservation(service, entity2.Id, inputParameter.Id))
                                        source = entity2.Id.ToString();
                                }
                                else
                                    source = entity2.Id.ToString();
                            }
                            if (instunits != "")
                            {
                                if (source != "")
                                {
                                    context.OutputParameters["stunits"] = instunits + ";" + source;
                                }
                                else
                                {
                                    context.OutputParameters["stunits"] = instunits;
                                }
                            }
                            else
                            {
                                if (source != "")
                                {
                                    context.OutputParameters["stunits"] = source;
                                }
                                else
                                {
                                    context.OutputParameters["stunits"] = instunits;
                                }
                            }
                        }
                        break;
                    }
                case "unit recovery b2":
                    {
                        string idUser = "";
                        if (!string.IsNullOrEmpty((string)context.InputParameters["idUser"]))
                        {
                            idUser = context.InputParameters["idUser"].ToString();
                        }
                        service = factory.CreateOrganizationService(Guid.Parse(idUser));
                        serviceAdmin = factory.CreateOrganizationService(context.UserId);
                        ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                        Entity Phl = service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[1] { "statuscode" }));
                        if (inoverqueue != "" && int.Parse(inoverqueue) > 0)
                        {
                            Guid empty = Guid.Empty;
                            CopyPhasesLaunch(service, Phl, ref empty);
                            if (empty.CompareTo(Guid.Empty) != 0)
                                context.OutputParameters["idNew"] = empty.ToString();
                            else
                                context.OutputParameters["idNew"] = "null";
                        }
                        break;
                    }
            }
        }
        private void Recovery(IOrganizationService services, Entity unit)
        {
            Entity entity = new Entity(unit.LogicalName);
            entity.Id = unit.Id;
            entity["statuscode"] = new OptionSetValue(1);
            if (unit.Contains("bsd_phaseslaunchid"))
                entity["bsd_phaseslaunchid"] = null;
            if (unit.Contains("bsd_locked"))
                entity["bsd_locked"] = null;
            service.Update(entity);
        }

        private bool CheckEvent(IOrganizationService service, Guid PhasesLaunch)
        {
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='bsd_event' >
                    <attribute name='bsd_eventid' />
                    <attribute name='bsd_name' />
                    <attribute name='createdon' />
                    <filter type='and' >
                      <condition attribute='bsd_startdate' operator='on-or-before' value='{0}' />
                      <condition attribute='bsd_enddate' operator='on-or-after' value='{0}' />
                      <condition attribute='bsd_phaselaunch' operator='eq' value='{1}' />
                      <condition attribute='statuscode' operator='eq' value='100000000' />
                    </filter>
                    <order attribute='bsd_name' />
                  </entity>
            </fetch>";

            fetchXml = string.Format(fetchXml, DateTime.Today.ToString(), PhasesLaunch);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.Count > 0;
        }

        private void CancelQueue(IOrganizationService services, Guid UnitID)
        {
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='opportunity' >
                    <attribute name='name' />
                    <attribute name='statuscode' />
                    <attribute name='opportunityid' />
                    <attribute name='createdon' />
                    <attribute name='bsd_queuingexpired' />
                    <filter type='and' >
                      <condition attribute='bsd_queuingexpired' operator='not-null' />
                      <condition attribute='statuscode' operator='in' >
                      <value>100000002</value>
                      <value>100000000</value>
                      </condition>
                    </filter>
                    <link-entity name='opportunityproduct' from='bsd_booking' to='opportunityid'>
                      <filter>
                        <condition attribute='productid' operator='eq' value='{0}' />
                      </filter>
                    </link-entity>
                  </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, UnitID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entc.Entities.Count <= 0)
                return;
            foreach (Entity entity1 in entc.Entities)
            {
                LoseOpportunityRequest opportunityRequest = new LoseOpportunityRequest();
                Entity entity2 = new Entity("opportunityclose");
                entity2["opportunityid"] = new EntityReference("opportunity", entity1.Id);
                opportunityRequest.OpportunityClose = entity2;
                opportunityRequest.Status = new OptionSetValue(4);
                service.Execute(opportunityRequest);
            }
        }

        private bool Checkreservation(IOrganizationService services, Guid unitID, Guid PLID)
        {
            bool flag = false;
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='quote' >
                    <attribute name='quoteid' />
                    <attribute name='name' />
                    <filter type='and' >
                      <condition attribute='bsd_phaseslaunchid' operator='eq' value='{1}' />
                      <condition attribute='statuscode' operator='in' >
                      <value>100000004</value>
                      <value>100000000</value>
                      <value>100000006</value>
                      <value>100000007</value>
                      <value>3</value>
                      <value>4</value>
                      </condition>
                    </filter>
                    <link-entity name='quotedetail' from='quoteid' to='quoteid'>
                      <filter>
                        <condition attribute='productid' operator='eq' value='{0}' />
                      </filter>
                    </link-entity>
                  </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, unitID, PLID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entc.Entities.Count <= 0)
                flag = true;
            return flag;
        }

        private bool CheckReservation(IOrganizationService services, Guid unitID, Guid PhasesLaunchID, ref List<Guid> quoteID)
        {
            bool flag = false;
            string fetchXml =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='product' >
                    <attribute name='name' alias='unit' />
                    <filter type='and' >
                      <condition attribute='productid' operator='eq' value='{0}' />
                    </filter>
                    <link-entity name='quote' from='bsd_unitno' to='productid'>
                      <attribute name='quoteid' alias='quoteid' />
                      <attribute name='statuscode' alias='quotestatus' />
                      <filter>
                        <condition attribute='statuscode' operator='in' >
                          <value>100000004</value>
                          <value>100000000</value>
                          <value>100000006</value>
                          <value>100000007</value>
                          <value>3</value>
                          </condition>
                      </filter>
                    </link-entity>
                  </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, unitID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entc.Entities.Count > 0)
            {
                foreach (Entity entity in entc.Entities)
                {
                    quoteID.Add((Guid)entity["quoteid"]);
                }
                flag = true;
            }
            string fetchXml2 =
                  @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='product' >
                    <attribute name='name' alias='unit' />
                    <filter type='and' >
                      <condition attribute='productid' operator='eq' value='{0}' />
                    </filter>
                    <link-entity name='quote' from='bsd_unitno' to='productid'>
                      <attribute name='quoteid' alias='quoteid' />
                      <attribute name='statuscode' alias='quotestatus' />
                      <filter>
                        <condition attribute='statuscode' operator='in' >
                          <value>4</value>
                          </condition>
                      </filter>
                      <link-entity name='salesorder' from='quoteid' to='quoteid'>
                          <attribute name='salesorderid' alias='orderid' />
                          <filter>
                            <condition attribute='statuscode' operator='ne' value='100000006' />
                          </filter>
                      </link-entity>
                    </link-entity>
                  </entity>
            </fetch>";
            fetchXml2 = string.Format(fetchXml2, unitID);
            EntityCollection entc2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
            if (entc2.Entities.Count > 0)
            {
                foreach (Entity entity in entc2.Entities)
                {
                    quoteID.Add((Guid)entity["quoteid"]);
                }
                flag = true;
            }
            return flag;
        }

        private void CopyPhasesLaunch(IOrganizationService services, Entity Phl, ref Guid id)
        {
            Entity entity1 = serviceAdmin.Retrieve(Phl.LogicalName, Phl.Id, new ColumnSet(new string[10]
            {
        "bsd_name",
        "bsd_projectid",
        "bsd_pricelistid",
        "bsd_discountlist",
        "ownerid",
        "bsd_salesagentcompany",
        "bsd_locked",
        "bsd_commissionforagent",
        "bsd_commission",
        "bsd_enddate"
            }));
            string str = "copy" + entity1["bsd_name"]?.ToString();
            DateTime dateTime = RetrieveLocalTimeFromUTCTime(DateTime.Now);
            Entity entity2 = new Entity(entity1.LogicalName);
            entity2["bsd_name"] = str;
            entity2["bsd_projectid"] = entity1["bsd_projectid"];
            entity2["bsd_pricelistid"] = entity1["bsd_pricelistid"];
            entity2["bsd_discountlist"] = entity1["bsd_discountlist"];
            entity2["statuscode"] = new OptionSetValue(1);
            if (entity1.Contains("bsd_salesagentcompany"))
                entity2["bsd_salesagentcompany"] = entity1["bsd_salesagentcompany"];
            if (entity1.Contains("bsd_commissionforagent"))
                entity2["bsd_commissionforagent"] = entity1["bsd_commissionforagent"];
            if (entity1.Contains("bsd_locked"))
                entity2["bsd_locked"] = entity1["bsd_locked"];
            if (entity1.Contains("bsd_enddate"))
                entity2["bsd_enddate"] = entity1["bsd_enddate"];
            entity2["bsd_startdate"] = dateTime;
            entity2["bsd_commission"] = entity1["bsd_commission"];
            entity2["ownerid"] = entity1["ownerid"];
            id = serviceAdmin.Create(entity2);
            EntityCollection entityCollection = RetrieveMultiRecord(service, "bsd_promotion", new ColumnSet(new string[7]
            {
        "bsd_name",
        "bsd_values",
        "bsd_startdate",
        "statuscode",
        "bsd_enddate",
        "bsd_approverjectdate",
        "bsd_approverrejecter"
            }), "bsd_phaselaunch", entity1.Id);
            foreach (Entity entity3 in entityCollection.Entities)
            {
                Entity entity4 = new Entity(entity3.LogicalName);
                entity4["bsd_name"] = entity3["bsd_name"];
                entity4["bsd_phaselaunch"] = new EntityReference(entity1.LogicalName, id);
                entity4["bsd_values"] = entity3["bsd_values"];
                if (entity3.Contains("bsd_startdate"))
                    entity4["bsd_startdate"] = entity3["bsd_startdate"];
                entity4["statuscode"] = entity3["statuscode"];
                if (entity3.Contains("bsd_enddate"))
                    entity4["bsd_enddate"] = entity3["bsd_enddate"];
                if (entity3.Contains("bsd_approverjectdate"))
                    entity4["bsd_approverjectdate"] = entity3["bsd_approverjectdate"];
                if (entity3.Contains("bsd_approverrejecter"))
                    entity4["bsd_approverrejecter"] = entity3["bsd_approverrejecter"];
                service.Create(entity4);
            }
            QueryExpression q = new QueryExpression("bsd_bsd_phaseslaunch_bsd_packageselling");
            q.ColumnSet = new ColumnSet(new string[1] { "bsd_packagesellingid" });
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression("bsd_phaseslaunchid", ConditionOperator.Equal, entity1.Id));
            EntityCollection entc = service.RetrieveMultiple(q);
            EntityReferenceCollection referenceCollection = new EntityReferenceCollection();
            foreach (Entity entity5 in entc.Entities)
                referenceCollection.Add(new EntityReference("bsd_packageselling", (Guid)entity5["bsd_packagesellingid"]));
            if (referenceCollection.Count <= 0)
                return;
            serviceAdmin.Associate(entity1.LogicalName, id, new Relationship("bsd_bsd_phaseslaunch_bsd_packageselling"), referenceCollection);
        }
        private EntityCollection RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression queryExpression = new QueryExpression(entity);
            queryExpression.ColumnSet = column;
            queryExpression.Criteria = new FilterExpression();
            queryExpression.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            return service.RetrieveMultiple((QueryBase)queryExpression);
        }

        public DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            int? timeZoneCode = RetrieveCurrentUsersSettings(service);
            if (!timeZoneCode.HasValue)
                throw new InvalidPluginExecutionException("Can't find time zone code");
            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            LocalTimeFromUtcTimeResponse response = (LocalTimeFromUtcTimeResponse)service.Execute(request);
            return response.LocalTime;
            //var utcTime = utcTime.ToString("MM/dd/yyyy HH:mm:ss");
            //var localDateOnly = response.LocalTime.ToString("dd-MM-yyyy");
        }
        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            var currentUserSettings = service.RetrieveMultiple(
            new QueryExpression("usersettings")
            {
                ColumnSet = new ColumnSet("localeid", "timezonecode"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("systemuserid", ConditionOperator.EqualUserId) }
                }
            }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];
        }
    }
}
