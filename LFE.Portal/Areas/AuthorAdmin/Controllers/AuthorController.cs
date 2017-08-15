using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.AuthorAdmin.Helpers;
using LFE.Portal.Areas.AuthorAdmin.Models;
using LFE.Portal.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using UploadS3.Models;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    [Authorize]
    public class AuthorController : BaseController
    {
        private readonly IAuthorAdminServices _authorAdminServices;
        private readonly IWebStoreServices _webStoreServices;
        private readonly IAuthorAdminCourseServices _courseServices;
        private readonly IBillingManageServices _billingManageServices;

        public AuthorController()
        {
            _courseServices        = DependencyResolver.Current.GetService<IAuthorAdminCourseServices>();
            _authorAdminServices   = DependencyResolver.Current.GetService<IAuthorAdminServices>();
            _webStoreServices      = DependencyResolver.Current.GetService<IWebStoreServices>();
            _billingManageServices = DependencyResolver.Current.GetService<IBillingManageServices>();
        }

        #region views
        /// <summary>
        /// currently using in user repo on index (2013-6-11) or from main navigation
        /// </summary>
        /// <returns></returns>
        public ActionResult Courses()
        {
            var user = GetCurrentUser();
            return View(user);
        }
        
        public ActionResult Marketing()
        {
            return View();
        }

        public ActionResult Coupons()
        {
            return View();
        }

        public ActionResult AffiliateItems()
        {
            ViewData["StoresLOV"] = _webStoreServices.GetUserAffiliateStoresLOV(CurrentUserId);
            return View();
        }

        public ActionResult Settings()
        {
           // string error;
            var token = new AccountSettingsDTO {Role = CommonEnums.UserRoles.Author}; //_userAccountServices.GetSettingsToken(CurrentUserId,out error);
            //token.ShowCancelButton = true;
            return View(token);
        }
        
        public ActionResult Videos()
        {
            var user = GetCurrentUser();
            
            return View("AuthorVideos",user);
        }

        public ActionResult SalesReport()
        {
            ViewData["StoresLOV"] = GetAuthorSotresLOV();
            return View("SalesReport");
        }

        public ActionResult PaymentsReport()
        {
            return View();
        }

        public ActionResult SubscribersReport()
        {
            return View();
        }

        //public ActionResult PurchaseReport()
        //{
        //    return View();
        //}

        public ActionResult Dashboard()
        {
            return RedirectToActionPermanent("Index", "Home", new {area = "AuthorAdmin"});
        }

        //public ActionResult DashboardPartial()
        //{
        //    SaveUserEvent(CommonEnums.eUserEvents.DASHBOARD_VIEW);

        //    var dto = CurrentUserId >= 0 ? BaseAuthorServices.GetAuthorStatistic(CurrentUserId) : new AuthorStatisticSummaryDTO();

        //    return PartialView("Author/_AuthorDashboard",dto);
        //}

        public ActionResult _AuthorSalesReport(SalesReportConfigToken token)
        {
            return PartialView("Dashboard/_SalesReport", token);
        }

        public ActionResult _SalesReport()
        {

            ViewData["StoresLOV"] = GetAuthorSotresLOV();

            return PartialView("SalesReports/Author/_SalesReport");
        }

        public ActionResult _OwnerStoresSalesReport(SalesReportConfigToken token)
        {
            return PartialView("Dashboard/_OwnerStoresSalesReport", token);
        }

        public ActionResult EditVideoPartial(long id,int? fileId)
        {
            var user = GetCurrentUser();

            var video = id >= 0 ? BaseAuthorServices.GetVideoToken(id, user.userId) : new UserVideoDto
            {
                status = ImportJobsEnums.eFileInterfaceStatus.Init
            };

            if (fileId != null)
            {
                video.fileId = fileId;
            }

            var token = new EditVideoToken
                                        {
                                            videoDTO = video
                                            ,mode = id >= 0 ? CommonEnums.ePageMode.edit : CommonEnums.ePageMode.insert
                                        };
                    
            return PartialView("Author/_EditVideo", token);
        }
     
        public ActionResult GetVideoReport(eReportTypes type, UserViewDto dto, int? pageSize = null)
        {
            switch (type)
            {
                case eReportTypes.Grid:
                    return PartialView("Author/_VideosList", new AuthorVideosPageToken{user = dto,ListPageSize = pageSize ?? 8});
                case eReportTypes.List:
                    return PartialView("Author/_VideosGrid", dto);
            }

            return PartialView("CourseReport/_CoursesList", dto);
        }

        public ActionResult FileInterfaceLogs()
        {
            return View();
        }       
        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorVideos([DataSourceRequest] DataSourceRequest request)
        {
            var user = GetCurrentUser();
            var list = new List<UserVideoDto>();

            if (user == null) return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            list = BaseAuthorServices.GetAuthorVideos(user.userId).OrderBy(x=>x.addon).ToList();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorVideosReport([DataSourceRequest] DataSourceRequest request)
        {
            var list = new List<UserVideoDto>();

            try
            {
                //var interfaced = BaseAuthorServices.GetAuthorUnporcessedVideos(CurrentUserId);

                list = BaseAuthorServices.GetAuthorVideos(CurrentUserId).OrderByDescending(x => x.addon).ThenBy(x => x.title).ToList();

                return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorSales([DataSourceRequest] DataSourceRequest request,int periodSelectionKind)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();
            
            var list = BaseAuthorServices.GetAuthorSales(CurrentUserId,kind);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOnetimeSales([DataSourceRequest] DataSourceRequest request, int periodSelectionKind,int? storeId)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind(); 
            
            var list = _authorAdminServices.GetAuthorSales(CurrentUserId,kind,BillingEnums.eOrderLineTypes.SALE, storeId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetSubscriptionSales([DataSourceRequest] DataSourceRequest request, int periodSelectionKind, int? storeId)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _authorAdminServices.GetAuthorSales(CurrentUserId, kind, BillingEnums.eOrderLineTypes.SUBSCRIPTION, storeId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOrderLinePayments([DataSourceRequest] DataSourceRequest request, int LineId)
        {

            var list = _billingManageServices.GetOrderLinePayments(LineId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetPaymentRefunds([DataSourceRequest] DataSourceRequest request, int PaymentId)
        {

            var list = _billingManageServices.GetOrderLinePaymentRefunds(PaymentId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetSellerPayments([DataSourceRequest] DataSourceRequest request, int year, int month, short currencyId = Constants.DEFAULT_CURRENCY_ID)
        {

            var list = _billingManageServices.GetSellerPayments(CurrentUserId, year, month,currencyId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSellerRefunds([DataSourceRequest] DataSourceRequest request, int year, int month, short currencyId = Constants.DEFAULT_CURRENCY_ID)
        {

            var list = _billingManageServices.GetSellerRefunds(CurrentUserId, year, month,currencyId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorSubscribers([DataSourceRequest] DataSourceRequest request, int? courseId = null)
        {
            var list = BaseAuthorServices.GetAuthorSubscribers(CurrentUserId, courseId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAuthorCourses()
        {
            var list = CurrentUserId >= 0 ? (object)_courseServices.GetAuthorCoursesList(Constants.DEFAULT_CURRENCY_ID, CurrentUserId).OrderBy(x => x.Name).Select(x => new
            {
                id = x.CourseId
                ,name = x.Name
                ,url = x.ImageUrl
            }).ToArray() : new CourseListDTO[0];
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetSalesChartData(int periodSelectionKind,eDashReportKinds kind)
        {
            var period = periodSelectionKind.ToPeriodSelectionKind(); 

            var groupping = ReportEnums.eChartGroupping.Day;

            switch (period)
            {
                case ReportEnums.ePeriodSelectionKinds.thisMonth:
                case ReportEnums.ePeriodSelectionKinds.week:
                    groupping = ReportEnums.eChartGroupping.Day;
                    break;
                case ReportEnums.ePeriodSelectionKinds.lastMonth:
                case ReportEnums.ePeriodSelectionKinds.last90:
                    groupping = ReportEnums.eChartGroupping.Week;
                    break;
                case ReportEnums.ePeriodSelectionKinds.last180:
                case ReportEnums.ePeriodSelectionKinds.all:
                    groupping = ReportEnums.eChartGroupping.Month;
                    break;
            }

            var list = new List<SalesAnalyticChartDTO>();

            switch (kind)
            {
                case eDashReportKinds.content:
                    list = BaseAuthorServices.GetSalesChartData(CurrentUserId, period, groupping);
                    break;
                case eDashReportKinds.stores:
                    list = _webStoreServices.GetSalesChartData(CurrentUserId, period, groupping);
                    break;
            }
            
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAffiliateItems([DataSourceRequest] DataSourceRequest request)
        {
            var list = _webStoreServices.GetUserAffiliateItems(CurrentUserId).OrderBy(x=>x.ItemName).ThenBy(x=>x.WebStore.Name).ToArray();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetFileInterfaceLogs([DataSourceRequest] DataSourceRequest request)
        {
            var list = _authorAdminServices.GetAuthorFileInterfaceLogs(CurrentUserId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private BaseListDTO[] GetAuthorSotresLOV()
        {
            return _webStoreServices.GetOwnerStores(CurrentUserId).Select(x => new BaseListDTO { id = x.StoreId, name = x.Name }).Union(_webStoreServices.GetUserAffiliateStoresLOV(CurrentUserId)).ToArray();
        } 

        #region dashboard
        public ActionResult GetAuthorSalesSummary(eDashReportKinds summaryRepKind)
        {

            decimal total30 = 0;
            switch (summaryRepKind)
            {
                case eDashReportKinds.content:
                    var list1 = BaseAuthorServices.GetAuthorSales(CurrentUserId, ReportEnums.ePeriodSelectionKinds.thisMonth).ToArray();
                    total30 = list1.Any() ? (int) list1.Sum(x => x.TotalAmount) : 0;
                    break;
                case eDashReportKinds.stores:
                    var list2 = _webStoreServices.GetOwnerStoreSales(CurrentUserId, ReportEnums.ePeriodSelectionKinds.thisMonth,null,null,null).ToArray();
                    total30 = list2.Any() ? (int) list2.Sum(x => x.TotalAmount) : 0;
                    break;
            }
    
            var response = new {
                                    total7 = 0
                                    ,total30
                                };

            return Json(response,JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        #region posts

        /// <summary>
        /// save video from edit window
        /// </summary>
        /// <param name="videoDTO">videoDTO equal to field in EditVideoToken </param>
        /// <returns></returns>
        public JsonResult SaveVideo(UserVideoDto videoDTO)
        {
            string error;
            var saved = BaseAuthorServices.SaveVideo(videoDTO, out error);

            return Json(new JsonResponseToken
            {
                success = saved
                ,error = error
            },JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DeleteVideo(long id,int? fileId)
        {
            string error;
            var isDeleted =fileId!=null ? BaseAuthorServices.DeleteWaitingVideo((int) fileId,out error) : BaseAuthorServices.DeleteVideo(id, out error);

            return Json(new JsonResponseToken
            {
                success = isDeleted
                ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DestroyVideo([DataSourceRequest] DataSourceRequest request, UserVideoDto dto)
        {
            string error;

            if (dto == null) return Json(ModelState.ToDataSourceResult());

            if (dto.fileId != null && dto.fileId >= 0)
            {
                BaseAuthorServices.DeleteWaitingVideo((int) dto.fileId, out error);
            }
            else
            {
                if (dto.identifier != null) BaseAuthorServices.DeleteVideo(long.Parse(dto.identifier), out error);    
            }
            
            return Json(ModelState.ToDataSourceResult());
        }


        public ActionResult ExportSubscribers(int? id = null)
        {

            var list = BaseAuthorServices.GetAuthorSubscribers(CurrentUserId, id).OrderBy(x=>x.name).ToList();

            var output = new MemoryStream();
            var writer = new StreamWriter(output, Encoding.UTF8);

            writer.Write("Name,");
            writer.Write("Email");
            
            writer.WriteLine();
            
            foreach (var subscriber in list)
            {
                writer.Write(subscriber.name);
                writer.Write(",");
                writer.Write("\"");
                writer.Write(subscriber.email);
                writer.Write("\"");
                writer.WriteLine();
            }

            writer.Flush();
            output.Position = 0;

            string fileName;

            if (id == null)
            {
                fileName = (TodayFileString() + "_" + this.CurrentUser().Nickname + "_subscribers.csv").OptimizedUrl();
            }
            else
            {
                var course = _courseServices.GetCourseEditDTO((int) id);

                fileName = (TodayFileString() + "_" + course.CourseName + "_" + this.CurrentUser().Nickname + "_subscribers.csv").OptimizedUrl();
                
            }

            return File(output, "text/comma-separated-values", fileName);
        }
        #endregion

    }
}
