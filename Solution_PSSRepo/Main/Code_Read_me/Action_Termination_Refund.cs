// Decompiled with JetBrains decompiler
// Type: Action_Termination_Refund.Action_Termination_Refund
// Assembly: Action_Termination_Refund, Version=1.0.0.0, Culture=neutral, PublicKeyToken=45c7fa885522c9ed
// MVID: E2F03ACA-CBA9-4B46-9754-F01BFD638938
// Assembly location: C:\Users\ngoct\Downloads\Action_Termination_Refund_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;


namespace Action_Termination_Refund
{
    public class Action_Termination_Refund : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        // Hàm thực thi chính của Plugin
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            // Lấy context thực thi plugin
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Lấy tham số đầu vào là EntityReference của bản ghi termination
            EntityReference inputParameter = (EntityReference)service1.InputParameters["Target"];
            // Kiểm tra entity có phải là bsd_termination không
            if (!(inputParameter.LogicalName == "bsd_termination"))
                return;
            // Lấy factory và service để thao tác dữ liệu CRM
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service1.UserId));
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Lấy thông tin termination
            Entity entity1 = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[13]
            {
            "bsd_name",
            "statuscode",
            "bsd_optionentry",
            "bsd_units",
            "bsd_followuplist",
            "bsd_refundamount",
            "bsd_receivedamount",
            "bsd_totalamountpaid",
            "bsd_totalforfeitureamount",
            "bsd_source",
            "bsd_quotationreservation",
            "bsd_resell",
            "bsd_phaselaunch"
            }));
            // Kiểm tra termination có phải là resell không
            bool flag = entity1.Contains("bsd_resell") && (bool)entity1["bsd_resell"];
            // Lấy thông tin phase launch nếu có
            EntityReference entityReference = entity1.Contains("bsd_phaselaunch") ? (EntityReference)entity1["bsd_phaselaunch"] : (EntityReference)null;
            // Kiểm tra đã chọn nguồn chưa
            if (!entity1.Contains("bsd_source"))
                throw new InvalidPluginExecutionException("Please choose Source!");
            // Xử lý theo từng loại nguồn termination
            switch (((OptionSetValue)entity1["bsd_source"]).Value)
            {
                case 100000000: // Nguồn là Reservation
                    if (((OptionSetValue)entity1["statuscode"]).Value == 1)
                    {
                        // Kiểm tra thông tin bắt buộc
                        if (!entity1.Contains("bsd_units"))
                            throw new InvalidPluginExecutionException("Termiation does not contain Unit information. Please check again!");
                        if (!entity1.Contains("bsd_quotationreservation"))
                            throw new InvalidPluginExecutionException("Termiation does not contain Quotation Reservation information. Please check again!");
                        // Lấy thông tin Unit
                        Entity entity2 = this.service.Retrieve(((EntityReference)entity1["bsd_units"]).LogicalName, ((EntityReference)entity1["bsd_units"]).Id, new ColumnSet(new string[7]
                        {
                  "statuscode",
                  "name",
                  "bsd_phaseslaunchid",
                  "bsd_terminated",
                  "productnumber",
                  "bsd_projectcode",
                  "bsd_terminatecount"
                        }));
                        // Lấy thông tin Reservation
                        Entity entity3 = this.service.Retrieve(((EntityReference)entity1["bsd_quotationreservation"]).LogicalName, ((EntityReference)entity1["bsd_quotationreservation"]).Id, new ColumnSet(new string[4]
                        {
                  "statuscode",
                  "name",
                  "customerid",
                  "bsd_totalamountpaid"
                        }));
                        // Cập nhật trạng thái termination
                        this.service.Update(new Entity(inputParameter.LogicalName)
                        {
                            Id = inputParameter.Id,
                            ["statuscode"] = (object)new OptionSetValue(100000001)
                        });
                        // Lấy thông tin khách hàng
                        new Entity(((EntityReference)entity3["customerid"]).LogicalName).Id = ((EntityReference)entity3["customerid"]).Id;
                        // Tạo bản ghi Refund
                        Entity entity4 = new Entity("bsd_refund");
                        entity4.Id = Guid.NewGuid();
                        if (((EntityReference)entity3["customerid"]).LogicalName == "contact")
                        {
                            // Nếu là khách hàng cá nhân
                            Entity entity5 = this.service.Retrieve(((EntityReference)entity3["customerid"]).LogicalName, ((EntityReference)entity3["customerid"]).Id, new ColumnSet(new string[2]
                            {
                    "bsd_fullname",
                    "fullname"
                            }));
                            entity4["bsd_name"] = (object)("Refund-" + (string)entity3["name"] + "-" + (string)entity2["productnumber"] + "-" + (entity5.Contains("bsd_fullname") ? (string)entity5["bsd_fullname"] : (string)entity5["fullname"]));
                        }
                        else
                        {
                            // Nếu là khách hàng tổ chức
                            Entity entity6 = this.service.Retrieve(((EntityReference)entity3["customerid"]).LogicalName, ((EntityReference)entity3["customerid"]).Id, new ColumnSet(new string[2]
                            {
                    "bsd_name",
                    "name"
                            }));
                            entity4["bsd_name"] = (object)("Refund-" + (string)entity3["name"] + "-" + (string)entity2["productnumber"] + "-" + (entity6.Contains("bsd_name") ? (string)entity6["bsd_name"] : (string)entity6["name"]));
                        }
                        // Gán các trường thông tin cho Refund
                        entity4["bsd_unitno"] = entity1["bsd_units"];
                        entity4["bsd_reservation"] = entity1["bsd_quotationreservation"];
                        entity4["statuscode"] = (object)new OptionSetValue(1);
                        entity4["bsd_totalamountpaid"] = entity3["bsd_totalamountpaid"];
                        entity4["bsd_project"] = entity2["bsd_projectcode"];
                        if (entity1.Contains("bsd_followuplist"))
                            entity4["bsd_followuplist"] = entity1["bsd_followuplist"];
                        entity4["bsd_refundtype"] = (object)new OptionSetValue(100000001);
                        entity4["bsd_customer"] = entity3["customerid"];
                        entity4["bsd_termination"] = (object)new EntityReference("bsd_termination", entity1.Id);
                        entity4["bsd_refundamount"] = entity1["bsd_refundamount"];
                        entity4["bsd_refundableamount"] = entity1["bsd_receivedamount"];
                        // Kiểm tra trạng thái phase launch
                        if (entityReference != null && this.find_phase(this.service, entityReference).Entities.Count > 0)
                            throw new InvalidPluginExecutionException("Status of the Phase launch is not Launched, please check again.");
                        // Tạo bản ghi Refund
                        this.service.Create(entity4);
                        // Kiểm tra trạng thái Reservation
                        Entity entity7 = ((OptionSetValue)entity3["statuscode"]).Value == 3 ? new Entity(entity3.LogicalName) : throw new InvalidPluginExecutionException("Status of Reservation Invalid. Please check again!");
                        entity7.Id = entity3.Id;
                        // Đóng Reservation
                        CloseQuoteRequest request = new CloseQuoteRequest();
                        Entity entity8 = new Entity("quoteclose");
                        entity8.Attributes.Add("quoteid", (object)new EntityReference("quote", new Guid(entity7.Id.ToString())));
                        entity8.Attributes.Add("subject", (object)"Close the Reservation!");
                        request.QuoteClose = entity8;
                        request.RequestName = "CloseQuote";
                        request.Status = new OptionSetValue(100000001);
                        this.service.Execute((OrganizationRequest)request);
                        // Cập nhật trạng thái Unit
                        this.service.Update(new Entity(entity2.LogicalName)
                        {
                            Id = entity2.Id,
                            ["statuscode"] = (object)new OptionSetValue(1)
                        });
                        break;
                    }
                    break;
                case 100000001: // Nguồn là Option Entry
                    if (((OptionSetValue)entity1["statuscode"]).Value == 100000001)
                        throw new InvalidPluginExecutionException("Termination has been complete!");
                    // Kiểm tra thông tin bắt buộc
                    if (!entity1.Contains("bsd_optionentry"))
                        throw new InvalidPluginExecutionException("Termiation does not contain Option Entry information. Please check again!");
                    if (!entity1.Contains("bsd_units"))
                        throw new InvalidPluginExecutionException("Termiation does not contain Unit information. Please check again!");
                    if (!entity1.Contains("bsd_followuplist"))
                        throw new InvalidPluginExecutionException("Termiation does not contain Unit information. Please check again!");
                    if (!entity1.Contains("bsd_totalamountpaid"))
                        throw new InvalidPluginExecutionException("Termiation does not contain Total Amount Paid. Please check again!");
                    if (!entity1.Contains("bsd_totalforfeitureamount"))
                        throw new InvalidPluginExecutionException("Termiation does not contain Total Forfeiture Amount. Please check again!");
                    if (((OptionSetValue)entity1["statuscode"]).Value == 1)
                    {
                        // Cập nhật trạng thái termination
                        this.service.Update(new Entity(entity1.LogicalName)
                        {
                            Id = entity1.Id,
                            ["statuscode"] = (object)new OptionSetValue(100000001)
                        });
                        // Lấy thông tin Option Entry
                        Entity entity9 = this.service.Retrieve(((EntityReference)entity1["bsd_optionentry"]).LogicalName, ((EntityReference)entity1["bsd_optionentry"]).Id, new ColumnSet(new string[5]
                        {
                  "statuscode",
                  "name",
                  "customerid",
                  "bsd_totalamountpaid",
                  "quoteid"
                        }));
                        // Kiểm tra Option Entry có khách hàng chưa
                        if (!entity9.Contains("customerid"))
                            throw new InvalidPluginExecutionException("Option Entry" + (string)entity9["name"] + " does not contain Purchaser information!");
                        // Cập nhật trạng thái Option Entry
                        this.service.Update(new Entity(entity9.LogicalName)
                        {
                            Id = entity9.Id,
                            ["statuscode"] = (object)new OptionSetValue(100000006)
                        });
                        // Lấy thông tin Quote
                        Entity entity10 = this.service.Retrieve(((EntityReference)entity9["quoteid"]).LogicalName, ((EntityReference)entity9["quoteid"]).Id, new ColumnSet(true));
                        // Cập nhật trạng thái Quote
                        this.service.Update(new Entity(entity10.LogicalName, entity10.Id)
                        {
                            ["statecode"] = (object)new OptionSetValue(3),
                            ["statuscode"] = (object)new OptionSetValue(100000001)
                        });
                        // Lấy thông tin Unit
                        Entity entity11 = this.service.Retrieve(((EntityReference)entity1["bsd_units"]).LogicalName, ((EntityReference)entity1["bsd_units"]).Id, new ColumnSet(new string[7]
                        {
                  "statuscode",
                  "name",
                  "bsd_phaseslaunchid",
                  "bsd_terminated",
                  "productnumber",
                  "bsd_projectcode",
                  "bsd_terminatecount"
                        }));
                        // Cập nhật trạng thái Unit
                        Entity entity12 = new Entity(entity11.LogicalName)
                        {
                            Id = entity11.Id,
                            ["statuscode"] = (object)new OptionSetValue(1),
                            ["bsd_phaseslaunchid"] = (object)null,
                            ["bsd_terminated"] = (object)true
                        };
                        // Tăng số lần terminate
                        entity12["bsd_terminatecount"] = entity12.Contains("bsd_terminatecount") ? (object)((int)entity12["bsd_terminatecount"] + 1) : (object)1;
                        this.service.Update(entity12);
                        // Lấy thông tin khách hàng
                        new Entity(((EntityReference)entity9["customerid"]).LogicalName).Id = ((EntityReference)entity9["customerid"]).Id;
                        // Tạo bản ghi Refund
                        Entity entity13 = new Entity("bsd_refund");
                        if (((EntityReference)entity9["customerid"]).LogicalName == "contact")
                        {
                            // Nếu là khách hàng cá nhân
                            Entity entity14 = this.service.Retrieve(((EntityReference)entity9["customerid"]).LogicalName, ((EntityReference)entity9["customerid"]).Id, new ColumnSet(new string[2]
                            {
                    "bsd_fullname",
                    "fullname"
                            }));
                            entity13["bsd_name"] = (object)("Refund-" + (string)entity9["name"] + "-" + (string)entity11["productnumber"] + "-" + (entity14.Contains("bsd_fullname") ? (string)entity14["bsd_fullname"] : (string)entity14["fullname"]));
                        }
                        else
                        {
                            // Nếu là khách hàng tổ chức
                            Entity entity15 = this.service.Retrieve(((EntityReference)entity9["customerid"]).LogicalName, ((EntityReference)entity9["customerid"]).Id, new ColumnSet(new string[2]
                            {
                    "bsd_name",
                    "name"
                            }));
                            entity13["bsd_name"] = (object)("Refund-" + (string)entity9["name"] + "-" + (string)entity11["productnumber"] + "-" + (entity15.Contains("bsd_name") ? (string)entity15["bsd_name"] : (string)entity15["name"]));
                        }
                        // Gán các trường thông tin cho Refund
                        entity13["bsd_unitno"] = entity1["bsd_units"];
                        entity13["bsd_optionentry"] = entity1["bsd_optionentry"];
                        entity13["statuscode"] = (object)new OptionSetValue(1);
                        entity13["bsd_totalamountpaid"] = entity1["bsd_totalamountpaid"];
                        entity13["bsd_termination"] = (object)new EntityReference("bsd_termination", entity1.Id);
                        // Kiểm tra trạng thái phase launch
                        if (entityReference != null && this.find_phase(this.service, entityReference).Entities.Count > 0)
                            throw new InvalidPluginExecutionException("Status of the Phase launch is not Launched, please check again.");
                        entity13["bsd_project"] = entity11["bsd_projectcode"];
                        entity13["bsd_followuplist"] = entity1["bsd_followuplist"];
                        entity13["bsd_refundtype"] = (object)new OptionSetValue(100000001);
                        entity13["bsd_customer"] = entity9["customerid"];
                        entity13["bsd_refundamount"] = entity1["bsd_refundamount"];
                        entity13["bsd_refundableamount"] = entity1["bsd_receivedamount"];
                        // Tạo bản ghi Refund
                        this.service.Create(entity13);
                        break;
                    }
                    break;
            }
            // Xử lý cập nhật lại giá và trạng thái cho Unit khi là Resell và có phase launch
            if (!flag || entityReference == null)
                return;
            EntityReference unit = entity1.Contains("bsd_units") ? (EntityReference)entity1["bsd_units"] : (EntityReference)null;
            EntityCollection entityCollection1 = this.fetch_phase(this.service, entityReference);
            if (entityCollection1.Entities.Count > 0)
            {
                foreach (Entity entity16 in (Collection<Entity>)entityCollection1.Entities)
                {
                    EntityReference pha = (EntityReference)entity16["bsd_pricelistid"];
                    EntityCollection entityCollection2 = this.checkunit_phase(this.service, pha, unit);
                    if (entityCollection2.Entities.Count > 0)
                    {
                        // Nếu đã có giá cho unit trong phase
                        Entity entity17 = this.service.Retrieve(((EntityReference)entity1["bsd_units"]).LogicalName, ((EntityReference)entity1["bsd_units"]).Id, new ColumnSet(new string[7]
                        {
                  "statuscode",
                  "name",
                  "bsd_phaseslaunchid",
                  "bsd_terminated",
                  "bsd_blocknumber",
                  "bsd_floor",
                  "bsd_terminatecount"
                        }));
                        foreach (Entity entity18 in (Collection<Entity>)entityCollection2.Entities)
                        {
                            Entity entity19 = new Entity(entity17.LogicalName);
                            entity19.Id = entity17.Id;
                            entity19["statuscode"] = (object)new OptionSetValue(100000000);
                            entity19["bsd_phaseslaunchid"] = (object)entityReference;
                            Decimal num = ((Money)entity18["amount"]).Value;
                            entity19["price"] = (object)new Money(num);
                            this.service.Update(entity19);
                        }
                    }
                    else
                    {
                        // Nếu chưa có giá thì tạo mới
                        EntityCollection entityCollection3 = this.price_list_new(this.service, unit);
                        if (entityCollection3.Entities.Count > 0)
                        {
                            Entity entity20 = this.service.Retrieve(((EntityReference)entity1["bsd_units"]).LogicalName, ((EntityReference)entity1["bsd_units"]).Id, new ColumnSet(new string[7]
                            {
                    "statuscode",
                    "name",
                    "bsd_phaseslaunchid",
                    "bsd_terminated",
                    "bsd_blocknumber",
                    "bsd_floor",
                    "bsd_terminatecount"
                            }));
                            foreach (Entity entity21 in (Collection<Entity>)entityCollection3.Entities)
                            {
                                // Tạo mới productpricelevel
                                Entity entity22 = new Entity("productpricelevel");
                                entity22["pricelevelid"] = (object)pha;
                                entity22["productid"] = (object)unit;
                                entity22["uomid"] = (object)(EntityReference)entity21["uomid"];
                                Decimal num1 = ((Money)entity21["amount"]).Value;
                                entity22["amount"] = (object)new Money(num1);
                                entity22["pricingmethodcode"] = (object)new OptionSetValue(((OptionSetValue)entity21["pricingmethodcode"]).Value);
                                entity22["transactioncurrencyid"] = (object)(EntityReference)entity21["transactioncurrencyid"];
                                entity22["quantitysellingcode"] = (object)new OptionSetValue(((OptionSetValue)entity21["quantitysellingcode"]).Value);
                                this.service.Create(entity22);
                                // Cập nhật lại giá cho Unit
                                Entity entity23 = new Entity(entity20.LogicalName);
                                entity23.Id = entity20.Id;
                                entity23["statuscode"] = (object)new OptionSetValue(100000000);
                                entity23["bsd_phaseslaunchid"] = (object)entityReference;
                                Decimal num2 = ((Money)entity21["amount"]).Value;
                                entity23["price"] = (object)new Money(num2);
                                this.service.Update(entity23);
                            }
                        }
                    }
                }
            }
        }

        // Hàm lấy phase launch theo id
        private EntityCollection fetch_phase(IOrganizationService crmservices, EntityReference pha)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n  <entity name='bsd_phaseslaunch'>\r\n    <attribute name='bsd_name' />\r\n    <attribute name='createdon' />\r\n    <attribute name='bsd_pricelistid' />\r\n    <attribute name='bsd_salesagentcompany' />\r\n    <attribute name='bsd_phaseslaunchid' />\r\n    <order attribute='createdon' descending='true' />\r\n    <filter type='and'>\r\n      <condition attribute='bsd_phaseslaunchid' operator='eq' value='{0}' />\r\n    </filter>\r\n  </entity>\r\n</fetch>", (object)pha.Id);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        // Hàm kiểm tra unit đã có trong phase chưa
        private EntityCollection checkunit_phase(
          IOrganizationService crmservices,
          EntityReference pha,
          EntityReference unit)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>\r\n  <entity name='productpricelevel'>\r\n    <attribute name='amount' />\r\n    <attribute name='pricingmethodcode' />\r\n    <filter type='and'>\r\n        <condition attribute='productid' operator='eq' uitype='product' value='{1}'/>\r\n      </filter>\r\n    <link-entity name='pricelevel' from='pricelevelid' to='pricelevelid' alias='ab'>\r\n      <filter type='and'>\r\n      <condition attribute='pricelevelid' operator='eq' value='{0}'/>\r\n    </filter>\r\n    </link-entity>\r\n  </entity>\r\n</fetch>", (object)pha.Id, (object)unit.Id);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        // Hàm lấy giá mới nhất của unit
        private EntityCollection price_list_new(IOrganizationService crmservices, EntityReference unit)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' top='1' distinct='true'>\r\n  <entity name='productpricelevel'>\r\n    <attribute name='amount' />\r\n  <attribute name='uomid' />\r\n  <attribute name='quantitysellingcode' />\r\n  <attribute name='transactioncurrencyid' />\r\n    <attribute name='pricingmethodcode'/>\r\n    <order attribute='createdon' descending='true' />\r\n    <filter type='and'>\r\n        <condition attribute='productid' operator='eq' uitype='product' value='{0}' />\r\n      </filter>    \r\n  </entity>\r\n</fetch>", (object)unit.Id);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        // Hàm kiểm tra trạng thái phase launch
        private EntityCollection find_phase(IOrganizationService service, EntityReference phase)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='bsd_phaseslaunch'>\r\n                    <attribute name='bsd_name'/>\r\n                    <attribute name='bsd_phaseslaunchid'/>\r\n                    <filter type='and'>\r\n                   <condition attribute='bsd_phaseslaunchid' operator='eq' value='{0}'/>\r\n                    <condition attribute='statuscode' operator='ne' value='100000000'/>\r\n                    </filter>\r\n                  </entity>\r\n                </fetch>", (object)phase.Id);
            return service.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }
    }
}
