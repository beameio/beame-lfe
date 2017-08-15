using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class BasePaymentDTO
    {
        public BasePaymentDTO()
        {
            currency = "USD";
        }
        public int priceLineId { get; set; }
        public BillingEnums.ePurchaseItemTypes Type { get; set; }
        [Required]
        public decimal amount { get; set; }

        [Required]
        public int? courseId { get; set; }
        public int? bundleId { get; set; }
        public string trackingId { get; set; }
        public int? addressId { get; set; }
        public Guid? paymentInstrumentId { get; set; }
        public string couponCode { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
    }

    public class PayPalCreatePaymentDTO : BasePaymentDTO
    {
        public string cancel_url { get; set; }
        public string success_url { get; set; }        
    }

    public class PayPalAgreementDTO : BasePaymentDTO
    {
        public string cancel_url { get; set; }
        public string success_url { get; set; }        
        public string buyerName { get; set; }
        public string buyerEmail { get; set; }
        public BillingEnums.ePaymentMethods method { get; set; }

        public BaseCurrencyDTO Currency { get; set; }
    }

    public class SubscriptionWithSavedCardDTO : BasePaymentDTO
    {
        public int UserId { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal BillingAmount { get; set; }
        public DateTime InitialPaymentDate { get; set; }
        public DateTime FirstBillingDate { get; set; }
        public string ProfileDescription { get; set; }
        public Guid PaymentInstrumentId { get; set; }
    }

    public class PayPalCreateResponseToken
    {
        public string approved_url { get; set; }
        public Guid id { get; set; }
    }

    public class PaypalPaymentRequestDTO
    {
        public Guid ReuqstId { get; set; }
        public BillingEnums.ePurchaseItemTypes Type { get; set; }
        public Guid? SourceReuqstId { get; set; }
        public BillingEnums.ePaymentMethods PaymentMethod { get; set; }
        public BillingEnums.ePaypalRequestTypes PaypalRequestType { get; set; }
        public BillingEnums.ePaymentRequestStatus RequestStatus { get; set; }
        public int UserId { get; set; }
        public int? CourseId { get; set; }
        public int? BundleId { get; set; }
        public int? PriceLineId { get; set; }
        public decimal Amount { get; set; }
        public string TrackingID { get; set; }
        public int? AddressId { get; set; }
        public Guid? PaymentInstrumentId { get; set; }
        public string CouponCode { get; set; }
        public string CreatePaymentId { get; set; }
        public string RecurringRequestToken { get; set; }
        public string ExecutionPaymentId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PaypalCreditCardPaymentDTO : BasePaymentDTO
    {
        public PaypalCreditCardPaymentDTO()
        {
            card = new CreditCardDTO
            {
                Type = BillingEnums.eCreditCardType.Visa
            };
        }

        public CreditCardDTO card { get; set; }
    }

    public class PayPalSavedCCPaymentDTO : PaypalCreditCardPaymentDTO
    {
        public bool IsCardExists { get; set; }

        public bool ChangeCard { get; set; }
    }

    public class CreditCardDTO
    {
        public CreditCardDTO()
        {
            BillingAddress = new BillingAddressDTO();
        }

        public BillingEnums.eCreditCardType Type { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Credit card number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "*")]
        public string CVV2 { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Expiration Month")]
        public int ExpireMonth { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Expiration Year")]
        public int ExpireYear { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "*")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "*")]
        public string LastName { get; set; }

        public BillingAddressDTO BillingAddress { get; set; }
        
    }

    public class PaymentInstrumentDTO
    {
        public Guid InstrumentId { get; set; }
        public string DisplayName { get; set; }
    }

    public class PaymentInstrumentViewDTO : PaymentInstrumentDTO
    {
        public string State { get; set; }
    }

    public class ReccuringPaymentDTO 
    {
        public BillingEnums.ePaymentMethods PaymentMethod { get; set; }
        public CreditCardDTO creditCard { get; set; }
        public DateTime billingStartDate { get; set; }
        public string subscriberName { get; set; }
        public string profileDescription { get; set; }
        public decimal billingAmount { get; set; }
        public decimal initialAmount { get; set; }
        public BillingEnums.eBillingPeriodType billingPeriod { get; set; }

        public BaseCurrencyDTO Currency { get; set; }
        //optional if trial period required,currently not in use
        //public string totalBillingCycles { get; set; }
        //public int billingFrequency { get; set; }
    }

    public class RecurringPaymentExecutionResultToken
    {
        public bool Success { get; set; }
        public Guid RequestId { get; set; }
        public string ProfileDescription { get; set; }
        public decimal BillingAmount { get; set; }
        public decimal InitialAmount { get; set; }
        public string PaypalProfileId { get; set; }
        public DateTime FirstBillingDate { get; set; }
    }

    public class RefundOrderLinePaymentDTO : BaseModelState
    {
        public int LineId { get; set; }
        public int PaymentId { get; set; }
        public int? TransactionId { get; set; }
        public string ExternalTransactionId { get; set; }
        public decimal Amount { get; set; }        
        public bool DeniedAccess { get; set; }      
    }

    
    public class TransactionFiltersLOV
    {
        public TransactionFiltersLOV()
        {
            AuthorsLOV = new List<SelectListItem>();
            CoursesLOV = new List<SelectListItem>();
            BundlesLOV = new List<SelectListItem>();
            BuyersLOV = new List<SelectListItem>();
        }
        public List<SelectListItem> AuthorsLOV { get; set; }
        public List<SelectListItem> BuyersLOV { get; set; }
        public List<SelectListItem> CoursesLOV { get; set; }
        public List<SelectListItem> BundlesLOV { get; set; }
    }

    public class PaypalPaymentExecutionToken
    {
        public decimal Amount { get; set; }

        public string ReceiverEmail { get; set; }

        public string InvoiceId { get; set; }

        public BaseCurrencyDTO Currency { get; set; }
    }
   
    public class PaypalRecurringProfileToken
    {
        public string ProfileID { get; set; }

        public string Description { get; set; }
        public string SubscriberName { get; set; }
        public string Status { get; set; }

        public DateTime BillingStartDate { get; set; }
        public DateTime NextPaymentDate { get; set; }

        public DateTime LastPaymentDate { get; set; }        

        public int? CompletedCycles { get; set; } 

        public int FailedPaymentCount { get; set; }

        public decimal AmountPaid { get; set; }

        public string BillingPeriod { get; set; }

        public int? BillingFrequency { get; set; }  
    }
    #region error handling
    public class Detail
    {
        public string field { get; set; }
        public string issue { get; set; }
    }

    public class PaypalExceptionToken
    {
        public string name { get; set; }
        public List<Detail> details { get; set; }
        public string message { get; set; }
        public string information_link { get; set; }
        public string debug_id { get; set; }
    }
    #endregion


    #region sales orders

    public class OrderLinePriceDTO
    {
        public OrderLinePriceDTO()
        {
            Price      = 0;
            Discount   = 0;
            FinalPrice = 0;
        }

        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
    }
    #endregion

    public class MonthlyStatmentPrintToken : BaseModelState
    {
        public List<MonthlyStatementDTO> Statments { get; set; }
    }

    public class MonthlyStatementDTO  : BaseModelState   
    {
        public BaseUserInfoDTO User { get; set; }

        public MonthlyStatementStatsDTO Stats { get; set; }

        public BaseCurrencyDTO Currency { get; set; }

        public string PaymentMethodName { get; set; }

        public int? PayoutTypeId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public string ChartImageUrl { get; set; }


    }

    public class MonthlyStatementStatsDTO
    {
        public MonthlyStatementStatsDTO()
        {
            TotalSales                = 0;
            TotalOneTimeSales         = 0;
            TotalSubscriptionPayments = 0;
            TotalRefunds              = 0;
            TotalRefundFees = 0;
            TotalLfeCommission        = 0;
            TotalPaypalFees           = 0;
            TotalPayout               = 0;
        }

        public decimal TotalOneTimeSales { get; set; }
        public decimal TotalRentalSales { get; set; }
        public decimal TotalSubscriptionPayments { get; set; }

        public decimal AffiliateSales { get; set; }

        public decimal Fees { get; set; }

        public decimal AffiliateCommission { get; set; }
        
        public decimal AffiliateFees { get; set; }
        
        public decimal RefundProgrammToHold { get; set; }
        
        public decimal RefundProgrammToRelease { get; set; }

        public decimal TotalRefunds { get; set; }
        public decimal TotalRefundFees { get; set; }
        public decimal TotalPaypalFees { get; set; }
        
        public decimal TotalLfeCommission { get; set; }

        public decimal TotalSales { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalPayout { get; set; }
    }
    //invoice token
    public class InvoiceViewDTO : BaseModelState
    {
        public int TransactionId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string CourseName { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    //currency
    public class BaseCurrencyDTO
    {
        public short CurrencyId { get; set; }

        [DisplayName("Currency")]
        public string CurrencyName { get; set; }

        [DisplayName("ISO")]
        public string ISO { get; set; }

        public string Symbol { get; set; }
    }

    public class CurrencyDTO : BaseCurrencyDTO
    {
        
        public string CurrencyCode { get; set; }
        
        public short? CountryId { get; set; }

        [DisplayName("IsActive")]
        public bool IsActive { get; set; }

        public bool KeepDecimal { get; set; }
    }

    public class RequestPurchaseItemNameToken
    {
        
        public int itemId { get; set; }
        public BillingEnums.ePurchaseItemTypes type{ get; set; }
        public PriceLineDTO priceLineDto { get; set; }
    }

    public class GRPRequestToken
    {
        public int OrderLineId { get; set; }
        [Required(ErrorMessage = "refund reason is mandatory")]
        public string ReasonText { get; set; }
    }

    //Payout
    public class BasePayoutSelectionToken
    {
        public int userId { get; set; }
        public short currId { get; set; }
    }
    public class PayoutExecutionDTO
    {
        public PayoutExecutionDTO()
        {
            ExecutionId = -1;
            Status      = BillingEnums.ePayoutStatuses.WAIT;
        }

        public PayoutExecutionDTO(int year, int month)
        {
            ExecutionId = -1;
            Status      = BillingEnums.ePayoutStatuses.WAIT;
            Year        = year;
            Month       = month;
        }

        public int ExecutionId { get; set; }

        public BillingEnums.ePayoutStatuses Status { get; set; }

        public DateTime PayoutDate { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int TotalRows { get; set; }
        public int TotCompletedalRows { get; set; }

        public DateTime? AddOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public BaseUserInfoDTO ExecutedBy { get; set; }

        public BaseUserInfoDTO UpdatedBy { get; set; }
    }


    public class PayoutStatmentDTO
    {
        public int ExecutionId { get; set; }
        public int PayoutId { get; set; }

        public BaseUserInfoDTO Beneficiary { get; set; }

        public BaseCurrencyDTO Currency { get; set; }

        public string PayKey { get; set; }

        public decimal Amount { get; set; }

        public string PaypalEmail { get; set; }

        public string Error { get; set; }
        public DateTime? AddOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public BaseUserInfoDTO ExecutedBy { get; set; }

        public BaseUserInfoDTO UpdatedBy { get; set; }
        public BillingEnums.ePayoutTypes? PayoutType { get; set; }
        public BillingEnums.ePayoutStatuses Status { get; set; }
    }
}
