using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Portal.Areas.AuthorAdmin.Models
{
    public enum ePageModes
    {
        Report
        ,EditForm
    }

    public enum eChapterContentKinds
    {
        Video
        ,Link
        ,Document
    }

    public enum eStoreContentKinds
    {
        Category
        ,Single
        ,Author
        ,LfeCategory
        ,My
    }

    public class BasePartialViewToken
    {
        public string Action { get; set; }
        public int? Id { get; set; }
    }

    public class CouponReportPageToken : BasePartialViewToken
    {
        public CouponReportPageToken()
        {
            PageSize = 10;
        }
        public bool ShowAuthorFields { get; set; }

        public int PageSize { get; set; }        
    }

    public class EditCoursePageToken
    {
        public string title { get; set; }
        public CommonEnums.ePageMode mode { get; set; }
        public UserViewDto user { get; set; }        
        public BaseEntityDTO course { get; set; }
    }

    public class EditBundlePageToken
    {
        public string title { get; set; }
        public CommonEnums.ePageMode mode { get; set; }
        public UserViewDto user { get; set; }
        public BaseEntityDTO bundle { get; set; }
    }

    public class CourseWizardPageToken
    {
        public string title { get; set; }
        public UserViewDto user { get; set; }
        public CourseWizardDto WizardDto { get; set; }
    }

    public class EditVideoToken
    {
        public UserVideoDto videoDTO { get; set; }
        public CommonEnums.ePageMode mode { get; set; }

        [RegularExpression("^[0-9A-Za-z ,]+$", ErrorMessage = "Invalid Tag")]
        public string Tags { get; set; }

        public string S3SignedUrl { get; set; }
    }

    public class SalesReportConfigToken
    {
        public int pageSize { get; set; }
        public bool showPageSizes { get; set; }
    }

    public class EditWebStorePageToken
    {
        public int? ownerUserId { get; set; }
        public string title { get; set; }
        public CommonEnums.ePageMode mode { get; set; }
        public UserViewDto user { get; set; }
        public BaseEntityDTO store { get; set; }
    }

    public class WebStoreAddByCategoryToken
    {
        [Required(ErrorMessage = "StoreCategory Required")]
        public int WebStoreCategoryId { get; set; }

        [Required(ErrorMessage = "Category Required")]
        public int? CategoryId { get; set; }

        public BaseWebstoreItemListDTO[] ItemList { get; set; }
    }

    public class WebStoreAddByAuthorToken
    {
        [Required(ErrorMessage = "StoreCategory Required")]
        public int WebStoreCategoryId { get; set; }

        [Required(ErrorMessage = "Author Required")]
        public int? AuthorId { get; set; }

        public BaseWebstoreItemListDTO[] ItemList { get; set; }
    }

    public class WebStoreAddByCurrentUserToken
    {
        [Required(ErrorMessage = "StoreCategory Required")]
        public int WebStoreCategoryId { get; set; }

        [Required(ErrorMessage = "Author Required")]
        public int? AuthorId { get; set; }

        public ItemListToken ItemListToken { get; set; }

        //public BaseWebstoreItemListDTO[] ItemList { get; set; }
    }

    public class ItemListToken
    {      
        public BaseWebstoreItemListDTO[] ItemList { get; set; }
    }

    public class RoomManagePageToken
    {
        public DiscussionClassRoomDTO RoomDto { get; set; }

        public List<BaseListDTO> Courses { get; set; }
    }

    public class AuthorVideosPageToken
    {
        public UserViewDto user { get; set; }
        public int ListPageSize { get; set; }
        public bool IsDynamicPageSize { get; set; }       
    }

    public class BundleCoursesPageToken : BaseEntityDTO
    {
        public bool IsPurchased { get; set; }
    }

    public class CourseStoresToken : BaseModelState
    {
        public int CourseId { get; set; }
        public List<TreeWebStoreDTO> Stores { get; set; }
    }

    public class ChapterContentsToken
    {
        public int ChapterId { get; set; }    
    }

    public class WebstoreCategoryContentsToken
    {
        public int CategoryId { get; set; }
    }

    public class PriceLinesManageToken
    {
        public BaseItemToken Item { get; set; }

        public bool IsFree { get; set; }

        public BaseCurrencyDTO Currency { get; set; }
    }
    public class RefundSettingsModel
    {
        public RefundSettingsModel()
        {
            ShowJoinedSuccess = ShowRemovedSuccess = false;
            IsValid = true;
            Error = string.Empty;
        }
        public bool ShowJoinedSuccess { get; set; }
        public bool ShowRemovedSuccess { get; set; }
        public bool IsValid { get; set; }
        public string Error { get; set; }
    }

    public class CourseContentsManageToken : BaseEntityDTO
    {
        public int TotalQuizzes { get; set; }
    }
}