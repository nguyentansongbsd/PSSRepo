using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_GetParamPaymentNotice
{
    public class Action_GetParamPaymentNotice : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory serviceFactory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            // Lấy context
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity paymentNotice = service.Retrieve("bsd_customernotices", new Guid(context.InputParameters["id"].ToString()), new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var enProjectRef = (EntityReference)paymentNotice["bsd_project"];
            var enProject = service.Retrieve(enProjectRef.LogicalName, enProjectRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var enOPRef = (EntityReference)paymentNotice["bsd_optionentry"];
            var enOP = service.Retrieve(enOPRef.LogicalName, enOPRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var enInsRef = (EntityReference)paymentNotice["bsd_paymentschemedetail"];
            var enIns = service.Retrieve(enInsRef.LogicalName, enInsRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var enDev =service.Retrieve("account", ((EntityReference)enProject["bsd_investor"]).Id,new ColumnSet(true));
            Entity enBankDetail = null;
            #region lấy bank project
            var query = new QueryExpression("bsd_projectbankaccount");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_project", ConditionOperator.Equal, enProject.Id.ToString());
            var rs = service.RetrieveMultiple(query);
            Entity enBank = null;
            foreach (var r in rs.Entities)
            {
                if (r.Contains("bsd_default") && (bool)r["bsd_default"])
                    enBank = r;
            }
            if (enBank == null && rs.Entities.Count > 0)
            {
                enBank = rs.Entities[0];
            }
            #endregion
            if (enBank != null)
            {
                enBankDetail = service.Retrieve("bsd_bank", (enBank["bsd_bank"] as EntityReference).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            }
            var bsd_ordernumber = (int)enIns["bsd_ordernumber"];
            var bsd_ordernumbernd = "_";
            if ((bsd_ordernumber) == 2)
            {
                bsd_ordernumbernd = "2nd";
            }
            else
                if ((bsd_ordernumber) == 3)
            {
                bsd_ordernumbernd = "3nd";
            }
            else
            {
                bsd_ordernumbernd = $"{bsd_ordernumber}th";
            }
            var bsd_dadate = enOP.Contains("bsd_dadate") ? ((DateTime)enOP["bsd_dadate"]).AddHours(7).ToString("dd/MM/yyyy") : "_____";
            var bsd_amountofthisphase = ((Money)enIns["bsd_amountofthisphase"]).Value.ToString("N0");
            var bsd_duedate = ((DateTime)enIns["bsd_duedate"]).AddHours(7).ToString("dd/MM/yyyy");
            context.OutputParameters["bsd_duedate"] = bsd_duedate;
            context.OutputParameters["bsd_ordernumber"] = bsd_ordernumber;
            context.OutputParameters["bsd_ordernumbernd"] = bsd_ordernumbernd;
            context.OutputParameters["bsd_amountofthisphase"] = bsd_amountofthisphase;
            context.OutputParameters["bsd_dadate"] = bsd_dadate;

            context.OutputParameters["bsd_accountnameother"] =enDev.Contains("bsd_accountnameother")? enDev["bsd_accountnameother"]:"_";//1
            tracingService.Trace("step1");
            context.OutputParameters["bsd_accountname"] =enDev != null ? enDev["name"] : "_";//2
            tracingService.Trace("step2");
            context.OutputParameters["bsd_acountdiachi"] = enDev.Contains
                ("bsd_diachithuongtru")? enDev["bsd_diachithuongtru"] : "_";//3
            tracingService.Trace("step3");
            context.OutputParameters["bsd_registrationcode1"] =enDev.Contains("bsd_registrationcode") ?enDev["bsd_registrationcode"]:"_";//4
            tracingService.Trace("step4");
            context.OutputParameters["bsd_developphone"] =enDev.Contains("telephone1")?enDev["telephone1"]:"_";//5
            tracingService.Trace("step5");
            context.OutputParameters["bsd_developfax"] =enDev.Contains("fax")? enDev["fax"]:"_";//6
            tracingService.Trace("step6");
            context.OutputParameters["bsd_companycode"] = enDev.Contains("bsd_companycode") ? enDev["bsd_companycode"] : "_";//7
            tracingService.Trace("step7");
            context.OutputParameters["bsd_danumber"] = enOP.Contains("bsd_danumber")? enOP["bsd_danumber"]:"_";//8
            tracingService.Trace("step8");
            context.OutputParameters["bsd_bankaccount"] = enBank!=null? enBank["bsd_name"]:"_";//9
            tracingService.Trace("step9");
            context.OutputParameters["bsd_othername"] =enBankDetail!=null && enBankDetail.Contains("bsd_othername") ? enBankDetail["bsd_othername"]:"_";//10
            tracingService.Trace("step10"); 
            context.OutputParameters["bsd_bankname"] = enBankDetail != null ? enBankDetail["bsd_name"] : "_";//11
            tracingService.Trace("step11");
            context.OutputParameters["bsd_addressother"] = (enBankDetail != null&& enBankDetail.Contains("bsd_addressother")) ? enBankDetail["bsd_addressother"] : "_";//12
            tracingService.Trace("step12");
            context.OutputParameters["bsd_address"] = enBankDetail != null && enBankDetail.Contains("bsd_address") ? enBankDetail["bsd_address"] : "_";//13
            tracingService.Trace("step13");
            context.OutputParameters["bsd_swiftcode"] = enBankDetail != null && enBankDetail.Contains("bsd_swiftcode") ? enBankDetail["bsd_swiftcode"] : "_";//14
            tracingService.Trace("step14");
            context.OutputParameters["bsd_acountant"] =enProject.Contains("bsd_acountant")? enProject["bsd_acountant"]:"_";//15
            tracingService.Trace("step15");
            context.OutputParameters["bsd_extfin"] =enProject.Contains("bsd_extfin")?enProject["bsd_extfin"]:"_";//16
            tracingService.Trace("step16");
            context.OutputParameters["bsd_contractnumber"]=enOP.Contains("bsd_contractnumber") ? enOP["bsd_contractnumber"] : "_";//17
            context.OutputParameters["bsd_contractdate"]=enOP.Contains("bsd_contractdate") ? ((DateTime)enOP["bsd_contractdate"]).AddHours(7).ToString("dd/MM/yyyy") : "_";//18

        }
    }
}
