function onCourseQuizzesBound(e) {
    $(".col-tooltip").kendoTooltip({
        position: "top",
        content: kendo.template($("#tp-template").html()),
        width: 250
    }).data("kendoTooltip");

    var grid = e.sender;
    var data = grid.dataSource.data();
    $.each(data, function (i, item) {
        if (item.Taken > 0) {
            $('tr[data-uid="' + item.uid + '"] ').find('.k-grid-delete').hide();//.addClass("disabled");
        }

        if (item.OpenRequests == 0) {
            $('tr[data-uid="' + item.uid + '"] ').find('.k-grid-Respond').hide();//.addClass("disabled");
        }
    });
}

function closeQuizWnd() {
    $(window.WND_QUIZ_MANAGE_SELECTOR).data("kendoWindow").close();
}

function onQuizRemoved(e) {
    if (e.model.Taken > 0) {
        alert("Quiz already taken by students and can't be deleted.");
        e.preventDefault();
    }
}



function rebindQuizRep() {
    $(window.QUIZ_REP_SELECTOR).data("kendoGrid").dataSource.read();
}

function onQuizEditWndClosed(e) {
    rebindQuizRep();
    e.sender.wrapper.find('.k-window-content').html("...");
}


function onQuizStudentsBound(e) {
    
    var grid = e.sender;
    var data = grid.dataSource.data();
    $.each(data, function (i, item) {
        if (!item.HasOpenRequest) {
            $('tr[data-uid="' + item.uid + '"] ').find('.k-grid-ResetAttempts').addClass("disabled");
        }

    });
}