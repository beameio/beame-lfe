using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IReportServices :IDisposable
    {
        List<UserEventDTO> GetUserEventLogs(int page,int size,int? userId, short? eventTypeId, ReportEnums.ePeriodSelectionKinds periodKind, int? courseId, int? bundleId,int? storeId,long? sessionId);
        List<UserEventDTO> GetUserEventLogs(ReportEnums.ePeriodSelectionKinds periodKind, int? userId, short? eventTypeId, int? courseId, int? bundleId, int? storeId, long? sessionId);
        List<FactDailyEventStatsDTO> GetEventStatsData(ReportEnums.ePeriodSelectionKinds period, int? userId, short? eventTypeId, int? courseId, int? bundleId, int? storeId);
        List<FactOwnerDailyEventStatsDTO> GetOwnerEventStatsData(ReportEnums.ePeriodSelectionKinds period);
        List<SystemLogDTO> GetSystemLogs(ReportEnums.ePeriodSelectionKinds periodKind,CommonEnums.LoggerObjectTypes? module, CommonEnums.eLogLevels? level,int? userId,long? sessionId,string ipAddress);
        List<FileInterfaceLogDTO> GetFileInterfaceLogs(ReportEnums.ePeriodSelectionKinds periodKind, int? userId,ImportJobsEnums.eFileInterfaceStatus? status);
        List<FbPostInterfaceLogDTO> GetFbPostLogs(ReportEnums.ePeriodSelectionKinds periodKind, int? userId, FbEnums.ePostInterfaceStatus? status);
        List<EmailInterfaceLogDTO> GetEmailInterfaceLogs(ReportEnums.ePeriodSelectionKinds periodKind, int? userId, EmailEnums.eSendInterfaceStatus? status);
        List<VideoDTO> GetVideosReport( ReportEnums.ePeriodSelectionKinds periodKind,int? userId,bool? attachedOnly);
        List<ItemListDTO> GetAuthorItemsList(int? authorId,int? itemId, CourseEnums.CourseStatus? status);
        List<BaseListDTO> FindCourses(int? courseId = null, int? userId = null, string name = null);

        List<BaseListDTO> FindBundles(int? bundleId = null, int? userId = null, string name = null);
        List<BaseEntityDTO> FindStores(int? storeId = null, int? userId = null, string name = null);
        List<StoreReportDTO> GetStoresReport(int? ownerId, CommonEnums.eRegistrationSources? source, bool? isAffiliate);
        SummaryReportDTO GetSummaryReport(ReportEnums.ePeriodSelectionKinds periodKind, ReportEnums.eChartGroupping groupBy);
        List<HostEventDTO> GetHostEventsReport(ReportEnums.ePeriodSelectionKinds periodKind, ReportEnums.eChartGroupping groupBy,string host);
        List<AbandonHostDTO> GetHostAbandon(DateTime from, DateTime last);
        SaleSummaryReportDTO GetSalesSummaryReport(int year, int month,int? userId);
        

        //KPI
        KpiDashboardFiltersToken GetKpiDashboardFilters();
        List<KpiViewsChartDTO> GetKpiData(ReportEnums.ePeriodSelectionKinds period, int? authorId, int? storeId,int? itemId, CommonEnums.eEventItemTypes? itemType);
        List<KpiDetailRowDTO> GetKpiDataRows(ReportEnums.ePeriodSelectionKinds period, int? authorId,int? storeId, int? itemId, CommonEnums.eEventItemTypes? itemType);
        List<FunnelViewsChartDTO> GetFunnelChartData(ReportEnums.ePeriodSelectionKinds period, int? authorId,int? storeId, int? itemId, CommonEnums.eEventItemTypes? itemType);
        List<StatementKpiChartDTO> GetStatementKpiData(AuthorStatementRequestToken token);

        List<FactDailyStatsDTO> GetDailyStatsData(ReportEnums.ePeriodSelectionKinds period);
        
        List<VideoUploadsChartDTO> GetDailyVideoStatsData(ReportEnums.ePeriodSelectionKinds period);
        List<FactDailyTotalsDTO> GetDailyTotalsData(ReportEnums.ePeriodSelectionKinds period);
        List<VideoDTO> GetDailyStatsVideoData(DateTime date, ReportEnums.eDailyStatsFields field);
        List<UserGridViewDto> GetDailyStatsUserData(DateTime date, ReportEnums.eDailyStatsFields field);
        List<ItemListDTO> GetDailyStatsItemData(DateTime date, ReportEnums.eDailyStatsFields field);
        List<WixStoreDTO> GetDailyStatsStoreData(DateTime date, ReportEnums.eDailyStatsFields field);
        List<OrderLineDTO> GetDailyStatsPurchaseData(DateTime date, ReportEnums.eDailyStatsFields field);
        List<PluginRepDTO> GetInstallationsReport(ReportEnums.ePeriodSelectionKinds? period, int? userId, int? typeId, bool? isactive);
    }
}
