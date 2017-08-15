using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class UserEventDTO
    {
        [DisplayName("Event Id")]
        public long EventId { get; set; }

        [DisplayName("SessionId")]
        public long SessionId { get; set; }

        [DisplayName("ASP.Net SessionId")]
        public string AspNetSessionId { get; set; }

        public int? UserId { get; set; }

        [DisplayName("User Name")]
        public string UserName { get; set; }

        public string UserPhotoUrl { get; set; }

        [DisplayName("IP")]
        public string IPAddress { get; set; }

        [DisplayName("Host")]
        public string HostName { get; set; }

        [DisplayName("Http Headers")]
        public string HttpHeaders { get; set; }
        
        public DateTime SessionDate { get; set; }

        [DisplayName("Event")]
        public string EventType { get; set; }

        [DisplayName("Add. Data")]
        public string AdditionalData { get; set; }

        [DisplayName("Event Date")]
        public DateTime EventDate { get; set; }

        public int? ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemPageUrl { get; set; }
        public int? ItemAuthorId { get; set; }
        public string ItemAuthorName { get; set; }
        
        public int? StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreUrl { get; set; }
        public int? StoreOwnerId { get; set; }
        public string StoreOwnerName { get; set; }


        public long? BcIdentifier { get; set; }
        public string VideoName { get; set; }
        public int? VideoAuthorId { get; set; }
        public string VideoAuthorName { get; set; }
       
    }

    public class SystemLogDTO
    {
        public long id { get; set; }
        public string Origin { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string StackTrace { get; set; }        
        public Guid? GuidId { get; set; }
        public int? IntId { get; set; }
        public DateTime AddOn { get; set; }
        public string Module { get; set; }

        public long? SessionId { get; set; }

        public string IpAddress { get; set; }

        public string HostName { get; set; }

        public BaseUserInfoDTO User { get; set; }
    }

    public class FileInterfaceLogDTO
    {
        public int FileId { get; set; }        
        [DisplayName("Path")]
        public string FilePath { get; set; }
        public string ETag { get; set; }
        [DisplayName("Type")]
        public string ContentType { get; set; }
        [DisplayName("BcId")]
        public long? BcIdentifier { get; set; }
        [DisplayName("Size")]
        public long? FileSize { get; set; }
        public string Title { get; set; }
        public string Tags { get; set; }
        public string Status { get; set; }
        [DisplayName("Add on")]
        public DateTime AddOn { get; set; }
        [DisplayName("Upd. on")]
        public DateTime? UpdateOn { get; set; }
        public UserLogDTO User { get; set; }
    }

    public class UserLogDTO
    {
        public int id { get; set; }
        [DisplayName("User")]
        public string name { get; set; }
        public string url { get; set; }
    }

    public class FbPostInterfaceLogDTO
    {
        public string FacebookID { get; set; }
        public long PostId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string LinkedName { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Error { get; set; }
        public string FbPostId { get; set; }
        public string Status { get; set; }
        [DisplayName("Add on")]
        public DateTime AddOn { get; set; }
        [DisplayName("Post on")]
        public DateTime? PostOn { get; set; }
        public UserLogDTO User { get; set; }
    }

    public class EmailInterfaceLogDTO
    {
        public long EmailId { get; set; }
        public int UserId { get; set; }
        public string Subject { get; set; }
        public string ToEmail { get; set; }        
        public string MessageBoby { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }        
        [DisplayName("Add on")]
        public DateTime AddOn { get; set; }
        [DisplayName("Send on")]
        public DateTime? SendOn { get; set; }
        public UserLogDTO User { get; set; }
    }

    public class SummaryReportDTO :BaseModelState
    {
        public SummaryReportDTO()
        {
            Periods = new List<string>();

            Rows = new List<SummaryReportRowDTO>();

        }

        public List<string> Periods { get; set; }

        public List<SummaryReportRowDTO> Rows { get; set; }

        public DataTable DT { get; set; }
    }

    public class SummaryReportRowDTO
    {
        public string Period { get; set; }

        public long Users { get; set; }
        public long Authors { get; set; }
        public long Courses { get; set; }

        public decimal Sales { get; set; }
    }

    public class SaleSummaryReportDTO : BaseModelState
    {
        public SaleSummaryReportDTO()
        {
            CurrencyRows = new List<PayoutCurrencySummaryDTO>();
        }

        public List<PayoutCurrencySummaryDTO> CurrencyRows { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public PayoutExecutionDTO PayoutExecution{get;set;}
    }

    public class PayoutCurrencySummaryDTO
    {
        public PayoutCurrencySummaryDTO()
        {
            Rows = new List<PayoutUserMonthlyStatementDTO>();
        }

        public BaseCurrencyDTO Currency { get; set; }

        public List<PayoutUserMonthlyStatementDTO> Rows { get; set; }
    }

    public class PayoutUserMonthlyStatementDTO
    {
        public PayoutUserMonthlyStatementDTO()
        {
            PayoutSettings = new PayoutSettingsDTO();
            uid = new Guid();
        }
        public Guid uid { get; set; }
        public int SellerId { get; set; }
        public BaseUserInfoDTO Seller { get; set; }
        public PayoutSettingsDTO PayoutSettings { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public BaseCurrencyDTO Currency { get; set; }
        public decimal TotalSales { get; set; }
        public decimal Sales { get; set; }

        public decimal AffiliateSales { get; set; }

        
        public decimal Fees { get; set; }

        public decimal AffiliateCommission { get; set; }
        public decimal AffiliateFees { get; set; }
        public decimal Refund { get; set; }
        public decimal RefundFees { get; set; }


        public decimal RefundProgrammToHold { get; set; }
        public decimal RefundProgrammToRelease { get; set; }

        public decimal Balance { get; set; }
        public decimal LfeCommission { get; set; }
        public decimal Payout { get; set; }

        public BillingEnums.ePayoutStatuses PayoutStatus { get; set; }

    }
    public class PayoutSettingsDTO : BaseModelState
    {
        public BillingEnums.ePayoutTypes? PayoutType { get; set; }
        
        public string Email { get; set; }

        public string Address { get; set; }
    }

    public class BaseChartPointToken : BaseKpiChardDTO
    {
        public int value { get; set; }
    }

    //KPI
    public class BaseKpiChardDTO
    {
        public DateTime date { get; set; }
    }

    public class KpiViewsChartDTO : BaseKpiChardDTO
    {       
       
        public int authors { get; set; }

        public int stores { get; set; }

        public int items { get; set; }

        public int videos { get; set; }
    }

    public class StatementKpiChartDTO : BaseKpiChardDTO
    {       
        public int sales { get; set; }

        [Description("Store Views")]
        public int stores { get; set; }

        public int items { get; set; }        
    }

    public class DashboardKpiChartDTO : StatementKpiChartDTO
    {
        [Description("Product Page Views")]
        public new int items { get; set; }        

        [Description("Buy page Entered")]
        public int buy_entered { get; set; }

        [Description("Video Preview Watch")]
        public int video_preview_watch { get; set; }

        [Description("Purchase Complete")]
        public int purchase_complete { get; set; }
    }

    public class KpiDashboardFiltersToken
    {
        public KpiDashboardFiltersToken()
        {
            Authors = new List<BaseListDTO>();
            Stores  = new List<BaseListDTO>();
            Items   = new List<EventItemListDTO>();
        }  
        public List<BaseListDTO> Authors { get; set; }
        public List<BaseListDTO> Stores { get; set; }
        public List<EventItemListDTO> Items { get; set; }
    }

    public class EventItemListDTO : BaseListDTO
    {
        public CommonEnums.eEventItemTypes ItemType { get; set; }
    }

    public class AuthorStatementRequestToken
    {
        public int userId { get; set; }

        public int year { get; set; }

        public int month { get; set; }

        public short currencyId { get; set; }
    }

    public class KpiDetailRowDTO
    {
        public BaseUserInfoDTO Author { get; set; }
        public BaseWebStoreDTO Store { get; set; }

        public EventItemListDTO Item { get; set; }

        public int Views { get; set; }
    }

    public class FunnelRequestToken
    {
        public ReportEnums.ePeriodSelectionKinds period { get; set; }
        public int? authorId { get; set; }
        public int? storeId{ get; set; }
        public int? itemId{ get; set; }
        public CommonEnums.eEventItemTypes? itemType{ get; set; }
    }

    public class FunnelViewToken
    {
        public FunnelRequestToken RequestToken { get; set; }

        public List<FunnelViewsChartDTO> ChartData { get; set; }
    }

    public class FunnelViewsChartDTO
    {
        public FunnelViewsChartDTO() { }

        public FunnelViewsChartDTO(DateTime datePoint, int? productViews, int? clipViews, int? buyEneted,int? purchaseCompleted)
        {
            date              = datePoint;
            ProductViews      = productViews ?? 0;
            ClipViews         = clipViews ?? 0;
            BuyEntered        = buyEneted ?? 0;
            PurchaseCompleted = purchaseCompleted ?? 0;
        }        
        public DateTime date { get; set; }
        public int ProductViews { get; set; }

        public int ClipViews { get; set; }

        public int BuyEntered { get; set; }

        public int PurchaseCompleted { get; set; }
    }


    public class FactDailyStatsDTO
    {
       
        public DateTime FactDate { get; set; }
        public int ItemsCreated { get; set; }
        public int ItemsPublished { get; set; }
        public int UsersCreated { get; set; }
        public int WixUsersCreated { get; set; }
        public int UserLogins { get; set; }
        public int AuthorLogins { get; set; }
        public int ReturnUsersLogins { get; set; }
        public int StoresCreated { get; set; }
        public int WixStoresCreated { get; set; }
        public int ItemsPurchased { get; set; }
        public int FreeItemsPurchased { get; set; }
    }

    public class FactOwnerDailyEventStatsDTO
    {
        public DateTime FactDate { get; set; }

        public int TotalEvents { get; set; }

        public int CommonEvents { get; set; }

        public int LearnerEvents { get; set; }

        public int AuthorEvents { get; set; }

    }
    public class FactDailyEventStatsDTO
    {
        public DateTime FactDate { get; set; }

        public int TotalEvents { get; set; }
        public int REGISTRATION_SUCCESS { get; set; }

        public int LOGIN_SUCCESS { get; set; }

        public int VIDEO_PREVIEW_WATCH { get; set; }

        public int VIDEO_COURSE_WATCH { get; set; }

        public int BUY_PAGE_ENTERED { get; set; }

        public int PURCHASE_COMPLETE { get; set; }

        public int DASHBOARD_VIEW { get; set; }

        public int COURSE_CREATED { get; set; }

        public int COURSE_PUBLISHED { get; set; }

        public int COURSE_VIEWER_ENTER { get; set; }

        public int COURSE_PREVIEW_ENTER { get; set; }

        public int VIDEO_UPLOAD { get; set; }

        public int STORE_CREATED { get; set; }

        public int WIX_APP_PUBLISHED { get; set; }

        public int STORE_VIEW { get; set; }
    }

    public class HostEventDTO
    {
        public string Period { get; set; }
        public string HostName { get; set; }
        
        public int TotalEvents { get; set; }
        public int REGISTRATION_SUCCESS { get; set; }
        public int STORE_VIEW { get; set; }
        public int PURCHASE_COMPLETE { get; set; }
        public int COURSE_PREVIEW_ENTER { get; set; }
        public int COURSE_VIEWER_ENTER { get; set; }
        public int CHECKOUT_REGISTER { get; set; }
        public int BUY_PAGE_ENTERED { get; set; }
    }

    public class AbandonHostDTO
    {
        public string HostName { get; set; }
        public DateTime? LastEventDate { get; set; }
        public DateTime? FirstEventDate { get; set; }

        public BaseUserInfoDTO User { get; set; }
        public int? WebStoreId { get; set; }
        public int TotalEvents { get; set; }
        public int PreviewCount { get; set; }
        public int TotalCourses { get; set; }
    }

    public class FactDailyTotalsDTO
    {
        public DateTime FactDate { get; set; }
        public int TotalItems { get; set; }
        public int TotalPublished { get; set; }
        public int Attached2Stores { get; set; }
        public int Attached2WixStores { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAuthors { get; set; }
        public int TotalLearners { get; set; }
        public int ItemsPurchased { get; set; }
        public int FreeItemsPurchased { get; set; }
        public int StoresCreated { get; set; }
        public int WixStoresCreated { get; set; }
    }

    public class VideoUploadsChartDTO
    {
        public DateTime UploadDate { get;set; }
        public int TotalVideos { get; set; }
        public int TotalUsedVideos { get; set; }
    }

    public class VideoDTO 
    {
        public int videoId { get; set; }
        public BaseUserInfoDTO user { get; set; }
        public long? identifier { get; set; }        
        public string name { get; set; }        
        public int uses { get; set; }
        public bool inUse { get; set; }
        public string thumb { get; set; }
        public string duration { get; set; }
        public DateTime addon { get; set; }        
    }

    public class PluginRepDTO
    {
        public int InstallationId { get; set; }
        public PluginEnums.ePluginType Type { get; set; }
        public string TypeName { get; set; }
        public string UId { get; set; }
        public string Domain { get; set; }
        public BaseUserInfoDTO User { get; set; }
        public DateTime? UserAddOn { get; set; }
        public DateTime AddOn { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class VideoStateResultToken : BaseModelState
    {
        public VideoStateResultToken()
        {
            VideosFoundInBC        = 0;
            BcVideosChecked        = 0;
            NewVideosFoundInBC     = 0;
            TotalUserVideos        = 0;
            TotalAttachedVideos    = 0;
            TotalNonAttachedVideos = 0;
            TotalNonExistsVideos = 0;
            TotalAttachedNonExistsVideos = 0;
        }
        [Description("Videos found in Brightcove")]
        public int VideosFoundInBC { get; set; }
        
        public int BcVideosChecked { get; set; }

        [Description("New Videos found in Brightcove")]
        public int NewVideosFoundInBC { get; set; }

        [Description("Total Users Videos")]
        public int TotalUserVideos { get; set; }

        [Description("Total attached videos")]
        public int TotalAttachedVideos { get; set; }

        [Description("Total non attached videos")]
        public int TotalAttachedNonExistsVideos { get; set; }

        [Description("Total not exists videos")]
        public int TotalNonExistsVideos { get; set; }

        [Description("Total attached not exists videos")]
        public int TotalNonAttachedVideos { get; set; }
    }
}
