
using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    //paypal
    public interface IPaypalPaymentRequestsRepository : IRepository<PAYPAL_PaymentRequests>
    {

    }
    public interface IPaypalIpnLogRepository : IRepository<PAYPAL_IpnLogs>
    {

    }
    
    //billing
    public interface IOrderRepository : IRepository<SALE_Orders>
    {
        IEnumerable<vw_SALE_Orders> SearchOrders(DateTime from, DateTime to, int? buyerId = null, int? sellerId = null, int? courseId = null, int? bundleId = null, int? storeId = null, bool? isSubscription = null, BillingEnums.eOrderStatuses? status = null); 
    }

    public interface IOrderLineRepository : IRepository<SALE_OrderLines>
    {
        string FindLineCurrencyISO(int lineId);
    }

    public interface IOrderLinePaymentRepository : IRepository<SALE_OrderLinePayments>
    {

    }

    public interface IOrderLinePaymentRefundsRepository : IRepository<SALE_OrderLinePaymentRefunds>
    {

    }

    public interface ITransactionRepository : IRepository<SALE_Transactions>
    {
        IEnumerable<vw_SALE_Transactions> SearchTransactions(DateTime @from, DateTime to, int? sellerUserId, int? buyerUserId, int? courseId, int? bundleId, int? trxTypeId);
    }


    public interface IOrdersViewRepository : IGetRepository<vw_SALE_Orders> { }

    public interface IOrderLinesViewRepository : IGetRepository<vw_SALE_OrderLines>
    {
        IEnumerable<SALE_OrderLineViewToken> SearchOrderLines(DateTime from, DateTime to,
                                                   int? sellerId                          = null,
                                                   int? buyerId                           = null,
                                                   int? storeOwnerId                      = null,
                                                   int? courseId                          = null,
                                                   int? bundleId                          = null,
                                                   int? storeId                           = null,
                                                   BillingEnums.eOrderLineTypes? lineType = null
                                                   ); 
    }

    public interface IOrderLinePaymentsViewRepository : IGetRepository<vw_SALE_OrderLinePayments> { }

    public interface IOrderLinePaymentRefundsViewRepository : IGetRepository<vw_SALE_OrderLinePaymentRefunds> { }

    public interface ITransactionsViewRepository : IGetRepository<vw_SALE_Transactions> { }

    //pricing
    public interface IPriceListRepository : IRepository<BILL_ItemsPriceList>
    {

    }
    public interface IPriceRevisionsReposiotry : IRepository<BILL_ItemsPriceRevisions>
    {

    }
    public interface ICurrencyRepository : IRepository<BASE_CurrencyLib>
    {

    }

    public interface IRefundRequestsRepository : IRepository<SALE_RefundRequests>
    {

    }

    //payout
    public interface IUserPayoutStatmentsRepository : IRepository<PO_UserPayoutStatments>
    {
        IEnumerable<PO_PayoutStatmentToken> GetPayoutStatments(int executionId);
    }

    public interface IPayoutExecutionsRepository : IRepository<PO_PayoutExecutions>
    {
        IEnumerable<PO_PayoutExecutionToken> GetPayoutExecutions(int? executionId);
    }
}
