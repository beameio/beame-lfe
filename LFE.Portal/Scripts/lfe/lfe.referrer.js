(function(){
    String.prototype.trim = function() {
        return this.replace(/^\s+|\s+$/g, '');
    }
    var Cookies = {
        Get: function(key) {
            var params = {};
            if (document.cookie && document.cookie.length > 0) {
                var __cArr = document.cookie.split(';');
                for (var i=0; i<__cArr.length; i++) {
                    var _index = __cArr[i].search('=');
                    var _key = __cArr[i].substr(0, _index).trim(), _value = unescape(__cArr[i].substr(_index + 1));
                    if (params[_key]) {
                        if (typeof(params[_key]) != 'object') 
                            params[_key] = [params[_key]];
                        params[_key].push(_value);
                    }
                    else params[_key] = _value;
                }
            }
            return key ? (params[key] != undefined ? params[key] : null) : params;
        },
        Set: function(key, value, durationSeconds) {
            if (!key || key.trim() == '') return;
            var expires = (durationSeconds)
                ? expires = ';expires=' + new Date((new Date()).getTime() + durationSeconds*1000).toUTCString()
                : '';
            document.cookie = key.trim() + '=' + escape(value) + expires + ';path=/';
        },
        Clear: function(key) {
            this.Set(key, '', -1000);
        }
    };
    //function getDomain(url) {
    //    if(url.search(/^https?\:\/\//) != -1)
    //        url = url.match(/^https?\:\/\/([^\/?#]+)(?:[\/?#]|$)/i);
    //    else
    //        url = url.match(/^([^\/?#]+)(?:[\/?#]|$)/i);
    //    if (url) {
    //        var split = url[1].split('.');
    //        if (split.length > 1) split.shift(); //removes the subdomain portion
    //        url = split.join('.');
    //    }
    //    return url;
    //}
    function isInternalReferrer(url) {
        for (var i=0; i<DOMAINS.length; i++)
            if (url.search(DOMAINS[i]) > -1) return true;
        return false;
    }

    var COOKIE_REFERRER = '_lfeReferrer', DOMAINS = [/\bbeame.io\b/ig],// /\blocalhost/ig],
        current = document.referrer;

    //====================== LOGIC ==============================
    if (!isInternalReferrer(current)) {
        current = !current || current.trim() == '' ? 'beame.io' : current;
        Cookies.Set(COOKIE_REFERRER, current);
    }
    else {
        current = Cookies.Get(COOKIE_REFERRER);
    }

    window.addEventListener("beforeunload", function (e) {
        var ev = (e || window.event);
        Cookies.Set(COOKIE_REFERRER, current);
    });

    // debug code
    //window.addEventListener("load", function() {
    //    var div = document.createElement('div');
    //    div.setAttribute('style', 'position:absolute;font-size:0.6em;top:5px;left:5px;background-color:#000000;color:#FFFFFF;padding:5px;z-index:1000;');
    //    div.addEventListener('click', function(e){ this.parentNode.removeChild(this); })
    //    document.body.appendChild(div);
    //    div.innerHTML = current;
    //    setTimeout(function(){
    //        div.parentNode.removeChild(div);
    //    }, 6000);
    //})
    
})();
