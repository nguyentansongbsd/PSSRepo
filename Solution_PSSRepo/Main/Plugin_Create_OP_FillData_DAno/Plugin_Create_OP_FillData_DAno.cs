using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Create_OP_FillData_DAno
{
    public class Plugin_Create_OP_FillData_DAno : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            FillBsdDaNoField(enCreated,service,tracingService);
        }
        private void FillBsdDaNoField(Entity entity, IOrganizationService service, ITracingService tracingService)
        {
            // Pseudocode:
            // 1. Retrieve project name and option no from entity.
            // 2. Switch/case on project name:
            //    - Heritage West Lake: left 3 chars + "/" + right 4 chars
            //    - LUMI SIGNATURE: "LUMI/" + right 4 chars
            //    - LUMI PRESTIGE: "LUMI PRESTIGE/" + right 4 chars
            //    - SENIQUE I&II: "SENIQUE/" + right 4 chars
            //    - SENIQUE PREMIER: "SENIQUE PREMIER/" + right 4 chars
            //    - Default: left 8 chars + "/" + right 4 chars
            // 3. Set bsd_da_no field.
            // 4. Trace each step.

            try
            {
                tracingService.Trace("Start FillBsdDaNoField");

                // Example attribute names, adjust as needed
                string projectName = entity.Contains("bsd_project") ? entity.GetAttributeValue<EntityReference>("bsd_project").Name : null;
                string optionNo = entity.Contains("bsd_optionno") ? entity.GetAttributeValue<string>("bsd_optionno") : null;

                tracingService.Trace($"Project Name: {projectName}");
                tracingService.Trace($"Option No: {optionNo}");

                if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(optionNo))
                {
                    tracingService.Trace("Project name or option no is missing. Skipping fill.");
                    return;
                }

                string bsdDaNo = string.Empty;

                switch (projectName.Trim().ToUpper())
                {
                    case "HERITAGE WEST LAKE":
                        tracingService.Trace("Case: HERITAGE WEST LAKE");
                        bsdDaNo = $"{Left(optionNo, 3)}/{Right(optionNo, 4)}";
                        break;
                    case "LUMI SIGNATURE":
                        tracingService.Trace("Case: LUMI SIGNATURE");
                        bsdDaNo = $"LUMI/{Right(optionNo, 4)}";
                        break;
                    case "LUMI PRESTIGE":
                        tracingService.Trace("Case: LUMI PRESTIGE");
                        bsdDaNo = $"LUMI PRESTIGE/{Right(optionNo, 4)}";
                        break;
                    case "SENIQUE I&II":
                        tracingService.Trace("Case: SENIQUE I&II");
                        bsdDaNo = $"SENIQUE/{Right(optionNo, 4)}";
                        break;
                    case "SENIQUE PREMIER":
                        tracingService.Trace("Case: SENIQUE PREMIER");
                        bsdDaNo = $"SENIQUE PREMIER/{Right(optionNo, 4)}";
                        break;
                    default:
                        tracingService.Trace("Case: Other Project");
                        bsdDaNo = $"{Left(optionNo, 8)}/{Right(optionNo, 4)}";
                        break;
                }

                tracingService.Trace($"bsd_da_no value to set: {bsdDaNo}");

                entity["bsd_da_no"] = bsdDaNo;
                service.Update(entity);

                tracingService.Trace("End FillBsdDaNoField");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Exception in FillBsdDaNoField: {ex.Message}");
                throw;
            }
        }

        // Helper functions
        private string Left(string input, int length)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= length ? input : input.Substring(0, length);
        }

        private string Right(string input, int length)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= length ? input : input.Substring(input.Length - length, length);
        }
    }

}
