using System;
using LFE.Application.Services.Helper;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Helpers;

namespace LFE.Portal.Areas.Widget.Helpers
{
    public static class PaymentExtensions
    {
        public static PayPalCreatePaymentDTO ItemPurchaseDataToken2PayPalCreatePaymentDto(this ItemPurchaseDataToken token, string SUCCESS_PAYMENT_URL, string CANCEL_PAYMENT_URL,int? addressId,Guid? instrumentId)
        {
            return new PayPalCreatePaymentDTO
                        {
                            Type                 = token.Type
                            ,cancel_url          = CANCEL_PAYMENT_URL
                            ,success_url         = SUCCESS_PAYMENT_URL
                            ,currency            = token.PriceToken.Currency.ISO //"USD"
                            ,priceLineId         = token.PriceToken.PriceLineID
                            ,amount              = token.Price
                            ,courseId            = token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?) null
                            ,bundleId            = token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?) null
                            ,couponCode          = token.CouponCode
                            ,trackingId          = token.TrackingID
                            ,addressId           = addressId
                            ,paymentInstrumentId = instrumentId
                            ,description         = token.ItemName.ItemName2CoursePurchaseDescription()
                        };
        }

        public static PaypalCreditCardPaymentDTO CoursePurchaseDataToken2PayPalDirectCcPaymentDto(this ItemPurchaseDataToken token, UserDTO user, int? addressId, Guid? instrumentId)
        {
            return new PaypalCreditCardPaymentDTO
                        {
                            Type                 = token.Type
                            ,courseId            = token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?) null
                            ,bundleId            = token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?) null
                            ,couponCode          = token.CouponCode
                            ,currency            = token.PriceToken.Currency.ISO //"USD"
                            ,priceLineId         = token.PriceToken.PriceLineID
                            ,amount              = token.Price                            
                            ,trackingId          = token.TrackingID
                            ,addressId           = addressId
                            ,paymentInstrumentId = instrumentId
                            ,card                = token.CoursePurchaseDataToken2CreditCardDto(user)
                            ,description         = token.ItemName.ItemName2CoursePurchaseDescription()
                           
                        };
        }

        public static CreditCardDTO CoursePurchaseDataToken2CreditCardDto(this ItemPurchaseDataToken token, UserDTO user)
        {
            return new CreditCardDTO
                        {
                            Type            = token.CreditCard.Type
                            ,CardNumber     = token.CreditCard.CardNumber.TrimString().Replace(" ",string.Empty)
                            ,CVV2           = token.CreditCard.CVV2.TrimString()
                            ,ExpireMonth    = token.CreditCard.ExpireMonth
                            ,ExpireYear     = token.CreditCard.ExpireYear
                            ,FirstName      = token.CreditCard.FirstName ?? user.FirstName
                            ,LastName       = token.CreditCard.FirstName ?? user.LastName
                            ,BillingAddress = token.BillingAddress
                        };
        }

        public static PayPalAgreementDTO CoursePurchaseDataToken2PayPalAgreementDto(this ItemPurchaseDataToken token, string buyerName, string buyerEmail, string SUCCESS_PAYMENT_URL, string CANCEL_PAYMENT_URL, int? addressId, Guid? instrumentId, BillingEnums.ePaymentMethods paymentMethod, PriceLineDTO priceToken)
        {
            return new PayPalAgreementDTO
                        {
                            cancel_url           = CANCEL_PAYMENT_URL
                            ,success_url         = SUCCESS_PAYMENT_URL
                            ,currency            = token.PriceToken.Currency.ISO //"USD"
                            ,priceLineId         = token.PriceToken.PriceLineID
                            ,amount              = token.Price
                            ,Type                = token.Type
                            ,courseId            = token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?) null
                            ,bundleId            = token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?) null
                            ,couponCode          = token.CouponCode
                            ,trackingId          = token.TrackingID
                            ,addressId           = addressId
                            ,paymentInstrumentId = instrumentId
                            ,description         = token.ItemName.ItemName2SubscriptionDescription()
                            ,method              = paymentMethod
                            ,buyerEmail          = buyerEmail
                            ,buyerName           = buyerName
                            ,Currency            = priceToken.Currency
                        };
        }

        public static PaypalPaymentRequestDTO CoursePurchaseDataToken2PaypalPaymentRequestDto(this ItemPurchaseDataToken token,Guid requestId,int userId, int? addressId)
        {
            return new PaypalPaymentRequestDTO
            {
                ReuqstId               = requestId
                ,UserId                = userId
                ,PaymentMethod         = BillingEnums.ePaymentMethods.Credit_Card
                ,PaypalRequestType     = BillingEnums.ePaypalRequestTypes.RECURRING_PAYMENT_EXECUTION
                ,Amount                = token.Price
                ,Type                  = token.Type
                ,CourseId              = token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?) null
                ,BundleId              = token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?) null
                ,TrackingID            = token.TrackingID
                ,AddressId             = addressId
                ,CouponCode            = token.CouponCode
            };
        }

        public static SubscriptionWithSavedCardDTO CoursePurchaseDataToken2SubscriptionWithSavedCardDto(this ItemPurchaseDataToken token,int userId, Guid instrumentId, int? addressId)
        {
            return new SubscriptionWithSavedCardDTO
            {
                 UserId                = userId
                ,BillingAmount         = token.Price
                ,InitialAmount         = token.Price.CalculateInitialCourseMonthlySubscriptionAmount()
                ,currency              = token.PriceToken.Currency.ISO //"USD"
                ,priceLineId           = token.PriceToken.PriceLineID
                ,Type                  = token.Type
                ,courseId              = token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.ItemId : (int?) null
                ,bundleId              = token.Type == BillingEnums.ePurchaseItemTypes.BUNDLE ? token.ItemId : (int?) null
                ,PaymentInstrumentId   = instrumentId
                ,trackingId            = token.TrackingID
                ,addressId             = addressId
                ,couponCode            = token.CouponCode
                ,ProfileDescription    = token.ItemName.ItemName2SubscriptionDescription()                
                ,InitialPaymentDate    = DateTime.Now
                ,FirstBillingDate      = DateTime.Now.ToBillingStartDate()
            };
        }

        public static string ItemPurchaseToken2ItemPageUrl(this ItemPurchaseDataToken token)
        {
            return token.Type == BillingEnums.ePurchaseItemTypes.COURSE ? token.GenerateCourseFullPageUrl(token.Author.fullName, token.ItemName, token.TrackingID) : token.GenerateBundleFullPageUrl(token.Author.fullName, token.ItemName, token.TrackingID);
        }
    }
}