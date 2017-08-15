using System.Web.Mvc;
using LFE.Core.Enums;
using System.Collections.Generic;
using System;


namespace LFE.DataTokens
{
    #region Index
    public class WidgetWebStoreDTO
    {
        public int WebStoreID { get; set; }
        public string WebStoreName { get; set; }
        public string GoogleAnalyticCode { get; set; }
        public string MetaTags { get; set; }
        public string Description { get; set; }
        public int OwnerUserID { get; set; }
        public string TrackingID { get; set; }
        public short? CurrencyId { get; set; }

        public WebStoreEnums.StoreStatus Status { get; set; }
        public string WixInstanceID { get; set; }

        public string FontColor { get; set; }
        public string BackgroundColor { get; set; }
        public string TabsFontColor { get; set; }
        public bool IsTransParent { get; set; }
        public bool IsShowBorder { get; set; }
        public bool IsShowTitleBar { get; set; }
        public DateTime LastUpdate { get; set; }
        public string UniqueId { get; set; }

        public string WixSiteUrl { get; set; }
    }


    public class WidgetItemListDTO
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public BillingEnums.ePurchaseItemTypes ItemType { get; set; }
        public string AuthorName { get; set; }
        public int AuthorID { get; set; }
        public string ImageURL { get; set; }
        public decimal? Price { get; set; }
        public decimal? MonthlySubscriptionPrice { get; set; }
        public int NumSubscribers { get; set; }
        public int Rating { get; set; }
        public bool IsFree { get; set; }
        public string ItemUrlName { get; set; }
        public int TotalCourses { get; set; }        
        public bool IsItemOwner { get; set; }
        public bool IsAuthorUnderRGP { get; set; }
        public bool IsItemUnderRGP {get; set;}       
        public int CoursesCnt { get; set; }
        public List<PriceLineDTO> ItemPrices { get; set; }        
    }

    public class WidgetCategoryDTO
    {
        public int WebStoreCategoryID { get; set; }
        public int? LfeCategoryID { get; set; }
        public string CategoryName { get; set; }
        public string CategoryUrlName { get; set; }
        public string Description { get; set; }
        public int Ordinal { get; set; }
    }

    public class WidgetIndexDTO
    {
        public List<WidgetItemListDTO> CoursesList { get; set; }
        public WidgetWebStoreDTO Webstore { get; set; }
        public WidgetCategoryDTO Category { get; set; }
    }


    public class BaseModelViewToken : BaseModelState
    {
        public WidgetCategoryDTO Category { get; set; }
        public List<WidgetCategoryDTO> CategoriesList { get; set; }
        public WidgetWebStoreDTO WebStore { get; set; }
        public string CategoryName { get; set; }
        public LoginViewModelToken LoginViewModel { get; set; }
        public string ParentURL { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public WebStoreEnums.StoreStatus Status { get; set; }
        public bool IsSingleCourseStore { get; set; }
        public bool IsUserPurchasesCategory { get; set; }
        public int? NumCourses { get; set; }
        public string WixViewMode { get; set; }
        public string WixAppLastUpdateDate { get; set; }
        public bool IsUnderSSL { get; set; }
        public string TrackingId { get; set; }
    }

    public class IndexModelViewToken
    {
        public List<WidgetItemListDTO> ItemsList { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Sort { get; set; }
        public int TotalCourses { get; set; }
        public string CategoryName { get; set; }
    }
    #endregion

    #region course purchase


    

    public class CoursePurchaseDTO : BasePurchaseDTO
    {
        public int? CourseId { get; set; }
        public string CourseName { get; set; }
        public int Rating { get; set; }
        public List<ContentTreeViewItemDTO> Chapters { get; set; }
        public VideoNavigationToken[] VideosNavigation { get; set; }
        public int? ClassRoomId { get; set; }
        public string CourseUrlName { get; set; }

    }

    public class BundlePurchaseDTO : BasePurchaseDTO
    {
        public int? BundleId { get; set; }
        public string BundleName { get; set; }
        public string BundleUrlName { get; set; }

        public List<BundleCourseListDTO> BundleCourses { get; set; } 
    }

    public class BasePurchaseDTO : BaseModelState
    {
        public UserBaseDTO User { get; set; }       
        public string ThumbUrl { get; set; }
        public string MetaTags { get; set; } 
        public int? NumSubscribers { get; set; }
        public decimal? Price { get; set; }
        public decimal? MonthlySubscriptionPrice { get; set; }
        public bool IsFreeCourse { get; set; }

        [AllowHtml]
        public string IntroHtml { get; set; }
        public string OverviewVideoIdentifier { get; set; }
        public ItemAccessStateToken ItemState { get; set; }

        public string TrackingID { get; set; }

        public BaseCurrencyDTO Currency { get; set; }

        public List<PriceLineDTO> PriceLines { get; set; } 
    }



    public class AuthorPurchaseDTO : BaseUserDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }

        public string BioHtml { get; set; }
        public string AuthorPictureURL { get; set; }
        public string PictureURL { get; set; }

        public List<WidgetItemListDTO> ItemsList { get; set; }
    }

    public class WidgetCourseReviewsToken
    {
        public List<ReviewDTO> ReviewsList { get; set; }
        public WidgetItemListDTO Item { get; set; }
    }
    #endregion

    #region Account
    public class LoginViewModelToken
    {
        public bool IsLoggedIn { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string LoginError { get; set; }
        public string RedirectUrl { get; set; }
    }


    #endregion
}
