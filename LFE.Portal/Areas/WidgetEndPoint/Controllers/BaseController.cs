using System.Web.Routing;
using LFE.Application.Services.Interfaces;
using System.Web.Mvc;

namespace LFE.Portal.Areas.WidgetEndPoint.Controllers
{
    public class BaseController : Portal.Controllers.BaseController
    {
        private readonly IWidgetEndpointServices _widgetEndpointServices;

        #region properties
        public IWidgetEndpointServices WidgetEndpointServices
        {
            get { return _widgetEndpointServices; }
        }
        #endregion

        public BaseController()
        {
            _widgetEndpointServices = DependencyResolver.Current.GetService<IWidgetEndpointServices>();
        }

        #region user
      
        #endregion


        #region common controller services
        #endregion

        public class PlaginAuthorize : AuthorizeAttribute
        {
            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "home" }, { "action", "index" } });
            }
        }

       
    }
}
