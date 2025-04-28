using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugin_BankAccount_Create_Update
{
    public class Plugin_BankAccount_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName == "Create")
            {
                traceService.Trace("Create");
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enBankAccount = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                traceService.Trace("step 1");
                bool bsd_default = enBankAccount.Contains("bsd_default") ? (bool)enBankAccount["bsd_default"] : false;
                EntityReference enKH = new EntityReference();
                bool step = false;
                if (enBankAccount.Contains("bsd_id"))
                {
                    var fetchXml = $@"
                    <fetch top=""1"">
                      <entity name='account'>
                        <filter>
                          <condition attribute='bsd_registrationcode' operator='eq' value='{(string)enBankAccount["bsd_id"]}'/>
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (list.Entities.Count > 0)
                    {
                        foreach (Entity item in list.Entities)
                        {
                            enKH = item.ToEntityReference();
                            step = true;
                        }
                    }
                    else
                    {
                        fetchXml = $@"
                        <fetch top=""1"">
                          <entity name='contact'>
                            <filter  type=""or"">
                              <condition attribute='bsd_identitycardnumber' operator='eq' value='{(string)enBankAccount["bsd_id"]}'/>
                              <condition attribute='bsd_passport' operator='eq' value='{(string)enBankAccount["bsd_id"]}'/>
                            </filter>
                          </entity>
                        </fetch>";
                        list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                        if (list.Entities.Count > 0)
                        {
                            foreach (Entity item in list.Entities)
                            {
                                enKH = item.ToEntityReference();
                                step = true;
                            }
                        }
                    }
                    if (bsd_default && step)
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='bsd_bankaccount'>
                            <filter>
                              <condition attribute='bsd_bankaccountid' operator='ne' value='{target.Id}'/>
                              <condition attribute='bsd_customer' operator='eq' value='{enKH.Id}'/>
                              <condition attribute='bsd_default' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        if (list2.Entities.Count > 0)
                            throw new InvalidPluginExecutionException("Default bank account exists.");
                    }
                    if (step)
                    {
                        Entity enUp = new Entity(target.LogicalName, target.Id);
                        enUp["bsd_customer"] = enKH;
                        service.Update(enUp);
                    }
                }
            }
            else if (context.MessageName == "Update")
            {
                traceService.Trace("Update");
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enBankAccount = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                traceService.Trace("step 1");
                bool bsd_default = enBankAccount.Contains("bsd_default") ? (bool)enBankAccount["bsd_default"] : false;
                if (bsd_default && enBankAccount.Contains("bsd_customer"))
                {
                    var fetchXml2 = $@"
                        <fetch>
                          <entity name='bsd_bankaccount'>
                            <filter>
                              <condition attribute='bsd_bankaccountid' operator='ne' value='{target.Id}'/>
                              <condition attribute='bsd_customer' operator='eq' value='{((EntityReference)enBankAccount["bsd_customer"]).Id}'/>
                              <condition attribute='bsd_default' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                    EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    if (list2.Entities.Count > 0)
                        throw new InvalidPluginExecutionException("Default bank account exists.");
                }
            }
        }
    }
}
