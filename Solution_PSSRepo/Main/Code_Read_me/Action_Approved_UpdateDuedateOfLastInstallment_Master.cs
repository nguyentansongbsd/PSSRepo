// Plugin này dùng để cập nhật ngày đến hạn (Due Date) của kỳ thanh toán cuối cùng (Last Installment) khi một bản ghi cập nhật ngày đến hạn được duyệt (Approved).
// Khi action được gọi, plugin sẽ:
// 1. Lấy ngày đến hạn mới (duedatenew) và id chi tiết (detail_id) từ InputParameters.
// 2. Lấy bản ghi chi tiết cập nhật ngày đến hạn (bsd_updateduedateoflastinstallment).
// 3. Cập nhật trạng thái (statuscode) của bản ghi chi tiết này sang trạng thái được truyền vào (thường là Approved).
// 4. Lấy tham chiếu đến bản ghi kỳ thanh toán cuối cùng (bsd_lastinstallment) từ bản ghi chi tiết.
// 5. Cập nhật trường ngày đến hạn (bsd_duedate) của bản ghi kỳ thanh toán cuối cùng bằng giá trị ngày đến hạn mới.
// 6. Lưu lại các thay đổi vào hệ thống.

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Approved_UpdateDuedateOfLastInstallment_Master
{
    public class Action_Approved_UpdateDuedateOfLastInstallment_Master : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            // Lấy context, service, tracing từ serviceProvider
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Lấy ngày đến hạn mới từ InputParameters
            DateTime newDueDate = DateTime.Parse(context.InputParameters["duedatenew"].ToString());

            // Lấy bản ghi chi tiết cập nhật ngày đến hạn theo detail_id
            Entity enDetail = service.Retrieve("bsd_updateduedateoflastinstallment", new Guid(context.InputParameters["detail_id"].ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            // Chuẩn bị entity để cập nhật trạng thái
            Entity enDetailUpdate = new Entity(enDetail.LogicalName, enDetail.Id);

            // Cập nhật trường statuscode của bản ghi chi tiết (thường là Approved)
            enDetailUpdate["statuscode"] = new OptionSetValue((int)context.InputParameters["statuscode"]); //Status Reason(entity Detail) = Approved
            service.Update(enDetailUpdate);

            // Lấy tham chiếu đến bản ghi kỳ thanh toán cuối cùng (bsd_lastinstallment)
            var enInstallmentRef = (EntityReference)enDetail["bsd_lastinstallment"];

            // Lấy bản ghi kỳ thanh toán cuối cùng
            var enInstallment = service.Retrieve(enInstallmentRef.LogicalName, enInstallmentRef.Id, new ColumnSet("bsd_duedate"));

            // Cập nhật ngày đến hạn mới cho kỳ thanh toán cuối cùng
            enInstallment["bsd_duedate"] = newDueDate;
            service.Update(enInstallment);
        }
    }
}
