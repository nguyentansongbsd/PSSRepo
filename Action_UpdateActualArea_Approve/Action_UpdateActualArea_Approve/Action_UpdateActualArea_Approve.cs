using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace Action_UpdateActualArea_Approve
{
    public class Action_UpdateActualArea_Approve : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService TracingSe = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            string input01 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input01"]))
            {
                input01 = context.InputParameters["input01"].ToString();//buoc
            }
            string input02 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input02"]))
            {
                input02 = context.InputParameters["input02"].ToString();//id master
            }
            string input03 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input03"]))
            {
                input03 = context.InputParameters["input03"].ToString();//id detail
            }
            string input04 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input04"]))
            {
                input04 = context.InputParameters["input04"].ToString();//id user
            }
            if (input01 == "Buoc01" && input02 != "")
            {
                TracingSe.Trace("Bước 01");
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch top=""1"">
                  <entity name=""bsd_updateactualarea"">
                    <attribute name=""bsd_updateactualareaid"" />
                    <filter>
                      <condition attribute=""bsd_updateactualareaapprove"" operator=""eq"" value=""{input02}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs.Entities.Count == 0) throw new InvalidPluginExecutionException("The Update Actual Area you have chosen doesn't contain any detail. Please check again.");
                Entity enTarget = new Entity("bsd_updateactualareaapprove");
                enTarget.Id = Guid.Parse(input02);
                enTarget["bsd_powerautomate"] = true;
                service.Update(enTarget);
            }
            else if (input01 == "Buoc02" && input02 != "" && input03 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                EntityCollection entityCollection2 = this.service.RetrieveMultiple((QueryBase)new FetchExpression("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                          <entity name='bsd_updateactualarea'>\r\n                            <attribute name='bsd_name' />\r\n                            <attribute name='bsd_netsaleableareasqm' />\r\n                            <attribute name='bsd_updateactualareaapprove' />\r\n                            <attribute name='bsd_units' />\r\n                            <attribute name='bsd_unitofareavariance' />\r\n                            <attribute name='statuscode' />\r\n                            <attribute name='bsd_newprice' />\r\n                            <attribute name='bsd_additionalareabillamount' />\r\n                            <attribute name='bsd_actualareavariance' />\r\n                            <attribute name='bsd_actualarea' />\r\n                            <attribute name='bsd_updateactualareaid' />\r\n                            <order attribute='createdon' descending='true' />\r\n                            <filter type='and'>\r\n                              <condition attribute='bsd_updateactualareaid' operator='eq' value='" + input03 + "' />\r\n                              <condition attribute='bsd_netsaleableareasqm' operator='gt' value='0' />\r\n                              <condition attribute='bsd_includeupdatensa' operator='eq' value='1' />\r\n                            </filter>\r\n                          </entity>\r\n                        </fetch>"));
                foreach (Entity entity in (Collection<Entity>)entityCollection2.Entities)
                {
                    service.Update(new Entity()
                    {
                        LogicalName = "product",
                        Id = ((EntityReference)entity["bsd_units"]).Id,
                        ["bsd_netsaleablearea"] = entity["bsd_netsaleableareasqm"]
                    });
                }
                EntityCollection entityCollection1 = this.RetrieveMultiRecord(this.service, "bsd_updateactualarea", new ColumnSet(new string[4]
                  {
                    "bsd_units",
                    "bsd_actualarea",
                    "bsd_name",
                    "statuscode"
                  }), "bsd_updateactualareaid", input03);
                foreach (Entity entity in (Collection<Entity>)entityCollection1.Entities)
                {
                    UpdateActualArea(entity);
                }
            }
            else if (input01 == "Buoc03" && input02 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enTarget = new Entity("bsd_updateactualareaapprove");
                enTarget.Id = Guid.Parse(input02);
                enTarget["bsd_powerautomate"] = false;
                enTarget["statuscode"] = new OptionSetValue(100000001);
                service.Update(enTarget);
            }
        }
        private void UpdateActualArea(Entity UAA)
        {
            Decimal money = 0M;
            Entity entity1 = new Entity();
            if (!UAA.Contains("bsd_units"))
                throw new InvalidPluginExecutionException("The Update Actual Area you have chosen doesn't contain any unit (detail). Please check again.");
            Entity entity2 = this.service.Retrieve("product", ((EntityReference)UAA["bsd_units"]).Id, new ColumnSet(new string[5]
            {
                "bsd_areavariance",
                "bsd_netsaleablearea",
                "statuscode",
                "name",
                "price"
            }));
            this.service.Update(new Entity(entity2.LogicalName, entity2.Id)
            {
                ["bsd_actualarea"] = UAA["bsd_actualarea"]
            });
            if (!entity2.Contains("bsd_areavariance") || !entity2.Contains("bsd_netsaleablearea"))
            {
                return;
            }
            else
            {
                if (!entity2.Contains("bsd_areavariance") || !entity2.Contains("bsd_netsaleablearea"))
                    return;
                Entity entity3 = new Entity("bsd_updateactualarea");
                entity3.Id = UAA.Id;
                entity3["statuscode"] = (object)new OptionSetValue(100000000);
                entity3["bsd_unitofareavariance"] = entity2["bsd_areavariance"];
                entity3["bsd_actualareavariance"] = (object)((Decimal)UAA["bsd_actualarea"] * 100M / (Decimal)entity2["bsd_netsaleablearea"] - 100M);
                Decimal num = ((Money)entity2["price"]).Value / (Decimal)entity2["bsd_netsaleablearea"] * (Decimal)UAA["bsd_actualarea"];
                entity3["bsd_newprice"] = (object)new Money(num);
                if (this.CheckOE(this.service, entity2.Id) && Math.Abs((Decimal)entity3["bsd_unitofareavariance"]) < Math.Abs((Decimal)entity3["bsd_actualareavariance"]))
                {
                    money = num - ((Money)entity2["price"]).Value;
                    entity3["bsd_additionalareabillamount"] = (object)new Money(money);
                }
                this.UpdatePaymentSchemeDetail(this.service, entity2.Id, money);
                this.service.Update(entity3);
            }
        }

        private void UpdatePaymentSchemeDetail(
          IOrganizationService service,
          Guid productID,
          Decimal money)
        {
            string str = string.Format("<fetch>\r\n                                  <entity name='bsd_paymentschemedetail' >\r\n                                    <attribute name='bsd_paymentschemedetailid' />\r\n                                    <attribute name='bsd_name' />\r\n                                    <filter>\r\n                                      <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002' />\r\n                                    </filter>\r\n                                    <link-entity name='quote' from='quoteid' to='bsd_reservation' >\r\n                                      <filter>\r\n                                        <condition attribute='bsd_unitno' operator='eq' value='{0}' />\r\n                                        <condition attribute='statuscode' operator='neq' value='6' />\r\n                                        <condition attribute='statuscode' operator='neq' value='100000001' />\r\n                                      </filter>\r\n                                    </link-entity>\r\n                                  </entity>\r\n                                </fetch>", (object)productID);
            EntityCollection entityCollection = service.RetrieveMultiple((QueryBase)new FetchExpression(str));
            if (((Collection<Entity>)entityCollection.Entities).Count <= 0)
                return;
            foreach (Entity entity in (Collection<Entity>)entityCollection.Entities)
                service.Update(new Entity(entity.LogicalName, entity.Id)
                {
                    ["bsd_additionalareabill"] = (object)new Money(money)
                });
        }

        private bool CheckOE(IOrganizationService service, Guid Unitid)
        {
            bool flag = true;
            string str = string.Format("<fetch>\r\n                                  <entity name='salesorder' >\r\n                                    <attribute name='statuscode' />\r\n                                    <filter type='and' >\r\n                                      <condition attribute='bsd_unitnumber' operator='eq' value='{0}' />\r\n                                    </filter>\r\n                                    <order attribute='createdon' descending='true' />\r\n                                  </entity>\r\n                                </fetch>", (object)Unitid);
            EntityCollection entityCollection = service.RetrieveMultiple((QueryBase)new FetchExpression(str));
            if (((Collection<Entity>)entityCollection.Entities).Count > 0 && ((OptionSetValue)((Collection<Entity>)entityCollection.Entities)[0]["statuscode"]).Value == 100000006)
                flag = false;
            return flag;
        }
        private EntityCollection RetrieveMultiRecord(
          IOrganizationService crmservices,
          string entity,
          ColumnSet column,
          string condition,
          object value)
        {
            QueryExpression queryExpression = new QueryExpression(entity);
            queryExpression.ColumnSet = column;
            queryExpression.Criteria = new FilterExpression();
            queryExpression.Criteria.AddCondition(new ConditionExpression(condition, (ConditionOperator)0, value));
            return this.service.RetrieveMultiple((QueryBase)queryExpression);
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