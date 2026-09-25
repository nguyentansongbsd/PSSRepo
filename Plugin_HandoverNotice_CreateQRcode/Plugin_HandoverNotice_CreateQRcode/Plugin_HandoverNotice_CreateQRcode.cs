using Microsoft.Xrm.Sdk;
using QRCoder;
using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk.Query;
using VietQRHelper;

namespace Plugin_HandoverNotice_CreateQRcode
{
    public class Plugin_HandoverNotice_CreateQRcode : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        bool isGenQR = true;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity entity = (Entity)context.InputParameters["Target"];
            Entity enTarget = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
            tracingService.Trace("Start ID: " + entity.Id);

            EntityReference enProjectRef = enTarget.GetAttributeValue<EntityReference>("bsd_project");
            Entity enProject = service.Retrieve(enProjectRef.LogicalName, enProjectRef.Id, new ColumnSet(true));

            // Kiểm tra cấu hình có cho phép gen QR hay không
            CheckGenQrFlag(enProject);
            if (!isGenQR)
            {
                tracingService.Trace("Project configured isGenQR = false. Skipping...");
                return;
            }

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            Entity entityToUpdate = new Entity(enTarget.LogicalName, enTarget.Id);

            // ==========================================
            // CASE 1: TẠO QR ĐỢT THANH TOÁN (bsd_default = true)
            // ==========================================
            string defaultBankNumber = "";
            string defaultBankBin = "";
            bool hasDefaultBank = GetBankAccountByCondition(enProject.Id, "bsd_default", ref defaultBankNumber, ref defaultBankBin);

            if (hasDefaultBank)
            {
                string purposeInstalment = GetContent(service, enTarget, isFeeContent: false);
                QRCodeData qrCodeInfo = qrGenerator.CreateQrCode(GenerateVietQRpayload(defaultBankBin, defaultBankNumber, purposeInstalment), QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeInfo);

                entityToUpdate["bsd_qrinstalment"] = qrCode.GetGraphic(20);
                tracingService.Trace("Case 1: Đã tạo mã QR Đợt thanh toán (bsd_qrinstalment).");
            }
            else
            {
                throw new Exception("Không tìm thấy Tài khoản Ngân hàng Mặc định (bsd_default = yes) của Dự án!");
            }

            // ==========================================
            // CASE 2: TẠO QR PHÍ (bsd_bankfee = true)
            // ==========================================
            string feeBankNumber = "";
            string feeBankBin = "";
            bool hasFeeBank = GetBankAccountByCondition(enProject.Id, "bsd_bankfee", ref feeBankNumber, ref feeBankBin);

            if (hasFeeBank)
            {
                string purposeFee = GetContent(service, enTarget, isFeeContent: true);
                QRCodeData qrCodeInfoFee = qrGenerator.CreateQrCode(GenerateVietQRpayload(feeBankBin, feeBankNumber, purposeFee), QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCodeFee = new PngByteQRCode(qrCodeInfoFee);

                entityToUpdate["bsd_qrfee"] = qrCodeFee.GetGraphic(20);
                tracingService.Trace("Case 2: Đã tìm thấy ngân hàng Fee -> Tạo mã QR Phí (bsd_qrfee).");
            }
            else
            {
                tracingService.Trace("Case 2: Không có ngân hàng tích bsd_bankfee = yes -> Bỏ qua tạo QR Phí.");
            }

            // Cập nhật thông tin bản ghi
            service.Update(entityToUpdate);
        }

        /// <summary>
        /// Hàm chung truy vấn Bank Account theo điều kiện (bsd_default hoặc bsd_bankfee)
        /// </summary>
        public bool GetBankAccountByCondition(Guid projectId, string conditionFieldName, ref string bankNumber, ref string bankBin)
        {
            string bankAccountEntityName = "bsd_projectbankaccount";
            string projectLookupField = "bsd_project";
            string bankAccountNumberField = "bsd_name";

            string bankLookupField = "bsd_bank";
            string bankEntityName = "bsd_bank";
            string bankBinField = "new_bankcode";
            string bankAlias = "bank";

            QueryExpression query = new QueryExpression(bankAccountEntityName);
            query.ColumnSet = new ColumnSet(bankAccountNumberField);
            query.Criteria.AddCondition(projectLookupField, ConditionOperator.Equal, projectId);
            query.Criteria.AddCondition(conditionFieldName, ConditionOperator.Equal, true);

            LinkEntity linkEntity = new LinkEntity(bankAccountEntityName, bankEntityName, bankLookupField, "bsd_bankid", JoinOperator.Inner);
            linkEntity.Columns.AddColumn(bankBinField);
            linkEntity.EntityAlias = bankAlias;
            query.LinkEntities.Add(linkEntity);

            EntityCollection results = service.RetrieveMultiple(query);

            if (results.Entities.Count > 0)
            {
                Entity bankAccount = results.Entities.First();

                if (bankAccount.Contains(bankAccountNumberField))
                {
                    bankNumber = bankAccount.GetAttributeValue<string>(bankAccountNumberField);
                }

                if (bankAccount.Contains($"{bankAlias}.{bankBinField}"))
                {
                    bankBin = (string)((AliasedValue)bankAccount[$"{bankAlias}.{bankBinField}"]).Value;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra cờ Print QR trên Project
        /// </summary>
        private void CheckGenQrFlag(Entity project)
        {
            if (project.Contains("bsd_print_qr_bank"))
            {
                isGenQR = project.GetAttributeValue<bool>("bsd_print_qr_bank");
            }
        }

        /// <summary>
        /// Lấy nội dung chuyển khoản. Dùng chung cho cả Đợt thanh toán và Phí
        /// </summary>
        public string GetContent(IOrganizationService service, Entity target, bool isFeeContent)
        {
            string customerName = "KHACH HANG";
            string unitName = "SAN PHAM";
            string projectName = "ProjectName";
            string orderNumber = "0";

            if (target.Contains("bsd_project"))
            {
                EntityReference projectRef = target.GetAttributeValue<EntityReference>("bsd_project");
                if (projectRef != null)
                {
                    Entity project = service.Retrieve(projectRef.LogicalName, projectRef.Id, new ColumnSet("bsd_name"));
                    projectName = project.GetAttributeValue<string>("bsd_name");
                }
            }

            if (target.Contains("bsd_customer"))
            {
                EntityReference customerRef = target.GetAttributeValue<EntityReference>("bsd_customer");
                if (customerRef != null)
                {
                    Entity customer = service.Retrieve(customerRef.LogicalName, customerRef.Id, new ColumnSet(true));
                    customerName = customer.LogicalName == "contact" ? customer.GetAttributeValue<string>("fullname") : customer.GetAttributeValue<string>("name");
                }
            }

            if (target.Contains("bsd_units"))
            {
                EntityReference unitRef = target.GetAttributeValue<EntityReference>("bsd_units");
                if (unitRef != null)
                {
                    Entity unit = service.Retrieve(unitRef.LogicalName, unitRef.Id, new ColumnSet("name"));
                    unitName = unit.GetAttributeValue<string>("name");
                }
            }

            string finalCustomerName = RemoveDiacritics(customerName ?? "").ToUpper();
            string finalUnitName = (unitName ?? "").ToUpper();

            // Nếu là nội dung Phí
            if (isFeeContent)
            {
                return $"{finalUnitName} {projectName} {finalCustomerName} Thanh toan phi";
            }

            // Nếu là nội dung Đợt thanh toán
            if (target.Contains("bsd_installment"))
            {
                EntityReference insRef = target.GetAttributeValue<EntityReference>("bsd_installment");
                if (insRef != null)
                {
                    Entity ins = service.Retrieve(insRef.LogicalName, insRef.Id, new ColumnSet("bsd_ordernumber"));
                    orderNumber = ins.GetAttributeValue<int>("bsd_ordernumber").ToString();
                }
            }

            return $"{finalUnitName} {projectName} {finalCustomerName} Thanh toan dot {orderNumber}";
        }

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

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace("Đ", "D").Replace("đ", "d");
        }

        private static string GenerateVietQRpayload(string bankBin, string bankNumber, string purpose)
        {
            var qrPay = QRPay.InitVietQR(
                bankBin: bankBin,
                bankNumber: bankNumber,
                purpose: purpose
            );
            return qrPay.Build();
        }
    }
}