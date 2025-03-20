// Decompiled with JetBrains decompiler
// Type: SaleDirectAction.SaleDirectAction
// Assembly: SaleDirectAction, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4e71628980e853ee
// MVID: F9B79C4D-3E6B-49FD-A188-86807627C22E
// Assembly location: C:\Users\XUAN CHINH\Downloads\SaleDirectAction.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SaleDirectAction
{
    public class SaleDirectAction : IPlugin
    {
        public static IOrganizationService service;
        private static IOrganizationServiceFactory factory;
        private static StringBuilder strbuil = new StringBuilder();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference entityReference1 = (EntityReference)context.InputParameters["Target"];
            string str1 = context.InputParameters["Command"].ToString();
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            try
            {
                strbuil.AppendLine("11111111");
                //throw new InvalidPluginExecutionException(strbuil.ToString());
                Main(str1, entityReference1, context);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
            
        }
        public static void Main(string str1, EntityReference entityReference1, IPluginExecutionContext context)
        {
            try
            {
                if (str1 == "Book")
                {
                    //factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    //service = factory.CreateOrganizationService(context.UserId);
                    Entity entity1 = RetrieveValidUnit(entityReference1.Id);
                    if (((OptionSetValue)entity1["statecode"]).Value == 1)
                        throw new InvalidPluginExecutionException("This unit is not public!");
                    if (((OptionSetValue)entity1["statuscode"]).Value == 100000002)
                        throw new InvalidPluginExecutionException("This unit was sold!");
                    if (!entity1.Contains("bsd_floor"))
                        throw new InvalidPluginExecutionException("Please select floor for this unit!");
                    if (!entity1.Contains("bsd_blocknumber"))
                        throw new InvalidPluginExecutionException("Please select block for this unit!");
                    if (!entity1.Contains("bsd_projectcode"))
                        throw new InvalidPluginExecutionException("Please select project for this unit!");
                    if (!entity1.Contains("defaultuomid"))
                        throw new InvalidPluginExecutionException("Please select default unit for this unit!");
                    Entity entity2 = new Entity("opportunity");
                    entity2["name"] = (object)entity1["name"].ToString();
                    entity2["bsd_project"] = entity1["bsd_projectcode"];
                    entity2["bsd_units"] = (object)entityReference1;
                    EntityReference pricelist_id = null;
                    if (entity1.Contains("bsd_phaseslaunchid"))
                    {
                        entity2["bsd_phaselaunch"] = entity1["bsd_phaseslaunchid"];
                        EntityReference entityReference2 = PhasesLaunchPriceList((EntityReference)entity1["bsd_phaseslaunchid"]);
                        pricelist_id = entityReference2;
                        if (entityReference2 == null)
                            throw new InvalidPluginExecutionException("Please choose pricelist for this phaseslaunch!");
                        entity2["pricelevelid"] = (object)entityReference2;
                        entity2["bsd_queuingfee"] = (object)new Money(Decimal.Zero);
                    }
                    else
                    {
                        EntityReference entityReference2 = (EntityReference)entity1["bsd_projectcode"];
                        Entity entity3 = service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(new string[2]
                        {
            "bsd_pricelistdefault",
            "bsd_bookingfee"
                        }));
                        if (entity3 == null)
                            throw new InvalidPluginExecutionException("Project named '" + entityReference2.Name + "' is not available!");
                        entity2["bsd_queuingfee"] = entity1.Contains("bsd_queuingfee") ? entity1["bsd_queuingfee"] : entity3.Contains("bsd_bookingfee") ? entity3["bsd_bookingfee"] : new Money(Decimal.Zero);
                    }
                    EntityReference pricelist_ref = null;

                    if (pricelist_id != null)
                    {
                        var rplCopy = getListByIDCopy(service, pricelist_id.Id);
                      
                        if (rplCopy == null || rplCopy.Entities.Count == 0)
                        {
                        }
                        else
                        {
                            var copy = rplCopy[0];
                            pricelist_ref = new EntityReference(copy.LogicalName, copy.Id);
                        }
                      //  entity2["bsd_pricelistapply"] = pricelist_ref;
                    }
                    Guid guid = service.Create(entity2);
                    Entity entity4 = new Entity("opportunityproduct");
                    entity4["opportunityid"] = entity4["bsd_booking"] = new EntityReference("opportunity", guid);
                    entity4["uomid"] = entity1["defaultuomid"];
                    entity4["bsd_floor"] = entity1["bsd_floor"];
                    entity4["bsd_block"] = entity1["bsd_blocknumber"];
                    entity4["bsd_project"] = entity1["bsd_projectcode"];
                    entity4["productid"] = entity4["bsd_units"] = entityReference1;
                    entity4["isproductoverridden"] = false;
                    entity4["ispriceoverridden"] = false;
                    StringBuilder st = new StringBuilder();
                    decimal amount = 0;
                    if (pricelist_ref != null)
                    {

                        var fetchXml = $@"
                                    <fetch>
                                      <entity name='productpricelevel'>
                                        <attribute name='amount' />
                                        <filter>
                                          <condition attribute='pricelevelid' operator='eq' value='{pricelist_ref.Id}'/>
                                          <condition attribute='productid' operator='eq' value='{entityReference1.Id}'/>
                                        </filter>
                                      </entity>
                                    </fetch>";
                        EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                        if (list.Entities.Count > 0)
                        {
                             amount = list.Entities[0].Contains("amount") ? ((Money)list.Entities[0]["amount"]).Value : 0;
                            if (amount > 0)
                            {
                                entity4["isproductoverridden"] = true;
                                entity4["ispriceoverridden"] = true;
                                entity4["priceperunit"] = new Money(amount);
                                entity4["extendedamount"] = new Money(amount);
                                
                                entity4["bsd_pricelist"] = pricelist_ref;
                            }
                           
                        }
                        
                       
                        
                    }
                    else
                    {
                        if (entity1.Contains("bsd_listprice"))
                            entity4["priceperunit"] = entity1["bsd_listprice"];

                        if (entity2.Contains("pricelevelid"))
                            entity4["bsd_pricelist"] = entity2["pricelevelid"];

                    }
                   
                   
                    entity4["quantity"] = (object)Decimal.One;
                   
                    if (entity1.Contains("bsd_phaseslaunchid"))
                    {
                        entity4["bsd_status"] = true;
                        entity4["bsd_phaseslaunch"] = entity1["bsd_phaseslaunchid"];
                    }
                    //throw new InvalidPluginExecutionException("nghiax tesst ");
                  Guid idopppro =  service.Create(entity4);
                 
                   

                    string str2 = "tmp={type:'Success',content:'" + guid.ToString() + "'}";
                    context.OutputParameters["Result"] = str2;
                }
                else if (str1 == "Reservation")
                {
                    //factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    //service = factory.CreateOrganizationService(context.UserId);
                    Entity enUnit = RetrieveValidUnit(entityReference1.Id);
                    if (enUnit == null)
                        throw new InvalidPluginExecutionException("Unit is not avaliable please check detail of unit!");
                    if (((OptionSetValue)enUnit["statecode"]).Value == 1)
                        throw new InvalidPluginExecutionException("This unit is not public!");
                    if (((OptionSetValue)enUnit["statuscode"]).Value != 100000000 && ((OptionSetValue)enUnit["statuscode"]).Value != 100000001 && ((OptionSetValue)enUnit["statuscode"]).Value != 100000004)
                        throw new InvalidPluginExecutionException("Unit must be available or queueing!");
                    if (((OptionSetValue)enUnit["statuscode"]).Value == 100000002)
                        throw new InvalidPluginExecutionException("Unit is sold!");
                    if (!enUnit.Contains("bsd_phaseslaunchid"))
                        throw new InvalidPluginExecutionException("Unit is not launched!");
                    if (!enUnit.Contains("bsd_floor"))
                        throw new InvalidPluginExecutionException("Please select floor for this unit!");
                    if (!enUnit.Contains("bsd_blocknumber"))
                        throw new InvalidPluginExecutionException("Please select block for this unit!");
                    if (!enUnit.Contains("bsd_projectcode"))
                        throw new InvalidPluginExecutionException("Please select project for this unit!");
                    if (!enUnit.Contains("defaultuomid"))
                        throw new InvalidPluginExecutionException("Please select default unit for this unit!");
                    if (!enUnit.Contains("bsd_depositamount"))
                        throw new InvalidPluginExecutionException("Please provide deposit for this unit!");
                    Entity entity2 = new Entity("quote");
                    entity2["name"] = enUnit["name"];
                    entity2["bsd_projectid"] = enUnit["bsd_projectcode"];
                    entity2["transactioncurrencyid"] = enUnit["transactioncurrencyid"];
                    entity2["bsd_depositfee"] = enUnit["bsd_depositamount"];
                    entity2["bsd_phaseslaunchid"] = enUnit["bsd_phaseslaunchid"];
                    entity2["bsd_unitno"] = (object)entityReference1;
                    entity2["bsd_unitstatus"] = enUnit["statuscode"];
                    strbuil.AppendLine("22222");
                    entity2["bsd_netusablearea"] = enUnit.Contains("bsd_netsaleablearea") ? enUnit["bsd_netsaleablearea"] : Decimal.Zero;
                    entity2["bsd_constructionarea"] = enUnit.Contains("bsd_constructionarea") ? enUnit["bsd_constructionarea"] : Decimal.Zero;
                    int numberofmonthspaidmf = -1;
                    strbuil.AppendLine("333333");
                    Entity entity3 = service.Retrieve(((EntityReference)enUnit["bsd_projectcode"]).LogicalName, ((EntityReference)enUnit["bsd_projectcode"]).Id, new ColumnSet(true));
                    if (enUnit.Contains("bsd_numberofmonthspaidmf"))
                    {
                        numberofmonthspaidmf = (int)enUnit["bsd_numberofmonthspaidmf"];
                        entity2["bsd_numberofmonthspaidmf"] = enUnit["bsd_numberofmonthspaidmf"];
                    }
                    else if (entity3.Contains("bsd_numberofmonthspaidmf"))
                    {
                        numberofmonthspaidmf = (int)entity3["bsd_numberofmonthspaidmf"];
                        entity2["bsd_numberofmonthspaidmf"] = entity3["bsd_numberofmonthspaidmf"];
                    }
                    strbuil.AppendLine("444444");
                    Decimal managementamount = entity3.Contains("bsd_managementamount") ? ((Money)entity3["bsd_managementamount"]).Value : Decimal.Zero;
                    Decimal netsaleablearea = enUnit.Contains("bsd_netsaleablearea") ? (Decimal)enUnit["bsd_netsaleablearea"] : Decimal.Zero;
                    Decimal actualarea = enUnit.Contains("bsd_actualarea") ? (Decimal)enUnit["bsd_actualarea"] : Decimal.Zero;
                    if (enUnit.Contains("bsd_managementamountmonth"))
                        managementamount = ((Money)enUnit["bsd_managementamountmonth"]).Value;
                    if (numberofmonthspaidmf > -1)
                    {
                        Decimal num5 = actualarea != Decimal.Zero ? actualarea : netsaleablearea;
                        Decimal num6 = new Decimal(1.1);
                        Guid idpro = ((EntityReference)enUnit["bsd_projectcode"]).Id;
                        Guid check = new Guid("{30B83A61-4FB3-ED11-83FF-002248593808}");
                        Guid check1 = new Guid("{1D561ECF-5221-EE11-9966-000D3AA0853D}");
                        Guid check2 = new Guid("{A1403588-5021-EE11-9CBE-000D3AA14FB9}");
                        Decimal num7 = 0;
                        if (idpro == check || idpro == check1 || idpro == check2)
                        {
                            num7 = (Decimal)numberofmonthspaidmf * num5 * managementamount;
                        }
                        else
                        {
                            num7 = (Decimal)numberofmonthspaidmf * num5 * managementamount * num6;
                        }

                        entity2["bsd_managementfee"] = (object)new Money(num7);
                    }
                    strbuil.AppendLine("5555555555");
                    EntityCollection taxCode = findTaxCode();
                    if (taxCode.Entities.Count > 0)
                    {
                        foreach (Entity entity4 in (Collection<Entity>)taxCode.Entities)
                            entity2["bsd_taxcode"] = (object)entity4.ToEntityReference();
                    }
                    strbuil.AppendLine("66666666");
                    if (enUnit.Contains("bsd_phaseslaunchid"))
                    {
                        EntityReference entityReference2 = PhasesLaunchPriceList((EntityReference)enUnit["bsd_phaseslaunchid"]);
                        if (entityReference2 == null)
                            throw new InvalidPluginExecutionException("Please choose pricelist for this phaseslaunch!");
                        entity2["bsd_pricelistphaselaunch"] = (object)entityReference2;
                    }
                    else
                    {
                        if (!enUnit.Contains("pricelevelid"))
                            throw new InvalidPluginExecutionException("Please enter 'default price list' on this Unit!");
                        entity2["bsd_pricelistphaselaunch"] = enUnit["pricelevelid"];
                    }
                    #region Update pricelist mới nhất
                    EntityReference pricelist_ref = null;
                    pricelist_ref = (EntityReference)entity2["bsd_pricelistphaselaunch"];
                    strbuil.AppendLine("aaaaa");
                    if (pricelist_ref != null)
                    {
                        strbuil.AppendLine("bbbbbbb");
                        var rplCopy = getListByIDCopy(service, pricelist_ref.Id);
                        strbuil.AppendLine("cccccc");
                        if (rplCopy == null || rplCopy.Entities.Count == 0)
                        {
                            strbuil.AppendLine("ddddd");
                        }
                        else
                        {
                            strbuil.AppendLine("eeeee");
                            var copy = rplCopy[0];
                            pricelist_ref = new EntityReference(copy.LogicalName, copy.Id);
                        }
                        strbuil.AppendLine("fffffffff");
                        entity2["pricelevelid"] = pricelist_ref; 
                    }
                    #endregion
                    strbuil.AppendLine("7777777");
                    //#region Update pricelist mới nhất
                    //EntityReference pricelist_ref = null;
                    //pricelist_ref = (EntityReference)entity2["pricelevelid"];
                    //if (pricelist_ref != null)
                    //{
                    //    var rplCopy = getListByIDCopy(service, pricelist_ref.Id);
                    //    if (rplCopy == null || rplCopy.Entities.Count == 0)
                    //    {
                    //    }
                    //    else
                    //    {
                    //        var copy = rplCopy[0];
                    //        pricelist_ref = new EntityReference(copy.LogicalName, copy.Id);
                    //    }
                    //    entity2["bsd_pricelistphaselaunch"] = pricelist_ref;
                    //}
                    //#endregion
                    
                    if (context.InputParameters.Contains("Parameters") && context.InputParameters["Parameters"] != null)
                    {
                        strbuil.AppendLine("88888888");
                        DataContractJsonSerializer contractJsonSerializer = new DataContractJsonSerializer(typeof(InputParameter[]));
                        MemoryStream ser = new MemoryStream(Encoding.UTF8.GetBytes((string)context.InputParameters["Parameters"]));
                        InputParameter[] inputParameter1 = (InputParameter[])contractJsonSerializer.ReadObject(ser);
                        foreach (InputParameter inputParameter in inputParameter1)
                        {
                            if (inputParameter.action == str1)
                            {
                                strbuil.AppendLine("9999999999");
                                Entity entity4 = service.Retrieve(inputParameter.name, Guid.Parse(inputParameter.value), new ColumnSet(new string[5]
                                {
                  "bsd_nameofstaffagent",
                  "customerid",
                  "bsd_queuingfee",
                  "bsd_salesagentcompany",
                  "bsd_referral"
                                }));
                                EntityReference entityReference2 = entity4.Contains("customerid") ? (EntityReference)entity4["customerid"] : (EntityReference)null;
                                if (entityReference2 != null)
                                {
                                    entity2["customerid"] = (object)entityReference2;
                                    EntityReference enfBA = getBankAccount(entityReference2.Id);
                                    if(enfBA != null)
                                        entity2["bsd_bankaccount"] = enfBA;
                                }
                                if (entity4.Contains("bsd_queuingfee"))
                                    entity2["bsd_bookingfee"] = entity4["bsd_queuingfee"];
                                if (entity4.Contains("bsd_nameofstaffagent"))
                                    entity2["bsd_nameofstaffagent"] = entity4["bsd_nameofstaffagent"];
                                if (entity4.Contains("bsd_salesagentcompany"))
                                    entity2["bsd_salessgentcompany"] = entity4["bsd_salesagentcompany"];
                                if (entity4.Contains("bsd_referral"))
                                    entity2["bsd_referral"] = entity4["bsd_referral"];
                            }
                        }

                    }

                    
                    strbuil.AppendLine("aaaaa");
                    Guid guid = service.Create(entity2);
                    Entity entity5 = new Entity("quotedetail");
                    //entity5["priceperunit"] = entity1["price"];
                    entity5["isproductoverridden"] = true;
                    entity5["ispriceoverridden"] = true;
                    entity5["productid"] = new EntityReference("product", entityReference1.Id);
                    entity5["quantity"] = (decimal)1;
                    //throw new InvalidPluginExecutionException("test");

                    entity5["priceperunit"] = new Money(((Money)enUnit["price"]).Value);
                    entity5["uomid"] = enUnit["defaultuomid"];
                    entity5["transactioncurrencyid"] = enUnit["transactioncurrencyid"];
                    entity5["quoteid"] = new EntityReference("quote", guid);
                    service.Create(entity5);
                    strbuil.AppendLine("bbbbbbbbbbbb");
                    //throw new InvalidPluginExecutionException(strbuil.ToString());
                    context.OutputParameters["Result"] = "tmp={type:'Success',content:'" + guid.ToString() + "'}";
                }
                else
                {
                    if (!(str1 == "OptionEntry"))
                        throw new InvalidPluginExecutionException("Command is not valid!");
                    //factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    //service = factory.CreateOrganizationService(context.UserId);
                    Entity entity1 = RetrieveValidUnit(entityReference1.Id);
                    if (entity1 == null)
                        throw new InvalidPluginExecutionException("Unit is not avaliable please check detail of unit!");
                    if (((OptionSetValue)entity1["statecode"]).Value == 1)
                        throw new InvalidPluginExecutionException("This unit is not public!");
                    if (((OptionSetValue)entity1["statuscode"]).Value != 100000000 && ((OptionSetValue)entity1["statuscode"]).Value != 100000001)
                        throw new InvalidPluginExecutionException("Unit must be available or on hold!");
                    if (!entity1.Contains("bsd_phaseslaunchid"))
                        throw new InvalidPluginExecutionException("Unit is not launched");
                    if (!entity1.Contains("bsd_floor"))
                        throw new InvalidPluginExecutionException("Please select floor for this unit!");
                    if (!entity1.Contains("bsd_blocknumber"))
                        throw new InvalidPluginExecutionException("Please select block for this unit!");
                    if (!entity1.Contains("bsd_projectcode"))
                        throw new InvalidPluginExecutionException("Please select project for this unit!");
                    if (!entity1.Contains("defaultuomid"))
                        throw new InvalidPluginExecutionException("Please select default unit for this unit!");
                    Entity entity2 = new Entity("salesorder");
                    entity2["name"] = entity1["productnumber"];
                    entity2["transactioncurrencyid"] = entity1["transactioncurrencyid"];
                    entity2["bsd_project"] = entity1["bsd_projectcode"];
                    entity2["bsd_phaseslaunch"] = entity1["bsd_phaseslaunchid"];
                    entity2["statuscode"] = (object)new OptionSetValue(100000008);
                    if (entity1.Contains("bsd_phaseslaunchid"))
                    {
                        EntityReference entityReference2 = PhasesLaunchPriceList((EntityReference)entity1["bsd_phaseslaunchid"]);
                        if (entityReference2 == null)
                            throw new InvalidPluginExecutionException("Please choose pricelist for this phaseslaunch!");
                        entity2["pricelevelid"] = (object)entityReference2;
                    }
                    else
                    {
                        if (!entity1.Contains("pricelevelid"))
                            throw new InvalidPluginExecutionException("Please enter 'default price list' on this Unit!");
                        entity2["pricelevelid"] = entity1["pricelevelid"];
                    }
                    Guid guid = service.Create(entity2);
                    Entity entity3 = new Entity("salesorderdetail");

                    entity3["isproductoverridden"] = (object)false;
                    entity3["ispriceoverridden"] = false;
                    entity3["productid"] = new EntityReference("product", entityReference1.Id);
                    entity3["quantity"] = Decimal.One;
                    entity3["uomid"] = entity1["defaultuomid"];
                    entity3["transactioncurrencyid"] = entity1["transactioncurrencyid"];
                    entity3["salesorderid"] = new EntityReference("salesorder", guid);
                    service.Create(entity3);
                    context.OutputParameters["Result"] = "tmp={type:'Success',content:'" + guid.ToString() + "'}";
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }

        private static EntityCollection findTaxCode()
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>\r\n                      <entity name='bsd_taxcode'>\r\n                        <attribute name='bsd_taxcodeid' />\r\n                        <filter type='and'>\r\n                          <condition attribute='bsd_default' operator='eq' value='1' />\r\n                        </filter>\r\n                      </entity>\r\n                    </fetch>");
            return service.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }

        private static Entity RetrieveValidUnit(Guid unitId)
        {
            QueryExpression q = new QueryExpression();
            q.EntityName = "product";
            q.ColumnSet = new ColumnSet(true);
            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("productid", ConditionOperator.Equal, unitId));
            LinkEntity link_floor_unit = new LinkEntity("product", "bsd_floor", "bsd_floor", "bsd_floorid", JoinOperator.Inner);
            link_floor_unit.EntityAlias = "fl";
            link_floor_unit.Columns = new ColumnSet(new string[] { "bsd_block" });
            q.LinkEntities.Add(link_floor_unit);
            LinkEntity link_block_floor = new LinkEntity("bsd_floor", "bsd_block", "bsd_block", "bsd_blockid", JoinOperator.Inner);
            link_block_floor.EntityAlias = "bl";
            link_block_floor.Columns = new ColumnSet(new string[] { "bsd_project" });
            link_floor_unit.LinkEntities.Add(link_block_floor);
            LinkEntity link_project_block = new LinkEntity("bsd_block", "bsd_project", "bsd_project", "bsd_projectid", JoinOperator.Inner);
            link_project_block.EntityAlias = "pj";
            link_project_block.Columns = new ColumnSet(new string[] { "bsd_defaultpaymentscheme", "bsd_pricelistdefault" });
            link_block_floor.LinkEntities.Add(link_project_block);
            q.TopCount = 1;

            #region FetchXML

            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("");
            //sb.AppendLine("<fetch mapping='logical' count='1' output-format='xml-platform'>");
            //sb.AppendLine("<entity name='product'>");
            //sb.AppendLine("<attribute name='name'/>");
            //sb.AppendLine("<attribute name='productid'/>");
            //sb.AppendLine("<attribute name='bsd_floor'/>");
            //sb.AppendLine("<attribute name='bsd_blocknumber'/>");
            //sb.AppendLine("<attribute name='bsd_projectcode'/>");
            //sb.AppendLine("<attribute name='bsd_phaseslaunchid'/>");
            //sb.AppendLine("<attribute name='bsd_listprice'/>");
            //sb.AppendLine("<attribute name='defaultuomid'/>");
            //sb.AppendLine("<attribute name='statuscode'/>");
            //sb.AppendLine("<attribute name='statecode'/>");
            //sb.AppendLine("<filter type='and'>");
            //sb.AppendLine("<condition attribute='productid' operator='eq' value='" + unitId.ToString() + "'></condition>");
            //sb.AppendLine("</filter>");
            //sb.AppendLine("<link-entity name='bsd_floor' from='bsd_floorid' to='bsd_floor' link-type='inner'>");
            //sb.AppendLine("<attribute name='bsd_block'/>");
            //sb.AppendLine("<link-entity name='bsd_block' from='bsd_blockid' to='bsd_block' link-type='inner'>");
            //sb.AppendLine("<attribute name='bsd_project'/>");
            //sb.AppendLine("<link-entity name='bsd_project' from='bsd_projectid' to='bsd_project' link-type='inner'></link-entity>");
            //sb.AppendLine("</link-entity>");
            //sb.AppendLine("</link-entity>");
            //sb.AppendLine("</entity>");
            //sb.AppendLine("</fetch>"); 
            #endregion
            //EntityCollection entcs = service.RetrieveMultiple(new FetchExpression(sb.ToString()));
            EntityCollection entcs = service.RetrieveMultiple(q);
            if (entcs.Entities.Count == 0)
                return null;
            else
                return entcs.Entities[0];
        }

        private static EntityReference PhasesLaunchPriceList(EntityReference phaseLaunch)
        {
            Entity en = service.Retrieve(phaseLaunch.LogicalName, phaseLaunch.Id, new ColumnSet(new string[] { "bsd_pricelistid" }));
            if (en.Attributes.Contains("bsd_pricelistid"))
                return (EntityReference)en["bsd_pricelistid"];
            else
                return null;
        }

        private static Money GetQueuefee(EntityReference phaseF)
        {
            Money m = new Money(0);
            if (phaseF != null)
            {
                Entity tmp = service.Retrieve(phaseF.LogicalName, phaseF.Id, new ColumnSet(new string[] {
                    "bsd_bookingfee"
                }));
                if (tmp.Contains("bsd_bookingfee"))
                    m = (Money)tmp["bsd_bookingfee"];
            }
            return m;
        }

        private static Money GetQepositfee(EntityReference pmSchRef)
        {
            Money money = new Money(Decimal.Zero);
            if (pmSchRef != null)
            {
                Entity entity = service.Retrieve(pmSchRef.LogicalName, pmSchRef.Id, new ColumnSet(new string[1]
                {
          "bsd_depositamount"
                }));
                if (entity.Contains("bsd_depositamount"))
                    money = (Money)entity["bsd_depositamount"];
            }
            return money;
        }
        private static EntityCollection getListByIDCopy(IOrganizationService service, Guid idcopy)
        {
            #region --- Danh sách sắp xếp theo filter createdon mới nhất top 1 ---

            var fetchXml = $@"
<fetch>
  <entity name='pricelevel'>
    <all-attributes />
    <filter type='and'>
      <condition attribute='bsd_approved' operator='eq' value='1'/>
      <condition attribute='bsd_pricelistcopy' operator='eq' value='{idcopy}'/>
    </filter>
    <order attribute='createdon' descending='true' />
  </entity>
</fetch>";

            //var xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
            //      <entity name='pricelevel'>
            //        <attribute name='name' />
            //        <attribute name='transactioncurrencyid' />
            //        <attribute name='enddate' />
            //        <attribute name='begindate' />
            //        <attribute name='statecode' />
            //        <attribute name='createdon' />
            //        <attribute name='pricelevelid' />
            //        <order attribute='createdon' descending='true' />
            //        <filter type='and'>
            //          <condition attribute='bsd_approved' operator='eq' value='1' />
            //          <condition attribute='bsd_pricelistcopy' operator='eq'  uitype='pricelevel' value='" + idcopy + @"' />
            //        </filter>
            //      </entity>
            //    </fetch>";
            #endregion

            var rs = service.RetrieveMultiple(new FetchExpression(fetchXml.ToString()));
            return rs;
        }

        private static EntityReference getBankAccount(Guid customerId)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch top=""1"">
              <entity name=""bsd_bankaccount"">
                <attribute name=""bsd_name"" />
                <filter>
                  <condition attribute=""bsd_customer"" operator=""eq"" value=""{customerId}"" />
                  <condition attribute=""bsd_default"" operator=""eq"" value=""1"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result == null && result.Entities.Count <= 0) return null;
            return result.Entities[0].ToEntityReference();
        }

        [DataContract]
        public class InputParameter
        {
            [DataMember]
            public string action { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string value { get; set; }
        }
    }
}
