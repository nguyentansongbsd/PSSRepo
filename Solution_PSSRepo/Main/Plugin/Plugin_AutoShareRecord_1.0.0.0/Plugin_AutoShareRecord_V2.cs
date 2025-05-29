// Decompiled with JetBrains decompiler
// Type: Plugin_AutoShareRecord.Plugin_AutoShareRecord
// Assembly: Plugin_AutoShareRecord, Version=1.0.0.0, Culture=neutral, PublicKeyToken=13a5ff2c1aad95bf
// MVID: F61B661D-1A17-48B9-A0D6-1E80740C65F7
// Assembly location: C:\Users\ngoct\Downloads\Plugin_AutoShareRecord_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.ConstrainedExecution;

namespace Plugin_AutoShareRecord
{
    public class Plugin_AutoShareRecord_V2 : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        private string strSM = "SALE";
        private string strCCR = "CCR";
        private string strFIN = "FINANCE";
        private string strSMGT = "SALE-MGT";

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service1.UserId));
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");
            if (!(service1.MessageName == "Create") && !(service1.MessageName == "Update"))
                return;
            Entity inputParameter = (Entity)service1.InputParameters["Target"];
            string logicalName = inputParameter.LogicalName;
            Entity entity1 = new Entity();
            //List<string> stringList = new List<string>();
            //stringList.Add("bsd_payment");
            //stringList.Add("bsd_advancepayment");
            //stringList.Add("bsd_customernotices");
            //stringList.Add("bsd_warningnotices");
            //stringList.Add("bsd_handovernotice");
            ////stringList.Add("bsd_terminateletter");
            //stringList.Add("bsd_applydocument");
            //stringList.Add("bsd_voidpayment");
            //stringList.Add("bsd_refund");
            //stringList.Add("bsd_transfermoney");
            ////stringList.Add("bsd_approvechangeduedateinstallment");
            ////stringList.Add("bsd_updateestimatehandoverdate");
            ////stringList.Add("bsd_updateduedateoflastinstallmentapprove");
            ////stringList.Add("bsd_updatelandvalue");
            ////stringList.Add("bsd_waiverapproval");
            ////stringList.Add("bsd_bulkwaiver");
            if (service1.MessageName == "Create")
            {
                //    if (stringList.Contains(inputParameter.LogicalName))
                //    {
                //        Entity entity2 = this.service.Retrieve("bsd_project", ((EntityReference)this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[1]
                //        {
                //"bsd_project"
                //        }))["bsd_project"]).Id, new ColumnSet(new string[2]
                //        {
                //"bsd_projectcode",
                //"bsd_name"
                //        }));
                //        string ProjectCode = entity2.Contains("bsd_projectcode") ? (string)entity2["bsd_projectcode"] : "";
                //        int num;
                //        bool SaleMgtTeam = (num = 0) != 0;
                //        bool CcrTeam = num != 0;
                //        bool SaleTeam = num != 0;
                //        bool FinTeam = true;
                //        EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                //        if (teamAccess.Entities.Count > 0)
                //        {
                //            foreach (Entity entity3 in (Collection<Entity>)teamAccess.Entities)
                //                this.Role_SharePrivileges(logicalName, inputParameter.Id, entity3.Id, true, true, true, this.service, FinTeam);
                //        }
                //    }
                //    if (inputParameter.LogicalName == "opportunity" && inputParameter.Contains("bsd_project"))
                //    {
                //        Entity entity4 = this.service.Retrieve("bsd_project", ((EntityReference)this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[1]
                //        {
                //"bsd_project"
                //        }))["bsd_project"]).Id, new ColumnSet(new string[2]
                //        {
                //"bsd_projectcode",
                //"bsd_name"
                //        }));
                //        string ProjectCode = entity4.Contains("bsd_projectcode") ? (string)entity4["bsd_projectcode"] : "";
                //        bool SaleTeam = false;
                //        int num;
                //        bool FinTeam = (num = 1) != 0;
                //        bool CcrTeam = num != 0;
                //        bool SaleMgtTeam = num != 0;
                //        EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                //        if (teamAccess.Entities.Count > 0)
                //        {
                //            foreach (Entity entity5 in (Collection<Entity>)teamAccess.Entities)
                //                this.Role_SharePrivileges(logicalName, inputParameter.Id, entity5.Id, true, true, true, this.service, false);
                //        }
                //    }
                //    if (inputParameter.LogicalName == "quote")
                //    {
                //        Entity entity6 = this.service.Retrieve("bsd_project", ((EntityReference)this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[1]
                //        {
                //"bsd_projectid"
                //        }))["bsd_projectid"]).Id, new ColumnSet(new string[2]
                //        {
                //"bsd_projectcode",
                //"bsd_name"
                //        }));
                //        string ProjectCode = entity6.Contains("bsd_projectcode") ? (string)entity6["bsd_projectcode"] : "";
                //        bool SaleTeam = false;
                //        int num;
                //        bool FinTeam = (num = 1) != 0;
                //        bool CcrTeam = num != 0;
                //        bool SaleMgtTeam = num != 0;
                //        EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                //        if (teamAccess.Entities.Count > 0)
                //        {
                //            foreach (Entity entity7 in (Collection<Entity>)teamAccess.Entities)
                //                this.Role_SharePrivileges(logicalName, inputParameter.Id, entity7.Id, true, true, true, this.service, false);
                //        }
                //    }
                //    if (inputParameter.LogicalName == "salesorder")
                //    {
                //        Entity entity8 = this.service.Retrieve("bsd_project", ((EntityReference)this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[1]
                //        {
                //"bsd_project"
                //        }))["bsd_project"]).Id, new ColumnSet(new string[2]
                //        {
                //"bsd_projectcode",
                //"bsd_name"
                //        }));
                //        string ProjectCode = entity8.Contains("bsd_projectcode") ? (string)entity8["bsd_projectcode"] : "";
                //        int num;
                //        bool FinTeam = (num = 1) != 0;
                //        bool CcrTeam = num != 0;
                //        bool SaleTeam = num != 0;
                //        bool SaleMgtTeam = num != 0;
                //        EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                //        if (teamAccess.Entities.Count > 0)
                //        {
                //            foreach (Entity entity9 in (Collection<Entity>)teamAccess.Entities)
                //            {
                //                if ((entity9.Contains("name") ? entity9.Attributes["name"].ToString() : "").IndexOf(this.strSM) != -1)
                //                    this.Role_SharePrivileges(logicalName, inputParameter.Id, entity9.Id, true, false, true, this.service, false);
                //                else
                //                    this.Role_SharePrivileges(logicalName, inputParameter.Id, entity9.Id, true, true, true, this.service, false);
                //            }
                //        }
                //    }
                //    if (inputParameter.LogicalName == "bsd_termination")
                //    {
                //        Entity entity10 = this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[1]
                //        {
                //"bsd_units"
                //        }));
                //        Entity entity11 = this.service.Retrieve("bsd_project", ((EntityReference)this.service.Retrieve(((EntityReference)entity10["bsd_units"]).LogicalName, ((EntityReference)entity10["bsd_units"]).Id, new ColumnSet(new string[1]
                //        {
                //"bsd_projectcode"
                //        }))["bsd_projectcode"]).Id, new ColumnSet(new string[2]
                //        {
                //"bsd_projectcode",
                //"bsd_name"
                //        }));
                //        string ProjectCode = entity11.Contains("bsd_projectcode") ? (string)entity11["bsd_projectcode"] : "";
                //        int num;
                //        bool FinTeam = (num = 1) != 0;
                //        bool CcrTeam = num != 0;
                //        bool SaleTeam = num != 0;
                //        bool SaleMgtTeam = num != 0;
                //        EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                //        if (teamAccess.Entities.Count > 0)
                //        {
                //            foreach (Entity entity12 in (Collection<Entity>)teamAccess.Entities)
                //            {
                //                if ((entity12.Contains("name") ? entity12.Attributes["name"].ToString() : "").IndexOf(this.strSM) != -1)
                //                    this.Role_SharePrivileges(logicalName, inputParameter.Id, entity12.Id, true, false, true, this.service, false);
                //                else
                //                    this.Role_SharePrivileges(logicalName, inputParameter.Id, entity12.Id, true, true, true, this.service, false);
                //            }
                //        }
                //    }
                if (inputParameter.LogicalName == "bsd_coowner")
                {
                    Entity entity13 = this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[3]
                    {
            "bsd_reservation",
            "bsd_optionentry",
            "bsd_subsale"
                    }));
                    Entity entity14 = new Entity();
                    Guid guid = new Guid();
                    Guid id;
                    if (entity13.Contains("bsd_reservation"))
                        id = ((EntityReference)this.service.Retrieve(((EntityReference)entity13["bsd_reservation"]).LogicalName, ((EntityReference)entity13["bsd_reservation"]).Id, new ColumnSet(new string[1]
                        {
              "bsd_projectid"
                        }))["bsd_projectid"]).Id;
                    else if (entity13.Contains("bsd_optionentry"))
                    {
                        id = ((EntityReference)this.service.Retrieve(((EntityReference)entity13["bsd_optionentry"]).LogicalName, ((EntityReference)entity13["bsd_optionentry"]).Id, new ColumnSet(new string[1]
                        {
              "bsd_project"
                        }))["bsd_project"]).Id;
                    }
                    else
                    {
                        if (!entity13.Contains("bsd_subsale"))
                            throw new InvalidPluginExecutionException("Cannot create Co-owner!");
                        id = ((EntityReference)this.service.Retrieve(((EntityReference)entity13["bsd_subsale"]).LogicalName, ((EntityReference)entity13["bsd_subsale"]).Id, new ColumnSet(new string[1]
                        {
              "bsd_project"
                        }))["bsd_project"]).Id;
                    }
                    Entity entity15 = this.service.Retrieve("bsd_project", id, new ColumnSet(new string[2]
                    {
            "bsd_projectcode",
            "bsd_name"
                    }));
                    string ProjectCode = entity15.Contains("bsd_projectcode") ? (string)entity15["bsd_projectcode"] : "";
                    int num;
                    bool FinTeam = (num = 1) != 0;
                    bool CcrTeam = num != 0;
                    bool SaleTeam = num != 0;
                    bool SaleMgtTeam = num != 0;
                    EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                    if (teamAccess.Entities.Count > 0)
                    {
                        foreach (Entity entity16 in (Collection<Entity>)teamAccess.Entities)
                            this.Role_SharePrivileges(logicalName, inputParameter.Id, entity16.Id, true, true, true, this.service, false);
                    }
                }
                //    if (inputParameter.LogicalName == "bsd_assign")
                //    {
                //        Entity entity17 = this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[3]
                //        {
                //"bsd_project",
                //"bsd_currentcustomer",
                //"bsd_newcustomer"
                //        }));
                //        Entity entity18 = this.service.Retrieve("bsd_project", ((EntityReference)entity17["bsd_project"]).Id, new ColumnSet(new string[2]
                //        {
                //"bsd_projectcode",
                //"bsd_name"
                //        }));
                //        string ProjectCode = entity18.Contains("bsd_projectcode") ? (string)entity18["bsd_projectcode"] : "";
                //        int num;
                //        bool FinTeam = (num = 0) != 0;
                //        bool SaleTeam = num != 0;
                //        bool SaleMgtTeam = num != 0;
                //        bool CcrTeam = true;
                //        EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                //        if (teamAccess.Entities.Count > 0)
                //        {
                //            foreach (Entity entity19 in (Collection<Entity>)teamAccess.Entities)
                //            {
                //                this.Role_SharePrivileges(logicalName, inputParameter.Id, entity19.Id, true, true, true, this.service, false);
                //                if (entity17.Contains("bsd_currentcustomer"))
                //                    this.Role_SharePrivileges(((EntityReference)entity17["bsd_currentcustomer"]).LogicalName, ((EntityReference)entity17["bsd_currentcustomer"]).Id, entity19.Id, true, true, true, this.service, false);
                //                if (entity17.Contains("bsd_newcustomer"))
                //                    this.Role_SharePrivileges(((EntityReference)entity17["bsd_newcustomer"]).LogicalName, ((EntityReference)entity17["bsd_newcustomer"]).Id, entity19.Id, true, true, true, this.service, false);
                //            }
                //        }
                //    }
                //    if (inputParameter.LogicalName == "bsd_paymentschemedetail" && inputParameter.Contains("bsd_reservation"))
                //    {
                //        Entity entity20 = this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[2]
                //        {
                //"bsd_project",
                //"bsd_reservation"
                //        }));
                //        Entity entity21 = this.service.Retrieve("bsd_project", ((EntityReference)this.service.Retrieve(((EntityReference)entity20["bsd_reservation"]).LogicalName, ((EntityReference)entity20["bsd_reservation"]).Id, new ColumnSet(new string[1]
                //        {
                //"bsd_projectid"
                //        }))["bsd_projectid"]).Id, new ColumnSet(new string[2]
                //        {
                //"bsd_projectcode",
                //"bsd_name"
                //        }));
                //        string ProjectCode = entity21.Contains("bsd_projectcode") ? (string)entity21["bsd_projectcode"] : "";
                //        bool SaleTeam = false;
                //        int num;
                //        bool FinTeam = (num = 1) != 0;
                //        bool CcrTeam = num != 0;
                //        bool SaleMgtTeam = num != 0;
                //        EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                //        if (teamAccess.Entities.Count > 0)
                //        {
                //            foreach (Entity entity22 in (Collection<Entity>)teamAccess.Entities)
                //                this.Role_SharePrivileges(logicalName, inputParameter.Id, entity22.Id, true, true, true, this.service, false);
                //        }
                //    }
                //    if (inputParameter.LogicalName == "bsd_followuplist")
                //    {
                //        Entity entity23 = this.service.Retrieve("bsd_project", ((EntityReference)this.service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(new string[1]
                //        {
                //"bsd_project"
                //        }))["bsd_project"]).Id, new ColumnSet(new string[2]
                //        {
                //"bsd_projectcode",
                //"bsd_name"
                //        }));
                //        string ProjectCode = entity23.Contains("bsd_projectcode") ? (string)entity23["bsd_projectcode"] : "";
                //        int num;
                //        bool FinTeam = (num = 1) != 0;
                //        bool CcrTeam = num != 0;
                //        bool SaleTeam = num != 0;
                //        bool SaleMgtTeam = num != 0;
                //        EntityCollection teamAccess = this.Get_TeamAccess(ProjectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                //        if (teamAccess.Entities.Count > 0)
                //        {
                //            foreach (Entity entity24 in (Collection<Entity>)teamAccess.Entities)
                //            {
                //                if ((entity24.Contains("name") ? entity24.Attributes["name"].ToString() : "").IndexOf(this.strSM) != -1)
                //                    this.Role_SharePrivileges(logicalName, inputParameter.Id, entity24.Id, true, false, true, this.service, false);
                //                else
                //                    this.Role_SharePrivileges(logicalName, inputParameter.Id, entity24.Id, true, true, true, this.service, false);
                //            }
                //        }
                //    }

                Plugin_AutoShareRecord_V2_1.Run_Create(service, traceService, inputParameter, service1);
            }
            if (service1.MessageName == "Update")
            {
                if (inputParameter.LogicalName == "bsd_discount")
                {
                    var en = service.Retrieve(logicalName, inputParameter.Id, new ColumnSet(true));
                    if (!en.Contains("bsd_applyafterpl") || ((bool)en["bsd_applyafterpl"]) == false)
                        return;
                    if (((OptionSetValue)en["statuscode"]).Value == 100000000)
                    {
                        // Lấy thông tin của thực thể dự án từ dịch vụ
                        Entity projectEntity = this.service.Retrieve(
                            "bsd_project",
                            ((EntityReference)this.service.Retrieve(
                                logicalName,
                                inputParameter.Id,
                                new ColumnSet("bsd_project")
                            )["bsd_project"]).Id,
                            new ColumnSet("bsd_projectcode", "bsd_name")
                        );
                        // Kiểm tra xem dự án có chứa mã dự án hay không và gán giá trị cho biến ProjectCode
                        string projectCode = projectEntity.Contains("bsd_projectcode")
                            ? (string)projectEntity["bsd_projectcode"]
                            : "";
                        int num;
                        bool FinTeam = (num = 1) != 0;
                        bool CcrTeam = num != 0;
                        bool SaleTeam = num != 0;
                        bool SaleMgtTeam = num != 0;
                        EntityCollection teamAccess = this.Get_TeamAccess(projectCode, SaleTeam, CcrTeam, FinTeam, SaleMgtTeam);
                        foreach (Entity entity in teamAccess.Entities)
                        {
                            traceService.Trace($"teams {entity["name"]}");
                        }
                        if (teamAccess.Entities.Count > 0)
                        {
                            foreach (Entity entity9 in (Collection<Entity>)teamAccess.Entities)
                            {

                                if (entity9.Attributes["name"].ToString().Contains("-CCR") ||
                                    entity9.Attributes["name"].ToString().Contains(strFIN) ||
                                    entity9.Attributes["name"].ToString().Contains(strSMGT))
                                    Plugin_AutoShareRecord_V2_2.ShareTeams(inputParameter.ToEntityReference(), entity9.ToEntityReference(), false);

                                if (entity9.Attributes["name"].ToString().Contains(strSM))
                                    Plugin_AutoShareRecord_V2_2.ShareTeams(inputParameter.ToEntityReference(), entity9.ToEntityReference(), true);
                            }
                        }
                    }
                }

                Plugin_AutoShareRecord_V2_1.Run_Update(service, traceService, inputParameter, service1);
            }
            Plugin_AutoShareRecord_V2_2.Run_ProcessShareTeam(service, traceService, inputParameter, service1);


        }


        private void Role_SharePrivileges(
          string targetEntityName,
          Guid targetRecordID,
          Guid teamID,
          bool read_Access,
          bool write_Access,
          bool append_Access,
          IOrganizationService orgService,
          bool FinTeam)
        {
            try
            {
                EntityReference entityReference1 = new EntityReference(targetEntityName, targetRecordID);
                EntityReference entityReference2 = new EntityReference("team", teamID);
                //AccessRights accessRights = (AccessRights)0;
                //if (read_Access)
                //    accessRights = (AccessRights)1;
                //if (write_Access)
                //    accessRights = accessRights != 0 ? (AccessRights)(accessRights | 2) : (AccessRights)2;
                //if (append_Access)
                //    accessRights = accessRights != 0 ? (AccessRights)(accessRights | 16 | 4) : (AccessRights)20;
                //if (FinTeam)
                //    accessRights = accessRights != 0 ? (AccessRights)(accessRights | 524288) : (AccessRights)524288;



                int accessRights = 0; // Khởi tạo với giá trị 0
                if (read_Access)
                    accessRights = 1; // Gán quyền đọc
                if (write_Access)
                    accessRights = accessRights != 0 ? accessRights | 2 : 2; // Thêm quyền ghi
                if (append_Access)
                    accessRights = accessRights != 0 ? accessRights | (16 | 4) : 20; // Thêm quyền thêm
                if (FinTeam)
                    accessRights = accessRights != 0 ? accessRights | 524288 : 524288; // Thêm quyền FinTeam

                AccessRights finalAccessRights = (AccessRights)accessRights; // Chuyển đổi lại sang AccessRights nếu cần
                GrantAccessRequest request = new GrantAccessRequest()
                {
                    PrincipalAccess = new PrincipalAccess()
                    {
                        AccessMask = finalAccessRights,
                        Principal = entityReference2
                    },
                    Target = entityReference1
                };
                orgService.Execute((OrganizationRequest)request);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while applying Sharing rules for the record." + ex.Message);
            }
        }

        private void Role_ModifyAccess(
          string targetEntityName,
          Guid targetRecordID,
          Guid teamID,
          IOrganizationService orgService)
        {
            try
            {
                EntityReference entityReference1 = new EntityReference(targetEntityName, targetRecordID);
                EntityReference entityReference2 = new EntityReference("team", teamID);
                AccessRights accessRights = (AccessRights)65536;
                ModifyAccessRequest request = new ModifyAccessRequest()
                {
                    PrincipalAccess = new PrincipalAccess()
                    {
                        AccessMask = accessRights,
                        Principal = entityReference2
                    },
                    Target = entityReference1
                };
                orgService.Execute((OrganizationRequest)request);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured in Modifying access." + ex.Message);
            }
        }

        private void Role_RevokeAccess(
          string targetEntityName,
          Guid targetRecordID,
          Guid teamID,
          IOrganizationService orgService)
        {
            try
            {
                EntityReference entityReference1 = new EntityReference(targetEntityName, targetRecordID);
                EntityReference entityReference2 = new EntityReference("team", teamID);
                RevokeAccessRequest request = new RevokeAccessRequest()
                {
                    Revokee = entityReference2,
                    Target = entityReference1
                };
                orgService.Execute((OrganizationRequest)request);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured in Revoking access." + ex.Message);
            }
        }

        private EntityCollection Get_TeamAccess(
          string ProjectCode,
          bool SaleTeam,
          bool CcrTeam,
          bool FinTeam,
          bool SaleMgtTeam)
        {
            string str = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >\r\n                  <entity name='team' >\r\n                    <attribute name='name' />\r\n                    <attribute name='teamid' />\r\n                    <filter type='and' >\r\n                      <condition attribute='name' operator='like' value='%{0}-%' />";
            if (SaleTeam | CcrTeam | FinTeam | SaleMgtTeam)
                str += "<filter type='or' >";
            if (SaleTeam)
                str = str + "<condition attribute='name' operator='like' value='%" + this.strSM + "%' />";
            if (CcrTeam)
                str = str + "<condition attribute='name' operator='like' value='%" + this.strCCR + "%' />";
            if (FinTeam)
                str = str + "<condition attribute='name' operator='like' value='%" + this.strFIN + "%' />";
            if (SaleMgtTeam)
                str = str + "<condition attribute='name' operator='like' value='%" + this.strSMGT + "%' />";
            if (SaleTeam | CcrTeam | FinTeam | SaleMgtTeam)
                str += "</filter>";
            return this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format(str + "</filter>\r\n                  </entity>\r\n            </fetch>", (object)ProjectCode)));
        }
    }
}