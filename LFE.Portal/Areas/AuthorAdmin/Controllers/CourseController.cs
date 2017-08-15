using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.AuthorAdmin.Helpers;
using LFE.Portal.Areas.AuthorAdmin.Models;
using LFE.Portal.Helpers;
using Resources;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    [Authorize]
    public class CourseController : BaseController
    {
        private readonly IAuthorAdminCourseServices _courseServices;
        private readonly ICourseWizardServices _courseWizardServices;
        private readonly IAuthorAdminCouponServices _couponServices;
        private readonly IAuthorAdminServices _authorAdminServices;
        private readonly IAuthorAdminCategoryServices _categoryServices;
        private readonly IAuthorAdminDiscussionServices _discussionServices;
        private readonly IWebStoreServices _webStoreServices;
        private readonly IUserAccountServices _userAccountServices;
   //     private readonly ICourseQuizzesServices _courseQuizzesServices;
        private readonly IQuizAdminServices _quizAdminServices;
        public CourseController()
        {
            _courseServices            = DependencyResolver.Current.GetService<IAuthorAdminCourseServices>();
            _courseWizardServices      = DependencyResolver.Current.GetService<ICourseWizardServices>();
            _couponServices            = DependencyResolver.Current.GetService<IAuthorAdminCouponServices>();
            _authorAdminServices       = DependencyResolver.Current.GetService<IAuthorAdminServices>();
            _categoryServices          = DependencyResolver.Current.GetService<IAuthorAdminCategoryServices>();
            _discussionServices        = DependencyResolver.Current.GetService<IAuthorAdminDiscussionServices>();
            _webStoreServices          = DependencyResolver.Current.GetService<IWebStoreServices>();
            _userAccountServices       = DependencyResolver.Current.GetService<IUserAccountServices>();
     //       _courseQuizzesServices     = DependencyResolver.Current.GetService<ICourseQuizzesServices>();
            _quizAdminServices         = DependencyResolver.Current.GetService<IQuizAdminServices>();
        }

        #region views
        public ActionResult GetCourseReport(eReportTypes type)
        {
            switch (type)
            {
               case eReportTypes.Grid:
                    return PartialView("CourseReport/_CoursesList");
               case eReportTypes.List:
                    return PartialView("CourseReport/_CoursesGrid");
            }

            return PartialView("CourseReport/_CoursesList");
        }

        public ActionResult GetBundleReport(eReportTypes type)
        {
            switch (type)
            {
                case eReportTypes.Grid:
                    return PartialView("CourseReport/_BundlesList");
                case eReportTypes.List:
                    return PartialView("CourseReport/_BundlesGrid");
            }

            return PartialView("CourseReport/_BundlesList");
        }

        /// <summary>
        /// add new course button or edit from list/grid
        /// </summary>
        /// <param name="id">course uid</param>
        /// <returns></returns>
        public ActionResult EditCourse(Guid id)
        {
            var author = GetCurrentUser();

            var isBelong2Author = _courseServices.ValidateAuthorCourseByUid(author.userId, id);

            if (!isBelong2Author)
            {
                return RedirectToAction("NonAuthorized", "Error");
            }

            var course =  _courseServices.FindCourseByUid(author.userId, id);

            var token = new EditCoursePageToken
                {
                    user      = author
                    ,course   = course
                    ,mode     = course.id < 0 ? CommonEnums.ePageMode.insert : CommonEnums.ePageMode.edit
                    ,title    = course.id < 0 ? "Create New Course" : "Edit " + course.name
                };

            return View(token);
        }

        public ActionResult EditBundle(Guid id)
        {
            var author = GetCurrentUser();

            var isBelong2Author = _courseServices.ValidateAuthorBundleByUid(author.userId, id);

            if (!isBelong2Author)
            {
                return RedirectToAction("NonAuthorized", "Error");
            }

            var bundle =  _courseServices.FindBundleByUid(author.userId, id);

            var token = new EditBundlePageToken
                {
                    user      = author
                    ,bundle   = bundle
                    ,mode     = bundle.id < 0 ? CommonEnums.ePageMode.insert : CommonEnums.ePageMode.edit
                    ,title    = bundle.id < 0 ? "Create New Bundle" : "Edit " + bundle.name
                };


            return View(token);
        }

        #region editor tab partials
        public ActionResult CourseDetails(int id,Guid Uid)
        {
            var token = id < 0 ? new CourseEditDTO(Uid,CurrentUserId) : _courseServices.GetCourseEditDTO(id);            

            ViewData[WebConstants.VD_CATEGORY_LOV] = _categoryServices.ActiveCategories();
            ViewData[WebConstants.AUTHOR_ROOMS_LOV] = _discussionServices.AuthorRoomsLOV(CurrentUserId);

            return PartialView("EditCourse/_CourseDetails",token);
        }
        
        public ActionResult CourseContent(int id)
        {
            var c = _courseServices.GetCourseEditDTO(id);

            return PartialView("EditCourse/_CourseContents", new CourseContentsManageToken
                {
                    id            =id
                    ,name         = c.CourseName
                    ,TotalQuizzes = _quizAdminServices.CourseAvailableQuizzes(id).Count
                });
        }
        public ActionResult CourseQuizzes(int id)
        {
            return PartialView("EditCourse/_QuizManage", new BaseEntityDTO
            {
                id = id
            });
        }
      
        public ActionResult CourseReviews(int id)
        {
            return PartialView("EditCourse/_Reviews", new BaseEntityDTO
            {
                id = id
            });
        }

        public ActionResult _CourseQuizViewForm(int id)
        {
            var token = _quizAdminServices.GetQuizToken(id);
            return PartialView("EditCourse/_QuizListViewForm",token);
        }

        public ActionResult CoursePricing(int id)
        {
            return PartialView("EditCourse/_Pricing",new BaseEntityDTO
            {
                id = id
            });
        }

        public ActionResult CoursePrice(int id)
        {
            var token = _courseServices.GetCoursePrice(id,Constants.DEFAULT_CURRENCY_ID);

            return PartialView("EditCourse/_CoursePrice",token);
        }

        public ActionResult EditCourseCoupon(int? id,int courseId)
        {
            var token = _couponServices.GetCourseCoupon(id ?? -1,courseId);
            return PartialView("EditCourse/_EditCoupon", token);
        }

        public ActionResult EditBundleCoupon(int? id, int bundleId)
        {
            var token = _couponServices.GetBundleCoupon(id ?? -1, bundleId);

            return PartialView("EditBundle/_EditCoupon", token);
        }

        public ActionResult EditAuthorCoupon(int? id)
        {
            var token = _couponServices.GetAuthorCoupon(id ?? -1, CurrentUserId);

            return PartialView("Coupons/_EditAuthorCoupon", token);
        }

        public ActionResult CourseSales(int id)
        {
            return PartialView("SalesReports/Course/_SalesReport", new BaseEntityDTO
            {
                id = id
            });
        }     

        public ActionResult CourseMarketing(int id)
        {
            return PartialView("EditCourse/_Marketing",new CourseEditDTO
                {
                    CourseId = id
                });
        }

        public ActionResult CourseStores(int id)
        {
            string error;
            var list = _courseServices.GetCourseStoresTree(id, CurrentUserId, out error);

            var token = new CourseStoresToken
            {
                CourseId = id
                ,Stores = list
                ,IsValid = String.IsNullOrEmpty(error)
                ,Message = error
            };

            //var token = new CourseStoresToken
            //{
            //    CourseId = id
            //    ,IsValid = true
            //};

            return PartialView("EditCourse/_CourseStores", token);
        }

        public JsonResult GetCourseStoresTree(int id)
        {
            string error;
            var list = _courseServices.GetCourseStoresTree(id, CurrentUserId, out error);

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VideoSelection()
        {
            var user = GetCurrentUser();

           return PartialView("EditCourse/_VideoSelection",user.userId);         
        }  

        //chapters
        public ActionResult ChapterEditForm(int id,int courseId)
        {
            var dto = _courseServices.GetChapterEditDTO(id,courseId);

            return PartialView("EditCourse/_EditChapter",dto);
        }

        public ActionResult ChapterContentEditForm(int id, int contentChapterId,eChapterContentKinds contentKind)
        {
            if (contentChapterId > 0)
            {
                var user = GetCurrentUser();

                switch (contentKind)
                {
                    case eChapterContentKinds.Video:
                        var dto = _courseServices.GetChapterVideoEditDTO(id, contentChapterId,user.userId);
                        return PartialView("EditCourse/_EditVideo", dto);
                    case eChapterContentKinds.Link:
                        var linkDto = _courseServices.GetChapterLinkEditDTO(id,contentChapterId,CourseEnums.eChapterLinkKind.Link);
                        return PartialView("EditCourse/_EditLink", linkDto);
                    case eChapterContentKinds.Document:
                        linkDto = _courseServices.GetChapterLinkEditDTO(id, contentChapterId, CourseEnums.eChapterLinkKind.Document);
                        return PartialView("EditCourse/_EditLink", linkDto);
                }

            }

            return Content(" invalid request data");
        }

        //public ActionResult _ChapterNewContentEditForm(int contentChapterId, eChapterContentKinds contentKind)
        //{
        //    if (contentChapterId <= 0) return Content(" invalid request data");

        //    const int id = -1;

        //    switch (contentKind)
        //    {
        //        case eChapterContentKinds.Video:
        //            var dto = _courseServices.GetChapterVideoEditDTO(id, contentChapterId,CurrentUserId);
        //            return PartialView("EditorTemplates/ChapterVideoEditDTO", dto);
        //        case eChapterContentKinds.Link:
        //            var linkDto = _courseServices.GetChapterLinkEditDTO(id, contentChapterId, CourseEnums.eChapterLinkKind.Link);
        //            return PartialView("EditCourse/_EditLink", linkDto);
        //        case eChapterContentKinds.Document:
        //            linkDto = _courseServices.GetChapterLinkEditDTO(id, contentChapterId, CourseEnums.eChapterLinkKind.Document);
        //            return PartialView("EditCourse/_EditLink", linkDto);
        //    }

        //    return Content(" invalid request data");
        //}

        public ActionResult LoadBcPlayer(long identifier)
        {
            return PartialView("EditCourse/_BcPlayer", identifier);
        }

        public ActionResult _ChapterContents(int id)
        {
            var token = new ChapterContentsToken
            {
                ChapterId = id
            };
            return PartialView("EditCourse/_ChapterContents", token);
        }
        #endregion

        #region bundle editor tabs
        public ActionResult BundleDetails(int id,Guid Uid)
        {
            var token = id < 0 ? new BundleEditDTO(Uid,CurrentUserId) : _courseServices.GetBundleEditDTO(id);

            ViewData[WebConstants.VD_CATEGORY_LOV] = _categoryServices.ActiveCategories();
    
            return PartialView("EditBundle/_BundleDetails", token);
        }

        public ActionResult BundleContent(int id)
        {
            //ViewData[WebConstants.BUNDLE_COURSE_IDS_LOV] = _courseServices.GetBundleCourses(id).Select(x=>x.id).ToList(); 
            var b = _courseServices.GetBundleEditDTO(id);
            return PartialView("EditBundle/_BundleContents", new BundleCoursesPageToken
            {
                id           = id
                ,name        = b.BundleName
                ,IsPurchased = _courseServices.IsBundlePurchased(id)
            });
        }

        
        public ActionResult BundlePricing(int id)
        {
            return PartialView("EditBundle/_Pricing", new BaseEntityDTO
            {
                id = id
            });
        }

        public ActionResult BundlePrice(int id)
        {
            var token = _courseServices.GetBundlePrice(id,Constants.DEFAULT_CURRENCY_ID);

            return PartialView("EditBundle/_BundlePrice", token);
        }

        public ActionResult BundleSales(int id)
        {
            return PartialView("SalesReports/Bundle/_SalesReport", new BaseEntityDTO
            {
                id = id
            });
        }
        #endregion
        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorCoursesList([DataSourceRequest] DataSourceRequest request)
        {
            var list = CurrentUserId >= 0 ? _courseServices.GetAuthorCoursesList(Constants.DEFAULT_CURRENCY_ID, CurrentUserId).OrderBy(x => x.Name).ToArray() : new CourseListDTO[0];
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorBundleList([DataSourceRequest] DataSourceRequest request)
        {
            var list = CurrentUserId >= 0 ? _courseServices.GetAuthorBundlesList(CurrentUserId).OrderBy(x => x.Name).ToArray() : new BundleListDTO[0];
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAuthorCoursesLOV()
        {
            var list = CurrentUserId >= 0 ? _courseServices.GetAuthorCoursesList(Constants.DEFAULT_CURRENCY_ID, CurrentUserId).OrderBy(x => x.Name).ToArray() : new CourseListDTO[0];
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorBundleLOV()
        {
            var list = CurrentUserId >= 0 ? _courseServices.GetAuthorBundlesList(CurrentUserId).OrderBy(x => x.Name).ToArray() : new BundleListDTO[0];
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorReviews([DataSourceRequest] DataSourceRequest request, ReportEnums.ePeriodSelectionKinds? periodSelectionKind)
        {
            var list = CurrentUserId >= 0 ? _courseServices.GetAuthorReviews(CurrentUserId, periodSelectionKind ?? ReportEnums.ePeriodSelectionKinds.all).ToArray() : new ReviewDTO[0];
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCourseChaptersList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetCourseChapters(id).OrderBy(x => x.index).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCourseEditChaptersList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetCourseEidtChaptersList(id).OrderBy(x => x.OrderIndex).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCourseContentsList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetCourseContentsList(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        //
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetChapterVideosList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetChapterVideos(id).OrderBy(x => x.index).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetChapterEditVideosList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetChapterEditVideosList(id).OrderBy(x => x.OrderIndex).ToList();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteChapterVideoFromList([DataSourceRequest] DataSourceRequest request, ChapterVideoEditDTO dto)
        {
            string error;

            if (dto == null) return Json(ModelState.ToDataSourceResult());

            _courseServices.DeleteChapterVideo(dto.VideoId, out error);
          
            return Json(ModelState.ToDataSourceResult());
        }
        
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetChapterLinksList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetChapterLinks(id).OrderBy(x => x.index).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetChapterEditLinksList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetChapterEditLinksList(id).ToList();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteChapterLinkFromList([DataSourceRequest] DataSourceRequest request, ChapterLinkEditDTO dto)
        {
            string error;

            if (dto == null) return Json(ModelState.ToDataSourceResult());

            _courseServices.DeleteChapterLink(dto.LinkId, out error);

            return Json(ModelState.ToDataSourceResult());
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCourseReviews([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetCourseReviews(id);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        #region sales reports
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCourseSales([DataSourceRequest] DataSourceRequest request, int id, int? periodSelectionKind)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();
            var list = _courseServices.GetCourseSales(id, kind, BillingEnums.eOrderLineTypes.SALE);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCourseSubscriptionSales([DataSourceRequest] DataSourceRequest request, int id, int? periodSelectionKind)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();
            var list = _courseServices.GetCourseSales(id, kind, BillingEnums.eOrderLineTypes.SUBSCRIPTION);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBundleSales([DataSourceRequest] DataSourceRequest request, int id, int? periodSelectionKind)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();
            var list = _courseServices.GetBundleSales(id, kind, BillingEnums.eOrderLineTypes.SALE);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBundleSubscriptionSales([DataSourceRequest] DataSourceRequest request, int id, int? periodSelectionKind)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();
            var list = _courseServices.GetBundleSales(id, kind, BillingEnums.eOrderLineTypes.SUBSCRIPTION);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCourseCoupons([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _couponServices.GetCourseCoupons(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBundleCoupons([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _couponServices.GetBundleCoupons(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorCoupons([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _couponServices.GetAuthorCoupons(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        //bundles
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBundleCourses([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _courseServices.GetBundleCourses(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAvailableBundleCourses([DataSourceRequest] DataSourceRequest request,int id)
        {
            var list = CurrentUserId >= 0 ? _courseServices.GetAvailableCourses4Bundle(id).OrderBy(x => x.name).ToArray() : new BundleCourseListDTO[0];
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPeriodUnits(BillingEnums.eBillingPeriodType type)
        {
            var values = new List<NameValue>();

            int max;

            switch (type)
            {
               case BillingEnums.eBillingPeriodType.HOUR:
                    max = 24;
                    break;
               case BillingEnums.eBillingPeriodType.DAY:
                    max = 31;
                    break;
               case BillingEnums.eBillingPeriodType.WEEK:
                    max = 52;
                    break;
               case BillingEnums.eBillingPeriodType.MONTH:
                    max = 12;
                    break;
                default:
                    max = 0;
                    break;
            }

            for (var i = 1; i <= max; i++)
            {
                values.Add(new NameValue{Title = i.ToString(),Value = i});
            }

            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetItemPriceLines([DataSourceRequest] DataSourceRequest request, int id,BillingEnums.ePurchaseItemTypes type)
        {
            var list = _courseServices.GetAllItemPriceLines(id,type,false).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        
        
        //quizzes
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetQuizAvailabilityLOV(int id)
        {
            var cnt = _courseServices.GetCourseEidtChaptersList(id).Count;
            var list = new List<SelectListItem>();

            for (int i = 1; i <= cnt; i++)
            {
                list.Add(new SelectListItem
                {
                    Value = i.ToString()
                    ,Text = "After chapter " + i
                });
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region posts
        //COURSE
        /// <summary>
        /// save event in course details tab
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveCourseDetails(CourseEditDTO dto)
        {
            if (CurrentUserId < 0) return RedirectToAction("NonAuthorized", "Error");

            if (dto != null && ModelState.IsValid)
            {
                string error;

                var isNew = dto.CourseId == -1;
                if(isNew) dto.Status = CourseEnums.CourseStatus.Draft;
                dto.CourseDescription = HttpUtility.HtmlDecode(dto.CourseDescription);
                var result = _courseServices.SaveCourse(ref dto,CurrentUserId, out error,Session.SessionID);

                if (dto.CourseId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

                if (!isNew) return Json(new JsonResponseToken
                                                            {
                                                                success = result
                                                                ,result = new
                                                                {
                                                                    id = dto.CourseId
                                                                    ,name = dto.CourseName
                                                                    ,url = this.GenerateCoursePageUrl(this.CurrentUserFullName(),dto.CourseName,null)
                                                                    ,isNew = false
                                                                }
                                                                ,error = error
                                                            },JsonRequestBehavior.AllowGet);


                SaveUserEvent(CommonEnums.eUserEvents.COURSE_CREATED,String.Format("Course \"{0}\" created",dto.CourseName),null,dto.CourseId);

                _webStoreServices.AddCourse2AuthorStores(CurrentUserId, dto.CourseId, out error);

                return Json(new JsonResponseToken
                {
                    success = result
                   ,result = new
                       {
                           id    = dto.CourseId
                           ,name = dto.CourseName
                           ,url  = this.GenerateCoursePageUrl(this.CurrentUserFullName(),dto.CourseName,null)
                           ,isNew = true
                       }
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });            
        }

       
        //[HttpPost]
        //public ActionResult SaveCoursePrice(CoursePriceDTO dto)
        //{
        //    if (dto != null && ModelState.IsValid)
        //    {
        //        string error;
                
        //        var result = _courseServices.SaveCoursePrice(dto,out error);

                
        //        return Json(new JsonResponseToken
        //        {
        //            success = result
        //           ,error = error
        //        });
        //    }

        //    return Json(new JsonResponseToken
        //    {
        //        success = false
        //       ,error = GetModelStateError(ModelState.Values.ToList())
        //    });            
        //}

        [HttpPost]
        public ActionResult SaveAffiliateCommission(ItemAffiliateCommissionDTO token)
        {

            if (token != null && ModelState.IsValid)
            {
                string error;

                var result = _courseServices.SaveItemAffiliateCommission(token, out error);

                
                return Json(new JsonResponseToken
                {
                    success = result
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });         
        }

        [HttpPost]
        public ActionResult SaveItemPriceLine(PriceLineDTO dto)
        {
            string error;

            switch (dto.PriceType)
            {
                case BillingEnums.ePricingTypes.FREE:
                    var saved = _courseServices.SaveCourseFreePrice(new BaseItemToken{ItemId = dto.ItemId},true, out error);
                    return Json(new JsonResponseToken
                    {
                        success = saved
                        ,error = error
                    });
                case BillingEnums.ePricingTypes.ONE_TIME:
                    dto.PeriodType = null;
                    break;
                case BillingEnums.ePricingTypes.SUBSCRIPTION:
                    dto.PeriodType = BillingEnums.eBillingPeriodType.MONTH ;
                    break;                    
            }

            var result = _courseServices.SavePriceLine(dto, out error);


            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            });
        }

        [HttpPost]
        public ActionResult UpdatePrice(PriceLineDTO dto)
        {
            string error;

            var result = _courseServices.UpdatePriceLine(dto.PriceLineID, dto.Price, out error);

            return Json(new JsonResponseToken
            {
                success = result
               ,error = error
            });
        }

        [HttpPost]
        public ActionResult DeletePriceLine(PriceLineDTO dto)
        {
            
            string error;

            var result = _courseServices.DeletePriceLine(dto.PriceLineID, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            });
        }

        /// <summary>
        /// save course coupon from tab
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveCourseCoupon(CourseCouponDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                                                    return Json(new JsonResponseToken
                                                    {
                                                        success = false
                                                        ,error = GetModelStateError(ModelState.Values.ToList())
                                                    });
            string error;

            var result =_couponServices.SaveCourseCoupon(ref dto, out error);
                
            return Json(new JsonResponseToken
            {
                success = result
                ,result = new
                {
                    id = dto.CouponId
                    ,name = dto.CouponName
                }
                ,error = error
            });
        }
        
        [HttpPost]
        public ActionResult SaveCourseStoreCategory(int courseId,int catId,bool attached)
        {

            string error;
            var result = attached ? _webStoreServices.SaveCategoryItem(catId, courseId, BillingEnums.ePurchaseItemTypes.COURSE, out error) : _webStoreServices.DeleteCategoryItem(catId, courseId, BillingEnums.ePurchaseItemTypes.COURSE, out error);            

            return Json(new JsonResponseToken
            {
                success  = result
                ,error   = error
                ,message = result ? (attached ? "Course attached successfully" : "Course removed successfully") : string.Empty
                ,result  = catId
            }, JsonRequestBehavior.AllowGet);
        }
        

        /// <summary>
        /// delete course from grid
        /// </summary>
        /// <param name="request"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DestroyCourse([DataSourceRequest] DataSourceRequest request, CourseListDTO dto)
        {
            if (dto != null)
            {
                string error;
                _courseServices.DeleteCourse(dto.CourseId, out error);
            }

            return Json(ModelState.ToDataSourceResult());
        }

        /// <summary>
        /// delete coupon from grid event
        /// </summary>
        /// <param name="request"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DestroyCoupon([DataSourceRequest] DataSourceRequest request, CourseCouponDTO dto)
        {
            if (dto != null)
            {
                string error;
                _couponServices.DeleteCoupon(dto.CouponId, out error);
            }

            return Json(ModelState.ToDataSourceResult());
        }

        /// <summary>
        /// delete course from list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteCourse(int? id)
        {
            if (id != null)
            {
                string error;
                var result = _courseServices.DeleteCourse((int) id,out error);

               return Json(new JsonResponseToken
               {
                   success = result
                   ,error = error
               });
            }

            return ErrorResponse("courseId missing");
        }
        
        //BUNDLE
        [HttpPost]
        public ActionResult SaveBundleDetails(BundleEditDTO dto)
        {
            if (CurrentUserId < 0) return RedirectToAction("NonAuthorized", "Error");

            if (dto != null && ModelState.IsValid)
            {
                string error;

                var isNew = dto.BundleId == -1;
                if(isNew) dto.Status=CourseEnums.CourseStatus.Draft;
                
                var coursesAttached = false;
                if (!isNew)
                {
                    coursesAttached = _courseServices.GetBundleCourses(dto.BundleId).ToList().Count > 0;                    
                }

                if(dto.Status==CourseEnums.CourseStatus.Published && (isNew || !coursesAttached)) return ErrorResponse("Publish not allowed.");

                var result = _courseServices.SaveBundle(ref dto,CurrentUserId, out error);

                if (dto.BundleId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

                if (!isNew) return Json(new JsonResponseToken
                                                            {
                                                                success = result
                                                                ,result = new
                                                                {
                                                                    id = dto.BundleId
                                                                    ,name = dto.BundleName
                                                                    ,url = this.GenerateCoursePageUrl(this.CurrentUserFullName(),dto.BundleName,null)
                                                                }
                                                                ,error = error
                                                            },JsonRequestBehavior.AllowGet);


                SaveUserEvent(CommonEnums.eUserEvents.BUNDLE_CREATED,String.Format("Bundle \"{0}\" created",dto.BundleName),null,null,dto.BundleId);

                //_webStoreServices.AddCourse2AuthorStores(CurrentUserId, dto.BundleId, out error);

                return Json(new JsonResponseToken
                {
                    success = result
                   ,result = new
                       {
                           id    = dto.BundleId
                           ,name = dto.BundleName
                           ,url  = this.GenerateCoursePageUrl(this.CurrentUserFullName(),dto.BundleName,null)
                       }
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });            
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteBundle(int? id)
        {
            if (id == null) return ErrorResponse("bundleId missing");

            string error;
            var result = _courseServices.DeleteBundle((int)id, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DestroyBundle([DataSourceRequest] DataSourceRequest request, BundleListDTO dto)
        {
            if (dto == null) return Json(ModelState.ToDataSourceResult());

            string error;
            _courseServices.DeleteBundle(dto.BundleId, out error);

            return Json(ModelState.ToDataSourceResult());
        }

        public JsonResult SaveBundleCoursesOrder(int[] idS)
        {
            string error;
            var result = _courseServices.SaveBundleCoursesOrder(idS,out error);
            return Json(new JsonResponseToken
                {
                    success = result
                    ,message = "Courses order saved"
                    ,error = error
                }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddCourse2Bundle(int bundleId, int courseId)
        {
            string error;
            var result = _courseServices.AddCourse2Bundle(bundleId, courseId, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            },JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveCourseFromBundle(int rowId)
        {
            string error;
            var result = _courseServices.RemoveCourseFromBundle(rowId, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            },JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult SaveBundlePrice(BundlePriceDTO dto)
        {
            if (dto != null && ModelState.IsValid)
            {
                string error;
                
                var result = _courseServices.SaveBundlePrice(dto,out error);

                
                return Json(new JsonResponseToken
                {
                    success = result
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });            
        }
        
        [HttpPost]
        public ActionResult SaveBundleCoupon(BundleCouponDTO dto)
        {
            if (dto != null && ModelState.IsValid)
            {
                string error;

                var result =_couponServices.SaveBundleCoupon(ref dto, out error);
                
                return Json(new JsonResponseToken
                {
                    success = result
                    ,result = new
                       {
                           id = dto.CouponId
                           ,name = dto.CouponName
                       }
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });
        }

        [HttpPost]
        public ActionResult SaveAuthorCoupon(AuthorCouponDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return Json(new JsonResponseToken
                {
                    success = false
                    ,error = GetModelStateError(ModelState.Values.ToList())
                });
            }
                
            string error;
            dto.OwnerUserId = CurrentUserId;
            var result =_couponServices.SaveAuthorCoupon(ref dto, out error);
                
            return Json(new JsonResponseToken
            {
                success = result
                ,result = new
                {
                    id = dto.CouponId
                    ,name = dto.CouponName
                }
                ,error = error
            });
        }
        //public ActionResult SaveBundleCourses(int id,List<int> CoursesCombo)
        //{
        //    string error;
        //    var result = _courseServices.SaveBundleCourses(id, CoursesCombo, out error);
        //    return Json(new JsonResponseToken
        //    {
        //        success = result
        //        ,error = error
        //    }, JsonRequestBehavior.AllowGet);
        //}
        //CHAPTERS
        /// <summary>
        /// save chapter
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>        
        [HttpPost]
        public ActionResult SaveChapterDetails(ChapterEditDTO dto)
        {
            if (dto != null && ModelState.IsValid)
            {
                string error;
                
                var result = _courseServices.SaveChapter(ref dto,out error);

                if (dto.ChapterId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

                return Json(new JsonResponseToken
                {
                    success = result
                   ,result = new
                       {
                           id = dto.ChapterId
                           ,name = dto.Name
                       }
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });            
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveChapter(ChapterEditDTO dto)
        {
            if (dto == null || !ModelState.IsValid) return ErrorResponse("invalid data");
            var isNew = dto.ChapterId < 0;
            string error;
            var result = _courseServices.SaveChapter(ref dto, out error);

            return Json(new JsonResponseToken { success = result, error = error,result = new{id=dto.ChapterId,isNew}});
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveContentChapter(CourseContentToken dto)
        {
            if (dto == null || !ModelState.IsValid) return ErrorResponse("invalid data");
            var isNew = dto.Chapter.ChapterId < 0;
            string error;
            var chapter = dto.Chapter;
            var result = _courseServices.SaveChapter(ref chapter, out error);

            return Json(new JsonResponseToken { success = result, error = error, result = new { id = chapter.ChapterId, isNew } });
        }

        /// <summary>
        /// save chapter order
        /// </summary>
        /// <param name="idS"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveChapterOrder(int[] idS)
        {
            string error;
            var result = _courseServices.SaveChaptersOrder(idS,out error);
            return Json(new JsonResponseToken
                {
                    success = result
                    ,message = "Chapters order saved"
                    ,error = error
                }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveContentOrder(ContentSortToken[] tokens)
        {
            string error;
            var result = _courseServices.SaveContentsOrder(tokens,out error);
            return Json(new JsonResponseToken
                {
                    success = result                    
                    ,error = error
                }, JsonRequestBehavior.AllowGet);
        }
        //

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteChapter(ChapterEditDTO token)
        {
            if (token == null) return ErrorResponse("chapterId missing");

            string error;
            
            var isDeleted = _courseServices.DeleteChapter(token.ChapterId, out error);

            return Json(new JsonResponseToken
            {
                success = isDeleted
                ,result = new { token.ChapterId }
                ,error  = error
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteCourseContent(CourseContentToken token)
        {
            string error;
            var isDeleted = false;
            int id;
            switch (token.Kind)
            {
               case CourseEnums.eCourseContentKind.Chapter:
                    isDeleted = _courseServices.DeleteChapter(token.Chapter.ChapterId, out error);
                    id = token.Chapter.ChapterId;
                    break;
               case CourseEnums.eCourseContentKind.Quiz:
                    isDeleted = _quizAdminServices.AttachQuizToContents(token.Quiz.QuizId,false, out error);
                    error = "not implemented";                    
                    id = token.Quiz.Sid;
                    break;
                default:
                    return ErrorResponse("unknown content type");
            }

            return Json(new JsonResponseToken
            {
                success = isDeleted
                ,result = new { id}
                ,error  = error
            });
        }

        //CHAPTER VIDEO
        /// <summary>
        /// save chapter video from tab form
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveChapterVideo(ChapterVideoEditDTO dto)
        {
            if (dto != null && ModelState.IsValid)
            {
                string error;
                
                var result = _courseServices.SaveChapterVideo(ref dto,out error);

                if (dto.VideoId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

                return Json(new JsonResponseToken
                {
                    success = result
                   ,result = new
                       {
                           id = dto.VideoId
                           ,name = dto.Title
                           ,chapId = dto.ChapterId
                       }
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });            
        }

        /// <summary>
        /// save chapter videos order
        /// </summary>
        /// <param name="idS"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveChapterVideosOrder(int[] idS)
        {
            string error;
            var result = _courseServices.SaveChapterVideosOrder(idS,out error);
            return Json(new JsonResponseToken
                {
                    success = result
                    ,message = "Videos order saved"
                    ,error = error
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// delete chapter video
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteChapterVideo(int? id)
        {
            if (id != null)
            {
                string error;
                var isDeleted = _courseServices.DeleteChapterVideo((int)id, out error);

                return Json(new JsonResponseToken
                {
                    success = isDeleted
                    ,result = new { id }
                    ,error  = error
                });
            }

            return ErrorResponse("videoId missing");
        }

          //CHAPTER LINKS
        /// <summary>
        /// save chapter link/document from tab form
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveChapterLink(ChapterLinkEditDTO dto)
        {
            if (dto != null && ModelState.IsValid)
            {
                string error;
                
                var result = _courseServices.SaveChapterLink(ref dto,out error);

                if (dto.LinkId < 0) return ErrorResponse(error ?? GlobalResources.ERR_CommonMessage);

                return Json(new JsonResponseToken
                {
                    success = result
                   ,result = new
                       {
                           id = dto.LinkId
                           ,name = dto.Title
                           ,chapId = dto.ChapterId                           
                       }
                   ,error = error
                   ,message = result ? (dto.Kind == CourseEnums.eChapterLinkKind.Document ? GlobalResources.ITEM_DOC_Saved : GlobalResources.ITEM_LINK_Saved) : ""
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });            
        }

        /// <summary>
        /// save chapter links order
        /// </summary>
        /// <param name="idS"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveChapterLinksOrder(int[] idS)
        {
            string error;
            var result = _courseServices.SaveChapterLinksOrder(idS,out error);
            return Json(new JsonResponseToken
                {
                    success = result
                    ,message = "Links order saved"
                    ,error = error
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// delete chapter link
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteChapterLink(int? id)
        {
            if (id != null)
            {
                string error;
                var isDeleted = _courseServices.DeleteChapterLink((int)id, out error);

                return Json(new JsonResponseToken
                {
                    success = isDeleted
                    ,result = new { id }
                    ,error  = error
                });
            }

            return ErrorResponse("linkId missing");
        }
        #endregion

        #region wizard
        #region views
        public ActionResult CourseWizard(Guid id)
        {
            if (id == Guid.Empty) return RedirectToAction("CourseWizard", new {id = Guid.NewGuid()});

            var author = GetCurrentUser();

            if (author == null) return Redirect(Url.Content("~/"));

            var isBelong2Author = _courseServices.ValidateAuthorCourseByUid(author.userId, id);

            if (!isBelong2Author)
            {
                return RedirectToAction("NonAuthorized", "Error");
            }

            var wizardDto = _courseWizardServices.LoadCourseWizard(id, author.userId);

            var token = new CourseWizardPageToken
                {
                    user       = author
                    ,title     = wizardDto.CourseId < 0 ? "Create New Course" : "Edit " + wizardDto.Name
                    ,WizardDto = wizardDto
                };

            return View(token);
        }

        public ActionResult MyVideos()
        {
            return View("WizardVideoManager", GetCurrentUser());
        }

        public ActionResult WixCourseWizard()
        {
            var u = this.CurrentUser();

            Logger.Debug("CREATE WIZARD:: " + ( u == null ? "no user found" : "userId:" + CurrentUserId ));

            var author = GetCurrentUser();

            if (author == null) return Redirect(Url.Content("~/"));


            var uid = Guid.NewGuid();

            var wizardDto = _courseWizardServices.LoadCourseWizard(uid, author.userId);

            var token = new CourseWizardPageToken
            {
                 user      = author
                ,title     = wizardDto.CourseId < 0 ? "Create New Course" : "Edit " + wizardDto.Name
                ,WizardDto = wizardDto
            };

            return View("CourseWizard",token);
        }

        public ActionResult ChangeWizardStep(Guid uid, CourseEnums.eWizardSteps nextStep, int? selectedChapterId = null)
        {

            var wizardDto = _courseWizardServices.LoadCourseWizard(uid, CurrentUserId, nextStep,selectedChapterId,Constants.DEFAULT_CURRENCY_ID);

            return PartialView("CourseWizard/_WizardContainer",wizardDto);
        }

        public ActionResult LoadChapterContentsForm(int id, int contentChapterId, eChapterContentKinds contentKind)
        {
            if (contentChapterId <= 0) return Content("invalid request data");

            switch (contentKind)
            {
                case eChapterContentKinds.Video:
                    var dto = _courseServices.GetChapterVideoEditDTO(id, contentChapterId, CurrentUserId);
                    return PartialView("CourseWizard/Forms/_EditVideo", dto);
                case eChapterContentKinds.Link:
                    var linkDto = _courseServices.GetChapterLinkEditDTO(id, contentChapterId, CourseEnums.eChapterLinkKind.Link);
                    return PartialView("CourseWizard/Forms/_EditLink", linkDto);
                case eChapterContentKinds.Document:
                    linkDto = _courseServices.GetChapterLinkEditDTO(id, contentChapterId, CourseEnums.eChapterLinkKind.Document);
                    return PartialView("CourseWizard/Forms/_EditLink", linkDto);
            }

            return Content("invalid request data");
        }
        #endregion

        #region posts
        [HttpPost]
        public ActionResult AddNewChapter(ChapterEditDTO dto,Guid Uid)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return Json(new JsonResponseToken{
                                                    success = false
                                                    ,error = GetModelStateError(ModelState.Values.ToList())
                                                  });
            }
            
            string error;

            var result = _courseServices.SaveChapter(ref dto,out error);

            if (dto.ChapterId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

            return Json(new JsonResponseToken
            {
                success = result
                ,result = new
                            {
                                id = dto.ChapterId
                            }
                ,error = error
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RenameChapter(WizardChapterListEditDTO dto)
        {
            if (dto == null || !ModelState.IsValid) return ErrorResponse("invalid data");

            string error;
            var result = _courseServices.RenameChapter(dto, out error);
            
            return Json(new JsonResponseToken{success = result,error = error});
        }
        //public ActionResult RenameChapter([DataSourceRequest] DataSourceRequest request, WizardChapterListEditDTO dto)
        //{
        //    if (dto == null || !ModelState.IsValid) return Json(ModelState.ToDataSourceResult());

        //    string error;
        //    _courseServices.RenameChapter(dto, out error);

        //    //Return any validation errors if any
        //    return Json(ModelState.ToDataSourceResult());
        //}

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult WizardDeleteChapter([DataSourceRequest] DataSourceRequest request, WizardChapterListEditDTO dto)
        {
            if (dto != null)
            {
                string error;
                _courseServices.DeleteChapter(dto.ChapterId, out error);
            }


            //Return any validation errors if any
            return Json(ModelState.ToDataSourceResult());


        }

        [HttpPost]
        public ActionResult SaveCourseName(WizardCourseNameDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return Json(new JsonResponseToken{
                                                    success = false
                                                    ,error = GetModelStateError(ModelState.Values.ToList())
                                                  });
            }
            
            string error;
            bool result;
            
            if (dto.CourseId < 0)
            {
                //create new course
                var course = new CourseEditDTO
                {
                    Uid                      = dto.Uid
                    ,CourseId                = -1
                    ,AuthorId                = CurrentUserId
                    ,CourseName              = dto.CourseName.TrimString()
                    ,Status                  = CourseEnums.CourseStatus.Draft
                    ,CourseDescription       = HttpUtility.HtmlDecode(dto.CourseDescription)
                    ,Description             = dto.Description.TrimString()
                    ,DisplayOtherLearnersTab = dto.DisplayOtherLearnersTab
                };

                result = _courseServices.SaveCourse(ref course, CurrentUserId, out error,Session.SessionID);

                if (course.CourseId < 0 || !result) return ErrorResponse(error ?? "Something went wrong. Please try again");

                dto.CourseId = course.CourseId;

                SaveUserEvent(CommonEnums.eUserEvents.COURSE_CREATED, String.Format("Wizard Course \"{0}\" created for uid:{1}", dto.CourseName, dto.Uid),null,dto.CourseId,null);

            }
            else
            {
                dto.CourseDescription = HttpUtility.HtmlDecode(dto.CourseDescription);
                result = _courseWizardServices.SaveCourseName(dto,CurrentUserId, out error);    
            }

            if (!result) return Json(new JsonResponseToken
                                    {
                                        success = false
                                        ,result = new
                                        {
                                            courseId = dto.CourseId
                                        }
                                        ,error = error
                                    });

            result = _courseServices.SaveCourseCategories(dto.CourseId, dto.Categories, out error);

            _webStoreServices.AddCourse2AuthorStores(CurrentUserId, dto.CourseId, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,result =  new
                                {
                                    courseId = dto.CourseId
                                }
                ,error = error
            });
        }
        
        [HttpPost]
        public ActionResult SaveCourseVisuals(WizardCourseVisualsDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return Json(new JsonResponseToken{
                                                    success = false
                                                    ,error = GetModelStateError(ModelState.Values.ToList())
                                                  });
            }
            
            string error;

            var result = _courseWizardServices.SaveCourseVisuals(dto,out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            });
        }

        [HttpPost]
        public ActionResult SaveCourseMeta(WizardCourseMetaDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return Json(new JsonResponseToken
                {
                     success = false
                   , error   = GetModelStateError(ModelState.Values.ToList())
                });
            }

            string error;

            var result = _courseWizardServices.SaveCourseMeta(dto, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error  = error
            });
        }

        [HttpPost]
        public ActionResult SaveUserSettings(WizardAboutAuthorDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return Json(new JsonResponseToken
                {
                     success = false
                   , error   = GetModelStateError(ModelState.Values.ToList())
                });
            }

            string error;

            var result = _userAccountServices.UpdateAccountSettings(dto, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error  = error
            });
        }
        
        [HttpPost]
        public ActionResult SaveWizardCoursePrice(WizardCoursePricingDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return Json(new JsonResponseToken
                {
                     success = false
                   , error   = GetModelStateError(ModelState.Values.ToList())
                });
            }

            string error;

            var result = _courseServices.SaveItemAffiliateCommission(new ItemAffiliateCommissionDTO{ItemId = dto.CourseId,ItemType = BillingEnums.ePurchaseItemTypes.COURSE,AffiliateCommission = dto.AffiliateCommission}, out error); //_courseServices.SaveCoursePrice(new CoursePriceDTO {CourseId = dto.CourseId,Price = dto.Price,MonthlySubscriptionPrice = dto.MonthlySubscriptionPrice,IsFree = dto.IsFree,AffiliateCommission = dto.AffiliateCommission}, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error  = error
            });
        }

        [HttpPost]
        public ActionResult PublishCourse(WizardCoursePublishDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return Json(new JsonResponseToken
                {
                     success = false
                   , error   = GetModelStateError(ModelState.Values.ToList())
                });
            }

            string error;

            var result = _courseWizardServices.PublishCourse(dto,CurrentUserId, out error);

            var status = dto.Status == CourseEnums.CourseStatus.Published ? "pub" : "draft";

            return Json(new JsonResponseToken
            {
                success = result
                ,error  = error
                ,result = status
            });
        }
        #endregion

        #region api
        public ActionResult LoadBreadcrumb([DataSourceRequest] DataSourceRequest request, CourseEnums.eWizardSteps last, CourseEnums.eWizardSteps current, int chaptersCnt = 0)
        {
            var list = _courseWizardServices.GetBreadcrumbSteps(last, current,chaptersCnt);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public int GetUserVideosCount()
        {
            try
            {
                var count =  _authorAdminServices.GetAuthorVideosCount(CurrentUserId,false);

                if (count > 0)
                {
                    //refresh cache
                    _authorAdminServices.GetAuthorVideosCount(CurrentUserId);
                }

                return count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetWizardCourseChaptersList([DataSourceRequest] DataSourceRequest request, Guid id)
        {
            var list = _courseServices.GetCourseChapters(id).Select(x => new WizardChapterListEditDTO {ChapterId = x.id,Name = x.name,OrderIndex = x.index}).OrderBy(x=>x.OrderIndex).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveCategories()
        {
            var list = _categoryServices.ActiveCategories();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion
    }
}
