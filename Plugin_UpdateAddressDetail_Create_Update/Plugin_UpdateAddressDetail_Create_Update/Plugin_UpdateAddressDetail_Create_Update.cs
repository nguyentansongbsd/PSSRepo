using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Routing;

namespace Plugin_UpdateAddressDetail_Create_Update
{
    public class Plugin_UpdateAddressDetail_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceServiceClass = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceServiceClass = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceServiceClass.Trace("Plugin_UpdateAddressDetail_Create_Update");
            traceServiceClass.Trace("MessageName " + context.MessageName);
            traceServiceClass.Trace("Depth " + context.Depth);
            if ((context.MessageName == "Create" && context.Depth < 3) || (context.MessageName == "Update" && context.Depth == 1))
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                int bsd_type = enTarget.Contains("bsd_type") ? ((OptionSetValue)enTarget["bsd_type"]).Value : 0;
                if (bsd_type == 100000000)//account
                {
                    Entity enUp = new Entity(target.LogicalName, target.Id);
                    string line = "";
                    string line1 = "";
                    if (enTarget.Contains("bsd_housenumberstreetwardvn"))
                    {
                        line += (string)enTarget["bsd_housenumberstreetwardvn"];
                    }
                    else throw new InvalidPluginExecutionException("Please fill in house number, street, ward (VN).");
                    if (enTarget.Contains("bsd_housenumberstreetward"))
                    {
                        line1 += (string)enTarget["bsd_housenumberstreetward"];
                    }
                    if (enTarget.Contains("bsd_ward"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='new_district'>
                            <attribute name=""new_name"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='new_districtid' operator='eq' value='{((EntityReference)enTarget["bsd_ward"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("new_name"))
                            {
                                line += ", " + (string)entity["new_name"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line1 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    if (enTarget.Contains("bsd_province"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='new_province'>
                            <attribute name=""bsd_provincename"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='new_provinceid' operator='eq' value='{((EntityReference)enTarget["bsd_province"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("bsd_provincename"))
                            {
                                line += ", " + (string)entity["bsd_provincename"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line1 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    if (enTarget.Contains("bsd_country"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='bsd_country'>
                            <attribute name=""bsd_countryname"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='bsd_countryid' operator='eq' value='{((EntityReference)enTarget["bsd_country"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("bsd_countryname"))
                            {
                                line += ", " + (string)entity["bsd_countryname"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line1 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    else throw new InvalidPluginExecutionException("Please fill in country information.");
                    enUp["bsd_addressvn"] = line;
                    enUp["bsd_address"] = line1;
                    service.Update(enUp);
                }
                else if (bsd_type == 100000001)//contact
                {
                    Entity enUp = new Entity(target.LogicalName, target.Id);
                    string line = "";
                    string line1 = "";
                    traceServiceClass.Trace("1111");
                    if (enTarget.Contains("bsd_housenumberstreetwardvn"))
                    {
                        line += (string)enTarget["bsd_housenumberstreetwardvn"];
                    }
                    else throw new InvalidPluginExecutionException("Please fill in house number, street, ward (VN).");
                    if (enTarget.Contains("bsd_housenumberstreetward"))
                    {
                        line1 += (string)enTarget["bsd_housenumberstreetward"];
                    }
                    traceServiceClass.Trace("1111");
                    if (enTarget.Contains("bsd_ward"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='new_district'>
                            <attribute name=""new_name"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='new_districtid' operator='eq' value='{((EntityReference)enTarget["bsd_ward"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("new_name"))
                            {
                                line += ", " + (string)entity["new_name"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line1 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    traceServiceClass.Trace("1111");
                    if (enTarget.Contains("bsd_province"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='new_province'>
                            <attribute name=""bsd_provincename"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='new_provinceid' operator='eq' value='{((EntityReference)enTarget["bsd_province"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("bsd_provincename"))
                            {
                                line += ", " + (string)entity["bsd_provincename"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line1 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    traceServiceClass.Trace("1111");
                    if (enTarget.Contains("bsd_country"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='bsd_country'>
                            <attribute name=""bsd_countryname"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='bsd_countryid' operator='eq' value='{((EntityReference)enTarget["bsd_country"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("bsd_countryname"))
                            {
                                line += ", " + (string)entity["bsd_countryname"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line1 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    else throw new InvalidPluginExecutionException("Please fill in country information.");
                    traceServiceClass.Trace("1111");
                    enUp["bsd_addressvn"] = line;
                    enUp["bsd_address"] = line1;
                    string line2 = "";
                    string line3 = "";
                    if (enTarget.Contains("bsd_permanenthousenumberstreetwardvn"))
                    {
                        line2 += (string)enTarget["bsd_permanenthousenumberstreetwardvn"];
                    }
                    else throw new InvalidPluginExecutionException("Please fill in house number, street, ward (VN).");
                    traceServiceClass.Trace("1111");
                    if (enTarget.Contains("bsd_permanenthousenumberstreetward"))
                    {
                        line3 += (string)enTarget["bsd_permanenthousenumberstreetward"];
                    }
                    traceServiceClass.Trace("1111");
                    if (enTarget.Contains("bsd_permanentward"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='new_district'>
                            <attribute name=""new_name"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='new_districtid' operator='eq' value='{((EntityReference)enTarget["bsd_permanentward"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("new_name"))
                            {
                                line2 += ", " + (string)entity["new_name"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line3 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    traceServiceClass.Trace("1111");
                    if (enTarget.Contains("bsd_permanentprovince"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='new_province'>
                            <attribute name=""bsd_provincename"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='new_provinceid' operator='eq' value='{((EntityReference)enTarget["bsd_permanentprovince"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("bsd_provincename"))
                            {
                                line2 += ", " + (string)entity["bsd_provincename"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line3 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    traceServiceClass.Trace("1111");
                    if (enTarget.Contains("bsd_permanentcountry"))
                    {
                        var fetchXml2 = $@"
                        <fetch>
                          <entity name='bsd_country'>
                            <attribute name=""bsd_countryname"" />
                            <attribute name=""bsd_nameen"" />
                            <filter>
                              <condition attribute='bsd_countryid' operator='eq' value='{((EntityReference)enTarget["bsd_permanentcountry"]).Id}'/>
                              <condition attribute='statuscode' operator='eq' value='1'/>
                            </filter>
                          </entity>
                        </fetch>";
                        EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                        foreach (Entity entity in list2.Entities)
                        {
                            if (entity.Contains("bsd_countryname"))
                            {
                                line2 += ", " + (string)entity["bsd_countryname"];
                            }
                            if (entity.Contains("bsd_nameen"))
                            {
                                line3 += ", " + (string)entity["bsd_nameen"];
                            }
                        }
                    }
                    else throw new InvalidPluginExecutionException("Please fill in country information.");
                    traceServiceClass.Trace("1111");
                    enUp["bsd_permanentaddressvn"] = line2;
                    enUp["bsd_permanentaddress"] = line3;
                    service.Update(enUp);
                }
            }
        }
    }
}