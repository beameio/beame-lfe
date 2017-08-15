var isQuizInProgress = false;

function loadUserQuiz(action, qid, questId) {

    var data = {
        quizAction: action
        ,quizId: qid
        ,questId:questId
    };
    actionFormResultWithCallback(window.quizUrl, data, '#qz-user-inner', '.user-quiz-container', onUserQuizLoaded);  
}
function onUserQuizLoaded() {
    hideLoader();
    $('#bc-player-container').hide();
    $('.user-g2t-container').hide();
    $('.user-quiz-container').show();
}