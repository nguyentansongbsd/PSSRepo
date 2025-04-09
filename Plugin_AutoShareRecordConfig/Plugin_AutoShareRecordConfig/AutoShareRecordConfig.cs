using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugin_AutoShareRecordConfig
{
    public class AutoShareRecordConfig : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            #region  khai báo thư viện
            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            #endregion
            if (context.MessageName == "Create" || context.MessageName == "Update")
            {
                Entity target = (Entity)context.InputParameters["Target"];
                var currEnt = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                EntityReference ettrProject = currEnt.Contains("bsd_project") ? (EntityReference)currEnt["bsd_project"] : null;
                string strEntityLogical = currEnt.Contains("bsd_entitylogical") ? (String)currEnt["bsd_entitylogical"] : "";
                if (ettrProject != null && strEntityLogical != "")
                {
                    QueryExpression Query = new QueryExpression("bsd_autosharerecordconfig");
                    Query.ColumnSet = new ColumnSet(true);
                    #region điều kiện cần
                    FilterExpression filterM = new FilterExpression(LogicalOperator.And);
                    filterM.AddCondition(new ConditionExpression("bsd_entitylogical", ConditionOperator.Equal, strEntityLogical));
                    filterM.AddCondition(new ConditionExpression("bsd_project", ConditionOperator.Equal, ettrProject.Id));
                    filterM.AddCondition(new ConditionExpression("bsd_type", ConditionOperator.Equal, ((OptionSetValue)currEnt["bsd_type"]).Value));
                    #endregion
                    #region team
                    var vTeam = currEnt.Contains("bsd_team") ? (EntityReference)currEnt["bsd_team"] : null;
                    if (vTeam != null)
                        filterM.AddCondition(new ConditionExpression("bsd_team", ConditionOperator.Equal, ((EntityReference)currEnt["bsd_team"]).Id));
                    #endregion
                    #region user
                    var vUser = currEnt.Contains("bsd_user") ? (EntityReference)currEnt["bsd_user"] : null;
                    if (vUser != null)
                        filterM.AddCondition(new ConditionExpression("bsd_user", ConditionOperator.Equal, ((EntityReference)currEnt["bsd_user"]).Id));
                    #endregion
                    Query.Criteria = filterM;
                    EntityCollection entcRecordConf = service.RetrieveMultiple(Query);
                    trace.Trace(entcRecordConf.Entities.Count.ToString());
                    if ((entcRecordConf.Entities.Count > 1 && context.MessageName == "Create") || (entcRecordConf.Entities.Count > 1 && context.MessageName == "Update"))
                    {
                        throw new InvalidPluginExecutionException("Your current setup has duplicated the data. Please check again!");
                    }
                }
            }
        }
    }
}
