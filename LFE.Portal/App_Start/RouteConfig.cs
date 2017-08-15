using System.Web.Mvc;
using System.Web.Routing;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*staticfile}", new { staticfile = @".*\.(css|js|gif|jpg|jpeg|png|xml|php|exe|aspx|txt)(/.*)?" });
            routes.IgnoreRoute("{*robotstxt}", new { robotstxt = @"(.*/)?robots.txt(/.*)?" });


            //routes.MapHttpRoute(
            //  name: "WidgetEndPointApi",
            //  routeTemplate: "WidgetEndPoint/api/{controller}/{action}/{id}",
            //  defaults: new { id = RouteParameter.Optional, area = "WidgetEndPointApi" }
            //);

            routes.MapRoute(
                "MainSiteToolBar",
                "MainSite/ToolBar",
                new { controller = "Home", action = "MainSiteToolBar", AreaName = "" },
                namespaces: new[] { "LFE.Portal.Controllers" }
            );

            routes.MapRoute(name: "Default", url: "{controller}/{action}/{id}", defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }, namespaces: new[] { "LFE.Portal.Controllers" });
           
        }
    }
}