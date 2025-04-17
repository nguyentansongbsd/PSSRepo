using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace Action_PaymetsNoticesMail_Wordtemplate
{
    public partial class Action_PaymetsNoticesMail_Wordtemplate : BasePlugin
    {
        public Action_PaymetsNoticesMail_Wordtemplate(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig)
        {
            // Register for any specific events by instantiating a new instance of the 'PluginEvent' class and registering it
            base.RegisteredEvents.Add(new PluginEvent()
            {
                Stage = Stage.PostOperation,
                MessageName = MessageNames.bsd_Action_PaymetsNoticesMail_Wordtemplate,
                
                PluginAction = ExecutePluginLogic
            });
        }

        public void ExecutePluginLogic(IServiceProvider serviceProvider)
        {
            // Use a 'using' statement to dispose of the service context properly
            // To use a specific early bound entity replace the 'Entity' below with the appropriate class type
            using (var localContext = new LocalPluginContext<Entity>(serviceProvider))
            {
                // Todo: Place your logic here for the plugin
                
            }
        }
    }
}
