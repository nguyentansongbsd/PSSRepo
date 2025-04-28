// JavaScript source code
function onload(executionContextObj) {
    filter_OE(executionContextObj);
    filter_quotation(executionContextObj);
    filter_quote(executionContextObj);
    var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
    if (statuscode == 100000000 || statuscode == 100000001) {
        disableFormFields(true);
    }
    else {
        //disableFormFields(false);
    }
    var installmentnumber = Xrm.Page.getAttribute("bsd_installmentnumber").getValue();
    if (installmentnumber != null && statuscode == 1) {
        Xrm.Page.getControl("bsd_installment").setDisabled(true);
    }
    else if (installmentnumber == null && statuscode == 1) {
        Xrm.Page.getControl("bsd_installment").setDisabled(false);
    }
}
function lock_ins() {
    var optionentry = Xrm.Page.getAttribute("bsd_optionentry").getValue();

    var installmentnumber = Xrm.Page.getAttribute("bsd_installmentnumber").getValue();
    if (optionentry != null) {
        if (installmentnumber != null) {
            Xrm.Page.getControl("bsd_installment").setDisabled(true);
            fillter_ins();
        }
        else {
            Xrm.Page.getControl("bsd_installment").setDisabled(false);
            Xrm.Page.getAttribute("bsd_installment").setValue(null);
            Xrm.Page.getAttribute("bsd_duedateold").setValue(null);
        }
    }
    else {
        if (installmentnumber != null) {
            Xrm.Page.getControl("bsd_installment").setDisabled(true);
            fillter_ins_quote();
        }
        else {
            Xrm.Page.getControl("bsd_installment").setDisabled(false);
            Xrm.Page.getAttribute("bsd_installment").setValue(null);
            Xrm.Page.getAttribute("bsd_duedateold").setValue(null);
        }
    }

}
function mapInstallment() {

    var bsd_installment = Xrm.Page.getAttribute("bsd_installment").getValue();
    if (bsd_installment == null) return;
    var xml = [];
    xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
    xml.push("<entity name='bsd_paymentschemedetail'>");
    xml.push("<attribute name='bsd_paymentschemedetailid' />");
    xml.push("<attribute name='bsd_name' />");
    xml.push("<attribute name='createdon' />");
    xml.push("<attribute name='bsd_ordernumber' />");
    xml.push("<order attribute='bsd_name' descending='false' />");
    xml.push("<filter type='and'>");
    xml.push("<condition attribute='bsd_paymentschemedetailid' operator='eq' value='" + bsd_installment[0].id + "'/>");
    xml.push("</filter>");
    xml.push("</entity>");
    xml.push("</fetch>");
    CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
        debugger;
        if (rs.length > 0) {
           
            var installmentnumber = Xrm.Page.getAttribute("bsd_installmentnumber").getValue();
            if (installmentnumber == null || installmentnumber != rs[0].attributes.bsd_ordernumber.value) {
                Xrm.Page.getAttribute("bsd_installmentnumber").setValue(rs[0].attributes.bsd_ordernumber.value);
            }
        }
        else {
        }
    },
        function (err) {
            console.log(err);
        });
}
function onchange_Project() {
    Xrm.Page.getAttribute("bsd_installmentnumber").setValue(null);
    Xrm.Page.getAttribute("bsd_installment").setValue(null);
    Xrm.Page.getAttribute("bsd_duedateold").setValue(null);
    Xrm.Page.getAttribute("bsd_units").setValue(null);
    Xrm.Page.getAttribute("bsd_optionentry").setValue(null);
}
function fillter_pro_unit() {
    debugger;
    var optionentry = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    var units = null;
    var project = null;
    if (optionentry != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='salesorder'>");
        xml.push("<attribute name='bsd_project' />");
        xml.push("<attribute name='bsd_unitnumber' />");
        xml.push("<attribute name='salesorderid' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='salesorderid' operator='eq'  uitype='salesorder' value='" + optionentry[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0 && rs[0].attributes.bsd_unitnumber) {
                units = new Array();
                units[0] = new Object();
                units[0].id = rs[0].attributes.bsd_unitnumber.guid;
                units[0].entityType = rs[0].attributes.bsd_unitnumber.logicalName;
                units[0].name = rs[0].attributes.bsd_unitnumber.name;
                project = new Array();
                project[0] = new Object();
                project[0].id = rs[0].attributes.bsd_project.guid;
                project[0].entityType = rs[0].attributes.bsd_project.logicalName;
                project[0].name = rs[0].attributes.bsd_project.name;
                Xrm.Page.getAttribute("bsd_units").setValue(units);
            }
        },
            function (err) {
                console.log(err);
            });
    }

    //Xrm.Page.getAttribute("bsd_project").setValue(project);
    if (optionentry == null) {
        Xrm.Page.getAttribute("bsd_installmentnumber").setValue(null);
        Xrm.Page.getAttribute("bsd_installment").setValue(null);
        Xrm.Page.getAttribute("bsd_duedateold").setValue(null);
        Xrm.Page.getAttribute("bsd_units").setValue(null);
        lock_ins();
    }
}
function fillter_ins() {
    debugger;
    var ins = null;
    var optionentry = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    var installmentnumber = Xrm.Page.getAttribute("bsd_installmentnumber").getValue();
    if (optionentry != null && installmentnumber != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_paymentschemedetail'>");
        xml.push("<attribute name='bsd_paymentschemedetailid' />");
        xml.push("<attribute name='bsd_name' />");
        xml.push("<attribute name='createdon' />");
        xml.push("<order attribute='bsd_name' descending='false' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_ordernumber' operator='eq' value='" + installmentnumber + "'/>");
        xml.push("<condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + optionentry[0].id + "' />");

        xml.push("<condition attribute='bsd_lastinstallment' operator='eq' value='" + 0 + "'/>");
        xml.push("<condition attribute='bsd_duedatecalculatingmethod' operator='ne' uitype='salesorder' value='" + 100000002 + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0) {
                Xrm.Page.getAttribute("bsd_installment").setValue([{
                    id: rs[0].attributes.bsd_paymentschemedetailid.value,
                    name: rs[0].attributes.bsd_name.value,
                    entityType: "bsd_paymentschemedetail"
                }]);
               
                fillter_olddate();
            }
            else {
                alert("No Installment" + installmentnumber + " in optionEntry");

                Xrm.Page.getAttribute("bsd_installmentnumber").setValue(null);
            }
        },
            function (err) {
                console.log(err);
            });
    }

}
function fillter_olddate() {
    var installment = Xrm.Page.getAttribute("bsd_installment").getValue();
    //var optionentry = Xrm.Page.getAttribute("bsd_optionentry").getValue();

    if (installment != null) {

        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_paymentschemedetail'>");
        xml.push("<attribute name='bsd_paymentschemedetailid' />");
        xml.push("<attribute name='bsd_duedate' />");
        xml.push("<attribute name='createdon' />");
        xml.push("<order attribute='bsd_name' descending='false' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_paymentschemedetailid' operator='eq' value='" + installment[0].id + "'/>");
        //xml.push("<condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + optionentry[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0 && rs[0].attributes.bsd_duedate) {
                Xrm.Page.getAttribute("bsd_duedateold").setValue(rs[0].attributes.bsd_duedate.value);
            }
            else {
                Xrm.Page.getAttribute("bsd_duedateold").setValue(null);
            }
        },
            function (err) {
                console.log(err);
            });
    }
}
function dislay_ins() {
    var optionEntry = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    if (optionEntry != null) {
        var xml = [];
        var viewId = "{90C27BE1-B614-435B-B655-53065CDED0F1}";
        var entityName = "bsd_paymentschemedetail";
        var viewDisplayName = "Payment Scheme Datail Lookup View";
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_paymentschemedetail'>");
        xml.push("<attribute name='bsd_paymentschemedetailid' />");
        xml.push("<attribute name='bsd_name' />");
        xml.push("<attribute name='createdon' />");
        xml.push("<order attribute='bsd_name' descending='false' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_optionentry' operator='eq' uitype='salesorder' value='" + optionEntry[0].id + "' />");

        xml.push("<condition attribute='bsd_lastinstallment' operator='eq' value='" + 0 + "'/>");
        xml.push("<condition attribute='bsd_duedatecalculatingmethod' operator='ne' uitype='salesorder' value='" + 100000002 + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        var layoutXml = "<grid name='resultset' " + "object='1' " + "jump='bsd_paymentschemedetailid'  " + "select='1'  " + "icon='1'  " + "preview='1'>  " + "<row name='result'  " + "id='bsd_paymentschemedetailid'>  " + "<cell name='bsd_name'   " + "width='200' />  " + "<cell name='createdon' " + "width='200' />  " + "</row>   " + "</grid>   ";
        Xrm.Page.getControl("bsd_installment").addCustomView(viewId, entityName, viewDisplayName, xml.join(""), layoutXml, true);
    }
}
function dislay_unit() {
    var option = Xrm.Page.getAttribute("bsd_optionentry").getValue();
    var viewId = "{8BA625B2-6A2A-4735-BAB2-0C74AE8442A4}";
    var entityName = "product";
    var viewDisplayName = "Product Lookup View";
    var units = null;
    if (option) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='salesorder'>");
        xml.push("<attribute name='bsd_project' />");
        xml.push("<attribute name='bsd_unitnumber' />");
        xml.push("<attribute name='salesorderid' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='salesorderid' operator='eq'  uitype='salesorder' value='" + option[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0 && rs[0].attributes.bsd_unitnumber) {
                units = new Array();
                units[0] = new Object();
                units[0].id = rs[0].attributes.bsd_unitnumber.guid;
                units[0].entityType = rs[0].attributes.bsd_unitnumber.logicalName;
                units[0].name = rs[0].attributes.bsd_unitnumber.name;
            }
        },
            function (err) {
                console.log(err);
            });
    }
    if (units != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='product'>");
        xml.push("<attribute name='productid'/>");
        xml.push("<attribute name='productnumber'/>");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='productid' operator='eq'  uitype='product' value='" + units[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        var layoutXml = "<grid name='resultset' " + "object='1' " + "jump='productid'  " + "select='1'  " + "icon='1'  " + "preview='1'>  " + "<row name='result'  " + "id='productid'>  " + "<cell name='bsd_name'   " + "width='200' />  " + "<cell name='createdon' " + "width='200' />  " + "</row>   " + "</grid>   ";

        Xrm.Page.getControl("bsd_units").addCustomView(viewId, entityName, viewDisplayName, xml.join(""), layoutXml, true);
    }

}

function filter_OE(executionContextObj) {
    var formContext = executionContextObj.getFormContext();
    formContext.getControl("bsd_optionentry").addPreSearch(FilterOE);
}
function FilterOE(executionContextObj) {
    var bsd_project = crmcontrol.getValue("bsd_project");
    var Filteroe = "";
    Filteroe += "<filter>";
    Filteroe += "<condition attribute='statuscode' operator='neq' value='100000006'/>";
    if (bsd_project != null) {
        Filteroe += "<condition attribute='bsd_project' operator='eq' value='" + bsd_project[0].id + "'/>";
    }
    Filteroe += "</filter>";
    var customerAccountFilter = Filteroe;
    var formContext = executionContextObj.getFormContext();
    formContext.getControl("bsd_optionentry").addCustomFilter(customerAccountFilter);
}

//region
function fillter_pro_unit_quote() {
    debugger;
    var bsd_quote = Xrm.Page.getAttribute("bsd_quote").getValue();
    var units = null;
    var project = null;
    if (bsd_quote != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='quote'>");
        xml.push("<attribute name='bsd_projectid' />");
        xml.push("<attribute name='bsd_unitno' />");
        xml.push("<attribute name='quoteid' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='quoteid' operator='eq'  uitype='quote' value='" + bsd_quote[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0 && rs[0].attributes.bsd_unitno) {
                units = new Array();
                units[0] = new Object();
                units[0].id = rs[0].attributes.bsd_unitno.guid;
                units[0].entityType = rs[0].attributes.bsd_unitno.logicalName;
                units[0].name = rs[0].attributes.bsd_unitno.name;
                project = new Array();
                project[0] = new Object();
                project[0].id = rs[0].attributes.bsd_projectid.guid;
                project[0].entityType = rs[0].attributes.bsd_projectid.logicalName;
                project[0].name = rs[0].attributes.bsd_projectid.name;
                Xrm.Page.getAttribute("bsd_units").setValue(units);
            }
        },
            function (err) {
                console.log(err);
            });
    }

    //Xrm.Page.getAttribute("bsd_project").setValue(project);
    if (bsd_quote == null) {
        Xrm.Page.getAttribute("bsd_installmentnumber").setValue(null);
        Xrm.Page.getAttribute("bsd_installment").setValue(null);
        Xrm.Page.getAttribute("bsd_duedateold").setValue(null);
        Xrm.Page.getAttribute("bsd_units").setValue(null);
        lock_ins();
    }
}
function fillter_ins_quote() {
    debugger;
    var ins = null;
    var bsd_quote = Xrm.Page.getAttribute("bsd_quote").getValue();
    var installmentnumber = Xrm.Page.getAttribute("bsd_installmentnumber").getValue();
    if (bsd_quote != null && installmentnumber != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_paymentschemedetail'>");
        xml.push("<attribute name='bsd_paymentschemedetailid' />");
        xml.push("<attribute name='bsd_name' />");
        xml.push("<attribute name='createdon' />");
        xml.push("<order attribute='bsd_name' descending='false' />");
        xml.push("<filter type='and'>");

        xml.push("<condition attribute='bsd_lastinstallment' operator='eq' value='" + 0 + "'/>");
        xml.push("<condition attribute='bsd_duedatecalculatingmethod' operator='ne' uitype='salesorder' value='" + 100000002 + "' />");
        xml.push("<condition attribute='bsd_ordernumber' operator='eq' value='" + installmentnumber + "'/>");
        xml.push("<condition attribute='bsd_reservation' operator='eq' uitype='quote' value='" + bsd_quote[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0) {
                Xrm.Page.getAttribute("bsd_installment").setValue([{
                    id: rs[0].attributes.bsd_paymentschemedetailid.value,
                    name: rs[0].attributes.bsd_name.value,
                    entityType: "bsd_paymentschemedetail"
                }]);
               
                fillter_olddate();
            }
            else {
                alert("No Installment" + installmentnumber + " in optionEntry");
                Xrm.Page.getAttribute("bsd_installmentnumber").setValue(null);
            }
        },
            function (err) {
                console.log(err);
            });
    }

}
function dislay_ins_quote() {
    var bsd_quote = Xrm.Page.getAttribute("bsd_quote").getValue();
    if (bsd_quote != null) {
        var xml = [];
        var viewId = "{90C27BE1-B614-435B-B655-53065CDED0F1}";
        var entityName = "bsd_paymentschemedetail";
        var viewDisplayName = "Payment Scheme Datail Lookup View";
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_paymentschemedetail'>");
        xml.push("<attribute name='bsd_paymentschemedetailid' />");
        xml.push("<attribute name='bsd_name' />");
        xml.push("<attribute name='createdon' />");
        xml.push("<order attribute='bsd_name' descending='false' />");
        xml.push("<filter type='and'>");

        xml.push("<condition attribute='bsd_lastinstallment' operator='eq' value='" + 0 + "'/>");
        xml.push("<condition attribute='bsd_duedatecalculatingmethod' operator='ne' uitype='salesorder' value='" + 100000002 + "' />");
        xml.push("<condition attribute='bsd_reservation' operator='eq' uitype='quote' value='" + bsd_quote[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        var layoutXml = "<grid name='resultset' " + "object='1' " + "jump='bsd_paymentschemedetailid'  " + "select='1'  " + "icon='1'  " + "preview='1'>  " + "<row name='result'  " + "id='bsd_paymentschemedetailid'>  " + "<cell name='bsd_name'   " + "width='200' />  " + "<cell name='createdon' " + "width='200' />  " + "</row>   " + "</grid>   ";
        Xrm.Page.getControl("bsd_installment").addCustomView(viewId, entityName, viewDisplayName, xml.join(""), layoutXml, true);
    }
}
function dislay_unit_quote() {
    var bsd_quote = Xrm.Page.getAttribute("bsd_quote").getValue();
    var viewId = "{8BA625B2-6A2A-4735-BAB2-0C74AE8442A4}";
    var entityName = "product";
    var viewDisplayName = "Product Lookup View";
    var units = null;
    if (bsd_quote) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='quote'>");
        xml.push("<attribute name='bsd_projectid' />");
        xml.push("<attribute name='bsd_unitno' />");
        xml.push("<attribute name='quoteid' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='quoteid' operator='eq'  uitype='quote' value='" + bsd_quote[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0 && rs[0].attributes.bsd_unitno) {
                units = new Array();
                units[0] = new Object();
                units[0].id = rs[0].attributes.bsd_unitno.guid;
                units[0].entityType = rs[0].attributes.bsd_unitno.logicalName;
                units[0].name = rs[0].attributes.bsd_unitno.name;
            }
        },
            function (err) {
                console.log(err);
            });
    }
    if (units != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='product'>");
        xml.push("<attribute name='productid'/>");
        xml.push("<attribute name='productnumber'/>");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='productid' operator='eq'  uitype='product' value='" + units[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        var layoutXml = "<grid name='resultset' " + "object='1' " + "jump='productid'  " + "select='1'  " + "icon='1'  " + "preview='1'>  " + "<row name='result'  " + "id='productid'>  " + "<cell name='bsd_name'   " + "width='200' />  " + "<cell name='createdon' " + "width='200' />  " + "</row>   " + "</grid>   ";

        Xrm.Page.getControl("bsd_units").addCustomView(viewId, entityName, viewDisplayName, xml.join(""), layoutXml, true);
    }

}

function filter_quote(executionContextObj) {
    var formContext = executionContextObj.getFormContext();
    formContext.getControl("bsd_quote").addPreSearch(Filter_quote);
}
function Filter_quote(executionContextObj) {
    var bsd_project = crmcontrol.getValue("bsd_project");
    var Filteroe = "";
    Filteroe += "<filter>";
    //Filteroe += "<condition attribute='statuscode' operator='neq' value='100000006'/>";
    if (bsd_project != null) {
        Filteroe += "<condition attribute='bsd_projectid' operator='eq' value='" + bsd_project[0].id + "'/>";
        Filteroe += "      <condition attribute='statuscode' operator='in'>" + "        <value>" + 100000004 + "</value>" + "        <value>" + 100000007 + "</value>" + "        <value>" + 100000000 + "</value>" + "        <value>" + 100000006 + "</value>" + "      </condition>";
    }
    Filteroe += "</filter>";
    var customerAccountFilter = Filteroe;
    var formContext = executionContextObj.getFormContext();
    formContext.getControl("bsd_quote").addCustomFilter(customerAccountFilter);
}
//endregion

//region
function fillter_pro_unit_quotation() {
    debugger;
    var bsd_quotation = Xrm.Page.getAttribute("bsd_quotation").getValue();
    var units = null;
    var project = null;
    if (bsd_quotation != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='bsd_quotation'>");
        xml.push("<attribute name='bsd_project' />");
        xml.push("<attribute name='bsd_units' />");
        xml.push("<attribute name='bsd_quotation' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_quotationid' operator='eq'  uitype='bsd_quotation' value='" + bsd_quotation[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0 && rs[0].attributes.bsd_units) {
                units = new Array();
                units[0] = new Object();
                units[0].id = rs[0].attributes.bsd_units.guid;
                units[0].entityType = rs[0].attributes.bsd_units.logicalName;
                units[0].name = rs[0].attributes.bsd_units.name;
                project = new Array();
                project[0] = new Object();
                project[0].id = rs[0].attributes.bsd_project.guid;
                project[0].entityType = rs[0].attributes.bsd_project.logicalName;
                project[0].name = rs[0].attributes.bsd_project.name;
                Xrm.Page.getAttribute("bsd_units").setValue(units);
            }
        },
            function (err) {
                console.log(err);
            });
    }

    //Xrm.Page.getAttribute("bsd_project").setValue(project);
    if (bsd_quotation == null) {
        Xrm.Page.getAttribute("bsd_installmentnumber").setValue(null);
        Xrm.Page.getAttribute("bsd_installment").setValue(null);
        Xrm.Page.getAttribute("bsd_duedateold").setValue(null);
        Xrm.Page.getAttribute("bsd_units").setValue(null);
        lock_ins();
    }
}
function fillter_ins_quotation() {
    debugger;
    var ins = null;
    var bsd_quotation = Xrm.Page.getAttribute("bsd_quotation").getValue();
    var installmentnumber = Xrm.Page.getAttribute("bsd_installmentnumber").getValue();
    if (bsd_quotation != null && installmentnumber != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_paymentschemedetail'>");
        xml.push("<attribute name='bsd_paymentschemedetailid' />");
        xml.push("<attribute name='bsd_name' />");
        xml.push("<attribute name='createdon' />");

        xml.push("<attribute name='bsd_ordernumber' />");
        xml.push("<order attribute='bsd_name' descending='false' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_ordernumber' operator='eq' value='" + installmentnumber + "'/>");
        xml.push("<condition attribute='bsd_quotation' operator='eq' uitype='bsd_quotation' value='" + bsd_quotation[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0) {
                Xrm.Page.getAttribute("bsd_installment").setValue([{
                    id: rs[0].attributes.bsd_paymentschemedetailid.value,
                    name: rs[0].attributes.bsd_name.value,
                    entityType: "bsd_paymentschemedetail"
                }]);
                var installmentnumber = Xrm.Page.getAttribute("bsd_installmentnumber").getValue();
                if (installmentnumber == null || installmentnumber != rs[0].attributes.bsd_ordernumber.value) {
                    Xrm.Page.getAttribute("bsd_installmentnumber").setValue(rs[0].attributes.bsd_ordernumber.value);
                }
                fillter_olddate();
            }
            else {
                alert("No Installment" + installmentnumber + " in Quote");
            }
        },
            function (err) {
                console.log(err);
            });
    }

}
function dislay_ins_quotation() {
    var bsd_quotation = Xrm.Page.getAttribute("bsd_quotation").getValue();
    if (bsd_quotation != null) {
        var xml = [];
        var viewId = "{90C27BE1-B614-435B-B655-53065CDED0F1}";
        var entityName = "bsd_paymentschemedetail";
        var viewDisplayName = "Payment Scheme Datail Lookup View";
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_paymentschemedetail'>");
        xml.push("<attribute name='bsd_paymentschemedetailid' />");
        xml.push("<attribute name='bsd_name' />");
        xml.push("<attribute name='createdon' />");
        xml.push("<order attribute='bsd_name' descending='false' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_quotation' operator='eq' uitype='bsd_quotation' value='" + bsd_quotation[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        var layoutXml = "<grid name='resultset' " + "object='1' " + "jump='bsd_paymentschemedetailid'  " + "select='1'  " + "icon='1'  " + "preview='1'>  " + "<row name='result'  " + "id='bsd_paymentschemedetailid'>  " + "<cell name='bsd_name'   " + "width='200' />  " + "<cell name='createdon' " + "width='200' />  " + "</row>   " + "</grid>   ";
        Xrm.Page.getControl("bsd_installment").addCustomView(viewId, entityName, viewDisplayName, xml.join(""), layoutXml, true);
    }
}
function dislay_unit_quotation() {
    var bsd_quotation = Xrm.Page.getAttribute("bsd_quotation").getValue();
    var viewId = "{8BA625B2-6A2A-4735-BAB2-0C74AE8442A4}";
    var entityName = "product";
    var viewDisplayName = "Product Lookup View";
    var units = null;
    if (bsd_quotation) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='bsd_quotation'>");
        xml.push("<attribute name='bsd_project' />");
        xml.push("<attribute name='bsd_units' />");
        xml.push("<attribute name='bsd_quotationid' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_quotationid' operator='eq'  uitype='bsd_quotation' value='" + bsd_quotation[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0 && rs[0].attributes.bsd_units) {
                units = new Array();
                units[0] = new Object();
                units[0].id = rs[0].attributes.bsd_units.guid;
                units[0].entityType = rs[0].attributes.bsd_units.logicalName;
                units[0].name = rs[0].attributes.bsd_units.name;
            }
        },
            function (err) {
                console.log(err);
            });
    }
    if (units != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' count='1' distinct='false'>");
        xml.push("<entity name='product'>");
        xml.push("<attribute name='productid'/>");
        xml.push("<attribute name='productnumber'/>");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='productid' operator='eq'  uitype='product' value='" + units[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        var layoutXml = "<grid name='resultset' " + "object='1' " + "jump='productid'  " + "select='1'  " + "icon='1'  " + "preview='1'>  " + "<row name='result'  " + "id='productid'>  " + "<cell name='bsd_name'   " + "width='200' />  " + "<cell name='createdon' " + "width='200' />  " + "</row>   " + "</grid>   ";

        Xrm.Page.getControl("bsd_units").addCustomView(viewId, entityName, viewDisplayName, xml.join(""), layoutXml, true);
    }

}

function filter_quotation(executionContextObj) {
    var formContext = executionContextObj.getFormContext();
    formContext.getControl("bsd_quotation").addPreSearch(Filter_quotation);
}
function Filter_quotation(executionContextObj) {
    var bsd_project = crmcontrol.getValue("bsd_project");
    var Filteroe = "";
    Filteroe += "<filter>";
    //Filteroe += "<condition attribute='statuscode' operator='neq' value='100000006'/>";
    if (bsd_project != null) {
        Filteroe += "<condition attribute='bsd_project' operator='eq' value='" + bsd_project[0].id + "'/>";
    }
    Filteroe += "</filter>";
    var customerAccountFilter = Filteroe;
    var formContext = executionContextObj.getFormContext();
    formContext.getControl("bsd_quotation").addCustomFilter(customerAccountFilter);
}
//endregion
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