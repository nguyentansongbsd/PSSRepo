function btn_approve() {
    // Tạo đối tượng dữ liệu để cập nhật
    //const data = {};
    //data["statuscode"] = 100000001;

    //// Gọi phương thức updateRecord của Xrm.WebApi
    //Xrm.WebApi.updateRecord("bsd_updatelandvalue", Xrm.Page.data.entity.getId().replace("{", "").replace("}", ""), data)
    //    .then(
    //        function success(result) {
    //            console.log(`Cập nhật bản ghi thành công!`);
    //            Xrm.Page.data.refresh();
    //        },
    //        function error(error) {
    //            console.error("Lỗi khi cập nhật bản ghi:", error.message);
    //        }
    //    );
    const formContext = Xrm.Page; // Dùng Xrm.Page cho form hiện tại
    if (formContext.getAttribute("statuscode")) {
        formContext.getAttribute("statuscode").setValue(100000001);
        var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
        if (statuscode == 100000001 || statuscode == 100000003) {
            crmcontrol.disabledForm();
        }
        Xrm.Page.data.save();
    }
}
function vis_btn_approve() {
    var statuscode = Xrm.Page.getAttribute("statuscode").getValue();
    var role = crmcontrol.checkRoles("CLVN_CCR Manager") || crmcontrol.checkRoles("CLVN_FIN_Finance Manager");
    if (statuscode == 1 && role) return true;
    else return false;
}
