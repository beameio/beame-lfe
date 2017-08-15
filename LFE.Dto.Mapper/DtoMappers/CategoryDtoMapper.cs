using System;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class CategoryDtoMapper
    {
        public static CategoryEditDTO Entity2CategoryEditDto(this CAT_CategoryToken entity)
        {
            return new CategoryEditDTO
            {
                id     = entity.Id
                ,name  = entity.CategoryName
                ,isActive      = entity.IsActive
                ,cnt      = entity.CoursesCnt + entity.BundlesCnt
            };
        }

        public static CategoryViewDTO Entity2CategoryViewDTO(this Categories entity)
        {
            return new CategoryViewDTO
            {
                id     = entity.Id
                ,name  = entity.CategoryName
                ,url   = Constants.ImageBaseUrl + entity.BannerImageUrl
                ,index = entity.Ordinal  
            };
        }

        public static BaseListDTO CategoryEntity2BaseListDto(this CAT_CategoryListToken entity)
        {
            var token = new BaseListDTO
            {
                id    = entity.Id
                ,name = entity.CategoryName
            };

            if (entity.CoursesCnt == 0 && entity.BundlesCnt == 0) return token;

            token.name = entity.CategoryName.NameWithContentCounts2DisplayName(entity.CoursesCnt, entity.BundlesCnt);
            //token.name += String.Format("({0} {1})", entity.CoursesCnt > 0 ? String.Format("{0} courses", entity.CoursesCnt) : string.Empty, entity.BundlesCnt > 0 ? String.Format("{0} bundles", entity.BundlesCnt) : string.Empty);

            return token;
        }

        public static BaseItemListDTO BaseItemEntity2BaseItemListDto(this SALE_BaseItemToken entity)
        {
            return new BaseItemListDTO
            {
                id    = entity.ItemId
                ,name = entity.ItemName
                ,type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemType)
            };
        }

        public static BaseItemListDTO BaseItemEntity2BaseItemListDtoWithTypePrefix(this SALE_BaseItemToken entity)
        {
            var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemType);
            return new BaseItemListDTO
            {
                id    = entity.ItemId
                ,name = entity.ItemName.StoreItemName2DisplayName(type)
                ,type = type
            };
        }
    }
}
