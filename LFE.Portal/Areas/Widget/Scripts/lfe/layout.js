var windowParentUrl = encodeURIComponent(getParameterByName('parentUrl'));
if (window.console) console.log(windowParentUrl);
$(document).ready(function () {
    AddParentUrl();
    window.parent.postMessage(document.location.href, '*');
});

function AddParentUrl() {
    if (!lfeIsEmpty(windowParentUrl)) {
        $('a').each(function () {
            var href = $(this).attr('href');

            if (href) {
                href += (href.match(/\?/) ? '&' : '?') + "parentUrl=" + windowParentUrl;
                $(this).attr('href', href);
            }
        });
    }
}
function lfeIsEmpty(str) {
    return (!str || 0 === str.length);
}

function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}