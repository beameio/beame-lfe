using System.Web.Mvc;

namespace LFE.Portal.Areas.WidgetEndPoint
{
    public class WidgetEndPointAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "WidgetEndPoint";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "WidgetEndPoint_default_short",
                "WidgetEndPoint/{id}",
                new { action = "Index", controller = "Home", AreaName = "WidgetEndPoint", id = UrlParameter.Optional },
                new[] { "LFE.Portal.Areas.WidgetEndPoint.Controllers" }
            );

            context.MapRoute(
                "WidgetEndPoint_default",
                "WidgetEndPoint/{controller}/{action}/{id}",
                new { action = "Index", controller = "Home", AreaName = "WidgetEndPoint", id = UrlParameter.Optional },
                new[] { "LFE.Portal.Areas.WidgetEndPoint.Controllers" }
            );

          
        }
    }
}
