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

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            //DownloadFile();
            //return;
            //get entity
            string enDetailid = context.InputParameters["id"].ToString();
            en = service.Retrieve("bsd_updateduedatedetail", new Guid(enDetailid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var item = en;
            if (!CheckConditionRun(en))
            {
                tracingService.Trace("stop");
                return;
            }
            var status = ((OptionSetValue)en["statuscode"]).Value;
            tracingService.Trace("start :" + status);
            tracingService.Trace("enDetailid :" + enDetailid);
            //check status
            var result = true;
            try
            {
                tracingService.Trace("start foreach");
                var enInstallmentRef = (EntityReference)item["bsd_installment"];
                var enInstallment = service.Retrieve(enInstallmentRef.LogicalName, enInstallmentRef.Id, new ColumnSet(true));
                #region check giao dịch
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
                            HandleError(item, "not found entity!");
                        }    

                    }

                }
                #endregion
                var enHDRef = (EntityReference)enInstallment[enIntalments_fieldNameHD];
                var enHD = service.Retrieve(enHDRef.LogicalName, enHDRef.Id, new ColumnSet(true));
                tracingService.Trace("CheckExistParentInDetail");
                CheckExistParentInDetail(ref result, item);
                tracingService.Trace("CheckIsLast");
                if (!result) return;

                CheckIsLast(ref result, item, enInstallment);
                if (!result) return;
                tracingService.Trace("CheckHD");

                CheckHD(ref result, item, enHD);
                tracingService.Trace("CheckPaidDetail");
                if (!result) return;

                CheckPaidDetail(ref result, item, enInstallment);
                tracingService.Trace("CheckDueDate");
                if (!result) return;

                CheckDueDate(ref result, item, enInstallment, enHD);
                tracingService.Trace("CheckNewDate");
                if (!result) return;

                CheckNewDate(ref result, item, enInstallment);
                tracingService.Trace("Approve");
                if (!result) return;

                Approve(ref result, item, enInstallment);
            }
            catch (Exception ex)
            {
                HandleError(item, ex.Message);
            }



           
        }
        /// <summary>
        ///  Dự án ở entity detail có trùng với entity Cha không?
        /// </summary>
        public void CheckExistParentInDetail(ref bool result, Entity item)
        {
            var enMasterRef = (EntityReference)item["bsd_updateduedate"];

            enMaster = service.Retrieve("bsd_updateduedate", enMasterRef.Id, new ColumnSet(true));
            if (((EntityReference)enMaster["bsd_project"]).Id != ((EntityReference)item["bsd_project"]).Id)
            {
                var mess = "The project in the Master and Detail entities does not match. Please check again.";
                HandleError(item, mess);

                result = false;
            }
        }
        /// <summary>
        /// Last Installment [bsd_lastinstallment] = yes 
        ///hoặc Duedate Calculating Method[bsd_duedatecalculatingmethod] =
        ///Estimate handover date?
        /// </summary>
        public void CheckIsLast(ref bool result, Entity item, Entity enInstallment)
        {
            if (((bool)enInstallment["bsd_lastinstallment"]) || (enInstallment.Contains("bsd_duedatecalculatingmethod") && ((OptionSetValue)enInstallment["bsd_duedatecalculatingmethod"]).Value == 100000002))
            {
                var mess = "The record contains a handover batch. Please check again.";
                HandleError(item, mess);
                result = false;
            }
        }
        /// <summary>
        ///  Từ đợt trong entity chi tiết kiểm tra các Hợp đồng có trạng thái =Terminated
        /// </summary>
        public void CheckHD(ref bool result, Entity item, Entity enHD)
        {
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
        /// kiểm tra đợt trong entity detail được thanh toán không? 
        /// (Kiểm tra 2 field: Amount Was Paid [bsd_amountwaspaid] ; 
        /// Deposit Amount Paid [bsd_depositamount] khác 0)
        /// </summary>
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
        /// Kiểm tra ngày đến hạn mới trên entity detail (Due Date (New) [bsd_duedatenew]) có nhỏ hơn đợt phía sau?
        /// Kiểm tra ngày đến hạn mới trên entity detail (Due Date (New) [bsd_duedatenew]) có lớn hơn đợt phía trước?
        /// </summary>
        /// <param name="result"></param>
        /// <param name="item"></param>
        /// <param name="enInstallment"></param>
        /// <param name="enHD"></param>
        public void CheckDueDate(ref bool result, Entity item, Entity enInstallment, Entity enHD)
        {

            var newDate = (DateTime)item["bsd_duedatenew"];

            var query = new QueryExpression(enInstallment.LogicalName);
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition(enIntalments_fieldNameHD, ConditionOperator.Equal, enHD.Id.ToString());
            query.Criteria.AddCondition("bsd_paymentschemedetailid", ConditionOperator.NotEqual, enInstallment.Id.ToString());
            var rs_ = service.RetrieveMultiple(query);
            tracingService.Trace($"enInstallment:{enInstallment.Id.ToString()}");
            var lstDetail=GetListDetail();
            foreach (var JItem in rs_.Entities)
            {
                tracingService.Trace($"JItem:{JItem.Id.ToString()}");

                if (JItem.Id != enInstallment.Id)
                {

                    if (JItem.Contains("bsd_duedate") == false) continue;
                    tracingService.Trace($"bsd_ordernumber:{(int)JItem["bsd_ordernumber"]}");
                    if (((int)JItem["bsd_ordernumber"]) < ((int)enInstallment["bsd_ordernumber"]))//lớn hơn đợt phí trước?
                    {
                        #region lấy item trong list detail nếu có
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
                    if (((int)JItem["bsd_ordernumber"]) > ((int)enInstallment["bsd_ordernumber"]))//nhở hơn đợt phí sau?
                    {
                        #region lấy item trong list detail nếu có
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
        public EntityCollection GetListDetail()
        {
            var query = new QueryExpression("bsd_updateduedatedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 100000003);
            query.Criteria.AddCondition("bsd_updateduedate", ConditionOperator.Equal, enMaster.Id.ToString());
            var rs_ = service.RetrieveMultiple(query);
            return rs_;
        }
        /// <summary>
        /// CNNDH 04.3
        ///Kiểm tra ngày mới có bằng ngày hiện tại
        ///Kiểm tra ngày đến hạn mới trên entity detail (Due Date (New) [bsd_duedatenew]) 
        ///trùng ngày đến hạn cũ Due Date (Old) [bsd_duedateold]
        /// </summary>
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
        public void Approve(ref bool result, Entity item, Entity enInstallment)
        {
            // Status Reason(entity cha) = Approved
            //Approved / Rejected Date[bsd_approvedrejecteddate] = Ngày cập nhật thành công
            //Approved / Rejected Person[bsd_approvedrejectedperson] = Người nhấn nút duyệt
            //Status Reason(entity Detail) = Approved
            //Due Date(New) (ở entity Detail) về field Due Date của installment ở entity Detail
            var request = new OrganizationRequest("bsd_Action_Approved_Updateduedate_Master");

            var newDate = (DateTime)item["bsd_duedatenew"];
            request["detail_id"] = item.Id.ToString();
            request["duedatenew"] = new DateTime(newDate.Year, newDate.Month, newDate.Day).AddHours(7).ToString();// newDate.AddHours(7).ToString();
            request["statuscode"] = 100000000;
            service.Execute(request);
        }
        public void HandleError(Entity item, string error)
        {
            var enMasterRef = (EntityReference)item["bsd_updateduedate"];
            var enMaster = new Entity("bsd_updateduedate", enMasterRef.Id);
            enMaster["bsd_error"] = true;
            enMaster["bsd_errordetail"] = error;

            enMaster["bsd_approvedrejecteddate"] = null ;
            enMaster["bsd_approvedrejectedperson"] = null;

            service.Update(enMaster);
            var enupdate= new Entity(en.LogicalName,en.Id);
            enupdate["statuscode"] = new OptionSetValue(100000003);
            enupdate["bsd_errordetail"] = error;
            service.Update(enupdate);
        }
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
