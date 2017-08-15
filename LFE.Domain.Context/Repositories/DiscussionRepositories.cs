using System;
using System.Collections.Generic;
using System.Linq;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    public class DiscussionClassRoomRepository : Repository<DSC_ClassRoom>, IDiscussionClassRoomRepository
    {
        public DiscussionClassRoomRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<DSC_AuhtorRoomToken> GetAuthorRooms(int authorId)
        {
            return DataContext.tvf_DSC_GetAuhtorRooms(authorId);
        }

        public IEnumerable<DSC_RoomMessageToken> GetRoomMessages(int roomId)
        {
            return DataContext.tvf_DSC_GetRoomMessages(roomId);
        }

        public IEnumerable<DSC_RoomMessageToken> GetHashtagMessages(long hashtagId)
        {
            return DataContext.tvf_DSC_GetHashtagMessages(hashtagId);
        }
    }

    public class DiscussionFollowersRepository : Repository<DSC_Followers>, IDiscussionFollowersRepository
    {
        public DiscussionFollowersRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class DiscussionHashtagRepository : Repository<DSC_Hashtags>, IDiscussionHashtagRepository
    {
        public DiscussionHashtagRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class DiscussionMessageRepository : Repository<DSC_Messages>, IDiscussionMessageRepository
    {
        public DiscussionMessageRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<DSC_NotificationsFbToken> GetMessageNotifications4FB(long messageId)
        {
            return DataContext.tvf_DSC_GetMessageNotifications4FB(messageId);
        }

        public IEnumerable<DSC_RoomMessageToken> GetMessageFeed(long messageId)
        {
            return DataContext.tvf_DSC_GetMessageFeed(messageId);
        }

        public DSC_RoomMessageToken GetMessage(Guid uid)
        {
            return DataContext.tvf_DSC_GetMessage(uid).FirstOrDefault();
        }
    }

    public class DiscussionMessageHashtagRepository: Repository<DSC_MessageHashtags>, IDiscussionMessageHashtagRepository
    {
        public DiscussionMessageHashtagRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class DiscussionMessageUserRepository : Repository<DSC_MessageUsers>, IDiscussionMessageUserRepository
    {
        public DiscussionMessageUserRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}
    }

    public class DiscussionMessageHashtagViewRepository : Repository<vw_DSC_MessageHashtags>, IDiscussionMessageHashtagViewRepository
    {
        public DiscussionMessageHashtagViewRepository(IUnitOfWork unitOfWork): base(unitOfWork){}
    }
}
