using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.DataTokens;


namespace LFE.Application.Services.Interfaces
{
    public interface IAuthorAdminCourseServices : IDisposable
    {
        //supporting        
        BaseEntityDTO FindCourseByUid(int authorId, Guid uid);
        BaseEntityDTO FindBundleByUid(int authorId, Guid uid);
        bool ValidateAuthorCourseByUid(int authorId, Guid uid);
        bool ValidateAuthorBundleByUid(int authorId, Guid uid);
        //manage
        CourseEditDTO GetCourseEditDTO(int id);
        BundleEditDTO GetBundleEditDTO(int id);

        BaseItemListDTO GetBaseItemListDto(int id, BillingEnums.ePurchaseItemTypes type,out string error);

        bool SaveCourse(ref CourseEditDTO dto, int? authorId, out string error,string sessionId=null);
        bool SaveBundle(ref BundleEditDTO dto, int? authorId, out string error);
        bool DeleteCourse(int courseId, out string error);
        bool DeleteBundle(int bundleId, out string error);
        bool IsBundlePurchased(int bundleId);
        bool IsCoursePurchased(int curseId);
        bool SaveCourseCategories(int courseId, List<int> categories, out string error);
        bool SaveBundleCategories(int bundleId, List<int> categories, out string error);
        List<int> GetCourseCategoryIds(int courseId);
        List<int> GetBundleCategoryIds(int bundleId);

        List<BundleCourseListDTO> GetBundleCourses(int bundleId);
        List<BundleCourseListDTO> GetAvailableCourses4Bundle(int bundleId);
        bool AddCourse2Bundle(int bundleId, int courseId, out string error);
        bool RemoveCourseFromBundle(int rowId, out string error);
        //bool SaveBundleCourses(int bundleId, List<int> courseIds, out string error);
        bool SaveBundleCoursesOrder(int[] rowsIds, out string error);
        //pricing
        CoursePriceDTO GetCoursePrice(int courseId, short currencyId);
        List<PriceLineDTO> GetItemPriceLines(int itemId, BillingEnums.ePurchaseItemTypes type, short currencyId);
        List<PriceLineDTO> GetAllItemPriceLines(int itemId, BillingEnums.ePurchaseItemTypes type,bool excludeFree);
        //bool SaveCoursePrice(CoursePriceDTO dto, out string error);
        bool SaveCourseFreePrice(BaseItemToken item,bool isFree, out string error);
        bool SaveItemAffiliateCommission(ItemAffiliateCommissionDTO dto, out string error);
        bool SavePriceLine(PriceLineDTO dto, out string error);
        bool UpdatePriceLine(int lineId,decimal price, out string error);
        bool DeletePriceLine(int lineId, out string error);

        BundlePriceDTO GetBundlePrice(int courseId, short currencyId);
        bool SaveBundlePrice(BundlePriceDTO dto, out string error);

        //report
        List<ReviewDTO> GetAuthorReviews(int userId, ReportEnums.ePeriodSelectionKinds? periodKind);
        List<ReviewDTO> GetCourseReviews(int courseId);
        List<OrderLineDTO> GetCourseSales(int courseId, ReportEnums.ePeriodSelectionKinds periodKind, BillingEnums.eOrderLineTypes lineType);
        List<OrderLineDTO> GetBundleSales(int bundleId, ReportEnums.ePeriodSelectionKinds periodKind, BillingEnums.eOrderLineTypes lineType);
        List<CourseListDTO> GetAuthorCoursesList(short currencyId,int userId);
        List<BundleListDTO> GetAuthorBundlesList(int userId);
        
        //chapters
        bool IsAnyCourseContentsCreated(int courseId);
        List<BaseListDTO> GetCourseChapters(int courseId);
        List<BaseListDTO> GetCourseChapters(Guid uid);
        List<ChapterEditDTO> GetCourseEidtChaptersList(int courseId);
        List<CourseContentToken> GetCourseContentsList(int courseId);
        ChapterEditDTO GetChapterEditDTO(int chapterId, int courseId);
        bool SaveChapter(ref ChapterEditDTO dto, out string error);
        bool RenameChapter(ChapterEditDTO dto, out string error);
        bool RenameChapter(WizardChapterListEditDTO dto, out string error);
        bool DeleteChapter(int chapterId, out string error);
        bool SaveChaptersOrder(int[] chapterIds, out string error);
        bool SaveContentsOrder(ContentSortToken[] tokens, out string error);
        //chapter videos
        List<VideoListDto> GetChapterVideos(int chapterId);
        List<ChapterVideoEditDTO> GetChapterEditVideosList(int chapterId);        
        
        ChapterVideoEditDTO GetChapterVideoEditDTO(int videoId, int chapterId,int authorId);
        bool SaveChapterVideo(ref ChapterVideoEditDTO dto, out string error);
        bool DeleteChapterVideo(int chapterVideoId, out string error);
        bool SaveChapterVideosOrder(int[] videoIds, out string error);
        //int GetVideoChapterUsage(long identifier);

        //chapter links
        List<LinkListDto> GetChapterLinks(int chapterId);
        List<ChapterLinkEditDTO> GetChapterEditLinksList(int chapterId);
        ChapterLinkEditDTO GetChapterLinkEditDTO(int linkId, int chapterId,CourseEnums.eChapterLinkKind kind);
        bool SaveChapterLink(ref ChapterLinkEditDTO dto, out string error);
        bool DeleteChapterLink(int chapterLinkId, out string error);
        bool SaveChapterLinksOrder(int[] linkIds, out string error);

        //course stores
        List<TreeWebStoreDTO> GetCourseStoresTree(int courseId, int? ownerUserId, out string error);
    }

    public interface ICourseWizardServices : IDisposable
    {
        CourseWizardDto LoadCourseWizard(Guid uid, int userId, CourseEnums.eWizardSteps? next = null, int? selectedChapterId = null,short? currencyId = null);
        List<BreadcrumbStepDTO> GetBreadcrumbSteps(CourseEnums.eWizardSteps completedSteps, CourseEnums.eWizardSteps currentStep, int chaptersCnt);

        //save steps
        bool SaveCourseName(WizardCourseNameDTO token, int userId, out string error);
        bool SaveCourseVisuals(WizardCourseVisualsDTO token, out string error);
        bool SaveCourseMeta(WizardCourseMetaDTO token, out string error);
        bool PublishCourse(WizardCoursePublishDTO token, int userId, out string error);
    }

    public interface IWidgetCourseServices : IDisposable
    {
        //supporting
        int GetItemAuthor(int itemId, BillingEnums.ePurchaseItemTypes type);
        CourseInfoDTO GetCourseToken(short currencyId,int courseId,string trackingId);
        BundleInfoDTO GetBundleToken(short currencyId, int bundleId, string trackingId);        

        BaseEntityDTO FindCourseByUrlName(string authorName,string urlName);
        BaseBundleDTO FindBundleByUrlName(string authorName, string urlName);
        
        bool IsItemAccessAllowed4User(int? userId, int itemId, byte itemTypeId);
        ItemAccessStateToken GetItemAccessState4User(int? userId, int itemId, byte itemTypeId);
        ItemAccessStateToken GetCourseAccessState4User(int? userId, int id);
        ItemAccessStateToken GetBundleAccessState4User(int? userId, int id);

        bool OnCourseRentalFinished(int courseId, int userId, out string error);
        //CourseFbToken GetCourseFbToken(int courseId);

        //viewer
        LearnerCourseViewerDTO GetLearnerCourseViewerDTO(int id, int userId, bool loadBasicInfo = false);
        List<ContentTreeViewItemDTO> GetCourseChaptersList(int courseId);
       // List<ContentTreeViewItemDTO> GetCourseG2TAssetsList(int courseId);
        List<ChapterLinkListToken> GetChapterLinks(int chapterId, CourseEnums.eChapterLinkKind kind);
        CourseUserReviewDTO GetUserCourseReview(int courseId, int userId);
        bool SaveCourseReview(CourseUserReviewDTO dto, int userId, out string error);
        int GetCourseRating(int courseId);

        //other learner
        List<LearnerListItemDTO> GetOtherCourseLearners(int courseId, int userId);

        //bundles
        List<BundleCourseListDTO> GetBundleCoursesList(int bundleId,string trackingId = null);

        //Purchase
        void PostFasebookPurchaseMessages(PurchaseMessageDTO messageToken);
        ItemPurchaseDataToken GetCoursePurchaseDtoByCourseUrlName(short currencyId, string authorName, string courseUrlName);
        ItemPurchaseDataToken GetBundlePurchaseDtoByBundleUrlName(short currencyId, string authorName, string bundleUrlName);

        ItemPurchaseDataToken GetItemPurchaseDtoByPriceLineId(int lineId, out string error);        
        ItemPurchaseCompleteToken GetItemPurchaseCompleteToken(int orderNo, out string error);
    }

    public interface IAuthorAdminCategoryServices : IDisposable
    {
        List<CategoryViewDTO> ActiveCategories();
        List<BaseListDTO> GetCategoriesLOV();
        List<BaseItemListDTO> GetCategoryItemsLOV(int categoryId);        
    }

    public interface IPortalAdminCourseServices : IDisposable
    {
        List<CourseBaseToken> GetCoursesList();
    }

    public interface ICategoryManageServices : IDisposable
    {
        List<CategoryEditDTO> GetCategories();
        bool SaveCategory(ref CategoryEditDTO token, out string error);
        bool DeleteCategory(CategoryEditDTO token, out string error);
    }
}
