using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;
using System.Web.UI.WebControls;

namespace Action_PhasesLaunch_Recovery
{
    public class Action_PhasesLaunch_Recovery : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService TracingSe = null;
        StringBuilder strMess = new StringBuilder();
        StringBuilder strMess2 = new StringBuilder();
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
                  <entity name=""product"">
                    <attribute name=""productid"" />
                    <filter>
                      <condition attribute=""bsd_phaseslaunchid"" operator=""eq"" value=""{input02}"" />
                      <condition attribute=""statuscode"" operator=""eq"" value=""{100000004}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs.Entities.Count > 0) throw new InvalidPluginExecutionException("Unit are currently in trading process. If you want to perform recovery action, please use 'Unit Recovery' feature.");
                fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch top=""1"">
                  <entity name=""quote"">
                    <attribute name=""quoteid"" />
                    <filter>
                      <condition attribute=""statuscode"" operator=""in"">
                        <value>{100000000}</value>
                        <value>{100000004}</value>
                        <value>{100000006}</value>
                        <value>{100000007}</value>
                        <value>{3}</value>
                        <value>{4}</value>
                      </condition>
                    </filter>
                    <link-entity name=""product"" from=""productid"" to=""bsd_unitno"">
                      <filter>
                        <condition attribute=""bsd_phaseslaunchid"" operator=""eq"" value=""{input02}"" />
                      </filter>
                    </link-entity>
                  </entity>
                </fetch>";
                rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs.Entities.Count > 0) throw new InvalidPluginExecutionException("Unit are currently in trading process. If you want to perform recovery action, please use 'Unit Recovery' feature.");
                Entity enPhasesLaunch = new Entity("bsd_phaseslaunch");
                enPhasesLaunch.Id = Guid.Parse(input02);
                enPhasesLaunch["bsd_powerautomate"] = true;
                service.Update(enPhasesLaunch);
                context.OutputParameters["output01"] = context.UserId.ToString();
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Phases Launch Recovery");
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
                Entity entity = new Entity("product", Guid.Parse(input03));
                entity["statuscode"] = new OptionSetValue(1);
                entity["bsd_phaseslaunchid"] = null;
                entity["bsd_locked"] = null;
                service.Update(entity);
            }
            else if (input01 == "Bước 03" && input02 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enPhasesLaunch = new Entity("bsd_phaseslaunch");
                enPhasesLaunch.Id = Guid.Parse(input02);
                enPhasesLaunch["bsd_powerautomate"] = false;
                enPhasesLaunch["statuscode"] = new OptionSetValue(100000001);
                service.Update(enPhasesLaunch);
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
            {
                strMess2.AppendLine("Can't find time zone code");
                throw new InvalidPluginExecutionException(strMess2.ToString());
            }
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