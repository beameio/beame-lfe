using System;
using System.Collections.Generic;
using System.Linq;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    public class CourseRepository : Repository<Courses>, ICourseRepository
    {
        public CourseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public CRS_CourseToken GetCourseToken(short currencyId, int courseId)
        {
            return DataContext.tvf_CRS_GetCourseInfo(currencyId,courseId).FirstOrDefault();
        }

        public Courses FindCourseByUrlName(string courseUrlName, string authorUrlName)
        {
            return DataContext.tvf_CRS_FindCourseByUrlName(courseUrlName, authorUrlName).FirstOrDefault();
        }

        public IEnumerable<CRS_CourseToken> FindCoursesByUrlName(short currencyId, string urlName)
        {
            return DataContext.tvf_CRS_GetCourseInfoByUrlName(currencyId,urlName);
        }

        public IEnumerable<USER_ReviewToken> GetAuthorReviews(int userId, DateTime from, DateTime to)
        {
            return DataContext.tvf_USER_GetCourseReviews(from,to,userId,null);
        }

        public IEnumerable<USER_ReviewToken> GetCourseReviews(int courseId, DateTime @from, DateTime to)
        {
            return DataContext.tvf_USER_GetCourseReviews(from, to, null,courseId);
        }

        public IEnumerable<USER_ReviewToken> GetAllCourseReviews(int courseId)
        {
            return DataContext.tvf_USER_GetAllCourseReviews(courseId);
        }

        public IEnumerable<CRS_LearnerToken> GetCourseSubscribers(int courseId)
        {
            return DataContext.tvf_CRS_GetSubscribers(courseId);
        }

        public IEnumerable<USER_CourseListToken> GetUserCourses(short currencyId, int? id)
        {
            return DataContext.tvf_USER_GetCourses(currencyId,id);
        }

        public CRS_ReviewAuthorMessageToken GetAuthorReviewMessageToken(int reviewId)
        {
            try
            {
                return DataContext.tvf_CRS_GetReviewToken4Author(reviewId).FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<CRS_ReviewLearnerMessageToken> GetLearnersReviewMessageToken(int reviewId)
        {
            try
            {
                return DataContext.tvf_CRS_GetReviewToken4Learners(reviewId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<Categories> GetCourseCategories(int courseId)
        {
            return DataContext.tvf_CRS_GetCourseCategories(courseId);
        }

        public short GetCourseContentsCount(int courseId)
        {
            return DataContext.tvf_CRS_GetCourseContentCnt(courseId).FirstOrDefault() ?? 0;
        }

    }

    public class CourseCategoryRepository : Repository<CourseCategories>, ICourseCategoryRepository
    {
        public CourseCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
    public class CourseAssetsRepository : Repository<CRS_Assets>, ICourseAssetsRepository
    {
        public CourseAssetsRepository(IUnitOfWork unitOfWork): base(unitOfWork)
        {
        }
    }
    public class BundleRepository : Repository<CRS_Bundles>, IBundleRepository
    {
        public BundleRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<CRS_BundleListToken> GetAuthorBundles(int userId)
        {
            return DataContext.tvf_USER_GetBundles(userId);
        }

        public CRS_BundleInfoToken GetBundleInfo(short currencyId, int bundleId)
        {
            return DataContext.tvf_CRS_GetBundleInfo(currencyId,bundleId).FirstOrDefault();
        }
    }

    public class BundleCourseRepository : Repository<CRS_BundleCourses>, IBundleCourseRepository
    {
        public BundleCourseRepository(IUnitOfWork unitOfWork): base(unitOfWork){}
        public IEnumerable<CRS_BundleCourseToken> GetBundleCourses(int bundleId)
        {
            return DataContext.tvf_CRS_GetBundleCourses(bundleId).OrderBy(x => x.OrderIndex).ToList();
        }

        public IEnumerable<CRS_AvailableCourseToken> GetAvailableCourses4Bundle(int bundleId)
        {
            return DataContext.tvf_CRS_GetAvailableCourses4Bundle(bundleId).ToList();
        }
    }

    public class BundleCategoryRepository : Repository<CRS_BundleCategories>, IBundleCategoryRepository
    {
        public BundleCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class ChapterRepository : Repository<CourseChapters>, IChapterRepository
    {
        public ChapterRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<CourseChapters> GetCourseChaptersByUid(Guid uid)
        {
            return DataContext.tvf_CRS_GetChaptersByUid(uid);
        }
    }

    public class ChapterLinkRepository : Repository<ChapterLinks>, IChapterLinkRepository
    {
        public ChapterLinkRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class ChapterVideoRepository : Repository<ChapterVideos>, IChapterVideoRepository
    {
        public ChapterVideoRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class CourseChangeLogRepository : Repository<CRS_CourseChangeLog>, ICourseChangeLogRepository
    {
        public CourseChangeLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class WizardStepsRepository : Repository<CRS_WizardStepsLOV>, IWizardStepsRepository
    {
        public WizardStepsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class CategoryRepository : Repository<Categories>, ICategoryRepository
    {
        public CategoryRepository(IUnitOfWork unitOfWork): base(unitOfWork){}

        public List<CAT_CategoryListToken> GetCategoriesLOV()
        {
            return DataContext.tvf_CAT_GetCategoriesWithCourseCount().ToList();
        }

        public List<SALE_BaseItemToken> GetCategoryItems(int categoryId)
        {
            return DataContext.tvf_CAT_GetCategoryItems(categoryId).ToList();
        }

        public List<CAT_CategoryToken> GetCategories()
        {
            return DataContext.tvf_CAT_GetCategories().ToList();
        }
    }
}
