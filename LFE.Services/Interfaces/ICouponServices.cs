using System;
using System.Collections.Generic;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IAuthorAdminCouponServices : IDisposable
    {
        //course coupons
        List<CourseCouponDTO> GetCourseCoupons(int courseId);
        CourseCouponDTO GetCourseCoupon(int couponId, int courseId);
        bool SaveCourseCoupon(ref CourseCouponDTO dto, out string error);        

        //bundle coupons
        List<BundleCouponDTO> GetBundleCoupons(int bundleId);
        BundleCouponDTO GetBundleCoupon(int couponId, int bundleId);
        bool SaveBundleCoupon(ref BundleCouponDTO dto, out string error);

        //author coupons
        List<AuthorCouponDTO> GetAuthorCoupons(int authorId);
        AuthorCouponDTO GetAuthorCoupon(int couponId, int userId);
        bool SaveAuthorCoupon(ref AuthorCouponDTO dto, out string error);
        bool SaveAuthorCoupon(ref AuthorCouponDTO dto, int totalInstances, out string error);
        //common
        bool DeleteCoupon(int couponId, out string error);
    }

    public interface ICouponWidgetServices : IDisposable
    {
        CouponValidationToken ValidateCoupon(int priceLineId, int couponOwnerId, int? courseId, int? bundleId, string couponCode);
        CouponValidationToken ValidateCoupon(int priceLineId, int couponOwnerId, int? courseId, int? bundleId, int couponInstanceId);

        CouponBaseDTO GetCouponBaseToken(int couponInstanceId, out string error);
    }
}
