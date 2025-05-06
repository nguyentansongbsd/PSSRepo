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
        public void Execute(IServiceProvider serviceProvider)
        {
            // Lấy context
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string idEmailMessage= context.InputParameters["id"].ToString();
            tracingService.Trace(idEmailMessage);
            Entity enEmailMessage = service.Retrieve("email", new Guid(idEmailMessage), new ColumnSet(true));
            GetMail_To_CC_BCC(idEmailMessage);
            tracingService.Trace("ok");
            context.OutputParameters["mailFrom"] = mailFrom;
            context.OutputParameters["mailTo"] = mailTo;
            context.OutputParameters["mailCC"] = mailCC;
            context.OutputParameters["mailBCC"] = mailBCC;
            context.OutputParameters["bodymail"] = enEmailMessage["description"].ToString();
            context.OutputParameters["subject"] = enEmailMessage["subject"].ToString();
            context.OutputParameters["fileNameAttach"] = enEmailMessage["subject"].ToString().Replace("/","-")+".pdf";
            //Entity enUpdate=new Entity(enEmailMessage.LogicalName, enEmailMessage.Id); enUpdate["statuscode"] = new OptionSetValue(6);
            //service.Update(enUpdate);
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
            foreach(var item in rs.Entities)
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

    }
}
