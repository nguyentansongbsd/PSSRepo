using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
namespace Action_WarningNotices_GenerateWarningNotices
{
    public class Action_WarningNotices_GenerateWarningNotices : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceService = null;

        private string owner = null;
        private string genWNId = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            traceService.Trace(string.Format("Context Depth {0}", context.Depth));
            //string pro = context.InputParameters["Project"].ToString();
            //string blo = context.InputParameters["Block"].ToString();
            //string flo = context.InputParameters["Floor"].ToString();
            //string units = context.InputParameters["Units"].ToString();
            traceService.Trace("1111");
            string optionEntryId = context.InputParameters.Contains("optionEntryId") ? context.InputParameters["optionEntryId"].ToString() : null;
            traceService.Trace("2222");
            string date = "";
            if (context.InputParameters["Date"] != null)
            {
                date = context.InputParameters["Date"].ToString();
            }
            if (context.InputParameters["Owner"] != null)
            {
                owner = context.InputParameters["Owner"].ToString().Replace("{", "").Replace("}", "");
            }
            if (context.InputParameters["GenWNId"] != null)
            {
                genWNId = context.InputParameters["GenWNId"].ToString().Replace("{", "").Replace("}", "");
            }
            traceService.Trace("date: " + date);
            //EntityCollection l_OptionEntry = findOptionEntry(service, pro, blo, flo, units);
            traceService.Trace("1");
            Entity OE = service.Retrieve("salesorder", Guid.Parse(optionEntryId), new ColumnSet(new string[] { "bsd_paymentscheme", "name",
                "customerid","bsd_project","bsd_unitnumber"}));
            int dem = 0;
            Entity PS = findPaymentScheme(service, (EntityReference)OE["bsd_paymentscheme"]);
            traceService.Trace("2");
            int PN_Date = -1;
            if (PS != null)
            {
                if (PS.Contains("bsd_warningnotices1date") || PS.Contains("bsd_warningnotices2date") || PS.Contains("bsd_warningnotices3date") || PS.Contains("bsd_warningnotices4date"))
                {
                    traceService.Trace("3");
                    #region INSTALLMENT
                    EntityCollection l_PSD = findPaymentSchemeDetail(service, OE);
                    foreach (Entity PSD in l_PSD.Entities)
                    {
                        int nday = (int)(DateTime.Now.AddHours(7).Date.Subtract(((DateTime)PSD["bsd_duedate"]).AddHours(7).Date).TotalDays);
                        if (nday > 0)
                        {
                            EntityCollection L_warning = findWarningNotices(service, PSD);
                            #region Da Generate WN
                            if (L_warning.Entities.Count > 0)
                            {
                                traceService.Trace("Da gen");
                                Entity warning = L_warning[0];
                                int numberofWarning = warning.Contains("bsd_numberofwarning") ? (int)warning["bsd_numberofwarning"] : -1;
                                if (numberofWarning > 0 && numberofWarning < 4)
                                {
                                    //numberofWarning == 1 ? "bsd_warningnotices2date" : (numberofWarning == 2 ? "bsd_warningnotices3date" : (numberofWarning == 3 ? "bsd_warningnotices4date" : null));
                                    string warningdate = "bsd_warningnotices" + (numberofWarning + 1) + "date";
                                    if (PS.Contains(warningdate) && nday >= (int)PS[warningdate])
                                    {
                                        EntityCollection l_war = findWarningNoticesByNumberOfWarning(service, PSD, (numberofWarning + 1));
                                        if (l_war.Entities.Count == 0)
                                        {
                                            Entity warningNotices = new Entity("bsd_warningnotices");
                                            if (OE.Contains("name"))
                                                warningNotices["bsd_name"] = "Warning Notices of " + OE["name"];
                                            else warningNotices["bsd_name"] = "Warning Notices";
                                            warningNotices["bsd_subject"] = "Warning Notices";
                                            warningNotices["bsd_optionentry"] = OE.ToEntityReference();
                                            if (OE.Contains("customerid"))
                                                warningNotices["bsd_customer"] = OE["customerid"];
                                            if (OE.Contains("bsd_project"))
                                                warningNotices["bsd_project"] = OE["bsd_project"];
                                            if (OE.Contains("bsd_unitnumber"))
                                                warningNotices["bsd_units"] = OE["bsd_unitnumber"];
                                            warningNotices["bsd_numberofwarning"] = numberofWarning + 1;
                                            warningNotices["bsd_type"] = new OptionSetValue(100000000);

                                            if (PSD.Contains("bsd_balance"))
                                            {
                                                decimal amount = PSD.Contains("bsd_balance") ? ((Money)PSD["bsd_balance"]).Value : 0;
                                                warningNotices["bsd_amount"] = new Money(amount);
                                            }
                                            warningNotices["bsd_date"] = !string.IsNullOrWhiteSpace(date) ? Convert.ToDateTime(date) : DateTime.Now; //RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                                            warningNotices["bsd_paymentschemedeitail"] = PSD.ToEntityReference();
                                            if (PSD.Contains("bsd_duedate"))
                                            {
                                                warningNotices["bsd_duedate"] = ((DateTime)PSD["bsd_duedate"]);
                                                int graceday = findGraceDays(service, PS.ToEntityReference());
                                                if (graceday != -1)
                                                    warningNotices["bsd_estimateduedate"] = ((DateTime)PSD["bsd_duedate"]).AddDays(graceday);
                                            }

                                            if (!string.IsNullOrWhiteSpace(owner))
                                            {
                                                traceService.Trace("owner1: " + owner);
                                                warningNotices["ownerid"] = new EntityReference("systemuser", Guid.Parse(owner));
                                            }
                                            if (!string.IsNullOrWhiteSpace(genWNId))
                                            {
                                                traceService.Trace("Gen WN Id: " + genWNId);
                                                warningNotices["bsd_generatewarningnotices"] = new EntityReference("bsd_genaratewarningnotices", Guid.Parse(genWNId));
                                            }

                                            #region bsd_deadlinewn1-bsd_deadlinewn2
                                            if (PSD.Contains("bsd_duedate"))
                                            {
                                                traceService.Trace("@@@");
                                                if (PSD.Contains("bsd_gracedays"))
                                                {
                                                    traceService.Trace("1@@@");
                                                    warningNotices["bsd_deadlinewn1"] = ((DateTime)PSD["bsd_duedate"]).AddHours(7).AddDays((int)PSD["bsd_gracedays"]);
                                                }
                                                else
                                                {
                                                    var bsd_paymentscheme = service.Retrieve("bsd_paymentscheme", ((EntityReference)PSD["bsd_paymentscheme"]).Id, new ColumnSet("bsd_interestratemaster"));
                                                    var bsd_interestratemaster = service.Retrieve("bsd_interestratemaster", ((EntityReference)bsd_paymentscheme["bsd_interestratemaster"]).Id, new ColumnSet("bsd_gracedays"));
                                                    warningNotices["bsd_deadlinewn1"] = ((DateTime)PSD["bsd_duedate"]).AddHours(7).AddDays((int)bsd_interestratemaster["bsd_gracedays"]);
                                                }
                                                warningNotices["bsd_deadlinewn2"] = ((DateTime)PSD["bsd_duedate"]).AddDays(60).AddHours(7);
                                            }
                                            traceService.Trace("11111: ");
                                            #endregion
                                            #region xử lý OdernumberE
                                            if (((int)PSD["bsd_ordernumber"]) == 1)
                                            {
                                                warningNotices["bsd_odernumber_e"] = "1st";
                                            }
                                            else if (((int)PSD["bsd_ordernumber"]) == 2)
                                            {
                                                warningNotices["bsd_odernumber_e"] = "2nd";
                                            }
                                            else if (((int)PSD["bsd_ordernumber"]) == 3)
                                            {
                                                warningNotices["bsd_odernumber_e"] = "3rd";

                                            }
                                            else
                                            {
                                                warningNotices["bsd_odernumber_e"] = $"{((int)PSD["bsd_ordernumber"])}th";
                                            }
                                            #endregion
                                            Guid id = service.Create(warningNotices);
                                            copy_CownerForOE(OE.Id, id, warningNotices.LogicalName, "bsd_warningnotice");
                                            var enWRN = service.Retrieve("bsd_warningnotices", id, new ColumnSet("bsd_date", "bsd_noticesnumber"));
                                            dem++;

                                            Entity ins = new Entity(PSD.LogicalName);
                                            ins.Id = PSD.Id;
                                            string field = "bsd_warningnotices" + (numberofWarning + 1);
                                            ins[field] = true;
                                            traceService.Trace("step1");
                                            string field2 = "bsd_warningdate" + (numberofWarning + 1);
                                            ins[field2] = ((DateTime)enWRN["bsd_date"]).AddHours(7);
                                            traceService.Trace("step2");

                                            string field3 = "bsd_w_noticesnumber" + (numberofWarning + 1);
                                            ins[field3] = enWRN["bsd_noticesnumber"];
                                            traceService.Trace("step3");

                                            service.Update(ins);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region Chua Generate WN
                            else
                            {
                                traceService.Trace("Chua gen");
                                PN_Date = PS.Contains("bsd_warningnotices1date") ? (int)PS["bsd_warningnotices1date"] : -1;
                                if (PN_Date >= 0 && nday >= PN_Date)
                                {
                                    Entity warningNotices = new Entity("bsd_warningnotices");
                                    if (OE.Contains("name"))
                                        warningNotices["bsd_name"] = "Warning Notices of " + OE["name"];
                                    else warningNotices["bsd_name"] = "Warning Notices";
                                    warningNotices["bsd_subject"] = "Warning Notices";
                                    warningNotices["bsd_optionentry"] = OE.ToEntityReference();
                                    if (OE.Contains("customerid"))
                                        warningNotices["bsd_customer"] = OE["customerid"];
                                    if (OE.Contains("bsd_project"))
                                        warningNotices["bsd_project"] = OE["bsd_project"];
                                    if (OE.Contains("bsd_unitnumber"))
                                        warningNotices["bsd_units"] = OE["bsd_unitnumber"];
                                    warningNotices["bsd_numberofwarning"] = 1;
                                    warningNotices["bsd_type"] = new OptionSetValue(100000000);
                                    if (PSD.Contains("bsd_balance"))
                                    {
                                        decimal amount = PSD.Contains("bsd_balance") ? ((Money)PSD["bsd_balance"]).Value : 0;
                                        warningNotices["bsd_amount"] = new Money(amount);
                                    }
                                    warningNotices["bsd_date"] = !string.IsNullOrWhiteSpace(date) ? Convert.ToDateTime(date) : DateTime.Now; //RetrieveLocalTimeFromUTCTime(DateTime.Now, service);
                                    warningNotices["bsd_paymentschemedeitail"] = PSD.ToEntityReference();
                                    if (PSD.Contains("bsd_duedate"))
                                    {
                                        warningNotices["bsd_duedate"] = ((DateTime)PSD["bsd_duedate"]);
                                        int graceday = findGraceDays(service, PS.ToEntityReference());
                                        if (graceday != -1)
                                            warningNotices["bsd_estimateduedate"] = ((DateTime)PSD["bsd_duedate"]).AddDays(graceday);
                                    }
                                    if (!string.IsNullOrWhiteSpace(owner))
                                    {
                                        traceService.Trace("owner2: " + owner);
                                        warningNotices["ownerid"] = new EntityReference("systemuser", Guid.Parse(owner));
                                    }
                                    if (!string.IsNullOrWhiteSpace(genWNId))
                                    {
                                        traceService.Trace("Gen WN Id: " + genWNId);
                                        warningNotices["bsd_generatewarningnotices"] = new EntityReference("bsd_genaratewarningnotices", Guid.Parse(genWNId));
                                    }

                                    #region bsd_deadlinewn1-bsd_deadlinewn2
                                    if (PSD.Contains("bsd_duedate"))
                                    {
                                        if (PSD.Contains("bsd_gracedays"))
                                        {
                                            traceService.Trace("1@@@");
                                            warningNotices["bsd_deadlinewn1"] = ((DateTime)PSD["bsd_duedate"]).AddHours(7).AddDays((int)PSD["bsd_gracedays"]);
                                        }
                                        else
                                        {
                                            var bsd_paymentscheme = service.Retrieve("bsd_paymentscheme", ((EntityReference)PSD["bsd_paymentscheme"]).Id, new ColumnSet("bsd_interestratemaster"));
                                            var bsd_interestratemaster = service.Retrieve("bsd_interestratemaster", ((EntityReference)bsd_paymentscheme["bsd_interestratemaster"]).Id, new ColumnSet("bsd_gracedays"));
                                            warningNotices["bsd_deadlinewn1"] = ((DateTime)PSD["bsd_duedate"]).AddHours(7).AddDays((int)bsd_interestratemaster["bsd_gracedays"]);
                                        }
                                        warningNotices["bsd_deadlinewn2"] = ((DateTime)PSD["bsd_duedate"]).AddDays(60).AddHours(7);


                                    }

                                    #endregion
                                    #region xử lý OdernumberE
                                    if (((int)PSD["bsd_ordernumber"]) == 1)
                                    {
                                        warningNotices["bsd_odernumber_e"] = "1st";
                                    }
                                    else if (((int)PSD["bsd_ordernumber"]) == 2)
                                    {
                                        warningNotices["bsd_odernumber_e"] = "2nd";
                                    }
                                    else if (((int)PSD["bsd_ordernumber"]) == 3)
                                    {
                                        warningNotices["bsd_odernumber_e"] = "3rd";

                                    }
                                    else
                                    {
                                        warningNotices["bsd_odernumber_e"] = $"{((int)PSD["bsd_ordernumber"])}th";
                                    }
                                    #endregion
                                    Guid id = service.Create(warningNotices);
                                    copy_CownerForOE(OE.Id, id, warningNotices.LogicalName, "bsd_warningnotice");
                                    var enWRN = service.Retrieve("bsd_warningnotices", id, new ColumnSet("bsd_date", "bsd_noticesnumber"));
                                    dem++;

                                    Entity ins = new Entity(PSD.LogicalName);
                                    ins.Id = PSD.Id;
                                    string field = "bsd_warningnotices1";
                                    ins[field] = true;
                                    traceService.Trace("step1z");
                                    string field2 = "bsd_warningdate" + (1);
                                    ins[field2] = ((DateTime)enWRN["bsd_date"]).AddHours(7);
                                    traceService.Trace("step2z");

                                    string field3 = "bsd_w_noticesnumber" + (1);
                                    ins[field3] = enWRN["bsd_noticesnumber"];
                                    traceService.Trace("step3z");
                                    service.Update(ins);
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
            traceService.Trace("dem: " + dem);
            context.OutputParameters["returnN"] = dem.ToString();
        }
        private void copy_CownerForOE(Guid idSource, Guid id, string localName, string fieldName)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_coowner"">
                    <filter>
                      <condition attribute=""bsd_optionentry"" operator=""eq"" value=""{idSource}"" />
                      <condition attribute=""bsd_current"" operator=""eq"" value=""1"" />
                    </filter>
                  </entity>
                </fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count <= 0) return;
            foreach (var item in result.Entities)
            {
                Entity it = new Entity(item.LogicalName);
                item.Attributes.Remove(item.LogicalName + "id");
                if(item.Contains("bsd_optionentry"))
                    item.Attributes.Remove("bsd_optionentry");
                if (item.Contains("bsd_reservation"))
                    item.Attributes.Remove("bsd_reservation");
                if (item.Contains("ownerid"))
                    item.Attributes.Remove("ownerid");
                item[fieldName] = new EntityReference(localName, id);
                item.Id = Guid.NewGuid();
                it = item;
                service.Create(it);
            }
        }
        EntityCollection RetrieveMultiRecord(IOrganizationService crmservices, string entity, ColumnSet column, string condition, object value)
        {
            QueryExpression q = new QueryExpression(entity);
            q.ColumnSet = column;
            q.Criteria = new FilterExpression();
            q.Criteria.AddCondition(new ConditionExpression(condition, ConditionOperator.Equal, value));
            EntityCollection entc = service.RetrieveMultiple(q);

            return entc;
        }
        private EntityCollection findPaymentSchemeDetail(IOrganizationService crmservices, Entity oe)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='bsd_paymentschemedetail'>
                    <attribute name='bsd_paymentschemedetailid' />
                    <attribute name='bsd_duedate' />
                    <attribute name='bsd_balance' />
                    <attribute name='bsd_ordernumber' />
                    <attribute name='bsd_gracedays' />

                    <attribute name='bsd_paymentscheme' />


                    <order attribute='bsd_duedate' descending='false' />
                    <filter type='and'>
                      <condition attribute='bsd_optionentry' operator='eq'  uitype='salesorder' value='{0}' />
                        <condition attribute='statuscode' operator='eq' value='100000000' />
                      <condition attribute='bsd_duedate' operator='not-null' />
                    </filter>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findunits(IOrganizationService crmservices, Entity oe)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
              <entity name='product'>
                <attribute name='name' />
                <attribute name='productid' />
                <attribute name='productnumber' />
                <attribute name='bsd_estimatehandoverdate' />
                <order attribute='productnumber' descending='false' />
                <filter type='and'>
                  <condition attribute='bsd_estimatehandoverdate' operator='not-null' />
                </filter>
                <link-entity name='salesorderdetail' from='productid' to='productid' alias='ac'>
                  <filter type='and'>
                    <condition attribute='salesorderid' operator='eq' uitype='salesorder' value='{0}' />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, oe.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private Entity findPaymentScheme(IOrganizationService crmservices, EntityReference ps)
        {
            Entity enPaymentScheme = service.Retrieve(ps.LogicalName, ps.Id, new ColumnSet("bsd_paymentschemeid", "bsd_warningnotices4date",
                "bsd_warningnotices3date", "bsd_warningnotices2date", "bsd_warningnotices1date"));
            return enPaymentScheme;

        }
        private EntityCollection findOptionEntry(IOrganizationService crmservices, string project, string block, string floor, string units)
        {
            string condition = "";
            traceService.Trace("project :" + project);
            if (project != "")
            {
                condition += "<condition attribute='bsd_projectcode' operator='eq' value='" + project + @"' />";
            }
            traceService.Trace("condition :" + condition);
            if (block != "")
            {
                condition += "<condition attribute='bsd_blocknumber' operator='eq' value='" + block + @"' />";
            }
            if (floor != "")
            {
                condition += "<condition attribute='bsd_floor' operator='eq' value='" + floor + @"' />";
            }
            if (units != "")
            {
                condition += "<condition attribute='productid' operator='eq' value='" + units + @"' />";
            }

            string fetchXMl = @"<fetch>
              <entity name='salesorder' >
                <all-attributes/>
                <filter type='and' >
                  <condition attribute='statuscode' operator='in' >
                    <value>100000001</value>
                    <value>100000003</value>
                    <value>100000000</value>
                    <value>100000002</value>
                    <value>100000005</value>
                  </condition>
                  <condition attribute='bsd_paymentscheme' operator='not-null' />
                  <condition attribute='customerid' operator='not-null' />
                  <condition attribute='totalamount' operator='gt' value='0' />
                  <condition attribute='bsd_terminationletter' operator='neq' value='1' />
                </filter>
                <link-entity name='product' from='productid' to='bsd_unitnumber' >
                  <filter>
                    '" + condition + @"'
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            traceService.Trace("findOptionEntry :" + fetchXMl);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXMl));
            return entc;
        }
        private EntityCollection findWarningNotices(IOrganizationService crmservices, Entity paymentDet)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_warningnotices'>
                <attribute name='bsd_warningnoticesid' />
                <attribute name='bsd_date' />
                <attribute name='bsd_numberofwarning' />
                <order attribute='bsd_numberofwarning' descending='true' />
                <filter type='and'>
                  <condition attribute='bsd_paymentschemedeitail' operator='eq' uitype='bsd_paymentschemedetail' value='{0}' />
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, paymentDet.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findWarningNoticesByNumberOfWarning(IOrganizationService crmservices, Entity paymentDet, int num)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_warningnotices'>
                <attribute name='bsd_warningnoticesid' />
                <attribute name='bsd_date' />
                <attribute name='bsd_numberofwarning' />
                <order attribute='bsd_numberofwarning' descending='true' />
                <filter type='and'>
                  <condition attribute='bsd_paymentschemedeitail' operator='eq' uitype='bsd_paymentschemedetail' value='{0}' />
                  <condition attribute='bsd_numberofwarning' operator='eq'  value='{1}' />
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, paymentDet.Id, num);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findWarningNotices_Units(IOrganizationService crmservices, Entity units)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_warningnotices'>
                <attribute name='bsd_warningnoticesid' />
                <attribute name='bsd_date' />
                <attribute name='bsd_numberofwarning' />
                <order attribute='bsd_numberofwarning' descending='true' />
                <filter type='and'>
                  <condition attribute='bsd_units' operator='eq' uitype='product' value='{0}' />
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, units.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findWarningNotices_Units_ByNumberOfWarning(IOrganizationService crmservices, Entity units, int num)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_warningnotices'>
                <attribute name='bsd_warningnoticesid' />
                <attribute name='bsd_date' />
                <attribute name='bsd_numberofwarning' />
                <order attribute='bsd_numberofwarning' descending='true' />
                <filter type='and'>
                  <condition attribute='bsd_units' operator='eq' uitype='product' value='{0}' />
                  <condition attribute='bsd_numberofwarning' operator='eq' value='{1}' />
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, units.Id, num);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private EntityCollection findWarningNotices_TODAY(IOrganizationService crmservices)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='bsd_warningnotices'>
                <attribute name='bsd_warningnoticesid' />
                <attribute name='createdon' />
                <order attribute='createdon' descending='false' />
                <filter type='and'>
                  <condition attribute='createdon' operator='today' />
                  <condition attribute='statecode' operator='eq' value='0' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        private int findGraceDays(IOrganizationService crmservices, EntityReference ps)
        {
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true'>
                  <entity name='bsd_interestratemaster'>
                    <attribute name='bsd_interestratemasterid' />
                    <attribute name='bsd_intereststartdatetype' />
                    <attribute name='bsd_gracedays' />
                    <link-entity name='bsd_paymentscheme' from='bsd_interestratemaster' to='bsd_interestratemasterid' alias='ab'>
                      <filter type='and'>
                        <condition attribute='bsd_paymentschemeid' operator='eq'  uitype='bsd_paymentscheme' value='{0}' />
                      </filter>
                    </link-entity>
                  </entity>
                </fetch>";
            fetchXml = string.Format(fetchXml, ps.Id);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entc.Entities.Count > 0)
            {
                if (((OptionSetValue)(entc.Entities[0]["bsd_intereststartdatetype"])).Value == 100000001)//type == graceday
                    return (int)entc.Entities[0]["bsd_gracedays"];
                else return 0;
            }
            else return -1;
        }
    }

}
