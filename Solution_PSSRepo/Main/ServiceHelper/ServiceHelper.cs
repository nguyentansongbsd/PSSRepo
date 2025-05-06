using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHelper
{
    public class ServiceHelper
    {
        private IOrganizationService _service = null;
        private EntityReference _userContext=null;
        private ITracingService tracingService = null;
        private string _actionName = "PA";
        public ServiceHelper(IOrganizationService service, EntityReference userContext, ITracingService tracingService, string actionName="PA")
        {
            _service = service;
            _userContext = userContext;
            this.tracingService = tracingService;
            _actionName = actionName;
        }
        public   void Update(Entity enUpdate)
        {
            try
            {
                tracingService.Trace(enUpdate.LogicalName);
                var enUser=_service.Retrieve(_userContext.LogicalName, _userContext.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                enUpdate["bsd_lastmodifiedlog"] =$"Last Moidified By : {enUser["domainname"].ToString()}\n- Modified On:{DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm")}\n- Action: {_actionName}";
                _service.Update(enUpdate);
            }
            catch(Exception ex)
            {
                enUpdate.Attributes.Remove("bsd_lastmodifiedlog");
                _service.Update(enUpdate);
                tracingService.Trace("not found: field (bsd_lastmodifiedby)"+"- entity: "+enUpdate.LogicalName);
            }
        }
    }
}
