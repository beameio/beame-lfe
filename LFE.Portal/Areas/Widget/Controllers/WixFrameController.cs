using System.Web.Mvc;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class WixFrameController : Controller
    {
        
        public ActionResult Frame()
        {            
            string wixInstance = Request.QueryString["instance"];
            return View("~/Areas/Widget/Views/Wix/Frame.cshtml", null, wixInstance);
        }
    }
}
