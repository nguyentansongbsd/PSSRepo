// JavaScript source code
$ = window.parent.$;
/*
+ status : 	Not Launch  = 1 	    
			Launched    = 100000000 	/ 	Recovery = 100000001 
*/
//-------------------------------------- btnGenerate---------------------------------------

function btnGenerate() {
	crmcontrol.save();
	debugger;
	window.top.$ui.Confirm("Confirms", "Are you sure to generate?", function (e) {
		window.top.processingDlg.show();
		debugger;
		var id = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
		ExecuteAction("", "", "bsd_Action_PhasesLaunch_Generate",
			[{ name: 'input01', type: 'string', value: "Bước 01" },
			{ name: 'input02', type: 'string', value: id }],
			function (result) {
				debugger;
				if (result != null) {
					if (result.status == "error") {
						window.top.processingDlg.hide();
						window.top.$ui.Dialog("Message", result.data);
					}
					else {
						callFlow3(result.data.output01.value, id, result.data.output02.value, result.data.output03.value, result.data.output04.value);
						window.top.processingDlg.hide();
						setTimeout(Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId()), 2000);
					}
				}
			}, true);
	}, null);
}
function callFlow3(Output01, id, Output02, flowURL, Output03) {
	let body = {
		"id": id,
		"userID": Output01,
		"listBlock": Output02,
		"listFloor": Output03,
	};
	let req = new XMLHttpRequest();
	req.open("POST", flowURL, true);
	req.setRequestHeader("Content-Type", "application/json");
	req.send(JSON.stringify(body));
}
function btnVis_btnGenerate() 
	{
		var role = crmcontrol.checkRoles("CLVN_S&M_Sales Manager");
		if (Xrm.Page.getAttribute("statuscode").getValue() == 1 && Xrm.Page.getAttribute("bsd_powerautomate").getValue() == true) {
			return false;
		}
		if (role && Xrm.Page.getAttribute("statuscode").getValue() == 1 && Xrm.Page.ui.getFormType() != 1) {
			return true;
		}
		return false;
	}

//-------------------------------------- Release ---------------------------------------

function btnRelease() {
	crmcontrol.save();
	debugger;
	var hadHandoverCondition = checkHandoverCondition();
	if (hadHandoverCondition == true) {
		var confirmStrings = {
			text: "Phase launch does not include handover condition. Please check the infomation again.",
			title: "Message"
		};
		Xrm.Navigation.openAlertDialog(confirmStrings);
	} else {
		window.top.$ui.Confirm("Confirms", "Are you sure to Release this Phases Launch?", function (e) {
			window.top.processingDlg.show();
			//debugger;
			ExecuteAction(
				Xrm.Page.data.entity.getId(), "bsd_phaseslaunch", "bsd_Action_PhasesLaunch_Release",
				[{ name: 'Input01', type: 'string', value: "Bước 01" }],
				function (result) {
					//debugger;
					if (result != null) {
						if (result.status == "error") {
							window.top.processingDlg.hide();
							window.top.$ui.Dialog("Message", result.data);
						}
						else {
							disableFormFields(true);
							callFlow(result.data.Output01.value, result.data.Output02.value, result.data.Output03.value);
							window.top.processingDlg.hide();
							setTimeout(Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId()), 2000);
						}
					}
				});
		}, null);
	}
}
function callFlow(Output01, Output02, flowURL) {
	let body = {
		"id": Xrm.Page.data.entity.getId().replace("{", "").replace("}", ""),
		"Output01": Output01,
		"Output02": Output02
	};
	let req = new XMLHttpRequest();
	req.open("POST", flowURL, true);
	req.setRequestHeader("Content-Type", "application/json");
	req.send(JSON.stringify(body));
}
function btnVis_Release() {
	var role = crmcontrol.checkRoles("CLVN_S&M_Sales Manager") || crmcontrol.checkRoles("CLVN_S&M_Head of Sale") ? true : false;
	if (Xrm.Page.getAttribute("statuscode").getValue() == 1 && Xrm.Page.getAttribute("bsd_powerautomate").getValue() == true) {
		return false;
	}
	if (role && Xrm.Page.getAttribute("statuscode").getValue() == 1 && Xrm.Page.ui.getFormType() != 1) {
		return true;
	}
	return false;
}
//-------------------------------------- Recovery ---------------------------------------

function btnRecovery() {
	crmcontrol.save();
	debugger;
	window.top.$ui.Confirm("Confirms", "Are you sure to Recovery this Phases Launch?", function (e) {
		window.top.processingDlg.show();
		debugger;
		var id = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
		ExecuteAction("", "", "bsd_Action_PhasesLaunch_Recoverybba903b95ad2ef118ee900224857be30",
			[{ name: 'input01', type: 'string', value: "Bước 01" },
			{ name: 'input02', type: 'string', value: id }],
			function (result) {
				debugger;
				if (result != null) {
					if (result.status == "error") {
						window.top.processingDlg.hide();
						window.top.$ui.Dialog("Message", result.data);
					}
					else {
						callFlow2(result.data.output01.value, id, result.data.output02.value);
						window.top.processingDlg.hide();
						setTimeout(Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId()), 2000);
					}
				}
			}, true);
	}, null);
}
function callFlow2(Output01, id, flowURL) {
	let body = {
		"id": id,
		"userID": Output01
	};
	let req = new XMLHttpRequest();
	req.open("POST", flowURL, true);
	req.setRequestHeader("Content-Type", "application/json");
	req.send(JSON.stringify(body));
}
function btnVis_Recovery() {
	var role = crmcontrol.checkRoles("CLVN_S&M_Sales Manager") || crmcontrol.checkRoles("CLVN_S&M_Head of Sale") ? true : false;

	if (role&&Xrm.Page.getAttribute("statuscode").getValue() == 100000000) {
		return true;
	}
	return false;
}
//-------------------------------------- UNIT RECOVERY ---------------------------------------

function btnUnitRecovery() {
	crmcontrol.save();
	debugger;
	window.top.$ui.Confirm("Confirms", "Are you sure to Recovery this Phases Launch?", function (e) {
		window.top.processingDlg.show();
		debugger;
		var id = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
		ExecuteAction(id, "bsd_phaseslaunch", "bsd_Action_PhasesLaunch_Recovery", [{ name: 'type', type: 'string', value: "unit recovery b0" }]
			, function (result) {
				if (result != null) {
					if (result.status == "error") {
						window.top.processingDlg.hide();
						window.top.$ui.Dialog("Message", result.data);
					}
					else {
						callFlow2(result.data.idUser0.value, id, result.data.url.value);
						window.top.processingDlg.hide();
						setTimeout(Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId()), 2000);
					}
				}
			});
	}, null);
}

function btnVis_UnitRecovery() {
	if (role &&Xrm.Page.getAttribute("statuscode").getValue() == 100000000) {
		return true;
	}
	return false;
}
//-------------------------------------- RUN REPORT ---------------------------------------

function getReportURL(action, fileName, idreport, idrecord, recordstype) {
	debugger;
	//var orgUrl = GetGlobalContext().getClientUrl();
	var orgUrl = Xrm.Page.context.getClientUrl();
	var reportUrl = orgUrl +
		"/crmreports/viewer/viewer.aspx?action=" + encodeURIComponent(action) + "&context=records" +
		"&helpID=" + encodeURIComponent(fileName) +
		"&id=%7b" + encodeURIComponent(idreport) +
		"%7d" +
		"&records=%7b" + encodeURIComponent(idrecord) +
		"%7d&recordstype=" + recordstype + "";
	return reportUrl;
}
//-----------------------------------RUN WORKFLOW---------------------------------------------------

function RunWorkflow(workflowId, entityId, callback) {
	debugger;
	var url = window.location.protocol + '//' + window.location.host; //+ '/' + Xrm.Page.context.getOrgUniqueName();//Xrm.Page.context.getServerUrl();
	var OrgServicePath = "/XRMServices/2011/Organization.svc/web";
	url = url + OrgServicePath;
	var request;
	request = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
		"<s:Body>" +
		"<Execute xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">" +
		"<request i:type=\"b:ExecuteWorkflowRequest\" xmlns:a=\"http://schemas.microsoft.com/xrm/2011/Contracts\" xmlns:b=\"http://schemas.microsoft.com/crm/2011/Contracts\">" +
		"<a:Parameters xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">" +
		"<a:KeyValuePairOfstringanyType>" +
		"<c:key>EntityId</c:key>" +
		"<c:value i:type=\"d:guid\" xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + entityId + "</c:value>" +
		"</a:KeyValuePairOfstringanyType>" +
		"<a:KeyValuePairOfstringanyType>" +
		"<c:key>WorkflowId</c:key>" +
		"<c:value i:type=\"d:guid\" xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + workflowId + "</c:value>" +
		"</a:KeyValuePairOfstringanyType>" +
		"</a:Parameters>" +
		"<a:RequestId i:nil=\"true\" />" +
		"<a:RequestName>ExecuteWorkflow</a:RequestName>" +
		"</request>" +
		"</Execute>" +
		"</s:Body>" +
		"</s:Envelope>";
	var req = new XMLHttpRequest();
	req.open("POST", url, true)
	// Responses will return XML. It isn't possible to return JSON.
	req.setRequestHeader("Accept", "application/xml, text/xml, */*");
	req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
	req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
	req.onreadystatechange = function () {
		if (callback != null) callback(req);
	};
	req.send(request);
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
function checkHandoverCondition() {
	var phasesId = crmcontrol.getId();
	var fetchXml = [
		"<fetch>",
		"  <entity name='bsd_packageselling'>",
		"    <attribute name='bsd_name'/>",
		"    <filter>",
		"      <condition attribute='statecode' operator='eq' value='0'/>",
		"    </filter>",
		"    <link-entity name='bsd_bsd_phaseslaunch_bsd_packageselling' from='bsd_packagesellingid' to='bsd_packagesellingid' alias='phaselaunch' intersect='true'>",
		"      <filter>",
		"        <condition attribute='bsd_phaseslaunchid' operator='eq' value='", phasesId, "'/>",
		"      </filter>",
		"    </link-entity>",
		"  </entity>",
		"</fetch>"
	].join("");
	var rs = CrmFetchKitNew.FetchSync(fetchXml);
	if (rs.length <= 0) return true;
	else return false;
}