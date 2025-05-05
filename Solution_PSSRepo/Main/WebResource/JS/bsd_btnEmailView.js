if (window.$ == null) window.$ = window.parent.$;
function GenerateEnableRule() {
    return true;
}
window.top.entityTypeCode = 0;
window.top.entityname = "";
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
function UpdateStatus_SendMail(item) {
    var listid = "";
    for (var j = 0, len = item.length; j < len; j++) {
        if (j < (len - 1))
            result.list_id += j;
        else result.list_id += j + ",";

    }
    countListid = list_id.split(',').length;
    var data = {
        listId: lstid,
    }; var fetchData = {
        "activityid": list_id.split(',')[0];
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='email'>",
        "    <attribute name='bsd_entityname'/>",
        "    <filter>",
        "      <condition attribute='activityid' operator='eq' value='", fetchData.activityid/*ec1d2309-fc1f-f011-9989-0022485654ef*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>"
    ].join("");
    Xrm.WebApi.retrieveMultipleRecords("bsd_documents", "?fetchXml=" + fetchXml).then(function (resutl) {
        var filenameroot = "";
        result.entities.forEach(function (email) {
            window.parent.processingDlg.show();
            if (email.bsd_entityname) {
                switch (email.bsd_entityname) {
                    case "bsd_payment":
                        filenameroot = "ConfirmPayment";
                        break;
                    default:
                        break;
                }
                if (filenameroot != "") {
                    var data = {
                        listId: lstid,
                        filennameroot: filenameroot
                    };
                    var url = "https://prod-49.southeastasia.logic.azure.com:443/workflows/320497af4dc74d849cea21649907c4c6/triggers/manual/paths/invoke?api-version=2016-06-01"
                    fetchWithTimeout(url, {
                        method: "POST",
                        headers: {
                            "OData-MaxVersion": "4.0",
                            "OData-Version": "4.0",
                            "Accept": "application/json",
                            "Content-Type": "application/json; charset=utf-8"
                        },
                        body: JSON.stringify(data)
                    }).then(response => {
                        debugger;
                        if (response.status == 200) {
                            window.parent.processingDlg.hide();

                        }
                    }).then(data => {

                        var confirmStrings = {
                            text: "Sending" + (countListid - data) + "/error" + data,
                            title: "Notice"
                        };
                        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                            function (success) {
                            }
                        );
                    }).
                        catch(error => {
                            debugger;
                            Xrm.Utility.closeProgressIndicator();
                            console.log(error);
                        });
                }
            }
        });
    })
    
}