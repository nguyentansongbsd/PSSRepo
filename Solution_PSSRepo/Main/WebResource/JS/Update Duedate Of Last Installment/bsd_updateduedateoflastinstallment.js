function blockAllFieldsApprove() {
    var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
    if (statuscode == 100000000) {
        crmcontrol.disabledForm();
    }
}


function OnLoad() {
    filter_UnitsByProject();
    blockAllFieldsApprove()
    filterLookupByStatus();
    
}

function filter_UnitsByProject(IsOnChange) {
    var project = Xrm.Page.getAttribute("bsd_project").getValue();
    xml = [];
    xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
    xml.push("<entity name='product'>");
    xml.push("<attribute name='name' />");
    xml.push("<attribute name='productid' />");
    xml.push("<attribute name='productnumber' />");
    xml.push("<attribute name='statuscode' />");
    xml.push("<order attribute='productnumber' descending='false' />");
    if (project != null) {
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_projectcode' operator='eq'  value='" + project[0].id + "' />");
        xml.push("</filter>");
    }
    xml.push("</entity>");
    xml.push("</fetch>");
    var entityName = "product";
    var viewId = "{8BA625B2-6A2A-4735-BAB2-0C74AE8442A4}";
    var viewDisplayName = "Units Lookup View";
    var layoutXml = "<grid name='resultset' " + "object='1' " + "jump='productid'  " + "select='1'  " + "icon='1'  " + "preview='1'>  " +
        "<row name='result'  " + "id='productid'>  " +
        "<cell name='name'   " + "width='200' />  " +
        "<cell name='productnumber' " + "width='200' />  " +
        "<cell name='statuscode' " + "width='200' />  " +
        "</row>   " +
        "</grid>   ";
    Xrm.Page.getControl("bsd_units").addCustomView(viewId, entityName, viewDisplayName, xml.join(""), layoutXml, true);
    if (IsOnChange == true)
        Xrm.Page.getAttribute("bsd_units").setValue(null);
}
function LoadProject() {
    var field = Xrm.Page.getAttribute("bsd_updateduedateoflastinstapprove").getValue();
    var value;
    if (field) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_updateduedateoflastinstallmentapprove'>");
        xml.push("<attribute name='bsd_updateduedateoflastinstallmentapproveid' />");
        xml.push("<attribute name='bsd_project' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_updateduedateoflastinstallmentapproveid' operator='eq' uitype='bsd_updateduedateoflastinstallmentapprove' value='" + field[0].id + "' />");
        xml.push("</filter>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            if (rs.length > 0 && rs[0].attributes.bsd_project) {
                debugger;
                value = [{
                    "id": rs[0].attributes.bsd_project.guid,
                    "name": rs[0].attributes.bsd_project.name,
                    "entityType": "bsd_project"
                }];
            }
        }, function (err) {
            console.log(err);
        });
    }
    Xrm.Page.getAttribute("bsd_project").setValue(value);
}
function UnitsChange() {
    var field = Xrm.Page.getAttribute("bsd_units").getValue();
    var oe, ins;
    if (field) {
        var xml = [];
        xml.push("<fetch version='1.0' output-format='xml-platform' count='1' mapping='logical' distinct='false'>");
        xml.push("<entity name='bsd_paymentschemedetail'>");
        xml.push("<attribute name='bsd_paymentschemedetailid' />");
        xml.push("<attribute name='bsd_name' />");
        xml.push("<attribute name='bsd_ordernumber' />");
        xml.push("<attribute name='bsd_optionentry' />");
        xml.push("<order attribute='bsd_ordernumber' descending='true' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='statecode' operator='eq' value='0' />");
        xml.push("</filter>");
        xml.push("<link-entity name='salesorder' from='salesorderid' to='bsd_optionentry' alias='ac'>");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='statuscode' operator='ne' value='100000006' />");
        xml.push("<condition attribute='bsd_unitnumber' operator='eq'  uitype='product' value='" + field[0].id + "' />");
        xml.push("</filter>");
        xml.push("</link-entity>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKit.Fetch(xml.join(""), false).then(function (rs) {
            if (rs.length > 0) {
                if (rs[0].attributes.bsd_optionentry) {
                    debugger;
                    oe = [{
                        "id": rs[0].attributes.bsd_optionentry.guid,
                        "name": rs[0].attributes.bsd_optionentry.name,
                        "entityType": "salesorder"
                    }];
                }
                if (rs[0].attributes.bsd_paymentschemedetailid && rs[0].attributes.bsd_name) {
                    ins = [{
                        "id": rs[0].attributes.bsd_paymentschemedetailid.value,
                        "name": rs[0].attributes.bsd_name.value,
                        "entityType": "bsd_paymentschemedetail"
                    }];
                }
            }
            else {
                window.top.$ui.Dialog("Message", "The units you have selected have no valid Option Entry. (All Option Entry have been terminated)");
                Xrm.Page.getAttribute("bsd_units").setValue(null);
            }
        }, function (err) {
            console.log(err);
        });
    }
    Xrm.Page.getAttribute("bsd_lastinstallment").setValue(ins);
    Xrm.Page.getAttribute("bsd_optionentry").setValue(oe);
    Xrm.Page.getAttribute("bsd_duedate").setValue(null);
}
function filterLookupByStatus() {

    // Lấy trường lookup
    var lookupField = Xrm.Page.getControl("bsd_updateduedateoflastinstapprove");

    // Kiểm tra nếu trường lookup tồn tại
    if (lookupField) {
        // Áp dụng bộ lọc
        lookupField.addPreSearch(function () {
            // Tạo FetchXML để lọc theo trạng thái
            var fetchXml = "<filter type='and'>" +
                "<condition attribute='statuscode' operator='eq' value='1' />" + // 1 là giá trị của trạng thái "Active"
                "</filter>";

            // Áp dụng FetchXML filter
            lookupField.addCustomFilter(fetchXml);
        });
    }
}