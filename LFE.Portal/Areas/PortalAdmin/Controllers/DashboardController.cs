using LFE.Application.Services.Interfaces;
using LFE.DataTokens;
using System.Web.Mvc;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly IAdminDashboardServices _dashboardServices;

        public DashboardController()
        {
            _dashboardServices = DependencyResolver.Current.GetService<IAdminDashboardServices>();
        }

        public ActionResult Index()
        {
            var token = _dashboardServices.GetAdminDashboardToken();

            return View(token);
        }

        //public ActionResult LoadPeriodStats(ReportEnums.ePeriodSelectionKinds? period = ReportEnums.ePeriodSelectionKinds.lastMonth, short? currencyId = Constants.DEFAULT_CURRENCY_ID)
        //{
        //    return PartialView("Dashboard/_PeriodStats", new AdminDashboardFiltersToken { period = Utils.ParseEnum<ReportEnums.ePeriodSelectionKinds>(period.ToString()), currencyId = currencyId ?? Constants.DEFAULT_CURRENCY_ID });
        //}

        public ActionResult LoadPlatformStats(AdminDashboardFiltersToken filter)
        {
            var list = _dashboardServices.GetPlatformStats(filter.period);
            return PartialView("Dashboard/_PlatformStats",list);
        }

        public ActionResult LoadTotals(AdminDashboardFiltersToken filter)
        {

            var list = _dashboardServices.GetTotals(filter.period);
            return PartialView("Dashboard/_Totals",list);
        }

        public ActionResult LoadSalesTotals(AdminDashboardFiltersToken filter)
        {

            var list = _dashboardServices.GetSalesTotals(filter);
            return PartialView("Dashboard/_SalesTotals", list);
        }

        public ActionResult LoadIntegrationTotals(AdminDashboardFiltersToken filter)
        {

            var token = _dashboardServices.GetIntegrationStatsToken(filter.period);
            return PartialView("Dashboard/_IntegrationStats", token);
        }

        public ActionResult LoadAuthorPeriodTotals(AdminDashboardFiltersToken filter)
        {

            var list = _dashboardServices.GetAuthorPeriodStats(filter.period);
            return PartialView("Dashboard/_AuthorPeriodStatsRow", list);
        }

        public ActionResult LoadLearnerPeriodTotals(AdminDashboardFiltersToken filter)
        {
            var list = _dashboardServices.GetLearnerPeriodStats(filter.period);
            return PartialView("Dashboard/_LearnerPeriodStatsRow", list);
        }

        public ActionResult LoadLearnerPeriodCouponTotals(AdminDashboardFiltersToken filter)
        {
            var token = _dashboardServices.GetLearnerPeriodCouponStats(filter);
            return PartialView("Dashboard/_LearnerPeriodStatsBox", token);
        }
    }
}
