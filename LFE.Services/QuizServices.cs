using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;

namespace LFE.Application.Services
{
    public class QuizServices : ServiceBase, IQuizAdminServices,  IQuizWidgetServices
    {
        private readonly IEmailServices _emailServices;
        private readonly IAmazonEmailWrapper _amazonEmailWrapper;
        public QuizServices()
        {
            _amazonEmailWrapper = DependencyResolver.Current.GetService<IAmazonEmailWrapper>();
            _emailServices      = DependencyResolver.Current.GetService<IEmailServices>();
        }

        #region private 
        //private CourseQuizDTO Entity2Token(QZ_CourseQuizzes entity)
        //{
        //    return entity == null ? "Not found".MessageToCourseQuizErrorToken() : entity.Entity2CourseQuizDto(CourseAvailableQuizzes(entity.CourseId));
        //}
        private bool UpdateQuizStatus(Guid quizId, QuizEnums.eQuizStatuses status, out string error)
        {
            try
            {
                var entity = CourseQuizzesRepository.GetById(quizId);

                if (entity == null)
                {
                    error = "quiz entity not found";
                    return false;
                }

                entity.UpdateQuizStatus(status);

                return CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("UpdateQuizStatus", quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }
        private void UpdateUserQuizAttempt(Guid attemptId, QuizEnums.eUserQuizStatuses? status, int? currentIndex, out string error)
        {
            try
            {
                var entity = StudentQuizAttemptsRepository.GetById(attemptId);

                if (entity == null)
                {
                    error = "attempt entity not found";
                    return;
                }

                entity.UpdateStudentAttempt(status,currentIndex);

                StudentQuizAttemptsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
               
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("UpdateUserQuizStatus", attemptId, ex, CommonEnums.LoggerObjectTypes.Quiz);              
            }
        }

        private bool GetStudentQuizId(Guid quizId, int userId, byte? attempts, out Guid userQuizId,out string error)
        {
            error = string.Empty;
            userQuizId = Guid.Empty;

            try
            {
                
                var entity = StudentQuizzesRepository.Get(x => x.QuizId == quizId && x.UserId == userId);

                if (entity == null)
                {
                    userQuizId = Guid.NewGuid();

                    entity = quizId.ToStudentQuizEntity(userId,userQuizId, attempts);

                    StudentQuizzesRepository.Add(entity);

                    return StudentQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
                }

                //if (entity.AvailableAttempts != null && entity.AvailableAttempts == 0)
                //{
                //    error = "no more attempts";
                //    return false;                    
                //}

                userQuizId = entity.StudentQuizId;

                return true;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("UserQuizId::" + userId, quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        private void UpdateStudentQuiz(Guid studentQuizId,DateTime start)
        {
            try
            {
                var entity = StudentQuizzesRepository.GetById(studentQuizId);

                if (entity == null)
                {
                    return;
                }

                entity.UpdateStudentQuiz(start);

                StudentQuizzesRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateQuizStatus", studentQuizId, ex, CommonEnums.LoggerObjectTypes.Quiz);                
            }
        }

        private void SetQuizValidationToken(QuizDTO token)
        {
            token.ValidationToken = GetQuizValidationToken(token.QuizId);
        }
        #endregion

        #region IQuizAdminServices implementation
        #region Quizzes
        #region reports
        public List<QuizListDTO> GetUserQuizzes(int? userId)
        {
            try
            {
                return QuizViewRepository.GetMany(x =>userId == null || x.AuthorUserId == userId).Select(x => x.Entity2QuizListDto()).OrderByDescending(x => x.Sid).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserQuizzes", userId ?? -1, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<QuizListDTO>();
            }
        }

        public List<StudentQuizDTO> GetQuizStudents(Guid quizId)
        {
            try
            {
              return StudentQuizzesRepository.GetQuizStudents(quizId).Select(x=>x.Entity2StudentQuizDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Get Quiz Students",quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<StudentQuizDTO>();
            }
        }

        public StudentQuizDTO GetStudentQuiz(Guid id)
        {
            try
            {
                return StudentQuizzesRepository.GetStudentQuizInfo(id).Entity2StudentQuizDto();
            }
            catch (Exception ex)
            {
                Logger.Error("Get Student Quiz", id, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new StudentQuizDTO{IsValid = false,Message = FormatError(ex)};
            }
        }

        public List<StudentQuizAttemptDTO> GetStudentQuizAttempts(Guid studentQuizId)
        {
            try
            {
              return StudentQuizAttemptsRepository.GetMany(x=>x.StudentQuizId == studentQuizId).Select(x=>x.Entity2StudentQuizAttemptDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Get Quiz Students attempts",studentQuizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<StudentQuizAttemptDTO>();
            }
        }

        public List<StudentAnswerToken> GetStudentAttemptAnswers(Guid attemptId)
        {
            try
            {
                return StudentQuizAnswersRepository.GetMany(x => x.AttemptId == attemptId).Select(x => x.Entity2StudentAnswerToken()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Get Quiz Students attempt answers", attemptId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<StudentAnswerToken>();
            }
        }

        public bool UpdateStudentAvailableAttempts(Guid studentQuizId, byte? attempts, out string error)
        {
            try
            {
                var entity = StudentQuizzesRepository.GetById(studentQuizId);

                if (entity == null)
                {
                    error = "Student quiz entity not found";
                    return false;
                }

                entity.UpdateStudentQuizAvailableAttempts(attempts);

                if(!StudentQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                //send email to learner
                long emailId;

                var token = new StudentMessageDTO{AvailableAttempts = attempts};
                
                var qentity = StudentQuizzesRepository.GetStudentQuizInfo(entity.QuizId, entity.UserId);

                token.UpdateStudentMessageDto(qentity);

                _emailServices.SaveQuizAuthorResponseMessage(token, out emailId, out error);

                if (emailId < 0 || !_amazonEmailWrapper.SendEmail(emailId, out error)) return false;

                entity.UpdateStudentQuizResponseDate();

                return StudentQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);

            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("UpdateStudentAvailableAttempts", studentQuizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;

            }
        }

        #endregion

        public QuizBaseDTO GetQuizBaseToken(int courseId, Guid? quizId)
        {
            try
            {
                return quizId == null ? new QuizDTO(courseId, _HasCertificate(courseId)) : CourseQuizzesRepository.GetById((Guid)quizId).Entity2QuizBaseDto(courseId);
            }
            catch (Exception ex)
            {
                Logger.Error("get quiz base token", quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new QuizBaseDTO
                {
                     IsValid = false
                    ,Message = FormatError(ex)
                };
            }
        }

        public List<QuizListDTO> GetCourseQuizzesList(int courseId)
        {
            try
            {
                return QuizViewRepository.GetMany(x => x.CourseId == courseId).Select(x => x.Entity2QuizListDto()).OrderByDescending(x => x.Sid).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetCourseQuizzes", courseId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<QuizListDTO>();
            }
        }

        public List<QuizDTO> GetCourseQuizzes(int courseId)
        {
            try
            {
                return QuizViewRepository.GetMany(x => x.CourseId == courseId).Select(x => x.Entity2QuizDto(_HasCertificate(x.CourseId))).OrderByDescending(x => x.Sid).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserQuizzes", courseId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<QuizDTO>();
            }
        }
        
        public List<SelectListItem> CourseAvailableQuizzes(int courseId)
        {
            try
            {
                return QuizViewRepository.GetMany(x => x.CourseId == courseId && !x.IsAttached && x.StatusId == (byte)QuizEnums.eQuizStatuses.PUBLISHED && x.IsValid != null && (bool)x.IsValid)
                        .Select(x => new SelectListItem
                        {
                            Value = x.QuizId.ToString()
                            ,Text = x.Title
                        })
                        .OrderBy(x=>x.Text)
                        .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("CourseAvailableQuizzes", courseId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<SelectListItem>();
            }
        }

        public bool IsOtherQuizHasCertificate(Guid quizId, int courseId, out string error)
        {
            error = string.Empty;
            try
            {
                return CourseQuizzesRepository.IsAny(x => x.CourseId == courseId && x.QuizId != quizId && x.AttachCertificate);
            }
            catch (Exception ex)
            {
                Logger.Error("IsOtherQuizHasCertificate", quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                error = FormatError(ex);
                return false;
            }
        }

        //
        
        public List<SelectListItem> GetUserValidPublishedQuizzes(int courseId)
        {
            try
            {
                return QuizViewRepository.GetMany(x => x.CourseId == courseId && x.StatusId == (byte)QuizEnums.eQuizStatuses.PUBLISHED && !x.IsAttached && (x.IsValid ?? true)).Select(x => x.Entity2SelectListItem()).OrderByDescending(x => x.Text).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserValidPublishedQuizzes", courseId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<SelectListItem>();
            }
        }
        public int TotalUserValidPublishedQuizzes(int courseId)
        {
            try
            {
                return QuizViewRepository.Count(x => x.CourseId == courseId && x.StatusId == (byte)QuizEnums.eQuizStatuses.PUBLISHED && (x.IsValid ?? true));
            }
            catch (Exception ex)
            {
                Logger.Error("TotalUserQuizzes", courseId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return 0;
            }
        }

        public QuizDTO GetQuizToken(int courseId, Guid? quizId)
        {
            try
            {
                QuizDTO token;
                if (quizId == null || quizId.Equals(Guid.Empty))
                {

                    token = new QuizDTO(courseId, _HasCertificate(courseId));

                    return token;
                }
                
                var entity = QuizViewRepository.Get(x=>x.QuizId == (Guid) quizId);

                if (entity == null) return new QuizDTO(courseId, _HasCertificate(courseId));
                
                token = entity.Entity2QuizDto(_HasCertificate(courseId));

                SetQuizValidationToken(token);

                return token;
            }
            catch (Exception ex)
            {
                Logger.Error("get quiz token", quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new QuizDTO
                {
                    IsValid  = false
                    ,Message = FormatError(ex)
                };
            }
        }
     
        public QuizDTO GetQuizToken(int sid)
        {
            try
            {
                var entity =  QuizViewRepository.Get(x => x.Sid == sid);
                
                return entity!= null ? entity.Entity2QuizDto(_HasCertificate(entity.CourseId)) : new QuizDTO{IsValid = false,Message = "Quiz not found"};
            }
            catch (Exception ex)
            {
                Logger.Error("get quiz token", sid, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new QuizDTO
                {
                    IsValid  = false
                    ,Message = FormatError(ex)
                };
            }
        }
        
        public QuizValidationToken GetQuizValidationToken(Guid quizId)
        {
            var quiz = CourseQuizzesRepository.GetById(quizId).Entity2QuizBaseToken();

            var questions = QuizQuestionsViewRepository.GetMany(x => x.QuizId == quizId).ToList();

            var validationToken = new QuizValidationToken
            {
                Quiz = quiz
                ,IsValid = true
            };

            var checkList = new List<QuizPublishCheckToken>
            {
                QuizEnums.eQuizValidationRules.ANY_QUEST.ToQuizPublishCheckToken(questions.Any(),"You have not added any questions."),
                QuizEnums.eQuizValidationRules.ANY_ACTIVE.ToQuizPublishCheckToken(questions.Any(x => x.IsActive),"You have no active questions."),
                QuizEnums.eQuizValidationRules.SCORE_VALID.ToQuizPublishCheckToken(quiz.ScoreRule != QuizEnums.eScoreRules.SCORE_PER_QUESTION || questions.Count == questions.Count(x => x.Score != null), "Please allocate a score to all questions."),
                QuizEnums.eQuizValidationRules.ANSWERS_VALID.ToQuizPublishCheckToken(questions.Count == questions.Count(x => x.TotalAnswers > 1),"Please allocate answers to all questions (at least 2 answers for each question)."),
                QuizEnums.eQuizValidationRules.ANSWERS_CORRECT_VALID.ToQuizPublishCheckToken(questions.Count == questions.Count(x => x.CorrectAnswer == 1),"Please assign a correct answer to all questions.")
            };


            validationToken.CheckList = checkList;

            validationToken.IsQuizValid = checkList.Count == checkList.Count(x => x.Pass);

            if(validationToken.IsQuizValid) return validationToken;

            string error;

            if (SaveQuizStatus(new QuizBaseToken {QuizId = quizId, Status = QuizEnums.eQuizStatuses.DRAFT}, out error))
            {
                validationToken.Quiz.Status = QuizEnums.eQuizStatuses.DRAFT;                
            }
            else
            {
                validationToken.IsValid = false;
                validationToken.Message = error;
            }

            AttachQuizToContents(quizId, false, out error);

            return validationToken;
        }

        public bool SaveQuiz(QuizDTO token, out string error)
        {
            try
            {
                var entity = CourseQuizzesRepository.GetById(token.QuizId);

                if (entity == null)
                {
                    entity = token.Dto2QuizEntity();

                    CourseQuizzesRepository.Add(entity);

                    if(!CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    token.Sid = entity.Sid;
                    
                    return true; //SetQuizCertificationState(token, out error);
                }

                entity.UpdateQuizEntity(token);

                return CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);// && SetQuizCertificationState(token, out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("save quiz token", token.Sid, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        //private bool SetQuizCertificationState(QuizDTO token, out string error)
        //{
        //    error = string.Empty;
        //    try
        //    {
        //        if (!token.AttachCertificate) return true;

        //        var others = CourseQuizzesRepository.GetMany(x => x.CourseId == token.CourseId && x.QuizId != token.QuizId && x.AttachCertificate).ToList();

        //        foreach (var entity in others)
        //        {
        //            entity.UpdateQuizCertificateStatus(false);
        //        }

        //        return QuizQuestionAnswerOptionsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
        //    }
        //    catch (Exception ex)
        //    {
        //        error = FormatError(ex);
        //        Logger.Error("SetQuizCertificationState::" + token.CourseId, token.QuizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
        //        return false;
        //    }
        //}

        public bool AttachQuizToContents(Guid quizId, bool isAttched, out string error)
        {
            try
            {
                var entity = CourseQuizzesRepository.GetById(quizId);

                if (entity == null)
                {
                    error = "quiz entity not found";
                    return false;
                }

                entity.UpdateQuizAttachState(isAttched);

                return CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("AttachQuizToContents", quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        public bool SaveQuizStatus(QuizBaseToken token, out string error)
        {
            try
            {
                var entity = CourseQuizzesRepository.GetById(token.QuizId);

                if (entity == null)
                {
                    error = "quiz entity not found";
                    return false;
                }

                entity.UpdateQuizStatus(token.Status);

                return CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("save quiz status", token.QuizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        public bool DeleteQuiz(Guid quizId, out string error)
        {
            try
            {
                CourseQuizzesRepository.Delete(x=>x.QuizId == quizId);

                return CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("delete Quiz", quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            } 
        }

        #endregion

        #region Questions
        public List<QuizQuestionDTO> GetQuizQuestion(Guid quizId)
        {
            try
            {
                return QuizQuestionsRepository.GetMany(x => x.QuizId == quizId).Select(x => x.Entity2QuizQuestionDto()).OrderBy(x => x.Index).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Get Quiz question", quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<QuizQuestionDTO>();
            }
        }
        public QuizQuestionDTO GetQuestionToken(Guid quizId, int? questId)
        {
            try
            {
                var quizEntity = CourseQuizzesRepository.GetById(quizId);

                if(quizEntity == null) return new QuizQuestionDTO{IsValid = false,Message = "Quiz entity not found"};

                if (questId == null || questId < 0) return new QuizQuestionDTO(quizId)
                {
                    ScoreRequired = quizEntity.ScoreRuleId == (byte)QuizEnums.eScoreRules.SCORE_PER_QUESTION
                };
                
                var entity = QuizQuestionsRepository.GetById((int) questId);

                var token = entity.Entity2QuizQuestionDto();

                token.ScoreRequired = quizEntity.ScoreRuleId == (byte) QuizEnums.eScoreRules.SCORE_PER_QUESTION;

                if (token.BcIdentifier == null) return token;

                try
                {
                    token.PromoVideo = _GetVideoToken((long)token.BcIdentifier, entity.QZ_CourseQuizzes.Courses.AuthorUserId);
                    //var videoToken = _brightcoveWrapper.FindVideoById((long)token.BcIdentifier);

                    //if (videoToken != null) token.PromoVideo = videoToken.BrightcoveVideo2VideoDTO(entity.QZ_CourseQuizzes.Courses.AuthorUserId, entity.QZ_CourseQuizzes.Courses.AuthorUserId.UserId2Tag(), _GetVideoChapterUsage((long)token.BcIdentifier));
                }
                catch (Exception ex)
                {
                    Logger.Error("get question video", (int)questId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                }

                return token;
               
            }
            catch (Exception ex)
            {
                Logger.Error("get question token", questId ?? -1, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new QuizQuestionDTO
                {
                    IsValid = false
                    ,Message = FormatError(ex)
                };
            }
        }

        public QuizQuestionDTO GetQuestionToken(int questId)
        {
            try
            {
                
                var entity = QuizQuestionsRepository.GetById(questId);

                return entity.Entity2QuizQuestionDto();

            }
            catch (Exception ex)
            {
                Logger.Error("get question token", questId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new QuizQuestionDTO
                {
                    IsValid  = false
                    ,Message = FormatError(ex)
                };
            }
        }

        public bool SaveQuestion(QuizQuestionDTO token, out string error)
        {
            try
            {
                var entity = QuizQuestionsRepository.GetById(token.QuestionId);

                if (entity == null)
                {
                    entity = token.Dto2QuizQuestionsEntity();

                    entity.OrderIndex = (short)QuizQuestionsRepository.Count(x => x.QuizId == token.QuizId);

                    QuizQuestionsRepository.Add(entity);

                    if (!CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    token.QuestionId = entity.QuestionId;

                    return true;
                }

                entity.UpdateQuizQuestionEntity(token);

                return CourseQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("save question token for quiz::" + token.QuizId, token.QuestionId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        public bool DeleteQuestion(int qid, out string error)
        {
            try
            {
                var entity = QuizQuestionsRepository.GetById(qid);

                var quizId = entity.QuizId;

                QuizQuestionsRepository.Delete(entity);

                if (!QuizQuestionsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                var cnt = QuizQuestionsRepository.Count(x => x.QuizId == quizId);

                return cnt > 0 || UpdateQuizStatus(quizId, QuizEnums.eQuizStatuses.DRAFT, out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("delete Question", qid, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            } 
        }
        public bool SaveQuestionOrder(int[] questionIds, out string error)
        {
            try
            {
                var i = 0;
                foreach (var questionId in questionIds)
                {
                    var entity = QuizQuestionsRepository.GetById(questionId);

                    if (entity == null) continue;

                    entity.UpdateQuizQuestionOrderIndex((short)i);

                    i++;
                }

                return QuizQuestionsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save question order", null, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }
        #endregion

        #region Answers
        public List<QuizAnswerOptionDTO> GetQuestionAnswers(int questionId)
        {
            try
            {
                return QuizQuestionAnswerOptionsRepository.GetMany(x => x.QuestionId == questionId).Select(x => x.Entity2AnswerOptionDto()).OrderBy(x => x.Index).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Get question answers", questionId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<QuizAnswerOptionDTO>();
            }
        }

        public bool SaveAnswer(QuizAnswerOptionDTO token, out string error)
        {
            try
            {
                var entity = QuizQuestionAnswerOptionsRepository.GetById(token.OptionId);

                if (entity == null)
                {
                    entity = token.Dto2QuestionAnswerOptionEntity();

                    entity.OrderIndex = (short)QuizQuestionAnswerOptionsRepository.Count(x => x.QuestionId == token.QuestionId);

                    QuizQuestionAnswerOptionsRepository.Add(entity);

                    if (!QuizQuestionAnswerOptionsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    token.OptionId = entity.OptionId;

                    return SetAnswerCorrectOption(token, out error);
                }

                entity.UpdateQuizAnswerOptionEntity(token);

                return QuizQuestionAnswerOptionsRepository.UnitOfWork.CommitAndRefreshChanges(out error) && SetAnswerCorrectOption(token, out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("save answer token for question::" + token.QuestionId, token.OptionId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        public bool UpdateAnswerCorrectOption(int optionId, bool isCorrect, out string error)
        {
             try
            {
                var entity = QuizQuestionAnswerOptionsRepository.GetById(optionId);

                if (entity == null)
                {
                    error = "Answer option entity not found by Id " + optionId;
                    return false;
                }

                entity.UpdateAnswerOptionsCorrectProp(isCorrect);

                return QuizQuestionAnswerOptionsRepository.UnitOfWork.CommitAndRefreshChanges(out error) && SetAnswerCorrectOption(new QuizAnswerOptionDTO{OptionId = optionId,QuestionId = entity.QuestionId,IsCorrect = isCorrect}, out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("UpdateAnswerCorrectOption::", optionId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        private bool SetAnswerCorrectOption(QuizAnswerOptionDTO token, out string error)
        {
            error = string.Empty;
            try
            {
                if (!token.IsCorrect) return true;

                var others = QuizQuestionAnswerOptionsRepository.GetMany(x=>x.QuestionId == token.QuestionId && x.OptionId != token.OptionId && x.IsCorrect).ToList();

                foreach (var entity in others)
                {
                    entity.UpdateAnswerOptionsCorrectProp(false);
                }

                return QuizQuestionAnswerOptionsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("SetAnswerCorrectOption::" + token.QuestionId, token.OptionId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        public bool DeleteAnswer(int optionId, out string error)
        {
            try
            {
                QuizQuestionAnswerOptionsRepository.Delete(x => x.OptionId == optionId);

                return QuizQuestionAnswerOptionsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("delete answer", optionId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            } 
        }

        public bool SaveAnswerOrder(int[] optionIds, out string error)
        {
            try
            {
                var i = 0;
                foreach (var optionId in optionIds)
                {
                    var entity = QuizQuestionAnswerOptionsRepository.GetById(optionId);

                    if (entity == null) continue;

                    entity.UpdateAnswerOptionsOrderIndex((short)i);

                    i++;
                }

                return QuizQuestionsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save answer order", null, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        #endregion
        #endregion

        #region IQuizWidgetServices
        public List<UserCourseQuizToken> GetUserCourseQuizzes(int courseId, int userId)
        {
            try
            {
                return CourseQuizzesRepository.GetUserCourseQuizzes(userId,courseId).Select(x => x.Entity2UserCourseQuizToken()).OrderBy(x => (x.AvailableAfter ?? 0)).ThenBy(x => x.Title).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Get course quizzes", courseId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new List<UserCourseQuizToken>();
            }
        }

        public UserCourseQuizToken GetUserQuiz(Guid quizId,int userId)
        {
            try
            {
               //check entity
                var quizEntity = CourseQuizzesRepository.GetById(quizId);

                if(quizEntity == null) return "Course Quiz not found. Please contact support team".MessageToUserQuizErrorToken();

                var token = quizEntity.Entity2UserCourseQuizToken();

                var state = new UserQuizStateToken();

                var attempts = StudentQuizAttemptsRepository.GetMany(x=>x.QZ_StudentQuizzes.QuizId == quizId && x.QZ_StudentQuizzes.UserId == userId && (x.StatusId != (byte)QuizEnums.eUserQuizStatuses.IN_PROGRESS)).ToList();

                if (!attempts.Any())
                {
                    state.Status            = QuizEnums.eUserQuizAviability.Available;
                    state.Reason            = QuizEnums.eUserQuizAviabilityReasons.Waiting;
                    
                    token.AvailableAttempts = quizEntity.Attempts;
                }
                else
                {
                    token.UserAttempts = attempts.Count();
                    token.Score        = attempts.Max(x => x.Score) ?? 0;
                    token.Passed       = attempts.Any(x => x.IsSuccess);

                    var studentQuizEntity = StudentQuizzesRepository.Get(x => x.QuizId == quizId && x.UserId == userId);

                    token.AvailableAttempts = studentQuizEntity != null ? studentQuizEntity.AvailableAttempts : (byte)(token.Attempts - token.UserAttempts);

                    //TODO check available attempts
                    state.Status = token.Attempts == null || token.AvailableAttempts < token.Attempts ? QuizEnums.eUserQuizAviability.Available : QuizEnums.eUserQuizAviability.NotAvailable;

                    switch (state.Status)
                    {
                        case QuizEnums.eUserQuizAviability.Available:
                            state.Reason = token.Passed ? QuizEnums.eUserQuizAviabilityReasons.Passed : QuizEnums.eUserQuizAviabilityReasons.Failed;
                            break;
                        case QuizEnums.eUserQuizAviability.NotAvailable:
                            state.Reason = QuizEnums.eUserQuizAviabilityReasons.MaxAttemptsExceed;
                            break;
                    }
                }

                token.State = state;

                return token;
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserQuiz", quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return  FormatError(ex).MessageToUserQuizErrorToken();
            }
        }

        public Guid? CurrentAttemptId(Guid quizId, int userId,out int index)
        {
            index = -1;
            try
            {
                var attempts = StudentQuizAttemptsRepository.GetMany(x => x.QZ_StudentQuizzes.QuizId == quizId && x.QZ_StudentQuizzes.UserId == userId && x.StatusId == (byte)QuizEnums.eUserQuizStatuses.IN_PROGRESS).OrderByDescending(x => x.StartOn).ToList();

                if (!attempts.Any()) return null;

                Guid? attemptId = null;
                var i = 0;
                foreach (var entity in attempts)
                {
                    var finishQuiz = true;

                    if (i == 0) //check last attempt
                    {
                        var limit = entity.QZ_StudentQuizzes.QZ_CourseQuizzes.TimeLimit;
                        if (limit == null)
                        {
                            if (DateTime.Now < entity.StartOn.AddDays(1))
                            {
                                attemptId  = entity.AttemptId;
                                index      = entity.CurrentIndex;
                                finishQuiz = false;
                            }

                        }
                        else
                        {
                            var end = entity.StartOn.AddMinutes((double)limit);

                            if (DateTime.Now < end)
                            {
                                attemptId  = entity.AttemptId;
                                index      = entity.CurrentIndex;
                                finishQuiz = false;
                            }
                        }
                    }

                    if (finishQuiz)
                    {
                        string error;
                        UpdateUserQuizAttempt(entity.AttemptId, QuizEnums.eUserQuizStatuses.UNFINISHED,null,out error);
                    }

                    i++;
                }

                return attemptId;
            }
            catch (Exception ex)
            {
                Logger.Error("CurrentAttemptId::" + userId, quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return null;
            }
        }

        public bool StartUserQuiz(Guid quizId, int userId, out Guid attemptId, out string error)
        {
            attemptId = Guid.Empty;
            error = string.Empty;
            try
            {
               
                //get quiz entity
                var quizEntity = CourseQuizzesRepository.GetById(quizId);

                if (quizEntity == null)
                {
                    error = "Quiz not found";
                    return false;
                }

                 //create student quiz
                Guid userQuizId;
                if(!GetStudentQuizId(quizId,userId,quizEntity.Attempts,out userQuizId,out error)) return false;

                //create attempt
                var attemptEntity = userQuizId.ToQuizAttemptEntity();

                StudentQuizAttemptsRepository.Add(attemptEntity);
                if (!StudentQuizAttemptsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                UpdateStudentQuiz(userQuizId,attemptEntity.StartOn);

                attemptId = attemptEntity.AttemptId;

                //fill student quiz question/answers table
                var questList = QuizQuestionsViewRepository.GetMany(x => x.QuizId == quizEntity.QuizId && x.TotalAnswers > 1 && x.CorrectAnswer == 1).Select(x => x.Entity2QuizQuestionDto()).OrderBy(x => x.Index).ToList();

                if (questList.Count.Equals(0))
                {
                    error = "No valid questions found. Please contact Author";
                    return false;
                }

                var sortedList = quizEntity.RandomOrder ? questList.Shuffle(new Random()) : questList.OrderBy(x=>x.Index).ToList();
                var scoreRule = Utils.ParseEnum<QuizEnums.eScoreRules>(quizEntity.ScoreRuleId);
                byte? questScore = null;
                if (scoreRule == QuizEnums.eScoreRules.EQUAL)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    questScore = (byte?) Math.Round((decimal) (100/questList.Count), 0);
                }
                var index = 0;
                foreach (var question in sortedList)
                {
                    if (questScore != null)
                    {
                        question.Score = questScore;
                    }

                    var answerEntity = question.QuestionDto2StudentQuizAnswerEntity(attemptId, index);

                    StudentQuizAnswersRepository.Add(answerEntity);

                    index++;
                }

                return StudentQuizAnswersRepository.UnitOfWork.CommitAndRefreshChanges(out error);

            }
            catch (Exception ex)
            {
                Logger.Error("StartUserQuiz::" + userId, quizId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        public UserQuizQuestionToken GetQuizQuestionToken(Guid attemptId, int index)
        {
            try
            {
                var entity = StudentQuizAnswersRepository.Get(x => x.AttemptId == attemptId && x.OrderIndex == index);

                if (entity == null) return new UserQuizQuestionToken { IsValid = false, Message = "Question not found"};

                var token = GetUserQuizQuestionToken(attemptId, index);

                if (!token.IsValid) return token;
                
                token.NavToken = new UserQuizQuestionNavToken
                                        {
                                             IsBackAllowed = entity.QZ_StudentQuizAttempts.QZ_StudentQuizzes.QZ_CourseQuizzes.IsBackAllowed
                                            ,TimeLimit     = entity.QZ_StudentQuizAttempts.QZ_StudentQuizzes.QZ_CourseQuizzes.TimeLimit
                                            ,TotalQuest    = StudentQuizAnswersRepository.Count(x => x.AttemptId == attemptId)
                                            ,QuizStarted   = entity.QZ_StudentQuizAttempts.StartOn
                                            ,Attempts      = entity.QZ_StudentQuizAttempts.QZ_StudentQuizzes.QZ_CourseQuizzes.Attempts
                                            ,UserAttempts  = StudentQuizAttemptsRepository.Count(x=>x.QZ_StudentQuizzes.QuizId == entity.QZ_StudentQuizAttempts.QZ_StudentQuizzes.QuizId && x.QZ_StudentQuizzes.UserId == entity.QZ_StudentQuizAttempts.QZ_StudentQuizzes.UserId && x.StatusId != (byte)QuizEnums.eUserQuizStatuses.IN_PROGRESS)
                                        };

               

                return token;
            }
            catch (Exception ex)
            {
                Logger.Error("GetQuizQuestionToken::" + attemptId, index, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new UserQuizQuestionToken{IsValid = false,Message = FormatError(ex)};
            }
        }

        public UserQuizQuestionToken GetUserQuizQuestionToken(Guid attemptId, int index)
        {
            try
            {
                 var nextEntity = StudentQuizAnswersRepository.Get(x => x.AttemptId == attemptId && x.OrderIndex == index);

                if (nextEntity == null) return new UserQuizQuestionToken { IsValid = false, Message = "Next Question not found" };

                if (nextEntity.QuestionId == null) return new UserQuizQuestionToken { IsValid = false, Message = "Question not valid. Please contact Support Team" };

                var questionEntity = QuizQuestionsRepository.GetById((int)nextEntity.QuestionId);

                if (questionEntity == null) return new UserQuizQuestionToken { IsValid = false, Message = "Question not found. Please contact Support Team" };


                var q = questionEntity.Entity2UserQuizQuestionBaseToken();
                q.AnswerId = nextEntity.AnswerId;
                q.AnswerOptions = StudentQuizAttemptsRepository.GetAttemptQuestionAnswerOptions((int)nextEntity.QuestionId, attemptId).Select(x => x.Entity2AnswerOptionToken()).ToList();

                var token = new UserQuizQuestionToken
                {
                    AttemptId      = attemptId
                    ,CurrentIndex  = nextEntity.OrderIndex
                    ,Question      = q
                    ,IsValid       = true
                };

                return token;
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserQuizQuestionToken", attemptId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new UserQuizQuestionToken { IsValid = false, Message = FormatError(ex) }; 
            }
        }

        public UserQuizQuestionToken GetQuizNextQuestionToken(int answerId)
        {
             try
            {
                var currentEntity = StudentQuizAnswersRepository.GetById(answerId);

                if (currentEntity == null) return new UserQuizQuestionToken { IsValid = false, Message = "Question not found" };

                var currentIndex  = currentEntity.OrderIndex + 1;

                var token = GetUserQuizQuestionToken(currentEntity.AttemptId, currentIndex);

                if (!token.IsValid) return token;

                string error;
                UpdateUserQuizAttempt(currentEntity.AttemptId, null, currentIndex, out error);

                return token;
            }
            catch (Exception ex)
            {
                Logger.Error("GetQuizQNextuestionToken", answerId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new UserQuizQuestionToken{IsValid = false,Message = FormatError(ex)};
            }
        }

        public bool FinishAttempt(Guid attemptId, QuizEnums.eUserQuizStatuses status, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = StudentQuizAttemptsRepository.GetById(attemptId);

                if (entity == null)
                {
                    error = "Attempt Entity not found";
                    return false;
                }

                var currentStatus = Utils.ParseEnum<QuizEnums.eUserQuizStatuses>(entity.StatusId);

                if (currentStatus != QuizEnums.eUserQuizStatuses.IN_PROGRESS) return true;

                var quizEntity = entity.QZ_StudentQuizzes.QZ_CourseQuizzes;

                if (quizEntity == null)
                {
                    error = "Quiz Entity not found";
                    return false;
                }

                var ruleId = quizEntity.ScoreRuleId;

                var rule = Utils.ParseEnum<QuizEnums.eScoreRules>(ruleId);

                var answers = StudentQuizAnswersRepository.GetMany(x=>x.AttemptId == attemptId).ToList();

                if (answers.Count > 0)
                {
                    var quizHelper = new QuizHelper();

                    var score = quizHelper.CalculateScore(answers, rule);

                    var passed = quizEntity.PassPercent <= Math.Ceiling(score);

                    entity.UpdateStudentAttempt(score,passed,status);

                    if (!StudentQuizAttemptsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    var studentQuizId = entity.StudentQuizId;

                    var studentQuizEntity = StudentQuizzesRepository.GetById(studentQuizId);

                    if (studentQuizEntity == null)
                    {
                        error = "Student Quiz Entity not found";
                        return false;
                    }

                    studentQuizEntity.UpdateStudentQuiz(passed,score);

                    return StudentQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
                }
                
                error = "No answers found";
                return false;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("CalculateAttemptScore", attemptId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }

        public bool SaveAnswer(int answerId, int optionId, bool calculateScore, out string error)
        {
            try
            {
                var entity = StudentQuizAnswersRepository.GetById(answerId);

                if (entity == null)
                {
                    error = "Answer Entity not found";
                    return false;
                }

                var option = QuizQuestionAnswerOptionsRepository.GetById(optionId);

                if (option == null)
                {
                    error = "Answer Option Entity not found";
                    return false;
                }

                entity.SaveUserAnswer(option);

                return StudentQuizAnswersRepository.UnitOfWork.CommitAndRefreshChanges(out error) && (!calculateScore || FinishAttempt(entity.AttemptId,QuizEnums.eUserQuizStatuses.COMPLETED, out error));
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("SaveAnswer", answerId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return false;
            }
        }      

        public StudentQuizAttemptDTO GetAttemptDto(Guid attemptId)
        {
            try
            {
                return StudentQuizAttemptsRepository.GetStudentAttempt(attemptId).Entity2StudentQuizAttemptDto();
            }
            catch (Exception ex)
            {
                Logger.Error("GetAttemptDto", attemptId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new StudentQuizAttemptDTO{IsValid = false,Message = FormatError(ex)};
            }
        }

        public StudentQuizAttemptDTO GetUserBestAttempt(Guid quizId, int userId)
        {
            try
            {
                return StudentQuizAttemptsRepository.GetStudentBestAttempt(quizId,userId).Entity2StudentQuizAttemptDto();
            }
            catch (Exception ex)
            {
                Logger.Error("Get best Attempt for quiz " + quizId, userId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new StudentQuizAttemptDTO { IsValid = false, Message = FormatError(ex) };
            }
        }

        public StudentQuizDTO GetContactAuthorToken(Guid quizId, int userId)
        {
            try
            {
              return StudentQuizzesRepository.GetStudentQuizInfo(quizId,userId).Entity2ContactQuizAuthorToken();
            }
            catch (Exception ex)
            {
                Logger.Error("GetContactAuthorToken for quiz " + quizId, userId, ex, CommonEnums.LoggerObjectTypes.Quiz);
                return new StudentQuizDTO { IsValid = false, Message = FormatError(ex) };
            }
        }

        public bool SendMessageToAuthor(StudentMessageDTO token, int userId, out string error)
        {

            var entity = StudentQuizzesRepository.GetStudentQuizInfo(token.QuizId, userId);

            if (entity == null)
            {
                error = "Student Quiz not found";
                return false;
            }

            if (entity.UserId != userId)
            {
                error = "Unauthorized access";
                return false;
            }

            token.UpdateStudentMessageDto(entity);

            long emailId;

            _emailServices.SaveQuizContactAuthorMessage(token, out emailId,out error);

            if (emailId < 0 || !_amazonEmailWrapper.SendEmail(emailId, out error)) return false;

            var studentQuizEntity = StudentQuizzesRepository.GetById(token.StudentQuizId);

            if (studentQuizEntity != null)
            {
                studentQuizEntity.UpdateStudentQuizRequestDate();
            }

            return StudentQuizzesRepository.UnitOfWork.CommitAndRefreshChanges(out error);
        }
      
        #endregion
        
    }

    public class QuizHelper
    {
        public decimal CalculateScore(List<QZ_StudentQuizAnswers> answers, QuizEnums.eScoreRules rule)
        {
            decimal score;
            switch (rule)
            {
                case QuizEnums.eScoreRules.EQUAL:
                    var correctAnswers = answers.Count(x => x.IsCorrect);
                    var totalAnswers = answers.Count;
                    // ReSharper disable once RedundantCast                            
                    score = (decimal)correctAnswers / totalAnswers * 100;
                    break;
                case QuizEnums.eScoreRules.SCORE_PER_QUESTION:
                    var totalPoints = answers.Sum(x => x.Score);
                    var correctPoints = answers.Where(x => x.IsCorrect).Sum(x => x.Score);
                    if (totalPoints != null && totalPoints > 0 && correctPoints != null)
                    {
                        score = ((decimal)correctPoints / (decimal)totalPoints * 100);
                    }
                    else
                    {
                        score = 0;
                    }
                    break;
                default:                 
                    return 0;
            }

            return score;
        }
    }

    public class QuizOfflineServices
    {
        public bool FinishAttempt(Guid attemptId, QuizEnums.eUserQuizStatuses status, out string error)
        {
            error = string.Empty;
            try
            {
                using (var context = new lfeAuthorEntities())
                {

                    var entity = context.QZ_StudentQuizAttempts.FirstOrDefault(x=>x.AttemptId == attemptId);

                    if (entity == null)
                    {
                        error = "Attempt Entity not found";
                        return false;
                    }

                    var currentStatus = Utils.ParseEnum<QuizEnums.eUserQuizStatuses>(entity.StatusId);

                    if (currentStatus != QuizEnums.eUserQuizStatuses.IN_PROGRESS) return true;

                    var quizEntity = entity.QZ_StudentQuizzes.QZ_CourseQuizzes;
                    if (quizEntity == null)
                    {
                        error = "Quiz Entity not found";
                        return false;
                    }

                    var ruleId = quizEntity.ScoreRuleId;

                    var rule = Utils.ParseEnum<QuizEnums.eScoreRules>(ruleId);

                    var answers = context.QZ_StudentQuizAnswers.Where(x => x.AttemptId == attemptId).ToList();

                    if (answers.Count > 0)
                    {
                        var quizHelper = new QuizHelper();

                        var score = quizHelper.CalculateScore(answers,rule);
                        //switch (rule)
                        //{
                        //    case QuizEnums.eScoreRules.EQUAL:
                        //        var correctAnswers = answers.Count(x => x.IsCorrect);
                        //        var totalAnswers = answers.Count;
                        //        // ReSharper disable once RedundantCast                            
                        //        score = (decimal) correctAnswers/totalAnswers*100;
                        //        break;
                        //    case QuizEnums.eScoreRules.SCORE_PER_QUESTION:
                        //        var totalPoints = answers.Sum(x => x.Score);
                        //        var correctPoints = answers.Where(x => x.IsCorrect).Sum(x => x.Score);
                        //        if (totalPoints != null && totalPoints > 0 && correctPoints != null)
                        //        {
                        //            score = ((decimal) correctPoints/(decimal) totalPoints*100);
                        //        }
                        //        else
                        //        {
                        //            score = 0;
                        //        }
                        //        break;
                        //    default:
                        //        error = "Unknown score rule";
                        //        return false;
                        //}

                        var passed = quizEntity.PassPercent <= Math.Ceiling(score);

                        entity.UpdateStudentAttempt(score, passed, status);

                        context.SaveChanges();

                        var studentQuizId = entity.StudentQuizId;

                        var studentQuizEntity = context.QZ_StudentQuizzes.FirstOrDefault(x=>x.StudentQuizId == studentQuizId);

                        if (studentQuizEntity == null)
                        {
                            error = "Student Quiz Entity not found";
                            return false;
                        }

                        studentQuizEntity.UpdateStudentQuiz(passed, score);

                        context.SaveChanges();

                        return true;
                    }

                    error = "No answers found";
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;                
                return false;
            }
        }
    }
}
