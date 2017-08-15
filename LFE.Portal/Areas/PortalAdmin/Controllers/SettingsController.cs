using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Utils;
using LFE.DataTokens;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    public class SettingsController : BaseController
    {
        private readonly IPortalAdminEmailServices _emailServices;
        private readonly ISettingsServices _settingsServices;
        private readonly ICategoryManageServices _categoryManageServices;
        private readonly IWebStoreServices _webStoreServices;
        public SettingsController()
        {
            _emailServices          = DependencyResolver.Current.GetService<IPortalAdminEmailServices>();
            _settingsServices       = DependencyResolver.Current.GetService<ISettingsServices>();
            _categoryManageServices = DependencyResolver.Current.GetService<ICategoryManageServices>();
            _webStoreServices       = DependencyResolver.Current.GetService<IWebStoreServices>();
        }


        #region wizard steps
        public ActionResult WizardStepTooltips()
        {
            return View();
        }

        public ActionResult GetSteps([DataSourceRequest] DataSourceRequest request)
        {
            var list = _settingsServices.GetSteps().ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveStepTooltip(WizardStepTooltipDTO token)
        {
            string error;
            var result = _settingsServices.SaveStepTooltip(token, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            },JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region email
        public ActionResult EmailTemplates()
        {
            return View();
        }

        public ActionResult _EmailTemplateEditForm(short kindId)
        {
            var token = _emailServices.GetTemplateDTO(kindId);

            return PartialView("Email/_EmailTemplate", token);
        }

        public JsonResult SaveEmailTemplate(EmailTemplateDTO token)
        {
            string error;
            var saved = _emailServices.SaveTemplate(token, CurrentUserId, out error);

            return Json(new JsonResponseToken
            {
                success = saved
                ,error = error
            }
            , JsonRequestBehavior.AllowGet);
        }
        #endregion
        
        #region categories
        public ActionResult CategoryManage()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ReadCategories([DataSourceRequest] DataSourceRequest request)
        {
            return Json(_categoryManageServices.GetCategories().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveCategory([DataSourceRequest] DataSourceRequest request, CategoryEditDTO category)
        {
            if (category != null && ModelState.IsValid)
            {
                string error;
                _categoryManageServices.SaveCategory(ref category, out error);
            }

            return Json(new[] { category }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DestroyCategory([DataSourceRequest] DataSourceRequest request, CategoryEditDTO category)
        {
            if (category != null)
            {
                string error;
                _categoryManageServices.DeleteCategory(category, out error);
            }

            return Json(ModelState.ToDataSourceResult());
        }
        #endregion

        #region lfe store

        public ActionResult LfeStoreManage()
        {
            return View();
        }
        #endregion
    }
}
