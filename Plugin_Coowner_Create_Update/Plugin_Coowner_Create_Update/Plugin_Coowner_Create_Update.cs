using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugin_Coowner_Create_Update
{
    public class Plugin_Coowner_Create_Update : IPlugin
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

            if (context.Depth > 2) return;
            if (context.MessageName == "Create")
            {
                traceService.Trace("Create");
                Entity target = context.InputParameters["Target"] as Entity;
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                if (enTarget.Contains("bsd_subsale"))
                {
                    traceService.Trace("bsd_subsale");
                    EntityCollection l_co_owner_Subsale = findCo_owner_subsale(service, ((EntityReference)enTarget["bsd_subsale"]).Id);
                    int dem = 2; 
                    traceService.Trace("count " + l_co_owner_Subsale.Entities.Count);
                    foreach (Entity co in l_co_owner_Subsale.Entities)
                    {
                        Entity co_owner = new Entity(co.LogicalName);
                        co_owner.Id = co.Id;
                        co_owner["bsd_stt"] = dem;
                        dem++;
                        service.Update(co_owner);
                    }
                }
                traceService.Trace("1111");
                Entity enUp = new Entity(enTarget.LogicalName, enTarget.Id);
                if (enTarget.Contains("bsd_customer"))
                {
                    EntityReference erf = (EntityReference)enTarget["bsd_customer"];
                    if (erf.LogicalName == "contact") setInforContact_Customer(erf.Id, erf.LogicalName, enUp);
                    else setInforAccount_Customer(erf.Id, erf.LogicalName, enUp);
                }
                service.Update(enUp);
            }
            else if (context.MessageName == "Update")
            {
                Entity target = context.InputParameters["Target"] as Entity;
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                if (enTarget.Contains("bsd_subsale"))
                {
                    EntityCollection l_co_owner_Subsale = findCo_owner_subsale(service, ((EntityReference)enTarget["bsd_subsale"]).Id);
                    int dem = 2;
                    foreach (Entity co in l_co_owner_Subsale.Entities)
                    {
                        Entity co_owner = new Entity(co.LogicalName);
                        co_owner.Id = co.Id;
                        co_owner["bsd_stt"] = dem;
                        dem++;
                        service.Update(co_owner);
                    }
                    EntityCollection l_co_owner_Subsale2 = findCo_owner_subsale2(service, ((EntityReference)enTarget["bsd_subsale"]).Id);
                    int dem2 = 2;
                    foreach (Entity co in l_co_owner_Subsale2.Entities)
                    {
                        Entity co_owner = new Entity(co.LogicalName);
                        co_owner.Id = co.Id;
                        co_owner["bsd_stt"] = dem2;
                        dem2++;
                        service.Update(co_owner);
                    }
                }
                if (enTarget.Contains("bsd_customer"))
                {
                    Entity enUp = new Entity(enTarget.LogicalName, enTarget.Id);
                    EntityReference erf = (EntityReference)enTarget["bsd_customer"];
                    if (erf.LogicalName == "contact") setInforContact_Customer(erf.Id, erf.LogicalName, enUp);
                    else setInforAccount_Customer(erf.Id, erf.LogicalName, enUp);
                    service.Update(enUp);
                }
            }
            else if (context.MessageName == "Delete")
            {
                Entity enPre = context.PreEntityImages["pre"];
                if (enPre.Contains("bsd_subsale"))
                {
                    EntityCollection l_co_owner_Subsale = findCo_owner_subsale(service, ((EntityReference)enPre["bsd_subsale"]).Id);
                    int dem = 2;
                    foreach (Entity co in l_co_owner_Subsale.Entities)
                    {
                        Entity co_owner = new Entity(co.LogicalName);
                        co_owner.Id = co.Id;
                        co_owner["bsd_stt"] = dem;
                        dem++;
                        service.Update(co_owner);
                    }
                }
            }
        }
        private void setInforContact_Customer(Guid id, string name, Entity enUp)
        {
            Entity en = service.Retrieve(name, id, new ColumnSet(true));
            if (en.Contains("bsd_fullname")) enUp["bsd_fullname"] = (string)en["bsd_fullname"];
            else enUp["bsd_fullname"] = "";
            if (en.Contains("bsd_identitycardnumber")) enUp["bsd_identitycardnumber"] = (string)en["bsd_identitycardnumber"];
            else enUp["bsd_identitycardnumber"] = "";
            if (en.Contains("bsd_localization")) enUp["bsd_locality"] = en.FormattedValues["bsd_localization"].ToString();
            else enUp["bsd_locality"] = "";
            if (en.Contains("bsd_dategrant")) enUp["bsd_dategrant2"] = ((DateTime)en["bsd_dategrant"]).Day + "/" + ((DateTime)en["bsd_dategrant"]).Month + "/" + ((DateTime)en["bsd_dategrant"]).Year;
            else enUp["bsd_dategrant2"] = "";
            if (en.Contains("bsd_contactaddress")) enUp["bsd_contactaddress"] = (string)en["bsd_contactaddress"];
            else enUp["bsd_contactaddress"] = "";
            if (en.Contains("bsd_permanentaddress1")) enUp["bsd_permanentaddress1"] = (string)en["bsd_permanentaddress1"];
            else enUp["bsd_permanentaddress1"] = "";
            if (en.Contains("mobilephone")) enUp["bsd_mobilephone"] = (string)en["mobilephone"];
            else enUp["bsd_mobilephone"] = "";
            if (en.Contains("emailaddress1")) enUp["bsd_email"] = (string)en["emailaddress1"];
            else enUp["bsd_email"] = "";
        }
        private void setInforAccount_Customer(Guid id, string name, Entity enUp)
        {
            Entity en = service.Retrieve(name, id, new ColumnSet(true));
            //traceService.Trace("1111");
            if (en.Contains("bsd_name")) enUp["bsd_fullname"] = (string)en["bsd_name"];
            else enUp["bsd_fullname"] = "";
            //traceService.Trace("1111");
            if (en.Contains("bsd_registrationcode")) enUp["bsd_identitycardnumber"] = (string)en["bsd_registrationcode"];
            else enUp["bsd_identitycardnumber"] = "";
            //traceService.Trace("1111");
            if (en.Contains("bsd_localization")) enUp["bsd_locality"] = en.FormattedValues["bsd_localization"].ToString();
            else enUp["bsd_locality"] = "";
            //traceService.Trace("1111");
            if (en.Contains("bsd_issuedon")) enUp["bsd_dategrant2"] = ((DateTime)en["bsd_issuedon"]).Day + "/" + ((DateTime)en["bsd_issuedon"]).Month + "/" + ((DateTime)en["bsd_issuedon"]).Year;
            else enUp["bsd_dategrant2"] = "";
            //traceService.Trace("1111");
            if (en.Contains("bsd_address")) enUp["bsd_contactaddress"] = (string)en["bsd_address"];
            else enUp["bsd_contactaddress"] = "";
            //traceService.Trace("1111");
            if (en.Contains("bsd_permanentaddress1")) enUp["bsd_permanentaddress1"] = (string)en["bsd_permanentaddress1"];
            else enUp["bsd_permanentaddress1"] = "";
            //traceService.Trace("1111");
            if (en.Contains("telephone1")) enUp["bsd_mobilephone"] = (string)en["telephone1"];
            else enUp["bsd_mobilephone"] = "";
            //traceService.Trace("1111");
            if (en.Contains("emailaddress1")) enUp["bsd_email"] = (string)en["emailaddress1"];
            else enUp["bsd_email"] = "";
            //traceService.Trace("1111");
        }
        private EntityCollection findCo_owner_subsale(IOrganizationService crmservices, Guid id)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_coowner"">
                <attribute name=""bsd_stt"" />
                <filter>
                  <condition attribute=""bsd_subsale"" operator=""eq"" value=""{id}"" />
                  <condition attribute=""bsd_current"" operator=""eq"" value=""{0}"" />
                  <condition attribute=""statuscode"" operator=""eq"" value=""{1}"" />
                </filter>
                <order attribute=""createdon"" />
              </entity>
            </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findCo_owner_subsale2(IOrganizationService crmservices, Guid id)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""bsd_coowner"">
                <attribute name=""bsd_stt"" />
                <filter>
                  <condition attribute=""bsd_subsale"" operator=""eq"" value=""{id}"" />
                  <condition attribute=""bsd_current"" operator=""eq"" value=""{1}"" />
                  <condition attribute=""statuscode"" operator=""eq"" value=""{1}"" />
                </filter>
                <order attribute=""createdon"" />
              </entity>
            </fetch>";
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
    }
}
