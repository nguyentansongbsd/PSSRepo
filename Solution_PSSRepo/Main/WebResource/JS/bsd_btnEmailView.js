if (window.$ == null) window.$ = window.parent.$;
var confirmOptions = { height: 200, width: 450 };
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
function fetchWithTimeout(url, options, timeout = 1800000) {
    return Promise.race([
        fetch(url, options),
        new Promise((_, reject) =>
            setTimeout(() => reject(new Error('Request timed out')), timeout)
        )
    ]);
}
//function UpdateStatus_SendMail(item) {
//    var listid = "";

//    countListid = listid.split(',').length;
//    var fetchXml =
//        "<fetch>" +
//        "  <entity name='queueitem'>" +
//        "    <filter>" +
//        "      <condition attribute='queueitemid' operator='in'>";
//    for (var j = 0, len = item.length; j < len; j++) {
//        if (j < (len - 1))
//            listid += item[j].Id + ",";
//        else listid += item[j].Id;
//        fetchXml += "        <value>" + item[j].Id + "</value>";
//    }
//    fetchXml +=
//        "      </condition>" +
//        "    </filter>" +
//        "    <link-entity name='email' from='activityid' to='objectid' alias='email'>" +
//        "      <attribute name='activityid' alias='id'/>" +
//        "      <attribute name='bsd_entityname'/>" +
//        "    </link-entity>" +
//        "  </entity>" +
//        "</fetch>";
//    Xrm.WebApi.retrieveMultipleRecords("queueitem", "?fetchXml=" + fetchXml).then(function (result) {
//        var filenameroot = "";
//        var count = 1;
//        result.entities.forEach(function (email) {
//            Xrm.Utility.showProgressIndicator("sending " + count + "/" + countListid);
//            if (email["email.bsd_entityname"]) {
//                switch (email["email.bsd_entityname"]) {
//                    case "bsd_payment":
//                        filenameroot = "ConfirmPayment";
//                        break;
//                    default:
//                        break;
//                }
//                if (filenameroot != "") {
//                    var data = {
//                        listId: listid,
//                        filennameroot: filenameroot
//                    };
//                    var url = "https://prod-49.southeastasia.logic.azure.com:443/workflows/320497af4dc74d849cea21649907c4c6/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=WsFNxxj9AyfRyyj9fPGSbCoB5_jNHxwaQx6QbiQySzc"
//                    fetchWithTimeout(url, {
//                        method: "POST",
//                        headers: {
//                            "OData-MaxVersion": "4.0",
//                            "OData-Version": "4.0",
//                            "Accept": "application/json",
//                            "Content-Type": "application/json; charset=utf-8"
//                        },
//                        body: JSON.stringify(data)
//                    }).then(response => {
//                        debugger;
//                        if (response.status == 200) {
//                            Xrm.Utility.closeProgressIndicator()
//                        }
//                    }).then(data => {
//                        Xrm.Utility.closeProgressIndicator();
//                        count++;
//                    }).
//                        catch(error => {
//                            debugger;
//                            Xrm.Utility.closeProgressIndicator()
//                            console.log(error); count++;

//                        });
//                }
//            }
//        });
//    })

//}
function UpdateStatus_SendMail() {
    var fetchXml =
        "<fetch>" +
        "  <entity name='email'>" +
        "    <attribute name='bsd_entityname'/>" +
        "    <attribute name='activityid'/>" +
        "    <filter type='and'>" +
        "      <condition attribute='bsd_bulksendmailmanager' operator='eq' value='" + Xrm.Page.data.entity.getId() + "'/>"+
        "      <condition attribute='statuscode' operator='eq' value='" + 1 + "'/>";

    fetchXml +=
        "    </filter>" +
        "  </entity>" +
        "</fetch>";
    Xrm.WebApi.retrieveMultipleRecords("email", "?fetchXml=" + fetchXml).then(function (result) {
        var filenameroot = "";
        var count = 1;
        //dev
        //var url = "https://1d554a713bc5455b887939f0df1c97.5c.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/b8ff54d276174d0c9772129127c67e7a/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=mn-44FazmqYOejyIg5CCe4hWqpadHnJRASJXkTJ4K2g"
        var url = "https://e32d24dee3864a64a2abda70077dc2.0c.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/320497af4dc74d849cea21649907c4c6/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=jlru0baGdwnZ-4F4-VNfbALyhJuXNdIsQzuFson1aKU"
        result.entities.forEach(function (email) {
            Xrm.Utility.showProgressIndicator("sending " + count + "/" + result.entities.length);
            if (email.bsd_entityname) {
                switch (email.bsd_entityname) {
                    case "bsd_payment":
                        filenameroot = "ConfirmPayment";
                        break;
                    case "bsd_customernotices":
                        filenameroot = "NoticePayment";
                        break;
                }
                if (filenameroot != "") {
                    var data = {
                        listId: email.activityid,
                        filennameroot: filenameroot
                    };
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
                            Xrm.Utility.closeProgressIndicator()
                        }
                    }).then(data => {
                        Xrm.Utility.closeProgressIndicator();
                        count++;
                    }).
                        catch(error => {
                            debugger;
                            Xrm.Utility.closeProgressIndicator()
                            console.log(error); count++;
                        });
                }
            }
        });
    })
}