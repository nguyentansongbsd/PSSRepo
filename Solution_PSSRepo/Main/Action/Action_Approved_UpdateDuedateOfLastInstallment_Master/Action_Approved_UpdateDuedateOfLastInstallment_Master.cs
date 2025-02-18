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
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            DateTime newDueDate= DateTime.Parse(context.InputParameters["duedatenew"].ToString());
            Entity enDetail = service.Retrieve("bsd_updateduedateoflastinstallment", new Guid(context.InputParameters["detail_id"].ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Entity enDetailUpdate = new Entity(enDetail.LogicalName, enDetail.Id);
            // Status Reason(entity cha) = Approved
            //Approved / Rejected Date[bsd_approvedrejecteddate] = Ngày cập nhật thành công
            //Approved / Rejected Person[bsd_approvedrejectedperson] = Người nhấn nút duyệt
            //Status Reason(entity Detail) = Approved
            //Due Date(New) (ở entity Detail) về field Due Date của installment ở entity Detail
            enDetailUpdate["statuscode"] = new OptionSetValue((int)context.InputParameters["statuscode"]); //Status Reason(entity Detail) = Approved
            service.Update(enDetailUpdate);
            var enInstallmentRef = (EntityReference)enDetail["bsd_lastinstallment"];
            var enInstallment = service.Retrieve(enInstallmentRef.LogicalName, enInstallmentRef.Id, new ColumnSet("bsd_duedate"));
            enInstallment["bsd_duedate"]=newDueDate;
            service.Update(enInstallment);
        }
    }
}
