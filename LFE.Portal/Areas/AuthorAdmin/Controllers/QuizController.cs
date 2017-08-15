using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    public class QuizController : BaseController
    {
        private readonly IQuizAdminServices _quizAdminServices;
        public QuizController()
        {
            _quizAdminServices        = DependencyResolver.Current.GetService<IQuizAdminServices>();
        }

        #region views

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }

        public ActionResult _QuizManageForm(Guid? id,int cid)
        {
            var token = _quizAdminServices.GetQuizBaseToken(cid,id);

            return View("Index", token);
        }

        public ActionResult _ResponseToStudent(Guid id)
        {
            return PartialView("Quiz/_ResponseToStudent", new QuizBaseToken{QuizId = id});
        }

        public ActionResult _StudentAttemptsWnd(Guid id)
        {
            var token = _quizAdminServices.GetStudentQuiz(id);

            return PartialView("Quiz/_StudentAttemptsWnd", token);
        }

        public ActionResult _QuizEditForm(Guid? id,int cid)
        {
            var token = _quizAdminServices.GetQuizToken(cid,id);

            return PartialView("Quiz/_QuizEditForm", token);
        }

        public ActionResult _QuizReport()
        {
            return PartialView("Quiz/_QuizReport");
        }       

        public ActionResult _QuizQuestions(Guid id, int cid)
        {
            var token = _quizAdminServices.GetQuizToken(cid,id);

            return PartialView("Quiz/_QuizQuestions", token);
        }

        public ActionResult _EditQuestion(int? id,Guid qid,int sid)
        {
            var token = _quizAdminServices.GetQuestionToken(qid, id);
            token.QuizSid = sid;
            return PartialView("Quiz/_QuestionEditForm",token);
        }

        public ActionResult LoadQuizPublishForm(Guid id)
        {
            var token = _quizAdminServices.GetQuizValidationToken(id);

            return PartialView("Quiz/_QuizPublishForm", token);
        }

        public ActionResult _AnswerManageForm(int id )
        {
            var token = _quizAdminServices.GetQuestionToken(id);
            return PartialView("Quiz/_AnswerManageForm", token);
        }

        
        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCourseQuizzes([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _quizAdminServices.GetCourseQuizzesList(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserQuizzes([DataSourceRequest] DataSourceRequest request)
        {
            var list = _quizAdminServices.GetUserQuizzes(CurrentUserId).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetQuizStudents([DataSourceRequest] DataSourceRequest request,Guid id)
        {
            var list = _quizAdminServices.GetQuizStudents(id).OrderByDescending(x=>x.LastAttemptDate).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetQuizOpenRequests([DataSourceRequest] DataSourceRequest request, Guid id)
        {
            var list = _quizAdminServices.GetQuizStudents(id).Where(x => x.RequestSendOn != null && x.ResponseSendOn == null).OrderBy(x => x.RequestSendOn).ToList();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStudentsQuizAttempts([DataSourceRequest] DataSourceRequest request, Guid id)
        {
            var list = _quizAdminServices.GetStudentQuizAttempts(id).OrderByDescending(x => x.StartOn).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStudentsAttemptAnswers([DataSourceRequest] DataSourceRequest request, Guid id)
        {
            var list = _quizAdminServices.GetStudentAttemptAnswers(id).OrderBy(x => x.AnswerId).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetQuizQuestionsList([DataSourceRequest] DataSourceRequest request, Guid id)
        {
            var list = _quizAdminServices.GetQuizQuestion(id).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ReadAnswers([DataSourceRequest] DataSourceRequest request, int id)
        {
            var answers = _quizAdminServices.GetQuestionAnswers(id);
            return Json(answers.ToDataSourceResult(request),JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetAuthorQuizzesLOV(int id)
        {
            var list = _quizAdminServices.GetUserValidPublishedQuizzes(id).ToArray();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region posts
        [HttpPost]
        public ActionResult SaveQuiz(QuizDTO token)
        {
            string error;
            var isNew = token.Sid < 0;
            var saved = _quizAdminServices.SaveQuiz(token, out error);
            
            if (isNew && saved) SaveUserEvent(CommonEnums.eUserEvents.QUIZ_CREATED);

            return Json(new JsonResponseToken {success = saved,result = new{isNew}, error = error}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckCertificate(Guid quizId, int courseId)
        {
            string error;
            var attached = _quizAdminServices.IsOtherQuizHasCertificate(quizId, courseId,out error);

            return Json(new JsonResponseToken {success = String.IsNullOrEmpty(error),error = error,result = attached}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveQuizStatus(QuizBaseToken token)
        {
            string error;
            
            var result = _quizAdminServices.SaveQuizStatus(token, out error);

            return Json(new JsonResponseToken { success = result, error = error }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveQuestion(QuizQuestionDTO token)
        {
            string error;
            var isNew = token.QuestionId < 0;
            var result = _quizAdminServices.SaveQuestion(token, out error);

            return Json(new JsonResponseToken { success = result, result = new { isNew,id=token.QuestionId }, error = error }, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult DestroyQuiz([DataSourceRequest] DataSourceRequest request, QuizListDTO quiz)
        {
            string error;
            var deleted = _quizAdminServices.DeleteQuiz(quiz.QuizId, out error);

            return Json(deleted ? new[] {quiz}.ToDataSourceResult(request, ModelState) : new[] { new QuizListDTO() }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteQuestion(QuizQuestionDTO token)
         {
             string error;
            _quizAdminServices.DeleteQuestion(token.QuestionId, out error);

            return Json(ModelState.ToDataSourceResult());
        }

        [HttpPost]
        public JsonResult SaveQuestionOrder(int[] idS)
        {
            string error;
            var result = _quizAdminServices.SaveQuestionOrder(idS,out error);
            return Json(new JsonResponseToken
                {
                    success = result
                    ,error = error
                }, JsonRequestBehavior.AllowGet);
        }

        #region answers
        [HttpPost]
        public ActionResult SaveAnswer(QuizAnswerOptionDTO token)
        {
            string error;
            var saved = _quizAdminServices.SaveAnswer(token, out error);
            return Json(new JsonResponseToken { success = saved, error = error, result = token.QuestionId });
        }

        [HttpPost]
        public ActionResult UpdateAnswerCorrectOption(int id, bool isCorrect)
        {
            string error;
            var saved = _quizAdminServices.UpdateAnswerCorrectOption(id, isCorrect, out error);
            return Json(new JsonResponseToken { success = saved, error = error });
        }

        [HttpPost]
        public ActionResult DeleteAnswer(int id)
        {
            string error;
            var saved = _quizAdminServices.DeleteAnswer(id, out error);
            return Json(new JsonResponseToken
            {
                success = saved
                ,
                error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveAnswerOrder(int[] idS)
        {
            string error;
            var result = _quizAdminServices.SaveAnswerOrder(idS, out error);
            return Json(new JsonResponseToken
            {
                success = result
                ,
                error = error
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpPost]
        public ActionResult AttchQuiz2Contents(CourseContentToken token)
        {
            string error;
            
            var saved = _quizAdminServices.AttachQuizToContents(token.Quiz.QuizId,true, out error);

            return Json(new JsonResponseToken { success = saved, error = error});
        }

        //[HttpPost]
        //public ActionResult SaveCourseQuiz2(CourseQuizDTO token)
        //{
        //    string error;
        //    var saved = _courseQuizzesServices.SaveCourseQuiz(token, out error);
        //    return Json(new JsonResponseToken { success = saved, error = error, result = token.CourseQuizId });
        //}

        public virtual JsonResult DestroyCourseQuiz([DataSourceRequest] DataSourceRequest request, QuizListDTO quiz)
        {
            string error;
            var deleted = _quizAdminServices.DeleteQuiz(quiz.QuizId, out error);

            return Json(deleted ? new[] { quiz }.ToDataSourceResult(request, ModelState) : new[] { new QuizListDTO() }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ResetStudentAttempts([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<StudentQuizDTO> tokens)
        {
            var studentQuizDtos = tokens as StudentQuizDTO[] ?? tokens.ToArray();
            if (tokens == null || !ModelState.IsValid) return Json(studentQuizDtos.ToDataSourceResult(request, ModelState));

            foreach (var token in studentQuizDtos.ToList())
            {
                string error;
                _quizAdminServices.UpdateStudentAvailableAttempts(token.StudentQuizId,token.AvailableAttempts,out error);  
            }

            return Json(studentQuizDtos.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ResetSingleStudentAttempts(Guid id,byte num)
        {
            
            string error;
            var result = _quizAdminServices.UpdateStudentAvailableAttempts(id, num, out error);
            
            return Json(new JsonResponseToken{success = result,error = error},JsonRequestBehavior.AllowGet);
        }
        #endregion
        
    }
}
