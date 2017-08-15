using System;
using System.Collections.Generic;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    public interface IFactEventAggRepository : IRepository<FACT_EventAgg>{}

    public interface IFactDailyStatsRepository : IRepository<FACT_DailyStats>
    {
        List<vw_USER_UserLogins> GetDailyUserLogins(DateTime date, bool returned);
        List<vw_USER_UserLogins> GetDailyAuthorLogins(DateTime date);
        List<WS_WixStoreToken> GetDailyStoreStats(DateTime date, bool wixOnly);
        List<USER_ItemToken> GetDailyItemStats(DateTime date, bool published);
    }

    public interface IFactDailyTotalsRepository : IRepository<FACT_DailyTotals>
    {
        
    }

    public interface IFactEventAggregatesViewRepository : IGetRepository<vw_FACT_EventAggregates> { }

    public interface IEventLogsViewRepository : IGetRepository<vw_EVENT_Logs> { }
}
