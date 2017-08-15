function clearEditForm() {
    $('#d-content-form-container').empty();
    $('#b-title-text').html(null);
    $('.btn-submit').unbind('click');
    $('.btn-delete').unbind('click').bind('click', function () {
        window.formUserNotifManager.show({ message: userMessages.SAVE_YOUR_WORK, kind: NotificationKinds.Info });
    });
}

function onListReordered(response) {
    var notifManager = window.formUserNotifManager;

    if (notifManager == undefined) return;

    var kind = response.success ? NotificationKinds.Success : NotificationKinds.Error;
    var msg = response.success ? response.message : response.error;

    window.formUserNotifManager.show({ message: msg, kind: kind });
}

function setListSortable(selector, postUrl, callback, idAttr) {
    $(selector).sortable({
        update: function () {
            var sortedIDs = $(selector).sortable('toArray', { attribute: idAttr });
            var data = { idS: sortedIDs };
            var url = postUrl;
            ajaxAction(url, data, callback);
        }
    });
}

function addNewRowOnInsert(text, ulSelector) {
    var emptyRow = kendo.template($("#kendo-ch-list-template").html());
    var data = {
        id: -1,
        name: text
    };

    var emptyLi = $(emptyRow(data)).addClass('k-state-selected');

    emptyLi.hide().prependTo(ulSelector).slideDown("slow");
}

function removeNewRow(ulSelector) {
    $(ulSelector).data("kendoListView").wrapper.find('li[data-val=-1]').remove();
}

function isNewRowIncludes(ulSelector) {
    return $(ulSelector).data("kendoListView").wrapper.find('li[data-val=-1]').length > 0;
}

function resetContentListsState() {
    removeNewRow(window.VIDEO_LIST_ID);
    removeNewRow(window.LINK_LIST_ID);
}

function setContentListsHeight() {
    var linkCnt = $(window.LINK_LIST_ID).data("kendoListView").wrapper.find('li').length;

    if (linkCnt == 0) {
        $('#video-list-container').addClass('full');
        setReportScroll("#video-list-container");
        setReportScroll("#link-list-container");

        $(window.LINK_LIST_ID).data("kendoListView").wrapper.removeClass('with-content');
    }
}

function rebindContents() {
    $(window.VIDEO_LIST_ID).data("kendoListView").dataSource.read();
    $(window.LINK_LIST_ID).data("kendoListView").dataSource.read();
}

function setListsSeparetorState(show) {
    var d = $('.content-lists-sep');
    if (show) {
        d.show();
    } else {
        d.hide();
    }
}

//#region course visuals helpers
function onThumbUploadAction(e) {
    // Array with information about the uploaded files
    var files = e.files;
    
    if (files.length > 1) {
        alert(userMessages.UPLOAD.ONLY_ONE_FILE);
        e.preventDefault();
        return;
    }

    // Check the extension of each file and abort the upload if it is not .jpg
    $.each(files, function () {
        if ($.inArray(this.extension.toLowerCase(), IMAGE_EXTENSIONS) < 0) {
            alert(userMessages.UPLOAD.ONLY_IMAGE);
            e.preventDefault();
            return;
        } else {
            $('#d-crs-thumb').empty();
            showContainerLoader('#d-crs-thumb', null);
            //	adjustCmdPosition(110);
        }
    });
}

function onThumbUploadSuccess(e) {
    var operation = e.operation;
    var response = e.response;

    hideLoader();

    if (!response) return;

    if (!response.success) {
        alert(response.error);

        return;
    }
    setEditMode();
    clearUploadConsole();
    //	adjustCmdPosition();

    var preview = $('#d-crs-thumb');
    preview.empty();

    switch (operation) {
        case "remove":
            break;
        case "upload":
            try {
                preview.append($('<img />').attr({ src: response.result.url, 'alt': response.result.name, 'class': 'img-preview' }));
                $('#a-fake-thumb-upload').html('Change');
                setValidPass('#d-crs-thumb', '#valid-thumb');
                $('#ThumbName').val(response.result.name);
                $(window.EDIT_FORM_SELECTOR).validate().element($('#ThumbName'));
                
                window.getNotifManagerInstance().notify(notifEvents.file.fileUploaded, null);
            } catch (e) {
                if (window.console) console.log(e);
            }
            break;
    }

}
//#endregion