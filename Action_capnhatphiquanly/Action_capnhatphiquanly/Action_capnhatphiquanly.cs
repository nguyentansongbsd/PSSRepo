using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_capnhatphiquanly
{
    public class Action_capnhatphiquanly : IPlugin
    {
        private ITracingService traceS;
        private IOrganizationServiceFactory factory;
        private IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            //IOrganizationService service1 = serviceFactory.CreateOrganizationService(context.UserId);
            this.service = this.factory.CreateOrganizationService(new Guid?(((IExecutionContext)service).UserId));
            string str = ((DataCollection<string, object>)((IExecutionContext)service).InputParameters)["id"].ToString();
            char[] chArray = new char[1] { ',' };
            foreach (string input in str.Split(chArray))
            {
                Entity capnhat = this.service.Retrieve("bsd_capnhatphiquanly", Guid.Parse(input), new ColumnSet(true));
                //throw new InvalidPluginExecutionException("thinh test" + entity1.Id);
                Entity upcapnhat = new Entity(capnhat.LogicalName, capnhat.Id);
                upcapnhat["statuscode"] = new OptionSetValue(100000000);//Đã cập nhật
                this.service.Update(upcapnhat);
                EntityReference oe = capnhat.Contains("bsd_optionentry") ? (EntityReference)capnhat["bsd_optionentry"] : null;
                Entity upoe = new Entity(oe.LogicalName, oe.Id);
                upoe["bsd_managementfee"] = capnhat.Contains("bsd_phiquanly") ? capnhat["bsd_phiquanly"]:null;
                this.service.Update(upoe);
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_paymentschemedetail"">
                    <attribute name=""bsd_managementamount"" />
                    <filter>
                      <condition attribute=""bsd_duedatecalculatingmethod"" operator=""eq"" value=""{100000002}"" />
                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{oe.Id}"" />
                    </filter>
                  </entity>
                </fetch>";
                EntityCollection rs = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (rs.Entities.Count > 0)
                {
                    foreach (var entity in rs.Entities)
                    {
                        // Create a new entity object to update
                        var updateEntity = new Entity(entity.LogicalName, entity.Id)
                        {
                            ["bsd_managementamount"] = capnhat.Contains("bsd_phiquanly") ? capnhat["bsd_phiquanly"] : null
                        };
                        this.service.Update(updateEntity);
                    }
                }
            }
        }
    }
}
