using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_BankingLoan_Mortgage
{
    public class Action_BankingLoan_Mortgage : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(context.UserId);

            EntityReference inputParameter = (EntityReference)(context.InputParameters)["Target"];
            Entity enBankLoan = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[2]
            {
                "bsd_units",
                "bsd_optionentry"
            }));
            Entity enUnit = new Entity("product");
            enUnit.Id = enBankLoan.Contains("bsd_units") ? ((EntityReference)enBankLoan["bsd_units"]).Id : throw new Exception("You haven't chosen the unit to perform this action. Please re-check before processing.");
            Entity entity5 = this.service.Retrieve(((EntityReference)enBankLoan["bsd_units"]).LogicalName, ((EntityReference)enBankLoan["bsd_units"]).Id, new ColumnSet(new string[2]
            {
                "bsd_bankloan",
                "statuscode"
            }));
            if (!enBankLoan.Contains("bsd_optionentry"))
                throw new Exception("You haven't chosen the Option Entry to perform this action. Please re-check before processing.");
            Entity enOE = this.service.Retrieve(((EntityReference)enBankLoan["bsd_optionentry"]).LogicalName, ((EntityReference)enBankLoan["bsd_optionentry"]).Id, new ColumnSet(new string[1]
            {
                "statuscode"
            }));
            if (enOE.Contains("statuscode") && (((OptionSetValue)enOE["statuscode"]).Value == 100000006 || ((OptionSetValue)enOE["statuscode"]).Value == 100000007 || ((OptionSetValue)enOE["statuscode"]).Value == 2))
            {
                string str = "";
                if (((OptionSetValue)enOE["statuscode"]).Value == 100000006)
                    str = "Terminate";
                if (((OptionSetValue)enOE["statuscode"]).Value == 100000007)
                    str = "Cancelling";
                if (((OptionSetValue)enOE["statuscode"]).Value == 100000002)
                    str = "Pending";
                throw new Exception("Option Entry status is not suitable to perform this action (" + str + "). Please re-check or re-pick.");
            }
            switch (context.InputParameters["type"].ToString())
            {
                case "Mortgage":
                    if (((OptionSetValue)entity5["statuscode"]).Value != 100000002)
                        throw new InvalidPluginExecutionException("This Unit have not been Sold. Please re-check or re-pick.");
                    if ((bool)entity5["bsd_bankloan"])
                        throw new Exception("The unit you have chosen is mortgaged. Please re-check or perform the mortgage session with another unit.");
                    enUnit["bsd_bankloan"] = (object)true;
                    this.service.Update(new Entity(enBankLoan.LogicalName, enBankLoan.Id)
                    {
                        ["statuscode"] = (object)new OptionSetValue(100000000)
                    });
                    break;
                case "Demortgage":
                    if (!(bool)entity5["bsd_bankloan"])
                        throw new Exception("The unit you have chosen is not mortgaged so it cannot be demortgaged.Please re-check!");
                    enUnit["bsd_bankloan"] = (object)false;
                    this.service.Update(new Entity(enBankLoan.LogicalName, enBankLoan.Id)
                    {
                        ["statuscode"] = (object)new OptionSetValue(100000001),
                        ["bsd_demortgagedate"] = Convert.ToDateTime(context.InputParameters["demortgageDate"].ToString())
                    });
                    break;
                case "cancel":
                    enUnit["bsd_bankloan"] = (object)false;
                    this.service.Update(new Entity(enBankLoan.LogicalName, enBankLoan.Id)
                    {
                        ["statuscode"] = (object)new OptionSetValue(100000002)
                    });
                    break;
            }
            this.service.Update(enUnit);
        }
    }
}
