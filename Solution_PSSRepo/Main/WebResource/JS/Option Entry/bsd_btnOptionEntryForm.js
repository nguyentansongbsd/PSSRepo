/// <reference path="JavaScript.js" />
// JavaScript source code
$ = window.parent.$;
/*
+ status : 	Open = 1 	                /   Cancel = 100000007
		    Deposited = 100000000 	    / 	Direct Buy = 100000008
		    On Hold = 100000001 	    / 	Signed Contract = 100000002
		    Being Payment = 100000003 	/ 	Complete Payment = 100000004
		    Handover = 100000005 	    / 	Termination = 100000006
*/
//--- Add Recalculator ---//
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
function ord_btnRecalculator() {
    var result = Xrm.Page.getAttribute("bsd_taxcode").getValue();
    if (result != null) {
        window.top.$ui.Confirm("Confirm", "This action will reload Tax", function () {
            //window.parent.processingDlg.show();
            RunWorkflow("813F76F5-5B87-48A5-AF95-11141AF09AB1", Xrm.Page.data.entity.getId(), function (req) {
                if (req.readyState == 4) {
                    //window.parent.processingDlg.hide();
                    if (req.status == 200) {
                        Xrm.Page.data.refresh();
                    }
                    else if (req.status == 500) {
                        if (req.responseXML != "") {
                            var mss = req.responseXML.getElementsByTagName("Message");
                            if (mss.length > 0) window.top.$ui.Dialog("Error", mss[0].firstChild.nodeValue, null);
                        }
                    }
                }
            });
        },
        null);
    }
    else {
        window.top.$ui.Dialog("Error", "Fill in Tax Code please !", null);
    }
}

function ordVis_btnRecalculator() {
    console.log("ordVis_btnRecalculator");
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1 && status != 100000006) return true;
    return false;
}
//--- Confirm Payment Scheme ---//

function ord_ConfirmPaymentScheme() {
    Xrm.Page.data.save().then(function () {
        window.top.$ui.Confirm("Confirm", "Would you like to generate payment scheme!", function () {
            window.top.processingDlg.show();
            RunWorkflow("70B4084B-8CE8-4776-95F9-A9073D3462EA", Xrm.Page.data.entity.getId(), function (req) {
                if (req.readyState == 4) {
                    window.top.processingDlg.hide();
                    if (req.status == 200) {
                        Xrm.Page.data.refresh();
                        window.top.$ui.Dialog("Message", "Generate Payment scheme successful!", null);
                    }
                    else if (req.status == 500) {
                        if (req.responseXML != "") {
                            var mss = req.responseXML.getElementsByTagName("Message");
                            if (mss.length > 0) window.top.$ui.Dialog("Error", mss[0].firstChild.nodeValue, null);
                        }
                    }
                }
            });
        },
        null);
    });
}

function ordVis_ConfirmPaymentScheme() {
    console.log(ordVis_ConfirmPaymentScheme);
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (status == 100000008 || status == 100000000 || status == 1) {
        return true;
    }
    return false;
}
//--- Sign Contract ---//

function ord_SignContract() {
    debugger;
    var signedcontractdate = Xrm.Page.getAttribute("bsd_signedcontractdate").getValue();
    if (signedcontractdate == null) {
        var pageInput = {
            pageType: "webresource",
            webresourceName: "bsd_select_date_signcontract.html",
            data: Xrm.Page.data.entity.getId()
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

        function success() {},

        function error(e) {});
        // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_select_date_signDA.html?id=" + Xrm.Page.data.entity.getId(), { width: 380, height: 344 }, null, null);
    }
    else {
        window.top.$ui.Confirm("Message", "OptionEntry is already Sign!");
    }
    //Xrm.Page.data.save().then(function () {
    //    window.top.$ui.Confirm("Confirm", "Would you like to sign this contract ?", function () {
    //        window.top.processingDlg.show();
    //        //RunWorkflow("F0340FB7-A80B-428A-A178-859BC40CFB1B", Xrm.Page.data.entity.getId(), function (req) {
    //        RunWorkflow("E7B5D133-B801-4BDD-B6DC-97660D82CB86", Xrm.Page.data.entity.getId(), function (req) {
    //        if (req.readyState == 4) {
    //
    //                window.top.processingDlg.hide();
    //                if (req.status == 200) {
    //                    ExecuteAction(
    //                     Xrm.Page.data.entity.getId()
    //                     , "salesorder"
    //                     , "bsd_Action_OESignContract_CreateFUL"
    //                     , null
    //                     , function (result) {
    //
    //                         if (result != null) {
    //                             window.top.processingDlg.hide();
    //                             if (result.status == "error") {
    //                                 var ss = result.data.split(':');
    //                                 var mss = ss[ss.length - 1];
    //                                 window.top.$ui.Dialog("Message", mss);
    //                             }
    //                             else {
    //                             }
    //                         }
    //                     });
    //                    Xrm.Page.data.refresh();
    //                    window.top.$ui.Confirm("Message", "Contract signed successfully!", function(){ window.top.location.reload(true)}, null);
    //                }
    //                else if (req.status == 500) {
    //                    if (req.responseXML != "") {
    //                        var mss = req.responseXML.getElementsByTagName("Message");
    //                        if (mss.length == 0) {
    //                            var xrq = req.responseXML.getElementsByTagName("faultstring");
    //                            if (xrq.length > 0)
    //                                xrq = xrq[0];
    //                            mss = xrq.innerHTML;
    //                        }
    //                        if (mss.length > 0)
    //                            window.top.$ui.Dialog("Error", mss[0].firstChild.nodeValue, null);
    //                    }
    //                }
    //            }
    //        });
    //    }, null);
    //});
}

function OESignContract_CreateFUL() {
    //    var parameters = {
    //        "Target": { id: crmcontrol.getId(), LogicalName: crmcontrol.getEntityName() }
    //    };
    //    Xrm.Utility.invokeProcessAction("bsd_Action_OESignContract_CreateFUL", parameters).then(function () {
    //
    //    });
    ExecuteAction(
    crmcontrol.getId(), crmcontrol.getEntityName(), "bsd_Action_OESignContract_CreateFUL", null,

    function (result) {
        window.top.processingDlg.hide();
        if (result != null) {
            if (result.status == "error") {
                var ss = result.data.split(':');
                var mss = ss[ss.length - 1];
                window.top.$ui.Dialog("Message", mss);
            }
            else if (result.status == "success") {
                //                            callback();
            }
        }
    });
    //    parameters = {
    //        "Target": { id: crmcontrol.getId(), LogicalName: crmcontrol.getEntityName() },
    //        "objtype": "optionentry",
    //        "contactid": "",
    //        "contacttype": ""
    //    };
    //    Xrm.Utility.invokeProcessAction("bsd_Action_OptionEntryPurchaserLoyaltyProgram", parameters).then(function () {
    //
    //    });
    ExecuteAction(
    crmcontrol.getId(), crmcontrol.getEntityName(), "bsd_Action_OptionEntryPurchaserLoyaltyProgram", null,

    function (result) {
        window.top.processingDlg.hide();
        if (result != null) {
            if (result.status == "error") {
                var ss = result.data.split(':');
                var mss = ss[ss.length - 1];
                window.top.$ui.Dialog("Message", mss);
            }
            else if (result.status == "success") {
                callback();
            }
        }
    });
}
window.top.applySign = function (ngay) {
    Xrm.Page.data.save().then(function () {
        var confirmStrings = {
            text: "Would you like to sign this contract ?",
            title : "Confirmation"
        };
        var confirmOptions = {
            height: 200,
            width: 450
        };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(

        function (success) {
            if (success.confirmed) {
                console.log("Dialog closed using OK button.");
                var date = new Date(ngay);
                Xrm.Page.getAttribute("bsd_signedcontractdate").setValue(date);
                Xrm.Page.data.save();
                Xrm.Utility.showProgressIndicator("Please wait");
                debugger;
                //                    var parameters = {
                //                        "Target": { id: crmcontrol.getId(), LogicalName: crmcontrol.getEntityName() }
                //                    };
                ExecuteAction(
                crmcontrol.getId(), crmcontrol.getEntityName(), "bsd_Action_SignedContract", null,

                function (result) {
                    Xrm.Utility.closeProgressIndicator();
                    if (result != null) {
                        if (result.status == "error") {
                            var ss = result.data.split(':');
                            var mss = ss[ss.length - 1];
                            window.top.$ui.Dialog("Message", mss);
                        }
                        else if (result.status == "success") {
                            callback();
                        }
                    }
                });
                //                    Xrm.Utility.invokeProcessAction("bsd_Action_SignedContract", parameters)
                //                        .then(function (result) {
                //
                //                            Xrm.Utility.closeProgressIndicator();
                //                            console.log(result);
                //                            var dataresult = result.get_outputParameters();
                //                            if (dataresult['output'] == 'done') {
                //                                OESignContract_CreateFUL();
                //
                //                                var alertStrings = { confirmButtonLabel: "Ok", text: "Contract signed successfully!" };
                //                                var alertOptions = { height: 147, width: 300 };
                //                                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                //                                    function success(resultDialog) {
                //
                //                                        console.log("Alert dialog closed");
                //                                        //crmcontrol.refresh();
                //                                        //window.location.reload();
                //                                        crmcontrol.refresh();
                //                                    },
                //                                    function (error) {
                //                                        console.log(error.message);
                //                                    }
                //                                );
                //                            }
                //
                //
                //                        }, function (error) {
                //
                //                            console.log(error);
                //                        });
            }
            else console.log("Dialog closed using Cancel button or X.");
        });
    });
    //Xrm.Page.data.save().then(function () {
    /*window.top.$ui.Confirm("Confirm", "Would you like to sign this contract ?", function () {
        //window.top.processingDlg.show();
        var date = new Date(ngay);
        Xrm.Page.getAttribute("bsd_signedcontractdate").setValue(date);
        Xrm.Page.data.save();
        Xrm.Utility.showProgressIndicator("Please wait");
        var parameters = {
            "Target": { id: crmcontrol.getId(), LogicalName: crmcontrol.getEntityName() }
        };
        Xrm.Utility.invokeProcessAction("bsd_Action_SignedContract", parameters)
            .then(function (result) {
                
                Xrm.Utility.closeProgressIndicator();
                console.log(result);
                var dataresult = result.get_outputParameters();
                if (dataresult['output'] == 'done') {

                }

                var alertStrings = { confirmButtonLabel: "Ok", text: dataresult['output'] };
                var alertOptions = { height: 147, width: 300 };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                    function success(resultDialog) {

                        console.log("Alert dialog closed");
                        crmcontrol.refresh();
                    },
                    function (error) {
                        console.log(error.message);
                    }
                );
            }, function (error) {

                console.log(error);
            });


        //RunWorkflow("F0340FB7-A80B-428A-A178-859BC40CFB1B", Xrm.Page.data.entity.getId(), function (req) {
        /*RunWorkflow("E7B5D133-B801-4BDD-B6DC-97660D82CB86", Xrm.Page.data.entity.getId(), function (req) {
            
            if (req.readyState == 4) {
                window.top.processingDlg.hide();
                if (req.status == 200) {
                    ExecuteAction(
                        Xrm.Page.data.entity.getId()
                        , "salesorder"
                        , "bsd_Action_OESignContract_CreateFUL"
                        , null
                        , function (result) {

                            if (result != null) {

                                window.top.processingDlg.hide();
                                if (result.status == "error") {
                                    var ss = result.data.split(':');
                                    var mss = ss[ss.length - 1];
                                    window.top.$ui.Dialog("Message", mss);
                                }
                                else {
                                }
                            }
                        });
                    //Hồ Code 20180619 Update PurchaserLoyaltyProgram bsd_membershiptier, bsd_totalamountofownership
                    ExecuteAction(
                        Xrm.Page.data.entity.getId()
                        , "salesorder"
                        , "bsd_Action_OptionEntryPurchaserLoyaltyProgram"
                        , [{ name: 'objtype', type: 'string', value: 'optionentry' },
                        { name: 'contactid', type: 'string', value: "" },
                        { name: 'contacttype', type: 'string', value: "" }]
                        , function (result) {

                        }
                    );
                    Xrm.Page.data.refresh();
                    window.top.$ui.Confirm("Message", "Contract signed successfully!", function () {
                        window.top.location.reload(true)
                    }, null);
                }
                else if (req.status == 500) {
                    if (req.responseXML != "") {
                        var mss = req.responseXML.getElementsByTagName("Message");
                        if (mss.length == 0) {
                            var xrq = req.responseXML.getElementsByTagName("faultstring");
                            if (xrq.length > 0)
                                xrq = xrq[0];
                            mss = xrq.innerHTML;
                        }
                        if (mss.length > 0)
                            window.top.$ui.Dialog("Error", mss[0].firstChild.nodeValue, null);
                    }
                }
            }
        });*/
    //}, null);
    //});
}

function ordVis_SignContract() {
    console.log("ordVis_SignContract");
    //Hô update 03-06-2019
    var checkshortfallamount = false;
    var statuscode = crmcontrol.getValue("statuscode");
    if (statuscode == 100000000) //Status Option Entry = Option Or 1st installment
    {
        var optionEntryId = crmcontrol.getId();
        var bsd_specialcontractprintingapproval = Xrm.Page.getAttribute("bsd_specialcontractprintingapproval") != null ? crmcontrol.getValue('bsd_specialcontractprintingapproval') : false;
        if (bsd_specialcontractprintingapproval == 1) //Special Contract Printing Approval = 1
        {
            console.log("Get Balance(Installment) trong Installment 1");
            var installment1 = optionEntryModel.getInstallmentByOrderNumber(optionEntryId, 1);
            console.log(installment1);
            var bsd_balance = installment1.attributes['bsd_balance'] != null ? installment1.attributes['bsd_balance'].value : 0;
            console.log("Get số tiền Shortfall trong Project");
            var enrefProject = crmcontrol.getValue('bsd_project');
            console.log(enrefProject);
            var projectId = enrefProject[0].id;
            enProject = projectModel.getItem(projectId);
            console.log(enProject);
            var bsd_shortfallamount = enProject.attributes['bsd_shortfallamount'] != null ? enProject.attributes['bsd_shortfallamount'].value : 0;
            if (bsd_balance <= bsd_shortfallamount) {
                checkshortfallamount = true;
            }
        }
    }
    var bsd_contractprinteddate = crmcontrol.getValue('bsd_contractprinteddate');
    var roleSeniorStaff = crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    var roleManager = crmcontrol.checkRoles("CLVN_CCR Manager");
    var checkpermission = (roleSeniorStaff || roleManager) && statuscode == 100000001;
    if ((checkshortfallamount || checkpermission) && bsd_contractprinteddate != null) {
        return true;
    }
    return false;
    //Hô update 03-06-2019
    //var status = Xrm.Page.getAttribute("statuscode").getValue(); //1st instsallment
    //var printcontract = Xrm.Page.getAttribute("bsd_contractprinteddate").getValue();
    ////Thạnh Đỗ Update Role chỉ được hiện khi
    //var flagFin = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Head of Section"])
    //    || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager"])
    //    || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Senior Staff"]));
    //if (flagFin == true && (status == 100000001 && printcontract != null))
    //    return true;
    //return false;
}

function updateProduct(myId) {
    window.top.processingDlg.show();
    var key = "{" + myId + "}";
    var tmp = {};
    tmp.StatusCode = {
        Value: "100000002"
    };
    SDK.REST.updateRecord(
    key, tmp, "Product",

    function (r) {
        window.top.processingDlg.hide();
        window.top.$ui.Dialog("Message", "Contract signed successfully!");
    },

    function (e) {
        window.top.window.top.processingDlg.hide();
        window.top.$ui.Dialog("Error", "Contract signed successfully!");
        console.log(e.message);
    });
}
//--- Print Contract ---//
//TriCM160817
// cap nhat ngay expired date trong bsd_optionentry_form ( function window.top.setdate())
//--- Print Deposit Agreement ---// 13122016

function btn_PrintDepositAgreement() {
    window.top.processingDlg.show();
    ExecuteAction(
    Xrm.Page.data.entity.getId(), "salesorder", "bsd_Action_OE_CalDAAmount", null //[{ name: 'ReturnId', type: 'string', value: null }]
    ,

    function (result) {
        window.top.processingDlg.hide();
        debugger;
        //
        //alert("a");
        if (result != null && result.status != null) {
            if (result.status == "error") {
                var ss = result.data.split(':');
                var mss = ss[ss.length - 1];
                window.top.$ui.Dialog("Message", mss);
            }
            else {
                var pageInput = {
                    pageType: "webresource",
                    webresourceName: "bsd_selectcontract.html",
                    data: '{ "id": "' + Xrm.Page.data.entity.getId() + '", "type": 2 }'
                };
                var navigationOptions = {
                    target: 2,
                    // 2 is for opening the page as a dialog.
                    width: 520,
                    // default is px. can be specified in % as well.
                    height: 400,
                    // default is px. can be specified in % as well.
                    position: 1,
                    // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
                    title: "Print D.A"
                };
                Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(

                function success() {},

                function error(e) {});
                //window.top.$ui.Dialog("Message", "Successfully!");
                //Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_selectcontract.html?id=" + Xrm.Page.data.entity.getId() + "&type=" + 2, { width: 380, height: 345 }, null, null);
            }
        }
    });
}

function btnVis_PrintDepositAgreement() {
    debugger;
    console.log("btnVis_PrintDepositAgreement");
    var status = Xrm.Page.getAttribute("statuscode").getValue(); // 1st installment
    var special = Xrm.Page.getAttribute("bsd_specialcontractprintingapproval").getValue();
    var roleSeniorStaff = crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    var roleManager = crmcontrol.checkRoles("CLVN_CCR Manager");
    var flagFin = (roleSeniorStaff || roleManager);
    var bsd_signedcontractdate = Xrm.Page.getAttribute("bsd_signedcontractdate").getValue();
    var id = Xrm.Page.data.entity.getId();
    //Thạnh Đỗ Update Role chỉ được hiện khi
    //var flagFin = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Head of Section"])
    //    || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager"])
    //    || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Senior Staff"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR_SubSale"]));
    var check = false;
    var hanmuc = 2000000;
    var fetchXml = ["<fetch>", "  <entity name='bsd_paymentschemedetail'>", "    <attribute name='bsd_balance' />", "    <filter type='and'>", "      <condition attribute='bsd_optionentry' operator='eq' value='", id, "'/>", "      <condition attribute='statecode' operator='eq' value='", 0, "'/>", "      <condition attribute='bsd_ordernumber' operator='eq' value='", 1, "'/>", "    </filter>", "  </entity>", "</fetch>", ].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) {
        if (rs[0].attributes.bsd_balance != null) {
            if (rs[0].attributes.bsd_balance.value <= hanmuc) {
                check = true;
            }
        }

    }
    //if (flagFin == true && (status == 100000001 || special == true) && bsd_signedcontractdate == null) return true;
    if (flagFin == true && (check || special == true) && bsd_signedcontractdate == null) return true;
    return false;
}
//--- Sign Deposit Agreement ---// 13122016

function btn_SignDepositAgreement() {
    var daSignedDate = Xrm.Page.getAttribute("bsd_signeddadate").getValue();
    var daPrintedDate = Xrm.Page.getAttribute("bsd_agreementdate").getValue();
    if (daSignedDate == null && daPrintedDate != null) {
        var pageInput = {
            pageType: "webresource",
            webresourceName: "bsd_select_date_signDA.html",
            data: Xrm.Page.data.entity.getId()
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

        function success() {},

        function error(e) {});
        // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_select_date_signDA.html?id=" + Xrm.Page.data.entity.getId(), { width: 380, height: 344 }, null, null);
    }
    //Xrm.Page.data.save().then(function () {
    //    var daSignedDate = Xrm.Page.getAttribute("bsd_signeddadate").getValue();
    //    if (daSignedDate == null) {
    //        var date = new Date();
    //        date = new Date(date.toDateString());
    //        Xrm.Page.getAttribute("bsd_signeddadate").setValue(date);
    //        Xrm.Page.getControl("bsd_agreementdate").setDisabled(true);
    //        Xrm.Page.getControl("bsd_signeddadate").setDisabled(true);
    //        Xrm.Page.data.save();
    //        window.top.$ui.Dialog("Message", "Successful.", null);
    //    } else {
    //        window.top.$ui.Dialog("Message", "Failed.", null);
    //    }
    //});
}
window.top.applySignda = function (ngay) {
    Xrm.Page.data.save().then(function () {
        var daSignedDate = Xrm.Page.getAttribute("bsd_signeddadate").getValue();
        if (daSignedDate == null) {
            var date = new Date(ngay);
            Xrm.Page.getAttribute("bsd_signeddadate").setValue(date);
            Xrm.Page.getControl("bsd_agreementdate").setDisabled(true);
            Xrm.Page.getControl("bsd_signeddadate").setDisabled(true);
            Xrm.Page.data.save();
            crmcontrol.alertdialogConfirm("Successful.")
            // window.top.$ui.Dialog("Message", "Successful.", null);
        }
        else {
            crmcontrol.alertdialogConfirm("Failed.")
            //window.top.$ui.Dialog("Message", "Failed.", null);
        }
    });
}

function btnVis_SignDepositAgreement() {
    console.log("btnVis_SignDepositAgreement");
    //Option 100000000
    /*(2) Cập nhật lại điều kiện để Hiện nút Sign DA / SPA:
    + Status Option Entry = Option
        + Special Contract Printing Approval
            + Balance(Installment) trong Installment 1 nhỏ hoặc bằng số tiền Shortfall trong Project*/
    //Hô update 03-06-2019
    var checkshortfallamount = false;
    var statuscode = crmcontrol.getValue("statuscode");
    if (statuscode == 100000000) //Status Option Entry = Option
    {
        var optionEntryId = crmcontrol.getId();
        var bsd_specialcontractprintingapproval = Xrm.Page.getAttribute("bsd_specialcontractprintingapproval") != null ? crmcontrol.getValue('bsd_specialcontractprintingapproval') : false;
        if (bsd_specialcontractprintingapproval == 1) //Special Contract Printing Approval = 1
        {
            console.log("Get Balance(Installment) trong Installment 1");
            var installment1 = optionEntryModel.getInstallmentByOrderNumber(optionEntryId, 1);
            console.log(installment1);
            var bsd_balance = installment1.attributes['bsd_balance'] != null ? installment1.attributes['bsd_balance'].value : 0;
            console.log("Get số tiền Shortfall trong Project");
            var enrefProject = crmcontrol.getValue('bsd_project');
            console.log(enrefProject);
            var projectId = enrefProject[0].id;
            enProject = projectModel.getItem(projectId);
            console.log(enProject);
            var bsd_shortfallamount = enProject.attributes['bsd_shortfallamount'] != null ? enProject.attributes['bsd_shortfallamount'].value : 0;
            if (bsd_balance <= bsd_shortfallamount) {
                checkshortfallamount = true;
            }
        }
    }
    var roleSeniorStaff = crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    var roleManager = crmcontrol.checkRoles("CLVN_CCR Manager");
    var flagFin = (roleSeniorStaff || roleManager);
    //var checkpermission = crmcontrol.checkRoles("CLVN_CCR Head of Section") || crmcontrol.checkRoles("CLVN_CCR Manager") || crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    //var checkpermission = flagFin && statuscode == 100000001; //1st installment
    var checkpermission = flagFin
    var daSignedDate = crmcontrol.getValue("bsd_signeddadate");
    var daPrintedDate = crmcontrol.getValue("bsd_agreementdate");
    if ((checkshortfallamount || checkpermission) && daSignedDate == null && daPrintedDate != null) {
        return true;
    }
    //Hô update 03-06-2019
    //var status = Xrm.Page.getAttribute("statuscode").getValue(); // 1st installment
    //var daSignedDate = Xrm.Page.getAttribute("bsd_signeddadate").getValue();
    //var daPrintedDate = Xrm.Page.getAttribute("bsd_agreementdate").getValue();
    ////Thạnh Đỗ Update Role chỉ được hiện khi
    //var flagFin = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Head of Section"])
    //    || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager"])
    //    || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Senior Staff"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR_SubSale"]));
    //if (flagFin == true && (daPrintedDate != null && daSignedDate == null && status == 100000001))
    //    return true;
    return false;
}
//--- Print Contract ---//

function ord_btnPrintContract() {
    //var t = new Date();
    //Xrm.Page.getAttribute("bsd_contractprinteddate").setValue(t);
    //var project = Xrm.Page.getAttribute("bsd_project").getValue();
    //var xml = [];
    //xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
    //xml.push("<entity name='bsd_project'>");
    //xml.push("<attribute name='bsd_expireddateofsigningrf' />");
    //xml.push("<attribute name='bsd_expireddateofsigningcontract' />");
    //xml.push("<attribute name='bsd_investmentcertificate' />");
    //xml.push("<attribute name='bsd_legaldateofcompletion' />");
    //xml.push("<filter type='and'>");
    //xml.push("<condition attribute='bsd_projectid' operator='eq' value='" + project[0].id + "' />");
    //xml.push("</filter>");
    //xml.push("</entity>");
    //xml.push("</fetch>");
    //CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
    //    if (rs.length > 0) {
    //        if (rs[0].attributes.bsd_expireddateofsigningcontract != null) {
    //            if (rs[0].attributes.bsd_investmentcertificate == null || rs[0].attributes.bsd_legaldateofcompletion == null) {
    //                var idrecord = Xrm.Page.data.entity.getId().replace('{', '').replace('}', '');
    //                var nameReport = "Deposit Agreement Form";
    //                var urlreport = getReportURL("run", "rpDepositAgreement.rdl", "A0A69030-0066-E611-80EC-3863BB36FFD0", idrecord, "1088");
    //                window.open(urlreport, nameReport, "resizable=1,width=800,height=700");
    //            }
    //            if (rs[0].attributes.bsd_investmentcertificate != null && rs[0].attributes.bsd_legaldateofcompletion != null) {
    //                var idrecord = Xrm.Page.data.entity.getId().replace('{', '').replace('}', '');
    //                var nameReport = "Contract Form";
    //var urlreport = getReportURL("run", "Contract.rdl", "6B25A247-471B-E611-80E3-3863BB352D28", idrecord, "1088");
    //                window.open(urlreport, nameReport, "resizable=1,width=800,height=700");
    //            }
    //            var tmp = rs[0].attributes.bsd_expireddateofsigningcontract.value;
    //            t.setDate(t.getDate() + tmp);
    //            Xrm.Page.getAttribute("bsd_signingexpired").setValue(t);
    //        };
    //    };
    //});
    //Xrm.Page.data.save();
    //dangpaedit 20200116
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_selectcontract.html",
        data: '{ "id": "' + Xrm.Page.data.entity.getId() + '", "type": 1 }',
    };
    var navigationOptions = {
        target: 2,
        // 2 is for opening the page as a dialog.
        width: 520,
        // default is px. can be specified in % as well.
        height: 400,
        // default is px. can be specified in % as well.
        position: 1,
        // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
        title: "Print contract"
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(

    function success() {},

    function error(e) {});
    //    if (Xrm.Page.data.entity.getIsDirty()) {
    //                        Xrm.Page.data.save().then(function () {
    //                            Xrm.Page.data.refresh();
    //                        }, null);
    //                    }
    //end dangpaedit 20200116
    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_selectcontract.html?id=" + Xrm.Page.data.entity.getId() + "&type=" + 1, { width: 410, height: 345 }, null, null);
}

function ordVis_PrintCotract() {
    debugger;
    console.log("ordVis_PrintCotract")
    var status = Xrm.Page.getAttribute("statuscode").getValue(); // 1st installment // signed contract // being payment // comolete payment
    //if (status == 100000002 || status == 100000003 || status == 100000004 || status == 100000005) {
    var special = Xrm.Page.getAttribute("bsd_specialcontractprintingapproval").getValue();
    var bsd_signedcontractdate = Xrm.Page.getAttribute("bsd_signedcontractdate").getValue();
    ////Thạnh Đỗ Update Role chỉ được hiện khi
    //var flagFin = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Head of Section"])
    //    || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager"])
    //    || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Senior Staff"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR_SubSale"]));
    var roleSeniorStaff = crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    var roleManager = crmcontrol.checkRoles("CLVN_CCR Manager");
    var flagFin = (roleSeniorStaff || roleManager);

    if (flagFin == true && (status == 100000001 || special == true) && bsd_signedcontractdate == null) return true;
    return false;

}
//--- Confirm Termination ---//

function ord_btnTermination() {
    //var optionID = Xrm.Page.data.entity.getId();
    //var optionName = Xrm.Page.getAttribute("name").getValue();
    //if (optionID != null && optionName) {
    //    var parameters = {};
    //    parameters["bsd_optionentry"] = optionID;
    //    parameters["bsd_optionentryname"] = optionName;
    //    Xrm.Utility.openEntityForm("bsd_termination", null, parameters, { openInNewWindow: true });
    //}
    var opID = Xrm.Page.data.entity.getId();
    var opName = Xrm.Page.getAttribute("name").getValue();
    var project = Xrm.Page.getAttribute("bsd_project").getValue();
    var parameters = {};
    if (project) {
        parameters["bsd_project"] = project[0].id;
        parameters["bsd_projectname"] = project[0].name;
    }
    //parameters["bsd_name"] = "FUL-" + ResName + "-" ;
    parameters["bsd_type"] = 100000006;
    parameters["bsd_optionentry"] = opID;
    parameters["bsd_optionentryname"] = opName;
    var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">' + '<entity name="product">' + '<attribute name="name" />' + '<attribute name="productid" />' + '<link-entity name="salesorderdetail" from="productid" to="productid" alias="ac">' + '<filter type="and">' + '<condition attribute="salesorderid" operator="eq" value="' + opID + '" />' + '</filter>' + '</link-entity>' + '</entity>' + '</fetch>';
    var rs = CrmFetchKitNew.FetchSync(fetchXML);
    //    CrmFetchKit.Fetch(fetchXML, false).then(
    //        function (rs) {
    if (rs.length > 0) {
        var tmp = rs[0].attributes;
        parameters["bsd_units"] = tmp.productid.value;
        parameters["bsd_unitsname"] = tmp.name.value;
    }
    Xrm.Utility.openEntityForm("bsd_followuplist", null, parameters, {
        openInNewWindow: true
    });
    //        }
    //        , function (err) {
    //            console.log(err);
    //        });
}

function btnVis_btnTermination() {
    console.log("btnVis_btnTermination");
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1 && status != 100000006 && status != 100000007) return true;
    return false;
}
//--- Sub_Sales---//

function CallActionSubSale() {
    var optionID = Xrm.Page.data.entity.getId();
    if (Xrm.Page.getAttribute("name").getValue() && Xrm.Page.getAttribute("customerid").getValue()) {
        var optionName = Xrm.Page.getAttribute("name").getValue();
        var parameters = {};
        var customer = Xrm.Page.getAttribute("customerid").getValue();
        if (customer) {
            parameters.customer_id = customer[0].id;
            parameters.customer_name = customer[0].name;
            parameters.customer_type = customer[0].entityType;
        }
        var units = Xrm.Page.getAttribute("bsd_unitnumber").getValue();
        if (units) {
            parameters["bsd_unit"] = units[0].id;
            parameters["bsd_unitname"] = units[0].name;
        }
        parameters["bsd_optionentry"] = optionID;
        parameters["bsd_optionentryname"] = optionName;
        var project = Xrm.Page.getAttribute("bsd_project").getValue();
        if (project) {
            parameters["bsd_project"] = project[0].id;
            parameters["bsd_projectname"] = project[0].name;
        }
        var date = new Date();
        date = CovertDateToString(date, "MM/dd/yyyy");
        parameters["bsd_assigndate"] = date;
        Xrm.Utility.openEntityForm("bsd_assign", null, parameters, {
            openInNewWindow: true
        });
    }
}

function btn_SubSale() {
    if (checkExistSubSale()) {
        window.top.$ui.Dialog("Message", "There is an active transfer request associated with this contract.");
    }
    else if (checkAdvancePayment()) {
        window.top.$ui.Dialog("Message", "The contract has advance payment in 'Active' or 'Pending Revert' status, so the transfer cannot be performed.");
    }
    else {
        var isCheckCongNo = checkCongNo();
        var isCheckBankLoan = checkBankLoan();
        if (isCheckCongNo && isCheckBankLoan) {
            window.top.$ui.Dialog("Message", "The contract has overdue payments, unpaid management or maintenance fees, outstanding interest that have not been settled, and the product is currently mortgaged and cannot be transferred.");
        } else if (isCheckBankLoan) {
            window.top.$ui.Dialog("Message", "This unit is in bank loan status, transaction cannot be proceeded.");
        } else if (isCheckCongNo) {
            window.top.$ui.Confirm('Confirm', "The contract has overdue payments, unpaid management or maintenance fees, and outstanding interest that have not been settled. Do you want to proceed?", function () { //OK
                open_SubSale();
            },
            function () { //Cancel
            });
        } else {
            open_SubSale();
        }
    }
}

function open_SubSale() {
    var optionID = Xrm.Page.data.entity.getId();
    if (optionID) {
        // var xml = [];
        // xml.push("<fetch version='1.0' output-format='xml-platform'  aggregate='true' mapping='logical' distinct='true'>");
        // xml.push("<entity name='product'>");
        // xml.push("<attribute name='name' alias='count' aggregate= 'countcolumn' />");
        // xml.push("<filter type='and'>");
        // xml.push("<condition attribute='bsd_bankloan' operator='eq' value='1' />");
        // //xml.push("<condition attribute='bsd_submitpinkbookdate' operator='not-null' />");
        // xml.push("</filter>");
        // xml.push("<link-entity name='salesorderdetail' from='productid' to='productid' alias='ak'>");
        // xml.push("<link-entity name='salesorder' from='salesorderid' to='salesorderid' alias='al'>");
        // xml.push("<filter type='and'>");
        // xml.push("<condition attribute='salesorderid' operator='eq' value='" + optionID + "' />");
        // xml.push("</filter>");
        // xml.push("</link-entity>");
        // xml.push("</link-entity>");
        // xml.push("</entity>");
        // xml.push("</fetch>");
        // var rs = CrmFetchKitNew.FetchSync(xml.join(""));
        // //        CrmFetchKit.Fetch(xml.join(""), true).then(function (rs) {
        // if (rs.length > 0 && rs[0].attributes.count.value == 0) {
        var xml1 = [];
        xml1.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='true'>");
        xml1.push("<entity name='product'>");
        xml1.push("<attribute name='bsd_submitpinkbookdate' />");
        xml1.push("<link-entity name='salesorderdetail' from='productid' to='productid' alias='ab'>");
        xml1.push("<filter type='and'>");
        xml1.push("<condition attribute='salesorderid' operator='eq'  uitype='salesorder' value='" + optionID + "' />");
        xml1.push("</filter>");
        xml1.push("</link-entity>");
        xml1.push("</entity>");
        xml1.push("</fetch>");
        var rs1 = CrmFetchKitNew.FetchSync(xml1.join(""));
        //                CrmFetchKit.Fetch(xml1.join(""), true).then(function (rs1) {
        if (rs1.length > 0 && rs1[0].attributes.bsd_submitpinkbookdate == null) {
            CallActionSubSale();
        }
        else window.top.$ui.Dialog("Warning", "Unit has submitted pink book. Can not Sub-sale.", null);
        //     //                },
        //     //                    function (er) {
        //     //                        console.log(er.message)
        //     //                    });
        // }
        // else window.top.$ui.Dialog("Warning", "This unit is in bank loan status, transaction cannot be proceeded.", null);
        // //        },
        // //            function (er) {
        // //                console.log(er.message)
        // //            });
    }
}

function btnVis_SubSale() {
    console.log("btnVis_SubSale");
    //var statuscode = Xrm.Page.getAttribute("bsd_unitstatus").getValue();
    //var statuscodes = Xrm.Page.getAttribute("statuscode").getValue();
    //if (statuscode == 100000006)
    //    return false;
    //var unitstatus = Xrm.Page.getAttribute("bsd_unitstatus").getValue();
    //if (unitstatus == 100000002) {
    //    return true;
    //} else {
    //    return false;
    //}
    //if (statuscodes == 100000007) {
    //    return false;
    //}
    if (crmcontrol.checkRoles("CLVN_CCR_SubSale")) {
        return true;
    }
    return false;
}

function CovertDateToString(date, format) {
    var today = "";
    var dd = date.getDate();
    var mm = date.getMonth() + 1; //January is 0!
    var yyyy = date.getFullYear();
    if (dd < 10) {
        dd = '0' + dd
    }
    if (mm < 10) {
        mm = '0' + mm
    }
    if (format == "dd/MM/yyyy") today = dd + '/' + mm + '/' + yyyy;
    else if (format == "MM/dd/yyyy") today = mm + '/' + dd + '/' + yyyy;
    return today;
}

function GetUserSetting() {
    var url = window.top.Xrm.Page.context.getClientUrl() + "/xrmservices/2011/OrganizationData.svc/UserSettingsSet?$select=TimeFormatCode,DateFormatString,TimeFormatString,TimeSeparator,TimeZoneCode&$filter=SystemUserId eq guid'" + window.top.Xrm.Page.context.getUserId() + "'";
    var format = "MM/dd/yyyy";
    $.ajax({
        type: "GET",
        async: false,
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: url,
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        success: function (data, textStatus, XmlHttpRequest) {
            var len = data.d.results.length;
            for (var i = 0; i < len; i++) {
                var ww = data.d.results;
                format = ww[0]["DateFormatString"];
            }
        },
        error: function (XmlHttpRequest, textStatus, errorThrown) {
            console.log('OData Select Failed: ' + textStatus + errorThrown);
        }
    });
    return format;
}
//--- Advance Payment ---//

function ord_btnAdvancePayment() {
    var optionID = Xrm.Page.data.entity.getId();
    if (Xrm.Page.getAttribute("name").getValue() && Xrm.Page.getAttribute("customerid").getValue()) {
        var optionName = Xrm.Page.getAttribute("name").getValue();
        var customerId = Xrm.Page.getAttribute("customerid").getValue()[0].id;
        var customerName = Xrm.Page.getAttribute("customerid").getValue()[0].name;
        var parameters = {};
        parameters["bsd_optionentry"] = optionID;
        parameters["bsd_optionentryname"] = optionName;
        var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" top="1" >' + '<entity name="salesorder">' + '<attribute name="ordernumber" />' + '<attribute name="customerid" />' + '<filter type="and" >' + '<condition attribute="salesorderid" operator="eq" value="' + optionID + '" />' + '</filter>' + '</entity>' + '</fetch>';
        var rs = CrmFetchKitNew.FetchSync(fetchXML);
        //        CrmFetchKit.Fetch(fetchXML, true).then(
        //            function (rs) {
        var tmp = rs[0].attributes;
        var ordernumber = tmp.ordernumber != null ? tmp.ordernumber.value : null;
        var customerType = tmp.customerid != null ? tmp.customerid.logicalName : null;
        var Number = 1;
        if (customerType == "contact") //Contact
        {
            parameters["bsd_contact"] = customerId;
            parameters["bsd_contactname"] = customerName;
            Number = 2;
        }
        else if (customerType == "account") {
            parameters["bsd_account"] = customerId;
            parameters["bsd_accountname"] = customerName;
        }
        parameters["bsd_customertype"] = Number;
        Xrm.Utility.openEntityForm("bsd_advancepayment", null, parameters, {
            openInNewWindow: true
        });
        //            }
        //            , function (err) {
        //                console.log(err);
        //            });
    }
}
//--- Apply Document ---//

function ord_btnApplyDocument() {
    var optionID = Xrm.Page.data.entity.getId();
    if (Xrm.Page.getAttribute("name").getValue() && Xrm.Page.getAttribute("customerid").getValue()) {
        var optionName = Xrm.Page.getAttribute("name").getValue();
        var customerId = Xrm.Page.getAttribute("customerid").getValue()[0].id;
        var customerName = Xrm.Page.getAttribute("customerid").getValue()[0].name;
        var parameters = {};
        //transaction
        parameters["bsd_transactiontype"] = 2;
        //option entry
        parameters["bsd_optionentry"] = optionID;
        parameters["bsd_optionentryname"] = optionName;
        var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" top="1" >' + '<entity name="salesorder">' + '<attribute name="ordernumber" />' + '<attribute name="customerid" />' + '<filter type="and" >' + '<condition attribute="salesorderid" operator="eq" value="' + optionID + '" />' + '</filter>' + '</entity>' + '</fetch>';
        var rs = CrmFetchKitNew.FetchSync(fetchXML);
        //        CrmFetchKit.Fetch(fetchXML, false).then(
        //            function (rs) {
        //
        var tmp = rs[0].attributes;
        var ordernumber = tmp.ordernumber != null ? tmp.ordernumber.value : null;
        var customerType = tmp.customerid != null ? tmp.customerid.logicalName : null;
        var current = new Date();
        parameters["bsd_name"] = ordernumber + "-AD" + current.format("ddMMyyyyhhmmss");
        var Number = 1;
        if (customerType == "contact") //Contact
        {
            parameters["bsd_contact"] = customerId;
            parameters["bsd_contactname"] = customerName;
            Number = 2;
        }
        else if (customerType == "account") {
            parameters["bsd_account"] = customerId;
            parameters["bsd_accountname"] = customerName;
        }
        parameters["bsd_customertype"] = Number;
        Xrm.Utility.openEntityForm("bsd_applydocument", null, parameters, {
            openInNewWindow: true
        });
        //            }
        //            , function (err) {
        //                console.log(err);
        //            });
    }
}

function ordVis_btnApplyDocument() {
    console.log("ordVis_btnApplyDocument");
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (status == 100000006) return false;
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1 && status != 100000006 && status != 100000007) return true;
    return false;
}
//--- Cancel Option Entry ---//

function ord_btnCancel() {
    window.top.$ui.Confirm("Confirm", "Are you sure you want to cancel this Option Entry ?", function () {
        var oe = {};
        oe.StatusCode = {
            Value: 100000007
        };
        SDK.REST.updateRecord(Xrm.Page.data.entity.getId(), oe, "SalesOrder", function (e) {
            Xrm.Page.data.refresh();
        },
        function (e) {
            console.log(e.message);
            window.top.$ui.Dialog("Message", e.message);
        });
        //Xrm.Page.data.entity.attributes.get("statuscode").setValue(100000007);
        //Xrm.Page.data.save();
    },
    null);
}

function ordVis_btnCancel() {
    console.log('ordVis_btnCancel');
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1 && status != 100000006) return true;
    return false;
}
//--- Delete Option Entry ---//

function ord_btnDelete() {
    window.top.$ui.Confirm("Confirm", "Are you sure you want to delete this Option Entry ?", function () {
        debugger;
        Xrm.Page.data.entity.attributes.get("statuscode").setValue(100000009);
        Xrm.Page.data.save().then(

        function () {
            var attr = Xrm.Page.getAttribute("statuscode").getValue();
            if (attr == 100000007) {
                Xrm.Page.ui.setFormNotification("This Option Entry has payment(s) which was paid. Do you want terminate ? <a href='#' onclick='createTerminate();'>&nbsp&nbsp<u>Yes</u>&nbsp&nbsp</a> <a href='#' onclick='clearNoti();'>&nbsp<u>No</u>&nbsp</a> ", "WARNING", "1")
            }
        },

        function () {});
    },
    null);
}
window.parent.createTerminate = function () {
    var Id = Xrm.Page.data.entity.getId();
    var Name = Xrm.Page.getAttribute("name").getValue();
    create(Id, Name, function () {
        alert("Passed");
    })
}

function create(recordid, name, callback) {
    var termination = {};
    termination.bsd_name = name + "-Terminated";
    termination.bsd_terminationdate = new Date();
    termination.bsd_optionentry = {
        Id: recordid,
        LogicalName: "salesorder",
        Name: name
    };
    SDK.REST.createRecord(
    termination, "bsd_termination",

    function (r) {},

    function (e) {
        debugger
    },

    function () {
        if (callback != null) callback();
    });
}
window.parent.clearNoti = function () {
    Xrm.Page.ui.clearFormNotification('1');
}
//--- Calculate Interest Charge ---//

function ord_btnCalculateInterestCharge() {
    //Xrm.Utility.openWebResource("bsd_Simulation", Xrm.Page.data.entity.getId(), 1000, 600);
    Xrm.Page.data.save().then(function () {
        var PS = Xrm.Page.getAttribute("bsd_paymentscheme").getValue();
        if (PS) {
            var xml = [];
            xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
            xml.push("<entity name='bsd_interestratemaster'>");
            xml.push("<attribute name='bsd_interestratemasterid' />");
            xml.push("<link-entity name='bsd_paymentscheme' from='bsd_interestratemaster' to='bsd_interestratemasterid' alias='ac'>");
            xml.push("<filter type='and'>");
            xml.push("<condition attribute='bsd_paymentschemeid' operator='eq' uitype='bsd_paymentscheme' value='" + PS[0].id + "' />");
            xml.push("</filter>");
            xml.push("</link-entity>");
            xml.push("</entity>");
            xml.push("</fetch>");
            var rs = CrmFetchKitNew.FetchSync(xml.join(""));
            //            CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            if (rs.length == 0) {
                window.top.$ui.Dialog("Message", "Please check field Interest Charge Master in Payment Scheme.");
            }
            else {
                var pageInput = {
                    pageType: "webresource",
                    webresourceName: "bsd_simulate/select_date_simulation_optionEntryForm.htm",
                    data: Xrm.Page.data.entity.getId()
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

                function success() {},

                function error(e) {});
                //Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_simulate/select_date.html?id=" + Xrm.Page.data.entity.getId(), { width: 380, height: 344 }, null, null);
            }
            //            }, function (err) {
            //                console.log(err.message);
            //            });
        }
    });
}

function ordVis_btnCalculateInterestCharge() {
    console.log('ordVis_btnCalculateInterestCharge');
    return true;
    //if (crmcontrol.checkRoles("CLVN_CCR_SubSale")) {
    //    return false;
    //}
    //var status = Xrm.Page.getAttribute("statuscode").getValue();
    //if (status == 100000006)
    //    return false;
    //var formtype = Xrm.Page.ui.getFormType();
    //if (formtype != 1 && status != 100000006)
    //    return true;
    //return false;
}
//--- Option Adjustment ---//

function btn_OptionAdjustment() {
    var parameters = {};
    parameters["bsd_optionentry"] = Xrm.Page.data.entity.getId();
    parameters["bsd_optionentryname"] = Xrm.Page.getAttribute("name").getValue();
    //parameters["name"] = "Adjusment";//+ OptionName;
    //parameters["bsd_date"] = new Date();
    Xrm.Utility.openEntityForm("bsd_optionadjustment", null, parameters, {
        openInNewWindow: true
    });
}
//--- Follow Up List ---//

function btn_FollowUpList() {
    var opID = Xrm.Page.data.entity.getId();
    var opName = Xrm.Page.getAttribute("name").getValue();
    var project = Xrm.Page.getAttribute("bsd_project").getValue();
    var parameters = {};
    if (project) {
        parameters["bsd_project"] = project[0].id;
        parameters["bsd_projectname"] = project[0].name;
    }
    var t = new Date();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (status == 100000000) //Option
    parameters["bsd_type"] = 100000002; //Option Entry - 1st installment
    else if (status == 100000001) //1st intallment
    parameters["bsd_type"] = 100000003; //Option Entry - Contract
    else if (status == 100000002) //Signed Contract
    parameters["bsd_type"] = 100000004; //Option Entry - Installments
    else //Con lại
    parameters["bsd_type"] = 100000006; //Option Entry - Terminate
    parameters["bsd_optionentry"] = opID;
    parameters["bsd_optionentryname"] = opName;
    var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">' + '<entity name="product">' + '<attribute name="name" />' + '<attribute name="productid" />' + '<link-entity name="salesorderdetail" from="productid" to="productid" alias="ac">' + '<filter type="and">' + '<condition attribute="salesorderid" operator="eq" value="' + opID + '" />' + '</filter>' + '</link-entity>' + '</entity>' + '</fetch>';
    var rs = CrmFetchKitNew.FetchSync(fetchXML);
    //    CrmFetchKit.Fetch(fetchXML, false).then(
    //        function (rs) {
    if (rs.length > 0) {
        var tmp = rs[0].attributes;
        parameters["bsd_units"] = tmp.productid.value;
        parameters["bsd_unitsname"] = tmp.name.value;
    }
    //        }
    //        , function (err) {
    //            console.log(err);
    //        });
    Xrm.Utility.openEntityForm("bsd_followuplist", null, parameters, {
        openInNewWindow: true
    });
}

function btnVis_FollowUpList() {
    console.log('btnVis_FollowUpList');
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1 && status != 100000006 && status != 100000007) return true;
    return false;
}
//--- Change Due Date Installments ---//

function btn_ChangeDueDateInst() {
    var opID = Xrm.Page.data.entity.getId();
    var opName = Xrm.Page.getAttribute("name").getValue();
    if (opID != null) {
        var parameters = {};
        parameters["bsd_optionentry"] = opID;
        if (opName != null) parameters["bsd_optionentryname"] = opName;
        var project = Xrm.Page.getAttribute("bsd_project").getValue();
        if (project) {
            parameters["bsd_project"] = project[0].id;
            parameters["bsd_projectname"] = project[0].name;
        }
        var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">' + '<entity name="product">' + '<attribute name="name" />' + '<attribute name="productid" />' + '<link-entity name="salesorderdetail" from="productid" to="productid" alias="ac">' + '<filter type="and">' + '<condition attribute="salesorderid" operator="eq" value="' + opID + '" />' + '</filter>' + '</link-entity>' + '</entity>' + '</fetch>';
        var rs = CrmFetchKitNew.FetchSync(fetchXML);
        //        CrmFetchKit.Fetch(fetchXML, false).then(
        //            function (rs) {
        if (rs.length > 0 && rs[0].attributes.productid && rs[0].attributes.name) {
            var tmp = rs[0].attributes;
            parameters["bsd_units"] = tmp.productid.value;
            parameters["bsd_unitsname"] = tmp.name.value;
        }
        //            }
        //            , function (err) {
        //                console.log(err);
        //            });
        Xrm.Utility.openEntityForm("bsd_approvechangeduedateinstallment", null, parameters, {
            openInNewWindow: true
        });
    }
}

function btnVis_ChangeDueDateInst() {
    console.log('btnVis_ChangeDueDateInst');
    var flag = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Financal Controller"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Finance Manager"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Chief Accountant"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Accountant"]));
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1 && status != 100000006 && status != 100000007 && flag) return true;
    return false;
}
//------ RUN REPORT ---//

function getReportURL(action, fileName, idreport, idrecord, recordstype) {
    //var orgUrl = GetGlobalContext().getClientUrl();
    var orgUrl = Xrm.Page.context.getClientUrl();
    var reportUrl = orgUrl + "/crmreports/viewer/viewer.aspx?action=" + encodeURIComponent(action) + "&context=records" + "&helpID=" + encodeURIComponent(fileName) + "&id=%7b" + encodeURIComponent(idreport) + "%7d" + "&records=%7b" + encodeURIComponent(idrecord) + "%7d&recordstype=" + recordstype + "";
    return reportUrl;
}
//---RUN WORKFLOW---//

function RunWorkflow(workflowId, entityId, callback) {
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
//--- Check User Role ---//

function CheckRole(arrRoleId, arrRoleName) {
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
        $.ajax({
            type: "GET",
            async: false,
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            url: odataSelect + filter.substr(0, filter.length - 4),
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            success: function (data, textStatus, XmlHttpRequest) {
                var len = data.d.results.length;
                for (var i = 0; i < len; i++) {
                    var tmp = data.d.results[i];
                    if (typeof(tmp["Name"]) != "undefined") {
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
//--- Bank Loan ---//

function btn_BankLoan() {
    var optionEntry = Xrm.Page.data.entity.getId();
    if (optionEntry) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_assign'>");
        xml.push("<attribute name='bsd_name' />");
        xml.push("<attribute name='bsd_optionentry' />");
        xml.push("<attribute name='bsd_assignid' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_optionentry' operator='eq' value='" + optionEntry + "' />");
        xml.push("<condition attribute='statuscode' operator='eq' value='1' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        var rs = CrmFetchKitNew.FetchSync(xml.join(""));
        //        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
        if (rs.length == 0) {
            var parameters = {};
            var units = Xrm.Page.getAttribute("bsd_unitnumber").getValue();
            var project = Xrm.Page.getAttribute("bsd_project").getValue();
            var optionEntryName = Xrm.Page.getAttribute("name").getValue();
            if (units) {
                parameters["bsd_unitsname"] = units[0].name;
                parameters["bsd_units"] = units[0].id;
            }
            if (project) {
                parameters["bsd_projectname"] = project[0].name;
                parameters["bsd_project"] = project[0].id;
            }
            if (optionEntryName) {
                parameters["bsd_optionentry"] = optionEntry;
                parameters["bsd_optionentryname"] = optionEntryName;
            }
            var customer = Xrm.Page.getAttribute("customerid").getValue();
            if (customer) {
                parameters.purchaser_id = customer[0].id;
                parameters.purchaser_name = customer[0].name;
                parameters.purchaser_type = customer[0].entityType;
            }
            Xrm.Utility.openEntityForm("bsd_bankingloan", null, parameters, {
                openInNewWindow: true
            }); //bsd_applydocument
        }
        else {
            crmcontrol.alertdialogConfirm("This Option Entry is in sub-sale processing, transaction cannot be proceeded!");
            // window.top.$ui.Dialog("Message", "This Option Entry is in sub-sale processing, transaction cannot be proceeded!", "tựa đề");
        }
        //        },
        //            function (er) {
        //                console.log(er.message)
        //            });
    }
}

function btnVis_BankLoan() {
    console.log('btnVis_BankLoan');
    var flag = (CheckRole(Xrm.Page.context.getUserRoles(), ["System Administrator"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Financal Controller"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Finance Manager"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Chief Accountant"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Accountant"]));
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var signeddate = Xrm.Page.getAttribute("bsd_signedcontractdate").getValue();
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1 && status != 100000006 && status != 100000007 && flag && signeddate) {
        return true;
    }
    return false;
}
//--- Special Contract Printing Approval ---//

function btn_SpecialSPAApproval() {
    window.top.$ui.Confirm("Confirm", "Are you sure you want to approve?", function () {
        Xrm.Page.data.save().then(

        function () {
            // Set Value
            Xrm.Page.getAttribute("bsd_specialcontractprintingapproval").setValue(true);
            Xrm.Page.getAttribute("bsd_approvaldateforspecialcontract").setValue(new Date());
            var user = new Array();
            user[0] = new Object();
            user[0].id = Xrm.Page.context.getUserId();
            user[0].entityType = 'systemuser';
            user[0].name = Xrm.Page.context.getUserName();
            Xrm.Page.getAttribute("bsd_approverforspecialcontract").setValue(user);
            // Block field
            Xrm.Page.getControl("bsd_specialcontractprintingapproval").setDisabled(true);
            Xrm.Page.getControl("bsd_approvaldateforspecialcontract").setDisabled(true);
            Xrm.Page.getControl("bsd_approverforspecialcontract").setDisabled(true);
            // Save data
            Xrm.Page.data.save();
        },

        function () {});
    },
    null);
}

function btnVis_SpecialSPAApproval() {
    console.log("btnVis_SpecialSPAApproval");
    var formtype = Xrm.Page.ui.getFormType();
    var special = Xrm.Page.getAttribute("bsd_specialcontractprintingapproval").getValue();
    var flag = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Head of Section"]));
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var shortFallAmount = getShortFallAmount();
    var installment1 = getInstall1();
    var checkInstallment = shortFallAmount != null && installment1 != null && installment1.bsd_balance.value <= shortFallAmount ? true : false;
    if (formtype != 1 && status == 100000000 && flag == true && special != 1 && checkInstallment == true) return true;
    return false;
}
//--- Han_25052018 > Print Actual Payment

function btn_PrintActualPayment() {
    var idrecord = Xrm.Page.data.entity.getId().replace('{', '').replace('}', '');
    var nameReport = "ActualInterestReport";
    // var urlreport = getReportURL("run", "All_ActualInterestReport_Up_Chua.rdl", "7AE7C020-6EA7-E811-8159-3863BB367D20", idrecord, window.top.Mscrm.EntityPropUtil.EntityTypeName2CodeMap[Xrm.Page.data.entity.getEntityName()]);
    var urlreport = Xrm.Page.context.getClientUrl() + "/Webresources/bsd_OptionEntry_ActualInterest_report.html?id=" + idrecord;
    window.open(urlreport, nameReport, "resizable=1,width=800,height=700");
}

function btnVis_PrintActualPayment() {
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue()
    //var flagAdmin = CheckRole(Xrm.Page.context.getUserRoles(), ["System Administrator"]);
    //var flagFin = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Financal Controller"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Finance Manager"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Chief Accountant"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Accountant"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN"]));
    //var roleSeniorStaff = crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    var roleManager = crmcontrol.checkRoles("CLVN_CCR Manager") || crmcontrol.checkRoles("CLVN_FIN_Chief Accountant") || crmcontrol.checkRoles("CLVN_FIN_Accountant") || crmcontrol.checkRoles("CLVN_FIN") || crmcontrol.checkRoles("CLVN_FIN_Financal Controller") || crmcontrol.checkRoles("CLVN_FIN_Finance Manager");
    var flagAdmin = crmcontrol.checkRoles("System Administrator");
    var flagFin = (roleManager || flagAdmin);
    if (formtype != 1 && status != 100000006 && flagFin == true) return true;
    return false;
}

function btnVis_PrintConfirmationLetter() {
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue()
    //var flagAdmin = CheckRole(Xrm.Page.context.getUserRoles(), ["System Administrator"]);
    //var flagFin = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Financal Controller"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Finance Manager"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Chief Accountant"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Accountant"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN"]));
    //var roleSeniorStaff = crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    var roleManager = crmcontrol.checkRoles("CLVN_CCR Manager") || crmcontrol.checkRoles("CLVN_FIN_Chief Accountant") || crmcontrol.checkRoles("CLVN_FIN_Accountant") || crmcontrol.checkRoles("CLVN_FIN") || crmcontrol.checkRoles("CLVN_FIN_Financal Controller") || crmcontrol.checkRoles("CLVN_FIN_Finance Manager");
    var flagAdmin = crmcontrol.checkRoles("System Administrator");
    var flagFin = (roleManager || flagAdmin);
    if (formtype != 1 && status != 100000006 && (flagFin == true)) return true;
    return false;
}
//--- Han_25052018 > Print Confirmation Letter ---//

function btn_PrintConfirmationLetter() {
    debugger;
    var optionentryid = Xrm.Page.data.entity.getId();
    //Total Paid (include COA)
    var depositamount = getInstallDepositAmount(optionentryid);
    var advancepayment = getAdvancePayment(optionentryid);
    //INSTALLMENT (Amount was paid)
    var installmentamountwaspaid = getInstallmentAmountwaspaid(optionentryid);
    var totalPaidincludCOA = advancepayment + installmentamountwaspaid + depositamount;
    //Total_SystemReceipt
    var totalSystemReceipt = getTotalSystemReceipt(optionentryid);
    //Total_Waiver Amount (Inst)
    var totalWaiverAmount = getTotalWaiverAmount(optionentryid);
    //Maintenance Fee
    var maintenanceFeePaid = getMaintenanceFeePaid(optionentryid);
    //Confirmation Amount
    var confirmationAmount = totalPaidincludCOA - totalSystemReceipt + maintenanceFeePaid;
    Xrm.Page.getAttribute("bsd_confirmationamount").setValue(confirmationAmount);
    Xrm.Page.data.save().then(

    function () {
        printConfirmationLetter()
    },

    function () {});
}

function printConfirmationLetter() {
    debugger;
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_confirmationletterreport.html",
        data: Xrm.Page.data.entity.getId(),
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

    function success() {},

    function error(e) {});
    //window.top.processingDlg.show();
    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_confirmationletterreport.html?id=" + Xrm.Page.data.entity.getId(), { width: 380, height: 350 }, null, null);
}

function getAdvancePayment(optionentryid) {
    var sum = 0;
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="bsd_advancepayment">', '    <attribute name="bsd_name" />', '    <attribute name="createdon" />', '    <attribute name="statuscode" />', '    <attribute name="bsd_amount" />', '    <attribute name="bsd_remainingamount" />', '    <attribute name="bsd_paidamount" />', '    <attribute name="bsd_project" />', '    <attribute name="bsd_customer" />', '    <attribute name="bsd_transferredamount" />', '    <attribute name="bsd_transfermoney" />', '    <attribute name="bsd_advancepaymentcode" />', '    <attribute name="bsd_transactiondate" />', '    <attribute name="bsd_optionentry" />', '    <attribute name="bsd_advancepaymentid" />', '    <order attribute="createdon" descending="true" />', '    <filter type="and">', '      <condition attribute="statuscode" operator="eq" value="100000000" />', '      <condition attribute="bsd_optionentry" operator="eq" uitype="salesorder" value="' + optionentryid + '" />', '    </filter>', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    if (rs.length > 0) {
        for (var i in rs) {
            sum += rs[i].attributes.bsd_remainingamount.value;
        }
    }
    return sum;
}

function getInstallmentAmountwaspaid(optionentryid) {
    var sum = 0;
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="bsd_paymentschemedetail">', '    <attribute name="bsd_paymentschemedetailid" />', '    <attribute name="bsd_name" />', '    <attribute name="bsd_amountwaspaid" />', '    <attribute name="bsd_waiveramount" />', '    <order attribute="bsd_name" descending="false" />', '    <filter type="and">', '      <condition attribute="bsd_optionentry" operator="eq" uitype="salesorder" value="' + optionentryid + '" />', '    </filter>', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    if (rs.length > 0) {
        for (var i in rs) {
            sum += rs[i].attributes.bsd_amountwaspaid.value;
        }
    }
    return sum;
}

function getInstallDepositAmount(optionentryid) {
    var sum = 0;
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="bsd_paymentschemedetail">', '    <attribute name="bsd_paymentschemedetailid" />', '    <attribute name="bsd_name" />', '    <attribute name="bsd_amountwaspaid" />', '    <attribute name="bsd_depositamount" />', '    <order attribute="bsd_name" descending="false" />', '    <filter type="and">', '      <condition attribute="bsd_optionentry" operator="eq" uitype="salesorder" value="' + optionentryid + '" />', '    </filter>', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    if (rs.length > 0) {
        for (var i in rs) {
            sum += rs[i].attributes.bsd_depositamount.value;
        }
    }
    return sum;
}

function getTotalSystemReceipt(optionentryid) {
    var sum = 0;
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="bsd_systemreceipt">', '    <attribute name="bsd_name" />', '    <attribute name="createdon" />', '    <attribute name="bsd_units" />', '    <attribute name="statuscode" />', '    <attribute name="bsd_receiptdate" />', '    <attribute name="bsd_purchaser" />', '    <attribute name="bsd_project" />', '    <attribute name="bsd_paymenttype" />', '    <attribute name="bsd_paymentnumbersams" />', '    <attribute name="bsd_optionentry" />', '    <attribute name="bsd_installmentnumber" />', '    <attribute name="bsd_installment" />', '    <attribute name="bsd_exchangerate" />', '    <attribute name="bsd_exchangemoney" />', '    <attribute name="bsd_amountpay" />', '    <attribute name="bsd_paymentnumber" />', '    <attribute name="bsd_systemreceiptid" />', '    <order attribute="createdon" descending="true" />', '    <filter type="and">', '      <condition attribute="statuscode" operator="eq" value="100000000" />', '      <condition attribute="bsd_optionentry" operator="eq" uitype="salesorder" value="' + optionentryid + '" />', '      <condition attribute="bsd_paymenttype" operator="eq" value="100000003" />', '    </filter>', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    if (rs.length > 0) {
        for (var i in rs) {
            sum += rs[i].attributes.bsd_amountpay.value;
        }
    }
    return sum;
}

function getTotalWaiverAmount(optionentryid) {
    var sum = 0;
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="bsd_paymentschemedetail">', '    <attribute name="bsd_paymentschemedetailid" />', '    <attribute name="bsd_name" />', '    <attribute name="bsd_amountwaspaid" />', '    <attribute name="bsd_waiverinstallment" />', '    <order attribute="bsd_name" descending="false" />', '    <filter type="and">', '      <condition attribute="bsd_optionentry" operator="eq" uitype="salesorder" value="' + optionentryid + '" />', '    </filter>', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    if (rs.length > 0) {
        for (var i in rs) {
            sum += rs[i].attributes.bsd_waiverinstallment.value;
        }
    }
    return sum;
}

function getMaintenanceFeePaid(optionentryid) {
    var sum = 0;
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="bsd_paymentschemedetail">', '    <attribute name="bsd_paymentschemedetailid" />', '    <attribute name="bsd_name" />', '    <attribute name="bsd_maintenancefeeremaining" />', '    <attribute name="bsd_maintenancefeepaid" />', '    <attribute name="bsd_maintenanceamount" />', '    <order attribute="bsd_name" descending="false" />', '    <filter type="and">', '      <condition attribute="bsd_optionentry" operator="eq" uitype="salesorder" value="' + optionentryid + '" />', '    </filter>', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    if (rs.length > 0) {
        for (var i in rs) {
            sum += rs[i].attributes.bsd_maintenancefeepaid != undefined ? rs[i].attributes.bsd_maintenancefeepaid.value : 0;
        }
    }
    return sum;
}
//--- Han_25052018 > Purchaser Loyalty Program ---//

function getListOptionEntryByContact(customerid) {
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="salesorder">', '    <attribute name="name" />', '    <attribute name="customerid" />', '    <attribute name="statuscode" />', '    <attribute name="totalamount" />', '    <attribute name="bsd_unitnumber" />', '    <attribute name="bsd_project" />', '    <attribute name="bsd_optionno" />', '    <attribute name="createdon" />', '    <attribute name="bsd_optioncodesams" />', '    <attribute name="bsd_contractnumber" />', '    <attribute name="salesorderid" />', '    <order attribute="createdon" descending="true" />', '    <filter type="and">', '      <condition attribute="customerid" operator="in">', '        <value uitype="account">' + customerid + '</value>', '        <value uitype="contact">' + customerid + '</value>', '      </condition>', '    </filter>', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    //    CrmFetchKit.Fetch(xml, false).then(function (rs) {
    if (rs.length > 0) {
        //Code Here
    }
    //    }, function (er) {
    //        console.log(er.message)
    //    });
}

function getListPurchaserLoyaltyProgram() {
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="bsd_purchaserloyaltyprogram">', '    <attribute name="bsd_purchaserloyaltyprogramid" />', '    <attribute name="bsd_name" />', '    <attribute name="createdon" />', '    <attribute name="bsd_method" />', '    <attribute name="bsd_membershiptier" />', '    <attribute name="bsd_endquantity" />', '    <attribute name="bsd_endamountcur" />', '    <attribute name="bsd_beginquantity" />', '    <attribute name="bsd_beginamountcur" />', '    <attribute name="bsd_approverejectperson" />', '    <attribute name="bsd_approverejectdate" />', '    <order attribute="bsd_name" descending="false" />', '    <filter type="and">', '      <condition attribute="statuscode" operator="eq" value="100000000" />', '    </filter>', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    //    CrmFetchKit.Fetch(xml, false).then(function (rs) {
    if (rs.length > 0) {
        //Code Here
    }
    //    }, function (er) {
    //        console.log(er.message)
    //    });
}
//--- Han_25052018 > Print SOA ---//
var glSOADate = "";
window.top.selectDateSOA = function (date) {
    glSOADate = date;
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_select_report_printsoa.html",
        data: Xrm.Page.data.entity.getId()
    };
    var navigationOptions = {
        target: 2,
        // 2 is for opening the page as a dialog.
        width: 620,
        // default is px. can be specified in % as well.
        height: 450,
        // default is px. can be specified in % as well.
        position: 1 // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(

    function success() {},

    function error(e) {});
    //Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_select_report_printsoa.html?id=" + Xrm.Page.data.entity.getId(), { width: 380, height: 344 }, null, null);
}
window.top.runReport = function (callback) {
    debugger;
    window.top.processingDlg.show();
    var optinentryid = Xrm.Page.data.entity.getId();
    var simulationdate = glSOADate._d.getFullYear() + "-" + glSOADate._d.getMonth() + "-" + glSOADate._d.getDate();
    var parameters = {
        "interestsimulationid": "",
        "callfrom": "OptionEntrySOA",
        "optinentryid": optinentryid,
        "aginginterestsimulationoptionid": '',
        "simulationdate": simulationdate,
        "dateofinterestcalculation": simulationdate
    };
    // Xrm.Utility.invokeProcessAction("bsd_Action_InterestSimulation_CalculateSimulation", parameters)
    //     .then(function (result) {
    //         window.top.processingDlg.hide();
    //         console.log(result);
    //         callback();
    //         // window.top.$ui.Dialog("Message", "Void Bulk waiver success!");
    //         // var alertStrings = { confirmButtonLabel: "Ok", text: "Void Bulk waiver success!" };
    //         // var alertOptions = { height: 147, width: 300 };
    //         Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
    //            function success(result) {
    //                console.log("Alert dialog closed");
    //            },
    //            function (error) {
    //                console.log(error.message);
    //            }
    //         );
    //     }, function (error) {
    //         window.top.processingDlg.hide();
    //         console.log(error);
    //     });
    ExecuteAction(
    null, null, "bsd_Action_InterestSimulation_CalculateSimulation", [{
        name: 'interestsimulationid',
        type: 'string',
        value: ""
    },
    {
        name: 'callfrom',
        type: 'string',
        value: "OptionEntrySOA"
    },
    {
        name: 'optinentryid',
        type: 'string',
        value: optinentryid
    },
    {
        name: 'aginginterestsimulationoptionid',
        type: 'string',
        value: ''
    },
    {
        name: 'simulationdate',
        type: 'string',
        value: simulationdate
    },
    {
        name: 'dateofinterestcalculation',
        type: 'string',
        value: simulationdate
    }],

    function (result) {
        // window.top.processingDlg.hide();
        if (result != null) {
            if (result.status == "error") {
                var ss = result.data.split(':');
                var mss = ss[ss.length - 1];
                window.top.$ui.Dialog("Message", mss);
                window.top.processingDlg.hide();
                console.log(result);
            }
            else if (result.status == "success") {
                console.log("Alert dialog closed");
                window.top.processingDlg.hide();
                console.log(result);
                callback();
                // window.top.$ui.Dialog("Message", "Void Bulk waiver success!");
                var confirmStrings = {
                    text: "Print SOA success!",
                    title: "Notices"
                };
                Xrm.Navigation.openAlertDialog(confirmStrings);
            }
        }
    },
    true);
    /*var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false" page="1" count="1">',
        '  <entity name="bsd_interestsimulation">',
        '    <attribute name="bsd_interestsimulationid" />',
        '    <attribute name="bsd_name" />',
        '    <attribute name="createdon" />',
        '    <order attribute="bsd_name" descending="false" />',
        '  </entity>',
        '</fetch>'].join('');
    
    CrmFetchKit.Fetch(xml, false).then(function (rs) {
        
        var simulationdate = glSOADate._d.getFullYear() + "-" + glSOADate._d.getMonth() + "-" + glSOADate._d.getDate();
        if (rs.length > 0) {
            var interestsimulationid = rs[0].Id;
            ExecuteAction(
                interestsimulationid
                , "bsd_interestsimulation"
                , "bsd_Action_InterestSimulation_CalculateSimulation"
                , [
                    { name: 'callfrom', type: 'string', value: "OptionEntrySOA" },
                    { name: 'optinentryid', type: 'string', value: optinentryid },
                    { name: 'aginginterestsimulationoptionid', type: 'string', value: '' },
                    { name: 'simulationdate', type: 'string', value: simulationdate },
                    { name: 'dateofinterestcalculation', type: 'string', value: simulationdate }
                ]
                ,
                function (result) {
                    window.top.processingDlg.hide();
                    if (result != null) {
                        if (result.status == "error") {

                            var ss = result.data.split(':');
                            var mss = ss[ss.length - 1];
                            window.top.$ui.Dialog("Message", mss);

                        }
                        else if (result.status == "success") {
                            callback();
                        }
                    }
                });
        }
    }, function (er) {
        console.log(er.message)
    });*/
}
window.top.printSOA = function (date) {
    debugger;
    window.top.processingDlg.show();
    var optinentryid = Xrm.Page.data.entity.getId();
    var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false" page="1" count="1">', '  <entity name="bsd_interestsimulation">', '    <attribute name="bsd_interestsimulationid" />', '    <attribute name="bsd_name" />', '    <attribute name="createdon" />', '    <order attribute="bsd_name" descending="false" />', '  </entity>', '</fetch>'].join('');
    var rs = CrmFetchKitNew.FetchSync(xml);
    //    CrmFetchKit.Fetch(xml, false).then(function (rs) {
    var simulationdate = date._d.getFullYear() + "-" + date._d.getMonth() + "-" + date._d.getDate();
    if (rs.length > 0) {
        var interestsimulationid = rs[0].Id;
        ExecuteAction(
        interestsimulationid, "bsd_interestsimulation", "bsd_Action_InterestSimulation_CalculateSimulation", [{
            name: 'callfrom',
            type: 'string',
            value: "OptionEntrySOA"
        },
        {
            name: 'optinentryid',
            type: 'string',
            value: optinentryid
        },
        {
            name: 'aginginterestsimulationoptionid',
            type: 'string',
            value: ""
        },
        {
            name: 'simulationdate',
            type: 'string',
            value: simulationdate
        },
        {
            name: 'dateofinterestcalculation',
            type: 'string',
            value: simulationdate
        }],

        function (result) {
            window.top.processingDlg.hide();
            if (result != null) {
                if (result.status == "error") {
                    var ss = result.data.split(':');
                    var mss = ss[ss.length - 1];
                    window.top.$ui.Dialog("Message", mss);
                }
                else if (result.status == "success") {
                    var pageInput = {
                        pageType: "webresource",
                        webresourceName: "bsd_select_report_printsoa.html",
                        data: Xrm.Page.data.entity.getId()
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

                    function success() {},

                    function error(e) {});
                    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_select_report_printsoa.html?id=" + Xrm.Page.data.entity.getId(), { width: 380, height: 344 }, null, null);
                }
            }
        });
    }
    //    }, function (er) {
    //        console.log(er.message)
    //    });
}

function btn_PrintSOA() {
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_select_date_printsoa.html",
        data: Xrm.Page.data.entity.getId()
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

    function success() {},

    function error(e) {});
    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_select_date_printsoa.html?id=" + Xrm.Page.data.entity.getId(), { width: 380, height: 344 }, null, null);
}

function btnVis_PrintSOA() {
    console.log('btnVis_PrintSOA');
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue()
    //var flagAdmin = CheckRole(Xrm.Page.context.getUserRoles(), ["System Administrator"]);
    //var flagFin = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Financal Controller"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Finance Manager"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Chief Accountant"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Accountant"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN"]));
    var flagFin = crmcontrol.checkRoles("CLVN_FIN_Chief Accountant") || crmcontrol.checkRoles("CLVN_FIN_Accountant") || crmcontrol.checkRoles("CLVN_FIN") || crmcontrol.checkRoles("CLVN_FIN_Financal Controller") || crmcontrol.checkRoles("CLVN_FIN_Finance Manager");
    var flagAdmin = crmcontrol.checkRoles("System Administrator");
    //var roleSeniorStaff = crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    var flag = (flagAdmin || flagFin);
    if (formtype != 1 && status != 100000006 && flag == true) return true;
    return false;
}
//--- Han_17092018 > Convert Contract  ---//

function btn_ConvertContract() {
    var parameters = {};
    var op = Xrm.Page.data.entity.getId();
    var opName = Xrm.Page.getAttribute("name").getValue();
    parameters["bsd_optionentry"] = op;
    parameters["bsd_optionentryname"] = opName;
    var optionno = Xrm.Page.getAttribute("bsd_optionno").getValue();
    var bsd_contracttype = Xrm.Page.getAttribute("bsd_contracttype").getValue();
    if (bsd_contracttype == 100000001) {
        parameters["bsd_converttype"] = 100000001;
    }
    else {
        parameters["bsd_converttype"] = 100000000;
    }
    parameters["bsd_name"] = "Convert Contract from Option Entry_" + optionno;
    var Project = Xrm.Page.getAttribute("bsd_project").getValue();
    if (Project) {
        parameters["bsd_project"] = Project[0].id;
        parameters["bsd_projectname"] = Project[0].name;
    }
    var unitnumber = Xrm.Page.getAttribute("bsd_unitnumber").getValue();
    if (unitnumber) {
        parameters["bsd_unit"] = unitnumber[0].id;
        parameters["bsd_unitname"] = unitnumber[0].name;
    }
    Xrm.Utility.openEntityForm("bsd_conversioncontractapproval", null, parameters, {
        openInNewWindow: true
    });
}

function btnVis_ConvertContract() {
    console.log('btnVis_ConvertContract');
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue()
    var flagAdmin = CheckRole(Xrm.Page.context.getUserRoles(), ["System Administrator"]);
    var flagFin = CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager"]);
    if (formtype != 1 && status != 100000006 && status != 100000007 && (flagAdmin == true || flagFin == true)) return true;
    return false;
}

function btn_UpdateContractDate() {
    //dangpa-20200116
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_select_date_updatecontractdate.html",
        data: Xrm.Page.data.entity.getId()
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

    function success() {},

    function error(e) {});
    //end dangpa-20200116
    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_select_date_updatecontractdate.html?id=" + Xrm.Page.data.entity.getId(), { width: 380, height: 344 }, null, null);
}
window.top.applySignUpdateContractDate = function (ngay) {
    var date = new Date(ngay);
    Xrm.Page.getAttribute("bsd_contractdate").setValue(date);
    Xrm.Page.getAttribute("bsd_updatecontractdate").setValue(true);
    var confirmStrings = {
        text: "Update Contract Date Success!",
        title: "Notices"
    };
    Xrm.Navigation.openAlertDialog(confirmStrings);
    // window.top.$ui.Dialog("Message", "Update Contract Date Success!");
    Xrm.Page.data.save();
}

function vis_UpdateContractDate() {
    console.log('vis_UpdateContractDate');
    var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
    var flagFin = (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Head of Section"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Senior Staff"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR_SubSale"]));
    var flagFin2 = CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager"]);
    if (((statuscode == 100000000 || statuscode == 100000001) && flagFin == true) || (statuscode == 100000002 && flagFin2 == true)) return true;
    return false;
}

function visBtnRecalculator() {
    console.log('visBtnRecalculator');
    return false;
}
function btn_Confirm_Amount_bulk(item) {
    debugger;
    window.top.$ui.Confirm("Confirms", "Are you sure to Confirm?", function (e) {
        Xrm.Utility.showProgressIndicator("Please Wait!...");
        debugger;
        let inputs = "";
        for (
        let i = 0; i < item.length; i++) {
            let selectedItem = item[i];

            if (i != item.length - 1) {

                inputs += selectedItem + ",";
            }
            else {
                inputs += selectedItem;
            }

        }
        callFlow(inputs);
    },
    null);
}

async function callFlow(ids) {
    var url = await getURLPA("Confirm_Amount_bulk");
    var data = {
        "ids": ids
    };
    fetch(url, {
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
            Xrm.Utility.closeProgressIndicator();
        }
    }).then(data => {

}).
    catch(error => {
        debugger;
        console.log(error);
    });
}
async function getURLPA(name) {
    var fetchXml = ["<fetch>", "  <entity name='bsd_configgolive'>", "    <attribute name='bsd_url'/>", "    <filter>", "      <condition attribute='bsd_name' operator='eq' value='", name, "'/>", "    </filter>", "  </entity>", "</fetch>"].join("");

    var result = await Xrm.WebApi.retrieveMultipleRecords("bsd_configgolive", "?fetchXml=" + fetchXml);
    if (result.entities.length > 0) {
        var url = result.entities[0].bsd_url;
        return url;
    }
}

function checkCongNo() {
    var id = Xrm.Page.data.entity.getId();
    var fetchXml = ["<fetch>", "  <entity name='bsd_paymentschemedetail'>", "    <attribute name='bsd_paymentschemedetailid'/>", "    <attribute name='bsd_name'/>", "    <attribute name='bsd_duedate'/>", "    <attribute name='statuscode'/>", "    <attribute name='bsd_maintenancefees'/>", "    <attribute name='bsd_maintenancefeeremaining'/>", "    <attribute name='bsd_managementfee'/>", "    <attribute name='bsd_managementfeeremaining'/>", "    <attribute name='bsd_interestchargeremaining'/>", "    <filter>", "      <condition attribute='bsd_optionentry' operator='eq' value='", id, "'/>", "      <condition attribute='statecode' operator='eq' value='0'/>", "    </filter>", "    <order attribute='bsd_ordernumber'/>", "  </entity>", "</fetch>"].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) {
        for (
        const ins of rs) {
            var bsd_duedate = ins.attributes.bsd_duedate;
            if (bsd_duedate && dateWithoutTime(bsd_duedate.value) < dateWithoutTime(new Date())) {
                var statuscode = ins.attributes.statuscode;
                var bsd_maintenancefees = ins.attributes.bsd_maintenancefees;
                var bsd_maintenancefeeremaining = ins.attributes.bsd_maintenancefeeremaining;
                var bsd_managementfee = ins.attributes.bsd_managementfee;
                var bsd_managementfeeremaining = ins.attributes.bsd_managementfeeremaining;
                if ((statuscode && statuscode.value == 100000000) || (bsd_maintenancefees && bsd_maintenancefees.value && bsd_maintenancefeeremaining && bsd_maintenancefeeremaining.value > 0) || (bsd_managementfee && bsd_managementfee.value && bsd_managementfeeremaining && bsd_managementfeeremaining.value > 0)) return true;
            }

            var bsd_interestchargeremaining = ins.attributes.bsd_interestchargeremaining;
            if (bsd_interestchargeremaining && bsd_interestchargeremaining.value > 0) return true;
        }
    }
    return false;
}

function checkExistSubSale() {
    var bsd_unitnumber = crmcontrol.getValue("bsd_unitnumber");

    var fetchXml = ["<fetch>", "  <entity name='bsd_assign'>", "    <attribute name='bsd_assignid'/>", "    <attribute name='bsd_name'/>", "    <filter>", "      <condition attribute='statuscode' operator='in'>", "        <value>1</value>", "        <value>100000000</value>", "      </condition>", "      <condition attribute='bsd_unit' operator='eq' value='", bsd_unitnumber ? bsd_unitnumber[0].id : "", "'/>", "      <condition attribute='statecode' operator='eq' value='0'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) {
        return true;
    }
    return false;
}
function checkAdvancePayment() {
    var fetchXml = ["<fetch>", "  <entity name='bsd_advancepayment'>", "    <attribute name='bsd_advancepaymentid'/>", "    <attribute name='bsd_name'/>", "    <filter>", "      <condition attribute='statuscode' operator='in'>", "        <value>1</value>", "        <value>100000003</value>", "      </condition>", "      <condition attribute='bsd_optionentry' operator='eq' value='", crmcontrol.getId(), "'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) {
        return true;
    }
    return false;
}
function checkBankLoan() {
    var bsd_unitnumber = crmcontrol.getValue("bsd_unitnumber");

    var fetchXml = ["<fetch>", "  <entity name='product'>", "    <attribute name='name'/>", "    <attribute name='bsd_bankloan'/>", "    <filter>", "      <condition attribute='productid' operator='eq' value='", bsd_unitnumber ? bsd_unitnumber[0].id : "", "'/>", "      <condition attribute='bsd_bankloan' operator='eq' value='1'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) {
        return true;
    }
    return false;
}

function dateWithoutTime(value) {
    return new Date(value).setHours(0, 0, 0, 0);
}
function getShortFallAmount() {
    var project = crmcontrol.getValue("bsd_project");
    var fetchXml = ["<fetch>", "  <entity name='bsd_project'>", "    <attribute name='bsd_shortfallamount'/>", "    <filter>", "      <condition attribute='bsd_projectid' operator='eq' value='", project[0].id, "'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) return rs[0].attributes.bsd_shortfallamount.value;
    else return null;
}
function getInstall1() {
    var OEId = crmcontrol.getId();
    var fetchXml = ["<fetch>", "  <entity name='bsd_paymentschemedetail'>", "    <attribute name='bsd_amountofthisphase'/>", "    <attribute name='bsd_amountwaspaid'/>", "    <attribute name='bsd_depositamount'/>", "    <attribute name='bsd_balance'/>", "    <filter>", "      <condition attribute='bsd_optionentry' operator='eq' value='", OEId, "'/>", "      <condition attribute='bsd_ordernumber' operator='eq' value='1'/>", "    </filter>", "  </entity>", "</fetch>"].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) return rs[0].attributes;
    else return null;
}