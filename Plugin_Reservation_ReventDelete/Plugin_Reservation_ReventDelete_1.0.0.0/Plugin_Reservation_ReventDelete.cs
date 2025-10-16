// Decompiled with JetBrains decompiler
// Type: Plugin_Reservation_ReventDelete.Plugin_Reservation_ReventDelete
// Assembly: Plugin_Reservation_ReventDelete, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e832cc3a4133e911
// MVID: 1655C8E0-54AB-4939-A9D0-D520D0CE6912
// Assembly location: D:\BSD\PL_ALL_DLL\Plugin\Plugin_Reservation_ReventDelete_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Threading;

namespace Plugin_Reservation_ReventDelete
{
    public class Plugin_Reservation_ReventDelete : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext service1 = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = this.factory.CreateOrganizationService(new Guid?(((IExecutionContext)service1).UserId));
            ITracingService service2 = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            EntityReference inputParameter = (EntityReference)((DataCollection<string, object>)((IExecutionContext)service1).InputParameters)["Target"];
            if (!(inputParameter.LogicalName == "quote") || !(((IExecutionContext)service1).MessageName == "Delete"))
                return;
            if (((OptionSetValue)this.service.Retrieve(inputParameter.LogicalName, inputParameter.Id, new ColumnSet(new string[1]
            {
        "statuscode"
            }))["statuscode"]).Value == 100000006)
                throw new InvalidPluginExecutionException("This Quotaion Reservation was Collected! You can't delete it.");
        }
    }
}
