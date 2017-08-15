var CHANGE_TAB_CONFIRM = 'You have unsaved work. Are you sure you want to leave this tab?';
function onTxtBoxInEdit($this) {
    var tb = $($this);
    window.isFormInEditMode = hasValue(tb.val());
}

function setEditMode() {
    window.isFormInEditMode = true;
}
function exitEditMode() {
    window.isFormInEditMode = false;
}
function validateEditMode() {
    if (!isFormInEditMode) return true;

    if (!confirm(CHANGE_CONFIRM)) return false;

    window.isFormInEditMode = false;

    return true;
}

function setStoreSelectionState(catId) {
    var li = $('#li-' + catId);

    li.parent().siblings('div').find('input[type=checkbox]').prop("checked", li.parent().find("input[type='checkbox']").is(":checked"));

}

function togglStore(id, $this, factor) {
    var v = $('#ul-c-' + id);

    var t = factor == 1 ? $($this).parent().parent() : $($this).parent();
    var toBeOpen = v.is(":hidden");

    v.slideToggle("slow", function () {
        if (toBeOpen) {
            t.addClass('no-bord');
        } else {
            t.removeClass('no-bord');
        }
        //refresh scroller
        $('#d-store-tree-container').nanoScroller();
    });

    if (toBeOpen) {
        t.find('.l-icon').removeClass('l-plus').addClass('l-minus');
    } else {
        t.find('.l-icon').addClass('l-plus').removeClass('l-minus');
    }

}

function expandCollapse(t) {

    $.each($('.ul-stores-tree').find('.l-icon'), function () {
        var btn = $(this);
        var p = btn.parent().parent();
        var exp = p.siblings('.expandable');
        var isHidden = exp.is(":hidden");

        if (t == 1) {
            if (isHidden) btn.click();
        } else {
            if (!isHidden) btn.click();
        }
    });
}
//#region contents tab
//function setListSortable(selector, postUrl, callback, idAttr) {
//    $(selector).sortable({
//        update: function () {
//            var sortedIDs = $(selector).sortable('toArray', { attribute: idAttr });
//            var data = { idS: sortedIDs };
//            var url = postUrl;
//            ajaxAction(url, data, callback);

//        }
//    });
//}

//function setListKendoSortable(selector, postUrl, callback, idAttr) {
//    $(selector).kendoSortable({
//        change: function (e) {
//            var sortedIDs = [];
//            $.each(e.sender.element.find('li'), function () {
//                sortedIDs.push($(this).attr(idAttr));
//            });

//            var data = { idS: sortedIDs };
//            ajaxAction(postUrl, data, callback);
//        }
//    });
//}

//#endregion

//#region course details tab
function onCourseSaved(response) {

    hideFormLoader();

    var kind = response.success ? NotificationKinds.Success : NotificationKinds.Error;
    var msg = response.success ? userMessages.COURSE.COURSE_SAVED : response.error;

    window.formUserNotifManager.show({ message: msg, kind: kind });

    if (response.success) {
        exitEditMode();
        $(window.EDIT_FORM_SELECTOR).find('#CourseId').val(response.result.id);
        $(window.EDIT_FORM_SELECTOR).find('.btn-prev-course').attr({ 'href': response.result.url });
        window.getNotifManagerInstance().notify(notifEvents.course.courseCreated, response.result);
        window.getNotifManagerInstance().notify(notifEvents.course.courseStateChanged, null);
    }
}

function onBundleSaved(response) {
    hideFormLoader();

    var kind = response.success ? NotificationKinds.Success : NotificationKinds.Error;
    var msg = response.success ? userMessages.COURSE.BUNDLE_SAVED : response.error;

    window.formUserNotifManager.show({ message: msg, kind: kind });

    if (response.success) {
        exitEditMode();
        $(window.EDIT_FORM_SELECTOR).find('#BundleId').val(response.result.id);
        window.getNotifManagerInstance().notify(notifEvents.course.bundleCreated, response.result);

    }
}


//#endregion

//#region tabs view
function setTabsState(mode) {
    if (mode == FormModes.edit) {
        $('#ul-course-tabs > li').removeClass('disabled');
    } else if (mode == FormModes.insert) {
        $.each($('#ul-course-tabs > li'), function (i) {
            if (i > 0) {
                $(this).addClass('disabled');
            }
        });
    }
}

function handleCourseSaveEvent(token) {
    var last = Number($('#CurrentCourseId').val());
    $('#CurrentCourseId').val(token.id);
    $('#li-page-name').html('Edit ' + token.name);
    setTabsState(FormModes.edit);

    if (last >= 0) {
        //refresh form and links
        $('#frmDetails').submit();
        return;
    }
    //update forms url on first creation
    $.each($('#ul-course-tabs > li > form'), function () {
        var form = $(this);
        var url = form.attr('action');
        var newUrl = url.substring(0, url.lastIndexOf('/') + 1) + token.id;
        form.attr('action', newUrl);
    });
   
    $('#frmDetails').submit();
}

function handleBundleSaveEvent(token) {
    var last = Number($('#CurrentBundleId').val());
    $('#CurrentBundleId').val(token.id);
    $('#li-page-name').html('Edit ' + token.name);
    setTabsState(FormModes.edit);

    if (last >= 0) {
        //refresh form and links
        $('#frmDetails').submit();
        return;
    }
    //update forms url on first creation
    $.each($('#ul-course-tabs > li > form'), function () {
        var form = $(this);
        var url = form.attr('action');
        var newUrl = url.substring(0, url.lastIndexOf('/') + 1) + token.id;
        form.attr('action', newUrl);
    });

    $('#frmDetails').submit();
}
//#endregion
