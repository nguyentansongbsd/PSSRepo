using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Plugin_ApplyDocument_Create_Update
{
    public class Plugin_ApplyDocument_Create_Update : IPlugin
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
            if (context.MessageName == "Create")
            {
                if (context.Depth > 2) return;
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                int bsd_transactiontype = enTarget.Contains("bsd_transactiontype") ? ((OptionSetValue)enTarget["bsd_transactiontype"]).Value : 0;
                decimal bsd_advancepaymentamount = enTarget.Contains("bsd_advancepaymentamount") ? ((Money)enTarget["bsd_advancepaymentamount"]).Value : 0;
                if (bsd_transactiontype == 1)
                {
                    if (!enTarget.Contains("bsd_customer") || !enTarget.Contains("bsd_project") || !enTarget.Contains("bsd_quote"))
                    {
                        throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                    }
                    Entity enQO = service.Retrieve(((EntityReference)enTarget["bsd_quote"]).LogicalName, ((EntityReference)enTarget["bsd_quote"]).Id, new ColumnSet(true));
                    int statuscode = enQO.Contains("statuscode") ? ((OptionSetValue)enQO["statuscode"]).Value : 0;
                    if (statuscode != 100000006 && statuscode != 100000000)
                    {
                        throw new InvalidPluginExecutionException("The transaction status appears to be incorrect. Please verify it.");
                    }
                    if (!enTarget.Contains("bsd_units") && enQO.Contains("bsd_unitno"))
                    {
                        Entity enUp = new Entity(enTarget.LogicalName, enTarget.Id);
                        enUp["bsd_units"] = (EntityReference)enQO["bsd_unitno"];
                        service.Update(enUp);
                    }
                    checkAmountAdvance(((EntityReference)enTarget["bsd_customer"]).Id, ((EntityReference)enTarget["bsd_project"]).Id, ((EntityReference)enTarget["bsd_quote"]).Id, bsd_advancepaymentamount);

                }
                else if (bsd_transactiontype != 0)
                {
                    if (!enTarget.Contains("bsd_customer") || !enTarget.Contains("bsd_project") || !enTarget.Contains("bsd_optionentry"))
                    {
                        throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                    }
                    Entity enOE = service.Retrieve(((EntityReference)enTarget["bsd_optionentry"]).LogicalName, ((EntityReference)enTarget["bsd_optionentry"]).Id, new ColumnSet(true));
                    int statuscode = enOE.Contains("statuscode") ? ((OptionSetValue)enOE["statuscode"]).Value : 0;
                    if (statuscode == 100000006)
                    {
                        throw new InvalidPluginExecutionException("The transaction status appears to be incorrect. Please verify it.");
                    }
                    if (!enTarget.Contains("bsd_units") && enOE.Contains("bsd_unitnumber"))
                    {
                        Entity enUp = new Entity(enTarget.LogicalName, enTarget.Id);
                        enUp["bsd_units"] = (EntityReference)enOE["bsd_unitnumber"];
                        service.Update(enUp);
                    }
                    checkAmountAdvance(((EntityReference)enTarget["bsd_customer"]).Id, ((EntityReference)enTarget["bsd_project"]).Id, ((EntityReference)enTarget["bsd_optionentry"]).Id, bsd_advancepaymentamount);

                }
                else throw new InvalidPluginExecutionException("Kindly complete the 'Types of payment' field.");
            }
            else if (context.MessageName == "Update")
            {
                if (context.Depth > 1) return;
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                if ((enTarget.Contains("statuscode") ? ((OptionSetValue)enTarget["statuscode"]).Value : 0) != 1) return;
                int bsd_transactiontype = enTarget.Contains("bsd_transactiontype") ? ((OptionSetValue)enTarget["bsd_transactiontype"]).Value : 0;
                decimal bsd_advancepaymentamount = enTarget.Contains("bsd_advancepaymentamount") ? ((Money)enTarget["bsd_advancepaymentamount"]).Value : 0;
                if (bsd_transactiontype == 1)
                {
                    if (!enTarget.Contains("bsd_customer") || !enTarget.Contains("bsd_project") || !enTarget.Contains("bsd_quote"))
                    {
                        throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                    }
                    Entity enQO = service.Retrieve(((EntityReference)enTarget["bsd_quote"]).LogicalName, ((EntityReference)enTarget["bsd_quote"]).Id, new ColumnSet(true));
                    int statuscode = enQO.Contains("statuscode") ? ((OptionSetValue)enQO["statuscode"]).Value : 0;
                    if (statuscode != 100000006 && statuscode != 100000000)
                    {
                        throw new InvalidPluginExecutionException("The transaction status appears to be incorrect. Please verify it.");
                    }
                    if (!enTarget.Contains("bsd_units") && enQO.Contains("bsd_unitno"))
                    {
                        Entity enUp = new Entity(enTarget.LogicalName, enTarget.Id);
                        enUp["bsd_units"] = (EntityReference)enQO["bsd_unitno"];
                        service.Update(enUp);
                    }
                    checkAmountAdvance(((EntityReference)enTarget["bsd_customer"]).Id, ((EntityReference)enTarget["bsd_project"]).Id, ((EntityReference)enTarget["bsd_quote"]).Id, bsd_advancepaymentamount);

                }
                else if (bsd_transactiontype != 0)
                {
                    if (!enTarget.Contains("bsd_customer") || !enTarget.Contains("bsd_project") || !enTarget.Contains("bsd_optionentry"))
                    {
                        throw new InvalidPluginExecutionException("Kindly provide the transaction information.");
                    }
                    Entity enOE = service.Retrieve(((EntityReference)enTarget["bsd_optionentry"]).LogicalName, ((EntityReference)enTarget["bsd_optionentry"]).Id, new ColumnSet(true));
                    int statuscode = enOE.Contains("statuscode") ? ((OptionSetValue)enOE["statuscode"]).Value : 0;
                    if (statuscode == 100000006)
                    {
                        throw new InvalidPluginExecutionException("The transaction status appears to be incorrect. Please verify it.");
                    }
                    if (!enTarget.Contains("bsd_units") && enOE.Contains("bsd_unitnumber"))
                    {
                        Entity enUp = new Entity(enTarget.LogicalName, enTarget.Id);
                        enUp["bsd_units"] = (EntityReference)enOE["bsd_unitnumber"];
                        service.Update(enUp);
                    }
                    checkAmountAdvance(((EntityReference)enTarget["bsd_customer"]).Id, ((EntityReference)enTarget["bsd_project"]).Id, ((EntityReference)enTarget["bsd_optionentry"]).Id, bsd_advancepaymentamount);

                }
                else throw new InvalidPluginExecutionException("Kindly complete the 'Types of payment' field.");
            }
        }
        private void checkAmountAdvance(Guid KH, Guid DA, Guid OE, decimal AmountAdvance)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch>
                                  <entity name=""bsd_advancepayment"">
                                    <attribute name=""bsd_remainingamount"" />
                                    <filter>
                                      <condition attribute=""bsd_customer"" operator=""eq"" value=""{KH}"" />
                                      <condition attribute=""bsd_project"" operator=""eq"" value=""{DA}"" />
                                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{OE}"" />
                                      <condition attribute=""bsd_remainingamount"" operator=""gt"" value=""{0}"" />
                                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                                    </filter>
                                  </entity>
                                </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs.Entities.Count == 0 || rs.Entities.Sum(x => ((Money)x["bsd_remainingamount"]).Value) < AmountAdvance)
            {
                throw new InvalidPluginExecutionException("The advance payment amount appears to be invalid. Kindly review and confirm.");
            }
        }
    }
}