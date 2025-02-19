using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_UpdatePriceList_Approve
{
    public class Action_UpdatePriceList_Approve : IPlugin
    {
        private IPluginExecutionContext context = null;
        private IOrganizationServiceFactory factory = null;
        private IOrganizationService service = null;
        private ITracingService tracingService = null;

        private Entity en = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = factory.CreateOrganizationService(this.context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                Init().Wait();
            }
            catch (AggregateException ex)
            {
                var inner = ex.InnerExceptions.FirstOrDefault();
                if (inner != null)
                    throw inner;
            }
        }
        private async Task Init()
        {
            try
            {
                EntityReference target = (EntityReference)this.context.InputParameters["Target"];
                this.en = this.service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "price", "statuscode", "bsd_amount" }));

                Entity enUnit = getProductPrice();
                if (!enUnit.Contains("price") || ((Money)enUnit["price"]).Value <= 0) throw new InvalidPluginExecutionException("The product price is less than or equal to 0. Please check the information again.");
                if (((OptionSetValue)enUnit["statuscode"]).Value != 1 && ((OptionSetValue)enUnit["statuscode"]).Value != 100000000) throw new InvalidPluginExecutionException("Products that are not in the \"Available\" or \"Preparing\" status will not have their prices updated.");
                

            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private Entity getProductPrice()
        {
            try
            {
                if (!this.en.Contains("bsd_product")) throw new InvalidPluginExecutionException("Please enter product.");
                Entity enUnit = this.service.Retrieve(((EntityReference)this.en["bsd_product"]).LogicalName, ((EntityReference)this.en["bsd_product"]).Id, new ColumnSet(new string[] { "price", "statuscode" }));
                return enUnit;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task updatePriceList()
        {
            try
            {

            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task updateProruct(EntityReference enfunit)
        {
            try
            {
                if (!this.en.Contains("bsd_amount")) return;
                Entity enUnit = new Entity(enfunit.LogicalName, enfunit.Id);
                enUnit["price"] = new Money(((Money)this.en["bsd_amount"]).Value);
                this.service.Update(enUnit);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task update_UpdatePriceList()
        {
            try
            {
                Entity enUpdatePriceList = new Entity(this.en.LogicalName,this.en.Id);
                enUpdatePriceList["statuscode"] = new OptionSetValue(100000000);
                enUpdatePriceList["bsd_approvaldate"] = DateTime.Now;
                enUpdatePriceList["bsd_approver"] = new EntityReference("systemuser", this.context.UserId);
                this.service.Update(enUpdatePriceList);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
