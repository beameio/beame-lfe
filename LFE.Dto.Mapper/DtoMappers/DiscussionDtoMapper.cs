using System;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class DiscussionDtoMapper
    {
        public static AuthorRoomListDTO Entity2AuthorRoomListDto(this DSC_AuhtorRoomToken entity)
        {
            return new AuthorRoomListDTO
            {
                 RoomId      = entity.ClassRoomId
                ,Name        = entity.Name
                ,IsActive    = entity.IsActive
                ,CourseCount = entity.Cnt
                ,AddOn       = entity.AddOn
            };
        }

        public static DiscussionClassRoomDTO Entity2ClassRoomDto(this DSC_ClassRoom entity)
        {
            return new DiscussionClassRoomDTO
            {
                 RoomId      = entity.ClassRoomId
                ,Name        = entity.Name
                ,IsActive    = entity.IsActive
                ,AuthorId    = entity.AuthorId
                ,AddOn       = entity.AddOn
            };
        }

        public static BaseListDTO RoomEntity2BaseListDto(this DSC_ClassRoom entity)
        {
            return new BaseListDTO
            {
                id    = entity.ClassRoomId
                ,name = entity.Name
            };
        }

        public static HashTagDTO Entity2HashTagDto(this DSC_Hashtags entity)
        {
            return new HashTagDTO
            {
                HashtagId = entity.HashtagId
                ,HashTag = entity.HashTag
            };
        }

        public static MessageViewDTO Entity2MessageViewDto(this DSC_RoomMessageToken entity)
        {
            return new MessageViewDTO
            {
                 MessageId       = entity.MessageId
                ,ParentMessageId = entity.ParentMessageId
                ,Uid             = entity.Uid
                ,Kind            = Utils.ParseEnum<eMessageKinds>(entity.MessageKindId)
                ,HTMLMessage     = entity.HtmlMessage
                ,MessageText     = entity.Text
                ,PostedOn        = ((DateTimeOffset)entity.AddOn).ToVerbalDateSinceNow()
                ,AddOn           = entity.AddOn
                ,CreatorId       = entity.UserId
                ,CreatorName     = entity.Entity2FullName()
                ,CreatorPhotoUrl = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                ,CourseName      = entity.CourseName
                ,RoomName        = entity.RoomName
            };
        }

        
    }
}
