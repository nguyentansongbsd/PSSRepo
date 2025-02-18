// Decompiled with JetBrains decompiler
// Type: Plugin_CollectionMeeting_GenerateTermination.MarkComplete
// Assembly: Plugin_CollectionMeeting_GenerateTermination, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f7afacec0aa430c5
// MVID: 48B5B8C3-1D78-484D-B78A-C63DDA7C8A96
// Assembly location: C:\Users\ngoct\Downloads\Plugin_CollectionMeeting_GenerateTermination_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using System;

namespace Plugin_CollectionMeeting_GenerateTermination2
{
    public class MarkComplete : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service1.UserId));
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (!service1.InputParameters.Contains("Target") || !(service1.InputParameters["Target"] is Entity))
                return;
            Entity inputParameter = (Entity)service1.InputParameters["Target"];
            Guid id = inputParameter.Id;
            if (inputParameter.LogicalName == "bsd_terminateletter")
            {
                if (service1.MessageName == "Create")
                {
                    try
                    {
                        info_Error rs = new info_Error();
                        EntityReference entityReference = (EntityReference)null;
                        if (inputParameter.Contains("bsd_followuplist"))
                            entityReference = (EntityReference)inputParameter["bsd_followuplist"];
                        else
                        {
                            return;
                        }
                        Decimal num1 = this.amount_TerminateFee(this.service, entityReference.Id, ref rs);
                        Decimal num2 = 0M;
                        if (!rs.result)
                            throw new Exception(rs.message);
                        this.service.Update(new Entity(inputParameter.LogicalName)
                        {
                            Id = inputParameter.Id,
                            ["bsd_terminatefeewaiver"] = (object)new Money(num2),
                            ["bsd_totalforfeitureamount"] = (object)new Money(num1),
                            ["bsd_terminatefee"] = (object)new Money(num1)
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
        }

        private Decimal amount_TerminateFee(IOrganizationService sv, Guid id, ref info_Error rs)
        {
            Decimal num1 = 0M;
            info_Error infoError = new info_Error();
            infoError.createMessageAdd("Function không trace lỗi, Exception return 0");
            try
            {
                bsd_followuplist bsdFollowuplist = new bsd_followuplist();
                info_Error byId = bsdFollowuplist.getByID(sv, id);
                if (!byId.result)
                {
                    num1 = 0M;
                }
                else
                {
                    Entity entFirst = byId.ent_First;
                    bool flag = false;
                    int num2 = -100;
                    infoError.index = 1;
                    infoError.createMessageAdd("- Get value fields: bsd_terminateletter,bsd_takeoutmoney");
                    if (entFirst.Contains("bsd_terminateletter"))
                        flag = entFirst.GetAttributeValue<bool>("bsd_terminateletter");
                    if (entFirst.Contains("bsd_takeoutmoney"))
                        num2 = ((OptionSetValue)entFirst["bsd_takeoutmoney"]).Value;
                    infoError.index = 2;
                    infoError.createMessageAdd(string.Format("- bsd_terminateletter: {0}", (object)flag));
                    infoError.createMessageAdd(string.Format("- bsd_takeoutmoney: {0}", (object)num2));
                    if (num2 == -100)
                        throw new Exception("[FUL] Take out money not value!");
                    if (flag)
                    {
                        infoError.index = 3;
                        infoError.createMessageAdd(string.Format("- Get money in FUL"));
                        if (entFirst.Contains("bsd_totalforfeitureamount"))
                            bsdFollowuplist.TotalForfeitureAmount = ((Money)entFirst["bsd_totalforfeitureamount"]).Value;
                        if (entFirst.Contains("bsd_totalamountpaid"))
                            bsdFollowuplist.TotalPaid = ((Money)entFirst["bsd_totalamountpaid"]).Value;
                        if (entFirst.Contains("bsd_forfeitureamount"))
                            bsdFollowuplist.Refund = ((Money)entFirst["bsd_forfeitureamount"]).Value;
                        switch (num2)
                        {
                            case 100000000:
                                infoError.index = 5;
                                infoError.createMessageAdd(string.Format("Case: Refund"));
                                Decimal num3 = bsdFollowuplist.TotalPaid - bsdFollowuplist.Refund;
                                if (num3 != bsdFollowuplist.TotalForfeitureAmount)
                                {
                                    infoError.index = 51;
                                    infoError.createMessageAdd(string.Format("#. Trace: Total Forfeiture Amount != Total Paid - Refund"));
                                    infoError.result = true;
                                    num1 = num3;
                                    break;
                                }
                                if (num3 < 0M)
                                {
                                    infoError.index = 52;
                                    infoError.createMessageAdd(string.Format("#. Trace: Total Paid - Refund < 0"));
                                    infoError.result = false;
                                    throw new Exception("Terminate Fee = Total Paid - Refund < 0 ");
                                }
                                infoError.result = true;
                                num1 = num3;
                                break;
                            case 100000001:
                                infoError.index = 4;
                                infoError.createMessageAdd(string.Format("Case: Forfeiture"));
                                infoError.result = true;
                                num1 = bsdFollowuplist.TotalForfeitureAmount;
                                break;
                            default:
                                infoError.index = 6;
                                infoError.createMessageAdd(string.Format("Case: Default"));
                                infoError.result = false;
                                num1 = 0M;
                                break;
                        }
                    }
                }
                infoError.createMessageAdd(string.Format("Return value: {0}", (object)num1));
                rs = infoError;
            }
            catch (Exception ex)
            {
                infoError.result = false;
                infoError.createMessageAdd(string.Format("#.Func amount_TerminateFee: index[{0}], message: {1}, exception: {2}", (object)infoError.index, (object)infoError.message, (object)ex.Message));
                rs = infoError;
                num1 = 0M;
            }
            return num1;
        }
    }
}
