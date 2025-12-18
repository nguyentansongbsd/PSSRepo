using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugin_Contact_Create_Update
{
    public class Plugin_Contact_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName == "Create" && context.Depth == 2)
            {
                traceService.Trace("Import");
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                traceService.Trace("step 1");
                string line = "";
                string line1 = "";
                string line0 = "";
                string line10 = "";
                if (enTarget.Contains("bsd_housenumberstreet"))
                    line += (string)enTarget["bsd_housenumberstreet"];
                if (enTarget.Contains("bsd_housenumber"))
                    line1 += (string)enTarget["bsd_housenumber"];
                if (enTarget.Contains("bsd_district"))
                {
                    var fetchXml = $@"
                    <fetch>
                      <entity name='new_district'>
                        <attribute name='new_name' />
                        <attribute name='bsd_nameen' />
                        <filter>
                          <condition attribute='new_districtid' operator='eq' value='{((EntityReference)enTarget["bsd_district"]).Id}'/>
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity item in list.Entities)
                    {
                        if (item.Contains("new_name"))
                            line += ", " + (string)item["new_name"];
                        if (enTarget.Contains("bsd_nameen"))
                            line1 += ", " + (string)item["bsd_nameen"];
                    }
                }
                if (enTarget.Contains("bsd_province"))
                {
                    var fetchXml = $@"
                    <fetch>
                      <entity name='new_province'>
                        <attribute name='bsd_provincename' />
                        <attribute name='bsd_nameen' />
                        <filter>
                          <condition attribute='new_provinceid' operator='eq' value='{((EntityReference)enTarget["bsd_province"]).Id}'/>
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity item in list.Entities)
                    {
                        if (item.Contains("bsd_provincename"))
                            line += ", " + (string)item["bsd_provincename"];
                        if (enTarget.Contains("bsd_nameen"))
                            line1 += ", " + (string)item["bsd_nameen"];
                    }
                }
                if (enTarget.Contains("bsd_country"))
                {
                    var fetchXml = $@"
                    <fetch>
                      <entity name='bsd_country'>
                        <attribute name='bsd_countryname' />
                        <attribute name='bsd_nameen' />
                        <filter>
                          <condition attribute='bsd_countryid' operator='eq' value='{((EntityReference)enTarget["bsd_country"]).Id}'/>
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity item in list.Entities)
                    {
                        if (item.Contains("bsd_countryname"))
                            line += ", " + (string)item["bsd_countryname"];
                        if (enTarget.Contains("bsd_nameen"))
                            line1 += ", " + (string)item["bsd_nameen"];
                    }
                }
                if (!enTarget.Contains("bsd_housenumberstreet") && line.Length > 0)
                    line = line.Substring(2, line.Length);
                if (!enTarget.Contains("bsd_housenumber") && line1.Length > 0)
                    line1 = line1.Substring(2, line1.Length);

                if (enTarget.Contains("bsd_permanentaddress"))
                    line0 += (string)enTarget["bsd_permanentaddress"];
                if (enTarget.Contains("bsd_permanenthousenumber"))
                    line10 += (string)enTarget["bsd_permanenthousenumber"];
                if (enTarget.Contains("bsd_permanentdistrict"))
                {
                    var fetchXml = $@"
                    <fetch>
                      <entity name='new_district'>
                        <attribute name='new_name' />
                        <attribute name='bsd_nameen' />
                        <filter>
                          <condition attribute='new_districtid' operator='eq' value='{((EntityReference)enTarget["bsd_permanentdistrict"]).Id}'/>
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity item in list.Entities)
                    {
                        if (item.Contains("new_name"))
                            line0 += ", " + (string)item["new_name"];
                        if (enTarget.Contains("bsd_nameen"))
                            line10 += ", " + (string)item["bsd_nameen"];
                    }
                }
                if (enTarget.Contains("bsd_permanentprovince"))
                {
                    var fetchXml = $@"
                    <fetch>
                      <entity name='new_province'>
                        <attribute name='bsd_provincename' />
                        <attribute name='bsd_nameen' />
                        <filter>
                          <condition attribute='new_provinceid' operator='eq' value='{((EntityReference)enTarget["bsd_permanentprovince"]).Id}'/>
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity item in list.Entities)
                    {
                        if (item.Contains("bsd_provincename"))
                            line0 += ", " + (string)item["bsd_provincename"];
                        if (enTarget.Contains("bsd_nameen"))
                            line10 += ", " + (string)item["bsd_nameen"];
                    }
                }
                if (enTarget.Contains("bsd_permanentcountry"))
                {
                    var fetchXml = $@"
                    <fetch>
                      <entity name='bsd_country'>
                        <attribute name='bsd_countryname' />
                        <attribute name='bsd_nameen' />
                        <filter>
                          <condition attribute='bsd_countryid' operator='eq' value='{((EntityReference)enTarget["bsd_permanentcountry"]).Id}'/>
                        </filter>
                      </entity>
                    </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity item in list.Entities)
                    {
                        if (item.Contains("bsd_countryname"))
                            line0 += ", " + (string)item["bsd_countryname"];
                        if (enTarget.Contains("bsd_nameen"))
                            line10 += ", " + (string)item["bsd_nameen"];
                    }
                }
                if (!enTarget.Contains("bsd_permanentaddress") && line0.Length > 0)
                    line0 = line0.Substring(2, line0.Length);
                if (!enTarget.Contains("bsd_permanenthousenumber") && line10.Length > 0)
                    line10 = line10.Substring(2, line10.Length);
                Entity enUp = new Entity(target.LogicalName, target.Id);
                enUp["bsd_contactaddress"] = line;
                enUp["bsd_diachi"] = line1;
                enUp["bsd_permanentaddress1"] = line0;
                enUp["bsd_diachithuongtru"] = line10;
                service.Update(enUp);
            }
            else if (context.MessageName == "Update")
            {
                traceService.Trace("Update");
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                decimal bsd_totalamountofownership = enTarget.Contains("bsd_totalamountofownership") ? ((Money)enTarget["bsd_totalamountofownership"]).Value : 0;
                int bsd_loyaltypointspa = enTarget.Contains("bsd_loyaltypointspa") ? (int)enTarget["bsd_loyaltypointspa"] : 0;
                Entity enUp = new Entity(target.LogicalName, target.Id);
                decimal bsd_loyaltypoints = 0;
                if (bsd_totalamountofownership > 0) bsd_loyaltypoints = bsd_loyaltypointspa + Math.Floor(bsd_totalamountofownership / 1_000_000m);
                enUp["bsd_loyaltypoints"] = bsd_loyaltypoints;
                service.Update(enUp);
                traceService.Trace("done");
            }
        }
    }
}
