// JavaScript source code
$ = window.parent.$;
jQuery = window.parent.jQuery;
var intervalId = null;
var _previousStatusValue = null; // To store the status value on load and on valid change

function ready() {
    debugger;
    var timer = null;
    function wait() {
        if (window.top.$ != null && window.top.$.fn != null) {
            if (timer != null) {
                clearTimeout(timer);
                timer = null;
            }
            RegisterModal();
            console.log("bsd_processing_pa : " + Xrm.Page.getAttribute("bsd_processing_pa").getValue());
            if (Xrm.Page.getAttribute("bsd_processing_pa").getValue() == 1) {
                window.parent.processingDlg.show();
                intervalId = setInterval(function () {
                    checkPA();
                },
                2000)
            }
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

    var script3 = window.parent.document.getElementById("ClientGlobalContext.js.aspx");
    if (script3 == null) {
        script3 = window.parent.document.createElement("script");
        script3.type = "text/javascript";
        script3.id = "ClientGlobalContext.js.aspx";
        script3.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/ClientGlobalContext.js.aspx";
        window.parent.document.head.appendChild(script3);
    }
    var script4 = window.parent.document.getElementById("new_jquery2.0.3.min");
    if (script4 == null) {
        script4 = window.parent.document.createElement("script");
        script4.type = "text/javascript";
        script4.id = "new_jquery2.0.3.min";
        script4.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/new_jquery2.0.3.min";
        window.parent.document.head.appendChild(script4);
    }
    var script5 = window.parent.document.getElementById("bsd_CrmFetchKit.js");
    if (script5 == null) {
        script5 = window.parent.document.createElement("script");
        script5.type = "text/javascript";
        script5.id = "bsd_CrmFetchKit.js";
        script5.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_CrmFetchKit.js";
        window.parent.document.head.appendChild(script5);
    }
}
function onLoad(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    _previousStatusValue = formContext.getAttribute("statuscode").getValue();

    var formType = formContext.ui.getFormType();
    // If Form is in create mode
    if (formType === 1) {
        // Check if user has 'CLVN_S&M_Senior Sale Staff' role
        if (checkUserRole("CLVN_S&M_Senior Sale Staff")) {
            // Set bsd_types to 'Update only for units' (100000000)
            formContext.getAttribute("bsd_types").setValue(100000000);
            formContext.getControl("bsd_types").setDisabled(true);
        }
    }

    ready();
    SHOW_HIDE_LOCK();
}

function SHOW_HIDE_LOCK() {
    debugger;
    // Block all field when status is Approved
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (status && status == 100000001 || status == 100000002) blockAllField();
}

function blockAllField() {
    Xrm.Page.ui.controls.forEach(
    function (control, index) {
        if (control.getControlType() != "subgrid") control.setDisabled(true);
    });
}
function onSaveReload(executionContext) {
    var formContext = executionContext.getFormContext();
    var saveEventArgs = executionContext.getEventArgs();
    formContext.data.entity.addOnPostSave(function () {

        window.parent.processingDlg.show();
        var intervalId = setInterval(function () {
            checkPA();
        },
        2000)
    });
}
var ishowErr = 0;
function checkPA() {
    var fetchXml = ["<fetch top='50'>", "  <entity name='bsd_updateestimatehandoverdate'>", "    <attribute name='bsd_error'/>", "    <attribute name='bsd_errordetail'/>", "    <attribute name='bsd_processing_pa'/>", "    <filter>", "      <condition attribute='bsd_updateestimatehandoverdateid' operator='eq' value='", Xrm.Page.data.entity.getId(), "'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
    window.top.CrmFetchKit.Fetch(fetchXml, false).then(function (rs) {
        if (rs.length > 0) {
            if (rs[0].attributes.bsd_processing_pa.value == false) {
                window.parent.processingDlg.hide();
                if (rs[0].attributes.bsd_error && rs[0].attributes.bsd_error.value === true) {
                    window.parent.processingDlg.hide();
                    if (ishowErr == 0) {
                        ishowErr = 1;
                        window.top.$ui.Confirm("Error", rs[0].attributes.bsd_errordetail.value, function () {
                            window.top.location.reload();
                        },
                        null);
                    }
                }
            }
        }
    });
}

/**
 * Checks if the current user has a specific role.
 * @param {string} roleName The name of the security role to check.
 * @returns {boolean} True if the user has the role, false otherwise.
 */
function checkUserRole(roleName) {
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
    var hasRole = false;
    userRoles.forEach(function (role) {
        if (role.name === roleName) {
            hasRole = true;
        }
    });
    return hasRole;
}

/**
 * Handles the logic when the status field changes.
 * @param {object} executionContext The execution context from the form event.
 */
function onChangeStatus(executionContext) {
    var formContext = executionContext.getFormContext();
    var statusAttribute = formContext.getAttribute("statuscode");
    var newStatus = statusAttribute.getValue();

    // --- PLEASE VERIFY OPTION SET VALUES ---
    // Dynamics 365 uses integer values for Option Sets.
    // The values used here are based on common practices and existing code.
    // Please verify them against your system's configuration.
    var statusValues = {
        active: 1,
        submitted: 100000000,
        approved: 100000001, // Value from SHOW_HIDE_LOCK
        rejected: 100000002  // Value from SHOW_HIDE_LOCK
    };

    var isSalesManager = checkUserRole("CLVN_S&M_Sales Manager");
    var isSeniorSaleStaff = checkUserRole("CLVN_S&M_Senior Sale Staff");

    var isValidChange = true;
    var errorMessage = "";

    if (isSalesManager) {
        if (newStatus !== statusValues.approved && newStatus !== statusValues.rejected) {
            isValidChange = false;
            errorMessage = "As a Sales Manager, you can only change the status to Approved or Rejected.";
        }
    } else if (isSeniorSaleStaff) {
        if (newStatus !== statusValues.active && newStatus !== statusValues.submitted) {
            isValidChange = false;
            errorMessage = "As a Senior Sale Staff, you can only change the status to Active or Submitted.";
        }
    }
    // Note: If a user has both roles, the first condition (Sales Manager) will take precedence.
    // If a user has neither role, no validation will occur with this logic.

    if (!isValidChange) {
        formContext.ui.clearNotification("status_validation"); // Clear previous notification
        formContext.ui.setFormNotification(errorMessage, "ERROR", "status_validation");
        statusAttribute.setValue(_previousStatusValue); // Revert to the previous valid value
    } else {
        formContext.ui.clearNotification("status_validation"); // Clear notification on valid change
        _previousStatusValue = newStatus; // Update the previous value to the new, valid one
    }
}
