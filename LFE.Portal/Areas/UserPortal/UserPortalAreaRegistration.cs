using System.Web.Mvc;

namespace LFE.Portal.Areas.UserPortal
{
    public class UserPortalAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "UserPortal";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {



            context.MapRoute(
                "UserPortal_course",
                "courses/{authorName}/{courseName}/{mode}",
                new { AreaName = "UserPortal", controller = "Course", action = "CourseViewer", authorName = UrlParameter.Optional, courseName = UrlParameter.Optional, mode = UrlParameter.Optional },
                new[] { "LFE.Portal.Areas.UserPortal.Controllers" }
            );

            context.MapRoute(
              "UserPortal_bundle",
              "bundles/{authorName}/{bundleName}/{mode}",
              new { AreaName = "UserPortal", controller = "Course", action = "BundleViewer", authorName = UrlParameter.Optional, bundleName = UrlParameter.Optional, mode = UrlParameter.Optional },
              new[] { "LFE.Portal.Areas.UserPortal.Controllers" }
          );

            context.MapRoute(
                "UserPortal_default",
                "UserPortal/{controller}/{action}/{id}",
                new { id = UrlParameter.Optional }
            );
        }
    }
}
