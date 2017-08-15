using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.Core.Utils;

namespace LFE.DataTokens
{
    public class QuizBaseToken : BaseModelState
    {
        public Guid QuizId { get; set; }
       
        [Required]
        public string Title { get; set; }
        public QuizEnums.eQuizStatuses Status { get; set; }
        public QuizEnums.eScoreRules ScoreRule { get; set; }
    }

    public class QuizBaseDTO : QuizBaseToken
    {        
        public int Sid { get; set; }
        public int CourseId { get; set; }
        public bool IsAttached { get; set; }       
    }

    public class QuizListDTO : QuizBaseDTO
    {
        public string CourseName { get; set; }
        public DateTime AddOn { get; set; }
        public int Completed { get; set; }
        public int Taken { get; set; }
        public int TotalPass { get; set; }
        public int TotalFailed { get; set; }
        public int OpenRequests { get; set; }
        public bool IsQuizValid { get; set; }
        public decimal AvgScore { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsBackAllowed { get; set; }
        public short? AvailableAfter { get; set; }
        public short? TimeLimit { get; set; }
        public byte? PassPercent { get; set; }
        public byte? Attempts { get; set; }
        public bool AttachCertificate { get; set; }
        public BaseUserInfoDTO Author { get; set; }

        [Description("Attempts")]
        public string NumOfAttempts
        {
            get
            {
                return Attempts != null ? Attempts.ToString() : "Unlimited";
            }
        }

        [Description("Time Limit")]
        public string TimeLimitation
        {
            get
            {
                return TimeLimit != null ? String.Format("{0} minutes", TimeLimit) : "Unlimited";
            }
        }
    }

    public class QuizDTO : QuizBaseDTO
    {
        public QuizDTO() { }
        public  QuizDTO(int courseId,bool hasCertificate)
        {
            QuizId               = Guid.NewGuid();
            Sid                  = -1;
            CourseId             = courseId;
            IsValid              = true;
            Title                = "New Quiz";
            Status               = QuizEnums.eQuizStatuses.DRAFT;
            ValidationToken      = new QuizValidationToken();
            CertificateAvailable = hasCertificate;
        }
    
        [AllowHtml]
        public string Description { get; set; }
        [AllowHtml]
        public string Instructions { get; set; }

        [DisplayName("Pass score")]
        public byte? PassPercent { get; set; }
             
        public bool RandomOrder { get; set; }
      
        public int TotalQuestions { get; set; }        

        public bool IsQuizValid { get; set; }

        public QuizValidationToken ValidationToken { get; set; }
        [DisplayName("Pass score")]
        public bool IsMandatory { get; set; }
        public bool IsBackAllowed { get; set; }
        public short? AvailableAfter { get; set; }
        public short? TimeLimit { get; set; }
        public byte? Attempts { get; set; }
        public bool AttachCertificate { get; set; }
        public bool CertificateAvailable { get; set; }
    }

    public class QuizValidationToken : BaseModelState
    {
        public QuizBaseToken Quiz { get; set; }
        public bool IsQuizValid { get; set; }
        public List<QuizPublishCheckToken> CheckList { get; set; }
    }

    public class QuizPublishCheckToken
    {
        public bool Pass { get; set; }
        public QuizEnums.eQuizValidationRules Kind { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class QuizBaseQuestionDTO : BaseModelState
    {
        public int QuestionId { get; set; }
        public Guid QuizId { get; set; }
        public int QuizSid { get; set; }
        public QuizEnums.eQuizQuestionTypes Type { get; set; }

        [Required]
        [AllowHtml]
        public string QuestionText { get; set; }
        public byte? Score { get; set; }

        public short Index { get; set; }
        public bool IsActive { get; set; }

    }

    public class QuizQuestionDTO : QuizBaseQuestionDTO
    {
        public QuizQuestionDTO() { }

        public QuizQuestionDTO(Guid quizId)
        {
            QuestionId    = -1;
            QuizId        = quizId;
            AnswerOptions = new List<QuizAnswerOptionDTO>();
            IsActive      = true;
        }
        public string Description { get; set; }
   
        public long? BcIdentifier { get; set; }
        public UserVideoDto PromoVideo { get; set; }

        public string ImageUrl { get;set;}

        public bool ScoreRequired { get; set; }

        public List<QuizAnswerOptionDTO> AnswerOptions { get; set; }
        
    }

    public class QuizAnswerOptionDTO
    {
        public QuizAnswerOptionDTO()
        {
            IsActive = true;
            OptionId = -1;
        }

        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        
        [AllowHtml]
        [Required]
        public string OptionText { get; set; }

        [AllowHtml]
        public string Text { get; set; }
        
        public bool IsCorrect { get; set; }
        public byte? Score { get; set; }
        public short Index { get; set; }
        public bool IsActive { get; set; }

    }

    //user quizzes
    public class UserQuizStateToken
    {
        public QuizEnums.eUserQuizAviability Status { get; set; }
        public QuizEnums.eUserQuizAviabilityReasons Reason { get; set; }
    }

    public class UserCourseQuizToken : QuizListDTO
    {
        public bool Passed { get; set; }

        public decimal Score { get; set; }
        public int UserAttempts { get; set; }
        public byte? AvailableAttempts { get; set; }
        public UserQuizStateToken State { get; set; }

        public string Description { get; set; }
        [AllowHtml]
        public string Instructions { get; set; }
    }

    public class UserQuizQuestionToken : BaseModelState
    {
        public Guid AttemptId { get; set; }
        public int CurrentIndex { get; set; }
        
        public UserQuizQuestionNavToken NavToken { get; set; }

        public UserQuizQuestionBaseToken Question { get; set; }

       
    }

    public class UserQuizQuestionNavToken
    {
        public short? TimeLimit { get; set; }
        public bool IsBackAllowed { get; set; }
        public int TotalQuest { get; set; }
        public int UserAttempts { get; set; }
        public byte? Attempts { get; set; }
        public DateTime QuizStarted { get; set; }
    }

    public class UserQuizQuestionBaseToken : BaseModelState
    {
        public int AnswerId { get; set; }
        public string QuestionText { get; set; }
        public long? BcIdentifier { get; set; }
        public UserVideoDto PromoVideo { get; set; }
        public string ImageUrl { get; set; }
        public List<AnswerOptionToken> AnswerOptions { get; set; }
    }

    public class AnswerOptionToken
    {
        public int OptionId { get; set; }
        
        public string OptionText { get; set; }

        public bool Selected { get; set; }
    }

    public class StudentQuizAttemptDTO : BaseModelState
    {
        private decimal? _userScore;

        public Guid AttemptId { get; set; }
        public QuizDTO Quiz { get; set; }

        public bool IsSuccess { get; set; }
        public decimal? UserScore
        {
            get { return _userScore.FormatdDecimal(0); }
            set { _userScore = value; }
        }
        public byte? AvailableAttempts { get; set; }
        public int UserAttempts { get; set; }
        public bool FinishedOnTimeout { get; set; }
        public bool NotifySuccess { get; set; }
        public bool HasCertificate { get; set; }
        public bool CertificateSent { get; set; }
        public DateTime? RequestSendOn { get;set; }
        public DateTime? ResponseSendOn { get; set; }
        public BaseUserInfoDTO Author { get; set; }
        public QuizEnums.eUserQuizStatuses Status { get; set; }
        public DateTime StartOn { get; set; }
        public DateTime? CompleteOn { get; set; }
    }

    public class StudentQuizDTO :BaseModelState
    {
        private decimal? _userScore;

        public Guid StudentQuizId { get; set; }
        public Guid QuizId { get; set; }
        public BaseUserInfoDTO Author { get; set; }
        public BaseUserInfoDTO Student { get; set; }
        public QuizDTO Quiz { get; set; }

        public bool IsSuccess { get; set; }
        public decimal? UserScore
        {
            get { return _userScore.FormatdDecimal(0); }
            set { _userScore = value; }
        }
        [Range(0,100)]
        public byte? AvailableAttempts { get; set; }
        public int UserAttempts { get; set; }
        public byte? QuizAttempts { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public DateTime? RequestSendOn { get; set; }
        public DateTime? ResponseSendOn { get; set; }

        public bool HasOpenRequest
        {
            get
            {
                return RequestSendOn != null && ResponseSendOn == null;
                
            }
        }
    }

    public class StudentMessageDTO
    {
        public Guid QuizId { get; set; }
        public Guid StudentQuizId { get; set; }
        [Required]
        public string Message { get; set; }
        public string CourseName { get; set; }
        public string QuizTitle { get; set; }
        public BaseUserInfoDTO Author { get; set; }
        public BaseUserInfoDTO Student{ get; set; }

        public byte? AvailableAttempts { get; set; }
    }

    public class StudentAnswerToken
    {
        private decimal? _userScore;

        public Guid AttemptId { get; set; }
        public int AnswerId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public decimal? Score
        {
            get { return _userScore.FormatdDecimal(0); }
            set { _userScore = value; }
        }
    }
}