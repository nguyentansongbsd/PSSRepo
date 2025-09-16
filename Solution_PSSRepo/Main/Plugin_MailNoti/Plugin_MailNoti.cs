using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_MailNoti
{
    public class Plugin_MailNoti : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        string bsd_tensp = "";
        string bsd_tenkh = "";
        string bsd_tenduan = "";
        string tileTemplate = "";
        string teamname = "";
        string projectCode = "";
        public void Execute(IServiceProvider serviceProvider)
        {

            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("Bắt đầu thực thi Plugin_MailNoti.");

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            
            if (context.InputParameters == null || !context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
            {
                tracingService.Trace("Plugin được kích hoạt không phải trên một entity. Kết thúc.");
                return;
            }
            //get entity
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;

            en = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var status = ((OptionSetValue)en["statuscode"]).Value;
            tracingService.Trace("start :" + status);
            switch (en.LogicalName)
            {
                case "quote":
                    tracingService.Trace("Entity là Quote. Bắt đầu lấy thông tin liên quan.");
                    GetValue("bsd_projectid", "bsd_unitno", "customerid");
                    if (status == 100000000 || status == 100000006)
                    {
                        tileTemplate = "Reservation_Form";
                        teamname = $"{projectCode}-FINANCE-TEAM";
                    }
                    else if (status == 3)
                    {
                        tileTemplate = "Deposit";
                        teamname = $"{projectCode}-CCR-TEAM";
                    }
                    else if (status == 4)
                    {
                        tileTemplate = "Convert_Quotation_Reservation";
                        teamname = $"{projectCode}-FINANCE-TEAM";
                    }
                        break;
                case "salesorder":
                    tracingService.Trace("Entity là Sales Order. Bắt đầu lấy thông tin liên quan.");
                    GetValue("bsd_project", "bsd_unitnumber", "customerid");
                    if (status == 100000001)
                    {
                        tileTemplate = "1st_installment";
                        teamname = $"{projectCode}-CCR-TEAM";
                    }
                    break;
                default:
                    return;
            }
            if (!string.IsNullOrEmpty(tileTemplate))
            {
                tracingService.Trace($"Sẽ sử dụng email template có tên: '{tileTemplate}'");
                var enTemplate =GetTemplateByName(tileTemplate);
                if (enTemplate == null)
                    throw new InvalidPluginExecutionException($"Không tìm thấy Email Template với tên '{tileTemplate}'. Vui lòng kiểm tra lại cấu hình.");

                string linkRegarding = GetConfigValue("linkRegardingmail");
                
                linkRegarding = linkRegarding.Replace("{entityname}", en.LogicalName).Replace("{entityid}", en.Id.ToString("D"));
                #region maplink nếu chuyển từ quote lên OP
                if (en.LogicalName == "quote" && status == 4)
                {
                    tracingService.Trace($"lấy thông tin OP từ Quote");
                    QueryExpression query = new QueryExpression("salesorder");
                    query.ColumnSet = new ColumnSet("quoteid");
                    query.Criteria.AddCondition("quoteid", ConditionOperator.Equal, en.Id);

                    EntityCollection results = service.RetrieveMultiple(query);
                    string idOP = "";
                    if (results.Entities.Count > 0)
                    {
                        idOP = results.Entities[0].Id.ToString().Replace("{", "").Replace("}", "");
                        linkRegarding = linkRegarding.Replace("{entityname}", "salesorder").Replace("{entityid}", idOP);
                    }
                    
                }
                else
                {
                    linkRegarding = linkRegarding.Replace("{entityname}", en.LogicalName).Replace("{entityid}", en.Id.ToString(""));
                }
               
                #endregion
            
                #region create email
                tracingService.Trace("Bắt đầu tạo email.");
                string subject = enTemplate.GetAttributeValue<string>("subjectsafehtml").Replace("{bsd_tenda}", bsd_tenduan).Replace("{bsd_tensp}",bsd_tensp).Replace("bsd_tenkh",bsd_tenkh);
                string content= enTemplate.GetAttributeValue<string>("safehtml").Replace("{bsd_tenda}", bsd_tenduan).Replace("{bsd_tensp}", bsd_tensp).Replace("bsd_tenkh", bsd_tenkh).Replace("{link}",linkRegarding);
                Entity emailMessage = new Entity("email");
                // Thiết lập các thuộc tính cho email message
                emailMessage["description"] = content; // Nội dung email
                emailMessage["regardingobjectid"] = new EntityReference(en.LogicalName, en.Id);
                emailMessage["subject"] = subject;
                emailMessage["bsd_typemail"] = new OptionSetValue(100000001);
                AddTeamMembersToEmail(teamname, emailMessage);
                SetEmailSender(emailMessage);
                service.Create(emailMessage);
                tracingService.Trace("Tạo email thành công.");
                #endregion
            }
        }
        private void GetValue(string projectLookupField, string productLookupField, string customerLookupField)
        {
            var lookupNames = new Dictionary<string, string>();
            // Lấy tên khách hàng từ lookup
            if (en.Attributes.Contains(customerLookupField) && en[customerLookupField] is EntityReference customerRef)
            {
                tracingService.Trace($"Bắt đầu lấy tên khách hàng từ lookup '{customerLookupField}'.");
                var customerNameField = customerRef.LogicalName == "contact" ? "bsd_fullname" : "bsd_name";
                var customerEntity = service.Retrieve(customerRef.LogicalName, customerRef.Id, new ColumnSet(customerNameField));
                if (customerEntity != null)
                {
                    bsd_tenkh = customerEntity.GetAttributeValue<string>(customerNameField);
                }
            }
            // Lấy tên dự án từ lookup
            if (en.Attributes.Contains(projectLookupField) && en[projectLookupField] is EntityReference projectRef)
            {
                tracingService.Trace($"Bắt đầu lấy tên dự án từ lookup '{projectLookupField}'.");
                var projectEntity = service.Retrieve(projectRef.LogicalName, projectRef.Id, new ColumnSet("bsd_name","bsd_projectcode"));
                if (projectEntity != null)
                {
                    bsd_tenduan = projectEntity.GetAttributeValue<string>("bsd_name");
                    projectCode = projectEntity.GetAttributeValue<string>("bsd_projectcode");
                }
            }
            // Lấy tên sản phẩm từ lookup
            if (en.Attributes.Contains(productLookupField) && en[productLookupField] is EntityReference productRef)
            {
                tracingService.Trace($"Bắt đầu lấy tên sản phẩm từ lookup '{productLookupField}'.");
                var productEntity = service.Retrieve(productRef.LogicalName, productRef.Id, new ColumnSet("name"));
                if (productEntity != null)
                {
                    bsd_tensp = productEntity.GetAttributeValue<string>("name");
                }
            }
        }

        private string GetConfigValue(string key)
        {
            tracingService.Trace($"Bắt đầu lấy giá trị cấu hình với key: '{key}'.");
            QueryExpression query = new QueryExpression("bsd_configgolive");
            query.ColumnSet = new ColumnSet("bsd_url");
            query.Criteria.AddCondition("bsd_name", ConditionOperator.Equal, key);

            EntityCollection results = service.RetrieveMultiple(query);

            if (results.Entities.Count > 0)
            {
                Entity config = results.Entities[0];
                if (config.Contains("bsd_url"))
                {
                    var value = config.GetAttributeValue<string>("bsd_url");
                    tracingService.Trace($"Tìm thấy giá trị cấu hình: '{value}'.");
                    return value;
                }
            }
            
            tracingService.Trace($"Không tìm thấy giá trị cấu hình cho key: '{key}'.");
            throw new InvalidPluginExecutionException($"Không tìm thấy cấu hình (bsd_configgolive) với tên (bsd_name) là '{key}'. Vui lòng kiểm tra lại.");
        }

        private Entity GetTemplateByName(string name)
        {
            tracingService.Trace($"Bắt đầu tìm template với tên: '{name}'.");
            QueryExpression query = new QueryExpression("template");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("title", ConditionOperator.Equal, name);
            EntityCollection results = service.RetrieveMultiple(query);
            if (results.Entities.Count > 0)
            {
                tracingService.Trace("Tìm thấy template.");
                return results.Entities[0];
            }
            return null;
        }

        /// <summary>
        /// Lấy tất cả người dùng trong một team và thêm vào trường 'To' của một email.
        /// </summary>
        /// <param name="teamName">Tên của team.</param>
        /// <param name="email">Entity email cần cập nhật.</param>
        /// <returns>Entity email đã được cập nhật với danh sách người nhận.</returns>
        private Entity AddTeamMembersToEmail(string teamName, Entity email)
        {
            tracingService.Trace($"Bắt đầu thêm thành viên từ team '{teamName}' vào email.");

            // 1. Tìm team theo tên để lấy ID
            QueryExpression teamQuery = new QueryExpression("team");
            teamQuery.ColumnSet = new ColumnSet("teamid");
            teamQuery.Criteria.AddCondition("name", ConditionOperator.Equal, teamName);
            EntityCollection teams = service.RetrieveMultiple(teamQuery);

            if (teams.Entities.Count == 0)
            {
                throw new InvalidPluginExecutionException($"Không tìm thấy team với tên '{teamName}'. Vui lòng kiểm tra lại.");
            }
            Guid teamId = teams.Entities[0].Id;
            tracingService.Trace($"Tìm thấy team '{teamName}' với ID: {teamId}.");

            // 2. Lấy tất cả user thuộc team đó thông qua bảng teammembership
            QueryExpression userQuery = new QueryExpression("systemuser");
            userQuery.ColumnSet = new ColumnSet("systemuserid"); // Chỉ cần lấy ID
            LinkEntity teamMembershipLink = new LinkEntity("systemuser", "teammembership", "systemuserid", "systemuserid", JoinOperator.Inner);
            teamMembershipLink.LinkCriteria.AddCondition("teamid", ConditionOperator.Equal, teamId);
            userQuery.LinkEntities.Add(teamMembershipLink);

            EntityCollection users = service.RetrieveMultiple(userQuery);

            if (users.Entities.Count == 0)
            {
                tracingService.Trace($"Không tìm thấy user nào trong team '{teamName}'.");
                return email; // Không có user nào để gửi, có thể bỏ qua hoặc ghi log
            }

            // 3. Tạo danh sách ActivityParty cho những người nhận (trường 'To')
            List<Entity> toParties = new List<Entity>();
            foreach (var user in users.Entities)
            {
                Entity toParty = new Entity("activityparty");
                toParty["partyid"] = new EntityReference("systemuser", user.Id);
                toParties.Add(toParty);
            }

            // 4. Gán danh sách người nhận vào trường 'to' của email
            email["to"] = new EntityCollection(toParties);
            tracingService.Trace($"Đã thêm thành công {users.Entities.Count} thành viên vào trường 'To' của email.");

            return email;
        }

        /// <summary>
        /// Gán người dùng đang thực thi plugin làm người gửi (From) của email.
        /// </summary>
        /// <param name="email">Entity email cần cập nhật.</param>
        /// <returns>Entity email đã được cập nhật với người gửi.</returns>
        private Entity SetEmailSender(Entity email)
        {
            tracingService.Trace("Bắt đầu thiết lập người gửi (From) cho email.");
            // Tạo một ActivityParty cho người gửi
            Entity fromParty = new Entity("activityparty");
            // partyid trỏ đến người dùng đang thực thi plugin
            fromParty["partyid"] = new EntityReference("systemuser", context.UserId);
            // Trường 'from' là một EntityCollection, mặc dù nó chỉ chứa một người gửi
            email["from"] = new EntityCollection(new List<Entity> { fromParty });
            tracingService.Trace($"Đã thiết lập người gửi là user có ID: {context.UserId}.");

            return email;
        }
    }
}
