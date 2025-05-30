using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugin_SubSale_Update_Coowner
{
    public class Plugin_SubSale_Update_Coowner : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity target = context.InputParameters["Target"] as Entity;
            Entity SubSale = service.Retrieve("bsd_assign", target.Id, new ColumnSet(true));
            int statuscode = SubSale.Contains("statuscode") ? ((OptionSetValue)SubSale["statuscode"]).Value : 0;
            if (context.MessageName == "Create")
            {
                Entity OE = new Entity(((EntityReference)SubSale["bsd_optionentry"]).LogicalName);
                //optionrntry của OE bằng với optionentry của ss tương ứng
                OE.Id = ((EntityReference)SubSale["bsd_optionentry"]).Id;

                Entity optionEntry = service.Retrieve(OE.LogicalName, OE.Id, new ColumnSet(true));
                EntityReference pro = (EntityReference)optionEntry["bsd_project"];
                EntityReference unit = (EntityReference)optionEntry["bsd_unitnumber"];

                EntityCollection l_co_owner_OE = findCo_owner_OE(service, OE.ToEntityReference());
                EntityCollection l_co_owner_Subsale = findCo_owner_subsale(service, target.Id);
                foreach (Entity co in l_co_owner_OE.Entities)
                {
                    Entity co_owner = new Entity(co.LogicalName);
                    co_owner.Id = co.Id;
                    //bsd_subsale của co-owner bằng với ss hiện tại
                    co_owner["bsd_subsale"] = new EntityReference("bsd_assign", target.Id);
                    co_owner["bsd_project"] = pro;
                    co_owner["bsd_units"] = unit;
                    co_owner["bsd_current"] = true;

                    service.Update(co_owner);
                }
                foreach (Entity co in l_co_owner_Subsale.Entities)
                {
                    Entity co_owner = new Entity(co.LogicalName);
                    co_owner.Id = co.Id;
                    service.Update(co_owner);
                }
            }
            else if (context.MessageName == "Update" && statuscode == 100000001)
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_advancepayment"">
                    <attribute name=""bsd_customer"" />
                    <filter>
                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{((EntityReference)SubSale["bsd_optionentry"]).Id}"" />
                      <condition attribute=""statuscode"" operator=""in"">
                        <value>1</value>
                        <value>100000003</value>
                      </condition>
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (entc.Entities.Count > 0) throw new InvalidPluginExecutionException("The contract has advance payment in 'Active' or 'Pending Revert' status, so the transfer cannot be performed.");
                if (SubSale.Contains("bsd_currentcustomer") && SubSale.Contains("bsd_newcustomer"))
                {
                    fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
                      <entity name=""bsd_advancepayment"">
                        <attribute name=""bsd_customer"" />
                        <filter>
                          <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{((EntityReference)SubSale["bsd_optionentry"]).Id}"" />
                          <condition attribute=""bsd_customer"" operator=""eq"" value=""{((EntityReference)SubSale["bsd_currentcustomer"]).Id}"" />
                          <condition attribute=""statuscode"" operator=""in"">
                            <value>100000000</value>
                          </condition>
                        </filter>
                      </entity>
                    </fetch>";
                    entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity ent in entc.Entities)
                    {
                        Entity enUP = new Entity(ent.LogicalName, ent.Id);
                        enUP["bsd_customer"] = (EntityReference)SubSale["bsd_newcustomer"];
                        service.Update(enUP);
                    }
                }
                EntityCollection l_co_owner_Subsale = findCo_owner_subsale2(service, target.Id);
                int dem = 0;
                foreach (Entity co in l_co_owner_Subsale.Entities)
                {
                    dem++;
                    Entity co_owner = new Entity(co.LogicalName);
                    co_owner.Id = co.Id;
                    co_owner["bsd_stt"] = dem;
                    service.Update(co_owner);
                }
            }
        }
        private EntityCollection findCo_owner_subsale2(IOrganizationService crmservices, Guid id)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_coowner'>
                    <attribute name='bsd_coownerid' />
                    <attribute name='bsd_name' />
                    <filter type='and'>
                      <condition attribute='bsd_subsale' operator='eq'  uitype='bsd_assign' value='{0}' />
                      <condition attribute='statecode' operator='eq' value='0' />
                      <condition attribute='bsd_current' operator='eq' value='0' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findCo_owner_subsale(IOrganizationService crmservices, Guid id)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_coowner'>
                    <attribute name='bsd_coownerid' />
                    <attribute name='bsd_name' />
                    <filter type='and'>
                      <condition attribute='bsd_subsale' operator='eq'  uitype='bsd_assign' value='{0}' />
                      <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findCo_owner_OE(IOrganizationService crmservices, EntityReference oe)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_coowner'>
                    <attribute name='bsd_coownerid' />
                    <attribute name='bsd_name' />
                    <filter type='and'>
                      <condition attribute='bsd_optionentry' operator='eq'  uitype='salesorder' value='{0}' />
                      <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
    }
}
