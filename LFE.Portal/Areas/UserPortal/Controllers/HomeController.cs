using System.Web.Mvc;

namespace LFE.Portal.Areas.UserPortal.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult _Header()
        {
            return PartialView("_Header");
        }
        public ActionResult _Footer()
        {
            return PartialView("_Footer");
        }
        public ActionResult _NotificationMenu()
        {
            return PartialView("_NotificationMenu");
        }
        public ActionResult _UserCoursesCombo()
        {
            return PartialView("_UserCoursesCombo");
        }
        public ActionResult _FeedWindow()
        {
            return PartialView("_FeedWindow");
        }
    }
}
