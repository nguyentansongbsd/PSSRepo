using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Plugin_AutoNumberProject
{
    public class Plugin_AutoNumberProject : IPlugin
    {
        private IOrganizationServiceFactory serviceProxy;
        private IOrganizationService service;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (!(((IExecutionContext)service).MessageName == "Create") || !((DataCollection<string, object>)((IExecutionContext)service).InputParameters).Contains("Target") || !(((DataCollection<string, object>)((IExecutionContext)service).InputParameters)["Target"] is Entity))
                return;
            this.serviceProxy = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.serviceProxy.CreateOrganizationService(new Guid?(((IExecutionContext)service).UserId));
            Entity inputParameter = (Entity)((DataCollection<string, object>)((IExecutionContext)service).InputParameters)["Target"];
            if (inputParameter.LogicalName == "bsd_autonumberproject")
            {
                if (inputParameter["bsd_entitylogical"].ToString() == "bsd_autonumber" || inputParameter["bsd_entitylogical"].ToString() == "bsd_autonumberproject")
                    throw new InvalidPluginExecutionException("Can not use this entity!");
                if (!inputParameter.Contains("bsd_entitylogical") || !inputParameter.Contains("bsd_project") || !inputParameter.Contains("bsd_fieldlogical"))
                    throw new InvalidPluginExecutionException("Please enter info!");
                if (this.IsExisted(inputParameter["bsd_entitylogical"].ToString(), (EntityReference)inputParameter["bsd_project"], inputParameter["bsd_fieldlogical"].ToString()))
                    throw new InvalidPluginExecutionException("This rule has already existed!");
            }
            else if ((inputParameter.Contains("bsd_project") || inputParameter.Contains("bsd_projectid")) && inputParameter.LogicalName != "bsd_project")
            {
                string str1 = "bsd_project";
                if (inputParameter.Contains("bsd_projectid"))
                    str1 = "bsd_projectid";
                foreach (Entity entity1 in (Collection<Entity>)this.RetrieveAutoNumbers(this.service, inputParameter.LogicalName, (EntityReference)inputParameter[str1]).Entities)
                {
                    string str2 = "";
                    string str3 = "";
                    string str4 = "";
                    string str5 = "";
                    Entity entity2 = this.service.Retrieve(((EntityReference)inputParameter[str1]).LogicalName, ((EntityReference)inputParameter[str1]).Id, new ColumnSet(new string[3]
                    {
                    "bsd_length",
                    "bsd_currentnumber",
                    "bsd_projectcode"
                    }));
                    int num1 = entity2.Contains("bsd_length") ? (int)entity2["bsd_length"] : 0;
                    if (num1 <= 0)
                        break;
                    if (entity1.Contains("bsd_useprojectcode") && (bool)entity1["bsd_useprojectcode"])
                        str5 = entity2.Contains("bsd_projectcode") ? entity2["bsd_projectcode"].ToString() + "-" : "";
                    int num2 = entity1.Contains("bsd_currentnumber") ? Convert.ToInt32(entity1["bsd_currentnumber"].ToString()) : 0;
                    if (((DataCollection<string, object>)entity1.Attributes).Contains("bsd_usecustom") && (bool)entity1["bsd_usecustom"])
                    {
                        str2 = ((DataCollection<string, object>)entity1.Attributes).Contains("bsd_prefix") ? entity1["bsd_prefix"].ToString() : string.Empty;
                        str3 = ((DataCollection<string, object>)entity1.Attributes).Contains("bsd_sufix") ? entity1["bsd_sufix"].ToString() : string.Empty;
                    }
                    if (((DataCollection<string, object>)entity1.Attributes).Contains("bsd_useunitscode") && (bool)entity1["bsd_useunitscode"])
                    {
                        string str6 = ((DataCollection<string, object>)entity1.Attributes).Contains("bsd_fieldunitslogical") ? entity1["bsd_fieldunitslogical"].ToString() : string.Empty;
                        if (str6 != null && inputParameter.Contains(str6))
                        {
                            Entity entity3 = this.service.Retrieve(((EntityReference)inputParameter[str6]).LogicalName, ((EntityReference)inputParameter[str6]).Id, new ColumnSet(new string[2]
                            {
                            "name",
                            "productnumber"
                            }));
                            if (entity3.Contains("productnumber"))
                                str4 = entity3["productnumber"].ToString() + "-";
                        }
                    }
                    string str7 = ((DataCollection<string, object>)entity1.Attributes).Contains("bsd_fieldlogical") ? entity1["bsd_fieldlogical"].ToString() : string.Empty;
                    if (!string.IsNullOrWhiteSpace(str7))
                    {
                        ++num2;
                        this.service.Update(new Entity(entity1.LogicalName)
                        {
                            Id = entity1.Id,
                            ["bsd_currentnumber"] = (object)num2
                        });

                        int num3 = num1 - num2.ToString().Length;
                        if (num3 < 0)
                        {
                            num2 = 1;
                            num3 = num1 - 1;
                        }
                        string str8 = "";
                        for (int index = 0; index < num3; ++index)
                            str8 += "0";
                        inputParameter[str7] = (object)string.Format("{0}{1}{2}{3}{4}{5}", (object)str5, (object)str4, (object)str2, (object)str8, (object)num2.ToString(), (object)str3);
                        
                    }
                }
            }
        }

        private bool IsExisted(string entityName, EntityReference project, string field)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<?xml version='1.0' encoding='utf-8'?>");
            stringBuilder.AppendLine("<fetch aggregate='true' mapping='logical'>");
            stringBuilder.AppendLine("<entity name='bsd_autonumberproject'>");
            stringBuilder.AppendLine("<attribute name='bsd_autonumberprojectid' alias='rs' aggregate='count' />");
            stringBuilder.AppendLine("<filter type='and'>");
            stringBuilder.AppendLine("<condition attribute='bsd_entitylogical' operator='eq' value='" + entityName + "'/>");
            stringBuilder.AppendLine("<condition attribute='bsd_project' operator='eq' value='" + (object)project.Id + "'/>");
            stringBuilder.AppendLine("<condition attribute='bsd_fieldlogical' operator='eq' value='" + field + "'/>");
            stringBuilder.AppendLine("</filter>");
            stringBuilder.AppendLine("</entity>");
            stringBuilder.AppendLine("</fetch>");
            EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)new FetchExpression(stringBuilder.ToString()));
            return ((Collection<Entity>)entityCollection.Entities).Count <= 0 || !((DataCollection<string, object>)((Collection<Entity>)entityCollection.Entities)[0].Attributes).Contains("rs") || (int)((AliasedValue)((Collection<Entity>)entityCollection.Entities)[0]["rs"]).Value > 0;
        }

        private EntityCollection RetrieveAutoNumbers(
          IOrganizationService crmservices,
          string entityName,
          EntityReference project)
        {
            string str = string.Format("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>\r\n              <entity name='bsd_autonumberproject'>\r\n                <attribute name='bsd_autonumberprojectid' />\r\n                <attribute name='bsd_name' />\r\n                <attribute name='bsd_useprojectcode' />\r\n                <attribute name='bsd_useunitscode' />\r\n                <attribute name='bsd_usecustom' />\r\n                <attribute name='bsd_currentnumber' />\r\n                <attribute name='bsd_sufix' />\r\n                <attribute name='bsd_project' />\r\n                <attribute name='bsd_prefix' />\r\n                <attribute name='bsd_fieldunitslogical' />\r\n                <attribute name='bsd_fieldlogical' />\r\n                <attribute name='bsd_entitylogical' />\r\n                <order attribute='bsd_name' descending='false' />\r\n                <filter type='and'>\r\n                  <condition attribute='bsd_entitylogical' operator='eq' value='{0}' />\r\n                  <condition attribute='bsd_project' operator='eq' value='{1}' />\r\n                </filter>\r\n              </entity>\r\n            </fetch>", (object)entityName, (object)project.Id);
            return crmservices.RetrieveMultiple((QueryBase)new FetchExpression(str));
        }
    }
}