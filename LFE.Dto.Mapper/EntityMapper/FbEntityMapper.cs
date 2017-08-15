using System;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Model;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class FbEntityMapper
    {
        public static FB_PostInterface Dto2FbPostInterfaceEntity(this PostMessageDTO token, long? notificationId = null)
        {
            return new FB_PostInterface
            {
                 UserId         = token.UserId
                ,FbUid          = token.UserFbId
                ,Title          = token.MessageTitle
                ,Message        = token.MessageText
                ,LinkedName     = token.MessageUrl
                ,Caption        = token.Caption
                ,Description    = token.Description
                ,ImageUrl       = token.ImageUrl
                ,ActionId       = token.Action==null ? null : (byte?)token.Action
                ,AddOn          = DateTime.Now
                ,Status         = FbEnums.ePostInterfaceStatus.Waiting.ToString()
                ,NotificationId = notificationId
                ,IsAppPagePost  = token.IsAppPagePost
                ,CourseId       = token.CourseId
                ,ChapterVideoId = token.ChapterVideoID
            };
        }
    }
}
