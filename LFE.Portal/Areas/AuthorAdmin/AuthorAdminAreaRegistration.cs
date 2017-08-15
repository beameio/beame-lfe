using System.Web.Mvc;

namespace LFE.Portal.Areas.AuthorAdmin
{
    public class AuthorAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "AuthorAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(name: "Logout", url: "AuthorAdmin/Account/LogOff", defaults: new { controller = "Account", action = "Logoff" }, namespaces: new[] { "LFE.Portal.Controllers" });

            context.MapRoute(
                name: "AuthorAdmin_default",
                url: "AuthorAdmin/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "LFE.Portal.Areas.AuthorAdmin.Controllers" }
            );
        }
    }
}
