using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.DataTokens;
using LFE.Portal.Areas.UserPortal.Models;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class BaseController : Portal.Controllers.BaseController
    {
        #region properties
        public IWidgetServices WidgetServices { get; private set; }
        public IWidgetUserServices WidgetUserServices { get; set; }
        public BaseModelViewToken MainLayoutViewModel { get; set; }


        public new HttpContextBase HttpContext
        {
            get
            {
                var context = new HttpContextWrapper(System.Web.HttpContext.Current);
                return context;
            }
        }
        #endregion

        protected override void OnException(ExceptionContext filterContext)
        {
            if (Request.IsLocal) return;

            if (filterContext.ExceptionHandled)
            {
                return;
            }
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Areas/Widget/Views/Shared/Error.cshtml"
            };
            filterContext.ExceptionHandled = true;
        }


        public BaseController()
        {
            WidgetServices = DependencyResolver.Current.GetService<IWidgetServices>();
            WidgetUserServices = DependencyResolver.Current.GetService<IWidgetUserServices>();

            if (String.IsNullOrEmpty(HttpContext.Request.RequestContext.RouteData.Values["trackingID"] as string) && TempData["MainLayoutViewModel"] != null)
            {
                MainLayoutViewModel = TempData["MainLayoutViewModel"] as BaseModelViewToken;
                return;
            }

            var trackingID = (HttpContext.Request.RequestContext.RouteData.Values["trackingID"] as string) ?? HttpContext.Request.QueryString["trackingId"];


            var categoryUrlName = HttpContext.Request.RequestContext.RouteData.Values["categoryName"] as string ?? HttpContext.Request.QueryString["categoryName"];
            
            //get iframe size
            int? width = null;
            int? height = null;
            if (!string.IsNullOrEmpty(HttpContext.Request.QueryString["width"]))
            {
                var widthStr = HttpContext.Request.QueryString["width"];
                if (widthStr.Contains(","))
                {
                    widthStr = widthStr.Substring(0, widthStr.IndexOf(",", StringComparison.Ordinal));
                }
                //width = Convert.ToInt32(widthStr);
                int w;
                if (Int32.TryParse(widthStr, out w)) width = w;
            }
            if (!string.IsNullOrEmpty(HttpContext.Request.QueryString["height"]))
            {
                //height = Convert.ToInt32(HttpContext.Request.QueryString["height"]);
                int h;
                if (Int32.TryParse(HttpContext.Request.QueryString["height"], out h)) height = h;
            }
            var viewMode = "site";
            if (!string.IsNullOrEmpty(HttpContext.Request.QueryString["viewmode"]))
            {
                viewMode = HttpContext.Request.QueryString["viewmode"];
            }
            
            MainLayoutViewModel = WidgetServices.GetBaseModelToken(trackingID, categoryUrlName, viewMode, width, height);

            ViewData["MainLayoutViewModel"] = MainLayoutViewModel;
            TempData["MainLayoutViewModel"] = MainLayoutViewModel;
        }


        public AuthorProfilePageToken GetAuthorProfilePageToken(int id)
        {
            var token = new AuthorProfilePageToken
            {
                AuthorItems = WidgetUserServices.GetAuthorItems(id, CurrentUserId,MainLayoutViewModel != null && MainLayoutViewModel.WebStore != null ? MainLayoutViewModel.WebStore.TrackingID : string.Empty).OrderByDescending(x => x.NumSubscribers).ThenBy(x => x.ItemName).ToList(),
                PageSize = 4
            };

            var userProfile = WidgetUserServices.GetAuthorProfileDto(id);

            var profile = new UserProfileCartToken
            {
                Profile     = userProfile
                ,TotalTeach = token.AuthorItems.Count
            };

            token.ProfileCart = profile;

            token.IsValid = true;

            return token;
        }

    }
}
