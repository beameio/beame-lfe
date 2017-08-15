using System;
using System.Collections.Generic;
using System.Linq;
using LFE.Core.Utils;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    public class FactEventAggRepository : Repository<FACT_EventAgg>, IFactEventAggRepository
    {
        public FactEventAggRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}
    }

    public class FactDailyStatsRepository : Repository<FACT_DailyStats>, IFactDailyStatsRepository
    {
        public FactDailyStatsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public List<vw_USER_UserLogins> GetDailyUserLogins(DateTime date, bool returned)
        {
            return DataContext.tvf_FACT_GetDailyUserLogins(date, returned).ToList();
        }

        public List<vw_USER_UserLogins> GetDailyAuthorLogins(DateTime date)
        {
            return DataContext.tvf_FACT_GetDailyAuthorLogins(date).ToList();
        }

        public List<WS_WixStoreToken> GetDailyStoreStats(DateTime date,bool wixOnly)
        {
            return DataContext.tvf_FACT_GetDailySotreStats(date,wixOnly).ToList();
        }

        public List<USER_ItemToken> GetDailyItemStats(DateTime date,bool published)
        {
            return DataContext.tvf_FACT_GetDailyItemsStats(Constants.DEFAULT_CURRENCY_ID,date,published).ToList();
        }
    }

    public class FactDailyTotalsRepository : Repository<FACT_DailyTotals>, IFactDailyTotalsRepository
    {
        public FactDailyTotalsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class FactEventAggregatesViewRepository : Repository<vw_FACT_EventAggregates>, IFactEventAggregatesViewRepository
    {
        public FactEventAggregatesViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class EventLogsViewRepository : Repository<vw_EVENT_Logs>, IEventLogsViewRepository
    {
        public EventLogsViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
    
}
