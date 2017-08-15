using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class BillingEntityMapper
    {
        #region payment instruments
        public static USER_PaymentInstruments UserIdToUserPrefrenceEntityWithCreditCard(this int userId,Guid  instrumentId,int addressId,string card_token,string lastDigits,string cardType)
        {
            return new USER_PaymentInstruments
                {
                     InstrumentId    = instrumentId
                    ,UserId          = userId
                    ,AddressId       = addressId
                    ,PaypalCcToken   = card_token
                    ,PaymentMethodId = (byte)BillingEnums.ePaymentMethods.Credit_Card
                    ,DisplayName     = String.Format("****{0}",lastDigits)
                    ,CreditCardType  = cardType
                    ,IsActive        = true
                    ,AddOn           = DateTime.Now
                    ,CreatedBy       = DtoExtensions.CurrentUserId
                };
        }
        
        public static void UpdateCardToken(this USER_PaymentInstruments entity, string card_token,bool isActive)
        {
            entity.PaypalCcToken = card_token;
            entity.IsActive      = isActive;
            entity.UpdateDate    = DateTime.Now;
            entity.UpdatedBy     = DtoExtensions.CurrentUserId;
        }

        public static USER_PaymentInstruments UserIdToUserPrefrenceEntityWithAgreementToken(this int userId, string agreement_token)
        {
            return new USER_PaymentInstruments
                {
                     InstrumentId         = Guid.NewGuid()
                    ,UserId               = userId
                    ,PaypalAgreementToken = agreement_token
                    ,PaymentMethodId      = (byte)BillingEnums.ePaymentMethods.Paypal
                    ,IsActive             = true
                    ,AddOn                = DateTime.Now
                    ,CreatedBy            = DtoExtensions.CurrentUserId
                };
        }

        public static void UpdateAgreementToken(this USER_PaymentInstruments entity, string agreement_token)
        {
            entity.PaypalAgreementToken = agreement_token;            
            entity.UpdateDate           = DateTime.Now;
            entity.UpdatedBy            = DtoExtensions.CurrentUserId;
        }
        #endregion
        
        #region address
        public static USER_Addresses AddressDto2UserAddressEntity(this BillingAddressDTO dto)
        {
            return new USER_Addresses
            {
                 UserId      =  dto.UserId
                ,CountryId   = dto.CountryId
                ,StateId     = dto.StateId
                ,CityName    = dto.City.TrimString()
                ,PostalCode  = dto.PostalCode.TrimString()
                ,Street1     = dto.Street1.TrimString()
                ,Street2     = dto.Street2.TrimString()
                ,FirstName   = dto.BillingFirstName.TrimString()
                ,LastName    = dto.BillingLastName.TrimString()
                ,IsActive    = true
                ,IsDefault   = dto.IsDefault
                ,AddOn       = DateTime.Now
                ,CreatedBy   = DtoExtensions.CurrentUserId
            };
        }

        public static void UpdateAddressEntity(this USER_Addresses entity, BillingAddressDTO dto)
        {
            entity.CountryId  = dto.CountryId;
            entity.StateId    = dto.StateId;
            entity.CityName   = dto.City.TrimString();
            entity.PostalCode = dto.PostalCode.TrimString();
            entity.Street1    = dto.Street1.TrimString();
            entity.Street2    = dto.Street2.TrimString();
            entity.FirstName  = dto.BillingFirstName.TrimString();
            entity.LastName   = dto.BillingLastName.TrimString();
            entity.IsActive   = dto.IsActive;
            entity.IsDefault  = dto.IsDefault;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
        #endregion

        #region paypal
        public static PAYPAL_IpnLogs Response2PaypalIpnLog(this IpnResponseToken token)
        {
            return new PAYPAL_IpnLogs
                        {
                            AddOn                   = DateTime.Now
                            ,response               = token.response_string
                            ,recurring_payment_id   = token.recurring_payment_id
                            ,amount                 = token.amount
                            ,mc_fee                 = token.mc_fee
                            ,mc_gross               = token.mc_gross
                            ,initial_payment_txn_id = token.initial_payment_txn_id
                            ,initial_payment_amount = token.initial_payment_amount
                            ,parent_txn_id          = token.parent_txn_id
                            ,txn_id                 = token.txn_id
                            ,txn_type               = token.txn_type
                        };
        }
        #endregion

        #region pricing
        public static BILL_ItemsPriceList Token2PriceLineEntity(this PriceLineDTO token)
        {
            return new BILL_ItemsPriceList
            {
                 ItemId           = token.ItemId
                ,ItemTypeId       = (byte)token.ItemType
                ,PriceTypeId      = (byte)token.PriceType
                ,PeriodTypeId     = token.PeriodType != null ? (byte)token.PeriodType : (byte?)null
                ,Price            = token.Price
                ,CurrencyId       = token.Currency.CurrencyId
                ,NumOfPeriodUnits = token.NumOfPeriodUnits
                ,Name             = token.ToPriceLineName()
                ,IsDeleted        = false
                ,AddOn            = DateTime.Now
                ,CreatedBy        = DtoExtensions.CurrentUserId
            };
        }

        public static void UpdatePriceLine(this BILL_ItemsPriceList entity, decimal? price,bool? isDeleted )
        {
            if(price!=null)  entity.Price = (decimal) price;
            
            if (isDeleted != null) entity.IsDeleted = (bool)isDeleted;

            entity.UpdateOn  = DateTime.Now;
            entity.UpdatedBy = DtoExtensions.CurrentUserId;
        }

        public static BILL_ItemsPriceRevisions ToBillItemsPriceRevision(this int lineId, decimal price)
        {
            return new BILL_ItemsPriceRevisions
            {
                 PriceLineId = lineId
                ,Price      = price
                ,IsDeleted  = false
                ,FromDate   = DateTime.Now
                ,CreatedBy  = DtoExtensions.CurrentUserId
            };
        }

        public static void UpdatePriceRevision(this BILL_ItemsPriceRevisions entity, bool isDeleted)
        {
            entity.IsDeleted = isDeleted;
            entity.ToDate    = DateTime.Now;
            entity.UpdatedBy = DtoExtensions.CurrentUserId;
        }
        #endregion

        #region Refunds
        
        public static void UpdateRefundStatus(this SALE_RefundRequests entity, int userId, BillingEnums.eRefundRequestStatus status)
        {
            entity.UpdatedBy = userId;
            entity.LastUpdate = DateTime.Now;
            entity.StatusId = (byte)status;
        }
        
        #endregion
    }
}

//#region user subscriptions
//public static BILL_UserSubscriptions PaypalRequestToken2UserSubscriptionEntity(this PAYPAL_PaymentRequests entity, RecurringPaymentExecutionResultToken token,Guid subscriptionId)
//{
//    return new BILL_UserSubscriptions
//    {
//        SubscriptionId          = subscriptionId
//        ,UserId                 = entity.UserId
//        ,CourseId               = entity.CourseId
//        ,PaypalProfileID        = token.PaypalProfileId
//        ,StartDate              = DateTime.Now.Date
//        ,FirstBillingDate       = token.FirstBillingDate
//        //TODO check behavior
//        //,EndDate                = DateTime.Now.Date.AddYears(1)
//        ,PaymentMethodId        = entity.PaymentMethodId
//        ,Amount                 = token.BillingAmount
//        ,InitialAmount          = token.InitialAmount
//        ,PaymentInstrumentId    = entity.InstrumentId
//        ,Description            = token.ProfileDescription
//        ,StatusId               = (byte)BillingEnums.eSubscriptionStatuses.ACTIVE
//        ,TotalPaymentsCompleted = 0
//        ,NextPaymentNum         = 1
//        ,AddOn                  = DateTime.Now
//        ,CreatedBy              = DtoExtensions.CurrentUserId
//    };
//}

//public static BILL_UserSubscriptions SubscriptionDto2BillUserSubscription(this SubscriptionWithSavedCardDTO token, Guid subscriptionId)
//{
//    return new BILL_UserSubscriptions
//    {
//        SubscriptionId          = subscriptionId
//        ,UserId                 = token.UserId
//        ,CourseId               = token.courseId
//        ,StartDate              = token.InitialPaymentDate
//        ,FirstBillingDate       = token.FirstBillingDate
//        //TODO check behavior
//        //,EndDate             = DateTime.Now.Date.AddYears(1)
//        ,PaymentMethodId        = (byte)BillingEnums.ePaymentMethods.Saved_Instrument
//        ,Amount                 = token.BillingAmount
//        ,InitialAmount          = token.InitialAmount
//        ,PaymentInstrumentId    = token.PaymentInstrumentId
//        ,Description            = token.ProfileDescription
//        ,StatusId               = (byte)BillingEnums.eSubscriptionStatuses.ACTIVE
//        ,TotalPaymentsCompleted = 0
//        ,NextPaymentNum         = 1
//        ,AddOn                  = DateTime.Now
//        ,CreatedBy              = DtoExtensions.CurrentUserId
//    };
//}

//public static void UpdateSubscription(this BILL_UserSubscriptions entity, DateTime paymentDate, int? totalPayments, int? nextPaymentNum, DateTime? nextBillingDate, BillingEnums.eSubscriptionStatuses? status, int? userId)
//{
//    if (totalPayments != null) entity.TotalPaymentsCompleted = (short)totalPayments;

//    if (nextPaymentNum != null) entity.NextPaymentNum = (short)nextPaymentNum;

//    if (status != null) entity.StatusId = (byte)status;

//    if (userId != null) entity.UpdatedBy = userId;

//    if (nextBillingDate != null) entity.NextBillingDate = nextBillingDate;

//    entity.LastPaymentDate = paymentDate;

//    entity.UpdateDate = DateTime.Now;

//}


//public static void UpdateSubscriptionStatus(this BILL_UserSubscriptions entity, BillingEnums.eSubscriptionStatuses status, int? userId)
//{
//    entity.StatusId = (byte)status;

//    if (userId != null && userId > 0) entity.UpdatedBy = userId;            

//    entity.UpdateDate = DateTime.Now;

//}

//public static BILL_UserSubscriptionPayments ToUserSubscriptionPayment(this Guid subscriptionId,short paymentNum,decimal amount,DateTime scheduledDate,DateTime? paymentDate,BillingEnums.eSubscriptionPaymentStatses status)
//{
//    return  new BILL_UserSubscriptionPayments
//                        {
//                            SubscriptionId = subscriptionId
//                            ,Number        = paymentNum
//                            ,Amount        = amount
//                            ,ScheduledDate = scheduledDate
//                            ,CompletedOn   = paymentDate
//                            ,StatusId      = (byte)status
//                            ,AddOn         = DateTime.Now
//                        };
//}

//public static void UpdateSubscriptionPaymentState(this BILL_UserSubscriptionPayments entity,DateTime? completedOn,BillingEnums.eSubscriptionPaymentStatses? status)
//{
//    if (completedOn != null)    entity.CompletedOn = completedOn;
//    if (status != null)         entity.StatusId = (byte)status;

//    entity.UpdateDate = DateTime.Now;
//}

//#endregion

//#region user transactions
//public static UserTransactions PaypalRequestEntity2UserTransaction(this PAYPAL_PaymentRequests entity,BillingEnums.eTransactionTypes type,int? storeId ,int? couponInstanceId)
//{
//    return new UserTransactions
//    {
//        RefRequestId          = entity.RequestId,
//        TransactionUser       = entity.UserId,
//        TransactionType       = (int)type,
//        TransactionDate       = DateTime.Now,
//        ExternalTransactionID = entity.TransactionId,
//        TransactionAmount     = entity.Amount ?? 0,
//        TransactionCourse     = entity.CourseId,
//        CouponInstanceId      = couponInstanceId,
//        AddressId             = entity.AddressId,
//        InstrumentId          = entity.InstrumentId,
//        WebStoreID            = storeId,
//        AddOn                 = DateTime.Now

//    };
//}

//public static UserTransactions PaypalRequestEntity2UserTransaction(this PAYPAL_PaymentRequests entity, BillingEnums.eTransactionTypes type, decimal amount, int? storeId, int? couponInstanceId)
//{
//    return new UserTransactions
//    {
//        RefRequestId                 = entity.RequestId,                
//        TransactionUser              = entity.UserId,
//        TransactionType              = (int)type,
//        TransactionDate              = DateTime.Now,
//        ExternalTransactionID        = entity.TransactionId,
//        TransactionAmount            = amount,
//        TransactionCourse            = entity.CourseId,
//        CouponInstanceId             = couponInstanceId,
//        AddressId                    = entity.AddressId,
//        InstrumentId                 = entity.InstrumentId,
//        WebStoreID                   = storeId,
//        AddOn                        = DateTime.Now
//    };
//}

//public static UserTransactions SubscriptionEntity2UserTransactions(this PAYPAL_PaymentRequests entity, Guid requestId, BillingEnums.eTransactionTypes type, string trxId, decimal amount, long? paymentId, int? storeId, int? couponInstanceId,decimal fee = 0)
//{
//    return new UserTransactions
//    {
//        RefRequestId          = requestId,
//        SubscriptionPaymentId = paymentId,
//        TransactionUser       = entity.UserId,
//        TransactionType       = (int) type,
//        TransactionDate       = DateTime.Now,
//        ExternalTransactionID = trxId,
//        TransactionAmount     = amount,
//        Fee                   = fee,
//        TransactionCourse     = entity.CourseId,
//        CouponInstanceId      = couponInstanceId,
//        AddressId             = entity.AddressId,
//        WebStoreID            = storeId,
//        AddOn                 = DateTime.Now
//    };
//}

//public static UserTransactions PaypalRequestEntity2UserRefundTransactions(this UserTransactions trxEntity, PAYPAL_PaymentRequests requestEntity, Guid requestId, string trxId, decimal amount, decimal fee = 0)
//{
//    return new UserTransactions
//    {
//        RefRequestId          = requestId,
//        SubscriptionPaymentId = trxEntity.SubscriptionPaymentId,
//        TransactionUser       = requestEntity.UserId,
//        TransactionType       = (int)(trxEntity.TransactionAmount==amount ? BillingEnums.eTransactionTypes.Refund : BillingEnums.eTransactionTypes.PartialRefund),
//        TransactionDate       = DateTime.Now,
//        ExternalTransactionID = trxId,
//        TransactionAmount     = amount*(-1), 
//        Fee                   = fee,
//        TransactionCourse     = requestEntity.CourseId,
//        CouponInstanceId      = trxEntity.CouponInstanceId,
//        AddressId             = trxEntity.AddressId,
//        WebStoreID            = trxEntity.WebStoreID,
//        RefundedTrxId         = trxEntity.Id,
//        AddOn                 = DateTime.Now
//    };
//}      

//public static UserTransactions SubscriptionWithSavedCardDto2UserTransactions(this SubscriptionWithSavedCardDTO token, Guid requestId, BillingEnums.eTransactionTypes type, string trxId, decimal amount, long? paymentId, int? storeId, int? couponInstanceId, decimal fee = 0)
//{
//    return new UserTransactions
//    {
//        RefRequestId          = requestId,
//        SubscriptionPaymentId = paymentId,
//        TransactionUser       = token.UserId,
//        TransactionType       = (int)type,
//        TransactionDate       = DateTime.Now,
//        ExternalTransactionID = trxId,
//        TransactionAmount     = amount,
//        Fee                   = fee,
//        TransactionCourse     = token.courseId,
//        CouponInstanceId      = couponInstanceId,
//        AddressId             = token.addressId,
//        WebStoreID            = storeId,
//        AddOn                 = DateTime.Now
//    };
//}

//public static void UpdateTransactionRefundAmount(this UserTransactions entity, decimal amount)
//{
//    entity.RefundAmount = entity.RefundAmount + amount;
//    entity.UpdateDate = DateTime.Now;            
//}
//#endregion