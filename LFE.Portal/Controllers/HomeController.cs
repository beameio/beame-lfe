using System;
using System.Net;
using System.Net.Http;
using LFE.Portal.Helpers;
using System.Web.Mvc;

namespace LFE.Portal.Controllers
{
    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            var user = this.CurrentUser();
            
            return RedirectToAction(user == null ? "Login" : "UserSettings", "Account");
        }

        public ActionResult MainSiteToolBar()
        {
            return View("MainSiteToolBar");
        }


        public Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }

        [HttpPost]
        public ActionResult Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }
        [AllowAnonymous]
        public HttpResponseMessage HealthCheck()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}
