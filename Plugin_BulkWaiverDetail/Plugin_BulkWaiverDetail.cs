using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugin_BulkWaiverDetail
{
    public class Plugin_BulkWaiverDetail : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        IPluginExecutionContext context = null;
        ITracingService traceService = null;
        public void Execute(IServiceProvider serviceProvider)
        {

            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity target = (Entity)context.InputParameters["Target"];
            traceService.Trace(context.MessageName);
            switch (context.MessageName)
            {
                case "Create":

                    if (target.Contains("bsd_installment") == false)
                    {
                        Entity enOptionEntry = service.Retrieve(((EntityReference)target.Attributes["bsd_optionentry"]).LogicalName, ((EntityReference)target.Attributes["bsd_optionentry"]).Id, new ColumnSet(true));
                        int statuscode_OE = ((OptionSetValue)enOptionEntry["statuscode"]).Value;
                        if (statuscode_OE == 100000006) throw new InvalidPluginExecutionException("Option Entry has been terminated.");
                        else if (statuscode_OE == 100001) throw new InvalidPluginExecutionException("Option Entry has been completed.");

                        traceService.Trace("1");
                        Entity enBulkWaiver = service.Retrieve(((EntityReference)target.Attributes["bsd_bulkwaiver"]).LogicalName, ((EntityReference)target.Attributes["bsd_bulkwaiver"]).Id, new ColumnSet(true));
                        traceService.Trace("2");
                        if (target.Contains("bsd_project") && enBulkWaiver.Contains("bsd_project") && ((EntityReference)target["bsd_project"]).Id != ((EntityReference)enBulkWaiver["bsd_project"]).Id)
                            throw new InvalidPluginExecutionException("Project for waiver is invalid.");
                        traceService.Trace("3");
                        EntityReference enrefUnit = (EntityReference)enOptionEntry.Attributes["bsd_unitnumber"];
                        traceService.Trace("4");
                        int bsd_installmentnumber = (int)target.Attributes["bsd_installmentnumber"];
                        traceService.Trace("5");
                        int bsd_waivertype = ((OptionSetValue)target.Attributes["bsd_waivertype"]).Value;
                        traceService.Trace("6");
                        string xml =
                            @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='bsd_paymentschemedetail'>
                                <attribute name='bsd_name' />
                                <attribute name='bsd_ordernumber' />
                                <attribute name='bsd_duedate' />
                                <attribute name='statuscode' />
                                <attribute name='bsd_waiveramount' />
                                <attribute name='bsd_waiverinterest' />
                                <attribute name='bsd_waiverinstallment' />
                                <attribute name='bsd_managementfeesstatus' />
                                <attribute name='bsd_managementfee' />
                                <attribute name='bsd_managementamount' />
                                <attribute name='bsd_managementfeepaid' />
                                <attribute name='bsd_managementfeewaiver' />
                                <attribute name='bsd_maintenancefees' />
                                <attribute name='bsd_maintenancefeesstatus' />
                                <attribute name='bsd_maintenancefeewaiver' />
                                <attribute name='bsd_maintenancefeepaid' />
                                <attribute name='bsd_maintenanceamount' />
                                <attribute name='bsd_interestwaspaid' />
                                <attribute name='bsd_interestchargeamount' />
                                <attribute name='bsd_depositamount' />
                                <attribute name='bsd_amountwaspaid' />
                                <attribute name='bsd_amountpay' />
                                <attribute name='bsd_amountofthisphase' />
                                <order attribute='bsd_ordernumber' descending='false' />
                                <filter type='and'>
                                  <condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + enOptionEntry.Id + @"' />
                                  <condition attribute='bsd_ordernumber' operator='eq' value='" + bsd_installmentnumber.ToString() + @"' />
                                </filter>
                              </entity>
                            </fetch>";

                        EntityCollection enCoInstallment = service.RetrieveMultiple(new FetchExpression(xml));

                        if (enCoInstallment.Entities.Count > 0)
                        {

                            Entity enInstallment = enCoInstallment.Entities[0];
                            target["bsd_name"] = enBulkWaiver.Attributes["bsd_name"];
                            target["bsd_units"] = enrefUnit;
                            target["bsd_installment"] = enInstallment.ToEntityReference();
                            switch (bsd_waivertype)
                            {
                                case 100000000://Installment
                                    Money bsd_amountofthisphase = enInstallment.Contains("bsd_amountofthisphase") ? (Money)enInstallment["bsd_amountofthisphase"] : new Money(0);
                                    Money bsd_amountwaspaid = enInstallment.Contains("bsd_amountwaspaid") ? (Money)enInstallment["bsd_amountwaspaid"] : new Money(0);
                                    Money bsd_waiverinstallment = enInstallment.Contains("bsd_waiverinstallment") ? (Money)enInstallment["bsd_waiverinstallment"] : new Money(0);
                                    Money bsd_outstandingamount = new Money(bsd_amountofthisphase.Value - bsd_amountwaspaid.Value - bsd_waiverinstallment.Value);
                                    target["bsd_installmentamount"] = bsd_amountofthisphase;
                                    target["bsd_paidamount"] = bsd_amountwaspaid;
                                    target["bsd_waiverinstallment"] = bsd_waiverinstallment;
                                    target["bsd_outstandingamount"] = bsd_outstandingamount;
                                    break;
                                case 100000001://Interest
                                    Money bsd_interestchargeamount = enInstallment.Contains("bsd_interestchargeamount") ? (Money)enInstallment["bsd_interestchargeamount"] : new Money(0);
                                    Money bsd_interestwaspaid = enInstallment.Contains("bsd_interestwaspaid") ? (Money)enInstallment["bsd_interestwaspaid"] : new Money(0);
                                    Money bsd_waiverinterest = enInstallment.Contains("bsd_waiverinterest") ? (Money)enInstallment["bsd_waiverinterest"] : new Money(0);
                                    target["bsd_interestchargeamount"] = bsd_interestchargeamount;
                                    target["bsd_interestwaspaid"] = bsd_interestwaspaid;
                                    target["bsd_waiverinterest"] = bsd_waiverinterest;
                                    break;
                                case 100000002://Management Fee
                                    Money bsd_managementamount = enInstallment.Contains("bsd_managementamount") ? (Money)enInstallment["bsd_managementamount"] : new Money(0);
                                    Money bsd_managementfeepaid = enInstallment.Contains("bsd_managementfeepaid") ? (Money)enInstallment["bsd_managementfeepaid"] : new Money(0);
                                    Money bsd_managementfeewaiver = enInstallment.Contains("bsd_managementfeewaiver") ? (Money)enInstallment["bsd_managementfeewaiver"] : new Money(0);
                                    target["bsd_managementamount"] = bsd_managementamount;
                                    target["bsd_managementfeepaid"] = bsd_managementfeepaid;
                                    target["bsd_managementfeewaiver"] = bsd_managementfeewaiver;
                                    break;
                                case 100000003://Maintenance Fee
                                    Money bsd_maintenanceamount = enInstallment.Contains("bsd_maintenanceamount") ? (Money)enInstallment["bsd_maintenanceamount"] : new Money(0);
                                    Money bsd_maintenancefeepaid = enInstallment.Contains("bsd_maintenancefeepaid") ? (Money)enInstallment["bsd_maintenancefeepaid"] : new Money(0);
                                    Money bsd_maintenancefeewaiver = enInstallment.Contains("bsd_maintenancefeewaiver") ? (Money)enInstallment["bsd_maintenancefeewaiver"] : new Money(0);
                                    target["bsd_maintenanceamount"] = bsd_maintenanceamount;
                                    target["bsd_maintenancefeepaid"] = bsd_maintenancefeepaid;
                                    target["bsd_maintenancefeewaiver"] = bsd_maintenancefeewaiver;
                                    break;
                            }
                            //service.Update(target);
                        }
                    }
                    break;
            }
        }
    }
}
