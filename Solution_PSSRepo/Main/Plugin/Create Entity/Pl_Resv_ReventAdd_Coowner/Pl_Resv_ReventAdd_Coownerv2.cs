// Decompiled with JetBrains decompiler
// Type: Pl_Resv_ReventAdd_Coowner.Pl_Resv_ReventAdd_Coowner
// Assembly: Pl_Resv_ReventAdd_Coowner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=2004ca381ff4b6b9
// MVID: DF9E8D3A-7493-4F2D-97D6-F807B23361A3
// Assembly location: C:\Users\ngoct\Downloads\Pl_Resv_ReventAdd_Coowner_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;


namespace Pl_Resv_ReventAdd_Coownerv2
{
    public class Pl_Resv_ReventAdd_Coownerv2 : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service1.InitiatingUserId));
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            EntityReference entityReference1 = new EntityReference("systemuser", service1.InitiatingUserId);
            Entity entity1 = this.service.Retrieve(entityReference1.LogicalName, entityReference1.Id, new ColumnSet(new string[3]
            {
        "systemuserid",
        "firstname",
        "lastname"
            }));
            string str1 = "<fetch>\r\n                          <entity name='role'>\r\n                            <attribute name = 'roleid'/>\r\n                            <attribute name='name'/>\r\n                            <link-entity name = 'systemuserroles' from='roleid' to='roleid' intersect='true'>\r\n                              <filter>\r\n                                <condition attribute = 'systemuserid' operator='eq' value= '" + entity1.Id.ToString() + "'/>\r\n                                </filter>\r\n                              </link-entity>\r\n                            </entity>\r\n                          </fetch>";
            service2.Trace(str1);
            EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)new FetchExpression(str1));
            string format = " ";
            foreach (Entity entity2 in (Collection<Entity>)entityCollection.Entities)
            {
                string str2 = (string)entity2["name"];
                format += str2;
            }
            string str3 = "CLVN_CCR Manager";
            string strrole2 = "CLVN_CCR Senior Staff";
            service2.Trace("Name: " + entity1.Id.ToString());
            service2.Trace(format);
            if (service1.MessageName == "Create")
            {
                Entity inputParameter = (Entity)service1.InputParameters["Target"];
                if (inputParameter.LogicalName == "bsd_coowner")
                {
                    if (inputParameter.Contains("bsd_subsale") && !inputParameter.Contains("bsd_reservation") && !inputParameter.Contains("bsd_optionentry"))
                        this.service.Update(new Entity(inputParameter.LogicalName)
                        {
                            Id = inputParameter.Id,
                            ["bsd_current"] = (object)false
                        });
                    EntityReference entityReference2 = inputParameter.Contains("bsd_optionentry") ? (EntityReference)inputParameter["bsd_optionentry"] : (EntityReference)null;
                    if (entityReference2 != null)
                    {
                        Entity entity3 = this.service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true));
                        if ((!((entity3.Contains("bsd_signedcontractdate") ? (DateTime)entity3["bsd_signedcontractdate"] : new DateTime(0L)) == new DateTime(0L)) || (!format.Contains(str3)&& !format.Contains(strrole2))) && !inputParameter.Contains("bsd_subsale"))
                            throw new InvalidPluginExecutionException("Can not add new Co-owner for Option Entry " + (string)entity3["name"]);
                        if (inputParameter.Contains("bsd_optionentry") && (format.Contains(str3)|| format.Contains(strrole2)))
                        {
                            Entity entity4 = this.service.Retrieve(((EntityReference)inputParameter["bsd_optionentry"]).LogicalName, ((EntityReference)inputParameter["bsd_optionentry"]).Id, new ColumnSet(new string[4]
                            {
                "name",
                "statuscode",
                "bsd_f_lockaddfieldfromresv",
                "quoteid"
                            }));
                            if (!entity4.Contains("quoteid"))
                                throw new InvalidPluginExecutionException("Option Entry " + (string)entity4["name"] + " does not contain Quotation Reservation information. Pleae check again!");
                        }
                    }
                    service2.Trace("bsd_reservation Contains: " + inputParameter.Contains("bsd_reservation").ToString());
                    if (inputParameter.Contains("bsd_reservation") && (format.Contains(str3)|| format.Contains(strrole2)))
                    {
                        service2.Trace("aaaaaaaaaaaaaaaaaaaaaaaa");
                        try
                        {
                            service2.Trace("bsd_reservation id: " + inputParameter["bsd_reservation"].ToString());
                            Entity entity5 = this.service.Retrieve(((EntityReference)inputParameter["bsd_reservation"]).LogicalName, ((EntityReference)inputParameter["bsd_reservation"]).Id, new ColumnSet(new string[2]
                            {
                "name",
                "statuscode"
                            }));
                            service2.Trace("en_Quo " + entity5.Id.ToString());
                            service2.Trace("statuscode " + ((OptionSetValue)entity5["statuscode"]).Value.ToString());
                            if (entity5.Contains("statuscode") && ((OptionSetValue)entity5["statuscode"]).Value != 100000007)
                                throw new InvalidPluginExecutionException("Cannot add new Co-owner for Quotation Reservation " + (string)entity5["name"] + " without quotation status!");
                        }
                        catch (Exception ex)
                        {
                            service2.Trace("New sub-sale");
                        }
                    }
                }
            }
            if (!(service1.MessageName == "Delete"))
                return;
            EntityReference inputParameter1 = (EntityReference)service1.InputParameters["Target"];
            if (inputParameter1.LogicalName == "bsd_coowner")
            {
                service2.Trace("1");
                Entity entity6 = this.service.Retrieve(inputParameter1.LogicalName, inputParameter1.Id, new ColumnSet(new string[4]
                {
          "statuscode",
          "bsd_reservation",
          "bsd_optionentry",
          "bsd_subsale"
                }));
                EntityReference entityReference3 = entity6.Contains("bsd_subsale") ? (EntityReference)entity6["bsd_subsale"] : (EntityReference)null;
                if (entityReference3 != null)
                {
                    Entity entity7 = this.service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(true));
                    if ((entity7.Contains("statuscode") ? ((OptionSetValue)entity7["statuscode"]).Value : 0) == 100000001)
                        throw new InvalidPluginExecutionException("Cannot delete Co-owner because Sub Sale is Approved!");
                }
                service2.Trace("2");
                EntityReference entityReference4 = entity6.Contains("bsd_optionentry") ? (EntityReference)entity6["bsd_optionentry"] : (EntityReference)null;
                EntityReference entityReference5 = entity6.Contains("bsd_reservation") ? (EntityReference)entity6["bsd_reservation"] : (EntityReference)null;
                if (entityReference4 != null)
                {
                    Entity entity8 = this.service.Retrieve(entityReference4.LogicalName, entityReference4.Id, new ColumnSet(new string[4]
                    {
            "name",
            "statuscode",
            "bsd_signedcontractdate",
            "bsd_f_lockaddfieldfromresv"
                    }));
                    service2.Trace("3");
                    if (entity8.Contains("bsd_signedcontractdate"))
                        throw new InvalidPluginExecutionException("Cannot delete Co-owner of Option Entry " + (string)entity8["name"] + ".");
                    if ((!format.Contains(str3)&& !format.Contains(strrole2)))
                        throw new InvalidPluginExecutionException("Cannot delete Co-owner of Option Entry " + (string)entity8["name"] + ".");
                }
                service2.Trace("4");
                if (entityReference5 != null && !format.Contains(str3))
                {
                    Entity entity9 = this.service.Retrieve(entityReference5.LogicalName, entityReference5.Id, new ColumnSet(new string[2]
                    {
            "name",
            "statuscode"
                    }));
                    if (entity9.Contains("statuscode") && ((OptionSetValue)entity9["statuscode"]).Value != 100000007)
                        throw new InvalidPluginExecutionException("Cannot delete Co-owner of Quotation Reservation" + (string)entity9["name"] + " without quotation status");
                }
                service2.Trace("5");
            }
        }

        private EntityCollection get_ec_resv_coowner(IOrganizationService crmservices, Guid quoteid)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='bsd_coowner'>\r\n                <attribute name='bsd_coownerid' />\r\n                <attribute name='bsd_name' />\r\n                <attribute name='createdon' />\r\n                <attribute name='statuscode' />\r\n                <attribute name='bsd_reservation' />\r\n                <attribute name='bsd_optionentry' />\r\n                <order attribute='bsd_name' descending='false' />\r\n                <filter type='and'>\r\n                  <condition attribute='bsd_reservation' operator='eq' value='{0}' />\r\n                </filter>\r\n              </entity>\r\n                </fetch>", (object)quoteid);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private EntityCollection getCo_owner_subsale(IOrganizationService crmservices, Guid id)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='bsd_coowner'>\r\n                    <attribute name='bsd_coownerid' />\r\n                    <attribute name='bsd_name' />\r\n                    <filter type='and'>\r\n                      <condition attribute='bsd_subsale' operator='eq' value='{0}' />\r\n                    </filter>\r\n                  </entity>\r\n                </fetch>", (object)id);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }
    }
}
