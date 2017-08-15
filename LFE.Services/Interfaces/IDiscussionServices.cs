using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IAuthorAdminDiscussionServices : IDisposable
    {
        List<BaseListDTO> AuthorRoomsLOV(int authorId);
        List<AuthorRoomListDTO> GetAuthorClassRooms(int authorId);
        List<BaseListDTO> GetRoomsCourses(int roomId);
        DiscussionClassRoomDTO GetClassRoomDTO(int roomId);
        bool SaveClassRoom(ref DiscussionClassRoomDTO dto, int authorId,int userId, out string error);
        bool DeleteRoom(int roomId, out string error);
    }

    public interface IUserPortalDiscussionServices : IDisposable
    {
        List<UserTagItemDTO> FindUsers(string q);
        int? FindAuthorClassRoom(string name, int authorId);
        List<MessageViewDTO> GetRoomMessages(int roomId, DiscussionSortFields field, CommonEnums.SortDirections dir);
        List<MessageViewDTO> GetHashtagFeed(string hashtag, DiscussionSortFields field, CommonEnums.SortDirections dir, out long? hashtagId, out string error);
        List<MessageViewDTO> GetMessageFeed(Guid uid);
        MessageViewDTO GetMessageDTO(Guid uid);
        HashTagDTO GetHashTagDto(long tagId);
        bool SaveUserMessage(ref DiscussionMessageInputDTO token, int userId, out string error);

        //
        bool SaveClassRoom(ref DiscussionClassRoomDTO dto, int authorId, int userId, out string error);
    }
  
}
