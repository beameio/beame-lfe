using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.Widget.Helpers;
using LFE.Portal.Areas.Widget.Models;
using LFE.Portal.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;


namespace LFE.Portal.Areas.Widget.Controllers
{

    [Authorize]
    public class PaymentController : Portal.Controllers.BaseController
    {
        private readonly IPaypalServices _paypalServices;
        private readonly IUserAccountServices _userAccountServices;
        private readonly IBillingServices _billingServices;
       // private readonly IWidgetWebStoreServices _webStorePortalServices;

        private readonly IWidgetCourseServices _widgetCourseServices;
        private readonly ICouponWidgetServices _couponWidgetServices;
        private readonly IGeoServices _geoServices;

        public PaymentController()
        {
            _paypalServices = DependencyResolver.Current.GetService<IPaypalServices>();
            _billingServices = DependencyResolver.Current.GetService<IBillingServices>();
            _widgetCourseServices = DependencyResolver.Current.GetService<IWidgetCourseServices>();
            _couponWidgetServices = DependencyResolver.Current.GetService<ICouponWidgetServices>();
            _geoServices = DependencyResolver.Current.GetService<IGeoServices>();
            _userAccountServices = DependencyResolver.Current.GetService<IUserAccountServices>();
           // _webStorePortalServices = DependencyResolver.Current.GetService<IWidgetWebStoreServices>();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (Request.Url == null || Request.Url.Host.ToLower() == "local.lfe.com") return;

            if (filterContext.ExceptionHandled)
            {
                return;
            }
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Areas/Widget/Views/Shared/Error.cshtml"
            };
            filterContext.ExceptionHandled = true;
        }

        #region private methods
        //private Guid? ExtractRequestIdFromQs()
        //{
        //    Guid requestId;

        //    return Guid.TryParse(Request.QueryString["id"], out requestId) ? (Guid?)requestId : null;
        //}

        private ActionResult Redirect2PaymentError(string error, string itemName, int priceLineId, string trackingId, eActionKinds kind,string refferal)
        {
            TempData["PaypalError"] = error;

           // return kind == eActionKinds.AJAX ? ErrorResponse(error) : View("PurchaseResult", new PurchaseResultToken { IsValid = false, ActionKind = kind, Message = error, PriceLineId = priceLineId, ItemName = itemName, TrackingId = trackingId });

            if (kind == eActionKinds.AJAX)
            {
                return ErrorResponse(error);
            }

            var cc = new CheckoutBaseController(priceLineId,refferal,trackingId);
            cc.CheckoutBase.IsValid = false;
            cc.CheckoutBase.Message = error;
            return View("../Checkout/Failure",cc.CheckoutBase);
        }

        private bool IsAddressValid(ref BillingAddressDTO address, out string error)
        {
            error = string.Empty;

            if (address == null)
            {
                error = "address required";
                return false;
            }

            if (address.CountryId == null)
            {
                error = "Country required";
                return false;
            }

            if (address.CountryId != null && Constants.COUNTRIES_WITH_STATES.Contains((int)address.CountryId) && address.StateId == null)
            {
                error = "State required";
                return false;
            }

            if (String.IsNullOrEmpty(address.PostalCode))
            {
                error = "Postal code required";
                return false;
            }

            if (String.IsNullOrEmpty(address.City))
            {
                error = "City required";
                return false;
            }

            if (String.IsNullOrEmpty(address.Street1))
            {
                error = "Street required";
                return false;
            }

            if (String.IsNullOrEmpty(address.BillingFirstName))
            {
                error = "Billing FirstName required";
                return false;
            }

            if (String.IsNullOrEmpty(address.BillingLastName))
            {
                error = "Billing LastName required";
                return false;
            }
            var cid = address.CountryId;
            var country = _geoServices.ActiveCountries().FirstOrDefault(x => x.CountryId == cid);
            if (country != null)
            {
                address.CountryCode = country.A2;
            }
            else
            {
                error = "country not found";
                return false;
            }

            if (address.StateId != null)
            {
                var sid = address.StateId;
                var state = _geoServices.GetStates((short)cid).FirstOrDefault(x => x.StateId == sid);
                if (state != null) address.StateCode = state.StateCode;
            }

            address.UserId = CurrentUserId;

            return true;
        }
        #endregion

        #region views
        public ActionResult LoadPurchaseForm(int? id, string trackingId)
        {
            //SaveUserEvent(CommonEnums.eUserEvents.BUY_PAGE_ENTERED,String.Format("Item is {0}, author is{1}, mode {2}",String.IsNullOrEmpty(courseName) ? bundleName: courseName,authorName,buySubscription ? "Subscription" : "Unlimited" ),trackingID);
           // var url = this.GeneratePurchaseCompleteUrl(1, trackingId);
            if (id == null)
            {
                var token = new ItemPurchaseDataToken
                {
                    IsValid = false
                    ,Message = "Required param ID missing"
                };

                return View("PurchaseForm", token);
            }
            string error;

            var dto = _widgetCourseServices.GetItemPurchaseDtoByPriceLineId((int)id, out error);

            if (dto == null)
            {
                var token = new ItemPurchaseDataToken
                {
                    IsValid = false
                    ,Message = String.IsNullOrEmpty(error) ? "Item not found" : error
                };

                return View("PurchaseForm", token);
            }

            ItemAccessStateToken itemState;
            switch (dto.Type)
            {
                case BillingEnums.ePurchaseItemTypes.COURSE:
                    itemState = _widgetCourseServices.GetCourseAccessState4User(CurrentUserId, dto.ItemId);
                    break;
                case BillingEnums.ePurchaseItemTypes.BUNDLE:
                    itemState = _widgetCourseServices.GetBundleAccessState4User(CurrentUserId, dto.ItemId);
                    break;
                default:
                    var token = new ItemPurchaseDataToken
                    {
                        IsValid = false
                        ,Message = "Unknown Item type"
                    };

                    return View("PurchaseForm", token);
            }

            //var itemState = !String.IsNullOrEmpty(courseName) ? _courseServices.GetCourseAccessState4User(CurrentUserId, dto.ItemId) : _courseServices.GetBundleAccessState4User(CurrentUserId, dto.ItemId);

            dto.IsPurchased = itemState.IsAccessAllowed || itemState.IsOwnedByUser;
            dto.TrackingID = trackingId;

            dto.BillingAddresses = _userAccountServices.GetUserBillingAddresses(CurrentUserId).Where(x => x.IsActive).ToList();
            dto.UserSavedCards = _userAccountServices.GetUserSavedCardsLOV(CurrentUserId);

            EventLoggerService.Report(new ReportToken
            {
                UserId = CurrentUserId,
                EventType = CommonEnums.eUserEvents.BUY_PAGE_ENTERED,
                NetSessionId = Session.SessionID,
                AdditionalMiscData = string.Format("item ID: {0}, itemType {1}", dto.ItemId, dto.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? "Bundle" : "Course"),
                TrackingID = trackingId,
                CourseId = dto.Type == BillingEnums.ePurchaseItemTypes.COURSE ? dto.ItemId : (int?)null,
                BundleId = dto.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? dto.ItemId : (int?)null,
                HostName = GetReferrer()
            });

            return View("Payment/_PurchaseForm", dto);
        }
        
        public ActionResult OnPaymentComplete(string refferal)
        {
            Guid requestId;

            if (!Guid.TryParse(Request.QueryString["id"], out requestId)) return Redirect2PaymentError("RequestId required", string.Empty, -1, string.Empty, eActionKinds.POST,refferal);

            if (String.IsNullOrEmpty(Request.QueryString["PayerID"])) return Redirect2PaymentError("PayerID required", string.Empty, -1, string.Empty, eActionKinds.POST,refferal);

            string error;
            var orderNo = -1;

            var baseInfo = _paypalServices.GetItemInfoFromPaymentRequest(requestId);

            if(!baseInfo.IsValid) return Redirect2PaymentError("Request not found", string.Empty, -1, string.Empty, eActionKinds.POST,refferal);

            var executed = _paypalServices.ExecutePayPalPayment(requestId, Request.QueryString["PayerID"], out error, Session.SessionID) && _billingServices.CompletePaymentRequest(requestId, out orderNo, out error);

            //return executed ? View("PurchaseResult", new PurchaseResultToken { IsValid = true, ActionKind = eActionKinds.POST, RedirectUrl = Url.Action("PurchaseComplete", "Payment", new { area = "Widget", id = orderNo, trackingId = baseInfo.TrackingId }) }) : 
            //                  Redirect2PaymentError(error ?? "unexpected error", baseInfo.ItemName, baseInfo.PriceLineId, baseInfo.TrackingId, eActionKinds.POST);

            return RedirectToAction("PurchaseResult", new
            {
                isValid    = executed,
                actionKind = eActionKinds.POST,
                lineId     = baseInfo.PriceLineId,
                itemName   = baseInfo.ItemName,
                trackingId = baseInfo.TrackingId,
                orderNo,
                error,
                refferal
            });
        }
        
        public ActionResult OnSubscriptionComplete(Guid? id,string refferal)
        {
            string error;
            var requestId = id;

            if (requestId == null) return Redirect2PaymentError("requestId missing", string.Empty, -1, string.Empty, eActionKinds.POST,refferal);

            var baseInfo = _paypalServices.GetItemInfoFromPaymentRequest((Guid) requestId);

            if (!baseInfo.IsValid) return Redirect2PaymentError("Request not found", string.Empty, -1, string.Empty, eActionKinds.POST, refferal);

            if (String.IsNullOrEmpty(Request.QueryString["TOKEN"]))
            {
                _paypalServices.UpdatePaypalRequestStatus((Guid)requestId, BillingEnums.ePaymentRequestStatus.failed, out error);

                return Redirect2PaymentError("token missing", baseInfo.ItemName, baseInfo.PriceLineId, baseInfo.TrackingId, eActionKinds.POST, refferal);
            }

            var agreement_token = Request.QueryString["TOKEN"];

            CreditCardDTO card = null;

            if (Session["RecurringCc"] != null)
            {
                card = Session["RecurringCc"] as CreditCardDTO;
            }

            RecurringPaymentExecutionResultToken result;
            
            var orderNo = -1;
           
            var executed = _paypalServices.ExecuteCourseSubscriptionPayalRecurringPayment(CurrentUserId, (Guid)requestId, agreement_token, card, out result, out error, Session.SessionID) && _billingServices.CompleteSubscriptionRequest(result, out orderNo, out error);

            //return executed ? View("PurchaseResult", new PurchaseResultToken { IsValid = true, ActionKind = eActionKinds.POST, RedirectUrl = Url.Action("PurchaseComplete", "Payment", new { area = "Widget", id = orderNo, trackingId = baseInfo.TrackingId }) }) :
            //                    Redirect2PaymentError(error ?? "unexpected error", baseInfo.ItemName, baseInfo.PriceLineId, baseInfo.TrackingId, eActionKinds.POST);

            return RedirectToAction("PurchaseResult", new
            {
                isValid    = executed,
                actionKind = eActionKinds.POST,
                lineId     = baseInfo.PriceLineId,
                itemName   = baseInfo.ItemName,
                trackingId = baseInfo.TrackingId,
                orderNo,
                error,
                refferal
            });
        }
        
        public ActionResult CancelPayment()
        {
            return View();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">order number</param>
        /// <param name="trackingId"></param>
        /// <returns></returns>
        public ActionResult PurchaseComplete(int? id, string trackingId)
        {
            ItemPurchaseCompleteToken token;
            if (id == null)
            {
                token = new ItemPurchaseCompleteToken { IsValid = false, Message = "Order number required" };
            }
            else
            {
                string error;

                token = _widgetCourseServices.GetItemPurchaseCompleteToken((int)id, out error);

                if (token == null)
                {
                    token = new ItemPurchaseCompleteToken { IsValid = false, Message = error };
                }
                else
                {
                    token.TrackingID = trackingId;

                    var user = this.CurrentUser();

                    if (!user.IsCurrentUserAdmin() && token.BuyerInfo.UserId != user.UserId)
                    {
                        token = new ItemPurchaseCompleteToken { IsValid = false, Message = "You haven't permission" };
                    }
                    else
                    {
                        token.IsValid = true;
                    }

                }
            }

            return View(token);
        }

        public ActionResult PurchaseResult(bool isValid,eActionKinds actionKind, int? orderNo,int? priceLineId,string itemName,string trackingId,string error,string refferal)
        {
            if (!isValid) return Redirect2PaymentError(error, itemName, priceLineId ?? -1, trackingId, actionKind, refferal);

            var url = this.GenerateCheckoutUrl(priceLineId ?? -1, orderNo ?? -1,"Success", trackingId, refferal);

            return Redirect(url);

            //var token = new PurchaseResultToken
            //{
            //    IsValid      = true
            //    ,ActionKind  = actionKind 
            //    ,ItemName    = itemName
            //    ,PriceLineId = priceLineId ?? -1
            //    ,RedirectUrl = this.GeneratePurchaseCompleteUrl(priceLineId?? -1,orderNo ?? -1, trackingId,refferal) 
            //};

            //return View(token);
        }

        public ActionResult PurchaseErrorResult(int? lineId,string itemName,string trackingId,string error)
        {
            var token = new PurchaseResultToken
            {
                IsValid      = false
                ,Message     = error
                ,ActionKind  = eActionKinds.POST
                ,ItemName    = itemName
                ,PriceLineId = lineId ?? -1
                ,RedirectUrl = string.Empty
                ,ShowPage    = true                
            };

            return View("PurchaseResult",token);
        }
        #endregion

        #region posts
        public ActionResult GetItemPriceWithCoupon(int lineId, int ownerId, int itemId, string couponCode, BillingEnums.ePurchaseItemTypes itemType)
        {
            int? courseId = null;
            int? bundleId = null;

            switch (itemType)
            {
                case BillingEnums.ePurchaseItemTypes.COURSE:
                    courseId = itemId;
                    break;
                case BillingEnums.ePurchaseItemTypes.BUNDLE:
                    bundleId = itemId;
                    break;
            }

            var checkResult = _couponWidgetServices.ValidateCoupon(lineId, ownerId, courseId, bundleId, couponCode);

            return Json(checkResult, JsonRequestBehavior.AllowGet);
        }

        private void SavePurchaseCompleteEvent(ItemPurchaseDataToken token)
        {
            SaveUserEvent(CommonEnums.eUserEvents.PURCHASE_COMPLETE, String.Format("{0} purchased", token.ItemName), token.TrackingID, token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?)null, token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?)null);
        }

        private ActionResult ReturnPurchaseSuccess(int priceLineId, int orderNo, string trackingId, string refferal)
        {
            return
                Json(new JsonResponseToken
                {
                    success = true,
                    result = new
                    {
                        url = this.GenerateCheckoutUrl(priceLineId,orderNo,"Success", trackingId,refferal)
                    }
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExecuteItemPurchase(ItemPurchaseDataToken token, BillingAddressDTO address, eActionKinds actionKind,string refferal)
        {
            string error;
            var orderNo = -1;
            var user = this.CurrentUser();

            if (user == null) return Redirect2PaymentError("authentication issue. please re-login again", token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);

            try
            {
                if (token.IsFree)
                {
                    if (String.IsNullOrWhiteSpace(token.CouponCode)) return Redirect2PaymentError("coupon code required", token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);

                    token.PaymentMethod = BillingEnums.ePaymentMethods.Charge_Free;

                    var completed = _billingServices.CompleteFreeCouponRequest(token, user.UserId, out orderNo, out error);

                    if (!completed) return Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);

                    SavePurchaseCompleteEvent(token);

                    return ReturnPurchaseSuccess(token.PriceToken.PriceLineID,orderNo, token.TrackingID,refferal);
                }


                var priceToken = _billingServices.GetPriceLineToken(token.PriceToken.PriceLineID, out error);

                if (priceToken == null) return Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);


                token.BuySubscription = priceToken.PriceType == BillingEnums.ePricingTypes.SUBSCRIPTION;

                #region private variables
                //int? storeId = null;
                //if (!String.IsNullOrEmpty(token.TrackingID))
                //{
                //    storeId = _webStorePortalServices.ValidateTrackingId(token.TrackingID);
                //}

                string approval_url;
                bool paymentExecuted;
                var paymentInstrumentId = Guid.Empty;
                var CANCEL_PAYMENT_URL = Utils.GetKeyValue("baseUrl") + Url.Action("CancelPayment");
                #endregion

                #region handle billing address

                int? addressId = null;
                if (token.PaymentMethod == BillingEnums.ePaymentMethods.Credit_Card)
                {
                    var billingAddressDto = address;

                    if (!IsAddressValid(ref billingAddressDto, out error)) return Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);

                    token.BillingAddress = billingAddressDto;

                    var addressSaved = _userAccountServices.SaveUserBillingAddress(ref billingAddressDto, out error);

                    if (!addressSaved) return Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);

                    addressId = billingAddressDto.AddressId;
                }

                #endregion

                #region payment method validation

                //in case of subscription with credit card save payment instrument option should be always true
                //we don't create on the moment recurring paypal payments with credit card = > payment method switched to Saved_Instrument
                if (token.BuySubscription && token.PaymentMethod == BillingEnums.ePaymentMethods.Credit_Card)
                {
                    token.SavePaymentInstrument = true;
                }

                switch (token.PaymentMethod)
                {
                    case BillingEnums.ePaymentMethods.Paypal:
                        //if (token.SavePaymentInstrument) return Redirect2PaymentError("saving paypal agreement currently not supported");
                        break;
                    case BillingEnums.ePaymentMethods.Credit_Card:
                        if (token.CreditCard == null) return Redirect2PaymentError("credit card required", token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);

                        if (addressId == null)
                        {
                            return Redirect2PaymentError("Address required", token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);
                        }

                        if (token.SavePaymentInstrument)
                        {
                            //create card token
                            var card = token.CoursePurchaseDataToken2CreditCardDto(this.CurrentUser());

                            //save cc to paypal
                            var cardSaved = _paypalServices.SaveCreditCard2Paypal(card, CurrentUserId, out paymentInstrumentId, out error);

                            if (!cardSaved) return Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);
                        }
                        Session["RecurringCc"] = token.CreditCard;

                        if (token.BuySubscription) token.PaymentMethod = BillingEnums.ePaymentMethods.Saved_Instrument;

                        break;

                    case BillingEnums.ePaymentMethods.Saved_Instrument:
                        paymentInstrumentId = token.PaymentInstrumentId ?? Guid.Empty;
                        if (paymentInstrumentId.Equals(Guid.Empty)) return Redirect2PaymentError("Select credit card", token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);
                        break;
                }
                #endregion

                switch (priceToken.PriceType)
                {
                    case BillingEnums.ePricingTypes.ONE_TIME:
                    case BillingEnums.ePricingTypes.RENTAL:
                        PaypalCreditCardPaymentDTO ccToken;
                        switch (token.PaymentMethod)
                        {
                            case BillingEnums.ePaymentMethods.Paypal:
                                #region
                                var SUCCESS_PAYMENT_URL = Utils.GetKeyValue("baseUrl") + Url.Action("OnPaymentComplete", "Payment", new { area = "Widget",refferal });

                                var data = token.ItemPurchaseDataToken2PayPalCreatePaymentDto(SUCCESS_PAYMENT_URL, CANCEL_PAYMENT_URL, addressId, null);

                                paymentExecuted = _paypalServices.CreatePaypalAccountPayment(data, out approval_url, out error);

                                if (paymentExecuted && !String.IsNullOrEmpty(approval_url)) return Redirect(approval_url);

                                return Redirect2PaymentError(error ?? "unexpected error", token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);
                                #endregion
                            case BillingEnums.ePaymentMethods.Credit_Card:
                                #region
                                Guid requestId;

                                if (token.SavePaymentInstrument)
                                {
                                    //create cc payment token
                                    ccToken = token.CoursePurchaseDataToken2PayPalDirectCcPaymentDto(this.CurrentUser(), addressId, null);

                                    ccToken.paymentInstrumentId = paymentInstrumentId;

                                    //execute payment and complete purchase process
                                    paymentExecuted = _paypalServices.ExecuteSavedCreditCardPayment(ccToken, CurrentUserId, paymentInstrumentId, out requestId, out error) && _billingServices.CompletePaymentRequest(requestId, out orderNo, out error);
                                }
                                else
                                {
                                    //create cc payment token
                                    ccToken = token.CoursePurchaseDataToken2PayPalDirectCcPaymentDto(this.CurrentUser(), addressId, null);

                                    //execute payment and complete purchase process
                                    paymentExecuted = _paypalServices.ExecuteDirectCreditCardPayment(ccToken, out requestId, out error) && _billingServices.CompletePaymentRequest(requestId, out orderNo, out error);
                                }

                                if (paymentExecuted) SavePurchaseCompleteEvent(token);

                                //return paymentExecuted ? (storeId != null ? View("ThankYouStore",new PaypalCompleteRequestToken{IsSuccess = true,RedirectUrl = itemPageUrl}) : View("ThankYou")) : Redirect2PaymentError(error); 
                                return paymentExecuted ? ReturnPurchaseSuccess(token.PriceToken.PriceLineID, orderNo, token.TrackingID, refferal) : Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);
                                #endregion
                            case BillingEnums.ePaymentMethods.Saved_Instrument:
                                #region
                                //create cc payment token
                                ccToken = token.CoursePurchaseDataToken2PayPalDirectCcPaymentDto(this.CurrentUser(), addressId, null);

                                ccToken.paymentInstrumentId = token.PaymentInstrumentId;

                                //execute payment and complete purchase process
                                paymentExecuted = _paypalServices.ExecuteSavedCreditCardPayment(ccToken, CurrentUserId, paymentInstrumentId, out requestId, out error) && _billingServices.CompletePaymentRequest(requestId, out orderNo, out error);

                                if (paymentExecuted) SavePurchaseCompleteEvent(token);

                                // return paymentExecuted ? (storeId != null ? View("ThankYouStore", new PaypalCompleteRequestToken { IsSuccess = true, RedirectUrl = itemPageUrl }) : View("ThankYou")) : Redirect2PaymentError(error); 
                                return paymentExecuted ? ReturnPurchaseSuccess(token.PriceToken.PriceLineID, orderNo, token.TrackingID, refferal) : Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal); 
                                #endregion
                        }
                        break;
                    case BillingEnums.ePricingTypes.SUBSCRIPTION:
                        switch (token.PaymentMethod)
                        {
                            case BillingEnums.ePaymentMethods.Paypal:
                                #region
                                //create paypal agreement
                                //create request token
                                var requestId = Guid.NewGuid();
                                var success_url = Utils.GetKeyValue("baseUrl") + Url.Action("OnSubscriptionComplete", null, new { id = requestId, refferal  });

                                var data = token.CoursePurchaseDataToken2PayPalAgreementDto(user.FullName, user.Email, success_url, CANCEL_PAYMENT_URL, addressId, paymentInstrumentId, token.PaymentMethod, priceToken);


                                //create payment agreement
                                paymentExecuted = _paypalServices.CreateRecurringPaymentAgreement(data, requestId, out approval_url, out error);

                                //redirect to paypal approval page
                                return paymentExecuted && !String.IsNullOrEmpty(approval_url) ? Redirect(approval_url) : Redirect2PaymentError(error ?? "unexpected error", token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);
                                #endregion
                            //currently(2014-2-11) this option disabled
                            //case BillingEnums.ePaymentMethods.Credit_Card:
                            //    #region 
                            //    requestId = Guid.NewGuid();
                            //    RecurringPaymentExecutionResultToken result;
                            //    var dto = token.CoursePurchaseDataToken2PaypalPaymentRequestDto(requestId, CurrentUserId, addressId);
                            //    var executed = _paypalServices.ExecuteCourseSubscriptionCcRecurringPayment(dto, CurrentUserId, requestId, token.CreditCard, out result, out error) && _billingServices.CompleteSubscriptionRequest(result, out error);

                            //    var responseToken = new PaypalCompleteRequestToken
                            //    {
                            //        IsSuccess    = executed
                            //        ,Message     = error
                            //        ,RedirectUrl = executed ? Utils.GetKeyValue("baseUrl") + Url.Action("ThankYou", null, new { requestId }) : string.Empty
                            //    };

                            //    return executed ? View("ThankYou", responseToken) : Redirect2PaymentError(error ?? "unexpected error"); 
                            //    #endregion
                            case BillingEnums.ePaymentMethods.Saved_Instrument:
                                #region
                                var subscriptionToken = token.CoursePurchaseDataToken2SubscriptionWithSavedCardDto(CurrentUserId, paymentInstrumentId, addressId);
                                //TODO check case , when initial amount == 0;
                                subscriptionToken.amount = subscriptionToken.InitialAmount > 0 ? subscriptionToken.InitialAmount : (decimal)0.01;

                                paymentExecuted = _paypalServices.ExecuteSubscriptionPaymentWithStoredCreditCard(subscriptionToken, out requestId, out error);

                                if (!paymentExecuted) return Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);

                                var subscriptionCreated = _billingServices.CreateSubscriptionWithSavedCard(subscriptionToken, requestId, out orderNo, out error);

                                if (!subscriptionCreated) return Redirect2PaymentError(error ?? "unexpected error", token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);


                                SavePurchaseCompleteEvent(token);

                                return ReturnPurchaseSuccess(token.PriceToken.PriceLineID, orderNo, token.TrackingID, refferal);
                                #endregion
                        }
                        break;
                }


                return View("Payment/_PurchaseItem", token);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);

                return Redirect2PaymentError(error, token.ItemName, token.PriceToken.PriceLineID, token.TrackingID, actionKind, refferal);

            }
        }

        [HttpPost]
        public ActionResult PurchaseFreeCourse(int courseId, string trackingId = null,string refferal = null)
        {
            var orderNo = -1;
            string error;
            var result = false;

            if (CurrentUserId > 0)
            {
                result = _billingServices.CompleteFreeCourseRequest(courseId, CurrentUserId, trackingId, out orderNo, out error);
            }
            else
            {
                error = "please login first";
            }

            return result ? ReturnPurchaseSuccess(-1, orderNo, trackingId, refferal) : ErrorResponse(error);
        }
        #endregion
    }
}

#region obsolete
//public ActionResult PaypalError()
//{
//    return View("PaypalError", null, TempData["PaypalError"]);
//}
//public ActionResult Buy(int? id, string trackingID)
//{
//    //SaveUserEvent(CommonEnums.eUserEvents.BUY_PAGE_ENTERED,String.Format("Item is {0}, author is{1}, mode {2}",String.IsNullOrEmpty(courseName) ? bundleName: courseName,authorName,buySubscription ? "Subscription" : "Unlimited" ),trackingID);

//    if (id == null)
//    {
//        var token = new ItemPurchaseDataToken
//        {
//            IsValid = false
//            ,Message = "Required param ID missing"
//        };

//        return View("Payment/_PurchaseItem", token);
//    }
//    string error;

//    var dto = _widgetCourseServices.GetItemPurchaseDtoByPriceLineId((int)id, out error);

//    if (dto == null)
//    {
//        var token = new ItemPurchaseDataToken
//        {
//            IsValid = false
//            ,Message = String.IsNullOrEmpty(error) ? "Item not found" : error
//        };

//        return View("Payment/_PurchaseItem", token);
//    }

//    ItemAccessStateToken itemState;
//    switch (dto.Type)
//    {
//        case BillingEnums.ePurchaseItemTypes.COURSE:
//            itemState = _widgetCourseServices.GetCourseAccessState4User(CurrentUserId, dto.ItemId);
//            break;
//        case BillingEnums.ePurchaseItemTypes.BUNDLE:
//            itemState = _widgetCourseServices.GetBundleAccessState4User(CurrentUserId, dto.ItemId);
//            break;
//        default:
//            var token = new ItemPurchaseDataToken
//            {
//                IsValid = false
//                ,
//                Message = "Unknown Item type"
//            };

//            return View("Payment/_PurchaseItem", token);
//    }

//    //var itemState = !String.IsNullOrEmpty(courseName) ? _courseServices.GetCourseAccessState4User(CurrentUserId, dto.ItemId) : _courseServices.GetBundleAccessState4User(CurrentUserId, dto.ItemId);

//    dto.IsPurchased = itemState.IsAccessAllowed || itemState.IsOwnedByUser;
//    dto.TrackingID = trackingID;

//    dto.BillingAddresses = _userAccountServices.GetUserBillingAddresses(CurrentUserId);
//    dto.UserSavedCards = _userAccountServices.GetUserSavedCardsLOV(CurrentUserId);

//    EventLoggerService.Report(CurrentUserId, CommonEnums.eUserEvents.BUY_PAGE_ENTERED, Session.SessionID, string.Format("item ID: {0}, itemType {1}", dto.ItemId, dto.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? "Bundle" : "Course"), trackingID, dto.Type == BillingEnums.ePurchaseItemTypes.COURSE ? dto.ItemId : (int?)null, dto.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? dto.ItemId : (int?)null);

//    return View("Payment/_PurchaseItem", dto);
//}
//public ActionResult Buy(string courseName, string bundleName, string trackingID, string categoryName, string authorName,int id, bool buySubscription = false)
//{
//    //SaveUserEvent(CommonEnums.eUserEvents.BUY_PAGE_ENTERED,String.Format("Item is {0}, author is{1}, mode {2}",String.IsNullOrEmpty(courseName) ? bundleName: courseName,authorName,buySubscription ? "Subscription" : "Unlimited" ),trackingID);

//    if (String.IsNullOrEmpty(courseName) && String.IsNullOrEmpty(bundleName))
//    {
//        var token = new ItemPurchaseDataToken
//        {
//            IsValid = false
//            ,Message = "Item not found"
//        };

//        return View("Payment/_PurchaseItem", token);
//    }

//    var dto = !String.IsNullOrEmpty(courseName) ? _courseServices.GetCoursePurchaseDtoByCourseUrlName(authorName, courseName) : _courseServices.GetBundlePurchaseDtoByBundleUrlName(authorName, bundleName.OptimizedUrl());

//    if (dto == null)
//    {
//        var token = new ItemPurchaseDataToken
//        {
//            IsValid = false
//            ,Message = (String.IsNullOrEmpty(courseName) ? "Bundle" : "Course") + " not found"
//        };

//        return View("Payment/_PurchaseItem", token);
//    }

//    //block subscription if price not defined
//    if (buySubscription && dto.MonthlySubscriptionPrice == null)
//    {
//        buySubscription = false;
//    }


//    var itemState = !String.IsNullOrEmpty(courseName) ? _courseServices.GetCourseAccessState4User(CurrentUserId, dto.ItemId) : _courseServices.GetBundleAccessState4User(CurrentUserId, dto.ItemId);

//    dto.IsPurchased      = itemState.IsAccessAllowed || itemState.IsOwnedByUser;
//    dto.TrackingID       = trackingID;
//    dto.BuySubscription  = buySubscription;                        
//    dto.BillingAddresses = _userAccountServices.GetUserBillingAddresses(CurrentUserId);
//    dto.UserSavedCards   = _userAccountServices.GetUserSavedCardsLOV(CurrentUserId);

//    EventLoggerService.Report(CurrentUserId, CommonEnums.eUserEvents.BUY_PAGE_ENTERED, Session.SessionID, string.Format("item ID: {0}, itemType {1}", dto.ItemId, String.IsNullOrEmpty(courseName) ? "Bundle" : "Course"), trackingID, String.IsNullOrEmpty(courseName) ? (int?) null : dto.ItemId, String.IsNullOrEmpty(courseName) ? dto.ItemId : (int?) null);

//    return View("Payment/_PurchaseItem", dto);
//}
//public ActionResult FreeCourseRedirect(string trackingID, string authorName, string courseUrlName, int courseId)
//{
//    var result = false;
//    if (CurrentUserId > 0)
//    {
//        string error;
//        int orderNo;
//        result = _billingServices.CompleteFreeCourseRequest(courseId, CurrentUserId, trackingID, out orderNo, out error);
//    }
//    else
//    {
//        ViewBag.ErrorMessage = "Please login first, in order to view the free content.";
//    }

//    if (result)
//    {
//        return Redirect(Url.GenerateCoursePageUrl(authorName, courseUrlName, trackingID));
//    }


//    return View("~/Areas/Widget/Views/Shared/Error.cshtml");
//}
//public ActionResult ThankYou(Guid? requestId)
//{
//    return View();
//}

//public ActionResult ThankYouStore(int isSuccess, string message, string returnUrl)
//{
//    var token = new PaypalCompleteRequestToken
//    {
//        IsSuccess = Convert.ToBoolean(isSuccess)
//        ,
//        Message = message
//        ,
//        RedirectUrl = returnUrl
//    };

//    return View(token);
//}
#endregion