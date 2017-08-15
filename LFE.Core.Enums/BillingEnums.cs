
using System;
using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class BillingEnums
    {
        public enum ePayoutTypes
        {
             [Description("Paypal")] PAYPAL = 1
            ,[Description("Cheque")] CHEQUE = 2
        }

        public enum ePurchaseItemTypes //BASE_ItemTypesLOV
        {
            [Description("Course")] COURSE = 1
            ,[Description("Bundle")] BUNDLE = 2
        }

        public enum ePricingTypes //BILL_PricingTypesLOV
        {
            [Description("One time payment")] ONE_TIME  = 1
            ,[Description("Subscription")] SUBSCRIPTION = 2
            ,[Description("Rental")] RENTAL             = 4
            ,[Description("Free")] FREE                 = 8
        }

        public enum ePayoutStatuses
        {
             Unknown                                        = 0
            ,[Description("Waiting")] WAIT                  = 1
            ,[Description("Skipped")] SKIP                  = 2
            ,[Description("Payout failed")] FAILED          = 3
            ,[Description("Completed")] COMPLETED           = 4
            ,[Description("Partially completed")] PARTIALLY = 5
            ,[Description("Waiting for IPN")] WAIT_4_IPN    = 6
        }

        [Flags]
        public enum eCreditCardType
        {
             Visa = 1
            ,MasterCard = 2
            ,Amex = 4
            ,Discover = 8            
           // ,Switch
           // ,Solo
        }

        public enum ePaymentAction
        {
            Order,
            Authorization,
            Sale
        }

        public enum ePaymentMethods //referenced to BILL_PaymentMethodsLOV
        {
            [Description("Paypal")] Paypal                         = 0,
            [Description("Credit Card")] Credit_Card               = 1,
            [Description("Saved Payment Method")] Saved_Instrument = 2,
            [Description("Charge Free")] Charge_Free               = 3
        }

        public enum ePaymentTerms
        {
             [Description("Immediate")] IMMEDIATE           = 1
            ,[Description("Monthly Subscription")] EVERY_30 = 2
        }

        public enum ePaymentStatuses
        {
             [Description("Scheduled")] SCHEDULED         = 1
            ,[Description("Payment completed")] COMPLETED = 2
            ,[Description("Failed")] FAILED               = 3
            ,[Description("Canceled")] CANCELED           = 4
        }

        public enum ePaymentTypes
        {
             [Description("One-time payment")] ONE_TIME                        = 1
            ,[Description("Initial subscription payment")] INTIAL_SUBSCRIPTION = 2
            ,[Description("Period subscription payment")] PERIOD_SUBSCRIPTION  = 3
            ,[Description("Refund")] REFUND                                    = 4
            ,[Description("Partial Refund")] PARTIAL_REFUND                    = 5
        }

        public enum ePaymentSources
        {
             ONE_TIME
            ,SUBSCRIPTION
            ,REFUND
        }

        public enum eAccessStatuses
        {
             [Description("Active")] ACTIVE       = 1
            ,[Description("Finished")] FINISHED   = 2
            ,[Description("Canceled")] CANCELED   = 3
            ,[Description("Suspended")] SUSPENDED = 4
        }

        public enum eOrderStatuses
        {
             [Description("Complete")] COMPLETE   = 1
            ,[Description("Active")] ACTIVE       = 2
            ,[Description("Canceled")] CANCELED   = 3
            ,[Description("Suspended")] SUSPENDED = 4
        }

        public enum eOrderLineTypes
        {
             [Description("Sale")] SALE                      = 1
            ,[Description("Subscription Sale")] SUBSCRIPTION = 2
            ,[Description("Rental")]RENTAL                   = 3
            ,[Description("Free")] FREE                      = 4
        }

        public enum ePaypalRequestTypes //referenced to PAYPAL_RequestTypesLOV
        {
             [Description("Account Payment")] ACCOUNT_PAYMENT                                                 = 1
            ,[Description("Recurring Payment Agreement")] RECURRING_PAYMENT_AGREEMENT                         = 2
            ,[Description("Recurring Payment Execution")] RECURRING_PAYMENT_EXECUTION                         = 6
            ,[Description("Recurring Payment Update")] RECURRING_PAYMENT_UPDATE_EXECUTION                     = 11
            ,[Description("Direct Credit Card Payment")] CC_DIRECT                                            = 3
            ,[Description("Save Credit Card")] CC_SAVE                                                        = 4
            ,[Description("Saved Credit Card Payment")] CC_PAY_WITH_SAVED                                     = 5
            ,[Description("Subscription Payment with Stored Credit Card")] SUBSCRIPTION_PAYMENT_WITH_SAVED_CC = 7
            ,[Description("REST API Refund")] REST_API_REFUND                                                 = 8
            ,[Description("Merchant API Refund")] MERCHANT_API_REFUND                                         = 9
            ,[Description("Cancel Recurring Payment")] RECURRING_PAYMENT_CANCEL                               = 10
        }

        public enum ePaymentRequestStatus
        {
             init
            ,approved
            ,failed
            ,cancel
        }

      
        [Flags]
        public enum eBillingPeriodType //imported from merchant sdk + additional values to BILL_PeriodTypesLOV 
        {
            [Description("NoBillingPeriodType")] NOBILLINGPERIODTYPE = 0,
            [Description("Day")] DAY                                 = 1,
            [Description("Week")] WEEK                               = 2,
            [Description("SemiMonth")] SEMIMONTH                     = 4,
            [Description("Month")] MONTH                             = 8,
            [Description("Year")] YEAR                               = 16,
            [Description("Hour")] HOUR                               = 32
        }

        //inherited from previous design 4 using in UserTransactions table
        //TODO review on finally refactoring
        public enum eTransactionTypes
        {
            Undefined                                                                = -1,
            [Description("Paypal Account Payment")] DirectPaymentTransaction         = 1,
            ExpressCheckoutTransaction                                               = 2,
            Complementary                                                            = 3,
            SellerPayment                                                            = 4,
            [Description("Free Coupon")] FreeCouponCourse                            = 5,
            [Description("Free Course")] FreeCourse                                  = 6,
            [Description("Credit Card Payment")] DirectCreditCardPayment             = 7,
            [Description("Stored Credit Card Payment")] SavedCreditCardPayment       = 8,
            [Description("Initial Subscription Payment")] InitialSubscriptionPayment = 9,
            [Description("Period Subscription Payment")]   PeriodSubscriptionPayment = 10,
            [Description("Full Payment Refund")] Refund                              = 11,
            [Description("Partial Payment Refund")] PartialRefund                    = 12,
            [Description("Cancel Subscription")] CancelSubscription                  = 13
        }

        //paypal enums
        public enum ePaypalPaymentStatus
        {
            Completed
        }
        public enum eIpnResponseTypes
        {
            //recurring payments
             recurring_payment_profile_created
            ,recurring_payment            
            ,recurring_payment_profile_cancel
            ,cart
            ,paypal_here
            //TODO implement logic and create test cases
            //,recurring_payment_expired
            //,recurring_payment_failed            
            //,recurring_payment_skipped	
            //,recurring_payment_suspended	
            //,recurring_payment_suspended_due_to_max_failed_payment	
            //currently not in use (2013-12-20)
            //,adjustment            
            //,express_checkout	
            //,masspay	
            //,mp_cancel	
            //,mp_signup	
            //,merch_pmt	
            //,new_case	
            //,payout	                        
            //,send_money	
            //,subscr_cancel	
            //,subscr_eot	
            //,subscr_failed	
            //,subscr_modify	
            //,subscr_payment	
            //,subscr_signup	
            //,virtual_terminal	
            //,web_accept	
        }

        public enum eRefundRequestStatus
        {
            [Description("Submitted")] SUBMITTED         = 1,
            [Description("In progress")] IN_PROGRESS     = 2,
            [Description("Approved by Author")] APPROVED = 3,
            [Description("Rejected by Author")] REJECTED = 4,
            [Description("Refunded")] REFUNDED           = 5,
            [Description("Denied")] DENIED               = 6,
            [Description("Error")] ERROR                 = 7
        }
    }
}
