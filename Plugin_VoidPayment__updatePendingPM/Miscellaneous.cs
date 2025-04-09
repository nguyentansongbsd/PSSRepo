using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;

namespace Plugin_VoidPayment_updatePendingPM
{
    class Miscellaneous
    {
        IOrganizationService service = null;
        IPluginExecutionContext context;
        StringBuilder strMess = new StringBuilder();
        public Miscellaneous(IOrganizationService service , IPluginExecutionContext context, StringBuilder strMess1)
        {
            this.service = service;
            this.context = context;
            strMess = strMess1;
        }
        public Entity getMiscellaneous(string miscellaneousid)
        {
            string fetchXmlMiscellaneous =
              @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                <entity name='bsd_miscellaneous' >
                <attribute name='bsd_balance' />
                <attribute name='statuscode' />
                <attribute name='bsd_miscellaneousnumber' />
                <attribute name='bsd_units' />
                <attribute name='bsd_optionentry' />
                <attribute name='bsd_miscellaneousid' />
                <attribute name='bsd_amount' />
                <attribute name='bsd_totalamount' />
                <attribute name='bsd_paidamount' />
                <attribute name='bsd_installment' />
                <attribute name='bsd_name' />
                <attribute name='bsd_project' />
                <attribute name='bsd_installmentnumber' />
                <filter type='and' >
                    <condition attribute='bsd_miscellaneousid' operator='eq' value='{0}'/>;
                </filter>                           
                </entity>
                </fetch>";
            fetchXmlMiscellaneous = string.Format(fetchXmlMiscellaneous, miscellaneousid);

            EntityCollection entc = service.RetrieveMultiple(new FetchExpression(fetchXmlMiscellaneous));
            if (entc.Entities.Count > 0)
            {
                return entc.Entities[0];
            }
            else
            {
                return null;
            }
        }
    }
}
