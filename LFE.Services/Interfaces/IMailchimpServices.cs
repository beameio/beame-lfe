using System;
using LFE.DataTokens;
using System.Collections.Generic;

namespace LFE.Application.Services.Interfaces
{
    public interface IMailchimpServices :  IDisposable
    {
        ChimpUserListDTO GetUserListDto(int userId);
        bool SaveUserList(ChimpUserListDTO token, out string error);

        bool SaveListSubscribers(int listId, out string error);

        bool SaveListSegment(int listId, out string error);

        bool GetListSegments(ChimpUserListDTO token);

        List<ChimpSegmentNameToken> GetMissingSegments(int userId, int listId);
    }
}
