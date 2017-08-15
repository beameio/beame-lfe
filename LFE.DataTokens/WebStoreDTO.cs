using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.Core.Utils;

namespace LFE.DataTokens
{
    //web store
    public class BaseWebStoreDTO
    {
        public BaseWebStoreDTO()
        {
            RegistrationSource = CommonEnums.eRegistrationSources.LFE;
        }

        public int StoreId { get; set; }
        public string TrackingID { get; set; }
        public string Name { get; set; }

        [DisplayName("Status")]
        public WebStoreEnums.StoreStatus Status { get; set; }

        public short? DefaultCurrencyId { get; set; }

        public CommonEnums.eRegistrationSources RegistrationSource { get; set; }
}   
    public class WixStoreDTO : BaseWebStoreDTO
    {
        public BaseUserInfoDTO Owner { get; set; }
        
        public DateTime AddOn { get; set; }
        public Guid? WixInstanceID { get; set; }

        public string WixSiteUrl { get; set; }
    }

    public class StoreReportDTO : BaseWebStoreDTO
    {
        public BaseUserInfoDTO Owner { get; set; }
        public DateTime AddOn { get; set; }
        public Guid? WixInstanceID { get; set; }
        public string SiteUrl { get; set; }
        public bool IsOwnerAdmin { get; set; }
        public bool IsAffiliate { get; set; }
        public int TotalItems { get; set; }
        public int TotalAffiliateItems { get; set; }
        public int TotalOwnedItems { get; set; }
    }

    public class WebStoreGridDTO :BaseWebStoreDTO
    {
        public Guid Uid { get; set; }
        public int OwnerId { get; set; }
        public DateTime AddOn { get; set; }        
        public int CoursesCount { get; set; }
        public int SubscribersCount { get; set; }

        public string WixSiteUrl { get; set; }
        [DisplayName("Default Currency")]
        public short? CurrencyId { get; set; }
    }

    public class WebStoreEditDTO :BaseWebStoreDTO
    {

        public WebStoreEditDTO()
        {
            StoreId    = -1;
            Uid        = Guid.NewGuid();
            Status     = WebStoreEnums.StoreStatus.Draft;
            CurrencyId = Constants.DEFAULT_CURRENCY_ID;
            TrackingID = Guid.NewGuid().ToString();
        }

         public WebStoreEditDTO(string storeName)
        {
            StoreId    = -1;
            Uid        = Guid.NewGuid();
            StoreName  = storeName;
            Status     = WebStoreEnums.StoreStatus.Draft;
            CurrencyId = Constants.DEFAULT_CURRENCY_ID;
            TrackingID = Guid.NewGuid().ToString();
        }

        public Guid Uid { get; set; }

        public int? OwnerUserId { get; set; }

        [Required]
        [DisplayName("Store Name")]
        [Remote("IsStoreNameAvailable", "WebStore", AdditionalFields = "StoreId")]
        [StringLength(256, ErrorMessage = "Name must be maximum 256 characters")]
        [RegularExpression(Constants.REGEX_NAME_FORMAT, ErrorMessage = Constants.REGEX_NAME_FORMAT_ERROR)]
        public string StoreName { get; set; }

        [Required]
        [DisplayName("TrackingID")]        
        [Remote("IsTrackingIDAvailable","WebStore",AdditionalFields = "Uid")]
        [StringLength(50, ErrorMessage = "Name must be maximum 50 characters")]
        [RegularExpression(@"(\S)+", ErrorMessage = "White space is not allowed.")]
        public new string TrackingID { get; set; }

        //[Required]
        [DisplayName("Description")]
        public string Description { get; set; }
        
        [DisplayName("Meta-tags (Free text)")]
        public string MetaTags { get; set; }

        //[DisplayName("Status")]
        //public WebStoreEnums.StoreStatus Status { get; set; }

        [DisplayName("Default Currency")]
        public short? CurrencyId { get; set; }


        public Guid? WixUid { get; set; }
        
        //added by Idan 
        public Guid? WixInstanceId { get; set; }

        public string WixSiteUrl { get; set; }

        public string FontColor { get; set; }

        public string BackgroundColor { get; set; }

        public string TabsFontColor { get; set; }

        public bool IsTransparent { get; set; }

        public bool IsShowBorder { get; set; }

        public bool IsShowTitleBar { get; set; }
    }

    //category
    public class BaseWebStoreCategoryDTO
    {
        public int WebStoreCategoryId { get; set; }

        [Required]
        [DisplayName("Category Name")]
        [Remote("IsStoreCategoryNameAvailable", "WebStore", AdditionalFields = "WebStoreId,WebStoreCategoryId")]
        [StringLength(256, ErrorMessage = "Name must be maximum 256 characters")]
        public string CategoryName { get; set; }
    }


    public class WebStoreCategoryEditDTO : BaseWebStoreCategoryDTO
    {

        public WebStoreCategoryEditDTO()
        {
            WebStoreCategoryId = -1;
            WebStoreId         = -1;
            CategoryName       = "New Category";
            IsPublic           = true;
        }

        public WebStoreCategoryEditDTO(int storeId)
        {
            WebStoreCategoryId = -1;
            WebStoreId         = storeId;
            CategoryName       = "New Category";
            IsPublic           = true;
        }

       
        [Required]
        public int WebStoreId { get; set; }

        public int? CategoryId { get; set; }

   
        [DisplayName("Description")]
        public string Description { get; set; }

        [Required]
        public int? OrderIndex { get; set; }

        [DisplayName("Open for public viewing")]
        public bool IsPublic { get; set; }

    }

    //course
    public class WebStoreAffiliateItemDTO : BaseWebStoreItemDTO
    {
        public string ItemUrl { get; set; }
        public string ItemTypeName { get; set; }
        public BaseUserInfoDTO Author { get; set; }

        public BaseUserInfoDTO StoreOwner { get; set; }

        public BaseWebStoreDTO WebStore { get; set; }

        public BaseWebStoreCategoryDTO WebStoreCategory { get; set; }

    }

    public class BaseWebStoreItemDTO : PriceBaseDTO
    {
        public int WebStoreItemId { get; set; }

        [Required]
        public BillingEnums.ePurchaseItemTypes ItemType { get; set; }

        [Required(ErrorMessage = "Item required")]
        public int? ItemId { get; set; }
        
        [DisplayName("Item Name")]
        public string ItemName { get; set; }
    }

    public class WebStoreItemEditDTO : BaseWebStoreItemDTO
    {
        public WebStoreItemEditDTO()
        {
            WebStoreCategoryId = -1;
            WebStoreItemId = -1;
            IsActive = true;            
        }

        public WebStoreItemEditDTO(int categoryId)
        {
            WebStoreCategoryId = categoryId;
            WebStoreItemId     = -1;
            IsActive           = true;
            OrderIndex         = -1;
        }

        [Required]
        public int WebStoreCategoryId { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }
        
        [Required]
        public int? OrderIndex { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; }      
    }

    public class CategoryCourseListDTO
    {
        public CategoryCourseListDTO()
        {
            Attach = true;
        }

        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int? WebCourseId { get; set; }
        public bool Attach { get; set; }
        public int index { get; set; }        
    }

    public class WebStoreEmbedToken
    {
        public int WebStoreId { get; set; }

        [Required]
        public int? Width { get; set; }
        
        [Required]
        public int? Height { get; set; }

        [Required]
        [DisplayName("URL of HTML page")]
        public string Url { get; set; }

        public string EmbedCode { get; set; }

        public string TrackingID { get; set; }

        [DisplayName("Display iframe border")]
        [DefaultValue(true)]
        public bool IsDisplayIframeBorder { get; set; }
    }

    public class WebStoreItemListDTO : ItemListDTO
    {
        public int WebStoreCategoryId { get; set; }
        public int WebstoreItemId { get; set; }      
        public int Index { get; set; }      
    }

    public class WebStoreCourseListDTO : BaseListDTO
    {
        public CourseEnums.CourseStatus status { get; set; }
    }
    public class AddItems2StoreCategoryToken
    {
        public int categoryId { get; set; }
        public List<BaseStoreItemToken> items { get; set; }
    }

    public class BaseStoreItemToken
    {
        public int id { get; set; }
        public BillingEnums.ePurchaseItemTypes type { get; set; }
    }
}
