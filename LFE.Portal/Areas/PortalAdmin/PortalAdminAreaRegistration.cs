using System.Web.Mvc;

namespace LFE.Portal.Areas.PortalAdmin
{
    public class PortalAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PortalAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PortalAdmin_default",
                "PortalAdmin/{controller}/{action}/{id}",
                new { action = "Index",controller="Home", id = UrlParameter.Optional },
                namespaces: new[] { "LFE.Portal.Areas.PortalAdmin.Controllers" }
            );
        }
    }
}
