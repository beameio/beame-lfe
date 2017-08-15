using System;
using System.Collections.Generic;
using System.Linq;
using LFE.Core.Enums;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    //paypal
    public class PaypalPaymentRequestsRepository : Repository<PAYPAL_PaymentRequests>, IPaypalPaymentRequestsRepository
    {
        public PaypalPaymentRequestsRepository(IUnitOfWork unitOfWork): base(unitOfWork){}
    }

    public class PaypalIpnLogRepository : Repository<PAYPAL_IpnLogs>, IPaypalIpnLogRepository
    {
        public PaypalIpnLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    //billing
    #region sale
    public class OrderRepository : Repository<SALE_Orders>, IOrderRepository
    {
        public OrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<vw_SALE_Orders> SearchOrders(DateTime @from, DateTime to, 
                                                        int? buyerId = null, 
                                                        int? sellerId = null, 
                                                        int? courseId = null,
                                                        int? bundleId = null, 
                                                        int? storeId = null, 
                                                        bool? isSubscription = null,
                                                        BillingEnums.eOrderStatuses? status = null)
        {
            return DataContext.tvf_SALE_SearchOrders(from,to,sellerId,buyerId,courseId,bundleId,storeId,isSubscription?? false,(byte?) status);
        }
    }
    public class OrderLineRepository : Repository<SALE_OrderLines>, IOrderLineRepository
    {
        public OrderLineRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public string FindLineCurrencyISO(int lineId)
        {
            return DataContext.tvf_SALE_GetLineCurrencyISO(lineId).FirstOrDefault();
        }
    }
    public class OrderLinePaymentRepository: Repository<SALE_OrderLinePayments>, IOrderLinePaymentRepository
    {
        public OrderLinePaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
    public class OrderLinePaymentRefundsRepository: Repository<SALE_OrderLinePaymentRefunds>, IOrderLinePaymentRefundsRepository
    {
        public OrderLinePaymentRefundsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
    public class TransactionsRepository : Repository<SALE_Transactions>, ITransactionRepository
    {
        public TransactionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<vw_SALE_Transactions> SearchTransactions(DateTime @from, DateTime to, int? sellerUserId, int? buyerUserId, int? courseId, int? bundleId, int? trxTypeId)
        {
            return DataContext.tvf_SALE_SearchTransactions(from, to, sellerUserId, buyerUserId, courseId,bundleId, trxTypeId);
        }
    }

    public class OrdersViewRepository : Repository<vw_SALE_Orders>, IOrdersViewRepository
    {
        public OrdersViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class  OrderLinesViewRepository : Repository<vw_SALE_OrderLines>, IOrderLinesViewRepository
    {
        public OrderLinesViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<SALE_OrderLineViewToken> SearchOrderLines(DateTime from, DateTime to,
                                                            int? sellerId = null,
                                                            int? buyerId = null,
                                                            int? storeOwnerId = null,
                                                            int? courseId = null,
                                                            int? bundleId = null,
                                                            int? storeId = null,
                                                            BillingEnums.eOrderLineTypes? lineType = null)
        {
            return DataContext.tvf_SALE_SearchOrderLines(from, to, sellerId, buyerId, storeOwnerId, courseId, bundleId, storeId, (byte?)lineType);
        }
    }

    public class OrderLinePaymentsViewRepository: Repository<vw_SALE_OrderLinePayments>, IOrderLinePaymentsViewRepository
    {
        public OrderLinePaymentsViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class OrderLinePaymentRefundsViewRepository : Repository<vw_SALE_OrderLinePaymentRefunds>, IOrderLinePaymentRefundsViewRepository
    {
        public OrderLinePaymentRefundsViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class TransactionsViewRepository : Repository<vw_SALE_Transactions>, ITransactionsViewRepository
    {
        public TransactionsViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
    #endregion
   
    //price
    public class PriceListRepository : Repository<BILL_ItemsPriceList>, IPriceListRepository
    {
        public PriceListRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class PriceRevisionsReposiotry : Repository<BILL_ItemsPriceRevisions>, IPriceRevisionsReposiotry
    {
        public PriceRevisionsReposiotry(IUnitOfWork unitOfWork): base(unitOfWork){}
    }

    public class CurrencyRepository : Repository<BASE_CurrencyLib>, ICurrencyRepository
    {
        public CurrencyRepository(IUnitOfWork unitOfWork): base(unitOfWork){}
    }

    public class RefundRequestsRepository : Repository<SALE_RefundRequests>, IRefundRequestsRepository
    {
        public RefundRequestsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    //payout 
    public class UserPayoutStatmentsRepository : Repository<PO_UserPayoutStatments>, IUserPayoutStatmentsRepository
    {
        public UserPayoutStatmentsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<PO_PayoutStatmentToken> GetPayoutStatments(int executionId)
        {
            return DataContext.tvf_PO_GetPayoutStatments(executionId).ToList();
        }
    }

    public class PayoutExecutionsRepository : Repository<PO_PayoutExecutions>, IPayoutExecutionsRepository
    {
        public PayoutExecutionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<PO_PayoutExecutionToken> GetPayoutExecutions(int? executionId)
        {
            return DataContext.tvf_PO_GetPayoutExecutions(executionId).ToList();
        }
    }
}
