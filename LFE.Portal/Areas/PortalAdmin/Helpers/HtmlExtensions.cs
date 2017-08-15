using System;
using System.Web;
using System.Web.Mvc;
using LFE.Core.Utils;

namespace LFE.Portal.Areas.PortalAdmin.Helpers
{
    public static class HtmlExtensions
    {
        public static string ToAuthorMonthlyStatementUrl(this UrlHelper basic, int year, int month)
        {
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            var relative = url.Action("PaymentsReport","Author",new {area="AuthorAdmin",y=year,m=month});

            if (String.IsNullOrEmpty(relative)) return string.Empty;

            return Utils.GetKeyValue("baseUrl") + relative.Remove(0, 1);
        }
    }
}