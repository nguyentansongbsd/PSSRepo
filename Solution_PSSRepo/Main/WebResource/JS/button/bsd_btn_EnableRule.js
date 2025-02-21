if (window.$ == null) window.$ = window.parent.$;
function ready() {
    debugger;
    var timer = null;
    function wait() {
        if (window.$ != null && window.$.fn != null) {
            if (timer != null) {
                clearTimeout(timer);
                timer = null;
            }
            RegisterModal();
        }
        else timer = setTimeout(function () {
            wait()
        },
            1000);
    }
    wait();
}

function RegisterModal() {
    debugger;
    var script = window.parent.document.getElementById("new_modal.utilities.js");
    if (script == null) {
        script = window.parent.document.createElement("script");
        script.type = "text/javascript";
        script.id = "new_modal.utilities.js";
        script.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/new_modal.utilities.js";
        window.parent.document.head.appendChild(script);
    }
    var script2 = window.top.document.getElementById("bsd_processingdialog.js");
    if (script2 == null) {
        script2 = window.top.document.createElement("script");
        script2.type = "text/javascript";
        script2.id = "bsd_processingdialog.js";
        script2.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_processingdialog.js";
        window.top.document.head.appendChild(script2);
    }
    var script1 = window.parent.document.getElementById("bsd_execute.services.ultilities.js");
    if (script1 == null) {
        script1 = window.parent.document.createElement("script");
        script1.type = "text/javascript";
        script1.id = "bsd_execute.services.ultilities.js";
        script1.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_execute.services.ultilities.js";
        window.parent.document.head.appendChild(script1);
    }
    var script3 = window.parent.document.getElementById("bsd_CrmFetchKit.js");
    if (script3 == null) {
        script3 = window.parent.document.createElement("script");
        script3.type = "text/javascript";
        script3.id = "bsd_CrmFetchKit.js";
        script3.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_CrmFetchKit.js";
        window.parent.document.head.appendChild(script3);
    }
    var script4 = window.parent.document.getElementById("bsd_Payment_Notices.html");
    if (script4 == null) {
        script4 = window.parent.document.createElement("script");
        script4.type = "text/javascript";
        script4.id = "bsd_Payment_Notices.html";
        script4.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_Payment_Notices.html";
        window.parent.document.head.appendChild(script4);
    }

}
ready();
function CheckRoleForUser(rolename) {
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
    //PL_INVESTOR, PL_INVERTOR Manager
    for (var i = 0; i < userRoles.getLength(); i++) {
        if (userRoles.get(i).name === rolename) {
            return true; // Hiện nút nếu người dùng có vai trò
        }
    }
    return false; // Ẩn nút nếu không có vai trò
}

function CheckEnable_Role() {

    switch (Xrm.Page.data.entity.getEntityName()) {
        case "bsd_updateduedate":
            if (CheckRoleForUser("System Administrator")) return true
            else return false;
        case "bsd_updateduedatedetail":
            if (CheckRoleForUser("System Administrator")) return true
            else return false;
        case "bsd_updateduedateoflastinstallmentapprove":
            if (CheckRoleForUser("System Administrator")) return true
            else return false;
        case "bsd_updateduedateoflastinstallment":
            if (CheckRoleForUser("System Administrator")) return true
            else return false;
        case "bsd_terminateletter":
            if (CheckRoleForUser("System Administrator")) return true
            else return false;
        case "bsd_updateestimatehandoverdate":
            if (CheckRoleForUser("System Administrator")) return true
            else return false;
        case "bsd_updateestimatehandoverdatedetail":
            if (CheckRoleForUser("System Administrator")) return true
            else return false;
        default:
            return false;
    }
}

function CheckEnable_Condition(tyle) {
    debugger;
    let status = Xrm.Page.getAttribute("statuscode").getValue();
    switch (Xrm.Page.data.entity.getEntityName() + "-" + tyle) {
        case "bsd_updateduedateoflastinstallmentapprove-subgriddetail":

            if (getStatusCodeValueByName("Approved") === status)
                return false;
            break;
        case "bsd_updatelandvalue-bsd_landvalue-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status)
                return false;
            break;
        case "bsd_updateduedate-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status)
                return false;
            break;
        case "bsd_updateestimatehandoverdate-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status)
                return false;
            break;
        case "bsd_discount-form_statuscode":
            if (getStatusCodeValueByName("Approved") === status)
                return false;
            break;
        case "bsd_packageselling-form_statuscode":
            if (getStatusCodeValueByName("Approved") === status)
                return false;
            break;
        case "bsd_phaseslaunch-bsd_promotion-subgriddetail":
            if (getStatusCodeValueByName("Launched") != status && getStatusCodeValueByName("Not Launch") != status)
                return false;
            break;
        case "bsd_phaseslaunch-bsd_packageselling-subgriddetail":
            if (getStatusCodeValueByName("Launched") != status && getStatusCodeValueByName("Not Launch") != status)
                return false;
            break;
        default:
            return false;
    }
    return true;
}
function getStatusCodeValueByName(optionName) {
    var statusCodeField = Xrm.Page.getAttribute("statuscode");
    if (statusCodeField) {
        var options = statusCodeField.getOptions();

        for (var i = 0; i < options.length; i++) {
            if (options[i].text === optionName) {
                return options[i].value;
            }
        }
        console.warn("Option with name '" + optionName + "' not found.");
        return null;
    } else {
        console.error("Field 'statuscode' not found.");
        return null;
    }
}
function getStatusCodeOptions() {
    var statusCodeField = Xrm.Page.getAttribute("statuscode");
    if (statusCodeField) {
        var options = statusCodeField.getOptions();
        var result = [];

        options.forEach(function (option) {
            result.push({
                value: option.value,
                text: option.text
            });
        });

        return result;
    } else {
        console.error("Field 'statuscode' not found.");
        return null;
    }
}
function GetDetailEntityName() {
    switch (Xrm.Page.data.entity.getEntityName()) {
        case "bsd_updateduedate":
            return "bsd_updateduedatedetail"
            break;
        case "bsd_updateduedateoflastinstallmentapprove":
            return "bsd_updateduedateoflastinstallment"
            break;
        case "bsd_updateestimatehandoverdate":
            return "bsd_updateestimatehandoverdatedetail"
            break;
        default:
            return "";
    }
}
function GetDetailFieldMasterName() {
    switch (Xrm.Page.data.entity.getEntityName()) {
        case "bsd_updateduedate":
            return "bsd_updateduedatedetail"
            break;
        case "bsd_updateduedateoflastinstallmentapprove":
            return "bsd_updateduedateoflastinstallment"
            break;
        case "bsd_updateestimatehandoverdate":
            return "bsd_updateestimatehandoverdatedetail"
            break;
        default:
            return "";
    }
}
function CheckDetailForMaster(entityName, entityId) {
    var fieldMaster = GetDetailFieldMasterName();
    var entityNameDetail = GetDetailEntityName();
    var fetchData = {
        "bsd_updateestimatehandoverdate": Xrm.Page.data.entity.getId()
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='", entityNameDetail, "'>",
        "    <filter>",
        "      <condition attribute='", fieldMaster, "' operator='eq' value='", fetchData.bsd_updateestimatehandoverdate/*00000000-0000-0000-0000-000000000000*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>"
    ].join("");
}

