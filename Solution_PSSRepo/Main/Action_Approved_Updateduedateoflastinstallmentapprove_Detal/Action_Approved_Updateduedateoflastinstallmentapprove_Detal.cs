// *********************************************************************************************************************
// Tên plugin: Action_Approved_Updateduedateoflastinstallmentapprove_Detal
// Mục đích: Xử lý logic khi một yêu cầu thay đổi ngày đến hạn của đợt thanh toán cuối cùng được phê duyệt.
// Tác giả: DevTu
// Ngày tạo: 2024-05-29
//
// Mô tả tổng quan:
// Plugin này được kích hoạt bởi một Custom Action để xử lý việc phê duyệt một bản ghi "Update Due Date of Last Installment"
// (bsd_updateduedateoflastinstallment). Chức năng chính là thực hiện một chuỗi các kiểm tra nghiệp vụ nghiêm ngặt
// trước khi chính thức cho phép thay đổi ngày đến hạn (due date) của đợt thanh toán cuối cùng (last installment).
//
// Các bước xử lý chính:
// 1. Khởi tạo: Lấy các context cần thiết của plugin (ExecutionContext, OrganizationService, TracingService).
// 2. Lấy dữ liệu: Đọc thông tin của bản ghi "bsd_updateduedateoflastinstallment" đang được xử lý từ tham số đầu vào của Action.
// 3. Kiểm tra điều kiện chạy: Xác định xem plugin có nên tiếp tục thực thi hay không (hiện tại luôn cho phép chạy).
// 4. Chuỗi kiểm tra nghiệp vụ:
//    - CheckExistParentInDetail: Đảm bảo Project trên bản ghi chi tiết khớp với Project trên bản ghi Master.
//    - CheckIsLast: Xác nhận rằng đợt thanh toán được chỉ định thực sự là đợt cuối cùng.
//    - CheckHD: Kiểm tra trạng thái của hợp đồng liên quan (Option Entry) không được ở trạng thái "Terminated".
//    - CheckPaidDetail: (Hiện đang bị vô hiệu hóa) Kiểm tra xem đợt thanh toán đã được trả tiền chưa.
//    - CheckDueDate: Đảm bảo ngày đến hạn mới phải lớn hơn ngày đến hạn của tất cả các đợt thanh toán trước đó.
//    - CheckNewDate: Đảm bảo ngày đến hạn mới phải khác với ngày đến hạn cũ.
// 5. Phê duyệt (Approve): Nếu tất cả các kiểm tra thành công, gọi một Custom Action khác ("bsd_Action_Approved_UpdateDuedateOfLastInstallment_Master")
//    để cập nhật trạng thái "Approved" và ngày đến hạn mới cho các bản ghi liên quan.
// 6. Xử lý lỗi (HandleError): Nếu có bất kỳ lỗi nào xảy ra trong quá trình kiểm tra, cập nhật trạng thái của bản ghi
//    cha và chi tiết thành "Error" và ghi lại thông báo lỗi.
// *********************************************************************************************************************
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Approved_Updateduedateoflastinstallmentapprove_Detal
{
    public class Action_Approved_Updateduedateoflastinstallmentapprove_Detal : IPlugin
    {
        // Khai báo các biến toàn cục để sử dụng trong plugin

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            // 1. KHỞI TẠO
            // Lấy execution context từ service provider
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            // Lấy factory để tạo organization service
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            // Tạo một instance của organization service
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            // Lấy tracing service để ghi log debug
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("start ");
            // 2. LẤY DỮ LIỆU
            // Lấy ID của bản ghi "bsd_updateduedateoflastinstallment" từ InputParameters của action
            string enDetailid = context.InputParameters["id"].ToString();
            tracingService.Trace("enDetailid :" + enDetailid);

            // Truy vấn bản ghi chi tiết với đầy đủ các cột
            en = service.Retrieve("bsd_updateduedateoflastinstallment", new Guid(enDetailid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var item = en;
            // 3. KIỂM TRA ĐIỀU KIỆN CHẠY
            // Nếu điều kiện không thỏa mãn, dừng thực thi
            if (!CheckConditionRun(en))
            {
                tracingService.Trace("stop");
                return;
            }
            // Lấy giá trị của status code
            var status = ((OptionSetValue)en["statuscode"]).Value;
            tracingService.Trace("start :" + status);

            // Biến cờ để kiểm tra kết quả của các bước xác thực
            var result = true;
            tracingService.Trace($"{item.Id}");
            try
            {

                // Lấy thông tin tham chiếu của đợt thanh toán cuối cùng
                EntityReference enInstallmentRef = (EntityReference)item["bsd_lastinstallment"];
                EntityReference enInstallmentRef2 = new EntityReference(enInstallmentRef.LogicalName, enInstallmentRef.Id);
                var query_bsd_paymentschemedetailid = enInstallmentRef2.Id.ToString();
                // Lấy thông tin chi tiết của đợt thanh toán
                Entity enInstallment = GetInstallment(enInstallmentRef2.Id.ToString());
                tracingService.Trace($"item {item["bsd_duedate"]}");
                //tracingService.Trace($"{enInstallment["bsd_duedate"]}");
                tracingService.Trace($"{enInstallment.Id}");
                // Lấy thông tin hợp đồng (Option Entry) từ đợt thanh toán
                var enHDRef = (EntityReference)enInstallment["bsd_optionentry"];
                var enHD = service.Retrieve(enHDRef.LogicalName, enHDRef.Id, new ColumnSet(true));
                // 4. CHUỖI KIỂM TRA NGHIỆP VỤ
                tracingService.Trace("CheckExistParentInDetail");
                CheckExistParentInDetail(ref result, item); // Kiểm tra project có khớp không
                if (!result) return; // Nếu lỗi, dừng lại
                tracingService.Trace("CheckIsLast");
                CheckIsLast(ref result, item, enInstallment); // Kiểm tra có phải đợt cuối không
                if (!result) return;
                tracingService.Trace("CheckHD");
                CheckHD(ref result, item, enHD); // Kiểm tra trạng thái hợp đồng
                if (!result) return;
                tracingService.Trace("CheckPaidDetail");
                CheckPaidDetail(ref result, item, enInstallment); // Kiểm tra đã thanh toán chưa
                if (!result) return;
                tracingService.Trace("CheckDueDate");
                CheckDueDate(ref result, item, enInstallment, enHD); // Kiểm tra ngày đến hạn mới so với các đợt khác
                if (!result) return;
                tracingService.Trace("CheckNewDate");
                CheckNewDate(ref result, item, enInstallment); // Kiểm tra ngày mới có khác ngày cũ không
                if (!result) return;
                // 5. PHÊ DUYỆT
                tracingService.Trace("Approve");
                Approve(ref result, item, enInstallment); // Nếu tất cả kiểm tra OK, tiến hành phê duyệt
            }
            catch (Exception ex)
            {
                // 6. XỬ LÝ LỖI
                // Nếu có lỗi ngoại lệ xảy ra, gọi hàm HandleError
                HandleError(item, ex.Message);
            }
            //Entity enDetailUpdate = new Entity(en.LogicalName, en.Id);
            // Status Reason(entity cha) = Approved
            //Approved / Rejected Date[bsd_approvedrejecteddate] = Ngày cập nhật thành công
            //Approved / Rejected Person[bsd_approvedrejectedperson] = Người nhấn nút duyệt
            //Status Reason(entity Detail) = Approved
            //Due Date(New) (ở entity Detail) về field Due Date của installment ở entity Detail
            //enDetailUpdate["statuscode"] = new OptionSetValue(100000000); //Status Reason(entity Detail) = Approved
            //service.Update(enDetailUpdate);

        }
        /// <summary>
        /// Lấy thông tin chi tiết của một đợt thanh toán bằng FetchXML.
        /// </summary>
        /// <param name="id">ID của bản ghi đợt thanh toán.</param>
        public Entity GetInstallment(string id)
        {
            var fetchData = new
            {
                bsd_paymentschemedetailid = id
            };
            var fetchXml = $@"
<fetch top=""50"">
  <entity name=""bsd_paymentschemedetail"">
    <filter>
      <condition attribute=""bsd_paymentschemedetailid"" operator=""eq"" value=""{fetchData.bsd_paymentschemedetailid/*86a5f5bb-343b-ee11-bdf4-000d3aa14fb9*/}"" />
    </filter>
  </entity>
</fetch>";
            var res = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Entity enInstallment = res.Entities[0];
            return enInstallment;
        }
        /// <summary>
        /// Kiểm tra xem Project trên bản ghi Detail có trùng với Project trên bản ghi Master không.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date of Last Installment'.</param>
        public void CheckExistParentInDetail(ref bool result, Entity item)
        {
            // Lấy thông tin bản ghi Master từ bản ghi Detail
            var enMasterRef = (EntityReference)item["bsd_updateduedateoflastinstapprove"];

            var enMaster = service.Retrieve("bsd_updateduedateoflastinstallmentapprove", enMasterRef.Id, new ColumnSet(true));
            // So sánh ID của Project
            if (((EntityReference)enMaster["bsd_project"]).Id != ((EntityReference)item["bsd_project"]).Id)
            {
                var mess = "The project in the Detail entity is invalid. Please check again.";
                HandleError(item, mess); // Ghi lỗi

                result = false; // Đánh dấu là có lỗi
            }



        }
        /// <summary>
        /// Kiểm tra xem đợt thanh toán có phải là đợt cuối cùng không (Last Installment = yes).
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date of Last Installment'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan.</param>
        public void CheckIsLast(ref bool result, Entity item, Entity enInstallment)
        {

            var enInstallmentRef = (EntityReference)item["bsd_lastinstallment"];
            // Kiểm tra trường 'bsd_lastinstallment' trên bản ghi Installment
            if (!((bool)enInstallment["bsd_lastinstallment"]))
            {
                var mess = "The record contains an invalid batch. Please check again.";
                HandleError(item, mess);

                result = false;
            }

        }

        /// <summary>
        /// Kiểm tra trạng thái của Hợp đồng (Option Entry).
        /// Nếu hợp đồng đã bị thanh lý (Terminated), không cho phép thay đổi.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date of Last Installment'.</param>
        /// <param name="enHD">Bản ghi hợp đồng (SalesOrder) liên quan.</param>
        public void CheckHD(ref bool result, Entity item, Entity enHD)
        {


            // Kiểm tra nếu statuscode của hợp đồng là 'Terminated' (100000006)
            if (((OptionSetValue)enHD["statuscode"]).Value == 100000006)
            {
                var mess = "The record contains a contract that has already been liquidated. Please check again.";
                HandleError(item, mess);

                result = false;
            }



        }
        /// <summary>
        /// kiểm tra đợt trong entity detail được thanh toán không? 
        /// (Kiểm tra 2 field: Amount Was Paid [bsd_amountwaspaid] ; 
        /// Deposit Amount Paid [bsd_depositamount] khác 0)
        /// </summary>
        public void CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)
        {
            //tracingService.Trace($"bsd_depositamount {((Money)enInstallment["bsd_depositamount"]).Value}");
            //tracingService.Trace($"bsd_amountwaspaid {((Money)enInstallment["bsd_amountwaspaid"]).Value}");
            //if ((((Money)enInstallment["bsd_depositamount"]).Value != 0 || ((Money)enInstallment["bsd_amountwaspaid"]).Value != 0))
            //{
            //    var mess = "There is a batch that has already been paid. Please check again.";
            //    HandleError(item, mess);

            //    result = false;
            //}
        }
        /// <summary>
        /// Kiểm tra tính hợp lệ của ngày đến hạn mới.
        /// Ngày mới phải lớn hơn ngày đến hạn của tất cả các đợt thanh toán trước đó.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date of Last Installment'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan.</param>
        /// <param name="enHD">Bản ghi hợp đồng liên quan.</param>
        public void CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)
        {
            if (!item.Contains("bsd_duedate")) return;
            var newDate = (DateTime)item["bsd_duedate"];
            tracingService.Trace("step 1");
            // Truy vấn tất cả các đợt thanh toán của cùng một hợp đồng
            var query = new QueryExpression(enInstallment.LogicalName);
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, enHD.Id.ToString());
            var rs_ = service.RetrieveMultiple(query);
            // Duyệt qua các đợt thanh toán khác
            foreach (var JItem in rs_.Entities)
            {
                // Bỏ qua chính nó
                if (JItem.Id != enInstallment.Id)
                {

                    if (!JItem.Contains("bsd_duedate")) continue;
                    tracingService.Trace($"{(DateTime)JItem["bsd_duedate"]}");

                    // Nếu ngày mới nhỏ hơn hoặc bằng ngày của một đợt khác -> lỗi
                    if (newDate <= ((DateTime)JItem["bsd_duedate"]))
                    {
                        var mess = "The new due date is invalid. Please check again.";
                        HandleError(item, mess);

                        result = false;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Kiểm tra ngày đến hạn mới có trùng với ngày đến hạn cũ không.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date of Last Installment'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan.</param>
        public void CheckNewDate(ref bool result, Entity item, Entity enInstallment)
        {
            // Bỏ qua nếu đợt thanh toán không có ngày đến hạn cũ
            if (!enInstallment.Contains("bsd_duedate"))
                return;
            var newDate = (DateTime)item["bsd_duedate"];
            tracingService.Trace($"{enInstallment.Id}");
            // So sánh ngày mới và ngày cũ
            if ((newDate - (DateTime)enInstallment["bsd_duedate"]).TotalDays == 0)
            {
                var mess = "The new due date is the same as the old due date. Please check again.";
                HandleError(item, mess);

                result = false;
            }


        }
        /// <summary>
        /// Thực hiện hành động phê duyệt.
        /// Gọi một custom action "bsd_Action_Approved_UpdateDuedateOfLastInstallment_Master" để cập nhật các bản ghi liên quan.
        /// </summary>
        /// <param name="result">Biến cờ kết quả (không được sử dụng trong hàm này).</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date of Last Installment'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan (không được sử dụng trực tiếp).</param>
        public void Approve(ref bool result, Entity item, Entity enInstallment)
        {
            // Status Reason(entity cha) = Approved
            //Approved / Rejected Date[bsd_approvedrejecteddate] = Ngày cập nhật thành công
            //Approved / Rejected Person[bsd_approvedrejectedperson] = Người nhấn nút duyệt
            //Status Reason(entity Detail) = Approved
            //Due Date(New) (ở entity Detail) về field Due Date của installment ở entity Detail
            // Chuẩn bị gọi custom action
            var request = new OrganizationRequest("bsd_Action_Approved_UpdateDuedateOfLastInstallment_Master");

            var newDate = (DateTime)item["bsd_duedate"];
            // Truyền các tham số cần thiết cho action
            request["detail_id"] = item.Id.ToString();
            // Chuyển đổi ngày mới sang định dạng chuỗi, cộng 7 giờ để xử lý múi giờ (UTC+7)
            request["duedatenew"] = newDate.AddHours(7).ToString();
            request["statuscode"] = 100000000; // Status code cho "Approved"
            // Thực thi action
            service.Execute(request);

        }
        /// <summary>
        /// Xử lý khi có lỗi xảy ra.
        /// Cập nhật bản ghi Master và Detail với thông tin lỗi.
        /// </summary>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date of Last Installment' gây ra lỗi.</param>
        /// <param name="error">Thông báo lỗi.</param>
        public void HandleError(Entity item, string error)
        {
            // Cập nhật bản ghi Master
            var enMasterRef = (EntityReference)item["bsd_updateduedateoflastinstapprove"];
            var enMaster = new Entity("bsd_updateduedateoflastinstallmentapprove", enMasterRef.Id);
            enMaster["bsd_error"] = true; // Đánh dấu có lỗi
            enMaster["bsd_errordetail"] = error; // Ghi lại thông báo lỗi
            enMaster["bsd_approvedrejectedperson"] = null; // Xóa người duyệt
            enMaster["bsd_approvedrejecteddate"] = null; // Xóa ngày duyệt
            service.Update(enMaster);
            // Cập nhật bản ghi Detail
            var enupdate = new Entity(en.LogicalName, en.Id);
            enupdate["statuscode"] = new OptionSetValue(100000004); // Chuyển status thành "Error"
            enupdate["bsd_errordetail"] = error;
            service.Update(enupdate);
            tracingService.Trace("error nè");
        }
        /// <summary>
        /// Kiểm tra các điều kiện ban đầu để quyết định có thực thi plugin hay không.
        /// Ghi chú: Logic này hiện đang được vô hiệu hóa và mặc định trả về true.
        /// </summary>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date of Last Installment'.</param>
        /// <returns>True nếu plugin nên tiếp tục thực thi, false nếu không.</returns>
        public bool CheckConditionRun(Entity item)
        {
            return true;
            //var enMasterRef = (EntityReference)item["bsd_updateduedateoflastinstapprove"];
            //var enMaster = service.Retrieve("bsd_updateduedateoflastinstallmentapprove", enMasterRef.Id, new ColumnSet(true));
            //tracingService.Trace("CheckConditionRun");
            //if ((bool)enMaster["bsd_error"] == true && (bool)enMaster["bsd_processing_pa"] == false)
            //{
            //    tracingService.Trace("error: " + (bool)enMaster["bsd_error"]);
            //    return false;
            //}
            //else
            //{
                
            //    return true;
            //}
        }
    }
}
