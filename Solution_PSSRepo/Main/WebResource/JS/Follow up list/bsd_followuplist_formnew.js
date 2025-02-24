function onloadForm() {
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    var Type = Xrm.Page.getAttribute("bsd_type").getValue();
    if (status == 1) {
        if (Type == 100000006 || Type == 100000000 || Type == 100000001 || Type == 100000005 || Type == 100000002 || Type == 100000003) {
            Xrm.Page.ui.clearFormNotification();

            Xrm.Page.ui.setFormNotification("Please check the Termination information before saving.", "WARNING");
        }
        else {
            Xrm.Page.ui.clearFormNotification();
        }
    }
}