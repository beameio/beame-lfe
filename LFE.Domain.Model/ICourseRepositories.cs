using System;
using System.Collections.Generic;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    public interface ICourseRepository : IRepository<Courses>
    {
        CRS_CourseToken GetCourseToken(short currencyId, int courseId);
        IEnumerable<CRS_CourseToken> FindCoursesByUrlName(short currencyId, string urlName);
        Courses FindCourseByUrlName(string courseUrlName, string authorUrlName);
        IEnumerable<USER_CourseListToken> GetUserCourses(short currencyId, int? id);
        IEnumerable<USER_ReviewToken> GetAuthorReviews(int userId,DateTime from, DateTime to);
        IEnumerable<USER_ReviewToken> GetCourseReviews(int courseId, DateTime from, DateTime to);
        IEnumerable<CRS_LearnerToken> GetCourseSubscribers(int courseId);
        IEnumerable<Categories> GetCourseCategories(int courseId);
        CRS_ReviewAuthorMessageToken GetAuthorReviewMessageToken(int reviewId);
        IEnumerable<CRS_ReviewLearnerMessageToken> GetLearnersReviewMessageToken(int reviewId);
        short GetCourseContentsCount(int courseId);
        IEnumerable<USER_ReviewToken> GetAllCourseReviews(int courseId);
    }

    public interface ICourseCategoryRepository : IRepository<CourseCategories>{}
    public interface ICourseAssetsRepository : IRepository<CRS_Assets> { }
    public interface IBundleRepository : IRepository<CRS_Bundles>
    {
        IEnumerable<CRS_BundleListToken> GetAuthorBundles(int userId);

        CRS_BundleInfoToken GetBundleInfo(short currencyId, int bundleId);
    }

    public interface IBundleCourseRepository : IRepository<CRS_BundleCourses>
    {
        IEnumerable<CRS_BundleCourseToken> GetBundleCourses(int bundleId);
        IEnumerable<CRS_AvailableCourseToken> GetAvailableCourses4Bundle(int bundleId);
    }
    public interface IBundleCategoryRepository : IRepository<CRS_BundleCategories>
    {
    }
    public interface IChapterRepository : IRepository<CourseChapters>
    {
        IEnumerable<CourseChapters> GetCourseChaptersByUid(Guid uid);
    }

    public interface IChapterLinkRepository : IRepository<ChapterLinks>{}

    public interface IChapterVideoRepository : IRepository<ChapterVideos>{}

    public interface IWizardStepsRepository :IRepository<CRS_WizardStepsLOV>{}

    public interface ICourseChangeLogRepository : IRepository<CRS_CourseChangeLog> { }

    public interface ICategoryRepository : IRepository<Categories>
    {
        List<CAT_CategoryListToken> GetCategoriesLOV();
        List<SALE_BaseItemToken> GetCategoryItems(int categoryId);

        List<CAT_CategoryToken> GetCategories();
    }   
}
