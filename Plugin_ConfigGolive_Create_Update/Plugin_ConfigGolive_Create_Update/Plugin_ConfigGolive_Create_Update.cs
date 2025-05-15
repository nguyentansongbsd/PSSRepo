using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IdentityModel.Metadata;
using System.IO;
using System.Net;
using System.Text;

namespace Plugin_ConfigGolive_Create_Update
{
    public class Plugin_ConfigGolive_Create_Update : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        ITracingService traceServiceClass = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceServiceClass = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.MessageName == "Update")
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity enTarget = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                string name = enTarget.Contains("bsd_name") ? (string)enTarget["bsd_name"] : "";
                if (name == "New Config Golive")
                {
                    var fetchXml2 = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch>
  <entity name=""salesorder"" >
    <attribute name=""bsd_optionno"" alias=""OptNo"" />
    <attribute name=""bsd_optioncodesams"" alias=""OptionCodeSams"" />
    <attribute name=""bsd_exchangeratesams"" alias=""ExchangeRateSams"" />
    <attribute name=""bsd_totalamountpaid"" alias=""TotalAmtColl"" />
    <attribute name=""statuscode"" alias=""OtpStt"" />
    <attribute name=""bsd_totalpercent"" alias=""TotalPercent"" />
    <attribute name=""bsd_vatadjamount"" alias=""VatAdjAmount"" />
    <attribute name=""bsd_vatadjamountsams"" alias=""VatAdjAmountSAMS"" />
    <attribute name=""totaltax"" alias=""VAT"" />
    <attribute name=""bsd_totalamountlessfreight"" alias=""NetSellingPriceBfVAT"" />
    <attribute name=""bsd_managementfee"" alias=""ManageFee"" />
    <attribute name=""freightamount"" alias=""MaintenanceFee"" />
    <attribute name=""bsd_remark"" alias=""Remarks"" />
    <attribute name=""ownerid"" alias=""CCRStaff"" />
    <attribute name=""bsd_nameofstaffagent"" alias=""StaffAgent"" />
    <attribute name=""bsd_salesagentcompany"" alias=""SalesAgentCompany"" />
    <attribute name=""bsd_totalpaidincludecoa"" alias=""TotalPaidIncludeCOA"" />
    <attribute name=""bsd_discountsams"" alias=""Discountsams"" />
    <attribute name=""bsd_totalpaybilldiscount"" alias=""TotalPaybillDiscount"" />
    <attribute name=""bsd_totalpaybilldiscountusd"" alias=""TotalPaybillDiscountUSD"" />
    <filter type=""or"" >
      <filter type=""and"" >
        <condition attribute=""bsd_optiondatesams"" operator=""on-or-after"" value=""09/05/2000"" />
        <condition attribute=""bsd_optiondatesams"" operator=""on-or-before"" value=""09/05/2025"" />
        <condition attribute=""bsd_project"" operator=""eq"" value=""c3de5ae2-9837-ec11-8c64-00224856e184"" />
        <condition attribute=""bsd_paymentscheme"" operator=""eq"" value=""67a6abce-47fe-ef11-bae3-000d3a084964"" />
        <condition attribute=""statuscode"" operator=""neq"" value=""100000006"" />
        <condition attribute=""bsd_optioncodesams"" operator=""not-null"" />
      </filter>
      <filter type=""and"" >
        <condition attribute=""createdon"" operator=""on-or-after"" value=""09/05/2000"" />
        <condition attribute=""createdon"" operator=""on-or-before"" value=""09/05/2025"" />
        <condition attribute=""bsd_project"" operator=""eq"" value=""c3de5ae2-9837-ec11-8c64-00224856e184"" />
        <condition attribute=""bsd_paymentscheme"" operator=""eq"" value=""67a6abce-47fe-ef11-bae3-000d3a084964"" />
        <condition attribute=""statuscode"" operator=""neq"" value=""100000006"" />
        <condition attribute=""bsd_optioncodesams"" operator=""null"" />
      </filter>
    </filter>
    <order attribute=""bsd_optionno"" />
    <link-entity name=""product"" from=""productid"" to=""bsd_unitnumber"" link-type=""outer"" alias=""prod"" >
      <attribute name=""name"" alias=""unit"" />
      <attribute name=""bsd_blocknumber"" alias=""block"" />
    </link-entity>
    <link-entity name=""account"" from=""accountid"" to=""customerid"" link-type=""outer"" alias=""account"" >
      <attribute name=""name"" alias=""accNameSys"" />
      <attribute name=""bsd_name"" alias=""accName"" />
      <attribute name=""bsd_accountnameother"" alias=""accNameEN"" />
      <attribute name=""bsd_address"" alias=""acc_add1"" />
      <attribute name=""bsd_permanentaddress1"" alias=""acc_addPer"" />
      <attribute name=""bsd_country"" alias=""acc_country"" />
      <attribute name=""emailaddress1"" alias=""acc_email"" />
      <attribute name=""bsd_email2"" alias=""acc_email2"" />
      <attribute name=""bsd_localization"" alias=""acc_localization"" />
      <link-entity name=""contact"" from=""contactid"" to=""primarycontactid"" link-type=""outer"" alias=""primary"" >
        <attribute name=""fullname"" alias=""primary_nameSys"" />
        <attribute name=""bsd_fullname"" alias=""primary_name"" />
      </link-entity>
    </link-entity>
    <link-entity name=""contact"" from=""contactid"" to=""customerid"" link-type=""outer"" alias=""cont"" >
      <attribute name=""fullname"" alias=""contNameSys"" />
      <attribute name=""bsd_fullname"" alias=""contName"" />
      <attribute name=""bsd_contactaddress"" alias=""cont_add1"" />
      <attribute name=""bsd_permanentaddress1"" alias=""cont_add2"" />
      <attribute name=""bsd_country"" alias=""cont_country"" />
      <attribute name=""emailaddress1"" alias=""cont_email"" />
      <attribute name=""bsd_email2"" alias=""cont_email2"" />
      <attribute name=""bsd_localization"" alias=""cont_localization"" />
    </link-entity>
    <link-entity name=""bsd_paymentschemedetail"" from=""bsd_optionentry"" to=""salesorderid"" link-type=""outer"" alias=""psd"" >
      <attribute name=""bsd_exchangeamount"" alias=""psd_InstallExRate"" />
      <attribute name=""bsd_interestchargeamount"" alias=""InterestBill"" />
      <attribute name=""bsd_interestwaspaid"" alias=""InterestPaid"" />
      <attribute name=""bsd_additionalareabill"" alias=""AreaBill"" />
      <attribute name=""bsd_additionalareapaid"" alias=""AreaPaid"" />
      <attribute name=""bsd_waiverinterest"" alias=""InterestWaiver"" />
      <attribute name=""bsd_depositamount"" alias=""DepositAmount"" />
      <attribute name=""bsd_amountwaspaid"" alias=""AmountWasPaid"" />
      <attribute name=""bsd_maintenanceamount"" alias=""MaintenanceBill"" />
      <attribute name=""bsd_maintenancefeepaid"" alias=""MaintenancePaid"" />
      <attribute name=""bsd_managementamount"" alias=""ManagementBill"" />
      <attribute name=""bsd_managementfeepaid"" alias=""ManagementPaid"" />
      <attribute name=""bsd_totalpaybilldiscountfees"" alias=""TotalPayBillDiscountFees"" />
      <filter type=""and"" >
        <condition attribute=""statecode"" operator=""eq"" value=""0"" />
      </filter>
      <order attribute=""bsd_ordernumber"" />
    </link-entity>
    <link-entity name=""bsd_exchangeratedetail"" from=""bsd_exchangeratedetailid"" to=""bsd_applyingexchangerate"" link-type=""outer"" alias=""Ex"" >
      <attribute name=""bsd_rate"" alias=""OtpExRate"" />
    </link-entity>
    <link-entity name=""account"" from=""accountid"" to=""bsd_salesagentcompany"" link-type=""outer"" alias=""saleAgent"" >
      <attribute name=""bsd_companycode"" alias=""AgentCode"" />
    </link-entity>
    <link-entity name=""quote"" from=""quoteid"" to=""quoteid"" link-type=""outer"" alias=""quote"" >
      <attribute name=""ownerid"" alias=""SalesStaff"" />
    </link-entity>
  </entity>
</fetch>";

                    EntityCollection list2 = service.RetrieveMultiple(new FetchExpression(fetchXml2));
                    traceServiceClass.Trace("count " + list2.Entities.Count);
                    foreach (var dynEntity in list2.Entities)
                    {
                        traceServiceClass.Trace("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&222222222222222222222222222222");
                        foreach (var prop in dynEntity.Attributes)
                        {
                            
                            traceServiceClass.Trace(prop.Key + ":" + prop.Value);
                        }
                    }
                    //traceServiceClass.Trace(list2.Entities.ToString());
                }
            }
        }
    }
}