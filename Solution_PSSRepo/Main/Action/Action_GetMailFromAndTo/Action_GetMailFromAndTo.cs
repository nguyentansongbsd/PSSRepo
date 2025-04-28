using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_GetMailFromAndTo
{
    public class Action_GetMailFromAndTo : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        string cusType = "";
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string idFrom = "";
            string idTo = context.InputParameters["idTo"].ToString();
            string entityName = context.InputParameters["entityName"].ToString();
            string entityId = context.InputParameters["entityId"].ToString();
            en = service.Retrieve(entityName,new Guid(entityId),new ColumnSet(true));
            var cus = service.Retrieve("contact", new Guid(idTo), new ColumnSet(true));
            if (cus.Contains("bsd_fullname"))
                cusType = "contact";
            else cusType = "account";
            tracingService.Trace("start");
            string message = "";
            string fieldProject = "";
            tracingService.Trace(entityName);
            switch(entityName)
            {
                case "bsd_payment":
                    fieldProject = "bsd_project";
                    break;
                default:break;
            }    
            tracingService.Trace(((EntityReference)en[fieldProject]).Id.ToString());
            if (CheckMailIsContain(idTo, ((EntityReference)en[fieldProject]).Id.ToString(), ref idFrom,ref message))
            {
                context.OutputParameters["res"] = 1;  
            }    
            else
            {
                context.OutputParameters["res"] = 0;
            }
        }
        public bool CheckMailIsContain( string idTo,string idProject,ref string  idFrom,ref string mess)
        {
            tracingService.Trace("00");
            //check mail dự án
            var query = new QueryExpression("bsd_project");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_projectid", ConditionOperator.Equal, idProject);
            var rs = service.RetrieveMultiple(query);
            if (rs.Entities[0].Contains("bsd_sendermail")==false)
            {

                mess = "not found mail project";
                return false;
            }
            else
            {
                tracingService.Trace("@");
                context.OutputParameters["idQueueFrom"] =((EntityReference) rs.Entities[0]["bsd_sendermail"]).Id.ToString();
            }
            tracingService.Trace("0");
            //check mail khách hàng
            query = new QueryExpression(cusType);
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition(cusType + "id", ConditionOperator.Equal, idTo);
            rs = service.RetrieveMultiple(query);
            tracingService.Trace("1");
            if (cusType == "account")
            {
                if (rs.Entities[0].Contains("emailaddress1"))
                {

                    mess = "not found mail account";
                    return false;
                }
            }
            tracingService.Trace("2");

            if (cusType == "contact")
            {
                if (rs.Entities[0].Contains("emailaddress1"))
                {

                    mess = "not found mail contact";
                    return false;
                }
            }
            tracingService.Trace("3");

            return true;
        }
        public bool CreateRecordError(string message)
        {
            Entity enCreate = new Entity("bsd_errorcreatemail");
            enCreate["bsd_entityid"] = new EntityReference(en.LogicalName, en.Id);
            enCreate["bsd_reason"] = message;
            service.Create(enCreate);
            return false;
        }
    }
}
