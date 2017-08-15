var text_max = 256;

var MsgInputManager = kendo.Class.extend({
    initialSettings: null,
    notifManager: null,
    mainList: $('#RoomMessages'),
    childList:null,

    init:function(settings) {
        var $that = this;
        $that.initialSettings = settings;
        $that.configTextArea();
        $that.notifManager = new UserNotificationManager('#' + $that.initialSettings.alertId, '.alert', false);
        if (!hasValue($that.initialSettings.parentId)) return;
        $that.childList = $('#ul-replay-' + $that.initialSettings.parentId);
    },
    
    configTextArea: function () {
        var $that = this;
        var settings = $that.initialSettings;
        var input = $('#' + settings.msgInputId);

        input.autosize();

        input.tagmate({
            exprs: {
                "@": Tagmate.NAME_TAG_EXPR,
                "#": Tagmate.HASH_TAG_EXPR
            },
            sources: {
                "@": function (request, response) {
                    var dataSource = new kendo.data.DataSource({
                        transport: {
                            read: {
                                url: window.findUrl,
                                dataType: "json",
                                data: {
                                    q: function () {
                                        return request.term;
                                    }
                                }
                            }
                        },
                        change: function () { // subscribe to the CHANGE event of the data source

                            var array = $.map(this._data, function (item) {
                                return [{
                                    value: item.value
                                    , label: item.label
                                     , image: item.image
                                }];
                            });

                            var filtered = array;// Tagmate.filterOptions(this.view(), request.term);
                            response(filtered);
                        }
                    });

                    // read data from the remote service
                    dataSource.read();


                }
            },
            //capture_tag: function (tag) {
            //    console.log("Got tag: " + tag);
            //},
            //replace_tag: function (tag, value) {
            //    console.log("Replaced tag: " + tag + " with: " + value);
            //},
            highlight_tags: true,
            highlight_class: "highlighter",
            menu_class: "menu",
            menu_option_class: "option",
            menu_option_active_class: "au-active"
        });

        function setRemaining(e) {
            try {
                var textLength = input.val().length;
                var textRemaining = text_max - textLength;
                $('#' + settings.charsRemainId).html(textRemaining + ' chars remaining');
                if (textRemaining == 0) {
                    e.preventDefault();
                    return false;
                }
                return true;
            } catch (ex) {
                console.log(ex);
                return false;
            }
        }

        input.keyup(function (e) {
            setRemaining(e);
        });

        input.change(function (e) {
            setRemaining(e);
        });

        

        input.keydown(function (e) {
            if (e.keyCode == 13 && !window.userSelected) {
                e.preventDefault();
                $('#' + settings.btnId).click();
            }
        });

        $('#' + settings.btnId).unbind('click').bind('click', function () {
            $that.submitUserMessage();
        });
    },
    
    submitUserMessage: function () {
        var $that = this;
        var settings = $that.initialSettings;
        var form = $('#' + settings.formId);
        var input = $('#' + settings.msgInputId);
        var isMsgValid = form.validate().element(input);

        if (!isMsgValid) {

            $that.notifManager.show({ message: userMessages.INVALID_MESSAGE, kind: NotificationKinds.Error });
            return;
        }

        var tags = input.getTags({
            "#": Tagmate.HASH_TAG_EXPR
        });
        var names = input.getTags({
            "@": Tagmate.NAME_TAG_EXPR
        });

        form.find('#NamesArrayStr').val(JSON.stringify(names));
        form.find('#TagsArrayStr').val(JSON.stringify(tags));

        window.showFormLoader('#' + settings.formId);
    
        var url = form.attr('action');
        var formData = form.serialize();
        window.showLoader(1000);
        $.post(url, formData, function (response) {
            window.hideFormLoader();
            $that.onMessageSaved(response);
        });
    },
    
    onMessageSaved: function (response) {
        var $that = this;        
        var settings = $that.initialSettings;
        var input = $('#' + settings.msgInputId);
        
        window.hideFormLoader();
        var kind = response.success ? NotificationKinds.Success : NotificationKinds.Error;
        var msg = response.success ? userMessages.MESSAGE_SAVE : response.error;

        $that.notifManager.show({ message: msg, kind: kind });

        if (response.success) {
            input.val(null);
            //$('.tagmate-container > pre').empty();
            input.siblings('pre').empty();
            $('#' + settings.charsRemainId).html(text_max + ' chars remaining');

            if (response.result == null) return;
            var li = $('<li />').attr({ 'data-val': response.result.MessageId }).addClass(hasValue(settings.parentId) ? '': 'li-topic');

            var template = hasValue(settings.parentId) ? kendo.template($("#replay-msg-list-template").html()) : kendo.template($("#msg-list-template").html());

            li.html(template(response.result));
            
            if (hasValue(settings.parentId )) {
                $that.childList.append(li);
                window.cancelReplay(settings.parentId);
            } else {
                $('#RoomMessages').prepend(li);
            }

            setTimeout(function() { $("#d-room-msg-container").nanoScroller(); }, 20);
        }
    }
});

//function configMessageTextArea(settings) {

//    var input = $('#' + settings.msgInputId);

//    input.autosize();

//    input.tagmate({
//        exprs: {
//            "@": Tagmate.NAME_TAG_EXPR,
//            "#": Tagmate.HASH_TAG_EXPR
//        },
//        sources: {
//            "@": function (request, response) {
//                var dataSource = new kendo.data.DataSource({
//                    transport: {
//                        read: {
//                            url: window.findUrl,
//                            dataType: "json",
//                            data: { 
//                                q: function () {
//                                    return request.term;
//                                }
//                            }
//                        }
//                    },
//                    change: function () { // subscribe to the CHANGE event of the data source
                        
//                        var array = $.map(this._data, function (item) {
//                            return [{
//                                 value: item.value
//                                , label: item.label
//                                 ,image:item.image
//                            }];
//                        });

//                        var filtered = array;// Tagmate.filterOptions(this.view(), request.term);
//                        response(filtered);
//                    }
//                });

//                // read data from the remote service
//                dataSource.read();
                
                
//            }           
//        },
//        //capture_tag: function (tag) {
//        //    console.log("Got tag: " + tag);
//        //},
//        //replace_tag: function (tag, value) {
//        //    console.log("Replaced tag: " + tag + " with: " + value);
//        //},
//        highlight_tags: true,
//        highlight_class: "highlighter",
//        menu_class: "menu",
//        menu_option_class: "option",
//        menu_option_active_class: "au-active"
//    });
    
//    input.keyup(function (e) {
//        setRemaining(e);
//    });

//    input.change(function (e) {
//        setRemaining(e);
//    });

//    function setRemaining(e) {
//        var textLength =input.val().length;
//        var textRemaining = text_max - textLength;
//        $('#'+settings.charsRemainId).html(textRemaining + ' chars remaining');
//        if (textRemaining == 0) {
//            e.preventDefault();
//            return false;
//        }
//       return true;
//    }

//    input.keydown(function (e) {
//        if (e.keyCode == 13 && !window.userSelected) {
//            e.preventDefault();
//            $('#' + settings.btnId).click();
//        }
//    });
    
//    $('#'+settings.btnId).unbind('click').bind('click', function () {
//        submitUserMessage(settings.formId, settings.msgInputId);
//    });
      
//}

//function submitUserMessage(formId,inputId) {
//    var form = $('#' + formId);
//    var isMsgValid = form.validate().element($('#'+ inputId));

//    if (!isMsgValid) {

//        window.formUserNotifManager.show({ message: userMessages.INVALID_MESSAGE, kind: NotificationKinds.Error });
//        return;
//    }

//    var tags = $("#"+ inputId).getTags({
//        "#": Tagmate.HASH_TAG_EXPR
//    });
//    var names = $('#' + inputId).getTags({
//        "@": Tagmate.NAME_TAG_EXPR
//    });

//    form.find('#NamesArrayStr').val(JSON.stringify(names));
//    form.find('#TagsArrayStr').val(JSON.stringify(tags));

//    window.showFormLoader('#' + formId);
    
//    var url = form.attr('action');
//    var formData = form.serialize();
//    window.showLoader(1000);
//    $.post(url, formData, function (response) {
//        window.hideFormLoader();
        
//    });
//}

//function onMessageSaved(response) {
//    window.hideFormLoader();
//    var kind = response.success ? NotificationKinds.Success : NotificationKinds.Error;
//    var msg = response.success ? userMessages.MESSAGE_SAVE : response.error;

//    window.formUserNotifManager.show({ message: msg, kind: kind });

//    if (response.success) {
//        $('#UserMessage').val(null);
//        $('.tagmate-container > pre').empty();
//        $('#cnt-rem-char').html(text_max + ' chars remaining');
//        window.getNotifManagerInstance().notify(notifEvents.disqus.rebindFeed, null);
//    }
//}

function showFeedByParam(view) {

    if (window.flipSupported) {        
        $(window.FLIP2_CONTAINER_SELECTOR).html(view);
        effect.stop();
        window.reverse ? effect.reverse() : effect.play();
    } else {
        loadFeedView(view);
    }

    window.reverse = !window.reverse;
}

function refreshFeed(view) {
    if (window.flipSupported) {
        $(window.FLIP2_CONTAINER_SELECTOR).hide().fadeOut().html(view).fadeIn();
    } else {
        loadFeedView(view);
    }
}

function loadFeedView(view) {
    $(window.FLIP1_CONTAINER_SELECTOR).toggle("slide", function () {
        $(window.FLIP1_CONTAINER_SELECTOR).html(view).toggle("slide");
    });
}

function showRoomFeed() {
    if (window.flipSupported) {
        effect.stop();
        window.reverse ? effect.reverse() : effect.play();
    } else {
        ajaxAction(window.roomFeedUrl, "", loadFeedView, null);
        //$(window.FLIP2_CONTAINER_SELECTOR).toggle("slide", function () {
        //    $(window.FLIP2_CONTAINER_SELECTOR).html(null);
        //    $(window.FLIP1_CONTAINER_SELECTOR).toggle("slide");
        //});
    }

    window.reverse = !window.reverse;
}



