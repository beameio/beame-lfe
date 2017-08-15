using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;

namespace LFE.Application.Services
{
    public class EmailServices : ServiceBase, IEmailServices,IPortalAdminEmailServices
    {
        #region constants
        private static readonly string FROM                             =  Utils.GetKeyValue("AWSEmailFrom");
        private const string DISCUSSION_SUBJECT                         = "You appear in LFE discussion";
        private static readonly string REVIEW_AUTHOR_SUBJECT            = $"Your course on {APP_OFFICIAL_NAME} has been reviewed";
        private static readonly string REVIEW_LEARNER_SUBJECT           = $"A review has been posted on {APP_OFFICIAL_NAME}";
        private static readonly string PURCHASE_AUTHOR_SUBJECT          = "Your {0} on " + APP_OFFICIAL_NAME + " has been purchased";
        private static readonly string PURCHASE_LEARNER_SUBJECT         = $"Confirmation of your purchase on {APP_OFFICIAL_NAME}";
        private static readonly string REGISTRATION_ACTIVATION_SUBJECT  = $"Confirm your Registration to {APP_OFFICIAL_NAME}";
        private static readonly string REGISTRATION_SUBJECT             = $"Welcome to {APP_OFFICIAL_NAME}";
        private const string FORGOTTEN_PASSWORD_SUBJECT                 = "Password reset request";
        private const string REFUND_PROGRAM_JOIN_SUBJECT                = "Refund Guaranteed Program";
        private const string GRP_REFUND_SUBMITTED_SUBJECT               = "Refund Guaranteed Program";
        private const string AUHTOR_SUBSCRIPTION_CANCEL_ALERT_SUBJECT   = "{0}, Subscription Cancellation Notification";
        private const string STUDENT_CERTIFICATE_SUBJECT                = "{0} course certificate";
        private const string QUIZ_CONTACT_AUTHOR_SUBJECT                = "Student Contact request – {0}";
        private const string QUIZ_AUTHOR_RESPONSE_SUBJECT               = "Quiz {0} attempt update";
        #endregion 

        #region private parameters
        private readonly IAmazonEmailWrapper _amazonEmailWrapper;
        private readonly Dictionary<EmailEnums.eTemplateKinds, string> _templateDictionary;
        #endregion
        
        #region .ctor
        public EmailServices()
        {
            _templateDictionary = GetTemplateDictionary();
            _amazonEmailWrapper = DependencyResolver.Current.GetService<IAmazonEmailWrapper>();
        }
        #endregion

        #region IEmailServices implementation

        #region private helpers
        private Dictionary<EmailEnums.eTemplateKinds, string> GetTemplateDictionary()
        {
            const string cacheKey = "EMAIL_TEMPLATES";

            if (!IsDebugMode)
            {
                var result = CacheProxy.Get<Dictionary<EmailEnums.eTemplateKinds, string>>(cacheKey);

                if (result != null) return result;    
            }
            
            var res = new Dictionary<EmailEnums.eTemplateKinds, string>();

            foreach (var p in EmailTemplateRepository.GetAll())
            {
                string temp;
                EmailEnums.eTemplateKinds key;

                try
                {
                    key = Utils.ParseEnum<EmailEnums.eTemplateKinds>(p.TemplateKindId);
                }
                catch (Exception)
                {
                    continue;
                }

                if (res.TryGetValue(key, out temp)) res.Remove(key);

                res.Add(key, p.Snippet);
            }

            if (IsDebugMode) return res;

            CacheProxy.Add(cacheKey, res, DateTimeOffset.Now.AddDays(30));

            return res;
        }

        private string GetTemplateHtml(EmailEnums.eTemplateKinds kind)
        {
            string html;

            var found = _templateDictionary.TryGetValue(kind, out html);

            return found ? html : string.Empty;
        }
      
        private void SaveMessage(EmailMessageDTO messageToken,out long emailId)
        {
            try
            {
                var entity = messageToken.Dto2EmailMesageEntity();

                EmailMessageRepository.Add(entity);

                EmailMessageRepository.UnitOfWork.CommitAndRefreshChanges();

                emailId = entity.EmailId;
            }
            catch (Exception ex)
            {
                emailId = -1;
                Logger.Error("save email message", null, ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }

        #region purchase
        private void SavePurchaseAuthor(EmailPurchaseToken token, bool send)
        {
            try
            {
                var html = token.PaymentMethod == BillingEnums.ePaymentMethods.Charge_Free && token.Coupon == null ? GetTemplateHtml(EmailEnums.eTemplateKinds.PURCHASE_AUTHOR_FREE) : GetTemplateHtml(EmailEnums.eTemplateKinds.PURCHASE_AUTHOR);

                var authorMessage = new EmailMessageDTO
                {
                    Subject      = String.Format(PURCHASE_AUTHOR_SUBJECT, token.Item.type.ToString().ToLower())
                    ,MessageFrom = FROM
                    ,UserId      = token.Seller.UserId
                    ,ToEmail     = token.Seller.Email
                    ,ToName      = token.Seller.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.COURSE_AUTHOR.ToString().Field2Template(), token.Seller.FullName)  
                                       .Replace(EmailEnums.eTemplateFields.COURSE_NAME.ToString().Field2Template(), token.Item.name)
                                       .Replace(EmailEnums.eTemplateFields.ITEM_TYPE_NAME.ToString().Field2Template(), token.Item.type.ToString().ToLower())
                                       .Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), token.Buyer.FullName)
                                       .Replace(EmailEnums.eTemplateFields.PURCHASE_DATE.ToString().Field2Template(), token.SentDate.ToShortDateString() + " " +  token.SentDate.ToShortTimeString())                    
                };

                long emailId;
                SaveMessage(authorMessage,out emailId);
                
                if (emailId < 0 || !send) return;
                
                string error;
                _amazonEmailWrapper.SendEmail(emailId, out error);
            }
            catch (Exception ex)
            {
                Logger.Error("save purchase author email message", null, ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }

        private void SavePurchaseLearner(EmailPurchaseToken token, bool send)
        {
            try
            {

                string html;
                string agreementDescription;

                var couponName = token.Coupon != null ? "Coupon discount" : string.Empty;
                string paymentInstrumentDesc = null;

                if (token.PaymentMethod == BillingEnums.ePaymentMethods.Charge_Free)
                {
                    html = GetTemplateHtml(token.Coupon != null ? EmailEnums.eTemplateKinds.PURCHASE_LEARNER_WITH_FREE_COUPON : EmailEnums.eTemplateKinds.PURCHASE_LEARNER_FREE);
                    agreementDescription = token.Coupon != null ? (token.PriceLineToken != null ? token.PriceLineToken.PriceLineToken2PurchaseEmailPaymentMethod() : "" ) : "Free Course";
                }
                else
                {
                    html = GetTemplateHtml(token.Coupon != null ? EmailEnums.eTemplateKinds.PURCHASE_LEARNER_WITH_COUPON : EmailEnums.eTemplateKinds.PURCHASE_LEARNER);
                    agreementDescription = token.PriceLineToken.PriceLineToken2PurchaseEmailPaymentMethod();
                    paymentInstrumentDesc = String.Format("{0} {1}", token.PaymentMethod == BillingEnums.ePaymentMethods.Paypal ? "Paypal account" : "credit card",token.PaymentTerm == BillingEnums.ePaymentTerms.EVERY_30 ? "every month" : string.Empty );
                }
                
                var learnerMessage = new EmailMessageDTO
                {
                    Subject      = PURCHASE_LEARNER_SUBJECT
                    ,MessageFrom = FROM
                    ,UserId      = token.Buyer.UserId
                    ,ToEmail     = token.Buyer.Email
                    ,ToName      = token.Buyer.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = token.SentDate
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), token.Buyer.FullName)
                                        .Replace(EmailEnums.eTemplateFields.COURSE_AUTHOR.ToString().Field2Template(), token.Seller.FullName)
                                        .Replace(EmailEnums.eTemplateFields.THUMB_URL.ToString().Field2Template(), token.Item.thumbUrl)
                                        .Replace(EmailEnums.eTemplateFields.ITEM_TYPE_NAME.ToString().Field2Template(), token.Item.type.ToString().ToLower())
                                        .Replace(EmailEnums.eTemplateFields.ISO.ToString().Field2Template(),token.PriceLineToken != null ? token.PriceLineToken.Currency.ISO : string.Empty)
                                        .Replace(EmailEnums.eTemplateFields.CURRENCY_SYMBOL.ToString().Field2Template(), token.PriceLineToken != null ? token.PriceLineToken.Currency.Symbol : string.Empty)
                                        .Replace(EmailEnums.eTemplateFields.AGREEMENT_DESC.ToString().Field2Template(), agreementDescription)
                                        .Replace(EmailEnums.eTemplateFields.COUPON_NAME.ToString().Field2Template(), couponName)
                                        .Replace(EmailEnums.eTemplateFields.PAYMENT_INSTRUMENT_DESC.ToString().Field2Template(),paymentInstrumentDesc )
                                        .Replace(EmailEnums.eTemplateFields.PRICE.ToString().Field2Template(), token.Price != null ? token.Price.FormatPrice2Str(2) : string.Empty)
                                        .Replace(EmailEnums.eTemplateFields.DISCOUNT.ToString().Field2Template(), token.Discount != null ? token.Discount.FormatPrice2Str(2) : string.Empty)                                        
                                        .Replace(EmailEnums.eTemplateFields.TOTAL_PRICE.ToString().Field2Template(), token.TotalAmount != null ? token.TotalAmount.FormatPrice2Str(2) : string.Empty)
                                        .Replace(EmailEnums.eTemplateFields.ITEM_PAGE_URL.ToString().Field2Template(), token.Item.pageUrl)
                                        .Replace(EmailEnums.eTemplateFields.COURSE_NAME.ToString().Field2Template(), token.Item.name) //"<a style=\"" + NAME_HTML_STYLE_UNDERLINE + "\"  href=\"" + token.Item.pageUrl + "\" >" + token.Item.name + "</a>"
                };

                long emailId;
                SaveMessage(learnerMessage,out emailId);
               
                if(emailId<0 || !send) return;
                string error;
                _amazonEmailWrapper.SendEmail(emailId, out error);
            }
            catch (Exception ex)
            {
                Logger.Error("save purchase learner email message", null, ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        #endregion

        #region discussion notifications
        private string NotificationDto2Html(NotificationMessageDTO dto)
        {
            var html = GetTemplateHtml(EmailEnums.eTemplateKinds.USER_NOT);

            if (string.IsNullOrEmpty(html)) return string.Empty;

            html = html.Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), dto.FullName.String2Html(null, NAME_HTML_STYLE, null))
                        .Replace(EmailEnums.eTemplateFields.POST_AUTHOR.ToString().Field2Template(), dto.PosterName.String2Html(USER_PROFILE_URL_TEMPLATE, NAME_HTML_STYLE_UNDERLINE, dto.PosterId))
                        .Replace(EmailEnums.eTemplateFields.POST_DATE.ToString().Field2Template(), dto.AddOn.DateFormat())
                        .Replace(EmailEnums.eTemplateFields.HTML_BODY.ToString().Field2Template(), dto.MessageHTML);

            return html;
        }
        #endregion

        #region review
        private string ReviewDto2AuthorMessageHtml(ReviewMessageDTO dto)
        {
            var html = GetTemplateHtml(EmailEnums.eTemplateKinds.REVIEW_AUTHOR);

            if (string.IsNullOrEmpty(html)) return string.Empty;

             html = html.Replace(EmailEnums.eTemplateFields.COURSE_AUTHOR.ToString().Field2Template(), dto.Author.name.String2Html(null, NAME_HTML_STYLE, null))
                        .Replace(EmailEnums.eTemplateFields.REVIEW_AUTHOR.ToString().Field2Template(), dto.Writer.name.String2Html(USER_PROFILE_URL_TEMPLATE, NAME_HTML_STYLE_UNDERLINE, dto.Writer.id))
                        .Replace(EmailEnums.eTemplateFields.COURSE_NAME.ToString().Field2Template(), dto.Item.name)
                        .Replace(EmailEnums.eTemplateFields.REVIEW_DATE.ToString().Field2Template(), dto.AddOn.DateFormat())
                        .Replace(EmailEnums.eTemplateFields.HTML_BODY.ToString().Field2Template(), dto.ReviewText);

            return html;
        }

        private string ReviewDto2LearnerMessageHtml(ReviewMessageDTO dto)
        {
            var html = GetTemplateHtml(EmailEnums.eTemplateKinds.REVIEW_LEARNER);

            if (string.IsNullOrEmpty(html)) return string.Empty;

            html = html.Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), dto.Learner.name.String2Html(null, NAME_HTML_STYLE, null))
                       .Replace(EmailEnums.eTemplateFields.COURSE_AUTHOR.ToString().Field2Template(), dto.Author.name.String2Html(USER_PROFILE_URL_TEMPLATE, NAME_HTML_STYLE, dto.Author.id))
                       .Replace(EmailEnums.eTemplateFields.REVIEW_AUTHOR.ToString().Field2Template(), dto.Writer.name.String2Html(USER_PROFILE_URL_TEMPLATE, NAME_HTML_STYLE_UNDERLINE, dto.Writer.id))
                       .Replace(EmailEnums.eTemplateFields.COURSE_NAME.ToString().Field2Template(), dto.Item.name)
                       .Replace(EmailEnums.eTemplateFields.REVIEW_DATE.ToString().Field2Template(), dto.AddOn.DateFormat())
                       .Replace(EmailEnums.eTemplateFields.HTML_BODY.ToString().Field2Template(), dto.ReviewText);

            return html;
        }
        #endregion

        #endregion

        #region interface implementation

        #region purchase
        public void SavePurchaseMessages(EmailPurchaseToken token, bool send)
        {
            try
            {
                SavePurchaseAuthor(token,send);
                SavePurchaseLearner(token,send);
            }
            catch (Exception ex)
            {
                Logger.Error("save email message", null, ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        #endregion

        #region registration
        public void SaveRegistrationActivationMessage(EmailRegistrationToken token, out long emailId)
        {
            try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.REGISTRATION_ACTIVATION);

                var message = new EmailMessageDTO
                {
                    Subject      = REGISTRATION_ACTIVATION_SUBJECT
                    ,MessageFrom = FROM
                    ,UserId      = token.UserID
                    ,ToEmail     = token.ToEmail
                    ,ToName      = token.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.ACTIVATION_LINK.ToString().Field2Template(), "<a style=\"" + NAME_HTML_STYLE_UNDERLINE + "\"  href=\"" + token.ActivationURL + "\" >ACTIVATE MY ACCOUNT</a>")
                                      .Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), token.FullName.String2Html(null, NAME_HTML_STYLE, null))
                };

                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                emailId = -1;
                Logger.Error("save registration email message", null, ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }

        public void SaveRegistrationMessage(EmailRegistrationToken token, out long emailId)
        {
            try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.REGISTRATION);

                var message = new EmailMessageDTO
                {
                    Subject      = REGISTRATION_SUBJECT
                    ,MessageFrom = FROM
                    ,UserId      = token.UserID
                    ,ToEmail     = token.ToEmail
                    ,ToName      = token.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), token.FullName.String2Html(null, NAME_HTML_STYLE, null))
                };

                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                emailId = -1;
                Logger.Error("save registration email message", null, ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        #endregion

        #region reset password
        public void SaveResetPasswordMessage(EmailForgottenPasswordToken token, out long emailId,out string error)
        {
            error = string.Empty;
            try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.FORGOTTEN_PASSWORD);

                var message = new EmailMessageDTO
                {
                    Subject      = FORGOTTEN_PASSWORD_SUBJECT
                    ,MessageFrom = FROM
                    ,UserId      = token.UserID
                    ,ToEmail     = token.ToEmail
                    ,ToName      = token.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.RESET_PASSWORD_LINK.ToString().Field2Template(), "<a style=\"" + NAME_HTML_BODY_STYLE_UNDERLINE + "\"  href=\"" + token.ResetPasswordURL + "\" >" + "Please follow this Link" + "</a>")
                                       .Replace(EmailEnums.eTemplateFields.BASE_SITE_URL.ToString().Field2Template(), "<a style=\"" + NAME_HTML_STYLE_UNDERLINE + "\"  href=\"" + Utils.GetKeyValue("baseUrl") + "\" >" + Utils.GetKeyValue("baseUrl") + "</a>")
                                       .Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), token.FullName)
                };

                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                emailId = -1;
                error = Utils.FormatError(ex);
                Logger.Error("save forgotten password email message", null, ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        #endregion

        #region discussion notifications
        public List<NotificationMessageDTO> GetMessageNotifications(long messageId)
        {
            return EmailMessageRepository.GetMessageNotifications(messageId).Select(x => x.Entity2NotificationMessageDto()).ToList();
        }       

        public bool SaveEmailNotificationRecord(NotificationMessageDTO token)
        {
            try
            {
                var html = NotificationDto2Html(token);

                if (String.IsNullOrEmpty(html)) return false;

                var entity = token.NotificationDto2EmailMessage(FROM, DISCUSSION_SUBJECT, html);

                EmailMessageRepository.Add(entity);

                EmailMessageRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Save email record", token.UserId, ex, CommonEnums.LoggerObjectTypes.Email);
                return false;
            }
        } 
        #endregion

        #region review
        public bool SaveEmailReviewAuthorRecord(ReviewMessageDTO token)
        {
            try
            {
                var html = ReviewDto2AuthorMessageHtml(token);

                if (String.IsNullOrEmpty(html)) return false;

                var entity = token.Author.ReviewDto2EmailMessage(FROM, REVIEW_AUTHOR_SUBJECT, html);

                EmailMessageRepository.Add(entity);

                EmailMessageRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Save review author email record", token.Author.id, ex, CommonEnums.LoggerObjectTypes.Email);
                return false;
            }
        }
        
        public bool SaveEmailReviewLearnerRecord(ReviewMessageDTO token)
        {
            try
            {
                var html = ReviewDto2LearnerMessageHtml(token);

                if (String.IsNullOrEmpty(html)) return false;

                var entity = token.Learner.ReviewDto2EmailMessage(FROM, REVIEW_LEARNER_SUBJECT, html);

                EmailMessageRepository.Add(entity);

                EmailMessageRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Save review author email record", token.Author.id, ex, CommonEnums.LoggerObjectTypes.Email);
                return false;
            }
        }       
        #endregion

        #region author monthly statement
        public void SaveMonthlyStatementMessage(MonthlyStatementDTO token,string messageBody,out long emailId,out string error)
        {
            error = string.Empty;
            try
            {
                var message = new EmailMessageDTO
                {
                    Subject      = $"Monthly Statement for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(token.Month)} {token.Year}"
                    ,MessageFrom = FROM
                    ,UserId      = token.User.UserId
                    ,ToEmail     = token.User.Email
                    ,ToName      = token.User.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = messageBody
                };

                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                emailId = -1;
                Logger.Error("save monthly statement email message", token.User.UserId, ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        #endregion

        #region subscription cancel
        public void SaveSubscriptionCancelMessage(EmailSubscriptionCancelToken token, out long emailId)
        {
             try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.AUTHOR_CANCELLATION_ALERT);

                var message = new EmailMessageDTO
                {
                    Subject = String.Format(AUHTOR_SUBSCRIPTION_CANCEL_ALERT_SUBJECT, token.CourseName)
                    ,MessageFrom = FROM
                    ,UserId      = token.UserID
                    ,ToEmail     = token.ToEmail
                    ,ToName      = token.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.COURSE_AUTHOR.ToString().Field2Template(), token.FullName)
                                        .Replace(EmailEnums.eTemplateFields.COURSE_NAME.ToString().Field2Template(), token.CourseName)
                                        .Replace(EmailEnums.eTemplateFields.LEARNER_NAME.ToString().Field2Template(), token.LearnerName)
                                        .Replace(EmailEnums.eTemplateFields.LEARNER_EMAIL.ToString().Field2Template(), token.LearnerEmail)
                                        .Replace(EmailEnums.eTemplateFields.FOLLOWING_MONTH.ToString().Field2Template(), token.Month)

                };
                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                emailId = -1;
                Logger.Error("email service::save SaveSubscriptionCancelMessage",  ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        #endregion

        #region refund program
        public void SaveJoinRefundProgramMessage(EmailBaseToken token, out long emailId)
        {
             try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.REFUND_PROGRAM_CONFIRMATION);

                var message = new EmailMessageDTO
                {
                    Subject      = REFUND_PROGRAM_JOIN_SUBJECT
                    ,MessageFrom = FROM
                    ,UserId      = token.UserID
                    ,ToEmail     = token.ToEmail
                    ,ToName      = token.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), token.FullName)

                };
                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                emailId = -1;
                Logger.Error("email service::save join refund program email message",  ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        public void SaveGRPRefundSubmitted(EmailGRPSubmitted token, out long emailId)
        {
            try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.GRP_REFUND_SUBMITTED);
                html = html.Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), token.FullName);
                html = html.Replace(EmailEnums.eTemplateFields.LEARNER_NAME.ToString().Field2Template(), token.LearnerName);
                html = html.Replace(EmailEnums.eTemplateFields.LEARNER_EMAIL.ToString().Field2Template(), token.LearnerEmail);
                html = html.Replace(EmailEnums.eTemplateFields.FREE_TEXT.ToString().Field2Template(), token.ReasonText);
                html = html.Replace(EmailEnums.eTemplateFields.COURSE_NAME.ToString().Field2Template(), token.ItemName);

                var message = new EmailMessageDTO
                {
                    Subject      = GRP_REFUND_SUBMITTED_SUBJECT
                    ,MessageFrom = FROM
                    ,UserId      = token.UserID
                    ,ToEmail     = token.ToEmail
                    ,ToName      = token.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html

                };
                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                emailId = -1;
                Logger.Error("email service::save join refund program email message",  ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        #endregion

        #region certificate
        public void SaveStudentCertificateMessage(StudentCertificateDTO token, string messageBody, out long emailId, out string error)
        {
            error = string.Empty;
            try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.STUDENT_CERTIFICATE);

                var message = new EmailMessageDTO
                {
                    Subject = String.Format(STUDENT_CERTIFICATE_SUBJECT, token.CourseName)
                    ,MessageFrom = FROM
                    ,UserId      = CurrentUserId
                    ,ToEmail     = token.StudentInfo.Email
                    ,ToName      = token.StudentInfo.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now                   
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.LEARNER_NAME.ToString().Field2Template(), token.StudentInfo.FullName)
                                        .Replace(EmailEnums.eTemplateFields.FULL_NAME.ToString().Field2Template(), token.PresentedBy)
                                        .Replace(EmailEnums.eTemplateFields.COURSE_NAME.ToString().Field2Template(), token.CourseName)
                                        .Replace(EmailEnums.eTemplateFields.ACTIVATION_LINK.ToString().Field2Template(), "<a style=\"" + NAME_HTML_STYLE_UNDERLINE + "\"  href=\"" + token.OnlineCertificateUrl + "\" >Online certificate link</a>")
                };
                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                emailId = -1;
                Logger.Error("email service::SaveStudentCertificateMessage email message", ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }
        #endregion

        #region Quizzes
        public void SaveQuizContactAuthorMessage(StudentMessageDTO token, out long emailId, out string error)
        {
            error = string.Empty;
            try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.QUIZ_AUTHOR_REQUEST);

                var message = new EmailMessageDTO
                {
                    Subject = String.Format(QUIZ_CONTACT_AUTHOR_SUBJECT, token.CourseName)
                    ,MessageFrom = FROM
                    ,UserId      = CurrentUserId
                    ,ToEmail     = token.Author.Email
                    ,ToName      = token.Author.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.LEARNER_NAME.ToString().Field2Template(), token.Student.FullName)
                                       .Replace(EmailEnums.eTemplateFields.LEARNER_EMAIL.ToString().Field2Template(), "<a style=\"" + NAME_HTML_STYLE_UNDERLINE + "\"  href=\"mailto:" + token.Student.Email + "\" >" + token.Student.Email + "</a>")
                                       .Replace(EmailEnums.eTemplateFields.QUIZ_NAME.ToString().Field2Template(), token.QuizTitle)
                                       .Replace(EmailEnums.eTemplateFields.FREE_TEXT.ToString().Field2Template(), token.Message)
                };
                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                emailId = -1;
                Logger.Error("email service::SaveQuizContactAuthorMessage email message", ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }

        public void SaveQuizAuthorResponseMessage(StudentMessageDTO token, out long emailId, out string error)
        {
           error = string.Empty;
            try
            {
                var html = GetTemplateHtml(EmailEnums.eTemplateKinds.QUIZ_AUTHOR_RESPONSE);

                var message = new EmailMessageDTO
                {
                    Subject = String.Format(QUIZ_AUTHOR_RESPONSE_SUBJECT, token.QuizTitle)
                    ,MessageFrom = FROM
                    ,UserId      = CurrentUserId
                    ,ToEmail     = token.Student.Email
                    ,ToName      = token.Student.FullName
                    ,Status      = EmailEnums.eSendInterfaceStatus.Waiting
                    ,AddOn       = DateTime.Now
                    ,MessageBody = html.Replace(EmailEnums.eTemplateFields.LEARNER_NAME.ToString().Field2Template(), token.Student.FullName)
                                       .Replace(EmailEnums.eTemplateFields.COURSE_AUTHOR.ToString().Field2Template(), token.Author.FullName)
                                       .Replace(EmailEnums.eTemplateFields.QUIZ_NAME.ToString().Field2Template(), token.QuizTitle)
                                       .Replace(EmailEnums.eTemplateFields.QUIZ_ATTEMPT_NO.ToString().Field2Template(), token.AvailableAttempts.ToString())
                };
                
                SaveMessage(message,out emailId);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                emailId = -1;
                Logger.Error("email service::SaveQuizAuthorResponseMessage email message", ex, CommonEnums.LoggerObjectTypes.Email);
            }
        }

        #endregion
        #endregion

        #endregion

        #region IPortalAdminEmailServices implementation
        public EmailTemplateDTO GetTemplateDTO(short kindId)
        {
            var entity = EmailTemplateRepository.Get(x => x.TemplateKindId == kindId);
            return entity == null ? new EmailTemplateDTO { Kind = Utils.ParseEnum<EmailEnums.eTemplateKinds>(kindId) } : entity.Entity2EmailTemplateDto();
        }

        public bool SaveTemplate(EmailTemplateDTO token, int userId, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = EmailTemplateRepository.Get(x => x.TemplateKindId == (short)token.Kind);

                if (entity == null)
                {
                    EmailTemplateRepository.Add(token.Token2EmailTemplateEntity(userId));
                }
                else
                {
                    entity.Snippet = token.Snipet;
                    entity.UpdateOn = DateTime.Now;
                    entity.UpdatedBy = userId;
                }

                EmailTemplateRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("SaveMessageHtml", (short)token.Kind, ex, CommonEnums.LoggerObjectTypes.Email);
                return false;
            }
        }
        #endregion
        
    }
}
