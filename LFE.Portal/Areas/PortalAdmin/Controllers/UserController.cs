using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Helpers;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    public class UserController : BaseController
    {
        private readonly Portal.Controllers.AccountController _mainAccountController = new Portal.Controllers.AccountController();
        private readonly IPortalAdminUserServices _userServices;
        private readonly IUserAccountServices _userAccountServices;
        private readonly IAuthorAdminCourseServices _courseServices;
        private readonly IUserPortalServices _userPortalServices;
        private readonly IAuthorAdminServices _authorAdminServices;
        private readonly IWebStoreServices _webStoreServices;
        public UserController()
        {
            _userServices        = DependencyResolver.Current.GetService<IPortalAdminUserServices>();
            _courseServices      = DependencyResolver.Current.GetService<IAuthorAdminCourseServices>();
            _userAccountServices = DependencyResolver.Current.GetService<IUserAccountServices>();
            _userPortalServices  = DependencyResolver.Current.GetService<IUserPortalServices>();
            _authorAdminServices = DependencyResolver.Current.GetService<IAuthorAdminServices>();
            _webStoreServices    = DependencyResolver.Current.GetService<IWebStoreServices>();
        }


        #region views
        public ActionResult Report()
        {
            return View();
        }
        public ActionResult StatisticsReport()
        {
            return View();
        }

        public ActionResult GetUserEditForm(int id)
        {
            var token = _userServices.GetUserEditDTO(id);

            return PartialView("User/_EditUserForm", token);
        }

        public ActionResult UserDetails(int id)
        {
            var token = _userServices.GetUserEditDTO(id);

            return PartialView("User/_UserDetails", token);
        }

        public ActionResult GetCourseReportPartial(int id)
        {
            return PartialView("User/_UserCourses",id);            
        }

        public ActionResult GetBundleReportPartial(int id)
        {
            return PartialView("User/_UserBundles",id);
        }

        public ActionResult GetPurchaseReportPartial(int id)
        {
            return PartialView("User/_UserPurchase", id);
        }
        public ActionResult GetSalesReportPartial(int id)
        {
            return PartialView("User/_UserSales", id);
        }
        public ActionResult GetStoresReportPartial(int id)
        {
            return PartialView("User/_UserStores", id);
        }

        public ActionResult GetVideoReportPartial(int id)
        {
            return PartialView("User/_UserVideos", id);
        }
        public ActionResult GetEventsReportPartial(int id)
        {
            return PartialView("User/_UserEvents", id);
        }
        public ActionResult GetStatisticsPartial(int id)
        {
            var token = _userServices.GetUserStatisticToken(id);

            return PartialView("User/_UserStatistics", token);
        }

        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUsers([DataSourceRequest] DataSourceRequest request)
        {
            var list = _userServices.GetUsers().OrderByDescending(x=>x.RegisterDate).ThenBy(x => x.FullName).ToList();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SearchUsers([DataSourceRequest] DataSourceRequest request, int? userId, int? typeId, DateTime? logFrom, DateTime? logTo, DateTime? regFrom, DateTime? regTo, bool isGrp, int? roleId)
        {
            var list = _userServices.SearchUsers(userId,typeId,logFrom,logTo,regFrom,regTo, isGrp, roleId);

            var jsonResult = Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }

        public FileResult ExportUserReport(int? userId, int? typeId, DateTime? logFrom, DateTime? logTo, DateTime? regFrom, DateTime? regTo)
        {

            var list = _userServices.SearchUsers(userId, typeId, logFrom, logTo, regFrom, regTo, false, null);

            var output = new MemoryStream();
            var writer = new StreamWriter(output, Encoding.UTF8);

            writer.Write("Name,");
            writer.Write("Email");

            writer.WriteLine();

            foreach (var subscriber in list)
            {
                writer.Write(subscriber.FullName);
                writer.Write(",");
                writer.Write("\"");
                writer.Write(subscriber.Email);
                writer.Write("\"");
                writer.WriteLine();
            }

            writer.Flush();
            output.Position = 0;

            string fileName = (TodayFileString() + "_users.csv").OptimizedUrl();

            
            return File(output, "text/comma-separated-values", fileName);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult FindUsers(string name = null, string email = null)
        {
            var list = _userServices.GetUsers(null,null,name,email).OrderBy(x => x.FullName).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorCoursesList([DataSourceRequest] DataSourceRequest request,int id)
        {
            var list = CurrentUserId >= 0 ? _courseServices.GetAuthorCoursesList(Constants.DEFAULT_CURRENCY_ID, id).OrderBy(x => x.Name).ToArray() : new CourseListDTO[0];
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorBundleList([DataSourceRequest] DataSourceRequest request,int id)
        {
            var list = CurrentUserId >= 0 ? _courseServices.GetAuthorBundlesList(id).OrderBy(x => x.Name).ToArray() : new BundleListDTO[0];
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserPurchases([DataSourceRequest] DataSourceRequest request,int id)
        {
            var list = _userPortalServices.GetUserPurchases(id).OrderByDescending(x => x.OrderDate).ToArray();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserSales([DataSourceRequest] DataSourceRequest request, int id)
        {
         
            var list = _authorAdminServices.GetAuthorSales(id,ReportEnums.ePeriodSelectionKinds.all);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserStores([DataSourceRequest] DataSourceRequest request,int id)
        {
          
            var list = _webStoreServices.GetOwnerStores(id).OrderBy(x => x.AddOn).ThenBy(x => x.Name).ToArray();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUsersStatistics([DataSourceRequest] DataSourceRequest request)
        {

            var list = _userServices.GetUsersStatistic().OrderByDescending(x=>x.Score).ToArray();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserVideos([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = new List<UserVideoDto>();

            try
            {
                var videos = _authorAdminServices.GetAuthorVideos(id);
                var interfaced = _authorAdminServices.GetAuthorUnporcessedVideos(id);

                list = videos.Union(interfaced).OrderByDescending(x => x.addon).ThenBy(x => x.title).ToList();

                return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region posts

        [HttpPost]
        public JsonResult SaveUser(UserEditDTO token)
        {
            string error;
            var result = _userAccountServices.UpdateUser(token, out error);
            
            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoginAsUser(string email)
        {
            string error;
            var logged = _mainAccountController.CreateAuthenticationTicket(email, string.Empty, string.Empty, out error);

            return logged ? RedirectToAction("UserSettings", "Account", new {area = ""}) : ErrorResponse(error);
        }

        public ActionResult DeleteUser(int? id)
        {
            if (id == null) return ErrorResponse("id required");

            string error;

            var result = _userAccountServices.DeleteUser((int) id, out error);

            return Json(new JsonResponseToken {success = result, error = error}, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
