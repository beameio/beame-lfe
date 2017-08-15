var QUEST_LIST_SELECTOR = '#QuizQuestions';
var newQuestId = null;


function setEditMode() {
	window.isFormInEditMode = true;
}
function exitEditMode() {
	window.isFormInEditMode = false;
}
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
function notifyQuestionChange() {
	window.getNotifManagerInstance().notify(notifEvents.quiz.questionStateChanged, null);
}
function readQuestions() {
	$(QUEST_LIST_SELECTOR).data('kendoListView').dataSource.read();
}
function closeQuizEditWnd() {
	$(window.WND_QUIZ_MANAGE_SELECTOR).data("kendoWindow").close();
}
function showAttachWarning() {
	swal({
		title: "This quiz is already attached to this course's contents.",
		text: "To edit this quiz please remove it from the course contents",
		type: "warning"
	});
}

function onQuestionRemoved(e) {
	var msg = window.quizStatus == 1 ? 'Are You sure?' : 'Quiz is already published. Are You sure?';
	if (!window.confirm(msg)) {
		e.preventDefault();
	}
}
function onQuestionListBound(e) {
 
	function loadQuestDetails(btn,url) {
		var qid = btn.attr('data-val');
		btn.unbind('click').click(function (ev) {
			ev.preventDefault();
			collapseQuestions();
			toggleQuestionEditForm(url, qid);
		});
	}

	setReportScroll(CONTENT_CONTAINER_SELECTOR);

	$.each(e.sender.wrapper.find('.btn-edit'), function () {        
		loadQuestDetails($(this), window.questEditUrl);       
	});

	$.each(e.sender.wrapper.find('.btn-add-answer'), function () {        
		loadQuestDetails($(this), window.answerManageUrl);
	});

	$.each(e.sender.wrapper.find('.btn-expand-collapse'), function () {
		var btn = $(this);

		btn.on('click', function (ev) {
			ev.preventDefault();
			var qid = btn.attr('data-val');
			if (btn.find('i').hasClass('expanded')) {
				btn.find('i').removeClass('expanded');
				collapseQuestion(qid);
			} else {
				toggleQuestionEditForm(window.answerManageUrl, qid);
				
			}
		});

	});
	
	$.each(e.sender.wrapper.find('.q-container'), function () {
		var row = $(this);
		row.on('click', function () {
			row.siblings('.btn-expand-collapse').click();
		});
	});

	setListSortable(QUEST_LIST_SELECTOR, window.saveQuestOrderUrl, onQOrderSaved, 'data-val');
	adjustQuestionScroll();

	if (newQuestId == null) return;

	e.sender.wrapper.find('.btn-add-answer[data-val=' + newQuestId + ']').click();

	newQuestId = null;
}

function toggleQuestionEditForm(url,questId) {

	newQuestId = null;

	var lv = $(QUEST_LIST_SELECTOR).data('kendoListView');

	var v = lv.wrapper.find('#q-detail-' + questId);

	lv.wrapper.find('.btn-collapse-first[data-val=' + questId + '] > i').addClass('expanded');
	lv.wrapper.find('li[data-val=' + questId + ']').addClass('active');

	
	v.slideToggle("slow", function () {
		actionFormResultWithCallback(url, { id: questId, qid: window.currentQuizId, sid: window.currentQuizSId }, '#q-detail-' + questId, CONTENT_CONTAINER_SELECTOR, onQuestEdit);
	});
	
}

function onQuestEdit() {
	try {
		var editor = $('#frm-edit-quiz-quest').find('#QuestionText');
		editor.data("kendoEditor").focus();
	} catch (e) {

	}
	adjustQuestionScroll();

	setTimeout(function() {
		$(CONTENT_CONTAINER_SELECTOR).nanoScroller({ scroll: 'bottom' });
	}, 500);
}

function expandQuestions() {
	collapseQuestions();
	$.each($(QUEST_LIST_SELECTOR).find('.btn-collapse-first'), function () {
	   $(this).click();
	});
}

function collapseQuestions() {
	
	$.each($(QUEST_LIST_SELECTOR).find('.btn-collapse-first'), function () {
		var id = $(this).attr('data-val');
		collapseQuestion(id);

	});
}

function collapseQuestion(questId) {
		$(QUEST_LIST_SELECTOR).find('li[data-val=' + questId + ']').removeClass("active");

		var btn = $(QUEST_LIST_SELECTOR).find('.btn-collapse-first[data-val=' + questId + ']');

		btn.find('i').removeClass('expanded');

		var detail = btn.parent().siblings('.first-contents');

		detail.empty();

		var isVisible = detail.is(":visible");

		if (isVisible) {
			detail.slideUp(500, adjustQuestionScroll);
		} else {
			adjustQuestionScroll();
		}
}


function initNewQuest(quizId, sid) {
	handleEditMode();

	var lv = $(QUEST_LIST_SELECTOR).data('kendoListView');

	//check if new item  is in edit mode
	if(lv.dataSource.get(-1) != undefined) return;

	lv.cancel();

	var token = {
		QuizId: quizId
		, QuizSid: sid
		, QuestionId: -1
		, QuestionText: 'New Question'
		, Type: 'American'
		, IsActive: true
		, Index: lv.dataSource._data.length
	};

	lv.dataSource.add(token);
	toggleQuestionEditForm(window.questEditUrl, -1);
}


function initNewAnswer(questId) {
	handleEditMode();

	//expandCollapseChapters(2);

	var lv = $('#AnswersListView-' + questId).data('kendoListView');

	//check if new item  is in edit mode
	if (lv.dataSource.get(-1) != undefined) return;

	lv.cancel();

	if (lv.dataSource._data.length >= 5) {
		swal(
		   {
			   title: "Maximum 5 answer options allowed",
			   //text: "Quiz saved",
			   type: "warning"
		   });
		return;
	}

   
	var token = {
		 QuestionId: questId
		, OptionId: -1
		, OptionText: ""
		, IsActive: true
		, IsCorrect:false
		, Index: lv.dataSource._data.length
	};

	lv.dataSource.add(token);

	setTimeout(function () {
		lv.edit(lv.element.children().last());       
	}, 100);
}

function adjustQuestionScroll() {
	setTimeout(function() {
		setReportScroll(CONTENT_CONTAINER_SELECTOR);
	}, 300);

}

function validateAnswerForm() {
	var form = $(this);
	var ot = form.find('#OptionText').data('kendoEditor').value();
	if (ot != null && ot.length > 0) return true;

	swal(
		   {
			   title: "Answer option required",
			   type: "warning",
			   timer: 2000
		   }, function () {
			   form.find('#OptionText').data("kendoEditor").focus();
		   });
	form.find('#OptionText').data("kendoEditor").focus();

	return false;
}
function onAnswerEdit(e) {
	var model = e.model;
	var btn = e.item.find('.l-save-button');
	btn.unbind('click').bind('click', function () {
		btn.parent().parent().submit();
	});

	var deleteBtn = e.item.find('.l-f-delete-button');

	deleteBtn.die('click').live('click', function() {
		deleteAnswer(model.OptionId, "AnswersListView-" + model.QuestionId);
	});

	try {
		var editor = e.item.find('#OptionText');
		editor.data("kendoEditor").focus();
	} catch (er) {
		console.log(er);
	}

	adjustQuestionScroll();

	setTimeout(function () {
		$(CONTENT_CONTAINER_SELECTOR).nanoScroller({ scroll: 'bottom' });
	}, 500);
}
function onAnswerListBound(e) {
	var listId = e.sender.wrapper.attr('id');
	setListSortable('#' + listId, window.saveAnswerOrderUrl, onAOrderSaved, 'data-val');

	//bind is correct check-boxes
	$.each(e.sender.wrapper.find('input[type=checkbox]'), function () {
		var chk = $(this);
		var id = chk.attr('data-val');

		chk.live('change', function() {
			var checked = $(this).is(':checked');
			showLoader();
			var data = { id: id,isCorrect:checked };
			$.ajax({
				url: window.updateAnswerCorrectUrl,
				cache: false,
				type: "Post",
				data: JSON.stringify(data),
				datatype: "json",
				contentType: "application/json; charset=utf-8"
				, success: function (response) {
					hideLoader();
					if (response.success) {
						swal(
						{
							title: "Success",
							text: "Answer updated",
							type: "success",
							timer: 1000
						});
						$('#' + listId).data('kendoListView').dataSource.read();
					} else {
						sweetAlert("Oops...", response.error, "error");
					}
				}
			});
		});
	});
	//bind delete button
	$.each(e.sender.wrapper.find('.l-delete-button'), function () {
																	var deleteBtn = $(this);
																	var id = deleteBtn.attr('data-val');

																	deleteBtn.live('click', function() {
																		deleteAnswer(id, listId);
																	});
															});
}

function deleteAnswer(id, listId) {

	swal(
		{
			title: "Are you sure?",
			text: "This Answer will be deleted",
			type: "warning",
			showCancelButton: true,
			confirmButtonText: "Yes",
			closeOnConfirm: false
		}, function () {
			showLoader();
			var data = { id: id };
			$.ajax({
				url: window.deleteAnswerUrl,
				cache: false,
				type: "Post",
				data: JSON.stringify(data),
				datatype: "json",
				contentType: "application/json; charset=utf-8"
				, success: function (response) {
					hideLoader();
					if (response.success) {
						swal(
						{
							title: "Success",
							text: "Answer deleted",
							type: "success",
							timer: 1000
						});
						$('#' + listId).data('kendoListView').dataSource.read();
					} else {
						sweetAlert("Oops...", response.error, "error");
					}
				}
			});
	});

	
}



//function onAnswerDeleting() {
//	var form = $(this);
//	var quid = form.attr('data-val');

//	swal(
//		{
//			title: "Are you sure?",
//			text: "This Answer will be deleted",
//			type: "warning",
//			showCancelButton: true,
//			confirmButtonText: "Yes",
//			closeOnConfirm: false
//		}, function () {

//			deleteAnswer(id, "#AnswersListView-" + quid);

//			//showLoader();
//			//var data = { id: id };
//			//$.ajax({
//			//	url: window.deleteAnswerUrl,
//			//	cache: false,
//			//	type: "Post",
//			//	data: JSON.stringify(data),
//			//	datatype: "json",
//			//	contentType: "application/json; charset=utf-8"
//			//	, success: function (response) {
//			//		hideLoader();
//			//		if (response.success) {
//			//			swal(
//			//			{
//			//				title: "Success",
//			//				text: "Answer deleted",
//			//				type: "success",
//			//				timer: 1000
//			//			});
//			//			$("#AnswersListView-" +quid).data('kendoListView').dataSource.read();
//			//		} else {
//			//			sweetAlert("Oops...", response.error, "error");
//			//		}
//			//	}
//			//});
//		});

//	return false;
//}

function onAOrderSaved(response) {
	if (response.success) {
		swal(
		{
			title: "Success",
			text: "Answers reordered",
			type: "success",
			timer: 1000
		}, function () {
		});

	} else {
		sweetAlert("Oops...", response.error, "error");
	}
}

function onQOrderSaved(response) {
	if (response.success) {
		swal(
		{
			title: "Success",
			text: "Question reordered",
			type: "success",
			timer: 1000
		}, function () {
		});

	} else {
		sweetAlert("Oops...", response.error, "error");
	}
}

function onAnswerSaved(response) {
	hideLoader();
	if (response.success) {
		swal(
		{
			title: "Success",
			text: "Answer saved",
			type: "success",
			timer: 1000
		});
		$("#AnswersListView-" + response.result).data('kendoListView').dataSource.read();

	} else {
		sweetAlert("Oops...", response.error, "error");
	}
}