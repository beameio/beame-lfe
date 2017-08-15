using LFE.Core.Enums;
using LFE.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace LFE.DataTokens
{
    public class ItemAccessStateToken
    {
        public bool IsOwnedByUser { get; set; }
        public bool IsAccessAllowed { get; set; }
        public bool IsPublished { get; set; }
        public bool IsPreview { get; set; }
        public DateTime? ValidUntill { get; set; }
        public int? MinutesRemind { get; set; }
        
        public bool IsRestOfCanceledSubscription { get; set; }
    }
   
    public class BaseItemDTO
    {
        public int itemId { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public string pageUrl { get; set; }
        public string thumbUrl { get; set; }

        public BillingEnums.ePurchaseItemTypes type { get; set; }
    }
    
    public class BundlePurchaseDataToken : BasePurchaseDataToken
    {
        public BundlePurchaseDataToken()
        {
            UserSavedCards        = new List<PaymentInstrumentDTO>();
            CreditCard            = new CreditCardDTO();
            BillingAddress        = new BillingAddressDTO();
            SavePaymentInstrument = true;
            PaymentMethod         = BillingEnums.ePaymentMethods.Credit_Card;
            IsValid               = true;
        }
        public int BundleId { get; set; }
        public string BundleName { get; set; }
    }
   
    public class CourseBaseDTO : PriceBaseDTO
    {
        public CourseBaseDTO() { }
        public CourseBaseDTO(Guid uid, int authorId)
        {
            Uid = uid;
            AuthorId = authorId;
            CourseId = -1;
        }
        public int CourseId { get; set; }
        public Guid Uid { get; set; }
        public int AuthorId { get; set; }
        public string Name { get; set; }
    }

    public class CourseFbToken : CourseBaseDTO
    {
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string CoursePageUrl { get; set; }
    }

    public class CourseBaseToken : CourseFbToken
    {
       
    }

   
    public class CourseBaseListDTO : CourseBaseDTO
    {      
        public DateTime AddOn { get; set; }
        public string ImageUrl { get; set; }
        public int ReviewCount { get; set; }
        public int LearnerCount { get; set; }
        public CourseEnums.CourseStatus Status { get; set; }
        public bool IsPurchased { get; set; }
    }

    public class CourseListDTO : CourseBaseListDTO
    {
        public string AuthorFullName { get; set; }

        public bool IsLearner { get; set; }

        public string CoursePageUrl { get; set; }

        public string PriceDisplayName { get; set; }
    }

    public class ItemListDTO : PriceBaseDTO
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string ItemPageUrl { get; set; }
        public BillingEnums.ePurchaseItemTypes ItemType { get; set; }
        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public CourseEnums.CourseStatus Status { get; set; }                
        public int ReviewCount { get; set; }
        public int SubscribersCount { get; set; }
        public DateTime AddOn { get; set; }
    }

    public class BundleListDTO : CourseBaseListDTO
    {
        public int BundleId { get; set; }

        public string AuthorFullName { get; set; }

        public string BundlePageUrl { get; set; }
    }

    //COURSE

    public class CourseEditDTO : PriceBaseDTO
    {
        public CourseEditDTO()
        {
            CourseId                = -1;
            Uid                     = Guid.NewGuid();
            Status                  = CourseEnums.CourseStatus.Draft;
            Categories              = new List<int>();  
            PriceLines              = new List<PriceLineDTO>();
            DisplayOtherLearnersTab = true;
        }

        public CourseEditDTO(Guid uid,int authorId)
        {
            CourseId                = -1;
            Uid                     = uid;
            AuthorId                = authorId;
            Status                  = CourseEnums.CourseStatus.Draft;
            Categories              = new List<int>();
            PriceLines              = new List<PriceLineDTO>();
            DisplayOtherLearnersTab = true;
        }

        public CourseEditDTO(string name)
        {
            CourseId                = -1;
            Uid                     = Guid.NewGuid();
            CourseName              = name;
            Status                  = CourseEnums.CourseStatus.Draft; 
            Categories              = new List<int>();
            PriceLines              = new List<PriceLineDTO>();
            DisplayOtherLearnersTab = true;
        }

        public int AuthorId { get; set; }

        public int CourseId { get; set; }

        public Guid Uid { get; set; }

        [Required]
        [DisplayName("Course Name")]
        [RegularExpression(Constants.REGEX_NAME_FORMAT, ErrorMessage = Constants.REGEX_NAME_FORMAT_ERROR)]
        public string CourseName { get; set; }

        
        [DisplayName("Description")]
        public string Description{ get; set; }

        [DisplayName("Course Description")]
        [AllowHtml]
        public string CourseDescription { get; set; }

        [Display(Name = "Display Other Learners Tab")]
        public bool DisplayOtherLearnersTab { get; set; }

        [DisplayName("Meta-tags (Free text)")]
        public string MetaTags { get; set; }

        [DisplayName("Categories (Multi-select)")]
        public List<int> Categories { get; set; }

        [Required]
        [DisplayName("Course Video Promo")]
        public long? PromoVideoIdentifier { get; set; }

        [Required]
        [DisplayName("Course Thumbnail")]
        public string ThumbName { get; set; }

        [DisplayName("Class Room")]
        public int? ClassRoomId { get; set; }

        public string ThumbUrl { get; set; }
        
        public UserVideoDto PromoVideo { get; set; }
        
        [DisplayName("Status")]
        public CourseEnums.CourseStatus Status { get; set; }

        public bool IsCoursePurchased { get; set; }

        public bool HasChapters { get; set; }

        public bool IsPriceDefined { get; set; }

        public string PriceDisplayName { get; set; }
    }


    public class BaseCourseInfoDTO : PriceBaseDTO
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CoursePageUrl { get; set; }
    }

    public class CourseInfoDTO : BaseCourseInfoDTO
    {
        public CourseInfoDTO()
        {
            DisplayOtherLearnersTab = true;
        }
        public Guid uid { get; set; }
        public string CourseDescription { get; set; }
        
        public int IsPushed { get; set; }
        public int IsVisibleOnSite { get; set; }
        public int IsAccessibleFromUrl { get; set; }
        public bool IsFreeCourse { get; set; }
        public string ThumbUrl { get; set; }
        public string OverviewVideoIdentifier { get; set; }
        public string MetaTags { get; set; }
        
        public CourseEnums.CourseStatus Status { get; set; }        
        
        public long? FbObjectId { get; set; }
        public bool FbObjectPublished { get; set; }
        public bool DisplayOtherLearnersTab { get; set; }  
        public UserInfoDTO Author { get; set; }

    }

    
    // COURSE & BUNDLE PRICE
    public class PriceBaseDTO
    {
        [DisplayName("Course price")]
        public decimal? Price { get; set; }

        [DisplayName("Monthly Subscription price")]
        public decimal? MonthlySubscriptionPrice { get; set; }

        [DisplayName("Free course")]
        public bool IsFree { get; set; }

        [DisplayName("Affiliate Commission Percent")]
        public decimal? AffiliateCommission { get; set; }

        public BaseCurrencyDTO Currency { get; set; }

        public List<PriceLineDTO> PriceLines { get; set; } 
    }

    public class PriceLineDTO : BaseItemToken
    {
        public PriceLineDTO()
        {
            PriceLineID = -1;
        }

        [Key]
        public int PriceLineID { get; set; }  

        public BillingEnums.ePricingTypes PriceType { get; set; }

        public BillingEnums.eBillingPeriodType? PeriodType { get; set; }
        
        [Required]
        public decimal Price { get; set; }

        public byte? NumOfPeriodUnits { get; set; }

        public BaseCurrencyDTO Currency { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public bool IsItemUnderRGP { get; set; }
    }

    public class CoursePriceDTO :PriceBaseDTO
    {
        public int CourseId { get; set; }
    }

    public class ItemAffiliateCommissionDTO : BaseItemToken
    {

        [DisplayName("Affiliate Commission Percent")]
        public decimal? AffiliateCommission { get; set; }
    }

    public class BundlePriceDTO : PriceBaseDTO
    {
        public int BundleId { get; set; }
    }
    //REVIEW
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public DateTime? Date { get; set; }
        public int? Rating { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int LearnerId { get; set; }
        public string LearnerEmail { get; set; }
        public string LearnerNickname { get; set; }
        public string LearnerFirstName { get; set; }
        public string LearnerLastName { get; set; }
        public string LearnerFullName { get; set; }
    }

    //CHAPTERS

    public class CourseContentToken
    {
        public int CourseId { get; set; }
        public string Name { get; set; }

        public int ContentId { get; set; }

        public Guid Uid { get; set; }

        public CourseEnums.eCourseContentKind Kind { get; set; }

        public ChapterEditDTO Chapter { get; set; }

        public QuizDTO Quiz { get; set; }

        //public string kind { get { return Kind.ToString(); }
            
        //}
        //public string kindStr { get; set; }
    }

    public class ContentSortToken
    {
        public int id { get; set; }
        public CourseEnums.eCourseContentKind kind { get; set; }
    }
    public class ChapterEditDTO
    {
        public ChapterEditDTO()
        {
            ChapterId = -1;
            CourseId = -1;
            Name = "New chapter";
        }

        public ChapterEditDTO(int courseId)
        {
            ChapterId = -1;
            CourseId = courseId;
            Name = "New chapter";
        }

        [Required]
        public int ChapterId { get; set; }
        
        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name must be maximum 50 characters")]
        [DisplayName("Chapter Name")]
        public string Name { get; set; }

        [Required]
        public int? OrderIndex { get; set; }
    }

    public class WizardNewChapterDTO : ChapterEditDTO
    {
        public Guid Uid { get; set; }
    }

    public class WizardChapterListEditDTO
    {
        [Required]
        public int ChapterId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name must be maximum 50 characters")]
        [DisplayName("Chapter Name")]
        public string Name { get; set; }

        public int? OrderIndex { get; set; }
    }

    //CHAPTER VIDEO
    public class VideoListDto : BaseListDTO
    {
        public int ChapterId { get; set; }
    }

    public class VideoListEditDTO
    {
        [Required]
        public int VideoId { get; set; }

        [Required]
        public int ChapterId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name must be maximum 50 characters")]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Video File")]
        public long? VideoIdentifier { get; set; }

        [DisplayName("Summary")]
        [DataType(DataType.Html)]
        [AllowHtml]
        public string SummaryHTML { get; set; }

        [DisplayName("Open for public viewing")]
        public bool IsOpen { get; set; }
       
        [Required]
        public int? OrderIndex { get; set; }

        public int? fileId { get; set; }
        [DisplayName("Video Name")]
        public string title { get; set; }
        public int views { get; set; }
        public int uses { get; set; }
        public TimeSpan? duration { get; set; }
        public long millisec { get; set; }
        public string minutes { get; set; }
        public string thumbUrl { get; set; }
        public string stillUrl { get; set; }
        public string videoUrl { get; set; }
        public DateTime addon { get; set; }
        public IEnumerable<string> tags { get; set; }
        public string tagsStr { get; set; }
    }

    public class ChapterVideoEditDTO
    {
        public ChapterVideoEditDTO()
        {
            ChapterId = -1;
            VideoId = -1;
            Title = "New video";
            VideoToken = new UserVideoDto
            {
                identifier = "-1"
            };
        }

        public ChapterVideoEditDTO(int chapterId)
        {
            ChapterId = chapterId;
            VideoId = -1;
            Title = "New video";
        }

        [Required]
        public int VideoId { get; set; }

        [Required]
        public int ChapterId { get; set; }
                
        [Required]
        [StringLength(50, ErrorMessage = "Name must be maximum 50 characters")]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Video File")]
        public long? VideoIdentifier { get; set; }

        [DisplayName("Summary")]
        [DataType(DataType.Html)]
        [AllowHtml]
        public string SummaryHTML { get; set; }

        [DisplayName("Open for public viewing")]
        public bool IsOpen { get; set; }

        public UserVideoDto VideoToken { get; set; }

        [Required]
        public int? OrderIndex { get; set; }
    }

    //CHAPTER LINK
    public class LinkListDto : BaseListDTO
    {
        public int ChapterId { get; set; }
        public CourseEnums.eChapterLinkKind kind { get; set; }
    }

    public class ChapterLinkEditDTO
    {
        public ChapterLinkEditDTO(){}

        public ChapterLinkEditDTO(int chapterId,CourseEnums.eChapterLinkKind kind)
        {
            ChapterId = chapterId;
            Kind      = kind;
            LinkId    = -1;
            //Title     = kind == CourseEnums.eChapterLinkKind.Link ? "New link" : "New document";
        }

        public int? CourseId { get; set; }

        [Required]
        public int LinkId { get; set; }

        [Required]
        public int ChapterId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]        
        [DisplayName("Link Url")]
        //[RegularExpression(@"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$", ErrorMessage = "please type valid url")]
        public string LinkHref { get; set; }

        [Required]
        public CourseEnums.eChapterLinkKind Kind { get; set; }

        //public string KindName { get; set; }

        [Required]
        public int? OrderIndex { get; set; }
    }

    // BUNDLES
    public class BundleInfoDTO : BaseBundleDTO
    {
        public string BundlePageUrl { get; set; }
        public string ThumbUrl { get; set; }
        public UserInfoDTO Author { get; set; }

        public PriceBaseDTO PriceToken { get; set; }
    }
    public class BaseBundleDTO
    {
        public int BundleId { get; set; }
        public string BundleName { get; set; }
        public long? PromoVideoIdentifier { get; set; }
        public int AuthorId { get; set; }
        public string Description { get; set; }
    }
    public class BundleEditDTO : PriceBaseDTO
    {
        public BundleEditDTO()
        {
            BundleId   = -1;
            Uid        = Guid.NewGuid();
            Status     = CourseEnums.CourseStatus.Draft;
            Categories = new List<int>();
        }

        public BundleEditDTO(Guid uid, int authorId)
        {
            BundleId   = -1;
            Uid        = uid;
            AuthorId   = authorId;
            Status     = CourseEnums.CourseStatus.Draft;
            Categories = new List<int>();
        }

        public BundleEditDTO(string name)
        {
            BundleId   = -1;
            Uid        = Guid.NewGuid();
            BundleName = name;
            Status     = CourseEnums.CourseStatus.Draft;
            Categories = new List<int>();
        }

        public int AuthorId { get; set; }

        public int BundleId { get; set; }

        public Guid Uid { get; set; }

        [Required]
        [DisplayName("Bundle Name")]
        [RegularExpression(Constants.REGEX_NAME_FORMAT, ErrorMessage = Constants.REGEX_NAME_FORMAT_ERROR)]
        public string BundleName { get; set; }

        [Required]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Meta-tags (Free text)")]
        public string MetaTags { get; set; }

        [DisplayName("Categories (Multi-select)")]
        public List<int> Categories { get; set; }

        [Required]
        [DisplayName("Bundle Video Promo")]
        public long? PromoVideoIdentifier { get; set; }

        [Required]
        [DisplayName("Bundle Thumbnail")]
        public string ThumbName { get; set; }
        
        public string ThumbUrl { get; set; }

        public UserVideoDto PromoVideo { get; set; }

        [DisplayName("Status")]
        public CourseEnums.CourseStatus Status { get; set; }
        public bool IsBundlePurchased { get; set; }
        public bool HasCourses { get; set; }
        public bool IsPriceDefined { get; set; }
       
    }

    public class BundleCourseListDTO : BaseListDTO
    {
        public int rowId { get; set; }
        public string url { get; set; }
        public string desc { get; set; }
        public string pageUrl { get; set; }
        public CourseEnums.CourseStatus status { get; set; }

        public decimal price { get; set; }
    }

    #region User portal
    public class LearnerCourseViewerDTO
    {
        public LearnerCourseViewerDTO()
        {
            LastChapterId = -1;
            LastVideoId   = -1;
        }

        public int CourseId { get; set; }
        public UserBaseDTO AuthorUserBaseDto { get; set; }
        public string CourseName { get; set; }
        public string ThumbUrl { get; set; }
        public string MetaTags { get; set; }
        public int Rating { get; set; }
        public int? ClassRoomId { get; set; }
        public List<ContentTreeViewItemDTO> Chapters { get; set; } 
        public VideoNavigationToken[] VideosNavigation { get; set; }

        public int LastChapterId { get; set; }
        public int LastVideoId { get; set; }

       // public int AuthorId { get; set; }

        //public Guid Uid { get; set; }
        
        //[DisplayName("Description")]
        //public string Description { get; set; }
      

        //[DisplayName("Course Video Promo")]
        //public long? PromoVideoIdentifier { get; set; }             

        //public AuthorVideoDTO PromoVideo { get; set; }

        //[DisplayName("Status")]
        //public CourseEnums.CourseStatus Status { get; set; }
    }


    public class VideoNavigationToken
    {
        public VideoNavigationToken()
        {
            prevId = -1;
            nextId = -1;
        }
        public int chapterId { get; set; }
        public int prevId { get; set; }
        public int videoId { get; set; }
        public string bcId { get; set; }
        public int nextId { get; set; }
    }

    

    public class ContentTreeViewItemDTO
    {
        public ContentTreeViewItemDTO()
        {
            videos    = new List<ContentTreeViewItemDTO>();
            available = true;
        }

       // [Key]
        public int  id { get; set; }

        public Guid? quizId { get; set; }

        public string name { get; set; }
        public CourseEnums.eContentTreeViewItemType type { get; set; }
        public string bcId { get; set; }
        public string desc { get; set; }
        public string thumb { get; set; }
        public string duration { get; set; }
        public bool IsOpen { get; set; }

        public bool available { get; set; }

        public int index { get; set; }
        
        public List<ContentTreeViewItemDTO> videos { get; set; } 
        
    }

    public class ChapterVideoDTO
    {
        public ChapterVideoDTO(){}

        public ChapterVideoDTO(int chapterId)
        {
            ChapterId = chapterId;
            Title = "Empty chapter";
        }
        public int ChapterId { get; set; }
        public string Title { get; set; }
    }
    
    public class ChapterLinkListToken
    {
        public string name { get; set; }

        [AllowHtml]
        public string url { get; set; }

        public int? index { get; set; }

        public CourseEnums.eChapterLinkKind kind { get; set; }
    }

    public class CourseUserReviewDTO
    {
        public CourseUserReviewDTO()
        {
            ReviewId = -1;
            Rating   = 0;
        }

        public int ReviewId { get; set; }

        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Text { get; set; }
                
        public int? Rating { get; set; }
    }


    public class TreeWebStoreDTO : BaseWebStoreDTO
    {
        public TreeWebStoreDTO()
        {
            Categories = new List<TreeWebStoreCategoryDTO>();
        }
        public int Id { get; set; }        
        public List<TreeWebStoreCategoryDTO> Categories { get; set; }

        public bool CourseIncluded { get; set; }
    }

    public class TreeWebStoreCategoryDTO 
    {

        public int Id { get; set; }
        public string Name { get; set; }

        public bool Attached { get; set; }

        public int? WebstoreItemId { get; set; }
    }
    #endregion
}
