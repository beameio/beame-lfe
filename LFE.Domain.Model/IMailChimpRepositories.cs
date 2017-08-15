using System.Collections.Generic;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    public interface IChimpUserListRepository : IRepository<CHIMP_UserLists>
    {
        List<CHIMP_ItemSubscribers> GetAuthorSubscribers(int userId);
    }

    public interface IChimpListSegmentRepository : IRepository<CHIMP_ListSegments>
    {
        List<CHIMP_MissingSegmentToken> GetMissingSegmentTokens(int userId, int listId);

        List<CHIMP_ItemSubscribers> GetItemSubscribers(int? itemId, int itemType);
    }

    public interface IChimpRejectsRepository : IRepository<CHIMP_Rejects>
    {       
    }
}
