using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.DataTokens;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class StyleController : Controller
    {
        private readonly IWidgetServices _widgetServices;

        public StyleController()
        {
            _widgetServices = DependencyResolver.Current.GetService<IWidgetServices>();
        }

        public ActionResult WidgetCss(string trackingID, string lastUpdate)
        {
            var webStore = _widgetServices.GetWidgetStoreDto(trackingID);
            Response.ContentType = "text/css";
            return View("~/Areas/Widget/Views/Shared/Styles/WidgetCss.cshtml", webStore);
        }

        public ActionResult CourseIntroCss(string trackingID, string lastUpdate)
        {
            var webStore = _widgetServices.GetWidgetStoreDto(trackingID);
            Response.ContentType = "text/css";
            return View("~/Areas/Widget/Views/Shared/Styles/CourseIntroCss.cshtml", webStore);

        }

    }
}
