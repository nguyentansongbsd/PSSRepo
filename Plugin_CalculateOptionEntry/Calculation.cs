using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Plugin_CalculateOptionEntry
{
    public class Calculation
    {
        private EntityReference proRef = null;

        private EntityReference opRef = null;

        private IOrganizationService service = null;

        private Entity proLine = null;

        public Calculation(IOrganizationService service, Entity proLine)
        {
            this.service = service;
            this.proRef = (EntityReference)proLine["productid"];
            this.opRef = (EntityReference)proLine["salesorderid"];
            this.proLine = proLine;
        }

        public void Calculate()
        {
            EntityReference item;
            AliasedValue aliasedValue;
            if (this.proLine.Contains("priceperunit"))
            {
                decimal value = ((Money)this.proLine["priceperunit"]).Value;
                Entity entity = this.service.Retrieve(this.proRef.LogicalName, this.proRef.Id, new ColumnSet(new string[] { "bsd_landvalue", "bsd_maintenancefees", "bsd_maintenancefeespercent", "bsd_projectcode" }));
                if (entity == null)
                {
                    throw new Exception(string.Format("Unit with name '{0}' is unavailable in system or deleted by user!", this.proRef.Name));
                }
                if (!entity.Contains("bsd_projectcode"))
                {
                    throw new InvalidPluginExecutionException(string.Format("Please select project for product '{0}'!", this.proRef.Name));
                }
                EntityReference entityReference = (EntityReference)entity["bsd_projectcode"];
                decimal num = (entity.Contains("bsd_landvalue") ? ((Money)entity["bsd_landvalue"]).Value : decimal.Zero);
                decimal item1 = new decimal();
                //decimal num1 = new decimal();
                Entity entity1 = this.service.Retrieve(this.opRef.LogicalName, this.opRef.Id, new ColumnSet(new string[] { "bsd_taxcode", "bsd_discountlist", "bsd_discounts", "bsd_packagesellingamount", "bsd_project", "bsd_paymentscheme", "bsd_amountdiscountchange" }));
                if (!entity1.Contains("bsd_project"))
                {
                    throw new InvalidPluginExecutionException(string.Format("Please select project for this option entry!", Array.Empty<object>()));
                }
                if (entityReference.Id.CompareTo(((EntityReference)entity1["bsd_project"]).Id) > 0)
                {
                    throw new InvalidPluginExecutionException("Project of option entry and unuit is not the same!");
                }
                Entity entity2 = null;
                if (entity1.Contains("bsd_paymentscheme"))
                {
                    EntityReference entityReference1 = (EntityReference)entity1["bsd_paymentscheme"];
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("<fetch aggregate='true'>");
                    stringBuilder.Append("<entity name='bsd_paymentschemedetail' >");
                    stringBuilder.Append("<attribute name='bsd_maintenancefees' alias='mtfee' groupby='true' />");
                    stringBuilder.Append("<attribute name='bsd_managementfee' alias='mnfee' groupby='true' />");
                    stringBuilder.Append("<filter type='and' >");
                    stringBuilder.Append("<condition attribute='bsd_optionentry' operator='null' />");
                    stringBuilder.Append("<condition attribute='bsd_reservation' operator='null' />");
                    Guid id = entityReference1.Id;
                    stringBuilder.Append(string.Concat("<condition attribute='bsd_paymentscheme' operator='eq' value='", id.ToString(), "' />"));
                    stringBuilder.Append("<filter type='or' >");
                    stringBuilder.Append("<condition attribute='bsd_maintenancefees' operator='eq' value='1' />");
                    stringBuilder.Append("<condition attribute='bsd_managementfee' operator='eq' value='1' />");
                    stringBuilder.Append("</filter>");
                    stringBuilder.Append("</filter>");
                    stringBuilder.Append("</entity>");
                    stringBuilder.Append("</fetch>");
                    EntityCollection entityCollection = this.service.RetrieveMultiple(new FetchExpression(stringBuilder.ToString()));
                    foreach (Entity entity3 in entityCollection.Entities)
                    {
                        if (entity3.Contains("mtfee"))
                        {
                            aliasedValue = (AliasedValue)entity3["mtfee"];
                        }
                        else
                        {
                            aliasedValue = null;
                        }
                        AliasedValue aliasedValue1 = aliasedValue;
                        if ((aliasedValue1 == null || aliasedValue1.Value == null ? false : (bool)aliasedValue1.Value))
                        {
                            item1 = (entity.Contains("bsd_maintenancefeespercent") ? (decimal)entity["bsd_maintenancefeespercent"] : decimal.Zero);
                            if (item1 == decimal.Zero)
                            {
                                entity2 = this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(new string[] { "bsd_maintenancefeespercent", "bsd_managementfeepercent" }));
                                if (!entity2.Contains("bsd_maintenancefeespercent"))
                                {
                                    throw new InvalidPluginExecutionException(string.Format("Please input the maintenance fee for project '{0}'", entityReference.Name));
                                }
                                item1 = (decimal)entity2["bsd_maintenancefeespercent"];
                            }
                        }
                    }
                }
                decimal num2 = (entity1.Contains("bsd_packagesellingamount") ? ((Money)entity1["bsd_packagesellingamount"]).Value : decimal.Zero);
                if (entity1.Contains("bsd_discountlist"))
                {
                    item = (EntityReference)entity1["bsd_discountlist"];
                }
                else
                {
                    item = null;
                }
                EntityReference entityReference2 = item;
                string str = (entity1.Contains("bsd_discounts") ? entity1["bsd_discounts"].ToString() : string.Empty);
                decimal num3 = (entity1.Contains("bsd_amountdiscountchange") ? ((Money)entity1["bsd_amountdiscountchange"]).Value : decimal.Zero);
                if (num3 != decimal.Zero)
                {
                    EntityCollection allDiscountTransaction = this.getAllDiscountTransaction(entity1);
                    if (allDiscountTransaction.Entities.Count > 0)
                    {
                        foreach (Entity entity4 in allDiscountTransaction.Entities)
                        {
                            Entity entity5 = new Entity(entity4.LogicalName, entity4.Id);
                            SetStateRequest setStateRequest = new SetStateRequest();
                            setStateRequest.EntityMoniker = entity5.ToEntityReference();
                            setStateRequest.State = new OptionSetValue(1);
                            setStateRequest.Status = new OptionSetValue(2);
                            this.service.Execute(setStateRequest);
                        }
                    }
                    EntityCollection allSpecialDiscount = this.getAllSpecialDiscount(entity1);
                    if (allSpecialDiscount.Entities.Count > 0)
                    {
                        foreach (Entity entity6 in allSpecialDiscount.Entities)
                        {
                            Entity entity7 = new Entity(entity6.LogicalName, entity6.Id);
                            SetStateRequest setStateRequest1 = new SetStateRequest();
                            setStateRequest1.EntityMoniker = entity7.ToEntityReference();
                            setStateRequest1.State=new OptionSetValue(1);
                            setStateRequest1.Status=new OptionSetValue(2);
                            this.service.Execute(setStateRequest1);
                        }
                    }
                }
                decimal num4 = new decimal();
                decimal num5 = new decimal();
                List<decimal> nums = new List<decimal>();
                List<decimal> nums1 = new List<decimal>();
                decimal num6 = new decimal();
                if (num3 == decimal.Zero)
                {
                    this.GetDiscount(entityReference2, str, out num4, out nums);
                    num6 = num4;
                    foreach (decimal specialDiscount in this.GetSpecialDiscount(this.opRef))
                    {
                        num5 += specialDiscount;
                        num6 += Math.Round((specialDiscount * value) / new decimal(100), MidpointRounding.AwayFromZero);
                    }
                    foreach (decimal num7 in nums)
                    {
                        num5 += num7;
                        num6 += Math.Round((num7 * value) / new decimal(100), MidpointRounding.AwayFromZero);
                    }
                }
                num6 += num3;
                num6 = Math.Round(num6, MidpointRounding.AwayFromZero);
                decimal num8 = new decimal();
                decimal test = 0;
                if (entity1.Contains("bsd_taxcode"))
                {
                    EntityReference item2 = (EntityReference)entity1["bsd_taxcode"];
                    Entity entity8 = this.service.Retrieve(item2.LogicalName, item2.Id, new ColumnSet(new string[] { "bsd_value" }));
                    if (entity8 == null)
                    {
                        throw new Exception(string.Format("Tax with name '{0}' is unavailable in system or deleted by user!", item2.Name));
                    }
                    if (!entity8.Contains("bsd_value"))
                    {
                        throw new Exception(string.Format("Please input tax value for Tax with name '{0}'!", item2.Name));
                    }
                    decimal item3 = (decimal)entity8["bsd_value"];
                    num8 = Math.Round(((((value - num) - num6) + num2) * item3) / new decimal(100),MidpointRounding.AwayFromZero);
                    //test = ((((value - num) - num6) + num2) * item3) / new decimal(100);
                    //test = Math.Round(test, MidpointRounding.AwayFromZero);
                }
                decimal num9 = Math.Round((value - num6) + num2);
                decimal num10 = Math.Round((num9 * item1) / new decimal(100));//maintenanceFee
               
                if (entity1.Contains("bsd_project"))
                {
                    Guid idpro = ((EntityReference)entity1["bsd_project"]).Id;
                    Guid check = new Guid("{30B83A61-4FB3-ED11-83FF-002248593808}");
                    Guid check1 = new Guid("{1D561ECF-5221-EE11-9966-000D3AA0853D}");
                    Guid check2 = new Guid("{A1403588-5021-EE11-9CBE-000D3AA14FB9}");
                    // throw new InvalidPluginExecutionException("id " + idpro);

                    if (idpro == check || idpro == check1 || idpro == check2)
                    {
                        Entity pro = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                        decimal per = 0;
                        per = pro.Contains("bsd_maintenancefeespercent") ? (decimal)pro["bsd_maintenancefeespercent"] : decimal.Zero;
                        decimal mainten = Math.Round(num9 * per / 100, 0);
                        num10 = mainten;
                        decimal taxnew = (num9 + mainten) * 10 / 100;
                        // throw new InvalidPluginExecutionException("amount " + taxnew);
                        num8 = taxnew;
                        //throw new InvalidPluginExecutionException("mainten " + mainten + " taxAmount "+ num8);
                    }
                }
                //throw new InvalidPluginExecutionException("maintance " + num10);
                Entity entity9 = new Entity(this.proLine.LogicalName);
                entity9.Id = this.proLine.Id;
                entity9["manualdiscountamount"] = new Money(num6);
                entity9["tax"] = new Money(num8);
                //throw new InvalidPluginExecutionException("TAX: " + test);
                this.service.Update(entity9);
                Entity entity10 = this.service.Retrieve(this.opRef.LogicalName, this.opRef.Id, new ColumnSet(new string[] { "totalamountlessfreight" }));
                decimal num11 = (entity10.Contains("totalamountlessfreight") ? ((Money)entity10["totalamountlessfreight"]).Value : decimal.Zero);
                Entity entity11 = new Entity(this.opRef.LogicalName);
                entity11.Id = this.opRef.Id;
                entity11["bsd_totalamountlessfreight"] = new Money((num2 + value) - num6);
                entity11["freightamount"] = new Money(num2 + Math.Round(num10));
                entity11["bsd_freightamount"] = new Money(num10);
                entity11["bsd_detailamount"] = new Money(value);
                entity11["bsd_discount"]= new Money(num6);
                entity11["bsd_landvaluededuction"] = new Money(num);
                entity11["bsd_totaldiscountpercent"] = num5;
                entity11["bsd_totaldiscountamount"]= new Money(num4);
                this.service.Update(entity11);
            }
        }

        private EntityCollection getAllDiscountTransaction(Entity enrfOptionEntry)
        {
            QueryExpression queryExpression = new QueryExpression("bsd_discounttransaction");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("bsd_optionentry",ConditionOperator.Equal, new object[] { enrfOptionEntry.Id });
            return this.service.RetrieveMultiple(queryExpression);
        }

        private EntityCollection getAllSpecialDiscount(Entity enrfOptionEntry)
        {
            QueryExpression queryExpression = new QueryExpression("bsd_discountspecial");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, new object[] { enrfOptionEntry.Id });
            return this.service.RetrieveMultiple(queryExpression);
        }

        private void GetDiscount(EntityReference disRef, string selectedDiscount, out decimal amount, out List<decimal> percents)
        {
            amount = new decimal();
            percents = new List<decimal>();
            QueryExpression queryExpression = new QueryExpression("bsd_discounttransaction");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria = new FilterExpression(LogicalOperator.And);
            queryExpression.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", 0, this.opRef.Id));
            queryExpression.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            EntityCollection entityCollection = this.service.RetrieveMultiple(queryExpression);
            Dictionary<Guid, string> guids = new Dictionary<Guid, string>();
            foreach (Entity entity in entityCollection.Entities)
            {
                guids[entity.Id] = string.Empty;
            }
            string[] strArrays = new string[0];
            if (!string.IsNullOrWhiteSpace(selectedDiscount))
            {
                strArrays = selectedDiscount.Split(new char[] { ',' });
            }
            string[] strArrays1 = strArrays;
            for (int i = 0; i < (int)strArrays1.Length; i++)
            {
                Guid guid = Guid.Parse(strArrays1[i]);
                if (!guids.ContainsKey(guid))
                {
                    Entity entity1 = this.service.Retrieve("bsd_discount", guid, new ColumnSet(new string[] { "bsd_name", "new_type", "bsd_amount", "bsd_percentage" }));
                    if (entity1 == null)
                    {
                        throw new InvalidPluginExecutionException(string.Format("Discount '{0}' dose not exist or deleted.", Array.Empty<object>()));
                    }
                    if (!entity1.Contains("new_type"))
                    {
                        throw new InvalidPluginExecutionException(string.Format("Please provide for discount '{0}'!", entity1["bsd_name"]));
                    }
                    int value = ((OptionSetValue)entity1["new_type"]).Value;
                    Entity entity2 = new Entity("bsd_discounttransaction");
                    if (value != 100000000)
                    {
                        if (value != 100000001)
                        {
                            throw new InvalidPluginExecutionException(string.Format("Please discount '{0}' is not valid!", entity1["bsd_name"]));
                        }
                        if (!entity1.Contains("bsd_amount"))
                        {
                            throw new InvalidPluginExecutionException(string.Format("Please provide discount amount for discount'{0}'", entity1["bsd_name"]));
                        }
                        entity2["bsd_name"]= entity1["bsd_name"];
                        entity2["bsd_discountamount"]= entity1["bsd_amount"];
                    }
                    else
                    {
                        if (!entity1.Contains("bsd_percentage"))
                        {
                            throw new InvalidPluginExecutionException(string.Format("Please provide discount percent for discount'{0}'", entity1["bsd_name"]));
                        }
                        entity2["bsd_name"] = entity1["bsd_name"];
                        entity2["bsd_discountpercent"]= entity1["bsd_percentage"];
                    }
                    entity2["bsd_discount"]= entity1.ToEntityReference();
                    entity2["bsd_optionentry"] = this.opRef;
                    this.service.Create(entity2);
                }
                else
                {
                    guids.Remove(guid);
                }
            }
            foreach (Entity entity3 in entityCollection.Entities)
            {
                this.service.Delete(entity3.LogicalName, entity3.Id);
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<fetch version='1.0'>");
            stringBuilder.Append("<entity name='bsd_discounttransaction' >");
            stringBuilder.Append("<attribute name='bsd_discountamount'/>");
            stringBuilder.Append("<attribute name='bsd_discountpercent'/>");
            stringBuilder.Append("<filter type='and' >");
            stringBuilder.AppendFormat("<condition attribute='bsd_optionentry' operator='eq' value='{0}' />", this.opRef.Id);
            stringBuilder.Append("<condition attribute='statecode' operator='eq' value='0' />");
            stringBuilder.Append("</filter>");
            stringBuilder.Append("</entity>");
            stringBuilder.Append("</fetch>");
            EntityCollection entityCollection1 = this.service.RetrieveMultiple(new FetchExpression(stringBuilder.ToString()));
            foreach (Entity entity4 in entityCollection1.Entities)
            {
                amount = amount + (entity4.Contains("bsd_discountamount") ? ((Money)entity4["bsd_discountamount"]).Value : decimal.Zero);
                if (entity4.Contains("bsd_discountpercent"))
                {
                    percents.Add((decimal)entity4["bsd_discountpercent"]);
                }
            }
        }

        private List<decimal> GetSpecialDiscount(EntityReference opRef)
        {
            List<decimal> nums = new List<decimal>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("<fetch mapping='logical' version='1.0'>", Array.Empty<object>());
            stringBuilder.AppendFormat("<entity name='bsd_discountspecial'>", Array.Empty<object>());
            stringBuilder.AppendFormat("<attribute name='bsd_percentdiscount'/>", Array.Empty<object>());
            stringBuilder.AppendFormat("<filter type='and'>", Array.Empty<object>());
            stringBuilder.AppendFormat("<condition attribute='statuscode' operator='eq' value='100000000'/>", Array.Empty<object>());
            stringBuilder.AppendFormat("<condition attribute='bsd_optionentry' operator='eq' value='{0}'/>", opRef.Id);
            stringBuilder.AppendFormat("</filter>", Array.Empty<object>());
            stringBuilder.AppendFormat("</entity>", Array.Empty<object>());
            stringBuilder.AppendFormat("</fetch>", Array.Empty<object>());
            EntityCollection entityCollection = this.service.RetrieveMultiple(new FetchExpression(stringBuilder.ToString()));
            foreach (Entity entity in entityCollection.Entities)
            {
                if (entity.Contains("bsd_percentdiscount"))
                {
                    nums.Add((decimal)entity["bsd_percentdiscount"]);
                }
            }
            return nums;
        }
    }
}