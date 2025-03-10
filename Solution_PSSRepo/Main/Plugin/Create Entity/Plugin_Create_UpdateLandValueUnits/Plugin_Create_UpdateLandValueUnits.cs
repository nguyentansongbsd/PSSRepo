using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Create_UpdateLandValueUnits
{
    public class Plugin_Create_UpdateLandValueUnitsIPluginExecutionContext : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //get entity
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            if (((OptionSetValue)enCreated["bsd_type"]).Value == 100000000)
            {
                if (enCreated.Contains("bsd_units"))
                {
                    EntityReference enUnitRef = (EntityReference)enCreated["bsd_units"];
                    var query_statuscode_1 = 100000001;
                    var query_statuscode_2 = 100000002;
                    var query_statuscode_3 = 100000003;
                    var query_statuscode_4 = 100000004;
                    var query_statuscode_5 = 100000005;
                    var query_statuscode_6 = 100000000;
                    var query = new QueryExpression("salesorder");
                    query.Criteria.AddCondition("bsd_unitnumber", ConditionOperator.Equal, enUnitRef.Id.ToString());
                    query.Criteria.AddCondition("statuscode", ConditionOperator.In, query_statuscode_1, query_statuscode_2, query_statuscode_3, query_statuscode_4, query_statuscode_5, query_statuscode_6);

                    var rs = service.RetrieveMultiple(query);
                    if (rs.Entities.Count > 0)
                    {
                        Entity enUpdate = new Entity(enCreated.LogicalName, enCreated.Id);
                        enUpdate["bsd_optionentry"] = new EntityReference(rs.Entities[0].LogicalName, rs.Entities[0].Id);
                        #region map  value
                        if (enCreated.Contains("bsd_type") && ((OptionSetValue)enCreated["bsd_type"]).Value == 100000000 && enUpdate.Contains("bsd_optionentry"))
                        {
                            Entity entity1 = this.service.Retrieve(((EntityReference)enUpdate["bsd_optionentry"]).LogicalName, ((EntityReference)enUpdate["bsd_optionentry"]).Id, new ColumnSet(true));
                            if ((entity1.Contains("statuscode") ? ((OptionSetValue)entity1["statuscode"]).Value : 0) == 100000006)
                                throw new InvalidPluginExecutionException("Option Entry is Terminated. Please check again.");
                            Money money1 = entity1.Contains("bsd_detailamount") ? (Money)entity1["bsd_detailamount"] : new Money(0M);
                            Money money2 = entity1.Contains("bsd_discount") ? (Money)entity1["bsd_discount"] : new Money(0M);
                            Money money3 = entity1.Contains("bsd_packagesellingamount") ? (Money)entity1["bsd_packagesellingamount"] : new Money(0M);
                            Money money4 = entity1.Contains("bsd_totalamountlessfreight") ? (Money)entity1["bsd_totalamountlessfreight"] : new Money(0M);
                            Money money5 = entity1.Contains("bsd_landvaluededuction") ? (Money)entity1["bsd_landvaluededuction"] : new Money(0M);
                            Money money6 = entity1.Contains("totaltax") ? (Money)entity1["totaltax"] : new Money(0M);
                            Money money7 = entity1.Contains("bsd_freightamount") ? (Money)entity1["bsd_freightamount"] : new Money(0M);
                            Money money8 = entity1.Contains("totalamount") ? (Money)entity1["totalamount"] : new Money(0M);
                            enUpdate["bsd_listedpricecurrent"] = (object)money1;
                            enUpdate["bsd_discountcurrent"] = (object)money2;
                            enUpdate["bsd_handoverconditionamountcurrent"] = (object)money3;
                            enUpdate["bsd_netsellingpricecurrent"] = (object)money4;
                            enUpdate["bsd_landvaluedeductioncurrent"] = (object)money5;
                            enUpdate["bsd_totalvattaxcurrent"] = (object)money6;
                            enUpdate["bsd_maintenancefeecurrent"] = (object)money7;
                            enUpdate["bsd_totalamount"] = (object)money8;
                            enUpdate["bsd_listedpricenew"] = (object)money1;
                            enUpdate["bsd_discountnew"] = (object)money2;
                            enUpdate["bsd_handoverconditionamountnew"] = (object)money3;
                            enUpdate["bsd_netsellingpricenew"] = (object)money4;
                            EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("\r\n                                            <fetch>\r\n                                              <entity name='bsd_paymentschemedetail'>\r\n                                                <filter>\r\n                                                  <condition attribute='bsd_optionentry' operator='eq' value='{0}'/>\r\n                                                  <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002'/>\r\n                                                </filter>\r\n                                              </entity>\r\n                                            </fetch>", (object)entity1.Id)));
                            if (entityCollection.Entities.Count > 0)
                            {
                                Entity entity2 = entityCollection.Entities[0];
                                enUpdate["bsd_installment"] = (object)entity2.ToEntityReference();
                                Money money9 = entity2.Contains("bsd_amountofthisphase") ? (Money)entity2["bsd_amountofthisphase"] : new Money(0M);
                                enUpdate["bsd_amountofthisphasecurrent"] = (object)money9;
                            }



                            Entity entity22 = this.service.Retrieve(((EntityReference)enCreated["bsd_units"]).LogicalName, ((EntityReference)enCreated["bsd_units"]).Id, new ColumnSet(true));
                            Entity entity3 = this.service.Retrieve(((EntityReference)enUpdate["bsd_optionentry"]).LogicalName, ((EntityReference)enUpdate["bsd_optionentry"]).Id, new ColumnSet(true));
                            Decimal num2 = entity3.Contains("bsd_freightamount") ? ((Money)entity3["bsd_freightamount"]).Value : 0M;
                            Decimal num3 = enCreated.Contains("bsd_landvaluenew") ? ((Money)enCreated["bsd_landvaluenew"]).Value : 0M;
                            Decimal num4 = enUpdate.Contains("bsd_netsellingpricenew") ? ((Money)enUpdate["bsd_netsellingpricenew"]).Value : 0M;
                            Decimal num5 = enUpdate.Contains("bsd_totalamount") ? ((Money)enUpdate["bsd_totalamount"]).Value : 0M;
                            Decimal num6 = enCreated.Contains("bsd_maintenancefeenew") ? ((Money)enCreated["bsd_maintenancefeenew"]).Value : 0M;
                            Decimal num7 = enUpdate.Contains("bsd_amountofthisphasecurrent") ? ((Money)enUpdate["bsd_amountofthisphasecurrent"]).Value : 0M;
                            Decimal num8 = entity22.Contains("bsd_netsaleablearea") ? (Decimal)entity22["bsd_netsaleablearea"] : 0M;
                            Decimal num9 = num3 * num8;
                            Decimal num10 = (num4 - num9) * 0.1M;
                            Decimal num11 = num4 + num10 + num2;
                            Decimal num12 = Math.Round(num5 - num11, 0);
                            enUpdate["bsd_maintenancefeenew"] = (object)new Money(num2);
                            enUpdate["bsd_landvaluedeductionnew"] = (object)new Money(num9);
                            enUpdate["bsd_totalvattaxnew"] = (object)new Money(num10);
                            enUpdate["bsd_valuedifference"] = (object)new Money(num12);
                            enUpdate["bsd_amountofthisphase"] = (object)new Money(Math.Round(num7 - num12, 0));

                        }
                        #endregion
                        service.Update(enUpdate);
                    }
                    else
                    {
                        throw new Exception("Unit don't have option entry. Please check again.");
                    }    
                }
            }
        }
    }
}
