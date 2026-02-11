using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Action_PhasesLaunch_Release
{

    public class Action_PhasesLaunch_Release : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            EntityReference target = (EntityReference)context.InputParameters["Target"];
            if (target.LogicalName == "bsd_phaseslaunch")
            {
                string input_01 = (string)context.InputParameters["Input01"];
                if (input_01 == "Bước 01")
                {
                    Entity PhaseLaunch = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_pricelistid", "bsd_discountlist" }));
                    if (PhaseLaunch.Contains("bsd_pricelistid"))
                    {
                        EntityReference enrfPricList = PhaseLaunch.Contains("bsd_pricelistid") ? (EntityReference)PhaseLaunch["bsd_pricelistid"] : null;
                        if (enrfPricList != null)
                        {
                            Entity enPriceList = service.Retrieve(enrfPricList.LogicalName, enrfPricList.Id, new ColumnSet(true));
                            int statuscode = enPriceList.Contains("statuscode") ? ((OptionSetValue)enPriceList["statuscode"]).Value : 0;
                            bool approve = enPriceList.Contains("bsd_approved") ? ((bool)enPriceList["bsd_approved"]) : false;
                            if (statuscode != 100001 || approve == false)
                            {
                                throw new InvalidPluginExecutionException("Vui lòng kiểm tra lại Pricelist!");
                            }
                        }
                        EntityCollection pricelistitems = RetrieveMultiRecord(service, "productpricelevel", new ColumnSet(new string[] { "productid", "amount" }), "pricelevelid", ((EntityReference)PhaseLaunch["bsd_pricelistid"]).Id);
                        if (pricelistitems.Entities.Count == 0)
                            throw new InvalidPluginExecutionException("The price list you have chosen doesn't contain any unit. Please check again.");
                        if (PhaseLaunch.Contains("bsd_discountlist"))
                        {
                            Guid dc = ((EntityReference)PhaseLaunch["bsd_discountlist"]).Id;
                            Boolean check = CheckDiscountList(dc);
                            if (check == false)
                                throw new InvalidPluginExecutionException("This discount list has expired. Please select another list.");
                            Boolean pm = CheckPaymentScheme(target.Id);
                            if (pm == false)
                                throw new InvalidPluginExecutionException("There are no approved payment schemes. Please re-check.");
                            List<string> products = new List<string>();
                            //get product unit preparing
                            List<string> productpre = getProductsFromUnitPre(PhaseLaunch.Id);
                            if (productpre.Count > 0)
                                products.AddRange(productpre);
                            // get product blocks launch
                            List<string> productblocks = getProductsFromBlocks(PhaseLaunch.Id);
                            if (productblocks.Count > 0)
                                products.AddRange(productblocks);
                            // get proudct floors launch
                            List<string> productfloors = getProductsFromFloors(PhaseLaunch.Id);
                            if (productfloors.Count > 0)
                                products.AddRange(productfloors);
                            // get product Units launch
                            List<string> productUnits = getProductsFromUnits(PhaseLaunch.Id);
                            if (productUnits.Count > 0)
                                products.AddRange(productUnits);
                            List<string> products2 = products.Distinct().ToList();
                            if (products2.Count > 0)
                            {
                                traceService.Trace("1");
                                Entity phaseslaunch = new Entity(target.LogicalName);
                                phaseslaunch.Id = target.Id;
                                traceService.Trace("3333333333333333");
                                phaseslaunch["bsd_powerautomate"] = true;
                                service.Update(phaseslaunch);
                                context.OutputParameters["Output01"] = "output_1_if";
                                context.OutputParameters["Output02"] = string.Join(";", products2);
                                //string url = "";
                                //EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                                //    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Phases Launch Release");
                                //foreach (Entity item in configGolive.Entities)
                                //{
                                //    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                                //}
                                //if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                                //context.OutputParameters["Output03"] = url;
                            }
                            else
                            {
                                List<string> productsElse = new List<string>();
                                traceService.Trace("2");
                                foreach (Entity pli in pricelistitems.Entities)
                                {
                                    traceService.Trace("2.1");
                                    Entity unit = service.Retrieve(((EntityReference)pli["productid"]).LogicalName, ((EntityReference)pli["productid"]).Id, new ColumnSet(new string[] { "statecode" }));
                                    if (((OptionSetValue)unit["statecode"]).Value != 0)
                                        throw new InvalidPluginExecutionException("The Unit state is not acvite.");
                                    else
                                    {
                                        string item = ((EntityReference)pli["productid"]).Id.ToString();
                                        productsElse.Add(item);
                                    }
                                }
                                List<string> productsElse2 = productsElse.Distinct().ToList();
                                if (productsElse2.Count == 0)
                                    throw new InvalidPluginExecutionException("None of the units can be released.");
                                Entity phaseslaunch = new Entity(target.LogicalName);
                                phaseslaunch.Id = target.Id;
                                traceService.Trace("3333333333333333");
                                phaseslaunch["bsd_powerautomate"] = true;
                                service.Update(phaseslaunch);
                                
                                context.OutputParameters["Output01"] = "output_1_else";
                                context.OutputParameters["Output02"] = string.Join(";", productsElse2);
                                //string url = "";
                                //EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                                //    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Phases Launch Release");
                                //foreach (Entity item in configGolive.Entities)
                                //{
                                //    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                                //}
                                //if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                                //context.OutputParameters["Output03"] = url;
                            }
                        }
                        else throw new InvalidPluginExecutionException("Please insert Discount List.");
                    }
                    else throw new InvalidPluginExecutionException("Please insert Price List.");
                }
                else if (input_01 == "Bước 02")
                {
                    string input_02 = (string)context.InputParameters["Input02"];
                    string input_03 = (string)context.InputParameters["Input03"];
                    Entity PhaseLaunch = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                    if (input_02 == "output_1_if")
                    {
                        Entity a = service.Retrieve("product", Guid.Parse(input_03), new ColumnSet(new string[] { "statuscode", "bsd_blocknumber", "bsd_floor", "bsd_vippriority", "bsd_phaseslaunchid", "bsd_landlot", "bsd_plotnumber" }));

                        traceService.Trace("1.2");
                        bool checkp = false;
                        bool f_vippriority = CheckVipViority(a);
                        if (f_vippriority == false)
                        {
                            traceService.Trace("1.3");
                            if (a.Contains("bsd_phaseslaunchid"))
                            {
                                traceService.Trace("1.3.1");
                                EntityCollection unitpre = RetriveMultiRecord("bsd_unit_preparing", false, new string[] { "bsd_unit" }, "bsd_unit", a.Id, "bsd_phareslaunch", target.Id, null);
                                if (unitpre.Entities.Count > 0)
                                {
                                    traceService.Trace("1.3.1.1");
                                    Entity up = new Entity(unitpre.Entities[0].LogicalName, unitpre.Entities[0].Id);
                                    up["bsd_reason"] = "This Units was released.";
                                    up["bsd_secondreason"] = "This Units was released.";
                                    up["bsd_reasonnumber"] = 2;
                                    service.Update(up);
                                }
                                else
                                {
                                    traceService.Trace("1.3.1.2");
                                    Entity newpre = new Entity("bsd_unit_preparing");
                                    newpre["bsd_phareslaunch"] = PhaseLaunch.ToEntityReference();
                                    newpre["bsd_pricelist"] = PhaseLaunch["bsd_pricelistid"];
                                    newpre["bsd_project"] = PhaseLaunch["bsd_projectid"];
                                    newpre["bsd_block"] = a.Contains("bsd_blocknumber") ? a["bsd_blocknumber"] : (a.Contains("bsd_landlot") ? a["bsd_landlot"] : null);
                                    newpre["bsd_floor"] = a.Contains("bsd_floor") ? a["bsd_floor"] : (a.Contains("bsd_plotnumber") ? a["bsd_plotnumber"] : null);
                                    newpre["bsd_unit"] = a.ToEntityReference();
                                    newpre["bsd_reason"] = "This Units was released.";
                                    newpre["bsd_secondreason"] = "This Units was released.";
                                    newpre["bsd_reasonnumber"] = 2;
                                    service.Create(newpre);
                                }
                            }
                            else
                            {
                                traceService.Trace("1.3.2");
                                EntityCollection pricelistitems = RetrieveMultiRecord_2value(service, "productpricelevel", new ColumnSet(new string[] { "productid", "amount" }), "productid", a.Id, "pricelevelid", ((EntityReference)PhaseLaunch["bsd_pricelistid"]).Id);
                                foreach (Entity pli in pricelistitems.Entities)
                                {
                                    traceService.Trace("1.3.2.1.1");
                                    checkp = true;
                                    //______________________________________
                                    //Add price to unit from price list!
                                    Entity u = new Entity("product");
                                    u.Id = a.Id;
                                    if (((OptionSetValue)a["statuscode"]).Value == 1)
                                        u["statuscode"] = new OptionSetValue(100000000);
                                    u["price"] = pli["amount"];
                                    u["bsd_phaseslaunchid"] = PhaseLaunch.ToEntityReference();
                                    if (PhaseLaunch.Contains("bsd_salesagentcompany"))
                                        u["bsd_saleagentcompany"] = PhaseLaunch["bsd_salesagentcompany"];
                                    if (PhaseLaunch.Contains("bsd_locked"))
                                        u["bsd_locked"] = PhaseLaunch["bsd_locked"];
                                    service.Update(u);
                                    AddPhaseLaunchToQueue(service, PhaseLaunch.Id, u.Id);
                                    //______________________________________
                                    Entity unitlaunch = new Entity("bsd_unitlaunched");
                                    unitlaunch["bsd_phaseslaunchid"] = PhaseLaunch.ToEntityReference();
                                    unitlaunch["bsd_blockid"] = a.Contains("bsd_blocknumber") ? a["bsd_blocknumber"] : (a.Contains("bsd_landlot") ? a["bsd_landlot"] : null);
                                    unitlaunch["bsd_floorid"] = a.Contains("bsd_floor") ? a["bsd_floor"] : (a.Contains("bsd_plotnumber") ? a["bsd_plotnumber"] : null);
                                    unitlaunch["bsd_productid"] = a.ToEntityReference();
                                    unitlaunch["bsd_price"] = pli["amount"];
                                    service.Create(unitlaunch);

                                    EntityCollection unitpre = RetrieveMultiRecord(service, "bsd_unit_preparing", new ColumnSet(new string[] { "bsd_unit" }), "bsd_unit", a.Id);
                                    foreach (Entity en in unitpre.Entities)
                                        service.Delete(en.LogicalName, en.Id);
                                }
                                if (checkp == false)
                                {
                                    traceService.Trace("1.3.2.1.2");
                                    EntityCollection unitpre = RetriveMultiRecord("bsd_unit_preparing", false, new string[] { "bsd_unit" }, "bsd_unit", a.Id, "bsd_phareslaunch", target.Id, null);
                                    if (unitpre.Entities.Count > 0)
                                    {
                                        Entity up = new Entity(unitpre.Entities[0].LogicalName, unitpre.Entities[0].Id);
                                        up["bsd_reason"] = "The price list does not contain this Units";
                                        up["bsd_secondreason"] = "The price list does not contain this Units";
                                        up["bsd_reasonnumber"] = 1;
                                        service.Update(up);
                                    }
                                    else
                                    {
                                        Entity newpre = new Entity("bsd_unit_preparing");
                                        newpre["bsd_phareslaunch"] = PhaseLaunch.ToEntityReference();
                                        newpre["bsd_pricelist"] = PhaseLaunch["bsd_pricelistid"];
                                        newpre["bsd_project"] = PhaseLaunch["bsd_projectid"];
                                        newpre["bsd_block"] = a.Contains("bsd_blocknumber") ? a["bsd_blocknumber"] : (a.Contains("bsd_landlot") ? a["bsd_landlot"] : null);
                                        newpre["bsd_floor"] = a.Contains("bsd_floor") ? a["bsd_floor"] : (a.Contains("bsd_plotnumber") ? a["bsd_plotnumber"] : null);
                                        newpre["bsd_unit"] = a.ToEntityReference();
                                        newpre["bsd_reason"] = "The price list does not contain this Units";
                                        newpre["bsd_secondreason"] = "The price list does not contain this Units";
                                        newpre["bsd_reasonnumber"] = 1;
                                        service.Create(newpre);
                                    }
                                }
                            }
                        }
                        else if (f_vippriority == true)
                        {
                            traceService.Trace("1.4");
                            EntityCollection unitpre = RetriveMultiRecord("bsd_unit_preparing", false, new string[] { "bsd_unit" }, "bsd_unit", a.Id, "bsd_phareslaunch", target.Id, null);
                            if (unitpre.Entities.Count > 0)
                            {
                                traceService.Trace("1.4.1");
                                Entity up = new Entity(unitpre.Entities[0].LogicalName, unitpre.Entities[0].Id);
                                up["bsd_reason"] = "This Units has VIP priority.";
                                up["bsd_secondreason"] = "This Units has VIP priority.";
                                up["bsd_reasonnumber"] = 3;
                                service.Update(up);
                            }
                            else
                            {
                                traceService.Trace("1.4.2");
                                Entity newpre = new Entity("bsd_unit_preparing");
                                //newpre["bsd_name"] = a.ToEntityReference().ToString();
                                newpre["bsd_phareslaunch"] = PhaseLaunch.ToEntityReference();
                                newpre["bsd_pricelist"] = PhaseLaunch["bsd_pricelistid"];
                                newpre["bsd_project"] = PhaseLaunch["bsd_projectid"];
                                newpre["bsd_block"] = a.Contains("bsd_blocknumber") ? a["bsd_blocknumber"] : (a.Contains("bsd_landlot") ? a["bsd_landlot"] : null);
                                newpre["bsd_floor"] = a.Contains("bsd_floor") ? a["bsd_floor"] : (a.Contains("bsd_plotnumber") ? a["bsd_plotnumber"] : null);
                                newpre["bsd_unit"] = a.ToEntityReference();
                                newpre["bsd_reason"] = "This Units has VIP priority.";
                                newpre["bsd_secondreason"] = "This Units has VIP priority.";
                                newpre["bsd_reasonnumber"] = 3;
                                service.Create(newpre);
                            }
                        }
                    }
                    else if (input_02 == "output_1_else")
                    {
                        traceService.Trace("2.0.0");
                        Entity unit = service.Retrieve("product", Guid.Parse(input_03), new ColumnSet(true));
                        EntityCollection pricelistitems = RetrieveMultiRecord_2value(service, "productpricelevel", new ColumnSet(new string[] { "productid", "amount" }), "productid", unit.Id, "pricelevelid", ((EntityReference)PhaseLaunch["bsd_pricelistid"]).Id);
                        foreach (Entity pli in pricelistitems.Entities)
                        {
                            traceService.Trace("2.1.1");
                            // Hai CM 04 05 24 
                            //@@ check if Units have VIP priority == true  - not release when click button Release on Phaselaunch
                            bool f_vippriority = CheckVipViority(unit);
                            if (f_vippriority == false)
                            {
                                traceService.Trace("2.1.2");
                                if (!unit.Contains("bsd_phaseslaunchid"))
                                {
                                    traceService.Trace("2.1.2.1");
                                    //______________________________________
                                    //Add price to unit from price list!
                                    Entity u = new Entity("product");
                                    u.Id = ((EntityReference)pli["productid"]).Id;
                                    if (((OptionSetValue)unit["statuscode"]).Value == 1)
                                        u["statuscode"] = new OptionSetValue(100000000);
                                    u["price"] = pli["amount"];
                                    u["bsd_phaseslaunchid"] = PhaseLaunch.ToEntityReference();
                                    traceService.Trace("2.1.2.2");
                                    if (PhaseLaunch.Contains("bsd_salesagentcompany"))
                                        u["bsd_saleagentcompany"] = PhaseLaunch["bsd_salesagentcompany"];
                                    if (PhaseLaunch.Contains("bsd_locked"))
                                        u["bsd_locked"] = PhaseLaunch["bsd_locked"];
                                    traceService.Trace("2.1.2.3");
                                    service.Update(u);
                                    AddPhaseLaunchToQueue(service, PhaseLaunch.Id, u.Id);
                                    //______________________________________
                                    Entity unitlaunch = new Entity("bsd_unitlaunched");
                                    traceService.Trace("2.1.2.4");
                                    unitlaunch["bsd_phaseslaunchid"] = PhaseLaunch.ToEntityReference();
                                    traceService.Trace("2.1.2.5");
                                    unitlaunch["bsd_blockid"] = unit.Contains("bsd_blocknumber") ? unit["bsd_blocknumber"] : (unit.Contains("bsd_landlot") ? unit["bsd_landlot"] : null);
                                    traceService.Trace("2.1.2.6");
                                    unitlaunch["bsd_floorid"] = unit.Contains("bsd_floor") ? unit["bsd_floor"] : (unit.Contains("bsd_plotnumber") ? unit["bsd_plotnumber"] : null);
                                    traceService.Trace("2.1.2.7");
                                    unitlaunch["bsd_productid"] = unit.ToEntityReference();
                                    traceService.Trace("2.1.2.8");
                                    unitlaunch["bsd_price"] = pli["amount"];
                                    traceService.Trace("2.1.2.9");
                                    service.Create(unitlaunch);
                                }
                                else if (unit.Contains("bsd_phaseslaunchid"))
                                {
                                    traceService.Trace("2.1.3.1");
                                    EntityCollection unitpre = RetriveMultiRecord("bsd_unit_preparing", false, new string[] { "bsd_unit" }, "bsd_unit", unit.Id, "bsd_phareslaunch", target.Id, null);
                                    if (unitpre.Entities.Count > 0)
                                    {
                                        traceService.Trace("2.1.3.2");
                                        Entity up = new Entity(unitpre.Entities[0].LogicalName, unitpre.Entities[0].Id);
                                        up["bsd_reason"] = "This Units was released.";
                                        traceService.Trace("2.1.3.3");
                                        up["bsd_secondreason"] = "This Units was released.";
                                        up["bsd_reasonnumber"] = 2;
                                        service.Update(up);
                                    }
                                    else
                                    {
                                        traceService.Trace("2.1.3.4.1");
                                        Entity newpre = new Entity("bsd_unit_preparing");
                                        traceService.Trace("2.1.3.5.2");
                                        //newpre["bsd_name"] = unit.ToEntityReference().ToString();
                                        newpre["bsd_phareslaunch"] = PhaseLaunch.ToEntityReference();
                                        traceService.Trace("2.1.3.6.3");
                                        newpre["bsd_pricelist"] = PhaseLaunch.Contains("bsd_pricelistid") ? PhaseLaunch["bsd_pricelistid"] : null;
                                        traceService.Trace("2.1.3.5.4.1.1.1");
                                        newpre["bsd_project"] = PhaseLaunch.Contains("bsd_projectid") ? PhaseLaunch["bsd_projectid"] : null;
                                        traceService.Trace("2.1.3.4.1.1.1");
                                        newpre["bsd_block"] = unit.Contains("bsd_blocknumber") ? unit["bsd_blocknumber"] : (unit.Contains("bsd_landlot") ? unit["bsd_landlot"] : null);
                                        traceService.Trace("2.1.3.5.5");
                                        newpre["bsd_floor"] = unit.Contains("bsd_floor") ? unit["bsd_floor"] : (unit.Contains("bsd_plotnumber") ? unit["bsd_plotnumber"] : null);
                                        if (unit != null)
                                        {
                                            newpre["bsd_unit"] = unit.ToEntityReference();
                                        }
                                        traceService.Trace("2.1.3.5.6");
                                        traceService.Trace("2.1.3.5.1");
                                        newpre["bsd_reason"] = "This Units was released.";
                                        traceService.Trace("2.1.3.5.3");
                                        newpre["bsd_secondreason"] = "This Units was released.";
                                        traceService.Trace("2.1.3.5.2");
                                        newpre["bsd_reasonnumber"] = 2;
                                        traceService.Trace("2.1.3.5.3");
                                        traceService.Trace("2.1.3.6");
                                        service.Create(newpre);
                                    }
                                }
                            }
                            else if (f_vippriority)
                            {
                                traceService.Trace("2.1.3");
                                EntityCollection unitpre = RetriveMultiRecord("bsd_unit_preparing", false, new string[] { "bsd_unit" }, "bsd_unit", unit.Id, "bsd_phareslaunch", target.Id, null);
                                if (unitpre.Entities.Count > 0)
                                {
                                    traceService.Trace("2.1.3.3.1");
                                    Entity up = new Entity(unitpre.Entities[0].LogicalName, unitpre.Entities[0].Id);
                                    up["bsd_reason"] = "This Units has VIP priority.";
                                    up["bsd_secondreason"] = "This Units has VIP priority.";
                                    up["bsd_reasonnumber"] = 3;
                                    traceService.Trace("2.1.3.3.2");
                                    service.Update(up);
                                }
                                else
                                {
                                    traceService.Trace("2.1.3.3.3");
                                    Entity newpre = new Entity("bsd_unit_preparing");
                                    //newpre["bsd_name"] = unit.ToEntityReference().ToString();
                                    newpre["bsd_phareslaunch"] = PhaseLaunch.ToEntityReference();
                                    newpre["bsd_pricelist"] = PhaseLaunch["bsd_pricelistid"];
                                    newpre["bsd_project"] = PhaseLaunch["bsd_projectid"];
                                    newpre["bsd_block"] = unit.Contains("bsd_blocknumber") ? unit["bsd_blocknumber"] : (unit.Contains("bsd_landlot") ? unit["bsd_landlot"] : null);
                                    traceService.Trace("2.1.3.3.4");
                                    newpre["bsd_floor"] = unit.Contains("bsd_floor") ? unit["bsd_floor"] : (unit.Contains("bsd_plotnumber") ? unit["bsd_plotnumber"] : null);
                                    newpre["bsd_unit"] = unit.ToEntityReference();
                                    newpre["bsd_reason"] = "This Units has VIP priority.";
                                    newpre["bsd_secondreason"] = "This Units has VIP priority.";
                                    newpre["bsd_reasonnumber"] = 3;
                                    traceService.Trace("2.1.3.3.5");
                                    service.Create(newpre);
                                }
                            }
                        }
                    }
                }
                else if (input_01 == "Bước 03")
                {
                    EntityCollection Units = RetrieveMultiRecord(service, "bsd_unitlaunched", new ColumnSet(new string[] { "bsd_productid" }), "bsd_phaseslaunchid", target.Id);
                    Entity phaseslaunch = new Entity(target.LogicalName);
                    phaseslaunch.Id = target.Id;
                    phaseslaunch["bsd_powerautomate"] = false;
                    if (Units.Entities.Count > 0)
                    {
                        DateTime t = RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                        phaseslaunch["statuscode"] = new OptionSetValue(100000000);
                        traceService.Trace("22222222222222222");
                        phaseslaunch["bsd_launchedon"] = t;
                        //phaseslaunch["bsd_launchedfrom"] = new EntityReference("systemuser", context.UserId);
                    }
                    service.Update(phaseslaunch);
                }
            }
        }
        private Boolean CheckDiscountList(Guid id)
        {
            string discount = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='bsd_discounttype'>
                                        <attribute name='bsd_discounttypeid' />
                                        <attribute name='bsd_date' />
                                        <order attribute='bsd_date' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='bsd_discounttypeid' operator='eq' value='{0}' />
                                        </filter>
                                      </entity>
                                    </fetch>";
            discount = string.Format(discount, id);
            EntityCollection D = service.RetrieveMultiple(new FetchExpression(discount));
            if (D.Entities.Count > 0)
            {
                Entity d = D.Entities[0];
                if (d.Contains("bsd_date"))
                {
                    int t = (int)(DateTime.Now.Subtract((DateTime)d["bsd_date"]).TotalDays);
                    if (t < 0)
                        return true;
                    else
                        return false;
                }
                else return true;
            }
            else
                return false;
        }
        private Boolean CheckPaymentScheme(Guid id)
        {
            string PS = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                  <entity name='bsd_paymentscheme'>
                                    <attribute name='bsd_paymentschemeid' />
                                    <filter type='and'>
                                      <condition attribute='statuscode' operator='eq' value='100000000' />
                                    </filter>
                                    <link-entity name='bsd_project' from='bsd_projectid' to='bsd_project' alias='aj'>
                                      <link-entity name='bsd_phaseslaunch' from='bsd_projectid' to='bsd_projectid' alias='ak'>
                                        <filter type='and'>
                                          <condition attribute='bsd_phaseslaunchid' operator='eq' value='{0}' />
                                        </filter>
                                      </link-entity>
                                    </link-entity>
                                  </entity>
                                </fetch>";
            PS = string.Format(PS, id);
            EntityCollection D = service.RetrieveMultiple(new FetchExpression(PS));
            if (D.Entities.Count > 0)
            {
                return true;
            }
            else
                return false;
        }
        public List<string> getProductsFromUnitPre(Guid phaselaunchid)
        {
            List<string> products = new List<string>();
            EntityCollection unitpre = RetriveMultiRecord("bsd_unit_preparing", false, new string[] { "bsd_unit" }, "bsd_phareslaunch", phaselaunchid, null);
            foreach (var item in unitpre.Entities)
            {
                products.Add(((EntityReference)item["bsd_unit"]).Id.ToString());
            }
            return products;
        }
        public List<string> getProductsFromBlocks(Guid phaselaunchid)
        {
            List<string> products = new List<string>();
            EntityCollection blocks = RetriveMultiRecord("bsd_blocklaunch", false, new string[] { "bsd_blockid" }, "bsd_phaseslaunchid", phaselaunchid, null);
            foreach (var item in blocks.Entities)
            {
                EntityCollection productblock = RetriveMultiRecord("product", false, new string[] { "statuscode", "bsd_blocknumber", "bsd_floor", "bsd_vippriority", "bsd_phaseslaunchid" }, "bsd_blocknumber", ((EntityReference)item["bsd_blockid"]).Id, "statecode", 0, null);
                if (productblock.Entities.Count > 0)
                    products.Add(productblock.Entities[0].Id.ToString());
            }
            return products;
        }
        public List<string> getProductsFromFloors(Guid phaselaunchid)
        {
            List<string> products = new List<string>();
            EntityCollection Floors = RetriveMultiRecord("bsd_floorlaunch", false, new string[] { "bsd_floorid" }, "bsd_phaseslaunchid", phaselaunchid, null);
            foreach (var item in Floors.Entities)
            {
                EntityCollection productfloors = RetriveMultiRecord("product", false, new string[] { "statuscode", "bsd_blocknumber", "bsd_floor", "bsd_vippriority", "bsd_phaseslaunchid" }, "bsd_floor", ((EntityReference)item["bsd_floorid"]).Id, "statecode", 0, null);
                if (productfloors.Entities.Count > 0)
                    products.Add(productfloors.Entities[0].Id.ToString());
            }
            return products;
        }
        public List<string> getProductsFromUnits(Guid phaselaunchid)
        {
            List<string> products = new List<string>();
            EntityCollection Units = RetrieveMultiRecord(service, "bsd_unitlaunch", new ColumnSet(new string[] { "bsd_productid" }), "bsd_phaseslaunchid", phaselaunchid);
            foreach (var item in Units.Entities)
            {
                products.Add(((EntityReference)item["bsd_productid"]).Id.ToString());
            }
            return products;
        }
        private EntityCollection RetriveMultiRecord(string entity, bool all, string[] getcolum, string cond1, object value1, string order)
        {
            QueryExpression qe = new QueryExpression();
            qe.EntityName = entity;
            if (all) qe.ColumnSet = new ColumnSet(true);
            else qe.ColumnSet = new ColumnSet(getcolum);
            qe.Criteria.FilterOperator = LogicalOperator.And;
            if (cond1 != null)
            {
                ConditionExpression condcl = new ConditionExpression();
                condcl.AttributeName = cond1;
                condcl.Operator = ConditionOperator.Equal;
                condcl.Values.Add(value1);
                qe.Criteria.AddCondition(condcl);
            }

            if (order != null)
                qe.AddOrder(order, OrderType.Ascending);
            EntityCollection dskq = service.RetrieveMultiple(qe);
            return dskq;
        }
        private EntityCollection RetriveMultiRecord(string entity, bool all, string[] getcolum, string cond1, object value1, string cond2, object value2, string order)
        {

            QueryExpression qe = new QueryExpression();
            qe.EntityName = entity;
            if (all) qe.ColumnSet = new ColumnSet(true);
            else qe.ColumnSet = new ColumnSet(getcolum);
            qe.Criteria.FilterOperator = LogicalOperator.And;
            if (cond1 != null)
            {
                ConditionExpression condcl = new ConditionExpression();
                condcl.AttributeName = cond1;
                condcl.Operator = ConditionOperator.Equal;
                condcl.Values.Add(value1);
                qe.Criteria.AddCondition(condcl);
            }
            if (cond2 != null)
            {
                ConditionExpression condcl = new ConditionExpression();
                condcl.AttributeName = cond2;
                condcl.Operator = ConditionOperator.Equal;
                condcl.Values.Add(value2);
                qe.Criteria.AddCondition(condcl);
            }
            if (order != null)
                qe.AddOrder(order, OrderType.Ascending);
            EntityCollection dskq = service.RetrieveMultiple(qe);
            return dskq;
        }
        EntityCollection RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            EntityCollection entc = service.RetrieveMultiple(q);
            return entc;
        }
        EntityCollection RetrieveMultiRecord_2value(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value, string condition2, object value2)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            q.Criteria.AddCondition(new ConditionExpression(condition2, ConditionOperator.Equal, value2));
            EntityCollection entc = service.RetrieveMultiple(q);
            return entc;
        }
        private Boolean CheckVipViority(Entity unit)
        {
            bool check = false;
            if (unit.Contains("bsd_vippriority") && (bool)unit["bsd_vippriority"] == true)
            {
                check = true;
            }
            return check;
        }
        private void AddPhaseLaunchToQueue(IOrganizationService service, Guid phaseslaunchID, Guid Unit)
        {
            string queues = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                                                  <entity name='opportunity' >
                                                    <attribute name='name' />
                                                    <attribute name='opportunityid' />
                                                    <attribute name='createdon' />
                                                    <attribute name='bsd_queuingexpired' />
                                                    <order attribute='bsd_queuingexpired' descending='true' />
                                                    <filter type='and' >
                                                      <condition attribute='bsd_queuingexpired' operator='not-null' />
                                                      <condition attribute='statuscode' operator='in' >
                                                        <value>100000000</value>
                                                        <value>100000002</value>
                                                      </condition>
                                                    </filter>
                                                    <link-entity name='opportunityproduct' from='opportunityid' to='opportunityid' alias='an' >
                                                     <filter type='and' >
                                                        <condition attribute='bsd_units' operator='eq' value='{0}' />
                                                     </filter>
                                                    </link-entity>
                                                  </entity>
                                            </fetch>";
            queues = string.Format(queues, Unit);
            EntityCollection Q = service.RetrieveMultiple(new FetchExpression(queues));
            if (Q.Entities.Count > 0)
            {
                foreach (Entity en in Q.Entities)
                {
                    Entity q = new Entity(en.LogicalName);
                    q.Id = en.Id;
                    q["bsd_phaselaunch"] = new EntityReference("bsd_phaseslaunch", phaseslaunchID);
                    service.Update(q);
                }
            }
        }
        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
        {

            int? timeZoneCode = RetrieveCurrentUsersSettings(service);


            if (!timeZoneCode.HasValue)
                throw new Exception("Can't find time zone code");



            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
            //var utcTime = utcTime.ToString("MM/dd/yyyy HH:mm:ss");
            //var localDateOnly = response.LocalTime.ToString("dd-MM-yyyy");
        }
        private int? RetrieveCurrentUsersSettings(IOrganizationService service)
        {
            var currentUserSettings = service.RetrieveMultiple(
            new QueryExpression("usersettings")
            {
                ColumnSet = new ColumnSet("localeid", "timezonecode"),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
            }
                }
            }).Entities[0].ToEntity<Entity>();

            return (int?)currentUserSettings.Attributes["timezonecode"];
        }
    }
}
