$ = window.parent.$;
jQuery = window.parent.jQuery;

//---------------------------------- GLOBAL ----------------------------------

// gridType     :     Advance = 1     /       Phase = 2

window.parent.ProductTree = SubGird;

var strCharacterCurrency = null;
var MaxLengthText = 40;
var MaxLengthTextPhase = 14;

//---------------------------------- BUILD TREE TEST ----------------------------------

function SubGird(gridType, id, sectionName, sort) {
    function Progress(ctn) {
        var t = null;
        var i = 1;
        var flag = true;
        var text = function () {
            var dots = '';
            for (j = 0; j < i; j++)
                dots += '.';
            ctn.html('loading' + dots);

            if (i >= 4)
                i = 1;
            else
                i++;
            if (flag) {
                t = setTimeout(text, 100);
            }
        };
        this.start = function () {
            flag = true;
            text();
        };
        this.stop = function () {
            flag = false;
            if (t != null)
                clearTimeout(t);
        };
    };
    var divId = id;
    var timer = null;

    var table = window.parent.$("table[name='" + sectionName + "']:eq(0)");
    table.css({ "table-layout": "inherit" });

    var tr = window.parent.$("table[name='" + sectionName + "']:eq(0)").children('tbody:first').children('tr:eq(2)');
    if (tr != null && tr.length > 0) {
        tr.children("td:last").remove();
        tr.children("td:first").attr("colspan", '2');
    }
    var prg = new Progress(tr.children('td:first-child'));
    tr.removeAttr('height');

    var BuildSubGrid = function (gridType, detail) {
        debugger;
        if (detail.length == 0) {
            tr.children('td:first-child').html('No Found Payment Scheme Detail');
            return;
        }

        var html = '';

        if (gridType == 2) // Payment Scheme Detail
        {
            if (sectionName == "SubGrid_PSD") {
                html += '<td width="100%">';
                html += '<div id="tree-container-' + divId + '" style="display:block;position:relative;width: 100%;">'
                                + '<table id="GridPhase" name="GridPhase" class="table" width="100%" cellpadding="0" cellspacing="0" border="0">'
                                    + '<tbody>'

                //+ Advance   =   true : payoff truoc ; false : collected truoc
                //+ Phase     =   true : paid truoc   ; false : Not Paid truoc 

                if (sort == true) // ko check
                {
                    html += '<tr>'
                            + '<td colspan="7" style="text-align: right;">'
                                + '<label><input name="ckbInstallment" id="ckbInstallment" class="ckbPaid1" checked type="checkbox" value="ckbPaid1" onclick="InputCheckSort(this)" > Sort</label>'
                            + '</td>'
                        + '</tr>';
                }
                else {
                    html += '<tr>'
                            + '<td colspan="7" style="text-align: right;">'
                                + '<label><input name="ckbInstallment" id="ckbInstallment" class="ckbPaid1" type="checkbox" value="ckbPaid1" onclick="InputCheckSort(this)" > Sort</label>'
                            + '</td>'
                        + '</tr>';
                }

                html += '<tr>'
                        + '<th width="4%"><span>No.</span></th>'
                        + '<th width="15%"><span>Payment Phase</span></th>'
                        + '<th width="13%" style="text-align:center"><span>Total Amount</span></th>'
                        + '<th width="13%" style="text-align:center"><span>Deposit</span></th>'
                        + '<th width="13%" style="text-align:center"><span>Amount Paid</span></th>'
                        + '<th width="13%" style="text-align:center"><span>Waiver<br/>(Installment)</span></th>'
                        + '<th width="13%" style="text-align:center"><span>Amount Apply</span></th>'
                        + '<th width="8%" style="text-align:center"><span>Status</span></th>'
                        + '<th width="8%"  style="text-align:center"><span>Selection</span></th>'
                    + '</tr>';

                for (var i = 0; i < detail.length; i++) {
                    var psd = detail[i];


                    var amountneed = psd.data[0].amountneed;
                    var amountneedFrm = amountneed.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.");
                    var amountFrm = psd.data[0].amountFrm == 0 ? (psd.data[0].amountFrm.toString() + " ₫") : psd.data[0].amountFrm;
                    var depositFrm = psd.data[0].depositFrm == 0 ? (psd.data[0].depositFrm.toString() + " ₫") : psd.data[0].depositFrm;
                    var waiverFrm = psd.data[0].waiverFrm == 0 ? (psd.data[0].waiverFrm.toString() + " ₫") : psd.data[0].waiverFrm;
                    var amountpaidFrm = psd.data[0].amountpaidFrm == 0 ? (psd.data[0].amountpaidFrm.toString() + " ₫") : psd.data[0].amountpaidFrm;

                    var FullName = psd.data[0].name;
                    var lenName = FullName.length;

                    var ShortName = FullName;
                    if (lenName > MaxLengthTextPhase)
                        ShortName = FullName.substr(0, MaxLengthTextPhase - 3) + '...';

                    html += '<tr>'
                                + '<td ><span>' + (i + 1) + '</span></td>'
                                + '<td ><span class="tooltip" title="' + FullName + '">' + ShortName + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + depositFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountpaidFrm + '</span></td>'


                                + '<td style="text-align: right; min-width: 80px;"><span>' + waiverFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountneedFrm + " ₫" + '</span></td>'
                                + '<td style="text-align: center;"><span>' + psd.data[0].statuscodename + '</span></td>';

                    html += '<td style="text-align: center;"><span><input name="ckb2" class="ckb2" type="checkbox"'
                                        + 'value="' + psd.data[0].id + '" style="height: 14px;width: 14px;vertical-align: middle;" '
                                        + 'onclick="InputCheckSubGrid(this,' + gridType + ')" id="IN-' + psd.data[0].id + '" /></span></td>';
                    html += '</tr>';
                }

                html += '</tbody>'
                            + '</table>'
                        + '</div>';
                html += '</td>';
            }
            else if (sectionName == "SubGrid_Interest") {
                html += '<td width="100%">';
                html += '<div id="tree-container-' + divId + '" style="display:block;position:relative;width: 100%;">'
                                + '<table id="GridInterest" name="GridInterest" class="table" width="100%" cellpadding="0" cellspacing="0" border="0">'
                                    + '<tbody>'

                if (sort == true) // ko check
                {
                    html += '<tr>'
                            + '<td colspan="7" style="text-align: right;">'
                                + '<label><input name="ckbInterest" id="ckbInterest" class="ckbPaid1" checked type="checkbox" value="ckbPaid1" onclick="InputCheckSort(this)" > Sort</label>'
                            + '</td>'
                        + '</tr>';
                }
                else {
                    html += '<tr>'
                            + '<td colspan="7" style="text-align: right;">'
                                + '<label><input name="ckbInterest" id="ckbInterest" class="ckbPaid1" type="checkbox" value="ckbPaid1" onclick="InputCheckSort(this)" > Sort</label>'
                            + '</td>'
                        + '</tr>';
                }


                html += '<tr>'
                        + '<th width="6%"><span>No.</span></th>'
                        + '<th width="18%"><span>Name</span></th>'
                        + '<th width="15%" style="text-align:center"><span>Interest Amount</span></th>'
                        + '<th width="15%" style="text-align:center"><span>Amount Paid</span></th>'

                        + '<th width="15%" style="text-align:center"><span>Waiver<br/>(Interest)</span></th>'
                        + '<th width="15%" style="text-align:center"><span>Amount Apply</span></th>'
                        + '<th width="8%" style="text-align:center"><span>Status</span></th>'
                        + '<th width="8%"  style="text-align:center"><span>Selection</span></th>'
                    + '</tr>';

                for (var i = 0; i < detail.length; i++) {
                    var psd = detail[i];


                    var amountneed = psd.data[0].amountneed;
                    var amountneedFrm = amountneed.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.");
                    var amountFrm = psd.data[0].amountFrm == 0 ? (psd.data[0].amountFrm.toString() + " ₫") : psd.data[0].amountFrm;
                    var amountpaidFrm = psd.data[0].amountpaidFrm == 0 ? (psd.data[0].amountpaidFrm.toString() + " ₫") : psd.data[0].amountpaidFrm;
                    var waiverInterestFrm = psd.data[0].waiverInterestFrm == 0 ? (psd.data[0].waiverInterestFrm.toString() + " ₫") : psd.data[0].waiverInterestFrm;

                    var FullName = psd.data[0].name;
                    var lenName = FullName.length;
                    var ShortName = FullName;
                    if (lenName > MaxLengthTextPhase)
                        ShortName = FullName.substr(0, MaxLengthTextPhase - 3) + '...';

                    html += '<tr>'
                                + '<td ><span>' + psd.data[0].stt + '</span></td>'
                                + '<td ><span class="tooltip" title="' + FullName + '">' + ShortName + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountpaidFrm + '</span></td>'

                                + '<td style="text-align: right; min-width: 80px;"><span>' + waiverInterestFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountneedFrm + " ₫" + '</span></td>'
                                + '<td style="text-align: center;"><span>' + psd.data[0].statuscodename + '</span></td>';

                    html += '<td style="text-align: center;"><span><input name="ckb2" class="ckb2" type="checkbox"'
                                        + 'value="' + psd.data[0].id + '" id="IS-' + psd.data[0].id + '" style="height: 14px;width: 14px;vertical-align: middle;" '
                                        + 'onclick="InputCheckSubGrid(this,' + gridType + ')" /></span></td>';
                    html += '</tr>';
                }

                html += '</tbody>'
                            + '</table>'
                        + '</div>';
                html += '</td>';
            }
            else if (sectionName == "SubGrid_Fees") {
                html += '<td width="100%">';
                html += '<div id="tree-container-' + divId + '" style="display:block;position:relative;width: 100%;">'
                                + '<table id="GridFees" name="GridFees" class="table" width="100%" cellpadding="0" cellspacing="0" border="0">'
                                    + '<tbody>'

                html += '<tr></tr>';


                html += '<tr>'
                        + '<th width="6%"><span>No.</span></th>'
                        + '<th width="18%"><span>Fee</span></th>'
                        + '<th width="20%" style="text-align:center"><span>Fee Amount</span></th>'
                        + '<th width="20%" style="text-align:center"><span>Amount Paid</span></th>'
                        + '<th width="16%" style="text-align:center"><span>Amount</span></th>'
                        + '<th width="11%" style="text-align:center"><span>Status</span></th>'
                        + '<th width="9%"  style="text-align:center"><span>Selection</span></th>'
                    + '</tr>';
                for (var i = 0; i < detail.length; i++) {
                    var psd = detail[i];


                    var amountneed = psd.data[0].amountneed;
                    var amountneedFrm = amountneed.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.");
                    var amountFrm = psd.data[0].amountFrm == 0 ? (psd.data[0].amountFrm.toString() + " ₫") : psd.data[0].amountFrm;
                    var amountpaidFrm = psd.data[0].amountpaidFrm == 0 ? (psd.data[0].amountpaidFrm.toString() + " ₫") : psd.data[0].amountpaidFrm;

                    var name = psd.data[0].name;

                    html += '<tr>'
                                + '<td ><span>' + psd.data[0].stt + '</span></td>'
                                + '<td ><span class="tooltip" title="' + name + '">' + name + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountpaidFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountneedFrm + " ₫" + '</span></td>'
                                + '<td style="text-align: center;"><span>' + psd.data[0].statuscodename + '</span></td>';

                    html += '<td style="text-align: center;"><span><input name="ckb2" class="ckb2" type="checkbox"'
                                        + 'value="' + psd.data[0].id + '" id="' + psd.data[0].id + '" style="height: 14px;width: 14px;vertical-align: middle;" '
                                        + 'onclick="InputCheckSubGrid(this,' + gridType + ')" /></span></td>';
                    html += '</tr>';
                }

                html += '</tbody>'
                            + '</table>'
                        + '</div>';
                html += '</td>';
            }
            else if (sectionName == "SubGrid_Miscellaneous") {
                html += '<td width="100%">';
                html += '<div id="tree-container-' + divId + '" style="display:block;position:relative;width: 100%;">'
                                + '<table id="GridMiscellaneous" name="GridMiscellaneous" class="table" width="100%" cellpadding="0" cellspacing="0" border="0">'
                                    + '<tbody>'

                html += '<tr></tr>';


                html += '<tr>'
                        + '<th width="5%"><span>No.</span></th>'
                        + '<th width="15%"><span>Name</span></th>'
                        + '<th width="10%" style="text-align:center"><span>Installment</span></th>'
                        + '<th width="10%" style="text-align:center"><span>Amount</span></th>'
                       // + '<th width="10%" style="text-align:center"><span>Waiver</span></th>'
                        + '<th width="10%" style="text-align:center"><span>Paid Amount</span></th>'
                        + '<th width="10%" style="text-align:center"><span>Amount Apply</span></th>'
                        + '<th width="10%" style="text-align:center"><span>Status</span></th>'
                        + '<th width="9%"  style="text-align:center"><span>Check</span></th>'
                    + '</tr>';

                for (var i = 0; i < detail.length; i++) {
                    var psd = detail[i];

                    var installment = psd.data[0].installment == 0 ? psd.data[0].installment.toString() : ("Installment " + psd.data[0].installment.toString());
                    var amountFrm = psd.data[0].amountFrm == 0 ? (psd.data[0].amountFrm.toString() + " ₫") : psd.data[0].amountFrm;
                    var amountpaidFrm = psd.data[0].amountpaidFrm == 0 ? (psd.data[0].amountpaidFrm.toString() + " ₫") : psd.data[0].amountpaidFrm;
                    var balanceFrm = psd.data[0].balanceFrm == 0 ? (psd.data[0].balanceFrm.toString() + " ₫") : psd.data[0].balanceFrm;
                    var waiverFrm = psd.data[0].waiverFrm == 0 ? (psd.data[0].waiverFrm.toString() + " ₫") : psd.data[0].waiverFrm;
                    var amountneed = psd.data[0].amountNeed;
                    var amountneedFrm = amountneed.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.");

                    var FullName = psd.data[0].name;
                    var lenName = FullName.length;
                    var ShortName = FullName;
                    if (lenName > MaxLengthTextPhase)
                        ShortName = FullName.substr(0, MaxLengthTextPhase - 3) + '...';

                    html += '<tr>'
                                + '<td ><span>' + psd.data[0].stt + '</span></td>'
                                + '<td ><span class="tooltip" title="' + FullName + '">' + ShortName + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + installment + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountFrm + '</span></td>'
                               // + '<td style="text-align: right; min-width: 80px;"><span>' + waiverFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountpaidFrm + '</span></td>'
                                + '<td style="text-align: right; min-width: 80px;"><span>' + amountneedFrm + " ₫" + '</span></td>'
                                + '<td style="text-align: center;"><span>' + psd.data[0].statuscodename + '</span></td>';

                    html += '<td style="text-align: center;"><span><input name="ckb2" class="ckb2" type="checkbox"'
                                        + 'value="' + psd.data[0].id + '" id="MIS-' + psd.data[0].id + '" style="height: 14px;width: 14px;vertical-align: middle;" '
                                        + 'onclick="InputCheckSubGrid(this,' + gridType + ')" /></span></td>';
                    html += '</tr>';
                }

                html += '</tbody>'
                            + '</table>'
                        + '</div>';
                html += '</td>';
            }
        }
        return html;
    };

    var Load = function (gridType, sort) {
        debugger;
        prg.start();
        var detail = new Array();

        var filter = '';
        if (gridType == 2) {
            var currentIstallment = Xrm.Page.getAttribute("bsd_paymentschemedetail").getValue();
            var option = Xrm.Page.getAttribute("bsd_optionentry").getValue()
            if (option && currentIstallment) {
                if (sectionName == "SubGrid_PSD") {
                    var fetchXML = '<?xml version="1.0" encoding="utf-8"?>'
                               + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" returntotalrecordcount="true" >'
                                    + '<entity name="bsd_paymentschemedetail">'
                                        + '<attribute name="bsd_paymentschemedetailid" />'
                                        + '<attribute name="bsd_ordernumber" />'
                                        + '<attribute name="bsd_name" />'
                                        + '<attribute name="bsd_waiveramount" />'
                                        + '<attribute name="bsd_waiverinstallment" />'
                                        + '<attribute name="bsd_depositamount" />'
                                        + '<attribute name="bsd_amountofthisphase" />'
                                        + '<attribute name="bsd_amountwaspaid" />'
                                        + '<attribute name="statuscode" />'
                                        + '<order attribute="statuscode" descending="' + !sort + '"/>' // false : Not Paid truoc ; true : paid truoc
                                        + '<order attribute="bsd_ordernumber" descending="false"/>'
                                        + '<filter>'
                                            + '<condition attribute="bsd_optionentry" operator="eq" value="' + option[0].id + '" />'
                                            + '<condition attribute="bsd_paymentschemedetailid" operator="ne" value="' + currentIstallment[0].id + '" />'
                                        + '</filter>'
                                    + '</entity>'
                                + '</fetch>';
                    CrmFetchKit.Fetch(fetchXML, false).then(
                    function (rs) {
                        var len = rs.length;
                        if (len > 0) {
                            for (var i = 0; i < rs.length; i++) {
                                var att = rs[i].attributes;

                                var id = att.bsd_paymentschemedetailid ? att.bsd_paymentschemedetailid.value : null;
                                var name = att.bsd_name ? att.bsd_name.value : '';
                                var stt = att.bsd_ordernumber ? att.bsd_ordernumber.value : '';

                                var amount = att.bsd_amountofthisphase ? att.bsd_amountofthisphase.value : 0;
                                var amountFrm = att.bsd_amountofthisphase ? att.bsd_amountofthisphase.formattedValue : 0;

                                var amountpaid = att.bsd_amountwaspaid ? att.bsd_amountwaspaid.value : 0;
                                var amountpaidFrm = att.bsd_amountwaspaid ? att.bsd_amountwaspaid.formattedValue : 0;

                                var waiver = att.bsd_waiverinstallment ? att.bsd_waiverinstallment.value : 0;
                                var waiverFrm = att.bsd_waiverinstallment ? att.bsd_waiverinstallment.formattedValue : 0;

                                var deposit = att.bsd_depositamount ? att.bsd_depositamount.value : 0;
                                var depositFrm = att.bsd_depositamount ? att.bsd_depositamount.formattedValue : 0;

                                var statuscode = att.statuscode ? att.statuscode.value : null;
                                var statuscodename = att.statuscode ? att.statuscode.formattedValue : null;

                                var amountneed = statuscode == 100000001 ? 0 : amount - amountpaid - deposit - waiver; //statuscode == paid

                                var obj = {
                                    'id': id
                                    , 'name': name
                                    , 'stt': stt
                                    , 'amountFrm': amountFrm
                                    , 'amountpaidFrm': amountpaidFrm
                                    , 'amountneed': amountneed
                                    , 'depositFrm': depositFrm
                                    , 'waiverFrm': waiverFrm
                                    , 'statuscode': statuscode
                                    , 'statuscodename': statuscodename
                                };

                                detail.push({ 'name': obj.name, 'data': [{ 'id': obj.id, 'name': obj.name, 'stt': obj.stt, 'waiverFrm': obj.waiverFrm, 'amountFrm': obj.amountFrm, 'depositFrm': obj.depositFrm, 'amountpaidFrm': obj.amountpaidFrm, 'amountneed': obj.amountneed, 'statuscode': obj.statuscode, 'statuscodename': obj.statuscodename }] });
                            }

                            prg.stop();
                            if (detail.length > 0) {
                                tr.html(BuildSubGrid(gridType, detail));
                                LoadArrayCheck(gridType, sectionName);
                                //var index = $(tr).index();
                                //LoadEventDouble(gridType, index);
                                CheckAmountInput(gridType);
                            }
                            else {
                                tr.children('td:first-child').html('Not Found Payment Scheme Detail');
                            }
                            var pr = tr.parents("div[id$='_content']:eq(0)");
                            var clearFloat = $("<div>").attr('class', 'clear-float').css({ 'display': 'block', 'clear': 'both', 'font-size': '0px' });
                            if (pr.children(".clear-float").size() <= 0) {
                                pr.append(clearFloat);
                                pr.css('overflow', 'visible');
                            }
                        } else {
                            // Ẩn section Installment List
                            Xrm.Page.ui.tabs.get('tab_installment').sections.get('SubGrid_PSD').setVisible(false);
                        }
                    }
                    , function (err) {
                        console.log(err);
                    });
                }
                else if (sectionName == "SubGrid_Interest") {
                    var fetchXML = '<?xml version="1.0" encoding="utf-8"?>'
                               + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" returntotalrecordcount="true" >'
                                    + '<entity name="bsd_paymentschemedetail">'
                                        + '<attribute name="bsd_paymentschemedetailid" />'
                                        + '<attribute name="bsd_ordernumber" />'
                                        + '<attribute name="bsd_name" />'
                                        + '<attribute name="bsd_waiverinterest" />'
                                        + '<attribute name="bsd_interestchargeamount" />'
                                        + '<attribute name="bsd_interestwaspaid" />'
                                        + '<attribute name="bsd_interestchargestatus" />'
                                        + '<order attribute="bsd_ordernumber" descending="false"/>' // false : Not Paid truoc ; true : paid truoc
                                        + '<filter type="and">'
                                            + '<condition attribute="bsd_optionentry" operator="eq" value="' + option[0].id + '" />'
                                            //+ '<condition attribute="bsd_interestchargestatus" operator="eq" value="100000000" />'
                                            + '<condition attribute="bsd_interestchargeamount" operator="gt" value="0" />'
                                        + '</filter>'
                                    + '</entity>'
                                + '</fetch>';
                    CrmFetchKit.Fetch(fetchXML, false).then(
                    function (rs) {
                        var len = rs.length;
                        if (len > 0) {
                            for (var i = 0; i < rs.length; i++) {
                                var att = rs[i].attributes;

                                var id = att.bsd_paymentschemedetailid ? att.bsd_paymentschemedetailid.value : null;
                                var name = att.bsd_name ? att.bsd_name.value : '';
                                var stt = (i + 1);

                                var amount = att.bsd_interestchargeamount ? att.bsd_interestchargeamount.value : 0;
                                var amountFrm = att.bsd_interestchargeamount ? att.bsd_interestchargeamount.formattedValue : 0;

                                var amountpaid = att.bsd_interestwaspaid ? att.bsd_interestwaspaid.value : 0;
                                var amountpaidFrm = att.bsd_interestwaspaid ? att.bsd_interestwaspaid.formattedValue : 0;

                                var statuscode = att.bsd_interestchargestatus ? att.bsd_interestchargestatus.value : null;
                                var statuscodename = att.bsd_interestchargestatus ? att.bsd_interestchargestatus.formattedValue : null;

                                var waiverInterest = att.bsd_waiverinterest ? att.bsd_waiverinterest.value : null;
                                var waiverInterestFrm = att.bsd_waiverinterest ? att.bsd_waiverinterest.formattedValue : 0;

                                var amountneed = statuscode == 100000001 ? 0 : amount - amountpaid - waiverInterest; //statuscode == paid

                                var obj = {
                                    'id': id
                                    , 'name': name
                                    , 'stt': stt
                                    , 'amountFrm': amountFrm
                                    , 'amountpaidFrm': amountpaidFrm
                                    , 'waiverInterestFrm': waiverInterestFrm
                                    , 'amountneed': amountneed
                                    , 'statuscode': statuscode
                                    , 'statuscodename': statuscodename
                                };

                                detail.push({ 'name': obj.name, 'data': [{ 'id': obj.id, 'name': obj.name, 'stt': obj.stt, 'amountFrm': obj.amountFrm, 'amountpaidFrm': obj.amountpaidFrm, 'amountneed': obj.amountneed, 'waiverInterestFrm': waiverInterestFrm, 'statuscode': obj.statuscode, 'statuscodename': obj.statuscodename }] });
                            }

                            prg.stop();
                            if (detail.length > 0) {
                                tr.html(BuildSubGrid(gridType, detail));
                                LoadArrayCheck(gridType, sectionName);
                                //var index = $(tr).index();
                                //LoadEventDouble(gridType, index);
                                CheckAmountInput(gridType);
                            }
                            else {
                                tr.children('td:first-child').html('Not Found Payment Scheme Detail');
                            }
                            var pr = tr.parents("div[id$='_content']:eq(0)");
                            var clearFloat = $("<div>").attr('class', 'clear-float').css({ 'display': 'block', 'clear': 'both', 'font-size': '0px' });
                            if (pr.children(".clear-float").size() <= 0) {
                                pr.append(clearFloat);
                                pr.css('overflow', 'visible');
                            }
                        } else {
                            Xrm.Page.ui.tabs.get('tab_installment').sections.get('SubGrid_Interest').setVisible(false);
                        }

                    }
                    , function (err) {
                        console.log(err);
                    });
                }
                else if (sectionName == "SubGrid_Fees") {
                    var fetchXML = '<?xml version="1.0" encoding="utf-8"?>'
                               + '<fetch version="1.0" output-format="xml-platform" mapping="logical" count="1" distinct="true" returntotalrecordcount="true" >'
                                    + '<entity name="bsd_paymentschemedetail">'
                                        + '<attribute name="bsd_paymentschemedetailid" />'
                                        + '<attribute name="bsd_maintenanceamount" />'
                                        + '<attribute name="bsd_maintenancefeepaid" />'
                                        + '<attribute name="bsd_maintenancefeesstatus" />'
                                        + '<attribute name="bsd_managementfeesstatus" />'
                                        + '<attribute name="bsd_managementamount" />'
                                        + '<attribute name="bsd_managementfeepaid" />'
                                        + '<filter type="and">'
                                            + '<condition attribute="bsd_optionentry" operator="eq" value="' + option[0].id + '" />'
                                            + '<condition attribute="bsd_duedatecalculatingmethod" operator="eq" value="100000002" />'//Estimate handover date
                                        + '</filter>'
                                    + '</entity>'
                                + '</fetch>';
                    CrmFetchKit.Fetch(fetchXML, false).then(function (rs) {
                        if (rs.length > 0) {
                            var att = rs[0].attributes;

                            var id = 1;
                            var name = 'Maintenance Fee';
                            var stt = 1;
                            var amount = att.bsd_maintenanceamount ? att.bsd_maintenanceamount.value : 0;
                            var amountFrm = att.bsd_maintenanceamount ? att.bsd_maintenanceamount.formattedValue : 0;
                            var amountpaid = att.bsd_maintenancefeepaid ? att.bsd_maintenancefeepaid.value : 0;
                            var amountpaidFrm = att.bsd_maintenancefeepaid ? att.bsd_maintenancefeepaid.formattedValue : 0;
                            var statuscode = att.bsd_maintenancefeesstatus ? att.bsd_maintenancefeesstatus.value : null;
                            var statuscodename = att.bsd_maintenancefeesstatus ? att.bsd_maintenancefeesstatus.formattedValue : null;
                            var amountneed = amount - amountpaid; //statuscode == paid

                            if (amount != 0) {
                                var obj = {
                                    'id': id
                                , 'name': name
                                , 'stt': stt
                                , 'amountFrm': amountFrm
                                , 'amountpaidFrm': amountpaidFrm
                                , 'amountneed': amountneed
                                , 'statuscode': statuscode
                                , 'statuscodename': statuscodename
                                };
                                detail.push({ 'name': obj.name, 'data': [{ 'id': obj.id, 'name': obj.name, 'stt': obj.stt, 'amountFrm': obj.amountFrm, 'amountpaidFrm': obj.amountpaidFrm, 'amountneed': obj.amountneed, 'statuscode': obj.statuscode, 'statuscodename': obj.statuscodename }] });
                            }

                            id = 2;
                            name = 'Management Fee';
                            stt = 2;
                            amount = att.bsd_managementamount ? att.bsd_managementamount.value : 0;
                            amountFrm = att.bsd_managementamount ? att.bsd_managementamount.formattedValue : 0;
                            amountpaid = att.bsd_managementfeepaid ? att.bsd_managementfeepaid.value : 0;
                            amountpaidFrm = att.bsd_managementfeepaid ? att.bsd_managementfeepaid.formattedValue : 0;
                            statuscode = att.bsd_managementfeesstatus ? att.bsd_managementfeesstatus.value : null;
                            statuscodename = att.bsd_managementfeesstatus ? att.bsd_managementfeesstatus.formattedValue : null;
                            amountneed = amount - amountpaid; //statuscode == paid

                            if (amount != 0) {
                                obj = {
                                    'id': id
                                , 'name': name
                                , 'stt': stt
                                , 'amountFrm': amountFrm
                                , 'amountpaidFrm': amountpaidFrm
                                , 'amountneed': amountneed
                                , 'statuscode': statuscode
                                , 'statuscodename': statuscodename
                                };

                                detail.push({ 'name': obj.name, 'data': [{ 'id': obj.id, 'name': obj.name, 'stt': obj.stt, 'amountFrm': obj.amountFrm, 'amountpaidFrm': obj.amountpaidFrm, 'amountneed': obj.amountneed, 'statuscode': obj.statuscode, 'statuscodename': obj.statuscodename }] });
                            }

                            if (detail.length <= 0)
                                Xrm.Page.ui.tabs.get('tab_installment').sections.get('SubGrid_Fees').setVisible(false);
                        }

                        prg.stop();
                        if (detail.length > 0) {
                            tr.html(BuildSubGrid(gridType, detail));
                            LoadArrayCheck(gridType, sectionName);
                            //var index = $(tr).index();
                            //LoadEventDouble(gridType, index);
                            CheckAmountInput(gridType);
                        }
                        else {
                            tr.children('td:first-child').html('Not Found Payment Scheme Detail');
                        }
                        var pr = tr.parents("div[id$='_content']:eq(0)");
                        var clearFloat = $("<div>").attr('class', 'clear-float').css({ 'display': 'block', 'clear': 'both', 'font-size': '0px' });
                        if (pr.children(".clear-float").size() <= 0) {
                            pr.append(clearFloat);
                            pr.css('overflow', 'visible');
                        }
                    }
                    , function (err) {
                        console.log(err);
                    });//Miscell
                } else if (sectionName == "SubGrid_Miscellaneous") {
                    var fetchXML = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">'
                                    + '<entity name="bsd_miscellaneous">'
                                        + '<attribute name="bsd_miscellaneousid" />'
                                        + '<attribute name="bsd_name" />'
                                        + '<attribute name="statuscode" />'
                                        + '<attribute name="bsd_paidamount" />'
                                        + '<attribute name="bsd_installmentnumber" />'
                                        + '<attribute name="bsd_waiveramount" />'
                                        + '<attribute name="bsd_balance" />'
                                        + '<attribute name="bsd_amount" />'
                                        + '<attribute name="createdon" />'
                                    + '<order attribute="createdon" descending="false" />'
                                    + '<filter type="and">'
                                         + '<condition attribute="bsd_optionentry" operator="eq" value="' + option[0].id + '"/>'
                                    + '</filter>'
                                + '</entity>'
                             + '</fetch>'
                    CrmFetchKit.Fetch(fetchXML, false).then(function (rs) {
                        if (rs.length > 0) {
                            var len = rs.length;
                            for (var i = 0; i < rs.length; i++) {
                                var att = rs[i].attributes;

                                var id = att.bsd_miscellaneousid ? att.bsd_miscellaneousid.value : null;
                                var name = att.bsd_name ? att.bsd_name.value : '';
                                var stt = (i + 1);

                                var installment = att.bsd_installmentnumber ? att.bsd_installmentnumber.value : 0;

                                var amountFrm = att.bsd_amount ? att.bsd_amount.formattedValue : 0;
                                var amountpaidFrm = att.bsd_paidamount ? att.bsd_paidamount.formattedValue : 0;
                                var balanceFrm = att.bsd_balance ? att.bsd_balance.formattedValue : 0;
                                var waiverFrm = att.bsd_waiveramount ? att.bsd_waiveramount.formattedValue : 0;

                                var amount = att.bsd_amount ? att.bsd_amount.value : 0;
                                var waiver = att.bsd_waiveramount ? att.bsd_waiveramount.value : 0;
                                var amountpaid = att.bsd_paidamount ? att.bsd_paidamount.value : 0;
                                var amountNeed = amount - waiver - amountpaid;

                                var statuscode = att.statuscode ? att.statuscode.value : null;
                                var statuscodename = att.statuscode ? att.statuscode.formattedValue : null;

                                var obj = {
                                    'id': id
                                    , 'name': name
                                    , 'stt': stt
                                    , 'installment': installment
                                    , 'amountFrm': amountFrm
                                    , 'amountNeed': amountNeed
                                    , 'balanceFrm': balanceFrm
                                    , 'amountpaidFrm': amountpaidFrm
                                    , 'waiverFrm': waiverFrm
                                    , 'statuscode': statuscode
                                    , 'statuscodename': statuscodename
                                };

                                detail.push({
                                    'name': obj.name,
                                    'data':
                                        [{
                                            'id': obj.id,
                                            'name': obj.name,
                                            'stt': obj.stt,
                                            'installment': obj.installment,
                                            'amountFrm': obj.amountFrm,
                                            'amountNeed': obj.amountNeed,
                                            'amountpaidFrm': obj.amountpaidFrm,
                                            'balanceFrm': obj.balanceFrm,
                                            'waiverFrm': obj.waiverFrm,
                                            'statuscode': obj.statuscode,
                                            'statuscodename': obj.statuscodename
                                        }]
                                });
                            }

                            prg.stop();

                            //an neu k co record nao
                            Xrm.Page.ui.tabs.get('tab_installment').sections.get('SubGrid_Miscellaneous').setVisible(detail.length > 0);
                            if (detail.length > 0) {
                                tr.html(BuildSubGrid(gridType, detail));
                                LoadArrayCheck(gridType, sectionName);
                                var index = $(tr).index();
                                LoadEventDouble(gridType, index);
                                CheckAmountInput(gridType);
                            }
                            else {
                                tr.children('td:first-child').html('Not Found Payment Scheme Detail');
                            }
                            var pr = tr.parents("div[id$='_content']:eq(0)");
                            var clearFloat = $("<div>").attr('class', 'clear-float').css({ 'display': 'block', 'clear': 'both', 'font-size': '0px' });
                            if (pr.children(".clear-float").size() <= 0) {
                                pr.append(clearFloat);
                                pr.css('overflow', 'visible');
                            }
                        }
                    },
                function (er) {
                    console.log(er.message)
                });
                }
            }
        }
    };

    var Listen = function (gridType, sort) {
        var entity = null;
        if (gridType == 1 || gridType == 3) {
            entity = Xrm.Page.getAttribute("bsd_customer").getValue() != null ? Xrm.Page.getAttribute("bsd_customer").getValue()[0] : null;
        }
        else
            entity = Xrm.Page.getAttribute("bsd_optionentry").getValue() != null ? Xrm.Page.getAttribute("bsd_optionentry").getValue()[0] : null;
        tr.children('td:first-child')
        if (entity != null) {
            if (timer != null)
                clearTimeout(timer);
            if (entity != null) {
                Load(gridType, sort);
            }
            else
                BuildSubGrid([]);
        }
        else {
            timer = setTimeout(Listen, 1000);
        }
    };
    Listen(gridType, sort);
};

//---------------------------------- LOAD PRODUCT CHECKED ----------------------------------

function LoadArrayCheck(gridType, sectionName) {

    var statuscodeCurr = Xrm.Page.getAttribute("statuscode").getValue();
    var bool = !(statuscodeCurr == 100000000 || statuscodeCurr == 100000001 || statuscodeCurr == 100000002);

    var applyID = Xrm.Page.data.entity.getId();

    if (applyID != null) {

        var arrChecked = new Array();
        var arrAmount = new Array();

        if (gridType == 1) {
            var advID = Xrm.Page.getAttribute("bsd_arrayadvancepayment").getValue() ? Xrm.Page.getAttribute("bsd_arrayadvancepayment").getValue() : "";
            var advAmount = Xrm.Page.getAttribute("bsd_arrayamountadvance").getValue() ? Xrm.Page.getAttribute("bsd_arrayamountadvance").getValue() : "";
            arrChecked = advID.split(',');
            arrAmount = advAmount.split(',');
            if (advID != "") {
                for (var i = 0; i < arrChecked.length; i++) {
                    var tmp = $('.ckb1[value="' + arrChecked[i] + '"]');
                    // Amount Input 
                    var amount = arrAmount[i];
                    var span = $(tmp).closest('tr').children('td').eq(4).children('span');
                    span.html(amount.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫");

                    //if (statuscodeCurr == 100000000) {
                    //    var parent = tmp.parent();
                    //    parent.html("Selected");
                    //}
                    //else {
                    //check box                   
                    tmp.attr("checked", "checked");
                    //Event
                    var index = $(tmp.closest('tr')).index();
                    if (index > 0 && bool)
                        LoadEventDouble(gridType, index);
                }
            }
            var arrAllInput = $('.ckb1');
            if (arrAllInput.length > 0) {
                for (var x = 0; x < arrAllInput.length; x++) {
                    var input = arrAllInput[x];
                    var inputValue = arrAllInput[x].value;
                    if (CheckPaid(gridType, inputValue, "")) {
                        if (arrChecked.indexOf(inputValue) < 0) {
                            var parent = input.parentElement;
                            parent.innerHTML = "Pay Off";
                        }
                        else input.disabled = true;
                    }

                }
            }
        }
        else if (gridType == 2) {
            if (sectionName == "SubGrid_PSD") {
                var aplArrID = Xrm.Page.getAttribute("bsd_arraypsdid").getValue() ? Xrm.Page.getAttribute("bsd_arraypsdid").getValue() : "";
                var aplArrAmount = Xrm.Page.getAttribute("bsd_arrayamountpay").getValue() ? Xrm.Page.getAttribute("bsd_arrayamountpay").getValue() : "";
                arrChecked = aplArrID.split(',');
                arrAmount = aplArrAmount.split(',');
                if (aplArrID != "") {
                    for (var i = 0; i < arrChecked.length; i++) {
                        var tmp = $('.ckb2[value="' + arrChecked[i] + '"]');
                        for (var j = 0; j < tmp.length; j++) {
                            var tableName = tmp[j].closest('table').getAttribute("id");
                            if (tableName == "GridPhase") {//Amount Input 
                                var amount = arrAmount[i];
                                var span = $(tmp[j]).closest('tr').children('td').eq(6).children('span');
                                span.html(amount.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫");
                                tmp[j].checked = "checked";
                                //Event
                                var index = $(tmp[j].closest('tr')).index();
                                if (index > 0 && bool)
                                    LoadEventDouble(gridType, index, tableName);
                            }
                        }
                    }
                }

                var arrAllInput = $('.ckb2');
                if (arrAllInput.length > 0) {
                    for (var x = 0; x < arrAllInput.length; x++) {
                        var input = arrAllInput[x];
                        if (input.closest('table').id == "GridPhase") {
                            var inputValue = arrAllInput[x].value;
                            var parent = input.parentElement;
                            var Newstatus = CheckPaid(gridType, inputValue, "Installment");
                            if (Newstatus == true) {
                                if (arrChecked.indexOf(inputValue) < 0) {
                                    var parent = input.parentElement;
                                    parent.innerHTML = "Paid";
                                }
                                else input.disabled = true;
                            }
                        }
                    }
                }
            }

            else if (sectionName == "SubGrid_Interest") {
                var interestArrID = Xrm.Page.getAttribute("bsd_arrayinstallmentinterest").getValue() ? Xrm.Page.getAttribute("bsd_arrayinstallmentinterest").getValue() : "";
                var interestArrAmount = Xrm.Page.getAttribute("bsd_arrayinterestamount").getValue() ? Xrm.Page.getAttribute("bsd_arrayinterestamount").getValue() : "";
                if (interestArrID && interestArrID.length > 0) {
                    arrChecked = interestArrID.split(',');
                    arrAmount = interestArrAmount.split(',');

                    for (var i = 0; i < arrChecked.length; i++) {
                        var tmp = $('.ckb2[id="IS-' + arrChecked[i] + '"]');
                        if (tmp.length > 0) {//Amount Input 
                            var amount = arrAmount[i];
                            var span = $(tmp).closest('tr').children('td').eq(5).children('span');
                            span.html(amount.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫");
                            tmp.attr("checked", "checked");

                            var index = $(tmp.closest('tr')).index();
                            var tableName = tmp.closest('table')[0].id;
                            if (index > 0 && bool)
                                LoadEventDouble(gridType, index, tableName);
                        }
                    }
                }
                var arrAllInput = $('.ckb2');
                if (arrAllInput.length > 0) {
                    for (var x = 0; x < arrAllInput.length; x++) {
                        var input = arrAllInput[x];
                        if (input.closest('table').id == "GridInterest") {
                            var inputValue = arrAllInput[x].value;
                            var parent = input.parentElement;
                            var Newstatus = CheckPaid(gridType, inputValue, "Interest");
                            if (Newstatus == true) {
                                if (arrChecked.indexOf(inputValue) < 0) {
                                    var parent = input.parentElement;
                                    parent.innerHTML = "Paid";
                                }
                                else input.disabled = true;
                            }
                        }
                    }
                }
            }
            else if (sectionName == "SubGrid_Fees") {
                var ArrFeesID = Xrm.Page.getAttribute("bsd_arrayfees").getValue() ? Xrm.Page.getAttribute("bsd_arrayfees").getValue() : "";
                var ArrAmount = Xrm.Page.getAttribute("bsd_arrayfeesamount").getValue() ? Xrm.Page.getAttribute("bsd_arrayfeesamount").getValue() : "";
                if (ArrFeesID && ArrFeesID.length > 0) {
                    arrChecked = ArrFeesID.split(',');
                    arrAmount = ArrAmount.split(',');

                    for (var i = 0; i < arrChecked.length; i++) {
                        var tmp = $('.ckb2[value="' + arrChecked[i] + '"]');
                        if (tmp.length > 0) {
                            //Amount Input 
                            var amount = arrAmount[i];
                            var span = $(tmp).closest('tr').children('td').eq(4).children('span');
                            span.html(amount.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫");


                            tmp.attr("checked", "checked");

                            var index = $(tmp.closest('tr')).index();
                            var tableName = tmp.closest('table')[0].id;
                            if (index > 0 && bool)
                                LoadEventDouble(gridType, index, tableName);
                        }
                    }
                }
                var arrAllInput = $('.ckb2');
                if (arrAllInput.length > 0) {
                    for (var x = 0; x < arrAllInput.length; x++) {
                        var input = arrAllInput[x];
                        if (input.closest('table').id == "GridFees") {
                            debugger;
                            var inputValue = arrAllInput[x].value;
                            var parent = input.parentElement;
                            var type = input.closest("tr").childNodes[1].textContent.split(" ")[0];
                            var Newstatus = CheckPaid(gridType, inputValue, type);
                            if (Newstatus == true) {
                                if (arrChecked.indexOf(inputValue) < 0) {
                                    var parent = input.parentElement;
                                    parent.innerHTML = "Paid";
                                }
                                else input.disabled = true;
                            }
                        }
                    }
                }
            }
            else if (sectionName == "SubGrid_Miscellaneous") {
                var missArrID = Xrm.Page.getAttribute("bsd_arraymicellaneousid").getValue() ? Xrm.Page.getAttribute("bsd_arraymicellaneousid").getValue() : "";
                var misArrAmount = Xrm.Page.getAttribute("bsd_arraymicellaneousamount").getValue() ? Xrm.Page.getAttribute("bsd_arraymicellaneousamount").getValue() : "";
                if (missArrID && missArrID.length > 0) {
                    arrChecked = missArrID.split(',');
                    arrAmount = misArrAmount.split(',');

                    for (var i = 0; i < arrChecked.length; i++) {
                        var tmp = $('.ckb2[id="MIS-' + arrChecked[i] + '"]');
                        if (tmp.length > 0) {//Amount Input 
                            var amount = arrAmount[i];
                            var span = $(tmp).closest('tr').children('td').eq(6).children('span');
                            span.html(amount.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫");
                            tmp.attr("checked", "checked");

                            var index = $(tmp.closest('tr')).index();
                            var tableName = tmp.closest('table')[0].id;
                            if (index > 0 && bool)
                                LoadEventDouble(gridType, index, tableName);
                        }
                    }
                }
                var arrAllInput = $('.ckb2');
                if (arrAllInput.length > 0) {
                    for (var x = 0; x < arrAllInput.length; x++) {
                        var input = arrAllInput[x];
                        if (input.closest('table').id == "GridMiscellaneous") {
                            var inputValue = arrAllInput[x].value;
                            var parent = input.parentElement;
                            var Newstatus = CheckPaid(gridType, inputValue, "Miscellaneous");
                            if (Newstatus == true) {
                                if (arrChecked.indexOf(inputValue) < 0) {
                                    var parent = input.parentElement;
                                    parent.innerHTML = "Paid";
                                }
                                else input.disabled = true;
                            }
                        }
                    }
                }
            }
        }
        else if (gridType == 3) {
            var aplArrID = Xrm.Page.getAttribute("bsd_arraypsdid").getValue() ? Xrm.Page.getAttribute("bsd_arraypsdid").getValue() : "";
            var aplArrAmount = Xrm.Page.getAttribute("bsd_arrayamountpay").getValue() ? Xrm.Page.getAttribute("bsd_arrayamountpay").getValue() : "";
            if (aplArrID) {
                arrChecked = aplArrID.split(',');
                arrAmount = aplArrAmount.split(',');

                for (var i = 0; i < arrChecked.length; i++) {
                    var tmp = $('.ckb2[value="' + arrChecked[i] + '"]');

                    //Amount Input 
                    var amount = arrAmount[i];
                    var span = $(tmp).closest('tr').children('td').eq(3).children('span');
                    span.html(amount.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫");

                    tmp.attr("checked", "checked");
                }
            }

            //var arrAllInput = $('.ckb2');

            //if (arrAllInput.length > 0) {
            //    for (var x = 0; x < arrAllInput.length; x++) {
            //        var input = arrAllInput[x];
            //        var inputValue = arrAllInput[x].value;

            //        for (var t = 0; t < arrChecked.length; t++) {
            //            var parent = input.parentElement;
            //            if (parent != null) {
            //                if (inputValue != arrChecked[t]) {
            //                    if (statuscodeCurr == 100000000)
            //                        parent.innerHTML = "none";
            //                    else if (CheckPaid(gridType, inputValue))
            //                        parent.innerHTML = "Paid";

            //                }
            //            }
            //        }
            //    }
            //}
        }
    }

}


//---------------------------------- CHECK PSD STATUSCODE  ----------------------------------

function CheckPaid(gridType, psdID, type) {
    var check = false;
    var fetchXML = null;

    if (gridType == 1) {
        var fetchXML = '<?xml version="1.0" encoding="utf-8"?>'
                       + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" returntotalrecordcount="true" >'
                            + '<entity name="bsd_advancepayment">'
                                + '<attribute name="statuscode" />'
                                + '<filter>'
                                + '<condition attribute="bsd_advancepaymentid" operator="eq" value="' + psdID + '" />'
                                + '</filter>'
                            + '</entity>'
                        + '</fetch>';
        CrmFetchKit.Fetch(fetchXML, false).then(
        function (rs) {
            //debugger;
            var len = rs.length;
            if (rs.length > 0) {
                var att = rs[0].attributes;
                var statuscode = att.statuscode ? att.statuscode.value : null;
                if (statuscode == 100000001)
                    check = true;
            }

        }, function (err) {
            console.log(err);
        });
    }
    else {

        var option = Xrm.Page.getAttribute("bsd_optionentry").getValue()
        var fetchXML = '<?xml version="1.0" encoding="utf-8"?>'
                           + '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true" returntotalrecordcount="true" >';
        if (type != "Miscellaneous")
            fetchXML += '<entity name="bsd_paymentschemedetail">';
        else {
            fetchXML += '<entity name="bsd_miscellaneous">'
            fetchXML += '<attribute name="statuscode" />'
          + '<filter>'
          + '<condition attribute="bsd_optionentry" operator="eq" value="' + option[0].id + '" />'
                  + '</filter>'
                 + '</entity>'
                + '</fetch>';
        }
        if (type == "Installment") {
            fetchXML += '<attribute name="statuscode" />'
          + '<filter>'
          + '<condition attribute="bsd_paymentschemedetailid" operator="eq" value="' + psdID + '" />'
                  + '</filter>'
                 + '</entity>'
                + '</fetch>';
        }
        else if (type == "Interest") {
            fetchXML += '<attribute name="bsd_interestchargestatus" />'
          + '<filter>'
          + '<condition attribute="bsd_paymentschemedetailid" operator="eq" value="' + psdID + '" />'
                  + '</filter>'
                 + '</entity>'
                + '</fetch>';
        }
        else if (type == "Maintenance")
            fetchXML += '<attribute name="bsd_maintenancefeesstatus" />';
        else if (type == "Management")
            fetchXML += '<attribute name="bsd_managementfeesstatus" />';
        if (type == "Maintenance" || type == "Management") {
            fetchXML += '<filter>'
              + '<condition attribute="bsd_optionentry" operator="eq" value="' + option[0].id + '" />'
              + '<condition attribute="bsd_duedatecalculatingmethod" operator="eq" value="100000002" />'//Estimate handover date
                      + '</filter>'
                     + '</entity>'
                    + '</fetch>';
        }
        CrmFetchKit.Fetch(fetchXML, false).then(
        function (rs) {
            //debugger;
            var len = rs.length;
            if (rs.length > 0) {
                var att = rs[0].attributes;
                if (type == "Miscellaneous") {
                    var statuscode = att.statuscode ? att.statuscode.value : null;
                    if (statuscode == 100000000)//PAID
                        check = true;
                }
                if (type == "Installment") {
                    var statuscode = att.statuscode ? att.statuscode.value : null;
                    if (statuscode == 100000001)//PAID
                        check = true;
                } if (type == "Interest") {
                    var statuscode = att.bsd_interestchargestatus ? att.bsd_interestchargestatus.value : null;
                    if (statuscode == 100000001)//PAID
                        check = true;
                }
                else if (type == "Maintenance") {
                    var statuscode = att.bsd_maintenancefeesstatus ? att.bsd_maintenancefeesstatus.value : null;
                    if (statuscode == 1)//PAID
                        check = true;
                }
                else if (type == "Management") {
                    var statuscode = att.bsd_managementfeesstatus ? att.bsd_managementfeesstatus.value : null;
                    if (statuscode == 1)//PAID
                        check = true;

                }
            }
        }, function (err) {
            console.log(err);
        });
    }
    return check;
}

//---------------------------------- LOAD CHECKED BUILD TREE  ---------------------------------- InputCheckPaid

window.parent.InputCheckSubGrid = function (element, gridType) {
    debugger;
    var strChecked = "";
    var strAmount = "";
    var itemCheck = "";
    var elementID = element.value ? element.value : "";

    var FieldArrayID = null;
    var FieldArrayAmount = null;

    if (gridType == 1) {
        FieldArrayID = "bsd_arrayadvancepayment";
        FieldArrayAmount = "bsd_arrayamountadvance";
    }
    else {
        if ($(element).closest('table')[0].id == "GridInterest") {
            FieldArrayID = "bsd_arrayinstallmentinterest";
            FieldArrayAmount = "bsd_arrayinterestamount";
        } else if ($(element).closest('table')[0].id == "GridMiscellaneous") {
            FieldArrayID = "bsd_arraymicellaneousid";
            FieldArrayAmount = "bsd_arraymicellaneousamount";
        }
        else if ($(element).closest('table')[0].id == "GridFees") {
            FieldArrayID = "bsd_arrayfeesid";
            FieldArrayAmount = "bsd_arrayfeesamount";
        }
        else  //($(element).closest('table')[0].id == "GridPhase") 
        {
            FieldArrayID = "bsd_arraypsdid";
            FieldArrayAmount = "bsd_arrayamountpay";
        }
    }

    if (Xrm.Page.getAttribute(FieldArrayID).getValue()) {
        strChecked = Xrm.Page.getAttribute(FieldArrayID).getValue();
    }

    if (Xrm.Page.getAttribute(FieldArrayAmount).getValue()) {
        strAmount = Xrm.Page.getAttribute(FieldArrayAmount).getValue();
    }

    var tableName = element.closest('table').id;
    if (element.checked) {
        // Array ID
        if (strChecked.length > 0)
            strChecked += "," + elementID;
        else
            strChecked += elementID;
        // Array Amount
        var tmp;
        if (gridType == 3)
            tmp = $(element).closest('tr').children('td').eq(3).children('span')["html"]();
        else if (tableName == "GridPhase")
            tmp = $(element).closest('tr').children('td').eq(6).children('span')["html"]();
        else if (tableName == "GridInterest")
            tmp = $(element).closest('tr').children('td').eq(5).children('span')["html"]();
        else tmp = $(element).closest('tr').children('td').eq(4).children('span')["html"]();
        tmp = tmp.trim().replace(/đ|₫| /gi, "");
        tmp = tmp.split(".").join("");

        if (strAmount.length > 0)
            strAmount += "," + tmp;
        else
            strAmount += tmp;

        //Event Double Click
        var index = $(element.closest('tr')).index();
        if (index > 0)
            LoadEventDouble(gridType, index, tableName);
    }
    else {
        if (strChecked.length >= 0) {
            var arrChecked = strChecked.split(',');
            var arrAmount = strAmount.split(',');

            var index = arrChecked.indexOf(elementID);
            if (index > -1) {
                arrChecked.splice(index, 1);
                arrAmount.splice(index, 1);
            }

            strChecked = arrChecked.toString();
            strAmount = arrAmount.toString();

            // Gán lại số tiền cần trả của đợt đó
            //var amountPhase = $(element).closest('tr').children('td').eq(2).children('span')["html"]();
            //if (amountPhase.length > 0) {
            //    amountPhase = amountPhase.trim().replace(/đ|₫| /gi, "")
            //    amountPhase = (amountPhase) ? (amountPhase).split(".").join("") : 0;
            //}
            //var amountPaid = $(element).closest('tr').children('td').eq(3).children('span')["html"]();
            //if (amountPaid.length > 0) {
            //    amountPaid = amountPaid.trim().replace(/đ|₫| /gi, "")
            //    amountPaid = (amountPaid) ? (amountPaid).split(".").join("") : 0;
            //}

            //var tmp = amountPhase - amountPaid;
            //$(element).closest('tr').children('td').eq(4).children('span')["html"](tmp.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,"));

            //Remove dblclick
            if (tableName == "GridPhase" || tableName == "GridMiscellaneous")
                $(element).closest('tr').children('td').eq(6).off();
            else if (tableName == "GridInterest")
                $(element).closest('tr').children('td').eq(5).off();

            else
                $(element).closest('tr').children('td').eq(4).off();
        }

    }

    Xrm.Page.getAttribute(FieldArrayID).setValue(strChecked);
    Xrm.Page.getAttribute(FieldArrayAmount).setValue(strAmount);

    CheckAmountInput(gridType);


}

//---------------------------------- INPUT CHECK PAID  ---------------------------------- 

window.parent.InputCheckSort = function (element) {
    debugger;
    var elementName = element.name;

    var sort = false;


    if (element.checked) sort = true;
    else if (element.checked == false) sort = false;

    if (elementName == 'ckbAdvance') SubGird(1, 'SubGrid_AdvancePhase_id', 'SubGrid_AdvancePhase', sort);
    else if (elementName == 'ckbInstallment') SubGird(2, 'SubGrid_PSD_id', 'SubGrid_PSD', sort);
    else if (elementName == 'ckbInterest') SubGird(2, 'SubGrid_Interest_id', 'SubGrid_Interest', sort);
    else if (elementName == 'ckbDeposit') SubGird(3, 'SubGrid_MultiDeposit_id', 'SubGrid_MultiDeposit', sort);
    var status = Xrm.Page.getAttribute("statuscode").getValue();
    if (status == 100000000 || status == 100000001 || status == 100000002) {
        LockCheckBox();
    }
}

//----------------------------------  CHECK TONG TIEN NHAP VOI TONG TIEN ADV PAYMENT  ----------------------------------

function CheckAmountInput(gridType) {
    debugger;
    Xrm.Page.ui.clearFormNotification('2');
    var amountAvd = Xrm.Page.getAttribute("bsd_amountadvancepayment").getValue();
    var differentAmount = Xrm.Page.getAttribute("bsd_differentamount").getValue();
    if (!amountAvd)
        amountAvd = 0;
    if (!differentAmount)
        differentAmount = 0;
    var strAmount = "";
    var total = 0;
    var assignamount = Xrm.Page.getAttribute("bsd_differentamount").getValue();
    if (gridType == 1) {
        strAmount = Xrm.Page.getAttribute("bsd_arrayamountadvance").getValue();
        if (strAmount) {
            var arrAmount = strAmount.split(",");
            if (arrAmount.length >= 0) {
                for (var i = 0; i < arrAmount.length; i++) {
                    var value = parseFloat(arrAmount[i]) ? parseFloat(arrAmount[i]) : 0;
                    total += value;
                }
                Xrm.Page.getAttribute("bsd_amountadvancepayment").setValue(total);
                Xrm.Page.getAttribute("bsd_amountadvancepayment").fireOnChange();
            }
        }
        else {
            total = 0;
            Xrm.Page.getAttribute("bsd_amountadvancepayment").setValue(total);
            Xrm.Page.getAttribute("bsd_amountadvancepayment").fireOnChange();
        }

        var totalPhase = Xrm.Page.getAttribute("bsd_totalapplyamount").getValue() ? Xrm.Page.getAttribute("bsd_totalapplyamount").getValue() : 0;

        if (total < totalPhase) {
            Xrm.Page.ui.setFormNotification('The amount you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!', 'WARNING', '2');
        }
    }
    else {
        var strAmount_phase = Xrm.Page.getAttribute("bsd_arrayamountpay").getValue();
        var strAmount_interest = Xrm.Page.getAttribute("bsd_arrayinterestamount").getValue();
        var strAmount_fees = Xrm.Page.getAttribute("bsd_arrayfeesamount").getValue();
        var strAmount_mis = Xrm.Page.getAttribute("bsd_arraymicellaneousamount").getValue();
        if (strAmount_phase)
            strAmount = strAmount.length == 0 ? strAmount_phase : strAmount.concat("," + strAmount_phase);
        if (strAmount_interest)
            strAmount = strAmount.length == 0 ? strAmount_interest : strAmount.concat("," + strAmount_interest);
        if (strAmount_fees)
            strAmount = strAmount.length == 0 ? strAmount_fees : strAmount.concat("," + strAmount_fees);
        if (strAmount_mis)
            strAmount = strAmount.length == 0 ? strAmount_mis : strAmount.concat("," + strAmount_mis);
        if (strAmount) {
            var arrAmount = strAmount.split(",");
            for (var i = 0; i < arrAmount.length; i++) {
                var value = parseFloat(arrAmount[i]) ? parseFloat(arrAmount[i]) : 0;
                total += value;
            }
            Xrm.Page.getAttribute("bsd_totalapplyamount").setValue(total);
            Xrm.Page.getAttribute("bsd_totalapplyamount").fireOnChange();

            Xrm.Page.getAttribute("bsd_assignamount").setValue(differentAmount - total);
            Xrm.Page.getAttribute("bsd_assignamount").fireOnChange();
        }
        else {
            total = 0;
            Xrm.Page.getAttribute("bsd_totalapplyamount").setValue(total);
            Xrm.Page.getAttribute("bsd_totalapplyamount").fireOnChange();

            Xrm.Page.getAttribute("bsd_assignamount").setValue(differentAmount - total);
            Xrm.Page.getAttribute("bsd_assignamount").fireOnChange();
        }
        if (total > differentAmount) {
            Xrm.Page.ui.clearFormNotification('2');
            Xrm.Page.ui.setFormNotification('The amount you pay exceeds the amount paid. Please choose another Advance Payment or other Payment Phase!', 'WARNING', '3');
        }
    }


    //Xrm.Page.data.save().then(function () {
    //    Xrm.Page.ui.controls.get("bsd_amountadvancepayment").setDisabled(true);
    //    Xrm.Page.ui.controls.get("bsd_totalapplyamount").setDisabled(true);
    //},
    //function (error) {
    //    //console.log(error.message);
    //});

}

//----------------------------------  ADD TEXTBOX + EVENT DOUBLE CLICK  ----------------------------------

function LoadEventDouble(gridType, index, sectionName) {
    if (gridType == 2) {
        if (sectionName == "GridPhase")
            $('#GridPhase').children('tbody:eq(0)').children('tr').eq(index).children('td:nth-child(7)').dblclick(function (sender) {
                var obj = this.firstElementChild.firstElementChild;
                if (!(obj != null && obj.tagName == 'INPUT'))
                    addTextBox(this);

            });
        else if (sectionName == "GridInterest")
            $('#GridInterest').children('tbody:eq(0)').children('tr').eq(index).children('td:nth-child(6)').dblclick(function (sender) {
                var obj = this.firstElementChild.firstElementChild;
                if (!(obj != null && obj.tagName == 'INPUT'))
                    addTextBox(this);

            });
        else if (sectionName == "GridFees")
            $('#GridFees').children('tbody:eq(0)').children('tr').eq(index).children('td:nth-child(5)').dblclick(function (sender) {
                addTextBox(this);

            });
        else if (sectionName == "GridMiscellaneous")
            $('#GridMiscellaneous').children('tbody:eq(0)').children('tr').eq(index).children('td:nth-child(7)').dblclick(function (sender) {
                addTextBox(this);

            });
    }
}
function addTextBox(target) {
    debugger;
    if (target != null) {
        var span = target.firstElementChild;
        var tmp = span.innerHTML; var value;
        var amount = tmp.trim().replace(/đ|₫| /gi, "");
        amount = (amount) ? (amount).split(".").join("") : 0;
        if (amount > 0) {
            span.innerHTML = '';
            //var l_input = $("input[name='customInput']")
            //var n=l_input.count
            var input = document.createElement("input");
            input.type = "text";
            input.name = "customInput"
            //input.id = "customInput" + n;
            input.style.cssText = 'width:100%;font-size:11px;';
            input.value = tmp;
            span.appendChild(input);
            input.focus();

            var row = target.closest("tr");
            var chexkbox = row.childNodes[row.childElementCount - 1].firstElementChild.firstElementChild;
            chexkbox.disabled = true;
            $(input).keyup(function (e) {
                if (e.keyCode == 13) {
                    $(this).trigger("enterKey");
                }
            });
            $(input).bind("enterKey", function () {
                var input = $(this);
                func(input, chexkbox);
            });
            $(input).focusout(function () {
                debugger;
                var input = $(this);
                if (input.context.name == "customInput")
                    func(input, chexkbox);
            });
            function func(e, checkbox) {
                debugger;
                var arrID;
                var arrAmount;
                var gridType = 1;
                var TableName = e.parents("table")[0].getAttribute("name");
                if (TableName == "GridPhase" || TableName == "GridInterest" || TableName == "GridFees" || TableName == "GridMiscellaneous")
                    gridType = 2;
                var ckbID = null;
                if (gridType == 1)
                    ckbID = e.closest('tr').find('.ckb1')[0].value;
                else
                    ckbID = e.closest('tr').find('.ckb2')[0].value;
                // Gán lại số tiền cần trả của đợt đó
                if (TableName == "GridMiscellaneous")
                    var amountPhase = e.closest('tr').children('td').eq(3).children('span')["html"]();
                else var amountPhase = e.closest('tr').children('td').eq(2).children('span')["html"]();
                if (amountPhase.length > 0) {
                    amountPhase = amountPhase.trim().replace(/đ|₫| /gi, "")
                    amountPhase = amountPhase ? amountPhase.split(".").join("") : 0;
                }
                if (TableName == "GridPhase")
                    var amountPaid = e.closest('tr').children('td').eq(5).children('span')["html"]();
                else if (TableName == "GridInterest")
                    var amountPaid = e.closest('tr').children('td').eq(4).children('span')["html"]();
                else if (TableName == "GridMiscellaneous")
                    var amountPaid = e.closest('tr').children('td').eq(5).children('span')["html"]();
                else
                    var amountPaid = e.closest('tr').children('td').eq(3).children('span')["html"]();
                if (amountPaid.length > 0) {
                    amountPaid = amountPaid.trim().replace(/đ|₫| /gi, "")
                    amountPaid = amountPaid ? (amountPaid).split(".").join("") : 0;
                }

                var amountNeed = amountPhase - amountPaid;
                if (TableName == "GridPhase") {
                    var waiver = e.closest('tr').children('td').eq(3).children('span')["html"]();
                    if (waiver.length > 0) {
                        waiver = waiver.trim().replace(/đ|₫| /gi, "")
                        waiver = waiver ? waiver.split(".").join("") : 0;
                    }
                    var deposit = e.closest('tr').children('td').eq(4).children('span')["html"]();
                    if (deposit.length > 0) {
                        deposit = deposit.trim().replace(/đ|₫| /gi, "")
                        deposit = deposit ? deposit.split(".").join("") : 0;
                    }
                    amountNeed = amountNeed - waiver - deposit;
                }
                else if (TableName == "GridInterest") {
                    var waiver = e.closest('tr').children('td').eq(3).children('span')["html"]();
                    if (waiver.length > 0) {
                        waiver = waiver.trim().replace(/đ|₫| /gi, "")
                        waiver = waiver ? waiver.split(".").join("") : 0;
                    }

                    amountNeed = amountNeed - waiver;
                }
                else if (TableName == "GridMiscellaneous") {
                    var waiver = e.closest('tr').children('td').eq(4).children('span')["html"]();
                    if (waiver.length > 0) {
                        waiver = waiver.trim().replace(/đ|₫| /gi, "")
                        waiver = waiver ? waiver.split(".").join("") : 0;
                    }
                    amountNeed = amountNeed - waiver;
                }

                if (e[0].value.length == 0) {
                    value = amountNeed;
                }
                else {
                    value = input.value;
                    value = value.trim().replace(/đ|₫| /gi, "");
                    value = (value) ? (value).split(".").join("") : 0;
                    value = parseFloat(value);
                }

                Xrm.Page.ui.clearFormNotification('2');
                if (value <= amountNeed) {

                    if (value == 0) {
                        Xrm.Page.ui.setFormNotification('You enter the amount of payment is 0. Please re-enter the payment amount!', 'WARNING', '2');
                    }
                    else if (value < 0) {
                        Xrm.Page.ui.setFormNotification('You enter the amount of payment is negative value. Please re-enter the payment amount!', 'WARNING', '2');
                    }
                    else {
                        var td = e[0].closest('td')
                        while (td.hasChildNodes() == true)
                            td.removeChild(td.firstElementChild);
                        var span1 = document.createElement('span');
                        td.appendChild(span1);

                        span1.innerHTML = value.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫";
                        checkbox.disabled = false;
                        if (gridType == 1) {
                            if (Xrm.Page.getAttribute("bsd_arrayadvancepayment").getValue()) {
                                var strChecked = Xrm.Page.getAttribute("bsd_arrayadvancepayment").getValue();
                                if (strChecked.length >= 0)
                                    arrID = strChecked.split(",");
                            }

                            if (Xrm.Page.getAttribute("bsd_arrayamountadvance").getValue()) {
                                var strAmount = Xrm.Page.getAttribute("bsd_arrayamountadvance").getValue();
                                if (strAmount.length >= 0)
                                    arrAmount = strAmount.split(",");
                            }

                            var index = arrID.indexOf(ckbID);
                            if (index > -1) {
                                arrAmount[index] = value;
                                Xrm.Page.getAttribute("bsd_arrayamountadvance").setValue(arrAmount.toString());
                            }
                        }
                        else if (TableName == "GridPhase") {//==2
                            if (Xrm.Page.getAttribute("bsd_arraypsdid").getValue()) {
                                var strChecked = Xrm.Page.getAttribute("bsd_arraypsdid").getValue();
                                if (strChecked.length >= 0)
                                    arrID = strChecked.split(",");
                            }

                            if (Xrm.Page.getAttribute("bsd_arrayamountpay").getValue()) {
                                var strAmount = Xrm.Page.getAttribute("bsd_arrayamountpay").getValue();
                                if (strAmount.length >= 0)
                                    arrAmount = strAmount.split(",");
                            }

                            var index = arrID.indexOf(ckbID);
                            if (index > -1) {
                                arrAmount[index] = value;
                                Xrm.Page.getAttribute("bsd_arrayamountpay").setValue(arrAmount.toString());
                            }
                        }
                        else if (TableName == "GridInterest") {//==2
                            if (Xrm.Page.getAttribute("bsd_arrayinstallmentinterest").getValue()) {
                                var strChecked = Xrm.Page.getAttribute("bsd_arrayinstallmentinterest").getValue();
                                if (strChecked.length >= 0)
                                    arrID = strChecked.split(",");
                            }

                            if (Xrm.Page.getAttribute("bsd_arrayinterestamount").getValue()) {
                                var strAmount = Xrm.Page.getAttribute("bsd_arrayinterestamount").getValue();
                                if (strAmount.length >= 0)
                                    arrAmount = strAmount.split(",");
                            }

                            var index = arrID.indexOf(ckbID);
                            if (index > -1) {
                                arrAmount[index] = value;
                                Xrm.Page.getAttribute("bsd_arrayinterestamount").setValue(arrAmount.toString());
                            }
                        }
                        else if (TableName == "GridFees") {//==2
                            if (Xrm.Page.getAttribute("bsd_arrayfeesid").getValue()) {
                                var strChecked = Xrm.Page.getAttribute("bsd_arrayfeesid").getValue();
                                if (strChecked.length >= 0)
                                    arrID = strChecked.split(",");
                            }
                            if (Xrm.Page.getAttribute("bsd_arrayfeesamount").getValue()) {
                                var strAmount = Xrm.Page.getAttribute("bsd_arrayfeesamount").getValue();
                                if (strAmount.length >= 0)
                                    arrAmount = strAmount.split(",");
                            }
                            var index = arrID.indexOf(ckbID);
                            if (index > -1) {
                                arrAmount[index] = value;
                                Xrm.Page.getAttribute("bsd_arrayfeesamount").setValue(arrAmount.toString());
                            }
                        }
                        else if (TableName == "GridMiscellaneous") {//==2
                            if (Xrm.Page.getAttribute("bsd_arraymicellaneousid").getValue()) {
                                var strChecked = Xrm.Page.getAttribute("bsd_arraymicellaneousid").getValue();
                                if (strChecked.length >= 0)
                                    arrID = strChecked.split(",");
                            }

                            if (Xrm.Page.getAttribute("bsd_arraymicellaneousamount").getValue()) {
                                var strAmount = Xrm.Page.getAttribute("bsd_arraymicellaneousamount").getValue();
                                if (strAmount.length >= 0)
                                    arrAmount = strAmount.split(",");
                            }

                            var index = arrID.indexOf(ckbID);
                            if (index > -1) {
                                arrAmount[index] = value;
                                Xrm.Page.getAttribute("bsd_arraymicellaneousamount").setValue(arrAmount.toString());
                            }
                        }
                        CheckAmountInput(gridType);
                    }
                }
                else {
                    Xrm.Page.ui.setFormNotification('You enter the amount is negative value. Please re-enter the amount less than or equal to ' + amountNeed.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫", 'WARNING', '2');
                }
                //var amountPhase = e.closest('tr').children('td').eq(2).children('span')["html"]();
                //if (amountPhase.length > 0) {
                //    amountPhase = amountPhase.trim().replace(/đ|₫| /gi, "")
                //    amountPhase = amountPhase ? amountPhase.split(".").join("") : 0;
                //if (TableName == "GridMiscellaneous")
                //    var amountPhase = e.closest('tr').children('td').eq(3).children('span')["html"]();
                //else var amountPhase = e.closest('tr').children('td').eq(2).children('span')["html"]();
                //if (amountPhase.length > 0) {
                //    amountPhase = amountPhase.trim().replace(/đ|₫| /gi, "")
                //    amountPhase = amountPhase ? amountPhase.split(".").join("") : 0;
                //}
                //}
                //if (TableName == "GridPhase")
                //    var amountPaid = e.closest('tr').children('td').eq(5).children('span')["html"]();
                //else if (TableName == "GridInterest")
                //    var amountPaid = e.closest('tr').children('td').eq(4).children('span')["html"]();
                //else
                //    var amountPaid = e.closest('tr').children('td').eq(3).children('span')["html"]();
                //if (amountPaid.length > 0) {
                //    amountPaid = amountPaid.trim().replace(/đ|₫| /gi, "")
                //    amountPaid = amountPaid ? (amountPaid).split(".").join("") : 0;
                //}

                //var amountNeed = amountPhase - amountPaid;
                //if (TableName == "GridPhase") {
                //    var waiver = e.closest('tr').children('td').eq(3).children('span')["html"]();
                //    if (waiver.length > 0) {
                //        waiver = waiver.trim().replace(/đ|₫| /gi, "")
                //        waiver = waiver ? waiver.split(".").join("") : 0;
                //    }
                //    var deposit = e.closest('tr').children('td').eq(4).children('span')["html"]();
                //    if (deposit.length > 0) {
                //        deposit = deposit.trim().replace(/đ|₫| /gi, "")
                //        deposit = deposit ? deposit.split(".").join("") : 0;
                //    }
                //    amountNeed = amountNeed - waiver - deposit;
                //}
                //else if (TableName == "GridInterest") {
                //    var waiver = e.closest('tr').children('td').eq(3).children('span')["html"]();
                //    if (waiver.length > 0) {
                //        waiver = waiver.trim().replace(/đ|₫| /gi, "")
                //        waiver = waiver ? waiver.split(".").join("") : 0;
                //    }

                //    amountNeed = amountNeed - waiver;
                //}

                //if (e[0].value.length == 0) {
                //    value = amountNeed;
                //}
                //else {
                //    value = input.value;
                //    value = value.trim().replace(/đ|₫| /gi, "");
                //    value = (value) ? (value).split(".").join("") : 0;
                //    value = parseFloat(value);
                //}

                //Xrm.Page.ui.clearFormNotification('2');
                //if (value <= amountNeed) {

                //    if (value == 0) {
                //        Xrm.Page.ui.setFormNotification('You enter the amount of payment is 0. Please re-enter the payment amount!', 'WARNING', '2');
                //    }
                //    else if (value < 0) {
                //        Xrm.Page.ui.setFormNotification('You enter the amount of payment is negative value. Please re-enter the payment amount!', 'WARNING', '2');
                //    }
                //    else {
                //        var td = e[0].closest('td')
                //        while (td.hasChildNodes() == true)
                //            td.removeChild(td.firstElementChild);
                //        var span1 = document.createElement('span');
                //        td.appendChild(span1);

                //        span1.innerHTML = value.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫";
                //        checkbox.disabled = false;
                //        if (gridType == 1) {
                //            if (Xrm.Page.getAttribute("bsd_arrayadvancepayment").getValue()) {
                //                var strChecked = Xrm.Page.getAttribute("bsd_arrayadvancepayment").getValue();
                //                if (strChecked.length >= 0)
                //                    arrID = strChecked.split(",");
                //            }

                //            if (Xrm.Page.getAttribute("bsd_arrayamountadvance").getValue()) {
                //                var strAmount = Xrm.Page.getAttribute("bsd_arrayamountadvance").getValue();
                //                if (strAmount.length >= 0)
                //                    arrAmount = strAmount.split(",");
                //            }

                //            var index = arrID.indexOf(ckbID);
                //            if (index > -1) {
                //                arrAmount[index] = value;
                //                Xrm.Page.getAttribute("bsd_arrayamountadvance").setValue(arrAmount.toString());
                //            }
                //        }
                //        else if (TableName == "GridPhase") {//==2
                //            if (Xrm.Page.getAttribute("bsd_arraypsdid").getValue()) {
                //                var strChecked = Xrm.Page.getAttribute("bsd_arraypsdid").getValue();
                //                if (strChecked.length >= 0)
                //                    arrID = strChecked.split(",");
                //            }

                //            if (Xrm.Page.getAttribute("bsd_arrayamountpay").getValue()) {
                //                var strAmount = Xrm.Page.getAttribute("bsd_arrayamountpay").getValue();
                //                if (strAmount.length >= 0)
                //                    arrAmount = strAmount.split(",");
                //            }

                //            var index = arrID.indexOf(ckbID);
                //            if (index > -1) {
                //                arrAmount[index] = value;
                //                Xrm.Page.getAttribute("bsd_arrayamountpay").setValue(arrAmount.toString());
                //            }
                //        }
                //        else if (TableName == "GridInterest") {//==2
                //            if (Xrm.Page.getAttribute("bsd_arrayinstallmentinterest").getValue()) {
                //                var strChecked = Xrm.Page.getAttribute("bsd_arrayinstallmentinterest").getValue();
                //                if (strChecked.length >= 0)
                //                    arrID = strChecked.split(",");
                //            }

                //            if (Xrm.Page.getAttribute("bsd_arrayinterestamount").getValue()) {
                //                var strAmount = Xrm.Page.getAttribute("bsd_arrayinterestamount").getValue();
                //                if (strAmount.length >= 0)
                //                    arrAmount = strAmount.split(",");
                //            }

                //            var index = arrID.indexOf(ckbID);
                //            if (index > -1) {
                //                arrAmount[index] = value;
                //                Xrm.Page.getAttribute("bsd_arrayinterestamount").setValue(arrAmount.toString());
                //            }
                //        }
                //        else if (TableName == "GridFees") {//==2
                //            if (Xrm.Page.getAttribute("bsd_arrayfees").getValue()) {
                //                var strChecked = Xrm.Page.getAttribute("bsd_arrayfees").getValue();
                //                if (strChecked.length >= 0)
                //                    arrID = strChecked.split(",");
                //            }
                //            if (Xrm.Page.getAttribute("bsd_arrayfeesamount").getValue()) {
                //                var strAmount = Xrm.Page.getAttribute("bsd_arrayfeesamount").getValue();
                //                if (strAmount.length >= 0)
                //                    arrAmount = strAmount.split(",");
                //            }
                //            var index = arrID.indexOf(ckbID);
                //            if (index > -1) {
                //                arrAmount[index] = value;
                //                Xrm.Page.getAttribute("bsd_arrayfeesamount").setValue(arrAmount.toString());
                //            }
                //        }
                //        CheckAmountInput(gridType);
                //    }
                //}
                //else {
                //    Xrm.Page.ui.setFormNotification('You enter the amount is negative value. Please re-enter the amount less than or equal to ' + amountNeed.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1.") + " ₫", 'WARNING', '2');
                //}
            }
        }
    }
}

