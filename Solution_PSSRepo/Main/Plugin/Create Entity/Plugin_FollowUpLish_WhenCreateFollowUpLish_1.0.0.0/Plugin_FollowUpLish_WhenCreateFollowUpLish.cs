// Decompiled with JetBrains decompiler
// Type: Plugin_FollowUpLish_WhenCreateFollowUpLish.Plugin_FollowUpLish_WhenCreateFollowUpLish
// Assembly: Plugin_FollowUpLish_WhenCreateFollowUpLish, Version=1.0.0.0, Culture=neutral, PublicKeyToken=cd2383821fd946cc
// MVID: 4DB237FC-C8EE-4A96-B9D1-2850D87E26CE
// Assembly location: C:\Users\ngoct\Downloads\New folder (3)\Plugin_FollowUpLish_WhenCreateFollowUpLish_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Plugin_FollowUpLish_WhenCreateFollowUpLish
{
    public class Plugin_FollowUpLish_WhenCreateFollowUpLish : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            // Bước 1: Lấy context thực thi plugin
            IPluginExecutionContext service = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Bước 2: Kiểm tra xem InputParameters có chứa "Target" và "Target" có phải là Entity không
            if (!service.InputParameters.Contains("Target") || !(service.InputParameters["Target"] is Entity))
                return;

            // Bước 3: Lấy entity từ InputParameters
            Entity inputParameter = (Entity)service.InputParameters["Target"];
            Guid id = inputParameter.Id;

            // Bước 4: Kiểm tra điều kiện thực thi plugin (chỉ thực thi khi tạo mới bsd_followuplist và có bsd_reservation hoặc bsd_optionentry)
            if (inputParameter.LogicalName == "bsd_followuplist" && service.MessageName == "Create" && (inputParameter.Contains("bsd_reservation") || inputParameter.Contains("bsd_optionentry")))
            {
                // Bước 5: Khởi tạo service để thao tác với CRM
                this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));

                // Bước 6: Ghi log độ sâu context để debug
                ((ITracingService)serviceProvider.GetService(typeof(ITracingService))).Trace(string.Format("Context Depth {0}", (object)service.Depth));

                // Bước 7: Nếu độ sâu lớn hơn 1 thì không thực thi tiếp (tránh lặp vô hạn)
                if (service.Depth > 1)
                    return;

                // Bước 8: Nếu có trường bsd_reservation
                if (inputParameter.Contains("bsd_reservation"))
                {
                    // Bước 9: Lấy thông tin entity reservation từ CRM
                    Entity entity = this.service.Retrieve(((EntityReference)inputParameter["bsd_reservation"]).LogicalName, ((EntityReference)inputParameter["bsd_reservation"]).Id, new ColumnSet(new string[2]
                    {
                "statecode",
                "statuscode"
                    }));

                    // Bước 10: Nếu trạng thái statecode = 2 (đã won) thì báo lỗi và dừng plugin
                    if (((OptionSetValue)entity["statecode"]).Value == 2)
                        throw new InvalidPluginExecutionException("This Reservation had been won. Please check again.");

                    // Bước 11: Nếu có trường statuscode
                    if (entity.Contains("statuscode"))
                    {
                        int num = ((OptionSetValue)entity["statuscode"]).Value;

                        // Bước 12: Nếu statuscode = 3 thì chuyển trạng thái về state = 0, status = 100000000
                        if (num == 3)
                            this.service.Execute((OrganizationRequest)new SetStateRequest()
                            {
                                EntityMoniker = new EntityReference()
                                {
                                    Id = entity.Id,
                                    LogicalName = entity.LogicalName
                                },
                                State = new OptionSetValue(0),
                                Status = new OptionSetValue(100000000)
                            });

                        // Bước 13: Cập nhật trường bsd_followuplist = true cho entity reservation
                        this.service.Update(new Entity(entity.LogicalName)
                        {
                            Id = entity.Id,
                            ["bsd_followuplist"] = (object)true
                        });

                        // Bước 14: Nếu statuscode = 3 thì chuyển lại trạng thái ban đầu state = 1, status = num
                        if (num == 3)
                            this.service.Execute((OrganizationRequest)new SetStateRequest()
                            {
                                EntityMoniker = new EntityReference()
                                {
                                    Id = entity.Id,
                                    LogicalName = entity.LogicalName
                                },
                                State = new OptionSetValue(1),
                                Status = new OptionSetValue(num)
                            });
                    }
                }
                // Bước 15: Nếu có trường bsd_optionentry thì cập nhật trường bsd_followuplist = true cho entity optionentry
                else if (inputParameter.Contains("bsd_optionentry"))
                    this.service.Update(new Entity(((EntityReference)inputParameter["bsd_optionentry"]).LogicalName)
                    {
                        Id = ((EntityReference)inputParameter["bsd_optionentry"]).Id,
                        ["bsd_followuplist"] = (object)true
                    });
            }
        }
    }
}
