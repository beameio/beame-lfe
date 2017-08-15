using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.DataTokens;
using PayPal.PayPalAPIInterfaceService.Model;

namespace LFE.Application.Services.Interfaces
{
    public interface IPaypalServices:IDisposable
    {
        //REST
        bool CreatePaypalAccountPayment(PayPalCreatePaymentDTO dto, out string approval_url, out string error);
        bool ExecutePayPalPayment(Guid requestId, string payerId, out string error,string sessionId = null);
        bool ExecuteDirectCreditCardPayment(PaypalCreditCardPaymentDTO dto,out Guid requestId, out string error);
        bool ExecuteSavedCreditCardPayment(PaypalCreditCardPaymentDTO dto, int userId, Guid instrumentId, out Guid requestId, out string error);
        bool ExecuteSubscriptionPaymentWithStoredCreditCard(SubscriptionWithSavedCardDTO dto, out Guid requestId, out string error);
        bool SaveCreditCard2Paypal(CreditCardDTO dto, int userId, out Guid instrumentId, out string error);
        

        //MERCHANT
        bool CreateRecurringPaymentAgreement(PayPalAgreementDTO dto,Guid requestId, out string approval_url, out string error);

        bool ExecuteCourseSubscriptionPayalRecurringPayment(int userId, Guid sourceRequestId, string agreement_token, CreditCardDTO credit_card, out RecurringPaymentExecutionResultToken result, out string error,string sessionId = null);

        bool ExecuteCourseSubscriptionPayalRecurringPayment(int userId, Guid sourceRequestId, string agreement_token, out RecurringPaymentExecutionResultToken result, out string error);

        bool ExecuteCourseSubscriptionCcRecurringPayment(PaypalPaymentRequestDTO dto, int userId, Guid requestId, CreditCardDTO credit_card, out RecurringPaymentExecutionResultToken result, out string error);

        bool ExecuteSubscriptionScheduledPaymentWithStoredCreditCard(int paymentId, out string error);
        bool ExecuteSubscriptionScheduledPaymentWithStoredCreditCard(DateTime? shceduledDate, out string error);

        //COMMON
        bool UpdatePaypalRequestStatus(Guid requestId, BillingEnums.ePaymentRequestStatus status, out string error);
        
        BaseRequestItemInfo GetItemInfoFromPaymentRequest(Guid requestId);
    }

    public interface IPaypalIpnServices : IDisposable
    {
        void HandleIpnResponse(IpnResponseToken response);

        void HandleIpnMasspayResponse(List<MassPaymentItemToken> list);
    }

    public interface IPaypalPaymentServices : IDisposable
    {
        bool ExecuteMassPayment(List<MassPayRequestItemType> list, out string error);
        bool ExecutePayment(PaypalPaymentExecutionToken token, out string payKey, out string error);
    }

    public interface IPaypalManageServies : IDisposable
    {
        PaypalRecurringProfileToken GetSubscriptionProfileDetails(string profileId, out string error);
        List<PaymentInstrumentViewDTO> GetUserPaymentInstruments(int userId);
        bool DeleteCreditCard(Guid paymentInstrumentId, out string error);
        bool RefundPaymentTransaction(RefundOrderLinePaymentDTO token, out int refundId, out string error);
        bool RefundPaymentTransaction(RefundOrderLinePaymentDTO token, out string error);
        bool CancelSubscription(int lineId, out string error);        
        bool UpdateMerchantTrxFees(out int found,out int updated,out string error);
        GetTransactionDetailsResponseType GetMerchantTransactionDetails(string trxId);
       // Sale GetRestSaleDetails(string trxId);
      //  Payment GetRestPaymentDetails(string paymentId);

        GetTransactionDetailsResponseType GetMerchantTrxDetails(string trxId);
    }

    public interface IBillingServices : IDisposable
    {
        bool SaveBillingAgreementToken(int userId, string token, out string error);        
        bool IsCreditCardExists(int userId);
        bool CompleteFreeCouponRequest(ItemPurchaseDataToken token, int buyerId, out int orderNo, out string error);
        bool CompleteFreeCourseRequest(int courseId, int buyerId, int priceLineId, string trackingId, out int orderNo, out string error);
        bool CompletePaymentRequest(Guid requestId, out int orderNo, out string error);
        bool CompleteSubscriptionRequest(RecurringPaymentExecutionResultToken result_token, out int orderNo, out string error);
        bool CreateSubscriptionWithSavedCard(SubscriptionWithSavedCardDTO token, Guid initialPaymentRequestId, out int orderNo, out string error);
        string GetItemPageUrlFromPaypalRequest(Guid requestId);
        PriceLineDTO GetPriceLineToken(int lineId, out string error);
    }

    public interface IUserSubscriptionsManageServices : IDisposable
    {
    
        List<ScheduledPaymentSummaryToken> SearchPayments(  ReportEnums.ePeriodSelectionKinds periodKind,
                                                            Guid? orderId,
                                                            int? orderNum,
                                                            int? sellerUserId,
                                                            int? buyerUserId,
                                                            int? courseId,
                                                            int? bundleId,
                                                            BillingEnums.ePaymentStatuses? paymentStatus,
                                                            BillingEnums.eOrderStatuses? orderStatus,
                                                            bool? onlySavedCards);
       
    }

    public interface IBillingManageServices : IDisposable
    {
        bool ProcessRefundGRP(GRPRequestToken token, out string error);
        TransactionFiltersLOV GetTransactionFiltersLov();
        List<TransactionSummaryToken> SearchTransactions(ReportEnums.ePeriodSelectionKinds periodKind, int? authorId = null, int? buyerId = null, int? courseId = null, int? bundleId = null, BillingEnums.eTransactionTypes? trxType = null);
        List<OrderDTO> SearchOrders(ReportEnums.ePeriodSelectionKinds periodKind, int? buyerId = null, int? sellerId = null, int? courseId = null, int? bundleId = null, int? storeId = null, bool? isSubscription = null, BillingEnums.eOrderStatuses? status = null);
        List<OrderLineDTO> GetOrderLines(Guid orderId);
        List<LinePaymentDTO> GetOrderLinePayments(int lineId);
        List<PaymentViewDTO> GetCompletedPayments(ReportEnums.ePeriodSelectionKinds periodKind, BillingEnums.ePaymentTypes? type = null, int? orderNum = null);
        List<PaymentViewDTO> GetSellerPayments(int sellerId, int year, int month, short currencyId);
        List<PaymentViewDTO> GetAffiliatePayments(int sellerId, int year, int month, short currencyId);
        List<PaymentViewDTO> GetRefundProgramPayments(int sellerId, int year, int month, short currencyId, bool released);
        List<RefundViewDTO> GetSellerRefunds(int sellerId, int year, int month, short currencyId);
        List<PaymentRefundDTO> GetOrderLinePaymentRefunds(int paymentId);
        List<TransactionSummaryToken> GetOrderLineTransactions(int lineId);
        RefundOrderLinePaymentDTO GetPaymentRefundDTO(int? paymentId);
        MonthlyStatementDTO GetAuthorMonthlyStatement(AuthorStatementRequestToken request);

        MonthlyStatmentPrintToken GetAuthorMonthlyStatementsPrintToken(AuthorStatementRequestToken request);
        bool SendAuthorMonthlyStatement(MonthlyStatementDTO token, string messageBody,out string error);
        bool BlockUserCourseAccessAndCancelOrderOnRefund(RefundOrderLinePaymentDTO token, out string error);
        bool BlockUserCourseAccessAndCancelOrderOnSubscriptionCancel(int lineId, out string error);
        bool BlockUserCourseAccess(int lineId,bool isSubscription, out string error);

        //custom trx
        CustomTrxDTO GetCustomTrxToken(int? paymentId);
        bool SaveCustomTrx(CustomTrxDTO token, out string error);
    }  
    
}
