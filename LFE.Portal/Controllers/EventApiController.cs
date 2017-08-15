using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using System;
using System.Web.Mvc;


namespace LFE.Portal.Controllers
{
    public class EventApiController : BaseController
    {
        public IUserEventLoggerServices _eventLoggerServices { get; private set; }
        public IWidgetUserServices _widgetUserServices { get; private set; }
        public EventApiController()
        {
            _eventLoggerServices =  DependencyResolver.Current.GetService<IUserEventLoggerServices>();
            _widgetUserServices  = DependencyResolver.Current.GetService<IWidgetUserServices>();
        }

        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet, System.Web.Http.ActionName("save")]
        public ActionResult SaveEvent(int? eventId, string additional = null, string trackingID = null, int? courseId = null, int? bundleId = null, long? bcId = null)
        {
            if (eventId == null)
            {
                return ErrorResponse("eventId required");
            }

            try
            {
                var userEvent = Utils.ParseEnum<CommonEnums.eUserEvents>(eventId.ToString());

                var result = _eventLoggerServices.Report(new ReportToken
                {
                    UserId = CurrentUserId,
                    EventType = userEvent,
                    NetSessionId = Session.SessionID,
                    AdditionalMiscData = additional,
                    TrackingID = trackingID,
                    CourseId = courseId,
                    BundleId = bundleId,
                    BcId = bcId,
                    HostName = GetReferrer()
                });

                return Json(new JsonResponseToken
                {
                    success = result
                },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ErrorResponse(Utils.FormatError(ex));
            }
        }

        public JsonResult SaveVideoStats(VideoStatsToken token)
        {
            string error;
            Guid sessionId;

            if (CurrentUserId < 0) return Json(new JsonResponseToken{result = false,error = "Unauthorized access"},JsonRequestBehavior.AllowGet);

            var result = _widgetUserServices.SaveUserVideoStats(CurrentUserId,token, out sessionId, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
                ,result =new {sessionId = result ? sessionId.ToString() : null,action=token.action.ToString()}
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateVideoStats(VideoStatsReasonToken token)
        {
            string error;
            
            if (CurrentUserId < 0) return Json(new JsonResponseToken{result = false,error = "Unauthorized access"},JsonRequestBehavior.AllowGet);

            var result = _widgetUserServices.UpdateVideoStatsReason(token, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error                
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
