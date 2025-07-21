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
var isload = 0;
var isShowNoti = 0;
function onLoad_bsd_GenTerminationLetterForm(executionContext) {
    lockFieldsIfStatusNotActive(executionContext);
    var entityId = Xrm.Page.data.entity.getId();
    if (!entityId) return;
    function fetchProcessingStatus(callback) {
        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() +
            "/api/data/v8.2/bsd_genterminationletters(" + entityId.replace(/[{}]/g, "") + ")?$select=bsd_processing_pa";
        req.open("GET", url, true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    callback(result.bsd_processing_pa);
                } else {
                    callback(null);
                }
            }
        };
        req.send();
    }

    function showProcessingDialog() {
        window.parent.processingDlg.show();
    }

    function closeProcessingDialog() {
        window.parent.processingDlg.hide();
    }

    function pollProcessingStatus() {
        fetchProcessingStatus(function (processing) {
            if (processing === true) {
                if (isShowNoti == 0) {
                    showProcessingDialog();
                }
                setTimeout(pollProcessingStatus, 5000);
                isload = 1;
                isShowNoti = 1;
            } else {
                if (isload == 1) {
                    window.top.location.reload();
                }
            }
        });
    }

    pollProcessingStatus();
}

function lockFieldsIfStatusNotActive(executionContext) {
    const formContext = executionContext.getFormContext();
    const status = formContext.getAttribute("statuscode").getValue();
    const project = formContext.getAttribute("bsd_project").getValue();

    if (!project) {

        formContext.getControl("bsd_block").setDisabled(true);
        formContext.getControl("bsd_floor").setDisabled(true);
        formContext.getControl("bsd_product").setDisabled(true);
    }
    if (status !== 1) {
        const allControls = formContext.ui.controls.get();

        allControls.forEach(function (control) {
            const fieldName = control.getName();

            // Chỉ khóa nếu field không phải là 'bsd_name'
            if (fieldName !== "bsd_displayname") {
                try {
                    formContext.getControl(fieldName).setDisabled(true);
                } catch (e) {
                    console.log("Cannot lock field: " + fieldName);
                }
            }
        });
    }
}



/**
 * Xử lý thay đổi trên trường 'Project' (bsd_project).
 * Lọc 'Block', 'Floor', và 'Product' dựa trên Project đã chọn.
 * @param {Xrm.ExecutionContext} executionContext - Đối tượng Execution Context.
 */
function onChangeProject(executionContext) {
    const formContext = executionContext.getFormContext();
    const projectAttribute = formContext.getAttribute("bsd_project");
    const project = projectAttribute.getValue();

    const blockControl = formContext.getControl("bsd_block");
    const floorControl = formContext.getControl("bsd_floor");
    const productControl = formContext.getControl("bsd_product");

    // Xóa tất cả các lookup phụ thuộc và vô hiệu hóa chúng nếu Project không có giá trị
    if (!project || project.length === 0) {
        formContext.getAttribute("bsd_block").setValue(null);
        formContext.getAttribute("bsd_floor").setValue(null);
        formContext.getAttribute("bsd_product").setValue(null);

        blockControl.setDisabled(true);
        floorControl.setDisabled(true);
        productControl.setDisabled(true);

      
        return; // Dừng thực thi nếu không có Project
    }

    const projectId = project[0].id.replace(/[{}]/g, "");

    // --- Lọc 'Block' ---
    formContext.getAttribute("bsd_block").setValue(null); // Xóa giá trị Block hiện tại
    const addBlockFilter = function () {
        const fetchXml = `<filter type='and'><condition attribute='bsd_project' operator='eq' value='${projectId}'/></filter>`;
        blockControl.addCustomFilter(fetchXml, "bsd_block"); // 'bsd_block' là tên logic của thực thể Block
    };
    blockControl.addPreSearch(addBlockFilter);
    blockControl.setDisabled(false); // Kích hoạt trường Block

    // --- Lọc 'Floor' (theo Project) ---
    formContext.getAttribute("bsd_floor").setValue(null); // Xóa giá trị Floor hiện tại
    const addFloorFilterForProject = function () {
        const fetchXml = `<filter type='and'><condition attribute='bsd_project' operator='eq' value='${projectId}'/></filter>`;
        floorControl.addCustomFilter(fetchXml, "bsd_floor"); // 'bsd_floor' là tên logic của thực thể Floor
    };
    floorControl.addPreSearch(addFloorFilterForProject);
    floorControl.setDisabled(false); // Kích hoạt trường Floor

    // --- Lọc 'Product' (theo Project) ---
    formContext.getAttribute("bsd_product").setValue(null); // Xóa giá trị Product hiện tại
    const addProductFilterForProject = function () {
        const fetchXml = `<filter type='and'><condition attribute='bsd_projectcode' operator='eq' value='${projectId}'/></filter>`;
        productControl.addCustomFilter(fetchXml, "product"); // 'product' là tên logic của thực thể Product
    };
    productControl.addPreSearch(addProductFilterForProject);
    productControl.setDisabled(false); // Kích hoạt trường Product
}

/**
 * Xử lý thay đổi trên trường 'Block' (bsd_block).
 * Lọc 'Floor' và 'Product' dựa trên Block đã chọn.
 * @param {Xrm.ExecutionContext} executionContext - Đối tượng Execution Context.
 */
function onChangeBlock(executionContext) {
    const formContext = executionContext.getFormContext();
    const blockAttribute = formContext.getAttribute("bsd_block");
    const block = blockAttribute.getValue();

    const floorControl = formContext.getControl("bsd_floor");
    const productControl = formContext.getControl("bsd_product");

    // Xóa tất cả các lookup phụ thuộc nếu Block không có giá trị
    if (!block || block.length === 0) {
        formContext.getAttribute("bsd_floor").setValue(null);
        formContext.getAttribute("bsd_product").setValue(null);

        return;
    }

    const blockId = block[0].id.replace(/[{}]/g, "");

    // --- Lọc 'Floor' (theo Block) ---
    formContext.getAttribute("bsd_floor").setValue(null); // Xóa giá trị Floor hiện tại
    const addFloorFilterForBlock = function () {
        const fetchXml = `<filter type='and'><condition attribute='bsd_block' operator='eq' value='${blockId}'/></filter>`;
        floorControl.addCustomFilter(fetchXml, "bsd_floor");
    };
    floorControl.addPreSearch(addFloorFilterForBlock);

    // --- Lọc 'Product' (theo Block) ---
    formContext.getAttribute("bsd_product").setValue(null); // Xóa giá trị Product hiện tại
    const addProductFilterForBlock = function () {
        const fetchXml = `<filter type='and'><condition attribute='bsd_blocknumber' operator='eq' value='${blockId}'/></filter>`;
        productControl.addCustomFilter(fetchXml, "product");
    };
    productControl.addPreSearch(addProductFilterForBlock);
}

/**
 * Xử lý thay đổi trên trường 'Floor' (bsd_floor).
 * Lọc 'Product' dựa trên Floor đã chọn.
 * @param {Xrm.ExecutionContext} executionContext - Đối tượng Execution Context.
 */
function onChangeFloor(executionContext) {
    const formContext = executionContext.getFormContext();
    const floorAttribute = formContext.getAttribute("bsd_floor");
    const floor = floorAttribute.getValue();

    const productControl = formContext.getControl("bsd_product");

    // Xóa tất cả các lookup phụ thuộc nếu Floor không có giá trị
    if (!floor || floor.length === 0) {
        formContext.getAttribute("bsd_product").setValue(null);
        return;
    }

    const floorId = floor[0].id.replace(/[{}]/g, "");

    // --- Lọc 'Product' (theo Floor) ---
    formContext.getAttribute("bsd_product").setValue(null); // Xóa giá trị Product hiện tại
    const addProductFilterForFloor = function () {
        const fetchXml = `<filter type='and'><condition attribute='bsd_floor' operator='eq' value='${floorId}'/></filter>`;
        productControl.addCustomFilter(fetchXml, "product");
    };
    productControl.addPreSearch(addProductFilterForFloor);
}

function formatDateToDDMMYYYY(dateInput) {
   
    var day = ("0" + dateInput.getDate()).slice(-2);
    var month = ("0" + (dateInput.getMonth() + 1)).slice(-2);
    var year = dateInput.getFullYear();
    return month + "/" + day + "/" + year;
}
function generateTerminateletter2() {
    try {
        debugger;
        var formContext = Xrm.Page;

        // Lấy giá trị Project
        var projectAttr = formContext.getAttribute("bsd_project");
        var project = projectAttr && projectAttr.getValue() && projectAttr.getValue().length > 0 ? projectAttr.getValue()[0].id.replace(/[{}]/g, "") : null;

        // Lấy giá trị Block
        var blockAttr = formContext.getAttribute("bsd_block");
        var block = blockAttr && blockAttr.getValue() && blockAttr.getValue().length > 0 ? blockAttr.getValue()[0].id.replace(/[{}]/g, "") : null;

        // Lấy giá trị Floor
        var floorAttr = formContext.getAttribute("bsd_floor");
        var floor = floorAttr && floorAttr.getValue() && floorAttr.getValue().length > 0 ? floorAttr.getValue()[0].id.replace(/[{}]/g, "") : null;

        // Lấy giá trị Units
        var unitsAttr = formContext.getAttribute("bsd_product");
        var units = unitsAttr && unitsAttr.getValue() && unitsAttr.getValue().length > 0 ? unitsAttr.getValue()[0].id.replace(/[{}]/g, "") : null;

        // Lấy giá trị _date
        var dateAttr = formContext.getAttribute("bsd_date");
        // Hàm chuyển đổi từ chuỗi ngày dạng "Mon Jul 07 2025 08:00:00 GMT+0700 (Indochina Time)" sang "dd/MM/yyyy"
       

        // Ví dụ sử dụng:
        var dateAttr = formContext.getAttribute("bsd_date");
        var _date = "";
        if (dateAttr && dateAttr.getValue()) {
            _date = formatDateToDDMMYYYY(dateAttr.getValue());
        }
       

        if (!project) {
            window.top.$ui.Dialog("Message", "Please select Project!");
            return;
        }
        window.top.$ui.Confirm("Confirms", "Do you want to genarate Terminate letter?", function (e) {
            debugger;
            window.parent.processingDlg.show();

            window.top.ExecuteAction(
                "",
                "",
                "bsd_Action_TerminateLetter_GenerateTerminateLetter",
                [
                    { name: 'Project', type: 'string', value: project || "" },
                    { name: 'Block', type: 'string', value: block || "" },
                    { name: 'Floor', type: 'string', value: floor || "" },
                    { name: 'Units', type: 'string', value: units || "" },
                    { name: '_date', type: 'string', value: _date ? _date.toString() : "" },
                ],
                function (result) {
                    window.parent.processingDlg.hide();
                    if (result != null && result.status != null) {
                        if (result.status == "error")
                            window.top.$ui.Dialog("Message", result.data);
                        else if (result.status == "success") {
                            if (result && result.data && result.data.idmaster && result.data.idmaster.value) {
                                var entityId = result.data.idmaster.value;
                                var entityName = "bsd_genterminationletter";
                                Xrm.Utility.openEntityForm(entityName, entityId);
                            } else {
                                window.top.$ui.Dialog("Error", "Cannot open Termination Letter form. Invalid result data.", null);
                            }
                        } else {
                            console.log(JSON.stringify(result));
                        }
                    }
                }, true);
        }, null);
    }
    catch (e) {
        window.top.$ui.Dialog("Error", e.message, null);
    }
}
function vis_Genbtn() {
    var formContext = Xrm.Page;
    const status = formContext.getAttribute("statuscode").getValue();
    var formType = formContext.ui.getFormType(); // Đối với Client API hiện đại (Unified Interface)

    if (status != 1 || formType==1) {
        return false
    } else {
        return true;
    }
}