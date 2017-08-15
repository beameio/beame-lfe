var CONTENTS_LIST_SELECTOR = '#CourseContents';

function handleEditMode() {
    if (typeof (setEditMode) != "undefined") {
        setEditMode();
    }
}
function handleEditModeExit() {
    if (typeof (exitEditMode) != "undefined") {
        exitEditMode();
    }
}
function onChapterEnterEdit(e) {
    handleEditMode();
    onChapterEdit(e);
}

function onChapterEditExit(e) {
    handleEditModeExit();
    onChapterListStateChanged(e);
}

//function setEditMode() {
//    window.isFormInEditMode = true;
//}
//function exitEditMode() {
//    window.isFormInEditMode = false;
//}
//function validateEditMode() {
//    if (!isFormInEditMode) return true;

//    if (!confirm(CHANGE_CONFIRM)) return false;

//    window.isFormInEditMode = false;

//    return true;
//}

function setVideoPHClickEvent(selector) {
    $(selector).attr({ title: 'click to open video manager' });
    $(selector).click(openVideoWnd);
}

//function onChapterHint(element) {
//    var w = $(CONTENTS_LIST_SELECTOR).width();
//    return element.clone().addClass("hint").css({ width: (w - 16) + 'px', height: '35px' });
//}

//function onContentHint(element) {
//    var w = $(CONTENTS_LIST_SELECTOR).width();
//    return element.clone().addClass("c-hint").css({ width: (w - 40 - 16) + 'px', height: '35px' });
//}

//function placeholder(element) {
//    return element.clone().addClass("placeholder").text("drop here");
//}


//function setContentListKendoSortable(selector, postUrl, callback, idAttr) {
//    $(selector).kendoSortable({
//        change: function (e) {
//            var sortedIDs = [];
//            $.each(e.sender.element.find('li'), function () {
//                sortedIDs.push($(this).attr(idAttr));
//            });

//            var data = { idS: sortedIDs };
//            ajaxAction(postUrl, data, callback);
//        }
//        ,handler: ".drag"
//        ,hint: onContentHint   
//        ,placeholder: placeholder
//    });
//}

var EXPAND_SAVED_CHAPTER = false;
var EXPAND_CHAPTER_ID = null;

function initNewChapter(courseId) {
    expandCollapseChapters(2);

    handleEditMode();

    var lv = $(CONTENTS_LIST_SELECTOR).data('kendoListView');
    var chapter = {
          CourseId: courseId
        , ChapterId: -1
        , Name: 'new chapter'
        , OrderIndex: lv.dataSource._data.length
    };

    var token = {
        ContentId: -1
        , Uid: emptyGuid
        , Kind: 1
        , Chapter: chapter
        , Name:'New Chapter'
    };

    lv.dataSource.add(token);

    setTimeout(function () {
        lv.edit(lv.element.children().last());
        window.IS_ANY_VIDEO_IN_EDIT_MODE = true;
        setReportScroll(CONTENT_CONTAINER_SELECTOR);
    }, 300);
}

function _totalChapters() {
    return $(CONTENTS_LIST_SELECTOR).find('li[data-kind=1]').length;
}

function initNewQuiz(courseId, cnt) {

    if (cnt === 0) {
        swal({
           // title: "<a href='http://ynet.co.il' target='_blank'>Link</a>",
            // html:true,
            title: "You haven't more available quizzes.",
            text:"Please create first.",
            type: "warning"
        });
        return;
    }

    expandCollapseChapters(2);
    handleEditMode();
    
    var lv = $(CONTENTS_LIST_SELECTOR).data('kendoListView');
    var token = {
          CourseId: courseId
        , ContentId: -1
        , Uid : emptyGuid        
        , Kind: 2
        , Name:'Add new Quiz'
        , Quiz : {
            CourseQuizId: emptyGuid
            , CourseQuizSid: -1
            , CourseId: courseId            
          }
    };

    lv.dataSource.add(token);

    setTimeout(function () {
        lv.edit(lv.element.children().last());
        window.IS_ANY_VIDEO_IN_EDIT_MODE = true;
        setReportScroll(CONTENT_CONTAINER_SELECTOR);
    }, 100);
}

function onCourseQuizSaving() {
   // console.log(e);
    showLoader();
    return true;
}

function onCourseQuizSaved(response) {
    hideLoader();
    if (response.success) {

        response.message = 'Quiz saved';
        reloadContents();
       

        window.getNotifManagerInstance().notify(notifEvents.course.courseStateChanged, null);
    } else {
        window.formUserNotifManager.show({ message: response.error, kind: NotificationKinds.Error });
    }
    
}
function reloadContents() {
    $(CONTENTS_LIST_SELECTOR).data('kendoListView').dataSource.read();
}
function onChapterListBound(e) {
    $.each(e.sender.wrapper.find('li .btn-collapse-first'), function () {
        var btn = $(this), $this = this;

        btn.unbind('click').click(function () {
            toggleChapter($this);
        });
    });

    $.each(e.sender.wrapper.find('li .ch-name'), function () {
        var btn = $(this), $this = $(this).siblings('.btn-collapse-first');

        btn.unbind('click').click(function () {
            toggleChapter($this);
        });
    });

    setReportScroll(CONTENT_CONTAINER_SELECTOR);

    //setContentsSortable(CONTENTS_LIST_SELECTOR, window.saveContentOrderUrl, onContentOrderChanged, 'data-val');
    setContentsSortable();

    if (!EXPAND_SAVED_CHAPTER) return;

    setTimeout(function() {
        EXPAND_SAVED_CHAPTER = false;

        e.sender.wrapper.find('li[data-id=' + EXPAND_CHAPTER_ID + ']').find('.btn-collapse-first').click();

        EXPAND_CHAPTER_ID = null;
    }, 300);

}


function onContentOrderChanged(response) {
    if (response.success) {
        window.formUserNotifManager.show({ message: "Order Saved", kind: NotificationKinds.Success });

        reloadContents();

    } else {
        window.formUserNotifManager.show({ message: response.error, kind: NotificationKinds.Error });
    }
}
function getSortedContents() {
    var sortedIDs = [];
    $.each($(CONTENTS_LIST_SELECTOR + '> li'), function () {
        var kind = $(this).attr('data-kind');
        var id = $(this).attr('data-id');
        sortedIDs.push({ kind: kind, id: id });
    });

    var data = { tokens: sortedIDs };

    return data;
}

function setContentsSortable() {
    $(CONTENTS_LIST_SELECTOR).sortable({
        update: function () {
            var data = getSortedContents();//$(selector).sortable('toArray', { attribute: 'data-id' });
            var url = window.saveContentOrderUrl;
            ajaxAction(url, data, onContentOrderChanged);
        }
    });
}
function cancelEditContent() {
    $(CONTENTS_LIST_SELECTOR).data('kendoListView').cancel();
    handleEditModeExit();
}
function expandCollapseChapters(t) {
    cancelEditContent();

    if (t == 1) showLoader();

    $.each($(CONTENTS_LIST_SELECTOR).find('.btn-collapse-first'), function () {
        var btn = $(this);
        var p = btn.parent();
        var exp = p.siblings('.first-contents');
        var isHidden = exp.is(":hidden");

        if (t == 1) {
            if (isHidden) btn.click();
        } else {
            if (!isHidden) btn.click();
        }
    });

    $(CONTENTS_LIST_SELECTOR).find('.btn-collapse-second').click();

    if (t == 1) hideLoader();

    setReportScroll(CONTENT_CONTAINER_SELECTOR);
}

function toggleChapter($this) {
    var v = $($this).parent().siblings('.first-contents');
    var loaded = v.attr('data-load') == "true";

    var toBeOpen = v.is(":hidden");

    v.slideToggle("slow",function() {
        setReportScroll(CONTENT_CONTAINER_SELECTOR);
    });

    if (toBeOpen) {
        $($this).addClass('expanded');

        if (!loaded) {
            var contentId = $($this).parent().parent().attr('data-id');
            var uid = $($this).parent().parent().attr('data-val');
            var kind = parseInt($($this).parent().parent().attr('data-kind'));
            var editBtn = $($this).parent().find('.k-edit-button');

            switch(kind) {
                case 1:
                    actionFormResultWithContainer(window.chapterContentsUrl, { id: contentId }, '#ch-cn-' + uid, CONTENT_CONTAINER_SELECTOR);
                    break;
                case 2: //quizViewFormUrl
                    actionFormResultWithContainer(window.quizViewFormUrl, { id: contentId }, '#ch-cn-' + uid, CONTENT_CONTAINER_SELECTOR);
                    //editBtn.click();
                    break;
                default:
                    return;
            }
            
            v.attr('data-load', "true");
        }
    } else {
        $($this).removeClass('expanded');
    }

    //setTimeout(function () {
    //    setReportScroll(CONTENT_CONTAINER_SELECTOR);
    //}, 300);

}

function onChapterListStateChanged(e) {
    //required to restore collapse functionality after cancel edit
     setTimeout(function() { onChapterListBound(e); }, 300);
}

function onChapterEdit(e) {
    var kind = e.model.Kind;
    var btn = e.item.find('.k-update-button');
    switch (kind)
    {
        case 1:
            e.item.find('input').focus();
            setTimeout(function () {
                e.item.find('input').select();
            });
           
            btn.unbind('click').bind('click', function () {
                $('#frmEditChapter').submit();
            });
            break;
        case 2:
            if (e.model.ContentId < 0) {
                btn.unbind('click').bind('click', function () {
                    $('#frmEditQuiz').submit();
                });
            } else {
                e.preventDefault();
                var wnd = $(window.WND_QUIZ_MANAGE_SELECTOR).data("kendoWindow");
                var url = window.quizEditWndUrl + '?id=' + e.model.Quiz.QuizId + '&cid=' + e.model.Quiz.CourseId;

                var t = "Edit " + e.model.Name;
                wnd.title(t);
                wnd.bind("close", reloadContents);
                wnd.refresh(url);
                wnd.center();
                wnd.open();
            }            
            break;
    }
}

function onChapterSaved(response) {

    if (response.success) {
        window.getNotifManagerInstance().notify(notifEvents.course.courseStateChanged, null);
        response.message = 'Chapter saved';

        if (response.result.isNew) {
            EXPAND_SAVED_CHAPTER = true;
            EXPAND_CHAPTER_ID = response.result.id;
        }

        reloadContents();

        window.getNotifManagerInstance().notify(notifEvents.course.courseStateChanged, null);
    } else {
        window.formUserNotifManager.show({ message: response.error, kind: NotificationKinds.Error });
    }
    window.getNotifManagerInstance().notify(notifEvents.chapter.chapterSaved, response);
    
}

function onChapterRemoved(e) {
    if (!window.confirm("Delete " + (e.model.Kind == 1 ? "chapter" : "quiz")  + "?")) {
        e.preventDefault();
    } else {
        window.getNotifManagerInstance().notify(notifEvents.course.courseStateChanged, null);
    }
}

//function onChapterSortChanged(e) {
//    ajaxAction(window.saveContentOrderUrl, getSortedIds(e), onContentOrderChanged);
//}



function onVideoSortChanged(e) {

    ajaxAction(window.saveVideoOrderUrl, getSortedIds(e), onListReordered);
}

function onLinksSortChanged(e) {
    ajaxAction(window.saveLinksOrderUrl, getSortedIds(e), onListReordered);
}


function getSortedIds(e) {
    var sortedIDs = [];
    $.each(e.sender.element.find('li'), function () {
        sortedIDs.push($(this).attr('data-val'));
    });

    var data = { idS: sortedIDs };

    return data;
}

//contents lists
function onChapterContentsListBound(e) {

    $.each(e.sender.wrapper.find('li .btn-collapse-second'), function () {
        var btn = $(this), $this = this;

        btn.unbind('click').click(function () {
            toggleChapterContent($this);
        });
    });

    $.each(e.sender.wrapper.find('li .cn-name'), function () {
        var btn = $(this), $this = $(this).siblings('.btn-collapse-second');

        btn.unbind('click').click(function () {
            toggleChapterContent($this);
        });
    });

    var isVideo = e.sender.wrapper.hasClass('ul-videos');

    setListSortable('#' + e.sender.wrapper.attr('id'), isVideo ? window.saveVideoOrderUrl : window.saveLinksOrderUrl, onListReordered, 'data-val');

    //setReportScroll(CONTENT_CONTAINER_SELECTOR);

}

function onChapterContentListStateChanged(e) {
    handleEditModeExit();
    $(e.container).parent().parent().parent().removeClass('k-edit-item');
    window.IS_ANY_VIDEO_IN_EDIT_MODE = false;
    //required to restore collapse functionality after cancel edit
    setTimeout(function () { onChapterContentsListBound(e); }, 300);
}

function toggleChapterContent($this) {
    var v = $($this).parent().siblings('.cn-details');

    var toBeOpen = v.is(":hidden");

    v.slideToggle("slow",function() {
        setReportScroll(CONTENT_CONTAINER_SELECTOR);
    });

    if (toBeOpen) {
        $($this).addClass('expanded');
    } else {
        $($this).removeClass('expanded');
    }
}

function handleEditClick($this, chId) {
    var lv = $('#ChapterVideos_' + chId).data('kendoListView');
    if (window.IS_ANY_VIDEO_IN_EDIT_MODE) {
        //  alert('Other video is editing now. Please save Your work or cancel edit');
        $(CONTENTS_LIST_SELECTOR).find('.ul-second-level').find('.k-cancel-button').click();
    }
    lv.edit($($this).parent().parent().parent());  
}

function handleLinkEditClick($this, chId) {
    var lv = $('#ChapterLinks_' + chId).data('kendoListView');
    if (window.IS_ANY_VIDEO_IN_EDIT_MODE) {
        //  alert('Other video is editing now. Please save Your work or cancel edit');
        $(CONTENTS_LIST_SELECTOR).find('.ul-second-level').find('.k-cancel-button').click();
    }
    lv.edit($($this).parent().parent().parent());
}

function addChapterVideo(chId) {
    handleEditMode();
    var lv = $('#ChapterVideos_' + chId).data('kendoListView');
    if (window.IS_ANY_VIDEO_IN_EDIT_MODE) {
        //  alert('Other video is editing now. Please save Your work or cancel edit');
        $(CONTENTS_LIST_SELECTOR).find('.ul-second-level').find('.k-cancel-button').click();
    }

    lv.dataSource.add({
        ChapterId: chId,
        VideoId: -1,
        Title: "New video",
        IsOpen: false,
        OrderIndex: lv.dataSource._data.length,
        VideoIdentifier: -1,
        SummaryHTML: '',
        VideoToken: {
            identifier: -1
            ,title:''
        }
    });

    setTimeout(function () {
        lv.edit(lv.element.children().last());
        window.IS_ANY_VIDEO_IN_EDIT_MODE = true;
    }, 300);
}


function onVideoEdit(e) {
    handleEditMode();
    $(e.item).parent().parent().parent().addClass('k-edit-item');

    var vId = e.model.VideoId;

    window.CURRENT_EDITED_VIDEO_ID = vId;
    window.IS_ANY_VIDEO_IN_EDIT_MODE = true;

    e.item.find('input').focus();
    setTimeout(function () {
        e.item.find('input').select();
    });

    var btn = e.item.find('.btn-save');
    btn.unbind('click').bind('click', function () {
        $('#frmEditVideo').submit();
    });

    window.getNotifManagerInstance().unsubscribe(notifEvents.course.videoSelected, onListVideoSelected, null);
    window.getNotifManagerInstance().subscribe(notifEvents.course.videoSelected, onListVideoSelected, null);
}

function onLinkEdit(e) {
    handleEditMode();
    $(e.item).parent().parent().parent().addClass('k-edit-item');

    window.IS_ANY_VIDEO_IN_EDIT_MODE = true;

    e.item.find('input').focus();
    setTimeout(function () {
        e.item.find('input').select();
    });

    var btn = e.item.find('.btn-save');
    btn.unbind('click').bind('click', function () {
        $('#frmChapterLink').submit();
    });
}


function onListVideoSelected(token) {
    if (token == null) return;
    $('#frmEditVideo #VideoIdentifier').val(token.identifier);
    $('#frmEditVideo .vd-name').html(token.title);
    

    buildThumbMediaPlayer(token, '#d-video-thumb-' + window.CURRENT_EDITED_VIDEO_ID);

    $('#d-video-thumb-' + window.CURRENT_EDITED_VIDEO_ID).unbind('click');
    $('#d-video-thumb-' + window.CURRENT_EDITED_VIDEO_ID).attr({ title: '' });

    closeVideoWnd();
}

function onVideoSaved(response) {
    window.IS_ANY_VIDEO_IN_EDIT_MODE = false;

    if (response.success) {
        window.getNotifManagerInstance().notify(notifEvents.course.courseStateChanged, null);
        response.message = 'Video saved';
        var listSelector = '#ChapterVideos_' + response.result.chapId;

        $(listSelector).parent().parent().removeClass('k-edit-item');

        $(listSelector).data('kendoListView').dataSource.read();

        handleEditModeExit();

    } else {
        window.formUserNotifManager.show({ message: response.error, kind: NotificationKinds.Error });
    }
    window.getNotifManagerInstance().notify(notifEvents.chapter.chapterSaved, response);
}

function onVideoRemoved(e) {
    if (!window.confirm('Delete video?')) e.preventDefault();
}

function onLinkRemoved(e) {
    if (!window.confirm('Delete link?')) e.preventDefault();
}
//links

function addChapterLink(chId, kind) {
    handleEditMode();

    var lv = $('#ChapterLinks_' + chId).data('kendoListView');
    if (window.IS_ANY_VIDEO_IN_EDIT_MODE) {
        //  alert('Other video is editing now. Please save Your work or cancel edit');
        $(CONTENTS_LIST_SELECTOR).find('.ul-second-level').find('.k-cancel-button').click();
    }

    lv.dataSource.add({
        ChapterId: chId,
        LinkId: -1,
        Title:'',// kind == 1 ? "New document" : "New link",
        LinkHref: '',
        OrderIndex: lv.dataSource._data.length,
        Kind: kind
    });

    setTimeout(function () {
        lv.edit(lv.element.children().last());
        window.IS_ANY_VIDEO_IN_EDIT_MODE = true;
    }, 300);


}

function onChapterLinkSaved(response) {
    window.IS_ANY_VIDEO_IN_EDIT_MODE = false;

    if (response.success) {
        exitEditMode();
        window.getNotifManagerInstance().notify(notifEvents.course.courseStateChanged, null);
        response.message = 'Link saved';
        var listSelector = '#ChapterLinks_' + response.result.chapId;
        $(listSelector).parent().parent().removeClass('k-edit-item');
        $(listSelector).data('kendoListView').dataSource.read();
        handleEditModeExit();
    }
    window.getNotifManagerInstance().notify(notifEvents.chapter.chapterSaved, response);
}

function onListDocUploadAction(e) {
    // Array with information about the uploaded files
    var files = e.files;

    if (files.length > 1) {
        alert("Only one document can be uploaded");
        e.preventDefault();
        return;
    }

    handleEditMode();
}

function onListDocUploadSuccess(e) {
    var operation = e.operation;
    var response = e.response;

    hideLoader();

    if (!response) return;

    if (!response.success) {
        alert(response.error);

        return;
    }
    handleEditModeExit();
    //clearUploadConsole();

    switch (operation) {
        case "remove":
            break;
        case "upload":
            try {
                $('#frmChapterLink #a-doc-preview').html(response.result.url).attr({ href: response.result.url });
                $('#frmChapterLink #LinkHref').val(response.result.url);
                setEditMode();
                if (hasValue($('#frmChapterLink #Title').val())) return;
                $('#frmChapterLink #Title').val(response.result.name);
            } catch (e) {
                if (window.console) console.log(e);
            }
            break;
    }

}

