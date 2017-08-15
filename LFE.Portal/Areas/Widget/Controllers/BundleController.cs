using LFE.Application.Services.Interfaces;
using System.Web.Mvc;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class BundleController : BaseController
    {
        public IWidgetServices _widgetServices { get; private set; }
        public IWidgetCourseServices _widgetCourseServices { get; private set; }
        public IUserPortalServices UserPortalServices { get; private set; }

        public BundleController()
        {
            _widgetServices       = DependencyResolver.Current.GetService<IWidgetServices>();
            _widgetCourseServices = DependencyResolver.Current.GetService<IWidgetCourseServices>();
            UserPortalServices    = DependencyResolver.Current.GetService<IUserPortalServices>();
        }


        public ActionResult Index(string trackingID, string categoryName, string authorName, string bundleName)
        {
            var pageToken = new UserPortal.Models.BundleViewerPageToken();

            var baseDto = _widgetCourseServices.FindBundleByUrlName(authorName, bundleName);

            if (baseDto == null)
            {
                pageToken.IsValid = false;

                pageToken.Message = "Bundle not found";

                return View("~/Areas/Widget/Views/Bundle/Bundle.cshtml", pageToken);
            }

            pageToken.IsValid = true;
            pageToken.Bundle = baseDto;

            var itemState = _widgetCourseServices.GetBundleAccessState4User(CurrentUserId, baseDto.BundleId);
            itemState.IsPreview = false;

            pageToken.ItemState = itemState;

            var loadViewer = !itemState.IsPreview && (itemState.IsOwnedByUser || (itemState.IsAccessAllowed && itemState.IsPublished));

            // pageToken.Author = _userPortalServices.GetUserProfileDto(baseDto.AuthorId);

            if (loadViewer)
            {
                pageToken.BundleCourses = _widgetCourseServices.GetBundleCoursesList(baseDto.BundleId);
            }


            ViewBag.BackgroundColor = "#FFFFFF";
            if (MainLayoutViewModel.WebStore != null && !string.IsNullOrEmpty(MainLayoutViewModel.WebStore.BackgroundColor))
            {
                ViewBag.BackgroundColor = MainLayoutViewModel.WebStore.BackgroundColor;
            }


            return View("~/Areas/Widget/Views/Bundle/Bundle.cshtml", pageToken);
        }

    }
}
