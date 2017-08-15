using System.Collections.Generic;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    public interface ICouponRepository : IRepository<Coupons>
    {
        IEnumerable<CRS_CouponToken> GetCourseCoupons(int courseId);
        IEnumerable<CRS_CouponToken> GetBundleCoupons(int bundleId);
        IEnumerable<CRS_CouponToken> GetAuthorCoupons(int authorId); 
    }

    public interface ICouponInstanceRepository : IRepository<CouponInstances>
    {
    }
}
