using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Action_GetEmailMessage
{
    public class Action_GetEmailMessageAndUpdatePendingSend : IPlugin
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
        string mailCC = "";
        string mailBCC = "";
        string mailTo = "";
        string mailFrom = "";
        Entity enEmailMessage = null;
        string bodyfile = "";
        string filename = "";
        public void Execute(IServiceProvider serviceProvider)
        {
            // Lấy context
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string idEmailMessage = context.InputParameters["id"].ToString();
            tracingService.Trace(idEmailMessage);
            enEmailMessage = service.Retrieve("email", new Guid(idEmailMessage), new ColumnSet(true));

            GetMail_To_CC_BCC(idEmailMessage);
            GetEmailAttachments(idEmailMessage);
            tracingService.Trace("ok");
            context.OutputParameters["mailFrom"] = GetMailForm();
            context.OutputParameters["mailTo"] = mailTo;
            context.OutputParameters["mailCC"] = mailCC;
            context.OutputParameters["mailBCC"] = mailBCC;
            context.OutputParameters["bodymail"] = enEmailMessage["description"].ToString();
            context.OutputParameters["subject"] = enEmailMessage["subject"].ToString();
            context.OutputParameters["fileNameAttach"] = filename;
            context.OutputParameters["bodyfile"] = bodyfile;
            //Entity enUpdate=new Entity(enEmailMessage.LogicalName, enEmailMessage.Id); enUpdate["statuscode"] = new OptionSetValue(6);
            //service.Update(enUpdate);
        }
        private string GetMailForm()
        {
            var regardingobjectid = (EntityReference)enEmailMessage["regardingobjectid"];
            var en = service.Retrieve(regardingobjectid.LogicalName, regardingobjectid.Id, new ColumnSet(true));
            var enProjectRef = new EntityReference();
            var enProject = new Entity();
            var enUserRef = new EntityReference();
            var enUser = new Entity();
            switch (regardingobjectid.LogicalName)
            {
                case "bsd_customernotices":
                    enProjectRef = (EntityReference)en["bsd_project"];
                    enProject = service.Retrieve(enProjectRef.LogicalName, enProjectRef.Id, new ColumnSet(true));
                    enUserRef = (EntityReference)enProject["bsd_senderconfigsystem"];
                    enUser = service.Retrieve(enUserRef.LogicalName, enUserRef.Id, new ColumnSet(true));
                    return enUser["internalemailaddress"].ToString();
                case "bsd_payment":
                    enProjectRef = (EntityReference)en["bsd_project"];
                    enProject = service.Retrieve(enProjectRef.LogicalName, enProjectRef.Id, new ColumnSet(true));
                    enUserRef = (EntityReference)enProject["bsd_senderconfigsystem"];
                    enUser = service.Retrieve(enUserRef.LogicalName, enUserRef.Id, new ColumnSet(true));
                    return enUser["internalemailaddress"].ToString();
            }
            return "";
        }
        private string GetMail_To_CC_BCC(string idEmailMessage)
        {
            var query_activityid = idEmailMessage;

            var query = new QueryExpression("activityparty");
            query.TopCount = 50; query.ColumnSet.AddColumns(
                "activityid",
                "activitypartyid",
                "addressused",
                "addressusedemailcolumnnumber",
                "donotphone",
                "effort",
                "exchangeentryid",
                "externalid",
                "externalidtype",
                "ispartydeleted",
                "participationtypemask",
                "partyid",
                "resourcespecid",
                "scheduledend",
                "scheduledstart",
                "unresolvedpartyname",
                "versionnumber");
            query.Criteria.AddCondition("activityid", ConditionOperator.Equal, query_activityid);
            var rs = service.RetrieveMultiple(query);
            tracingService.Trace(rs.Entities.Count().ToString());
            foreach (var item in rs.Entities)
            {
                tracingService.Trace($"@participationtypemask {((OptionSetValue)item["participationtypemask"]).Value}");
                switch (((OptionSetValue)item["participationtypemask"]).Value)
                {
                    case 1:
                        mailFrom = item["addressused"].ToString();
                        break;
                    case 2:
                        mailTo = item["addressused"].ToString();
                        break;
                    case 3:
                        if (mailCC.Length == 0) mailCC = item["addressused"].ToString();
                        else mailCC += ";" + item["addressused"].ToString();
                        break;
                    case 4:
                        if (mailBCC.Length == 0) mailBCC = item["addressused"].ToString();
                        else mailBCC += ";" + item["addressused"].ToString();
                        break;
                    default:
                        break;
                }
            }
            return "";
        }
        private void GetEmailAttachments(string emailId)
        {
            var query = new QueryExpression("activitymimeattachment")
            {
                ColumnSet = new ColumnSet("filename", "body"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("objectid", ConditionOperator.Equal, emailId),
                        new ConditionExpression("objecttypecode", ConditionOperator.Equal, "email")
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            filename = results.Entities[0].Contains("filename") ? results.Entities[0]["filename"].ToString() : string.Empty;
            bodyfile = results.Entities[0].Contains("body") ? results.Entities[0]["body"].ToString() : null;

        }


    }
}
