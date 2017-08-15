using System;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class QuizDtoMapper
    {
        public static QuizListDTO Entity2QuizListDto(this vw_QZ_CourseQuizzes entity)
        {
            if (entity == null) return new QuizListDTO();

            return new QuizListDTO
            {
                QuizId             = entity.QuizId
                ,Sid               = entity.Sid
                ,Title             = entity.Title
                ,CourseId          = entity.CourseId
                ,CourseName        = entity.CourseName
                ,Status            = Utils.ParseEnum<QuizEnums.eQuizStatuses>(entity.StatusId)   
                ,AddOn             = entity.AddOn
                ,Completed         = entity.TotalCompleted
                ,Attempts          = entity.Attempts
                ,PassPercent       = entity.PassPercent
                ,TimeLimit         = entity.TimeLimit
                ,AvailableAfter    = entity.AvailableAfter
                ,IsMandatory       = entity.IsMandatory
                ,Taken             = entity.TotalTaken
                ,TotalFailed       = entity.TotalFailed
                ,TotalPass         = entity.TotalPass
                ,IsQuizValid       = entity.IsValid ?? false
                ,IsAttached        = entity.IsAttached
                ,AttachCertificate = entity.AttachCertificate
                ,AvgScore          = 0
                ,OpenRequests      = entity.OpenRequests
                ,Author            = new BaseUserInfoDTO{UserId = entity.AuthorUserId,FullName = entity.Entity2AuthorFullName(),Email = entity.Email}
            };
        }
        public static SelectListItem Entity2SelectListItem(this vw_QZ_CourseQuizzes entity)
        {
            return new SelectListItem
            {
                Value             = entity.QuizId.ToString()
                ,Text = entity.Title                
            };
        }

        public static QuizBaseDTO Entity2QuizBaseDto(this QZ_CourseQuizzes entity,int courseId)
        {
            if (entity == null) return new  QuizBaseDTO
                                            {
                                                QuizId  = Guid.NewGuid(),
                                                Sid     = -1,
                                                CourseId = courseId,
                                                IsValid = true,
                                                Title   = "New quiz"
                                            };

            return new QuizBaseDTO
            {
                QuizId             = entity.QuizId
                ,Sid               = entity.Sid
                ,Title             = entity.Title
                ,CourseId          = entity.CourseId
               ,Status            = Utils.ParseEnum<QuizEnums.eQuizStatuses>(entity.StatusId)              
            };
        }

        public static QuizBaseToken Entity2QuizBaseToken(this QZ_CourseQuizzes entity)
        {
           return  new QuizBaseToken
           {
               QuizId    = entity.QuizId, 
               Status    = Utils.ParseEnum<QuizEnums.eQuizStatuses>(entity.StatusId), 
               ScoreRule = Utils.ParseEnum<QuizEnums.eScoreRules>(entity.ScoreRuleId)
           };
        }

        public static QuizPublishCheckToken ToQuizPublishCheckToken(this QuizEnums.eQuizValidationRules rule,bool isValid,string error)
        {
            var token = new QuizPublishCheckToken
            {
                Kind = rule,
                Name = Utils.GetEnumDescription(rule),
                Pass = isValid
            };

            token.Message = token.Pass ? "" : error;

            return token;
        }

        public static QuizDTO Entity2QuizDto(this vw_QZ_CourseQuizzes entity,bool certAvailable)
        {
            if(entity == null) return new QuizDTO();

            return new QuizDTO
            {
                QuizId                = entity.QuizId
                ,Sid                  = entity.Sid
                ,Title                = entity.Title
                ,CourseId             = entity.CourseId
                ,Description          = entity.Description
                ,Instructions         = entity.Instructions
                ,IsMandatory          = entity.IsMandatory
                ,IsBackAllowed        = entity.IsBackAllowed
                ,Attempts             = entity.Attempts
                ,TimeLimit            = entity.TimeLimit
                ,AvailableAfter       = entity.AvailableAfter
                ,Status               = Utils.ParseEnum<QuizEnums.eQuizStatuses>(entity.StatusId)
                ,ScoreRule            = Utils.ParseEnum<QuizEnums.eScoreRules>(entity.ScoreRuleId)
                ,PassPercent          = entity.PassPercent
                ,RandomOrder          = entity.RandomOrder
                ,TotalQuestions       = entity.TotalQustions
                ,IsQuizValid          = entity.IsValid ?? false
                ,IsAttached           = entity.IsAttached
                ,CertificateAvailable = certAvailable
                ,AttachCertificate    = entity.AttachCertificate
            };
        }

        public static QuizQuestionDTO Entity2QuizQuestionDto(this QZ_QuizQuestionsLib entity)
        {
            return new QuizQuestionDTO
            {
                QuizId        = entity.QuizId
                ,QuizSid      = entity.QZ_CourseQuizzes.Sid
                ,QuestionId   = entity.QuestionId
                ,Type         = Utils.ParseEnum<QuizEnums.eQuizQuestionTypes>(entity.TypeId)
                ,QuestionText = entity.QuestionText
                ,Score        = entity.Score
                ,BcIdentifier = entity.BcIdentifier
                ,ImageUrl     = String.IsNullOrEmpty(entity.ImageUrl) ? "" : entity.ImageUrl.CombineQuizQuestionImageUrl(entity.QZ_CourseQuizzes.Sid).ToThumbUrl(Constants.ImageBaseUrl) 
                ,Description  = entity.Description
                ,Index        = entity.OrderIndex
                ,IsActive     = entity.IsActive
            };
        }

        public static UserQuizQuestionBaseToken Entity2UserQuizQuestionBaseToken(this QZ_QuizQuestionsLib entity)
        {
            return new UserQuizQuestionBaseToken
            {
                QuestionText = entity.QuestionText
                ,BcIdentifier = entity.BcIdentifier
                ,ImageUrl     = String.IsNullOrEmpty(entity.ImageUrl) ? "" : entity.ImageUrl.CombineQuizQuestionImageUrl(entity.QZ_CourseQuizzes.Sid).ToThumbUrl(Constants.ImageBaseUrl)                 
            };
        }

        public static QuizQuestionDTO Entity2QuizQuestionDto(this vw_QZ_QuizQuestionsLib entity)
        {
            return new QuizQuestionDTO
            {
                QuizId        = entity.QuizId
                ,QuizSid      = entity.QuizSid
                ,QuestionId   = entity.QuestionId
                ,Type         = Utils.ParseEnum<QuizEnums.eQuizQuestionTypes>(entity.TypeId)
                ,QuestionText = entity.QuestionText
                ,Score        = entity.Score
                ,BcIdentifier = entity.BcIdentifier
                ,ImageUrl     = String.IsNullOrEmpty(entity.ImageUrl) ? "" : entity.ImageUrl.CombineQuizQuestionImageUrl(entity.QuizSid).ToThumbUrl(Constants.ImageBaseUrl) 
                ,Description  = entity.Description
                ,Index        = entity.OrderIndex
                ,IsActive     = entity.IsActive
            };
        }

        public static QuizAnswerOptionDTO Entity2AnswerOptionDto(this QZ_QuestionAnswerOptions entity)
        {
            return new QuizAnswerOptionDTO
            {
                 OptionId          = entity.OptionId
                ,QuestionId        = entity.QuestionId
                ,OptionText        = entity.OptionText
                ,Score             = entity.Score
                ,IsCorrect         = entity.IsCorrect
                ,Index             = entity.OrderIndex
                ,IsActive          = entity.IsActive
            };
        }
      
        public static UserCourseQuizToken Entity2UserCourseQuizToken(this QZ_UserCourseQuizToken entity)
        {

            return new UserCourseQuizToken
            {
                 QuizId         = entity.QuizId
                ,Sid            = entity.Sid
                ,Status         = Utils.ParseEnum<QuizEnums.eQuizStatuses>(entity.StatusId)
                ,Title          = entity.Title
                ,IsMandatory    = entity.IsMandatory
                ,Attempts       = entity.Attempts
                ,TimeLimit      = entity.TimeLimit
                ,AvailableAfter = entity.AvailableAfter                
                ,IsAttached     = entity.IsAttached
                ,Passed         = entity.Passed ?? false
                ,Score          = entity.Score
            };
        }

        public static UserCourseQuizToken Entity2UserCourseQuizToken(this QZ_CourseQuizzes entity)
        {
            return new UserCourseQuizToken
            {
                 QuizId         = entity.QuizId
                ,Sid            = entity.Sid
                ,Status         = Utils.ParseEnum<QuizEnums.eQuizStatuses>(entity.StatusId)
                ,Title          = entity.Title
                ,Description    = entity.Description
                ,Instructions   = entity.Instructions
                ,IsMandatory    = entity.IsMandatory                
                ,Attempts       = entity.Attempts
                ,TimeLimit      = entity.TimeLimit
                ,AvailableAfter = entity.AvailableAfter
               ,PassPercent    = entity.PassPercent
                ,IsValid        = true
            };
        }
        
        public  static QuizDTO MessageToCourseQuizErrorToken(this string error)
        {
            return new QuizDTO
            {
                IsValid = false
                ,Message = error
            };
        }

        public static UserCourseQuizToken MessageToUserQuizErrorToken(this string error)
        {
            return new UserCourseQuizToken
            {
                IsValid = false
                ,Message = error
            };
        }

        public static ContentTreeViewItemDTO Quiz2ContentTreeViewItemDto(this QuizListDTO token)
        {
            return new ContentTreeViewItemDTO
            {
                id      = token.Sid
                ,quizId = token.QuizId
                ,name   = token.Title
                ,type   = CourseEnums.eContentTreeViewItemType.quiz
            };
        }

        public static CourseContentToken ChapterToken2ContentToken(this ChapterEditDTO token)
        {
            return new CourseContentToken
                {
                    Name       = token.Name
                    ,CourseId  = token.CourseId
                    ,ContentId = token.ChapterId
                    ,Uid       = Guid.NewGuid()
                    ,Kind      = CourseEnums.eCourseContentKind.Chapter
                    ,Chapter   = token
                };
        }

         public static CourseContentToken QuizToken2ContentToken(this QuizDTO token)
        {
            return new CourseContentToken
                {
                    Name       = token.Title
                    ,CourseId  = token.CourseId
                    ,ContentId = token.Sid
                    ,Uid       = Guid.NewGuid()
                    ,Kind      = CourseEnums.eCourseContentKind.Quiz
                    ,Quiz      = token
                };
        }

        public static AnswerOptionToken Entity2AnswerOptionToken(this QZ_AttemptAnswerOptionToken entity)
        {
            return new AnswerOptionToken
            {
                OptionId   = entity.OptionId,
                OptionText = entity.OptionText,
                Selected   = entity.Selected ?? false
            };
        }
         public static StudentQuizAttemptDTO Entity2StudentQuizAttemptDto(this QZ_StudentQuizAttempts entity)
        {
            return new StudentQuizAttemptDTO
            {
                AttemptId          = entity.AttemptId
                ,IsSuccess         = entity.IsSuccess
                ,UserScore         = entity.Score
                ,StartOn           = entity.StartOn
                ,CompleteOn        = entity.FinishedOn
                ,Status            = Utils.ParseEnum<QuizEnums.eUserQuizStatuses>(entity.StatusId)
            }; 
        }
        public static StudentQuizAttemptDTO Entity2StudentQuizAttemptDto(this QZ_StudentAttemptToken entity)
        {
            return new StudentQuizAttemptDTO
            {
                AttemptId          = entity.AttemptId
                ,IsSuccess         = entity.IsSuccess
                ,UserScore         = entity.Score                
                ,AvailableAttempts = entity.AvailableAttempts
                ,UserAttempts      = entity.UserAttempts
                ,HasCertificate    = entity.AttachCertificate
                ,IsValid           = true
                ,RequestSendOn     = entity.RequestSendOn
                ,ResponseSendOn    = entity.ResponseSendOn
                ,Author            = new BaseUserInfoDTO
                                        {
                                            UserId = entity.AuthorUserId
                                            ,Email = entity.AuthorEmail
                                            ,FullName = entity.Entity2AuthorFullName()
                                        }
                ,Quiz              = new QuizDTO
                                        {
                                            QuizId             = entity.QuizId
                                            ,Title             = entity.Title
                                            ,PassPercent       = entity.PassPercent
                                            ,Attempts          = entity.Attempts
                                            ,TimeLimit         = entity.TimeLimit
                                            ,IsMandatory       = entity.IsMandatory
                                        }
            }; 
        }
        public static StudentQuizDTO Entity2StudentQuizDto(this QZ_StudentQuizInfoToken entity)
        {
            
            return new StudentQuizDTO
            {
                StudentQuizId       = entity.StudentQuizId
                ,IsSuccess          = entity.IsSuccess
                ,UserScore          = entity.Score
                ,AvailableAttempts  = entity.AvailableAttempts
                ,LastAttemptDate    = entity.LastAttemptStartDate
                ,RequestSendOn      = entity.RequestSendOn
                ,ResponseSendOn     = entity.ResponseSendOn
                ,UserAttempts       = entity.UserAttempts
                ,QuizAttempts       = entity.Attempts
                ,QuizId             = entity.QuizId
                ,Student            = new BaseUserInfoDTO
                                        {
                                            UserId = entity.UserId
                                            ,Email = entity.Email
                                            ,FullName = entity.Entity2StudentFullName()
                                        }
            };
        }

        public static StudentQuizDTO Entity2ContactQuizAuthorToken(this QZ_StudentQuizInfoToken entity)
        {
            if(entity == null) return new StudentQuizDTO{IsValid = false,Message = "Student Quiz not found"};

            return new StudentQuizDTO
            {
                StudentQuizId = entity.StudentQuizId
                ,Author = new BaseUserInfoDTO
                {
                    UserId = entity.AuthorUserId
                    ,Email = entity.Email
                }
                ,IsSuccess           = entity.IsSuccess
                ,UserScore           = entity.Score
                ,AvailableAttempts   = entity.AvailableAttempts
                ,UserAttempts        = entity.UserAttempts
                ,Quiz                = new QuizDTO
                                        {
                                            QuizId             = entity.QuizId
                                            ,Title             = entity.Title
                                            ,PassPercent       = entity.PassPercent
                                            ,Attempts          = entity.Attempts
                                            ,TimeLimit         = entity.TimeLimit
                                            ,IsMandatory       = entity.IsMandatory
                                        }
                ,IsValid = true
            };
        }

        public static void UpdateStudentMessageDto(this StudentMessageDTO token, QZ_StudentQuizInfoToken entity)
        {
            token.QuizTitle = entity.Title;

            token.Author = new BaseUserInfoDTO
            {
                UserId    = entity.AuthorUserId
                ,Email    = entity.AuthorEmail
                ,FullName = entity.Entity2AuthorFullName()
            };

            token.Student = new BaseUserInfoDTO
            {
                UserId = entity.UserId
                ,Email = entity.Email
                ,FullName = entity.Entity2StudentFullName()
            };
        }

        public static StudentAnswerToken Entity2StudentAnswerToken(this QZ_StudentQuizAnswers entity)
        {
            return new StudentAnswerToken
            {
                AttemptId  = entity.AttemptId
                ,AnswerId  = entity.AnswerId
                ,Answer    = entity.AnswerText
                ,Question  = entity.QuestionText
                ,IsCorrect = entity.IsCorrect
                ,Score     = entity.Score
            };
        }
    }
}
