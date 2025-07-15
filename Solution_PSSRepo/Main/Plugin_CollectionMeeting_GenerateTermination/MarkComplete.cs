using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


namespace Plugin_CollectionMeeting_GenerateTermination
{
    public class MarkComplete : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && (context.InputParameters["Target"] is Entity))
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Guid targetId = target.Id;
                if (target.LogicalName == "bsd_terminateletter")
                {
                    if (context.MessageName == "Create")
                    {
                        try
                        {
                            //Jira: CLV-1436
                            #region ---#. Check condintion bsd_terminatefee in FUL ---
                            info_Error info = new info_Error();
                            EntityReference FUL = null;
                            if (target.Contains("bsd_followuplist")) FUL = (EntityReference)target["bsd_followuplist"];
                            var val = amount_TerminateFee(service, FUL.Id, ref info);
                            Decimal zero = 0m;
                            if (info.result)
                            {
                                var update = new Entity(target.LogicalName);
                                update.Id = target.Id;
                                update["bsd_terminatefeewaiver"] = new Money(zero);
                                update["bsd_totalforfeitureamount"] = new Money(val);
                                update["bsd_terminatefee"] = new Money(val);
                                service.Update(update);
                            }
                            else throw new Exception(info.message);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get entity bsd_followuplist, return bsd_terminatefee off case
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private Decimal amount_TerminateFee(IOrganizationService sv, Guid id, ref info_Error rs)
        {
            decimal val = 0m;
            info_Error info = new info_Error();
            info.createMessageAdd("Function không trace lỗi, Exception return 0");
            try
            {
                bsd_followuplist FUL = new bsd_followuplist();
                var result = FUL.getByID(sv, id);
                if (!result.result) val = 0m;
                else
                {
                    var item = result.ent_First;
                    bool bsd_terminateletter = false;
                    int type = -100;

                    info.index = 1;
                    info.createMessageAdd("- Get value fields: bsd_terminateletter,bsd_takeoutmoney");

                    if (item.Contains("bsd_terminateletter")) bsd_terminateletter = item.GetAttributeValue<bool>("bsd_terminateletter");
                    if (item.Contains("bsd_takeoutmoney")) type = ((OptionSetValue)item["bsd_takeoutmoney"]).Value;

                    #region --- Trace ---
                    info.index = 2;
                    info.createMessageAdd(string.Format("- bsd_terminateletter: {0}", bsd_terminateletter));
                    info.createMessageAdd(string.Format("- bsd_takeoutmoney: {0}", type));
                    if (type == -100) throw new Exception("[FUL] Take out money not value!");
                    #endregion

                    #region --- bsd_terminateletter = yes ---
                    if (bsd_terminateletter)
                    {
                        #region --- Get fields money in FUL ---
                        info.index = 3;
                        info.createMessageAdd(string.Format("- Get money in FUL"));
                        if (item.Contains("bsd_totalforfeitureamount")) FUL.TotalForfeitureAmount = ((Money)item["bsd_totalforfeitureamount"]).Value;
                        if (item.Contains("bsd_totalamountpaid")) FUL.TotalPaid = ((Money)item["bsd_totalamountpaid"]).Value;
                        if (item.Contains("bsd_forfeitureamount")) FUL.Refund = ((Money)item["bsd_forfeitureamount"]).Value;
                        #endregion

                        switch (type)
                        {
                            case (int)bsd_followuplist.bsd_takeoutmoney.Forfeiture:
                                #region --- Terminate Fee = Total Forfeiture Amount ---
                                info.index = 4;
                                info.createMessageAdd(string.Format("Case: Forfeiture"));
                                info.result = true;
                                val = FUL.TotalForfeitureAmount;
                                #endregion
                                break;
                            case (int)bsd_followuplist.bsd_takeoutmoney.Refund:
                                #region --- Terminate Fee = Total Forfeiture Amount = Total Paid - Refund --> luôn luôn lớn hơn hoặc = 0 ---
                                info.index = 5;
                                info.createMessageAdd(string.Format("Case: Refund"));
                                decimal TerminateFee = FUL.TotalPaid - FUL.Refund;
                                if (TerminateFee != FUL.TotalForfeitureAmount)
                                {
                                    info.index = 51;
                                    info.createMessageAdd(string.Format("#. Trace: Total Forfeiture Amount != Total Paid - Refund"));
                                    info.result = true;
                                    val = TerminateFee;
                                }
                                else if (TerminateFee < 0)
                                {
                                    info.index = 52;
                                    info.createMessageAdd(string.Format("#. Trace: Total Paid - Refund < 0"));
                                    info.result = false;
                                    throw new Exception("Terminate Fee = Total Paid - Refund < 0 ");
                                }
                                else
                                {
                                    info.result = true;
                                    val = TerminateFee;
                                }
                                #endregion
                                break;
                            default:
                                info.index = 6;
                                info.createMessageAdd(string.Format("Case: Default"));
                                info.result = false;
                                val = 0m;
                                break;
                        }
                    }
                    #endregion
                }
                info.createMessageAdd(string.Format("Return value: {0}", val));
                rs = info;
            }
            catch (Exception ex)
            {
                info.result = false;
                info.createMessageAdd(string.Format("#.Func amount_TerminateFee: index[{0}], message: {1}, exception: {2}", info.index, info.message, ex.Message));
                rs = info;
                val = 0;
            }
            return val;
        }
    }
}
