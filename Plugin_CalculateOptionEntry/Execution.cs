using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;

namespace Plugin_CalculateOptionEntry
{
    public class Execution : IPlugin
    {
        private IOrganizationService service = null;

        private ITracingService trace = null;

        private Entity target = null;

        public Execution()
        {
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            bool flag;
            IPluginExecutionContext service = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            if (service.MessageName == "Create")
            {
                this.target = service.InputParameters["Target"] as Entity;
                if (this.target.LogicalName == "salesorderdetail")
                {
                    if ((!this.target.Contains("productid") ? false : this.target.Contains("salesorderid")))
                    {
                        IOrganizationServiceFactory organizationServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        this.service = organizationServiceFactory.CreateOrganizationService(new Guid?(service.UserId));
                        (new Calculation(this.service, this.target)).Calculate();
                    }
                }
            }
            else if (service.MessageName == "Update")
            {
                this.target = service.InputParameters["Target"] as Entity;
                IOrganizationServiceFactory service1 = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                this.service = service1.CreateOrganizationService(new Guid?(service.UserId));

                if (this.target.LogicalName != "salesorder")
                {
                    flag = false;
                }
                else
                {
                  
                  
            
                    flag = (this.target.Contains("bsd_waivermanafeemonth") || this.target.Contains("bsd_taxcode") || this.target.Contains("bsd_discountlist")  || this.target.Contains("bsd_discounts") || this.target.Contains("bsd_packagesellingamount") ? true : this.target.Contains("bsd_amountdiscountchange"));
                }
                if (flag)
                {
                   
                    QueryExpression queryExpression = new QueryExpression("salesorderdetail");
                    queryExpression.ColumnSet = new ColumnSet(new string[] { "priceperunit", "baseamount", "productid", "salesorderid" });
                    queryExpression.Criteria = new FilterExpression(LogicalOperator.And);
                    queryExpression.Criteria.AddCondition(new ConditionExpression("salesorderid",ConditionOperator.Equal, (object)this.target.Id));
                    queryExpression.TopCount = 1;
                    EntityCollection entityCollection = this.service.RetrieveMultiple(queryExpression);
                    if (entityCollection.Entities.Count > 0)
                    {
                        
                        Calculation calculation = new Calculation(this.service, entityCollection.Entities[0]);
                        calculation.Calculate();
                    }
                }
               
            }
        }
    }
}