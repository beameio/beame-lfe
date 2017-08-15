using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
   // [Authorize]
    public class UploadController : BaseController
    {
        private readonly IS3Wrapper _s3Wrapper;
        private readonly IAuthorAdminServices _authorServices;

        public UploadController()
        {
            _s3Wrapper      = DependencyResolver.Current.GetService<IS3Wrapper>();
            _authorServices = DependencyResolver.Current.GetService<IAuthorAdminServices>();
        }


        //private IEnumerable<string> GetFileInfo(IEnumerable<HttpPostedFileBase> files)
        //{
        //    return
        //        from a in files
        //        where a != null
        //        select string.Format("{0} ({1} bytes)", Path.GetFileName(a.FileName), a.ContentLength);
        //}

        public ActionResult Remove(HttpPostedFileBase file)
        {
            if (file == null) return ErrorResponse("file not found");

            _s3Wrapper.RemoveFile(file.FileName);
            // Return an empty string to signify success
            return Json(new JsonResponseToken
            {
                success = true    
                ,result = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCourseFile(HttpPostedFileBase file)
        {
            try
            {
                var userId = CurrentUserId;

                if (userId < 0) return ErrorResponse("user not found");

                Logger.Debug("upload course thumb::" + (file != null ? Path.GetFileName(file.FileName) : "file not found"));

                if (file == null) return ErrorResponse("file not found");


                var fileName = Path.GetFileName(file.FileName);

                if (fileName == null) return ErrorResponse("file name missing");

                var size = file.ContentLength;

                string error;

                fileName = FileEnums.eFileOwners.Course + "/" + userId + "/" + ShortGuid.NewGuid() + $".{ImageFormat.Jpeg.ToString().ToLower()}"; //Path.GetExtension(fileName);

                var original = new Bitmap(file.InputStream);

                var msStream = new MemoryStream();

                var image = ImageHelper.LimitBitmapSize(original, Constants.COURSE_THUMB_W, Constants.COURSE_THUMB_H);
                    
                image.Save(msStream, ImageFormat.Jpeg);
                    
                msStream.Seek(0, SeekOrigin.Begin);

                var etag = _s3Wrapper.Upload(fileName, file.ContentType, msStream,out error);

                if (string.IsNullOrEmpty(etag)) return ErrorResponse(error ?? "file name missing");

                int fileId;

                var saved = _authorServices.SaveAuthorS3File(userId, fileName, etag, file.ContentType, size,ImportJobsEnums.eFileInterfaceStatus.Transferred, out fileId, out error);

                msStream.Dispose();

                var url = saved ? _s3Wrapper.GetFileURL(fileName) : string.Empty;

                return Json(
                    new JsonResponseToken
                    {
                        success = saved
                        ,result = new
                        {
                            name = fileName
                            ,url
                        }
                        ,error = error
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ErrorResponse(Utils.FormatError(ex));
            }
        }      

        public ActionResult SaveCourseDoc(HttpPostedFileBase file)
        {
            try
            {
                var userId = CurrentUserId;

                if (userId < 0) return ErrorResponse("user not found");

                Logger.Debug("upload course doc::" + (file != null ? Path.GetFileName(file.FileName) : "file not found"));

                if (file == null) return ErrorResponse("file not found");
                var fileName = Path.GetFileName(file.FileName);

                if (fileName == null) return ErrorResponse("file name missing");

                var size = file.ContentLength;

                string error;

                var originalFileName = Path.GetFileNameWithoutExtension(fileName);

                fileName = fileName.CombineDocumentPath(null, null); //FileEnums.eFileOwners.Course + "/" + DateTime.Now.ToShortDateString() + "/" + fileName;//ShortGuid.NewGuid() + Path.GetExtension(fileName);

                var etag = _s3Wrapper.Upload(fileName, file.ContentType, file.InputStream,out error);

                if (string.IsNullOrEmpty(etag)) return ErrorResponse(error ?? "file name missing");

                int fileId;

                var saved = _authorServices.SaveAuthorS3File(userId, fileName, etag, file.ContentType, size,ImportJobsEnums.eFileInterfaceStatus.Transferred, out fileId, out error);

                var url = saved ? _s3Wrapper.GetFileURL(fileName) : string.Empty;

                return Json(
                    new JsonResponseToken
                    {
                        success = saved
                        ,result = new
                        {
                            name = originalFileName
                            ,url
                        }
                        ,error = error
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ErrorResponse(Utils.FormatError(ex));
            }
        } 

        public ActionResult SaveFiles(IEnumerable<HttpPostedFileBase> Files,int userId)
        {
            try
            {
                var saved = false;
                if (Files != null)
                {
                    foreach (var file in Files)
                    {
                        var fileName = Path.GetFileName(file.FileName);

                        if (fileName == null) continue;

                        var size = file.ContentLength;

                        string error;
                        var etag = _s3Wrapper.Upload(fileName,file.ContentType,file.InputStream,out error);

                        if (string.IsNullOrEmpty(etag)) continue;

                        int fileId;

                        saved = _authorServices.SaveAuthorS3File(userId, fileName, etag, file.ContentType,size,ImportJobsEnums.eFileInterfaceStatus.Waiting, out fileId, out error);
                    }
                }

                return Json(
                           new JsonResponseToken
                           {
                               success = saved
                               ,error = string.Empty
                           }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                
               return Json(new JsonResponseToken
                                {
                                    success = false
                                    ,error = Utils.FormatError(ex)
                                }, JsonRequestBehavior.AllowGet);
            }        
        }

        public ActionResult RemoveVideoFile(HttpPostedFileBase VideoFile,int? fileId)
        {
            if (VideoFile == null && fileId==null) return ErrorResponse("file not found");

            var fileName = VideoFile != null ? VideoFile.FileName : _authorServices.GetInterfacedFileName((int) fileId);

            if (string.IsNullOrEmpty(fileName)) return ErrorResponse("file not found");

            _s3Wrapper.RemoveFile(fileName);
            // Return an empty string to signify success
            return Json(new JsonResponseToken
            {
                success = true                
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveVideoFiles(string[] fileNames)
        {
            // The parameter of the Remove action must be called "fileNames"

            //if (fileNames != null)
            //{
            //    foreach (var fullName in fileNames)
            //    {
                   
            //    }
            //}

            // Return an empty string to signify success
            return Content("");
        }
        
        public ActionResult SaveVideoThumb(HttpPostedFileBase VideoThumb, string id)
        {
            try
            {
                var userId = CurrentUserId;

                if (string.IsNullOrEmpty(id)) return ErrorResponse("videoId required");

                if (userId < 0) return ErrorResponse("user not found");


                if (VideoThumb == null) return ErrorResponse("file not found");


                var fileName = Path.GetFileName(VideoThumb.FileName);

                if (fileName == null) return ErrorResponse("file name missing");

                string error;

                var result = _authorServices.SaveVideoThumb(long.Parse(JsSerializer.DeserializeObject(id).ToString()),fileName,VideoThumb.InputStream,out error);

                #region old
                //var original = new Bitmap(VideoThumb.InputStream);

                //var msStream = new MemoryStream();

                //var image = ImageHelper.LimitBitmapSize(original, 120, 90);

                //image.Save(msStream, ImageFormat.Jpeg);

                //msStream.Seek(0, SeekOrigin.Begin);

                //var savedThumb = _authorServices.SaveVideoImage(new VideoImageDto
                //{
                //    identifier = Int64.Parse(JsSerializer.DeserializeObject(id).ToString())
                //    ,
                //    ImageName = "thumb_" + fileName
                //    ,
                //    _Stream = msStream
                //    ,
                //    Type = FileEnums.ImageType.Thumbnail

                //}, out error);

                //msStream.Dispose();
                //msStream.Flush();

                //var msStream2 = new MemoryStream();

                //image = ImageHelper.LimitBitmapSize(original, 480, 360);

                //image.Save(msStream2, ImageFormat.Jpeg);

                //msStream2.Seek(0, SeekOrigin.Begin);

                //var savedStill = _authorServices.SaveVideoImage(new VideoImageDto
                //{
                //    identifier = Int64.Parse(JsSerializer.DeserializeObject(id).ToString())
                //    ,
                //    ImageName = "still_" + fileName
                //    ,
                //    _Stream = msStream2
                //    ,
                //    Type = FileEnums.ImageType.VideoStill

                //}, out error);

                //msStream2.Dispose();
                //msStream2.Flush(); 
                #endregion
                
                return Json(
                    new JsonResponseToken
                    {
                        success = result//savedThumb && savedStill
                        ,result = new
                        {
                            name = fileName
                        }
                        ,error = error
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ErrorResponse(Utils.FormatError(ex));
            }
        }

        public ActionResult OnUploadBegin(VideoUploadToken token)
        {
            //if (token.uid == null) return Json(new JsonResponseToken{success = false,error = "file uid required"}, JsonRequestBehavior.AllowGet);

            Logger.Debug("Begin upload for " + token.fileName + " with bcId " + token.bcId);

            if (token.bcId == null) return Json(new JsonResponseToken { success = false, error = "file bcId required" }, JsonRequestBehavior.AllowGet);
            
            string error;
            
            int fileId;

            var saved = _authorServices.SaveS3VideoFile(token,CurrentUserId, ImportJobsEnums.eFileInterfaceStatus.InProgress, out fileId, out error);
            
            return Json(new JsonResponseToken{success = saved,error = error},JsonRequestBehavior.AllowGet);
        }

        public ActionResult OnUploadEnd(string vid)
        {
            if (string.IsNullOrEmpty(vid)) return Json(new JsonResponseToken { success = false, error = "refId required" }, JsonRequestBehavior.AllowGet);

            string error;
            int fileId;
            long bcId;

            if (!long.TryParse(vid, out bcId)) return Json(new JsonResponseToken { success = false, error = "invalid videoId parameter", result = -1 }, JsonRequestBehavior.AllowGet);

            Logger.Debug("End upload for  bcId " + bcId);

            var saved = _authorServices.UpdateS3InterfaceRecord(bcId, ImportJobsEnums.eFileInterfaceStatus.UploadedToS3,out fileId,out error);

            if (saved)
            {
                saved = _authorServices.SaveUserVideoFromInterface(fileId, CurrentUserId, out error);
            }

            if (saved)
            {
                Logger.Debug("Trancoder will call for  bcId " + bcId);
            }

            return Json(new JsonResponseToken { success = saved, error = error,result = saved ? bcId : -1}, JsonRequestBehavior.AllowGet);
        }
        //account settings
        public ActionResult RemoveProfilePicture(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return ErrorResponse("file not found");

            _s3Wrapper.RemoveFile(fileName);
            // Return an empty string to signify success
            return Json(new JsonResponseToken
            {
                success = true    
                ,result = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveProfilePicture(HttpPostedFileBase ProfilePicture, int userId)
        {
            try
            {
                if (userId < 0) return ErrorResponse("user not found");

                if (ProfilePicture == null) return ErrorResponse("file not found");

                var fileName = Path.GetFileName(ProfilePicture.FileName);

                if (fileName == null) return ErrorResponse("file name missing");

                var size = ProfilePicture.ContentLength;

                string error;

                fileName = FileEnums.eFileOwners.Author + "/" + userId + "/Profile/" + ShortGuid.NewGuid() + Path.GetExtension(fileName);

                var etag = _s3Wrapper.Upload(fileName, ProfilePicture.ContentType, ProfilePicture.InputStream,out error);

                if (string.IsNullOrEmpty(etag)) return ErrorResponse(error ?? "file name missing");

                int fileId;

                var saved = _authorServices.SaveAuthorS3File(userId, fileName, etag, ProfilePicture.ContentType, size,ImportJobsEnums.eFileInterfaceStatus.Transferred, out fileId, out error);

                var url = saved ? _s3Wrapper.GetFileURL(fileName) : string.Empty;

                return Json(
                    new JsonResponseToken
                    {
                        success = saved
                        ,result = new
                        {
                            name = fileName
                            ,url
                        }
                        ,error = error
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ErrorResponse(Utils.FormatError(ex));
            }
        }      

       
        //quizzes
        public ActionResult SaveQuizQuestImage(HttpPostedFileBase file, int? sid)
        {
            try
            {
                if (sid == null) return ErrorResponse("Quiz SID required");
                
                if (file == null) return ErrorResponse("file not found");

                var fileName = Path.GetFileName(file.FileName);

                if (fileName == null) return ErrorResponse("file name missing");

                var size = file.ContentLength;

                string error;

                var path = Path.GetFileNameWithoutExtension(fileName).CombineQuizQuestionImagePath((int)sid);

                fileName = Path.GetFileName(path);

                var original = new Bitmap(file.InputStream);

                var msStream = new MemoryStream();

                var image = ImageHelper.LimitBitmapSize(original, Constants.QZ_Q_THUMB_W, Constants.QZ_Q_THUMB_H);

                image.Save(msStream, ImageFormat.Jpeg);

                msStream.Seek(0, SeekOrigin.Begin);

                var etag = _s3Wrapper.Upload(path, file.ContentType, msStream, out error);

                if (string.IsNullOrEmpty(etag)) return ErrorResponse(error ?? "file name missing");

                int fileId;

                var saved = _authorServices.SaveAuthorS3File(CurrentUserId, path, etag, file.ContentType, size, ImportJobsEnums.eFileInterfaceStatus.Transferred, out fileId, out error);

                msStream.Dispose();

                var url = saved ? _s3Wrapper.GetFileURL(path) : string.Empty;

                return Json(
                    new JsonResponseToken
                    {
                        success = saved
                        ,result = new
                        {
                            path
                            ,url
                            ,name = fileName
                        }
                        ,error = error
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ErrorResponse(Utils.FormatError(ex));
            }
        }      

        //certificate
        public ActionResult SaveCertificateSignature(HttpPostedFileBase SignFile, int? id)
        {
            try
            {
                if (id == null) return ErrorResponse("CourseId required");
                
                if (SignFile == null) return ErrorResponse("file not found");

                var fileName = Path.GetFileName(SignFile.FileName);

                if (fileName == null) return ErrorResponse("file name missing");

                var size = SignFile.ContentLength;

                string error;

                var path = ShortGuid.NewGuid().ToString().CombineCertImagePath((int)id); //Path.GetFileNameWithoutExtension(fileName).CombineCertImagePath((int)id);

                fileName = Path.GetFileName(path);

                var original = new Bitmap(SignFile.InputStream);

                var msStream = new MemoryStream();

                int width;
                int height;


                if (!int.TryParse(Utils.GetKeyValue("certW"), out width))
                {
                    width = Constants.CERT_SIGN_W;
                }
                if (!int.TryParse(Utils.GetKeyValue("certH"), out height))
                {
                    height = Constants.CERT_SIGN_H;
                }                

                var image = ImageHelper.LimitBitmapSize(original, width, height);

                image.Save(msStream, ImageFormat.Png);

                msStream.Seek(0, SeekOrigin.Begin);

                var etag = _s3Wrapper.Upload(path, SignFile.ContentType, msStream, out error);

                if (string.IsNullOrEmpty(etag)) return ErrorResponse(error ?? "file name missing");

                int fileId;

                var saved = _authorServices.SaveAuthorS3File(CurrentUserId, path, etag, SignFile.ContentType, size, ImportJobsEnums.eFileInterfaceStatus.Transferred, out fileId, out error);

                msStream.Dispose();

                var url = saved ? _s3Wrapper.GetFileURL(path) : string.Empty;

                return Json(
                    new JsonResponseToken
                    {
                        success = saved
                        ,result = new
                        {
                            path
                            ,url
                            ,name = fileName
                        }
                        ,error = error
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return ErrorResponse(Utils.FormatError(ex));
            }
        }


        [AllowAnonymous]
        public HttpResponseMessage S3UploadCallback()
        {

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
