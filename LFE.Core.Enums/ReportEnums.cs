using System;
using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class ReportEnums
    {
        [Flags]
        public enum ePeriodSelectionKinds
        {
             [Description("Last 30 days")] lastMonth       = 1 
            ,[Description("Week")] week                    = 2
            ,[Description("This Month")] thisMonth         = 4
            ,[Description("Last 90 days")] last90          = 8
            ,[Description("Last six months")] last180      = 16
            ,[Description("All time")] all                 = 32
            ,[Description("By Period")] period             = 64
            ,[Description("Previous Month")] previousMonth = 128
        }
        
        [Flags]
        public enum eChartGroupping
        {
             Day  = 1
            ,Week = 2
            ,Month  = 4
            ,Quarter = 8
            ,Year = 16
        }

        public enum eSummaryRows
        {
            [Description("New Users")] User,
            [Description("New Authors")] Authors,
            [Description("New Courses")] Courses,
            [Description("Sales")] Sales
        }

        public enum eSaleSummaryRows
        {
            [Description("Seller")] Seller,
            [Description("Sales")] Sales

        }

        public enum eAnalyticChartKinds
        {
             Count
            ,Value
        }

        public enum eStatsTarget
        {
             Dahsboard
            , Analytics
        }

        public enum eDailyStatsFields
        {
            ItemsCreated,
            ItemsPublished ,
            UsersCreated ,
            WixUsersCreated ,
            UserLogins ,
            AuthorLogins ,
            ReturnUsersLogins ,
            StoresCreated ,
            WixStoresCreated ,
            ItemsPurchased ,
            FreeItemsPurchased,
            TotalVideos,
            TotalUsedVideos
        }

        public enum eTendencyDirections
        {
            Up,
            Down,
            Equal
        }

        public enum eStatsTypes
        {
            [Description("Authors")] Authors,
            [Description("Courses")] Items,
            [Description("Stores")] Stores,
            [Description("Learners")] Learners,
            [Description("Sales")] Sales,
            [Description("Courses")] Courses,
            [Description("Bundles")] Bundles,
            [Description("One Time")] OneTimeSales,
            [Description("Subscription")] Subscription,
            [Description("Rental")] Rental,
            [Description("Free")] Free,
            [Description("MBG")] MBG,
            [Description("Active Authors")] ActiveAuthors,
            [Description("Average Logins")] AvgAuthorLogins,
            [Description("Coupons Created")] CouponsCreated,
            [Description("Dashboard Views")] DashboardViews,
            [Description("Active Learners")] ActiveLearners,
            [Description("Avg. Videos Watched")] AvgVideosWatchedPerCourse,
            [Description("Avg. Logins")] AvgLearnerLogin,
            [Description("Course Preview Watched")] VideoPreviewWatched,
            [Description("Product Page Views")] CoursePreviewEntered,
            [Description("Purchase Page Entered")] PurchasePageEntered,
            [Description("Completed")] PurchaseComplete,
            [Description("Videos Watched")] TotalVideosWatched,
            [Description("Coupons Claimed")] LearnerCouponsClaimed,
            [Description("Value")] LearnerCouponsClaimedValue
        }

    }
}
