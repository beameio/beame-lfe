using LFE.Core.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LFE.DataTokens
{

    public class CouponBaseDTO
    {
        [Required]
        public int CouponId { get; set; }

        public int? OwnerUserId { get; set; }

        [Required]
        [DisplayName("Coupon Name")]
        public string CouponName { get; set; }

        [DisplayName("Coupon Code")]
        public string CouponCode { get; set; }

        
        [DisplayName("Discount Amount")]
        public decimal? Amount { get; set; }

        [Required]
        [DisplayName("Type")]
        public CourseEnums.CouponType Type { get; set; }
        
    }

    public  class CouponDTO : CouponBaseDTO
    {
        public CouponDTO()
        {
            Type = CourseEnums.CouponType.PERCENT;
            TotalInstances = 1;
        }

        [DisplayName("Valid Until")]
        public DateTime? ExpirationDate { get; set; }

        [DisplayName("Discount period (months)")]
        public byte? SubscriptionMonths { get; set; }

        [Required]
        [DisplayName("Auto Generation")]
        public bool AutoGeneration { get; set; }

       
        [DisplayName("Usage Limit")]
        public int? UsageLimit { get; set; }
     
        [DisplayName("Active")]
        public bool IsActive { get; set; }

        public int ActualUsage { get; set; }
        
        [Required]
        public int TotalInstances { get; set; }

        public string ItemName { get; set; }

        public CourseEnums.eCouponKinds Kind { get; set; }
        public string KindDisplayName { get; set; }
    }

    public class AuthorCouponDTO : CouponDTO
    {
        public AuthorCouponDTO()
        {
            OwnerUserId     = -1;
            CouponId        = -1;
            CourseId        = -1;
            BundleId        = -1;
            UsageLimit      = null;
            TotalInstances  = 1;
            IsDeleteAllowed = true;
            Kind            = CourseEnums.eCouponKinds.Author;
        }
        public AuthorCouponDTO(int userId)
        {
            OwnerUserId     = userId;
            CouponId        = -1;
            CourseId        = -1;
            BundleId        = -1;
            UsageLimit      = null;
            TotalInstances  = 1;
            IsDeleteAllowed = true;
            Kind            = CourseEnums.eCouponKinds.Author;
        }
        public AuthorCouponDTO(int couponId, int userId)
        {
            OwnerUserId     = userId;
            CouponId        = couponId;
            CourseId        = -1;
            BundleId        = -1;
            UsageLimit      = null;
            TotalInstances  = 1;
            IsDeleteAllowed = true;
            Kind            = CourseEnums.eCouponKinds.Author;
        }
        public int? CourseId { get; set; }
        public int? BundleId { get; set; }

        public bool IsDeleteAllowed { get; set; }
    }

    public class CourseCouponDTO: CouponDTO
    {
        public CourseCouponDTO()
        {
            CourseId        = -1;
            CouponId        = -1;
            UsageLimit = null;
            IsDeleteAllowed = true;
        }
        public CourseCouponDTO(int courseId)
        {
            CourseId        = courseId;
            CouponId        = -1;
            UsageLimit = null;
            IsDeleteAllowed = true;
        }
        public CourseCouponDTO(int couponId,int courseId)
        {
            CourseId        = courseId;
            CouponId        = couponId;
            UsageLimit = null;
            IsDeleteAllowed = true;
        }
        public int? CourseId { get; set; }
        public bool IsDeleteAllowed { get; set; }
    }

    public class BundleCouponDTO : CouponDTO
    {
        public BundleCouponDTO()
        {
            BundleId = -1;
            CouponId = -1;
            UsageLimit = null;
            IsDeleteAllowed = true;
        }
        public BundleCouponDTO(int bundleId)
        {
            BundleId = bundleId;
            CouponId = -1;
            UsageLimit = null;
            IsDeleteAllowed = true;
        }
        public BundleCouponDTO(int couponId, int bundleId)
        {
            BundleId = bundleId;
            CouponId = couponId;
            UsageLimit = null;
            IsDeleteAllowed = true;
        }
        public int? BundleId { get; set; }
        public bool IsDeleteAllowed { get; set; }
    }


    public class CouponValidationToken
    {
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Discount { get; set; }
        public string Message { get; set; }
        public bool IsValid { get; set; }

        public bool IsFree { get; set; }
    }
}
