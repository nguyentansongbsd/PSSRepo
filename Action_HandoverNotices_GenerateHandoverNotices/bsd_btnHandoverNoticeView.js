// JavaScript source code

if (window.$ == null)
    window.$ = window.parent.$;

// EnableRule : add script load 
function btn_GenerateEnableRule() {
    return true;
}

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

}

ready();

function btnView_Generate() {
    debugger;
    try {
        //Code th�nh
        Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_HandoverNotice_table.html", { width: 380, height: 325 }, null, null);
        //window.top.$ui.Confirm("Confirm", "Do you want to generate handover notices?", function (e) {
        //    window.parent.processingDlg.show();
        //    window.top.ExecuteAction(
        //    ""
        //    , ""
        //    , "bsd_Action_HandoverNotices_GenerateHandoverNotices"
        //    , null//[{ name: 'ReturnId', type: 'string', value: null }]
        //    , function (result) {
        //        window.parent.processingDlg.hide();
        //        if (result != null && result.status != null) {
        //            if (result.status == "error")
        //                window.top.$ui.Dialog("Message", result.data);
        //            else if (result.status == "success") {
        //                //window.parent.Xrm.Utility.openEntityForm("bsd_warningnotices", result.data.ReturnId.value, null, { openInNewWindow: true });
        //                if (result.data.ReturnId) {
        //                    if (result.data.ReturnId.value >= 1) {
        //                        var grid = document.getElementById("crmGrid");
        //                        if (grid && grid.control)
        //                            grid.control.refresh();
        //                    }
        //                    window.top.$ui.Dialog("Message", "Generated " + result.data.ReturnId.value + (result.data.ReturnId.value > 1 ? " records." : " record."));

        //                }
        //            }
        //            else {
        //                console.log(JSON.stringify(result));
        //            }
        //        }
        //    }, true);
        //}, null);

    }
    catch (e) {
        window.top.$ui.Dialog("Error", e.message, null);
    }
}



