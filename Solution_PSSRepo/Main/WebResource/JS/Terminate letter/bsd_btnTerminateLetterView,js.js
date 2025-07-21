// JavaScript source code
//## new script bsd_btnTerminateLetter Jack 2016 .06.29
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

// # Jack - 16.06.29 - bsd_btn(  get from Ribbon btn ) to call function bsd_Action_TerminateLetter_GenerateTerminateLetter
//function btnView_Generate() {
//    debugger;
//    try {
//        window.top.$ui.Confirm("Confirm", "Do you want to genarate Terminate letter?", function (e) {
//            window.parent.processingDlg.show();
//            window.top.ExecuteAction("", "", "bsd_Action_TerminateLetter_GenerateTerminateLetter", null //[{ name: 'ReturnId', type: 'string', value: null }]
//            , function (result) {
//                window.parent.processingDlg.hide();
//                if (result != null && result.status != null) {
//                    if (result.status == "error") window.top.$ui.Dialog("Error", result.data);
//                    else if (result.status == "success") {
//                        //window.parent.Xrm.Utility.openEntityForm("bsd_warningnotices", result.data.ReturnId.value, null, { openInNewWindow: true });
//                        window.top.$ui.Dialog("Message", "The process of initiating the Terminate Letter is currently in progress!");
//                    }
//                    else {
//                        console.log(JSON.stringify(result));
//                    }
//                }
//            },
//            true);
//        },
//        null);

//    }
//    catch(e) {
//        window.top.$ui.Dialog("Error", e.message, null);
//    }
//}
window.top.generateTerminateletter2 = function (project, block, floor, units,_date) {
    try {
        debugger;
        if (project == null) {
            window.top.$ui.Dialog("Message", "Please select Project!");
            return;
        }
        window.top.$ui.Confirm("Confirms", "Do you want to genarate Terminate letter?", function (e) {
            debugger;
            window.parent.processingDlg.show();

            window.top.ExecuteAction(
                ""
                , ""
                , "bsd_Action_TerminateLetter_GenerateTerminateLetter"
                , [
                    { name: 'Project', type: 'string', value: project != null ? project : "" },
                    { name: 'Block', type: 'string', value: block != null ? block : "" },
                    { name: 'Floor', type: 'string', value: floor != null ? floor : "" },
                    { name: 'Units', type: 'string', value: units != null ? units : "" },
                    { name: '_date', type: 'string', value: _date != null ? _date.toString() : "" },
                  ]
                , function (result) {
                    window.parent.processingDlg.hide();
                    if (result != null && result.status != null) {
                        if (result.status == "error")
                            window.top.$ui.Dialog("Message", result.data);
                        else if (result.status == "success") {

                            if (result && result.data && result.data.idmaster && result.data.idmaster.value) {
                                var entityId = result.data.idmaster.value;
                                var entityName = "bsd_genterminationletter";
                                // Open the entity form in a new tab
                                Xrm.Utility.openEntityForm(entityName, entityId, null, { openInNewWindow: true });
                            } else {
                                window.top.$ui.Dialog("Error", "Cannot open Termination Letter form. Invalid result data.", null);
                            }
                        }
                        else {
                            console.log(JSON.stringify(result));
                        }
                    }
                }, true);
        }, null);
    }
    catch (e) {
        window.top.$ui.Dialog("Error", e.message, null);
    }
}
function btnView_Generate() {
    debugger;

    var entityFormOptions = {};
    entityFormOptions["entityName"] = "bsd_genterminationletter";
    entityFormOptions["use"] = true; 
    // Mở form
    Xrm.Navigation.openForm(entityFormOptions).then(
        function (lookup) {
            console.log("Form bsd_generateterminationletter đã được mở thành công.");
            // lookup.savedEntityReference trả về thông tin của bản ghi được tạo (nếu có)
        },
        function (error) {
            console.log(error.message);
        }
    );
    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_WarningNotice_Form.html", { width: 360,height: 270 }, null, null);

}
function openOptionFilter() {
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_terminateletter_option_filter_generate.html",
        data: null
    };
    var navigationOptions = {
        target: 2, // 2 is for opening the page as a dialog.
        width: 520, // default is px. can be specified in % as well.
        height: 400, // default is px. can be specified in % as well.
        position: 1 // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() {
        },
        function error(e) {
        }
    );
}
var intervalId = null;
var intervalIds = [];
var isAction = 1;
function btn_GenerateEnableRule(type) {
    if (type != 1) {
        if (intervalId == null && isAction == 1) {
            isAction = 0;
            intervalId = setInterval(function () {
                var check = btn_GenerateEnableRule();
            }, 1000)
        }
        else {
            if (intervalId != null && isAction != 1)
                clearInterval(intervalId);
        }
    }
        
    
    
    var fetchData = {
        "bsd_name": "GENTerminationLetter",
        "statuscode": "100000000"
    };
    var fetchXml = [
        "<fetch top='50'>",
        "  <entity name='bsd_process'>",
        "    <filter>",
        "      <condition attribute='bsd_name' operator='eq' value='", fetchData.bsd_name/*GENTerminationLetter*/, "'/>",
        "      <condition attribute='statuscode' operator='eq' value='", fetchData.statuscode/*100000000*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>"
    ].join("");
    window.top.CrmFetchKit.Fetch(fetchXml, false).then(function (rs) {
        if (rs.length > 0) {
            console.log("processing")

            if (isShowNoti == 0) {
                window.top.$ui.Confirm("Confirms", "The Terminate Letter creation process is running. Please wait, or press 'Cancel' to stop.", function (e) {
                    window.parent.processingDlg.show();
                    isShowNoti = 1;
                    clearInterval(intervalId);
                    intervalId = setInterval(function () {
                        var check = btn_GenerateEnableRule();
                    }, 5000)
                }, function () {
                    updateRecord();
                    return true;
                });
                return false;
            }
        }
        else {
            console.log("processing")
            clearInterval(intervalId);
            if (isShowNoti == 1) {
                window.parent.processingDlg.hide();
                
                window.top.$ui.Confirm("Confirms", "The process has been successfully completed.", function (e) {
                }, function () {
                });
            }
            return true;

        }
    }, function (er) {
        console.log(er.message)
        return true;
    });
}
function updateRecord() {
    var fetchData = {
        "bsd_name": "GENTerminationLetter",
        "statuscode": "100000000"
    };
    var fetchXml = [
        "<fetch top='50'>",
        "  <entity name='bsd_process'>",
        "    <filter>",
        "      <condition attribute='bsd_name' operator='eq' value='", fetchData.bsd_name/*GENTerminationLetter*/, "'/>",
        "      <condition attribute='statuscode' operator='eq' value='", fetchData.statuscode/*100000000*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>"
    ].join("");
    window.top.CrmFetchKit.Fetch(fetchXml, false).then(function (rs) {
        if (rs.length > 0) {
            var record = {
                "statuscode": 1 // Thay đổi giá trị trường bạn muốn cập nhật
            };

            Xrm.WebApi.updateRecord("bsd_process", rs[0].Id, record).then(
                function success(result) {
                    console.log("Record updated successfully. Record ID: " + result.id);
                    // Thực hiện các hành động khác nếu cần
                },
                function (error) {
                    console.log("Error updating record: " + error.message);
                    // Xử lý lỗi nếu cần
                }
            );
        }
    })

}
//function btn_GenerateEnableRule() {
//    ready();
//    checkProcessGenerate();
//    return true;
//}