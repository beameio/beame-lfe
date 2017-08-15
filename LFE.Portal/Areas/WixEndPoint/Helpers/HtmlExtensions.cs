using System.Web;
using System.Web.Mvc;

namespace LFE.Portal.Areas.WixEndPoint.Helpers
{
    public static class HtmlExtensions
    {
        public static string ToWixLoginHandlerUrl(this UrlHelper basic, string instanceToken, string compId, string origCompId)
        {
            var u = new UrlHelper(HttpContext.Current.Request.RequestContext);

            return u.Action("Index", "Home", new { area = "WixEndPoint", instance = instanceToken, compId, origCompId });
        }
    }
}