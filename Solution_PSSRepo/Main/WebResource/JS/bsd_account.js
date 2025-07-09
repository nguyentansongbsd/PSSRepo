function onLoad() {
    crmcontrol.getRoles(function () {
        CCRManagerEdit();
    });
    if (crmcontrol.getValue("bsd_specialbuyer") == 1) {
        Xrm.Page.ui.setFormNotification("This is a special buyer.", "WARNING", "1");
        crmcontrol.alertdialogConfirm("This is a special buyer.");
    }
    else {
        Xrm.Page.ui.clearFormNotification('1');
    }
    requied_Address3();
    crmcontrol.setDisabled('bsd_specialbuyer', !(crmcontrol.checkRoles("CLVN_CCR Manager") && crmcontrol.getValue("statecode") == 0));
}
function onchangeName()
{
    debugger;
    var bsd_name = crmcontrol.getValue("bsd_name");
    Xrm.Page.getAttribute("name").setValue(bsd_name);
}
function BusinessTypeEvent() {
    debugger;
    setTimeout(function () {
        var bus = document.getElementById("bsd_businesstypesys");
        if (bus != null) {
            Xrm.Page.getAttribute("bsd_businesstypevaluename").setValue(bus);
        } else
            Xrm.Page.getAttribute("bsd_businesstypevaluename").setValue(null);
    }, 1000);

}

function CCRManagerEdit() {

    debugger;
    var roleUpdate = CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_ContactID_Update"]);
    var roleEditAccount = CheckRole(Xrm.Page.context.getUserRoles(), ["CLVN_CCR Manager", "CLVN_CCR Senior Staff", "CLVN_CCR Staff"]);
    var owner = Xrm.Page.getAttribute("ownerid").getValue()[0].id;
    var formtype = Xrm.Page.ui.getFormType();
    var uid = Xrm.Page.context.getUserId();
    var checkOptionEntry = checkOptionEntryForAccount();
    if (checkOptionEntry == false) {
        if (uid == owner || roleEditAccount) {
            disableFormFields(false);
        }
        else {
            disableFormFields(true);
        }
    }
    else {
        if (roleUpdate) {
            disableFormFields(false);
        }
        else {
            disableFormFields(true);
        }
    }
    Xrm.Page.ui.controls.get("name").setDisabled(true);
    Xrm.Page.ui.controls.get("bsd_totaltransaction").setDisabled(true);
    Xrm.Page.ui.controls.get("bsd_membershiptier").setDisabled(true);
    Xrm.Page.ui.controls.get("bsd_loyaltystatus").setDisabled(true);
    Xrm.Page.ui.controls.get("bsd_totalamountofownership").setDisabled(true);
    Xrm.Page.ui.controls.get("bsd_totalamountofownership3years").setDisabled(true);
    Xrm.Page.ui.controls.get("bsd_loyaltydate").setDisabled(true);
    Xrm.Page.ui.controls.get("address1_composite").setDisabled(true);
    Xrm.Page.ui.controls.get("bsd_addressold").setDisabled(true);
    Xrm.Page.ui.controls.get("bsd_permanentaddressold").setDisabled(true);
    //Xrm.Page.ui.controls.get("lastusedincampaign").setDisabled(true);

    //    if (formtype != 1) {
    //        allowEdit('bsd_registrationcode');
    //        allowEdit('name');
    //    }
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
function Address() {
    debugger;
    var house = Xrm.Page.getAttribute("bsd_housenumberstreet").getValue();
    var street = Xrm.Page.getAttribute("bsd_street").getValue();
    var district = Xrm.Page.getAttribute("bsd_district").getValue();
    var province = Xrm.Page.getAttribute("bsd_province").getValue();
    var Country = Xrm.Page.getAttribute("bsd_nation").getValue();
    var add = Xrm.Page.getAttribute("address1_composite");
    var cadd = Xrm.Page.getAttribute("bsd_address");
    var line = "";
    var line1 = "";
    if (house != null) {
        line += house;
    }
    if (street != null) {
        line1 += street;
    }
    //if (ward != null) {
    //    line += ward + ", ";
    //}
    if (district != null) {
        var xml = [];
        xml.push("<fetch>");
        xml.push("<entity name='new_district' >");
        xml.push("<attribute name='new_name' />");
        xml.push("<attribute name='bsd_nameen' />");
        xml.push("<filter type='and' >");
        xml.push("<condition attribute='new_districtid' operator='eq' value='" + district[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            if (rs.length > 0) {
                if (rs[0].attributes.new_name != null) {
                    line += ", " + rs[0].attributes.new_name.value;
                }
                if (rs[0].attributes.bsd_nameen != null) {
                    line1 += ", " + rs[0].attributes.bsd_nameen.value;
                }
            }
        },
            function (er) {
                console.log(er.message)
            });
    }
    if (province != null) {
        var xml = [];
        xml.push("<fetch>");
        xml.push("<entity name='new_province' >");
        xml.push("<attribute name='bsd_provincename' />");
        xml.push("<attribute name='bsd_nameen' />");
        xml.push("<filter type='and' >");
        xml.push("<condition attribute='new_provinceid' operator='eq' value='" + province[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            if (rs.length > 0) {
                if (rs[0].attributes.bsd_provincename != null) {
                    line += ", " + rs[0].attributes.bsd_provincename.value;
                }
                if (rs[0].attributes.bsd_nameen != null) {
                    line1 += ", " + rs[0].attributes.bsd_nameen.value;
                }
            }
        },
            function (er) {
                console.log(er.message)
            });
    }

    if (Country != null) {
        var xml = [];
        xml.push("<fetch>");
        xml.push("<entity name='bsd_country' >");
        xml.push("<attribute name='bsd_nameen' />");
        xml.push("<attribute name='bsd_countryname' />");
        xml.push("<filter type='and' >");
        xml.push("<condition attribute='bsd_countryid' operator='eq' value='" + Country[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            if (rs.length > 0) {
                if (rs[0].attributes.bsd_countryname != null) {
                    line += ", " + rs[0].attributes.bsd_countryname.value;
                }
                if (rs[0].attributes.bsd_nameen != null) {
                    line1 += ", " + rs[0].attributes.bsd_nameen.value;
                }
            }
        },
            function (er) {
                console.log(er.message)
            });
    }

    if (house == null) {
        if (line.length > 0) {
            line = line.substring(2, line.length);
        }
    }
    if (street == null) {
        if (line1.length > 0) {
            line1 = line1.substring(2, line1.length);
        }
    }
    //ca = line;
    //if (p != "")
    //    ca += ", " + p;
    //if (c != "")
    //    ca += ", " + c;
    cadd.setValue(line);
    Xrm.Page.getAttribute("bsd_diachi").setValue(line1);
    requied_Address3();
}
function requied_Address3() {
    var Country = Xrm.Page.getAttribute("bsd_nation").getValue();
    //if (Country != null) {
    //    var fetchXml = [
    //        "<fetch>",
    //        "  <entity name='bsd_country'>",
    //        "   <attribute name='bsd_id' />",
    //        "    <filter>",
    //        "      <condition attribute='bsd_countryid' operator='eq' value='", Country[0].id, "'/>",
    //        "      <condition attribute='bsd_id' operator='eq' value='VN'/>",
    //        "    </filter>",
    //        "  </entity>",
    //        "</fetch>",
    //    ].join("");
    //    var country = CrmFetchKitNew.FetchSync(fetchXml);
    //    //if (country.length > 0) {
    //    //    crmcontrol.setRequired("bsd_province", "required");
    //    //} else crmcontrol.setRequired("bsd_province", "none");
    //} 
    //else crmcontrol.setRequired("bsd_province", "none");
    crmcontrol.setRequired("bsd_nation", "required");
}
function onRegistrationCodeChange() {
    debugger;
    var status = Xrm.Page.getAttribute("statecode").getValue();
    var registrationCode = Xrm.Page.getAttribute("bsd_registrationcode").getValue();
    if (registrationCode != null) {
        registrationCode = replaceSpace(registrationCode);
        Xrm.Page.getAttribute("bsd_registrationcode").setValue(registrationCode);
        var accountId = Xrm.Page.data.entity.getId();
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='account'>");
        xml.push("  <attribute name='primarycontactid' />");
        xml.push("  <attribute name='telephone1' />");
        xml.push("  <attribute name='bsd_rocnumber2' />");
        xml.push("  <attribute name='bsd_rocnumber1' />");
        xml.push("  <attribute name='websiteurl' />");
        xml.push("  <attribute name='bsd_vatregistrationnumber' />");
        xml.push("  <attribute name='bsd_incorporatedate' />");
        xml.push("  <attribute name='bsd_hotlines' />");
        xml.push("  <attribute name='bsd_generalledgercompanynumber' />");
        xml.push("  <attribute name='fax' />");
        xml.push("  <attribute name='emailaddress1' />");
        xml.push("  <attribute name='bsd_groupgstregisttationnumber' />");
        xml.push("  <attribute name='statuscode' />");
        xml.push("  <attribute name='ownerid' />");
        xml.push("  <attribute name='createdon' />");
        xml.push("  <attribute name='address1_composite' />");
        xml.push("  <attribute name='bsd_companycode' />");
        xml.push("  <attribute name='bsd_registrationcode' />");
        xml.push("  <attribute name='bsd_accountnameother' />");
        xml.push("  <attribute name='bsd_name' />");
        xml.push("  <attribute name='name' />");
        xml.push("  <attribute name='accountid' />");
        xml.push("  <order attribute='createdon' descending='true' />");
        xml.push("  <filter type='and'>");
        xml.push("  <condition attribute='statecode' operator='eq' value='0' />");
        //xml.push("     <condition attribute='accountid' operator='ne' uitype='account' value='" + accountId + "' />");
        //xml.push("    <condition attribute='bsd_registrationcode' operator='eq' value='" + registrationCode + "' />");
        xml.push("  </filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            if (rs.length > 0) {
                debugger;
                if (status == 0) {
                    var r = registrationCode.replace(/\s/g, "");
                    registrationCode = r;
                    for (var i = 0; i < rs.length; i++) {
                        var bsd_registrationcode = rs[i].attributes.bsd_registrationcode ? rs[i].attributes.bsd_registrationcode.value : '';
                        bsd_registrationcode = bsd_registrationcode.replace(/\s/g, "");
                        if (registrationCode == bsd_registrationcode) {
                            window.top.$ui.Dialog("Message", "You have entered a Registration Code that already exists. Only unique Registration Code is allowed.");
                            Xrm.Page.getAttribute("bsd_registrationcode").setValue(null);

                        }
                    }
                }
            }
        })
    }
}

function replaceSpace(str) {
    if (str.indexOf("  ") != -1) {
        var s = str.replace("  ", " ");
        return replaceSpace(s);
    }
    return str;
}

function checkOptionEntryForAccount() {
    var check = false;
    var idAccount = crmcontrol.getId();
    var fetchData = {
        accountid: idAccount
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='account'>",
        "    <all-attributes />",
        "    <filter>",
        "      <condition attribute='accountid' operator='eq' value='", fetchData.accountid/*{08AA729B-F636-E811-8133-3863BB360C48}*/, "'/>",
        "    </filter>",
        "    <link-entity name='salesorder' from='customerid' to='accountid' link-type='inner'>",
        "      <filter type='and'>",
        "        <condition attribute='bsd_signedcontractdate' operator='not-null' />",
        "        <condition attribute='statuscode' operator='neq' value='", 100000006, "'/>",
        "      </filter>",
        "    </link-entity>",
        "  </entity>",
        "</fetch>",
    ].join("");
    var entities = CrmFetchKitNew.FetchSync(fetchXml);
    if (entities.length > 0) {
        check = true;
    }
    return check;
}
function disableFormFields(onOff) {
    debugger;
    function doesControlHaveAttribute(control) {
        var controlType = control.getControlType();
        return controlType != "iframe" && controlType != "webresource" && controlType != "subgrid";
    }
    Xrm.Page.ui.controls.forEach(function (control, index) {
        if (doesControlHaveAttribute(control)) {
            if (control._controlName != "header_ownerid") {
                control.setDisabled(onOff);
            }

        }
    });
}