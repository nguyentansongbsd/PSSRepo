$ = window.parent.$;

/*
+ status : 	In Progress = 1 	    
            Reservation = 100000000 	/ 	Deposited = 3 
            Pending Cancel Deposit = 100000002  / sign RF = 100000004
+ State : Close
            Reject = 100000003    / Expired of sign RF = 100000005
            Terminated = 100000001  / Revised = 7
*/

//----------------------------------- Add Product --------------------------------------- Hide

function quo_btnAddProduct() {

    var reservation = Xrm.Page.data.entity.getId();
    var project = Xrm.Page.getAttribute("bsd_projectid").getValue();
    var pricelist = Xrm.Page.getAttribute("pricelevelid").getValue();
    var phaseslaunch = Xrm.Page.getAttribute("bsd_phaseslaunchid").getValue();
    if (reservation) {
        var parameters = {};
        parameters["bsd_reservation"] = reservation
        parameters["bsd_reservationname"] = Xrm.Page.getAttribute("name").getValue();
        if (project) {
            parameters["bsd_project"] = project[0].id;
            parameters["bsd_projectname"] = project[0].name;
        }
        if (pricelist) {
            parameters["bsd_pricelist"] = pricelist[0].id;
            parameters["bsd_pricelistname"] = pricelist[0].name;
        }
        if (phaseslaunch) {
            parameters["bsd_phaseslaunch"] = phaseslaunch[0].id;;
            parameters["bsd_phaseslaunchname"] = phaseslaunch[0].name;;
        }
        Xrm.Utility.openEntityForm("bsd_reservationproduct", null, parameters, {
            openInNewWindow: true
        });
    }

}

//----------------------------------- Add Recalculator --------------------------------------- Hide

function quo_btnRecalculator() {
    var result = Xrm.Page.getAttribute("bsd_taxcode").getValue();
    if (result != null) {
        window.top.$ui.Confirm("Confirm", "This action will reload Tax", function () {
            window.top.processingDlg.show();
            RunWorkflow("ADD6C8AD-0925-4BA1-9E4C-C54F1AC78DC8", Xrm.Page.data.entity.getId(), function (req) {
                if (req.readyState == 4) {
                    window.top.processingDlg.hide();
                    if (req.status == 200) {
                        if (Xrm.Page.data.entity.getIsDirty()) {
                            Xrm.Page.data.save().then(function () {
                                Xrm.Page.data.refresh();
                            },
                            null);
                        }
                    }
                    else if (req.status == 500) {
                        if (req.responseXML != "") {
                            var mss = req.responseXML.getElementsByTagName("Message");
                            if (mss.length > 0) {
                                var confirmStrings = {
                                    text: mss[0].firstChild.nodeValue,
                                    title: "Message"
                                };
                                Xrm.Navigation.openAlertDialog(confirmStrings);
                            }

                        }
                    }
                }
            });
        },
        null);
    } else {

        var confirmStrings = {
            text: "Fill in Tax Code please !",
            title: "Message"
        };
        Xrm.Navigation.openAlertDialog(confirmStrings);
    }

}

//----------------------------------- Convert To Reservation ---------------------------------------

function btn_ConvertToReservation() {
    var id = Xrm.Page.data.entity.getId();
    window.top.$ui.Confirm('Confirm', "Are you sure to convert to Reservation?", function () {
        window.top.processingDlg.show();
        ExecuteAction(
        id, "quote", "bsd_Action_QuotationReservation_ConvertToReservation", null //[{ name: 'type', type: 'string', value: "sign" }]
        , function (result) {
            window.top.processingDlg.hide();
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
                    var confirmStrings = {
                        text: "Success!",
                        title: "Message"
                    };
                    Xrm.Navigation.openAlertDialog(confirmStrings);
                    if (Xrm.Page.data.entity.getIsDirty()) {
                        Xrm.Page.data.save().then(function () {
                            Xrm.Page.data.refresh();
                        },
                        null);
                    }
                }
            }
        });
    });

}

function btnVis_ConvertToReservation() {
    debugger;
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (formtype != 1 && status == 100000007) return false;
    return false;
}

//----------------------------------- Print Quotation Final ---------------------------------------

function quo_btnPrintQuoteForm() {
    Xrm.Page.data.save().then(function () {
        var idrecord = Xrm.Page.data.entity.getId().replace('{', '').replace('}', '');
        var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" count="1" mapping="logical" distinct="false">' + '<entity name="bsd_paymentschemedetail">' + '<attribute name="bsd_paymentschemedetailid" />' + '<attribute name="bsd_name" />' + '<attribute name="createdon" />' + '<order attribute="bsd_name" descending="false" />' + '<link-entity name="quote" from="quoteid" to="bsd_reservation" alias="ab">' + '<filter type="and">' + '<condition attribute="quoteid" operator="eq" value="{' + idrecord + '}" />' + '</filter>' + '</link-entity>' + '</entity>' + '</fetch>';
        var rs = CrmFetchKitNew.FetchSync(fetchXML);
        if (rs.length > 0) {
            var pageInput = {
                pageType: "webresource",
                webresourceName: "bsd_Print_Quotation.html",
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
            if (Xrm.Page.data.entity.getIsDirty()) {
                Xrm.Page.data.save().then(function () {
                    Xrm.Page.data.refresh();
                },
                null);
            }
            //  code cu
            //  Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_Print_Quotation.html?id=" + Xrm.Page.data.entity.getId(), { width: 480, height: 360 }, null, null);

        }
        else {
            var confirmStrings = {
                text: "You haven't generated Installments for this Reservation, please genarate Installments before printing quote.",
                title: "Notices"
            };
            Xrm.Navigation.openAlertDialog(confirmStrings);
        }
    });

}

function quoVis_btnPrintQuoteForm() {
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (formtype != 1 && status != 100000008 && status != 100000003 && status != 6) return true;
    else return false;
}

//----------------------------------- Sign Quotation Final ---------------------------------------

function btn_SignQuoteForm() {
    Xrm.Page.data.save().then(function () {
        window.top.processingDlg.show();
        var id = Xrm.Page.data.entity.getId();
        ExecuteAction(
        id, "quote", "bsd_Action_QuotationReservation_ConvertToReservation", null //[{ name: 'type', type: 'string', value: "sign" }]
        , function (result) {
            window.top.processingDlg.hide();
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
                else if (result.status == "success") {
                    if (id && id.length > 0) {
                        Xrm.Utility.openEntityForm('quote', id);
                        if (Xrm.Page.data.entity.getIsDirty()) {
                            Xrm.Page.data.save().then(function () {
                                Xrm.Page.data.refresh();
                            },
                            null);
                        }
                    }

                }
            }
        });
    });

}

function btnVis_btnSignQuoteForm() {

    var formtype = Xrm.Page.ui.getFormType();
    var printDate = Xrm.Page.getAttribute("bsd_quotationprinteddate").getValue();
    var signdate = Xrm.Page.getAttribute("bsd_quotationsigneddate").getValue();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (formtype != 1 && printDate && !signdate && status != 100000008 && status != 6) // Status = 2/6 -> Expired/Cancelled
    return true;
    else return false;

}

//----------------------------------- Confirm Deposit ---------------------------------------
function quo_btnConfirmDeposit() {

    Xrm.Page.data.save().then(function () {
        var reservationid = Xrm.Page.data.entity.getId();
        var parameters = {};

        parameters.bsd_reservation = reservationid;
        parameters.bsd_reservationname = Xrm.Page.getAttribute("name").getValue();

        Xrm.Utility.openEntityForm('bsd_payment', null, parameters, {
            openInNewWindow: true
        });
    },
    null);

}

//----------------------------------- Apply Document ---------------------------------------

function quo_btnApplyDocument() {

    var quoteID = Xrm.Page.data.entity.getId();

    if (Xrm.Page.getAttribute("name").getValue() && Xrm.Page.getAttribute("customerid").getValue()) {
        var quoteName = Xrm.Page.getAttribute("name").getValue();

        var customerId = Xrm.Page.getAttribute("customerid").getValue()[0].id;
        var customerName = Xrm.Page.getAttribute("customerid").getValue()[0].name;

        var parameters = {};

        //transaction
        parameters["bsd_transactiontype"] = 1;

        //option entry
        parameters["bsd_reservation"] = quoteID;
        parameters["bsd_reservationname"] = quoteName;

        var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" top="1" >' + '<entity name="quote">' + '<attribute name="quotenumber" />' + '<attribute name="customerid" />' + '<filter type="and" >' + '<condition attribute="quoteid" operator="eq" value="' + quoteID + '" />' + '</filter>' + '</entity>' + '</fetch>';
        var rs = CrmFetchKitNew.FetchSync(fetchXML);
        if (rs.length > 0) {

            var tmp = rs[0].attributes;
            var quotenumber = tmp.quotenumber != null ? tmp.quotenumber.value : null;
            var customerType = tmp.customerid != null ? tmp.customerid.logicalName : null;

            var current = new Date();
            parameters["bsd_name"] = quotenumber + "-AD" + current.format("ddMMyyyyhhmmss");

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
        }

    }

}

function quoVis_btnApplyDocument() {
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (formtype != 1 && (status == 100000006 || status == 100000000 || status == 3)) // Status = colected or reseration or deposited -> Won
    return true;
    else return false;
}

//----------------------------------- Print Reservation Form ---------------------------------------

function quo_btnPrintReservationForm() {
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_Print_Reservation_Form.html",
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

    //code cu
    // Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_Print_Reservation_Form.html?id=" + Xrm.Page.data.entity.getId(), { width: 480, height: 360 }, null, null);
    //var customer = Xrm.Page.getAttribute("customerid").getValue();
    //if (customer[0].type == 2) {
    //    var xml = [];
    //    xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
    //    xml.push("<entity name='contact'>");
    //    xml.push("<attribute name='bsd_fullname' />");
    //    xml.push("<attribute name='contactid' />");
    //    xml.push("<attribute name='gendercode' />");
    //    xml.push("<attribute name='bsd_contactaddress' />");
    //    xml.push("<attribute name='birthdate' />");
    //    xml.push("<attribute name='bsd_diachi' />");
    //    xml.push("<attribute name='bsd_identitycardnumber' />");
    //    xml.push("<filter type='and'>");
    //    xml.push("<condition attribute='contactid' operator='eq' value='" + customer[0].id + "' />");
    //    xml.push("</filter>");
    //    xml.push("</entity>");
    //    xml.push("</fetch>");
    //    CrmFetchKit.Fetch(xml.join(""), true).then(function (rs) {
    //        if (rs.length > 0) {
    //            if (rs[0].attributes.bsd_fullname != null) {
    //                if (rs[0].attributes.gendercode != null) {
    //                    if (rs[0].attributes.birthdate != null) {
    //                        if (rs[0].attributes.bsd_identitycardnumber != null) {
    //                            if (rs[0].attributes.bsd_diachi != null && rs[0].attributes.bsd_contactaddress != null) {
    //                                RunReport();
    //                            }
    //                            else
    //                                window.top.$ui.Dialog("Message", "Missing Customer's Information (Contact Address). Please check again before proceeding this action.", null);
    //                        }
    //                        else
    //                            window.top.$ui.Dialog("Message", "Missing Customer's Information (ID number). Please check again before proceeding this action.", null);

    //                    }
    //                    else
    //                        window.top.$ui.Dialog("Message", "Missing Customer's Information (Birthdate). Please check again before proceeding this action.", null);

    //                }
    //                else
    //                    window.top.$ui.Dialog("Message", "Missing Customer's Information (Gender). Please check again before proceeding this action.", null);
    //            }
    //            else
    //                window.top.$ui.Dialog("Message", "Missing Customer's Information (Full Name). Please check again before proceeding this action.", null);
    //        }
    //    },
    //       function (er) {
    //           console.log(er.message)
    //       });
    //}
    //else if (customer[0].type == 1) {
    //    var xml = [];
    //    xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
    //    xml.push("<entity name='account'>");
    //    xml.push("<attribute name='name' />");
    //    xml.push("<attribute name='primarycontactid' />");
    //    xml.push("<attribute name='telephone1' />");
    //    xml.push("<attribute name='bsd_diachi' />");
    //    xml.push("<attribute name='bsd_address' />");
    //    xml.push("<attribute name='primarycontactid' />");
    //    xml.push("<attribute name='bsd_registrationcode' />");
    //    xml.push("<attribute name='emailaddress1' />");
    //    xml.push("<attribute name='bsd_businesstype' />");
    //    xml.push("<filter type='and'>");
    //    xml.push("<condition attribute='accountid' operator='eq' value='" + customer[0].id + "' />");
    //    xml.push("</filter>");
    //    xml.push("</entity>");
    //    xml.push("</fetch>");
    //    CrmFetchKit.Fetch(xml.join(""), true).then(function (rs) {
    //        if (rs.length > 0) {
    //            if (rs[0].attributes.name != null) {
    //                if (rs[0].attributes.telephone1 != null) {
    //                    if (rs[0].attributes.primarycontactid != null) {
    //                        if (rs[0].attributes.emailaddress1 != null) {
    //                            if (rs[0].attributes.bsd_address != null && rs[0].attributes.bsd_diachi != null) {
    //                                RunReport();
    //                            }
    //                            else
    //                                window.top.$ui.Dialog("Message", "Missing Customer's Information (Contact Address).. Please check again before proceeding this action.", null);
    //                        }
    //                        else
    //                            window.top.$ui.Dialog("Message", "Missing Customer's Information (Email).. Please check again before proceeding this action.", null);

    //                    }
    //                    else
    //                        window.top.$ui.Dialog("Message", "Missing Customer's Information (Mandatory Primary). Please check again before proceeding this action.", null);

    //                }
    //                else
    //                    window.top.$ui.Dialog("Message", "Missing Customer's Information (Phone). Please check again before proceeding this action.", null);
    //            }
    //            else
    //                window.top.$ui.Dialog("Message", "Missing Customer's Information (Account Name). Please check again before proceeding this action.", null);
    //        }
    //    },
    //       function (er) {
    //           console.log(er.message)
    //       });
    //}

}
window.top.setham = function (urlreport, nameReport) {
    var dateprint = new Date();
    var ngay = 0;
    var bsd_reservationprinteddate = Xrm.Page.getAttribute("bsd_reservationprinteddate").getValue();
    crmcontrol.setValue("bsd_reservationprinteddate", new Date());
    var project = crmcontrol.getValue("bsd_projectid");
    if (project != null) {
        var fetch = ["<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>", "<entity name='bsd_project'>", "<attribute name='bsd_expireddateofsigningrf'/>", "<filter type ='and'>", "<condition attribute ='bsd_projectid' operator ='eq'  value ='" + project[0].id + "'/>", "</filter>", "</entity>", "</fetch>"].join("");
        var rs = CrmFetchKitNew.FetchSync(fetch);
        if (rs.length > 0) {

            ngay = rs[0].attributes.bsd_expireddateofsigningrf ? rs[0].attributes.bsd_expireddateofsigningrf.value : 0;
        }
    }
    Xrm.Page.getControl("bsd_signingexpired").setDisabled(true);
    dateprint = common.addDate(dateprint, ngay);
    crmcontrol.setValue("bsd_signingexpired", dateprint);
    if (!bsd_reservationprinteddate) crmcontrol.setValue("bsd_reservationformstatus", 100000001);
    var customer = Xrm.Page.getAttribute("customerid").getValue();
    if (customer[0].entityType == "contact") {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='contact'>");
        xml.push("<attribute name='bsd_fullname' />");
        xml.push("<attribute name='contactid' />");
        xml.push("<attribute name='gendercode' />");
        xml.push("<attribute name='bsd_contactaddress' />");
        xml.push("<attribute name='birthdate' />");
        xml.push("<attribute name='bsd_diachi' />");
        xml.push("<attribute name='bsd_identitycardnumber' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='contactid' operator='eq' value='" + customer[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKitNew.Fetch(xml.join(""), true).then(function (rs) {
            if (rs.length > 0) {
                if (rs[0].attributes.bsd_fullname != null) {
                    if (rs[0].attributes.gendercode != null) {
                        if (rs[0].attributes.birthdate != null) {
                            if (rs[0].attributes.bsd_identitycardnumber != null) {
                                if (rs[0].attributes.bsd_diachi != null && rs[0].attributes.bsd_contactaddress != null) {
                                    RunReport();
                                    window.open(urlreport, nameReport, "resizable=1,width=800,height=700");
                                }
                                else {
                                    //window.top.$ui.Dialog("Message", "Missing Customer's Information (Contact Address). Please check again before proceeding this action.", null);
                                    var alertStrings = {
                                        confirmButtonLabel: "Ok",
                                        text: "Missing Customer's Information (Contact Address). Please check again before proceeding this action.",
                                        title: "Notices"
                                    };
                                    var alertOptions = {
                                        height: 147,
                                        width: 300
                                    };
                                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                                    function success(result) {
                                        console.log("Alert dialog closed");
                                    },
                                    function (error) {
                                        console.log(error.message);
                                    });
                                }
                            }
                            else {
                                var confirmStrings = {
                                    text: "Missing Customer's Information (ID number). Please check again before proceeding this action.",
                                    title: "Message"
                                };
                                Xrm.Navigation.openAlertDialog(confirmStrings);
                                //var alertStrings = { confirmButtonLabel: "Ok", text: "Missing Customer's Information (ID number). Please check again before proceeding this action." };
                                //var alertOptions = { height: 147, width: 300 };
                                //Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                                //    function success(result) {
                                //        console.log("Alert dialog closed");
                                //    },
                                //    function (error) {
                                //        console.log(error.message);
                                //    }
                                //);
                            }
                        }
                        else {
                            //window.top.$ui.Dialog("Message", "Missing Customer's Information (Birthdate). Please check again before proceeding this action.", null);
                            var alertStrings = {
                                confirmButtonLabel: "Ok",
                                text: "Missing Customer's Information (Birthdate). Please check again before proceeding this action.",
                                title: "Notices"
                            };
                            var alertOptions = {
                                height: 147,
                                width: 300
                            };
                            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                            function success(result) {
                                console.log("Alert dialog closed");
                            },
                            function (error) {
                                console.log(error.message);
                            });
                        }

                    }
                    else {
                        var alertStrings = {
                            confirmButtonLabel: "Ok",
                            text: "Missing Customer's Information (Gender). Please check again before proceeding this action.",
                            title: "Notices"
                        };
                        var alertOptions = {
                            height: 147,
                            width: 300
                        };
                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                        function success(result) {
                            console.log("Alert dialog closed");
                        },
                        function (error) {
                            console.log(error.message);
                        });
                        //window.top.$ui.Dialog("Message", "Missing Customer's Information (Gender). Please check again before proceeding this action.", null);
                    }

                }
                else {
                    var alertStrings = {
                        confirmButtonLabel: "Ok",
                        text: "Missing Customer's Information (Full Name). Please check again before proceeding this action.",
                        title: "Notices"
                    };
                    var alertOptions = {
                        height: 147,
                        width: 300
                    };
                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                    function success(result) {
                        console.log("Alert dialog closed");
                    },
                    function (error) {
                        console.log(error.message);
                    });
                    //window.top.$ui.Dialog("Message", "Missing Customer's Information (Full Name). Please check again before proceeding this action.", null);
                }

            }
        },
        function (er) {
            console.log(er.message)
        });
    }
    else if (customer[0].entityType == "account") {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='account'>");
        xml.push("<attribute name='name' />");
        xml.push("<attribute name='primarycontactid' />");
        xml.push("<attribute name='telephone1' />");
        xml.push("<attribute name='bsd_diachi' />");
        xml.push("<attribute name='bsd_address' />");
        xml.push("<attribute name='primarycontactid' />");
        xml.push("<attribute name='bsd_registrationcode' />");
        xml.push("<attribute name='emailaddress1' />");
        xml.push("<attribute name='bsd_businesstype' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='accountid' operator='eq' value='" + customer[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKitNew.Fetch(xml.join(""), true).then(function (rs) {
            if (rs.length > 0) {
                if (rs[0].attributes.name != null) {
                    if (rs[0].attributes.telephone1 != null) {
                        if (rs[0].attributes.primarycontactid != null) {
                            if (rs[0].attributes.emailaddress1 != null) {
                                if (rs[0].attributes.bsd_address != null && rs[0].attributes.bsd_diachi != null) {
                                    RunReport();
                                    window.open(urlreport, nameReport, "resizable=1,width=800,height=700");
                                }
                                else {
                                    //window.top.$ui.Dialog("Message", "Missing Customer's Information (Contact Address).. Please check again before proceeding this action.", null);
                                    var alertStrings = {
                                        confirmButtonLabel: "Ok",
                                        text: "Missing Customer's Information (Contact Address).. Please check again before proceeding this action.",
                                        title: "Notices"
                                    };
                                    var alertOptions = {
                                        height: 147,
                                        width: 300
                                    };
                                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                                    function success(result) {
                                        console.log("Alert dialog closed");
                                    },
                                    function (error) {
                                        console.log(error.message);
                                    });
                                }
                            }
                            else {
                                //window.top.$ui.Dialog("Message", "Missing Customer's Information (Email).. Please check again before proceeding this action.", null);
                                var alertStrings = {
                                    confirmButtonLabel: "Ok",
                                    text: "Missing Customer's Information (Email).. Please check again before proceeding this action.",
                                    title: "Notices"
                                };
                                var alertOptions = {
                                    height: 147,
                                    width: 300
                                };
                                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                                function success(result) {
                                    console.log("Alert dialog closed");
                                },
                                function (error) {
                                    console.log(error.message);
                                });
                            }
                        }
                        else {
                            //window.top.$ui.Dialog("Message", "Missing Customer's Information (Mandatory Primary). Please check again before proceeding this action.", null);
                            var alertStrings = {
                                confirmButtonLabel: "Ok",
                                text: "Missing Customer's Information (Mandatory Primary). Please check again before proceeding this action.",
                                title: "Notices"
                            };
                            var alertOptions = {
                                height: 147,
                                width: 300
                            };
                            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                            function success(result) {
                                console.log("Alert dialog closed");
                            },
                            function (error) {
                                console.log(error.message);
                            });
                        }

                    }
                    else {
                        //window.top.$ui.Dialog("Message", "Missing Customer's Information (Phone). Please check again before proceeding this action.", null);
                        var alertStrings = {
                            confirmButtonLabel: "Ok",
                            text: "Missing Customer's Information (Phone). Please check again before proceeding this action.",
                            title: "Notices"
                        };
                        var alertOptions = {
                            height: 147,
                            width: 300
                        };
                        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                        function success(result) {
                            console.log("Alert dialog closed");
                        },
                        function (error) {
                            console.log(error.message);
                        });
                    }
                }
                else {
                    //window.top.$ui.Dialog("Message", "Missing Customer's Information (Account Name). Please check again before proceeding this action.", null);
                    var alertStrings = {
                        confirmButtonLabel: "Ok",
                        text: "Missing Customer's Information (Account Name). Please check again before proceeding this action.",
                        title: "Notices"
                    };
                    var alertOptions = {
                        height: 147,
                        width: 300
                    };
                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                    function success(result) {
                        console.log("Alert dialog closed");
                    },
                    function (error) {
                        console.log(error.message);
                    });
                }
            }
        },
        function (er) {
            console.log(er.message)
        });
    }

}
function RunReport() {
    //var idrecord = Xrm.Page.data.entity.getId().replace('{', '').replace('}', '');
    //var nameReport = "Deposit Receipt";
    //var urlreport = getReportURL("run", "rpD1M_EN%20Reservation.rdl", "E688FF8C-F0BC-E611-80FD-3863BB36FBF0", idrecord, window.top.Mscrm.EntityPropUtil.EntityTypeName2CodeMap[Xrm.Page.data.entity.getEntityName()]);
    //window.open(urlreport, nameReport, "resizable=1,width=800,height=700");
    RunWorkflow("4E7295B4-12D4-47A1-851B-61B50D4D55D3", Xrm.Page.data.entity.getId(), function (req) {
        if (req.readyState == 4) {
            window.top.processingDlg.hide();
            if (req.status == 200) {
                if (Xrm.Page.data.entity.getIsDirty()) {
                    //Xrm.Page.data.save().then(function () {
                    //    Xrm.Page.data.refresh();
                    //},
                    //null);
                    ExecuteAction(
                        id, "quote", "bsd_Action_Resv_Gene_PMS", null //[{ name: 'type', type: 'string', value: "sign" }]
                        , function (result) {
                            Xrm.Page.data.refresh();
                        });
                }
            }
            else if (req.status == 500) {
                if (req.responseXML != "") {
                    var mss = req.responseXML.getElementsByTagName("Message");
                    if (mss.length > 0) {
                        var confirmStrings = {
                            text: mss[0].firstChild.nodeValue,
                            title: "Message"
                        };
                        Xrm.Navigation.openAlertDialog(confirmStrings);
                    }

                }
            }
        }
    });

}

//----------------------------------- Follow Up List---------------------------------------
function btn_FollowUpList() {
    debugger;
    var ResID = Xrm.Page.data.entity.getId();
    var ResName = Xrm.Page.getAttribute("name").getValue();
    var project = Xrm.Page.getAttribute("bsd_projectid").getValue();

    var parameters = {};

    if (project) {
        parameters["bsd_project"] = project[0].id;
        parameters["bsd_projectname"] = project[0].name;
    }

    parameters["bsd_group"] = 100000000;

    var t = new Date();
    parameters["bsd_date"] = new Date().toISOString();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var ReservationFormStatus = Xrm.Page.getAttribute("bsd_reservationformstatus").getValue();
    if (status == 3) //Deposted
    parameters["bsd_type"] = 100000005; //Reservation terminate
    else if (status == 100000006 || status == 100000004) // Collected || Signed RS
    parameters["bsd_type"] = 100000001; // Reservation Deposited
    else if (ReservationFormStatus && ReservationFormStatus == 100000001) // Prinetd RS
    parameters["bsd_type"] = 100000000; //Sign off RS
    else if (status == 100000000) parameters["bsd_type"] = 100000000; //Sign off RS
    parameters["bsd_reservation"] = ResID;
    parameters["bsd_reservationname"] = ResName;

    var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">' + '<entity name="product">' + '<attribute name="name" />' + '<attribute name="productid" />' + '<link-entity name="quotedetail" from="productid" to="productid" alias="ac">' + '<link-entity name="quote" from="quoteid" to="quoteid" alias="ad">' + '<filter type="and">' + '<condition attribute="quoteid" operator="eq" value="' + ResID + '" />' + '</filter>' + '</link-entity>' + '</link-entity>' + '</entity>' + '</fetch>';
    var rs = CrmFetchKitNew.FetchSync(fetchXML);
    if (rs.length > 0) {

        var tmp = rs[0].attributes;
        parameters["bsd_units"] = tmp.productid.value;
        parameters["bsd_unitsname"] = tmp.name.value;

    }
    Xrm.Utility.openEntityForm("bsd_followuplist", null, parameters, {
        openInNewWindow: true
    });

}

//----------------------------------- CCR REJECT ---------------------------------------
function btn_CCRReject() {

    var status = Xrm.Page.getAttribute("statuscode").getValue();
    //var RSupload = Xrm.Page.getAttribute("bsd_reservationuploadeddate").getValue();
    //if (RSupload == null)
    //    window.top.$ui.Dialog("Message", "Please provide reservation uploaded date to continue.");
    //if (( status == 100000004))
    //Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_RefusalReasons", { width: 500, height: 220 }, null, null);
    var applyID = Xrm.Page.data.entity.getId();
    var pageInput = {
        pageType: "webresource",
        webresourceName: "bsd_RefusalReasons",
        data: applyID
    };
    var navigationOptions = {
        target: 2,
        // 2 is for opening the page as a dialog.
        width: 560,
        // default is px. can be specified in % as well.
        height: 320,
        // default is px. can be specified in % as well.
        position: 1 // Specify 1 to open the dialog in center; 2 to open the dialog on the side. Default is 1 (center).
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
    function success() {},
    function error(e) {});

}

function btnVis_CCRReject() {
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (formtype != 1 && (status == 3)) // Signed
    return true;
    return false;
}

//----------------------------------- Sign Reservation Form ---------------------------------------
function btn_SignRF() {
    debugger;
    var bsd_managementfee = Xrm.Page.getAttribute("bsd_managementfee").getValue();
    var flag = check_management(bsd_managementfee);
    switch (flag) {
    case true:
        window.top.$ui.Confirm("Confirms", "Are you sure to Sign this Reservation?", function (e) {
            window.top.processingDlg.show();
            ExecuteAction(
            Xrm.Page.data.entity.getId(), "quote", "bsd_Action_Reservation_SignandUploadRF", [{
                name: 'type',
                type: 'string',
                value: "sign"
            }], function (result) {

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
                        ExecuteAction(
                        Xrm.Page.data.entity.getId(), "quote", "bsd_Action_QR_CreateFUL", [{
                            name: 'type',
                            type: 'string',
                            value: "Sign"
                        }], function (result) {

                            if (result != null) {

                                window.top.processingDlg.hide();
                                if (result.status == "error") {
                                    var ss = result.data.split(':');
                                    var mss = ss[ss.length - 1];
                                    var confirmStrings = {
                                        text: mss,
                                        title: "Message"
                                    };
                                    Xrm.Navigation.openAlertDialog(confirmStrings);
                                }
                                else {}
                            }
                        });

                        var confirmStrings = {
                            text: "Signed successfully!",
                            title: "Message"
                        };
                        Xrm.Navigation.openAlertDialog(confirmStrings).then(
                        function (success) {
                            window.top.location.reload();
                        },
                        function (error) {
                            console.log(error.message);
                        },
                        );
                        //                                if (Xrm.Page.data.entity.getIsDirty()) {
                        //                                    Xrm.Page.data.save().then(function () {
                        //                                        Xrm.Page.data.refresh();
                        //                                    }, null);
                        //                                }
                    }
                }
            });
        });
        break;
    case false:
        default:
        var confirmStrings = {
            text: "Please check Management Fee on Installment!",
            title: "Notices"
        };
        Xrm.Navigation.openAlertDialog(confirmStrings);
        //window.top.$ui.Dialog("Message", "Please check Management Fee on Installment!");
        break;
    }

}
function btnVis_SignRF() {

    var formtype = Xrm.Page.ui.getFormType();
    //var status = Xrm.Page.getAttribute("statuscode").getValue();
    var update = Xrm.Page.getAttribute("bsd_reservationuploadeddate").getValue();
    var signRS = Xrm.Page.getAttribute("bsd_rfsigneddate").getValue()
    if (formtype != 1 && !signRS && update) {
        return true;
    }
    return false;
}

//----------------------------------- Date Upload Reservation Form ---------------------------------------

function btn_DateUploadRF() {
    window.top.$ui.Confirm("Confirms", "Are you sure to Upload the Reservation?", function (e) {
        debugger;
        ExecuteAction(
        Xrm.Page.data.entity.getId(), "quote", "bsd_Action_Reservation_SignandUploadRF", [{
            name: 'type',
            type: 'string',
            value: "upload"
        }], function (result) {

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
                    var confirmStrings = {
                        text: "Update successful.",
                        title: "Message"
                    };
                    crmcontrol.setValue("bsd_reservationuploadeddate", new Date());
                    Xrm.Navigation.openAlertDialog(confirmStrings);
                    if (Xrm.Page.data.entity.getIsDirty()) {
                        Xrm.Page.data.save().then(function () {
                            Xrm.Page.data.refresh();
                        },
                        null);
                    }
                }
            }
        });
    });
    if (Xrm.Page.data.entity.getIsDirty()) {
        Xrm.Page.data.save().then(function () {
            Xrm.Page.data.refresh();
        },
        null);
    }
}

function btnVis_DateUploadRF() {
    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1) {
        var printDate = Xrm.Page.getAttribute("bsd_reservationprinteddate").getValue();
        var status = Xrm.Page.getAttribute("statuscode").getValue();
        var update = Xrm.Page.getAttribute("bsd_reservationuploadeddate").getValue();
        if (printDate && status != 100000008 && !update) return true;
    }
    return false;
}

//----------------------------------- Cancel Reservation (Not Deposit) ---------------------------------------

function btn_CancelReservation() {

    window.top.$ui.Confirm("Confirm", "Do you want to Cancel Reservation?", function (e) {
        window.top.processingDlg.show();
        ExecuteAction(
        Xrm.Page.data.entity.getId(), "quote", "bsd_Action_Reservation_Cancel", null // [{ name: 'type', type: 'string', value: "queue" }]
        , function (result) {
            window.top.processingDlg.hide();
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
                else if (result.status == "success") {
                    var confirmStrings = {
                        text: "Successfull!",
                        title: "Message"
                    };
                    Xrm.Navigation.openAlertDialog(confirmStrings);
                    if (Xrm.Page.data.entity.getIsDirty()) {
                        Xrm.Page.data.save().then(function () {
                            Xrm.Page.data.refresh();
                        },
                        null);
                    }
                }
            }
        });
    },
    null);

}

function btnVis_CancelReservation() {
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (formtype != 1) {
        if ((status == 1 || status == 100000000 || status == 100000006 || status == 100000007) && status != 100000008 && status != 100000003) return true;
        //var xml1 = [];
        //xml.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>");
        //xml.push("<entity name='bsd_payment'>");
        //xml.push("<attribute name='bsd_paymentid' />");
        //xml.push("<filter type='and'>");
        //xml.push("<condition attribute='bsd_reservation' operator='eq' uitype='quote' value='" + id + "' />");
        //xml.push("<condition attribute='statuscode' operator='eq' value='100000000' />");
        //xml.push("<condition attribute='bsd_paymenttype' operator='eq' value='100000001' />");
        //xml.push("</filter>");
        //xml.push("</entity>");
        //xml.push("</fetch>");
        //CrmFetchKit.Fetch(xml1.join(""), false).then(function (rs) {
        //    if (rs.length == 0) {
        //        return true;
        //    }
        //},
        //       function (er) {
        //           console.log(er.message)
        //       });
        //return false;
    }
    return false;
}

//----------------------------------- Termination Reservation (Deposited) ---------------------------------------

function btn_TerminationReservation() {

    var ResID = Xrm.Page.data.entity.getId();
    var ResName = Xrm.Page.getAttribute("name").getValue();

    var project = Xrm.Page.getAttribute("bsd_projectid").getValue();

    var parameters = {};

    if (project) {
        parameters["bsd_project"] = project[0].id;
        parameters["bsd_projectname"] = project[0].name;
    }
    var t = new Date();
    parameters["bsd_date"] = new Date().toISOString();
    //parameters["bsd_name"] = "FUL-" + ResName + "-" ;
    parameters["bsd_type"] = 100000005;
    parameters["bsd_reservation"] = ResID;
    parameters["bsd_reservationname"] = ResName;

    var fetchXML = '<?xml version="1.0" encoding="utf-8"?>' + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">' + '<entity name="product">' + '<attribute name="name" />' + '<attribute name="productid" />' + '<link-entity name="quotedetail" from="productid" to="productid" alias="ac">' + '<link-entity name="quote" from="quoteid" to="quoteid" alias="ad">' + '<filter type="and">' + '<condition attribute="quoteid" operator="eq" value="' + ResID + '" />' + '</filter>' + '</link-entity>' + '</link-entity>' + '</entity>' + '</fetch>';
    var rs = CrmFetchKitNew.FetchSync(fetchXML);
    if (rs.length > 0) {

        var tmp = rs[0].attributes;
        parameters["bsd_units"] = tmp.productid.value;
        parameters["bsd_unitsname"] = tmp.name.value;

    }

    Xrm.Utility.openEntityForm("bsd_followuplist", null, parameters, {
        openInNewWindow: true
    });

}

function btnVis_TerminationReservation() {

    var formtype = Xrm.Page.ui.getFormType();
    if (formtype != 1) {
        var state = Xrm.Page.getAttribute("statecode").getValue(); //state==0: draft
        var status = Xrm.Page.getAttribute("statuscode").getValue();

        if (status == 4 || status == 100000008) { // won || !=expired
            return false;
        }
        var id = Xrm.Page.data.entity.getId();
        if (id && status != 100000003) {
            var xml1 = [];
            var flag1 = false;
            var flag2 = false;
            if (status == 3) flag1 = true;
            else {
                xml1.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>");
                xml1.push("<entity name='bsd_payment'>");
                xml1.push("<attribute name='bsd_paymentid' />");
                xml1.push("<filter type='and'>");
                xml1.push("<condition attribute='bsd_reservation' operator='eq' uitype='quote' value='" + id + "' />");
                xml1.push("<condition attribute='statuscode' operator='eq' value='100000000' />");
                xml1.push("<condition attribute='bsd_paymenttype' operator='eq' value='100000001' />");
                xml1.push("</filter>");
                xml1.push("</entity>");
                xml1.push("</fetch>");
                var rs = CrmFetchKitNew.FetchSync(xml1.join(""));
                if (rs.length > 0) {
                    flag1 = true;
                }
            }
            var xml = [];
            xml.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>");
            xml.push("<entity name='salesorder'>");
            xml.push("<attribute name='salesorderid' />");
            xml.push("<filter type='and'>");
            xml.push("<condition attribute='quoteid' operator='eq' uitype='quote' value='" + id + "' />");
            xml.push("</filter>");
            xml.push("</entity>");
            xml.push("</fetch>");
            var rs = CrmFetchKitNew.FetchSync(xml.join(""));

            if (rs.length == 0) {
                flag2 = true;
            }
            return (flag1 && flag2);
        }
    }
    return false;
}
//Check role
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
//----------------------------------- Visable Button ---------------------------------------

function quoVis_AddUnits_ConfirmQuote() {

    if (Xrm.Page.getAttribute("statuscode").getValue() == 1) {
        return true;
    }
    return false;
}

function quoVis_ConfirmDeposit() {

    if ((Xrm.Page.getAttribute("statuscode").getValue() == 100000000 || Xrm.Page.getAttribute("statuscode").getValue() == 100000006) && ((CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Accountant"])) || (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Chief Accountant"])) || (CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Financal Controller"])) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_FIN_Finance Manager"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_S&M_Senior Sale Staff"]) || CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_S&M_Sales Manager"]) || heckRole(Xrm.Page.context.getUserRoles(), ["CLVN_S&M_Head of Sale"]))) {
        return true;
    }
    return false;
}
function quoVis_PrintReservationForm() {
    //var rec = Xrm.Page.getAttribute("bsd_salesdepartmentreceiveddeposit").getValue();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (status == 3 || status == 100000006 || status == 4) { // Status : 4 -> Won
        return true;
    }
    return false;
}
function btnVis_FollowUpList() {

    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (Xrm.Page.ui.getFormType() != 1 && status != 100000008 && status != 100000003 && status != 100000007 && status != 4 && status != 6) {
        return true;
    }
    return false;
}
function btnVis_ConvertToOption() {
    debugger;
    //return false;
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var bsd_rfsigneddate = Xrm.Page.getAttribute("bsd_rfsigneddate").getValue();
    var roleSeniorStaff = crmcontrol.checkRoles("CLVN_CCR Senior Staff");
    var roleManager = crmcontrol.checkRoles("CLVN_CCR Manager");
    //Expired Quotation 100000008
    //Won 4

    if (Xrm.Page.ui.getFormType() != 1 && status != 100000008 && status != 4 && bsd_rfsigneddate && (roleSeniorStaff || roleManager)) {
        return true;
    }
    return false;
}
//-------------------------------------- RUN REPORT ---------------------------------------

function getReportURL(action, fileName, idreport, idrecord, recordstype) {

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

//----------------------------------- Payment scheme generate ---------------------------------------

function btn_PSGenerate() {
    if (!checkHandoverCondition()) {
        crmcontrol.openAlertDialog("Please select a Handover Condition.", "Notices");
        return;
    }

    var id = Xrm.Page.data.entity.getId();
    window.top.$ui.Confirm('Confirm', "Would you like to generate payment scheme?", function () {
        window.top.processingDlg.show();
        Xrm.Page.data.save().then(
        function () {
            ExecuteAction(
            id, "quote", "bsd_Action_Resv_Gene_PMS", null //[{ name: 'type', type: 'string', value: "sign" }]
            , function (result) {
                window.top.processingDlg.hide();
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
                            text: "Generate payment scheme success!",
                            title: "Notices"
                        };
                        Xrm.Navigation.openAlertDialog(confirmStrings).then(
                        function success(result) {
                            Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId());
                        },

                        function (error) {
                            crmcontrol.alertdialogConfirm(error.message);
                        });
                        // if (Xrm.Page.data.entity.getIsDirty()) {
                        //     Xrm.Page.data.save().then(function () {
                        //         Xrm.Page.data.refresh();
                        //     }, null);
                        // }
                    }
                }
            });
        },

        function () {
            window.top.processingDlg.hide();
            return;
        });
    });

}

function btnVis_PSGenerate() {

    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    // if (formtype != 1 && status != 100000008 && status == 100000007) // Status = Quotation (100000007) -> Hiện
    //     return true;
    if (formtype != 1 && status == 100000007) // Status = Quotation (100000007) -> Hiện
    {
        var isExist = Xrm.Page.getAttribute("bsd_existinstallment").getValue();
        return !isExist;
    }
    return false;
}

function blockAllFields() {
    Xrm.Page.ui.controls.forEach(
    function (control, index) {
        if (control.getControlType() != "subgrid") control.setDisabled(true);
    });
}
window.top.setngay = function (dayexpired) {
    var projectcode = "";
    var dayexpired = dayexpired;
    var xml1 = [];
    xml1.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='true'>");
    xml1.push("<entity name='bsd_project'>");
    xml1.push("<attribute name='bsd_projectid' />");
    xml1.push("<attribute name='bsd_projectcode' />");
    xml1.push("<attribute name='bsd_quotationvalidatetime' />");
    xml1.push("<order attribute='bsd_name' descending='false' />");
    xml1.push("<link-entity name='quote' from='bsd_projectid' to='bsd_projectid' alias='ac'>");
    xml1.push("<filter type='and'>");
    xml1.push("<condition attribute='quoteid' operator='eq'  uitype='quote' value='" + Xrm.Page.data.entity.getId() + "' />");
    xml1.push("</filter>");
    xml1.push("</link-entity>");
    xml1.push("</entity>");
    xml1.push("</fetch>");
    var rs1 = CrmFetchKitNew.FetchSync(xml1.join(""));
    if (rs1.length > 0 && rs1[0].attributes.bsd_quotationvalidatetime) {
        dayexpired = rs1[0].attributes.bsd_quotationvalidatetime.value;
        if (rs1[0].attributes.bsd_projectcode) projectcode = rs1[0].attributes.bsd_projectcode.value;
    }

    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (Xrm.Page.getAttribute("bsd_quotationprinteddate").getValue() == null && (status != 4)) {
        var date = new Date();
        date = new Date(date.toDateString());
        Xrm.Page.getAttribute("bsd_quotationprinteddate").setValue(date);

        date.setDate(date.getDate() + dayexpired);
        Xrm.Page.getAttribute("bsd_expireddateofsigningqf").setValue(date);
        Xrm.Page.data.save();
    }
}
function check_management(mana) {
    debugger;
    var flag = false;
    var id = Xrm.Page.data.entity.getId();
    var fetchXml = ["<fetch aggregate='true'>", "  <entity name='bsd_paymentschemedetail'>", "    <attribute name='bsd_managementamount' alias='managementamount' aggregate='sum' />", "    <filter>", "      <condition attribute='bsd_reservation' operator='eq' value='", id, "'/>", "    </filter>", "  </entity>", "</fetch>", ].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) {
        var management = rs[0].attributes.managementamount.value;
        if (mana == management) {
            flag = true;
        }
    }
    return flag;
}
function getobjectTypeCode() {
    var requestUrl = "/api/data/v8.2/EntityDefinitions?$filter=LogicalName eq '" + Xrm.Page.data.entity.getEntityName() + "'&$select=ObjectTypeCode";
    var object = null;

    var context;

    if (typeof GetGlobalContext === "function") {
        context = GetGlobalContext();
    } else {
        context = Xrm.Page.context;
    }
    var req = new XMLHttpRequest();
    req.open("GET", context.getClientUrl() + requestUrl, true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var result = JSON.parse(this.response);
                var objectTypeCode = result.value[0].ObjectTypeCode;
                alert(objectTypeCode);
                object = objectTypeCode;
                //use retrieved objectTypeCode
            } else {
                var errorText = this.responseText;
                //handle error here
            }
        }
    };
    req.send();
    return object;
}

function btn_ClearIns() {
    window.top.$ui.Confirm('Confirm', "Would you like to delete payment scheme?", function () {
        window.top.processingDlg.show();
        Xrm.Page.data.save().then(

        function () {
            ExecuteAction(
            null, null, "bsd_Action_ClearInstallment", [{
                name: 'id',
                type: 'string',
                value: Xrm.Page.data.entity.getId()
            },
            {
                name: 'logicalNameLookup',
                type: 'string',
                value: 'bsd_reservation'
            }], function (result) {
                window.top.processingDlg.hide();
                if (result != null) {
                    if (result.status == "error") crmcontrol.alertdialogConfirm(result.data);
                    else {
                        var confirmStrings = {
                            text: "Delete payment scheme success!",
                            title: "Notices"
                        };
                        Xrm.Navigation.openAlertDialog(confirmStrings).then(
                        function success(result) {
                            Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId());
                        },

                        function (error) {
                            crmcontrol.alertdialogConfirm(error.message);
                        });
                    }
                }
            },
            true);
        },

        function () {
            window.top.processingDlg.hide();
            return;
        });
    });
}

function vis_ClearIns() {
    var formtype = Xrm.Page.ui.getFormType();
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (formtype != 1 && status == 100000007) // Quotation
    {
        var isExist = Xrm.Page.getAttribute("bsd_existinstallment").getValue();
        return isExist;
    }
    return false;
}

function checkHandoverCondition() {
    var id = Xrm.Page.data.entity.getId();
    var fetchXml = ["<fetch>", "  <entity name='bsd_packageselling'>", "    <attribute name='bsd_packagesellingid'/>", "    <attribute name='bsd_name'/>", "    <filter>", "      <condition attribute='statecode' operator='eq' value='0'/>", "    </filter>", "    <link-entity name='bsd_quote_bsd_packageselling' from='bsd_packagesellingid' to='bsd_packagesellingid' alias='quote' intersect='true'>", "      <attribute name='quoteid'/>", "      <filter>", "        <condition attribute='quoteid' operator='eq' value='", id, "'/>", "      </filter>", "    </link-entity>", "  </entity>", "</fetch>"].join("");
    var rs = CrmFetchKitNew.FetchSync(fetchXml);
    if (rs.length > 0) {
        return true;
    }
    return false;
}