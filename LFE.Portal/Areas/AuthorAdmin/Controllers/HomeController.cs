using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.ExternalProviders;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.AuthorAdmin.Helpers;
using LFE.Portal.Areas.AuthorAdmin.Models;
using LFE.Portal.Controllers;
using LFE.Portal.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {

        private readonly IDashboardServices _dashboardServices;
        private readonly IWidgetEndpointServices _widgetEndpointServices;
        private readonly IBillingManageServices _billingManageServices;
        private readonly StandardPdfRenderer _pdfRenderer = new StandardPdfRenderer();

        public HomeController()
        {
            _dashboardServices      = DependencyResolver.Current.GetService<IDashboardServices>();
            _widgetEndpointServices = DependencyResolver.Current.GetService<IWidgetEndpointServices>();
            _billingManageServices  = DependencyResolver.Current.GetService<IBillingManageServices>();
        }

        private DashboardToken getDashboardToken()
        {
            var userCurrencies = _dashboardServices.GetUserCurrencies(CurrentUserId);
            var token = new DashboardToken
            {
                UserId         = CurrentUserId,
                Currencies     = userCurrencies,
                NextPayoutList = _dashboardServices.GetNextPayout(CurrentUserId),
                Stats          = _dashboardServices.GetAuthorDashboardStats(CurrentUserId),
                StoreList      = _widgetEndpointServices.GetOwnerStores(CurrentUserId),
                Filters        = new FiltersToken
                {
                    UserId        = CurrentUserId,
                    CurrencyId    = userCurrencies.Count > 0 ? userCurrencies[0].CurrencyId : Constants.DEFAULT_CURRENCY_ID,
                    PeriodTypeId  = (int)ReportEnums.ePeriodSelectionKinds.thisMonth,
                    IsCompareMode = false
                },
            };

            return token;
        }

        public ActionResult Index()
        {
            return View(getDashboardToken());
        }

        public ActionResult AdminDashboard(int id)
        {
            return View(getDashboardToken());
        }

        public ActionResult GetSalesSecion(FiltersToken token)
        {
            var tokenList = _dashboardServices.GetSales(token, token.UserId);
            tokenList.Filters = token;
            return PartialView("~/Areas/AuthorAdmin/Views/Shared/Dashboard/_dbYourSales.cshtml", tokenList);
        }

        
        #region charts
        private ActionResult GetChartData(bool isCompareChart, FiltersToken filter)
        {
            var facts = _dashboardServices.GetChartData(filter, filter.UserId, isCompareChart);
            return Json(facts, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetMainChartData(FiltersToken token)
        {
            return GetChartData(false, token);
        }

        public ActionResult GetCompChartData(FiltersToken token)
        {
            return GetChartData(true, token);
        }

        public ActionResult GetPeriods(FiltersToken token)
        {
            var periodKind = token.PeriodTypeId.ToPeriodSelectionKind();
            var result = new PeriodsToken
            {
                Period        = _dashboardServices.PeriodKindToDateRange(periodKind, false),
                ComparePeriod = token.IsCompareMode ? _dashboardServices.PeriodKindToDateRange(periodKind, true) : null
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDashboardEventsList([DataSourceRequest] DataSourceRequest request,int userId, DateRangeToken dates)
        {
            var list = _dashboardServices.GetDashboardEvents(userId,dates);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        #region custom events manage
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDashboardCustomEventsList([DataSourceRequest] DataSourceRequest request,int userId)
        {
            //var list = SessionCustomEvents;
            var list = _dashboardServices.CustomEventGetList(userId);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateCustomeEvent([DataSourceRequest] DataSourceRequest request, DashboardEventToken token,int userId)
        {
            if (token != null && token.Uid.Equals(Guid.Empty) && token.Date != null && !string.IsNullOrEmpty(token.Name) && !string.IsNullOrEmpty(token.Color))
            {
                _dashboardServices.CustomEventAdd(userId, token);
            }
            return Json(new List<DashboardEventToken> { token }.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UpdateCustomeEvent([DataSourceRequest] DataSourceRequest request, DashboardEventToken token,int userId)
        {
            if (token != null && !token.Uid.Equals(Guid.Empty) && token.Date != null && !string.IsNullOrEmpty(token.Name) && !string.IsNullOrEmpty(token.Color))
            {
                _dashboardServices.CustomEventUpdate(userId, token);
            }

            return Json(ModelState.ToDataSourceResult());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DestroyCustomeEvent([DataSourceRequest] DataSourceRequest request, DashboardEventToken token, int userId)
        {
            if (token != null && !token.Uid.Equals(Guid.Empty))
            {
                _dashboardServices.CustomEventRemove(userId, token);
            }

            return Json(ModelState.ToDataSourceResult());
        }
        #endregion
        #endregion

        public ActionResult GetEventWindow(int period, bool isCompare,int userId)
        {
            var periodKind = period.ToPeriodSelectionKind();

            var dates = isCompare ? _dashboardServices.TwoPeriodsDateRange(periodKind) : _dashboardServices.PeriodKindToDateRange(periodKind, false);

            var token = new EventWindowToken
            {
                DateRange = dates,
                UserId    = userId
            };

            return PartialView("Dashboard/_EventWindow", token);
        }

        public ActionResult GetSalesDetailsWindow(FiltersToken filter, DashboardEnums.eSaleBoxType type, bool compareBox)
        {
            var periodKind = filter.PeriodTypeId.ToPeriodSelectionKind();

            filter.DateRange = _dashboardServices.PeriodKindToDateRange(periodKind, compareBox);

            var token = new SalesDetailsWindowToken
            {
                Title         = String.Format("{0} for {1}-{2}", Utils.GetEnumDescription(type), filter.DateRange.from.ToString("MMMM dd, yyyy"), filter.DateRange.to.ToString("MMMM dd, yyyy"))
                ,Filter       = filter
                ,Type         = type
                ,IsCompareBox = compareBox
            };

            return PartialView("Dashboard/_SalesDetailWindow", token);
        }

        public ActionResult GetActiveSubscribersWindow(int userId)
        {
         
            var token = new SalesDetailsWindowToken
            {
                Title         = "Active Subscribers Report"
                ,Filter       = new FiltersToken{UserId = userId,DateRange = new DateRangeToken()}
                ,Type         = DashboardEnums.eSaleBoxType.ACTIVE_SUBSCRIBERS
                ,IsCompareBox = false
            };

            return PartialView("Dashboard/_SalesDetailWindow", token);
        }

        public ActionResult GetAuthorSales([DataSourceRequest] DataSourceRequest request, DashboardEnums.eSaleBoxType saleBoxType, short CurrencyId, int? StoreId, DateTime from, DateTime to,int userId)
        {
            var filter = new FiltersToken
            {
                CurrencyId = CurrencyId,
                DateRange  = new DateRangeToken { from = from, to = to },
                StoreId    = StoreId
            };
            var list = _dashboardServices.GetSalesRows(userId, saleBoxType, filter);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetRefunds([DataSourceRequest] DataSourceRequest request, DashboardEnums.eSaleBoxType saleBoxType, short CurrencyId, int? StoreId, DateTime from, DateTime to, int userId)
        {
            var filter = new FiltersToken
            {
                CurrencyId = CurrencyId,
                DateRange  = new DateRangeToken { from = from, to = to },
                StoreId    = StoreId
            };
            var list = _dashboardServices.GetRefundRows(userId, filter);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCouponsUsed([DataSourceRequest] DataSourceRequest request, DashboardEnums.eSaleBoxType saleBoxType, short CurrencyId, int? StoreId, DateTime from, DateTime to, int userId)
        {
            var filter = new FiltersToken
            {
                CurrencyId = CurrencyId,
                DateRange  = new DateRangeToken { from = from, to = to },
                StoreId    = StoreId
            };
            var list = _dashboardServices.GetCouponRows(userId, filter);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSubscriptionCancellation([DataSourceRequest] DataSourceRequest request, DashboardEnums.eSaleBoxType saleBoxType, short CurrencyId, int? StoreId, DateTime from, DateTime to, int userId)
        {
            var filter = new FiltersToken
            {
                CurrencyId = CurrencyId,
                DateRange  = new DateRangeToken { from = from, to = to },
                StoreId    = StoreId
            };
            var list = _dashboardServices.GetSubscriptionCancelRows(userId, filter);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetActiveSubscriptions([DataSourceRequest] DataSourceRequest request, int userId)
        {
            var list = _dashboardServices.GetActiveSubscribers(userId);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #region download monthly statement
        public ActionResult DownloadMonthlyStatement(int userId,short? currencyId = null)
        {
            var prevMonth = this.ToPrevMonthFirstDate();
            var request = new AuthorStatementRequestToken
            {
                currencyId = currencyId ?? Constants.DEFAULT_CURRENCY_ID,
                userId     = userId,
                year       = prevMonth.Year,
                month      = prevMonth.Month
            };
            
            var token = _billingManageServices.GetAuthorMonthlyStatementsPrintToken(request);

            var htmlContent = this.ToHtml("MonthlyStatement", token);

            var buffer = _pdfRenderer.Html2Pdf(htmlContent);
            var fileName = String.Format("{0}_{1}_monthly_statement.pdf", prevMonth.Year, prevMonth.Month);

            return new BinaryContentResult(buffer, "application/pdf", fileName);     
        } 
        #endregion
     
    
    }
}
