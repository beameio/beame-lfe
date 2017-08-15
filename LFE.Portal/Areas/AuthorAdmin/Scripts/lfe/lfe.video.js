var USER_VIDEOS_REFRESH_INTERVAL;

function onVideosBound(e) {
    var ds = e.sender.dataSource;
    var total = e.sender.dataSource._data.length;
    window.getNotifManagerInstance().notify(notifEvents.video.videosLoaded, total);

    var elements = e.sender.wrapper.find('li');

    var waiting = false;

    $.each(ds._data, function () {
        var video = this;

        if (!hasValue(video.videoUrl)) {
            waiting = true;
        }

        var $this = e.sender.wrapper.find('li[data-uid=' + video.uid + ']');

        var id = $this.attr("data-val");
        var uid = $this.attr("data-uid");
        var li = $this;

        if (id >= 0) {

            var placeholder = $this.find('#player-' + id);
            var thumb = $this.find('#thumb-' + id);
            var hint = $this.find('#hint-' + id);
            var btn = hint.find('#play-' + id);

            btn.bind("click", function() {
                thumb.hide();
                hint.hide();
                placeholder.show();
                var playerInstance = jwplayer('player-' + id);
                playerInstance.setup({
                    file: video.videoUrl,
                    image: video.thumbUrl,
                    autostart: true,
                    width: window.BC_PLAEYR_W,
                    height: window.BC_PLAEYR_H,
                    skin: '/Scripts/jwplayer/jwplayer-skins-premium/vapor.xml'
                });               
            });

            thumb.hover(
                function() {
                    hint.show();
                }
            );
            hint.hover(
                function() {
                },
                function() {
                    hint.hide();
                }
            );
        }

        var row = ds.getByUid(uid);
        if (row != undefined) {
            if (parseInt(row.uses) > 0) {
                li.find(".cmd > .rem").remove();
            }
        }
        
    });

    
    if (USER_VIDEOS_REFRESH_INTERVAL) clearInterval(USER_VIDEOS_REFRESH_INTERVAL);

    if (waiting) {       
        USER_VIDEOS_REFRESH_INTERVAL = setInterval(function() {
            ds.read();
        }, 1000 * 100);
    } 

    //var grid = e.sender;
    //var data = grid.dataSource.data();

    //$.each(data, function (i, row) {
    //    if (row.uses > 0)
    //        $('tr[data-uid="' + row.uid + '"] ').find(".cmd > .rem").hide();
    //});
}


function onVideosGridBound(e) {
    var grid = e.sender;
    var data = grid.dataSource.data();

    $.each(data, function (i, row) {
        if (parseInt(row.uses) > 0) {
            $('tr[data-uid="' + row.uid + '"] ').find(".k-grid-delete").remove();
        }
    });

    var total = e.sender.dataSource._data.length;
    window.getNotifManagerInstance().notify(notifEvents.video.videosLoaded, total);
}