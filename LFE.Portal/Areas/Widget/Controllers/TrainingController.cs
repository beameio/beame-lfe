using System.Linq;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using System.Web.Mvc;
using LFE.DataTokens;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class TrainingController : BaseController
    {

        private readonly ITrainingWidgetServices _trainingServices;

        public TrainingController()
        {
            _trainingServices = DependencyResolver.Current.GetService<ITrainingWidgetServices>();
        }

        public ActionResult LoadTrainingBoxes(int id, BillingEnums.ePurchaseItemTypes type, bool isVeiwer = false)
        {
            var token = _trainingServices.GetTrainingBoxesDto(id, type);
            token.IsInViewer = isVeiwer;
            return PartialView("Training/_LiveSessionBoxes", token);
        }

        public ActionResult LoadUserTrainingForm(int id, BillingEnums.ePurchaseItemTypes type)
        {
            var registrations = _trainingServices.GetItemTrainingUserRegistrtions(CurrentUserId, id, type);

            if (registrations.Count == 0 || registrations.All(x => x.RegistrantStatus == TrainingEnums.eTrainingStatus.CANCEL))
            {
                var token = _trainingServices.GetTrainingUserRegisterDto(id, type);
                token.Item = new BaseItemToken{ ItemId = id, ItemType = type};
                return PartialView("Training/_UserRegister", token);    
            }

            var registration = registrations.Where(x => x.RegistrantStatus == TrainingEnums.eTrainingStatus.REGISTERED).OrderBy(x=>x.Start).First();

            return PartialView("Training/_UserCountdown", registration);
        }

        [Authorize]
        public ActionResult RegisterUser(int? id, TrainingUserNotificationSettingsDTO notifySettings)
        {
            string error;

            if (id == null) return PartialView("Training/_Error",new BaseModelState{Message = "trainingId required"});

            int registrantId;

            var registered = _trainingServices.RegisterUserToTraining(CurrentUserId, (int)id,notifySettings,out registrantId, out error);

            if (!registered) return PartialView("Training/_Error", new BaseModelState { Message = error });

            var token = _trainingServices.GetTrainingRegistrantDto(registrantId);

            return token.IsValid ? PartialView("Training/_UserThankYou",token) : PartialView("Training/_Error", new BaseModelState { Message = token.Message});
        }
        [Authorize]
        public ActionResult LoadNotificationSettingsForm(int? id)
        {
            if (id == null) return ErrorResponse("RegistrationId required");

            var token = _trainingServices.GetTrainingRegistrantDto((int)id);

            return token.IsValid ? PartialView("Training/_EditNotificationSettings", token) : PartialView("Training/_Error", new BaseModelState { Message = token.Message });
        }

        [Authorize]
        public ActionResult UpdateNotificationSettings(int? id, TrainingUserNotificationSettingsDTO notifySettings)
        {
            string error;

            if (id == null) return ErrorResponse("RegistrationId required");

            var updated = _trainingServices.UpdateNotificationSettings((int)id, notifySettings, out error);

            return Json(new JsonResponseToken { success = updated, error = error }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult CancelRegistration(int? id)
        {
            string error;

            if (id == null) return ErrorResponse("RegistrationId required");

            var cancelled = _trainingServices.CancelTrainingRegistration((int)id, out error);

            return Json(new JsonResponseToken{success = cancelled,error = error},JsonRequestBehavior.AllowGet);
        }
    }
}
