using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;

namespace Plugin_VoidPayment_updatePendingPM
{
    class Installment
    {
        IOrganizationService service = null;
        IPluginExecutionContext context;
        StringBuilder strMess = new StringBuilder();
        public Installment(IOrganizationService service , IPluginExecutionContext context, StringBuilder strMess1)
        {
            this.service = service;
            this.context = context;
            strMess = strMess1;
        }

        public bool check_Ins_Paid(IOrganizationService crmservices, Guid oeID, int i_ordernumber)
        {
            bool res = false;
            // check neu installment truoc do da paid hay chua , neu paid r thi cho excute, neu chua paid thi thong bao ra
            string fetchXml =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_paymentschemedetail' >
                <attribute name='bsd_amountpay' />
                <attribute name='statuscode' />
                <attribute name='bsd_name' />
                <attribute name='bsd_ordernumber' />
                <attribute name='bsd_paymentschemedetailid' />
                <attribute name='bsd_paiddate' />
                <filter type='and' >
                <condition attribute='bsd_ordernumber' operator='eq' value='{0}' />
                  <condition attribute='bsd_optionentry' operator='eq' value='{1}' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, i_ordernumber, oeID);

            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));

            if (entc.Entities.Count > 0)
            {
                Entity en_tmp = entc.Entities[0];
                en_tmp.Id = entc.Entities[0].Id;
                if (((OptionSetValue)en_tmp["statuscode"]).Value == 100000001) res = true;
                if (((OptionSetValue)en_tmp["statuscode"]).Value == 100000000) res = false;
            }
            return res;
        }
        public EntityCollection get_Ins_ES(string oeID)
        {
            string fetchXml =
               @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_paymentschemedetail' >
                <attribute name='bsd_duedatecalculatingmethod' />
                <attribute name='bsd_maintenanceamount' />
                <attribute name='bsd_maintenancefeepaid' />
                <attribute name='bsd_ordernumber' />
                <attribute name='statuscode' />
                <attribute name='bsd_managementfeepaid' />
                <attribute name='bsd_amountpay' />
                <attribute name='bsd_maintenancefees' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_managementfee' />
                <attribute name='bsd_managementfeesstatus' />
                <attribute name='bsd_managementamount' />
                <attribute name='bsd_amountwaspaid' />
                <attribute name='bsd_maintenancefeesstatus' />
                <attribute name='bsd_name' />
                <attribute name='bsd_paymentschemedetailid' />
                <attribute name='bsd_paiddate' />
                <filter type='and' >
                  <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                  <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002' />
                </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, oeID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public EntityCollection GetPSD(string OptionEntryID)
        {
            QueryExpression query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet = new ColumnSet(new string[] { "bsd_ordernumber", "bsd_paymentschemedetailid", "statuscode", "bsd_amountwaspaid", "bsd_paiddate" });
            query.Distinct = true;
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, OptionEntryID);
            query.AddOrder("bsd_ordernumber", OrderType.Ascending);
            //query.TopCount = 1;
            EntityCollection psdFirst = service.RetrieveMultiple(query);
            return psdFirst;
        }
        public EntityCollection get_1stinstallment(Guid resvID)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_balance' />
                    <attribute name='bsd_amountwaspaid' />
                    <attribute name='bsd_reservation' />
                    <attribute name='statuscode' />
                    <attribute name='bsd_amountofthisphase' />
                    <attribute name='bsd_ordernumber' />
                    <attribute name='bsd_paiddate' />
                    <filter type='and' >
                      <condition attribute='bsd_ordernumber' operator='eq' value='1' />
                      <condition attribute='bsd_reservation' operator='eq' value='{0}' />
                    </filter>
              </entity>
            </fetch>";
            fetchXml = string.Format(fetchXml, resvID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
    }

}
