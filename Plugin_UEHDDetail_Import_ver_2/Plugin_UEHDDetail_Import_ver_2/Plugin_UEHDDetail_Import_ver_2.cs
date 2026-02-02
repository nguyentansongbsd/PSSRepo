// Decompiled with JetBrains decompiler
// Type: Plugin_UEHDDetail_Import.Plugin_UEHDDetail_Import
// Assembly: Plugin_UEHDDetail_Import, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5de1e6efa028ef71
// MVID: 4BAF723D-6137-446F-9CF9-D039ADD17355
// Assembly location: C:\Users\BSD\Desktop\Plugin_UEHDDetail_Import.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;

namespace Plugin_UEHDDetail_Import_ver_2
{
    public class Plugin_UEHDDetail_Import_ver_2 : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        public void Execute(IServiceProvider serviceProvider)
        {
            IExecutionContext service = serviceProvider.GetService(typeof(IExecutionContext)) as IExecutionContext;
            Entity inputParameter = ((DataCollection<string, object>)service.InputParameters)["Target"] as Entity;
            if (!(inputParameter.LogicalName == "bsd_updateestimatehandoverdatedetail") || !(service.MessageName == "Create"))
                return;
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));
            if (!inputParameter.Contains("bsd_units"))
                throw new InvalidPluginExecutionException("Please input Units!");
            if (!inputParameter.Contains("bsd_updateestimatehandoverdate"))
                throw new InvalidPluginExecutionException("Please input Update estimate handover date!");
            Entity entity1 = this.service.Retrieve(((EntityReference)inputParameter["bsd_units"]).LogicalName, ((EntityReference)inputParameter["bsd_units"]).Id, new ColumnSet(new string[2]
            {
        "bsd_projectcode",
        "bsd_estimatehandoverdate"
            }));
            if (!entity1.Contains("bsd_projectcode"))
                throw new InvalidPluginExecutionException("Please input Project in Units!");
            Entity entity2 = this.service.Retrieve(((EntityReference)inputParameter["bsd_updateestimatehandoverdate"]).LogicalName, ((EntityReference)inputParameter["bsd_updateestimatehandoverdate"]).Id, new ColumnSet(new string[2]
            {
        "bsd_project", "bsd_typehandoverdudate"
            }));
            if (!entity2.Contains("bsd_project"))
                throw new InvalidPluginExecutionException("Please input Project in Update estimate handover date!");
            if (!entity2.Contains("bsd_typehandoverdudate"))
                throw new InvalidPluginExecutionException("Please input Type Handover Duedate in Update estimate handover date!");
            if (((EntityReference)entity1["bsd_projectcode"]).Id != ((EntityReference)entity2["bsd_project"]).Id)
                throw new InvalidPluginExecutionException("Project in Units not valid. Please check again!");
            int bsd_typehandoverdudate = ((OptionSetValue)entity2["bsd_typehandoverdudate"]).Value;
            int bsd_duedatecalculatingmethod = 100000002;
            if (bsd_typehandoverdudate == 100000001) bsd_duedatecalculatingmethod = 100000003;
            else if (bsd_typehandoverdudate == 100000002) bsd_duedatecalculatingmethod = 100000004;
            else if (bsd_typehandoverdudate == 100000003) bsd_duedatecalculatingmethod = 100000005;
            else if (bsd_typehandoverdudate == 100000004) bsd_duedatecalculatingmethod = 100000006;
            inputParameter["bsd_project"] = entity2["bsd_project"];
            if (entity1.Contains("bsd_estimatehandoverdate"))
                inputParameter["bsd_estimatehandoverdateold"] = entity1["bsd_estimatehandoverdate"];
            if (!inputParameter.Contains("bsd_paymentduedate"))
            {
                EntityCollection optionEntry = this.findOptionEntry(this.service, entity1.ToEntityReference());
                if (((Collection<Entity>)optionEntry.Entities).Count > 0)
                {
                    foreach (Entity entity3 in (Collection<Entity>)optionEntry.Entities)
                    {
                        inputParameter["bsd_optionentry"] = (object)entity3.ToEntityReference();
                        foreach (Entity entity4 in (Collection<Entity>)this.findEstimateInstallment(this.service, entity3.ToEntityReference(), bsd_duedatecalculatingmethod).Entities)
                        {
                            inputParameter["bsd_installment"] = (object)entity4.ToEntityReference();
                            if (entity4.Contains("bsd_duedate"))
                                inputParameter["bsd_paymentduedate"] = entity4["bsd_duedate"];
                        }
                    }
                }
                else
                {
                    foreach (Entity entity5 in (Collection<Entity>)this.findReservation(this.service, entity1.ToEntityReference()).Entities)
                    {
                        inputParameter["bsd_quotationreservation"] = (object)entity5.ToEntityReference();
                        foreach (Entity entity6 in (Collection<Entity>)this.findEstimateInstallment_RS(this.service, entity5.ToEntityReference(), bsd_duedatecalculatingmethod).Entities)
                        {
                            inputParameter["bsd_installment"] = (object)entity6.ToEntityReference();
                            if (entity6.Contains("bsd_duedate"))
                                inputParameter["bsd_paymentduedate"] = entity6["bsd_duedate"];
                        }
                    }
                }
            }
            else
            {
                EntityCollection optionEntry = this.findOptionEntry(this.service, entity1.ToEntityReference());
                if (((Collection<Entity>)optionEntry.Entities).Count > 0)
                {
                    foreach (Entity entity7 in (Collection<Entity>)optionEntry.Entities)
                    {
                        inputParameter["bsd_optionentry"] = (object)entity7.ToEntityReference();
                        foreach (Entity entity8 in (Collection<Entity>)this.findEstimateInstallment(this.service, entity7.ToEntityReference(), bsd_duedatecalculatingmethod).Entities)
                            inputParameter["bsd_installment"] = (object)entity8.ToEntityReference();
                    }
                }
                else
                {
                    foreach (Entity entity9 in (Collection<Entity>)this.findReservation(this.service, entity1.ToEntityReference()).Entities)
                    {
                        inputParameter["bsd_quotationreservation"] = (object)entity9.ToEntityReference();
                        foreach (Entity entity10 in (Collection<Entity>)this.findEstimateInstallment_RS(this.service, entity9.ToEntityReference(), bsd_duedatecalculatingmethod).Entities)
                            inputParameter["bsd_installment"] = (object)entity10.ToEntityReference();
                    }
                }
            }
        }

        private EntityCollection findOptionEntry(IOrganizationService service, EntityReference unit)
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true'>\r\n                            <entity name='salesorder'>\r\n                            <attribute name='salesorderid' />\r\n                            <filter type='and'>\r\n                              <condition attribute='statuscode' operator='ne' value='100000006' />\r\n                            </filter>\r\n                            <link-entity name='salesorderdetail' from='salesorderid' to='salesorderid' alias='ap'>\r\n                              <filter type='and'>\r\n                                <condition attribute='productid' operator='eq'  uitype='product' value='{0}' />\r\n                              </filter>\r\n                            </link-entity>\r\n                          </entity>\r\n                        </fetch>    ", (object)unit.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }

        private EntityCollection findReservation(IOrganizationService service, EntityReference unit)
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true'>\r\n                            <entity name='quote'>\r\n                            <attribute name='quoteid' />\r\n                            <filter type='and'>\r\n                              <condition attribute='statuscode' operator='ne' value='2' />\r\n                              <condition attribute='statuscode' operator='ne' value='6' />\r\n                              <condition attribute='bsd_unitno' operator='eq' uitype='product' value='{0}' />    \r\n                            </filter>                            \r\n                          </entity>\r\n                        </fetch>    ", (object)unit.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }

        private EntityCollection findEstimateInstallment(
          IOrganizationService service,
          EntityReference oe, int bsd_duedatecalculatingmethod)
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>\r\n              <entity name='bsd_paymentschemedetail'>\r\n                <attribute name='bsd_paymentschemedetailid' />\r\n                <attribute name='bsd_duedate' />\r\n                <filter type='and'>\r\n                  <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='{0}' />\r\n                  <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{0}' />\r\n                </filter>\r\n              </entity>\r\n            </fetch>", bsd_duedatecalculatingmethod, (object)oe.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }

        private EntityCollection findEstimateInstallment_RS(
          IOrganizationService service,
          EntityReference rs, int bsd_duedatecalculatingmethod)
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>\r\n              <entity name='bsd_paymentschemedetail'>\r\n                <attribute name='bsd_paymentschemedetailid' />\r\n                <attribute name='bsd_duedate' />\r\n                <filter type='and'>\r\n                  <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='{0}' />\r\n                  <condition attribute='bsd_reservation' operator='eq' uitype='quote' value='{0}' />\r\n                </filter>\r\n              </entity>\r\n            </fetch>", bsd_duedatecalculatingmethod, (object)rs.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }
    }
}
