// JavaScript source code
if (window.$ ==  null)
    window.$ = window.parent.$;
jQuery = window.parent.jQuery;
var script = window.parent.document.getElementById('select2.js');
if (script == null) {
    script = window.parent.document.createElement('script');
    script.type = 'text/javascript';
    script.id = 'select2.js';
    script.src = window.top.Xrm.Page.context.getClientUrl() + '/webresources/bsd_select2.min';
    window.parent.document.head.appendChild(script);
}
function InitRowEntity(callback) {
    debugger;
    var style = window.parent.document.getElementById('select2css');
    if (style == null) {
        style = window.parent.document.createElement('link');
        style.rel = 'stylesheet';
        style.type = 'text/css';
        style.id = 'select2css';
        style.href = window.top.Xrm.Page.context.getClientUrl() + '/webresources/bsd_select2';
        window.parent.document.head.appendChild(style);
    }
    var entityRow = window.parent.document.getElementById('entityrow');
    if (entityRow == null) {
        entityRow = window.parent.document.createElement('tr');
        entityRow.id = 'entityrow';
        var name = window.parent.document.getElementById('bsd_name_c');
        var targetRow;
        if (name != null)
            targetRow = window.parent.document.getElementById('bsd_name_c').parentElement;
        $(entityRow).insertAfter($(targetRow));
        var entityRowContent = [];
        entityRowContent.push('<td class='ms-crm-ReadField-Normal ms-crm-FieldLabel-LeftAlign' id='bsd_entity_c' title='Select entity.'>');
        entityRowContent.push('<span class='ms-crm-InlineEditLabel'>');
        entityRowContent.push('<span class='ms-crm-InlineEditLabelText' style='max-width:105px;text-align:Left;'>Entity</span>');
        entityRowContent.push('<img alt='Required' src='/_imgs/imagestrips/transparent_spacer.gif?ver=-1043241157' class='ms-crm-ImageStrip-frm_required ms-crm-Inline-RequiredLevel'>');
        entityRowContent.push('</span>');
        entityRowContent.push('</td>');
        entityRowContent.push('<td class='ms-crm-Field-Data-Print' id='bsd_entity_d' style='padding-left:23px;'><div style='display:block;'><select id='selectEntity' style='width:100%'><option value=''>Select Entity</option></select></div></td>');

        entityRowContent.push('<td class='ms-crm-ReadField-Normal ms-crm-FieldLabel-LeftAlign' id='bsd_fieldname_c' title='Field'>');
        entityRowContent.push('<span class='ms-crm-InlineEditLabel'>');
        entityRowContent.push('<span class='ms-crm-InlineEditLabelText' style='max-width:105px;text-align:Left;'>Field</span>');
        entityRowContent.push('<img alt='Required' src='/_imgs/imagestrips/transparent_spacer.gif?ver=-1043241157' class='ms-crm-ImageStrip-frm_required ms-crm-Inline-RequiredLevel'>');
        entityRowContent.push('</span>');
        entityRowContent.push('</td>');

        entityRowContent.push('<td class='ms-crm-Field-Data-Print' id='bsd_fieldname_d' style='padding-left:23px;'>');
        entityRowContent.push('<div style='display:block;'><select id='selectField' style='width:99%'><option value=''>Select field</option></select></div>');
        entityRowContent.push('</td>');

        entityRowContent.push('<td class='ms-crm-Field-Data-Print' colspan='2'><div class='ms-crm-Field-Data-Print' data-height='24' colspan='2'></div></td>');
        entityRow.innerHTML = entityRowContent.join('');
        var tds = $(entityRow).children('td');
        if (callback != null)
            callback(tds[1].firstElementChild.firstElementChild, tds[3].firstElementChild.firstElementChild);
    }
    else {
        var tds = $(entityRow).children('td');
        if (callback != null)
            callback(tds.eq(1).children('div:first').children('select:first')[0], tds.eq(3).children('div:first').children('select:first')[0]);
    }
}

function RetrieveEntities(select, val, callback) {
    //select = window.parent.document.createElement('select');
    select.innerHTML = '';
    SDK.Metadata.RetrieveAllEntities(1, false, function (ents) {
        var opts = '<option value='' selected='selected'>Select entity</option>';
        var opt = window.parent.document.createElement('option');
        opt.innerHTML = 'Select entity';
        opt.value = '';
        opt.selected = true;
        select.appendChild(opt);
        var len = ents.length;
        for (var i = 0; i < len; i++) {
            if (ents[i].IsCustomizable != null && ents[i].IsCustomizable.Value == true && ents[i].OwnershipType != null && ents[i].OwnershipType != 'None' && ents[i].LogicalName != 'bsd_autonumber') {
                obj = ents[i];
                var o = window.parent.document.createElement('option');
                o.innerHTML = obj.LogicalName;
                o.value = obj.LogicalName + '&' + obj.MetadataId;
                if (val != null && val.length > 0) {
                    if (obj.LogicalName == val) {
                        o.selected = true;
                        opt.selected = false;
                    }
                }
                select.appendChild(o);
            }

        }
        if (callback != null)
            callback(select);
    },
    function (error) {
        console.log('[RetrieveEntities]' + error.message);
    });
}

function RetrieveFields(select, val, lname, eid, callback) {
    //select = window.parent.document.getElementById();
    debugger;
    select.innerHTML = '';
    var dopt = window.parent.document.createElement('option');
    dopt.innerHTML = 'Select field';
    dopt.value = '';
    dopt.selected = true;
    select.appendChild(dopt);
    SDK.Metadata.RetrieveEntity(2, lname, eid, false
            , function (rs) {
                debugger;
                var flen = rs.Attributes.length;
                for (i = 0; i < flen; i++) {
                    var tmp = rs.Attributes[i];
                    if (tmp.IsCustomizable != null
                        && tmp.IsCustomizable.Value == true
                        && tmp.DisplayName.LocalizedLabels[0] != null
                        && ('[customer][virtual][status][owner][uniqueidentifier]').indexOf('[' + tmp.AttributeType.toLocaleLowerCase() + ']') < 0
                        && tmp.AttributeType == 'String') {
                        var obj = {
                            'label': tmp.DisplayName.LocalizedLabels[0].Label
                            , 'name': tmp.LogicalName
                            , 'sname': tmp.SchemaName
                            , 'type': tmp.AttributeType
                        };
                        var opt = window.parent.document.createElement('option');
                        opt.innerHTML = obj.label;
                        opt.value = obj.name + '&' + obj.type;
                        if (val != null && val.length > 0) {
                            if (obj.name == val) {
                                dopt.selected = false;
                                opt.selected = true;
                            }
                        }
                        select.appendChild(opt);
                    }
                }
                if (callback != null)
                    callback(select);
            }
            , function (err) {
                console.log('[RetrieveFields] ' + err.message);
                if (callback != null)
                    callback(select);
            });
}

function InitAll() {
    if (Xrm.Page.getAttribute('bsd_currentposition').getValue() == null)
        Xrm.Page.getAttribute('bsd_currentposition').setValue('0');
    InitRowEntity(function (e, e1) {
        RetrieveEntities(e, Xrm.Page.getAttribute('bsd_name').getValue(), function (e_e) {
            debugger;
            $(e_e).select2();
            if (e_e.value.length > 0) {
                var values = e_e.value.split('&');
                Xrm.Page.getAttribute('bsd_name').setValue(values[0]);
                RetrieveFields(e1, Xrm.Page.getAttribute('bsd_field').getValue(), values[0], values[1], function (e1_e) {
                    $(e1_e).select2().on('change', function (e1_e_e) {
                        var val = e1_e_e.val;
                        if (val.length > 0) {
                            var vals = val.split('&');
                            if (vals[1] == 'String')
                                Xrm.Page.getAttribute('bsd_field').setValue(vals[0]);
                            else {
                                alert('Field must be string type');
                                Xrm.Page.getAttribute('bsd_field').setValue(null);
                            }
                        }
                        else
                            Xrm.Page.getAttribute('bsd_field').setValue(null);
                    });
                });
            }
            else {
                Xrm.Page.getAttribute('bsd_field').setValue(null);
                e1.innerHTML = '<option value='' selected='selected'>Select field</option>';
                $(e1).select2();
            }
            $(e).on('change', function (e) {
                debugger;
                if (e.val.length > 0) {
                    var values = e_e.value.split('&');
                    Xrm.Page.getAttribute('bsd_name').setValue(values[0]);
                    Xrm.Page.getAttribute('bsd_field').setValue(null);
                    RetrieveFields(e1, Xrm.Page.getAttribute('bsd_field').getValue(), values[0], values[1], function (e1_e) {
                        $(e1_e).select2().on('change', function (e1_e_e) {
                            debugger;
                            var val = e1_e_e.val;
                            if (val.length > 0) {
                                debugger;
                                var vals = val.split('&');
                                if (vals[1] == 'String')
                                    Xrm.Page.getAttribute('bsd_field').setValue(vals[0]);
                                else {
                                    alert('Field must be string type');
                                    Xrm.Page.getAttribute('bsd_field').setValue(null);
                                }
                            }
                            else
                                Xrm.Page.getAttribute('bsd_field').setValue(null);
                        });

                    });
                }
                else {
                    Xrm.Page.getAttribute('bsd_field').setValue(null);
                    e1.innerHTML = '<option value='' selected='selected'>Select field</option>';
                    $(e1).select2();
                }
            });
        });
    });
}
function UseCustomChange() {
    var flag = Xrm.Page.getAttribute('bsd_usecustom').getValue();
    if (flag == null)
        flag = false;
    Xrm.Page.getControl('bsd_prefix').setVisible(flag);
    Xrm.Page.getControl('bsd_sufix').setVisible(flag);

}