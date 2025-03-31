using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_InterestSimulation_Option
{
    public class Action_InterestSimulation_Option : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;
        
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            EntityReference enRef = context.InputParameters["Target"] as EntityReference;
            
            QueryExpression queryExpression1 = new QueryExpression("bsd_bsd_interestsimulation_product");
            queryExpression1.ColumnSet = new ColumnSet(new string[] { "productid" });
            queryExpression1.Criteria = new FilterExpression(LogicalOperator.And);
            queryExpression1.Criteria.AddCondition(new ConditionExpression("bsd_interestsimulationid", ConditionOperator.Equal, enRef.Id));
            EntityCollection listUnit = service.RetrieveMultiple(queryExpression1);

            clearAgingInterestSimulationOption(service, enRef.Id.ToString());
            if (listUnit.Entities.Count > 0)
            {
                EntityCollection optionEntrys = getOptionEntrys(service, listUnit);
                traceService.Trace("Count Unit:" + listUnit.Entities.Count.ToString());
                traceService.Trace("Count OptionEntry:" + optionEntrys.Entities.Count.ToString());
                int count = 0;
                var countrecord = optionEntrys.Entities.Count;
                var multipleRequest = new ExecuteMultipleRequest()
                {
                    Settings = new ExecuteMultipleSettings()
                    {
                        ContinueOnError = false,
                        ReturnResponses = true
                    },
                    Requests = new OrganizationRequestCollection()
                };
                CreateRequest createRequest = null;
                foreach (Entity optionEntry in optionEntrys.Entities)
                {
                    //throw new Exception(enRef.Id.ToString() + "---"+ optionEntry.Id.ToString());
                    Entity agInSimOption = new Entity("bsd_aginginterestsimulationoption");
                    agInSimOption["bsd_name"] = optionEntry.Attributes["name"].ToString();
                    EntityReference aginginterestsimulation = new EntityReference(enRef.LogicalName, enRef.Id);
                    EntityReference optionentry = new EntityReference(optionEntry.LogicalName, optionEntry.Id);
                    agInSimOption["bsd_aginginterestsimulation"] = aginginterestsimulation;
                    agInSimOption["bsd_optionentry"] = optionentry;
                    //Guid createdId = service.Create(agInSimOption);
                    //traceService.Trace("Item: " + count++);
                    createRequest = new CreateRequest { Target = agInSimOption };
                    multipleRequest.Requests.Add(createRequest);
                    count += 1;
                    countrecord -= 1;
                    if ((count == 1000) || (count < 1000 && countrecord == 0))
                    {
                        ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse)service.Execute(multipleRequest);
                        multipleRequest.Requests.Clear();
                        count = 0;
                    }
                }
            }
        }
        private void clearAgingInterestSimulationOption(IOrganizationService crmservices, string agingInterestSimulationId)
        {
            string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_aginginterestsimulationoption'>
                    <attribute name='bsd_name' />
                   
                    <filter type='and'>
                      <condition attribute='bsd_aginginterestsimulation' operator='eq' value='" + agingInterestSimulationId + @"' />
                    </filter>
                    
                  </entity>
                </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            int count = 0;
            var countrecord = entc.Entities.Count;

            var multipleRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };
            DeleteRequest deleteRequest = null;

            foreach (Entity entity in entc.Entities)
            {
                EntityReference entityRef = new EntityReference(entity.LogicalName, entity.Id);
                deleteRequest = new DeleteRequest { Target = entityRef };
                multipleRequest.Requests.Add(deleteRequest);
                count += 1;
                countrecord -= 1;
                if ((count == 1000) || (count < 1000 && countrecord == 0))
                {
                    ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse)service.Execute(multipleRequest);
                    multipleRequest.Requests.Clear();
                    count = 0;
                }
            }
            //foreach (Entity option in entc.Entities)
            //{
            //    crmservices.Delete(option.LogicalName, option.Id);
            //}
        }
        private EntityCollection getOptionEntrys(IOrganizationService crmservices, EntityCollection listUnit)
        {
            string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='salesorder'>
                    <attribute name='name' />
                    <attribute name='totalamount' />
                    <attribute name='statuscode' />
                    <attribute name='customerid' />
                    <attribute name='createdon' />
                    <attribute name='bsd_unitnumber' />
                    <attribute name='bsd_project' />
                    <attribute name='bsd_optionno' />
                    <attribute name='bsd_contractnumber' />
                    <attribute name='bsd_optioncodesams' />
                    <attribute name='salesorderid' />
                    <order attribute='createdon' descending='true' />
                    <order attribute='customerid' descending='true' />
                    <filter type='and'>
                      <condition attribute='statuscode' operator='ne' value='100000006' />
                      <condition attribute='statuscode' operator='ne' value='100000007' />
                      
                        {0}
                      
                    </filter>
                    
                  </entity>
                </fetch>";
            string str = "";
            if (listUnit.Entities.Count > 0)
            {
                str = "<condition attribute='bsd_unitnumber' operator='in'>";
                foreach (Entity unit in listUnit.Entities)
                {
                    str += "<value>" + unit["productid"] + "</value>";
                }
                str += "</condition>";

            }

            fetchXml = string.Format(fetchXml, str);
            traceService.Trace(fetchXml);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));

            return entc;
        }
    }
}
