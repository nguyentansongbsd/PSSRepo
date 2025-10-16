// JavaScript source code
//## new script bsd_btnTerminateLetter Jack 2016 .06.29
if (window.$ == null) window.$ = window.parent.$;

// EnableRule : add script load

var isShowNoti = 0;
function ready() {
    debugger;
    var timer = null;
    function wait() {
        if (window.top.$ != null
            && window.top.$.fn != null) {
            if (timer != null) {
                clearTimeout(timer);
                timer = null;
            }
            RegisterModal();
        }
        else
            timer = setTimeout(function () { wait() }, 1000);
    }
    wait();
}

function RegisterModal() {
    debugger;
    var script = window.parent.document.getElementById("new_modal.utilities.js");
    if (script == null) {
        script = window.parent.document.createElement("script");
        script.type = "text/javascript";
        script.id = "new_modal.utilities.js";
        script.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/new_modal.utilities.js";
        window.parent.document.head.appendChild(script);
    }
    var script2 = window.top.document.getElementById("bsd_processingdialog.js");
    if (script2 == null) {
        script2 = window.top.document.createElement("script");
        script2.type = "text/javascript";
        script2.id = "bsd_processingdialog.js";
        script2.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_processingdialog.js";
        window.top.document.head.appendChild(script2);
    }
    var script1 = window.parent.document.getElementById("bsd_execute.services.ultilities.js");
    if (script1 == null) {
        script1 = window.parent.document.createElement("script");
        script1.type = "text/javascript";
        script1.id = "bsd_execute.services.ultilities.js";
        script1.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_execute.services.ultilities.js";
        window.parent.document.head.appendChild(script1);
    }


    var script3 = window.parent.document.getElementById("ClientGlobalContext.js.aspx");
    if (script3 == null) {
        script3 = window.parent.document.createElement("script");
        script3.type = "text/javascript";
        script3.id = "ClientGlobalContext.js.aspx";
        script3.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/ClientGlobalContext.js.aspx";
        window.parent.document.head.appendChild(script3);
    }
    var script4 = window.parent.document.getElementById("new_jquery2.0.3.min");
    if (script4 == null) {
        script4 = window.parent.document.createElement("script");
        script4.type = "text/javascript";
        script4.id = "new_jquery2.0.3.min";
        script4.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/new_jquery2.0.3.min";
        window.parent.document.head.appendChild(script4);
    }
    var script5 = window.parent.document.getElementById("bsd_CrmFetchKit.js");
    if (script5 == null) {
        script5 = window.parent.document.createElement("script");
        script5.type = "text/javascript";
        script5.id = "bsd_CrmFetchKit.js";
        script5.src = window.top.Xrm.Page.context.getClientUrl() + "/webresources/bsd_CrmFetchKit.js";
        window.parent.document.head.appendChild(script5);
    }
}

ready();
function onloadForm() {
    checkValuePrintQRBank();
}
function checkValuePrintQRBank() {
    if (Xrm.Page.getAttribute("bsd_print_qr_bank").getValue()) {
        getEntityDataByLookup("bsd_projectbankaccount", "bsd_project", Xrm.Page.data.entity.getId())
            .then(item => {
                // ✅ Xử lý khi thành công
                // Ví dụ: lặp qua và hiển thị tên
                if (item.length == 0) {
                    Xrm.Page.ui.setFormNotification("Please add Bank Account Default!", "WARNING");
                }
                else {
                    var isExistDefault = item.some(x => x.bsd_default === true);
                    if (!isExistDefault) {

                        Xrm.Page.ui.setFormNotification("Please add Bank Account Default!", "WARNING");
                    }
                }
            })
            .catch(error => {
                // ❌ Xử lý khi có lỗi
                console.error('Yêu cầu thất bại:', error.message);
            });
    }
}
/**
 * Lấy danh sách các bản ghi từ một entity dựa trên giá trị của một trường lookup.
 * Phiên bản này không sử dụng async/await.
 *
 * @param {string} entityName - Tên logic của entity bạn muốn truy vấn (ví dụ: "contacts").
 * @param {string} lookupFieldName - Tên logic của trường lookup (ví dụ: "parentcustomerid").
 * @param {string} lookupId - GUID của bản ghi trong trường lookup.
 * @returns {Promise<Array<object>>} - Một promise sẽ giải quyết với một mảng các đối tượng dữ liệu.
 */
function getEntityDataByLookup(entityName, lookupFieldName, lookupId) {
    // Trả về một đối tượng Promise mới ngay lập tức
    return new Promise((resolve, reject) => {
        // 1. Xây dựng URL cho API endpoint
        // ⚠️ Hãy nhớ thay thế bằng URL API thực tế của bạn!
        const apiEndpoint = `https://pssvn-preview.crm5.dynamics.com/api/data/v9.2/${entityName}s?$filter=_${lookupFieldName}_value eq ${lookupId}`;

        const headers = {
            'OData-MaxVersion': '4.0',
            'OData-Version': '4.0',
            'Accept': 'application/json',
            'Content-Type': 'application/json; charset=utf-8',
            // Thêm các header xác thực cần thiết ở đây, ví dụ:
            // 'Authorization': 'Bearer YOUR_ACCESS_TOKEN'
        };

        // 2. Gọi fetch API, nó sẽ trả về một Promise
        fetch(apiEndpoint, { method: 'GET', headers: headers })
            .then(response => {
                // 3. Kiểm tra xem phản hồi có thành công không
                if (!response.ok) {
                    // Nếu không thành công, ném ra một lỗi để nhảy tới khối .catch()
                    throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
                }
                // response.json() cũng trả về một Promise, nên ta return nó
                // để chuỗi .then() tiếp theo có thể xử lý kết quả JSON
                return response.json();
            })
            .then(data => {
                // 4. Xử lý dữ liệu JSON đã được phân tích cú pháp
                // Dữ liệu thường nằm trong thuộc tính 'value' của OData
                resolve(data.value); // THÀNH CÔNG: Hoàn thành Promise với dữ liệu
            })
            .catch(error => {
                // 5. Bắt bất kỳ lỗi nào xảy ra trong chuỗi Promise
                console.error('Không thể lấy dữ liệu:', error);
                reject(error); // THẤT BẠI: Từ chối Promise với thông tin lỗi
            });
    });
}
