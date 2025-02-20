// Decompiled with JetBrains decompiler
// Type: Action_FollowupList_GenerateTerminateLetter.Action_FUL_GenerateTerminateLetter
// Assembly: Action_FUL_GenerateTerminateLetter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=2004ca381ff4b6b9
// MVID: 1E192ACB-875A-4F3A-AEB1-D2732DEBA9A6
// Assembly location: C:\Users\ngoct\Downloads\New folder\Action_FUL_GenerateTerminateLetter_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;


namespace Action_FollowupList_GenerateTerminateLetter
{
    public class Action_FUL_GenerateTerminateLetter : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service.UserId));
            ((ITracingService)serviceProvider.GetService(typeof(ITracingService))).Trace(string.Format("Context Depth {0}", (object)service.Depth));
            if (service.Depth > 1)
                return;
            EntityReference inputParameter = (EntityReference)service.InputParameters["Target"];
            Entity entity1 = this.service.Retrieve("bsd_followuplist", inputParameter.Id, new ColumnSet(new string[4]
            {
        "bsd_optionentry",
        "bsd_type",
        "bsd_name",
        "bsd_units"
            }));
            if (!entity1.Contains("bsd_optionentry"))
                throw new Exception("This follow up list does not containt Option Entry information. Please check again!");
            if (((OptionSetValue)entity1["bsd_type"]).Value != 100000006)
                throw new Exception("This record is not in 'Terminate' status, cannot proceed to the next stage.");
            Entity entity2 = this.service.Retrieve(((EntityReference)entity1["bsd_optionentry"]).LogicalName, ((EntityReference)entity1["bsd_optionentry"]).Id, new ColumnSet(new string[2]
            {
        "customerid",
        "statuscode"
            }));
            if (this.get_OE_in_Terminate(((EntityReference)entity1["bsd_optionentry"]).Id).Entities.Count > 0)
                throw new Exception("This followup list already exist in Termination list! Cannot proceed to next stage.");
            if (this.get_OE_in_Terminateletter(((EntityReference)entity1["bsd_optionentry"]).Id).Entities.Count > 0)
                throw new Exception("This followup list already exist in TerminateLetter list! Cannot proceed to next stage.");
            EntityReference entityReference = entity2.Contains("customerid") ? (EntityReference)entity2["customerid"] : throw new Exception("Please check field 'Customer' in Option entry data!");
            this.service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(new string[1]
            {
        entityReference.LogicalName == "contact" ? "fullname" : "name"
            }));
            Entity entity3 = this.service.Retrieve(((EntityReference)entity1["bsd_units"]).LogicalName, ((EntityReference)entity1["bsd_units"]).Id, new ColumnSet(new string[1]
            {
        "bsd_signedcontractdate"
            }));
            if (!entity3.Contains("bsd_signedcontractdate"))
                throw new Exception("Please check 'Signed Contract date' in Units data!");
            Entity entity4 = new Entity("bsd_terminateletter");
            Entity entity5 = new Entity("bsd_termination");
            entity5["bsd_name"] = (object)("Termination of " + (entity1.Contains("bsd_name") ? entity1["bsd_name"] : (object)""));
            entity5["bsd_terminationdate"] = (object)DateTime.Now;
            entity5["bsd_optionentry"] = entity1["bsd_optionentry"];
            entity5["bsd_followuplist"] = (object)entity1.ToEntityReference();
            entity5["bsd_terminationtype"] = (object)false;
            entity4["bsd_name"] = (object)("Terminate letter of " + (entity1.Contains("bsd_name") ? entity1["bsd_name"] : (object)""));
            entity4["bsd_subject"] = (object)"Terminate letter - Follow Up List";
            entity4["bsd_date"] = (object)DateTime.Now;
            entity4["bsd_terminatefee"] = (object)new Money(0M);
            entity4["bsd_optionentry"] = entity1["bsd_optionentry"];
            entity4["bsd_customer"] = (object)entityReference;
            entity4["bsd_units"] = entity1["bsd_units"];
            entity4["bsd_signedcontractdate"] = entity3["bsd_signedcontractdate"];
            this.service.Create(entity5);
            this.service.Create(entity4);
            this.service.Update(new Entity(inputParameter.LogicalName)
            {
                Id = inputParameter.Id,
                ["statuscode"] = (object)new OptionSetValue(100000000)
            });
            this.service.Execute((OrganizationRequest)new SetStateRequest()
            {
                EntityMoniker = new EntityReference()
                {
                    Id = entity2.Id,
                    LogicalName = entity2.LogicalName
                },
                State = new OptionSetValue(0),
                Status = new OptionSetValue(2)
            });
        }

        private EntityCollection getFollowUpList(IOrganizationService crmservices)
        {
            string query = string.Format("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                  <entity name='Followuplist'>\r\n                    <attribute name='bsd_name' />\r\n                    <attribute name='bsd_date' />\r\n                    <attribute name='bsd_type' />\r\n                    <attribute name='bsd_units' />\r\n                    <attribute name='bsd_expiredate' />\r\n                    <order attribute='bsd_name' descending='false' />\r\n                    <filter type='and'>\r\n                      <condition attribute='bsd_name' operator='not-null' />\r\n                      <condition attribute='bsd_type' operator='eq' value='100000006' />                      \r\n                      </condition>\r\n                    </filter>                    \r\n                  </entity>\r\n                </fetch>");
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(query));
        }

        private EntityCollection get_OE_in_Terminate(Guid opID)
        {
            QueryExpression query = new QueryExpression("bsd_termination");
            query.ColumnSet = new ColumnSet(new string[1]
            {
        "bsd_optionentry"
            });
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object)opID));
            query.TopCount = new int?(1);
            return this.service.RetrieveMultiple((QueryBase)query);
        }

        private EntityCollection get_OE_in_Terminateletter(Guid opID)
        {
            QueryExpression query = new QueryExpression("bsd_terminateletter");
            query.ColumnSet = new ColumnSet(new string[1]
            {
        "bsd_optionentry"
            });
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("bsd_optionentry", ConditionOperator.Equal, (object)opID));
            query.TopCount = new int?(1);
            return this.service.RetrieveMultiple((QueryBase)query);
        }
    }
}
