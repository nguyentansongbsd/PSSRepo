// Decompiled with JetBrains decompiler
// Type: Plugin_AppendixContract_CreateAdvancePayment.Plugin_AppendixContract_CreateAdvancePayment
// Assembly: Plugin_AutoShare_All, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e09245f531e270dc
// MVID: 1EE1234C-401D-4F40-BF45-5057E52B59CF
// Assembly location: C:\Users\ngoct\Downloads\Plugin_AutoShare_All_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Plugin_AppendixContract_CreateAdvancePayment
{
    public class Plugin_AppendixContract_CreateAdvancePayment : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        private ITracingService traceService = (ITracingService)null;
        private IPluginExecutionContext context = (IPluginExecutionContext)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(this.context.UserId));
            this.traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity inputParameter = (Entity)this.context.InputParameters["Target"];
            Entity entity1 = this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(true));

            List<string> source = new List<string>();

            // Retrieve teams associated with the current user
            foreach (Entity entity2 in (Collection<Entity>)this.service.RetrieveMultiple(
                new FetchExpression(string.Format(
                    "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
                    "<fetch distinct=\"true\">\r\n" +
                    "  <entity name=\"team\">\r\n" +
                    "    <attribute name=\"teamid\" />\r\n" +
                    "    <attribute name=\"name\" />\r\n" +
                    "    <link-entity name=\"teammembership\" from=\"teamid\" to=\"teamid\" intersect=\"true\">\r\n" +
                    "      <link-entity name=\"systemuser\" from=\"systemuserid\" to=\"systemuserid\" intersect=\"true\">\r\n" +
                    "        <filter>\r\n" +
                    "          <condition attribute=\"systemuserid\" operator=\"eq\" value=\"{0}\" />\r\n" +
                    "        </filter>\r\n" +
                    "      </link-entity>\r\n" +
                    "    </link-entity>\r\n" +
                    "  </entity>\r\n" +
                    "</fetch>", this.context.UserId))).Entities)
            {
                source.Add(((string)entity2["name"]).Split('-')[0]);
            }

            // Retrieve teams where the current user is the administrator
            foreach (Entity entity3 in (Collection<Entity>)this.service.RetrieveMultiple(
                new FetchExpression(string.Format(
                    "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
                    "<fetch distinct=\"true\">\r\n" +
                    "  <entity name=\"team\">\r\n" +
                    "    <attribute name=\"teamid\" />\r\n" +
                    "    <attribute name=\"name\" />\r\n" +
                    "    <filter>\r\n" +
                    "      <condition attribute=\"administratorid\" operator=\"eq\" value=\"{0}\" />\r\n" +
                    "    </filter>\r\n" +
                    "  </entity>\r\n" +
                    "</fetch>", this.context.UserId))).Entities)
            {
                source.Add(((string)entity3["name"]).Split('-')[0]);
            }

            // Share privileges for distinct team names
            foreach (string str in source.Distinct().ToList())
            {
                foreach (Entity entity4 in (Collection<Entity>)this.service.RetrieveMultiple(
                    new FetchExpression(
                        "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
                        "<fetch>\r\n" +
                        "  <entity name=\"team\">\r\n" +
                        "    <attribute name=\"name\" />\r\n" +
                        "    <attribute name=\"teamid\" />\r\n" +
                        "    <filter>\r\n" +
                        "      <condition attribute=\"name\" operator=\"like\" value=\"" + str + "-SALE%\" />\r\n" +
                        "    </filter>\r\n" +
                        "  </entity>\r\n" +
                        "</fetch>")).Entities)
                {
                    this.Role_SharePrivileges(entity4.ToEntityReference(), entity1.ToEntityReference(), this.service, true, true, false, false);
                }
            }
        }

        private void Role_SharePrivileges(
          EntityReference USER,
          EntityReference Target,
          IOrganizationService service,
          bool write_Access,
          bool append_Access,
          bool assign,
          bool share)
        {
            try
            {
                AccessRights accessRights1 = (AccessRights)0;
                accessRights1 = (AccessRights)0;
                AccessRights accessRights2 = (AccessRights)1; // Khởi tạo với giá trị 1
                if (write_Access)
                    accessRights2 = accessRights2 != 0 ? accessRights2 | (AccessRights)2 : (AccessRights)2; // Thêm quyền ghi
                if (append_Access)
                    accessRights2 = accessRights2 != 0 ? accessRights2 | (AccessRights)(16 | 4) : (AccessRights)20; // Thêm quyền thêm
                GrantAccessRequest request = new GrantAccessRequest()
                {
                    PrincipalAccess = new PrincipalAccess()
                    {
                        AccessMask = accessRights2,
                        Principal = USER
                    },
                    Target = Target
                };
                service.Execute((OrganizationRequest)request);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while applying Sharing rules for the record. " + ex.Message);
            }
        }
    }
}
