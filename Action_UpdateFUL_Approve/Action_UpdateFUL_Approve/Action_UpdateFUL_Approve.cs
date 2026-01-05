using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Action_UpdateFUL_Approve
{
    public class Action_UpdateFUL_Approve : IPlugin
    {
        public static IOrganizationService service = null;
        static IOrganizationServiceFactory factory = null;
        public static ITracingService traceService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            string inCase = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["inCase"]))
            {
                inCase = context.InputParameters["inCase"].ToString();
            }
            string idUser = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["idUser"]))
            {
                idUser = context.InputParameters["idUser"].ToString();
            }
            string idOE = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["idOE"]))
            {
                idOE = context.InputParameters["idOE"].ToString();
            }
            string idDetail = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["idDetail"]))
            {
                idDetail = context.InputParameters["idDetail"].ToString();
            }
            string inValue = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["inValue"]))
            {
                inValue = context.InputParameters["inValue"].ToString();
            }
            if (inCase == "Case1" && idOE != "" && inValue != "" && idUser != "")
            {
                traceService.Trace("Case1");
                service = factory.CreateOrganizationService(Guid.Parse(idUser));
                Entity enOE = new Entity("salesorder");
                enOE.Id = Guid.Parse(idOE);
                enOE["bsd_tobeterminated"] = inValue;
                service.Update(enOE);
            }
            else if (inCase == "Case2" && idDetail != "" && idUser != "")
            {
                traceService.Trace("Case1");
                service = factory.CreateOrganizationService(Guid.Parse(idUser));
                Entity enDetail = new Entity("bsd_updatefuldetail");
                enDetail.Id = Guid.Parse(idDetail);
                enDetail["bsd_error"] = null;
                enDetail["statuscode"] = new OptionSetValue(100000001);
                service.Update(enDetail);
            }
            else if (inCase == "Case3" && idDetail != "" && inValue != "" && idUser != "")
            {
                traceService.Trace("Case1");
                service = factory.CreateOrganizationService(Guid.Parse(idUser));
                Entity enDetail = new Entity("bsd_updatefuldetail");
                enDetail.Id = Guid.Parse(idDetail);
                enDetail["bsd_error"] = inValue;
                service.Update(enDetail);
            }
        }
    }
}