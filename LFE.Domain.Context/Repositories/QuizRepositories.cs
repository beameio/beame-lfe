using System;
using System.Collections.Generic;
using System.Linq;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    
    public class QuizViewRepository : Repository<vw_QZ_CourseQuizzes>, IQuizViewRepository
    {
        public QuizViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class QuizQuestionsRepository : Repository<QZ_QuizQuestionsLib>, IQuizQuestionsRepository
    {
        public QuizQuestionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class QuizQuestionsViewRepository : Repository<vw_QZ_QuizQuestionsLib>, IQuizQuestionsViewRepository
    {
        public QuizQuestionsViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
    
    public class QuizQuestionAnswerOptionsRepository : Repository<QZ_QuestionAnswerOptions>, IQuizQuestionAnswerOptionsRepository
    {
        public QuizQuestionAnswerOptionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class CourseQuizzesRepository : Repository<QZ_CourseQuizzes>, ICourseQuizzesRepository
    {
        public CourseQuizzesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public List<QZ_CourseQuizToken> GetCourseQuizzes(int courseId)
        {
            return DataContext.tvf_QZ_GetCourseQuizzes(courseId).ToList();
        }

        
        public List<QZ_UserCourseQuizToken> GetUserCourseQuizzes(int userId, int courseId)
        {
            return DataContext.tvf_QZ_GetUserCourseQuizzes(userId, courseId).ToList();
        }
    }

    public class StudentQuizzesRepository : Repository<QZ_StudentQuizzes>, IStudentQuizzesRepository
    {
        public StudentQuizzesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public QZ_StudentQuizInfoToken GetStudentQuizInfo(Guid quizId, int userId)
        {
            return DataContext.tvf_QZ_GetStudentQuizInfo(quizId,null, userId).FirstOrDefault();
        }

        public QZ_StudentQuizInfoToken GetStudentQuizInfo(Guid studentQuizId)
        {
            return DataContext.tvf_QZ_GetStudentQuizInfo(null, studentQuizId, null).FirstOrDefault();
        }

        public List<QZ_StudentQuizInfoToken> GetQuizStudents(Guid quizId)
        {
            return DataContext.tvf_QZ_GetStudentQuizInfo(quizId,null, null).ToList();
        }
    }

    public class StudentQuizAttemptsRepository : Repository<QZ_StudentQuizAttempts>, IStudentQuizAttemptsRepository
    {
        public StudentQuizAttemptsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public List<QZ_AttemptAnswerOptionToken> GetAttemptQuestionAnswerOptions(int questionId, Guid attemptId)
        {
            return DataContext.tvf_QZ_GetAttemptQuestionAnswerOptions(questionId, attemptId).ToList();
        }

        public QZ_StudentAttemptToken GetStudentAttempt(Guid attemptId)
        {
            return DataContext.tvf_QZ_GetStudentAttempt(attemptId).FirstOrDefault();
        }

        public QZ_StudentAttemptToken GetStudentBestAttempt(Guid quizId, int userId)
        {
            return DataContext.tvf_QZ_GetStudentBestAttempt(quizId,userId).FirstOrDefault();
        }
    }

    public class StudentQuizAnswersRepository : Repository<QZ_StudentQuizAnswers>, IStudentQuizAnswersRepository
    {
        public StudentQuizAnswersRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    
}
