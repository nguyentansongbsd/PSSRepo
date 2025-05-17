function TypeOnChange() {
    Xrm.Page.getAttribute("bsd_priceperm2").setValue(null);
    Xrm.Page.getAttribute("bsd_unittype").setValue(null);
    Xrm.Page.getAttribute("bsd_amount").setValue(null);
}

function methodOnChange() {
    var method = Xrm.Page.getAttribute("bsd_method").getValue();

    if (method == 100000000) {
        // Method = Price/m2
        Xrm.Page.getControl("bsd_priceperm2").setVisible(true);
        Xrm.Page.getControl("bsd_percent").setVisible(false);
        Xrm.Page.getControl("bsd_amount").setVisible(false);
        Xrm.Page.getAttribute("bsd_priceperm2").setRequiredLevel('required');
        Xrm.Page.getAttribute("bsd_percent").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_amount").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_priceperm2").setValue(null);
        Xrm.Page.getAttribute("bsd_percent").setValue(null);
        Xrm.Page.getAttribute("bsd_amount").setValue(null);
    } else if (method == 100000001) {
        // Method = Amount
        Xrm.Page.getControl("bsd_priceperm2").setVisible(false);
        Xrm.Page.getControl("bsd_percent").setVisible(false);
        Xrm.Page.getControl("bsd_amount").setVisible(true);
        Xrm.Page.getAttribute("bsd_priceperm2").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_percent").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_amount").setRequiredLevel('required');
        Xrm.Page.getAttribute("bsd_priceperm2").setValue(null);
        Xrm.Page.getAttribute("bsd_percent").setValue(null);
        Xrm.Page.getAttribute("bsd_amount").setValue(null);
    } else if (method == 100000002) {
        // Method = Percent
        Xrm.Page.getControl("bsd_priceperm2").setVisible(false);
        Xrm.Page.getControl("bsd_percent").setVisible(true);
        Xrm.Page.getControl("bsd_amount").setVisible(false);
        Xrm.Page.getAttribute("bsd_priceperm2").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_percent").setRequiredLevel('required');
        Xrm.Page.getAttribute("bsd_amount").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_priceperm2").setValue(null);
        Xrm.Page.getAttribute("bsd_percent").setValue(null);
        Xrm.Page.getAttribute("bsd_amount").setValue(null);
    } else {
        Xrm.Page.getControl("bsd_priceperm2").setVisible(false);
        Xrm.Page.getControl("bsd_percent").setVisible(false);
        Xrm.Page.getControl("bsd_amount").setVisible(false);
        Xrm.Page.getAttribute("bsd_priceperm2").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_percent").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_amount").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_priceperm2").setValue(null);
        Xrm.Page.getAttribute("bsd_percent").setValue(null);
        Xrm.Page.getAttribute("bsd_amount").setValue(null);
    }
}

function SHOW_HIDE_FIELD() {
    debugger;
    var byUnitType = Xrm.Page.getAttribute("bsd_byunittype").getValue();
    if (byUnitType == 1) {
        Xrm.Page.getControl("bsd_unittype").setVisible(true);
        Xrm.Page.getAttribute("bsd_unittype").setRequiredLevel('required');
    } else {
        Xrm.Page.getControl("bsd_unittype").setVisible(false);
        Xrm.Page.getAttribute("bsd_unittype").setRequiredLevel('none');
    }

    var method = Xrm.Page.getAttribute("bsd_method").getValue();
    if (method == 100000000) {
        // Method = Price/m2
        Xrm.Page.getControl("bsd_priceperm2").setVisible(true);
        Xrm.Page.getControl("bsd_percent").setVisible(false);
        Xrm.Page.getControl("bsd_amount").setVisible(false);
        Xrm.Page.getAttribute("bsd_priceperm2").setRequiredLevel('required');
        Xrm.Page.getAttribute("bsd_percent").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_amount").setRequiredLevel('none');
    } else if (method == 100000001) {
        // Method = Amount
        Xrm.Page.getControl("bsd_priceperm2").setVisible(false);
        Xrm.Page.getControl("bsd_percent").setVisible(false);
        Xrm.Page.getControl("bsd_amount").setVisible(true);
        Xrm.Page.getAttribute("bsd_priceperm2").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_percent").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_amount").setRequiredLevel('required');
    } else if (method == 100000002) {
        // Method = Percent
        Xrm.Page.getControl("bsd_priceperm2").setVisible(false);
        Xrm.Page.getControl("bsd_percent").setVisible(true);
        Xrm.Page.getControl("bsd_amount").setVisible(false);
        Xrm.Page.getAttribute("bsd_priceperm2").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_percent").setRequiredLevel('required');
        Xrm.Page.getAttribute("bsd_amount").setRequiredLevel('none');
    } else {
        Xrm.Page.getControl("bsd_priceperm2").setVisible(false);
        Xrm.Page.getControl("bsd_percent").setVisible(false);
        Xrm.Page.getControl("bsd_amount").setVisible(false);
        Xrm.Page.getAttribute("bsd_priceperm2").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_percent").setRequiredLevel('none');
        Xrm.Page.getAttribute("bsd_amount").setRequiredLevel('none');
    }
}
function onLoad() {
    SHOW_HIDE_FIELD();
    var sts = crmcontrol.getValue("statuscode");
    if (sts == 1) {
        crmcontrol.setDisabled("ownerid", true);
        crmcontrol.setDisabled("bsd_approvedate", true);
        crmcontrol.setDisabled("bsd_approver", true);
        if (CheckRoleForUser("CLVN_S&M_Sales Manager") || CheckRoleForUser("System Administrator")) {

            crmcontrol.setDisabled("statuscode", false);
            crmcontrol.setDisabled("header_statuscode", false);
        }
    }
    else {
        

            crmcontrol.setDisabled("statuscode", true);
            crmcontrol.setDisabled("header_statuscode", true);
    }
}
function CheckRoleForUser(rolename) {
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
    //PL_INVESTOR, PL_INVERTOR Manager
    for (var i = 0; i < userRoles.getLength(); i++) {
        if (userRoles.get(i).name === rolename) {
            return true; // Hiện nút nếu người dùng có vai trò
        }
    }
    return false; // Ẩn nút nếu không có vai trò
}

function vis_BtnAddExist(selectedControl) {
    debugger;
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
    if (entityname == "bsd_phaseslaunch") {
        var sts = Xrm.Page.getAttribute("statuscode").getValue();
        if (sts == 100000000) return false; // 100000000 = Launched
    }
    return true;
}

function vis_BtnDelete(selectedControl, selectedIds) {
    debugger;
    var entityname = Xrm.Page.data.entity.getEntityName();
    switch (entityname) {
        case "quote":
            var isExist = Xrm.Page.getAttribute("bsd_existinstallment").getValue();
            return !isExist;
        case "salesorder":
            if (selectedIds && selectedIds.length > 0) {
                var xml = [];
                xml.push("<fetch>");
                xml.push("  <entity name='bsd_packageselling'>");
                xml.push("    <attribute name='bsd_packagesellingid'/>");
                xml.push("    <attribute name='bsd_name'/>");
                xml.push("    <attribute name='bsd_amount'/>");
                xml.push("    <attribute name='bsd_startdate'/>");
                xml.push("    <attribute name='bsd_enddate'/>");
                xml.push("    <filter>");
                xml.push("      <condition attribute='bsd_packagesellingid' operator='in'>");
                for (const id of selectedIds) {
                    xml.push("        <value>", id, "</value>");
                }
                xml.push("      </condition>");
                xml.push("      <filter type='or'>");
                xml.push("        <condition attribute='bsd_amount' operator='eq' value='0'/>");
                xml.push("        <condition attribute='bsd_amount' operator='null'/>");
                xml.push("      </filter>");
                xml.push("    </filter>");
                xml.push("  </entity>");
                xml.push("</fetch>");
                var rs = CrmFetchKitNew.FetchSync(xml.join(""));
                if (rs.length == selectedIds.length) {
                    return true;
                }
            }
            return false;
        case "bsd_phaseslaunch":
            var sts = Xrm.Page.getAttribute("statuscode").getValue();
            if (sts == 100000000) return false; // 100000000 = Launched
            break;
    }
    return true;
}