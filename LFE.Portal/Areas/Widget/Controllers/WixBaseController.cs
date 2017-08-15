using LFE.Application.Services.Interfaces;
using LFE.DataTokens;
using LFE.Portal.Helpers;
using System;
using System.Web;
using System.Web.Mvc;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class WixBaseController : Portal.Controllers.BaseController
    {
        
        #region properties
        public IWidgetServices WidgetServices { get; private set; }

        public BaseModelViewToken MainLayoutViewModel { get; set; }


        public new HttpContextBase HttpContext
        {
            get
            {
                var context =
                    new HttpContextWrapper(System.Web.HttpContext.Current);
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


        public WixBaseController()
        {
            if (TempData["MainLayoutViewModel"] != null)
            {
                MainLayoutViewModel = TempData["MainLayoutViewModel"] as BaseModelViewToken;
                return;
            }
            
            WidgetServices = DependencyResolver.Current.GetService<IWidgetServices>();
     
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
               // width = Convert.ToInt32(widthStr);
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

            if (ViewData["MainLayoutViewModel"] != null) return;

            var trackingID = (HttpContext.Request.RequestContext.RouteData.Values["trackingID"] as string) ?? HttpContext.Request.QueryString["trackingId"];

            if (!string.IsNullOrEmpty(trackingID))
            {
                //TODO fix this exception(route Widget_CourseProductPage for partial course product page)
                if (trackingID == "CoursePage") return;

                var categoryUrlName = (HttpContext.Request.RequestContext.RouteData.Values["categoryName"] as string) ?? HttpContext.Request.QueryString["categoryName"];
                MainLayoutViewModel = WidgetServices.GetBaseModelToken(trackingID, categoryUrlName, viewMode, width, height);
            }
            else//set empty place holder view model
            {
                    
                var currentUserIdStr = "";
                if (this.CurrentUser() != null)
                {
                    currentUserIdStr = this.CurrentUser().UserId.ToString();
                }
                MainLayoutViewModel = WidgetServices.GetWixBaseModelToken(HttpContext.Request.QueryString["instance"], "", viewMode, width, height, currentUserIdStr);
            }


            ViewData["MainLayoutViewModel"] = MainLayoutViewModel;
            TempData["MainLayoutViewModel"] = MainLayoutViewModel;
        }      
    }
}
