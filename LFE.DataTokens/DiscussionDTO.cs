using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class AuthorRoomListDTO 
    {
        [Key]
        public int RoomId { get; set; }

        [DisplayName("Room Name")]
        public string Name { get; set; }

        [DisplayName("Courses")]
        public int CourseCount { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; }

        [DisplayName("Add on")]
        public DateTime AddOn { get; set; }
    }

    public class DiscussionClassRoomDTO
    {

        public DiscussionClassRoomDTO()
        {
            RoomId = -1;
            IsActive = true;
        }     

        [Key]
        public int RoomId { get; set; }
        
        public int AuthorId { get; set; }

        [Required]
        [DisplayName("Room Name")]
        public string Name { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; }

        [DisplayName("Add on")]
        public DateTime AddOn { get; set; }
    }

    public class CourseDiscussionToken
    {
        public int RoomId { get; set; }

        public int CourseId { get; set; }

        public string Name { get; set; }

        public MessageViewDTO[] Messages { get; set; } 
    }

    public class HashTagDTO
    {
        public HashTagDTO()
        {
            HashtagId = -1;
            HashTag = string.Empty;
        }
        public long HashtagId { get; set; }
        
        public string HashTag { get; set; }
    }

    public class DiscussionMessageBaseDTO
    {
        public int RoomId { get; set; }
        
        public int CourseId { get; set; }

        public string Message { get; set; }

        [AllowHtml]
        public string HTMLMessage { get; set; }
        
        public int HTMLVersion { get; set; }

        public string WriterName { get; set; }

        public string WriterPhotoUrl { get; set; }

        public eMessageKinds Kind { get; set; }        
        
    }

    public class DiscussionMessageInputDTO : DiscussionMessageBaseDTO
    {
        public DiscussionMessageInputDTO()
        {
            MessageId = -1;
            Kind      = eMessageKinds.USER_MESSAGE;
        }
        
        public long MessageId { get; set; }

        public long? ParentMessageId { get; set; }
        public Guid? Uid { get; set; }

        [Required]
        [DisplayName("Message")]
        public string UserMessage { get; set; }

        public string NamesArrayStr { get; set; }
        public string TagsArrayStr { get; set; }              
    }

    public class MessageViewDTO
    {
        public MessageViewDTO()
        {
            HasReplies = false;
            Replies = new List<MessageViewDTO>();
        }

        public long MessageId { get; set; }
        
        public long? ParentMessageId { get; set; }

        public Guid Uid { get; set; }

        public eMessageKinds Kind { get; set; } 

        [AllowHtml]
        public string HTMLMessage { get; set; }

        public string MessageText { get; set; }

        public int CreatorId { get; set; }

        public string CreatorName {get; set;}

        public string CreatorPhotoUrl { get; set; }

        public string PostedOn { get; set; }

        public DateTime AddOn { get; set; }

        public string RoomName { get; set; }
        
        public string CourseName { get; set; }

        public List<MessageViewDTO> Replies { get; set; } 

        public bool HasReplies { get; set; }
    }
}
