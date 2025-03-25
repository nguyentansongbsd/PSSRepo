var intervalId=null
function ready() {
    debugger;
    var timer = null;
    function wait() {
        if (window.top.$ != null
            && window.top.$.fn != null) {
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
                }, 2000)
            }
        }
        else
            timer = setTimeout(function () { wait() }, 1000);
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
    var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
    if (statuscode == 100000000 || statuscode == 100000001 ) {
        disableFormFields(true);
    }
    else {
        //disableFormFields(false);
    }
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
function onFieldChange(executionContext) {
    var formContext = executionContext.getFormContext(); // Lấy ngữ cảnh form

    // Kiểm tra xem trường cụ thể đã thay đổi hay chưa
    var fieldValue = formContext.getAttribute("bsd_project").getValue(); // Thay "your_field_name" bằng tên trường của bạn

    // Nếu trường có giá trị, thực hiện lưu
    if (fieldValue !== null) {
        //formContext.data.entity.save(); // Lưu form
    }
}
function onSaveReload(executionContext) {
    var formContext = executionContext.getFormContext();
    var saveEventArgs = executionContext.getEventArgs();

    Xrm.Page.getAttribute("bsd_errordetail").setValue(null);
    Xrm.Page.getAttribute("bsd_error").setValue(false);
    formContext.data.entity.addOnPostSave(function () {

        window.parent.processingDlg.show();

        var intervalId = setInterval(function () {
            checkPA();
        }, 2000)
    });
}
var ishowErr = 0;
function checkPA() {
    var fetchXml = [
        "<fetch top='50'>",
        "  <entity name='bsd_updateduedate'>",
        "    <attribute name='bsd_error'/>",
        "    <attribute name='bsd_errordetail'/>",
        "    <attribute name='bsd_processing_pa'/>",
        "    <filter>",
        "      <condition attribute='bsd_updateduedateid' operator='eq' value='", Xrm.Page.data.entity.getId(), "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>"
    ].join("");
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
                        },null);
                    }
                }
            }
        }
    });
}

    
