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
            //bsd_warningnotices

            var fetchData = new
            {
                statuscode = "2"
            };

            var fetchXml = @"";
            var allEntities = new List<Entity>();
            string pagingCookie = null;
            int pageNumber = (int)context.InputParameters["size"];
            #region
            var query_statuscode = 2;
            var query_bsd_numberofwarning = (int)context.InputParameters["wnumber"];

            var query = new QueryExpression("bsd_warningnotices");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, query_statuscode);
            query.Criteria.AddCondition("bsd_numberofwarning", ConditionOperator.Equal, query_bsd_numberofwarning);
            query.AddOrder("createdon", OrderType.Ascending);
            var query_bsd_paymentschemedetail = query.AddLink(
                "bsd_paymentschemedetail",
                "bsd_paymentschemedeitail",
                "bsd_paymentschemedetailid");
            query_bsd_paymentschemedetail.LinkCriteria.AddCondition($"bsd_w_noticesnumber{(int)context.InputParameters["wnumber"]}", ConditionOperator.Null);
            var collection = service.RetrieveMultiple(query);
            allEntities.AddRange(collection.Entities);
            tracingService.Trace(" count: " + allEntities.Count);
            int size = 500;
            if (context.InputParameters.Contains("size"))
            {
                size = (int)context.InputParameters["size"];
            }
            var s = "";
            tracingService.Trace("step1 count: " + allEntities.Count);
            var request = new OrganizationRequest("bsd_Action_Active_SynsImtallment");
            for (int i = 0; i < allEntities.Count; i += size)
            {
                tracingService.Trace("i: " + i);
                s = "";
                var lst = allEntities
                     .Skip(i).Take(size)
                     .Select(x => $"<value>{x.Id.ToString().Replace("{", "").Replace("}", "")}</value>")
                     .ToList();
                s = string.Join("\n", lst);
                request["listid"] = s;
                request["type"] = "warningNo";
                service.Execute(request);
            }
            #endregion
            allEntities = new List<Entity>();
            pagingCookie = null;
            pageNumber = 1;
            if(((int)context.InputParameters["payno_start"])==0) return;

            //bsd_customernotices
            fetchXml = @"<fetch page='{0}' paging-cookie='{1}' count='5000'>
                                      <entity name='bsd_customernotices'>
                                        <filter type='and'>
                                          <condition attribute='statuscode' operator='ne' value='{2}' />
                                        </filter>
                                        <link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_paymentschemedetail'>
                                              <filter>
                                                <condition attribute='bsd_paymentnoticesnumber' operator='null' value='' />
                                              </filter>
                                         </link-entity>
                                        <order attribute='createdon' />
                                      </entity>
                                    </fetch>";

            fetchXml = string.Format(fetchXml, pageNumber,
            pagingCookie == null ? "" : System.Security.SecurityElement.Escape(pagingCookie),
            fetchData.statuscode);
            collection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            allEntities.AddRange(collection.Entities);
            tracingService.Trace(" count: " + allEntities.Count);
            if (collection.MoreRecords)
            {
                pageNumber++;
                pagingCookie = collection.PagingCookie;
            }
            tracingService.Trace("step2: count: " + allEntities.Count);
            for (int i = 0; i < allEntities.Count; i += size)
            {
                tracingService.Trace("i: " + i);
                s = "";
                var lst = allEntities
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
