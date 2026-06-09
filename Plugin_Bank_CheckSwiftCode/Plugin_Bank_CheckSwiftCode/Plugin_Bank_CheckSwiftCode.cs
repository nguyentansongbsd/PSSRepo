using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Bank_CheckSwiftCode
{
    public class Plugin_Bank_CheckSwiftCode : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Khởi tạo các dịch vụ cần thiết
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService organizationService = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(new Guid("D90CE220-655A-E811-812E-3863BB36DC00"));

            // Lấy bản ghi liên hệ từ InputParameters
            Entity inputParameter = pluginExecutionContext.InputParameters["Target"] as Entity;
            tracingService.Trace("1");
            Entity entity = organizationService.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(true));
            tracingService.Trace("2");
            tracingService.Trace(entity.Id.ToString());
            tracingService.Trace("MessageName" + pluginExecutionContext.MessageName);

            // Kiểm tra nếu là hành động "Update"
            string additionalCondition = "";
            if (pluginExecutionContext.MessageName == "Update")
            {
                additionalCondition = "<condition attribute='bsd_bankid' operator='ne' value='" + entity.Id.ToString() + "' />";
                tracingService.Trace(additionalCondition);
            }
            else
            {
                additionalCondition = "<condition attribute='bsd_bankid' operator='ne' value='" + entity.Id.ToString() + "' />";
                tracingService.Trace(additionalCondition);
            }

            if (!inputParameter.Contains("bsd_swiftcode"))
                return;

            // Chuẩn hóa giá trị bsd_identitycardnumber và bsd_passport
            string bsd_swiftcode = inputParameter.Contains("bsd_swiftcode") ? ((string)inputParameter["bsd_swiftcode"]).Replace(" ", "") : entity.Contains("bsd_swiftcode") ? ((string)entity["bsd_swiftcode"]).Replace(" ", "") : null;

            if (bsd_swiftcode != null)
            {
                tracingService.Trace(bsd_swiftcode);
                inputParameter["bsd_swiftcode"] = bsd_swiftcode;
                // Xây dựng truy vấn fetch XML cho bsd_identitycardnumber
                string fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                          "<entity name='bsd_bank'>" +
                                          "<filter type='and'>" +
                                          "<condition attribute='bsd_swiftcode' operator='eq' value='" + bsd_swiftcode + "' />" +
                                          additionalCondition +
                                          "<condition attribute='statecode' operator='eq' value='0' />" +
                                          "</filter>" +
                                          "</entity>" +
                                          "</fetch>";
                tracingService.Trace(fetchXml);
                // Thực thi truy vấn fetch XML cho bsd_identitycardnumber
                EntityCollection entityCollectionIdNumber = organizationService.RetrieveMultiple(new FetchExpression(fetchXml));
                if (entityCollectionIdNumber.Entities.Count > 0)
                    throw new InvalidPluginExecutionException("Swift Code not allow duplicate");
            }
        }
    }
}
