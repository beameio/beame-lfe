using System;
using LFE.Core.Enums;
using LFE.DataTokens;
using System.Collections.Generic;

namespace LFE.Application.Services.Interfaces
{
    public interface IDashboardServices
    {
        List<BaseCurrencyDTO> GetUserCurrencies(int userId);
        DashboardStatsToken GetAuthorDashboardStats(int userId);
        DashboardSaleBoxTokenLists GetSales(FiltersToken filter, int userId);
        List<DashboardPayoutToken> GetNextPayout(int userId);
        List<DashboardKpiChartDTO> GetChartData(FiltersToken filter, int userId, bool isCompareChart);

        List<DashboardEventToken> GetDashboardEvents(int userId, DateRangeToken dates);
        DashboardEventToken GetDashboardEventToken(int userId, DashboardEnums.eDbEventTypes type, DateRangeToken dates, string eventName = null);
       // DashboardEventToken GetDashboardCustomEventToken(DateTime date, string name);

        //sales box details
        List<BaseOrderLineDTO> GetSalesRows(int userId, DashboardEnums.eSaleBoxType type, FiltersToken filter);
        List<BaseOrderLineDTO> GetCouponRows(int userId, FiltersToken filter);
        List<DbRefundDetailToken> GetRefundRows(int userId, FiltersToken filter);
        List<DbSubscriptionDetailToken> GetSubscriptionCancelRows(int userId, FiltersToken filter);
        List<DbSubscriptionDetailToken> GetActiveSubscribers(int userId);
        //helper
        DateRangeToken TwoPeriodsDateRange(ReportEnums.ePeriodSelectionKinds period);
        DateRangeToken PeriodKindToDateRange(ReportEnums.ePeriodSelectionKinds period, bool isInCompareMode);
        List<DashboardEventToken> CustomEventGetList(int userId);
        bool CustomEventAdd(int userId, DashboardEventToken token);
        bool CustomEventRemove(int userId, DashboardEventToken token);
        bool CustomEventUpdate(int userId, DashboardEventToken token);
    }
}
