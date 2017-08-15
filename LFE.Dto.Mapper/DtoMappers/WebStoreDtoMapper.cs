using System;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public  static class WebStoreDtoMapper
    {
        public static BaseEntityDTO Entity2BaseEntityDto(this WebStores entity)
        {
            return new BaseEntityDTO
            {
                id    = entity.StoreID
                ,name = entity.StoreName
                ,Uid  = entity.uid
            };
        }

        public static BaseWebStoreDTO WebStoreGridToken2BaseWebStoreDto(this WebStoreGridDTO token)
        {
            return new BaseWebStoreDTO
            {
                StoreId             = token.StoreId,
                TrackingID          = token.TrackingID,
                Name                = token.Name,
                DefaultCurrencyId   = token.DefaultCurrencyId
            };
        }

        public static WebStoreGridDTO Entity2StoreListDto(this WS_StoreListToken entity)
        {
            return new WebStoreGridDTO
            {
                StoreId           = entity.StoreID
                ,Uid              = entity.uid
                ,TrackingID       = entity.TrackingID
                ,Name             = entity.StoreName
                ,AddOn            = entity.AddOn
                ,CoursesCount     = entity.C_Courses ?? 0
                ,SubscribersCount = entity.C_Subscribers //?? 0   
                ,WixSiteUrl       = entity.WixSiteUrl               
                ,Status           = Utils.ParseEnum<WebStoreEnums.StoreStatus>(entity.StatusId)
                ,DefaultCurrencyId = entity.DefaultCurrencyId ?? Constants.DEFAULT_CURRENCY_ID
            };
        }

        public static WebStoreEditDTO Entity2StoreEditDto(this WebStores entity)
        {
            return new WebStoreEditDTO
            {
                 StoreId     = entity.StoreID
                ,TrackingID  = entity.TrackingID
                ,Uid         = entity.uid
                ,StoreName   = entity.StoreName
                ,CurrencyId  = entity.DefaultCurrencyId ?? Constants.DEFAULT_CURRENCY_ID
                ,Description = entity.Description
                ,MetaTags    = entity.MetaTags
                ,Status      = Utils.ParseEnum<WebStoreEnums.StoreStatus>(entity.StatusId.ToString())
                ,WixUid      = entity.WixInstanceId
            };
        }

        public static WixStoreDTO Entity2WixStoreDto(this WS_WixStoreToken entity)
        {
            return new WixStoreDTO
            {
                StoreId         = entity.StoreID
                ,TrackingID     = entity.TrackingID
                ,Name           = entity.StoreName
                ,Status         = Utils.ParseEnum<WebStoreEnums.StoreStatus>(entity.StatusId)
                ,AddOn          = entity.AddOn
                ,WixInstanceID  = entity.WixInstanceId
                ,WixSiteUrl     = entity.WixSiteUrl
                ,Owner          = new BaseUserInfoDTO
                                    {
                                        UserId    = entity.OwnerUserID
                                        ,FullName = entity.Entity2StoreOwnerFullName()
                                        ,Email    = entity.Email
                                    }
            };
        }

        public static void UpdateStoreWixSiteUrl(this WebStores entity, WixStoreUrlToken token)
        {
            entity.WixSiteUrl = token.WixSiteUrl;
            entity.SiteUrl    = token.WixSiteUrl;
            entity.UpdateOn   = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }

        //categories
        public static WebStoreCategoryEditDTO CategoryEntity2WebStoreCategoryEditDto(this Categories entity,int storeId,int index)
        {
            return new WebStoreCategoryEditDTO
            {
                WebStoreId    = storeId
                ,CategoryId   = entity.Id
                ,CategoryName = entity.CategoryName
                ,Description  = entity.CategoryDescription
                ,IsPublic     = true
                ,OrderIndex   = index
            };
        }

        public static BaseListDTO CategoryEntity2BaseListDto(this WebStoreCategories entity)
        {
            return new BaseListDTO
            {
                 id    = entity.WebStoreCategoryID
                ,name  = entity.CategoryName
                ,index = entity.Ordinal
            };
        }

        public static WebStoreCategoryEditDTO Entity2CategoryEditDto(this WebStoreCategories entity)
        {
            return new WebStoreCategoryEditDTO
            {
                 WebStoreCategoryId    = entity.WebStoreCategoryID
                ,WebStoreId            = entity.WebStoreID
                ,CategoryName          = entity.CategoryName
                ,Description           = entity.Description
                ,IsPublic              = entity.IsPublic
                ,OrderIndex            = entity.Ordinal
            };
        }

        //course
        public static WebStoreItemEditDTO CourseEntity2WebStoreCourseEditDto(this Courses entity,int webCategoryId, int? index = null)
        {
            return new WebStoreItemEditDTO
            {
                 WebStoreCategoryId = webCategoryId
                ,ItemId             = entity.Id
                ,ItemName           = entity.CourseName
                ,Description        = entity.Description
                ,IsActive           = true
                ,OrderIndex         = index
                ,ItemType = BillingEnums.ePurchaseItemTypes.COURSE
            };
        }

        public static BaseItemListDTO CourseEntity2BaseItemListDto(this Courses entity)
        {
            return new BaseItemListDTO
            {
                 id = entity.Id
                ,name = entity.CourseName
                ,desc = entity.Description 
                ,type = BillingEnums.ePurchaseItemTypes.COURSE
            };
        }

        public static BaseItemListDTO BundleEntity2BaseItemListDto(this CRS_Bundles entity)
        {
            return new BaseItemListDTO
            {
                 id = entity.BundleId
                ,name = entity.BundleName
                ,desc = entity.BundleDescription
                ,type = BillingEnums.ePurchaseItemTypes.BUNDLE
            };
        }

        public static BaseListDTO BundleEntity2BaseListDto(this CRS_Bundles entity)
        {
            return new BaseListDTO
            {
                 id = entity.BundleId
                ,name = entity.BundleName               
            };
        }

        public static WebStoreCourseListDTO CourseEntity2BaseListDto(this WebStoreItems entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemTypeId);

            return new WebStoreCourseListDTO
            {
                 id    = entity.WebstoreItemId
                ,name  = entity.ItemName.StoreItemName2DisplayName(type)
                ,index = entity.Ordinal
                ,status = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.CourseId != null ? entity.Courses.StatusId : entity.CRS_Bundles.StatusId)
            };
        }

        public static WebStoreItemListDTO ItemEntity2WebStoreItemListDto(this vw_WS_Items entity,decimal? price,decimal? monthlySubscriptionPrice)
        {
            var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemTypeId);

            return new WebStoreItemListDTO
            {
                 WebstoreItemId             = entity.WebstoreItemId
                ,WebStoreCategoryId         = entity.WebStoreCategoryID
                ,ItemId                     = entity.ItemId
                ,ItemName                   = entity.ItemName.StoreItemName2DisplayName(type)
                ,ItemType                   = type
                ,Index                      = entity.Ordinal
                ,AuthorName                 = entity.AuthorName
                ,AuthorId                   = entity.AuthorID
                ,Price                      = price
                ,MonthlySubscriptionPrice   = monthlySubscriptionPrice
                ,AffiliateCommission        = entity.AffiliateCommission
                ,Description                = entity.ItemDescription
                ,Status                     = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.ItemStatusId)
                ,ItemPageUrl                = type == BillingEnums.ePurchaseItemTypes.COURSE ? entity.GenerateCourseFullPageUrl(entity.AuthorName, entity.ItemName,entity.TrackingID) : entity.GenerateBundleFullPageUrl(entity.AuthorName, entity.ItemName,entity.TrackingID)
            };
        }      
        public static WebStoreItemListDTO ItemEntity2WebStoreItemListDto(this WS_ItemsToken entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemTypeId.ToString());

            return new WebStoreItemListDTO
            {
                 ItemId              = entity.ItemId
                ,ItemName            = entity.ItemName.StoreItemName2DisplayName(type)
                ,ItemType            = type
                ,Description         = entity.ItemDescription
                ,AuthorName          = entity.AuthorName
                ,AuthorId            = entity.AuthorID
                ,Price               = entity.Price
                ,MonthlySubscriptionPrice   = entity.MonthlySubscriptionPrice
                ,AffiliateCommission = entity.AffiliateCommission
                ,Status              = Utils.ParseEnum<CourseEnums.CourseStatus>(entity.ItemStatusId)
                ,ItemPageUrl         = type == BillingEnums.ePurchaseItemTypes.COURSE ? entity.GenerateCourseFullPageUrl(entity.AuthorName, entity.ItemName,null) : entity.GenerateBundleFullPageUrl(entity.AuthorName, entity.ItemName,null)
            };
        }      
        public static WebStoreItemEditDTO Entity2CourseEditDto(this WebStoreItems entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemTypeId);
            return new WebStoreItemEditDTO
            {
                 WebStoreCategoryId    = entity.WebStoreCategoryID
                ,WebStoreItemId        = entity.WebstoreItemId
                ,ItemId                = type == BillingEnums.ePurchaseItemTypes.COURSE ? entity.CourseId : entity.BundleId
                ,ItemName              = entity.ItemName
                ,Description           = entity.Description
                ,IsActive              = entity.IsActive
                ,OrderIndex            = entity.Ordinal
                ,ItemType              = type
            };
        }

        public static BaseWebstoreItemListDTO Entity2CourseListDto(this WS_ItemCandidateToken entity, int index)
        {
            var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemType);

            return new BaseWebstoreItemListDTO
            {
                id           = entity.ItemId
                ,name        = entity.ItemName.StoreItemName2DisplayName(type)
                ,type        = type
                ,storeItemId = entity.WebstoreItemId
                ,index       = index
                ,attach      = entity.WebstoreItemId==null
                ,status =  Utils.ParseEnum<CourseEnums.CourseStatus>(entity.StatusId)
            };
        }
            
        public static WebStoreAffiliateItemDTO Entity2AffiliateItemDto(this WS_AffiliateItemToken entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.LineType);

            var token = new WebStoreAffiliateItemDTO
            {
                 ItemId                     = entity.ItemId
                ,WebStoreItemId             = entity.WebstoreItemId
                ,ItemName                   = entity.ItemName
                ,ItemType                   = type
                ,ItemTypeName               = Utils.GetEnumDescription(type)
                ,Price                      = entity.Price
                ,MonthlySubscriptionPrice   = entity.MonthlySubscriptionPrice
                ,StoreOwner                 = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.OwnerUserID
                                                            ,FullName = entity.Entity2StoreOwnerFullName()                                                            
                                                        }
                ,Author                     = new BaseUserInfoDTO
                                                        {
                                                            UserId    = entity.AuthorId
                                                            ,FullName = entity.Entity2AuthorFullName()
                                                        }
                ,WebStore                   = new BaseWebStoreDTO
                                                        {
                                                            StoreId     = entity.StoreID
                                                            ,TrackingID = entity.TrackingID
                                                            ,Name       = entity.StoreName
                                                        } 
                ,WebStoreCategory           =  new BaseWebStoreCategoryDTO
                                                        {
                                                            WebStoreCategoryId = entity.WebStoreCategoryID
                                                            ,CategoryName      = entity.CategoryName
                                                        }
            };

            token.ItemUrl = Utils.GetKeyValue("baseUrl") + entity.GenerateStoreItemPageUrl(token.Author.FullName, token.ItemName.OptimizedUrl(), token.WebStoreCategory.CategoryName.OptimizedUrl(), token.WebStore.TrackingID).Remove(0,1);

            return token;
        }

        public static BaseListDTO BaseStoreToken2BaseListDto(this WS_BaseStoreToken token)
        {
            return new BaseListDTO
            {
                id    = token.StoreID
                ,name = token.StoreName
            };
        }

        //wix
         public static WebStoreEditDTO WixSettingsToken2EditDto(this WixRegisterStoreDTO token)
        {
            return new WebStoreEditDTO
            {
                Description   = "",
                MetaTags      = "",
                OwnerUserId   = token.UserId,
                Status        = WebStoreEnums.StoreStatus.Published,
                StoreId       = token.StoreId,
                StoreName     = token.StoreName,
                TrackingID    = token.InstanceId.ToString(),
                CurrencyId    = Constants.DEFAULT_CURRENCY_ID,
                Uid           = Guid.NewGuid(),
                WixUid        = null,                
                WixInstanceId = token.InstanceId,  
                         
            };   
        }


        public static WebStoreEditDTO WixSettingsToken2EditDto(this WixSettingsToken dto, int ownerId, string trackingId)
        {
            return new WebStoreEditDTO
            {
                OwnerUserId         = ownerId
                ,TrackingID         = trackingId
                ,Uid                = Guid.NewGuid()
                ,Status             = WebStoreEnums.StoreStatus.Published
                ,CurrencyId         = Constants.DEFAULT_CURRENCY_ID
                ,StoreName          = dto.StoreName
                ,Description        = ""
                ,MetaTags           = ""
                ,FontColor          = dto.FontColor
                ,BackgroundColor    = dto.BackgroundColor
                ,TabsFontColor      = dto.TabsFontColor
                ,IsShowBorder       = dto.IsShowBorder
                ,IsTransparent      = dto.IsTransparent
                ,WixInstanceId      = dto.InstanceId != null ? new Guid(dto.InstanceId) : Guid.Empty
                ,StoreId            = dto.StoreId != null ? (int)dto.StoreId : -1
                ,WixSiteUrl         = dto.WixSiteUrl   
                ,RegistrationSource = CommonEnums.eRegistrationSources.WIX
            };
        }

        public static WebStoreEditDTO FbPageId2StoreEditDto(this int ownerId, string pageId)
        {
            return new WebStoreEditDTO
            {
                OwnerUserId         = ownerId
                ,TrackingID         = pageId
                ,Uid                = Guid.NewGuid()
                ,Status             = WebStoreEnums.StoreStatus.Draft
                ,CurrencyId         = Constants.DEFAULT_CURRENCY_ID
                ,StoreName          = String.Format("FB Tab Page App Store N{0}",pageId)
                ,Description        = "Default FB Tab Page App"
                ,MetaTags           = string.Empty
                ,FontColor          = string.Empty
                ,BackgroundColor    = string.Empty
                ,TabsFontColor      = string.Empty
                ,IsShowBorder       = false
                ,IsTransparent      = false
                ,StoreId            = -1
                ,RegistrationSource = CommonEnums.eRegistrationSources.FB
            };
        }

    }
}
