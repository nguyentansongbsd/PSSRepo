function approve() {
    // Hiển thị hộp thoại xác nhận
    recordId = Xrm.Page.data.entity.getId();
    entityName = Xrm.Page.data.entity.getEntityName();
    newStatus = 100000000;
    var confirmOptions = { height: 200, width: 450 };
    Xrm.Utility.getEntityMetadata(entityName).then(
        function (metadata) {

            // Lấy tên hiển thị (display name) của entity
            var displayName = metadata.DisplayName;
            var confirmStrings = {
                text: "Are you sure to Approve " + displayName+"?",
                title: "Confirm Approve"
            };
            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                function (success) {
                    if (success.confirmed) {
                        // Nếu người dùng xác nhận, thực hiện update record
                        var data = {
                            "statuscode": newStatus // Thay đổi trạng thái
                        };

                        Xrm.WebApi.updateRecord(entityName, recordId, data).then(
                            function (result) {
                                Xrm.Page.data.refresh(false).then(
                                    function () {
                                        onload();
                                        console.log("Form reloaded successfully");
                                    },
                                    function (error) {
                                        console.error("Error reloading form: ", error);
                                    }
                                );
                            },
                            function (error) {
                                console.error("Error updating record: ", error);
                                Xrm.Navigation.openAlertDialog({ text: error });
                            }
                        );
                    } else {
                        // Nếu người dùng không xác nhận, không làm gì cả
                        console.log("Update cancelled by user");
                    }
                }
            );
            // Hiển thị tên hiển thị
            
        },
        function (error) {
            console.error("Lỗi khi lấy metadata của entity: ", error);
            Xrm.Navigation.openAlertDialog({ text: "Có lỗi xảy ra khi lấy tên hiển thị của entity!" });
        }
    );
}
function onload()
{
    debugger;
    if (Xrm.Page.getAttribute("statuscode").getValue() == 100000000) {
        disableFormFields(true);
    }
    else {
        disableFormFields(false);
    }
}
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