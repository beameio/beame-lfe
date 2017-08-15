var emptyHref = '#';

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



function isFxSupported() {
	var b = $.browser;
	return !b.msie || (b.msie && parseInt(b.version) > 9);
}

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
function cleanWndContent(e) {
	e.sender.wrapper.find('.k-window-content').html("...");
	//$('.k-window, .k-overlay').remove();
}
function setReportScroll(selector) {  
	$(selector).nanoScroller({ sliderMinHeight:80, alwaysVisible: true });
}

function setVisibility(selector, isVisible) {
	$(selector).css({ 'visibility': isVisible ? 'visible' : 'hidden' });
}


function isNumber(n) {
	return !isNaN(parseFloat(n)) && isFinite(n);
}

function hasValue(value) {

	if (value == null) return false;

	if (isNumber(value)) {
		return value > 0;
	}

	return value.length > 0;
}

function onWndClose(e) {
	e.sender.content(' ');
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

		var playerInstance = jwplayer(player.attr('id'));
		playerInstance.setup({
		    file: fixSR(token.videoUrl),
		    image: fixSR(token.stillUrl),
		    autostart: true,
		    width: window.BC_TH_PLAEYR_W,
		    height: window.BC_TH_PLAEYR_H,
		    skin: '/Scripts/jwplayer/jwplayer-skins-premium/vapor.xml'
		});
		//BCL.addPlayer(player, id, window.BC_TH_PLAEYR_W, window.BC_TH_PLAEYR_H);
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