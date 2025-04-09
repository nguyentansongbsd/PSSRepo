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

namespace Action_CofirmCollect_Bulk
{
    public class Action_CofirmCollect_Bulk : IPlugin
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
            string input = "";
            if (!string.IsNullOrEmpty((string)context.InputParameters["input"]))
            {
                input = context.InputParameters["input"].ToString();
                List<string> list = input.Split(',').ToList();
                foreach (string item in list)
                {
                    var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_advancepayment'>
                                <attribute name='statuscode' />
                                <attribute name='bsd_optionentry' />
                                <filter type='and'>
                                  <condition attribute='bsd_advancepaymentid' operator='eq' value='{item}'/>
                                </filter>
                              </entity>
                            </fetch>";
                    EntityCollection enAdvan = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity enItem in enAdvan.Entities)
                    {
                        int statuscode = enItem.Contains("statuscode") ? ((OptionSetValue)enItem["statuscode"]).Value : 0;
                        if (statuscode != 1) throw new InvalidPluginExecutionException("The record status is invalid. Please check again.");
                        if (enItem.Contains("bsd_optionentry"))
                        {
                            var fetchXml2 = $@"
                            <fetch>
                              <entity name='salesorder'>
                                <attribute name='statuscode' />
                                <attribute name='salesorderid' />
                                <filter type='and'>
                                  <condition attribute='salesorderid' operator='eq' value='{((EntityReference)enItem["bsd_optionentry"]).Id}'/>
                                  <condition attribute='statuscode' operator='eq' value='100000006'/>
                                </filter>
                              </entity>
                            </fetch>";
                            EntityCollection enOE = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                            if (enOE.Entities.Count > 0) throw new InvalidPluginExecutionException("Option Entry has Termination. Please check again.");
                        }
                        Entity enUp = new Entity(enItem.LogicalName, enItem.Id);
                        enUp["statuscode"] = new OptionSetValue(100000000);
                        service.Update(enUp);
                    }
                }
            }
        }
    }
}
