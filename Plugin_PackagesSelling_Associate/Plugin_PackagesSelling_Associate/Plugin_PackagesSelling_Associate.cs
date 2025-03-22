// Decompiled with JetBrains decompiler
// Type: Plugin_PackagesSelling_Associate.Plugin_PackagesSelling_Associate
// Assembly: Plugin_PackagesSelling_Associate, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1e7b199793955ed9
// MVID: 2BBA803A-91E1-455F-9747-205D7EB1020D

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Plugin_PackagesSelling_Associate
{
    public class Plugin_PackagesSelling_Associate : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service1.UserId));
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (service1.MessageName == "Associate")
            {
                Relationship inputParameter1 = (Relationship)service1.InputParameters["Relationship"];
                EntityReference target = (EntityReference)service1.InputParameters["Target"];
                traceService.Trace("target: " + target.LogicalName + " " + target.Id);

                //if (inputParameter1.SchemaName == "bsd_quote_bsd_packageselling" || inputParameter1.SchemaName == "bsd_salesorder_bsd_packageselling")
                if (inputParameter1.SchemaName == "bsd_quote_bsd_packageselling" && target.LogicalName == "bsd_packageselling")
                {
                    EntityReferenceCollection inputParameter3 = (EntityReferenceCollection)service1.InputParameters["RelatedEntities"];
                    Entity entity1 = new Entity();
                    string attributeName = "";
                    string str = "";
                    if (inputParameter1.SchemaName == "bsd_quote_bsd_packageselling")
                    {
                        attributeName = "bsd_phaseslaunchid";
                        str = "bsd_quote_bsd_packageselling";
                    }
                    else if (inputParameter1.SchemaName == "bsd_salesorder_bsd_packageselling")
                    {
                        attributeName = "bsd_phaseslaunch";
                        str = "bsd_salesorder_bsd_packageselling";
                    }
                    //        Entity entity2 = this.service.Retrieve(inputParameter2.LogicalName, inputParameter2.Id, new ColumnSet(new string[4]
                    //        {
                    //attributeName,
                    //"bsd_totalamountlessfreight",
                    //"bsd_detailamount",
                    //"bsd_discount"
                    //        }));
                    Entity enHD = this.service.Retrieve(inputParameter3[0].LogicalName, inputParameter3[0].Id, new ColumnSet(new string[4]
                    {
                        attributeName,
                        "bsd_totalamountlessfreight",
                        "bsd_detailamount",
                        "bsd_discount"
                    }));
                    if (!enHD.Contains(attributeName))
                        throw new InvalidPluginExecutionException("The Phases launch is currently empty. Please fill in the blank before processing this transaction.");
                    if (this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>\r\n                                      <entity name='bsd_bsd_phaseslaunch_bsd_packageselling'>\r\n                                            <filter type='and'>\r\n                                              <condition attribute='bsd_phaseslaunchid' operator='eq' value='{0}' />\r\n                                              <condition attribute='bsd_packagesellingid' operator='eq' value='{1}' />\r\n                                            </filter>\r\n                                      </entity>\r\n                                    </fetch>", ((EntityReference)enHD[attributeName]).Id, target.Id))).Entities.Count == 0)
                        throw new InvalidPluginExecutionException("The package you selected is not on this phase launch. Please pick another one.");
                    StringBuilder stringBuilder1 = new StringBuilder();
                    stringBuilder1.AppendLine("<fetch mapping='logical' version='1.0'>");
                    stringBuilder1.AppendLine("<entity name='" + str + "'>");
                    stringBuilder1.AppendLine("<filter type='and'>");
                    if (inputParameter1.SchemaName == "bsd_quote_bsd_packageselling")
                        stringBuilder1.AppendLine(string.Format("<condition attribute='quoteid' operator='eq' value='{0}' />", (object)enHD.Id));
                    else if (inputParameter1.SchemaName == "bsd_salesorder_bsd_packageselling")
                        stringBuilder1.AppendLine(string.Format("<condition attribute='salesorderid' operator='eq' value='{0}' />", (object)enHD.Id));
                    stringBuilder1.AppendLine("</filter>");
                    stringBuilder1.AppendLine("</entity>");
                    stringBuilder1.AppendLine("</fetch>");
                    EntityCollection entityCollection1 = this.service.RetrieveMultiple((QueryBase)new FetchExpression(stringBuilder1.ToString()));
                    Entity entity3 = new Entity(enHD.LogicalName);
                    entity3.Id = enHD.Id;
                    Decimal num1 = 0M;
                    foreach (Entity entity4 in (Collection<Entity>)entityCollection1.Entities)
                    {
                        Entity entity5 = this.service.Retrieve(target.LogicalName, (Guid)entity4["bsd_packagesellingid"], new ColumnSet(new string[8]
                        {
              "bsd_name",
              "bsd_amount",
              "bsd_type",
              "bsd_priceperm2",
              "bsd_unittype",
              "bsd_percent",
              "bsd_method",
              "bsd_byunittype"
                        }));
                        if (entity5.Contains("bsd_byunittype"))
                        {
                            StringBuilder stringBuilder2 = new StringBuilder();
                            stringBuilder2.AppendLine("<fetch version='1.0' mapping='logical'>");
                            stringBuilder2.AppendLine("<entity name='product'>");
                            stringBuilder2.AppendLine("<attribute name='bsd_netsaleablearea' />");
                            stringBuilder2.AppendLine("<attribute name='bsd_actualarea' />");
                            stringBuilder2.AppendLine("<attribute name='bsd_unittype' />");
                            if (inputParameter1.SchemaName == "bsd_quote_bsd_packageselling")
                            {
                                stringBuilder2.AppendLine("<link-entity name='quotedetail' from='productid' to='productid' alias='af'>");
                                stringBuilder2.AppendLine("<filter type='and'>");
                                stringBuilder2.AppendLine(string.Format("<condition attribute='quoteid' operator='eq' value='{0}' />", (object)enHD.Id));
                            }
                            else if (inputParameter1.SchemaName == "bsd_salesorder_bsd_packageselling")
                            {
                                stringBuilder2.AppendLine("<link-entity name='salesorderdetail' from='productid' to='productid' alias='ae'>");
                                stringBuilder2.AppendLine("<filter type='and'>");
                                stringBuilder2.AppendLine(string.Format("<condition attribute='salesorderid' operator='eq' value='{0}' />", (object)enHD.Id));
                            }
                            stringBuilder2.AppendLine("</filter>");
                            stringBuilder2.AppendLine("</link-entity>");
                            stringBuilder2.AppendLine("</entity>");
                            stringBuilder2.AppendLine("</fetch>");
                            EntityCollection entityCollection2 = this.service.RetrieveMultiple((QueryBase)new FetchExpression(stringBuilder2.ToString()));
                            if ((bool)entity5["bsd_byunittype"])
                            {
                                if (entityCollection2.Entities.Count > 0)
                                {
                                    Entity entity6 = entityCollection2.Entities[0];
                                    if (entity6.Contains("bsd_unittype"))
                                    {
                                        if (entity6["bsd_unittype"] != entity5["bsd_unittype"])
                                            throw new InvalidPluginExecutionException("Handover condition is not matching with Unit type. Please check again!");
                                        if (!entity5.Contains("bsd_method"))
                                            throw new InvalidPluginExecutionException("Please choose Method of Handover condition " + (string)entity5["bsd_name"]);
                                        switch (((OptionSetValue)entity5["bsd_method"]).Value)
                                        {
                                            case 100000000:
                                                if (!entity5.Contains("bsd_priceperm2"))
                                                    throw new InvalidPluginExecutionException("Please choose Price/m2 of Handover Condition " + (string)entity5["bsd_name"]);
                                                if (entity6.Contains("bsd_actualarea"))
                                                {
                                                    Decimal num2 = (Decimal)entity6["bsd_actualarea"] * ((Money)entity5["bsd_priceperm2"]).Value;
                                                    num1 += num2;
                                                    break;
                                                }
                                                if (entity6.Contains("bsd_netsaleablearea"))
                                                {
                                                    Decimal num3 = (Decimal)entity6["bsd_netsaleablearea"] * ((Money)entity5["bsd_priceperm2"]).Value;
                                                    num1 += num3;
                                                }
                                                break;
                                            case 100000001:
                                                if (!entity5.Contains("bsd_amount"))
                                                    throw new InvalidPluginExecutionException("Please choose Amount of Handover Condition " + (string)entity5["bsd_name"]);
                                                if (entity6.Contains("bsd_actualarea"))
                                                {
                                                    Decimal num4 = ((Money)entity5["bsd_amount"]).Value;
                                                    num1 += num4;
                                                    break;
                                                }
                                                if (entity6.Contains("bsd_netsaleablearea"))
                                                {
                                                    Decimal num5 = ((Money)entity5["bsd_amount"]).Value;
                                                    num1 += num5;
                                                }
                                                break;
                                            case 100000002:
                                                Decimal num6 = ((Money)enHD["bsd_discount"]).Value;
                                                Decimal num7 = ((Money)enHD["bsd_detailamount"]).Value;
                                                Decimal num8 = entity5.Contains("bsd_percent") ? (Decimal)entity5["bsd_percent"] : throw new InvalidPluginExecutionException("Please choose Percent of Handover Condition " + (string)entity5["bsd_name"]);
                                                num1 += (num7 - num6) * num8 / 100M;
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Entity entity7 = entityCollection2.Entities[0];
                                if (!entity5.Contains("bsd_method"))
                                    throw new InvalidPluginExecutionException("Please choose Method of Handover condition " + (string)entity5["bsd_name"]);
                                switch (((OptionSetValue)entity5["bsd_method"]).Value)
                                {
                                    case 100000000:
                                        if (!entity5.Contains("bsd_priceperm2"))
                                            throw new InvalidPluginExecutionException("Please choose Price/m2 of Handover Condition " + (string)entity5["bsd_name"]);
                                        if (entity7.Contains("bsd_actualarea"))
                                        {
                                            Decimal num9 = (Decimal)entity7["bsd_actualarea"] * ((Money)entity5["bsd_priceperm2"]).Value;
                                            num1 += num9;
                                            break;
                                        }
                                        if (entity7.Contains("bsd_netsaleablearea"))
                                        {
                                            Decimal num10 = (Decimal)entity7["bsd_netsaleablearea"] * ((Money)entity5["bsd_priceperm2"]).Value;
                                            num1 += num10;
                                        }
                                        break;
                                    case 100000001:
                                        if (!entity5.Contains("bsd_amount"))
                                            throw new InvalidPluginExecutionException("Please choose Amount of Handover condition " + (string)entity5["bsd_name"]);
                                        if (entity7.Contains("bsd_actualarea"))
                                        {
                                            Decimal num11 = ((Money)entity5["bsd_amount"]).Value;
                                            num1 += num11;
                                            break;
                                        }
                                        if (entity7.Contains("bsd_netsaleablearea"))
                                        {
                                            Decimal num12 = ((Money)entity5["bsd_amount"]).Value;
                                            num1 += num12;
                                        }
                                        break;
                                    case 100000002:
                                        Decimal num13 = ((Money)enHD["bsd_discount"]).Value;
                                        Decimal num14 = ((Money)enHD["bsd_detailamount"]).Value;
                                        Decimal num15 = entity5.Contains("bsd_percent") ? (Decimal)entity5["bsd_percent"] : throw new InvalidPluginExecutionException("Please choose Percent of Handover condition " + (string)entity5["bsd_name"]);
                                        num1 += (num14 - num13) * num15 / 100M;
                                        break;
                                }
                            }
                        }
                    }
                    entity3["bsd_packagesellingamount"] = (object)new Money(num1);
                    this.service.Update(entity3);
                }
                else if ("bsd_salesorder_bsd_packageselling".Equals(inputParameter1.SchemaName) && "bsd_packageselling".Equals(target.LogicalName))
                {
                    Entity enPackagesSelling = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_name", "bsd_amount" }));
                    string bsd_name = enPackagesSelling.Contains("bsd_name") ? (string)enPackagesSelling["bsd_name"] : string.Empty;
                    decimal bsd_amount = enPackagesSelling.Contains("bsd_amount") ? ((Money)enPackagesSelling["bsd_amount"]).Value : 0;
                    if (bsd_amount > 0)
                        throw new InvalidPluginExecutionException("Cannot add Handover Condition, Handover Condition '" + bsd_name + "' has amount > 0.");
                }
            }
            else if (service1.MessageName == "Disassociate")
            {
                Relationship inputParameter4 = (Relationship)service1.InputParameters["Relationship"];
                EntityReference target = (EntityReference)service1.InputParameters["Target"];
                traceService.Trace("target: " + target.LogicalName + " " + target.Id);
                //if (inputParameter4.SchemaName == "bsd_quote_bsd_packageselling" || inputParameter4.SchemaName == "bsd_salesorder_bsd_packageselling")
                if (inputParameter4.SchemaName == "bsd_quote_bsd_packageselling" && target.LogicalName == "quote")
                {
                    EntityReferenceCollection inputParameter6 = (EntityReferenceCollection)service1.InputParameters["RelatedEntities"];
                    Entity entity8 = new Entity();
                    string str1 = "";
                    string str2 = "";
                    if (inputParameter4.SchemaName == "bsd_quote_bsd_packageselling")
                    {
                        str1 = "bsd_phaseslaunchid";
                        str2 = "bsd_quote_bsd_packageselling";
                    }
                    else if (inputParameter4.SchemaName == "bsd_salesorder_bsd_packageselling")
                    {
                        str1 = "bsd_phaseslaunch";
                        str2 = "bsd_salesorder_bsd_packageselling";
                    }
                    if (inputParameter4.SchemaName == "bsd_quote_bsd_packageselling")
                        entity8 = this.service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[4]
                        {
            str1,
            "bsd_totalamountlessfreight",
            "bsd_detailamount",
            "bsd_discount"
                        }));
                    else if (inputParameter4.SchemaName == "bsd_salesorder_bsd_packageselling")
                        entity8 = this.service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[1]
                        {
            str1
                        }));
                    StringBuilder stringBuilder3 = new StringBuilder();
                    stringBuilder3.AppendLine("<fetch mapping='logical' version='1.0'>");
                    stringBuilder3.AppendLine("<entity name='" + str2 + "'>");
                    stringBuilder3.AppendLine("<filter type='and'>");
                    if (inputParameter4.SchemaName == "bsd_quote_bsd_packageselling")
                        stringBuilder3.AppendLine(string.Format("<condition attribute='quoteid' operator='eq' value='{0}' />", (object)target.Id));
                    else if (inputParameter4.SchemaName == "bsd_salesorder_bsd_packageselling")
                        stringBuilder3.AppendLine(string.Format("<condition attribute='salesorderid' operator='eq' value='{0}' />", (object)target.Id));
                    stringBuilder3.AppendLine("</filter>");
                    stringBuilder3.AppendLine("</entity>");
                    stringBuilder3.AppendLine("</fetch>");
                    EntityCollection entityCollection3 = this.service.RetrieveMultiple((QueryBase)new FetchExpression(stringBuilder3.ToString()));
                    Entity entity9 = new Entity(target.LogicalName);
                    entity9.Id = target.Id;
                    Decimal num16 = 0M;
                    foreach (Entity entity10 in (Collection<Entity>)entityCollection3.Entities)
                    {
                        Entity entity11 = this.service.Retrieve(inputParameter6[0].LogicalName, (Guid)entity10["bsd_packagesellingid"], new ColumnSet(new string[8]
                        {
            "bsd_name",
            "bsd_amount",
            "bsd_type",
            "bsd_priceperm2",
            "bsd_unittype",
            "bsd_percent",
            "bsd_method",
            "bsd_byunittype"
                        }));
                        if (entity11.Contains("bsd_byunittype"))
                        {
                            StringBuilder stringBuilder4 = new StringBuilder();
                            stringBuilder4.AppendLine("<fetch version='1.0' mapping='logical'>");
                            stringBuilder4.AppendLine("<entity name='product'>");
                            stringBuilder4.AppendLine("<attribute name='bsd_netsaleablearea' />");
                            stringBuilder4.AppendLine("<attribute name='bsd_actualarea' />");
                            stringBuilder4.AppendLine("<attribute name='bsd_unittype' />");
                            if (inputParameter4.SchemaName == "bsd_quote_bsd_packageselling")
                            {
                                stringBuilder4.AppendLine("<link-entity name='quotedetail' from='productid' to='productid' alias='af'>");
                                stringBuilder4.AppendLine("<filter type='and'>");
                                stringBuilder4.AppendLine(string.Format("<condition attribute='quoteid' operator='eq' value='{0}' />", (object)entity8.Id));
                            }
                            else if (inputParameter4.SchemaName == "bsd_salesorder_bsd_packageselling")
                            {
                                stringBuilder4.AppendLine("<link-entity name='salesorderdetail' from='productid' to='productid' alias='ae'>");
                                stringBuilder4.AppendLine("<filter type='and'>");
                                stringBuilder4.AppendLine(string.Format("<condition attribute='salesorderid' operator='eq' value='{0}' />", (object)entity8.Id));
                            }
                            stringBuilder4.AppendLine("</filter>");
                            stringBuilder4.AppendLine("</link-entity>");
                            stringBuilder4.AppendLine("</entity>");
                            stringBuilder4.AppendLine("</fetch>");
                            EntityCollection entityCollection4 = this.service.RetrieveMultiple((QueryBase)new FetchExpression(stringBuilder4.ToString()));
                            if ((bool)entity11["bsd_byunittype"])
                            {
                                if (entityCollection4.Entities.Count > 0)
                                {
                                    Entity entity12 = entityCollection4.Entities[0];
                                    if (entity12.Contains("bsd_unittype"))
                                    {
                                        if (entity12["bsd_unittype"] != entity11["bsd_unittype"])
                                            throw new InvalidPluginExecutionException("Handover condition is not matching with Unit type. Please check again!");
                                        if (!entity11.Contains("bsd_method"))
                                            throw new InvalidPluginExecutionException("Please choose Method of Handover condition " + (string)entity11["bsd_name"]);
                                        switch (((OptionSetValue)entity11["bsd_method"]).Value)
                                        {
                                            case 100000000:
                                                if (!entity11.Contains("bsd_priceperm2"))
                                                    throw new InvalidPluginExecutionException("Please choose Price/m2 of Handover condition " + (string)entity11["bsd_name"]);
                                                if (entity12.Contains("bsd_actualarea"))
                                                {
                                                    Decimal num17 = (Decimal)entity12["bsd_actualarea"] * ((Money)entity11["bsd_priceperm2"]).Value;
                                                    num16 += num17;
                                                    break;
                                                }
                                                if (entity12.Contains("bsd_netsaleablearea"))
                                                {
                                                    Decimal num18 = (Decimal)entity12["bsd_netsaleablearea"] * ((Money)entity11["bsd_priceperm2"]).Value;
                                                    num16 += num18;
                                                }
                                                break;
                                            case 100000001:
                                                if (!entity11.Contains("bsd_amount"))
                                                    throw new InvalidPluginExecutionException("Please choose Amount of Handover condition " + (string)entity11["bsd_name"]);
                                                if (entity12.Contains("bsd_actualarea"))
                                                {
                                                    Decimal num19 = ((Money)entity11["bsd_amount"]).Value;
                                                    num16 += num19;
                                                    break;
                                                }
                                                if (entity12.Contains("bsd_netsaleablearea"))
                                                {
                                                    Decimal num20 = ((Money)entity11["bsd_amount"]).Value;
                                                    num16 += num20;
                                                }
                                                break;
                                            case 100000002:
                                                Decimal num21 = ((Money)entity8["bsd_discount"]).Value;
                                                Decimal num22 = ((Money)entity8["bsd_detailamount"]).Value;
                                                Decimal num23 = entity11.Contains("bsd_percent") ? (Decimal)entity11["bsd_percent"] : throw new InvalidPluginExecutionException("Please choose Percent of Handover condition " + (string)entity11["bsd_name"]);
                                                num16 += (num22 - num21) * num23 / 100M;
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Entity entity13 = entityCollection4.Entities[0];
                                if (!entity11.Contains("bsd_method"))
                                    throw new InvalidPluginExecutionException("Please choose Method of Handover condition " + (string)entity11["bsd_name"]);
                                switch (((OptionSetValue)entity11["bsd_method"]).Value)
                                {
                                    case 100000000:
                                        if (!entity11.Contains("bsd_priceperm2"))
                                            throw new InvalidPluginExecutionException("Please choose Price/m2 of Handover condition " + (string)entity11["bsd_name"]);
                                        if (entity13.Contains("bsd_actualarea"))
                                        {
                                            Decimal num24 = (Decimal)entity13["bsd_actualarea"] * ((Money)entity11["bsd_priceperm2"]).Value;
                                            num16 += num24;
                                            break;
                                        }
                                        if (entity13.Contains("bsd_netsaleablearea"))
                                        {
                                            Decimal num25 = (Decimal)entity13["bsd_netsaleablearea"] * ((Money)entity11["bsd_priceperm2"]).Value;
                                            num16 += num25;
                                        }
                                        break;
                                    case 100000001:
                                        if (!entity11.Contains("bsd_amount"))
                                            throw new InvalidPluginExecutionException("Please choose Amount of Handover condition " + (string)entity11["bsd_name"]);
                                        if (entity13.Contains("bsd_actualarea"))
                                        {
                                            Decimal num26 = ((Money)entity11["bsd_amount"]).Value;
                                            num16 += num26;
                                            break;
                                        }
                                        if (entity13.Contains("bsd_netsaleablearea"))
                                        {
                                            Decimal num27 = ((Money)entity11["bsd_amount"]).Value;
                                            num16 += num27;
                                        }
                                        break;
                                    case 100000002:
                                        if (!entity11.Contains("bsd_percent"))
                                            throw new InvalidPluginExecutionException("Please choose Percent of Handover condition " + (string)entity11["bsd_name"]);
                                        Decimal num28 = ((Money)entity8["bsd_discount"]).Value;
                                        Decimal num29 = ((Money)entity8["bsd_detailamount"]).Value;
                                        Decimal num30 = (Decimal)entity11["bsd_percent"];
                                        num16 += (num29 - num28) * num30 / 100M;
                                        break;
                                }
                            }
                        }
                    }
                    entity9["bsd_packagesellingamount"] = (object)new Money(num16);
                    this.service.Update(entity9);
                }
                else if ("bsd_salesorder_bsd_packageselling".Equals(inputParameter4.SchemaName) && "salesorder".Equals(target.LogicalName))
                {
                    EntityReferenceCollection relatedEntities = (EntityReferenceCollection)service1.InputParameters["RelatedEntities"];
                    if (relatedEntities.Count > 0)
                    {
                        List<Guid> listId = new List<Guid>();
                        foreach (var relatedEntity in relatedEntities)
                        {
                            listId.Add(relatedEntity.Id);
                        }

                        var query = new QueryExpression("bsd_packageselling");
                        query.ColumnSet.AddColumns("bsd_packagesellingid", "bsd_name", "bsd_amount", "bsd_startdate", "bsd_enddate");
                        query.Criteria.AddCondition("bsd_amount", ConditionOperator.GreaterThan, 0);
                        query.Criteria.AddCondition("bsd_packagesellingid", ConditionOperator.In, listId.Cast<object>().ToArray());

                        EntityCollection rs = service.RetrieveMultiple(query);
                        if (rs != null && rs.Entities != null && rs.Entities.Count > 0)
                        {
                            throw new InvalidPluginExecutionException("Cannot remove Handover Condition, Handover Condition has amount > 0.");
                        }
                    }
                }
            }
        }

        private EntityCollection RetrieveMultiRecord(
          IOrganizationService crmservices,
          string entity,
          ColumnSet column,
          string condition,
          object value)
        {
            QueryExpression query = new QueryExpression(entity);
            query.ColumnSet = column;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            return this.service.RetrieveMultiple((QueryBase)query);
        }
    }
}
