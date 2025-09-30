function onsave(executionContext) {

}
var intervalId = null
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
                    5000)
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
function onload() {

    ready();
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

function saveAsComplete() {
    window.parent.processingDlg.show();
    var entityId = Xrm.Page.data.entity.getId()
        , entityName = Xrm.Page.data.entity.getEntityName()
        , stateCode = 1
        , statusCode = -1
        , closePage = true;
    var record = {
        "statecode": 1,
         "statuscode":3
        // Thay đổi giá trị trường bạn muốn cập nhật
    };

    Xrm.WebApi.updateRecord("appointment", entityId, record).then(
        function success(result) {
            window.top.location.reload();
        },
        function (error) {
            console.log("Error updating record: " + error.message);
            // Xử lý lỗi nếu cần
        }
    );
}
var ishowErr = 0;
function checkPA() {
    var fetchXml = ["<fetch top='50'>", "  <entity name='appointment'>", "    <attribute name='bsd_error'/>", "    <attribute name='bsd_errordetail'/>", "    <attribute name='bsd_processing_pa'/>", "    <filter>", "      <condition attribute='appointmentid' operator='eq' value='", Xrm.Page.data.entity.getId(), "'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
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

function lockStatus() {
    //debugger;
    //var role = crmcontrol.checkRoles("CLVN_S&M_Senior Sale Staff") || crmcontrol.checkRoles("CLVN_S&M_Sales Manager") || crmcontrol.checkRoles("CLVN_S&M_Head of Sale") || crmcontrol.checkRoles("CLVN_FIN_Finance Manager");
    //var status = crmcontrol.getValue("statuscode");
    //if (role && status != 100000000) crmcontrol.setDisabled("statuscode", false);
    //else crmcontrol.setDisabled("statuscode", true);
}