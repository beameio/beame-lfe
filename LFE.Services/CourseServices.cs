using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Helper;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Application.Services
{
    public class CourseServices : ServiceBase, IAuthorAdminCourseServices, IAuthorAdminCategoryServices, IWidgetCourseServices, IPortalAdminCourseServices, ICategoryManageServices
    {
        private readonly IFacebookServices _facebookServices;
        private readonly IEmailServices _emailServices;
        private readonly IQuizAdminServices _quizAdminServices;
       

        private enum ePriceRevisionAction
        {
            New
            ,Update
            ,Delete
        }

        public CourseServices()
        {
           _facebookServices      = DependencyResolver.Current.GetService<IFacebookServices>();
           _emailServices         = DependencyResolver.Current.GetService<IEmailServices>();
           _quizAdminServices = DependencyResolver.Current.GetService<IQuizAdminServices>();           
        }

        #region IAuthorAdminCourseServices implementation
        #region private helpers
        /// <summary>
        /// validate course name on save
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="authorId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        
        private bool IsBundleNameValid(BundleEditDTO dto, int authorId, out string error)
        {
            if (!dto.BundleName.IsObjectNameValid(out error)) return false;

            try
            {
                if (dto.BundleId < 0)
                {
                    return !CourseRepository.IsAny(x => x.AuthorUserId == authorId && x.CourseName == dto.BundleName);
                }

                return !CourseRepository.IsAny(x => x.AuthorUserId == authorId && x.CourseName == dto.BundleName && x.Id != dto.BundleId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        /// <summary>
        /// validate chapter name on chapter save event
        /// </summary>
        /// <param name="dto"></param>        
        /// <param name="error"></param>
        /// <returns></returns>
        private bool IsChapterNameValid(ChapterEditDTO dto, out string error)
        {
            error = string.Empty;
            try
            {
                if (dto.ChapterId < 0)
                {
                    return !ChapterRepository.IsAny(x => x.CourseId == dto.CourseId && x.ChapterName == dto.Name);
                }

                return !ChapterRepository.IsAny(x => x.CourseId == dto.CourseId && x.ChapterName == dto.Name && x.Id != dto.ChapterId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

       

        private void UpdateCourseChapterIndieces(int courseId)
        {
            var chapters = ChapterRepository.GetMany(x => x.CourseId == courseId).OrderBy(x => x.ChapterOrdinal).ToList();

            for (var i = 0; i < chapters.Count; i++)
            {
                chapters[i].UpdateChapterEntityOrderIndex(i);
            }

            ChapterRepository.UnitOfWork.CommitAndRefreshChanges();
        }

        private void UpdateChapterVideoIndieces(int chapterId)
        {
            var videos = ChapterVideoRepository.GetMany(x => x.CourseChapterId == chapterId).OrderBy(x => x.Ordinal).ToList();

            for (var i = 0; i < videos.Count; i++)
            {
                videos[i].UpdateChapterVideoEntityOrderIndex(i);
            }

            ChapterVideoRepository.UnitOfWork.CommitAndRefreshChanges();
        }

        private void UpdateChapterLinksIndieces(int chapterId)
        {
            var videos = ChapterLinkRepository.GetMany(x => x.CourseChapterId == chapterId).OrderBy(x => x.Ordinal).ToList();

            for (var i = 0; i < videos.Count; i++)
            {
                videos[i].UpdateChapterLinkEntityOrderIndex(i);
            }

            ChapterLinkRepository.UnitOfWork.CommitAndRefreshChanges();
        }

        private void SavePriceLineRevision(int lineId, decimal price,ePriceRevisionAction action)
        {
            try
            {
                switch (action)
                {
                    case ePriceRevisionAction.New:
                        PriceRevisionsReposiotry.Add(lineId.ToBillItemsPriceRevision(price));
                        break;
                    case ePriceRevisionAction.Update:
                        var current = PriceRevisionsReposiotry.GetMany(x=>x.PriceLineId==lineId && x.ToDate == null).OrderByDescending(x=>x.RevisionId).FirstOrDefault();
                        if (current == null)
                        {
                            Logger.Warn("revision for " + lineId + " not found");
                        }
                        else
                        {
                            current.UpdatePriceRevision(false);
                        }

                        PriceRevisionsReposiotry.Add(lineId.ToBillItemsPriceRevision(price));
                        break;
                    case ePriceRevisionAction.Delete:
                        current = PriceRevisionsReposiotry.GetMany(x=>x.PriceLineId==lineId && x.ToDate == null).OrderByDescending(x=>x.RevisionId).FirstOrDefault();
                        if (current == null)
                        {
                            Logger.Warn("revision for " + lineId + " not found");
                        }
                        else
                        {
                            current.UpdatePriceRevision(true);
                        }
                        break;
                }

                PriceRevisionsReposiotry.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("save price revision", lineId, ex, CommonEnums.LoggerObjectTypes.Course);
            }
        }
        #endregion
        
        public BaseEntityDTO FindCourseByUid(int authorId, Guid uid)
        {
            try
            {
                var entity = CourseRepository.Get(x => x.AuthorUserId == authorId && x.uid == uid);

                return entity == null ? new BaseEntityDTO("New Course",uid) : entity.CourseEntity2BaseEntityDTO();
            }
            catch (Exception ex)
            {
                Logger.Error("find course by uid", authorId, ex, CommonEnums.LoggerObjectTypes.Course);

                return null;
            }
        }

        public BaseEntityDTO FindBundleByUid(int authorId, Guid uid)
        {
            try
            {
                var entity = BundleRepository.Get(x => x.AuthorId == authorId && x.uid == uid);

                return entity == null ? new BaseEntityDTO("New Bundle", uid) : entity.BundleEntity2BaseEntityDTO();
            }
            catch (Exception ex)
            {
                Logger.Error("find bundle by uid", authorId, ex, CommonEnums.LoggerObjectTypes.Course);

                return null;
            }
        }

        public bool ValidateAuthorCourseByUid(int authorId, Guid uid)
        {
            try
            {
                var entity = CourseRepository.Get(x => x.uid == uid);

                return entity == null || CourseRepository.IsAny(x => x.AuthorUserId == authorId && x.uid == uid);
            }
            catch (Exception ex)
            {
                Logger.Error("validate author course by uid", authorId, ex, CommonEnums.LoggerObjectTypes.Course);

                return false;
            }
        }

        public bool ValidateAuthorBundleByUid(int authorId, Guid uid)
        {
            try
            {
                var entity = BundleRepository.Get(x => x.uid == uid);

                return entity == null || BundleRepository.IsAny(x => x.AuthorId == authorId && x.uid == uid);
            }
            catch (Exception ex)
            {
                Logger.Error("validate author bundle by uid", authorId, ex, CommonEnums.LoggerObjectTypes.Course);

                return false;
            }
        }

        #region course&bundle manage

        public List<int> GetCourseCategoryIds(int courseId)
        {
            try
            {
                return CourseCategoryRepository.GetMany(x => x.CourseId == courseId).Select(x => x.CategoryId).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get course category ids",ex,courseId,CommonEnums.LoggerObjectTypes.Course);
                return new List<int>();
            }
        }

        public List<int> GetBundleCategoryIds(int bundleId)
        {
            try
            {
                return BundleCategoryRepository.GetMany(x => x.BundleId == bundleId).Select(x => x.CategoryId).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get bundle category ids", ex, bundleId, CommonEnums.LoggerObjectTypes.Course);
                return new List<int>();
            }
        }

        /// <summary>
        /// get token for course detail tab by courseID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CourseEditDTO GetCourseEditDTO(int id)
        {
            try
            {
                var entity = CourseRepository.GetById(id);
                
                if (entity == null) return new CourseEditDTO("New Course");

                var token = entity.CourseEntity2CourseEditDTO(IsItemPricesExists(id,BillingEnums.ePurchaseItemTypes.COURSE));               
                
                if (token.PromoVideoIdentifier != null)
                {
                    token.PromoVideo = _GetVideoToken((long)token.PromoVideoIdentifier, token.AuthorId);
                }

                //get categories
                token.Categories = GetCourseCategoryIds(id);

                return token;
            }
            catch (Exception ex)
            {
               
                Logger.Error("get course edit dto by id", id, ex, CommonEnums.LoggerObjectTypes.Course);

                return null;
            }
        }

        public BundleEditDTO GetBundleEditDTO(int id)
        {
            try
            {
                var entity = BundleRepository.GetById(id);

                if (entity == null) return new BundleEditDTO("New Bundle");

                var token = entity.BundleEntity2BundleEditDto(IsItemPricesExists(id,BillingEnums.ePurchaseItemTypes.BUNDLE));

                //set VideoThumbUrl
                if (token.PromoVideoIdentifier != null)
                {
                    token.PromoVideo = _GetVideoToken((long)token.PromoVideoIdentifier, token.AuthorId);
                }

                //get categories
                token.Categories = GetBundleCategoryIds(id);

                return token;
            }
            catch (Exception ex)
            {

                Logger.Error("get bundle edit dto by id", id, ex, CommonEnums.LoggerObjectTypes.Course);

                return null;
            }
        }

        public BaseItemListDTO GetBaseItemListDto(int id, BillingEnums.ePurchaseItemTypes type,out string error)
        {
            error = string.Empty;
            try
            {
                switch (type)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        var entity = CourseRepository.GetById(id);
                        if (entity != null) return entity.CourseEntity2BaseItemListDto();
                        error = "course entity not found";
                        return null;
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        var bentity = BundleRepository.GetById(id);
                        if (bentity != null) return bentity.BundleEntity2BaseItemListDto();
                        error = "course entity not found";
                        return null;
                    default:
                        error = "invalid item type";
                        return null;
                }
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("get web store category base item token",ex,id,CommonEnums.LoggerObjectTypes.Course);
                return null;
            }
        }

        /// <summary>
        /// save course / course detail tab action
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="authorId"></param>
        /// <param name="error"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public bool SaveCourse(ref CourseEditDTO dto,int? authorId, out string error,string sessionId=null)
        {
            if (authorId == null)
            {
                error = "authorId missing";
                return false;
            }


            if(!IsCourseNameValid(dto.CourseId,dto.CourseName,(int) authorId,out error))
            {
                error = string.IsNullOrEmpty(error) ? "Course Name already exists" : error;
                return false;
            }

            try
            {
                Courses entity;

                if (dto.CourseId < 0) //new course
                {
                    entity = dto.EditDto2CourseEntity((int) authorId);
                    CourseRepository.Add(entity);                    
                }
                else
                {
                    entity = CourseRepository.GetById(dto.CourseId);

                    if (entity == null)
                    {
                        error = "Course entity not found";
                        return false;
                    }

                    entity.UpdateCourseEntity(dto);
                }

                if(!CourseRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                //add role
                AddRole2User((int) authorId, CommonEnums.UserRoles.Author);

                dto.CourseId = entity.Id;

                if (dto.Status != CourseEnums.CourseStatus.Published || entity.FbObjectPublished) return CourseRepository.UnitOfWork.CommitAndRefreshChanges(out error) && SaveCourseCategories(dto.CourseId, dto.Categories, out error);

                WriteEventRecord(CurrentUserId, CommonEnums.eUserEvents.COURSE_PUBLISHED,sessionId,null,null,dto.CourseId);

                _facebookServices.CreateUserFbStory((int) authorId, dto.CourseId, FbEnums.eFbActions.publish_course);

                entity.FbObjectPublished = true;

                return CourseRepository.UnitOfWork.CommitAndRefreshChanges(out error) && SaveCourseCategories(dto.CourseId,dto.Categories,out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course dto", dto.CourseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool SaveBundle(ref BundleEditDTO dto, int? authorId, out string error)
        {
            if (authorId == null)
            {
                error = "authorId missing";
                return false;
            }

            if (!IsBundleNameValid(dto, (int)authorId, out error))
            {
                error = string.IsNullOrEmpty(error) ? "Bundle Name already exists" : error;
                return false;
            }

            try
            {
                CRS_Bundles entity;

                if (dto.BundleId < 0) //new course
                {
                    entity = dto.EditDto2BundleEntity((int)authorId);
                    BundleRepository.Add(entity);
                }
                else
                {
                    entity = BundleRepository.GetById(dto.BundleId);

                    if (entity == null)
                    {
                        error = "Bundle entity not found";
                        return false;
                    }

                    entity.UpdateBundleEntity(dto);
                }

                if (!CourseRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                //add role
                AddRole2User((int)authorId, CommonEnums.UserRoles.Author);

                dto.BundleId = entity.BundleId;

                if (dto.Status == CourseEnums.CourseStatus.Published && !entity.FbObjectPublished)
                {
                    //FbServices.CreateUserFbStory((int) authorId, dto.CourseId, FbEnums.eFbActions.publish_course);

                    entity.FbObjectPublished = true;
                }

                return BundleRepository.UnitOfWork.CommitAndRefreshChanges(out error) && SaveBundleCategories(dto.BundleId, dto.Categories, out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save bundle dto", dto.BundleId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        /// <summary>
        /// save course categories on course save event , also 4 wizard
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="error"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public bool SaveCourseCategories(int courseId, List<int> categories, out string error)
        {
            error = string.Empty;
            try
            {
                //delete current
                CourseCategoryRepository.Delete(x => x.CourseId == courseId);

                //save new
                foreach (var categoryId in categories)
                {
                    CourseCategoryRepository.Add(( categoryId ).CategoryId2CourseCategoryEntity(courseId));
                }

                CourseCategoryRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        public bool SaveBundleCategories(int bundleId, List<int> categories, out string error)
        {
            error = string.Empty;
            try
            {
                //delete current
                BundleCategoryRepository.Delete(x => x.BundleId == bundleId);

                //save new
                foreach (var categoryId in categories)
                {
                    BundleCategoryRepository.Add((categoryId).CategoryId2BundleCategoryEntity(bundleId));
                }

                BundleCategoryRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        /// <summary>
        /// delete course by courseId
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool DeleteCourse(int courseId, out string error)
        {
            try
            {
                var entity = CourseRepository.GetById(courseId);

                if (entity == null)
                {
                    error = "Course entity not found";
                    return false;
                }

                var isPurchased = IsCoursePurchased(courseId);

                if (isPurchased)
                {
                    error = "Course already purchased and can't be deleted";
                    return false;
                }

                CourseRepository.Delete(entity);

                return CourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete course", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }
        public bool DeleteBundle(int bundleId, out string error)
        {
            try
            {
                var entity = BundleRepository.GetById(bundleId);

                if (entity == null)
                {
                    error = "Bundle entity not found";
                    return false;
                }

                var isPurchased = IsBundlePurchased(bundleId);

                if (isPurchased)
                {
                    error = "Bundle already purchased and can't be deleted";
                    return false;
                }

                BundleRepository.Delete(entity);

                return BundleRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete bundle", bundleId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }
        public bool IsBundlePurchased(int bundleId)
        {
            return UserBundleRepository.IsAny(x => x.BundleId == bundleId);
        }
        public bool IsCoursePurchased(int curseId)
        {
            return UserCourseRepository.IsAny(x => x.CourseId == curseId);
        }

        #region bundle courses
        public List<BundleCourseListDTO> GetBundleCourses(int bundleId)
        {
            try
            {
                return BundleCourseRepository.GetBundleCourses(bundleId).Where(x=>x.IsActive).Select(x => x.Entity2BundleCourseListDTO(string.Empty)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get bundle courses list", bundleId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<BundleCourseListDTO>();
            }
        }

        public List<BundleCourseListDTO> GetAvailableCourses4Bundle(int bundleId)
        {
            try
            {
                return BundleCourseRepository.GetAvailableCourses4Bundle(bundleId).Select(x => x.Entity2BundleCourseListDTO()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get available bundle courses list", bundleId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<BundleCourseListDTO>();
            }
        }

        public bool AddCourse2Bundle(int bundleId, int courseId, out string error)
        {            
            try
            {
                var ind = BundleCourseRepository.Count(x => x.BundleId == bundleId && x.IsActive);

                return SaveBundleCourse(bundleId,courseId,(short)ind,out error) && AddCourse2UserBundles(bundleId, courseId, out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save bundle course::" + bundleId + "::courseId::" + courseId, ex, bundleId, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool RemoveCourseFromBundle(int rowId, out string error)
        {
            try
            {
                var entity = BundleCourseRepository.GetById(rowId);
                if (entity == null)
                {
                    error = "entity not found";
                    return false;
                }

                var bundleId = entity.BundleId;
                var courseId = entity.CourseId;

                return UpdateBundleCourseStatus(bundleId, courseId, false, out error) && UpdateBundleCourseOrderIndex(bundleId, courseId, -1, out error) && RemoveCourseFromUserBundles(bundleId, courseId, out  error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("remove course from bundle", ex, rowId, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        //public bool SaveBundleCourses(int bundleId, List<int> courseIds, out string error)
        //{
        //    try
        //    {
        //        var current = BundleCourseRepository.GetBundleCourses(bundleId).Where(x=>x.IsActive).ToList();

        //        var currentIds = current.Select(x => x.CourseId).ToList();

        //        var toBeDeleted = currentIds.Except(courseIds).ToList();

        //        foreach (var id in toBeDeleted)
        //        {
        //            var updated = UpdateBundleCourseStatus(bundleId, id, false, out error) && UpdateBundleCourseOrderIndex(bundleId, id, -1, out error);
        //        }

        //        var toBeCreated = courseIds.Except(currentIds).ToList();

        //        var i = (short) (currentIds.Count-toBeDeleted.Count);
               
        //        foreach (var id in toBeCreated)
        //        {
        //            var add = SaveBundleCourse(bundleId, id, i, out error);
        //            if (!add) return false;
        //            i++;
        //        }

        //        //reorder current state
        //        var rowIds = BundleCourseRepository.GetBundleCourses(bundleId).Where(x=>x.IsActive).OrderBy(x=>x.OrderIndex).Select(x=>x.BundleCourseId).ToArray();
                
        //        return SaveBundleCoursesOrder(rowIds,out error);
        //    }
        //    catch (Exception ex)
        //    {
        //        error = Utils.FormatError(ex);
        //        Logger.Error("save bundle courses", ex, bundleId, CommonEnums.LoggerObjectTypes.Course);
        //        return false;
        //    }
        //}

        public bool SaveBundleCoursesOrder(int[] rowsIds, out string error)
        {
            error = string.Empty;

            try
            {
                short i = 0;
                foreach (var id in rowsIds)
                {
                    var updated = UpdateBundleCourseOrderIndex(id, i, out error);
                    if (!updated) return false;
                    i++;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save bundle courses order", null, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        private bool AddCourse2UserBundles(int bundleId, int courseId, out string error)
        {
            error = string.Empty;
            try
            {
                var userBundles = UserBundleRepository.GetMany(x => x.BundleId == bundleId && x.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE).ToList();

                foreach (var userBundle in userBundles)
                {
                    AttachCourse2User(courseId,userBundle.OrderLineId,userBundle.UserId,userBundle.UserBundleId,null,out error);
                }
               
                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("add course to user bundles::" + bundleId + "::courseId::" + courseId, ex, bundleId, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        private bool RemoveCourseFromUserBundles(int bundleId, int courseId, out string error)
        {
            try
            {
                var userBundles = UserBundleRepository.GetMany(x => x.BundleId == bundleId && x.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE).ToList();

                foreach (var userBundle in userBundles)
                {
                    var bundle = userBundle;
                    var userCourseEntity = UserCourseRepository.Get(x=>x.CourseId==courseId && x.UserId==bundle.UserId && (x.UserBundleId != null && x.UserBundleId==bundle.UserBundleId));

                    if(userCourseEntity==null) continue;
                    
                    userCourseEntity.StatusId = (byte)BillingEnums.eAccessStatuses.CANCELED;
                    userCourseEntity.UpdateDate = DateTime.Now;
                    if (CurrentUserId > 0) userCourseEntity.UpdatedBy = CurrentUserId;                    
                }

                UserCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("add course to user bundles::" + bundleId + "::courseId::" + courseId, ex, bundleId, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        private bool SaveBundleCourse(int bundleId, int courseId, short index, out string error)
        {
            try
            {
                var entity = BundleCourseRepository.Get(x => x.BundleId == bundleId && x.CourseId == courseId);

                if (entity != null) return UpdateBundleCourseStatus(bundleId, courseId, true, out error) && UpdateBundleCourseOrderIndex(bundleId, courseId, index, out error);

                BundleCourseRepository.Add(bundleId.NewBundleCourseEntity(courseId, index));

                return BundleCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save bundle course", ex, bundleId, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }
        private bool UpdateBundleCourseStatus(int bundleId, int courseId, bool isActive, out string error)
        {
            try
            {
                var entity = BundleCourseRepository.Get(x => x.BundleId == bundleId && x.CourseId == courseId);

                if (entity == null)
                {
                    error = "bundle course entity not found";
                    return false;
                }

                entity.UpdateBundleCourseEntityStatus(isActive);                

                return BundleCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save bundle course", ex, bundleId, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }
        private bool UpdateBundleCourseOrderIndex(int bundleId, int courseId, short index, out string error)
        {
            try
            {
                var entity = BundleCourseRepository.Get(x=>x.BundleId==bundleId && x.CourseId == courseId);

                if (entity == null)
                {
                    error = "bundle course entity not found";
                    return false;
                }

                return UpdateBundleCourseOrderIndex(entity.BundleCourseId, index, out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("update bundle course order index", bundleId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }
        private bool UpdateBundleCourseOrderIndex(int rowId, short index,out string error)
        {
            try
            {
                var entity = BundleCourseRepository.GetById(rowId);

                if (entity == null)
                {
                    error = "bundle course entity not found";
                    return false;
                }

                entity.UpdateBundleCourseEntityOrderIndex(index);

                return BundleCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("update bundle course order index", rowId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        } 
        #endregion
        #endregion

        #region course price

        /// <summary>
        /// get price dto for course tab manage
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="currencyId"></param>
        /// <returns></returns>
        public CoursePriceDTO GetCoursePrice(int courseId, short currencyId)
        {
            var entity = CourseRepository.GetById(courseId);

            if(entity==null) return new CoursePriceDTO
                {
                    CourseId = courseId
                };

            return entity.Entity2CoursePriceDTO(GetCurrencyDto(currencyId));
        }

        public List<PriceLineDTO> GetItemPriceLines(int itemId, BillingEnums.ePurchaseItemTypes type,short currencyId)
        {
            return GetItemPrices(itemId, type, currencyId);
        }

        public List<PriceLineDTO> GetAllItemPriceLines(int itemId, BillingEnums.ePurchaseItemTypes type, bool excludeFree)
        {
            return GetAllItemPrices(itemId, type).Where(x=>!excludeFree ||  x.PriceType != BillingEnums.ePricingTypes.FREE).ToList();
        }

        
        //public bool SaveCoursePrice(CoursePriceDTO dto, out string error)
        //{
        //    try
        //    {
        //        var entity = CourseRepository.GetById(dto.CourseId);

        //        if (entity == null)
        //        {
        //            error = "Course entity not found";
        //            return false;
        //        }

        //        entity.UpdateCourseFreePrice(dto);

        //        if(!CourseRepository.UnitOfWork.CommitAndRefreshChanges(out  error)) return false;

        //        if (!entity.IsFreeCourse) return DeleteCourseFreePriceLine(dto.CourseId,out error);

        //        DeleteCourseActivePriceLines(dto.CourseId);

        //        var token = new PriceLineDTO
        //        {
        //            ItemId     = dto.CourseId
        //            ,ItemType  = BillingEnums.ePurchaseItemTypes.COURSE
        //            ,PriceType = BillingEnums.ePricingTypes.FREE
        //            ,Price     = 0
        //            ,Currency  = new BaseCurrencyDTO { CurrencyId = DEFAULT_CURRENCY_ID}
        //        };

        //        return SavePriceLine(token,out error);
        //    }
        //    catch (Exception ex)
        //    {
        //        error = Utils.FormatError(ex);
        //        Logger.Error("save course price", dto.CourseId, ex, CommonEnums.LoggerObjectTypes.Course);
        //        return false;
        //    }
        //}

        public bool SaveCourseFreePrice(BaseItemToken item, bool isFree, out string error)
        {
            try
            {
                var courseId = item.ItemId;

                var entity = CourseRepository.GetById(courseId);

                if (entity == null)
                {
                    error = "Course entity not found";
                    return false;
                }

                entity.UpdateCourseFreePrice(isFree);

                if(!CourseRepository.UnitOfWork.CommitAndRefreshChanges(out  error)) return false;

                if (!entity.IsFreeCourse) return DeleteCourseFreePriceLine(courseId,out error);

                DeleteCourseActivePriceLines(courseId);

                var token = new PriceLineDTO
                {
                    ItemId     = courseId
                    ,ItemType  = BillingEnums.ePurchaseItemTypes.COURSE
                    ,PriceType = BillingEnums.ePricingTypes.FREE
                    ,Price     = 0
                    ,Currency  = new BaseCurrencyDTO { CurrencyId = DEFAULT_CURRENCY_ID}
                };

                return SavePriceLine(token,out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course free price", item.ItemId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool SaveItemAffiliateCommission(ItemAffiliateCommissionDTO dto, out string error)
        {
            try
            {
                switch (dto.ItemType)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        var courseEntity = CourseRepository.GetById(dto.ItemId);

                        if (courseEntity == null)
                        {
                            error = "Course entity not found";
                            return false;
                        }

                        courseEntity.UpdateCourseAffiliateCommission(dto.AffiliateCommission);

                        return CourseRepository.UnitOfWork.CommitAndRefreshChanges(out  error);
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        var bundleEntity = BundleRepository.GetById(dto.ItemId);

                        if (bundleEntity == null)
                        {
                            error = "Bundle entity not found";
                            return false;
                        }

                        bundleEntity.UpdateBundleAffiliateCommission(dto.AffiliateCommission);

                        return BundleRepository.UnitOfWork.CommitAndRefreshChanges(out  error);
                    default:
                        error = "Unknown type";
                        return false;
                }

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save item affiliate commission", dto.ItemId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool SavePriceLine(PriceLineDTO dto, out string error)
        {
            try
            {
                if (dto.PriceLineID >= 0) return UpdatePriceLine(dto.PriceLineID, dto.Price, out error);

                if (!IsPriceLineValid(dto, out error)) return false;

                var entity = dto.Token2PriceLineEntity();

                PriceListRepository.Add(entity);

                if(!PriceListRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                SavePriceLineRevision(entity.PriceLineId,entity.Price,ePriceRevisionAction.New);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course price line", dto.ItemId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool UpdatePriceLine(int lineId, decimal price, out string error)
        {
            try
            {
                var entity = PriceListRepository.GetById(lineId);

                if (entity == null)
                {
                    error = "Price entity not found";
                    return false;
                }

                entity.UpdatePriceLine(price,null);

                if(!PriceListRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                SavePriceLineRevision(entity.PriceLineId, entity.Price, ePriceRevisionAction.Update);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("update price line",lineId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        private void DeleteCourseActivePriceLines(int courseId)
        {
            var prices = PriceListRepository.GetMany(x=>x.ItemId==courseId && x.ItemTypeId==(byte)BillingEnums.ePurchaseItemTypes.COURSE && !x.IsDeleted).ToList();
            foreach (var price in prices)
            {
                string error;
                DeletePriceLine(price.PriceLineId, out error);
            }
        }

        public bool DeletePriceLine(int lineId,out string error)
        {
            try
            {
                var entity = PriceListRepository.GetById(lineId);

                if (entity == null)
                {
                    error = "Price entity not found";
                    return false;
                }

                var type = Utils.ParseEnum<BillingEnums.ePricingTypes>(entity.PriceTypeId);

                if (type == BillingEnums.ePricingTypes.FREE)
                    return SaveCourseFreePrice(new BaseItemToken{ItemId = entity.ItemId,ItemType = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(entity.ItemTypeId)}, false, out error);


                entity.UpdatePriceLine(null, true);
                
                if (!PriceListRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                SavePriceLineRevision(entity.PriceLineId, entity.Price, ePriceRevisionAction.Delete);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("update price line", lineId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }
        private bool DeleteCourseFreePriceLine(int itemId, out string error)
        {
            error = string.Empty;

            try
            {
                var entity = PriceListRepository.Get(x => x.ItemId == itemId && x.ItemTypeId == (byte)BillingEnums.ePurchaseItemTypes.COURSE && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.FREE && !x.IsDeleted);

                if (entity == null) return true;

                entity.UpdatePriceLine(null, true);

                if (!PriceListRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                SavePriceLineRevision(entity.PriceLineId, entity.Price, ePriceRevisionAction.Delete);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("update free price line", itemId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }
        public BundlePriceDTO GetBundlePrice(int courseId, short currencyId)
        {
            var entity = BundleRepository.GetById(courseId);

            if (entity == null) return new BundlePriceDTO
                {
                    BundleId = courseId
                };

            return entity.Entity2BundlePriceDTO(GetCurrencyDto(currencyId));
        }

        public bool SaveBundlePrice(BundlePriceDTO dto, out string error)
        {
            try
            {
                var entity = BundleRepository.GetById(dto.BundleId);

                if (entity == null)
                {
                    error = "bundle entity not found";
                    return false;
                }

                entity.UpdateBundleEntityPrice(dto);

                return CourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save bundle price", dto.BundleId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        #endregion

        #region course report page services

        /// <summary>
        /// get author courses
        /// </summary>
        /// <param name="currencyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CourseListDTO> GetAuthorCoursesList(short currencyId,int userId)
        {
            return CourseRepository.GetUserCourses(currencyId, userId).Select(x => x.Entity2CourseListDTO(GetItemDefaultPriceName(x.Id,BillingEnums.ePurchaseItemTypes.COURSE, currencyId,x.IsFreeCourse))).OrderByDescending(x => x.AddOn).ToList();
        }

        /// <summary>
        /// get author bundles for report
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<BundleListDTO> GetAuthorBundlesList(int userId)
        {
            return BundleRepository.GetAuthorBundles(userId).Select(x => x.Entity2BundleListDTO()).OrderByDescending(x => x.AddOn).ToList();
        }

        /// <summary>
        /// get author reviews
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="periodKind"></param>
        /// <returns></returns>
        public List<ReviewDTO> GetAuthorReviews(int userId, ReportEnums.ePeriodSelectionKinds? periodKind)
        {
            return _GetAuthorReviews(userId, periodKind).ToList();
        }
        #endregion        

        #region chapters
        /// <summary>
        /// get course chapters for content tab list
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public List<BaseListDTO> GetCourseChapters(int courseId)
        {
            try
            {
                return ChapterRepository.GetMany(x => x.CourseId == courseId).Select(x => x.ChapterEntity2ListDTO()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get course chapters", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<BaseListDTO>();  
            }
        }

        public List<BaseListDTO> GetCourseChapters(Guid uid)
        {
            try
            {
                return ChapterRepository.GetCourseChaptersByUid(uid).Select(x => x.ChapterEntity2ListDTO()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get course chapters by uid", uid, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<BaseListDTO>();
            }
        }

        public List<ChapterEditDTO> GetCourseEidtChaptersList(int courseId)
        {
            try
            {
                return ChapterRepository.GetMany(x => x.CourseId == courseId).Select(x => x.ChapterEntity2ChapterEditDTO()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get course edit chapters", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<ChapterEditDTO>();
            }
        }

        public List<CourseContentToken> GetCourseContentsList(int courseId)
        {
            try
            {
                var chapters = GetCourseEidtChaptersList(courseId).OrderBy(x => x.OrderIndex).Select(x => x.ChapterToken2ContentToken()).ToList();
                
                //TODO

                var quizzes = _quizAdminServices.GetCourseQuizzes(courseId).Where(x => x.IsAttached).OrderBy(x => x.AvailableAfter).ThenBy(x => x.Sid).Select(x => x.QuizToken2ContentToken()).ToList();


                foreach (var content in quizzes)
                {
                    var quiz = content.Quiz;

                    if (quiz.AvailableAfter != null)
                    {

                        var index = chapters.FindIndex(a => a.Kind == CourseEnums.eCourseContentKind.Chapter && a.Chapter.OrderIndex == ((int)quiz.AvailableAfter - 1));

                        index = index < 0 ? chapters.Count - 1 : index + 1;

                        chapters.Insert(index, content);
                    }
                    else
                    {
                        chapters.Insert(chapters.Count - 1, content);
                    }
                }

                return chapters;

            }
            catch (Exception ex)
            {
                Logger.Error("get course contents", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<CourseContentToken>();
            }
        }

        public bool IsAnyCourseContentsCreated(int courseId)
        {
            return courseId >= 0 && CourseRepository.GetCourseContentsCount(courseId) > 0;
        }

        /// <summary>
        /// get chapter token for editing
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public ChapterEditDTO GetChapterEditDTO(int chapterId, int courseId)
        {
            if (chapterId < 0)
            {
               return new ChapterEditDTO(courseId)
                   {
                       OrderIndex = GetCourseChapters(courseId).Count
                   };
            }

            var entity = ChapterRepository.GetById(chapterId);
            
           return entity != null ? entity.ChapterEntity2ChapterEditDTO() : null;
        }

        /// <summary>
        /// save chapter
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SaveChapter(ref ChapterEditDTO dto, out string error)
        {
            if (dto.CourseId < 0)
            {
                error = "courseId missing";
                return false;
            }

            if (!IsChapterNameValid(dto, out error))
            {
                error = string.IsNullOrEmpty(error) ? "Chapter Name already exists" : error;
                return false;
            }

            try
            {
                if (dto.OrderIndex == null || dto.OrderIndex < 0)
                {
                    dto.OrderIndex = GetCourseChapters(dto.CourseId).Count;
                }

                if (dto.ChapterId < 0) //new chapter
                {
                    var newEntity = dto.EditDto2CourseChapterEntity();
                    ChapterRepository.Add(newEntity);
                    ChapterRepository.UnitOfWork.CommitAndRefreshChanges();
                    dto.ChapterId = newEntity.Id;
                    return true;
                    
                }
                
                var entity = ChapterRepository.GetById(dto.ChapterId);

                if (entity == null)
                {
                    error = "Chapter entity not found";
                    return false;
                }

                entity.UpdateChapterEntity(dto);

                ChapterRepository.UnitOfWork.CommitAndRefreshChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save chapter dto", dto.CourseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool RenameChapter(ChapterEditDTO dto, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = ChapterRepository.GetById(dto.ChapterId);

                if (entity == null)
                {
                    error = "Chapter entity not found";
                    return false;
                }

                entity.UpdateChapterEntity(dto);

                ChapterRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("rename chapter", dto.ChapterId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool RenameChapter(WizardChapterListEditDTO dto, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = ChapterRepository.GetById(dto.ChapterId);

                if (entity == null)
                {
                    error = "Chapter entity not found";
                    return false;
                }

                entity.UpdateChapterEntity(dto);

                ChapterRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("rename chapter", dto.ChapterId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        /// <summary>
        /// delete chapter
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool DeleteChapter(int chapterId, out string error)
        {
            error = string.Empty;

            try
            {
                var entity = ChapterRepository.GetById(chapterId);

                if (entity == null)
                {
                    error = "Chapter entity not found";
                    return false;
                }

                var courseId = entity.CourseId;

                ChapterRepository.Delete(entity);

                ChapterRepository.UnitOfWork.CommitAndRefreshChanges();

                UpdateCourseChapterIndieces(courseId);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete chapter", chapterId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        /// <summary>
        /// save chapter indices on client reorder
        /// </summary>
        /// <param name="chapterIds"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SaveChaptersOrder(int[] chapterIds, out string error)
        {
            error = string.Empty;

            try
            {
                var i = 0;
                foreach (var chapterId in chapterIds)
                {
                    var entity = ChapterRepository.GetById(chapterId);

                    if (entity == null) continue;

                    entity.UpdateChapterEntityOrderIndex(i);

                    i++;
                }

                ChapterRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("save chapter order", null, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool SaveContentsOrder(ContentSortToken[] tokens, out string error)
        {
            error = string.Empty;

            try
            {
                var i = 0;
                foreach (var token in tokens)
                {
                    switch (token.kind)
                    {
                        case CourseEnums.eCourseContentKind.Chapter:
                             var entity = ChapterRepository.GetById(token.id);

                            if (entity == null) continue;

                            entity.UpdateChapterEntityOrderIndex(i);
                            if (!ChapterRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                            i++;
                            break;
                        case CourseEnums.eCourseContentKind.Quiz:
                            var token1 = token;
                            var quizEntity = CourseQuizzesRepository.Get(x=>x.Sid == token1.id);

                            if (quizEntity == null) continue;

                            quizEntity.UpdateCourseQuizAvailability((short)(i));
                            if (!CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;
                            break;
                    }

                   
                }
                
                return true;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("save content order", null, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        #endregion

        #region chapter videos
        /// <summary>
        /// get chapter videos for content tab list
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        public List<VideoListDto> GetChapterVideos(int chapterId)
        {
            try
            {
                return ChapterVideoRepository.GetMany(x => x.CourseChapterId == chapterId).Select(x => x.ChapterVideoEntity2ListDTO()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get chapter videos", chapterId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<VideoListDto>();  
            }
        }

        public List<ChapterVideoEditDTO> GetChapterEditVideosList(int chapterId)
        {
            try
            {
                var list = ChapterVideoRepository.GetMany(x => x.CourseChapterId == chapterId).Select(x => x.ChapterVideoEntity2VideoEditDTO()).ToList();

                foreach (var token in list)
                {
                    if (token.VideoIdentifier == null) continue;

                    token.VideoToken = _GetVideoToken((long)token.VideoIdentifier, CurrentUserId);//videoToken.BrightcoveVideo2VideoDTO(CurrentUserId, CurrentUserId.UserId2Tag(),_GetVideoChapterUsage((long) token.VideoIdentifier));
                }

                return list;
            }
            catch (Exception ex)
            {
                Logger.Error("get chapter edit videos list", chapterId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<ChapterVideoEditDTO>();
            }
        }

        /// <summary>
        /// get chapter video token for editing
        /// </summary>
        /// <param name="videoId"></param>
        /// <param name="chapterId"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        public ChapterVideoEditDTO GetChapterVideoEditDTO(int videoId, int chapterId,int authorId)
        {
            if (videoId < 0)
            {
               return new ChapterVideoEditDTO(chapterId)
                   {
                       OrderIndex = GetChapterVideos(chapterId).Count
                   };
            }

            var entity = ChapterVideoRepository.GetById(videoId);

            if (entity == null) return new ChapterVideoEditDTO(chapterId)
                                            {
                                                OrderIndex = GetChapterVideos(chapterId).Count
                                            };

            var token = entity.ChapterVideoEntity2VideoEditDTO();

            //set VideoThumbUrl
            if (token.VideoIdentifier == null) return token;

            token.VideoToken = _GetVideoToken((long)token.VideoIdentifier, authorId);

            //var videoToken = _brightcoveWrapper.FindVideoById((long)token.VideoIdentifier);

            //if (videoToken != null) token.VideoToken = videoToken.BrightcoveVideo2VideoDTO(authorId, authorId.UserId2Tag(),_GetVideoChapterUsage((long) token.VideoIdentifier));

            return token;
        }

        /// <summary>
        /// save chapter video
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SaveChapterVideo(ref ChapterVideoEditDTO dto, out string error)
        {
            return SaveChapterVideoDTO(ref dto, out error);
        }

        /// <summary>
        /// delete chapter video
        /// </summary>
        /// <param name="chapterVideoId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool DeleteChapterVideo(int chapterVideoId, out string error)
        {
            error = string.Empty;

            try
            {
                var entity = ChapterVideoRepository.GetById(chapterVideoId);

                if (entity == null)
                {
                    error = "entity not found";
                    return false;
                }

                var chapterId = entity.CourseChapterId;

                ChapterVideoRepository.Delete(entity);

                ChapterVideoRepository.UnitOfWork.CommitAndRefreshChanges();

                UpdateChapterVideoIndieces(chapterId);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete chapter video", chapterVideoId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        /// <summary>
        /// save chapter video indices on client reorder
        /// </summary>
        /// <param name="videoIds"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SaveChapterVideosOrder(int[] videoIds, out string error)
        {
            error = string.Empty;

            try
            {
                var i = 0;
                foreach (var videoId in videoIds)
                {
                    var entity = ChapterVideoRepository.GetById(videoId);

                    if (entity == null) continue;

                    entity.UpdateChapterVideoEntityOrderIndex(i);

                    i++;
                }

                ChapterVideoRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save chapter videos order", null, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }
        #endregion

        #region chapter links
        /// <summary>
        /// get chapter links for content tab list
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        public List<LinkListDto> GetChapterLinks(int chapterId)
        {
            try
            {
                return ChapterLinkRepository.GetMany(x => x.CourseChapterId == chapterId).Select(x => x.ChapterLinkEntity2ListDTO()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get chapter links", chapterId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<LinkListDto>();  
            }
        }

        public List<ChapterLinkEditDTO> GetChapterEditLinksList(int chapterId)
        {
            try
            {
                return ChapterLinkRepository.GetMany(x => x.CourseChapterId == chapterId).Select(x => x.ChapterLinkEntity2LinkEditDTO()).OrderBy(x=>x.OrderIndex).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get chapter edit links  list", chapterId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<ChapterLinkEditDTO>();
            }
        }

        /// <summary>
        /// get chapter link token for editing
        /// </summary>
        /// <param name="linkId"></param>
        /// <param name="chapterId"></param>
        /// <param name="kind">link type</param>
        /// <returns></returns>
        public ChapterLinkEditDTO GetChapterLinkEditDTO(int linkId, int chapterId, CourseEnums.eChapterLinkKind kind)
        {
            if (linkId < 0)
            {
               return new ChapterLinkEditDTO(chapterId,kind)
                   {
                       OrderIndex = GetChapterLinks(chapterId).Count
                   };
            }

            var entity = ChapterLinkRepository.GetById(linkId);

            return entity == null ? new ChapterLinkEditDTO(chapterId,kind){OrderIndex = GetChapterLinks(chapterId).Count} : entity.ChapterLinkEntity2LinkEditDTO();
        }

        /// <summary>
        /// save chapter link
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SaveChapterLink(ref ChapterLinkEditDTO dto, out string error)
        {
            return _SaveChapterLink(ref dto, out error);
        }

        /// <summary>
        /// delete chapter link
        /// </summary>
        /// <param name="chapterLinkId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool DeleteChapterLink(int chapterLinkId, out string error)
        {
            error = string.Empty;

            try
            {
                var entity = ChapterLinkRepository.GetById(chapterLinkId);

                if (entity == null)
                {
                    error = "entity not found";
                    return false;
                }

                var chapterId = entity.CourseChapterId;

                ChapterLinkRepository.Delete(entity);

                ChapterLinkRepository.UnitOfWork.CommitAndRefreshChanges();

                UpdateChapterLinksIndieces(chapterId);
                
                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete chapter link", chapterLinkId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        /// <summary>
        /// save chapter link indices on client reorder
        /// </summary>
        /// <param name="linkIds"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SaveChapterLinksOrder(int[] linkIds, out string error)
        {
            error = string.Empty;

            try
            {
                var i = 0;
                foreach (var linkId in linkIds)
                {
                    var entity = ChapterLinkRepository.GetById(linkId);

                    if (entity == null) continue;

                    entity.UpdateChapterLinkEntityOrderIndex(i);

                    i++;
                }

                ChapterLinkRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save chapter links order", null, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }        
        #endregion

        #region course review
        /// <summary>
        /// get course reviews for reviews tab in course manage
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public List<ReviewDTO> GetCourseReviews(int courseId)
        {
            try
            {
                var dates = PeriodSelection2DateRange(ReportEnums.ePeriodSelectionKinds.all);
                return CourseRepository.GetCourseReviews(courseId, dates.from, dates.to).Select(x => x.Entity2ReviewDTO()).OrderByDescending(x => x.Date).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get course reviews", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<ReviewDTO>();
            }
        }
        #endregion

        #region course/bundle sales
        public List<OrderLineDTO> GetCourseSales(int courseId, ReportEnums.ePeriodSelectionKinds periodKind, BillingEnums.eOrderLineTypes lineType)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);
                return SearchOrderLines(dates.@from, dates.to, sellerId: null, buyerId: null, storeOwnerId: null, courseId: courseId, bundleId: null, storeId: null, lineType: lineType).Select(x => x.Entity2OrderLineDto()).ToList();                   
            }
            catch (Exception ex)
            {
                Logger.Error("get course sales", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<OrderLineDTO>();
            }
        }

        public List<OrderLineDTO> GetBundleSales(int bundleId, ReportEnums.ePeriodSelectionKinds periodKind, BillingEnums.eOrderLineTypes lineType)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);
                return SearchOrderLines(dates.@from, dates.to, sellerId: null, buyerId: null, storeOwnerId: null, courseId: null, bundleId: bundleId, storeId: null, lineType: lineType).Select(x => x.Entity2OrderLineDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get bundle sales", bundleId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<OrderLineDTO>();
            }
        }
        #endregion

        #region course stores
        public List<TreeWebStoreDTO> GetCourseStoresTree(int courseId,int? ownerUserId,out string error)
        {
            error = string.Empty;
            try
            {
                var entity = CourseRepository.GetById(courseId);

                if (entity == null)
                {
                    error = "course entity not found";
                    return new List<TreeWebStoreDTO>();
                }

                var userId = ownerUserId == null ? entity.AuthorUserId : (int)ownerUserId;

                var list = new List<TreeWebStoreDTO>();

                var stores = WebStoreRepository.GetMany(x => x.OwnerUserID == userId);

                foreach (var store in stores)
                {
                    var storeDto = new TreeWebStoreDTO
                    {
                         Id         = store.StoreID
                        ,Name       = store.StoreName
                        ,TrackingID = store.TrackingID
                    };

                    var storeId = store.StoreID;
                    var categories = WebStoreCategoryRepository.GetMany(x => x.WebStoreID == storeId);

                    foreach (var category in categories)
                    {
                        var categoryId = category.WebStoreCategoryID;
                        var categoryDto = new TreeWebStoreCategoryDTO
                        {
                            Id        = category.WebStoreCategoryID
                            ,Name     = category.CategoryName
                            ,Attached = WebStoreItemRepository.IsAny(x => x.WebStoreCategoryID == categoryId && (x.CourseId != null && x.CourseId == courseId))                            
                        };
                        storeDto.Categories.Add(categoryDto);

                        if (categoryDto.Attached) storeDto.CourseIncluded = true;
                    }

                    list.Add(storeDto);
                }

                return list;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("get course stores tree", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<TreeWebStoreDTO>();
            }
        } 
        #endregion

        #endregion

        #region IAuthorAdminCategoryServices implementation
        /// <summary>
        /// get active categories LOV
        /// </summary>        
        /// <returns></returns>
        public List<CategoryViewDTO> ActiveCategories()
        {
            return CategoryRepository.GetMany(x => x.IsActive).Select(x => x.Entity2CategoryViewDTO()).OrderByDescending(x => x.index).ToList();
        }

        /// <summary>
        /// get categories LOV, using in WebStore Single Course Selection
        /// </summary>
        /// <returns></returns>
        public List<BaseListDTO> GetCategoriesLOV()
        {
            return CategoryRepository.GetCategoriesLOV().OrderByDescending(x => x.BundlesCnt).ThenByDescending(x=>x.CoursesCnt).Select(x => x.CategoryEntity2BaseListDto()).ToList();
        }

        /// <summary>
        /// get category courses LOV
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<BaseItemListDTO> GetCategoryItemsLOV(int categoryId)
        {
            return CategoryRepository.GetCategoryItems(categoryId).Select(x => x.BaseItemEntity2BaseItemListDtoWithTypePrefix()).ToList();
        }
        #endregion

        #region IWidgetCourseServices implementation
        #region private helpers
        /// <summary>
        /// Update course rating on user review save 
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool UpdateCourseRating(int courseId, out string error)
        {
            error = string.Empty;

            try
            {

                var entity = CourseRepository.GetById(courseId);

                if (entity == null)
                {
                    error = "course entity not found";
                    return false;
                }

                var average = UserCourseReviewsRepository.GetMany(x=>x.CourseId==courseId && x.Approved).ToList();
                if (average.Any())
                {
                    var r = average.Average(x => x.ReviewRating);
                    if (r != null)
                        entity.Rating = (int)r;
                }

                UserCourseReviewsRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("update course rating", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        private CRS_CourseToken FindCourseByUrlAndAuthor(short currencyId,string authorName, string urlName)
        {
            var courses = CourseRepository.FindCoursesByUrlName(currencyId,urlName).ToList();

            if (courses.Count.Equals(0)) return null;

            return courses.Count==1 ? courses[0] : courses.FirstOrDefault(x => x.Entity2AuthorFullName().OptimizedUrl() == authorName);
        }

        private CRS_Bundles FindBundleByUrlAndAuthor(string authorName, string urlName)
        {
            var bundles = BundleRepository.GetMany(x=>x.BundleUrlName==urlName).ToList();

            if (bundles.Count.Equals(0)) return null;

            return bundles.Count == 1 ? bundles[0] : bundles.FirstOrDefault(x => x.Users.Entity2FullName().OptimizedUrl() == authorName);
        }
        #endregion
        public int GetItemAuthor(int itemId, BillingEnums.ePurchaseItemTypes type)
        {
            try
            {
                switch (type)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE :
                        var entity = CourseRepository.GetById(itemId);
                        return entity?.AuthorUserId ?? -1;
                    case BillingEnums.ePurchaseItemTypes.BUNDLE :
                        var bundleEntity = BundleRepository.GetById(itemId);
                        return bundleEntity?.AuthorId ?? -1;
                    default: return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get item authorId", itemId, ex, CommonEnums.LoggerObjectTypes.UserCourse);
                return -1;
            }
        }


        public CourseInfoDTO GetCourseToken(short currencyId, int courseId, string trackingId)
        {
            return CourseRepository.GetCourseToken(currencyId,courseId).CourseToken2CourseInfoDto(trackingId);
        }

       
        public BundleInfoDTO GetBundleToken(short currencyId, int bundleId, string trackingId)
        {
            return BundleRepository.GetBundleInfo(currencyId,bundleId).BundleToken2BundleInfoDto(trackingId);
        }

        public BaseEntityDTO FindCourseByUrlName(string authorName, string urlName)
        {
            try
            {
                if (string.IsNullOrEmpty(urlName)) return null;

                //var entities = CourseRepository.GetMany(x => x.CourseUrlName == urlName.Trim()).ToList();

                //if (entities.Count().Equals(0)) return null;

                //if(entities.Count().Equals(1)) return entities[0].CourseEntity2BaseEntityDTO();
                
                //var query = (from course in CourseRepository.GetAll()
                //    join user in UserRepository.GetAll() on course.AuthorUserId equals user.Id
                //    where course.CourseUrlName == urlName.Trim()
                //    select new {author = user.Entity2FullName().OptimizedUrl(), course}).Where(
                //        x => x.author == authorName).Select(x => x.course).ToList();

                //return query.Count != 1 ? null : query[0].CourseEntity2BaseEntityDTO(); 

                var course = CourseRepository.FindCourseByUrlName(urlName, authorName.OptimizedUrl());

                return course?.CourseEntity2BaseEntityDTO();
            }
            catch (Exception ex)
            {
                Logger.Error("find course by url name " + urlName, null, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return null;
            }
        }

        public BaseBundleDTO FindBundleByUrlName(string authorName, string urlName)
        {
            try
            {
                if (string.IsNullOrEmpty(urlName)) return null;

                var entities = BundleRepository.GetMany(x => x.BundleUrlName == urlName.Trim()).ToList();

                if (entities.Count.Equals(0)) return null;

                if (entities.Count.Equals(1)) return entities[0].BundleEntity2BaseBundleDto();

                var query = (from course in BundleRepository.GetAll()
                             join user in UserRepository.GetAll() on course.AuthorId equals user.Id
                             where course.BundleUrlName == urlName.Trim()
                             select new { author = user.Entity2FullName().OptimizedUrl(), course }).Where(
                        x => x.author == authorName).Select(x => x.course).ToList();

                return query.Count != 1 ? null : query[0].BundleEntity2BaseBundleDto();
            }
            catch (Exception ex)
            {
                Logger.Error("find bundle by url name " + urlName, null, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return null;
            }
        }

        public ItemAccessStateToken GetCourseAccessState4User(int? userId,int id)
        {
            try
            {
                var course = CourseRepository.GetById(id);
                
                if(course==null) return new ItemAccessStateToken();
                
                var entities = UserCourseRepository.GetMany(x => x.UserId == userId 
                                                                && x.CourseId == id 
                                                               // && (x.ValidUntil == null || x.ValidUntil > DateTime.Now)
                                                                && (x.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE || x.StatusId == (byte)BillingEnums.eAccessStatuses.SUSPENDED)).ToList();

                var rental = entities.Where(x => x.ValidUntil != null).OrderByDescending(x=>x.ValidUntil).FirstOrDefault();


                var validUntil = rental?.ValidUntil;
                var isSubscriptionRest = false;
                
                if (validUntil != null)
                {
                    var line = OrderLinesViewRepository.Get(x => x.LineId == rental.OrderLineId);
                    isSubscriptionRest = line != null && line.LineTypeCode == BillingEnums.eOrderLineTypes.SUBSCRIPTION.ToString() && line.OrderStatusCode == BillingEnums.eOrderStatuses.SUSPENDED.ToString();
                }

                var utc = this.UtcDateTime();

                var statusUpdated = false;

                if (userId != null && validUntil != null && validUntil < utc)
                {
                    statusUpdated = true;
                    string error;
                    if (isSubscriptionRest)
                    {
                        var canceled =  SHARED_CancelOrder(rental.OrderLineId,out error) && SHARED_BlockUserCourseAccess(rental.OrderLineId,true, out error);
                        if (!canceled) SendAdminMail("Completion Subscription Cancel failed", "Completion Subscription Cancel failed",error);
                    }
                    else
                    {
                        OnCourseRentalFinished(id, (int) userId, out error);
                    }
                }

                //check again if active exists
                if (statusUpdated)
                {
                    entities = UserCourseRepository.GetMany(x => x.UserId == userId  && x.CourseId == id && (x.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE)).ToList();
                }

                var token = new ItemAccessStateToken
                {
                    IsOwnedByUser                 = userId != null && userId > 0 && course.AuthorUserId == userId
                    ,IsPublished                  = course.StatusId == (short)CourseEnums.CourseStatus.Published
                    ,IsAccessAllowed              = userId != null && userId > 0 && entities.Any(x=>x.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE) // && (validUntil==null || validUntil > utc)
                    ,ValidUntill                  = validUntil
                    ,MinutesRemind                = validUntil == null ? null : (int?)((validUntil - utc).HasValue ? (validUntil - utc).Value.TotalMinutes : (double?)null)
                    ,IsRestOfCanceledSubscription = isSubscriptionRest
                };


                return token;

            }
            catch (Exception ex)
            {
                Logger.Error("GetCourseAccessState4User - " + id + " -  allowed for user", userId ?? id, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return new ItemAccessStateToken();               
            }
        }

        public ItemAccessStateToken GetBundleAccessState4User(int? userId, int id)
        {
            try
            {
                var bundle = BundleRepository.GetById(id);

                if (bundle == null) return new ItemAccessStateToken();

                return new ItemAccessStateToken
                {
                     IsOwnedByUser   = userId != null && userId > 0 && bundle.AuthorId == userId
                    ,IsPublished     = bundle.StatusId == (short)CourseEnums.CourseStatus.Published
                    ,IsAccessAllowed = userId != null && userId > 0 &&UserBundleRepository.IsAny(x => x.UserId == userId && x.BundleId == id && x.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE)
                };

            }
            catch (Exception ex)
            {
                Logger.Error("if course - " + id + " -  allowed for user", userId ?? id, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return new ItemAccessStateToken();
            }
        }

        public ItemAccessStateToken GetItemAccessState4User(int? userId, int itemId,byte itemTypeId)
        {
            try
            {
                var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(itemTypeId);

                switch (type)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        return GetCourseAccessState4User(userId, itemId);
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        return GetBundleAccessState4User(userId, itemId);
                    default:
                        return new ItemAccessStateToken();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("get item access state", userId ?? itemId, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return new ItemAccessStateToken();
            }
        }

        public bool IsItemAccessAllowed4User(int? userId, int itemId,byte itemTypeId)
        {
            try
            {
                var type = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(itemTypeId);
                ItemAccessStateToken state;
                switch (type)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        state = GetCourseAccessState4User(userId, itemId);
                        break;
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        state =  GetBundleAccessState4User(userId, itemId);
                        break;
                    default:
                        return false;
                }

                return state.IsOwnedByUser || state.IsAccessAllowed;
            }
            catch (Exception ex)
            {
                Logger.Error("get item access state", userId ?? itemId, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return false;
            }
        }

        public bool OnCourseRentalFinished(int courseId, int userId, out string error)
        {            
            try
            {
                var entity = UserCourseRepository.GetMany(x => x.UserId == userId
                                                              && x.CourseId == courseId
                                                              && (x.ValidUntil != null || x.ValidUntil <= DateTime.Now)
                                                              && x.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE).FirstOrDefault();

                if (entity == null)
                {
                    error = "entity not found";
                    return false;
                }

                entity.StatusId = (byte)BillingEnums.eAccessStatuses.SUSPENDED;
                entity.UpdateDate = DateTime.Now;

                return CourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error($"on course rental finished:: userId -{userId}:: courseId - {courseId}", userId, ex, CommonEnums.LoggerObjectTypes.UserCourse);
                return false;
            }
        }

        //Course viewer

        /// <summary>
        /// get course dto for course viewer page
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="loadBasicInfo"></param>
        /// <returns></returns>
        public LearnerCourseViewerDTO GetLearnerCourseViewerDTO(int id, int userId,bool loadBasicInfo = false)
        {
            try
            {
                var entity = CourseRepository.GetById(id);

                if (entity == null) return null;

                var token = entity.CourseEntity2CourseViewDto();

                //TODO temp for initializing of course classrooms
                #region class room init

                if (token.ClassRoomId == null)
                {
                    var roomId = _FindAuthorClassRoom(token.CourseName, entity.AuthorUserId);
                    if (roomId == null)
                    { 
                        string error;
                        var room = new DiscussionClassRoomDTO
                        {
                            AuthorId = entity.AuthorUserId
                            ,Name = token.CourseName
                        };

                        _SaveClassRoom(ref room, entity.AuthorUserId, userId, out error);

                        if (room.RoomId >= 0)
                        {
                            entity.ClassRoomId = room.RoomId;
                            CourseRepository.UnitOfWork.CommitAndRefreshChanges();
                            token.ClassRoomId = room.RoomId;
                        }
                    }
                    else
                    {
                        entity.ClassRoomId = roomId;
                        CourseRepository.UnitOfWork.CommitAndRefreshChanges();
                        token.ClassRoomId = roomId;
                    }
                }
                #endregion

                token.AuthorUserBaseDto = _GetAuthorBaseDto(entity.AuthorUserId);

                if (loadBasicInfo) return token;

                token.Chapters = GetCourseChaptersList(id);

                token.VideosNavigation = token.Chapters.ChapterTreeList2VideoNavigation();

                var watchEntity = UserCourseWatchStateRepository.Get(x => x.UserId == userId && x.CourseId == id);

                if (watchEntity == null) return token;

                token.LastChapterId = watchEntity.LastChapterID ?? -1;
                token.LastVideoId = watchEntity.LastVideoID ?? -1;

                return token;
            }
            catch (Exception ex)
            {

                Logger.Error("get course view dto by id", id, ex, CommonEnums.LoggerObjectTypes.Course);

                return null;
            }
        }
        
        /// <summary>
        /// get course chapters tree list view
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public List<ContentTreeViewItemDTO> GetCourseChaptersList(int courseId)
        {
            try
            {
                var list = new List<ContentTreeViewItemDTO>();

                var chapters = ChapterRepository.GetMany(x => x.CourseId == courseId).OrderBy(x => x.ChapterOrdinal).ToList();

                foreach (var chapter in chapters)
                {
                    var ch = chapter;
                    var chapterVideos = ChapterVideoRepository.GetMany(x => x.CourseChapterId == ch.Id).OrderBy(x => x.Ordinal).ToList();

                    if (chapterVideos.Count.Equals(0)) continue;

                    var token = new ContentTreeViewItemDTO
                    {
                        id           = ch.Id
                        ,name        = ch.ChapterName
                        ,index       = ch.ChapterOrdinal
                        ,bcId        = null
                        ,type        = CourseEnums.eContentTreeViewItemType.chapter  
                        ,desc        = null
                        ,thumb       = null
                        ,videos      = new List<ContentTreeViewItemDTO>()
                    };

                    long chapterLength = 0;

                    foreach (var video in chapterVideos)
                    {
                        var bcId = Convert.ToInt64(video.VideoSupplierIdentifier);


                        var v = UserVideosRepository.Get(x => x.BcIdentifier == bcId); // _brightcoveWrapper.FindVideoById(bcId);

                        if (v?.UserId == null)
                        {
                            Logger.Warn($"Video {video.VideoSupplierIdentifier} for courseId {courseId}");
                            continue;
                        }

                        var vtoken = new ContentTreeViewItemDTO
                        {
                            id           = video.Id
                            ,name        = video.VideoTitle
                            ,bcId        = string.IsNullOrEmpty(video.VideoSupplierIdentifier) ? "-1" : video.VideoSupplierIdentifier
                            ,type        = CourseEnums.eContentTreeViewItemType.video
                            ,desc        = video.VideoSummary
                            ,thumb       = bcId.CombimeVideoUrl((int) v.UserId,CommonEnums.eVideoPictureTypes.Thumb).ToCloudfrontSignedUrl()
                            ,duration    = v.Length.VideoLength2Duration().Duration2HoursString()
                            ,IsOpen      = Convert.ToBoolean(video.IsOpen)
                        }; 

                        chapterLength += v.Length;

                        token.videos.Add(vtoken);

                        //#region future use local data
                        // var userVideo = UserVideosRepository.Get(x => x.BcIdentifier == bcId);

                        //if (userVideo != null)
                        //{

                        //    var vtoken = new ContentTreeViewItemDTO
                        //    {
                        //        id           = video.Id
                        //        ,name        = video.VideoTitle
                        //        ,bcId        = String.IsNullOrEmpty(video.VideoSupplierIdentifier) ? -1 : Int64.Parse(video.VideoSupplierIdentifier)
                        //        ,type        = CourseEnums.eContentTreeViewItemType.video
                        //        ,desc        = video.VideoSummary
                        //        ,thumb       = userVideo.ThumbUrl
                        //        ,duration    = userVideo.Duration
                        //        ,IsOpen      = Convert.ToBoolean(video.IsOpen)
                        //    };

                        //    chapterLength += userVideo.Duration.MinSecString2MilliSeconds();

                        //    token.videos.Add(vtoken);     
                        //}
                        //else
                        //{
                        //    var v = _brightcoveWrapper.FindVideoById(bcId);

                        //    if (v == null)
                        //    {
                        //        Logger.Warn(String.Format("Video {0} for courseId {1}", video.VideoSupplierIdentifier, courseId));
                        //        continue;
                        //    }

                        //    var vtoken = new ContentTreeViewItemDTO
                        //    {
                        //        id           = video.Id
                        //        ,name        = video.VideoTitle
                        //        ,bcId        = String.IsNullOrEmpty(video.VideoSupplierIdentifier) ? -1 : Int64.Parse(video.VideoSupplierIdentifier)
                        //        ,type        = CourseEnums.eContentTreeViewItemType.video
                        //        ,desc        = video.VideoSummary
                        //        ,thumb       = v.ThumbnailUrl
                        //        ,duration    = v.Length.VideoLength2Duration().Duration2HoursString()
                        //        ,IsOpen      = Convert.ToBoolean(video.IsOpen)
                        //    }; 

                        //    chapterLength += v.Length;

                        //    token.videos.Add(vtoken);    
                        //}        
                        //#endregion                 
                    }

                    token.duration = chapterLength.VideoLength2Duration().Duration2HoursString();

                    list.Add(token);
                }
                return list;
            }
            catch (Exception ex)
            {
                Logger.Error("get course chapters tree-view", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<ContentTreeViewItemDTO>();
            }
        }

        //public List<ContentTreeViewItemDTO> GetCourseG2TAssetsList(int courseId)
        //{
        //    try
        //    {
        //        var list = new List<ContentTreeViewItemDTO>();

        //        var chapters = CourseAssetsRepository.GetMany(x => x.CourseId == courseId && x.IsActive).OrderBy(x => x.OrderIndex).ToList();

        //        foreach (var chapter in chapters)
        //        {
        //            var ch = chapter;
                 
        //            if(ch.BcIdentifier == null) continue;

        //            int chapId;

        //            if (!Int32.TryParse(chapter.RefId, out chapId)) continue;

        //            var v = _brightcoveWrapper.FindVideoById((long) ch.BcIdentifier);

        //            if (v == null) continue;

        //            var vtoken = new ContentTreeViewItemDTO
        //            {
        //                id           = ch.AssetId
        //                ,name        = ch.Title
        //                ,bcId        = ch.BcIdentifier
        //                ,type        = CourseEnums.eContentTreeViewItemType.video
        //                ,desc        = ch.Description
        //                ,thumb       = v.ThumbnailUrl
        //                ,duration    = v.Length.VideoLength2Duration().Duration2HoursString()
        //                ,IsOpen      = false
        //            }; 
                    
        //            var token = new ContentTreeViewItemDTO
        //            {
        //                id           = chapId
        //                ,name        = ch.Title
        //                ,bcId        = null
        //                ,type        = CourseEnums.eContentTreeViewItemType.asset
        //                ,desc        = ch.Description
        //                ,thumb       = null
        //                ,videos      = new List<ContentTreeViewItemDTO>{vtoken}
        //                ,duration    = vtoken.duration
        //            };
                    

        //            list.Add(token);
        //        }
        //        return list;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("get course chapters tree-view", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
        //        return new List<ContentTreeViewItemDTO>();
        //    }
        //}


        /// <summary>
        /// get chapter links for course viewer
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public List<ChapterLinkListToken> GetChapterLinks(int chapterId, CourseEnums.eChapterLinkKind kind)
        {
            try
            {
                return ChapterLinkRepository.GetMany(x => x.CourseChapterId == chapterId && x.LinkType == (short)kind).Select(x => x.ChapterLinkEntity2LinkListToken()).Select(x=>new ChapterLinkListToken
                {
                    name   = x.name
                    ,url   = x.kind == CourseEnums.eChapterLinkKind.Link ?  x.url : x.url.ToS3SignedUrl(S3_BUCKET_NAME)
                    ,index = x.index
                    ,kind  = x.kind
                }).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get chapter links by type", chapterId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<ChapterLinkListToken>();
            }
        }

        //other learners
        public List<LearnerListItemDTO> GetOtherCourseLearners(int courseId, int userId)
        {
            try
            {
                var learners = UserCourseRepository.GetOtherLearners(userId, courseId);//

                var crsOtherLearnerTokens = learners as CRS_LearnerToken[] ?? learners.ToArray();
              
                return crsOtherLearnerTokens.Select(x => x.LearnerToken2LearnerListItemDTO()).ToList(); 
            }
            catch (Exception ex)
            {
                Logger.Error("get other course learners", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<LearnerListItemDTO>(); 
            }
        }

        #region review services
        
        /// <summary>
        /// get user review for edit review window
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public CourseUserReviewDTO GetUserCourseReview(int courseId,int userId)
        {
            try
            {
                var entity = UserCourseReviewsRepository.Get(x => x.CourseId == courseId && x.UserId == userId);

                return entity==null ? new CourseUserReviewDTO
                                        {
                                            CourseId = courseId
                                        } : entity.Entity2CourseUserReviewDTO();
            }
            catch (Exception ex)
            {
                Logger.Error("get user course review", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new CourseUserReviewDTO
                {
                    CourseId = courseId
                };
            }
        }

        /// <summary>
        /// save user course review
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="userId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SaveCourseReview(CourseUserReviewDTO dto,int userId, out string error)
        {
            if (dto.CourseId < 0)
            {
                error = "courseId missing";
                return false;
            }            
            try
            {
                UserCourseReviews entity;
                
                if (dto.ReviewId < 0) //new link
                {
                    entity = dto.Dto2CourseReviewEntity(userId);

                    UserCourseReviewsRepository.Add(entity);
                }
                else
                {
                    entity = UserCourseReviewsRepository.GetById(dto.ReviewId);

                    if (entity == null)
                    {
                        error = "Review entity not found";
                        return false;
                    }

                    entity.UpdateCourseReviewEntity(dto);
                }

                UserCourseReviewsRepository.UnitOfWork.CommitAndRefreshChanges();

                dto.ReviewId = entity.Id;

                var updated = UpdateCourseRating(dto.CourseId,out error);

                if (!updated) return false;

                SaveReviewPostsAndMails(dto, userId);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course review", dto.CourseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        private void SaveReviewPostsAndMails(CourseUserReviewDTO dto, int userId)
        {
            try
            {
                string error;

                //save writer story
               _facebookServices.CreateUserFbStory(userId,dto.CourseId,FbEnums.eFbActions.review);

                //author
               var token = CourseRepository.GetAuthorReviewMessageToken(dto.ReviewId).AuthorDto2ReviewMessageDto();
               if (token != null)
               {
                   //email
                   _emailServices.SaveEmailReviewAuthorRecord(token);

                   //FB
                   PostMessageDTO postDto;
                   
                   if (token.Author.fbUid != null)
                   {
                       postDto = token.Token2AuthorPostMessageDto();

                       if (postDto != null) _facebookServices.SavePostMessage(postDto, out error);
                   }

                   //LFE FB Post
                   postDto = token.Token2AppPostMessageDto();
                   if (postDto != null) _facebookServices.SavePostMessage(postDto, out error);
               }

               //learners
               var learners = CourseRepository.GetLearnersReviewMessageToken(dto.ReviewId).Select(x => x.LearnerDto2ReviewMessageDto()).ToList();

               foreach (var learnerDto in learners)
               {
                   //email
                   _emailServices.SaveEmailReviewLearnerRecord(learnerDto);

                   //FB
                   var postDto = learnerDto.Token2LearnerPostMessageDto();

                   if (postDto == null) continue;

                   _facebookServices.SavePostMessage(postDto, out error);
               }

            }
            catch (Exception ex)
            {
                Logger.Error("save review posts and mails", dto.CourseId, ex, CommonEnums.LoggerObjectTypes.Course);                
            }
        }


       
        public int GetCourseRating(int courseId)
        {
            try
            {
                return CourseRepository.GetById(courseId).Rating ?? 0;
            }
            catch (Exception ex)
            {
                Logger.Error("get course rating", courseId, ex, CommonEnums.LoggerObjectTypes.Course);
                return 0;
            }
        }
        #endregion

        #region bundles
        public List<BundleCourseListDTO> GetBundleCoursesList(int bundleId, string trackingId = null)
        {
            try
            {
                var list = BundleCourseRepository.GetBundleCourses(bundleId).Where(x => x.IsActive && x.StatusId == (short)CourseEnums.CourseStatus.Published).Select(x => x.Entity2BundleCourseListDTO(trackingId)).ToList();

                return list;
            }
            catch (Exception ex)
            {
                Logger.Error("get bundle courses list", bundleId, ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<BundleCourseListDTO>();
            }
        }
        #endregion
        
        #region purchase services
        public ItemPurchaseDataToken GetCoursePurchaseDtoByCourseUrlName(short currencyId, string authorName, string courseUrlName)
        {
            try
            {
                if (string.IsNullOrEmpty(courseUrlName)) return null;

                var course = FindCourseByUrlAndAuthor(currencyId,authorName.TrimString(),courseUrlName.TrimString());

                return course?.CourseToken2ItemPurchaseDataToken();
            }
            catch (Exception ex)
            {
                Logger.Error("find course by author and url name " + courseUrlName, null, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return null;
            }
        }

        public ItemPurchaseDataToken GetBundlePurchaseDtoByBundleUrlName(short currencyId, string authorName, string bundleUrlName)
        {
            try
            {
                if (string.IsNullOrEmpty(bundleUrlName)) return null;

                var bundle = FindBundleByUrlAndAuthor(authorName.TrimString(), bundleUrlName.TrimString());

                return bundle?.BundleToken2ItemPurchaseDataToken(GetItemRegularPrice(bundle.BundleId, BillingEnums.ePurchaseItemTypes.BUNDLE, currencyId), GetItemMonthlySubscriptionPrice(bundle.BundleId, BillingEnums.ePurchaseItemTypes.BUNDLE, currencyId));
            }
            catch (Exception ex)
            {
                Logger.Error("find bundle by author and url name " + bundleUrlName, null, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return null;
            }
        }

        public ItemPurchaseDataToken GetItemPurchaseDtoByPriceLineId(int lineId,out string error)
        {
            error = string.Empty;
            try
            {
                var priceLineEntity = PriceListRepository.GetById(lineId);

                if (priceLineEntity == null)
                {
                    error = "Price list entity not found";
                    return null;
                }

                var priceToken = priceLineEntity.Entity2PriceLineDto(GetCurrencyDto(priceLineEntity.CurrencyId));

                switch (priceToken.ItemType)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        var course = GetCourseToken(priceToken.Currency.CurrencyId,priceToken.ItemId,null);
                        
                        if(course == null) return null;
                        
                        priceToken.IsItemUnderRGP = priceToken.PriceType == BillingEnums.ePricingTypes.ONE_TIME && IsUnderRGP(course.Author.UserId);
                        
                        return course.CourseInfoDTO2ItemPurchaseDataToken(priceToken);
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        var bundle = GetBundleToken(priceToken.Currency.CurrencyId, priceToken.ItemId,null);
                        
                        if (bundle == null) return null;
    
                        priceToken.IsItemUnderRGP = priceToken.PriceType == BillingEnums.ePricingTypes.ONE_TIME && IsUnderRGP(bundle.Author.UserId);

                        return bundle.BundleInfoDTO2ItemPurchaseDataToken(priceToken);                    
                    default:
                        error = "Unknown Item type";
                        return null;
                }
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);

                Logger.Error("find item by price lineId " + lineId,lineId, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return null;
            }
        }

        public ItemPurchaseCompleteToken GetItemPurchaseCompleteToken(int orderNo, out string error)
        {
            error = string.Empty;
            try
            {
                var orderLineEntity = OrderLinesViewRepository.GetMany(x=>x.OrderNumber == orderNo).FirstOrDefault();

                if (orderLineEntity == null)
                {
                    error = "Order line entity not found";
                    return null;
                }

                var paymentMethod = Utils.ParseEnum<BillingEnums.ePaymentMethods>(orderLineEntity.PaymentMethodId);

                if (paymentMethod != BillingEnums.ePaymentMethods.Charge_Free)
                {
                    if (orderLineEntity.PriceLineId == null)
                    {
                        error = "Price line entity not found";
                        return null;
                    }

                    var priceLineEntity = PriceListRepository.GetById((int)orderLineEntity.PriceLineId);

                    if (priceLineEntity == null)
                    {
                        error = "Price list entity not found";
                        return null;
                    }

                    var priceToken = priceLineEntity.Entity2PriceLineDto(GetCurrencyDto(priceLineEntity.CurrencyId));

                    var itemType = priceToken.ItemType;

                    switch (itemType)
                    {
                        case BillingEnums.ePurchaseItemTypes.COURSE:
                            var course = GetCourseToken(priceToken.Currency.CurrencyId, priceToken.ItemId,orderLineEntity.TrackingID);
                            return course?.CourseInfoDto2ItemPurchaseCompleteToken(priceToken, orderLineEntity.SalesOrderLine2BuyerInfoDto(), orderLineEntity.TotalPrice);
                        case BillingEnums.ePurchaseItemTypes.BUNDLE:
                            var bundle = GetBundleToken(priceToken.Currency.CurrencyId, priceToken.ItemId, orderLineEntity.TrackingID);
                            return bundle?.BundleInfoDto2ItemPurchaseCompleteToken(priceToken, orderLineEntity.SalesOrderLine2BuyerInfoDto(), orderLineEntity.TotalPrice);
                        default:
                            error = "Unknown Item type";
                            return null;
                    }
                }
                
                
                if (orderLineEntity.CourseId != null)
                {
                    var course = CourseRepository.GetById((int)orderLineEntity.CourseId);
                    return course?.CourseEntity2ItemPurchaseCompleteToken(orderLineEntity, orderLineEntity.SalesOrderLine2BuyerInfoDto());
                }

                if (orderLineEntity.BundleId != null)
                {
                    var course = BundleRepository.GetById((int)orderLineEntity.BundleId);
                    return course?.BundleEntity2ItemPurchaseCompleteToken(orderLineEntity, orderLineEntity.SalesOrderLine2BuyerInfoDto());
                } 

                error = "Unknown Item type";
                return null;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);

                Logger.Error("GetItemPurchaseCompleteToken " + orderNo, orderNo, ex, CommonEnums.LoggerObjectTypes.Course);

                return null;
            }
        }

        public void PostFasebookPurchaseMessages(PurchaseMessageDTO messageToken)
        {   
            string error; 
            //author
            try
            {   
                var postDto =  messageToken.AuthorPurchaser2MessageDto();
                if (postDto != null)
                {
                    _facebookServices.SavePostMessage(postDto, out error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("save author purchase facebook message ", null, ex, CommonEnums.LoggerObjectTypes.Course);
            }

            //learner
            try
            {
                var postDto = messageToken.LearnerPurchase2MessageDTO();
                if (postDto != null)
                {
                    _facebookServices.SavePostMessage(postDto, out error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("save learner purchase facebook message ", null, ex, CommonEnums.LoggerObjectTypes.Course);
            }

            //lfe wall
            try
            {
                var postDto = messageToken.LfePurchase2MessageDto();
                if (postDto != null)
                {
                    _facebookServices.SavePostMessage(postDto, out error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("save lfe facebook wall purchase message ", null, ex, CommonEnums.LoggerObjectTypes.Course);
            }

            //buyer story
            _facebookServices.CreateUserFbStory(messageToken.Buyer.id, messageToken.Item.id, FbEnums.eFbActions.purchase_course);
        }
        #endregion        
        #endregion
        
        #region IPortalAdminCourseServices implementation
        public List<CourseBaseToken> GetCoursesList()
        {
            return CourseRepository.GetAll().Select(x => x.Entity2CourseBaseToken(GetItemRegularPrice(x.Id, BillingEnums.ePurchaseItemTypes.COURSE, DEFAULT_CURRENCY_ID), GetItemMonthlySubscriptionPrice(x.Id, BillingEnums.ePurchaseItemTypes.COURSE, DEFAULT_CURRENCY_ID))).OrderByDescending(x => x.Name).ToList(); //.GetMany(x=>x.StatusId==(short)CourseEnums.CourseStatus.Published)
        } 
        #endregion

        #region ICategoryManageServices implementation
        private bool IsCategoryNameValid(int id, string name, out string error)
        {
            error = string.Empty;
            try
            {
                return id < 0  ? !CategoryRepository.IsAny(x => x.CategoryName.ToLower() == name.ToLower()) : !CategoryRepository.IsAny(x => x.Id != id && x.CategoryName.ToLower() == name.ToLower());
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }
        public List<CategoryEditDTO> GetCategories()
        {
            try
            {
                return CategoryRepository.GetCategories().OrderBy(x=>x.CategoryName).Select(x=>x.Entity2CategoryEditDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get categories", ex, CommonEnums.LoggerObjectTypes.Course);
                return new List<CategoryEditDTO>();
            }
        }

        public bool SaveCategory(ref CategoryEditDTO token, out string error)
        {
            error = string.Empty;
            try
            {
                if (!IsCategoryNameValid(token.id, token.name, out error))
                {
                    error = "category name exists";
                    return false;
                }

                if (token.id < 0)
                {
                    var newEntity = token.Dto2CategoryEntity();
                    
                    CategoryRepository.Add(newEntity);

                    if (!CategoryRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    token.id = newEntity.Id;

                    return true;
                }
                
                var entity = CategoryRepository.GetById(token.id);

                if (entity == null)
                {
                    error = "category entity not found";
                    return false;
                }

                entity.UpdateCategoryEntity(token);

                return CategoryRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                Logger.Error("save category " +token.name, ex,token.id, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        }

        public bool DeleteCategory(CategoryEditDTO token, out string error)
        {
            error = string.Empty;
            try
            {
                if (token.cnt > 0)
                {
                    error = "category has attached courses";
                    return false;
                }

                var entity = CategoryRepository.GetById(token.id);

                if (entity == null)
                {
                    error = "category entity not found";
                    return false;
                }

                CategoryRepository.Delete(entity);
                
                return CategoryRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                Logger.Error("delete category " +token.name, ex,token.id, CommonEnums.LoggerObjectTypes.Course);
                return false;
            }
        } 
        #endregion
    }

    public class CourseWizardServices : ServiceBase, ICourseWizardServices
    {
        private readonly IAuthorAdminServices _authorAdminServices;
        private readonly IUserAccountServices _userAccountServices;
        private readonly IAuthorAdminCourseServices _courseServices;
        private readonly IAuthorAdminCategoryServices _categoryServices;
        private readonly IFacebookServices _facebookServices;
        private readonly IQuizAdminServices _quizAdminServices;
        #region private wizard params
        public static Dictionary<CourseEnums.eWizardSteps, Type> _wizardSteps;
        public static Dictionary<CourseEnums.eWizardSteps, string> _wizardStepTooltips; 
        public static Dictionary<CourseEnums.eWizardSteps, string> _wizardStepsTitles;
        public static Dictionary<CourseEnums.eWizardSteps, string> _wizardBreadcrumbs;        
        #endregion

        #region .ctor
        public CourseWizardServices()
        {

            _authorAdminServices      = DependencyResolver.Current.GetService<IAuthorAdminServices>();
            _userAccountServices      = DependencyResolver.Current.GetService<IUserAccountServices>();
            _courseServices           = DependencyResolver.Current.GetService<IAuthorAdminCourseServices>();
            _categoryServices         = DependencyResolver.Current.GetService<IAuthorAdminCategoryServices>();
            _facebookServices         = DependencyResolver.Current.GetService<IFacebookServices>();
            _quizAdminServices        = DependencyResolver.Current.GetService<IQuizAdminServices>();

            #region init dictionaries
            if (_wizardSteps == null)
            {
                _wizardSteps = new Dictionary<CourseEnums.eWizardSteps, Type>
                       {
                                   //{CourseEnums.eWizardSteps.Introduction,typeof(WizardIntroDTO)},
                                   {CourseEnums.eWizardSteps.CourseName,typeof(WizardCourseNameDTO)}
                                  ,{CourseEnums.eWizardSteps.VideoManager,typeof(WizardVideoManageDTO)}
                                //  ,{CourseEnums.eWizardSteps.ChapterManage,typeof(WizardChapterManageDTO)}
                                  ,{CourseEnums.eWizardSteps.ChapterContents,typeof(WizardChapterContentManageDTO)}
                                  ,{CourseEnums.eWizardSteps.CourseVisuals,typeof(WizardCourseVisualsDTO)}
                                  ,{CourseEnums.eWizardSteps.CourseMeta,typeof(WizardCourseMetaDTO)}
                                  ,{CourseEnums.eWizardSteps.AboutAuthor,typeof(WizardAboutAuthorDTO)}
                                  ,{CourseEnums.eWizardSteps.CoursePrice,typeof(WizardCoursePricingDTO)}
                                  ,{CourseEnums.eWizardSteps.Publish,typeof(WizardCoursePublishDTO)}
                              };
            }

            if (_wizardStepsTitles == null)
            {
                _wizardStepsTitles = new Dictionary<CourseEnums.eWizardSteps, string>
                       {
                                   //{CourseEnums.eWizardSteps.Introduction,"Creating a new LFE Course"},
                                   {CourseEnums.eWizardSteps.CourseName,"Course Name"}
                                  ,{CourseEnums.eWizardSteps.VideoManager,"My Videos"}
                                 // ,{CourseEnums.eWizardSteps.ChapterManage,"Chapter List"}
                                  ,{CourseEnums.eWizardSteps.ChapterContents,"Chapter Contents"}
                                  ,{CourseEnums.eWizardSteps.CourseVisuals,"Course Visuals"}
                                  ,{CourseEnums.eWizardSteps.CourseMeta,"Metadata"}
                                  ,{CourseEnums.eWizardSteps.AboutAuthor,"About You"}                                  
                                  ,{CourseEnums.eWizardSteps.CoursePrice,"Pricing and Coupons"}
                                  ,{CourseEnums.eWizardSteps.Publish,"Publish"}
                              };
            }

            if (_wizardBreadcrumbs == null)
            {
                _wizardBreadcrumbs = new Dictionary<CourseEnums.eWizardSteps, string>
                       {
                                   //{CourseEnums.eWizardSteps.Introduction,"Introduction"},
                                   {CourseEnums.eWizardSteps.CourseContentGroup,"Course content:"}
                                  ,{CourseEnums.eWizardSteps.CourseName,"Course Name"}
                                  ,{CourseEnums.eWizardSteps.VideoManager,"Upload videos"}
                                //  ,{CourseEnums.eWizardSteps.ChapterManage,"Chapter List"}
                                  ,{CourseEnums.eWizardSteps.ChapterContents,"Chapter Contents"}                                  
                                  ,{CourseEnums.eWizardSteps.MarketingInfoGroup,"Marketing Info:"}
                                  ,{CourseEnums.eWizardSteps.CourseVisuals,"Course Visuals"}
                                  ,{CourseEnums.eWizardSteps.CourseMeta,"Meta Data"}
                                  ,{CourseEnums.eWizardSteps.AboutAuthor,"About You"}
                                  ,{CourseEnums.eWizardSteps.CorseSettingsGroup,"Course Settings:"}
                                  ,{CourseEnums.eWizardSteps.CoursePrice,"Pricing"}
                                  ,{CourseEnums.eWizardSteps.Publish,"Publish"}
                              };
            }

            if (_wizardStepTooltips == null)
            {
                _wizardStepTooltips = new Dictionary<CourseEnums.eWizardSteps, string>();

                var steps = WizardStepsRepository.GetAll().OrderBy(x => x.StepId).ToList();

                foreach (var step in steps)
                {
                    var enums = Utils.ParseEnum<CourseEnums.eWizardSteps>(step.StepCode);

                    if(_wizardStepTooltips.ContainsKey(enums)) continue;

                    _wizardStepTooltips.Add(enums,step.TooltipHTML);
                }

            }
            #endregion
        }
        #endregion

        #region private helpers
        private IWizardStep GetStepObject(CourseEnums.eWizardSteps step)
        {
            try
            {
                if (_wizardSteps.ContainsKey(step))
                {
                    return (IWizardStep)Activator.CreateInstance(_wizardSteps[step], new object[] { });
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("get wizard step instance: " + step,null, ex, CommonEnums.LoggerObjectTypes.CourseWizard);
                return null;
            }
        }

        private static string GetStepTitle(CourseEnums.eWizardSteps step)
        {
            string title;
            var isFound = _wizardStepsTitles.TryGetValue(step, out title);
            return isFound ? title : "Title not found";
        }

        private static string GetStepTooltip(CourseEnums.eWizardSteps step)
        {
            string tooltip;
            var isFound = _wizardStepTooltips.TryGetValue(step, out tooltip);
            return isFound ? tooltip : string.Empty;
        }

        private IWizardStep LoadCurrentStep(Guid uid, int courseId, StepDataToken dataToken, CourseEnums.eWizardSteps currentStep)
        {
            try
            {
                var step = GetStepObject(currentStep);

                if (step == null) return null;

                step.CourseId = courseId;
                step.Uid      = uid;
                step.Data     = dataToken;

                return step.LoadStep();
            }
            catch (Exception ex)
            {
                Logger.Error("load current wizard step:" + currentStep, dataToken.Course.Uid, ex, CommonEnums.LoggerObjectTypes.CourseWizard);
                return null;
            }

        }

        private CourseEditDTO FindCourseByUid(Guid uid, int authorId, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = CourseRepository.Get(x => x.AuthorUserId == authorId && x.uid == uid);

                if (entity == null) return new CourseEditDTO(uid, authorId);
                var token = entity.CourseEntity2CourseEditDTO(IsItemPricesExists(entity.Id,BillingEnums.ePurchaseItemTypes.COURSE));
                //get categories
                token.Categories = CourseCategoryRepository.GetMany(x => x.CourseId == token.CourseId).Select(x => x.CategoryId).ToList();
                //get price lines
                token.PriceLines = GetAllItemPrices(entity.Id, BillingEnums.ePurchaseItemTypes.COURSE);
                token.PriceDisplayName = GetItemDefaultPriceName(entity.Id,BillingEnums.ePurchaseItemTypes.COURSE,DEFAULT_CURRENCY_ID,entity.IsFreeCourse);
                return token;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);

                Logger.Error("find course by uid", authorId, ex, CommonEnums.LoggerObjectTypes.CourseWizard);

                return null;
            }
        }
        
        #region breadcrumb
        private static CourseEnums.eWizardSetpModes GetStepMode(CourseEnums.eWizardSteps step, CourseEnums.eWizardSteps current, short completedSteps)
        {
            if(step==current) return CourseEnums.eWizardSetpModes.Current; 

            switch (step)
            {
                case CourseEnums.eWizardSteps.MarketingInfoGroup:
                case CourseEnums.eWizardSteps.CourseContentGroup:
                case CourseEnums.eWizardSteps.CorseSettingsGroup:
                    return CourseEnums.eWizardSetpModes.Dummy;
                case CourseEnums.eWizardSteps.CourseName:
                    return CourseEnums.eWizardSetpModes.Allowed;
            //    case CourseEnums.eWizardSteps.ChapterContents:
                  //  return chaptersCnt> 0 ? CourseEnums.eWizardSetpModes.Allowed : CourseEnums.eWizardSetpModes.Disable;
                default:
                    //all steps allowed if course created
                    return completedSteps > (short)CourseEnums.eWizardSteps.CourseName ? CourseEnums.eWizardSetpModes.Allowed : CourseEnums.eWizardSetpModes.Disable;
                        //(short)step <= completedSteps+1 ? CourseEnums.eWizardSetpModes.Allowed : CourseEnums.eWizardSetpModes.Disable;
            }
        }

        private static bool IsNextAllowed(CourseEnums.eWizardSteps currentStep, bool courseCreated)
        {
            if (!courseCreated)
            {
                return currentStep == CourseEnums.eWizardSteps.CourseName;
            }
            
            switch (currentStep)
            {
                case CourseEnums.eWizardSteps.CourseName:
                    return true;
                case CourseEnums.eWizardSteps.ChapterContents:
                    return true;//chaptersCnt > 0;
                    //TODO create prerequisite logic for publishing
                case CourseEnums.eWizardSteps.Publish:
                    return true;
                default:
                    //all steps allowed if course created
                    return true;
            }
        }

        #endregion
        #endregion

        #region interface implementation
        public CourseWizardDto LoadCourseWizard(Guid uid, int userId, CourseEnums.eWizardSteps? next = null, int? selectedChapterId = null, short? currencyId = null)
        {
            try
            {
                string error;
                var course = FindCourseByUid(uid, userId,out error);

                if(course==null) return new CourseWizardDto
                {
                    IsValid       = false
                    ,ErrorMessage = error
                };

                var user = _userAccountServices.GetSettingsToken(userId,out error);

                if (user == null) return new CourseWizardDto
                {
                     IsValid = false
                    ,ErrorMessage = error
                };

                var videosCnt = _authorAdminServices.GetAuthorVideosCount(userId);

                var data = new StepDataToken
                {
                    User = user
                    ,Course = course
                };

                CourseWizardDto dto;

                if (course.CourseId < 0 && next==null) //onload case
                {
                    dto =new CourseWizardDto(uid,userId)
                    {
                        LastCompletedStep    = CourseEnums.eWizardSteps.Introduction,
                        StepTitle            = GetStepTitle(CourseEnums.eWizardSteps.CourseName),
                        CurrentStep          = LoadCurrentStep(uid,-1,data, CourseEnums.eWizardSteps.CourseName),
                        VideosCount          = videosCnt,
                        ChapersCount         = 0,
                        IsAnyContentsCreated = false
                    };
                }
                else
                {
                    #region set wizard state
                    var chapters = _courseServices.GetCourseChapters(course.CourseId).ToList();
                    var categories = _courseServices.GetCourseCategoryIds(course.CourseId);
                    var anyContents = _courseServices.IsAnyCourseContentsCreated(course.CourseId);
                    
                    dto = new CourseWizardDto(uid, userId)
                                                            {
                                                                CourseId              = course.CourseId
                                                                ,Name                 = course.CourseName == course.Uid.ToString() ? "New course" : course.CourseName
                                                                ,VideosCount          = videosCnt
                                                                ,ChapersCount         = chapters.Count
                                                                ,IsAnyContentsCreated = anyContents
                                                            };

                    data.CourseChapters = chapters;
                    data.ChapersCount   = dto.ChapersCount;
                    data.AnyContents    = anyContents;
                    
                    if (string.IsNullOrEmpty(course.CourseName) || course.CourseName == course.Uid.ToString() || categories.Count.Equals(0))
                    {
                        dto.LastCompletedStep = CourseEnums.eWizardSteps.Introduction;
                        dto.CurrentWizardStep = CourseEnums.eWizardSteps.CourseName;

                    }
                    else
                    {
                        //always allow all steps if course created
                        dto.LastCompletedStep = CourseEnums.eWizardSteps.CoursePrice;

                        if (videosCnt == 0)
                        {
                            dto.CurrentWizardStep = CourseEnums.eWizardSteps.VideoManager;
                        }
                        else if (!chapters.Any())
                        //{
                        //    dto.CurrentWizardStep = CourseEnums.eWizardSteps.ChapterManage;
                        //}
                        //else if (!anyContents)
                        {
                            dto.CurrentWizardStep = CourseEnums.eWizardSteps.ChapterContents;
                        }
                        else if (string.IsNullOrEmpty(course.ThumbUrl) || course.PromoVideoIdentifier == null)
                        {
                            dto.CurrentWizardStep = CourseEnums.eWizardSteps.CourseVisuals;
                        }
                        else if (course.Price == null)
                        {
                            dto.CurrentWizardStep = CourseEnums.eWizardSteps.CoursePrice;
                        }
                        else
                        {
                            dto.CurrentWizardStep = CourseEnums.eWizardSteps.Publish;
                        }    
                    }                    
                    #endregion
                }

                //jump to next if supplied
                if (next != null && (short) next <= (short) dto.LastCompletedStep + 1)
                {
                    dto.CurrentWizardStep = (CourseEnums.eWizardSteps)next;
                }

                //set next step
                dto.NextWizardStep = dto.CurrentWizardStep != CourseEnums.eWizardSteps.Publish ? 
                                                                                                Utils.ParseEnum<CourseEnums.eWizardSteps>(( (short)dto.CurrentWizardStep + 1 ).ToString()) 
                                                                                              : CourseEnums.eWizardSteps.Finish;
                
                //disable back button for introduction
                if (dto.CurrentWizardStep != CourseEnums.eWizardSteps.CourseName)
                {
                    dto.BackWizardStep = Utils.ParseEnum<CourseEnums.eWizardSteps>(( (short)dto.CurrentWizardStep - 1 ).ToString());
                }


                //load step token
                dto.CurrentStep = LoadCurrentStep(uid,dto.CourseId,data, dto.CurrentWizardStep);

                if (dto.CurrentStep == null)
                {
                    dto.IsValid = false;
                    dto.ErrorMessage = "Module loading failed";

                    return dto;
                }

                #region specific steps actions
                
                //COURSE NAME
                //add categories data to token, required for multiselect
                if (dto.CurrentWizardStep == CourseEnums.eWizardSteps.CourseName)
                {
                    var token = dto.CurrentStep as WizardCourseNameDTO;
                    // ReSharper disable once PossibleNullReferenceException
                    token.CategoriesData = _categoryServices.ActiveCategories().OrderBy(x => x.name).ToArray();
                }

                //COURSE VISUALS
                //set course visuals
                if (dto.CurrentWizardStep == CourseEnums.eWizardSteps.CourseVisuals)
                {
                    var token = dto.CurrentStep as WizardCourseVisualsDTO;
                    //set VideoThumbUrl
                    // ReSharper disable once PossibleNullReferenceException
                    if (token.PromoVideoIdentifier != null)
                    {
                        token.PromoVideo = _GetVideoToken((long)token.PromoVideoIdentifier, userId);

                        //try
                        //{
                            
                        //    var videoToken = _brightcoveWrapper.FindVideoById((long)token.PromoVideoIdentifier);

                        //    if (videoToken != null) token.PromoVideo = videoToken.BrightcoveVideo2VideoDTO(userId, userId.UserId2Tag(), 0);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Logger.Error("get course video::" + token.PromoVideoIdentifier, userId, ex, CommonEnums.LoggerObjectTypes.Course);
                        //}
                    }
                }

                //CHAPTER CONTENTS
                //set selected chapter , if supplied ( jump from chapter list to chapter contents tab
                if (dto.CurrentWizardStep == CourseEnums.eWizardSteps.ChapterContents)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    (dto.CurrentStep as WizardChapterContentManageDTO ).TotalQuizzes = _quizAdminServices.TotalUserValidPublishedQuizzes(userId);

                    // ReSharper disable once PossibleNullReferenceException
                    if (selectedChapterId != null) (dto.CurrentStep as WizardChapterContentManageDTO ).SelectedChapterId = (int)selectedChapterId;                    
                }

                //UPLOAD MANAGER
                //set info field
                if (dto.CurrentWizardStep == CourseEnums.eWizardSteps.VideoManager)
                {
                    (dto.CurrentStep as WizardVideoManageDTO).Info = GetStepTooltip(dto.CurrentWizardStep);
                }

                //PRICING
                if (dto.CurrentWizardStep == CourseEnums.eWizardSteps.CoursePrice)
                {
                    (dto.CurrentStep as WizardCoursePricingDTO).Currency = GetCurrencyDto(currencyId ?? DEFAULT_CURRENCY_ID);
                }

                //PUBLISH
                //check publish ready state
                if (dto.CurrentWizardStep == CourseEnums.eWizardSteps.Publish)
                {
                    var token               = dto.CurrentStep as WizardCoursePublishDTO;
                    dto.IsPublishAllowed    = token.Ready2Publish;
                    dto.IsNextAllowed       = dto.IsPublishAllowed;
                }
                #endregion

                //set additional properties
                dto.StepTitle       = GetStepTitle(dto.CurrentWizardStep);
                dto.StepTooltip     = GetStepTooltip(dto.CurrentWizardStep);
                
                if (dto.CurrentWizardStep != CourseEnums.eWizardSteps.Publish) dto.IsNextAllowed   = IsNextAllowed(dto.CurrentWizardStep,dto.CourseId>0);
                
                dto.IsSaveAndNext   = dto.CurrentWizardStep.WizardStep2NextButtonState();
                dto.NextButtonTitle = dto.CurrentWizardStep.WizardStep2NextButtonText();
                dto.CheckVideoState = videosCnt == 0;
                
               
                
                return dto;
            }
            catch (Exception ex)
            {
                Logger.Error("load course wizard", userId, ex, CommonEnums.LoggerObjectTypes.CourseWizard);
                return new CourseWizardDto
                {
                    IsValid       = false
                    ,ErrorMessage = FormatError(ex)
                };
            }
        }

        public List<BreadcrumbStepDTO> GetBreadcrumbSteps(CourseEnums.eWizardSteps completedSteps, CourseEnums.eWizardSteps currentStep, int chaptersCnt)
        {
            return _wizardBreadcrumbs.Where(x=>x.Key!=CourseEnums.eWizardSteps.Introduction).Select(wizardStep => new BreadcrumbStepDTO
                {
                     StepTitle   = wizardStep.Value                    
                    ,Mode        = GetStepMode(wizardStep.Key,currentStep,(short)completedSteps)
                    ,Step        = wizardStep.Key
                }).Select(x => x.SetBreadcrumbItemClass()).ToList();
        }

        #region save events
        public bool SaveCourseName(WizardCourseNameDTO token, int userId, out string error)
        {
            try
            {
                if (!IsCourseNameValid(token.CourseId, token.CourseName, userId, out error))
                {
                    error = string.IsNullOrEmpty(error) ? "Course Name already exists" : error;
                    return false;
                }
                var entity = CourseRepository.GetById(token.CourseId);

                if (entity == null)
                {
                    error = "course entity not found";
                    return false;
                }

                entity.UpdateCourseEntity(token);

                CourseRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course name", ex, token.CourseId, CommonEnums.LoggerObjectTypes.CourseWizard);
                return false;
            }
        }

        public bool SaveCourseVisuals(WizardCourseVisualsDTO token, out string error)
        {
            error = string.Empty;
            try
            {

                var entity = CourseRepository.GetById(token.CourseId);

                if (entity == null)
                {
                    error = "course entity not found";
                    return false;
                }

                entity.UpdateCourseEntity(token);

                CourseRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course visuals", ex, token.CourseId, CommonEnums.LoggerObjectTypes.CourseWizard);
                return false;
            }
        }

        public bool SaveCourseMeta(WizardCourseMetaDTO token, out string error)
        {
            error = string.Empty;

            try
            {

                var entity = CourseRepository.GetById(token.CourseId);

                if (entity == null)
                {
                    error = "course entity not found";
                    return false;
                }

                entity.UpdateCourseEntity(token);

                CourseRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course meta tags", ex, token.CourseId, CommonEnums.LoggerObjectTypes.CourseWizard);
                return false;
            }
        }

        public bool PublishCourse(WizardCoursePublishDTO token, int userId, out string error)
        {
            error = string.Empty;
            try
            {

                var entity = CourseRepository.GetById(token.CourseId);

                if (entity == null)
                {
                    error = "course entity not found";
                    return false;
                }

                entity.UpdateCourseEntity(token);

                if (token.Status == CourseEnums.CourseStatus.Published && !entity.FbObjectPublished)
                {
                    _facebookServices.CreateUserFbStory(userId,token.CourseId,FbEnums.eFbActions.publish_course);

                    entity.FbObjectPublished = true;
                }

                CourseRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;


            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("publish course", ex,token.CourseId, CommonEnums.LoggerObjectTypes.CourseWizard);
                return false;
            }
        }
        #endregion
        #endregion

        #region dispose
        public new void Dispose()
        {
            _courseServices.Dispose();
            _authorAdminServices.Dispose();
            _categoryServices.Dispose();
            _userAccountServices.Dispose();
         
            CourseRepository.Dispose();
            CourseCategoryRepository.Dispose();
            WizardStepsRepository.Dispose();
        } 
        #endregion
    }

}
