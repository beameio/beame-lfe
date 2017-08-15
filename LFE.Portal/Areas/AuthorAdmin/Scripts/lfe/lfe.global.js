
//loader
var DEFAULT_CONTAINER_SELECTOR = '.wrap';
var LOADER_SELECTOR = '#site-loader';
var SITE_LOADER = null;
//command row
var FLOAT_FACTOR = 70;
var FLOAT_HEIGHT = 45;
var IE_SCROLL_FACTOR = 30;

var COUPON_FORM_HEIGHT = 390;

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

var StoreFormKinds = {
	 category: 'category'
	, single: 'single'
	 , lfecategory: 'lfecategory'
	 ,author:'author'
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
var lfe = {
	SignOff: function () {
		try {
			if (window.parent && window.parent != window.self) {
				window.parent.lfe.SignOff();
			}
		} catch (e) { }
	},
	isMobile: {
		Android: function () {
			return navigator.userAgent.match(/Android/i);
		},
		BlackBerry: function () {
			return navigator.userAgent.match(/BlackBerry/i);
		},
		iOS: function () {
			return navigator.userAgent.match(/iPhone|iPad|iPod/i);
		},
		Opera: function () {
			return navigator.userAgent.match(/Opera Mini/i);
		},
		Windows: function () {
			return navigator.userAgent.match(/IEMobile/i);
		},
		any: function () {
			return (this.Android() || this.BlackBerry() || this.iOS() || this.Opera() || this.Windows());
		}
	}
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

//#region video selection video
function onVideoSelected(token) {
    
	if (token == null) return;
	$(window.VIDEO_FIELD_NAME).val(token.identifier);
	$(window.EDIT_FORM_SELECTOR).validate().element($(window.VIDEO_FIELD_NAME));

	buildThumbMediaPlayer(token, '#d-video-thumb');

	setValidPass('#d-video-thumb', '#valid-video');

	$('#d-video-thumb').unbind('click');
	$('#d-video-thumb').attr({ title: '' });

	closeVideoWnd();

}

function closeVideoWnd() {
	$("#wndVideo").data("kendoWindow").close();
}

function openVideoWnd() {

	var wnd = $("#wndVideo").data("kendoWindow");
	var url = window.videoSelectionUrl;
	wnd.refresh(url);
	wnd.center();
	wnd.open();
}
function onAccWndClose(e) {
	e.sender.content(' ');
}
//#endregion

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

//#region adjust command row position       
function getScrollTop() {
	if (typeof pageYOffset != 'undefined') {
		//most browsers
		return pageYOffset;
	}
	else {
		var b = document.body; //IE 'quirks'
		var d = document.documentElement; //IE with doctype
		d = (d.clientHeight) ? d : b;
		return d.scrollTop;
	}
}

function isScrolledIntoView(elem) {
	var docViewTop = $(window).scrollTop();
	var docViewBottom = docViewTop + $(window).height();

	var elemTop = $(elem).offset().top;
	var elemBottom = elemTop + $(elem).height();

	return ((elemBottom <= docViewBottom) && (elemTop >= docViewTop));
}

function adjustCmdPosition(formSelector, hFactor) {

	if (!hasValue(hFactor)) {
		window.FLOAT_FACTOR = 70;
	} else {
		window.FLOAT_FACTOR = hFactor;
	}

	delay(function () {

		var cmd = $('.form-cmd-row');
		var form = $(formSelector);

		if (form.length == 0) return;

		var offset = form.offset();
		var fBottom = parseInt(offset.top + window.FORM_HEIGHT);
		var wH = parseInt($(window).height());


		var top;
		var p;

		//var a = $('#d-fd-bottom').is(':appeared');
		//var b = isScrolledIntoView('#d-fd-bottom');
		//var o = $('#d-fd-bottom').offset().top;
		//var f = $('footer').offset().top;

		//console.log('a=' + a + ' b=' + b);
		//console.log('o=' + o + ' f=' + f);

		var factor;

		if (jQuery.browser.webkit) {
			factor = 30;
		} else {
			factor = 25;
		}

		if (fBottom <= wH && (Math.abs(fBottom - wH) > window.FLOAT_FACTOR + factor)) // bottom of form visible
		{
			var tt = fBottom + window.FLOAT_FACTOR;

			if (wH - tt < window.FLOAT_FACTOR) {
				p = true;
				top = wH - FLOAT_HEIGHT;
			} else {
				p = false;
				top = fBottom + window.FLOAT_FACTOR;
			}

		} else { // bottom of form under scroll


			var scroll = getScrollTop();

			if (jQuery.browser.msie) {
				scroll = scroll + IE_SCROLL_FACTOR;
			}

			var bt = scroll + window.FORM_HEIGHT;

			if (jQuery.browser.webkit) {
				factor = 100;
			} else {
				factor = 30;
			}
			// console.log(factor);

			if (bt > fBottom || (Math.abs(fBottom - bt) <= factor)) {

				p = false;
				top = fBottom + window.FLOAT_FACTOR - scroll - 10;
			} else {

				p = true;
				top = wH - FLOAT_HEIGHT;
			}

		}

		var bg = p ? '#cacaca' : '#fff';
		var op = 1;//p ? 0.5 : 1;

		cmd.animate({ top: top, left: $('#main').offset().left }, 150, function () {
			cmd.css({ 'opacity': op, 'background-color': bg });
		});

	}, 150);
}
//#endregion

//#region uploader services
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
	var id = token.identifier;
	if (id == undefined) return;
	//console.log(id);

	var data = {
	    stillUrl: fixSR(token.stillUrl)
	  , title: token.title
	  , minutes: token.minutes
	  , identifier: token.identifier
	};

	var template = kendo.template($("#kendo-video-thumb-template").html());

	$(containerSelector).html(template(data));

  
	var player = $(containerSelector).find('.jwp');
	var hint = $(containerSelector).find('#hint-' + id);
	var btn = hint.find('#play-' + id);
	var thumb = $(containerSelector).find('#thumb-' + id);
	var size = window.BC_TH_PLAEYR_W +'px ' + window.BC_TH_PLAEYR_H + 'px';
	thumb.css({'background-size': size});
	var btnSelect = $(containerSelector).find('.cmd-row > span');

	btnSelect.unbind('click').bind('click', function () {
		window.getNotifManagerInstance().notify(notifEvents.course.videoSelected, token);
	});



	btn.bind("click", function () {
		thumb.hide();
		hint.hide();
		player.show();
	    //showFormLoader(containerSelector, 150);
		var playerInstance = jwplayer(player.attr('id'));
		playerInstance.setup({
		    file: token.videoUrl,
		    image: token.stillUrl,
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
