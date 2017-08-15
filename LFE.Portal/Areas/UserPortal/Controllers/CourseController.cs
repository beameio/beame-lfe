using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.UserPortal.Models;

namespace LFE.Portal.Areas.UserPortal.Controllers
{
    public class CourseController : BaseController
    {
        private readonly IWidgetCourseServices _widgetCourseServices;
        private readonly IUserPortalServices _userPortalServices;

        public CourseController()
        {
            _widgetCourseServices        = DependencyResolver.Current.GetService<IWidgetCourseServices>();
            _userPortalServices    = DependencyResolver.Current.GetService<IUserPortalServices>();
        }

        #region views
        public ActionResult CourseViewer(string authorName, string courseName, string mode = null,string trackingID = null)
        {

            //return RedirectToRoute("UserPortal2Widget_Course", new { type = BillingEnums.ePurchaseItemTypes.COURSE, author = authorName.OptimizedUrl(), itemName = courseName.OptimizedUrl(), trackingId = trackingID });

            var url = this.GenerateItemPageUrl(authorName.OptimizedUrl(),courseName.OptimizedUrl(),BillingEnums.ePurchaseItemTypes.COURSE,trackingID);

            return RedirectPermanent(url);
            
            return RedirectToAction("Index", "Item", new { area="Widget",type = BillingEnums.ePurchaseItemTypes.COURSE, author = authorName.OptimizedUrl(), itemName = courseName.OptimizedUrl(), trackingId = trackingID});

            var pageToken = new CourseViewerPageToken();

            var baseDto = _widgetCourseServices.FindCourseByUrlName(authorName,courseName);

            if (baseDto == null)
            {
                pageToken.IsValid = false;
                
                pageToken.Message = "Course not found";
                
                return View(pageToken);
            }

            pageToken.IsValid    = true;
            pageToken.CourseId   = baseDto.id;
            pageToken.CourseName = baseDto.name;

            var courseState =  _widgetCourseServices.GetCourseAccessState4User(CurrentUserId, baseDto.id);
            courseState.IsPreview = ( !String.IsNullOrEmpty(mode) && mode == Core.Utils.Constants.QS_COURSE_PREVIEW_PREFIX);

            var loadViewer = !courseState.IsPreview && (courseState.IsOwnedByUser || (courseState.IsAccessAllowed && courseState.IsPublished));            

            // ReSharper disable once RedundantArgumentName
            pageToken.CourseViewerDTO = _widgetCourseServices.GetLearnerCourseViewerDTO(baseDto.id, CurrentUserId,loadBasicInfo:!loadViewer);

            pageToken.ItemState = courseState;

            pageToken.TrackingID = trackingID;

            return View(pageToken);
        }

        public ActionResult BundleViewer(string authorName, string bundleName, string mode = null, string trackingID = null)
        {
            var url = this.GenerateItemPageUrl(authorName.OptimizedUrl(), bundleName.OptimizedUrl(), BillingEnums.ePurchaseItemTypes.BUNDLE, trackingID);

            return RedirectPermanent(url);
            return RedirectToAction("Index", "Item", new { area = "Widget", type = BillingEnums.ePurchaseItemTypes.BUNDLE, author = authorName.OptimizedUrl(), itemName = bundleName.OptimizedUrl(), trackingId = trackingID });
            var pageToken = new BundleViewerPageToken();

            var baseDto = _widgetCourseServices.FindBundleByUrlName(authorName,bundleName);

            if (baseDto == null)
            {
                pageToken.IsValid = false;

                pageToken.Message = "Bundle not found";

                return View(pageToken);
            }

            pageToken.IsValid    = true;
            pageToken.Bundle     = baseDto;

            var itemState = _widgetCourseServices.GetBundleAccessState4User(CurrentUserId, baseDto.BundleId);
            itemState.IsPreview = (!String.IsNullOrEmpty(mode) && mode == Core.Utils.Constants.QS_COURSE_PREVIEW_PREFIX);

            pageToken.ItemState = itemState;

            var loadViewer = !itemState.IsPreview && (itemState.IsOwnedByUser || (itemState.IsAccessAllowed && itemState.IsPublished));            

            pageToken.Author = _userPortalServices.GetUserProfileDto(baseDto.AuthorId);

            if (loadViewer)
            {               
                pageToken.BundleCourses = _widgetCourseServices.GetBundleCoursesList(baseDto.BundleId);
            }

            pageToken.TrackingID = trackingID;

            return View(pageToken);
        }

        [Authorize]
        public ActionResult GetCourseReviewPartial(int id)
        {
            var dto = _widgetCourseServices.GetUserCourseReview(id, CurrentUserId);

            return PartialView("CourseViewer/_CourseReview", dto);
        }
        
        [Authorize]
        public ActionResult GetChapterContentsPartial()
        {
            return PartialView("CourseViewer/_ChapterContent");
        }

        public ActionResult GetOtherLearnersPartial(int id)
        {
            return PartialView("CourseViewer/_OtherLearners",id);
        }

        public ActionResult GetOtherLearnerPersonalPartial(int id)
        {
            return PartialView("CourseViewer/_OtherLearnerPersonal", GetUserProfileDto(id, 3));
        }
        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetChapterLinks([DataSourceRequest] DataSourceRequest request, int? id, CourseEnums.eChapterLinkKind kind)
        {
            var list =id!=null ?   _widgetCourseServices.GetChapterLinks((int) id, kind).OrderBy(x => x.index).ToArray() : new List<ChapterLinkListToken>().ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOtherLearners([DataSourceRequest] DataSourceRequest request, int? id)
        {
            var list =id!=null ?  _widgetCourseServices.GetOtherCourseLearners((int) id,CurrentUserId).OrderBy(x => x.name).ToArray() : new List<LearnerListItemDTO>().ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetChaptersTreeView(int id)
        //{
        //    var json = _widgetCourseServices.GetCourseChaptersList(id);
        //    return Json(json, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region posts
        [Authorize]
        [HttpPost]
        public ActionResult SaveReview(CourseUserReviewDTO token)
        {
            string error;
            var saved = _widgetCourseServices.SaveCourseReview(token, CurrentUserId, out error);

            return Json(new JsonResponseToken
                        {
                            success = saved
                            ,result = _widgetCourseServices.GetCourseRating(token.CourseId)
                            ,error = error
                        },JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveLearnerCourseState(int courseId, int chapterId, int videoId,long? bcId)
        {
            try
            {
                if (CurrentUserId < 0) return Json(new JsonResponseToken { success = false,error = "user not found"}, JsonRequestBehavior.AllowGet);
                // ReSharper disable once RedundantArgumentDefaultValue
                _userPortalServices.UpdateCourseStateAndCreateStory(courseId, CurrentUserId, chapterId, videoId,bcId,createStory:true);

                SaveUserEvent(CommonEnums.eUserEvents.VIDEO_COURSE_WATCH,"VideoId::" + videoId,null,courseId,null,bcId);

                return Json(new JsonResponseToken{success = true}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new JsonResponseToken(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult OnRentalFinished(int id)
        {
            try
            {
                if (CurrentUserId < 0) return Json(new JsonResponseToken { success = false, error = "user not found" }, JsonRequestBehavior.AllowGet);
                string error;
                var result = _widgetCourseServices.OnCourseRentalFinished(id, CurrentUserId, out error);

                return Json(new JsonResponseToken { success = result,error = error}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new JsonResponseToken(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PublishUserViewStory(int? courseId, int? chapterId, int? videoId, long? bcId)
        {
            try
            {
                if(courseId == null || chapterId == null || videoId==null) return ErrorResponse("required params missing");

                if (CurrentUserId < 0) return Json(new JsonResponseToken { success = false, error = "user not found" }, JsonRequestBehavior.AllowGet);
               
                _userPortalServices.CreateStoryView((int) courseId, CurrentUserId, (int) chapterId, (int) videoId, bcId);

                //SaveUserEvent(CommonEnums.eUserEvents.VIDEO_COURSE_WATCH, "VideoId::" + videoId, null, courseId, null, bcId);

                return Json(new JsonResponseToken { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new JsonResponseToken(), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion        
    }
}
