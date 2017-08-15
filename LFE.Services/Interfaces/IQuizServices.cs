using System;
using System.Collections.Generic;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IQuizAdminServices
    {
        //reports
        List<QuizListDTO> GetUserQuizzes(int? userId);
        List<StudentQuizDTO> GetQuizStudents(Guid quizId);
        StudentQuizDTO GetStudentQuiz(Guid id);

        List<StudentQuizAttemptDTO> GetStudentQuizAttempts(Guid studentQuizId);
        List<StudentAnswerToken> GetStudentAttemptAnswers(Guid attemptId);

        bool UpdateStudentAvailableAttempts(Guid studentQuizId, byte? attempts, out string error);  
        //quizzes
        List<QuizListDTO> GetCourseQuizzesList(int courseId);
        List<QuizDTO> GetCourseQuizzes(int courseId);
        QuizBaseDTO GetQuizBaseToken(int courseId,Guid? quizId);
        bool AttachQuizToContents(Guid quizId,bool isAttched, out string error);      
        QuizDTO GetQuizToken(int courseId, Guid? quizId);
        QuizDTO GetQuizToken(int sid);
        List<SelectListItem> CourseAvailableQuizzes(int courseId);
        bool IsOtherQuizHasCertificate(Guid quizId, int courseId,out string error);
        //
        List<SelectListItem> GetUserValidPublishedQuizzes(int courseId);

        int TotalUserValidPublishedQuizzes(int courseId);
        
        QuizValidationToken GetQuizValidationToken(Guid quizId);
        
        bool SaveQuiz(QuizDTO token,out string error);
        bool SaveQuizStatus(QuizBaseToken token, out string error);
        bool DeleteQuiz(Guid quizId, out string error);

        //questions
        List<QuizQuestionDTO> GetQuizQuestion(Guid quizId);
        QuizQuestionDTO GetQuestionToken(Guid quizId,int? questId);
        QuizQuestionDTO GetQuestionToken(int questId);
        bool SaveQuestion(QuizQuestionDTO token, out string error);
        bool DeleteQuestion(int qid, out string error);
        bool SaveQuestionOrder(int[] questionIds, out string error);

        //answers
        List<QuizAnswerOptionDTO> GetQuestionAnswers(int questionId);
        bool SaveAnswer(QuizAnswerOptionDTO token, out string error);
        bool UpdateAnswerCorrectOption(int optionId,bool isCorrect, out string error);
        bool DeleteAnswer(int optionId, out string error);
        bool SaveAnswerOrder(int[] optionIds, out string error);
    }


    public interface IQuizWidgetServices
    {
        List<UserCourseQuizToken> GetUserCourseQuizzes(int courseId, int userId);

        UserCourseQuizToken GetUserQuiz(Guid quizId,int userId);
        
        Guid? CurrentAttemptId(Guid quizId, int userId, out int index);
        
        bool StartUserQuiz(Guid quizId, int userId, out Guid attemptId, out string error);
        
        UserQuizQuestionToken GetUserQuizQuestionToken(Guid attemptId, int index);
        UserQuizQuestionToken GetQuizQuestionToken(Guid attemptId, int index);
        UserQuizQuestionToken GetQuizNextQuestionToken(int answerId);
        bool SaveAnswer(int answerId, int optionId,bool calculateScore, out string error);
        bool FinishAttempt(Guid attemptId, QuizEnums.eUserQuizStatuses status, out string error);
        StudentQuizAttemptDTO GetAttemptDto(Guid attemptId);
        StudentQuizAttemptDTO GetUserBestAttempt(Guid quizId,int userId);

        StudentQuizDTO GetContactAuthorToken(Guid quizId, int userId);

        bool SendMessageToAuthor(StudentMessageDTO token, int userId, out string error);

    }
}
