using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_FormatString_Input_Selectedrecord
{
    public class Action_FormatString_Input_Selectedrecord : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var listid = context.InputParameters["listid"].ToString().Split(',');
            string res = "";
            foreach(var item in listid)
            {
                if(string.IsNullOrEmpty(item)) 
                    res="\"{item}\"";
                else
                    res+=","+"\"{item}\"";

            }
            context.OutputParameters["res"]=res;
        }
    }
}
