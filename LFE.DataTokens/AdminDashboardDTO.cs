using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.Core.Utils;

namespace LFE.DataTokens
{

   
    public class TendencyToken
    {
        public decimal Percent { get; set; }

        public ReportEnums.eTendencyDirections Direction { get; set; }

        public string Tooltip { get; set; }
    }

    public class AdminPayoutToken 
    {
         public BaseCurrencyDTO Currency { get; set; }
        public decimal TotalSales { get; set; }
        public decimal LfeCommission { get; set; }
        public decimal LicenseFee { get; set; }

        public decimal TotalRevenue
        {
            get { return (LfeCommission + LicenseFee).FormatMoney(0); }
        }

        public TendencyToken Tendency { get; set; }
      
    }

    public class TopSellersToken
    {
        public TopSellersToken()
        {
            Top_USD = new List<AuthorPayoutToken>();
            Top_EUR = new List<AuthorPayoutToken>();
        }
        public List<AuthorPayoutToken> Top_USD { get; set; }
        public List<AuthorPayoutToken> Top_EUR { get; set; }
    }

    public class AuthorPayoutToken
    {
        public decimal Sales { get; set; }
        public BaseUserInfoDTO Seller { get; set; }
        public BaseCurrencyDTO Currency { get; set; }
    }

    public class AdminDashboardToken : BaseModelState
    {
        public List<BaseCurrencyDTO> Currencies { get; set; }
        public List<AdminPayoutToken> NextPayoutList { get; set; }
        public TopSellersToken TopSellers { get; set; }
        public AdminVideoStatsToken VideoStats { get; set; }

        public AuthorTotalStatsToken AuthorTotalStats { get; set; }
    }

    public class AdminDashboardFiltersToken
    {
        public ReportEnums.ePeriodSelectionKinds period { get; set; }
        public short currencyId { get; set; }
    }

    public class PlatformStatsToken
    {
        public CommonEnums.eRegistrationSources Platform { get; set; }
        public int TotalPlatformNew { get; set; }
        public TendencyToken Tendency { get; set; }
        public short Index { get; set; }
        public List<PlatformStatsBoxToken> Stats { get; set; }
    }

    public class PlatformStatsBoxToken : BaseStatsBoxToken
    {
        public List<BaseChartPointToken> Points { get; set; }
    }

    public class TotalsBoxToken : BaseStatsBoxToken
    {
        
    }
    public class SalesTotalsBoxToken : BaseStatsBoxToken
    {
        public decimal TotalIncome { get; set; }
        public BaseCurrencyDTO Currency { get; set; }
    }
    public class BaseStatsBoxToken
    {
        public ReportEnums.eStatsTypes Type { get; set; }

        public short Index { get; set; }

        public int Total { get; set; }

        public int New { get; set; }
        public TendencyToken Tendency { get; set; }
    }

    public class AdminVideoStatsToken
    {
        public int TotalUploaded { get; set; }

        public int CourseAttached { get; set; }

        public int NotAttached { get; set; }

        public int TotalPreviews { get; set; }
    }

    public class IntegrationStatsToken
    {
        public int TotalMailchimp { get; set; }
        public int MbgJoined { get; set; }

        public int MbgCanceled { get; set; }
    }

    public class AuthorTotalStatsToken
    {
        public decimal AverageCourseChapters { get; set; }
        public decimal AverageCoursesPerAuthor { get; set; }
        public decimal AverageBundlesPerAuthor { get; set; }

        public int TotalFreeCourses { get; set; }

        public decimal AverageFreeCoursesPerAuthor { get; set; }
    }

    public class AuthorPeriodStatsBoxToken : BaseStatsBoxToken
    {
        public decimal Average { get; set; }

        public string DisplayedValue { get; set; }       
    }

    public class LearnerPeriodStatsBoxToken : BaseStatsBoxToken
    {
        public decimal Average { get; set; }

        public string DisplayedValue { get; set; }

        public LearnerPeriodStatsBoxToken Related { get; set; }
    }
}
