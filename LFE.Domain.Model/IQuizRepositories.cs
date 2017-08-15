using System;
using System.Collections.Generic;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    
    public interface IQuizViewRepository : IGetRepository<vw_QZ_CourseQuizzes>
    {
    }
    public interface IQuizQuestionsRepository : IRepository<QZ_QuizQuestionsLib>
    {
    }
    public interface IQuizQuestionsViewRepository : IGetRepository<vw_QZ_QuizQuestionsLib>
    {
    }
    public interface IQuizQuestionAnswerOptionsRepository : IRepository<QZ_QuestionAnswerOptions>
    {
    }

    public interface ICourseQuizzesRepository : IRepository<QZ_CourseQuizzes>
    {
        List<QZ_CourseQuizToken> GetCourseQuizzes(int courseId);

        List<QZ_UserCourseQuizToken> GetUserCourseQuizzes(int userId, int courseId);
    }

    public interface IStudentQuizzesRepository : IRepository<QZ_StudentQuizzes>
    {
        QZ_StudentQuizInfoToken GetStudentQuizInfo(Guid quizId, int userId);
        QZ_StudentQuizInfoToken GetStudentQuizInfo(Guid studentQuizId);
        List<QZ_StudentQuizInfoToken> GetQuizStudents(Guid quizId);
    }
    public interface IStudentQuizAttemptsRepository : IRepository<QZ_StudentQuizAttempts>
    {
        List<QZ_AttemptAnswerOptionToken> GetAttemptQuestionAnswerOptions(int questionId, Guid attemptId);

        QZ_StudentAttemptToken GetStudentAttempt(Guid attemptId);
        QZ_StudentAttemptToken GetStudentBestAttempt(Guid quizId,int userId);
    }

    public interface IStudentQuizAnswersRepository : IRepository<QZ_StudentQuizAnswers>
    {
    }

   
}
