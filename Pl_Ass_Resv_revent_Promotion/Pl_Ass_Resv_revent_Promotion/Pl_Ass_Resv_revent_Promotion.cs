// Decompiled with JetBrains decompiler
// Type: Pl_Ass_Resv_revent_Promotion.Pl_Ass_Resv_revent_Promotion
// Assembly: Pl_Ass_Resv_revent_Promotion, Version=1.0.0.0, Culture=neutral, PublicKeyToken=2004ca381ff4b6b9
// MVID: DA8E3BA3-66CD-42A4-A348-F908846277BB
// Assembly location: C:\Users\Admin\source\repos\Pl_Ass_Resv_revent_Promotion\Pl_Ass_Resv_revent_Promotion\bin\Debug\Pl_Ass_Resv_revent_Promotion Preview.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;

namespace Pl_Ass_Resv_revent_Promotion
{
    public class Pl_Ass_Resv_revent_Promotion : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (!(service1.MessageName == "Associate") && !(service1.MessageName == "Disassociate"))
                return;
            Relationship inputParameter1 = (Relationship)service1.InputParameters["Relationship"];
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service1.UserId));
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            EntityReference inputParameter2 = (EntityReference)service1.InputParameters["Target"];
            EntityReferenceCollection inputParameter3 = (EntityReferenceCollection)service1.InputParameters["RelatedEntities"];
            if (inputParameter2.LogicalName == "salesorder")
            {
                string relationship = ((Relationship)service1.InputParameters["Relationship"]).SchemaName;
                service2.Trace("relationship: " + relationship);
                EntityReference target = (EntityReference)service1.InputParameters["Target"];
                service2.Trace("target: " + target.LogicalName + " " + target.Id);
                Entity entity1 = this.service.Retrieve(inputParameter2.LogicalName, inputParameter2.Id, new ColumnSet(new string[4]
                {
          "name",
          "statuscode",
          "bsd_f_lockaddfieldfromresv",
          "quoteid"
                }));
                if (inputParameter1.SchemaName == "bsd_salesorder_bsd_promotion")
                {
                    if (service1.MessageName == "Disassociate")
                        throw new InvalidPluginExecutionException("Cannot delete promotion of Option Entry " + (string)entity1["name"] + "!");
                    EntityCollection ecQuotePro = this.get_ec_quote_pro(this.service, ((EntityReference)entity1["quoteid"]).Id);
                    bool flag = false;
                    if (ecQuotePro.Entities.Count <= 0)
                        throw new InvalidPluginExecutionException("Cannot excute this step for Option Entry " + (string)entity1["name"] + "!");
                    foreach (Entity entity2 in (Collection<Entity>)ecQuotePro.Entities)
                    {
                        if (inputParameter3[0].Id.ToString() == entity2["bsd_promotionid"].ToString())
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                        throw new InvalidPluginExecutionException("Cannot excute this step for Option Entry " + (string)entity1["name"] + "!");
                }
                if (inputParameter1.SchemaName == "bsd_salesorder_bsd_packageselling")
                {
                    //if (service1.MessageName == "Disassociate")
                    //    throw new InvalidPluginExecutionException("Cannot delete Handover Condition of Option Entry " + (string)entity1["name"] + "!");
                    //EntityCollection ecQuotePack = this.get_ec_quote_pack(this.service, ((EntityReference)entity1["quoteid"]).Id);
                    //bool flag = false;
                    //if (ecQuotePack.Entities.Count <= 0)
                    //    throw new InvalidPluginExecutionException("Cannot add new Handover Condition for Option Entry " + (string)entity1["name"] + "!");
                    //foreach (Entity entity3 in (Collection<Entity>)ecQuotePack.Entities)
                    //{
                    //    if (inputParameter3[0].Id.ToString() == entity3["bsd_packagesellingid"].ToString())
                    //    {
                    //        flag = true;
                    //        break;
                    //    }
                    //}
                    //if (!flag)
                    //    throw new InvalidPluginExecutionException("Cannot add new Handover Condition for Option Entry " + (string)entity1["name"] + "!");
                }
            }
            if (inputParameter2.LogicalName == "quote")
            {
                Entity entity = this.service.Retrieve(inputParameter2.LogicalName, inputParameter2.Id, new ColumnSet(new string[2]
                {
          "name",
          "statuscode"
                }));
                if (entity.Contains("statuscode") && ((OptionSetValue)entity["statuscode"]).Value != 100000007 && (inputParameter1.SchemaName == "bsd_quote_bsd_promotion" || inputParameter1.SchemaName == "bsd_quote_bsd_packageselling"))
                    throw new InvalidPluginExecutionException("Cannot excute this step for Quotation Reservation " + (string)entity["name"] + " without Quotation status!");
            }
        }

        private EntityCollection get_ec_quote_pro(IOrganizationService crmservices, Guid quoteid)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\r\n                  <entity name='bsd_quote_bsd_promotion' >\r\n                    <attribute name='bsd_promotionid' />\r\n                    <attribute name='quoteid' />\r\n                    <filter type='and' >\r\n                      <condition attribute='quoteid' operator='eq' value='{0}' />\r\n                    </filter>\r\n                  </entity>\r\n            </fetch>", (object)quoteid);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private EntityCollection get_ec_quote_pack(IOrganizationService crmservices, Guid quoteid)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\r\n                  <entity name='bsd_quote_bsd_packageselling' >\r\n                    <attribute name='bsd_packagesellingid' />\r\n                    <attribute name='quoteid' />\r\n                    <filter type='and' >\r\n                      <condition attribute='quoteid' operator='eq' value='{0}' />\r\n                    </filter>\r\n                  </entity>\r\n            </fetch>", (object)quoteid);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }
    }
}