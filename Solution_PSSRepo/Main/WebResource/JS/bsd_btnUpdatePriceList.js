$ = window.parent.$;
function ready() {
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
    var script = document.createElement("script");
    script.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_execute.services.ultilities.js";
    script.type = "text/javascript";
    script.onload = function () {
        console.log("Script loaded successfully execute.");
    };
    script.onerror = function () {
        console.error("Lỗi khi tải WebResource execute.");
    };

    var script_crmControl = document.createElement("script");
    script_crmControl.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_execute.services.ultilities.js";
    script_crmControl.type = "text/javascript";
    script_crmControl.onload = function () {
        console.log("Script loaded successfully crmControl.");
    };
    script_crmControl.onerror = function () {
        console.error("Lỗi khi tải WebResource crmControl.");
    };

    document.head.appendChild(script_crmControl);
    document.head.appendChild(script);

}
ready();

function vis_btn_view_approve(items) {
    return true;
}

function btn_view_approve(items) {
    debugger;
    var confirmStrings = {
        text: "Are you sure to Approve?",
        title : "Confirmation"
    };
    var confirmOptions = {
        height: 200,
        width: 450
    };
    Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
    function (success) {
        if (!success.confirmed) return;
        Xrm.Utility.showProgressIndicator("Please wait");
        items.forEach(item => {
            ExecuteAction(
            item, "bsd_updatepricelist", "bsd_Action_UpdatePriceList_Approve", null,

            function (result) {
                Xrm.Utility.closeProgressIndicator();
                if (result != null) {
                    debugger;
                    if (result.status == "error") {
                        var ss = result.data.split(':');
                        var mss = ss[ss.length - 1];
                        var alertStrings = {
                            confirmButtonLabel: "OK",
                            text: mss,
                            title: "Notices"
                        };
                        var alertOptions = {
                            height: 200,
                            width: 400
                        };

                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                        function (success) {
                            if (success.confirmed) {
                                console.log("Người dùng đã nhấn OK.");
                            } else {
                                console.log("Lỗi dialog");
                            }
                        });
                    }
                    else if (result.status == "success") {
                        var alertStrings = {
                            confirmButtonLabel: "OK",
                            text: "Success.",
                            title: "Notices"
                        };
                        var alertOptions = {
                            height: 200,
                            width: 400
                        };

                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                        function (success) {
                            window.top.location.reload(true);
                        });
                    }
                }
            });
        });
    });
}
function vis_btn_approve() {
    var sts = crmcontrol.getValue("statuscode");
    var role = crmcontrol.checkRoles("CLVN_CCR Manager") || crmcontrol.checkRoles("CLVN_S&M_Head of Sale") || crmcontrol.checkRoles("System Administrator") ? true : false;

    if (role&&sts == 1 && Xrm.Page.ui.getFormType() != 1) return true;
    else return false;
}
function btn_approve() {
    debugger;
    var confirmStrings = {
        text: "Are you sure to Approve?",
        title : "Confirmation"
    };
    var confirmOptions = {
        height: 200,
        width: 450
    };
    Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
    function (success) {
        if (!success.confirmed) return;
        Xrm.Utility.showProgressIndicator("Please wait");
        ExecuteAction(
        crmcontrol.getId(), "bsd_updatepricelist", "bsd_Action_UpdatePriceList_Approve", null,

        function (result) {
            Xrm.Utility.closeProgressIndicator();
            if (result != null) {
                debugger;
                if (result.status == "error") {
                    var ss = result.data.split(':');
                    var mss = ss[ss.length - 1];
                    var alertStrings = {
                        confirmButtonLabel: "OK",
                        text: mss,
                        title: "Notices"
                    };
                    var alertOptions = {
                        height: 200,
                        width: 400
                    };

                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                    function (success) {
                        if (success.confirmed) {
                            console.log("Người dùng đã nhấn OK.");
                        } else {
                            console.log("Lỗi dialog");
                        }
                    });
                }
                else if (result.status == "success") {
                    var alertStrings = {
                        confirmButtonLabel: "OK",
                        text: "Success.",
                        title: "Notices"
                    };
                    var alertOptions = {
                        height: 200,
                        width: 400
                    };

                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                    function (success) {
                        window.top.location.reload(true);
                    });
                }
            }
        });
    });
}