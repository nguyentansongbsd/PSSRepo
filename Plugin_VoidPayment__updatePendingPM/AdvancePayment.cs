using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;

namespace Plugin_VoidPayment_updatePendingPM
{
    class AdvancePayment
    {
        IOrganizationService service = null;
        IPluginExecutionContext context;
        StringBuilder strMess = new StringBuilder();
        public AdvancePayment(IOrganizationService service, IPluginExecutionContext context, StringBuilder strMess1)
        {
            this.service = service;
            this.context = context;
            strMess = strMess1;
        }
        public EntityCollection get_AdvPM(Guid pmID)
        {
            string fetchXml =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='bsd_advancepayment' >
                <attribute name='bsd_remainingamount' />
                <attribute name='bsd_transactiondate' />
                <attribute name='bsd_payment' />
                <attribute name='bsd_amount' />
                <attribute name='bsd_paidamount' />
                <attribute name='bsd_name' />
                <attribute name='bsd_advancepaymentid' />
                <attribute name='statuscode' />
                <filter type='and' >
                  <condition attribute='bsd_payment' operator='eq' value='{0}' />
                </filter>
              </entity>
                </fetch>";
            //< condition attribute = 'statuscode' operator= 'eq' value = '100000000' />
            fetchXml = string.Format(fetchXml, pmID);
            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
        public void voidAdvancePayment(Entity enPayment)
        {
            EntityCollection ec_advPM = get_AdvPM(enPayment.Id);

            if (ec_advPM.Entities.Count > 0)
            {
                Entity en_Adv_up = new Entity(ec_advPM.Entities[0].LogicalName);
                en_Adv_up.Id = ec_advPM.Entities[0].Id;
                en_Adv_up["statuscode"] = new OptionSetValue(100000002); // revert

                service.Update(en_Adv_up);
            }
        }
        public void pendingRevertAdvancePayment(Entity enPayment)
        {
            EntityCollection ec_advPM = get_AdvPM(enPayment.Id);

            if (ec_advPM.Entities.Count > 0)
            {
                Entity en_Adv_up = new Entity(ec_advPM.Entities[0].LogicalName);
                en_Adv_up.Id = ec_advPM.Entities[0].Id;
                en_Adv_up["statuscode"] = new OptionSetValue(100000003); // pending revert

                service.Update(en_Adv_up);
            }
        }
    }
}
