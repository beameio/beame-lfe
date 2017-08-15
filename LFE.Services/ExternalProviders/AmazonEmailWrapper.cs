using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Model;


namespace LFE.Application.Services.ExternalProviders
{
    public class AmazonEmailWrapper : ServiceBase, IAmazonEmailWrapper
    {
        private static readonly string FROM      = Utils.GetKeyValue("AWSEmailFrom");//"hi@beame.io"; //
        private static readonly string AccessKey = Utils.GetKeyValue("AWSAccessKey");
        private static readonly string AppSecret = Utils.GetKeyValue("AWSSecretKey");
        
        private static Dictionary<EmailEnums.eTemplateKinds, string> _templateDictionary = new Dictionary<EmailEnums.eTemplateKinds, string>();
        private static AmazonSimpleEmailServiceClient _s3MailClinet;
        public AmazonEmailWrapper()
        {
           if(_templateDictionary.Count.Equals(0)) _templateDictionary = GetTemplateDictionary();
            _s3MailClinet = new AmazonSimpleEmailServiceClient(AccessKey, AppSecret, RegionEndpoint.USEast1);
        }

        #region private helpers

        private static void _setSecurityProtocol()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
        }
        private Dictionary<EmailEnums.eTemplateKinds, string> GetTemplateDictionary()
        {
            var res = new Dictionary<EmailEnums.eTemplateKinds, string>();

            foreach (var p in EmailTemplateRepository.GetAll().ToList())
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
         
            return res;
        }

        private bool SendAmazonEmail(EMAIL_Messages entity, out string error)
        {
            error = string.Empty;
            try
            {

                var TO      = entity.ToEmail;
                var SUBJECT = entity.Subject;
                var BODY    = entity.MessageBoby;

                // Construct an object to contain the recipient address.
                var destination = new Destination
                                        {
                                            ToAddresses = new List<string> { TO }
                                        };

                // Create the subject and body of the message.
                var subject = new Content(SUBJECT);
                var body = new Body
                    {
                        Html = new Content(BODY)
                    };

                // Create a message with the specified subject and body.
                var message = new Message
                {
                    Subject = subject
                    ,Body   = body
                   
                };
                
                // Assemble the email.
                var request = new SendEmailRequest
                {
                    Source = FROM
                    ,Destination = destination
                    ,Message = message
                };

            
                Logger.Debug("send email to " + entity.ToEmail);
                _setSecurityProtocol();
                // Send the email.
                var response = _s3MailClinet.SendEmail(request);

                Logger.Debug($"email to {entity.ToEmail} has been sent");

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("send email", entity.EmailId, ex, CommonEnums.LoggerObjectTypes.Email);
                return false;
            }
        }
        private bool SendEmail(EMAIL_Messages email,out string error)
        {
            error = string.Empty;
            try
            {
                
                var emailSent = SendAmazonEmail(email, out error);

                if (emailSent)
                {
                    email.Status = EmailEnums.eSendInterfaceStatus.Send.ToString();
                    email.SendOn = DateTime.Now;
                    email.UpdateOn = DateTime.Now;
                }
                else
                {
                    email.Status = EmailEnums.eSendInterfaceStatus.Failed.ToString();
                    email.Error = error;
                    email.UpdateOn = DateTime.Now;
                }

                EmailMessageRepository.UnitOfWork.CommitAndRefreshChanges();

                return emailSent;
            }
            catch (Exception ex)
            {
                Logger.Error("send email message", email.EmailId, ex, CommonEnums.LoggerObjectTypes.Email);
                return false;
            }
        }

        private static bool SendRawEmail(EMAIL_Messages email, string attachPath, out string error)
        {
            error = string.Empty;
            var htmlView = AlternateView.CreateAlternateViewFromString(email.MessageBoby, Encoding.UTF8, "text/html");
            var mailMessage = new MailMessage
            {
                From    = new MailAddress(email.MessageFrom),
                To      = { email.ToEmail},
                Subject = email.Subject,
               AlternateViews = { htmlView}
            };

            if (attachPath.Trim() != "")
            {
                if (File.Exists(attachPath))
                {
                    var objAttach = new Attachment(attachPath)
                    {
                        ContentType = new ContentType("application/octet-stream")
                    };
                    
                    var disposition = objAttach.ContentDisposition;

                    disposition.DispositionType  = "attachment";
                    disposition.CreationDate     = File.GetCreationTime(attachPath);
                    disposition.ModificationDate = File.GetLastWriteTime(attachPath);
                    disposition.ReadDate         = File.GetLastAccessTime(attachPath);

                    mailMessage.Attachments.Add(objAttach);
                }
            }

            var rawMessage = new RawMessage();
            using (var memoryStream = ConvertMailMessageToMemoryStream(mailMessage))
            {
                rawMessage.Data = memoryStream;
            }

            var request = new SendRawEmailRequest
            {
                RawMessage = rawMessage,
                Destinations = new List<string> {email.ToEmail}
                ,Source = email.MessageFrom
            };
            
            try
            {
                _setSecurityProtocol();
                var response = _s3MailClinet.SendRawEmail(request);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (AmazonSimpleEmailServiceException ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private static bool SendRawEmail(EMAIL_Messages email, MemoryStream stream, out string error)
        {
            error = string.Empty;
            var htmlView = AlternateView.CreateAlternateViewFromString(email.MessageBoby, Encoding.UTF8, "text/html");
            var mailMessage = new MailMessage
            {
                From = new MailAddress(email.MessageFrom),
                To = { email.ToEmail },
                Subject = email.Subject,
                AlternateViews = { htmlView }
            };

            if (stream.CanRead && stream.Length > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var ct = new ContentType
                {
                    MediaType = MediaTypeNames.Application.Pdf,
                    Name = "certificate.pdf"
                };

                var attach = new Attachment(stream,ct);
                mailMessage.Attachments.Add(attach);

//                var objAttach = new Attachment(stream,"certificate.pdf" ,MediaTypeNames.Application.Pdf)
//                {
//                    ContentType = new ContentType("application/octet-stream")
//                };
//
//                var disposition              = objAttach.ContentDisposition;
//                disposition.CreationDate     = DateTime.Now;
//                disposition.ModificationDate = DateTime.Now;
//                disposition.ReadDate         = DateTime.Now;
//
//                disposition.DispositionType = "attachment";
//                
//                mailMessage.Attachments.Add(objAttach);
            }

            var rawMessage = new RawMessage();
            using (var memoryStream = ConvertMailMessageToMemoryStream(mailMessage))
            {
                rawMessage.Data = memoryStream;
            }

            var request = new SendRawEmailRequest
            {
                RawMessage   = rawMessage,
                Destinations = new List<string> { email.ToEmail },
                Source       = email.MessageFrom
            };

            try
            {
                _setSecurityProtocol();
                var response = _s3MailClinet.SendRawEmail(request);
                stream.Close();
                stream.Dispose();
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (AmazonSimpleEmailServiceException ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        public static MemoryStream ConvertMailMessageToMemoryStream(MailMessage message)
        {
            var assembly             = typeof(SmtpClient).Assembly;
            var mailWriterType       = assembly.GetType("System.Net.Mail.MailWriter");
            var fileStream           = new MemoryStream();
            var mailWriterContructor = mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Stream) }, null);
            var mailWriter           = mailWriterContructor.Invoke(new object[] { fileStream });
            var sendMethod           = typeof(MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);
            
            sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { mailWriter, true,true }, null);
            
            var closeMethod = mailWriter.GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);
            
            closeMethod.Invoke(mailWriter, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { }, null);
            
            return fileStream;
        }
        #endregion

        #region interface implementation
        
        public bool SendEmail(long emailId, out string error)
        {
            var entity = EmailMessageRepository.GetById(emailId);

            if (entity != null) return SendEmail(entity,out error);

            error = "email message entity not found";

            return false;
        }

        public bool SendEmailWithAttachment(long emailId, string attachmentPath, out string error)
        {
            var entity = EmailMessageRepository.GetById(emailId);
            
            if (entity != null) return SendRawEmail(entity, attachmentPath, out error);

            error = "email message entity not found";

            return false;
        }

        public bool SendEmailWithAttachment(long emailId, MemoryStream stream, out string error)
        {
            var entity = EmailMessageRepository.GetById(emailId);

            if (entity != null) return SendRawEmail(entity, stream,out error);

            error = "email message entity not found";

            return false;
        }     
        #endregion
    }
}
