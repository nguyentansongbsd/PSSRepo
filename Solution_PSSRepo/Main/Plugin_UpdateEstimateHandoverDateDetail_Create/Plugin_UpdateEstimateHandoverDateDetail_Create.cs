using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Plugin_UpdateEstimateHandoverDateDetail_Create
{
    public class Plugin_UpdateEstimateHandoverDateDetail_Create : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Khởi tạo các service cần thiết
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            // Kiểm tra InputParameters có chứa Target và Target có phải là một Entity không
            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
            {
                tracingService.Trace("Plugin exiting: Target is missing or not an Entity.");
                return;
            }

            Entity entity = (Entity)context.InputParameters["Target"];

            try
            {
                tracingService.Trace($"Plugin start for {entity.LogicalName} with Id: {entity.Id}");

                // 1. Lấy ra EntityReference của Sales Order (Hợp đồng) từ field 'bsd_optionentry'.
                if (!entity.Contains("bsd_optionentry"))
                {
                    tracingService.Trace("Field 'bsd_optionentry' not found on the target entity. Plugin will exit.");
                    return; // Không làm gì nếu field không tồn tại.
                }

                EntityReference salesOrderRef = entity.GetAttributeValue<EntityReference>("bsd_optionentry");
                if (salesOrderRef == null)
                {
                    tracingService.Trace("Field 'bsd_optionentry' is null. Plugin will exit.");
                    return; // Không làm gì nếu lookup rỗng.
                }

                // 2. Lấy thông tin của Sales Order để kiểm tra.
                // Chỉ lấy field 'bsd_tobeterminated' để tối ưu hiệu năng.
                tracingService.Trace($"Retrieving SalesOrder with ID: {salesOrderRef.Id}");
                Entity salesOrder = service.Retrieve("salesorder", salesOrderRef.Id, new ColumnSet("bsd_tobeterminated"));

                // 3. Kiểm tra field 'bsd_tobeterminated' có giá trị là Yes (true) hay không.
                // GetAttributeValue<bool> sẽ trả về false nếu field không tồn tại hoặc null
                if (salesOrder.GetAttributeValue<bool>("bsd_tobeterminated"))
                {
                    // Nếu giá trị là true (Yes), quăng lỗi và dừng tiến trình.
                    tracingService.Trace("SalesOrder is marked to be terminated. Throwing error.");
                    throw new InvalidPluginExecutionException("Option Entry is currently in the 'To be terminated' status.");
                }

                tracingService.Trace("SalesOrder is not marked for termination. Plugin finished successfully.");
            }
           
            catch (Exception ex)
            {
                tracingService.Trace($"Exception: {ex.Message}");
                throw new InvalidPluginExecutionException(ex.Message, ex);
            }
        }
    }
}
