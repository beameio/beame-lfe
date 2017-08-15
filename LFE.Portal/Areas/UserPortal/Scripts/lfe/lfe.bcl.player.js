var playerActions = {
    STOP: 'stop',
    PAUSE: 'pause',
    PLAY:'play'
};

var BclPlayer = kendo.Class.extend({
    playerData: {
        "objectId":"bc-player-1775581216001",
        "playerID": "1775581216001",
        "playerKey": "AQ~~,AAABm0drRPk~,DiQRmh9VgVWZubcHWonC2cZbcR-19kzC",
        "width": "600",
        "height": "450",
        "videoID": null        
    },
    "isPlayerAdded": false,
    "isAutoPlay": false,
    currentToken: null,
    videoContainer: null,    
    videosList: [],
    initialSettings:null,
    playerTemplate : "<div style=\"display:none\"></div>" +
                     "<object id=\"{{objectId}}\" class=\"BrightcoveExperience\">" +
                         "<param name=\"bgcolor\" value=\"#fff\" />" +
                         "<param name=\"width\" value=\"{{width}}\" />" +
                         "<param name=\"height\" value=\"{{height}}\" />" +
                         "<param name=\"playerID\" value=\"{{playerID}}\" />" +
                         "<param name=\"playerKey\" value=\"{{playerKey}}\" />" +
                         "<param name=\"isVid\" value=\"true\" />" +
                         "<param name=\"isUI\" value=\"true\" />" +
                         "<param name=\"wmode\" value=\"transparent\" />" +
                         "<param name=\"dynamicStreaming\" value=\"true\" />" +
                         "<param name=\"@videoPlayer\" value=\"{{videoID}}\" />" +
                         "<param name=\"templateLoadHandler\" value=\"bcLfePlayer.onTemplateLoaded\" />" +
                         "<param name=\"templateReadyHandler\" value=\"bcLfePlayer.onTemplateReadyHandler\" />" +
                     "</object>",

    init: function (settings) {
        var $that = this;
        $that.initialSettings = settings;

        $that._extendSettings();

      
    },

    _extendSettings: function () {
        var $that = this;

        var settings = $that.initialSettings;
        
        $that.videoContainer = $(settings.containerSelector);

        if ($that.videoContainer.length == 0) return;

        $that.playerData.width = settings.playerW;
        $that.playerData.height = settings.playerH;

        $that.isAutoPlay = settings.isAutoPlay;
        $that.videosList = settings.videosList;
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
    },

    loadVideo: function (token) {
        var $that = this;
        $that.currentToken = token;

        if ($that.videoContainer == null || $that.videoContainer == undefined || $that.videoContainer.length == 0) $that._extendSettings();
        
        if ($that.videoContainer == null || $that.videoContainer == undefined || $that.videoContainer.length==0) return;

       // window.getNotifManagerInstance().notify(notifEvents.video.videoChanged, $that.currentToken);
        
        
        if ($that.isPlayerAdded == false || $that.videoPlayer==undefined) {
            $that.isPlayerAdded = true;
            $that.playerData.videoID = token.bcId;
            
           var playerHTML = $that.markup($that.playerTemplate, $that.playerData);
            
            $that.videoContainer.html(playerHTML);

            //$that.videoContainer.fadeOut("slow", function () {
            //    $that.videoContainer.html(playerHTML).fadeIn("slow");
            //});
            
            window.brightcove.createExperiences();
        }            
        else {            
            $that.videoPlayer.loadVideo(token.bcId);
        }

    },    

    onTemplateLoaded: function () {
        var $that = window.getBcPlayerInstance();
        $that.player           = window.brightcove.getExperience($that.playerData.objectId);
        $that.modVP            = $that.player.getModule(window.APIModules.VIDEO_PLAYER);
        $that.experienceModule = $that.player.getModule(window.APIModules.EXPERIENCE);
        $that.videoPlayer      = $that.player.getModule(window.APIModules.VIDEO_PLAYER);
     
    },
    
    onTemplateReadyHandler: function () {
        var $that = window.getBcPlayerInstance();
        $that.modVP.addEventListener(window.BCMediaEvent.COMPLETE, $that.onMediaComplete);
        //$that.experienceModule.setSize(600, 450);
        $that.videoPlayer.play();
    },
    


    onMediaComplete: function () {
        var $that = window.getBcPlayerInstance();

        window.PublishFacebookStory(window.CURRENT_VIDEO_TOKEN);

        $that.isAutoPlay = $('#btn-autoplay').hasClass('on');

        var nextId = $that.currentToken.nextId;

        if (nextId < 0 || !$that.isAutoPlay) return;
        
        var next = $that.getVideoById(nextId);

        if (next == null || next.videoId == $that.currentToken.videoId) return;

        //$that.loadVideo(next);
        //window.getNotifManagerInstance().notify(notifEvents.video.videoSelected, next.videoId);
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

