using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// - map  value SPA Date -> SPA Date String  với định dạng Ví dụ 14/October/2025
namespace Plugin_Create_OP
{
    public class Plugin_Create_Pre_OP : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity entity = (Entity)context.InputParameters["Target"];
           
            tracingService.Trace("start");
            Entity targetEntity = (Entity)context.InputParameters["Target"];
            if (!targetEntity.Contains("bsd_contractdate")) return;
            DateTime spa_Date = targetEntity.GetAttributeValue<DateTime>("bsd_contractdate");
            targetEntity["bsd_spadatestring"] = ToDayMonthNameYearString(spa_Date);


        }
        public string ToDayMonthNameYearString(DateTime dateToFormat)
        {
            // Sử dụng "dd/MMMM/yyyy" để có được định dạng mong muốn.
            // CultureInfo.InvariantCulture đảm bảo tên tháng luôn là tiếng Anh.
            return dateToFormat.ToString("dd/MMMM/yyyy", CultureInfo.InvariantCulture);
        }
    }

}
