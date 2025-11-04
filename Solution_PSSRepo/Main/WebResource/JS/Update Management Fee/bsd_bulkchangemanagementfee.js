function onLoad() {
    var statuscode = crmcontrol.getValue("statuscode");
    var bsd_powerautomate = Xrm.Page.getAttribute("bsd_powerautomate").getValue();
    if (bsd_powerautomate == true) {
        window.top.processingDlg.show();
        var id = setInterval(frame, 30000);
        function frame() {
            var fetchXml = ["<fetch>", "  <entity name='bsd_bulkchangemanagementfee'>", "    <attribute name='bsd_powerautomate'/>", "    <filter>", "      <condition attribute='bsd_bulkchangemanagementfeeid' operator='eq' value='", Xrm.Page.data.entity.getId(), "'/>", "      <condition attribute='bsd_powerautomate' operator='eq' value='0'/>", "    </filter>", "  </entity>", "</fetch>", ].join("");
            var entities = CrmFetchKitNew.FetchSync(fetchXml);
            if (entities.length > 0) {
                clearInterval(id);
                window.top.processingDlg.hide();
                Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId());
            }
        }
    }
    else if (statuscode != 1) {
        crmcontrol.disabledForm();
    }
}
//----------------------------------- Bulk Change Mana Fee Approve ---------------------------------------
function btn_BulkChangeManaFee_Approve() {
    debugger;
    crmcontrol.save();
    window.top.$ui.Confirm("Confirms", "Are you sure to Approve Bulk Change Management Fee?", function (e) {
        window.top.processingDlg.show();
        debugger;
        var id = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
        ExecuteAction("", "", "bsd_Action_BulkChangeManaFee_Approve", [{
            name: 'input01',
            type: 'string',
            value: "Bước 01"
        },
        {
            name: 'input02',
            type: 'string',
            value: id
        }], function (result) {
            debugger;
            if (result != null) {
                if (result.status == "error") {
                    window.top.processingDlg.hide();
                    window.top.$ui.Dialog("Message", result.data);
                }
                else {
                    crmcontrol.disabledForm();
                    callFlow(result.data.output01.value, id, result.data.output02.value);
                    window.top.processingDlg.hide();
                    setTimeout(Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId()), 2000);
                }
            }
        },
        true);
    },
    null);
}
function callFlow(userID, id, flowURL) {
    let body = {
        "id": id,
        "userID": userID
    };
    let req = new XMLHttpRequest();
    req.open("POST", flowURL, true);
    req.setRequestHeader("Content-Type", "application/json");
    req.send(JSON.stringify(body));
}
function btnVis_BulkChangeManaFee_Approve() {
    var formtype = Xrm.Page.ui.getFormType();
    var role = crmcontrol.checkRoles("CLVN_S&M_Head of Sale") || crmcontrol.checkRoles("CLVN_S&M_Sales Manager");

    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (formtype != 1 && status == 1 && role) {
        return true;
    }
    return false;
}