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
                text: "Are you sure to Approve " + displayName + "?",
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
function onload() {
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
        if (control.name == "header_statuscode" || control.name == "header_bsd_discountnumber") {
            control.setDisabled(true);
        }
        else if (doesControlHaveAttribute(control)) {
            control.setDisabled(onOff);
        }
    });
}
function updateSaveButtonVisibility() {
    debugger;
    var anyChecked = $("table input[name='choose-ckb-team']:checked").length > 0;
    console.log("anyChecked:" + anyChecked);
    if (anyChecked) {
        $("#saveToMultipleTeam").show();
    } else {
        $("#saveToMultipleTeam").hide();
    }
}
function addModelShareTeam() {
    var modalShare = `
  <script>
   
  function btnSaveMulti() {
  debugger;
  var radioValue = Xrm.Page.data.entity.getEntityName() == "account" ? 3 : (Xrm.Page.data.entity.getEntityName() == "contact" ? 2 : 4);
  var id = "";
  $("table input[name='choose-ckb-team']:checked").each(function (index) {
      id = id + $(this).data('id') + ",";
  });
  if (id == "") {
      //window.top.processingDlg.hide();
      alertdialogConfirm("Xin vui lòng chọn team cần share !");
      return;
  }
  ExecuteAction(
      null, null, "bsd_Action_ShareCustomerToTeam", [
      {
          name: 'type',
          type: 'int',
          value: Xrm.Page.data.entity.getEntityName() == "account" ? 1 : (Xrm.Page.data.entity.getEntityName() == "contact" ? 0 : 3);

      },
      {
          name: 'id',
          type: 'string',
          value: arrID
      },
      {
          name: 'idTeam',
          type: 'string',
          value: id
      }
  ],
      function (result) {
          window.top.processingDlg.hide();
          if (result != null && result.status != null) {
              if (result.status == "error") alertdialogConfirm(result.data);
              else if (result.status == "success") {
                  alertdialogConfirm("Thao tác thành công !");
                  //crmcontrol.openAlertDialog("Thao tác thành công !","Notices");
                  $("#modalForm").css("display", "none");
              }
          }

      }, true);

  console.log(id);
}
  </script>
   <style type="text/css">
      * {
          margin: 0;
          padding: 0;
          box-sizing: border-box;
          font-family: "Segoe UI Regular", SegoeUI, "Segoe UI";
      }

      body {
          font-family: "Segoe UI Regular", SegoeUI, "Segoe UI";
          margin: 30px 15px 15px 15px;
          background: white;
          font-size: 12px;
      }

      a.search {
          display: block;
          width: 21px;
          height: 21px;
          background: url(data:image/png;base64,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) no-repeat center center;
          cursor: pointer;
      }

      .mr-10 {
          margin-right: 10px;
      }

      table {
          border-collapse: collapse;
          width: 100%;
      }

      td,
      th {
          border: 1px solid #dddddd;
          text-align: left;
          padding: 8px;
          font-size: 13px;
      }

      tr:nth-child(even) {
          background-color: #f9f9f9;
      }

      table th {
          font-weight: 600;
      }

      tbody tr:hover {
          background-color: #eaeaea;
      }

      .vis-hidden {
          display: none !important;
      }

      .vis-show {
          display: block !important;
      }

      .vis-show-flex {
          display: flex;
      }

      #search-content table {
          background: white;
      }

      .align-right {
          display: flex;
          justify-content: flex-end;
          /* margin-top: 20px; */
          flex: 1;
      }

      .d-flex-row {
          display: flex;
          flex-wrap: wrap;
          gap: 15px;
          align-items: baseline;
      }

      .row-radio {
          margin-top: 10px;
          margin-bottom: 10px;
      }

      .align-flex-center {
          align-self: center;
      }

      /* CSS */
      .button-7 {
          background-color: #0095ff;
          border: 1px solid transparent;
          border-radius: 3px;
          box-shadow: rgba(255, 255, 255, .4) 0 1px 0 0 inset;
          box-sizing: border-box;
          color: #fff;
          cursor: pointer;
          display: inline-block;
          /* font-family: -apple-system, system-ui, "Segoe UI", "Liberation Sans", sans-serif; */
          /* font-size: 13px; */
          font-weight: 400;
          line-height: 1.15385;
          margin: 0;
          outline: none;
          padding: 8px .8em;
          position: relative;
          text-align: center;
          text-decoration: none;
          user-select: none;
          -webkit-user-select: none;
          touch-action: manipulation;
          vertical-align: baseline;
          white-space: nowrap;
      }

      .button-7:hover,
      .button-7:focus {
          background-color: #07c;
      }

      .button-7:focus {
          box-shadow: 0 0 0 4px rgba(0, 149, 255, .15);
      }

      .button-7:active {
          background-color: #0064bd;
          box-shadow: none;
      }

      /* The Modal (background) */
      .modal {
          display: none;
          /* Hidden by default */
          position: fixed;
          /* Stay in place */
          z-index: 1;
          /* Sit on top */
          padding-top: 30vh;
          /* Location of the box */
          left: 0;
          top: 0;
          width: 100%;
          /* Full width */
          height: 100%;
          /* Full height */
          overflow: auto;
          /* Enable scroll if needed */
          background-color: rgb(0, 0, 0);
          /* Fallback color */
          background-color: rgba(0, 0, 0, 0.4);
          /* Black w/ opacity */
      }

      /* Modal Content */
      .modal-content {
          position: relative;
          background-color: #fefefe;
          margin: auto;
          padding: 0;
          border: 1px solid #888;
          width: 30%;
          box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);
          -webkit-animation-name: animatetop;
          -webkit-animation-duration: 0.4s;
          animation-name: animatetop;
          animation-duration: 0.4s
      }

      /* Add Animation */
      @-webkit-keyframes animatetop {
          from {
              top: -300px;
              opacity: 0
          }

          to {
              top: 0;
              opacity: 1
          }
      }

      @keyframes animatetop {
          from {
              top: -300px;
              opacity: 0
          }

          to {
              top: 0;
              opacity: 1
          }
      }

      /* The Close Button */
      #closeModal {
          color: white;
          float: right;
          font-size: 23px;
          font-weight: bold;
      }

      #closeModal:hover,
      #closeModal:focus {
          color: #000;
          text-decoration: none;
          cursor: pointer;
      }

      .modal-header {
          padding: 2px 16px;
          background-color: #29a6ff;
          color: white;
      }

      .modal-body {
          padding: 16px 16px 12px 16px;
      }

      .w-30 {
          width: 30px;
      }

      .mt-12 {
          margin-top: 12px;
      }

      .w-78 {
          width: 78%;
      }
  </style>
  <div id="modalForm" class="modal">
      <div class="modal-content">
          <div class="modal-header">
              <span id="closeModal" class="close align-right">&times;</span>
              <h2 style="text-align: center; padding: 5px;">Chọn Team</h2>
          </div>
          <div class="modal-body">
              <table>
                  <thead>
                      <tr>
                          <th class="w-30"></th>
                          <th>STT</th>
                          <th class="w-78">Team</th>
                      </tr>
                  </thead>
                  <tbody id="bodyTableTeam">
                  </tbody>
              </table>

              <div class="align-right mt-12">
                  <button id="saveToMultipleTeam" class="button-7" >Lưu</button>
              </div>
          </div>
      </div>

  </div>`
    $('body').append(modalShare);
    // Thêm logic ẩn/hiện nút "Lưu" khi chưa có checkbox nào được chọn

    // Gọi hàm này sau khi render bảng team (sau khi append các dòng vào #bodyTableTeam)
   

    // Gắn sự kiện khi checkbox thay đổi trạng thái
   

    // Khi mở modal, ẩn nút lưu nếu chưa có checkbox nào được chọn
    // Thêm vào cuối đoạn xử lý success trong btnSaveLAC, sau khi append các dòng vào #bodyTableTeam:

}
function btnSaveLAC() {
    if (CheckRoleForUser("CLVN_S&M_Head of Sale") || CheckRoleForUser("CLVN_S&M_Senior Sale Staff") || CheckRoleForUser("CLVN_S&M_Sales Manager"))
    {
        hasTeamAccess(Xrm.Page.data.entity.getEntityName(), Xrm.Page.data.entity.getId()).then(function (rss) {
            if (!rss) {
                $('#closeModal').click(function () {
                    $("#modalForm").css("display", "none");
                });
                $('#saveToMultipleTeam').unbind('click');
                $('#saveToMultipleTeam').click(btnSaveMulti);
                window.top.processingDlg.show();
                var id = Xrm.Page.data.entity.getId();
                var radioValue = Xrm.Page.data.entity.getEntityName() == "account" ? 1 : (Xrm.Page.data.entity.getEntityName() == "contact" ? 0 : 10);
                ExecuteAction(
                    null, null, "bsd_Action_ShareCustomerToTeam", [
                    {
                        name: 'type',
                        type: 'int',
                        value: radioValue
                    },
                    {
                        name: 'id',
                        type: 'string',
                        value: id
                    }
                ],
                    function (result) {
                        window.top.processingDlg.hide();
                        if (result != null && result.status != null) {
                            if (result.status == "error") alertdialogConfirm(result.data);
                            else if (result.status == "success") {

                                if (result.data.entityColl.value == "")
                                    alertdialogConfirm("Done!");
                                else {
                                    arrID = id;
                                    var data = JSON.parse(result.data.entityColl.value);

                                    $('#bodyTableTeam').empty();
                                    for (let i = 0; i < data.length; i++) {
                                        const item = data[i];
                                        $("#bodyTableTeam").append(`
                                      <tr>
                                          <td><input type='checkbox' name='choose-ckb-team' data-id='${item.TeamID}'  /></td>
                                          <td>${i + 1}</td>
                                          <td>${item.TeamName}</td>
                                      </tr>
                                  `);
                                    }
                                    $("#modalForm").css("display", "block");
                                    $('input[name="choose-ckb-team"]').click(updateSaveButtonVisibility);
                                    updateSaveButtonVisibility();



                                }
                            }
                        }
                    }, true);

            }
        })
    }

}
function btnSaveMulti() {
    debugger;
    window.top.processingDlg.show();
    var id = "";
    $("table input[name='choose-ckb-team']:checked").each(function (index) {
        id = id + $(this).data('id') + ",";
    });
    if (id == "") {
        //window.top.processingDlg.hide();
        alertdialogConfirm("Xin vui lòng chọn team cần share !");
        return;
    }
    ExecuteAction(
        null, null, "bsd_Action_ShareCustomerToTeam", [
        {
            name: 'type',
            type: 'int',
            value: Xrm.Page.data.entity.getEntityName() == "account" ? 3 : (Xrm.Page.data.entity.getEntityName() == "contact" ? 2 : 4)

        },
        {
            name: 'id',
            type: 'string',
            value: arrID
        },
        {
            name: 'idTeam',
            type: 'string',
            value: id
        }
    ],
        function (result) {
            if (result != null && result.status != null) {
                if (result.status == "error") alertdialogConfirm(result.data);
                else if (result.status == "success") {
                    alert("Thao tác thành công !");

                    window.top.processingDlg.hide();
                    //crmcontrol.openAlertDialog("Thao tác thành công !","Notices");
                    $("#modalForm").css("display", "none");
                }
            }

        }, true);

    console.log(id);
}
function alertdialogConfirm(content, titless) {
    var alertStrings = { confirmButtonLabel: "OK", text: content, title: "Notices" };
    var alertOptions = { height: 200, width: 600 };
    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
}
function hasTeamAccess(entityName, recordId) {
    return new Promise((resolve, reject) => {
        try {
            // Đảm bảo các tham số được truyền vào
            if (!entityName || !recordId) {
                throw new Error("Entity name và Record ID là bắt buộc");
            }

            // Đảm bảo recordId có định dạng đúng (không có dấu ngoặc nhọn)
            recordId = recordId.replace(/[{}]/g, "");

            // FetchXML để kiểm tra xem có team nào có quyền truy cập hay không
            var fetchData = {
                "principaltypecode": "9",
                "objecttypecode": entityName == "lead" ? 4 : entityName == "account" ? 1 : 2,
                "objectid": recordId,
                "accessrightsmask": "0"
            };
            var fetchXml = [
                "<fetch top='50'>",
                "  <entity name='principalobjectaccess'>",
                "    <filter>",
                "      <condition attribute='principaltypecode' operator='eq' value='", fetchData.principaltypecode/*9*/, "'/>",
                "      <condition attribute='objecttypecode' operator='eq' value='", fetchData.objecttypecode/*4*/, "'/>",
                "      <condition attribute='objectid' operator='eq' value='", fetchData.objectid/*a42ecf6b-7430-f011-8c4d-6045bd1d2e19*/, "'/>",
                "      <condition attribute='accessrightsmask' operator='ne' value='", fetchData.accessrightsmask/*0*/, "'/>",
                "    </filter>",
                "  </entity>",
                "</fetch>"
            ].join("");

            // Mã hóa FetchXML để sử dụng trong URL
            const encodedFetch = encodeURIComponent(fetchXml);

            // Thực hiện truy vấn để kiểm tra sự tồn tại của team access
            Xrm.WebApi.retrieveMultipleRecords("principalobjectaccess", `?fetchXml=${encodedFetch}`)
                .then(function (results) {
                    // Nếu có ít nhất một kết quả, trả về true
                    resolve(results.entities.length > 0);
                })
                .catch(function (error) {
                    reject(new Error(`Error checking team access: ${error.message}`));
                });
        } catch (error) {
            reject(error);
        }
    });
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