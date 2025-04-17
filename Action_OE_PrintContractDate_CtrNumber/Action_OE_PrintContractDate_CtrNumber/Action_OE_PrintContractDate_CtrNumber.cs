using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Action_OE_PrintContractDate_CtrNumber
{
    public class Action_OE_PrintContractDate_CtrNumber : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            EntityReference inputParameter = (EntityReference)service1.InputParameters["Target"];
            if (!(inputParameter.LogicalName == "salesorder"))
                return;
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(service1.UserId));
            tracingService.Trace("Vào Action_OE_PrintContractDate_CtrNumber");
            Entity entity1 = this.service.Retrieve("salesorder", inputParameter.Id, new ColumnSet(new string[10]
            {
                "statuscode",
                "bsd_signedcontractdate",
                "bsd_contractprinteddate",
                "bsd_paymentscheme",
                "ordernumber",
                "bsd_unitnumber",
                "bsd_contractnumber",
                "bsd_identifynumber",
                "bsd_project",
                "bsd_optionno"
            }));
            if (entity1.Contains("bsd_contractnumber") && entity1["bsd_contractnumber"] == null || !entity1.Contains("bsd_contractnumber"))
            {
                int num1 = ((OptionSetValue)entity1["statuscode"]).Value;
                Guid id = ((EntityReference)entity1["bsd_paymentscheme"]).Id;
                if (!entity1.Contains("ordernumber"))
                    throw new InvalidPluginExecutionException("Contract does not contain 'Option Number'!");
                if (!entity1.Contains("bsd_project"))
                    throw new InvalidPluginExecutionException("Contract does not contain 'Project'!");
                if (entity1.Contains("bsd_signedcontractdate"))
                    throw new InvalidPluginExecutionException("Option Entry has already signed!");
                if (!entity1.Contains("bsd_contractnumber"))
                {
                    Entity entity2 = new Entity("salesorder");
                    entity2.Id = entity1.Id;
                    Entity entity3 = this.service.Retrieve(((EntityReference)entity1["bsd_project"]).LogicalName, ((EntityReference)entity1["bsd_project"]).Id, new ColumnSet(new string[4]
                    {
            "bsd_currentnumber",
            "bsd_name",
            "bsd_prefixcontract",
            "bsd_length"
            
                    }));

                    if (!entity3.Contains("bsd_prefixcontract"))
                        throw new InvalidPluginExecutionException("Can not find Prefix (Contract) data in Project " + (entity3.Contains("bsd_name") ? (string)entity3["bsd_name"] : "") + ". Please check again!");
                    int num2 = entity3.Contains("bsd_length") ? (int)entity3["bsd_length"] : throw new InvalidPluginExecutionException("Can not find Length data in Project " + (entity3.Contains("bsd_name") ? (string)entity3["bsd_name"] : "") + ". Please check again!");
                    this.service.Retrieve("product", new Entity("product")
                    {
                        Id = this.getProductId(inputParameter.Id)
                    }.Id, new ColumnSet(new string[3]
                    {
            "statuscode",
            "bsd_signedcontractdate",
            "name"
                    }));
                    if (entity1.Contains("bsd_project"))
                    {
                        string projectCode = (string)entity1["bsd_optionno"];
                        Match match = Regex.Match(projectCode, $@"\d{{{num2}}}");
                        string lastFourCharacters = match.Success ? match.Value : null;
                        entity2["bsd_contractnumber"] = (object)((string)entity3["bsd_prefixcontract"] + "/" + lastFourCharacters);
                        this.service.Update(entity2);

                        //tracingService.Trace("Vào case dự án lumi lấy trường bsd_prefixcontract bên dự án + 4 ký tự cuối trường optiono bên salesoder để gán cho trường contract number Thịnh làm");
                        //Guid idpro = ((EntityReference)entity1["bsd_project"]).Id;
                        //Guid check = new Guid("{779F0E92-CC3D-EF11-A316-6045BD1BAA9C}");
                        //Guid check1 = new Guid("{FC2C423E-FA85-EF11-AC20-0022485725FB}");
                        //Guid check2 = new Guid("{1D227A77-66F5-EE11-A1FE-000D3AC96AE0}");
                        //Guid check3 = new Guid("{2F0AC773-AEA1-EF11-8A69-000D3AA3F0A0}");// SENIQUE PREMIER
                        //Guid check4 = new Guid("{941833DF-A7A1-EF11-8A69-000D3AA3F0A0}");//SENIQ HN
                        //if (idpro == check || idpro == check1 || idpro == check2 || idpro == check3 || idpro == check4)
                        //{
                        //    string projectCode = (string)entity1["bsd_optionno"];
                        //    string lastFourCharacters = projectCode.Substring(projectCode.Length - 4);
                        //    entity2["bsd_contractnumber"] = (object)((string)entity3["bsd_prefixcontract"] + "/"+ lastFourCharacters);
                        //    this.service.Update(entity2);
                        //}
                        //else
                        //{
                        //    //int num3 = (entity3.Contains("bsd_currentnumber") ? (int)entity3["bsd_currentnumber"] : 0) + 1;
                        //    //int num4 = num3;
                        //    //string str = num3.ToString();
                        //    //int num5 = num2 - str.Length;
                        //    //for (int index = 0; index < num5; ++index)
                        //    //    str = "0" + str;
                        //    //entity2["bsd_contractnumber"] = (object)((string)entity3["bsd_prefixcontract"] + str);
                        //    //this.service.Update(entity2);
                        //    //this.service.Update(new Entity(entity3.LogicalName)
                        //    //{
                        //    //    Id = entity3.Id,
                        //    //    ["bsd_currentnumber"] = (object)num4
                        //    //});
                        //    string projectCode = (string)entity1["bsd_optionno"];
                        //    string lastFourCharacters = projectCode.Substring(projectCode.Length - 4);
                        //    entity2["bsd_contractnumber"] = (object)((string)entity3["bsd_prefixcontract"] + "/" + lastFourCharacters);
                        //    this.service.Update(entity2);
                        //}
                    }


                }
            }
        }

        private Guid getProductId(Guid salesorderId)
        {
            Guid productId = new Guid();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
            stringBuilder.AppendLine("<entity name='salesorderdetail'>");
            stringBuilder.AppendLine("<attribute name='productid' />");
            stringBuilder.AppendLine("<filter type='and'>");
            stringBuilder.AppendLine(string.Format("<condition attribute='salesorderid' operator='eq' value='{0}'/>", (object)salesorderId));
            stringBuilder.AppendLine("</filter>");
            stringBuilder.AppendLine("</entity>");
            stringBuilder.AppendLine("</fetch>");
            EntityCollection entityCollection = this.service.RetrieveMultiple((QueryBase)new FetchExpression(stringBuilder.ToString()));
            if (entityCollection.Entities.Count <= 0)
                throw new InvalidPluginExecutionException("Cannot find product information in this Contract!");
            foreach (Entity entity in (Collection<Entity>)entityCollection.Entities)
                productId = ((EntityReference)entity.Attributes["productid"]).Id;
            return productId;
        }
    }
}
