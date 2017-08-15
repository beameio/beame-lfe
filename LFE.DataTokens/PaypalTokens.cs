using System.Collections.Generic;

namespace LFE.DataTokens
{
    public class BaseRequestItemInfo : BaseModelState
    {
        public string ItemName { get; set; }

        public string TrackingId { get; set; }

        public int PriceLineId { get; set; }
    }

    public class SubscriberShippingAddress
    {
        public object Name { get; set; }
        public string Street1 { get; set; }
        public object Street2 { get; set; }
        public string CityName { get; set; }
        public string StateOrProvince { get; set; }
        public int Country { get; set; }
        public string CountryName { get; set; }
        public object Phone { get; set; }
        public string PostalCode { get; set; }
        public object AddressID { get; set; }
        public int AddressOwner { get; set; }
        public object ExternalAddressID { get; set; }
        public object InternationalName { get; set; }
        public object InternationalStateAndCity { get; set; }
        public object InternationalStreet { get; set; }
        public int AddressStatus { get; set; }
        public object AddressNormalizationStatus { get; set; }
    }

    public class RecurringPaymentsProfileDetails
    {
        public string SubscriberName { get; set; }
        public SubscriberShippingAddress SubscriberShippingAddress { get; set; }
        public string BillingStartDate { get; set; }
        public object ProfileReference { get; set; }
    }

    public class Amount
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class ShippingAmount
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class TaxAmount
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class CurrentRecurringPaymentsPeriod
    {
        public int BillingPeriod { get; set; }
        public int BillingFrequency { get; set; }
        public int TotalBillingCycles { get; set; }
        public Amount Amount { get; set; }
        public ShippingAmount ShippingAmount { get; set; }
        public TaxAmount TaxAmount { get; set; }
    }

    public class OutstandingBalance
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class LastPaymentAmount
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class RecurringPaymentsSummary
    {
        public string NextBillingDate { get; set; }
        public int NumberCyclesCompleted { get; set; }
        public int NumberCyclesRemaining { get; set; }
        public OutstandingBalance OutstandingBalance { get; set; }
        public int FailedPaymentCount { get; set; }
        public string LastPaymentDate { get; set; }
        public LastPaymentAmount LastPaymentAmount { get; set; }
    }

    public class Amount2
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class ShippingAmount2
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class TaxAmount2
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class RegularRecurringPaymentsPeriod
    {
        public int BillingPeriod { get; set; }
        public int BillingFrequency { get; set; }
        public int TotalBillingCycles { get; set; }
        public Amount2 Amount { get; set; }
        public ShippingAmount2 ShippingAmount { get; set; }
        public TaxAmount2 TaxAmount { get; set; }
    }

    public class TrialAmountPaid
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class RegularAmountPaid
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class AggregateAmount
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class AggregateOptionalAmount
    {
        public int currencyID { get; set; }
        public string value { get; set; }
    }

    public class GetRecurringPaymentsProfileDetailsResponseDetails
    {
        public string ProfileID { get; set; }
        public int ProfileStatus { get; set; }
        public string Description { get; set; }
        public int AutoBillOutstandingAmount { get; set; }
        public int MaxFailedPayments { get; set; }
        public RecurringPaymentsProfileDetails RecurringPaymentsProfileDetails { get; set; }
        public CurrentRecurringPaymentsPeriod CurrentRecurringPaymentsPeriod { get; set; }
        public RecurringPaymentsSummary RecurringPaymentsSummary { get; set; }
        public object CreditCard { get; set; }
        public object TrialRecurringPaymentsPeriod { get; set; }
        public RegularRecurringPaymentsPeriod RegularRecurringPaymentsPeriod { get; set; }
        public TrialAmountPaid TrialAmountPaid { get; set; }
        public RegularAmountPaid RegularAmountPaid { get; set; }
        public AggregateAmount AggregateAmount { get; set; }
        public AggregateOptionalAmount AggregateOptionalAmount { get; set; }
        public string FinalPaymentDueDate { get; set; }
    }

    public class GetRecurringPaymentsProfileDto
    {
        public GetRecurringPaymentsProfileDetailsResponseDetails GetRecurringPaymentsProfileDetailsResponseDetails { get; set; }
        public string Timestamp { get; set; }
        public int Ack { get; set; }
        public string CorrelationID { get; set; }
        public List<object> Errors { get; set; }
        public string Version { get; set; }
        public string Build { get; set; }
    }

    //ipn
    //public class IpnResponseDTO
    //{
    //    public BillingEnums.eIpnResponseTypes Type { get; set; }

    //    //Recurring Payment variables
    //    public string ProfileId { get; set; }
        
    //    public  decimal Amount { get; set; }
        
    //    public  decimal CycleAmount { get; set; }
        
    //    public  decimal InitialPaymentAmount { get; set; }

    //    public DateTime? NextPaymentDate { get; set; }
        
    //    public  decimal Balance { get; set; }
        
    //    public BillingEnums.eSubscriptionStatuses Status { get; set; }

    //}

    public class IpnResponseToken
    {
        public string response_string { get; set; }

        //common
        public string parent_txn_id { get; set; }

        public string txn_id { get; set; }

        public string txn_type { get; set; }

        public string business { get; set; }
        //recurring
        public decimal? amount { get; set; }
        public decimal? outstanding_balance { get; set; }
        public string recurring_payment_id { get; set; }
        public decimal? initial_payment_amount { get; set; }
        public string initial_payment_status { get; set; }
        public string next_payment_date { get; set; }
        public string payment_cycle { get; set; }
        public decimal? amount_per_cycle { get; set; }
        public string period_type { get; set; }
        public string profile_status { get; set; }
        public string time_created { get; set; }

        //payments variables
        public decimal? auth_amount { get; set; }

        public string auth_exp { get; set; }

        public string auth_id { get; set; }

        public string auth_status { get; set; }

        //payment
        public string payment_date { get; set; }

        public string payment_status { get; set; }

        public string payment_type { get; set; }

        public string pending_reason { get; set; }

        public decimal? payment_fee { get; set; }

        public decimal? payment_gross { get; set; }

        public string reason_code { get; set; }

        
        //ms
        public string mc_currency { get; set; }

        public decimal? mc_fee { get; set; }

        public decimal? mc_gross { get; set; }

        public decimal? mc_handling { get; set; }

        
        //misc
        public string last_name { get; set; }
        public string residence_country { get; set; }
        public string currency_code { get; set; }
        
        public string verify_sign { get; set; }
        public string payer_status { get; set; }
        public int test_ipn { get; set; }
        public decimal? tax { get; set; }
        public string payer_email { get; set; }
        public string first_name { get; set; }
        public string receiver_email { get; set; }
        public string payer_id { get; set; }
        public int product_type { get; set; }
        public string payer_business_name { get; set; }
        public string initial_payment_txn_id { get; set; }
        public decimal? shipping { get; set; }
        public string charset { get; set; }
        public double notify_version { get; set; }
        public string product_name { get; set; }
        public string ipn_track_id { get; set; }
        public int? hipping { get; set; }
        public string cmd { get; set; }
    }

    public class MassPaymentItemToken
    {
        public string masspay_txn_id { get; set; }

        public string status { get; set; }

        public string unique_id { get; set; }

        public decimal? payment_gross { get; set; }
        public decimal? payment_fee { get; set; }
        public string receiver_email { get; set; }

        //mc
        public string mc_currency { get; set; }

        public decimal? mc_fee { get; set; }

        public decimal? mc_gross { get; set; }
    }
}
