using System;
using System.Web.Http;
using System.Web.Script.Serialization;
using LFE.DataTokens;
using LFE.Infrastructure.NLogger;
using LFE.Portal.Interfaces;

namespace LFE.Portal.Controllers
{
    /// <summary>
    /// Base Api controller for third part usage, optional required filter for app_id route parameter as GUID
    /// </summary>    
    
    public class ApiBaseController : ApiController
    {

        public NLogLogger Logger { get; private set; }
        public static JavaScriptSerializer JsSerializer { get; private set; }      

        public ApiBaseController()
        {
            Logger       = new NLogLogger();
            JsSerializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        }

        public JsonResponseToken ErrorResponse(string error)
        {
            return new JsonResponseToken
            {
                success = false
                ,error = error
            };
        }
    }
}
