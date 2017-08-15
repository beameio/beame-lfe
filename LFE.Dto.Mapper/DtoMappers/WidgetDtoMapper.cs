using System.Linq;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;
using System.Collections.Generic;


namespace LFE.Dto.Mapper.DtoMappers
{
    public static class WidgetDtoMapper
    {
        public static WidgetWebStoreDTO Entity2WidgetStoreDto(this WebStores entity)
        {
            return new WidgetWebStoreDTO
            {
                 WebStoreID         = entity.StoreID
                ,WebStoreName       = entity.StoreName
                ,GoogleAnalyticCode = entity.GoogleAnalyticCode
                ,MetaTags           = entity.MetaTags
                ,Description        = entity.Description
                ,OwnerUserID        = entity.OwnerUserID
                ,TrackingID         = entity.TrackingID
                ,CurrencyId         = entity.DefaultCurrencyId 
                ,Status             = Utils.ParseEnum<WebStoreEnums.StoreStatus>(entity.StatusId)
                ,WixInstanceID      = entity.WixInstanceId != null ?  entity.WixInstanceId.ToString() : string.Empty
                ,FontColor          = entity.FontColor
                ,BackgroundColor    = entity.BackgroundColor
                ,TabsFontColor      = entity.TabsFontColor
                ,IsTransParent      = entity.IsTransparent != null && Convert.ToBoolean(entity.IsTransparent)
                ,IsShowBorder       = entity.IsShowBorder != null && Convert.ToBoolean(entity.IsShowBorder)
                ,IsShowTitleBar     = entity.IsShowTitleBar != null && Convert.ToBoolean(entity.IsShowTitleBar)
                ,LastUpdate         = Convert.ToDateTime(entity.UpdateOn ?? entity.AddOn)
                ,UniqueId           = entity.uid.ToString()
                ,WixSiteUrl         = entity.WixSiteUrl
            }; 
        }

        public static WidgetItemListDTO Entity2CourseListDto(this WIDGET_CourseListToken entity,bool isItemOwner,bool isUnderRGP,List<PriceLineDTO> itemPrices,short currencyId)
        {
            return new WidgetItemListDTO
            {
                 ItemID                   = entity.ItemId
                ,ItemName                 = entity.ItemName
                ,ItemType                 = (BillingEnums.ePurchaseItemTypes)entity.ItemTypeId
                ,AuthorName               = entity.AuthorName
                ,AuthorID                 = entity.AuthorID
                ,ImageURL                 = entity.ImageURL
                //,Price                    = price.FormatNullablePrice()
                //,MonthlySubscriptionPrice = monthlySubscriptionPrice.FormatNullablePrice()
                ,NumSubscribers           = entity.NumSubscribers
                ,Rating                   = entity.Rating
                ,IsFree                   = entity.IsFreeCourse ?? false
                ,IsAuthorUnderRGP         = isUnderRGP
                ,ItemUrlName              = entity.UrlName
                ,TotalCourses             = entity.SumCourses
                ,IsItemOwner              = isItemOwner
                ,CoursesCnt               = entity.CoursesCnt
                ,ItemPrices               = itemPrices
                ,IsItemUnderRGP           = isUnderRGP && itemPrices.Any(x=>x.PriceType == BillingEnums.ePricingTypes.ONE_TIME)// && x.Currency.CurrencyId == currencyId)              
            };
        }

        public static WidgetItemListDTO Entity2CourseListDto(this vw_WS_Items entity,bool isItemOwner,List<PriceLineDTO> itemPrices)
        {
            return new WidgetItemListDTO
            {
                 ItemID                   = entity.ItemId
                ,ItemName                 = entity.ItemName
                ,ItemType                 = (BillingEnums.ePurchaseItemTypes)entity.ItemTypeId
                ,AuthorName               = entity.AuthorName
                ,AuthorID                 = entity.AuthorID
                ,ImageURL                 = entity.ImageURL
                ,NumSubscribers           = entity.NumSubscribers
                ,Rating                   = entity.Rating
                ,IsFree                   = entity.IsFreeCourse ?? false
                ,ItemUrlName              = entity.UrlName
                ,IsItemOwner              = isItemOwner
                ,CoursesCnt               = entity.CoursesCnt
                ,ItemPrices               = itemPrices
            };
        }

        public static WidgetItemListDTO Entity2CourseWidgetListDto(this Courses entity, decimal? price, decimal? monthlySubscriptionPrice)
        {
            
            return new WidgetItemListDTO
            {
                ItemName                  = entity.CourseName
                ,AuthorID                 = Convert.ToInt32(entity.AuthorUserId)
                ,ImageURL                 = entity.SmallImage
                ,Price                    = price.FormatNullablePrice()
                ,MonthlySubscriptionPrice = monthlySubscriptionPrice.FormatNullablePrice()
               // ,NumSubscribers           = Convert.ToInt32(entity.Popularity)
                ,Rating                   = Convert.ToInt32(entity.Rating)
                ,IsFree                   = Convert.ToBoolean(entity.IsFreeCourse)
                ,ItemUrlName              = entity.CourseUrlName
            };
        }

       

        public static WidgetCategoryDTO Entity2WidgetCategoryDTO(this WebStoreCategories entity)
        {
            return entity != null ? new WidgetCategoryDTO
            {
                WebStoreCategoryID = entity.WebStoreCategoryID
                ,LfeCategoryID     = entity.CategoryId
                ,CategoryName      = entity.CategoryName
                ,CategoryUrlName   = entity.CategoryUrlName
                ,Description       = entity.Description
                ,Ordinal           = entity.Ordinal
            } : null;
        }


        #region Course Purchase
        public static AuthorPurchaseDTO UserAndCourses2AuthorPurchaseDTO(this Users entity, List<WidgetItemListDTO> itemsList)
        {           
            return new AuthorPurchaseDTO
            {
                FirstName         = entity.FirstName
                ,LastName         = entity.LastName
                ,BioHtml          = entity.BioHtml
                ,AuthorPictureURL = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                ,PictureURL       = entity.PictureURL
                ,ItemsList        = itemsList
                ,FullName         = entity.Entity2FullName()
            };
        }

        public static CoursePurchaseDTO CourseEntity2CoursePurchaseDto(this Courses entity,decimal? price,decimal? monthlySubscriptionPrice)
        {       
            return new CoursePurchaseDTO
            {
                CourseId                  = entity.Id
                ,CourseName               = entity.CourseName
                ,MetaTags                 = entity.MetaTags
                ,ThumbUrl                 = String.IsNullOrEmpty(entity.SmallImage) ? string.Empty : Constants.ImageBaseUrl + entity.SmallImage
                ,Rating                   = entity.Rating ?? 0
                ,ClassRoomId              = entity.ClassRoomId
               // ,NumSubscribers           = entity.Popularity
                ,Price                    = price.FormatNullablePrice()
                ,MonthlySubscriptionPrice = monthlySubscriptionPrice.FormatNullablePrice()
                ,IsFreeCourse             = Convert.ToBoolean(entity.IsFreeCourse)                
                ,IntroHtml                = entity.Description
                ,CourseUrlName            = entity.CourseUrlName
                ,OverviewVideoIdentifier  = entity.OverviewVideoIdentifier
            };
        }


        public static BundlePurchaseDTO BundleEntity2BundlePurchaseDto(this CRS_Bundles entity, decimal? price, decimal? monthlySubscriptionPrice)
        {
            return new BundlePurchaseDTO
            {
                 BundleId                 = entity.BundleId
                ,BundleName               = entity.BundleName
                ,MetaTags                 = entity.MetaTags
                ,ThumbUrl                 = String.IsNullOrEmpty(entity.BannerImage) ? string.Empty : Constants.ImageBaseUrl + entity.BannerImage
                ,NumSubscribers           = entity.USER_Bundles.Count
                ,Price                    = price.FormatNullablePrice()
                ,MonthlySubscriptionPrice = monthlySubscriptionPrice.FormatNullablePrice() 
                ,IsFreeCourse             = false
                ,IntroHtml                = entity.BundleDescription
                ,OverviewVideoIdentifier  = entity.OverviewVideoIdentifier
                ,BundleUrlName            = entity.BundleUrlName
            };
        }
        #endregion

        #region item
        public static WidgetItemListDTO Entity2WidgetItemListDto(this USER_ItemToken entity, bool isItemOwner, List<PriceLineDTO> itemPrices)
        {
            return new WidgetItemListDTO
            {
                 ItemID                   = entity.ItemId
                ,ItemName                 = entity.ItemName
                ,ItemType                 = (BillingEnums.ePurchaseItemTypes)entity.ItemTypeId
                ,AuthorName               = entity.Entity2AuthorFullName()
                ,AuthorID                 = entity.AuthorId
                ,ImageURL                 = entity.ImageURL.ToThumbUrl(Constants.ImageBaseUrl)         
                ,NumSubscribers           = entity.NumSubscribers
                ,Rating                   = entity.Rating
                ,IsFree                   = entity.IsFreeCourse ?? false
                ,ItemUrlName              = entity.UrlName                
                ,IsItemOwner              = isItemOwner
                ,CoursesCnt               = entity.CoursesCnt
                ,ItemPrices               = itemPrices
            };
        }

        public static WidgetItemListDTO Entity2WidgetItemListDto(this LRNR_ItemToken entity, bool isItemOwner, List<PriceLineDTO> itemPrices)
        {
            return new WidgetItemListDTO
            {
                 ItemID                   = entity.Id
                ,ItemName                 = entity.CourseName
                ,ItemType                 = BillingEnums.ePurchaseItemTypes.COURSE
                ,AuthorName               = entity.Entity2AuthorFullName()
                ,AuthorID                 = entity.AuthorUserId
                ,ImageURL                 = entity.SmallImage.ToThumbUrl(Constants.ImageBaseUrl)                
                ,NumSubscribers           = entity.NumSubscribers
                ,Rating                   = entity.Rating
                ,IsFree                   = entity.IsFreeCourse
                ,ItemUrlName              = entity.CourseUrlName                
                ,IsItemOwner              = isItemOwner
                ,CoursesCnt               = 1
                ,ItemPrices               = itemPrices
            };
        }
        #endregion
    }
}
