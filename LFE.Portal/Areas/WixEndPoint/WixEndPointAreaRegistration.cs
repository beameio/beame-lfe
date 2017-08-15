using System.Web.Mvc;

namespace LFE.Portal.Areas.WixEndPoint
{
    public class WixEndPointAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "WixEndPoint";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
        
            context.MapRoute(
                name: "WixEndPoint_default",
                url: "WixEndPoint/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "LFE.Portal.Areas.WixEndPoint.Controllers" }
            );
        }
    }
}
