using Antlr.Runtime.Misc;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;
using System.Web.Mvc;


namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    public class MailchimpController : BaseController
    {
        private readonly IMailchimpServices _mailchimpServices;

        public MailchimpController()
        {
            _mailchimpServices = DependencyResolver.Current.GetService<IMailchimpServices>();
        }

        public ActionResult Index()
        {
            var submitToken = TempData["token"] as SubmitToken;
            ViewBag.viewSuccess = false;
            ViewBag.viewNotification = false;

            var token = _mailchimpServices.GetUserListDto(CurrentUserId);
            
            if (token == null)
                return View("Login", new ChimpUserListDTO { IsValid = true });

            if (submitToken != null)
            {
                ViewBag.viewSuccess = submitToken.IsValid;
                token.Message = !submitToken.IsValid ? submitToken.Message : token.Message;
                if (!submitToken.IsValid)
                    return View("Index", token);
            }

            token.IsValid = _mailchimpServices.GetListSegments(token);
            if (!token.IsValid)
            {
                token.Message = "segments not found";
                ViewBag.viewSuccess = false;
                return View("Index", token);
            }

            var missingSegments = _mailchimpServices.GetMissingSegments(CurrentUserId, token.ListId);
            if (missingSegments.Count > 0)
            {
                ViewBag.viewNotification = true;
                ViewBag.missingSegments = missingSegments;
            }

            return View("Index", token);
        }

        [HttpPost]
        public ActionResult Submit(ChimpUserListDTO token)
        {
            if (!ModelState.IsValid)
                return View("Login", token);

            string error;
            token.UserId = CurrentUserId;

            token.IsValid = _mailchimpServices.SaveUserList(token, out error);
            if (!token.IsValid)
            {
                token.Message = string.Format("Credentials error: {0}", error);
                return View("Login", token);
            }

            token.IsValid = _mailchimpServices.SaveListSegment(token.ListId, out error);
            if (!token.IsValid)
                return RedirectToIndex(new SubmitToken { IsValid = false, Message = string.Format("Segments error: {0}", error) });

            token.IsValid = _mailchimpServices.SaveListSubscribers(token.ListId, out error);
            if (!token.IsValid)
                return RedirectToIndex(new SubmitToken { IsValid = false, Message = string.Format("Subscribers error: {0}", error) });

            SaveUserEvent(CommonEnums.eUserEvents.MAILCHIMP_CONNECTED);
            return RedirectToIndex(new SubmitToken { IsValid = true});
        }

        
        [HttpPost]
        public ActionResult RefreshSegments(ChimpUserListDTO token)
        {
            string error;

            token.IsValid = _mailchimpServices.SaveListSegment(token.ListId, out error);
            if (!token.IsValid)
                return RedirectToIndex(new SubmitToken { IsValid = false, Message = string.Format("Segments error: {0}", error) });

            token.IsValid = _mailchimpServices.SaveListSubscribers(token.ListId, out error);
            if (!token.IsValid)
                return RedirectToIndex(new SubmitToken { IsValid = false, Message = string.Format("Subscribers error: {0}", error) });

            SaveUserEvent(CommonEnums.eUserEvents.MAILCHIMP_REFRESHED);
            return RedirectToIndex(new SubmitToken { IsValid = true });
        }

        private ActionResult RedirectToIndex(SubmitToken token)
        {
            TempData["token"] = token;
            return RedirectToAction("Index");
        }


        class SubmitToken
        {
            public bool IsValid { get; set; }
            public string Message { get; set; }
        }

    }
}
