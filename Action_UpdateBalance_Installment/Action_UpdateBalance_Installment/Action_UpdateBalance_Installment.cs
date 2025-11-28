using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_UpdateBalance_Installment
{
    public class Action_UpdateBalance_Installment : IPlugin
    {


        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService((context).UserId);
            string str = ((context).InputParameters)["id"].ToString();
            char[] chArray = new char[1] { ',' };
            foreach (string input in str.Split(chArray))
            {
                Entity entity1 = service.Retrieve("bsd_updatebalanceinstallment", Guid.Parse(input), new ColumnSet(true));
                Entity upentity1 = new Entity(entity1.LogicalName, entity1.Id);
                //throw new InvalidPluginExecutionException("thinh test" + entity1.Id);
                if (entity1.Contains("bsd_installment"))
                {
                    //throw new InvalidPluginExecutionException("thinh test" + entity1.LogicalName);
                    tracingService.Trace("vào Action_UpdateBalance_Installment");
                    Entity installment = service.Retrieve(((EntityReference)entity1["bsd_installment"]).LogicalName, ((EntityReference)entity1["bsd_installment"]).Id, new ColumnSet(true));
                    Entity enupinstallment = new Entity(installment.LogicalName, installment.Id);
                    
                    //int num = entity1.Contains("statuscode") ? ((OptionSetValue)entity1["statuscode"]).Value : 0;
                    //enupdateproduct["bsd_approveddate"] = DateTime.UtcNow;
                    enupinstallment["bsd_balance"] = entity1.Contains("bsd_balance") ? entity1["bsd_balance"] : null;
                    upentity1["statuscode"] = new OptionSetValue(100000000);
                    service.Update(upentity1);
                    service.Update(enupinstallment);
                }
                tracingService.Trace("Kết thúc Action_UpdateBalance_Installment");
                
            }
        }
    }
}

