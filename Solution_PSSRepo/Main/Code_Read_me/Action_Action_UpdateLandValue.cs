using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Action_UpdateLandValue
{
    public class Action_Action_UpdateLandValue : IPlugin
    {

        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;

        public void Execute(IServiceProvider serviceProvider)
        {
        }
    }
}
