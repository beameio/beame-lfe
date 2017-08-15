
//loader
var DEFAULT_CONTAINER_SELECTOR = '.wrap';
var LOADER_SELECTOR = '#site-loader';
var SITE_LOADER = null;
//command row
var FLOAT_FACTOR = 70;
var FLOAT_HEIGHT = 45;
var IE_SCROLL_FACTOR = 30;

//enums
var FormModes = {
	insert: 'insert'
	,edit: 'edit'
	,view:'view'
};

var ChapterFormKinds = {
	chapter: 'chapter'
	, video: 'video'
	, link: 'link'
};


var PeriodKinds = {
	lastMonth: 1 
	,week : 2
	,thisMonth : 4
	,last90 : 8
	,last180 : 16
	,all : 32
	,period : 64  
};

function onMessageClickEvent(messageId, userOrTagId, kind) {
	var data = {
		messageId:messageId,
		Id: userOrTagId, //Id- syntax is Important , associated with FeedPageToken property
		kind: kind
	};
   window.getNotifManagerInstance().notify(notifEvents.disqus.messageClicked, data);
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

function markHashtag(id,selector) {
    $(selector + ' > li').find(".txt").find('.btn[data-kind=Hashtag][data-val=' + id + ']').addClass('hl');   
}

//#endregion

//#region learner


function onSortFiltersBound(e) {
    var listView = e.sender;

    var notifyEvent = notifEvents.report.sortCourse;

    $.each(listView.element.children().find('a'), function (i) {
        var token = listView.dataSource._data[i];
        var link = $(this);
        link.unbind('click');
        link.bind('click', function () {
            listView.element.children().find('a').removeClass('active');
            $(this).addClass('active');
            var filter = { field: token.field, dir: token.dir };
            window.getNotifManagerInstance().notify(notifyEvent, filter);
        });
        if (i == 0) link.addClass('active');
    });
}

function onCourseListBound() {
    window.getNotifManagerInstance().unsubscribe(notifEvents.report.sortCourse, sortCourseList, null);
    window.getNotifManagerInstance().subscribe(notifEvents.report.sortCourse, sortCourseList, null);
    window.getNotifManagerInstance().notify(notifEvents.course.listBound, null);
}

function onUserCourseListBound() {
    window.getNotifManagerInstance().notify(notifEvents.course.listBound, null);
}


function sortCourseList(filter) {
    if ($('#LearnerCourses').data("kendoListView") == undefined) return;
    $('#LearnerCourses').data("kendoListView").dataSource.sort(filter);
}

function sortListById(id, filter) {
    if($('#' + id).data("kendoListView")==undefined) return;
    $('#'+id).data("kendoListView").dataSource.sort(filter);
}
//#endregion

function selectListItemById(listSelector, id, key) {
	var listView = $(listSelector).data("kendoListView");
	var children = listView.element.children();
	var index = -1;
	for (var x = 0; x < children.length; x++) {
		if (listView.dataSource.view()[x][key] == id) {
			index = x;
		};
	};

	if (index < 0) return;

	listView.select(children[index]);
	// $(".nano").nanoScroller({ scrollTop: $(children[index]).offset().top }); scrollTo
	$(".nano").nanoScroller({ scrollTo: $(children[index]) });
}

//#region common helpers
//fix scale problem of kendo window , when flash object is on content
function fixScale(e) {
	setTimeout(function () {
		e.sender.wrapper.css({ 'transform': '' });
	}, 5);
}


var delay = (function () {
	var timer = 0;
	return function (callback, ms) {
		clearTimeout(timer);
		timer = setTimeout(callback, ms);
	};
})();
//#endregion

//#region messaging
function onTxtBoxChanged($this) {
	var tb = $($this);
	var s = '#' + tb.attr('id');
	var v = '.validator-container';
	if (hasValue(tb.val())) {
		setValidPass(s, v);
	} else {
		setValidReq(s, v);
	}
}

function setValidPass(elmntSelector, validSelector) {
	$(elmntSelector).parent().siblings(validSelector).addClass('pass');
}
function setValidReq(elmntSelector, validSelector) {
	$(elmntSelector).parent().siblings(validSelector).removeClass('pass');
}
//#endregion



//#region uplaoder services


function onUploadProgress(e) {
	window.uploadConsole.html("Upload progress :: " + e.percentComplete + "% :: " + getFileInfo(e));
}
function clearUploadConsole() {
	window.uploadConsole.empty();
}

function getFileInfo(e) {
	return $.map(e.files, function (file) {
		var info = file.name;

		// File size is not available in all browsers
		if (file.size > 0) {
			info += " (" + Math.ceil(file.size / 1024) + " KB)";
		}
		return info;
	}).join(", ");
}

//#endregion

function buildThumbMediaPlayer(token, containerSelector) {

	var data = {
	    stillUrl: fixSR(token.stillUrl)
	  , title: token.title
	  , minutes: token.minutes
	  , identifier: token.identifier
	};

	var template = kendo.template($("#kendo-video-thumb-template").html());

	$(containerSelector).html(template(data));

	var id = token.identifier;

	var player = $(containerSelector).find('#player-' + id);
	var hint = $(containerSelector).find('#hint-' + id);
	var btn = hint.find('#play-' + id);
	var thumb = $(containerSelector).find('#thumb-' + id);
	var btnSelect = $(containerSelector).find('.cmd-row > span');

	btnSelect.unbind('click').bind('click', function () {
		window.getNotifManagerInstance().notify(notifEvents.course.videoSelected, token);
	});



	btn.bind("click", function () {
		thumb.hide();
		hint.hide();
		player.show();
		showContainerLoader('#d-video-thumb', null);
		BCL.addPlayer(player, id, window.BC_TH_PLAEYR_W, window.BC_TH_PLAEYR_H);
	});

	thumb.hover(
				  function () {
					  hint.show();
				  }
			   );
	hint.hover(
				 function () {
				 },
				 function () {
					 hint.hide();
				 }
			  );
}

//(function ($) {
//    $.fn.ellipsis = function () {
//        return this.each(function () {
//            var el = $(this);

//            if (el.css("overflow") == "hidden") {
//                var text = el.html();
//                var multiline = el.hasClass('multiline');
//                var t = $(this.cloneNode(true))
//                                .hide()
//                                .css('position', 'absolute')
//                                .css('overflow', 'visible')
//                                .width(multiline ? el.width() : 'auto')
//                                .height(multiline ? 'auto' : el.height())
//                ;

//                el.after(t);

//                function height() { return t.height() > el.height(); };
//                function width() { return t.width() > el.width(); };

//                var func = multiline ? height : width;

//                setTimeout(function () {
//                    while (text.length > 0 && func()) {
//                        text = text.substr(0, text.length - 1);
//                        t.html(text + "...");
//                    }
//                }, 100);

//                el.html(t.html());
//                t.remove();
//            }
//        });
//    };
//})(jQuery);

//String.prototype.parseHashtag = function () {
//    return this.replace(/[#]+[A-Za-z0-9-_]+/g, function (t) {
//        var tag = t.replace("#", "%23")
//        return t.link("http://search.twitter.com/search?q=" + tag);
//    });
//};
//String.prototype.parseUsername = function () {
//    return this.replace(/[@]+[A-Za-z0-9-_]+/g, function (u) {
//        var username = u.replace("@", "")
//        return u.link("http://twitter.com/" + username);
//    });
//};
//String.prototype.parseURL = function () {
//    return this.replace(/[A-Za-z]+:\/\/[A-Za-z0-9-_]+\.[A-Za-z0-9-_:%&~\?\/.=]+/g, function (url) {
//        return url.link(url);
//    });
//};