using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_BankLoan_Import
{
    public class Plugin_BankLoan_Import : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        Entity en = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = factory.CreateOrganizationService(this.context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Init();
        }
        private void Init()
        {
            try
            {
                if (this.context.MessageName != "Create") return;
                this.en = (Entity)this.context.InputParameters["Target"];
                if ((((OptionSetValue)this.en["statuscode"]).Value != 100000000 && ((OptionSetValue)this.en["statuscode"]).Value != 1) && (bool)this.en["bsd_import"] == true) throw new InvalidPluginExecutionException("The status of the bank loan isn't \"Active\" or \"Mortgage\", so it cannot be processed.");
                if (!this.en.Contains("bsd_optionentry")) throw new InvalidPluginExecutionException("Record don't have Contract.");
                Entity enOE = getOE((EntityReference)this.en["bsd_optionentry"]);
                if (((EntityReference)this.en["bsd_project"]).Id != ((EntityReference)enOE["bsd_project"]).Id) throw new InvalidPluginExecutionException("The contract doesn't belong to the entered Project. Please check the information again.");
                if (((OptionSetValue)enOE["statuscode"]).Value == 100000000 || ((OptionSetValue)enOE["statuscode"]).Value == 100000001) throw new InvalidPluginExecutionException("The contract has not been signed, so the bank loan cannot be processed.");
                if (checkBankLoan(enOE) && (bool)this.en["bsd_import"] == true) throw new InvalidPluginExecutionException("The contract already has a bank loan in 'Active' or 'Mortgage' status, so it can't be processed.");

                mapField(enOE);
                updateUnit((EntityReference)enOE["bsd_unitnumber"]);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private Entity getOE(EntityReference enfOE)
        {
            try
            {
                Entity en = this.service.Retrieve(enfOE.LogicalName, enfOE.Id, new ColumnSet(new string[] { "customerid", "bsd_unitnumber", "bsd_project", "statuscode" }));
                return en;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void updateUnit(EntityReference enfUnit)
        {
            try
            {
                tracingService.Trace("Start update unit");
                if (((OptionSetValue)this.en["statuscode"]).Value == 1) return;
                Entity enUnit = new Entity(enfUnit.LogicalName, enfUnit.Id);
                enUnit["bsd_bankloan"] = true;
                this.service.Update(enUnit);
                tracingService.Trace("End update unit");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void mapField(Entity enOE)
        {
            try
            {
                tracingService.Trace("Start map field bank loan");
                Entity enBankLoan = new Entity(this.en.LogicalName, this.en.Id);
                enBankLoan["bsd_units"] = enOE.Contains("bsd_unitnumber") ? (EntityReference)enOE["bsd_unitnumber"] : null;
                enBankLoan["bsd_purchaser"] = enOE.Contains("customerid") ? (EntityReference)enOE["customerid"] : null;
                this.service.Update(enBankLoan);
                tracingService.Trace("End map field bank loan");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private bool checkBankLoan(Entity enOE)
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_bankingloan"">
                    <attribute name=""bsd_bankingloanid"" />
                    <attribute name=""statuscode"" />
                    <filter>
                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{enOE.Id}"" />
                      <condition attribute=""bsd_bankingloanid"" operator=""ne"" value=""{this.en.Id}"" />
                      <condition attribute=""statuscode"" operator=""in"" >
                        <value>1</value>
                        <value>100000000</value>
                      </condition>
                      <condition attribute=""statecode"" operator=""eq"" value=""0"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection enBankLoan = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (enBankLoan.Entities.Count == 0) return false;
                else return true;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
