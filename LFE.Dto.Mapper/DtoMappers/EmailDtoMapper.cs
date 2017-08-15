using System;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class EmailDtoMapper
    {
        public static NotificationMessageDTO Entity2NotificationMessageDto(this EMAIL_NotificationMessageToken entity)
        {
            return new NotificationMessageDTO
            {
                MessageId       = entity.MessageId
                ,UserId         = entity.UserId
                ,NotificationId = entity.NotificationId
                ,Email          = entity.Email
                ,FullName       = entity.Entity2FullName()
                ,PosterId       = entity.PosterId
                ,PosterName     = entity.Entity2PosterFullName()
                ,AddOn          = entity.AddOn
                ,MessageText    = entity.Text
                ,MessageHTML    = entity.HtmlEmailMessage
            };
        }

        public static EmailTemplateDTO Entity2EmailTemplateDto(this EMAIL_Templates entity)
        {
            return new EmailTemplateDTO
            {
                TemplateId = entity.TemplateId
                ,Kind      = Utils.ParseEnum<EmailEnums.eTemplateKinds>(entity.TemplateKindId)
                ,Snipet    = entity.Snippet
            };
        }

       
    }
}
