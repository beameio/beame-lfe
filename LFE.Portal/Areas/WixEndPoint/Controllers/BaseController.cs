using System.Web.Mvc;
using LFE.Application.Services.Interfaces;

namespace LFE.Portal.Areas.WixEndPoint.Controllers
{
    public class BaseController : Portal.Controllers.BaseController
    {
        private readonly IWixUserServices _userServices;
        private readonly IWebStoreServices _webStoreServices;
        private readonly IWidgetServices _widgetServices;
        private readonly IAuthorAdminCourseServices _coursesServices;
        private readonly IWebStoreWixServices _webStoreWixService;
        private readonly IWidgetEndpointServices _widgetEndpointServices;


        public IWebStoreWixServices WebstoreWixService
        {
            get { return _webStoreWixService;  }
        }
        public IAuthorAdminCourseServices CoursesServices
        {
            get { return _coursesServices; }
        }
        
        public IWebStoreServices WebStoreServices
        {
            get { return _webStoreServices; }
        } 

        public IWixUserServices UserServices
        {
            get { return _userServices; }
        }

        public IWidgetServices WidgetServices
        {
            get
            {
                return _widgetServices;
            }
        }
        public IWidgetEndpointServices WidgetEndpointServices
        {
            get
            {
                return _widgetEndpointServices;
            }
        }
        #region properties
     
        //public ICourseServices CourseServices { get; private set; } 
        #endregion

        public BaseController()
        {
            _userServices           = DependencyResolver.Current.GetService<IWixUserServices>();
            _webStoreServices       = DependencyResolver.Current.GetService<IWebStoreServices>();
            _widgetServices         = DependencyResolver.Current.GetService<IWidgetServices>();
            _coursesServices        = DependencyResolver.Current.GetService<IAuthorAdminCourseServices>();
            _webStoreWixService     = DependencyResolver.Current.GetService<IWebStoreWixServices>();
            _widgetEndpointServices = DependencyResolver.Current.GetService<IWidgetEndpointServices>();          
        }
       
      
        #region common controller services
               
        #endregion      

       protected override void OnException(ExceptionContext filterContext)
       {
           if (Request.Url == null || Request.Url.Host.ToLower() == "local.lfe.com") return;

           if (filterContext.ExceptionHandled)
           {
               return;
           }
           Response.StatusCode = 500;
           Response.TrySkipIisCustomErrors = true;
           filterContext.Result = new ViewResult
           {
               ViewName = "~/Areas/WixEndPoint/Views/Home/Error.cshtml"
           };
           filterContext.ExceptionHandled = true;
       }       
    }
}
