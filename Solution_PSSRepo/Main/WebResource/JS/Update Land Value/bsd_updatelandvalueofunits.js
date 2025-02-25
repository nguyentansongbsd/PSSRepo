function OnLoad() {
    blockAllFieldsApprove()
    onchange_type();
    filter_ps();
}

function blockAllFieldsApprove() {
    var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
    if (statuscode == 100000002) {
        crmcontrol.disabledForm(true);
    }
}
function onChangeUnit() {
    fillter_OE();
}
function fillter_OE() {
    debugger;
    var ins = null;
    var bsd_units = Xrm.Page.getAttribute("bsd_units").getValue();
    if (bsd_units != null) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>");
        xml.push("<entity name='salesorder'>");
        xml.push("<attribute name='salesorderid' />");
        xml.push("<attribute name='name' />");
        xml.push("<attribute name='createdon' />");
        xml.push("<order attribute='name' descending='false' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_unitnumber' operator='eq' uitype='bsd_unitnumber' value='" + bsd_units[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0) {
                Xrm.Page.getAttribute("bsd_optionentry").setValue([{
                    id: rs[0].attributes.salesorderid.value,
                    name: rs[0].attributes.name.value,
                    entityType: "salesorder"
                }]);
                onchange_OE()
            }
        },
            function (err) {
                console.log(err);
            });
    }

}

function onchange_type() {
    var type = crmcontrol.getValue("bsd_type");
    if (type) {
        if (type == 100000000) {
            crmcontrol.setVisible("bsd_optionentry", true);
            crmcontrol.setVisible("bsd_installment", true);
            crmcontrol.setVisible("bsd_valuedifference", true);
            crmcontrol.setVisible("bsd_amountofthisphasecurrent", true);
            crmcontrol.setVisible("bsd_amountofthisphase", true);
            crmcontrol.setVisibleSection("tab_general", "section_5", true);
            crmcontrol.setVisibleSection("tab_general", "section_6", true);
            crmcontrol.setRequired("bsd_optionentry", 'required');

        } else {
            crmcontrol.setVisible("bsd_valuedifference", false);
            crmcontrol.setVisible("bsd_optionentry", false);
            crmcontrol.setVisible("bsd_installment", false);
            crmcontrol.setVisible("bsd_amountofthisphase", false);
            crmcontrol.setVisible("bsd_amountofthisphasecurrent", false);
            crmcontrol.setVisibleSection("tab_general", "section_5", false);
            crmcontrol.setVisibleSection("tab_general", "section_6", false);
            crmcontrol.setRequired("bsd_optionentry", 'none');
        }
    }

}

function onchange_OE() {
    debugger;
    var OE = crmcontrol.getValue("bsd_optionentry");
    var bsd_installment = crmcontrol.getValue("bsd_installment");

    filter_ps();
    load_OE();
    if (OE) {
        var fetchXml = ["<fetch>", "  <entity name='bsd_paymentschemedetail'>", "    <filter type='and'>", "      <condition attribute='bsd_optionentry' operator='eq' value='", OE[0].id, "'/>", "      <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002'/>", "    </filter>", "  </entity>", "</fetch>",].join("");
        var rs = CrmFetchKitNew.FetchSync(fetchXml);
        if (rs.length > 0) {
            var tmp = rs[0].attributes;
            Xrm.Page.getAttribute("bsd_installment").setValue([{
                id: rs[0].attributes["bsd_paymentschemedetailid"].value,
                name: rs[0].attributes["bsd_name"].value,
                entityType: "bsd_paymentschemedetail"
            }]);
            var tmp = rs[0].attributes;
            var bsd_amountofthisphase = tmp.bsd_amountofthisphase != null ? tmp.bsd_amountofthisphase.value : 0;
            crmcontrol.setValue("bsd_amountofthisphasecurrent", bsd_amountofthisphase);
        }
    } else {
        crmcontrol.setValue("bsd_installment", null);
        crmcontrol.setValue("bsd_amountofthisphasecurrent", null);
    }
}

function onchange_unit() {
    var unit = crmcontrol.getValue("bsd_units");
    if (!unit) {
        crmcontrol.setValue("bsd_optionentry", null);
    }

}

function onchange_ps() {
    var ps = crmcontrol.getValue("bsd_installment");
    if (ps) {
        var fetchXml = ["<fetch>", "  <entity name='bsd_paymentschemedetail'>", "    <attribute name='bsd_amountofthisphase' />", "    <filter>", "      <condition attribute='bsd_paymentschemedetailid' operator='eq' value='", ps[0].id, "'/>", "    </filter>", "  </entity>", "</fetch>",].join("");
        var rs = CrmFetchKitNew.FetchSync(fetchXml);
        if (rs.length > 0) {
            var tmp = rs[0].attributes;
            var bsd_amountofthisphase = tmp.bsd_amountofthisphase != null ? tmp.bsd_amountofthisphase.value : 0;
            crmcontrol.setValue("bsd_amountofthisphasecurrent", bsd_amountofthisphase);
        }
    } else crmcontrol.setValue("bsd_amountofthisphasecurrent", null);

    filter_ps();
}

function load_OE() {
    var OE = crmcontrol.getValue("bsd_optionentry");
    if (OE) {
        var fetchXml = ["<fetch>", "  <entity name='salesorder'>", "    <attribute name='bsd_detailamount' />", "    <attribute name='bsd_discount' />", "    <attribute name='bsd_packagesellingamount' />", "    <attribute name='bsd_totalamountlessfreight' />", "    <attribute name='bsd_landvaluededuction' />", "    <attribute name='totaltax' />", "    <attribute name='bsd_freightamount' />", "    <attribute name='totalamount' />", "    <filter>", "      <condition attribute='salesorderid' operator='eq' value='", OE[0].id, "'/>", "    </filter>", "  </entity>", "</fetch>",].join("");
        var rs = CrmFetchKitNew.FetchSync(fetchXml);
        if (rs.length > 0) {
            var tmp = rs[0].attributes;
            var bsd_detailamount = tmp.bsd_detailamount != null ? tmp.bsd_detailamount.value : 0;
            var bsd_discount = tmp.bsd_discount != null ? tmp.bsd_discount.value : 0;
            var bsd_packagesellingamount = tmp.bsd_packagesellingamount != null ? tmp.bsd_packagesellingamount.value : 0;
            var bsd_totalamountlessfreight = tmp.bsd_totalamountlessfreight != null ? tmp.bsd_totalamountlessfreight.value : 0;
            var bsd_landvaluededuction = tmp.bsd_landvaluededuction != null ? tmp.bsd_landvaluededuction.value : 0;
            var totaltax = tmp.totaltax != null ? tmp.totaltax.value : 0;
            var bsd_freightamount = tmp.bsd_freightamount != null ? tmp.bsd_freightamount.value : 0;
            var totalamount = tmp.totalamount != null ? tmp.totalamount.value : 0;

            crmcontrol.setValue("bsd_listedpricecurrent", bsd_detailamount);
            crmcontrol.setValue("bsd_discountcurrent", bsd_discount);
            crmcontrol.setValue("bsd_handoverconditionamountcurrent", bsd_packagesellingamount);
            crmcontrol.setValue("bsd_netsellingpricecurrent", bsd_totalamountlessfreight);
            crmcontrol.setValue("bsd_landvaluedeductioncurrent", bsd_landvaluededuction);
            crmcontrol.setValue("bsd_totalvattaxcurrent", totaltax);
            crmcontrol.setValue("bsd_maintenancefeecurrent", bsd_freightamount);
            crmcontrol.setValue("bsd_totalamount", totalamount);

            crmcontrol.setValue("bsd_listedpricenew", bsd_detailamount);
            crmcontrol.setValue("bsd_discountnew", bsd_discount);
            crmcontrol.setValue("bsd_handoverconditionamountnew", bsd_packagesellingamount);
            crmcontrol.setValue("bsd_netsellingpricenew", bsd_totalamountlessfreight);
        } else {
            crmcontrol.setValue("bsd_listedpricecurrent", null);
            crmcontrol.setValue("bsd_discountcurrent", null);
            crmcontrol.setValue("bsd_handoverconditionamountcurrent", null);
            crmcontrol.setValue("bsd_netsellingpricecurrent", null);
            crmcontrol.setValue("bsd_landvaluedeductioncurrent", null);
            crmcontrol.setValue("bsd_totalvattaxcurrent", null);
            crmcontrol.setValue("bsd_maintenancefeecurrent", null);
            crmcontrol.setValue("bsd_totalamount", null);

            crmcontrol.setValue("bsd_listedpricenew", null);
            crmcontrol.setValue("bsd_discountnew", null);
            crmcontrol.setValue("bsd_handoverconditionamountnew", null);
            crmcontrol.setValue("bsd_netsellingpricenew", null);

        }

    }

}
function filter_ps() {
    var OE = crmcontrol.getValue("bsd_optionentry");
    if (OE) {
        var ViewID = "{72fc99a0-b37b-4a6f-9955-e3c65d42f524}";
        var entityName = "bsd_paymentschemedetail";
        var viewDisplayName = "Paymentscheme Detail Lookup View";
        var fetchXml = ["<fetch>", "  <entity name='bsd_paymentschemedetail'>", "    <filter type='and'>", "      <condition attribute='bsd_optionentry' operator='eq' value='", OE[0].id, "'/>", "      <condition attribute='statuscode' operator='eq' value='", 100000000, "'/>", "    </filter>", "  </entity>", "</fetch>",];
        var layoutXml = "<grid>  " + "<row name='result'>  " + "<cell name='bsd_name'  />  " + "</row>   " + "</grid>   ";
        Xrm.Page.getControl("bsd_installment").addCustomView(ViewID, entityName, viewDisplayName, fetchXml.join(""), layoutXml, true);

    } else {
        var ViewID = "{72fc99a0-b37b-4a6f-9955-e3c65d42f524}";
        var entityName = "bsd_paymentschemedetail";
        var viewDisplayName = "Payment Scheme Detail Lookup View";
        var fetchXml = ["<fetch>", "  <entity name='bsd_paymentschemedetail'>", "    <filter type='and'>", "      <condition attribute='statuscode' operator='eq' value='", 1234, "'/>", "    </filter>", "  </entity>", "</fetch>",];
        var layoutXml = "<grid>  " + "<row name='result'>  " + "<cell name='bsd_name'  />  " + "</row>   " + "</grid>   ";
        Xrm.Page.getControl("bsd_installment").addCustomView(ViewID, entityName, viewDisplayName, fetchXml.join(""), layoutXml, true);
    }

}

function UnitsChange() {
    LoadLandValue();
    var units = Xrm.Page.getAttribute("bsd_units").getValue();
    var bsd_type = Xrm.Page.getAttribute("bsd_type").getValue();
    if (bsd_type == 100000000 && units == null) {
        crmcontrol.setValue("bsd_optionentry", null);
        crmcontrol.setValue("bsd_installment", null);
        crmcontrol.setValue("bsd_amountofthisphasecurrent", null);
    }
}
function LoadLandValue() {
    var units = Xrm.Page.getAttribute("bsd_units").getValue();
    var xml = [];
    var landvalue;
    if (units) {
        xml.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>");
        xml.push("<entity name='product'>");
        xml.push("<attribute name='productid' />");
        xml.push("<attribute name='bsd_landvalueofunit' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='productid' operator='eq' uitype='product' value='" + units[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            debugger;
            if (rs.length > 0) {
                if (rs[0].attributes.bsd_landvalueofunit != null) landvalue = rs[0].attributes.bsd_landvalueofunit.value;

            }
        },
            function (er) {
                console.log(er.message)
            });
    }
    Xrm.Page.getAttribute("bsd_landvalueold").setValue(landvalue);
}