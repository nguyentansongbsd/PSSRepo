using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Action_ConfirmInvoice
{
    public class Action_ConfirmInvoice : IPlugin
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
            string type = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input01"]))
            {
                type = context.InputParameters["input01"].ToString();
            }
            string idRecord = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input02"]))
            {
                idRecord = context.InputParameters["input02"].ToString();
            }
            string idUser = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input03"]))
            {
                idUser = context.InputParameters["input03"].ToString();
            }
            string input = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input04"]))
            {
                input = context.InputParameters["input04"].ToString();
            }
            if (type == "Confirm_Buoc01" && idRecord != "" && idUser != "" && input != "")
            {
                traceService.Trace("Confirm Bước 01");
                service = factory.CreateOrganizationService(Guid.Parse(idUser));
                Entity enTarget = new Entity("bsd_invoice");
                enTarget.Id = Guid.Parse(idRecord);
                enTarget["statuscode"] = new OptionSetValue(int.Parse(input));
                service.Update(enTarget);
            }
            else if (type == "Confirm_Buoc02" && idRecord != "" && idUser != "")
            {
                traceService.Trace("Confirm Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(idUser));
                Entity enTarget = new Entity("bsd_confirminvoice");
                enTarget.Id = Guid.Parse(idRecord);
                enTarget["bsd_submit"] = false;
                enTarget["bsd_powerautomate"] = false;
                service.Update(enTarget);
            }
            else if (type == "Reject_Buoc01" && idRecord != "" && idUser != "" && input != "")
            {
                traceService.Trace("Reject Bước 01");
                service = factory.CreateOrganizationService(Guid.Parse(idUser));
                Entity enTarget = new Entity("bsd_invoice");
                enTarget.Id = Guid.Parse(idRecord);
                enTarget["statuscode"] = new OptionSetValue(int.Parse(input));
                service.Update(enTarget);
            }
            else if (type == "Reject_Buoc02" && idRecord != "" && idUser != "")
            {
                traceService.Trace("Reject Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(idUser));
                Entity enTarget = new Entity("bsd_confirminvoice");
                enTarget.Id = Guid.Parse(idRecord);
                enTarget["bsd_reject"] = false;
                enTarget["bsd_powerautomate"] = false;
                service.Update(enTarget);
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