using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LFE.Portal.Controllers
{
    public class ErrorController : BaseController
    {
        //
        // GET: /Error/

        public ActionResult General()
        {
            return View();
        }

        public ActionResult HttpError404()
        {
            return View();
        }

        public ActionResult HttpError500()
        {
            return View();
        }

    }
}
