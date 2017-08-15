using System.Web.Mvc;
// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new AuthorizeAttribute());
        }
    }
}