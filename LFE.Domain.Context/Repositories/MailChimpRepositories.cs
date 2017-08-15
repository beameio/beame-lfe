using System.Collections.Generic;
using System.Linq;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    public class ChimpUserListRepository : Repository<CHIMP_UserLists>, IChimpUserListRepository
    {
        public ChimpUserListRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<CHIMP_ItemSubscribers> GetAuthorSubscribers(int userId)
        {
            return DataContext.tvf_CHIMP_GetAuthorSubscribers(userId).ToList();
        }
    }

    public class ChimpListSegmentRepository : Repository<CHIMP_ListSegments>, IChimpListSegmentRepository
    {
        public ChimpListSegmentRepository(IUnitOfWork unitOfWork): base(unitOfWork)
        {
        }

        public List<CHIMP_MissingSegmentToken> GetMissingSegmentTokens(int userId, int listId)
        {
            return DataContext.tvf_CHIMP_GetMissingSegments(userId, listId).ToList();
        }

        public List<CHIMP_ItemSubscribers> GetItemSubscribers(int? itemId, int itemType)
        {
            return DataContext.tvf_CHIMP_GetItemSubscribers(itemId, itemType).ToList();
        }
    }

    public class ChimpRejectsRepository : Repository<CHIMP_Rejects>, IChimpRejectsRepository
    {
        public ChimpRejectsRepository(IUnitOfWork unitOfWork): base(unitOfWork)
        {
        }
        
    }
}
