using System;
using System.Collections.Generic;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IEmailServices :IDisposable
    {
       
        //discussion notifications
        List<NotificationMessageDTO> GetMessageNotifications(long messageId);        
        bool SaveEmailNotificationRecord(NotificationMessageDTO token);

        //review
        bool SaveEmailReviewAuthorRecord(ReviewMessageDTO token);
        bool SaveEmailReviewLearnerRecord(ReviewMessageDTO token);

        //purchase
        void SavePurchaseMessages(EmailPurchaseToken token,bool send);

        //reset password
        void SaveResetPasswordMessage(EmailForgottenPasswordToken token, out long emailId, out string error);

        //Registration
        void SaveRegistrationActivationMessage(EmailRegistrationToken token, out long emailId);
        void SaveRegistrationMessage(EmailRegistrationToken token, out long emailId);
       
        //Monthly statement
        void SaveMonthlyStatementMessage(MonthlyStatementDTO token,string messageBody,out long emailId,out string error);

        //subscription cancel
        void SaveSubscriptionCancelMessage(EmailSubscriptionCancelToken token, out long emailId);

        //Refund program
        void SaveJoinRefundProgramMessage(EmailBaseToken token, out long emailId);
        void SaveGRPRefundSubmitted(EmailGRPSubmitted token, out long emailId);

       //certificate
        void SaveStudentCertificateMessage(StudentCertificateDTO token, string messageBody, out long emailId, out string error);

        //quiz
        void SaveQuizContactAuthorMessage(StudentMessageDTO token, out long emailId, out string error);
        void SaveQuizAuthorResponseMessage(StudentMessageDTO token, out long emailId, out string error);
    }

    public interface IPortalAdminEmailServices : IDisposable
    {
        EmailTemplateDTO GetTemplateDTO(short kindId);
        bool SaveTemplate(EmailTemplateDTO token, int userId, out string error);
    }
}
