using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class CourseEntityMapper
    {
        public static Courses EditDto2CourseEntity(this CourseEditDTO dto,int authorId)
        {
            var entity = new Courses
                {
                     AuthorUserId            = authorId
                    ,uid                     = dto.Uid
                    ,CourseName              = dto.CourseName.TrimString()
                    ,CourseUrlName           = dto.CourseName.TrimString().OptimizedUrl()
                    ,StatusId                = (short)dto.Status                
                    ,Description             = dto.CourseDescription
                    ,OverviewVideoIdentifier = dto.PromoVideoIdentifier.ToString()
                    ,SmallImage              = dto.ThumbName
                    ,ClassRoomId             = dto.ClassRoomId
                    ,MetaTags                = dto.MetaTags
                    ,Created                 = DateTime.Now
                    ,LastModified            = DateTime.Now
                    ,DisplayOtherLearnersTab = dto.DisplayOtherLearnersTab
                    //default not nullable fields                  
                    ,AffiliateCommission = Constants.AFFILIATE_COMMISSION_DEFAULT
                    ,IsFreeCourse        = false                 
                };

            if (entity.PublishDate == null && dto.Status == CourseEnums.CourseStatus.Published) entity.PublishDate = DateTime.Now;

            return entity;
        }
        
        public static CRS_Bundles EditDto2BundleEntity(this BundleEditDTO dto,int authorId)
        {
            return new CRS_Bundles
                {
                     AuthorId                = authorId
                    ,uid                     = dto.Uid
                    ,BundleName              = dto.BundleName.TrimString()
                    ,BundleUrlName           = dto.BundleName.TrimString().OptimizedUrl()
                    ,StatusId                = (short)dto.Status
                    ,BundleDescription       = dto.Description.TrimString()
                    ,OverviewVideoIdentifier = dto.PromoVideoIdentifier.ToString()
                    ,BannerImage             = dto.ThumbName
                    ,MetaTags                = dto.MetaTags
                    ,AddOn                   = DateTime.Now
                    ,CreatedBy               = DtoExtensions.CurrentUserId
                };
        }

        public static void UpdateCourseEntity(this Courses entity, CourseEditDTO dto)
        {
            entity.CourseName              = dto.CourseName.TrimString();
            entity.CourseUrlName           = dto.CourseName.OptimizedUrl();
            entity.Description             = dto.CourseDescription;
            entity.OverviewVideoIdentifier = dto.PromoVideoIdentifier.ToString();
            entity.SmallImage              = dto.ThumbName;
            entity.ClassRoomId             = dto.ClassRoomId;
            entity.StatusId                = (short) dto.Status;
            entity.MetaTags                = dto.MetaTags;
            entity.LastModified            = DateTime.Now;
            entity.DisplayOtherLearnersTab = dto.DisplayOtherLearnersTab;

            if (entity.PublishDate == null && dto.Status == CourseEnums.CourseStatus.Published) entity.PublishDate = DateTime.Now;
        }

        public static void UpdateBundleEntity(this CRS_Bundles entity, BundleEditDTO dto)
        {
            entity.BundleName              = dto.BundleName.TrimString();
            entity.BundleUrlName           = dto.BundleName.TrimString().OptimizedUrl();
            entity.BundleDescription       = dto.Description.TrimString();
            entity.OverviewVideoIdentifier = dto.PromoVideoIdentifier.ToString();
            entity.BannerImage             = dto.ThumbName;
            entity.StatusId                = (short)dto.Status;
            entity.MetaTags                = dto.MetaTags;
            entity.UpdateDate              = DateTime.Now;
            entity.UpdatedBy               = DtoExtensions.CurrentUserId;

            if (entity.PublishDate == null && dto.Status == CourseEnums.CourseStatus.Published) entity.PublishDate = DateTime.Now;
        }

        public static CRS_BundleCourses NewBundleCourseEntity(this int bundleId, int courseId, short index)
        {
            return new CRS_BundleCourses
            {
                BundleId    = bundleId
                ,CourseId   = courseId
                ,OrderIndex = index
                ,IsActive   = true
                ,AddOn      = DateTime.Now
                ,CreatedBy  = DtoExtensions.CurrentUserId
            };
        }
        public static void UpdateBundleCourseEntityOrderIndex(this CRS_BundleCourses entity, short index)
        {
            entity.OrderIndex = index;            
        }
        public static void UpdateBundleCourseEntityStatus(this CRS_BundleCourses entity, bool isActive)
        {
            entity.IsActive     = isActive;
            entity.UpdateDate   = DateTime.Now;
            entity.UpdatedBy    = DtoExtensions.CurrentUserId;
        }

        public static void UpdateBundleEntityPrice(this CRS_Bundles entity, BundlePriceDTO token)
        {
            entity.AffiliateCommission      = token.AffiliateCommission ?? entity.AffiliateCommission;
            entity.UpdateDate               = DateTime.Now;
            entity.UpdatedBy                = DtoExtensions.CurrentUserId;
        }

        public static void UpdateCourseFreePrice(this Courses entity, bool isFree)
        {
            entity.IsFreeCourse                = isFree;
           // entity.AffiliateCommission         = token.AffiliateCommission ?? entity.AffiliateCommission;
            entity.LastModified                = DateTime.Now;
        }

        public static void UpdateCourseAffiliateCommission(this Courses entity, decimal? commission)
        {
            entity.AffiliateCommission = commission ?? entity.AffiliateCommission;
            entity.LastModified        = DateTime.Now;
        }
        public static void UpdateBundleAffiliateCommission(this CRS_Bundles entity, decimal? commission)
        {
            entity.AffiliateCommission  = commission ?? entity.AffiliateCommission;
            entity.UpdateDate           = DateTime.Now;
            entity.UpdatedBy            = DtoExtensions.CurrentUserId;
        }

        public static void UpdateCourseEntity(this Courses entity, WizardCourseNameDTO token)
        {
            entity.CourseName              = token.CourseName.TrimString();
            entity.CourseUrlName           = token.CourseName.TrimString().OptimizedUrl();
            entity.Description             = token.CourseDescription;
            entity.DisplayOtherLearnersTab = token.DisplayOtherLearnersTab;
            entity.LastModified            = DateTime.Now;
        }

        public static void UpdateCourseEntity(this Courses entity, WizardCourseVisualsDTO token)
        {
            entity.OverviewVideoIdentifier = token.PromoVideoIdentifier.ToString();
            entity.SmallImage              = token.ThumbName;
            entity.LastModified            = DateTime.Now;
        }

        public static void UpdateCourseEntity(this Courses entity, WizardCourseMetaDTO token)
        {
            entity.MetaTags     = token.MetaTags;
            entity.LastModified = DateTime.Now;
        }

        public static void UpdateCourseEntity(this Courses entity, WizardCoursePublishDTO token)
        {
            entity.StatusId     = (short)token.Status;
            entity.LastModified = DateTime.Now;

            if (entity.PublishDate == null && token.Status == CourseEnums.CourseStatus.Published) entity.PublishDate = DateTime.Now;
        }
        //chapters
        public static CourseChapters EditDto2CourseChapterEntity(this ChapterEditDTO dto)
        {
            return new CourseChapters
                {
                     CourseId                = dto.CourseId
                    ,ChapterName             = dto.Name.TrimString()
                    ,ChapterOrdinal          = dto.OrderIndex ?? 0
                    ,ChapterDescriptionHTML  = string.Empty
                    ,Created                 = DateTime.Now
                    ,LastModified            = DateTime.Now
                };
        }

        public static void UpdateChapterEntity(this CourseChapters entity, ChapterEditDTO dto)
        {
            entity.ChapterName              = dto.Name.TrimString();
            entity.LastModified             = DateTime.Now;
        }
        public static void UpdateChapterEntity(this CourseChapters entity, WizardChapterListEditDTO dto)
        {
            entity.ChapterName = dto.Name.TrimString();
            entity.LastModified = DateTime.Now;
        }

        public static void UpdateChapterEntityOrderIndex(this CourseChapters entity, int index)
        {
            entity.ChapterOrdinal= index;
            entity.LastModified  = DateTime.Now;
        }


        //chapter videos
        public static ChapterVideos EditDto2ChapterVideoEntity(this ChapterVideoEditDTO dto)
        {
            return new ChapterVideos
                {
                     CourseChapterId         = dto.ChapterId
                    ,VideoTitle              = dto.Title.TrimString()
                    ,Ordinal                 = dto.OrderIndex ?? 0
                    ,VideoSummary            = dto.SummaryHTML
                    ,VideoSupplierIdentifier = dto.VideoIdentifier!=null ? dto.VideoIdentifier.ToString() : string.Empty
                    ,IsOpen                  = dto.IsOpen ? 1 : 0
                    ,Created                 = DateTime.Now
                    ,LastModified            = DateTime.Now
                };
        }
     
        public static void UpdateChapterVideoEntity(this ChapterVideos entity, ChapterVideoEditDTO dto)
        {
            entity.VideoTitle              = dto.Title.TrimString();
            entity.VideoSummary            = dto.SummaryHTML;
            entity.VideoSupplierIdentifier = dto.VideoIdentifier != null ? dto.VideoIdentifier.ToString() : string.Empty;
            entity.IsOpen                  = dto.IsOpen ? 1 : 0;
            entity.LastModified            = DateTime.Now;
        }

        public static void UpdateChapterVideoEntityOrderIndex(this ChapterVideos entity, int index)
        {
            entity.Ordinal       = index;
            entity.LastModified  = DateTime.Now;
        }

        //chapter Links
        public static ChapterLinks EditDto2ChapterLinkEntity(this ChapterLinkEditDTO dto)
        {
            return new ChapterLinks
                {
                     CourseChapterId = dto.ChapterId
                    ,LinkText        = dto.Title.TrimString()
                    ,LinkHref        = dto.LinkHref.FormatToFullUri()
                    ,LinkType        = (int)dto.Kind
                    ,Ordinal         = dto.OrderIndex ?? 0
                    ,Created         = DateTime.Now
                    ,LastModified    = DateTime.Now
                };
        }

        public static void UpdateChapterLinkEntity(this ChapterLinks entity, ChapterLinkEditDTO dto)
        {
            entity.LinkText     = dto.Title.TrimString();
            entity.LinkHref     = dto.LinkHref.FormatToFullUri();
            entity.LastModified = DateTime.Now;
        }

        public static void UpdateChapterLinkEntityOrderIndex(this ChapterLinks entity, int index)
        {
            entity.Ordinal       = index;
            entity.LastModified  = DateTime.Now;
        }

        //categories
        public static CourseCategories CategoryId2CourseCategoryEntity(this int categoryId,int courseId)
        {
            return new CourseCategories
                {
                     CategoryId = categoryId
                     ,CourseId  = courseId
                };
        }

        public static CRS_BundleCategories CategoryId2BundleCategoryEntity(this int categoryId, int bundleId)
        {
            return new CRS_BundleCategories
            {
                CategoryId = categoryId
                ,BundleId = bundleId
            };
        }

        #region user portal
        public static UserCourseReviews Dto2CourseReviewEntity(this CourseUserReviewDTO dto,int userId)
        {
            return new UserCourseReviews
            {
                CourseId      = dto.CourseId
                ,UserId       = userId
                ,ReviewRating = dto.Rating
                ,ReviewTitle  = dto.Title.TrimString()
                ,ReviewText   = dto.Text
                ,ReviewDate   = DateTime.Now
                ,Approved     = true
            };
        }

        public static void UpdateCourseReviewEntity(this UserCourseReviews entity, CourseUserReviewDTO dto)
        {
            entity.ReviewRating = dto.Rating;
            entity.ReviewTitle  = dto.Title.TrimString();
            entity.ReviewText   = dto.Text;
            entity.ReviewDate   = DateTime.Now;
        }
        #endregion

        #region category manage
        public static Categories Dto2CategoryEntity(this CategoryEditDTO token)
        {
            return new Categories
                {
                     CategoryName = token.name
                    ,IsActive = token.isActive                    
                };
        }

        public static void UpdateCategoryEntity(this Categories entity, CategoryEditDTO token)
        {
            entity.CategoryName = token.name;
            entity.IsActive     = token.isActive;
        }
        #endregion

    }
}
