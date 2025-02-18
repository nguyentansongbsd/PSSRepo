//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Client;
//using Microsoft.Xrm.Sdk.Messages;
//using Microsoft.Xrm.Sdk.Query;
//using Microsoft.Crm.Sdk;
//using Microsoft.Crm.Sdk.Messages;
//namespace Action_HandoverNotices_GenerateHandoverNotices
//{
//    public class Action_HandoverNotices_Generate : IPlugin
//    {
//        IOrganizationService service = null;
//        IOrganizationServiceFactory factory = null;
//        void IPlugin.Execute(IServiceProvider serviceProvider)
//        {
//            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
//            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
//            service = factory.CreateOrganizationService(context.UserId);
//            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

//            traceService.Trace(string.Format("Context Depth {0}", context.Depth));
//            int dem = 0;
//            EntityCollection l_OE = findOptionEntry(service);
//            foreach (Entity OE in l_OE.Entities)
//            {
//                EntityCollection l_Units = findUnits(service, OE);
//                foreach (Entity units in l_Units.Entities)
//                {
//                    if (((DateTime)units["bsd_estimatehandoverdate"]).Date >= DateTime.Now.Date && (decimal)OE["bsd_testpercent"] >= (decimal)units["bsd_handovercondition"])
//                    {
//                        EntityCollection l_HN = findHandoverNotices(service, units);
//                        if (l_HN.Entities.Count == 0)
//                        {
//                            Entity handover = new Entity("bsd_handovernotice");
//                            if (OE.Contains("name"))
//                                handover["bsd_name"] = "Handover Notices of " + (string)OE["name"];
//                            else
//                                handover["bsd_name"] = "Handover Notices";
//                            handover["bsd_subject"] = "Handover Notices";
//                            handover["bsd_customer"] = OE["customerid"];
//                            handover["bsd_date"] = DateTime.Today;
//                            handover["bsd_optionentry"] = OE.ToEntityReference();
//                            handover["bsd_units"] = units.ToEntityReference();
//                            //throw new InvalidPluginExecutionException(OE.Id.ToString() + " " + units.Id.ToString());
//                            service.Create(handover);

//                            dem++;
//                        }
//                    }
//                }
//            }

//            context.OutputParameters["ReturnId"] = dem.ToString();
//        }
//        private EntityCollection findHandoverNotices(IOrganizationService crmservices, Entity unit)
//        {
//            string fetchXml =
//                  @"<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>
//                      <entity name='bsd_handovernotice'>
//                        <attribute name='bsd_handovernoticeid' />
//                        <filter type='and'>
//                          <condition attribute='bsd_units' operator='eq'  uitype='product' value='{0}' />
//                        </filter>
//                      </entity>
//                    </fetch>";
//            fetchXml = string.Format(fetchXml, unit.Id);
//            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
//            return entc;
//        }
//        private EntityCollection findUnits(IOrganizationService crmservices, Entity opEn)
//        {
//            string fetchXml =
//                  @"<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true'>
//                  <entity name='product'>
//                    <attribute name='productid' />
//                    <attribute name='bsd_estimatehandoverdate' />
//                    <attribute name='bsd_handovercondition' />
//                    <order attribute='bsd_estimatehandoverdate' descending='false' />
//                    <filter type='and'>
//                      <condition attribute='bsd_handovercondition' operator='not-null' />
//                      <condition attribute='bsd_estimatehandoverdate' operator='not-null' />
//                    </filter>
//                    <link-entity name='salesorderdetail' from='productid' to='productid' alias='ac'>
//                      <filter type='and'>
//                        <condition attribute='salesorderid' operator='eq' uitype='salesorder' value='{0}' />
//                      </filter>
//                    </link-entity>
//                  </entity>
//                </fetch>";
//            fetchXml = string.Format(fetchXml, opEn.Id);
//            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
//            return entc;
//        }
//        private EntityCollection findOptionEntry(IOrganizationService crmservices)
//        {
//            string fetchXml =
//              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
//              <entity name='salesorder'>
//                <attribute name='name' />
//                <attribute name='customerid' />
//                <attribute name='salesorderid' />
//                <attribute name='bsd_testpercent' />
//                <filter type='and'>
//                  <condition attribute='bsd_totalamountpaid' operator='not-null' />
//                  <condition attribute='statuscode' operator='in'>
//                    <value>100000001</value>
//                    <value>100000003</value>
//                    <value>100000004</value>
//                    <value>100000002</value>
//                  </condition>
//                  <condition attribute='customerid' operator='not-null' />
//                  <condition attribute='totalamount' operator='not-null' />
//                  <condition attribute='bsd_testpercent' operator='not-null' />
//                </filter>
//              </entity>
//            </fetch>";
//            fetchXml = string.Format(fetchXml);
//            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
//            return entc;
//        }
//    }
//}

