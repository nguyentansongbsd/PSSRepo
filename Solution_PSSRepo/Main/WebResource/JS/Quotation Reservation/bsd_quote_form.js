function onloadForm() {
    var xml = [];
    xml.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
    xml.push("<entity name='bsd_paymentschemedetail'>");
    xml.push("<attribute name='bsd_name' />");
    xml.push("<filter type='and'>");
    xml.push("<condition attribute='bsd_reservation' operator='eq' value='" + Xrm.Page.data.entity.getId() + "' />");
    xml.push("</filter>");
    xml.push("</entity>");
    xml.push("</fetch>");
    console.log(xml);
    CrmFetchKit.Fetch(xml.join(""), true).then(function (rs) {
        if (rs.length > 0) {
            Xrm.Page.getControl("bsd_ngaydatcoc").setDisabled(true);
        }
    })
}
   