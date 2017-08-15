using System;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class PaypalEntityMapper
    {
        #region paypal

        public static PAYPAL_PaymentRequests Dto2PaypalPaymentRequestEntity(this PaypalPaymentRequestDTO token)
        {
            return new PAYPAL_PaymentRequests
                {
                     RequestId             = token.ReuqstId
                    ,UserId                = token.UserId
                    ,RequestTypeId         = (byte)token.PaypalRequestType
                    ,PaymentMethodId       = (byte)token.PaymentMethod
                    ,PriceLineId           = token.PriceLineId
                    ,Status                = BillingEnums.ePaymentRequestStatus.init.ToString()
                    ,CreatePaymentId       = token.CreatePaymentId
                    ,RecurringRequestToken = token.RecurringRequestToken
                    ,CourseId              = token.CourseId
                    ,BundleId              = token.BundleId
                    ,TrackingID            = token.TrackingID
                    ,AddressId             = token.AddressId
                    ,InstrumentId          = token.PaymentInstrumentId
                    ,CouponCode            = token.CouponCode
                    ,Amount                = token.Amount
                    ,AddOn                 = DateTime.Now
                    ,CreatedBy             = DtoExtensions.CurrentUserId
                    ,SourceRequestId       = token.SourceReuqstId
                };
        }

        public static void EntityUpdateExecutionPaymentId(this PAYPAL_PaymentRequests entity, string payment_id = null, string transactionId = null)
        {
            entity.ExecutionPaymentId = payment_id;
            entity.TransactionId      = transactionId;
            entity.UpdateDate         = DateTime.Now;
            entity.UpdatedBy          = DtoExtensions.CurrentUserId;
        }

        public static void EntityUpdateReccuringRequestToken(this PAYPAL_PaymentRequests entity, string token)
        {
            entity.RecurringRequestToken = token;
            entity.UpdateDate            = DateTime.Now;
            entity.UpdatedBy             = DtoExtensions.CurrentUserId;
        }

        public static void EntityUpdateRequestError(this PAYPAL_PaymentRequests entity, string error)
        {            
            entity.Status     = BillingEnums.ePaymentRequestStatus.failed.ToString();
            entity.Error      = error;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }

        public static void EntityUpdateRequestStatus(this PAYPAL_PaymentRequests entity, BillingEnums.ePaymentRequestStatus status)
        {
            entity.Status     = status.ToString();
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
        #endregion       
    }
}
