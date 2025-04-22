using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_SearchCustomer
{
    public class Action_SearchCustomer : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");

            int type = (int)context.InputParameters["type"];
            var html = string.Empty;
            if (type == 0) // khcn
            {
                string cmnd = (string)context.InputParameters["cmnd"];
                string cccd = (string)context.InputParameters["cccd"];
                string passport = (string)context.InputParameters["passport"];
                string otherCode = (string)context.InputParameters["otherCode"];
                string fullName = (string)context.InputParameters["fullName"];
                string telephone = (string)context.InputParameters["telephone"];
                string email = (string)context.InputParameters["email"];

                var queryKHCN = new QueryExpression("contact");
                traceService.Trace(cmnd);
                queryKHCN.ColumnSet.AllColumns = true;
                if (cmnd != "") queryKHCN.Criteria.AddCondition("bsd_identitycardnumber", ConditionOperator.Equal,cmnd);
                if (cmnd == "") return;
                //if (cccd != "") queryKHCN.Criteria.AddCondition("bsd_identitycard", ConditionOperator.Equal,cccd);
                if (passport != "") queryKHCN.Criteria.AddCondition("bsd_passport", ConditionOperator.Equal, passport);
                //if (otherCode != "") queryKHCN.Criteria.AddCondition("bsd_othercode", ConditionOperator.Equal, otherCode);
                //if (fullName != "") queryKHCN.Criteria.AddCondition("fullname", ConditionOperator.Equal, fullName);
                //if (telephone != "") queryKHCN.Criteria.AddCondition("mobilephone", ConditionOperator.Equal, telephone);
                //if (email != "") queryKHCN.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, email);
                queryKHCN.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                EntityCollection result = service.RetrieveMultiple(queryKHCN);
                traceService.Trace(result.Entities.Count.ToString());
                if (result != null && result.Entities != null && result.Entities.Count > 0)
                {
                    for (int i = 0; i < result.Entities.Count; i++)
                    {
                        Entity item = result.Entities[i];
                        html += $@"
                        <tr>
                            <td><input type='checkbox' name='choose-ckb' data-id='{item.Id}' /></td>
                            <td>{i + 1}</td>
                            <td>{getValueXML(item, "fullname")}</td>
                            <td>{(item.Contains("gendercode") ? item.FormattedValues["gendercode"] : "")}</td>
                            <td>{getValueXML(item, "bsd_contactaddress")}</td>
                            <td>{getValueXML(item, "mobilephone")}</td>
                            <td>{getValueXML(item, "emailaddress1")}</td>
                        </tr>
                    ";
                    }
                }
            }
            else
            {
                string nameCompany = (string)context.InputParameters["nameCompany"];
                string registrationCode = (string)context.InputParameters["registrationCode"];
                string telephone = (string)context.InputParameters["telephone"];
                string email = (string)context.InputParameters["email"];

                var queryKHDN = new QueryExpression("account");
                queryKHDN.ColumnSet.AllColumns = true;
                if (nameCompany != "") queryKHDN.Criteria.AddCondition("bsd_name", ConditionOperator.Equal,""+ nameCompany + "");
                if (registrationCode != "") queryKHDN.Criteria.AddCondition("bsd_registrationcode", ConditionOperator.Equal,""+ registrationCode + "");
                if (registrationCode == "") return;
                if (telephone != "") queryKHDN.Criteria.AddCondition("telephone1", ConditionOperator.Equal,""+ telephone + "");
                if (email != "") queryKHDN.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal,""+ email + "");
                queryKHDN.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                //throw new InvalidPluginExecutionException(nameCompany + "||" + registrationCode + "||" + telephone  + "||" + email);
                EntityCollection result = service.RetrieveMultiple(queryKHDN);
                if (result != null && result.Entities != null && result.Entities.Count > 0)
                {
                    for (int i = 0; i < result.Entities.Count; i++)
                    {
                        Entity item = result.Entities[i];
                        html += $@"
                        <tr>
                            <td><input type='checkbox' name='choose-ckb' data-id='{item.Id}' /></td>
                            <td>{i + 1}</td>
                            <td>{getValueXML(item, "bsd_name")}</td>
                            <td>{(item.Contains("bsd_chudautu") ? item.FormattedValues["bsd_chudautu"] : "")}</td>
                            <td>{getValueXML(item, "bsd_address")}</td>
                            <td>{getValueXML(item, "telephone1")}</td>
                            <td>{getValueXML(item, "emailaddress1")}</td>
                        </tr>
                    ";
                    }
                }
            }

            context.OutputParameters["result"] = html;
        }

        private string getValueXML(Entity item, string logicalName)
        {
            return (item.Contains(logicalName) ? (string)item[logicalName] : "");
        }
    }
}
