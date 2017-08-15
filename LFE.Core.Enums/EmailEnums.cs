using System.ComponentModel;

namespace LFE.Core.Enums
{
    public static class EmailEnums
    {
        public enum eTemplateKinds
        {
             Unknown
            ,[Description("User Message Notification")] USER_NOT                          = 1
            ,[Description("Activation")] REGISTRATION_ACTIVATION                          = 2
            ,[Description("Registration")] REGISTRATION                                   = 11
            ,[Description("Purchase Author")] PURCHASE_AUTHOR                             = 3
            ,[Description("Purchase Author Free")] PURCHASE_AUTHOR_FREE                   = 15
            ,[Description("Purchase Learner")] PURCHASE_LEARNER                           = 6
            ,[Description(" Purchase Free Course")] PURCHASE_LEARNER_FREE                 = 12
            ,[Description("Purchase with coupon")] PURCHASE_LEARNER_WITH_COUPON           = 13
            ,[Description("Purchase with free coupon")] PURCHASE_LEARNER_WITH_FREE_COUPON = 14
            ,[Description("Review Author")] REVIEW_AUTHOR                                 = 5
            ,[Description("Review Learner")] REVIEW_LEARNER                               = 7
            ,[Description("Forgotten Password")] FORGOTTEN_PASSWORD                       = 8
            ,[Description("Author Cancellation Alert")] AUTHOR_CANCELLATION_ALERT         = 16
            ,[Description("RGP confirmation")] REFUND_PROGRAM_CONFIRMATION                = 18
            ,[Description("RGP refund submitted")] GRP_REFUND_SUBMITTED                   = 19
            ,[Description("Student Certificate")] STUDENT_CERTIFICATE                     = 20
            ,[Description("Quiz Author Request")] QUIZ_AUTHOR_REQUEST                     = 21
            ,[Description("Quiz Author Response")] QUIZ_AUTHOR_RESPONSE                   = 22            
        }

        public enum eTemplateFields
        {
             [Description("Html Body")] HTML_BODY
            ,[Description("Post Author")] POST_AUTHOR
            ,[Description("Post Date")] POST_DATE
            ,[Description("Full Name")] FULL_NAME
            ,[Description("Learner Name")] LEARNER_NAME
            ,[Description("Learner Email")] LEARNER_EMAIL
            ,[Description("Course Name")] COURSE_NAME
            ,[Description("Coupon Name")] COUPON_NAME
            ,[Description("Course Author")] COURSE_AUTHOR
            ,[Description("Thumb Url")] THUMB_URL
            ,[Description("Item Type")] ITEM_TYPE_NAME
            ,[Description("Page Url")] ITEM_PAGE_URL
            ,[Description("Purchase Date")] PURCHASE_DATE            
            ,[Description("Reset password link")] RESET_PASSWORD_LINK
            ,[Description("Base site url")] BASE_SITE_URL
            ,[Description("Activation Link")] ACTIVATION_LINK
            ,[Description("Price")] PRICE
            ,[Description("Discount")] DISCOUNT
            ,[Description("Total Price")] TOTAL_PRICE
            ,[Description("ISO")] ISO
            ,[Description("Currency Symbol")] CURRENCY_SYMBOL
            ,[Description("Agreement Description")] AGREEMENT_DESC
            ,[Description("Payment Instrument")] PAYMENT_INSTRUMENT_DESC
            ,[Description("Review Author")] REVIEW_AUTHOR
            ,[Description("Review Date")] REVIEW_DATE
            ,[Description("Following Month")] FOLLOWING_MONTH
            ,[Description("Free text")] FREE_TEXT
            ,[Description("License Key")] LICENSE_KEY
            ,[Description("Email")] EMAIL
            ,[Description("Hub Name")] HUB_NAME
            ,[Description("Quiz Name")] QUIZ_NAME
            ,[Description("Attempt No")] QUIZ_ATTEMPT_NO
        }

        public enum eSendInterfaceStatus
        {
            Waiting = 1,
            Send    = 2,
            Failed  = 3
        }
    }
}
