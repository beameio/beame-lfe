using System.Collections.Generic;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    public class CouponRepository : Repository<Coupons>,  ICouponRepository
    {
        public CouponRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<CRS_CouponToken> GetCourseCoupons(int courseId)
        {
            return DataContext.tvf_CRS_GetCourseCoupons(courseId);
        }

        public IEnumerable<CRS_CouponToken> GetBundleCoupons(int bundleId)
        {
            return DataContext.tvf_CRS_GetBundlesCoupons(bundleId);
        }

        public IEnumerable<CRS_CouponToken> GetAuthorCoupons(int authorId)
        {
            return DataContext.tvf_CRS_GetAuthorsCoupons(authorId);
        }
    }

    public class CouponInstanceRepository : Repository<CouponInstances>, ICouponInstanceRepository
    {
        public CouponInstanceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
