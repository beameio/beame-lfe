var FEED_REFRESH_INTERVAL;
//
var CHAPTER_CONTAINER_SELECTOR = "#d-chapter-content-container";
var BTN_CHPTER_TOGGL_PREFIX = 'btn-ch-toggle-';
var BTN_SELECT_VIDEO_PREFIX = 'btn-vd-select-';
var CHAPTER_UL_PREFIX = 'ul-v-';
var VIDE_NAME_PREFIX = 'lbl-video-name-';
var VIDEO_DESC_CONTAINER_PREFIX = 'spn-vd-des-';
var VIDEO_TITLE_SELECTOR = '#lbl-video-title';
var VIDEO_DESC_SELECTOR = '.ch-desc';
var CURRENT_VIDEO_TOKEN;
var VIDEO_NAV_ARRAY;
var isChapterLinksBound = false;
var currentChapterId = -1;
var CONTENT_CONTAINER_SELECTOR = ".cv-content";
var CONTENT_TAB_FORM_SELECTOR = '#frmContents';
var CONTENT_TAB_BTN_SELECTOR = '.tb-content';
var notifyVideoSelection = false;
var SELECTED_VIDEO_ID = null;

var tabsHashPrefix = {
    CONTENT: "tabContent"
    , LEARNER: "tabLearner"
    , DISCUSS: "tabDiscuss"
};

function notifyHeightChange() {
    window.getNotifManagerInstance().notify(notifEvents.window.heightChanged);
}

function getCurrentTab() {
    var h = parseHash();

    return h[0];
}

function parseHash() {
    var hash = window.location.hash ? window.location.hash.substring(1) : null;
    if (hash == null) return [tabsHashPrefix.CONTENT];

    return hash.split('-');
}

function onCoursePageLoad(lastVideoId) {
    var hash = parseHash();
    if (lastVideoId < 0) {
        if (hash.length > 1) {
            if (hash.length == 3) {
                var video = findVideo(hash[2]);
                CURRENT_VIDEO_TOKEN = video != null ? video : VIDEO_NAV_ARRAY[0];
            } else {
                CURRENT_VIDEO_TOKEN = VIDEO_NAV_ARRAY[0];
            }
        } else {
            CURRENT_VIDEO_TOKEN = VIDEO_NAV_ARRAY[0];
        }
    } else {
        video = findVideo(lastVideoId);
        CURRENT_VIDEO_TOKEN = video != null ? video : VIDEO_NAV_ARRAY[0];
    }

    if (CURRENT_VIDEO_TOKEN == null) return;
    //set hash
    window.location.hash = hash[0] + '-' + CURRENT_VIDEO_TOKEN.chapterId + '-' + CURRENT_VIDEO_TOKEN.videoId;
    //toggle chapter
    $('#' + BTN_CHPTER_TOGGL_PREFIX + CURRENT_VIDEO_TOKEN.chapterId).click();
    //select video
    $('#' + BTN_SELECT_VIDEO_PREFIX + CURRENT_VIDEO_TOKEN.videoId).click();
}

function findVideo(id) {
    var json = VIDEO_NAV_ARRAY;
    if (json.length == 0) return null;

    for (var i = 0; i < json.length; i++) {
        if (json[i].videoId == id) return json[i];
    }

    return null;
}

function selectVideo(chapterId, videoId, $this, switch2Tab) {
    //reset selection
    $('.li-video').removeClass('selected');
    ($($this).parent().parent()).addClass('selected'); //li

    //expand chapter if necessary
    var ul = $('#' + CHAPTER_UL_PREFIX + chapterId);
    var toBeOpen = ul.is(":hidden");
    if (toBeOpen) {
        $('#' + BTN_CHPTER_TOGGL_PREFIX + chapterId).click();
    }

    if (switch2Tab != null && switch2Tab) {
        var currentTab = getCurrentTab();
        if (currentTab != tabsHashPrefix.CONTENT) {
            $(CONTENT_TAB_BTN_SELECTOR).focus().click();
            notifyVideoSelection = true;
            SELECTED_VIDEO_ID = videoId;
            var video = findVideo(SELECTED_VIDEO_ID);
            window.CURRENT_VIDEO_TOKEN = video;


            checkPlayerVisibility();
            return;
        } else {
            checkPlayerVisibility();
        }
    }
    window.getNotifManagerInstance().notify(notifEvents.video.videoSelected, videoId);
}

function checkPlayerVisibility() {
    if ($('#bc-player-container').is(":visible")) return;
    
    $('.user-g2t-container').hide();
    $('.user-quiz-container').hide();
    $('#bc-player-container').show();
    //$('.user-g2t-container').slideToggle(500, function () {
    //    $('#bc-player-container').slideToggle(500);
    //});
}

function onVideoSelected(videoId) {

    var video = findVideo(videoId);

    if (video == null) {
        alert('video not found');
        return;
    }

    loadVideoData(video);
}

function loadVideoData(video) {

    var isChapterChanged = !isChapterLinksBound || window.CURRENT_VIDEO_TOKEN == null || window.CURRENT_VIDEO_TOKEN.chapterId != video.chapterId;

    window.CURRENT_VIDEO_TOKEN = video;

    if (isChapterChanged) {
        isChapterLinksBound = true;
        currentChapterId = window.CURRENT_VIDEO_TOKEN.chapterId;
        window.getNotifManagerInstance().notify(notifEvents.chapter.chapterChanged, window.CURRENT_VIDEO_TOKEN.chapterId);
    }

    var h = parseHash();

    window.location.hash = h[0] + '-' + CURRENT_VIDEO_TOKEN.chapterId + '-' + CURRENT_VIDEO_TOKEN.videoId;

    window.getNotifManagerInstance().notify(notifEvents.video.videoChanged, video);
    window.getNotifManagerInstance().notify(notifEvents.course.saveState, video);
    // setVideoContent(video);

}

function setVideoContent(video) {
    window.getBcPlayerInstance().loadVideo(video);

    var name = $('#' + VIDE_NAME_PREFIX + video.videoId).html();
    var desc = $('#' + VIDEO_DESC_CONTAINER_PREFIX + video.videoId).html();

    $(VIDEO_TITLE_SELECTOR).fadeOut("slow", function () {
        $(VIDEO_TITLE_SELECTOR).html(name).removeClass('qz').fadeIn("slow");
    });

    $(VIDEO_DESC_SELECTOR).fadeOut("slow", function () {
        $(VIDEO_DESC_SELECTOR).html(desc).fadeIn("slow");
    });
}

function setNavButtonsState(video) {
    var nextId = video.nextId;
    var prevId = video.prevId;

    var btnPrev = $('#d-player-nav > .pl-nav-btn > .prev');
    var btnNext = $('#d-player-nav > .pl-nav-btn > .next');

    btnPrev.attr('data-val', prevId);
    btnNext.attr('data-val', nextId);

    if (nextId < 0) {
        btnNext.addClass('off');
    } else {
        btnNext.removeClass('off');
    }
    if (prevId < 0) {
        btnPrev.addClass('off');
    } else {
        btnPrev.removeClass('off');
    }
}
function onNavButtonClicked(direction, $this) {
    var btn = $($this);
    var targetVideoId = Number(btn.attr('data-val'));
    if (targetVideoId < 0) return;
    $('#' + BTN_SELECT_VIDEO_PREFIX + targetVideoId).click();
}

function setAutoplay() {
    var btn = $('#btn-autoplay');
    var isAutoplay = btn.hasClass('off');

    if (isAutoplay) {
        btn.removeClass('off').addClass('on').html('on');
    } else {
        btn.removeClass('on').addClass('off').html('off');
    }

    // window.getBcPlayerInstance().isAutoPlay = isAutoplay;
}

//#region feed sorting
function onFeedSortFiltersBound(e) {
    var listView = e.sender;

    $.each(listView.element.children().find('a'), function (i) {
        var token = listView.dataSource._data[i];
        var link = $(this);
        link.unbind('click');
        link.bind('click', function (ev) {
            ev.preventDefault();
            listView.element.children().find('a').removeClass('active');
            listView.element.children().find('.k-icon').remove();
            var el = $(this);
            el.addClass('active');
            switch (token.field) {
                case "AddOn":
                    var dir = el.attr("data-dir");
                    var func = dir == "asc" ? sortDateAsc : sortDateDesc;
                    $(window.FEED_TREE_SELECTOR + ' > li[class=li-topic]').sort(func).appendTo(window.FEED_TREE_SELECTOR);
                    break;
                case "CreatorName":
                case "CourseName":
                    var ctx = el.attr("data-dir");
                    dir = hasValue(ctx) ? (ctx == "asc" ? "desc" : "asc") : "asc";
                    el.attr({ "data-dir": dir });

                    if (el.parent().find('.k-icon').length == 0) {
                        var ar = $('<span />').addClass('k-icon').addClass(dir == "asc" ? "k-i-arrow-s" : "k-i-arrow-n");
                        el.parent().append(ar);
                    } else {
                        el.parent().find('.k-icon').removeClass("k-i-arrow-s k-i-arrow-n").addClass(dir == "asc" ? "k-i-arrow-s" : "k-i-arrow-n");
                    }

                    func = token.field == "CreatorName" ? (dir == "asc" ? sortNameAsc : sortNameDesc) : (dir == "asc" ? sortCourseAsc : sortCourseDesc);
                    $(window.FEED_TREE_SELECTOR + ' > li[class=li-topic]').sort(func).appendTo(window.FEED_TREE_SELECTOR);
                    break;
            }


        });
        if (i == 0) link.addClass('active');
    });
}

function sortNameAsc(a, b) {
    var n1 = $(a).find(".topic").text();
    var n2 = $(b).find(".topic").text();
    return n1 > n2 ? 1 : -1;
}
function sortNameDesc(a, b) {
    var n1 = $(a).find(".topic").text();
    var n2 = $(b).find(".topic").text();
    return n2 > n1 ? 1 : -1;
}
function sortCourseAsc(a, b) {
    var n1 = $(a).find(".fc-name").text();
    var n2 = $(b).find(".fc-name").text();
    return n1 > n2 ? 1 : -1;
}
function sortCourseDesc(a, b) {
    var n1 = $(a).find(".fc-name").text();
    var n2 = $(b).find(".fc-name").text();
    return n2 > n1 ? 1 : -1;
}
function sortDateDesc(a, b) {
    var date1 = kendo.parseDate($(a).attr("data-addon"));
    var date2 = kendo.parseDate($(b).attr("data-addon"));
    return date2 > date1 ? 1 : -1;
}
function sortDateAsc(a, b) {
    var date1 = kendo.parseDate($(a).attr("data-addon"));
    var date2 = kendo.parseDate($(b).attr("data-addon"));
    return date1 > date2 ? 1 : -1;
}

function markHashtag(id, selector) {
    $(selector + ' > li').find(".txt").find('.btn[data-kind=Hashtag][data-val=' + id + ']').addClass('hl');
}

//#endregion

//#region tree events

function togglChapter(id, $this, factor) {
    var v = $('#ul-v-' + id);

    var t = factor == 1 ? $($this).parent().parent() : $($this).parent();
    var toBeOpen = v.is(":hidden");

    v.slideToggle("slow", function () {
        if (toBeOpen) {
            t.addClass('no-bord');
        } else {
            t.removeClass('no-bord');
        }
        notifyHeightChange();
        //refresh scroller
        // $('#d-ch-tree-container').nanoScroller();
    });

    if (toBeOpen) {
        t.find('.l-icon').removeClass('l-plus').addClass('l-minus');
    } else {
        t.find('.l-icon').addClass('l-plus').removeClass('l-minus');
    }

}

function togglVideo(id, $this) {
    var v = $('#vd-desc-' + id);

    var t = $($this).parent().parent();
    var toBeOpen = v.is(":hidden");

    if (toBeOpen) {
        t.find('.l-icon').removeClass('l-plus').addClass('l-minus');
    } else {
        t.find('.l-icon').addClass('l-plus').removeClass('l-minus');
    }

    v.slideToggle("slow", notifyHeightChange);
}



function expandCollapse(t) {

    $.each($('.ul-chapters-tree').find('.l-icon'), function () {
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
//#endregion tree events

//#region tabs events
function initTabEvents() {

    $('#iv-nav-tabs > li > a[data-toggle="tab"]').on('shown.bs.tab', function (e) {

      //  var id = $(e.target).attr("href").substr(1);
      //  window.location.hash = id;

        //imported from UserPortal _CourseNavigation
        clearInterval(FEED_REFRESH_INTERVAL);
        var link = $(this);

        if (link.attr('href').substr(1) == tabsHashPrefix.CONTENT) {
            $('.autoplay-container').show();
        }
        else {
            $('.autoplay-container').hide();
        }

        var h = parseHash();
        window.location.hash = link.attr('href').substr(1) + '-' + h[1] + '-' + h[2];

      
        window.getBcPlayerInstance().resetPlayer();
        //end import

        var url = $(this).attr("data-url");

        var target = $(e.target).attr("href"); // activated tab

        if ($(target).is(':empty')) {

            var pane = $(this);
           
            $(target).load(url, function () {
                //  hideFormLoader();
                pane.tab('show');

                if (window.notifyVideoSelection) {
                    window.notifyVideoSelection = false;
                    window.getNotifManagerInstance().notify(notifEvents.video.videoSelected, window.SELECTED_VIDEO_ID);
                    window.SELECTED_VIDEO_ID = null;
                }
            });
        }
    });

    var hash = parseHash();
    if (hasValue(hash)) {
        $('#iv-nav-tabs a[href="#' + hash[0] + '"]').tab('show');
        $('#' + hash[0]).addClass('in');
    }
}

//#endregion

//#region player helpers
function getBcPlayerInstance() {
    if (window.bcLfePlayer == undefined || window.bcLfePlayer == null) {

        var data = {
            containerSelector: '#bc-player-container',
            videosList: window.VIDEO_NAV_ARRAY,
            isAutoPlay: true,
            playerW: 600,
            playerH: 338
        };

        window.bcLfePlayer = new BclPlayer(data);
    }
    return window.bcLfePlayer;
}

function saveCoursState(video) {

    var data = {
        courseId: window.CURRENT_COURSE_ID
        , chapterId: video.chapterId
        , videoId: video.videoId
        , bcId: video.bcId
        , isSecured: window.isSecured
    };

    $.ajax({
        url: window.saveLearnerStateUrl,
        cache: false,
        type: "Post",
        data: JSON.stringify(data),
        datatype: "json",
        contentType: "application/json; charset=utf-8"
    });
}
//#endregion

//#region review
function openReviewWnd() {

    var m = $('#modWriteReview');

    m.find('.modal-body').load(window.reviewWndUrl, function () {
        m.modal('show');
    });
}


function updateRating(value) {
    var jst = kendo.template($("#item-rating-template").html());
    
    $("#head-item-rating").html(jst(value));
}

function onReviewSaveBegin() {
    window.showFormLoader(window.REVIEW_FORM_SELECTOR);

    return true;
}

function onReviewSaved(response) {
    window.hideFormLoader();
    var kind = response.success ? NotificationKinds.Success : NotificationKinds.Error;
    var msg = response.success ? userMessages.REVIEW_SAVED : response.error;

    window.formUserNotifManager.show({ message: msg, kind: kind });

    if (response.success) {
        $('#modWriteReview').modal('hide');
        //window.getNotifManagerInstance().notify(notifEvents.course.ratingUpdated, response.result);
        //setTimeout(function () {
        //    $('#modWriteReview').modal('hide');
        //}, 2500);
    }
}
//#endregion

//#region FB watch video story
function PublishFacebookStory(video) {
    var isPostStory = readCookie("lfe_FacebookStory");

    if (isPostStory == '1') {
        return;
    }

    $("#msgFacebookStory").toggle();
    $("#msgFacebookOff").toggle();

    var data = {
          courseId: window.CURRENT_COURSE_ID
        , chapterId: video.chapterId
        , videoId: video.videoId
        , bcId: video.bcId
    };

    $.ajax({
        url: window.fbPostVideoStoryUrl,
        cache: false,
        type: "Post",
        data: JSON.stringify(data),
        datatype: "json",
        contentType: "application/json; charset=utf-8"
    });


}
var turnOff = "Turn Off";
var turnOn = "Turn On";

function CloseFacebookMessage() {
    $("#msgFacebookStory").toggle();
    $("#msgFacebookOff").toggle();
}

function SetFacebookCoockie() {
    var isPostStory = readCookie("lfe_FacebookStory");

    if (isPostStory == '1') {
        isPostStory = '0';
        UpdateFaceboockSwitchTitle(turnOff);
    }
    else {
        isPostStory = '1';
        UpdateFaceboockSwitchTitle(turnOn);
    }

    createCookie("lfe_FacebookStory", isPostStory, 1000);
}

$(document).ready(function () {
    var isPostStory = readCookie("lfe_FacebookStory");

    if (isPostStory == '1') {
        UpdateFaceboockSwitchTitle(turnOn);
    }
    else {
        UpdateFaceboockSwitchTitle(turnOff);
    }
});

function UpdateFaceboockSwitchTitle(text) {
    $("#aFacebookSwitch2").text(text);
    $("#aFacebookSwitch2").text(text);
}

function readCookie(name) {
    var nameEQ = escape(name) + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return unescape(c.substring(nameEQ.length, c.length));
    }
    return null;
}

function createCookie(name, value, days) {
    var expires;

    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toGMTString();
    } else {
        expires = "";
    }

    document.cookie = escape(name) + "=" + escape(value) + expires + "; path=/";
}

//#endregion
function addMinutes(date, minutes) {
    return new Date(date.getTime() + minutes * 60000);
}