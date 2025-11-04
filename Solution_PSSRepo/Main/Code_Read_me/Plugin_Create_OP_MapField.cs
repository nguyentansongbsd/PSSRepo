using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//- map bsd_mandatoryprimaryaccount = primarycontactid nếu khách hàng là account
//- map  value SPA Date -> SPA Date String  với định dạng Ví dụ 14/October/2025
namespace Plugin_Create_OP_MapField
{
    public class Class1Plugin_Create_UpdateLandValueUnitsIPluginExecutionContext : IPlugin
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
            Guid recordId = entity.Id;
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            EntityReference enCusRef = (EntityReference)enCreated["customerid"];
            if (enCusRef.LogicalName== "account") 
            {
                Entity enCus = service.Retrieve(enCusRef.LogicalName, enCusRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                Entity enUpdate = new Entity(entity.LogicalName,entity.Id);
                enUpdate["bsd_mandatoryprimaryaccount"] = enCus["primarycontactid"];
                service.Update(enUpdate);
            }
        }
        public string ToDayMonthNameYearString(DateTime dateToFormat)
        {
            // Sử dụng "dd/MMMM/yyyy" để có được định dạng mong muốn.
            // CultureInfo.InvariantCulture đảm bảo tên tháng luôn là tiếng Anh.
            return dateToFormat.ToString("dd/MMMM/yyyy", CultureInfo.InvariantCulture);
        }
    }
}
