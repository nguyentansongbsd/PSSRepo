function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
    results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

var template = '';
var gb_projectcode = "";
function buldReportById(pos) {
    console.log(pos);
    template = $('#divtemplate').html();
    if (pos < arrid.length) {
        var id = arrid[pos];
        console.log("id: " + id);
        //dsInfoNew
        var xml = [];
        xml.push("<fetch>");
        xml.push("<entity name='bsd_payment' enableprefiltering='1' >");
        xml.push("<attribute name='bsd_differentamount' alias='Difference' />");
        xml.push("<attribute name='statuscode' alias='statuscode' />");
        xml.push("<attribute name='bsd_interestcharge' alias='interestchargeAmount' />");
        xml.push("<attribute name='bsd_name' alias='pm_name' />");
        xml.push("<attribute name='bsd_balance' alias='balance' />");
        xml.push("<attribute name='bsd_paymentactualtime' alias='pm_Date' />");
        xml.push("<attribute name='bsd_paymentnumber' alias='pm_paymentnumber' />");
        xml.push("<attribute name='bsd_depositamount' alias='pm_Deposit' />");
        xml.push("<attribute name='bsd_totalamountpayablephase' alias='pm_totalamountpayablephase' />");
        xml.push("<attribute name='bsd_amountpay' alias='pm_amountpay' />");
        xml.push("<attribute name='bsd_paymenttype' alias='pm_type' />");
        xml.push("<attribute name='bsd_units' alias='pm_units' />");
        xml.push("<attribute name='bsd_purchaser' alias='pm_purchaser' />");
        xml.push("<attribute name='bsd_project' alias='pm_project'/>");
        xml.push("<attribute name='bsd_optionentry' alias='pm_optionentry' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_paymentid' operator='eq' uitype='bsd_payment' value='" + id + "' />");
        xml.push("</filter>");
        xml.push("<link-entity name='bsd_project' from='bsd_projectid' to='bsd_project' alias='proj' >");
        xml.push("<attribute name='bsd_name' alias='proj_name' />");
        xml.push("<attribute name='bsd_address' alias='proj_add' />");
        xml.push("<attribute name='bsd_projectcode' alias='proj_code' />");
        xml.push("<attribute name='bsd_logoimagename' alias='proj_logo' />");
        xml.push("<link-entity name='account' from='accountid' to='bsd_investor' alias='investor' >");
        xml.push("<attribute name='bsd_diachi' alias='investor_addEN' />");
        xml.push("<attribute name='bsd_permanentaddress1' alias='permanentaddress1'/>");
        xml.push("<attribute name='bsd_diachithuongtru' alias='diachithuongtru'/>");
        xml.push("<attribute name='bsd_address' alias='investor_addVN' />");
        xml.push("<attribute name='address2_composite' alias='investor_add2' />");
        xml.push("<attribute name='name' alias='investor_nameSys' />");
        xml.push("<attribute name='bsd_name' alias='investor_name' />");
        xml.push("<attribute name='bsd_accountnameother' alias='investor_nameEN' />");
        xml.push("<attribute name='fax' alias='investor_fax' />");
        xml.push("<attribute name='accountnumber' alias='investor_AccountNo' />");
        xml.push("<attribute name='bsd_registrationcode' alias='investor_MSThue' />");
        xml.push("<attribute name='telephone1' alias='investor_phone' />");
        xml.push("<attribute name='bsd_companycode' alias='investor_companycode' />");
        xml.push("<attribute name='emailaddress1' alias='investor_email' />");
        xml.push("<link-entity name='contact' from='contactid' to='primarycontactid' link-type='outer' alias='cont_primarycontact' >");
        xml.push("<attribute name='fullname' alias='primary_nameSys' />");
        xml.push("<attribute name='bsd_fullname' alias='primary_name' />");
        xml.push("<attribute name='jobtitle' alias='primary_job' />");
        xml.push("<attribute name='bsd_identitycardnumber' alias='primary_idnumber' />");
        xml.push("<attribute name='bsd_placeofissue' alias='primary_placeissue' />");
        xml.push("<attribute name='bsd_dategrant' alias='primary_dategrant' />");
        xml.push("</link-entity>");
        xml.push("</link-entity>");
        xml.push("</link-entity>");
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKitNew.Fetch(xml.join(""), true).then(function (rs) {
            console.log("buldReportById " + pos);
            console.log(rs);
            if (rs.length > 0) {
                //Load logo
                var image = rs[0].attributes['proj_logo'] ? rs[0].attributes['proj_logo'].value : "";

                var bsd_projectcode = rs[0].attributes['proj_code'] ? rs[0].attributes['proj_code'].value : "";
                gb_projectcode = bsd_projectcode;
                console.log("bsd_projectcode: " + bsd_projectcode);
                var row = '';
                var row_end = '';

                //Set Img Logo
                if (image != "") row = ['<img id="img_logoproject" src="' + image + '" />'].join("");

                if (bsd_projectcode == 'MBL') {
                    row_end = ['<div style="color:#ccc8c8;text-align:center;font-weight:bold;"><span style="text-transform:uppercase;">CAPITALAND – HOANG THANH CO., LTD</span> – Lot CT08, Co Ngua Area, Mo Lao New Urban Area, Ha Dong, Hanoi, Vietnam –</div>', '<div style="font-weight:bold;color:#ccc8c8;text-align:center;margin-bottom:60px">Tel: (84-4) 3939 3232 – Fax: (84-4) 3939 3099 – www.mulberrylane.com.vn </div>'].join("");
                    template = template.replace("{website}", row_end);
                    template = template.replace("{accountant}", "Nguyễn Thị Thu Trang");
                }
                if (bsd_projectcode == 'SSA') {
                    row_end = ['<div style="color:#ccc8c8;text-align:center;font-weight:bold;"><span style="text-transform:uppercase;">CAPITALAND – HOANG THANH INVESTMENT CO., LTD</span> – Lot CT09, Co Ngua Area, Mo Lao New Urban Area, Ha Dong, Hanoi, Vietnam - Tel: (84-4) 3939 3232 – Fax: (84-4) 3939 3099 – www.seasonsavenue.com.vn </div>'].join("");
                    template = template.replace("{website}", row_end);
                    template = template.replace("{accountant}", "Nguyễn Thị Ngọc Thanh");
                }

                template = template.replace("{projectcode_logo}", row);

                var bsd_registrationcode = rs[0].attributes['investor_MSThue'] ? rs[0].attributes['investor_MSThue'].value : "";
                template = template.replace("{bsd_registrationcode}", bsd_registrationcode);
                console.log("bsd_registrationcode: " + bsd_registrationcode);
                var bsd_paymentnumber = rs[0].attributes['pm_paymentnumber'] ? rs[0].attributes['pm_paymentnumber'].value : "";
                template = template.replace("{bsd_paymentnumber}", bsd_paymentnumber);
                console.log("bsd_paymentnumber: " + bsd_paymentnumber);
                var bsd_name = rs[0].attributes['investor_name'] ? rs[0].attributes['investor_name'].value : "";
                template = template.replace("{bsd_name}", bsd_name);
                console.log("investor_name: " + bsd_name);

                var bsd_accountnameother = rs[0].attributes['investor_nameEN'] ? rs[0].attributes['investor_nameEN'].value : "";
                template = template.replace("{bsd_accountnameother}", bsd_accountnameother);

                var permanentaddress1 = rs[0].attributes['permanentaddress1'] ? rs[0].attributes['permanentaddress1'].value : "";
                template = template.replace("{bsd_AddressVN}", permanentaddress1);

                var diachithuongtru = rs[0].attributes['diachithuongtru'] ? rs[0].attributes['diachithuongtru'].value : "";
                template = template.replace("{bsd_AddressForeign}", diachithuongtru);

                var bsd_paymentactualtime = rs[0].attributes['pm_Date'] ? rs[0].attributes['pm_Date'].formattedValue : "";
                console.log("bsd_paymentactualtime: " + bsd_paymentactualtime);
                template = template.replace("{bsd_paymentactualtime}", bsd_paymentactualtime);

                var bsd_amountpay = rs[0].attributes['pm_amountpay'] ? rs[0].attributes['pm_amountpay'].value : 0;
                console.log("bsd_amountpay: " + bsd_amountpay);
                var bsd_paymenttype = rs[0].attributes['pm_type'] ? rs[0].attributes['pm_type'].value : 0;

                var bsd_differentamount = rs[0].attributes['Difference'] ? rs[0].attributes['Difference'].value : 0;
                var pm_amountpay = rs[0].attributes['pm_amountpay'] ? rs[0].attributes['pm_amountpay'].value : 0;

                getPaymentType(bsd_paymenttype, id, function () {
                    console.log("getPaymentType callback");
                    getAdvancePayment(id, function (template) {
                        console.log("getAdvancePayment callback");
                        console.log("Pos: " + pos);
                        console.log("arrid: " + arrid.length);

                        if (pos < arrid.length) {
                            $('#showreport').append(template);

                            buldReportById(pos + 1);
                            updatePrintDate(id);
                            $('.clearspace').each(function () {
                                if ($(this).html() == '') {
                                    $(this).parent().remove();
                                }

                            });

                        } else {
                            $('.clearspace').each(function () {
                                if ($(this).html() == '') {
                                    $(this).parent().remove();
                                }

                            });
                        }

                        if (pos + 1 < arrid.length && template != '') {
                            console.log("Add break page");
                            $('#showreport').append('<p style="page-break-after:always;font-weight:bold"></p>');
                        }
                        console.log("buldReportById " + pos + "Done");
                    })
                });

            }

        });
    }

}

function getPaymentType(bsd_paymenttype, paymentid, callback) {
    debugger;
    console.log("getPaymentType " + bsd_paymenttype + " - " + paymentid);
    if (bsd_paymenttype == 100000000) {
        var xml1 = [];
        xml1.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >");
        xml1.push("<entity name='bsd_payment'>");
        xml1.push("<attribute name='bsd_differentamount' alias='Difference' />");
        xml1.push("<attribute name='bsd_amountpay' alias='pm_amountpay' />");
        xml1.push("<attribute name='bsd_totalamountpayablephase' alias='pm_totalamountpayablephase' />");
        xml1.push("<attribute name='bsd_name' alias='pm_name' />");
        xml1.push("<filter type='and'>");
        xml1.push("<condition attribute='bsd_paymentid' operator='eq' uitype='bsd_payment' value='" + paymentid + "' />");
        xml1.push("</filter>");
        xml1.push("<link-entity name='opportunity' from='opportunityid' to='bsd_queue' alias='queue' >");
        xml1.push("<link-entity name='account' from='accountid' to='customerid' visible='false' link-type='outer' alias='accountinfor'>");
        xml1.push("<attribute name='name' alias='accNameSys' />");
        xml1.push("<attribute name='bsd_name' alias='accName' />");
        xml1.push("<attribute name='bsd_accountnameother' alias='accNameEN' />");
        xml1.push("<attribute name='bsd_address' alias='acc_addVN' />");
        xml1.push("<attribute name='bsd_diachi' alias='acc_addEN' />");
        xml1.push("</link-entity>");
        xml1.push("<link-entity name='contact' from='contactid' to='customerid' visible='false' link-type='outer' alias='contactinfor'>");
        xml1.push("<attribute name='fullname' alias='contNameSys' />");
        xml1.push("<attribute name='bsd_fullname' alias='contName' />");
        xml1.push("<attribute name='bsd_contactaddress' alias='cont_addVN' />");
        xml1.push("<attribute name='bsd_diachi' alias='cont_addEN' />");
        xml1.push("<attribute name='bsd_housenumber' alias='bsd_housenumber' />");
        xml1.push("</link-entity>");
        xml1.push("<attribute name='totalamount' alias='queueTotalamount' />");
        xml1.push("<attribute name='name' alias='opp_name' />");
        xml1.push("<link-entity name='opportunityproduct' from='opportunityid' to='opportunityid' alias='opprod' link-type='outer' >");
        xml1.push("<link-entity name='product' from='productid' to='bsd_units' alias='prod' link-type='outer' >");
        xml1.push("<attribute name='name' alias='prod_name' />");
        xml1.push("<attribute name='bsd_unitscodesams' alias='bsd_unitscodesams' />");
        xml1.push("<attribute name='vendorname' alias='prod_vendor' />");
        xml1.push("</link-entity>");
        xml1.push("</link-entity>");
        xml1.push("<link-entity name='bsd_project' from='bsd_projectid' to='bsd_project' alias='proj' link-type='outer' >");
        xml1.push("<attribute name='bsd_name' alias='proj_name' />");
        xml1.push("<attribute name='bsd_address' alias='proj_add' />");
        xml1.push("<link-entity name='account' from='accountid' to='bsd_investor' alias='investor' link-type='outer' >");
        xml1.push("<attribute name='bsd_address' alias='investor_add1' />");
        xml1.push("<attribute name='name' alias='investor_nameSys' />");
        xml1.push("<attribute name='bsd_name' alias='investor_name' />");
        xml1.push("<attribute name='address2_composite' alias='investor_add2' />");
        xml1.push("<attribute name='fax' alias='investor_fax' />");
        xml1.push("<attribute name='accountnumber' alias='investor_number' />");
        xml1.push("<attribute name='telephone1' alias='investor_phone' />");
        xml1.push("<attribute name='slaid' alias='investor_slaid' />");
        xml1.push("<attribute name='bsd_companycode' alias='investor_companycode' />");
        xml1.push("<attribute name='bsd_rocnumber2' alias='investor_ROCnumber2' />");
        xml1.push("<attribute name='bsd_groupgstregisttationnumber' alias='investor_groupgstregisttationnumber' />");
        xml1.push("<attribute name='territorycode' alias='investor_territorycode' />");
        xml1.push("<attribute name='bsd_generalledgercompanynumber' alias='investor_generalledgercompanynumber' />");
        xml1.push("<attribute name='stockexchange' alias='investor_stockexchange' />");
        xml1.push("<attribute name='bsd_rocnumber1' alias='investor_ROCnumber1' />");
        xml1.push("<attribute name='bsd_vatregistrationnumber' alias='investor_vatregistrationnumber' />");
        xml1.push("<attribute name='emailaddress1' alias='investor_email' />");
        xml1.push("<link-entity name='contact' from='contactid' to='primarycontactid' link-type='outer' alias='cont_primarycontact' >");
        xml1.push("<attribute name='fullname' alias='primary_nameSys' />");
        xml1.push("<attribute name='bsd_fullname' alias='primary_name' />");
        xml1.push("<attribute name='jobtitle' alias='primary_job' />");
        xml1.push("<attribute name='bsd_identitycardnumber' alias='primary_idnumber' />");
        xml1.push("<attribute name='bsd_placeofissue' alias='primary_placeissue' />");
        xml1.push("<attribute name='bsd_dategrant' alias='primary_dategrant' />");
        xml1.push("</link-entity>");
        xml1.push("</link-entity>");
        xml1.push("</link-entity>");
        xml1.push("</link-entity>");
        xml1.push("</entity>");
        xml1.push("</fetch>");
        CrmFetchKitNew.Fetch(xml1.join(""), true).then(function (rs1) {
            console.log("bsd_paymenttype: " + bsd_paymenttype);
            console.log(rs1);
            debugger;
            if (rs1.length > 0) {

                var bsd_fullname = rs1[0].attributes['contName'] != undefined ? rs1[0].attributes['contName'].value : '';
                var bsd_name = rs1[0].attributes['accName'] ? rs1[0].attributes['accName'].value : '';
                var bsd_accountnameother = rs1[0].attributes['accNameEN'] ? rs1[0].attributes['accNameEN'].value : '';
                var ac_address = rs1[0].attributes['acc_addVN'] ? rs1[0].attributes['acc_addVN'].value : '';
                var ac_diachi = rs1[0].attributes['acc_addEN'] ? rs1[0].attributes['acc_addEN'].value : '';
                var bsd_contactaddress = rs1[0].attributes['cont_addVN'] ? rs1[0].attributes['cont_addVN'].value : '';
                var bsd_diachi = rs1[0].attributes['cont_addEN'] ? rs1[0].attributes['cont_addEN'].value : '';
                var bsd_housenumber = rs1[0].attributes['bsd_housenumber'] ? rs1[0].attributes['bsd_housenumber'].value : '';
                if (bsd_housenumber == '') {
                    bsd_diachi = '';
                }
                if (bsd_fullname != '') {
                    template = template.replace("{bsd_fullname}", bsd_fullname);
                    template = template.replace("{bsd_contactaddress}", bsd_contactaddress);
                    template = template.replace("{bsd_diachi}", bsd_diachi);
                    template = template.replace("{bsd_accountname_other}", '');
                }
                else {
                    template = template.replace("{bsd_fullname}", bsd_name);
                    if (bsd_accountnameother != '') {
                        template = template.replace("{bsd_customer}", bsd_accountnameother);
                        template = template.replace("{bsd_accountname_other}", '(' + bsd_accountnameother + ')');
                    }
                    else {
                        template = template.replace("{bsd_customer}", '');
                        template = template.replace("{bsd_accountname_other}", '');
                    }
                    template = template.replace("{bsd_contactaddress}", ac_address);
                    template = template.replace("{bsd_diachi}", ac_diachi);
                }

                var bsd_project = rs1[0].attributes['proj_name'] ? rs1[0].attributes['proj_name'].name : '';
                template = template.replace("{bsd_project}", bsd_project);

                //if (bsd_project == "MULBERRY LANE") {
                if (gb_projectcode == "MBL") {
                    var bsd_units = rs1[0].attributes['bsd_unitscodesams'] ? rs1[0].attributes['bsd_unitscodesams'].value : '';
                    template = template.replace("{bsd_units}", bsd_units);
                }
                else {
                    var bsd_units = rs1[0].attributes['prod_name'] ? rs1[0].attributes['prod_name'].name : '';
                    template = template.replace("{bsd_units}", bsd_units);
                }

                var totalamount = rs1[0].attributes['queueTotalamount'] ? rs1[0].attributes['queueTotalamount'].value : '';
                template = template.replace("{totalamount}", common.formatNumberEx1(totalamount.toString()));

                //Tiền giữ chỗ
                var bsd_differentamount = rs1[0].attributes['Difference'] ? rs1[0].attributes['Difference'].value : 0;
                var pm_amountpay = rs1[0].attributes['pm_amountpay'] ? rs1[0].attributes['pm_amountpay'].value : 0;
                var queuing_amount = 0;

                if (bsd_differentamount != null && bsd_differentamount > 0) {
                    queuing_amount = rs1[0].attributes['pm_totalamountpayablephase'] ? rs1[0].attributes['pm_totalamountpayablephase'].value : 0;
                }
                else {
                    queuing_amount = rs1[0].attributes['pm_amountpay'] ? rs1[0].attributes['pm_amountpay'].value : 0;
                }

                var row_queuing = ['<tr>', '<td height="30px">Queuing Amount / Số tiền giữ chỗ</td>', '<td style="text-align:right;width:350px;margin-bottom:20px" colspan="2">' + common.formatNumberEx1(queuing_amount.toString()) + '</td>', '</tr>'].join("");

                template = template.replace("{queuing_amount}", row_queuing);

                template = template.replace("{fee}", '');
                template = template.replace("{interest_c}", '');
                template = template.replace("{listinterest}", '');
                template = template.replace("{listinstallment}", '');
                template = template.replace("{firstinstallment}", '');
                template = template.replace("{overpayment}", '');
                template = template.replace("{description_a}", '');
                template = template.replace("{bsd_customer}", '');
                template = template.replace("{bsd_co_account}", '');
                template = template.replace("{bsd_co_accountEN}", '');

            }
            callback();
        });
    }
    else if (bsd_paymenttype == 100000001) { //deposit
        var xml2 = [];
        xml2.push("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >");
        xml2.push("<entity name='bsd_payment'>");
        xml2.push("<attribute name='bsd_paymenttype' alias='pm_type' />");
        xml2.push("<attribute name='bsd_reservation' alias='pm_reser' />");
        xml2.push("<attribute name='bsd_differentamount' alias='difference' />");
        xml2.push("<filter type='and'>");

        xml2.push("<condition attribute='bsd_paymentid' operator='eq' uitype='bsd_payment' value='" + paymentid + "' />");
        xml2.push("</filter>");
        xml2.push("<link-entity name='quote' from='quoteid' to='bsd_reservation' link-type='outer' alias='reser' >");
        xml2.push("<attribute name='totalamount' alias='quotetotalamount' />");
        xml2.push("<attribute name='bsd_projectid' alias='quoteproj' />");
        xml2.push("<attribute name='bsd_depositfee' alias='quotedeposit' />");
        console.log('111111111111');
        xml2.push("<link-entity name='bsd_paymentschemedetail' from='bsd_reservation' to='quoteid' link-type='outer' alias='psd' >");
        xml2.push("<attribute name='bsd_amountwaspaid' alias='amountwaspaid' />");
        xml2.push("<attribute name='bsd_amountofthisphase' alias='amountofthisphase' />");
        xml2.push("<attribute name='bsd_depositamount' alias='depositamount' />");
        xml2.push("<attribute name='bsd_ordernumber' alias='quote_ordernumber' />");
        xml2.push("<filter type='and' >");
        xml2.push("<condition attribute='bsd_ordernumber' operator='eq' value='1' />");
        xml2.push("</filter>");
        xml2.push("</link-entity>");
        console.log('2222222222');
        xml2.push("<link-entity name='account' from='accountid' to='customerid' visible='false' link-type='outer' alias='accountinfor' >");
        xml2.push("<attribute name='name' alias='accNameSys' />");
        xml2.push("<attribute name='bsd_name' alias='accName' />");
        xml2.push("<attribute name='bsd_accountnameother' alias='accNameEN' />");
        xml2.push("<attribute name='bsd_address' alias='acc_addVN' />");
        xml2.push("<attribute name='bsd_diachi' alias='acc_addEN' />");
        xml2.push("</link-entity>");
        console.log('3333333333333');
        xml2.push("<link-entity name='contact' from='contactid' to='customerid' visible='false' link-type='outer' alias='contactinfor' >");
        console.log('31');
        xml2.push("<attribute name='fullname' alias='contNameSys' />");
        console.log('32');
        xml2.push("<attribute name='bsd_fullname' alias='contName' />");
        console.log('33');
        xml2.push("<attribute name='bsd_contactaddress' alias='cont_addVN' />");
        console.log('34');
        xml2.push("<attribute name='bsd_diachi' alias='cont_addEN' />");
        console.log('5');
        xml2.push("<attribute name='bsd_housenumber' alias='bsd_housenumber' />");
        xml2.push("</link-entity>");
        console.log('4');
        xml2.push("<link-entity name='product' from='productid' to='bsd_unitno' link-type='outer' alias='resprod' >");
        xml2.push("<attribute name='bsd_depositamount' alias='resprod_deposit' />");
        xml2.push("<attribute name='name' alias='resprod_name' />");
        xml2.push("<attribute name='bsd_unitscodesams' alias='bsd_unitscodesams' />");
        xml2.push("</link-entity>");
        console.log('5');
        xml2.push("<link-entity name='bsd_project' from='bsd_projectid' to='bsd_projectid' link-type='outer' alias='proj' >");
        xml2.push("<attribute name='bsd_name' alias='proj_name' />");
        xml2.push("<attribute name='bsd_address' alias='proj_add' />");
        xml2.push("<link-entity name='account' from='accountid' to='bsd_investor' link-type='outer' alias='investor' >");
        xml2.push("<attribute name='bsd_address' alias='investor_add1' />");
        xml2.push("<attribute name='name' alias='investor_namesys' />");
        xml2.push("<attribute name='bsd_name' alias='investor_name' />");
        xml2.push("<attribute name='address2_composite' alias='investor_add2' />");
        xml2.push("<attribute name='fax' alias='investor_fax' />");
        xml2.push("</link-entity>");
        xml2.push("</link-entity>");
        console.log('6');
        xml2.push("<link-entity name='bsd_coowner' from='bsd_reservation' to='quoteid' link-type='outer' alias='coo' >");
        xml2.push("<link-entity name='contact' from='contactid' to='bsd_customer' link-type='outer' alias='cooCont' >");
        xml2.push("<attribute name='bsd_fullname' alias='ccontName' />");
        xml2.push("</link-entity>");

        xml2.push("<link-entity name='account' from='accountid' to='bsd_customer' link-type='outer' alias='cooacc' >");

        xml2.push("<attribute name='bsd_name' alias='cooaccName' />");
        xml2.push("<attribute name='bsd_accountnameother' alias='cooaccNameEN' />");
        xml2.push("</link-entity>");
        xml2.push("</link-entity>");
        console.log('7');
        xml2.push("</link-entity>");
        xml2.push("</entity>");
        xml2.push("</fetch>");
        console.log(xml2);
        CrmFetchKitNew.Fetch(xml2.join(""), true).then(function (rs2) {
            console.log("bsd_paymenttype: " + bsd_paymenttype);
            console.log(rs2);
            if (rs2.length > 0) {

                var bsd_fullname = rs2[0].attributes['contName'] ? rs2[0].attributes['contName'].value : '';
                var bsd_contactaddress = rs2[0].attributes['cont_addVN'] ? rs2[0].attributes['cont_addVN'].value : '';
                var bsd_diachi = rs2[0].attributes['cont_addEN'] ? rs2[0].attributes['cont_addEN'].value : '';
                var bsd_housenumber = rs2[0].attributes['bsd_housenumber'] ? rs2[0].attributes['bsd_housenumber'].value : '';
                if (bsd_housenumber == '') bsd_housenumber = '';
                var bsd_customer = rs2[0].attributes['ccontName'] ? rs2[0].attributes['ccontName'].value : '';

                var bsd_project = rs2[0].attributes['proj_name'] ? rs2[0].attributes['proj_name'].value : '';
                template = template.replace("{bsd_project}", bsd_project);

                //if (bsd_project == "MULBERRY LANE") {
                if (gb_projectcode == "MBL") {
                    var bsd_units = rs2[0].attributes['bsd_unitscodesams'] ? rs2[0].attributes['bsd_unitscodesams'].value : '';
                    template = template.replace("{bsd_units}", bsd_units);
                }
                else {
                    var bsd_units = rs2[0].attributes['resprod_name'] ? rs2[0].attributes['resprod_name'].value : '';
                    template = template.replace("{bsd_units}", bsd_units);
                }

                var totalamount = rs2[0].attributes['quotetotalamount'] ? rs2[0].attributes['quotetotalamount'].value : '';
                template = template.replace("{totalamount}", common.formatNumberEx1(totalamount.toString()));

                //dòng tt3
                var rows = '';
                var co = false;
                for (var k = 0; k < rs2.length; k++) {
                    var quote_number = rs2[k].attributes['quote_ordernumber'] ? rs2[k].attributes['quote_ordernumber'].value : 0;
                    for (var i = 0; i < rs2.length; i++) {
                        var lastNumber = quote_number % 10;
                        if (quote_number == 1) {
                            stt = "st";
                        }
                        else if (quote_number == 2) {
                            stt = "nd";
                        }
                        else if (quote_number == 3) {
                            stt = "rd";
                        }
                        else if (quote_number > 20 && lastNumber == 1) {
                            stt = "st";
                        }
                        else if (quote_number > 20 && lastNumber == 2) {
                            stt = "nd";
                        }
                        else if (quote_number > 20 && lastNumber == 3) {
                            stt = "rd";
                        }
                        else {
                            stt = "th";
                        }
                    }

                    var row2 = '';
                    var bsd_differentamount = rs2[k].attributes['difference'] ? rs2[k].attributes['difference'].value : 0;
                    var bsd_amountofthisphase = rs2[k].attributes['amountofthisphase'] ? rs2[k].attributes['amountofthisphase'].value : 0;
                    var bsd_depositamount = rs2[k].attributes['depositamount'] ? rs2[k].attributes['depositamount'].value : 0;
                    var bsd_amount_deposit = bsd_amountofthisphase - bsd_depositamount;
                    if (bsd_differentamount <= 0) {
                        row2 = '';
                    }
                    else {
                        if (quote_number == 1) {
                            if (bsd_differentamount <= bsd_amount_deposit) {
                                co = true;
                                row2 = [' <tr>', '<td height="30px">' + quote_number + '' + stt + ' installment / thanh toán lần thứ ' + quote_number + '</td>', '<td style="text-align:right">' + common.formatNumberEx1(bsd_differentamount.toString()) + '</td>', '</tr>'].join("");
                            }
                            else {
                                co = true;
                                row2 = [' <tr>', '<td height="30px">' + quote_number + '' + stt + ' installment / thanh toán lần thứ ' + quote_number + '</td>', '<td style="text-align:right">' + common.formatNumberEx1(bsd_amount_deposit.toString()) + '</td>', '</tr>'].join("");
                            }
                            rows += row2;
                        }
                        else {
                            if (co == false && k == rs2.length - 1) {
                                template = template.replace("{listinstallment}", '');
                            }
                        }

                    }
                }
                template = template.replace("{listinstallment}", rows);

                if (bsd_fullname == '') {

                    var ac_address = rs2[0].attributes['acc_addVN'] ? rs2[0].attributes['acc_addVN'].value : '';
                    var ac_diachi = rs2[0].attributes['acc_addEN'] ? rs2[0].attributes['acc_addEN'].value : '';

                    var bsd_name = rs2[0].attributes['accName'] ? rs2[0].attributes['accName'].value : '';
                    var bsd_accountnameother = rs2[0].attributes['accNameEN'] != undefined ? rs2[0].attributes['accNameEN'].value : '';
                    if (bsd_accountnameother != '') {
                        template = template.replace("{bsd_accountname_other}", '(' + bsd_accountnameother + ')');
                    }
                    else {
                        template = template.replace("{bsd_customer}", '');
                        template = template.replace("{bsd_accountname_other}", '');
                    }
                    template = template.replace("{bsd_fullname}", bsd_name);
                    template = template.replace("{bsd_contactaddress}", ac_address);
                    template = template.replace("{bsd_diachi}", ac_diachi);

                    var bsd_co_account = rs2[0].attributes['cooaccName'] ? rs2[0].attributes['cooaccName'].value : '';
                    var bsd_co_accountEN = rs2[0].attributes['cooaccNameEN'] ? rs2[0].attributes['cooaccNameEN'].value : '';

                    if (bsd_co_account != '') {
                        template = template.replace("{bsd_co_account}", bsd_co_account);
                    }
                    else {
                        template = template.replace("{bsd_co_account}", '');
                    }

                    if (bsd_co_accountEN != '') {
                        template = template.replace("{bsd_co_accountEN}", '(' + bsd_co_accountEN + ')');
                    }
                    else {
                        template = template.replace("{bsd_co_accountEN}", '');
                    }

                    template = template.replace("{bsd_customer}", '');
                    template = template.replace("{bsd_accountname_other}", '');
                }

                else {

                    template = template.replace("{bsd_fullname}", bsd_fullname);
                    template = template.replace("{bsd_contactaddress}", bsd_contactaddress);
                    template = template.replace("{bsd_diachi}", bsd_diachi);
                    template = template.replace("{bsd_co_account}", '');
                    template = template.replace("{bsd_co_accountEN}", '');
                    var bsd_accountnameother = rs2[0].attributes['accNameEN'] != undefined ? rs2[0].attributes['accNameEN'].value : '';
                    if (bsd_accountnameother != '') {
                        template = template.replace("{bsd_accountname_other}", '(' + bsd_accountnameother + ')');
                    }
                    else {
                        //template = template.replace("{bsd_customer}", '');
                        template = template.replace("{bsd_accountname_other}", '');
                    }
                    if (bsd_customer != '') {
                        template = template.replace("{bsd_customer}", bsd_customer);
                    }
                    else {
                        template = template.replace("{bsd_customer}", '');
                    }
                    //template = template.replace("{bsd_accountname_other}", '');
                }

                template = template.replace("{fee}", '');
                template = template.replace("{interest_c}", '');
                template = template.replace("{listinterest}", '');
                template = template.replace("{listinstallment}", '');
                template = template.replace("{firstinstallment}", '');
                template = template.replace("{overpayment}", '');
                template = template.replace("{description_a}", '');
                template = template.replace("{queuing_amount}", '');

            }
            callback();
        })

    }
    else if (bsd_paymenttype == 100000002) {

        var xml = [];
        xml.push("<fetch version='1.0'>");
        xml.push("<entity name='bsd_payment' enableprefiltering='1' >");
        xml.push("<attribute name='bsd_latepayment' alias='latepayment' />");
        xml.push("<attribute name='bsd_transactiontype' alias='transactiontype' />");
        xml.push("<attribute name='bsd_differentamount' alias='Difference' />");
        xml.push("<attribute name='bsd_latepayment' alias='CheckInteres' />");
        xml.push("<attribute name='bsd_interestcharge' alias='interestchargeAmount' />");
        xml.push("<attribute name='bsd_name' alias='pm_name' />");
        xml.push("<attribute name='bsd_balance' alias='balance' />");
        xml.push("<attribute name='bsd_paymentactualtime' alias='pm_Date' />");
        xml.push("<attribute name='bsd_amountpay' alias='pm_amountpay' />");
        xml.push("<attribute name='bsd_overpayment' alias='overpayment' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_paymentid' operator='eq' uitype='bsd_payment' value='" + paymentid + "' />");
        xml.push("</filter>");
        xml.push("<link-entity name='salesorder' from='salesorderid' to='bsd_optionentry' link-type='outer' alias='order' >");
        xml.push("<attribute name='totalamount' alias='orderTotalamount' />");
        xml.push("<attribute name='bsd_project' alias='orderProject' />");
        xml.push("<link-entity name='account' from='accountid' to='customerid' link-type='outer' alias='account' >");
        xml.push("<attribute name='bsd_address' alias='acc_addVN' />");
        xml.push("<attribute name='bsd_diachi' alias='acc_addEN' />");
        xml.push("<attribute name='name' alias='accNameSys' />");
        xml.push("<attribute name='bsd_name' alias='accName' />");
        xml.push("<attribute name='bsd_accountnameother' alias='accNameEN' />");
        xml.push("</link-entity>");
        xml.push("<link-entity name='contact' from='contactid' to='customerid' link-type='outer' alias='cont' >");
        xml.push("<attribute name='fullname' alias='contNameSys' />");
        xml.push("<attribute name='bsd_fullname' alias='contName' />");
        xml.push("<attribute name='bsd_contactaddress' alias='cont_addVN' />");
        xml.push("<attribute name='bsd_diachi' alias='cont_addEN' />");
        xml.push("<attribute name='bsd_housenumber' alias='bsd_housenumber' />");
        xml.push("</link-entity>");
        xml.push("<link-entity name='product' from='productid' to='bsd_unitnumber' link-type='outer' alias='prod' >");
        xml.push("<attribute name='bsd_maintenancefees' alias='prod_duedatecalculatingmethod' />");
        xml.push("<attribute name='name' alias='unitcode' />");
        xml.push("<attribute name='bsd_unitscodesams' alias='bsd_unitscodesams' />");
        xml.push("</link-entity>");
        //        xml.push("<link-entity name='bsd_coowner' from='bsd_optionentry' to='salesorderid' link-type='outer' alias='coo' >");
        //        xml.push("<link-entity name='contact' from='contactid' to='bsd_customer' link-type='outer' alias='cooCont' >");
        //        xml.push("<attribute name='fullname' alias='ccontNameSys' />");
        //        xml.push("<attribute name='bsd_fullname' alias='ccontName' />");
        //        xml.push("</link-entity>");
        //        xml.push("<link-entity name='account' from='accountid' to='bsd_customer' link-type='outer' alias='cooAcc' >");
        //        xml.push("<attribute name='name' alias='cooaccNameSys' />");
        //        xml.push("<attribute name='bsd_name' alias='cooaccName' />");
        //        xml.push("<attribute name='bsd_accountnameother' alias='cooaccNameEN' />");
        //        xml.push("</link-entity>");
        //        xml.push("</link-entity>");
        xml.push("</link-entity>");
        xml.push("<link-entity name='bsd_transactionpayment' from='bsd_payment' to='bsd_paymentid' link-type='outer' alias='transaction' >");
        xml.push("<attribute name='bsd_transactiontype' alias='tran_transactiontype' />");
        xml.push("<attribute name='bsd_feetype' alias='feeType' />");
        xml.push("<attribute name='bsd_amount' alias='amount' />");
        xml.push("<link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_installment' link-type='outer' alias='installment' >");
        xml.push("<attribute name='bsd_ordernumber' alias='tran_number' />");
        xml.push("</link-entity>");
        xml.push("</link-entity>");
        xml.push("<link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_paymentschemedetail' link-type='outer' alias='psdetail' >");
        xml.push("<attribute name='bsd_managementfee' alias='psdetail_managementfee' />");
        xml.push("<attribute name='bsd_ordernumber' alias='Ordernumber' />");
        xml.push("<attribute name='bsd_maintenancefees' alias='psdetail_maintenancefees' />");
        xml.push("<attribute name='bsd_lastinstallment' alias='psdetail_lasinstall' />");
        xml.push("<attribute name='bsd_duedatecalculatingmethod' alias='psdetail_duedatecalculatingmethod' />");
        xml.push("<attribute name='bsd_name' alias='psdetail_name' />");
        xml.push("</link-entity>");
        xml.push("</entity>");
        xml.push("</fetch>");
        //console.log(xml.join(""));
        let rs = CrmFetchKitNew.FetchSync(xml.join(""));
        //        CrmFetchKitNew.Fetch(xml.join(""), true).then(function (rs) {
        //        console.log("bsd_paymenttype: " + bsd_paymenttype);
        //        console.log(rs);
        debugger;
        if (rs.length > 0) {

            //dòng tt1
            var stt = '';
            var tran_stt = '';
            var bsd_latepayment = rs[0].attributes['latepayment'] ? rs[0].attributes['latepayment'].value : '';
            var bsd_name = rs[0].attributes['accName'] ? rs[0].attributes['accName'].value : '';
            var bsd_accountnameother = rs[0].attributes['accNameEN'] ? rs[0].attributes['accNameEN'].value : '';
            var ac_address = rs[0].attributes['acc_addVN'] ? rs[0].attributes['acc_addVN'].value : '';
            var ac_diachi = rs[0].attributes['acc_addEN'] ? rs[0].attributes['acc_addEN'].value : '';
            var bsd_fullname = rs[0].attributes['contName'] ? rs[0].attributes['contName'].value : '';
            var bsd_contactaddress = rs[0].attributes['cont_addVN'] ? rs[0].attributes['cont_addVN'].value : '';
            var bsd_diachi = rs[0].attributes['cont_addEN'] ? rs[0].attributes['cont_addEN'].value : '';
            var bsd_housenumber = rs[0].attributes['bsd_housenumber'] ? rs[0].attributes['bsd_housenumber'].value : '';
            if (bsd_housenumber == '') {
                bsd_diachi = '';
            }

            if (bsd_fullname != '') {
                template = template.replace("{bsd_fullname}", bsd_fullname);
                template = template.replace("{bsd_contactaddress}", bsd_contactaddress);
                template = template.replace("{bsd_diachi}", bsd_diachi);
                template = template.replace("{bsd_accountname_other}", '');
            }
            else {
                template = template.replace("{bsd_fullname}", bsd_name);
                if (bsd_accountnameother != '') {
                    template = template.replace("{bsd_accountname_other}", '(' + bsd_accountnameother + ')');
                }
                else {
                    template = template.replace("{bsd_accountname_other}", '');
                }
                //                if (bsd_accountnameother != '') {
                //                    template = template.replace("{bsd_customer}", bsd_accountnameother);
                //                }
                //                else {
                //                    template = template.replace("{bsd_customer}", '');
                //                }
                template = template.replace("{bsd_contactaddress}", ac_address);
                template = template.replace("{bsd_diachi}", ac_diachi);
            }

            //            var bsd_customer = rs[0].attributes['ccontName'] ? rs[0].attributes['ccontName'].value : '';
            //            template = template.replace("{bsd_customer}", bsd_customer);
            //            var bsd_co_account = rs[0].attributes['cooaccName'] ? rs[0].attributes['cooaccName'].value : '';
            //            var bsd_co_accountEN = rs[0].attributes['cooaccNameEN'] ? rs[0].attributes['cooaccNameEN'].value : '';
            //
            //            if (bsd_co_account != '') {
            //                template = template.replace("{bsd_co_account}", bsd_co_account);
            //            }
            //            else {
            //                template = template.replace("{bsd_co_account}", '');
            //            }
            //
            //            if (bsd_co_accountEN != '') {
            //                template = template.replace("{bsd_co_accountEN}", '(' + bsd_co_accountEN + ')');
            //            }
            //            else {
            //                template = template.replace("{bsd_co_accountEN}", '');
            //            }
            var bsd_project = rs[0].attributes['orderProject'] ? rs[0].attributes['orderProject'].name : '';
            template = template.replace("{bsd_project}", bsd_project);

            //if (bsd_project == "MULBERRY LANE") {
            if (gb_projectcode == "MBL") {
                var bsd_units = rs[0].attributes['bsd_unitscodesams'] ? rs[0].attributes['bsd_unitscodesams'].value : '';
                template = template.replace("{bsd_units}", bsd_units);
            }
            else {
                var bsd_units = rs[0].attributes['unitcode'] ? rs[0].attributes['unitcode'].value : '';
                template = template.replace("{bsd_units}", bsd_units);
            }

            var totalamount = rs[0].attributes['orderTotalamount'] ? rs[0].attributes['orderTotalamount'].value : '';
            template = template.replace("{totalamount}", common.formatNumberEx1(totalamount.toString()));

            var row1 = '';

            var row2 = '';
            var balan_pay = 0;
            var rows_ins = '';
            var rows_inter = '';
            var bsd_differentamount = rs[0].attributes['Difference'] ? rs[0].attributes['Difference'].value : 0;

            var checkinteres = rs[0].attributes['CheckInteres'] ? rs[0].attributes['CheckInteres'].value : '';
            var number = rs[0].attributes['Ordernumber'] ? rs[0].attributes['Ordernumber'].value : 0;

            for (var i = 0; i < rs.length; i++) {
                var lastNumber = number % 10;
                if (number == 1) {
                    stt = "st";
                }
                else if (number == 2) {
                    stt = "nd";
                }
                else if (number == 3) {
                    stt = "rd";
                }
                else if (number > 20 && lastNumber == 1) {
                    stt = "st";
                }
                else if (number > 20 && lastNumber == 2) {
                    stt = "nd";
                }
                else if (number > 20 && lastNumber == 3) {
                    stt = "rd";
                }
                else {
                    stt = "th";
                }
            }

            //dòng tt1
            var bsd_amount = rs[0].attributes['amount'] ? rs[0].attributes['amount'].value : 0;
            if (bsd_differentamount != '' && bsd_differentamount > 0) {
                balan_pay = rs[0].attributes['balance'] ? rs[0].attributes['balance'].value : 0;
            }
            else {
                balan_pay = rs[0].attributes['pm_amountpay'] ? rs[0].attributes['pm_amountpay'].value : 0;
            }

            row1 = [' <tr>', '<td height="20px">' + number + '' + stt + ' Installment / Thanh toán lần thứ ' + number + '</td>', '<td style="text-align:right">' + common.formatNumberEx1(balan_pay.toString()) + '</td>', '</tr>'].join("");
            template = template.replace("{firstinstallment}", row1);
            if (number == 1) {
                template = template.replace("{listinterest}", '');
            }

            //Listinstallment
            var co_a = false;
            for (var r = 0; r < rs.length; r++) {
                //var co_b = false;
                var row = '';
                var bsd_transactiontype = rs[r].attributes['tran_transactiontype'] ? rs[r].attributes['tran_transactiontype'].value : 0;
                var ins_amount = rs[r].attributes['amount'] ? rs[r].attributes['amount'].value : 0;
                var tran_number = rs[r].attributes['tran_number'] ? rs[r].attributes['tran_number'].value : 0;
                if (r > 1) {
                    var tran_number_a = rs[r].attributes['tran_number'] ? rs[r].attributes['tran_number'].value : 0;
                    if (r + 1 <= rs.length - 1) {
                        var tran_number_b = rs[r + 1].attributes['tran_number'] ? rs[r + 1].attributes['tran_number'].value : 0;
                        if (tran_number_a == tran_number_b) {
                            rows_ins = rows_ins;
                            rows_inter = rows_inter;
                            break;
                        }
                    }
                }

                for (var j = 0; j < rs.length; j++) {
                    var tran_lastNumber = tran_number % 10;
                    if (tran_number == 1) {
                        tran_stt = "st";
                    }
                    else if (tran_number == 2) {
                        tran_stt = "nd";
                    }
                    else if (tran_number == 3) {
                        tran_stt = "rd";
                    }
                    else if (tran_number > 20 && tran_lastNumber == 1) {
                        tran_stt = "st";
                    }
                    else if (tran_number > 20 && tran_lastNumber == 2) {
                        tran_stt = "nd";
                    }
                    else if (tran_number > 20 && tran_lastNumber == 3) {
                        tran_stt = "rd";
                    }
                    else {
                        tran_stt = "th";
                    }
                }
                //dòng tt2
                if (bsd_transactiontype == 100000000) {
                    co_a = true;
                    row = [' <tr>', '<td height="25px">' + tran_number + '' + tran_stt + ' Installment / Thanh toán lần thứ ' + tran_number + '</td>', '<td style="text-align:right">' + common.formatNumberEx1(ins_amount.toString()) + '</td>', '</tr>'].join("");
                }

                if (bsd_transactiontype == 100000001) {
                    co_a = true;
                    row = [' <tr>', '<td height="25px">' + tran_number + '' + tran_stt + ' Installment Interest Charge / Trả lãi suất cho đợt thứ ' + tran_number + '</td>', '<td style="text-align:right">' + common.formatNumberEx1(ins_amount.toString()) + '</td>', '</tr>'].join("");
                }

                //dòng tl1
                var bsd_interestcharge = rs[r].attributes['interestchargeAmount'] ? rs[r].attributes['interestchargeAmount'].value : 0;

                if (bsd_interestcharge > bsd_differentamount) {
                    if (bsd_latepayment == true) {
                        co_a = true;
                        row2 = ["<tr>", "<td height='25px'>" + number + "" + stt + " Installment Interest Charge / Trả lãi suất cho đợt thứ " + number + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(bsd_differentamount.toString()) + "</td>", "</tr>"].join("");
                    }
                    else {
                        co_a = true;
                        row2 = ["<tr>", "<td height='25px'>" + tran_number + "" + tran_stt + " Installment Interest Charge / Trả lãi suất cho đợt thứ " + tran_number + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(bsd_differentamount.toString()) + "</td>", "</tr>"].join("");
                    }
                }
                else {
                    if (bsd_latepayment == true) {
                        co_a = true;
                        row2 = ["<tr>", "<td height='25px'>" + number + "" + stt + " Installment Interest Charge / Trả lãi suất cho đợt thứ " + number + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(bsd_interestcharge.toString()) + "</td>", "</tr>"].join("");
                    }
                    else {
                        co_a = true;
                        row2 = ["<tr>", "<td height='25px'>" + tran_number + "" + tran_stt + " Installment Interest Charge / Trả lãi suất cho đợt thứ " + tran_number + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(bsd_interestcharge.toString()) + "</td>", "</tr>"].join("");
                    }
                }

                if (checkinteres == false || !checkinteres) {
                    row2 = '';
                }

                if (co_a == false && r == rs.length - 1) {
                    row = '';
                    row2 = '';
                }
                rows_ins += row;
                rows_inter += row2;

            }

            template = template.replace("{listinstallment}", rows_ins);
            template = template.replace("{listinterest}", rows_inter);

            //FEE.........
            var fee_EN = '';
            var fee_VN = '';
            var bsd_feetype = 0;
            var row_fee = '';
            var fee_amount = 0;
            var rows = '';
            var Co = false;
            //
            //                debugger;
            for (var k = 0; k < rs.length; k++) {
                fee_amount = rs[k].attributes['amount'] ? rs[k].attributes['amount'].value : 0;
                bsd_feetype = rs[k].attributes['feeType'] ? rs[k].attributes['feeType'].value : 0;

                if (bsd_feetype != 0) {
                    Co = true;
                    if (bsd_feetype == 100000000) {
                        fee_EN = 'Maintenance Fee';
                        fee_VN = 'Phí bảo trì';
                        // if (bsd_transactiontype == 100000002) {
                        row_fee = ["<tr>", "<td height='25px'>" + fee_EN + " / " + fee_VN + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(fee_amount.toString()) + "</td>", "</tr>"].join("");
                        //rows += row_fee;
                        // }
                    }
                    else if (bsd_feetype == 100000001) {
                        fee_EN = 'Management Fee';
                        fee_VN = 'Phí quản lý';
                        // if (bsd_transactiontype == 100000002) {
                        row_fee = ["<tr>", "<td height='25px'>" + fee_EN + " / " + fee_VN + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(fee_amount.toString()) + "</td>", "</tr>"].join("");
                        // rows += row_fee;
                        // }
                    }
                    else {
                        fee_EN = 'Installment Fee';
                        fee_VN = 'Thanh toán phí';
                        //  if (bsd_transactiontype == 100000002) {
                        row_fee = ["<tr>", "<td height='25px'>" + fee_EN + " / " + fee_VN + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(fee_amount.toString()) + "</td>", "</tr>"].join("");
                        //rows += row_fee;
                        // }
                    }

                    rows += row_fee;
                }
                else {
                    if (Co == false && k == rs.length - 1) {
                        template = template.replace("{fee}", '');
                    }
                }
            }

            template = template.replace("{fee}", rows);

            template = template.replace("{interest_c}", '');
            template = template.replace("{overpayment}", '');
            template = template.replace("{description_a}", '');
            template = template.replace("{queuing_amount}", '');

        }

        let fetchXml = ["<fetch version='1.0'>", "  <entity name='bsd_payment' enableprefiltering='1'>", "    <filter type='and'>", "      <condition attribute='bsd_paymentid' operator='eq' uitype='bsd_payment' value='" + paymentid + "' />", "    </filter>", "    <link-entity name='salesorder' from='salesorderid' to='bsd_optionentry' link-type='outer' alias='order'>", "      <link-entity name='bsd_coowner' from='bsd_optionentry' to='salesorderid' link-type='outer' alias='coo'>", "        <link-entity name='contact' from='contactid' to='bsd_customer' link-type='outer' alias='cooCont'>", "          <attribute name='bsd_fullname' alias='ccontName' />", "        </link-entity>", "        <link-entity name='account' from='accountid' to='bsd_customer' link-type='outer' alias='cooAcc'>", "          <attribute name='bsd_name' alias='cooaccName' />", "        </link-entity>", "      </link-entity>", "    </link-entity>", "  </entity>", "</fetch>", ].join("");
        let listcoowner = CrmFetchKitNew.FetchSync(fetchXml);
        debugger;
        if (listcoowner.length > 0) {

            let bsd_coowner = "";
            for (
            let i = 0; i < listcoowner.length; i++) {
                if (listcoowner[i].attributes['ccontName']) {
                    bsd_coowner += ["<span style=\"display: block; line-height:5px;\" ><b>" + listcoowner[i].attributes['ccontName'].value + "</b></span>", ].join("");
                    if (i != listcoowner.length - 1) bsd_coowner += "<br>";
                }
                if (listcoowner[i].attributes['cooaccName']) {
                    bsd_coowner += ["<span style=\"display: block;line-height:5px;\" ><b>" + listcoowner[i].attributes['cooaccName'].value + "</b></span>", ].join("");
                    if (i != listcoowner.length - 1) bsd_coowner += "<br>";
                }
            }
            template = template.replace("{bsd_customer}", bsd_coowner);
        }
        callback();
        //        });
    }
    else if (bsd_paymenttype == 100000003 || bsd_paymenttype == 100000004) {

        var xml = [];
        xml.push("<fetch version='1.0'>");
        xml.push("<entity name='bsd_payment' enableprefiltering='1' >");
        xml.push("<attribute name='bsd_differentamount' alias='Difference' />");
        xml.push("<attribute name='bsd_overpayment' alias='overpayment' />");
        xml.push("<attribute name='bsd_latepayment' alias='CheckInteres' />");
        xml.push("<attribute name='bsd_interestcharge' alias='interestchargeAmount' />");
        xml.push("<attribute name='bsd_name' alias='pm_name' />");
        xml.push("<attribute name='bsd_balance' alias='balance' />");
        xml.push("<attribute name='bsd_paymentactualtime' alias='pm_Date' />");
        xml.push("<attribute name='bsd_amountpay' alias='pm_amountpay' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_paymentid' operator='eq' uitype='bsd_payment' value='" + paymentid + "' />");
        xml.push("</filter>");
        //===>saleother
        xml.push("<link-entity name='salesorder' from='salesorderid' to='bsd_optionentry' link-type='outer' alias='order' >");
        xml.push("<attribute name='totalamount' alias='orderTotalamount' />");
        xml.push("<attribute name='bsd_project' alias='orderProject' />");
        ////===>account
        xml.push("<link-entity name='account' from='accountid' to='customerid' link-type='outer' alias='account' >");
        xml.push("<attribute name='bsd_address' alias='acc_addVN' />");
        xml.push("<attribute name='bsd_diachi' alias='acc_addEN' />");
        xml.push("<attribute name='name' alias='accNameSys' />");
        xml.push("<attribute name='bsd_name' alias='accName' />");
        xml.push("<attribute name='bsd_accountnameother' alias='accNameEN' />");
        xml.push("</link-entity>");
        //==>account
        //==>contact
        xml.push("<link-entity name='contact' from='contactid' to='customerid' link-type='outer' alias='cont' >");
        xml.push("<attribute name='fullname' alias='contNameSys' />");
        xml.push("<attribute name='bsd_fullname' alias='contName' />");
        xml.push("<attribute name='bsd_contactaddress' alias='cont_addVN' />");
        xml.push("<attribute name='bsd_diachi' alias='cont_addEN' />");
        xml.push("<attribute name='bsd_housenumber' alias='bsd_housenumber' />");
        xml.push("</link-entity>");
        ////==>contact
        xml.push("<link-entity name='product' from='productid' to='bsd_unitnumber' link-type='outer' alias='prod' >");
        xml.push("<attribute name='bsd_maintenancefees' alias='prod_duedatecalculatingmethod' />");
        xml.push("<attribute name='name' alias='unitcode' />");
        xml.push("<attribute name='bsd_unitscodesams' alias='bsd_unitscodesams' />");
        xml.push("</link-entity>");
        ////cow
        xml.push("<link-entity name='bsd_coowner' from='bsd_optionentry' to='salesorderid' link-type='outer' alias='coo' >");
        //====>contact
        xml.push("<link-entity name='contact' from='contactid' to='bsd_customer' link-type='outer' alias='cooCont' >");
        xml.push("<attribute name='fullname' alias='ccontNameSys' />");
        xml.push("<attribute name='bsd_fullname' alias='ccontName' />");
        xml.push("</link-entity>");
        //contact
        //==>account
        xml.push("<link-entity name='account' from='accountid' to='bsd_customer' link-type='outer' alias='cooAcc' >");
        xml.push("<attribute name='name' alias='cooaccNameSys' />");
        xml.push("<attribute name='bsd_name' alias='cooaccName' />");
        xml.push("<attribute name='bsd_accountnameother' alias='cooaccNameEN' />");
        xml.push("</link-entity>");
        //==>acc
        xml.push("</link-entity>");
        ////==>cow
        xml.push("</link-entity>");
        ////==>sale
        ////==>trans
        xml.push("<link-entity name='bsd_transactionpayment' from='bsd_payment' to='bsd_paymentid' link-type='outer' alias='transaction' >");
        xml.push("<attribute name='bsd_transactiontype' alias='transactiontype' />");
        xml.push("<attribute name='bsd_feetype' alias='feeType' />");
        xml.push("<attribute name='bsd_amount' alias='amount' />");
        //====>pmdetail
        xml.push("<link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_installment' link-type='outer' alias='installment' >");
        xml.push("<attribute name='bsd_ordernumber' alias='tran_number' />");
        xml.push("</link-entity>");
        //===>pmđetail
        //======>mis
        xml.push("<link-entity name='bsd_miscellaneous' from='bsd_miscellaneousid' to='bsd_miscellaneous' link-type='outer' alias='mis' >");
        xml.push("<attribute name='bsd_description' alias='tran_Description' />");
        xml.push("</link-entity>");
        ////===>mis
        xml.push("</link-entity>");
        //==>trans
        //////mis
        //xml.push("<link-entity name='bsd_miscellaneous' from='bsd_miscellaneousid' to='bsd_miscellaneous' link-type='outer' alias='pay_mis' >");
        //xml.push("<attribute name='bsd_description' alias='Description' />");
        //xml.push("</link-entity>");
        ////==>mis
        xml.push("</entity>");
        xml.push("</fetch>");
        CrmFetchKitNew.Fetch(xml.join(""), true).then(function (rs) {
            console.log("bsd_paymenttype: " + bsd_paymenttype);
            console.log(rs);
            if (rs.length > 0) {

                var stt = '';
                var tran_stt = '';
                var interest = '';
                var payinterest = '';

                var bsd_transactiontype = rs[0].attributes['transactiontype'] ? rs[0].attributes['transactiontype'].value : 0;
                var bsd_accountnameother = rs[0].attributes['accNameEN'] ? rs[0].attributes['accNameEN'].value : '';
                var bsd_name = rs[0].attributes['accName'] ? rs[0].attributes['accName'].value : '';
                var ac_address = rs[0].attributes['acc_addVN'] ? rs[0].attributes['acc_addVN'].value : '';
                var ac_diachi = rs[0].attributes['acc_addEN'] ? rs[0].attributes['acc_addEN'].value : '';
                var bsd_fullname = rs[0].attributes['contName'] ? rs[0].attributes['contName'].value : '';
                var bsd_contactaddress = rs[0].attributes['cont_addVN'] ? rs[0].attributes['cont_addVN'].value : '';
                var bsd_diachi = rs[0].attributes['cont_addEN'] ? rs[0].attributes['cont_addEN'].value : '';
                var bsd_housenumber = rs[0].attributes['bsd_housenumber'] ? rs[0].attributes['bsd_housenumber'].value : '';
                if (bsd_housenumber == '') {
                    bsd_diachi = '';
                }

                if (bsd_fullname != '') {
                    template = template.replace("{bsd_fullname}", bsd_fullname);
                    template = template.replace("{bsd_contactaddress}", bsd_contactaddress);
                    template = template.replace("{bsd_diachi}", bsd_diachi);
                    template = template.replace("{bsd_accountname_other}", '');
                }
                else {
                    template = template.replace("{bsd_fullname}", bsd_name);
                    if (bsd_accountnameother != '') {
                        template = template.replace("{bsd_customer}", bsd_accountnameother);
                        template = template.replace("{bsd_accountname_other}", '(' + bsd_accountnameother + ')');
                    }
                    else {
                        template = template.replace("{bsd_customer}", '');
                        template = template.replace("{bsd_accountname_other}", '');
                    }
                    template = template.replace("{bsd_contactaddress}", ac_address);
                    template = template.replace("{bsd_diachi}", ac_diachi);
                }

                var bsd_customer = rs[0].attributes['ccontName'] ? rs[0].attributes['ccontName'].value : '';
                template = template.replace("{bsd_customer}", bsd_customer);

                var bsd_co_account = rs[0].attributes['cooaccName'] ? rs[0].attributes['cooaccName'].value : '';
                var bsd_co_accountEN = rs[0].attributes['cooaccNameEN'] ? rs[0].attributes['cooaccNameEN'].value : '';

                if (bsd_co_account != '') {
                    template = template.replace("{bsd_co_account}", bsd_co_account);
                }
                else {
                    template = template.replace("{bsd_co_account}", '');
                }

                if (bsd_co_accountEN != '') {
                    template = template.replace("{bsd_co_accountEN}", '(' + bsd_co_accountEN + ')');
                }
                else {
                    template = template.replace("{bsd_co_accountEN}", '');
                }

                var bsd_project = rs[0].attributes['orderProject'] ? rs[0].attributes['orderProject'].name : '';
                template = template.replace("{bsd_project}", bsd_project);

                //if (bsd_project == "MULBERRY LANE") {
                if (gb_projectcode == "MBL") {
                    var bsd_units = rs[0].attributes['bsd_unitscodesams'] ? rs[0].attributes['bsd_unitscodesams'].value : '';
                    template = template.replace("{bsd_units}", bsd_units);
                }
                else {
                    var bsd_units = rs[0].attributes['unitcode'] ? rs[0].attributes['unitcode'].value : '';
                    template = template.replace("{bsd_units}", bsd_units);
                }

                var totalamount = rs[0].attributes['orderTotalamount'] ? rs[0].attributes['orderTotalamount'].value : '';
                template = template.replace("{totalamount}", common.formatNumberEx1(totalamount.toString()));

                //dòng tl cuối
                var bsd_amountpay = rs[0].attributes['pm_amountpay'] ? rs[0].attributes['pm_amountpay'].value : 0;
                var bsd_balance = rs[0].attributes['balance'] ? rs[0].attributes['balance'].value : 0;
                var row5 = '';
                var rows_charg = '';

                if (bsd_paymenttype == 100000003) {
                    for (var i = 0; i < rs.length; i++) {
                        var bsd_amount = rs[i].attributes['amount'] ? rs[i].attributes['amount'].value : 0;
                        var tran_number = rs[i].attributes['tran_number'] ? rs[i].attributes['tran_number'].value : 0;

                        for (var j = 0; j < rs.length; j++) {
                            var tran_lastNumber = tran_number % 10;
                            if (tran_number == 1) {
                                tran_stt = "st";
                            }
                            else if (tran_number == 2) {
                                tran_stt = "nd";
                            }
                            else if (tran_number == 3) {
                                tran_stt = "rd";
                            }
                            else if (tran_number > 20 && tran_lastNumber == 1) {
                                tran_stt = "st";
                            }
                            else if (tran_number > 20 && tran_lastNumber == 2) {
                                tran_stt = "nd";
                            }
                            else if (tran_number > 20 && tran_lastNumber == 3) {
                                tran_stt = "rd";
                            }
                            else {
                                tran_stt = "th";
                            }
                        }

                        if (tran_number == 0) {
                            if (Co == false && i == rs.length - 1) {
                                template = template.replace("{interest_c}", '');
                            }
                        }
                        else {
                            if (bsd_transactiontype == 100000001) {
                                Co = true;
                                row5 = ["<tr>", "<td height='30px'>" + tran_number + "" + tran_stt + " Installment Interest Charge / Trả lãi suất cho đợt thứ " + tran_number + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(bsd_amount.toString()) + "</td>", "</tr>"].join("");
                                rows_charg += row5;
                            }

                        }

                    }
                    template = template.replace("{interest_c}", rows_charg);
                }
                else {
                    template = template.replace("{interest_c}", '');
                }

                ////Thanh toán thừa
                var bsd_overpayment = rs[0].attributes['overpayment'] ? rs[0].attributes['overpayment'].value : 0;
                if (bsd_overpayment == 0 || !bsd_overpayment) {
                    template = template.replace("{overpayment}", '');
                }
                else {
                    if (bsd_paymenttype == 100000003) {
                        var row_opm = ["<tr>", "<td height='30px'>Over Payment / Thanh toán thừa</td>", "<td style='text-align:right'>" + common.formatNumberEx1(bsd_overpayment.toString()) + "</td>", "</tr>"].join("");
                        template = template.replace("{overpayment}", row_opm);
                    }
                    else {
                        template = template.replace("{overpayment}", '');
                    }

                }

                //FEE.........
                if (bsd_paymenttype == 100000004) {
                    var fee_EN = '';
                    var fee_VN = '';
                    var bsd_feetype = 0;
                    var row_fee = '';
                    var fee_amount = 0;
                    var rows = '';
                    var Co = false;

                    for (var k = 0; k < rs.length; k++) {
                        fee_amount = rs[k].attributes['amount'] ? rs[k].attributes['amount'].value : 0;
                        bsd_feetype = rs[k].attributes['feeType'] ? rs[k].attributes['feeType'].value : 0;

                        if (bsd_feetype != 0) {
                            Co = true;
                            if (bsd_feetype == 100000000) {
                                fee_EN = 'Maintenance Fee';
                                fee_VN = 'Phí bảo trì';
                                // if (bsd_transactiontype == 100000002) {
                                row_fee = ["<tr>", "<td height='30px'>" + fee_EN + " / " + fee_VN + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(fee_amount.toString()) + "</td>", "</tr>"].join("");
                                //rows += row_fee;
                                // }
                            }
                            else if (bsd_feetype == 100000001) {
                                fee_EN = 'Management Fee';
                                fee_VN = 'Phí quản lý';
                                // if (bsd_transactiontype == 100000002) {
                                row_fee = ["<tr>", "<td height='30px'>" + fee_EN + " / " + fee_VN + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(fee_amount.toString()) + "</td>", "</tr>"].join("");
                                // rows += row_fee;
                                // }
                            }
                            else {
                                fee_EN = 'Installment Fee';
                                fee_VN = 'Thanh toán phí';
                                //  if (bsd_transactiontype == 100000002) {
                                row_fee = ["<tr>", "<td height='30px'>" + fee_EN + " / " + fee_VN + "</td>", "<td style='text-align:right'>" + common.formatNumberEx1(fee_amount.toString()) + "</td>", "</tr>"].join("");
                                //rows += row_fee;
                                // }
                            }

                            rows += row_fee;
                        }
                        else {
                            if (Co == false && k == rs.length - 1) {
                                template = template.replace("{fee}", '');
                            }
                        }
                    }

                    template = template.replace("{fee}", rows);
                }
                else {
                    template = template.replace("{fee}", '');
                }
                template = template.replace("{queuing_amount}", '');
                template = template.replace("{description_a}", '');
                template = template.replace("{listinstallment}", '');
                template = template.replace("{firstinstallment}", '');
                template = template.replace("{listinterest}", '');
            }
            callback();
        });
    }
    else {

        var xml = [];
        xml.push("<fetch version='1.0'>");
        xml.push("<entity name='bsd_payment' enableprefiltering='1' >");
        xml.push("<attribute name='bsd_balance' alias='balance' />");
        xml.push("<attribute name='bsd_amountpay' alias='pm_amountpay' />");
        xml.push("<filter type='and'>");
        xml.push("<condition attribute='bsd_paymentid' operator='eq' uitype='bsd_payment' value='" + paymentid + "' />");
        xml.push("</filter>");
        //===>saleother
        xml.push("<link-entity name='salesorder' from='salesorderid' to='bsd_optionentry' link-type='outer' alias='order' >");
        xml.push("<attribute name='totalamount' alias='orderTotalamount' />");
        xml.push("<attribute name='bsd_project' alias='orderProject' />");
        ////===>account
        xml.push("<link-entity name='account' from='accountid' to='customerid' link-type='outer' alias='account' >");
        xml.push("<attribute name='bsd_address' alias='acc_addVN' />");
        xml.push("<attribute name='bsd_diachi' alias='acc_addEN' />");
        xml.push("<attribute name='name' alias='accNameSys' />");
        xml.push("<attribute name='bsd_name' alias='accName' />");
        xml.push("<attribute name='bsd_accountnameother' alias='accNameEN' />");
        xml.push("</link-entity>");
        //==>account
        //==>contact
        xml.push("<link-entity name='contact' from='contactid' to='customerid' link-type='outer' alias='cont' >");
        xml.push("<attribute name='fullname' alias='contNameSys' />");
        xml.push("<attribute name='bsd_fullname' alias='contName' />");
        xml.push("<attribute name='bsd_contactaddress' alias='cont_addVN' />");
        xml.push("<attribute name='bsd_diachi' alias='cont_addEN' />");
        xml.push("<attribute name='bsd_housenumber' alias='bsd_housenumber' />");
        xml.push("</link-entity>");
        ////==>contact
        xml.push("<link-entity name='product' from='productid' to='bsd_unitnumber' link-type='outer' alias='prod' >");
        xml.push("<attribute name='bsd_maintenancefees' alias='prod_duedatecalculatingmethod' />");
        xml.push("<attribute name='name' alias='unitcode' />");
        xml.push("<attribute name='bsd_unitscodesams' alias='bsd_unitscodesams' />");
        xml.push("</link-entity>");
        ////cow
        xml.push("<link-entity name='bsd_coowner' from='bsd_optionentry' to='salesorderid' link-type='outer' alias='coo' >");
        //====>contact
        xml.push("<link-entity name='contact' from='contactid' to='bsd_customer' link-type='outer' alias='cooCont' >");
        xml.push("<attribute name='fullname' alias='ccontNameSys' />");
        xml.push("<attribute name='bsd_fullname' alias='ccontName' />");
        xml.push("</link-entity>");
        //contact
        //==>account
        xml.push("<link-entity name='account' from='accountid' to='bsd_customer' link-type='outer' alias='cooAcc' >");
        xml.push("<attribute name='name' alias='cooaccNameSys' />");
        xml.push("<attribute name='bsd_name' alias='cooaccName' />");
        xml.push("<attribute name='bsd_accountnameother' alias='cooaccNameEN' />");
        xml.push("</link-entity>");
        //==>acc
        xml.push("</link-entity>");
        ////==>cow
        xml.push("</link-entity>");
        ////==>sale
        ////==>trans
        xml.push("<link-entity name='bsd_transactionpayment' from='bsd_payment' to='bsd_paymentid' link-type='outer' alias='transaction' >");
        xml.push("<attribute name='bsd_transactiontype' alias='transactiontype' />");
        xml.push("<attribute name='bsd_feetype' alias='feeType' />");
        xml.push("<attribute name='bsd_amount' alias='amount' />");
        xml.push("</link-entity>");
        //==>trans
        //////mis
        xml.push("<link-entity name='bsd_miscellaneous' from='bsd_miscellaneousid' to='bsd_miscellaneous' link-type='outer' alias='pay_mis' >");
        xml.push("<attribute name='bsd_description' alias='Description' />");
        xml.push("</link-entity>");
        ////==>mis
        xml.push("</entity>");
        xml.push("</fetch>");
        console.log(xml.join(""));
        CrmFetchKitNew.Fetch(xml.join(""), true).then(function (rs) {
            console.log("bsd_paymenttype: " + bsd_paymenttype);
            console.log(rs);
            if (rs.length > 0) {

                var bsd_accountnameother = rs[0].attributes['accNameEN'] ? rs[0].attributes['accNameEN'].value : '';
                var bsd_name = rs[0].attributes['accName'] ? rs[0].attributes['accName'].value : '';
                var ac_address = rs[0].attributes['acc_addVN'] ? rs[0].attributes['acc_addVN'].value : '';
                var ac_diachi = rs[0].attributes['acc_addEN'] ? rs[0].attributes['acc_addEN'].value : '';
                var bsd_fullname = rs[0].attributes['contName'] ? rs[0].attributes['contName'].value : '';
                var bsd_contactaddress = rs[0].attributes['cont_addVN'] ? rs[0].attributes['cont_addVN'].value : '';
                var bsd_diachi = rs[0].attributes['cont_addEN'] ? rs[0].attributes['cont_addEN'].value : '';
                var bsd_housenumber = rs[0].attributes['bsd_housenumber'] ? rs[0].attributes['bsd_housenumber'].value : '';
                if (bsd_housenumber == '') {
                    bsd_diachi = '';
                }

                if (bsd_fullname != '') {
                    template = template.replace("{bsd_fullname}", bsd_fullname);
                    template = template.replace("{bsd_contactaddress}", bsd_contactaddress);
                    template = template.replace("{bsd_diachi}", bsd_diachi);
                    template = template.replace("{bsd_accountname_other}", "");
                }
                else {
                    //template = template.replace("{bsd_fullname}", bsd_name);
                    //if (bsd_accountnameother != '') {
                    //    template = template.replace("{bsd_customer}", bsd_accountnameother);
                    //}
                    //else {
                    //    template = template.replace("{bsd_customer}", '');
                    //}
                    template = template.replace("{bsd_fullname}", bsd_name);
                    if (bsd_accountnameother != '') {
                        template = template.replace("{bsd_customer}", bsd_accountnameother);
                        template = template.replace("{bsd_accountname_other}", '(' + bsd_accountnameother + ')');
                    }
                    else {
                        template = template.replace("{bsd_customer}", '');
                        template = template.replace("{bsd_accountname_other}", '');
                    }
                    template = template.replace("{bsd_contactaddress}", ac_address);
                    template = template.replace("{bsd_diachi}", ac_diachi);
                }

                var bsd_customer = rs[0].attributes['ccontName'] ? rs[0].attributes['ccontName'].value : '';
                template = template.replace("{bsd_customer}", bsd_customer);

                var bsd_co_account = rs[0].attributes['cooaccName'] ? rs[0].attributes['cooaccName'].value : '';
                var bsd_co_accountEN = rs[0].attributes['cooaccNameEN'] ? rs[0].attributes['cooaccNameEN'].value : '';

                if (bsd_co_account != '') {
                    template = template.replace("{bsd_co_account}", bsd_co_account);
                }
                else {
                    template = template.replace("{bsd_co_account}", '');
                }

                if (bsd_co_accountEN != '') {
                    template = template.replace("{bsd_co_accountEN}", '(' + bsd_co_accountEN + ')');
                }
                else {
                    template = template.replace("{bsd_co_accountEN}", '');
                }

                var bsd_project = rs[0].attributes['orderProject'] ? rs[0].attributes['orderProject'].name : '';
                template = template.replace("{bsd_project}", bsd_project);

                //if (bsd_project == "MULBERRY LANE") {
                if (gb_projectcode == "MBL") {
                    var bsd_units = rs[0].attributes['bsd_unitscodesams'] ? rs[0].attributes['bsd_unitscodesams'].value : '';
                    template = template.replace("{bsd_units}", bsd_units);
                }
                else {
                    var bsd_units = rs[0].attributes['unitcode'] ? rs[0].attributes['unitcode'].value : '';
                    template = template.replace("{bsd_units}", bsd_units);
                }

                var totalamount = rs[0].attributes['orderTotalamount'] ? rs[0].attributes['orderTotalamount'].value : '';
                template = template.replace("{totalamount}", common.formatNumberEx1(totalamount.toString()));

                //Description_a
                var row_des = '';
                var row = '';
                for (var i = 0; i < rs.length; i++) {
                    var description_a = rs[i].attributes['Description'] ? rs[i].attributes['Description'].value : '';
                    //var bsd_transactiontype = rs[i].attributes['transactiontype'] ? rs[i].attributes['transactiontype'].value : 0;
                    var bsd_balance = rs[i].attributes['balance'] ? rs[i].attributes['balance'].value : 0;
                    var bsd_amountpay = rs[i].attributes['pmAmountPay'] ? rs[i].attributes['pmAmountPay'].value : 0;

                    if (bsd_balance > bsd_amountpay) {
                        if (description_a == '' && bsd_amountpay == 0) {
                            template = template.replace("{description_a}", '');
                        }
                        else {
                            if (bsd_amountpay < 0) {
                                row = [' <tr>', '<td height="25px">' + description_a + '</td>', '<td style="text-align:right">(' + common.formatNumberEx1(bsd_amountpay.toString()) + ')</td>', '</tr>'].join("");
                            }
                            else {
                                row = [' <tr>', '<td height="25px">' + description_a + '</td>', '<td style="text-align:right">' + common.formatNumberEx1(bsd_amountpay.toString()) + '</td>', '</tr>'].join("");
                            }
                        }

                    }
                    else {
                        if (description_a == '' && bsd_balance == 0) {
                            template = template.replace("{description_a}", '');
                        }
                        else {
                            if (bsd_balance < 0) {
                                row = [' <tr>', '<td height="25px">' + description_a + '</td>', '<td style="text-align:right">(' + common.formatNumberEx1(bsd_balance.toString()) + ')</td>', '</tr>'].join("");
                            }
                            else {
                                row = [' <tr>', '<td height="25px">' + description_a + '</td>', '<td style="text-align:right">' + common.formatNumberEx1(bsd_balance.toString()) + '</td>', '</tr>'].join("");
                            }
                        }
                    }
                    row_des += row;
                }

                template = template.replace("{description_a}", row_des);

                template = template.replace("{queuing_amount}", '');
                template = template.replace("{listinterest}", '');
                template = template.replace("{listinstallment}", '');
                template = template.replace("{firstinstallment}", '');
                template = template.replace("{interest_c}", '');
                template = template.replace("{fee}", '');
                template = template.replace("{overpayment}", '');
            }
            callback();
        });
    }

}

function updatePrintDate(idrecord) {
    debugger;
    console.log("updatePrintDate");
    var date = new Date();
    var day = date.getDate(); // yields date
    var month = date.getMonth() + 1; // yields month (add one as '.getMonth()' is zero indexed)
    var year = date.getFullYear(); // yields year
    var hour = date.getHours(); // yields hours
    var minute = date.getMinutes(); // yields minutes
    var second = date.getSeconds(); // yields seconds
    // After this construct a string with the above results as below
    var time = year + "/" + common.numberToString(month, 2) + "/" + common.numberToString(day, 2) + " " + common.numberToString(hour, 2) + ':' + common.numberToString(minute, 2) + ':' + common.numberToString(second, 2);
    var cols = ["bsd_receiptprinteddate"];
    var vals = [time];
    var types = ["datetime"];
    var checkDateUpdate = checkPrintReciptDate(idrecord);
    if (checkDateUpdate == true) {
        paymentModel.updateCols(idrecord, cols, vals, types, function (result) {

            if (result != null) {
                if (result.status == "error") {
                    var ss = result.data.split(':');
                    var mss = ss[ss.length - 1];

                }
                else if (result.status == "success") {}
            }
        });
    }

}

function checkPrintReciptDate(idrecord) {
    var check = false;
    var fetchData = {
        bsd_paymentid: idrecord
    };
    var fetchXml = ["<fetch>", "  <entity name='bsd_payment'>", "    <attribute name='bsd_receiptprinteddate' />", "    <filter>", "      <condition attribute='bsd_paymentid' operator='eq' value='", fetchData.bsd_paymentid
    /*fd044f66-7e9f-4c11-814d-7b4e28505381*/
    , "'/>", "    </filter>", "  </entity>", "</fetch>", ].join("");
    var entities = CrmFetchKitNew.FetchSync(fetchXml);
    if (entities.length > 0) {
        var bsd_receiptprinteddate = entities[0].attributes.bsd_receiptprinteddate != undefined ? entities[0].attributes.bsd_receiptprinteddate.formattedValue : '';
        if (bsd_receiptprinteddate == '' || bsd_receiptprinteddate == undefined) {
            check = true;
        }
    }
    return check;
}

function getAdvancePayment(paymentid, callback) {
    debugger;
    var xml = [];
    var statuscode;
    xml.push("<fetch>");
    xml.push("<entity name='bsd_payment' enableprefiltering='1'>");
    xml.push("<attribute name='bsd_differentamount' alias='Difference' />");
    xml.push("<attribute name='bsd_totalamountpayablephase' alias='pm_totalamountpayablephase' />");
    xml.push("<attribute name='bsd_amountpay' alias='pm_amountpay' />");
    xml.push("<attribute name='statuscode' alias='statuscode' />");
    xml.push("<attribute name='bsd_overpayment' alias='overpayment' />");
    xml.push("<attribute name='bsd_depositamount' alias='pm_Deposit' />");
    xml.push("<attribute name='bsd_paymenttype' alias='pm_type' />");
    xml.push("<filter type='and'>");
    xml.push("<condition attribute='bsd_paymentid' operator='eq' uitype='bsd_payment' value='" + paymentid + "' />");
    xml.push("</filter>");
    xml.push("<link-entity name='bsd_advancepayment' from='bsd_payment' to='bsd_paymentid' link-type='outer' alias='apm' >");
    xml.push("<attribute name='bsd_amount' alias='AdvanceAmount' />");
    xml.push("</link-entity>");
    xml.push("<link-entity name='bsd_paymentschemedetail' from='bsd_paymentschemedetailid' to='bsd_paymentschemedetail' link-type='outer' alias='psdetail' >");
    xml.push("<attribute name='bsd_ordernumber' alias='Ordernumber' />");
    xml.push("<attribute name='bsd_name' alias='psdetail_name' />");
    xml.push("</link-entity>");
    xml.push("<link-entity name='bsd_transactionpayment' from='bsd_payment' to='bsd_paymentid' link-type='outer' alias='transaction' >");
    xml.push("<attribute name='bsd_transactiontype' alias='transactiontype' />");
    xml.push("<attribute name='bsd_feetype' alias='feeType' />");
    xml.push("<attribute name='bsd_amount' alias='amount' />");
    xml.push("<link-entity name='bsd_miscellaneous' from='bsd_miscellaneousid' to='bsd_miscellaneous' link-type='outer' alias='mis' >");
    xml.push("<attribute name='bsd_description' alias='tran_Description' />");
    xml.push("</link-entity>");
    xml.push("</link-entity>");
    xml.push("</entity>");
    xml.push("</fetch>");
    CrmFetchKitNew.Fetch(xml.join(""), true).then(function (rs) {
        if (rs.length > 0) {

            var row = '';
            statuscode = rs[0].attributes['statuscode'] ? rs[0].attributes['statuscode'].value : 0;
            var bsd_paymenttype = rs[0].attributes['pm_type'] ? rs[0].attributes['pm_type'].value : 0;
            var pm_Deposit = rs[0].attributes['pm_Deposit'] ? rs[0].attributes['pm_Deposit'].value : 0;
            var bsd_differentamount = rs[0].attributes['Difference'] ? rs[0].attributes['Difference'].value : 0;
            // //Description_b
            var row_des = '';
            var row = '';
            var Co = false;
            for (var i = 0; i < rs.length; i++) {
                var bsd_amount = rs[i].attributes['amount'] ? rs[i].attributes['amount'].value : 0;
                var description_b = rs[i].attributes['tran_Description'] ? rs[i].attributes['tran_Description'].value : '';
                var bsd_transactiontype = rs[i].attributes['transactiontype'] ? rs[i].attributes['transactiontype'].value : 0;
                if (bsd_transactiontype == 100000003) {
                    Co = true;
                    if (bsd_amount < 0) {
                        bsd_amount = bsd_amount * (-1);
                        row = [' <tr>', '<td height="25px">' + description_b + '</td>', '<td style="text-align:right">(' + common.formatNumberEx1(bsd_amount.toString()) + ')</td>', '</tr>'].join("");
                    }
                    else {
                        row = [' <tr>', '<td height="25px">' + description_b + '</td>', '<td style="text-align:right">' + common.formatNumberEx1(bsd_amount.toString()) + '</td>', '</tr>'].join("");
                    }
                    row_des += row;
                }
                else {
                    if (Co == false && i == rs.length - 1) {
                        template = template.replace("{description_b}", '');
                    }
                }

            }
            template = template.replace("{description_b}", row_des);

            //advancepayment
            var advanceamount = rs[0].attributes['AdvanceAmount'] ? rs[0].attributes['AdvanceAmount'].value : 0;
            if (!advanceamount || advanceamount == 0) {
                template = template.replace("{advancepayment}", '');
                ////Tiền đặt cọc
                //row = ['<tr>',
                //       '<td height="30px" style="padding-bottom:55px">Deposit Amount / Số tiền đặt cọc</td>',
                //       '<td style="text-align:right;width:350px;padding-bottom:55px" colspan="2">' + common.formatNumberEx1(bsd_depositamount.toString()) + '</td>',
                //       '</tr>'].join("");
                //if (bsd_paymenttype != 100000001) {
                //    template = template.replace("{deposit_amount}", '');
                //}
                //template = template.replace("{deposit_amount}", row);
            }
            else {
                var row_advan = [' <tr>', '<td height="25px" style="padding-bottom:55px">Advance Payment / Thanh toán trước hạn </td>', '<td style="text-align:right;padding-bottom:55px">' + common.formatNumberEx1(advanceamount.toString()) + '</td>', '</tr>'].join("");
                template = template.replace("{advancepayment}", row_advan);
                ////Tiền đặt cọc
                //row = ['<tr>',
                //'<td height="30px">Deposit Amount / Số tiền đặt cọc</td>',
                //'<td style="text-align:right;width:350px;margin-bottom:20px" colspan="2">' + common.formatNumberEx1(bsd_depositamount.toString()) + '</td>',
                //'</tr>'].join("");
                //if (bsd_paymenttype != 100000001) {
                //    template = template.replace("{deposit_amount}", '');
                //}
                //template = template.replace("{deposit_amount}", row);
            }
            //Sub-Total
            var bsd_amountpay = rs[0].attributes['pm_amountpay'] ? rs[0].attributes['pm_amountpay'].value : 0;
            var bsd_overpayment = rs[0].attributes['overpayment'] ? rs[0].attributes['overpayment'].value : 0;
            var subTotal = bsd_amountpay + bsd_overpayment;
            var row_sub = [' <tr>', '<td style="padding-left:104px;height:30px;margin-bottom:20px">Sub-Total / Số tiền </td>', '<td style="text-align:right;padding-bottom:13px;padding-top:13px;width:350px;border-top: 1px solid;">' + common.formatNumberEx1(subTotal.toString()) + '</td>', '</tr>'].join("");
            template = template.replace("{sub_total}", row_sub);

            //Tiền đặt cọc
            var bsd_depositamount = 0;

            if (bsd_differentamount != null && bsd_differentamount > 0) {
                bsd_depositamount = rs[0].attributes['pm_totalamountpayablephase'] ? rs[0].attributes['pm_totalamountpayablephase'].value : 0;
            }
            else {
                bsd_depositamount = rs[0].attributes['pm_amountpay'] ? rs[0].attributes['pm_amountpay'].value : 0;
            }
            row = ['<tr>', '<td height="30px">Deposit Amount / Số tiền đặt cọc</td>', '<td style="text-align:right;width:350px" colspan="2">' + common.formatNumberEx1(bsd_depositamount.toString()) + '</td>', '</tr>'].join("");
            if (bsd_paymenttype == 100000001) {
                template = template.replace("{deposit_amount}", row);
            }
            else {
                template = template.replace("{deposit_amount}", '');
            }
            //Total Amount / Tổng cộng
            var bsd_amountpay = rs[0].attributes['pm_amountpay'] ? rs[0].attributes['pm_amountpay'].value : 0;
            var bsd_overpayment = rs[0].attributes['overpayment'] ? rs[0].attributes['overpayment'].value : 0;
            var netAmount = 0;
            var row_net = '';
            var ordernumber = rs[0].attributes['Ordernumber'] ? rs[0].attributes['Ordernumber'].value : 0;
            if (bsd_paymenttype == 100000001 || ordernumber == 1) {
                netAmount = bsd_amountpay;
                row_net = [' <tr>', '<td style="padding-left:104px;height:30px;margin-bottom:20px">Total Amount / Tổng cộng </td>', '<td style="text-align:right;font-weight:bold;width:100px;border-top:1px solid;">' + common.formatNumberEx1(netAmount.toString()) + '</td>', '</tr>'].join("");
            }
            else {
                netAmount = bsd_amountpay + pm_Deposit;
                row_net = [' <tr>', '<td style="padding-left:104px;height:30px;margin-bottom:20px">Total Amount / Tổng cộng </td>', '<td style="text-align:right;font-weight:bold;width:100px;border-top:1px solid;">' + common.formatNumberEx1(netAmount.toString()) + '</td>', '</tr>'].join("");
            }
            template = template.replace("{bsd_amountpay}", row_net);

        }
        if (statuscode == '1' || statuscode == '100000002') {

            //$('#showreport').append('');
            callback('');

        }
        else {
            //$('#showreport').append(template);
            callback(template);
        }

    });
}

var paymentId = getParameterByName("data");
var arrid = paymentId.split(',');
buldReportById(0);