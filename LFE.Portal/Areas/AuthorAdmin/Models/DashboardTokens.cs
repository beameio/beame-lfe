using LFE.Core.Enums;
using LFE.DataTokens;
using System.Collections.Generic;

namespace LFE.Portal.Areas.AuthorAdmin.Models
{
    public class DashboardToken
    {
        public int UserId { get; set; }
        public List<BaseCurrencyDTO> Currencies { get; set; }
        public List<DashboardPayoutToken> NextPayoutList { get; set; }
        public DashboardStatsToken Stats { get; set; } 
        public FiltersToken Filters { get; set; }
        public List<BaseWebStoreDTO> StoreList { get; set; }
    }

   

    public class FiltersSectionModel
    {
        public FiltersToken Filters { get; set; }
        public string OnFiltersChanged { get; set; }
        public List<BaseCurrencyDTO> Currencies { get; set; }
        public List<BaseWebStoreDTO> StoreList { get; set; }

        public string GetFilter()
        {
            return "filters.getFilter()";
        }
    }

    public class ChartsModel
    {

        public string GetFilter { get; set; }
        public string GetGroupBy { get; set; }
        public string Reload()
        {
            return "charts.reload()";
        }
    }

    public class PeriodsToken
    {
        public DateRangeToken Period { get; set; }
        public DateRangeToken ComparePeriod { get; set; }
    }

    public class DashboardSaleBoxPageToken
    {
        public BaseCurrencyDTO Currency { get; set; }
        public DashboardSaleBoxToken StatsToken { get; set; }

        public bool ShowTitle { get; set; }

        public string BoxWidth { get; set; }

        public bool IsCompareBox { get; set; }
    }

    public class SalesDetailsWindowToken
    {
        public string Title { get; set; }

        public FiltersToken Filter { get; set; }

        public DashboardEnums.eSaleBoxType Type { get; set; }
        public bool IsCompareBox { get; set; }

        public dynamic QueryParams { get; set; }
    }

    public class EventWindowToken
    {
        public DateRangeToken DateRange { get; set; }

        public int UserId { get; set; }
    }

    public class NextPayoutViewToken
    {
        public List<DashboardPayoutToken> Statments { get; set; }

        public int UserId { get; set; }
    }

    public class SubscriptionReportViewToken : SalesDetailsWindowToken
    {
        public bool ShowActive { get; set; }

        public string Action { get; set; }
    }
}