using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using System.Linq;
using System.Web.Mvc;
using LFE.Core.Utils;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class CourseController : BaseController
    {
        private readonly IWidgetWebStoreServices _webStorePortalServices;
        public IWidgetServices _widgetServices { get; private set; }
        public IWidgetCourseServices _widgetCourseServices { get; private set; }
        public IUserPortalServices UserPortalServices { get; private set; }

        public CourseController()
        {
            _widgetServices         = DependencyResolver.Current.GetService<IWidgetServices>();
            _widgetCourseServices   = DependencyResolver.Current.GetService<IWidgetCourseServices>();
            UserPortalServices      = DependencyResolver.Current.GetService<IUserPortalServices>();
            _webStorePortalServices = DependencyResolver.Current.GetService<IWidgetWebStoreServices>();
        }

        #region views
        public ActionResult CourseTab(string trackingID, string categoryName, string authorName, string courseName, string _escaped_fragment_)
        {
            //string escapedFragment = Request.QueryString["_escaped_fragment_"] as string;

            var baseDto = _widgetCourseServices.FindCourseByUrlName(authorName, courseName);
            var token = _widgetServices.GetCoursePurchaseDTO(Constants.DEFAULT_CURRENCY_ID,baseDto.id, CurrentUserId, trackingID);

          //  string redirectUrl = "";
            switch (_escaped_fragment_.ToLower())
            {
                case "content":
                    return RedirectToAction("GetCourseContentsPartial", "Course", new { Area = "Widget", courseID = token.CourseId });                    
                case "author":
                    return RedirectToAction("GetAuthorContentPartial", "Course", new { Area = "Widget", authorID = token.User.userId, currentCourseID = token.CourseId });                    
                case "reviews":
                    return RedirectToAction("GetCourseReviewsPartial", "Course", new { Area = "Widget", courseID = token.CourseId });                    
                default:
                    return RedirectToAction("GetCourseContentsPartial", "Course", new { Area = "Widget", courseID = token.CourseId });
            }

        }       
        
        public ActionResult Index(string trackingID, string categoryName, string authorName, string courseName)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["_escaped_fragment_"]))
            {
                ViewBag.TabName = Request.QueryString["_escaped_fragment_"];
            }

            var baseDto = _widgetCourseServices.FindCourseByUrlName(authorName,courseName);

            if (baseDto == null)
            {               
                return View("~/Areas/Widget/Views/Shared/Error404.cshtml", trackingID);                
            }

            var token = _widgetServices.GetCoursePurchaseDTO(_webStorePortalServices.GetStoreCurrencyByTrackingId(trackingID), baseDto.id, CurrentUserId, trackingID);

            ViewBag.BackgroundColor = "#FFFFFF";
            if (MainLayoutViewModel.WebStore != null && !string.IsNullOrEmpty(MainLayoutViewModel.WebStore.BackgroundColor))
            {
                ViewBag.BackgroundColor = MainLayoutViewModel.WebStore.BackgroundColor;
            }

            return View("Purchase", token);
        }
        
        public ActionResult _CourseProductPage(int id,string trackingId, bool isPreview = false)
        {
            var token = _widgetServices.GetCoursePurchaseDTO(_webStorePortalServices.GetStoreCurrencyByTrackingId(trackingId), id, CurrentUserId, string.Empty);
            token.ItemState.IsPreview = isPreview;
            return PartialView("Course/_Course", token); 
        }

        public ActionResult _BundleProductPage(int id, string trackingId, bool isPreview = false)
        {
            var token = _widgetServices.GetBundlePurchaseDTO(_webStorePortalServices.GetStoreCurrencyByTrackingId(trackingId), id, CurrentUserId, string.Empty);
            token.ItemState.IsPreview = isPreview;

            return PartialView("Bundle/_BundleProductPage", token);
        }

        public ActionResult _BundleContent(int bundleId)
        {
            var token = _widgetServices.GetBundlePurchaseDTO(Constants.DEFAULT_CURRENCY_ID, bundleId, CurrentUserId, string.Empty);

            return PartialView("Bundle/_BundleContent", token);
        }

        public ActionResult GetAuthorContentPartial(int authorID, int? currentCourseID)
        {
            var model = _widgetServices.GetAuthorContentDTO(Constants.DEFAULT_CURRENCY_ID, authorID);
            return PartialView("Course/_CourseAuthor", model);
        }

        public ActionResult GetCourseContentsPartial(int courseID)
        {
            var token = _widgetServices.GetCoursePurchaseDTO(Constants.DEFAULT_CURRENCY_ID, courseID, CurrentUserId, string.Empty);

            return PartialView("Course/_CourseContent", token);
        }

        public ActionResult GetCourseReviewsPartial(int courseID)
        {
            var token = _widgetServices.GetCourseReviews(courseID);

            return PartialView("Course/_CourseReviews", token);
        }

        public ActionResult GetChapterContentsPartial()
        {
            return PartialView("Course/_ChapterContent");
        }

        public ActionResult _BuyPlaceHolder(bool isWixDraft)
        {
            return PartialView("~/Areas/Widget/Views/Shared/Course/_BuyPlaceHolder.cshtml", isWixDraft);
        }

        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetChapterLinks([DataSourceRequest] DataSourceRequest request, int id, CourseEnums.eChapterLinkKind kind)
        {
            var list = _widgetCourseServices.GetChapterLinks(id, kind).OrderBy(x => x.index).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetBundleCourses([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _widgetCourseServices.GetBundleCoursesList(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion
        
     
        
    }
}
