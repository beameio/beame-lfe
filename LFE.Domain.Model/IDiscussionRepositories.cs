using System;
using System.Collections.Generic;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    public interface IDiscussionClassRoomRepository : IRepository<DSC_ClassRoom>
    {
        IEnumerable<DSC_AuhtorRoomToken> GetAuthorRooms(int authorId);
        IEnumerable<DSC_RoomMessageToken> GetRoomMessages(int roomId);
        IEnumerable<DSC_RoomMessageToken> GetHashtagMessages(long hashtagId);
    }

    public interface IDiscussionFollowersRepository : IRepository<DSC_Followers>
    {
       
    }

    public interface IDiscussionHashtagRepository : IRepository<DSC_Hashtags>
    {
        
    }

    public interface IDiscussionMessageRepository : IRepository<DSC_Messages>
    {
        IEnumerable<DSC_NotificationsFbToken> GetMessageNotifications4FB(long messageId);
        IEnumerable<DSC_RoomMessageToken> GetMessageFeed(long messageId);
        DSC_RoomMessageToken GetMessage(Guid uid);
    }

    public interface IDiscussionMessageHashtagRepository : IRepository<DSC_MessageHashtags>
    {

    }
    public interface IDiscussionMessageUserRepository : IRepository<DSC_MessageUsers>
    {

    }

    public interface IDiscussionMessageHashtagViewRepository : IGetRepository<vw_DSC_MessageHashtags> { }
}
