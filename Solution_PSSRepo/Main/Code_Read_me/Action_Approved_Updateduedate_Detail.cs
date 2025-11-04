// *********************************************************************************************************************
// Tên plugin: Action_Approved_Updateduedate_Detail
// Mục đích: Xử lý logic khi một chi tiết "Update Due Date Detail" được phê duyệt.
// Tác giả: Gemini Code Assist
// Ngày tạo: 2024-05-29
//
// Mô tả tổng quan:
// Plugin này được kích hoạt khi một action được gọi để phê duyệt một bản ghi chi tiết yêu cầu cập nhật ngày đến hạn (bsd_updateduedatedetail).
// Chức năng chính của plugin là thực hiện một loạt các kiểm tra nghiệp vụ nghiêm ngặt để đảm bảo tính hợp lệ của yêu cầu thay đổi ngày đến hạn
// của một đợt thanh toán (installment) trước khi cho phép phê duyệt.
//
// Các bước xử lý chính:
// 1. Khởi tạo: Lấy các context cần thiết của plugin (ExecutionContext, OrganizationService, TracingService).
// 2. Lấy dữ liệu: Đọc thông tin của bản ghi "bsd_updateduedatedetail" đang được xử lý.
// 3. Kiểm tra điều kiện chạy: Xác định xem plugin có nên tiếp tục thực thi hay không.
// 4. Chuỗi kiểm tra nghiệp vụ:
//    - CheckExistParentInDetail: Đảm bảo Project trên bản ghi chi tiết khớp với Project trên bản ghi Master.
//    - CheckIsLast: Ngăn chặn thay đổi nếu đây là đợt thanh toán cuối cùng hoặc liên quan đến bàn giao.
//    - CheckHD: Kiểm tra trạng thái của hợp đồng liên quan (Option Entry, Quote, Quotation) không được ở trạng thái "Terminated".
//    - CheckPaidDetail: (Hiện đang bị vô hiệu hóa) Kiểm tra đợt thanh toán đã được trả tiền chưa.
//    - CheckDueDate: Đảm bảo ngày đến hạn mới hợp lệ so với các đợt thanh toán trước và sau nó.
//    - CheckNewDate: (Hiện đang bị vô hiệu hóa) Kiểm tra ngày đến hạn mới phải khác ngày đến hạn cũ.
// 5. Phê duyệt (Approve): Nếu tất cả các kiểm tra đều thành công, gọi một Custom Action ("bsd_Action_Approved_Updateduedate_Master")
//    để cập nhật trạng thái "Approved" và ngày đến hạn mới cho các bản ghi liên quan.
// 6. Xử lý lỗi (HandleError): Nếu có bất kỳ lỗi nào xảy ra trong quá trình kiểm tra, cập nhật trạng thái của bản ghi
//    cha và chi tiết thành "Error" và ghi lại thông báo lỗi.
// *********************************************************************************************************************
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Action_Approved_Updateduedate_Detail
{
    public class Action_Approved_Updateduedate_Detail : IPlugin
    {
        // Khai báo các biến toàn cục để sử dụng trong plugin

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        string enHD_name = "";
        string enIntalments_fieldNameHD = "";
        Entity enMaster = new Entity();
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
            //DownloadFile();
            //return;
            // 2. LẤY DỮ LIỆU
            // Lấy ID của bản ghi "bsd_updateduedatedetail" từ InputParameters của action
            string enDetailid = context.InputParameters["id"].ToString();
            // Truy vấn bản ghi chi tiết với đầy đủ các cột
            en = service.Retrieve("bsd_updateduedatedetail", new Guid(enDetailid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
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
            tracingService.Trace("enDetailid :" + enDetailid);
            // Biến cờ để kiểm tra kết quả của các bước xác thực
            var result = true;
            try
            {
                // 4. CHUỖI KIỂM TRA NGHIỆP VỤ
                tracingService.Trace("start foreach");
                // Lấy thông tin đợt thanh toán (Installment) từ bản ghi chi tiết
                var enInstallmentRef = (EntityReference)item["bsd_installment"];
                var enInstallment = service.Retrieve(enInstallmentRef.LogicalName, enInstallmentRef.Id, new ColumnSet(true));
                #region Xác định loại giao dịch (hợp đồng) và các trường liên quan
                if (item.Contains("bsd_optionentry"))
                {
                    enIntalments_fieldNameHD = "bsd_optionentry";
                    enHD_name = "salesorder";
                }
                else
                {
                    if (item.Contains("bsd_quote"))
                    {
                        enIntalments_fieldNameHD = "bsd_reservation";
                        enHD_name = "quote";

                    }
                    else
                    {
                        if (item.Contains("bsd_quotation"))
                        {

                            enIntalments_fieldNameHD = "bsd_quotation";
                            enHD_name = "bsd_quotation";

                        }
                        else
                        {
                            // Nếu không tìm thấy loại hợp đồng nào, ghi lỗi và dừng
                            HandleError(item, "not found entity!");
                        }    

                    }

                }
                #endregion
                // Lấy thông tin bản ghi hợp đồng liên quan
                var enHDRef = (EntityReference)enInstallment[enIntalments_fieldNameHD];
                var enHD = service.Retrieve(enHDRef.LogicalName, enHDRef.Id, new ColumnSet(true));
                tracingService.Trace("CheckExistParentInDetail");
                CheckExistParentInDetail(ref result, item); // Kiểm tra project có khớp không
                if (!result) return; // Nếu lỗi, dừng lại

                tracingService.Trace("CheckIsLast");
                CheckIsLast(ref result, item, enInstallment); // Kiểm tra có phải đợt cuối/bàn giao không
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



           
        }
        /// <summary>
        /// Kiểm tra xem Project trên bản ghi Detail có trùng với Project trên bản ghi Master không.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail'.</param>
        public void CheckExistParentInDetail(ref bool result, Entity item)
        {
            // Lấy thông tin bản ghi Master từ bản ghi Detail
            var enMasterRef = (EntityReference)item["bsd_updateduedate"];

            enMaster = service.Retrieve("bsd_updateduedate", enMasterRef.Id, new ColumnSet(true));
            // So sánh ID của Project
            if (((EntityReference)enMaster["bsd_project"]).Id != ((EntityReference)item["bsd_project"]).Id)
            {
                var mess = "The project in the Master and Detail entities does not match. Please check again.";
                HandleError(item, mess);

                result = false;
            }
        }
        /// <summary>
        /// Kiểm tra xem đợt thanh toán có phải là đợt cuối cùng (Last Installment = yes)
        /// hoặc có phương thức tính ngày đến hạn là "Estimate handover date" hay không.
        /// Nếu đúng, không cho phép thay đổi.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan.</param>
        public void CheckIsLast(ref bool result, Entity item, Entity enInstallment)
        {
            // Kiểm tra trường 'bsd_lastinstallment' hoặc 'bsd_duedatecalculatingmethod'
            if (((bool)enInstallment["bsd_lastinstallment"]) || (enInstallment.Contains("bsd_duedatecalculatingmethod") && ((OptionSetValue)enInstallment["bsd_duedatecalculatingmethod"]).Value == 100000002))
            {
                var mess = "The record contains a handover batch. Please check again.";
                HandleError(item, mess);
                result = false;
            }
        }
        /// <summary>
        /// Kiểm tra trạng thái của Hợp đồng (Quotation, Option Entry, Reservation).
        /// Nếu hợp đồng đã bị thanh lý (Terminated), không cho phép thay đổi.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail'.</param>
        /// <param name="enHD">Bản ghi hợp đồng (Quotation, SalesOrder, Quote) liên quan.</param>
        public void CheckHD(ref bool result, Entity item, Entity enHD)
        {
            // Sử dụng switch để kiểm tra statuscode dựa trên loại hợp đồng
            switch (enHD_name)
            {
                case "bsd_quotation":
                    if (((OptionSetValue)enHD["statuscode"]).Value == 100000001)
                    {
                        var mess = "The record contains a contract that has already been liquidated. Please check again.";
                        HandleError(item, mess);
                        result = false;
                    }
                    break;
                case "salesorder":
                    if (((OptionSetValue)enHD["statuscode"]).Value == 100000006)
                    {
                        var mess = "The record contains a contract that has already been liquidated. Please check again.";
                        HandleError(item, mess);

                        result = false;
                    }
                    break;
                case "quote":
                    if (((OptionSetValue)enHD["statuscode"]).Value == 100000001)
                    {
                        var mess = "The record contains a contract that has already been liquidated. Please check again.";
                        HandleError(item, mess);

                        result = false;
                    }
                    break;
            }
        }
        /// <summary>
        /// Kiểm tra xem đợt thanh toán trong entity detail đã được thanh toán hay chưa.
        /// (Kiểm tra 2 field: Amount Was Paid [bsd_amountwaspaid] ; Deposit Amount Paid [bsd_depositamount] khác 0)
        /// Ghi chú: Điều kiện này đã bị vô hiệu hóa (comment out) theo yêu cầu ngày 17/02/2025.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan.</param>
        public void CheckPaidDetail(ref bool result, Entity item, Entity enInstallment)
        {
            //bỏ điều kiện check này DevTu - 17.2.2025
            //if (((Money)enInstallment["bsd_depositamount"]).Value != 0 || ((Money)enInstallment["bsd_amountwaspaid"]).Value != 0)
            //{
            //    var mess = "There is a batch that has already been paid. Please check again.";
            //    HandleError(item, mess);

            //    result = false;
            //}
        }
        /// <summary>
        /// Kiểm tra tính hợp lệ của ngày đến hạn mới.
        /// 1. Ngày mới phải lớn hơn ngày đến hạn của đợt trước.
        /// 2. Ngày mới phải nhỏ hơn ngày đến hạn của đợt sau.
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan.</param>
        /// <param name="enHD">Bản ghi hợp đồng liên quan.</param>
        public void CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)
        {

            var newDate = (DateTime)item["bsd_duedatenew"];

            // Truy vấn tất cả các đợt thanh toán khác của cùng một hợp đồng
            var query = new QueryExpression(enInstallment.LogicalName);
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition(enIntalments_fieldNameHD, ConditionOperator.Equal, enHD.Id.ToString());
            query.Criteria.AddCondition("bsd_paymentschemedetailid", ConditionOperator.NotEqual, enInstallment.Id.ToString());
            var rs_ = service.RetrieveMultiple(query);
            tracingService.Trace($"enInstallment:{enInstallment.Id.ToString()}");
            // Lấy danh sách các bản ghi detail khác đang được xử lý trong cùng 1 master
            var lstDetail=GetListDetail();
            // Duyệt qua các đợt thanh toán khác
            foreach (var JItem in rs_.Entities)
            {
                tracingService.Trace($"JItem:{JItem.Id.ToString()}");

                // Bỏ qua chính nó
                if (JItem.Id != enInstallment.Id)
                {

                    if (JItem.Contains("bsd_duedate") == false) continue;
                    tracingService.Trace($"bsd_ordernumber:{(int)JItem["bsd_ordernumber"]}");
                    // So sánh với các đợt có số thứ tự nhỏ hơn (đợt trước)
                    if (((int)JItem["bsd_ordernumber"]) < ((int)enInstallment["bsd_ordernumber"]))//lớn hơn đợt phí trước?
                    {
                        #region Kiểm tra với ngày của đợt trước
                        // Ưu tiên lấy ngày mới từ một bản ghi detail khác nếu có
                        var itemDetail = lstDetail.Entities.FirstOrDefault(x =>((EntityReference)x["bsd_installment"]).Id == JItem.Id);
                        tracingService.Trace("@@");
                        if(itemDetail != null)
                        {
                            if ((newDate - (((DateTime)itemDetail["bsd_duedatenew"]))).TotalDays <=-1)
                            {
                                var mess = "The new due date is earlier than the next batch. Please check again.";
                                HandleError(item, mess);
                                result = false;
                                break;
                            }
                        }
                        else if( (newDate - (((DateTime)JItem["bsd_duedate"]))).TotalDays<=-1)
                        {
                            tracingService.Trace("islist1");
                            var mess = "The new due date is earlier than the next batch. Please check again.";
                            HandleError(item, mess);
                            result = false;
                            break;
                        }
                        #endregion

                    }
                    // So sánh với các đợt có số thứ tự lớn hơn (đợt sau)
                    if (((int)JItem["bsd_ordernumber"]) > ((int)enInstallment["bsd_ordernumber"]))//nhở hơn đợt phí sau?
                    {
                        #region Kiểm tra với ngày của đợt sau
                        // Ưu tiên lấy ngày mới từ một bản ghi detail khác nếu có
                        var itemDetail = lstDetail.Entities.FirstOrDefault(x => ((EntityReference)x["bsd_installment"]).Id == JItem.Id);
                        tracingService.Trace("###");

                        if (itemDetail != null)
                        {
                            if ((newDate - (((DateTime)itemDetail["bsd_duedatenew"]))).TotalDays >= 1)
                            {
                                var mess = "The new due date is later than the previous batch. Please check again..";
                                HandleError(item, mess);
                                result = false;
                                break;
                            }
                        }
                        else if( (newDate - (((DateTime)JItem["bsd_duedate"]))).TotalDays >= 1)
                        {
                            tracingService.Trace("islist2");

                            var mess = "The new due date is later than the previous batch. Please check again.";
                            HandleError(item, mess);
                            result = false;
                            break;
                        }

                        #endregion
                    }
                }

            }
            
        }
        /// <summary>
        /// Lấy danh sách các bản ghi "Update Due Date Detail" khác thuộc cùng một bản ghi Master
        /// và chưa bị xử lý lỗi.
        /// </summary>
        /// <returns>EntityCollection chứa các bản ghi 'Update Due Date Detail' thỏa mãn điều kiện.</returns>
        public EntityCollection GetListDetail()
        {
            var query = new QueryExpression("bsd_updateduedatedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 100000003); // Không phải là "Error"
            query.Criteria.AddCondition("bsd_updateduedate", ConditionOperator.Equal, enMaster.Id.ToString()); // Lọc theo bản ghi Master
            var rs_ = service.RetrieveMultiple(query);
            return rs_;
        }
        /// <summary>
        /// Kiểm tra ngày đến hạn mới (Due Date (New)) có trùng với ngày đến hạn cũ (Due Date (Old)) không.
        /// Ghi chú: Logic này hiện đang được vô hiệu hóa (comment out).
        /// </summary>
        /// <param name="result">Biến cờ kết quả, true nếu thành công, false nếu có lỗi.</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan.</param>
        public void CheckNewDate(ref bool result, Entity item, Entity enInstallment)
        {
            tracingService.Trace("item :" + item.Id.ToString());
            tracingService.Trace("enInstallment :" + enInstallment.Id.ToString());
            var newDate = (DateTime)item["bsd_duedatenew"];
            if (enInstallment.Contains("bsd_duedate") == false) return;
            tracingService.Trace($"oldDate {enInstallment["bsd_duedate"]}");

            tracingService.Trace($"{enInstallment.Id}");
            tracingService.Trace($"newDate {newDate}");
            //if ((newDate - ((DateTime)enInstallment["bsd_duedate"]).AddHours(7)).TotalDays == 0|| (newDate - ((DateTime)enInstallment["bsd_duedate"])).TotalDays == 0)
            //{
                

            //    var mess = "The new due date is the same as the old due date. Please check again.";
            //    HandleError(item, mess);
            //    result = false;
            //}

        }
        /// <summary>
        /// Thực hiện hành động phê duyệt.
        /// Gọi một custom action "bsd_Action_Approved_Updateduedate_Master" để cập nhật các bản ghi liên quan.
        /// </summary>
        /// <param name="result">Biến cờ kết quả (không được sử dụng trong hàm này).</param>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail'.</param>
        /// <param name="enInstallment">Bản ghi 'Installment' liên quan (không được sử dụng trực tiếp).</param>
        public void Approve(ref bool result, Entity item, Entity enInstallment)
        {
            // Status Reason(entity cha) = Approved
            //Approved / Rejected Date[bsd_approvedrejecteddate] = Ngày cập nhật thành công
            //Approved / Rejected Person[bsd_approvedrejectedperson] = Người nhấn nút duyệt
            //Status Reason(entity Detail) = Approved
            //Due Date(New) (ở entity Detail) về field Due Date của installment ở entity Detail
            // Chuẩn bị gọi custom action
            var request = new OrganizationRequest("bsd_Action_Approved_Updateduedate_Master");

            var newDate = (DateTime)item["bsd_duedatenew"];
            // Truyền các tham số cần thiết cho action
            request["detail_id"] = item.Id.ToString();
            // Chuyển đổi ngày mới sang định dạng chuỗi, cộng 7 giờ để xử lý múi giờ (UTC+7)
            request["duedatenew"] = new DateTime(newDate.Year, newDate.Month, newDate.Day).AddHours(7).ToString();// newDate.AddHours(7).ToString();
            request["statuscode"] = 100000000; // Status code cho "Approved"
            // Thực thi action
            service.Execute(request);
        }
        /// <summary>
        /// Xử lý khi có lỗi xảy ra.
        /// Cập nhật bản ghi Master và Detail với thông tin lỗi.
        /// </summary>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail' gây ra lỗi.</param>
        /// <param name="error">Thông báo lỗi.</param>
        public void HandleError(Entity item, string error)
        {
            // Cập nhật bản ghi Master
            var enMasterRef = (EntityReference)item["bsd_updateduedate"];
            var enMaster = new Entity("bsd_updateduedate", enMasterRef.Id);
            enMaster["bsd_error"] = true;
            enMaster["bsd_errordetail"] = error;

            enMaster["bsd_approvedrejecteddate"] = null ;
            enMaster["bsd_approvedrejectedperson"] = null;

            service.Update(enMaster);
            // Cập nhật bản ghi Detail
            var enupdate= new Entity(en.LogicalName,en.Id);
            enupdate["statuscode"] = new OptionSetValue(100000003); // Chuyển status thành "Error"
            enupdate["bsd_errordetail"] = error;
            service.Update(enupdate);
        }
        /// <summary>
        /// Kiểm tra các điều kiện ban đầu để quyết định có thực thi plugin hay không.
        /// Ghi chú: Logic này hiện đang được vô hiệu hóa và mặc định trả về true.
        /// </summary>
        /// <param name="item">Bản ghi chi tiết 'Update Due Date Detail'.</param>
        /// <returns>True nếu plugin nên tiếp tục thực thi, false nếu không.</returns>
        public bool CheckConditionRun(Entity item)
        {
            return true;
            //var enMasterRef = (EntityReference)item["bsd_updateduedate"];
            //var enMaster = service.Retrieve("bsd_updateduedate", enMasterRef.Id, new ColumnSet(true));
            //if ((bool)enMaster["bsd_error"] == true && (bool)enMaster["bsd_processing_pa"] == false)
            //{
            //    var enupdate = new Entity(en.LogicalName, en.Id);
            //    enupdate["statuscode"] = new OptionSetValue(1);
            //    service.Update(enupdate);
            //    return false;
            //}
            //else
            //{
            //    return true;
            //}

        }
        ///// <summary>
        ///// Downloads a file or image
        ///// </summary>
        ///// <param name="service">The service</param>
        ///// <param name="entityReference">A reference to the record with the file or image column</param>
        ///// <param name="attributeName">The name of the file or image column</param>
        ///// <returns></returns>
        //private  void DownloadFile(
        //            )
        //{
        //    InitializeFileBlocksDownloadRequest initializeFileBlocksDownloadRequest = new InitializeFileBlocksDownloadRequest()
        //    {
        //        Target = new EntityReference("bsd_documents",new Guid("757e592a-7ff2-ef11-be20-0022485a0a4f")),
        //        FileAttributeName = "bsd_filewordtemplatedocx"
        //    };

        //    var initializeFileBlocksDownloadResponse =
        //          (InitializeFileBlocksDownloadResponse)service.Execute(initializeFileBlocksDownloadRequest);

        //    string fileContinuationToken = initializeFileBlocksDownloadResponse.FileContinuationToken;
        //    long fileSizeInBytes = initializeFileBlocksDownloadResponse.FileSizeInBytes;
        //    tracingService.Trace(fileContinuationToken);
        //    tracingService.Trace(fileSizeInBytes.ToString());

        //    List<byte> fileBytes = new List<byte>((int)fileSizeInBytes);

        //    long offset = 0;
        //    // If chunking is not supported, chunk size will be full size of the file.
        //    long blockSizeDownload = !initializeFileBlocksDownloadResponse.IsChunkingSupported ? fileSizeInBytes : 4 * 1024 * 1024;

        //    // File size may be smaller than defined block size
        //    if (fileSizeInBytes < blockSizeDownload)
        //    {
        //        blockSizeDownload = fileSizeInBytes;
        //    }
        //    tracingService.Trace(blockSizeDownload.ToString());
        //    while (fileSizeInBytes > 0)
        //    {
        //        // Prepare the request
        //        DownloadBlockRequest downLoadBlockRequest = new DownloadBlockRequest()
        //        {
        //            BlockLength = blockSizeDownload,
        //            FileContinuationToken = fileContinuationToken,
        //            Offset = offset
        //        };

        //        // Send the request
        //        var downloadBlockResponse =
        //                 (DownloadBlockResponse)service.Execute(downLoadBlockRequest);

        //        // Add the block returned to the list
        //        fileBytes.AddRange(downloadBlockResponse.Data);

        //        // Subtract the amount downloaded,
        //        // which may make fileSizeInBytes < 0 and indicate
        //        // no further blocks to download
        //        fileSizeInBytes -= (int)blockSizeDownload;
        //        // Increment the offset to start at the beginning of the next block.
        //        offset += blockSizeDownload;
        //    }
        //    string base64String = Convert.ToBase64String(fileBytes.ToArray());
        //    tracingService.Trace(base64String);
        //}
    }
}
