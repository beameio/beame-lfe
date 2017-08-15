using System.Security.Policy;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.Core.Utils;

namespace LFE.Portal.Areas.Widget
{
    public class WidgetAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Widget";
            }
        }


        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                  name: "Facebook_default",
                  url: "facebook/{action}",
                  defaults: new { controller = "Facebook", action = "Index", AreaName = "Widget" });

            context.MapRoute(
             "WebStore_Checkout",
             "Widget/{trackingId}/Checkout/{action}/{id}",
             new { controller = "Checkout", AreaName = "Widget", id = UrlParameter.Optional, trackingId = UrlParameter.Optional }
                //,new { scheme = "https" }
            );

            context.MapRoute(
             "Web_Checkout",
             "Widget/Checkout/{action}/{id}",
             new { controller = "Checkout", AreaName = "Widget", id = UrlParameter.Optional }
           //  , new { scheme = "https" }
         );

            context.MapRoute(
                 "Wix",
                 "Widget/Wix/{tab}",
                 new { controller = "Wix", action = "Index", AreaName = "Widget", tab = UrlParameter.Optional }
             );

            context.MapRoute(
                "Wix_SingleCourse",
                "Widget/Wix/SingleCourse/{tabName}",
                new { controller = "Wix", action = "SingleCourse", AreaName = "Widget", }
            );

            context.MapRoute(
               "Widget_Item_Index_Default",
               "Widget/Item/Index",
               new { controller = "Item", action = "Index", author = UrlParameter.Optional, itemName = UrlParameter.Optional, trackingId = UrlParameter.Optional, type = BillingEnums.ePurchaseItemTypes.COURSE }
           );
          

            context.MapRoute(
                "Widget_StoreCourse",
                "Widget/{trackingId}/Item/Course/{author}/{itemName}",
                new { controller = "Item", action = "Index", author = UrlParameter.Optional, itemName = UrlParameter.Optional, trackingId = UrlParameter.Optional, type = BillingEnums.ePurchaseItemTypes.COURSE }
            );
            
            context.MapRoute(
                "Widget_Course",
                "Widget/Item/Course/{author}/{itemName}",
                new { controller = "Item", action = "Index", author = UrlParameter.Optional, itemName = UrlParameter.Optional,trackingId=string.Empty, type = BillingEnums.ePurchaseItemTypes.COURSE }
            );

            context.MapRoute(
                "Widget_StoreBundle",
                "Widget/{trackingId}/Item/Bundle/{author}/{itemName}",
                new { controller = "Item", action = "Index", author = UrlParameter.Optional, itemName = UrlParameter.Optional, trackingId = UrlParameter.Optional, type = BillingEnums.ePurchaseItemTypes.BUNDLE }
            );
            
            context.MapRoute(
                "Widget_Bundle",
                "Widget/Item/Bundle/{author}/{itemName}",
                new { controller = "Item", action = "Index", author = UrlParameter.Optional, itemName = UrlParameter.Optional,trackingId=string.Empty, type = BillingEnums.ePurchaseItemTypes.BUNDLE }
            );

            
            context.MapRoute(
                "Web_Payment",
                "Widget/Payment/{action}/{id}",
                new { controller = "Payment", AreaName = "Widget", id = UrlParameter.Optional }
                //,new { scheme = "https" }
            );
            
            context.MapRoute(
                "WebStore_Payment",
                "Widget/{trackingId}/Payment/{action}/{id}",
                new { controller = "Payment", AreaName = "Widget", id = UrlParameter.Optional, trackingId = UrlParameter.Optional }
                //,new { scheme = "https" }
            );
            
            //context.MapRoute(
            //    "Widget_PurchaseComplete",
            //    "Widget/PurchaseComplete/{id}",
            //    new { controller = "Payment", action = "Success", AreaName = "Widget", id = UrlParameter.Optional }
            //    //new { controller = "Payment", action = "PurchaseComplete", AreaName = "Widget", id = UrlParameter.Optional }
            //        //,new { scheme = "https" }
            //);
            
            //context.MapRoute(
            //    "Widget_StorePurchaseComplete",
            //    "Widget/{trackingId}/PurchaseComplete/{id}",
            //    new { controller = "Payment", action = "PurchaseComplete", AreaName = "Widget", id = UrlParameter.Optional, trackingId=UrlParameter.Optional }
            //    //,new { scheme = "https" }
            //);
                  //string courseName, string bundleName, string trackingID, string categoryName, string authorName, bool buySubscription = false
         //   context.MapRoute(
         //    "WebStore_Course_Buy",
         //    "Widget/{trackingID}/Course/{courseName}/Buy",
         //    new { controller = "Payment", action = "Buy", AreaName = "Widget"  }
         //);

         //   context.MapRoute(
         //  "WebStore_Bundle_Buy",
         //  "Widget/{trackingID}/Bundle/{bundleName}/Buy",
         //  new { controller = "Payment", action = "Buy", AreaName = "Widget" }
         //);


            context.MapRoute(
                "SingleCourse",
                "Widget/SingleCourse/{tabName}",
                new { controller = "Wix", action = "SingleCourse", AreaName = "Widget", }
            );

            //context.MapRoute(
            //    "SearchCourses",
            //    "Widget/{trackingID}/SearchCourses/{keyword}/{page}",
            //    new { controller = "Widget", action = "Search", AreaName = "Widget", keyword = UrlParameter.Optional, page = UrlParameter.Optional }
            //);

            context.MapRoute(
                "WebStore_Error",
                "Widget/{trackingID}/Errors/{action}",
                new { controller = "Error", action = "Error", AreaName = "Widget" }
            );


            context.MapRoute(
                "WebStoreScript",
                "Widget/{trackingID}/ParentScript",
                new { controller = "Widget", action = "ParentScript", AreaName = "Widget" }
            );
            
            context.MapRoute(
                "WebStore_Account",
                "Widget/{trackingID}/Account/{action}",
                new { controller = "Account", AreaName = "Widget" }
            );

          
        //    context.MapRoute(
        //    "Wix_WebStore_Course",
        //    "Widget/Wix/Widget/{trackingID}/Course/{action}",
        //    new { controller = "Course", action = "Index", categoryName = UrlParameter.Optional, authorName = UrlParameter.Optional, courseName = UrlParameter.Optional }
        //);

         //   context.MapRoute(
         //    "WebStore_Bundle",
         //    "Widget/{trackingID}/Bundle/{action}",
         //    new { controller = "Course", action = "Bundle", categoryName = UrlParameter.Optional, authorName = UrlParameter.Optional, bundleName = UrlParameter.Optional }
         //);
            context.MapRoute(
               "Widget_UserPurchases",
               "Widget/Users/MyCourses",
               new { controller = "Widget", action = "MyCourses", AreaName = "Widget", categoryName = Constants.USER_COURSES_CATEGORY_NAME}
                //,new { scheme = "https" }
           );
             context.MapRoute(
                "Widget_UserStorePurchases",
                "Widget/Users/{trackingId}/MyCourses",
                new { controller = "Widget", action = "MyCourses", AreaName = "Widget", categoryName = Constants.USER_COURSES_CATEGORY_NAME, trackingId = UrlParameter.Optional }
                //,new { scheme = "https" }
            );

            context.MapRoute(
             "WebStore_Widget",
             "Widget/{trackingID}/Index/{categoryName}/{page}",
             new { controller = "Widget", action = "Index", categoryName = UrlParameter.Optional, page = UrlParameter.Optional });

            context.MapRoute(
           "Wix_WebStore_Widget",
           "Widget/Wix/Widget/{trackingID}/Index/{categoryName}/{page}",
           new { controller = "Widget", action = "Index", categoryName = UrlParameter.Optional, page = UrlParameter.Optional });

            context.MapRoute(
          "Wix_SEO_WebStore_Widget",
          "Widget/Widget/{trackingID}/Index/{categoryName}/{page}",
          new { controller = "Widget", action = "Index", categoryName = UrlParameter.Optional, page = UrlParameter.Optional });



            context.MapRoute(
              "WebStore_Default",
              "Widget/{trackingID}",
              new { controller = "Widget", action = "Index" });


            context.MapRoute(
            "WebStore_Default_Wix",
            "Widget/Wix/Widget/{trackingID}",
            new { controller = "Widget", action = "Index" });
            

            context.MapRoute(
                "WebStore_BundleFull",
                "Widget/{trackingId}/Bundle/{action}/{categoryName}/{author}/{itemName}",
                new { controller = "Item", action = "Index", type = BillingEnums.ePurchaseItemTypes.BUNDLE, trackingId = UrlParameter.Optional, categoryName = UrlParameter.Optional, author = UrlParameter.Optional, itemName = UrlParameter.Optional }
            );


            context.MapRoute(
                "WebStore_CourseFull",
                "Widget/{trackingId}/Course/{action}/{categoryName}/{author}/{itemName}",
                new { controller = "Item", action = "Index", type = BillingEnums.ePurchaseItemTypes.COURSE, trackingId = UrlParameter.Optional, categoryName = UrlParameter.Optional, author = UrlParameter.Optional, itemName = UrlParameter.Optional }
            );


            context.MapRoute(
               "Wix_WebStore_BundleFull",
               "Widget/Wix/Widget/{trackingId}/Bundle/{action}/{categoryName}/{author}/{itemName}",
               new { controller = "Item", action = "Index", type = BillingEnums.ePurchaseItemTypes.BUNDLE, trackingId = UrlParameter.Optional, categoryName = UrlParameter.Optional, author = UrlParameter.Optional, itemName = UrlParameter.Optional }
           );

            context.MapRoute(
            "Wix_SEO_WebStore_BundleFull",
            "Widget/Widget/{trackingId}/Bundle/{action}/{categoryName}/{author}/{itemName}",
            new { controller = "Item", action = "Index", type = BillingEnums.ePurchaseItemTypes.BUNDLE, trackingId = UrlParameter.Optional, categoryName = UrlParameter.Optional, author = UrlParameter.Optional, itemName = UrlParameter.Optional }
        );


            context.MapRoute(
           "Wix_WebStore_CourseFull",
           "Widget/Wix/Widget/{trackingId}/{controller}/{action}/{categoryName}/{author}/{itemName}",
           new { controller = "Item", action = "Index", type = BillingEnums.ePurchaseItemTypes.COURSE, trackingId = UrlParameter.Optional, categoryName = UrlParameter.Optional, author = UrlParameter.Optional, itemName = UrlParameter.Optional }
       );

            context.MapRoute(
           "Wix__SEO_WebStore_CourseFull",
           "Widget/Widget/{trackingId}/{controller}/{action}/{categoryName}/{author}/{itemName}",
           new { controller = "Item", action = "Index", type = BillingEnums.ePurchaseItemTypes.COURSE,trackingId=UrlParameter.Optional, categoryName = UrlParameter.Optional, author = UrlParameter.Optional, itemName = UrlParameter.Optional }
       );
           
            //context.MapRoute(
            // "Widget_CourseProductPage",
            // "Widget/CoursePage/{controller}/{action}/{id}",
            //    new { action = "_CourseProductPage", controller = "Course", id = UrlParameter.Optional }
            //);

            //context.MapRoute(
            //    "Widget_GetFreeCourse",
            //    "Widget/CoursePage/{controller}/{action}/{courseId}/{couponCode}",
            //        new { action = "GetFreeCourse", controller = "Course" }
            //);

            context.MapRoute(
           "Wix_SEO_SingleCourse",
           "Widget/Widget/SingleCourse/{tabName}",
           new { controller = "Wix", action = "SingleCourse", AreaName = "Widget", });


            context.MapRoute(
              "Widget_default",
              "Widget/{controller}/{action}/{id}",
              new { id = UrlParameter.Optional }
          );
          
        }
    }
}
