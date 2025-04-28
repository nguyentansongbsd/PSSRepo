using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_PhasesLaunch_GenerateUnit
{
    public class Action_PhasesLaunch_GenerateUnit : IPlugin
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
                Entity pl = service.Retrieve("bsd_phaseslaunch", target.Id, new ColumnSet(true));
                Guid id = ((EntityReference)pl["bsd_projectid"]).Id;
                if (pl.Attributes.Contains("bsd_fromblock") == false && pl.Attributes.Contains("bsd_toblock") == false)
                {
                    throw new InvalidPluginExecutionException("Please insert value for the filter!!!");
                }
                else
                {
                    List<Entity> pricelistitems = RetrieveMultiRecord(service, "productpricelevel", new ColumnSet(true), "pricelevelid", ((EntityReference)pl["bsd_pricelistid"]).Id);
                    List<Entity> unitprepare = RetrieveMultiRecord(service, "bsd_unit_preparing", new ColumnSet(true), "bsd_phareslaunch", target.Id);
                    if (unitprepare.Count > 0)
                    {
                        foreach (Entity up in unitprepare)
                        {
                            service.Delete("bsd_unit_preparing", up.Id);
                        }
                    }

                    Entity block = new Entity();
                    int s = 0;
                    int e = 0;
                    if (pl.Attributes.Contains("bsd_fromblock") && pl.Attributes.Contains("bsd_toblock"))
                    {
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
                                block = blocks.Entities[i];

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
                        if (pl.Attributes.Contains("bsd_fromfloor") && pl.Attributes.Contains("bsd_tofloor"))
                        {
                            Entity floor = new Entity();
                            int sf = 0;
                            int ef = 0;
                            for (int i = s; i < e + 1; i++)
                            {
                                block = blocks.Entities[i];
                                Guid ID = block.Id;
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

                                    List<Entity> units = RetrieveMultiRecord(service, "product", new ColumnSet(new string[] { "bsd_blocknumber", "bsd_floor", "name" }), "bsd_floor", floor.Id);

                                    foreach (Entity fl in units)
                                    {
                                        Entity up = new Entity("bsd_unit_preparing");
                                        up["bsd_name"] = fl["name"];
                                        up["bsd_phareslaunch"] = pl.ToEntityReference();
                                        up["bsd_pricelist"] = pl["bsd_pricelistid"];
                                        up["bsd_project"] = pl["bsd_projectid"];
                                        up["bsd_block"] = fl["bsd_blocknumber"];
                                        up["bsd_floor"] = fl["bsd_floor"];
                                        up["bsd_unit"] = fl.ToEntityReference();
                                        foreach (Entity pi in pricelistitems)
                                        {
                                            if (((EntityReference)pi["productid"]).Id == fl.Id)
                                            {
                                                if (pi.Contains("amount"))
                                                {
                                                    up["bsd_price"] = pi["amount"];
                                                }
                                            }
                                        }
                                        service.Create(up);
                                    }
                                }
                            }
                        }
                        if (pl.Attributes.Contains("bsd_fromfloor") == false && pl.Attributes.Contains("bsd_tofloor") == false)
                        {
                            for (int i = s; i < e + 1; i++)
                            {
                                block = blocks.Entities[i];

                                List<Entity> units = RetrieveMultiRecord(service, "product", new ColumnSet(new string[] { "bsd_blocknumber", "bsd_floor", "name" }), "bsd_blocknumber", block.Id);

                                foreach (Entity en in units)
                                {
                                    Entity up = new Entity("bsd_unit_preparing");
                                    up["bsd_name"] = en["name"];
                                    up["bsd_phareslaunch"] = pl.ToEntityReference();
                                    up["bsd_project"] = pl["bsd_projectid"];
                                    up["bsd_block"] = en["bsd_blocknumber"];
                                    up["bsd_floor"] = en["bsd_floor"];
                                    up["bsd_unit"] = en.ToEntityReference();
                                    foreach (Entity pi in pricelistitems)
                                    {
                                        if (((EntityReference)pi["productid"]).Id == en.Id)
                                        {
                                            if (((OptionSetValue)pi["pricingmethodcode"]).Value != 1)
                                                throw new InvalidPluginExecutionException("This unit (" + en["name"].ToString() + ") is attached with a wrong pricing method. Please re-pick.(Price list item - Currency Amount");
                                            up["bsd_price"] = pi["amount"];
                                            //throw new Exception(up.LogicalName.ToString() + (pi["amount"]).ToString());
                                        }
                                    }
                                    service.Create(up);
                                }
                            }
                        }
                        if ((pl.Attributes.Contains("bsd_fromfloor") && pl.Attributes.Contains("bsd_tofloor") == false) || (pl.Attributes.Contains("bsd_fromfloor") == false && pl.Attributes.Contains("bsd_tofloor")))
                        {
                            if (pl.Attributes.Contains("bsd_fromfloor") && pl.Attributes.Contains("bsd_tofloor") == false)
                            {
                                Entity floor = new Entity();
                                int sf = 0;
                                for (int i = s; i < e + 1; i++)
                                {
                                    block = blocks.Entities[i];
                                    Guid ID = block.Id;
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

                                        List<Entity> units = RetrieveMultiRecord(service, "product", new ColumnSet(new string[] { "bsd_blocknumber", "bsd_floor", "name" }), "bsd_floor", floor.Id);

                                        foreach (Entity fl in units)
                                        {
                                            Entity up = new Entity("bsd_unit_preparing");
                                            up["bsd_name"] = fl["name"];
                                            up["bsd_phareslaunch"] = pl.ToEntityReference();
                                            up["bsd_pricelist"] = pl["bsd_pricelistid"];
                                            up["bsd_project"] = pl["bsd_projectid"];
                                            up["bsd_block"] = fl["bsd_blocknumber"];
                                            up["bsd_floor"] = fl["bsd_floor"];
                                            up["bsd_unit"] = fl.ToEntityReference();
                                            foreach (Entity pi in pricelistitems)
                                            {
                                                if (((EntityReference)pi["productid"]).Id == fl.Id)
                                                {
                                                    up["bsd_price"] = pi["amount"];
                                                }
                                            }
                                            service.Create(up);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Entity floor = new Entity();

                                int ef = 0;
                                for (int i = s; i < e + 1; i++)
                                {
                                    block = blocks.Entities[i];
                                    Guid ID = block.Id;
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

                                        List<Entity> units = RetrieveMultiRecord(service, "product", new ColumnSet(new string[] { "bsd_blocknumber", "bsd_floor", "name" }), "bsd_floor", floor.Id);

                                        foreach (Entity fl in units)
                                        {
                                            Entity up = new Entity("bsd_unit_preparing");
                                            up["bsd_name"] = fl["name"];
                                            up["bsd_phareslaunch"] = pl.ToEntityReference();
                                            up["bsd_pricelist"] = pl["bsd_pricelistid"];
                                            up["bsd_project"] = pl["bsd_projectid"];
                                            up["bsd_block"] = fl["bsd_blocknumber"];
                                            up["bsd_floor"] = fl["bsd_floor"];
                                            up["bsd_unit"] = fl.ToEntityReference();
                                            foreach (Entity pi in pricelistitems)
                                            {
                                                if (((EntityReference)pi["productid"]).Id == fl.Id)
                                                {
                                                    up["bsd_price"] = pi["amount"];
                                                }
                                            }
                                            service.Create(up);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        List<Entity> RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            EntityCollection entc = service.RetrieveMultiple(q);
            return entc.Entities.ToList<Entity>();
        }
    }
}
