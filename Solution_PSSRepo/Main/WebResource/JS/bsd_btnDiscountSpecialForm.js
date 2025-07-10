// JavaScript source code - btnAdvancePaymentForm
$ = window.parent.$;

/*
+ status : 	Acitve = 1 	   
		    
*/

//----------------------------------- Approve ---------------------------------------
function disS_btnApprove() {
    debugger;
    var id = Xrm.Page.data.entity.getId();
    var quoteid = Xrm.Page.getAttribute("bsd_quote").getValue();
    Xrm.Utility.showProgressIndicator("Approve Discount Special. Please Wait!...");
    ExecuteAction(
    id, "bsd_discountspecial", "bsd_Action_Approve_Special_Discount", null //[{ name: 'type', type: 'string', value: "sign" }]
    , function (result) {
        Xrm.Utility.closeProgressIndicator();
        if (result != null) {
            if (result.status == "error") {

                var ss = result.data.split(':');
                var mss = ss[ss.length - 1];
                var confirmStrings = {
                    text: mss,
                    title: "Message"
                };
                Xrm.Navigation.openAlertDialog(confirmStrings);

            }
            else {

                GenPS(quoteid[0].id);

                //                                Xrm.Page.data.refresh();
                //                                var confirmStrings = {
                //                                  text: mss,
                //                                  title: "Message"
                //                                    };
                //                                Xrm.Navigation.openAlertDialog(confirmStrings);
            }
        }
    });

    //    Xrm.Page.getAttribute("statuscode").setValue(100000000);
    //    Xrm.Page.getAttribute("bsd_approvaldate").setValue(new Date());
    //                var user = new Array();
    //                user[0] = new Object();
    //                user[0].id = Xrm.Page.context.getUserId();
    //                user[0].entityType = 'systemuser';
    //                user[0].name = Xrm.Page.context.getUserName();
    //                Xrm.Page.getAttribute("bsd_approverrejecter").setValue(user);
    Xrm.Page.data.entity.save();
}

function btnVis_Approve() {

    var role = crmcontrol.checkRoles("CLVN_S&M_Head of Sale");
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1&&role) return true;
    return false;
}
function GenPS(id) {
    Xrm.Utility.showProgressIndicator("Gen PS. Please Wait!...");
    ExecuteAction(
    id, "quote", "bsd_Action_Resv_Gene_PMS", null //[{ name: 'type', type: 'string', value: "sign" }]
    , function (result) {
        Xrm.Utility.closeProgressIndicator();
        if (result != null) {
            if (result.status == "error") {

                var ss = result.data.split(':');
                var mss = ss[ss.length - 1];
                var confirmStrings = {
                    text: mss,
                    title: "Notices"
                };
                Xrm.Navigation.openAlertDialog(confirmStrings);

            }
            else {
                var confirmStrings = {
                    text: "Successfully approved special discounts and completed generate again Payment Scheme.",
                    title: "Notices"
                };
                Xrm.Navigation.openAlertDialog(confirmStrings);
                Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId());

            }
        }
    });
}

//-------------------------------------- RUN REPORT ---------------------------------------
function getReportURL(action, fileName, idreport, idrecord, recordstype) {
    debugger;
    //var orgUrl = GetGlobalContext().getClientUrl();
    var orgUrl = Xrm.Page.context.getClientUrl();
    var reportUrl = orgUrl + "/crmreports/viewer/viewer.aspx?action=" + encodeURIComponent(action) + "&context=records" + "&helpID=" + encodeURIComponent(fileName) + "&id=%7b" + encodeURIComponent(idreport) + "%7d" + "&records=%7b" + encodeURIComponent(idrecord) + "%7d&recordstype=" + recordstype + "";
    return reportUrl;
}

//-----------------------------------RUN WORKFLOW---------------------------------------------------
function RunWorkflow(workflowId, entityId, callback) {
    debugger;
    // var _return = window.confirm('Are you want to execute workflow.');
    //if (_return) {
    var url = window.location.protocol + '//' + window.location.host; //+ '/' + Xrm.Page.context.getOrgUniqueName();//Xrm.Page.context.getServerUrl();
    // var entityId = Xrm.Page.data.entity.getId();
    //var workflowId = 'CFA66414-AA64-4831-B151-4357FB750F0B';
    var OrgServicePath = "/XRMServices/2011/Organization.svc/web";
    url = url + OrgServicePath;
    var request;
    request = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" + "<s:Body>" + "<Execute xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" + "<request i:type=\"b:ExecuteWorkflowRequest\" xmlns:a=\"http://schemas.microsoft.com/xrm/2011/Contracts\" xmlns:b=\"http://schemas.microsoft.com/crm/2011/Contracts\">" + "<a:Parameters xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">" + "<a:KeyValuePairOfstringanyType>" + "<c:key>EntityId</c:key>" + "<c:value i:type=\"d:guid\" xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + entityId + "</c:value>" + "</a:KeyValuePairOfstringanyType>" + "<a:KeyValuePairOfstringanyType>" + "<c:key>WorkflowId</c:key>" + "<c:value i:type=\"d:guid\" xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + workflowId + "</c:value>" + "</a:KeyValuePairOfstringanyType>" + "</a:Parameters>" + "<a:RequestId i:nil=\"true\" />" + "<a:RequestName>ExecuteWorkflow</a:RequestName>" + "</request>" + "</Execute>" + "</s:Body>" + "</s:Envelope>";

    var req = new XMLHttpRequest();
    req.open("POST", url, true);
    // Responses will return XML. It isn't possible to return JSON.
    req.setRequestHeader("Accept", "application/xml, text/xml, */*");
    req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
    req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
    req.onreadystatechange = function () {
        if (callback != null) callback(req);
    };
    req.send(request);
}

function vis_BtnNew(selectedControl) {
    var entityname = Xrm.Page.data.entity.getEntityName();
    if (entityname == "quote") {
        var isExist = Xrm.Page.getAttribute("bsd_existinstallment").getValue();
        return !isExist;
    }
    if (entityname == "salesorder") {
        return false;
    }
    return true;
}

function vis_BtnActive(selectedControl) {
    var entityname = Xrm.Page.data.entity.getEntityName();
    if (entityname == "quote") {
        var isExist = Xrm.Page.getAttribute("bsd_existinstallment").getValue();
        return !isExist;
    }
    if (entityname == "salesorder") {
        return false;
    }
    return true;
}

function vis_BtnDeActive(selectedControl) {
    var entityname = Xrm.Page.data.entity.getEntityName();
    if (entityname == "quote") {
        var isExist = Xrm.Page.getAttribute("bsd_existinstallment").getValue();
        return !isExist;
    }
    if (entityname == "salesorder") {
        return false;
    }
    return true;
}

function vis_BtnDelete(selectedControl) {
    var entityname = Xrm.Page.data.entity.getEntityName();
    if (entityname == "quote") {
        var isExist = Xrm.Page.getAttribute("bsd_existinstallment").getValue();
        return !isExist;
    }
    if (entityname == "salesorder") {
        return false;
    }
    return true;
}