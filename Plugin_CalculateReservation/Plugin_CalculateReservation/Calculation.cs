using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Plugin_CalculateReservation
{
    public class Calculation
    {
        private EntityReference proRef = null;
        private EntityReference reserRef = null;
        private IOrganizationService service = null;
        private Entity proLine = null;

        public Calculation(IOrganizationService service, Entity proLine)
        {
            this.service = service;
            proRef = (EntityReference)proLine["productid"];
            reserRef = (EntityReference)proLine["quoteid"];
            this.proLine = proLine;
        }

        public void Calculate()
        {
            if (!this.proLine.Contains("priceperunit"))
                return;
            decimal unitPrice = ((Money)proLine["priceperunit"]).Value;
            Entity pro = service.Retrieve(proRef.LogicalName, proRef.Id, new ColumnSet(new string[3]
            {
                "bsd_landvalue",
                "bsd_maintenancefeespercent",
                "bsd_projectcode"
            }));
            if (pro == null)
                throw new Exception(string.Format("Unit with name '{0}' is unavailable in system or deleted by user!", proRef.Name));
            if (!pro.Contains("bsd_projectcode"))
                throw new InvalidPluginExecutionException(string.Format("Please select project for product '{0}'!", proRef.Name));
            EntityReference projRef = (EntityReference)pro["bsd_projectcode"];
            decimal landValue = pro.Contains("bsd_landvalue") ? ((Money)pro["bsd_landvalue"]).Value : decimal.Zero;
            decimal maintenancePercent = 0;
            //decimal managementPercent = 0;
            Entity rsv = service.Retrieve(reserRef.LogicalName, reserRef.Id, new ColumnSet(new string[]
            {
                "bsd_taxcode",
                "bsd_discountlist",
                "bsd_discounts",
                "bsd_packagesellingamount",
                "bsd_projectid",
                "bsd_paymentscheme"
            }));
            if (!rsv.Contains("bsd_projectid"))
                throw new InvalidPluginExecutionException(string.Format("Please select project for this reservation!"));
            if (projRef.Id.CompareTo(((EntityReference)rsv["bsd_projectid"]).Id) > 0)
                throw new InvalidPluginExecutionException("Project of reservation and unuit is not the same!");
            Entity proj = null;

            if (rsv.Contains("bsd_paymentscheme"))
            {
                EntityReference pmsRef = (EntityReference)rsv["bsd_paymentscheme"];
                StringBuilder fetch = new StringBuilder();
                fetch.Append("<fetch aggregate='true'>");
                fetch.Append("<entity name='bsd_paymentschemedetail' >");
                fetch.Append("<attribute name='bsd_maintenancefees' alias='mtfee' groupby='true' />");
                fetch.Append("<attribute name='bsd_managementfee' alias='mnfee' groupby='true' />");
                fetch.Append("<filter type='and' >");
                fetch.Append("<condition attribute='bsd_optionentry' operator='null' />");
                fetch.Append("<condition attribute='bsd_reservation' operator='null' />");
                fetch.Append("<condition attribute='bsd_paymentscheme' operator='eq' value='" + pmsRef.Id.ToString() + "' />");
                fetch.Append("<filter type='or' >");
                fetch.Append("<condition attribute='bsd_maintenancefees' operator='eq' value='1' />");
                fetch.Append("<condition attribute='bsd_managementfee' operator='eq' value='1' />");
                fetch.Append("</filter>");
                fetch.Append("</filter>");
                fetch.Append("</entity>");
                fetch.Append("</fetch>");
                EntityCollection etnc = service.RetrieveMultiple(new FetchExpression(fetch.ToString()));
                foreach (Entity en in etnc.Entities)
                {
                    AliasedValue a_mtFee = en.Contains("mtfee") ? (AliasedValue)en["mtfee"] : null;

                    if (a_mtFee != null && a_mtFee.Value != null && (bool)a_mtFee.Value == true)
                    {
                        maintenancePercent = pro.Contains("bsd_maintenancefeespercent") ? (decimal)pro["bsd_maintenancefeespercent"] : decimal.Zero;
                        if (maintenancePercent == decimal.Zero)
                        {
                            proj = service.Retrieve(projRef.LogicalName, projRef.Id, new ColumnSet(new string[2]
                            {
                                "bsd_maintenancefeespercent",
                                "bsd_managementfeepercent"
                            }));
                            if (!proj.Contains("bsd_maintenancefeespercent"))
                                throw new InvalidPluginExecutionException(string.Format("Please input the maintenance fee for project '{0}'", projRef.Name));
                            maintenancePercent = (decimal)proj["bsd_maintenancefeespercent"];
                        }
                    }

                    //AliasedValue a_mnFee = en.Contains("mnfee") ? (AliasedValue)en["mtfee"] : null;
                    //if (a_mnFee != null && a_mnFee.Value != null && (bool)a_mnFee.Value == true)
                    //{
                    //    if (proj == null)
                    //        proj = service.Retrieve(projRef.LogicalName, projRef.Id, new ColumnSet(new string[2]
                    //        {
                    //            "bsd_maintenancefeespercent",
                    //            "bsd_managementfeepercent"
                    //        }));
                    //    if (proj != null)
                    //        managementPercent = proj.Contains("bsd_managementfeepercent") ? (decimal)proj["bsd_managementfeepercent"] : decimal.Zero;
                    //}
                }
            }

            decimal pkgSellingAmount = rsv.Contains("bsd_packagesellingamount") ? ((Money)rsv["bsd_packagesellingamount"]).Value : decimal.Zero;
            EntityReference disRef = rsv.Contains("bsd_discountlist") ? (EntityReference)rsv["bsd_discountlist"] : null;
            string selectedDiscount = rsv.Contains("bsd_discounts") ? rsv["bsd_discounts"].ToString() : string.Empty;
            decimal amount = 0;
            List<decimal> percents = new List<decimal>();
            GetDiscount(disRef, selectedDiscount, out amount,unitPrice, out percents);
            List<decimal> specialDiscounts = GetSpecialDiscount(reserRef);
            decimal percent = 0;
            //decimal totalDiscount = amount + (specialDiscount + percent) * unitPrice / 100;
            //decimal totalDiscount = amount + Math.Round(specialDiscount * unitPrice / 100) + Math.Round(percent * unitPrice / 100);
            decimal totalDiscount = amount;
            foreach (decimal d in specialDiscounts)
            {
                percent += d;
                totalDiscount += Math.Ceiling(d * unitPrice / 100);
                
            }
            
            foreach (decimal d in percents)
            {
                percent += d;
                totalDiscount += Math.Ceiling(d * unitPrice / 100);
                
            }
            //throw new InvalidPluginExecutionException(totalDiscount.ToString()+" "+ Math.Ceiling(totalDiscount).ToString());
            totalDiscount = Math.Ceiling(totalDiscount);
            decimal taxAmount = 0;
            if (rsv.Contains("bsd_taxcode"))
            {
                EntityReference taxRef = (EntityReference)rsv["bsd_taxcode"];
                Entity taxCode = service.Retrieve(taxRef.LogicalName, taxRef.Id, new ColumnSet(new string[1] { "bsd_value" }));
                if (taxCode == null)
                    throw new Exception(string.Format("Tax with name '{0}' is unavailable in system or deleted by user!", taxRef.Name));
                if (!taxCode.Contains("bsd_value"))
                    throw new Exception(string.Format("Please input tax value for Tax with name '{0}'!", taxRef.Name));
                decimal taxPercent = (decimal)taxCode["bsd_value"];
                taxAmount = Math.Round((unitPrice - landValue - totalDiscount + pkgSellingAmount) * taxPercent / 100);
            }
            //throw new Exception(taxAmount + "");
            decimal net = unitPrice - totalDiscount + pkgSellingAmount;
            decimal maintenanceFee = net * maintenancePercent / 100;
            //decimal managementFee = net * managementPercent / 100;
            Entity entity5 = new Entity(this.proLine.LogicalName);
            entity5.Id = proLine.Id;
            entity5["manualdiscountamount"] = new Money(totalDiscount);
            entity5["tax"] = new Money(taxAmount);
            service.Update(entity5);
            Entity entity6 = service.Retrieve(this.reserRef.LogicalName, this.reserRef.Id, new ColumnSet(new string[1]
            {
                "totalamountlessfreight"
            }));
            decimal systemNetSelling = entity6.Contains("totalamountlessfreight") ? ((Money)entity6["totalamountlessfreight"]).Value : decimal.Zero;
            Entity tmpRsv = new Entity(this.reserRef.LogicalName);
            tmpRsv.Id = this.reserRef.Id;
            tmpRsv["bsd_totalamountlessfreight"] = new Money(pkgSellingAmount + unitPrice - totalDiscount);
            tmpRsv["freightamount"] = new Money(pkgSellingAmount + Math.Round(maintenanceFee));//tmpRsv["freightamount"] = new Money(Math.Round(maintenanceFee) + pkgSellingAmount + Math.Round(managementFee));
            tmpRsv["bsd_freightamount"] = new Money(maintenanceFee);
            //tmpRsv["bsd_managementfee"] = new Money(managementFee);
            tmpRsv["bsd_detailamount"] = new Money(unitPrice);
            tmpRsv["bsd_discount"] = new Money(totalDiscount);
            tmpRsv["bsd_landvaluededuction"] = new Money(landValue);
            tmpRsv["bsd_totaldiscountpercent"] = (percent);
            tmpRsv["bsd_totaldiscountamount"] = new Money(amount);
            service.Update(tmpRsv);
        }

        private void GetDiscount(EntityReference disRef, string selectedDiscount, out decimal amount,decimal unitprice, out List<decimal> percents)
        {
            amount = 0;
            percents = new List<decimal>();
            QueryExpression queryExpression = new QueryExpression("bsd_discounttransaction");
            queryExpression.ColumnSet = new ColumnSet(new string[0]);
            queryExpression.Criteria = new FilterExpression(LogicalOperator.And);
            queryExpression.Criteria.AddCondition(new ConditionExpression("bsd_reservation", ConditionOperator.Equal, this.reserRef.Id));
            EntityCollection entityCollection1 = this.service.RetrieveMultiple(queryExpression);
            Dictionary<Guid, string> dictionary = new Dictionary<Guid, string>();
            foreach (Entity entity in entityCollection1.Entities)
                dictionary[entity.Id] = string.Empty;
            string[] strArray = new string[0];
            if (!string.IsNullOrWhiteSpace(selectedDiscount))
                strArray = selectedDiscount.Split(',');
            foreach (string input in strArray)
            {
                Guid guid = Guid.Parse(input);
                if (dictionary.ContainsKey(guid))
                {
                    dictionary.Remove(guid);
                }
                else
                {
                    Entity pro = this.service.Retrieve("bsd_discount", guid, new ColumnSet(new string[4]
                    {
                        "bsd_name",
                        "new_type",
                        "bsd_amount",
                        "bsd_percentage"
                    }));
                    if (pro == null)
                        throw new InvalidPluginExecutionException(string.Format("Discount '{0}' dose not exist or deleted."));
                    if (!pro.Contains("new_type"))
                        throw new InvalidPluginExecutionException(string.Format("Please provide type for discount '{0}'!", pro["bsd_name"]));
                    int num = ((OptionSetValue)pro["new_type"]).Value;
                    Entity rsv = new Entity("bsd_discounttransaction");
                    if (num == 100000000)//percent
                    {
                        if (!pro.Contains("bsd_percentage"))
                            throw new InvalidPluginExecutionException(string.Format("Please provide discount percent for discount'{0}'", pro["bsd_name"]));
                        rsv["bsd_name"] = pro["bsd_name"];
                        rsv["bsd_discountpercent"] = pro["bsd_percentage"];
                        rsv["bsd_totaldiscountamount"]= new Money(Math.Ceiling((decimal)pro["bsd_percentage"] * unitprice / 100));
                    }
                    
                    else
                    {
                        if (num != 100000001)//amount
                            throw new InvalidPluginExecutionException(string.Format("Please discount '{0}' is not valid!", pro["bsd_name"]));
                        if (!pro.Contains("bsd_amount"))
                            throw new InvalidPluginExecutionException(string.Format("Please provide discount amount for discount'{0}'", pro["bsd_name"]));
                        rsv["bsd_name"] = pro["bsd_name"];
                        rsv["bsd_discountamount"] = pro["bsd_amount"];
                        rsv["bsd_totaldiscountamount"] = (Money)pro["bsd_amount"];
                    }
                    rsv["bsd_discount"] = pro.ToEntityReference();
                    rsv["bsd_reservation"] = this.reserRef;
                    this.service.Create(rsv);
                }
            }
            foreach (Entity entity in entityCollection1.Entities)
                this.service.Delete(entity.LogicalName, entity.Id);
            StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append("<fetch version='1.0' aggregate='true' >");
            stringBuilder.Append("<fetch version='1.0'>");
            stringBuilder.Append("<entity name='bsd_discounttransaction' >");
            //stringBuilder.Append("<attribute name='bsd_discountamount' alias='amount' aggregate='sum' />");
            //stringBuilder.Append("<attribute name='bsd_discountpercent' alias='percent' aggregate='sum' />");
            stringBuilder.Append("<attribute name='bsd_discountamount'/>");
            stringBuilder.Append("<attribute name='bsd_discountpercent'/>");
            stringBuilder.Append("<filter type='and' >");
            stringBuilder.AppendFormat("<condition attribute='bsd_reservation' operator='eq' value='{0}' />", this.reserRef.Id);
            stringBuilder.Append("</filter>");
            stringBuilder.Append("</entity>");
            stringBuilder.Append("</fetch>");
            EntityCollection etnc = this.service.RetrieveMultiple(new FetchExpression(stringBuilder.ToString()));
            //if (entityCollection2.Entities.Count <= 0)
            //    return;
            //Entity proj = entityCollection2[0];
            //AliasedValue aliasedValue1 = proj.Contains("amount") ? (AliasedValue)proj["amount"] : (AliasedValue)null;
            //amount = aliasedValue1 == null || aliasedValue1.Value == null ? Decimal.Zero : ((Money)aliasedValue1.Value).Value;


            //AliasedValue aliasedValue2 = proj.Contains("percent") ? (AliasedValue)proj["percent"] : (AliasedValue)null;
            //percent=aliasedValue2 == null || aliasedValue2.Value == null ? Decimal.Zero : ((Money)aliasedValue2.Value).Value;
            foreach (Entity etn in etnc.Entities)
            {
                amount += etn.Contains("bsd_discountamount") ? ((Money)etn["bsd_discountamount"]).Value : 0;
                if (etn.Contains("bsd_discountpercent"))
                    percents.Add((decimal)etn["bsd_discountpercent"]);
            }
        }

        private List<decimal> GetSpecialDiscount(EntityReference rsvRef)
        {
            List<decimal> nums = new List<decimal>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("<fetch mapping='logical' version='1.0'>");
            stringBuilder.AppendFormat("<entity name='bsd_discountspecial'>");
            stringBuilder.AppendFormat("<attribute name='bsd_percentdiscount'/>");
            stringBuilder.AppendFormat("<filter type='and'>");
            stringBuilder.AppendFormat("<condition attribute='statuscode' operator='eq' value='100000000'/>");
            stringBuilder.AppendFormat("<condition attribute='bsd_quote' operator='eq' value='{0}'/>", rsvRef.Id);
            stringBuilder.AppendFormat("</filter>");
            stringBuilder.AppendFormat("</entity>");
            stringBuilder.AppendFormat("</fetch>");
            EntityCollection entityCollection = this.service.RetrieveMultiple(new FetchExpression(stringBuilder.ToString()));
            foreach (Entity etn in entityCollection.Entities)
            {
                if (etn.Contains("bsd_percentdiscount"))
                    nums.Add((decimal)etn["bsd_percentdiscount"]);
            }
            return nums;
        }
    }
}