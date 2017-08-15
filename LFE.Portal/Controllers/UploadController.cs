using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LFE.Portal.Controllers
{
    [System.Web.Mvc.Authorize]
    public class UploadController : BaseController
    {

        private readonly IS3Wrapper _s3Wrapper;
        private readonly IAuthorAdminServices _authorAdminServices;
        public UploadController()
        {
            _s3Wrapper = DependencyResolver.Current.GetService<IS3Wrapper>();
            _authorAdminServices = DependencyResolver.Current.GetService<IAuthorAdminServices>();
        }

      
        [AllowAnonymous]
        public HttpResponseMessage S3TrancodeCallback()
        {
            string error;
            try
            {
                Logger.Debug("Parse elastic transcoder response begin");

                var req = Request.InputStream;
                req.Seek(0, SeekOrigin.Begin);
                var data = new StreamReader(req, Encoding.UTF8).ReadToEnd();

                Logger.Debug(data);

                _authorAdminServices.SaveS3TranscoderResponse(data, out error);
                
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.Error("Parse elastic transcoder response begin",ex,CommonEnums.LoggerObjectTypes.Video);
                _authorAdminServices.SaveS3TranscoderResponse("", out error);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        [AllowAnonymous]
        public HttpResponseMessage S3TrancodeErrorCallback()
        {
            string error;
            try
            {
                Logger.Debug("Parse elastic transcoder error response");

                var req = Request.InputStream;
                req.Seek(0, SeekOrigin.Begin);
                var data = new StreamReader(req, Encoding.UTF8).ReadToEnd();

                Logger.Debug(data);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.Error("Parse elastic transcoder error response", ex, CommonEnums.LoggerObjectTypes.Video);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        public ActionResult SaveS3TranscodeJob(string id,string status,string err)
        {
            if (status == "OK")
            {
                Logger.Debug("Transcode job for bcId " + id + " start");

                string error;
                var saved = _authorAdminServices.SaveS3TranscodeJob(CurrentUserId, id, out error);

                return Json(new JsonResponseToken { success = saved, error = error }, JsonRequestBehavior.AllowGet);    
            }

            Logger.Warn("Transcode job for bcId " + id + " return error " + err);

            return Json(new JsonResponseToken { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}
