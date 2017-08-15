using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LFE.Portal.Areas.Widget.Helpers
{
    public static class HtmlExtensions
    {

        public static MvcHtmlString ToJson(this HtmlHelper html, object obj)
        {
            var serializer = new JavaScriptSerializer();
            return MvcHtmlString.Create(serializer.Serialize(obj));
        }

        public static MvcHtmlString ToJson(this HtmlHelper html, object obj, int recursionDepth)
        {
            var serializer = new JavaScriptSerializer {RecursionLimit = recursionDepth};
            return MvcHtmlString.Create(serializer.Serialize(obj));
        }      
    }
}