using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Active_CollectionMeeting_Complete
{
    public class Action_Active_CollectionMeeting_Complete : IPlugin
    {
        private IOrganizationService service = (IOrganizationService)null;
        private IOrganizationServiceFactory factory = (IOrganizationServiceFactory)null;
        IPluginExecutionContext context = null;
        ITracingService tracingService = null;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {

        }
    }
}
