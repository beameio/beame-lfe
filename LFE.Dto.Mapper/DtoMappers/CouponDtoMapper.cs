using System;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class CouponDtoMapper
    {
        public static CourseCouponDTO Entity2CourseCouponDTO(this Coupons entity)
        {
            return new CourseCouponDTO
                {
                     CouponId           = entity.Id
                    ,CourseId           = entity.CourseId
                    ,CouponName         = entity.CouponName
                    ,Amount             = (decimal) (entity.CouponTypeAmount ?? 0)
                    ,AutoGeneration     = entity.AutoGenerate
                    ,SubscriptionMonths = entity.SubscriptionMonths
                    ,Type               = Utils.ParseEnum<CourseEnums.CouponType>(entity.CouponTypeId.ToString())
                    ,ExpirationDate     = entity.ExpirationDate
                };
        }

        public static BundleCouponDTO Entity2BundleCouponDTO(this Coupons entity)
        {
            return new BundleCouponDTO
                {
                     CouponId           = entity.Id
                    ,BundleId           = entity.BundleId
                    ,CouponName         = entity.CouponName
                    ,Amount             = (decimal) (entity.CouponTypeAmount ?? 0)
                    ,AutoGeneration     = entity.AutoGenerate
                    ,SubscriptionMonths = entity.SubscriptionMonths
                    ,Type               = Utils.ParseEnum<CourseEnums.CouponType>(entity.CouponTypeId.ToString())
                    ,ExpirationDate     = entity.ExpirationDate
                };
        }

        public static CouponBaseDTO Entity2CouponBaseDTO(this Coupons entity)
        {
            return new CouponBaseDTO
                {
                     CouponId           = entity.Id
                    ,CouponName         = entity.CouponName
                    ,Amount             = (decimal) (entity.CouponTypeAmount ?? 0)                    
                    ,Type               = Utils.ParseEnum<CourseEnums.CouponType>(entity.CouponTypeId.ToString())                    
                };
        }

        public static AuthorCouponDTO Entity2AuthorCouponDTO(this Coupons entity, int userId)
        {
            CourseEnums.eCouponKinds kind;
            if (entity.CourseId != null)
            {
                kind = CourseEnums.eCouponKinds.Course;                
            }
            else if (entity.BundleId != null)
            {
                kind = CourseEnums.eCouponKinds.Bundle;
            }
            else
            {
                kind = CourseEnums.eCouponKinds.Author;
            }

            return new AuthorCouponDTO(userId)
                {
                     CouponId           = entity.Id
                    ,Kind               = kind
                    ,KindDisplayName    = kind.ToString()
                    ,BundleId           = entity.BundleId
                    ,CourseId           = entity.CourseId
                    ,CouponName         = entity.CouponName
                    ,Amount             = Math.Round((decimal) (entity.CouponTypeAmount ?? 0),2)
                    ,AutoGeneration     = entity.AutoGenerate
                    ,Type               = Utils.ParseEnum<CourseEnums.CouponType>(entity.CouponTypeId.ToString())
                    ,ExpirationDate     = entity.ExpirationDate
                    ,SubscriptionMonths = entity.SubscriptionMonths
                    ,IsActive           = entity.ExpirationDate == null || DateTime.Now.Date <= entity.ExpirationDate                   
                };
        }

        public static AuthorCouponDTO Entity2AuthorCouponDTO(this Coupons entity)
        {
            CourseEnums.eCouponKinds kind;
            if (entity.CourseId != null)
            {
                kind = CourseEnums.eCouponKinds.Course;                
            }
            else if (entity.BundleId != null)
            {
                kind = CourseEnums.eCouponKinds.Bundle;
            }
            else
            {
                kind = CourseEnums.eCouponKinds.Author;
            }

            return new AuthorCouponDTO
                {
                     CouponId           = entity.Id
                    ,Kind               = kind
                    ,KindDisplayName    = kind.ToString()
                    ,OwnerUserId        = entity.OwnerUserId
                    ,BundleId           = entity.BundleId
                    ,CourseId           = entity.CourseId
                    ,CouponName         = entity.CouponName
                    ,Amount             = Math.Round((decimal) (entity.CouponTypeAmount ?? 0),2)
                    ,AutoGeneration     = entity.AutoGenerate
                    ,Type               = Utils.ParseEnum<CourseEnums.CouponType>(entity.CouponTypeId.ToString())
                    ,ExpirationDate     = entity.ExpirationDate
                    ,SubscriptionMonths = entity.SubscriptionMonths
                    ,IsActive           = entity.ExpirationDate == null || DateTime.Now.Date <= entity.ExpirationDate                   
                };
        }

        public static CourseCouponDTO Entity2CouponDTO(this CRS_CouponToken entity)
        {
            return new CourseCouponDTO
                {
                     CouponId           = entity.Id
                    ,CourseId           = entity.CourseId
                    ,CouponName         = entity.CouponName
                    ,Amount             = Math.Round((decimal) (entity.CouponTypeAmount ?? 0),2)
                    ,AutoGeneration     = entity.AutoGenerate
                    ,Type               = Utils.ParseEnum<CourseEnums.CouponType>(entity.CouponTypeId.ToString())
                    ,ExpirationDate     = entity.ExpirationDate
                    ,SubscriptionMonths = entity.SubscriptionMonths
                    ,UsageLimit         = entity.UsageLimit == null || entity.UsageLimit <= 0 ? null : entity.UsageLimit
                    ,ActualUsage        = entity.ActualUsage ?? 0
                    ,CouponCode         = entity.Code
                    ,IsActive           = entity.ExpirationDate == null || DateTime.Now.Date <= entity.ExpirationDate
                    ,IsDeleteAllowed    = entity.IsDeleteAllowed ?? true
                };
        }
     
        public static BundleCouponDTO Entity2BundleCouponDTO(this CRS_CouponToken entity)
        {
            return new BundleCouponDTO
                {
                     CouponId           = entity.Id
                    ,BundleId           = entity.BundleId
                    ,CouponName         = entity.CouponName
                    ,Amount             = Math.Round((decimal) (entity.CouponTypeAmount ?? 0),2)
                    ,AutoGeneration     = entity.AutoGenerate
                    ,Type               = Utils.ParseEnum<CourseEnums.CouponType>(entity.CouponTypeId.ToString())
                    ,ExpirationDate     = entity.ExpirationDate
                    ,SubscriptionMonths = entity.SubscriptionMonths
                    ,UsageLimit         = entity.UsageLimit == null || entity.UsageLimit <= 0 ? null : entity.UsageLimit
                    ,ActualUsage        = entity.ActualUsage ?? 0
                    ,CouponCode         = entity.Code
                    ,IsActive           = entity.ExpirationDate == null || DateTime.Now.Date <= entity.ExpirationDate
                    ,IsDeleteAllowed    = entity.IsDeleteAllowed ?? true
                };
        }

        public static AuthorCouponDTO Entity2AuthorCouponDTO(this CRS_CouponToken entity,int userId)
        {
            CourseEnums.eCouponKinds kind;
            string itemName;
            if (entity.CourseId != null)
            {
                kind = CourseEnums.eCouponKinds.Course;
                itemName = entity.CourseName;
            }
            else if (entity.BundleId != null)
            {
                kind = CourseEnums.eCouponKinds.Bundle;
                itemName = entity.BundleName;
            }
            else
            {
                kind = CourseEnums.eCouponKinds.Author;
                itemName = "Author coupon";
            }

            return new AuthorCouponDTO(userId)
                {
                     CouponId           = entity.Id
                    ,Kind               = kind
                    ,KindDisplayName    = kind.ToString()
                    ,ItemName           = itemName
                    ,BundleId           = entity.BundleId
                    ,CourseId           = entity.CourseId
                    ,CouponName         = entity.CouponName
                    ,Amount             = Math.Round((decimal) (entity.CouponTypeAmount ?? 0),2)
                    ,AutoGeneration     = entity.AutoGenerate
                    ,Type               = Utils.ParseEnum<CourseEnums.CouponType>(entity.CouponTypeId.ToString())
                    ,ExpirationDate     = entity.ExpirationDate
                    ,SubscriptionMonths = entity.SubscriptionMonths
                    ,UsageLimit         = entity.UsageLimit == null || entity.UsageLimit <= 0 ? null : entity.UsageLimit
                    ,ActualUsage        = entity.ActualUsage ?? 0
                    ,CouponCode         = entity.Code
                    ,IsActive           = entity.ExpirationDate == null || DateTime.Now.Date <= entity.ExpirationDate
                    ,IsDeleteAllowed    = entity.IsDeleteAllowed ?? true
                };
        }

        
    }
}
