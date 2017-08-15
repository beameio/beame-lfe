using System.Linq;
using System.Web.Mvc;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    public class WebStoreController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult FindUsers(string text)
        {
            var users = UserAccountServices.FindUsers(text).OrderBy(x=>x.fullName).ToArray();

            return Json(users,JsonRequestBehavior.AllowGet);
        }
    }
}
