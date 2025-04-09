using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace Action_AgingSimulation_GenerateUnit
{
    public class Action_AgingSimulation_GenerateUnit : IPlugin
    {
        public static IOrganizationService service = null;
        static IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        static StringBuilder strMess = new StringBuilder();
        static StringBuilder strMess2 = new StringBuilder();
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
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
            if (input01 == "Bước 01" && input02 != "")
            {
                traceService.Trace("Bước 01");
                Entity enTarget = new Entity("bsd_interestsimulation");
                enTarget.Id = Guid.Parse(input02);
                enTarget["bsd_powerautomate"] = true;
                service.Update(enTarget);
                context.OutputParameters["output01"] = context.UserId.ToString();
                string url = "";
                EntityCollection configGolive = RetrieveMultiRecord(service, "bsd_configgolive",
                    new ColumnSet(new string[] { "bsd_url" }), "bsd_name", "Aging Simulation Generate Unit");
                foreach (Entity item in configGolive.Entities)
                {
                    if (item.Contains("bsd_url")) url = (string)item["bsd_url"];
                }
                if (url == "") throw new InvalidPluginExecutionException("Link to run PA not found. Please check again.");
                context.OutputParameters["output02"] = url;
                Entity enInterestsimulation = service.Retrieve(enTarget.LogicalName, enTarget.Id, new ColumnSet(new string[4]
                  {
                    "bsd_project",
                    "bsd_block",
                    "bsd_floor",
                    "bsd_floorto"
                  }));
                if (!enInterestsimulation.Contains("bsd_project"))
                    throw new InvalidPluginExecutionException("Please input Project!");
                EntityReference block = enInterestsimulation.Contains("bsd_block") ? (EntityReference)enInterestsimulation["bsd_block"] : null;
                EntityReference floor = enInterestsimulation.Contains("bsd_floor") ? (EntityReference)enInterestsimulation["bsd_floor"] : null;
                EntityReference floorto = enInterestsimulation.Contains("bsd_floorto") ? (EntityReference)enInterestsimulation["bsd_floorto"] : null;
                EntityReference project = (EntityReference)enInterestsimulation["bsd_project"];
                EntityCollection paymentScheme = getPaymentScheme(enTarget.Id);
                EntityCollection unit = findUnit(project, block, floor, floorto, paymentScheme);
                List<string> listUnit = new List<string>();
                foreach (Entity item in unit.Entities)
                {
                    listUnit.Add(item.Id.ToString());
                }
                traceService.Trace("paymentScheme " + paymentScheme.Entities.Count);
                traceService.Trace("output03 " + string.Join(";", listUnit));
                context.OutputParameters["output03"] = string.Join(";", listUnit);
            }
            else if (input01 == "Bước 02" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 02");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                EntityReference master = new EntityReference("bsd_interestsimulation", Guid.Parse(input02));
                EntityReferenceCollection referenceCollection2 = new EntityReferenceCollection();
                referenceCollection2.Add(new EntityReference("product", Guid.Parse(input03)));
                service.Disassociate(master.LogicalName, master.Id, new Relationship("bsd_bsd_interestsimulation_product"), referenceCollection2);
            }
            else if (input01 == "Bước 03" && input02 != "" && input03 != "" && input04 != "")
            {
                traceService.Trace("Bước 03");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                EntityReference master = new EntityReference("bsd_interestsimulation", Guid.Parse(input02));
                var fetchXml = $@"
                            <fetch>
                              <entity name='salesorder'>
                                <attribute name='name' />
                                <attribute name='salesorderid' />
                                <attribute name='bsd_unitnumber' />
                                <filter type='and'>
                                  <condition attribute='bsd_unitnumber' operator='eq' value='{input03}'/>
                                  <condition attribute='statuscode' operator='ne' value='100000006'/>
                                  <condition attribute='statuscode' operator='ne' value='100000007'/>
                                </filter>
                              </entity>
                            </fetch>";
                EntityCollection enOE = service.RetrieveMultiple(new FetchExpression(fetchXml));
                foreach (Entity item in enOE.Entities)
                {
                    Entity enCreate = new Entity("bsd_aginginterestsimulationoption");
                    enCreate["bsd_name"] = item["name"];
                    enCreate["bsd_aginginterestsimulation"] = master;
                    enCreate["bsd_optionentry"] = item.ToEntityReference();
                    service.Create(enCreate);
                    EntityReferenceCollection referenceCollection2 = new EntityReferenceCollection();
                    referenceCollection2.Add((EntityReference)item["bsd_unitnumber"]);
                    service.Associate(master.LogicalName, master.Id, new Relationship("bsd_bsd_interestsimulation_product"), referenceCollection2);
                }
            }
            else if (input01 == "Bước 04" && input02 != "" && input04 != "")
            {
                traceService.Trace("Bước 04");
                service = factory.CreateOrganizationService(Guid.Parse(input04));
                Entity enConfirmPayment = new Entity("bsd_interestsimulation");
                enConfirmPayment.Id = Guid.Parse(input02);
                enConfirmPayment["bsd_powerautomate"] = false;
                enConfirmPayment["bsd_errorincalculation"] = "";
                service.Update(enConfirmPayment);
            }
        }
        private EntityCollection findUnit(EntityReference project, EntityReference block, EntityReference floor, EntityReference floorto, EntityCollection paymentSchemes)
        {
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical'>");
            xml.AppendLine("<entity name='product'>");
            xml.AppendLine("<attribute name='productid' />");
            xml.AppendLine("<attribute name='statuscode' />");
            xml.AppendLine("<filter type='and'>");
            xml.AppendLine(string.Format("<condition attribute='bsd_projectcode' operator='eq' value='{0}'/>", project.Id));
            xml.AppendLine("<condition attribute='statuscode' operator='in'>");
            xml.AppendLine("<value>100000001</value>");
            xml.AppendLine("<value>100000002</value>");
            xml.AppendLine("</condition>");
            if (block != null && floor != null && floorto == null)
            {
                xml.AppendLine(string.Format("<condition attribute='bsd_blocknumber' operator='eq' value='{0}'/>", block.Id));
                xml.AppendLine(string.Format("<condition attribute='bsd_floor' operator='eq' value='{0}'/>", floor.Id));
            }
            else if (block != null && floor != null && floorto != null)
            {
                int floorNumber1 = toFloorNumber(((DataCollection<string, object>)service.Retrieve(floor.LogicalName, floor.Id, new ColumnSet(true)).Attributes)["bsd_floor"].ToString());
                int floorNumber2 = toFloorNumber(((DataCollection<string, object>)service.Retrieve(floorto.LogicalName, floorto.Id, new ColumnSet(true)).Attributes)["bsd_floor"].ToString());
                EntityCollection floor1 = getFloor(project, block, floorNumber1, floorNumber2);
                xml.AppendLine(string.Format("<condition attribute='bsd_blocknumber' operator='eq' value='{0}'/>", block.Id));
                if (floor1.Entities.Count > 0)
                {
                    xml.AppendLine("<filter type='or'>");
                    foreach (Entity item in floor1.Entities)
                    {
                        xml.AppendLine(string.Format("<condition attribute='bsd_floor' operator='eq' value='{0}'/>", item.Id));
                    }
                    xml.AppendLine("</filter>");
                }
            }
            else if (block != null)
            {
                xml.AppendLine(string.Format("<condition attribute='bsd_blocknumber' operator='eq' value='{0}'/>", block.Id));
            }
            else if (floor != null)
            {
                xml.AppendLine(string.Format("<condition attribute='bsd_floor' operator='eq' value='{0}'/>", floor.Id));
            }
            xml.AppendLine("</filter>");
            xml.AppendLine("</entity>");
            xml.AppendLine("</fetch>");
            EntityCollection unit1 = service.RetrieveMultiple(new FetchExpression(xml.ToString()));
            traceService.Trace("unit1 " + unit1.Entities.Count);
            if (paymentSchemes.Entities.Count == 0) return unit1;
            StringBuilder xml2 = new StringBuilder();
            xml2.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical'>");
            xml2.AppendLine("<entity name='salesorder'>");
            xml2.AppendLine("<attribute name='name' />");
            xml2.AppendLine("<attribute name='customerid' />");
            xml2.AppendLine("<attribute name='statuscode' />");
            xml2.AppendLine("<attribute name='totalamount' />");
            xml2.AppendLine("<attribute name='bsd_unitnumber' />");
            xml2.AppendLine("<attribute name='bsd_project' />");
            xml2.AppendLine("<attribute name='bsd_optionno' />");
            xml2.AppendLine("<attribute name='createdon' />");
            xml2.AppendLine("<attribute name='bsd_optioncodesams' />");
            xml2.AppendLine("<attribute name='bsd_contractnumber' />");
            xml2.AppendLine("<attribute name='salesorderid' />");
            xml2.AppendLine("<filter type='and'>");
            xml2.AppendLine(string.Format("<condition attribute='bsd_project' operator='eq' value='{0}'/>", project.Id));
            xml2.AppendLine("<condition attribute='statuscode' operator='ne' value='100000006'/>");
            xml2.AppendLine("<condition attribute='bsd_paymentscheme' operator='in'>");
            foreach (Entity entity in paymentSchemes.Entities)
                xml2.AppendLine(string.Format("<value>{0}</value>", (Guid)entity["bsd_paymentschemeid"]));
            xml2.AppendLine("</condition>");
            xml2.AppendLine("</filter>");
            xml2.AppendLine("</entity>");
            xml2.AppendLine("</fetch>");
            traceService.Trace(xml2.ToString());
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(xml2.ToString()));
            EntityCollection unit2 = new EntityCollection();
            traceService.Trace("entityCollection " + entityCollection.Entities.Count);
            foreach (Entity entity1 in entityCollection.Entities)
            {
                foreach (Entity entity2 in unit1.Entities)
                {
                    //traceService.Trace("bsd_unitnumber " + ((EntityReference)entity1["bsd_unitnumber"]).Id);
                    //traceService.Trace("entity2.Id " + entity2.Id);
                    if (((EntityReference)entity1["bsd_unitnumber"]).Id == entity2.Id)
                        unit2.Entities.Add(entity2);
                }
            }
            return unit2;
        }
        private EntityCollection getFloor(EntityReference project, EntityReference block, int from, int to)
        {
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical'>");
            xml.AppendLine("<entity name='bsd_floor'>");
            xml.AppendLine("<attribute name='bsd_floor' />");
            xml.AppendLine("<filter type='and'>");
            xml.AppendLine(string.Format("<condition attribute='bsd_project' operator='eq' value='{0}'/>", project.Id)); ;
            xml.AppendLine(string.Format("<condition attribute='bsd_block' operator='eq' value='{0}'/>", block.Id)); ;
            xml.AppendLine("</filter>");
            xml.AppendLine("</entity>");
            xml.AppendLine("</fetch>");
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(xml.ToString()));
            EntityCollection floor = new EntityCollection();
            foreach (Entity entity in (Collection<Entity>)entityCollection.Entities)
            {
                int floorNumber = toFloorNumber(((DataCollection<string, object>)entity.Attributes)["bsd_floor"].ToString());
                if (floorNumber >= from && floorNumber <= to)
                    floor.Entities.Add(entity);
            }
            return floor;
        }
        private int toFloorNumber(string floor)
        {
            string upper = floor.ToUpper();
            byte[] bytes = Encoding.ASCII.GetBytes(upper);
            string str1 = "";
            string str2 = "";
            for (int index = 0; index < upper.Length; ++index)
            {
                if (bytes[index] >= (byte)48 && bytes[index] <= (byte)57)
                    str1 += upper[index].ToString();
                else
                    str2 += upper[index].ToString();
            }
            switch (Convert.ToInt32(str1).ToString() + str2)
            {
                case "3A":
                    return 4;
                case "12A":
                    return 13;
                case "12B":
                    return 14;
                default:
                    return Convert.ToInt32(str1);
            }
        }
        private EntityCollection getPaymentScheme(Guid aisid)
        {
            QueryExpression queryExpression = new QueryExpression("bsd_bsd_interestsimulation_bsd_paymentschem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria = new FilterExpression(LogicalOperator.And);
            queryExpression.Criteria.AddCondition(new ConditionExpression("bsd_interestsimulationid", ConditionOperator.Equal, aisid));
            return service.RetrieveMultiple((QueryBase)queryExpression);
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
