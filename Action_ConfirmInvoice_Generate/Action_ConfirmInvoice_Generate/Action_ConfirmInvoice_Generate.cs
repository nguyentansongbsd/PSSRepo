using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Action_ConfirmInvoice_Generate
{
    public class Action_ConfirmInvoice_Generate : IPlugin
    {
        public IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        StringBuilder strMess = new StringBuilder();
        StringBuilder strMess2 = new StringBuilder();
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
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
            if (input01 == "Buoc 01" && input02 != "")
            {
                traceService.Trace("Bước 01");
                Entity enTarget = new Entity("bsd_confirminvoice");
                enTarget.Id = Guid.Parse(input02);
                Entity enMaster = service.Retrieve(enTarget.LogicalName, enTarget.Id, new ColumnSet(new string[4]
                  {
                    "bsd_project",
                    "bsd_invoicetype",
                    "bsd_issuedate",
                    "bsd_createdate"
                  }));
                if (!enMaster.Contains("bsd_project"))
                    throw new InvalidPluginExecutionException("Please input Project!");
                EntityReference project = (EntityReference)enMaster["bsd_project"];
                bool issue_Date = false;
                bool create_Date = false;
                DateTime bsd_issueddate = DateTime.Now;
                DateTime createdon = DateTime.Now;
                if (enMaster.Contains("bsd_issuedate"))
                {
                    issue_Date = true;
                    bsd_issueddate = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)enMaster["bsd_issuedate"]).ToString("MM/dd/yyyy"));
                }
                if (enMaster.Contains("bsd_createdate"))
                {
                    create_Date = true;
                    createdon = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)enMaster["bsd_createdate"]).ToString("MM/dd/yyyy"));
                }
                StringBuilder xml = new StringBuilder();
                xml.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical'>");
                xml.AppendLine("<entity name='bsd_invoice'>");
                xml.AppendLine("<attribute name='bsd_issueddate' />");
                xml.AppendLine("<attribute name='createdon' />");
                xml.AppendLine("<filter type='and'>");
                xml.AppendLine(string.Format("<condition attribute='bsd_project' operator='eq' value='{0}'/>", project.Id));
                xml.AppendLine("<condition attribute='statuscode' operator='in'>");
                xml.AppendLine("<value>1</value>");
                xml.AppendLine("</condition>");
                if (enMaster.Contains("bsd_invoicetype"))
                {
                    xml.AppendLine(string.Format("<condition attribute='bsd_type' operator='eq' value='{0}'/>", ((OptionSetValue)enMaster["bsd_invoicetype"]).Value));
                }
                xml.AppendLine("</filter>");
                xml.AppendLine("</entity>");
                xml.AppendLine("</fetch>");
                EntityCollection unit = service.RetrieveMultiple(new FetchExpression(xml.ToString()));
                List<string> listUnit = new List<string>();
                foreach (Entity item in unit.Entities)
                {
                    bool check = false;
                    if (issue_Date && create_Date && item.Contains("bsd_issueddate") && item.Contains("createdon"))
                    {
                        DateTime paymentactualtime = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)item["bsd_issueddate"]).ToString("MM/dd/yyyy"));
                        DateTime bsd_createdon = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)item["createdon"]).ToString("MM/dd/yyyy"));
                        if (bsd_issueddate == paymentactualtime && createdon == bsd_createdon) check = true;
                    }
                    else if (issue_Date && item.Contains("bsd_issueddate"))
                    {
                        DateTime paymentactualtime = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)item["bsd_issueddate"]).ToString("MM/dd/yyyy"));
                        if (bsd_issueddate == paymentactualtime) check = true;
                    }
                    else if (create_Date && item.Contains("createdon"))
                    {
                        DateTime bsd_createdon = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)item["createdon"]).ToString("MM/dd/yyyy"));
                        if (createdon == bsd_createdon) check = true;
                    }
                    else check = true;
                    if (check) listUnit.Add(item.Id.ToString());
                }
                if (listUnit.Count == 0) throw new InvalidPluginExecutionException("The list is empty. Please check again.");
                enTarget["bsd_powerautomate"] = true;
                enTarget["bsd_genarate"] = true;
                enTarget["bsd_list"] = string.Join(";", listUnit);
                service.Update(enTarget);
            }
            else if (input01 == "Buoc 02" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                EntityReference master = new EntityReference("bsd_confirminvoice", Guid.Parse(input02));
                EntityReferenceCollection referenceCollection2 = new EntityReferenceCollection();
                referenceCollection2.Add(new EntityReference("bsd_invoice", Guid.Parse(input03)));
                service.Disassociate(master.LogicalName, master.Id, new Relationship("bsd_ConfirmInvoice_bsd_invoice_bsd_invoice"), referenceCollection2);
            }
            else if (input01 == "Buoc 03" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                EntityReference master = new EntityReference("bsd_confirminvoice", Guid.Parse(input02));
                EntityReference detail = new EntityReference("bsd_invoice", Guid.Parse(input03));
                EntityReferenceCollection referenceCollection2 = new EntityReferenceCollection();
                referenceCollection2.Add(detail);
                service.Associate(master.LogicalName, master.Id, new Relationship("bsd_ConfirmInvoice_bsd_invoice_bsd_invoice"), referenceCollection2);
            }
            else if (input01 == "Buoc 04" && input02 != "" && input04 != "")
            {
                traceService.Trace("Bước 04");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enConfirmPayment = new Entity("bsd_confirminvoice");
                enConfirmPayment.Id = Guid.Parse(input02);
                enConfirmPayment["bsd_powerautomate"] = false;
                enConfirmPayment["bsd_genarate"] = false;
                service.Update(enConfirmPayment);
            }
        }
        public DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime)
        {
            int? timeZoneCode = RetrieveCurrentUsersSettings(service);
            if (!timeZoneCode.HasValue)
                throw new InvalidPluginExecutionException("Can't find time zone code");
            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            LocalTimeFromUtcTimeResponse response = (LocalTimeFromUtcTimeResponse)service.Execute(request);
            return response.LocalTime;
            //var utcTime = utcTime.ToString("MM/dd/yyyy HH:mm:ss");
            //var localDateOnly = response.LocalTime.ToString("dd-MM-yyyy");
        }
        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            var currentUserSettings = service.RetrieveMultiple(
            new QueryExpression("usersettings")
            {
                ColumnSet = new ColumnSet("localeid", "timezonecode"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("systemuserid", ConditionOperator.EqualUserId) }
                }
            }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];
        }
    }
}