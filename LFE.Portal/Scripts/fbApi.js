
(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) return;
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/en_US/all.js#xfbml=1&appId=" + window.facebookAppId;
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));


$(document).ready(function () {
    $('#facebookLogin, #facebookLoginBig').click(function (e) {
        e.preventDefault();
        if (typeof FB !== 'undefined' && typeof FB.login === 'function') {
            FB.login(function (response) {
                if (response.authResponse && response.status === 'connected') {
                    onFacebokLogin(response.authResponse);
                }
            }, { scope: 'email,user_birthday,publish_stream' });
        }
    });
});


function onFacebokLogin(response) {
    $.post(window.socialLoginUrl, { provider: 'facebook', token: response.accessToken }, function (data) {
        if (data.isSuccess) {
            location.reload();
        } else {
            alert(response.error);
        }
    });
}

