// Decompiled with JetBrains decompiler
// Type: Action_OE_CalDAAmount.Action_OE_CalDAAmount
// Assembly: Action_OE_CalDAAmount, Version=1.0.0.0, Culture=neutral, PublicKeyToken=17305b4ee8ea4c3a
// MVID: 4961DBFA-9EBE-4905-A0EA-3BE293603ACB
// Assembly location: C:\Users\BSD\Desktop\Action_OE_CalDAAmount.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;

namespace Action_OE_CalDAAmount
{
    public class Action_OE_CalDAAmount : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(((IExecutionContext)service).UserId));
            EntityReference inputParameter = (EntityReference)((DataCollection<string, object>)((IExecutionContext)service).InputParameters)["Target"];
            if (!(inputParameter.LogicalName == "salesorder"))
                return;
            Entity entity1 = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[2]
            {
        "bsd_project",
        "bsd_unitnumber"
            }));
            if (entity1.Contains("bsd_project") && entity1.Contains("bsd_unitnumber"))
            {
                //      Entity entity2 = this.service.Retrieve(((EntityReference)entity1["bsd_unitnumber"]).LogicalName, ((EntityReference)entity1["bsd_unitnumber"]).Id, new ColumnSet(new string[1]
                //      {
                //"bsd_depositamountpercent"
                //      }));
                //      Decimal num1 = entity2.Contains("bsd_depositamountpercent") ? (Decimal)entity2["bsd_depositamountpercent"] : 0M;
                //      Entity entity3 = this.service.Retrieve(((EntityReference)entity1["bsd_project"]).LogicalName, ((EntityReference)entity1["bsd_project"]).Id, new ColumnSet(new string[1]
                //      {
                //"bsd_depositpercentda"
                //      }));
                //      if (entity3.Contains("bsd_depositpercentda"))
                //      {
                Decimal num2 = 0M;
                Decimal d = 0M;
                Entity entity4 = new Entity(inputParameter.LogicalName, inputParameter.Id);
                EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)new FetchExpression(string.Format("<fetch>" +
                    "\r\n                                          <entity name='bsd_paymentschemedetail' >" +
                    "\r\n                                            <attribute name='bsd_duedate' />" +
                    "\r\n                                            <attribute name='bsd_ordernumber' />" +
                    "\r\n                                            <attribute name='bsd_amountpercent' />" +
                    "\r\n                                            <attribute name='bsd_amountofthisphase' />" +
                    "\r\n                                            <filter type='and'>" +
                    "\r\n                                              <condition attribute='bsd_optionentry' operator='eq' value='{0}' />" +
                    "\r\n                                            <filter type='or'>" +
                    "\r\n                                              <condition attribute='bsd_ordernumber' operator='eq' value='1' />" +
                    "\r\n                                              <condition attribute='bsd_ordernumber' operator='eq' value='2' />" +
                    "\r\n                                            </filter>" +
                    "\r\n                                            </filter>" +
                    "\r\n                                            <order attribute='bsd_ordernumber' />" +
                    "\r\n                                          </entity>" +
                    "\r\n                                        </fetch>", (object)inputParameter.Id)));
                if (((Collection<Entity>)entityCollection.Entities).Count > 0)
                {
                    for (int index = 0; index < ((Collection<Entity>)entityCollection.Entities).Count; ++index)
                    {
                        Entity entity5 = ((Collection<Entity>)entityCollection.Entities)[index];
                        //num2 += (Decimal)entity5["bsd_amountpercent"];
                        d += ((Money)entity5["bsd_amountofthisphase"]).Value;
                        int bsd_ordernumber = (int)entity5["bsd_ordernumber"];
                        //if (num1 == 0M)
                        //    num1 = (Decimal)entity3["bsd_depositpercentda"];
                        //if (num2 >= num1)
                        if (bsd_ordernumber == 2)
                        {
                            entity4["bsd_duedateda"] = entity5["bsd_duedate"];
                            this.CheckInstallment(this.service, 2, inputParameter.Id);
                            break;
                        }
                    }
                    entity4["bsd_daamount"] = (object)new Money(Math.Ceiling(d));
                    this.service.Update(entity4);
                }
                //}
            }
        }

        public void CheckInstallment(IOrganizationService service, int SoInstallment, Guid orderID)
        {
            string str = string.Format("<fetch>\r\n                                          <entity name='bsd_paymentschemedetail' >\r\n                                            <attribute name='bsd_duedate' />\r\n                                            <attribute name='bsd_ordernumber' />\r\n                                            <attribute name='bsd_amountpercent' />\r\n                                            <attribute name='bsd_amountofthisphase' />\r\n                                            <attribute name='bsd_da' />\r\n                                            <filter type='and' >\r\n                                              <condition attribute='bsd_optionentry' operator='eq' value='{0}' />\r\n                                            </filter>\r\n                                            <order attribute='bsd_ordernumber' />\r\n                                          </entity>\r\n                                        </fetch>", (object)orderID);
            EntityCollection entityCollection = service.RetrieveMultiple((QueryBase)new FetchExpression(str));
            if (((Collection<Entity>)entityCollection.Entities).Count <= 0)
                return;
            foreach (Entity entity in (Collection<Entity>)entityCollection.Entities)
                service.Update(new Entity(entity.LogicalName, entity.Id)
                {
                    ["bsd_da"] = (int)entity["bsd_ordernumber"] > SoInstallment ? (object)false : (object)true
                });
        }
    }
}
