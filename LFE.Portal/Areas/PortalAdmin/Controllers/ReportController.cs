using System;
using System.Drawing.Imaging;
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
using LFE.Portal.Areas.AuthorAdmin.Helpers;
using LFE.Portal.Areas.PortalAdmin.Models;
using LFE.Portal.Helpers;
using System.Collections.Generic;
using Svg;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        private readonly IReportServices _reportServices;
        private readonly IQuizAdminServices _quizAdminServices;
        public ReportController()
        {
            _reportServices      = DependencyResolver.Current.GetService<IReportServices>();
            _quizAdminServices   = DependencyResolver.Current.GetService<IQuizAdminServices>();
        }

        #region views
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SystemLogs()
        {
            return View();
        }
        public ActionResult FileInterfaceLogs()
        {
            return View();
        }
        public ActionResult EventLogsReport()
        {
            return View();
        }
        public ActionResult EventChartReport()
        {
            return View();
        }
        public ActionResult EventOwnerChartReport()
        {
            return View();
        }
        public ActionResult FbPostInterfaceLogs()
        {
            return View();
        }

        public ActionResult VideosReport()
        {
            return View();
        }

        public ActionResult CourseReport()
        {
            return View();
        }
        public ActionResult EmailInterfaceLogs()
        {
            return View();
        }

      
        public ActionResult StoresReport()
        {
            return View();
        }

        public ActionResult HostEventsReport()
        {
            return View();
        }

        public ActionResult HostAbandon()
        {
            return View(new HostAbandonFilterToken());
        }

        public ActionResult SummaryReport()
        {
            return View();
        }

        public ActionResult PluginInstallations()
        {
            return View();
        }

       
        public ActionResult QuizReport()
        {
            return View();
        }
        #endregion
        
        #region posts

        public ActionResult GenerateSummaryReport(ReportEnums.ePeriodSelectionKinds? periodKind,ReportEnums.eChartGroupping? groupKind)
        {
            if(periodKind==null) periodKind = ReportEnums.ePeriodSelectionKinds.last180;

            if(groupKind==null) groupKind = ReportEnums.eChartGroupping.Month;

            var token = _reportServices.GetSummaryReport((ReportEnums.ePeriodSelectionKinds) periodKind,(ReportEnums.eChartGroupping) groupKind);

            return PartialView("Report/_SummaryReportGrid",token);
        }
       
        #endregion

        #region api
        //user events
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetEventLogs([DataSourceRequest] DataSourceRequest request, int periodSelectionKind, short? eventTypeId = null, int? userId = null, int? courseId = null, int? bundleId = null, int? storeId = null, long? sessionId = null)
        {

            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _reportServices.GetUserEventLogs(request.Page,request.PageSize,userId, eventTypeId, kind,courseId,bundleId,storeId,sessionId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

       
        //system logs
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetSystemLogs([DataSourceRequest] DataSourceRequest request, int periodSelectionKind, CommonEnums.LoggerObjectTypes? module = null, CommonEnums.eLogLevels? level = null, int? userId = null, long? sessionId=null, string ipAddress=null)
        {

            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _reportServices.GetSystemLogs(kind,module, level,userId,sessionId,ipAddress);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        //file interface
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetFileInterfaceLogs([DataSourceRequest] DataSourceRequest request, int periodSelectionKind, int? userId = null, string status = null)
        {
            ImportJobsEnums.eFileInterfaceStatus? fileStatus = null;

            try
            {
                if (!string.IsNullOrEmpty(status)) fileStatus = Utils.ParseEnum<ImportJobsEnums.eFileInterfaceStatus>(status);
            }
            catch (Exception)
            {
                fileStatus = null;
            }

            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _reportServices.GetFileInterfaceLogs(kind, userId, fileStatus);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFileInterfaceUsersLOV()
        {
            var list = _reportServices.GetFileInterfaceLogs(ReportEnums.ePeriodSelectionKinds.all, null, null);

            var users = list.GroupBy(x => new { x.User.id,x.User.name,x.User.url }).Select(x => new UserViewToken { id = x.Key.id, name = x.Key.name, url = x.Key.url }).ToArray();

            return Json(users, JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetVideos([DataSourceRequest] DataSourceRequest request, int periodSelectionKind, int? userId = null,bool? attachedOnly = null)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _reportServices.GetVideosReport(kind, userId,(attachedOnly==null || (bool)!attachedOnly ? (bool?)null : true));

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        //fb posts
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetFbPostInterfaceLogs([DataSourceRequest] DataSourceRequest request, int periodSelectionKind, int? userId = null, string status = null)
        {
            FbEnums.ePostInterfaceStatus? postStatus = null;

            try
            {
                if (!string.IsNullOrEmpty(status)) postStatus = Utils.ParseEnum<FbEnums.ePostInterfaceStatus>(status);
            }
            catch (Exception)
            {
                postStatus = null;
            }

            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _reportServices.GetFbPostLogs(kind, userId, postStatus);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFbPostInterfaceUsersLOV()
        {
            var list = _reportServices.GetFbPostLogs(ReportEnums.ePeriodSelectionKinds.all, null, null);

            var users = list.GroupBy(x => new { x.User.id,x.User.name,x.User.url }).Select(x => new UserViewToken { id = x.Key.id, name = x.Key.name, url = x.Key.url }).ToArray();

            return Json(users, JsonRequestBehavior.AllowGet);

        }

        //email interface
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetEmailInterfaceLogs([DataSourceRequest] DataSourceRequest request, int periodSelectionKind, int? userId = null, string status = null)
        {
            EmailEnums.eSendInterfaceStatus? sendStatus = null;

            try
            {
                if (!string.IsNullOrEmpty(status)) sendStatus = Utils.ParseEnum<EmailEnums.eSendInterfaceStatus>(status);
            }
            catch (Exception)
            {
                sendStatus = null;
            }

            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _reportServices.GetEmailInterfaceLogs(kind, userId, sendStatus);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorItemsReport([DataSourceRequest] DataSourceRequest request, int? id = null, int? itemId = null, CourseEnums.CourseStatus? status = null)
        {
            var list = _reportServices.GetAuthorItemsList(id,itemId,status).OrderByDescending(x => x.AddOn).ThenBy(x=>x.AuthorName).ThenBy(x=>x.ItemName).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemNamesLOV()
        {
            var list = _reportServices.GetAuthorItemsList(null,null,null).OrderBy(x=>x.ItemName).Select(x=>new NameValue
            {
                Name = x.ItemName
                ,Value = x.ItemId
            }).ToArray();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStoresReport([DataSourceRequest] DataSourceRequest request, int? userId, int? sourceId, bool? isAffiliate)
        {
            var source = sourceId == null ? (CommonEnums.eRegistrationSources?) null : Utils.ParseEnum<CommonEnums.eRegistrationSources>(sourceId.ToString());

            var list = _reportServices.GetStoresReport(userId,source,isAffiliate);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmailInterfaceUsersLOV()
        {
            var list = _reportServices.GetEmailInterfaceLogs(ReportEnums.ePeriodSelectionKinds.all, null, null);

            var users = list.GroupBy(x => new { x.User.id,x.User.name,x.User.url }).Select(x => new UserViewToken { id = x.Key.id, name = x.Key.name, url = x.Key.url }).ToArray();

            return Json(users, JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult FindCourses(string name = null)
        {
            var list = _reportServices.FindCourses(null, null, name).OrderBy(x => x.name).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult FindBundles(string name = null)
        {
            var list = _reportServices.FindBundles(null, null, name).OrderBy(x => x.name).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult FindStores(string name = null,int? userId=null)
        {
            var list = _reportServices.FindStores(null, userId, name).OrderBy(x => x.name).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetHostEvents([DataSourceRequest] DataSourceRequest request,ReportEnums.ePeriodSelectionKinds? periodKind, ReportEnums.eChartGroupping? groupKind,string host)
        {
            if (periodKind == null) periodKind = ReportEnums.ePeriodSelectionKinds.last180;

            if (groupKind == null) groupKind = ReportEnums.eChartGroupping.Month;

            var list = _reportServices.GetHostEventsReport((ReportEnums.ePeriodSelectionKinds)periodKind, (ReportEnums.eChartGroupping)groupKind,host);

            var result = Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            result.MaxJsonLength = Int32.MaxValue;

            return result;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetHostAbandon([DataSourceRequest] DataSourceRequest request, DateTime? from, DateTime? last)
        {
            if (from == null) from = DateTime.Now.AddYears(-1);
            if (last == null) last = DateTime.Now.AddMonths(-1);

            var list = _reportServices.GetHostAbandon((DateTime)@from, (DateTime)last);

            var result = Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            result.MaxJsonLength = Int32.MaxValue;

            return result;
        }
        

        public ActionResult GetHostPivotEvents([DataSourceRequest] DataSourceRequest request)
        {
            var periodKind = ReportEnums.ePeriodSelectionKinds.lastMonth;

            var groupKind = ReportEnums.eChartGroupping.Week;

            var list = _reportServices.GetHostEventsReport(periodKind, groupKind, null);

            var result = Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            result.MaxJsonLength = Int32.MaxValue;

            return result;
        }
       
        public ActionResult GetInstallationsReport([DataSourceRequest] DataSourceRequest request, int? userId, int? periodSelectionKind, int? typeId, bool? isactive)
        {
            ReportEnums.ePeriodSelectionKinds? kind = periodSelectionKind != null 
                ? (ReportEnums.ePeriodSelectionKinds?)periodSelectionKind.ToPeriodSelectionKind() : null;
            var list = _reportServices.GetInstallationsReport(kind, userId, typeId, isactive).OrderByDescending(x=>x.AddOn).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserQuizzes([DataSourceRequest] DataSourceRequest request,int? id)
        {
            var list = _quizAdminServices.GetUserQuizzes(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region KPI Dashboard

        public ActionResult _DashboardAdmin()
        {
            var token = _reportServices.GetKpiDashboardFilters();

            return PartialView("Report/_DashboardAdmin", token);
        }

        public ActionResult _FunnelChart(FunnelRequestToken request)
        {
            var data = _reportServices.GetFunnelChartData(request.period, request.authorId, request.storeId, request.itemId, request.itemType);

            var token = new FunnelViewToken
            {
                ChartData = data,
                RequestToken = request
            };

            return PartialView("Report/_FunnelChart", token);
        }

        public ActionResult GetKpiData(ReportEnums.ePeriodSelectionKinds period,int? authorId,int? storeId,int? itemId,CommonEnums.eEventItemTypes? itemType)
        {
            var facts = _reportServices.GetKpiData(period, authorId, storeId, itemId, itemType);

            return Json(facts, JsonRequestBehavior.AllowGet);
        }

        public FileContentResult GetAuthorKpiChartImage(AuthorStatementRequestToken request)
        {
            var items = _reportServices.GetStatementKpiData(request);

            var svgText = RenderRazorViewToString("_AuthorKpiChart", items);


            var bytes = Encoding.ASCII.GetBytes(svgText);

            using (var stream = new MemoryStream(bytes))
            {
                var svgDocument = SvgDocument.Open(svgText);
                var bitmap = svgDocument.Draw();
                bitmap.Save(stream, ImageFormat.Png);
                var imageBytes = stream.ToArray();
                
                return new FileContentResult(imageBytes, "image/png");
            }

        }

        
        public ActionResult GetStatementKpiData(AuthorStatementRequestToken token)
        {
            var facts = _reportServices.GetStatementKpiData(token);

            return Json(facts, JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetKpiDataRows([DataSourceRequest] DataSourceRequest request, ReportEnums.ePeriodSelectionKinds period, int? authorId, int? storeId, int? itemId, CommonEnums.eEventItemTypes? itemType)
        {
            var list = _reportServices.GetKpiDataRows(period, authorId, storeId, itemId, itemType).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DailyTotals(short? tPeriod)
        {
            var period = tPeriod == null ? ReportEnums.ePeriodSelectionKinds.lastMonth : Utils.ParseEnum<ReportEnums.ePeriodSelectionKinds>(tPeriod.ToString());

            var facts = _reportServices.GetDailyTotalsData(period);

            return PartialView("Report/_DashboardDailyTotalsCharts", facts);
        }

        public ActionResult _DailyStats(short? sPeriod)
        {

            var period = sPeriod == null ? ReportEnums.ePeriodSelectionKinds.lastMonth : Utils.ParseEnum<ReportEnums.ePeriodSelectionKinds>(sPeriod.ToString());
            var facts = _reportServices.GetDailyStatsData(period);

            return PartialView("Report/_DashboardDailyStatsCharts", facts);
        }

        public ActionResult GetVideoUploadsDailyStatsData(ReportEnums.ePeriodSelectionKinds period)
        {
            var facts = _reportServices.GetDailyVideoStatsData(period);

            return Json(facts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFactDailyStatsData(ReportEnums.ePeriodSelectionKinds period)
        {
            var facts = _reportServices.GetDailyStatsData(period);

            return Json(facts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFactDailyTotalsData(ReportEnums.ePeriodSelectionKinds period)
        {
            var facts = _reportServices.GetDailyTotalsData(period);

            return Json(facts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDailyStatsDataRows(DateTime? date, ReportEnums.eDailyStatsFields field)
        {
            if (date == null) return Content("<h3>Date required</h3>");

            switch (field)
            {
                case ReportEnums.eDailyStatsFields.UsersCreated:
                case ReportEnums.eDailyStatsFields.WixUsersCreated:
                case ReportEnums.eDailyStatsFields.UserLogins:
                case ReportEnums.eDailyStatsFields.AuthorLogins:
                case ReportEnums.eDailyStatsFields.ReturnUsersLogins:
                    var users = _reportServices.GetDailyStatsUserData((DateTime) date, field);
                    return PartialView("Report/_DailyStatsUserData",users);
                case ReportEnums.eDailyStatsFields.ItemsCreated:
                case ReportEnums.eDailyStatsFields.ItemsPublished:
                    var items = _reportServices.GetDailyStatsItemData((DateTime) date, field);
                    return PartialView("Report/_DailyStatsItemData", items);
                case ReportEnums.eDailyStatsFields.StoresCreated:
                case ReportEnums.eDailyStatsFields.WixStoresCreated:
                    var stores = _reportServices.GetDailyStatsStoreData((DateTime)date, field);
                    return PartialView("Report/_DailyStatsStoreData", stores);
                case ReportEnums.eDailyStatsFields.ItemsPurchased:
                case ReportEnums.eDailyStatsFields.FreeItemsPurchased:
                    var lines = _reportServices.GetDailyStatsPurchaseData((DateTime)date, field);
                    return PartialView("Report/_DailyStatsPurchaseData", lines);
                case ReportEnums.eDailyStatsFields.TotalVideos:
                case ReportEnums.eDailyStatsFields.TotalUsedVideos:
                    var videos = _reportServices.GetDailyStatsVideoData((DateTime)date, field);
                    return PartialView("Report/_DailyStatsVideoData", videos.OrderBy(x=>x.user.FullName).ToList());
            }

            return Content("d=" + date + " f=" + field);
        }

        public ActionResult EventChartData(int periodSelectionKind, short? eventTypeId = null, int? userId = null, int? courseId = null, int? bundleId = null, int? storeId = null)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var facts = _reportServices.GetEventStatsData(kind, userId, eventTypeId, courseId, bundleId, storeId);

            return Json(facts,JsonRequestBehavior.AllowGet);
        }

        public ActionResult EventOwnerChartData(int periodSelectionKind)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var facts = _reportServices.GetOwnerEventStatsData(kind);

            return Json(facts, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
