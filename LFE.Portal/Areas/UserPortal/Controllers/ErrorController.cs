using System.Web.Mvc;

namespace LFE.Portal.Areas.UserPortal.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult NonAuthorized()
        {
            return View();
        }

        public ActionResult ErrorResult()
        {
            return View("Error");
        }

    }
}
