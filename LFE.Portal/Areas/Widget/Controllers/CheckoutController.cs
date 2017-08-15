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
    public class CheckoutController : CheckoutBaseController
    {
        //
        // GET: /Widget/Checkout/
        private readonly IWidgetCourseServices _widgetCourseServices;
        private readonly IUserAccountServices _userAccountServices;
        private readonly IPaypalServices _paypalServices;        
        private readonly IBillingServices _billingServices;        
        private readonly ICouponWidgetServices _couponWidgetServices;
        private readonly IGeoServices _geoServices;
        private string _BaseUrl
        {
            get
            {
                return BaseUrl.Remove(BaseUrl.Length - 1);
                
            }
        }
        public CheckoutController()
        {
            _paypalServices       = DependencyResolver.Current.GetService<IPaypalServices>();
            _billingServices      = DependencyResolver.Current.GetService<IBillingServices>();
            _widgetCourseServices = DependencyResolver.Current.GetService<IWidgetCourseServices>();
            _couponWidgetServices = DependencyResolver.Current.GetService<ICouponWidgetServices>();
            _geoServices          = DependencyResolver.Current.GetService<IGeoServices>();
            _userAccountServices  = DependencyResolver.Current.GetService<IUserAccountServices>();
        }

        private ActionResult Redirect2Local(string action,int? orderNo = null)
        {
            var url = action.GenerateCheckoutUrl(CheckoutBase.PriceLineId, orderNo, CheckoutBase.TrackingId, CheckoutBase.Refferal);

            return Redirect(url);
        }
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated) return Redirect2Local("Login");

            if (!CheckoutBase.IsValid) return Redirect2Local("Failure");

            var purchaseToken = _GetPurchaseDataToken(CheckoutBase.PriceLineId, CheckoutBase.TrackingId);

            if (purchaseToken.IsValid)
            {
                CheckoutBase.PurchaseDataToken = purchaseToken;
                return View("Index", CheckoutBase);
            }

            CheckoutBase.IsValid = false;
            CheckoutBase.Message = purchaseToken.Message;

            return View("Failure", CheckoutBase);
        }

        public ActionResult Register()
        {
            if (!CheckoutBase.IsValid) return Redirect2Local("Failure");

            return User.Identity.IsAuthenticated ? Redirect2Local("Index") : View("Register",CheckoutBase);
        }

        public ActionResult Login()
        {
            if (!CheckoutBase.IsValid) return Redirect2Local("Failure");

            return User.Identity.IsAuthenticated ? Redirect2Local("Index") : View("Login",CheckoutBase);
        }

        public ActionResult ForgottenPassword()
        {
            if (!CheckoutBase.IsValid) return Redirect2Local("Failure");

            return User.Identity.IsAuthenticated ?  Redirect2Local("Index"): View("ForgottenPassword",CheckoutBase);
        }

        public ActionResult Success(int? orderNo)
        {
            if (!User.Identity.IsAuthenticated) return Redirect2Local("Login");

            if (!CheckoutBase.IsValid) return Redirect2Local("Failure");

            var purchaseCompleteToken = _GetpPurchaseCompleteToken(orderNo, CheckoutBase.TrackingId);

            if (!purchaseCompleteToken.IsValid) return Redirect2PaymentError(purchaseCompleteToken.Message);

            CheckoutBase.PurchaseCompleteToken = purchaseCompleteToken;
            
            return View("Success", CheckoutBase);
        }

        public ActionResult Failure()
        {
            var error = TempData["CheckoutBaseError"];
            if (error != null)
                CheckoutBase.Message = error.ToString();
            return View(CheckoutBase);
        }

        private ActionResult Redirect2PaymentError(string error)
        {
            TempData["CheckoutBaseError"] = error;

            var url = "Failure".GenerateCheckoutUrl(CheckoutBase.PriceLineId, null, CheckoutBase.TrackingId, CheckoutBase.Refferal);

            return Redirect(url); //View("Failure",CheckoutBase);
        }

        #region posts
        [HttpPost]
        public ActionResult ExecuteItemPurchase(ItemPurchaseDataToken token, BillingAddressDTO address, eActionKinds actionKind,string referral)
        {
            string error;
            var orderNo = -1;
            var user = this.CurrentUser();

            if (user == null) return Redirect2PaymentError("authentication issue. please re-login again");

            try
            {
                if (token.IsFree)
                {
                    if (String.IsNullOrWhiteSpace(token.CouponCode)) return Redirect2PaymentError("coupon code required");

                    token.PaymentMethod = BillingEnums.ePaymentMethods.Charge_Free;

                    var completed = _billingServices.CompleteFreeCouponRequest(token, user.UserId, out orderNo, out error);

                    if (!completed) return Redirect2PaymentError(error);

                    SavePurchaseCompleteEvent(token);

                    return ReturnPurchaseSuccess(token.PriceToken.PriceLineID,orderNo, token.TrackingID,referral);
                }


                var priceToken = _billingServices.GetPriceLineToken(token.PriceToken.PriceLineID, out error);

                if (priceToken == null) return Redirect2PaymentError(error);


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
                var CANCEL_PAYMENT_URL = _BaseUrl + "CancelPayment".GenerateCheckoutUrl(token.PriceToken.PriceLineID, null, token.TrackingID, referral);
                #endregion

                #region handle billing address

                int? addressId = null;
                if (token.PaymentMethod == BillingEnums.ePaymentMethods.Credit_Card)
                {
                    var billingAddressDto = address;

                    if (!IsAddressValid(ref billingAddressDto, out error)) return Redirect2PaymentError(error);

                    token.BillingAddress = billingAddressDto;

                    var addressSaved = _userAccountServices.SaveUserBillingAddress(ref billingAddressDto, out error);

                    if (!addressSaved) return Redirect2PaymentError(error);

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
                        if (token.CreditCard == null) return Redirect2PaymentError("credit card required");

                        if (addressId == null)
                        {
                            return Redirect2PaymentError("Address required");
                        }

                        if (token.SavePaymentInstrument)
                        {
                            //create card token
                            var card = token.CoursePurchaseDataToken2CreditCardDto(this.CurrentUser());

                            //save cc to paypal
                            var cardSaved = _paypalServices.SaveCreditCard2Paypal(card, CurrentUserId, out paymentInstrumentId, out error);

                            if (!cardSaved) return Redirect2PaymentError(error);
                        }
                        Session["RecurringCc"] = token.CreditCard;

                        if (token.BuySubscription) token.PaymentMethod = BillingEnums.ePaymentMethods.Saved_Instrument;

                        break;

                    case BillingEnums.ePaymentMethods.Saved_Instrument:
                        paymentInstrumentId = token.PaymentInstrumentId ?? Guid.Empty;
                        if (paymentInstrumentId.Equals(Guid.Empty)) return Redirect2PaymentError("Select credit card");
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

                                var SUCCESS_PAYMENT_URL = _BaseUrl + "OnPaymentComplete".GenerateCheckoutUrl(token.PriceToken.PriceLineID, null, token.TrackingID, referral);//Url.Action("OnPaymentComplete", "Payment", new { area = "Widget",referral });

                                var data = token.ItemPurchaseDataToken2PayPalCreatePaymentDto(SUCCESS_PAYMENT_URL, CANCEL_PAYMENT_URL, addressId, null);

                                paymentExecuted = _paypalServices.CreatePaypalAccountPayment(data, out approval_url, out error);

                                if (paymentExecuted && !String.IsNullOrEmpty(approval_url)) return Redirect(approval_url);

                                return Redirect2PaymentError(error ?? "unexpected error");
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
                                return paymentExecuted ? ReturnPurchaseSuccess(token.PriceToken.PriceLineID, orderNo, token.TrackingID, referral) : Redirect2PaymentError(error);
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
                                return paymentExecuted ? ReturnPurchaseSuccess(token.PriceToken.PriceLineID, orderNo, token.TrackingID, referral) : Redirect2PaymentError(error); 
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
                                var success_url =  _BaseUrl + "OnSubscriptionComplete".GenerateCheckoutUrl(token.PriceToken.PriceLineID, null, token.TrackingID, referral);//Utils.GetKeyValue("baseUrl") + Url.Action("OnSubscriptionComplete", null, new { id = requestId, referral  });

                                var data = token.CoursePurchaseDataToken2PayPalAgreementDto(user.FullName, user.Email, success_url, CANCEL_PAYMENT_URL, addressId, paymentInstrumentId, token.PaymentMethod, priceToken);


                                //create payment agreement
                                paymentExecuted = _paypalServices.CreateRecurringPaymentAgreement(data, requestId, out approval_url, out error);

                                //redirect to paypal approval page
                                return paymentExecuted && !String.IsNullOrEmpty(approval_url) ? Redirect(approval_url) : Redirect2PaymentError(error ?? "unexpected error");
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

                                if (!paymentExecuted) return Redirect2PaymentError(error);

                                var subscriptionCreated = _billingServices.CreateSubscriptionWithSavedCard(subscriptionToken, requestId, out orderNo, out error);

                                if (!subscriptionCreated) return Redirect2PaymentError(error ?? "unexpected error");
                                
                                SavePurchaseCompleteEvent(token);

                                return ReturnPurchaseSuccess(token.PriceToken.PriceLineID, orderNo, token.TrackingID, referral);
                                #endregion
                        }
                        break;
                }

                CheckoutBase.IsValid = false;
                CheckoutBase.Message = "Unexpected result. Please contact support team";
                CheckoutBase.PurchaseDataToken = token;
                return View("Index", CheckoutBase);
              // return View("Payment/_PurchaseItem", token);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);

                return Redirect2PaymentError(error);

            }
        }

        //[HttpPost]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="id">priceLineId</param>
        /// <param name="trackingId"></param>
        /// <returns></returns>
        public ActionResult PurchaseFreeCourse(int courseId,int? id = null, string trackingId = null)
        {
            var orderNo = -1;
            string error;
            var result = false;

            if (CurrentUserId > 0)
            {
                result = _billingServices.CompleteFreeCourseRequest(courseId, CurrentUserId,id ?? -1, trackingId, out orderNo, out error);
            }
            else
            {
                error = "please login first";
            }

            if(!result ) return Redirect2PaymentError(error);

            var course = _widgetCourseServices.GetCourseToken(Constants.DEFAULT_CURRENCY_ID, courseId, trackingId);

            var token = new ItemPurchaseDataToken
            {
                ItemId      = courseId
                ,Type       = BillingEnums.ePurchaseItemTypes.COURSE
                ,ItemName   = course != null ? course.CourseName : "Free course"
                ,TrackingID = trackingId
            };

            SavePurchaseCompleteEvent(token);

            return Redirect2Local("Success", orderNo);
        }

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
        #endregion

        #region helper actions
        public ActionResult OnPaymentComplete()
        {
            Guid requestId;
            var referral = Request.QueryString["ref"];

            if (!Guid.TryParse(Request.QueryString["requestId"], out requestId)) return Redirect2PaymentError("RequestId required");

            if (String.IsNullOrEmpty(Request.QueryString["PayerID"])) return Redirect2PaymentError("PayerID required");

            string error;
            var orderNo = -1;

            var baseInfo = _paypalServices.GetItemInfoFromPaymentRequest(requestId);

            if (!baseInfo.IsValid) return Redirect2PaymentError("Request not found");

            var executed = _paypalServices.ExecutePayPalPayment(requestId, Request.QueryString["PayerID"], out error, Session.SessionID) && _billingServices.CompletePaymentRequest(requestId, out orderNo, out error);


            CheckoutBase.IsValid = executed;
            CheckoutBase.Message = error;

            //return executed ? ReturnPurchaseSuccess(baseInfo.PriceLineId, orderNo, baseInfo.TrackingId, referral) : View("Failure", CheckoutBase);
            return RedirectToAction("PurchaseResult", new
            {
                isValid    = executed,
                actionKind = eActionKinds.POST,
                id         = baseInfo.PriceLineId,
                itemName   = baseInfo.ItemName,
                trackingId = baseInfo.TrackingId,
                orderNo,
                error,
                referral
            });
        }

        public ActionResult OnSubscriptionComplete(Guid? requestId)
        {
            var referral = Request.QueryString["ref"];
            string error;
            var reqId = requestId;

            if (reqId == null) return Redirect2PaymentError("requestId missing");

            var baseInfo = _paypalServices.GetItemInfoFromPaymentRequest((Guid)reqId);

            if (!baseInfo.IsValid) return Redirect2PaymentError("Request not found");

            if (String.IsNullOrEmpty(Request.QueryString["TOKEN"]))
            {
                _paypalServices.UpdatePaypalRequestStatus((Guid)reqId, BillingEnums.ePaymentRequestStatus.failed, out error);

                return Redirect2PaymentError("token missing");
            }

            var agreement_token = Request.QueryString["TOKEN"];

            CreditCardDTO card = null;

            if (Session["RecurringCc"] != null)
            {
                card = Session["RecurringCc"] as CreditCardDTO;
            }

            RecurringPaymentExecutionResultToken result;

            var orderNo = -1;

            var executed = _paypalServices.ExecuteCourseSubscriptionPayalRecurringPayment(CurrentUserId, (Guid)reqId, agreement_token, card, out result, out error, Session.SessionID) && _billingServices.CompleteSubscriptionRequest(result, out orderNo, out error);

            //if (executed) return ReturnPurchaseSuccess(baseInfo.PriceLineId, orderNo, baseInfo.TrackingId, referral);

            //CheckoutBase.Message = error;

            //return View("Failure", CheckoutBase);

            return RedirectToAction("PurchaseResult", new
            {
                isValid    = executed,
                actionKind = eActionKinds.POST,
                id         = baseInfo.PriceLineId,
                itemName   = baseInfo.ItemName,
                trackingId = baseInfo.TrackingId,
                orderNo,
                error,
                referral
            });
           
        }
        public ActionResult CancelPayment(string referral)
        {
            //CheckoutBase.Message = "Payment canceled";
            //return View("Failure",CheckoutBase);

            return RedirectToAction("PurchaseResult", new
            {
                isValid    = false,
                actionKind = eActionKinds.POST,
                id         = CheckoutBase.PriceLineId,
                itemName   = CheckoutBase.ItemInfo.ItemName,
                trackingId = CheckoutBase.TrackingId,
                orderNo    = -1,
                error = "Payment canceled",
                referral
            });
        }
        private ActionResult ReturnPurchaseSuccess(int priceLineId, int orderNo, string trackingId, string referral)
        {
            var url = "Success".GenerateCheckoutUrl(priceLineId, orderNo, trackingId, referral);

            return Redirect(url);
        }

        public ActionResult PurchaseResult(bool isValid, eActionKinds actionKind, int? orderNo, int? id, string itemName, string trackingId, string error, string referral)
        {
            var token = new PurchaseResultToken
            {
                IsValid      = isValid
                ,ActionKind  = actionKind 
                ,ItemName    = itemName
                ,PriceLineId = id ?? -1
                ,Message     = error
                ,RedirectUrl = (isValid ? "Success" :"Failure").GenerateCheckoutUrl(id?? -1,orderNo ?? -1, trackingId,referral) 
            };

            return View(token);
        }

        public ActionResult PurchaseErrorResult(int? lineId,string itemName,string trackingId,string error)
        {
            CheckoutBase.Message = error;
            return View("Failure",CheckoutBase);
        }
        #endregion

        #region private helpers
        private void SavePurchaseCompleteEvent(ItemPurchaseDataToken token)
        {
            SaveUserEvent(CommonEnums.eUserEvents.PURCHASE_COMPLETE, String.Format("{0} purchased", token.ItemName), token.TrackingID, token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?)null, token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?)null);
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

        private ItemPurchaseDataToken _GetPurchaseDataToken(int? id, string trackingId)
        {
           

             if (id == null)
            {
                var token = new ItemPurchaseDataToken
                {
                    IsValid = false
                    ,Message = "Required param ID missing"
                };

                return token;
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

                return  token;
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

                    return token;
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

            return dto;
        }

        private ItemPurchaseCompleteToken _GetpPurchaseCompleteToken(int? id, string trackingId)
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

            return token;
        }
        #endregion
    }
}
