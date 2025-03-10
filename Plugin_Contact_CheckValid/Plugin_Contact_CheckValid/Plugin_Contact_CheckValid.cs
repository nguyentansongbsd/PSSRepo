using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Contact_CheckValid
{
    public class Plugin_Contact_CheckValid : IPlugin
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
                additionalCondition = "<condition attribute='contactid' operator='ne' value='" + entity.Id.ToString() + "' />";
                tracingService.Trace(additionalCondition);
            }
            else
            {
                additionalCondition = "<condition attribute='contactid' operator='ne' value='" + entity.Id.ToString() + "' />";
                tracingService.Trace(additionalCondition);
            }

            tracingService.Trace("3");
            tracingService.Trace("statecode: " + ((OptionSetValue)entity["statecode"]).Value.ToString());

            if ((!inputParameter.Contains("bsd_identitycardnumber") && !inputParameter.Contains("bsd_passport")) && (!inputParameter.Contains("statecode") || ((OptionSetValue)entity["statecode"]).Value != 1))
                return;

            // Chuẩn hóa giá trị bsd_identitycardnumber và bsd_passport
            string identityCardNumber = inputParameter.Contains("bsd_identitycardnumber") ? ((string)inputParameter["bsd_identitycardnumber"]).Replace(" ", "") : entity.Contains("bsd_identitycardnumber") ? ((string)entity["bsd_identitycardnumber"]).Replace(" ", "") : null;
            string passportNumber = inputParameter.Contains("bsd_passport") ? ((string)inputParameter["bsd_passport"]).Replace(" ", "") : entity.Contains("bsd_passport") ? ((string)entity["bsd_passport"]).Replace(" ", "") : null;

            if (identityCardNumber != null)
            {
                tracingService.Trace(identityCardNumber);
                inputParameter["bsd_identitycardnumber"] = identityCardNumber;

                // Xây dựng truy vấn fetch XML cho bsd_identitycardnumber
                string fetchXmlIdNumber = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                          "<entity name='contact'>" +
                                          "<attribute name='fullname' />" +
                                          "<attribute name='statuscode' />" +
                                          "<attribute name='ownerid' />" +
                                          "<attribute name='mobilephone' />" +
                                          "<attribute name='jobtitle' />" +
                                          "<attribute name='bsd_identitycardnumber' />" +
                                          "<attribute name='gendercode' />" +
                                          "<attribute name='emailaddress1' />" +
                                          "<attribute name='createdon' />" +
                                          "<attribute name='birthdate' />" +
                                          "<attribute name='address1_composite' />" +
                                          "<attribute name='bsd_fullname' />" +
                                          "<attribute name='contactid' />" +
                                          "<order attribute='createdon' descending='true' />" +
                                          "<filter type='and'>" +
                                          "<condition attribute='bsd_identitycardnumber' operator='eq' value='" + identityCardNumber + "' />" +
                                          additionalCondition +
                                          "<condition attribute='statecode' operator='eq' value='0' />" +
                                          "</filter>" +
                                          "</entity>" +
                                          "</fetch>";
                tracingService.Trace(fetchXmlIdNumber);

                // Thực thi truy vấn fetch XML cho bsd_identitycardnumber
                EntityCollection entityCollectionIdNumber = organizationService.RetrieveMultiple(new FetchExpression(fetchXmlIdNumber));

                    if (entityCollectionIdNumber.Entities.Count > 0)
                        throw new InvalidPluginExecutionException("This ID/PP already existed in PSS.");
                
            }

            if (passportNumber != null)
            {
                tracingService.Trace(passportNumber);
                inputParameter["bsd_passport"] = passportNumber;

                // Xây dựng truy vấn fetch XML cho bsd_passport
                string fetchXmlPassport = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                          "<entity name='contact'>" +
                                          "<attribute name='fullname' />" +
                                          "<attribute name='statuscode' />" +
                                          "<attribute name='ownerid' />" +
                                          "<attribute name='mobilephone' />" +
                                          "<attribute name='jobtitle' />" +
                                          "<attribute name='bsd_passport' />" +
                                          "<attribute name='gendercode' />" +
                                          "<attribute name='emailaddress1' />" +
                                          "<attribute name='createdon' />" +
                                          "<attribute name='birthdate' />" +
                                          "<attribute name='address1_composite' />" +
                                          "<attribute name='bsd_fullname' />" +
                                          "<attribute name='contactid' />" +
                                          "<order attribute='createdon' descending='true' />" +
                                          "<filter type='and'>" +
                                          "<condition attribute='bsd_passport' operator='eq' value='" + passportNumber + "' />"+
                                          additionalCondition +
                                          "<condition attribute='statecode' operator='eq' value='0' />" +
                                          "</filter>" +
                                          "</entity>" +
                                          "</fetch>";
                tracingService.Trace(fetchXmlPassport);

                // Thực thi truy vấn fetch XML cho bsd_passport
                EntityCollection entityCollectionPassport = organizationService.RetrieveMultiple(new FetchExpression(fetchXmlPassport));

                // Kiểm tra kết quả và xử lý trùng lặp cho bsd_passport
                
                    if (entityCollectionPassport.Entities.Count > 0)
                        throw new InvalidPluginExecutionException("This ID/PP already existed in PSS.");
                

            }
        }
    }
}
