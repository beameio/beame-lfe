using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class CouponEntityMapper
    {
        public static Coupons CourseCouponDto2CouponEntity(this CourseCouponDTO dto)
        {
            return new Coupons
                {
                    CourseId            = dto.CourseId
                    ,CouponName         = dto.CouponName
                    ,CouponTypeId       = (byte)dto.Type
                    ,CouponTypeAmount   = (double?)(dto.Amount ?? 0)
                    ,ExpirationDate     = dto.ExpirationDate
                    ,AutoGenerate       = dto.AutoGeneration
                    ,SubscriptionMonths = dto.SubscriptionMonths
                    ,AddOn              = DateTime.Now
                    ,CreatedBy          = DtoExtensions.CurrentUserId
                };
        }

        public static Coupons BundleCouponDto2CouponEntity(this BundleCouponDTO dto)
        {
            return new Coupons
                {
                    BundleId            = dto.BundleId
                    ,CouponName         = dto.CouponName
                    ,CouponTypeId       = (byte)dto.Type
                    ,CouponTypeAmount   = (double?) (dto.Amount ?? 0)
                    ,ExpirationDate     = dto.ExpirationDate
                    ,AutoGenerate       = dto.AutoGeneration
                    ,SubscriptionMonths = dto.SubscriptionMonths
                    ,AddOn              = DateTime.Now
                    ,CreatedBy          = DtoExtensions.CurrentUserId
                };
        }

        public static Coupons AuthorCouponDto2CouponEntity(this AuthorCouponDTO dto)
        {
            return new Coupons
                {
                    BundleId            = dto.BundleId
                    ,CourseId           =  dto.CourseId
                    ,OwnerUserId        = dto.OwnerUserId
                    ,CouponName         = dto.CouponName
                    ,CouponTypeId       = (byte)dto.Type
                    ,CouponTypeAmount   = (double?) (dto.Amount ?? 0)
                    ,ExpirationDate     = dto.ExpirationDate
                    ,AutoGenerate       = dto.AutoGeneration
                    ,SubscriptionMonths = dto.SubscriptionMonths
                    ,AddOn              = DateTime.Now
                    ,CreatedBy          = DtoExtensions.CurrentUserId
                };
        }

        public static void UpdateCouponEntity(this Coupons entity, CouponDTO dto)
        {
            entity.CouponName         = dto.CouponName;
            entity.CouponTypeId       = (byte)dto.Type;
            entity.CouponTypeAmount   = (double?)( dto.Amount ?? 0 );
            entity.ExpirationDate     = dto.ExpirationDate;
            entity.SubscriptionMonths = dto.SubscriptionMonths;
            entity.UpdateDate         = DateTime.Now;
            entity.UpdatedBy          = DtoExtensions.CurrentUserId;
        }

        public static CouponInstances CourseCouponDto2CouponInstanceEntity(this CouponDTO dto)
        {
            return new CouponInstances
                {
                     CouponId          = dto.CouponId
                    ,CouponCode        = dto.CouponName.OptimizedUrl()
                    ,UsageLimit        = dto.UsageLimit ?? -1
                    ,AddOn             = DateTime.Now
                    ,CreatedBy         = DtoExtensions.CurrentUserId
                };
        }

        public static CouponInstances CourseCouponDto2CouponInstanceEntity(this CouponDTO dto,string code)
        {
            return new CouponInstances
                {
                     CouponId          = dto.CouponId
                    ,CouponCode        = code
                    ,UsageLimit        = dto.UsageLimit ?? -1
                    ,AddOn             = DateTime.Now
                    ,CreatedBy         = DtoExtensions.CurrentUserId
                };
        }

        public static void UpdateCouponInstanceEntity(this CouponInstances entity, CouponDTO dto)
        {
            entity.CouponCode   = dto.CouponName.OptimizedUrl();
            entity.UsageLimit   = dto.UsageLimit ?? -1;
            entity.UpdateDate   = DateTime.Now;
            entity.UpdatedBy    = DtoExtensions.CurrentUserId;
        }
    }
}
