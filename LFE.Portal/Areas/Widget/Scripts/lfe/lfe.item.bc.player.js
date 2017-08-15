var videoActionsReasons =
{
    Begin: 'Begin',
    Stop : 'Stop',
    Complete : 'Complete',
    Seek : 'Seek',
    Change : 'Change',
    WindowUnload: 'WindowUnload',
    Blur: 'Blur',
    Focus: 'Focus',
    Ajax :'Ajax'
}

var playerActions = {
    STOP: 'stop',
    PAUSE: 'pause',
    PLAY:'play'
};

var videoActions = {
    STOP: 'Stop',
    PROGRESS: 'Progress',
    PLAY: 'Play',
    SEEK:'Seek',
    BEGIN:"Begin",
    CHANGE:"Change",
    COMPLETE:"Complete"
};

var BclPlayer = kendo.Class.extend({
    "isPlayerAdded": false,
    "isAutoPlay": false,
    currentToken: null,
    videoContainer: null,
    videosList: [],
    initialSettings: null,  
    videoHub: null,
    lastSessionId: null,
    isMediaChanged: false,    
    videoState: {
        position: 0,
        isInProgress: false,
        sessionId: null,
        bcId: null,
        chapterId:null,
        action: null,
        endReason: null,
        startReason:null
    },

    init: function (settings) {
        var $that = this;

        //window.saveVideoEvents = true;

        $that.initialSettings = settings;

        $that._extendSettings();

        //window.getNotifManagerInstance().subscribe(notifEvents.window.ajaxComplete, $that.onPageStateChanged, $that);
       // window.getNotifManagerInstance().subscribe(notifEvents.window.windowUnload, $that.onPageStateChanged, $that);
    },

    _extendSettings: function () {
        var $that = this;

        var settings = $that.initialSettings;
        
        $that.videoContainer = $(settings.containerSelector);

        if ($that.videoContainer.length == 0) return;

        $that.isAutoPlay = settings.isAutoPlay;
        $that.videosList = settings.videosList;

        $that.resetVideoState();

    },

    resetVideoState: function () {
        var $that = window.getBcPlayerInstance();
        $that.lastSessionId = $that.videoState.sessionId;
        $that.videoState = {
            position: 0,
            isInProgress: false,
            sessionId: null,
            bcId: null,
            chapterId:null,
            action: null,
            endReason: null,
            startReason: null
        };
    },

    swithPlayerMode:function(action) {
        var $that = this;
        var tab = getCurrentTab();

        if (tab != tabsHashPrefix.CONTENT) return;

        if ($that.videoPlayer == undefined) return;
        
        try {
            switch (action) {
                case playerActions.STOP:
                    $that.videoPlayer.stop();
                    return;
                case playerActions.PAUSE:
                    $that.videoPlayer.pause();
                    return;
                case playerActions.PLAY:
                    $that.videoPlayer.play();
                    return;
            }
        } catch (ex) {
            if (window.console) console.log(ex);
        }
    },

    resetPlayer:function() {
        var $that = this;
        $that.isPlayerAdded = false;
        $that.videoContainer = null;

        if ($that.videoState.isInProgress) {
            $that.videoState.action = videoActions.STOP;
           // $that.savePlayerEvent();
        }
    },

    loadVideo: function (token) {
        var $that = this;
        $that.currentToken = token;

        if ($that.videoContainer == null || $that.videoContainer == undefined || $that.videoContainer.length == 0) $that._extendSettings();
        
        if ($that.videoContainer == null || $that.videoContainer == undefined || $that.videoContainer.length==0) return;

       // window.getNotifManagerInstance().notify(notifEvents.video.videoChanged, $that.currentToken);


        window.loadJWPlayer(token.bcId);
    },    


  

    onMediaComplete: function (e) {
       // console.log(e);
        var $that = window.getBcPlayerInstance();

       // window.PublishFacebookStory(window.CURRENT_VIDEO_TOKEN);

        //$that.savePlayerEvent(false);
        //setTimeout(function () {
        //    $that.updatePlayerEvent(true, $that.lastSessionId, videoActionsReasons.Complete, videoActions.STOP);
        //}, 2500);
        

        $that.isAutoPlay = $('#btn-autoplay').hasClass('on');

        var nextId = $that.currentToken.nextId;

        if (nextId < 0) {
            var notify = window.notifyLastComplete != null && window.notifyLastComplete != undefined ? window.notifyLastComplete : false;
            if (notify) {
                    $.ajax({
                        url: window.onCourseCompleteUrl,
                        cache: false,
                        async: true ,
                        type: "Post",                        
                        datatype: "json",
                        contentType: "application/json; charset=utf-8",
                        success: function () {}
                    });
            }
        }

        if (nextId < 0 || !$that.isAutoPlay) return;
        
        var next = $that.getVideoById(nextId);

        if (next == null || next.videoId == $that.currentToken.videoId) return;

       
        $('#' + BTN_SELECT_VIDEO_PREFIX + next.videoId).click();
    },
    
    getVideoById:function(id) {
        var $that = this;

        var json = $that.videosList;
        
        for (var i = 0; i < json.length; i++) {
            if (json[i].videoId == id) return json[i];
        }

        return null;
    },

    markup : function (html, data) {
        var m;
        var i = 0;
        var match = html.match(data instanceof Array ? /{{\d+}}/g : /{{\w+}}/g) || [];

        while (m = match[i++]) {
            html = html.replace(m, data[m.substr(2, m.length - 4)]);
        }
        return html;
    }
});
