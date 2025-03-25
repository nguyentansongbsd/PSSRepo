using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_ConvertToOptionEntry
{
    public class ConvertToOptionEntry : IPlugin
    {
        private IOrganizationService service;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            Entity inputParameter = ((DataCollection<string, object>)((IExecutionContext)service).InputParameters)["Target"] as Entity;
            if (!(inputParameter.LogicalName == "salesorderdetail") || !(((IExecutionContext)service).MessageName == "Create") || !inputParameter.Contains("salesorderid"))
                return;
            this.service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(new Guid?(((IExecutionContext)service).UserId));
            EntityReference entityReference = (EntityReference)inputParameter["salesorderid"];
            Entity entity1 = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(new string[1]
            {
        "quoteid"
            }));
            if (!entity1.Contains("quoteid"))
                return;
            EntityReference quote = (EntityReference)entity1["quoteid"];
            Entity entity2 = this.service.Retrieve(quote.LogicalName, quote.Id, new ColumnSet(new string[14]
            {
        "bsd_unitno",
        "bsd_unitstatus",
        "bsd_netusablearea",
        "bsd_constructionarea",
        "bsd_salessgentcompany",
        "bsd_taxcode",
        "bsd_numberofmonthspaidmf",
        "bsd_managementfee",
        "bsd_totalamountpaid",
        "bsd_rfsigneddate",
        "bsd_nameofstaffagent",
        "bsd_actualarea",
        "bsd_bookingfee",
        "bsd_depositfee"
            }));
            if (!entity2.Contains("bsd_rfsigneddate"))
                throw new InvalidPluginExecutionException("Reservation had not signed. Can not execute this action!");
            Entity entity3 = new Entity(entityReference.LogicalName, entityReference.Id);
            entity3["bsd_unitnumber"] = entity2.Contains("bsd_unitno") ? entity2["bsd_unitno"] : (object)null;
            entity3["bsd_unitstatus"] = entity2.Contains("bsd_unitstatus") ? entity2["bsd_unitstatus"] : (object)null;
            entity3["bsd_constructionarea"] = entity2.Contains("bsd_constructionarea") ? entity2["bsd_constructionarea"] : (object)null;
            entity3["bsd_netusablearea"] = entity2.Contains("bsd_netusablearea") ? entity2["bsd_netusablearea"] : (object)null;
            entity3["bsd_salesagentcompany"] = entity2.Contains("bsd_salessgentcompany") ? entity2["bsd_salessgentcompany"] : (object)null;
            entity3["bsd_queuingfee"] = entity2.Contains("bsd_bookingfee") ? entity2["bsd_bookingfee"] : (object)null;
            entity3["bsd_depositamount"] = entity2.Contains("bsd_depositfee") ? entity2["bsd_depositfee"] : (object)null;
            entity3["bsd_contractdate"] = (object)this.RetrieveLocalTimeFromUTCTime(DateTime.Now, this.service);
            entity3["bsd_actualarea"] = entity2.Contains("bsd_actualarea") ? entity2["bsd_actualarea"] : (object)null;
            entity3["bsd_nameofstaffagent"] = entity2.Contains("bsd_nameofstaffagent") ? entity2["bsd_nameofstaffagent"] : (object)null;
            entity3["bsd_taxcode"] = entity2.Contains("bsd_taxcode") ? entity2["bsd_taxcode"] : (object)null;
            entity3["bsd_numberofmonthspaidmf"] = entity2.Contains("bsd_numberofmonthspaidmf") ? entity2["bsd_numberofmonthspaidmf"] : (object)null;
            entity3["bsd_managementfee"] = entity2.Contains("bsd_managementfee") ? entity2["bsd_managementfee"] : (object)null;
            entity3["bsd_totalamountpaid"] = entity2.Contains("bsd_totalamountpaid") ? entity2["bsd_totalamountpaid"] : (object)null;
            if (entity2.Contains("bsd_unitno"))
            {
                //EntityCollection unitsSpec = this.findUnitsSpec(this.service, (EntityReference)entity2["bsd_unitno"]);
                //if (((Collection<Entity>)unitsSpec.Entities).Count > 0)
                //    entity3["bsd_unitsspecification"] = (object)((Collection<Entity>)unitsSpec.Entities)[0].ToEntityReference();
                Entity entity4 = this.service.Retrieve(((EntityReference)entity2["bsd_unitno"]).LogicalName, ((EntityReference)entity2["bsd_unitno"]).Id, new ColumnSet(new string[1]
                {
          "bsd_estimatehandoverdate"
                }));
                if (entity4.Contains("bsd_estimatehandoverdate"))
                    entity3["bsd_estimatehandoverdatecontract"] = entity4["bsd_estimatehandoverdate"];
            }
            foreach (Entity entity5 in (Collection<Entity>)this.findFirstInstallment(this.service, quote).Entities)
            {
                if (entity5.Contains("statuscode") && ((OptionSetValue)entity5["statuscode"]).Value == 100000001)
                    entity3["statuscode"] = (object)new OptionSetValue(100000001);
            }
            foreach (Entity entity6 in (Collection<Entity>)this.findExchangeRate(this.service).Entities)
                entity3["bsd_applyingexchangerate"] = (object)entity6.ToEntityReference();
            this.service.Update(entity3);
            EntityReferenceCollection referenceCollection = new EntityReferenceCollection();
            QueryExpression queryExpression1 = new QueryExpression("bsd_quote_bsd_promotion");
            queryExpression1.ColumnSet = new ColumnSet(new string[1]
            {
        "bsd_promotionid"
            });
            queryExpression1.Criteria = new FilterExpression((LogicalOperator)0);
            queryExpression1.Criteria.AddCondition(new ConditionExpression("quoteid", (ConditionOperator)0, (object)quote.Id));
            foreach (Entity entity7 in (Collection<Entity>)this.service.RetrieveMultiple((QueryBase)queryExpression1).Entities)
                ((Collection<EntityReference>)referenceCollection).Add(new EntityReference("bsd_promotion", (Guid)entity7["bsd_promotionid"]));
            if (((Collection<EntityReference>)referenceCollection).Count > 0)
                this.service.Associate(entityReference.LogicalName, entityReference.Id, new Relationship("bsd_salesorder_bsd_promotion"), referenceCollection);
            QueryExpression queryExpression2 = new QueryExpression("bsd_discountspecial");
            queryExpression2.ColumnSet = new ColumnSet(new string[1]
            {
        "bsd_discountspecialid"
            });
            queryExpression2.Criteria = new FilterExpression((LogicalOperator)0);
            queryExpression2.Criteria.AddCondition(new ConditionExpression("bsd_quote", (ConditionOperator)0, (object)quote.Id));
            foreach (Entity entity8 in (Collection<Entity>)this.service.RetrieveMultiple((QueryBase)queryExpression2).Entities)
                this.service.Update(new Entity(entity8.LogicalName, entity8.Id)
                {
                    ["bsd_optionentry"] = (object)entityReference
                });
        }

        private EntityCollection findExchangeRate(IOrganizationService crmservices)
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>\r\n                          <entity name='bsd_exchangeratedetail'>\r\n                            <attribute name='bsd_exchangeratedetailid' />\r\n                            <attribute name='bsd_name' />\r\n                            <attribute name='createdon' />\r\n                            <order attribute='bsd_name' descending='false' />\r\n                            <filter type='and'>\r\n                              <condition attribute='bsd_last' operator='eq' value='1' />\r\n                            </filter>\r\n                            <link-entity name='bsd_exchangerate' from='bsd_exchangerateid' to='bsd_exchangerate' alias='ab'>\r\n                              <filter type='and'>\r\n                                <condition attribute='bsd_default' operator='eq' value='1' />\r\n                              </filter>\r\n                            </link-entity>\r\n                          </entity>\r\n                        </fetch>");
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }

        private EntityCollection findUnitsSpec(IOrganizationService crmservices, EntityReference units)
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>\r\n                  <entity name='bsd_unitsspecification'>\r\n                    <attribute name='bsd_unitsspecificationid' />\r\n                    <order attribute='createdon' descending='true' />\r\n                    <link-entity name='bsd_unittype' from='bsd_unittypeid' to='bsd_unittype' alias='ac'>\r\n                      <link-entity name='product' from='bsd_unittype' to='bsd_unittypeid' alias='ad'>\r\n                        <filter type='and'>\r\n                          <condition attribute='productid' operator='eq'  uitype='product' value='{0}' />\r\n                        </filter>\r\n                      </link-entity>\r\n                    </link-entity>\r\n                  </entity>\r\n                </fetch>", (object)units.Id);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }

        private EntityCollection findFirstInstallment(
          IOrganizationService crmservices,
          EntityReference quote)
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>\r\n                  <entity name='bsd_paymentschemedetail'>\r\n                    <attribute name='bsd_paymentschemedetailid' />\r\n                    <attribute name='statuscode' />\r\n                    <filter type='and'>\r\n                      <condition attribute='bsd_ordernumber' operator='eq' value='1' />\r\n                      <condition attribute='bsd_reservation' operator='eq' uiname='D,02.06' uitype='quote' value='{0}' />\r\n                    </filter>\r\n                  </entity>\r\n                </fetch>", (object)quote.Id);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }

        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
        {
            LocalTimeFromUtcTimeRequest fromUtcTimeRequest = new LocalTimeFromUtcTimeRequest()
            {
                TimeZoneCode = (this.RetrieveCurrentUsersSettings(service) ?? throw new Exception("Can't find time zone code")),
                UtcTime = utcTime.ToUniversalTime()
            };
            return ((LocalTimeFromUtcTimeResponse)service.Execute((OrganizationRequest)fromUtcTimeRequest)).LocalTime;
        }

        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            IOrganizationService iorganizationService = service;
            QueryExpression queryExpression1 = new QueryExpression("usersettings");
            queryExpression1.ColumnSet = new ColumnSet(new string[2]
            {
        "localeid",
        "timezonecode"
            });
            QueryExpression queryExpression2 = queryExpression1;
            FilterExpression filterExpression = new FilterExpression();
            ((Collection<ConditionExpression>)filterExpression.Conditions).Add(new ConditionExpression("systemuserid", (ConditionOperator)41));
            queryExpression2.Criteria = filterExpression;
            QueryExpression queryExpression3 = queryExpression1;
            return (int?)((DataCollection<string, object>)((Collection<Entity>)iorganizationService.RetrieveMultiple((QueryBase)queryExpression3).Entities)[0].ToEntity<Entity>().Attributes)["timezonecode"];
        }
    }
}
