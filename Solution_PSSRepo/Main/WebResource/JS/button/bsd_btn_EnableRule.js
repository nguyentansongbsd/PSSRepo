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
function DisableStatus() {
    switch (Xrm.Page.data.entity.getEntityName()) {
        case "bsd_interestratemaster":
            if (!CheckRoleForUser("CLVN_CCR Manager") && !CheckRoleForUser("CLVN_FIN_Finance Manager"))
                Xrm.Page.ui.controls.get("header_statuscode").setDisabled(true);
            break;
        case "bsd_updatelandvalue":
            if (!CheckRoleForUser("CLVN_S&M_Sales Manager") && CheckRoleForUser("CLVN_S & M_Head of Sale"))
                Xrm.Page.ui.controls.get("header_statuscode").setDisabled(true);
            break;
        case "bsd_updateactualareaapprove":
            if (!CheckRoleForUser("CLVN_S&M_Sales Manager") && CheckRoleForUser("CLVN_S & M_Head of Sale"))
                Xrm.Page.ui.controls.get("header_statuscode").setDisabled(true);
            break;
        case "bsd_capnhatphiquanly":
            if (!CheckRoleForUser("CLVN_S&M_Sales Manager") && CheckRoleForUser("CLVN_S & M_Head of Sale"))
                Xrm.Page.ui.controls.get("header_statuscode").setDisabled(true);
            break;
        case "bsd_refund":
            if (!CheckRoleForUser("CLVN_FIN_Finance Manager"))
                Xrm.Page.ui.controls.get("header_statuscode").setDisabled(true);
            break;
        case "bsd_waiverapproval":
            if (!CheckRoleForUser("CLVN_FIN_Finance Manager"))
                Xrm.Page.ui.controls.get("header_statuscode").setDisabled(true);
            break;
        case "bsd_updateestimatehandoverdate":
            if (!CheckRoleForUser("CLVN_FIN_Finance Manager") && !CheckRoleForUser("CLVN_S&M_Sales Manager") && !CheckRoleForUser("CLVN_S&M_Senior Sale Staff"))
                Xrm.Page.ui.controls.get("header_statuscode").setDisabled(true);
            break;
    }

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
    let status = -1;
    if (Xrm.Page.getAttribute("statuscode")) {

         status = Xrm.Page.getAttribute("statuscode").getValue();
    }
    var key = "";
    var formType = Xrm.Page.ui.getFormType();
    if (formType === 1 || formType === 2 || formType === 3) {
        // Đây là form (Create, Update, Read-Only)
        key = Xrm.Page.data.entity.getEntityName();
    } else {
        // Có thể là view hoặc nơi khác
        var pageContext = Xrm.Utility.getPageContext();
        key = pageContext.input.entityName
    }
    switch (key + "-" + tyle) {
        case "bsd_sharecustomers-FormApprove":
            if ((getStatusCodeValueByName("Approved") === status) || ((!CheckRoleForUser("CLVN_S&M_Head of Sale") && !CheckRoleForUser("CLVN_S&M_Sales Manager") && !CheckRoleForUser("System Administrator")))) return false;
            break;
        case "bsd_sharecustomers-ViewApprove":
            if ((!CheckRoleForUser("CLVN_S&M_Head of Sale") && !CheckRoleForUser("CLVN_S&M_Sales Manager") && !CheckRoleForUser("System Administrator"))) return false;
            break;
        case "bsd_updateduedateoflastinstallmentapprove-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status) return false;
            break;
        case "bsd_updateduedateoflastinstallmentapprove-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status) return false;
            break;
        case "bsd_updateduedateoflastinstallmentapprove-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status) return false;
            break;
        case "bsd_bulksendmailmanager-form-Delete":
            if (getStatusCodeValueByName("Sent") === status) return false;
            break;
        case "email-form-Delete":
            if (getStatusCodeValueByName("Sent") === status) return false;
            break;
        case "bsd_updatelandvalue-bsd_landvalue-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status) return false;
            break;
        case "bsd_updateduedate-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status) return false;
            break;
        case "bsd_updateestimatehandoverdate-subgriddetail":
            if (getStatusCodeValueByName("Approved") === status) return false;
            break;
        case "bsd_discount-form_statuscode":
            if (getStatusCodeValueByName("Approved") === status) return false;
            break;
        case "bsd_packageselling-form_statuscode":
            if (getStatusCodeValueByName("Approved") === status) return false;
            break;
        case "bsd_phaseslaunch-bsd_promotion-subgriddetail":
            if (getStatusCodeValueByName("Launched") != status && getStatusCodeValueByName("Not Launch") != status) return false;
            break;
        case "bsd_phaseslaunch-bsd_packageselling-subgriddetail":
            if (getStatusCodeValueByName("Launched") != status && getStatusCodeValueByName("Not Launch") != status) return false;
            break;
        case "salesorder-form-PrintHOM":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break;
        case "salesorder-form-InterestSimulation":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break;
        case "salesorder-form-SubSale":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break;
        case "salesorder-form-PrintHOM":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break;
        case "salesorder-form-InterestSimulation":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break;
        case "quote-form-PrintRevervationForm":
            if (getStatusCodeValueByName("Won") == status) return false;
            break;
        case "quote-form-Applydocument":
            if (!CheckRoleForUser("CLVN_S&M_Sales Manager") && !CheckRoleForUser("System Administrator")) return false;
            break;
        case "quote-form-FULTerminate":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break;
        case "quote-form-CreateOrder":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break;
        case "quote-form-PrintQuotaionFinal":
            if (getStatusCodeValueByName("Terminated") == status || getStatusCodeValueByName("Won") == status) return false;
            break;
        case "quote-form-ConvertToOption":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break
        case "quote-form-FUL":
            if (getStatusCodeValueByName("Terminated") == status) return false;
            break;
        case "quote-form-Recalculator":
            if (getStatusCodeValueByName("Terminated") == status || getStatusCodeValueByName("Won") == status) return false;
            break;

        case "bsd_discount-form-Approved":
            if (!CheckRoleForUser("CLVN_S&M_Sales Manager") && !CheckRoleForUser("System Administrator") && !CheckRoleForUser("CLVN_S&M_Head of Sale")) return false;
            break;
        case "bsd_confirmpayment-form-Confirm":
            if (!CheckRoleForUser("CLVN_FIN_Finance Manager") && !CheckRoleForUser("System Administrator")) return false;
            break;
        case "bsd_payment-form-Confirm":
            if (!CheckRoleForUser("CLVN_FIN_Finance Manager") && !CheckRoleForUser("System Administrator")) return false;
            break;
        case "bsd_payment-form-VoidPayment":
            if ((!CheckRoleForUser("CLVN_FIN_Finance Manager") && !CheckRoleForUser("System Administrator"))) return false;
            break;
        case "bsd_packageselling-form-Approved":
            if (CheckRoleForUser("CLVN_S&M_Senior Sale Staff")) return false;
            break;
        case "bsd_advancepayment-form-ConfirmCollect":
            if (!CheckRoleForUser("CLVN_FIN_Finance Manager") && !CheckRoleForUser("System Administrator")) return false;
            break;
        case "bsd_bankingloan-form-Mortgage":
            if (getStatusCodeValueByName("Terminated")) return false;
            break;
        case "bsd_bankingloan-form-Demortgage":
            if (getStatusCodeValueByName("CLVN_FIN_Finance Manager")) return false;
            break;
        case "quote-bsd_packageselling-subgriddetail":
        case "quote-bsd_promotion-subgriddetail":
            var isExist = Xrm.Page.getAttribute("bsd_existinstallment").getValue();
            return !isExist;
        case "salesorder-bsd_promotion-subgriddetail":
            return false;
        default:
            return true;
    }
    return true;
}
function CheckEnable_ConditionView(tyle, item) {
    switch (Xrm.Page.data.entity.getEntityName() + "-" + tyle) {
        case "email-View-Delete":
            //#region check list item có statecode = sent (3)
            var fetchXml =
                "<fetch top='1'>" +
                "  <entity name='" + item[0].TypeName + "'>" +
                "    <attribute name='bsd_name'/>" +
                "    <attribute name='bsd_project'/>" +
                "    <filter>" +
                "      <condition attribute='activityid'  operator='in' />"
            for (var i = 0; i < item.length; i++) {
                fetchXml += "      <value>" + item[i].Id + "</value>";
            }
            "      </condition>" +
                "<condition attribute='statuscode' operator='eq' value='3'/>"

            "    </filter>" +
                "  </entity>" +
                "</fetch>";
            //#endregion
            rm.WebApi.retrieveMultipleRecords("email", "?fetchXml=" + fetchXml).then(
                function (results) {
                    if (results.entities.length > 0) {
                        return false;
                    }
                    else {
                        return true;
                    }
                });
            break;
        case "email-SubGrid-Delete":
            //#region check list item có statecode = sent (3)
            var fetchXml =
                "<fetch top='1'>" +
                "  <entity name='" + item[0].TypeName + "'>" +
                "    <attribute name='bsd_name'/>" +
                "    <attribute name='bsd_project'/>" +
                "    <filter>" +
                "      <condition attribute='activityid'  operator='in' />"
            for (var i = 0; i < item.length; i++) {
                fetchXml += "      <value>" + item[i].Id + "</value>";
            }
            "      </condition>" +
                "<condition attribute='statuscode' operator='eq' value='3'/>"

            "    </filter>" +
                "  </entity>" +
                "</fetch>";
            //#endregion
            rm.WebApi.retrieveMultipleRecords("email", "?fetchXml=" + fetchXml).then(
                function (results) {
                    if (results.entities.length > 0) {
                        return false;
                    }
                    else {
                        return true;
                    }
                });
            break;
        case "bsd_bulksendmailmanager-View-Delete":
            //#region check list item có statecode = sent (3)
            var fetchXml =
                "<fetch top='1'>" +
                "  <entity name='" + item[0].TypeName + "'>" +
                "    <attribute name='bsd_name'/>" +
                "    <attribute name='bsd_project'/>" +
                "    <filter type='and'>" +
                "      <condition attribute='bsd_bulksendmailmanagerid'  operator='in' />"
            for (var i = 0; i < item.length; i++) {
                fetchXml += "      <value>" + item[i].Id + "</value>";
            }
            "      </condition>" +
                "<condition attribute='statuscode' operator='eq' value='100000001'/>"
            "    </filter>" +
                "  </entity>" +
                "</fetch>";
            //#endregion
            rm.WebApi.retrieveMultipleRecords("bsd_bulksendmailmanager", "?fetchXml=" + fetchXml).then(
                function (results) {
                    if (results.entities.length > 0) {
                        return false;
                    }
                    else {
                        return true;
                    }
                });
    }
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
    var fetchXml = ["<fetch>", "  <entity name='", entityNameDetail, "'>", "    <filter>", "      <condition attribute='", fieldMaster, "' operator='eq' value='", fetchData.bsd_updateestimatehandoverdate
        /*00000000-0000-0000-0000-000000000000*/
        , "'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
}