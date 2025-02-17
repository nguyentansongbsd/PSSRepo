if (window.$ == null)
    window.$ = window.parent.$;
function ONLOAD() {
    SHOW_HIDE_LOCK();
    SHOW_HIDE_NOTICES_TAB();
    SHOW_HIDE_MISCELLANEOUS_TAB();
    nextperiodtypechange();
}
function SHOW_HIDE_LOCK() {
    var formtype = Xrm.Page.ui.getFormType();
    var optionentry = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    var reservation = Xrm.Page.getAttribute("bsd_reservation").getValue();
    var quotation = Xrm.Page.getAttribute("bsd_quotation").getValue();
    if (formtype != 1 && (optionentry || reservation || quotation)) {
        var status = Xrm.Page.getAttribute("statuscode").getValue();
        if (status == 100000001 || status == 100000000)
            disableFormFields(true);
        var flag = CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Finance Manager"]);
        debugger;
        if (status == 100000000 && flag == true)//notpaid
        {
            Xrm.Page.getControl("bsd_duedate").setDisabled(false);
            Xrm.Page.getControl("bsd_amountofthisphase").setDisabled(false);
            Xrm.Page.getControl("bsd_balanceincludeinterest").setDisabled(false);
        }
    }

}
function DueDateOnChange() {
    Xrm.Page.ui.clearFormNotification('2');
    var ordernumber = Xrm.Page.getAttribute("bsd_ordernumber").getValue();
    if (ordernumber && ordernumber > 1) {
        var duedate = Xrm.Page.getAttribute("bsd_duedate").getValue();
        var optionentry = Xrm.Page.getAttribute("bsd_optionentry").getValue();
        var reservation = Xrm.Page.getAttribute("bsd_reservation").getValue();
        if (duedate && (optionentry || reservation)) {

            duedate.setHours(0); duedate.setMinutes(0); duedate.setSeconds(0); duedate.setMilliseconds(0);
            var xml = [];
            if (optionentry) {
                xml.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>");
                xml.push("<entity name='bsd_paymentschemedetail'>");
                xml.push("<attribute name='bsd_paymentschemedetailid' />");
                xml.push("<attribute name='bsd_duedate' />");
                xml.push("<filter type='and'>");
                xml.push("<condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + optionentry[0].id + "' />");
                xml.push("<condition attribute='bsd_ordernumber' operator='eq' value='" + (ordernumber - 1) + "' />");
                xml.push("<condition attribute='bsd_duedate' operator='not-null' />");
                xml.push("</filter>");
                xml.push("</entity>");
                xml.push("</fetch>");
            }
            else if (reservation) {
                xml.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>");
                xml.push("<entity name='bsd_paymentschemedetail'>");
                xml.push("<attribute name='bsd_paymentschemedetailid' />");
                xml.push("<attribute name='bsd_duedate' />");
                xml.push("<filter type='and'>");
                xml.push("<condition attribute='bsd_reservation' operator='eq' uitype='quote' value='" + reservation[0].id + "' />");
                xml.push("<condition attribute='bsd_ordernumber' operator='eq' value='" + (ordernumber - 1) + "' />");
                xml.push("<condition attribute='bsd_duedate' operator='not-null' />");
                xml.push("</filter>");
                xml.push("</entity>");
                xml.push("</fetch>");
            }

            CrmFetchKit.Fetch(xml.join(""), true).then(function (rs) {
                if (rs.length > 0) {
                    var duedate_truoc = rs[0].attributes.bsd_duedate.value;
                    duedate_truoc.setHours(0); duedate_truoc.setMinutes(0); duedate_truoc.setSeconds(0); duedate_truoc.setMilliseconds(0);
                    if (duedate < duedate_truoc)
                        Xrm.Page.ui.setFormNotification('You have set a due date that is less than the previous installment due date.', 'WARNING', '2');

                }
            }, function (er) {
                console.log(er.message)
            });
        }
    }
}
function NotMaster() {
    var quotation = Xrm.Page.getAttribute("bsd_quotation").getValue();
    var res = Xrm.Page.getAttribute("bsd_reservation").getValue();
    var option = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    if (quotation || res || option) {
        debugger;
        var allsection = Xrm.Page.ui.tabs.get("tab_general").getSections().getAll();
        allsection.forEach(function (arg) {
            var allcontrol = arg.getControls().getAll();
            allcontrol.forEach(function (ar) {
                ar.setDisabled("true");
                ar.getAttribute().setRequiredLevel('none');
            });
        });

        var fixeddate = Xrm.Page.getControl("bsd_fixeddate");
        fixeddate.setDisabled("true");
        fixeddate.getAttribute().setRequiredLevel('none');

        var allsection = Xrm.Page.ui.tabs.get("tab_summary").getSections().getAll();
        allsection.forEach(function (arg) {
            var allcontrol = arg.getControls().getAll();
            allcontrol.forEach(function (ar) {
                ar.setDisabled("true");
                ar.getAttribute().setRequiredLevel('none');
            });
        });

        var allsection = Xrm.Page.ui.tabs.get("tab_estimateHD").getSections().getAll();
        allsection.forEach(function (arg) {
            var allcontrol = arg.getControls().getAll();
            allcontrol.forEach(function (ar) {
                ar.setDisabled("true");
                ar.getAttribute().setRequiredLevel('none');

            });
        });
    }
}
function nextperiodtypechange() {
    var nextperidtype = Xrm.Page.getAttribute("bsd_nextperiodtype").getValue();
    if (nextperidtype ==1) {
        Xrm.Page.getControl("bsd_numberofnextmonth").setVisible(true);
        Xrm.Page.getControl("bsd_numberofnextdays").setVisible(false);
        Xrm.Page.getAttribute("bsd_numberofnextmonth").setRequiredLevel('required');
        Xrm.Page.getAttribute("bsd_numberofnextdays").setRequiredLevel('none');
    }
    else if (nextperidtype ==2) {
        Xrm.Page.getControl("bsd_numberofnextmonth").setVisible(false);
        Xrm.Page.getControl("bsd_numberofnextdays").setVisible(true);
        Xrm.Page.getAttribute("bsd_numberofnextdays").setRequiredLevel('required');
        Xrm.Page.getAttribute("bsd_numberofnextmonth").setRequiredLevel('none');
    }
}

function typepaymentchange() {
    var typepayment = Xrm.Page.getAttribute("bsd_typepayment").getValue();
    if (typepayment ==1) {
        Xrm.Page.getControl("bsd_number").setVisible(false);
        Xrm.Page.getAttribute("bsd_number").setRequiredLevel('none');
    }
    else if (typepayment ==2) {
        Xrm.Page.getControl("bsd_number").setVisible(true);
        Xrm.Page.getAttribute("bsd_number").setRequiredLevel('required');
    }
}

function DuedateCalculatingMethodOnChange(IsOnChange) {
    var dcmethod = Xrm.Page.getAttribute("bsd_duedatecalculatingmethod").getValue();
    var fixeddate = Xrm.Page.getControl("bsd_fixeddate");
    var typeofStartdate = Xrm.Page.getAttribute("bsd_typeofstartdate");
    var emaillocal = Xrm.Page.getControl("bsd_withindate");
    var emailforeiger = Xrm.Page.getControl("bsd_emailreminderforeigner");
    var nextperidtype = Xrm.Page.getAttribute("bsd_nextperiodtype");
    var NoNextMonth = Xrm.Page.getAttribute("bsd_numberofnextmonth");
    var NoNextDay = Xrm.Page.getAttribute("bsd_numberofnextdays");

    var res = Xrm.Page.getAttribute("bsd_reservation").getValue();
    var option = Xrm.Page.getAttribute("bsd_optionentry").getValue();


    Xrm.Page.ui.tabs.get("tab_estimateHD").setVisible(dcmethod == 100000002);

    if (dcmethod == 100000002 && !res && !option) {
        var paymentScheme = Xrm.Page.getAttribute("bsd_paymentscheme").getValue();
        var maintenanceF = Xrm.Page.getAttribute("bsd_maintenancefees");
        var managementF = Xrm.Page.getAttribute("bsd_managementfee");
        if (paymentScheme) {
            debugger;
            var xml = [];
            xml.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true'>");
            xml.push("<entity name='bsd_project'>");
            xml.push("<attribute name='bsd_projectid' />");
            xml.push("<attribute name='bsd_managementfeepercent' />");
            xml.push("<attribute name='bsd_maintenancefeespercent' />");
            xml.push("<attribute name='bsd_managementamount' />");
            xml.push("<link-entity name='bsd_paymentscheme' from='bsd_project' to='bsd_projectid' alias='ab'>");
            xml.push("<filter type='and'>");
            xml.push("<condition attribute='bsd_paymentschemeid' operator='eq' uitype='bsd_paymentscheme' value='" + paymentScheme[0].id + "' />");
            xml.push("</filter>");
            xml.push("</link-entity>");
            xml.push("</entity>");
            xml.push("</fetch>");
            CrmFetchKit.Fetch(xml.join(""), true).then(function (rs) {
                if (rs.length > 0) {
                    if (rs[0].attributes.bsd_managementfeepercent && rs[0].attributes.bsd_managementfeepercent.value > 0)
                        managementF.setValue(1);
                    else managementF.setValue(0);
                    if (rs[0].attributes.bsd_maintenancefeespercent && rs[0].attributes.bsd_maintenancefeespercent.value > 0)
                        maintenanceF.setValue(1);
                    else maintenanceF.setValue(0);
                    if (rs[0].attributes.bsd_managementamount && rs[0].attributes.bsd_managementamount.value > 0)
                        managementF.setValue(1);
                }
            }, function (er) {
                console.log(er.message)
            });
        }
    }
    var allsection = Xrm.Page.ui.tabs.get("tab_estimateHD").getSections().getAll();
    allsection.forEach(function (arg) {
        var allcontrol = arg.getControls().getAll();
        allcontrol.forEach(function (ar) {
            if (dcmethod == 100000002) {
                ar.getAttribute().setRequiredLevel("required");
                var nameat = ar.getName();
                if ((nameat == "bsd_maintenancefees" && ar.getAttribute().getValue() == null) || (nameat == "bsd_managementfee" && ar.getAttribute().getValue() == null)) {

                    ar.getAttribute().setValue(0);
                }
            }
            else {
                ar.getAttribute().setRequiredLevel("none");
                if (ar.getAttribute().getValue() && ar.getAttribute().getValue() != false)
                    ar.getAttribute().setValue(null);
            }

        });
    });

    if (IsOnChange && IsOnChange == 7) {
        typeofStartdate.setValue(false);
        fixeddate.getAttribute().setValue(null);
        nextperidtype.setValue(null);
        NoNextMonth.setValue(null);
        NoNextDay.setValue(null);
        emaillocal.getAttribute().setValue(null);
        emailforeiger.getAttribute().setValue(null);
    }

    fixeddate.setVisible(dcmethod == 100000000);
    fixeddate.getAttribute().setRequiredLevel(fixeddate.getVisible() ? 'required' : 'none');

    Xrm.Page.ui.tabs.get("tab_summary").setVisible(dcmethod == 100000001);
    nextperidtype.setRequiredLevel(dcmethod == 100000001 ? 'required' : 'none');
    NoNextMonth.setRequiredLevel(NoNextMonth.getValue() == 1 ? 'required' : 'none');
    NoNextDay.setRequiredLevel(NoNextMonth.getValue() == 2 ? 'required' : 'none');
    

    emaillocal.setVisible(typeofStartdate.getValue() == true)
    emailforeiger.setVisible(typeofStartdate.getValue() == true);
    emaillocal.getAttribute().setRequiredLevel(emaillocal.getVisible() ? 'required' : 'none');
    emailforeiger.getAttribute().setRequiredLevel(emailforeiger.getVisible() ? 'required' : 'none');
    NotMaster();
}
function TyepOfStartDate(IsOnChange) {
    var typeofStartdate = Xrm.Page.getAttribute("bsd_typeofstartdate");
    var emaillocal = Xrm.Page.getControl("bsd_withindate");
    var emailforeiger = Xrm.Page.getControl("bsd_emailreminderforeigner");
    emaillocal.setVisible(typeofStartdate.getValue() == 1);
    emailforeiger.setVisible(typeofStartdate.getValue() == 1);
    emaillocal.getAttribute().setRequiredLevel(emaillocal.getVisible() ? 'required' : 'none');
    emailforeiger.getAttribute().setRequiredLevel(emailforeiger.getVisible() ? 'required' : 'none');
    if (IsOnChange && IsOnChange == 5) {
        emaillocal.getAttribute().setValue(null);
        emailforeiger.getAttribute().setValue(null);
    }
    NotMaster();
}
function ShowHide_tab_PaymentOptionEntry() {
    var res = Xrm.Page.getAttribute("bsd_reservation").getValue();
    var option = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    Xrm.Page.ui.tabs.get("tab_3").setVisible(res != null || option != null)
}
function setLastInstallment() {
    debugger;
    var lastInstallment = Xrm.Page.getAttribute("bsd_lastinstallment").getValue();
    if (lastInstallment != null) {
        var dcmethod = Xrm.Page.getControl("bsd_duedatecalculatingmethod");
        var fixeddate = Xrm.Page.getControl("bsd_fixeddate");

        dcmethod.setVisible(lastInstallment != 1);
        dcmethod.getAttribute().setRequiredLevel(dcmethod.getVisible() ? 'required' : 'none');

        if (lastInstallment == 1) {
            fixeddate.setVisible(false);
            fixeddate.getAttribute().setRequiredLevel(fixeddate.getVisible() ? 'required' : 'none');
            dcmethod.getAttribute().setValue(null);
            fixeddate.getAttribute().setValue(null);
            Xrm.Page.ui.tabs.get("tab_summary").setVisible(lastInstallment != 1);
            var allsection = Xrm.Page.ui.tabs.get("tab_summary").getSections().getAll();
            allsection.forEach(function (arg) {
                var allcontrol = arg.getControls().getAll();
                allcontrol.forEach(function (ar) {
                    if (ar.getAttribute().getRequiredLevel() == "required")
                        ar.getAttribute().setRequiredLevel("none");
                    if (ar.getAttribute().getValue() && ar.getAttribute().getValue() != false)
                        ar.getAttribute().setValue(null);
                });
            });
            Xrm.Page.ui.tabs.get("tab_estimateHD").setVisible(lastInstallment != 1);
            var allsection = Xrm.Page.ui.tabs.get("tab_estimateHD").getSections().getAll();
            allsection.forEach(function (arg) {
                var allcontrol = arg.getControls().getAll();
                allcontrol.forEach(function (ar) {
                    if (ar.getAttribute().getRequiredLevel() == "required")
                        ar.getAttribute().setRequiredLevel("none");
                    if (ar.getAttribute().getValue() && ar.getAttribute().getValue() != false)
                        ar.getAttribute().setValue(null);
                });
            });
        }
    }
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
function CheckRole(arrRoleId, arrRoleName) {
    debugger;
    var rs = false;
    if (arrRoleId == null || arrRoleId.length <= 0 || arrRoleName == null || arrRoleName.length <= 0) {
        console.log("arrRoleId is required a array with length >= 1 || arrRoleName is required a array with length >= 1");
    }
    else {
        var serverUrl = Xrm.Page.context.getClientUrl();
        var odataSelect = serverUrl + "/XRMServices/2011/OrganizationData.svc" + "/" + "RoleSet?$select=Name";
        var filter = "&$filter=";
        arrRoleId.forEach(function (r) {
            filter += "RoleId eq guid'" + r + "' or ";
        });
        $.ajax(
            {
                type: "GET",
                async: false,
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: odataSelect + filter.substr(0, filter.length - 4),
                beforeSend: function (XMLHttpRequest) { XMLHttpRequest.setRequestHeader("Accept", "application/json"); },
                success: function (data, textStatus, XmlHttpRequest) {
                    var len = data.d.results.length;
                    for (var i = 0; i < len; i++) {
                        var tmp = data.d.results[i];
                        if (typeof (tmp["Name"]) != "undefined") {
                            if (arrRoleName.indexOf(tmp["Name"]) > -1) {
                                rs = true;
                                break;
                            }
                        }
                    }

                },
                error: function (XmlHttpRequest, textStatus, errorThrown) {
                    console.log('OData Select Failed: ' + textStatus + errorThrown + odataSelect);
                }
            });
    }
    return rs;
}

function SHOW_HIDE_NOTICES_TAB() {
    debugger;
    var optionEntry = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    if (optionEntry != null) {
        Xrm.Page.ui.tabs.get("tab_7").setVisible(true);
    } else {
        Xrm.Page.ui.tabs.get("tab_7").setVisible(false);
    }
}

function SHOW_HIDE_MISCELLANEOUS_TAB() {
    debugger;
    var optionEntry = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    if (optionEntry != null) {
        Xrm.Page.ui.tabs.get("tab_miscellaneouslist").setVisible(true);
    } else {
        Xrm.Page.ui.tabs.get("tab_miscellaneouslist").setVisible(false);
    }
}

//function updateCheckManagementFee() {
//    debugger;
//    var duedateCalculatingMethod = Xrm.Page.getAttribute("bsd_duedatecalculatingmethod").getValue();
//    // 100000002 = Estimate handover date
//    if (duedateCalculatingMethod != null && duedateCalculatingMethod == 100000002) {
//        var installmentID = Xrm.Page.data.entity.getId();

//        if (installmentID) {
//            var xml = [];
//            xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
//            xml.push("<entity name='bsd_project'>");
//            xml.push("<attribute name='bsd_projectid' />");
//            xml.push("<attribute name='bsd_name' />");
//            xml.push("<attribute name='bsd_managementamount' />");
//            xml.push("<link-entity name='bsd_paymentscheme' from='bsd_project' to='bsd_projectid' alias='ac'>");
//            xml.push("<link-entity name='bsd_paymentschemedetail' from='bsd_paymentscheme' to='bsd_paymentschemeid' alias='ad'>");
//            xml.push("<filter type='and'>");
//            xml.push("<condition attribute='bsd_paymentschemedetailid' operator='eq' value='" + installmentID + "' />");
//            xml.push("</filter>");
//            xml.push("</link-entity>");
//            xml.push("</link-entity>");
//            xml.push("</entity>");
//            xml.push("</fetch>");
//            CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
//                if (rs.length > 0) {
//                    var managementAmount = rs[0].attributes.bsd_managementamount.value;
//                    if (managementAmount != null && managementAmount != 0) {
//                        Xrm.Page.getAttribute("bsd_managementfee").setValue(1);
//                    }
//                }
//            },
//                   function (er) {
//                       console.log(er.message)
//                   });
//        }
//    } 

    
//}