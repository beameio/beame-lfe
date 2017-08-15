using System;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Model;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class EmailEntityMapper
    {
        public static EMAIL_Messages NotificationDto2EmailMessage(this NotificationMessageDTO token, string from, string subject, string htmlBody)
        {
            return new EMAIL_Messages
            {
                 MessageFrom    = from
                ,UserId         = token.UserId
                ,NotificationId = token.NotificationId
                ,Subject        = subject
                ,ToEmail        = token.Email
                ,ToName         = token.FullName
                ,AddOn          = DateTime.Now
                ,MessageBoby    = htmlBody
                ,Status         = EmailEnums.eSendInterfaceStatus.Waiting.ToString()
            };
        }

        public static EMAIL_Messages ReviewDto2EmailMessage(this MessageUserDTO token, string from, string subject, string htmlBody)
        {
            return new EMAIL_Messages
            {
                 MessageFrom    = from
                ,UserId         = token.id
                ,Subject        = subject
                ,ToEmail        = token.email
                ,ToName         = token.name
                ,AddOn          = DateTime.Now
                ,MessageBoby    = htmlBody
                ,Status         = EmailEnums.eSendInterfaceStatus.Waiting.ToString()
            };
        }

        public static EMAIL_Templates Token2EmailTemplateEntity(this EmailTemplateDTO token,int userId)
        {
            return new EMAIL_Templates
            {
                TemplateKindId = (short) token.Kind
                ,Snippet       = token.Snipet
                ,AddOn         = DateTime.Now
                ,CreatedBy     = userId
            };
        }

         public static EMAIL_Messages Dto2EmailMesageEntity(this EmailMessageDTO token)
        {
            return new EMAIL_Messages
            {
                Subject      = token.Subject
                ,UserId      = token.UserId
                ,MessageFrom = token.MessageFrom
                ,ToEmail     = token.ToEmail
                ,ToName      = token.ToName
                ,MessageBoby = token.MessageBody
                ,Status      =  token.Status.ToString()
                ,AddOn       = token.AddOn
            };
        }       
    }
}
