using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_CreateEmailMessage
{
    public class Action_CreateEmailMessage : IPlugin
    {
        Entity entityMain = null;
        Entity enCus = null;
        Entity enEmailTemplate = null;
        Entity enProject = null;
        Entity enUnit = null;
        Entity enUserAction = null;
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Lấy context
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            try
            {
                // Lấy các tham số đầu vào
                string entityIdBulkSendMail = context.InputParameters.Contains("entityIdBulkSendMail") ? context.InputParameters["entityIdBulkSendMail"].ToString() : "";
                string entityName = context.InputParameters["entityName"].ToString().Substring(0, context.InputParameters["entityName"].ToString().Length - 1);
                Guid entityMainId = new Guid(context.InputParameters["entityMainId"].ToString());
                string base64FileAttach = context.InputParameters["base64FileAttach"].ToString();
                entityMain = service.Retrieve(entityName, entityMainId, new ColumnSet(true));
                var enProjectRef = (EntityReference)entityMain["bsd_project"];
                enProject = service.Retrieve("bsd_project", enProjectRef.Id, new ColumnSet(true));
                enUserAction = service.Retrieve("systemuser", new Guid(context.InputParameters["userAction"].ToString()), new ColumnSet(true));
                tracingService.Trace("step0");
                enCus = GetEntityCustomer();
                tracingService.Trace("step1");
                enProject = GetEntityProject();
                tracingService.Trace("step2");

                enUnit = GetEntityunit(); tracingService.Trace("step3");

                // Tạo một email message mới
                Entity emailMessage = new Entity("email");
                // Thiết lập các thuộc tính cho email message
                emailMessage["description"] = GetEmailTemplate(); // Nội dung email
                emailMessage["bsd_entityname"] = entityName;
                emailMessage["bsd_entityid"] = entityMainId.ToString();
                emailMessage["bsd_emailcreator"] = enUserAction.ToEntityReference();
                if (entityIdBulkSendMail == "")
                {
                    var enBulkSendManager = new Entity("bsd_bulksendmailmanager");
                    enBulkSendManager["bsd_project"] = enProjectRef;
                    string name = "";
                    if (entityName == "bsd_payment")
                    {
                        enBulkSendManager["bsd_types"] = new OptionSetValue(100000000);
                        name = "Confirm Payment";
                    }
                    else
                    {
                        enBulkSendManager["bsd_types"] = new OptionSetValue(100000001);
                        name = "Payment Notices";

                    }
                    enBulkSendManager["bsd_name"] = name + $"-{enProject["bsd_name"]}-{DateTime.UtcNow.AddHours(7)}";
                    enBulkSendManager["ownerid"] = enUserAction.ToEntityReference();
                    var id= service.Create(enBulkSendManager).ToString();
                    context.OutputParameters["idBulkSendMail"] = id;
                    entityIdBulkSendMail = id;
                }
                emailMessage["bsd_bulksendmailmanager"] = new EntityReference("bsd_bulksendmailmanager", new Guid(entityIdBulkSendMail));
                tracingService.Trace("step4");
                emailMessage["subject"] = GetSubject(); // Tiêu đề email
                tracingService.Trace("step5");

                emailMessage["ownerid"] = new EntityReference("systemuser", new Guid(context.InputParameters["userAction"] as string));

                MapFromMail(emailMessage);
                tracingService.Trace("step6");

                MapToMail(emailMessage);
                tracingService.Trace("step7");
                MapCC_BCC(emailMessage);
                tracingService.Trace("step8");

                // Thêm email message vào hệ thống
                Guid emailId = service.Create(emailMessage);
                Entity linkedAttachment = new Entity("activitymimeattachment");
                linkedAttachment.Attributes["objectid"] = new EntityReference("email", emailId);
                linkedAttachment.Attributes["objecttypecode"] = "email";
                linkedAttachment.Attributes["filename"] = GenFileNameAttach();
                tracingService.Trace("step9");

                linkedAttachment.Attributes["mimetype"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                linkedAttachment.Attributes["body"] = base64FileAttach;
                service.Create(linkedAttachment);
                tracingService.Trace("step10");

                // Trả về ID của email message đã tạo
                context.OutputParameters["EmailId"] = emailId;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in CreateEmailMessageAction: " + ex.Message);
            }
        }
        private string GetEmailTemplate()
        {
            var query_title = "Comfirm Payment";
            switch (entityMain.LogicalName)
            {
                case "bsd_payment":
                    query_title = "Comfirm Payment";
                    break;
                default:
                    query_title = "Payment Notice";

                    break;
            }
            var query = new QueryExpression("template");
            query.TopCount = 50; query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("title", ConditionOperator.Equal, query_title);
            var rs = service.RetrieveMultiple(query);
            if (rs.Entities.Count > 0)
            {
                tracingService.Trace("##");
                tracingService.Trace("GetEmailTemplate:" + rs.Entities.Count);
                enEmailTemplate = rs.Entities[0];
                return MapParamMailTemplate(rs.Entities[0]["safehtml"].ToString());
            }
            return "";
        }
        private string MapParamMailTemplate(string mailTemplate)
        {
            switch (entityMain.LogicalName)
            {
                case "bsd_payment":
                    mailTemplate = mailTemplate.Replace("{fullname}", GetFullNameCustomer()).Replace("{sign_mail}", GetSignMail());
                    break;
                default:
                    break;

            }
            return mailTemplate;
        }
        private Entity GetEntityCustomer()
        {
            EntityReference entityReference = null;
            Entity entity = null;
            switch (entityMain.LogicalName)
            {
                case "bsd_payment":
                    entityReference = (EntityReference)entityMain["bsd_purchaser"];
                    entity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    break;
                default:
                    entityReference = (EntityReference)entityMain["bsd_customer"];
                    entity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    break;
            }
            return entity;
        }
        private Entity GetEntityunit()
        {
            EntityReference entityReference = null;
            Entity entity = null;
            switch (entityMain.LogicalName)
            {
                case "bsd_payment":
                    entityReference = (EntityReference)entityMain["bsd_units"];
                    entity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    break;
                default:
                    entityReference = (EntityReference)entityMain["bsd_units"];
                    entity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    break;
            }
            return entity;
        }
        private Entity GetEntityProject()
        {
            EntityReference entityReference = null;
            Entity entity = null;
            switch (entityMain.LogicalName)
            {
                case "bsd_payment":
                    entityReference = (EntityReference)entityMain["bsd_project"];
                    entity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    break;
                default:
                    entityReference = (EntityReference)entityMain["bsd_project"];
                    entity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    break;
            }
            return entity;
        }
        private string GetSubject()
        {
            tracingService.Trace("****1"+ enEmailTemplate["subjectsafehtml"].ToString());
            tracingService.Trace("****1" + enUnit["name"].ToString());
            tracingService.Trace("****1" + enProject["bsd_name"].ToString());

            string subject = "";
            switch (entityMain.LogicalName)
            {
                case "bsd_payment":
                    tracingService.Trace(enEmailTemplate.Id.ToString());
                    subject = enEmailTemplate["subjectsafehtml"].ToString().Replace("{project}", enProject["bsd_name"].ToString()).Replace("{unitname}", enUnit["name"].ToString());
                    break;
                default:
                    tracingService.Trace(enEmailTemplate.Id.ToString());
                    subject = enEmailTemplate["subjectsafehtml"].ToString().Replace("{project}", enProject["bsd_name"].ToString()).Replace("{unitname}", enUnit["name"].ToString());
                    break;
            }
            tracingService.Trace(subject);
            return subject;

        }
        private void MapCC_BCC(Entity entity)
        {
            var query_bsd_project = enProject.Id.ToString();

            var query = new QueryExpression("bsd_listmailcc");
            query.TopCount = 50; query.ColumnSet.AddColumns("bsd_usersystem", "bsd_project", "bsd_type");
            query.Criteria.AddCondition("bsd_project", ConditionOperator.Equal, query_bsd_project);
            var rs = service.RetrieveMultiple(query);
            var lstBcc = new List<Entity>();
            var lstCc = new List<Entity>();
            foreach (var item in rs.Entities)
            {
                var userSystem = service.Retrieve("systemuser", ((EntityReference)item["bsd_usersystem"]).Id, new ColumnSet(true));
                if (((OptionSetValue)item["bsd_type"]).Value == 100000001)
                {
                    Entity fromparty = new Entity("activityparty");
                    fromparty["addressused"] = userSystem["internalemailaddress"].ToString();
                    fromparty["partyid"] = item["bsd_usersystem"];
                    lstBcc.Add(fromparty);
                }
                else
                {
                    Entity fromparty = new Entity("activityparty");
                    fromparty["addressused"] = userSystem["internalemailaddress"].ToString();
                    fromparty["partyid"] = item["bsd_usersystem"];
                    lstCc.Add(fromparty);
                }
            }
            if (lstBcc.Count > 0)
            {
                entity["bcc"] = lstBcc.ToArray();
            }
            if (lstCc.Count > 0)
            {
                entity["cc"] = lstCc.ToArray();
            }

        }
        private void MapFromMail(Entity entity)
        {
            var userSystem = service.Retrieve("systemuser", ((EntityReference)enProject["bsd_senderconfigsystem"]).Id, new ColumnSet(true));
            Entity fromparty = new Entity("activityparty");
            fromparty["addressused"] = userSystem["internalemailaddress"].ToString();
            fromparty["partyid"] = enProject["bsd_senderconfigsystem"];
            entity["from"] = new Entity[] { fromparty };
        }
        private void MapToMail(Entity entity)
        {
            Entity toparty = new Entity("activityparty");
            if (enCus.LogicalName == "contact")
            {
                toparty["addressused"] = enCus.Contains("bsd_email2") ? enCus["bsd_email2"].ToString() : (enCus.Contains("emailaddress11") ? enCus["emailaddress11"].ToString() : (enCus.Contains("emailaddress1") ? enCus["emailaddress1"].ToString() : null));
            }
            else
            {
                toparty["addressused"] = enCus.Contains("bsd_email2") ? enCus["bsd_email2"].ToString() : (enCus.Contains("emailaddress11") ? enCus["emailaddress11"].ToString() : (enCus.Contains("emailaddress1") ? enCus["emailaddress1"].ToString() : null));
            }
            toparty["partyid"] = new EntityReference(enCus.LogicalName, enCus.Id);
            entity["to"] = new Entity[] { toparty };
        }
        private string GenFileNameAttach()
        {
            return enProject["bsd_name"].ToString() + "_ConfirmPayment_" + enUnit["name"].ToString() + ".docx";
        }
        private string GetFullNameCustomer()
        {
            string fullname = "";
            switch (enCus.LogicalName)
            {
                case "contact":
                    fullname = enCus["bsd_fullname"].ToString();
                    break;
                case "account":
                    fullname = enCus["bsd_name"].ToString();
                    break;
                default:
                    break;
            }
            return fullname;
        }
        private string GetSignMail()
        {
            var query = new QueryExpression("emailsignature");
            query.TopCount = 50; query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, enUserAction.Id.ToString());
            var rs = service.RetrieveMultiple(query);
            if (rs.Entities.Count > 0)
            {
                return rs.Entities[0]["safehtml"].ToString();
            }
            else return "";

        }
    }
}
