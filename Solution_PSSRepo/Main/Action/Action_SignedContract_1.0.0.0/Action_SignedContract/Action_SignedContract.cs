// Decompiled with JetBrains decompiler
// Type: Action_SignedContract.Action_SignedContract
// Assembly: Action_SignedContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=91af1975bd46f505
// MVID: 64A057F8-04D7-4937-A84E-D4EF3DDC89DB
// Assembly location: C:\Users\ngoct\Downloads\Action_SignedContract_1.0.0.0.dll

using BSDLibrary;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Action_SignedContract
{
    public class Action_SignedContract : IPlugin
    {
        private IPluginExecutionContext context;
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        public ITracingService traceService = (ITracingService)null;
        private ParameterCollection target = (ParameterCollection)null;
        public Entity enBulkWaiver;
        private Common common;
        private IServiceProvider serviceProvider;

        public void Execute(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
            this.traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            this.target = this.context.InputParameters;
            EntityReference entityReference = this.target["Target"] as EntityReference;
            this.common = new Common(this.service);
            Entity enOptionEntry = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(true));
            int num1 = ((OptionSetValue)enOptionEntry["statuscode"]).Value;
            bool flag = new OptionEntry(serviceProvider, enOptionEntry).checkShortFallAmount(enOptionEntry);
            Guid id = ((EntityReference)enOptionEntry["bsd_paymentscheme"]).Id;
            if (num1 == 100000001 | flag)
            {
                if (!enOptionEntry.Contains("customerid"))
                    throw new InvalidPluginExecutionException("Contract does not contain Purchaser!");
                if (!enOptionEntry.Contains("ordernumber"))
                    throw new InvalidPluginExecutionException("Contract does not contain 'Option Number'!");
                if (!enOptionEntry.Contains("bsd_project"))
                    throw new InvalidPluginExecutionException("Contract does not contain 'Project'!");
                if (!enOptionEntry.Contains("bsd_contractprinteddate"))
                    throw new InvalidPluginExecutionException("Option Entry must be printed before signing!");
                DateTime dateTime1 = this.common.RetrieveLocalTimeFromUTCTime(DateTime.Now);
                this.service.Update(new Entity(enOptionEntry.LogicalName)
                {
                    Id = enOptionEntry.Id,
                    ["statuscode"] = (object)new OptionSetValue(100000002)
                });
                int num2 = enOptionEntry.Contains("bsd_numberofmonthspaidmf") ? (int)enOptionEntry["bsd_numberofmonthspaidmf"] : 0;
                if (!enOptionEntry.Contains("bsd_project"))
                    throw new InvalidPluginExecutionException("Cannot find project information on Option Entry: " + (string)enOptionEntry["name"] + "!");
                Entity entity1 = this.service.Retrieve(((EntityReference)enOptionEntry["bsd_unitnumber"]).LogicalName, ((EntityReference)enOptionEntry["bsd_unitnumber"]).Id, new ColumnSet(new string[2]
                {
          "bsd_numberofmonthspaidmf",
          "bsd_managementamountmonth"
                }));
                Entity entity2 = this.service.Retrieve(((EntityReference)enOptionEntry["bsd_project"]).LogicalName, ((EntityReference)enOptionEntry["bsd_project"]).Id, new ColumnSet(new string[2]
                {
          "bsd_name",
          "bsd_managementamount"
                }));
                Decimal num3 = !entity1.Contains("bsd_managementamountmonth") ? (entity2.Contains("bsd_managementamount") ? ((Money)entity2["bsd_managementamount"]).Value : 0M) : (entity1.Contains("bsd_managementamountmonth") ? ((Money)entity1["bsd_managementamountmonth"]).Value : 0M);
                DateTime dateTime2 = this.common.RetrieveLocalTimeFromUTCTime(enOptionEntry.Contains("bsd_signedcontractdate") ? (DateTime)enOptionEntry["bsd_signedcontractdate"] : dateTime1);
                Entity entity3 = this.service.Retrieve(((EntityReference)enOptionEntry["bsd_unitnumber"]).LogicalName, ((EntityReference)enOptionEntry["bsd_unitnumber"]).Id, new ColumnSet(new string[6]
                {
          "name",
          "statuscode",
          "bsd_signedcontractdate",
          "bsd_actualarea",
          "bsd_netsaleablearea",
          "bsd_optionnumber"
                }));
                Decimal num4 = !entity3.Contains("bsd_actualarea") ? (entity3.Contains("bsd_netsaleablearea") ? (Decimal)entity3["bsd_netsaleablearea"] : 0M) : (Decimal)entity3["bsd_actualarea"];
                Entity entity4 = this.service.Retrieve(((EntityReference)enOptionEntry["customerid"]).LogicalName, ((EntityReference)enOptionEntry["customerid"]).Id, new ColumnSet(new string[1]
                {
          "bsd_totaltransaction"
                }));
                int num5 = (entity4.Contains("bsd_totaltransaction") ? (int)entity4["bsd_totaltransaction"] : 0) + 1;
                this.service.Update(new Entity(entity3.LogicalName)
                {
                    Id = entity3.Id,
                    ["statuscode"] = (object)new OptionSetValue(100000002),
                    ["bsd_signedcontractdate"] = (object)dateTime2,
                    ["bsd_optionnumber"] = enOptionEntry["bsd_optionno"]
                });
                this.service.Update(new Entity(entity4.LogicalName)
                {
                    Id = entity4.Id,
                    ["bsd_totaltransaction"] = (object)num5
                });
                EntityCollection instFee = this.get_Inst_Fee(this.service, enOptionEntry.Id, id);
                if (instFee.Entities.Count > 0)
                {
                    Entity entity5 = instFee.Entities[0];
                    entity5.Id = instFee.Entities[0].Id;
                    Decimal num6 = instFee.Entities[0].Contains("bsd_managementamount") ? ((Money)instFee.Entities[0]["bsd_managementamount"]).Value : 0M;
                    Decimal num7 = num3 * (Decimal)num2 * num4;
                    Decimal num8 = num7 * 10M / 100M;
                    Decimal num9 = num7 + num8;
                    this.service.Update(new Entity(entity5.LogicalName)
                    {
                        Id = entity5.Id,
                        ["bsd_managementamount"] = (object)new Money(num9)
                    });
                }
            }
            #region  Qua lại TĐTT kiểm tra đợt nào có sts = Paid => Update field Due Date wordtemplate trong đợt = null

            var query_bsd_optionentry = enOptionEntry.Id.ToString();

            var query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, query_bsd_optionentry);
            var rs=service.RetrieveMultiple(query);
            if(rs.Entities.Count > 0)
            {
                foreach(var item in  rs.Entities)
                {
                    if (((OptionSetValue)item["statuscode"]).Value== 100000001)
                    {
                        var itemEnUpdate = new Entity(item.LogicalName, item.Id);
                        itemEnUpdate["bsd_duedatewordtemplate"] = null;
                        service.Update(itemEnUpdate);
                    }    
                }    
            }    
            #endregion
            this.context.OutputParameters["output"] = (object)"done";
        }

        private EntityCollection get_Inst_Fee(IOrganizationService crmservices, Guid oeID, Guid pmsID)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\r\n                  <entity name='bsd_paymentschemedetail' >\r\n                    <attribute name='bsd_duedate' />\r\n                    <attribute name='bsd_name' />\r\n                    <attribute name='bsd_duedatecalculatingmethod' />\r\n                    <attribute name='bsd_maintenanceamount' />\r\n                    <attribute name='bsd_maintenancefees' />\r\n                    <attribute name='bsd_managementfee' />\r\n                    <attribute name='bsd_amountofthisphase' />\r\n                    <attribute name='bsd_managementfeesstatus' />\r\n                    <attribute name='bsd_managementamount' />\r\n                    <attribute name='bsd_maintenancefeesstatus' />\r\n                    <attribute name='bsd_paymentschemedetailid' />\r\n                    <filter type='and' >\r\n                      <condition attribute='bsd_optionentry' operator='eq' value='{0}' />\r\n                      <condition attribute='bsd_managementamount' operator='gt' value='0' />\r\n                      <condition attribute='statecode' operator='eq' value='0' />\r\n                    </filter>\r\n                  </entity>\r\n                </fetch>", (object)oeID, (object)pmsID);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }
    }
}
