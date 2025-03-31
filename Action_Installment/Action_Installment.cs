using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using BSDLibrary;
using Newtonsoft.Json;


namespace Action_Installment
{
    public class Action_Installment : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;

        Common common;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            common = new Common(service);
            traceService.Trace("Begin");
            string installmentid = context.InputParameters["installmentid"].ToString();
            traceService.Trace("installmentid: " + installmentid);
            string stramountpay = context.InputParameters["amountpay"].ToString();
            decimal amountpay = Convert.ToDecimal(stramountpay);
            traceService.Trace("amountpay: " + amountpay.ToString());
            string receiptdateimport = (string)context.InputParameters["receiptdate"];
            traceService.Trace("receiptdate1111: " + receiptdateimport);

            DateTime receiptdate = Convert.ToDateTime(receiptdateimport);

            receiptdate = common.RetrieveLocalTimeFromUTCTime(receiptdate);
            traceService.Trace("receiptdate: " + receiptdate.ToString());
            Entity enIntallment = new Entity("bsd_paymentschemedetail", new Guid(installmentid));
            traceService.Trace("11");
            Installment installment = new Installment(serviceProvider, enIntallment);
            traceService.Trace("22");
            installment.LateDays = installment.getLateDays(receiptdate);
            traceService.Trace("LateDays: " + installment.LateDays.ToString());
            if (amountpay > 0)
            {
                installment.InterestCharge = installment.calc_InterestCharge(receiptdate, amountpay);
                traceService.Trace("InterestCharge: " + installment.InterestCharge.ToString());
            }
            context.OutputParameters["result"] = JsonConvert.SerializeObject(installment);
        }
    }
}
