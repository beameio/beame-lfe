using LFE.DataTokens;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class DiscussionEntityMapper
    {
        public static DSC_ClassRoom Dto2ClassRoomEntity(this DiscussionClassRoomDTO dto,int authorId, int userId)
        {
            return new DSC_ClassRoom
            {
                 AuthorId   = authorId
                ,Name       = dto.Name
                ,IsActive   = dto.IsActive
                ,AddOn      = DateTime.Now
                ,CreatedBy  = userId
            };
        }

        public static void UpdateClassRoomEntity(this DSC_ClassRoom entity, DiscussionClassRoomDTO dto, int userId)
        {
            entity.Name       = dto.Name;
            entity.IsActive   = dto.IsActive;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = userId;
        }

        public static DSC_Messages Dto2MessageEntity(this DiscussionMessageInputDTO dto, int userId)
        {
            return new DSC_Messages
            {
                ClassRoomId      = dto.RoomId
                ,Uid             = Guid.NewGuid()
                ,ParentMessageId = dto.ParentMessageId
                ,CourseId        = dto.CourseId
                ,MessageKindId   = (short) dto.Kind
                ,Text            = dto.UserMessage
                ,HtmlVersion     = dto.HTMLVersion
                ,AddOn           = DateTime.Now
                ,CreatedBy       = userId
            };
        }
    }
}
