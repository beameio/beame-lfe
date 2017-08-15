
using System;
using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class CommonEnums
    {

        public enum SocialProviders
        {
            Facebook,
            Google,
            Twitter,
            Live
        }

        public enum eVideoPictureTypes
        {
            Thumb,
            Still
        }

        [Flags]
        public enum eAdminStatuses
        {
             [Description("Active")]    ACTIVE  = 1
            ,[Description("Inactive")] INACTIVE = 2
            ,[Description("Suspend")] SUSPEND   = 4
            ,[Description("Pending")] PENDING   = 8
            ,[Description("Accepted")] ACCEPTED = 16	
            ,[Description("Rejected")] REJECTED = 32
        }

        [Flags]
        public enum UserRoles
        {
             System                                    = 1
            ,[Description("Administrator")] Admin      = 2
            ,[Description("Author")] Author            = 4
            ,[Description("Learner")] Learner          = 8
            ,[Description("Affiliate")] Affiliate      = 16
            ,[Description("Guest")] Guest              = 32
            ,[Description("Tester")] Tester            = 64
            ,[Description("Store Owner")] StoreOwner   = 128
            ,[Description("Hub Admin")] HubAdmin       = 256
            ,Unknown                                   = 4096
        }
        public enum eRegistrationSources
        {
             Unknown                              = 0
            ,[Description("LFE")] LFE             = 1
            ,[Description("Wix")] WIX             = 2
            ,[Description("Facebook")] FB         = 3
            ,[Description("Wordpress")] WORDPRESS = 4
            ,[Description("Drupal")] DRUPAL       = 5
            ,[Description("Joomla")] JOOMLA       = 6
            ,[Description("Hub")] HUB             = 7

        }

        public enum LoggerObjectTypes
        {
             Unknown
            ,Author
            ,Dashboard
            ,Billing
            ,Course
            ,CourseWizard
            ,UserCourse
            ,UserAccount
            ,Coupon
            ,Video
            ,Learner
            ,S3Wrapper
            ,BrightcoveWrapper
            ,ServiceUploader
            ,EventLogs
            ,PayPal
            ,SalesOrder
            ,WebStore
            ,Widget
            ,Discussion
            ,UserNotification
            ,FB
            ,Geo
            ,Email
            ,UnitOfWork
            ,AdminSettings
            ,Plugin
            ,Application
            ,Wix
            ,IPadApi
            ,Reports
            ,Mailchimp
            ,DownloadManager
            ,Payout
            ,Quiz
            ,Certificate
            ,PortalAdmin
            ,ProvisionApi
        }

        public enum eLogLevels
        {
            Error
            ,Debug
            ,Fatal
            ,Warn
        }

        public enum eEventOwnerTypes
        {
            [Description("Common")] Common = 1
            ,[Description("Learner")] Learner = 2
            ,[Description("Author")] Author = 3
        }
        public enum eUserEvents
        {
             [Description("Registration Succeeded")] REGISTRATION_SUCCESS      = 1
            ,[Description("Registration Failed")] REGISTRATION_FAILED          = 2
            ,[Description("Login Succeeded")] LOGIN_SUCCESS                    = 3
            ,[Description("Login Failed")] LOGIN_FAILED                        = 4
            ,[Description("Video Preview Watch")] VIDEO_PREVIEW_WATCH          = 5
            ,[Description("Video Course Watch")] VIDEO_COURSE_WATCH            = 6
            ,[Description("Buy Page Entered")] BUY_PAGE_ENTERED                = 7
            ,[Description("Purchase completed")] PURCHASE_COMPLETE             = 8
            ,[Description("Purchase failed")] PURCHASE_FAILED                  = 9
            ,[Description("Search usage")] SEARCH_USAGE                        = 10
            ,[Description("Session start")] SESSION_START                      = 11
            ,[Description("Dashboard View")] DASHBOARD_VIEW                    = 12
            ,[Description("Course Created")] COURSE_CREATED                    = 13
            ,[Description("Bundle Created")] BUNDLE_CREATED                    = 23
            ,[Description("Course Published")] COURSE_PUBLISHED                = 19
            ,[Description("Enter Course Viewer")] COURSE_VIEWER_ENTER          = 20
            ,[Description("Enter Course Preview")] COURSE_PREVIEW_ENTER        = 21
            ,[Description("Video Upload")] VIDEO_UPLOAD                        = 14
            ,[Description("Store Created")] STORE_CREATED                      = 15
            ,[Description("ClassRoom Created")] ROOM_CREATED                   = 16
            ,[Description("Wix App Published")] WIX_APP_PUBLISHED              = 17
            ,[Description("Wix App Deleted")] WIX_APP_DELETED                  = 18
            ,[Description("Store view")] STORE_VIEW                            = 22
            ,[Description("Checkout login")] CHECKOUT_LOGIN                    = 24
            ,[Description("Checkout register")] CHECKOUT_REGISTER              = 25
            ,[Description("Checkout error")] CHECKOUT_ERROR                    = 26
            ,[Description("Mailchimp connected")] MAILCHIMP_CONNECTED          = 27
            ,[Description("Mailchimp segments refreshed")] MAILCHIMP_REFRESHED = 28
            ,[Description("Plugin Installation")] PLUGIN_INSTALLATION          = 29
            ,[Description("Join MBG")] MBG_JOIN                                = 30
            ,[Description("Cancel MBG")] MBG_CANCEL                            = 31
            ,[Description("Quiz Created")] QUIZ_CREATED                        = 32
        }

        [Flags]
        public enum eEventItemTypes
        {
             Unknown = 0
            ,[Description("Course")] COURSE = 1
            ,[Description("Bundle")] BUNDLE = 2
        }

        public enum ePageMode
        {
            insert
            ,edit
            ,view
        }

        public enum eApplParameters
        {
            ActivationEmailLinkFormatString
            ,AdminBaseUrl
            ,AllCoursesCategoryKey
            ,BrightcoveReadToken
            ,BrightcoveWriteToken
            ,CategoryCoursesPageSize
            ,EmailNameRegistration
            ,EmailNameService
            ,ExpressCheckoutUrlFormatString
            ,FacebookAppId
            ,MyCoursesCategoryKey
            ,NonProductionCompanyPassword
            ,RegistrationConfirmationAccount
            ,RegistrationConfirmationPassword
            ,S3AccessKeyID
            ,S3BucketName
            ,S3Secret
            ,SiteBaseUrl
        }

        public enum SortDirections
        {
             asc
            ,desc
            ,none
        }

        public enum eGenders
        {
            male = 1
            ,female = 2
        }
        
    }
}
