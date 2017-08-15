using System.Web.Http;

// ReSharper disable once CheckNamespace
namespace LFE.Portal.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {


            config.Routes.MapHttpRoute(
               name: "WixCoursesApiUpdateLog",
               routeTemplate: "api/wix/course/updatelog/{id}",
               defaults: new { controller = "WixCourse", action= "UpdateCourseChangeLog", id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
               name: "WixCoursesApiGetLastUpdate",
               routeTemplate: "api/wix/course/get/{id}",
               defaults: new { controller = "WixCourse", action = "GetLastCourseChange", id = RouteParameter.Optional }
           );

            config.Routes.MapHttpRoute(
              name: "WixStoreApiUpdateStore",
              routeTemplate: "api/wix/webstore/update/{id}",
              defaults: new { controller = "WixStore", action= "UpdateWebStoreChangeLog", id = RouteParameter.Optional }
          );
            config.Routes.MapHttpRoute(
             name: "WixStoreApiUpdateStoreCourse",
             routeTemplate: "api/wix/webstore/updatecourse/{id}",
             defaults: new { controller = "WixStore", action = "UpdateCourseWebStoresChangeLog", id = RouteParameter.Optional }
         );

            config.Routes.MapHttpRoute(
            name: "WixStoreApiGetStoreChange",
            routeTemplate: "api/wix/webstore/get/{id}",
            defaults: new { controller = "WixStore", action = "GetLastWebStoreChange", id = RouteParameter.Optional }
        );
            config.Routes.MapHttpRoute(
             name: "WixStoreApiGetByTrack",
             routeTemplate: "api/wix/webstore/getByTrack/{id}",
             defaults: new { controller = "WixStore", action = "GetLastStoreChangeByTrackingId", id = RouteParameter.Optional }
         );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
        }
    }
}