using System;
using System.Collections.Generic;
using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class FbResponse
    {
        public long id { get; set; }
        public string name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string link { get; set; }
        public DateTime? birthday { get; set; }
        public FbLocation hometown { get; set; }
        public FbLocation location { get; set; }
        public string gender { get; set; }
        public string relationship_status { get; set; }
        public string email { get; set; }
        public string bio { get; set; }
        public List<dynamic> favorite_teams { get; set; }
        public short? timezone { get; set; }
        public string locale { get; set; }
        public bool verified { get; set; }
        public DateTime? updated_time { get; set; }
    }
  
    public class FbLocation
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class FbFriendDTO
    {
        public string name { get; set; }
        public long id { get; set; }
    }

    public class PostMessageDTO
    {
        public long? MessageId { get; set; }
        public int? UserId { get; set; }
        public long? UserFbId { get; set; }
        public string MessageTitle { get; set; }
        public string MessageUrl { get; set; }
        public string MessageText { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }        
        public bool IsAppPagePost { get; set; }
        public FbEnums.eFbActions? Action { get; set; }
        public int? CourseId { get; set; }
        public int? ChapterVideoID { get; set; }
    }

    public class FbUser
    {
        public long id { get; set; }
        public string name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string username { get; set; }        
    }
}
