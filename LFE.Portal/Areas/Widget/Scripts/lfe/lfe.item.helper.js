//#region price events
function bindRentComboEvents(id) {
    var ul = $('#' + id);

    $.each(ul.find('.price-container'),function() {
        var box = $(this);
        
        box.unbind('click').click(function() {
            handlePriceClickEvent(this, id);
        });
    });
}

function handlePriceClickEvent($this, id) {
    var current = $('#rent-selected').html();//.clone();
    var li = $($this).parent();
    $('#rent-selected').html($($this));
    li.html(current);
    bindRentComboEvents(id);

    $('#rent-selected > .price-container').unbind('click').click(notifyPriceSelection);
}

function notifyPriceSelection(e) {
    
    e.stopPropagation();
    var id = $(e.currentTarget).attr('data-target');

    window.getNotifManagerInstance().notify(notifEvents.item.priceSelected, id);
}
function onFreeItemPurchaceComplete(response) {
    hideLoader();
    if (response.success) {
        window.location.href = response.result.url;
    } else {
        alert(response.error);
    }

}
//#endregion

//#region tabs events
function initTabEvents() {


    function cleanTabNav() {
        var tabLinks = $('#pp-nav-tabs > li > a[data-toggle="tab"]');//document.getElementsByClassName('a-tab-nav');
        for (var i = 0; i < tabLinks.length; i++) {
            var h = tabLinks[i].href.substring(tabLinks[i].href.lastIndexOf('#'));
            if (h.lastIndexOf('?') >= 0) {
                h = h.substring(0, h.lastIndexOf('?'));
                tabLinks[i].setAttribute('href', h);
            }
        }

        $('#pp-nav-tabs > li > a[data-toggle="tab"]').on('shown.bs.tab', function (e) {

            var id = $(e.target).attr("href").substr(1);
            window.location.hash = id;

            var url = $(this).attr("data-url");

            if (!hasValue(url)) {
                onTanContentLoaded();
                return;
            }

            var target = $(e.target).attr("href"); // activated tab

            if ($(target).is(':empty')) {

                var pane = $(this);
                // showFormLoader('#tp-author');
                // ajax load from data-url
                $(target).load(url, function () {
                    //  hideFormLoader();
                    pane.tab('show');
                    onTanContentLoaded();
                });
            }
            else {
                onTanContentLoaded();
            }
        });

        var hash = window.location.hash;
        if (hasValue(hash)) {
            $('#pp-nav-tabs a[href="' + hash + '"]').tab('show');
        }
    }

    setTimeout(cleanTabNav, 200);

    
}

function onTanContentLoaded() {
    $("html, body").animate({ scrollTop: 0 }, 300);
    window.getNotifManagerInstance().notify(notifEvents.window.contentLoaded, null);
}

//#endregion

//#region sc iframe
function adjustscIframeH(contentDocument) {
    try {
        //$('#ifrm-sc').height(contentDocument.body.scrollHeight);

        $('#ifrm-sc').height(contentDocument.body.offsetHeight);

        $('#modSC .modal-body').height(Math.min($(window).height() - $('.modal-header').outerHeight() - 15, contentDocument.body.scrollHeight));
       // $('#modSC .modal-body').height($(window).height() - $('.modal-header').outerHeight() - 15);
        //  $('#ifrm-sc').iFrameResize();
    } catch (e) {
        console.log(e);
    }
}

function onScLoad($this) {
    hideFormLoader();

    adjustscIframeH($this);
}

function onWindowChanged() {
    var f = document.getElementById('ifrm-sc');
    //console.log(f.contentWindow.document);
    adjustscIframeH(f.contentWindow.document);    
}
//#endregion