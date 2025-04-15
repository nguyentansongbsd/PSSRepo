using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Update_Email_MapInforToPayment
{
    /// <summary>
    /// khi email thay đổi trạng thái check field entityname và entity id để cập nhật trạng thái Payments
    /// </summary>
    public class Plugin_Update_Email_MapInforToPayment : IPlugin
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
            Entity entity = (Entity)context.InputParameters["Target"];
            Entity enTarget = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            tracingService.Trace("start id:" + entity.Id);
            if(enTarget.Contains("bsd_entityid"))
            {
                var enMap = service.Retrieve(enTarget["bsd_entityname"].ToString(), new Guid(enTarget["bsd_entityid"].ToString()),new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                var enUpdate=new Entity(enMap.LogicalName,enMap.Id);
                enUpdate["sd_emailstatus"] = enTarget["statecode"];
                enUpdate["bsd_datesent"] = DateTime.Now;
                service.Update(enUpdate);
            }

        }
        
    }
}
