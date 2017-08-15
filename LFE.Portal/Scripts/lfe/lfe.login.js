var LOGIN_CONTAINER_SELECTOR = '#log-form';

var LOGIN_MAIN_CONTAINER_SELECTOR_FLIP = '#login-flip';
var REGISTER_MAIN_CONTAINER_SELECTOR_FLIP = '#reg-flip';

var LOGIN_MAIN_CONTAINER_SELECTOR = '#login-main';

var REQUEST_PWD_FORM_CONTAINER_SELECTOR = '#req-pwd-form-container';

var SWITCH_BTN_SELECTOR = '#btn-form-switch';

var TITLE_SELECTOR = '.login-head > h4';
var SUBMIT_BUTTON_SELECTOR = '#lw-action-button';
var ACC_FOOTER_LINK_SELECTOR = '#login-footer-link > a';

var LOGIN_FORM_SELECTOR = '#frm-lfe-login';
var REGISTER_FORM_SELECTOR = '#frmRegisterUser';
var FPWD_FORM_SELECTOR = '#frm-forgot-pwd';

var LOGIN_ERROR_FORM_SELECTOR = '.login-info-error';
var LOGIN_ERROR_MESSAGE_SELECTOR = '.login-info-error > .inner > .e-row';

var ACC_FORM_TYPES = {
    Login: 'Login'
    , Register: 'Register'
    , Password: 'Password'
}
//variables for login/register forms flip
var lr_frm_reverse;
var lr_frm_effect;

function doLfeLogin(formSelector,formLoaderSelector) {
    if ($(LOGIN_ERROR_FORM_SELECTOR).is(":visible")) {
        $(LOGIN_ERROR_MESSAGE_SELECTOR).empty();
        $(LOGIN_ERROR_FORM_SELECTOR).slideToggle();
    }
    var isValid = $(formSelector).validate().form();

    if (!isValid) return;
    showFormLoader(formLoaderSelector);
    $(formSelector).submit();
}

function doLfeRegister(formSelector, formLoaderSelector) {
    var isValid = $(formSelector).validate().form();

    if (!isValid) return;
    showFormLoader(formLoaderSelector);
    $(formSelector).submit();
}

function initLoginBehavior(mode) {
    window.reverse = false;
    

    handleLoginFormMode(mode);

    $(SUBMIT_BUTTON_SELECTOR).unbind('click').bind('click', function () {
        
        switch (mode) {
            case 'Login':
                doLfeLogin(LOGIN_FORM_SELECTOR, '.wnd-login');
                break;
            case 'Register':
                doLfeRegister(REGISTER_FORM_SELECTOR,'.wnd-login');
                break;
            case 'Password':
                showFormLoader('.wnd-login');
                $(FPWD_FORM_SELECTOR).submit();
                break;
        }
    });


    window.flipSupported = isFxSupported();

    if (window.flipSupported) {
        window.effect = kendo.fx(LOGIN_MAIN_CONTAINER_SELECTOR).flipHorizontal($(LOGIN_CONTAINER_SELECTOR), $(REQUEST_PWD_FORM_CONTAINER_SELECTOR)).duration(1000);
        window.lr_frm_effect = kendo.fx(LOGIN_MAIN_CONTAINER_SELECTOR).flipHorizontal($(LOGIN_MAIN_CONTAINER_SELECTOR_FLIP), $(REGISTER_MAIN_CONTAINER_SELECTOR_FLIP)).duration(1000);
    }

    if (isOldIE()) {
        try {
            $('input').placeholder();
        } catch (ex) {
            if (window.console) console.log(ex);
        }
    }

   
    $('#a-fp').unbind('click').click(function(e) {
        e.preventDefault();
        flipAccPanels(REQUEST_PWD_FORM_CONTAINER_SELECTOR, LOGIN_CONTAINER_SELECTOR, ACC_FORM_TYPES.Password);
    });

    $('#a-log').unbind('click').click(function (e) {
        e.preventDefault();
        showLogin(REQUEST_PWD_FORM_CONTAINER_SELECTOR);
    });
}

function setLoginSettings(type,forgotten_password_class) {
    var title,btnTitle,formName,switchTitle;

    $(TITLE_SELECTOR).removeClass(forgotten_password_class);

    switch (type) {
        case ACC_FORM_TYPES.Login:
            title = 'Login';
            btnTitle = title;
            formName = LOGIN_FORM_SELECTOR;
            $('#btn-link-reg');
            switchTitle = 'Register new account';
            lr_frm_reverse = false;
            $('.login-main').addClass('login-main-360');
            break;
        case ACC_FORM_TYPES.Register:
            title = 'Register';
            btnTitle = title;
            formName = REGISTER_FORM_SELECTOR;
            switchTitle = 'Login';
            lr_frm_reverse = true;
            $('.login-main').removeClass('login-main-360');
            break;
        case ACC_FORM_TYPES.Password:
            title = 'Forgot your password?';
            btnTitle = 'Send';
            $(TITLE_SELECTOR).addClass(forgotten_password_class);
            formName = FPWD_FORM_SELECTOR;
            switchTitle = 'Register new account';
            lr_frm_reverse = false;
            $('.login-main').addClass('login-main-360');
            break;
        default :
            return;
    }

    initUnobstructiveFormValidation($(formName));

    $(SWITCH_BTN_SELECTOR).html(switchTitle);
    $(TITLE_SELECTOR).html(title);
    $(SUBMIT_BUTTON_SELECTOR).find('span').html(btnTitle);
   
    $(SWITCH_BTN_SELECTOR).unbind('click').click(function (e) {
        e.preventDefault();
        if ($(REGISTER_MAIN_CONTAINER_SELECTOR_FLIP).is(":visible")) {
            flipMainAccPanels(LOGIN_MAIN_CONTAINER_SELECTOR_FLIP, REGISTER_MAIN_CONTAINER_SELECTOR_FLIP, ACC_FORM_TYPES.Login);
        } else {
            flipMainAccPanels(REGISTER_MAIN_CONTAINER_SELECTOR_FLIP, LOGIN_MAIN_CONTAINER_SELECTOR_FLIP, ACC_FORM_TYPES.Register);
        }

    });
}

function handleLoginFormMode(mode) {
    switch (window.LOGIN_FORM_MODE) {
        case 'MAIN':
            setLoginSettings(mode, 'mfp');
            break;
        case 'WINDOW':
            setLoginSettings(mode, 'fp');
            break;
        default:
            return;
    }
}

function showLogin(panel_2_hide_selector) {
    flipAccPanels(LOGIN_CONTAINER_SELECTOR, panel_2_hide_selector, ACC_FORM_TYPES.Login);
}

function onLoginEnd(response) {
    hideFormLoader();
    if (response.success) {
        if (response.result && hasValue(response.result.returnUrl)) {
            window.location.href = response.result.returnUrl;
            return;
        }
        window.location.reload();
        window.location.href = window.location.href;
    } else {
        showLoginError(response.error);
    }
}

function flipMainAccPanels(panel_2_show_selector, panel_2_hide_selector, type) {

    if ($(panel_2_show_selector).is(":visible")) return;

    if (flipSupported) {
        lr_frm_effect.stop();
        lr_frm_reverse ? lr_frm_effect.reverse() : lr_frm_effect.play();
        lr_frm_reverse = !lr_frm_reverse;
    } else {
        $(panel_2_hide_selector).toggle("slide", function () {
            $(panel_2_show_selector).toggle("slide");
        });
    }

    initLoginBehavior(type);
}

function flipAccPanels(panel_2_show_selector, panel_2_hide_selector,type) {

    if ($(panel_2_show_selector).is(":visible")) return;

    if (flipSupported) {
        effect.stop();
        window.reverse ? effect.reverse() : effect.play();
        window.reverse = !window.reverse;
    } else {
        $(panel_2_hide_selector).toggle("slide", function () {
            $(panel_2_show_selector).toggle("slide");
        });
    }

    initLoginBehavior(type);
}

function showLoginError(error) {
    $(LOGIN_ERROR_MESSAGE_SELECTOR).html(error);

    if (hasValue(error)) {
        if ($(LOGIN_ERROR_FORM_SELECTOR).is(":visible")) return;

        $(LOGIN_ERROR_FORM_SELECTOR).slideToggle();

    } else {
        $(LOGIN_ERROR_FORM_SELECTOR).hide();
    }
}

function clearwndForgotPwd() {
    $('#frm-forgot-pwd #email').val(null);
}

function closeLoginWnd() {
    $('#modLogin').modal('hide');
}

function onForgotPasswordRequested(response) {
    hideFormLoader();

    if (response.success) {

        $('#frm-forgot-pwd').slideToggle(300, function () {
            $('.d-fp-form > p').html(
            '<h2 style="text-align:center">SUCCESS!!</h2>' +
            '<br />' +
            'A password update email was successfully sent to your email. Click O.K. to log-in with your new password'
        );

            $(SUBMIT_BUTTON_SELECTOR).find('span').html('O.K.');

            $(SUBMIT_BUTTON_SELECTOR).unbind('click').click(function () {
                window.reverse = true;
                showLogin(REQUEST_PWD_FORM_CONTAINER_SELECTOR);
            });
        });
    }
    else {
        showLoginError(response.error);
    }

}

function onReqPwd() {
    var email = $('#ul-fpwd').find('#email').val();
    if (email == null || email.length == 0) {
        showLoginError('Please, enter email address');
        return false;
    }
    showLoginError(null);
    showFormLoader('.wnd-login');
    return true;
}