﻿using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_OptionEntry_ConvertToOption_MappingFields
{
    public class Plugin_OptionEntry_ConvertToOption_MappingFields : IPlugin
    {
        private IPluginExecutionContext context = null;
        private IOrganizationServiceFactory factory = null;
        private IOrganizationService service = null;
        private ITracingService tracingService = null;

        private Entity target = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = factory.CreateOrganizationService(this.context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Init();
        }

        private void Init()
        {
            try
            {
                if (this.context.Depth > 3) return;
                if (this.context.MessageName != "Update") return;
                Entity _target = (Entity)this.context.InputParameters["Target"];
                this.target = this.service.Retrieve(_target.LogicalName,_target.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                
                if (((OptionSetValue)this.target["statuscode"]).Value != 100000000) return; // 100000000 = Option

                MappingFielUnitType();
                MappingFielWordTemplate();
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void MappingFielUnitType()
        {
            try
            {
                if (this.target.Contains("bsd_unittype")) return;
                Entity enOption = new Entity(this.target.LogicalName, this.target.Id);
                enOption["bsd_unittype"] = getUnitType();
                this.service.Update(enOption);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void MappingFielWordTemplate()
        {
            try
            {
                Entity enOption = new Entity(this.target.LogicalName, this.target.Id);
                EntityReference enDeveloper = null;
                if(!this.target.Contains("bsd_developer"))
                    enOption["bsd_developer"] = enDeveloper = getDeveloper();
                if (!this.target.Contains("bsd_developer") && enDeveloper != null)
                    enOption["bsd_mandatoryprimary"] = getMandatoryPrimary(enDeveloper);
                if (!this.target.Contains("bsd_floor"))
                    enOption["bsd_floor"] = getFloor();
                //if (!this.target.Contains("bsd_theunitpriceof01sqm"))
                //{
                //    decimal totalamount = this.target.Contains("totalamount") ? ((Money)this.target["totalamount"]).Value : 0;
                //    decimal netusablearea = this.target.Contains("bsd_netusablearea") ? (decimal)this.target["bsd_netusablearea"] : 0;
                //    tracingService.Trace(totalamount + " - " + netusablearea);
                //    enOption["bsd_theunitpriceof01sqm"] = netusablearea > 0 ?  totalamount / netusablearea : 0;
                //}    
                    
                this.service.Update(enOption);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityReference getUnitType()
        {
            try
            {
                tracingService.Trace("get unit type");
                if (!this.target.Contains("bsd_unitnumber")) return null;
                Entity enUnit = this.service.Retrieve(((EntityReference)this.target["bsd_unitnumber"]).LogicalName, ((EntityReference)this.target["bsd_unitnumber"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { "bsd_unittype" }));
                if (!enUnit.Contains("bsd_unittype")) return null;
                tracingService.Trace("Có unit type");
                return (EntityReference)enUnit["bsd_unittype"];
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityReference getDeveloper()
        {
            try
            {
                tracingService.Trace("get developer");
                Entity enProject = this.service.Retrieve(((EntityReference)this.target["bsd_project"]).LogicalName, ((EntityReference)this.target["bsd_project"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { "bsd_investor" }));
                if (enProject.Contains("bsd_investor")) return (EntityReference)enProject["bsd_investor"];
                else return null;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityReference getMandatoryPrimary(EntityReference enfDeveloper)
        {
            try
            {
                tracingService.Trace("get primary");
                Entity enContact = this.service.Retrieve(enfDeveloper.LogicalName, enfDeveloper.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { "primarycontactid" }));
                if (enContact.Contains("primarycontactid")) return (EntityReference)enContact["primarycontactid"];
                else return null;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private EntityReference getFloor()
        {
            try
            {
                tracingService.Trace("get floor");
                Entity enUnit = this.service.Retrieve(((EntityReference)this.target["bsd_unitnumber"]).LogicalName, ((EntityReference)this.target["bsd_unitnumber"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { "bsd_floor" }));
                if (enUnit.Contains("bsd_floor")) return (EntityReference)enUnit["bsd_floor"];
                else return null;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
