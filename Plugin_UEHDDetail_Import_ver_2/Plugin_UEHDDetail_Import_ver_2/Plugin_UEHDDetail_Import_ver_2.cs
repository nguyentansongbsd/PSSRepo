// Decompiled with JetBrains decompiler
// Type: Plugin_UEHDDetail_Import.Plugin_UEHDDetail_Import
// Assembly: Plugin_UEHDDetail_Import, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5de1e6efa028ef71
// MVID: 4BAF723D-6137-446F-9CF9-D039ADD17355
// Assembly location: C:\Users\BSD\Desktop\Plugin_UEHDDetail_Import.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Metadata;
using System.Runtime.Remoting.Contexts;
using System.Web.UI.WebControls;

namespace Plugin_UEHDDetail_Import_ver_2
{
    public class Plugin_UEHDDetail_Import_ver_2 : IPlugin
    {
        public IOrganizationService service = (IOrganizationService)null;
        public IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            if (!context.InputParameters.Contains("Target"))
                return;

            if (!(context.InputParameters["Target"] is Entity target))
                return;
            Entity inputParameter = new Entity(target.LogicalName, target.Id);
            if (!(target.LogicalName == "bsd_updateestimatehandoverdatedetail") || !(context.MessageName == "Create"))
                return;
            
            if (!target.Contains("bsd_units"))
                throw new InvalidPluginExecutionException("Please input Units!");
            if (!target.Contains("bsd_updateestimatehandoverdate"))
                throw new InvalidPluginExecutionException("Please input Update estimate handover date!");
            Entity entity1 = this.service.Retrieve(((EntityReference)target["bsd_units"]).LogicalName, ((EntityReference)target["bsd_units"]).Id, new ColumnSet(new string[2]
            {
        "bsd_projectcode",
        "bsd_estimatehandoverdate"
            }));
            if (!entity1.Contains("bsd_projectcode"))
                throw new InvalidPluginExecutionException("Please input Project in Units!");
            Entity entity2 = this.service.Retrieve(((EntityReference)target["bsd_updateestimatehandoverdate"]).LogicalName, ((EntityReference)target["bsd_updateestimatehandoverdate"]).Id, new ColumnSet(new string[3]
            {
        "bsd_project", "bsd_typehandoverdudate", "bsd_paymentduedate"
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
                //throw new InvalidPluginExecutionException("if Count " + optionEntry.Entities.Count);
                if (optionEntry.Entities.Count > 0)
                {
                    foreach (Entity entity3 in optionEntry.Entities)
                    {
                        inputParameter["bsd_optionentry"] = entity3.ToEntityReference();
                        int count = 0;
                        foreach (Entity entity4 in (Collection<Entity>)this.findEstimateInstallment(this.service, entity3.ToEntityReference(), bsd_duedatecalculatingmethod).Entities)
                        {
                            count++;
                            inputParameter["bsd_installment"] = (object)entity4.ToEntityReference();
                            if (entity2.Contains("bsd_paymentduedate"))
                                inputParameter["bsd_paymentduedate"] = entity2["bsd_paymentduedate"];
                        }
                        if (count == 0)
                            throw new InvalidPluginExecutionException("Installment not found. Please check again!");
                    }
                }
                else
                {
                    foreach (Entity entity5 in (Collection<Entity>)this.findReservation(this.service, entity1.ToEntityReference()).Entities)
                    {
                        inputParameter["bsd_quotationreservation"] = (object)entity5.ToEntityReference();
                        int count = 0;
                        foreach (Entity entity6 in (Collection<Entity>)this.findEstimateInstallment_RS(this.service, entity5.ToEntityReference(), bsd_duedatecalculatingmethod).Entities)
                        {
                            count++;
                            inputParameter["bsd_installment"] = (object)entity6.ToEntityReference();
                            if (entity2.Contains("bsd_paymentduedate"))
                                inputParameter["bsd_paymentduedate"] = entity2["bsd_paymentduedate"];
                        }
                        if (count == 0)
                            throw new InvalidPluginExecutionException("Installment not found. Please check again!");
                    }
                }
            }
            else
            {
                EntityCollection optionEntry = this.findOptionEntry(this.service, entity1.ToEntityReference());
                //throw new InvalidPluginExecutionException("else Count " + optionEntry.Entities.Count);
                if (optionEntry.Entities.Count > 0)
                {
                    foreach (Entity entity7 in optionEntry.Entities)
                    {
                        inputParameter["bsd_optionentry"] = (object)entity7.ToEntityReference();
                        int count = 0;
                        foreach (Entity entity8 in (Collection<Entity>)this.findEstimateInstallment(this.service, entity7.ToEntityReference(), bsd_duedatecalculatingmethod).Entities)
                        {
                            count++;
                            inputParameter["bsd_installment"] = (object)entity8.ToEntityReference();
                        }
                        if (count == 0)
                            throw new InvalidPluginExecutionException("Installment not found. Please check again!");
                    }
                }
                else
                {
                    foreach (Entity entity9 in (Collection<Entity>)this.findReservation(this.service, entity1.ToEntityReference()).Entities)
                    {
                        inputParameter["bsd_quotationreservation"] = (object)entity9.ToEntityReference();
                        int count = 0;
                        foreach (Entity entity10 in (Collection<Entity>)this.findEstimateInstallment_RS(this.service, entity9.ToEntityReference(), bsd_duedatecalculatingmethod).Entities)
                        {
                            count++;
                            inputParameter["bsd_installment"] = (object)entity10.ToEntityReference();
                        }
                        if (count == 0)
                            throw new InvalidPluginExecutionException("Installment not found. Please check again!");
                    }
                }
                if (entity2.Contains("bsd_paymentduedate"))
                    inputParameter["bsd_paymentduedate"] = entity2["bsd_paymentduedate"];
            }
            service.Update(inputParameter);
        }

        private EntityCollection findOptionEntry(IOrganizationService service, EntityReference unit)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""salesorder"">
                <attribute name=""salesorderid"" />
                <filter>
                  <condition attribute=""statuscode"" operator=""ne"" value=""{100000006}"" />
                  <condition attribute=""bsd_unitnumber"" operator=""eq"" value=""{unit.Id}"" />
                </filter>
              </entity>
            </fetch>";
            //string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true'>\r\n                            <entity name='salesorder'>\r\n                            <attribute name='salesorderid' />\r\n                            <filter type='and'>\r\n                              <condition attribute='statuscode' operator='ne' value='100000006' />\r\n                            </filter>\r\n                            <link-entity name='salesorderdetail' from='salesorderid' to='salesorderid' alias='ap'>\r\n                              <filter type='and'>\r\n                                <condition attribute='productid' operator='eq'  uitype='product' value='{0}' />\r\n                              </filter>\r\n                            </link-entity>\r\n                          </entity>\r\n                        </fetch>    ", (object)unit.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(fetchXml));
        }

        private EntityCollection findReservation(IOrganizationService service, EntityReference unit)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""quote"">
                <attribute name=""quoteid"" />
                <filter>
                  <condition attribute=""statuscode"" operator=""ne"" value=""{2}"" />
                  <condition attribute=""statuscode"" operator=""ne"" value=""{6}"" />
                  <condition attribute=""bsd_unitno"" operator=""eq"" value=""{unit.Id}"" />
                </filter>
              </entity>
            </fetch>";
            //string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true'>\r\n                            <entity name='quote'>\r\n                            <attribute name='quoteid' />\r\n                            <filter type='and'>\r\n                              <condition attribute='statuscode' operator='ne' value='2' />\r\n                              <condition attribute='statuscode' operator='ne' value='6' />\r\n                              <condition attribute='bsd_unitno' operator='eq' uitype='product' value='{0}' />    \r\n                            </filter>                            \r\n                          </entity>\r\n                        </fetch>    ", (object)unit.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(fetchXml));
        }

        private EntityCollection findEstimateInstallment(
          IOrganizationService service,
          EntityReference oe, int bsd_duedatecalculatingmethod)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_paymentschemedetailid"" />
                <attribute name=""bsd_duedate"" />
                <filter>
                  <condition attribute=""bsd_duedatecalculatingmethod"" operator=""eq"" value=""{bsd_duedatecalculatingmethod}"" />
                  <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{oe.Id}"" />
                </filter>
              </entity>
            </fetch>";
            //string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>\r\n              <entity name='bsd_paymentschemedetail'>\r\n                <attribute name='bsd_paymentschemedetailid' />\r\n                <attribute name='bsd_duedate' />\r\n                <filter type='and'>\r\n                  <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='{0}' />\r\n                  <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='{1}' />\r\n                </filter>\r\n              </entity>\r\n            </fetch>", bsd_duedatecalculatingmethod, (object)oe.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(fetchXml));
        }

        private EntityCollection findEstimateInstallment_RS(
          IOrganizationService service,
          EntityReference rs, int bsd_duedatecalculatingmethod)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_paymentschemedetailid"" />
                <attribute name=""bsd_duedate"" />
                <filter>
                  <condition attribute=""bsd_duedatecalculatingmethod"" operator=""eq"" value=""{bsd_duedatecalculatingmethod}"" />
                  <condition attribute=""bsd_reservation"" operator=""eq"" value=""{rs.Id}"" />
                </filter>
              </entity>
            </fetch>";
            //string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>\r\n              <entity name='bsd_paymentschemedetail'>\r\n                <attribute name='bsd_paymentschemedetailid' />\r\n                <attribute name='bsd_duedate' />\r\n                <filter type='and'>\r\n                  <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='{0}' />\r\n                  <condition attribute='bsd_reservation' operator='eq' uitype='quote' value='{1}' />\r\n                </filter>\r\n              </entity>\r\n            </fetch>", bsd_duedatecalculatingmethod, (object)rs.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(fetchXml));
        }
    }
}
