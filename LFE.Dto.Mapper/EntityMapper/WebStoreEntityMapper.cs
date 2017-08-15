using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class WebStoreEntityMapper
    {
        //store

        public static WebStores EditDto2StoreEntity(this WebStoreEditDTO dto,int ownerId)
        {
            return new WebStores
                {
                     OwnerUserID          = ownerId
                    ,TrackingID           = dto.TrackingID
                    ,uid                  = dto.Uid
                    ,StoreName            = dto.StoreName
                    ,StatusId             = (short)dto.Status
                    ,Description          = dto.Description
                    ,MetaTags             = dto.MetaTags
                    ,DefaultCurrencyId    = dto.CurrencyId
                    ,AddOn                = DateTime.Now
                    ,CreatedBy            = DtoExtensions.CurrentUserId
                    ,RegistrationSourceId = (byte)dto.RegistrationSource
                    ,WixInstanceId        = dto.WixInstanceId
                    ,WixSiteUrl           = dto.WixSiteUrl
                    ,FontColor            = dto.FontColor
                    ,BackgroundColor      = dto.BackgroundColor
                    ,TabsFontColor        = dto.TabsFontColor
                    ,IsTransparent        = dto.IsTransparent
                    ,IsShowBorder         = dto.IsShowBorder
                    ,IsShowTitleBar       = dto.IsShowTitleBar
                };
        }
        public static WebStores CloneStoreEntity(this WebStores entity,string storeName,string trackingId )
        {
            return new WebStores
                {
                     OwnerUserID          = entity.OwnerUserID
                    ,TrackingID           = trackingId
                    ,uid                  = Guid.NewGuid()
                    ,StoreName            = storeName
                    ,StatusId             = (short) WebStoreEnums.StoreStatus.Published
                    ,RegistrationSourceId = entity.RegistrationSourceId
                    ,Description          = entity.Description
                    ,MetaTags             = entity.MetaTags
                    ,DefaultCurrencyId    = entity.DefaultCurrencyId
                    ,AddOn                = DateTime.Now
                    ,CreatedBy            = DtoExtensions.CurrentUserId                    
                };
        }
        public static WebStores EditDto2StoreEntity(this BaseWebStoreDTO dto,int ownerId)
        {
            return new WebStores
                {
                     OwnerUserID          = ownerId
                    ,TrackingID           = dto.TrackingID
                    ,uid                  = Guid.NewGuid()
                    ,StoreName            = dto.Name
                    ,StatusId             = (short)WebStoreEnums.StoreStatus.Published
                    ,DefaultCurrencyId    = dto.DefaultCurrencyId
                    ,AddOn                = DateTime.Now
                    ,CreatedBy            = DtoExtensions.CurrentUserId
                    ,RegistrationSourceId = (byte)dto.RegistrationSource
                };
        }

        //public static WebStores EditDto2StoreEntity(this WixSettingsToken dto, int ownerId)
        //{
        //    return new WebStores
        //    {
        //         OwnerUserID       = ownerId
        //        ,TrackingID        = dto.TrackingID
        //        ,uid               = Guid.NewGuid()
        //        ,StatusId          = (int)WebStoreEnums.StoreStatus.Published
        //        ,DefaultCurrencyId = Constants.DEFAULT_CURRENCY_ID
        //        ,StoreName         = dto.StoreName
        //        ,Description       = ""
        //        ,MetaTags          = ""
        //        ,FontColor         = dto.FontColor
        //        ,BackgroundColor   = dto.BackgroundColor
        //        ,TabsFontColor     = dto.TabsFontColor
        //        ,IsShowBorder      = dto.IsShowBorder
        //        ,IsTransparent     = dto.IsTransparent
        //        ,AddOn             = DateTime.Now
        //        ,CreatedBy         = DtoExtensions.CurrentUserId
        //        ,WixInstanceId     = dto.InstanceId != null ? new Guid(dto.InstanceId) : new Guid()
        //    };
        //}


        //public static WebStores WixReigster2WebStores(this WixRegisterStoreDTO token)
        //{
        //    return new WebStores
        //        {
        //             OwnerUserID       = token.UserId
        //            ,TrackingID        = token.InstanceId.ToString()
        //            ,WixInstanceId     = token.InstanceId
        //            ,uid               = Guid.NewGuid()
        //            ,StoreName         = token.StoreName
        //            ,StatusId          = (short)WebStoreEnums.StoreStatus.Draft
        //            ,DefaultCurrencyId = Constants.DEFAULT_CURRENCY_ID
        //            ,Description       = string.Empty
        //            ,MetaTags          = string.Empty
        //            ,AddOn             = DateTime.Now
        //            ,CreatedBy         = DtoExtensions.CurrentUserId
        //        };
        //}

        public static void UpdateStoreEntity(this WebStores entity, WebStoreEditDTO dto)
        {
            entity.StoreName         = dto.StoreName;
            entity.TrackingID        = dto.TrackingID;
            entity.StatusId          = (short)dto.Status;
            entity.Description       = dto.Description;
            entity.DefaultCurrencyId = dto.CurrencyId;
            entity.MetaTags          = dto.MetaTags;
            entity.UpdateOn          = DateTime.Now;
            entity.UpdatedBy         = DtoExtensions.CurrentUserId;
             
        }

        public static void UpdateStoreEntity(this WebStores entity, BaseWebStoreDTO dto)
        {
            entity.StoreName = dto.Name;
            entity.TrackingID = dto.TrackingID;
            entity.DefaultCurrencyId = dto.DefaultCurrencyId ?? Constants.DEFAULT_CURRENCY_ID;
            entity.UpdateOn = DateTime.Now;
            entity.UpdatedBy = DtoExtensions.CurrentUserId;

        }

        //wix settings
        //public static void UpdateStoreEntity(this WebStores entity, WixSettingsToken dto)
        //{
        //    entity.StoreName       = dto.StoreName;
        //    entity.FontColor       = dto.FontColor;
        //    entity.BackgroundColor = dto.BackgroundColor;
        //    entity.TabsFontColor   = dto.TabsFontColor;
        //    entity.IsShowTitleBar  = dto.IsShowTitleBar;
        //    entity.IsShowBorder    = dto.IsShowBorder;
        //    entity.IsTransparent   = dto.IsTransparent;
        //    entity.UpdateOn        = DateTime.Now;
        //    entity.UpdatedBy       = DtoExtensions.CurrentUserId;

        //    if (string.IsNullOrEmpty(entity.WixSiteUrl) && !string.IsNullOrEmpty(dto.WixSiteUrl))
        //    {
        //        entity.WixSiteUrl = dto.WixSiteUrl;
        //    }
        //}

        //category
        public static WebStoreCategories EditDto2CategoryEntity(this WebStoreCategoryEditDTO dto)
        {
            return new WebStoreCategories
                {
                     WebStoreID      = dto.WebStoreId
                    ,CategoryName    = dto.CategoryName
                    ,CategoryUrlName = dto.CategoryName.OptimizedUrl()
                    ,IsPublic        = true //dto.IsPublic
                    ,Description     = dto.Description.TrimString(4000)
                    ,Ordinal         = dto.OrderIndex ?? 0
                    ,IsAutoUpdate    = false
                    ,AddOn           = DateTime.Now
                };
        }

        public static WebStoreCategories CloneCategoryEntity(this WebStoreCategories entity, int storeID)
        {
            return new WebStoreCategories
            {
                AddOn           = DateTime.Now,
                CategoryId      = entity.CategoryId,
                CategoryName    = entity.CategoryName,
                CategoryUrlName = entity.CategoryUrlName,
                Description     = entity.Description,
                IsAutoUpdate    = entity.IsAutoUpdate,
                IsPublic        = true,
                Ordinal         = entity.Ordinal,
                WebStoreID      = storeID
            };
        }

        public static void UpdateCategoryEntity(this WebStoreCategories entity, WebStoreCategoryEditDTO dto)
        {
            entity.CategoryName    = dto.CategoryName;
            entity.CategoryUrlName = dto.CategoryName.OptimizedUrl();
            entity.IsPublic        = true; //dto.IsPublic;
            entity.Description     = dto.Description.TrimString(4000);
        }

        public static void UpdateCategoryEntityOrderIndex(this WebStoreCategories entity, int index)
        {
            entity.Ordinal = index;
        }

        //course
        public static WebStoreItems WebStoreItemEditDto2WebStoreItemEntity(this WebStoreItemEditDTO dto)
        {
            return new WebStoreItems
                {
                     WebStoreCategoryID   = dto.WebStoreCategoryId
                    ,CourseId             = dto.ItemType == BillingEnums.ePurchaseItemTypes.COURSE ? dto.ItemId  : null
                    ,BundleId             = dto.ItemType == BillingEnums.ePurchaseItemTypes.BUNDLE ? dto.ItemId  : null
                    ,ItemName             = dto.ItemName
                    ,ItemTypeId           = (byte)dto.ItemType
                    ,IsActive             = dto.IsActive
                    ,Description          = dto.Description
                    ,Ordinal              = dto.OrderIndex ?? 0
                    ,AddOn                = DateTime.Now
                };
        }

        public static WebStoreItems CloneWsCategoryItemEntity(this WebStoreItems entity,int categoryId)
        {
            return new WebStoreItems
                            {
                                AddOn       = DateTime.Now,
                                BundleId    = entity.BundleId,
                                CourseId    = entity.CourseId,
                                CreatedBy   = DtoExtensions.CurrentUserId,
                                Description = entity.Description,
                                IsActive    = entity.IsActive,
                                ItemName    = entity.ItemName,
                                ItemTypeId  = entity.ItemTypeId,
                                Ordinal     = entity.Ordinal,
                                WebStoreCategoryID = categoryId
                            };
        }


        public static void UpdateCourseEntity(this WebStoreItems entity, WebStoreItemEditDTO dto)
        {
            entity.ItemName   = dto.ItemName;
            entity.IsActive     = dto.IsActive;
            entity.Description  = dto.Description;
        }

        public static void UpdateCourseEntityOrderIndex(this WebStoreItems entity, int index)
        {
            entity.Ordinal = index;
        }
    }
}
