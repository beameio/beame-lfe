var FORM_MANAGER_SELECTOR = "#frmWizMaqnager";
var STEP_CONTAINER_SELECTOR = ".step-container";
var CHANGE_CONFIRM = 'You have unsaved work. Are you sure you want to leave this screen?';

var isFormInEditMode;
//#region constants
var wizardStepsEnum = {
    //'Introduction': 0,
    'CourseName': 1    
    ,'VideoManager': 2
 //   ,'ChapterManage': 3
    , 'ChapterContents': 3
    , 'CourseVisuals': 4
    , 'CourseMeta': 5
    , 'AboutAuthor': 6
    ,'CoursePrice': 7
    ,'Publish': 8
};

var wizardSteps = [
    //  { name: 'Introduction', id: 0 },
      { name: 'CourseName', id: 1 }    
    , { name: 'VideoManager', id: 2 }
  //  , { name: 'ChapterManage', id: 3 }
    , { name: 'ChapterContents', id: 3 }
    , { name: 'CourseVisuals', id: 4 }
    , { name: 'CourseMeta', id: 5 }
    , { name: 'AboutAuthor', id: 6 }
    , { name: 'CoursePrice', id: 7 }
    , { name: 'Publish', id: 8 }
];

var stepModes = {
    Unknown : -1            
    ,Current : 1
    ,Allowed : 2
    ,Disable: 3
    ,Dummy : 4
};

var stepKinds = {
      next: 'next'
    , last: 'last'
    , current: 'current'
};
//#endregion


//#region steps services
function getStepIndex(step) {
    var steps = getObjects(wizardSteps, 'name', step);

    if (steps.length == 0) return null;

    return steps[0].id;
}

function onBreadcrumbLoad(e) {

   $.each(e.sender.dataSource._data, function () {
       var item = this;
       var step = item.Step;
        var li = $('#ulNavTree > li[data-uid=' + item.uid + ']');
        li.unbind('click');

        if (item.Mode == stepModes.Disable || item.Mode == stepModes.Unknown) {
            //  li.remove();
            li.css({ opacity: 0.5 });
        } else {
            if (item.Mode == stepModes.Allowed) {
                li.bind('click', function () {

                    if (!validateEditMode()) return;
                   
                    updateManagerStep(step,stepKinds.next);
                    changeStep();
                });
            }
        }
    });
   
}

function updateManagerStep(step,prop) {
    $(FORM_MANAGER_SELECTOR).find('#' + prop + 'Step').val(step);
}

function getManagerStep(prop) {
    return $(FORM_MANAGER_SELECTOR).find('#' + prop + 'Step').val();
}

function changeStep() {
    if (isFormInEditMode) {
        if(!CHANGE_CONFIRM) return;
    }
    
    $(FORM_MANAGER_SELECTOR).submit();
}
//#endregion

function onTxtBoxInEdit($this) {
    var tb = $($this);
    isFormInEditMode = hasValue(tb.val());
}

function setEditMode() {
    isFormInEditMode = true;
}
function exitEditMode() {
    isFormInEditMode = false;
}
function validateEditMode() {
    if (!isFormInEditMode) return true;

    if (!confirm(CHANGE_CONFIRM)) return false;

    window.isFormInEditMode = false;

    return true;
}

function onWizardVideoSelected(token) {
    window.isFormInEditMode = true;
    onVideoSelected(token);
}

function loadCurrentStep() {
    //$(STEP_CONTAINER_SELECTOR).fadeOut(300, function() {
    //    $(STEP_CONTAINER_SELECTOR).fadeIn(300,function() {
    //        $(STEP_CONTAINER_SELECTOR).html(view);
    //    });
    //});
    //$(STEP_CONTAINER_SELECTOR).empty();
    //$(STEP_CONTAINER_SELECTOR).html(view);
    //reset chapter
    $(FORM_MANAGER_SELECTOR).find('#selectedChapterId').val(null);
    window.hideLoader();
}

//#region helper
function handleHash(h, current) {
    try {

        var steps = getObjects(wizardSteps, 'id', h);

        if (steps.length == 0) return false;

        var obj = steps[0];

        var id = Number(obj.id);
        var step = obj.name;

        if (current == id || current == step) return false;//don't reload current

        var last = $('#lastStep').val();

        var lasts = getObjects(wizardSteps, 'name', last);

        if (lasts.length == 0) return false;

        var lastId = Number(lasts[0].id);

        if (id > lastId + 1) return false;

        updateManagerStep(step, stepKinds.next);
        changeStep();
        return true;

    } catch (e) {
        return false;
    }
}

function getObjects(obj, key, val) {
    var objects = [];
    for (var i in obj) {
        if (!obj.hasOwnProperty(i)) continue;
        if (typeof obj[i] == 'object') {
            objects = objects.concat(getObjects(obj[i], key, val));
        }
        else if (i == key && obj[key] == val) {
            objects.push(obj);
        }
    }
    return objects;
}
//#endregion

//#region
var WizardNotificationManager = kendo.Class.extend({

    container: null,
    label: null,

    init: function () {},

    show: function (token) {
       
        if (token.message == null || token.message.length == 0) return;

        var cont = $('#wiz-video-info');

        var isVideo = cont.length > 0;

        switch (token.kind) {
            case NotificationKinds.Success:
                if (!isVideo) return;
                var label = cont.find('.alert');
                label.html(token.message).addClass(token.kind.toLowerCase()).show();
                return;
            case NotificationKinds.Info:
                if (!isVideo) alert(token.message);
                return;
            case NotificationKinds.Error:
                alert(token.message);
                return;
        }
    }
});
//#endregion