
using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class DashboardEnums
    {

        public enum eSaleSources
        {
             [Description("Author Sales")] AS
            ,[Description("By Affiliate Sales")] BAFS
            ,[Description("Affiliate Sales")] AFS
        }

        public enum eDbEventTypes
        {
             [Description("New course/bundle")] NewItem
            ,[Description("New chapter")] NewChapter
            ,[Description("New facebook store")] NewFbStore
            ,[Description("New store")] NewStore
            ,[Description("New Mailchimp campaign")] NewMailchimp
            ,[Description("Custom Event")] Custom
        }

        public enum eSaleBoxType
        {
            [Description("ONE-TIME SALE")] ONE_TIME                              = 1,
            [Description("SUBSCRIPTION SALES")] SUBSCRIPTION                     = 2,
            [Description("RENTAL SALES")] RENTAL                                 = 3, 
            [Description("SALES BY AFFILIATES")] SALES_BY_AFFILIATES             = 4,
            [Description("SUBSCRIPTION CANCELLATION")] SUBSCRIPTION_CANCELLATION = 5,
            [Description("REFUNDS")] REFUNDS                                     = 6,
            [Description("COUPONS USED")] COUPONS_USED                           = 7,
            [Description("Affiliate Sales")] AFFILIATE_SALES                     = 8,
            [Description("Active Subscribers")] ACTIVE_SUBSCRIBERS               = 9
        }
    }
   
}
