using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_UpdateLandValue_Detail
{
    public class Action_UpdateLandValue_Detail : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        string enHD_name = "";
        string enIntalments_fieldNameHD = "";
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            //get entity
            string enDetailid = context.InputParameters["id"].ToString();
            en = service.Retrieve("bsd_landvalue", new Guid(enDetailid), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
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
                Entity entity3 = en;
                this.service.Update(new Entity(entity3.LogicalName, entity3.Id)
                {
                    ["statecode"] = (object)new OptionSetValue(0),
                    ["statuscode"] = (object)new OptionSetValue(100000002)
                });
                Entity entity4 = this.service.Retrieve(entity3.LogicalName, entity3.Id, new ColumnSet(true));
                if (((OptionSetValue)entity4["bsd_type"]).Value == 100000000)
                {
                    Decimal num1 = entity4.Contains("bsd_listedpricenew") ? ((Money)entity4["bsd_listedpricenew"]).Value : 0M;
                    Decimal num2 = entity4.Contains("bsd_discountnew") ? ((Money)entity4["bsd_discountnew"]).Value : 0M;
                    Decimal num3 = entity4.Contains("bsd_handoverconditionamountnew") ? ((Money)entity4["bsd_handoverconditionamountnew"]).Value : 0M;
                    Decimal num4 = entity4.Contains("bsd_netsellingpricenew") ? ((Money)entity4["bsd_netsellingpricenew"]).Value : 0M;
                    Decimal num5 = entity4.Contains("bsd_landvaluedeductionnew") ? ((Money)entity4["bsd_landvaluedeductionnew"]).Value : 0M;
                    Decimal num6 = entity4.Contains("bsd_totalvattaxnew") ? ((Money)entity4["bsd_totalvattaxnew"]).Value : 0M;
                    Decimal num7 = entity4.Contains("bsd_maintenancefeenew") ? ((Money)entity4["bsd_maintenancefeenew"]).Value : 0M;
                    Decimal num8 = entity4.Contains("bsd_totalamountnew") ? ((Money)entity4["bsd_totalamountnew"]).Value : 0M;
                    EntityReference entityReference1 = entity4.Contains("bsd_optionentry") ? (EntityReference)entity4["bsd_optionentry"] : (EntityReference)null;
                    Entity entity5 = this.service.Retrieve(entityReference1.LogicalName, entityReference1.Id, new ColumnSet(new string[5]
                    {
                "salesorderid",
                "bsd_landvaluededuction",
                "totaltax",
                "bsd_freightamount",
                "totalamount"
                    }));
                    EntityReference entityReference2 = entity4.Contains("bsd_units") ? (EntityReference)entity4["bsd_units"] : (EntityReference)null;
                    this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true));
                    //#update
                    this.service.Update(new Entity(entity5.LogicalName, entity5.Id)
                    {
                        ["bsd_landvaluededuction"] = (object)new Money(num5),
                        ["totaltax"] = (object)new Money(num6),
                        ["bsd_freightamount"] = (object)new Money(num7),
                        ["totalamount"] = (object)new Money(num8)
                    });
                    //#update
                    this.service.Update(new Entity("salesorderdetail", this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("\r\n                        <fetch>\r\n                          <entity name='salesorderdetail'>\r\n                            <all-attributes />\r\n                            <filter>\r\n                              <condition attribute='salesorderid' operator='eq' value='{0}'/>\r\n                            </filter>\r\n                          </entity>\r\n                        </fetch>", (object)entity5.Id))).Entities[0].Id)
                    {
                        ["tax"] = (object)new Money(num6),
                        ["extendedamount"] = (object)new Money(num1 + num6)
                    });
                    if (entity4.Contains("bsd_installment"))
                    {
                        Decimal num9 = entity4.Contains("bsd_amountofthisphase") ? ((Money)entity4["bsd_amountofthisphase"]).Value : 0M;
                        if (num9 > 0M)
                        {
                            EntityReference entityReference3 = entity4.Contains("bsd_installment") ? (EntityReference)entity4["bsd_installment"] : (EntityReference)null;
                            Entity entity6 = this.service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(true));
                            entity6["bsd_amountofthisphase"] = (object)num9;
                            this.service.Update(entity6);
                        }
                    }
                }
                if (entity4.Contains("bsd_units"))
                {
                    EntityReference entityReference = entity4.Contains("bsd_units") ? (EntityReference)entity4["bsd_units"] : (EntityReference)null;
                    Entity entity7 = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(true));
                    //#update
                    this.service.Update(new Entity(entity7.LogicalName, entity7.Id)
                    {
                        ["bsd_landvalueofunit"] = entity4["bsd_landvaluenew"]
                    });
                }
            }
            catch (Exception ex)
            {
                HandleError(item, ex.Message);
            }
        }
       
       
        public void HandleError(Entity item, string error)
        {
            var enMasterRef = (EntityReference)item["bsd_updatelandvalue"];
            var enMaster = new Entity("bsd_updatelandvalue", enMasterRef.Id);
            enMaster["bsd_error"] = true;
            enMaster["bsd_errordetail"] = error;
            enMaster["bsd_processing_pa"] = false;
            enMaster["statuscode"] = new OptionSetValue(1);
            service.Update(enMaster);
        }
        public bool CheckConditionRun(Entity item)
        {
            var enMasterRef = (EntityReference)item["bsd_updatelandvalue"];
            var enMaster = service.Retrieve("bsd_updatelandvalue", enMasterRef.Id, new ColumnSet(true));
            tracingService.Trace($"masterid {enMaster.Id}");
            if ((bool)enMaster["bsd_error"] == true && (bool)enMaster["bsd_processing_pa"] == false)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}
