using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Net;
using System.Text;

namespace Plugin_BulkChangeManaFeeDetail_Create_Update
{
    public class Plugin_BulkChangeManaFeeDetail_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceServiceClass = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceServiceClass = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            //traceServiceClass.Trace("Plugin_BulkChangeManaFeeDetail_Create_Update");
            //traceServiceClass.Trace("context.context " + context.Depth);
            if (context.MessageName == "Create" && context.Depth <= 2)
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                Entity enUp = new Entity(target.LogicalName, target.Id);
                int bsd_numberofmonthspaidmfnew = enTarget.Contains("bsd_numberofmonthspaidmfnew") ? (int)enTarget["bsd_numberofmonthspaidmfnew"] : 0;
                decimal bsd_managementamountmonth_new = enTarget.Contains("bsd_managementamountmonth_new") ? ((Money)enTarget["bsd_managementamountmonth_new"]).Value : 0;
                bool checkUp = false;
                if (enTarget.Contains("bsd_optionentry"))
                {
                    var fetchXml2 = $@"
                        <fetch>
                          <entity name='salesorder'>
                            <attribute name=""bsd_managementfee"" />
                            <attribute name=""bsd_numberofmonthspaidmf"" />
                            <attribute name=""bsd_unitnumber"" />
                            <attribute name=""bsd_project"" />
                            <filter>
                              <condition attribute='salesorderid' operator='eq' value='{((EntityReference)enTarget["bsd_optionentry"]).Id}'/>
                            </filter>
                          </entity>
                        </fetch>";
                    EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    foreach (Entity entity in list2.Entities)
                    {
                        enUp["bsd_project"] = (EntityReference)entity["bsd_project"];
                        enUp["bsd_numberofmonthspaidmfcurrent"] = entity.Contains("bsd_numberofmonthspaidmf") ? (int)entity["bsd_numberofmonthspaidmf"] : 0;
                        enUp["bsd_managementfeecurrent"] = entity.Contains("bsd_managementfee") ? (Money)entity["bsd_managementfee"] : new Money(0);
                        if (!enTarget.Contains("bsd_units"))
                        {
                            var fetchXml21 = $@"
                            <fetch>
                              <entity name='product'>
                                <attribute name=""bsd_managementamountmonth"" />
                                <attribute name=""bsd_netsaleablearea"" />
                                <attribute name=""productid"" />
                                <filter>
                                  <condition attribute='productid' operator='eq' value='{((EntityReference)entity["bsd_unitnumber"]).Id}'/>
                                </filter>
                              </entity>
                            </fetch>";
                            EntityCollection list21 = service.RetrieveMultiple(new FetchExpression(fetchXml21));
                            foreach (Entity entity1 in list21.Entities)
                            {
                                decimal bsd_netsaleablearea = entity1.Contains("bsd_netsaleablearea") ? (decimal)entity1["bsd_netsaleablearea"] : 0;
                                enUp["bsd_managementfeenew"] = bsd_netsaleablearea * bsd_numberofmonthspaidmfnew * bsd_managementamountmonth_new;
                                enUp["bsd_units"] = entity1.ToEntityReference();
                                enUp["bsd_managementamountmonth_current"] = entity1.Contains("bsd_managementamountmonth") ? (Money)entity1["bsd_managementamountmonth"] : new Money(0);
                            }
                        }
                        var fetchXml212 = $@"
                            <fetch>
                              <entity name='bsd_paymentschemedetail'>
                                <attribute name=""bsd_paymentschemedetailid"" />
                                <filter>
                                  <condition attribute='bsd_optionentry' operator='eq' value='{((EntityReference)enTarget["bsd_optionentry"]).Id}'/>
                                  <condition attribute='bsd_managementfee' operator='eq' value='1'/>
                                </filter>
                              </entity>
                            </fetch>";
                        EntityCollection list212 = service.RetrieveMultiple(new FetchExpression(fetchXml212));
                        foreach (Entity entity12 in list212.Entities)
                        {
                            enUp["bsd_installment"] = entity12.ToEntityReference();
                        }
                        checkUp = true;
                    }
                }
                else if (enTarget.Contains("bsd_reservation"))
                {
                    var fetchXml2 = $@"
                        <fetch>
                          <entity name='quote'>
                            <attribute name=""bsd_managementfee"" />
                            <attribute name=""bsd_numberofmonthspaidmf"" />
                            <attribute name=""bsd_unitno"" />
                            <attribute name=""bsd_projectid"" />
                            <filter>
                              <condition attribute='quoteid' operator='eq' value='{((EntityReference)enTarget["bsd_reservation"]).Id}'/>
                            </filter>
                          </entity>
                        </fetch>";
                    EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    foreach (Entity entity in list2.Entities)
                    {
                        enUp["bsd_project"] = (EntityReference)entity["bsd_projectid"];
                        enUp["bsd_numberofmonthspaidmfcurrent"] = entity.Contains("bsd_numberofmonthspaidmf") ? (int)entity["bsd_numberofmonthspaidmf"] : 0;
                        enUp["bsd_managementfeecurrent"] = entity.Contains("bsd_managementfee") ? (Money)entity["bsd_managementfee"] : new Money(0);
                        if (!enTarget.Contains("bsd_units"))
                        {
                            var fetchXml21 = $@"
                            <fetch>
                              <entity name='product'>
                                <attribute name=""bsd_managementamountmonth"" />
                                <attribute name=""bsd_netsaleablearea"" />
                                <attribute name=""productid"" />
                                <filter>
                                  <condition attribute='productid' operator='eq' value='{((EntityReference)entity["bsd_unitno"]).Id}'/>
                                </filter>
                              </entity>
                            </fetch>";
                            EntityCollection list21 = service.RetrieveMultiple(new FetchExpression(fetchXml21));
                            foreach (Entity entity1 in list21.Entities)
                            {
                                decimal bsd_netsaleablearea = entity1.Contains("bsd_netsaleablearea") ? (decimal)entity1["bsd_netsaleablearea"] : 0;
                                enUp["bsd_managementfeenew"] = bsd_netsaleablearea * bsd_numberofmonthspaidmfnew * bsd_managementamountmonth_new;
                                enUp["bsd_units"] = entity1.ToEntityReference();
                                enUp["bsd_managementamountmonth_current"] = entity1.Contains("bsd_managementamountmonth") ? (Money)entity1["bsd_managementamountmonth"] : new Money(0);
                            }
                        }
                        var fetchXml212 = $@"
                            <fetch>
                              <entity name='bsd_paymentschemedetail'>
                                <attribute name=""bsd_paymentschemedetailid"" />
                                <filter>
                                  <condition attribute='bsd_reservation' operator='eq' value='{((EntityReference)enTarget["bsd_reservation"]).Id}'/>
                                  <condition attribute='bsd_managementfee' operator='eq' value='1'/>
                                </filter>
                              </entity>
                            </fetch>";
                        EntityCollection list212 = service.RetrieveMultiple(new FetchExpression(fetchXml212));
                        foreach (Entity entity12 in list212.Entities)
                        {
                            enUp["bsd_installment"] = entity12.ToEntityReference();
                        }
                        checkUp = true;
                    }
                }
                if (enTarget.Contains("bsd_units"))
                {
                    var fetchXml2 = $@"
                        <fetch>
                          <entity name='product'>
                            <attribute name=""bsd_managementamountmonth"" />
                            <attribute name=""bsd_netsaleablearea"" />
                            <attribute name=""productid"" />
                            <filter>
                              <condition attribute='productid' operator='eq' value='{((EntityReference)enTarget["bsd_units"]).Id}'/>
                            </filter>
                          </entity>
                        </fetch>";
                    EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    foreach (Entity entity in list2.Entities)
                    {
                        decimal bsd_netsaleablearea = entity.Contains("bsd_netsaleablearea") ? (decimal)entity["bsd_netsaleablearea"] : 0;
                        enUp["bsd_managementfeenew"] = bsd_netsaleablearea * bsd_numberofmonthspaidmfnew * bsd_managementamountmonth_new;
                        enUp["bsd_managementamountmonth_current"] = entity.Contains("bsd_managementamountmonth") ? (Money)entity["bsd_managementamountmonth"] : new Money(0);
                        checkUp = true;
                    }
                }
                if (checkUp) service.Update(enUp);
            }
        }
    }
}