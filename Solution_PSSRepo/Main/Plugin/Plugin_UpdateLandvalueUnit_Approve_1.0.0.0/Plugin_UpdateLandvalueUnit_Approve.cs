// Decompiled with JetBrains decompiler
// Type: Plugin_UpdateLandvalueUnit_Approve.Plugin_UpdateLandvalueUnit_Approve
// Assembly: Plugin_UpdateLandvalueUnit_Approve, Version=1.0.0.0, Culture=neutral, PublicKeyToken=c09c887dd246e957
// MVID: C8DFA1B7-5EFF-446E-9836-78F9367E6BE3
// Assembly location: C:\Users\ngoct\Downloads\New folder\Plugin_UpdateLandvalueUnit_Approve_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.Remoting.Services;


namespace Plugin_UpdateLandvalueUnit_Approve
{
    public class Plugin_UpdateLandvalueUnit_Approve : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        private ITracingService tracingService = (ITracingService)null;
        private ParameterCollection target = (ParameterCollection)null;
        private IPluginExecutionContext context = (IPluginExecutionContext)null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            this.target = service.InputParameters;
            Entity entity1 = (Entity)this.target["Target"];
            if (!(service.MessageName == "Update") || !(entity1.LogicalName == "bsd_updatelandvalue"))
                return;
            Entity entity2 = this.service.Retrieve(entity1.LogicalName, entity1.Id, new ColumnSet(true));
            if (((OptionSetValue)entity2["statuscode"]).Value == 100000001)
            {
                EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("\r\n                            <fetch>\r\n                              <entity name='bsd_landvalue'>\r\n                                <all-attributes />\r\n                                <filter>\r\n                                  <condition attribute='statecode' operator='eq' value='0'/>\r\n                                  <condition attribute='bsd_updatelandvalue' operator='eq' value='{0}'/>\r\n                                </filter>\r\n                              </entity>\r\n                            </fetch>", (object)entity2.Id)));
                if (entityCollection.Entities.Count > 0)
                {
                    foreach (Entity entity3 in (Collection<Entity>)entityCollection.Entities)
                    {
                        //#update
                        #region chuyển qua action để chạy PA từng item
                        //        this.service.Update(new Entity(entity3.LogicalName, entity3.Id)
                        //        {
                        //            ["statecode"] = (object)new OptionSetValue(0),
                        //            ["statuscode"] = (object)new OptionSetValue(100000002)
                        //        });
                        //        Entity entity4 = this.service.Retrieve(entity3.LogicalName, entity3.Id, new ColumnSet(true));
                        //        if (((OptionSetValue)entity4["bsd_type"]).Value == 100000000)
                        //        {
                        //            Decimal num1 = entity4.Contains("bsd_listedpricenew") ? ((Money)entity4["bsd_listedpricenew"]).Value : 0M;
                        //            Decimal num2 = entity4.Contains("bsd_discountnew") ? ((Money)entity4["bsd_discountnew"]).Value : 0M;
                        //            Decimal num3 = entity4.Contains("bsd_handoverconditionamountnew") ? ((Money)entity4["bsd_handoverconditionamountnew"]).Value : 0M;
                        //            Decimal num4 = entity4.Contains("bsd_netsellingpricenew") ? ((Money)entity4["bsd_netsellingpricenew"]).Value : 0M;
                        //            Decimal num5 = entity4.Contains("bsd_landvaluedeductionnew") ? ((Money)entity4["bsd_landvaluedeductionnew"]).Value : 0M;
                        //            Decimal num6 = entity4.Contains("bsd_totalvattaxnew") ? ((Money)entity4["bsd_totalvattaxnew"]).Value : 0M;
                        //            Decimal num7 = entity4.Contains("bsd_maintenancefeenew") ? ((Money)entity4["bsd_maintenancefeenew"]).Value : 0M;
                        //            Decimal num8 = entity4.Contains("bsd_totalamountnew") ? ((Money)entity4["bsd_totalamountnew"]).Value : 0M;
                        //            EntityReference entityReference1 = entity4.Contains("bsd_optionentry") ? (EntityReference)entity4["bsd_optionentry"] : (EntityReference)null;
                        //            Entity entity5 = this.service.Retrieve(entityReference1.LogicalName, entityReference1.Id, new ColumnSet(new string[5]
                        //            {
                        //"salesorderid",
                        //"bsd_landvaluededuction",
                        //"totaltax",
                        //"bsd_freightamount",
                        //"totalamount"
                        //            }));
                        //            EntityReference entityReference2 = entity4.Contains("bsd_units") ? (EntityReference)entity4["bsd_units"] : (EntityReference)null;
                        //            this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true));
                        //            //#update
                        //            this.service.Update(new Entity(entity5.LogicalName, entity5.Id)
                        //            {
                        //                ["bsd_landvaluededuction"] = (object)new Money(num5),
                        //                ["totaltax"] = (object)new Money(num6),
                        //                ["bsd_freightamount"] = (object)new Money(num7),
                        //                ["totalamount"] = (object)new Money(num8)
                        //            });
                        //            //#update
                        //            this.service.Update(new Entity("salesorderdetail", this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("\r\n                        <fetch>\r\n                          <entity name='salesorderdetail'>\r\n                            <all-attributes />\r\n                            <filter>\r\n                              <condition attribute='salesorderid' operator='eq' value='{0}'/>\r\n                            </filter>\r\n                          </entity>\r\n                        </fetch>", (object)entity5.Id))).Entities[0].Id)
                        //            {
                        //                ["tax"] = (object)new Money(num6),
                        //                ["extendedamount"] = (object)new Money(num1 + num6)
                        //            });
                        //            if (entity4.Contains("bsd_installment"))
                        //            {
                        //                Decimal num9 = entity4.Contains("bsd_amountofthisphase") ? ((Money)entity4["bsd_amountofthisphase"]).Value : 0M;
                        //                if (num9 > 0M)
                        //                {
                        //                    EntityReference entityReference3 = entity4.Contains("bsd_installment") ? (EntityReference)entity4["bsd_installment"] : (EntityReference)null;
                        //                    Entity entity6 = this.service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(true));
                        //                    entity6["bsd_amountofthisphase"] = (object)num9;
                        //                    this.service.Update(entity6);
                        //                }
                        //            }
                        //        }
                        //        if (entity4.Contains("bsd_units"))
                        //        {
                        //            EntityReference entityReference = entity4.Contains("bsd_units") ? (EntityReference)entity4["bsd_units"] : (EntityReference)null;
                        //            Entity entity7 = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(true));
                        //            //#update
                        //            this.service.Update(new Entity(entity7.LogicalName, entity7.Id)
                        //            {
                        //                ["bsd_landvalueofunit"] = entity4["bsd_landvaluenew"]
                        //            });
                        //        }
                        #endregion
                    }
                    tracingService.Trace("count: " + entityCollection.Entities.Count);
                    Entity enUpdate = new Entity(entity2.LogicalName, entity2.Id);
                    enUpdate["bsd_processing_pa"] = true; //Status Reason(entity Detail) = Approved
                    enUpdate["bsd_error"] = false;
                    enUpdate["bsd_errordetail"] = "";
                    enUpdate["bsd_approvedrejectedperson"] = (object)new EntityReference("systemuser", this.context.UserId);
                    enUpdate["bsd_approvedrejecteddate"] = (object)RetrieveLocalTimeFromUTCTime(DateTime.Now);
                    this.service.Update(enUpdate);
                    var request = new OrganizationRequest("bsd_Action_Action_UpdateLandValue");
                    string listid = string.Join(",", entityCollection.Entities.Select(x => x.Id.ToString()));
                    request["listid"] = listid;
                    request["idmaster"] = entity2.Id.ToString();
                    this.service.Execute(request);
                }
            }
        }

        public DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            return ((LocalTimeFromUtcTimeResponse)this.service.Execute((OrganizationRequest)new LocalTimeFromUtcTimeRequest()
            {
                TimeZoneCode = this.RetrieveCurrentUsersSettings(this.service) == null ? throw new InvalidPluginExecutionException("Can't find time zone code") : this.RetrieveCurrentUsersSettings(this.service).Value,
                UtcTime = utcTime.ToUniversalTime()
            })).LocalTime;
        }

        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            IOrganizationService organizationService = service;
            QueryExpression queryExpression1 = new QueryExpression("usersettings");
            queryExpression1.ColumnSet = new ColumnSet(new string[2]
            {
        "localeid",
        "timezonecode"
            });
            QueryExpression queryExpression2 = queryExpression1;
            FilterExpression filterExpression = new FilterExpression();
            filterExpression.Conditions.Add(new ConditionExpression("systemuserid", ConditionOperator.EqualUserId));
            queryExpression2.Criteria = filterExpression;
            QueryExpression query = queryExpression1;
            return (int?)organizationService.RetrieveMultiple((QueryBase)query).Entities[0].ToEntity<Entity>().Attributes["timezonecode"];
        }

    }
}
