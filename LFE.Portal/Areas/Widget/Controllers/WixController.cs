using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Core.Utils;
using LFE.DataTokens;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class WixController : WixBaseController
    {

        private readonly IWidgetServices _widgetServices;
        public WixController()
        {
            _widgetServices = DependencyResolver.Current.GetService<IWidgetServices>();
        }

        public ActionResult SingleCourse(string instance, string tabName, int? width, int? height)
        {
            ItemProductPageToken token;
            if (MainLayoutViewModel.NumCourses > 0 && MainLayoutViewModel.CategoriesList != null && MainLayoutViewModel.CategoriesList.Any())
            {
                ViewBag.TabName = tabName;
                token = WidgetServices.GetWixDefaultItem(MainLayoutViewModel.CategoriesList.Select(x => x.WebStoreCategoryID).ToList(), CurrentUserId);
            }
            else
            {
                token = WidgetServices.GetPlaceHolderItemInfoToken();
            }
            return View("~/Areas/Widget/Views/Wix/PlaceHolder.cshtml", token);
        }




        public ActionResult Index(string instance, int? width, int? height)
        {
            if (MainLayoutViewModel.IsSingleCourseStore)
            {
                ItemProductPageToken token;
                if (MainLayoutViewModel.NumCourses > 0 && MainLayoutViewModel.CategoriesList != null && MainLayoutViewModel.CategoriesList.Any())
                {
                    token = _widgetServices.GetWixDefaultItem(MainLayoutViewModel.CategoriesList.Select(x => x.WebStoreCategoryID).ToList(), CurrentUserId);
                }
                else
                {
                    token = _widgetServices.GetPlaceHolderItemInfoToken();                    
                }

                return View("~/Areas/Widget/Views/Wix/PlaceHolder.cshtml", token);
            }
            
            var pagesize = _widgetServices.NumItemsInPage(width, height);
            
            const int page = 1;

           // var sort = "";

            string error;
            var instanceDTO = instance.DecodeInstance2WixInstanceDTO(out error);
            
            if (instanceDTO == null) return View("~/Areas/Widget/Views/Shared/Error.cshtml");

            var currencyId = Constants.DEFAULT_CURRENCY_ID;

            if (MainLayoutViewModel != null &&  MainLayoutViewModel.WebStore != null && MainLayoutViewModel.WebStore.CurrencyId != null )
            {
                currencyId = (short)MainLayoutViewModel.WebStore.CurrencyId;
            }

            var model = _widgetServices.GetIndexModelView(currencyId, instanceDTO.instanceId.ToString(), page, "", pagesize, MainLayoutViewModel.Category != null ? MainLayoutViewModel.Category.WebStoreCategoryID : (int?) null, MainLayoutViewModel.CategoryName ?? "", CurrentUserId, MainLayoutViewModel.WixViewMode ?? "site");

            if (MainLayoutViewModel.WixViewMode == "editor")
            {

            }

            return model == null ? View("~/Areas/Widget/Views/Shared/Error.cshtml") : View("~/Areas/Widget/Views/Widget/Index.cshtml", model);
        }

    }
}
