
if (window.$ == null) window.$ = window.parent.$;

// EnableRule : add script load

var isShowNoti = 0;
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

ready();
function Approve(item) {
    for (var j = 0, len = item.length; j < len; j++) {
        window.top.processingDlg.show();
        window.top.ExecuteAction(
            null, null, "bsd_Action_ShareCustomerToTeam", [
            {
                name: 'sharecusid',
                type: 'string',
                value: item[j].Id
            }
        ],
            function (result) {
                window.top.processingDlg.hide();
                if (result != null && result.status != null) {
                    window.top.location.reload();
                    if (result.status == "error") alertdialogConfirm(result.data);
                    else if (result.status == "success") {
                        window.top.location.reload();
                    }
                }
            }, true);
    }
}
function ApproveForm() {
    window.top.processingDlg.show();
    window.top.ExecuteAction(
        null, null, "bsd_Action_ShareCustomerToTeam", [
        {
            name: 'sharecusid',
            type: 'string',
            value: Xrm.Page.data.entity.getId()
        }
    ],
        function (result) {
            window.top.processingDlg.hide();
            if (result != null && result.status != null) {
                if (result.status == "error") alertdialogConfirm(result.data);
                else if (result.status == "success") {
                    window.top.location.reload();
                }
            }
        }, true);
}