using LFE.Application.Services;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Portal.Areas.Widget.Models;
using System;
using System.Web.Mvc;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class QuizController : BaseController
    {
        private readonly IQuizWidgetServices _quizWidgetServices;
        private readonly IStudentSertificateCervices _studentSertificateCervices;
        private readonly IWidgetUserServices _widgetUserServices;
        public QuizController()
        {
            _quizWidgetServices         = DependencyResolver.Current.GetService<IQuizWidgetServices>();
            _studentSertificateCervices = DependencyResolver.Current.GetService<IStudentSertificateCervices>();
            _widgetUserServices         = DependencyResolver.Current.GetService<IWidgetUserServices>();
        }

        private static UserCourseQuizToken invalidUserQuizToken(string message)
        {
            return new UserCourseQuizToken {IsValid = false, Message = message};
        }
        private static UserQuizQuestionToken invalidUserQuizQuestionToken(string message)
        {
            return new UserQuizQuestionToken { IsValid = false, Message = message };
        }
        public ActionResult LoadUserQuiz(UserQuizRequestToken request)
        {
            string error;
            Guid attemptId;
            int index;
            Guid? currentAttemptId;
            UserQuizQuestionToken question;
            StudentQuizAttemptDTO attempt;

            switch (request.quizAction)
            {
                case eUserQuizActions.LoadQuiz:
                case eUserQuizActions.QuizIntro:
                    if (request.quizId != null)
                    {
                        currentAttemptId = _quizWidgetServices.CurrentAttemptId((Guid)request.quizId, CurrentUserId, out index);

                        if (currentAttemptId != null)
                        {
                            attemptId = (Guid)currentAttemptId;
                            question = _quizWidgetServices.GetQuizQuestionToken(attemptId, index);

                            return PartialView("Quiz/_UserQuizRun", question);
                        }    
                    }
                    
                    var token = request.quizId != null ? _quizWidgetServices.GetUserQuiz((Guid)request.quizId, CurrentUserId) : invalidUserQuizToken("Quiz Id required");

                    //first run
                    if(request.quizAction == eUserQuizActions.QuizIntro || (token.State.Status == QuizEnums.eUserQuizAviability.Available && token.State.Reason == QuizEnums.eUserQuizAviabilityReasons.Waiting) )
                        return PartialView("Quiz/_QuizIntro",token);

                    //load best attempt result
                    attempt = request.quizId != null ? _quizWidgetServices.GetUserBestAttempt((Guid) request.quizId, CurrentUserId) : new StudentQuizAttemptDTO{IsValid = false,Message = "QuizId required"};

                    return attempt.RequestSendOn != null && attempt.ResponseSendOn == null ?  PartialView("Quiz/_ContactAuthorResult", attempt) : PartialView("Quiz/_UserQuizComplete",attempt);
                case eUserQuizActions.StartQuiz:
                    if (request.quizId == null) return PartialView("Quiz/_UserQuizRun", invalidUserQuizQuestionToken("Quiz Id required"));
                   
                    var quizCreated = _quizWidgetServices.StartUserQuiz((Guid)request.quizId, CurrentUserId, out attemptId, out error);

                    if (!quizCreated) return PartialView("Quiz/_UserQuizRun", invalidUserQuizQuestionToken(error));
                    
                    question = _quizWidgetServices.GetQuizQuestionToken(attemptId, 0);

                    if (question.NavToken.TimeLimit != null)
                    {
                        try
                        {
                            var ms = (int)question.NavToken.TimeLimit * 60 * 1000 + 5*1000;
                            var timer = new System.Timers.Timer(ms);
                            timer.Elapsed += (sender, e) => KillAttempt(sender, e, attemptId);
                            timer.Start();
                        }
                        catch (Exception)
                        {
                            /**/
                        }
                    }

                    return PartialView("Quiz/_UserQuizRun", question);
                case eUserQuizActions.ContactAuthor:
                    var contactToken = request.quizId != null ? _quizWidgetServices.GetContactAuthorToken((Guid)request.quizId, CurrentUserId) : new StudentQuizDTO { IsValid = false, Message = "QuizId required" };
                    return PartialView("Quiz/_ContactAuthor", contactToken);
                case eUserQuizActions.ContactAuthorResult:
                    attempt = request.quizId != null ? _quizWidgetServices.GetUserBestAttempt((Guid)request.quizId, CurrentUserId) : new StudentQuizAttemptDTO { IsValid = false, Message = "QuizId required" };                    
                    return PartialView("Quiz/_ContactAuthorResult", attempt);
            }

            return PartialView("Quiz/_QuizIntro", invalidUserQuizToken("Unknown action"));
        }


        public ActionResult LoadQuestionContent(Guid attemptId, int index)
        {
            
            var token = _quizWidgetServices.GetUserQuizQuestionToken(attemptId,index);

            return PartialView("Quiz/_UserQuizQuestionContent", token.Question);
        }

        [HttpPost]
        public ActionResult SaveUserAnswer(Guid? attemptId,int? answerId, int? optionId, bool finish)
        {
            string error;

            if (attemptId == null || answerId == null || optionId == null) return PartialView("Quiz/_UserQuizQuestionContent", new UserQuizQuestionBaseToken { IsValid = false, Message = "Required parameters missing" });

            var answerSaved = _quizWidgetServices.SaveAnswer((int)answerId, (int)optionId,finish, out error);

            if (!answerSaved) return PartialView("Quiz/_UserQuizQuestionContent", new UserQuizQuestionBaseToken{IsValid = false,Message = error});

            if (finish)
            {
                var attempt = _quizWidgetServices.GetAttemptDto((Guid)attemptId);
                attempt.NotifySuccess = true;

                if (!attempt.IsSuccess || !attempt.HasCertificate) return PartialView("Quiz/_UserQuizComplete", attempt);
                
                var cert = _studentSertificateCervices.GetStudentCertificate(attempt.AttemptId, CurrentUserId);

                if (!cert.IsValid || cert.SendOn != null) return PartialView("Quiz/_UserQuizComplete", attempt);

                attempt.CertificateSent = SendStudentCertificate(cert, out error);

                return PartialView("Quiz/_UserQuizComplete",attempt);
            }

            var token = _quizWidgetServices.GetQuizNextQuestionToken((int)answerId);

            return PartialView("Quiz/_UserQuizQuestionContent", token.Question);
        }

        public ActionResult OnCourseCompleted(int id)
        {
            var hasCertificate = _studentSertificateCervices.IsCourseHasCertificateOnCompletion(id);

            if (!hasCertificate) return Json(new JsonResponseToken{success = true,message = "no certificate found"}, JsonRequestBehavior.AllowGet);

            var cert = _studentSertificateCervices.GetStudentCertificate(id,CurrentUserId);

            string error;
            var result = SendStudentCertificate(cert, out error);

            return Json(new JsonResponseToken{success = result,error = error},JsonRequestBehavior.AllowGet);
        }

        private bool SendStudentCertificate(StudentCertificateDTO cert,out string error)
        {
            error = string.Empty;

            if (!cert.IsValid || cert.SendOn != null) return true;

            var body = RenderRazorViewToString("StudentCertificateEmail", cert);

            var result = _studentSertificateCervices.SendStudentCertificate(cert, body, out error);

            return result;
        }

        [HttpPost]
        public ActionResult FinishQuiz(Guid id)
        {
            string error;
            
            var finished = _quizWidgetServices.FinishAttempt(id,QuizEnums.eUserQuizStatuses.UNFINISHED, out error);

            var attempt = finished ? _quizWidgetServices.GetAttemptDto(id) : new StudentQuizAttemptDTO{IsValid = false,Message = error};
            
            attempt.FinishedOnTimeout = true;

            return PartialView("Quiz/_UserQuizComplete", attempt);
        }

        [HttpPost]
        public ActionResult SendMessage(StudentMessageDTO token)
        {
            string error;
            var result = _quizWidgetServices.SendMessageToAuthor(token,CurrentUserId, out error);

            return Json(new JsonResponseToken{success = result,error = error},JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPlayer(long? id)
        {
            if (id == null) return Content("<h3>Id required</h3>");
            var token = _widgetUserServices.GetVideoRenditions((long) id);
            return PartialView("Quiz/_Player", token);
        }
        //
        private void KillAttempt(object sender, EventArgs e,Guid attemptId)
        {
            try
            {
                ((System.Timers.Timer)sender).Stop();
                string error;
                var service = new QuizOfflineServices();
                service.FinishAttempt(attemptId, QuizEnums.eUserQuizStatuses.UNFINISHED, out error);
            }
            catch (Exception ex)
            {
                Logger.Error("Kill Attempt",ex,CommonEnums.LoggerObjectTypes.Quiz);
            }
        }     
    }
}
