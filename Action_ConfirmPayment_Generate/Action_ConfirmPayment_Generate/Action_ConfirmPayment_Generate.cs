using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Action_ConfirmPayment_Generate
{
    public class Action_ConfirmPayment_Generate : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceS = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            traceS = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
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
                traceS.Trace("Bước 01");
                Entity enConfirmPayment = new Entity("bsd_confirmpayment");
                enConfirmPayment.Id = Guid.Parse(input02);
                enConfirmPayment["bsd_powerautomate"] = true;
                enConfirmPayment["bsd_generate"] = true;
                service.Update(enConfirmPayment);
                //context.OutputParameters["output01"] = context.UserId.ToString();
                //string url = "";
                //EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                //    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Confirm Payment Generate");
                //foreach (Entity item in configGolive.Entities)
                //{
                //    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                //}
                //if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                //context.OutputParameters["output02"] = url;
            }
            else if (input01 == "Bước 02" && input02 != "" && input03 != "")
            {
                traceS.Trace("Bước 02");
                EntityReferenceCollection collection = new EntityReferenceCollection();
                var reference = new EntityReference("bsd_payment", Guid.Parse(input03));
                collection.Add(reference); //Create a collection of entity references
                Relationship relationship = new Relationship("bsd_bsd_confirmpayment_bsd_payment"); //schema name of N:N relationship
                service.Disassociate("bsd_confirmpayment", Guid.Parse(input02), relationship, collection); //Pass the entity reference collections to be disassociated from the specific Email Send record
            }
            else if (input01 == "Bước 03" && input02 != "" && input04 != "")
            {
                traceS.Trace("Bước 03");
                Entity enConfirmPayment = service.Retrieve("bsd_confirmpayment", Guid.Parse(input02), new ColumnSet(new string[] { "bsd_project", "bsd_paymenttype", "bsd_user", "bsd_paymentactualtime", "bsd_createddate" }));
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                bool Receipt_Date = false;
                bool Created_Date = false;
                DateTime bsd_paymentactualtime = DateTime.Now;
                DateTime bsd_createddate = DateTime.Now;
                if (enConfirmPayment.Contains("bsd_paymentactualtime"))
                {
                    Receipt_Date = true;
                    bsd_paymentactualtime = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)enConfirmPayment["bsd_paymentactualtime"]).ToString("MM/dd/yyyy"));
                }
                if (enConfirmPayment.Contains("bsd_createddate"))
                {
                    Created_Date = true;
                    bsd_createddate = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)enConfirmPayment["bsd_createddate"]).ToString("MM/dd/yyyy"));
                }
                QueryExpression q = new QueryExpression("bsd_payment");
                q.ColumnSet = new ColumnSet(new string[] { "bsd_paymentid", "bsd_paymentactualtime", "createdon" });
                q.Criteria = new FilterExpression();
                q.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));
                if (enConfirmPayment.Contains("bsd_project")) q.Criteria.AddCondition(new ConditionExpression("bsd_project", ConditionOperator.Equal, ((EntityReference)enConfirmPayment["bsd_project"]).Id));
                if (enConfirmPayment.Contains("bsd_user")) q.Criteria.AddCondition(new ConditionExpression("ownerid", ConditionOperator.Equal, ((EntityReference)enConfirmPayment["bsd_user"]).Id));
                if (enConfirmPayment.Contains("bsd_paymenttype")) q.Criteria.AddCondition(new ConditionExpression("bsd_paymenttype", ConditionOperator.Equal, ((OptionSetValue)enConfirmPayment["bsd_paymenttype"]).Value));
                if (Receipt_Date) q.Criteria.AddCondition(new ConditionExpression("bsd_paymentactualtime", ConditionOperator.NotNull));
                if (Created_Date) q.Criteria.AddCondition(new ConditionExpression("createdon", ConditionOperator.NotNull));
                EntityCollection entc = service.RetrieveMultiple(q);
                List<string> list = new List<string>();
                foreach (Entity item in entc.Entities)
                {
                    bool check = false;
                    if (Receipt_Date && Created_Date && item.Contains("bsd_paymentactualtime") && item.Contains("createdon"))
                    {
                        DateTime paymentactualtime = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)item["bsd_paymentactualtime"]).ToString("MM/dd/yyyy"));
                        DateTime createdon = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)item["createdon"]).ToString("MM/dd/yyyy"));
                        if (bsd_paymentactualtime == paymentactualtime && bsd_createddate == createdon) check = true;
                    }
                    else if (Receipt_Date && item.Contains("bsd_paymentactualtime"))
                    {
                        DateTime paymentactualtime = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)item["bsd_paymentactualtime"]).ToString("MM/dd/yyyy"));
                        if (bsd_paymentactualtime == paymentactualtime) check = true;
                    }
                    else if (Created_Date && item.Contains("createdon"))
                    {
                        DateTime createdon = DateTime.Parse(RetrieveLocalTimeFromUTCTime((DateTime)item["createdon"]).ToString("MM/dd/yyyy"));
                        if (bsd_createddate == createdon) check = true;
                    }
                    else check = true;
                    if (check) list.Add(item.Id.ToString());
                }
                context.OutputParameters["output02"] = string.Join(";", list);
            }
            else if (input01 == "Bước 04" && input02 != "" && input03 != "" && input04 != "")
            {
                traceS.Trace("Bước 04");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                var qe = new QueryExpression("bsd_bsd_confirmpayment_bsd_payment");
                qe.ColumnSet = new ColumnSet("bsd_confirmpaymentid", "bsd_paymentid");
                qe.Criteria.AddCondition("bsd_confirmpaymentid", ConditionOperator.Equal, input02);
                qe.Criteria.AddCondition("bsd_paymentid", ConditionOperator.Equal, input03);
                var exists = service.RetrieveMultiple(qe).Entities.Any();
                if (!exists)
                {
                    EntityReferenceCollection collection = new EntityReferenceCollection();
                    var reference = new EntityReference("bsd_payment", Guid.Parse(input03));
                    collection.Add(reference); //Create a collection of entity references
                    Relationship relationship = new Relationship("bsd_bsd_confirmpayment_bsd_payment"); //schema name of N:N relationship
                    service.Associate("bsd_confirmpayment", Guid.Parse(input02), relationship, collection);
                }

            }
            else if (input01 == "Bước 05" && input02 != "" && input04 != "")
            {
                traceS.Trace("Bước 05");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enConfirmPayment = new Entity("bsd_confirmpayment");
                enConfirmPayment.Id = Guid.Parse(input02);
                enConfirmPayment["bsd_powerautomate"] = false;
                enConfirmPayment["bsd_generate"] = false;
                service.Update(enConfirmPayment);
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
