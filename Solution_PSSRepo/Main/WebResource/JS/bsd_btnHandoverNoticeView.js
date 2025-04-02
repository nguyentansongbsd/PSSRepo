// JavaScript source code

if (window.$ == null) window.$ = window.parent.$;

// EnableRule : add script load
function btn_GenerateEnableRule() {
    return true;
}

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
    var script5 = window.parent.document.getElementById("bsd_CrmFetchKit.js");
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

function btnView_Generate() {
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_Handover_Notice_SelectProject.html",
        data: ""
    };
    var navigationOptions = {
        target: 2,
        // 2 is for opening the page as a dialog.
        width: 520,
        // default is px. can be specified in % as well.
        height: 300,
        // default is px. can be specified in % as well.
        position: 1 // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() { },
        function error(e) { });

    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_Handover_Notice_SelectProject.html", { width: 520, height: 240 }, null, null);

    //try {
    //    window.top.$ui.Confirm("Confirm", "Do you want to generate handover notices?", function (e) {
    //        window.parent.processingDlg.show();
    //        window.top.ExecuteAction(
    //            ""
    //            , ""
    //            , "bsd_Action_HandoverNotices_GenerateHandoverNotices"
    //            , null//[{ name: 'ReturnId', type: 'string', value: null }]
    //            , function (result) {
    //                window.parent.processingDlg.hide();
    //                if (result != null && result.status != null) {
    //                    if (result.status == "error")
    //                        window.top.$ui.Dialog("Message", result.data);
    //                    else if (result.status == "success") {
    //                        //window.parent.Xrm.Utility.openEntityForm("bsd_warningnotices", result.data.ReturnId.value, null, { openInNewWindow: true });
    //                        if (result.data.ReturnId) {
    //                            if (result.data.ReturnId.value >= 1) {
    //                                var grid = document.getElementById("crmGrid");
    //                                if (grid && grid.control)
    //                                    grid.control.refresh();
    //                            }
    //                            window.top.$ui.Dialog("Message", "Generated " + result.data.ReturnId.value + (result.data.ReturnId.value > 1 ? " records." : " record."));

    //                        }
    //                    }
    //                    else {
    //                        console.log(JSON.stringify(result));
    //                    }
    //                }
    //            }, true);
    //    }, null);

    //}
    //catch (e) {
    //    window.top.$ui.Dialog("Error", e.message, null);
    //}
}

//--- Han_15082018: BULK PRINT HANDOVER NOTICE ---
function btnView_BulkPrint(item) {
    debugger;
    var result = {};
    result.entityname = "";
    result.typecode = "";
    result.list_id = [];
    for (var j = 0, len = item.length; j < len; j++) {
        if (result.entityname == "") {
            var entityname = item[j].TypeName;
            result.entityname = entityname;
        }
        var id = item[j].Id;
        result.list_id.push(id);
    }
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_handovernotices_select_report.html",
        data: result.list_id.join(",")
    };
    var navigationOptions = {
        target: 2,
        // 2 is for opening the page as a dialog.
        width: 520,
        // default is px. can be specified in % as well.
        height: 400,
        // default is px. can be specified in % as well.
        position: 1 // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() { },
        function error(e) { });

    //var lstrow = document.getElementsByClassName("ms-crm-List-SelectedRow");
    //var result = {};
    //result.entityname = "";
    //result.list_id = [];

    //for (var j = 0, len = lstrow.length; j < len; j++) {
    //    if (result.entityname == "") {
    //        var entityname = $(lstrow[j].attributes[3]).context.nodeValue;
    //        result.entityname = entityname;
    //    }
    //    var id = lstrow[j].attributes[1].textContent;
    //    id = id.replace('{', '').replace('}', '');
    //    result.list_id.push(id);
    //}
    //// var urlreport = Xrm.Page.context.getClientUrl() + "/webresources/bsd_handovernotices_select_report.html?data=" + result.list_id.join(",");
    ////var urlreport = Xrm.Page.context.getClientUrl() + "/webresources/bsd_handovernotices_report.html?data=abc";
    //Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_handovernotices_select_report.html?data=" + result.list_id.join(","), { width: 410, height: 345 }, null, null);
    //var nameReport = "Handover Notices Form";
    //window.open(urlreport, nameReport, "resizable=1,width=380,height=344");
    //// alert("btnView_BulkPrint");
    ////Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_handovernotices_select_report.html?data=" + Xrm.Page.data.entity.getId(), { width: 410, height: 345 }, null, null);
    ////var urlreport = Xrm.Page.context.getClientUrl() + "/webresources/bsd_handovernotices_select_report.html?data=";
    ////var nameReport = "Handover notices report";
    ////window.open(urlreport, nameReport, "resizable=1,width=380,height=344");

}
function btnView_createFileSharePoint(item) {
    var result = {};
    result.entityname = "";
    result.typecode = "";
    result.list_id = [];
    for (var j = 0, len = item.length; j < len; j++) {
        if (result.entityname == "") {
            var entityname = item[j].TypeName;
            result.entityname = entityname;
            result.typecode = "createfile";
        }
        var id = item[j].Id;
        result.list_id.push(id);
    }
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_handovernotices_select_report.html",
        data: result.list_id.join(",") + "," + result.typecode
    };
    var navigationOptions = {
        target: 2,
        // 2 is for opening the page as a dialog.
        width: 520,
        // default is px. can be specified in % as well.
        height: 400,
        // default is px. can be specified in % as well.
        position: 1 // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() { },
        function error(e) { });

    //Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_handovernotices_select_report.html?type=createfile&data=" + result.list_id.join(","), { width: 410, height: 345 }, null, null);
    var nameReport = "Handover Notices Form";
    //  window.open(urlreport, nameReport, "resizable=1,width=380,height=344");
}
window.top.Handovernotice = function (project, enstimatehandover, bill_date) {
    window.parent.$('button[data-id="dialogCloseIconButton"]').click();
    try {
        console.log(project, enstimatehandover);
        window.top.$ui.Confirm("Confirm", "Do you want to generate handover notices?", function (e) {
            window.parent.processingDlg.show();
            window.top.ExecuteAction("", "", "bsd_Action_HandoverNotices_GenerateHandoverNotices", [{
                name: 'project',
                type: 'string',
                value: project
            },
            {
                name: 'enstimatehandover',
                type: 'string',
                value: enstimatehandover
            },
            {
                name: 'billdate',
                type: 'string',
                value: bill_date.toString()
            }], function (result) {
                window.parent.processingDlg.hide();
                if (result != null && result.status != null) {
                    if (result.status == "error") window.top.$ui.Dialog("Message", result.data);
                    else if (result.status == "success") {
                        //window.parent.Xrm.Utility.openEntityForm("bsd_warningnotices", result.data.ReturnId.value, null, { openInNewWindow: true });
                        if (result.data.ReturnId) {
                            if (result.data.ReturnId.value >= 1) {
                                var grid = document.getElementById("crmGrid");
                                if (grid && grid.control) grid.control.refresh();
                            }
                            window.top.$ui.Dialog("Message", "Generated " + result.data.ReturnId.value + (result.data.ReturnId.value > 1 ? " records." : " record."));

                        }
                    }
                    else {
                        console.log(JSON.stringify(result));
                    }
                }
            },
                true);
        },
            null);

    }
    catch (e) {
        window.top.$ui.Dialog("Error", e.message, null);
    }
}

function vis_btnBulkCheckSend() {
    return true;
}
function Btn_BulkCheckSend(item) {
    debugger;
    // Lấy lên danh sách id
    var result = {};
    result.entityname = "";
    result.typecode = "";
    result.list_id = [];
    for (var j = 0, len = item.length; j < len; j++) {
        if (result.entityname == "") {
            var entityname = item[j].TypeName;
            result.entityname = entityname;
            result.typecode = "Bulkcheckemail";
        }
        var id = item[j].Id;
        result.list_id.push(id);
    }
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_handovernotices_select_report.html",
        data: result.list_id.join(",") + "," + result.typecode
    };
    var navigationOptions = {
        target: 2,
        // 2 is for opening the page as a dialog.
        width: 520,
        // default is px. can be specified in % as well.
        height: 400,
        // default is px. can be specified in % as well.
        position: 1 // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() { },
        function error(e) { });
    // gọi report html chọn mẫu
    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_selectreport_customernotices.html?type=Bulkcheckemail&data=" + result.list_id.join(","), { width: 410, height: 345 }, null, null);
    var nameReport = "Handover Notices Form";
    // window.open(urlreport, nameReport, "resizable=1,width=380,height=344");
}
function Btn_BulkSendMail(item) {
    var result = {};
    result.entityname = "";
    result.typecode = "";
    result.list_id = [];
    for (var j = 0, len = item.length; j < len; j++) {
        if (result.entityname == "") {
            var entityname = item[j].TypeName;
            result.entityname = entityname;
            result.typecode = "Bulksendemail";
        }
        var id = item[j].Id;
        result.list_id.push(id);
    }
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_handovernotices_select_report.html",
        data: result.list_id.join(",") + "," + result.typecode
    };
    var navigationOptions = {
        target: 2,
        // 2 is for opening the page as a dialog.
        width: 520,
        // default is px. can be specified in % as well.
        height: 400,
        // default is px. can be specified in % as well.
        position: 1 // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() { },
        function error(e) { });
    // gọi report html chọn mẫu
    //Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_selectreport_customernotices.html?type=Bulksendemail&data=" + result.list_id.join(","), { width: 410, height: 345 }, null, null);
    var nameReport = "Handover Notices Form";
    //window.open(urlreport, nameReport, "resizable=1,width=380,height=344");
}
function VisBtn_BulkSendMail() {
    return true;
}