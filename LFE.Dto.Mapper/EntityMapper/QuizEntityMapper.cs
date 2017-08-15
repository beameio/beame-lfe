using System;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class QuizEntityMapper
    {

        public static QZ_CourseQuizzes Dto2QuizEntity(this QuizDTO token)
        {
            return new QZ_CourseQuizzes
            {
                QuizId               = token.QuizId
                ,Title               = token.Title
                ,CourseId            = token.CourseId
                ,Description         = token.Description
                ,Instructions        = token.Instructions
                ,RandomOrder         = token.RandomOrder
                ,PassPercent         = token.PassPercent
                ,StatusId            = (byte)token.Status
                ,ScoreRuleId         = (byte)token.ScoreRule
                ,AvailableAfter      = token.AvailableAfter
                ,Attempts            = token.Attempts
                ,TimeLimit           = token.TimeLimit
                ,IsMandatory         = token.IsMandatory
                ,IsBackAllowed       = token.IsBackAllowed
                ,AttachCertificate   = token.AttachCertificate
                ,AddOn               = DateTime.Now
                ,CreatedBy           = DtoExtensions.CurrentUserId
            };
        }
        public static void UpdateQuizCertificateStatus(this QZ_CourseQuizzes entity, bool isAttached)
        {
            entity.AttachCertificate = isAttached;
            entity.UpdateDate        = DateTime.Now;
            entity.UpdatedBy         = DtoExtensions.CurrentUserId;
        }
        public static void UpdateQuizStatus(this QZ_CourseQuizzes entity, QuizEnums.eQuizStatuses status)
        {
            entity.StatusId   = (byte) status;
            entity.IsAttached = status == QuizEnums.eQuizStatuses.PUBLISHED;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
        public static void UpdateQuizAttachState(this QZ_CourseQuizzes entity, bool isAttched)
        {
            entity.IsAttached = isAttched;
            entity.StatusId   = (byte) (isAttched ? QuizEnums.eQuizStatuses.PUBLISHED : QuizEnums.eQuizStatuses.DRAFT);
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
        public static void UpdateQuizCertAttachState(this QZ_CourseQuizzes entity, bool isAttched)
        {
            entity.AttachCertificate = isAttched;
            entity.UpdateDate        = DateTime.Now;
            entity.UpdatedBy         = DtoExtensions.CurrentUserId;
        }
        public static void UpdateQuizEntity(this QZ_CourseQuizzes entity, QuizDTO token)
        {
            entity.Title               = token.Title;            
            entity.Description         = token.Description;
            entity.Instructions        = token.Instructions;
            entity.RandomOrder         = token.RandomOrder;
            entity.PassPercent         = token.PassPercent;
            entity.ScoreRuleId         = (byte)token.ScoreRule;
            entity.AvailableAfter      = token.AvailableAfter;
            entity.Attempts            = token.Attempts;
            entity.TimeLimit           = token.TimeLimit;
            entity.IsMandatory         = token.IsMandatory;
            entity.IsBackAllowed       = token.IsBackAllowed;
            entity.AttachCertificate   = token.AttachCertificate;
            entity.UpdateDate          = DateTime.Now;
            entity.UpdatedBy           = DtoExtensions.CurrentUserId;
        }

        public static QZ_QuizQuestionsLib Dto2QuizQuestionsEntity(this QuizQuestionDTO token)
        {
            return new QZ_QuizQuestionsLib
            {
                QuizId               = token.QuizId
                ,QuestionText        = token.QuestionText
                ,TypeId              = (byte)token.Type
                ,Description         = token.Description
                ,Score               = token.Score
                ,OrderIndex          = token.Index
                ,BcIdentifier        = token.BcIdentifier
                ,ImageUrl            = token.ImageUrl
                ,IsActive            = token.IsActive
                ,AddOn               = DateTime.Now
                ,CreatedBy           = DtoExtensions.CurrentUserId
            };
        }

        public static void UpdateQuizQuestionEntity(this QZ_QuizQuestionsLib entity, QuizQuestionDTO token)
        {
            entity.QuestionText = token.QuestionText;
            entity.TypeId       = (byte)token.Type;
            entity.Description  = token.Description;
            entity.Score        = token.Score;
            entity.BcIdentifier = token.BcIdentifier;
            entity.ImageUrl     = token.ImageUrl;
            entity.IsActive     = token.IsActive;
            entity.UpdateDate   = DateTime.Now;
            entity.UpdatedBy    = DtoExtensions.CurrentUserId;
        }
        public static void UpdateQuizQuestionOrderIndex(this QZ_QuizQuestionsLib entity, short index)
        {
            entity.OrderIndex = index;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
        public static QZ_QuestionAnswerOptions Dto2QuestionAnswerOptionEntity(this QuizAnswerOptionDTO token)
        {
            return new QZ_QuestionAnswerOptions
            {
                QuestionId           = token.QuestionId
                ,OptionText          = token.OptionText
                ,IsCorrect           = token.IsCorrect
                ,Score               = token.Score
                ,OrderIndex          = token.Index
                ,IsActive            = true//token.IsActive
                ,AddOn               = DateTime.Now
                ,CreatedBy           = DtoExtensions.CurrentUserId
            };
        }

        public static void UpdateQuizAnswerOptionEntity(this QZ_QuestionAnswerOptions entity,QuizAnswerOptionDTO token)
        {
            entity.OptionText = token.OptionText;
            entity.IsCorrect  = token.IsCorrect;
            entity.Score      = token.Score;
            //entity.IsActive = token.IsActive;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
        public static void UpdateAnswerOptionsCorrectProp(this QZ_QuestionAnswerOptions entity, bool isCorrect)
        {
            entity.IsCorrect  = isCorrect;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
        public static void UpdateAnswerOptionsOrderIndex(this QZ_QuestionAnswerOptions entity, short index)
        {
            entity.OrderIndex = index;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }

        public static void UpdateCourseQuizAvailability(this QZ_CourseQuizzes entity, short num)
        {
            entity.AvailableAfter = num;
            entity.UpdateDate     = DateTime.Now;
            entity.UpdatedBy      = DtoExtensions.CurrentUserId;
        }

        // user quizzes
        public static QZ_StudentQuizzes ToStudentQuizEntity(this Guid quizId, int userId,Guid studentQuizId,byte? availableAttempts)
        {
            return new QZ_StudentQuizzes
            {
                StudentQuizId      = studentQuizId
                ,QuizId            = quizId
                ,UserId            = userId
                ,AvailableAttempts = availableAttempts
                ,AddOn             = DateTime.Now
                ,CreatedBy         = DtoExtensions.CurrentUserId
                ,IsSuccess         = false
            };
        }

        public static void UpdateStudentQuiz(this QZ_StudentQuizzes entity, DateTime startDate)
        {
            entity.LastAttemptStartDate = startDate;

            if (entity.AvailableAttempts != null) entity.AvailableAttempts = (byte?)Math.Max(0, (decimal)(entity.AvailableAttempts - 1));

            entity.UpdateDate           = DateTime.Now;
            entity.UpdatedBy            = DtoExtensions.CurrentUserId;
        }

        public static void UpdateStudentQuiz(this QZ_StudentQuizzes entity, bool isSuccess,decimal score)
        {
            var s = Math.Max(score, entity.Score ?? score);

            entity.Score                    = s;
            if (isSuccess) entity.IsSuccess = true;
            entity.UpdateDate               = DateTime.Now;
            entity.UpdatedBy                = DtoExtensions.CurrentUserId;
        }

        public static void UpdateStudentQuizRequestDate(this QZ_StudentQuizzes entity)
        {
            entity.RequestSendOn  = DateTime.Now;
            entity.ResponseSendOn = null;
            entity.UpdateDate     = DateTime.Now;
            entity.UpdatedBy      = DtoExtensions.CurrentUserId;
        }
        public static void UpdateStudentQuizAvailableAttempts(this QZ_StudentQuizzes entity,byte? attempts)
        {
            entity.AvailableAttempts = attempts;
            entity.UpdateDate        = DateTime.Now;
            entity.UpdatedBy         = DtoExtensions.CurrentUserId;
        }

        public static void UpdateStudentQuizResponseDate(this QZ_StudentQuizzes entity)
        {
            entity.ResponseSendOn = DateTime.Now;
            entity.UpdateDate     = DateTime.Now;
            entity.UpdatedBy      = DtoExtensions.CurrentUserId;
        }

        public static QZ_StudentQuizAttempts ToQuizAttemptEntity(this Guid studentQuizId)
        {
            return new QZ_StudentQuizAttempts
            {
                AttemptId           = Guid.NewGuid()
                ,StudentQuizId      = studentQuizId
                ,StartOn            = DateTime.Now
                ,CurrentIndex       = 0
                ,StatusId           = (byte)QuizEnums.eUserQuizStatuses.IN_PROGRESS
                ,AddOn              = DateTime.Now
                ,CreatedBy          = DtoExtensions.CurrentUserId
                ,IsSuccess          = false
            };
        }
        public static void UpdateStudentAttempt(this QZ_StudentQuizAttempts entity, QuizEnums.eUserQuizStatuses? status,int? currentIndex)
        {
            if (status != null) entity.StatusId   = (byte)status;
            if (currentIndex != null) entity.CurrentIndex = (int)currentIndex;

            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }       
        public static void UpdateStudentAttempt(this QZ_StudentQuizAttempts entity, decimal score, bool isSuccess,QuizEnums.eUserQuizStatuses status)
        {
            entity.Score      = score;
            entity.IsSuccess  = isSuccess;
            entity.StatusId   = (byte)status;
            entity.FinishedOn = DateTime.Now;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }

        public static QZ_StudentQuizAnswers QuestionDto2StudentQuizAnswerEntity(this QuizQuestionDTO token,Guid attemptId,int index)
        {
            return new QZ_StudentQuizAnswers
            {
                AttemptId     = attemptId
                ,QuestionId   = token.QuestionId
                ,QuestionText = token.QuestionText
                ,OrderIndex   = index
                ,Score        = token.Score
                ,IsCorrect    = false
                ,AddOn        = DateTime.Now

            };
        }

        public static void SaveUserAnswer(this QZ_StudentQuizAnswers entity, QZ_QuestionAnswerOptions option)
        {
            entity.AnswerText = option.OptionText;
            entity.OptionId   = option.OptionId;
            entity.IsCorrect  = option.IsCorrect;
            entity.UpdateDate = DateTime.Now;
        }
    }
}
