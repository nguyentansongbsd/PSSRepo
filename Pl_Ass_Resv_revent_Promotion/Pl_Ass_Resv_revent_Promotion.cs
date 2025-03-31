// 170105 - Quote - Associate - Chan k cho user add Promotion khi Quote chuyen statuscode sag Reservation
// Get schema bsd_quote_bsd_promotion trong entity Quote - get entity target = quote & relateentity = promotion
// disassociate - delete 
// prevent user add of delete handover condition for OE or Reservation without quotation statuscode


using Microsoft.Xrm.Sdk;
using System;
using Microsoft.Xrm.Sdk.Query;

namespace Pl_Ass_Resv_revent_Promotion
{
    public class Pl_Ass_Resv_revent_Promotion : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.MessageName == "Associate" || context.MessageName == "Disassociate")
            {

                Relationship entityRelationship = (Relationship)context.InputParameters["Relationship"];
                factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = factory.CreateOrganizationService(context.UserId);
                ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                EntityReference targetEntity = (EntityReference)context.InputParameters["Target"];
                EntityReferenceCollection relatedEntities = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];

                #region saleorder
                if (targetEntity.LogicalName == "salesorder")
                {
                    Entity en_OE = service.Retrieve(targetEntity.LogicalName, targetEntity.Id,
                           new ColumnSet(new string[] { "name", "statuscode", "bsd_f_lockaddfieldfromresv", "quoteid" }));
                    if (entityRelationship.SchemaName == "bsd_salesorder_bsd_promotion")// check schema name ( N-N relationship)
                    {

                        // disassociate
                        if (context.MessageName == "Disassociate")
                        {
                            throw new InvalidPluginExecutionException("Cannot delete promotion of Option Entry " + (string)en_OE["name"] + "!");
                        }
                        // associate
                        
                        EntityCollection ec_quo_pro = get_ec_quote_pro(service, ((EntityReference)en_OE["quoteid"]).Id);
                        bool f_pro = false;
                       
                        if (ec_quo_pro.Entities.Count > 0)
                        {
                            foreach (Entity en_tmp in ec_quo_pro.Entities)
                            {
                                if (relatedEntities[0].Id.ToString() ==en_tmp["bsd_promotionid"].ToString())
                                {
                                    f_pro = true;
                                    break;

                                }
                            }
                           
                            if (f_pro == false)
                                throw new InvalidPluginExecutionException("Cannot excute this step for Option Entry " + (string)en_OE["name"] + "!");
                        }
                        else throw new InvalidPluginExecutionException("Cannot excute this step for Option Entry " + (string)en_OE["name"] + "!");

                    }
                    if (entityRelationship.SchemaName == "bsd_salesorder_bsd_packageselling")  // check schema name ( N-N relationship))
                    {
                        if (context.MessageName == "Disassociate")
                        {
                            throw new InvalidPluginExecutionException("Cannot delete Handover Condition of Option Entry " + (string)en_OE["name"] + "!");
                        }
                        //associate
                        
                            EntityCollection ec_quo_pack = get_ec_quote_pack(service, ((EntityReference)en_OE["quoteid"]).Id);
                            bool f_pro = false;
                            if (ec_quo_pack.Entities.Count > 0)
                            {
                                foreach (Entity en_tmp in ec_quo_pack.Entities)
                                {
                                    if (relatedEntities[0].Id.ToString() == en_tmp["bsd_packagesellingid"].ToString())
                                    {
                                        f_pro = true;
                                        break;
                                    }

                                }
                                if (f_pro == false)
                                    throw new InvalidPluginExecutionException("Cannot add new Handover Condition for Option Entry " + (string)en_OE["name"] + "!");
                            }
                            else throw new InvalidPluginExecutionException("Cannot add new Handover Condition for Option Entry " + (string)en_OE["name"] + "!");
                        
                    }
                }
                #endregion

                #region ------------ Resv -------------
                if (targetEntity.LogicalName == "quote")
                {
                    Entity en_Quo = service.Retrieve(targetEntity.LogicalName, targetEntity.Id,
                              new ColumnSet(new string[] { "name", "statuscode" }));
                    // relatedEntities = promotion       targetEntity = Quote
                    int statuscodeRe = en_Quo.Contains("statuscode") ? ((OptionSetValue)en_Quo["statuscode"]).Value : 0;
                    if (statuscodeRe != 100000012)//Draft
                    {
                        if (en_Quo.Contains("statuscode") && ((OptionSetValue)en_Quo["statuscode"]).Value != 100000007)
                        {
                            if (entityRelationship.SchemaName == "bsd_quote_bsd_promotion" || entityRelationship.SchemaName == "bsd_quote_bsd_packageselling") // check schema name ( N-N relationship)
                            {
                                throw new InvalidPluginExecutionException("Cannot excute this step for Quotation Reservation " + (string)en_Quo["name"] + " without Quotation status!");
                            }
                        }
                    }
                }

                #endregion

            }

        }

        // promotion
        //private EntityCollection get_ec_pro(IOrganizationService crmservices, Guid saleID)
        //{
        //    string fetchXml =
        //       @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
        //          <entity name='bsd_salesorder_bsd_promotion' >
        //        <attribute name='bsd_promotionid' />
        //        <attribute name='salesorderid' />
        //        <filter type='and' >
        //          <condition attribute='salesorderid' operator='eq' value='{0}' />
        //        </filter>
        //      </entity>
        //    </fetch>";

        //    fetchXml = string.Format(fetchXml, saleID);
        //    EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
        //    return entc;
        //}
        private EntityCollection get_ec_quote_pro(IOrganizationService crmservices, Guid quoteid)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='bsd_quote_bsd_promotion' >
                    <attribute name='bsd_promotionid' />
                    <attribute name='quoteid' />
                    <filter type='and' >
                      <condition attribute='quoteid' operator='eq' value='{0}' />
                    </filter>
                  </entity>
            </fetch>";

            fetchXml = string.Format(fetchXml, quoteid);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        // packedselling
        //private EntityCollection get_ec_packsell(IOrganizationService crmservices, Guid saleID)
        //{
        //    string fetchXml =
        //       @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
        //         <entity name='bsd_salesorder_bsd_packageselling' >
        //        <attribute name='salesorderid' />
        //        <attribute name='bsd_packagesellingid' />
        //        <filter type='and' >
        //          <condition attribute='salesorderid' operator='eq' value='{0}' />
        //        </filter>
        //      </entity>
        //    </fetch>";

        //    fetchXml = string.Format(fetchXml, saleID);
        //    EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
        //    return entc;
        //}
        private EntityCollection get_ec_quote_pack(IOrganizationService crmservices, Guid quoteid)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='bsd_quote_bsd_packageselling' >
                    <attribute name='bsd_packagesellingid' />
                    <attribute name='quoteid' />
                    <filter type='and' >
                      <condition attribute='quoteid' operator='eq' value='{0}' />
                    </filter>
                  </entity>
            </fetch>";

            fetchXml = string.Format(fetchXml, quoteid);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
    }
}
