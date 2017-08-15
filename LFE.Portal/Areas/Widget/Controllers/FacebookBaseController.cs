using LFE.Application.Services.Interfaces;
using LFE.DataTokens;
using System;
using System.Web;
using System.Web.Mvc;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class FacebookBaseController : Portal.Controllers.BaseController
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


        public FacebookBaseController()
        {

            if (TempData["MainLayoutViewModel"] != null)
            {
                MainLayoutViewModel = TempData["MainLayoutViewModel"] as BaseModelViewToken;
                return;
            }
            
            WidgetServices = DependencyResolver.Current.GetService<IWidgetServices>();
     
            //get iframe size
            int? width = 810;
            int? height = 760;
            if (!string.IsNullOrEmpty(HttpContext.Request.QueryString["width"]))
            {
                width = Convert.ToInt32(HttpContext.Request.QueryString["width"]);
            }
            if (!string.IsNullOrEmpty(HttpContext.Request.QueryString["height"]))
            {
                height = Convert.ToInt32(HttpContext.Request.QueryString["height"]);
            }
            var viewMode = "site";

            if (!string.IsNullOrEmpty(HttpContext.Request.QueryString["viewmode"]))
            {
                viewMode = HttpContext.Request.QueryString["viewmode"];
            }

            if (ViewData["MainLayoutViewModel"] != null) return;

            string trackingId = null;

            if (HttpContext.Request.RequestContext.RouteData.Values["trackingID"] != null && !String.IsNullOrEmpty(HttpContext.Request.RequestContext.RouteData.Values["trackingID"] as string))
            {
                trackingId = HttpContext.Request.RequestContext.RouteData.Values["trackingID"] as string;
            }
            else if (HttpContext.Request.QueryString["trackingId"] != null && !String.IsNullOrEmpty(HttpContext.Request.QueryString["trackingId"]))
            {
                trackingId = HttpContext.Request.QueryString["trackingId"];
            }

            SetMainLayoutViewModel(trackingId,viewMode,width,height);
           
        }

        public void SetMainLayoutViewModel(string trackingId,string viewMode = null,int? width = null, int? height = null)
        {
            //if (!string.IsNullOrEmpty(HttpContext.Request.RequestContext.RouteData.Values["trackingID"] as string))
            //{
            //    var trackingID = HttpContext.Request.RequestContext.RouteData.Values["trackingID"] as string;

            //    //TODO fix this exception(route Widget_CourseProductPage for partial course product page)
            //    if (trackingID == "CoursePage") return;

            //    var categoryUrlName = HttpContext.Request.RequestContext.RouteData.Values["categoryName"] as string;
            //    MainLayoutViewModel = WidgetServices.GetFacebookBaseModelToken(trackingID, categoryUrlName, viewMode, width, height);
            //}
            //else//set empty place holder view model
            //{
            //    MainLayoutViewModel = WidgetServices.GetFacebookBaseModelToken(HttpContext.Request.QueryString["trackingId"], "", viewMode, width, height);
            //}


            var categoryUrlName = HttpContext.Request.RequestContext.RouteData.Values["categoryName"] as string;
            MainLayoutViewModel = WidgetServices.GetFacebookBaseModelToken(trackingId,categoryUrlName, viewMode, width, height);
            ViewData["MainLayoutViewModel"] = MainLayoutViewModel;
            TempData["MainLayoutViewModel"] = MainLayoutViewModel;
        }
         
    }
}
