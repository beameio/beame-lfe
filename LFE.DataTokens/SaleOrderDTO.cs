using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LFE.Core.Enums;
using System;

namespace LFE.DataTokens
{
    public class OrderDTO
    {
        public int OrderNumber { get; set; }
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public BaseUserInfoDTO Seller { get; set; }
        public BaseUserInfoDTO Buyer { get; set; }
        public BaseWebStoreDTO WebStore { get; set; }
        public BillingEnums.eOrderStatuses OrderStatus { get; set; }
        public string StatusName { get; set; }
        public BillingEnums.ePaymentMethods PaymentMethod { get; set; }
        public BaseCurrencyDTO Currency { get; set; }
        public string PaymentMethodName { get; set; }
        public string PaymentInstrumentName { get; set; } 
    }

    public class BaseOrderLineDTO
    {
        public int OrderNumber { get; set; }
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int LineId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public BaseCurrencyDTO Currency { get; set; }
        public BaseUserInfoDTO Seller { get; set; }
        public BaseUserInfoDTO Buyer { get; set; }
        public BaseUserInfoDTO WebStoreOwner { get; set; }
        public BaseWebStoreDTO WebStore { get; set; }
        public BillingEnums.eOrderStatuses OrderStatus { get; set; }
        public string Status { get; set; }
        public BillingEnums.ePaymentMethods PaymentMethod { get; set; }
        public BillingEnums.eOrderLineTypes LineType { get; set; }
        public BillingEnums.ePaymentTerms PaymentTerm { get; set; }
        public string PaymentTermName { get; set; }
        public string PaypalProfileID { get; set; }
        public decimal TotalRefunded { get; set; }
        public string CouponValue { get; set; }
        public bool IsUnderGRP { get; set; }
        public bool IsGRPRefundable { get; set; }

        public decimal? AffiliateCommisssion { get; set; }
    }

    public class OrderLineDTO : BaseOrderLineDTO
    {
      
    }


    public class BaseLinePaymentDTO : BaseModelState
    {
        
        public int PaymentId { get; set; }
        
        public int LineId { get; set; }
        
        public decimal Amount { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public BaseCurrencyDTO Currency { get; set; }

        public BillingEnums.ePaymentTypes Type { get; set; }
        public BillingEnums.ePaymentStatuses Status { get; set; }
       
    }

    public class LinePaymentDTO : BaseLinePaymentDTO
    {
        public Guid OrderId { get; set; }
        public short Number { get; set; }
        public decimal? Fee { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string TypeName { get; set; }
        public bool IsRefundable { get; set; }
        public decimal TotalRefunded { get; set; }
        
        public bool IsCustomTrxAllowed { get; set; }
        public decimal AffiliateCommission { get; set; }
    }

    public class PaymentDTO : LinePaymentDTO
    {
        public int OrderNumber { get; set; }
        public string ItemName { get; set; }        
    }

    public class CustomTrxDTO : BaseLinePaymentDTO
    {
        [Required]
        public new int PaymentId { get; set; }
        [Required]
        public new int LineId { get; set; }
        [Required]
        public new decimal Amount { get; set; }

        [Required]
        [DisplayName("External ID")]
        public string ExternalTrxID { get; set; }

        public decimal Fee { get; set; }      
    }

    public class PaymentViewDTO : PaymentDTO
    {
        public BaseUserInfoDTO Buyer { get; set; }
        public BaseUserInfoDTO Seller { get; set; }

        public BaseUserInfoDTO StoreOwner { get; set; }

        public string TrxId { get; set; }

        public decimal? Commission { get; set; }

        public decimal Payout { get; set; }

        public DateTime OrderDate { get; set; }
    }

    public class RefundViewDTO : RefundDTO
    {
        public BaseUserInfoDTO Buyer { get; set; }
    }

    public class RefundDTO : LinePaymentDTO
    {
        public int OrderNumber { get; set; }
        public string ItemName { get; set; }
        public int RefundId { get; set; }
        public decimal RefundAmount { get; set; }
        public DateTime RefundDate { get; set; }
        public BillingEnums.ePaymentTypes RefundType { get; set; }
        public string RefundTypeName { get; set; }
    }

    public class ScheduledPaymentSummaryToken
    {
        public Guid OrderId { get; set; }
        public int OrderNumber { get; set; }
        public int LineId { get; set; }
        public int PaymentId { get; set; }
        public string ItemName { get; set; }
        public short Number { get; set; }
        public decimal Amount { get; set; }
        public DateTime ScheduledDate { get; set; }
        public BaseUserInfoDTO Buyer { get; set; }
        public BaseUserInfoDTO Seller { get; set; }
        public BaseWebStoreDTO WebStore { get; set; }
        public  BillingEnums.ePaymentStatuses Status { get; set; }
        public BaseCurrencyDTO Currency { get; set; }
        public PaymentInstrumentDTO PaymentInstrument { get; set; }
        public BillingEnums.ePaymentMethods PaymentMethod { get; set; }
        public string PaymentMethodName { get; set; }
        public string PaypalProfileID { get; set; }       
        public string PaymentInstrumentName { get; set; } 
        public bool AutoBill { get; set; }
    }

    public class PaymentRefundDTO
    {
        public int RefundId { get; set; }
        public int PaymentId { get; set; }        
        public decimal Amount { get; set; }
        public DateTime RefundDate { get; set; }
        public BillingEnums.ePaymentTypes Type { get; set; }
        public string TypeName { get; set; }
    }

    public class TransactionSummaryToken
    {
        public TransactionSummaryToken()
        {
            Buyer = new BaseUserInfoDTO();
            Seller = new BaseUserInfoDTO();
            Course = new BaseCourseInfoDTO();
        }

        public int OrderNumber { get; set; }
        public int OrderLineId { get; set; }
        public int TrxId { get; set; }
        public string ItemName { get; set; }
        public BaseCurrencyDTO Currency { get; set; }
        public BaseUserInfoDTO Buyer { get; set; }
        public BaseUserInfoDTO Seller { get; set; }

        public BaseCourseInfoDTO Course { get; set; }

        public BillingEnums.eTransactionTypes TrxType { get; set; }

        public string TrxTypeName { get; set; }

        public DateTime TrxDate { get; set; }

        public decimal Amount { get; set; }

        public decimal RefundAmount { get; set; }

        public decimal? Fee { get; set; }

        public short? PaymentNumber { get; set; }

        public string ExternalTrxId { get; set; }

        //  public bool IsRefundAllowed { get; set; }
    }
}
