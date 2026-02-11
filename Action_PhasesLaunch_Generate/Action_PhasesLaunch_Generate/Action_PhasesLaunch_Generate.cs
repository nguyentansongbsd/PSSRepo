using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Action_PhasesLaunch_Generate
{
    public class Action_PhasesLaunch_Generate : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService TracingSe = null;
        StringBuilder strMess = new StringBuilder();
        StringBuilder strMess2 = new StringBuilder();
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            TracingSe = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            string input01 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input01"]))
            {
                input01 = context.InputParameters["input01"].ToString();
            }
            string input02 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input02"]))
            {
                input02 = context.InputParameters["input02"].ToString();
            }
            string input03 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input03"]))
            {
                input03 = context.InputParameters["input03"].ToString();
            }
            string input04 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input04"]))
            {
                input04 = context.InputParameters["input04"].ToString();
            }
            string input05 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input05"]))
            {
                input05 = context.InputParameters["input05"].ToString();
            }
            string input06 = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input06"]))
            {
                input06 = context.InputParameters["input06"].ToString();
            }
            if (input01 == "Bước 01" && input02 != "" && input06 != "")
            {
                TracingSe.Trace("Bước 01");
                Entity enPhasesLaunch = new Entity("bsd_phaseslaunch");
                enPhasesLaunch.Id = Guid.Parse(input02);
                enPhasesLaunch["bsd_powerautomate"] = true;
                service.Update(enPhasesLaunch);
                context.OutputParameters["output01"] = context.UserId.ToString();
                List<string> listBlock = new List<string>();
                List<string> listFloor = new List<string>();
                Entity pl = service.Retrieve("bsd_phaseslaunch", enPhasesLaunch.Id, new ColumnSet(true));
                if (!pl.Contains("bsd_fromblock") && !pl.Contains("bsd_toblock")) throw new InvalidPluginExecutionException("Please insert value for the filter!");
                else
                {
                    Guid id = ((EntityReference)pl["bsd_projectid"]).Id;
                    int s = 0;
                    int e = 0;
                    string blockXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                            <entity name='bsd_block'>
                                            <attribute name='bsd_blockid' />
                                            <attribute name='bsd_name' />
                                            <attribute name='createdon' />
                                            <order attribute='bsd_name' descending='false' />
                                            <filter type='and'>
                                            <condition attribute='bsd_project' operator='eq' uitype='bsd_project' value='{0}' />
                                            </filter>
                                            </entity>
                                            </fetch>";
                    blockXml = string.Format(blockXml, id);
                    EntityCollection blocks = service.RetrieveMultiple(new FetchExpression(blockXml));
                    if (blocks.Entities.Count > 0)
                    {
                        for (int i = 0; i < blocks.Entities.Count; i++)
                        {
                            Entity block = blocks.Entities[i];

                            if (block.Id == ((EntityReference)pl["bsd_fromblock"]).Id)
                            {
                                s = i;
                            }
                            if (block.Id == ((EntityReference)pl["bsd_toblock"]).Id)
                            {
                                e = i;
                            }
                            if (s != 0 && e != 0) break;
                        }
                    }
                    if (pl.Contains("bsd_fromfloor") && pl.Contains("bsd_tofloor"))
                    {
                        Entity floor = new Entity();
                        int sf = 0;
                        int ef = 0;
                        for (int i = s; i < e + 1; i++)
                        {
                            string ID = blocks.Entities[i].Id.ToString();
                            listBlock.Add(ID);
                            string floorXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                    <entity name='bsd_floor'>
                                    <attribute name='bsd_floorid' />
                                    <attribute name='bsd_name' />
                                    <attribute name='bsd_project' />
                                    <attribute name='bsd_floor' />
                                    <attribute name='bsd_block' />
                                    <order attribute='bsd_name' descending='false' />
                                    <filter type='and'>
                                    <condition attribute='bsd_block' operator='eq' uitype='bsd_block' value='{0}' />
                                    </filter>
                                    </entity>
                                    </fetch>";
                            floorXml = string.Format(floorXml, ID);
                            EntityCollection floors = service.RetrieveMultiple(new FetchExpression(floorXml));
                            if (floors.Entities.Count > 0)
                            {

                                for (int y = 0; y < floors.Entities.Count; y++)
                                {
                                    floor = floors.Entities[y];

                                    if (floor.Id == ((EntityReference)pl["bsd_fromfloor"]).Id)
                                    {
                                        sf = y;
                                    }
                                    if (floor.Id == ((EntityReference)pl["bsd_tofloor"]).Id)
                                    {
                                        ef = y;
                                    }
                                    if (sf != 0 && ef != 0) break;
                                }
                            }

                            for (int y = sf; y < ef + 1; y++)
                            {
                                floor = floors.Entities[y];
                                listFloor.Add(floor.Id.ToString());
                            }
                        }
                    }
                    else if (pl.Contains("bsd_fromfloor") && !pl.Contains("bsd_tofloor"))
                    {
                        Entity floor = new Entity();
                        int sf = 0;
                        for (int i = s; i < e + 1; i++)
                        {
                            string ID = blocks.Entities[i].Id.ToString();
                            listBlock.Add(ID);
                            string floorXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                    <entity name='bsd_floor'>
                                    <attribute name='bsd_floorid' />
                                    <attribute name='bsd_name' />
                                    <attribute name='bsd_project' />
                                    <attribute name='bsd_floor' />
                                    <attribute name='bsd_block' />
                                    <order attribute='bsd_name' descending='false' />
                                    <filter type='and'>
                                    <condition attribute='bsd_block' operator='eq' uitype='bsd_block' value='{0}' />
                                    </filter>
                                    </entity>
                                    </fetch>";
                            floorXml = string.Format(floorXml, ID);
                            EntityCollection floors = service.RetrieveMultiple(new FetchExpression(floorXml));
                            if (floors.Entities.Count > 0)
                            {

                                for (int y = 0; y < floors.Entities.Count; y++)
                                {
                                    floor = floors.Entities[y];

                                    if (floor.Id == ((EntityReference)pl["bsd_fromfloor"]).Id)
                                    {
                                        sf = y;
                                    }
                                    if (sf != 0) break;
                                }
                            }
                            for (int z = sf; z < floors.Entities.Count; z++)
                            {
                                floor = floors.Entities[z];
                                listFloor.Add(floor.Id.ToString());
                            }
                        }
                    }
                    else if (!pl.Contains("bsd_fromfloor") && pl.Contains("bsd_tofloor"))
                    {
                        Entity floor = new Entity();

                        int ef = 0;
                        for (int i = s; i < e + 1; i++)
                        {
                            string ID = blocks.Entities[i].Id.ToString();
                            listBlock.Add(ID);
                            string floorXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                    <entity name='bsd_floor'>
                                    <attribute name='bsd_floorid' />
                                    <attribute name='bsd_name' />
                                    <attribute name='bsd_project' />
                                    <attribute name='bsd_floor' />
                                    <attribute name='bsd_block' />
                                    <order attribute='bsd_name' descending='false' />
                                    <filter type='and'>
                                    <condition attribute='bsd_block' operator='eq' uitype='bsd_block' value='{0}' />
                                    </filter>
                                    </entity>
                                    </fetch>";
                            floorXml = string.Format(floorXml, ID);
                            EntityCollection floors = service.RetrieveMultiple(new FetchExpression(floorXml));
                            if (floors.Entities.Count > 0)
                            {

                                for (int y = 0; y < floors.Entities.Count; y++)
                                {
                                    floor = floors.Entities[y];
                                    if (floor.Id == ((EntityReference)pl["bsd_tofloor"]).Id)
                                    {
                                        ef = y;
                                    }
                                    if (ef != 0) break;
                                }
                            }
                            for (int z = 0; z < ef + 1; z++)
                            {
                                floor = floors.Entities[z];
                                listFloor.Add(floor.Id.ToString());
                            }
                        }
                    }
                    else
                    {
                        for (int i = s; i < e + 1; i++)
                        {
                            string ID = blocks.Entities[i].Id.ToString();
                            listBlock.Add(ID);
                            if (input06 == "0")
                            {
                                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch top=""1"">
                                  <entity name=""productpricelevel"">
                                    <attribute name=""productpricelevelid"" />
                                    <filter>
                                      <condition attribute=""pricelevelid"" operator=""eq"" value=""{((EntityReference)pl["bsd_pricelistid"]).Id}"" />
                                      <condition attribute=""pricingmethodcode"" operator=""ne"" value=""{1}"" />
                                    </filter>
                                    <link-entity name=""product"" from=""productid"" to=""productid"">
                                      <link-entity name=""bsd_block"" from=""bsd_blockid"" to=""bsd_landlot"">
                                        <filter>
                                          <condition attribute=""bsd_blockid"" operator=""eq"" value=""{ID}"" />
                                        </filter>
                                      </link-entity>
                                    </link-entity>
                                  </entity>
                                </fetch>";
                                EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                                if (rs.Entities.Count > 0) throw new InvalidPluginExecutionException("This unit is attached with a wrong pricing method. Please re-pick.(Price list item - Currency Amount)");
                            }
                            else
                            {
                                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                                <fetch top=""1"">
                                  <entity name=""productpricelevel"">
                                    <attribute name=""productpricelevelid"" />
                                    <filter>
                                      <condition attribute=""pricelevelid"" operator=""eq"" value=""{((EntityReference)pl["bsd_pricelistid"]).Id}"" />
                                      <condition attribute=""pricingmethodcode"" operator=""ne"" value=""{1}"" />
                                    </filter>
                                    <link-entity name=""product"" from=""productid"" to=""productid"">
                                      <link-entity name=""bsd_block"" from=""bsd_blockid"" to=""bsd_blocknumber"">
                                        <filter>
                                          <condition attribute=""bsd_blockid"" operator=""eq"" value=""{ID}"" />
                                        </filter>
                                      </link-entity>
                                    </link-entity>
                                  </entity>
                                </fetch>";
                                EntityCollection rs = service.RetrieveMultiple(new FetchExpression(fetchXml));
                                if (rs.Entities.Count > 0) throw new InvalidPluginExecutionException("This unit is attached with a wrong pricing method. Please re-pick.(Price list item - Currency Amount)");
                            }
                        }
                    }
                }
                context.OutputParameters["output02"] = string.Join(";", listBlock);
                context.OutputParameters["output04"] = string.Join(";", listFloor);
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Phases Launch Generate");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Không tìm thấy link duyệt bảng giá PA. Vui lòng kiểm tra lại.");
                context.OutputParameters["output03"] = url;
            }
            else if (input01 == "Bước 02" && input04 != "" && input06 != "")
            {
                TracingSe.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                List<string> listUnit = new List<string>();
                if (input05 != "")// có danh sách floor
                {
                    List<string> listFloor = new List<string>();
                    listFloor = input05.Split(';').ToList();
                    listFloor.RemoveAll(string.IsNullOrEmpty);
                    foreach (string item in listFloor)
                    {
                        List<Entity> units = RetrieveMultiRecord2(service, "product", new ColumnSet(new string[] { "productid" }), input06 == "0" ? "bsd_plotnumber" : "bsd_floor", item);
                        foreach (Entity fl in units)
                        {
                            listUnit.Add(fl.Id.ToString());
                        }
                    }
                }
                else if (input03 != "")// có danh sách block
                {
                    List<string> listBlock = new List<string>();
                    listBlock = input03.Split(';').ToList();
                    listBlock.RemoveAll(string.IsNullOrEmpty);
                    foreach (string item in listBlock)
                    {
                        List<Entity> units = RetrieveMultiRecord2(service, "product", new ColumnSet(new string[] { "productid" }), input06 == "0" ? "bsd_landlot" : "bsd_blocknumber", item);
                        foreach (Entity fl in units)
                        {
                            listUnit.Add(fl.Id.ToString());
                        }
                    }
                }
                context.OutputParameters["output05"] = string.Join(";", listUnit);
                //TracingSe.Trace("output05: " + string.Join(";", listUnit));
            }
            else if (input01 == "Bước 03" && input02 != "" && input03 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity pl = service.Retrieve("bsd_phaseslaunch", Guid.Parse(input02), new ColumnSet(new string[] { "bsd_phaseslaunchid", "bsd_pricelistid", "bsd_projectid" }));
                Entity fl = service.Retrieve("product", Guid.Parse(input03), new ColumnSet(new string[] { "bsd_blocknumber", "bsd_floor", "name", "productid", "bsd_landlot", "bsd_plotnumber" }));
                Entity up = new Entity("bsd_unit_preparing");
                up["bsd_name"] = fl["name"];
                up["bsd_phareslaunch"] = pl.ToEntityReference();
                up["bsd_pricelist"] = pl["bsd_pricelistid"];
                up["bsd_project"] = pl["bsd_projectid"];
                up["bsd_block"] = pl.Contains("bsd_blocknumber") ? pl["bsd_blocknumber"] : (pl.Contains("bsd_landlot") ? pl["bsd_landlot"] : null);
                up["bsd_floor"] = pl.Contains("bsd_floor") ? pl["bsd_floor"] : (pl.Contains("bsd_plotnumber") ? pl["bsd_plotnumber"] : null);
                up["bsd_unit"] = fl.ToEntityReference();
                string Xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                            <entity name='productpricelevel'>
                                            <attribute name='productpricelevelid' />
                                            <attribute name='productid' />
                                            <attribute name='amount' />
                                            <filter type='and'>
                                            <condition attribute='pricelevelid' operator='eq' value='{0}' />
                                            <condition attribute='productid' operator='eq' value='{1}' />
                                            <condition attribute='amount' operator='not-null' />
                                            </filter>
                                            </entity>
                                            </fetch>";
                Xml = string.Format(Xml, ((EntityReference)pl["bsd_pricelistid"]).Id, input03);
                EntityCollection pricelistitems = service.RetrieveMultiple(new FetchExpression(Xml));
                foreach (Entity pi in pricelistitems.Entities)
                {
                    up["bsd_price"] = pi["amount"];
                }
                service.Create(up);
            }
            else if (input01 == "Bước 04" && input02 != "" && input04 != "")
            {
                TracingSe.Trace("Bước 04");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enPhasesLaunch = new Entity("bsd_phaseslaunch");
                enPhasesLaunch.Id = Guid.Parse(input02);
                enPhasesLaunch["bsd_powerautomate"] = false;
                service.Update(enPhasesLaunch);
            }
        }
        List<Entity> RetrieveMultiRecord2(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            EntityCollection entc = service.RetrieveMultiple(q);
            return entc.Entities.ToList<Entity>();
        }
        EntityCollection RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            q.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            EntityCollection entc = service.RetrieveMultiple(q);

            return entc;
        }
    }
}
