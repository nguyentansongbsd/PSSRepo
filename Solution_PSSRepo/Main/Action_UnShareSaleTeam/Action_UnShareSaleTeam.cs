using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Action_UnShareSaleTeam
{
    public class Action_UnShareSaleTeam : IPlugin
    {
        IOrganizationService service = null;
        IPluginExecutionContext context = null;
        ITracingService traceService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Guid CurrentUser = context.UserId;
            service = factory.CreateOrganizationService(CurrentUser);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");

            try
            {
                // BƯỚC 1: Lấy các tham số đầu vào từ Action
                // Lưu ý: Tên tham số 'TargetEntityName' và 'TargetEntityId' phải khớp với định nghĩa trong Custom Action của bạn.
                traceService.Trace("Bắt đầu lấy tham số đầu vào.");
                if (!context.InputParameters.Contains("TargetEntityName") || !context.InputParameters.Contains("TargetEntityId"))
                {
                    throw new InvalidPluginExecutionException("Các tham số đầu vào TargetEntityName và TargetEntityId là bắt buộc.");
                }

                string entityName = (string)context.InputParameters["TargetEntityName"];
                Guid entityId;
                // Phân tích GUID từ chuỗi một cách an toàn hơn
                if (!Guid.TryParse(context.InputParameters["TargetEntityId"].ToString(), out entityId))
                {
                    throw new InvalidPluginExecutionException("Giá trị của TargetEntityId không phải là một GUID hợp lệ.");
                }

                if (string.IsNullOrEmpty(entityName) || entityId == Guid.Empty)
                {
                    traceService.Trace("Tham số đầu vào không hợp lệ.");
                    throw new InvalidPluginExecutionException("Giá trị của EntityName và EntityId không được rỗng.");
                }

                EntityReference targetRecord = new EntityReference(entityName, entityId);
                traceService.Trace($"Đang xử lý bản ghi: {entityName}, ID: {entityId}");

                // BƯỚC 2: Lấy ObjectTypeCode của entity để sử dụng trong truy vấn POA
                traceService.Trace($"Lấy ObjectTypeCode cho entity: {entityName}");
                RetrieveEntityRequest entityRequest = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    LogicalName = entityName
                };
                RetrieveEntityResponse entityResponse = (RetrieveEntityResponse)service.Execute(entityRequest);
                int objectTypeCode = entityResponse.EntityMetadata.ObjectTypeCode.GetValueOrDefault();

                if (objectTypeCode == 0)
                {
                    throw new InvalidPluginExecutionException($"Không thể tìm thấy ObjectTypeCode cho entity '{entityName}'.");
                }
                traceService.Trace($"ObjectTypeCode là: {objectTypeCode}");

                // BƯỚC 3: Dùng FetchXML để truy vấn trực tiếp bảng PrincipalObjectAccess (POA)
                // Lấy các team có tên kết thúc bằng '-SALE-TEAM' đang được share bản ghi này
                traceService.Trace("Sử dụng FetchXML để truy vấn trực tiếp bảng POA.");
                string fetchXml = $@"
                    <fetch distinct='true'>
                      <entity name='principalobjectaccess'>
                        <link-entity name='team' from='teamid' to='principalid' alias='t'>
                          <attribute name='name' />
                          <attribute name='teamid' />
                          <filter>
                            <condition attribute='name' operator='eq' value='cl' />
                          </filter>
                        </link-entity>
                        <filter type='and'>
                          <condition attribute='objectid' operator='eq' value='{entityId}' />
                          <condition attribute='objecttypecode' operator='eq' value='{objectTypeCode}' />
                        </filter>
                      </entity>
                    </fetch>";
                #region check xem có thuộc logic share team CL không
                if (entityName == "contact") //nếu contact là primary contact của account có bsd_businesstypesys là 100000003 || 100000002 thì không unshare
                {
                    var demo_bsd_businesstypesys_1 = 100000003;
                    var demo_bsd_businesstypesys_2 = 100000002;

                    var query = new QueryExpression("contact")
                    {
                        ColumnSet = new ColumnSet("contactid"),
                        LinkEntities =
                                        {
                                            new LinkEntity("contact", "account", "contactid", "primarycontactid", JoinOperator.Inner)
                                            {
                                                EntityAlias = "demo",
                                                Columns = new ColumnSet("bsd_businesstypesys"),
                                                LinkCriteria =
                                                {
                                                    FilterOperator = LogicalOperator.Or,
                                                    Conditions =
                                                    {
                                                        new ConditionExpression("bsd_businesstypesys", ConditionOperator.ContainValues, demo_bsd_businesstypesys_1, demo_bsd_businesstypesys_2)
                                                    }
                                                }
                                            }
                                        }
                    };
                    var contactids = service.RetrieveMultiple(query).Entities.Select(e => e.Id.ToString()).ToList(); 
                    traceService.Trace($"Số lượng contact thuộc logic share team CL: {contactids.Count()}");
                    traceService.Trace($"Danh sách contact thuộc logic share team CL: {string.Join(", ", contactids)}");
                    if (contactids.Contains(entityId.ToString()))
                    {
                        traceService.Trace(" KHCN thuộc logic share CL, không thực hiện unshare team CL");
                        return;
                    }
                }
                if(entityName=="account")
                {
                    var account= service.Retrieve("account", entityId, new ColumnSet(true));
                    if(account.Contains("bsd_businesstypesys") &&( 
                         ((OptionSetValueCollection)account["bsd_businesstypesys"]).Contains(new OptionSetValue(100000002))||((OptionSetValueCollection)account["bsd_businesstypesys"]).Contains(new OptionSetValue(100000003))))
                    {
                        traceService.Trace(" KHDN thuộc logic share CL, không thực hiện unshare team CL");
                        return;
                    }
                }    

                #endregion
                EntityCollection sharedTeams = service.RetrieveMultiple(new FetchExpression(fetchXml));
                traceService.Trace($"Tìm thấy {sharedTeams.Entities.Count} team phù hợp để unshare.");

                // BƯỚC 4: Thu hồi quyền truy cập của các team đã lọc được
                foreach (var poaEntity in sharedTeams.Entities)
                {
                    // Lấy thông tin team từ linked-entity alias
                    Guid teamId = (Guid)((AliasedValue)poaEntity["t.teamid"]).Value;
                    string teamName = (string)((AliasedValue)poaEntity["t.name"]).Value;

                    traceService.Trace($"Bắt đầu thu hồi quyền của team: '{teamName}' (ID: {teamId}).");

                    RevokeAccessRequest revokeRequest = new RevokeAccessRequest
                    {
                        Target = targetRecord,
                        Revokee = new EntityReference("team", teamId)
                    };

                    service.Execute(revokeRequest);
                    traceService.Trace($"Đã thu hồi quyền truy cập của team '{teamName}'.");
                }
            }
            catch (Exception ex)
            {
                traceService.Trace($"Đã xảy ra lỗi: {ex.ToString()}");
                // Ném ra lỗi để thông báo cho người dùng trên giao diện Dynamics 365
                throw new InvalidPluginExecutionException("Đã có lỗi xảy ra trong plugin Action_UnShareSaleTeam.", ex);
            }

            traceService.Trace("end");
        }
    }
}
