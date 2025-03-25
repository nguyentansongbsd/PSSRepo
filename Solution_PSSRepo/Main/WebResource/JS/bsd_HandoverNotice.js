// JavaScript source code
window.top.winpopup = null;
if (window.$ == null) window.$ = window.parent.$;
function onLoad() {
    var path = Xrm.Page.context.getClientUrl() + "/WebResources/";
    var head = window.top.document.getElementsByTagName('head')[0];
    var style = window.top.document.createElement('link');
    style.href = path + "bsd_loginform.css";
    style.type = 'text/css';
    style.rel = 'stylesheet';
    head.append(style);
    var statuscode = crmcontrol.getValue('statuscode');
    if (statuscode == 100000003) //Closed
    {
        crmcontrol.disabledForm();
    }
    if (crmcontrol.getValue('bsd_generatedbysystem') == true) {
        crmcontrol.disabledForm();
    }
}

function onchange_Estimate_Interest() {
    var estimateintesrest = Xrm.Page.getAttribute("bsd_estimateintesrest").getValue();
    var actualinterest = Xrm.Page.getAttribute("bsd_actualinterest").getValue();
    var installmentamount = Xrm.Page.getAttribute("bsd_installmentamount").getValue();
    var maintenancefee = Xrm.Page.getAttribute("bsd_maintenancefee").getValue();
    var managementfee = Xrm.Page.getAttribute("bsd_managementfee").getValue();
    var outstandingincludeinterest = Xrm.Page.getAttribute("bsd_outstandingincludeinterest").getValue();
    var advancepaymentamount = Xrm.Page.getAttribute("bsd_advancepaymentamount").getValue();
    var other = Xrm.Page.getAttribute("bsd_other").getValue();
    var totalwaiveramount = Xrm.Page.getAttribute("bsd_totalwaiveramount").getValue();
    var TotalInterestAmount = actualinterest + estimateintesrest;
    Xrm.Page.getAttribute("bsd_totalinterestamount").setValue(TotalInterestAmount);
    var TotalAmount = installmentamount + maintenancefee + managementfee + outstandingincludeinterest + TotalInterestAmount - totalwaiveramount - advancepaymentamount + other;
    Xrm.Page.getAttribute("bsd_totalamount").setValue(TotalAmount);

}

function onSave(context) {
    var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
    var bsd_name = Xrm.Page.getAttribute("bsd_name").getValue();
    var id = Xrm.Page.data.entity.getId();
    if (statuscode == 100000002) {
        //gb_context = context
        //context.getEventArgs().preventDefault();
        Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_handovernotices_select_report.html?type=sendemail&data=" + id, {
            width: 410,
            height: 345
        },
        null, null);
        var timerpopup = setInterval(function () {
            console.log(window.top.winpopup);
            if (window.top.winpopup != null) {
                if (window.top.winpopup.closed) {
                    window.top.location.reload();
                }
            }
        },
        1000);
    }

}
var gb_emailto = "";
var gb_emailtoname = "";
var gb_emailcc = "";
var gb_emailccname = "";
var gb_context;
window.top.showPopupSendMail = function () {
    window.top.processingDlg.show();
    var bsd_optionentry = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    var bsd_customer = Xrm.Page.getAttribute("bsd_customer").getValue();
    common.getCustomer(bsd_customer[0].id, bsd_customer[0].typename, function (customer) {
        if (customer != null) {
            switch (customer.logicalName) {
            case "contact":
                gb_emailto = customer.attributes['emailaddress1'] != null ? customer.attributes['emailaddress1'].value : '';
                gb_emailtoname = customer.attributes['bsd_fullname'] != null ? customer.attributes['bsd_fullname'].value : '';
                break;
            case "account":
                gb_emailto = customer.attributes['emailaddress1'] != null ? customer.attributes['emailaddress1'].value : '';
                gb_emailtoname = customer.attributes['bsd_name'] != null ? customer.attributes['bsd_name'].value : '';
                break;
            }
        }
        common.getCoownerbyOptionEntry(bsd_optionentry[0].id, function (coownerlist) {
            window.top.processingDlg.hide();

            if (coownerlist.length > 0) {
                coCustomer = coownerlist[0].attributes['bsd_customer'];
                common.getCustomer(coCustomer.guid, coCustomer.logicalName, function (data) {
                    if (data != null) {
                        switch (data.logicalName) {
                        case "contact":
                            gb_emailcc = data.attributes['emailaddress1'] != null ? data.attributes['emailaddress1'].value : '';
                            gb_emailccname = data.attributes['bsd_fullname'] != null ? data.attributes['bsd_fullname'].value : '';
                            break;
                        case "account":
                            gb_emailcc = data.attributes['emailaddress1'] != null ? data.attributes['emailaddress1'].value : '';
                            gb_emailccname = data.attributes['bsd_name'] != null ? data.attributes['bsd_name'].value : '';
                            break;
                        }
                    }
                    var strloginfrm = common.createFromLoginEmail('', '', gb_emailto, gb_emailtoname, gb_emailcc, gb_emailccname, 'window.top.sendEmailAttach()');
                    window.top.$ui.Popup('popup', "", strloginfrm);
                });
            } else {
                var strloginfrm = common.createFromLoginEmail('', '', gb_emailto, gb_emailtoname, gb_emailcc, gb_emailccname, 'window.top.sendEmailAttach()');
                window.top.$ui.Popup('popup', "", strloginfrm);
            }

        });
    });

}

window.top.sendEmailAttach = function () {

    var id = Xrm.Page.data.entity.getId();
    var bsd_name = Xrm.Page.getAttribute("bsd_name").getValue();
    var bsd_noticesnumber = Xrm.Page.getAttribute("bsd_noticesnumber").getValue();
    var bsd_customer = Xrm.Page.getAttribute("bsd_customer").getValue();
    var url = "https://gby.vn/doc/" + bsd_noticesnumber.replace(/[^a-zA-Z0-9]/g, '_') + ".pdf";
    var attachment = url;
    var email = window.top.$('#frmLoginEmail #email').val();
    var password = window.top.$('#frmLoginEmail #password').val();
    if (email != '' && password != '') {
        window.top.$ui.ClosePopup('popup');
        window.top.processingDlg.show();
        switch (bsd_customer[0].entityType) {
        case "contact":
            var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="contact">', '    <attribute name="fullname" />', '    <attribute name="statuscode" />', '    <attribute name="ownerid" />', '    <attribute name="mobilephone" />', '    <attribute name="jobtitle" />', '    <attribute name="bsd_identitycardnumber" />', '    <attribute name="gendercode" />', '    <attribute name="emailaddress1" />', '    <attribute name="createdon" />', '    <attribute name="birthdate" />', '    <attribute name="address1_composite" />', '    <attribute name="bsd_fullname" />', '    <attribute name="contactid" />', '    <order attribute="createdon" descending="true" />', '    <filter type="and">', '      <condition attribute="contactid" operator="eq" uitype="contact" value="' + bsd_customer[0].id + '" />', '    </filter>', '  </entity>', '</fetch>'].join('');
            CrmFetchKit.Fetch(xml, false).then(function (rs) {
                if (rs.length > 0) {
                    if (rs[0].attributes["emailaddress1"] != null) {
                        var to = rs[0].attributes["emailaddress1"].value;
                        var toname = rs[0].attributes["bsd_fullname"].value;
                        sendEmailHandoverNotice(email, password, id, to, toname, bsd_name, attachment);
                    }
                    else {
                        window.top.processingDlg.hide();
                        window.top.$ui.Dialog("Message", "Email does not exist!");
                    }
                }
            });
            break;
        case "account":
            var xml = ['<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">', '  <entity name="account">', '    <attribute name="primarycontactid" />', '    <attribute name="telephone1" />', '    <attribute name="bsd_rocnumber2" />', '    <attribute name="bsd_rocnumber1" />', '    <attribute name="websiteurl" />', '    <attribute name="bsd_vatregistrationnumber" />', '    <attribute name="bsd_incorporatedate" />', '    <attribute name="bsd_hotlines" />', '    <attribute name="bsd_generalledgercompanynumber" />', '    <attribute name="fax" />', '    <attribute name="emailaddress1" />', '    <attribute name="bsd_groupgstregisttationnumber" />', '    <attribute name="statuscode" />', '    <attribute name="ownerid" />', '    <attribute name="createdon" />', '    <attribute name="address1_composite" />', '    <attribute name="bsd_companycode" />', '    <attribute name="bsd_registrationcode" />', '    <attribute name="bsd_accountnameother" />', '    <attribute name="bsd_name" />', '    <attribute name="name" />', '    <attribute name="accountid" />', '    <order attribute="createdon" descending="true" />', '    <filter type="and">', '      <condition attribute="accountid" operator="eq" uitype="account" value="' + bsd_customer[0].id + '" />', '    </filter>', '  </entity>', '</fetch>'].join('');
            CrmFetchKit.Fetch(xml, false).then(function (rs) {
                if (rs.length > 0) {
                    if (rs[0].attributes["emailaddress1"] != null) {
                        var to = rs[0].attributes["emailaddress1"].value;
                        var toname = rs[0].attributes["bsd_name"].value;
                        sendEmailHandoverNotice(email, password, id, to, toname, bsd_name, attachment);
                    }
                    else {
                        window.top.processingDlg.hide();
                        window.top.$ui.Dialog("Message", "Email does not exist!");
                    }
                }
            });
            break;
        }
    } else {
        window.top.$ui.Dialog("Message", "You must enter email and password!");
    }

}

function sendEmailHandoverNotice(email, password, handoverNoticeId, to, toname, title, attachment) {
    var iframe = document.createElement('iframe');
    iframe.style.display = "none";
    iframe.src = "https://pssvn-preview.crm5.dynamics.com/webresources/bsd_handovernotices_report_BRV.html?type=sendemail&data=" + handoverNoticeId;
    iframe.id = "viewcontent";
    document.body.appendChild(iframe);
    var timer = setInterval(function () {
        var iframeContent = iframe.contentDocument.body.innerHTML;
        if (iframeContent != "") {
            var reportcontent = iframe.contentDocument.body.getElementsByTagName("div")["showreport"].innerHTML;
            var style = iframe.contentDocument.getElementsByTagName("style")[0].innerHTML;
            if (reportcontent != "") {

                console.log(encodeURI(reportcontent));
                clearInterval(timer);
                $('#viewcontent').remove();

                var template = '<html><head><title></title><meta charset="utf-8"><style>' + style + '</style></head><body>' + reportcontent + '</body></html>';

                ExecuteAction("", "", "bsd_Action_SendEmail", [{
                    name: 'from',
                    type: 'string',
                    value: email
                },
                {
                    name: 'fromname',
                    type: 'string',
                    value: "CLVN"
                },
                {
                    name: 'password',
                    type: 'string',
                    value: password
                },
                {
                    name: 'to',
                    type: 'string',
                    value: to
                },
                {
                    name: 'toname',
                    type: 'string',
                    value: toname
                },
                {
                    name: 'cc',
                    type: 'string',
                    value: ""
                },
                {
                    name: 'ccname',
                    type: 'string',
                    value: ""
                },
                {
                    name: 'bcc',
                    type: 'string',
                    value: ""
                },
                {
                    name: 'bccname',
                    type: 'string',
                    value: ""
                },
                {
                    name: 'title',
                    type: 'string',
                    value: title
                },
                {
                    name: 'content',
                    type: 'string',
                    value: encodeURI(template)
                },
                {
                    name: 'attachment',
                    type: 'string',
                    value: attachment
                }

                ], function (result) {

                    window.top.processingDlg.hide();
                    if (result != null) {
                        if (result.status == "error") {
                            var ss = result.data.split(':');
                            var mss = ss[ss.length - 1];
                            window.top.$ui.Dialog("Message", mss);

                        }
                        else if (result.status == "success") {
                            window.top.$ui.Dialog("Message", "Send email success!");
                            updateSendDate()
                        }
                    }
                },
                true);
            }

        }

    },
    1000);
}
function updateSendDate() {

    var id = Xrm.Page.data.entity.getId();
    var entity = Xrm.Page.data.entity.getEntityName();

    var date = new Date();
    var day = date.getDate(); // yields date
    var month = date.getMonth() + 1; // yields month (add one as '.getMonth()' is zero indexed)
    var year = date.getFullYear(); // yields year
    var hour = date.getHours(); // yields hours
    var minute = date.getMinutes(); // yields minutes
    var second = date.getSeconds(); // yields seconds
    // After this construct a string with the above results as below
    var time = year + "/" + common.numberToString(month, 2) + "/" + common.numberToString(day, 2) + " " + common.numberToString(hour, 2) + ':' + common.numberToString(minute, 2) + ':' + common.numberToString(second, 2);
    var cols = ["statuscode", "bsd_datesend"];
    var vals = ["100000003", time];
    var types = ["optionset", "datetime"];
    handovernoticesModel.updateCols(id, cols, vals, types, function (result) {
        window.top.processingDlg.hide();
        if (result != null) {
            if (result.status == "error") {
                var ss = result.data.split(':');
                var mss = ss[ss.length - 1];
                window.top.$ui.Dialog("Message", mss);

            }
            else if (result.status == "success") {
                Xrm.Page.data.refresh();
            }
        }
    })

}
function createPDFFileToSharePoint(handoverNoticeId) {
    //alert("aaaaaaaaaaaaaaa");
    var iframe = document.createElement('iframe');
    iframe.style.display = "none";
    iframe.src = "https://pssvn-preview.crm5.dynamics.com/webresources/bsd_handovernotices_report_BRV.html?type=sendemail&data=" + handoverNoticeId;
    iframe.id = "viewcontent";
    document.body.appendChild(iframe);
    var timer = setInterval(function () {
        var iframeContent = iframe.contentDocument.body.innerHTML;
        if (iframeContent != "") {
            var reportcontent = iframe.contentDocument.body.getElementsByTagName("div")["showreport"].innerHTML;
            var style = iframe.contentDocument.getElementsByTagName("style")[0].innerHTML;
            if (reportcontent != "") {

                clearInterval(timer);
                $('#viewcontent').remove();

                var template = '<html><head><title></title><meta charset="utf-8"><style>' + style + '</style></head><body>' + reportcontent + '</body></html>';
                console.log(template);
                ExecuteAction("", "", "bsd_Action_UploadFile_SharePoint", [{
                    name: 'filename',
                    type: 'string',
                    value: "testpdf.pdf"
                },
                {
                    name: 'content',
                    type: 'string',
                    value: encodeURI(template)
                }

                ], function (result) {

                    window.top.processingDlg.hide();
                    if (result != null) {
                        if (result.status == "error") {
                            var ss = result.data.split(':');
                            var mss = ss[ss.length - 1];
                            window.top.$ui.Dialog("Message", mss);

                        }
                        else if (result.status == "success") {
                            window.top.$ui.Dialog("Message", "Create file success!");
                            updateSendDate()
                        }
                    }
                },
                true);
            }

        }

    },
    1000);
}