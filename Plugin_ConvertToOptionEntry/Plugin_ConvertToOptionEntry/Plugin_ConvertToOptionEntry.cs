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
        private ITracingService tracingService;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            Entity inputParameter = ((DataCollection<string, object>)((IExecutionContext)service).InputParameters)["Target"] as Entity;
            if (!(inputParameter.LogicalName == "salesorderdetail") || !(((IExecutionContext)service).MessageName == "Create") || !inputParameter.Contains("salesorderid"))
                return;
            this.service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(new Guid?(((IExecutionContext)service).UserId));
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            EntityReference enfOE = (EntityReference)inputParameter["salesorderid"];
            Entity enOE = this.service.Retrieve(enfOE.LogicalName, enfOE.Id, new ColumnSet(new string[]
            {
                "quoteid", "bsd_unittype"
            }));
            if (!enOE.Contains("quoteid"))
                return;
            EntityReference quote = (EntityReference)enOE["quoteid"];
            Entity enQuote = this.service.Retrieve(quote.LogicalName, quote.Id, new ColumnSet(new string[14]
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
            if (!enQuote.Contains("bsd_rfsigneddate"))
                throw new InvalidPluginExecutionException("Reservation had not signed. Can not execute this action!");
            Entity enOENew = new Entity(enfOE.LogicalName, enfOE.Id);
            enOENew["bsd_unitnumber"] = enQuote.Contains("bsd_unitno") ? enQuote["bsd_unitno"] : (object)null;
            enOENew["bsd_unitstatus"] = enQuote.Contains("bsd_unitstatus") ? enQuote["bsd_unitstatus"] : (object)null;
            enOENew["bsd_constructionarea"] = enQuote.Contains("bsd_constructionarea") ? enQuote["bsd_constructionarea"] : (object)null;
            enOENew["bsd_netusablearea"] = enQuote.Contains("bsd_netusablearea") ? enQuote["bsd_netusablearea"] : (object)null;
            enOENew["bsd_salesagentcompany"] = enQuote.Contains("bsd_salessgentcompany") ? enQuote["bsd_salessgentcompany"] : (object)null;
            enOENew["bsd_queuingfee"] = enQuote.Contains("bsd_bookingfee") ? enQuote["bsd_bookingfee"] : (object)null;
            enOENew["bsd_depositamount"] = enQuote.Contains("bsd_depositfee") ? enQuote["bsd_depositfee"] : (object)null;
            enOENew["bsd_contractdate"] = (object)this.RetrieveLocalTimeFromUTCTime(DateTime.Now, this.service);
            enOENew["bsd_actualarea"] = enQuote.Contains("bsd_actualarea") ? enQuote["bsd_actualarea"] : (object)null;
            enOENew["bsd_nameofstaffagent"] = enQuote.Contains("bsd_nameofstaffagent") ? enQuote["bsd_nameofstaffagent"] : (object)null;
            enOENew["bsd_taxcode"] = enQuote.Contains("bsd_taxcode") ? enQuote["bsd_taxcode"] : (object)null;
            enOENew["bsd_numberofmonthspaidmf"] = enQuote.Contains("bsd_numberofmonthspaidmf") ? enQuote["bsd_numberofmonthspaidmf"] : (object)null;
            enOENew["bsd_managementfee"] = enQuote.Contains("bsd_managementfee") ? enQuote["bsd_managementfee"] : (object)null;
            enOENew["bsd_totalamountpaid"] = enQuote.Contains("bsd_totalamountpaid") ? enQuote["bsd_totalamountpaid"] : (object)null;
            if (enQuote.Contains("bsd_unitno"))
            {
                //EntityCollection unitsSpec = this.findUnitsSpec(this.service, (EntityReference)entity2["bsd_unitno"]);
                //if (((Collection<Entity>)unitsSpec.Entities).Count > 0)
                //    entity3["bsd_unitsspecification"] = (object)((Collection<Entity>)unitsSpec.Entities)[0].ToEntityReference();

                Entity entity4 = this.service.Retrieve(((EntityReference)enQuote["bsd_unitno"]).LogicalName, ((EntityReference)enQuote["bsd_unitno"]).Id, new ColumnSet(new string[1]
                {
          "bsd_estimatehandoverdate"
                }));
                if (entity4.Contains("bsd_estimatehandoverdate"))
                    enOENew["bsd_estimatehandoverdatecontract"] = entity4["bsd_estimatehandoverdate"];
            }
            foreach (Entity entity5 in (Collection<Entity>)this.findFirstInstallment(this.service, quote).Entities)
            {
                if (entity5.Contains("statuscode") && ((OptionSetValue)entity5["statuscode"]).Value == 100000001)
                    enOENew["statuscode"] = (object)new OptionSetValue(100000001);
            }
            foreach (Entity entity6 in (Collection<Entity>)this.findExchangeRate(this.service).Entities)
                enOENew["bsd_applyingexchangerate"] = (object)entity6.ToEntityReference();
            this.service.Update(enOENew);
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
                this.service.Associate(enfOE.LogicalName, enfOE.Id, new Relationship("bsd_salesorder_bsd_promotion"), referenceCollection);
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
                    ["bsd_optionentry"] = (object)enfOE
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