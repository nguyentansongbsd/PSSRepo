// JavaScript source code
$ = window.parent.$;
jQuery = window.parent.jQuery;

function onLoad() {
    SHOW_HIDE_LOCK();
    onloadSystem();
}

function SHOW_HIDE_LOCK() {
    onChange_Source();
}

function onChange_Source() {
    var source = Xrm.Page.getAttribute("bsd_source").getValue();
    // Source = Quotation Reservation
    if (source && source == 100000000) {
        Xrm.Page.getControl("bsd_reservation").setVisible(true);
        Xrm.Page.getAttribute("bsd_reservation").setRequiredLevel("required");
        Xrm.Page.getControl("bsd_optionentry").setVisible(false);
        Xrm.Page.getAttribute("bsd_optionentry").setValue(null);
        Xrm.Page.getAttribute("bsd_optionentry").setRequiredLevel("none");
    }
    // Source = Option Entry
    if (source && source == 100000001) {
        Xrm.Page.getControl("bsd_optionentry").setVisible(true);
        Xrm.Page.getAttribute("bsd_optionentry").setRequiredLevel("required");
        Xrm.Page.getControl("bsd_reservation").setVisible(false);
        Xrm.Page.getAttribute("bsd_reservation").setValue(null);
        Xrm.Page.getAttribute("bsd_reservation").setRequiredLevel("none");
    }
}
function onSave(context) {
    debugger;
    alert("ok");
    var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
    var bsd_name = Xrm.Page.getAttribute("bsd_name").getValue();
    if (statuscode == 100000002) {
        //gb_context = context
        //context.getEventArgs().preventDefault();
        var id = Xrm.Page.data.entity.getId();
        Xrm.Utility.openDialog(Xrm.Page.context.getClientUrl() + "/webresources/bsd_handovernotices_select_report.html?type=sendemail&data=" + id, { width: 410, height: 345 }, null, null);
        var timerpopup = setInterval(function () {
            console.log(window.top.winpopup);
            if (window.top.winpopup != null) {
                if (window.top.winpopup.closed) {
                    window.top.location.reload();
                }
            }
        }, 1000);
    }

}
function onloadSystem() {
    var sys = crmcontrol.getValue("bsd_system");
    if (sys == true) {
        blockAllField(true);
        crmcontrol.setDisabled("bsd_name", false);
        crmcontrol.setDisabled("bsd_date", false);
        crmcontrol.setDisabled("bsd_terminatefee", false);
        crmcontrol.setDisabled("bsd_terminatefeewaiver", false);
    }
}
function blockAllField(block) {
    Xrm.Page.ui.controls.forEach(
        function (control, index) {
            if (control.getControlType() != "subgrid")
                control.setDisabled(block);
        });
}

function onChange_terminatefeewaiver() {
    var terminatefeewaiver = Xrm.Page.getAttribute("bsd_terminatefeewaiver").getValue()||0;
    var totalforfeitureamount = Xrm.Page.getAttribute("bsd_totalforfeitureamount").getValue() || 0;
    Xrm.Page.getAttribute("bsd_terminatefee").setValue(totalforfeitureamount - terminatefeewaiver);
}

ấdâd aa ằquaf 