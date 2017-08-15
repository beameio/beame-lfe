using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.Core.Extensions;
using LFE.DataTokens;
using System.Web.Routing;
using System.Web;
using LFE.Core.Enums;
using LFE.Portal.Areas.Widget.Models;
using LFE.Core.Utils;
using LFE.Portal.Helpers;


namespace LFE.Portal.Areas.Widget.Controllers
{
    public class WidgetController : BaseController
    {
      
        private readonly IWidgetWebStoreServices _webStorePortalServices;
        public WidgetController()
        {
            _webStorePortalServices = DependencyResolver.Current.GetService<IWidgetWebStoreServices>();
        }

        public ActionResult MainSiteToolBar(bool? isLogout = false)
        {
            if (Convert.ToBoolean(isLogout))
            {
                ViewBag.IsLogout = true;
            }
            HttpContext.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            return View("~/Areas/Widget/Views/MainSite/MainSiteToolBar.cshtml");
        }

        public ActionResult Search(string trackingID, string keyword, int? page, int? width, int? height)
        {
            if (MainLayoutViewModel.Status != WebStoreEnums.StoreStatus.Published)
            {
                return View("~/Areas/Widget/Views/Widget/NotFound.cshtml", new NotFoundToken{FirstMessage =  "The Web Store you were looking for was not publish. If you are the "});
            }
            //int? userID
            //string wixViewMode,
            var userID = CurrentUserId < 0 ? (int?)null : CurrentUserId;
            var pagesize = WidgetServices.NumItemsInPage(width, height);
            if (page == null)
            {
                page = 1;
            }

            ViewBag.Keyword = keyword;

            var model = WidgetServices.SearchModelView(_webStorePortalServices.GetStoreCurrencyByTrackingId(trackingID), trackingID, (int)page, pagesize, userID, MainLayoutViewModel.WixViewMode, keyword);
            return model == null ? View("~/Areas/Widget/Views/Shared/Error.cshtml") : View(model);
        }

        [EnableCompression]
        public ActionResult MyCourses(string trackingId)
        {
            if (MainLayoutViewModel == null)
            {
                MainLayoutViewModel = new BaseModelViewToken
                {
                    WebStore        = null
                    ,CategoriesList = new List<WidgetCategoryDTO>()
                    ,IsValid        = true
                    ,CategoryName   = Constants.USER_COURSES_CATEGORY_NAME
                    ,TrackingId     = trackingId
                };

                ViewData["MainLayoutViewModel"] = MainLayoutViewModel;
                TempData["MainLayoutViewModel"] = MainLayoutViewModel;    
            }
            
            var pagesize = WidgetServices.NumItemsInPage(null, null);

            var model = WidgetServices.GetIndexModelView(_webStorePortalServices.GetStoreCurrencyByTrackingId(trackingId), trackingId, 1, string.Empty, pagesize, null, "mycourses", CurrentUserId, "site");

            if (model.ItemsList.Count != 1) return View("Index",model);

            var item = model.ItemsList[0];

          //  var url = Url.ActionString("Index", "Item", new RouteValueDictionary { { "area", "Widget" }, { "type", item.ItemType }, { "author", item.AuthorName.OptimizedUrl() }, { "itemName", item.ItemName.OptimizedUrl() }, { "trackingId", trackingId } });

          //  return new RedirectResult(url,true);

            return RedirectToAction("Index", "Item", new {area="Widget" ,type = item.ItemType, author = item.AuthorName.OptimizedUrl(), itemName = item.ItemName.OptimizedUrl(), trackingId });            
        }


        [EnableCompression]
        public ActionResult Index(string trackingID, string categoryName, int? page, string sort, int? width, int? height)
        {
            //in case request came from wix to css page and not to inner page 
            if (!string.IsNullOrEmpty(Request.QueryString["instance"]) && !Request.Url.AbsoluteUri.ToLower().Contains("widget/wix/widget"))
            {
                return new MVCTransferResult(new {
                                                    controller   = "Wix", 
                                                    action       = "Index",
                                                    instance     = Request.QueryString["instance"],
                                                    trackingID   = (HttpContext.Request.RequestContext.RouteData.Values["trackingID"] as string) ?? HttpContext.Request.QueryString["trackingId"], 
                                                    categoryName = HttpContext.Request.RequestContext.RouteData.Values["categoryName"],
                                                    width        = HttpContext.Request.QueryString["width"],
                                                    height       = HttpContext.Request.QueryString["height"],
                                                    viewmode     = HttpContext.Request.QueryString["viewmode"],
                                                    compId       = HttpContext.Request.QueryString["compId"],
                                                    cacheKiller  = HttpContext.Request.QueryString["cacheKiller"],
                                                    section_url  = HttpContext.Request.QueryString["section-url"],
                                                    target       = HttpContext.Request.QueryString["target"]
                                                  });
            }

            if (MainLayoutViewModel==null || (MainLayoutViewModel.Status != WebStoreEnums.StoreStatus.Published && !MainLayoutViewModel.IsUserPurchasesCategory))
            {
                return RedirectToAction("NotFound", "Error", new { status = MainLayoutViewModel != null ? MainLayoutViewModel.Status : WebStoreEnums.StoreStatus.Unknown, Area = "Widget" });
            }

            var userID = CurrentUserId < 0 ? (int?) null : CurrentUserId;

            //if only one course exist in store don't display the index
            if (MainLayoutViewModel.IsSingleCourseStore && !MainLayoutViewModel.IsUserPurchasesCategory)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["_escaped_fragment_"]))
                {
                    ViewBag.TabName = Request.QueryString["_escaped_fragment_"];
                }

                ItemProductPageToken token;

                //if (MainLayoutViewModel.NumCourses == 0)
                //{
                //    token = WidgetServices.GetPlaceHolderItemInfoToken();
                //    return View("~/Areas/Widget/Views/Wix/PlaceHolder.cshtml", token);
                //}

                if (MainLayoutViewModel.NumCourses > 0 && MainLayoutViewModel.CategoriesList != null && MainLayoutViewModel.CategoriesList.Any())
                {
                    // get the single course
                    token = WidgetServices.GetWixDefaultItem( MainLayoutViewModel.CategoriesList.Select(x => x.WebStoreCategoryID).ToList(), CurrentUserId);
                    token.TrackingID = trackingID;
                    return View("~/Areas/Widget/Views/Wix/PlaceHolder.cshtml", token);
                }

                //if no courses were defined display template place holder.
                token = WidgetServices.GetPlaceHolderItemInfoToken();
                token.TrackingID = trackingID;
                return View("~/Areas/Widget/Views/Wix/PlaceHolder.cshtml", token);
            }
            //display Index
            var pagesize = WidgetServices.NumItemsInPage(width, height);

            if (page == null)
            {
                page = 1;
            }

            //if main lfe site - display all courses that exist in lfe
            IndexModelViewToken model;
            if ((MainLayoutViewModel.ParentURL.ToLower().StartsWith("http://" + Request.Url.Host.ToLower()) || MainLayoutViewModel.ParentURL.ToLower().StartsWith(Utils.GetKeyValue("baseUrl"))) && (string.IsNullOrEmpty(categoryName) || categoryName.ToLower() == "_all"))
            {                
                model = WidgetServices.GetAllCoursesView(_webStorePortalServices.GetStoreCurrencyByTrackingId(trackingID), (int)page, sort, pagesize, userID);
            }
            else
            {
                model = WidgetServices.GetIndexModelView(_webStorePortalServices.GetStoreCurrencyByTrackingId(trackingID), trackingID, (int)page, sort, pagesize, MainLayoutViewModel.Category != null ? (int?)MainLayoutViewModel.Category.WebStoreCategoryID : null, categoryName, userID, MainLayoutViewModel.WixViewMode);

                if (model == null) return View("~/Areas/Widget/Views/Shared/Error.cshtml");

                if (model.ItemsList.Count.Equals(0)) return View(model);

                //if only one course or bundle in category redirect to product page
                if (model.ItemsList.Count > 1 || page != 1) return View(model); //|| page != 1
                
                var item = model.ItemsList[0];

                return RedirectToAction("Index", "Item", new { type = item.ItemType, author = item.AuthorName.OptimizedUrl(), itemName = item.ItemName.OptimizedUrl(), trackingId = MainLayoutViewModel.WebStore != null ? MainLayoutViewModel.WebStore.TrackingID : string.Empty,mode=string.Empty, width = MainLayoutViewModel.Width, height = MainLayoutViewModel.Height,compId = this.GetWixCompId() });
            }

            return model == null ? View("~/Areas/Widget/Views/Shared/Error.cshtml") : View(model);
        }
        
        public ActionResult ParentScript(string trackingID)
        {
            return View("~/Areas/Widget/Views/Shared/ParentScript.cshtml", (object)trackingID);
        }
        
        //public ActionResult _FaceBookScript()
        //{
        //    return PartialView("~/Areas/Widget/Views/Shared/_FacebookScript.cshtml");
        //}        


    }


    public class MVCTransferResult : RedirectResult
    {
        public MVCTransferResult(string url)
            : base(url)
        {
        }

        public MVCTransferResult(object routeValues)
            : base(GetRouteURL(routeValues))
        {
        }

        private static string GetRouteURL(object routeValues)
        {
            var url = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()), RouteTable.Routes);
            return url.RouteUrl(routeValues);
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var httpContext = HttpContext.Current;

            var newUrl = Url;

            if (Url.ToLower().Trim() == "/wix")
            {
                newUrl = "/Widget" + Url;
            }
            httpContext.Server.TransferRequest(newUrl, true); // change to false to pass query string parameters if you have already processed them       
        }
    }
}
