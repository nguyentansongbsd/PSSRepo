using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_GetDataSyncInstallment
{
    public class Action_GetDataSyncInstallment : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var query_statuscode = 2;
            //bsd_warningnotices
            var query = new QueryExpression("bsd_warningnotices");
            query.Distinct = true;
            //query.TopCount = 10;
            query.ColumnSet.AllColumns = true;
            query.ColumnSet.AddColumn("bsd_warningnoticesid");
            query.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, query_statuscode);
            query.AddOrder("createdon", OrderType.Ascending);
            var rs = service.RetrieveMultiple(query);
            int size = 500;
            if(context.InputParameters.Contains("size"))
            {
                size=(int)context.InputParameters["size"];
            }
            var s = "";
            tracingService.Trace("step1");
            var request = new OrganizationRequest("bsd_Action_Active_SynsImtallment");
            for (int i = 0; i < rs.Entities.Count; i += size)
            {
                s = "";
                var lst = rs.Entities
                     .Skip(i).Take(size)
                     .Select(x => $"<value>{x.Id.ToString().Replace("{", "").Replace("}", "")}</value>")
                     .ToList();
                s = string.Join("\n", lst);
                request["listid"] = s;
                request["type"] = "warningNo";
                service.Execute(request);
            }


            //bsd_customernotices
            query = new QueryExpression("bsd_customernotices");
            query.Distinct = true;
            //query.TopCount = 10;
            query.ColumnSet.AllColumns = true;
            query.ColumnSet.AddColumn("bsd_customernoticesid");
            query.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, query_statuscode);
            query.AddOrder("createdon", OrderType.Ascending);
            rs = service.RetrieveMultiple(query);
            for (int i = 0; i < rs.Entities.Count; i += size)
            {
                s = "";
               var lst= rs.Entities
                    .Skip(i).Take(size)
                    .Select(x => $"<value>{x.Id.ToString().Replace("{", "").Replace("}", "")}</value>")
                    .ToList();
                s = string.Join("\n", lst);
                request["listid"] = s;
                request["type"] = "paymentno";
                service.Execute(request);
            }
        }
    }
}
