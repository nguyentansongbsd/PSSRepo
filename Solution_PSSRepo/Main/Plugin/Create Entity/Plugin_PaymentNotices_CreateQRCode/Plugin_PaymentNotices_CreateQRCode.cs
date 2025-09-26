using Microsoft.Xrm.Sdk;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        Entity en = new Entity();
        
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
            string amount = GetValueMoneyTranfer(); // Số tiền (ví dụ: 100,000 VND)
            string purpose = "THANH TOAN DON HANG";
            GetBankInfor(enProject, ref banknumber, ref bankbin);
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            
            QRCodeData qrCodeInfo = qrGenerator.CreateQrCode(GenerateVietQRpayload(bankbin,banknumber,amount,purpose), QRCodeGenerator.ECCLevel.Q);
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
        public string GetValueMoneyTranfer()
        {
            return "0";
        }
        private static string GenerateVietQRpayload(string bankBin, string bankNumber, string amount, string purpose)
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
