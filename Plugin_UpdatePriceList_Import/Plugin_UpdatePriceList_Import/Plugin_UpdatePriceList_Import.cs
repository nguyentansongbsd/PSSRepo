using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_UpdatePriceList_Import
{
    public class Plugin_UpdatePriceList_Import : IPlugin
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

            Init();
        }

        private void Init()
        {
            try
            {
                if (this.context.Depth > 3) return;
                if (this.context.MessageName != "Create") return;
                this.en = (Entity)this.context.InputParameters["Target"];

                if (((OptionSetValue)this.en["statuscode"]).Value != 1) throw new InvalidPluginExecutionException("The price update status is not \"Active\", the action cannot be performed.");
                Entity enUnit = getProductPrice();
                if(!enUnit.Contains("price") || ((Money)enUnit["price"]).Value <= 0) throw new InvalidPluginExecutionException("The product price is less than or equal to 0. Please check the information again.");
                if (((OptionSetValue)enUnit["statuscode"]).Value != 1 && ((OptionSetValue)enUnit["statuscode"]).Value != 100000000) throw new InvalidPluginExecutionException("Products that are not in the \"Available\" or \"Preparing\" status will not have their prices updated.");

                update(((Money)enUnit["price"]).Value);
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
                Entity enUnit = this.service.Retrieve(((EntityReference)this.en["bsd_product"]).LogicalName, ((EntityReference)this.en["bsd_product"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { "price", "statuscode" }));
                return enUnit;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private void update(decimal amount)
        {
            try
            {
                Entity enUpdateListPrice = new Entity(this.en.LogicalName, this.en.Id);
                enUpdateListPrice["bsd_amountold"] = new Money(amount);
                this.service.Update(enUpdateListPrice);
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
