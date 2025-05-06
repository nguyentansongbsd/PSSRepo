using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IdentityModel.Metadata;
using System.Web.UI.WebControls;

namespace Action_BulkChangeManaFee_Approve
{
    public class Action_BulkChangeManaFee_Approve : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService TracingSe = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            string input01 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input01"]))
            {
                input01 = context.InputParameters["input01"].ToString();
            }
            string input02 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input02"]))
            {
                input02 = context.InputParameters["input02"].ToString();
            }
            string input03 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input03"]))
            {
                input03 = context.InputParameters["input03"].ToString();
            }
            string input04 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input04"]))
            {
                input04 = context.InputParameters["input04"].ToString();
            }
            if (input01 == "Bước 01" && input02 != "")
            {
                TracingSe.Trace("Bước 01");
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch top=""1"">
                  <entity name=""bsd_bulkchangemanagementfeedetail"">
                    <attribute name=""bsd_bulkchangemanagementfeedetailid"" />
                    <filter>
                      <condition attribute=""bsd_bulkchangemanagementfee"" operator=""eq"" value=""{input02}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs.Entities.Count == 0) throw new InvalidPluginExecutionException("The list of Bulk Change Management Fee to be processed is currently empty. Please check again.");
                Entity enTarget = new Entity("bsd_bulkchangemanagementfee");
                enTarget.Id = Guid.Parse(input02);
                enTarget["bsd_powerautomate"] = true;
                service.Update(enTarget);
                context.OutputParameters["output01"] = context.UserId.ToString();
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Bulk Change Management Fee Approve");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                context.OutputParameters["output02"] = url;
            }
            else if (input01 == "Bước 02" && input02 != "" && input03 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity en_detail = service.Retrieve("bsd_bulkchangemanagementfeedetail", Guid.Parse(input03), new ColumnSet(true));
                int bsd_numberofmonthspaidmfnew = (int)en_detail["bsd_numberofmonthspaidmfnew"];
                decimal bsd_managementamountmonth_new = ((Money)en_detail["bsd_managementamountmonth_new"]).Value;
                decimal bsd_managementfeenew = ((Money)en_detail["bsd_managementfeenew"]).Value;
                TracingSe.Trace("checkInput");
                if (en_detail.Contains("bsd_installment"))
                {
                    EntityReference enInstall = (EntityReference)en_detail["bsd_installment"];
                    if (checkFeeInstall(enInstall.Id)) throw new InvalidPluginExecutionException("The management fee has been paid. Please check again.");
                    service.Update(new Entity(enInstall.LogicalName)
                    {
                        Id = enInstall.Id,
                        ["bsd_managementamount"] = new Money(bsd_managementfeenew)
                    });
                }
                if (en_detail.Contains("bsd_optionentry"))
                {
                    EntityReference enOE = (EntityReference)en_detail["bsd_optionentry"];
                    service.Update(new Entity(enOE.LogicalName)
                    {
                        Id = enOE.Id,
                        ["bsd_numberofmonthspaidmf"] = bsd_numberofmonthspaidmfnew,
                        ["bsd_managementfee"] = new Money(bsd_managementfeenew)
                    });
                }
                if (en_detail.Contains("bsd_units"))
                {
                    EntityReference enUnit = (EntityReference)en_detail["bsd_units"];
                    service.Update(new Entity(enUnit.LogicalName)
                    {
                        Id = enUnit.Id,
                        ["bsd_managementamountmonth"] = new Money(bsd_managementamountmonth_new),
                        ["bsd_numberofmonthspaidmf"] = new Money(bsd_numberofmonthspaidmfnew)
                    });
                }
                service.Update(new Entity(en_detail.LogicalName)
                {
                    Id = en_detail.Id,
                    ["statuscode"] = new OptionSetValue(100000000),
                    ["bsd_error"] = null
                });
            }
            else if (input01 == "Bước 03" && input02 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enTarget = new Entity("bsd_bulkchangemanagementfee");
                enTarget.Id = Guid.Parse(input02);
                enTarget["bsd_powerautomate"] = false;
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch top=""1"">
                  <entity name=""bsd_bulkchangemanagementfeedetail"">
                    <attribute name=""bsd_bulkchangemanagementfeedetailid"" />
                    <filter>
                      <condition attribute=""bsd_bulkchangemanagementfee"" operator=""eq"" value=""{input02}"" />
                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000000}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs.Entities.Count > 0)
                {
                    enTarget["statuscode"] = new OptionSetValue(100000000);
                    enTarget["bsd_approvedrejecteddate"] = DateTime.Now;
                    enTarget["bsd_approvedrejectedperson"] = new EntityReference("systemuser", Guid.Parse(input04));
                }
                service.Update(enTarget);
            }
        }
        EntityCollection RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            q.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            EntityCollection entc = service.RetrieveMultiple(q);

            return entc;
        }
        private bool checkFeeInstall(Guid id)
        {
            bool output = false;
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_paymentschemedetail"">
                <attribute name=""bsd_paymentschemedetailid"" />
                <attribute name=""bsd_managementfeesstatus"" />
                <filter>
                  <condition attribute=""bsd_paymentschemedetailid"" operator=""eq"" value=""{id}"" />
                  <condition attribute=""bsd_managementfeesstatus"" operator=""eq"" value=""{1}"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs.Entities.Count > 0) output = true;
            return output;
        }
    }
}
