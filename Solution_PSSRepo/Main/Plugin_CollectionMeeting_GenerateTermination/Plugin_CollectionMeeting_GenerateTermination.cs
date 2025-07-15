using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
namespace Plugin_CollectionMeeting_GenerateTermination
{
    public class Plugin_CollectionMeeting_GenerateTermination : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            traceService.Trace(string.Format("Context Depth {0}", context.Depth));
            if (context.Depth > 1)
                return;

            if (context.InputParameters.Contains("Target") && (context.InputParameters["Target"] is Entity))
            {
                Entity tar = (Entity)context.InputParameters["Target"];
                Guid targetId = tar.Id;

                if (tar.LogicalName == "appointment")
                {
                    traceService.Trace("Begin plugin");
                    if (context.MessageName == "Update" && tar.Contains("statecode") && ((OptionSetValue)tar["statecode"]).Value == 1)
                    {
                        EntityCollection l_FUL = findFuL(service, tar.ToEntityReference());
                        //throw new InvalidPluginExecutionException(l_FUL.Entities.Count.ToString());
                        foreach (Entity ful in l_FUL.Entities)
                        {
                            DateTime today = RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                            if (ful.Contains("bsd_type") && ((OptionSetValue)ful["statuscode"]).Value != 100000000)//statuscode = complete
                            {
                                //Cap nhat FuL >>> Complete
                                Entity follow = new Entity(ful.LogicalName);
                                follow.Id = ful.Id;
                                follow["statuscode"] = new OptionSetValue(100000000);
                                service.Update(follow);

                                #region Reservation - Sign off RF
                                if (((OptionSetValue)ful["bsd_type"]).Value == 100000000 && ful.Contains("bsd_reservation"))//Reservation - Sign off RF
                                {
                                    Entity res = service.Retrieve(((EntityReference)ful["bsd_reservation"]).LogicalName, ((EntityReference)ful["bsd_reservation"]).Id, new ColumnSet(new string[] { "statuscode", "bsd_reservationprinteddate", "bsd_signingexpired" }));
                                    if (res.Contains("bsd_reservationprinteddate") && ((OptionSetValue)res["statuscode"]).Value == 100000005 && res.Contains("bsd_signingexpired") && ful.Contains("bsd_expiredate"))
                                    {
                                        DateTime expired = (DateTime)res["bsd_signingexpired"];
                                        DateTime new_expired = (DateTime)ful["bsd_expiredate"];

                                        if (((int)(new_expired.Date.Subtract(expired.Date).TotalDays)) > 0)
                                        {
                                            //throw new InvalidPluginExecutionException((new_expired.Date.Subtract(expired.Date).TotalDays.ToString()));
                                            //Cap nhat expired tren Reservation
                                            //mo form
                                            SetStateRequest setStateRequest = new SetStateRequest()
                                            {
                                                EntityMoniker = new EntityReference
                                                {
                                                    Id = res.Id,
                                                    LogicalName = res.LogicalName
                                                },
                                                State = new OptionSetValue(0),
                                                Status = new OptionSetValue(100000000)
                                            };
                                            service.Execute(setStateRequest);

                                            //cap nhat expired of signing
                                            Entity reservation = new Entity(res.LogicalName);
                                            reservation.Id = res.Id;
                                            reservation["bsd_signingexpired"] = ful["bsd_expiredate"];
                                            service.Update(reservation);
                                            traceService.Trace("1");
                                            // cap nhat tinh trang thanh Deposited
                                            SetStateRequest setStateRequest1 = new SetStateRequest()
                                            {
                                                EntityMoniker = new EntityReference
                                                {
                                                    Id = res.Id,
                                                    LogicalName = res.LogicalName
                                                },
                                                State = new OptionSetValue(1),
                                                Status = new OptionSetValue(3)
                                            };
                                            service.Execute(setStateRequest1);
                                            traceService.Trace("2");
                                        }
                                    }
                                }

                                #endregion

                                #region Option entry - 1St installment OR Option entry - Installment
                                else if (((OptionSetValue)ful["bsd_type"]).Value == 100000002 || ((OptionSetValue)ful["bsd_type"]).Value == 100000004)
                                {
                                    if (ful.Contains("bsd_optionentry") && ful.Contains("bsd_installment") && ful.Contains("bsd_expiredate"))
                                    {
                                        //Cap nhat due date of installment
                                        Entity installment = new Entity(((EntityReference)ful["bsd_installment"]).LogicalName);
                                        installment.Id = ((EntityReference)ful["bsd_installment"]).Id;
                                        installment["bsd_duedate"] = ful["bsd_expiredate"];
                                        service.Update(installment);


                                    }
                                }
                                #endregion

                                #region Reservation - Terminate
                                else if (((OptionSetValue)ful["bsd_type"]).Value == 100000005)
                                {
                                    if (ful.Contains("bsd_reservation"))
                                    {
                                        Entity reser = service.Retrieve(((EntityReference)ful["bsd_reservation"]).LogicalName, ((EntityReference)ful["bsd_reservation"]).Id, new ColumnSet(new string[] 
                                        { "name", "customerid", "bsd_unitno", "bsd_projectid","bsd_salessgentcompany" }));
                                        //Tao refund
                                        //Entity Refund = new Entity("bsd_refund");
                                        //Refund["bsd_name"] = "Refund of " + (reser.Contains("name") ? reser["name"] : "");
                                        //Refund["bsd_followuplist"] = ful.ToEntityReference();
                                        //Refund["bsd_refundtype"] = new OptionSetValue(100000000);
                                        //if (reser.Contains("customerid"))
                                        //    Refund["bsd_customer"] = reser["customerid"];

                                        //if (reser.Contains("bsd_projectid"))
                                        //    Refund["bsd_project"] = reser["bsd_projectid"];

                                        //if (reser.Contains("bsd_unitno"))
                                        //    Refund["bsd_unitno"] = reser["bsd_unitno"];
                                        //Refund["bsd_reservation"] = ful["bsd_reservation"];
                                        //Refund["bsd_paymentactualtime"] = today;
                                        //decimal amountDeposit = tinhTienDeposited(service, (EntityReference)ful["bsd_reservation"]);
                                        //Refund["bsd_totalamountpaid"] = new Money(amountDeposit);
                                        //Refund["bsd_refundamount"] = new Money(amountDeposit);
                                        //Refund["bsd_refundableamount"] = new Money(amountDeposit);
                                        //service.Create(Refund);

                                        //----------------tạo termination--------------
                                        if (ful.Contains("bsd_termination") && (bool)ful["bsd_termination"] == true)
                                        {
                                            Entity termination = new Entity("bsd_termination");
                                            termination["bsd_name"] = "Termination of " + (ful.Contains("bsd_name") ? ful["bsd_name"] : "");
                                            termination["bsd_terminationdate"] = today;
                                            termination["bsd_terminationtype"] = false;
                                            if (reser.Contains("bsd_salessgentcompany"))
                                                termination["bsd_salesagentcompany"] = reser["bsd_salessgentcompany"];
                                            termination["bsd_source"] = new OptionSetValue(100000000);
                                            termination["bsd_followuplist"] = ful.ToEntityReference();
                                            termination["bsd_quotationreservation"] = ful["bsd_reservation"];
                                            termination["bsd_units"] = ful["bsd_units"];
                                            decimal totalpaid = ful.Contains("bsd_totalamountpaid") ? ((Money)ful["bsd_totalamountpaid"]).Value : 0;
                                            decimal paid = ful.Contains("bsd_totalamount") ? ((Money)ful["bsd_totalamount"]).Value : 0;
                                            decimal percent = 0, amount = 0, totalForfeiture = 0;
                                            if (((OptionSetValue)ful["bsd_takeoutmoney"]).Value == 100000001)
                                            {
                                                percent = ful.Contains("bsd_forfeiturepercent") ? ((decimal)ful["bsd_forfeiturepercent"]) : 0;
                                                termination["bsd_forfeiturepercent"] = percent;
                                                totalForfeiture = percent * paid / 100;
                                                termination["bsd_totalforfeitureamount"] = new Money(((Money)ful["bsd_totalforfeitureamount"]).Value);
                                                termination["bsd_refundamount"] = new Money(totalpaid-totalForfeiture);
                                                termination["bsd_receivedamount"] = new Money(totalpaid-totalForfeiture);
                                            }
                                            else if (((OptionSetValue)ful["bsd_takeoutmoney"]).Value == 100000000)
                                            {
                                                amount = ful.Contains("bsd_forfeitureamount") ? ((Money)ful["bsd_forfeitureamount"]).Value : 0;
                                                termination["bsd_forfeitureamount"] = new Money(amount);
                                                totalForfeiture = amount;
                                                termination["bsd_totalforfeitureamount"] = new Money(totalpaid-totalForfeiture);
                                                termination["bsd_refundamount"] = new Money(totalForfeiture);
                                                termination["bsd_receivedamount"] = new Money(totalForfeiture);
                                            }
                                            termination["bsd_totalamountpaid"] = new Money(totalpaid);
                                            if (ful.Contains("bsd_terminationtype"))
                                            {
                                                termination["bsd_terminationtypeful"] = ful["bsd_terminationtype"];
                                            }
                                            if (ful.Contains("bsd_description"))
                                            {
                                                termination["bsd_remark"] = ful["bsd_description"];
                                            }
                                            if (ful.Contains("bsd_resell"))
                                            {
                                                Boolean resel = (Boolean)ful["bsd_resell"];
                                                if(resel==true)
                                                {
                                                    termination["bsd_resell"] = true;
                                                    if(ful.Contains("bsd_phaselaunch"))
                                                    {
                                                        EntityReference phase = (EntityReference)ful["bsd_phaselaunch"];
                                                        EntityCollection PE = find_phase(service, phase);
                                                        if (PE.Entities.Count == 0)
                                                        {
                                                            termination["bsd_phaselaunch"] = phase;
                                                        }
                                                        else
                                                        {
                                                            throw new InvalidPluginExecutionException("Status of the Phase launch is not Launched, please check again.");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    termination["bsd_resell"] = false;
                                                }
                                            }                             
                                            //traceService.Trace(paid.ToString() + " " + percent.ToString() + " " + amount.ToString() + " " + totalForfeiture.ToString() + " " + (paid - totalForfeiture).ToString());
                                            service.Create(termination);
                                        }
                                        if (ful.Contains("bsd_terminateletter") && (bool)ful["bsd_terminateletter"] == true && (!ful.Contains("bsd_termination") || (bool)ful["bsd_termination"] == false))
                                        {
                                            //----------------tạo terminateletter--------------------
                                            Entity terminateLetter = new Entity("bsd_terminateletter");
                                            terminateLetter["bsd_name"] = "Terminate letter of " + (ful.Contains("bsd_name") ? ful["bsd_name"] : "");
                                            terminateLetter["bsd_subject"] = "Terminate letter - Follow Up List";
                                            terminateLetter["bsd_date"] = DateTime.Today;
                                            terminateLetter["bsd_terminatefee"] = new Money(0);
                                            terminateLetter["bsd_source"] = new OptionSetValue(100000000);
                                            EntityReference reserRef = (EntityReference)ful["bsd_reservation"];
                                            terminateLetter["bsd_reservation"] = reserRef;
                                            Entity rs = service.Retrieve(reserRef.LogicalName, reserRef.Id, new ColumnSet(new string[] { "customerid" }));
                                            if (rs.Contains("customerid"))
                                                terminateLetter["bsd_customer"] = (EntityReference)rs["customerid"];
                                            if (ful.Contains("bsd_project"))
                                                terminateLetter["bsd_project"] = ful["bsd_project"];
                                            terminateLetter["bsd_followuplist"] = ful.ToEntityReference();
                                            terminateLetter["bsd_units"] = ful["bsd_units"];
                                            terminateLetter["bsd_system"] = true;

                                            if (ful.Contains("bsd_takeoutmoney"))
                                            {
                                                if (((OptionSetValue)ful["bsd_takeoutmoney"]).Value == 100000001)//Forfeiture
                                                {
                                                    //decimal amountpaid = ful.Contains("bsd_totalamountpaid") ? ((Money)ful["bsd_totalamountpaid"]).Value : -1;
                                                    decimal amountpaid = ful.Contains("bsd_totalamount") ? ((Money)ful["bsd_totalamount"]).Value : -1;
                                                    decimal forfeiturePercent = ful.Contains("bsd_forfeiturepercent") ? (decimal)ful["bsd_forfeiturepercent"] : -1;

                                                    traceService.Trace("1");
                                                    if (amountpaid != -1 && forfeiturePercent != -1)
                                                        terminateLetter["bsd_totalforfeitureamount"] = new Money(((Money)ful["bsd_totalforfeitureamount"]).Value); 
                                                    traceService.Trace("2");
                                                }
                                                else if (((OptionSetValue)ful["bsd_takeoutmoney"]).Value == 100000000)//Refund
                                                {// field Refund Amount dislay name bsd_forfeitureamount
                                                    if (ful.Contains("bsd_forfeitureamount"))
                                                        terminateLetter["bsd_totalforfeitureamount"] = ful["bsd_forfeitureamount"];
                                                }
                                            }
                                            Entity Units = service.Retrieve(((EntityReference)ful["bsd_units"]).LogicalName, ((EntityReference)ful["bsd_units"]).Id, new ColumnSet(new string[] { "bsd_signedcontractdate" }));
                                            if (Units.Contains("bsd_signedcontractdate"))
                                                terminateLetter["bsd_signedcontractdate"] = Units["bsd_signedcontractdate"];

                                            service.Create(terminateLetter);
                                            //----------------tạo Follow up list COPY--------------------
                                            Entity FollowUpList = CloneEntity(ful);
                                            FollowUpList.Attributes.Remove("bsd_followuplistid");
                                            FollowUpList["bsd_name"] = ful["bsd_name"] + " - Copy";
                                            FollowUpList["bsd_date"] = today;
                                            FollowUpList["bsd_copy"] = true;
                                            FollowUpList["bsd_system"] = true;
                                            service.Create(FollowUpList);
                                        }
                                    }
                                }
                                #endregion

                                #region Option entry - Termination
                                else if (((OptionSetValue)ful["bsd_type"]).Value == 100000006 && ful.Contains("bsd_optionentry") && ful.Contains("bsd_units"))//option-termination
                                {
                                    traceService.Trace("Option entry - Termination");
                                    EntityReference optionE = (EntityReference)ful["bsd_optionentry"];
                                    Entity OE = service.Retrieve(optionE.LogicalName, optionE.Id, new ColumnSet(new string[] { "customerid", "bsd_salesagentcompany" }));
                                    //----------------tạo termination--------------
                                    traceService.Trace((ful.Contains("bsd_termination") && (bool)ful["bsd_termination"] == true).ToString());
                                    if (ful.Contains("bsd_termination") && (bool)ful["bsd_termination"] == true)
                                    {
                                        //if (ful.Contains("bsd_termination"))
                                        //    throw new InvalidPluginExecutionException(((bool)ful["bsd_termination"]).ToString()+"dsđsddsds");
                                        decimal bsd_maintenancefeepaid = ful.Contains("bsd_maintenancefeepaid") ? ((Money)ful["bsd_maintenancefeepaid"]).Value : 0;
                                        decimal bsd_managementfeepaid = ful.Contains("bsd_managementfeepaid") ? ((Money)ful["bsd_managementfeepaid"]).Value : 0;
                                        traceService.Trace("bsd_maintenancefeepaid: " + bsd_maintenancefeepaid.ToString());
                                        traceService.Trace("bsd_managementfeepaid: " + bsd_managementfeepaid.ToString());
                                        Entity termination = new Entity("bsd_termination");
                                        termination["bsd_name"] = "Termination of " + (ful.Contains("bsd_name") ? ful["bsd_name"] : "");
                                        termination["bsd_terminationdate"] = DateTime.Now.AddHours(7).Date;
                                        termination["bsd_terminationtype"] = false;
                                        if (OE.Contains("bsd_salesagentcompany"))
                                            termination["bsd_salesagentcompany"] = OE["bsd_salesagentcompany"];
                                        termination["bsd_source"] = new OptionSetValue(100000001);
                                        termination["bsd_followuplist"] = ful.ToEntityReference();
                                        termination["bsd_optionentry"] = ful["bsd_optionentry"];
                                        termination["bsd_units"] = ful["bsd_units"];
                                        termination["bsd_maintenancefeepaid"] = ful["bsd_maintenancefeepaid"];
                                        termination["bsd_managementfeepaid"] = ful["bsd_managementfeepaid"];

                                        decimal totalpaid = ful.Contains("bsd_totalamountpaid") ? ((Money)ful["bsd_totalamountpaid"]).Value : 0;
                                        decimal paid = ful.Contains("bsd_totalamount") ? ((Money)ful["bsd_totalamount"]).Value : 0;
                                        decimal percent = 0, amount = 0, totalForfeiture = 0;
                                        if (((OptionSetValue)ful["bsd_takeoutmoney"]).Value == 100000001)
                                        {
                                            percent = ful.Contains("bsd_forfeiturepercent") ? ((decimal)ful["bsd_forfeiturepercent"]) : 0;
                                            termination["bsd_forfeiturepercent"] = percent;
                                            totalForfeiture = percent * paid / 100;
                                            termination["bsd_totalforfeitureamount"] = new Money(((Money)ful["bsd_totalforfeitureamount"]).Value);
                                            termination["bsd_refundamount"] = new Money(totalpaid - totalForfeiture + bsd_maintenancefeepaid + bsd_managementfeepaid);
                                            termination["bsd_receivedamount"] = new Money(totalpaid - totalForfeiture + bsd_maintenancefeepaid + bsd_managementfeepaid); ;
                                        }
                                        else if (((OptionSetValue)ful["bsd_takeoutmoney"]).Value == 100000000)
                                        {
                                            amount = ful.Contains("bsd_forfeitureamount") ? ((Money)ful["bsd_forfeitureamount"]).Value : 0;
                                            termination["bsd_forfeitureamount"] = new Money(amount);
                                            totalForfeiture = amount;
                                            termination["bsd_totalforfeitureamount"] = new Money(totalpaid-totalForfeiture);
                                            termination["bsd_refundamount"] = new Money(totalForfeiture + bsd_maintenancefeepaid + bsd_managementfeepaid);
                                            termination["bsd_receivedamount"] = new Money(totalForfeiture + bsd_maintenancefeepaid + bsd_managementfeepaid);
                                        }
                                        termination["bsd_totalamountpaid"] = new Money(totalpaid);
                                        if (ful.Contains("bsd_terminationtype"))
                                        {
                                            termination["bsd_terminationtypeful"] = ful["bsd_terminationtype"];
                                        }
                                        if (ful.Contains("bsd_description"))
                                        {
                                            termination["bsd_remark"] = ful["bsd_description"];
                                        }
                                        if (ful.Contains("bsd_resell"))
                                        {
                                            Boolean resel = (Boolean)ful["bsd_resell"];
                                            if (resel == true)
                                            {
                                                termination["bsd_resell"] = true;
                                                if (ful.Contains("bsd_phaselaunch"))
                                                {
                                                    EntityReference phase = (EntityReference)ful["bsd_phaselaunch"];
                                                    EntityCollection PE = find_phase(service, phase);
                                                    if(PE.Entities.Count==0)
                                                    {
                                                        termination["bsd_phaselaunch"] = phase;
                                                    }
                                                    else
                                                    {
                                                        throw new InvalidPluginExecutionException("Status of the Phase launch is not Launched, please check again.");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                termination["bsd_resell"] = false;
                                            }
                                        }
                                        traceService.Trace(paid.ToString() + " " + percent.ToString() + " " + amount.ToString() + " " + totalForfeiture.ToString() + " " + (paid - totalForfeiture).ToString());
                                        service.Create(termination);
                                    }


                                    //if (ful.Contains("bsd_optionentry"))
                                    //{
                                    //    Entity oe = new Entity(((EntityReference)ful["bsd_optionentry"]).LogicalName);
                                    //    oe.Id = ((EntityReference)ful["bsd_optionentry"]).Id;
                                    //    oe["statuscode"] = new OptionSetValue(100000006);
                                    //    service.Update(oe);

                                    if (ful.Contains("bsd_terminateletter") && (bool)ful["bsd_terminateletter"] == true && (!ful.Contains("bsd_termination") || (bool)ful["bsd_termination"] == false))
                                    {
                                        //----------------tạo terminateletter--------------------
                                        Entity terminateLetter = new Entity("bsd_terminateletter");
                                        terminateLetter["bsd_name"] = "Terminate letter of " + (ful.Contains("bsd_name") ? ful["bsd_name"] : "");
                                        terminateLetter["bsd_subject"] = "Terminate letter - Follow Up List";
                                        terminateLetter["bsd_date"] = DateTime.Today;
                                        terminateLetter["bsd_terminatefee"] = new Money(0);

                                        terminateLetter["bsd_optionentry"] = optionE;
                                        terminateLetter["bsd_source"] = new OptionSetValue(100000001);
                                        if (OE.Contains("customerid"))
                                            terminateLetter["bsd_customer"] = (EntityReference)OE["customerid"];
                                        if (ful.Contains("bsd_project"))
                                            terminateLetter["bsd_project"] = ful["bsd_project"];
                                        terminateLetter["bsd_followuplist"] = ful.ToEntityReference();
                                        terminateLetter["bsd_units"] = ful["bsd_units"];
                                        terminateLetter["bsd_system"] = true;
                                        if (ful.Contains("bsd_takeoutmoney"))
                                        {
                                            if (((OptionSetValue)ful["bsd_takeoutmoney"]).Value == 100000001)//Forfeiture
                                            {
                                                //decimal amountpaid = ful.Contains("bsd_totalamountpaid") ? ((Money)ful["bsd_totalamountpaid"]).Value : -1;
                                                decimal amountpaid = ful.Contains("bsd_totalamount") ? ((Money)ful["bsd_totalamount"]).Value : -1;
                                                decimal forfeiturePercent = ful.Contains("bsd_forfeiturepercent") ? (decimal)ful["bsd_forfeiturepercent"] : -1;

                                                traceService.Trace("1");
                                                if (amountpaid != -1 && forfeiturePercent != -1)
                                                    terminateLetter["bsd_totalforfeitureamount"] = new Money(((Money)ful["bsd_totalforfeitureamount"]).Value);
                                                traceService.Trace("2");
                                            }
                                            else if (((OptionSetValue)ful["bsd_takeoutmoney"]).Value == 100000000)//Refund
                                            {
                                                if (ful.Contains("bsd_forfeitureamount"))
                                                    terminateLetter["bsd_totalforfeitureamount"] = ful["bsd_forfeitureamount"];
                                            }
                                        }
                                        Entity Units = service.Retrieve(((EntityReference)ful["bsd_units"]).LogicalName, ((EntityReference)ful["bsd_units"]).Id, new ColumnSet(new string[] { "bsd_signedcontractdate" }));
                                        if (Units.Contains("bsd_signedcontractdate"))
                                            terminateLetter["bsd_signedcontractdate"] = Units["bsd_signedcontractdate"];

                                        service.Create(terminateLetter);
                                        //----------------tạo Follow up list COPY--------------------
                                        Entity FollowUpList = CloneEntity(ful);
                                        FollowUpList.Attributes.Remove("bsd_followuplistid");
                                        FollowUpList.Attributes.Remove("createdon");
                                        FollowUpList["bsd_name"] = ful["bsd_name"] + " - Copy";
                                        //if (ful.Contains("bsd_terminateletter"))
                                        //    FollowUpList["bsd_terminateletter"] = ful["bsd_terminateletter"];
                                        //if (ful.Contains("bsd_termination"))
                                        //    FollowUpList["bsd_termination"] = ful["bsd_termination"];
                                        //if (ful.Contains("bsd_group"))
                                        //    FollowUpList["bsd_group"] = ful["bsd_group"];
                                        //if (ful.Contains("bsd_owneroptionreservation"))
                                        //    FollowUpList["bsd_owneroptionreservation"] = ful["bsd_owneroptionreservation"];
                                        //if (ful.Contains("bsd_project"))
                                        //    FollowUpList["bsd_project"] = ful["bsd_project"];
                                        FollowUpList["bsd_date"] = today;
                                        FollowUpList["bsd_copy"] = true;
                                        FollowUpList["bsd_system"] = true;
                                        service.Create(FollowUpList);
                                    }

                                }
                                #endregion

                            }
                        }
                        //throw new InvalidPluginExecutionException(l_FUL.Entities.Count.ToString());
                    }

                }
            }
        }
        //private EntityCollection findFUL(EntityReference rsv)
        //{
        //    QueryExpression q = new QueryExpression("bsd_followuplist");
        //    q.ColumnSet = new ColumnSet(new string[] { "bsd_followuplistid", "bsd_type", "bsd_name", "bsd_optionentry", "bsd_units", "bsd_takeoutmoney", "bsd_totalamountpaid", "bsd_forfeiturepercent", "bsd_forfeitureamount", "statuscode" });
        //    q.Criteria = new FilterExpression(LogicalOperator.And);
        //    q.Criteria.AddCondition(new ConditionExpression("bsd_collectionmeeting", ConditionOperator.Equal, rsv.Id));
        //    //q.Criteria.AddCondition(new ConditionExpression("bsd_type", ConditionOperator.Equal, 100000006));
        //    EntityCollection etnc = service.RetrieveMultiple(q);
        //    return etnc;
        private Entity CloneEntity(Entity input)
        {
            Entity outPut = new Entity(input.LogicalName);
            foreach (string fieldName in input.Attributes.Keys)
            {
                outPut[fieldName] = input[fieldName];
            }
            return outPut;
        }
        private EntityCollection findFuL(IOrganizationService service, EntityReference CM)
        {
            string fetXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_followuplist'>
                    <all-attributes/>
                    <order attribute='bsd_name' descending='false' />
                    <filter type='and'>
                      <condition attribute='bsd_collectionmeeting' operator='eq'  uitype='appointment' value='{0}' />
                    </filter>
                  </entity>
                </fetch>";
            fetXml = string.Format(fetXml, CM.Id);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetXml));
            return entc;
        }
        private decimal tinhTienDeposited(IOrganizationService service, EntityReference res)
        {
            decimal tong = 0;
            string fetXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' aggregate='true'>
                                  <entity name='bsd_payment'>
                                    <attribute name='bsd_amountpay' aggregate='sum' alias='sum'/>
                                    <filter type='and'>
                                      <condition attribute='statuscode' operator='eq' value='100000000' />
                                      <condition attribute='bsd_reservation' operator='eq'  uitype='quote' value='{0}' />
                                    </filter>
                                  </entity>
                                </fetch>";
            fetXml = string.Format(fetXml, res.Id);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetXml));
            foreach (Entity en in entc.Entities)
            {
                if (en.Contains("sum"))
                {
                    AliasedValue asum = (AliasedValue)en["sum"];
                    tong += asum.Value != null ? ((Money)asum.Value).Value : 0;
                }
                break;
            }
            return tong;
        }
        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcTime, IOrganizationService service)
        {
            var currentUserSettings = service.RetrieveMultiple(
           new QueryExpression("usersettings")
           {
               ColumnSet = new ColumnSet("localeid", "timezonecode"),
               Criteria = new FilterExpression
               {
                   Conditions =
           {
            new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
           }
               }
           }).Entities[0].ToEntity<Entity>();

            int? timeZoneCode = (int?)currentUserSettings.Attributes["timezonecode"];
            if (!timeZoneCode.HasValue)
                throw new Exception("Can't find time zone code");

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcTime.ToUniversalTime()
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);

            return response.LocalTime;
            //var utcTime = utcTime.ToString("MM/dd/yyyy HH:mm:ss");
            //var localDateOnly = response.LocalTime.ToString("dd-MM-yyyy");
        }
        private EntityCollection find_phase(IOrganizationService service, EntityReference phase)
        {
            string fetXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_phaseslaunch'>
                    <attribute name='bsd_name'/>
                    <attribute name='bsd_phaseslaunchid'/>
                    <filter type='and'>
                   <condition attribute='bsd_phaseslaunchid' operator='eq' value='{0}'/>
                    <condition attribute='statuscode' operator='ne' value='100000000'/>
                    </filter>
                  </entity>
                </fetch>";
            fetXml = string.Format(fetXml, phase.Id);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetXml));
            return entc;
        }

    }

}
