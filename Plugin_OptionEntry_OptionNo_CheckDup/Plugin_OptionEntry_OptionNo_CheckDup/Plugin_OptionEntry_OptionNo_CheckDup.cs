using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin_OptionEntry_OptionNo_CheckDup
{
    public class Plugin_OptionEntry_OptionNo_CheckDup : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("Start");
            tracingService.Trace("Message: " + context.MessageName);
            try
            {
                Init().Wait();
            }
            catch (AggregateException ex)
            {
                var innerException = ex.InnerExceptions.FirstOrDefault();
                if (innerException != null)
                {
                    throw innerException;
                }
            }
        }
        public async Task Init()
        {
            tracingService.Trace("vao update");
            if (context.MessageName != "Update") return;
            try
            {
                Entity target = (Entity)context.InputParameters["Target"];
                Entity targetEntity = this.service.Retrieve(target.LogicalName, target.Id,new ColumnSet(new string[] { "statuscode", "bsd_optionno", "bsd_project" }));
                tracingService.Trace(((OptionSetValue)targetEntity["statuscode"]).Value.ToString());
                if (((OptionSetValue)targetEntity["statuscode"]).Value != 100000000) return;

                await Task.Delay(3000);
                Entity enAutoNumProject = getAutoNumberProject((EntityReference)targetEntity["bsd_project"]);
                string optionNo = targetEntity["bsd_optionno"].ToString();
                string sufix = enAutoNumProject.Contains("bsd_sufix") && (bool)enAutoNumProject["bsd_usecustom"] == true ? enAutoNumProject["bsd_sufix"].ToString() : null;
                if (!string.IsNullOrWhiteSpace(sufix)) optionNo = optionNo.Substring(0, optionNo.Length - sufix.Length);

                string currentNumOfOptionNo = getOptionNo(optionNo, (EntityReference)targetEntity["bsd_project"]);
                bool isDub = checkDupOptionNo(currentNumOfOptionNo, ((EntityReference)targetEntity["bsd_project"]).Id, targetEntity.Id);
                tracingService.Trace("is dup: " + isDub);
                if (isDub == true)
                {
                    int currentNum = (int)enAutoNumProject["bsd_currentnumber"];
                    currentNum++;
                    int numLength = currentNum.ToString().Length;
                    
                    string optionNo_new = optionNo.Substring(0, optionNo.Length - numLength) + currentNum + sufix;
                    await Task.WhenAll(updateAutoNumberProject(currentNum, enAutoNumProject), updateOE(targetEntity, optionNo_new));
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private bool checkDupOptionNo(string optionNo, Guid projectId, Guid salesorderId)
        {
            try
            {
                tracingService.Trace("Option No: " + optionNo);
                if (string.IsNullOrWhiteSpace(optionNo)) return false;
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""salesorder"">
                    <attribute name=""bsd_optionno"" />
                    <filter>
                      <condition attribute=""bsd_project"" operator=""eq"" value=""{projectId}"" />
                      <condition attribute=""salesorderid"" operator=""ne"" value=""{salesorderId}"" />
                    </filter>
                  </entity>
                </fetch>";
                var salesorder = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                tracingService.Trace("fetch: " + fetchXml);
                if (salesorder.Entities.Count > 0)
                {
                    bool isContain = salesorder.Entities.Any(x => x["bsd_optionno"].ToString().Contains(optionNo));
                    return isContain;
                }
                else return false;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private Entity getAutoNumberProject(EntityReference enfProject)
        {
            try
            {
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                <fetch>
                  <entity name=""bsd_autonumberproject"">
                    <attribute name=""bsd_sufix"" />
                    <attribute name=""bsd_usecustom"" />
                    <attribute name=""bsd_currentnumber"" />
                    <attribute name=""bsd_autonumberprojectid"" />
                    <filter>
                      <condition attribute=""bsd_project"" operator=""eq"" value=""{enfProject.Id}"" />
                      <condition attribute=""bsd_entitylogical"" operator=""eq"" value=""salesorder"" />
                    </filter>
                  </entity>
                </fetch>";
                var rsAutoNumber = this.service.RetrieveMultiple(new FetchExpression(fetchXml));
                tracingService.Trace("fetch autonumber: " + fetchXml);
                return rsAutoNumber.Entities[0];
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task updateAutoNumberProject(int currentNum, Entity enAutoNumberProject)
        {
            try
            {
                tracingService.Trace("Start upda autonumber");
                Entity enAutoNumProject = new Entity(enAutoNumberProject.LogicalName, enAutoNumberProject.Id);
                enAutoNumProject["bsd_currentnumber"] = currentNum;
                this.service.Update(enAutoNumProject);
                tracingService.Trace("End upda autonumber");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private async Task updateOE(Entity enOE, string optionNo)
        {
            try
            {
                tracingService.Trace("Start upda OE");
                Entity enOE_UD = new Entity(enOE.LogicalName, enOE.Id);
                enOE_UD["bsd_optionno"] = optionNo;
                this.service.Update(enOE_UD);
                tracingService.Trace("End upda OE");
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private string getOptionNo(string optionNo, EntityReference enfProject)
        {
            try
            {
                int length = getLength_Project(enfProject);
                if (length == 0) return null;
                Match match = Regex.Match(optionNo, $@"\d{{{length}}}");
                if (match.Success)
                    return match.Value;
                else
                    return null;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
        private int getLength_Project(EntityReference enfProject)
        {
            try
            {
                Entity enProject = this.service.Retrieve(enfProject.LogicalName, enfProject.Id, new ColumnSet(new string[] { "bsd_length" }));
                return enProject.Contains("bsd_length") ? (int)enProject["bsd_length"] : 0;
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
        }
    }
}
