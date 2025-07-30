using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Create_OP_FillData_DAno
{
    public class Plugin_Create_OP_FillData_DAno : IPlugin
    {

        // Khai báo các biến dùng trong plugin
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();

        // Hàm thực thi plugin
        public void Execute(IServiceProvider serviceProvider)
        {
            // Lấy context thực thi plugin
            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            // Lấy factory để tạo service
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            // Tạo service để thao tác với dữ liệu CRM
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            // Lấy tracing service để ghi log
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Lấy entity từ InputParameters
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;
            // Lấy dữ liệu entity vừa tạo hoặc cập nhật
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            // Gọi hàm điền dữ liệu vào trường bsd_dano
            FillBsdDaNoField(enCreated, service, tracingService);
        }

        // Hàm điền dữ liệu vào trường bsd_dano
        private void FillBsdDaNoField(Entity entity, IOrganizationService service, ITracingService tracingService)
        {
            // Pseudocode:
            // 1. Lấy tên dự án và số option từ entity.
            // 2. Xử lý theo từng trường hợp tên dự án:
            //    - Heritage West Lake: lấy 3 ký tự trái + "/" + 4 ký tự phải
            //    - LUMI SIGNATURE: "LUMI/" + 4 ký tự phải
            //    - LUMI PRESTIGE: "LUMI PRESTIGE/" + 4 ký tự phải
            //    - SENIQUE I&II: "SENIQUE/" + 4 ký tự phải
            //    - SENIQUE PREMIER: "SENIQUE PREMIER/" + 4 ký tự phải
            //    - Mặc định: lấy 8 ký tự trái + "/" + 4 ký tự phải
            // 3. Gán giá trị cho trường bsd_dano.
            // 4. Ghi log từng bước.

            try
            {
                tracingService.Trace("Start FillBsdDaNoField");

                // Lấy tên dự án từ trường bsd_project (EntityReference)
                string projectName = entity.Contains("bsd_project") ? entity.GetAttributeValue<EntityReference>("bsd_project").Name : null;
                // Lấy số option từ trường bsd_optionno
                string optionNo = entity.Contains("bsd_optionno") ? entity.GetAttributeValue<string>("bsd_optionno") : null;

                tracingService.Trace($"Project Name: {projectName}");
                tracingService.Trace($"Option No: {optionNo}");

                // Kiểm tra nếu thiếu tên dự án hoặc số option thì bỏ qua
                if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(optionNo))
                {
                    tracingService.Trace("Project name or option no is missing. Skipping fill.");
                    return;
                }

                string bsdDaNo = string.Empty;

                // Xử lý theo từng trường hợp tên dự án
                switch (projectName.Trim().ToUpper())
                {
                    case "HERITAGE WEST LAKE":
                        tracingService.Trace("Case: HERITAGE WEST LAKE");
                        bsdDaNo = $"{Left(optionNo, 3)}/{Right(optionNo, 4)}";
                        break;
                    case "LUMI SIGNATURE":
                        tracingService.Trace("Case: LUMI SIGNATURE");
                        bsdDaNo = $"LUMI/{Right(optionNo, 4)}";
                        break;
                    case "LUMI PRESTIGE":
                        tracingService.Trace("Case: LUMI PRESTIGE");
                        bsdDaNo = $"LUMI PRESTIGE/{Right(optionNo, 4)}";
                        break;
                    case "SENIQUE I&II":
                        tracingService.Trace("Case: SENIQUE I&II");
                        bsdDaNo = $"SENIQUE/{Right(optionNo, 4)}";
                        break;
                    case "SENIQUE PREMIER":
                        tracingService.Trace("Case: SENIQUE PREMIER");
                        bsdDaNo = $"SENIQUE PREMIER/{Right(optionNo, 4)}";
                        break;
                    default:
                        tracingService.Trace("Case: Other Project");
                        bsdDaNo = $"{Left(optionNo, 8)}/{Right(optionNo, 4)}";
                        break;
                }

                tracingService.Trace($"bsd_dano value to set: {bsdDaNo}");

                // Gán giá trị cho trường bsd_dano
                entity["bsd_dano"] = bsdDaNo;
                // Cập nhật entity lên CRM
                service.Update(entity);

                tracingService.Trace("End FillBsdDaNoField");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Exception in FillBsdDaNoField: {ex.Message}");
                throw;
            }
        }

        // Hàm lấy ký tự bên trái của chuỗi
        private string Left(string input, int length)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= length ? input : input.Substring(0, length);
        }

        // Hàm lấy ký tự bên phải của chuỗi
        private string Right(string input, int length)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= length ? input : input.Substring(input.Length - length, length);
        }
    }

}
