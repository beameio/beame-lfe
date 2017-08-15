var emptyGuid = "00000000-0000-0000-0000-000000000000";
var emptyHref = "#";
var SAVE_PAYPAL_MESSAGE = 'Sign a billing agreement to streaming future purchase with paypal';
var SAVE_CC_MESSAGE = 'Save credit card details on PayPal for future use';
var BUY_CC_BTN_TEXT = 'Buy';
var BUY_PAYPAL_BTN_TEXT = 'Continue';
var BUY_CC_TITLE = 'Payment with Credit Card';
var BUY_SAVED_CC_TITLE = 'Payment with Saved Credit Card';
var BUY_PAYPAL_TITLE = 'Payment with Paypal';
var SAVED_CC_INFO_MESSAGE = 'This payment will be processed with the above saved credit card ';
var PAYPAL_INFO_MESSAGE = 'You will be redirected to the Paypal website for completion of the payment process';


var CC_ERROR_PANE_SELECTOR = "#cc-error-pane";

var FREE_FORM_SELECTOR = '#frm-pay-free';
var CC_FORM_SELECTOR = '#frm-pay-cc';
var SCC_FORM_SELECTOR = '#frm-pay-scc';
var PAYPAL_FORM_SELECTOR = '#frm-pay-paypal';

var messageKind = {
    error: 'error',
    info: 'info'
};

function adjustScModalH() {
    parent.adjustscIframeH(document);
}

function onCouponChecked(response) {
    window.hideLoader();
    var kind;

    if (response.IsValid) {
        kind = window.messageKind.info;
        purchaseModel.set('Price', response.FinalPrice.toFixed(2));
        purchaseModel.set('CouponCode', $('#couponCode').val());
        purchaseModel.set('CouponApplied', true);
        $('#couponCode').attr({ disabled: 'disabled' });
        $('#btn-submit-coupon').addClass('disabled');
        showMessage(response.Message, kind);


        $('.coupon-container').hide();

        if (response.IsFree) {
            $('#po-options-area').slideToggle(300, function() {
                $('#po-options-area').empty();
                $('#po-options-area-container #IsFree').val(true);
                $('.btn-buy > span').html('Complete');                
                //adjustScModalH();
            });
        } else {
            //adjustScModalH();
        }



    } else {
        purchaseModel.set('Price', window.basePrice);
        kind = window.messageKind.error;
        showMessage(response.Message, kind);
        //adjustScModalH();
    }
    
}

function showMessage(msg, kind) {
    var messageContainer = $('#msg-container');
    //reset
    purchaseModel.set('msgClass', '');
    purchaseModel.set('infoMessage', '');
    messageContainer.hide();
    purchaseModel.set('msgClass', kind);
    purchaseModel.set('infoMessage', msg);
    messageContainer.fadeIn(300);
    //parent.adjustscIframeH(document);
}

//function reloadCoursePage() {
//    self.parent.showLoader();
//    self.parent.location.reload(true);
//}



function onAddressSelected(e) {
    var dataItem = this.dataItem(e.item.index());

    $('#frmLoadAddress #addressId').val(dataItem.AddressId);

    var isAddr = hasValue(dataItem.AddressId);

    purchaseModel.set('isNewAddrChecked', !isAddr);

    $('#frmLoadAddress').submit();
}

function loadDefaultContact() {
    var combo = $('#ddlUserAdresses').data("kendoDropDownList");
    if (combo == undefined) return;
    if (combo.dataSource._data.length == 0) {
        purchaseModel.set('isNewAddrChecked', true);
        purchaseModel.set('isNewAddrDisabled', true);
        return;
    }
    combo.select(1);
    combo.trigger("select", { item: $("#ddlUserAdresses_listbox li.k-state-selected") });
}


function bindSavedCCComboEvents() {
    var ul = $('#ul-scc');

    $.each(ul.find('.scc-opt-container'), function () {
        var box = $(this);

        box.unbind('click').click(function () {            
            handleSccClickEvent(this);
        });
    });
}

function handleSccClickEvent($this) {
    
    var current = $('#scc-selected').html();

    var currentId = $(current).find('span').attr('data-val');
    var selectedId = $($this).find('span').attr('data-val');

    if (selectedId == currentId) return;

    var li = $($this).parent();

    $('#scc-selected').html($($this));

    li.html(current);

    bindSavedCCComboEvents();

    purchaseModel.onSavedCCSelected();
}

function setCcFormValidator() {
    $(CC_FORM_SELECTOR).validate().settings.ignore = [];
    initUnobstructiveFormValidation($(CC_FORM_SELECTOR));
}

function onSubmitClicked() {

    if ($(CC_ERROR_PANE_SELECTOR).is(":visible")) $(CC_ERROR_PANE_SELECTOR).slideToggle(300, function () {
        $(CC_ERROR_PANE_SELECTOR).empty();
        //adjustScModalH();
    });

    var fv = $('#IsFree').val();
    var isFree = hasValue(fv) && fv.toLowerCase() == 'true';

    var formName;

    if (!isFree) {

        var method = purchaseModel.get("method");
        switch (method) {
        case 'Credit_Card':
            formName = CC_FORM_SELECTOR;
            break;
        case 'Saved_Instrument':
            formName = SCC_FORM_SELECTOR;
            break;
        case 'Paypal':
            formName = PAYPAL_FORM_SELECTOR;
            break;
        default:
            alert('Unknown payment method');
            return;

        }
        var isValid = $(formName).validate().form();

        if (!isValid) {
            console.log($(formName).validate());
            return;
        }
    } else {
        formName = FREE_FORM_SELECTOR;

    }

    showLoadingPanel('.sc-main-area', 'We are processing your purchase request. This may take a few moments.', null);

    $(formName).submit();
}

function onMoreErrorClicked(e) {
    e.preventDefault();

    var title = $(e.currentTarget).parent().siblings('.pp-error').is(":visible") ? "More" : "Less";
    $(e.currentTarget).html(title);

   // $(e.currentTarget).parent().siblings('.pp-error').slideToggle(300, adjustScModalH);
}

function showErrorPage(message) {
    $('#frm-purchase-error #error').val(message);
    $('#frm-purchase-error').submit();
}

function onPurchaseRequestComplete(response) {
    hideFormLoader();
   
    if (!response.success) {

        var errorTemplate = kendo.template($("#cc-error-template").html());

        var data = { paypal_error: response.error };

        $(CC_ERROR_PANE_SELECTOR).html(errorTemplate(data)).slideToggle(300, function () {
            $(CC_ERROR_PANE_SELECTOR).find('a').unbind('click').click(onMoreErrorClicked);
            //adjustScModalH();
        });
    } else {
        window.parent.location.href = response.result.url;
    }
}
//#region model


var purchaseModel = kendo.observable({
    emptyGuid: emptyGuid,
    Price: basePrice,
    PriceType: null,
    CouponCode: '',
    CouponApplied: false,
    PaymentInstrumentId: emptyGuid,
    //selected payment method
    method: 'Paypal',


    //notification properties
    msgClass: '',
    infoMessage: '',

    //checkbox and submit button
    saveInstrMessage: SAVE_CC_MESSAGE,
    buyButtonText: BUY_CC_BTN_TEXT,

    //info block instead cc
    instrumentTitle: BUY_CC_TITLE,
    instrumentInfoMessage: PAYPAL_INFO_MESSAGE,

    //new address checkbox behavior
    isNewAddrDisabled: false,
    isNewAddrChecked: true,
    savedAddressCount: 0,

    getMessageClass: function () {
        return this.get('msgClass');
    },

    isSaveCreditCardVisible: function () {
        return this.get('method') == 'Credit_Card' ? 'block' : 'none';
    },

    isPaypalAreaVisible: function () {
        return this.get('method') == 'Paypal' ? 'block' : 'none';
    },

    isCCAreaVisible: function () {
        return this.get('method') != 'Paypal' ? 'block' : 'none';
    },

    isAddressComboVisible: function () {
        // console.log(this.get('savedAddressCount'));
        return this.get('savedAddressCount') > 0 ? 'block' : 'none';
    },

    onShowCouponClicked: function (e) {
        e.preventDefault();
        $('#d-coupon-input-container').slideToggle(300, function() {
            // adjustScModalH();
        });

    },
    isSaveInstrumentChecked: function () {
        return this.get('method') == 'Credit_Card';
    },

    isSaveInstrumentDisabled: function () {

        return this.get('PriceType') == 'SUBSCRIPTION' || !(this.get('method') == 'Credit_Card');
    },

    onNewAddressClicked: function () {
        var checked = $('#chkUseNewAddress').prop('checked');
        if (checked) {
            $('#ddlUserAdresses').data("kendoDropDownList").value(null);
            $('#frmLoadAddress #addressId').val(-1); //get new
            $('#frmLoadAddress').submit();
        } else {
            loadDefaultContact();
        }
    },

    resetPaymentOptionActive: function () {
        $('.po-box').removeClass('active');
        $('#scc-selected > .scc-opt-container > .po-box').removeClass('active');
    },

    onPaymentMethodChanged: function (e) {
        this.resetPaymentOptionActive();
        $(e.currentTarget).addClass('active');
        this.set('method', $(e.currentTarget).attr('data-val'));
        this.handlePaymentMethopdChange();
    },

    onSavedCCSelected: function () {
        var id = $('#scc-selected > .scc-opt-container > .po-box').attr('data-val');
        this.set('PaymentInstrumentId', id);
        this.set('method', id == emptyGuid ? 'Credit_Card' : 'Saved_Instrument');
        $('.po-box').removeClass('active');

        this.resetPaymentOptionActive();

        if (id == emptyGuid) { //set cc as selected
            this.init();
        } else {
            $('#scc-selected > .scc-opt-container > .po-box').addClass('active');
        }
        this.handlePaymentMethopdChange();
    },

    resetSavedCC: function () {
        var id = $('#scc-selected > .scc-opt-container > .po-box').attr('data-val');

        if (id == emptyGuid) return;//combo in default mode

        var $this = $('.scc-ddl').find("[data-val='" + emptyGuid + "']").parent();

        var current = $('#scc-selected').html();
        var li = $($this).parent();
        $('#scc-selected').html($($this));
        li.html(current);
        bindSavedCCComboEvents();
    },

    handlePaymentMethopdChange: function () {
        var method = this.get("method");
        var $this = purchaseModel;
       // console.log(method);
        switch (method) {
            case 'Credit_Card':
                $this.set('saveInstrMessage', SAVE_CC_MESSAGE);
                $this.set('buyButtonText', BUY_CC_BTN_TEXT);
                $this.set('instrumentTitle', BUY_CC_TITLE);
                $('#d-pp-container').slideUp(300, function () {
                    $this.setCCValidationStatus(true);

                    $this.set('PaymentInstrumentId', emptyGuid);
                    $('#d-cc-container').slideDown(300, function () {
                        //adjustScModalH();
                    });

                    if (!$('#d-ba-box').is(':visible')) {
                        $('#d-ba-box').slideDown(300, function () {
                            $('#frmLoadAddress').submit();
                            $('#d-ba-container').slideDown(300, function () {
                                //adjustScModalH();
                            });
                        });
                    }

                    //$('#frmBuyCourse').attr('target', '_self');
                });

                $this.get('savedAddressCount') > 0 ? $('#d-ba-box').show() : $('#d-ba-box').hide();

                $this.resetSavedCC();

                setCcFormValidator();
                break;
            case 'Paypal':
            case 'Saved_Instrument':
                var toHideSelector, buttonText, infoTitle, infoMessage;

                if (method == 'Paypal') {
                    buttonText = BUY_PAYPAL_BTN_TEXT;
                    infoTitle = BUY_PAYPAL_TITLE;
                    infoMessage = PAYPAL_INFO_MESSAGE;
                    $this.set('PaymentInstrumentId', emptyGuid);
                    //$('#frmBuyCourse').attr('target', '_blank');
                } else {
                    buttonText = BUY_CC_BTN_TEXT;
                    infoTitle = BUY_SAVED_CC_TITLE;
                    infoMessage = SAVED_CC_INFO_MESSAGE;

                   // $('#frmBuyCourse').attr('target', '_self');
                }

                if ($('#d-cc-container').is(':visible')) {
                    toHideSelector = '#d-cc-container';
                } else {
                    toHideSelector = '#d-pp-container';
                    $(toHideSelector).fadeToggle();
                }
                if ($('#d-ba-box').is(':visible')) {
                    $('#d-ba-box').slideToggle(300, function () {
                        $('#d-ba-container').slideToggle().empty();
                        //adjustScModalH();
                    });
                }
                $(toHideSelector).slideUp(300, function () {
                    $this.set('saveInstrMessage', '');
                    $this.set('buyButtonText', buttonText);
                    $this.set('instrumentTitle', infoTitle);
                    $this.set('instrumentInfoMessage', infoMessage);
                    $this.setCCValidationStatus(false);
                    $('#d-pp-container').slideDown(300, function () {
                        //adjustScModalH();
                    });
                });
                if (method == 'Saved_Instrument') return;
                $this.resetSavedCC();
                break;

            default:
                return;

        }
    },

    setCCValidationStatus: function (enabled) {
        if (enabled) {
            $('#d-cc-container input').removeAttr('disabled');
        } else {
            $('#d-cc-container input').attr('disabled', 'disabled');
        }
    },

    onCheckCouponClicked: function (e) {
        var couponApplied = purchaseModel.get('CouponApplied');
        if (couponApplied) {
            e.preventDefault();
            return false;
        }

        if (!hasValue($('#couponCode').val())) {
            e.preventDefault();
            showMessage("insert coupon code", window.messageKind.error);
            return false;
        }
        showLoader();
        $('#frmCheckCoupon').submit();
        return true;
    },



    init: function (price, priceType, addresses) {
        var method = this.get("method");

        if(price!=null) this.set('savedAddressCount', addresses);
        if (priceType != null) this.set('Price', price);
        if (addresses != null) this.set('PriceType', priceType);

        $('.po-box').filter("[data-val='" + method + "']").addClass('active');

        this.setCCValidationStatus(false);

        $('#po-options-area-container').css('visibility', 'visible');

    }
});

//#endregion