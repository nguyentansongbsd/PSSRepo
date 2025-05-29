if (window.$ == null) window.$ = window.parent.$;
function GenerateEnableRule() {
    return true;
}
window.top.entityTypeCode = 0;
window.top.entityname = "";
window.top.projectCode = "";
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
function btnBulkCrateEmail_WordTemplate(item) {
    debugger;
    var fetchXml =
        "<fetch top='1'>" +
        "  <entity name='" + item[0].TypeName + "'>" +
        "    <attribute name='bsd_name'/>" +
        "    <attribute name='bsd_project'/>" +
        "    <filter>" +
        "      <condition attribute='" + item[0].TypeName + "id'  operator='eq' value='" + item[0].Id + "'/>" +
        "    </filter>" +
        "    <order attribute='createdon' descending='true'/>" +
        "    <link-entity name='bsd_project' from='bsd_projectid' to='bsd_project' alias='pro'>" +
        "      <attribute name='bsd_projectcode'/>" +
        "    </link-entity>" +
        "  </entity>" +
        "</fetch>";
    // Gọi API để thực hiện truy vấn FetchXML
    Xrm.WebApi.retrieveMultipleRecords(item[0].TypeName, "?fetchXml=" + fetchXml).then(
        function (results) {
            // Lấy lên danh sách id
            var result = {};
            result.entityname = "";
            result.typecode = "";
            result.list_id = [];
            window.top.projectCode = results.entities[0]["pro.bsd_projectcode"]
            for (var j = 0, len = item.length; j < len; j++) {
                if (result.entityname == "") {
                    var entityname = item[j].TypeName;
                    console.log(entityname);
                    result.entityname = entityname;
                    window.top.entityname = entityname
                    result.typecode = item[j].TypeCode;
                    window.top.entityTypeCode = item[j].TypeCode;
                }
                var id = item[j].Id;
                result.list_id.push(id);
            }
            // gọi report html chọn mẫu
            var pageInput = {
                pageType: "webresource",
                webresourceName: "bsd_bsd_bsd_Entityview_SelectReport_Email.html",
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
            // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_selectreport_customernotices.html?data=" + result.list_id.join(","), { width: 410, height: 345 }, null, null);
            var nameReport = "Option Entry Form";
            // window.open(urlreport, nameReport, "resizable=1,width=380,height=344");
        })

}