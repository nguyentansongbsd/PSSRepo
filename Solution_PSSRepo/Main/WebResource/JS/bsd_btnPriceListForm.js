function btn_ApproveEnable() {
    return Xrm.Page.getAttribute("bsd_approved").getValue() != true;
}
function btn_Approve() {
    debugger;
    var id_rice = Xrm.Page.data.entity.getId();
    window.top.$ui.Confirm("Confirms", "Are you sure to Approve Price List?", function (e) {
        if (checkApprove()) {
            return;
        }
        Xrm.Page.getAttribute("bsd_automate").setValue(true);
        crmcontrol.save();
        window.top.processingDlg.show();
        debugger;
        var fetchXml = ["<fetch top='1'>", "  <entity name='bsd_configgolive'>", "    <attribute name='bsd_url'/>", "    <filter>", "      <condition attribute='bsd_name' operator='eq' value='Price List Approve'/>", "      <condition attribute='bsd_url' operator='not-null'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
        var rs = CrmFetchKitNew.FetchSync(fetchXml);
        if (rs.length > 0) {
            callFlow(id_rice, rs[0].attributes.bsd_url.value);
            var intervalId = setInterval(function () {
                var fetchXml = ["<fetch top='1'>", "  <entity name='pricelevel'>", "    <attribute name='bsd_automate'/>", "    <filter>", "      <condition attribute='pricelevelid' operator='eq' value='", id_rice, "'/>", "      <condition attribute='bsd_automate' operator='eq' value='0'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
                var rsCheck = CrmFetchKitNew.FetchSync(fetchXml);
                if (rsCheck.length > 0) {
                    clearInterval(intervalId);
                    window.top.processingDlg.hide();
                    Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), id_rice);
                }
            },
            10000);
        }
        else {
            window.top.processingDlg.hide();
            window.top.$ui.Dialog("Message", "Link to run PA not found. Please check again.");
        }
    },
    null);
}
function callFlow(Output01, flowURL) {
    debugger;
    let body = {
        "id": Output01.replace("{", "").replace("}", "")
    };
    let req = new XMLHttpRequest();
    req.open("POST", flowURL, true); // true for asynchronous
    req.setRequestHeader("Content-Type", "application/json");
    req.send(JSON.stringify(body));
}
//end
function btn_ApproveRequest() {
    var enRef = Xrm.Page.data.entity.getEntityReference();
    var params = {
        command: "pricelevel",
        targetType: enRef.entityType,
        targetId: enRef.id,
        targetName: enRef.name
    };
    debugger;
    window.top.$ui.Confirm("Question", "Are you sure to send approval request?", function (e) {
        ExecuteAction(
        null, null, "new_Action_EmailApproveRequest", [{
            name: 'Parameter',
            type: 'string',
            value: JSON.stringify(params)
        }], function (result) {
            if (result != null) {
                if (result.status == "error") {
                    var ss = result.data.split(':');
                    var mss = ss[ss.length - 1];
                    window.top.$ui.Dialog("Message", mss);
                }
                else if (result.data != null && result.data.Result != null) {
                    try {
                        var message = eval(result.data.Result.value);
                        if (message.type == "Success") {
                            window.parent.Xrm.Utility.openEntityForm("quote", message.content, null, {
                                openInNewWindow: true
                            });
                        }
                        else window.top.$ui.Dialog(message.type, message.content, null);
                    }
                    catch(e) {
                        window.top.$ui.Dialog("Message", e.message, null);
                    }
                }
            }
        },
        true);
    },
    null);
}
//-------------------------------------- btnCopyPL---------------------------------------
function btnCopyPL() {
    try {
        debugger;
        var id = Xrm.Page.data.entity.getId();
        var localname = "pricelevel";
        if (id != null) {
            window.top.$ui.Confirm("Confirm", "Would you want to Copy this Price List?", function (e) {
                window.top.processingDlg.show();
                ExecuteAction(id, localname, "bsd_Action_PhasesLaunch_btncopyPRL", null, function (result) {
                    debugger;
                    window.top.processingDlg.hide();
                    if (result != null && result.status != null) {
                        if (result.status == "error") {
                            var array = result.data.split(':');
                            var recordLast = array[array.length - 1];
                            window.top.$ui.Dialog("Message", recordLast);
                        }
                        else {
                            var info = result.data.Data.value;
                            window.top.$ui.Dialog("Message", "Successfully! " + info);
                            Xrm.Page.data.refresh();
                        }
                    }
                });
            },
            null);
        }
    }
    catch(e) {
        window.top.$ui.Dialog("Message", e.message, null);
    }
}

function btnVis_btnCopyPL() {
    return false
}
function btnVis_Approve() {
    var role = crmcontrol.checkRoles("CLVN_CCR Manager") || crmcontrol.checkRoles("CLVN_S&M_Head of Sale") || crmcontrol.checkRoles("System Administrator") ? true : false;
    var approver = Xrm.Page.getAttribute("bsd_approved").getValue();
    var formtype = Xrm.Page.ui.getFormType();
    if (role && approver == false && formtype != 1) {
        return true;
    }
    return false;
}
function btnVis_SendApprovalRequest() {
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1) {
        return false;
    }
    return false;
}
function disableFormFields(onOff) {
    function doesControlHaveAttribute(control) {
        var controlType = control.getControlType();
        return controlType != "iframe" && controlType != "webresource" && controlType != "subgrid";
    }
    Xrm.Page.ui.controls.forEach(function (control, index) {
        if (doesControlHaveAttribute(control)) {
            control.setDisabled(onOff);
        }
    });
}

function checkApprove() {
    var formId = crmcontrol.getId();

    var fetchXml = ["<fetch top='1'>", "  <entity name='productpricelevel'>", "    <attribute name='productpricelevelid'/>", "    <filter>", "      <condition attribute='pricelevelid' operator='eq' value='", formId, "'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length <= 0) {
        crmcontrol.openAlertDialog("The price list does not include any units. Please check the information again.", "Notices");
        return true;
    }

    var fetchXml = ["<fetch top='1'>", "  <entity name='productpricelevel'>", "    <attribute name='productpricelevelid'/>", "    <attribute name='amount'/>", "    <filter>", "      <condition attribute='pricelevelid' operator='eq' value='", formId, "'/>", "      <filter type='or'>", "        <condition attribute='amount' operator='null'/>", "        <condition attribute='amount' operator='le' value='0'/>", "      </filter>", "    </filter>", "  </entity>", "</fetch>"].join("");
    rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) {
        crmcontrol.openAlertDialog("The product has an invalid price. Please check the information again.", "Notices");
        return true;
    }

    return false;
}