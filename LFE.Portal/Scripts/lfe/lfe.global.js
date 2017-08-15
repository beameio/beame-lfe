var emptyHref = '#';
var emptyGuid = "00000000-0000-0000-0000-000000000000";
var IMAGE_EXTENSIONS = ['.jpeg', '.jpg', '.gif', '.png'];
var VIDEO_EXTENSIONS = ['.mov', '.mpg', '.mpeg4', '.mpeg', '.mp4', '.mv4', '.wmv', '.avi', '.ogv', '.3gp', '.3g2', '.h264','.qt','.flv'];

$(document).ready(function () {
	if (window.location.hash == "#_=_")
		window.location.hash = "";
});

var ePurchaseItemTypes = { //BASE_ItemTypesLOV
	COURSE: 1,
	BUNDLE: 2
}

var USER_EVENTS = {
	REGISTRATION_SUCCESS: 1,
	REGISTRATION_FAILED: 2,
	LOGIN_SUCCESS: 3,
	LOGIN_FAILED: 4,
	VIDEO_PREVIEW_WATCH: 5,
	VIDEO_COURSE_WATCH: 6,
	BUY_PAGE_ENTERED: 7,
	PURCHASE_COMPLETE: 8,
	PURCHASE_FAILED: 9,
	SEARCH_USAGE: 10,
	SESSION_START: 11,
	DASHBOARD_VIEW: 12,
	COURSE_CREATED: 13,
	VIDEO_UPLOAD: 14,
	STORE_CREATED: 15,
	ROOM_CREATED: 16,
	WIX_APP_PUBLISHED: 17,
	WIX_APP_DELETED: 18,
	COURSE_PUBLISHED: 19,
	COURSE_VIEWER_ENTER: 20,
	COURSE_PREVIEW_ENTER: 21,
	STORE_VIEW: 22,
	BUNDLE_CREATED: 23,
	CHECKOUT_LOGIN: 24,
	CHECKOUT_REGISTER: 25,
	CHECKOUT_ERROR: 26
};

var CouponTypes = {
	PERCENT: 1
	, FIXED: 2
	, FREE: 3
	, SUBSCRIPTION: 4
};
function newGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
function onNotifShow(e) {
	var w = GetWidth();
	e.element.parent().animate({left:(w/2-170) + 'px'});
}
function GetWidth() {
	var x = 0;
	if (self.innerHeight) {
		x = self.innerWidth;
	}
	else if (document.documentElement && document.documentElement.clientHeight) {
		x = document.documentElement.clientWidth;
	}
	else if (document.body) {
		x = document.body.clientWidth;
	}
	return x;
}
function onPaymentsBound() {
	var dataSource = this.dataSource;
	this.element.find('tr.k-master-row').each(function() {
		var row = $(this);
		var data = dataSource.getByUid(row.data('uid'));
		// this example will work if ReportId is null or 0 (if the row has no details)
		if (data.get('TotalRefunded')==0) {
			row.find("td.k-hierarchy-cell .k-icon").removeClass();
		}
	});
	//if (this.dataSource.data().length === 0) {
	//    var masterRow = this.element.closest("tr.k-detail-row").prev();
	//    e.sender.collapseRow(masterRow);
	//    masterRow.find("td.k-hierarchy-cell .k-icon").removeClass();
	//}
}

function fixSR(url) {
    return url.replace(/amp;/g, '');
}

//#region form
function adjustValidatorsH() {
	$.each($('.ul-edit-form').find('.field-validation-valid'), function () {
		$(this).height($(this).parent().height());
	});
}

function initUnobstructiveFormValidation(form) {
	if (form.length == 0) return;
	$.validator.unobtrusive.parse(form);
	form.validate().settings.ignore = [];
}

function submitForm(id) {
	var form = $('#' + id);
	//$.validator.unobtrusive.parse(form);
	//var v = form.validate();
   // var v = window.formValidator.validate();
	form.submit();
}

function actionResult(url, data, targetContainerId) {
	window.showLoader();

	$.ajax({
		url: url,
		cache: false,
		type: "Post",
		data: JSON.stringify(data),
		datatype: "json",
		contentType: "application/json; charset=utf-8"
		, success: function (view) {
			$('#' + targetContainerId).empty().html(view);
			setTimeout(window.hideLoader, 150);   
		}
	});
}

function actionFormResult(url, data, targetSelector) {
	showFormLoader(targetSelector);

	$.ajax({
		url: url,
		cache: false,
		type: "Post",
		data: JSON.stringify(data),
		datatype: "json",
		contentType: "application/json; charset=utf-8"
		, success: function (view) {
			$(targetSelector).empty().html(view);
			setTimeout(hideFormLoader, 50);
		}
	});
}

function actionFormResultWithContainer(url, data, targetSelector,containerSelector) {
	showContainerLoader(containerSelector);

	$.ajax({
		url: url,
		cache: false,
		type: "Post",
		data: JSON.stringify(data),
		datatype: "json",
		contentType: "application/json; charset=utf-8"
		, success: function (view) {
			$(targetSelector).empty().html(view);
			setTimeout(hideFormLoader, 50);
		}
	});
}

function actionFormResultWithCallback(url, data, targetSelector, containerSelector,callback) {
	showContainerLoader(containerSelector);

	$.ajax({
		url: url,
		cache: false,
		type: "Post",
		data: JSON.stringify(data),
		datatype: "json",
		contentType: "application/json; charset=utf-8"
		, success: function (view) {
			$(targetSelector).empty().html(view);
			setTimeout(hideFormLoader, 50);
			if (callback == null) return;
			callback();
		}
	});
}

function ajaxLoadPartial(form, data) {
	window.showLoader();
	var url = form.attr('action');
	var targetContainerId = form.attr('data-ajax-update');
	$.ajax({
		url: url,
		cache: false,
		type: "Post",
		data: JSON.stringify(data),
		datatype: "json",
		contentType: "application/json; charset=utf-8"
	, success: function (view) {
		$(targetContainerId).empty().html(view);
		setTimeout(window.hideLoader, 150);

	}
	});
}

function ajaxAction(url, data, callback, loaderSelector) {
	if (loaderSelector == null) {
		window.showLoader();
	} else {
		showContainerLoader(loaderSelector);
	}
	$.ajax({
			url: url,
			cache: false,
			type: "Post",
			data: JSON.stringify(data),
			datatype: "json",
			contentType: "application/json; charset=utf-8"
		, success: function (response) {
			hideLoader();
			if (callback == null) return;
			callback(response);
		}
		,error:function() {
			hideLoader();
		}
		});
}

function saveEventApi(eventId,tid,cid,bid,vid,add) {

	var data = {
		 eventId : eventId
		,trackingID: tid
		,courseId: cid
		,bundleId: bid
		,bcId:vid //brightcove video identifier
		,additional: add
	};

	$.ajax({
		url: window.saveEventUrl,
		cache: false,
		type: "Post",
		data: JSON.stringify(data),
		datatype: "json",
		contentType: "application/json; charset=utf-8"	
	});
}
function saveItemEvent(eventId, tid, itemId, itemType) {
	var cid = ePurchaseItemTypes.COURSE == itemType ? itemId : null;
	var bid = ePurchaseItemTypes.BUNDLE == itemType ? itemId : null;
	saveEventApi(eventId, tid, cid, bid);
}

function postForm(form, callback) {
	var url = form.attr('action');
	var formData = form.serialize();
	window.showLoader(1000);
	$.post(url, formData, function (response) {
		window.hideLoader();
		if (callback == null) return;
		callback(response);
	});
}

function postContextForm(form, callback,context) {
	var url = form.attr('action');
	var formData = form.serialize();
	window.showLoader(1000);
	$.post(url, formData, function (response) {
		window.hideFormLoader();
		window.hideLoader();
		callback(context,response);
	});
}
//#endregion

//#region loader
var DEFAULT_LOADER_MESSAGE = "We're processing your request";
var FORM_LOADER_SELECTOR = '#d-form-loader';
var FORM_LOADER_ID = 'd-form-loader';
var FORM_LOADER = null;


//function adjustLoaderSize(selector) {

//	if (selector == null) selector = DEFAULT_CONTAINER_SELECTOR;

//	var container = $(selector);
//	var offset = container.offset();

//	if (offset == null) return;

//	SITE_LOADER.css({
//		top: offset.top + 'px',
//		left: offset.left + 'px',
//		width: container.width() + (selector == DEFAULT_CONTAINER_SELECTOR ? 30 : 0) + 'px',
//		height: container.height() + 'px'
//	});
//}

function showContainerLoader(selector, timeout) { 
	showFormLoader(selector, timeout);
}

function showLoader(timeout) {
	showFormLoader(DEFAULT_CONTAINER_SELECTOR, timeout);
}

function hideLoader(selector) {
	hideFormLoader(selector);
}

function showLoadingPanel(selector,text, timeout) {
	showFormLoader(selector, timeout, text);
}

function showFormLoader(selector, timeout,text) {
	//clear
	hideFormLoader(selector);
	if (!hasValue(text)) text = DEFAULT_LOADER_MESSAGE;
	var info = { info: text };

	var template = kendo.template($("#common-loader-template").html());

	var FORM_LOADER = $('<div />').attr({ id: FORM_LOADER_ID }).addClass('loader-mask').html(template(info));

	$(selector).append(FORM_LOADER);

	adjustFormLoaderSize(selector);

	$(selector).find(FORM_LOADER_SELECTOR).each(function(){ $(this).show().fadeIn(200); });

	if (timeout != null) setTimeout(function () {
		FORM_LOADER.hide();
	}, timeout);
}

function adjustFormLoaderSize(selector) {
	var container = $(selector);
	container.find(FORM_LOADER_SELECTOR).each(function() {
		$(this).css({
			top: '0',
			left: '0',
			width: container.width() + (selector == DEFAULT_CONTAINER_SELECTOR ? 30 : 0) + 'px',
			height: container.height() + 'px'
		});
	});
}

function hideFormLoader(selector) {
	if (selector) {
		$(selector).find(FORM_LOADER_SELECTOR).each(function(){ $(this).hide(); $(this).remove(); }); 
		return;
	}
	$(FORM_LOADER_SELECTOR).hide();
	$(FORM_LOADER_SELECTOR).remove();
}
//#endregion

//#region common
var endscript = "</script>";

function setReportScroll(selector) {
	$(selector).nanoScroller({ sliderMinHeight: 80, alwaysVisible: true });
}

function cleanWndContent(e) {
	e.sender.wrapper.find('.k-window-content').html("...");
}

function onWndClose(e) {
	e.sender.content(' ');
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

function isFxSupported() {
	var b = $.browser;
	return !b.msie || (b.msie && parseInt(b.version) > 9);
}

function isOldIE() {
	var myNav = navigator.userAgent.toLowerCase();
	if (myNav.indexOf('msie') != -1) {
		var v = parseInt(myNav.split('msie')[1]);
		return v < 10;
	}
	return false;
}
//#endregion

//#region messages
var accountMessages = {
	ACCOUNT: {
		SETTINGS_UPDATED: "Account settings updated successfully"
	   , COMM_SETTINGS_UPDATED: "Communication settings updated successfully"
	   , PAYOUT_SETTINGS_UPDATED: "Payout settings updated successfully"
	   ,NAME_REQUIRED: "Name or Nickname required"
	}
};
//#endregion

//#region focus events
function setFocusComboEvnt(e) {
	try {
		var input = $(e.sender._inputWrapper.find('input'));
		setFocusEvent(input);
	} catch (ex) {

	}
}
function setFocusAuEvnt(id) {
	try {
		var input = $('#' + id).data("kendoAutoComplete").wrapper.find('input');
		setFocusEvent(input);
	} catch (e) {

	}
}
function setFocusNumericEvnt(id) {
	try {
		var input = $('#' + id).data("kendoNumericTextBox").wrapper.find('input');
		setFocusEvent(input);
	} catch (e) {

	}
}
function setFocusEvent(input) {

	if (input.length == 0) return;

	input.focus(function () {
		var inp = $(this);
		setTimeout(function () {
			inp.select();
		});
	});
}
//#endregion

//#region grid services
function checkFilter(filter, gridId, objSelector) {
	var grid = $("#" + gridId).data(objSelector);

	if (grid == null) return;

	var $filter = grid.dataSource.filter() == undefined ? [] : grid.dataSource.filter().filters;

	if ($filter.length == 0) return;

	for (var i = 0; i < $filter.length; i++) {

		if ($filter[i].logic == undefined) {
			if ($filter[i].field == filter.field) {
				$filter.splice(i, 1);
				grid.dataSource.filter($filter);
				return;
			}
		} else {

			var field = filter.filters == undefined ? filter.field : filter.filters[0].field;

			if ($filter[i].filters[0].field ==  field) {
				$filter.splice(i, 1);
				grid.dataSource.filter($filter);
				return;
			}
		}
	}
}

function removeFilter(field, gridId, objSelector) {
	var grid = $("#" + gridId).data(objSelector);

	if (grid == null) return;

	var $filter = grid.dataSource.filter() == undefined ? [] : grid.dataSource.filter().filters;

	if ($filter.length == 0) return;

	for (var i = 0; i < $filter.length; i++) {
		if ($filter[i].field == field) {
			$filter.splice(i, 1);
			grid.dataSource.filter($filter);
			//return;
		}
	}
}

function filterReport(filter, gridId, objSelector) {
	var grid = $("#" + gridId).data(objSelector);
	if (grid == null) return;
	if (filter != null) {

		var $filter = grid.dataSource.filter() == undefined ? [] : grid.dataSource.filter().filters;

		$filter.push(filter);

		grid.dataSource.filter($filter);
	} else {
		grid.dataSource.filter([]);
	}
}

function filterGridByPeriod(gridId, dateFieldName, dates) {
	var filter = {
		logic: "and",
		filters: []
	};

	var filterExists = false;

	if (dates != null) {
		var from = dates.from;
		var to = dates.to;

		if (from) {
			var f = { field: dateFieldName, operator: "gte", value: from };
			filter.filters.push(f);
			filterExists = true;
		}
		if (to) {
			f = { field: dateFieldName, operator: "lte", value: to };
			filter.filters.push(f);
			filterExists = true;
		}
	}

	if (filterExists) {
		checkFilter(filter, gridId, "kendoGrid");
		filterReport(filter, gridId, "kendoGrid");
	} else {
		removeFilter(dateFieldName, gridId, "kendoGrid");
		filterReport(null, gridId, "kendoGrid");
	}
}

function startChange() {
	var endPicker = $("#end").data("kendoDatePicker"),
		startDate = this.value();

	if (startDate) {
		startDate = new Date(startDate);
		startDate.setDate(startDate.getDate() + 1);
		endPicker.min(startDate);
	}

	filterSalesRep();
}

function endChange() {
	var startPicker = $("#start").data("kendoDatePicker"),
		endDate = this.value();

	if (endDate) {
		endDate = new Date(endDate);
		endDate.setDate(endDate.getDate() - 1);
		startPicker.max(endDate);
	}

	filterSalesRep();
}

function filterSalesRep() {
	var filterExists = false;

	var filter = {
		logic: "and",
		filters: []
	};

	var from = $("#start").data("kendoDatePicker").value();
	var to = $("#end").data("kendoDatePicker").value();

	if (from) {
		var f = { field: "TrxDate", operator: "gte", value: from };
		filter.filters.push(f);
		filterExists = true;
	}
	if (to) {
		f = { field: "TrxDate", operator: "lte", value: to };
		filter.filters.push(f);
		filterExists = true;
	}

	if (filterExists) {
		checkFilter(filter, window.gridId, "kendoGrid");
		filterReport(filter, window.gridId, "kendoGrid");
	} else {
		removeFilter("TrxDate", window.gridId, "kendoGrid");
		filterReport(null, window.gridId, "kendoGrid");
	}
}

function clearSalesRepFilters() {
	var start = $("#start").data("kendoDatePicker");
	start.value(null);
	start.min(new Date(2012, 1, 1));

	var d = new Date();
	d.setDate(d.getDate() - 1);

	start.max(d);

	var end = $("#end").data("kendoDatePicker");
	end.value(null);
	end.min(new Date());

	filterSalesRep();
}
//#endregion

//#region field validations
function validateNumeric(event) {
	var theEvent = event || window.event;
	var key = theEvent.keyCode || theEvent.which;
	key = String.fromCharCode(key);
	if (event.ctrlKey === true && key === "v") return;
	var regex = /[0-9]|\./;
	if (!regex.test(key) && event.keyCode != 8) {
		theEvent.returnValue = false;
		if (theEvent.preventDefault) theEvent.preventDefault();
	}
}

function validateAlphaNumeric(event) {
	var theEvent = event || window.event;
	var key = theEvent.keyCode || theEvent.which;
	key = String.fromCharCode(key);
	var regex = /[a-zA-Z0-9]|\./;
	if (!regex.test(key) && event.keyCode != 8) {
		theEvent.returnValue = false;
		if (theEvent.preventDefault) theEvent.preventDefault();
	}
}
function IsAlphaNumeric(strValue) {
	var regex = /^[a-z0-9]+\.[a-z0-9]+$/i;
	return regex.test(strValue);
}

//#endregion

//#region user notifications
var headNotificationManager;
var NOTIFICATION_REFRESH_TIMEOUT = 1000 * 60 * 10;

var headNotifEvents = {
	notif: {
		notifyNewMsg: 'notify/NewMsg',
		userNotifBound: 'userNotif/Bound'
	}
};

function getHeadNotifManagerInstance() {
	if (headNotificationManager == undefined || headNotificationManager == null) {
		headNotificationManager = new NotificationManager();
	}
	return headNotificationManager;
}
//#endregion

//#region notification manager
var NotificationManager = kendo.Class.extend({
	queue: [],

	init: function () {
		var that = this;
		that.queue = [];
	},

	subscribe: function (eventName, callback, context) {
		var that = this;
		if (!that.queue[eventName]) {
			that.queue[eventName] = [];
		}
		that.queue[eventName].push({
			callback: callback,
			context: context
		});
	},

	unsubscribe: function (eventName, callback, context) {
		var that = this;

		if (that.queue[eventName]) {
			that.queue[eventName].pop({
				callback: callback,
				context: context
			});
		}
	},

	notify: function (eventName, data) {
		var that = this;
		var context, intervalId, idx = 0;

		if (that.queue[eventName]) {
			intervalId = setInterval(function () {
				if (that.queue[eventName][idx]) {
					try {
						context = that.queue[eventName][idx].context || this;
						that.queue[eventName][idx].callback.call(context, data);
					} catch (e) {
					}

					idx += 1;
				} else {
					clearInterval(intervalId);
				}
			}, 0);

		}
	}
});

var USER_NOTIFY_TIMEOUT = 15;

var NotificationKinds = {
	Info: 'info'
	, Error: 'error'
	, Success: 'success'
	, Attention: 'attention'
};

var UserNotificationManager = kendo.Class.extend({

	container: null,
	label: null,

	init: function (containerSelector, boxSelector, subscribe) {
		var that = this;

		that.container = $(containerSelector);

		that.label = that.container.find(boxSelector);

		if (subscribe) window.getNotifManagerInstance().subscribe(notifEvents.userNotification.show, that.show, that);
	},

	clear: function () {
		var that = this;
		that.resetAlertBox();
		that.label.html('').show();
	},

	resetAlertBox: function () {
		var that = this;

		that.label.removeClass('info').removeClass('error').removeClass('success');
	},

	show: function (token) {
		var that = this;

		that.resetAlertBox();

		if (token.message == null || token.message.length == 0) {
			that.label.html('').css('visibility','hidden');
			return;
		}

		that.label.html(token.message).addClass(token.kind.toLowerCase()).css('visibility','visible');

		setTimeout(function () {
			that.clear();
		}, 1000 * USER_NOTIFY_TIMEOUT);
	}
});
//#endregion
