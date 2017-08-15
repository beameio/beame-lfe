using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IAdminDashboardServices
    {
        AdminDashboardToken GetAdminDashboardToken();
        List<PlatformStatsToken> GetPlatformStats(ReportEnums.ePeriodSelectionKinds period);
        List<TotalsBoxToken> GetTotals(ReportEnums.ePeriodSelectionKinds period);
        List<AuthorPeriodStatsBoxToken> GetAuthorPeriodStats(ReportEnums.ePeriodSelectionKinds period);
        List<LearnerPeriodStatsBoxToken> GetLearnerPeriodStats(ReportEnums.ePeriodSelectionKinds period);

        LearnerPeriodStatsBoxToken GetLearnerPeriodCouponStats(AdminDashboardFiltersToken filter);
        List<SalesTotalsBoxToken> GetSalesTotals(AdminDashboardFiltersToken filter);
        AdminVideoStatsToken GetVideoStats();
        IntegrationStatsToken GetIntegrationStatsToken(ReportEnums.ePeriodSelectionKinds period);
        
    }
}