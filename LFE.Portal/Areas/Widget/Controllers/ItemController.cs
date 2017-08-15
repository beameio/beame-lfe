using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.UserPortal.Models;
using LFE.Portal.Areas.Widget.Models;
using LFE.Portal.Helpers;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace LFE.Portal.Areas.Widget.Controllers
{
    public class ItemController : BaseController
    {
        private readonly IWidgetServices _widgetServices;
        private readonly IWidgetCourseServices _widgetCourseServices;
        private readonly IUserPortalServices _userPortalServices;
        private readonly IWidgetUserServices _widgetUserServices;
        private readonly IWebStoreServices _webStoreServices; 
        public ItemController()
        {
            _widgetServices         = DependencyResolver.Current.GetService<IWidgetServices>();
            _widgetCourseServices   = DependencyResolver.Current.GetService<IWidgetCourseServices>();
            _userPortalServices     = DependencyResolver.Current.GetService<IUserPortalServices>();
            _widgetUserServices     = DependencyResolver.Current.GetService<IWidgetUserServices>();
            _webStoreServices       = DependencyResolver.Current.GetService<IWebStoreServices>();
        }

        public ActionResult ItemNotPublished()
        {
            return View();
        }

        [EnableCompression]
        //[CacheFilter(Duration = 60)]
        public ActionResult Index(BillingEnums.ePurchaseItemTypes type, string author, string itemName,string trackingId, string mode = null, int? width = null, int? height = null)
        {
            var item = WidgetServices.FindItemByUrlName(author, itemName, type);

            if (!item.IsValid) return View("ItemNotFound", item);

            #region ViewBag settings

            try
            {
                ViewBag.BackgroundColor = "#FFFFFF";
                if (MainLayoutViewModel != null)
                {
                    if (MainLayoutViewModel.WebStore != null && !string.IsNullOrEmpty(MainLayoutViewModel.WebStore.BackgroundColor))
                    {
                        ViewBag.BackgroundColor = MainLayoutViewModel.WebStore.BackgroundColor;
                    }

                    if (!string.IsNullOrEmpty(Request.QueryString["_escaped_fragment_"]))
                    {
                        ViewBag.TabName = Request.QueryString["_escaped_fragment_"];
                    }

                    if (width != null) MainLayoutViewModel.Width = width;
                    if (height != null) MainLayoutViewModel.Height = height;    
                }
                else
                {
                    MainLayoutViewModel = new BaseModelViewToken
                    {
                        WebStore             = null
                        ,CategoriesList      = new List<WidgetCategoryDTO>()
                        ,IsValid             = true
                        ,IsSingleCourseStore = true
                        ,TrackingId          = trackingId
                    };

                    ViewData["MainLayoutViewModel"] = MainLayoutViewModel;
                    TempData["MainLayoutViewModel"] = MainLayoutViewModel;    
                    
                }
                
            }
            catch (Exception ex)
            {
                Logger.Error("Item page ViewBag::" + itemName + "::" + author + "::" + trackingId, ex,CommonEnums.LoggerObjectTypes.Widget);
            }

            #endregion


            var itemState = WidgetServices.GetItemAccessState4User(CurrentUserId, item.ItemId, item.ItemType);

            itemState.IsPreview = (!String.IsNullOrEmpty(mode) && mode == Constants.QS_COURSE_PREVIEW_PREFIX);

            var loadViewer = !itemState.IsPreview && (itemState.IsOwnedByUser || (itemState.IsAccessAllowed && itemState.IsPublished) || (mode == Constants.QS_COURSE_ADMIN_PREVIEW_PREFIX && this.IsCurrentUserAdmin()));

            var token = WidgetServices.ItemInfoToken2ItemViewerPageToken(item, itemState, trackingId);

            if(loadViewer) return View("ItemViewer", token);

            return itemState.IsPublished || (itemState.IsPreview && itemState.IsOwnedByUser) ? View("ProductPage", WidgetServices.ItemInfoToken2ItemProductPageToken(item, itemState, trackingId)) : View("ItemNotPublished");
            
           // var url = Url.ActionString("ItemNotPublished","Item",new RouteValueDictionary{{"area","Widget"}});

            //return Redirect(url);
        }

        public ActionResult _AuthorCatalog(int? id)
        {
            MainLayoutViewModel = WidgetServices.GetStoreBaseModelToken(Constants.LFE_MAIN_STORE_TRACKING_ID);

            ViewData["MainLayoutViewModel"] = MainLayoutViewModel;
            TempData["MainLayoutViewModel"] = MainLayoutViewModel;

            var model = id!= null ? _widgetServices.GetAuthorIndexModelViewToken(Constants.DEFAULT_CURRENCY_ID,Constants.LFE_MAIN_STORE_TRACKING_ID,(int)id) : new IndexModelViewToken{ItemsList = new List<WidgetItemListDTO>()};
            return PartialView("Item/_Catalog",model);
        }

        #region product page partials

        public ActionResult GetCourseContentsPartial(int id, string trackingId)
        {
            var infoToken = WidgetServices.GetItemInfoToken(id, BillingEnums.ePurchaseItemTypes.COURSE);

            return PartialView("Item/Course/_CourseContent",
                !infoToken.IsValid
                    ? new ItemProductPageToken {IsValid = false, Message = infoToken.Message}
                    : WidgetServices.ItemInfoToken2ItemProductPageToken(infoToken, new ItemAccessStateToken(),
                        trackingId));
        }

        public ActionResult GetCourseReviewsPartial(int id, int rait)
        {
            var list = WidgetServices.GetItemReviews(id);

            return PartialView("Item/Course/_CourseReviews", new ItemReviewsPageToken
            {
                itemId = id,
                raiting = rait,
                reviews = list
            });
        }

        #endregion

        #region viewer partials

        [Authorize]
        public ActionResult GetChapterContentsPartial()
        {
            return PartialView("Item/Course/Viewer/_ChapterContent");
        }
     

        [Authorize]
        public ActionResult GetCourseReviewPartial(int id)
        {
            var dto = _widgetCourseServices.GetUserCourseReview(id, CurrentUserId);

            return PartialView("Item/Course/Viewer/_CourseReview", dto);
        }

        public ActionResult GetCourseDiscussionPartial(int id, int courseId)
        {
            var token = new CourseDiscussionToken
            {
                RoomId = id
                ,CourseId = courseId
            };

            return PartialView("Discussion/_CourseDiscussion", token);
        }

        public ActionResult GetOtherLearnersPartial(int? id)
        {
             var token = id == null ? new AuthorProfilePageToken
                                    {
                                        IsValid = false
                                        ,Message = "itemId required"
                                    }
                                   : GetAuthorProfilePageToken(_widgetCourseServices.GetItemAuthor((int)id,BillingEnums.ePurchaseItemTypes.COURSE));

          //  if (token.IsValid && id != null) token.LearnerCourses = _widgetUserServices.GetLearnerPurchasedItems(CurrentUserId, null);

            return PartialView("Item/Course/Viewer/_OtherLearners", new OtherLearnersPageToken
            {
                AuthorProfile = token
                ,ItemId = id ?? -1
            });
        }

        public ActionResult GetOtherLearnerPersonalPartial(int? id)
        {
            var token = id == null ? new AuthorProfilePageToken
                                    {
                                        IsValid = false
                                        ,Message = "authorId required"
                                    }
                                   : GetAuthorProfilePageToken((int)id);

            if (token.IsValid)
            {
                token.LearnerCourses     = id != null ? _widgetUserServices.GetLearnerPurchasedItems((int)id, null) : new List<WidgetItemListDTO>();
                token.ShowPurchased      = true;
                token.ShowItemsListTitle = true;
                token.ItemsListTitle     = GlobalResources.LRNR_TakenCourses;
            }

            return PartialView("Item/Course/Viewer/_OtherLearnerPersonal", token);
        }

        public ActionResult LoadPlayer(long? bcid)
        {

            var token = bcid != null ? _widgetUserServices.GetVideoRenditions((long)bcid) : new VideoInfoToken{IsValid = false,Message = "BcId required"};
            return PartialView("Item/_ViewerPlayer",token);
        }
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

        public JsonResult PublishUserViewStory(int? courseId, int? chapterId, int? videoId, long? bcId)
        {
            try
            {
                if (courseId == null || chapterId == null || videoId == null) return Json(new JsonResponseToken { success = false, error = "required params missing" }, JsonRequestBehavior.AllowGet);

                if (CurrentUserId < 0) return Json(new JsonResponseToken { success = false, error = "user not found" }, JsonRequestBehavior.AllowGet);
                // ReSharper disable once RedundantArgumentDefaultValue
                _userPortalServices.CreateStoryView((int)courseId, CurrentUserId, (int)chapterId, (int)videoId, bcId);

                //SaveUserEvent(CommonEnums.eUserEvents.VIDEO_COURSE_WATCH, "VideoId::" + videoId, null, courseId, null, bcId);

                return Json(new JsonResponseToken { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new JsonResponseToken(), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion        

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOtherLearners([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _widgetCourseServices.GetOtherCourseLearners(id, CurrentUserId).OrderBy(x => x.name).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetLfeAuthors(int id)
        {
            var list = _webStoreServices.GetStoreAuthorsLOV(id).OrderBy(x=>x.fullName).ToArray();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLfeItems(int id)
        {
            var list = _webStoreServices.GetStoreItemsLOV(id);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadCourseContentsTree(int id)
        {
            var contents = _widgetServices.GetCourseContentsList(id);
            
            return PartialView("Item/Course/Viewer/_ChaptersNavigation",contents);
        }
        #endregion

    }
}
