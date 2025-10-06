using Microsoft.Xrm.Sdk;
using QRCoder;
using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk.Query;
using VietQRHelper;

namespace Plugin_PaymentNotices_CreateQRCode
{
    public class Plugin_PaymentNotices_CreateQRCode : IPlugin
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
            Entity entity = (Entity)context.InputParameters["Target"];
            Entity enTarget = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            tracingService.Trace("start id:" + entity.Id);
            EntityReference enProjectRef = enTarget.GetAttributeValue<EntityReference>("bsd_project");
            Entity enProject= service.Retrieve(enProjectRef.LogicalName,enProjectRef.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            string banknumber = "";
            string bankbin = "";
            // Lấy nội dung cho mã QR từ thông tin Khách hàng và Sản phẩm (Unit)
            string purpose = GetContent(service, enTarget);
            GetBankInfor(enProject, ref banknumber, ref bankbin);
            QRCodeGenerator qrGenerator = new QRCodeGenerator(); 
            
            QRCodeData qrCodeInfo = qrGenerator.CreateQrCode(GenerateVietQRpayload(bankbin,banknumber,purpose), QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeInfo);
            byte[] qrCodeImageBytes = qrCode.GetGraphic(20); // Kích thước pixel của mỗi module QR

            // Chuyển đổi ảnh QR (dạng byte array) sang chuỗi Base64 để lưu vào trường text.
            string qrCodeBase64 = Convert.ToBase64String(qrCodeImageBytes);
            tracingService.Trace("Plugin_Booking_GenQRcode: Đã tạo mã QR và chuyển sang Base64.");
            byte[] imageBytes = Convert.FromBase64String(qrCodeBase64);
            // Cập nhật lại bản ghi vừa tạo với mã QR.
            Entity entityToUpdate = new Entity(enTarget.LogicalName, enTarget.Id);
            entityToUpdate["bsd_qrcode"] = imageBytes; // Đảm bảo trường này có kiểu 'Multiple Lines of Text'.
            service.Update(entityToUpdate);
        }
        public void GetBankInfor(Entity enproject,ref string banknumber,ref string bankbin)
        {
            // Giả định tên logic của các thực thể và trường.
            // Bạn cần thay đổi cho phù hợp với hệ thống của bạn.
            string bankAccountEntityName = "bsd_projectbankaccount";
            string projectLookupFieldOnBankAccount = "bsd_project";
            string isDefaultField = "bsd_default";
            string bankAccountNumberField = "bsd_name";

            string bankLookupFieldOnBankAccount = "bsd_bank";
            string bankEntityName = "bsd_bank";
            string bankBinField = "new_bankcode";
            string bankAlias = "bank";

            QueryExpression query = new QueryExpression(bankAccountEntityName);
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition(projectLookupFieldOnBankAccount, ConditionOperator.Equal, enproject.Id);
            query.Criteria.AddCondition(isDefaultField, ConditionOperator.Equal, true); // Lấy bank account mặc định

            // Liên kết đến thực thể Bank để lấy Bank BIN
            LinkEntity linkEntity = new LinkEntity(bankAccountEntityName, bankEntityName, bankLookupFieldOnBankAccount, "bsd_bankid", JoinOperator.Inner);
            linkEntity.Columns.AddColumn(bankBinField);
            linkEntity.EntityAlias = bankAlias;
            query.LinkEntities.Add(linkEntity);

            EntityCollection results = service.RetrieveMultiple(query);

            if (results.Entities.Count > 0)
            {
                Entity defaultBankAccount = results.Entities.First();

                // Lấy banknumber từ Bank Account
                if (defaultBankAccount.Contains(bankAccountNumberField))
                {
                    banknumber = defaultBankAccount.GetAttributeValue<string>(bankAccountNumberField);
                }

                // Lấy bankbin từ thực thể Bank đã liên kết
                if (defaultBankAccount.Contains($"{bankAlias}.{bankBinField}"))
                {
                    bankbin = (string)((AliasedValue)defaultBankAccount[$"{bankAlias}.{bankBinField}"]).Value;
                }
            }
        }

        /// <summary>
        /// Lấy nội dung thanh toán từ thông tin Khách hàng và Sản phẩm (Unit).
        /// </summary>
        /// <param name="service">Đối tượng IOrganizationService.</param>
        /// <param name="target">Entity Payment Notice.</param>
        /// <returns>Chuỗi nội dung theo định dạng "TenKhachHang-TenSanPham".</returns>
        public string GetContent(IOrganizationService service, Entity target)
        {
            string customerName = "KHACH HANG"; // Giá trị mặc định
            string unitName = "SAN PHAM"; // Giá trị mặc định
            string projectname = "ProjectName";
            string ordernumber = "0";
            // 1. Lấy thông tin khách hàng (Contact/Account)
            if (target.Contains("bsd_customer"))
            {
                EntityReference customerRef = target.GetAttributeValue<EntityReference>("bsd_customer");
                if (customerRef != null)
                {
                    Entity customer = service.Retrieve(customerRef.LogicalName, customerRef.Id, new ColumnSet(true));
                    // Lấy tên tùy thuộc vào loại thực thể là contact hay account
                    customerName = customer.LogicalName == "contact" ? customer.GetAttributeValue<string>("fullname") : customer.GetAttributeValue<string>("name");
                }
            }

            // 2. Lấy thông tin sản phẩm (Unit)
            if (target.Contains("bsd_units"))
            {
                EntityReference unitRef = target.GetAttributeValue<EntityReference>("bsd_units");
                if (unitRef != null)
                {
                    Entity unit = service.Retrieve(unitRef.LogicalName, unitRef.Id, new ColumnSet("name"));
                    unitName = unit.GetAttributeValue<string>("name");
                }
            }
            if(target.Contains("bsd_project"))
            {
                EntityReference projectRef = target.GetAttributeValue<EntityReference>("bsd_project");
                if (projectRef != null)
                {
                    Entity project = service.Retrieve(projectRef.LogicalName, projectRef.Id, new ColumnSet("bsd_name"));
                    projectname = project.GetAttributeValue<string>("bsd_name");
                }
            }
            if (target.Contains("bsd_paymentschemedetail"))
            {
                EntityReference insRef = target.GetAttributeValue<EntityReference>("bsd_paymentschemedetail");
                if (insRef != null)
                {
                    Entity ins = service.Retrieve(insRef.LogicalName, insRef.Id, new ColumnSet("bsd_ordernumber"));
                    ordernumber = ins.GetAttributeValue<int>("bsd_ordernumber").ToString();
                }
            }
            // 3. Chuyển tên khách hàng về dạng không dấu và viết hoa
            string finalCustomerName = RemoveDiacritics(customerName ?? "").ToUpper();
            string finalUnitName = (unitName ?? "").ToUpper();
            
            // 5. Kết hợp và trả về kết quả
            return $"{finalUnitName}_{projectname}_{finalCustomerName}_Thanh toan dot {ordernumber}";
        }

        /// <summary>
        /// Chuyển đổi chuỗi có dấu thành không dấu.
        /// </summary>
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        private static string GenerateVietQRpayload(string bankBin, string bankNumber, string purpose)
        {
            // Khởi tạo đối tượng VietQR và truyền vào các tham số
            var qrPay = QRPay.InitVietQR(
                bankBin: bankBin,
                bankNumber: bankNumber,
                purpose: purpose
            );

            // Xây dựng chuỗi VietQR và trả về
            return qrPay.Build();
        }

    }
}
